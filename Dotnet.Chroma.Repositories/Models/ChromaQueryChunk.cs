
namespace Dotnet.Chroma.Repositories.Models
{
    /// <summary>
    /// Result of a similarity search (collection.Query).
    /// Returns the Text / Document, attached embedding and metadata.
    /// Includes the 'distance' score.
    /// Uses default ChunkMetadata class
    /// </summary>
    public class ChromaQueryChunk : ChromaChunk
    {
        public ChromaQueryChunk() : base() { }
        public ChromaQueryChunk(string id, double distance, string document, ReadOnlyMemory<float> embedding, Dictionary<string, object> metadata) : base(id,document, embedding, metadata) { Distance = distance; }
        public double Distance { get; set; }
    }
}
