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
    /// This auto generated class provides the basic CRUD operations for the ReplicationTarget entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ReplicationTarget entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ReplicationTargetsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 100;

		static object replicationTargetPutSyncRoot = new object();
		static object replicationTargetDeleteSyncRoot = new object();

		private DeepSpaceContext _context;

		private ILogger<ReplicationTargetsController> _logger;

		public ReplicationTargetsController(DeepSpaceContext context, ILogger<ReplicationTargetsController> logger) : base("DeepSpace", "ReplicationTarget")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ReplicationTargets filtered by the parameters provided.
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
		[Route("api/ReplicationTargets")]
		public async Task<IActionResult> GetReplicationTargets(
			string name = null,
			string description = null,
			int? sourceStorageProviderId = null,
			int? targetStorageProviderId = null,
			string keyPrefixPattern = null,
			bool? isEnabled = null,
			DateTime? lastSyncUtc = null,
			int? objectsInSync = null,
			int? objectsPendingSync = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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
			if (lastSyncUtc.HasValue == true && lastSyncUtc.Value.Kind != DateTimeKind.Utc)
			{
				lastSyncUtc = lastSyncUtc.Value.ToUniversalTime();
			}

			IQueryable<Database.ReplicationTarget> query = (from rt in _context.ReplicationTargets select rt);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(rt => rt.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(rt => rt.description == description);
			}
			if (sourceStorageProviderId.HasValue == true)
			{
				query = query.Where(rt => rt.sourceStorageProviderId == sourceStorageProviderId.Value);
			}
			if (targetStorageProviderId.HasValue == true)
			{
				query = query.Where(rt => rt.targetStorageProviderId == targetStorageProviderId.Value);
			}
			if (string.IsNullOrEmpty(keyPrefixPattern) == false)
			{
				query = query.Where(rt => rt.keyPrefixPattern == keyPrefixPattern);
			}
			if (isEnabled.HasValue == true)
			{
				query = query.Where(rt => rt.isEnabled == isEnabled.Value);
			}
			if (lastSyncUtc.HasValue == true)
			{
				query = query.Where(rt => rt.lastSyncUtc == lastSyncUtc.Value);
			}
			if (objectsInSync.HasValue == true)
			{
				query = query.Where(rt => rt.objectsInSync == objectsInSync.Value);
			}
			if (objectsPendingSync.HasValue == true)
			{
				query = query.Where(rt => rt.objectsPendingSync == objectsPendingSync.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(rt => rt.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rt => rt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rt => rt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rt => rt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rt => rt.deleted == false);
				}
			}
			else
			{
				query = query.Where(rt => rt.active == true);
				query = query.Where(rt => rt.deleted == false);
			}

			query = query.OrderBy(rt => rt.name).ThenBy(rt => rt.description).ThenBy(rt => rt.keyPrefixPattern);


			//
			// Add the any string contains parameter to span all the string fields on the Replication Target, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.keyPrefixPattern.Contains(anyStringContains)
			       || (includeRelations == true && x.sourceStorageProvider.name.Contains(anyStringContains))
			       || (includeRelations == true && x.sourceStorageProvider.description.Contains(anyStringContains))
			       || (includeRelations == true && x.sourceStorageProvider.configJson.Contains(anyStringContains))
			       || (includeRelations == true && x.sourceStorageProvider.healthStatus.Contains(anyStringContains))
			       || (includeRelations == true && x.targetStorageProvider.name.Contains(anyStringContains))
			       || (includeRelations == true && x.targetStorageProvider.description.Contains(anyStringContains))
			       || (includeRelations == true && x.targetStorageProvider.configJson.Contains(anyStringContains))
			       || (includeRelations == true && x.targetStorageProvider.healthStatus.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.sourceStorageProvider);
				query = query.Include(x => x.targetStorageProvider);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.ReplicationTarget> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ReplicationTarget replicationTarget in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(replicationTarget, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "DeepSpace.ReplicationTarget Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "DeepSpace.ReplicationTarget Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ReplicationTargets filtered by the parameters provided.  Its query is similar to the GetReplicationTargets method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ReplicationTargets/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			int? sourceStorageProviderId = null,
			int? targetStorageProviderId = null,
			string keyPrefixPattern = null,
			bool? isEnabled = null,
			DateTime? lastSyncUtc = null,
			int? objectsInSync = null,
			int? objectsPendingSync = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (lastSyncUtc.HasValue == true && lastSyncUtc.Value.Kind != DateTimeKind.Utc)
			{
				lastSyncUtc = lastSyncUtc.Value.ToUniversalTime();
			}

			IQueryable<Database.ReplicationTarget> query = (from rt in _context.ReplicationTargets select rt);
			if (name != null)
			{
				query = query.Where(rt => rt.name == name);
			}
			if (description != null)
			{
				query = query.Where(rt => rt.description == description);
			}
			if (sourceStorageProviderId.HasValue == true)
			{
				query = query.Where(rt => rt.sourceStorageProviderId == sourceStorageProviderId.Value);
			}
			if (targetStorageProviderId.HasValue == true)
			{
				query = query.Where(rt => rt.targetStorageProviderId == targetStorageProviderId.Value);
			}
			if (keyPrefixPattern != null)
			{
				query = query.Where(rt => rt.keyPrefixPattern == keyPrefixPattern);
			}
			if (isEnabled.HasValue == true)
			{
				query = query.Where(rt => rt.isEnabled == isEnabled.Value);
			}
			if (lastSyncUtc.HasValue == true)
			{
				query = query.Where(rt => rt.lastSyncUtc == lastSyncUtc.Value);
			}
			if (objectsInSync.HasValue == true)
			{
				query = query.Where(rt => rt.objectsInSync == objectsInSync.Value);
			}
			if (objectsPendingSync.HasValue == true)
			{
				query = query.Where(rt => rt.objectsPendingSync == objectsPendingSync.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(rt => rt.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rt => rt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rt => rt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rt => rt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rt => rt.deleted == false);
				}
			}
			else
			{
				query = query.Where(rt => rt.active == true);
				query = query.Where(rt => rt.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Replication Target, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.keyPrefixPattern.Contains(anyStringContains)
			       || x.sourceStorageProvider.name.Contains(anyStringContains)
			       || x.sourceStorageProvider.description.Contains(anyStringContains)
			       || x.sourceStorageProvider.configJson.Contains(anyStringContains)
			       || x.sourceStorageProvider.healthStatus.Contains(anyStringContains)
			       || x.targetStorageProvider.name.Contains(anyStringContains)
			       || x.targetStorageProvider.description.Contains(anyStringContains)
			       || x.targetStorageProvider.configJson.Contains(anyStringContains)
			       || x.targetStorageProvider.healthStatus.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ReplicationTarget by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ReplicationTarget/{id}")]
		public async Task<IActionResult> GetReplicationTarget(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.ReplicationTarget> query = (from rt in _context.ReplicationTargets where
							(rt.id == id) &&
							(userIsAdmin == true || rt.deleted == false) &&
							(userIsWriter == true || rt.active == true)
					select rt);

				if (includeRelations == true)
				{
					query = query.Include(x => x.sourceStorageProvider);
					query = query.Include(x => x.targetStorageProvider);
					query = query.AsSplitQuery();
				}

				Database.ReplicationTarget materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "DeepSpace.ReplicationTarget Entity was read with Admin privilege." : "DeepSpace.ReplicationTarget Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ReplicationTarget", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a DeepSpace.ReplicationTarget entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of DeepSpace.ReplicationTarget.   Entity was read with Admin privilege." : "Exception caught during entity read of DeepSpace.ReplicationTarget.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ReplicationTarget record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ReplicationTarget/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutReplicationTarget(int id, [FromBody]Database.ReplicationTarget.ReplicationTargetDTO replicationTargetDTO, CancellationToken cancellationToken = default)
		{
			if (replicationTargetDTO == null)
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



			if (id != replicationTargetDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.ReplicationTarget> query = (from x in _context.ReplicationTargets
				where
				(x.id == id)
				select x);


			Database.ReplicationTarget existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for DeepSpace.ReplicationTarget PUT", id.ToString(), new Exception("No DeepSpace.ReplicationTarget entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (replicationTargetDTO.objectGuid == Guid.Empty)
            {
                replicationTargetDTO.objectGuid = existing.objectGuid;
            }
            else if (replicationTargetDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ReplicationTarget record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ReplicationTarget cloneOfExisting = (Database.ReplicationTarget)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ReplicationTarget object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ReplicationTarget replicationTarget = (Database.ReplicationTarget)_context.Entry(existing).GetDatabaseValues().ToObject();
			replicationTarget.ApplyDTO(replicationTargetDTO);
			lock (replicationTargetPutSyncRoot)
			{
				//
				// Validate the version number for the replicationTarget being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != replicationTarget.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ReplicationTarget save attempt was made but save request was with version " + replicationTarget.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ReplicationTarget you are trying to update has already changed.  Please try your save again after reloading the ReplicationTarget.");
				}
				else
				{
					// Same record.  Increase version.
					replicationTarget.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (replicationTarget.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted DeepSpace.ReplicationTarget record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (replicationTarget.name != null && replicationTarget.name.Length > 100)
				{
					replicationTarget.name = replicationTarget.name.Substring(0, 100);
				}

				if (replicationTarget.description != null && replicationTarget.description.Length > 500)
				{
					replicationTarget.description = replicationTarget.description.Substring(0, 500);
				}

				if (replicationTarget.keyPrefixPattern != null && replicationTarget.keyPrefixPattern.Length > 500)
				{
					replicationTarget.keyPrefixPattern = replicationTarget.keyPrefixPattern.Substring(0, 500);
				}

				if (replicationTarget.lastSyncUtc.HasValue == true && replicationTarget.lastSyncUtc.Value.Kind != DateTimeKind.Utc)
				{
					replicationTarget.lastSyncUtc = replicationTarget.lastSyncUtc.Value.ToUniversalTime();
				}

				try
				{
				    EntityEntry<Database.ReplicationTarget> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(replicationTarget);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ReplicationTargetChangeHistory replicationTargetChangeHistory = new ReplicationTargetChangeHistory();
				        replicationTargetChangeHistory.replicationTargetId = replicationTarget.id;
				        replicationTargetChangeHistory.versionNumber = replicationTarget.versionNumber;
				        replicationTargetChangeHistory.timeStamp = DateTime.UtcNow;
				        replicationTargetChangeHistory.userId = securityUser.id;
				        replicationTargetChangeHistory.data = JsonSerializer.Serialize(Database.ReplicationTarget.CreateAnonymousWithFirstLevelSubObjects(replicationTarget));
				        _context.ReplicationTargetChangeHistories.Add(replicationTargetChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"DeepSpace.ReplicationTarget entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ReplicationTarget.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ReplicationTarget.CreateAnonymousWithFirstLevelSubObjects(replicationTarget)),
						null);

				return Ok(Database.ReplicationTarget.CreateAnonymous(replicationTarget));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"DeepSpace.ReplicationTarget entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ReplicationTarget.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ReplicationTarget.CreateAnonymousWithFirstLevelSubObjects(replicationTarget)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ReplicationTarget record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ReplicationTarget", Name = "ReplicationTarget")]
		public async Task<IActionResult> PostReplicationTarget([FromBody]Database.ReplicationTarget.ReplicationTargetDTO replicationTargetDTO, CancellationToken cancellationToken = default)
		{
			if (replicationTargetDTO == null)
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
			// Create a new ReplicationTarget object using the data from the DTO
			//
			Database.ReplicationTarget replicationTarget = Database.ReplicationTarget.FromDTO(replicationTargetDTO);

			try
			{
				if (replicationTarget.name != null && replicationTarget.name.Length > 100)
				{
					replicationTarget.name = replicationTarget.name.Substring(0, 100);
				}

				if (replicationTarget.description != null && replicationTarget.description.Length > 500)
				{
					replicationTarget.description = replicationTarget.description.Substring(0, 500);
				}

				if (replicationTarget.keyPrefixPattern != null && replicationTarget.keyPrefixPattern.Length > 500)
				{
					replicationTarget.keyPrefixPattern = replicationTarget.keyPrefixPattern.Substring(0, 500);
				}

				if (replicationTarget.lastSyncUtc.HasValue == true && replicationTarget.lastSyncUtc.Value.Kind != DateTimeKind.Utc)
				{
					replicationTarget.lastSyncUtc = replicationTarget.lastSyncUtc.Value.ToUniversalTime();
				}

				replicationTarget.objectGuid = Guid.NewGuid();
				replicationTarget.versionNumber = 1;

				_context.ReplicationTargets.Add(replicationTarget);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the replicationTarget object so that no further changes will be written to the database
				    //
				    _context.Entry(replicationTarget).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					replicationTarget.ReplicationTargetChangeHistories = null;
					replicationTarget.sourceStorageProvider = null;
					replicationTarget.targetStorageProvider = null;


				    ReplicationTargetChangeHistory replicationTargetChangeHistory = new ReplicationTargetChangeHistory();
				    replicationTargetChangeHistory.replicationTargetId = replicationTarget.id;
				    replicationTargetChangeHistory.versionNumber = replicationTarget.versionNumber;
				    replicationTargetChangeHistory.timeStamp = DateTime.UtcNow;
				    replicationTargetChangeHistory.userId = securityUser.id;
				    replicationTargetChangeHistory.data = JsonSerializer.Serialize(Database.ReplicationTarget.CreateAnonymousWithFirstLevelSubObjects(replicationTarget));
				    _context.ReplicationTargetChangeHistories.Add(replicationTargetChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"DeepSpace.ReplicationTarget entity successfully created.",
						true,
						replicationTarget. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ReplicationTarget.CreateAnonymousWithFirstLevelSubObjects(replicationTarget)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "DeepSpace.ReplicationTarget entity creation failed.", false, replicationTarget.id.ToString(), "", JsonSerializer.Serialize(Database.ReplicationTarget.CreateAnonymousWithFirstLevelSubObjects(replicationTarget)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ReplicationTarget", replicationTarget.id, replicationTarget.name));

			return CreatedAtRoute("ReplicationTarget", new { id = replicationTarget.id }, Database.ReplicationTarget.CreateAnonymousWithFirstLevelSubObjects(replicationTarget));
		}



        /// <summary>
        /// 
        /// This rolls a ReplicationTarget entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ReplicationTarget/Rollback/{id}")]
		[Route("api/ReplicationTarget/Rollback")]
		public async Task<IActionResult> RollbackToReplicationTargetVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.ReplicationTarget> query = (from x in _context.ReplicationTargets
			        where
			        (x.id == id)
			        select x);


			//
			// Make sure nobody else is editing this ReplicationTarget concurrently
			//
			lock (replicationTargetPutSyncRoot)
			{
				
				Database.ReplicationTarget replicationTarget = query.FirstOrDefault();
				
				if (replicationTarget == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for DeepSpace.ReplicationTarget rollback", id.ToString(), new Exception("No DeepSpace.ReplicationTarget entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ReplicationTarget current state so we can log it.
				//
				Database.ReplicationTarget cloneOfExisting = (Database.ReplicationTarget)_context.Entry(replicationTarget).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.ReplicationTargetChangeHistories = null;
				cloneOfExisting.sourceStorageProvider = null;
				cloneOfExisting.targetStorageProvider = null;

				if (versionNumber >= replicationTarget.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for DeepSpace.ReplicationTarget rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for DeepSpace.ReplicationTarget rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ReplicationTargetChangeHistory replicationTargetChangeHistory = (from x in _context.ReplicationTargetChangeHistories
				                                               where
				                                               x.replicationTargetId == id &&
				                                               x.versionNumber == versionNumber
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (replicationTargetChangeHistory != null)
				{
				    Database.ReplicationTarget oldReplicationTarget = JsonSerializer.Deserialize<Database.ReplicationTarget>(replicationTargetChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    replicationTarget.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    replicationTarget.name = oldReplicationTarget.name;
				    replicationTarget.description = oldReplicationTarget.description;
				    replicationTarget.sourceStorageProviderId = oldReplicationTarget.sourceStorageProviderId;
				    replicationTarget.targetStorageProviderId = oldReplicationTarget.targetStorageProviderId;
				    replicationTarget.keyPrefixPattern = oldReplicationTarget.keyPrefixPattern;
				    replicationTarget.isEnabled = oldReplicationTarget.isEnabled;
				    replicationTarget.lastSyncUtc = oldReplicationTarget.lastSyncUtc;
				    replicationTarget.objectsInSync = oldReplicationTarget.objectsInSync;
				    replicationTarget.objectsPendingSync = oldReplicationTarget.objectsPendingSync;
				    replicationTarget.objectGuid = oldReplicationTarget.objectGuid;
				    replicationTarget.active = oldReplicationTarget.active;
				    replicationTarget.deleted = oldReplicationTarget.deleted;

				    string serializedReplicationTarget = JsonSerializer.Serialize(replicationTarget);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ReplicationTargetChangeHistory newReplicationTargetChangeHistory = new ReplicationTargetChangeHistory();
				        newReplicationTargetChangeHistory.replicationTargetId = replicationTarget.id;
				        newReplicationTargetChangeHistory.versionNumber = replicationTarget.versionNumber;
				        newReplicationTargetChangeHistory.timeStamp = DateTime.UtcNow;
				        newReplicationTargetChangeHistory.userId = securityUser.id;
				        newReplicationTargetChangeHistory.data = JsonSerializer.Serialize(Database.ReplicationTarget.CreateAnonymousWithFirstLevelSubObjects(replicationTarget));
				        _context.ReplicationTargetChangeHistories.Add(newReplicationTargetChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"DeepSpace.ReplicationTarget rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ReplicationTarget.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ReplicationTarget.CreateAnonymousWithFirstLevelSubObjects(replicationTarget)),
						null);


				    return Ok(Database.ReplicationTarget.CreateAnonymous(replicationTarget));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for DeepSpace.ReplicationTarget rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for DeepSpace.ReplicationTarget rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a ReplicationTarget.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ReplicationTarget</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ReplicationTarget/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetReplicationTargetChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Database.ReplicationTarget replicationTarget = await _context.ReplicationTargets.Where(x => x.id == id
			).FirstOrDefaultAsync(cancellationToken);

			if (replicationTarget == null)
			{
				return NotFound();
			}

			try
			{
				replicationTarget.SetupVersionInquiry(_context, Guid.Empty);

				VersionInformation<Database.ReplicationTarget> versionInfo = await replicationTarget.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a ReplicationTarget.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ReplicationTarget</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ReplicationTarget/{id}/AuditHistory")]
		public async Task<IActionResult> GetReplicationTargetAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Database.ReplicationTarget replicationTarget = await _context.ReplicationTargets.Where(x => x.id == id
			).FirstOrDefaultAsync(cancellationToken);

			if (replicationTarget == null)
			{
				return NotFound();
			}

			try
			{
				replicationTarget.SetupVersionInquiry(_context, Guid.Empty);

				List<VersionInformation<Database.ReplicationTarget>> versions = await replicationTarget.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a ReplicationTarget.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ReplicationTarget</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The ReplicationTarget object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ReplicationTarget/{id}/Version/{version}")]
		public async Task<IActionResult> GetReplicationTargetVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Database.ReplicationTarget replicationTarget = await _context.ReplicationTargets.Where(x => x.id == id
			).FirstOrDefaultAsync(cancellationToken);

			if (replicationTarget == null)
			{
				return NotFound();
			}

			try
			{
				replicationTarget.SetupVersionInquiry(_context, Guid.Empty);

				VersionInformation<Database.ReplicationTarget> versionInfo = await replicationTarget.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a ReplicationTarget at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ReplicationTarget</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The ReplicationTarget object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ReplicationTarget/{id}/StateAtTime")]
		public async Task<IActionResult> GetReplicationTargetStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Database.ReplicationTarget replicationTarget = await _context.ReplicationTargets.Where(x => x.id == id
			).FirstOrDefaultAsync(cancellationToken);

			if (replicationTarget == null)
			{
				return NotFound();
			}

			try
			{
				replicationTarget.SetupVersionInquiry(_context, Guid.Empty);

				VersionInformation<Database.ReplicationTarget> versionInfo = await replicationTarget.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a ReplicationTarget record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ReplicationTarget/{id}")]
		[Route("api/ReplicationTarget")]
		public async Task<IActionResult> DeleteReplicationTarget(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ReplicationTarget> query = (from x in _context.ReplicationTargets
				where
				(x.id == id)
				select x);


			Database.ReplicationTarget replicationTarget = await query.FirstOrDefaultAsync(cancellationToken);

			if (replicationTarget == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for DeepSpace.ReplicationTarget DELETE", id.ToString(), new Exception("No DeepSpace.ReplicationTarget entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ReplicationTarget cloneOfExisting = (Database.ReplicationTarget)_context.Entry(replicationTarget).GetDatabaseValues().ToObject();


			lock (replicationTargetDeleteSyncRoot)
			{
			    try
			    {
			        replicationTarget.deleted = true;
			        replicationTarget.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ReplicationTargetChangeHistory replicationTargetChangeHistory = new ReplicationTargetChangeHistory();
			        replicationTargetChangeHistory.replicationTargetId = replicationTarget.id;
			        replicationTargetChangeHistory.versionNumber = replicationTarget.versionNumber;
			        replicationTargetChangeHistory.timeStamp = DateTime.UtcNow;
			        replicationTargetChangeHistory.userId = securityUser.id;
			        replicationTargetChangeHistory.data = JsonSerializer.Serialize(Database.ReplicationTarget.CreateAnonymousWithFirstLevelSubObjects(replicationTarget));
			        _context.ReplicationTargetChangeHistories.Add(replicationTargetChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"DeepSpace.ReplicationTarget entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ReplicationTarget.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ReplicationTarget.CreateAnonymousWithFirstLevelSubObjects(replicationTarget)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"DeepSpace.ReplicationTarget entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ReplicationTarget.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ReplicationTarget.CreateAnonymousWithFirstLevelSubObjects(replicationTarget)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of ReplicationTarget records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ReplicationTargets/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			int? sourceStorageProviderId = null,
			int? targetStorageProviderId = null,
			string keyPrefixPattern = null,
			bool? isEnabled = null,
			DateTime? lastSyncUtc = null,
			int? objectsInSync = null,
			int? objectsPendingSync = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);


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
			if (lastSyncUtc.HasValue == true && lastSyncUtc.Value.Kind != DateTimeKind.Utc)
			{
				lastSyncUtc = lastSyncUtc.Value.ToUniversalTime();
			}

			IQueryable<Database.ReplicationTarget> query = (from rt in _context.ReplicationTargets select rt);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(rt => rt.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(rt => rt.description == description);
			}
			if (sourceStorageProviderId.HasValue == true)
			{
				query = query.Where(rt => rt.sourceStorageProviderId == sourceStorageProviderId.Value);
			}
			if (targetStorageProviderId.HasValue == true)
			{
				query = query.Where(rt => rt.targetStorageProviderId == targetStorageProviderId.Value);
			}
			if (string.IsNullOrEmpty(keyPrefixPattern) == false)
			{
				query = query.Where(rt => rt.keyPrefixPattern == keyPrefixPattern);
			}
			if (isEnabled.HasValue == true)
			{
				query = query.Where(rt => rt.isEnabled == isEnabled.Value);
			}
			if (lastSyncUtc.HasValue == true)
			{
				query = query.Where(rt => rt.lastSyncUtc == lastSyncUtc.Value);
			}
			if (objectsInSync.HasValue == true)
			{
				query = query.Where(rt => rt.objectsInSync == objectsInSync.Value);
			}
			if (objectsPendingSync.HasValue == true)
			{
				query = query.Where(rt => rt.objectsPendingSync == objectsPendingSync.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(rt => rt.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rt => rt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rt => rt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rt => rt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rt => rt.deleted == false);
				}
			}
			else
			{
				query = query.Where(rt => rt.active == true);
				query = query.Where(rt => rt.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Replication Target, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.keyPrefixPattern.Contains(anyStringContains)
			       || x.sourceStorageProvider.name.Contains(anyStringContains)
			       || x.sourceStorageProvider.description.Contains(anyStringContains)
			       || x.sourceStorageProvider.configJson.Contains(anyStringContains)
			       || x.sourceStorageProvider.healthStatus.Contains(anyStringContains)
			       || x.targetStorageProvider.name.Contains(anyStringContains)
			       || x.targetStorageProvider.description.Contains(anyStringContains)
			       || x.targetStorageProvider.configJson.Contains(anyStringContains)
			       || x.targetStorageProvider.healthStatus.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.keyPrefixPattern);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ReplicationTarget.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ReplicationTarget/CreateAuditEvent")]
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
