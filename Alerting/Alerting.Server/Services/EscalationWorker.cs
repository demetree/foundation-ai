//
// Escalation Worker
//
// Background job that processes pending escalations on a recurring schedule.
//
using System;
using System.Threading;
using System.Threading.Tasks;
using Alerting.Server.Services.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Alerting.Server.Services
{
    /// <summary>
    /// Background service that processes escalations on a recurring interval.
    /// </summary>
    public class EscalationWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EscalationWorker> _logger;
        private readonly TimeSpan _interval;

        public EscalationWorker(
            IServiceProvider serviceProvider, 
            ILogger<EscalationWorker> logger,
            IOptions<NotificationEngineOptions> options)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _interval = TimeSpan.FromSeconds(options.Value.EscalationWorkerIntervalSeconds);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            NotificationLogger.System($"EscalationWorker starting with {_interval.TotalSeconds}s interval");
            _logger.LogInformation("EscalationWorker starting with {Interval}s interval", _interval.TotalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessEscalationsAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    NotificationLogger.Exception("Error in EscalationWorker", ex);
                    _logger.LogError(ex, "Error in EscalationWorker");
                }

                await Task.Delay(_interval, stoppingToken).ConfigureAwait(false);
            }

            NotificationLogger.System("EscalationWorker stopping");
            _logger.LogInformation("EscalationWorker stopping");
        }

        private async Task ProcessEscalationsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var escalationService = scope.ServiceProvider.GetRequiredService<IEscalationService>();

            var processed = await escalationService.ProcessPendingEscalationsAsync().ConfigureAwait(false);

            // Always update worker status for flight control (even if nothing processed)
            NotificationFlightControlService.UpdateEscalationWorkerStatus(DateTime.UtcNow, processed);

            if (processed > 0)
            {
                NotificationLogger.Info($"Processed {processed} escalations");
                _logger.LogDebug("Processed {Count} escalations", processed);
            }
        }
    }
}

