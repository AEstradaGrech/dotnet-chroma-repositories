using System.Text.Json.Serialization;

namespace Dotnet.Chroma.Repositories.Models.Metadata
{
    /// <summary>
    /// Type for the collection should be always COLLECTION
    /// Use CHUNK_TYPE to filter the collections by any Enum used to classify inherited classes
    /// Extend this class to create new type of chunk collections of different shapes / metadatas / fields
    /// </summary>
    public class ChromaCollectionMetadata : ChromaMetadata
    {
        [JsonPropertyName("total_chunks")]
        public int TOTAL_CHUNKS { get; set; }
        [JsonPropertyName("chunk_type")]
        public int CHUNK_TYPE { get; set; }
    }
}
