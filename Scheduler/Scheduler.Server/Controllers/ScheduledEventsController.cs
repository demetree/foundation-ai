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

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the ScheduledEvent entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ScheduledEvent entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ScheduledEventsController : SecureWebAPIController
	{


		/// <summary>
		/// 
		/// This gets a list of ScheduledEvents filtered by the parameters provided.
		/// 
		/// There is a filter parameter for every field, and an 'anyStringContains' parameter for cross field string partial searches.
		/// 
		/// Note also the pagination control in the pageSize and pageNumber parameters.
		/// 
		/// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEvents")]
		public async Task<IActionResult> GetScheduledEvents(
			string name = null,
			string description = null,
			int? recurrenceRuleId = null,
			int? schedulingTargetId = null,
			DateTime? startDateTime = null,
			DateTime? endDateTime = null,
			string location = null,
			int? resourceId = null,
			string notes = null,
			string externalId = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			int? pageSize = null,
			int? pageNumber = null,
			string anyStringContains = null,
			bool includeRelations = true,
			CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
				return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);

			Guid userTenantGuid;

			try
			{
				userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
				return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}

			if (pageNumber.HasValue == true &&
				pageNumber < 1)
			{
				pageNumber = null;
			}

			if (pageSize.HasValue == true &&
				pageSize <= 0)
			{
				pageSize = null;
			}

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (startDateTime.HasValue == true && startDateTime.Value.Kind != DateTimeKind.Utc)
			{
				startDateTime = startDateTime.Value.ToUniversalTime();
			}

			if (endDateTime.HasValue == true && endDateTime.Value.Kind != DateTimeKind.Utc)
			{
				endDateTime = endDateTime.Value.ToUniversalTime();
			}

			IQueryable<ScheduledEvent> query = (from se in _context.ScheduledEvents select se);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(se => se.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(se => se.description == description);
			}
			if (recurrenceRuleId.HasValue == true)
			{
				query = query.Where(se => se.recurrenceRuleId == recurrenceRuleId.Value);
			}
			if (schedulingTargetId.HasValue == true)
			{
				query = query.Where(se => se.schedulingTargetId == schedulingTargetId.Value);
			}
			if (startDateTime.HasValue == true)
			{
				query = query.Where(se => se.startDateTime >= startDateTime.Value);         // Custom change here to make this >= instead of ==
			}
			if (endDateTime.HasValue == true)
			{
				query = query.Where(se => se.endDateTime <= endDateTime.Value);             // Custom change here to make this <= instead of ==
			}
			if (string.IsNullOrEmpty(location) == false)
			{
				query = query.Where(se => se.location == location);
			}
			if (resourceId.HasValue == true)
			{
				query = query.Where(se => se.resourceId == resourceId.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(se => se.notes == notes);
			}
			if (string.IsNullOrEmpty(externalId) == false)
			{
				query = query.Where(se => se.externalId == externalId);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(se => se.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(se => se.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(se => se.active == active.Value);
				}

				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(se => se.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(se => se.deleted == false);
				}
			}
			else
			{
				query = query.Where(se => se.active == true);
				query = query.Where(se => se.deleted == false);
			}

			query = query.OrderBy(se => se.name);

			if (pageNumber.HasValue == true &&
				pageSize.HasValue == true)
			{
				query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}

			if (includeRelations == true)
			{
				query = query.Include("recurrenceRule");
				query = query.Include("resource");
				query = query.Include("schedulingTarget");
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Scheduled Event, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
				query = query.Where(x =>
					x.name.Contains(anyStringContains)
					|| x.description.Contains(anyStringContains)
					|| x.location.Contains(anyStringContains)
					|| x.notes.Contains(anyStringContains)
					|| x.externalId.Contains(anyStringContains)
					|| (includeRelations == true && x.resource.name.Contains(anyStringContains))
					|| (includeRelations == true && x.resource.description.Contains(anyStringContains))
					|| (includeRelations == true && x.resource.externalId.Contains(anyStringContains))
					|| (includeRelations == true && x.schedulingTarget.name.Contains(anyStringContains))
					|| (includeRelations == true && x.schedulingTarget.description.Contains(anyStringContains))
					|| (includeRelations == true && x.schedulingTarget.externalId.Contains(anyStringContains))
				);
			}

			query = query.AsNoTracking();

			List<Database.ScheduledEvent> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ScheduledEvent scheduledEvent in materialized)
			{
				Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(scheduledEvent, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ScheduledEvent Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ScheduledEvent Entity list was read.  Returning " + materialized.Count + " rows of data.");

			// Create a new output object that only includes the relations if necessary, and doesn't include the empty list objects, so that we can reduce the amount of data being transferred.
			if (includeRelations == true)
			{
				//             //List<Object> reducedFieldOutput = (from materializedData in materialized
				//             //		 select Database.ScheduledEvent.CreateAnonymousWithFirstLevelSubObjects(materializedData)).ToList();

				//             List<ScheduledEvent.ScheduledEventOutputDTO> reducedFieldOutput = (from materializedData in materialized select materializedData.ToOutputDTO()).ToList();

				//             return Ok(reducedFieldOutput);
				//}
				//else
				//{
				//	List<ScheduledEvent.ScheduledEventDTO> reducedFieldOutput = (from materializedData in materialized select materializedData.ToDTO()).ToList();

				//	return Ok(reducedFieldOutput);

				// Return a DTO with nav properties.
				return Ok((from data in materialized select data.ToOutputDTO()).ToList());
			}
			else
			{
				// Return a DTO without nav properties.
				return Ok((from data in materialized select data.ToDTO()).ToList());

			}

		}
        /// <summary>
        /// 
        /// This returns a row count of ScheduledEvents filtered by the parameters provided.  Its query is similar to the GetScheduledEvents method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEvents/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			int? recurrenceRuleId = null,
			int? schedulingTargetId = null,
			DateTime? startDateTime = null,
			DateTime? endDateTime = null,
			string location = null,
			int? resourceId = null,
			string notes = null,
			string externalId = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			//
			// Fix any non-UTC date parameters that come in.
			//
			if (startDateTime.HasValue == true && startDateTime.Value.Kind != DateTimeKind.Utc)
			{
				startDateTime = startDateTime.Value.ToUniversalTime();
			}

			if (endDateTime.HasValue == true && endDateTime.Value.Kind != DateTimeKind.Utc)
			{
				endDateTime = endDateTime.Value.ToUniversalTime();
			}

			IQueryable<ScheduledEvent> query = (from se in _context.ScheduledEvents select se);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(se => se.name == name);
			}
			if (description != null)
			{
				query = query.Where(se => se.description == description);
			}
			if (recurrenceRuleId.HasValue == true)
			{
				query = query.Where(se => se.recurrenceRuleId == recurrenceRuleId.Value);
			}
			if (schedulingTargetId.HasValue == true)
			{
				query = query.Where(se => se.schedulingTargetId == schedulingTargetId.Value);
			}
			if (startDateTime.HasValue == true)
			{
				query = query.Where(se => se.startDateTime >= startDateTime.Value);             // Custom change here to make this >= instead of ==
            }
			if (endDateTime.HasValue == true)
			{
				query = query.Where(se => se.endDateTime <= endDateTime.Value);                 // Custom change here to make this <= instead of ==
            }
			if (location != null)
			{
				query = query.Where(se => se.location == location);
			}
			if (resourceId.HasValue == true)
			{
				query = query.Where(se => se.resourceId == resourceId.Value);
			}
			if (notes != null)
			{
				query = query.Where(se => se.notes == notes);
			}
			if (externalId != null)
			{
				query = query.Where(se => se.externalId == externalId);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(se => se.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(se => se.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(se => se.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(se => se.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(se => se.deleted == false);
				}
			}
			else
			{
				query = query.Where(se => se.active == true);
				query = query.Where(se => se.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Scheduled Event, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.location.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.resource.name.Contains(anyStringContains)
			       || x.resource.description.Contains(anyStringContains)
			       || x.resource.externalId.Contains(anyStringContains)
			       || x.schedulingTarget.name.Contains(anyStringContains)
			       || x.schedulingTarget.description.Contains(anyStringContains)
			       || x.schedulingTarget.externalId.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}



       





        /// <summary>
        /// 
        /// This gets a list of ScheduledEvent records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ScheduledEvents/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			int? recurrenceRuleId = null,
			int? schedulingTargetId = null,
			DateTime? startDateTime = null,
			DateTime? endDateTime = null,
			string location = null,
			int? resourceId = null,
			string notes = null,
			string externalId = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);

			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);

			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			if (pageNumber.HasValue == true &&
			    pageNumber < 1)
			{
			    pageNumber = null;
			}

			if (pageSize.HasValue == true &&
			    pageSize <= 0)
			{
			    pageSize = null;
			}

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (startDateTime.HasValue == true && startDateTime.Value.Kind != DateTimeKind.Utc)
			{
				startDateTime = startDateTime.Value.ToUniversalTime();
			}

			if (endDateTime.HasValue == true && endDateTime.Value.Kind != DateTimeKind.Utc)
			{
				endDateTime = endDateTime.Value.ToUniversalTime();
			}

			IQueryable<ScheduledEvent> query = (from se in _context.ScheduledEvents select se);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(se => se.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(se => se.description == description);
			}
			if (recurrenceRuleId.HasValue == true)
			{
				query = query.Where(se => se.recurrenceRuleId == recurrenceRuleId.Value);
			}
			if (schedulingTargetId.HasValue == true)
			{
				query = query.Where(se => se.schedulingTargetId == schedulingTargetId.Value);
			}
			if (startDateTime.HasValue == true)
			{
				query = query.Where(se => se.startDateTime >= startDateTime.Value);         // Custom change here to make this >= instead of ==
            }
			if (endDateTime.HasValue == true)
			{
				query = query.Where(se => se.endDateTime <= endDateTime.Value);				// Custom change here to make this <= instead of ==
            }
			if (string.IsNullOrEmpty(location) == false)
			{
				query = query.Where(se => se.location == location);
			}
			if (resourceId.HasValue == true)
			{
				query = query.Where(se => se.resourceId == resourceId.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(se => se.notes == notes);
			}
			if (string.IsNullOrEmpty(externalId) == false)
			{
				query = query.Where(se => se.externalId == externalId);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(se => se.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(se => se.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(se => se.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(se => se.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(se => se.deleted == false);
				}
			}
			else
			{
				query = query.Where(se => se.active == true);
				query = query.Where(se => se.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Scheduled Event, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.location.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.resource.name.Contains(anyStringContains)
			       || x.resource.description.Contains(anyStringContains)
			       || x.resource.externalId.Contains(anyStringContains)
			       || x.schedulingTarget.name.Contains(anyStringContains)
			       || x.schedulingTarget.description.Contains(anyStringContains)
			       || x.schedulingTarget.externalId.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ScheduledEvent.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
		}


        /// <summary>
        /// Checks for booking conflicts (overlapping events) on the same scheduling target (calendar).
        ///
        /// Two events overlap when: newStart &lt; existingEnd AND newEnd &gt; existingStart
        ///
        /// Returns the conflicting events so the user can decide whether to proceed.
        /// This is a non-blocking check — the client shows a warning, not an error.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/ScheduledEvents/CheckConflicts")]
        public async Task<IActionResult> CheckConflicts(
            int schedulingTargetId,
            DateTime startDateTime,
            DateTime endDateTime,
            int? excludeEventId = null,
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
                await CreateAuditEventAsync(AuditType.Error,
                    "CheckConflicts requested by user with no tenant. User: " + securityUser?.accountName,
                    securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant.");
            }

            //
            // Fix any non-UTC date parameters
            //
            if (startDateTime.Kind != DateTimeKind.Utc)
                startDateTime = startDateTime.ToUniversalTime();
            if (endDateTime.Kind != DateTimeKind.Utc)
                endDateTime = endDateTime.ToUniversalTime();

            //
            // Two intervals overlap when: newStart < existingEnd AND newEnd > existingStart
            //
            var query = _context.ScheduledEvents
                .Where(se => se.tenantGuid == userTenantGuid
                    && se.schedulingTargetId == schedulingTargetId
                    && se.active == true && se.deleted == false
                    && se.startDateTime < endDateTime
                    && se.endDateTime > startDateTime);

            if (excludeEventId.HasValue)
            {
                query = query.Where(se => se.id != excludeEventId.Value);
            }

            var conflicts = await query
                .OrderBy(se => se.startDateTime)
                .Select(se => new
                {
                    id = se.id,
                    name = se.name,
                    startDateTime = se.startDateTime,
                    endDateTime = se.endDateTime,
                    location = se.location
                })
                .Take(10) // Limit to prevent large payloads
                .ToListAsync(cancellationToken);

            return Ok(new
            {
                hasConflict = conflicts.Count > 0,
                conflictCount = conflicts.Count,
                conflicts
            });
        }
	}
}
