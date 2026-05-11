namespace Dotnet.Chroma.Repositories.Models.Interfaces
{
    /// <summary>
    /// Repository implementation contract for the default implementation.
    /// Extend the implementation interface with custom methods for your extended collections or base method overrides if necessary
    /// </summary>
    public interface IChromaChunksRepository : IChromaRepository<ChromaChunksCollection<ChromaChunk>, ChromaChunk> {}
}
