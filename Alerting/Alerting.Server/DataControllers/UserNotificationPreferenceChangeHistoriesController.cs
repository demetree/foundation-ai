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
    /// This auto generated class provides the basic CRUD operations for the UserNotificationPreferenceChangeHistory entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the UserNotificationPreferenceChangeHistory entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class UserNotificationPreferenceChangeHistoriesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 10;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private AlertingContext _context;

		private ILogger<UserNotificationPreferenceChangeHistoriesController> _logger;

		public UserNotificationPreferenceChangeHistoriesController(AlertingContext context, ILogger<UserNotificationPreferenceChangeHistoriesController> logger) : base("Alerting", "UserNotificationPreferenceChangeHistory")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of UserNotificationPreferenceChangeHistories filtered by the parameters provided.
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
		[Route("api/UserNotificationPreferenceChangeHistories")]
		public async Task<IActionResult> GetUserNotificationPreferenceChangeHistories(
			int? userNotificationPreferenceId = null,
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

			IQueryable<Database.UserNotificationPreferenceChangeHistory> query = (from unpch in _context.UserNotificationPreferenceChangeHistories select unpch);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userNotificationPreferenceId.HasValue == true)
			{
				query = query.Where(unpch => unpch.userNotificationPreferenceId == userNotificationPreferenceId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(unpch => unpch.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(unpch => unpch.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(unpch => unpch.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(data) == false)
			{
				query = query.Where(unpch => unpch.data == data);
			}

			query = query.OrderByDescending(unpch => unpch.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.userNotificationPreference);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Notification Preference Change History, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.data.Contains(anyStringContains)
			       || (includeRelations == true && x.userNotificationPreference.timeZoneId.Contains(anyStringContains))
			       || (includeRelations == true && x.userNotificationPreference.quietHoursStart.Contains(anyStringContains))
			       || (includeRelations == true && x.userNotificationPreference.quietHoursEnd.Contains(anyStringContains))
			       || (includeRelations == true && x.userNotificationPreference.customSettingsJson.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.UserNotificationPreferenceChangeHistory> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.UserNotificationPreferenceChangeHistory userNotificationPreferenceChangeHistory in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(userNotificationPreferenceChangeHistory, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Alerting.UserNotificationPreferenceChangeHistory Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Alerting.UserNotificationPreferenceChangeHistory Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of UserNotificationPreferenceChangeHistories filtered by the parameters provided.  Its query is similar to the GetUserNotificationPreferenceChangeHistories method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationPreferenceChangeHistories/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? userNotificationPreferenceId = null,
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

			IQueryable<Database.UserNotificationPreferenceChangeHistory> query = (from unpch in _context.UserNotificationPreferenceChangeHistories select unpch);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (userNotificationPreferenceId.HasValue == true)
			{
				query = query.Where(unpch => unpch.userNotificationPreferenceId == userNotificationPreferenceId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(unpch => unpch.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(unpch => unpch.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(unpch => unpch.userId == userId.Value);
			}
			if (data != null)
			{
				query = query.Where(unpch => unpch.data == data);
			}

			//
			// Add the any string contains parameter to span all the string fields on the User Notification Preference Change History, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.data.Contains(anyStringContains)
			       || x.userNotificationPreference.timeZoneId.Contains(anyStringContains)
			       || x.userNotificationPreference.quietHoursStart.Contains(anyStringContains)
			       || x.userNotificationPreference.quietHoursEnd.Contains(anyStringContains)
			       || x.userNotificationPreference.customSettingsJson.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single UserNotificationPreferenceChangeHistory by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationPreferenceChangeHistory/{id}")]
		public async Task<IActionResult> GetUserNotificationPreferenceChangeHistory(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.UserNotificationPreferenceChangeHistory> query = (from unpch in _context.UserNotificationPreferenceChangeHistories where
				(unpch.id == id)
					select unpch);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.userNotificationPreference);
					query = query.AsSplitQuery();
				}

				Database.UserNotificationPreferenceChangeHistory materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Alerting.UserNotificationPreferenceChangeHistory Entity was read with Admin privilege." : "Alerting.UserNotificationPreferenceChangeHistory Entity was read.");

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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Alerting.UserNotificationPreferenceChangeHistory entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Alerting.UserNotificationPreferenceChangeHistory.   Entity was read with Admin privilege." : "Exception caught during entity read of Alerting.UserNotificationPreferenceChangeHistory.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing UserNotificationPreferenceChangeHistory record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/UserNotificationPreferenceChangeHistory/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutUserNotificationPreferenceChangeHistory(int id, [FromBody]Database.UserNotificationPreferenceChangeHistory.UserNotificationPreferenceChangeHistoryDTO userNotificationPreferenceChangeHistoryDTO, CancellationToken cancellationToken = default)
		{
			if (userNotificationPreferenceChangeHistoryDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			// Admin privilege needed to write to this table.
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != userNotificationPreferenceChangeHistoryDTO.id)
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


			IQueryable<Database.UserNotificationPreferenceChangeHistory> query = (from x in _context.UserNotificationPreferenceChangeHistories
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserNotificationPreferenceChangeHistory existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.UserNotificationPreferenceChangeHistory PUT", id.ToString(), new Exception("No Alerting.UserNotificationPreferenceChangeHistory entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.UserNotificationPreferenceChangeHistory cloneOfExisting = (Database.UserNotificationPreferenceChangeHistory)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new UserNotificationPreferenceChangeHistory object using the data from the existing record, updated with what is in the DTO.
			//
			Database.UserNotificationPreferenceChangeHistory userNotificationPreferenceChangeHistory = (Database.UserNotificationPreferenceChangeHistory)_context.Entry(existing).GetDatabaseValues().ToObject();
			userNotificationPreferenceChangeHistory.ApplyDTO(userNotificationPreferenceChangeHistoryDTO);
			//
			// The tenant guid for any UserNotificationPreferenceChangeHistory being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the UserNotificationPreferenceChangeHistory because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				userNotificationPreferenceChangeHistory.tenantGuid = existing.tenantGuid;
			}



			if (userNotificationPreferenceChangeHistory.timeStamp.Kind != DateTimeKind.Utc)
			{
				userNotificationPreferenceChangeHistory.timeStamp = userNotificationPreferenceChangeHistory.timeStamp.ToUniversalTime();
			}

			EntityEntry<Database.UserNotificationPreferenceChangeHistory> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(userNotificationPreferenceChangeHistory);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Alerting.UserNotificationPreferenceChangeHistory entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserNotificationPreferenceChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserNotificationPreferenceChangeHistory.CreateAnonymousWithFirstLevelSubObjects(userNotificationPreferenceChangeHistory)),
					null);


				return Ok(Database.UserNotificationPreferenceChangeHistory.CreateAnonymous(userNotificationPreferenceChangeHistory));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Alerting.UserNotificationPreferenceChangeHistory entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserNotificationPreferenceChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserNotificationPreferenceChangeHistory.CreateAnonymousWithFirstLevelSubObjects(userNotificationPreferenceChangeHistory)),
					ex);

				return Problem(ex.Message);
			}

		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new UserNotificationPreferenceChangeHistory record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationPreferenceChangeHistory", Name = "UserNotificationPreferenceChangeHistory")]
		public async Task<IActionResult> PostUserNotificationPreferenceChangeHistory([FromBody]Database.UserNotificationPreferenceChangeHistory.UserNotificationPreferenceChangeHistoryDTO userNotificationPreferenceChangeHistoryDTO, CancellationToken cancellationToken = default)
		{
			if (userNotificationPreferenceChangeHistoryDTO == null)
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
			// Create a new UserNotificationPreferenceChangeHistory object using the data from the DTO
			//
			Database.UserNotificationPreferenceChangeHistory userNotificationPreferenceChangeHistory = Database.UserNotificationPreferenceChangeHistory.FromDTO(userNotificationPreferenceChangeHistoryDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				userNotificationPreferenceChangeHistory.tenantGuid = userTenantGuid;

				if (userNotificationPreferenceChangeHistory.timeStamp.Kind != DateTimeKind.Utc)
				{
					userNotificationPreferenceChangeHistory.timeStamp = userNotificationPreferenceChangeHistory.timeStamp.ToUniversalTime();
				}

				_context.UserNotificationPreferenceChangeHistories.Add(userNotificationPreferenceChangeHistory);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Alerting.UserNotificationPreferenceChangeHistory entity successfully created.",
					true,
					userNotificationPreferenceChangeHistory.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.UserNotificationPreferenceChangeHistory.CreateAnonymousWithFirstLevelSubObjects(userNotificationPreferenceChangeHistory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Alerting.UserNotificationPreferenceChangeHistory entity creation failed.", false, userNotificationPreferenceChangeHistory.id.ToString(), "", JsonSerializer.Serialize(userNotificationPreferenceChangeHistory), ex);

				return Problem(ex.Message);
			}


			return CreatedAtRoute("UserNotificationPreferenceChangeHistory", new { id = userNotificationPreferenceChangeHistory.id }, Database.UserNotificationPreferenceChangeHistory.CreateAnonymousWithFirstLevelSubObjects(userNotificationPreferenceChangeHistory));
		}

*/


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a UserNotificationPreferenceChangeHistory record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationPreferenceChangeHistory/{id}")]
		[Route("api/UserNotificationPreferenceChangeHistory")]
		public async Task<IActionResult> DeleteUserNotificationPreferenceChangeHistory(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.UserNotificationPreferenceChangeHistory> query = (from x in _context.UserNotificationPreferenceChangeHistories
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserNotificationPreferenceChangeHistory userNotificationPreferenceChangeHistory = await query.FirstOrDefaultAsync(cancellationToken);

			if (userNotificationPreferenceChangeHistory == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.UserNotificationPreferenceChangeHistory DELETE", id.ToString(), new Exception("No Alerting.UserNotificationPreferenceChangeHistory entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.UserNotificationPreferenceChangeHistory cloneOfExisting = (Database.UserNotificationPreferenceChangeHistory)_context.Entry(userNotificationPreferenceChangeHistory).GetDatabaseValues().ToObject();


			try
			{
				_context.UserNotificationPreferenceChangeHistories.Remove(userNotificationPreferenceChangeHistory);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Alerting.UserNotificationPreferenceChangeHistory entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserNotificationPreferenceChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserNotificationPreferenceChangeHistory.CreateAnonymousWithFirstLevelSubObjects(userNotificationPreferenceChangeHistory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Alerting.UserNotificationPreferenceChangeHistory entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserNotificationPreferenceChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserNotificationPreferenceChangeHistory.CreateAnonymousWithFirstLevelSubObjects(userNotificationPreferenceChangeHistory)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


*/
        /// <summary>
        /// 
        /// This gets a list of UserNotificationPreferenceChangeHistory records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/UserNotificationPreferenceChangeHistories/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? userNotificationPreferenceId = null,
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

			IQueryable<Database.UserNotificationPreferenceChangeHistory> query = (from unpch in _context.UserNotificationPreferenceChangeHistories select unpch);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userNotificationPreferenceId.HasValue == true)
			{
				query = query.Where(unpch => unpch.userNotificationPreferenceId == userNotificationPreferenceId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(unpch => unpch.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(unpch => unpch.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(unpch => unpch.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(data) == false)
			{
				query = query.Where(unpch => unpch.data == data);
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Notification Preference Change History, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.data.Contains(anyStringContains)
			       || x.userNotificationPreference.timeZoneId.Contains(anyStringContains)
			       || x.userNotificationPreference.quietHoursStart.Contains(anyStringContains)
			       || x.userNotificationPreference.quietHoursEnd.Contains(anyStringContains)
			       || x.userNotificationPreference.customSettingsJson.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderByDescending (x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.UserNotificationPreferenceChangeHistory.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/UserNotificationPreferenceChangeHistory/CreateAuditEvent")]
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
