//
// Escalation Worker
//
// Background job that processes pending escalations on a recurring schedule.
//
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Alerting.Server.Services
{
    /// <summary>
    /// Background service that processes escalations on a recurring interval.
    /// </summary>
    public class EscalationWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EscalationWorker> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);

        public EscalationWorker(IServiceProvider serviceProvider, ILogger<EscalationWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EscalationWorker starting with {Interval}s interval", _interval.TotalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessEscalationsAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in EscalationWorker");
                }

                await Task.Delay(_interval, stoppingToken).ConfigureAwait(false);
            }

            _logger.LogInformation("EscalationWorker stopping");
        }

        private async Task ProcessEscalationsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var escalationService = scope.ServiceProvider.GetRequiredService<IEscalationService>();

            var processed = await escalationService.ProcessPendingEscalationsAsync().ConfigureAwait(false);

            if (processed > 0)
            {
                _logger.LogDebug("Processed {Count} escalations", processed);
            }
        }
    }
}
