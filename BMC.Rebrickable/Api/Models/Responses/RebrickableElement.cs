using System.Text.Json.Serialization;


namespace BMC.Rebrickable.Api.Models.Responses
{
    /// <summary>
    /// Rebrickable element (part+colour combination) from /lego/elements/{element_id}/.
    /// </summary>
    public class RebrickableElement
    {
        [JsonPropertyName("element_id")]
        public string ElementId { get; set; }

        [JsonPropertyName("part")]
        public RebrickablePart Part { get; set; }

        [JsonPropertyName("color")]
        public RebrickableColor Color { get; set; }

        [JsonPropertyName("element_img_url")]
        public string ElementImgUrl { get; set; }

        [JsonPropertyName("design_id")]
        public string DesignId { get; set; }
    }
}
