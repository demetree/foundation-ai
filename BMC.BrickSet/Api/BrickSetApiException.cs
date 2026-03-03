using System;
using System.Net;


namespace BMC.BrickSet.Api
{
    /// <summary>
    /// Exception thrown when the BrickSet API returns an error status or non-success HTTP status code.
    /// </summary>
    public class BrickSetApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string ReasonPhrase { get; }
        public string ResponseBody { get; }


        public BrickSetApiException(HttpStatusCode statusCode, string reasonPhrase, string responseBody)
            : base($"BrickSet API error {(int)statusCode} {reasonPhrase}: {responseBody}")
        {
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
            ResponseBody = responseBody;
        }


        /// <summary>
        /// Constructor for BrickSet API-level errors (HTTP 200 but status != "success").
        /// </summary>
        public BrickSetApiException(string apiErrorMessage)
            : base($"BrickSet API error: {apiErrorMessage}")
        {
            StatusCode = HttpStatusCode.OK;
            ReasonPhrase = "API Error";
            ResponseBody = apiErrorMessage;
        }
    }
}
