using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Foundation.AI.Inference;

/// <summary>
/// Admin-facing, runtime-mutable accessor for the currently-active inference
/// model tag. Holds a reference to the shared <see cref="OpenAiInferenceConfig"/>
/// singleton and mutates its <see cref="OpenAiInferenceConfig.Model"/> property
/// in place — <see cref="OpenAiInferenceProvider"/> reads the tag per request,
/// so the switch takes effect on the next chat without restarting DI.
///
/// <para>In-memory only. Restart restores the tag from <c>AI:Inference:Model</c>
/// in appsettings.json.</para>
///
/// <para>Also exposes helpers for the admin UI: listing Ollama's installed
/// models (<c>/api/tags</c>) and unloading the previous model from VRAM on
/// switch (<c>/api/generate</c> with <c>keep_alive: 0</c>).</para>
/// </summary>
public sealed class ActiveInferenceModel
{
    private readonly OpenAiInferenceConfig _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ActiveInferenceModel> _logger;

    public ActiveInferenceModel(
        OpenAiInferenceConfig config,
        IHttpClientFactory httpClientFactory,
        ILogger<ActiveInferenceModel> logger)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// The model tag currently used for inference. Mutated by
    /// <see cref="SetModelAsync"/>; reads are lock-free because string
    /// reference assignment is atomic on the CLR.
    /// </summary>
    public string CurrentModel => _config.Model;

    /// <summary>
    /// The inference endpoint currently configured. Exposed so callers can
    /// show the user where we're pointing without re-reading config.
    /// </summary>
    public string Endpoint => _config.Endpoint;

    /// <summary>
    /// Switches the active model. Optionally unloads the previous model from
    /// Ollama's VRAM immediately (by issuing a <c>keep_alive: 0</c> generate
    /// call against it) so the new model has room to load.
    /// </summary>
    /// <param name="newTag">The new model tag, e.g. <c>"qwen3:4b"</c>.</param>
    /// <param name="unloadPrevious">When true (default), issue a keep_alive=0
    /// request for the outgoing tag so Ollama drops it from VRAM now rather
    /// than waiting out its idle timer.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task SetModelAsync(string newTag, bool unloadPrevious, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(newTag))
            throw new ArgumentException("Model tag cannot be empty.", nameof(newTag));

        string previous = _config.Model;
        if (string.Equals(previous, newTag, StringComparison.Ordinal))
        {
            _logger.LogInformation("Active inference model unchanged at {Model}", newTag);
            return;
        }

        _config.Model = newTag;
        _logger.LogInformation("Active inference model switched from {Previous} to {Next}", previous, newTag);

        if (unloadPrevious && !string.IsNullOrWhiteSpace(previous))
        {
            try
            {
                string ollamaBase = GetOllamaBase();
                HttpClient http = _httpClientFactory.CreateClient();
                http.Timeout = TimeSpan.FromSeconds(10);

                // Ollama's native /api/generate accepts keep_alive. Sending "0"
                // (or "0s") asks it to unload the model as soon as this empty
                // request returns — no tokens generated, just a VRAM eviction.
                var payload = new { model = previous, prompt = "", keep_alive = "0s", stream = false };
                HttpResponseMessage resp = await http.PostAsJsonAsync(
                    ollamaBase.TrimEnd('/') + "/api/generate", payload, ct).ConfigureAwait(false);

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Ollama keep_alive=0 request for {Previous} returned {Status}; VRAM may linger until the idle timeout",
                        previous, (int)resp.StatusCode);
                }
            }
            catch (Exception ex)
            {
                // Unload is best-effort. Ollama will evict on its own idle timer
                // eventually, and a failure here shouldn't roll back the switch.
                _logger.LogWarning(ex, "Failed to unload previous model {Previous} from Ollama; VRAM may linger until idle timeout", previous);
            }
        }
    }

    /// <summary>
    /// Lists all models installed in the local Ollama instance (equivalent to
    /// <c>ollama list</c>). Returns an empty list if the endpoint is
    /// unreachable or the response is malformed — callers should treat that
    /// as "Ollama is offline", not as "no models".
    /// </summary>
    public async Task<IReadOnlyList<OllamaModelInfo>> ListInstalledAsync(CancellationToken ct)
    {
        string ollamaBase = GetOllamaBase();
        HttpClient http = _httpClientFactory.CreateClient();
        http.Timeout = TimeSpan.FromSeconds(3);

        try
        {
            HttpResponseMessage resp = await http.GetAsync(
                ollamaBase.TrimEnd('/') + "/api/tags", ct).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogDebug("Ollama /api/tags returned {Status}", (int)resp.StatusCode);
                return Array.Empty<OllamaModelInfo>();
            }

            OllamaTagsResponse parsed = await resp.Content.ReadFromJsonAsync<OllamaTagsResponse>(
                JsonOpts, ct).ConfigureAwait(false);
            return parsed?.Models ?? (IReadOnlyList<OllamaModelInfo>)Array.Empty<OllamaModelInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Ollama /api/tags probe failed against {Base}", ollamaBase);
            return Array.Empty<OllamaModelInfo>();
        }
    }

    private string GetOllamaBase()
    {
        try
        {
            return new Uri(_config.Endpoint).GetLeftPart(UriPartial.Authority);
        }
        catch
        {
            return "http://localhost:11434";
        }
    }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    private sealed class OllamaTagsResponse
    {
        [JsonPropertyName("models")]
        public List<OllamaModelInfo> Models { get; set; }
    }

    public sealed class OllamaModelInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("modified_at")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [JsonPropertyName("details")]
        public OllamaModelDetails Details { get; set; }
    }

    public sealed class OllamaModelDetails
    {
        [JsonPropertyName("parameter_size")]
        public string ParameterSize { get; set; }

        [JsonPropertyName("quantization_level")]
        public string QuantizationLevel { get; set; }

        [JsonPropertyName("family")]
        public string Family { get; set; }
    }
}
