//
// Escalation Service Interface
//
// Defines escalation processing and on-call resolution.
//
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation.Alerting.Database;

namespace Alerting.Server.Services
{
    /// <summary>
    /// Service for processing escalation rules and resolving on-call targets.
    /// </summary>
    public interface IEscalationService
    {
        /// <summary>
        /// Processes all incidents that need escalation (nextEscalationAt <= now).
        /// </summary>
        /// <returns>Number of incidents processed.</returns>
        Task<int> ProcessPendingEscalationsAsync();

        /// <summary>
        /// Gets the current on-call user(s) for a schedule at a given time.
        /// </summary>
        /// <param name="scheduleId">The on-call schedule ID.</param>
        /// <param name="atTime">The time to check (defaults to now).</param>
        /// <returns>List of user objectGuids currently on call.</returns>
        Task<List<Guid>> GetCurrentOnCallUsersAsync(int scheduleId, DateTime? atTime = null);

        /// <summary>
        /// Resolves the target users for an escalation rule.
        /// Handles User, Team, and Schedule target types.
        /// </summary>
        /// <param name="rule">The escalation rule.</param>
        /// <returns>List of user objectGuids to notify.</returns>
        Task<List<Guid>> ResolveEscalationTargetsAsync(EscalationRule rule);
    }
}
