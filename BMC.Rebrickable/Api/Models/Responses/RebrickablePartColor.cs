using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace BMC.Rebrickable.Api.Models.Responses
{
    /// <summary>
    /// Part/colour combination from /lego/parts/{part_num}/colors/.
    /// </summary>
    public class RebrickablePartColor
    {
        [JsonPropertyName("color_id")]
        public int ColorId { get; set; }

        [JsonPropertyName("color_name")]
        public string ColorName { get; set; }

        [JsonPropertyName("num_sets")]
        public int NumSets { get; set; }

        [JsonPropertyName("num_set_parts")]
        public int NumSetParts { get; set; }

        [JsonPropertyName("part_img_url")]
        public string PartImgUrl { get; set; }

        [JsonPropertyName("elements")]
        public List<string> Elements { get; set; }
    }
}
