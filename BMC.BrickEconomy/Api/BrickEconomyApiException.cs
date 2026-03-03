using System;
using System.Net;


namespace BMC.BrickEconomy.Api
{
    /// <summary>
    /// Exception thrown when the BrickEconomy API returns an error status or non-success HTTP status code.
    /// </summary>
    public class BrickEconomyApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string ReasonPhrase { get; }
        public string ResponseBody { get; }


        /// <summary>
        /// Constructor for HTTP-level errors (non-2xx status codes).
        /// </summary>
        public BrickEconomyApiException(HttpStatusCode statusCode, string reasonPhrase, string responseBody)
            : base($"BrickEconomy API error {(int)statusCode} {reasonPhrase}: {responseBody}")
        {
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
            ResponseBody = responseBody;
        }


        /// <summary>
        /// Constructor for API-level errors (HTTP 200 but error in response body).
        /// </summary>
        public BrickEconomyApiException(string apiErrorMessage)
            : base($"BrickEconomy API error: {apiErrorMessage}")
        {
            StatusCode = HttpStatusCode.OK;
            ReasonPhrase = "API Error";
            ResponseBody = apiErrorMessage;
        }
    }
}
