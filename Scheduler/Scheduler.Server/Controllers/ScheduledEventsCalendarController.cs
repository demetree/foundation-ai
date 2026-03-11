//
// ScheduledEventsCalendarController.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// This partial class extends ScheduledEventsController with a calendar-optimized endpoint that returns
// both standalone events and expanded recurring event instances within a date range.
//
// This keeps the auto-generated CRUD controller untouched while adding calendar-specific behavior.
//
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Foundation.Security;
using Foundation.Auditor;
using Foundation.Controllers;
using Foundation.Security.Database;
using static Foundation.Auditor.AuditEngine;
using Foundation.Scheduler.Database;
using Scheduler.Server.Services;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    ///
    /// Partial class extending ScheduledEventsController with a calendar-optimized endpoint.
    ///
    /// The GetCalendarEvents endpoint returns all events visible within a date range, including
    /// virtual instances expanded from recurring event series.  This gives calendar consumers
    /// a single call to get everything they need for display.
    ///
    /// </summary>
    public partial class ScheduledEventsController
    {
        /// <summary>
        ///
        /// Gets all events for calendar display within the specified date range.
        ///
        /// This includes:
        ///   - Standalone (non-recurring) events whose start or end falls within the range
        ///   - Virtual instances expanded from recurring events based on their RecurrenceRule
        ///
        /// Recurring master events are NOT returned directly — only their expanded instances.
        /// RecurrenceException records are applied to skip or move individual occurrences.
        ///
        /// </summary>
        /// <param name="rangeStart">Start of the date range (UTC)</param>
        /// <param name="rangeEnd">End of the date range (UTC)</param>
        /// <param name="expansionService">Injected RecurrenceExpansionService</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of ScheduledEventOutputDTO objects for calendar display</returns>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/ScheduledEvents/Calendar")]
        public async Task<IActionResult> GetCalendarEvents(
            DateTime rangeStart,
            DateTime rangeEnd,
            string? calendarIds,
            [FromServices] RecurrenceExpansionService expansionService,
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
            }


            //
            // Normalize dates to UTC
            //
            if (rangeStart.Kind != DateTimeKind.Utc)
            {
                rangeStart = rangeStart.ToUniversalTime();
            }

            if (rangeEnd.Kind != DateTimeKind.Utc)
            {
                rangeEnd = rangeEnd.ToUniversalTime();
            }


            //
            // Parse optional calendar filter
            //
            // When calendarIds are provided, only events assigned to those calendars (via the EventCalendar
            // join table) will be returned.  When omitted, all events are returned (backwards compatible).
            //
            HashSet<int>? allowedEventIds = null;

            if (!string.IsNullOrWhiteSpace(calendarIds))
            {
                List<int> calIdList = calendarIds.Split(',')
                    .Select(s => int.TryParse(s.Trim(), out int v) ? v : (int?)null)
                    .Where(v => v.HasValue)
                    .Select(v => v!.Value)
                    .ToList();

                if (calIdList.Count > 0)
                {
                    allowedEventIds = (await _context.EventCalendars
                        .Where(ec => calIdList.Contains(ec.calendarId))
                        .Where(ec => ec.tenantGuid == userTenantGuid)
                        .Where(ec => ec.active == true && ec.deleted == false)
                        .Select(ec => ec.scheduledEventId)
                        .Distinct()
                        .ToListAsync(cancellationToken)
                        .ConfigureAwait(false))
                        .ToHashSet();
                }
            }


            //
            // Step 1: Fetch all standalone (non-recurring) events that overlap with the date range.
            //
            // An event overlaps if its start is before the range end AND its end is after the range start.
            //
            List<ScheduledEvent> standaloneEvents = await _context.ScheduledEvents
                .Where(se => se.tenantGuid == userTenantGuid)
                .Where(se => se.recurrenceRuleId == null)
                .Where(se => se.parentScheduledEventId == null)
                .Where(se => se.active == true)
                .Where(se => se.deleted == false)
                .Where(se => se.startDateTime <= rangeEnd && se.endDateTime >= rangeStart)
                .Where(se => allowedEventIds == null || allowedEventIds.Contains(se.id))
                .Include(se => se.bookingSourceType)
                .Include(se => se.client)
                .Include(se => se.crew)
                .Include(se => se.eventStatus)
                .Include(se => se.office)
                .Include(se => se.priority)
                .Include(se => se.resource)
                .Include(se => se.scheduledEventTemplate)
                .Include(se => se.schedulingTarget)
                .Include(se => se.timeZone)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);


            //
            // Step 2: Fetch all master recurring events (events that have a RecurrenceRule).
            //
            // We include the RecurrenceRule and its frequency for expansion.
            // We don't filter by date range here because the master event's dates may be outside the
            // range, but its expanded instances might be inside.
            //
            List<ScheduledEvent> masterRecurringEvents = await _context.ScheduledEvents
                .Where(se => se.tenantGuid == userTenantGuid)
                .Where(se => se.recurrenceRuleId != null)
                .Where(se => se.parentScheduledEventId == null)
                .Where(se => se.active == true)
                .Where(se => se.deleted == false)
                .Where(se => allowedEventIds == null || allowedEventIds.Contains(se.id))
                .Include(se => se.recurrenceRule)
                    .ThenInclude(rr => rr.recurrenceFrequency)
                .Include(se => se.bookingSourceType)
                .Include(se => se.client)
                .Include(se => se.crew)
                .Include(se => se.eventStatus)
                .Include(se => se.office)
                .Include(se => se.priority)
                .Include(se => se.resource)
                .Include(se => se.scheduledEventTemplate)
                .Include(se => se.schedulingTarget)
                .Include(se => se.timeZone)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);


            //
            // Step 3: Fetch RecurrenceException records for all master recurring events.
            //
            List<int> masterEventIds = masterRecurringEvents.Select(e => e.id).ToList();

            List<RecurrenceException> allExceptions = new List<RecurrenceException>();

            if (masterEventIds.Count > 0)
            {
                allExceptions = await _context.RecurrenceExceptions
                    .Where(re => masterEventIds.Contains(re.scheduledEventId))
                    .Where(re => re.tenantGuid == userTenantGuid)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
            }


            //
            // Step 4: Also fetch any persisted exception instances (child events that were split off
            // from a series — they have parentScheduledEventId set).  These are real database rows
            // that represent "this event only" edits.
            //
            List<ScheduledEvent> persistedExceptionInstances = new List<ScheduledEvent>();

            if (masterEventIds.Count > 0)
            {
                persistedExceptionInstances = await _context.ScheduledEvents
                    .Where(se => se.tenantGuid == userTenantGuid)
                    .Where(se => se.parentScheduledEventId != null && masterEventIds.Contains(se.parentScheduledEventId.Value))
                    .Where(se => se.active == true)
                    .Where(se => se.deleted == false)
                    .Where(se => se.startDateTime <= rangeEnd && se.endDateTime >= rangeStart)
                    .Where(se => allowedEventIds == null || allowedEventIds.Contains(se.id))
                    .Include(se => se.bookingSourceType)
                    .Include(se => se.client)
                    .Include(se => se.crew)
                    .Include(se => se.eventStatus)
                    .Include(se => se.office)
                    .Include(se => se.priority)
                    .Include(se => se.resource)
                    .Include(se => se.scheduledEventTemplate)
                    .Include(se => se.schedulingTarget)
                    .Include(se => se.timeZone)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
            }


            //
            // Step 5: Expand each master recurring event into virtual instances and combine everything.
            //
            List<ScheduledEvent> resultList = new List<ScheduledEvent>();

            //
            // Add standalone events
            //
            resultList.AddRange(standaloneEvents);

            //
            // Add persisted exception instances (real "this event only" edits)
            //
            resultList.AddRange(persistedExceptionInstances);

            //
            // Expand recurring events and add virtual instances
            //
            foreach (ScheduledEvent masterEvent in masterRecurringEvents)
            {
                List<RecurrenceException> exceptionsForThisEvent = allExceptions
                    .Where(re => re.scheduledEventId == masterEvent.id).ToList();

                List<ScheduledEvent> expandedInstances = expansionService.ExpandOccurrences(
                    masterEvent: masterEvent,
                    exceptions: exceptionsForThisEvent,
                    rangeStart: rangeStart,
                    rangeEnd: rangeEnd);

                resultList.AddRange(expandedInstances);
            }


            //
            // Step 6: Convert to output DTOs and return
            //
            List<ScheduledEvent.ScheduledEventOutputDTO> outputDTOList = resultList
                .Select(se => se.ToOutputDTO())
                .ToList();


            await CreateAuditEventAsync(AuditType.ReadList, "Calendar events retrieved for range " + rangeStart.ToString("s") + " to " + rangeEnd.ToString("s") + ".  Count: " + outputDTOList.Count, securityUser?.accountName);


            return Ok(outputDTOList);
        }


        /// <summary>
        /// Returns a printer-friendly HTML page of the event schedule for the given date range.
        ///
        /// Designed to be opened in a new tab and printed / posted on a bulletin board.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/ScheduledEvents/PrintSchedule")]
        public async Task<IActionResult> PrintSchedule(
            DateTime rangeStart,
            DateTime rangeEnd,
            int? schedulingTargetId = null,
            string? calendarIds = null,
            [FromServices] RecurrenceExpansionService expansionService = null,
            CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditType.Error,
                    "PrintSchedule requested by user with no tenant. User: " + securityUser?.accountName,
                    securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant.");
            }

            if (rangeStart.Kind != DateTimeKind.Utc) rangeStart = rangeStart.ToUniversalTime();
            if (rangeEnd.Kind != DateTimeKind.Utc) rangeEnd = rangeEnd.ToUniversalTime();

            //
            // Query events (standalone + recurrence expanded)
            //
            var query = _context.ScheduledEvents
                .Where(se => se.tenantGuid == userTenantGuid
                    && se.active == true && se.deleted == false
                    && se.recurrenceRuleId == null
                    && se.parentScheduledEventId == null
                    && se.startDateTime <= rangeEnd && se.endDateTime >= rangeStart)
                .Include(se => se.schedulingTarget)
                .AsNoTracking();

            if (schedulingTargetId.HasValue)
            {
                query = query.Where(se => se.schedulingTargetId == schedulingTargetId.Value);
            }

            // Calendar filter
            HashSet<int>? allowedEventIds = null;
            if (!string.IsNullOrWhiteSpace(calendarIds))
            {
                var calIdList = calendarIds.Split(',')
                    .Select(s => int.TryParse(s.Trim(), out int v) ? v : (int?)null)
                    .Where(v => v.HasValue)
                    .Select(v => v!.Value)
                    .ToList();
                if (calIdList.Count > 0)
                {
                    allowedEventIds = (await _context.EventCalendars
                        .Where(ec => calIdList.Contains(ec.calendarId) && ec.tenantGuid == userTenantGuid && ec.active == true && ec.deleted == false)
                        .Select(ec => ec.scheduledEventId)
                        .Distinct()
                        .ToListAsync(cancellationToken)).ToHashSet();
                }
            }

            if (allowedEventIds != null)
                query = query.Where(se => allowedEventIds.Contains(se.id));

            var events = await query.OrderBy(se => se.startDateTime).ToListAsync(cancellationToken);

            //
            // Expand recurring events if the expansion service is available
            //
            if (expansionService != null)
            {
                var recurringQuery = _context.ScheduledEvents
                    .Where(se => se.tenantGuid == userTenantGuid && se.active == true && se.deleted == false
                        && se.recurrenceRuleId != null && se.parentScheduledEventId == null)
                    .Include(se => se.recurrenceRule).ThenInclude(rr => rr.recurrenceFrequency)
                    .Include(se => se.schedulingTarget)
                    .AsNoTracking();

                if (schedulingTargetId.HasValue)
                    recurringQuery = recurringQuery.Where(se => se.schedulingTargetId == schedulingTargetId.Value);
                if (allowedEventIds != null)
                    recurringQuery = recurringQuery.Where(se => allowedEventIds.Contains(se.id));

                var masters = await recurringQuery.ToListAsync(cancellationToken);
                var masterIds = masters.Select(e => e.id).ToList();
                var exceptions = masterIds.Count > 0
                    ? await _context.RecurrenceExceptions.Where(re => masterIds.Contains(re.scheduledEventId) && re.tenantGuid == userTenantGuid).AsNoTracking().ToListAsync(cancellationToken)
                    : new List<RecurrenceException>();

                foreach (var master in masters)
                {
                    var exForEvent = exceptions.Where(re => re.scheduledEventId == master.id).ToList();
                    events.AddRange(expansionService.ExpandOccurrences(master, exForEvent, rangeStart, rangeEnd));
                }
            }

            events = events.OrderBy(e => e.startDateTime).ToList();

            //
            // Group events by date (local time)
            //
            var grouped = events
                .GroupBy(e => e.startDateTime.ToLocalTime().Date)
                .OrderBy(g => g.Key);

            //
            // Build printable HTML
            //
            var html = new System.Text.StringBuilder();
            html.AppendLine("<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\">");
            html.AppendLine("<title>Event Schedule</title>");
            html.AppendLine("<style>");
            html.AppendLine("* { margin: 0; padding: 0; box-sizing: border-box; }");
            html.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; font-size: 11pt; color: #333; padding: 20px; }");
            html.AppendLine("h1 { font-size: 16pt; margin-bottom: 4px; }");
            html.AppendLine("h2 { font-size: 12pt; background: #2563eb; color: #fff; padding: 6px 12px; margin: 16px 0 0 0; border-radius: 4px 4px 0 0; }");
            html.AppendLine(".subtitle { color: #666; font-size: 10pt; margin-bottom: 12px; }");
            html.AppendLine("table { width: 100%; border-collapse: collapse; margin-bottom: 12px; }");
            html.AppendLine("th { background: #f1f5f9; text-align: left; padding: 6px 10px; font-size: 9pt; text-transform: uppercase; color: #64748b; border: 1px solid #e2e8f0; }");
            html.AppendLine("td { padding: 6px 10px; border: 1px solid #e2e8f0; font-size: 10pt; }");
            html.AppendLine("tr:nth-child(even) td { background: #f8fafc; }");
            html.AppendLine(".empty { color: #94a3b8; font-style: italic; padding: 10px; }");
            html.AppendLine("@media print { body { padding: 0; } h2 { break-after: avoid; } }");
            html.AppendLine("</style></head><body>");

            html.AppendLine($"<h1>Event Schedule</h1>");
            html.AppendLine($"<p class=\"subtitle\">{rangeStart.ToLocalTime():dddd, MMMM d, yyyy} — {rangeEnd.ToLocalTime():dddd, MMMM d, yyyy}</p>");

            foreach (var dayGroup in grouped)
            {
                html.AppendLine($"<h2>{dayGroup.Key:dddd, MMMM d, yyyy}</h2>");
                html.AppendLine("<table><thead><tr><th>Time</th><th>Event</th><th>Location</th><th>Calendar</th></tr></thead><tbody>");

                foreach (var ev in dayGroup)
                {
                    string timeStr = ev.startDateTime.ToLocalTime().ToString("h:mm tt") + " – " + ev.endDateTime.ToLocalTime().ToString("h:mm tt");
                    string name = System.Net.WebUtility.HtmlEncode(ev.name ?? "");
                    string loc = System.Net.WebUtility.HtmlEncode(ev.location ?? "—");
                    string cal = System.Net.WebUtility.HtmlEncode(ev.schedulingTarget?.name ?? "—");

                    html.AppendLine($"<tr><td>{timeStr}</td><td>{name}</td><td>{loc}</td><td>{cal}</td></tr>");
                }

                html.AppendLine("</tbody></table>");
            }

            if (!grouped.Any())
            {
                html.AppendLine("<p class=\"empty\">No events scheduled for this period.</p>");
            }

            html.AppendLine("</body></html>");

            return Content(html.ToString(), "text/html", System.Text.Encoding.UTF8);
        }
    }
}
