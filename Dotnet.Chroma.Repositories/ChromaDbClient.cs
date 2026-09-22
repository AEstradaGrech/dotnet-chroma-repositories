using Dotnet.Chroma.Repositories.Interfaces;
using Dotnet.Chroma.Repositories.Models.Settings;
using Microsoft.Extensions.Options;
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

        public async Task<IEnumerable<string>> GetCollections()
        {
            var result = await _httpClient.GetFromJsonAsync<IEnumerable<JsonObject>>($"{_settings.ServerUrl}/api/v2/tenants/{_settings.Tenant}/databases/{_settings.Database}/collections");
            return result.Select(x => (string)x["name"]).Where(x => !string.IsNullOrEmpty(x)).OrderBy(x => x);
        }

        public async Task<string> CreateCollection(string collectionName)
        {
            throw new NotImplementedException();
        }

        
    }
}
