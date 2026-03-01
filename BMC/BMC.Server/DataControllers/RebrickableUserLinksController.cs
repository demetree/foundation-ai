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
    /// This auto generated class provides the basic CRUD operations for the RebrickableUserLink entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the RebrickableUserLink entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class RebrickableUserLinksController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 10;

		private BMCContext _context;

		private ILogger<RebrickableUserLinksController> _logger;

		public RebrickableUserLinksController(BMCContext context, ILogger<RebrickableUserLinksController> logger) : base("BMC", "RebrickableUserLink")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of RebrickableUserLinks filtered by the parameters provided.
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
		[Route("api/RebrickableUserLinks")]
		public async Task<IActionResult> GetRebrickableUserLinks(
			string rebrickableUsername = null,
			string encryptedApiToken = null,
			string authMode = null,
			string encryptedPassword = null,
			bool? syncEnabled = null,
			string syncDirectionFlags = null,
			int? pullIntervalMinutes = null,
			DateTime? lastSyncDate = null,
			DateTime? lastPullDate = null,
			DateTime? lastPushDate = null,
			string lastSyncError = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
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
			if (lastSyncDate.HasValue == true && lastSyncDate.Value.Kind != DateTimeKind.Utc)
			{
				lastSyncDate = lastSyncDate.Value.ToUniversalTime();
			}

			if (lastPullDate.HasValue == true && lastPullDate.Value.Kind != DateTimeKind.Utc)
			{
				lastPullDate = lastPullDate.Value.ToUniversalTime();
			}

			if (lastPushDate.HasValue == true && lastPushDate.Value.Kind != DateTimeKind.Utc)
			{
				lastPushDate = lastPushDate.Value.ToUniversalTime();
			}

			IQueryable<Database.RebrickableUserLink> query = (from rul in _context.RebrickableUserLinks select rul);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(rebrickableUsername) == false)
			{
				query = query.Where(rul => rul.rebrickableUsername == rebrickableUsername);
			}
			if (string.IsNullOrEmpty(encryptedApiToken) == false)
			{
				query = query.Where(rul => rul.encryptedApiToken == encryptedApiToken);
			}
			if (string.IsNullOrEmpty(authMode) == false)
			{
				query = query.Where(rul => rul.authMode == authMode);
			}
			if (string.IsNullOrEmpty(encryptedPassword) == false)
			{
				query = query.Where(rul => rul.encryptedPassword == encryptedPassword);
			}
			if (syncEnabled.HasValue == true)
			{
				query = query.Where(rul => rul.syncEnabled == syncEnabled.Value);
			}
			if (string.IsNullOrEmpty(syncDirectionFlags) == false)
			{
				query = query.Where(rul => rul.syncDirectionFlags == syncDirectionFlags);
			}
			if (pullIntervalMinutes.HasValue == true)
			{
				query = query.Where(rul => rul.pullIntervalMinutes == pullIntervalMinutes.Value);
			}
			if (lastSyncDate.HasValue == true)
			{
				query = query.Where(rul => rul.lastSyncDate == lastSyncDate.Value);
			}
			if (lastPullDate.HasValue == true)
			{
				query = query.Where(rul => rul.lastPullDate == lastPullDate.Value);
			}
			if (lastPushDate.HasValue == true)
			{
				query = query.Where(rul => rul.lastPushDate == lastPushDate.Value);
			}
			if (string.IsNullOrEmpty(lastSyncError) == false)
			{
				query = query.Where(rul => rul.lastSyncError == lastSyncError);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rul => rul.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rul => rul.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rul => rul.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rul => rul.deleted == false);
				}
			}
			else
			{
				query = query.Where(rul => rul.active == true);
				query = query.Where(rul => rul.deleted == false);
			}

			query = query.OrderBy(rul => rul.rebrickableUsername).ThenBy(rul => rul.encryptedApiToken).ThenBy(rul => rul.authMode);


			//
			// Add the any string contains parameter to span all the string fields on the Rebrickable User Link, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.rebrickableUsername.Contains(anyStringContains)
			       || x.encryptedApiToken.Contains(anyStringContains)
			       || x.authMode.Contains(anyStringContains)
			       || x.encryptedPassword.Contains(anyStringContains)
			       || x.syncDirectionFlags.Contains(anyStringContains)
			       || x.lastSyncError.Contains(anyStringContains)
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
			
			List<Database.RebrickableUserLink> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.RebrickableUserLink rebrickableUserLink in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(rebrickableUserLink, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.RebrickableUserLink Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.RebrickableUserLink Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of RebrickableUserLinks filtered by the parameters provided.  Its query is similar to the GetRebrickableUserLinks method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RebrickableUserLinks/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string rebrickableUsername = null,
			string encryptedApiToken = null,
			string authMode = null,
			string encryptedPassword = null,
			bool? syncEnabled = null,
			string syncDirectionFlags = null,
			int? pullIntervalMinutes = null,
			DateTime? lastSyncDate = null,
			DateTime? lastPullDate = null,
			DateTime? lastPushDate = null,
			string lastSyncError = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
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
			if (lastSyncDate.HasValue == true && lastSyncDate.Value.Kind != DateTimeKind.Utc)
			{
				lastSyncDate = lastSyncDate.Value.ToUniversalTime();
			}

			if (lastPullDate.HasValue == true && lastPullDate.Value.Kind != DateTimeKind.Utc)
			{
				lastPullDate = lastPullDate.Value.ToUniversalTime();
			}

			if (lastPushDate.HasValue == true && lastPushDate.Value.Kind != DateTimeKind.Utc)
			{
				lastPushDate = lastPushDate.Value.ToUniversalTime();
			}

			IQueryable<Database.RebrickableUserLink> query = (from rul in _context.RebrickableUserLinks select rul);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (rebrickableUsername != null)
			{
				query = query.Where(rul => rul.rebrickableUsername == rebrickableUsername);
			}
			if (encryptedApiToken != null)
			{
				query = query.Where(rul => rul.encryptedApiToken == encryptedApiToken);
			}
			if (authMode != null)
			{
				query = query.Where(rul => rul.authMode == authMode);
			}
			if (encryptedPassword != null)
			{
				query = query.Where(rul => rul.encryptedPassword == encryptedPassword);
			}
			if (syncEnabled.HasValue == true)
			{
				query = query.Where(rul => rul.syncEnabled == syncEnabled.Value);
			}
			if (syncDirectionFlags != null)
			{
				query = query.Where(rul => rul.syncDirectionFlags == syncDirectionFlags);
			}
			if (pullIntervalMinutes.HasValue == true)
			{
				query = query.Where(rul => rul.pullIntervalMinutes == pullIntervalMinutes.Value);
			}
			if (lastSyncDate.HasValue == true)
			{
				query = query.Where(rul => rul.lastSyncDate == lastSyncDate.Value);
			}
			if (lastPullDate.HasValue == true)
			{
				query = query.Where(rul => rul.lastPullDate == lastPullDate.Value);
			}
			if (lastPushDate.HasValue == true)
			{
				query = query.Where(rul => rul.lastPushDate == lastPushDate.Value);
			}
			if (lastSyncError != null)
			{
				query = query.Where(rul => rul.lastSyncError == lastSyncError);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rul => rul.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rul => rul.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rul => rul.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rul => rul.deleted == false);
				}
			}
			else
			{
				query = query.Where(rul => rul.active == true);
				query = query.Where(rul => rul.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Rebrickable User Link, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.rebrickableUsername.Contains(anyStringContains)
			       || x.encryptedApiToken.Contains(anyStringContains)
			       || x.authMode.Contains(anyStringContains)
			       || x.encryptedPassword.Contains(anyStringContains)
			       || x.syncDirectionFlags.Contains(anyStringContains)
			       || x.lastSyncError.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single RebrickableUserLink by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RebrickableUserLink/{id}")]
		public async Task<IActionResult> GetRebrickableUserLink(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
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
				IQueryable<Database.RebrickableUserLink> query = (from rul in _context.RebrickableUserLinks where
							(rul.id == id) &&
							(userIsAdmin == true || rul.deleted == false) &&
							(userIsWriter == true || rul.active == true)
					select rul);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.RebrickableUserLink materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.RebrickableUserLink Entity was read with Admin privilege." : "BMC.RebrickableUserLink Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "RebrickableUserLink", materialized.id, materialized.rebrickableUsername));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.RebrickableUserLink entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.RebrickableUserLink.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.RebrickableUserLink.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing RebrickableUserLink record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/RebrickableUserLink/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutRebrickableUserLink(int id, [FromBody]Database.RebrickableUserLink.RebrickableUserLinkDTO rebrickableUserLinkDTO, CancellationToken cancellationToken = default)
		{
			if (rebrickableUserLinkDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Collection Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != rebrickableUserLinkDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
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


			IQueryable<Database.RebrickableUserLink> query = (from x in _context.RebrickableUserLinks
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.RebrickableUserLink existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.RebrickableUserLink PUT", id.ToString(), new Exception("No BMC.RebrickableUserLink entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (rebrickableUserLinkDTO.objectGuid == Guid.Empty)
            {
                rebrickableUserLinkDTO.objectGuid = existing.objectGuid;
            }
            else if (rebrickableUserLinkDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a RebrickableUserLink record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.RebrickableUserLink cloneOfExisting = (Database.RebrickableUserLink)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new RebrickableUserLink object using the data from the existing record, updated with what is in the DTO.
			//
			Database.RebrickableUserLink rebrickableUserLink = (Database.RebrickableUserLink)_context.Entry(existing).GetDatabaseValues().ToObject();
			rebrickableUserLink.ApplyDTO(rebrickableUserLinkDTO);
			//
			// The tenant guid for any RebrickableUserLink being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the RebrickableUserLink because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				rebrickableUserLink.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (rebrickableUserLink.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.RebrickableUserLink record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (rebrickableUserLink.rebrickableUsername != null && rebrickableUserLink.rebrickableUsername.Length > 100)
			{
				rebrickableUserLink.rebrickableUsername = rebrickableUserLink.rebrickableUsername.Substring(0, 100);
			}

			if (rebrickableUserLink.encryptedApiToken != null && rebrickableUserLink.encryptedApiToken.Length > 500)
			{
				rebrickableUserLink.encryptedApiToken = rebrickableUserLink.encryptedApiToken.Substring(0, 500);
			}

			if (rebrickableUserLink.authMode != null && rebrickableUserLink.authMode.Length > 50)
			{
				rebrickableUserLink.authMode = rebrickableUserLink.authMode.Substring(0, 50);
			}

			if (rebrickableUserLink.encryptedPassword != null && rebrickableUserLink.encryptedPassword.Length > 500)
			{
				rebrickableUserLink.encryptedPassword = rebrickableUserLink.encryptedPassword.Substring(0, 500);
			}

			if (rebrickableUserLink.syncDirectionFlags != null && rebrickableUserLink.syncDirectionFlags.Length > 50)
			{
				rebrickableUserLink.syncDirectionFlags = rebrickableUserLink.syncDirectionFlags.Substring(0, 50);
			}

			if (rebrickableUserLink.lastSyncDate.HasValue == true && rebrickableUserLink.lastSyncDate.Value.Kind != DateTimeKind.Utc)
			{
				rebrickableUserLink.lastSyncDate = rebrickableUserLink.lastSyncDate.Value.ToUniversalTime();
			}

			if (rebrickableUserLink.lastPullDate.HasValue == true && rebrickableUserLink.lastPullDate.Value.Kind != DateTimeKind.Utc)
			{
				rebrickableUserLink.lastPullDate = rebrickableUserLink.lastPullDate.Value.ToUniversalTime();
			}

			if (rebrickableUserLink.lastPushDate.HasValue == true && rebrickableUserLink.lastPushDate.Value.Kind != DateTimeKind.Utc)
			{
				rebrickableUserLink.lastPushDate = rebrickableUserLink.lastPushDate.Value.ToUniversalTime();
			}

			EntityEntry<Database.RebrickableUserLink> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(rebrickableUserLink);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.RebrickableUserLink entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.RebrickableUserLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.RebrickableUserLink.CreateAnonymousWithFirstLevelSubObjects(rebrickableUserLink)),
					null);


				return Ok(Database.RebrickableUserLink.CreateAnonymous(rebrickableUserLink));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.RebrickableUserLink entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.RebrickableUserLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.RebrickableUserLink.CreateAnonymousWithFirstLevelSubObjects(rebrickableUserLink)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new RebrickableUserLink record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RebrickableUserLink", Name = "RebrickableUserLink")]
		public async Task<IActionResult> PostRebrickableUserLink([FromBody]Database.RebrickableUserLink.RebrickableUserLinkDTO rebrickableUserLinkDTO, CancellationToken cancellationToken = default)
		{
			if (rebrickableUserLinkDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Collection Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new RebrickableUserLink object using the data from the DTO
			//
			Database.RebrickableUserLink rebrickableUserLink = Database.RebrickableUserLink.FromDTO(rebrickableUserLinkDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				rebrickableUserLink.tenantGuid = userTenantGuid;

				if (rebrickableUserLink.rebrickableUsername != null && rebrickableUserLink.rebrickableUsername.Length > 100)
				{
					rebrickableUserLink.rebrickableUsername = rebrickableUserLink.rebrickableUsername.Substring(0, 100);
				}

				if (rebrickableUserLink.encryptedApiToken != null && rebrickableUserLink.encryptedApiToken.Length > 500)
				{
					rebrickableUserLink.encryptedApiToken = rebrickableUserLink.encryptedApiToken.Substring(0, 500);
				}

				if (rebrickableUserLink.authMode != null && rebrickableUserLink.authMode.Length > 50)
				{
					rebrickableUserLink.authMode = rebrickableUserLink.authMode.Substring(0, 50);
				}

				if (rebrickableUserLink.encryptedPassword != null && rebrickableUserLink.encryptedPassword.Length > 500)
				{
					rebrickableUserLink.encryptedPassword = rebrickableUserLink.encryptedPassword.Substring(0, 500);
				}

				if (rebrickableUserLink.syncDirectionFlags != null && rebrickableUserLink.syncDirectionFlags.Length > 50)
				{
					rebrickableUserLink.syncDirectionFlags = rebrickableUserLink.syncDirectionFlags.Substring(0, 50);
				}

				if (rebrickableUserLink.lastSyncDate.HasValue == true && rebrickableUserLink.lastSyncDate.Value.Kind != DateTimeKind.Utc)
				{
					rebrickableUserLink.lastSyncDate = rebrickableUserLink.lastSyncDate.Value.ToUniversalTime();
				}

				if (rebrickableUserLink.lastPullDate.HasValue == true && rebrickableUserLink.lastPullDate.Value.Kind != DateTimeKind.Utc)
				{
					rebrickableUserLink.lastPullDate = rebrickableUserLink.lastPullDate.Value.ToUniversalTime();
				}

				if (rebrickableUserLink.lastPushDate.HasValue == true && rebrickableUserLink.lastPushDate.Value.Kind != DateTimeKind.Utc)
				{
					rebrickableUserLink.lastPushDate = rebrickableUserLink.lastPushDate.Value.ToUniversalTime();
				}

				rebrickableUserLink.objectGuid = Guid.NewGuid();
				_context.RebrickableUserLinks.Add(rebrickableUserLink);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.RebrickableUserLink entity successfully created.",
					true,
					rebrickableUserLink.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.RebrickableUserLink.CreateAnonymousWithFirstLevelSubObjects(rebrickableUserLink)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.RebrickableUserLink entity creation failed.", false, rebrickableUserLink.id.ToString(), "", JsonSerializer.Serialize(rebrickableUserLink), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "RebrickableUserLink", rebrickableUserLink.id, rebrickableUserLink.rebrickableUsername));

			return CreatedAtRoute("RebrickableUserLink", new { id = rebrickableUserLink.id }, Database.RebrickableUserLink.CreateAnonymousWithFirstLevelSubObjects(rebrickableUserLink));
		}



        /// <summary>
        /// 
        /// This deletes a RebrickableUserLink record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RebrickableUserLink/{id}")]
		[Route("api/RebrickableUserLink")]
		public async Task<IActionResult> DeleteRebrickableUserLink(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Collection Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.RebrickableUserLink> query = (from x in _context.RebrickableUserLinks
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.RebrickableUserLink rebrickableUserLink = await query.FirstOrDefaultAsync(cancellationToken);

			if (rebrickableUserLink == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.RebrickableUserLink DELETE", id.ToString(), new Exception("No BMC.RebrickableUserLink entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.RebrickableUserLink cloneOfExisting = (Database.RebrickableUserLink)_context.Entry(rebrickableUserLink).GetDatabaseValues().ToObject();


			try
			{
				rebrickableUserLink.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.RebrickableUserLink entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.RebrickableUserLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.RebrickableUserLink.CreateAnonymousWithFirstLevelSubObjects(rebrickableUserLink)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.RebrickableUserLink entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.RebrickableUserLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.RebrickableUserLink.CreateAnonymousWithFirstLevelSubObjects(rebrickableUserLink)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of RebrickableUserLink records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/RebrickableUserLinks/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string rebrickableUsername = null,
			string encryptedApiToken = null,
			string authMode = null,
			string encryptedPassword = null,
			bool? syncEnabled = null,
			string syncDirectionFlags = null,
			int? pullIntervalMinutes = null,
			DateTime? lastSyncDate = null,
			DateTime? lastPullDate = null,
			DateTime? lastPushDate = null,
			string lastSyncError = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);


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
			if (lastSyncDate.HasValue == true && lastSyncDate.Value.Kind != DateTimeKind.Utc)
			{
				lastSyncDate = lastSyncDate.Value.ToUniversalTime();
			}

			if (lastPullDate.HasValue == true && lastPullDate.Value.Kind != DateTimeKind.Utc)
			{
				lastPullDate = lastPullDate.Value.ToUniversalTime();
			}

			if (lastPushDate.HasValue == true && lastPushDate.Value.Kind != DateTimeKind.Utc)
			{
				lastPushDate = lastPushDate.Value.ToUniversalTime();
			}

			IQueryable<Database.RebrickableUserLink> query = (from rul in _context.RebrickableUserLinks select rul);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(rebrickableUsername) == false)
			{
				query = query.Where(rul => rul.rebrickableUsername == rebrickableUsername);
			}
			if (string.IsNullOrEmpty(encryptedApiToken) == false)
			{
				query = query.Where(rul => rul.encryptedApiToken == encryptedApiToken);
			}
			if (string.IsNullOrEmpty(authMode) == false)
			{
				query = query.Where(rul => rul.authMode == authMode);
			}
			if (string.IsNullOrEmpty(encryptedPassword) == false)
			{
				query = query.Where(rul => rul.encryptedPassword == encryptedPassword);
			}
			if (syncEnabled.HasValue == true)
			{
				query = query.Where(rul => rul.syncEnabled == syncEnabled.Value);
			}
			if (string.IsNullOrEmpty(syncDirectionFlags) == false)
			{
				query = query.Where(rul => rul.syncDirectionFlags == syncDirectionFlags);
			}
			if (pullIntervalMinutes.HasValue == true)
			{
				query = query.Where(rul => rul.pullIntervalMinutes == pullIntervalMinutes.Value);
			}
			if (lastSyncDate.HasValue == true)
			{
				query = query.Where(rul => rul.lastSyncDate == lastSyncDate.Value);
			}
			if (lastPullDate.HasValue == true)
			{
				query = query.Where(rul => rul.lastPullDate == lastPullDate.Value);
			}
			if (lastPushDate.HasValue == true)
			{
				query = query.Where(rul => rul.lastPushDate == lastPushDate.Value);
			}
			if (string.IsNullOrEmpty(lastSyncError) == false)
			{
				query = query.Where(rul => rul.lastSyncError == lastSyncError);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rul => rul.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rul => rul.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rul => rul.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rul => rul.deleted == false);
				}
			}
			else
			{
				query = query.Where(rul => rul.active == true);
				query = query.Where(rul => rul.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Rebrickable User Link, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.rebrickableUsername.Contains(anyStringContains)
			       || x.encryptedApiToken.Contains(anyStringContains)
			       || x.authMode.Contains(anyStringContains)
			       || x.encryptedPassword.Contains(anyStringContains)
			       || x.syncDirectionFlags.Contains(anyStringContains)
			       || x.lastSyncError.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.rebrickableUsername).ThenBy(x => x.encryptedApiToken).ThenBy(x => x.authMode);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.RebrickableUserLink.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/RebrickableUserLink/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Collection Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
