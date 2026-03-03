using System.Text.Json.Serialization;


namespace BMC.BrickEconomy.Api.Models.Responses
{
    //
    // BrickEconomy Sales Ledger
    //
    // Returned by GET /api/v1/salesledger
    //
    // AI-Developed — This file was significantly developed with AI assistance.
    //

    /// <summary>
    /// A sales ledger entry from BrickEconomy — tracks bought/sold transactions.
    /// </summary>
    public class BrickEconomySalesLedgerEntry
    {
        [JsonPropertyName("item_number")]
        public string ItemNumber { get; set; }

        [JsonPropertyName("item_type")]
        public string ItemType { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("price")]
        public decimal? Price { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("condition")]
        public string Condition { get; set; }

        [JsonPropertyName("notes")]
        public string Notes { get; set; }
    }
}
