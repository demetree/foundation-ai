using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace BMC.BrickOwl.Api.Models.Responses
{
    //
    // Brick Owl Order Models
    //
    // Returned by GET /order/list, /order/view, /order/items
    //
    // AI-Developed — This file was significantly developed with AI assistance.
    //

    /// <summary>
    /// An order from the user's Brick Owl store.
    /// </summary>
    public class BrickOwlOrder
    {
        [JsonPropertyName("order_id")]
        public string OrderId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("order_date")]
        public string OrderDate { get; set; }

        [JsonPropertyName("total_quantity")]
        public int? TotalQuantity { get; set; }

        [JsonPropertyName("total_lots")]
        public int? TotalLots { get; set; }

        [JsonPropertyName("sub_total")]
        public decimal? SubTotal { get; set; }

        [JsonPropertyName("shipping")]
        public decimal? Shipping { get; set; }

        [JsonPropertyName("grand_total")]
        public decimal? GrandTotal { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("buyer_name")]
        public string BuyerName { get; set; }

        [JsonPropertyName("ship_method")]
        public string ShipMethod { get; set; }

        [JsonPropertyName("tracking_number")]
        public string TrackingNumber { get; set; }

        [JsonPropertyName("payment_method")]
        public string PaymentMethod { get; set; }

        [JsonPropertyName("payment_status")]
        public string PaymentStatus { get; set; }

        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; }
    }


    /// <summary>
    /// An individual item within an order.
    /// </summary>
    public class BrickOwlOrderItem
    {
        [JsonPropertyName("order_item_id")]
        public string OrderItemId { get; set; }

        [JsonPropertyName("boid")]
        public string Boid { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("color_id")]
        public int? ColorId { get; set; }

        [JsonPropertyName("color_name")]
        public string ColorName { get; set; }

        [JsonPropertyName("qty")]
        public int Quantity { get; set; }

        [JsonPropertyName("price")]
        public decimal? Price { get; set; }

        [JsonPropertyName("condition")]
        public string Condition { get; set; }

        [JsonPropertyName("image_small")]
        public string ImageSmall { get; set; }
    }
}
