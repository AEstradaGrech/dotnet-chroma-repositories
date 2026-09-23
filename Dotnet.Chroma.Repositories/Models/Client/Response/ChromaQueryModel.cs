
using System.Text.Json.Serialization;

namespace Dotnet.Chroma.Repositories.Models.Client.Response
{
    public class ChromaQueryModel : ChromaDocumentModel
    {

        /// <summary>
        /// List of embedding distances.
        /// </summary>
        [JsonPropertyName("distances")]
        public List<List<double>> Distances { get; set; } = [];
    }
}
