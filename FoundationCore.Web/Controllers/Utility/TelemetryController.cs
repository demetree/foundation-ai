//
// Telemetry Controller
//
// API endpoints for querying historical telemetry data collected by the TelemetryCollectorService.
// Provides trend analysis and historical snapshots for system administrators.
//
using Foundation.Security;
using Foundation.Telemetry.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Foundation.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// Telemetry API
    /// 
    /// Provides historical telemetry data and trend analysis for system administrators.
    /// Access control is done through the Auditor Module.
    /// 
    /// </summary>
    [Route("api/[controller]")]
    public class TelemetryController : SecureWebAPIController
    {
        private readonly ILogger<TelemetryController> _logger;

        public TelemetryController(ILogger<TelemetryController> logger)
            : base("Auditor", "Telemetry")
        {
            _logger = logger;
        }


        //
        // GET: api/Telemetry/snapshots
        //
        // Returns historical snapshots with optional filtering by app and date range
        //
        [HttpGet("snapshots")]
        public async Task<IActionResult> GetSnapshots([FromQuery] string appName = null,
                                                      [FromQuery] DateTime? startDate = null,
                                                      [FromQuery] DateTime? endDate = null,
                                                      [FromQuery] int limit = 100)
        {
            try
            {
                await using (TelemetryContext context = new TelemetryContext())
                {
                    IQueryable<TelemetrySnapshot> query = context.TelemetrySnapshots
                                                                 .Include(s => s.telemetryApplication)
                                                                 .AsQueryable();

                    if (!string.IsNullOrWhiteSpace(appName))
                    {
                        query = query.Where(s => s.telemetryApplication.name == appName);
                    }

                    if (startDate.HasValue)
                    {
                        query = query.Where(s => s.collectedAt >= startDate.Value);
                    }

                    if (endDate.HasValue)
                    {
                        query = query.Where(s => s.collectedAt <= endDate.Value);
                    }

                    var snapshots = await query
                        .OrderByDescending(s => s.collectedAt)
                        .Take(Math.Min(limit, 1000))
                        .Select(s => new
                        {
                            s.id,
                            applicationName = s.telemetryApplication.name,
                            s.collectedAt,
                            s.isOnline,
                            s.uptimeSeconds,
                            s.memoryWorkingSetMB,
                            s.memoryGcHeapMB,
                            s.cpuPercent,
                            s.threadPoolWorkerThreads,
                            s.threadPoolPendingWorkItems,
                            s.machineName
                        })
                        .ToListAsync()
                        .ConfigureAwait(false);

                    return Ok(new { snapshots, count = snapshots.Count });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving telemetry snapshots");
                return Problem("Failed to retrieve telemetry snapshots");
            }
        }


        //
        // GET: api/Telemetry/snapshots/{id}
        //
        // Returns full snapshot detail with child records for drill-down modal
        //
        [HttpGet("snapshots/{id}")]
        public async Task<IActionResult> GetSnapshotDetail(int id)
        {
            try
            {
                await using (TelemetryContext context = new TelemetryContext())
                {
                    TelemetrySnapshot snapshot = await context.TelemetrySnapshots.Include(s => s.telemetryApplication)
                                                                                 .Include(s => s.TelemetryDatabaseHealths)
                                                                                 .Include(s => s.TelemetryDiskHealths)
                                                                                 .Include(s => s.TelemetrySessionSnapshots)
                                                                                 .Include(s => s.TelemetryApplicationMetrics)
                                                                                 .Include(s => s.TelemetryLogErrors)
                                                                                 .FirstOrDefaultAsync(s => s.id == id)
                                                                                 .ConfigureAwait(false);

                    if (snapshot == null)
                    {
                        return NotFound(new { error = $"Snapshot {id} not found" });
                    }

                    var result = new
                    {
                        snapshot.id,
                        applicationName = snapshot.telemetryApplication?.name,
                        snapshot.collectedAt,
                        snapshot.isOnline,
                        snapshot.uptimeSeconds,
                        snapshot.memoryWorkingSetMB,
                        snapshot.memoryGcHeapMB,
                        snapshot.cpuPercent,
                        snapshot.threadPoolWorkerThreads,
                        snapshot.threadPoolPendingWorkItems,
                        snapshot.machineName,
                        databases = snapshot.TelemetryDatabaseHealths.Select(d => new
                        {
                            d.databaseName,
                            d.isConnected,
                            d.status,
                            d.server,
                            d.provider,
                            d.errorMessage
                        }),
                        disks = snapshot.TelemetryDiskHealths.Select(d => new
                        {
                            d.driveName,
                            d.driveLabel,
                            d.totalGB,
                            d.freeGB,
                            d.freePercent,
                            d.status,
                            d.isApplicationDrive
                        }),
                        sessions = snapshot.TelemetrySessionSnapshots.FirstOrDefault() != null
                            ? new
                            {
                                snapshot.TelemetrySessionSnapshots.First().activeSessionCount,
                                snapshot.TelemetrySessionSnapshots.First().expiredSessionCount,
                                snapshot.TelemetrySessionSnapshots.First().oldestSessionStart,
                                snapshot.TelemetrySessionSnapshots.First().newestSessionStart
                            }
                            : null,
                        metrics = snapshot.TelemetryApplicationMetrics.Select(m => new
                        {
                            m.metricName,
                            m.metricValue,
                            m.state,
                            m.dataType,
                            m.numericValue,
                            m.category
                        }),
                        logErrors = snapshot.TelemetryLogErrors.Select(e => new
                        {
                            e.logTimestamp,
                            e.level,
                            e.message,
                            e.exception,
                            e.logFileName,
                            e.occurrenceCount
                        })
                    };

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving snapshot detail for id {SnapshotId}", id);
                return Problem("Failed to retrieve snapshot detail");
            }
        }


        //
        // GET: api/Telemetry/trends/memory
        //
        // Returns memory usage trend data for charting
        //
        [HttpGet("trends/memory")]
        public async Task<IActionResult> GetMemoryTrends([FromQuery] string appName = null,
                                                         [FromQuery] int hours = 24)
        {
            try
            {
                DateTime startTime = DateTime.UtcNow.AddHours(-hours);

                await using (TelemetryContext context = new TelemetryContext())
                {
                    IQueryable<TelemetrySnapshot> query = context.TelemetrySnapshots.Include(s => s.telemetryApplication)
                                                                                    .Where(s => s.collectedAt >= startTime && s.isOnline)
                                                                                    .AsQueryable();

                    if (!string.IsNullOrWhiteSpace(appName))
                    {
                        query = query.Where(s => s.telemetryApplication.name == appName);
                    }

                    var data = await query
                        .OrderBy(s => s.collectedAt)
                        .Select(s => new
                        {
                            timestamp = s.collectedAt,
                            applicationName = s.telemetryApplication.name,
                            workingSetMB = s.memoryWorkingSetMB,
                            gcHeapMB = s.memoryGcHeapMB
                        })
                        .ToListAsync()
                        .ConfigureAwait(false);

                    return Ok(new { data, hours, count = data.Count });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving memory trends");
                return Problem("Failed to retrieve memory trends");
            }
        }


        //
        // GET: api/Telemetry/trends/cpu
        //
        // Returns CPU usage trend data for charting
        //
        [HttpGet("trends/cpu")]
        public async Task<IActionResult> GetCpuTrends([FromQuery] string appName = null,
                                                      [FromQuery] int hours = 24)
        {
            try
            {
                DateTime startTime = DateTime.UtcNow.AddHours(-hours);

                await using (TelemetryContext context = new TelemetryContext())
                {
                   IQueryable<TelemetrySnapshot> query = context.TelemetrySnapshots
                        .Include(s => s.telemetryApplication)
                        .Where(s => s.collectedAt >= startTime && s.isOnline && s.cpuPercent.HasValue)
                        .AsQueryable();

                    if (!string.IsNullOrWhiteSpace(appName))
                    {
                        query = query.Where(s => s.telemetryApplication.name == appName);
                    }

                    var data = await query
                        .OrderBy(s => s.collectedAt)
                        .Select(s => new
                        {
                            timestamp = s.collectedAt,
                            applicationName = s.telemetryApplication.name,
                            cpuPercent = s.cpuPercent
                        })
                        .ToListAsync()
                        .ConfigureAwait(false);

                    return Ok(new { data, hours, count = data.Count });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving CPU trends");
                return Problem("Failed to retrieve CPU trends");
            }
        }


        //
        // GET: api/Telemetry/trends/disk
        //
        // Returns disk usage trend data for charting
        //
        [HttpGet("trends/disk")]
        public async Task<IActionResult> GetDiskTrends(
            [FromQuery] string appName = null,
            [FromQuery] int hours = 24)
        {
            try
            {
                DateTime startTime = DateTime.UtcNow.AddHours(-hours);

                await using (TelemetryContext context = new TelemetryContext())
                {
                    IQueryable<TelemetryDiskHealth> query = context.TelemetryDiskHealths
                                                                   .Include(d => d.telemetrySnapshot)
                                                                       .ThenInclude(s => s.telemetryApplication)
                                                                   .Where(d => d.telemetrySnapshot.collectedAt >= startTime)
                                                                   .AsQueryable();

                    if (!string.IsNullOrWhiteSpace(appName))
                    {
                        query = query.Where(d => d.telemetrySnapshot.telemetryApplication.name == appName);
                    }

                    var data = await query
                        .OrderBy(d => d.telemetrySnapshot.collectedAt)
                        .Select(d => new
                        {
                            timestamp = d.telemetrySnapshot.collectedAt,
                            applicationName = d.telemetrySnapshot.telemetryApplication.name,
                            driveName = d.driveName,
                            totalGB = d.totalGB,
                            freeGB = d.freeGB,
                            freePercent = d.freePercent,
                            status = d.status
                        })
                        .ToListAsync()
                        .ConfigureAwait(false);

                    return Ok(new { data, hours, count = data.Count });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving disk trends");
                return Problem("Failed to retrieve disk trends");
            }
        }


        //
        // GET: api/Telemetry/trends/network
        //
        // Returns network utilization trend data for sparklines
        //
        [HttpGet("trends/network")]
        public async Task<IActionResult> GetNetworkTrends(
            [FromQuery] string appName = null,
            [FromQuery] int hours = 24)
        {
            try
            {
                DateTime startTime = DateTime.UtcNow.AddHours(-hours);

                await using (TelemetryContext context = new TelemetryContext())
                {
                    IQueryable<TelemetryNetworkHealth> query = context.TelemetryNetworkHealths
                                                                       .Include(n => n.telemetrySnapshot)
                                                                           .ThenInclude(s => s.telemetryApplication)
                                                                       .Where(n => n.telemetrySnapshot.collectedAt >= startTime && n.isActive)
                                                                       .AsQueryable();

                    if (!string.IsNullOrWhiteSpace(appName))
                    {
                        query = query.Where(n => n.telemetrySnapshot.telemetryApplication.name == appName);
                    }

                    var data = await query
                        .OrderBy(n => n.telemetrySnapshot.collectedAt)
                        .Select(n => new
                        {
                            timestamp = n.telemetrySnapshot.collectedAt,
                            applicationName = n.telemetrySnapshot.telemetryApplication.name,
                            interfaceName = n.interfaceName,
                            linkSpeedMbps = n.linkSpeedMbps,
                            bytesSentPerSecond = n.bytesSentPerSecond,
                            bytesReceivedPerSecond = n.bytesReceivedPerSecond,
                            utilizationPercent = n.utilizationPercent,
                            status = n.status
                        })
                        .ToListAsync()
                        .ConfigureAwait(false);

                    return Ok(new { data, hours, count = data.Count });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving network trends");
                return Problem("Failed to retrieve network trends");
            }
        }


        //
        // GET: api/Telemetry/trends/systemMemory
        //
        // Returns system-wide memory percentage trend data for sparklines
        //
        [HttpGet("trends/systemMemory")]
        public async Task<IActionResult> GetSystemMemoryTrends([FromQuery] string appName = null,
                                                                [FromQuery] int hours = 24)
        {
            try
            {
                DateTime startTime = DateTime.UtcNow.AddHours(-hours);

                await using (TelemetryContext context = new TelemetryContext())
                {
                    IQueryable<TelemetrySnapshot> query = context.TelemetrySnapshots
                        .Include(s => s.telemetryApplication)
                        .Where(s => s.collectedAt >= startTime && s.isOnline && s.systemMemoryPercent.HasValue)
                        .AsQueryable();

                    if (!string.IsNullOrWhiteSpace(appName))
                    {
                        query = query.Where(s => s.telemetryApplication.name == appName);
                    }

                    var data = await query
                        .OrderBy(s => s.collectedAt)
                        .Select(s => new
                        {
                            timestamp = s.collectedAt,
                            applicationName = s.telemetryApplication.name,
                            systemMemoryPercent = s.systemMemoryPercent
                        })
                        .ToListAsync()
                        .ConfigureAwait(false);

                    return Ok(new { data, hours, count = data.Count });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system memory trends");
                return Problem("Failed to retrieve system memory trends");
            }
        }


        //
        // GET: api/Telemetry/trends/systemCpu
        //
        // Returns system-wide CPU percentage trend data for sparklines
        //
        [HttpGet("trends/systemCpu")]
        public async Task<IActionResult> GetSystemCpuTrends([FromQuery] string appName = null,
                                                             [FromQuery] int hours = 24)
        {
            try
            {
                DateTime startTime = DateTime.UtcNow.AddHours(-hours);

                await using (TelemetryContext context = new TelemetryContext())
                {
                    IQueryable<TelemetrySnapshot> query = context.TelemetrySnapshots
                        .Include(s => s.telemetryApplication)
                        .Where(s => s.collectedAt >= startTime && s.isOnline && s.systemCpuPercent.HasValue)
                        .AsQueryable();

                    if (!string.IsNullOrWhiteSpace(appName))
                    {
                        query = query.Where(s => s.telemetryApplication.name == appName);
                    }

                    var data = await query
                        .OrderBy(s => s.collectedAt)
                        .Select(s => new
                        {
                            timestamp = s.collectedAt,
                            applicationName = s.telemetryApplication.name,
                            systemCpuPercent = s.systemCpuPercent
                        })
                        .ToListAsync()
                        .ConfigureAwait(false);

                    return Ok(new { data, hours, count = data.Count });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system CPU trends");
                return Problem("Failed to retrieve system CPU trends");
            }
        }


        //
        // GET: api/Telemetry/trends/sessions
        //
        // Returns active session count trends for charting
        //
        [HttpGet("trends/sessions")]
        public async Task<IActionResult> GetSessionTrends([FromQuery] string appName = null,
                                                          [FromQuery] int hours = 24)
        {
            try
            {
                DateTime startTime = DateTime.UtcNow.AddHours(-hours);

                await using (TelemetryContext context = new TelemetryContext())
                {
                    IQueryable<TelemetrySessionSnapshot> query = context.TelemetrySessionSnapshots
                                                                        .Include(ss => ss.telemetrySnapshot)
                                                                            .ThenInclude(s => s.telemetryApplication)
                                                                        .Where(ss => ss.telemetrySnapshot.collectedAt >= startTime)
                                                                        .AsQueryable();

                    if (!string.IsNullOrWhiteSpace(appName))
                    {
                        query = query.Where(ss => ss.telemetrySnapshot.telemetryApplication.name == appName);
                    }

                    var data = await query
                        .OrderBy(ss => ss.telemetrySnapshot.collectedAt)
                        .Select(ss => new
                        {
                            timestamp = ss.telemetrySnapshot.collectedAt,
                            applicationName = ss.telemetrySnapshot.telemetryApplication.name,
                            activeSessionCount = ss.activeSessionCount,
                            expiredSessionCount = ss.expiredSessionCount
                        })
                        .ToListAsync()
                        .ConfigureAwait(false);

                    return Ok(new { data, hours, count = data.Count });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving session trends");
                return Problem("Failed to retrieve session trends");
            }
        }


        //
        // GET: api/Telemetry/trends/metrics
        //
        // Returns business metric trends for charting/sparklines
        //
        [HttpGet("trends/metrics")]
        public async Task<IActionResult> GetMetricTrends([FromQuery] string appName = null,
                                                         [FromQuery] string metricName = null,
                                                         [FromQuery] int hours = 24,
                                                         [FromQuery] int limit = 100)
        {
            try
            {
                DateTime startTime = DateTime.UtcNow.AddHours(-hours);

                await using (TelemetryContext context = new TelemetryContext())
                {
                    IQueryable<TelemetryApplicationMetric> query = context.TelemetryApplicationMetrics
                                                                          .Include(m => m.telemetrySnapshot)
                                                                             .ThenInclude(s => s.telemetryApplication)
                                                                          .Where(m => m.telemetrySnapshot.collectedAt >= startTime)
                                                                          .AsQueryable();

                    if (!string.IsNullOrWhiteSpace(appName))
                    {
                        query = query.Where(m => m.telemetrySnapshot.telemetryApplication.name == appName);
                    }

                    if (!string.IsNullOrWhiteSpace(metricName))
                    {
                        query = query.Where(m => m.metricName == metricName);
                    }

                    var data = await query
                        .OrderByDescending(m => m.telemetrySnapshot.collectedAt)
                        .Take(limit)
                        .Select(m => new
                        {
                            timestamp = m.telemetrySnapshot.collectedAt,
                            applicationName = m.telemetrySnapshot.telemetryApplication.name,
                            metricName = m.metricName,
                            value = m.numericValue,
                            state = m.state
                        })
                        .ToListAsync()
                        .ConfigureAwait(false);

                    // Also return available metric names for the filter dropdown
                    List<string> availableMetrics = await context.TelemetryApplicationMetrics
                                                                 .Where(m => m.telemetrySnapshot.collectedAt >= startTime)
                                                                 .Select(m => m.metricName)
                                                                 .Distinct()
                                                                 .ToListAsync()
                                                                 .ConfigureAwait(false);

                    return Ok(new { data, hours, count = data.Count, availableMetrics });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving metric trends");
                return Problem("Failed to retrieve metric trends");
            }
        }


        //
        // GET: api/Telemetry/fleetMetrics
        //
        // Returns aggregated metrics across all applications for fleet overview
        //
        [HttpGet("fleetMetrics")]
        public async Task<IActionResult> GetFleetMetrics()
        {
            try
            {
                await using (TelemetryContext context = new TelemetryContext())
                {
                    // Get the latest snapshot for each application
                    List<TelemetrySnapshot> latestSnapshots = await context.TelemetrySnapshots.Include(s => s.telemetryApplication)
                                                                                              .Include(s => s.TelemetryApplicationMetrics)
                                                                                              .Include(s => s.TelemetryLogErrors)
                                                                                              .GroupBy(s => s.telemetryApplicationId)
                                                                                              .Select(g => g.OrderByDescending(s => s.collectedAt).FirstOrDefault())
                                                                                              .ToListAsync()
                                                                                              .ConfigureAwait(false);

                    // Aggregate numeric metrics by name
                    var metricAggregates = latestSnapshots
                        .SelectMany(s => s.TelemetryApplicationMetrics)
                        .Where(m => m.numericValue.HasValue)
                        .GroupBy(m => m.metricName)
                        .Select(g => new
                        {
                            metricName = g.Key,
                            total = g.Sum(m => m.numericValue ?? 0),
                            average = g.Average(m => m.numericValue ?? 0),
                            count = g.Count(),
                            worstState = g.Max(m => m.state)
                        })
                        .ToList();

                    // Aggregate system metrics
                    var systemAggregates = new
                    {
                        applicationCount = latestSnapshots.Count,
                        onlineCount = latestSnapshots.Count(s => s.isOnline),
                        totalMemoryMB = latestSnapshots.Where(s => s.isOnline).Sum(s => s.memoryWorkingSetMB ?? 0),
                        avgCpuPercent = latestSnapshots.Where(s => s.isOnline && s.cpuPercent.HasValue)
                            .Select(s => s.cpuPercent.Value)
                            .DefaultIfEmpty(0)
                            .Average(),
                        totalLogErrors = latestSnapshots.Sum(s => s.TelemetryLogErrors.Sum(e => e.occurrenceCount)),
                        avgSystemMemoryPercent = latestSnapshots.Where(s => s.isOnline && s.systemMemoryPercent.HasValue)
                            .Select(s => s.systemMemoryPercent.Value)
                            .DefaultIfEmpty()
                            .Average() is double memAvg && memAvg > 0 ? (double?)memAvg : null,
                        avgSystemCpuPercent = latestSnapshots.Where(s => s.isOnline && s.systemCpuPercent.HasValue)
                            .Select(s => s.systemCpuPercent.Value)
                            .DefaultIfEmpty()
                            .Average() is double cpuAvg && cpuAvg > 0 ? (double?)cpuAvg : null
                    };

                    return Ok(new
                    {
                        system = systemAggregates,
                        metrics = metricAggregates,
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving fleet metrics");
                return Problem("Failed to retrieve fleet metrics");
            }
        }


        //
        // GET: api/Telemetry/errors
        //
        // Returns correlated error events with system state
        //
        [HttpGet("errors")]
        public async Task<IActionResult> GetErrors([FromQuery] string appName = null,
                                                   [FromQuery] DateTime? startDate = null,
                                                   [FromQuery] DateTime? endDate = null,
                                                   [FromQuery] int limit = 100)
        {
            try
            {
                await using (TelemetryContext context = new TelemetryContext())
                {
                    IQueryable<TelemetryErrorEvent> query = context.TelemetryErrorEvents.Include(e => e.telemetryApplication)
                                                                                        .AsQueryable();

                    if (!string.IsNullOrWhiteSpace(appName))
                    {
                        query = query.Where(e => e.telemetryApplication.name == appName);
                    }

                    if (startDate.HasValue)
                    {
                        query = query.Where(e => e.occurredAt >= startDate.Value);
                    }

                    if (endDate.HasValue)
                    {
                        query = query.Where(e => e.occurredAt <= endDate.Value);
                    }

                    var errors = await query.OrderByDescending(e => e.occurredAt)
                                            .Take(Math.Min(limit, 500))
                                            .Select(e => new
                                            {
                                                e.id,
                                                applicationName = e.telemetryApplication.name,
                                                e.occurredAt,
                                                e.auditTypeName,
                                                e.moduleName,
                                                e.entityName,
                                                e.userName,
                                                e.message
                                            })
                                            .ToListAsync()
                                            .ConfigureAwait(false);

                    return Ok(new { errors, count = errors.Count });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving telemetry errors");
                return Problem("Failed to retrieve telemetry errors");
            }
        }


        //
        // GET: api/Telemetry/summary
        //
        // Returns dashboard summary with latest snapshot and aggregates
        //
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                await using (TelemetryContext context = new TelemetryContext())
                {
                    // Get all applications
                    var applications = await context.TelemetryApplications.Select(a => new
                                                                            {
                                                                                a.name,
                                                                                a.url,
                                                                                a.isSelf,
                                                                                a.firstSeen,
                                                                                a.lastSeen
                                                                            })
                                                                            .ToListAsync()
                                                                            .ConfigureAwait(false);

                    // Get latest snapshot per application using a different approach:
                    // First get the max collectedAt per app, then fetch those snapshots
                    List<int> latestSnapshotIds = await context.TelemetrySnapshots.GroupBy(s => s.telemetryApplicationId)
                                                                            .Select(g => g.Max(s => s.id))
                                                                            .ToListAsync()
                                                                            .ConfigureAwait(false);

                    var latestSnapshots = await context.TelemetrySnapshots
                        .Include(s => s.telemetryApplication)
                        .Where(s => latestSnapshotIds.Contains(s.id))
                        .Select(s => new
                        {
                            applicationName = s.telemetryApplication.name,
                            s.collectedAt,
                            s.isOnline,
                            s.uptimeSeconds,
                            s.memoryWorkingSetMB,
                            s.memoryGcHeapMB,
                            s.cpuPercent,
                            s.machineName,
                            s.systemMemoryPercent,
                            s.systemCpuPercent
                        })
                        .ToListAsync()
                        .ConfigureAwait(false);

                    // Get last collection run
                    var lastRun = await context.TelemetryCollectionRuns
                        .OrderByDescending(r => r.startTime)
                        .Select(r => new
                        {
                            r.startTime,
                            r.endTime,
                            r.applicationsPolled,
                            r.applicationsSucceeded,
                            r.errorMessage
                        })
                        .FirstOrDefaultAsync()
                        .ConfigureAwait(false);

                    // Get 24h statistics - use separate queries instead of complex GroupBy
                    DateTime last24h = DateTime.UtcNow.AddHours(-24);
                    
                    int totalSnapshots24h = await context.TelemetrySnapshots
                        .Where(s => s.collectedAt >= last24h)
                        .CountAsync()
                        .ConfigureAwait(false);

                    int onlineCount24h = await context.TelemetrySnapshots
                        .Where(s => s.collectedAt >= last24h && s.isOnline)
                        .CountAsync()
                        .ConfigureAwait(false);

                    List<double> memoryStats = await context.TelemetrySnapshots
                        .Where(s => s.collectedAt >= last24h && s.memoryWorkingSetMB.HasValue)
                        .Select(s => s.memoryWorkingSetMB.Value)
                        .ToListAsync()
                        .ConfigureAwait(false);

                    double? avgMemoryMB = memoryStats.Any() ? memoryStats.Average() : null;
                    double? maxMemoryMB = memoryStats.Any() ? memoryStats.Max() : null;

                    // Get error count for last 24h
                    int errorCount24h = await context.TelemetryErrorEvents
                        .Where(e => e.occurredAt >= last24h)
                        .CountAsync()
                        .ConfigureAwait(false);

                    return Ok(new
                    {
                        applications,
                        latestSnapshots,
                        lastCollectionRun = lastRun,
                        last24Hours = new
                        {
                            totalSnapshots = totalSnapshots24h,
                            onlineCount = onlineCount24h,
                            avgMemoryMB,
                            maxMemoryMB,
                            errorCount = errorCount24h
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving telemetry summary");
                return Problem("Failed to retrieve telemetry summary");
            }
        }


        //
        // GET: api/Telemetry/applications
        //
        // Returns list of monitored applications
        //
        [HttpGet("applications")]
        public async Task<IActionResult> GetApplications()
        {
            try
            {
                await using (TelemetryContext context = new TelemetryContext())
                {
                    var applications = await context.TelemetryApplications
                        .Select(a => new
                        {
                            a.id,
                            a.name,
                            a.url,
                            a.isSelf,
                            a.firstSeen,
                            a.lastSeen
                        })
                        .OrderBy(a => a.name)
                        .ToListAsync()
                        .ConfigureAwait(false);

                    return Ok(new { applications, count = applications.Count });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving telemetry applications");
                return Problem("Failed to retrieve telemetry applications");
            }
        }


        //
        // GET: api/Telemetry/collection-runs
        //
        // Returns recent collection run history
        //
        [HttpGet("collection-runs")]
        public async Task<IActionResult> GetCollectionRuns([FromQuery] int limit = 50)
        {
            try
            {
                await using (TelemetryContext context = new TelemetryContext())
                {
                    var runs = await context.TelemetryCollectionRuns
                        .OrderByDescending(r => r.startTime)
                        .Take(Math.Min(limit, 200))
                        .Select(r => new
                        {
                            r.id,
                            r.startTime,
                            r.endTime,
                            durationMs = r.endTime.HasValue 
                                ? (r.endTime.Value - r.startTime).TotalMilliseconds 
                                : (double?)null,
                            r.applicationsPolled,
                            r.applicationsSucceeded,
                            r.errorMessage
                        })
                        .ToListAsync()
                        .ConfigureAwait(false);

                    return Ok(new { runs, count = runs.Count });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving collection runs");
                return Problem("Failed to retrieve collection runs");
            }
        }
    }
}
