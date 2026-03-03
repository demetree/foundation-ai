using System.Text.Json.Serialization;


namespace BMC.BrickSet.Api.Models.Responses
{
    /// <summary>
    /// An instruction PDF link from BrickSet.
    /// </summary>
    public class BrickSetInstruction
    {
        [JsonPropertyName("URL")]
        public string URL { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }


    /// <summary>
    /// Response from getInstructions / getInstructions2.
    /// </summary>
    public class BrickSetInstructionsResponse : BrickSetApiResponse
    {
        [JsonPropertyName("instructions")]
        public System.Collections.Generic.List<BrickSetInstruction> Instructions { get; set; }
    }
}
