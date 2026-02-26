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
    /// This auto generated class provides the basic CRUD operations for the TelemetryErrorEvent entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the TelemetryErrorEvent entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class TelemetryErrorEventsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private TelemetryContext _context;

		private ILogger<TelemetryErrorEventsController> _logger;

		public TelemetryErrorEventsController(TelemetryContext context, ILogger<TelemetryErrorEventsController> logger) : base("Telemetry", "TelemetryErrorEvent")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of TelemetryErrorEvents filtered by the parameters provided.
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
		[Route("api/TelemetryErrorEvents")]
		public async Task<IActionResult> GetTelemetryErrorEvents(
			int? telemetryApplicationId = null,
			int? telemetrySnapshotId = null,
			long? originalAuditEventId = null,
			DateTime? occurredAt = null,
			string auditTypeName = null,
			string moduleName = null,
			string entityName = null,
			string userName = null,
			string message = null,
			string errorMessage = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (occurredAt.HasValue == true && occurredAt.Value.Kind != DateTimeKind.Utc)
			{
				occurredAt = occurredAt.Value.ToUniversalTime();
			}

			IQueryable<Database.TelemetryErrorEvent> query = (from tee in _context.TelemetryErrorEvents select tee);
			if (telemetryApplicationId.HasValue == true)
			{
				query = query.Where(tee => tee.telemetryApplicationId == telemetryApplicationId.Value);
			}
			if (telemetrySnapshotId.HasValue == true)
			{
				query = query.Where(tee => tee.telemetrySnapshotId == telemetrySnapshotId.Value);
			}
			if (originalAuditEventId.HasValue == true)
			{
				query = query.Where(tee => tee.originalAuditEventId == originalAuditEventId.Value);
			}
			if (occurredAt.HasValue == true)
			{
				query = query.Where(tee => tee.occurredAt == occurredAt.Value);
			}
			if (string.IsNullOrEmpty(auditTypeName) == false)
			{
				query = query.Where(tee => tee.auditTypeName == auditTypeName);
			}
			if (string.IsNullOrEmpty(moduleName) == false)
			{
				query = query.Where(tee => tee.moduleName == moduleName);
			}
			if (string.IsNullOrEmpty(entityName) == false)
			{
				query = query.Where(tee => tee.entityName == entityName);
			}
			if (string.IsNullOrEmpty(userName) == false)
			{
				query = query.Where(tee => tee.userName == userName);
			}
			if (string.IsNullOrEmpty(message) == false)
			{
				query = query.Where(tee => tee.message == message);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(tee => tee.errorMessage == errorMessage);
			}

			query = query.OrderBy(tee => tee.auditTypeName).ThenBy(tee => tee.moduleName).ThenBy(tee => tee.entityName);


			//
			// Add the any string contains parameter to span all the string fields on the Telemetry Error Event, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.auditTypeName.Contains(anyStringContains)
			       || x.moduleName.Contains(anyStringContains)
			       || x.entityName.Contains(anyStringContains)
			       || x.userName.Contains(anyStringContains)
			       || x.message.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			       || (includeRelations == true && x.telemetryApplication.name.Contains(anyStringContains))
			       || (includeRelations == true && x.telemetryApplication.url.Contains(anyStringContains))
			       || (includeRelations == true && x.telemetrySnapshot.machineName.Contains(anyStringContains))
			       || (includeRelations == true && x.telemetrySnapshot.dotNetVersion.Contains(anyStringContains))
			       || (includeRelations == true && x.telemetrySnapshot.statusJson.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.telemetryApplication);
				query = query.Include(x => x.telemetrySnapshot);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.TelemetryErrorEvent> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.TelemetryErrorEvent telemetryErrorEvent in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(telemetryErrorEvent, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Telemetry.TelemetryErrorEvent Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Telemetry.TelemetryErrorEvent Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of TelemetryErrorEvents filtered by the parameters provided.  Its query is similar to the GetTelemetryErrorEvents method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryErrorEvents/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? telemetryApplicationId = null,
			int? telemetrySnapshotId = null,
			long? originalAuditEventId = null,
			DateTime? occurredAt = null,
			string auditTypeName = null,
			string moduleName = null,
			string entityName = null,
			string userName = null,
			string message = null,
			string errorMessage = null,
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

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (occurredAt.HasValue == true && occurredAt.Value.Kind != DateTimeKind.Utc)
			{
				occurredAt = occurredAt.Value.ToUniversalTime();
			}

			IQueryable<Database.TelemetryErrorEvent> query = (from tee in _context.TelemetryErrorEvents select tee);
			if (telemetryApplicationId.HasValue == true)
			{
				query = query.Where(tee => tee.telemetryApplicationId == telemetryApplicationId.Value);
			}
			if (telemetrySnapshotId.HasValue == true)
			{
				query = query.Where(tee => tee.telemetrySnapshotId == telemetrySnapshotId.Value);
			}
			if (originalAuditEventId.HasValue == true)
			{
				query = query.Where(tee => tee.originalAuditEventId == originalAuditEventId.Value);
			}
			if (occurredAt.HasValue == true)
			{
				query = query.Where(tee => tee.occurredAt == occurredAt.Value);
			}
			if (auditTypeName != null)
			{
				query = query.Where(tee => tee.auditTypeName == auditTypeName);
			}
			if (moduleName != null)
			{
				query = query.Where(tee => tee.moduleName == moduleName);
			}
			if (entityName != null)
			{
				query = query.Where(tee => tee.entityName == entityName);
			}
			if (userName != null)
			{
				query = query.Where(tee => tee.userName == userName);
			}
			if (message != null)
			{
				query = query.Where(tee => tee.message == message);
			}
			if (errorMessage != null)
			{
				query = query.Where(tee => tee.errorMessage == errorMessage);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Telemetry Error Event, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.auditTypeName.Contains(anyStringContains)
			       || x.moduleName.Contains(anyStringContains)
			       || x.entityName.Contains(anyStringContains)
			       || x.userName.Contains(anyStringContains)
			       || x.message.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			       || x.telemetryApplication.name.Contains(anyStringContains)
			       || x.telemetryApplication.url.Contains(anyStringContains)
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
        /// This gets a single TelemetryErrorEvent by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryErrorEvent/{id}")]
		public async Task<IActionResult> GetTelemetryErrorEvent(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.TelemetryErrorEvent> query = (from tee in _context.TelemetryErrorEvents where
				(tee.id == id)
					select tee);

				if (includeRelations == true)
				{
					query = query.Include(x => x.telemetryApplication);
					query = query.Include(x => x.telemetrySnapshot);
					query = query.AsSplitQuery();
				}

				Database.TelemetryErrorEvent materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Telemetry.TelemetryErrorEvent Entity was read with Admin privilege." : "Telemetry.TelemetryErrorEvent Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TelemetryErrorEvent", materialized.id, materialized.auditTypeName));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Telemetry.TelemetryErrorEvent entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Telemetry.TelemetryErrorEvent.   Entity was read with Admin privilege." : "Exception caught during entity read of Telemetry.TelemetryErrorEvent.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing TelemetryErrorEvent record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/TelemetryErrorEvent/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutTelemetryErrorEvent(int id, [FromBody]Database.TelemetryErrorEvent.TelemetryErrorEventDTO telemetryErrorEventDTO, CancellationToken cancellationToken = default)
		{
			if (telemetryErrorEventDTO == null)
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



			if (id != telemetryErrorEventDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.TelemetryErrorEvent> query = (from x in _context.TelemetryErrorEvents
				where
				(x.id == id)
				select x);


			Database.TelemetryErrorEvent existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Telemetry.TelemetryErrorEvent PUT", id.ToString(), new Exception("No Telemetry.TelemetryErrorEvent entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.TelemetryErrorEvent cloneOfExisting = (Database.TelemetryErrorEvent)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new TelemetryErrorEvent object using the data from the existing record, updated with what is in the DTO.
			//
			Database.TelemetryErrorEvent telemetryErrorEvent = (Database.TelemetryErrorEvent)_context.Entry(existing).GetDatabaseValues().ToObject();
			telemetryErrorEvent.ApplyDTO(telemetryErrorEventDTO);


			if (telemetryErrorEvent.occurredAt.Kind != DateTimeKind.Utc)
			{
				telemetryErrorEvent.occurredAt = telemetryErrorEvent.occurredAt.ToUniversalTime();
			}

			if (telemetryErrorEvent.auditTypeName != null && telemetryErrorEvent.auditTypeName.Length > 100)
			{
				telemetryErrorEvent.auditTypeName = telemetryErrorEvent.auditTypeName.Substring(0, 100);
			}

			if (telemetryErrorEvent.moduleName != null && telemetryErrorEvent.moduleName.Length > 100)
			{
				telemetryErrorEvent.moduleName = telemetryErrorEvent.moduleName.Substring(0, 100);
			}

			if (telemetryErrorEvent.entityName != null && telemetryErrorEvent.entityName.Length > 100)
			{
				telemetryErrorEvent.entityName = telemetryErrorEvent.entityName.Substring(0, 100);
			}

			if (telemetryErrorEvent.userName != null && telemetryErrorEvent.userName.Length > 500)
			{
				telemetryErrorEvent.userName = telemetryErrorEvent.userName.Substring(0, 500);
			}

			EntityEntry<Database.TelemetryErrorEvent> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(telemetryErrorEvent);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Telemetry.TelemetryErrorEvent entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryErrorEvent.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryErrorEvent.CreateAnonymousWithFirstLevelSubObjects(telemetryErrorEvent)),
					null);


				return Ok(Database.TelemetryErrorEvent.CreateAnonymous(telemetryErrorEvent));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Telemetry.TelemetryErrorEvent entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryErrorEvent.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryErrorEvent.CreateAnonymousWithFirstLevelSubObjects(telemetryErrorEvent)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new TelemetryErrorEvent record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryErrorEvent", Name = "TelemetryErrorEvent")]
		public async Task<IActionResult> PostTelemetryErrorEvent([FromBody]Database.TelemetryErrorEvent.TelemetryErrorEventDTO telemetryErrorEventDTO, CancellationToken cancellationToken = default)
		{
			if (telemetryErrorEventDTO == null)
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
			// Create a new TelemetryErrorEvent object using the data from the DTO
			//
			Database.TelemetryErrorEvent telemetryErrorEvent = Database.TelemetryErrorEvent.FromDTO(telemetryErrorEventDTO);

			try
			{
				if (telemetryErrorEvent.occurredAt.Kind != DateTimeKind.Utc)
				{
					telemetryErrorEvent.occurredAt = telemetryErrorEvent.occurredAt.ToUniversalTime();
				}

				if (telemetryErrorEvent.auditTypeName != null && telemetryErrorEvent.auditTypeName.Length > 100)
				{
					telemetryErrorEvent.auditTypeName = telemetryErrorEvent.auditTypeName.Substring(0, 100);
				}

				if (telemetryErrorEvent.moduleName != null && telemetryErrorEvent.moduleName.Length > 100)
				{
					telemetryErrorEvent.moduleName = telemetryErrorEvent.moduleName.Substring(0, 100);
				}

				if (telemetryErrorEvent.entityName != null && telemetryErrorEvent.entityName.Length > 100)
				{
					telemetryErrorEvent.entityName = telemetryErrorEvent.entityName.Substring(0, 100);
				}

				if (telemetryErrorEvent.userName != null && telemetryErrorEvent.userName.Length > 500)
				{
					telemetryErrorEvent.userName = telemetryErrorEvent.userName.Substring(0, 500);
				}

				_context.TelemetryErrorEvents.Add(telemetryErrorEvent);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Telemetry.TelemetryErrorEvent entity successfully created.",
					true,
					telemetryErrorEvent.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.TelemetryErrorEvent.CreateAnonymousWithFirstLevelSubObjects(telemetryErrorEvent)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Telemetry.TelemetryErrorEvent entity creation failed.", false, telemetryErrorEvent.id.ToString(), "", JsonSerializer.Serialize(telemetryErrorEvent), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TelemetryErrorEvent", telemetryErrorEvent.id, telemetryErrorEvent.auditTypeName));

			return CreatedAtRoute("TelemetryErrorEvent", new { id = telemetryErrorEvent.id }, Database.TelemetryErrorEvent.CreateAnonymousWithFirstLevelSubObjects(telemetryErrorEvent));
		}



        /// <summary>
        /// 
        /// This deletes a TelemetryErrorEvent record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryErrorEvent/{id}")]
		[Route("api/TelemetryErrorEvent")]
		public async Task<IActionResult> DeleteTelemetryErrorEvent(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.TelemetryErrorEvent> query = (from x in _context.TelemetryErrorEvents
				where
				(x.id == id)
				select x);


			Database.TelemetryErrorEvent telemetryErrorEvent = await query.FirstOrDefaultAsync(cancellationToken);

			if (telemetryErrorEvent == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Telemetry.TelemetryErrorEvent DELETE", id.ToString(), new Exception("No Telemetry.TelemetryErrorEvent entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.TelemetryErrorEvent cloneOfExisting = (Database.TelemetryErrorEvent)_context.Entry(telemetryErrorEvent).GetDatabaseValues().ToObject();


			try
			{
				_context.TelemetryErrorEvents.Remove(telemetryErrorEvent);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Telemetry.TelemetryErrorEvent entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryErrorEvent.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryErrorEvent.CreateAnonymousWithFirstLevelSubObjects(telemetryErrorEvent)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Telemetry.TelemetryErrorEvent entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryErrorEvent.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryErrorEvent.CreateAnonymousWithFirstLevelSubObjects(telemetryErrorEvent)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of TelemetryErrorEvent records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/TelemetryErrorEvents/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? telemetryApplicationId = null,
			int? telemetrySnapshotId = null,
			long? originalAuditEventId = null,
			DateTime? occurredAt = null,
			string auditTypeName = null,
			string moduleName = null,
			string entityName = null,
			string userName = null,
			string message = null,
			string errorMessage = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (occurredAt.HasValue == true && occurredAt.Value.Kind != DateTimeKind.Utc)
			{
				occurredAt = occurredAt.Value.ToUniversalTime();
			}

			IQueryable<Database.TelemetryErrorEvent> query = (from tee in _context.TelemetryErrorEvents select tee);
			if (telemetryApplicationId.HasValue == true)
			{
				query = query.Where(tee => tee.telemetryApplicationId == telemetryApplicationId.Value);
			}
			if (telemetrySnapshotId.HasValue == true)
			{
				query = query.Where(tee => tee.telemetrySnapshotId == telemetrySnapshotId.Value);
			}
			if (originalAuditEventId.HasValue == true)
			{
				query = query.Where(tee => tee.originalAuditEventId == originalAuditEventId.Value);
			}
			if (occurredAt.HasValue == true)
			{
				query = query.Where(tee => tee.occurredAt == occurredAt.Value);
			}
			if (string.IsNullOrEmpty(auditTypeName) == false)
			{
				query = query.Where(tee => tee.auditTypeName == auditTypeName);
			}
			if (string.IsNullOrEmpty(moduleName) == false)
			{
				query = query.Where(tee => tee.moduleName == moduleName);
			}
			if (string.IsNullOrEmpty(entityName) == false)
			{
				query = query.Where(tee => tee.entityName == entityName);
			}
			if (string.IsNullOrEmpty(userName) == false)
			{
				query = query.Where(tee => tee.userName == userName);
			}
			if (string.IsNullOrEmpty(message) == false)
			{
				query = query.Where(tee => tee.message == message);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(tee => tee.errorMessage == errorMessage);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Telemetry Error Event, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.auditTypeName.Contains(anyStringContains)
			       || x.moduleName.Contains(anyStringContains)
			       || x.entityName.Contains(anyStringContains)
			       || x.userName.Contains(anyStringContains)
			       || x.message.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			       || x.telemetryApplication.name.Contains(anyStringContains)
			       || x.telemetryApplication.url.Contains(anyStringContains)
			       || x.telemetrySnapshot.machineName.Contains(anyStringContains)
			       || x.telemetrySnapshot.dotNetVersion.Contains(anyStringContains)
			       || x.telemetrySnapshot.statusJson.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.auditTypeName).ThenBy(x => x.moduleName).ThenBy(x => x.entityName);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.TelemetryErrorEvent.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/TelemetryErrorEvent/CreateAuditEvent")]
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
