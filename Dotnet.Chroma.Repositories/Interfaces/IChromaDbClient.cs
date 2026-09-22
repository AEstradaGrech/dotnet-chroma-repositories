
using Dotnet.Chroma.Repositories.Models.Settings;

namespace Dotnet.Chroma.Repositories.Interfaces
{
    public interface IChromaDbClient
    {
        Task<IEnumerable<string>> ListCollections();
        Task<ChromaCollection> CreateCollection(string collectionName, HnswSettings? config);
    }
}
