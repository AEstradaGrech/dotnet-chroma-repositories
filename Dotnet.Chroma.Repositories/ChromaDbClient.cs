using Dotnet.Chroma.Repositories.Interfaces;
using Dotnet.Chroma.Repositories.Models.Client.Response;
using Dotnet.Chroma.Repositories.Models.Exceptions;
using Dotnet.Chroma.Repositories.Models.Settings;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
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
                var result = await _httpClient.GetFromJsonAsync<IEnumerable<JsonObject>>($"{_settings.ServerUrl}/api/v2/tenants/{_settings.Tenant}/databases/{_settings.Database}/collections");

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
                
                var result = await _httpClient.PostAsJsonAsync($"{_settings.ServerUrl}/api/v2/tenants/{_settings.Tenant}/databases/{_settings.Database}/collections", new { name = collectionName, configuration = config });

                var content = await result.Content.ReadFromJsonAsync<JsonObject>();
                return new ChromaCollection { Id = (string)content["id"], Name = (string)content["name"] };
            }
            catch (Exception ex) 
            {
                throw new ChromaClientException(HttpStatusCode.InternalServerError, $"{nameof(CreateCollection)} >> {ex.Message}");
            }
        }
    }
}
