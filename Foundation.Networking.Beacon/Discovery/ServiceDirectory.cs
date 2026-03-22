// ============================================================================
//
// ServiceDirectory.cs — Service discovery and registration.
//
// Allows Foundation services to register themselves and discover
// other services by name, with health tracking and stale removal.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Foundation.Networking.Beacon.Configuration;

namespace Foundation.Networking.Beacon.Discovery
{
    /// <summary>
    /// A discovered service endpoint.
    /// </summary>
    public class ServiceEndpoint
    {
        public string ServiceName { get; set; } = string.Empty;
        public string InstanceId { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Protocol { get; set; } = "http";
        public string HealthCheckPath { get; set; } = "/api/health";
        public bool IsHealthy { get; set; } = true;
        public DateTime RegisteredUtc { get; set; } = DateTime.UtcNow;
        public DateTime LastHeartbeatUtc { get; set; } = DateTime.UtcNow;
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Full URL of this endpoint.
        /// </summary>
        public string Url => Protocol + "://" + Host + ":" + Port;
    }


    /// <summary>
    /// Service directory statistics.
    /// </summary>
    public class DirectoryStatistics
    {
        public int TotalServices { get; set; }
        public int TotalEndpoints { get; set; }
        public int HealthyEndpoints { get; set; }
        public int UnhealthyEndpoints { get; set; }
    }


    /// <summary>
    ///
    /// Service directory for Foundation service discovery.
    ///
    /// Services register on startup and can be looked up by name.
    /// Includes health monitoring and automatic stale removal.
    ///
    /// </summary>
    public class ServiceDirectory : IHostedService, IDisposable
    {
        private readonly BeaconConfiguration _config;
        private readonly ILogger<ServiceDirectory> _logger;
        private readonly ConcurrentDictionary<string, ServiceEndpoint> _endpoints;

        private Timer _healthTimer;
        private Timer _staleTimer;
        private bool _disposed = false;


        public ServiceDirectory(BeaconConfiguration config, ILogger<ServiceDirectory> logger)
        {
            _config = config;
            _logger = logger;
            _endpoints = new ConcurrentDictionary<string, ServiceEndpoint>();
        }


        /// <summary>
        /// Total registered endpoints.
        /// </summary>
        public int EndpointCount => _endpoints.Count;


        // ── IHostedService ────────────────────────────────────────────────


        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_config.HealthCheckEnabled == true)
            {
                _healthTimer = new Timer(
                    async _ => await RunHealthChecksAsync(),
                    null,
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(_config.HealthCheckIntervalSeconds));
            }

            _staleTimer = new Timer(
                _ => RemoveStale(),
                null,
                TimeSpan.FromSeconds(30),
                TimeSpan.FromSeconds(30));

            _logger.LogInformation("Beacon service directory started");

            return Task.CompletedTask;
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            _healthTimer?.Change(Timeout.Infinite, 0);
            _staleTimer?.Change(Timeout.Infinite, 0);

            _logger.LogInformation("Beacon service directory stopped");

            return Task.CompletedTask;
        }


        // ── Registration ──────────────────────────────────────────────────


        /// <summary>
        /// Registers a service endpoint. Returns the instance ID.
        /// </summary>
        public string Register(ServiceEndpoint endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint.InstanceId))
            {
                endpoint.InstanceId = Guid.NewGuid().ToString("N");
            }

            endpoint.RegisteredUtc = DateTime.UtcNow;
            endpoint.LastHeartbeatUtc = DateTime.UtcNow;

            _endpoints[endpoint.InstanceId] = endpoint;

            _logger.LogInformation(
                "Beacon: registered {service} at {url} (instance: {id})",
                endpoint.ServiceName, endpoint.Url, endpoint.InstanceId.Substring(0, Math.Min(8, endpoint.InstanceId.Length)));

            return endpoint.InstanceId;
        }


        /// <summary>
        /// Deregisters a service endpoint.
        /// </summary>
        public bool Deregister(string instanceId)
        {
            return _endpoints.TryRemove(instanceId, out _);
        }


        /// <summary>
        /// Updates the heartbeat for an endpoint.
        /// </summary>
        public void Heartbeat(string instanceId)
        {
            if (_endpoints.TryGetValue(instanceId, out ServiceEndpoint endpoint))
            {
                endpoint.LastHeartbeatUtc = DateTime.UtcNow;
            }
        }


        // ── Discovery ─────────────────────────────────────────────────────


        /// <summary>
        /// Finds all healthy endpoints for a service name.
        /// </summary>
        public List<ServiceEndpoint> Discover(string serviceName)
        {
            return _endpoints.Values
                .Where(e => string.Equals(e.ServiceName, serviceName, StringComparison.OrdinalIgnoreCase))
                .Where(e => e.IsHealthy == true)
                .ToList();
        }


        /// <summary>
        /// Finds all endpoints (including unhealthy) for a service name.
        /// </summary>
        public List<ServiceEndpoint> DiscoverAll(string serviceName)
        {
            return _endpoints.Values
                .Where(e => string.Equals(e.ServiceName, serviceName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }


        /// <summary>
        /// Gets all unique service names.
        /// </summary>
        public List<string> GetServiceNames()
        {
            return _endpoints.Values
                .Select(e => e.ServiceName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }


        /// <summary>
        /// Gets a specific endpoint by instance ID.
        /// </summary>
        public ServiceEndpoint GetEndpoint(string instanceId)
        {
            _endpoints.TryGetValue(instanceId, out ServiceEndpoint endpoint);
            return endpoint;
        }


        /// <summary>
        /// Gets all registered endpoints.
        /// </summary>
        public List<ServiceEndpoint> GetAllEndpoints()
        {
            return _endpoints.Values.ToList();
        }


        // ── Statistics ────────────────────────────────────────────────────


        public DirectoryStatistics GetStatistics()
        {
            List<ServiceEndpoint> all = _endpoints.Values.ToList();

            return new DirectoryStatistics
            {
                TotalServices = all.Select(e => e.ServiceName).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                TotalEndpoints = all.Count,
                HealthyEndpoints = all.Count(e => e.IsHealthy == true),
                UnhealthyEndpoints = all.Count(e => e.IsHealthy == false)
            };
        }


        // ── Health / Stale ────────────────────────────────────────────────


        private async Task RunHealthChecksAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(5);

                foreach (ServiceEndpoint endpoint in _endpoints.Values)
                {
                    try
                    {
                        string healthUrl = endpoint.Url.TrimEnd('/') + endpoint.HealthCheckPath;
                        HttpResponseMessage response = await client.GetAsync(healthUrl);

                        endpoint.IsHealthy = response.IsSuccessStatusCode;
                    }
                    catch
                    {
                        endpoint.IsHealthy = false;
                    }
                }
            }
        }


        private void RemoveStale()
        {
            DateTime cutoff = DateTime.UtcNow.AddSeconds(-_config.DiscoveryStaleTimeoutSeconds);
            List<string> stale = new List<string>();

            foreach (var kvp in _endpoints)
            {
                if (kvp.Value.LastHeartbeatUtc < cutoff)
                {
                    stale.Add(kvp.Key);
                }
            }

            foreach (string id in stale)
            {
                if (_endpoints.TryRemove(id, out ServiceEndpoint removed))
                {
                    _logger.LogInformation(
                        "Beacon: removed stale endpoint {service} (instance: {id})",
                        removed.ServiceName, id.Substring(0, Math.Min(8, id.Length)));
                }
            }
        }


        // ── IDisposable ───────────────────────────────────────────────────


        public void Dispose()
        {
            if (_disposed == false)
            {
                _disposed = true;
                _healthTimer?.Dispose();
                _staleTimer?.Dispose();
            }
        }
    }
}
