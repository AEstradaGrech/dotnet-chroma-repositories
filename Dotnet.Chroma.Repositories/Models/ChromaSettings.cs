namespace Dotnet.Chroma.Repositories.Models
{
    /// <summary>
    /// Chroma basic / required settings;
    /// Inject this from your app settings using the StartupExtensions (create a section in your appsettings.json with this exact name 'ChromaSettings' with the desired values
    /// </summary>
    public class ChromaSettings
    {
        public string ServerUrl { get; set; } = "http://localhost:9000";
        public string EmbeddingModel { get; set; } = "nomic-text-embed";
        public int EmbeddingsDimension { get; set; } = 512;
    }
}
