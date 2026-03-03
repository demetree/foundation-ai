using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace BMC.BrickEconomy.Api.Models.Responses
{
    //
    // BrickEconomy API Response Models
    //
    // BrickEconomy wraps all responses in a { "data": {...} } envelope.
    // The API uses simple API key auth via x-apikey header.
    //
    // Rate limit: 100 requests/day (Premium membership required)
    //
    // AI-Developed — This file was significantly developed with AI assistance.
    //


    /// <summary>
    /// Generic envelope for BrickEconomy API responses that return a single object.
    /// </summary>
    public class BrickEconomyResponse<T>
    {
        [JsonPropertyName("data")]
        public T Data { get; set; }
    }


    /// <summary>
    /// Generic envelope for BrickEconomy API responses that return a list.
    /// </summary>
    public class BrickEconomyListResponse<T>
    {
        [JsonPropertyName("data")]
        public List<T> Data { get; set; }
    }
}
