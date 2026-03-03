using System.Text.Json.Serialization;


namespace BMC.BrickEconomy.Api.Models.Responses
{
    //
    // BrickEconomy Collection Item
    //
    // Returned by GET /api/v1/collection/sets and GET /api/v1/collection/minifigs
    //
    // These represent items in a user's personal BrickEconomy collection,
    // with purchase price, condition, and current valuation.
    //
    // AI-Developed — This file was significantly developed with AI assistance.
    //

    /// <summary>
    /// A set in the user's BrickEconomy collection, including purchase info and current valuation.
    /// </summary>
    public class BrickEconomyCollectionSet
    {
        [JsonPropertyName("set_number")]
        public string SetNumber { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("theme")]
        public string Theme { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; }


        // ─── Purchase Info ───

        [JsonPropertyName("paid_price")]
        public decimal? PaidPrice { get; set; }

        [JsonPropertyName("condition")]
        public string Condition { get; set; }

        [JsonPropertyName("quantity")]
        public int? Quantity { get; set; }

        [JsonPropertyName("notes")]
        public string Notes { get; set; }


        // ─── Valuation ───

        [JsonPropertyName("current_value")]
        public decimal? CurrentValue { get; set; }

        [JsonPropertyName("retail_price")]
        public decimal? RetailPrice { get; set; }

        [JsonPropertyName("growth_percentage")]
        public decimal? GrowthPercentage { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }
    }


    /// <summary>
    /// A minifig in the user's BrickEconomy collection.
    /// </summary>
    public class BrickEconomyCollectionMinifig
    {
        [JsonPropertyName("minifig_number")]
        public string MinifigNumber { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("theme")]
        public string Theme { get; set; }

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; }


        // ─── Purchase Info ───

        [JsonPropertyName("paid_price")]
        public decimal? PaidPrice { get; set; }

        [JsonPropertyName("condition")]
        public string Condition { get; set; }

        [JsonPropertyName("quantity")]
        public int? Quantity { get; set; }

        [JsonPropertyName("notes")]
        public string Notes { get; set; }


        // ─── Valuation ───

        [JsonPropertyName("current_value")]
        public decimal? CurrentValue { get; set; }

        [JsonPropertyName("growth_percentage")]
        public decimal? GrowthPercentage { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }
    }
}
