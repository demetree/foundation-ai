using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace BMC.BrickOwl.Api.Models.Responses
{
    //
    // Brick Owl Wishlist Models
    //
    // Returned by GET /wishlist/list, /wishlist/items
    //
    // AI-Developed — This file was significantly developed with AI assistance.
    //

    /// <summary>
    /// A wishlist on the user's Brick Owl account.
    /// </summary>
    public class BrickOwlWishlist
    {
        [JsonPropertyName("wishlist_id")]
        public string WishlistId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("item_count")]
        public int? ItemCount { get; set; }
    }


    /// <summary>
    /// An item within a wishlist.
    /// </summary>
    public class BrickOwlWishlistItem
    {
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

        [JsonPropertyName("condition")]
        public string Condition { get; set; }

        [JsonPropertyName("image_small")]
        public string ImageSmall { get; set; }
    }
}
