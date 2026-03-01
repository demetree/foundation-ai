using System.Text.Json.Serialization;


namespace BMC.Rebrickable.Api.Models.Responses
{
    /// <summary>
    /// Rebrickable theme definition from /lego/themes/.
    /// </summary>
    public class RebrickableTheme
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("parent_id")]
        public int? ParentId { get; set; }
    }
}
