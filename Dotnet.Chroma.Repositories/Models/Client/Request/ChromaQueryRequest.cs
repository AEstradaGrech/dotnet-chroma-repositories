
using System.Text.Json.Serialization;

namespace Dotnet.Chroma.Repositories.Models.Client.Request
{
    public class ChromaQueryRequest : ChromaDocumentsRequest
    {
        [JsonPropertyName("query_embeddings")]
        public List<ReadOnlyMemory<float>> Embeddings { get; set; }

        [JsonPropertyName("n_results")]
        public int Results { get; set; }
    }
}
