using System.Net;
using System.Text.Json.Serialization;

namespace Dotnet.Chroma.Repositories.Models.Client.Response
{
    /// <summary>
    /// Helper class to parse inner chroma error messages throwed by the IChromaClient
    /// </summary>
    public class ChromaClientError
    {
        [JsonPropertyName("error")]
        public string? Error { get; set; }
        public HttpStatusCode Code { get; set; } = HttpStatusCode.BadRequest;
    }
}
