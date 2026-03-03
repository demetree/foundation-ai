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
    /// This auto generated class provides the basic CRUD operations for the BrickLinkUserLink entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the BrickLinkUserLink entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class BrickLinkUserLinksController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 10;

		private BMCContext _context;

		private ILogger<BrickLinkUserLinksController> _logger;

		public BrickLinkUserLinksController(BMCContext context, ILogger<BrickLinkUserLinksController> logger) : base("BMC", "BrickLinkUserLink")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of BrickLinkUserLinks filtered by the parameters provided.
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
		[Route("api/BrickLinkUserLinks")]
		public async Task<IActionResult> GetBrickLinkUserLinks(
			string encryptedTokenValue = null,
			string encryptedTokenSecret = null,
			bool? syncEnabled = null,
			string syncDirection = null,
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

			IQueryable<Database.BrickLinkUserLink> query = (from blul in _context.BrickLinkUserLinks select blul);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(encryptedTokenValue) == false)
			{
				query = query.Where(blul => blul.encryptedTokenValue == encryptedTokenValue);
			}
			if (string.IsNullOrEmpty(encryptedTokenSecret) == false)
			{
				query = query.Where(blul => blul.encryptedTokenSecret == encryptedTokenSecret);
			}
			if (syncEnabled.HasValue == true)
			{
				query = query.Where(blul => blul.syncEnabled == syncEnabled.Value);
			}
			if (string.IsNullOrEmpty(syncDirection) == false)
			{
				query = query.Where(blul => blul.syncDirection == syncDirection);
			}
			if (lastSyncDate.HasValue == true)
			{
				query = query.Where(blul => blul.lastSyncDate == lastSyncDate.Value);
			}
			if (lastPullDate.HasValue == true)
			{
				query = query.Where(blul => blul.lastPullDate == lastPullDate.Value);
			}
			if (lastPushDate.HasValue == true)
			{
				query = query.Where(blul => blul.lastPushDate == lastPushDate.Value);
			}
			if (string.IsNullOrEmpty(lastSyncError) == false)
			{
				query = query.Where(blul => blul.lastSyncError == lastSyncError);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(blul => blul.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(blul => blul.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(blul => blul.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(blul => blul.deleted == false);
				}
			}
			else
			{
				query = query.Where(blul => blul.active == true);
				query = query.Where(blul => blul.deleted == false);
			}

			query = query.OrderBy(blul => blul.encryptedTokenValue).ThenBy(blul => blul.encryptedTokenSecret).ThenBy(blul => blul.syncDirection);


			//
			// Add the any string contains parameter to span all the string fields on the Brick Link User Link, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.encryptedTokenValue.Contains(anyStringContains)
			       || x.encryptedTokenSecret.Contains(anyStringContains)
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
			
			List<Database.BrickLinkUserLink> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.BrickLinkUserLink brickLinkUserLink in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(brickLinkUserLink, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.BrickLinkUserLink Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.BrickLinkUserLink Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of BrickLinkUserLinks filtered by the parameters provided.  Its query is similar to the GetBrickLinkUserLinks method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickLinkUserLinks/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string encryptedTokenValue = null,
			string encryptedTokenSecret = null,
			bool? syncEnabled = null,
			string syncDirection = null,
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

			IQueryable<Database.BrickLinkUserLink> query = (from blul in _context.BrickLinkUserLinks select blul);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (encryptedTokenValue != null)
			{
				query = query.Where(blul => blul.encryptedTokenValue == encryptedTokenValue);
			}
			if (encryptedTokenSecret != null)
			{
				query = query.Where(blul => blul.encryptedTokenSecret == encryptedTokenSecret);
			}
			if (syncEnabled.HasValue == true)
			{
				query = query.Where(blul => blul.syncEnabled == syncEnabled.Value);
			}
			if (syncDirection != null)
			{
				query = query.Where(blul => blul.syncDirection == syncDirection);
			}
			if (lastSyncDate.HasValue == true)
			{
				query = query.Where(blul => blul.lastSyncDate == lastSyncDate.Value);
			}
			if (lastPullDate.HasValue == true)
			{
				query = query.Where(blul => blul.lastPullDate == lastPullDate.Value);
			}
			if (lastPushDate.HasValue == true)
			{
				query = query.Where(blul => blul.lastPushDate == lastPushDate.Value);
			}
			if (lastSyncError != null)
			{
				query = query.Where(blul => blul.lastSyncError == lastSyncError);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(blul => blul.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(blul => blul.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(blul => blul.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(blul => blul.deleted == false);
				}
			}
			else
			{
				query = query.Where(blul => blul.active == true);
				query = query.Where(blul => blul.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Brick Link User Link, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.encryptedTokenValue.Contains(anyStringContains)
			       || x.encryptedTokenSecret.Contains(anyStringContains)
			       || x.syncDirection.Contains(anyStringContains)
			       || x.lastSyncError.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single BrickLinkUserLink by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickLinkUserLink/{id}")]
		public async Task<IActionResult> GetBrickLinkUserLink(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.BrickLinkUserLink> query = (from blul in _context.BrickLinkUserLinks where
							(blul.id == id) &&
							(userIsAdmin == true || blul.deleted == false) &&
							(userIsWriter == true || blul.active == true)
					select blul);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.BrickLinkUserLink materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.BrickLinkUserLink Entity was read with Admin privilege." : "BMC.BrickLinkUserLink Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickLinkUserLink", materialized.id, materialized.encryptedTokenValue));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.BrickLinkUserLink entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.BrickLinkUserLink.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.BrickLinkUserLink.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing BrickLinkUserLink record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/BrickLinkUserLink/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutBrickLinkUserLink(int id, [FromBody]Database.BrickLinkUserLink.BrickLinkUserLinkDTO brickLinkUserLinkDTO, CancellationToken cancellationToken = default)
		{
			if (brickLinkUserLinkDTO == null)
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



			if (id != brickLinkUserLinkDTO.id)
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


			IQueryable<Database.BrickLinkUserLink> query = (from x in _context.BrickLinkUserLinks
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.BrickLinkUserLink existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickLinkUserLink PUT", id.ToString(), new Exception("No BMC.BrickLinkUserLink entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (brickLinkUserLinkDTO.objectGuid == Guid.Empty)
            {
                brickLinkUserLinkDTO.objectGuid = existing.objectGuid;
            }
            else if (brickLinkUserLinkDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a BrickLinkUserLink record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.BrickLinkUserLink cloneOfExisting = (Database.BrickLinkUserLink)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new BrickLinkUserLink object using the data from the existing record, updated with what is in the DTO.
			//
			Database.BrickLinkUserLink brickLinkUserLink = (Database.BrickLinkUserLink)_context.Entry(existing).GetDatabaseValues().ToObject();
			brickLinkUserLink.ApplyDTO(brickLinkUserLinkDTO);
			//
			// The tenant guid for any BrickLinkUserLink being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the BrickLinkUserLink because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				brickLinkUserLink.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (brickLinkUserLink.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.BrickLinkUserLink record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (brickLinkUserLink.encryptedTokenValue != null && brickLinkUserLink.encryptedTokenValue.Length > 500)
			{
				brickLinkUserLink.encryptedTokenValue = brickLinkUserLink.encryptedTokenValue.Substring(0, 500);
			}

			if (brickLinkUserLink.encryptedTokenSecret != null && brickLinkUserLink.encryptedTokenSecret.Length > 500)
			{
				brickLinkUserLink.encryptedTokenSecret = brickLinkUserLink.encryptedTokenSecret.Substring(0, 500);
			}

			if (brickLinkUserLink.syncDirection != null && brickLinkUserLink.syncDirection.Length > 50)
			{
				brickLinkUserLink.syncDirection = brickLinkUserLink.syncDirection.Substring(0, 50);
			}

			if (brickLinkUserLink.lastSyncDate.HasValue == true && brickLinkUserLink.lastSyncDate.Value.Kind != DateTimeKind.Utc)
			{
				brickLinkUserLink.lastSyncDate = brickLinkUserLink.lastSyncDate.Value.ToUniversalTime();
			}

			if (brickLinkUserLink.lastPullDate.HasValue == true && brickLinkUserLink.lastPullDate.Value.Kind != DateTimeKind.Utc)
			{
				brickLinkUserLink.lastPullDate = brickLinkUserLink.lastPullDate.Value.ToUniversalTime();
			}

			if (brickLinkUserLink.lastPushDate.HasValue == true && brickLinkUserLink.lastPushDate.Value.Kind != DateTimeKind.Utc)
			{
				brickLinkUserLink.lastPushDate = brickLinkUserLink.lastPushDate.Value.ToUniversalTime();
			}

			if (brickLinkUserLink.lastSyncError != null && brickLinkUserLink.lastSyncError.Length > 1000)
			{
				brickLinkUserLink.lastSyncError = brickLinkUserLink.lastSyncError.Substring(0, 1000);
			}

			EntityEntry<Database.BrickLinkUserLink> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(brickLinkUserLink);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.BrickLinkUserLink entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickLinkUserLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickLinkUserLink.CreateAnonymousWithFirstLevelSubObjects(brickLinkUserLink)),
					null);


				return Ok(Database.BrickLinkUserLink.CreateAnonymous(brickLinkUserLink));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.BrickLinkUserLink entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickLinkUserLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickLinkUserLink.CreateAnonymousWithFirstLevelSubObjects(brickLinkUserLink)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new BrickLinkUserLink record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickLinkUserLink", Name = "BrickLinkUserLink")]
		public async Task<IActionResult> PostBrickLinkUserLink([FromBody]Database.BrickLinkUserLink.BrickLinkUserLinkDTO brickLinkUserLinkDTO, CancellationToken cancellationToken = default)
		{
			if (brickLinkUserLinkDTO == null)
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
			// Create a new BrickLinkUserLink object using the data from the DTO
			//
			Database.BrickLinkUserLink brickLinkUserLink = Database.BrickLinkUserLink.FromDTO(brickLinkUserLinkDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				brickLinkUserLink.tenantGuid = userTenantGuid;

				if (brickLinkUserLink.encryptedTokenValue != null && brickLinkUserLink.encryptedTokenValue.Length > 500)
				{
					brickLinkUserLink.encryptedTokenValue = brickLinkUserLink.encryptedTokenValue.Substring(0, 500);
				}

				if (brickLinkUserLink.encryptedTokenSecret != null && brickLinkUserLink.encryptedTokenSecret.Length > 500)
				{
					brickLinkUserLink.encryptedTokenSecret = brickLinkUserLink.encryptedTokenSecret.Substring(0, 500);
				}

				if (brickLinkUserLink.syncDirection != null && brickLinkUserLink.syncDirection.Length > 50)
				{
					brickLinkUserLink.syncDirection = brickLinkUserLink.syncDirection.Substring(0, 50);
				}

				if (brickLinkUserLink.lastSyncDate.HasValue == true && brickLinkUserLink.lastSyncDate.Value.Kind != DateTimeKind.Utc)
				{
					brickLinkUserLink.lastSyncDate = brickLinkUserLink.lastSyncDate.Value.ToUniversalTime();
				}

				if (brickLinkUserLink.lastPullDate.HasValue == true && brickLinkUserLink.lastPullDate.Value.Kind != DateTimeKind.Utc)
				{
					brickLinkUserLink.lastPullDate = brickLinkUserLink.lastPullDate.Value.ToUniversalTime();
				}

				if (brickLinkUserLink.lastPushDate.HasValue == true && brickLinkUserLink.lastPushDate.Value.Kind != DateTimeKind.Utc)
				{
					brickLinkUserLink.lastPushDate = brickLinkUserLink.lastPushDate.Value.ToUniversalTime();
				}

				if (brickLinkUserLink.lastSyncError != null && brickLinkUserLink.lastSyncError.Length > 1000)
				{
					brickLinkUserLink.lastSyncError = brickLinkUserLink.lastSyncError.Substring(0, 1000);
				}

				brickLinkUserLink.objectGuid = Guid.NewGuid();
				_context.BrickLinkUserLinks.Add(brickLinkUserLink);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.BrickLinkUserLink entity successfully created.",
					true,
					brickLinkUserLink.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.BrickLinkUserLink.CreateAnonymousWithFirstLevelSubObjects(brickLinkUserLink)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.BrickLinkUserLink entity creation failed.", false, brickLinkUserLink.id.ToString(), "", JsonSerializer.Serialize(brickLinkUserLink), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickLinkUserLink", brickLinkUserLink.id, brickLinkUserLink.encryptedTokenValue));

			return CreatedAtRoute("BrickLinkUserLink", new { id = brickLinkUserLink.id }, Database.BrickLinkUserLink.CreateAnonymousWithFirstLevelSubObjects(brickLinkUserLink));
		}



        /// <summary>
        /// 
        /// This deletes a BrickLinkUserLink record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickLinkUserLink/{id}")]
		[Route("api/BrickLinkUserLink")]
		public async Task<IActionResult> DeleteBrickLinkUserLink(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.BrickLinkUserLink> query = (from x in _context.BrickLinkUserLinks
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.BrickLinkUserLink brickLinkUserLink = await query.FirstOrDefaultAsync(cancellationToken);

			if (brickLinkUserLink == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickLinkUserLink DELETE", id.ToString(), new Exception("No BMC.BrickLinkUserLink entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.BrickLinkUserLink cloneOfExisting = (Database.BrickLinkUserLink)_context.Entry(brickLinkUserLink).GetDatabaseValues().ToObject();


			try
			{
				brickLinkUserLink.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BrickLinkUserLink entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickLinkUserLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickLinkUserLink.CreateAnonymousWithFirstLevelSubObjects(brickLinkUserLink)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BrickLinkUserLink entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickLinkUserLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickLinkUserLink.CreateAnonymousWithFirstLevelSubObjects(brickLinkUserLink)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of BrickLinkUserLink records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/BrickLinkUserLinks/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string encryptedTokenValue = null,
			string encryptedTokenSecret = null,
			bool? syncEnabled = null,
			string syncDirection = null,
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

			IQueryable<Database.BrickLinkUserLink> query = (from blul in _context.BrickLinkUserLinks select blul);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(encryptedTokenValue) == false)
			{
				query = query.Where(blul => blul.encryptedTokenValue == encryptedTokenValue);
			}
			if (string.IsNullOrEmpty(encryptedTokenSecret) == false)
			{
				query = query.Where(blul => blul.encryptedTokenSecret == encryptedTokenSecret);
			}
			if (syncEnabled.HasValue == true)
			{
				query = query.Where(blul => blul.syncEnabled == syncEnabled.Value);
			}
			if (string.IsNullOrEmpty(syncDirection) == false)
			{
				query = query.Where(blul => blul.syncDirection == syncDirection);
			}
			if (lastSyncDate.HasValue == true)
			{
				query = query.Where(blul => blul.lastSyncDate == lastSyncDate.Value);
			}
			if (lastPullDate.HasValue == true)
			{
				query = query.Where(blul => blul.lastPullDate == lastPullDate.Value);
			}
			if (lastPushDate.HasValue == true)
			{
				query = query.Where(blul => blul.lastPushDate == lastPushDate.Value);
			}
			if (string.IsNullOrEmpty(lastSyncError) == false)
			{
				query = query.Where(blul => blul.lastSyncError == lastSyncError);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(blul => blul.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(blul => blul.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(blul => blul.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(blul => blul.deleted == false);
				}
			}
			else
			{
				query = query.Where(blul => blul.active == true);
				query = query.Where(blul => blul.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Brick Link User Link, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.encryptedTokenValue.Contains(anyStringContains)
			       || x.encryptedTokenSecret.Contains(anyStringContains)
			       || x.syncDirection.Contains(anyStringContains)
			       || x.lastSyncError.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.encryptedTokenValue).ThenBy(x => x.encryptedTokenSecret).ThenBy(x => x.syncDirection);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.BrickLinkUserLink.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/BrickLinkUserLink/CreateAuditEvent")]
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
