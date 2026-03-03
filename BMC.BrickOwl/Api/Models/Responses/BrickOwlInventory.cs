using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace BMC.BrickOwl.Api.Models.Responses
{
    //
    // Brick Owl Inventory & Collection Models
    //
    // Inventory: GET /inventory/list (store inventory)
    // Collection: GET /collection/lots (personal collection)
    //
    // AI-Developed — This file was significantly developed with AI assistance.
    //

    /// <summary>
    /// A lot in the user's store inventory.
    /// </summary>
    public class BrickOwlInventoryLot
    {
        [JsonPropertyName("lot_id")]
        public string LotId { get; set; }

        [JsonPropertyName("boid")]
        public string Boid { get; set; }

        [JsonPropertyName("color_id")]
        public int? ColorId { get; set; }

        [JsonPropertyName("color_name")]
        public string ColorName { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("qty")]
        public int Quantity { get; set; }

        [JsonPropertyName("price")]
        public decimal? Price { get; set; }

        [JsonPropertyName("condition")]
        public string Condition { get; set; }

        [JsonPropertyName("external_id")]
        public string ExternalId { get; set; }

        [JsonPropertyName("for_sale")]
        public int? ForSale { get; set; }

        [JsonPropertyName("personal_note")]
        public string PersonalNote { get; set; }

        [JsonPropertyName("my_cost")]
        public decimal? MyCost { get; set; }

        [JsonPropertyName("sale_percent")]
        public int? SalePercent { get; set; }

        [JsonPropertyName("image_small")]
        public string ImageSmall { get; set; }

        [JsonPropertyName("public_note")]
        public string PublicNote { get; set; }

        [JsonPropertyName("lot_weight")]
        public decimal? LotWeight { get; set; }
    }


    /// <summary>
    /// A lot in the user's personal collection (non-store).
    /// </summary>
    public class BrickOwlCollectionLot
    {
        [JsonPropertyName("lot_id")]
        public string LotId { get; set; }

        [JsonPropertyName("boid")]
        public string Boid { get; set; }

        [JsonPropertyName("color_id")]
        public int? ColorId { get; set; }

        [JsonPropertyName("color_name")]
        public string ColorName { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("qty")]
        public int Quantity { get; set; }

        [JsonPropertyName("condition")]
        public string Condition { get; set; }

        [JsonPropertyName("personal_note")]
        public string PersonalNote { get; set; }

        [JsonPropertyName("image_small")]
        public string ImageSmall { get; set; }
    }
}
