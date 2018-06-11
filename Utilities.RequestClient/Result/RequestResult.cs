using System;
using System.Net;

namespace Utilities.RequestClient.Result
{
    public class RequestResult
    {
        public Exception Exception { get; set; }
        public string ExceptionDetail { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}
