using System.Text.Json.Serialization;


namespace BMC.BrickSet.Api.Models.Responses
{
    /// <summary>
    /// API key usage statistics from BrickSet.
    /// Used to track daily call quota and avoid rate limit breaches.
    /// </summary>
    public class BrickSetKeyUsageStat
    {
        [JsonPropertyName("dateStamp")]
        public string DateStamp { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }


    /// <summary>
    /// Response from getKeyUsageStats.
    /// </summary>
    public class BrickSetKeyUsageStatsResponse : BrickSetApiResponse
    {
        [JsonPropertyName("apiKeyUsage")]
        public System.Collections.Generic.List<BrickSetKeyUsageStat> ApiKeyUsage { get; set; }
    }
}
