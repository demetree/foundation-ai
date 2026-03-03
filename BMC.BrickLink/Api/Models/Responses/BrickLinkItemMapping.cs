using System.Text.Json.Serialization;


namespace BMC.BrickLink.Api.Models.Responses
{
    //
    // BrickLink Item Mapping
    //
    // Returned by GET /item_mapping/{id}
    //
    // Maps between BrickLink item numbers and LEGO Element IDs (Pick-a-Brick codes).
    //
    // AI-Developed — This file was significantly developed with AI assistance.
    //

    /// <summary>
    /// Mapping between a BrickLink catalog item and a LEGO Element ID.
    /// </summary>
    public class BrickLinkItemMapping
    {
        [JsonPropertyName("item")]
        public BrickLinkItemMappingItem Item { get; set; }

        [JsonPropertyName("color_id")]
        public int ColorId { get; set; }

        [JsonPropertyName("color_name")]
        public string ColorName { get; set; }

        [JsonPropertyName("element_id")]
        public string ElementId { get; set; }
    }


    /// <summary>
    /// Minimal item reference within an item mapping response.
    /// </summary>
    public class BrickLinkItemMappingItem
    {
        [JsonPropertyName("no")]
        public string Number { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
