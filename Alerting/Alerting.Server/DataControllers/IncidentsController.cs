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
using Foundation.Alerting.Database;
using Foundation.ChangeHistory;

namespace Foundation.Alerting.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the Incident entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Incident entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class IncidentsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		static object incidentPutSyncRoot = new object();
		static object incidentDeleteSyncRoot = new object();

		private AlertingContext _context;

		private ILogger<IncidentsController> _logger;

		public IncidentsController(AlertingContext context, ILogger<IncidentsController> logger) : base("Alerting", "Incident")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Incidents filtered by the parameters provided.
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
		[Route("api/Incidents")]
		public async Task<IActionResult> GetIncidents(
			string incidentKey = null,
			int? serviceId = null,
			string title = null,
			string description = null,
			int? severityTypeId = null,
			int? incidentStatusTypeId = null,
			DateTime? createdAt = null,
			int? escalationRuleId = null,
			int? currentRepeatCount = null,
			DateTime? nextEscalationAt = null,
			DateTime? acknowledgedAt = null,
			DateTime? resolvedAt = null,
			Guid? currentAssigneeObjectGuid = null,
			string sourcePayloadJson = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
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
			if (createdAt.HasValue == true && createdAt.Value.Kind != DateTimeKind.Utc)
			{
				createdAt = createdAt.Value.ToUniversalTime();
			}

			if (nextEscalationAt.HasValue == true && nextEscalationAt.Value.Kind != DateTimeKind.Utc)
			{
				nextEscalationAt = nextEscalationAt.Value.ToUniversalTime();
			}

			if (acknowledgedAt.HasValue == true && acknowledgedAt.Value.Kind != DateTimeKind.Utc)
			{
				acknowledgedAt = acknowledgedAt.Value.ToUniversalTime();
			}

			if (resolvedAt.HasValue == true && resolvedAt.Value.Kind != DateTimeKind.Utc)
			{
				resolvedAt = resolvedAt.Value.ToUniversalTime();
			}

			IQueryable<Database.Incident> query = (from i in _context.Incidents select i);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(incidentKey) == false)
			{
				query = query.Where(i => i.incidentKey == incidentKey);
			}
			if (serviceId.HasValue == true)
			{
				query = query.Where(i => i.serviceId == serviceId.Value);
			}
			if (string.IsNullOrEmpty(title) == false)
			{
				query = query.Where(i => i.title == title);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(i => i.description == description);
			}
			if (severityTypeId.HasValue == true)
			{
				query = query.Where(i => i.severityTypeId == severityTypeId.Value);
			}
			if (incidentStatusTypeId.HasValue == true)
			{
				query = query.Where(i => i.incidentStatusTypeId == incidentStatusTypeId.Value);
			}
			if (createdAt.HasValue == true)
			{
				query = query.Where(i => i.createdAt == createdAt.Value);
			}
			if (escalationRuleId.HasValue == true)
			{
				query = query.Where(i => i.escalationRuleId == escalationRuleId.Value);
			}
			if (currentRepeatCount.HasValue == true)
			{
				query = query.Where(i => i.currentRepeatCount == currentRepeatCount.Value);
			}
			if (nextEscalationAt.HasValue == true)
			{
				query = query.Where(i => i.nextEscalationAt == nextEscalationAt.Value);
			}
			if (acknowledgedAt.HasValue == true)
			{
				query = query.Where(i => i.acknowledgedAt == acknowledgedAt.Value);
			}
			if (resolvedAt.HasValue == true)
			{
				query = query.Where(i => i.resolvedAt == resolvedAt.Value);
			}
			if (currentAssigneeObjectGuid.HasValue == true)
			{
				query = query.Where(i => i.currentAssigneeObjectGuid == currentAssigneeObjectGuid);
			}
			if (string.IsNullOrEmpty(sourcePayloadJson) == false)
			{
				query = query.Where(i => i.sourcePayloadJson == sourcePayloadJson);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(i => i.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(i => i.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(i => i.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(i => i.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(i => i.deleted == false);
				}
			}
			else
			{
				query = query.Where(i => i.active == true);
				query = query.Where(i => i.deleted == false);
			}

			query = query.OrderBy(i => i.incidentKey).ThenBy(i => i.title);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.escalationRule);
				query = query.Include(x => x.incidentStatusType);
				query = query.Include(x => x.service);
				query = query.Include(x => x.severityType);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Incident, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.incidentKey.Contains(anyStringContains)
			       || x.title.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.sourcePayloadJson.Contains(anyStringContains)
			       || (includeRelations == true && x.escalationRule.targetType.Contains(anyStringContains))
			       || (includeRelations == true && x.incidentStatusType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.incidentStatusType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.service.name.Contains(anyStringContains))
			       || (includeRelations == true && x.service.description.Contains(anyStringContains))
			       || (includeRelations == true && x.severityType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.severityType.description.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.Incident> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Incident incident in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(incident, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Alerting.Incident Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Alerting.Incident Entity list was read.  Returning " + materialized.Count + " rows of data.");

			// Create a new output object that only includes the relations if necessary, and doesn't include the empty list objects, so that we can reduce the amount of data being transferred.
			if (includeRelations == true)
			{
				// Return a DTO with nav properties.
				return Ok((from materializedData in materialized select materializedData.ToOutputDTO()).ToList());
			}
			else
			{
				// Return a DTO without nav properties.
				return Ok((from materializedData in materialized select materializedData.ToDTO()).ToList());
			}
		}
		
		
        /// <summary>
        /// 
        /// This returns a row count of Incidents filtered by the parameters provided.  Its query is similar to the GetIncidents method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Incidents/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string incidentKey = null,
			int? serviceId = null,
			string title = null,
			string description = null,
			int? severityTypeId = null,
			int? incidentStatusTypeId = null,
			DateTime? createdAt = null,
			int? escalationRuleId = null,
			int? currentRepeatCount = null,
			DateTime? nextEscalationAt = null,
			DateTime? acknowledgedAt = null,
			DateTime? resolvedAt = null,
			Guid? currentAssigneeObjectGuid = null,
			string sourcePayloadJson = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
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
			if (createdAt.HasValue == true && createdAt.Value.Kind != DateTimeKind.Utc)
			{
				createdAt = createdAt.Value.ToUniversalTime();
			}

			if (nextEscalationAt.HasValue == true && nextEscalationAt.Value.Kind != DateTimeKind.Utc)
			{
				nextEscalationAt = nextEscalationAt.Value.ToUniversalTime();
			}

			if (acknowledgedAt.HasValue == true && acknowledgedAt.Value.Kind != DateTimeKind.Utc)
			{
				acknowledgedAt = acknowledgedAt.Value.ToUniversalTime();
			}

			if (resolvedAt.HasValue == true && resolvedAt.Value.Kind != DateTimeKind.Utc)
			{
				resolvedAt = resolvedAt.Value.ToUniversalTime();
			}

			IQueryable<Database.Incident> query = (from i in _context.Incidents select i);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (incidentKey != null)
			{
				query = query.Where(i => i.incidentKey == incidentKey);
			}
			if (serviceId.HasValue == true)
			{
				query = query.Where(i => i.serviceId == serviceId.Value);
			}
			if (title != null)
			{
				query = query.Where(i => i.title == title);
			}
			if (description != null)
			{
				query = query.Where(i => i.description == description);
			}
			if (severityTypeId.HasValue == true)
			{
				query = query.Where(i => i.severityTypeId == severityTypeId.Value);
			}
			if (incidentStatusTypeId.HasValue == true)
			{
				query = query.Where(i => i.incidentStatusTypeId == incidentStatusTypeId.Value);
			}
			if (createdAt.HasValue == true)
			{
				query = query.Where(i => i.createdAt == createdAt.Value);
			}
			if (escalationRuleId.HasValue == true)
			{
				query = query.Where(i => i.escalationRuleId == escalationRuleId.Value);
			}
			if (currentRepeatCount.HasValue == true)
			{
				query = query.Where(i => i.currentRepeatCount == currentRepeatCount.Value);
			}
			if (nextEscalationAt.HasValue == true)
			{
				query = query.Where(i => i.nextEscalationAt == nextEscalationAt.Value);
			}
			if (acknowledgedAt.HasValue == true)
			{
				query = query.Where(i => i.acknowledgedAt == acknowledgedAt.Value);
			}
			if (resolvedAt.HasValue == true)
			{
				query = query.Where(i => i.resolvedAt == resolvedAt.Value);
			}
			if (currentAssigneeObjectGuid.HasValue == true)
			{
				query = query.Where(i => i.currentAssigneeObjectGuid == currentAssigneeObjectGuid);
			}
			if (sourcePayloadJson != null)
			{
				query = query.Where(i => i.sourcePayloadJson == sourcePayloadJson);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(i => i.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(i => i.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(i => i.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(i => i.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(i => i.deleted == false);
				}
			}
			else
			{
				query = query.Where(i => i.active == true);
				query = query.Where(i => i.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Incident, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.incidentKey.Contains(anyStringContains)
			       || x.title.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.sourcePayloadJson.Contains(anyStringContains)
			       || x.escalationRule.targetType.Contains(anyStringContains)
			       || x.incidentStatusType.name.Contains(anyStringContains)
			       || x.incidentStatusType.description.Contains(anyStringContains)
			       || x.service.name.Contains(anyStringContains)
			       || x.service.description.Contains(anyStringContains)
			       || x.severityType.name.Contains(anyStringContains)
			       || x.severityType.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single Incident by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Incident/{id}")]
		public async Task<IActionResult> GetIncident(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
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


			try
			{
				IQueryable<Database.Incident> query = (from i in _context.Incidents where
							(i.id == id) &&
							(userIsAdmin == true || i.deleted == false) &&
							(userIsWriter == true || i.active == true)
					select i);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.escalationRule);
					query = query.Include(x => x.incidentStatusType);
					query = query.Include(x => x.service);
					query = query.Include(x => x.severityType);
					query = query.AsSplitQuery();
				}

				Database.Incident materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Alerting.Incident Entity was read with Admin privilege." : "Alerting.Incident Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Incident", materialized.id, materialized.incidentKey));


					// Create a new output object that only includes the relations if necessary, and doesn't include the empty list objects, so that we can reduce the amount of data being transferred.
					if (includeRelations == true)
					{
						return Ok(materialized.ToOutputDTO());             // DTO with nav properties
					}
					else
					{
						return Ok(materialized.ToDTO());                   // DTO without nav properties
					}
				}
				else
				{
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Alerting.Incident entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Alerting.Incident.   Entity was read with Admin privilege." : "Exception caught during entity read of Alerting.Incident.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing Incident record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Incident/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutIncident(int id, [FromBody]Database.Incident.IncidentDTO incidentDTO, CancellationToken cancellationToken = default)
		{
			if (incidentDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != incidentDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
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


			IQueryable<Database.Incident> query = (from x in _context.Incidents
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Incident existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.Incident PUT", id.ToString(), new Exception("No Alerting.Incident entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (incidentDTO.objectGuid == Guid.Empty)
            {
                incidentDTO.objectGuid = existing.objectGuid;
            }
            else if (incidentDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Incident record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Incident cloneOfExisting = (Database.Incident)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Incident object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Incident incident = (Database.Incident)_context.Entry(existing).GetDatabaseValues().ToObject();
			incident.ApplyDTO(incidentDTO);
			//
			// The tenant guid for any Incident being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the Incident because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				incident.tenantGuid = existing.tenantGuid;
			}

			lock (incidentPutSyncRoot)
			{
				//
				// Validate the version number for the incident being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != incident.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Incident save attempt was made but save request was with version " + incident.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The Incident you are trying to update has already changed.  Please try your save again after reloading the Incident.");
				}
				else
				{
					// Same record.  Increase version.
					incident.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (incident.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Alerting.Incident record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (incident.incidentKey != null && incident.incidentKey.Length > 250)
				{
					incident.incidentKey = incident.incidentKey.Substring(0, 250);
				}

				if (incident.title != null && incident.title.Length > 250)
				{
					incident.title = incident.title.Substring(0, 250);
				}

				if (incident.createdAt.Kind != DateTimeKind.Utc)
				{
					incident.createdAt = incident.createdAt.ToUniversalTime();
				}

				if (incident.nextEscalationAt.HasValue == true && incident.nextEscalationAt.Value.Kind != DateTimeKind.Utc)
				{
					incident.nextEscalationAt = incident.nextEscalationAt.Value.ToUniversalTime();
				}

				if (incident.acknowledgedAt.HasValue == true && incident.acknowledgedAt.Value.Kind != DateTimeKind.Utc)
				{
					incident.acknowledgedAt = incident.acknowledgedAt.Value.ToUniversalTime();
				}

				if (incident.resolvedAt.HasValue == true && incident.resolvedAt.Value.Kind != DateTimeKind.Utc)
				{
					incident.resolvedAt = incident.resolvedAt.Value.ToUniversalTime();
				}

				try
				{
				    EntityEntry<Database.Incident> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(incident);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        IncidentChangeHistory incidentChangeHistory = new IncidentChangeHistory();
				        incidentChangeHistory.incidentId = incident.id;
				        incidentChangeHistory.versionNumber = incident.versionNumber;
				        incidentChangeHistory.timeStamp = DateTime.UtcNow;
				        incidentChangeHistory.userId = securityUser.id;
				        incidentChangeHistory.tenantGuid = userTenantGuid;
				        incidentChangeHistory.data = JsonSerializer.Serialize(Database.Incident.CreateAnonymousWithFirstLevelSubObjects(incident));
				        _context.IncidentChangeHistories.Add(incidentChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.Incident entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Incident.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Incident.CreateAnonymousWithFirstLevelSubObjects(incident)),
						null);

				return Ok(Database.Incident.CreateAnonymous(incident));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.Incident entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Incident.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Incident.CreateAnonymousWithFirstLevelSubObjects(incident)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new Incident record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Incident", Name = "Incident")]
		public async Task<IActionResult> PostIncident([FromBody]Database.Incident.IncidentDTO incidentDTO, CancellationToken cancellationToken = default)
		{
			if (incidentDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

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
			// Create a new Incident object using the data from the DTO
			//
			Database.Incident incident = Database.Incident.FromDTO(incidentDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				incident.tenantGuid = userTenantGuid;

				if (incident.incidentKey != null && incident.incidentKey.Length > 250)
				{
					incident.incidentKey = incident.incidentKey.Substring(0, 250);
				}

				if (incident.title != null && incident.title.Length > 250)
				{
					incident.title = incident.title.Substring(0, 250);
				}

				if (incident.createdAt.Kind != DateTimeKind.Utc)
				{
					incident.createdAt = incident.createdAt.ToUniversalTime();
				}

				if (incident.nextEscalationAt.HasValue == true && incident.nextEscalationAt.Value.Kind != DateTimeKind.Utc)
				{
					incident.nextEscalationAt = incident.nextEscalationAt.Value.ToUniversalTime();
				}

				if (incident.acknowledgedAt.HasValue == true && incident.acknowledgedAt.Value.Kind != DateTimeKind.Utc)
				{
					incident.acknowledgedAt = incident.acknowledgedAt.Value.ToUniversalTime();
				}

				if (incident.resolvedAt.HasValue == true && incident.resolvedAt.Value.Kind != DateTimeKind.Utc)
				{
					incident.resolvedAt = incident.resolvedAt.Value.ToUniversalTime();
				}

				incident.objectGuid = Guid.NewGuid();
				incident.versionNumber = 1;

				_context.Incidents.Add(incident);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the incident object so that no further changes will be written to the database
				    //
				    _context.Entry(incident).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					incident.IncidentChangeHistories = null;
					incident.IncidentNotes = null;
					incident.IncidentNotifications = null;
					incident.IncidentTimelineEvents = null;
					incident.WebhookDeliveryAttempts = null;
					incident.escalationRule = null;
					incident.incidentStatusType = null;
					incident.service = null;
					incident.severityType = null;


				    IncidentChangeHistory incidentChangeHistory = new IncidentChangeHistory();
				    incidentChangeHistory.incidentId = incident.id;
				    incidentChangeHistory.versionNumber = incident.versionNumber;
				    incidentChangeHistory.timeStamp = DateTime.UtcNow;
				    incidentChangeHistory.userId = securityUser.id;
				    incidentChangeHistory.tenantGuid = userTenantGuid;
				    incidentChangeHistory.data = JsonSerializer.Serialize(Database.Incident.CreateAnonymousWithFirstLevelSubObjects(incident));
				    _context.IncidentChangeHistories.Add(incidentChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Alerting.Incident entity successfully created.",
						true,
						incident. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.Incident.CreateAnonymousWithFirstLevelSubObjects(incident)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Alerting.Incident entity creation failed.", false, incident.id.ToString(), "", JsonSerializer.Serialize(Database.Incident.CreateAnonymousWithFirstLevelSubObjects(incident)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Incident", incident.id, incident.incidentKey));

			return CreatedAtRoute("Incident", new { id = incident.id }, Database.Incident.CreateAnonymousWithFirstLevelSubObjects(incident));
		}



        /// <summary>
        /// 
        /// This rolls a Incident entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Incident/Rollback/{id}")]
		[Route("api/Incident/Rollback")]
		public async Task<IActionResult> RollbackToIncidentVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
		{
			//
			// Data rollback is an admin only function, like Deletes.
			//
			StartAuditEventClock();
			
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			
			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
			
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

			

			
			IQueryable <Database.Incident> query = (from x in _context.Incidents
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this Incident concurrently
			//
			lock (incidentPutSyncRoot)
			{
				
				Database.Incident incident = query.FirstOrDefault();
				
				if (incident == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.Incident rollback", id.ToString(), new Exception("No Alerting.Incident entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the Incident current state so we can log it.
				//
				Database.Incident cloneOfExisting = (Database.Incident)_context.Entry(incident).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.IncidentChangeHistories = null;
				cloneOfExisting.IncidentNotes = null;
				cloneOfExisting.IncidentNotifications = null;
				cloneOfExisting.IncidentTimelineEvents = null;
				cloneOfExisting.WebhookDeliveryAttempts = null;
				cloneOfExisting.escalationRule = null;
				cloneOfExisting.incidentStatusType = null;
				cloneOfExisting.service = null;
				cloneOfExisting.severityType = null;

				if (versionNumber >= incident.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Alerting.Incident rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Alerting.Incident rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				IncidentChangeHistory incidentChangeHistory = (from x in _context.IncidentChangeHistories
				                                               where
				                                               x.incidentId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (incidentChangeHistory != null)
				{
				    Database.Incident oldIncident = JsonSerializer.Deserialize<Database.Incident>(incidentChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    incident.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    incident.incidentKey = oldIncident.incidentKey;
				    incident.serviceId = oldIncident.serviceId;
				    incident.title = oldIncident.title;
				    incident.description = oldIncident.description;
				    incident.severityTypeId = oldIncident.severityTypeId;
				    incident.incidentStatusTypeId = oldIncident.incidentStatusTypeId;
				    incident.createdAt = oldIncident.createdAt;
				    incident.escalationRuleId = oldIncident.escalationRuleId;
				    incident.currentRepeatCount = oldIncident.currentRepeatCount;
				    incident.nextEscalationAt = oldIncident.nextEscalationAt;
				    incident.acknowledgedAt = oldIncident.acknowledgedAt;
				    incident.resolvedAt = oldIncident.resolvedAt;
				    incident.currentAssigneeObjectGuid = oldIncident.currentAssigneeObjectGuid;
				    incident.sourcePayloadJson = oldIncident.sourcePayloadJson;
				    incident.objectGuid = oldIncident.objectGuid;
				    incident.active = oldIncident.active;
				    incident.deleted = oldIncident.deleted;

				    string serializedIncident = JsonSerializer.Serialize(incident);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        IncidentChangeHistory newIncidentChangeHistory = new IncidentChangeHistory();
				        newIncidentChangeHistory.incidentId = incident.id;
				        newIncidentChangeHistory.versionNumber = incident.versionNumber;
				        newIncidentChangeHistory.timeStamp = DateTime.UtcNow;
				        newIncidentChangeHistory.userId = securityUser.id;
				        newIncidentChangeHistory.tenantGuid = userTenantGuid;
				        newIncidentChangeHistory.data = JsonSerializer.Serialize(Database.Incident.CreateAnonymousWithFirstLevelSubObjects(incident));
				        _context.IncidentChangeHistories.Add(newIncidentChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.Incident rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Incident.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Incident.CreateAnonymousWithFirstLevelSubObjects(incident)),
						null);


				    return Ok(Database.Incident.CreateAnonymous(incident));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Alerting.Incident rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Alerting.Incident rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a Incident.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Incident</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Incident/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetIncidentChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			Database.Incident incident = await _context.Incidents.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (incident == null)
			{
				return NotFound();
			}

			try
			{
				incident.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Incident> versionInfo = await incident.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

				if (versionInfo == null)
				{
					return NotFound($"Version {versionNumber} not found.");
				}

				return Ok(versionInfo);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets the full audit history for a Incident.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Incident</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Incident/{id}/AuditHistory")]
		public async Task<IActionResult> GetIncidentAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			Database.Incident incident = await _context.Incidents.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (incident == null)
			{
				return NotFound();
			}

			try
			{
				incident.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.Incident>> versions = await incident.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a Incident.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Incident</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The Incident object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Incident/{id}/Version/{version}")]
		public async Task<IActionResult> GetIncidentVersion(int id, int version, CancellationToken cancellationToken = default)
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
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			Database.Incident incident = await _context.Incidents.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (incident == null)
			{
				return NotFound();
			}

			try
			{
				incident.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Incident> versionInfo = await incident.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

				if (versionInfo == null || versionInfo.data == null)
				{
					return NotFound();
				}

				return Ok(versionInfo.data.ToOutputDTO());
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets the state of a Incident at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Incident</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The Incident object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Incident/{id}/StateAtTime")]
		public async Task<IActionResult> GetIncidentStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			Database.Incident incident = await _context.Incidents.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (incident == null)
			{
				return NotFound();
			}

			try
			{
				incident.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Incident> versionInfo = await incident.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

				if (versionInfo == null || versionInfo.data == null)
				{
					return NotFound("No state found at specified time.");
				}

				return Ok(versionInfo.data.ToOutputDTO());
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}

        /// <summary>
        /// 
        /// This deletes a Incident record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Incident/{id}")]
		[Route("api/Incident")]
		public async Task<IActionResult> DeleteIncident(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(cancellationToken);
			
			
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

			IQueryable<Database.Incident> query = (from x in _context.Incidents
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Incident incident = await query.FirstOrDefaultAsync(cancellationToken);

			if (incident == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.Incident DELETE", id.ToString(), new Exception("No Alerting.Incident entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Incident cloneOfExisting = (Database.Incident)_context.Entry(incident).GetDatabaseValues().ToObject();


			lock (incidentDeleteSyncRoot)
			{
			    try
			    {
			        incident.deleted = true;
			        incident.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        IncidentChangeHistory incidentChangeHistory = new IncidentChangeHistory();
			        incidentChangeHistory.incidentId = incident.id;
			        incidentChangeHistory.versionNumber = incident.versionNumber;
			        incidentChangeHistory.timeStamp = DateTime.UtcNow;
			        incidentChangeHistory.userId = securityUser.id;
			        incidentChangeHistory.tenantGuid = userTenantGuid;
			        incidentChangeHistory.data = JsonSerializer.Serialize(Database.Incident.CreateAnonymousWithFirstLevelSubObjects(incident));
			        _context.IncidentChangeHistories.Add(incidentChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Alerting.Incident entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Incident.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Incident.CreateAnonymousWithFirstLevelSubObjects(incident)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Alerting.Incident entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Incident.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Incident.CreateAnonymousWithFirstLevelSubObjects(incident)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of Incident records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Incidents/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string incidentKey = null,
			int? serviceId = null,
			string title = null,
			string description = null,
			int? severityTypeId = null,
			int? incidentStatusTypeId = null,
			DateTime? createdAt = null,
			int? escalationRuleId = null,
			int? currentRepeatCount = null,
			DateTime? nextEscalationAt = null,
			DateTime? acknowledgedAt = null,
			DateTime? resolvedAt = null,
			Guid? currentAssigneeObjectGuid = null,
			string sourcePayloadJson = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);

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
			if (createdAt.HasValue == true && createdAt.Value.Kind != DateTimeKind.Utc)
			{
				createdAt = createdAt.Value.ToUniversalTime();
			}

			if (nextEscalationAt.HasValue == true && nextEscalationAt.Value.Kind != DateTimeKind.Utc)
			{
				nextEscalationAt = nextEscalationAt.Value.ToUniversalTime();
			}

			if (acknowledgedAt.HasValue == true && acknowledgedAt.Value.Kind != DateTimeKind.Utc)
			{
				acknowledgedAt = acknowledgedAt.Value.ToUniversalTime();
			}

			if (resolvedAt.HasValue == true && resolvedAt.Value.Kind != DateTimeKind.Utc)
			{
				resolvedAt = resolvedAt.Value.ToUniversalTime();
			}

			IQueryable<Database.Incident> query = (from i in _context.Incidents select i);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(incidentKey) == false)
			{
				query = query.Where(i => i.incidentKey == incidentKey);
			}
			if (serviceId.HasValue == true)
			{
				query = query.Where(i => i.serviceId == serviceId.Value);
			}
			if (string.IsNullOrEmpty(title) == false)
			{
				query = query.Where(i => i.title == title);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(i => i.description == description);
			}
			if (severityTypeId.HasValue == true)
			{
				query = query.Where(i => i.severityTypeId == severityTypeId.Value);
			}
			if (incidentStatusTypeId.HasValue == true)
			{
				query = query.Where(i => i.incidentStatusTypeId == incidentStatusTypeId.Value);
			}
			if (createdAt.HasValue == true)
			{
				query = query.Where(i => i.createdAt == createdAt.Value);
			}
			if (escalationRuleId.HasValue == true)
			{
				query = query.Where(i => i.escalationRuleId == escalationRuleId.Value);
			}
			if (currentRepeatCount.HasValue == true)
			{
				query = query.Where(i => i.currentRepeatCount == currentRepeatCount.Value);
			}
			if (nextEscalationAt.HasValue == true)
			{
				query = query.Where(i => i.nextEscalationAt == nextEscalationAt.Value);
			}
			if (acknowledgedAt.HasValue == true)
			{
				query = query.Where(i => i.acknowledgedAt == acknowledgedAt.Value);
			}
			if (resolvedAt.HasValue == true)
			{
				query = query.Where(i => i.resolvedAt == resolvedAt.Value);
			}
			if (currentAssigneeObjectGuid.HasValue == true)
			{
				query = query.Where(i => i.currentAssigneeObjectGuid == currentAssigneeObjectGuid);
			}
			if (string.IsNullOrEmpty(sourcePayloadJson) == false)
			{
				query = query.Where(i => i.sourcePayloadJson == sourcePayloadJson);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(i => i.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(i => i.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(i => i.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(i => i.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(i => i.deleted == false);
				}
			}
			else
			{
				query = query.Where(i => i.active == true);
				query = query.Where(i => i.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Incident, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.incidentKey.Contains(anyStringContains)
			       || x.title.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.sourcePayloadJson.Contains(anyStringContains)
			       || x.escalationRule.targetType.Contains(anyStringContains)
			       || x.incidentStatusType.name.Contains(anyStringContains)
			       || x.incidentStatusType.description.Contains(anyStringContains)
			       || x.service.name.Contains(anyStringContains)
			       || x.service.description.Contains(anyStringContains)
			       || x.severityType.name.Contains(anyStringContains)
			       || x.severityType.description.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.incidentKey).ThenBy(x => x.title);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Incident.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
		}


        /// <summary>
        /// 
        /// This method creates an audit event from within the controller.  It is intended for use by custom logic in client applications that needs to create audit events.
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Incident/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
			{
			   return Forbid();
			}

		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
