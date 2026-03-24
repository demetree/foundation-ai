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
    /// This auto generated class provides the basic CRUD operations for the PushDeliveryLog entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the PushDeliveryLog entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class PushDeliveryLogsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 50;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SchedulerContext _context;

		private ILogger<PushDeliveryLogsController> _logger;

		public PushDeliveryLogsController(SchedulerContext context, ILogger<PushDeliveryLogsController> logger) : base("Scheduler", "PushDeliveryLog")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of PushDeliveryLogs filtered by the parameters provided.
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
		[Route("api/PushDeliveryLogs")]
		public async Task<IActionResult> GetPushDeliveryLogs(
			int? userId = null,
			string providerId = null,
			string destination = null,
			string sourceType = null,
			int? sourceNotificationId = null,
			int? sourceConversationMessageId = null,
			bool? success = null,
			string externalId = null,
			string errorMessage = null,
			int? attemptNumber = null,
			DateTime? dateTimeCreated = null,
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

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

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
			if (dateTimeCreated.HasValue == true && dateTimeCreated.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeCreated = dateTimeCreated.Value.ToUniversalTime();
			}

			IQueryable<Database.PushDeliveryLog> query = (from pdl in _context.PushDeliveryLogs select pdl);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userId.HasValue == true)
			{
				query = query.Where(pdl => pdl.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(providerId) == false)
			{
				query = query.Where(pdl => pdl.providerId == providerId);
			}
			if (string.IsNullOrEmpty(destination) == false)
			{
				query = query.Where(pdl => pdl.destination == destination);
			}
			if (string.IsNullOrEmpty(sourceType) == false)
			{
				query = query.Where(pdl => pdl.sourceType == sourceType);
			}
			if (sourceNotificationId.HasValue == true)
			{
				query = query.Where(pdl => pdl.sourceNotificationId == sourceNotificationId.Value);
			}
			if (sourceConversationMessageId.HasValue == true)
			{
				query = query.Where(pdl => pdl.sourceConversationMessageId == sourceConversationMessageId.Value);
			}
			if (success.HasValue == true)
			{
				query = query.Where(pdl => pdl.success == success.Value);
			}
			if (string.IsNullOrEmpty(externalId) == false)
			{
				query = query.Where(pdl => pdl.externalId == externalId);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(pdl => pdl.errorMessage == errorMessage);
			}
			if (attemptNumber.HasValue == true)
			{
				query = query.Where(pdl => pdl.attemptNumber == attemptNumber.Value);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(pdl => pdl.dateTimeCreated == dateTimeCreated.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pdl => pdl.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pdl => pdl.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pdl => pdl.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pdl => pdl.deleted == false);
				}
			}
			else
			{
				query = query.Where(pdl => pdl.active == true);
				query = query.Where(pdl => pdl.deleted == false);
			}

			query = query.OrderBy(pdl => pdl.providerId).ThenBy(pdl => pdl.destination).ThenBy(pdl => pdl.sourceType);


			//
			// Add the any string contains parameter to span all the string fields on the Push Delivery Log, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.providerId.Contains(anyStringContains)
			       || x.destination.Contains(anyStringContains)
			       || x.sourceType.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			   );
			}

			if (includeRelations == true)
			{
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.PushDeliveryLog> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.PushDeliveryLog pushDeliveryLog in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(pushDeliveryLog, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.PushDeliveryLog Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.PushDeliveryLog Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of PushDeliveryLogs filtered by the parameters provided.  Its query is similar to the GetPushDeliveryLogs method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PushDeliveryLogs/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? userId = null,
			string providerId = null,
			string destination = null,
			string sourceType = null,
			int? sourceNotificationId = null,
			int? sourceConversationMessageId = null,
			bool? success = null,
			string externalId = null,
			string errorMessage = null,
			int? attemptNumber = null,
			DateTime? dateTimeCreated = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
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
			if (dateTimeCreated.HasValue == true && dateTimeCreated.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeCreated = dateTimeCreated.Value.ToUniversalTime();
			}

			IQueryable<Database.PushDeliveryLog> query = (from pdl in _context.PushDeliveryLogs select pdl);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (userId.HasValue == true)
			{
				query = query.Where(pdl => pdl.userId == userId.Value);
			}
			if (providerId != null)
			{
				query = query.Where(pdl => pdl.providerId == providerId);
			}
			if (destination != null)
			{
				query = query.Where(pdl => pdl.destination == destination);
			}
			if (sourceType != null)
			{
				query = query.Where(pdl => pdl.sourceType == sourceType);
			}
			if (sourceNotificationId.HasValue == true)
			{
				query = query.Where(pdl => pdl.sourceNotificationId == sourceNotificationId.Value);
			}
			if (sourceConversationMessageId.HasValue == true)
			{
				query = query.Where(pdl => pdl.sourceConversationMessageId == sourceConversationMessageId.Value);
			}
			if (success.HasValue == true)
			{
				query = query.Where(pdl => pdl.success == success.Value);
			}
			if (externalId != null)
			{
				query = query.Where(pdl => pdl.externalId == externalId);
			}
			if (errorMessage != null)
			{
				query = query.Where(pdl => pdl.errorMessage == errorMessage);
			}
			if (attemptNumber.HasValue == true)
			{
				query = query.Where(pdl => pdl.attemptNumber == attemptNumber.Value);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(pdl => pdl.dateTimeCreated == dateTimeCreated.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pdl => pdl.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pdl => pdl.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pdl => pdl.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pdl => pdl.deleted == false);
				}
			}
			else
			{
				query = query.Where(pdl => pdl.active == true);
				query = query.Where(pdl => pdl.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Push Delivery Log, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.providerId.Contains(anyStringContains)
			       || x.destination.Contains(anyStringContains)
			       || x.sourceType.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single PushDeliveryLog by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PushDeliveryLog/{id}")]
		public async Task<IActionResult> GetPushDeliveryLog(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			
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
				IQueryable<Database.PushDeliveryLog> query = (from pdl in _context.PushDeliveryLogs where
							(pdl.id == id) &&
							(userIsAdmin == true || pdl.deleted == false) &&
							(userIsWriter == true || pdl.active == true)
					select pdl);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.PushDeliveryLog materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.PushDeliveryLog Entity was read with Admin privilege." : "Scheduler.PushDeliveryLog Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PushDeliveryLog", materialized.id, materialized.providerId));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.PushDeliveryLog entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.PushDeliveryLog.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.PushDeliveryLog.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing PushDeliveryLog record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/PushDeliveryLog/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutPushDeliveryLog(int id, [FromBody]Database.PushDeliveryLog.PushDeliveryLogDTO pushDeliveryLogDTO, CancellationToken cancellationToken = default)
		{
			if (pushDeliveryLogDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != pushDeliveryLogDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
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


			IQueryable<Database.PushDeliveryLog> query = (from x in _context.PushDeliveryLogs
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.PushDeliveryLog existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.PushDeliveryLog PUT", id.ToString(), new Exception("No Scheduler.PushDeliveryLog entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (pushDeliveryLogDTO.objectGuid == Guid.Empty)
            {
                pushDeliveryLogDTO.objectGuid = existing.objectGuid;
            }
            else if (pushDeliveryLogDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a PushDeliveryLog record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.PushDeliveryLog cloneOfExisting = (Database.PushDeliveryLog)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new PushDeliveryLog object using the data from the existing record, updated with what is in the DTO.
			//
			Database.PushDeliveryLog pushDeliveryLog = (Database.PushDeliveryLog)_context.Entry(existing).GetDatabaseValues().ToObject();
			pushDeliveryLog.ApplyDTO(pushDeliveryLogDTO);
			//
			// The tenant guid for any PushDeliveryLog being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the PushDeliveryLog because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				pushDeliveryLog.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (pushDeliveryLog.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.PushDeliveryLog record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (pushDeliveryLog.providerId != null && pushDeliveryLog.providerId.Length > 50)
			{
				pushDeliveryLog.providerId = pushDeliveryLog.providerId.Substring(0, 50);
			}

			if (pushDeliveryLog.destination != null && pushDeliveryLog.destination.Length > 250)
			{
				pushDeliveryLog.destination = pushDeliveryLog.destination.Substring(0, 250);
			}

			if (pushDeliveryLog.sourceType != null && pushDeliveryLog.sourceType.Length > 50)
			{
				pushDeliveryLog.sourceType = pushDeliveryLog.sourceType.Substring(0, 50);
			}

			if (pushDeliveryLog.externalId != null && pushDeliveryLog.externalId.Length > 250)
			{
				pushDeliveryLog.externalId = pushDeliveryLog.externalId.Substring(0, 250);
			}

			if (pushDeliveryLog.errorMessage != null && pushDeliveryLog.errorMessage.Length > 1000)
			{
				pushDeliveryLog.errorMessage = pushDeliveryLog.errorMessage.Substring(0, 1000);
			}

			if (pushDeliveryLog.dateTimeCreated.Kind != DateTimeKind.Utc)
			{
				pushDeliveryLog.dateTimeCreated = pushDeliveryLog.dateTimeCreated.ToUniversalTime();
			}

			EntityEntry<Database.PushDeliveryLog> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(pushDeliveryLog);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.PushDeliveryLog entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.PushDeliveryLog.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PushDeliveryLog.CreateAnonymousWithFirstLevelSubObjects(pushDeliveryLog)),
					null);


				return Ok(Database.PushDeliveryLog.CreateAnonymous(pushDeliveryLog));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.PushDeliveryLog entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.PushDeliveryLog.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PushDeliveryLog.CreateAnonymousWithFirstLevelSubObjects(pushDeliveryLog)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new PushDeliveryLog record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PushDeliveryLog", Name = "PushDeliveryLog")]
		public async Task<IActionResult> PostPushDeliveryLog([FromBody]Database.PushDeliveryLog.PushDeliveryLogDTO pushDeliveryLogDTO, CancellationToken cancellationToken = default)
		{
			if (pushDeliveryLogDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
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
			// Create a new PushDeliveryLog object using the data from the DTO
			//
			Database.PushDeliveryLog pushDeliveryLog = Database.PushDeliveryLog.FromDTO(pushDeliveryLogDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				pushDeliveryLog.tenantGuid = userTenantGuid;

				if (pushDeliveryLog.providerId != null && pushDeliveryLog.providerId.Length > 50)
				{
					pushDeliveryLog.providerId = pushDeliveryLog.providerId.Substring(0, 50);
				}

				if (pushDeliveryLog.destination != null && pushDeliveryLog.destination.Length > 250)
				{
					pushDeliveryLog.destination = pushDeliveryLog.destination.Substring(0, 250);
				}

				if (pushDeliveryLog.sourceType != null && pushDeliveryLog.sourceType.Length > 50)
				{
					pushDeliveryLog.sourceType = pushDeliveryLog.sourceType.Substring(0, 50);
				}

				if (pushDeliveryLog.externalId != null && pushDeliveryLog.externalId.Length > 250)
				{
					pushDeliveryLog.externalId = pushDeliveryLog.externalId.Substring(0, 250);
				}

				if (pushDeliveryLog.errorMessage != null && pushDeliveryLog.errorMessage.Length > 1000)
				{
					pushDeliveryLog.errorMessage = pushDeliveryLog.errorMessage.Substring(0, 1000);
				}

				if (pushDeliveryLog.dateTimeCreated.Kind != DateTimeKind.Utc)
				{
					pushDeliveryLog.dateTimeCreated = pushDeliveryLog.dateTimeCreated.ToUniversalTime();
				}

				pushDeliveryLog.objectGuid = Guid.NewGuid();
				_context.PushDeliveryLogs.Add(pushDeliveryLog);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.PushDeliveryLog entity successfully created.",
					true,
					pushDeliveryLog.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.PushDeliveryLog.CreateAnonymousWithFirstLevelSubObjects(pushDeliveryLog)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.PushDeliveryLog entity creation failed.", false, pushDeliveryLog.id.ToString(), "", JsonSerializer.Serialize(pushDeliveryLog), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PushDeliveryLog", pushDeliveryLog.id, pushDeliveryLog.providerId));

			return CreatedAtRoute("PushDeliveryLog", new { id = pushDeliveryLog.id }, Database.PushDeliveryLog.CreateAnonymousWithFirstLevelSubObjects(pushDeliveryLog));
		}



        /// <summary>
        /// 
        /// This deletes a PushDeliveryLog record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PushDeliveryLog/{id}")]
		[Route("api/PushDeliveryLog")]
		public async Task<IActionResult> DeletePushDeliveryLog(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.PushDeliveryLog> query = (from x in _context.PushDeliveryLogs
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.PushDeliveryLog pushDeliveryLog = await query.FirstOrDefaultAsync(cancellationToken);

			if (pushDeliveryLog == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.PushDeliveryLog DELETE", id.ToString(), new Exception("No Scheduler.PushDeliveryLog entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.PushDeliveryLog cloneOfExisting = (Database.PushDeliveryLog)_context.Entry(pushDeliveryLog).GetDatabaseValues().ToObject();


			try
			{
				pushDeliveryLog.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.PushDeliveryLog entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.PushDeliveryLog.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PushDeliveryLog.CreateAnonymousWithFirstLevelSubObjects(pushDeliveryLog)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.PushDeliveryLog entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.PushDeliveryLog.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PushDeliveryLog.CreateAnonymousWithFirstLevelSubObjects(pushDeliveryLog)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of PushDeliveryLog records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/PushDeliveryLogs/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? userId = null,
			string providerId = null,
			string destination = null,
			string sourceType = null,
			int? sourceNotificationId = null,
			int? sourceConversationMessageId = null,
			bool? success = null,
			string externalId = null,
			string errorMessage = null,
			int? attemptNumber = null,
			DateTime? dateTimeCreated = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);


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
			if (dateTimeCreated.HasValue == true && dateTimeCreated.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeCreated = dateTimeCreated.Value.ToUniversalTime();
			}

			IQueryable<Database.PushDeliveryLog> query = (from pdl in _context.PushDeliveryLogs select pdl);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userId.HasValue == true)
			{
				query = query.Where(pdl => pdl.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(providerId) == false)
			{
				query = query.Where(pdl => pdl.providerId == providerId);
			}
			if (string.IsNullOrEmpty(destination) == false)
			{
				query = query.Where(pdl => pdl.destination == destination);
			}
			if (string.IsNullOrEmpty(sourceType) == false)
			{
				query = query.Where(pdl => pdl.sourceType == sourceType);
			}
			if (sourceNotificationId.HasValue == true)
			{
				query = query.Where(pdl => pdl.sourceNotificationId == sourceNotificationId.Value);
			}
			if (sourceConversationMessageId.HasValue == true)
			{
				query = query.Where(pdl => pdl.sourceConversationMessageId == sourceConversationMessageId.Value);
			}
			if (success.HasValue == true)
			{
				query = query.Where(pdl => pdl.success == success.Value);
			}
			if (string.IsNullOrEmpty(externalId) == false)
			{
				query = query.Where(pdl => pdl.externalId == externalId);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(pdl => pdl.errorMessage == errorMessage);
			}
			if (attemptNumber.HasValue == true)
			{
				query = query.Where(pdl => pdl.attemptNumber == attemptNumber.Value);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(pdl => pdl.dateTimeCreated == dateTimeCreated.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pdl => pdl.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pdl => pdl.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pdl => pdl.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pdl => pdl.deleted == false);
				}
			}
			else
			{
				query = query.Where(pdl => pdl.active == true);
				query = query.Where(pdl => pdl.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Push Delivery Log, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.providerId.Contains(anyStringContains)
			       || x.destination.Contains(anyStringContains)
			       || x.sourceType.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.providerId).ThenBy(x => x.destination).ThenBy(x => x.sourceType);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.PushDeliveryLog.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/PushDeliveryLog/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
