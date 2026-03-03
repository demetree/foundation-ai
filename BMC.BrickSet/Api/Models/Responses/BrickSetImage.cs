using System.Text.Json.Serialization;


namespace BMC.BrickSet.Api.Models.Responses
{
    /// <summary>
    /// An additional image for a set from BrickSet.
    /// </summary>
    public class BrickSetImage
    {
        [JsonPropertyName("thumbnailURL")]
        public string ThumbnailURL { get; set; }

        [JsonPropertyName("imageURL")]
        public string ImageURL { get; set; }
    }


    /// <summary>
    /// Response from getAdditionalImages.
    /// </summary>
    public class BrickSetImagesResponse : BrickSetApiResponse
    {
        [JsonPropertyName("additionalImages")]
        public System.Collections.Generic.List<BrickSetImage> AdditionalImages { get; set; }
    }
}
