using Dotnet.Chroma.Repositories.Models.Enums;
using Dotnet.Chroma.Repositories.Models.Metadata;
using System.Text.Json;

namespace Dotnet.Chroma.Repositories.Models
{
    /// <summary>
    /// Represents a collection of chunks that may have been generated from different files and hold summary data
    /// It also stores the MODEL and DIMENSIONS for all the collection(the query embedding and the embedded document must match this fields)
    ///  
    /// It has no embedding since it does not hold any relevant data for a similarity search,
    /// it's purpose is to group chunks and store common data (like total number of chunks or embedding params for files)
    /// 
    /// It is always the first created chunk of a collectio(ID = 0) but reshaped as a collection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChromaChunksCollection<T> : ChromaModel where T : ChromaChunk
    {
        public ChromaChunksCollection() : base() { Type = (int)EChunkType.COLLECTION; }
        public ChromaChunksCollection(string id, Dictionary<string, object> metadata) : base(id, metadata) { }
        public ChromaChunksCollection(string id, string name, string description, Dictionary<string, object> metadata) : base(id, metadata)
        {
            Name = name;
            Description = description;
            Chunks = new List<T>();
        }
        public ChromaChunksCollection(string id, string name, string description, Dictionary<string, object> metadata, List<T> chunks) : this(id, name, description, metadata)
        {
            Chunks = chunks.OrderBy(x => int.Parse(x.Id)).ToList();
        }
        
        /// <summary>
        /// Collection DB NAME
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Collection Description mapped from Chunk 0 Text
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A sample of collection CHUNKS. Defaults to empty
        /// </summary>
        public List<T> Chunks { get; set; }

        protected override void setDefaultMetadata()
        {
            DefaultMetadata = JsonSerializer.Deserialize<ChromaCollectionMetadata>(JsonSerializer.Serialize(Metadata));
        }
    }
}
