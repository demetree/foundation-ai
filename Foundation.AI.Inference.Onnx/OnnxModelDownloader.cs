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
        bool allPresent = true;
        foreach (var f in config.FilesToDownload)
        {
            if (!File.Exists(Path.Combine(targetDirectory, Path.GetFileName(f))))
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
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to download {file} from {url}", ex);
            }
        }

        Console.WriteLine("[Foundation.AI] ONNX Model download complete.");
    }
}
