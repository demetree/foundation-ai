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
    /// This auto generated class provides the basic CRUD operations for the TelemetryDatabaseHealth entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the TelemetryDatabaseHealth entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class TelemetryDatabaseHealthsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private TelemetryContext _context;

		private ILogger<TelemetryDatabaseHealthsController> _logger;

		public TelemetryDatabaseHealthsController(TelemetryContext context, ILogger<TelemetryDatabaseHealthsController> logger) : base("Telemetry", "TelemetryDatabaseHealth")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of TelemetryDatabaseHealths filtered by the parameters provided.
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
		[Route("api/TelemetryDatabaseHealths")]
		public async Task<IActionResult> GetTelemetryDatabaseHealths(
			int? telemetrySnapshotId = null,
			string databaseName = null,
			bool? isConnected = null,
			string status = null,
			string server = null,
			string provider = null,
			string errorMessage = null,
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

			IQueryable<Database.TelemetryDatabaseHealth> query = (from tdh in _context.TelemetryDatabaseHealths select tdh);
			if (telemetrySnapshotId.HasValue == true)
			{
				query = query.Where(tdh => tdh.telemetrySnapshotId == telemetrySnapshotId.Value);
			}
			if (string.IsNullOrEmpty(databaseName) == false)
			{
				query = query.Where(tdh => tdh.databaseName == databaseName);
			}
			if (isConnected.HasValue == true)
			{
				query = query.Where(tdh => tdh.isConnected == isConnected.Value);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(tdh => tdh.status == status);
			}
			if (string.IsNullOrEmpty(server) == false)
			{
				query = query.Where(tdh => tdh.server == server);
			}
			if (string.IsNullOrEmpty(provider) == false)
			{
				query = query.Where(tdh => tdh.provider == provider);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(tdh => tdh.errorMessage == errorMessage);
			}

			query = query.OrderBy(tdh => tdh.databaseName).ThenBy(tdh => tdh.status).ThenBy(tdh => tdh.server);

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
			// Add the any string contains parameter to span all the string fields on the Telemetry Database Health, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.databaseName.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || x.server.Contains(anyStringContains)
			       || x.provider.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			       || (includeRelations == true && x.telemetrySnapshot.machineName.Contains(anyStringContains))
			       || (includeRelations == true && x.telemetrySnapshot.dotNetVersion.Contains(anyStringContains))
			       || (includeRelations == true && x.telemetrySnapshot.statusJson.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.TelemetryDatabaseHealth> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.TelemetryDatabaseHealth telemetryDatabaseHealth in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(telemetryDatabaseHealth, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Telemetry.TelemetryDatabaseHealth Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Telemetry.TelemetryDatabaseHealth Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of TelemetryDatabaseHealths filtered by the parameters provided.  Its query is similar to the GetTelemetryDatabaseHealths method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryDatabaseHealths/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? telemetrySnapshotId = null,
			string databaseName = null,
			bool? isConnected = null,
			string status = null,
			string server = null,
			string provider = null,
			string errorMessage = null,
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

			IQueryable<Database.TelemetryDatabaseHealth> query = (from tdh in _context.TelemetryDatabaseHealths select tdh);
			if (telemetrySnapshotId.HasValue == true)
			{
				query = query.Where(tdh => tdh.telemetrySnapshotId == telemetrySnapshotId.Value);
			}
			if (databaseName != null)
			{
				query = query.Where(tdh => tdh.databaseName == databaseName);
			}
			if (isConnected.HasValue == true)
			{
				query = query.Where(tdh => tdh.isConnected == isConnected.Value);
			}
			if (status != null)
			{
				query = query.Where(tdh => tdh.status == status);
			}
			if (server != null)
			{
				query = query.Where(tdh => tdh.server == server);
			}
			if (provider != null)
			{
				query = query.Where(tdh => tdh.provider == provider);
			}
			if (errorMessage != null)
			{
				query = query.Where(tdh => tdh.errorMessage == errorMessage);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Telemetry Database Health, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.databaseName.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || x.server.Contains(anyStringContains)
			       || x.provider.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
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
        /// This gets a single TelemetryDatabaseHealth by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryDatabaseHealth/{id}")]
		public async Task<IActionResult> GetTelemetryDatabaseHealth(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.TelemetryDatabaseHealth> query = (from tdh in _context.TelemetryDatabaseHealths where
				(tdh.id == id)
					select tdh);

				if (includeRelations == true)
				{
					query = query.Include(x => x.telemetrySnapshot);
					query = query.AsSplitQuery();
				}

				Database.TelemetryDatabaseHealth materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Telemetry.TelemetryDatabaseHealth Entity was read with Admin privilege." : "Telemetry.TelemetryDatabaseHealth Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TelemetryDatabaseHealth", materialized.id, materialized.databaseName));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Telemetry.TelemetryDatabaseHealth entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Telemetry.TelemetryDatabaseHealth.   Entity was read with Admin privilege." : "Exception caught during entity read of Telemetry.TelemetryDatabaseHealth.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing TelemetryDatabaseHealth record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/TelemetryDatabaseHealth/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutTelemetryDatabaseHealth(int id, [FromBody]Database.TelemetryDatabaseHealth.TelemetryDatabaseHealthDTO telemetryDatabaseHealthDTO, CancellationToken cancellationToken = default)
		{
			if (telemetryDatabaseHealthDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			// Admin privilege needed to write to this table.
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != telemetryDatabaseHealthDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.TelemetryDatabaseHealth> query = (from x in _context.TelemetryDatabaseHealths
				where
				(x.id == id)
				select x);


			Database.TelemetryDatabaseHealth existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Telemetry.TelemetryDatabaseHealth PUT", id.ToString(), new Exception("No Telemetry.TelemetryDatabaseHealth entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.TelemetryDatabaseHealth cloneOfExisting = (Database.TelemetryDatabaseHealth)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new TelemetryDatabaseHealth object using the data from the existing record, updated with what is in the DTO.
			//
			Database.TelemetryDatabaseHealth telemetryDatabaseHealth = (Database.TelemetryDatabaseHealth)_context.Entry(existing).GetDatabaseValues().ToObject();
			telemetryDatabaseHealth.ApplyDTO(telemetryDatabaseHealthDTO);


			if (telemetryDatabaseHealth.databaseName != null && telemetryDatabaseHealth.databaseName.Length > 100)
			{
				telemetryDatabaseHealth.databaseName = telemetryDatabaseHealth.databaseName.Substring(0, 100);
			}

			if (telemetryDatabaseHealth.status != null && telemetryDatabaseHealth.status.Length > 50)
			{
				telemetryDatabaseHealth.status = telemetryDatabaseHealth.status.Substring(0, 50);
			}

			if (telemetryDatabaseHealth.server != null && telemetryDatabaseHealth.server.Length > 250)
			{
				telemetryDatabaseHealth.server = telemetryDatabaseHealth.server.Substring(0, 250);
			}

			if (telemetryDatabaseHealth.provider != null && telemetryDatabaseHealth.provider.Length > 100)
			{
				telemetryDatabaseHealth.provider = telemetryDatabaseHealth.provider.Substring(0, 100);
			}

			EntityEntry<Database.TelemetryDatabaseHealth> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(telemetryDatabaseHealth);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Telemetry.TelemetryDatabaseHealth entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryDatabaseHealth.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryDatabaseHealth.CreateAnonymousWithFirstLevelSubObjects(telemetryDatabaseHealth)),
					null);


				return Ok(Database.TelemetryDatabaseHealth.CreateAnonymous(telemetryDatabaseHealth));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Telemetry.TelemetryDatabaseHealth entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryDatabaseHealth.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryDatabaseHealth.CreateAnonymousWithFirstLevelSubObjects(telemetryDatabaseHealth)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new TelemetryDatabaseHealth record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryDatabaseHealth", Name = "TelemetryDatabaseHealth")]
		public async Task<IActionResult> PostTelemetryDatabaseHealth([FromBody]Database.TelemetryDatabaseHealth.TelemetryDatabaseHealthDTO telemetryDatabaseHealthDTO, CancellationToken cancellationToken = default)
		{
			if (telemetryDatabaseHealthDTO == null)
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
			// Create a new TelemetryDatabaseHealth object using the data from the DTO
			//
			Database.TelemetryDatabaseHealth telemetryDatabaseHealth = Database.TelemetryDatabaseHealth.FromDTO(telemetryDatabaseHealthDTO);

			try
			{
				if (telemetryDatabaseHealth.databaseName != null && telemetryDatabaseHealth.databaseName.Length > 100)
				{
					telemetryDatabaseHealth.databaseName = telemetryDatabaseHealth.databaseName.Substring(0, 100);
				}

				if (telemetryDatabaseHealth.status != null && telemetryDatabaseHealth.status.Length > 50)
				{
					telemetryDatabaseHealth.status = telemetryDatabaseHealth.status.Substring(0, 50);
				}

				if (telemetryDatabaseHealth.server != null && telemetryDatabaseHealth.server.Length > 250)
				{
					telemetryDatabaseHealth.server = telemetryDatabaseHealth.server.Substring(0, 250);
				}

				if (telemetryDatabaseHealth.provider != null && telemetryDatabaseHealth.provider.Length > 100)
				{
					telemetryDatabaseHealth.provider = telemetryDatabaseHealth.provider.Substring(0, 100);
				}

				_context.TelemetryDatabaseHealths.Add(telemetryDatabaseHealth);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Telemetry.TelemetryDatabaseHealth entity successfully created.",
					true,
					telemetryDatabaseHealth.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.TelemetryDatabaseHealth.CreateAnonymousWithFirstLevelSubObjects(telemetryDatabaseHealth)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Telemetry.TelemetryDatabaseHealth entity creation failed.", false, telemetryDatabaseHealth.id.ToString(), "", JsonSerializer.Serialize(telemetryDatabaseHealth), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TelemetryDatabaseHealth", telemetryDatabaseHealth.id, telemetryDatabaseHealth.databaseName));

			return CreatedAtRoute("TelemetryDatabaseHealth", new { id = telemetryDatabaseHealth.id }, Database.TelemetryDatabaseHealth.CreateAnonymousWithFirstLevelSubObjects(telemetryDatabaseHealth));
		}



        /// <summary>
        /// 
        /// This deletes a TelemetryDatabaseHealth record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryDatabaseHealth/{id}")]
		[Route("api/TelemetryDatabaseHealth")]
		public async Task<IActionResult> DeleteTelemetryDatabaseHealth(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.TelemetryDatabaseHealth> query = (from x in _context.TelemetryDatabaseHealths
				where
				(x.id == id)
				select x);


			Database.TelemetryDatabaseHealth telemetryDatabaseHealth = await query.FirstOrDefaultAsync(cancellationToken);

			if (telemetryDatabaseHealth == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Telemetry.TelemetryDatabaseHealth DELETE", id.ToString(), new Exception("No Telemetry.TelemetryDatabaseHealth entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.TelemetryDatabaseHealth cloneOfExisting = (Database.TelemetryDatabaseHealth)_context.Entry(telemetryDatabaseHealth).GetDatabaseValues().ToObject();


			try
			{
				_context.TelemetryDatabaseHealths.Remove(telemetryDatabaseHealth);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Telemetry.TelemetryDatabaseHealth entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryDatabaseHealth.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryDatabaseHealth.CreateAnonymousWithFirstLevelSubObjects(telemetryDatabaseHealth)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Telemetry.TelemetryDatabaseHealth entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryDatabaseHealth.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryDatabaseHealth.CreateAnonymousWithFirstLevelSubObjects(telemetryDatabaseHealth)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of TelemetryDatabaseHealth records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/TelemetryDatabaseHealths/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? telemetrySnapshotId = null,
			string databaseName = null,
			bool? isConnected = null,
			string status = null,
			string server = null,
			string provider = null,
			string errorMessage = null,
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

			IQueryable<Database.TelemetryDatabaseHealth> query = (from tdh in _context.TelemetryDatabaseHealths select tdh);
			if (telemetrySnapshotId.HasValue == true)
			{
				query = query.Where(tdh => tdh.telemetrySnapshotId == telemetrySnapshotId.Value);
			}
			if (string.IsNullOrEmpty(databaseName) == false)
			{
				query = query.Where(tdh => tdh.databaseName == databaseName);
			}
			if (isConnected.HasValue == true)
			{
				query = query.Where(tdh => tdh.isConnected == isConnected.Value);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(tdh => tdh.status == status);
			}
			if (string.IsNullOrEmpty(server) == false)
			{
				query = query.Where(tdh => tdh.server == server);
			}
			if (string.IsNullOrEmpty(provider) == false)
			{
				query = query.Where(tdh => tdh.provider == provider);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(tdh => tdh.errorMessage == errorMessage);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Telemetry Database Health, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.databaseName.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || x.server.Contains(anyStringContains)
			       || x.provider.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			       || x.telemetrySnapshot.machineName.Contains(anyStringContains)
			       || x.telemetrySnapshot.dotNetVersion.Contains(anyStringContains)
			       || x.telemetrySnapshot.statusJson.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.databaseName).ThenBy(x => x.status).ThenBy(x => x.server);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.TelemetryDatabaseHealth.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/TelemetryDatabaseHealth/CreateAuditEvent")]
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
