using Dotnet.Chroma.Repositories.Interfaces;
using Dotnet.Chroma.Repositories.Models.Client.Request;
using Dotnet.Chroma.Repositories.Models.Client.Response;
using Dotnet.Chroma.Repositories.Models.Exceptions;
using Dotnet.Chroma.Repositories.Models.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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

                if (await validChromaResponse(result))
                {
                    var content = await result.Content.ReadFromJsonAsync<JsonObject>();

                    return new ChromaCollection { Id = (string)content["id"], Name = (string)content["name"] };
                }

                else return null;
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

                return await validChromaResponse(result);
            }
            catch (Exception ex)
            {
                throw new ChromaClientException(HttpStatusCode.InternalServerError, $"{nameof(DeleteCollection)} >> {ex.Message}");
            }
        }

        public async Task<ChromaDocumentModel> GetDocuments(string collection, List<string> ids, bool withEmbeddings = true, string? textSearch = null)
        {
            try
            {
                var request = new ChromaDocumentsRequest
                {
                    Ids = ids,
                    Includes = ["metadatas", "documents"],
                    TextMatch = !string.IsNullOrEmpty(textSearch) ? $"{{\"$contains\": \"{textSearch}\"}}" : null
                };

                if (withEmbeddings)
                    request.Includes.Add("embeddings");

                var result = await _httpClient.PostAsJsonAsync($"{_settings.BaseUrl()}/collections/{collection}/get", request);

                //var response = await result.Content.ReadFromJsonAsync<JsonObject>();

                //return JsonSerializer.Deserialize<ChromaDocumentModel>(response);
                return await validChromaResponse(result) ? await result.Content.ReadFromJsonAsync<ChromaDocumentModel>() : null;
            }
            catch (Exception ex)
            {
                throw new ChromaClientException(HttpStatusCode.InternalServerError, $"{nameof(GetDocuments)} >> {ex.Message}");
            }
        }

        public async Task<ChromaDocumentModel> FilterDocuments(string collection, Dictionary<string, object>? filters = null, bool withEmbeddings = false, int? pageSize = null, int? skip = null, List<string>? ids = null, string? textSearch = null)
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

                return await validChromaResponse(result) ? await result.Content.ReadFromJsonAsync<ChromaDocumentModel>() : null;
            }
            catch (Exception ex)
            {
                throw new ChromaClientException(HttpStatusCode.InternalServerError, $"{nameof(FilterDocuments)} >> {ex.Message}");
            }
        }

        public async Task<bool> UpsertDocument(string collectionId, ChromaClientUpsertRequest request)
        {
            try
            {
                var result = await _httpClient.PostAsJsonAsync($"{_settings.BaseUrl(collectionId)}/upsert", request);

                return await validChromaResponse(result);
            }
            catch (Exception ex)
            {
                throw new ChromaClientException(HttpStatusCode.InternalServerError, $"{nameof(UpsertDocument)} >> {ex.Message}");
            }
        }

        public async Task<int> DeleteDocuments(string collection, List<string> ids)
        {
            try
            {
                var result = await _httpClient.PostAsJsonAsync($"{_settings.BaseUrl(collection)}/delete", new { ids = ids });

                if (await validChromaResponse(result))
                {
                    var resultObject = await result.Content.ReadFromJsonAsync<JsonObject>();

                    return resultObject["deleted"].GetValue<int>();
                }

                else return -1;
            }
            catch (Exception ex)
            {
                throw new ChromaClientException(HttpStatusCode.InternalServerError, $"{nameof(DeleteDocuments)} >> {ex.Message}");
            }
        }

        public async Task<ChromaQueryModel> QueryDocuments(string collection, ReadOnlyMemory<float> queryEmbeddings, int resultsNumber, Dictionary<string, object>? metadataFilters = null, int? offset = null, int? limit = null)
        {
            try
            {
                var url = $"{_settings.BaseUrl(collection)}/query";

                if (offset.HasValue)
                    url += $"?offset={offset}";

                if(limit.HasValue)
                    url += $"{(offset.HasValue ? "&" : "?")}limit={limit}";
                
                var result = await _httpClient.PostAsJsonAsync(url, new ChromaQueryRequest
                {
                    Embeddings = [queryEmbeddings],
                    Results = resultsNumber,
                    MetadataFilters = metadataFilters != null && metadataFilters.Any() ? buildMetadataFilter(metadataFilters) : null,
                    Includes = ["documents", "embeddings", "distances", "metadatas"]
                });

                return await validChromaResponse(result) ? await result.Content.ReadFromJsonAsync<ChromaQueryModel>() : null;
            }
            catch(Exception ex)
            {
                throw new ChromaClientException(HttpStatusCode.InternalServerError, $"{nameof(QueryDocuments)} >> {ex.Message}");
            }
        }

        private string buildMetadataFilter(Dictionary<string, object> filters)
            => $"{{\"$and\": [{string.Join(",", filters.Select(kv => $"{{\"{kv.Key}\": \"{kv.Value}\"}}").ToList())}]}}";

        private async Task<bool> validChromaResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadFromJsonAsync<JsonObject>();

                var reasonPhrase = response.ReasonPhrase ?? "Chroma Db Error";

                throw new ChromaClientException(response.StatusCode, $"{reasonPhrase} >> {errorMessage["error"]} >> {errorMessage["message"]}");
            }
            
            return true;
        }
    }
}
