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
using Foundation.Scheduler.Database;
using Foundation.ChangeHistory;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the SalesforceTenantLink entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SalesforceTenantLink entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SalesforceTenantLinksController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		static object salesforceTenantLinkPutSyncRoot = new object();
		static object salesforceTenantLinkDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<SalesforceTenantLinksController> _logger;

		public SalesforceTenantLinksController(SchedulerContext context, ILogger<SalesforceTenantLinksController> logger) : base("Scheduler", "SalesforceTenantLink")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of SalesforceTenantLinks filtered by the parameters provided.
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
		[Route("api/SalesforceTenantLinks")]
		public async Task<IActionResult> GetSalesforceTenantLinks(
			bool? syncEnabled = null,
			string syncDirectionFlags = null,
			int? pullIntervalMinutes = null,
			DateTime? lastPullDate = null,
			string loginUrl = null,
			string sfClientId = null,
			string sfClientSecret = null,
			string sfUsername = null,
			string sfPassword = null,
			string sfSecurityToken = null,
			string instanceUrl = null,
			string apiVersion = null,
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
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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
			if (lastPullDate.HasValue == true && lastPullDate.Value.Kind != DateTimeKind.Utc)
			{
				lastPullDate = lastPullDate.Value.ToUniversalTime();
			}

			IQueryable<Database.SalesforceTenantLink> query = (from stl in _context.SalesforceTenantLinks select stl);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (syncEnabled.HasValue == true)
			{
				query = query.Where(stl => stl.syncEnabled == syncEnabled.Value);
			}
			if (string.IsNullOrEmpty(syncDirectionFlags) == false)
			{
				query = query.Where(stl => stl.syncDirectionFlags == syncDirectionFlags);
			}
			if (pullIntervalMinutes.HasValue == true)
			{
				query = query.Where(stl => stl.pullIntervalMinutes == pullIntervalMinutes.Value);
			}
			if (lastPullDate.HasValue == true)
			{
				query = query.Where(stl => stl.lastPullDate == lastPullDate.Value);
			}
			if (string.IsNullOrEmpty(loginUrl) == false)
			{
				query = query.Where(stl => stl.loginUrl == loginUrl);
			}
			if (string.IsNullOrEmpty(sfClientId) == false)
			{
				query = query.Where(stl => stl.sfClientId == sfClientId);
			}
			if (string.IsNullOrEmpty(sfClientSecret) == false)
			{
				query = query.Where(stl => stl.sfClientSecret == sfClientSecret);
			}
			if (string.IsNullOrEmpty(sfUsername) == false)
			{
				query = query.Where(stl => stl.sfUsername == sfUsername);
			}
			if (string.IsNullOrEmpty(sfPassword) == false)
			{
				query = query.Where(stl => stl.sfPassword == sfPassword);
			}
			if (string.IsNullOrEmpty(sfSecurityToken) == false)
			{
				query = query.Where(stl => stl.sfSecurityToken == sfSecurityToken);
			}
			if (string.IsNullOrEmpty(instanceUrl) == false)
			{
				query = query.Where(stl => stl.instanceUrl == instanceUrl);
			}
			if (string.IsNullOrEmpty(apiVersion) == false)
			{
				query = query.Where(stl => stl.apiVersion == apiVersion);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(stl => stl.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(stl => stl.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(stl => stl.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(stl => stl.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(stl => stl.deleted == false);
				}
			}
			else
			{
				query = query.Where(stl => stl.active == true);
				query = query.Where(stl => stl.deleted == false);
			}

			query = query.OrderBy(stl => stl.syncDirectionFlags).ThenBy(stl => stl.loginUrl).ThenBy(stl => stl.sfClientId);


			//
			// Add the any string contains parameter to span all the string fields on the Salesforce Tenant Link, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.syncDirectionFlags.Contains(anyStringContains)
			       || x.loginUrl.Contains(anyStringContains)
			       || x.sfClientId.Contains(anyStringContains)
			       || x.sfClientSecret.Contains(anyStringContains)
			       || x.sfUsername.Contains(anyStringContains)
			       || x.sfPassword.Contains(anyStringContains)
			       || x.sfSecurityToken.Contains(anyStringContains)
			       || x.instanceUrl.Contains(anyStringContains)
			       || x.apiVersion.Contains(anyStringContains)
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
			
			List<Database.SalesforceTenantLink> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SalesforceTenantLink salesforceTenantLink in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(salesforceTenantLink, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.SalesforceTenantLink Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.SalesforceTenantLink Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of SalesforceTenantLinks filtered by the parameters provided.  Its query is similar to the GetSalesforceTenantLinks method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SalesforceTenantLinks/RowCount")]
		public async Task<IActionResult> GetRowCount(
			bool? syncEnabled = null,
			string syncDirectionFlags = null,
			int? pullIntervalMinutes = null,
			DateTime? lastPullDate = null,
			string loginUrl = null,
			string sfClientId = null,
			string sfClientSecret = null,
			string sfUsername = null,
			string sfPassword = null,
			string sfSecurityToken = null,
			string instanceUrl = null,
			string apiVersion = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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
			if (lastPullDate.HasValue == true && lastPullDate.Value.Kind != DateTimeKind.Utc)
			{
				lastPullDate = lastPullDate.Value.ToUniversalTime();
			}

			IQueryable<Database.SalesforceTenantLink> query = (from stl in _context.SalesforceTenantLinks select stl);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (syncEnabled.HasValue == true)
			{
				query = query.Where(stl => stl.syncEnabled == syncEnabled.Value);
			}
			if (syncDirectionFlags != null)
			{
				query = query.Where(stl => stl.syncDirectionFlags == syncDirectionFlags);
			}
			if (pullIntervalMinutes.HasValue == true)
			{
				query = query.Where(stl => stl.pullIntervalMinutes == pullIntervalMinutes.Value);
			}
			if (lastPullDate.HasValue == true)
			{
				query = query.Where(stl => stl.lastPullDate == lastPullDate.Value);
			}
			if (loginUrl != null)
			{
				query = query.Where(stl => stl.loginUrl == loginUrl);
			}
			if (sfClientId != null)
			{
				query = query.Where(stl => stl.sfClientId == sfClientId);
			}
			if (sfClientSecret != null)
			{
				query = query.Where(stl => stl.sfClientSecret == sfClientSecret);
			}
			if (sfUsername != null)
			{
				query = query.Where(stl => stl.sfUsername == sfUsername);
			}
			if (sfPassword != null)
			{
				query = query.Where(stl => stl.sfPassword == sfPassword);
			}
			if (sfSecurityToken != null)
			{
				query = query.Where(stl => stl.sfSecurityToken == sfSecurityToken);
			}
			if (instanceUrl != null)
			{
				query = query.Where(stl => stl.instanceUrl == instanceUrl);
			}
			if (apiVersion != null)
			{
				query = query.Where(stl => stl.apiVersion == apiVersion);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(stl => stl.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(stl => stl.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(stl => stl.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(stl => stl.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(stl => stl.deleted == false);
				}
			}
			else
			{
				query = query.Where(stl => stl.active == true);
				query = query.Where(stl => stl.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Salesforce Tenant Link, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.syncDirectionFlags.Contains(anyStringContains)
			       || x.loginUrl.Contains(anyStringContains)
			       || x.sfClientId.Contains(anyStringContains)
			       || x.sfClientSecret.Contains(anyStringContains)
			       || x.sfUsername.Contains(anyStringContains)
			       || x.sfPassword.Contains(anyStringContains)
			       || x.sfSecurityToken.Contains(anyStringContains)
			       || x.instanceUrl.Contains(anyStringContains)
			       || x.apiVersion.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single SalesforceTenantLink by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SalesforceTenantLink/{id}")]
		public async Task<IActionResult> GetSalesforceTenantLink(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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
				IQueryable<Database.SalesforceTenantLink> query = (from stl in _context.SalesforceTenantLinks where
							(stl.id == id) &&
							(userIsAdmin == true || stl.deleted == false) &&
							(userIsWriter == true || stl.active == true)
					select stl);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.SalesforceTenantLink materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.SalesforceTenantLink Entity was read with Admin privilege." : "Scheduler.SalesforceTenantLink Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SalesforceTenantLink", materialized.id, materialized.syncDirectionFlags));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.SalesforceTenantLink entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.SalesforceTenantLink.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.SalesforceTenantLink.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing SalesforceTenantLink record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SalesforceTenantLink/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSalesforceTenantLink(int id, [FromBody]Database.SalesforceTenantLink.SalesforceTenantLinkDTO salesforceTenantLinkDTO, CancellationToken cancellationToken = default)
		{
			if (salesforceTenantLinkDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != salesforceTenantLinkDTO.id)
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


			IQueryable<Database.SalesforceTenantLink> query = (from x in _context.SalesforceTenantLinks
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.SalesforceTenantLink existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.SalesforceTenantLink PUT", id.ToString(), new Exception("No Scheduler.SalesforceTenantLink entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (salesforceTenantLinkDTO.objectGuid == Guid.Empty)
            {
                salesforceTenantLinkDTO.objectGuid = existing.objectGuid;
            }
            else if (salesforceTenantLinkDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a SalesforceTenantLink record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SalesforceTenantLink cloneOfExisting = (Database.SalesforceTenantLink)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SalesforceTenantLink object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SalesforceTenantLink salesforceTenantLink = (Database.SalesforceTenantLink)_context.Entry(existing).GetDatabaseValues().ToObject();
			salesforceTenantLink.ApplyDTO(salesforceTenantLinkDTO);
			//
			// The tenant guid for any SalesforceTenantLink being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the SalesforceTenantLink because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				salesforceTenantLink.tenantGuid = existing.tenantGuid;
			}

			lock (salesforceTenantLinkPutSyncRoot)
			{
				//
				// Validate the version number for the salesforceTenantLink being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != salesforceTenantLink.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "SalesforceTenantLink save attempt was made but save request was with version " + salesforceTenantLink.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The SalesforceTenantLink you are trying to update has already changed.  Please try your save again after reloading the SalesforceTenantLink.");
				}
				else
				{
					// Same record.  Increase version.
					salesforceTenantLink.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (salesforceTenantLink.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.SalesforceTenantLink record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (salesforceTenantLink.syncDirectionFlags != null && salesforceTenantLink.syncDirectionFlags.Length > 100)
				{
					salesforceTenantLink.syncDirectionFlags = salesforceTenantLink.syncDirectionFlags.Substring(0, 100);
				}

				if (salesforceTenantLink.lastPullDate.HasValue == true && salesforceTenantLink.lastPullDate.Value.Kind != DateTimeKind.Utc)
				{
					salesforceTenantLink.lastPullDate = salesforceTenantLink.lastPullDate.Value.ToUniversalTime();
				}

				if (salesforceTenantLink.loginUrl != null && salesforceTenantLink.loginUrl.Length > 250)
				{
					salesforceTenantLink.loginUrl = salesforceTenantLink.loginUrl.Substring(0, 250);
				}

				if (salesforceTenantLink.sfClientId != null && salesforceTenantLink.sfClientId.Length > 250)
				{
					salesforceTenantLink.sfClientId = salesforceTenantLink.sfClientId.Substring(0, 250);
				}

				if (salesforceTenantLink.sfClientSecret != null && salesforceTenantLink.sfClientSecret.Length > 500)
				{
					salesforceTenantLink.sfClientSecret = salesforceTenantLink.sfClientSecret.Substring(0, 500);
				}

				if (salesforceTenantLink.sfUsername != null && salesforceTenantLink.sfUsername.Length > 250)
				{
					salesforceTenantLink.sfUsername = salesforceTenantLink.sfUsername.Substring(0, 250);
				}

				if (salesforceTenantLink.sfPassword != null && salesforceTenantLink.sfPassword.Length > 500)
				{
					salesforceTenantLink.sfPassword = salesforceTenantLink.sfPassword.Substring(0, 500);
				}

				if (salesforceTenantLink.sfSecurityToken != null && salesforceTenantLink.sfSecurityToken.Length > 250)
				{
					salesforceTenantLink.sfSecurityToken = salesforceTenantLink.sfSecurityToken.Substring(0, 250);
				}

				if (salesforceTenantLink.instanceUrl != null && salesforceTenantLink.instanceUrl.Length > 250)
				{
					salesforceTenantLink.instanceUrl = salesforceTenantLink.instanceUrl.Substring(0, 250);
				}

				if (salesforceTenantLink.apiVersion != null && salesforceTenantLink.apiVersion.Length > 50)
				{
					salesforceTenantLink.apiVersion = salesforceTenantLink.apiVersion.Substring(0, 50);
				}

				try
				{
				    EntityEntry<Database.SalesforceTenantLink> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(salesforceTenantLink);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        SalesforceTenantLinkChangeHistory salesforceTenantLinkChangeHistory = new SalesforceTenantLinkChangeHistory();
				        salesforceTenantLinkChangeHistory.salesforceTenantLinkId = salesforceTenantLink.id;
				        salesforceTenantLinkChangeHistory.versionNumber = salesforceTenantLink.versionNumber;
				        salesforceTenantLinkChangeHistory.timeStamp = DateTime.UtcNow;
				        salesforceTenantLinkChangeHistory.userId = securityUser.id;
				        salesforceTenantLinkChangeHistory.tenantGuid = userTenantGuid;
				        salesforceTenantLinkChangeHistory.data = JsonSerializer.Serialize(Database.SalesforceTenantLink.CreateAnonymousWithFirstLevelSubObjects(salesforceTenantLink));
				        _context.SalesforceTenantLinkChangeHistories.Add(salesforceTenantLinkChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.SalesforceTenantLink entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.SalesforceTenantLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.SalesforceTenantLink.CreateAnonymousWithFirstLevelSubObjects(salesforceTenantLink)),
						null);

				return Ok(Database.SalesforceTenantLink.CreateAnonymous(salesforceTenantLink));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.SalesforceTenantLink entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.SalesforceTenantLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.SalesforceTenantLink.CreateAnonymousWithFirstLevelSubObjects(salesforceTenantLink)),
						ex);

					return Problem(ex.Message);
				}

			}
		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new SalesforceTenantLink record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SalesforceTenantLink", Name = "SalesforceTenantLink")]
		public async Task<IActionResult> PostSalesforceTenantLink([FromBody]Database.SalesforceTenantLink.SalesforceTenantLinkDTO salesforceTenantLinkDTO, CancellationToken cancellationToken = default)
		{
			if (salesforceTenantLinkDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
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
			// Create a new SalesforceTenantLink object using the data from the DTO
			//
			Database.SalesforceTenantLink salesforceTenantLink = Database.SalesforceTenantLink.FromDTO(salesforceTenantLinkDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				salesforceTenantLink.tenantGuid = userTenantGuid;

				if (salesforceTenantLink.syncDirectionFlags != null && salesforceTenantLink.syncDirectionFlags.Length > 100)
				{
					salesforceTenantLink.syncDirectionFlags = salesforceTenantLink.syncDirectionFlags.Substring(0, 100);
				}

				if (salesforceTenantLink.lastPullDate.HasValue == true && salesforceTenantLink.lastPullDate.Value.Kind != DateTimeKind.Utc)
				{
					salesforceTenantLink.lastPullDate = salesforceTenantLink.lastPullDate.Value.ToUniversalTime();
				}

				if (salesforceTenantLink.loginUrl != null && salesforceTenantLink.loginUrl.Length > 250)
				{
					salesforceTenantLink.loginUrl = salesforceTenantLink.loginUrl.Substring(0, 250);
				}

				if (salesforceTenantLink.sfClientId != null && salesforceTenantLink.sfClientId.Length > 250)
				{
					salesforceTenantLink.sfClientId = salesforceTenantLink.sfClientId.Substring(0, 250);
				}

				if (salesforceTenantLink.sfClientSecret != null && salesforceTenantLink.sfClientSecret.Length > 500)
				{
					salesforceTenantLink.sfClientSecret = salesforceTenantLink.sfClientSecret.Substring(0, 500);
				}

				if (salesforceTenantLink.sfUsername != null && salesforceTenantLink.sfUsername.Length > 250)
				{
					salesforceTenantLink.sfUsername = salesforceTenantLink.sfUsername.Substring(0, 250);
				}

				if (salesforceTenantLink.sfPassword != null && salesforceTenantLink.sfPassword.Length > 500)
				{
					salesforceTenantLink.sfPassword = salesforceTenantLink.sfPassword.Substring(0, 500);
				}

				if (salesforceTenantLink.sfSecurityToken != null && salesforceTenantLink.sfSecurityToken.Length > 250)
				{
					salesforceTenantLink.sfSecurityToken = salesforceTenantLink.sfSecurityToken.Substring(0, 250);
				}

				if (salesforceTenantLink.instanceUrl != null && salesforceTenantLink.instanceUrl.Length > 250)
				{
					salesforceTenantLink.instanceUrl = salesforceTenantLink.instanceUrl.Substring(0, 250);
				}

				if (salesforceTenantLink.apiVersion != null && salesforceTenantLink.apiVersion.Length > 50)
				{
					salesforceTenantLink.apiVersion = salesforceTenantLink.apiVersion.Substring(0, 50);
				}

				salesforceTenantLink.objectGuid = Guid.NewGuid();
				salesforceTenantLink.versionNumber = 1;

				_context.SalesforceTenantLinks.Add(salesforceTenantLink);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the salesforceTenantLink object so that no further changes will be written to the database
				    //
				    _context.Entry(salesforceTenantLink).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					salesforceTenantLink.SalesforceTenantLinkChangeHistories = null;


				    SalesforceTenantLinkChangeHistory salesforceTenantLinkChangeHistory = new SalesforceTenantLinkChangeHistory();
				    salesforceTenantLinkChangeHistory.salesforceTenantLinkId = salesforceTenantLink.id;
				    salesforceTenantLinkChangeHistory.versionNumber = salesforceTenantLink.versionNumber;
				    salesforceTenantLinkChangeHistory.timeStamp = DateTime.UtcNow;
				    salesforceTenantLinkChangeHistory.userId = securityUser.id;
				    salesforceTenantLinkChangeHistory.tenantGuid = userTenantGuid;
				    salesforceTenantLinkChangeHistory.data = JsonSerializer.Serialize(Database.SalesforceTenantLink.CreateAnonymousWithFirstLevelSubObjects(salesforceTenantLink));
				    _context.SalesforceTenantLinkChangeHistories.Add(salesforceTenantLinkChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.SalesforceTenantLink entity successfully created.",
						true,
						salesforceTenantLink. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.SalesforceTenantLink.CreateAnonymousWithFirstLevelSubObjects(salesforceTenantLink)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.SalesforceTenantLink entity creation failed.", false, salesforceTenantLink.id.ToString(), "", JsonSerializer.Serialize(Database.SalesforceTenantLink.CreateAnonymousWithFirstLevelSubObjects(salesforceTenantLink)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SalesforceTenantLink", salesforceTenantLink.id, salesforceTenantLink.syncDirectionFlags));

			return CreatedAtRoute("SalesforceTenantLink", new { id = salesforceTenantLink.id }, Database.SalesforceTenantLink.CreateAnonymousWithFirstLevelSubObjects(salesforceTenantLink));
		}

*/

/* This function is expected to be overridden in a custom file

        /// <summary>
        /// 
        /// This rolls a SalesforceTenantLink entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SalesforceTenantLink/Rollback/{id}")]
		[Route("api/SalesforceTenantLink/Rollback")]
		public async Task<IActionResult> RollbackToSalesforceTenantLinkVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.SalesforceTenantLink> query = (from x in _context.SalesforceTenantLinks
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this SalesforceTenantLink concurrently
			//
			lock (salesforceTenantLinkPutSyncRoot)
			{
				
				Database.SalesforceTenantLink salesforceTenantLink = query.FirstOrDefault();
				
				if (salesforceTenantLink == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.SalesforceTenantLink rollback", id.ToString(), new Exception("No Scheduler.SalesforceTenantLink entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the SalesforceTenantLink current state so we can log it.
				//
				Database.SalesforceTenantLink cloneOfExisting = (Database.SalesforceTenantLink)_context.Entry(salesforceTenantLink).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.SalesforceTenantLinkChangeHistories = null;

				if (versionNumber >= salesforceTenantLink.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.SalesforceTenantLink rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.SalesforceTenantLink rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				SalesforceTenantLinkChangeHistory salesforceTenantLinkChangeHistory = (from x in _context.SalesforceTenantLinkChangeHistories
				                                               where
				                                               x.salesforceTenantLinkId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (salesforceTenantLinkChangeHistory != null)
				{
				    Database.SalesforceTenantLink oldSalesforceTenantLink = JsonSerializer.Deserialize<Database.SalesforceTenantLink>(salesforceTenantLinkChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    salesforceTenantLink.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    salesforceTenantLink.syncEnabled = oldSalesforceTenantLink.syncEnabled;
				    salesforceTenantLink.syncDirectionFlags = oldSalesforceTenantLink.syncDirectionFlags;
				    salesforceTenantLink.pullIntervalMinutes = oldSalesforceTenantLink.pullIntervalMinutes;
				    salesforceTenantLink.lastPullDate = oldSalesforceTenantLink.lastPullDate;
				    salesforceTenantLink.loginUrl = oldSalesforceTenantLink.loginUrl;
				    salesforceTenantLink.sfClientId = oldSalesforceTenantLink.sfClientId;
				    salesforceTenantLink.sfClientSecret = oldSalesforceTenantLink.sfClientSecret;
				    salesforceTenantLink.sfUsername = oldSalesforceTenantLink.sfUsername;
				    salesforceTenantLink.sfPassword = oldSalesforceTenantLink.sfPassword;
				    salesforceTenantLink.sfSecurityToken = oldSalesforceTenantLink.sfSecurityToken;
				    salesforceTenantLink.instanceUrl = oldSalesforceTenantLink.instanceUrl;
				    salesforceTenantLink.apiVersion = oldSalesforceTenantLink.apiVersion;
				    salesforceTenantLink.objectGuid = oldSalesforceTenantLink.objectGuid;
				    salesforceTenantLink.active = oldSalesforceTenantLink.active;
				    salesforceTenantLink.deleted = oldSalesforceTenantLink.deleted;

				    string serializedSalesforceTenantLink = JsonSerializer.Serialize(salesforceTenantLink);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        SalesforceTenantLinkChangeHistory newSalesforceTenantLinkChangeHistory = new SalesforceTenantLinkChangeHistory();
				        newSalesforceTenantLinkChangeHistory.salesforceTenantLinkId = salesforceTenantLink.id;
				        newSalesforceTenantLinkChangeHistory.versionNumber = salesforceTenantLink.versionNumber;
				        newSalesforceTenantLinkChangeHistory.timeStamp = DateTime.UtcNow;
				        newSalesforceTenantLinkChangeHistory.userId = securityUser.id;
				        newSalesforceTenantLinkChangeHistory.tenantGuid = userTenantGuid;
				        newSalesforceTenantLinkChangeHistory.data = JsonSerializer.Serialize(Database.SalesforceTenantLink.CreateAnonymousWithFirstLevelSubObjects(salesforceTenantLink));
				        _context.SalesforceTenantLinkChangeHistories.Add(newSalesforceTenantLinkChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.SalesforceTenantLink rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.SalesforceTenantLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.SalesforceTenantLink.CreateAnonymousWithFirstLevelSubObjects(salesforceTenantLink)),
						null);


				    return Ok(Database.SalesforceTenantLink.CreateAnonymous(salesforceTenantLink));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.SalesforceTenantLink rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.SalesforceTenantLink rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}

*/



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a SalesforceTenantLink.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the SalesforceTenantLink</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SalesforceTenantLink/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetSalesforceTenantLinkChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.SalesforceTenantLink salesforceTenantLink = await _context.SalesforceTenantLinks.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (salesforceTenantLink == null)
			{
				return NotFound();
			}

			try
			{
				salesforceTenantLink.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.SalesforceTenantLink> versionInfo = await salesforceTenantLink.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a SalesforceTenantLink.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the SalesforceTenantLink</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SalesforceTenantLink/{id}/AuditHistory")]
		public async Task<IActionResult> GetSalesforceTenantLinkAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.SalesforceTenantLink salesforceTenantLink = await _context.SalesforceTenantLinks.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (salesforceTenantLink == null)
			{
				return NotFound();
			}

			try
			{
				salesforceTenantLink.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.SalesforceTenantLink>> versions = await salesforceTenantLink.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a SalesforceTenantLink.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the SalesforceTenantLink</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The SalesforceTenantLink object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SalesforceTenantLink/{id}/Version/{version}")]
		public async Task<IActionResult> GetSalesforceTenantLinkVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.SalesforceTenantLink salesforceTenantLink = await _context.SalesforceTenantLinks.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (salesforceTenantLink == null)
			{
				return NotFound();
			}

			try
			{
				salesforceTenantLink.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.SalesforceTenantLink> versionInfo = await salesforceTenantLink.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a SalesforceTenantLink at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the SalesforceTenantLink</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The SalesforceTenantLink object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SalesforceTenantLink/{id}/StateAtTime")]
		public async Task<IActionResult> GetSalesforceTenantLinkStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.SalesforceTenantLink salesforceTenantLink = await _context.SalesforceTenantLinks.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (salesforceTenantLink == null)
			{
				return NotFound();
			}

			try
			{
				salesforceTenantLink.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.SalesforceTenantLink> versionInfo = await salesforceTenantLink.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a SalesforceTenantLink record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SalesforceTenantLink/{id}")]
		[Route("api/SalesforceTenantLink")]
		public async Task<IActionResult> DeleteSalesforceTenantLink(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
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

			IQueryable<Database.SalesforceTenantLink> query = (from x in _context.SalesforceTenantLinks
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.SalesforceTenantLink salesforceTenantLink = await query.FirstOrDefaultAsync(cancellationToken);

			if (salesforceTenantLink == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.SalesforceTenantLink DELETE", id.ToString(), new Exception("No Scheduler.SalesforceTenantLink entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SalesforceTenantLink cloneOfExisting = (Database.SalesforceTenantLink)_context.Entry(salesforceTenantLink).GetDatabaseValues().ToObject();


			lock (salesforceTenantLinkDeleteSyncRoot)
			{
			    try
			    {
			        salesforceTenantLink.deleted = true;
			        salesforceTenantLink.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        SalesforceTenantLinkChangeHistory salesforceTenantLinkChangeHistory = new SalesforceTenantLinkChangeHistory();
			        salesforceTenantLinkChangeHistory.salesforceTenantLinkId = salesforceTenantLink.id;
			        salesforceTenantLinkChangeHistory.versionNumber = salesforceTenantLink.versionNumber;
			        salesforceTenantLinkChangeHistory.timeStamp = DateTime.UtcNow;
			        salesforceTenantLinkChangeHistory.userId = securityUser.id;
			        salesforceTenantLinkChangeHistory.tenantGuid = userTenantGuid;
			        salesforceTenantLinkChangeHistory.data = JsonSerializer.Serialize(Database.SalesforceTenantLink.CreateAnonymousWithFirstLevelSubObjects(salesforceTenantLink));
			        _context.SalesforceTenantLinkChangeHistories.Add(salesforceTenantLinkChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.SalesforceTenantLink entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.SalesforceTenantLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.SalesforceTenantLink.CreateAnonymousWithFirstLevelSubObjects(salesforceTenantLink)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.SalesforceTenantLink entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.SalesforceTenantLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.SalesforceTenantLink.CreateAnonymousWithFirstLevelSubObjects(salesforceTenantLink)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


*/
        /// <summary>
        /// 
        /// This gets a list of SalesforceTenantLink records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SalesforceTenantLinks/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			bool? syncEnabled = null,
			string syncDirectionFlags = null,
			int? pullIntervalMinutes = null,
			DateTime? lastPullDate = null,
			string loginUrl = null,
			string sfClientId = null,
			string sfClientSecret = null,
			string sfUsername = null,
			string sfPassword = null,
			string sfSecurityToken = null,
			string instanceUrl = null,
			string apiVersion = null,
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
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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
			if (lastPullDate.HasValue == true && lastPullDate.Value.Kind != DateTimeKind.Utc)
			{
				lastPullDate = lastPullDate.Value.ToUniversalTime();
			}

			IQueryable<Database.SalesforceTenantLink> query = (from stl in _context.SalesforceTenantLinks select stl);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (syncEnabled.HasValue == true)
			{
				query = query.Where(stl => stl.syncEnabled == syncEnabled.Value);
			}
			if (string.IsNullOrEmpty(syncDirectionFlags) == false)
			{
				query = query.Where(stl => stl.syncDirectionFlags == syncDirectionFlags);
			}
			if (pullIntervalMinutes.HasValue == true)
			{
				query = query.Where(stl => stl.pullIntervalMinutes == pullIntervalMinutes.Value);
			}
			if (lastPullDate.HasValue == true)
			{
				query = query.Where(stl => stl.lastPullDate == lastPullDate.Value);
			}
			if (string.IsNullOrEmpty(loginUrl) == false)
			{
				query = query.Where(stl => stl.loginUrl == loginUrl);
			}
			if (string.IsNullOrEmpty(sfClientId) == false)
			{
				query = query.Where(stl => stl.sfClientId == sfClientId);
			}
			if (string.IsNullOrEmpty(sfClientSecret) == false)
			{
				query = query.Where(stl => stl.sfClientSecret == sfClientSecret);
			}
			if (string.IsNullOrEmpty(sfUsername) == false)
			{
				query = query.Where(stl => stl.sfUsername == sfUsername);
			}
			if (string.IsNullOrEmpty(sfPassword) == false)
			{
				query = query.Where(stl => stl.sfPassword == sfPassword);
			}
			if (string.IsNullOrEmpty(sfSecurityToken) == false)
			{
				query = query.Where(stl => stl.sfSecurityToken == sfSecurityToken);
			}
			if (string.IsNullOrEmpty(instanceUrl) == false)
			{
				query = query.Where(stl => stl.instanceUrl == instanceUrl);
			}
			if (string.IsNullOrEmpty(apiVersion) == false)
			{
				query = query.Where(stl => stl.apiVersion == apiVersion);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(stl => stl.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(stl => stl.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(stl => stl.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(stl => stl.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(stl => stl.deleted == false);
				}
			}
			else
			{
				query = query.Where(stl => stl.active == true);
				query = query.Where(stl => stl.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Salesforce Tenant Link, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.syncDirectionFlags.Contains(anyStringContains)
			       || x.loginUrl.Contains(anyStringContains)
			       || x.sfClientId.Contains(anyStringContains)
			       || x.sfClientSecret.Contains(anyStringContains)
			       || x.sfUsername.Contains(anyStringContains)
			       || x.sfPassword.Contains(anyStringContains)
			       || x.sfSecurityToken.Contains(anyStringContains)
			       || x.instanceUrl.Contains(anyStringContains)
			       || x.apiVersion.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.syncDirectionFlags).ThenBy(x => x.loginUrl).ThenBy(x => x.sfClientId);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.SalesforceTenantLink.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/SalesforceTenantLink/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
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
