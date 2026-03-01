using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace BMC.Rebrickable.Api.Models.Responses
{
    /// <summary>
    /// Rebrickable part definition from /lego/parts/.
    /// </summary>
    public class RebrickablePart
    {
        [JsonPropertyName("part_num")]
        public string PartNum { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("part_cat_id")]
        public int PartCatId { get; set; }

        [JsonPropertyName("part_url")]
        public string PartUrl { get; set; }

        [JsonPropertyName("part_img_url")]
        public string PartImgUrl { get; set; }

        [JsonPropertyName("external_ids")]
        public RebrickablePartExternalIds ExternalIds { get; set; }

        [JsonPropertyName("print_of")]
        public string PrintOf { get; set; }
    }


    /// <summary>
    /// External ID mappings for parts.
    /// </summary>
    public class RebrickablePartExternalIds
    {
        [JsonPropertyName("BrickLink")]
        public List<string> BrickLink { get; set; }

        [JsonPropertyName("BrickOwl")]
        public List<string> BrickOwl { get; set; }

        [JsonPropertyName("Brickset")]
        public List<string> Brickset { get; set; }

        [JsonPropertyName("LEGO")]
        public List<string> Lego { get; set; }

        [JsonPropertyName("LDraw")]
        public List<string> LDraw { get; set; }
    }
}
