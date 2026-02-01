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
using Foundation.Telemetry.Database;

namespace Foundation.Telemetry.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the TelemetrySnapshot entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the TelemetrySnapshot entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class TelemetrySnapshotsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private TelemetryContext _context;

		private ILogger<TelemetrySnapshotsController> _logger;

		public TelemetrySnapshotsController(TelemetryContext context, ILogger<TelemetrySnapshotsController> logger) : base("Telemetry", "TelemetrySnapshot")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of TelemetrySnapshots filtered by the parameters provided.
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
		[Route("api/TelemetrySnapshots")]
		public async Task<IActionResult> GetTelemetrySnapshots(
			int? telemetryApplicationId = null,
			int? telemetryCollectionRunId = null,
			DateTime? collectedAt = null,
			bool? isOnline = null,
			long? uptimeSeconds = null,
			double? memoryWorkingSetMB = null,
			double? memoryGcHeapMB = null,
			double? memoryPercent = null,
			double? systemMemoryPercent = null,
			double? cpuPercent = null,
			double? systemCpuPercent = null,
			int? threadPoolWorkerThreads = null,
			int? threadPoolCompletionPortThreads = null,
			int? threadPoolPendingWorkItems = null,
			string machineName = null,
			string dotNetVersion = null,
			string statusJson = null,
			int? pageSize = null,
			int? pageNumber = null,
			string anyStringContains = null,
			bool includeRelations = true,
			CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
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
			if (collectedAt.HasValue == true && collectedAt.Value.Kind != DateTimeKind.Utc)
			{
				collectedAt = collectedAt.Value.ToUniversalTime();
			}

			IQueryable<Database.TelemetrySnapshot> query = (from ts in _context.TelemetrySnapshots select ts);
			if (telemetryApplicationId.HasValue == true)
			{
				query = query.Where(ts => ts.telemetryApplicationId == telemetryApplicationId.Value);
			}
			if (telemetryCollectionRunId.HasValue == true)
			{
				query = query.Where(ts => ts.telemetryCollectionRunId == telemetryCollectionRunId.Value);
			}
			if (collectedAt.HasValue == true)
			{
				query = query.Where(ts => ts.collectedAt == collectedAt.Value);
			}
			if (isOnline.HasValue == true)
			{
				query = query.Where(ts => ts.isOnline == isOnline.Value);
			}
			if (uptimeSeconds.HasValue == true)
			{
				query = query.Where(ts => ts.uptimeSeconds == uptimeSeconds.Value);
			}
			if (memoryWorkingSetMB.HasValue == true)
			{
				query = query.Where(ts => ts.memoryWorkingSetMB == memoryWorkingSetMB.Value);
			}
			if (memoryGcHeapMB.HasValue == true)
			{
				query = query.Where(ts => ts.memoryGcHeapMB == memoryGcHeapMB.Value);
			}
			if (memoryPercent.HasValue == true)
			{
				query = query.Where(ts => ts.memoryPercent == memoryPercent.Value);
			}
			if (systemMemoryPercent.HasValue == true)
			{
				query = query.Where(ts => ts.systemMemoryPercent == systemMemoryPercent.Value);
			}
			if (cpuPercent.HasValue == true)
			{
				query = query.Where(ts => ts.cpuPercent == cpuPercent.Value);
			}
			if (systemCpuPercent.HasValue == true)
			{
				query = query.Where(ts => ts.systemCpuPercent == systemCpuPercent.Value);
			}
			if (threadPoolWorkerThreads.HasValue == true)
			{
				query = query.Where(ts => ts.threadPoolWorkerThreads == threadPoolWorkerThreads.Value);
			}
			if (threadPoolCompletionPortThreads.HasValue == true)
			{
				query = query.Where(ts => ts.threadPoolCompletionPortThreads == threadPoolCompletionPortThreads.Value);
			}
			if (threadPoolPendingWorkItems.HasValue == true)
			{
				query = query.Where(ts => ts.threadPoolPendingWorkItems == threadPoolPendingWorkItems.Value);
			}
			if (string.IsNullOrEmpty(machineName) == false)
			{
				query = query.Where(ts => ts.machineName == machineName);
			}
			if (string.IsNullOrEmpty(dotNetVersion) == false)
			{
				query = query.Where(ts => ts.dotNetVersion == dotNetVersion);
			}
			if (string.IsNullOrEmpty(statusJson) == false)
			{
				query = query.Where(ts => ts.statusJson == statusJson);
			}

			query = query.OrderBy(ts => ts.machineName).ThenBy(ts => ts.dotNetVersion);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.telemetryApplication);
				query = query.Include(x => x.telemetryCollectionRun);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Telemetry Snapshot, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.machineName.Contains(anyStringContains)
			       || x.dotNetVersion.Contains(anyStringContains)
			       || x.statusJson.Contains(anyStringContains)
			       || (includeRelations == true && x.telemetryApplication.name.Contains(anyStringContains))
			       || (includeRelations == true && x.telemetryApplication.url.Contains(anyStringContains))
			       || (includeRelations == true && x.telemetryCollectionRun.errorMessage.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.TelemetrySnapshot> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.TelemetrySnapshot telemetrySnapshot in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(telemetrySnapshot, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Telemetry.TelemetrySnapshot Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Telemetry.TelemetrySnapshot Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of TelemetrySnapshots filtered by the parameters provided.  Its query is similar to the GetTelemetrySnapshots method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetrySnapshots/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? telemetryApplicationId = null,
			int? telemetryCollectionRunId = null,
			DateTime? collectedAt = null,
			bool? isOnline = null,
			long? uptimeSeconds = null,
			double? memoryWorkingSetMB = null,
			double? memoryGcHeapMB = null,
			double? memoryPercent = null,
			double? systemMemoryPercent = null,
			double? cpuPercent = null,
			double? systemCpuPercent = null,
			int? threadPoolWorkerThreads = null,
			int? threadPoolCompletionPortThreads = null,
			int? threadPoolPendingWorkItems = null,
			string machineName = null,
			string dotNetVersion = null,
			string statusJson = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (collectedAt.HasValue == true && collectedAt.Value.Kind != DateTimeKind.Utc)
			{
				collectedAt = collectedAt.Value.ToUniversalTime();
			}

			IQueryable<Database.TelemetrySnapshot> query = (from ts in _context.TelemetrySnapshots select ts);
			if (telemetryApplicationId.HasValue == true)
			{
				query = query.Where(ts => ts.telemetryApplicationId == telemetryApplicationId.Value);
			}
			if (telemetryCollectionRunId.HasValue == true)
			{
				query = query.Where(ts => ts.telemetryCollectionRunId == telemetryCollectionRunId.Value);
			}
			if (collectedAt.HasValue == true)
			{
				query = query.Where(ts => ts.collectedAt == collectedAt.Value);
			}
			if (isOnline.HasValue == true)
			{
				query = query.Where(ts => ts.isOnline == isOnline.Value);
			}
			if (uptimeSeconds.HasValue == true)
			{
				query = query.Where(ts => ts.uptimeSeconds == uptimeSeconds.Value);
			}
			if (memoryWorkingSetMB.HasValue == true)
			{
				query = query.Where(ts => ts.memoryWorkingSetMB == memoryWorkingSetMB.Value);
			}
			if (memoryGcHeapMB.HasValue == true)
			{
				query = query.Where(ts => ts.memoryGcHeapMB == memoryGcHeapMB.Value);
			}
			if (memoryPercent.HasValue == true)
			{
				query = query.Where(ts => ts.memoryPercent == memoryPercent.Value);
			}
			if (systemMemoryPercent.HasValue == true)
			{
				query = query.Where(ts => ts.systemMemoryPercent == systemMemoryPercent.Value);
			}
			if (cpuPercent.HasValue == true)
			{
				query = query.Where(ts => ts.cpuPercent == cpuPercent.Value);
			}
			if (systemCpuPercent.HasValue == true)
			{
				query = query.Where(ts => ts.systemCpuPercent == systemCpuPercent.Value);
			}
			if (threadPoolWorkerThreads.HasValue == true)
			{
				query = query.Where(ts => ts.threadPoolWorkerThreads == threadPoolWorkerThreads.Value);
			}
			if (threadPoolCompletionPortThreads.HasValue == true)
			{
				query = query.Where(ts => ts.threadPoolCompletionPortThreads == threadPoolCompletionPortThreads.Value);
			}
			if (threadPoolPendingWorkItems.HasValue == true)
			{
				query = query.Where(ts => ts.threadPoolPendingWorkItems == threadPoolPendingWorkItems.Value);
			}
			if (machineName != null)
			{
				query = query.Where(ts => ts.machineName == machineName);
			}
			if (dotNetVersion != null)
			{
				query = query.Where(ts => ts.dotNetVersion == dotNetVersion);
			}
			if (statusJson != null)
			{
				query = query.Where(ts => ts.statusJson == statusJson);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Telemetry Snapshot, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.machineName.Contains(anyStringContains)
			       || x.dotNetVersion.Contains(anyStringContains)
			       || x.statusJson.Contains(anyStringContains)
			       || x.telemetryApplication.name.Contains(anyStringContains)
			       || x.telemetryApplication.url.Contains(anyStringContains)
			       || x.telemetryCollectionRun.errorMessage.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single TelemetrySnapshot by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetrySnapshot/{id}")]
		public async Task<IActionResult> GetTelemetrySnapshot(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.TelemetrySnapshot> query = (from ts in _context.TelemetrySnapshots where
				(ts.id == id)
					select ts);

				if (includeRelations == true)
				{
					query = query.Include(x => x.telemetryApplication);
					query = query.Include(x => x.telemetryCollectionRun);
					query = query.AsSplitQuery();
				}

				Database.TelemetrySnapshot materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Telemetry.TelemetrySnapshot Entity was read with Admin privilege." : "Telemetry.TelemetrySnapshot Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TelemetrySnapshot", materialized.id, materialized.machineName));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Telemetry.TelemetrySnapshot entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Telemetry.TelemetrySnapshot.   Entity was read with Admin privilege." : "Exception caught during entity read of Telemetry.TelemetrySnapshot.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing TelemetrySnapshot record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/TelemetrySnapshot/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutTelemetrySnapshot(int id, [FromBody]Database.TelemetrySnapshot.TelemetrySnapshotDTO telemetrySnapshotDTO, CancellationToken cancellationToken = default)
		{
			if (telemetrySnapshotDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			// Admin privilege needed to write to this table.
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != telemetrySnapshotDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.TelemetrySnapshot> query = (from x in _context.TelemetrySnapshots
				where
				(x.id == id)
				select x);


			Database.TelemetrySnapshot existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Telemetry.TelemetrySnapshot PUT", id.ToString(), new Exception("No Telemetry.TelemetrySnapshot entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.TelemetrySnapshot cloneOfExisting = (Database.TelemetrySnapshot)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new TelemetrySnapshot object using the data from the existing record, updated with what is in the DTO.
			//
			Database.TelemetrySnapshot telemetrySnapshot = (Database.TelemetrySnapshot)_context.Entry(existing).GetDatabaseValues().ToObject();
			telemetrySnapshot.ApplyDTO(telemetrySnapshotDTO);


			if (telemetrySnapshot.collectedAt.Kind != DateTimeKind.Utc)
			{
				telemetrySnapshot.collectedAt = telemetrySnapshot.collectedAt.ToUniversalTime();
			}

			if (telemetrySnapshot.machineName != null && telemetrySnapshot.machineName.Length > 100)
			{
				telemetrySnapshot.machineName = telemetrySnapshot.machineName.Substring(0, 100);
			}

			if (telemetrySnapshot.dotNetVersion != null && telemetrySnapshot.dotNetVersion.Length > 50)
			{
				telemetrySnapshot.dotNetVersion = telemetrySnapshot.dotNetVersion.Substring(0, 50);
			}

			EntityEntry<Database.TelemetrySnapshot> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(telemetrySnapshot);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Telemetry.TelemetrySnapshot entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetrySnapshot.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetrySnapshot.CreateAnonymousWithFirstLevelSubObjects(telemetrySnapshot)),
					null);


				return Ok(Database.TelemetrySnapshot.CreateAnonymous(telemetrySnapshot));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Telemetry.TelemetrySnapshot entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetrySnapshot.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetrySnapshot.CreateAnonymousWithFirstLevelSubObjects(telemetrySnapshot)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new TelemetrySnapshot record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetrySnapshot", Name = "TelemetrySnapshot")]
		public async Task<IActionResult> PostTelemetrySnapshot([FromBody]Database.TelemetrySnapshot.TelemetrySnapshotDTO telemetrySnapshotDTO, CancellationToken cancellationToken = default)
		{
			if (telemetrySnapshotDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			// Admin privilege needed to write to this table.
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new TelemetrySnapshot object using the data from the DTO
			//
			Database.TelemetrySnapshot telemetrySnapshot = Database.TelemetrySnapshot.FromDTO(telemetrySnapshotDTO);

			try
			{
				if (telemetrySnapshot.collectedAt.Kind != DateTimeKind.Utc)
				{
					telemetrySnapshot.collectedAt = telemetrySnapshot.collectedAt.ToUniversalTime();
				}

				if (telemetrySnapshot.machineName != null && telemetrySnapshot.machineName.Length > 100)
				{
					telemetrySnapshot.machineName = telemetrySnapshot.machineName.Substring(0, 100);
				}

				if (telemetrySnapshot.dotNetVersion != null && telemetrySnapshot.dotNetVersion.Length > 50)
				{
					telemetrySnapshot.dotNetVersion = telemetrySnapshot.dotNetVersion.Substring(0, 50);
				}

				_context.TelemetrySnapshots.Add(telemetrySnapshot);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Telemetry.TelemetrySnapshot entity successfully created.",
					true,
					telemetrySnapshot.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.TelemetrySnapshot.CreateAnonymousWithFirstLevelSubObjects(telemetrySnapshot)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Telemetry.TelemetrySnapshot entity creation failed.", false, telemetrySnapshot.id.ToString(), "", JsonSerializer.Serialize(telemetrySnapshot), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TelemetrySnapshot", telemetrySnapshot.id, telemetrySnapshot.machineName));

			return CreatedAtRoute("TelemetrySnapshot", new { id = telemetrySnapshot.id }, Database.TelemetrySnapshot.CreateAnonymousWithFirstLevelSubObjects(telemetrySnapshot));
		}



        /// <summary>
        /// 
        /// This deletes a TelemetrySnapshot record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetrySnapshot/{id}")]
		[Route("api/TelemetrySnapshot")]
		public async Task<IActionResult> DeleteTelemetrySnapshot(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.TelemetrySnapshot> query = (from x in _context.TelemetrySnapshots
				where
				(x.id == id)
				select x);


			Database.TelemetrySnapshot telemetrySnapshot = await query.FirstOrDefaultAsync(cancellationToken);

			if (telemetrySnapshot == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Telemetry.TelemetrySnapshot DELETE", id.ToString(), new Exception("No Telemetry.TelemetrySnapshot entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.TelemetrySnapshot cloneOfExisting = (Database.TelemetrySnapshot)_context.Entry(telemetrySnapshot).GetDatabaseValues().ToObject();


			try
			{
				_context.TelemetrySnapshots.Remove(telemetrySnapshot);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Telemetry.TelemetrySnapshot entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetrySnapshot.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetrySnapshot.CreateAnonymousWithFirstLevelSubObjects(telemetrySnapshot)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Telemetry.TelemetrySnapshot entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetrySnapshot.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetrySnapshot.CreateAnonymousWithFirstLevelSubObjects(telemetrySnapshot)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of TelemetrySnapshot records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/TelemetrySnapshots/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? telemetryApplicationId = null,
			int? telemetryCollectionRunId = null,
			DateTime? collectedAt = null,
			bool? isOnline = null,
			long? uptimeSeconds = null,
			double? memoryWorkingSetMB = null,
			double? memoryGcHeapMB = null,
			double? memoryPercent = null,
			double? systemMemoryPercent = null,
			double? cpuPercent = null,
			double? systemCpuPercent = null,
			int? threadPoolWorkerThreads = null,
			int? threadPoolCompletionPortThreads = null,
			int? threadPoolPendingWorkItems = null,
			string machineName = null,
			string dotNetVersion = null,
			string statusJson = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);

			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);

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
			if (collectedAt.HasValue == true && collectedAt.Value.Kind != DateTimeKind.Utc)
			{
				collectedAt = collectedAt.Value.ToUniversalTime();
			}

			IQueryable<Database.TelemetrySnapshot> query = (from ts in _context.TelemetrySnapshots select ts);
			if (telemetryApplicationId.HasValue == true)
			{
				query = query.Where(ts => ts.telemetryApplicationId == telemetryApplicationId.Value);
			}
			if (telemetryCollectionRunId.HasValue == true)
			{
				query = query.Where(ts => ts.telemetryCollectionRunId == telemetryCollectionRunId.Value);
			}
			if (collectedAt.HasValue == true)
			{
				query = query.Where(ts => ts.collectedAt == collectedAt.Value);
			}
			if (isOnline.HasValue == true)
			{
				query = query.Where(ts => ts.isOnline == isOnline.Value);
			}
			if (uptimeSeconds.HasValue == true)
			{
				query = query.Where(ts => ts.uptimeSeconds == uptimeSeconds.Value);
			}
			if (threadPoolWorkerThreads.HasValue == true)
			{
				query = query.Where(ts => ts.threadPoolWorkerThreads == threadPoolWorkerThreads.Value);
			}
			if (threadPoolCompletionPortThreads.HasValue == true)
			{
				query = query.Where(ts => ts.threadPoolCompletionPortThreads == threadPoolCompletionPortThreads.Value);
			}
			if (threadPoolPendingWorkItems.HasValue == true)
			{
				query = query.Where(ts => ts.threadPoolPendingWorkItems == threadPoolPendingWorkItems.Value);
			}
			if (string.IsNullOrEmpty(machineName) == false)
			{
				query = query.Where(ts => ts.machineName == machineName);
			}
			if (string.IsNullOrEmpty(dotNetVersion) == false)
			{
				query = query.Where(ts => ts.dotNetVersion == dotNetVersion);
			}
			if (string.IsNullOrEmpty(statusJson) == false)
			{
				query = query.Where(ts => ts.statusJson == statusJson);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Telemetry Snapshot, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.machineName.Contains(anyStringContains)
			       || x.dotNetVersion.Contains(anyStringContains)
			       || x.statusJson.Contains(anyStringContains)
			       || x.telemetryApplication.name.Contains(anyStringContains)
			       || x.telemetryApplication.url.Contains(anyStringContains)
			       || x.telemetryCollectionRun.errorMessage.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.machineName).ThenBy(x => x.dotNetVersion);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.TelemetrySnapshot.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/TelemetrySnapshot/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
			{
			   return Forbid();
			}

		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
