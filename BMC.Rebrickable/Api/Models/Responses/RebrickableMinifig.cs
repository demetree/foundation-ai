using System.Text.Json.Serialization;


namespace BMC.Rebrickable.Api.Models.Responses
{
    /// <summary>
    /// Rebrickable minifig definition from /lego/minifigs/.
    /// </summary>
    public class RebrickableMinifig
    {
        [JsonPropertyName("set_num")]
        public string SetNum { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("num_parts")]
        public int NumParts { get; set; }

        [JsonPropertyName("set_img_url")]
        public string SetImgUrl { get; set; }

        [JsonPropertyName("set_url")]
        public string SetUrl { get; set; }
    }
}
