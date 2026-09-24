using Dotnet.Chroma.Repositories.Interfaces;
using Dotnet.Chroma.Repositories.Models;
using Dotnet.Chroma.Repositories.Models.Interfaces;
using Dotnet.Chroma.Repositories.Models.Settings;
using Microsoft.Extensions.Options;

namespace Dotnet.Chroma.Repositories
{

    /// <summary>
    /// Default implementation
    /// </summary>
    public class ChromaChunksRepository : ChromaRepository<ChromaChunksCollection<ChromaChunk>, ChromaChunk>, IChromaChunksRepository
    {
        public ChromaChunksRepository(IOptions<ChromaSettings> dbSettings, IChromaDbClient dbClient) : base(dbClient, dbSettings) { }
    }
}
