namespace Foundation.AI.Inference.Onnx;

/// <summary>
/// Identifies a HuggingFace-hosted ONNX model variant. The downloader discovers
/// the file list at runtime via the HF tree API, so nothing here names individual
/// files — just the repo coordinates.
/// </summary>
public class OnnxModelConfig
{
    /// <summary>
    /// The HuggingFace repository ID (e.g., "microsoft/Phi-4-mini-instruct-onnx").
    /// </summary>
    public string RepoId { get; set; } = string.Empty;

    /// <summary>
    /// The exact branch of the repository. Defaults to main.
    /// </summary>
    public string Branch { get; set; } = "main";

    /// <summary>
    /// The subdirectory containing the ONNX model files. Selects the execution
    /// provider variant for multi-variant repos (e.g.
    /// "directml/directml-int4-awq-block-128" vs
    /// "cpu_and_mobile/cpu-int4-rtn-block-32-acc-level-4").
    /// </summary>
    public string Subfolder { get; set; } = string.Empty;

    /// <summary>
    /// The prompt formatting template that the model was instructed with (e.g. "Phi3" or "ChatML").
    /// </summary>
    public string PromptTemplate { get; set; } = "Phi3";
}
