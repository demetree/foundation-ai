using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace BMC.Rebrickable.Api.Models.Responses
{
    /// <summary>
    /// Rebrickable colour definition from /lego/colors/.
    /// </summary>
    public class RebrickableColor
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("rgb")]
        public string Rgb { get; set; }

        [JsonPropertyName("is_trans")]
        public bool IsTrans { get; set; }

        [JsonPropertyName("external_ids")]
        public RebrickableExternalIds ExternalIds { get; set; }
    }


    /// <summary>
    /// External ID mappings for colours (BrickLink, LEGO, LDraw, etc.).
    /// </summary>
    public class RebrickableExternalIds
    {
        [JsonPropertyName("BrickLink")]
        public RebrickableExternalIdEntry BrickLink { get; set; }

        [JsonPropertyName("BrickOwl")]
        public RebrickableExternalIdEntry BrickOwl { get; set; }

        [JsonPropertyName("LEGO")]
        public RebrickableExternalIdEntry Lego { get; set; }

        [JsonPropertyName("LDraw")]
        public RebrickableExternalIdEntry LDraw { get; set; }

        [JsonPropertyName("Peeron")]
        public RebrickableExternalIdEntry Peeron { get; set; }
    }


    /// <summary>
    /// A single external ID entry with ext_ids and ext_descrs arrays.
    /// </summary>
    public class RebrickableExternalIdEntry
    {
        [JsonPropertyName("ext_ids")]
        public List<object> ExtIds { get; set; }

        [JsonPropertyName("ext_descrs")]
        public List<List<string>> ExtDescrs { get; set; }
    }
}
