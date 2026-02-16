//
// Session Cache Sync Worker
//
// Background service that keeps the local IndexedDB session cache in sync.
// Handles cleanup of expired sessions periodically.
//
// Located in Foundation.Web due to Foundation.IndexedDB dependency chain.
//
// AI-assisted development - February 2026
//
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Foundation.Web.Services
{
    /// <summary>
    /// Background worker that maintains the local session cache:
    /// 1. Periodically cleans up expired sessions from the local store.
    /// 2. Can be extended for bidirectional sync if needed.
    /// </summary>
    public class SessionCacheSyncWorker : BackgroundService
    {
        private readonly ISessionEventBuffer _buffer;
        private readonly ILogger<SessionCacheSyncWorker> _logger;

        /// <summary>
        /// How often to clean up expired sessions (default: 60 seconds).
        /// </summary>
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromSeconds(60);

        public SessionCacheSyncWorker(
            ISessionEventBuffer buffer,
            ILogger<SessionCacheSyncWorker> logger)
        {
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SessionCacheSyncWorker started. Cleanup interval: {Interval}s",
                _cleanupInterval.TotalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_cleanupInterval, stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                try
                {
                    //
                    // Remove sessions that have expired (with a 1-hour grace period)
                    //
                    await _buffer.CleanupExpiredSessionsAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "SessionCacheSyncWorker cleanup cycle failed.");
                }
            }

            _logger.LogInformation("SessionCacheSyncWorker stopped.");
        }
    }
}
