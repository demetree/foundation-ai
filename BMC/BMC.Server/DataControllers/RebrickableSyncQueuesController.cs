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
using Foundation.BMC.Database;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the RebrickableSyncQueue entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the RebrickableSyncQueue entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class RebrickableSyncQueuesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<RebrickableSyncQueuesController> _logger;

		public RebrickableSyncQueuesController(BMCContext context, ILogger<RebrickableSyncQueuesController> logger) : base("BMC", "RebrickableSyncQueue")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of RebrickableSyncQueues filtered by the parameters provided.
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
		[Route("api/RebrickableSyncQueues")]
		public async Task<IActionResult> GetRebrickableSyncQueues(
			string operationType = null,
			string entityType = null,
			long? entityId = null,
			string payload = null,
			string status = null,
			DateTime? createdDate = null,
			DateTime? lastAttemptDate = null,
			DateTime? completedDate = null,
			int? attemptCount = null,
			int? maxAttempts = null,
			string errorMessage = null,
			string responseBody = null,
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
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
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
			if (createdDate.HasValue == true && createdDate.Value.Kind != DateTimeKind.Utc)
			{
				createdDate = createdDate.Value.ToUniversalTime();
			}

			if (lastAttemptDate.HasValue == true && lastAttemptDate.Value.Kind != DateTimeKind.Utc)
			{
				lastAttemptDate = lastAttemptDate.Value.ToUniversalTime();
			}

			if (completedDate.HasValue == true && completedDate.Value.Kind != DateTimeKind.Utc)
			{
				completedDate = completedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.RebrickableSyncQueue> query = (from rsq in _context.RebrickableSyncQueues select rsq);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(operationType) == false)
			{
				query = query.Where(rsq => rsq.operationType == operationType);
			}
			if (string.IsNullOrEmpty(entityType) == false)
			{
				query = query.Where(rsq => rsq.entityType == entityType);
			}
			if (entityId.HasValue == true)
			{
				query = query.Where(rsq => rsq.entityId == entityId.Value);
			}
			if (string.IsNullOrEmpty(payload) == false)
			{
				query = query.Where(rsq => rsq.payload == payload);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(rsq => rsq.status == status);
			}
			if (createdDate.HasValue == true)
			{
				query = query.Where(rsq => rsq.createdDate == createdDate.Value);
			}
			if (lastAttemptDate.HasValue == true)
			{
				query = query.Where(rsq => rsq.lastAttemptDate == lastAttemptDate.Value);
			}
			if (completedDate.HasValue == true)
			{
				query = query.Where(rsq => rsq.completedDate == completedDate.Value);
			}
			if (attemptCount.HasValue == true)
			{
				query = query.Where(rsq => rsq.attemptCount == attemptCount.Value);
			}
			if (maxAttempts.HasValue == true)
			{
				query = query.Where(rsq => rsq.maxAttempts == maxAttempts.Value);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(rsq => rsq.errorMessage == errorMessage);
			}
			if (string.IsNullOrEmpty(responseBody) == false)
			{
				query = query.Where(rsq => rsq.responseBody == responseBody);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rsq => rsq.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rsq => rsq.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rsq => rsq.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rsq => rsq.deleted == false);
				}
			}
			else
			{
				query = query.Where(rsq => rsq.active == true);
				query = query.Where(rsq => rsq.deleted == false);
			}

			query = query.OrderBy(rsq => rsq.operationType).ThenBy(rsq => rsq.entityType).ThenBy(rsq => rsq.status);


			//
			// Add the any string contains parameter to span all the string fields on the Rebrickable Sync Queue, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.operationType.Contains(anyStringContains)
			       || x.entityType.Contains(anyStringContains)
			       || x.payload.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			       || x.responseBody.Contains(anyStringContains)
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
			
			List<Database.RebrickableSyncQueue> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.RebrickableSyncQueue rebrickableSyncQueue in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(rebrickableSyncQueue, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.RebrickableSyncQueue Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.RebrickableSyncQueue Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of RebrickableSyncQueues filtered by the parameters provided.  Its query is similar to the GetRebrickableSyncQueues method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RebrickableSyncQueues/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string operationType = null,
			string entityType = null,
			long? entityId = null,
			string payload = null,
			string status = null,
			DateTime? createdDate = null,
			DateTime? lastAttemptDate = null,
			DateTime? completedDate = null,
			int? attemptCount = null,
			int? maxAttempts = null,
			string errorMessage = null,
			string responseBody = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
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
			if (createdDate.HasValue == true && createdDate.Value.Kind != DateTimeKind.Utc)
			{
				createdDate = createdDate.Value.ToUniversalTime();
			}

			if (lastAttemptDate.HasValue == true && lastAttemptDate.Value.Kind != DateTimeKind.Utc)
			{
				lastAttemptDate = lastAttemptDate.Value.ToUniversalTime();
			}

			if (completedDate.HasValue == true && completedDate.Value.Kind != DateTimeKind.Utc)
			{
				completedDate = completedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.RebrickableSyncQueue> query = (from rsq in _context.RebrickableSyncQueues select rsq);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (operationType != null)
			{
				query = query.Where(rsq => rsq.operationType == operationType);
			}
			if (entityType != null)
			{
				query = query.Where(rsq => rsq.entityType == entityType);
			}
			if (entityId.HasValue == true)
			{
				query = query.Where(rsq => rsq.entityId == entityId.Value);
			}
			if (payload != null)
			{
				query = query.Where(rsq => rsq.payload == payload);
			}
			if (status != null)
			{
				query = query.Where(rsq => rsq.status == status);
			}
			if (createdDate.HasValue == true)
			{
				query = query.Where(rsq => rsq.createdDate == createdDate.Value);
			}
			if (lastAttemptDate.HasValue == true)
			{
				query = query.Where(rsq => rsq.lastAttemptDate == lastAttemptDate.Value);
			}
			if (completedDate.HasValue == true)
			{
				query = query.Where(rsq => rsq.completedDate == completedDate.Value);
			}
			if (attemptCount.HasValue == true)
			{
				query = query.Where(rsq => rsq.attemptCount == attemptCount.Value);
			}
			if (maxAttempts.HasValue == true)
			{
				query = query.Where(rsq => rsq.maxAttempts == maxAttempts.Value);
			}
			if (errorMessage != null)
			{
				query = query.Where(rsq => rsq.errorMessage == errorMessage);
			}
			if (responseBody != null)
			{
				query = query.Where(rsq => rsq.responseBody == responseBody);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rsq => rsq.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rsq => rsq.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rsq => rsq.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rsq => rsq.deleted == false);
				}
			}
			else
			{
				query = query.Where(rsq => rsq.active == true);
				query = query.Where(rsq => rsq.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Rebrickable Sync Queue, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.operationType.Contains(anyStringContains)
			       || x.entityType.Contains(anyStringContains)
			       || x.payload.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			       || x.responseBody.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single RebrickableSyncQueue by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RebrickableSyncQueue/{id}")]
		public async Task<IActionResult> GetRebrickableSyncQueue(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
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
				IQueryable<Database.RebrickableSyncQueue> query = (from rsq in _context.RebrickableSyncQueues where
							(rsq.id == id) &&
							(userIsAdmin == true || rsq.deleted == false) &&
							(userIsWriter == true || rsq.active == true)
					select rsq);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.RebrickableSyncQueue materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.RebrickableSyncQueue Entity was read with Admin privilege." : "BMC.RebrickableSyncQueue Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "RebrickableSyncQueue", materialized.id, materialized.operationType));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.RebrickableSyncQueue entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.RebrickableSyncQueue.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.RebrickableSyncQueue.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing RebrickableSyncQueue record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/RebrickableSyncQueue/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutRebrickableSyncQueue(int id, [FromBody]Database.RebrickableSyncQueue.RebrickableSyncQueueDTO rebrickableSyncQueueDTO, CancellationToken cancellationToken = default)
		{
			if (rebrickableSyncQueueDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != rebrickableSyncQueueDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
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


			IQueryable<Database.RebrickableSyncQueue> query = (from x in _context.RebrickableSyncQueues
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.RebrickableSyncQueue existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.RebrickableSyncQueue PUT", id.ToString(), new Exception("No BMC.RebrickableSyncQueue entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (rebrickableSyncQueueDTO.objectGuid == Guid.Empty)
            {
                rebrickableSyncQueueDTO.objectGuid = existing.objectGuid;
            }
            else if (rebrickableSyncQueueDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a RebrickableSyncQueue record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.RebrickableSyncQueue cloneOfExisting = (Database.RebrickableSyncQueue)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new RebrickableSyncQueue object using the data from the existing record, updated with what is in the DTO.
			//
			Database.RebrickableSyncQueue rebrickableSyncQueue = (Database.RebrickableSyncQueue)_context.Entry(existing).GetDatabaseValues().ToObject();
			rebrickableSyncQueue.ApplyDTO(rebrickableSyncQueueDTO);
			//
			// The tenant guid for any RebrickableSyncQueue being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the RebrickableSyncQueue because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				rebrickableSyncQueue.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (rebrickableSyncQueue.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.RebrickableSyncQueue record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (rebrickableSyncQueue.operationType != null && rebrickableSyncQueue.operationType.Length > 50)
			{
				rebrickableSyncQueue.operationType = rebrickableSyncQueue.operationType.Substring(0, 50);
			}

			if (rebrickableSyncQueue.entityType != null && rebrickableSyncQueue.entityType.Length > 50)
			{
				rebrickableSyncQueue.entityType = rebrickableSyncQueue.entityType.Substring(0, 50);
			}

			if (rebrickableSyncQueue.status != null && rebrickableSyncQueue.status.Length > 50)
			{
				rebrickableSyncQueue.status = rebrickableSyncQueue.status.Substring(0, 50);
			}

			if (rebrickableSyncQueue.createdDate.HasValue == true && rebrickableSyncQueue.createdDate.Value.Kind != DateTimeKind.Utc)
			{
				rebrickableSyncQueue.createdDate = rebrickableSyncQueue.createdDate.Value.ToUniversalTime();
			}

			if (rebrickableSyncQueue.lastAttemptDate.HasValue == true && rebrickableSyncQueue.lastAttemptDate.Value.Kind != DateTimeKind.Utc)
			{
				rebrickableSyncQueue.lastAttemptDate = rebrickableSyncQueue.lastAttemptDate.Value.ToUniversalTime();
			}

			if (rebrickableSyncQueue.completedDate.HasValue == true && rebrickableSyncQueue.completedDate.Value.Kind != DateTimeKind.Utc)
			{
				rebrickableSyncQueue.completedDate = rebrickableSyncQueue.completedDate.Value.ToUniversalTime();
			}

			EntityEntry<Database.RebrickableSyncQueue> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(rebrickableSyncQueue);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.RebrickableSyncQueue entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.RebrickableSyncQueue.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.RebrickableSyncQueue.CreateAnonymousWithFirstLevelSubObjects(rebrickableSyncQueue)),
					null);


				return Ok(Database.RebrickableSyncQueue.CreateAnonymous(rebrickableSyncQueue));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.RebrickableSyncQueue entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.RebrickableSyncQueue.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.RebrickableSyncQueue.CreateAnonymousWithFirstLevelSubObjects(rebrickableSyncQueue)),
					ex);

				return Problem(ex.Message);
			}

		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new RebrickableSyncQueue record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RebrickableSyncQueue", Name = "RebrickableSyncQueue")]
		public async Task<IActionResult> PostRebrickableSyncQueue([FromBody]Database.RebrickableSyncQueue.RebrickableSyncQueueDTO rebrickableSyncQueueDTO, CancellationToken cancellationToken = default)
		{
			if (rebrickableSyncQueueDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
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
			// Create a new RebrickableSyncQueue object using the data from the DTO
			//
			Database.RebrickableSyncQueue rebrickableSyncQueue = Database.RebrickableSyncQueue.FromDTO(rebrickableSyncQueueDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				rebrickableSyncQueue.tenantGuid = userTenantGuid;

				if (rebrickableSyncQueue.operationType != null && rebrickableSyncQueue.operationType.Length > 50)
				{
					rebrickableSyncQueue.operationType = rebrickableSyncQueue.operationType.Substring(0, 50);
				}

				if (rebrickableSyncQueue.entityType != null && rebrickableSyncQueue.entityType.Length > 50)
				{
					rebrickableSyncQueue.entityType = rebrickableSyncQueue.entityType.Substring(0, 50);
				}

				if (rebrickableSyncQueue.status != null && rebrickableSyncQueue.status.Length > 50)
				{
					rebrickableSyncQueue.status = rebrickableSyncQueue.status.Substring(0, 50);
				}

				if (rebrickableSyncQueue.createdDate.HasValue == true && rebrickableSyncQueue.createdDate.Value.Kind != DateTimeKind.Utc)
				{
					rebrickableSyncQueue.createdDate = rebrickableSyncQueue.createdDate.Value.ToUniversalTime();
				}

				if (rebrickableSyncQueue.lastAttemptDate.HasValue == true && rebrickableSyncQueue.lastAttemptDate.Value.Kind != DateTimeKind.Utc)
				{
					rebrickableSyncQueue.lastAttemptDate = rebrickableSyncQueue.lastAttemptDate.Value.ToUniversalTime();
				}

				if (rebrickableSyncQueue.completedDate.HasValue == true && rebrickableSyncQueue.completedDate.Value.Kind != DateTimeKind.Utc)
				{
					rebrickableSyncQueue.completedDate = rebrickableSyncQueue.completedDate.Value.ToUniversalTime();
				}

				rebrickableSyncQueue.objectGuid = Guid.NewGuid();
				_context.RebrickableSyncQueues.Add(rebrickableSyncQueue);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.RebrickableSyncQueue entity successfully created.",
					true,
					rebrickableSyncQueue.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.RebrickableSyncQueue.CreateAnonymousWithFirstLevelSubObjects(rebrickableSyncQueue)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.RebrickableSyncQueue entity creation failed.", false, rebrickableSyncQueue.id.ToString(), "", JsonSerializer.Serialize(rebrickableSyncQueue), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "RebrickableSyncQueue", rebrickableSyncQueue.id, rebrickableSyncQueue.operationType));

			return CreatedAtRoute("RebrickableSyncQueue", new { id = rebrickableSyncQueue.id }, Database.RebrickableSyncQueue.CreateAnonymousWithFirstLevelSubObjects(rebrickableSyncQueue));
		}

*/


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a RebrickableSyncQueue record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RebrickableSyncQueue/{id}")]
		[Route("api/RebrickableSyncQueue")]
		public async Task<IActionResult> DeleteRebrickableSyncQueue(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
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

			IQueryable<Database.RebrickableSyncQueue> query = (from x in _context.RebrickableSyncQueues
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.RebrickableSyncQueue rebrickableSyncQueue = await query.FirstOrDefaultAsync(cancellationToken);

			if (rebrickableSyncQueue == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.RebrickableSyncQueue DELETE", id.ToString(), new Exception("No BMC.RebrickableSyncQueue entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.RebrickableSyncQueue cloneOfExisting = (Database.RebrickableSyncQueue)_context.Entry(rebrickableSyncQueue).GetDatabaseValues().ToObject();


			try
			{
				rebrickableSyncQueue.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.RebrickableSyncQueue entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.RebrickableSyncQueue.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.RebrickableSyncQueue.CreateAnonymousWithFirstLevelSubObjects(rebrickableSyncQueue)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.RebrickableSyncQueue entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.RebrickableSyncQueue.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.RebrickableSyncQueue.CreateAnonymousWithFirstLevelSubObjects(rebrickableSyncQueue)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


*/
        /// <summary>
        /// 
        /// This gets a list of RebrickableSyncQueue records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/RebrickableSyncQueues/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string operationType = null,
			string entityType = null,
			long? entityId = null,
			string payload = null,
			string status = null,
			DateTime? createdDate = null,
			DateTime? lastAttemptDate = null,
			DateTime? completedDate = null,
			int? attemptCount = null,
			int? maxAttempts = null,
			string errorMessage = null,
			string responseBody = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);


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
			if (createdDate.HasValue == true && createdDate.Value.Kind != DateTimeKind.Utc)
			{
				createdDate = createdDate.Value.ToUniversalTime();
			}

			if (lastAttemptDate.HasValue == true && lastAttemptDate.Value.Kind != DateTimeKind.Utc)
			{
				lastAttemptDate = lastAttemptDate.Value.ToUniversalTime();
			}

			if (completedDate.HasValue == true && completedDate.Value.Kind != DateTimeKind.Utc)
			{
				completedDate = completedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.RebrickableSyncQueue> query = (from rsq in _context.RebrickableSyncQueues select rsq);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(operationType) == false)
			{
				query = query.Where(rsq => rsq.operationType == operationType);
			}
			if (string.IsNullOrEmpty(entityType) == false)
			{
				query = query.Where(rsq => rsq.entityType == entityType);
			}
			if (entityId.HasValue == true)
			{
				query = query.Where(rsq => rsq.entityId == entityId.Value);
			}
			if (string.IsNullOrEmpty(payload) == false)
			{
				query = query.Where(rsq => rsq.payload == payload);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(rsq => rsq.status == status);
			}
			if (createdDate.HasValue == true)
			{
				query = query.Where(rsq => rsq.createdDate == createdDate.Value);
			}
			if (lastAttemptDate.HasValue == true)
			{
				query = query.Where(rsq => rsq.lastAttemptDate == lastAttemptDate.Value);
			}
			if (completedDate.HasValue == true)
			{
				query = query.Where(rsq => rsq.completedDate == completedDate.Value);
			}
			if (attemptCount.HasValue == true)
			{
				query = query.Where(rsq => rsq.attemptCount == attemptCount.Value);
			}
			if (maxAttempts.HasValue == true)
			{
				query = query.Where(rsq => rsq.maxAttempts == maxAttempts.Value);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(rsq => rsq.errorMessage == errorMessage);
			}
			if (string.IsNullOrEmpty(responseBody) == false)
			{
				query = query.Where(rsq => rsq.responseBody == responseBody);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rsq => rsq.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rsq => rsq.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rsq => rsq.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rsq => rsq.deleted == false);
				}
			}
			else
			{
				query = query.Where(rsq => rsq.active == true);
				query = query.Where(rsq => rsq.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Rebrickable Sync Queue, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.operationType.Contains(anyStringContains)
			       || x.entityType.Contains(anyStringContains)
			       || x.payload.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			       || x.responseBody.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.operationType).ThenBy(x => x.entityType).ThenBy(x => x.status);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.RebrickableSyncQueue.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/RebrickableSyncQueue/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
