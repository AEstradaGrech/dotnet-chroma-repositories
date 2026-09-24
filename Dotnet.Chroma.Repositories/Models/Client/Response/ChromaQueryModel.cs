
using System.Text.Json.Serialization;

namespace Dotnet.Chroma.Repositories.Models.Client.Response
{
    public class ChromaQueryModel
    {
        /// <summary>
        /// List of embedding identifiers.
        /// </summary>
        [JsonPropertyName("ids")]
        public List<List<string>> Ids { get; set; } = [];


        /// <summary>
        /// Chunk text associated with the embedding.
        /// </summary>
        [JsonPropertyName("documents")]
        public List<List<string>> Documents { get; set; } = new List<List<string>>();

        /// <summary>
        /// List of embedding vectors.
        /// </summary>
        [JsonPropertyName("embeddings")]
        public List<List<float[]>> Embeddings { get; set; } = [];

        /// <summary>
        /// List of embedding metadatas.
        /// </summary>
        [JsonPropertyName("metadatas")]
        public List<List<Dictionary<string, object>>> Metadatas { get; set; } = [];

        /// <summary>
        /// List of embedding distances.
        /// </summary>
        [JsonPropertyName("distances")]
        public List<List<double>> Distances { get; set; } = [];
    }
}
