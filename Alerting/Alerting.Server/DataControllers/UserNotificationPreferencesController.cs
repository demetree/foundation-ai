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
    /// This auto generated class provides the basic CRUD operations for the UserNotificationPreference entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the UserNotificationPreference entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class UserNotificationPreferencesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object userNotificationPreferencePutSyncRoot = new object();
		static object userNotificationPreferenceDeleteSyncRoot = new object();

		private AlertingContext _context;

		private ILogger<UserNotificationPreferencesController> _logger;

		public UserNotificationPreferencesController(AlertingContext context, ILogger<UserNotificationPreferencesController> logger) : base("Alerting", "UserNotificationPreference")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of UserNotificationPreferences filtered by the parameters provided.
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
		[Route("api/UserNotificationPreferences")]
		public async Task<IActionResult> GetUserNotificationPreferences(
			Guid? securityUserObjectGuid = null,
			string timeZoneId = null,
			string quietHoursStart = null,
			string quietHoursEnd = null,
			bool? isDoNotDisturb = null,
			bool? isDoNotDisturbPermanent = null,
			DateTime? doNotDisturbUntil = null,
			string customSettingsJson = null,
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
			if (doNotDisturbUntil.HasValue == true && doNotDisturbUntil.Value.Kind != DateTimeKind.Utc)
			{
				doNotDisturbUntil = doNotDisturbUntil.Value.ToUniversalTime();
			}

			IQueryable<Database.UserNotificationPreference> query = (from unp in _context.UserNotificationPreferences select unp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (securityUserObjectGuid.HasValue == true)
			{
				query = query.Where(unp => unp.securityUserObjectGuid == securityUserObjectGuid);
			}
			if (string.IsNullOrEmpty(timeZoneId) == false)
			{
				query = query.Where(unp => unp.timeZoneId == timeZoneId);
			}
			if (string.IsNullOrEmpty(quietHoursStart) == false)
			{
				query = query.Where(unp => unp.quietHoursStart == quietHoursStart);
			}
			if (string.IsNullOrEmpty(quietHoursEnd) == false)
			{
				query = query.Where(unp => unp.quietHoursEnd == quietHoursEnd);
			}
			if (isDoNotDisturb.HasValue == true)
			{
				query = query.Where(unp => unp.isDoNotDisturb == isDoNotDisturb.Value);
			}
			if (isDoNotDisturbPermanent.HasValue == true)
			{
				query = query.Where(unp => unp.isDoNotDisturbPermanent == isDoNotDisturbPermanent.Value);
			}
			if (doNotDisturbUntil.HasValue == true)
			{
				query = query.Where(unp => unp.doNotDisturbUntil == doNotDisturbUntil.Value);
			}
			if (string.IsNullOrEmpty(customSettingsJson) == false)
			{
				query = query.Where(unp => unp.customSettingsJson == customSettingsJson);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(unp => unp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(unp => unp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(unp => unp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(unp => unp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(unp => unp.deleted == false);
				}
			}
			else
			{
				query = query.Where(unp => unp.active == true);
				query = query.Where(unp => unp.deleted == false);
			}

			query = query.OrderBy(unp => unp.timeZoneId).ThenBy(unp => unp.quietHoursStart).ThenBy(unp => unp.quietHoursEnd);

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
			// Add the any string contains parameter to span all the string fields on the User Notification Preference, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.timeZoneId.Contains(anyStringContains)
			       || x.quietHoursStart.Contains(anyStringContains)
			       || x.quietHoursEnd.Contains(anyStringContains)
			       || x.customSettingsJson.Contains(anyStringContains)
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.UserNotificationPreference> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.UserNotificationPreference userNotificationPreference in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(userNotificationPreference, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Alerting.UserNotificationPreference Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Alerting.UserNotificationPreference Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of UserNotificationPreferences filtered by the parameters provided.  Its query is similar to the GetUserNotificationPreferences method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationPreferences/RowCount")]
		public async Task<IActionResult> GetRowCount(
			Guid? securityUserObjectGuid = null,
			string timeZoneId = null,
			string quietHoursStart = null,
			string quietHoursEnd = null,
			bool? isDoNotDisturb = null,
			bool? isDoNotDisturbPermanent = null,
			DateTime? doNotDisturbUntil = null,
			string customSettingsJson = null,
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
			if (doNotDisturbUntil.HasValue == true && doNotDisturbUntil.Value.Kind != DateTimeKind.Utc)
			{
				doNotDisturbUntil = doNotDisturbUntil.Value.ToUniversalTime();
			}

			IQueryable<Database.UserNotificationPreference> query = (from unp in _context.UserNotificationPreferences select unp);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (securityUserObjectGuid.HasValue == true)
			{
				query = query.Where(unp => unp.securityUserObjectGuid == securityUserObjectGuid);
			}
			if (timeZoneId != null)
			{
				query = query.Where(unp => unp.timeZoneId == timeZoneId);
			}
			if (quietHoursStart != null)
			{
				query = query.Where(unp => unp.quietHoursStart == quietHoursStart);
			}
			if (quietHoursEnd != null)
			{
				query = query.Where(unp => unp.quietHoursEnd == quietHoursEnd);
			}
			if (isDoNotDisturb.HasValue == true)
			{
				query = query.Where(unp => unp.isDoNotDisturb == isDoNotDisturb.Value);
			}
			if (isDoNotDisturbPermanent.HasValue == true)
			{
				query = query.Where(unp => unp.isDoNotDisturbPermanent == isDoNotDisturbPermanent.Value);
			}
			if (doNotDisturbUntil.HasValue == true)
			{
				query = query.Where(unp => unp.doNotDisturbUntil == doNotDisturbUntil.Value);
			}
			if (customSettingsJson != null)
			{
				query = query.Where(unp => unp.customSettingsJson == customSettingsJson);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(unp => unp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(unp => unp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(unp => unp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(unp => unp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(unp => unp.deleted == false);
				}
			}
			else
			{
				query = query.Where(unp => unp.active == true);
				query = query.Where(unp => unp.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the User Notification Preference, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.timeZoneId.Contains(anyStringContains)
			       || x.quietHoursStart.Contains(anyStringContains)
			       || x.quietHoursEnd.Contains(anyStringContains)
			       || x.customSettingsJson.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single UserNotificationPreference by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationPreference/{id}")]
		public async Task<IActionResult> GetUserNotificationPreference(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.UserNotificationPreference> query = (from unp in _context.UserNotificationPreferences where
							(unp.id == id) &&
							(userIsAdmin == true || unp.deleted == false) &&
							(userIsWriter == true || unp.active == true)
					select unp);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.UserNotificationPreference materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Alerting.UserNotificationPreference Entity was read with Admin privilege." : "Alerting.UserNotificationPreference Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserNotificationPreference", materialized.id, materialized.timeZoneId));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Alerting.UserNotificationPreference entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Alerting.UserNotificationPreference.   Entity was read with Admin privilege." : "Exception caught during entity read of Alerting.UserNotificationPreference.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing UserNotificationPreference record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/UserNotificationPreference/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutUserNotificationPreference(int id, [FromBody]Database.UserNotificationPreference.UserNotificationPreferenceDTO userNotificationPreferenceDTO, CancellationToken cancellationToken = default)
		{
			if (userNotificationPreferenceDTO == null)
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



			if (id != userNotificationPreferenceDTO.id)
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


			IQueryable<Database.UserNotificationPreference> query = (from x in _context.UserNotificationPreferences
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserNotificationPreference existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.UserNotificationPreference PUT", id.ToString(), new Exception("No Alerting.UserNotificationPreference entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (userNotificationPreferenceDTO.objectGuid == Guid.Empty)
            {
                userNotificationPreferenceDTO.objectGuid = existing.objectGuid;
            }
            else if (userNotificationPreferenceDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a UserNotificationPreference record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.UserNotificationPreference cloneOfExisting = (Database.UserNotificationPreference)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new UserNotificationPreference object using the data from the existing record, updated with what is in the DTO.
			//
			Database.UserNotificationPreference userNotificationPreference = (Database.UserNotificationPreference)_context.Entry(existing).GetDatabaseValues().ToObject();
			userNotificationPreference.ApplyDTO(userNotificationPreferenceDTO);
			//
			// The tenant guid for any UserNotificationPreference being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the UserNotificationPreference because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				userNotificationPreference.tenantGuid = existing.tenantGuid;
			}

			lock (userNotificationPreferencePutSyncRoot)
			{
				//
				// Validate the version number for the userNotificationPreference being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != userNotificationPreference.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "UserNotificationPreference save attempt was made but save request was with version " + userNotificationPreference.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The UserNotificationPreference you are trying to update has already changed.  Please try your save again after reloading the UserNotificationPreference.");
				}
				else
				{
					// Same record.  Increase version.
					userNotificationPreference.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (userNotificationPreference.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Alerting.UserNotificationPreference record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (userNotificationPreference.timeZoneId != null && userNotificationPreference.timeZoneId.Length > 50)
				{
					userNotificationPreference.timeZoneId = userNotificationPreference.timeZoneId.Substring(0, 50);
				}

				if (userNotificationPreference.quietHoursStart != null && userNotificationPreference.quietHoursStart.Length > 10)
				{
					userNotificationPreference.quietHoursStart = userNotificationPreference.quietHoursStart.Substring(0, 10);
				}

				if (userNotificationPreference.quietHoursEnd != null && userNotificationPreference.quietHoursEnd.Length > 10)
				{
					userNotificationPreference.quietHoursEnd = userNotificationPreference.quietHoursEnd.Substring(0, 10);
				}

				if (userNotificationPreference.doNotDisturbUntil.HasValue == true && userNotificationPreference.doNotDisturbUntil.Value.Kind != DateTimeKind.Utc)
				{
					userNotificationPreference.doNotDisturbUntil = userNotificationPreference.doNotDisturbUntil.Value.ToUniversalTime();
				}

				try
				{
				    EntityEntry<Database.UserNotificationPreference> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(userNotificationPreference);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        UserNotificationPreferenceChangeHistory userNotificationPreferenceChangeHistory = new UserNotificationPreferenceChangeHistory();
				        userNotificationPreferenceChangeHistory.userNotificationPreferenceId = userNotificationPreference.id;
				        userNotificationPreferenceChangeHistory.versionNumber = userNotificationPreference.versionNumber;
				        userNotificationPreferenceChangeHistory.timeStamp = DateTime.UtcNow;
				        userNotificationPreferenceChangeHistory.userId = securityUser.id;
				        userNotificationPreferenceChangeHistory.tenantGuid = userTenantGuid;
				        userNotificationPreferenceChangeHistory.data = JsonSerializer.Serialize(Database.UserNotificationPreference.CreateAnonymousWithFirstLevelSubObjects(userNotificationPreference));
				        _context.UserNotificationPreferenceChangeHistories.Add(userNotificationPreferenceChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.UserNotificationPreference entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserNotificationPreference.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserNotificationPreference.CreateAnonymousWithFirstLevelSubObjects(userNotificationPreference)),
						null);

				return Ok(Database.UserNotificationPreference.CreateAnonymous(userNotificationPreference));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.UserNotificationPreference entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserNotificationPreference.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserNotificationPreference.CreateAnonymousWithFirstLevelSubObjects(userNotificationPreference)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new UserNotificationPreference record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationPreference", Name = "UserNotificationPreference")]
		public async Task<IActionResult> PostUserNotificationPreference([FromBody]Database.UserNotificationPreference.UserNotificationPreferenceDTO userNotificationPreferenceDTO, CancellationToken cancellationToken = default)
		{
			if (userNotificationPreferenceDTO == null)
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
			// Create a new UserNotificationPreference object using the data from the DTO
			//
			Database.UserNotificationPreference userNotificationPreference = Database.UserNotificationPreference.FromDTO(userNotificationPreferenceDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				userNotificationPreference.tenantGuid = userTenantGuid;

				if (userNotificationPreference.timeZoneId != null && userNotificationPreference.timeZoneId.Length > 50)
				{
					userNotificationPreference.timeZoneId = userNotificationPreference.timeZoneId.Substring(0, 50);
				}

				if (userNotificationPreference.quietHoursStart != null && userNotificationPreference.quietHoursStart.Length > 10)
				{
					userNotificationPreference.quietHoursStart = userNotificationPreference.quietHoursStart.Substring(0, 10);
				}

				if (userNotificationPreference.quietHoursEnd != null && userNotificationPreference.quietHoursEnd.Length > 10)
				{
					userNotificationPreference.quietHoursEnd = userNotificationPreference.quietHoursEnd.Substring(0, 10);
				}

				if (userNotificationPreference.doNotDisturbUntil.HasValue == true && userNotificationPreference.doNotDisturbUntil.Value.Kind != DateTimeKind.Utc)
				{
					userNotificationPreference.doNotDisturbUntil = userNotificationPreference.doNotDisturbUntil.Value.ToUniversalTime();
				}

				userNotificationPreference.objectGuid = Guid.NewGuid();
				userNotificationPreference.versionNumber = 1;

				_context.UserNotificationPreferences.Add(userNotificationPreference);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the userNotificationPreference object so that no further changes will be written to the database
				    //
				    _context.Entry(userNotificationPreference).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					userNotificationPreference.UserNotificationChannelPreferences = null;
					userNotificationPreference.UserNotificationPreferenceChangeHistories = null;


				    UserNotificationPreferenceChangeHistory userNotificationPreferenceChangeHistory = new UserNotificationPreferenceChangeHistory();
				    userNotificationPreferenceChangeHistory.userNotificationPreferenceId = userNotificationPreference.id;
				    userNotificationPreferenceChangeHistory.versionNumber = userNotificationPreference.versionNumber;
				    userNotificationPreferenceChangeHistory.timeStamp = DateTime.UtcNow;
				    userNotificationPreferenceChangeHistory.userId = securityUser.id;
				    userNotificationPreferenceChangeHistory.tenantGuid = userTenantGuid;
				    userNotificationPreferenceChangeHistory.data = JsonSerializer.Serialize(Database.UserNotificationPreference.CreateAnonymousWithFirstLevelSubObjects(userNotificationPreference));
				    _context.UserNotificationPreferenceChangeHistories.Add(userNotificationPreferenceChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Alerting.UserNotificationPreference entity successfully created.",
						true,
						userNotificationPreference. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.UserNotificationPreference.CreateAnonymousWithFirstLevelSubObjects(userNotificationPreference)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Alerting.UserNotificationPreference entity creation failed.", false, userNotificationPreference.id.ToString(), "", JsonSerializer.Serialize(Database.UserNotificationPreference.CreateAnonymousWithFirstLevelSubObjects(userNotificationPreference)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserNotificationPreference", userNotificationPreference.id, userNotificationPreference.timeZoneId));

			return CreatedAtRoute("UserNotificationPreference", new { id = userNotificationPreference.id }, Database.UserNotificationPreference.CreateAnonymousWithFirstLevelSubObjects(userNotificationPreference));
		}



        /// <summary>
        /// 
        /// This rolls a UserNotificationPreference entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationPreference/Rollback/{id}")]
		[Route("api/UserNotificationPreference/Rollback")]
		public async Task<IActionResult> RollbackToUserNotificationPreferenceVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.UserNotificationPreference> query = (from x in _context.UserNotificationPreferences
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this UserNotificationPreference concurrently
			//
			lock (userNotificationPreferencePutSyncRoot)
			{
				
				Database.UserNotificationPreference userNotificationPreference = query.FirstOrDefault();
				
				if (userNotificationPreference == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.UserNotificationPreference rollback", id.ToString(), new Exception("No Alerting.UserNotificationPreference entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the UserNotificationPreference current state so we can log it.
				//
				Database.UserNotificationPreference cloneOfExisting = (Database.UserNotificationPreference)_context.Entry(userNotificationPreference).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.UserNotificationChannelPreferences = null;
				cloneOfExisting.UserNotificationPreferenceChangeHistories = null;

				if (versionNumber >= userNotificationPreference.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Alerting.UserNotificationPreference rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Alerting.UserNotificationPreference rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				UserNotificationPreferenceChangeHistory userNotificationPreferenceChangeHistory = (from x in _context.UserNotificationPreferenceChangeHistories
				                                               where
				                                               x.userNotificationPreferenceId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (userNotificationPreferenceChangeHistory != null)
				{
				    Database.UserNotificationPreference oldUserNotificationPreference = JsonSerializer.Deserialize<Database.UserNotificationPreference>(userNotificationPreferenceChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    userNotificationPreference.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    userNotificationPreference.securityUserObjectGuid = oldUserNotificationPreference.securityUserObjectGuid;
				    userNotificationPreference.timeZoneId = oldUserNotificationPreference.timeZoneId;
				    userNotificationPreference.quietHoursStart = oldUserNotificationPreference.quietHoursStart;
				    userNotificationPreference.quietHoursEnd = oldUserNotificationPreference.quietHoursEnd;
				    userNotificationPreference.isDoNotDisturb = oldUserNotificationPreference.isDoNotDisturb;
				    userNotificationPreference.isDoNotDisturbPermanent = oldUserNotificationPreference.isDoNotDisturbPermanent;
				    userNotificationPreference.doNotDisturbUntil = oldUserNotificationPreference.doNotDisturbUntil;
				    userNotificationPreference.customSettingsJson = oldUserNotificationPreference.customSettingsJson;
				    userNotificationPreference.objectGuid = oldUserNotificationPreference.objectGuid;
				    userNotificationPreference.active = oldUserNotificationPreference.active;
				    userNotificationPreference.deleted = oldUserNotificationPreference.deleted;

				    string serializedUserNotificationPreference = JsonSerializer.Serialize(userNotificationPreference);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        UserNotificationPreferenceChangeHistory newUserNotificationPreferenceChangeHistory = new UserNotificationPreferenceChangeHistory();
				        newUserNotificationPreferenceChangeHistory.userNotificationPreferenceId = userNotificationPreference.id;
				        newUserNotificationPreferenceChangeHistory.versionNumber = userNotificationPreference.versionNumber;
				        newUserNotificationPreferenceChangeHistory.timeStamp = DateTime.UtcNow;
				        newUserNotificationPreferenceChangeHistory.userId = securityUser.id;
				        newUserNotificationPreferenceChangeHistory.tenantGuid = userTenantGuid;
				        newUserNotificationPreferenceChangeHistory.data = JsonSerializer.Serialize(Database.UserNotificationPreference.CreateAnonymousWithFirstLevelSubObjects(userNotificationPreference));
				        _context.UserNotificationPreferenceChangeHistories.Add(newUserNotificationPreferenceChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.UserNotificationPreference rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserNotificationPreference.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserNotificationPreference.CreateAnonymousWithFirstLevelSubObjects(userNotificationPreference)),
						null);


				    return Ok(Database.UserNotificationPreference.CreateAnonymous(userNotificationPreference));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Alerting.UserNotificationPreference rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Alerting.UserNotificationPreference rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a UserNotificationPreference.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the UserNotificationPreference</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationPreference/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetUserNotificationPreferenceChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.UserNotificationPreference userNotificationPreference = await _context.UserNotificationPreferences.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (userNotificationPreference == null)
			{
				return NotFound();
			}

			try
			{
				userNotificationPreference.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.UserNotificationPreference> versionInfo = await userNotificationPreference.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a UserNotificationPreference.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the UserNotificationPreference</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationPreference/{id}/AuditHistory")]
		public async Task<IActionResult> GetUserNotificationPreferenceAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.UserNotificationPreference userNotificationPreference = await _context.UserNotificationPreferences.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (userNotificationPreference == null)
			{
				return NotFound();
			}

			try
			{
				userNotificationPreference.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.UserNotificationPreference>> versions = await userNotificationPreference.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a UserNotificationPreference.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the UserNotificationPreference</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The UserNotificationPreference object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationPreference/{id}/Version/{version}")]
		public async Task<IActionResult> GetUserNotificationPreferenceVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.UserNotificationPreference userNotificationPreference = await _context.UserNotificationPreferences.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (userNotificationPreference == null)
			{
				return NotFound();
			}

			try
			{
				userNotificationPreference.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.UserNotificationPreference> versionInfo = await userNotificationPreference.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a UserNotificationPreference at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the UserNotificationPreference</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The UserNotificationPreference object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationPreference/{id}/StateAtTime")]
		public async Task<IActionResult> GetUserNotificationPreferenceStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.UserNotificationPreference userNotificationPreference = await _context.UserNotificationPreferences.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (userNotificationPreference == null)
			{
				return NotFound();
			}

			try
			{
				userNotificationPreference.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.UserNotificationPreference> versionInfo = await userNotificationPreference.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a UserNotificationPreference record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationPreference/{id}")]
		[Route("api/UserNotificationPreference")]
		public async Task<IActionResult> DeleteUserNotificationPreference(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.UserNotificationPreference> query = (from x in _context.UserNotificationPreferences
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserNotificationPreference userNotificationPreference = await query.FirstOrDefaultAsync(cancellationToken);

			if (userNotificationPreference == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.UserNotificationPreference DELETE", id.ToString(), new Exception("No Alerting.UserNotificationPreference entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.UserNotificationPreference cloneOfExisting = (Database.UserNotificationPreference)_context.Entry(userNotificationPreference).GetDatabaseValues().ToObject();


			lock (userNotificationPreferenceDeleteSyncRoot)
			{
			    try
			    {
			        userNotificationPreference.deleted = true;
			        userNotificationPreference.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        UserNotificationPreferenceChangeHistory userNotificationPreferenceChangeHistory = new UserNotificationPreferenceChangeHistory();
			        userNotificationPreferenceChangeHistory.userNotificationPreferenceId = userNotificationPreference.id;
			        userNotificationPreferenceChangeHistory.versionNumber = userNotificationPreference.versionNumber;
			        userNotificationPreferenceChangeHistory.timeStamp = DateTime.UtcNow;
			        userNotificationPreferenceChangeHistory.userId = securityUser.id;
			        userNotificationPreferenceChangeHistory.tenantGuid = userTenantGuid;
			        userNotificationPreferenceChangeHistory.data = JsonSerializer.Serialize(Database.UserNotificationPreference.CreateAnonymousWithFirstLevelSubObjects(userNotificationPreference));
			        _context.UserNotificationPreferenceChangeHistories.Add(userNotificationPreferenceChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Alerting.UserNotificationPreference entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserNotificationPreference.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserNotificationPreference.CreateAnonymousWithFirstLevelSubObjects(userNotificationPreference)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Alerting.UserNotificationPreference entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserNotificationPreference.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserNotificationPreference.CreateAnonymousWithFirstLevelSubObjects(userNotificationPreference)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of UserNotificationPreference records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/UserNotificationPreferences/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			Guid? securityUserObjectGuid = null,
			string timeZoneId = null,
			string quietHoursStart = null,
			string quietHoursEnd = null,
			bool? isDoNotDisturb = null,
			bool? isDoNotDisturbPermanent = null,
			DateTime? doNotDisturbUntil = null,
			string customSettingsJson = null,
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
			if (doNotDisturbUntil.HasValue == true && doNotDisturbUntil.Value.Kind != DateTimeKind.Utc)
			{
				doNotDisturbUntil = doNotDisturbUntil.Value.ToUniversalTime();
			}

			IQueryable<Database.UserNotificationPreference> query = (from unp in _context.UserNotificationPreferences select unp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (securityUserObjectGuid.HasValue == true)
			{
				query = query.Where(unp => unp.securityUserObjectGuid == securityUserObjectGuid);
			}
			if (string.IsNullOrEmpty(timeZoneId) == false)
			{
				query = query.Where(unp => unp.timeZoneId == timeZoneId);
			}
			if (string.IsNullOrEmpty(quietHoursStart) == false)
			{
				query = query.Where(unp => unp.quietHoursStart == quietHoursStart);
			}
			if (string.IsNullOrEmpty(quietHoursEnd) == false)
			{
				query = query.Where(unp => unp.quietHoursEnd == quietHoursEnd);
			}
			if (isDoNotDisturb.HasValue == true)
			{
				query = query.Where(unp => unp.isDoNotDisturb == isDoNotDisturb.Value);
			}
			if (isDoNotDisturbPermanent.HasValue == true)
			{
				query = query.Where(unp => unp.isDoNotDisturbPermanent == isDoNotDisturbPermanent.Value);
			}
			if (doNotDisturbUntil.HasValue == true)
			{
				query = query.Where(unp => unp.doNotDisturbUntil == doNotDisturbUntil.Value);
			}
			if (string.IsNullOrEmpty(customSettingsJson) == false)
			{
				query = query.Where(unp => unp.customSettingsJson == customSettingsJson);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(unp => unp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(unp => unp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(unp => unp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(unp => unp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(unp => unp.deleted == false);
				}
			}
			else
			{
				query = query.Where(unp => unp.active == true);
				query = query.Where(unp => unp.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Notification Preference, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.timeZoneId.Contains(anyStringContains)
			       || x.quietHoursStart.Contains(anyStringContains)
			       || x.quietHoursEnd.Contains(anyStringContains)
			       || x.customSettingsJson.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.timeZoneId).ThenBy(x => x.quietHoursStart).ThenBy(x => x.quietHoursEnd);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.UserNotificationPreference.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/UserNotificationPreference/CreateAuditEvent")]
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
