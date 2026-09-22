using System.Text.Json.Serialization;

namespace Dotnet.Chroma.Repositories.Models.Settings
{
    public class HnswSettings
    {
        /// <summary>
        /// Distance metric (l2, cosine, ip)
        /// </summary>
        [JsonPropertyName("space")]
        public string Space { get; set; } = "l2";
        /// <summary>
        /// Candidate list size during index build
        /// </summary>
        [JsonPropertyName("ef_construction")]
        public int EfConstruction { get; set; } = 100;
        /// <summary>
        /// Max connections per node (M)
        /// </summary>
        [JsonPropertyName("max_neighbors")]
        public int MaxNeighbors { get; set; } = 16;
        /// <summary>
        /// Candidate list size during search
        /// </summary>
        [JsonPropertyName("ef_search")]
        public int EfSearch { get; set; } = 50;
        /// <summary>
        /// Threads for index ops (CPU cores)
        /// </summary>
        [JsonPropertyName("num_threads")]
        public int NumThreads { get; set; } = 4;
    }
}
