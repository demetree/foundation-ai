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
    }
}
