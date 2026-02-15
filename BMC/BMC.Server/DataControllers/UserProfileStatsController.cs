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
    /// This auto generated class provides the basic CRUD operations for the UserProfileStat entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the UserProfileStat entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class UserProfileStatsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<UserProfileStatsController> _logger;

		public UserProfileStatsController(BMCContext context, ILogger<UserProfileStatsController> logger) : base("BMC", "UserProfileStat")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of UserProfileStats filtered by the parameters provided.
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
		[Route("api/UserProfileStats")]
		public async Task<IActionResult> GetUserProfileStats(
			int? userProfileId = null,
			int? totalPartsOwned = null,
			int? totalUniquePartsOwned = null,
			int? totalSetsOwned = null,
			int? totalMocsPublished = null,
			int? totalFollowers = null,
			int? totalFollowing = null,
			int? totalLikesReceived = null,
			int? totalAchievementPoints = null,
			DateTime? lastCalculatedDate = null,
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
			if (lastCalculatedDate.HasValue == true && lastCalculatedDate.Value.Kind != DateTimeKind.Utc)
			{
				lastCalculatedDate = lastCalculatedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.UserProfileStat> query = (from ups in _context.UserProfileStats select ups);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userProfileId.HasValue == true)
			{
				query = query.Where(ups => ups.userProfileId == userProfileId.Value);
			}
			if (totalPartsOwned.HasValue == true)
			{
				query = query.Where(ups => ups.totalPartsOwned == totalPartsOwned.Value);
			}
			if (totalUniquePartsOwned.HasValue == true)
			{
				query = query.Where(ups => ups.totalUniquePartsOwned == totalUniquePartsOwned.Value);
			}
			if (totalSetsOwned.HasValue == true)
			{
				query = query.Where(ups => ups.totalSetsOwned == totalSetsOwned.Value);
			}
			if (totalMocsPublished.HasValue == true)
			{
				query = query.Where(ups => ups.totalMocsPublished == totalMocsPublished.Value);
			}
			if (totalFollowers.HasValue == true)
			{
				query = query.Where(ups => ups.totalFollowers == totalFollowers.Value);
			}
			if (totalFollowing.HasValue == true)
			{
				query = query.Where(ups => ups.totalFollowing == totalFollowing.Value);
			}
			if (totalLikesReceived.HasValue == true)
			{
				query = query.Where(ups => ups.totalLikesReceived == totalLikesReceived.Value);
			}
			if (totalAchievementPoints.HasValue == true)
			{
				query = query.Where(ups => ups.totalAchievementPoints == totalAchievementPoints.Value);
			}
			if (lastCalculatedDate.HasValue == true)
			{
				query = query.Where(ups => ups.lastCalculatedDate == lastCalculatedDate.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ups => ups.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ups => ups.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ups => ups.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ups => ups.deleted == false);
				}
			}
			else
			{
				query = query.Where(ups => ups.active == true);
				query = query.Where(ups => ups.deleted == false);
			}

			query = query.OrderBy(ups => ups.id);

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
			// Add the any string contains parameter to span all the string fields on the User Profile Stat, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       (includeRelations == true && x.userProfile.displayName.Contains(anyStringContains))
			       || (includeRelations == true && x.userProfile.bio.Contains(anyStringContains))
			       || (includeRelations == true && x.userProfile.location.Contains(anyStringContains))
			       || (includeRelations == true && x.userProfile.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.userProfile.avatarMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.userProfile.bannerFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.userProfile.bannerMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.userProfile.websiteUrl.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.UserProfileStat> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.UserProfileStat userProfileStat in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(userProfileStat, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.UserProfileStat Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.UserProfileStat Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of UserProfileStats filtered by the parameters provided.  Its query is similar to the GetUserProfileStats method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserProfileStats/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? userProfileId = null,
			int? totalPartsOwned = null,
			int? totalUniquePartsOwned = null,
			int? totalSetsOwned = null,
			int? totalMocsPublished = null,
			int? totalFollowers = null,
			int? totalFollowing = null,
			int? totalLikesReceived = null,
			int? totalAchievementPoints = null,
			DateTime? lastCalculatedDate = null,
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
			if (lastCalculatedDate.HasValue == true && lastCalculatedDate.Value.Kind != DateTimeKind.Utc)
			{
				lastCalculatedDate = lastCalculatedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.UserProfileStat> query = (from ups in _context.UserProfileStats select ups);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (userProfileId.HasValue == true)
			{
				query = query.Where(ups => ups.userProfileId == userProfileId.Value);
			}
			if (totalPartsOwned.HasValue == true)
			{
				query = query.Where(ups => ups.totalPartsOwned == totalPartsOwned.Value);
			}
			if (totalUniquePartsOwned.HasValue == true)
			{
				query = query.Where(ups => ups.totalUniquePartsOwned == totalUniquePartsOwned.Value);
			}
			if (totalSetsOwned.HasValue == true)
			{
				query = query.Where(ups => ups.totalSetsOwned == totalSetsOwned.Value);
			}
			if (totalMocsPublished.HasValue == true)
			{
				query = query.Where(ups => ups.totalMocsPublished == totalMocsPublished.Value);
			}
			if (totalFollowers.HasValue == true)
			{
				query = query.Where(ups => ups.totalFollowers == totalFollowers.Value);
			}
			if (totalFollowing.HasValue == true)
			{
				query = query.Where(ups => ups.totalFollowing == totalFollowing.Value);
			}
			if (totalLikesReceived.HasValue == true)
			{
				query = query.Where(ups => ups.totalLikesReceived == totalLikesReceived.Value);
			}
			if (totalAchievementPoints.HasValue == true)
			{
				query = query.Where(ups => ups.totalAchievementPoints == totalAchievementPoints.Value);
			}
			if (lastCalculatedDate.HasValue == true)
			{
				query = query.Where(ups => ups.lastCalculatedDate == lastCalculatedDate.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ups => ups.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ups => ups.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ups => ups.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ups => ups.deleted == false);
				}
			}
			else
			{
				query = query.Where(ups => ups.active == true);
				query = query.Where(ups => ups.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the User Profile Stat, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.userProfile.displayName.Contains(anyStringContains)
			       || x.userProfile.bio.Contains(anyStringContains)
			       || x.userProfile.location.Contains(anyStringContains)
			       || x.userProfile.avatarFileName.Contains(anyStringContains)
			       || x.userProfile.avatarMimeType.Contains(anyStringContains)
			       || x.userProfile.bannerFileName.Contains(anyStringContains)
			       || x.userProfile.bannerMimeType.Contains(anyStringContains)
			       || x.userProfile.websiteUrl.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single UserProfileStat by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserProfileStat/{id}")]
		public async Task<IActionResult> GetUserProfileStat(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.UserProfileStat> query = (from ups in _context.UserProfileStats where
							(ups.id == id) &&
							(userIsAdmin == true || ups.deleted == false) &&
							(userIsWriter == true || ups.active == true)
					select ups);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.userProfile);
					query = query.AsSplitQuery();
				}

				Database.UserProfileStat materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.UserProfileStat Entity was read with Admin privilege." : "BMC.UserProfileStat Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserProfileStat", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.UserProfileStat entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.UserProfileStat.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.UserProfileStat.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing UserProfileStat record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/UserProfileStat/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutUserProfileStat(int id, [FromBody]Database.UserProfileStat.UserProfileStatDTO userProfileStatDTO, CancellationToken cancellationToken = default)
		{
			if (userProfileStatDTO == null)
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



			if (id != userProfileStatDTO.id)
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


			IQueryable<Database.UserProfileStat> query = (from x in _context.UserProfileStats
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserProfileStat existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserProfileStat PUT", id.ToString(), new Exception("No BMC.UserProfileStat entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (userProfileStatDTO.objectGuid == Guid.Empty)
            {
                userProfileStatDTO.objectGuid = existing.objectGuid;
            }
            else if (userProfileStatDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a UserProfileStat record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.UserProfileStat cloneOfExisting = (Database.UserProfileStat)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new UserProfileStat object using the data from the existing record, updated with what is in the DTO.
			//
			Database.UserProfileStat userProfileStat = (Database.UserProfileStat)_context.Entry(existing).GetDatabaseValues().ToObject();
			userProfileStat.ApplyDTO(userProfileStatDTO);
			//
			// The tenant guid for any UserProfileStat being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the UserProfileStat because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				userProfileStat.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (userProfileStat.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.UserProfileStat record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (userProfileStat.lastCalculatedDate.HasValue == true && userProfileStat.lastCalculatedDate.Value.Kind != DateTimeKind.Utc)
			{
				userProfileStat.lastCalculatedDate = userProfileStat.lastCalculatedDate.Value.ToUniversalTime();
			}

			EntityEntry<Database.UserProfileStat> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(userProfileStat);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.UserProfileStat entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserProfileStat.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserProfileStat.CreateAnonymousWithFirstLevelSubObjects(userProfileStat)),
					null);


				return Ok(Database.UserProfileStat.CreateAnonymous(userProfileStat));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.UserProfileStat entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserProfileStat.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserProfileStat.CreateAnonymousWithFirstLevelSubObjects(userProfileStat)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new UserProfileStat record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserProfileStat", Name = "UserProfileStat")]
		public async Task<IActionResult> PostUserProfileStat([FromBody]Database.UserProfileStat.UserProfileStatDTO userProfileStatDTO, CancellationToken cancellationToken = default)
		{
			if (userProfileStatDTO == null)
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
			// Create a new UserProfileStat object using the data from the DTO
			//
			Database.UserProfileStat userProfileStat = Database.UserProfileStat.FromDTO(userProfileStatDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				userProfileStat.tenantGuid = userTenantGuid;

				if (userProfileStat.lastCalculatedDate.HasValue == true && userProfileStat.lastCalculatedDate.Value.Kind != DateTimeKind.Utc)
				{
					userProfileStat.lastCalculatedDate = userProfileStat.lastCalculatedDate.Value.ToUniversalTime();
				}

				userProfileStat.objectGuid = Guid.NewGuid();
				_context.UserProfileStats.Add(userProfileStat);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.UserProfileStat entity successfully created.",
					true,
					userProfileStat.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.UserProfileStat.CreateAnonymousWithFirstLevelSubObjects(userProfileStat)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.UserProfileStat entity creation failed.", false, userProfileStat.id.ToString(), "", JsonSerializer.Serialize(userProfileStat), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserProfileStat", userProfileStat.id, userProfileStat.id.ToString()));

			return CreatedAtRoute("UserProfileStat", new { id = userProfileStat.id }, Database.UserProfileStat.CreateAnonymousWithFirstLevelSubObjects(userProfileStat));
		}



        /// <summary>
        /// 
        /// This deletes a UserProfileStat record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserProfileStat/{id}")]
		[Route("api/UserProfileStat")]
		public async Task<IActionResult> DeleteUserProfileStat(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.UserProfileStat> query = (from x in _context.UserProfileStats
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserProfileStat userProfileStat = await query.FirstOrDefaultAsync(cancellationToken);

			if (userProfileStat == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserProfileStat DELETE", id.ToString(), new Exception("No BMC.UserProfileStat entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.UserProfileStat cloneOfExisting = (Database.UserProfileStat)_context.Entry(userProfileStat).GetDatabaseValues().ToObject();


			try
			{
				userProfileStat.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.UserProfileStat entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserProfileStat.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserProfileStat.CreateAnonymousWithFirstLevelSubObjects(userProfileStat)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.UserProfileStat entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserProfileStat.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserProfileStat.CreateAnonymousWithFirstLevelSubObjects(userProfileStat)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of UserProfileStat records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/UserProfileStats/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? userProfileId = null,
			int? totalPartsOwned = null,
			int? totalUniquePartsOwned = null,
			int? totalSetsOwned = null,
			int? totalMocsPublished = null,
			int? totalFollowers = null,
			int? totalFollowing = null,
			int? totalLikesReceived = null,
			int? totalAchievementPoints = null,
			DateTime? lastCalculatedDate = null,
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
			if (lastCalculatedDate.HasValue == true && lastCalculatedDate.Value.Kind != DateTimeKind.Utc)
			{
				lastCalculatedDate = lastCalculatedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.UserProfileStat> query = (from ups in _context.UserProfileStats select ups);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userProfileId.HasValue == true)
			{
				query = query.Where(ups => ups.userProfileId == userProfileId.Value);
			}
			if (totalPartsOwned.HasValue == true)
			{
				query = query.Where(ups => ups.totalPartsOwned == totalPartsOwned.Value);
			}
			if (totalUniquePartsOwned.HasValue == true)
			{
				query = query.Where(ups => ups.totalUniquePartsOwned == totalUniquePartsOwned.Value);
			}
			if (totalSetsOwned.HasValue == true)
			{
				query = query.Where(ups => ups.totalSetsOwned == totalSetsOwned.Value);
			}
			if (totalMocsPublished.HasValue == true)
			{
				query = query.Where(ups => ups.totalMocsPublished == totalMocsPublished.Value);
			}
			if (totalFollowers.HasValue == true)
			{
				query = query.Where(ups => ups.totalFollowers == totalFollowers.Value);
			}
			if (totalFollowing.HasValue == true)
			{
				query = query.Where(ups => ups.totalFollowing == totalFollowing.Value);
			}
			if (totalLikesReceived.HasValue == true)
			{
				query = query.Where(ups => ups.totalLikesReceived == totalLikesReceived.Value);
			}
			if (totalAchievementPoints.HasValue == true)
			{
				query = query.Where(ups => ups.totalAchievementPoints == totalAchievementPoints.Value);
			}
			if (lastCalculatedDate.HasValue == true)
			{
				query = query.Where(ups => ups.lastCalculatedDate == lastCalculatedDate.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ups => ups.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ups => ups.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ups => ups.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ups => ups.deleted == false);
				}
			}
			else
			{
				query = query.Where(ups => ups.active == true);
				query = query.Where(ups => ups.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Profile Stat, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.userProfile.displayName.Contains(anyStringContains)
			       || x.userProfile.bio.Contains(anyStringContains)
			       || x.userProfile.location.Contains(anyStringContains)
			       || x.userProfile.avatarFileName.Contains(anyStringContains)
			       || x.userProfile.avatarMimeType.Contains(anyStringContains)
			       || x.userProfile.bannerFileName.Contains(anyStringContains)
			       || x.userProfile.bannerMimeType.Contains(anyStringContains)
			       || x.userProfile.websiteUrl.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.UserProfileStat.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/UserProfileStat/CreateAuditEvent")]
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
