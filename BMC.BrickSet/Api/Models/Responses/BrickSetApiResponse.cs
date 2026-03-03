using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace BMC.BrickSet.Api.Models.Responses
{
    /// <summary>
    /// Generic wrapper for BrickSet API v3 responses.
    /// 
    /// All BrickSet endpoints return:
    ///   { "status": "success|error", "matches": N, "[collection]": [...] }
    /// 
    /// The collection property name varies per endpoint (e.g. "sets", "themes", "reviews").
    /// Individual response classes inherit from this to provide the typed collection.
    /// </summary>
    public class BrickSetApiResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("matches")]
        public int Matches { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        /// <summary>Whether the API call succeeded.</summary>
        public bool IsSuccess => Status == "success";
    }


    /// <summary>
    /// Response wrapper for endpoints that return a list of items.
    /// </summary>
    public class BrickSetListResponse<T> : BrickSetApiResponse
    {
        /// <summary>
        /// The results collection.  The JSON property name varies by endpoint
        /// (e.g. "sets", "themes"), so individual typed responses override this
        /// with the correct [JsonPropertyName].
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();
    }


    /// <summary>
    /// Response from the checkKey endpoint.
    /// </summary>
    public class BrickSetCheckKeyResponse : BrickSetApiResponse
    {
    }


    /// <summary>
    /// Response from the login endpoint.
    /// </summary>
    public class BrickSetLoginResponse : BrickSetApiResponse
    {
        [JsonPropertyName("hash")]
        public string Hash { get; set; }
    }


    /// <summary>
    /// Response from the checkUserHash endpoint.
    /// </summary>
    public class BrickSetCheckUserHashResponse : BrickSetApiResponse
    {
    }
}
