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
        public List<List<string>> Ids { get; set; } = new List<List<string>>();

        //
        // Summary:
        //     List of embedding vectors.
        [JsonPropertyName("embeddings")]
        public List<List<float[]>> Embeddings { get; set; } = new List<List<float[]>>();

        //
        // Summary:
        //     List of embedding metadatas.
        [JsonPropertyName("metadatas")]
        public List<List<Dictionary<string, object>>> Metadatas { get; set; } = new List<List<Dictionary<string, object>>>();

        /// <summary>
        /// Chunk text associated with the embedding.
        /// </summary>
        [JsonPropertyName("documents")]
        public List<string> Documents { get; set; } = new List<string>();
    }
}
