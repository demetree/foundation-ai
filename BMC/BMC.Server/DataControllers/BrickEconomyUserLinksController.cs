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
    /// This auto generated class provides the basic CRUD operations for the BrickEconomyUserLink entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the BrickEconomyUserLink entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class BrickEconomyUserLinksController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 10;

		private BMCContext _context;

		private ILogger<BrickEconomyUserLinksController> _logger;

		public BrickEconomyUserLinksController(BMCContext context, ILogger<BrickEconomyUserLinksController> logger) : base("BMC", "BrickEconomyUserLink")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of BrickEconomyUserLinks filtered by the parameters provided.
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
		[Route("api/BrickEconomyUserLinks")]
		public async Task<IActionResult> GetBrickEconomyUserLinks(
			string encryptedApiKey = null,
			bool? syncEnabled = null,
			DateTime? lastSyncDate = null,
			string lastSyncError = null,
			int? dailyQuotaUsed = null,
			DateTime? quotaResetDate = null,
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

			if (quotaResetDate.HasValue == true && quotaResetDate.Value.Kind != DateTimeKind.Utc)
			{
				quotaResetDate = quotaResetDate.Value.ToUniversalTime();
			}

			IQueryable<Database.BrickEconomyUserLink> query = (from beul in _context.BrickEconomyUserLinks select beul);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(encryptedApiKey) == false)
			{
				query = query.Where(beul => beul.encryptedApiKey == encryptedApiKey);
			}
			if (syncEnabled.HasValue == true)
			{
				query = query.Where(beul => beul.syncEnabled == syncEnabled.Value);
			}
			if (lastSyncDate.HasValue == true)
			{
				query = query.Where(beul => beul.lastSyncDate == lastSyncDate.Value);
			}
			if (string.IsNullOrEmpty(lastSyncError) == false)
			{
				query = query.Where(beul => beul.lastSyncError == lastSyncError);
			}
			if (dailyQuotaUsed.HasValue == true)
			{
				query = query.Where(beul => beul.dailyQuotaUsed == dailyQuotaUsed.Value);
			}
			if (quotaResetDate.HasValue == true)
			{
				query = query.Where(beul => beul.quotaResetDate == quotaResetDate.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(beul => beul.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(beul => beul.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(beul => beul.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(beul => beul.deleted == false);
				}
			}
			else
			{
				query = query.Where(beul => beul.active == true);
				query = query.Where(beul => beul.deleted == false);
			}

			query = query.OrderBy(beul => beul.encryptedApiKey).ThenBy(beul => beul.lastSyncError);


			//
			// Add the any string contains parameter to span all the string fields on the Brick Economy User Link, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.encryptedApiKey.Contains(anyStringContains)
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
			
			List<Database.BrickEconomyUserLink> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.BrickEconomyUserLink brickEconomyUserLink in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(brickEconomyUserLink, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.BrickEconomyUserLink Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.BrickEconomyUserLink Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of BrickEconomyUserLinks filtered by the parameters provided.  Its query is similar to the GetBrickEconomyUserLinks method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickEconomyUserLinks/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string encryptedApiKey = null,
			bool? syncEnabled = null,
			DateTime? lastSyncDate = null,
			string lastSyncError = null,
			int? dailyQuotaUsed = null,
			DateTime? quotaResetDate = null,
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

			if (quotaResetDate.HasValue == true && quotaResetDate.Value.Kind != DateTimeKind.Utc)
			{
				quotaResetDate = quotaResetDate.Value.ToUniversalTime();
			}

			IQueryable<Database.BrickEconomyUserLink> query = (from beul in _context.BrickEconomyUserLinks select beul);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (encryptedApiKey != null)
			{
				query = query.Where(beul => beul.encryptedApiKey == encryptedApiKey);
			}
			if (syncEnabled.HasValue == true)
			{
				query = query.Where(beul => beul.syncEnabled == syncEnabled.Value);
			}
			if (lastSyncDate.HasValue == true)
			{
				query = query.Where(beul => beul.lastSyncDate == lastSyncDate.Value);
			}
			if (lastSyncError != null)
			{
				query = query.Where(beul => beul.lastSyncError == lastSyncError);
			}
			if (dailyQuotaUsed.HasValue == true)
			{
				query = query.Where(beul => beul.dailyQuotaUsed == dailyQuotaUsed.Value);
			}
			if (quotaResetDate.HasValue == true)
			{
				query = query.Where(beul => beul.quotaResetDate == quotaResetDate.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(beul => beul.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(beul => beul.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(beul => beul.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(beul => beul.deleted == false);
				}
			}
			else
			{
				query = query.Where(beul => beul.active == true);
				query = query.Where(beul => beul.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Brick Economy User Link, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.encryptedApiKey.Contains(anyStringContains)
			       || x.lastSyncError.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single BrickEconomyUserLink by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickEconomyUserLink/{id}")]
		public async Task<IActionResult> GetBrickEconomyUserLink(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.BrickEconomyUserLink> query = (from beul in _context.BrickEconomyUserLinks where
							(beul.id == id) &&
							(userIsAdmin == true || beul.deleted == false) &&
							(userIsWriter == true || beul.active == true)
					select beul);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.BrickEconomyUserLink materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.BrickEconomyUserLink Entity was read with Admin privilege." : "BMC.BrickEconomyUserLink Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickEconomyUserLink", materialized.id, materialized.encryptedApiKey));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.BrickEconomyUserLink entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.BrickEconomyUserLink.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.BrickEconomyUserLink.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing BrickEconomyUserLink record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/BrickEconomyUserLink/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutBrickEconomyUserLink(int id, [FromBody]Database.BrickEconomyUserLink.BrickEconomyUserLinkDTO brickEconomyUserLinkDTO, CancellationToken cancellationToken = default)
		{
			if (brickEconomyUserLinkDTO == null)
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



			if (id != brickEconomyUserLinkDTO.id)
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


			IQueryable<Database.BrickEconomyUserLink> query = (from x in _context.BrickEconomyUserLinks
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.BrickEconomyUserLink existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickEconomyUserLink PUT", id.ToString(), new Exception("No BMC.BrickEconomyUserLink entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (brickEconomyUserLinkDTO.objectGuid == Guid.Empty)
            {
                brickEconomyUserLinkDTO.objectGuid = existing.objectGuid;
            }
            else if (brickEconomyUserLinkDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a BrickEconomyUserLink record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.BrickEconomyUserLink cloneOfExisting = (Database.BrickEconomyUserLink)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new BrickEconomyUserLink object using the data from the existing record, updated with what is in the DTO.
			//
			Database.BrickEconomyUserLink brickEconomyUserLink = (Database.BrickEconomyUserLink)_context.Entry(existing).GetDatabaseValues().ToObject();
			brickEconomyUserLink.ApplyDTO(brickEconomyUserLinkDTO);
			//
			// The tenant guid for any BrickEconomyUserLink being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the BrickEconomyUserLink because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				brickEconomyUserLink.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (brickEconomyUserLink.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.BrickEconomyUserLink record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (brickEconomyUserLink.encryptedApiKey != null && brickEconomyUserLink.encryptedApiKey.Length > 500)
			{
				brickEconomyUserLink.encryptedApiKey = brickEconomyUserLink.encryptedApiKey.Substring(0, 500);
			}

			if (brickEconomyUserLink.lastSyncDate.HasValue == true && brickEconomyUserLink.lastSyncDate.Value.Kind != DateTimeKind.Utc)
			{
				brickEconomyUserLink.lastSyncDate = brickEconomyUserLink.lastSyncDate.Value.ToUniversalTime();
			}

			if (brickEconomyUserLink.lastSyncError != null && brickEconomyUserLink.lastSyncError.Length > 1000)
			{
				brickEconomyUserLink.lastSyncError = brickEconomyUserLink.lastSyncError.Substring(0, 1000);
			}

			if (brickEconomyUserLink.quotaResetDate.HasValue == true && brickEconomyUserLink.quotaResetDate.Value.Kind != DateTimeKind.Utc)
			{
				brickEconomyUserLink.quotaResetDate = brickEconomyUserLink.quotaResetDate.Value.ToUniversalTime();
			}

			EntityEntry<Database.BrickEconomyUserLink> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(brickEconomyUserLink);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.BrickEconomyUserLink entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickEconomyUserLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickEconomyUserLink.CreateAnonymousWithFirstLevelSubObjects(brickEconomyUserLink)),
					null);


				return Ok(Database.BrickEconomyUserLink.CreateAnonymous(brickEconomyUserLink));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.BrickEconomyUserLink entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickEconomyUserLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickEconomyUserLink.CreateAnonymousWithFirstLevelSubObjects(brickEconomyUserLink)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new BrickEconomyUserLink record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickEconomyUserLink", Name = "BrickEconomyUserLink")]
		public async Task<IActionResult> PostBrickEconomyUserLink([FromBody]Database.BrickEconomyUserLink.BrickEconomyUserLinkDTO brickEconomyUserLinkDTO, CancellationToken cancellationToken = default)
		{
			if (brickEconomyUserLinkDTO == null)
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
			// Create a new BrickEconomyUserLink object using the data from the DTO
			//
			Database.BrickEconomyUserLink brickEconomyUserLink = Database.BrickEconomyUserLink.FromDTO(brickEconomyUserLinkDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				brickEconomyUserLink.tenantGuid = userTenantGuid;

				if (brickEconomyUserLink.encryptedApiKey != null && brickEconomyUserLink.encryptedApiKey.Length > 500)
				{
					brickEconomyUserLink.encryptedApiKey = brickEconomyUserLink.encryptedApiKey.Substring(0, 500);
				}

				if (brickEconomyUserLink.lastSyncDate.HasValue == true && brickEconomyUserLink.lastSyncDate.Value.Kind != DateTimeKind.Utc)
				{
					brickEconomyUserLink.lastSyncDate = brickEconomyUserLink.lastSyncDate.Value.ToUniversalTime();
				}

				if (brickEconomyUserLink.lastSyncError != null && brickEconomyUserLink.lastSyncError.Length > 1000)
				{
					brickEconomyUserLink.lastSyncError = brickEconomyUserLink.lastSyncError.Substring(0, 1000);
				}

				if (brickEconomyUserLink.quotaResetDate.HasValue == true && brickEconomyUserLink.quotaResetDate.Value.Kind != DateTimeKind.Utc)
				{
					brickEconomyUserLink.quotaResetDate = brickEconomyUserLink.quotaResetDate.Value.ToUniversalTime();
				}

				brickEconomyUserLink.objectGuid = Guid.NewGuid();
				_context.BrickEconomyUserLinks.Add(brickEconomyUserLink);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.BrickEconomyUserLink entity successfully created.",
					true,
					brickEconomyUserLink.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.BrickEconomyUserLink.CreateAnonymousWithFirstLevelSubObjects(brickEconomyUserLink)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.BrickEconomyUserLink entity creation failed.", false, brickEconomyUserLink.id.ToString(), "", JsonSerializer.Serialize(brickEconomyUserLink), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickEconomyUserLink", brickEconomyUserLink.id, brickEconomyUserLink.encryptedApiKey));

			return CreatedAtRoute("BrickEconomyUserLink", new { id = brickEconomyUserLink.id }, Database.BrickEconomyUserLink.CreateAnonymousWithFirstLevelSubObjects(brickEconomyUserLink));
		}



        /// <summary>
        /// 
        /// This deletes a BrickEconomyUserLink record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickEconomyUserLink/{id}")]
		[Route("api/BrickEconomyUserLink")]
		public async Task<IActionResult> DeleteBrickEconomyUserLink(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.BrickEconomyUserLink> query = (from x in _context.BrickEconomyUserLinks
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.BrickEconomyUserLink brickEconomyUserLink = await query.FirstOrDefaultAsync(cancellationToken);

			if (brickEconomyUserLink == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickEconomyUserLink DELETE", id.ToString(), new Exception("No BMC.BrickEconomyUserLink entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.BrickEconomyUserLink cloneOfExisting = (Database.BrickEconomyUserLink)_context.Entry(brickEconomyUserLink).GetDatabaseValues().ToObject();


			try
			{
				brickEconomyUserLink.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BrickEconomyUserLink entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickEconomyUserLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickEconomyUserLink.CreateAnonymousWithFirstLevelSubObjects(brickEconomyUserLink)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BrickEconomyUserLink entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickEconomyUserLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickEconomyUserLink.CreateAnonymousWithFirstLevelSubObjects(brickEconomyUserLink)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of BrickEconomyUserLink records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/BrickEconomyUserLinks/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string encryptedApiKey = null,
			bool? syncEnabled = null,
			DateTime? lastSyncDate = null,
			string lastSyncError = null,
			int? dailyQuotaUsed = null,
			DateTime? quotaResetDate = null,
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

			if (quotaResetDate.HasValue == true && quotaResetDate.Value.Kind != DateTimeKind.Utc)
			{
				quotaResetDate = quotaResetDate.Value.ToUniversalTime();
			}

			IQueryable<Database.BrickEconomyUserLink> query = (from beul in _context.BrickEconomyUserLinks select beul);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(encryptedApiKey) == false)
			{
				query = query.Where(beul => beul.encryptedApiKey == encryptedApiKey);
			}
			if (syncEnabled.HasValue == true)
			{
				query = query.Where(beul => beul.syncEnabled == syncEnabled.Value);
			}
			if (lastSyncDate.HasValue == true)
			{
				query = query.Where(beul => beul.lastSyncDate == lastSyncDate.Value);
			}
			if (string.IsNullOrEmpty(lastSyncError) == false)
			{
				query = query.Where(beul => beul.lastSyncError == lastSyncError);
			}
			if (dailyQuotaUsed.HasValue == true)
			{
				query = query.Where(beul => beul.dailyQuotaUsed == dailyQuotaUsed.Value);
			}
			if (quotaResetDate.HasValue == true)
			{
				query = query.Where(beul => beul.quotaResetDate == quotaResetDate.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(beul => beul.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(beul => beul.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(beul => beul.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(beul => beul.deleted == false);
				}
			}
			else
			{
				query = query.Where(beul => beul.active == true);
				query = query.Where(beul => beul.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Brick Economy User Link, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.encryptedApiKey.Contains(anyStringContains)
			       || x.lastSyncError.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.encryptedApiKey).ThenBy(x => x.lastSyncError);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.BrickEconomyUserLink.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/BrickEconomyUserLink/CreateAuditEvent")]
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
