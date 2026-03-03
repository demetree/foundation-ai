using System.Text.Json.Serialization;


namespace BMC.BrickLink.Api.Models.Responses
{
    //
    // BrickLink Color
    //
    // Returned by GET /colors and GET /colors/{id}
    //
    // AI-Developed — This file was significantly developed with AI assistance.
    //

    /// <summary>
    /// A BrickLink color definition.
    /// </summary>
    public class BrickLinkColor
    {
        [JsonPropertyName("color_id")]
        public int ColorId { get; set; }

        [JsonPropertyName("color_name")]
        public string ColorName { get; set; }

        [JsonPropertyName("color_code")]
        public string ColorCode { get; set; }

        [JsonPropertyName("color_type")]
        public string ColorType { get; set; }
    }
}
