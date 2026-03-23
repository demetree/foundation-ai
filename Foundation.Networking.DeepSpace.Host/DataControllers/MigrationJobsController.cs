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
    /// This auto generated class provides the basic CRUD operations for the MigrationJob entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the MigrationJob entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class MigrationJobsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 100;

		private DeepSpaceContext _context;

		private ILogger<MigrationJobsController> _logger;

		public MigrationJobsController(DeepSpaceContext context, ILogger<MigrationJobsController> logger) : base("DeepSpace", "MigrationJob")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of MigrationJobs filtered by the parameters provided.
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
		[Route("api/MigrationJobs")]
		public async Task<IActionResult> GetMigrationJobs(
			int? lifecycleRuleId = null,
			int? storageObjectId = null,
			int? sourceStorageProviderId = null,
			int? targetStorageProviderId = null,
			int? migrationJobStatusId = null,
			DateTime? startedUtc = null,
			DateTime? completedUtc = null,
			int? bytesTransferred = null,
			string errorMessage = null,
			int? retryCount = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			int? pageSize = null,
			int? pageNumber = null,
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
			if (startedUtc.HasValue == true && startedUtc.Value.Kind != DateTimeKind.Utc)
			{
				startedUtc = startedUtc.Value.ToUniversalTime();
			}

			if (completedUtc.HasValue == true && completedUtc.Value.Kind != DateTimeKind.Utc)
			{
				completedUtc = completedUtc.Value.ToUniversalTime();
			}

			IQueryable<Database.MigrationJob> query = (from mj in _context.MigrationJobs select mj);
			if (lifecycleRuleId.HasValue == true)
			{
				query = query.Where(mj => mj.lifecycleRuleId == lifecycleRuleId.Value);
			}
			if (storageObjectId.HasValue == true)
			{
				query = query.Where(mj => mj.storageObjectId == storageObjectId.Value);
			}
			if (sourceStorageProviderId.HasValue == true)
			{
				query = query.Where(mj => mj.sourceStorageProviderId == sourceStorageProviderId.Value);
			}
			if (targetStorageProviderId.HasValue == true)
			{
				query = query.Where(mj => mj.targetStorageProviderId == targetStorageProviderId.Value);
			}
			if (migrationJobStatusId.HasValue == true)
			{
				query = query.Where(mj => mj.migrationJobStatusId == migrationJobStatusId.Value);
			}
			if (startedUtc.HasValue == true)
			{
				query = query.Where(mj => mj.startedUtc == startedUtc.Value);
			}
			if (completedUtc.HasValue == true)
			{
				query = query.Where(mj => mj.completedUtc == completedUtc.Value);
			}
			if (bytesTransferred.HasValue == true)
			{
				query = query.Where(mj => mj.bytesTransferred == bytesTransferred.Value);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(mj => mj.errorMessage == errorMessage);
			}
			if (retryCount.HasValue == true)
			{
				query = query.Where(mj => mj.retryCount == retryCount.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mj => mj.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mj => mj.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mj => mj.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mj => mj.deleted == false);
				}
			}
			else
			{
				query = query.Where(mj => mj.active == true);
				query = query.Where(mj => mj.deleted == false);
			}

			query = query.OrderByDescending(mj => mj.id);

			if (includeRelations == true)
			{
				query = query.Include(x => x.lifecycleRule);
				query = query.Include(x => x.migrationJobStatus);
				query = query.Include(x => x.sourceStorageProvider);
				query = query.Include(x => x.storageObject);
				query = query.Include(x => x.targetStorageProvider);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.MigrationJob> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.MigrationJob migrationJob in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(migrationJob, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "DeepSpace.MigrationJob Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "DeepSpace.MigrationJob Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of MigrationJobs filtered by the parameters provided.  Its query is similar to the GetMigrationJobs method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MigrationJobs/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? lifecycleRuleId = null,
			int? storageObjectId = null,
			int? sourceStorageProviderId = null,
			int? targetStorageProviderId = null,
			int? migrationJobStatusId = null,
			DateTime? startedUtc = null,
			DateTime? completedUtc = null,
			int? bytesTransferred = null,
			string errorMessage = null,
			int? retryCount = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
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
			if (startedUtc.HasValue == true && startedUtc.Value.Kind != DateTimeKind.Utc)
			{
				startedUtc = startedUtc.Value.ToUniversalTime();
			}

			if (completedUtc.HasValue == true && completedUtc.Value.Kind != DateTimeKind.Utc)
			{
				completedUtc = completedUtc.Value.ToUniversalTime();
			}

			IQueryable<Database.MigrationJob> query = (from mj in _context.MigrationJobs select mj);
			if (lifecycleRuleId.HasValue == true)
			{
				query = query.Where(mj => mj.lifecycleRuleId == lifecycleRuleId.Value);
			}
			if (storageObjectId.HasValue == true)
			{
				query = query.Where(mj => mj.storageObjectId == storageObjectId.Value);
			}
			if (sourceStorageProviderId.HasValue == true)
			{
				query = query.Where(mj => mj.sourceStorageProviderId == sourceStorageProviderId.Value);
			}
			if (targetStorageProviderId.HasValue == true)
			{
				query = query.Where(mj => mj.targetStorageProviderId == targetStorageProviderId.Value);
			}
			if (migrationJobStatusId.HasValue == true)
			{
				query = query.Where(mj => mj.migrationJobStatusId == migrationJobStatusId.Value);
			}
			if (startedUtc.HasValue == true)
			{
				query = query.Where(mj => mj.startedUtc == startedUtc.Value);
			}
			if (completedUtc.HasValue == true)
			{
				query = query.Where(mj => mj.completedUtc == completedUtc.Value);
			}
			if (bytesTransferred.HasValue == true)
			{
				query = query.Where(mj => mj.bytesTransferred == bytesTransferred.Value);
			}
			if (errorMessage != null)
			{
				query = query.Where(mj => mj.errorMessage == errorMessage);
			}
			if (retryCount.HasValue == true)
			{
				query = query.Where(mj => mj.retryCount == retryCount.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mj => mj.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mj => mj.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mj => mj.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mj => mj.deleted == false);
				}
			}
			else
			{
				query = query.Where(mj => mj.active == true);
				query = query.Where(mj => mj.deleted == false);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single MigrationJob by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MigrationJob/{id}")]
		public async Task<IActionResult> GetMigrationJob(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.MigrationJob> query = (from mj in _context.MigrationJobs where
							(mj.id == id) &&
							(userIsAdmin == true || mj.deleted == false) &&
							(userIsWriter == true || mj.active == true)
					select mj);

				if (includeRelations == true)
				{
					query = query.Include(x => x.lifecycleRule);
					query = query.Include(x => x.migrationJobStatus);
					query = query.Include(x => x.sourceStorageProvider);
					query = query.Include(x => x.storageObject);
					query = query.Include(x => x.targetStorageProvider);
					query = query.AsSplitQuery();
				}

				Database.MigrationJob materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "DeepSpace.MigrationJob Entity was read with Admin privilege." : "DeepSpace.MigrationJob Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "MigrationJob", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a DeepSpace.MigrationJob entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of DeepSpace.MigrationJob.   Entity was read with Admin privilege." : "Exception caught during entity read of DeepSpace.MigrationJob.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing MigrationJob record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/MigrationJob/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutMigrationJob(int id, [FromBody]Database.MigrationJob.MigrationJobDTO migrationJobDTO, CancellationToken cancellationToken = default)
		{
			if (migrationJobDTO == null)
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



			if (id != migrationJobDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.MigrationJob> query = (from x in _context.MigrationJobs
				where
				(x.id == id)
				select x);


			Database.MigrationJob existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for DeepSpace.MigrationJob PUT", id.ToString(), new Exception("No DeepSpace.MigrationJob entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (migrationJobDTO.objectGuid == Guid.Empty)
            {
                migrationJobDTO.objectGuid = existing.objectGuid;
            }
            else if (migrationJobDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a MigrationJob record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.MigrationJob cloneOfExisting = (Database.MigrationJob)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new MigrationJob object using the data from the existing record, updated with what is in the DTO.
			//
			Database.MigrationJob migrationJob = (Database.MigrationJob)_context.Entry(existing).GetDatabaseValues().ToObject();
			migrationJob.ApplyDTO(migrationJobDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (migrationJob.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted DeepSpace.MigrationJob record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (migrationJob.startedUtc.HasValue == true && migrationJob.startedUtc.Value.Kind != DateTimeKind.Utc)
			{
				migrationJob.startedUtc = migrationJob.startedUtc.Value.ToUniversalTime();
			}

			if (migrationJob.completedUtc.HasValue == true && migrationJob.completedUtc.Value.Kind != DateTimeKind.Utc)
			{
				migrationJob.completedUtc = migrationJob.completedUtc.Value.ToUniversalTime();
			}

			EntityEntry<Database.MigrationJob> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(migrationJob);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"DeepSpace.MigrationJob entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.MigrationJob.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MigrationJob.CreateAnonymousWithFirstLevelSubObjects(migrationJob)),
					null);


				return Ok(Database.MigrationJob.CreateAnonymous(migrationJob));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"DeepSpace.MigrationJob entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.MigrationJob.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MigrationJob.CreateAnonymousWithFirstLevelSubObjects(migrationJob)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new MigrationJob record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MigrationJob", Name = "MigrationJob")]
		public async Task<IActionResult> PostMigrationJob([FromBody]Database.MigrationJob.MigrationJobDTO migrationJobDTO, CancellationToken cancellationToken = default)
		{
			if (migrationJobDTO == null)
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
			// Create a new MigrationJob object using the data from the DTO
			//
			Database.MigrationJob migrationJob = Database.MigrationJob.FromDTO(migrationJobDTO);

			try
			{
				if (migrationJob.startedUtc.HasValue == true && migrationJob.startedUtc.Value.Kind != DateTimeKind.Utc)
				{
					migrationJob.startedUtc = migrationJob.startedUtc.Value.ToUniversalTime();
				}

				if (migrationJob.completedUtc.HasValue == true && migrationJob.completedUtc.Value.Kind != DateTimeKind.Utc)
				{
					migrationJob.completedUtc = migrationJob.completedUtc.Value.ToUniversalTime();
				}

				migrationJob.objectGuid = Guid.NewGuid();
				_context.MigrationJobs.Add(migrationJob);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"DeepSpace.MigrationJob entity successfully created.",
					true,
					migrationJob.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.MigrationJob.CreateAnonymousWithFirstLevelSubObjects(migrationJob)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "DeepSpace.MigrationJob entity creation failed.", false, migrationJob.id.ToString(), "", JsonSerializer.Serialize(migrationJob), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "MigrationJob", migrationJob.id, migrationJob.id.ToString()));

			return CreatedAtRoute("MigrationJob", new { id = migrationJob.id }, Database.MigrationJob.CreateAnonymousWithFirstLevelSubObjects(migrationJob));
		}



        /// <summary>
        /// 
        /// This deletes a MigrationJob record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MigrationJob/{id}")]
		[Route("api/MigrationJob")]
		public async Task<IActionResult> DeleteMigrationJob(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.MigrationJob> query = (from x in _context.MigrationJobs
				where
				(x.id == id)
				select x);


			Database.MigrationJob migrationJob = await query.FirstOrDefaultAsync(cancellationToken);

			if (migrationJob == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for DeepSpace.MigrationJob DELETE", id.ToString(), new Exception("No DeepSpace.MigrationJob entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.MigrationJob cloneOfExisting = (Database.MigrationJob)_context.Entry(migrationJob).GetDatabaseValues().ToObject();


			try
			{
				migrationJob.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"DeepSpace.MigrationJob entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.MigrationJob.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MigrationJob.CreateAnonymousWithFirstLevelSubObjects(migrationJob)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"DeepSpace.MigrationJob entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.MigrationJob.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MigrationJob.CreateAnonymousWithFirstLevelSubObjects(migrationJob)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of MigrationJob records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/MigrationJobs/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? lifecycleRuleId = null,
			int? storageObjectId = null,
			int? sourceStorageProviderId = null,
			int? targetStorageProviderId = null,
			int? migrationJobStatusId = null,
			DateTime? startedUtc = null,
			DateTime? completedUtc = null,
			int? bytesTransferred = null,
			string errorMessage = null,
			int? retryCount = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
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
			if (startedUtc.HasValue == true && startedUtc.Value.Kind != DateTimeKind.Utc)
			{
				startedUtc = startedUtc.Value.ToUniversalTime();
			}

			if (completedUtc.HasValue == true && completedUtc.Value.Kind != DateTimeKind.Utc)
			{
				completedUtc = completedUtc.Value.ToUniversalTime();
			}

			IQueryable<Database.MigrationJob> query = (from mj in _context.MigrationJobs select mj);
			if (lifecycleRuleId.HasValue == true)
			{
				query = query.Where(mj => mj.lifecycleRuleId == lifecycleRuleId.Value);
			}
			if (storageObjectId.HasValue == true)
			{
				query = query.Where(mj => mj.storageObjectId == storageObjectId.Value);
			}
			if (sourceStorageProviderId.HasValue == true)
			{
				query = query.Where(mj => mj.sourceStorageProviderId == sourceStorageProviderId.Value);
			}
			if (targetStorageProviderId.HasValue == true)
			{
				query = query.Where(mj => mj.targetStorageProviderId == targetStorageProviderId.Value);
			}
			if (migrationJobStatusId.HasValue == true)
			{
				query = query.Where(mj => mj.migrationJobStatusId == migrationJobStatusId.Value);
			}
			if (startedUtc.HasValue == true)
			{
				query = query.Where(mj => mj.startedUtc == startedUtc.Value);
			}
			if (completedUtc.HasValue == true)
			{
				query = query.Where(mj => mj.completedUtc == completedUtc.Value);
			}
			if (bytesTransferred.HasValue == true)
			{
				query = query.Where(mj => mj.bytesTransferred == bytesTransferred.Value);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(mj => mj.errorMessage == errorMessage);
			}
			if (retryCount.HasValue == true)
			{
				query = query.Where(mj => mj.retryCount == retryCount.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mj => mj.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mj => mj.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mj => mj.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mj => mj.deleted == false);
				}
			}
			else
			{
				query = query.Where(mj => mj.active == true);
				query = query.Where(mj => mj.deleted == false);
			}


			query = query.OrderByDescending (x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.MigrationJob.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/MigrationJob/CreateAuditEvent")]
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
