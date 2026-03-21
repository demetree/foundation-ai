// ============================================================================
//
// HealthProber.cs — Background health check service for backends.
//
// Periodically probes each backend's health check endpoint and updates
// the LoadBalancer's health status accordingly.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Foundation.Networking.Switchboard.Balancing;
using Foundation.Networking.Switchboard.Configuration;

namespace Foundation.Networking.Switchboard.Health
{
    /// <summary>
    /// Health check result for a single backend.
    /// </summary>
    public class HealthCheckResult
    {
        public string BackendId { get; set; } = string.Empty;
        public string BackendLabel { get; set; } = string.Empty;
        public bool IsHealthy { get; set; }
        public int StatusCode { get; set; }
        public long ResponseTimeMs { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime CheckedAtUtc { get; set; } = DateTime.UtcNow;
        public int ConsecutiveFailures { get; set; }
        public int ConsecutiveSuccesses { get; set; }
    }


    /// <summary>
    ///
    /// Background service that periodically health-checks all backends
    /// and updates the LoadBalancer's health state.
    ///
    /// </summary>
    public class HealthProber : IHostedService, IDisposable
    {
        private readonly SwitchboardConfiguration _config;
        private readonly LoadBalancer _loadBalancer;
        private readonly ILogger<HealthProber> _logger;

        private readonly ConcurrentDictionary<string, HealthCheckResult> _lastResults;
        private readonly ConcurrentDictionary<string, int> _consecutiveFailures;
        private readonly ConcurrentDictionary<string, int> _consecutiveSuccesses;

        private Timer _healthTimer;
        private bool _disposed = false;


        public HealthProber(
            SwitchboardConfiguration config,
            LoadBalancer loadBalancer,
            ILogger<HealthProber> logger)
        {
            _config = config;
            _loadBalancer = loadBalancer;
            _logger = logger;
            _lastResults = new ConcurrentDictionary<string, HealthCheckResult>();
            _consecutiveFailures = new ConcurrentDictionary<string, int>();
            _consecutiveSuccesses = new ConcurrentDictionary<string, int>();
        }


        /// <summary>
        /// Whether the prober is running.
        /// </summary>
        public bool IsRunning { get; private set; }


        // ── IHostedService ────────────────────────────────────────────────


        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_loadBalancer.TotalBackends == 0)
            {
                _logger.LogInformation("Switchboard health prober has no backends to monitor");
                return Task.CompletedTask;
            }

            int intervalMs = _config.HealthCheckIntervalSeconds * 1000;

            _healthTimer = new Timer(
                async _ => await RunHealthChecksAsync(),
                null,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromMilliseconds(intervalMs));

            IsRunning = true;

            _logger.LogInformation(
                "Switchboard health prober started — checking {count} backends every {interval}s",
                _loadBalancer.TotalBackends,
                _config.HealthCheckIntervalSeconds);

            return Task.CompletedTask;
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            IsRunning = false;

            if (_healthTimer != null)
            {
                _healthTimer.Change(Timeout.Infinite, 0);
            }

            _logger.LogInformation("Switchboard health prober stopped");

            return Task.CompletedTask;
        }


        // ── Health Checks ─────────────────────────────────────────────────


        /// <summary>
        /// Runs a health check against all backends.
        /// </summary>
        public async Task RunHealthChecksAsync()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromMilliseconds(_config.HealthCheckTimeoutMs);

                foreach (BackendNode backend in _loadBalancer.GetBackends())
                {
                    await CheckBackendAsync(httpClient, backend);
                }
            }
        }


        private async Task CheckBackendAsync(HttpClient httpClient, BackendNode backend)
        {
            HealthCheckResult result = new HealthCheckResult
            {
                BackendId = backend.Id,
                BackendLabel = backend.Label,
                CheckedAtUtc = DateTime.UtcNow
            };

            SwitchboardBackend backendConfig = null;

            foreach (SwitchboardBackend cfg in _config.Backends)
            {
                if (cfg.Id == backend.Id)
                {
                    backendConfig = cfg;
                    break;
                }
            }

            string healthPath = backendConfig?.HealthCheckPath ?? "/api/health";
            string healthUrl = backend.Url.TrimEnd('/') + healthPath;

            try
            {
                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
                HttpResponseMessage response = await httpClient.GetAsync(healthUrl);
                sw.Stop();

                result.StatusCode = (int)response.StatusCode;
                result.ResponseTimeMs = sw.ElapsedMilliseconds;

                if (response.IsSuccessStatusCode == true)
                {
                    result.IsHealthy = true;

                    //
                    // Track consecutive successes
                    //
                    _consecutiveFailures[backend.Id] = 0;
                    int successes = _consecutiveSuccesses.AddOrUpdate(backend.Id, 1, (_, v) => v + 1);
                    result.ConsecutiveSuccesses = successes;

                    //
                    // Re-enable backend if it hits the healthy threshold
                    //
                    if (successes >= _config.HealthyThreshold)
                    {
                        if (backend.IsHealthy == false)
                        {
                            _logger.LogInformation(
                                "Switchboard: backend '{label}' is now HEALTHY after {count} consecutive successes",
                                backend.Label, successes);
                        }

                        _loadBalancer.SetHealth(backend.Id, true);
                    }
                }
                else
                {
                    HandleFailure(result, backend, "HTTP " + result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                HandleFailure(result, backend, ex.Message);
            }

            _lastResults[backend.Id] = result;
        }


        private void HandleFailure(HealthCheckResult result, BackendNode backend, string error)
        {
            result.IsHealthy = false;
            result.ErrorMessage = error;

            _consecutiveSuccesses[backend.Id] = 0;
            int failures = _consecutiveFailures.AddOrUpdate(backend.Id, 1, (_, v) => v + 1);
            result.ConsecutiveFailures = failures;

            //
            // Mark backend as unhealthy if it hits the threshold
            //
            if (failures >= _config.UnhealthyThreshold)
            {
                if (backend.IsHealthy == true)
                {
                    _logger.LogWarning(
                        "Switchboard: backend '{label}' marked UNHEALTHY after {count} consecutive failures: {error}",
                        backend.Label, failures, error);
                }

                _loadBalancer.SetHealth(backend.Id, false);
            }
        }


        // ── Query ─────────────────────────────────────────────────────────


        /// <summary>
        /// Gets the last health check result for all backends.
        /// </summary>
        public List<HealthCheckResult> GetResults()
        {
            return new List<HealthCheckResult>(_lastResults.Values);
        }


        /// <summary>
        /// Gets the last health check result for a specific backend.
        /// </summary>
        public HealthCheckResult GetResult(string backendId)
        {
            if (_lastResults.TryGetValue(backendId, out HealthCheckResult result))
            {
                return result;
            }

            return null;
        }


        // ── IDisposable ───────────────────────────────────────────────────


        public void Dispose()
        {
            if (_disposed == false)
            {
                _disposed = true;

                if (_healthTimer != null)
                {
                    _healthTimer.Dispose();
                    _healthTimer = null;
                }
            }
        }
    }
}
