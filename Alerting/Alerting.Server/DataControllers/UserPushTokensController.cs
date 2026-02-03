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
using Foundation.ChangeHistory;

namespace Foundation.Alerting.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the UserPushToken entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the UserPushToken entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class UserPushTokensController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object userPushTokenPutSyncRoot = new object();
		static object userPushTokenDeleteSyncRoot = new object();

		private AlertingContext _context;

		private ILogger<UserPushTokensController> _logger;

		public UserPushTokensController(AlertingContext context, ILogger<UserPushTokensController> logger) : base("Alerting", "UserPushToken")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of UserPushTokens filtered by the parameters provided.
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
		[Route("api/UserPushTokens")]
		public async Task<IActionResult> GetUserPushTokens(
			Guid? userObjectGuid = null,
			string fcmToken = null,
			string deviceFingerprint = null,
			string platform = null,
			string userAgent = null,
			DateTime? registeredAt = null,
			DateTime? lastUpdatedAt = null,
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
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
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
			if (registeredAt.HasValue == true && registeredAt.Value.Kind != DateTimeKind.Utc)
			{
				registeredAt = registeredAt.Value.ToUniversalTime();
			}

			if (lastUpdatedAt.HasValue == true && lastUpdatedAt.Value.Kind != DateTimeKind.Utc)
			{
				lastUpdatedAt = lastUpdatedAt.Value.ToUniversalTime();
			}

			IQueryable<Database.UserPushToken> query = (from upt in _context.UserPushTokens select upt);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userObjectGuid.HasValue == true)
			{
				query = query.Where(upt => upt.userObjectGuid == userObjectGuid);
			}
			if (string.IsNullOrEmpty(fcmToken) == false)
			{
				query = query.Where(upt => upt.fcmToken == fcmToken);
			}
			if (string.IsNullOrEmpty(deviceFingerprint) == false)
			{
				query = query.Where(upt => upt.deviceFingerprint == deviceFingerprint);
			}
			if (string.IsNullOrEmpty(platform) == false)
			{
				query = query.Where(upt => upt.platform == platform);
			}
			if (string.IsNullOrEmpty(userAgent) == false)
			{
				query = query.Where(upt => upt.userAgent == userAgent);
			}
			if (registeredAt.HasValue == true)
			{
				query = query.Where(upt => upt.registeredAt == registeredAt.Value);
			}
			if (lastUpdatedAt.HasValue == true)
			{
				query = query.Where(upt => upt.lastUpdatedAt == lastUpdatedAt.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(upt => upt.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(upt => upt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(upt => upt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(upt => upt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(upt => upt.deleted == false);
				}
			}
			else
			{
				query = query.Where(upt => upt.active == true);
				query = query.Where(upt => upt.deleted == false);
			}

			query = query.OrderBy(upt => upt.fcmToken).ThenBy(upt => upt.deviceFingerprint).ThenBy(upt => upt.platform);

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
			// Add the any string contains parameter to span all the string fields on the User Push Token, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.fcmToken.Contains(anyStringContains)
			       || x.deviceFingerprint.Contains(anyStringContains)
			       || x.platform.Contains(anyStringContains)
			       || x.userAgent.Contains(anyStringContains)
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.UserPushToken> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.UserPushToken userPushToken in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(userPushToken, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Alerting.UserPushToken Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Alerting.UserPushToken Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of UserPushTokens filtered by the parameters provided.  Its query is similar to the GetUserPushTokens method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserPushTokens/RowCount")]
		public async Task<IActionResult> GetRowCount(
			Guid? userObjectGuid = null,
			string fcmToken = null,
			string deviceFingerprint = null,
			string platform = null,
			string userAgent = null,
			DateTime? registeredAt = null,
			DateTime? lastUpdatedAt = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
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
			if (registeredAt.HasValue == true && registeredAt.Value.Kind != DateTimeKind.Utc)
			{
				registeredAt = registeredAt.Value.ToUniversalTime();
			}

			if (lastUpdatedAt.HasValue == true && lastUpdatedAt.Value.Kind != DateTimeKind.Utc)
			{
				lastUpdatedAt = lastUpdatedAt.Value.ToUniversalTime();
			}

			IQueryable<Database.UserPushToken> query = (from upt in _context.UserPushTokens select upt);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (userObjectGuid.HasValue == true)
			{
				query = query.Where(upt => upt.userObjectGuid == userObjectGuid);
			}
			if (fcmToken != null)
			{
				query = query.Where(upt => upt.fcmToken == fcmToken);
			}
			if (deviceFingerprint != null)
			{
				query = query.Where(upt => upt.deviceFingerprint == deviceFingerprint);
			}
			if (platform != null)
			{
				query = query.Where(upt => upt.platform == platform);
			}
			if (userAgent != null)
			{
				query = query.Where(upt => upt.userAgent == userAgent);
			}
			if (registeredAt.HasValue == true)
			{
				query = query.Where(upt => upt.registeredAt == registeredAt.Value);
			}
			if (lastUpdatedAt.HasValue == true)
			{
				query = query.Where(upt => upt.lastUpdatedAt == lastUpdatedAt.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(upt => upt.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(upt => upt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(upt => upt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(upt => upt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(upt => upt.deleted == false);
				}
			}
			else
			{
				query = query.Where(upt => upt.active == true);
				query = query.Where(upt => upt.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the User Push Token, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.fcmToken.Contains(anyStringContains)
			       || x.deviceFingerprint.Contains(anyStringContains)
			       || x.platform.Contains(anyStringContains)
			       || x.userAgent.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single UserPushToken by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserPushToken/{id}")]
		public async Task<IActionResult> GetUserPushToken(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
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
				IQueryable<Database.UserPushToken> query = (from upt in _context.UserPushTokens where
							(upt.id == id) &&
							(userIsAdmin == true || upt.deleted == false) &&
							(userIsWriter == true || upt.active == true)
					select upt);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.UserPushToken materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Alerting.UserPushToken Entity was read with Admin privilege." : "Alerting.UserPushToken Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserPushToken", materialized.id, materialized.fcmToken));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Alerting.UserPushToken entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Alerting.UserPushToken.   Entity was read with Admin privilege." : "Exception caught during entity read of Alerting.UserPushToken.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing UserPushToken record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/UserPushToken/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutUserPushToken(int id, [FromBody]Database.UserPushToken.UserPushTokenDTO userPushTokenDTO, CancellationToken cancellationToken = default)
		{
			if (userPushTokenDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Alerting User Writer role needed to write to this table, or Alerting Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Alerting User Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != userPushTokenDTO.id)
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


			IQueryable<Database.UserPushToken> query = (from x in _context.UserPushTokens
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserPushToken existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.UserPushToken PUT", id.ToString(), new Exception("No Alerting.UserPushToken entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (userPushTokenDTO.objectGuid == Guid.Empty)
            {
                userPushTokenDTO.objectGuid = existing.objectGuid;
            }
            else if (userPushTokenDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a UserPushToken record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.UserPushToken cloneOfExisting = (Database.UserPushToken)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new UserPushToken object using the data from the existing record, updated with what is in the DTO.
			//
			Database.UserPushToken userPushToken = (Database.UserPushToken)_context.Entry(existing).GetDatabaseValues().ToObject();
			userPushToken.ApplyDTO(userPushTokenDTO);
			//
			// The tenant guid for any UserPushToken being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the UserPushToken because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				userPushToken.tenantGuid = existing.tenantGuid;
			}

			lock (userPushTokenPutSyncRoot)
			{
				//
				// Validate the version number for the userPushToken being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != userPushToken.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "UserPushToken save attempt was made but save request was with version " + userPushToken.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The UserPushToken you are trying to update has already changed.  Please try your save again after reloading the UserPushToken.");
				}
				else
				{
					// Same record.  Increase version.
					userPushToken.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (userPushToken.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Alerting.UserPushToken record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (userPushToken.fcmToken != null && userPushToken.fcmToken.Length > 500)
				{
					userPushToken.fcmToken = userPushToken.fcmToken.Substring(0, 500);
				}

				if (userPushToken.deviceFingerprint != null && userPushToken.deviceFingerprint.Length > 100)
				{
					userPushToken.deviceFingerprint = userPushToken.deviceFingerprint.Substring(0, 100);
				}

				if (userPushToken.platform != null && userPushToken.platform.Length > 50)
				{
					userPushToken.platform = userPushToken.platform.Substring(0, 50);
				}

				if (userPushToken.userAgent != null && userPushToken.userAgent.Length > 500)
				{
					userPushToken.userAgent = userPushToken.userAgent.Substring(0, 500);
				}

				if (userPushToken.registeredAt.Kind != DateTimeKind.Utc)
				{
					userPushToken.registeredAt = userPushToken.registeredAt.ToUniversalTime();
				}

				if (userPushToken.lastUpdatedAt.Kind != DateTimeKind.Utc)
				{
					userPushToken.lastUpdatedAt = userPushToken.lastUpdatedAt.ToUniversalTime();
				}

				try
				{
				    EntityEntry<Database.UserPushToken> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(userPushToken);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        UserPushTokenChangeHistory userPushTokenChangeHistory = new UserPushTokenChangeHistory();
				        userPushTokenChangeHistory.userPushTokenId = userPushToken.id;
				        userPushTokenChangeHistory.versionNumber = userPushToken.versionNumber;
				        userPushTokenChangeHistory.timeStamp = DateTime.UtcNow;
				        userPushTokenChangeHistory.userId = securityUser.id;
				        userPushTokenChangeHistory.tenantGuid = userTenantGuid;
				        userPushTokenChangeHistory.data = JsonSerializer.Serialize(Database.UserPushToken.CreateAnonymousWithFirstLevelSubObjects(userPushToken));
				        _context.UserPushTokenChangeHistories.Add(userPushTokenChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.UserPushToken entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserPushToken.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserPushToken.CreateAnonymousWithFirstLevelSubObjects(userPushToken)),
						null);

				return Ok(Database.UserPushToken.CreateAnonymous(userPushToken));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.UserPushToken entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserPushToken.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserPushToken.CreateAnonymousWithFirstLevelSubObjects(userPushToken)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new UserPushToken record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserPushToken", Name = "UserPushToken")]
		public async Task<IActionResult> PostUserPushToken([FromBody]Database.UserPushToken.UserPushTokenDTO userPushTokenDTO, CancellationToken cancellationToken = default)
		{
			if (userPushTokenDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Alerting User Writer role needed to write to this table, or Alerting Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Alerting User Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new UserPushToken object using the data from the DTO
			//
			Database.UserPushToken userPushToken = Database.UserPushToken.FromDTO(userPushTokenDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				userPushToken.tenantGuid = userTenantGuid;

				if (userPushToken.fcmToken != null && userPushToken.fcmToken.Length > 500)
				{
					userPushToken.fcmToken = userPushToken.fcmToken.Substring(0, 500);
				}

				if (userPushToken.deviceFingerprint != null && userPushToken.deviceFingerprint.Length > 100)
				{
					userPushToken.deviceFingerprint = userPushToken.deviceFingerprint.Substring(0, 100);
				}

				if (userPushToken.platform != null && userPushToken.platform.Length > 50)
				{
					userPushToken.platform = userPushToken.platform.Substring(0, 50);
				}

				if (userPushToken.userAgent != null && userPushToken.userAgent.Length > 500)
				{
					userPushToken.userAgent = userPushToken.userAgent.Substring(0, 500);
				}

				if (userPushToken.registeredAt.Kind != DateTimeKind.Utc)
				{
					userPushToken.registeredAt = userPushToken.registeredAt.ToUniversalTime();
				}

				if (userPushToken.lastUpdatedAt.Kind != DateTimeKind.Utc)
				{
					userPushToken.lastUpdatedAt = userPushToken.lastUpdatedAt.ToUniversalTime();
				}

				userPushToken.objectGuid = Guid.NewGuid();
				userPushToken.versionNumber = 1;

				_context.UserPushTokens.Add(userPushToken);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the userPushToken object so that no further changes will be written to the database
				    //
				    _context.Entry(userPushToken).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					userPushToken.UserPushTokenChangeHistories = null;


				    UserPushTokenChangeHistory userPushTokenChangeHistory = new UserPushTokenChangeHistory();
				    userPushTokenChangeHistory.userPushTokenId = userPushToken.id;
				    userPushTokenChangeHistory.versionNumber = userPushToken.versionNumber;
				    userPushTokenChangeHistory.timeStamp = DateTime.UtcNow;
				    userPushTokenChangeHistory.userId = securityUser.id;
				    userPushTokenChangeHistory.tenantGuid = userTenantGuid;
				    userPushTokenChangeHistory.data = JsonSerializer.Serialize(Database.UserPushToken.CreateAnonymousWithFirstLevelSubObjects(userPushToken));
				    _context.UserPushTokenChangeHistories.Add(userPushTokenChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Alerting.UserPushToken entity successfully created.",
						true,
						userPushToken. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.UserPushToken.CreateAnonymousWithFirstLevelSubObjects(userPushToken)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Alerting.UserPushToken entity creation failed.", false, userPushToken.id.ToString(), "", JsonSerializer.Serialize(Database.UserPushToken.CreateAnonymousWithFirstLevelSubObjects(userPushToken)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserPushToken", userPushToken.id, userPushToken.fcmToken));

			return CreatedAtRoute("UserPushToken", new { id = userPushToken.id }, Database.UserPushToken.CreateAnonymousWithFirstLevelSubObjects(userPushToken));
		}



        /// <summary>
        /// 
        /// This rolls a UserPushToken entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserPushToken/Rollback/{id}")]
		[Route("api/UserPushToken/Rollback")]
		public async Task<IActionResult> RollbackToUserPushTokenVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.UserPushToken> query = (from x in _context.UserPushTokens
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this UserPushToken concurrently
			//
			lock (userPushTokenPutSyncRoot)
			{
				
				Database.UserPushToken userPushToken = query.FirstOrDefault();
				
				if (userPushToken == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.UserPushToken rollback", id.ToString(), new Exception("No Alerting.UserPushToken entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the UserPushToken current state so we can log it.
				//
				Database.UserPushToken cloneOfExisting = (Database.UserPushToken)_context.Entry(userPushToken).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.UserPushTokenChangeHistories = null;

				if (versionNumber >= userPushToken.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Alerting.UserPushToken rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Alerting.UserPushToken rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				UserPushTokenChangeHistory userPushTokenChangeHistory = (from x in _context.UserPushTokenChangeHistories
				                                               where
				                                               x.userPushTokenId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (userPushTokenChangeHistory != null)
				{
				    Database.UserPushToken oldUserPushToken = JsonSerializer.Deserialize<Database.UserPushToken>(userPushTokenChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    userPushToken.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    userPushToken.userObjectGuid = oldUserPushToken.userObjectGuid;
				    userPushToken.fcmToken = oldUserPushToken.fcmToken;
				    userPushToken.deviceFingerprint = oldUserPushToken.deviceFingerprint;
				    userPushToken.platform = oldUserPushToken.platform;
				    userPushToken.userAgent = oldUserPushToken.userAgent;
				    userPushToken.registeredAt = oldUserPushToken.registeredAt;
				    userPushToken.lastUpdatedAt = oldUserPushToken.lastUpdatedAt;
				    userPushToken.objectGuid = oldUserPushToken.objectGuid;
				    userPushToken.active = oldUserPushToken.active;
				    userPushToken.deleted = oldUserPushToken.deleted;

				    string serializedUserPushToken = JsonSerializer.Serialize(userPushToken);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        UserPushTokenChangeHistory newUserPushTokenChangeHistory = new UserPushTokenChangeHistory();
				        newUserPushTokenChangeHistory.userPushTokenId = userPushToken.id;
				        newUserPushTokenChangeHistory.versionNumber = userPushToken.versionNumber;
				        newUserPushTokenChangeHistory.timeStamp = DateTime.UtcNow;
				        newUserPushTokenChangeHistory.userId = securityUser.id;
				        newUserPushTokenChangeHistory.tenantGuid = userTenantGuid;
				        newUserPushTokenChangeHistory.data = JsonSerializer.Serialize(Database.UserPushToken.CreateAnonymousWithFirstLevelSubObjects(userPushToken));
				        _context.UserPushTokenChangeHistories.Add(newUserPushTokenChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.UserPushToken rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserPushToken.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserPushToken.CreateAnonymousWithFirstLevelSubObjects(userPushToken)),
						null);


				    return Ok(Database.UserPushToken.CreateAnonymous(userPushToken));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Alerting.UserPushToken rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Alerting.UserPushToken rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a UserPushToken.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the UserPushToken</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserPushToken/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetUserPushTokenChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.UserPushToken userPushToken = await _context.UserPushTokens.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (userPushToken == null)
			{
				return NotFound();
			}

			try
			{
				userPushToken.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.UserPushToken> versionInfo = await userPushToken.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a UserPushToken.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the UserPushToken</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserPushToken/{id}/AuditHistory")]
		public async Task<IActionResult> GetUserPushTokenAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.UserPushToken userPushToken = await _context.UserPushTokens.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (userPushToken == null)
			{
				return NotFound();
			}

			try
			{
				userPushToken.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.UserPushToken>> versions = await userPushToken.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a UserPushToken.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the UserPushToken</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The UserPushToken object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserPushToken/{id}/Version/{version}")]
		public async Task<IActionResult> GetUserPushTokenVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.UserPushToken userPushToken = await _context.UserPushTokens.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (userPushToken == null)
			{
				return NotFound();
			}

			try
			{
				userPushToken.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.UserPushToken> versionInfo = await userPushToken.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a UserPushToken at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the UserPushToken</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The UserPushToken object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserPushToken/{id}/StateAtTime")]
		public async Task<IActionResult> GetUserPushTokenStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.UserPushToken userPushToken = await _context.UserPushTokens.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (userPushToken == null)
			{
				return NotFound();
			}

			try
			{
				userPushToken.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.UserPushToken> versionInfo = await userPushToken.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a UserPushToken record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserPushToken/{id}")]
		[Route("api/UserPushToken")]
		public async Task<IActionResult> DeleteUserPushToken(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Alerting User Writer role needed to write to this table, or Alerting Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Alerting User Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.UserPushToken> query = (from x in _context.UserPushTokens
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserPushToken userPushToken = await query.FirstOrDefaultAsync(cancellationToken);

			if (userPushToken == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.UserPushToken DELETE", id.ToString(), new Exception("No Alerting.UserPushToken entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.UserPushToken cloneOfExisting = (Database.UserPushToken)_context.Entry(userPushToken).GetDatabaseValues().ToObject();


			lock (userPushTokenDeleteSyncRoot)
			{
			    try
			    {
			        userPushToken.deleted = true;
			        userPushToken.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        UserPushTokenChangeHistory userPushTokenChangeHistory = new UserPushTokenChangeHistory();
			        userPushTokenChangeHistory.userPushTokenId = userPushToken.id;
			        userPushTokenChangeHistory.versionNumber = userPushToken.versionNumber;
			        userPushTokenChangeHistory.timeStamp = DateTime.UtcNow;
			        userPushTokenChangeHistory.userId = securityUser.id;
			        userPushTokenChangeHistory.tenantGuid = userTenantGuid;
			        userPushTokenChangeHistory.data = JsonSerializer.Serialize(Database.UserPushToken.CreateAnonymousWithFirstLevelSubObjects(userPushToken));
			        _context.UserPushTokenChangeHistories.Add(userPushTokenChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Alerting.UserPushToken entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserPushToken.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserPushToken.CreateAnonymousWithFirstLevelSubObjects(userPushToken)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Alerting.UserPushToken entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserPushToken.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserPushToken.CreateAnonymousWithFirstLevelSubObjects(userPushToken)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of UserPushToken records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/UserPushTokens/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			Guid? userObjectGuid = null,
			string fcmToken = null,
			string deviceFingerprint = null,
			string platform = null,
			string userAgent = null,
			DateTime? registeredAt = null,
			DateTime? lastUpdatedAt = null,
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
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
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
			if (registeredAt.HasValue == true && registeredAt.Value.Kind != DateTimeKind.Utc)
			{
				registeredAt = registeredAt.Value.ToUniversalTime();
			}

			if (lastUpdatedAt.HasValue == true && lastUpdatedAt.Value.Kind != DateTimeKind.Utc)
			{
				lastUpdatedAt = lastUpdatedAt.Value.ToUniversalTime();
			}

			IQueryable<Database.UserPushToken> query = (from upt in _context.UserPushTokens select upt);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userObjectGuid.HasValue == true)
			{
				query = query.Where(upt => upt.userObjectGuid == userObjectGuid);
			}
			if (string.IsNullOrEmpty(fcmToken) == false)
			{
				query = query.Where(upt => upt.fcmToken == fcmToken);
			}
			if (string.IsNullOrEmpty(deviceFingerprint) == false)
			{
				query = query.Where(upt => upt.deviceFingerprint == deviceFingerprint);
			}
			if (string.IsNullOrEmpty(platform) == false)
			{
				query = query.Where(upt => upt.platform == platform);
			}
			if (string.IsNullOrEmpty(userAgent) == false)
			{
				query = query.Where(upt => upt.userAgent == userAgent);
			}
			if (registeredAt.HasValue == true)
			{
				query = query.Where(upt => upt.registeredAt == registeredAt.Value);
			}
			if (lastUpdatedAt.HasValue == true)
			{
				query = query.Where(upt => upt.lastUpdatedAt == lastUpdatedAt.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(upt => upt.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(upt => upt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(upt => upt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(upt => upt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(upt => upt.deleted == false);
				}
			}
			else
			{
				query = query.Where(upt => upt.active == true);
				query = query.Where(upt => upt.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Push Token, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.fcmToken.Contains(anyStringContains)
			       || x.deviceFingerprint.Contains(anyStringContains)
			       || x.platform.Contains(anyStringContains)
			       || x.userAgent.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.fcmToken).ThenBy(x => x.deviceFingerprint).ThenBy(x => x.platform);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.UserPushToken.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/UserPushToken/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Alerting User Writer role needed to write to this table, or Alerting Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Alerting User Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
