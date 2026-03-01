using System.Text.Json.Serialization;


namespace BMC.Rebrickable.Api.Models.Responses
{
    /// <summary>
    /// Rebrickable set definition from /lego/sets/.
    /// </summary>
    public class RebrickableSet
    {
        [JsonPropertyName("set_num")]
        public string SetNum { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("theme_id")]
        public int ThemeId { get; set; }

        [JsonPropertyName("num_parts")]
        public int NumParts { get; set; }

        [JsonPropertyName("set_img_url")]
        public string SetImgUrl { get; set; }

        [JsonPropertyName("set_url")]
        public string SetUrl { get; set; }

        [JsonPropertyName("last_modified_dt")]
        public string LastModifiedDt { get; set; }
    }
}
