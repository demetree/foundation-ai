using System;
using System.Threading;
using System.Threading.Tasks;
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
        // Execute download on startup
        await _downloader.EnsureModelExistsAsync(_targetPath, _modelConfig, cancellationToken);

        // Pre-load the ONNX Inference matrix into RAM immediately at startup.
        // Since it is registered as a Singleton, resolving it here will trigger its constructor.
        Console.WriteLine("[Foundation.AI] Pre-loading inference model into RAM...");
        var _ = _serviceProvider.GetRequiredService<IInferenceProvider>();
        Console.WriteLine("[Foundation.AI] Inference model successfully loaded.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
