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
    /// This auto generated class provides the basic CRUD operations for the MessagingAuditLog entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the MessagingAuditLog entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class MessagingAuditLogsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 50;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 100;

		private SchedulerContext _context;

		private ILogger<MessagingAuditLogsController> _logger;

		public MessagingAuditLogsController(SchedulerContext context, ILogger<MessagingAuditLogsController> logger) : base("Scheduler", "MessagingAuditLog")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of MessagingAuditLogs filtered by the parameters provided.
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
		[Route("api/MessagingAuditLogs")]
		public async Task<IActionResult> GetMessagingAuditLogs(
			int? performedByUserId = null,
			string action = null,
			string entityType = null,
			int? entityId = null,
			string details = null,
			string ipAddress = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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

			IQueryable<Database.MessagingAuditLog> query = (from mal in _context.MessagingAuditLogs select mal);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (performedByUserId.HasValue == true)
			{
				query = query.Where(mal => mal.performedByUserId == performedByUserId.Value);
			}
			if (string.IsNullOrEmpty(action) == false)
			{
				query = query.Where(mal => mal.action == action);
			}
			if (string.IsNullOrEmpty(entityType) == false)
			{
				query = query.Where(mal => mal.entityType == entityType);
			}
			if (entityId.HasValue == true)
			{
				query = query.Where(mal => mal.entityId == entityId.Value);
			}
			if (string.IsNullOrEmpty(details) == false)
			{
				query = query.Where(mal => mal.details == details);
			}
			if (string.IsNullOrEmpty(ipAddress) == false)
			{
				query = query.Where(mal => mal.ipAddress == ipAddress);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(mal => mal.dateTimeCreated == dateTimeCreated.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mal => mal.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mal => mal.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mal => mal.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mal => mal.deleted == false);
				}
			}
			else
			{
				query = query.Where(mal => mal.active == true);
				query = query.Where(mal => mal.deleted == false);
			}

			query = query.OrderBy(mal => mal.action).ThenBy(mal => mal.entityType).ThenBy(mal => mal.ipAddress);


			//
			// Add the any string contains parameter to span all the string fields on the Messaging Audit Log, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.action.Contains(anyStringContains)
			       || x.entityType.Contains(anyStringContains)
			       || x.details.Contains(anyStringContains)
			       || x.ipAddress.Contains(anyStringContains)
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
			
			List<Database.MessagingAuditLog> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.MessagingAuditLog messagingAuditLog in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(messagingAuditLog, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.MessagingAuditLog Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.MessagingAuditLog Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of MessagingAuditLogs filtered by the parameters provided.  Its query is similar to the GetMessagingAuditLogs method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MessagingAuditLogs/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? performedByUserId = null,
			string action = null,
			string entityType = null,
			int? entityId = null,
			string details = null,
			string ipAddress = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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

			IQueryable<Database.MessagingAuditLog> query = (from mal in _context.MessagingAuditLogs select mal);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (performedByUserId.HasValue == true)
			{
				query = query.Where(mal => mal.performedByUserId == performedByUserId.Value);
			}
			if (action != null)
			{
				query = query.Where(mal => mal.action == action);
			}
			if (entityType != null)
			{
				query = query.Where(mal => mal.entityType == entityType);
			}
			if (entityId.HasValue == true)
			{
				query = query.Where(mal => mal.entityId == entityId.Value);
			}
			if (details != null)
			{
				query = query.Where(mal => mal.details == details);
			}
			if (ipAddress != null)
			{
				query = query.Where(mal => mal.ipAddress == ipAddress);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(mal => mal.dateTimeCreated == dateTimeCreated.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mal => mal.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mal => mal.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mal => mal.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mal => mal.deleted == false);
				}
			}
			else
			{
				query = query.Where(mal => mal.active == true);
				query = query.Where(mal => mal.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Messaging Audit Log, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.action.Contains(anyStringContains)
			       || x.entityType.Contains(anyStringContains)
			       || x.details.Contains(anyStringContains)
			       || x.ipAddress.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single MessagingAuditLog by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MessagingAuditLog/{id}")]
		public async Task<IActionResult> GetMessagingAuditLog(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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
				IQueryable<Database.MessagingAuditLog> query = (from mal in _context.MessagingAuditLogs where
							(mal.id == id) &&
							(userIsAdmin == true || mal.deleted == false) &&
							(userIsWriter == true || mal.active == true)
					select mal);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.MessagingAuditLog materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.MessagingAuditLog Entity was read with Admin privilege." : "Scheduler.MessagingAuditLog Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "MessagingAuditLog", materialized.id, materialized.action));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.MessagingAuditLog entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.MessagingAuditLog.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.MessagingAuditLog.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing MessagingAuditLog record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/MessagingAuditLog/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutMessagingAuditLog(int id, [FromBody]Database.MessagingAuditLog.MessagingAuditLogDTO messagingAuditLogDTO, CancellationToken cancellationToken = default)
		{
			if (messagingAuditLogDTO == null)
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



			if (id != messagingAuditLogDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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


			IQueryable<Database.MessagingAuditLog> query = (from x in _context.MessagingAuditLogs
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.MessagingAuditLog existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.MessagingAuditLog PUT", id.ToString(), new Exception("No Scheduler.MessagingAuditLog entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (messagingAuditLogDTO.objectGuid == Guid.Empty)
            {
                messagingAuditLogDTO.objectGuid = existing.objectGuid;
            }
            else if (messagingAuditLogDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a MessagingAuditLog record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.MessagingAuditLog cloneOfExisting = (Database.MessagingAuditLog)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new MessagingAuditLog object using the data from the existing record, updated with what is in the DTO.
			//
			Database.MessagingAuditLog messagingAuditLog = (Database.MessagingAuditLog)_context.Entry(existing).GetDatabaseValues().ToObject();
			messagingAuditLog.ApplyDTO(messagingAuditLogDTO);
			//
			// The tenant guid for any MessagingAuditLog being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the MessagingAuditLog because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				messagingAuditLog.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (messagingAuditLog.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.MessagingAuditLog record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (messagingAuditLog.action != null && messagingAuditLog.action.Length > 100)
			{
				messagingAuditLog.action = messagingAuditLog.action.Substring(0, 100);
			}

			if (messagingAuditLog.entityType != null && messagingAuditLog.entityType.Length > 100)
			{
				messagingAuditLog.entityType = messagingAuditLog.entityType.Substring(0, 100);
			}

			if (messagingAuditLog.ipAddress != null && messagingAuditLog.ipAddress.Length > 50)
			{
				messagingAuditLog.ipAddress = messagingAuditLog.ipAddress.Substring(0, 50);
			}

			if (messagingAuditLog.dateTimeCreated.Kind != DateTimeKind.Utc)
			{
				messagingAuditLog.dateTimeCreated = messagingAuditLog.dateTimeCreated.ToUniversalTime();
			}

			EntityEntry<Database.MessagingAuditLog> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(messagingAuditLog);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.MessagingAuditLog entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.MessagingAuditLog.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MessagingAuditLog.CreateAnonymousWithFirstLevelSubObjects(messagingAuditLog)),
					null);


				return Ok(Database.MessagingAuditLog.CreateAnonymous(messagingAuditLog));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.MessagingAuditLog entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.MessagingAuditLog.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MessagingAuditLog.CreateAnonymousWithFirstLevelSubObjects(messagingAuditLog)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new MessagingAuditLog record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MessagingAuditLog", Name = "MessagingAuditLog")]
		public async Task<IActionResult> PostMessagingAuditLog([FromBody]Database.MessagingAuditLog.MessagingAuditLogDTO messagingAuditLogDTO, CancellationToken cancellationToken = default)
		{
			if (messagingAuditLogDTO == null)
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
			// Create a new MessagingAuditLog object using the data from the DTO
			//
			Database.MessagingAuditLog messagingAuditLog = Database.MessagingAuditLog.FromDTO(messagingAuditLogDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				messagingAuditLog.tenantGuid = userTenantGuid;

				if (messagingAuditLog.action != null && messagingAuditLog.action.Length > 100)
				{
					messagingAuditLog.action = messagingAuditLog.action.Substring(0, 100);
				}

				if (messagingAuditLog.entityType != null && messagingAuditLog.entityType.Length > 100)
				{
					messagingAuditLog.entityType = messagingAuditLog.entityType.Substring(0, 100);
				}

				if (messagingAuditLog.ipAddress != null && messagingAuditLog.ipAddress.Length > 50)
				{
					messagingAuditLog.ipAddress = messagingAuditLog.ipAddress.Substring(0, 50);
				}

				if (messagingAuditLog.dateTimeCreated.Kind != DateTimeKind.Utc)
				{
					messagingAuditLog.dateTimeCreated = messagingAuditLog.dateTimeCreated.ToUniversalTime();
				}

				messagingAuditLog.objectGuid = Guid.NewGuid();
				_context.MessagingAuditLogs.Add(messagingAuditLog);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.MessagingAuditLog entity successfully created.",
					true,
					messagingAuditLog.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.MessagingAuditLog.CreateAnonymousWithFirstLevelSubObjects(messagingAuditLog)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.MessagingAuditLog entity creation failed.", false, messagingAuditLog.id.ToString(), "", JsonSerializer.Serialize(messagingAuditLog), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "MessagingAuditLog", messagingAuditLog.id, messagingAuditLog.action));

			return CreatedAtRoute("MessagingAuditLog", new { id = messagingAuditLog.id }, Database.MessagingAuditLog.CreateAnonymousWithFirstLevelSubObjects(messagingAuditLog));
		}



        /// <summary>
        /// 
        /// This deletes a MessagingAuditLog record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MessagingAuditLog/{id}")]
		[Route("api/MessagingAuditLog")]
		public async Task<IActionResult> DeleteMessagingAuditLog(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.MessagingAuditLog> query = (from x in _context.MessagingAuditLogs
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.MessagingAuditLog messagingAuditLog = await query.FirstOrDefaultAsync(cancellationToken);

			if (messagingAuditLog == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.MessagingAuditLog DELETE", id.ToString(), new Exception("No Scheduler.MessagingAuditLog entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.MessagingAuditLog cloneOfExisting = (Database.MessagingAuditLog)_context.Entry(messagingAuditLog).GetDatabaseValues().ToObject();


			try
			{
				messagingAuditLog.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.MessagingAuditLog entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.MessagingAuditLog.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MessagingAuditLog.CreateAnonymousWithFirstLevelSubObjects(messagingAuditLog)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.MessagingAuditLog entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.MessagingAuditLog.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MessagingAuditLog.CreateAnonymousWithFirstLevelSubObjects(messagingAuditLog)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of MessagingAuditLog records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/MessagingAuditLogs/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? performedByUserId = null,
			string action = null,
			string entityType = null,
			int? entityId = null,
			string details = null,
			string ipAddress = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);


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

			IQueryable<Database.MessagingAuditLog> query = (from mal in _context.MessagingAuditLogs select mal);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (performedByUserId.HasValue == true)
			{
				query = query.Where(mal => mal.performedByUserId == performedByUserId.Value);
			}
			if (string.IsNullOrEmpty(action) == false)
			{
				query = query.Where(mal => mal.action == action);
			}
			if (string.IsNullOrEmpty(entityType) == false)
			{
				query = query.Where(mal => mal.entityType == entityType);
			}
			if (entityId.HasValue == true)
			{
				query = query.Where(mal => mal.entityId == entityId.Value);
			}
			if (string.IsNullOrEmpty(details) == false)
			{
				query = query.Where(mal => mal.details == details);
			}
			if (string.IsNullOrEmpty(ipAddress) == false)
			{
				query = query.Where(mal => mal.ipAddress == ipAddress);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(mal => mal.dateTimeCreated == dateTimeCreated.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mal => mal.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mal => mal.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mal => mal.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mal => mal.deleted == false);
				}
			}
			else
			{
				query = query.Where(mal => mal.active == true);
				query = query.Where(mal => mal.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Messaging Audit Log, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.action.Contains(anyStringContains)
			       || x.entityType.Contains(anyStringContains)
			       || x.details.Contains(anyStringContains)
			       || x.ipAddress.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.action).ThenBy(x => x.entityType).ThenBy(x => x.ipAddress);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.MessagingAuditLog.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/MessagingAuditLog/CreateAuditEvent")]
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
