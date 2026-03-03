using System.Text.Json.Serialization;


namespace BMC.BrickLink.Api.Models.Responses
{
    //
    // BrickLink Known Color
    //
    // Returned by GET /items/{type}/{no}/colors
    //
    // AI-Developed — This file was significantly developed with AI assistance.
    //

    /// <summary>
    /// A color that a specific part/item is known to exist in.
    /// </summary>
    public class BrickLinkKnownColor
    {
        [JsonPropertyName("color_id")]
        public int ColorId { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }
}
