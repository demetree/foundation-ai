using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace BMC.BrickSet.Api.Models.Responses
{
    /// <summary>
    /// Core set data from BrickSet.
    /// This is the richest model — contains pricing, dimensions, ratings, availability,
    /// and all the data that Rebrickable doesn't provide.
    /// </summary>
    public class BrickSetSet
    {
        [JsonPropertyName("setID")]
        public int SetID { get; set; }

        [JsonPropertyName("number")]
        public string Number { get; set; }

        [JsonPropertyName("numberVariant")]
        public int NumberVariant { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("theme")]
        public string Theme { get; set; }

        [JsonPropertyName("themeGroup")]
        public string ThemeGroup { get; set; }

        [JsonPropertyName("subtheme")]
        public string Subtheme { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("released")]
        public bool Released { get; set; }

        [JsonPropertyName("pieces")]
        public int? Pieces { get; set; }

        [JsonPropertyName("minifigs")]
        public int? Minifigs { get; set; }

        [JsonPropertyName("image")]
        public BrickSetSetImage Image { get; set; }

        [JsonPropertyName("bricksetURL")]
        public string BricksetURL { get; set; }


        // ─── Pricing ───

        [JsonPropertyName("LEGOCom")]
        public BrickSetSetPricing LEGOCom { get; set; }


        // ─── Dimensions ───

        [JsonPropertyName("dimensions")]
        public BrickSetSetDimensions Dimensions { get; set; }


        // ─── Ratings ───

        [JsonPropertyName("rating")]
        public double Rating { get; set; }

        [JsonPropertyName("reviewCount")]
        public int ReviewCount { get; set; }


        // ─── Availability ───

        [JsonPropertyName("availability")]
        public string Availability { get; set; }

        [JsonPropertyName("instructionsCount")]
        public int InstructionsCount { get; set; }

        [JsonPropertyName("additionalImageCount")]
        public int AdditionalImageCount { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }


        // ─── User collection fields (populated when userHash is provided) ───

        [JsonPropertyName("collection")]
        public BrickSetSetCollection Collection { get; set; }


        /// <summary>
        /// Convenience: full set number including variant (e.g. "75192-1").
        /// </summary>
        public string FullSetNumber => $"{Number}-{NumberVariant}";
    }


    /// <summary>
    /// Set image from BrickSet.
    /// </summary>
    public class BrickSetSetImage
    {
        [JsonPropertyName("thumbnailURL")]
        public string ThumbnailURL { get; set; }

        [JsonPropertyName("imageURL")]
        public string ImageURL { get; set; }
    }


    /// <summary>
    /// Retail pricing from BrickSet — prices in local currency for each market.
    /// </summary>
    public class BrickSetSetPricing
    {
        [JsonPropertyName("US")]
        public BrickSetPrice US { get; set; }

        [JsonPropertyName("UK")]
        public BrickSetPrice UK { get; set; }

        [JsonPropertyName("CA")]
        public BrickSetPrice CA { get; set; }

        [JsonPropertyName("DE")]
        public BrickSetPrice DE { get; set; }
    }


    /// <summary>
    /// Individual market price from BrickSet.
    /// </summary>
    public class BrickSetPrice
    {
        [JsonPropertyName("retailPrice")]
        public decimal? RetailPrice { get; set; }

        [JsonPropertyName("dateFirstAvailable")]
        public string DateFirstAvailable { get; set; }

        [JsonPropertyName("dateLastAvailable")]
        public string DateLastAvailable { get; set; }
    }


    /// <summary>
    /// Physical dimensions of a set from BrickSet.
    /// </summary>
    public class BrickSetSetDimensions
    {
        [JsonPropertyName("height")]
        public double? Height { get; set; }

        [JsonPropertyName("width")]
        public double? Width { get; set; }

        [JsonPropertyName("depth")]
        public double? Depth { get; set; }

        [JsonPropertyName("weight")]
        public double? Weight { get; set; }
    }


    /// <summary>
    /// User's collection status for a set (populated when userHash is supplied to getSets).
    /// </summary>
    public class BrickSetSetCollection
    {
        [JsonPropertyName("owned")]
        public bool Owned { get; set; }

        [JsonPropertyName("wanted")]
        public bool Wanted { get; set; }

        [JsonPropertyName("qtyOwned")]
        public int QtyOwned { get; set; }

        [JsonPropertyName("rating")]
        public int Rating { get; set; }

        [JsonPropertyName("notes")]
        public string Notes { get; set; }
    }


    /// <summary>
    /// Response from getSets.
    /// </summary>
    public class BrickSetSetsResponse : BrickSetApiResponse
    {
        [JsonPropertyName("sets")]
        public List<BrickSetSet> Sets { get; set; }
    }
}
