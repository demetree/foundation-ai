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
    /// This auto generated class provides the basic CRUD operations for the BrickOwlUserLink entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the BrickOwlUserLink entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class BrickOwlUserLinksController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 10;

		private BMCContext _context;

		private ILogger<BrickOwlUserLinksController> _logger;

		public BrickOwlUserLinksController(BMCContext context, ILogger<BrickOwlUserLinksController> logger) : base("BMC", "BrickOwlUserLink")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of BrickOwlUserLinks filtered by the parameters provided.
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
		[Route("api/BrickOwlUserLinks")]
		public async Task<IActionResult> GetBrickOwlUserLinks(
			string encryptedApiKey = null,
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

			IQueryable<Database.BrickOwlUserLink> query = (from boul in _context.BrickOwlUserLinks select boul);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(encryptedApiKey) == false)
			{
				query = query.Where(boul => boul.encryptedApiKey == encryptedApiKey);
			}
			if (syncEnabled.HasValue == true)
			{
				query = query.Where(boul => boul.syncEnabled == syncEnabled.Value);
			}
			if (string.IsNullOrEmpty(syncDirection) == false)
			{
				query = query.Where(boul => boul.syncDirection == syncDirection);
			}
			if (lastSyncDate.HasValue == true)
			{
				query = query.Where(boul => boul.lastSyncDate == lastSyncDate.Value);
			}
			if (lastPullDate.HasValue == true)
			{
				query = query.Where(boul => boul.lastPullDate == lastPullDate.Value);
			}
			if (lastPushDate.HasValue == true)
			{
				query = query.Where(boul => boul.lastPushDate == lastPushDate.Value);
			}
			if (string.IsNullOrEmpty(lastSyncError) == false)
			{
				query = query.Where(boul => boul.lastSyncError == lastSyncError);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(boul => boul.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(boul => boul.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(boul => boul.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(boul => boul.deleted == false);
				}
			}
			else
			{
				query = query.Where(boul => boul.active == true);
				query = query.Where(boul => boul.deleted == false);
			}

			query = query.OrderBy(boul => boul.encryptedApiKey).ThenBy(boul => boul.syncDirection).ThenBy(boul => boul.lastSyncError);


			//
			// Add the any string contains parameter to span all the string fields on the Brick Owl User Link, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.encryptedApiKey.Contains(anyStringContains)
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
			
			List<Database.BrickOwlUserLink> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.BrickOwlUserLink brickOwlUserLink in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(brickOwlUserLink, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.BrickOwlUserLink Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.BrickOwlUserLink Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of BrickOwlUserLinks filtered by the parameters provided.  Its query is similar to the GetBrickOwlUserLinks method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickOwlUserLinks/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string encryptedApiKey = null,
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

			IQueryable<Database.BrickOwlUserLink> query = (from boul in _context.BrickOwlUserLinks select boul);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (encryptedApiKey != null)
			{
				query = query.Where(boul => boul.encryptedApiKey == encryptedApiKey);
			}
			if (syncEnabled.HasValue == true)
			{
				query = query.Where(boul => boul.syncEnabled == syncEnabled.Value);
			}
			if (syncDirection != null)
			{
				query = query.Where(boul => boul.syncDirection == syncDirection);
			}
			if (lastSyncDate.HasValue == true)
			{
				query = query.Where(boul => boul.lastSyncDate == lastSyncDate.Value);
			}
			if (lastPullDate.HasValue == true)
			{
				query = query.Where(boul => boul.lastPullDate == lastPullDate.Value);
			}
			if (lastPushDate.HasValue == true)
			{
				query = query.Where(boul => boul.lastPushDate == lastPushDate.Value);
			}
			if (lastSyncError != null)
			{
				query = query.Where(boul => boul.lastSyncError == lastSyncError);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(boul => boul.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(boul => boul.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(boul => boul.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(boul => boul.deleted == false);
				}
			}
			else
			{
				query = query.Where(boul => boul.active == true);
				query = query.Where(boul => boul.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Brick Owl User Link, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.encryptedApiKey.Contains(anyStringContains)
			       || x.syncDirection.Contains(anyStringContains)
			       || x.lastSyncError.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single BrickOwlUserLink by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickOwlUserLink/{id}")]
		public async Task<IActionResult> GetBrickOwlUserLink(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.BrickOwlUserLink> query = (from boul in _context.BrickOwlUserLinks where
							(boul.id == id) &&
							(userIsAdmin == true || boul.deleted == false) &&
							(userIsWriter == true || boul.active == true)
					select boul);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.BrickOwlUserLink materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.BrickOwlUserLink Entity was read with Admin privilege." : "BMC.BrickOwlUserLink Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickOwlUserLink", materialized.id, materialized.encryptedApiKey));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.BrickOwlUserLink entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.BrickOwlUserLink.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.BrickOwlUserLink.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing BrickOwlUserLink record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/BrickOwlUserLink/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutBrickOwlUserLink(int id, [FromBody]Database.BrickOwlUserLink.BrickOwlUserLinkDTO brickOwlUserLinkDTO, CancellationToken cancellationToken = default)
		{
			if (brickOwlUserLinkDTO == null)
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



			if (id != brickOwlUserLinkDTO.id)
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


			IQueryable<Database.BrickOwlUserLink> query = (from x in _context.BrickOwlUserLinks
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.BrickOwlUserLink existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickOwlUserLink PUT", id.ToString(), new Exception("No BMC.BrickOwlUserLink entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (brickOwlUserLinkDTO.objectGuid == Guid.Empty)
            {
                brickOwlUserLinkDTO.objectGuid = existing.objectGuid;
            }
            else if (brickOwlUserLinkDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a BrickOwlUserLink record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.BrickOwlUserLink cloneOfExisting = (Database.BrickOwlUserLink)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new BrickOwlUserLink object using the data from the existing record, updated with what is in the DTO.
			//
			Database.BrickOwlUserLink brickOwlUserLink = (Database.BrickOwlUserLink)_context.Entry(existing).GetDatabaseValues().ToObject();
			brickOwlUserLink.ApplyDTO(brickOwlUserLinkDTO);
			//
			// The tenant guid for any BrickOwlUserLink being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the BrickOwlUserLink because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				brickOwlUserLink.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (brickOwlUserLink.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.BrickOwlUserLink record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (brickOwlUserLink.encryptedApiKey != null && brickOwlUserLink.encryptedApiKey.Length > 500)
			{
				brickOwlUserLink.encryptedApiKey = brickOwlUserLink.encryptedApiKey.Substring(0, 500);
			}

			if (brickOwlUserLink.syncDirection != null && brickOwlUserLink.syncDirection.Length > 50)
			{
				brickOwlUserLink.syncDirection = brickOwlUserLink.syncDirection.Substring(0, 50);
			}

			if (brickOwlUserLink.lastSyncDate.HasValue == true && brickOwlUserLink.lastSyncDate.Value.Kind != DateTimeKind.Utc)
			{
				brickOwlUserLink.lastSyncDate = brickOwlUserLink.lastSyncDate.Value.ToUniversalTime();
			}

			if (brickOwlUserLink.lastPullDate.HasValue == true && brickOwlUserLink.lastPullDate.Value.Kind != DateTimeKind.Utc)
			{
				brickOwlUserLink.lastPullDate = brickOwlUserLink.lastPullDate.Value.ToUniversalTime();
			}

			if (brickOwlUserLink.lastPushDate.HasValue == true && brickOwlUserLink.lastPushDate.Value.Kind != DateTimeKind.Utc)
			{
				brickOwlUserLink.lastPushDate = brickOwlUserLink.lastPushDate.Value.ToUniversalTime();
			}

			if (brickOwlUserLink.lastSyncError != null && brickOwlUserLink.lastSyncError.Length > 1000)
			{
				brickOwlUserLink.lastSyncError = brickOwlUserLink.lastSyncError.Substring(0, 1000);
			}

			EntityEntry<Database.BrickOwlUserLink> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(brickOwlUserLink);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.BrickOwlUserLink entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickOwlUserLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickOwlUserLink.CreateAnonymousWithFirstLevelSubObjects(brickOwlUserLink)),
					null);


				return Ok(Database.BrickOwlUserLink.CreateAnonymous(brickOwlUserLink));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.BrickOwlUserLink entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickOwlUserLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickOwlUserLink.CreateAnonymousWithFirstLevelSubObjects(brickOwlUserLink)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new BrickOwlUserLink record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickOwlUserLink", Name = "BrickOwlUserLink")]
		public async Task<IActionResult> PostBrickOwlUserLink([FromBody]Database.BrickOwlUserLink.BrickOwlUserLinkDTO brickOwlUserLinkDTO, CancellationToken cancellationToken = default)
		{
			if (brickOwlUserLinkDTO == null)
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
			// Create a new BrickOwlUserLink object using the data from the DTO
			//
			Database.BrickOwlUserLink brickOwlUserLink = Database.BrickOwlUserLink.FromDTO(brickOwlUserLinkDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				brickOwlUserLink.tenantGuid = userTenantGuid;

				if (brickOwlUserLink.encryptedApiKey != null && brickOwlUserLink.encryptedApiKey.Length > 500)
				{
					brickOwlUserLink.encryptedApiKey = brickOwlUserLink.encryptedApiKey.Substring(0, 500);
				}

				if (brickOwlUserLink.syncDirection != null && brickOwlUserLink.syncDirection.Length > 50)
				{
					brickOwlUserLink.syncDirection = brickOwlUserLink.syncDirection.Substring(0, 50);
				}

				if (brickOwlUserLink.lastSyncDate.HasValue == true && brickOwlUserLink.lastSyncDate.Value.Kind != DateTimeKind.Utc)
				{
					brickOwlUserLink.lastSyncDate = brickOwlUserLink.lastSyncDate.Value.ToUniversalTime();
				}

				if (brickOwlUserLink.lastPullDate.HasValue == true && brickOwlUserLink.lastPullDate.Value.Kind != DateTimeKind.Utc)
				{
					brickOwlUserLink.lastPullDate = brickOwlUserLink.lastPullDate.Value.ToUniversalTime();
				}

				if (brickOwlUserLink.lastPushDate.HasValue == true && brickOwlUserLink.lastPushDate.Value.Kind != DateTimeKind.Utc)
				{
					brickOwlUserLink.lastPushDate = brickOwlUserLink.lastPushDate.Value.ToUniversalTime();
				}

				if (brickOwlUserLink.lastSyncError != null && brickOwlUserLink.lastSyncError.Length > 1000)
				{
					brickOwlUserLink.lastSyncError = brickOwlUserLink.lastSyncError.Substring(0, 1000);
				}

				brickOwlUserLink.objectGuid = Guid.NewGuid();
				_context.BrickOwlUserLinks.Add(brickOwlUserLink);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.BrickOwlUserLink entity successfully created.",
					true,
					brickOwlUserLink.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.BrickOwlUserLink.CreateAnonymousWithFirstLevelSubObjects(brickOwlUserLink)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.BrickOwlUserLink entity creation failed.", false, brickOwlUserLink.id.ToString(), "", JsonSerializer.Serialize(brickOwlUserLink), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickOwlUserLink", brickOwlUserLink.id, brickOwlUserLink.encryptedApiKey));

			return CreatedAtRoute("BrickOwlUserLink", new { id = brickOwlUserLink.id }, Database.BrickOwlUserLink.CreateAnonymousWithFirstLevelSubObjects(brickOwlUserLink));
		}



        /// <summary>
        /// 
        /// This deletes a BrickOwlUserLink record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickOwlUserLink/{id}")]
		[Route("api/BrickOwlUserLink")]
		public async Task<IActionResult> DeleteBrickOwlUserLink(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.BrickOwlUserLink> query = (from x in _context.BrickOwlUserLinks
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.BrickOwlUserLink brickOwlUserLink = await query.FirstOrDefaultAsync(cancellationToken);

			if (brickOwlUserLink == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickOwlUserLink DELETE", id.ToString(), new Exception("No BMC.BrickOwlUserLink entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.BrickOwlUserLink cloneOfExisting = (Database.BrickOwlUserLink)_context.Entry(brickOwlUserLink).GetDatabaseValues().ToObject();


			try
			{
				brickOwlUserLink.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BrickOwlUserLink entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickOwlUserLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickOwlUserLink.CreateAnonymousWithFirstLevelSubObjects(brickOwlUserLink)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BrickOwlUserLink entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickOwlUserLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickOwlUserLink.CreateAnonymousWithFirstLevelSubObjects(brickOwlUserLink)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of BrickOwlUserLink records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/BrickOwlUserLinks/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string encryptedApiKey = null,
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

			IQueryable<Database.BrickOwlUserLink> query = (from boul in _context.BrickOwlUserLinks select boul);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(encryptedApiKey) == false)
			{
				query = query.Where(boul => boul.encryptedApiKey == encryptedApiKey);
			}
			if (syncEnabled.HasValue == true)
			{
				query = query.Where(boul => boul.syncEnabled == syncEnabled.Value);
			}
			if (string.IsNullOrEmpty(syncDirection) == false)
			{
				query = query.Where(boul => boul.syncDirection == syncDirection);
			}
			if (lastSyncDate.HasValue == true)
			{
				query = query.Where(boul => boul.lastSyncDate == lastSyncDate.Value);
			}
			if (lastPullDate.HasValue == true)
			{
				query = query.Where(boul => boul.lastPullDate == lastPullDate.Value);
			}
			if (lastPushDate.HasValue == true)
			{
				query = query.Where(boul => boul.lastPushDate == lastPushDate.Value);
			}
			if (string.IsNullOrEmpty(lastSyncError) == false)
			{
				query = query.Where(boul => boul.lastSyncError == lastSyncError);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(boul => boul.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(boul => boul.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(boul => boul.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(boul => boul.deleted == false);
				}
			}
			else
			{
				query = query.Where(boul => boul.active == true);
				query = query.Where(boul => boul.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Brick Owl User Link, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.encryptedApiKey.Contains(anyStringContains)
			       || x.syncDirection.Contains(anyStringContains)
			       || x.lastSyncError.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.encryptedApiKey).ThenBy(x => x.syncDirection).ThenBy(x => x.lastSyncError);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.BrickOwlUserLink.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/BrickOwlUserLink/CreateAuditEvent")]
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
