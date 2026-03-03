using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace BMC.BrickLink.Api.Models.Responses
{
    //
    // BrickLink API Response Models
    //
    // BrickLink wraps all responses in a standard envelope:
    //   {
    //     "meta": { "description": "...", "message": "...", "code": 200 },
    //     "data": { ... } or [ ... ]
    //   }
    //
    // The "meta" block indicates success/failure, and "data" contains the payload.
    //
    // AI-Developed — This file was significantly developed with AI assistance.
    //


    /// <summary>
    /// The "meta" portion of every BrickLink API response.
    /// Contains status code, description, and optional error message.
    /// </summary>
    public class BrickLinkMeta
    {
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }
    }


    /// <summary>
    /// Generic envelope for BrickLink API responses that return a single object.
    /// </summary>
    public class BrickLinkResponse<T>
    {
        [JsonPropertyName("meta")]
        public BrickLinkMeta Meta { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }

        /// <summary>Whether the API call succeeded (HTTP 200 equivalent in meta.code).</summary>
        public bool IsSuccess => Meta != null && Meta.Code >= 200 && Meta.Code < 300;
    }


    /// <summary>
    /// Generic envelope for BrickLink API responses that return a list of objects.
    /// </summary>
    public class BrickLinkListResponse<T>
    {
        [JsonPropertyName("meta")]
        public BrickLinkMeta Meta { get; set; }

        [JsonPropertyName("data")]
        public List<T> Data { get; set; }

        /// <summary>Whether the API call succeeded.</summary>
        public bool IsSuccess => Meta != null && Meta.Code >= 200 && Meta.Code < 300;
    }
}
