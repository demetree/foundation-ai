using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace BMC.BrickLink.Api.Models.Responses
{
    //
    // BrickLink Price Guide
    //
    // The price guide is the crown jewel of the BrickLink API — real secondary market
    // pricing data that no other LEGO service provides.
    //
    // Returned by GET /items/{type}/{no}/price
    //
    // Returns min/max/average/quantity pricing for both new and used conditions,
    // separated into "stock" (currently for sale) and "sold" (last 6 months) data.
    //
    // AI-Developed — This file was significantly developed with AI assistance.
    //

    /// <summary>
    /// Price guide data from BrickLink for a specific item + color + condition combination.
    /// </summary>
    public class BrickLinkPriceGuide
    {
        [JsonPropertyName("item")]
        public BrickLinkPriceGuideItem Item { get; set; }

        [JsonPropertyName("new_or_used")]
        public string NewOrUsed { get; set; }

        [JsonPropertyName("currency_code")]
        public string CurrencyCode { get; set; }

        [JsonPropertyName("min_price")]
        public string MinPrice { get; set; }

        [JsonPropertyName("max_price")]
        public string MaxPrice { get; set; }

        [JsonPropertyName("avg_price")]
        public string AvgPrice { get; set; }

        [JsonPropertyName("qty_avg_price")]
        public string QtyAvgPrice { get; set; }

        [JsonPropertyName("unit_quantity")]
        public int UnitQuantity { get; set; }

        [JsonPropertyName("total_quantity")]
        public int TotalQuantity { get; set; }

        [JsonPropertyName("price_detail")]
        public List<BrickLinkPriceDetail> PriceDetail { get; set; }
    }


    /// <summary>
    /// Minimal item reference within a price guide response.
    /// </summary>
    public class BrickLinkPriceGuideItem
    {
        [JsonPropertyName("no")]
        public string Number { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }


    /// <summary>
    /// Individual price point from a specific seller/sale in the price guide.
    /// </summary>
    public class BrickLinkPriceDetail
    {
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("unit_price")]
        public string UnitPrice { get; set; }

        [JsonPropertyName("shipping_available")]
        public bool ShippingAvailable { get; set; }

        [JsonPropertyName("seller_country_code")]
        public string SellerCountryCode { get; set; }

        [JsonPropertyName("buyer_country_code")]
        public string BuyerCountryCode { get; set; }

        [JsonPropertyName("date_ordered")]
        public string DateOrdered { get; set; }
    }
}
