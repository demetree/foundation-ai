using System;
using System.Net;


namespace BMC.Rebrickable.Api
{
    /// <summary>
    /// Exception thrown when the Rebrickable API returns a non-success status code.
    /// </summary>
    public class RebrickableApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string ReasonPhrase { get; }
        public string ResponseBody { get; }


        public RebrickableApiException(HttpStatusCode statusCode, string reasonPhrase, string responseBody)
            : base($"Rebrickable API error {(int)statusCode} {reasonPhrase}: {responseBody}")
        {
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
            ResponseBody = responseBody;
        }
    }
}
