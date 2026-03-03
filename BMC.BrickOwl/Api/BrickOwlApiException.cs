using System;
using System.Net;


namespace BMC.BrickOwl.Api
{
    /// <summary>
    /// Exception thrown when the Brick Owl API returns an error status or non-success HTTP status code.
    /// </summary>
    public class BrickOwlApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string ReasonPhrase { get; }
        public string ResponseBody { get; }


        /// <summary>
        /// Constructor for HTTP-level errors (non-2xx status codes).
        /// </summary>
        public BrickOwlApiException(HttpStatusCode statusCode, string reasonPhrase, string responseBody)
            : base($"Brick Owl API error {(int)statusCode} {reasonPhrase}: {responseBody}")
        {
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
            ResponseBody = responseBody;
        }


        /// <summary>
        /// Constructor for API-level errors.
        /// </summary>
        public BrickOwlApiException(string apiErrorMessage)
            : base($"Brick Owl API error: {apiErrorMessage}")
        {
            StatusCode = HttpStatusCode.OK;
            ReasonPhrase = "API Error";
            ResponseBody = apiErrorMessage;
        }
    }
}
