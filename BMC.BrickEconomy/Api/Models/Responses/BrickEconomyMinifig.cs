using System.Text.Json.Serialization;


namespace BMC.BrickEconomy.Api.Models.Responses
{
    //
    // BrickEconomy Minifig Valuation
    //
    // Returned by GET /api/v1/minifig/{minifig_id}
    //
    // AI-Developed — This file was significantly developed with AI assistance.
    //

    /// <summary>
    /// Minifig valuation data from BrickEconomy.
    /// </summary>
    public class BrickEconomyMinifig
    {
        [JsonPropertyName("minifig_number")]
        public string MinifigNumber { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("theme")]
        public string Theme { get; set; }

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; }

        [JsonPropertyName("brickeconomy_url")]
        public string BrickEconomyUrl { get; set; }

        [JsonPropertyName("year_released")]
        public int? YearReleased { get; set; }

        [JsonPropertyName("set_count")]
        public int? SetCount { get; set; }


        // ─── Valuation ───

        [JsonPropertyName("current_value")]
        public decimal? CurrentValue { get; set; }

        [JsonPropertyName("forecast_value")]
        public decimal? ForecastValue { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("growth_percentage")]
        public decimal? GrowthPercentage { get; set; }

        [JsonPropertyName("annual_growth")]
        public decimal? AnnualGrowth { get; set; }
    }
}
