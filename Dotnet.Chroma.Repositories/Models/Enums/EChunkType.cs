
namespace Dotnet.Chroma.Repositories.Models.Enums
{
    /// <summary>
    /// Required by the framework. Helps to differentiate Collections (groupping units) from chunks (data)
    /// It is used also as metadata filter
    /// </summary>
    public enum EChunkType
    {
        COLLECTION = 0,
        DOCUMENT = 1
    }
}
