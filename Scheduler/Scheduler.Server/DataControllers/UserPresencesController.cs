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
    /// This auto generated class provides the basic CRUD operations for the UserPresence entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the UserPresence entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class UserPresencesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 50;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SchedulerContext _context;

		private ILogger<UserPresencesController> _logger;

		public UserPresencesController(SchedulerContext context, ILogger<UserPresencesController> logger) : base("Scheduler", "UserPresence")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of UserPresences filtered by the parameters provided.
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
		[Route("api/UserPresences")]
		public async Task<IActionResult> GetUserPresences(
			int? userId = null,
			string status = null,
			string customStatusMessage = null,
			DateTime? lastSeenDateTime = null,
			DateTime? lastActivityDateTime = null,
			int? connectionCount = null,
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
			if (lastSeenDateTime.HasValue == true && lastSeenDateTime.Value.Kind != DateTimeKind.Utc)
			{
				lastSeenDateTime = lastSeenDateTime.Value.ToUniversalTime();
			}

			if (lastActivityDateTime.HasValue == true && lastActivityDateTime.Value.Kind != DateTimeKind.Utc)
			{
				lastActivityDateTime = lastActivityDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.UserPresence> query = (from up in _context.UserPresences select up);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userId.HasValue == true)
			{
				query = query.Where(up => up.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(up => up.status == status);
			}
			if (string.IsNullOrEmpty(customStatusMessage) == false)
			{
				query = query.Where(up => up.customStatusMessage == customStatusMessage);
			}
			if (lastSeenDateTime.HasValue == true)
			{
				query = query.Where(up => up.lastSeenDateTime == lastSeenDateTime.Value);
			}
			if (lastActivityDateTime.HasValue == true)
			{
				query = query.Where(up => up.lastActivityDateTime == lastActivityDateTime.Value);
			}
			if (connectionCount.HasValue == true)
			{
				query = query.Where(up => up.connectionCount == connectionCount.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(up => up.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(up => up.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(up => up.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(up => up.deleted == false);
				}
			}
			else
			{
				query = query.Where(up => up.active == true);
				query = query.Where(up => up.deleted == false);
			}

			query = query.OrderBy(up => up.status).ThenBy(up => up.customStatusMessage);


			//
			// Add the any string contains parameter to span all the string fields on the User Presence, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.status.Contains(anyStringContains)
			       || x.customStatusMessage.Contains(anyStringContains)
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
			
			List<Database.UserPresence> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.UserPresence userPresence in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(userPresence, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.UserPresence Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.UserPresence Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of UserPresences filtered by the parameters provided.  Its query is similar to the GetUserPresences method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserPresences/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? userId = null,
			string status = null,
			string customStatusMessage = null,
			DateTime? lastSeenDateTime = null,
			DateTime? lastActivityDateTime = null,
			int? connectionCount = null,
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
			if (lastSeenDateTime.HasValue == true && lastSeenDateTime.Value.Kind != DateTimeKind.Utc)
			{
				lastSeenDateTime = lastSeenDateTime.Value.ToUniversalTime();
			}

			if (lastActivityDateTime.HasValue == true && lastActivityDateTime.Value.Kind != DateTimeKind.Utc)
			{
				lastActivityDateTime = lastActivityDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.UserPresence> query = (from up in _context.UserPresences select up);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (userId.HasValue == true)
			{
				query = query.Where(up => up.userId == userId.Value);
			}
			if (status != null)
			{
				query = query.Where(up => up.status == status);
			}
			if (customStatusMessage != null)
			{
				query = query.Where(up => up.customStatusMessage == customStatusMessage);
			}
			if (lastSeenDateTime.HasValue == true)
			{
				query = query.Where(up => up.lastSeenDateTime == lastSeenDateTime.Value);
			}
			if (lastActivityDateTime.HasValue == true)
			{
				query = query.Where(up => up.lastActivityDateTime == lastActivityDateTime.Value);
			}
			if (connectionCount.HasValue == true)
			{
				query = query.Where(up => up.connectionCount == connectionCount.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(up => up.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(up => up.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(up => up.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(up => up.deleted == false);
				}
			}
			else
			{
				query = query.Where(up => up.active == true);
				query = query.Where(up => up.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the User Presence, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.status.Contains(anyStringContains)
			       || x.customStatusMessage.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single UserPresence by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserPresence/{id}")]
		public async Task<IActionResult> GetUserPresence(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.UserPresence> query = (from up in _context.UserPresences where
							(up.id == id) &&
							(userIsAdmin == true || up.deleted == false) &&
							(userIsWriter == true || up.active == true)
					select up);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.UserPresence materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.UserPresence Entity was read with Admin privilege." : "Scheduler.UserPresence Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserPresence", materialized.id, materialized.status));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.UserPresence entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.UserPresence.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.UserPresence.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing UserPresence record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/UserPresence/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutUserPresence(int id, [FromBody]Database.UserPresence.UserPresenceDTO userPresenceDTO, CancellationToken cancellationToken = default)
		{
			if (userPresenceDTO == null)
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



			if (id != userPresenceDTO.id)
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


			IQueryable<Database.UserPresence> query = (from x in _context.UserPresences
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserPresence existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.UserPresence PUT", id.ToString(), new Exception("No Scheduler.UserPresence entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (userPresenceDTO.objectGuid == Guid.Empty)
            {
                userPresenceDTO.objectGuid = existing.objectGuid;
            }
            else if (userPresenceDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a UserPresence record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.UserPresence cloneOfExisting = (Database.UserPresence)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new UserPresence object using the data from the existing record, updated with what is in the DTO.
			//
			Database.UserPresence userPresence = (Database.UserPresence)_context.Entry(existing).GetDatabaseValues().ToObject();
			userPresence.ApplyDTO(userPresenceDTO);
			//
			// The tenant guid for any UserPresence being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the UserPresence because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				userPresence.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (userPresence.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.UserPresence record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (userPresence.status != null && userPresence.status.Length > 50)
			{
				userPresence.status = userPresence.status.Substring(0, 50);
			}

			if (userPresence.customStatusMessage != null && userPresence.customStatusMessage.Length > 250)
			{
				userPresence.customStatusMessage = userPresence.customStatusMessage.Substring(0, 250);
			}

			if (userPresence.lastSeenDateTime.Kind != DateTimeKind.Utc)
			{
				userPresence.lastSeenDateTime = userPresence.lastSeenDateTime.ToUniversalTime();
			}

			if (userPresence.lastActivityDateTime.Kind != DateTimeKind.Utc)
			{
				userPresence.lastActivityDateTime = userPresence.lastActivityDateTime.ToUniversalTime();
			}

			EntityEntry<Database.UserPresence> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(userPresence);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.UserPresence entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserPresence.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserPresence.CreateAnonymousWithFirstLevelSubObjects(userPresence)),
					null);


				return Ok(Database.UserPresence.CreateAnonymous(userPresence));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.UserPresence entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserPresence.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserPresence.CreateAnonymousWithFirstLevelSubObjects(userPresence)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new UserPresence record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserPresence", Name = "UserPresence")]
		public async Task<IActionResult> PostUserPresence([FromBody]Database.UserPresence.UserPresenceDTO userPresenceDTO, CancellationToken cancellationToken = default)
		{
			if (userPresenceDTO == null)
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
			// Create a new UserPresence object using the data from the DTO
			//
			Database.UserPresence userPresence = Database.UserPresence.FromDTO(userPresenceDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				userPresence.tenantGuid = userTenantGuid;

				if (userPresence.status != null && userPresence.status.Length > 50)
				{
					userPresence.status = userPresence.status.Substring(0, 50);
				}

				if (userPresence.customStatusMessage != null && userPresence.customStatusMessage.Length > 250)
				{
					userPresence.customStatusMessage = userPresence.customStatusMessage.Substring(0, 250);
				}

				if (userPresence.lastSeenDateTime.Kind != DateTimeKind.Utc)
				{
					userPresence.lastSeenDateTime = userPresence.lastSeenDateTime.ToUniversalTime();
				}

				if (userPresence.lastActivityDateTime.Kind != DateTimeKind.Utc)
				{
					userPresence.lastActivityDateTime = userPresence.lastActivityDateTime.ToUniversalTime();
				}

				userPresence.objectGuid = Guid.NewGuid();
				_context.UserPresences.Add(userPresence);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.UserPresence entity successfully created.",
					true,
					userPresence.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.UserPresence.CreateAnonymousWithFirstLevelSubObjects(userPresence)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.UserPresence entity creation failed.", false, userPresence.id.ToString(), "", JsonSerializer.Serialize(userPresence), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserPresence", userPresence.id, userPresence.status));

			return CreatedAtRoute("UserPresence", new { id = userPresence.id }, Database.UserPresence.CreateAnonymousWithFirstLevelSubObjects(userPresence));
		}



        /// <summary>
        /// 
        /// This deletes a UserPresence record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserPresence/{id}")]
		[Route("api/UserPresence")]
		public async Task<IActionResult> DeleteUserPresence(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.UserPresence> query = (from x in _context.UserPresences
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserPresence userPresence = await query.FirstOrDefaultAsync(cancellationToken);

			if (userPresence == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.UserPresence DELETE", id.ToString(), new Exception("No Scheduler.UserPresence entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.UserPresence cloneOfExisting = (Database.UserPresence)_context.Entry(userPresence).GetDatabaseValues().ToObject();


			try
			{
				userPresence.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.UserPresence entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserPresence.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserPresence.CreateAnonymousWithFirstLevelSubObjects(userPresence)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.UserPresence entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserPresence.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserPresence.CreateAnonymousWithFirstLevelSubObjects(userPresence)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of UserPresence records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/UserPresences/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? userId = null,
			string status = null,
			string customStatusMessage = null,
			DateTime? lastSeenDateTime = null,
			DateTime? lastActivityDateTime = null,
			int? connectionCount = null,
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
			if (lastSeenDateTime.HasValue == true && lastSeenDateTime.Value.Kind != DateTimeKind.Utc)
			{
				lastSeenDateTime = lastSeenDateTime.Value.ToUniversalTime();
			}

			if (lastActivityDateTime.HasValue == true && lastActivityDateTime.Value.Kind != DateTimeKind.Utc)
			{
				lastActivityDateTime = lastActivityDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.UserPresence> query = (from up in _context.UserPresences select up);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userId.HasValue == true)
			{
				query = query.Where(up => up.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(up => up.status == status);
			}
			if (string.IsNullOrEmpty(customStatusMessage) == false)
			{
				query = query.Where(up => up.customStatusMessage == customStatusMessage);
			}
			if (lastSeenDateTime.HasValue == true)
			{
				query = query.Where(up => up.lastSeenDateTime == lastSeenDateTime.Value);
			}
			if (lastActivityDateTime.HasValue == true)
			{
				query = query.Where(up => up.lastActivityDateTime == lastActivityDateTime.Value);
			}
			if (connectionCount.HasValue == true)
			{
				query = query.Where(up => up.connectionCount == connectionCount.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(up => up.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(up => up.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(up => up.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(up => up.deleted == false);
				}
			}
			else
			{
				query = query.Where(up => up.active == true);
				query = query.Where(up => up.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Presence, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.status.Contains(anyStringContains)
			       || x.customStatusMessage.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.status).ThenBy(x => x.customStatusMessage);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.UserPresence.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/UserPresence/CreateAuditEvent")]
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
