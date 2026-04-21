using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Foundation.AI.Inference.Onnx;

public class OnnxModelDownloader
{
    private readonly HttpClient _httpClient;

    public OnnxModelDownloader()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromMinutes(30); // Prevent 100s default timeout
    }

    /// <summary>
    /// Downloads the requested ONNX model from HuggingFace if the local path is empty.
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

            try
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
            catch (Exception ex)
            {
                throw new Exception($"Failed to download {file} from {url}", ex);
            }
        }

        Console.WriteLine("[Foundation.AI] ONNX Model download complete.");
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
