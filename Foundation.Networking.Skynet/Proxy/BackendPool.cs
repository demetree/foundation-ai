// ============================================================================
//
// BackendPool.cs — Backend server pool with health tracking.
//
// Manages a collection of backend servers, tracks their health status,
// and provides selection methods for proxy routing.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Foundation.Networking.Skynet.Configuration;

namespace Foundation.Networking.Skynet.Proxy
{
    /// <summary>
    /// Health status of a backend server.
    /// </summary>
    public enum BackendHealthStatus
    {
        Unknown,
        Healthy,
        Unhealthy,
        Disabled
    }


    /// <summary>
    /// Runtime state of a backend server.
    /// </summary>
    public class BackendState
    {
        /// <summary>
        /// Backend configuration.
        /// </summary>
        public BackendServer Config { get; set; }

        /// <summary>
        /// Current health status.
        /// </summary>
        public BackendHealthStatus Status { get; set; } = BackendHealthStatus.Unknown;

        /// <summary>
        /// Active connection count.
        /// </summary>
        public int ActiveConnections { get; set; }

        /// <summary>
        /// Total requests served.
        /// </summary>
        public long TotalRequests { get; set; }

        /// <summary>
        /// Total errors encountered.
        /// </summary>
        public long TotalErrors { get; set; }

        /// <summary>
        /// Last health check timestamp.
        /// </summary>
        public DateTime LastHealthCheckUtc { get; set; }

        /// <summary>
        /// Last error message, if unhealthy.
        /// </summary>
        public string LastError { get; set; } = string.Empty;
    }


    /// <summary>
    ///
    /// Manages a pool of backend servers with health checking.
    ///
    /// </summary>
    public class BackendPool : IDisposable
    {
        private readonly SkynetConfiguration _config;
        private readonly ILogger<BackendPool> _logger;
        private readonly List<BackendState> _backends;
        private readonly object _lock = new object();

        private Timer _healthCheckTimer;
        private int _roundRobinIndex = 0;
        private bool _disposed = false;


        public BackendPool(SkynetConfiguration config, ILogger<BackendPool> logger)
        {
            _config = config;
            _logger = logger;
            _backends = new List<BackendState>();

            //
            // Initialize backend states
            //
            foreach (BackendServer server in config.Backends)
            {
                _backends.Add(new BackendState
                {
                    Config = server,
                    Status = server.Enabled ? BackendHealthStatus.Unknown : BackendHealthStatus.Disabled
                });
            }
        }


        /// <summary>
        /// All backend states.
        /// </summary>
        public List<BackendState> AllBackends
        {
            get
            {
                lock (_lock)
                {
                    return _backends.ToList();
                }
            }
        }


        /// <summary>
        /// Number of healthy backends.
        /// </summary>
        public int HealthyCount
        {
            get
            {
                lock (_lock)
                {
                    return _backends.Count(b => b.Status == BackendHealthStatus.Healthy);
                }
            }
        }


        /// <summary>
        /// Total backend count.
        /// </summary>
        public int TotalCount => _backends.Count;


        /// <summary>
        /// Starts periodic health checking.
        /// </summary>
        public void StartHealthChecks(int intervalSeconds = 30)
        {
            _healthCheckTimer = new Timer(
                async _ => await RunHealthChecksAsync(),
                null,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(intervalSeconds));
        }


        /// <summary>
        /// Selects the next healthy backend using round-robin.
        /// Returns null if no healthy backends are available.
        /// </summary>
        public BackendState SelectBackend()
        {
            lock (_lock)
            {
                List<BackendState> healthy = _backends
                    .Where(b => b.Status == BackendHealthStatus.Healthy || b.Status == BackendHealthStatus.Unknown)
                    .Where(b => b.Config.Enabled == true)
                    .ToList();

                if (healthy.Count == 0)
                {
                    return null;
                }

                int index = _roundRobinIndex % healthy.Count;
                _roundRobinIndex++;

                BackendState selected = healthy[index];
                selected.ActiveConnections++;
                selected.TotalRequests++;

                return selected;
            }
        }


        /// <summary>
        /// Releases a connection back to the pool.
        /// </summary>
        public void ReleaseConnection(BackendState backend)
        {
            lock (_lock)
            {
                if (backend.ActiveConnections > 0)
                {
                    backend.ActiveConnections--;
                }
            }
        }


        /// <summary>
        /// Records an error for a backend.
        /// </summary>
        public void RecordError(BackendState backend, string error)
        {
            lock (_lock)
            {
                backend.TotalErrors++;
                backend.LastError = error;
            }
        }


        /// <summary>
        /// Marks a backend as healthy or unhealthy.
        /// </summary>
        public void SetHealth(BackendState backend, BackendHealthStatus status)
        {
            lock (_lock)
            {
                backend.Status = status;
                backend.LastHealthCheckUtc = DateTime.UtcNow;
            }
        }


        // ── Health Checks ─────────────────────────────────────────────────


        /// <summary>
        /// Runs health checks against all backends.
        /// </summary>
        public async Task RunHealthChecksAsync()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                foreach (BackendState backend in _backends)
                {
                    if (backend.Config.Enabled == false)
                    {
                        continue;
                    }

                    try
                    {
                        string healthUrl = backend.Config.Url.TrimEnd('/') + backend.Config.HealthCheckPath;
                        HttpResponseMessage response = await httpClient.GetAsync(healthUrl);

                        if (response.IsSuccessStatusCode == true)
                        {
                            SetHealth(backend, BackendHealthStatus.Healthy);
                        }
                        else
                        {
                            SetHealth(backend, BackendHealthStatus.Unhealthy);
                            RecordError(backend, "Health check returned " + (int)response.StatusCode);
                        }
                    }
                    catch (Exception ex)
                    {
                        SetHealth(backend, BackendHealthStatus.Unhealthy);
                        RecordError(backend, ex.Message);

                        _logger.LogWarning(
                            "Skynet: backend '{label}' ({url}) health check failed: {error}",
                            backend.Config.Label, backend.Config.Url, ex.Message);
                    }
                }
            }
        }


        // ── IDisposable ───────────────────────────────────────────────────


        public void Dispose()
        {
            if (_disposed == false)
            {
                _disposed = true;

                if (_healthCheckTimer != null)
                {
                    _healthCheckTimer.Dispose();
                    _healthCheckTimer = null;
                }
            }
        }
    }
}
