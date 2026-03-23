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
using Foundation.DeepSpace.Database;
using Foundation.ChangeHistory;

namespace Foundation.DeepSpace.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the StorageObject entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the StorageObject entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class StorageObjectsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object storageObjectPutSyncRoot = new object();
		static object storageObjectDeleteSyncRoot = new object();

		private DeepSpaceContext _context;

		private ILogger<StorageObjectsController> _logger;

		public StorageObjectsController(DeepSpaceContext context, ILogger<StorageObjectsController> logger) : base("DeepSpace", "StorageObject")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of StorageObjects filtered by the parameters provided.
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
		[Route("api/StorageObjects")]
		public async Task<IActionResult> GetStorageObjects(
			string key = null,
			int? storageProviderId = null,
			int? storageTierId = null,
			int? sizeBytes = null,
			string contentType = null,
			string md5Hash = null,
			string sha256Hash = null,
			Guid? createdByUserGuid = null,
			DateTime? lastAccessedUtc = null,
			int? accessCount = null,
			bool? isDeleted = null,
			DateTime? deletedUtc = null,
			Guid? deletedByUserGuid = null,
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
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
			if (lastAccessedUtc.HasValue == true && lastAccessedUtc.Value.Kind != DateTimeKind.Utc)
			{
				lastAccessedUtc = lastAccessedUtc.Value.ToUniversalTime();
			}

			if (deletedUtc.HasValue == true && deletedUtc.Value.Kind != DateTimeKind.Utc)
			{
				deletedUtc = deletedUtc.Value.ToUniversalTime();
			}

			IQueryable<Database.StorageObject> query = (from so in _context.StorageObjects select so);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(key) == false)
			{
				query = query.Where(so => so.key == key);
			}
			if (storageProviderId.HasValue == true)
			{
				query = query.Where(so => so.storageProviderId == storageProviderId.Value);
			}
			if (storageTierId.HasValue == true)
			{
				query = query.Where(so => so.storageTierId == storageTierId.Value);
			}
			if (sizeBytes.HasValue == true)
			{
				query = query.Where(so => so.sizeBytes == sizeBytes.Value);
			}
			if (string.IsNullOrEmpty(contentType) == false)
			{
				query = query.Where(so => so.contentType == contentType);
			}
			if (string.IsNullOrEmpty(md5Hash) == false)
			{
				query = query.Where(so => so.md5Hash == md5Hash);
			}
			if (string.IsNullOrEmpty(sha256Hash) == false)
			{
				query = query.Where(so => so.sha256Hash == sha256Hash);
			}
			if (createdByUserGuid.HasValue == true)
			{
				query = query.Where(so => so.createdByUserGuid == createdByUserGuid);
			}
			if (lastAccessedUtc.HasValue == true)
			{
				query = query.Where(so => so.lastAccessedUtc == lastAccessedUtc.Value);
			}
			if (accessCount.HasValue == true)
			{
				query = query.Where(so => so.accessCount == accessCount.Value);
			}
			if (isDeleted.HasValue == true)
			{
				query = query.Where(so => so.isDeleted == isDeleted.Value);
			}
			if (deletedUtc.HasValue == true)
			{
				query = query.Where(so => so.deletedUtc == deletedUtc.Value);
			}
			if (deletedByUserGuid.HasValue == true)
			{
				query = query.Where(so => so.deletedByUserGuid == deletedByUserGuid);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(so => so.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(so => so.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(so => so.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(so => so.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(so => so.deleted == false);
				}
			}
			else
			{
				query = query.Where(so => so.active == true);
				query = query.Where(so => so.deleted == false);
			}

			query = query.OrderBy(so => so.key).ThenBy(so => so.contentType).ThenBy(so => so.md5Hash);


			//
			// Add the any string contains parameter to span all the string fields on the Storage Object, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.key.Contains(anyStringContains)
			       || x.contentType.Contains(anyStringContains)
			       || x.md5Hash.Contains(anyStringContains)
			       || x.sha256Hash.Contains(anyStringContains)
			       || (includeRelations == true && x.storageProvider.name.Contains(anyStringContains))
			       || (includeRelations == true && x.storageProvider.description.Contains(anyStringContains))
			       || (includeRelations == true && x.storageProvider.configJson.Contains(anyStringContains))
			       || (includeRelations == true && x.storageProvider.healthStatus.Contains(anyStringContains))
			       || (includeRelations == true && x.storageTier.name.Contains(anyStringContains))
			       || (includeRelations == true && x.storageTier.description.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.storageProvider);
				query = query.Include(x => x.storageTier);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.StorageObject> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.StorageObject storageObject in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(storageObject, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "DeepSpace.StorageObject Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "DeepSpace.StorageObject Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of StorageObjects filtered by the parameters provided.  Its query is similar to the GetStorageObjects method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/StorageObjects/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string key = null,
			int? storageProviderId = null,
			int? storageTierId = null,
			int? sizeBytes = null,
			string contentType = null,
			string md5Hash = null,
			string sha256Hash = null,
			Guid? createdByUserGuid = null,
			DateTime? lastAccessedUtc = null,
			int? accessCount = null,
			bool? isDeleted = null,
			DateTime? deletedUtc = null,
			Guid? deletedByUserGuid = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
			if (lastAccessedUtc.HasValue == true && lastAccessedUtc.Value.Kind != DateTimeKind.Utc)
			{
				lastAccessedUtc = lastAccessedUtc.Value.ToUniversalTime();
			}

			if (deletedUtc.HasValue == true && deletedUtc.Value.Kind != DateTimeKind.Utc)
			{
				deletedUtc = deletedUtc.Value.ToUniversalTime();
			}

			IQueryable<Database.StorageObject> query = (from so in _context.StorageObjects select so);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (key != null)
			{
				query = query.Where(so => so.key == key);
			}
			if (storageProviderId.HasValue == true)
			{
				query = query.Where(so => so.storageProviderId == storageProviderId.Value);
			}
			if (storageTierId.HasValue == true)
			{
				query = query.Where(so => so.storageTierId == storageTierId.Value);
			}
			if (sizeBytes.HasValue == true)
			{
				query = query.Where(so => so.sizeBytes == sizeBytes.Value);
			}
			if (contentType != null)
			{
				query = query.Where(so => so.contentType == contentType);
			}
			if (md5Hash != null)
			{
				query = query.Where(so => so.md5Hash == md5Hash);
			}
			if (sha256Hash != null)
			{
				query = query.Where(so => so.sha256Hash == sha256Hash);
			}
			if (createdByUserGuid.HasValue == true)
			{
				query = query.Where(so => so.createdByUserGuid == createdByUserGuid);
			}
			if (lastAccessedUtc.HasValue == true)
			{
				query = query.Where(so => so.lastAccessedUtc == lastAccessedUtc.Value);
			}
			if (accessCount.HasValue == true)
			{
				query = query.Where(so => so.accessCount == accessCount.Value);
			}
			if (isDeleted.HasValue == true)
			{
				query = query.Where(so => so.isDeleted == isDeleted.Value);
			}
			if (deletedUtc.HasValue == true)
			{
				query = query.Where(so => so.deletedUtc == deletedUtc.Value);
			}
			if (deletedByUserGuid.HasValue == true)
			{
				query = query.Where(so => so.deletedByUserGuid == deletedByUserGuid);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(so => so.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(so => so.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(so => so.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(so => so.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(so => so.deleted == false);
				}
			}
			else
			{
				query = query.Where(so => so.active == true);
				query = query.Where(so => so.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Storage Object, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.key.Contains(anyStringContains)
			       || x.contentType.Contains(anyStringContains)
			       || x.md5Hash.Contains(anyStringContains)
			       || x.sha256Hash.Contains(anyStringContains)
			       || x.storageProvider.name.Contains(anyStringContains)
			       || x.storageProvider.description.Contains(anyStringContains)
			       || x.storageProvider.configJson.Contains(anyStringContains)
			       || x.storageProvider.healthStatus.Contains(anyStringContains)
			       || x.storageTier.name.Contains(anyStringContains)
			       || x.storageTier.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single StorageObject by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/StorageObject/{id}")]
		public async Task<IActionResult> GetStorageObject(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
				IQueryable<Database.StorageObject> query = (from so in _context.StorageObjects where
							(so.id == id) &&
							(userIsAdmin == true || so.deleted == false) &&
							(userIsWriter == true || so.active == true)
					select so);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.storageProvider);
					query = query.Include(x => x.storageTier);
					query = query.AsSplitQuery();
				}

				Database.StorageObject materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "DeepSpace.StorageObject Entity was read with Admin privilege." : "DeepSpace.StorageObject Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "StorageObject", materialized.id, materialized.key));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a DeepSpace.StorageObject entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of DeepSpace.StorageObject.   Entity was read with Admin privilege." : "Exception caught during entity read of DeepSpace.StorageObject.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing StorageObject record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/StorageObject/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutStorageObject(int id, [FromBody]Database.StorageObject.StorageObjectDTO storageObjectDTO, CancellationToken cancellationToken = default)
		{
			if (storageObjectDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// DeepSpace Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != storageObjectDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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


			IQueryable<Database.StorageObject> query = (from x in _context.StorageObjects
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.StorageObject existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for DeepSpace.StorageObject PUT", id.ToString(), new Exception("No DeepSpace.StorageObject entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (storageObjectDTO.objectGuid == Guid.Empty)
            {
                storageObjectDTO.objectGuid = existing.objectGuid;
            }
            else if (storageObjectDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a StorageObject record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.StorageObject cloneOfExisting = (Database.StorageObject)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new StorageObject object using the data from the existing record, updated with what is in the DTO.
			//
			Database.StorageObject storageObject = (Database.StorageObject)_context.Entry(existing).GetDatabaseValues().ToObject();
			storageObject.ApplyDTO(storageObjectDTO);
			//
			// The tenant guid for any StorageObject being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the StorageObject because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				storageObject.tenantGuid = existing.tenantGuid;
			}

			lock (storageObjectPutSyncRoot)
			{
				//
				// Validate the version number for the storageObject being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != storageObject.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "StorageObject save attempt was made but save request was with version " + storageObject.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The StorageObject you are trying to update has already changed.  Please try your save again after reloading the StorageObject.");
				}
				else
				{
					// Same record.  Increase version.
					storageObject.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (storageObject.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted DeepSpace.StorageObject record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (storageObject.key != null && storageObject.key.Length > 1000)
				{
					storageObject.key = storageObject.key.Substring(0, 1000);
				}

				if (storageObject.contentType != null && storageObject.contentType.Length > 250)
				{
					storageObject.contentType = storageObject.contentType.Substring(0, 250);
				}

				if (storageObject.md5Hash != null && storageObject.md5Hash.Length > 100)
				{
					storageObject.md5Hash = storageObject.md5Hash.Substring(0, 100);
				}

				if (storageObject.sha256Hash != null && storageObject.sha256Hash.Length > 100)
				{
					storageObject.sha256Hash = storageObject.sha256Hash.Substring(0, 100);
				}

				if (storageObject.lastAccessedUtc.HasValue == true && storageObject.lastAccessedUtc.Value.Kind != DateTimeKind.Utc)
				{
					storageObject.lastAccessedUtc = storageObject.lastAccessedUtc.Value.ToUniversalTime();
				}

				if (storageObject.deletedUtc.HasValue == true && storageObject.deletedUtc.Value.Kind != DateTimeKind.Utc)
				{
					storageObject.deletedUtc = storageObject.deletedUtc.Value.ToUniversalTime();
				}

				try
				{
				    EntityEntry<Database.StorageObject> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(storageObject);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        StorageObjectChangeHistory storageObjectChangeHistory = new StorageObjectChangeHistory();
				        storageObjectChangeHistory.storageObjectId = storageObject.id;
				        storageObjectChangeHistory.versionNumber = storageObject.versionNumber;
				        storageObjectChangeHistory.timeStamp = DateTime.UtcNow;
				        storageObjectChangeHistory.userId = securityUser.id;
				        storageObjectChangeHistory.tenantGuid = userTenantGuid;
				        storageObjectChangeHistory.data = JsonSerializer.Serialize(Database.StorageObject.CreateAnonymousWithFirstLevelSubObjects(storageObject));
				        _context.StorageObjectChangeHistories.Add(storageObjectChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"DeepSpace.StorageObject entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.StorageObject.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.StorageObject.CreateAnonymousWithFirstLevelSubObjects(storageObject)),
						null);

				return Ok(Database.StorageObject.CreateAnonymous(storageObject));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"DeepSpace.StorageObject entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.StorageObject.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.StorageObject.CreateAnonymousWithFirstLevelSubObjects(storageObject)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new StorageObject record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/StorageObject", Name = "StorageObject")]
		public async Task<IActionResult> PostStorageObject([FromBody]Database.StorageObject.StorageObjectDTO storageObjectDTO, CancellationToken cancellationToken = default)
		{
			if (storageObjectDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// DeepSpace Writer role needed to write to this table, as well as the minimum write permission level.
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
			// Create a new StorageObject object using the data from the DTO
			//
			Database.StorageObject storageObject = Database.StorageObject.FromDTO(storageObjectDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				storageObject.tenantGuid = userTenantGuid;

				if (storageObject.key != null && storageObject.key.Length > 1000)
				{
					storageObject.key = storageObject.key.Substring(0, 1000);
				}

				if (storageObject.contentType != null && storageObject.contentType.Length > 250)
				{
					storageObject.contentType = storageObject.contentType.Substring(0, 250);
				}

				if (storageObject.md5Hash != null && storageObject.md5Hash.Length > 100)
				{
					storageObject.md5Hash = storageObject.md5Hash.Substring(0, 100);
				}

				if (storageObject.sha256Hash != null && storageObject.sha256Hash.Length > 100)
				{
					storageObject.sha256Hash = storageObject.sha256Hash.Substring(0, 100);
				}

				if (storageObject.lastAccessedUtc.HasValue == true && storageObject.lastAccessedUtc.Value.Kind != DateTimeKind.Utc)
				{
					storageObject.lastAccessedUtc = storageObject.lastAccessedUtc.Value.ToUniversalTime();
				}

				if (storageObject.deletedUtc.HasValue == true && storageObject.deletedUtc.Value.Kind != DateTimeKind.Utc)
				{
					storageObject.deletedUtc = storageObject.deletedUtc.Value.ToUniversalTime();
				}

				storageObject.objectGuid = Guid.NewGuid();
				storageObject.versionNumber = 1;

				_context.StorageObjects.Add(storageObject);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the storageObject object so that no further changes will be written to the database
				    //
				    _context.Entry(storageObject).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					storageObject.AccessLogs = null;
					storageObject.MigrationJobs = null;
					storageObject.StorageObjectChangeHistories = null;
					storageObject.StorageObjectVersions = null;
					storageObject.storageProvider = null;
					storageObject.storageTier = null;


				    StorageObjectChangeHistory storageObjectChangeHistory = new StorageObjectChangeHistory();
				    storageObjectChangeHistory.storageObjectId = storageObject.id;
				    storageObjectChangeHistory.versionNumber = storageObject.versionNumber;
				    storageObjectChangeHistory.timeStamp = DateTime.UtcNow;
				    storageObjectChangeHistory.userId = securityUser.id;
				    storageObjectChangeHistory.tenantGuid = userTenantGuid;
				    storageObjectChangeHistory.data = JsonSerializer.Serialize(Database.StorageObject.CreateAnonymousWithFirstLevelSubObjects(storageObject));
				    _context.StorageObjectChangeHistories.Add(storageObjectChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"DeepSpace.StorageObject entity successfully created.",
						true,
						storageObject. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.StorageObject.CreateAnonymousWithFirstLevelSubObjects(storageObject)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "DeepSpace.StorageObject entity creation failed.", false, storageObject.id.ToString(), "", JsonSerializer.Serialize(Database.StorageObject.CreateAnonymousWithFirstLevelSubObjects(storageObject)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "StorageObject", storageObject.id, storageObject.key));

			return CreatedAtRoute("StorageObject", new { id = storageObject.id }, Database.StorageObject.CreateAnonymousWithFirstLevelSubObjects(storageObject));
		}



        /// <summary>
        /// 
        /// This rolls a StorageObject entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/StorageObject/Rollback/{id}")]
		[Route("api/StorageObject/Rollback")]
		public async Task<IActionResult> RollbackToStorageObjectVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.StorageObject> query = (from x in _context.StorageObjects
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this StorageObject concurrently
			//
			lock (storageObjectPutSyncRoot)
			{
				
				Database.StorageObject storageObject = query.FirstOrDefault();
				
				if (storageObject == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for DeepSpace.StorageObject rollback", id.ToString(), new Exception("No DeepSpace.StorageObject entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the StorageObject current state so we can log it.
				//
				Database.StorageObject cloneOfExisting = (Database.StorageObject)_context.Entry(storageObject).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.AccessLogs = null;
				cloneOfExisting.MigrationJobs = null;
				cloneOfExisting.StorageObjectChangeHistories = null;
				cloneOfExisting.StorageObjectVersions = null;
				cloneOfExisting.storageProvider = null;
				cloneOfExisting.storageTier = null;

				if (versionNumber >= storageObject.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for DeepSpace.StorageObject rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for DeepSpace.StorageObject rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				StorageObjectChangeHistory storageObjectChangeHistory = (from x in _context.StorageObjectChangeHistories
				                                               where
				                                               x.storageObjectId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (storageObjectChangeHistory != null)
				{
				    Database.StorageObject oldStorageObject = JsonSerializer.Deserialize<Database.StorageObject>(storageObjectChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    storageObject.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    storageObject.key = oldStorageObject.key;
				    storageObject.storageProviderId = oldStorageObject.storageProviderId;
				    storageObject.storageTierId = oldStorageObject.storageTierId;
				    storageObject.sizeBytes = oldStorageObject.sizeBytes;
				    storageObject.contentType = oldStorageObject.contentType;
				    storageObject.md5Hash = oldStorageObject.md5Hash;
				    storageObject.sha256Hash = oldStorageObject.sha256Hash;
				    storageObject.createdByUserGuid = oldStorageObject.createdByUserGuid;
				    storageObject.lastAccessedUtc = oldStorageObject.lastAccessedUtc;
				    storageObject.accessCount = oldStorageObject.accessCount;
				    storageObject.isDeleted = oldStorageObject.isDeleted;
				    storageObject.deletedUtc = oldStorageObject.deletedUtc;
				    storageObject.deletedByUserGuid = oldStorageObject.deletedByUserGuid;
				    storageObject.objectGuid = oldStorageObject.objectGuid;
				    storageObject.active = oldStorageObject.active;
				    storageObject.deleted = oldStorageObject.deleted;

				    string serializedStorageObject = JsonSerializer.Serialize(storageObject);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        StorageObjectChangeHistory newStorageObjectChangeHistory = new StorageObjectChangeHistory();
				        newStorageObjectChangeHistory.storageObjectId = storageObject.id;
				        newStorageObjectChangeHistory.versionNumber = storageObject.versionNumber;
				        newStorageObjectChangeHistory.timeStamp = DateTime.UtcNow;
				        newStorageObjectChangeHistory.userId = securityUser.id;
				        newStorageObjectChangeHistory.tenantGuid = userTenantGuid;
				        newStorageObjectChangeHistory.data = JsonSerializer.Serialize(Database.StorageObject.CreateAnonymousWithFirstLevelSubObjects(storageObject));
				        _context.StorageObjectChangeHistories.Add(newStorageObjectChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"DeepSpace.StorageObject rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.StorageObject.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.StorageObject.CreateAnonymousWithFirstLevelSubObjects(storageObject)),
						null);


				    return Ok(Database.StorageObject.CreateAnonymous(storageObject));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for DeepSpace.StorageObject rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for DeepSpace.StorageObject rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a StorageObject.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the StorageObject</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/StorageObject/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetStorageObjectChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.StorageObject storageObject = await _context.StorageObjects.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (storageObject == null)
			{
				return NotFound();
			}

			try
			{
				storageObject.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.StorageObject> versionInfo = await storageObject.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a StorageObject.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the StorageObject</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/StorageObject/{id}/AuditHistory")]
		public async Task<IActionResult> GetStorageObjectAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.StorageObject storageObject = await _context.StorageObjects.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (storageObject == null)
			{
				return NotFound();
			}

			try
			{
				storageObject.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.StorageObject>> versions = await storageObject.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a StorageObject.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the StorageObject</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The StorageObject object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/StorageObject/{id}/Version/{version}")]
		public async Task<IActionResult> GetStorageObjectVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.StorageObject storageObject = await _context.StorageObjects.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (storageObject == null)
			{
				return NotFound();
			}

			try
			{
				storageObject.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.StorageObject> versionInfo = await storageObject.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a StorageObject at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the StorageObject</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The StorageObject object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/StorageObject/{id}/StateAtTime")]
		public async Task<IActionResult> GetStorageObjectStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.StorageObject storageObject = await _context.StorageObjects.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (storageObject == null)
			{
				return NotFound();
			}

			try
			{
				storageObject.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.StorageObject> versionInfo = await storageObject.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a StorageObject record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/StorageObject/{id}")]
		[Route("api/StorageObject")]
		public async Task<IActionResult> DeleteStorageObject(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// DeepSpace Writer role needed to write to this table, as well as the minimum write permission level.
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

			IQueryable<Database.StorageObject> query = (from x in _context.StorageObjects
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.StorageObject storageObject = await query.FirstOrDefaultAsync(cancellationToken);

			if (storageObject == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for DeepSpace.StorageObject DELETE", id.ToString(), new Exception("No DeepSpace.StorageObject entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.StorageObject cloneOfExisting = (Database.StorageObject)_context.Entry(storageObject).GetDatabaseValues().ToObject();


			lock (storageObjectDeleteSyncRoot)
			{
			    try
			    {
			        storageObject.deleted = true;
			        storageObject.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        StorageObjectChangeHistory storageObjectChangeHistory = new StorageObjectChangeHistory();
			        storageObjectChangeHistory.storageObjectId = storageObject.id;
			        storageObjectChangeHistory.versionNumber = storageObject.versionNumber;
			        storageObjectChangeHistory.timeStamp = DateTime.UtcNow;
			        storageObjectChangeHistory.userId = securityUser.id;
			        storageObjectChangeHistory.tenantGuid = userTenantGuid;
			        storageObjectChangeHistory.data = JsonSerializer.Serialize(Database.StorageObject.CreateAnonymousWithFirstLevelSubObjects(storageObject));
			        _context.StorageObjectChangeHistories.Add(storageObjectChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"DeepSpace.StorageObject entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.StorageObject.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.StorageObject.CreateAnonymousWithFirstLevelSubObjects(storageObject)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"DeepSpace.StorageObject entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.StorageObject.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.StorageObject.CreateAnonymousWithFirstLevelSubObjects(storageObject)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of StorageObject records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/StorageObjects/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string key = null,
			int? storageProviderId = null,
			int? storageTierId = null,
			int? sizeBytes = null,
			string contentType = null,
			string md5Hash = null,
			string sha256Hash = null,
			Guid? createdByUserGuid = null,
			DateTime? lastAccessedUtc = null,
			int? accessCount = null,
			bool? isDeleted = null,
			DateTime? deletedUtc = null,
			Guid? deletedByUserGuid = null,
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
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);


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
			if (lastAccessedUtc.HasValue == true && lastAccessedUtc.Value.Kind != DateTimeKind.Utc)
			{
				lastAccessedUtc = lastAccessedUtc.Value.ToUniversalTime();
			}

			if (deletedUtc.HasValue == true && deletedUtc.Value.Kind != DateTimeKind.Utc)
			{
				deletedUtc = deletedUtc.Value.ToUniversalTime();
			}

			IQueryable<Database.StorageObject> query = (from so in _context.StorageObjects select so);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(key) == false)
			{
				query = query.Where(so => so.key == key);
			}
			if (storageProviderId.HasValue == true)
			{
				query = query.Where(so => so.storageProviderId == storageProviderId.Value);
			}
			if (storageTierId.HasValue == true)
			{
				query = query.Where(so => so.storageTierId == storageTierId.Value);
			}
			if (sizeBytes.HasValue == true)
			{
				query = query.Where(so => so.sizeBytes == sizeBytes.Value);
			}
			if (string.IsNullOrEmpty(contentType) == false)
			{
				query = query.Where(so => so.contentType == contentType);
			}
			if (string.IsNullOrEmpty(md5Hash) == false)
			{
				query = query.Where(so => so.md5Hash == md5Hash);
			}
			if (string.IsNullOrEmpty(sha256Hash) == false)
			{
				query = query.Where(so => so.sha256Hash == sha256Hash);
			}
			if (createdByUserGuid.HasValue == true)
			{
				query = query.Where(so => so.createdByUserGuid == createdByUserGuid);
			}
			if (lastAccessedUtc.HasValue == true)
			{
				query = query.Where(so => so.lastAccessedUtc == lastAccessedUtc.Value);
			}
			if (accessCount.HasValue == true)
			{
				query = query.Where(so => so.accessCount == accessCount.Value);
			}
			if (isDeleted.HasValue == true)
			{
				query = query.Where(so => so.isDeleted == isDeleted.Value);
			}
			if (deletedUtc.HasValue == true)
			{
				query = query.Where(so => so.deletedUtc == deletedUtc.Value);
			}
			if (deletedByUserGuid.HasValue == true)
			{
				query = query.Where(so => so.deletedByUserGuid == deletedByUserGuid);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(so => so.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(so => so.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(so => so.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(so => so.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(so => so.deleted == false);
				}
			}
			else
			{
				query = query.Where(so => so.active == true);
				query = query.Where(so => so.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Storage Object, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.key.Contains(anyStringContains)
			       || x.contentType.Contains(anyStringContains)
			       || x.md5Hash.Contains(anyStringContains)
			       || x.sha256Hash.Contains(anyStringContains)
			       || x.storageProvider.name.Contains(anyStringContains)
			       || x.storageProvider.description.Contains(anyStringContains)
			       || x.storageProvider.configJson.Contains(anyStringContains)
			       || x.storageProvider.healthStatus.Contains(anyStringContains)
			       || x.storageTier.name.Contains(anyStringContains)
			       || x.storageTier.description.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.key).ThenBy(x => x.contentType).ThenBy(x => x.md5Hash);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.StorageObject.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/StorageObject/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// DeepSpace Writer role needed to write to this table, as well as the minimum write permission level.
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
