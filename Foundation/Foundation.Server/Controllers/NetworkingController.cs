// ============================================================================
//
// NetworkingController.cs — Unified Admin API for Foundation networking services.
//
// Provides a single overview endpoint and per-service detail endpoints for
// monitoring the 8 Foundation networking libraries from the client dashboard.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Foundation.Security;
using Foundation.Networking.Watchtower.Services;
using Foundation.Networking.Locksmith.Services;
using Foundation.Networking.Skynet.Firewall;
using Foundation.Networking.Skynet.Proxy;
using Foundation.Networking.Switchboard.Balancing;
using Foundation.Networking.Switchboard.Registry;
using Foundation.Networking.Hivemind.Cache;
using Foundation.Networking.Hivemind.Sessions;
using Foundation.Networking.Hivemind.PubSub;
using Foundation.Networking.DeepSpace;
using Foundation.Networking.Beacon.Discovery;
using Foundation.Networking.Beacon.Dns;
using Foundation.Networking.Conduit.Connections;
using Foundation.Networking.Conduit.Channels;

namespace Foundation.Server.Controllers
{
    [ApiController]
    [Route("api/networking")]
    [Authorize]
    public class NetworkingController : SecureWebAPIController
    {
        private readonly ILogger<NetworkingController> _logger;

        //
        // Networking services (injected via DI)
        //
        private readonly LatencyMonitorService _latencyMonitor;
        private readonly CertificateMonitorService _certMonitor;
        private readonly FirewallEngine _firewall;
        private readonly BackendPool _backendPool;
        private readonly LoadBalancer _loadBalancer;
        private readonly ServiceRegistry _serviceRegistry;
        private readonly DistributedCache _cache;
        private readonly SessionStore _sessionStore;
        private readonly MessageBus _messageBus;
        private readonly StorageManager _storageManager;
        private readonly ServiceDirectory _serviceDirectory;
        private readonly DnsResolver _dnsResolver;
        private readonly ConnectionManager _connectionManager;
        private readonly ChannelManager _channelManager;


        public NetworkingController(
            ILogger<NetworkingController> logger,
            LatencyMonitorService latencyMonitor,
            CertificateMonitorService certMonitor,
            FirewallEngine firewall,
            BackendPool backendPool,
            LoadBalancer loadBalancer,
            ServiceRegistry serviceRegistry,
            DistributedCache cache,
            SessionStore sessionStore,
            MessageBus messageBus,
            StorageManager storageManager,
            ServiceDirectory serviceDirectory,
            DnsResolver dnsResolver,
            ConnectionManager connectionManager,
            ChannelManager channelManager) : base("Foundation", "Networking")
        {
            _logger = logger;
            _latencyMonitor = latencyMonitor;
            _certMonitor = certMonitor;
            _firewall = firewall;
            _backendPool = backendPool;
            _loadBalancer = loadBalancer;
            _serviceRegistry = serviceRegistry;
            _cache = cache;
            _sessionStore = sessionStore;
            _messageBus = messageBus;
            _storageManager = storageManager;
            _serviceDirectory = serviceDirectory;
            _dnsResolver = dnsResolver;
            _connectionManager = connectionManager;
            _channelManager = channelManager;
        }


        // ── Overview ──────────────────────────────────────────────────────


        /// <summary>
        /// Returns aggregated status for all networking services.
        /// </summary>
        [HttpGet("overview")]
        public IActionResult GetOverview()
        {
            try
            {
                List<ServiceStatusSummary> services = new List<ServiceStatusSummary>();

                //
                // Watchtower
                //
                try
                {
                    var summaries = _latencyMonitor.GetAllSummaries();
                    services.Add(new ServiceStatusSummary
                    {
                        Name = "Watchtower",
                        Icon = "fa-binoculars",
                        Status = "healthy",
                        MetricLabel = "Monitored Endpoints",
                        MetricValue = summaries.Count.ToString()
                    });
                }
                catch
                {
                    services.Add(ServiceStatusSummary.Offline("Watchtower", "fa-binoculars"));
                }


                //
                // Locksmith
                //
                try
                {
                    var statuses = _certMonitor.GetAllStatuses();
                    int expiringSoon = statuses.Count(s => s.DaysUntilExpiry < 30 && s.DaysUntilExpiry >= 0);
                    string status = expiringSoon > 0 ? "warning" : "healthy";

                    services.Add(new ServiceStatusSummary
                    {
                        Name = "Locksmith",
                        Icon = "fa-key",
                        Status = status,
                        MetricLabel = "Certificates",
                        MetricValue = statuses.Count.ToString(),
                        SecondaryLabel = expiringSoon > 0 ? $"{expiringSoon} expiring soon" : null
                    });
                }
                catch
                {
                    services.Add(ServiceStatusSummary.Offline("Locksmith", "fa-key"));
                }


                //
                // Skynet
                //
                try
                {
                    services.Add(new ServiceStatusSummary
                    {
                        Name = "Skynet",
                        Icon = "fa-shield-halved",
                        Status = "healthy",
                        MetricLabel = "Firewall Rules",
                        MetricValue = _firewall.RuleCount.ToString(),
                        SecondaryLabel = $"{_backendPool.TotalCount} backends"
                    });
                }
                catch
                {
                    services.Add(ServiceStatusSummary.Offline("Skynet", "fa-shield-halved"));
                }


                //
                // Switchboard
                //
                try
                {
                    services.Add(new ServiceStatusSummary
                    {
                        Name = "Switchboard",
                        Icon = "fa-shuffle",
                        Status = _loadBalancer.HealthyBackends > 0 ? "healthy" : "warning",
                        MetricLabel = "Backends",
                        MetricValue = $"{_loadBalancer.HealthyBackends}/{_loadBalancer.TotalBackends}",
                        SecondaryLabel = _loadBalancer.StrategyName
                    });
                }
                catch
                {
                    services.Add(ServiceStatusSummary.Offline("Switchboard", "fa-shuffle"));
                }


                //
                // Hivemind
                //
                try
                {
                    var cacheStats = _cache.GetStatistics();
                    services.Add(new ServiceStatusSummary
                    {
                        Name = "Hivemind",
                        Icon = "fa-brain",
                        Status = "healthy",
                        MetricLabel = "Cache Entries",
                        MetricValue = cacheStats.EntryCount.ToString(),
                        SecondaryLabel = $"{_sessionStore.Count} sessions"
                    });
                }
                catch
                {
                    services.Add(ServiceStatusSummary.Offline("Hivemind", "fa-brain"));
                }


                //
                // Deep Space
                //
                try
                {
                    var storageStats = _storageManager.GetStatistics();
                    services.Add(new ServiceStatusSummary
                    {
                        Name = "Deep Space",
                        Icon = "fa-cloud",
                        Status = "healthy",
                        MetricLabel = "Providers",
                        MetricValue = storageStats.ProviderCount.ToString(),
                        SecondaryLabel = $"{storageStats.TotalPuts + storageStats.TotalGets + storageStats.TotalDeletes} ops"
                    });
                }
                catch
                {
                    services.Add(ServiceStatusSummary.Offline("Deep Space", "fa-cloud"));
                }


                //
                // Beacon
                //
                try
                {
                    var discoveryStats = _serviceDirectory.GetStatistics();
                    services.Add(new ServiceStatusSummary
                    {
                        Name = "Beacon",
                        Icon = "fa-tower-broadcast",
                        Status = "healthy",
                        MetricLabel = "Services",
                        MetricValue = discoveryStats.TotalServices.ToString(),
                        SecondaryLabel = $"{discoveryStats.TotalEndpoints} endpoints"
                    });
                }
                catch
                {
                    services.Add(ServiceStatusSummary.Offline("Beacon", "fa-tower-broadcast"));
                }


                //
                // Conduit
                //
                try
                {
                    services.Add(new ServiceStatusSummary
                    {
                        Name = "Conduit",
                        Icon = "fa-plug",
                        Status = "healthy",
                        MetricLabel = "Connections",
                        MetricValue = _connectionManager.ActiveCount.ToString(),
                        SecondaryLabel = $"{_channelManager.ChannelCount} channels"
                    });
                }
                catch
                {
                    services.Add(ServiceStatusSummary.Offline("Conduit", "fa-plug"));
                }


                int healthyCount = services.Count(s => s.Status == "healthy");
                int warningCount = services.Count(s => s.Status == "warning");
                int offlineCount = services.Count(s => s.Status == "offline");

                return Ok(new NetworkingOverviewResponse
                {
                    Services = services,
                    TotalServices = services.Count,
                    HealthyCount = healthyCount,
                    WarningCount = warningCount,
                    OfflineCount = offlineCount,
                    OverallStatus = offlineCount > 0 ? "degraded" : (warningCount > 0 ? "warning" : "healthy")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get networking overview");
                return StatusCode(500, new { Error = "Failed to retrieve networking overview." });
            }
        }


        // ── Per-Service Detail Endpoints ──────────────────────────────────


        /// <summary>
        /// Watchtower detail: latency monitoring summaries.
        /// </summary>
        [HttpGet("watchtower")]
        public IActionResult GetWatchtowerDetail()
        {
            try
            {
                var summaries = _latencyMonitor.GetAllSummaries();
                return Ok(new { Summaries = summaries, Count = summaries.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Watchtower detail");
                return StatusCode(500, new { Error = "Failed to retrieve Watchtower data." });
            }
        }


        /// <summary>
        /// Locksmith detail: certificate monitoring statuses.
        /// </summary>
        [HttpGet("locksmith")]
        public IActionResult GetLocksmithDetail()
        {
            try
            {
                var statuses = _certMonitor.GetAllStatuses();
                return Ok(new { Certificates = statuses, Count = statuses.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Locksmith detail");
                return StatusCode(500, new { Error = "Failed to retrieve Locksmith data." });
            }
        }


        /// <summary>
        /// Skynet detail: firewall rules and backend pool status.
        /// </summary>
        [HttpGet("skynet")]
        public IActionResult GetSkynetDetail()
        {
            try
            {
                return Ok(new
                {
                    FirewallRuleCount = _firewall.RuleCount,
                    Backends = _backendPool.AllBackends.Select(b => new
                    {
                        Address = b.Config?.Url ?? "unknown",
                        b.Status,
                        b.ActiveConnections,
                        b.TotalRequests,
                        b.TotalErrors
                    }),
                    TotalBackends = _backendPool.TotalCount,
                    HealthyBackends = _backendPool.HealthyCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Skynet detail");
                return StatusCode(500, new { Error = "Failed to retrieve Skynet data." });
            }
        }


        /// <summary>
        /// Switchboard detail: load balancer status and service registry.
        /// </summary>
        [HttpGet("switchboard")]
        public IActionResult GetSwitchboardDetail()
        {
            try
            {
                return Ok(new
                {
                    Strategy = _loadBalancer.StrategyName,
                    TotalBackends = _loadBalancer.TotalBackends,
                    HealthyBackends = _loadBalancer.HealthyBackends,
                    RegisteredServices = _serviceRegistry.GetAll().Select(s => new
                    {
                        s.ServiceName,
                        s.Url,
                        s.Weight,
                        s.HealthCheckPath
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Switchboard detail");
                return StatusCode(500, new { Error = "Failed to retrieve Switchboard data." });
            }
        }


        /// <summary>
        /// Hivemind detail: cache, session, and pub/sub statistics.
        /// </summary>
        [HttpGet("hivemind")]
        public IActionResult GetHivemindDetail()
        {
            try
            {
                var cacheStats = _cache.GetStatistics();
                var busStats = _messageBus.GetStatistics();
                return Ok(new
                {
                    Cache = new
                    {
                        cacheStats.EntryCount,
                        cacheStats.Hits,
                        cacheStats.Misses,
                        cacheStats.HitRate,
                        cacheStats.EvictionCount,
                        cacheStats.MaxEntries
                    },
                    Sessions = new
                    {
                        ActiveCount = _sessionStore.Count
                    },
                    PubSub = new
                    {
                        busStats.ChannelCount,
                        busStats.TotalSubscribers,
                        busStats.TotalPublished,
                        busStats.TotalDelivered
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Hivemind detail");
                return StatusCode(500, new { Error = "Failed to retrieve Hivemind data." });
            }
        }


        /// <summary>
        /// Deep Space detail: storage provider statistics.
        /// </summary>
        [HttpGet("deepspace")]
        public IActionResult GetDeepSpaceDetail()
        {
            try
            {
                var stats = _storageManager.GetStatistics();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Deep Space detail");
                return StatusCode(500, new { Error = "Failed to retrieve Deep Space data." });
            }
        }


        /// <summary>
        /// Beacon detail: DNS zones and service discovery statistics.
        /// </summary>
        [HttpGet("beacon")]
        public IActionResult GetBeaconDetail()
        {
            try
            {
                var discoveryStats = _serviceDirectory.GetStatistics();
                return Ok(new
                {
                    Discovery = discoveryStats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Beacon detail");
                return StatusCode(500, new { Error = "Failed to retrieve Beacon data." });
            }
        }


        /// <summary>
        /// Conduit detail: active connections and channels.
        /// </summary>
        [HttpGet("conduit")]
        public IActionResult GetConduitDetail()
        {
            try
            {
                var connStats = _connectionManager.GetStatistics();
                return Ok(new
                {
                    ConnectionStats = connStats,
                    Connections = _connectionManager.GetAll().Select(c => new
                    {
                        c.ConnectionId,
                        c.ClientIp,
                        c.UserId,
                        c.ConnectedUtc,
                        c.MessagesSent,
                        c.MessagesReceived
                    }),
                    Channels = _channelManager.GetChannelNames().Select(name => _channelManager.GetChannelInfo(name))
                        .Where(ch => ch != null)
                        .Select(ch => new
                        {
                            ch.Name,
                            ch.SubscriberCount,
                            ch.TotalMessages
                        })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Conduit detail");
                return StatusCode(500, new { Error = "Failed to retrieve Conduit data." });
            }
        }


        // ── Response Models ───────────────────────────────────────────────


        public class NetworkingOverviewResponse
        {
            public List<ServiceStatusSummary> Services { get; set; }
            public int TotalServices { get; set; }
            public int HealthyCount { get; set; }
            public int WarningCount { get; set; }
            public int OfflineCount { get; set; }
            public string OverallStatus { get; set; }
        }


        public class ServiceStatusSummary
        {
            public string Name { get; set; }
            public string Icon { get; set; }
            public string Status { get; set; }
            public string MetricLabel { get; set; }
            public string MetricValue { get; set; }
            public string SecondaryLabel { get; set; }


            public static ServiceStatusSummary Offline(string name, string icon)
            {
                return new ServiceStatusSummary
                {
                    Name = name,
                    Icon = icon,
                    Status = "offline",
                    MetricLabel = "Status",
                    MetricValue = "Offline"
                };
            }
        }
    }
}
