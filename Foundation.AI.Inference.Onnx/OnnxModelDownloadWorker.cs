using System;
using System.Threading;
using System.Threading.Tasks;
using Foundation.AI.Inference;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.AI.Inference.Onnx;

public class OnnxModelDownloadWorker : IHostedService
{
    private readonly OnnxModelDownloader _downloader;
    private readonly string _targetPath;
    private readonly OnnxModelConfig _modelConfig;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OnnxModelDownloadWorker> _logger;

    public OnnxModelDownloadWorker(OnnxModelDownloader downloader, string targetPath, OnnxModelConfig modelConfig, IServiceProvider serviceProvider)
    {
        _downloader = downloader;
        _targetPath = targetPath;
        _modelConfig = modelConfig;
        _serviceProvider = serviceProvider;

        //
        // Logger is resolved from the service provider rather than taken as a
        // constructor parameter so existing DI factories that already pass the
        // sp on their own keep working without signature changes. Falls back
        // to the null logger when nobody has wired an ILogger<T> (e.g. unit
        // tests constructing this directly).
        //
        _logger = serviceProvider.GetService<ILogger<OnnxModelDownloadWorker>>()
                  ?? NullLogger<OnnxModelDownloadWorker>.Instance;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _downloader.EnsureModelExistsAsync(_targetPath, _modelConfig, cancellationToken);

            _logger.LogInformation("Pre-loading inference model into RAM from {TargetPath}...", _targetPath);
            var provider = _serviceProvider.GetRequiredService<IInferenceProvider>();
            (provider as OnnxInferenceProvider)?.Preload();
            _logger.LogInformation("Inference model loaded from {TargetPath}.", _targetPath);
        }
        catch (Exception ex)
        {
            //
            // Swallow — the server should still boot even if the model isn't
            // available. AI features will surface the "not ready" banner. The
            // exception detail goes through the structured log so ops can see
            // it in whatever sink is wired (file, Seq, App Insights, etc.).
            //
            _logger.LogError(ex,
                "Failed to download or load inference model. AI chat features will be unavailable until the model is present at {TargetPath}.",
                _targetPath);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
