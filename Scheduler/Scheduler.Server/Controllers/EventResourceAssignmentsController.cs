using System;
using System.Threading;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Foundation.Security;
using Foundation.Auditor;
using Foundation.Controllers;
using Foundation.Security.Database;
using static Foundation.Auditor.AuditEngine;
using Foundation.Scheduler.Database;
using static Foundation.Scheduler.Database.EventResourceAssignment;

namespace Foundation.Scheduler.Controllers.WebAPI
{

    //
    // Extends the EventResourceAssignmentsController to add conflict checking
    //
    public partial class EventResourceAssignmentsController : SecureWebAPIController
	{

        /// <summary>
        /// Represents a conflict where a resource is already assigned to another event during an overlapping time period.
        /// </summary>
        public class AssignmentOverlapConflictDTO
        {
            public string Type => "AssignmentOverlap";
            public int ResourceId { get; set; }
            public int ConflictingAssignmentId { get; set; }
            public string ConflictingEventName { get; set; } = string.Empty;
            public int ProposedIndex { get; set; }
            public string Message { get; set; } = string.Empty;
        }

        /// <summary>
        /// Represents a conflict where a resource is unavailable due to a blackout (vacation, maintenance, etc.).
        /// </summary>
        public class BlackoutConflictDTO
        {
            public string Type => "BlackoutConflict";
            public int ResourceId { get; set; }
            public DateTime BlackoutStart { get; set; }
            public DateTime? BlackoutEnd { get; set; }
            public string Reason { get; set; } = string.Empty;
            public int ProposedIndex { get; set; }
            public string Message { get; set; } = string.Empty;
        }


        /// <summary>
        /// Checks proposed EventResourceAssignment records for conflicts with existing assignments
        /// and resource blackouts (ResourceAvailability records).
        ///
        /// Conflicts include:
        /// - Overlapping time ranges on the same resource (individual or crew member)
        /// - Proposed assignments that overlap active blackouts for any involved resource
        ///
        /// The endpoint accepts an array of proposed assignments (new or updated).
        /// It does NOT save anything — it only validates.
        ///
        /// Returns 200 OK with a list of strongly-typed conflict objects (empty list = no conflicts)
        /// Returns 400 BadRequest if input is invalid
        ///
        /// Rate limited to prevent abuse.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/EventResourceAssignments/CheckConflicts")]
        public async Task<IActionResult> CheckConflicts([FromBody] EventResourceAssignmentDTO[] proposedAssignments,
                                                        CancellationToken cancellationToken = default)
        {
            // Basic input validation — ensure we have data to work with
            if (proposedAssignments == null || proposedAssignments.Length == 0)
            {
                return BadRequest("No assignments provided for conflict check.");
            }

            StartAuditEventClock();

            // Security: User must have read access to view existing assignments and blackouts
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
                await CreateAuditEventAsync(
                    AuditEngine.AuditType.Error,
                    "Conflict check attempted by user without tenant configuration.",
                    securityUser?.accountName,
                    ex);

                return Problem("Your user account is not configured with a tenant.");
            }

            // Normalize all incoming dates to UTC to ensure consistent comparison
            foreach (EventResourceAssignmentDTO dto in proposedAssignments)
            {
                if (dto.assignmentStartDateTime.HasValue && dto.assignmentStartDateTime.Value.Kind != DateTimeKind.Utc)
                {
                    dto.assignmentStartDateTime = dto.assignmentStartDateTime.Value.ToUniversalTime();
                }

                if (dto.assignmentEndDateTime.HasValue && dto.assignmentEndDateTime.Value.Kind != DateTimeKind.Utc)
                {
                    dto.assignmentEndDateTime = dto.assignmentEndDateTime.Value.ToUniversalTime();
                }
            }

            // Collection to hold all detected conflicts
            var conflicts = new List<object>();

            // Helper tuple for internal processing: resource ID and time range for each proposed assignment
            var expandedAssignments = new List<(int ResourceId, DateTime Start, DateTime End, int ProposedIndex)>();

            // Step 1: Expand crew assignments into individual resource checks
            for (int i = 0; i < proposedAssignments.Length; i++)
            {
                EventResourceAssignmentDTO assignment = proposedAssignments[i];

                // Default to full day if no specific time range provided
                DateTime start = assignment.assignmentStartDateTime ?? DateTime.UtcNow.Date;
                DateTime end = assignment.assignmentEndDateTime ?? start.AddDays(1);

                if (assignment.crewId.HasValue)
                {
                    // Expand crew into its active members
                    var crewMemberResourceIds = await _context.CrewMembers
                        .Where(cm =>
                            cm.crewId == assignment.crewId.Value &&
                            cm.tenantGuid == userTenantGuid &&
                            cm.active && !cm.deleted)
                        .Select(cm => cm.resourceId)
                        .ToListAsync(cancellationToken);

                    foreach (int resourceId in crewMemberResourceIds)
                    {
                        expandedAssignments.Add((resourceId, start, end, i));
                    }
                }
                else if (assignment.resourceId.HasValue)
                {
                    // Direct individual resource assignment
                    expandedAssignments.Add((assignment.resourceId.Value, start, end, i));
                }
                // If neither crew nor resource specified — invalid, but let save handle it
            }

            // Step 2: Check for overlaps with existing active assignments
            foreach (var (resourceId, proposedStart, proposedEnd, proposedIndex) in expandedAssignments)
            {
                DateTime effectiveProposedEnd = proposedEnd;

                var overlappingAssignments = await _context.EventResourceAssignments
                    .Where(era =>
                        era.tenantGuid == userTenantGuid &&
                        era.resourceId == resourceId &&
                        era.active && !era.deleted &&
                        // Standard overlap condition
                        era.assignmentStartDateTime < effectiveProposedEnd &&
                        (era.assignmentEndDateTime ?? era.assignmentStartDateTime.Value.AddDays(1)) > proposedStart)
                    .Include(era => era.scheduledEvent)
                    .Select(era => new
                    {
                        era.id,
                        ScheduledEventName = era.scheduledEvent!.name
                    })
                    .ToListAsync(cancellationToken);

                foreach (var overlap in overlappingAssignments)
                {
                    conflicts.Add(new AssignmentOverlapConflictDTO
                    {
                        ResourceId = resourceId,
                        ConflictingAssignmentId = overlap.id,
                        ConflictingEventName = overlap.ScheduledEventName,
                        ProposedIndex = proposedIndex,
                        Message = $"Resource is already assigned to event '{overlap.ScheduledEventName}' during overlapping time."
                    });
                }
            }

            // Step 3: Check against active resource blackouts
            foreach (var (resourceId, proposedStart, proposedEnd, proposedIndex) in expandedAssignments)
            {
                DateTime effectiveProposedEnd = proposedEnd;

                var blackoutConflicts = await _context.ResourceAvailabilities
                    .Where(ra =>
                        ra.tenantGuid == userTenantGuid &&
                        ra.resourceId == resourceId &&
                        ra.active && !ra.deleted &&
                        ra.startDateTime < effectiveProposedEnd &&
                        (ra.endDateTime ?? DateTime.MaxValue) > proposedStart)
                    .Select(ra => new
                    {
                        ra.startDateTime,
                        ra.endDateTime,
                        ra.reason
                    })
                    .ToListAsync(cancellationToken);

                foreach (var blackout in blackoutConflicts)
                {
                    conflicts.Add(new BlackoutConflictDTO
                    {
                        ResourceId = resourceId,
                        BlackoutStart = blackout.startDateTime,
                        BlackoutEnd = blackout.endDateTime,
                        Reason = blackout.reason ?? "No reason given",
                        ProposedIndex = proposedIndex,
                        Message = $"Resource is unavailable due to blackout: {blackout.reason ?? "Scheduled downtime"}."
                    });
                }
            }

            // Audit the conflict check operation
            await CreateAuditEventAsync(
                AuditEngine.AuditType.ReadList,
                $"Conflict check performed on {proposedAssignments.Length} proposed assignment(s). Found {conflicts.Count} conflict(s).",
                securityUser?.accountName);

            // Return strongly-typed conflict list (empty = no conflicts)
            return Ok(conflicts);
        }
    }
}
