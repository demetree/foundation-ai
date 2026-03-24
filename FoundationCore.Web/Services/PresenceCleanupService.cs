using Foundation.Messaging.Services;
using Foundation.HubConfig;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace Foundation.Messaging.Hosting
{
    /// <summary>
    /// 
    /// Background service that periodically cleans up stale presence records.
    /// 
    /// If a user's network drops without a clean TCP close (laptop sleep, WiFi loss, power loss),
    /// SignalR's disconnect detection may not fire and the user remains stuck as "Online".
    /// 
    /// This service runs every 2 minutes and marks users as Offline when their heartbeat
    /// (lastActivityDateTime) has not been updated within the stale threshold (3 minutes).
    /// 
    /// Each module (Catalyst, Basecamp) registers this as a hosted service in their startup.
    /// 
    /// </summary>
    public class PresenceCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<MessagingHub, IMessagingHub> _hubContext;
        private readonly ILogger<PresenceCleanupService> _logger;

        private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(2);
        private const int STALE_THRESHOLD_MINUTES = 3;


        public PresenceCleanupService(
            IServiceScopeFactory scopeFactory,
            IHubContext<MessagingHub, IMessagingHub> hubContext,
            ILogger<PresenceCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PresenceCleanupService started. Sweep interval: {Interval}, Stale threshold: {Threshold} minutes.",
                SweepInterval, STALE_THRESHOLD_MINUTES);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(SweepInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                try
                {
                    await RunCleanupSweepAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "PresenceCleanupService: Error during stale presence cleanup sweep.");
                }
            }

            _logger.LogInformation("PresenceCleanupService stopped.");
        }


        private async Task RunCleanupSweepAsync()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var presenceService = scope.ServiceProvider.GetRequiredService<PresenceService>();

                var staleUsers = await presenceService.CleanupStalePresenceAsync(STALE_THRESHOLD_MINUTES);

                if (staleUsers.Count == 0)
                {
                    return;
                }

                //
                // Broadcast PresenceChanged for each transitioned user, grouped by tenant
                //
                int totalTransitioned = 0;

                foreach (var (tenantGuid, users) in staleUsers)
                {
                    string tenantGroup = $"tenant:{tenantGuid}";

                    foreach (var user in users)
                    {
                        await _hubContext.Clients.Group(tenantGroup).PresenceChanged(new PresencePayload
                        {
                            userId = user.userId,
                            userDisplayName = user.displayName,
                            status = user.status,
                            customStatusMessage = user.customStatusMessage
                        });

                        totalTransitioned++;
                    }
                }

                _logger.LogInformation("PresenceCleanupService: Transitioned {Count} stale user(s) to Offline.", totalTransitioned);
            }
        }
    }
}
