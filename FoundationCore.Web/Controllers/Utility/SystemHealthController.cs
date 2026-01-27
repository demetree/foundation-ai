//
// System Health Controller
//
// API endpoints for monitoring system health and operational metrics.
// Provides real-time diagnostics for system administrators.
//
using Foundation.Auditor;
using Foundation.Security;
using Foundation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Foundation.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// System Health Dashboard API
    /// 
    /// Provides real-time operational metrics for system administrators.
    /// Access control is done through the Auditor Module.
    /// 
    /// </summary>
    [Route("api/[controller]")]
    public class SystemHealthController : SecureWebAPIController
    {
        private readonly ILogger<SystemHealthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEnumerable<IDatabaseHealthProvider> _databaseProviders;
        private readonly IMonitoredApplicationService _monitoredAppsService;
        private readonly IAuthenticatedUsersProvider _authenticatedUsersProvider;
        private readonly IEnumerable<IApplicationMetricsProvider> _metricsProviders;
        private static readonly DateTime _startTime = DateTime.UtcNow;

        //
        // Response caching for rate limiting (protects anonymous endpoint from DDOS)
        //
        private static object _cachedStatusResponse;
        private static DateTime _cacheExpiry = DateTime.MinValue;
        private static readonly object _cacheLock = new object();
        private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(1);


        public SystemHealthController(
            ILogger<SystemHealthController> logger,
            IConfiguration configuration,
            IMonitoredApplicationService monitoredAppsService = null,
            IAuthenticatedUsersProvider authenticatedUsersProvider = null,
            IEnumerable<IDatabaseHealthProvider> databaseProviders = null,
            IEnumerable<IApplicationMetricsProvider> metricsProviders = null)
            : base("Auditor", "SystemHealth")
        {
            _logger = logger;
            _configuration = configuration;
            _monitoredAppsService = monitoredAppsService;
            _authenticatedUsersProvider = authenticatedUsersProvider;
            _databaseProviders = databaseProviders ?? Array.Empty<IDatabaseHealthProvider>();
            _metricsProviders = metricsProviders ?? Array.Empty<IApplicationMetricsProvider>();
        }


        //
        // GET: api/SystemHealth/status
        //
        // Returns complete system health snapshot
        // Requires authentication via Auditor module (same as other endpoints)
        // Responses are cached for 1 second to rate limit
        //
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                //
                // Return cached response if still valid
                //
                lock (_cacheLock)
                {
                    if (_cachedStatusResponse != null && DateTime.UtcNow < _cacheExpiry)
                    {
                        return Ok(_cachedStatusResponse);
                    }
                }

                //
                // Generate fresh response
                //
                var process = Process.GetCurrentProcess();

                var status = new
                {
                    Timestamp = DateTime.UtcNow,
                    Application = GetApplicationMetrics(process),
                    Database = await GetDatabaseStatusesAsync().ConfigureAwait(false),
                    Disk = GetDiskMetrics(),
                    ThreadPool = GetThreadPoolMetrics()
                };

                //
                // Cache the response
                //
                lock (_cacheLock)
                {
                    _cachedStatusResponse = status;
                    _cacheExpiry = DateTime.UtcNow.Add(CacheDuration);
                }

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system health status");
                return Problem("Failed to retrieve system health status");
            }
        }


        //
        // GET: api/SystemHealth/application
        //
        // Returns application-specific metrics
        //
        [HttpGet("application")]
        public IActionResult GetApplication()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                return Ok(GetApplicationMetrics(process));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting application metrics");
                return Problem("Failed to retrieve application metrics");
            }
        }


        //
        // GET: api/SystemHealth/database
        //
        // Returns database connection status for all registered providers
        //
        [HttpGet("database")]
        public async Task<IActionResult> GetDatabase()
        {
            try
            {
                return Ok(await GetDatabaseStatusesAsync().ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database status");
                return Problem("Failed to retrieve database status");
            }
        }


        //
        // GET: api/SystemHealth/database/tables?database=Security&appName=Scheduler
        //
        // Returns table-level statistics for a specific database.
        // If appName is provided and doesn't match self, proxies to that remote application.
        // This is an expensive operation and should only be called on-demand.
        //
        [HttpGet("database/tables")]
        public async Task<IActionResult> GetDatabaseTables([FromQuery] string database, [FromQuery] string appName = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(database))
                {
                    return BadRequest("Database name is required");
                }

                //
                // Check if we need to proxy to a remote application
                //
                if (!string.IsNullOrWhiteSpace(appName) && _monitoredAppsService != null)
                {
                    var app = _monitoredAppsService.GetApplicationByName(appName);
                    if (app != null && !app.IsSelf)
                    {
                        //
                        // Proxy the request to the remote application
                        //
                        var userObjectGuid = User?.Claims?.FirstOrDefault(c => c.Type == "sub")?.Value;
                        if (string.IsNullOrEmpty(userObjectGuid))
                        {
                            return Unauthorized("User authentication required for remote app access");
                        }

                        var response = await _monitoredAppsService.MakeAuthenticatedRequestAsync(
                            appName, 
                            $"api/SystemHealth/database/tables?database={database}",
                            userObjectGuid).ConfigureAwait(false);

                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            return Content(content, "application/json");
                        }
                        else
                        {
                            var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            _logger.LogWarning("Remote app {AppName} returned {StatusCode}: {Error}", 
                                appName, response.StatusCode, errorContent);
                            return Problem($"Remote application returned error: {response.StatusCode}");
                        }
                    }
                }

                //
                // Local database - find the provider for the requested database
                //
                var provider = _databaseProviders.FirstOrDefault(p => 
                    string.Equals(p.Name, database, StringComparison.OrdinalIgnoreCase));

                if (provider == null)
                {
                    return NotFound($"Database '{database}' not found");
                }

                var statistics = await provider.GetTableStatisticsAsync().ConfigureAwait(false);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting table statistics for database {Database}", database);
                return Problem($"Failed to retrieve table statistics for database '{database}'");
            }
        }


        //
        // GET: api/SystemHealth/disk
        //
        // Returns disk space metrics
        //
        [HttpGet("disk")]
        public IActionResult GetDisk()
        {
            try
            {
                return Ok(GetDiskMetrics());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting disk metrics");
                return Problem("Failed to retrieve disk metrics");
            }
        }


        //
        // GET: api/SystemHealth/users
        //
        // Returns authenticated user sessions from OAuth tokens
        //
        [HttpGet("users")]
        public async Task<IActionResult> GetAuthenticatedUsers()
        {
            try
            {
                if (_authenticatedUsersProvider == null)
                {
                    return Ok(new AuthenticatedUsersInfo
                    {
                        ErrorMessage = "Authenticated users provider not configured"
                    });
                }

                var result = await _authenticatedUsersProvider.GetAuthenticatedUsersAsync().ConfigureAwait(false);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting authenticated users");
                return Problem("Failed to retrieve authenticated users");
            }
        }


        //
        // GET: api/SystemHealth/metrics
        //
        // Returns application-specific business metrics from all registered providers
        //
        [HttpGet("metrics")]
        public async Task<IActionResult> GetApplicationMetrics()
        {
            try
            {
                var response = new ApplicationMetricsResponse();

                foreach (var provider in _metricsProviders)
                {
                    try
                    {
                        var metrics = await provider.GetMetricsAsync().ConfigureAwait(false);
                        response.Applications.Add(new ApplicationMetricsGroup
                        {
                            ApplicationName = provider.ApplicationName,
                            Metrics = metrics?.ToList() ?? new List<ApplicationMetric>()
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get metrics from provider {Provider}", provider.ApplicationName);
                        response.Applications.Add(new ApplicationMetricsGroup
                        {
                            ApplicationName = provider.ApplicationName,
                            Metrics = new List<ApplicationMetric>
                            {
                                new ApplicationMetric
                                {
                                    Name = "Error",
                                    Value = "Failed to retrieve",
                                    State = MetricState.Critical,
                                    DataType = MetricDataType.Text
                                }
                            }
                        });
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting application metrics");
                return Problem("Failed to retrieve application metrics");
            }
        }


        //
        // Helper methods for gathering metrics
        //

        private object GetApplicationMetrics(Process process)
        {
            var uptime = DateTime.UtcNow - _startTime;

            // Calculate CPU usage as percentage of total CPU time over uptime
            double cpuPercent = 0;
            try
            {
                var totalCpuTime = process.TotalProcessorTime.TotalMilliseconds;
                var uptimeMs = uptime.TotalMilliseconds;
                if (uptimeMs > 0)
                {
                    // Divide by processor count to get per-core average
                    cpuPercent = Math.Round((totalCpuTime / uptimeMs / Environment.ProcessorCount) * 100, 2);
                }
            }
            catch
            {
                // CPU metrics may not be available on all platforms
            }

            return new
            {
                Status = "Running",
                Uptime = new
                {
                    TotalSeconds = uptime.TotalSeconds,
                    Days = uptime.Days,
                    Hours = uptime.Hours,
                    Minutes = uptime.Minutes,
                    Display = FormatUptime(uptime)
                },
                Memory = new
                {
                    WorkingSetMB = Math.Round(process.WorkingSet64 / (1024.0 * 1024.0), 2),
                    PrivateMemoryMB = Math.Round(process.PrivateMemorySize64 / (1024.0 * 1024.0), 2),
                    GCHeapMB = Math.Round(GC.GetTotalMemory(false) / (1024.0 * 1024.0), 2),
                    Gen0Collections = GC.CollectionCount(0),
                    Gen1Collections = GC.CollectionCount(1),
                    Gen2Collections = GC.CollectionCount(2)
                },
                Cpu = new
                {
                    Percent = cpuPercent,
                    ProcessorCount = Environment.ProcessorCount
                },
                Process = new
                {
                    Id = process.Id,
                    Name = process.ProcessName,
                    StartTime = _startTime,
                    ThreadCount = process.Threads.Count,
                    HandleCount = process.HandleCount
                },
                Environment = new
                {
                    MachineName = System.Environment.MachineName,
                    OSVersion = System.Environment.OSVersion.ToString(),
                    ProcessorCount = System.Environment.ProcessorCount,
                    Is64Bit = System.Environment.Is64BitProcess,
                    DotNetVersion = System.Environment.Version.ToString()
                }
            };
        }


        private async Task<object> GetDatabaseStatusesAsync()
        {
            var databases = new List<DatabaseHealthInfo>();

            //
            // Query all registered database health providers
            //
            foreach (var provider in _databaseProviders)
            {
                try
                {
                    var health = await provider.GetHealthAsync().ConfigureAwait(false);
                    databases.Add(health);
                }
                catch (Exception ex)
                {
                    //
                    // If a provider fails, still include it with error status
                    //
                    databases.Add(new DatabaseHealthInfo
                    {
                        Name = provider.Name,
                        Status = "Error",
                        IsConnected = false,
                        Provider = "Unknown",
                        Server = "Unknown",
                        ErrorMessage = ex.Message
                    });
                }
            }

            //
            // If no providers registered, return a placeholder
            //
            if (databases.Count == 0)
            {
                databases.Add(new DatabaseHealthInfo
                {
                    Name = "Database",
                    Status = "No providers registered",
                    IsConnected = false,
                    Provider = "Unknown",
                    Server = "Not configured"
                });
            }

            return new
            {
                Databases = databases
            };
        }


        private object GetDiskMetrics()
        {
            var drives = new List<object>();

            try
            {
                //
                // Get the application's base path drive
                //
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var baseDrive = Path.GetPathRoot(basePath);

                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                    {
                        var freePercent = Math.Round((double)drive.AvailableFreeSpace / drive.TotalSize * 100, 1);
                        var usedPercent = 100 - freePercent;

                        drives.Add(new
                        {
                            Name = drive.Name,
                            Label = drive.VolumeLabel,
                            TotalGB = Math.Round(drive.TotalSize / (1024.0 * 1024.0 * 1024.0), 2),
                            FreeGB = Math.Round(drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0), 2),
                            UsedGB = Math.Round((drive.TotalSize - drive.AvailableFreeSpace) / (1024.0 * 1024.0 * 1024.0), 2),
                            FreePercent = freePercent,
                            UsedPercent = usedPercent,
                            Status = freePercent < 10 ? "Critical" : (freePercent < 20 ? "Warning" : "Healthy"),
                            IsApplicationDrive = drive.Name.Equals(baseDrive, StringComparison.OrdinalIgnoreCase)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting drive information");
            }

            return new
            {
                Drives = drives,
                ApplicationPath = AppDomain.CurrentDomain.BaseDirectory
            };
        }


        private object GetThreadPoolMetrics()
        {
            ThreadPool.GetAvailableThreads(out int workerThreads, out int completionPortThreads);
            ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);
            ThreadPool.GetMinThreads(out int minWorkerThreads, out int minCompletionPortThreads);

            return new
            {
                WorkerThreads = new
                {
                    Available = workerThreads,
                    Max = maxWorkerThreads,
                    Min = minWorkerThreads,
                    InUse = maxWorkerThreads - workerThreads
                },
                CompletionPortThreads = new
                {
                    Available = completionPortThreads,
                    Max = maxCompletionPortThreads,
                    Min = minCompletionPortThreads,
                    InUse = maxCompletionPortThreads - completionPortThreads
                }
            };
        }


        private static string FormatUptime(TimeSpan uptime)
        {
            if (uptime.TotalDays >= 1)
            {
                return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";
            }
            else if (uptime.TotalHours >= 1)
            {
                return $"{uptime.Hours}h {uptime.Minutes}m";
            }
            else
            {
                return $"{uptime.Minutes}m {uptime.Seconds}s";
            }
        }


        //
        // GET: api/SystemHealth/remote/{appName}
        //
        // Returns real-time system health from a remote monitored application.
        // Uses MonitoredApplicationService for proper authentication and proxying.
        //
        [HttpGet("remote/{appName}")]
        public async Task<IActionResult> GetRemoteStatus(string appName)
        {
            try
            {
                if (_monitoredAppsService == null)
                {
                    return Problem("Monitored application service not available");
                }

                // Check if this is the current server
                var app = _monitoredAppsService.GetApplicationByName(appName);
                if (app == null)
                {
                    return NotFound($"Application '{appName}' not found");
                }

                if (app.IsSelf)
                {
                    // Redirect to local status - frontend should call GetStatus instead
                    return Ok(new { isSelf = true, message = "Use local /api/SystemHealth endpoint" });
                }

                // Get user identity for authenticated proxy request
                var userObjectGuid = User?.Claims?.FirstOrDefault(c => c.Type == "sub")?.Value;

                // Use the monitored app service to fetch status with proper auth
                var status = await _monitoredAppsService.GetApplicationStatusAsync(appName, userObjectGuid);

                if (status.IsAvailable && status.HealthData != null)
                {
                    // Return the full health data from the remote app
                    return Ok(status.HealthData);
                }
                else
                {
                    return StatusCode(503, new
                    {
                        error = status.Error ?? "Remote application unavailable",
                        appName = appName,
                        status = status.Status
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching remote status for {AppName}", appName);
                return Problem($"Failed to fetch system health from {appName}");
            }
        }


        //
        // GET: api/SystemHealth/remote/{appName}/users
        //
        // Returns authenticated users from a remote monitored application.
        //
        [HttpGet("remote/{appName}/users")]
        public async Task<IActionResult> GetRemoteUsers(string appName)
        {
            try
            {
                if (_monitoredAppsService == null)
                {
                    return Problem("Monitored application service not available");
                }

                var app = _monitoredAppsService.GetApplicationByName(appName);
                if (app == null)
                {
                    return NotFound($"Application '{appName}' not found");
                }

                if (app.IsSelf)
                {
                    return Ok(new { isSelf = true, message = "Use local endpoint" });
                }

                var userObjectGuid = User?.Claims?.FirstOrDefault(c => c.Type == "sub")?.Value;

                var response = await _monitoredAppsService.MakeAuthenticatedRequestAsync(
                    appName, 
                    "api/SystemHealth/users",
                    userObjectGuid);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return Content(content, "application/json");
                }
                else
                {
                    return StatusCode((int)response.StatusCode, new { 
                        error = $"Remote server returned {response.StatusCode}"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching remote users for {AppName}", appName);
                return StatusCode(503, new { error = "Cannot fetch remote users" });
            }
        }
    }
}
