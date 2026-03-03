using System.Text.Json.Serialization;


namespace BMC.BrickSet.Api.Models.Responses
{
    /// <summary>
    /// Year data for a theme from BrickSet.
    /// </summary>
    public class BrickSetYear
    {
        [JsonPropertyName("theme")]
        public string Theme { get; set; }

        [JsonPropertyName("year")]
        public string Year { get; set; }

        [JsonPropertyName("setCount")]
        public int SetCount { get; set; }
    }


    /// <summary>
    /// Response from getYears.
    /// </summary>
    public class BrickSetYearsResponse : BrickSetApiResponse
    {
        [JsonPropertyName("years")]
        public System.Collections.Generic.List<BrickSetYear> Years { get; set; }
    }
}
