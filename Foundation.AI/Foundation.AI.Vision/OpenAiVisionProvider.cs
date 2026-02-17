using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Foundation.AI.Vision;

/// <summary>
/// Vision provider using the OpenAI Chat Completions API with image inputs.
///
/// <para><b>Compatible with:</b>
/// <list type="bullet">
/// <item><b>OpenAI</b> — GPT-4o, GPT-4o-mini (both support vision)</item>
/// <item><b>Azure OpenAI</b> — GPT-4o deployments</item>
/// <item><b>Ollama</b> — LLaVA, Llama 3.2 Vision, BakLLaVA, etc.</item>
/// </list></para>
///
/// <para><b>How it works:</b>
/// Uses the standard chat completions API with multimodal content blocks.
/// Images are sent as base64-encoded data URLs or as external URLs.</para>
/// </summary>
public sealed class OpenAiVisionProvider : IVisionProvider
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiVisionConfig _config;

    public string ModelName => $"openai-vision:{_config.Model}";

    public OpenAiVisionProvider(OpenAiVisionConfig config)
    {
        _config = config;
        _httpClient = new HttpClient { Timeout = config.Timeout };

        if (!string.IsNullOrWhiteSpace(config.ApiKey))
        {
            if (config.UseAzureAuth)
                _httpClient.DefaultRequestHeaders.Add("api-key", config.ApiKey);
            else
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.ApiKey);
        }
    }

    public async Task<VisionResponse> DescribeImageAsync(ReadOnlyMemory<byte> imageBytes,
        string? prompt = null, VisionOptions? options = null, CancellationToken ct = default)
    {
        prompt ??= "Describe this image in detail.";
        var content = BuildImageContent(imageBytes, prompt, options);
        return await SendRequestAsync(content, options, ct);
    }

    public async Task<VisionResponse> DescribeImageAsync(Uri imageUrl,
        string? prompt = null, VisionOptions? options = null, CancellationToken ct = default)
    {
        prompt ??= "Describe this image in detail.";
        var content = BuildUrlContent(imageUrl, prompt, options);
        return await SendRequestAsync(content, options, ct);
    }

    public async Task<VisionResponse> AskAboutImageAsync(ReadOnlyMemory<byte> imageBytes,
        string question, VisionOptions? options = null, CancellationToken ct = default)
    {
        var content = BuildImageContent(imageBytes, question, options);
        return await SendRequestAsync(content, options, ct);
    }

    public async Task<VisionResponse> AnalyzeImagesAsync(IReadOnlyList<ReadOnlyMemory<byte>> images,
        string prompt, VisionOptions? options = null, CancellationToken ct = default)
    {
        var contentParts = new List<object>();

        // Add each image as a content part
        foreach (var imageBytes in images)
        {
            var base64 = Convert.ToBase64String(imageBytes.Span);
            var mimeType = DetectMimeType(imageBytes.Span);
            contentParts.Add(new
            {
                type = "image_url",
                image_url = new
                {
                    url = $"data:{mimeType};base64,{base64}",
                    detail = options?.Detail ?? "auto"
                }
            });
        }

        // Add the text prompt
        contentParts.Add(new { type = "text", text = prompt });

        return await SendRequestAsync(contentParts, options, ct);
    }

    public ValueTask DisposeAsync()
    {
        _httpClient.Dispose();
        return ValueTask.CompletedTask;
    }

    // ─── Request Building ───────────────────────────────────────

    private List<object> BuildImageContent(ReadOnlyMemory<byte> imageBytes,
        string text, VisionOptions? options)
    {
        var base64 = Convert.ToBase64String(imageBytes.Span);
        var mimeType = DetectMimeType(imageBytes.Span);

        return
        [
            new
            {
                type = "image_url",
                image_url = new
                {
                    url = $"data:{mimeType};base64,{base64}",
                    detail = options?.Detail ?? "auto"
                }
            },
            new { type = "text", text }
        ];
    }

    private static List<object> BuildUrlContent(Uri imageUrl, string text, VisionOptions? options)
    {
        return
        [
            new
            {
                type = "image_url",
                image_url = new
                {
                    url = imageUrl.ToString(),
                    detail = options?.Detail ?? "auto"
                }
            },
            new { type = "text", text }
        ];
    }

    private async Task<VisionResponse> SendRequestAsync(List<object> contentParts,
        VisionOptions? options, CancellationToken ct)
    {
        options ??= new VisionOptions();

        var request = new
        {
            model = _config.Model,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = contentParts
                }
            },
            temperature = options.Temperature,
            max_tokens = options.MaxTokens
        };

        var response = await _httpClient.PostAsJsonAsync(
            _config.Endpoint, request, JsonOptions, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(
            JsonOptions, ct);

        var choice = result?.Choices?.FirstOrDefault();
        return new VisionResponse(
            Description: choice?.Message?.Content ?? "",
            TokensUsed: result?.Usage?.TotalTokens);
    }

    // ─── Helpers ────────────────────────────────────────────────

    /// <summary>
    /// Detect image MIME type from magic bytes.
    /// </summary>
    private static string DetectMimeType(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            return "image/jpeg";
        if (bytes.Length >= 8 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            return "image/png";
        if (bytes.Length >= 4 && bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46)
            return "image/webp";
        if (bytes.Length >= 4 && bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46)
            return "image/gif";

        return "image/jpeg"; // fallback
    }

    // ─── JSON Contracts ─────────────────────────────────────────

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private sealed class ChatCompletionResponse
    {
        public List<ChatChoice>? Choices { get; set; }
        public UsageInfo? Usage { get; set; }
    }

    private sealed class ChatChoice
    {
        public MessageContent? Message { get; set; }
    }

    private sealed class MessageContent
    {
        public string? Content { get; set; }
    }

    private sealed class UsageInfo
    {
        public int? TotalTokens { get; set; }
    }
}
