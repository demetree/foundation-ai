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
    /// This auto generated class provides the basic CRUD operations for the TelemetryCollectionRun entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the TelemetryCollectionRun entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class TelemetryCollectionRunsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private TelemetryContext _context;

		private ILogger<TelemetryCollectionRunsController> _logger;

		public TelemetryCollectionRunsController(TelemetryContext context, ILogger<TelemetryCollectionRunsController> logger) : base("Telemetry", "TelemetryCollectionRun")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of TelemetryCollectionRuns filtered by the parameters provided.
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
		[Route("api/TelemetryCollectionRuns")]
		public async Task<IActionResult> GetTelemetryCollectionRuns(
			DateTime? startTime = null,
			DateTime? endTime = null,
			int? applicationsPolled = null,
			int? applicationsSucceeded = null,
			string errorMessage = null,
			int? pageSize = null,
			int? pageNumber = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (startTime.HasValue == true && startTime.Value.Kind != DateTimeKind.Utc)
			{
				startTime = startTime.Value.ToUniversalTime();
			}

			if (endTime.HasValue == true && endTime.Value.Kind != DateTimeKind.Utc)
			{
				endTime = endTime.Value.ToUniversalTime();
			}

			IQueryable<Database.TelemetryCollectionRun> query = (from tcr in _context.TelemetryCollectionRuns select tcr);
			if (startTime.HasValue == true)
			{
				query = query.Where(tcr => tcr.startTime == startTime.Value);
			}
			if (endTime.HasValue == true)
			{
				query = query.Where(tcr => tcr.endTime == endTime.Value);
			}
			if (applicationsPolled.HasValue == true)
			{
				query = query.Where(tcr => tcr.applicationsPolled == applicationsPolled.Value);
			}
			if (applicationsSucceeded.HasValue == true)
			{
				query = query.Where(tcr => tcr.applicationsSucceeded == applicationsSucceeded.Value);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(tcr => tcr.errorMessage == errorMessage);
			}

			query = query.OrderBy(tcr => tcr.id);

			if (includeRelations == true)
			{
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.TelemetryCollectionRun> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.TelemetryCollectionRun telemetryCollectionRun in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(telemetryCollectionRun, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Telemetry.TelemetryCollectionRun Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Telemetry.TelemetryCollectionRun Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of TelemetryCollectionRuns filtered by the parameters provided.  Its query is similar to the GetTelemetryCollectionRuns method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryCollectionRuns/RowCount")]
		public async Task<IActionResult> GetRowCount(
			DateTime? startTime = null,
			DateTime? endTime = null,
			int? applicationsPolled = null,
			int? applicationsSucceeded = null,
			string errorMessage = null,
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

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (startTime.HasValue == true && startTime.Value.Kind != DateTimeKind.Utc)
			{
				startTime = startTime.Value.ToUniversalTime();
			}

			if (endTime.HasValue == true && endTime.Value.Kind != DateTimeKind.Utc)
			{
				endTime = endTime.Value.ToUniversalTime();
			}

			IQueryable<Database.TelemetryCollectionRun> query = (from tcr in _context.TelemetryCollectionRuns select tcr);
			if (startTime.HasValue == true)
			{
				query = query.Where(tcr => tcr.startTime == startTime.Value);
			}
			if (endTime.HasValue == true)
			{
				query = query.Where(tcr => tcr.endTime == endTime.Value);
			}
			if (applicationsPolled.HasValue == true)
			{
				query = query.Where(tcr => tcr.applicationsPolled == applicationsPolled.Value);
			}
			if (applicationsSucceeded.HasValue == true)
			{
				query = query.Where(tcr => tcr.applicationsSucceeded == applicationsSucceeded.Value);
			}
			if (errorMessage != null)
			{
				query = query.Where(tcr => tcr.errorMessage == errorMessage);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single TelemetryCollectionRun by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryCollectionRun/{id}")]
		public async Task<IActionResult> GetTelemetryCollectionRun(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.TelemetryCollectionRun> query = (from tcr in _context.TelemetryCollectionRuns where
				(tcr.id == id)
					select tcr);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.TelemetryCollectionRun materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Telemetry.TelemetryCollectionRun Entity was read with Admin privilege." : "Telemetry.TelemetryCollectionRun Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TelemetryCollectionRun", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Telemetry.TelemetryCollectionRun entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Telemetry.TelemetryCollectionRun.   Entity was read with Admin privilege." : "Exception caught during entity read of Telemetry.TelemetryCollectionRun.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing TelemetryCollectionRun record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/TelemetryCollectionRun/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutTelemetryCollectionRun(int id, [FromBody]Database.TelemetryCollectionRun.TelemetryCollectionRunDTO telemetryCollectionRunDTO, CancellationToken cancellationToken = default)
		{
			if (telemetryCollectionRunDTO == null)
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



			if (id != telemetryCollectionRunDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.TelemetryCollectionRun> query = (from x in _context.TelemetryCollectionRuns
				where
				(x.id == id)
				select x);


			Database.TelemetryCollectionRun existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Telemetry.TelemetryCollectionRun PUT", id.ToString(), new Exception("No Telemetry.TelemetryCollectionRun entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.TelemetryCollectionRun cloneOfExisting = (Database.TelemetryCollectionRun)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new TelemetryCollectionRun object using the data from the existing record, updated with what is in the DTO.
			//
			Database.TelemetryCollectionRun telemetryCollectionRun = (Database.TelemetryCollectionRun)_context.Entry(existing).GetDatabaseValues().ToObject();
			telemetryCollectionRun.ApplyDTO(telemetryCollectionRunDTO);


			if (telemetryCollectionRun.startTime.Kind != DateTimeKind.Utc)
			{
				telemetryCollectionRun.startTime = telemetryCollectionRun.startTime.ToUniversalTime();
			}

			if (telemetryCollectionRun.endTime.HasValue == true && telemetryCollectionRun.endTime.Value.Kind != DateTimeKind.Utc)
			{
				telemetryCollectionRun.endTime = telemetryCollectionRun.endTime.Value.ToUniversalTime();
			}

			EntityEntry<Database.TelemetryCollectionRun> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(telemetryCollectionRun);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Telemetry.TelemetryCollectionRun entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryCollectionRun.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryCollectionRun.CreateAnonymousWithFirstLevelSubObjects(telemetryCollectionRun)),
					null);


				return Ok(Database.TelemetryCollectionRun.CreateAnonymous(telemetryCollectionRun));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Telemetry.TelemetryCollectionRun entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryCollectionRun.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryCollectionRun.CreateAnonymousWithFirstLevelSubObjects(telemetryCollectionRun)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new TelemetryCollectionRun record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryCollectionRun", Name = "TelemetryCollectionRun")]
		public async Task<IActionResult> PostTelemetryCollectionRun([FromBody]Database.TelemetryCollectionRun.TelemetryCollectionRunDTO telemetryCollectionRunDTO, CancellationToken cancellationToken = default)
		{
			if (telemetryCollectionRunDTO == null)
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
			// Create a new TelemetryCollectionRun object using the data from the DTO
			//
			Database.TelemetryCollectionRun telemetryCollectionRun = Database.TelemetryCollectionRun.FromDTO(telemetryCollectionRunDTO);

			try
			{
				if (telemetryCollectionRun.startTime.Kind != DateTimeKind.Utc)
				{
					telemetryCollectionRun.startTime = telemetryCollectionRun.startTime.ToUniversalTime();
				}

				if (telemetryCollectionRun.endTime.HasValue == true && telemetryCollectionRun.endTime.Value.Kind != DateTimeKind.Utc)
				{
					telemetryCollectionRun.endTime = telemetryCollectionRun.endTime.Value.ToUniversalTime();
				}

				_context.TelemetryCollectionRuns.Add(telemetryCollectionRun);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Telemetry.TelemetryCollectionRun entity successfully created.",
					true,
					telemetryCollectionRun.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.TelemetryCollectionRun.CreateAnonymousWithFirstLevelSubObjects(telemetryCollectionRun)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Telemetry.TelemetryCollectionRun entity creation failed.", false, telemetryCollectionRun.id.ToString(), "", JsonSerializer.Serialize(telemetryCollectionRun), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TelemetryCollectionRun", telemetryCollectionRun.id, telemetryCollectionRun.id.ToString()));

			return CreatedAtRoute("TelemetryCollectionRun", new { id = telemetryCollectionRun.id }, Database.TelemetryCollectionRun.CreateAnonymousWithFirstLevelSubObjects(telemetryCollectionRun));
		}



        /// <summary>
        /// 
        /// This deletes a TelemetryCollectionRun record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryCollectionRun/{id}")]
		[Route("api/TelemetryCollectionRun")]
		public async Task<IActionResult> DeleteTelemetryCollectionRun(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.TelemetryCollectionRun> query = (from x in _context.TelemetryCollectionRuns
				where
				(x.id == id)
				select x);


			Database.TelemetryCollectionRun telemetryCollectionRun = await query.FirstOrDefaultAsync(cancellationToken);

			if (telemetryCollectionRun == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Telemetry.TelemetryCollectionRun DELETE", id.ToString(), new Exception("No Telemetry.TelemetryCollectionRun entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.TelemetryCollectionRun cloneOfExisting = (Database.TelemetryCollectionRun)_context.Entry(telemetryCollectionRun).GetDatabaseValues().ToObject();


			try
			{
				_context.TelemetryCollectionRuns.Remove(telemetryCollectionRun);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Telemetry.TelemetryCollectionRun entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryCollectionRun.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryCollectionRun.CreateAnonymousWithFirstLevelSubObjects(telemetryCollectionRun)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Telemetry.TelemetryCollectionRun entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryCollectionRun.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryCollectionRun.CreateAnonymousWithFirstLevelSubObjects(telemetryCollectionRun)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of TelemetryCollectionRun records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/TelemetryCollectionRuns/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			DateTime? startTime = null,
			DateTime? endTime = null,
			int? applicationsPolled = null,
			int? applicationsSucceeded = null,
			string errorMessage = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (startTime.HasValue == true && startTime.Value.Kind != DateTimeKind.Utc)
			{
				startTime = startTime.Value.ToUniversalTime();
			}

			if (endTime.HasValue == true && endTime.Value.Kind != DateTimeKind.Utc)
			{
				endTime = endTime.Value.ToUniversalTime();
			}

			IQueryable<Database.TelemetryCollectionRun> query = (from tcr in _context.TelemetryCollectionRuns select tcr);
			if (startTime.HasValue == true)
			{
				query = query.Where(tcr => tcr.startTime == startTime.Value);
			}
			if (endTime.HasValue == true)
			{
				query = query.Where(tcr => tcr.endTime == endTime.Value);
			}
			if (applicationsPolled.HasValue == true)
			{
				query = query.Where(tcr => tcr.applicationsPolled == applicationsPolled.Value);
			}
			if (applicationsSucceeded.HasValue == true)
			{
				query = query.Where(tcr => tcr.applicationsSucceeded == applicationsSucceeded.Value);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(tcr => tcr.errorMessage == errorMessage);
			}


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.TelemetryCollectionRun.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/TelemetryCollectionRun/CreateAuditEvent")]
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
