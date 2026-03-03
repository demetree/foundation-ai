using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace BMC.BrickLink.Api.Models.Responses
{
    //
    // BrickLink Subset and Superset entries
    //
    // Returned by GET /items/{type}/{no}/subsets (part-out) and
    //              GET /items/{type}/{no}/supersets (find containing sets)
    //
    // AI-Developed — This file was significantly developed with AI assistance.
    //

    /// <summary>
    /// A superset entry — an item that contains the queried item.
    /// </summary>
    public class BrickLinkSupersetEntry
    {
        [JsonPropertyName("color_id")]
        public int ColorId { get; set; }

        [JsonPropertyName("entries")]
        public List<BrickLinkSupersetItem> Entries { get; set; }
    }


    /// <summary>
    /// Individual item within a superset entry.
    /// </summary>
    public class BrickLinkSupersetItem
    {
        [JsonPropertyName("item")]
        public BrickLinkItem Item { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("appears_as")]
        public string AppearsAs { get; set; }
    }


    /// <summary>
    /// A subset entry — an item that is contained within the queried item.
    /// Subset responses come grouped by match number (for alternate parts).
    /// </summary>
    public class BrickLinkSubsetEntry
    {
        [JsonPropertyName("match_no")]
        public int MatchNo { get; set; }

        [JsonPropertyName("entries")]
        public List<BrickLinkSubsetItem> Entries { get; set; }
    }


    /// <summary>
    /// Individual item within a subset entry.
    /// </summary>
    public class BrickLinkSubsetItem
    {
        [JsonPropertyName("item")]
        public BrickLinkItem Item { get; set; }

        [JsonPropertyName("color_id")]
        public int ColorId { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("extra_quantity")]
        public int ExtraQuantity { get; set; }

        [JsonPropertyName("is_alternate")]
        public bool IsAlternate { get; set; }
    }
}
