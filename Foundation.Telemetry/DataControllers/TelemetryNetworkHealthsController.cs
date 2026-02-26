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
    /// This auto generated class provides the basic CRUD operations for the TelemetryNetworkHealth entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the TelemetryNetworkHealth entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class TelemetryNetworkHealthsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private TelemetryContext _context;

		private ILogger<TelemetryNetworkHealthsController> _logger;

		public TelemetryNetworkHealthsController(TelemetryContext context, ILogger<TelemetryNetworkHealthsController> logger) : base("Telemetry", "TelemetryNetworkHealth")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of TelemetryNetworkHealths filtered by the parameters provided.
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
		[Route("api/TelemetryNetworkHealths")]
		public async Task<IActionResult> GetTelemetryNetworkHealths(
			int? telemetrySnapshotId = null,
			string interfaceName = null,
			string interfaceDescription = null,
			double? linkSpeedMbps = null,
			long? bytesSentTotal = null,
			long? bytesReceivedTotal = null,
			double? bytesSentPerSecond = null,
			double? bytesReceivedPerSecond = null,
			double? utilizationPercent = null,
			string status = null,
			bool? isActive = null,
			int? pageSize = null,
			int? pageNumber = null,
			string anyStringContains = null,
			bool includeRelations = true,
			CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Telemetry Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
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

			IQueryable<Database.TelemetryNetworkHealth> query = (from tnh in _context.TelemetryNetworkHealths select tnh);
			if (telemetrySnapshotId.HasValue == true)
			{
				query = query.Where(tnh => tnh.telemetrySnapshotId == telemetrySnapshotId.Value);
			}
			if (string.IsNullOrEmpty(interfaceName) == false)
			{
				query = query.Where(tnh => tnh.interfaceName == interfaceName);
			}
			if (string.IsNullOrEmpty(interfaceDescription) == false)
			{
				query = query.Where(tnh => tnh.interfaceDescription == interfaceDescription);
			}
			if (linkSpeedMbps.HasValue == true)
			{
				query = query.Where(tnh => tnh.linkSpeedMbps == linkSpeedMbps.Value);
			}
			if (bytesSentTotal.HasValue == true)
			{
				query = query.Where(tnh => tnh.bytesSentTotal == bytesSentTotal.Value);
			}
			if (bytesReceivedTotal.HasValue == true)
			{
				query = query.Where(tnh => tnh.bytesReceivedTotal == bytesReceivedTotal.Value);
			}
			if (bytesSentPerSecond.HasValue == true)
			{
				query = query.Where(tnh => tnh.bytesSentPerSecond == bytesSentPerSecond.Value);
			}
			if (bytesReceivedPerSecond.HasValue == true)
			{
				query = query.Where(tnh => tnh.bytesReceivedPerSecond == bytesReceivedPerSecond.Value);
			}
			if (utilizationPercent.HasValue == true)
			{
				query = query.Where(tnh => tnh.utilizationPercent == utilizationPercent.Value);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(tnh => tnh.status == status);
			}
			if (isActive.HasValue == true)
			{
				query = query.Where(tnh => tnh.isActive == isActive.Value);
			}

			query = query.OrderBy(tnh => tnh.interfaceName).ThenBy(tnh => tnh.interfaceDescription).ThenBy(tnh => tnh.status);


			//
			// Add the any string contains parameter to span all the string fields on the Telemetry Network Health, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.interfaceName.Contains(anyStringContains)
			       || x.interfaceDescription.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || (includeRelations == true && x.telemetrySnapshot.machineName.Contains(anyStringContains))
			       || (includeRelations == true && x.telemetrySnapshot.dotNetVersion.Contains(anyStringContains))
			       || (includeRelations == true && x.telemetrySnapshot.statusJson.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.telemetrySnapshot);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.TelemetryNetworkHealth> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.TelemetryNetworkHealth telemetryNetworkHealth in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(telemetryNetworkHealth, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Telemetry.TelemetryNetworkHealth Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Telemetry.TelemetryNetworkHealth Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of TelemetryNetworkHealths filtered by the parameters provided.  Its query is similar to the GetTelemetryNetworkHealths method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryNetworkHealths/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? telemetrySnapshotId = null,
			string interfaceName = null,
			string interfaceDescription = null,
			double? linkSpeedMbps = null,
			long? bytesSentTotal = null,
			long? bytesReceivedTotal = null,
			double? bytesSentPerSecond = null,
			double? bytesReceivedPerSecond = null,
			double? utilizationPercent = null,
			string status = null,
			bool? isActive = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Telemetry Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.TelemetryNetworkHealth> query = (from tnh in _context.TelemetryNetworkHealths select tnh);
			if (telemetrySnapshotId.HasValue == true)
			{
				query = query.Where(tnh => tnh.telemetrySnapshotId == telemetrySnapshotId.Value);
			}
			if (interfaceName != null)
			{
				query = query.Where(tnh => tnh.interfaceName == interfaceName);
			}
			if (interfaceDescription != null)
			{
				query = query.Where(tnh => tnh.interfaceDescription == interfaceDescription);
			}
			if (linkSpeedMbps.HasValue == true)
			{
				query = query.Where(tnh => tnh.linkSpeedMbps == linkSpeedMbps.Value);
			}
			if (bytesSentTotal.HasValue == true)
			{
				query = query.Where(tnh => tnh.bytesSentTotal == bytesSentTotal.Value);
			}
			if (bytesReceivedTotal.HasValue == true)
			{
				query = query.Where(tnh => tnh.bytesReceivedTotal == bytesReceivedTotal.Value);
			}
			if (bytesSentPerSecond.HasValue == true)
			{
				query = query.Where(tnh => tnh.bytesSentPerSecond == bytesSentPerSecond.Value);
			}
			if (bytesReceivedPerSecond.HasValue == true)
			{
				query = query.Where(tnh => tnh.bytesReceivedPerSecond == bytesReceivedPerSecond.Value);
			}
			if (utilizationPercent.HasValue == true)
			{
				query = query.Where(tnh => tnh.utilizationPercent == utilizationPercent.Value);
			}
			if (status != null)
			{
				query = query.Where(tnh => tnh.status == status);
			}
			if (isActive.HasValue == true)
			{
				query = query.Where(tnh => tnh.isActive == isActive.Value);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Telemetry Network Health, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.interfaceName.Contains(anyStringContains)
			       || x.interfaceDescription.Contains(anyStringContains)
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
        /// This gets a single TelemetryNetworkHealth by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryNetworkHealth/{id}")]
		public async Task<IActionResult> GetTelemetryNetworkHealth(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Telemetry Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.TelemetryNetworkHealth> query = (from tnh in _context.TelemetryNetworkHealths where
				(tnh.id == id)
					select tnh);

				if (includeRelations == true)
				{
					query = query.Include(x => x.telemetrySnapshot);
					query = query.AsSplitQuery();
				}

				Database.TelemetryNetworkHealth materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Telemetry.TelemetryNetworkHealth Entity was read with Admin privilege." : "Telemetry.TelemetryNetworkHealth Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TelemetryNetworkHealth", materialized.id, materialized.interfaceName));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Telemetry.TelemetryNetworkHealth entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Telemetry.TelemetryNetworkHealth.   Entity was read with Admin privilege." : "Exception caught during entity read of Telemetry.TelemetryNetworkHealth.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing TelemetryNetworkHealth record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/TelemetryNetworkHealth/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutTelemetryNetworkHealth(int id, [FromBody]Database.TelemetryNetworkHealth.TelemetryNetworkHealthDTO telemetryNetworkHealthDTO, CancellationToken cancellationToken = default)
		{
			if (telemetryNetworkHealthDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Telemetry Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != telemetryNetworkHealthDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.TelemetryNetworkHealth> query = (from x in _context.TelemetryNetworkHealths
				where
				(x.id == id)
				select x);


			Database.TelemetryNetworkHealth existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Telemetry.TelemetryNetworkHealth PUT", id.ToString(), new Exception("No Telemetry.TelemetryNetworkHealth entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.TelemetryNetworkHealth cloneOfExisting = (Database.TelemetryNetworkHealth)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new TelemetryNetworkHealth object using the data from the existing record, updated with what is in the DTO.
			//
			Database.TelemetryNetworkHealth telemetryNetworkHealth = (Database.TelemetryNetworkHealth)_context.Entry(existing).GetDatabaseValues().ToObject();
			telemetryNetworkHealth.ApplyDTO(telemetryNetworkHealthDTO);


			if (telemetryNetworkHealth.interfaceName != null && telemetryNetworkHealth.interfaceName.Length > 100)
			{
				telemetryNetworkHealth.interfaceName = telemetryNetworkHealth.interfaceName.Substring(0, 100);
			}

			if (telemetryNetworkHealth.interfaceDescription != null && telemetryNetworkHealth.interfaceDescription.Length > 250)
			{
				telemetryNetworkHealth.interfaceDescription = telemetryNetworkHealth.interfaceDescription.Substring(0, 250);
			}

			if (telemetryNetworkHealth.status != null && telemetryNetworkHealth.status.Length > 50)
			{
				telemetryNetworkHealth.status = telemetryNetworkHealth.status.Substring(0, 50);
			}

			EntityEntry<Database.TelemetryNetworkHealth> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(telemetryNetworkHealth);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Telemetry.TelemetryNetworkHealth entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryNetworkHealth.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryNetworkHealth.CreateAnonymousWithFirstLevelSubObjects(telemetryNetworkHealth)),
					null);


				return Ok(Database.TelemetryNetworkHealth.CreateAnonymous(telemetryNetworkHealth));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Telemetry.TelemetryNetworkHealth entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryNetworkHealth.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryNetworkHealth.CreateAnonymousWithFirstLevelSubObjects(telemetryNetworkHealth)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new TelemetryNetworkHealth record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryNetworkHealth", Name = "TelemetryNetworkHealth")]
		public async Task<IActionResult> PostTelemetryNetworkHealth([FromBody]Database.TelemetryNetworkHealth.TelemetryNetworkHealthDTO telemetryNetworkHealthDTO, CancellationToken cancellationToken = default)
		{
			if (telemetryNetworkHealthDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Telemetry Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new TelemetryNetworkHealth object using the data from the DTO
			//
			Database.TelemetryNetworkHealth telemetryNetworkHealth = Database.TelemetryNetworkHealth.FromDTO(telemetryNetworkHealthDTO);

			try
			{
				if (telemetryNetworkHealth.interfaceName != null && telemetryNetworkHealth.interfaceName.Length > 100)
				{
					telemetryNetworkHealth.interfaceName = telemetryNetworkHealth.interfaceName.Substring(0, 100);
				}

				if (telemetryNetworkHealth.interfaceDescription != null && telemetryNetworkHealth.interfaceDescription.Length > 250)
				{
					telemetryNetworkHealth.interfaceDescription = telemetryNetworkHealth.interfaceDescription.Substring(0, 250);
				}

				if (telemetryNetworkHealth.status != null && telemetryNetworkHealth.status.Length > 50)
				{
					telemetryNetworkHealth.status = telemetryNetworkHealth.status.Substring(0, 50);
				}

				_context.TelemetryNetworkHealths.Add(telemetryNetworkHealth);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Telemetry.TelemetryNetworkHealth entity successfully created.",
					true,
					telemetryNetworkHealth.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.TelemetryNetworkHealth.CreateAnonymousWithFirstLevelSubObjects(telemetryNetworkHealth)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Telemetry.TelemetryNetworkHealth entity creation failed.", false, telemetryNetworkHealth.id.ToString(), "", JsonSerializer.Serialize(telemetryNetworkHealth), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TelemetryNetworkHealth", telemetryNetworkHealth.id, telemetryNetworkHealth.interfaceName));

			return CreatedAtRoute("TelemetryNetworkHealth", new { id = telemetryNetworkHealth.id }, Database.TelemetryNetworkHealth.CreateAnonymousWithFirstLevelSubObjects(telemetryNetworkHealth));
		}



        /// <summary>
        /// 
        /// This deletes a TelemetryNetworkHealth record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryNetworkHealth/{id}")]
		[Route("api/TelemetryNetworkHealth")]
		public async Task<IActionResult> DeleteTelemetryNetworkHealth(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Telemetry Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.TelemetryNetworkHealth> query = (from x in _context.TelemetryNetworkHealths
				where
				(x.id == id)
				select x);


			Database.TelemetryNetworkHealth telemetryNetworkHealth = await query.FirstOrDefaultAsync(cancellationToken);

			if (telemetryNetworkHealth == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Telemetry.TelemetryNetworkHealth DELETE", id.ToString(), new Exception("No Telemetry.TelemetryNetworkHealth entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.TelemetryNetworkHealth cloneOfExisting = (Database.TelemetryNetworkHealth)_context.Entry(telemetryNetworkHealth).GetDatabaseValues().ToObject();


			try
			{
				_context.TelemetryNetworkHealths.Remove(telemetryNetworkHealth);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Telemetry.TelemetryNetworkHealth entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryNetworkHealth.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryNetworkHealth.CreateAnonymousWithFirstLevelSubObjects(telemetryNetworkHealth)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Telemetry.TelemetryNetworkHealth entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryNetworkHealth.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryNetworkHealth.CreateAnonymousWithFirstLevelSubObjects(telemetryNetworkHealth)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of TelemetryNetworkHealth records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/TelemetryNetworkHealths/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? telemetrySnapshotId = null,
			string interfaceName = null,
			string interfaceDescription = null,
			double? linkSpeedMbps = null,
			long? bytesSentTotal = null,
			long? bytesReceivedTotal = null,
			double? bytesSentPerSecond = null,
			double? bytesReceivedPerSecond = null,
			double? utilizationPercent = null,
			string status = null,
			bool? isActive = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Telemetry Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);


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

			IQueryable<Database.TelemetryNetworkHealth> query = (from tnh in _context.TelemetryNetworkHealths select tnh);
			if (telemetrySnapshotId.HasValue == true)
			{
				query = query.Where(tnh => tnh.telemetrySnapshotId == telemetrySnapshotId.Value);
			}
			if (string.IsNullOrEmpty(interfaceName) == false)
			{
				query = query.Where(tnh => tnh.interfaceName == interfaceName);
			}
			if (string.IsNullOrEmpty(interfaceDescription) == false)
			{
				query = query.Where(tnh => tnh.interfaceDescription == interfaceDescription);
			}
			if (bytesSentTotal.HasValue == true)
			{
				query = query.Where(tnh => tnh.bytesSentTotal == bytesSentTotal.Value);
			}
			if (bytesReceivedTotal.HasValue == true)
			{
				query = query.Where(tnh => tnh.bytesReceivedTotal == bytesReceivedTotal.Value);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(tnh => tnh.status == status);
			}
			if (isActive.HasValue == true)
			{
				query = query.Where(tnh => tnh.isActive == isActive.Value);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Telemetry Network Health, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.interfaceName.Contains(anyStringContains)
			       || x.interfaceDescription.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || x.telemetrySnapshot.machineName.Contains(anyStringContains)
			       || x.telemetrySnapshot.dotNetVersion.Contains(anyStringContains)
			       || x.telemetrySnapshot.statusJson.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.interfaceName).ThenBy(x => x.interfaceDescription).ThenBy(x => x.status);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.TelemetryNetworkHealth.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/TelemetryNetworkHealth/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Telemetry Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
