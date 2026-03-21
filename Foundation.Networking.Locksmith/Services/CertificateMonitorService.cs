// ============================================================================
//
// CertificateMonitorService.cs — Background certificate expiry monitoring.
//
// Runs as an IHostedService, periodically inspecting TLS certificates on
// configured endpoints and logging warnings/critical alerts for approaching
// expiry.
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

using Foundation.Networking.Locksmith.Configuration;

namespace Foundation.Networking.Locksmith.Services
{
    /// <summary>
    ///
    /// Background service that periodically inspects TLS certificates on
    /// monitored endpoints and raises alerts for approaching expiry.
    ///
    /// </summary>
    public class CertificateMonitorService : IHostedService, IDisposable
    {
        private readonly LocksmithConfiguration _config;
        private readonly CertificateInspector _inspector;
        private readonly ILogger<CertificateMonitorService> _logger;

        //
        // Cached status per endpoint (keyed by "host:port")
        //
        private readonly ConcurrentDictionary<string, CertificateEndpointStatus> _statuses;

        //
        // Background timer
        //
        private Timer _monitorTimer;
        private bool _disposed = false;


        public CertificateMonitorService(
            LocksmithConfiguration config,
            CertificateInspector inspector,
            ILogger<CertificateMonitorService> logger)
        {
            _config = config;
            _inspector = inspector;
            _logger = logger;
            _statuses = new ConcurrentDictionary<string, CertificateEndpointStatus>();
        }


        /// <summary>
        /// Whether the monitor is currently running.
        /// </summary>
        public bool IsRunning { get; private set; }


        // ── IHostedService ────────────────────────────────────────────────


        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_config.Monitor.Enabled == false)
            {
                _logger.LogInformation("Locksmith certificate monitor is disabled");
                return Task.CompletedTask;
            }

            if (_config.Monitor.Endpoints.Count == 0)
            {
                _logger.LogInformation("Locksmith certificate monitor has no endpoints configured");
                return Task.CompletedTask;
            }

            //
            // Start the monitoring timer
            //
            int intervalMs = _config.Monitor.IntervalHours * 60 * 60 * 1000;

            _monitorTimer = new Timer(
                MonitorCallback,
                null,
                TimeSpan.FromSeconds(10),
                TimeSpan.FromMilliseconds(intervalMs));

            IsRunning = true;

            _logger.LogInformation(
                "Locksmith certificate monitor started — monitoring {count} endpoints every {interval}h",
                _config.Monitor.Endpoints.Count,
                _config.Monitor.IntervalHours);

            return Task.CompletedTask;
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            IsRunning = false;

            if (_monitorTimer != null)
            {
                _monitorTimer.Change(Timeout.Infinite, 0);
            }

            _logger.LogInformation("Locksmith certificate monitor stopped");

            return Task.CompletedTask;
        }


        // ── Monitoring ────────────────────────────────────────────────────


        private async void MonitorCallback(object state)
        {
            try
            {
                foreach (CertificateEndpoint endpoint in _config.Monitor.Endpoints)
                {
                    await CheckEndpointAsync(endpoint);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during certificate monitor sweep");
            }
        }


        private async Task CheckEndpointAsync(CertificateEndpoint endpoint)
        {
            string key = endpoint.Host + ":" + endpoint.Port;

            CertificateEndpointStatus status = new CertificateEndpointStatus
            {
                Label = endpoint.Label,
                Host = endpoint.Host,
                Port = endpoint.Port,
                LastCheckedUtc = DateTime.UtcNow
            };

            try
            {
                CertificateInfo info = await _inspector.InspectAsync(endpoint.Host, endpoint.Port);

                if (info.Success == true)
                {
                    status.Subject = info.Subject;
                    status.Issuer = info.Issuer;
                    status.NotAfterUtc = info.NotAfterUtc;
                    status.DaysUntilExpiry = info.DaysUntilExpiry;

                    //
                    // Determine health status
                    //
                    if (info.IsExpired == true)
                    {
                        status.Status = CertificateHealthStatus.Expired;

                        _logger.LogCritical(
                            "Locksmith: certificate for '{label}' ({host}:{port}) has EXPIRED! Subject: {subject}",
                            endpoint.Label, endpoint.Host, endpoint.Port, info.Subject);
                    }
                    else if (info.ChainIsValid == false)
                    {
                        status.Status = CertificateHealthStatus.ChainInvalid;

                        _logger.LogWarning(
                            "Locksmith: certificate chain invalid for '{label}' ({host}:{port}): {messages}",
                            endpoint.Label, endpoint.Host, endpoint.Port,
                            string.Join("; ", info.ChainStatusMessages));
                    }
                    else if (info.DaysUntilExpiry <= _config.Monitor.CriticalDays)
                    {
                        status.Status = CertificateHealthStatus.Critical;

                        _logger.LogCritical(
                            "Locksmith: certificate for '{label}' ({host}:{port}) expires in {days} days! Subject: {subject}",
                            endpoint.Label, endpoint.Host, endpoint.Port, info.DaysUntilExpiry, info.Subject);
                    }
                    else if (info.DaysUntilExpiry <= _config.Monitor.WarningDays)
                    {
                        status.Status = CertificateHealthStatus.Warning;

                        _logger.LogWarning(
                            "Locksmith: certificate for '{label}' ({host}:{port}) expires in {days} days. Subject: {subject}",
                            endpoint.Label, endpoint.Host, endpoint.Port, info.DaysUntilExpiry, info.Subject);
                    }
                    else
                    {
                        status.Status = CertificateHealthStatus.Healthy;
                    }
                }
                else
                {
                    status.Status = CertificateHealthStatus.Unreachable;
                    status.ErrorMessage = info.ErrorMessage;

                    _logger.LogWarning(
                        "Locksmith: cannot inspect certificate for '{label}' ({host}:{port}): {error}",
                        endpoint.Label, endpoint.Host, endpoint.Port, info.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                status.Status = CertificateHealthStatus.Unreachable;
                status.ErrorMessage = ex.Message;

                _logger.LogError(ex, "Error inspecting certificate for '{label}' ({host}:{port})",
                    endpoint.Label, endpoint.Host, endpoint.Port);
            }

            //
            // Update the cached status
            //
            _statuses[key] = status;
        }


        // ── Query Methods ─────────────────────────────────────────────────


        /// <summary>
        /// Gets the certificate status for all monitored endpoints.
        /// </summary>
        public List<CertificateEndpointStatus> GetAllStatuses()
        {
            List<CertificateEndpointStatus> statuses = new List<CertificateEndpointStatus>();

            foreach (CertificateEndpoint endpoint in _config.Monitor.Endpoints)
            {
                string key = endpoint.Host + ":" + endpoint.Port;

                if (_statuses.TryGetValue(key, out CertificateEndpointStatus status))
                {
                    statuses.Add(status);
                }
                else
                {
                    //
                    // Endpoint hasn't been checked yet
                    //
                    statuses.Add(new CertificateEndpointStatus
                    {
                        Label = endpoint.Label,
                        Host = endpoint.Host,
                        Port = endpoint.Port,
                        Status = CertificateHealthStatus.Unknown
                    });
                }
            }

            return statuses;
        }


        /// <summary>
        /// Gets the certificate status for a specific endpoint.
        /// </summary>
        public CertificateEndpointStatus GetStatus(string host, int port)
        {
            string key = host + ":" + port;

            if (_statuses.TryGetValue(key, out CertificateEndpointStatus status))
            {
                return status;
            }

            return new CertificateEndpointStatus
            {
                Host = host,
                Port = port,
                Status = CertificateHealthStatus.Unknown
            };
        }


        /// <summary>
        /// Gets only endpoints that have warnings or critical alerts.
        /// </summary>
        public List<CertificateEndpointStatus> GetAlerts()
        {
            return _statuses.Values
                .Where(s =>
                    s.Status == CertificateHealthStatus.Warning ||
                    s.Status == CertificateHealthStatus.Critical ||
                    s.Status == CertificateHealthStatus.Expired ||
                    s.Status == CertificateHealthStatus.ChainInvalid ||
                    s.Status == CertificateHealthStatus.Unreachable)
                .OrderBy(s => s.DaysUntilExpiry)
                .ToList();
        }


        /// <summary>
        /// Forces an immediate check of all endpoints.
        /// </summary>
        public async Task ForceCheckAsync()
        {
            foreach (CertificateEndpoint endpoint in _config.Monitor.Endpoints)
            {
                await CheckEndpointAsync(endpoint);
            }
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
