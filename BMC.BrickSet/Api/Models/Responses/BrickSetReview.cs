using System.Text.Json.Serialization;


namespace BMC.BrickSet.Api.Models.Responses
{
    /// <summary>
    /// A community review from BrickSet with granular ratings.
    /// </summary>
    public class BrickSetReview
    {
        [JsonPropertyName("author")]
        public string Author { get; set; }

        [JsonPropertyName("datePosted")]
        public string DatePosted { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("review")]
        public string ReviewText { get; set; }

        [JsonPropertyName("HTML")]
        public bool IsHTML { get; set; }


        // ─── Granular ratings (1-5 scale) ───

        [JsonPropertyName("overallRating")]
        public int OverallRating { get; set; }

        [JsonPropertyName("parts")]
        public int Parts { get; set; }

        [JsonPropertyName("buildingExperience")]
        public int BuildingExperience { get; set; }

        [JsonPropertyName("playability")]
        public int Playability { get; set; }

        [JsonPropertyName("valueForMoney")]
        public int ValueForMoney { get; set; }
    }


    /// <summary>
    /// Response from getReviews.
    /// </summary>
    public class BrickSetReviewsResponse : BrickSetApiResponse
    {
        [JsonPropertyName("reviews")]
        public System.Collections.Generic.List<BrickSetReview> Reviews { get; set; }
    }
}
