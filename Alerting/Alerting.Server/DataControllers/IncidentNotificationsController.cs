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

namespace Foundation.Alerting.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the IncidentNotification entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the IncidentNotification entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class IncidentNotificationsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private AlertingContext _context;

		private ILogger<IncidentNotificationsController> _logger;

		public IncidentNotificationsController(AlertingContext context, ILogger<IncidentNotificationsController> logger) : base("Alerting", "IncidentNotification")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of IncidentNotifications filtered by the parameters provided.
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
		[Route("api/IncidentNotifications")]
		public async Task<IActionResult> GetIncidentNotifications(
			int? incidentId = null,
			int? escalationRuleId = null,
			Guid? userObjectGuid = null,
			DateTime? firstNotifiedAt = null,
			DateTime? lastNotifiedAt = null,
			DateTime? acknowledgedAt = null,
			Guid? acknowledgedByObjectGuid = null,
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
			if (firstNotifiedAt.HasValue == true && firstNotifiedAt.Value.Kind != DateTimeKind.Utc)
			{
				firstNotifiedAt = firstNotifiedAt.Value.ToUniversalTime();
			}

			if (lastNotifiedAt.HasValue == true && lastNotifiedAt.Value.Kind != DateTimeKind.Utc)
			{
				lastNotifiedAt = lastNotifiedAt.Value.ToUniversalTime();
			}

			if (acknowledgedAt.HasValue == true && acknowledgedAt.Value.Kind != DateTimeKind.Utc)
			{
				acknowledgedAt = acknowledgedAt.Value.ToUniversalTime();
			}

			IQueryable<Database.IncidentNotification> query = (from in_ in _context.IncidentNotifications select in_);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (incidentId.HasValue == true)
			{
				query = query.Where(in_ => in_.incidentId == incidentId.Value);
			}
			if (escalationRuleId.HasValue == true)
			{
				query = query.Where(in_ => in_.escalationRuleId == escalationRuleId.Value);
			}
			if (userObjectGuid.HasValue == true)
			{
				query = query.Where(in_ => in_.userObjectGuid == userObjectGuid);
			}
			if (firstNotifiedAt.HasValue == true)
			{
				query = query.Where(in_ => in_.firstNotifiedAt == firstNotifiedAt.Value);
			}
			if (lastNotifiedAt.HasValue == true)
			{
				query = query.Where(in_ => in_.lastNotifiedAt == lastNotifiedAt.Value);
			}
			if (acknowledgedAt.HasValue == true)
			{
				query = query.Where(in_ => in_.acknowledgedAt == acknowledgedAt.Value);
			}
			if (acknowledgedByObjectGuid.HasValue == true)
			{
				query = query.Where(in_ => in_.acknowledgedByObjectGuid == acknowledgedByObjectGuid);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(in_ => in_.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(in_ => in_.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(in_ => in_.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(in_ => in_.deleted == false);
				}
			}
			else
			{
				query = query.Where(in_ => in_.active == true);
				query = query.Where(in_ => in_.deleted == false);
			}

			query = query.OrderBy(in_ => in_.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.escalationRule);
				query = query.Include(x => x.incident);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Incident Notification, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       (includeRelations == true && x.escalationRule.targetType.Contains(anyStringContains))
			       || (includeRelations == true && x.incident.incidentKey.Contains(anyStringContains))
			       || (includeRelations == true && x.incident.title.Contains(anyStringContains))
			       || (includeRelations == true && x.incident.description.Contains(anyStringContains))
			       || (includeRelations == true && x.incident.sourcePayloadJson.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.IncidentNotification> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.IncidentNotification incidentNotification in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(incidentNotification, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Alerting.IncidentNotification Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Alerting.IncidentNotification Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of IncidentNotifications filtered by the parameters provided.  Its query is similar to the GetIncidentNotifications method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/IncidentNotifications/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? incidentId = null,
			int? escalationRuleId = null,
			Guid? userObjectGuid = null,
			DateTime? firstNotifiedAt = null,
			DateTime? lastNotifiedAt = null,
			DateTime? acknowledgedAt = null,
			Guid? acknowledgedByObjectGuid = null,
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
			if (firstNotifiedAt.HasValue == true && firstNotifiedAt.Value.Kind != DateTimeKind.Utc)
			{
				firstNotifiedAt = firstNotifiedAt.Value.ToUniversalTime();
			}

			if (lastNotifiedAt.HasValue == true && lastNotifiedAt.Value.Kind != DateTimeKind.Utc)
			{
				lastNotifiedAt = lastNotifiedAt.Value.ToUniversalTime();
			}

			if (acknowledgedAt.HasValue == true && acknowledgedAt.Value.Kind != DateTimeKind.Utc)
			{
				acknowledgedAt = acknowledgedAt.Value.ToUniversalTime();
			}

			IQueryable<Database.IncidentNotification> query = (from in_ in _context.IncidentNotifications select in_);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (incidentId.HasValue == true)
			{
				query = query.Where(in_ => in_.incidentId == incidentId.Value);
			}
			if (escalationRuleId.HasValue == true)
			{
				query = query.Where(in_ => in_.escalationRuleId == escalationRuleId.Value);
			}
			if (userObjectGuid.HasValue == true)
			{
				query = query.Where(in_ => in_.userObjectGuid == userObjectGuid);
			}
			if (firstNotifiedAt.HasValue == true)
			{
				query = query.Where(in_ => in_.firstNotifiedAt == firstNotifiedAt.Value);
			}
			if (lastNotifiedAt.HasValue == true)
			{
				query = query.Where(in_ => in_.lastNotifiedAt == lastNotifiedAt.Value);
			}
			if (acknowledgedAt.HasValue == true)
			{
				query = query.Where(in_ => in_.acknowledgedAt == acknowledgedAt.Value);
			}
			if (acknowledgedByObjectGuid.HasValue == true)
			{
				query = query.Where(in_ => in_.acknowledgedByObjectGuid == acknowledgedByObjectGuid);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(in_ => in_.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(in_ => in_.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(in_ => in_.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(in_ => in_.deleted == false);
				}
			}
			else
			{
				query = query.Where(in_ => in_.active == true);
				query = query.Where(in_ => in_.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Incident Notification, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.escalationRule.targetType.Contains(anyStringContains)
			       || x.incident.incidentKey.Contains(anyStringContains)
			       || x.incident.title.Contains(anyStringContains)
			       || x.incident.description.Contains(anyStringContains)
			       || x.incident.sourcePayloadJson.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single IncidentNotification by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/IncidentNotification/{id}")]
		public async Task<IActionResult> GetIncidentNotification(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.IncidentNotification> query = (from in_ in _context.IncidentNotifications where
							(in_.id == id) &&
							(userIsAdmin == true || in_.deleted == false) &&
							(userIsWriter == true || in_.active == true)
					select in_);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.escalationRule);
					query = query.Include(x => x.incident);
					query = query.AsSplitQuery();
				}

				Database.IncidentNotification materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Alerting.IncidentNotification Entity was read with Admin privilege." : "Alerting.IncidentNotification Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "IncidentNotification", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Alerting.IncidentNotification entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Alerting.IncidentNotification.   Entity was read with Admin privilege." : "Exception caught during entity read of Alerting.IncidentNotification.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing IncidentNotification record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/IncidentNotification/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutIncidentNotification(int id, [FromBody]Database.IncidentNotification.IncidentNotificationDTO incidentNotificationDTO, CancellationToken cancellationToken = default)
		{
			if (incidentNotificationDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != incidentNotificationDTO.id)
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


			IQueryable<Database.IncidentNotification> query = (from x in _context.IncidentNotifications
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.IncidentNotification existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.IncidentNotification PUT", id.ToString(), new Exception("No Alerting.IncidentNotification entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (incidentNotificationDTO.objectGuid == Guid.Empty)
            {
                incidentNotificationDTO.objectGuid = existing.objectGuid;
            }
            else if (incidentNotificationDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a IncidentNotification record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.IncidentNotification cloneOfExisting = (Database.IncidentNotification)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new IncidentNotification object using the data from the existing record, updated with what is in the DTO.
			//
			Database.IncidentNotification incidentNotification = (Database.IncidentNotification)_context.Entry(existing).GetDatabaseValues().ToObject();
			incidentNotification.ApplyDTO(incidentNotificationDTO);
			//
			// The tenant guid for any IncidentNotification being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the IncidentNotification because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				incidentNotification.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (incidentNotification.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Alerting.IncidentNotification record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (incidentNotification.firstNotifiedAt.Kind != DateTimeKind.Utc)
			{
				incidentNotification.firstNotifiedAt = incidentNotification.firstNotifiedAt.ToUniversalTime();
			}

			if (incidentNotification.lastNotifiedAt.HasValue == true && incidentNotification.lastNotifiedAt.Value.Kind != DateTimeKind.Utc)
			{
				incidentNotification.lastNotifiedAt = incidentNotification.lastNotifiedAt.Value.ToUniversalTime();
			}

			if (incidentNotification.acknowledgedAt.HasValue == true && incidentNotification.acknowledgedAt.Value.Kind != DateTimeKind.Utc)
			{
				incidentNotification.acknowledgedAt = incidentNotification.acknowledgedAt.Value.ToUniversalTime();
			}

			EntityEntry<Database.IncidentNotification> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(incidentNotification);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Alerting.IncidentNotification entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.IncidentNotification.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.IncidentNotification.CreateAnonymousWithFirstLevelSubObjects(incidentNotification)),
					null);


				return Ok(Database.IncidentNotification.CreateAnonymous(incidentNotification));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Alerting.IncidentNotification entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.IncidentNotification.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.IncidentNotification.CreateAnonymousWithFirstLevelSubObjects(incidentNotification)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new IncidentNotification record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/IncidentNotification", Name = "IncidentNotification")]
		public async Task<IActionResult> PostIncidentNotification([FromBody]Database.IncidentNotification.IncidentNotificationDTO incidentNotificationDTO, CancellationToken cancellationToken = default)
		{
			if (incidentNotificationDTO == null)
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
			// Create a new IncidentNotification object using the data from the DTO
			//
			Database.IncidentNotification incidentNotification = Database.IncidentNotification.FromDTO(incidentNotificationDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				incidentNotification.tenantGuid = userTenantGuid;

				if (incidentNotification.firstNotifiedAt.Kind != DateTimeKind.Utc)
				{
					incidentNotification.firstNotifiedAt = incidentNotification.firstNotifiedAt.ToUniversalTime();
				}

				if (incidentNotification.lastNotifiedAt.HasValue == true && incidentNotification.lastNotifiedAt.Value.Kind != DateTimeKind.Utc)
				{
					incidentNotification.lastNotifiedAt = incidentNotification.lastNotifiedAt.Value.ToUniversalTime();
				}

				if (incidentNotification.acknowledgedAt.HasValue == true && incidentNotification.acknowledgedAt.Value.Kind != DateTimeKind.Utc)
				{
					incidentNotification.acknowledgedAt = incidentNotification.acknowledgedAt.Value.ToUniversalTime();
				}

				incidentNotification.objectGuid = Guid.NewGuid();
				_context.IncidentNotifications.Add(incidentNotification);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Alerting.IncidentNotification entity successfully created.",
					true,
					incidentNotification.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.IncidentNotification.CreateAnonymousWithFirstLevelSubObjects(incidentNotification)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Alerting.IncidentNotification entity creation failed.", false, incidentNotification.id.ToString(), "", JsonSerializer.Serialize(incidentNotification), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "IncidentNotification", incidentNotification.id, incidentNotification.id.ToString()));

			return CreatedAtRoute("IncidentNotification", new { id = incidentNotification.id }, Database.IncidentNotification.CreateAnonymousWithFirstLevelSubObjects(incidentNotification));
		}



        /// <summary>
        /// 
        /// This deletes a IncidentNotification record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/IncidentNotification/{id}")]
		[Route("api/IncidentNotification")]
		public async Task<IActionResult> DeleteIncidentNotification(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.IncidentNotification> query = (from x in _context.IncidentNotifications
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.IncidentNotification incidentNotification = await query.FirstOrDefaultAsync(cancellationToken);

			if (incidentNotification == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.IncidentNotification DELETE", id.ToString(), new Exception("No Alerting.IncidentNotification entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.IncidentNotification cloneOfExisting = (Database.IncidentNotification)_context.Entry(incidentNotification).GetDatabaseValues().ToObject();


			try
			{
				incidentNotification.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Alerting.IncidentNotification entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.IncidentNotification.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.IncidentNotification.CreateAnonymousWithFirstLevelSubObjects(incidentNotification)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Alerting.IncidentNotification entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.IncidentNotification.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.IncidentNotification.CreateAnonymousWithFirstLevelSubObjects(incidentNotification)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of IncidentNotification records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/IncidentNotifications/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? incidentId = null,
			int? escalationRuleId = null,
			Guid? userObjectGuid = null,
			DateTime? firstNotifiedAt = null,
			DateTime? lastNotifiedAt = null,
			DateTime? acknowledgedAt = null,
			Guid? acknowledgedByObjectGuid = null,
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
			if (firstNotifiedAt.HasValue == true && firstNotifiedAt.Value.Kind != DateTimeKind.Utc)
			{
				firstNotifiedAt = firstNotifiedAt.Value.ToUniversalTime();
			}

			if (lastNotifiedAt.HasValue == true && lastNotifiedAt.Value.Kind != DateTimeKind.Utc)
			{
				lastNotifiedAt = lastNotifiedAt.Value.ToUniversalTime();
			}

			if (acknowledgedAt.HasValue == true && acknowledgedAt.Value.Kind != DateTimeKind.Utc)
			{
				acknowledgedAt = acknowledgedAt.Value.ToUniversalTime();
			}

			IQueryable<Database.IncidentNotification> query = (from in_ in _context.IncidentNotifications select in_);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (incidentId.HasValue == true)
			{
				query = query.Where(in_ => in_.incidentId == incidentId.Value);
			}
			if (escalationRuleId.HasValue == true)
			{
				query = query.Where(in_ => in_.escalationRuleId == escalationRuleId.Value);
			}
			if (userObjectGuid.HasValue == true)
			{
				query = query.Where(in_ => in_.userObjectGuid == userObjectGuid);
			}
			if (firstNotifiedAt.HasValue == true)
			{
				query = query.Where(in_ => in_.firstNotifiedAt == firstNotifiedAt.Value);
			}
			if (lastNotifiedAt.HasValue == true)
			{
				query = query.Where(in_ => in_.lastNotifiedAt == lastNotifiedAt.Value);
			}
			if (acknowledgedAt.HasValue == true)
			{
				query = query.Where(in_ => in_.acknowledgedAt == acknowledgedAt.Value);
			}
			if (acknowledgedByObjectGuid.HasValue == true)
			{
				query = query.Where(in_ => in_.acknowledgedByObjectGuid == acknowledgedByObjectGuid);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(in_ => in_.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(in_ => in_.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(in_ => in_.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(in_ => in_.deleted == false);
				}
			}
			else
			{
				query = query.Where(in_ => in_.active == true);
				query = query.Where(in_ => in_.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Incident Notification, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.escalationRule.targetType.Contains(anyStringContains)
			       || x.incident.incidentKey.Contains(anyStringContains)
			       || x.incident.title.Contains(anyStringContains)
			       || x.incident.description.Contains(anyStringContains)
			       || x.incident.sourcePayloadJson.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.IncidentNotification.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/IncidentNotification/CreateAuditEvent")]
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
