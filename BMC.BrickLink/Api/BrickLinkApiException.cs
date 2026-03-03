using System;
using System.Net;


namespace BMC.BrickLink.Api
{
    /// <summary>
    /// Exception thrown when the BrickLink API returns an error status or non-success HTTP status code.
    /// </summary>
    public class BrickLinkApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string ReasonPhrase { get; }
        public string ResponseBody { get; }


        /// <summary>
        /// Constructor for HTTP-level errors (non-2xx status codes).
        /// </summary>
        public BrickLinkApiException(HttpStatusCode statusCode, string reasonPhrase, string responseBody)
            : base($"BrickLink API error {(int)statusCode} {reasonPhrase}: {responseBody}")
        {
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
            ResponseBody = responseBody;
        }


        /// <summary>
        /// Constructor for BrickLink API-level errors (HTTP 200 but error in response body).
        /// </summary>
        public BrickLinkApiException(string apiErrorMessage)
            : base($"BrickLink API error: {apiErrorMessage}")
        {
            StatusCode = HttpStatusCode.OK;
            ReasonPhrase = "API Error";
            ResponseBody = apiErrorMessage;
        }
    }
}
