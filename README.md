# dotnet-chroma-repositories

Lightweight ChromaDB repository SDK for .NET that wraps around the Semantic Kernel Microsoft.SemanticKernel.Connectors.Chroma (for now).

This is the repository of the 'AEG.ChromaDB.Repositories' nuget package.

# What's in the box:

- A little framework that splits the chroma data model in COLLECTION and CHUNKS and allows to work with Chroma data in more C# friendly way.
- A repository template of <TCollection, TChunk> to access chroma and easily work with this structures.
- A default implementation sample ready to register, inject and use.
- A some features missing from the Microsoft.SemanticKernel.Connectors.Chroma package. Specifically:
    > Document property binding.
    > Metadata filtering.
    > Pagination options.

### IMPORTANT:
 - For this package: Microsoft.SemanticKernel.Connectors.Chroma --version 1.74.0-alpha
 - USE THIS exact Chroma image VERSION: chromadb/chroma:0.4.24
 - Run the container: docker run --name local-chroma-container -v ./chroma-data:/data -p XXXX:8000 chromadb/chroma:0.4.24

Note: I plan to remove the Semantic Kernel dependency in future versions, but this is the required setup for now.
Note: You will need the docker volume to persist chroma data. In the command is a bind mount to a folder of you PC so
      create a folder that will be your 'bridge' to the container and execute the docker command from there.
      
# How does it work:

The sdk makes use of this classes to handle the ChromaDB data:

    - COLLECTION: A collection is a groupping structure for chunks and summary / common data.
    - CHUNK: A chunk is a unit of data associated to a collection. It may have an embedding attached or it can be a 'text only' chunk.
    - METADATA: This maps the chroma metadata to a typed class. This DEFINES the shape of a chunk or a collection. 
    - REPOSITORY: This is the chroma connector. Handles the access to chroma and defines the types of TCol and TChunk that are going to be retrieved / inserted through it.

So, the final structures hierarchy is:

- Repository<TCol, TChunk> of:
    - Collections<TCol> with:
        - 1 x CollectionMetadata<TCol>
        - N x Chunks<TChunk> with:
            - 1 x ChunkMetadata<TChunk> 

## Class details:

    - COLLECTIONS: A collection is a data structure to hold summary data that is common to or necessary for the whole collection chunks.

            > A collection is ALWAYS the first chunk that is created in a collection (so ID = 0 is always the Collection Chunk)
            > A collection has no text embedding, only a description.
            > A collection always stores:

                · MODEL + DIMENSIONS: a similarity search requires the query text to be embedded with the same model and dimensions than the queried texts. 
                                    This ensures that al the stored texts in a collection use the same params and they are retrieved to embed the user query during a search request.

                · TOTAL CHUNKS: the total generated chunks for this collection. Useful for pagination.

                · TYPE & CHUNK TYPE: 'Type' is a common property to both collections and nodes and it is used mostly as a metadata filter to query collections and chunks separately.
                                    For a collection should be ALWAYS collection (assigned by default) and should not be overriden (if you do that, use different enums for collections and chunks)
                                    Type differentiates the Collection Chunk (ID = 0) from the rest, but in order to differentiate a collection type from other, you must use 
                                    the CHUNK TYPE metadata field, that stores the assigned Chunk type (for example: Type = Collection - Chunk Type = FILES | CHATS | OTHER
                                    
    - CHUNKS: Chunks are the pieces of data itselfs. This are the texts you want to query later, with the shape / metadata you need to work with them.

            > A chunk has an ID that is assigned on creation based on the total count retrieved from the Collection.
            > A chunk may have or may have nor an embedding nad be queried by it's metadata and or ID (then Chroma becomes a text database)
            > A chunk is a data structure that can be considered as a Table Item in which you define the table fields using the metadata tags and a class to serialize them and work easier with the values.
            > All chunks have a DefaultMetadata class that contains a typed form of it's attached metadata. You can map values from this Metadata class to your extened Chunk class to work better with it or 
            you can use the 'chunk.GetMeta<T>()' generic method to access the metadata directly. The important thing is: CHUNK = CHUNK.cs + METADATA.cs;
            
    
    - METADATA: The metadata class maps the chunk attached metadata to a C# class. This makes it easier to work with the data, but it also DEFINES the shape of a chunk and it can be seen as a DB Table definition. 
                The way it shapes the data is through the chunk constructors system. It works like this:
    
            > Chroma allows to add metadata to a chunk that can be used to filter the data in a similarity search or get requests. Metadata are Key-Value pairs of any type.
            Everytime a chunk is created it uses the default constructors to set the default chunk type and also an empty dictionary of properties, and the it calls the 'setDefaultMetadata' method
            that will deserialize the dictionary and convert it to the assigned Metadata Class.

            > When a chunk is created / recieved from ChromaDB, it uses the 'full' constructor that expects all the data required by the Chunk model (Id, document, optional, embedding and default metadata),
            assignes the db values and call the base default constructors too so the recieved metadata is deserialized again to the Metadata class.

            > Because the 'setDefaultMetadata' is abstract, it must be implemented in any extended class where you deserialize to the type's own metadata class and, because the method is called always during construction for default (no id) and db chunks, the chunk
            always get transformed to its type. Then you can use the GetMeta<T> method of the chunk model to get the DefaultMetadata class casted to the extended metadata class.

            > Also, the metadata class MAPS the metadata name to the dictionary that is being passed to chroma. To ensure that the correct keys are passed, use the 'AddMetadata'
              method of a chunk to add or override dictionary values, optionally re-deserializing the result to the MetadataClass, 'updating' the model values (this is to avoid deserializing each call and to work with 'current' and 'toUpdate' values) 
    
    Note: In order to enforce the Collection / Chunk model distinction, Collections and Chunk have both their own Metadata class. Remeber to extend 'CollectionMetadata' & 'ChunkMetadata' accordingly in inherited classes

    - REPOSITORIES: A repository is a way to connect to chroma but also a way to reshape the recieved data according to the desired types. It isolates the chroma client access from your app to enforce the usage of your defined types
                    and provides an API to read / write data to chroma in a way that is aligned with the SDK rules, and catch any inner chroma exception so you can handle it according to your needs.  

                    The Repository makes use of the Semantic Kernel IChromaClient (for now) to communicate with Chroma, and it lacked of some features that I required so the package includes a few Extension methods for the SK client
                    to allow:

                        > get / query data filtering by any desired metadata tag (useful to perform hybrid searches)
                        > 'document' field binding. The chroma api allows to send a 'documents' array that will map to a 'document' field automatically for the correlative chunk Id of the Ids array, so you can
                        retrieve the embedded document text using the "documents" tag in the 'includes' array. The SK package allows only to insert Ids + embeddings + metadata so I added extensions method to bind
                        the embeded text and make it part of the Chunk Model (it is also the collection 'Description' property)
                        > pagination options: the chroma api allows to request 'limit' and 'offset' options in the /get endpoint, so I added that feature to the extension related to that endpoint. 
# How to use:

Let's say you want to create repository of Files to embed multiple files in the same collection. To do that:

(Step 0: use the 'AddChromaClient' and optionally the 'AddChromaConfiguration' startup extensions to register the services client and inject the ChromaSettings from your appsettings.json. Also, ensure you have registered your repo)

1 - Create a class inheriting from ChromaCollection (for example, ChromaFilesCollection). This will be the 'recipient' of File Chunks. 
2 - Create a class that inherits from ChromaCollectionMetadata (for example, FilesCollectionMetadata). This will be used to type the new FileCollection metadata properties.
3 - Create a class that inherits from ChromaChunk (for example, ChromaFileChunk). This will be the file containing the actual data / the queried data.
4 - Create a class that inherits from ChromaMetadata (for example, FileChunkMetadata). This will be used to type the FileChunks metadata.
5 - Create a class that inherits from ChromaRepository and type it with your new classes (for example, ChromaFilesRepository<ChromaFilesCollection, ChromaFileChunk>).
    Recomended: Create an interface with the same name than your repository extending the IChromaRepository<TCol, TChunk> interface. This will make it easier to register the service by name convention and syntax is prettier)
6 - Create an enum to tag the TYPE and CHUNK_TYPE properties of chunks and collections (respectively). This will help to identify different  collection types and it is also used as a metadata filter. For example:

        EChunkType.FILE = 2, 
        EChunkType.CUSTOMER_REVIEWS = 3,
        [...],

    (Important: values 0 and 1 are reserved for 'COLLECTION' and 'DOCUMENT' types. ALWAYS START FROM 2 TO AVOID OVERRIDING ANY 'COLLECTION' TAG)

    Then when you create a collection the sdk will assign 'COLLECTION' as 'TYPE' for the collection chunk and your Enum value for the 'CHUNK_TYPE'.
    Chunks in the other hand will have 'TYPE' = <YourEnumValueAsInt> so they can filtered later using your enum. 
    
    (Note: use the 'DefaultChunk(string collectionName)' method to generate new chunks for a collection with using the predefined collection CHUNK_TYPE, EMBEDDING MODEL and DIMENSIONS)

# Implementation sample repo:

This repository is the origin of the package and the current implementation sample of the nuget: https://github.com/AEstradaGrech/dotnet-llamasharp

It makes use of the package to handle File, Chats and System messages for a RAG application and makes use of the package Default Implementation to inspect the files in a generic way.
