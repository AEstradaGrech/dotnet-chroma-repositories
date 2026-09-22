
namespace Dotnet.Chroma.Repositories.Interfaces
{
    public interface IChromaDbClient
    {
        Task<IEnumerable<string>> GetCollections();
        Task<string> CreateCollection(string collectionName);
    }
}
