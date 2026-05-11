using System.Text.Json.Serialization;

namespace Dotnet.Chroma.Repositories.Models.Client.Request
{
    /// <summary>
    /// Dto for the GetEmbeddings extension method request with some missing properties not present in the SK package.
    /// </summary>
    public class ChromaClientGetRequest : ChromaClientFilterRequest
    {
        [JsonPropertyName("ids")]
        public List<string>? Ids { get; set; } = null;

        [JsonPropertyName("limit")]
        public int? Results { get; set; } = null;

        [JsonPropertyName("offset")]
        public int? Skip { get; set; } = null;
    }
}
