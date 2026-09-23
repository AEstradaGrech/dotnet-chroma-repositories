
using Dotnet.Chroma.Repositories.Models.Client.Request;
using Dotnet.Chroma.Repositories.Models.Client.Response;
using Dotnet.Chroma.Repositories.Models.Settings;
using Microsoft.SemanticKernel.Connectors.Chroma;

namespace Dotnet.Chroma.Repositories.Interfaces
{
    public interface IChromaDbClient
    {
        Task<IEnumerable<string>> ListCollections();
        Task<ChromaCollection> CreateCollection(string collectionName, HnswSettings? config);
        Task<ChromaCollection> GetCollection(string collectionName);
        Task<bool> DeleteCollection(string collectionName);

        /// <summary>
        /// Get by doc ids and text match, allowing to retrieve the embedding too
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="ids"></param>
        /// <param name="withEmbeddings"></param>
        /// <returns></returns>
        Task<IEnumerable<ChromaDocumentModel>> GetDocuments(string collection, List<string> ids, bool withEmbeddings = true, string? textSearch = null);

        /// <summary>
        /// Get documents with optional filters, pagination, and, optionally, specific ids or text match
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="pageSize"></param>
        /// <param name="skip"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<IEnumerable<ChromaDocumentModel>> FilterDocuments(string collection, Dictionary<string, object>? filters = null, bool withEmbeddings = false, int? pageSize = null, int? skip = null, List<string>? ids = null, string? textSearch = null);

        Task<bool> UpsertDocument(string collectionId, ChromaClientUpsertRequest request);
    }
}
