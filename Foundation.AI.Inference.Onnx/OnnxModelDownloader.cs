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
    /// Downloads the Microsoft Phi-3-mini-4k-instruct ONNX Int4 CPU variant
    /// from HuggingFace if the local path is empty.
    /// </summary>
    public async Task EnsureModelExistsAsync(string targetDirectory, CancellationToken ct = default)
    {
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

        string repoId = "microsoft/Phi-3-mini-4k-instruct-onnx";
        string branch = "main";
        string subfolder = "cpu_and_mobile/cpu-int4-rtn-block-32-acc-level-4";

        var filesToDownload = new[]
        {
            "added_tokens.json",
            "genai_config.json",
            "phi3-mini-4k-instruct-cpu-int4-rtn-block-32-acc-level-4.onnx",
            "phi3-mini-4k-instruct-cpu-int4-rtn-block-32-acc-level-4.onnx.data",
            "special_tokens_map.json",
            "tokenizer.json",
            "tokenizer_config.json"
        };

        foreach (var file in filesToDownload)
        {
            string url = $"https://huggingface.co/{repoId}/resolve/{branch}/{subfolder}/{file}";
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
