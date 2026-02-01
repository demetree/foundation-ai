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
    /// This auto generated class provides the basic CRUD operations for the UserNotificationChannelPreferenceChangeHistory entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the UserNotificationChannelPreferenceChangeHistory entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class UserNotificationChannelPreferenceChangeHistoriesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 10;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private AlertingContext _context;

		private ILogger<UserNotificationChannelPreferenceChangeHistoriesController> _logger;

		public UserNotificationChannelPreferenceChangeHistoriesController(AlertingContext context, ILogger<UserNotificationChannelPreferenceChangeHistoriesController> logger) : base("Alerting", "UserNotificationChannelPreferenceChangeHistory")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of UserNotificationChannelPreferenceChangeHistories filtered by the parameters provided.
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
		[Route("api/UserNotificationChannelPreferenceChangeHistories")]
		public async Task<IActionResult> GetUserNotificationChannelPreferenceChangeHistories(
			int? userNotificationChannelPreferenceId = null,
			int? versionNumber = null,
			DateTime? timeStamp = null,
			int? userId = null,
			string data = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
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
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.UserNotificationChannelPreferenceChangeHistory> query = (from uncpch in _context.UserNotificationChannelPreferenceChangeHistories select uncpch);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userNotificationChannelPreferenceId.HasValue == true)
			{
				query = query.Where(uncpch => uncpch.userNotificationChannelPreferenceId == userNotificationChannelPreferenceId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(uncpch => uncpch.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(uncpch => uncpch.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(uncpch => uncpch.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(data) == false)
			{
				query = query.Where(uncpch => uncpch.data == data);
			}

			query = query.OrderByDescending(uncpch => uncpch.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.userNotificationChannelPreference);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Notification Channel Preference Change History, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.data.Contains(anyStringContains)
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.UserNotificationChannelPreferenceChangeHistory> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.UserNotificationChannelPreferenceChangeHistory userNotificationChannelPreferenceChangeHistory in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(userNotificationChannelPreferenceChangeHistory, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Alerting.UserNotificationChannelPreferenceChangeHistory Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Alerting.UserNotificationChannelPreferenceChangeHistory Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of UserNotificationChannelPreferenceChangeHistories filtered by the parameters provided.  Its query is similar to the GetUserNotificationChannelPreferenceChangeHistories method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationChannelPreferenceChangeHistories/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? userNotificationChannelPreferenceId = null,
			int? versionNumber = null,
			DateTime? timeStamp = null,
			int? userId = null,
			string data = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
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
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.UserNotificationChannelPreferenceChangeHistory> query = (from uncpch in _context.UserNotificationChannelPreferenceChangeHistories select uncpch);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (userNotificationChannelPreferenceId.HasValue == true)
			{
				query = query.Where(uncpch => uncpch.userNotificationChannelPreferenceId == userNotificationChannelPreferenceId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(uncpch => uncpch.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(uncpch => uncpch.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(uncpch => uncpch.userId == userId.Value);
			}
			if (data != null)
			{
				query = query.Where(uncpch => uncpch.data == data);
			}

			//
			// Add the any string contains parameter to span all the string fields on the User Notification Channel Preference Change History, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.data.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single UserNotificationChannelPreferenceChangeHistory by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationChannelPreferenceChangeHistory/{id}")]
		public async Task<IActionResult> GetUserNotificationChannelPreferenceChangeHistory(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
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
				IQueryable<Database.UserNotificationChannelPreferenceChangeHistory> query = (from uncpch in _context.UserNotificationChannelPreferenceChangeHistories where
				(uncpch.id == id)
					select uncpch);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.userNotificationChannelPreference);
					query = query.AsSplitQuery();
				}

				Database.UserNotificationChannelPreferenceChangeHistory materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Alerting.UserNotificationChannelPreferenceChangeHistory Entity was read with Admin privilege." : "Alerting.UserNotificationChannelPreferenceChangeHistory Entity was read.");

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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Alerting.UserNotificationChannelPreferenceChangeHistory entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Alerting.UserNotificationChannelPreferenceChangeHistory.   Entity was read with Admin privilege." : "Exception caught during entity read of Alerting.UserNotificationChannelPreferenceChangeHistory.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing UserNotificationChannelPreferenceChangeHistory record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/UserNotificationChannelPreferenceChangeHistory/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutUserNotificationChannelPreferenceChangeHistory(int id, [FromBody]Database.UserNotificationChannelPreferenceChangeHistory.UserNotificationChannelPreferenceChangeHistoryDTO userNotificationChannelPreferenceChangeHistoryDTO, CancellationToken cancellationToken = default)
		{
			if (userNotificationChannelPreferenceChangeHistoryDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			// Admin privilege needed to write to this table.
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != userNotificationChannelPreferenceChangeHistoryDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
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


			IQueryable<Database.UserNotificationChannelPreferenceChangeHistory> query = (from x in _context.UserNotificationChannelPreferenceChangeHistories
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserNotificationChannelPreferenceChangeHistory existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.UserNotificationChannelPreferenceChangeHistory PUT", id.ToString(), new Exception("No Alerting.UserNotificationChannelPreferenceChangeHistory entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.UserNotificationChannelPreferenceChangeHistory cloneOfExisting = (Database.UserNotificationChannelPreferenceChangeHistory)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new UserNotificationChannelPreferenceChangeHistory object using the data from the existing record, updated with what is in the DTO.
			//
			Database.UserNotificationChannelPreferenceChangeHistory userNotificationChannelPreferenceChangeHistory = (Database.UserNotificationChannelPreferenceChangeHistory)_context.Entry(existing).GetDatabaseValues().ToObject();
			userNotificationChannelPreferenceChangeHistory.ApplyDTO(userNotificationChannelPreferenceChangeHistoryDTO);
			//
			// The tenant guid for any UserNotificationChannelPreferenceChangeHistory being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the UserNotificationChannelPreferenceChangeHistory because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				userNotificationChannelPreferenceChangeHistory.tenantGuid = existing.tenantGuid;
			}



			if (userNotificationChannelPreferenceChangeHistory.timeStamp.Kind != DateTimeKind.Utc)
			{
				userNotificationChannelPreferenceChangeHistory.timeStamp = userNotificationChannelPreferenceChangeHistory.timeStamp.ToUniversalTime();
			}

			EntityEntry<Database.UserNotificationChannelPreferenceChangeHistory> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(userNotificationChannelPreferenceChangeHistory);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Alerting.UserNotificationChannelPreferenceChangeHistory entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserNotificationChannelPreferenceChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserNotificationChannelPreferenceChangeHistory.CreateAnonymousWithFirstLevelSubObjects(userNotificationChannelPreferenceChangeHistory)),
					null);


				return Ok(Database.UserNotificationChannelPreferenceChangeHistory.CreateAnonymous(userNotificationChannelPreferenceChangeHistory));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Alerting.UserNotificationChannelPreferenceChangeHistory entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserNotificationChannelPreferenceChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserNotificationChannelPreferenceChangeHistory.CreateAnonymousWithFirstLevelSubObjects(userNotificationChannelPreferenceChangeHistory)),
					ex);

				return Problem(ex.Message);
			}

		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new UserNotificationChannelPreferenceChangeHistory record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationChannelPreferenceChangeHistory", Name = "UserNotificationChannelPreferenceChangeHistory")]
		public async Task<IActionResult> PostUserNotificationChannelPreferenceChangeHistory([FromBody]Database.UserNotificationChannelPreferenceChangeHistory.UserNotificationChannelPreferenceChangeHistoryDTO userNotificationChannelPreferenceChangeHistoryDTO, CancellationToken cancellationToken = default)
		{
			if (userNotificationChannelPreferenceChangeHistoryDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			// Admin privilege needed to write to this table.
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

			//
			// Create a new UserNotificationChannelPreferenceChangeHistory object using the data from the DTO
			//
			Database.UserNotificationChannelPreferenceChangeHistory userNotificationChannelPreferenceChangeHistory = Database.UserNotificationChannelPreferenceChangeHistory.FromDTO(userNotificationChannelPreferenceChangeHistoryDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				userNotificationChannelPreferenceChangeHistory.tenantGuid = userTenantGuid;

				if (userNotificationChannelPreferenceChangeHistory.timeStamp.Kind != DateTimeKind.Utc)
				{
					userNotificationChannelPreferenceChangeHistory.timeStamp = userNotificationChannelPreferenceChangeHistory.timeStamp.ToUniversalTime();
				}

				_context.UserNotificationChannelPreferenceChangeHistories.Add(userNotificationChannelPreferenceChangeHistory);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Alerting.UserNotificationChannelPreferenceChangeHistory entity successfully created.",
					true,
					userNotificationChannelPreferenceChangeHistory.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.UserNotificationChannelPreferenceChangeHistory.CreateAnonymousWithFirstLevelSubObjects(userNotificationChannelPreferenceChangeHistory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Alerting.UserNotificationChannelPreferenceChangeHistory entity creation failed.", false, userNotificationChannelPreferenceChangeHistory.id.ToString(), "", JsonSerializer.Serialize(userNotificationChannelPreferenceChangeHistory), ex);

				return Problem(ex.Message);
			}


			return CreatedAtRoute("UserNotificationChannelPreferenceChangeHistory", new { id = userNotificationChannelPreferenceChangeHistory.id }, Database.UserNotificationChannelPreferenceChangeHistory.CreateAnonymousWithFirstLevelSubObjects(userNotificationChannelPreferenceChangeHistory));
		}

*/


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a UserNotificationChannelPreferenceChangeHistory record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationChannelPreferenceChangeHistory/{id}")]
		[Route("api/UserNotificationChannelPreferenceChangeHistory")]
		public async Task<IActionResult> DeleteUserNotificationChannelPreferenceChangeHistory(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.UserNotificationChannelPreferenceChangeHistory> query = (from x in _context.UserNotificationChannelPreferenceChangeHistories
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserNotificationChannelPreferenceChangeHistory userNotificationChannelPreferenceChangeHistory = await query.FirstOrDefaultAsync(cancellationToken);

			if (userNotificationChannelPreferenceChangeHistory == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.UserNotificationChannelPreferenceChangeHistory DELETE", id.ToString(), new Exception("No Alerting.UserNotificationChannelPreferenceChangeHistory entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.UserNotificationChannelPreferenceChangeHistory cloneOfExisting = (Database.UserNotificationChannelPreferenceChangeHistory)_context.Entry(userNotificationChannelPreferenceChangeHistory).GetDatabaseValues().ToObject();


			try
			{
				_context.UserNotificationChannelPreferenceChangeHistories.Remove(userNotificationChannelPreferenceChangeHistory);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Alerting.UserNotificationChannelPreferenceChangeHistory entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserNotificationChannelPreferenceChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserNotificationChannelPreferenceChangeHistory.CreateAnonymousWithFirstLevelSubObjects(userNotificationChannelPreferenceChangeHistory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Alerting.UserNotificationChannelPreferenceChangeHistory entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserNotificationChannelPreferenceChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserNotificationChannelPreferenceChangeHistory.CreateAnonymousWithFirstLevelSubObjects(userNotificationChannelPreferenceChangeHistory)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


*/
        /// <summary>
        /// 
        /// This gets a list of UserNotificationChannelPreferenceChangeHistory records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/UserNotificationChannelPreferenceChangeHistories/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? userNotificationChannelPreferenceId = null,
			int? versionNumber = null,
			DateTime? timeStamp = null,
			int? userId = null,
			string data = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);

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
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.UserNotificationChannelPreferenceChangeHistory> query = (from uncpch in _context.UserNotificationChannelPreferenceChangeHistories select uncpch);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userNotificationChannelPreferenceId.HasValue == true)
			{
				query = query.Where(uncpch => uncpch.userNotificationChannelPreferenceId == userNotificationChannelPreferenceId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(uncpch => uncpch.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(uncpch => uncpch.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(uncpch => uncpch.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(data) == false)
			{
				query = query.Where(uncpch => uncpch.data == data);
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Notification Channel Preference Change History, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.data.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderByDescending (x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.UserNotificationChannelPreferenceChangeHistory.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/UserNotificationChannelPreferenceChangeHistory/CreateAuditEvent")]
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
