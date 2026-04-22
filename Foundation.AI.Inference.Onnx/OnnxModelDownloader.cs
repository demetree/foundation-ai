using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Foundation.AI.Inference.Onnx;

public class OnnxModelDownloader
{
    private const int MaxRetries = 3;
    private static readonly TimeSpan[] RetryBackoff = new[]
    {
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(30)
    };

    private readonly HttpClient _httpClient;

    public OnnxModelDownloader()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromMinutes(30); // Prevent 100s default timeout
    }

    /// <summary>
    /// Ensures the requested ONNX model is present at <paramref name="targetDirectory"/>.
    ///
    /// The file list is discovered at runtime by hitting HuggingFace's public tree API
    /// for the configured repo/branch/subfolder, so the config only needs repo coordinates
    /// — never a hand-curated file list. Already-downloaded files are skipped via a
    /// HEAD-check against the remote Content-Length. Each file gets retry-with-backoff
    /// on transient failures.
    /// </summary>
    public async Task EnsureModelExistsAsync(string targetDirectory, OnnxModelConfig config, CancellationToken ct = default)
    {
        if (config == null) return;
        if (string.IsNullOrWhiteSpace(config.RepoId))
        {
            throw new InvalidOperationException("OnnxModelConfig.RepoId must be set to a HuggingFace repo ID.");
        }

        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        //
        // Discover the file list from HuggingFace. No local cache, no fallback —
        // the call is cheap (one JSON response, ~KB) and we need it to know
        // what "complete" even means.
        //
        string[] files = await ListRemoteFilesAsync(config.RepoId, config.Branch, config.Subfolder, ct);
        if (files.Length == 0)
        {
            Console.WriteLine($"[Foundation.AI] HuggingFace tree listing returned no files for {config.RepoId}/{config.Subfolder}. Nothing to download.");
            return;
        }

        //
        // Per-file: HEAD to get expected size, skip if local already matches,
        // else download with retry. The download loop is order-independent
        // so there's no partial-state concern.
        //
        Console.WriteLine($"[Foundation.AI] Ensuring {files.Length} model file(s) at {targetDirectory} (repo {config.RepoId}, subfolder '{config.Subfolder}')...");

        foreach (var file in files)
        {
            string subPath = string.IsNullOrEmpty(config.Subfolder) ? "" : $"{config.Subfolder}/";
            string url = $"https://huggingface.co/{config.RepoId}/resolve/{config.Branch ?? "main"}/{subPath}{file}";
            string destination = Path.Combine(targetDirectory, Path.GetFileName(file));

            await DownloadFileWithRetryAsync(url, destination, file, ct);
        }

        Console.WriteLine("[Foundation.AI] ONNX model ready.");
    }


    /// <summary>
    /// Calls HuggingFace's public tree API and returns the filenames (subfolder-
    /// relative, not full repo paths) that live in the requested subfolder.
    /// Throws on failure — callers rely on the list to know what to download.
    /// </summary>
    private async Task<string[]> ListRemoteFilesAsync(string repoId, string branch, string subfolder, CancellationToken ct)
    {
        string apiSubPath = string.IsNullOrEmpty(subfolder) ? "" : $"/{subfolder}";
        string url = $"https://huggingface.co/api/models/{repoId}/tree/{branch ?? "main"}{apiSubPath}?recursive=true";

        using var resp = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!resp.IsSuccessStatusCode)
        {
            throw new Exception($"HuggingFace tree API returned {(int)resp.StatusCode} for {url}. Check RepoId / Branch / Subfolder.");
        }

        string json = await resp.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new Exception($"HuggingFace tree API returned an unexpected JSON shape for {url}.");
        }

        var files = new List<string>();
        foreach (var entry in doc.RootElement.EnumerateArray())
        {
            if (!entry.TryGetProperty("type", out var typeProp)) continue;
            if (typeProp.GetString() != "file") continue;

            if (!entry.TryGetProperty("path", out var pathProp)) continue;
            string fullPath = pathProp.GetString();
            if (string.IsNullOrWhiteSpace(fullPath)) continue;

            //
            // API paths are repo-root relative. Strip the subfolder prefix so
            // names are relative to Subfolder — the download loop joins them
            // back onto {Subfolder}/{file} when building the resolve URL.
            //
            string relativePath = fullPath;
            if (!string.IsNullOrEmpty(subfolder))
            {
                string prefix = subfolder.TrimEnd('/') + "/";
                if (relativePath.StartsWith(prefix, StringComparison.Ordinal))
                {
                    relativePath = relativePath.Substring(prefix.Length);
                }
            }

            files.Add(relativePath);
        }

        return files.ToArray();
    }


    /// <summary>
    /// Download a single file, skipping if it's already present at the expected
    /// size, with retry-on-transient-failure semantics.
    /// </summary>
    private async Task DownloadFileWithRetryAsync(string url, string destination, string file, CancellationToken ct)
    {
        //
        // Per-file skip: HEAD the remote to learn the expected size. If the
        // local file matches byte-for-byte, we're done. Saves re-downloading
        // big weight files on every boot when a prior run completed partially.
        //
        long? remoteSize = await TryGetRemoteContentLengthAsync(url, ct);
        if (File.Exists(destination) && remoteSize.HasValue)
        {
            long localSize = new FileInfo(destination).Length;
            if (localSize == remoteSize.Value)
            {
                Console.WriteLine($"[Foundation.AI] Skipping {file} — already present ({localSize / 1048576d:F1} MB).");
                return;
            }
        }

        Exception lastError = null;

        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                await DownloadFileOnceAsync(url, destination, file, ct);
                return; // Success.
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                // Shutdown in progress — don't retry.
                throw;
            }
            catch (HttpRequestException httpEx) when (IsPermanentStatusCode(httpEx.StatusCode))
            {
                //
                // 4xx is a permanent error — the file genuinely isn't at that
                // URL (auth required, repo moved since tree listing, rate-limited).
                // No point retrying. Surface a clean diagnostic.
                //
                throw new Exception(
                    $"Failed to download {file} from {url}: {httpEx.Message} (permanent error, not retrying).",
                    httpEx);
            }
            catch (Exception ex)
            {
                lastError = ex;
                Console.WriteLine();
                Console.WriteLine($"[Foundation.AI] Download attempt {attempt}/{MaxRetries} for {file} failed: {ex.Message}");

                if (attempt < MaxRetries)
                {
                    TimeSpan delay = RetryBackoff[attempt - 1];
                    Console.WriteLine($"[Foundation.AI] Retrying in {delay.TotalSeconds:F0}s...");
                    try { await Task.Delay(delay, ct); }
                    catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
                }
            }
        }

        throw new Exception($"Failed to download {file} from {url} after {MaxRetries} attempts.", lastError);
    }


    /// <summary>
    /// True for HTTP status codes that will never succeed on retry — 4xx
    /// (client error: not found, unauthorized, bad request, etc). Server-side
    /// 5xx and null (connection errors) are considered transient and eligible
    /// for retry.
    /// </summary>
    private static bool IsPermanentStatusCode(System.Net.HttpStatusCode? status)
    {
        if (status == null) return false;
        int code = (int)status.Value;
        return code >= 400 && code < 500;
    }


    /// <summary>
    /// Do the actual HTTP download — one attempt. Caller is responsible for retry.
    /// </summary>
    private async Task DownloadFileOnceAsync(string url, string destination, string file, CancellationToken ct)
    {
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        long? totalBytes = response.Content.Headers.ContentLength;

        using var contentStream = await response.Content.ReadAsStreamAsync(ct);
        using var fileStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        if (totalBytes.HasValue && totalBytes.Value > 0)
        {
            byte[] buffer = new byte[8192];
            long totalRead = 0;
            int bytesRead;
            int lastPercent = -1;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, ct);
                totalRead += bytesRead;

                int currentPercent = (int)((totalRead * 100) / totalBytes.Value);
                if (currentPercent != lastPercent)
                {
                    Console.Write($"\rDownloading {file}... {currentPercent}% ({(totalRead / 1048576)} MB / {(totalBytes.Value / 1048576)} MB)    ");
                    lastPercent = currentPercent;
                }
            }
            Console.WriteLine(); // Finalize line after 100%
        }
        else
        {
            Console.Write($"Downloading {file}... ");
            await contentStream.CopyToAsync(fileStream, ct);
            Console.WriteLine("Done.");
        }

        // Flush before size check so the FileStream's buffer is on disk.
        await fileStream.FlushAsync(ct);
        fileStream.Close();

        if (totalBytes.HasValue && totalBytes.Value > 0)
        {
            long actualBytes = new FileInfo(destination).Length;
            if (actualBytes != totalBytes.Value)
            {
                try { File.Delete(destination); } catch { /* best-effort cleanup */ }
                throw new IOException($"Size mismatch for {file}: expected {totalBytes.Value} bytes, got {actualBytes}. Partial file deleted.");
            }
        }
    }


    /// <summary>
    /// HEAD the URL and return the Content-Length, or null if the server didn't
    /// report one / the HEAD failed. Best-effort — a null result just means the
    /// skip-check short-circuit is unavailable for this file, not that the
    /// download will fail.
    /// </summary>
    private async Task<long?> TryGetRemoteContentLengthAsync(string url, CancellationToken ct)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Head, url);
            using var resp = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!resp.IsSuccessStatusCode) return null;
            return resp.Content.Headers.ContentLength;
        }
        catch
        {
            return null;
        }
    }
}
