// ============================================================================
//
// LatencyMonitorService.cs — Background latency monitoring service.
//
// Runs as an IHostedService, periodically pinging configured endpoints
// and maintaining a rolling history of latency records.  Provides
// summaries for the admin dashboard.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Foundation.Networking.Watchtower.Configuration;

namespace Foundation.Networking.Watchtower.Services
{
    /// <summary>
    ///
    /// Background service that periodically pings monitored endpoints
    /// and maintains a rolling latency history.
    ///
    /// Implements IHostedService for automatic lifecycle management
    /// in the ASP.NET Core host.
    ///
    /// </summary>
    public class LatencyMonitorService : IHostedService, IDisposable
    {
        private readonly WatchtowerConfiguration _config;
        private readonly PingService _pingService;
        private readonly ILogger<LatencyMonitorService> _logger;

        //
        // Rolling history per endpoint (keyed by host)
        //
        private readonly ConcurrentDictionary<string, ConcurrentQueue<LatencyRecord>> _history;

        //
        // Background timer
        //
        private Timer _monitorTimer;
        private bool _disposed = false;


        public LatencyMonitorService(
            WatchtowerConfiguration config,
            PingService pingService,
            ILogger<LatencyMonitorService> logger)
        {
            _config = config;
            _pingService = pingService;
            _logger = logger;
            _history = new ConcurrentDictionary<string, ConcurrentQueue<LatencyRecord>>();
        }


        /// <summary>
        /// Whether the monitor is currently running.
        /// </summary>
        public bool IsRunning { get; private set; }


        // ── IHostedService ────────────────────────────────────────────────


        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_config.LatencyMonitor.Enabled == false)
            {
                _logger.LogInformation("Watchtower latency monitor is disabled");
                return Task.CompletedTask;
            }

            if (_config.LatencyMonitor.Endpoints.Count == 0)
            {
                _logger.LogInformation("Watchtower latency monitor has no endpoints configured");
                return Task.CompletedTask;
            }

            //
            // Initialize history queues for each endpoint
            //
            foreach (MonitoredEndpoint endpoint in _config.LatencyMonitor.Endpoints)
            {
                _history.TryAdd(endpoint.Host, new ConcurrentQueue<LatencyRecord>());
            }

            //
            // Start the monitoring timer
            //
            int intervalMs = _config.LatencyMonitor.IntervalSeconds * 1000;

            _monitorTimer = new Timer(
                MonitorCallback,
                null,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromMilliseconds(intervalMs));

            IsRunning = true;

            _logger.LogInformation(
                "Watchtower latency monitor started — monitoring {count} endpoints every {interval}s",
                _config.LatencyMonitor.Endpoints.Count,
                _config.LatencyMonitor.IntervalSeconds);

            return Task.CompletedTask;
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            IsRunning = false;

            if (_monitorTimer != null)
            {
                _monitorTimer.Change(Timeout.Infinite, 0);
            }

            _logger.LogInformation("Watchtower latency monitor stopped");

            return Task.CompletedTask;
        }


        // ── Monitoring ────────────────────────────────────────────────────


        private async void MonitorCallback(object state)
        {
            try
            {
                foreach (MonitoredEndpoint endpoint in _config.LatencyMonitor.Endpoints)
                {
                    await MonitorEndpointAsync(endpoint);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during latency monitor sweep");
            }
        }


        private async Task MonitorEndpointAsync(MonitoredEndpoint endpoint)
        {
            try
            {
                PingResult pingResult = await _pingService.PingAsync(endpoint.Host);

                LatencyRecord record = new LatencyRecord
                {
                    Label = endpoint.Label,
                    Host = endpoint.Host,
                    RoundTripTimeMs = pingResult.Success ? pingResult.RoundTripTimeMs : -1,
                    IsReachable = pingResult.Success,
                    ExceedsThreshold = pingResult.Success && pingResult.RoundTripTimeMs > _config.LatencyMonitor.AlertThresholdMs,
                    TimestampUtc = DateTime.UtcNow
                };

                //
                // Add to history
                //
                ConcurrentQueue<LatencyRecord> queue = _history.GetOrAdd(endpoint.Host, _ => new ConcurrentQueue<LatencyRecord>());
                queue.Enqueue(record);

                //
                // Trim history to max size
                //
                while (queue.Count > _config.LatencyMonitor.MaxHistoryPerEndpoint)
                {
                    queue.TryDequeue(out _);
                }

                //
                // Log warnings
                //
                if (record.IsReachable == false)
                {
                    _logger.LogWarning("Watchtower: endpoint '{label}' ({host}) is unreachable", endpoint.Label, endpoint.Host);
                }
                else if (record.ExceedsThreshold == true)
                {
                    _logger.LogWarning(
                        "Watchtower: endpoint '{label}' ({host}) latency {rtt}ms exceeds threshold {threshold}ms",
                        endpoint.Label, endpoint.Host, record.RoundTripTimeMs, _config.LatencyMonitor.AlertThresholdMs);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoring endpoint '{label}' ({host})", endpoint.Label, endpoint.Host);
            }
        }


        // ── Query Methods ─────────────────────────────────────────────────


        /// <summary>
        /// Gets the latency summary for all monitored endpoints.
        /// </summary>
        public List<LatencyEndpointSummary> GetAllSummaries()
        {
            List<LatencyEndpointSummary> summaries = new List<LatencyEndpointSummary>();

            foreach (MonitoredEndpoint endpoint in _config.LatencyMonitor.Endpoints)
            {
                summaries.Add(GetEndpointSummary(endpoint.Host));
            }

            return summaries;
        }


        /// <summary>
        /// Gets the latency summary for a specific endpoint.
        /// </summary>
        public LatencyEndpointSummary GetEndpointSummary(string host)
        {
            LatencyEndpointSummary summary = new LatencyEndpointSummary
            {
                Host = host
            };

            //
            // Find the endpoint label
            //
            MonitoredEndpoint endpoint = _config.LatencyMonitor.Endpoints
                .FirstOrDefault(e => e.Host == host);

            if (endpoint != null)
            {
                summary.Label = endpoint.Label;
            }

            //
            // Get the history
            //
            if (_history.TryGetValue(host, out ConcurrentQueue<LatencyRecord> queue) == false || queue.IsEmpty == true)
            {
                return summary;
            }

            List<LatencyRecord> records = queue.ToList();
            summary.History = records;
            summary.RecordCount = records.Count;

            //
            // Current state (most recent record)
            //
            LatencyRecord latest = records.Last();
            summary.IsReachable = latest.IsReachable;
            summary.CurrentRttMs = latest.RoundTripTimeMs;

            //
            // Statistics from reachable records only
            //
            List<LatencyRecord> reachableRecords = records.Where(r => r.IsReachable == true).ToList();

            if (reachableRecords.Count > 0)
            {
                List<long> rttValues = reachableRecords.Select(r => r.RoundTripTimeMs).ToList();

                summary.MinRttMs = rttValues.Min();
                summary.MaxRttMs = rttValues.Max();
                summary.AverageRttMs = rttValues.Average();

                //
                // Jitter (standard deviation)
                //
                if (rttValues.Count > 1)
                {
                    double mean = summary.AverageRttMs;
                    double sumSquaredDiffs = 0;

                    foreach (long rtt in rttValues)
                    {
                        double diff = rtt - mean;
                        sumSquaredDiffs += diff * diff;
                    }

                    summary.JitterMs = Math.Sqrt(sumSquaredDiffs / rttValues.Count);
                }
            }

            //
            // Uptime percentage
            //
            summary.UptimePercent = records.Count > 0
                ? ((double)reachableRecords.Count / records.Count) * 100.0
                : 0;

            return summary;
        }


        /// <summary>
        /// Gets the raw latency history for a specific endpoint.
        /// </summary>
        public List<LatencyRecord> GetHistory(string host, int maxRecords = 100)
        {
            if (_history.TryGetValue(host, out ConcurrentQueue<LatencyRecord> queue))
            {
                List<LatencyRecord> records = queue.ToList();
                int skip = Math.Max(0, records.Count - maxRecords);
                return records.Skip(skip).ToList();
            }

            return new List<LatencyRecord>();
        }


        // ── IDisposable ───────────────────────────────────────────────────


        public void Dispose()
        {
            if (_disposed == false)
            {
                _disposed = true;

                if (_monitorTimer != null)
                {
                    _monitorTimer.Dispose();
                    _monitorTimer = null;
                }
            }
        }
    }
}
