//
// IpAddressLocationWorker — Background service that resolves IP geolocation for login attempts
//
// This hosted service runs on a timer (every 30 seconds) and processes LoginAttempt records
// that have an IP address but no linked IpAddressLocation record.  It is completely decoupled
// from the login flow, ensuring zero impact on authentication performance.
//
// The worker delegates to IpAddressLocationManager which handles cache-table-first lookups
// and provider calls.
//
// AI-assisted development - February 2026
//

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.Services
{
    /// <summary>
    ///
    /// Background worker that periodically resolves IP addresses on LoginAttempt records
    /// to geographic locations.
    ///
    /// Runs every 30 seconds and processes up to 40 unique IPs per cycle to stay within
    /// the ip-api.com free tier rate limit of 45 requests/minute.
    ///
    /// This service is fully decoupled from the login path.  Login attempts are recorded
    /// as normal (with ipAddressLocationId = NULL), and this worker fills in the geo data
    /// asynchronously.
    ///
    /// </summary>
    public class IpAddressLocationWorker : BackgroundService
    {
        //
        // How often to check for unlinked records (30 seconds)
        //
        private const int CYCLE_INTERVAL_SECONDS = 30;

        //
        // Maximum unique IPs to process per cycle (respects ip-api.com 45/min rate limit)
        //
        private const int MAX_IPS_PER_CYCLE = 40;

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<IpAddressLocationWorker> _logger;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceProvider">Service provider for creating scoped dependencies</param>
        /// <param name="logger">Logger for recording worker activity</param>
        public IpAddressLocationWorker(
            IServiceProvider serviceProvider,
            ILogger<IpAddressLocationWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }


        /// <summary>
        ///
        /// Main execution loop.  Runs until the application shuts down.
        ///
        /// Each cycle:
        /// 1. Creates a new DI scope for the IpAddressLocationManager
        /// 2. Calls ResolveUnlinkedAttemptsAsync to process up to MAX_IPS_PER_CYCLE unique IPs
        /// 3. Waits CYCLE_INTERVAL_SECONDS before the next cycle
        ///
        /// All exceptions are caught and logged — the worker never crashes the host process.
        ///
        /// </summary>
        /// <param name="stoppingToken">Cancellation token signaling application shutdown</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("IpAddressLocationWorker started.  Cycle interval: {Interval}s, Max IPs per cycle: {Max}",
                                   CYCLE_INTERVAL_SECONDS,
                                   MAX_IPS_PER_CYCLE);

            //
            // Wait a short time after startup before the first cycle.  This lets the application
            // fully initialize (database connections, etc.) before we start querying.
            //
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (stoppingToken.IsCancellationRequested == false)
            {
                try
                {
                    await ProcessCycleAsync();
                }
                catch (Exception ex)
                {
                    //
                    // Log and continue — the worker must never crash
                    //
                    _logger.LogError(ex, "IpAddressLocationWorker encountered an error during processing cycle");
                }

                //
                // Wait for the next cycle
                //
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(CYCLE_INTERVAL_SECONDS), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    //
                    // Application is shutting down — exit gracefully
                    //
                    break;
                }
            }

            _logger.LogInformation("IpAddressLocationWorker stopped");
        }


        /// <summary>
        /// Processes a single cycle of unlinked LoginAttempt resolution.
        /// Creates a scoped IpAddressLocationManager and delegates the batch processing to it.
        /// </summary>
        private async Task ProcessCycleAsync()
        {
            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                IpAddressLocationManager manager = scope.ServiceProvider.GetRequiredService<IpAddressLocationManager>();

                int linkedCount = await manager.ResolveUnlinkedAttemptsAsync(maxRecords: MAX_IPS_PER_CYCLE);

                if (linkedCount > 0)
                {
                    _logger.LogDebug("IpAddressLocationWorker cycle complete.  Linked {Count} records", linkedCount);
                }
            }
        }
    }
}
