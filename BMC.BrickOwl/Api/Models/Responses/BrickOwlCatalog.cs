using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace BMC.BrickOwl.Api.Models.Responses
{
    //
    // Brick Owl Catalog Models
    //
    // Returned by GET /catalog/lookup, /catalog/id_lookup, /catalog/availability
    //
    // AI-Developed — This file was significantly developed with AI assistance.
    //

    /// <summary>
    /// Catalog item from Brick Owl — returned by /catalog/lookup.
    /// Contains basic item metadata using the BOID (Brick Owl ID) system.
    /// </summary>
    public class BrickOwlCatalogItem
    {
        [JsonPropertyName("boid")]
        public string Boid { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("category_name")]
        public string CategoryName { get; set; }

        [JsonPropertyName("category_id")]
        public string CategoryId { get; set; }

        [JsonPropertyName("weight")]
        public string Weight { get; set; }

        [JsonPropertyName("dimensions")]
        public string Dimensions { get; set; }

        [JsonPropertyName("design_id")]
        public string DesignId { get; set; }

        [JsonPropertyName("image_small")]
        public string ImageSmall { get; set; }

        [JsonPropertyName("image_large")]
        public string ImageLarge { get; set; }

        [JsonPropertyName("external_ids")]
        public Dictionary<string, List<string>> ExternalIds { get; set; }

        [JsonPropertyName("year_released")]
        public string YearReleased { get; set; }

        [JsonPropertyName("piece_count")]
        public int? PieceCount { get; set; }

        [JsonPropertyName("minifig_count")]
        public int? MinifigCount { get; set; }

        [JsonPropertyName("instructions")]
        public string Instructions { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }


    /// <summary>
    /// ID lookup result — maps external IDs to Brick Owl BOIDs.
    /// Returned by /catalog/id_lookup.
    /// </summary>
    public class BrickOwlIdLookupResult
    {
        [JsonPropertyName("boids")]
        public List<string> Boids { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }


    /// <summary>
    /// Availability/pricing info for an item.
    /// Returned by /catalog/availability (requires approval).
    /// </summary>
    public class BrickOwlAvailability
    {
        [JsonPropertyName("boid")]
        public string Boid { get; set; }

        [JsonPropertyName("qty")]
        public int? Quantity { get; set; }

        [JsonPropertyName("store_count")]
        public int? StoreCount { get; set; }

        [JsonPropertyName("min_price")]
        public decimal? MinPrice { get; set; }

        [JsonPropertyName("avg_price")]
        public decimal? AvgPrice { get; set; }

        [JsonPropertyName("max_price")]
        public decimal? MaxPrice { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }
    }
}
