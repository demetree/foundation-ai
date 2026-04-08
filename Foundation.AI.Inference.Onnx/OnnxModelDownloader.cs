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

        // Check if the FINAL file exists to determine if download fully succeeded previously
        var lastFilePath = Path.Combine(targetDirectory, "tokenizer_config.json");
        if (File.Exists(lastFilePath))
        {
            return; // Model already exists locally and downloaded completely.
        }

        Console.WriteLine($"[Foundation.AI] Downloading ONNX Model to {targetDirectory}...");

        foreach (var file in config.FilesToDownload)
        {
            string url = $"https://huggingface.co/{config.RepoId}/resolve/{config.Branch}/{config.Subfolder}/{file}";
            string destination = Path.Combine(targetDirectory, file);

            Console.WriteLine($"Downloading: {file} ...");
            try
            {
                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
                response.EnsureSuccessStatusCode();

                using var contentStream = await response.Content.ReadAsStreamAsync(ct);
                using var fileStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
                
                await contentStream.CopyToAsync(fileStream, ct);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to download {file} from {url}", ex);
            }
        }

        Console.WriteLine("[Foundation.AI] ONNX Model download complete.");
    }
}
