using System.Text.Json.Serialization;


namespace BMC.Rebrickable.Api.Models.Responses
{
    /// <summary>
    /// Rebrickable part category from /lego/part_categories/.
    /// </summary>
    public class RebrickablePartCategory
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("part_count")]
        public int PartCount { get; set; }
    }
}
