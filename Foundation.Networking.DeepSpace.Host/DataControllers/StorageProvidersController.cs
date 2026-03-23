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
    /// This auto generated class provides the basic CRUD operations for the StorageProvider entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the StorageProvider entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class StorageProvidersController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private DeepSpaceContext _context;

		private ILogger<StorageProvidersController> _logger;

		public StorageProvidersController(DeepSpaceContext context, ILogger<StorageProvidersController> logger) : base("DeepSpace", "StorageProvider")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of StorageProviders filtered by the parameters provided.
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
		[Route("api/StorageProviders")]
		public async Task<IActionResult> GetStorageProviders(
			string name = null,
			string description = null,
			int? storageProviderTypeId = null,
			string configJson = null,
			bool? isDefault = null,
			bool? isEnabled = null,
			string healthStatus = null,
			DateTime? lastHealthCheckUtc = null,
			int? totalCapacityBytes = null,
			int? usedCapacityBytes = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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
			if (lastHealthCheckUtc.HasValue == true && lastHealthCheckUtc.Value.Kind != DateTimeKind.Utc)
			{
				lastHealthCheckUtc = lastHealthCheckUtc.Value.ToUniversalTime();
			}

			IQueryable<Database.StorageProvider> query = (from sp in _context.StorageProviders select sp);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(sp => sp.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(sp => sp.description == description);
			}
			if (storageProviderTypeId.HasValue == true)
			{
				query = query.Where(sp => sp.storageProviderTypeId == storageProviderTypeId.Value);
			}
			if (string.IsNullOrEmpty(configJson) == false)
			{
				query = query.Where(sp => sp.configJson == configJson);
			}
			if (isDefault.HasValue == true)
			{
				query = query.Where(sp => sp.isDefault == isDefault.Value);
			}
			if (isEnabled.HasValue == true)
			{
				query = query.Where(sp => sp.isEnabled == isEnabled.Value);
			}
			if (string.IsNullOrEmpty(healthStatus) == false)
			{
				query = query.Where(sp => sp.healthStatus == healthStatus);
			}
			if (lastHealthCheckUtc.HasValue == true)
			{
				query = query.Where(sp => sp.lastHealthCheckUtc == lastHealthCheckUtc.Value);
			}
			if (totalCapacityBytes.HasValue == true)
			{
				query = query.Where(sp => sp.totalCapacityBytes == totalCapacityBytes.Value);
			}
			if (usedCapacityBytes.HasValue == true)
			{
				query = query.Where(sp => sp.usedCapacityBytes == usedCapacityBytes.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sp => sp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sp => sp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sp => sp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sp => sp.deleted == false);
				}
			}
			else
			{
				query = query.Where(sp => sp.active == true);
				query = query.Where(sp => sp.deleted == false);
			}

			query = query.OrderBy(sp => sp.name).ThenBy(sp => sp.description).ThenBy(sp => sp.healthStatus);


			//
			// Add the any string contains parameter to span all the string fields on the Storage Provider, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.configJson.Contains(anyStringContains)
			       || x.healthStatus.Contains(anyStringContains)
			       || (includeRelations == true && x.storageProviderType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.storageProviderType.description.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.storageProviderType);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.StorageProvider> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.StorageProvider storageProvider in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(storageProvider, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "DeepSpace.StorageProvider Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "DeepSpace.StorageProvider Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of StorageProviders filtered by the parameters provided.  Its query is similar to the GetStorageProviders method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/StorageProviders/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			int? storageProviderTypeId = null,
			string configJson = null,
			bool? isDefault = null,
			bool? isEnabled = null,
			string healthStatus = null,
			DateTime? lastHealthCheckUtc = null,
			int? totalCapacityBytes = null,
			int? usedCapacityBytes = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (lastHealthCheckUtc.HasValue == true && lastHealthCheckUtc.Value.Kind != DateTimeKind.Utc)
			{
				lastHealthCheckUtc = lastHealthCheckUtc.Value.ToUniversalTime();
			}

			IQueryable<Database.StorageProvider> query = (from sp in _context.StorageProviders select sp);
			if (name != null)
			{
				query = query.Where(sp => sp.name == name);
			}
			if (description != null)
			{
				query = query.Where(sp => sp.description == description);
			}
			if (storageProviderTypeId.HasValue == true)
			{
				query = query.Where(sp => sp.storageProviderTypeId == storageProviderTypeId.Value);
			}
			if (configJson != null)
			{
				query = query.Where(sp => sp.configJson == configJson);
			}
			if (isDefault.HasValue == true)
			{
				query = query.Where(sp => sp.isDefault == isDefault.Value);
			}
			if (isEnabled.HasValue == true)
			{
				query = query.Where(sp => sp.isEnabled == isEnabled.Value);
			}
			if (healthStatus != null)
			{
				query = query.Where(sp => sp.healthStatus == healthStatus);
			}
			if (lastHealthCheckUtc.HasValue == true)
			{
				query = query.Where(sp => sp.lastHealthCheckUtc == lastHealthCheckUtc.Value);
			}
			if (totalCapacityBytes.HasValue == true)
			{
				query = query.Where(sp => sp.totalCapacityBytes == totalCapacityBytes.Value);
			}
			if (usedCapacityBytes.HasValue == true)
			{
				query = query.Where(sp => sp.usedCapacityBytes == usedCapacityBytes.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sp => sp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sp => sp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sp => sp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sp => sp.deleted == false);
				}
			}
			else
			{
				query = query.Where(sp => sp.active == true);
				query = query.Where(sp => sp.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Storage Provider, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.configJson.Contains(anyStringContains)
			       || x.healthStatus.Contains(anyStringContains)
			       || x.storageProviderType.name.Contains(anyStringContains)
			       || x.storageProviderType.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single StorageProvider by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/StorageProvider/{id}")]
		public async Task<IActionResult> GetStorageProvider(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.StorageProvider> query = (from sp in _context.StorageProviders where
							(sp.id == id) &&
							(userIsAdmin == true || sp.deleted == false) &&
							(userIsWriter == true || sp.active == true)
					select sp);

				if (includeRelations == true)
				{
					query = query.Include(x => x.storageProviderType);
					query = query.AsSplitQuery();
				}

				Database.StorageProvider materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "DeepSpace.StorageProvider Entity was read with Admin privilege." : "DeepSpace.StorageProvider Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "StorageProvider", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a DeepSpace.StorageProvider entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of DeepSpace.StorageProvider.   Entity was read with Admin privilege." : "Exception caught during entity read of DeepSpace.StorageProvider.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing StorageProvider record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/StorageProvider/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutStorageProvider(int id, [FromBody]Database.StorageProvider.StorageProviderDTO storageProviderDTO, CancellationToken cancellationToken = default)
		{
			if (storageProviderDTO == null)
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



			if (id != storageProviderDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.StorageProvider> query = (from x in _context.StorageProviders
				where
				(x.id == id)
				select x);


			Database.StorageProvider existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for DeepSpace.StorageProvider PUT", id.ToString(), new Exception("No DeepSpace.StorageProvider entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (storageProviderDTO.objectGuid == Guid.Empty)
            {
                storageProviderDTO.objectGuid = existing.objectGuid;
            }
            else if (storageProviderDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a StorageProvider record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.StorageProvider cloneOfExisting = (Database.StorageProvider)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new StorageProvider object using the data from the existing record, updated with what is in the DTO.
			//
			Database.StorageProvider storageProvider = (Database.StorageProvider)_context.Entry(existing).GetDatabaseValues().ToObject();
			storageProvider.ApplyDTO(storageProviderDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (storageProvider.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted DeepSpace.StorageProvider record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (storageProvider.name != null && storageProvider.name.Length > 100)
			{
				storageProvider.name = storageProvider.name.Substring(0, 100);
			}

			if (storageProvider.description != null && storageProvider.description.Length > 500)
			{
				storageProvider.description = storageProvider.description.Substring(0, 500);
			}

			if (storageProvider.healthStatus != null && storageProvider.healthStatus.Length > 50)
			{
				storageProvider.healthStatus = storageProvider.healthStatus.Substring(0, 50);
			}

			if (storageProvider.lastHealthCheckUtc.HasValue == true && storageProvider.lastHealthCheckUtc.Value.Kind != DateTimeKind.Utc)
			{
				storageProvider.lastHealthCheckUtc = storageProvider.lastHealthCheckUtc.Value.ToUniversalTime();
			}

			EntityEntry<Database.StorageProvider> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(storageProvider);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"DeepSpace.StorageProvider entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.StorageProvider.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.StorageProvider.CreateAnonymousWithFirstLevelSubObjects(storageProvider)),
					null);


				return Ok(Database.StorageProvider.CreateAnonymous(storageProvider));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"DeepSpace.StorageProvider entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.StorageProvider.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.StorageProvider.CreateAnonymousWithFirstLevelSubObjects(storageProvider)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new StorageProvider record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/StorageProvider", Name = "StorageProvider")]
		public async Task<IActionResult> PostStorageProvider([FromBody]Database.StorageProvider.StorageProviderDTO storageProviderDTO, CancellationToken cancellationToken = default)
		{
			if (storageProviderDTO == null)
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
			// Create a new StorageProvider object using the data from the DTO
			//
			Database.StorageProvider storageProvider = Database.StorageProvider.FromDTO(storageProviderDTO);

			try
			{
				if (storageProvider.name != null && storageProvider.name.Length > 100)
				{
					storageProvider.name = storageProvider.name.Substring(0, 100);
				}

				if (storageProvider.description != null && storageProvider.description.Length > 500)
				{
					storageProvider.description = storageProvider.description.Substring(0, 500);
				}

				if (storageProvider.healthStatus != null && storageProvider.healthStatus.Length > 50)
				{
					storageProvider.healthStatus = storageProvider.healthStatus.Substring(0, 50);
				}

				if (storageProvider.lastHealthCheckUtc.HasValue == true && storageProvider.lastHealthCheckUtc.Value.Kind != DateTimeKind.Utc)
				{
					storageProvider.lastHealthCheckUtc = storageProvider.lastHealthCheckUtc.Value.ToUniversalTime();
				}

				storageProvider.objectGuid = Guid.NewGuid();
				_context.StorageProviders.Add(storageProvider);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"DeepSpace.StorageProvider entity successfully created.",
					true,
					storageProvider.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.StorageProvider.CreateAnonymousWithFirstLevelSubObjects(storageProvider)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "DeepSpace.StorageProvider entity creation failed.", false, storageProvider.id.ToString(), "", JsonSerializer.Serialize(storageProvider), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "StorageProvider", storageProvider.id, storageProvider.name));

			return CreatedAtRoute("StorageProvider", new { id = storageProvider.id }, Database.StorageProvider.CreateAnonymousWithFirstLevelSubObjects(storageProvider));
		}



        /// <summary>
        /// 
        /// This deletes a StorageProvider record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/StorageProvider/{id}")]
		[Route("api/StorageProvider")]
		public async Task<IActionResult> DeleteStorageProvider(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.StorageProvider> query = (from x in _context.StorageProviders
				where
				(x.id == id)
				select x);


			Database.StorageProvider storageProvider = await query.FirstOrDefaultAsync(cancellationToken);

			if (storageProvider == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for DeepSpace.StorageProvider DELETE", id.ToString(), new Exception("No DeepSpace.StorageProvider entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.StorageProvider cloneOfExisting = (Database.StorageProvider)_context.Entry(storageProvider).GetDatabaseValues().ToObject();


			try
			{
				storageProvider.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"DeepSpace.StorageProvider entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.StorageProvider.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.StorageProvider.CreateAnonymousWithFirstLevelSubObjects(storageProvider)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"DeepSpace.StorageProvider entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.StorageProvider.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.StorageProvider.CreateAnonymousWithFirstLevelSubObjects(storageProvider)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of StorageProvider records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/StorageProviders/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			int? storageProviderTypeId = null,
			string configJson = null,
			bool? isDefault = null,
			bool? isEnabled = null,
			string healthStatus = null,
			DateTime? lastHealthCheckUtc = null,
			int? totalCapacityBytes = null,
			int? usedCapacityBytes = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);


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
			if (lastHealthCheckUtc.HasValue == true && lastHealthCheckUtc.Value.Kind != DateTimeKind.Utc)
			{
				lastHealthCheckUtc = lastHealthCheckUtc.Value.ToUniversalTime();
			}

			IQueryable<Database.StorageProvider> query = (from sp in _context.StorageProviders select sp);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(sp => sp.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(sp => sp.description == description);
			}
			if (storageProviderTypeId.HasValue == true)
			{
				query = query.Where(sp => sp.storageProviderTypeId == storageProviderTypeId.Value);
			}
			if (string.IsNullOrEmpty(configJson) == false)
			{
				query = query.Where(sp => sp.configJson == configJson);
			}
			if (isDefault.HasValue == true)
			{
				query = query.Where(sp => sp.isDefault == isDefault.Value);
			}
			if (isEnabled.HasValue == true)
			{
				query = query.Where(sp => sp.isEnabled == isEnabled.Value);
			}
			if (string.IsNullOrEmpty(healthStatus) == false)
			{
				query = query.Where(sp => sp.healthStatus == healthStatus);
			}
			if (lastHealthCheckUtc.HasValue == true)
			{
				query = query.Where(sp => sp.lastHealthCheckUtc == lastHealthCheckUtc.Value);
			}
			if (totalCapacityBytes.HasValue == true)
			{
				query = query.Where(sp => sp.totalCapacityBytes == totalCapacityBytes.Value);
			}
			if (usedCapacityBytes.HasValue == true)
			{
				query = query.Where(sp => sp.usedCapacityBytes == usedCapacityBytes.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sp => sp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sp => sp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sp => sp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sp => sp.deleted == false);
				}
			}
			else
			{
				query = query.Where(sp => sp.active == true);
				query = query.Where(sp => sp.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Storage Provider, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.configJson.Contains(anyStringContains)
			       || x.healthStatus.Contains(anyStringContains)
			       || x.storageProviderType.name.Contains(anyStringContains)
			       || x.storageProviderType.description.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.healthStatus);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.StorageProvider.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/StorageProvider/CreateAuditEvent")]
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
