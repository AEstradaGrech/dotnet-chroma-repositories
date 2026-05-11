using Dotnet.Chroma.Repositories.Models.Metadata;
using System.Text.Json;

namespace Dotnet.Chroma.Repositories.Models
{
    /// <summary>
    /// Basic INSTANTIABLE representation of a chroma chunk.
    /// Can be converted to different extended classes with the '.As<T>() method.
    /// This will recreate the chunk again from the existing data and reserialize the metadata class to the new type.
    /// </summary>
    public class ChromaChunk : ChromaModel
    {
        public ChromaChunk() : base() 
        {
            Text = string.Empty;
        }

        /// <summary>
        /// Constructor for BASIC chunks (ignores text and metadata). Usually for DefaultChunks (NO ID YET)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="metadata"></param>
        public ChromaChunk(string id, Dictionary<string, object> metadata) : base(id, metadata) { Text = string.Empty; }

        /// <summary>
        /// REQUIRED constructor for Chroma Chunks (chunks already stored in DB)
        /// Recieves Chroma data and constructs the appropriate chunk type according to the Metadata dictionary values.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="document"></param>
        /// <param name="embedding"></param>
        /// <param name="metadata"></param>
        public ChromaChunk(string id, string document, ReadOnlyMemory<float> embedding, Dictionary<string, object> metadata) : base(id, metadata) { Text = document; Embedding = embedding; }

        public string Text { get; set; } //Document
        public ReadOnlyMemory<float> Embedding { get; set; }

        protected override void setDefaultMetadata()
        {
            DefaultMetadata = JsonSerializer.Deserialize<ChromaMetadata>(JsonSerializer.Serialize(Metadata));
        }
        /// <summary>
        /// Recreates the chunk again from the existing data and reserialize the metadata class to the new type.
        /// Useful to reshape ChromaQueryChunks from a similarity search according to their assigned type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T As<T>() where T : ChromaChunk
            => Activator.CreateInstance(typeof(T), Id, Text, Embedding, Metadata) as T;
    }
}
