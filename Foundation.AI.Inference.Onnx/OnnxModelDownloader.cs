using System;
using System.IO;
using System.Net.Http;
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
    /// Downloads the requested ONNX model from HuggingFace if the local path is empty.
    /// Skips files that are already on disk at the expected size, so a partially
    /// completed prior run resumes instead of re-downloading from scratch. Each
    /// individual file is retried up to 3 times with backoff on transient failures.
    /// </summary>
    public async Task EnsureModelExistsAsync(string targetDirectory, OnnxModelConfig config, CancellationToken ct = default)
    {
        if (config == null) return;

        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        // All expected files must be present — checking only the sentinel led to
        // silent failures when a prior run completed tokenizer_config.json but not others.
        // A file under the weight-file threshold is treated as a truncated remnant of an
        // interrupted prior run and re-downloaded.
        bool allPresent = true;
        foreach (var f in config.FilesToDownload)
        {
            var path = Path.Combine(targetDirectory, Path.GetFileName(f));
            if (!File.Exists(path) || IsImplausiblySmall(path, f))
            {
                allPresent = false;
                break;
            }
        }
        if (allPresent)
        {
            return; // Model already fully downloaded.
        }

        Console.WriteLine($"[Foundation.AI] Downloading ONNX Model to {targetDirectory}...");

        foreach (var file in config.FilesToDownload)
        {
            string subPath = string.IsNullOrEmpty(config.Subfolder) ? "" : $"{config.Subfolder}/";
            string url = $"https://huggingface.co/{config.RepoId}/resolve/{config.Branch}/{subPath}{file}";
            string destination = Path.Combine(targetDirectory, Path.GetFileName(file));

            await DownloadFileWithRetryAsync(url, destination, file, ct);
        }

        Console.WriteLine("[Foundation.AI] ONNX Model download complete.");
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
        // tokenizer JSONs on every boot when only the big weight file failed.
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
                // URL (repo renamed it, branch wrong, auth required, etc.).
                // Retrying burns attempts for no gain and delays boot.
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

    // Weight files (.onnx / .onnx.data / .bin / .safetensors) under 1 KB are truncation
    // artefacts from an interrupted download. JSON configs can legitimately be tiny, so
    // for those we only reject zero-byte files.
    private static bool IsImplausiblySmall(string path, string originalName)
    {
        long size = new FileInfo(path).Length;
        if (size == 0) return true;

        string lower = originalName.ToLowerInvariant();
        bool isWeightFile = lower.EndsWith(".onnx")
            || lower.EndsWith(".onnx.data")
            || lower.EndsWith(".bin")
            || lower.EndsWith(".safetensors");

        return isWeightFile && size < 1024;
    }
}
