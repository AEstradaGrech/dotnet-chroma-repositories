
using Dotnet.Chroma.Repositories.Extensions;
using Dotnet.Chroma.Repositories.Models;
using Dotnet.Chroma.Repositories.Models.Client.Response;
using Dotnet.Chroma.Repositories.Models.Enums;
using Dotnet.Chroma.Repositories.Models.Exceptions;
using Dotnet.Chroma.Repositories.Models.Interfaces;
using Dotnet.Chroma.Repositories.Models.Metadata;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Chroma;
using System.Net;
using System.Text.Json;

namespace Dotnet.Chroma.Repositories
{
#pragma warning disable SKEXP0020 // SK warning >> Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    /// <summary>
    /// IMPORTANT:
    ///  - For this package: Microsoft.SemanticKernel.Connectors.Chroma --version 1.74.0-alpha
    ///  - USE THIS exact Chroma image VERSION: chromadb/chroma:0.4.24
    ///  - Run the container: docker run --name local-chroma -v ./chroma-data:/data -p XXXX:8000 chromadb/chroma:0.4.24
    ///  
    /// Base ABSTRACT Repository. Defines the types and performs the chroma db http requests (privately. inherited members cannot talk to chroma).
    /// Handles the access to chroma and shapes the recieved data according to the defined types / models.
    /// 
    /// The sdk works with to types of model:
    /// - COLLECTIONS: A collection is a data structure to hold summary data that is common to or necessary for the whole collection chunks.
    ///         > A collection is ALWAYS the first chunk that is created in a collection (so ID = 0 is always the Collection Chunk)
    ///         > A collection has no text embedding, only a description.
    ///         > A collection always stores:
    ///             · MODEL + DIMENSIONS: a similarity search requires the query text to be embedded with the same model and dimensions than the queried texts. 
    ///                                   This ensures that al the stored texts in a collection use the same params and they are retrieved to embed the user query during a search request.
    ///             · TOTAL CHUNKS: the total generated chunks for this collection. Useful for pagination.
    ///             · TYPE & CHUNK TYPE: Type is a common property to both collections and nodes and it is used mostly as a metadata filter to query collections and chunks separately.
    ///                                  For a collection should be ALWAYS collection (assigned by default) and should not be overriden (if you do that, use different enums for collections and chunks)
    ///                                  Type differentiates the Collection Chunk (ID = 0) from the rest, but in order to differentiate a collection type from other, you must use 
    ///                                  the CHUNK TYPE metadata field, that stores the assigned Chunk type (for example: Type = Collection - Chunk Type = FILES | CHATS | OTHER
    ///                                  
    /// - CHUNKS: Chunks are the pieces of data itselfs. This are the text you want to query later, with the shape / metadata you need to work with them.
    ///         > A chunk has an ID that is assigned on creation based on the total count retrieved from the Collection.
    ///         > A chunk may have or may have nor an embedding nad be queried by it's metadata and or ID (then Chroma becomes a text database)
    ///         > A chunk is a data structure that can be considered as a Table Item in which you define the table fields using the metadata tags and a class to serialize them and work easier with the values.
    ///         > All chunks have a DefaultMetadata class that contains a typed form of it's attached metadata. You can map values from this Metadata class to your extened Chunk class to work better with it or 
    ///           you can use the 'chunk.GetMeta<T>()' generic method to access the metadata directly. The important thing is: CHUNK = CHUNK.cs + METADATA.cs;
    ///         
    /// 
    /// ### METADATA: Chroma allows to add metadata to a chunk that can be used to filter the data in a similarity search. These are Key-Value pairs of any type.
    /// 
    ///     Depending on the type of data you are working with, you may need to add different types of metadata so, in order to handle that, a CHROMA METADATA class is being
    /// used to deserialize the attached metadata into a specific class that the chunk stores / reads during construction to accquire it's final shape.
    /// Then use the chunk model methods to 'AddMetadata' to add new values or override the existing ones.
    /// 
    /// Note: In order to enforce the Collection / Chunk model distinction, Collections have their own Metadata class and Chunks their own class to. Extend 'CollectionMetadata' & 'ChunkMetadata' accordingly in inherited classes
    /// 
    /// So, the final structures schema is:
    ///     - Collection
    ///         - 1 CollectionMetadata
    ///         - N Chunks (containing)
    ///             - 1 ChunkMetadata 
    /// </summary>
    /// <typeparam name="TCol">The ChromaCollection type that groups the data</typeparam>
    /// <typeparam name="TChunk">The ChromaChunk model for the repository</typeparam>
    public abstract class  ChromaRepository<TCol, TChunk> : IChromaRepository<TCol, TChunk> where TCol : ChromaChunksCollection<TChunk> where TChunk : ChromaChunk
    {
        private readonly IChromaClient _dbClient;
        protected readonly ChromaSettings _settings;
        
        public ChromaRepository(IChromaClient client, IOptions<ChromaSettings> settings)
        {
            _dbClient = client ?? throw new ArgumentNullException(nameof(client));
            _settings = settings.Value ?? new ChromaSettings();
        }

        /// <summary>
        /// Gets a string list with the name of all DB collections
        /// </summary>
        /// <returns></returns>
        public async Task<IAsyncEnumerable<string>> GetDbCollections()
            => _dbClient.ListCollectionsAsync();

        /// <summary>
        /// Gets a Collection of type TCol by it's name
        /// Gets the Collection Chunk (ID=0) and 'transforms' it to TCol using the constructors system
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<TCol> GetCollection(string name)
        {
            var collection = await requestCollection(name);

            if (collection != null)
            {
                var data = await GetChunkById(name, "0");

                if (data == null)
                    throw new ArgumentNullException($"No data has been found for collection: {name}");

                return Activator.CreateInstance(typeof(TCol), collection.Id, collection.Name, data.Text, data.Metadata) as TCol;
            }
            return null;
        }

        /// <summary>
        /// Checks if a collection exists in Chroma
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<bool> CollectionExists(string name)
        {
            await foreach (string collection in requestCollectionList())
                if (collection == name)
                    return true;

            return false;
        }

        /// <summary>
        /// Gets all the collections of type TCol filtering by the CHUNK_TYPE value.
        /// This should be the value of your custom ChunkType enum casted to int
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<List<TCol>> CollectionsOf(int type)
        {
            var results = new List<TCol>();
            await foreach(var name in _dbClient.ListCollectionsAsync())
            {
                var collection = await GetCollection(name);

                if (collection.GetMeta<ChromaCollectionMetadata>().CHUNK_TYPE == type)
                    results.Add(collection);
            }
            
            return results;
        }

        /// <summary>
        /// Creates a collection according to the sdk rules (chunk 0, no embedding, default metadata attached)
        /// Allows to create a collection using specific model & dimensions or the default settings
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="model"></param>
        /// <param name="dimensions"></param>
        /// <returns></returns>
        public async Task<TCol> CreateCollection(string name, string description, string? model = null, int? dimensions = null, int chunkType = (int)EChunkType.DOCUMENT)
        {
            if (string.IsNullOrEmpty(model))
                model = _settings.EmbeddingModel;

            var metadata = getDefaultCollectionMetadata(description, model, dimensions);

            var dataChunk = new ChromaChunk("0", description, new ReadOnlyMemory<float>([]), metadata);
            
            dataChunk.UpdateType((int)EChunkType.COLLECTION);
            dataChunk.AddMetadata(nameof(ChromaCollectionMetadata.CHUNK_TYPE).ToLower(), chunkType);

            return await CreateCollection(name, dataChunk);
        }

        /// <summary>
        /// Creates a collection from a pre-processed chunk that will be used to create the Collection Chunk / Chunk 0
        /// It also ensures that the sdk conditions for a new collection are met
        /// </summary>
        /// <param name="name"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<TCol> CreateCollection(string name, ChromaChunk data)
        {
            if (await CollectionExists(name))
                throw new InvalidDataException($"An error has occured while creating the chroma collection: a collection with name {name} already exists");

            await requestNewCollection(name);

            var collection = await requestCollection(name);

            if (collection == null)
                throw new ArgumentNullException($"An error has occured while creating the chroma collection: {name}");

            data.UpdateType((int)EChunkType.COLLECTION);
            
            var defaultMetadata = getDefaultCollectionMetadata(name, data.DefaultMetadata.MODEL ?? null, data.DefaultMetadata.DIMENSIONS);

            //if chunk has already some of the default values, preserve them. Do not reserialize the DefaultMeta to validate next step (TEXT)
            defaultMetadata.Keys.ToList().ForEach(key => data.AddMetadata(key, defaultMetadata[key], resetDefault: false, isOverride: false));

            data.Id = "0";
            data.Embedding = null;
            await requestEmbeddingsUpsert(collection.Id, [data.Id], [data.Text], null, [data.Metadata], isCreate: true);

            return await GetCollection(name);
        }

        /// <summary>
        /// Performs a similarity search against a specified collection, with optional filtering metadata
        /// </summary>
        /// <param name="name"></param>
        /// <param name="queryEmbedding"></param>
        /// <param name="resultsNumber"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public virtual async Task<List<ChromaQueryChunk>> QueryCollection(string name, ReadOnlyMemory<float> queryEmbedding, int resultsNumber, Dictionary<string, object> filters = null)
        {
            var collection = await GetCollection(name);

            var queryResult = await requestQuery(collection.Id, queryEmbedding, resultsNumber, filters);

            var results = new List<ChromaQueryChunk>();

            if (queryResult.Ids.Count >= 1 && queryResult.Distances.Count >= 1 && queryResult.Metadatas.Count >= 1 && queryResult.Documents.Count >= 1 && queryResult.Embeddings.Count >= 1)
            {
                var resultIds = queryResult.Ids.First();
                var resultDistances = queryResult.Distances.First();
                var resultMetas = queryResult.Metadatas.First();
                var resultDocuments = queryResult.Documents.First();
                var resultEmbeddings = queryResult.Embeddings.First();

                if (resultIds.Count > resultsNumber || resultIds.Count > resultsNumber || resultMetas.Count > resultsNumber)
                    throw new InvalidDataException($"Invalid number of results");

                for (int i = 0; i < resultIds.Count; i++)
                    results.Add(Activator.CreateInstance(typeof(ChromaQueryChunk), resultIds[i], resultDistances[i], resultDocuments[i], resultEmbeddings[i] ?? new ReadOnlyMemory<float>([]), resultMetas[i] ?? new Dictionary<string, object>()) as ChromaQueryChunk);
            }

            return results;
        }

        /// <summary>
        /// Inserts or Updates a chunk of the specified collection, allowing to add some extra metadata tags to the chunk
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="chunk"></param>
        /// <param name="extraTags"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<TChunk> UpsertChunk(string collectionName, ChromaChunk chunk, Dictionary<string, object> extraTags = null)
        {
            if (!await CollectionExists(collectionName))
                throw new InvalidOperationException($"No Chroma Collection has been found with name: {collectionName}");

            var collection = await GetCollection(collectionName);

            if (!string.IsNullOrEmpty(chunk.Id))
            {
                if (extraTags != null && extraTags.Count() > 0)
                    extraTags.Keys.ToList().ForEach(key => chunk.AddMetadata(key, extraTags[key], resetDefault: true));

                await requestEmbeddingsUpsert(collection.Id, [chunk.Id], [chunk.Text], [chunk.Embedding], [chunk.Metadata]);

                return await GetChunkById(collectionName, chunk.Id);
            }

            else return await InsertChunk(collectionName, chunk, extraTags);
        }

        /// <summary>
        /// Inserts a new collection node along the data that can be included in a 
        /// chroma request ("documents", "embeddings", "metadatas")
        /// 
        /// Generates the ID from the collection total chunks count and updates the count.
        /// Returns the created chunk
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="chunk"></param>
        /// <param name="extraMetas"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<TChunk> InsertChunk(string collectionName, ChromaChunk chunk, Dictionary<string, object> extraMetas = null)
        {
            if (!await CollectionExists(collectionName))
                throw new InvalidOperationException($"No Chroma Collection has been found with name: {collectionName}");

            var collection = await GetCollection(collectionName);

            if (string.IsNullOrEmpty(chunk.Text))
                throw new InvalidOperationException("Chroma chunk has no associated document");

            if (extraMetas != null && extraMetas.Count() > 0)
                extraMetas.Keys.ToList().ForEach(key => chunk.AddMetadata(key, extraMetas[key]));

            chunk.Id = $"{collection.GetMeta<ChromaCollectionMetadata>().TOTAL_CHUNKS + 1}";
            chunk.AddMetadata(nameof(ChromaMetadata.TYPE).ToLower(), collection.GetMeta<ChromaCollectionMetadata>().CHUNK_TYPE);

            await requestEmbeddingsUpsert(collection.Id, [chunk.Id], [chunk.Text], [chunk.Embedding], [chunk.Metadata], isCreate: true);

            await updateCollectionChunksCount(collectionName, bIncrease: true);

            return await GetChunkById(collection.Name, chunk.Id);
        }
        
        /// <summary>
        /// Batch chunk insert
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="chunks"></param>
        /// <param name="extraTags"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<int> InsertChunks(string collectionName, List<ChromaChunk> chunks, Dictionary<string, object> extraTags = null)
        {
            if (!await CollectionExists(collectionName))
                throw new InvalidOperationException($"No Chroma Collection has been found with name: {collectionName}");

            var collection = await GetCollection(collectionName);

            var selectedChunks = chunks.Where(x => !x.Embedding.IsEmpty);

            if (selectedChunks.Count() == 0)
                throw new InvalidOperationException($"Invalid batch insert for collection: {collectionName} >> No embeddings to insert");

            var totalChunks = collection.GetMeta<ChromaCollectionMetadata>().TOTAL_CHUNKS;

            for (int i = 0; i < selectedChunks.Count(); i++)
            {
                var chunk = chunks[i];

                chunk.Id = $"{totalChunks + (i + 1)}";

                if (string.IsNullOrEmpty(chunk.Text))
                    throw new InvalidOperationException("Chroma chunk has no associated document");

                if (extraTags != null && extraTags.Count() > 0)
                    extraTags.Keys.ToList().ForEach(key => chunk.AddMetadata(key, extraTags[key]));

                chunk.AddMetadata(nameof(ChromaMetadata.TYPE).ToLower(), collection.GetMeta<ChromaCollectionMetadata>().CHUNK_TYPE);
            }

            await requestEmbeddingsUpsert(collection.Id, selectedChunks.Select(x => $"{x.Id}").ToList(), selectedChunks.Select(x => x.Text).ToList(), selectedChunks.Select(x => x.Embedding).ToList(), selectedChunks.Select(x => x.Metadata).ToList(), isCreate: true);

            await updateCollectionChunksCount(collectionName, bIncrease: true, amount: selectedChunks.Count());

            return selectedChunks.Count();
        }

        /// <summary>
        /// Retrieves collection chunks by ID to be inspected. Useful for data pre-processing tasks and to check the results of a new collection insert.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="chunkIds"></param>
        /// <param name="includeEmbeddings"></param>
        /// <returns></returns>
        public async Task<TCol> InspectCollection(string name, List<string> chunkIds, bool includeEmbeddings = false)
        {
            var collection = await GetCollection(name);

            return Activator.CreateInstance(typeof(TCol), collection.Id, collection.Name, collection.Description, collection.Metadata, await GetChunks(name, chunkIds, includeEmbeddings)) as TCol;
        }

        /// <summary>
        /// Gets a list of chunks of the specified collection, with or without the generated embedding
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="chunkIds"></param>
        /// <param name="withEmbeddings"></param>
        /// <returns></returns>
        public async Task<List<TChunk>> GetChunks(string collectionName, List<string> chunkIds, bool withEmbeddings = true)
        {
            var collection = await GetCollection(collectionName);

            return mapChunks(await requestDocuments(collection.Id, chunkIds, withEmbeddings), withEmbeddings);
        }

        /// <summary>
        /// Gets a page of chunk of the specified collection filtering by its metadata / table fields and returning them with or withoud the attached embedding
        /// </summary>
        /// <param name="name"></param>
        /// <param name="filters"></param>
        /// <param name="withEmbeddings"></param>
        /// <param name="pageSize"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<TCol> GetCollectionPage(string name, Dictionary<string, object>? filters = null, bool withEmbeddings = true, int pageSize = 10, int page = 0)
        {
            var collection = await GetCollection(name);

            if (pageSize <= 0)
                pageSize = 1;
            
            if (page < 0)
                page = 0;

            if (page * pageSize > collection.GetMeta<ChromaCollectionMetadata>().TOTAL_CHUNKS)
                throw new InvalidOperationException($"{name} >> {nameof(GetCollectionPage)} >> Requested page index out of range");

            var chunks = await GetChunks(name, filters, withEmbeddings, pageSize, page * pageSize);

            collection.Chunks = chunks.Where(chunk => chunk.Id != "0").ToList();

            return collection;
        }

        /// <summary>
        /// Gets a list of ALL | PAGESIZE chunks, filtering by their metadata / table fields. With / without attached embedding 
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="filters"></param>
        /// <param name="withEmbeddings"></param>
        /// <param name="pageSize"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        public async Task<List<TChunk>> GetChunks(string collectionName, Dictionary<string, object>? filters = null, bool withEmbeddings = true, int ? pageSize = null, int? skip = null)
        {
            var collection = await GetCollection(collectionName);

            return mapChunks(await requestDocuments(collection.Id, withEmbeddings, filters, pageSize, skip), withEmbeddings);
        }

        /// <summary>
        /// Creates an empty chunk (NO ID YET) for a collection, adding the required metadata from the collection chunk
        /// </summary>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public async Task<TChunk> DefaultChunk(string collectionName)
        {
            var collection = await GetCollection(collectionName);

            if (string.IsNullOrEmpty(collection.DefaultMetadata.MODEL))
                throw new InvalidDataException($"{nameof(DefaultChunk)} >> {collectionName} >> Invalid collection settings >> Embedding MODEL name empty");

            if (collection.DefaultMetadata.DIMENSIONS <= 0)
                throw new InvalidDataException($"{nameof(DefaultChunk)} >> {collectionName} >> Invalid collection settings >> Embedding DIMENSIONS value ({collection.DefaultMetadata.DIMENSIONS})");

            var chunk = DefaultChunk(collection.DefaultMetadata.MODEL, collection.DefaultMetadata.DIMENSIONS);

            chunk.AddMetadata(nameof(ChromaMetadata.DOCUMENT_NAME).ToLower(), collection.DefaultMetadata.DOCUMENT_NAME);
            chunk.AddMetadata(nameof(ChromaMetadata.TYPE).ToLower(), collection.GetMeta<ChromaCollectionMetadata>().CHUNK_TYPE);

            return chunk;
        }
        /// <summary>
        /// Creates an empty chunk (NO ID YET) for a specific embedding model and dimensions (not related to any collection yet)
        /// </summary>
        /// <param name="embeddingModel"></param>
        /// <param name="dimensions"></param>
        /// <returns></returns>
        public TChunk DefaultChunk(string embeddingModel, int dimensions)
        {
            if (string.IsNullOrEmpty(embeddingModel))
                embeddingModel = _settings.EmbeddingModel;

            if (dimensions <= 0)
                dimensions = _settings.EmbeddingDimensions;

            var metadata = new Dictionary<string, object>();

            metadata.Add(nameof(ChromaMetadata.MODEL).ToLower(), embeddingModel);

            metadata.Add(nameof(ChromaMetadata.DIMENSIONS).ToLower(), dimensions);

            return DefaultChunk(metadata);
        }

        /// <summary>
        /// Creates a completely empy chunk or an empty chunk with some predefined metadata. Useful generate chunks from chunks
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>

        public TChunk DefaultChunk(Dictionary<string, object>? metadata = null)
        {
            var chunk = Activator.CreateInstance(typeof(TChunk)) as TChunk;

            if (metadata != null)
                chunk.CloneMetadata(metadata);

            return chunk;
        }
       
        public async Task<TChunk> GetChunkById(string collectionName, string id)
        {
            if (!await CollectionExists(collectionName))
                throw new InvalidOperationException($"No Chroma Collection has been found with name: {collectionName}");

            var collection = await requestCollection(collectionName);

            var chunks = await requestDocuments(collection.Id, [id], withEmbeddings: true);

            return mapChunks(chunks, withEmbeddings: true).FirstOrDefault();
        }
        public async Task<bool> DeleteCollection(string collection)
        {
            if (await CollectionExists(collection))
            {
                await requestCollectionDelete(collection);

                return !await CollectionExists(collection);
            }

            return false;
        }

        public async Task<bool> DeleteChunk(string collectionName, string id)
        {
            if (!await CollectionExists(collectionName))
                throw new InvalidOperationException($"No Chroma Collection has been found with name: {collectionName}");

            var collection = await requestCollection(collectionName);

            await requestDelete(collection.Id, [id]);

            var chunk = await GetChunkById(collection.Name, id);

            bool bSucceeded = chunk == null;

            if (bSucceeded)
                await updateCollectionChunksCount(collection.Name, bIncrease: false);

            return bSucceeded;
        }

        /// <summary>
        /// Add or overrides metadata from the specified collection
        /// </summary>
        /// <param name="name"></param>
        /// <param name="metadata"></param>
        /// <param name="isOverride"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<TCol> UpdateCollectionData(string name, Dictionary<string, object> metadata, bool isOverride = true)
        {
            if (!await CollectionExists(name))
                throw new ArgumentNullException($"No collection found with name: {name}");

            var data = await GetCollection(name);

            metadata.Keys.ToList().ForEach(key => data.AddMetadata(key, metadata[key], isOverride));

            await requestEmbeddingsUpsert(data.Id, ["0"], [data.Description], null, [data.Metadata]);

            return await GetCollection(name);
        }

        // CHROMA DB CLIENT REQUESTS
        private IAsyncEnumerable<string> requestCollectionList()
        {
            try
            {
                return _dbClient.ListCollectionsAsync();
            }
            catch (HttpOperationException ex)
            {
                var error = handleChromaClientError(ex);

                throw new ChromaClientException(error.Code, $"{error.Error}");
            }
        }

        private async Task<ChromaCollectionModel> requestCollection(string name)
        {
            try
            {
                return await _dbClient.GetCollectionAsync(name);
            }
            catch (HttpOperationException ex)
            {
                var error = handleChromaClientError(ex);

                throw new ChromaClientException(error.Code, $"{nameof(requestCollection)} >> {error.Error}");
            }
        }

        private async Task<ChromaCollectionModel> requestNewCollection(string name)
        {
            try
            {
                await _dbClient.CreateCollectionAsync(name);

                return await requestCollection(name);
            }
            catch (HttpOperationException ex)
            {
                var error = handleChromaClientError(ex);

                throw new ChromaClientException(error.Code, $"{nameof(requestNewCollection)} >> {error.Error}");
            }
        }

        private async Task requestCollectionDelete(string name)
        {
            try
            {
                await _dbClient.DeleteCollectionAsync(name);
            }
            catch (HttpOperationException ex)
            {
                var error = handleChromaClientError(ex);

                throw new ChromaClientException(error.Code, $"{nameof(requestCollectionDelete)} >> {error.Error}");
            }
        }

        private async Task<DocumentGetResultModel> requestDocuments(string collectionId, List<string> ids, bool withEmbeddings = true)
        {
            try
            {
                return await _dbClient.GetDocuments(_settings.ServerUrl, collectionId, ids, withEmbeddings: withEmbeddings);
            }
            catch (HttpOperationException ex)
            {
                var error = handleChromaClientError(ex);

                throw new ChromaClientException(error.Code, $"{nameof(requestDocuments)} >> {error.Error}");
            }
        }

        private async Task<DocumentGetResultModel> requestDocuments(string collectionId, bool withEmbeddings, Dictionary<string, object>? filters = null, int? pageSize = null, int? skip = null)
        {
            try
            {
                return await _dbClient.GetDocuments(_settings.ServerUrl, collectionId, withEmbeddings, filters, pageSize, skip);
            }
            catch (HttpOperationException ex)
            {
                var error = handleChromaClientError(ex);

                throw new ChromaClientException(error.Code, $"{nameof(requestDocuments)} >> {error.Error}");
            }
        }

        //null = keep current, value = override
        private async Task<DocumentGetResultModel> requestEmbeddingsUpsert(string collectionId, List<string> ids, List<string>? texts, List<ReadOnlyMemory<float>>? embeddings, List<Dictionary<string, object>>? metadatas = null, bool isCreate = false)
        {
            try
            {
                return await _dbClient.UpsertDocuments(_settings.ServerUrl, collectionId, ids, texts, embeddings, metadatas, isCreate);
            }
            catch (HttpOperationException ex)
            {
                var error = handleChromaClientError(ex);

                throw new ChromaClientException(error.Code, $"{nameof(requestEmbeddingsUpsert)} >> {error.Error}");
            }
        }

        private async Task requestDelete(string collectionId, List<string> ids)
        {
            try
            {
                await _dbClient.DeleteEmbeddingsAsync(collectionId, ids.ToArray());
            }
            catch (HttpOperationException ex)
            {
                var error = handleChromaClientError(ex);

                throw new ChromaClientException(error.Code, $"{nameof(requestDelete)} >> {error.Error}");
            }
        }

        private async Task<DocumentsQueryResultModel> requestQuery(string collectionId, ReadOnlyMemory<float> queryEmbeddings, int nResults, Dictionary<string, object> filters)
        {
            try
            {
                return await _dbClient.QueryDocuments(_settings.ServerUrl, collectionId, [queryEmbeddings], nResults, filters);
            }
            catch (HttpOperationException ex)
            {
                var error = handleChromaClientError(ex);

                throw new ChromaClientException(error.Code, $"{nameof(requestDelete)} >> {error.Error}");
            }
        }

        private ChromaClientError handleChromaClientError(HttpOperationException ex)
            => !string.IsNullOrEmpty(ex.ResponseContent) ? JsonSerializer.Deserialize<ChromaClientError>(ex.ResponseContent) : new ChromaClientError { Error = ex.Message, Code = HttpStatusCode.InternalServerError };

        private List<TChunk> mapChunks(DocumentGetResultModel query, bool withEmbeddings)
        {
            var results = new List<TChunk>();

            if (!query.Ids.Any()) return results;

            for (int i = 0; i < query.Ids.Count; i++)
                results.Add((TChunk)Activator.CreateInstance(typeof(TChunk), query.Ids[i],
                    i <= query.Documents.Count ? query.Documents[i] : string.Empty,
                    withEmbeddings ? query.Embeddings != null && query.Embeddings.Any() && i <= query.Embeddings.Count? new ReadOnlyMemory<float>(query.Embeddings[i]) : new ReadOnlyMemory<float>([]) : new ReadOnlyMemory<float>([]),
                    i <= query.Metadatas.Count ? query.Metadatas[i] : []));

            return results;
        }
        private async Task<int> updateCollectionChunksCount(string name, bool bIncrease, int amount = 1)
        {
            var collection = await GetCollection(name);

            if (collection == null)
                throw new ArgumentNullException($"An error has occured while updating the collection chunks count >> no collection found with name: {name}");

            if (amount == 0)
                amount = 1;

            collection.AddMetadata(nameof(ChromaCollectionMetadata.TOTAL_CHUNKS).ToLower(), collection.GetMeta<ChromaCollectionMetadata>().TOTAL_CHUNKS + (bIncrease ? Math.Abs(amount) : -Math.Abs(amount)));

            await requestEmbeddingsUpsert(collection.Id, ["0"], [collection.Description], [], [collection.Metadata]);

            var update = await GetCollection(name);

            return update.GetMeta<ChromaCollectionMetadata>().TOTAL_CHUNKS;
        }

        protected virtual Dictionary<string, object> getDefaultCollectionMetadata(string description, string? embeddingModel = null, int? dimensions = null)
            => new Dictionary<string, object>
            {
                { nameof(ChromaMetadata.TYPE).ToLower(), EChunkType.COLLECTION },
                { nameof(ChromaCollectionMetadata.MODEL).ToLower(), embeddingModel ?? _settings.EmbeddingModel },
                { nameof(ChromaCollectionMetadata.DIMENSIONS).ToLower(), dimensions == null || dimensions <= 0 ? _settings.EmbeddingDimensions : dimensions },
                { nameof(ChromaCollectionMetadata.TOTAL_CHUNKS).ToLower(), 0 }
            };
    }
}
