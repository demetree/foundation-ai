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
using Foundation.ChangeHistory;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the UserProfile entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the UserProfile entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class UserProfilesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object userProfilePutSyncRoot = new object();
		static object userProfileDeleteSyncRoot = new object();

		private BMCContext _context;

		private ILogger<UserProfilesController> _logger;

		public UserProfilesController(BMCContext context, ILogger<UserProfilesController> logger) : base("BMC", "UserProfile")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of UserProfiles filtered by the parameters provided.
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
		[Route("api/UserProfiles")]
		public async Task<IActionResult> GetUserProfiles(
			string displayName = null,
			string bio = null,
			string location = null,
			string avatarFileName = null,
			long? avatarSize = null,
			string avatarMimeType = null,
			string bannerFileName = null,
			long? bannerSize = null,
			string bannerMimeType = null,
			string websiteUrl = null,
			bool? isPublic = null,
			DateTime? memberSinceDate = null,
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

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
			if (memberSinceDate.HasValue == true && memberSinceDate.Value.Kind != DateTimeKind.Utc)
			{
				memberSinceDate = memberSinceDate.Value.ToUniversalTime();
			}

			IQueryable<Database.UserProfile> query = (from up in _context.UserProfiles select up);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(displayName) == false)
			{
				query = query.Where(up => up.displayName == displayName);
			}
			if (string.IsNullOrEmpty(bio) == false)
			{
				query = query.Where(up => up.bio == bio);
			}
			if (string.IsNullOrEmpty(location) == false)
			{
				query = query.Where(up => up.location == location);
			}
			if (string.IsNullOrEmpty(avatarFileName) == false)
			{
				query = query.Where(up => up.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(up => up.avatarSize == avatarSize.Value);
			}
			if (string.IsNullOrEmpty(avatarMimeType) == false)
			{
				query = query.Where(up => up.avatarMimeType == avatarMimeType);
			}
			if (string.IsNullOrEmpty(bannerFileName) == false)
			{
				query = query.Where(up => up.bannerFileName == bannerFileName);
			}
			if (bannerSize.HasValue == true)
			{
				query = query.Where(up => up.bannerSize == bannerSize.Value);
			}
			if (string.IsNullOrEmpty(bannerMimeType) == false)
			{
				query = query.Where(up => up.bannerMimeType == bannerMimeType);
			}
			if (string.IsNullOrEmpty(websiteUrl) == false)
			{
				query = query.Where(up => up.websiteUrl == websiteUrl);
			}
			if (isPublic.HasValue == true)
			{
				query = query.Where(up => up.isPublic == isPublic.Value);
			}
			if (memberSinceDate.HasValue == true)
			{
				query = query.Where(up => up.memberSinceDate == memberSinceDate.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(up => up.versionNumber == versionNumber.Value);
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

			query = query.OrderBy(up => up.displayName).ThenBy(up => up.location).ThenBy(up => up.avatarFileName);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Profile, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.displayName.Contains(anyStringContains)
			       || x.bio.Contains(anyStringContains)
			       || x.location.Contains(anyStringContains)
			       || x.avatarFileName.Contains(anyStringContains)
			       || x.avatarMimeType.Contains(anyStringContains)
			       || x.bannerFileName.Contains(anyStringContains)
			       || x.bannerMimeType.Contains(anyStringContains)
			       || x.websiteUrl.Contains(anyStringContains)
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.UserProfile> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.UserProfile userProfile in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(userProfile, databaseStoresDateWithTimeZone);
			}


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			if (diskBasedBinaryStorageMode == true)
			{
				var tasks = materialized.Select(async userProfile =>
				{

					if (userProfile.bannerData == null &&
					    userProfile.bannerSize.HasValue == true &&
					    userProfile.bannerSize.Value > 0)
					{
					    userProfile.bannerData = await LoadDataFromDiskAsync(userProfile.objectGuid, userProfile.versionNumber, "data");
					}

				}).ToList();

				// Run tasks concurrently and await their completion
				await Task.WhenAll(tasks);
			}

			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.UserProfile Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.UserProfile Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of UserProfiles filtered by the parameters provided.  Its query is similar to the GetUserProfiles method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserProfiles/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string displayName = null,
			string bio = null,
			string location = null,
			string avatarFileName = null,
			long? avatarSize = null,
			string avatarMimeType = null,
			string bannerFileName = null,
			long? bannerSize = null,
			string bannerMimeType = null,
			string websiteUrl = null,
			bool? isPublic = null,
			DateTime? memberSinceDate = null,
			int? versionNumber = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
			if (memberSinceDate.HasValue == true && memberSinceDate.Value.Kind != DateTimeKind.Utc)
			{
				memberSinceDate = memberSinceDate.Value.ToUniversalTime();
			}

			IQueryable<Database.UserProfile> query = (from up in _context.UserProfiles select up);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (displayName != null)
			{
				query = query.Where(up => up.displayName == displayName);
			}
			if (bio != null)
			{
				query = query.Where(up => up.bio == bio);
			}
			if (location != null)
			{
				query = query.Where(up => up.location == location);
			}
			if (avatarFileName != null)
			{
				query = query.Where(up => up.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(up => up.avatarSize == avatarSize.Value);
			}
			if (avatarMimeType != null)
			{
				query = query.Where(up => up.avatarMimeType == avatarMimeType);
			}
			if (bannerFileName != null)
			{
				query = query.Where(up => up.bannerFileName == bannerFileName);
			}
			if (bannerSize.HasValue == true)
			{
				query = query.Where(up => up.bannerSize == bannerSize.Value);
			}
			if (bannerMimeType != null)
			{
				query = query.Where(up => up.bannerMimeType == bannerMimeType);
			}
			if (websiteUrl != null)
			{
				query = query.Where(up => up.websiteUrl == websiteUrl);
			}
			if (isPublic.HasValue == true)
			{
				query = query.Where(up => up.isPublic == isPublic.Value);
			}
			if (memberSinceDate.HasValue == true)
			{
				query = query.Where(up => up.memberSinceDate == memberSinceDate.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(up => up.versionNumber == versionNumber.Value);
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
			// Add the any string contains parameter to span all the string fields on the User Profile, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.displayName.Contains(anyStringContains)
			       || x.bio.Contains(anyStringContains)
			       || x.location.Contains(anyStringContains)
			       || x.avatarFileName.Contains(anyStringContains)
			       || x.avatarMimeType.Contains(anyStringContains)
			       || x.bannerFileName.Contains(anyStringContains)
			       || x.bannerMimeType.Contains(anyStringContains)
			       || x.websiteUrl.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single UserProfile by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserProfile/{id}")]
		public async Task<IActionResult> GetUserProfile(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
				IQueryable<Database.UserProfile> query = (from up in _context.UserProfiles where
							(up.id == id) &&
							(userIsAdmin == true || up.deleted == false) &&
							(userIsWriter == true || up.active == true)
					select up);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.UserProfile materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

					if (diskBasedBinaryStorageMode == true &&
					    materialized.bannerData == null &&
					    materialized.bannerSize.HasValue == true &&
					    materialized.bannerSize.Value > 0)
					{
					    materialized.bannerData = await LoadDataFromDiskAsync(materialized.objectGuid, materialized.versionNumber, "data", cancellationToken);
					}

					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.UserProfile Entity was read with Admin privilege." : "BMC.UserProfile Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserProfile", materialized.id, materialized.displayName));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.UserProfile entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.UserProfile.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.UserProfile.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing UserProfile record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/UserProfile/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutUserProfile(int id, [FromBody]Database.UserProfile.UserProfileDTO userProfileDTO, CancellationToken cancellationToken = default)
		{
			if (userProfileDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != userProfileDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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


			IQueryable<Database.UserProfile> query = (from x in _context.UserProfiles
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserProfile existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserProfile PUT", id.ToString(), new Exception("No BMC.UserProfile entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (userProfileDTO.objectGuid == Guid.Empty)
            {
                userProfileDTO.objectGuid = existing.objectGuid;
            }
            else if (userProfileDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a UserProfile record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.UserProfile cloneOfExisting = (Database.UserProfile)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new UserProfile object using the data from the existing record, updated with what is in the DTO.
			//
			Database.UserProfile userProfile = (Database.UserProfile)_context.Entry(existing).GetDatabaseValues().ToObject();
			userProfile.ApplyDTO(userProfileDTO);
			//
			// The tenant guid for any UserProfile being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the UserProfile because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				userProfile.tenantGuid = existing.tenantGuid;
			}

			lock (userProfilePutSyncRoot)
			{
				//
				// Validate the version number for the userProfile being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != userProfile.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "UserProfile save attempt was made but save request was with version " + userProfile.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The UserProfile you are trying to update has already changed.  Please try your save again after reloading the UserProfile.");
				}
				else
				{
					// Same record.  Increase version.
					userProfile.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (userProfile.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.UserProfile record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (userProfile.displayName != null && userProfile.displayName.Length > 100)
				{
					userProfile.displayName = userProfile.displayName.Substring(0, 100);
				}

				if (userProfile.location != null && userProfile.location.Length > 100)
				{
					userProfile.location = userProfile.location.Substring(0, 100);
				}

				if (userProfile.avatarFileName != null && userProfile.avatarFileName.Length > 250)
				{
					userProfile.avatarFileName = userProfile.avatarFileName.Substring(0, 250);
				}

				if (userProfile.avatarMimeType != null && userProfile.avatarMimeType.Length > 100)
				{
					userProfile.avatarMimeType = userProfile.avatarMimeType.Substring(0, 100);
				}

				if (userProfile.bannerFileName != null && userProfile.bannerFileName.Length > 250)
				{
					userProfile.bannerFileName = userProfile.bannerFileName.Substring(0, 250);
				}

				if (userProfile.bannerMimeType != null && userProfile.bannerMimeType.Length > 100)
				{
					userProfile.bannerMimeType = userProfile.bannerMimeType.Substring(0, 100);
				}

				if (userProfile.websiteUrl != null && userProfile.websiteUrl.Length > 250)
				{
					userProfile.websiteUrl = userProfile.websiteUrl.Substring(0, 250);
				}

				if (userProfile.memberSinceDate.HasValue == true && userProfile.memberSinceDate.Value.Kind != DateTimeKind.Utc)
				{
					userProfile.memberSinceDate = userProfile.memberSinceDate.Value.ToUniversalTime();
				}


				//
				// Add default values for any missing data attribute fields.
				//
				if (userProfile.bannerData != null && string.IsNullOrEmpty(userProfile.bannerFileName))
				{
				    userProfile.bannerFileName = userProfile.objectGuid.ToString() + ".data";
				}

				if (userProfile.bannerData != null && (userProfile.bannerSize.HasValue == false || userProfile.bannerSize != userProfile.bannerData.Length))
				{
				    userProfile.bannerSize = userProfile.bannerData.Length;
				}

				if (userProfile.bannerData != null && string.IsNullOrEmpty(userProfile.bannerMimeType))
				{
				    userProfile.bannerMimeType = "application/octet-stream";
				}

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				try
				{
					byte[] dataReferenceBeforeClearing = userProfile.bannerData;

					if (diskBasedBinaryStorageMode == true &&
					    userProfile.bannerFileName != null &&
					    userProfile.bannerData != null &&
					    userProfile.bannerSize.HasValue == true &&
					    userProfile.bannerSize.Value > 0)
					{
					    //
					    // write the bytes to disk
					    //
					    WriteDataToDisk(userProfile.objectGuid, userProfile.versionNumber, userProfile.bannerData, "data");

					    //
					    // Clear the data from the object before we put it into the db
					    //
					    userProfile.bannerData = null;

					}

				    EntityEntry<Database.UserProfile> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(userProfile);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        UserProfileChangeHistory userProfileChangeHistory = new UserProfileChangeHistory();
				        userProfileChangeHistory.userProfileId = userProfile.id;
				        userProfileChangeHistory.versionNumber = userProfile.versionNumber;
				        userProfileChangeHistory.timeStamp = DateTime.UtcNow;
				        userProfileChangeHistory.userId = securityUser.id;
				        userProfileChangeHistory.tenantGuid = userTenantGuid;
				        userProfileChangeHistory.data = JsonSerializer.Serialize(Database.UserProfile.CreateAnonymousWithFirstLevelSubObjects(userProfile));
				        _context.UserProfileChangeHistories.Add(userProfileChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    userProfile.bannerData = dataReferenceBeforeClearing;
					}

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.UserProfile entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserProfile.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserProfile.CreateAnonymousWithFirstLevelSubObjects(userProfile)),
						null);

				return Ok(Database.UserProfile.CreateAnonymous(userProfile));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.UserProfile entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserProfile.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserProfile.CreateAnonymousWithFirstLevelSubObjects(userProfile)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new UserProfile record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserProfile", Name = "UserProfile")]
		public async Task<IActionResult> PostUserProfile([FromBody]Database.UserProfile.UserProfileDTO userProfileDTO, CancellationToken cancellationToken = default)
		{
			if (userProfileDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new UserProfile object using the data from the DTO
			//
			Database.UserProfile userProfile = Database.UserProfile.FromDTO(userProfileDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				userProfile.tenantGuid = userTenantGuid;

				if (userProfile.displayName != null && userProfile.displayName.Length > 100)
				{
					userProfile.displayName = userProfile.displayName.Substring(0, 100);
				}

				if (userProfile.location != null && userProfile.location.Length > 100)
				{
					userProfile.location = userProfile.location.Substring(0, 100);
				}

				if (userProfile.avatarFileName != null && userProfile.avatarFileName.Length > 250)
				{
					userProfile.avatarFileName = userProfile.avatarFileName.Substring(0, 250);
				}

				if (userProfile.avatarMimeType != null && userProfile.avatarMimeType.Length > 100)
				{
					userProfile.avatarMimeType = userProfile.avatarMimeType.Substring(0, 100);
				}

				if (userProfile.bannerFileName != null && userProfile.bannerFileName.Length > 250)
				{
					userProfile.bannerFileName = userProfile.bannerFileName.Substring(0, 250);
				}

				if (userProfile.bannerMimeType != null && userProfile.bannerMimeType.Length > 100)
				{
					userProfile.bannerMimeType = userProfile.bannerMimeType.Substring(0, 100);
				}

				if (userProfile.websiteUrl != null && userProfile.websiteUrl.Length > 250)
				{
					userProfile.websiteUrl = userProfile.websiteUrl.Substring(0, 250);
				}

				if (userProfile.memberSinceDate.HasValue == true && userProfile.memberSinceDate.Value.Kind != DateTimeKind.Utc)
				{
					userProfile.memberSinceDate = userProfile.memberSinceDate.Value.ToUniversalTime();
				}

				userProfile.objectGuid = Guid.NewGuid();

				//
				// Add default values for any missing data attribute fields.
				//
				if (userProfile.bannerData != null && string.IsNullOrEmpty(userProfile.bannerFileName))
				{
				    userProfile.bannerFileName = userProfile.objectGuid.ToString() + ".data";
				}

				if (userProfile.bannerData != null && (userProfile.bannerSize.HasValue == false || userProfile.bannerSize != userProfile.bannerData.Length))
				{
				    userProfile.bannerSize = userProfile.bannerData.Length;
				}

				if (userProfile.bannerData != null && string.IsNullOrEmpty(userProfile.bannerMimeType))
				{
				    userProfile.bannerMimeType = "application/octet-stream";
				}

				userProfile.versionNumber = 1;

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				byte[] dataReferenceBeforeClearing = userProfile.bannerData;

				if (diskBasedBinaryStorageMode == true &&
				    userProfile.bannerData != null &&
				    userProfile.bannerFileName != null &&
				    userProfile.bannerSize.HasValue == true &&
				    userProfile.bannerSize.Value > 0)
				{
				    //
				    // write the bytes to disk
				    //
				    await WriteDataToDiskAsync(userProfile.objectGuid, userProfile.versionNumber, userProfile.bannerData, "data", cancellationToken);

				    //
				    // Clear the data from the object before we put it into the db
				    //
				    userProfile.bannerData = null;

				}

				_context.UserProfiles.Add(userProfile);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the userProfile object so that no further changes will be written to the database
				    //
				    _context.Entry(userProfile).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					userProfile.avatarData = null;
					userProfile.bannerData = null;
					userProfile.UserProfileChangeHistories = null;
					userProfile.UserProfileLinks = null;
					userProfile.UserProfileStats = null;


				    UserProfileChangeHistory userProfileChangeHistory = new UserProfileChangeHistory();
				    userProfileChangeHistory.userProfileId = userProfile.id;
				    userProfileChangeHistory.versionNumber = userProfile.versionNumber;
				    userProfileChangeHistory.timeStamp = DateTime.UtcNow;
				    userProfileChangeHistory.userId = securityUser.id;
				    userProfileChangeHistory.tenantGuid = userTenantGuid;
				    userProfileChangeHistory.data = JsonSerializer.Serialize(Database.UserProfile.CreateAnonymousWithFirstLevelSubObjects(userProfile));
				    _context.UserProfileChangeHistories.Add(userProfileChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"BMC.UserProfile entity successfully created.",
						true,
						userProfile. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.UserProfile.CreateAnonymousWithFirstLevelSubObjects(userProfile)),
						null);



					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    userProfile.bannerData = dataReferenceBeforeClearing;
					}

				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.UserProfile entity creation failed.", false, userProfile.id.ToString(), "", JsonSerializer.Serialize(Database.UserProfile.CreateAnonymousWithFirstLevelSubObjects(userProfile)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserProfile", userProfile.id, userProfile.displayName));

			return CreatedAtRoute("UserProfile", new { id = userProfile.id }, Database.UserProfile.CreateAnonymousWithFirstLevelSubObjects(userProfile));
		}



        /// <summary>
        /// 
        /// This rolls a UserProfile entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserProfile/Rollback/{id}")]
		[Route("api/UserProfile/Rollback")]
		public async Task<IActionResult> RollbackToUserProfileVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.UserProfile> query = (from x in _context.UserProfiles
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();


			//
			// Make sure nobody else is editing this UserProfile concurrently
			//
			lock (userProfilePutSyncRoot)
			{
				
				Database.UserProfile userProfile = query.FirstOrDefault();
				
				if (userProfile == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserProfile rollback", id.ToString(), new Exception("No BMC.UserProfile entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the UserProfile current state so we can log it.
				//
				Database.UserProfile cloneOfExisting = (Database.UserProfile)_context.Entry(userProfile).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.avatarData = null;
				cloneOfExisting.bannerData = null;
				cloneOfExisting.UserProfileChangeHistories = null;
				cloneOfExisting.UserProfileLinks = null;
				cloneOfExisting.UserProfileStats = null;

				if (versionNumber >= userProfile.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for BMC.UserProfile rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for BMC.UserProfile rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				UserProfileChangeHistory userProfileChangeHistory = (from x in _context.UserProfileChangeHistories
				                                               where
				                                               x.userProfileId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (userProfileChangeHistory != null)
				{
				    Database.UserProfile oldUserProfile = JsonSerializer.Deserialize<Database.UserProfile>(userProfileChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    userProfile.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    userProfile.displayName = oldUserProfile.displayName;
				    userProfile.bio = oldUserProfile.bio;
				    userProfile.location = oldUserProfile.location;
				    userProfile.avatarFileName = oldUserProfile.avatarFileName;
				    userProfile.avatarSize = oldUserProfile.avatarSize;
				    userProfile.avatarData = oldUserProfile.avatarData;
				    userProfile.avatarMimeType = oldUserProfile.avatarMimeType;
				    userProfile.bannerFileName = oldUserProfile.bannerFileName;
				    userProfile.bannerSize = oldUserProfile.bannerSize;
				    userProfile.bannerData = oldUserProfile.bannerData;
				    userProfile.bannerMimeType = oldUserProfile.bannerMimeType;
				    userProfile.websiteUrl = oldUserProfile.websiteUrl;
				    userProfile.isPublic = oldUserProfile.isPublic;
				    userProfile.memberSinceDate = oldUserProfile.memberSinceDate;
				    userProfile.objectGuid = oldUserProfile.objectGuid;
				    userProfile.active = oldUserProfile.active;
				    userProfile.deleted = oldUserProfile.deleted;
				    //
				    // If disk based binary mode is on, then we need to copy the old data file over as well.
				    //
				    if (diskBasedBinaryStorageMode == true)
				    {
				    	Byte[] binaryData = LoadDataFromDisk(oldUserProfile.objectGuid, oldUserProfile.versionNumber, "data");

				    	//
				    	// Write out the data as the new version
				    	//
				    	WriteDataToDisk(userProfile.objectGuid, userProfile.versionNumber, binaryData, "data");
				    }

				    string serializedUserProfile = JsonSerializer.Serialize(userProfile);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        UserProfileChangeHistory newUserProfileChangeHistory = new UserProfileChangeHistory();
				        newUserProfileChangeHistory.userProfileId = userProfile.id;
				        newUserProfileChangeHistory.versionNumber = userProfile.versionNumber;
				        newUserProfileChangeHistory.timeStamp = DateTime.UtcNow;
				        newUserProfileChangeHistory.userId = securityUser.id;
				        newUserProfileChangeHistory.tenantGuid = userTenantGuid;
				        newUserProfileChangeHistory.data = JsonSerializer.Serialize(Database.UserProfile.CreateAnonymousWithFirstLevelSubObjects(userProfile));
				        _context.UserProfileChangeHistories.Add(newUserProfileChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.UserProfile rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserProfile.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserProfile.CreateAnonymousWithFirstLevelSubObjects(userProfile)),
						null);


				    return Ok(Database.UserProfile.CreateAnonymous(userProfile));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for BMC.UserProfile rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for BMC.UserProfile rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a UserProfile.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the UserProfile</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserProfile/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetUserProfileChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
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


			Database.UserProfile userProfile = await _context.UserProfiles.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (userProfile == null)
			{
				return NotFound();
			}

			try
			{
				userProfile.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.UserProfile> versionInfo = await userProfile.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a UserProfile.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the UserProfile</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserProfile/{id}/AuditHistory")]
		public async Task<IActionResult> GetUserProfileAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
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


			Database.UserProfile userProfile = await _context.UserProfiles.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (userProfile == null)
			{
				return NotFound();
			}

			try
			{
				userProfile.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.UserProfile>> versions = await userProfile.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a UserProfile.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the UserProfile</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The UserProfile object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserProfile/{id}/Version/{version}")]
		public async Task<IActionResult> GetUserProfileVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
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


			Database.UserProfile userProfile = await _context.UserProfiles.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (userProfile == null)
			{
				return NotFound();
			}

			try
			{
				userProfile.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.UserProfile> versionInfo = await userProfile.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a UserProfile at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the UserProfile</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The UserProfile object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserProfile/{id}/StateAtTime")]
		public async Task<IActionResult> GetUserProfileStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
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


			Database.UserProfile userProfile = await _context.UserProfiles.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (userProfile == null)
			{
				return NotFound();
			}

			try
			{
				userProfile.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.UserProfile> versionInfo = await userProfile.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a UserProfile record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserProfile/{id}")]
		[Route("api/UserProfile")]
		public async Task<IActionResult> DeleteUserProfile(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.UserProfile> query = (from x in _context.UserProfiles
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserProfile userProfile = await query.FirstOrDefaultAsync(cancellationToken);

			if (userProfile == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserProfile DELETE", id.ToString(), new Exception("No BMC.UserProfile entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.UserProfile cloneOfExisting = (Database.UserProfile)_context.Entry(userProfile).GetDatabaseValues().ToObject();


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			lock (userProfileDeleteSyncRoot)
			{
			    try
			    {
			        userProfile.deleted = true;
			        userProfile.versionNumber++;

			        _context.SaveChanges();

			        //
			        // If in disk based storage mode, create a copy of the disk data file for the new version.
			        //
			        if (diskBasedBinaryStorageMode == true)
			        {
			        	Byte[] binaryData = LoadDataFromDisk(userProfile.objectGuid, userProfile.versionNumber -1, "data");

			        	//
			        	// Write out the same data
			        	//
			        	WriteDataToDisk(userProfile.objectGuid, userProfile.versionNumber, binaryData, "data");
			        }

			        //
			        // Now add the change history
			        //
			        UserProfileChangeHistory userProfileChangeHistory = new UserProfileChangeHistory();
			        userProfileChangeHistory.userProfileId = userProfile.id;
			        userProfileChangeHistory.versionNumber = userProfile.versionNumber;
			        userProfileChangeHistory.timeStamp = DateTime.UtcNow;
			        userProfileChangeHistory.userId = securityUser.id;
			        userProfileChangeHistory.tenantGuid = userTenantGuid;
			        userProfileChangeHistory.data = JsonSerializer.Serialize(Database.UserProfile.CreateAnonymousWithFirstLevelSubObjects(userProfile));
			        _context.UserProfileChangeHistories.Add(userProfileChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.UserProfile entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserProfile.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserProfile.CreateAnonymousWithFirstLevelSubObjects(userProfile)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.UserProfile entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserProfile.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserProfile.CreateAnonymousWithFirstLevelSubObjects(userProfile)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of UserProfile records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/UserProfiles/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string displayName = null,
			string bio = null,
			string location = null,
			string avatarFileName = null,
			long? avatarSize = null,
			string avatarMimeType = null,
			string bannerFileName = null,
			long? bannerSize = null,
			string bannerMimeType = null,
			string websiteUrl = null,
			bool? isPublic = null,
			DateTime? memberSinceDate = null,
			int? versionNumber = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);


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
			if (memberSinceDate.HasValue == true && memberSinceDate.Value.Kind != DateTimeKind.Utc)
			{
				memberSinceDate = memberSinceDate.Value.ToUniversalTime();
			}

			IQueryable<Database.UserProfile> query = (from up in _context.UserProfiles select up);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(displayName) == false)
			{
				query = query.Where(up => up.displayName == displayName);
			}
			if (string.IsNullOrEmpty(bio) == false)
			{
				query = query.Where(up => up.bio == bio);
			}
			if (string.IsNullOrEmpty(location) == false)
			{
				query = query.Where(up => up.location == location);
			}
			if (string.IsNullOrEmpty(avatarFileName) == false)
			{
				query = query.Where(up => up.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(up => up.avatarSize == avatarSize.Value);
			}
			if (string.IsNullOrEmpty(avatarMimeType) == false)
			{
				query = query.Where(up => up.avatarMimeType == avatarMimeType);
			}
			if (string.IsNullOrEmpty(bannerFileName) == false)
			{
				query = query.Where(up => up.bannerFileName == bannerFileName);
			}
			if (bannerSize.HasValue == true)
			{
				query = query.Where(up => up.bannerSize == bannerSize.Value);
			}
			if (string.IsNullOrEmpty(bannerMimeType) == false)
			{
				query = query.Where(up => up.bannerMimeType == bannerMimeType);
			}
			if (string.IsNullOrEmpty(websiteUrl) == false)
			{
				query = query.Where(up => up.websiteUrl == websiteUrl);
			}
			if (isPublic.HasValue == true)
			{
				query = query.Where(up => up.isPublic == isPublic.Value);
			}
			if (memberSinceDate.HasValue == true)
			{
				query = query.Where(up => up.memberSinceDate == memberSinceDate.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(up => up.versionNumber == versionNumber.Value);
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
			// Add the any string contains parameter to span all the string fields on the User Profile, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.displayName.Contains(anyStringContains)
			       || x.bio.Contains(anyStringContains)
			       || x.location.Contains(anyStringContains)
			       || x.avatarFileName.Contains(anyStringContains)
			       || x.avatarMimeType.Contains(anyStringContains)
			       || x.bannerFileName.Contains(anyStringContains)
			       || x.bannerMimeType.Contains(anyStringContains)
			       || x.websiteUrl.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.displayName).ThenBy(x => x.location).ThenBy(x => x.avatarFileName);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.UserProfile.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/UserProfile/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}




        [Route("api/UserProfile/Data/{id:int}")]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [HttpPost]
        [HttpPut]
        public async Task<IActionResult> UploadData(int id, CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			MediaTypeHeaderValue mediaTypeHeader; 

            if (!HttpContext.Request.HasFormContentType ||
				!MediaTypeHeaderValue.TryParse(HttpContext.Request.ContentType, out mediaTypeHeader) ||
                string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
            {
                return new UnsupportedMediaTypeResult();
            }


            Database.UserProfile userProfile = await (from x in _context.UserProfiles where x.id == id && x.active == true && x.deleted == false select x).FirstOrDefaultAsync();
            if (userProfile == null)
            {
                return NotFound();
            }

            bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();


            // This will be used to signal whether we are saving data or clearing it.
            bool foundFileData = false;


            //
            // This will get the first file from the request and save it
            //
			try
			{
                MultipartReader reader = new MultipartReader(mediaTypeHeader.Boundary.Value, HttpContext.Request.Body);
                MultipartSection section = await reader.ReadNextSectionAsync();

                while (section != null)
				{
					ContentDispositionHeaderValue contentDisposition;

					bool hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);


					if (hasContentDispositionHeader && contentDisposition.DispositionType.Equals("form-data") &&
						!string.IsNullOrEmpty(contentDisposition.FileName.Value))
					{

						foundFileData = true;
						string fileName = contentDisposition.FileName.ToString().Trim('"');

						// default the mime type to be the one for arbitrary binary data unless we have a mime type on the content headers that tells us otherwise.
						MediaTypeHeaderValue mediaType;
						bool hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);

						string mimeType = "application/octet-stream";
						if (hasMediaTypeHeader && mediaTypeHeader.MediaType != null )
						{
							mimeType = mediaTypeHeader.MediaType.ToString();
						}

						lock (userProfilePutSyncRoot)
						{
							try
							{
								using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
								{
									userProfile.bannerFileName = fileName.Trim();
									userProfile.bannerMimeType = mimeType;
									userProfile.bannerSize = section.Body.Length;

									userProfile.versionNumber++;

									if (diskBasedBinaryStorageMode == true &&
										 userProfile.bannerFileName != null &&
										 userProfile.bannerSize > 0)
									{
										//
										// write the bytes to disk
										//
										WriteDataToDisk(userProfile.objectGuid, userProfile.versionNumber, section.Body, "data");
										//
										// Clear the data from the object before we put it into the db
										//
										userProfile.bannerData = null;
									}
									else
									{
										using (MemoryStream memoryStream = new MemoryStream((int)section.Body.Length))
										{
											section.Body.CopyTo(memoryStream);
											userProfile.bannerData = memoryStream.ToArray();
										}
									}
									//
									// Now add the change history
									//
									UserProfileChangeHistory userProfileChangeHistory = new UserProfileChangeHistory();
									userProfileChangeHistory.userProfileId = userProfile.id;
									userProfileChangeHistory.versionNumber = userProfile.versionNumber;
									userProfileChangeHistory.timeStamp = DateTime.UtcNow;
									userProfileChangeHistory.userId = securityUser.id;
									userProfileChangeHistory.tenantGuid = userProfile.tenantGuid;
									userProfileChangeHistory.data = JsonSerializer.Serialize(Database.UserProfile.CreateAnonymousWithFirstLevelSubObjects(userProfile));
									_context.UserProfileChangeHistories.Add(userProfileChangeHistory);

									_context.SaveChanges();

									transaction.Commit();

									CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "UserProfile Data Uploaded with filename of " + fileName + " and with size of " + section.Body.Length, id.ToString());
								}
							}
							catch (Exception ex)
							{
								CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "UserProfile Data Upload Failed.", false, id.ToString(), "", "", ex);

								return Problem(ex.Message);
							}
						}


						//
						// Stop looking for more files.
						//
						break;
					}

					section = await reader.ReadNextSectionAsync();
				}
            }
            catch (Exception ex)
            {
                CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Caught error in UploadData handler", id.ToString(), ex);

                return Problem(ex.Message);
            }

            //
            // Treat the situation where we have a valid ID but no file content as a request to clear the data
            //
            if (foundFileData == false)
            {
                lock (userProfilePutSyncRoot)
                {
                    try
                    {
                        using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                        {
                            if (diskBasedBinaryStorageMode == true)
                            {
								DeleteDataFromDisk(userProfile.objectGuid, userProfile.versionNumber, "data");
                            }

                            userProfile.bannerFileName = null;
                            userProfile.bannerMimeType = null;
                            userProfile.bannerSize = 0;
                            userProfile.bannerData = null;
                            userProfile.versionNumber++;


                            //
                            // Now add the change history
                            //
                            UserProfileChangeHistory userProfileChangeHistory = new UserProfileChangeHistory();
                            userProfileChangeHistory.userProfileId = userProfile.id;
                            userProfileChangeHistory.versionNumber = userProfile.versionNumber;
                            userProfileChangeHistory.timeStamp = DateTime.UtcNow;
                            userProfileChangeHistory.userId = securityUser.id;
                                    userProfileChangeHistory.tenantGuid = userProfile.tenantGuid;
                                    userProfileChangeHistory.data = JsonSerializer.Serialize(Database.UserProfile.CreateAnonymousWithFirstLevelSubObjects(userProfile));
                            _context.UserProfileChangeHistories.Add(userProfileChangeHistory);

                            _context.SaveChanges();

                            transaction.Commit();

                            CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "UserProfile data cleared.", id.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "UserProfile data clear failed.", false, id.ToString(), "", "", ex);

                        return Problem(ex.Message);
                    }
                }
            }

            return Ok();
        }

        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/UserProfile/Data/{id:int}")]
        public async Task<IActionResult> DownloadDataAsync(int id, CancellationToken cancellationToken = default)
        {

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			using (BMCContext context = new BMCContext())
            {
                //
                // Return the data to the user as though it was a file.
                //
                Database.UserProfile userProfile = await (from d in context.UserProfiles
                                                where d.id == id &&
                                                d.active == true &&
                                                d.deleted == false
                                                select d).FirstOrDefaultAsync();

                if (userProfile != null && userProfile.bannerData != null)
                {
                   return File(userProfile.bannerData.ToArray<byte>(), userProfile.bannerMimeType, userProfile.bannerFileName != null ? userProfile.bannerFileName.Trim() : "UserProfile_" + userProfile.id.ToString(), true);
                }
                else
                {
                    return BadRequest();
                }
            }
        }
	}
}
