using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace BMC.Rebrickable.Api.Models.Responses
{
    /// <summary>
    /// Inventory part from set or minifig parts endpoints.
    /// </summary>
    public class RebrickableInventoryPart
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("inv_part_id")]
        public int InvPartId { get; set; }

        [JsonPropertyName("part")]
        public RebrickablePart Part { get; set; }

        [JsonPropertyName("color")]
        public RebrickableColor Color { get; set; }

        [JsonPropertyName("set_num")]
        public string SetNum { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("is_spare")]
        public bool IsSpare { get; set; }

        [JsonPropertyName("element_id")]
        public string ElementId { get; set; }

        [JsonPropertyName("num_sets")]
        public int NumSets { get; set; }
    }


    /// <summary>
    /// Inventory minifig from /lego/sets/{set_num}/minifigs/.
    /// </summary>
    public class RebrickableInventoryMinifig
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("set_num")]
        public string SetNum { get; set; }

        [JsonPropertyName("set_name")]
        public string SetName { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("set_img_url")]
        public string SetImgUrl { get; set; }
    }


    /// <summary>
    /// Inventory set (subset) from /lego/sets/{set_num}/sets/.
    /// </summary>
    public class RebrickableInventorySet
    {
        [JsonPropertyName("set_num")]
        public string SetNum { get; set; }

        [JsonPropertyName("set_name")]
        public string SetName { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("set_img_url")]
        public string SetImgUrl { get; set; }
    }


    /// <summary>
    /// Alternate build of a set (MOC built from same parts).
    /// </summary>
    public class RebrickableAlternateBuild
    {
        [JsonPropertyName("set_num")]
        public string SetNum { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("theme_id")]
        public int ThemeId { get; set; }

        [JsonPropertyName("num_parts")]
        public int NumParts { get; set; }

        [JsonPropertyName("moc_img_url")]
        public string MocImgUrl { get; set; }

        [JsonPropertyName("moc_url")]
        public string MocUrl { get; set; }

        [JsonPropertyName("designer_name")]
        public string DesignerName { get; set; }

        [JsonPropertyName("designer_url")]
        public string DesignerUrl { get; set; }
    }
}
