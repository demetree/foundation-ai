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
    /// This auto generated class provides the basic CRUD operations for the TelemetryDiskHealth entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the TelemetryDiskHealth entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class TelemetryDiskHealthsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private TelemetryContext _context;

		private ILogger<TelemetryDiskHealthsController> _logger;

		public TelemetryDiskHealthsController(TelemetryContext context, ILogger<TelemetryDiskHealthsController> logger) : base("Telemetry", "TelemetryDiskHealth")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of TelemetryDiskHealths filtered by the parameters provided.
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
		[Route("api/TelemetryDiskHealths")]
		public async Task<IActionResult> GetTelemetryDiskHealths(
			int? telemetrySnapshotId = null,
			string driveName = null,
			string driveLabel = null,
			double? totalGB = null,
			double? freeGB = null,
			double? freePercent = null,
			double? usedPercent = null,
			string status = null,
			bool? isApplicationDrive = null,
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

			IQueryable<Database.TelemetryDiskHealth> query = (from tdh in _context.TelemetryDiskHealths select tdh);
			if (telemetrySnapshotId.HasValue == true)
			{
				query = query.Where(tdh => tdh.telemetrySnapshotId == telemetrySnapshotId.Value);
			}
			if (string.IsNullOrEmpty(driveName) == false)
			{
				query = query.Where(tdh => tdh.driveName == driveName);
			}
			if (string.IsNullOrEmpty(driveLabel) == false)
			{
				query = query.Where(tdh => tdh.driveLabel == driveLabel);
			}
			if (totalGB.HasValue == true)
			{
				query = query.Where(tdh => tdh.totalGB == totalGB.Value);
			}
			if (freeGB.HasValue == true)
			{
				query = query.Where(tdh => tdh.freeGB == freeGB.Value);
			}
			if (freePercent.HasValue == true)
			{
				query = query.Where(tdh => tdh.freePercent == freePercent.Value);
			}
			if (usedPercent.HasValue == true)
			{
				query = query.Where(tdh => tdh.usedPercent == usedPercent.Value);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(tdh => tdh.status == status);
			}
			if (isApplicationDrive.HasValue == true)
			{
				query = query.Where(tdh => tdh.isApplicationDrive == isApplicationDrive.Value);
			}

			query = query.OrderBy(tdh => tdh.driveName).ThenBy(tdh => tdh.driveLabel).ThenBy(tdh => tdh.status);

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
			// Add the any string contains parameter to span all the string fields on the Telemetry Disk Health, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.driveName.Contains(anyStringContains)
			       || x.driveLabel.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || (includeRelations == true && x.telemetrySnapshot.machineName.Contains(anyStringContains))
			       || (includeRelations == true && x.telemetrySnapshot.dotNetVersion.Contains(anyStringContains))
			       || (includeRelations == true && x.telemetrySnapshot.statusJson.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.TelemetryDiskHealth> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.TelemetryDiskHealth telemetryDiskHealth in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(telemetryDiskHealth, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Telemetry.TelemetryDiskHealth Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Telemetry.TelemetryDiskHealth Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of TelemetryDiskHealths filtered by the parameters provided.  Its query is similar to the GetTelemetryDiskHealths method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryDiskHealths/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? telemetrySnapshotId = null,
			string driveName = null,
			string driveLabel = null,
			double? totalGB = null,
			double? freeGB = null,
			double? freePercent = null,
			double? usedPercent = null,
			string status = null,
			bool? isApplicationDrive = null,
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

			IQueryable<Database.TelemetryDiskHealth> query = (from tdh in _context.TelemetryDiskHealths select tdh);
			if (telemetrySnapshotId.HasValue == true)
			{
				query = query.Where(tdh => tdh.telemetrySnapshotId == telemetrySnapshotId.Value);
			}
			if (driveName != null)
			{
				query = query.Where(tdh => tdh.driveName == driveName);
			}
			if (driveLabel != null)
			{
				query = query.Where(tdh => tdh.driveLabel == driveLabel);
			}
			if (totalGB.HasValue == true)
			{
				query = query.Where(tdh => tdh.totalGB == totalGB.Value);
			}
			if (freeGB.HasValue == true)
			{
				query = query.Where(tdh => tdh.freeGB == freeGB.Value);
			}
			if (freePercent.HasValue == true)
			{
				query = query.Where(tdh => tdh.freePercent == freePercent.Value);
			}
			if (usedPercent.HasValue == true)
			{
				query = query.Where(tdh => tdh.usedPercent == usedPercent.Value);
			}
			if (status != null)
			{
				query = query.Where(tdh => tdh.status == status);
			}
			if (isApplicationDrive.HasValue == true)
			{
				query = query.Where(tdh => tdh.isApplicationDrive == isApplicationDrive.Value);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Telemetry Disk Health, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.driveName.Contains(anyStringContains)
			       || x.driveLabel.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || x.telemetrySnapshot.machineName.Contains(anyStringContains)
			       || x.telemetrySnapshot.dotNetVersion.Contains(anyStringContains)
			       || x.telemetrySnapshot.statusJson.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single TelemetryDiskHealth by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryDiskHealth/{id}")]
		public async Task<IActionResult> GetTelemetryDiskHealth(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.TelemetryDiskHealth> query = (from tdh in _context.TelemetryDiskHealths where
				(tdh.id == id)
					select tdh);

				if (includeRelations == true)
				{
					query = query.Include(x => x.telemetrySnapshot);
					query = query.AsSplitQuery();
				}

				Database.TelemetryDiskHealth materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Telemetry.TelemetryDiskHealth Entity was read with Admin privilege." : "Telemetry.TelemetryDiskHealth Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TelemetryDiskHealth", materialized.id, materialized.driveName));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Telemetry.TelemetryDiskHealth entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Telemetry.TelemetryDiskHealth.   Entity was read with Admin privilege." : "Exception caught during entity read of Telemetry.TelemetryDiskHealth.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing TelemetryDiskHealth record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/TelemetryDiskHealth/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutTelemetryDiskHealth(int id, [FromBody]Database.TelemetryDiskHealth.TelemetryDiskHealthDTO telemetryDiskHealthDTO, CancellationToken cancellationToken = default)
		{
			if (telemetryDiskHealthDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			// Admin privilege needed to write to this table.
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != telemetryDiskHealthDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.TelemetryDiskHealth> query = (from x in _context.TelemetryDiskHealths
				where
				(x.id == id)
				select x);


			Database.TelemetryDiskHealth existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Telemetry.TelemetryDiskHealth PUT", id.ToString(), new Exception("No Telemetry.TelemetryDiskHealth entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.TelemetryDiskHealth cloneOfExisting = (Database.TelemetryDiskHealth)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new TelemetryDiskHealth object using the data from the existing record, updated with what is in the DTO.
			//
			Database.TelemetryDiskHealth telemetryDiskHealth = (Database.TelemetryDiskHealth)_context.Entry(existing).GetDatabaseValues().ToObject();
			telemetryDiskHealth.ApplyDTO(telemetryDiskHealthDTO);


			if (telemetryDiskHealth.driveName != null && telemetryDiskHealth.driveName.Length > 10)
			{
				telemetryDiskHealth.driveName = telemetryDiskHealth.driveName.Substring(0, 10);
			}

			if (telemetryDiskHealth.driveLabel != null && telemetryDiskHealth.driveLabel.Length > 100)
			{
				telemetryDiskHealth.driveLabel = telemetryDiskHealth.driveLabel.Substring(0, 100);
			}

			if (telemetryDiskHealth.status != null && telemetryDiskHealth.status.Length > 50)
			{
				telemetryDiskHealth.status = telemetryDiskHealth.status.Substring(0, 50);
			}

			EntityEntry<Database.TelemetryDiskHealth> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(telemetryDiskHealth);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Telemetry.TelemetryDiskHealth entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryDiskHealth.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryDiskHealth.CreateAnonymousWithFirstLevelSubObjects(telemetryDiskHealth)),
					null);


				return Ok(Database.TelemetryDiskHealth.CreateAnonymous(telemetryDiskHealth));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Telemetry.TelemetryDiskHealth entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryDiskHealth.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryDiskHealth.CreateAnonymousWithFirstLevelSubObjects(telemetryDiskHealth)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new TelemetryDiskHealth record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryDiskHealth", Name = "TelemetryDiskHealth")]
		public async Task<IActionResult> PostTelemetryDiskHealth([FromBody]Database.TelemetryDiskHealth.TelemetryDiskHealthDTO telemetryDiskHealthDTO, CancellationToken cancellationToken = default)
		{
			if (telemetryDiskHealthDTO == null)
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
			// Create a new TelemetryDiskHealth object using the data from the DTO
			//
			Database.TelemetryDiskHealth telemetryDiskHealth = Database.TelemetryDiskHealth.FromDTO(telemetryDiskHealthDTO);

			try
			{
				if (telemetryDiskHealth.driveName != null && telemetryDiskHealth.driveName.Length > 10)
				{
					telemetryDiskHealth.driveName = telemetryDiskHealth.driveName.Substring(0, 10);
				}

				if (telemetryDiskHealth.driveLabel != null && telemetryDiskHealth.driveLabel.Length > 100)
				{
					telemetryDiskHealth.driveLabel = telemetryDiskHealth.driveLabel.Substring(0, 100);
				}

				if (telemetryDiskHealth.status != null && telemetryDiskHealth.status.Length > 50)
				{
					telemetryDiskHealth.status = telemetryDiskHealth.status.Substring(0, 50);
				}

				_context.TelemetryDiskHealths.Add(telemetryDiskHealth);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Telemetry.TelemetryDiskHealth entity successfully created.",
					true,
					telemetryDiskHealth.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.TelemetryDiskHealth.CreateAnonymousWithFirstLevelSubObjects(telemetryDiskHealth)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Telemetry.TelemetryDiskHealth entity creation failed.", false, telemetryDiskHealth.id.ToString(), "", JsonSerializer.Serialize(telemetryDiskHealth), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TelemetryDiskHealth", telemetryDiskHealth.id, telemetryDiskHealth.driveName));

			return CreatedAtRoute("TelemetryDiskHealth", new { id = telemetryDiskHealth.id }, Database.TelemetryDiskHealth.CreateAnonymousWithFirstLevelSubObjects(telemetryDiskHealth));
		}



        /// <summary>
        /// 
        /// This deletes a TelemetryDiskHealth record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryDiskHealth/{id}")]
		[Route("api/TelemetryDiskHealth")]
		public async Task<IActionResult> DeleteTelemetryDiskHealth(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.TelemetryDiskHealth> query = (from x in _context.TelemetryDiskHealths
				where
				(x.id == id)
				select x);


			Database.TelemetryDiskHealth telemetryDiskHealth = await query.FirstOrDefaultAsync(cancellationToken);

			if (telemetryDiskHealth == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Telemetry.TelemetryDiskHealth DELETE", id.ToString(), new Exception("No Telemetry.TelemetryDiskHealth entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.TelemetryDiskHealth cloneOfExisting = (Database.TelemetryDiskHealth)_context.Entry(telemetryDiskHealth).GetDatabaseValues().ToObject();


			try
			{
				_context.TelemetryDiskHealths.Remove(telemetryDiskHealth);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Telemetry.TelemetryDiskHealth entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryDiskHealth.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryDiskHealth.CreateAnonymousWithFirstLevelSubObjects(telemetryDiskHealth)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Telemetry.TelemetryDiskHealth entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryDiskHealth.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryDiskHealth.CreateAnonymousWithFirstLevelSubObjects(telemetryDiskHealth)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of TelemetryDiskHealth records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/TelemetryDiskHealths/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? telemetrySnapshotId = null,
			string driveName = null,
			string driveLabel = null,
			double? totalGB = null,
			double? freeGB = null,
			double? freePercent = null,
			double? usedPercent = null,
			string status = null,
			bool? isApplicationDrive = null,
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

			IQueryable<Database.TelemetryDiskHealth> query = (from tdh in _context.TelemetryDiskHealths select tdh);
			if (telemetrySnapshotId.HasValue == true)
			{
				query = query.Where(tdh => tdh.telemetrySnapshotId == telemetrySnapshotId.Value);
			}
			if (string.IsNullOrEmpty(driveName) == false)
			{
				query = query.Where(tdh => tdh.driveName == driveName);
			}
			if (string.IsNullOrEmpty(driveLabel) == false)
			{
				query = query.Where(tdh => tdh.driveLabel == driveLabel);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(tdh => tdh.status == status);
			}
			if (isApplicationDrive.HasValue == true)
			{
				query = query.Where(tdh => tdh.isApplicationDrive == isApplicationDrive.Value);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Telemetry Disk Health, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.driveName.Contains(anyStringContains)
			       || x.driveLabel.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || x.telemetrySnapshot.machineName.Contains(anyStringContains)
			       || x.telemetrySnapshot.dotNetVersion.Contains(anyStringContains)
			       || x.telemetrySnapshot.statusJson.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.driveName).ThenBy(x => x.driveLabel).ThenBy(x => x.status);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.TelemetryDiskHealth.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/TelemetryDiskHealth/CreateAuditEvent")]
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
