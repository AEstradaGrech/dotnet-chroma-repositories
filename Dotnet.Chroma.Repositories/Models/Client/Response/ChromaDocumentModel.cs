using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Dotnet.Chroma.Repositories.Models.Client.Response
{
    /// <summary>
    /// Chunk representation without query distances. Might include the embedding
    /// </summary>
    public class ChromaDocumentModel
    {
        //
        // Summary:
        //     List of embedding identifiers.
        [JsonPropertyName("ids")]
        public List<string> Ids { get; set; } = new List<string>();

        //
        // Summary:
        //     List of embedding vectors.
        [JsonPropertyName("embeddings")]
        public List<float[]> Embeddings { get; set; } = new List<float[]>();

        //
        // Summary:
        //     List of embedding metadatas.
        [JsonPropertyName("metadatas")]
        public List<Dictionary<string, object>> Metadatas { get; set; } = new List<Dictionary<string, object>>();

        /// <summary>
        /// Chunk text associated with the embedding.
        /// </summary>
        [JsonPropertyName("documents")]
        public List<string> Documents { get; set; } = new List<string>();
    }
}
