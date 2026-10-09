
using Dotnet.Chroma.Repositories.Models.Settings;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Dotnet.Chroma.Repositories.Models.Client
{
    public class ChromaCollectionConverter : JsonConverter<ChromaCollection>
    {
        public override ChromaCollection? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);

            var root = doc.RootElement;

            var hnsw = root.GetProperty("configuration_json").GetProperty("hnsw").GetRawText();

            return new ChromaCollection { Id = root.GetProperty("id").GetString() ?? string.Empty, Name = root.GetProperty("name").GetString() ?? string.Empty, Config = JsonSerializer.Deserialize<HnswSettings>(hnsw) };
        }

        public override void Write(Utf8JsonWriter writer, ChromaCollection value, JsonSerializerOptions options)
            => JsonSerializer.Serialize<ChromaCollection>(writer, value, options);
    }
}
