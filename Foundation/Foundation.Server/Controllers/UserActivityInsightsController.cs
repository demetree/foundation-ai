//
// User Activity Insights Controller
//
// Provides a pre-aggregated analytics endpoint for the User Activity Insights Dashboard.
// All audit event types (including custom/ad-hoc events from client-side CreateEvent) are included.
//
// AI-assisted development - February 2026
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Foundation.Auditor;
using Foundation.Auditor.Database;
using Foundation.Security;

namespace Foundation.Server.Controllers
{
    [ApiController]
    [Route("api/UserActivityInsights")]
    [Authorize]
    public class UserActivityInsightsController : SecureWebAPIController
    {
        private readonly AuditorContext _context;
        private readonly ILogger<UserActivityInsightsController> _logger;

        public UserActivityInsightsController(AuditorContext context,
                                               ILogger<UserActivityInsightsController> logger)
            : base("Auditor", "UserActivityInsights")           // We use the auditor module for this so the security roles will be found.
        {
            _context = context;
            _logger = logger;
            _context.Database.SetCommandTimeout(60);    // analytics queries may be heavier than standard CRUD
        }


        /// <summary>
        /// Returns pre-aggregated analytics data for the User Activity Insights Dashboard.
        /// Includes all audit event types: standard CRUD operations AND custom/ad-hoc events
        /// created via the client-side CreateEvent endpoint (type=Miscellaneous).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(UserActivityInsightsResponse), 200)]
        public async Task<IActionResult> GetInsights([FromQuery] DateTime? startTime = null,
                                                      [FromQuery] DateTime? stopTime = null,
                                                      [FromQuery] string userName = null,
                                                      CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            //
            // Security check — require read privilege (same as AuditEventsController)
            //
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(0, cancellationToken) == false)
            {
                return Forbid();
            }

            //
            // Flush memory queue if in memory queue mode so data is current
            //
            if (AuditEngine.Instance.GetAuditorMode() == AuditEngine.AuditorMode.MemoryQueueWithOneMinuteFlush)
            {
                AuditEngine.FlushMemoryQueueToDatabase();
            }

            //
            // Default time range: last 30 days
            //
            if (startTime.HasValue && startTime.Value.Kind != DateTimeKind.Utc)
                startTime = startTime.Value.ToUniversalTime();
            if (stopTime.HasValue && stopTime.Value.Kind != DateTimeKind.Utc)
                stopTime = stopTime.Value.ToUniversalTime();

            DateTime effectiveStop = stopTime ?? DateTime.UtcNow;
            DateTime effectiveStart = startTime ?? effectiveStop.AddDays(-30);

            // Safety: cap at 365 days
            if ((effectiveStop - effectiveStart).TotalDays > 365)
                effectiveStart = effectiveStop.AddDays(-365);

            try
            {
                //
                // Load dimension lookup tables into dictionaries for fast name resolution
                // (same performance pattern as AuditEventsController)
                //
                var auditUsers = await _context.AuditUsers.AsNoTracking()
                    .ToDictionaryAsync(u => u.id, u => u.name, cancellationToken);
                var auditModules = await _context.AuditModules.AsNoTracking()
                    .ToDictionaryAsync(m => m.id, m => m.name, cancellationToken);
                var auditTypes = await _context.AuditTypes.AsNoTracking()
                    .ToDictionaryAsync(t => t.id, t => t.name, cancellationToken);
                var auditSessions = await _context.AuditSessions.AsNoTracking()
                    .ToDictionaryAsync(s => s.id, s => s.name, cancellationToken);
                var auditModuleEntities = await _context.AuditModuleEntities.AsNoTracking()
                    .ToDictionaryAsync(me => me.id, me => new { me.name, me.auditModuleId }, cancellationToken);

                //
                // Base query — all events in the time range, with optional user filter.
                // Note: this includes ALL audit types (Login, Logout, ReadList, CreateEntity, ..., Miscellaneous)
                // so custom/ad-hoc events from client-side CreateEvent are captured.
                //
                IQueryable<AuditEvent> baseQuery = _context.AuditEvents
                    .AsNoTracking()
                    .Where(ae => ae.startTime >= effectiveStart && ae.startTime <= effectiveStop);

                if (!string.IsNullOrWhiteSpace(userName))
                {
                    // Find matching user IDs first, then filter by them
                    var matchingUserIds = auditUsers.Where(u => u.Value.ToUpper().Contains(userName.ToUpper())).Select(u => u.Key).ToList();
                    baseQuery = baseQuery.Where(ae => matchingUserIds.Contains(ae.auditUserId));
                }

                //
                // Materialize the lightweight projection we need for aggregation.
                // Only selecting the fields needed for analytics — no message, no state, no error details.
                //
                var events = await baseQuery.Select(ae => new
                {
                    ae.id,
                    ae.startTime,
                    ae.stopTime,
                    ae.completedSuccessfully,
                    ae.auditUserId,
                    ae.auditSessionId,
                    ae.auditTypeId,
                    ae.auditModuleId,
                    ae.auditModuleEntityId,
                    ae.message
                }).ToListAsync(cancellationToken);

                int totalEvents = events.Count;
                int uniqueUsers = events.Select(e => e.auditUserId).Distinct().Count();
                int uniqueSessions = events.Select(e => e.auditSessionId).Distinct().Count();
                int successCount = events.Count(e => e.completedSuccessfully);
                double successRate = totalEvents > 0 ? Math.Round((double)successCount / totalEvents * 100, 1) : 0;
                int daysInRange = Math.Max(1, (int)(effectiveStop - effectiveStart).TotalDays);
                double avgEventsPerDay = Math.Round((double)totalEvents / daysInRange, 1);

                //
                // Activity by hour (0-23)
                //
                var activityByHour = events
                    .GroupBy(e => e.startTime.Hour)
                    .Select(g => new HourlyActivity { Hour = g.Key, Count = g.Count() })
                    .OrderBy(h => h.Hour)
                    .ToList();

                //
                // Activity by day
                //
                var activityByDay = events
                    .GroupBy(e => e.startTime.Date)
                    .Select(g => new DailyActivity
                    {
                        Date = g.Key,
                        Count = g.Count(),
                        UniqueUsers = g.Select(e => e.auditUserId).Distinct().Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                //
                // Top users (top 20 by event count)
                //
                var topUsers = events
                    .GroupBy(e => e.auditUserId)
                    .Select(g =>
                    {
                        string name = auditUsers.ContainsKey(g.Key) ? auditUsers[g.Key] : $"User {g.Key}";
                        var topModuleId = g.GroupBy(e => e.auditModuleId)
                            .OrderByDescending(mg => mg.Count())
                            .FirstOrDefault()?.Key ?? 0;
                        int userSuccessCount = g.Count(e => e.completedSuccessfully);

                        return new TopUser
                        {
                            UserName = name,
                            EventCount = g.Count(),
                            LastActive = g.Max(e => e.startTime),
                            TopModule = auditModules.ContainsKey(topModuleId) ? auditModules[topModuleId] : "Unknown",
                            SuccessRate = g.Count() > 0 ? Math.Round((double)userSuccessCount / g.Count() * 100, 1) : 0
                        };
                    })
                    .OrderByDescending(u => u.EventCount)
                    .Take(20)
                    .ToList();

                //
                // Top modules
                //
                var topModules = events
                    .GroupBy(e => e.auditModuleId)
                    .Select(g =>
                    {
                        string name = auditModules.ContainsKey(g.Key) ? auditModules[g.Key] : $"Module {g.Key}";
                        // Read types: 3 (ReadList), 4 (ReadListRedacted), 5 (ReadEntity), 6 (ReadEntityRedacted), 15 (Search)
                        int readCount = g.Count(e => e.auditTypeId == 3 || e.auditTypeId == 4 || e.auditTypeId == 5 || e.auditTypeId == 6 || e.auditTypeId == 15);
                        // Write types: 7 (Create), 8 (Update), 9 (Delete), 10 (WriteList)
                        int writeCount = g.Count(e => e.auditTypeId == 7 || e.auditTypeId == 8 || e.auditTypeId == 9 || e.auditTypeId == 10);

                        return new TopModule
                        {
                            ModuleName = name,
                            EventCount = g.Count(),
                            UniqueUsers = g.Select(e => e.auditUserId).Distinct().Count(),
                            ReadCount = readCount,
                            WriteCount = writeCount
                        };
                    })
                    .OrderByDescending(m => m.EventCount)
                    .Take(20)
                    .ToList();

                //
                // Module → Entity breakdown (all module-entity combinations)
                //
                var moduleEntityBreakdown = events
                    .Where(e => e.auditModuleEntityId > 0)
                    .GroupBy(e => new { e.auditModuleId, e.auditModuleEntityId })
                    .Select(g =>
                    {
                        string moduleName = auditModules.ContainsKey(g.Key.auditModuleId) ? auditModules[g.Key.auditModuleId] : $"Module {g.Key.auditModuleId}";
                        string entityName = auditModuleEntities.ContainsKey(g.Key.auditModuleEntityId)
                            ? auditModuleEntities[g.Key.auditModuleEntityId].name
                            : $"Entity {g.Key.auditModuleEntityId}";
                        int readCount = g.Count(e => e.auditTypeId == 3 || e.auditTypeId == 4 || e.auditTypeId == 5 || e.auditTypeId == 6 || e.auditTypeId == 15);
                        int writeCount = g.Count(e => e.auditTypeId == 7 || e.auditTypeId == 8 || e.auditTypeId == 9 || e.auditTypeId == 10);

                        return new ModuleEntityDetail
                        {
                            ModuleName = moduleName,
                            EntityName = entityName,
                            EventCount = g.Count(),
                            ReadCount = readCount,
                            WriteCount = writeCount,
                            UniqueUsers = g.Select(e => e.auditUserId).Distinct().Count()
                        };
                    })
                    .OrderByDescending(me => me.EventCount)
                    .ToList();

                //
                // Event type breakdown
                //
                var eventTypeBreakdown = events
                    .GroupBy(e => e.auditTypeId)
                    .Select(g => new EventTypeBreakdown
                    {
                        AuditTypeName = auditTypes.ContainsKey(g.Key) ? auditTypes[g.Key] : $"Type {g.Key}",
                        AuditTypeId = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(t => t.Count)
                    .ToList();

                //
                // User → Module matrix (for Sankey: top 10 users × their modules)
                //
                var top10UserIds = topUsers.Take(10).Select(u => auditUsers.FirstOrDefault(kv => kv.Value == u.UserName).Key).ToList();
                var userModuleMatrix = events
                    .Where(e => top10UserIds.Contains(e.auditUserId))
                    .GroupBy(e => new { e.auditUserId, e.auditModuleId })
                    .Select(g => new UserModuleLink
                    {
                        UserName = auditUsers.ContainsKey(g.Key.auditUserId) ? auditUsers[g.Key.auditUserId] : $"User {g.Key.auditUserId}",
                        ModuleName = auditModules.ContainsKey(g.Key.auditModuleId) ? auditModules[g.Key.auditModuleId] : $"Module {g.Key.auditModuleId}",
                        Count = g.Count()
                    })
                    .OrderByDescending(l => l.Count)
                    .ToList();

                //
                // Recent sessions (last 50 distinct sessions)
                //
                var recentSessions = events
                    .GroupBy(e => e.auditSessionId)
                    .Select(g =>
                    {
                        var sessionEvents = g.OrderBy(e => e.startTime).ToList();
                        var sessionUserId = sessionEvents.First().auditUserId;

                        return new RecentSession
                        {
                            UserName = auditUsers.ContainsKey(sessionUserId) ? auditUsers[sessionUserId] : $"User {sessionUserId}",
                            SessionId = auditSessions.ContainsKey(g.Key) ? auditSessions[g.Key] : $"{g.Key}",
                            SessionStart = sessionEvents.First().startTime,
                            SessionEnd = sessionEvents.Last().stopTime,
                            EventCount = g.Count(),
                            Modules = sessionEvents.Select(e => e.auditModuleId)
                                .Distinct()
                                .Select(mid => auditModules.ContainsKey(mid) ? auditModules[mid] : $"Module {mid}")
                                .ToList()
                        };
                    })
                    .OrderByDescending(s => s.SessionStart)
                    .Take(50)
                    .ToList();

                //
                // Failure hotspots (users + modules with the most failures)
                //
                var failureHotspots = events
                    .Where(e => !e.completedSuccessfully)
                    .GroupBy(e => new { e.auditUserId, e.auditModuleId })
                    .Select(g => new FailureHotspot
                    {
                        UserName = auditUsers.ContainsKey(g.Key.auditUserId) ? auditUsers[g.Key.auditUserId] : $"User {g.Key.auditUserId}",
                        ModuleName = auditModules.ContainsKey(g.Key.auditModuleId) ? auditModules[g.Key.auditModuleId] : $"Module {g.Key.auditModuleId}",
                        FailureCount = g.Count(),
                        LastFailure = g.Max(e => e.startTime)
                    })
                    .OrderByDescending(f => f.FailureCount)
                    .Take(20)
                    .ToList();


                var response = new UserActivityInsightsResponse
                {
                    Summary = new InsightsSummary
                    {
                        TotalEvents = totalEvents,
                        UniqueUsers = uniqueUsers,
                        UniqueSessions = uniqueSessions,
                        SuccessRate = successRate,
                        AvgEventsPerDay = avgEventsPerDay,
                        StartDate = effectiveStart,
                        EndDate = effectiveStop
                    },
                    ActivityByHour = activityByHour,
                    ActivityByDay = activityByDay,
                    TopUsers = topUsers,
                    TopModules = topModules,
                    EventTypeBreakdown = eventTypeBreakdown,
                    UserModuleMatrix = userModuleMatrix,
                    RecentSessions = recentSessions,
                    FailureHotspots = failureHotspots,
                    ModuleEntityBreakdown = moduleEntityBreakdown
                };

                await CreateAuditEventAsync(AuditEngine.AuditType.ReadList,
                    $"User Activity Insights read. Time range: {effectiveStart:yyyy-MM-dd} to {effectiveStop:yyyy-MM-dd}. Events analyzed: {totalEvents}.");

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate user activity insights");
                return Problem($"Failed to generate insights: {ex.Message}");
            }
        }


        #region Response DTOs

        public class UserActivityInsightsResponse
        {
            public InsightsSummary Summary { get; set; }
            public List<HourlyActivity> ActivityByHour { get; set; }
            public List<DailyActivity> ActivityByDay { get; set; }
            public List<TopUser> TopUsers { get; set; }
            public List<TopModule> TopModules { get; set; }
            public List<EventTypeBreakdown> EventTypeBreakdown { get; set; }
            public List<UserModuleLink> UserModuleMatrix { get; set; }
            public List<RecentSession> RecentSessions { get; set; }
            public List<FailureHotspot> FailureHotspots { get; set; }
            public List<ModuleEntityDetail> ModuleEntityBreakdown { get; set; }
        }

        public class InsightsSummary
        {
            public int TotalEvents { get; set; }
            public int UniqueUsers { get; set; }
            public int UniqueSessions { get; set; }
            public double SuccessRate { get; set; }
            public double AvgEventsPerDay { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }

        public class HourlyActivity
        {
            public int Hour { get; set; }
            public int Count { get; set; }
        }

        public class DailyActivity
        {
            public DateTime Date { get; set; }
            public int Count { get; set; }
            public int UniqueUsers { get; set; }
        }

        public class TopUser
        {
            public string UserName { get; set; }
            public int EventCount { get; set; }
            public DateTime LastActive { get; set; }
            public string TopModule { get; set; }
            public double SuccessRate { get; set; }
        }

        public class TopModule
        {
            public string ModuleName { get; set; }
            public int EventCount { get; set; }
            public int UniqueUsers { get; set; }
            public int ReadCount { get; set; }
            public int WriteCount { get; set; }
        }

        public class EventTypeBreakdown
        {
            public string AuditTypeName { get; set; }
            public int AuditTypeId { get; set; }
            public int Count { get; set; }
        }

        public class UserModuleLink
        {
            public string UserName { get; set; }
            public string ModuleName { get; set; }
            public int Count { get; set; }
        }

        public class RecentSession
        {
            public string UserName { get; set; }
            public string SessionId { get; set; }
            public DateTime SessionStart { get; set; }
            public DateTime SessionEnd { get; set; }
            public int EventCount { get; set; }
            public List<string> Modules { get; set; }
        }

        public class FailureHotspot
        {
            public string UserName { get; set; }
            public string ModuleName { get; set; }
            public int FailureCount { get; set; }
            public DateTime LastFailure { get; set; }
        }

        public class ModuleEntityDetail
        {
            public string ModuleName { get; set; }
            public string EntityName { get; set; }
            public int EventCount { get; set; }
            public int ReadCount { get; set; }
            public int WriteCount { get; set; }
            public int UniqueUsers { get; set; }
        }

        #endregion
    }
}
