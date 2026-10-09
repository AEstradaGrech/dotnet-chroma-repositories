using Dotnet.Chroma.Repositories.Models.Settings;

namespace Dotnet.Chroma.Repositories
{
    public class ChromaCollection
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public HnswSettings Config { get; set; }
    }
}
