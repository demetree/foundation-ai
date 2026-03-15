//
// ResourceScheduleContextController.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// This partial class extends ResourcesController with scheduling context endpoints:
//   - LogSchedulingWarningDismissal: Audit logging when users dismiss availability/shift warnings
//   - GetResourceScheduleContext:   (Future) Provides availability and shift data for calendar display
//
// This keeps the auto-generated CRUD controller untouched while adding scheduling-specific behavior.
//
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Foundation.Security;
using Foundation.Auditor;
using Foundation.Controllers;
using Foundation.Security.Database;
using static Foundation.Auditor.AuditEngine;
using Foundation.Scheduler.Database;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    ///
    /// Partial class extending ResourcesController with scheduling context endpoints.
    ///
    /// LogSchedulingWarningDismissal: Accepts scheduling warning data from the client when
    /// a user dismisses availability or shift warnings during event creation/editing.
    /// Logs each warning via the audit engine for compliance and traceability.
    ///
    /// </summary>
    public partial class ResourcesController
    {
        /// <summary>
        ///
        /// Logs dismissed scheduling warnings for audit purposes.
        ///
        /// Called by the client when a user acknowledges and proceeds past
        /// resource availability conflicts or shift boundary warnings while saving an event.
        ///
        /// Each warning is logged as an AuditType.Miscellaneous audit event.
        ///
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/Resources/LogSchedulingWarningDismissal")]
        public async Task<IActionResult> LogSchedulingWarningDismissal(
            [FromBody] SchedulingWarningDismissalRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null || request.Warnings == null || request.Warnings.Count == 0)
            {
                return Ok(); // Nothing to log
            }

            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

            try
            {
                string resourceIdList = request.ResourceIds != null && request.ResourceIds.Count > 0
                    ? string.Join(", ", request.ResourceIds)
                    : "none";

                string warningDetails = string.Join(" | ", request.Warnings);

                string auditMessage = $"User dismissed scheduling warnings while saving event " +
                    $"'{request.EventName ?? "Unknown"}' (ID: {request.EventId}). " +
                    $"Resources: [{resourceIdList}]. " +
                    $"Warnings: {warningDetails}";

                await CreateAuditEventAsync(
                    AuditEngine.AuditType.Miscellaneous,
                    auditMessage,
                    securityUser?.accountName
                );

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log scheduling warning dismissal");
                return Ok(); // Don't fail the client request even if audit logging fails
            }
        }
    }

    /// <summary>
    /// Request model for the LogSchedulingWarningDismissal endpoint.
    /// </summary>
    public class SchedulingWarningDismissalRequest
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public List<string> Warnings { get; set; }
        public List<int> ResourceIds { get; set; }
    }
}
