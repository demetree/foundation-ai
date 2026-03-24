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

namespace Foundation.DeepSpace.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the StorageObjectVersion entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the StorageObjectVersion entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class StorageObjectVersionsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object storageObjectVersionPutSyncRoot = new object();
		static object storageObjectVersionDeleteSyncRoot = new object();

		private DeepSpaceContext _context;

		private ILogger<StorageObjectVersionsController> _logger;

		public StorageObjectVersionsController(DeepSpaceContext context, ILogger<StorageObjectVersionsController> logger) : base("DeepSpace", "StorageObjectVersion")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of StorageObjectVersions filtered by the parameters provided.
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
		[Route("api/StorageObjectVersions")]
		public async Task<IActionResult> GetStorageObjectVersions(
			int? storageObjectId = null,
			int? versionNumber = null,
			int? storageProviderId = null,
			string providerKey = null,
			int? sizeBytes = null,
			string md5Hash = null,
			Guid? createdByUserGuid = null,
			DateTime? createdUtc = null,
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
			if (createdUtc.HasValue == true && createdUtc.Value.Kind != DateTimeKind.Utc)
			{
				createdUtc = createdUtc.Value.ToUniversalTime();
			}

			IQueryable<Database.StorageObjectVersion> query = (from sov in _context.StorageObjectVersions select sov);
			if (storageObjectId.HasValue == true)
			{
				query = query.Where(sov => sov.storageObjectId == storageObjectId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(sov => sov.versionNumber == versionNumber.Value);
			}
			if (storageProviderId.HasValue == true)
			{
				query = query.Where(sov => sov.storageProviderId == storageProviderId.Value);
			}
			if (string.IsNullOrEmpty(providerKey) == false)
			{
				query = query.Where(sov => sov.providerKey == providerKey);
			}
			if (sizeBytes.HasValue == true)
			{
				query = query.Where(sov => sov.sizeBytes == sizeBytes.Value);
			}
			if (string.IsNullOrEmpty(md5Hash) == false)
			{
				query = query.Where(sov => sov.md5Hash == md5Hash);
			}
			if (createdByUserGuid.HasValue == true)
			{
				query = query.Where(sov => sov.createdByUserGuid == createdByUserGuid);
			}
			if (createdUtc.HasValue == true)
			{
				query = query.Where(sov => sov.createdUtc == createdUtc.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sov => sov.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sov => sov.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sov => sov.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sov => sov.deleted == false);
				}
			}
			else
			{
				query = query.Where(sov => sov.active == true);
				query = query.Where(sov => sov.deleted == false);
			}

			query = query.OrderBy(sov => sov.providerKey).ThenBy(sov => sov.md5Hash);


			//
			// Add the any string contains parameter to span all the string fields on the Storage Object Version, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.providerKey.Contains(anyStringContains)
			       || x.md5Hash.Contains(anyStringContains)
			       || (includeRelations == true && x.storageObject.key.Contains(anyStringContains))
			       || (includeRelations == true && x.storageObject.contentType.Contains(anyStringContains))
			       || (includeRelations == true && x.storageObject.md5Hash.Contains(anyStringContains))
			       || (includeRelations == true && x.storageObject.sha256Hash.Contains(anyStringContains))
			       || (includeRelations == true && x.storageProvider.name.Contains(anyStringContains))
			       || (includeRelations == true && x.storageProvider.description.Contains(anyStringContains))
			       || (includeRelations == true && x.storageProvider.configJson.Contains(anyStringContains))
			       || (includeRelations == true && x.storageProvider.healthStatus.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.storageObject);
				query = query.Include(x => x.storageProvider);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.StorageObjectVersion> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.StorageObjectVersion storageObjectVersion in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(storageObjectVersion, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "DeepSpace.StorageObjectVersion Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "DeepSpace.StorageObjectVersion Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of StorageObjectVersions filtered by the parameters provided.  Its query is similar to the GetStorageObjectVersions method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/StorageObjectVersions/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? storageObjectId = null,
			int? versionNumber = null,
			int? storageProviderId = null,
			string providerKey = null,
			int? sizeBytes = null,
			string md5Hash = null,
			Guid? createdByUserGuid = null,
			DateTime? createdUtc = null,
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

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (createdUtc.HasValue == true && createdUtc.Value.Kind != DateTimeKind.Utc)
			{
				createdUtc = createdUtc.Value.ToUniversalTime();
			}

			IQueryable<Database.StorageObjectVersion> query = (from sov in _context.StorageObjectVersions select sov);
			if (storageObjectId.HasValue == true)
			{
				query = query.Where(sov => sov.storageObjectId == storageObjectId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(sov => sov.versionNumber == versionNumber.Value);
			}
			if (storageProviderId.HasValue == true)
			{
				query = query.Where(sov => sov.storageProviderId == storageProviderId.Value);
			}
			if (providerKey != null)
			{
				query = query.Where(sov => sov.providerKey == providerKey);
			}
			if (sizeBytes.HasValue == true)
			{
				query = query.Where(sov => sov.sizeBytes == sizeBytes.Value);
			}
			if (md5Hash != null)
			{
				query = query.Where(sov => sov.md5Hash == md5Hash);
			}
			if (createdByUserGuid.HasValue == true)
			{
				query = query.Where(sov => sov.createdByUserGuid == createdByUserGuid);
			}
			if (createdUtc.HasValue == true)
			{
				query = query.Where(sov => sov.createdUtc == createdUtc.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sov => sov.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sov => sov.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sov => sov.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sov => sov.deleted == false);
				}
			}
			else
			{
				query = query.Where(sov => sov.active == true);
				query = query.Where(sov => sov.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Storage Object Version, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.providerKey.Contains(anyStringContains)
			       || x.md5Hash.Contains(anyStringContains)
			       || x.storageObject.key.Contains(anyStringContains)
			       || x.storageObject.contentType.Contains(anyStringContains)
			       || x.storageObject.md5Hash.Contains(anyStringContains)
			       || x.storageObject.sha256Hash.Contains(anyStringContains)
			       || x.storageProvider.name.Contains(anyStringContains)
			       || x.storageProvider.description.Contains(anyStringContains)
			       || x.storageProvider.configJson.Contains(anyStringContains)
			       || x.storageProvider.healthStatus.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single StorageObjectVersion by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/StorageObjectVersion/{id}")]
		public async Task<IActionResult> GetStorageObjectVersion(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			try
			{
				IQueryable<Database.StorageObjectVersion> query = (from sov in _context.StorageObjectVersions where
							(sov.id == id) &&
							(userIsAdmin == true || sov.deleted == false) &&
							(userIsWriter == true || sov.active == true)
					select sov);

				if (includeRelations == true)
				{
					query = query.Include(x => x.storageObject);
					query = query.Include(x => x.storageProvider);
					query = query.AsSplitQuery();
				}

				Database.StorageObjectVersion materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "DeepSpace.StorageObjectVersion Entity was read with Admin privilege." : "DeepSpace.StorageObjectVersion Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "StorageObjectVersion", materialized.id, materialized.providerKey));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a DeepSpace.StorageObjectVersion entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of DeepSpace.StorageObjectVersion.   Entity was read with Admin privilege." : "Exception caught during entity read of DeepSpace.StorageObjectVersion.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing StorageObjectVersion record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/StorageObjectVersion/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutStorageObjectVersion(int id, [FromBody]Database.StorageObjectVersion.StorageObjectVersionDTO storageObjectVersionDTO, CancellationToken cancellationToken = default)
		{
			if (storageObjectVersionDTO == null)
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



			if (id != storageObjectVersionDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.StorageObjectVersion> query = (from x in _context.StorageObjectVersions
				where
				(x.id == id)
				select x);


			Database.StorageObjectVersion existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for DeepSpace.StorageObjectVersion PUT", id.ToString(), new Exception("No DeepSpace.StorageObjectVersion entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (storageObjectVersionDTO.objectGuid == Guid.Empty)
            {
                storageObjectVersionDTO.objectGuid = existing.objectGuid;
            }
            else if (storageObjectVersionDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a StorageObjectVersion record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.StorageObjectVersion cloneOfExisting = (Database.StorageObjectVersion)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new StorageObjectVersion object using the data from the existing record, updated with what is in the DTO.
			//
			Database.StorageObjectVersion storageObjectVersion = (Database.StorageObjectVersion)_context.Entry(existing).GetDatabaseValues().ToObject();
			storageObjectVersion.ApplyDTO(storageObjectVersionDTO);
			lock (storageObjectVersionPutSyncRoot)
			{
				//
				// Validate the version number for the storageObjectVersion being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != storageObjectVersion.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "StorageObjectVersion save attempt was made but save request was with version " + storageObjectVersion.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The StorageObjectVersion you are trying to update has already changed.  Please try your save again after reloading the StorageObjectVersion.");
				}
				else
				{
					// Same record.  Increase version.
					storageObjectVersion.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (storageObjectVersion.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted DeepSpace.StorageObjectVersion record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (storageObjectVersion.providerKey != null && storageObjectVersion.providerKey.Length > 1000)
				{
					storageObjectVersion.providerKey = storageObjectVersion.providerKey.Substring(0, 1000);
				}

				if (storageObjectVersion.md5Hash != null && storageObjectVersion.md5Hash.Length > 100)
				{
					storageObjectVersion.md5Hash = storageObjectVersion.md5Hash.Substring(0, 100);
				}

				if (storageObjectVersion.createdUtc.Kind != DateTimeKind.Utc)
				{
					storageObjectVersion.createdUtc = storageObjectVersion.createdUtc.ToUniversalTime();
				}

				try
				{
				    EntityEntry<Database.StorageObjectVersion> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(storageObjectVersion);

				    _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"DeepSpace.StorageObjectVersion entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.StorageObjectVersion.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.StorageObjectVersion.CreateAnonymousWithFirstLevelSubObjects(storageObjectVersion)),
						null);

				return Ok(Database.StorageObjectVersion.CreateAnonymous(storageObjectVersion));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"DeepSpace.StorageObjectVersion entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.StorageObjectVersion.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.StorageObjectVersion.CreateAnonymousWithFirstLevelSubObjects(storageObjectVersion)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new StorageObjectVersion record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/StorageObjectVersion", Name = "StorageObjectVersion")]
		public async Task<IActionResult> PostStorageObjectVersion([FromBody]Database.StorageObjectVersion.StorageObjectVersionDTO storageObjectVersionDTO, CancellationToken cancellationToken = default)
		{
			if (storageObjectVersionDTO == null)
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

			//
			// Create a new StorageObjectVersion object using the data from the DTO
			//
			Database.StorageObjectVersion storageObjectVersion = Database.StorageObjectVersion.FromDTO(storageObjectVersionDTO);

			try
			{
				if (storageObjectVersion.providerKey != null && storageObjectVersion.providerKey.Length > 1000)
				{
					storageObjectVersion.providerKey = storageObjectVersion.providerKey.Substring(0, 1000);
				}

				if (storageObjectVersion.md5Hash != null && storageObjectVersion.md5Hash.Length > 100)
				{
					storageObjectVersion.md5Hash = storageObjectVersion.md5Hash.Substring(0, 100);
				}

				if (storageObjectVersion.createdUtc.Kind != DateTimeKind.Utc)
				{
					storageObjectVersion.createdUtc = storageObjectVersion.createdUtc.ToUniversalTime();
				}

				storageObjectVersion.objectGuid = Guid.NewGuid();
				storageObjectVersion.versionNumber = 1;

				_context.StorageObjectVersions.Add(storageObjectVersion);
				await _context.SaveChangesAsync(cancellationToken);

				//
				// Detach the storageObjectVersion object so that no further changes will be written to the database
				//
				_context.Entry(storageObjectVersion).State = EntityState.Detached;

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"DeepSpace.StorageObjectVersion entity successfully created.",
					true,
					storageObjectVersion.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.StorageObjectVersion.CreateAnonymousWithFirstLevelSubObjects(storageObjectVersion)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "DeepSpace.StorageObjectVersion entity creation failed.", false, storageObjectVersion.id.ToString(), "", JsonSerializer.Serialize(Database.StorageObjectVersion.CreateAnonymousWithFirstLevelSubObjects(storageObjectVersion)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "StorageObjectVersion", storageObjectVersion.id, storageObjectVersion.providerKey));

			return CreatedAtRoute("StorageObjectVersion", new { id = storageObjectVersion.id }, Database.StorageObjectVersion.CreateAnonymousWithFirstLevelSubObjects(storageObjectVersion));
		}





        /// <summary>
        /// 
        /// This deletes a StorageObjectVersion record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/StorageObjectVersion/{id}")]
		[Route("api/StorageObjectVersion")]
		public async Task<IActionResult> DeleteStorageObjectVersion(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.StorageObjectVersion> query = (from x in _context.StorageObjectVersions
				where
				(x.id == id)
				select x);


			Database.StorageObjectVersion storageObjectVersion = await query.FirstOrDefaultAsync(cancellationToken);

			if (storageObjectVersion == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for DeepSpace.StorageObjectVersion DELETE", id.ToString(), new Exception("No DeepSpace.StorageObjectVersion entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.StorageObjectVersion cloneOfExisting = (Database.StorageObjectVersion)_context.Entry(storageObjectVersion).GetDatabaseValues().ToObject();


			lock (storageObjectVersionDeleteSyncRoot)
			{
			    try
			    {
			        storageObjectVersion.deleted = true;
			        storageObjectVersion.versionNumber++;

			        _context.SaveChanges();


					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"DeepSpace.StorageObjectVersion entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.StorageObjectVersion.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.StorageObjectVersion.CreateAnonymousWithFirstLevelSubObjects(storageObjectVersion)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"DeepSpace.StorageObjectVersion entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.StorageObjectVersion.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.StorageObjectVersion.CreateAnonymousWithFirstLevelSubObjects(storageObjectVersion)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of StorageObjectVersion records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/StorageObjectVersions/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? storageObjectId = null,
			int? versionNumber = null,
			int? storageProviderId = null,
			string providerKey = null,
			int? sizeBytes = null,
			string md5Hash = null,
			Guid? createdByUserGuid = null,
			DateTime? createdUtc = null,
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
			if (createdUtc.HasValue == true && createdUtc.Value.Kind != DateTimeKind.Utc)
			{
				createdUtc = createdUtc.Value.ToUniversalTime();
			}

			IQueryable<Database.StorageObjectVersion> query = (from sov in _context.StorageObjectVersions select sov);
			if (storageObjectId.HasValue == true)
			{
				query = query.Where(sov => sov.storageObjectId == storageObjectId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(sov => sov.versionNumber == versionNumber.Value);
			}
			if (storageProviderId.HasValue == true)
			{
				query = query.Where(sov => sov.storageProviderId == storageProviderId.Value);
			}
			if (string.IsNullOrEmpty(providerKey) == false)
			{
				query = query.Where(sov => sov.providerKey == providerKey);
			}
			if (sizeBytes.HasValue == true)
			{
				query = query.Where(sov => sov.sizeBytes == sizeBytes.Value);
			}
			if (string.IsNullOrEmpty(md5Hash) == false)
			{
				query = query.Where(sov => sov.md5Hash == md5Hash);
			}
			if (createdByUserGuid.HasValue == true)
			{
				query = query.Where(sov => sov.createdByUserGuid == createdByUserGuid);
			}
			if (createdUtc.HasValue == true)
			{
				query = query.Where(sov => sov.createdUtc == createdUtc.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sov => sov.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sov => sov.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sov => sov.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sov => sov.deleted == false);
				}
			}
			else
			{
				query = query.Where(sov => sov.active == true);
				query = query.Where(sov => sov.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Storage Object Version, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.providerKey.Contains(anyStringContains)
			       || x.md5Hash.Contains(anyStringContains)
			       || x.storageObject.key.Contains(anyStringContains)
			       || x.storageObject.contentType.Contains(anyStringContains)
			       || x.storageObject.md5Hash.Contains(anyStringContains)
			       || x.storageObject.sha256Hash.Contains(anyStringContains)
			       || x.storageProvider.name.Contains(anyStringContains)
			       || x.storageProvider.description.Contains(anyStringContains)
			       || x.storageProvider.configJson.Contains(anyStringContains)
			       || x.storageProvider.healthStatus.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.providerKey).ThenBy(x => x.md5Hash);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.StorageObjectVersion.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/StorageObjectVersion/CreateAuditEvent")]
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
