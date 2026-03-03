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
    /// This auto generated class provides the basic CRUD operations for the BrickSetUserLink entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the BrickSetUserLink entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class BrickSetUserLinksController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 10;

		private BMCContext _context;

		private ILogger<BrickSetUserLinksController> _logger;

		public BrickSetUserLinksController(BMCContext context, ILogger<BrickSetUserLinksController> logger) : base("BMC", "BrickSetUserLink")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of BrickSetUserLinks filtered by the parameters provided.
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
		[Route("api/BrickSetUserLinks")]
		public async Task<IActionResult> GetBrickSetUserLinks(
			string brickSetUsername = null,
			string encryptedUserHash = null,
			string encryptedPassword = null,
			bool? syncEnabled = null,
			string syncDirection = null,
			DateTime? lastSyncDate = null,
			DateTime? lastPullDate = null,
			DateTime? lastPushDate = null,
			string lastSyncError = null,
			DateTime? userHashStoredDate = null,
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

			if (userHashStoredDate.HasValue == true && userHashStoredDate.Value.Kind != DateTimeKind.Utc)
			{
				userHashStoredDate = userHashStoredDate.Value.ToUniversalTime();
			}

			IQueryable<Database.BrickSetUserLink> query = (from bsul in _context.BrickSetUserLinks select bsul);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(brickSetUsername) == false)
			{
				query = query.Where(bsul => bsul.brickSetUsername == brickSetUsername);
			}
			if (string.IsNullOrEmpty(encryptedUserHash) == false)
			{
				query = query.Where(bsul => bsul.encryptedUserHash == encryptedUserHash);
			}
			if (string.IsNullOrEmpty(encryptedPassword) == false)
			{
				query = query.Where(bsul => bsul.encryptedPassword == encryptedPassword);
			}
			if (syncEnabled.HasValue == true)
			{
				query = query.Where(bsul => bsul.syncEnabled == syncEnabled.Value);
			}
			if (string.IsNullOrEmpty(syncDirection) == false)
			{
				query = query.Where(bsul => bsul.syncDirection == syncDirection);
			}
			if (lastSyncDate.HasValue == true)
			{
				query = query.Where(bsul => bsul.lastSyncDate == lastSyncDate.Value);
			}
			if (lastPullDate.HasValue == true)
			{
				query = query.Where(bsul => bsul.lastPullDate == lastPullDate.Value);
			}
			if (lastPushDate.HasValue == true)
			{
				query = query.Where(bsul => bsul.lastPushDate == lastPushDate.Value);
			}
			if (string.IsNullOrEmpty(lastSyncError) == false)
			{
				query = query.Where(bsul => bsul.lastSyncError == lastSyncError);
			}
			if (userHashStoredDate.HasValue == true)
			{
				query = query.Where(bsul => bsul.userHashStoredDate == userHashStoredDate.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bsul => bsul.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bsul => bsul.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bsul => bsul.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bsul => bsul.deleted == false);
				}
			}
			else
			{
				query = query.Where(bsul => bsul.active == true);
				query = query.Where(bsul => bsul.deleted == false);
			}

			query = query.OrderBy(bsul => bsul.brickSetUsername).ThenBy(bsul => bsul.encryptedUserHash).ThenBy(bsul => bsul.encryptedPassword);


			//
			// Add the any string contains parameter to span all the string fields on the Brick Set User Link, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.brickSetUsername.Contains(anyStringContains)
			       || x.encryptedUserHash.Contains(anyStringContains)
			       || x.encryptedPassword.Contains(anyStringContains)
			       || x.syncDirection.Contains(anyStringContains)
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
			
			List<Database.BrickSetUserLink> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.BrickSetUserLink brickSetUserLink in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(brickSetUserLink, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.BrickSetUserLink Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.BrickSetUserLink Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of BrickSetUserLinks filtered by the parameters provided.  Its query is similar to the GetBrickSetUserLinks method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickSetUserLinks/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string brickSetUsername = null,
			string encryptedUserHash = null,
			string encryptedPassword = null,
			bool? syncEnabled = null,
			string syncDirection = null,
			DateTime? lastSyncDate = null,
			DateTime? lastPullDate = null,
			DateTime? lastPushDate = null,
			string lastSyncError = null,
			DateTime? userHashStoredDate = null,
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

			if (userHashStoredDate.HasValue == true && userHashStoredDate.Value.Kind != DateTimeKind.Utc)
			{
				userHashStoredDate = userHashStoredDate.Value.ToUniversalTime();
			}

			IQueryable<Database.BrickSetUserLink> query = (from bsul in _context.BrickSetUserLinks select bsul);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (brickSetUsername != null)
			{
				query = query.Where(bsul => bsul.brickSetUsername == brickSetUsername);
			}
			if (encryptedUserHash != null)
			{
				query = query.Where(bsul => bsul.encryptedUserHash == encryptedUserHash);
			}
			if (encryptedPassword != null)
			{
				query = query.Where(bsul => bsul.encryptedPassword == encryptedPassword);
			}
			if (syncEnabled.HasValue == true)
			{
				query = query.Where(bsul => bsul.syncEnabled == syncEnabled.Value);
			}
			if (syncDirection != null)
			{
				query = query.Where(bsul => bsul.syncDirection == syncDirection);
			}
			if (lastSyncDate.HasValue == true)
			{
				query = query.Where(bsul => bsul.lastSyncDate == lastSyncDate.Value);
			}
			if (lastPullDate.HasValue == true)
			{
				query = query.Where(bsul => bsul.lastPullDate == lastPullDate.Value);
			}
			if (lastPushDate.HasValue == true)
			{
				query = query.Where(bsul => bsul.lastPushDate == lastPushDate.Value);
			}
			if (lastSyncError != null)
			{
				query = query.Where(bsul => bsul.lastSyncError == lastSyncError);
			}
			if (userHashStoredDate.HasValue == true)
			{
				query = query.Where(bsul => bsul.userHashStoredDate == userHashStoredDate.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bsul => bsul.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bsul => bsul.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bsul => bsul.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bsul => bsul.deleted == false);
				}
			}
			else
			{
				query = query.Where(bsul => bsul.active == true);
				query = query.Where(bsul => bsul.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Brick Set User Link, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.brickSetUsername.Contains(anyStringContains)
			       || x.encryptedUserHash.Contains(anyStringContains)
			       || x.encryptedPassword.Contains(anyStringContains)
			       || x.syncDirection.Contains(anyStringContains)
			       || x.lastSyncError.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single BrickSetUserLink by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickSetUserLink/{id}")]
		public async Task<IActionResult> GetBrickSetUserLink(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.BrickSetUserLink> query = (from bsul in _context.BrickSetUserLinks where
							(bsul.id == id) &&
							(userIsAdmin == true || bsul.deleted == false) &&
							(userIsWriter == true || bsul.active == true)
					select bsul);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.BrickSetUserLink materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.BrickSetUserLink Entity was read with Admin privilege." : "BMC.BrickSetUserLink Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickSetUserLink", materialized.id, materialized.brickSetUsername));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.BrickSetUserLink entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.BrickSetUserLink.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.BrickSetUserLink.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing BrickSetUserLink record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/BrickSetUserLink/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutBrickSetUserLink(int id, [FromBody]Database.BrickSetUserLink.BrickSetUserLinkDTO brickSetUserLinkDTO, CancellationToken cancellationToken = default)
		{
			if (brickSetUserLinkDTO == null)
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



			if (id != brickSetUserLinkDTO.id)
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


			IQueryable<Database.BrickSetUserLink> query = (from x in _context.BrickSetUserLinks
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.BrickSetUserLink existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickSetUserLink PUT", id.ToString(), new Exception("No BMC.BrickSetUserLink entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (brickSetUserLinkDTO.objectGuid == Guid.Empty)
            {
                brickSetUserLinkDTO.objectGuid = existing.objectGuid;
            }
            else if (brickSetUserLinkDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a BrickSetUserLink record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.BrickSetUserLink cloneOfExisting = (Database.BrickSetUserLink)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new BrickSetUserLink object using the data from the existing record, updated with what is in the DTO.
			//
			Database.BrickSetUserLink brickSetUserLink = (Database.BrickSetUserLink)_context.Entry(existing).GetDatabaseValues().ToObject();
			brickSetUserLink.ApplyDTO(brickSetUserLinkDTO);
			//
			// The tenant guid for any BrickSetUserLink being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the BrickSetUserLink because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				brickSetUserLink.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (brickSetUserLink.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.BrickSetUserLink record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (brickSetUserLink.brickSetUsername != null && brickSetUserLink.brickSetUsername.Length > 100)
			{
				brickSetUserLink.brickSetUsername = brickSetUserLink.brickSetUsername.Substring(0, 100);
			}

			if (brickSetUserLink.encryptedUserHash != null && brickSetUserLink.encryptedUserHash.Length > 500)
			{
				brickSetUserLink.encryptedUserHash = brickSetUserLink.encryptedUserHash.Substring(0, 500);
			}

			if (brickSetUserLink.encryptedPassword != null && brickSetUserLink.encryptedPassword.Length > 500)
			{
				brickSetUserLink.encryptedPassword = brickSetUserLink.encryptedPassword.Substring(0, 500);
			}

			if (brickSetUserLink.syncDirection != null && brickSetUserLink.syncDirection.Length > 50)
			{
				brickSetUserLink.syncDirection = brickSetUserLink.syncDirection.Substring(0, 50);
			}

			if (brickSetUserLink.lastSyncDate.HasValue == true && brickSetUserLink.lastSyncDate.Value.Kind != DateTimeKind.Utc)
			{
				brickSetUserLink.lastSyncDate = brickSetUserLink.lastSyncDate.Value.ToUniversalTime();
			}

			if (brickSetUserLink.lastPullDate.HasValue == true && brickSetUserLink.lastPullDate.Value.Kind != DateTimeKind.Utc)
			{
				brickSetUserLink.lastPullDate = brickSetUserLink.lastPullDate.Value.ToUniversalTime();
			}

			if (brickSetUserLink.lastPushDate.HasValue == true && brickSetUserLink.lastPushDate.Value.Kind != DateTimeKind.Utc)
			{
				brickSetUserLink.lastPushDate = brickSetUserLink.lastPushDate.Value.ToUniversalTime();
			}

			if (brickSetUserLink.userHashStoredDate.HasValue == true && brickSetUserLink.userHashStoredDate.Value.Kind != DateTimeKind.Utc)
			{
				brickSetUserLink.userHashStoredDate = brickSetUserLink.userHashStoredDate.Value.ToUniversalTime();
			}

			EntityEntry<Database.BrickSetUserLink> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(brickSetUserLink);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.BrickSetUserLink entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickSetUserLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickSetUserLink.CreateAnonymousWithFirstLevelSubObjects(brickSetUserLink)),
					null);


				return Ok(Database.BrickSetUserLink.CreateAnonymous(brickSetUserLink));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.BrickSetUserLink entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickSetUserLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickSetUserLink.CreateAnonymousWithFirstLevelSubObjects(brickSetUserLink)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new BrickSetUserLink record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickSetUserLink", Name = "BrickSetUserLink")]
		public async Task<IActionResult> PostBrickSetUserLink([FromBody]Database.BrickSetUserLink.BrickSetUserLinkDTO brickSetUserLinkDTO, CancellationToken cancellationToken = default)
		{
			if (brickSetUserLinkDTO == null)
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
			// Create a new BrickSetUserLink object using the data from the DTO
			//
			Database.BrickSetUserLink brickSetUserLink = Database.BrickSetUserLink.FromDTO(brickSetUserLinkDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				brickSetUserLink.tenantGuid = userTenantGuid;

				if (brickSetUserLink.brickSetUsername != null && brickSetUserLink.brickSetUsername.Length > 100)
				{
					brickSetUserLink.brickSetUsername = brickSetUserLink.brickSetUsername.Substring(0, 100);
				}

				if (brickSetUserLink.encryptedUserHash != null && brickSetUserLink.encryptedUserHash.Length > 500)
				{
					brickSetUserLink.encryptedUserHash = brickSetUserLink.encryptedUserHash.Substring(0, 500);
				}

				if (brickSetUserLink.encryptedPassword != null && brickSetUserLink.encryptedPassword.Length > 500)
				{
					brickSetUserLink.encryptedPassword = brickSetUserLink.encryptedPassword.Substring(0, 500);
				}

				if (brickSetUserLink.syncDirection != null && brickSetUserLink.syncDirection.Length > 50)
				{
					brickSetUserLink.syncDirection = brickSetUserLink.syncDirection.Substring(0, 50);
				}

				if (brickSetUserLink.lastSyncDate.HasValue == true && brickSetUserLink.lastSyncDate.Value.Kind != DateTimeKind.Utc)
				{
					brickSetUserLink.lastSyncDate = brickSetUserLink.lastSyncDate.Value.ToUniversalTime();
				}

				if (brickSetUserLink.lastPullDate.HasValue == true && brickSetUserLink.lastPullDate.Value.Kind != DateTimeKind.Utc)
				{
					brickSetUserLink.lastPullDate = brickSetUserLink.lastPullDate.Value.ToUniversalTime();
				}

				if (brickSetUserLink.lastPushDate.HasValue == true && brickSetUserLink.lastPushDate.Value.Kind != DateTimeKind.Utc)
				{
					brickSetUserLink.lastPushDate = brickSetUserLink.lastPushDate.Value.ToUniversalTime();
				}

				if (brickSetUserLink.userHashStoredDate.HasValue == true && brickSetUserLink.userHashStoredDate.Value.Kind != DateTimeKind.Utc)
				{
					brickSetUserLink.userHashStoredDate = brickSetUserLink.userHashStoredDate.Value.ToUniversalTime();
				}

				brickSetUserLink.objectGuid = Guid.NewGuid();
				_context.BrickSetUserLinks.Add(brickSetUserLink);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.BrickSetUserLink entity successfully created.",
					true,
					brickSetUserLink.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.BrickSetUserLink.CreateAnonymousWithFirstLevelSubObjects(brickSetUserLink)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.BrickSetUserLink entity creation failed.", false, brickSetUserLink.id.ToString(), "", JsonSerializer.Serialize(brickSetUserLink), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickSetUserLink", brickSetUserLink.id, brickSetUserLink.brickSetUsername));

			return CreatedAtRoute("BrickSetUserLink", new { id = brickSetUserLink.id }, Database.BrickSetUserLink.CreateAnonymousWithFirstLevelSubObjects(brickSetUserLink));
		}



        /// <summary>
        /// 
        /// This deletes a BrickSetUserLink record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickSetUserLink/{id}")]
		[Route("api/BrickSetUserLink")]
		public async Task<IActionResult> DeleteBrickSetUserLink(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.BrickSetUserLink> query = (from x in _context.BrickSetUserLinks
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.BrickSetUserLink brickSetUserLink = await query.FirstOrDefaultAsync(cancellationToken);

			if (brickSetUserLink == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickSetUserLink DELETE", id.ToString(), new Exception("No BMC.BrickSetUserLink entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.BrickSetUserLink cloneOfExisting = (Database.BrickSetUserLink)_context.Entry(brickSetUserLink).GetDatabaseValues().ToObject();


			try
			{
				brickSetUserLink.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BrickSetUserLink entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickSetUserLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickSetUserLink.CreateAnonymousWithFirstLevelSubObjects(brickSetUserLink)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BrickSetUserLink entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickSetUserLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickSetUserLink.CreateAnonymousWithFirstLevelSubObjects(brickSetUserLink)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of BrickSetUserLink records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/BrickSetUserLinks/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string brickSetUsername = null,
			string encryptedUserHash = null,
			string encryptedPassword = null,
			bool? syncEnabled = null,
			string syncDirection = null,
			DateTime? lastSyncDate = null,
			DateTime? lastPullDate = null,
			DateTime? lastPushDate = null,
			string lastSyncError = null,
			DateTime? userHashStoredDate = null,
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

			if (userHashStoredDate.HasValue == true && userHashStoredDate.Value.Kind != DateTimeKind.Utc)
			{
				userHashStoredDate = userHashStoredDate.Value.ToUniversalTime();
			}

			IQueryable<Database.BrickSetUserLink> query = (from bsul in _context.BrickSetUserLinks select bsul);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(brickSetUsername) == false)
			{
				query = query.Where(bsul => bsul.brickSetUsername == brickSetUsername);
			}
			if (string.IsNullOrEmpty(encryptedUserHash) == false)
			{
				query = query.Where(bsul => bsul.encryptedUserHash == encryptedUserHash);
			}
			if (string.IsNullOrEmpty(encryptedPassword) == false)
			{
				query = query.Where(bsul => bsul.encryptedPassword == encryptedPassword);
			}
			if (syncEnabled.HasValue == true)
			{
				query = query.Where(bsul => bsul.syncEnabled == syncEnabled.Value);
			}
			if (string.IsNullOrEmpty(syncDirection) == false)
			{
				query = query.Where(bsul => bsul.syncDirection == syncDirection);
			}
			if (lastSyncDate.HasValue == true)
			{
				query = query.Where(bsul => bsul.lastSyncDate == lastSyncDate.Value);
			}
			if (lastPullDate.HasValue == true)
			{
				query = query.Where(bsul => bsul.lastPullDate == lastPullDate.Value);
			}
			if (lastPushDate.HasValue == true)
			{
				query = query.Where(bsul => bsul.lastPushDate == lastPushDate.Value);
			}
			if (string.IsNullOrEmpty(lastSyncError) == false)
			{
				query = query.Where(bsul => bsul.lastSyncError == lastSyncError);
			}
			if (userHashStoredDate.HasValue == true)
			{
				query = query.Where(bsul => bsul.userHashStoredDate == userHashStoredDate.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bsul => bsul.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bsul => bsul.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bsul => bsul.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bsul => bsul.deleted == false);
				}
			}
			else
			{
				query = query.Where(bsul => bsul.active == true);
				query = query.Where(bsul => bsul.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Brick Set User Link, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.brickSetUsername.Contains(anyStringContains)
			       || x.encryptedUserHash.Contains(anyStringContains)
			       || x.encryptedPassword.Contains(anyStringContains)
			       || x.syncDirection.Contains(anyStringContains)
			       || x.lastSyncError.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.brickSetUsername).ThenBy(x => x.encryptedUserHash).ThenBy(x => x.encryptedPassword);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.BrickSetUserLink.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/BrickSetUserLink/CreateAuditEvent")]
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
