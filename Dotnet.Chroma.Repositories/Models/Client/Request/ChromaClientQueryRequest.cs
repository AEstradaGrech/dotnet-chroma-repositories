using System.Text.Json.Serialization;

namespace Dotnet.Chroma.Repositories.Models.Client.Request
{
    /// <summary>
    /// Dto for the QueryEmbeddings extension method request with the metadata filter feature missing in the SK package.
    /// </summary>
    public class ChromaClientQueryRequest : ChromaClientFilterRequest
    {
        [JsonPropertyName("query_embeddings")]
        public List<ReadOnlyMemory<float>> Embeddings { get; set; }
        [JsonPropertyName("n_results")]
        public int Results { get; set; }
    }
}
