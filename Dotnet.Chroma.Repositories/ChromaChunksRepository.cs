using Dotnet.Chroma.Repositories.Models;
using Dotnet.Chroma.Repositories.Models.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Connectors.Chroma;

namespace Dotnet.Chroma.Repositories
{
#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    
    /// <summary>
    /// Default implementation
    /// </summary>
    public class ChromaChunksRepository : ChromaRepository<ChromaChunksCollection<ChromaChunk>, ChromaChunk>, IChromaChunksRepository
    {
        public ChromaChunksRepository(IOptions<ChromaSettings> dbSettings, IChromaClient client) : base(client, dbSettings) { }
    }
}
