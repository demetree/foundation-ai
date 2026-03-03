using System.Text.Json.Serialization;


namespace BMC.BrickLink.Api.Models.Responses
{
    //
    // BrickLink Category
    //
    // Returned by GET /categories and GET /categories/{id}
    //
    // AI-Developed — This file was significantly developed with AI assistance.
    //

    /// <summary>
    /// A BrickLink part category.
    /// </summary>
    public class BrickLinkCategory
    {
        [JsonPropertyName("category_id")]
        public int CategoryId { get; set; }

        [JsonPropertyName("category_name")]
        public string CategoryName { get; set; }

        [JsonPropertyName("parent_id")]
        public int ParentId { get; set; }
    }
}
