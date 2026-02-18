using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BMC.AI;

/// <summary>
/// Background service that indexes BMC parts and sets into the vector store
/// on application startup. Runs once, then stops.
///
/// Uses <see cref="IServiceScopeFactory"/> because <see cref="BmcSearchIndex"/>
/// is a scoped service (depends on BMCContext).
///
/// AI-DEVELOPED: Auto-indexes on startup so the AI assistant is ready immediately.
/// </summary>
public sealed class BmcAiIndexingService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BmcAiIndexingService> _logger;

    /// <summary>
    /// Delay before starting indexing, to let the app finish bootstrapping.
    /// </summary>
    private static readonly TimeSpan StartupDelay = TimeSpan.FromSeconds(10);

    public BmcAiIndexingService(
        IServiceScopeFactory scopeFactory,
        ILogger<BmcAiIndexingService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait a bit so the app is fully ready before we start hitting the DB + embeddings API
        await Task.Delay(StartupDelay, stoppingToken);

        _logger.LogInformation("BMC AI Indexing: starting automatic index build...");

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var searchIndex = scope.ServiceProvider.GetRequiredService<BmcSearchIndex>();

            await searchIndex.IndexAllAsync(ct: stoppingToken);

            _logger.LogInformation("BMC AI Indexing: complete — vector store is ready.");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("BMC AI Indexing: cancelled during shutdown.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BMC AI Indexing: failed — AI search/chat will not work until indexing succeeds.");
        }
    }
}
