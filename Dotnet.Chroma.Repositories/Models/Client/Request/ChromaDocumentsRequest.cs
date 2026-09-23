using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Dotnet.Chroma.Repositories.Models.Client.Request
{
    public class ChromaDocumentsRequest
    {
        [JsonPropertyName("where")]
        public string? MetadataFilters { get; set; } = null;
        [JsonPropertyName("where_document")]
        public string? TextMatch { get; set; } = null;
        [JsonPropertyName("include")]
        public List<string> Includes { get; set; }

        [JsonPropertyName("ids")]
        public List<string>? Ids { get; set; } = null;

        [JsonPropertyName("limit")]
        public int? Results { get; set; } = null;

        [JsonPropertyName("offset")]
        public int? Skip { get; set; } = null;
    }
}
