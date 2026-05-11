using System.Text.Json.Serialization;

namespace Dotnet.Chroma.Repositories.Models.Client.Request
{
    /// <summary>
    /// Dto for the UpsertEmbeddings extension method.
    /// Allows to insert or update already inserted items including the 'documents' property missing from the SemanticKernel package.
    /// 
    /// Note: the 'documents' param maps to the inner ChromaDB 'documents' field, and it is necessary to store the embedding's attached
    /// text 'the chroma way' so it is returned in the documents array when using the 'include: [ "documents"] request option.)
    /// </summary>
    public class ChromaClientUpsertRequest
    {
        [JsonPropertyName("ids")]
        public List<string> Ids { get; set; }

        [JsonPropertyName("documents")]
        public List<string>? Documents { get; set; }

        [JsonPropertyName("embeddings")]
        public List<ReadOnlyMemory<float>>? Embeddings { get; set; }

        [JsonPropertyName("metadatas")]
        public List<Dictionary<string, object>>? Metadatas { get; set; }
    }
}
