using Microsoft.SemanticKernel.Connectors.Chroma;

namespace Dotnet.Chroma.Repositories.Models.Client.Response
{
#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    /// <summary>
    /// Extends the Semantic Kernel model to allow the mapping of the 'document' property comming from Chroma DB in GetEmbeddings requests
    /// </summary>
    public class DocumentGetResultModel : ChromaEmbeddingsModel
    {
        public List<string> Documents { get; set; } = new List<string>();
    }
#pragma warning restore SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
}
