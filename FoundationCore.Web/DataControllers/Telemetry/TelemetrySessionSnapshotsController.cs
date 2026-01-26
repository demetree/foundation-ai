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
using Foundation.Telemetry.Telemetry.Database;

namespace Foundation.Telemetry.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the TelemetrySessionSnapshot entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the TelemetrySessionSnapshot entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class TelemetrySessionSnapshotsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private TelemetryContext _context;

		private ILogger<TelemetrySessionSnapshotsController> _logger;

		public TelemetrySessionSnapshotsController(TelemetryContext context, ILogger<TelemetrySessionSnapshotsController> logger) : base("Telemetry", "TelemetrySessionSnapshot")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of TelemetrySessionSnapshots filtered by the parameters provided.
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
		[Route("api/TelemetrySessionSnapshots")]
		public async Task<IActionResult> GetTelemetrySessionSnapshots(
			int? telemetrySnapshotId = null,
			int? activeSessionCount = null,
			int? expiredSessionCount = null,
			DateTime? oldestSessionStart = null,
			DateTime? newestSessionStart = null,
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
			if (oldestSessionStart.HasValue == true && oldestSessionStart.Value.Kind != DateTimeKind.Utc)
			{
				oldestSessionStart = oldestSessionStart.Value.ToUniversalTime();
			}

			if (newestSessionStart.HasValue == true && newestSessionStart.Value.Kind != DateTimeKind.Utc)
			{
				newestSessionStart = newestSessionStart.Value.ToUniversalTime();
			}

			IQueryable<Telemetry.Database.TelemetrySessionSnapshot> query = (from tss in _context.TelemetrySessionSnapshots select tss);
			if (telemetrySnapshotId.HasValue == true)
			{
				query = query.Where(tss => tss.telemetrySnapshotId == telemetrySnapshotId.Value);
			}
			if (activeSessionCount.HasValue == true)
			{
				query = query.Where(tss => tss.activeSessionCount == activeSessionCount.Value);
			}
			if (expiredSessionCount.HasValue == true)
			{
				query = query.Where(tss => tss.expiredSessionCount == expiredSessionCount.Value);
			}
			if (oldestSessionStart.HasValue == true)
			{
				query = query.Where(tss => tss.oldestSessionStart == oldestSessionStart.Value);
			}
			if (newestSessionStart.HasValue == true)
			{
				query = query.Where(tss => tss.newestSessionStart == newestSessionStart.Value);
			}

			query = query.OrderBy(tss => tss.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.telemetrySnapshot);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Telemetry Session Snapshot, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       (includeRelations == true && x.telemetrySnapshot.machineName.Contains(anyStringContains))
			       || (includeRelations == true && x.telemetrySnapshot.dotNetVersion.Contains(anyStringContains))
			       || (includeRelations == true && x.telemetrySnapshot.statusJson.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Telemetry.Database.TelemetrySessionSnapshot> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Telemetry.Database.TelemetrySessionSnapshot telemetrySessionSnapshot in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(telemetrySessionSnapshot, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Telemetry.TelemetrySessionSnapshot Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Telemetry.TelemetrySessionSnapshot Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of TelemetrySessionSnapshots filtered by the parameters provided.  Its query is similar to the GetTelemetrySessionSnapshots method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetrySessionSnapshots/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? telemetrySnapshotId = null,
			int? activeSessionCount = null,
			int? expiredSessionCount = null,
			DateTime? oldestSessionStart = null,
			DateTime? newestSessionStart = null,
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
			if (oldestSessionStart.HasValue == true && oldestSessionStart.Value.Kind != DateTimeKind.Utc)
			{
				oldestSessionStart = oldestSessionStart.Value.ToUniversalTime();
			}

			if (newestSessionStart.HasValue == true && newestSessionStart.Value.Kind != DateTimeKind.Utc)
			{
				newestSessionStart = newestSessionStart.Value.ToUniversalTime();
			}

			IQueryable<Telemetry.Database.TelemetrySessionSnapshot> query = (from tss in _context.TelemetrySessionSnapshots select tss);
			if (telemetrySnapshotId.HasValue == true)
			{
				query = query.Where(tss => tss.telemetrySnapshotId == telemetrySnapshotId.Value);
			}
			if (activeSessionCount.HasValue == true)
			{
				query = query.Where(tss => tss.activeSessionCount == activeSessionCount.Value);
			}
			if (expiredSessionCount.HasValue == true)
			{
				query = query.Where(tss => tss.expiredSessionCount == expiredSessionCount.Value);
			}
			if (oldestSessionStart.HasValue == true)
			{
				query = query.Where(tss => tss.oldestSessionStart == oldestSessionStart.Value);
			}
			if (newestSessionStart.HasValue == true)
			{
				query = query.Where(tss => tss.newestSessionStart == newestSessionStart.Value);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Telemetry Session Snapshot, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.telemetrySnapshot.machineName.Contains(anyStringContains)
			       || x.telemetrySnapshot.dotNetVersion.Contains(anyStringContains)
			       || x.telemetrySnapshot.statusJson.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single TelemetrySessionSnapshot by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetrySessionSnapshot/{id}")]
		public async Task<IActionResult> GetTelemetrySessionSnapshot(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Telemetry.Database.TelemetrySessionSnapshot> query = (from tss in _context.TelemetrySessionSnapshots where
				(tss.id == id)
					select tss);

				if (includeRelations == true)
				{
					query = query.Include(x => x.telemetrySnapshot);
					query = query.AsSplitQuery();
				}

				Telemetry.Database.TelemetrySessionSnapshot materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Telemetry.TelemetrySessionSnapshot Entity was read with Admin privilege." : "Telemetry.TelemetrySessionSnapshot Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TelemetrySessionSnapshot", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Telemetry.TelemetrySessionSnapshot entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Telemetry.TelemetrySessionSnapshot.   Entity was read with Admin privilege." : "Exception caught during entity read of Telemetry.TelemetrySessionSnapshot.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing TelemetrySessionSnapshot record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/TelemetrySessionSnapshot/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutTelemetrySessionSnapshot(int id, [FromBody]Telemetry.Database.TelemetrySessionSnapshot.TelemetrySessionSnapshotDTO telemetrySessionSnapshotDTO, CancellationToken cancellationToken = default)
		{
			if (telemetrySessionSnapshotDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			// Admin privilege needed to write to this table.
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != telemetrySessionSnapshotDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Telemetry.Database.TelemetrySessionSnapshot> query = (from x in _context.TelemetrySessionSnapshots
				where
				(x.id == id)
				select x);


			Telemetry.Database.TelemetrySessionSnapshot existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Telemetry.TelemetrySessionSnapshot PUT", id.ToString(), new Exception("No Telemetry.TelemetrySessionSnapshot entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Telemetry.Database.TelemetrySessionSnapshot cloneOfExisting = (Telemetry.Database.TelemetrySessionSnapshot)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new TelemetrySessionSnapshot object using the data from the existing record, updated with what is in the DTO.
			//
			Telemetry.Database.TelemetrySessionSnapshot telemetrySessionSnapshot = (Telemetry.Database.TelemetrySessionSnapshot)_context.Entry(existing).GetDatabaseValues().ToObject();
			telemetrySessionSnapshot.ApplyDTO(telemetrySessionSnapshotDTO);


			if (telemetrySessionSnapshot.oldestSessionStart.HasValue == true && telemetrySessionSnapshot.oldestSessionStart.Value.Kind != DateTimeKind.Utc)
			{
				telemetrySessionSnapshot.oldestSessionStart = telemetrySessionSnapshot.oldestSessionStart.Value.ToUniversalTime();
			}

			if (telemetrySessionSnapshot.newestSessionStart.HasValue == true && telemetrySessionSnapshot.newestSessionStart.Value.Kind != DateTimeKind.Utc)
			{
				telemetrySessionSnapshot.newestSessionStart = telemetrySessionSnapshot.newestSessionStart.Value.ToUniversalTime();
			}

			EntityEntry<Telemetry.Database.TelemetrySessionSnapshot> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(telemetrySessionSnapshot);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Telemetry.TelemetrySessionSnapshot entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Telemetry.Database.TelemetrySessionSnapshot.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Telemetry.Database.TelemetrySessionSnapshot.CreateAnonymousWithFirstLevelSubObjects(telemetrySessionSnapshot)),
					null);


				return Ok(Telemetry.Database.TelemetrySessionSnapshot.CreateAnonymous(telemetrySessionSnapshot));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Telemetry.TelemetrySessionSnapshot entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Telemetry.Database.TelemetrySessionSnapshot.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Telemetry.Database.TelemetrySessionSnapshot.CreateAnonymousWithFirstLevelSubObjects(telemetrySessionSnapshot)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new TelemetrySessionSnapshot record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetrySessionSnapshot", Name = "TelemetrySessionSnapshot")]
		public async Task<IActionResult> PostTelemetrySessionSnapshot([FromBody]Telemetry.Database.TelemetrySessionSnapshot.TelemetrySessionSnapshotDTO telemetrySessionSnapshotDTO, CancellationToken cancellationToken = default)
		{
			if (telemetrySessionSnapshotDTO == null)
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
			// Create a new TelemetrySessionSnapshot object using the data from the DTO
			//
			Telemetry.Database.TelemetrySessionSnapshot telemetrySessionSnapshot = Telemetry.Database.TelemetrySessionSnapshot.FromDTO(telemetrySessionSnapshotDTO);

			try
			{
				if (telemetrySessionSnapshot.oldestSessionStart.HasValue == true && telemetrySessionSnapshot.oldestSessionStart.Value.Kind != DateTimeKind.Utc)
				{
					telemetrySessionSnapshot.oldestSessionStart = telemetrySessionSnapshot.oldestSessionStart.Value.ToUniversalTime();
				}

				if (telemetrySessionSnapshot.newestSessionStart.HasValue == true && telemetrySessionSnapshot.newestSessionStart.Value.Kind != DateTimeKind.Utc)
				{
					telemetrySessionSnapshot.newestSessionStart = telemetrySessionSnapshot.newestSessionStart.Value.ToUniversalTime();
				}

				_context.TelemetrySessionSnapshots.Add(telemetrySessionSnapshot);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Telemetry.TelemetrySessionSnapshot entity successfully created.",
					true,
					telemetrySessionSnapshot.id.ToString(),
					"",
					JsonSerializer.Serialize(Telemetry.Database.TelemetrySessionSnapshot.CreateAnonymousWithFirstLevelSubObjects(telemetrySessionSnapshot)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Telemetry.TelemetrySessionSnapshot entity creation failed.", false, telemetrySessionSnapshot.id.ToString(), "", JsonSerializer.Serialize(telemetrySessionSnapshot), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TelemetrySessionSnapshot", telemetrySessionSnapshot.id, telemetrySessionSnapshot.id.ToString()));

			return CreatedAtRoute("TelemetrySessionSnapshot", new { id = telemetrySessionSnapshot.id }, Telemetry.Database.TelemetrySessionSnapshot.CreateAnonymousWithFirstLevelSubObjects(telemetrySessionSnapshot));
		}



        /// <summary>
        /// 
        /// This deletes a TelemetrySessionSnapshot record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetrySessionSnapshot/{id}")]
		[Route("api/TelemetrySessionSnapshot")]
		public async Task<IActionResult> DeleteTelemetrySessionSnapshot(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Telemetry.Database.TelemetrySessionSnapshot> query = (from x in _context.TelemetrySessionSnapshots
				where
				(x.id == id)
				select x);


			Telemetry.Database.TelemetrySessionSnapshot telemetrySessionSnapshot = await query.FirstOrDefaultAsync(cancellationToken);

			if (telemetrySessionSnapshot == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Telemetry.TelemetrySessionSnapshot DELETE", id.ToString(), new Exception("No Telemetry.TelemetrySessionSnapshot entity could be find with the primary key provided."));
				return NotFound();
			}
			Telemetry.Database.TelemetrySessionSnapshot cloneOfExisting = (Telemetry.Database.TelemetrySessionSnapshot)_context.Entry(telemetrySessionSnapshot).GetDatabaseValues().ToObject();


			try
			{
				_context.TelemetrySessionSnapshots.Remove(telemetrySessionSnapshot);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Telemetry.TelemetrySessionSnapshot entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Telemetry.Database.TelemetrySessionSnapshot.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Telemetry.Database.TelemetrySessionSnapshot.CreateAnonymousWithFirstLevelSubObjects(telemetrySessionSnapshot)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Telemetry.TelemetrySessionSnapshot entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Telemetry.Database.TelemetrySessionSnapshot.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Telemetry.Database.TelemetrySessionSnapshot.CreateAnonymousWithFirstLevelSubObjects(telemetrySessionSnapshot)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of TelemetrySessionSnapshot records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/TelemetrySessionSnapshots/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? telemetrySnapshotId = null,
			int? activeSessionCount = null,
			int? expiredSessionCount = null,
			DateTime? oldestSessionStart = null,
			DateTime? newestSessionStart = null,
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
			if (oldestSessionStart.HasValue == true && oldestSessionStart.Value.Kind != DateTimeKind.Utc)
			{
				oldestSessionStart = oldestSessionStart.Value.ToUniversalTime();
			}

			if (newestSessionStart.HasValue == true && newestSessionStart.Value.Kind != DateTimeKind.Utc)
			{
				newestSessionStart = newestSessionStart.Value.ToUniversalTime();
			}

			IQueryable<Telemetry.Database.TelemetrySessionSnapshot> query = (from tss in _context.TelemetrySessionSnapshots select tss);
			if (telemetrySnapshotId.HasValue == true)
			{
				query = query.Where(tss => tss.telemetrySnapshotId == telemetrySnapshotId.Value);
			}
			if (activeSessionCount.HasValue == true)
			{
				query = query.Where(tss => tss.activeSessionCount == activeSessionCount.Value);
			}
			if (expiredSessionCount.HasValue == true)
			{
				query = query.Where(tss => tss.expiredSessionCount == expiredSessionCount.Value);
			}
			if (oldestSessionStart.HasValue == true)
			{
				query = query.Where(tss => tss.oldestSessionStart == oldestSessionStart.Value);
			}
			if (newestSessionStart.HasValue == true)
			{
				query = query.Where(tss => tss.newestSessionStart == newestSessionStart.Value);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Telemetry Session Snapshot, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.telemetrySnapshot.machineName.Contains(anyStringContains)
			       || x.telemetrySnapshot.dotNetVersion.Contains(anyStringContains)
			       || x.telemetrySnapshot.statusJson.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Telemetry.Database.TelemetrySessionSnapshot.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/TelemetrySessionSnapshot/CreateAuditEvent")]
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
