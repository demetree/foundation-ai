using System.Text.Json.Serialization;


namespace BMC.BrickLink.Api.Models.Responses
{
    //
    // BrickLink Catalog Item
    //
    // Represents a single item in the BrickLink catalog.
    // Returned by GET /items/{type}/{no}
    //
    // Item types: MINIFIG, PART, SET, BOOK, GEAR, CATALOG, INSTRUCTION,
    //             UNSORTED_LOT, ORIGINAL_BOX
    //
    // AI-Developed — This file was significantly developed with AI assistance.
    //

    /// <summary>
    /// A catalog item from BrickLink (set, part, minifig, etc.).
    /// </summary>
    public class BrickLinkItem
    {
        [JsonPropertyName("no")]
        public string Number { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("category_id")]
        public int CategoryId { get; set; }

        [JsonPropertyName("alternate_no")]
        public string AlternateNumber { get; set; }

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; }

        [JsonPropertyName("thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [JsonPropertyName("weight")]
        public string Weight { get; set; }

        [JsonPropertyName("dim_x")]
        public string DimensionX { get; set; }

        [JsonPropertyName("dim_y")]
        public string DimensionY { get; set; }

        [JsonPropertyName("dim_z")]
        public string DimensionZ { get; set; }

        [JsonPropertyName("year_released")]
        public int YearReleased { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("is_obsolete")]
        public bool IsObsolete { get; set; }

        [JsonPropertyName("language_code")]
        public string LanguageCode { get; set; }
    }
}
