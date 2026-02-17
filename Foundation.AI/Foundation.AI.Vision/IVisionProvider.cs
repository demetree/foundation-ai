namespace Foundation.AI.Vision;

/// <summary>
/// Provides image understanding capabilities: description, analysis, and object detection.
///
/// <para><b>Purpose:</b>
/// Unified interface for vision AI tasks — image description (captioning),
/// visual question-answering (VQA), and object detection. Supports both
/// cloud providers (GPT-4o Vision, Azure) and local models (LLaVA via Ollama,
/// YOLO via ONNX Runtime).</para>
///
/// <para><b>BMC use cases:</b>
/// <list type="bullet">
/// <item>Classify surface conditions from site photos</item>
/// <item>Analyze heatmap images for pattern anomalies</item>
/// <item>Describe drone/site imagery for searchable logs</item>
/// <item>Extract information from construction documents</item>
/// </list></para>
/// </summary>
public interface IVisionProvider : IAsyncDisposable
{
    /// <summary>Human-readable provider name (e.g., "openai:gpt-4o", "ollama:llava").</summary>
    string ModelName { get; }

    /// <summary>
    /// Describe an image in natural language.
    /// </summary>
    /// <param name="imageBytes">Raw image bytes (JPEG, PNG, WebP, GIF).</param>
    /// <param name="prompt">Optional prompt to guide the description (e.g., "Describe the surface condition").</param>
    /// <returns>Natural language description of the image.</returns>
    Task<VisionResponse> DescribeImageAsync(ReadOnlyMemory<byte> imageBytes,
        string? prompt = null,
        VisionOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Describe an image from a URL.
    /// </summary>
    Task<VisionResponse> DescribeImageAsync(Uri imageUrl,
        string? prompt = null,
        VisionOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Ask a specific question about an image (Visual Question Answering).
    /// </summary>
    /// <param name="imageBytes">Raw image bytes.</param>
    /// <param name="question">The question to answer about the image.</param>
    Task<VisionResponse> AskAboutImageAsync(ReadOnlyMemory<byte> imageBytes,
        string question,
        VisionOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Analyze multiple images in a single request (comparative analysis).
    /// </summary>
    /// <param name="images">List of image byte arrays to analyze together.</param>
    /// <param name="prompt">Prompt for the analysis (e.g., "Compare these two site conditions").</param>
    Task<VisionResponse> AnalyzeImagesAsync(IReadOnlyList<ReadOnlyMemory<byte>> images,
        string prompt,
        VisionOptions? options = null,
        CancellationToken ct = default);
}

/// <summary>Options for vision processing.</summary>
public class VisionOptions
{
    /// <summary>Temperature for response generation. Default: 0.3 (factual).</summary>
    public float Temperature { get; set; } = 0.3f;

    /// <summary>Maximum tokens for the generated response. Default: 1024.</summary>
    public int MaxTokens { get; set; } = 1024;

    /// <summary>
    /// Image detail level: "low" (fast, cheap), "high" (detailed analysis), or "auto".
    /// Only applicable to OpenAI/Azure providers. Default: "auto".
    /// </summary>
    public string Detail { get; set; } = "auto";
}

/// <summary>Response from a vision analysis request.</summary>
/// <param name="Description">The generated text description or answer.</param>
/// <param name="TokensUsed">Total tokens consumed, if reported by the provider.</param>
public record VisionResponse(string Description, int? TokensUsed = null);
