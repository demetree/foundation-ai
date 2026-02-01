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
    /// This auto generated class provides the basic CRUD operations for the UserNotificationChannelPreference entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the UserNotificationChannelPreference entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class UserNotificationChannelPreferencesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		static object userNotificationChannelPreferencePutSyncRoot = new object();
		static object userNotificationChannelPreferenceDeleteSyncRoot = new object();

		private AlertingContext _context;

		private ILogger<UserNotificationChannelPreferencesController> _logger;

		public UserNotificationChannelPreferencesController(AlertingContext context, ILogger<UserNotificationChannelPreferencesController> logger) : base("Alerting", "UserNotificationChannelPreference")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of UserNotificationChannelPreferences filtered by the parameters provided.
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
		[Route("api/UserNotificationChannelPreferences")]
		public async Task<IActionResult> GetUserNotificationChannelPreferences(
			int? userNotificationPreferenceId = null,
			int? notificationChannelTypeId = null,
			bool? isEnabled = null,
			int? priorityOverride = null,
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

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
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

			IQueryable<Database.UserNotificationChannelPreference> query = (from uncp in _context.UserNotificationChannelPreferences select uncp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userNotificationPreferenceId.HasValue == true)
			{
				query = query.Where(uncp => uncp.userNotificationPreferenceId == userNotificationPreferenceId.Value);
			}
			if (notificationChannelTypeId.HasValue == true)
			{
				query = query.Where(uncp => uncp.notificationChannelTypeId == notificationChannelTypeId.Value);
			}
			if (isEnabled.HasValue == true)
			{
				query = query.Where(uncp => uncp.isEnabled == isEnabled.Value);
			}
			if (priorityOverride.HasValue == true)
			{
				query = query.Where(uncp => uncp.priorityOverride == priorityOverride.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(uncp => uncp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(uncp => uncp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(uncp => uncp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(uncp => uncp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(uncp => uncp.deleted == false);
				}
			}
			else
			{
				query = query.Where(uncp => uncp.active == true);
				query = query.Where(uncp => uncp.deleted == false);
			}

			query = query.OrderBy(uncp => uncp.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.notificationChannelType);
				query = query.Include(x => x.userNotificationPreference);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Notification Channel Preference, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       (includeRelations == true && x.notificationChannelType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.notificationChannelType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.userNotificationPreference.timeZoneId.Contains(anyStringContains))
			       || (includeRelations == true && x.userNotificationPreference.quietHoursStart.Contains(anyStringContains))
			       || (includeRelations == true && x.userNotificationPreference.quietHoursEnd.Contains(anyStringContains))
			       || (includeRelations == true && x.userNotificationPreference.customSettingsJson.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.UserNotificationChannelPreference> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.UserNotificationChannelPreference userNotificationChannelPreference in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(userNotificationChannelPreference, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Alerting.UserNotificationChannelPreference Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Alerting.UserNotificationChannelPreference Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of UserNotificationChannelPreferences filtered by the parameters provided.  Its query is similar to the GetUserNotificationChannelPreferences method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationChannelPreferences/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? userNotificationPreferenceId = null,
			int? notificationChannelTypeId = null,
			bool? isEnabled = null,
			int? priorityOverride = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
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


			IQueryable<Database.UserNotificationChannelPreference> query = (from uncp in _context.UserNotificationChannelPreferences select uncp);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (userNotificationPreferenceId.HasValue == true)
			{
				query = query.Where(uncp => uncp.userNotificationPreferenceId == userNotificationPreferenceId.Value);
			}
			if (notificationChannelTypeId.HasValue == true)
			{
				query = query.Where(uncp => uncp.notificationChannelTypeId == notificationChannelTypeId.Value);
			}
			if (isEnabled.HasValue == true)
			{
				query = query.Where(uncp => uncp.isEnabled == isEnabled.Value);
			}
			if (priorityOverride.HasValue == true)
			{
				query = query.Where(uncp => uncp.priorityOverride == priorityOverride.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(uncp => uncp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(uncp => uncp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(uncp => uncp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(uncp => uncp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(uncp => uncp.deleted == false);
				}
			}
			else
			{
				query = query.Where(uncp => uncp.active == true);
				query = query.Where(uncp => uncp.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the User Notification Channel Preference, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.notificationChannelType.name.Contains(anyStringContains)
			       || x.notificationChannelType.description.Contains(anyStringContains)
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
        /// This gets a single UserNotificationChannelPreference by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationChannelPreference/{id}")]
		public async Task<IActionResult> GetUserNotificationChannelPreference(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
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
				IQueryable<Database.UserNotificationChannelPreference> query = (from uncp in _context.UserNotificationChannelPreferences where
							(uncp.id == id) &&
							(userIsAdmin == true || uncp.deleted == false) &&
							(userIsWriter == true || uncp.active == true)
					select uncp);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.notificationChannelType);
					query = query.Include(x => x.userNotificationPreference);
					query = query.AsSplitQuery();
				}

				Database.UserNotificationChannelPreference materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Alerting.UserNotificationChannelPreference Entity was read with Admin privilege." : "Alerting.UserNotificationChannelPreference Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserNotificationChannelPreference", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Alerting.UserNotificationChannelPreference entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Alerting.UserNotificationChannelPreference.   Entity was read with Admin privilege." : "Exception caught during entity read of Alerting.UserNotificationChannelPreference.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing UserNotificationChannelPreference record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/UserNotificationChannelPreference/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutUserNotificationChannelPreference(int id, [FromBody]Database.UserNotificationChannelPreference.UserNotificationChannelPreferenceDTO userNotificationChannelPreferenceDTO, CancellationToken cancellationToken = default)
		{
			if (userNotificationChannelPreferenceDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != userNotificationChannelPreferenceDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
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


			IQueryable<Database.UserNotificationChannelPreference> query = (from x in _context.UserNotificationChannelPreferences
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserNotificationChannelPreference existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.UserNotificationChannelPreference PUT", id.ToString(), new Exception("No Alerting.UserNotificationChannelPreference entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (userNotificationChannelPreferenceDTO.objectGuid == Guid.Empty)
            {
                userNotificationChannelPreferenceDTO.objectGuid = existing.objectGuid;
            }
            else if (userNotificationChannelPreferenceDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a UserNotificationChannelPreference record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.UserNotificationChannelPreference cloneOfExisting = (Database.UserNotificationChannelPreference)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new UserNotificationChannelPreference object using the data from the existing record, updated with what is in the DTO.
			//
			Database.UserNotificationChannelPreference userNotificationChannelPreference = (Database.UserNotificationChannelPreference)_context.Entry(existing).GetDatabaseValues().ToObject();
			userNotificationChannelPreference.ApplyDTO(userNotificationChannelPreferenceDTO);
			//
			// The tenant guid for any UserNotificationChannelPreference being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the UserNotificationChannelPreference because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				userNotificationChannelPreference.tenantGuid = existing.tenantGuid;
			}

			lock (userNotificationChannelPreferencePutSyncRoot)
			{
				//
				// Validate the version number for the userNotificationChannelPreference being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != userNotificationChannelPreference.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "UserNotificationChannelPreference save attempt was made but save request was with version " + userNotificationChannelPreference.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The UserNotificationChannelPreference you are trying to update has already changed.  Please try your save again after reloading the UserNotificationChannelPreference.");
				}
				else
				{
					// Same record.  Increase version.
					userNotificationChannelPreference.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (userNotificationChannelPreference.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Alerting.UserNotificationChannelPreference record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				try
				{
				    EntityEntry<Database.UserNotificationChannelPreference> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(userNotificationChannelPreference);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        UserNotificationChannelPreferenceChangeHistory userNotificationChannelPreferenceChangeHistory = new UserNotificationChannelPreferenceChangeHistory();
				        userNotificationChannelPreferenceChangeHistory.userNotificationChannelPreferenceId = userNotificationChannelPreference.id;
				        userNotificationChannelPreferenceChangeHistory.versionNumber = userNotificationChannelPreference.versionNumber;
				        userNotificationChannelPreferenceChangeHistory.timeStamp = DateTime.UtcNow;
				        userNotificationChannelPreferenceChangeHistory.userId = securityUser.id;
				        userNotificationChannelPreferenceChangeHistory.tenantGuid = userTenantGuid;
				        userNotificationChannelPreferenceChangeHistory.data = JsonSerializer.Serialize(Database.UserNotificationChannelPreference.CreateAnonymousWithFirstLevelSubObjects(userNotificationChannelPreference));
				        _context.UserNotificationChannelPreferenceChangeHistories.Add(userNotificationChannelPreferenceChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.UserNotificationChannelPreference entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserNotificationChannelPreference.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserNotificationChannelPreference.CreateAnonymousWithFirstLevelSubObjects(userNotificationChannelPreference)),
						null);

				return Ok(Database.UserNotificationChannelPreference.CreateAnonymous(userNotificationChannelPreference));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.UserNotificationChannelPreference entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserNotificationChannelPreference.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserNotificationChannelPreference.CreateAnonymousWithFirstLevelSubObjects(userNotificationChannelPreference)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new UserNotificationChannelPreference record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationChannelPreference", Name = "UserNotificationChannelPreference")]
		public async Task<IActionResult> PostUserNotificationChannelPreference([FromBody]Database.UserNotificationChannelPreference.UserNotificationChannelPreferenceDTO userNotificationChannelPreferenceDTO, CancellationToken cancellationToken = default)
		{
			if (userNotificationChannelPreferenceDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
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
			// Create a new UserNotificationChannelPreference object using the data from the DTO
			//
			Database.UserNotificationChannelPreference userNotificationChannelPreference = Database.UserNotificationChannelPreference.FromDTO(userNotificationChannelPreferenceDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				userNotificationChannelPreference.tenantGuid = userTenantGuid;

				userNotificationChannelPreference.objectGuid = Guid.NewGuid();
				userNotificationChannelPreference.versionNumber = 1;

				_context.UserNotificationChannelPreferences.Add(userNotificationChannelPreference);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the userNotificationChannelPreference object so that no further changes will be written to the database
				    //
				    _context.Entry(userNotificationChannelPreference).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					userNotificationChannelPreference.UserNotificationChannelPreferenceChangeHistories = null;
					userNotificationChannelPreference.notificationChannelType = null;
					userNotificationChannelPreference.userNotificationPreference = null;


				    UserNotificationChannelPreferenceChangeHistory userNotificationChannelPreferenceChangeHistory = new UserNotificationChannelPreferenceChangeHistory();
				    userNotificationChannelPreferenceChangeHistory.userNotificationChannelPreferenceId = userNotificationChannelPreference.id;
				    userNotificationChannelPreferenceChangeHistory.versionNumber = userNotificationChannelPreference.versionNumber;
				    userNotificationChannelPreferenceChangeHistory.timeStamp = DateTime.UtcNow;
				    userNotificationChannelPreferenceChangeHistory.userId = securityUser.id;
				    userNotificationChannelPreferenceChangeHistory.tenantGuid = userTenantGuid;
				    userNotificationChannelPreferenceChangeHistory.data = JsonSerializer.Serialize(Database.UserNotificationChannelPreference.CreateAnonymousWithFirstLevelSubObjects(userNotificationChannelPreference));
				    _context.UserNotificationChannelPreferenceChangeHistories.Add(userNotificationChannelPreferenceChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Alerting.UserNotificationChannelPreference entity successfully created.",
						true,
						userNotificationChannelPreference. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.UserNotificationChannelPreference.CreateAnonymousWithFirstLevelSubObjects(userNotificationChannelPreference)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Alerting.UserNotificationChannelPreference entity creation failed.", false, userNotificationChannelPreference.id.ToString(), "", JsonSerializer.Serialize(Database.UserNotificationChannelPreference.CreateAnonymousWithFirstLevelSubObjects(userNotificationChannelPreference)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserNotificationChannelPreference", userNotificationChannelPreference.id, userNotificationChannelPreference.id.ToString()));

			return CreatedAtRoute("UserNotificationChannelPreference", new { id = userNotificationChannelPreference.id }, Database.UserNotificationChannelPreference.CreateAnonymousWithFirstLevelSubObjects(userNotificationChannelPreference));
		}



        /// <summary>
        /// 
        /// This rolls a UserNotificationChannelPreference entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationChannelPreference/Rollback/{id}")]
		[Route("api/UserNotificationChannelPreference/Rollback")]
		public async Task<IActionResult> RollbackToUserNotificationChannelPreferenceVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.UserNotificationChannelPreference> query = (from x in _context.UserNotificationChannelPreferences
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this UserNotificationChannelPreference concurrently
			//
			lock (userNotificationChannelPreferencePutSyncRoot)
			{
				
				Database.UserNotificationChannelPreference userNotificationChannelPreference = query.FirstOrDefault();
				
				if (userNotificationChannelPreference == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.UserNotificationChannelPreference rollback", id.ToString(), new Exception("No Alerting.UserNotificationChannelPreference entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the UserNotificationChannelPreference current state so we can log it.
				//
				Database.UserNotificationChannelPreference cloneOfExisting = (Database.UserNotificationChannelPreference)_context.Entry(userNotificationChannelPreference).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.UserNotificationChannelPreferenceChangeHistories = null;
				cloneOfExisting.notificationChannelType = null;
				cloneOfExisting.userNotificationPreference = null;

				if (versionNumber >= userNotificationChannelPreference.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Alerting.UserNotificationChannelPreference rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Alerting.UserNotificationChannelPreference rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				UserNotificationChannelPreferenceChangeHistory userNotificationChannelPreferenceChangeHistory = (from x in _context.UserNotificationChannelPreferenceChangeHistories
				                                               where
				                                               x.userNotificationChannelPreferenceId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (userNotificationChannelPreferenceChangeHistory != null)
				{
				    Database.UserNotificationChannelPreference oldUserNotificationChannelPreference = JsonSerializer.Deserialize<Database.UserNotificationChannelPreference>(userNotificationChannelPreferenceChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    userNotificationChannelPreference.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    userNotificationChannelPreference.userNotificationPreferenceId = oldUserNotificationChannelPreference.userNotificationPreferenceId;
				    userNotificationChannelPreference.notificationChannelTypeId = oldUserNotificationChannelPreference.notificationChannelTypeId;
				    userNotificationChannelPreference.isEnabled = oldUserNotificationChannelPreference.isEnabled;
				    userNotificationChannelPreference.priorityOverride = oldUserNotificationChannelPreference.priorityOverride;
				    userNotificationChannelPreference.objectGuid = oldUserNotificationChannelPreference.objectGuid;
				    userNotificationChannelPreference.active = oldUserNotificationChannelPreference.active;
				    userNotificationChannelPreference.deleted = oldUserNotificationChannelPreference.deleted;

				    string serializedUserNotificationChannelPreference = JsonSerializer.Serialize(userNotificationChannelPreference);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        UserNotificationChannelPreferenceChangeHistory newUserNotificationChannelPreferenceChangeHistory = new UserNotificationChannelPreferenceChangeHistory();
				        newUserNotificationChannelPreferenceChangeHistory.userNotificationChannelPreferenceId = userNotificationChannelPreference.id;
				        newUserNotificationChannelPreferenceChangeHistory.versionNumber = userNotificationChannelPreference.versionNumber;
				        newUserNotificationChannelPreferenceChangeHistory.timeStamp = DateTime.UtcNow;
				        newUserNotificationChannelPreferenceChangeHistory.userId = securityUser.id;
				        newUserNotificationChannelPreferenceChangeHistory.tenantGuid = userTenantGuid;
				        newUserNotificationChannelPreferenceChangeHistory.data = JsonSerializer.Serialize(Database.UserNotificationChannelPreference.CreateAnonymousWithFirstLevelSubObjects(userNotificationChannelPreference));
				        _context.UserNotificationChannelPreferenceChangeHistories.Add(newUserNotificationChannelPreferenceChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.UserNotificationChannelPreference rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserNotificationChannelPreference.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserNotificationChannelPreference.CreateAnonymousWithFirstLevelSubObjects(userNotificationChannelPreference)),
						null);


				    return Ok(Database.UserNotificationChannelPreference.CreateAnonymous(userNotificationChannelPreference));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Alerting.UserNotificationChannelPreference rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Alerting.UserNotificationChannelPreference rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a UserNotificationChannelPreference.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the UserNotificationChannelPreference</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationChannelPreference/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetUserNotificationChannelPreferenceChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{
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


			Database.UserNotificationChannelPreference userNotificationChannelPreference = await _context.UserNotificationChannelPreferences.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (userNotificationChannelPreference == null)
			{
				return NotFound();
			}

			try
			{
				userNotificationChannelPreference.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.UserNotificationChannelPreference> versionInfo = await userNotificationChannelPreference.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a UserNotificationChannelPreference.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the UserNotificationChannelPreference</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationChannelPreference/{id}/AuditHistory")]
		public async Task<IActionResult> GetUserNotificationChannelPreferenceAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{
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


			Database.UserNotificationChannelPreference userNotificationChannelPreference = await _context.UserNotificationChannelPreferences.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (userNotificationChannelPreference == null)
			{
				return NotFound();
			}

			try
			{
				userNotificationChannelPreference.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.UserNotificationChannelPreference>> versions = await userNotificationChannelPreference.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a UserNotificationChannelPreference.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the UserNotificationChannelPreference</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The UserNotificationChannelPreference object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationChannelPreference/{id}/Version/{version}")]
		public async Task<IActionResult> GetUserNotificationChannelPreferenceVersion(int id, int version, CancellationToken cancellationToken = default)
		{
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


			Database.UserNotificationChannelPreference userNotificationChannelPreference = await _context.UserNotificationChannelPreferences.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (userNotificationChannelPreference == null)
			{
				return NotFound();
			}

			try
			{
				userNotificationChannelPreference.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.UserNotificationChannelPreference> versionInfo = await userNotificationChannelPreference.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a UserNotificationChannelPreference at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the UserNotificationChannelPreference</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The UserNotificationChannelPreference object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationChannelPreference/{id}/StateAtTime")]
		public async Task<IActionResult> GetUserNotificationChannelPreferenceStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{
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


			Database.UserNotificationChannelPreference userNotificationChannelPreference = await _context.UserNotificationChannelPreferences.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (userNotificationChannelPreference == null)
			{
				return NotFound();
			}

			try
			{
				userNotificationChannelPreference.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.UserNotificationChannelPreference> versionInfo = await userNotificationChannelPreference.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a UserNotificationChannelPreference record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserNotificationChannelPreference/{id}")]
		[Route("api/UserNotificationChannelPreference")]
		public async Task<IActionResult> DeleteUserNotificationChannelPreference(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
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

			IQueryable<Database.UserNotificationChannelPreference> query = (from x in _context.UserNotificationChannelPreferences
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserNotificationChannelPreference userNotificationChannelPreference = await query.FirstOrDefaultAsync(cancellationToken);

			if (userNotificationChannelPreference == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.UserNotificationChannelPreference DELETE", id.ToString(), new Exception("No Alerting.UserNotificationChannelPreference entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.UserNotificationChannelPreference cloneOfExisting = (Database.UserNotificationChannelPreference)_context.Entry(userNotificationChannelPreference).GetDatabaseValues().ToObject();


			lock (userNotificationChannelPreferenceDeleteSyncRoot)
			{
			    try
			    {
			        userNotificationChannelPreference.deleted = true;
			        userNotificationChannelPreference.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        UserNotificationChannelPreferenceChangeHistory userNotificationChannelPreferenceChangeHistory = new UserNotificationChannelPreferenceChangeHistory();
			        userNotificationChannelPreferenceChangeHistory.userNotificationChannelPreferenceId = userNotificationChannelPreference.id;
			        userNotificationChannelPreferenceChangeHistory.versionNumber = userNotificationChannelPreference.versionNumber;
			        userNotificationChannelPreferenceChangeHistory.timeStamp = DateTime.UtcNow;
			        userNotificationChannelPreferenceChangeHistory.userId = securityUser.id;
			        userNotificationChannelPreferenceChangeHistory.tenantGuid = userTenantGuid;
			        userNotificationChannelPreferenceChangeHistory.data = JsonSerializer.Serialize(Database.UserNotificationChannelPreference.CreateAnonymousWithFirstLevelSubObjects(userNotificationChannelPreference));
			        _context.UserNotificationChannelPreferenceChangeHistories.Add(userNotificationChannelPreferenceChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Alerting.UserNotificationChannelPreference entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserNotificationChannelPreference.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserNotificationChannelPreference.CreateAnonymousWithFirstLevelSubObjects(userNotificationChannelPreference)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Alerting.UserNotificationChannelPreference entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserNotificationChannelPreference.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserNotificationChannelPreference.CreateAnonymousWithFirstLevelSubObjects(userNotificationChannelPreference)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of UserNotificationChannelPreference records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/UserNotificationChannelPreferences/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? userNotificationPreferenceId = null,
			int? notificationChannelTypeId = null,
			bool? isEnabled = null,
			int? priorityOverride = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);

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

			IQueryable<Database.UserNotificationChannelPreference> query = (from uncp in _context.UserNotificationChannelPreferences select uncp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userNotificationPreferenceId.HasValue == true)
			{
				query = query.Where(uncp => uncp.userNotificationPreferenceId == userNotificationPreferenceId.Value);
			}
			if (notificationChannelTypeId.HasValue == true)
			{
				query = query.Where(uncp => uncp.notificationChannelTypeId == notificationChannelTypeId.Value);
			}
			if (isEnabled.HasValue == true)
			{
				query = query.Where(uncp => uncp.isEnabled == isEnabled.Value);
			}
			if (priorityOverride.HasValue == true)
			{
				query = query.Where(uncp => uncp.priorityOverride == priorityOverride.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(uncp => uncp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(uncp => uncp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(uncp => uncp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(uncp => uncp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(uncp => uncp.deleted == false);
				}
			}
			else
			{
				query = query.Where(uncp => uncp.active == true);
				query = query.Where(uncp => uncp.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Notification Channel Preference, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.notificationChannelType.name.Contains(anyStringContains)
			       || x.notificationChannelType.description.Contains(anyStringContains)
			       || x.userNotificationPreference.timeZoneId.Contains(anyStringContains)
			       || x.userNotificationPreference.quietHoursStart.Contains(anyStringContains)
			       || x.userNotificationPreference.quietHoursEnd.Contains(anyStringContains)
			       || x.userNotificationPreference.customSettingsJson.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.UserNotificationChannelPreference.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/UserNotificationChannelPreference/CreateAuditEvent")]
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
