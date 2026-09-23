using Dotnet.Chroma.Repositories.Interfaces;
using Dotnet.Chroma.Repositories.Models.Client.Request;
using Dotnet.Chroma.Repositories.Models.Client.Response;
using Dotnet.Chroma.Repositories.Models.Exceptions;
using Dotnet.Chroma.Repositories.Models.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;


namespace Dotnet.Chroma.Repositories
{
    public class ChromaDbClient : IChromaDbClient
    {
        private readonly HttpClient _httpClient;
        private readonly ChromaSettings _settings;
        public ChromaDbClient(HttpClient client, IOptions<ChromaSettings> settings) 
        {
            _httpClient = client;
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(ChromaSettings));
        }

        public async Task<IEnumerable<string>> ListCollections()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<IEnumerable<JsonObject>>($"{_settings.BaseUrl()}/collections");

                return result.Select(x => (string)x["name"]).Where(x => !string.IsNullOrEmpty(x)).OrderBy(x => x);
            }
            catch (Exception ex)
            {
                throw new ChromaClientException(HttpStatusCode.InternalServerError, $"{nameof(ListCollections)} >> {ex.Message}");
            }
        }

        public async Task<ChromaCollection> CreateCollection(string collectionName, HnswSettings? config)
        {
            try
            {
                if (config == null)
                    config = _settings.HnswSettings;
                
                var result = await _httpClient.PostAsJsonAsync($"{_settings.BaseUrl()}/collections", new { name = collectionName, configuration = config });

                var content = await result.Content.ReadFromJsonAsync<JsonObject>();
                return new ChromaCollection { Id = (string)content["id"], Name = (string)content["name"] };
            }
            catch (Exception ex) 
            {
                throw new ChromaClientException(HttpStatusCode.InternalServerError, $"{nameof(CreateCollection)} >> {ex.Message}");
            }
        }

        public async Task<ChromaCollection> GetCollection(string collectionName)
        {
            try
            {
                var collection = await _httpClient.GetFromJsonAsync<ChromaCollection>($"{_settings.BaseUrl(collectionName)}");
                
                return collection;
            }
            catch(Exception ex)
            {
                throw new ChromaClientException(HttpStatusCode.InternalServerError, $"{nameof(GetCollection)} >> {ex.Message}");
            }
        }

        public async Task<bool> DeleteCollection(string collectionName)
        {
            try
            {
                var result = await _httpClient.DeleteAsync($"{_settings.BaseUrl(collectionName)}");

                return result.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new ChromaClientException(HttpStatusCode.InternalServerError, $"{nameof(DeleteCollection)} >> {ex.Message}");
            }
        }

        public async Task<IEnumerable<ChromaDocumentModel>> GetDocuments(string collection, List<string> ids, bool withEmbeddings = true, string? textSearch = null)
        {
            try
            {
                var request = new ChromaDocumentsRequest
                {
                    Ids = ids,
                    Includes = ["metadatas", "documents"],
                    TextMatch = !string.IsNullOrEmpty(textSearch.Trim()) ? $"{{\"$contains\": \"{textSearch}\"}}" : null
                };

                if (withEmbeddings)
                    request.Includes.Add("embeddings");

                var result = await _httpClient.PostAsJsonAsync($"{_settings.BaseUrl()}/collections/{collection}/get", request);

                return await result.Content.ReadFromJsonAsync<IEnumerable<ChromaDocumentModel>>();
            }
            catch (Exception ex)
            {
                throw new ChromaClientException(HttpStatusCode.InternalServerError, $"{nameof(GetDocuments)} >> {ex.Message}");
            }
        }

        public async Task<IEnumerable<ChromaDocumentModel>> FilterDocuments(string collection, Dictionary<string, object>? filters = null, bool withEmbeddings = false, int? pageSize = null, int? skip = null, List<string>? ids = null, string? textSearch = null)
        {
            try
            {
                var request = new ChromaDocumentsRequest
                {
                    Ids = ids,
                    Includes = ["metadatas", "documents"],
                    TextMatch = !string.IsNullOrEmpty(textSearch.Trim()) ? $"{{\"$contains\": \"{textSearch}\"}}" : null,
                    Skip = skip,
                    Results = pageSize,
                    MetadataFilters = filters != null && filters.Any() ? buildMetadataFilter(filters) : null
                };

                if (withEmbeddings)
                    request.Includes.Add("embeddings");

                var result = await _httpClient.PostAsJsonAsync<ChromaDocumentsRequest>($"{_settings.BaseUrl(collection)}/get", request);

                return await result.Content.ReadFromJsonAsync<IEnumerable<ChromaDocumentModel>>();
            }
            catch (Exception ex)
            {
                throw new ChromaClientException(HttpStatusCode.InternalServerError, $"{nameof(GetDocuments)} >> {ex.Message}");
            }
        }

        //Upsert

        private string buildMetadataFilter(Dictionary<string, object> filters)
            => $"{{\"$and\": [{string.Join(",", filters.Select(kv => $"{{\"{kv.Key}\": \"{kv.Value}\"}}").ToList())}]}}";

        public async Task<bool> UpsertDocument(string collection, ChromaClientUpsertRequest request)
        {
            try
            {
                var result = await _httpClient.PostAsJsonAsync($"{_settings.BaseUrl(collection)}/upsert", request);

                return result.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new ChromaClientException(HttpStatusCode.InternalServerError, $"{nameof(GetDocuments)} >> {ex.Message}");
            }
        }
    }
}
