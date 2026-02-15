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
    /// This auto generated class provides the basic CRUD operations for the UserProfileChangeHistory entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the UserProfileChangeHistory entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class UserProfileChangeHistoriesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 10;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<UserProfileChangeHistoriesController> _logger;

		public UserProfileChangeHistoriesController(BMCContext context, ILogger<UserProfileChangeHistoriesController> logger) : base("BMC", "UserProfileChangeHistory")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of UserProfileChangeHistories filtered by the parameters provided.
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
		[Route("api/UserProfileChangeHistories")]
		public async Task<IActionResult> GetUserProfileChangeHistories(
			int? userProfileId = null,
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
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.UserProfileChangeHistory> query = (from upch in _context.UserProfileChangeHistories select upch);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userProfileId.HasValue == true)
			{
				query = query.Where(upch => upch.userProfileId == userProfileId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(upch => upch.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(upch => upch.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(upch => upch.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(data) == false)
			{
				query = query.Where(upch => upch.data == data);
			}

			query = query.OrderByDescending(upch => upch.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.userProfile);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Profile Change History, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.data.Contains(anyStringContains)
			       || (includeRelations == true && x.userProfile.displayName.Contains(anyStringContains))
			       || (includeRelations == true && x.userProfile.bio.Contains(anyStringContains))
			       || (includeRelations == true && x.userProfile.location.Contains(anyStringContains))
			       || (includeRelations == true && x.userProfile.avatarImagePath.Contains(anyStringContains))
			       || (includeRelations == true && x.userProfile.profileBannerImagePath.Contains(anyStringContains))
			       || (includeRelations == true && x.userProfile.websiteUrl.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.UserProfileChangeHistory> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.UserProfileChangeHistory userProfileChangeHistory in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(userProfileChangeHistory, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.UserProfileChangeHistory Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.UserProfileChangeHistory Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of UserProfileChangeHistories filtered by the parameters provided.  Its query is similar to the GetUserProfileChangeHistories method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserProfileChangeHistories/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? userProfileId = null,
			int? versionNumber = null,
			DateTime? timeStamp = null,
			int? userId = null,
			string data = null,
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
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.UserProfileChangeHistory> query = (from upch in _context.UserProfileChangeHistories select upch);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (userProfileId.HasValue == true)
			{
				query = query.Where(upch => upch.userProfileId == userProfileId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(upch => upch.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(upch => upch.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(upch => upch.userId == userId.Value);
			}
			if (data != null)
			{
				query = query.Where(upch => upch.data == data);
			}

			//
			// Add the any string contains parameter to span all the string fields on the User Profile Change History, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.data.Contains(anyStringContains)
			       || x.userProfile.displayName.Contains(anyStringContains)
			       || x.userProfile.bio.Contains(anyStringContains)
			       || x.userProfile.location.Contains(anyStringContains)
			       || x.userProfile.avatarImagePath.Contains(anyStringContains)
			       || x.userProfile.profileBannerImagePath.Contains(anyStringContains)
			       || x.userProfile.websiteUrl.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single UserProfileChangeHistory by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserProfileChangeHistory/{id}")]
		public async Task<IActionResult> GetUserProfileChangeHistory(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.UserProfileChangeHistory> query = (from upch in _context.UserProfileChangeHistories where
				(upch.id == id)
					select upch);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.userProfile);
					query = query.AsSplitQuery();
				}

				Database.UserProfileChangeHistory materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.UserProfileChangeHistory Entity was read with Admin privilege." : "BMC.UserProfileChangeHistory Entity was read.");

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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.UserProfileChangeHistory entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.UserProfileChangeHistory.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.UserProfileChangeHistory.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing UserProfileChangeHistory record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/UserProfileChangeHistory/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutUserProfileChangeHistory(int id, [FromBody]Database.UserProfileChangeHistory.UserProfileChangeHistoryDTO userProfileChangeHistoryDTO, CancellationToken cancellationToken = default)
		{
			if (userProfileChangeHistoryDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != userProfileChangeHistoryDTO.id)
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


			IQueryable<Database.UserProfileChangeHistory> query = (from x in _context.UserProfileChangeHistories
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserProfileChangeHistory existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserProfileChangeHistory PUT", id.ToString(), new Exception("No BMC.UserProfileChangeHistory entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.UserProfileChangeHistory cloneOfExisting = (Database.UserProfileChangeHistory)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new UserProfileChangeHistory object using the data from the existing record, updated with what is in the DTO.
			//
			Database.UserProfileChangeHistory userProfileChangeHistory = (Database.UserProfileChangeHistory)_context.Entry(existing).GetDatabaseValues().ToObject();
			userProfileChangeHistory.ApplyDTO(userProfileChangeHistoryDTO);
			//
			// The tenant guid for any UserProfileChangeHistory being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the UserProfileChangeHistory because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				userProfileChangeHistory.tenantGuid = existing.tenantGuid;
			}



			if (userProfileChangeHistory.timeStamp.Kind != DateTimeKind.Utc)
			{
				userProfileChangeHistory.timeStamp = userProfileChangeHistory.timeStamp.ToUniversalTime();
			}

			EntityEntry<Database.UserProfileChangeHistory> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(userProfileChangeHistory);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.UserProfileChangeHistory entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserProfileChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserProfileChangeHistory.CreateAnonymousWithFirstLevelSubObjects(userProfileChangeHistory)),
					null);


				return Ok(Database.UserProfileChangeHistory.CreateAnonymous(userProfileChangeHistory));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.UserProfileChangeHistory entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserProfileChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserProfileChangeHistory.CreateAnonymousWithFirstLevelSubObjects(userProfileChangeHistory)),
					ex);

				return Problem(ex.Message);
			}

		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new UserProfileChangeHistory record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserProfileChangeHistory", Name = "UserProfileChangeHistory")]
		public async Task<IActionResult> PostUserProfileChangeHistory([FromBody]Database.UserProfileChangeHistory.UserProfileChangeHistoryDTO userProfileChangeHistoryDTO, CancellationToken cancellationToken = default)
		{
			if (userProfileChangeHistoryDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Administrator role needed to write to this table.
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
			// Create a new UserProfileChangeHistory object using the data from the DTO
			//
			Database.UserProfileChangeHistory userProfileChangeHistory = Database.UserProfileChangeHistory.FromDTO(userProfileChangeHistoryDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				userProfileChangeHistory.tenantGuid = userTenantGuid;

				if (userProfileChangeHistory.timeStamp.Kind != DateTimeKind.Utc)
				{
					userProfileChangeHistory.timeStamp = userProfileChangeHistory.timeStamp.ToUniversalTime();
				}

				_context.UserProfileChangeHistories.Add(userProfileChangeHistory);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.UserProfileChangeHistory entity successfully created.",
					true,
					userProfileChangeHistory.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.UserProfileChangeHistory.CreateAnonymousWithFirstLevelSubObjects(userProfileChangeHistory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.UserProfileChangeHistory entity creation failed.", false, userProfileChangeHistory.id.ToString(), "", JsonSerializer.Serialize(userProfileChangeHistory), ex);

				return Problem(ex.Message);
			}


			return CreatedAtRoute("UserProfileChangeHistory", new { id = userProfileChangeHistory.id }, Database.UserProfileChangeHistory.CreateAnonymousWithFirstLevelSubObjects(userProfileChangeHistory));
		}

*/


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a UserProfileChangeHistory record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserProfileChangeHistory/{id}")]
		[Route("api/UserProfileChangeHistory")]
		public async Task<IActionResult> DeleteUserProfileChangeHistory(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Administrator role needed to write to this table.
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

			IQueryable<Database.UserProfileChangeHistory> query = (from x in _context.UserProfileChangeHistories
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserProfileChangeHistory userProfileChangeHistory = await query.FirstOrDefaultAsync(cancellationToken);

			if (userProfileChangeHistory == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserProfileChangeHistory DELETE", id.ToString(), new Exception("No BMC.UserProfileChangeHistory entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.UserProfileChangeHistory cloneOfExisting = (Database.UserProfileChangeHistory)_context.Entry(userProfileChangeHistory).GetDatabaseValues().ToObject();


			try
			{
				_context.UserProfileChangeHistories.Remove(userProfileChangeHistory);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.UserProfileChangeHistory entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserProfileChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserProfileChangeHistory.CreateAnonymousWithFirstLevelSubObjects(userProfileChangeHistory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.UserProfileChangeHistory entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserProfileChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserProfileChangeHistory.CreateAnonymousWithFirstLevelSubObjects(userProfileChangeHistory)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


*/
        /// <summary>
        /// 
        /// This gets a list of UserProfileChangeHistory records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/UserProfileChangeHistories/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? userProfileId = null,
			int? versionNumber = null,
			DateTime? timeStamp = null,
			int? userId = null,
			string data = null,
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
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.UserProfileChangeHistory> query = (from upch in _context.UserProfileChangeHistories select upch);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userProfileId.HasValue == true)
			{
				query = query.Where(upch => upch.userProfileId == userProfileId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(upch => upch.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(upch => upch.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(upch => upch.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(data) == false)
			{
				query = query.Where(upch => upch.data == data);
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Profile Change History, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.data.Contains(anyStringContains)
			       || x.userProfile.displayName.Contains(anyStringContains)
			       || x.userProfile.bio.Contains(anyStringContains)
			       || x.userProfile.location.Contains(anyStringContains)
			       || x.userProfile.avatarImagePath.Contains(anyStringContains)
			       || x.userProfile.profileBannerImagePath.Contains(anyStringContains)
			       || x.userProfile.websiteUrl.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderByDescending (x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.UserProfileChangeHistory.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/UserProfileChangeHistory/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Administrator role needed to write to this table.
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
