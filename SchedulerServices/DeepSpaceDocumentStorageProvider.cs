//
// DeepSpaceDocumentStorageProvider.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// IDocumentStorageProvider that stores binary content via the DeepSpace Host
// service over HTTP. This is the "Standard/Enterprise" option — gets full
// metadata tracking, version history, sidecar-based disaster recovery, and
// multi-provider support for free.
//
// Communicates with the DeepSpace Host API:
//   PUT    /api/deepspace/put?key={storageKey}       → binary body
//   GET    /api/deepspace/get?key={storageKey}        → binary response
//   DELETE /api/deepspace/delete?key={storageKey}     → { Deleted: true }
//   GET    /api/deepspace/exists?key={storageKey}     → { Exists: true/false }
//
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Scheduler.Server.Services
{
    /// <summary>
    /// Stores document binary content via the DeepSpace Host service over HTTP.
    /// Leverages full metadata tracking, version history, and sidecar files.
    /// </summary>
    public class DeepSpaceDocumentStorageProvider : IDocumentStorageProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DeepSpaceDocumentStorageProvider> _logger;

        public string ProviderName => "DeepSpace";


        /// <summary>
        /// Creates a new DeepSpace provider.
        /// The HttpClient should have its BaseAddress set to the DeepSpace Host URL.
        /// </summary>
        public DeepSpaceDocumentStorageProvider(HttpClient httpClient, ILogger<DeepSpaceDocumentStorageProvider> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger;
        }


        public async Task<byte[]> GetContentAsync(string storageKey, CancellationToken ct = default)
        {
            try
            {
                string url = $"/api/deepspace/get?key={Uri.EscapeDataString(storageKey)}";

                HttpResponseMessage response = await _httpClient.GetAsync(url, ct).ConfigureAwait(false);

                if (response.IsSuccessStatusCode == false)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return null;
                    }

                    _logger.LogWarning("DeepSpaceDocumentStorageProvider: GET failed for key '{Key}' — {Status}.", storageKey, response.StatusCode);
                    return null;
                }

                return await response.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeepSpaceDocumentStorageProvider: exception during GET for key '{Key}'.", storageKey);
                return null;
            }
        }


        public async Task<string> StoreContentAsync(string storageKey, byte[] data, string mimeType, CancellationToken ct = default)
        {
            try
            {
                string url = $"/api/deepspace/put?key={Uri.EscapeDataString(storageKey)}";

                using ByteArrayContent content = new ByteArrayContent(data);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType ?? "application/octet-stream");

                HttpResponseMessage response = await _httpClient.PostAsync(url, content, ct).ConfigureAwait(false);

                if (response.IsSuccessStatusCode == false)
                {
                    _logger.LogError("DeepSpaceDocumentStorageProvider: PUT failed for key '{Key}' — {Status}.", storageKey, response.StatusCode);
                    throw new InvalidOperationException($"DeepSpace PUT failed: {response.StatusCode}");
                }

                _logger.LogDebug("DeepSpaceDocumentStorageProvider: stored {Size} bytes at '{Key}'.", data.Length, storageKey);

                return storageKey;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "DeepSpaceDocumentStorageProvider: connection error during PUT for key '{Key}'. Is the DeepSpace Host running?", storageKey);
                throw;
            }
        }


        public async Task DeleteContentAsync(string storageKey, CancellationToken ct = default)
        {
            try
            {
                string url = $"/api/deepspace/delete?key={Uri.EscapeDataString(storageKey)}";

                HttpResponseMessage response = await _httpClient.DeleteAsync(url, ct).ConfigureAwait(false);

                if (response.IsSuccessStatusCode == false)
                {
                    _logger.LogWarning("DeepSpaceDocumentStorageProvider: DELETE failed for key '{Key}' — {Status}.", storageKey, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "DeepSpaceDocumentStorageProvider: exception during DELETE for key '{Key}'.", storageKey);
            }
        }


        public async Task<bool> ExistsAsync(string storageKey, CancellationToken ct = default)
        {
            try
            {
                string url = $"/api/deepspace/exists?key={Uri.EscapeDataString(storageKey)}";

                HttpResponseMessage response = await _httpClient.GetAsync(url, ct).ConfigureAwait(false);

                if (response.IsSuccessStatusCode == false)
                {
                    return false;
                }

                string json = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

                //
                // Simple parse — response is { "Key": "...", "Exists": true/false }
                //
                return json.Contains("true", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "DeepSpaceDocumentStorageProvider: exception during EXISTS for key '{Key}'.", storageKey);
                return false;
            }
        }


        public async Task<string> GetPresignedUrlAsync(string storageKey, TimeSpan expires, CancellationToken ct = default)
        {
            try
            {
                string url = $"/api/deepspace/presigned-url?key={Uri.EscapeDataString(storageKey)}&expires={expires.TotalMinutes}";

                HttpResponseMessage response = await _httpClient.GetAsync(url, ct).ConfigureAwait(false);

                if (response.IsSuccessStatusCode == false)
                {
                    return null;
                }

                string json = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

                using var doc = System.Text.Json.JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("Url", out var urlProp) || doc.RootElement.TryGetProperty("url", out urlProp))
                {
                    string presignedUrl = urlProp.GetString();
                    return string.IsNullOrEmpty(presignedUrl) ? null : presignedUrl;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "DeepSpaceDocumentStorageProvider: exception during GetPresignedUrlAsync for key '{Key}'.", storageKey);
                return null;
            }
        }
    }
}
