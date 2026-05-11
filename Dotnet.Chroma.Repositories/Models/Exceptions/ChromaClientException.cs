using System.Net;

namespace Dotnet.Chroma.Repositories.Models.Exceptions
{
    /// <summary>
    /// Exception to catch inner chroma errors returned by the IChromaClient
    /// </summary>
    public class ChromaClientException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        public ChromaClientException(string message) : base(message) 
        {
            StatusCode = HttpStatusCode.BadRequest;
        }

        public ChromaClientException(HttpStatusCode code, string message) : base(message)
        {
            StatusCode = code;
        }
    }
}
