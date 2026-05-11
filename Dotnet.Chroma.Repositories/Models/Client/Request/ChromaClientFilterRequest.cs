using System.Text.Json.Serialization;

namespace Dotnet.Chroma.Repositories.Models.Client.Request
{
    /// <summary>
    /// Base Request for the Chroma Client extensions method that allows to pass a 'where' clause
    /// to filter by metadata to perform hybrid searches.
    /// </summary>
    public class ChromaClientFilterRequest
    {
        [JsonPropertyName("where")]
        public Dictionary<string, object>? MetadataFilters { get; set; } = null;
        [JsonPropertyName("include")]
        public List<string> Includes { get; set; }
    }
}
