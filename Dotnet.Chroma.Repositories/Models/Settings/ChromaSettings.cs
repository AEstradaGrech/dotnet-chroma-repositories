namespace Dotnet.Chroma.Repositories.Models.Settings
{
    /// <summary>
    /// Chroma basic / required settings;
    /// Inject this from your app settings using the StartupExtensions (create a section in your appsettings.json with this exact name 'ChromaSettings' with the desired values
    /// </summary>
    public class ChromaSettings
    {
        public ChromaSettings() { }

        public ChromaSettings(string url) { ServerUrl = url; }
        public ChromaSettings(string model, int dimensions) { EmbeddingModel = model; EmbeddingDimensions = dimensions; }
        public ChromaSettings(string url, string model, int dimensions) : this(model, dimensions) { ServerUrl = url; }
        public string ServerUrl { get; set; } = "http://localhost:8000";
        public string Tenant { get; set; } = "default_tenant";
        public string Database { get; set; } = "default_database";
        public string EmbeddingModel { get; set; } = "nomic-embed-text";
        public int EmbeddingDimensions { get; set; } = 512;
        public HnswSettings HnswSettings { get; set; } = new HnswSettings();
    }
}
