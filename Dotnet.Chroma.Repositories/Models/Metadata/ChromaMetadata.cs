using System.Text.Json.Serialization;

namespace Dotnet.Chroma.Repositories.Models.Metadata
{
    // TYPE is DOCUMENT by default.
    // Use any casted Enum value to classify inherited classes and filter your queries by extended clasess (like: FILES | CHATS | WHATEVER)
    // Extend this class to map metadata fields to your extended class and give a chunk the shape you need.
    public class ChromaMetadata
    {
        [JsonPropertyName("model")]
        public string MODEL { get; set; }
        [JsonPropertyName("dimensions")]
        public int DIMENSIONS { get; set; }
        [JsonPropertyName("type")]
        public int TYPE { get; set; }
        [JsonPropertyName("document_name")]
        public string DOCUMENT_NAME { get; set; }
    }
}
