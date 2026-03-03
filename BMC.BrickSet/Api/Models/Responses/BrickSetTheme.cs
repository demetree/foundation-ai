using System.Text.Json.Serialization;


namespace BMC.BrickSet.Api.Models.Responses
{
    /// <summary>
    /// A LEGO theme from BrickSet.
    /// </summary>
    public class BrickSetTheme
    {
        [JsonPropertyName("theme")]
        public string Theme { get; set; }

        [JsonPropertyName("setCount")]
        public int SetCount { get; set; }

        [JsonPropertyName("subthemeCount")]
        public int SubthemeCount { get; set; }

        [JsonPropertyName("yearFrom")]
        public int YearFrom { get; set; }

        [JsonPropertyName("yearTo")]
        public int YearTo { get; set; }
    }


    /// <summary>
    /// Response from getThemes.
    /// </summary>
    public class BrickSetThemesResponse : BrickSetApiResponse
    {
        [JsonPropertyName("themes")]
        public System.Collections.Generic.List<BrickSetTheme> Themes { get; set; }
    }
}
