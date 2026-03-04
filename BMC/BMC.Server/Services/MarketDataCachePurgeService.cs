using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Foundation.BMC.Services
{
    /// <summary>
    /// Background service that periodically purges expired cache entries from the
    /// MarketDataCache table to keep the table lean.
    ///
    /// Runs on a configurable interval (default: every 6 hours).
    /// Uses IServiceScopeFactory to create scoped instances since BackgroundService
    /// runs as a singleton.
    ///
    /// AI-Developed — This file was significantly developed with AI assistance.
    /// </summary>
    public class MarketDataCachePurgeService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MarketDataCachePurgeService> _logger;
        private readonly int _intervalMinutes;


        public MarketDataCachePurgeService(
            IServiceScopeFactory scopeFactory,
            IOptions<MarketDataCacheOptions> options,
            ILogger<MarketDataCachePurgeService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _intervalMinutes = options.Value.PurgeIntervalMinutes;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Market data cache purge service started (interval: {Interval}min).", _intervalMinutes);

            // Initial delay to let the app fully start
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var cacheService = scope.ServiceProvider.GetRequiredService<MarketDataCacheService>();

                    int purged = await cacheService.PurgeExpiredAsync(stoppingToken);

                    if (purged > 0)
                    {
                        _logger.LogInformation("Cache purge completed: removed {Count} expired entries.", purged);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in market data cache purge loop.");
                }

                await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);
            }

            _logger.LogInformation("Market data cache purge service stopped.");
        }
    }
}
