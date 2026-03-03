using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace BMC.BrickEconomy.Api.Models.Responses
{
    //
    // BrickEconomy Set Valuation
    //
    // Returned by GET /api/v1/set/{set_number}
    //
    // The crown jewel — AI/ML-powered valuation with growth tracking and forecasts.
    //
    // AI-Developed — This file was significantly developed with AI assistance.
    //

    /// <summary>
    /// Detailed set valuation data from BrickEconomy.
    /// Includes retail pricing, current market value, growth metrics,
    /// and AI-powered forecast values.
    /// </summary>
    public class BrickEconomySet
    {
        [JsonPropertyName("set_number")]
        public string SetNumber { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("theme")]
        public string Theme { get; set; }

        [JsonPropertyName("subtheme")]
        public string Subtheme { get; set; }

        [JsonPropertyName("pieces")]
        public int? Pieces { get; set; }

        [JsonPropertyName("minifigs")]
        public int? Minifigs { get; set; }

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; }

        [JsonPropertyName("brickeconomy_url")]
        public string BrickEconomyUrl { get; set; }


        // ─── Pricing ───

        [JsonPropertyName("retail_price")]
        public decimal? RetailPrice { get; set; }

        [JsonPropertyName("current_value")]
        public decimal? CurrentValue { get; set; }

        [JsonPropertyName("forecast_value")]
        public decimal? ForecastValue { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }


        // ─── Growth Metrics ───

        [JsonPropertyName("growth_percentage")]
        public decimal? GrowthPercentage { get; set; }

        [JsonPropertyName("annual_growth")]
        public decimal? AnnualGrowth { get; set; }

        [JsonPropertyName("value_new")]
        public decimal? ValueNew { get; set; }

        [JsonPropertyName("value_used")]
        public decimal? ValueUsed { get; set; }


        // ─── Availability ───

        [JsonPropertyName("availability")]
        public string Availability { get; set; }

        [JsonPropertyName("retired")]
        public bool? Retired { get; set; }


        // ─── Price History ───

        [JsonPropertyName("price_events")]
        public List<BrickEconomyPriceEvent> PriceEvents { get; set; }
    }


    /// <summary>
    /// A historical price event (e.g. price change, retirement date).
    /// </summary>
    public class BrickEconomyPriceEvent
    {
        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("event")]
        public string Event { get; set; }

        [JsonPropertyName("price")]
        public decimal? Price { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }
    }
}
