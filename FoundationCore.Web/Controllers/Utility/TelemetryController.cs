//
// Telemetry Controller
//
// API endpoints for querying historical telemetry data collected by the TelemetryCollectorService.
// Provides trend analysis and historical snapshots for system administrators.
//
using Foundation.Security;
using Foundation.Telemetry.Telemetry.Database;
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
        public async Task<IActionResult> GetSnapshots(
            [FromQuery] string appName = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int limit = 100)
        {
            try
            {
                using (var context = new TelemetryContext())
                {
                    var query = context.TelemetrySnapshots
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
        // GET: api/Telemetry/trends/memory
        //
        // Returns memory usage trend data for charting
        //
        [HttpGet("trends/memory")]
        public async Task<IActionResult> GetMemoryTrends(
            [FromQuery] string appName = null,
            [FromQuery] int hours = 24)
        {
            try
            {
                var startTime = DateTime.UtcNow.AddHours(-hours);

                using (var context = new TelemetryContext())
                {
                    var query = context.TelemetrySnapshots
                        .Include(s => s.telemetryApplication)
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
                var startTime = DateTime.UtcNow.AddHours(-hours);

                using (var context = new TelemetryContext())
                {
                    var query = context.TelemetryDiskHealths
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
        // GET: api/Telemetry/trends/sessions
        //
        // Returns active session count trends for charting
        //
        [HttpGet("trends/sessions")]
        public async Task<IActionResult> GetSessionTrends(
            [FromQuery] string appName = null,
            [FromQuery] int hours = 24)
        {
            try
            {
                var startTime = DateTime.UtcNow.AddHours(-hours);

                using (var context = new TelemetryContext())
                {
                    var query = context.TelemetrySessionSnapshots
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
        // GET: api/Telemetry/errors
        //
        // Returns correlated error events with system state
        //
        [HttpGet("errors")]
        public async Task<IActionResult> GetErrors(
            [FromQuery] string appName = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int limit = 100)
        {
            try
            {
                using (var context = new TelemetryContext())
                {
                    var query = context.TelemetryErrorEvents
                        .Include(e => e.telemetryApplication)
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

                    var errors = await query
                        .OrderByDescending(e => e.occurredAt)
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
                using (var context = new TelemetryContext())
                {
                    // Get all applications
                    var applications = await context.TelemetryApplications
                        .Select(a => new
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
                    var latestSnapshotIds = await context.TelemetrySnapshots
                        .GroupBy(s => s.telemetryApplicationId)
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
                            s.machineName
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
                    var last24h = DateTime.UtcNow.AddHours(-24);
                    
                    var totalSnapshots24h = await context.TelemetrySnapshots
                        .Where(s => s.collectedAt >= last24h)
                        .CountAsync()
                        .ConfigureAwait(false);

                    var onlineCount24h = await context.TelemetrySnapshots
                        .Where(s => s.collectedAt >= last24h && s.isOnline)
                        .CountAsync()
                        .ConfigureAwait(false);

                    var memoryStats = await context.TelemetrySnapshots
                        .Where(s => s.collectedAt >= last24h && s.memoryWorkingSetMB.HasValue)
                        .Select(s => s.memoryWorkingSetMB.Value)
                        .ToListAsync()
                        .ConfigureAwait(false);

                    double? avgMemoryMB = memoryStats.Any() ? memoryStats.Average() : null;
                    double? maxMemoryMB = memoryStats.Any() ? memoryStats.Max() : null;

                    // Get error count for last 24h
                    var errorCount24h = await context.TelemetryErrorEvents
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
                using (var context = new TelemetryContext())
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
                using (var context = new TelemetryContext())
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
