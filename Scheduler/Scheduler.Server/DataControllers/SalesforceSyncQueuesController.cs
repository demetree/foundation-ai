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
    /// This auto generated class provides the basic CRUD operations for the SalesforceSyncQueue entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SalesforceSyncQueue entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SalesforceSyncQueuesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private SchedulerContext _context;

		private ILogger<SalesforceSyncQueuesController> _logger;

		public SalesforceSyncQueuesController(SchedulerContext context, ILogger<SalesforceSyncQueuesController> logger) : base("Scheduler", "SalesforceSyncQueue")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of SalesforceSyncQueues filtered by the parameters provided.
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
		[Route("api/SalesforceSyncQueues")]
		public async Task<IActionResult> GetSalesforceSyncQueues(
			string entityType = null,
			string operationType = null,
			int? entityId = null,
			string payload = null,
			string status = null,
			int? attemptCount = null,
			int? maxAttempts = null,
			DateTime? lastAttemptDate = null,
			DateTime? completedDate = null,
			DateTime? createdDate = null,
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
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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
			if (lastAttemptDate.HasValue == true && lastAttemptDate.Value.Kind != DateTimeKind.Utc)
			{
				lastAttemptDate = lastAttemptDate.Value.ToUniversalTime();
			}

			if (completedDate.HasValue == true && completedDate.Value.Kind != DateTimeKind.Utc)
			{
				completedDate = completedDate.Value.ToUniversalTime();
			}

			if (createdDate.HasValue == true && createdDate.Value.Kind != DateTimeKind.Utc)
			{
				createdDate = createdDate.Value.ToUniversalTime();
			}

			IQueryable<Database.SalesforceSyncQueue> query = (from ssq in _context.SalesforceSyncQueues select ssq);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(entityType) == false)
			{
				query = query.Where(ssq => ssq.entityType == entityType);
			}
			if (string.IsNullOrEmpty(operationType) == false)
			{
				query = query.Where(ssq => ssq.operationType == operationType);
			}
			if (entityId.HasValue == true)
			{
				query = query.Where(ssq => ssq.entityId == entityId.Value);
			}
			if (string.IsNullOrEmpty(payload) == false)
			{
				query = query.Where(ssq => ssq.payload == payload);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(ssq => ssq.status == status);
			}
			if (attemptCount.HasValue == true)
			{
				query = query.Where(ssq => ssq.attemptCount == attemptCount.Value);
			}
			if (maxAttempts.HasValue == true)
			{
				query = query.Where(ssq => ssq.maxAttempts == maxAttempts.Value);
			}
			if (lastAttemptDate.HasValue == true)
			{
				query = query.Where(ssq => ssq.lastAttemptDate == lastAttemptDate.Value);
			}
			if (completedDate.HasValue == true)
			{
				query = query.Where(ssq => ssq.completedDate == completedDate.Value);
			}
			if (createdDate.HasValue == true)
			{
				query = query.Where(ssq => ssq.createdDate == createdDate.Value);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(ssq => ssq.errorMessage == errorMessage);
			}
			if (string.IsNullOrEmpty(responseBody) == false)
			{
				query = query.Where(ssq => ssq.responseBody == responseBody);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ssq => ssq.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ssq => ssq.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ssq => ssq.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ssq => ssq.deleted == false);
				}
			}
			else
			{
				query = query.Where(ssq => ssq.active == true);
				query = query.Where(ssq => ssq.deleted == false);
			}

			query = query.OrderBy(ssq => ssq.entityType).ThenBy(ssq => ssq.operationType).ThenBy(ssq => ssq.status);


			//
			// Add the any string contains parameter to span all the string fields on the Salesforce Sync Queue, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.entityType.Contains(anyStringContains)
			       || x.operationType.Contains(anyStringContains)
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
			
			List<Database.SalesforceSyncQueue> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SalesforceSyncQueue salesforceSyncQueue in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(salesforceSyncQueue, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.SalesforceSyncQueue Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.SalesforceSyncQueue Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of SalesforceSyncQueues filtered by the parameters provided.  Its query is similar to the GetSalesforceSyncQueues method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SalesforceSyncQueues/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string entityType = null,
			string operationType = null,
			int? entityId = null,
			string payload = null,
			string status = null,
			int? attemptCount = null,
			int? maxAttempts = null,
			DateTime? lastAttemptDate = null,
			DateTime? completedDate = null,
			DateTime? createdDate = null,
			string errorMessage = null,
			string responseBody = null,
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
			if (lastAttemptDate.HasValue == true && lastAttemptDate.Value.Kind != DateTimeKind.Utc)
			{
				lastAttemptDate = lastAttemptDate.Value.ToUniversalTime();
			}

			if (completedDate.HasValue == true && completedDate.Value.Kind != DateTimeKind.Utc)
			{
				completedDate = completedDate.Value.ToUniversalTime();
			}

			if (createdDate.HasValue == true && createdDate.Value.Kind != DateTimeKind.Utc)
			{
				createdDate = createdDate.Value.ToUniversalTime();
			}

			IQueryable<Database.SalesforceSyncQueue> query = (from ssq in _context.SalesforceSyncQueues select ssq);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (entityType != null)
			{
				query = query.Where(ssq => ssq.entityType == entityType);
			}
			if (operationType != null)
			{
				query = query.Where(ssq => ssq.operationType == operationType);
			}
			if (entityId.HasValue == true)
			{
				query = query.Where(ssq => ssq.entityId == entityId.Value);
			}
			if (payload != null)
			{
				query = query.Where(ssq => ssq.payload == payload);
			}
			if (status != null)
			{
				query = query.Where(ssq => ssq.status == status);
			}
			if (attemptCount.HasValue == true)
			{
				query = query.Where(ssq => ssq.attemptCount == attemptCount.Value);
			}
			if (maxAttempts.HasValue == true)
			{
				query = query.Where(ssq => ssq.maxAttempts == maxAttempts.Value);
			}
			if (lastAttemptDate.HasValue == true)
			{
				query = query.Where(ssq => ssq.lastAttemptDate == lastAttemptDate.Value);
			}
			if (completedDate.HasValue == true)
			{
				query = query.Where(ssq => ssq.completedDate == completedDate.Value);
			}
			if (createdDate.HasValue == true)
			{
				query = query.Where(ssq => ssq.createdDate == createdDate.Value);
			}
			if (errorMessage != null)
			{
				query = query.Where(ssq => ssq.errorMessage == errorMessage);
			}
			if (responseBody != null)
			{
				query = query.Where(ssq => ssq.responseBody == responseBody);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ssq => ssq.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ssq => ssq.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ssq => ssq.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ssq => ssq.deleted == false);
				}
			}
			else
			{
				query = query.Where(ssq => ssq.active == true);
				query = query.Where(ssq => ssq.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Salesforce Sync Queue, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.entityType.Contains(anyStringContains)
			       || x.operationType.Contains(anyStringContains)
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
        /// This gets a single SalesforceSyncQueue by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SalesforceSyncQueue/{id}")]
		public async Task<IActionResult> GetSalesforceSyncQueue(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.SalesforceSyncQueue> query = (from ssq in _context.SalesforceSyncQueues where
							(ssq.id == id) &&
							(userIsAdmin == true || ssq.deleted == false) &&
							(userIsWriter == true || ssq.active == true)
					select ssq);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.SalesforceSyncQueue materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.SalesforceSyncQueue Entity was read with Admin privilege." : "Scheduler.SalesforceSyncQueue Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SalesforceSyncQueue", materialized.id, materialized.entityType));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.SalesforceSyncQueue entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.SalesforceSyncQueue.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.SalesforceSyncQueue.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing SalesforceSyncQueue record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SalesforceSyncQueue/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSalesforceSyncQueue(int id, [FromBody]Database.SalesforceSyncQueue.SalesforceSyncQueueDTO salesforceSyncQueueDTO, CancellationToken cancellationToken = default)
		{
			if (salesforceSyncQueueDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != salesforceSyncQueueDTO.id)
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


			IQueryable<Database.SalesforceSyncQueue> query = (from x in _context.SalesforceSyncQueues
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.SalesforceSyncQueue existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.SalesforceSyncQueue PUT", id.ToString(), new Exception("No Scheduler.SalesforceSyncQueue entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (salesforceSyncQueueDTO.objectGuid == Guid.Empty)
            {
                salesforceSyncQueueDTO.objectGuid = existing.objectGuid;
            }
            else if (salesforceSyncQueueDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a SalesforceSyncQueue record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SalesforceSyncQueue cloneOfExisting = (Database.SalesforceSyncQueue)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SalesforceSyncQueue object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SalesforceSyncQueue salesforceSyncQueue = (Database.SalesforceSyncQueue)_context.Entry(existing).GetDatabaseValues().ToObject();
			salesforceSyncQueue.ApplyDTO(salesforceSyncQueueDTO);
			//
			// The tenant guid for any SalesforceSyncQueue being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the SalesforceSyncQueue because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				salesforceSyncQueue.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (salesforceSyncQueue.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.SalesforceSyncQueue record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (salesforceSyncQueue.entityType != null && salesforceSyncQueue.entityType.Length > 100)
			{
				salesforceSyncQueue.entityType = salesforceSyncQueue.entityType.Substring(0, 100);
			}

			if (salesforceSyncQueue.operationType != null && salesforceSyncQueue.operationType.Length > 50)
			{
				salesforceSyncQueue.operationType = salesforceSyncQueue.operationType.Substring(0, 50);
			}

			if (salesforceSyncQueue.status != null && salesforceSyncQueue.status.Length > 50)
			{
				salesforceSyncQueue.status = salesforceSyncQueue.status.Substring(0, 50);
			}

			if (salesforceSyncQueue.lastAttemptDate.HasValue == true && salesforceSyncQueue.lastAttemptDate.Value.Kind != DateTimeKind.Utc)
			{
				salesforceSyncQueue.lastAttemptDate = salesforceSyncQueue.lastAttemptDate.Value.ToUniversalTime();
			}

			if (salesforceSyncQueue.completedDate.HasValue == true && salesforceSyncQueue.completedDate.Value.Kind != DateTimeKind.Utc)
			{
				salesforceSyncQueue.completedDate = salesforceSyncQueue.completedDate.Value.ToUniversalTime();
			}

			if (salesforceSyncQueue.createdDate.HasValue == true && salesforceSyncQueue.createdDate.Value.Kind != DateTimeKind.Utc)
			{
				salesforceSyncQueue.createdDate = salesforceSyncQueue.createdDate.Value.ToUniversalTime();
			}

			EntityEntry<Database.SalesforceSyncQueue> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(salesforceSyncQueue);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.SalesforceSyncQueue entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SalesforceSyncQueue.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SalesforceSyncQueue.CreateAnonymousWithFirstLevelSubObjects(salesforceSyncQueue)),
					null);


				return Ok(Database.SalesforceSyncQueue.CreateAnonymous(salesforceSyncQueue));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.SalesforceSyncQueue entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SalesforceSyncQueue.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SalesforceSyncQueue.CreateAnonymousWithFirstLevelSubObjects(salesforceSyncQueue)),
					ex);

				return Problem(ex.Message);
			}

		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new SalesforceSyncQueue record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SalesforceSyncQueue", Name = "SalesforceSyncQueue")]
		public async Task<IActionResult> PostSalesforceSyncQueue([FromBody]Database.SalesforceSyncQueue.SalesforceSyncQueueDTO salesforceSyncQueueDTO, CancellationToken cancellationToken = default)
		{
			if (salesforceSyncQueueDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
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
			// Create a new SalesforceSyncQueue object using the data from the DTO
			//
			Database.SalesforceSyncQueue salesforceSyncQueue = Database.SalesforceSyncQueue.FromDTO(salesforceSyncQueueDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				salesforceSyncQueue.tenantGuid = userTenantGuid;

				if (salesforceSyncQueue.entityType != null && salesforceSyncQueue.entityType.Length > 100)
				{
					salesforceSyncQueue.entityType = salesforceSyncQueue.entityType.Substring(0, 100);
				}

				if (salesforceSyncQueue.operationType != null && salesforceSyncQueue.operationType.Length > 50)
				{
					salesforceSyncQueue.operationType = salesforceSyncQueue.operationType.Substring(0, 50);
				}

				if (salesforceSyncQueue.status != null && salesforceSyncQueue.status.Length > 50)
				{
					salesforceSyncQueue.status = salesforceSyncQueue.status.Substring(0, 50);
				}

				if (salesforceSyncQueue.lastAttemptDate.HasValue == true && salesforceSyncQueue.lastAttemptDate.Value.Kind != DateTimeKind.Utc)
				{
					salesforceSyncQueue.lastAttemptDate = salesforceSyncQueue.lastAttemptDate.Value.ToUniversalTime();
				}

				if (salesforceSyncQueue.completedDate.HasValue == true && salesforceSyncQueue.completedDate.Value.Kind != DateTimeKind.Utc)
				{
					salesforceSyncQueue.completedDate = salesforceSyncQueue.completedDate.Value.ToUniversalTime();
				}

				if (salesforceSyncQueue.createdDate.HasValue == true && salesforceSyncQueue.createdDate.Value.Kind != DateTimeKind.Utc)
				{
					salesforceSyncQueue.createdDate = salesforceSyncQueue.createdDate.Value.ToUniversalTime();
				}

				salesforceSyncQueue.objectGuid = Guid.NewGuid();
				_context.SalesforceSyncQueues.Add(salesforceSyncQueue);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.SalesforceSyncQueue entity successfully created.",
					true,
					salesforceSyncQueue.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.SalesforceSyncQueue.CreateAnonymousWithFirstLevelSubObjects(salesforceSyncQueue)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.SalesforceSyncQueue entity creation failed.", false, salesforceSyncQueue.id.ToString(), "", JsonSerializer.Serialize(salesforceSyncQueue), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SalesforceSyncQueue", salesforceSyncQueue.id, salesforceSyncQueue.entityType));

			return CreatedAtRoute("SalesforceSyncQueue", new { id = salesforceSyncQueue.id }, Database.SalesforceSyncQueue.CreateAnonymousWithFirstLevelSubObjects(salesforceSyncQueue));
		}

*/


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a SalesforceSyncQueue record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SalesforceSyncQueue/{id}")]
		[Route("api/SalesforceSyncQueue")]
		public async Task<IActionResult> DeleteSalesforceSyncQueue(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
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

			IQueryable<Database.SalesforceSyncQueue> query = (from x in _context.SalesforceSyncQueues
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.SalesforceSyncQueue salesforceSyncQueue = await query.FirstOrDefaultAsync(cancellationToken);

			if (salesforceSyncQueue == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.SalesforceSyncQueue DELETE", id.ToString(), new Exception("No Scheduler.SalesforceSyncQueue entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SalesforceSyncQueue cloneOfExisting = (Database.SalesforceSyncQueue)_context.Entry(salesforceSyncQueue).GetDatabaseValues().ToObject();


			try
			{
				salesforceSyncQueue.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.SalesforceSyncQueue entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SalesforceSyncQueue.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SalesforceSyncQueue.CreateAnonymousWithFirstLevelSubObjects(salesforceSyncQueue)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.SalesforceSyncQueue entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SalesforceSyncQueue.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SalesforceSyncQueue.CreateAnonymousWithFirstLevelSubObjects(salesforceSyncQueue)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


*/
        /// <summary>
        /// 
        /// This gets a list of SalesforceSyncQueue records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SalesforceSyncQueues/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string entityType = null,
			string operationType = null,
			int? entityId = null,
			string payload = null,
			string status = null,
			int? attemptCount = null,
			int? maxAttempts = null,
			DateTime? lastAttemptDate = null,
			DateTime? completedDate = null,
			DateTime? createdDate = null,
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
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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
			if (lastAttemptDate.HasValue == true && lastAttemptDate.Value.Kind != DateTimeKind.Utc)
			{
				lastAttemptDate = lastAttemptDate.Value.ToUniversalTime();
			}

			if (completedDate.HasValue == true && completedDate.Value.Kind != DateTimeKind.Utc)
			{
				completedDate = completedDate.Value.ToUniversalTime();
			}

			if (createdDate.HasValue == true && createdDate.Value.Kind != DateTimeKind.Utc)
			{
				createdDate = createdDate.Value.ToUniversalTime();
			}

			IQueryable<Database.SalesforceSyncQueue> query = (from ssq in _context.SalesforceSyncQueues select ssq);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(entityType) == false)
			{
				query = query.Where(ssq => ssq.entityType == entityType);
			}
			if (string.IsNullOrEmpty(operationType) == false)
			{
				query = query.Where(ssq => ssq.operationType == operationType);
			}
			if (entityId.HasValue == true)
			{
				query = query.Where(ssq => ssq.entityId == entityId.Value);
			}
			if (string.IsNullOrEmpty(payload) == false)
			{
				query = query.Where(ssq => ssq.payload == payload);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(ssq => ssq.status == status);
			}
			if (attemptCount.HasValue == true)
			{
				query = query.Where(ssq => ssq.attemptCount == attemptCount.Value);
			}
			if (maxAttempts.HasValue == true)
			{
				query = query.Where(ssq => ssq.maxAttempts == maxAttempts.Value);
			}
			if (lastAttemptDate.HasValue == true)
			{
				query = query.Where(ssq => ssq.lastAttemptDate == lastAttemptDate.Value);
			}
			if (completedDate.HasValue == true)
			{
				query = query.Where(ssq => ssq.completedDate == completedDate.Value);
			}
			if (createdDate.HasValue == true)
			{
				query = query.Where(ssq => ssq.createdDate == createdDate.Value);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(ssq => ssq.errorMessage == errorMessage);
			}
			if (string.IsNullOrEmpty(responseBody) == false)
			{
				query = query.Where(ssq => ssq.responseBody == responseBody);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ssq => ssq.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ssq => ssq.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ssq => ssq.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ssq => ssq.deleted == false);
				}
			}
			else
			{
				query = query.Where(ssq => ssq.active == true);
				query = query.Where(ssq => ssq.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Salesforce Sync Queue, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.entityType.Contains(anyStringContains)
			       || x.operationType.Contains(anyStringContains)
			       || x.payload.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			       || x.responseBody.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.entityType).ThenBy(x => x.operationType).ThenBy(x => x.status);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.SalesforceSyncQueue.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/SalesforceSyncQueue/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
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
