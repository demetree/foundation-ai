using System;

namespace Foundation.AI.Inference.Onnx;

/// <summary>
/// Defines the required HuggingFace properties and file lists to download and instantiate an ONNX model.
/// </summary>
public class OnnxModelConfig
{
    /// <summary>
    /// The HuggingFace repository ID (e.g., "microsoft/Phi-3-mini-4k-instruct-onnx")
    /// </summary>
    public string RepoId { get; set; } = string.Empty;

    /// <summary>
    /// The exact branch of the repository. Defaults to main.
    /// </summary>
    public string Branch { get; set; } = "main";

    /// <summary>
    /// The subdirectory containing the ONNX model files.
    /// </summary>
    public string Subfolder { get; set; } = string.Empty;

    /// <summary>
    /// The prompt formatting template that the model was instructed with (e.g. "Phi3" or "ChatML").
    /// </summary>
    public string PromptTemplate { get; set; } = "Phi3";

    /// <summary>
    /// An exhaustive list of files inside the repository's subfolder that must be downloaded for the local model to boot.
    /// </summary>
    public string[] FilesToDownload { get; set; } = Array.Empty<string>();
}
