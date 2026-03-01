using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace BMC.Rebrickable.Api.Models.Responses
{
    /// <summary>
    /// Generic paginated response wrapper matching Rebrickable's standard pagination format.
    /// All list endpoints return this shape.
    /// </summary>
    public class PagedResponse<T>
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("next")]
        public string Next { get; set; }

        [JsonPropertyName("previous")]
        public string Previous { get; set; }

        [JsonPropertyName("results")]
        public List<T> Results { get; set; } = new List<T>();
    }
}
