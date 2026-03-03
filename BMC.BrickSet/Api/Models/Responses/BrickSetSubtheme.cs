using System.Text.Json.Serialization;


namespace BMC.BrickSet.Api.Models.Responses
{
    /// <summary>
    /// A subtheme within a LEGO theme from BrickSet.
    /// </summary>
    public class BrickSetSubtheme
    {
        [JsonPropertyName("theme")]
        public string Theme { get; set; }

        [JsonPropertyName("subtheme")]
        public string Subtheme { get; set; }

        [JsonPropertyName("setCount")]
        public int SetCount { get; set; }

        [JsonPropertyName("yearFrom")]
        public int YearFrom { get; set; }

        [JsonPropertyName("yearTo")]
        public int YearTo { get; set; }
    }


    /// <summary>
    /// Response from getSubthemes.
    /// </summary>
    public class BrickSetSubthemesResponse : BrickSetApiResponse
    {
        [JsonPropertyName("subthemes")]
        public System.Collections.Generic.List<BrickSetSubtheme> Subthemes { get; set; }
    }
}
