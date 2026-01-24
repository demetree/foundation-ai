//
// System Health Controller
//
// API endpoints for monitoring system health and operational metrics.
// Provides real-time diagnostics for system administrators.
//
using Foundation.Auditor;
using Foundation.Security;
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
            IConfiguration configuration)
            : base("Auditor", "SystemHealth")
        {
            _logger = logger;
            _configuration = configuration;
        }


        //
        // GET: api/SystemHealth/status
        //
        // Returns complete system health snapshot
        // AllowAnonymous enables server-to-server health checks without auth
        // Responses are cached for 1 second to rate limit and protect against DDOS
        //
        [HttpGet("status")]
        [AllowAnonymous]
        public IActionResult GetStatus()
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
                    Database = GetDatabaseStatus(),
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
        // Returns database connection status
        //
        [HttpGet("database")]
        public IActionResult GetDatabase()
        {
            try
            {
                return Ok(GetDatabaseStatus());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database status");
                return Problem("Failed to retrieve database status");
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
        // Helper methods for gathering metrics
        //

        private object GetApplicationMetrics(Process process)
        {
            var uptime = DateTime.UtcNow - _startTime;

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


        private object GetDatabaseStatus()
        {
            //
            // Get connection string info (hide sensitive data)
            //
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var isConfigured = !string.IsNullOrEmpty(connectionString);

            string provider = "Unknown";
            string serverInfo = "Not configured";

            if (isConfigured)
            {
                //
                // Parse basic info from connection string without exposing credentials
                //
                try
                {
                    var parts = connectionString.Split(';')
                        .Select(p => p.Trim())
                        .Where(p => !string.IsNullOrEmpty(p))
                        .Select(p => p.Split('='))
                        .Where(p => p.Length == 2)
                        .ToDictionary(p => p[0].Trim().ToLower(), p => p[1].Trim());

                    if (parts.ContainsKey("server") || parts.ContainsKey("data source"))
                    {
                        serverInfo = parts.ContainsKey("server") ? parts["server"] : parts["data source"];
                        provider = "SQL Server";
                    }
                    else if (parts.ContainsKey("host"))
                    {
                        serverInfo = parts["host"];
                        provider = "PostgreSQL";
                    }
                }
                catch
                {
                    serverInfo = "Configured";
                }
            }

            return new
            {
                Status = isConfigured ? "Configured" : "Not Configured",
                IsConnected = isConfigured,
                Provider = provider,
                Server = serverInfo
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
    }
}
