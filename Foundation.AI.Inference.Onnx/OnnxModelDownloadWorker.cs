using System;
using System.Threading;
using System.Threading.Tasks;
using Foundation.AI.Inference;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Foundation.AI.Inference.Onnx;

public class OnnxModelDownloadWorker : IHostedService
{
    private readonly OnnxModelDownloader _downloader;
    private readonly string _targetPath;
    private readonly OnnxModelConfig _modelConfig;
    private readonly IServiceProvider _serviceProvider;

    public OnnxModelDownloadWorker(OnnxModelDownloader downloader, string targetPath, OnnxModelConfig modelConfig, IServiceProvider serviceProvider)
    {
        _downloader = downloader;
        _targetPath = targetPath;
        _modelConfig = modelConfig;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _downloader.EnsureModelExistsAsync(_targetPath, _modelConfig, cancellationToken);

            Console.WriteLine("[Foundation.AI] Pre-loading inference model into RAM...");
            var provider = _serviceProvider.GetRequiredService<IInferenceProvider>();
            (provider as OnnxInferenceProvider)?.Preload();
            Console.WriteLine("[Foundation.AI] Inference model successfully loaded.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Foundation.AI] CRITICAL: Failed to download or load inference model. AI chat features will be unavailable until the model is present at '{_targetPath}'. Error: {ex.Message}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
