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
    /// This auto generated class provides the basic CRUD operations for the TelemetryLogError entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the TelemetryLogError entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class TelemetryLogErrorsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private TelemetryContext _context;

		private ILogger<TelemetryLogErrorsController> _logger;

		public TelemetryLogErrorsController(TelemetryContext context, ILogger<TelemetryLogErrorsController> logger) : base("Telemetry", "TelemetryLogError")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of TelemetryLogErrors filtered by the parameters provided.
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
		[Route("api/TelemetryLogErrors")]
		public async Task<IActionResult> GetTelemetryLogErrors(
			int? telemetryApplicationId = null,
			int? telemetrySnapshotId = null,
			DateTime? capturedAt = null,
			string logFileName = null,
			DateTime? logTimestamp = null,
			string level = null,
			string message = null,
			string exception = null,
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
			if (capturedAt.HasValue == true && capturedAt.Value.Kind != DateTimeKind.Utc)
			{
				capturedAt = capturedAt.Value.ToUniversalTime();
			}

			if (logTimestamp.HasValue == true && logTimestamp.Value.Kind != DateTimeKind.Utc)
			{
				logTimestamp = logTimestamp.Value.ToUniversalTime();
			}

			IQueryable<Telemetry.Database.TelemetryLogError> query = (from tle in _context.TelemetryLogErrors select tle);
			if (telemetryApplicationId.HasValue == true)
			{
				query = query.Where(tle => tle.telemetryApplicationId == telemetryApplicationId.Value);
			}
			if (telemetrySnapshotId.HasValue == true)
			{
				query = query.Where(tle => tle.telemetrySnapshotId == telemetrySnapshotId.Value);
			}
			if (capturedAt.HasValue == true)
			{
				query = query.Where(tle => tle.capturedAt == capturedAt.Value);
			}
			if (string.IsNullOrEmpty(logFileName) == false)
			{
				query = query.Where(tle => tle.logFileName == logFileName);
			}
			if (logTimestamp.HasValue == true)
			{
				query = query.Where(tle => tle.logTimestamp == logTimestamp.Value);
			}
			if (string.IsNullOrEmpty(level) == false)
			{
				query = query.Where(tle => tle.level == level);
			}
			if (string.IsNullOrEmpty(message) == false)
			{
				query = query.Where(tle => tle.message == message);
			}
			if (string.IsNullOrEmpty(exception) == false)
			{
				query = query.Where(tle => tle.exception == exception);
			}

			query = query.OrderBy(tle => tle.logFileName).ThenBy(tle => tle.level);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.telemetryApplication);
				query = query.Include(x => x.telemetrySnapshot);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Telemetry Log Error, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.logFileName.Contains(anyStringContains)
			       || x.level.Contains(anyStringContains)
			       || x.message.Contains(anyStringContains)
			       || x.exception.Contains(anyStringContains)
			       || (includeRelations == true && x.telemetryApplication.name.Contains(anyStringContains))
			       || (includeRelations == true && x.telemetryApplication.url.Contains(anyStringContains))
			       || (includeRelations == true && x.telemetrySnapshot.machineName.Contains(anyStringContains))
			       || (includeRelations == true && x.telemetrySnapshot.dotNetVersion.Contains(anyStringContains))
			       || (includeRelations == true && x.telemetrySnapshot.statusJson.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Telemetry.Database.TelemetryLogError> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Telemetry.Database.TelemetryLogError telemetryLogError in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(telemetryLogError, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Telemetry.TelemetryLogError Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Telemetry.TelemetryLogError Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of TelemetryLogErrors filtered by the parameters provided.  Its query is similar to the GetTelemetryLogErrors method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryLogErrors/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? telemetryApplicationId = null,
			int? telemetrySnapshotId = null,
			DateTime? capturedAt = null,
			string logFileName = null,
			DateTime? logTimestamp = null,
			string level = null,
			string message = null,
			string exception = null,
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
			if (capturedAt.HasValue == true && capturedAt.Value.Kind != DateTimeKind.Utc)
			{
				capturedAt = capturedAt.Value.ToUniversalTime();
			}

			if (logTimestamp.HasValue == true && logTimestamp.Value.Kind != DateTimeKind.Utc)
			{
				logTimestamp = logTimestamp.Value.ToUniversalTime();
			}

			IQueryable<Telemetry.Database.TelemetryLogError> query = (from tle in _context.TelemetryLogErrors select tle);
			if (telemetryApplicationId.HasValue == true)
			{
				query = query.Where(tle => tle.telemetryApplicationId == telemetryApplicationId.Value);
			}
			if (telemetrySnapshotId.HasValue == true)
			{
				query = query.Where(tle => tle.telemetrySnapshotId == telemetrySnapshotId.Value);
			}
			if (capturedAt.HasValue == true)
			{
				query = query.Where(tle => tle.capturedAt == capturedAt.Value);
			}
			if (logFileName != null)
			{
				query = query.Where(tle => tle.logFileName == logFileName);
			}
			if (logTimestamp.HasValue == true)
			{
				query = query.Where(tle => tle.logTimestamp == logTimestamp.Value);
			}
			if (level != null)
			{
				query = query.Where(tle => tle.level == level);
			}
			if (message != null)
			{
				query = query.Where(tle => tle.message == message);
			}
			if (exception != null)
			{
				query = query.Where(tle => tle.exception == exception);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Telemetry Log Error, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.logFileName.Contains(anyStringContains)
			       || x.level.Contains(anyStringContains)
			       || x.message.Contains(anyStringContains)
			       || x.exception.Contains(anyStringContains)
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
        /// This gets a single TelemetryLogError by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryLogError/{id}")]
		public async Task<IActionResult> GetTelemetryLogError(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Telemetry.Database.TelemetryLogError> query = (from tle in _context.TelemetryLogErrors where
				(tle.id == id)
					select tle);

				if (includeRelations == true)
				{
					query = query.Include(x => x.telemetryApplication);
					query = query.Include(x => x.telemetrySnapshot);
					query = query.AsSplitQuery();
				}

				Telemetry.Database.TelemetryLogError materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Telemetry.TelemetryLogError Entity was read with Admin privilege." : "Telemetry.TelemetryLogError Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TelemetryLogError", materialized.id, materialized.logFileName));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Telemetry.TelemetryLogError entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Telemetry.TelemetryLogError.   Entity was read with Admin privilege." : "Exception caught during entity read of Telemetry.TelemetryLogError.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing TelemetryLogError record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/TelemetryLogError/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutTelemetryLogError(int id, [FromBody]Telemetry.Database.TelemetryLogError.TelemetryLogErrorDTO telemetryLogErrorDTO, CancellationToken cancellationToken = default)
		{
			if (telemetryLogErrorDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			// Admin privilege needed to write to this table.
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != telemetryLogErrorDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Telemetry.Database.TelemetryLogError> query = (from x in _context.TelemetryLogErrors
				where
				(x.id == id)
				select x);


			Telemetry.Database.TelemetryLogError existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Telemetry.TelemetryLogError PUT", id.ToString(), new Exception("No Telemetry.TelemetryLogError entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Telemetry.Database.TelemetryLogError cloneOfExisting = (Telemetry.Database.TelemetryLogError)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new TelemetryLogError object using the data from the existing record, updated with what is in the DTO.
			//
			Telemetry.Database.TelemetryLogError telemetryLogError = (Telemetry.Database.TelemetryLogError)_context.Entry(existing).GetDatabaseValues().ToObject();
			telemetryLogError.ApplyDTO(telemetryLogErrorDTO);


			if (telemetryLogError.capturedAt.Kind != DateTimeKind.Utc)
			{
				telemetryLogError.capturedAt = telemetryLogError.capturedAt.ToUniversalTime();
			}

			if (telemetryLogError.logFileName != null && telemetryLogError.logFileName.Length > 250)
			{
				telemetryLogError.logFileName = telemetryLogError.logFileName.Substring(0, 250);
			}

			if (telemetryLogError.logTimestamp.HasValue == true && telemetryLogError.logTimestamp.Value.Kind != DateTimeKind.Utc)
			{
				telemetryLogError.logTimestamp = telemetryLogError.logTimestamp.Value.ToUniversalTime();
			}

			if (telemetryLogError.level != null && telemetryLogError.level.Length > 50)
			{
				telemetryLogError.level = telemetryLogError.level.Substring(0, 50);
			}

			EntityEntry<Telemetry.Database.TelemetryLogError> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(telemetryLogError);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Telemetry.TelemetryLogError entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Telemetry.Database.TelemetryLogError.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Telemetry.Database.TelemetryLogError.CreateAnonymousWithFirstLevelSubObjects(telemetryLogError)),
					null);


				return Ok(Telemetry.Database.TelemetryLogError.CreateAnonymous(telemetryLogError));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Telemetry.TelemetryLogError entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Telemetry.Database.TelemetryLogError.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Telemetry.Database.TelemetryLogError.CreateAnonymousWithFirstLevelSubObjects(telemetryLogError)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new TelemetryLogError record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryLogError", Name = "TelemetryLogError")]
		public async Task<IActionResult> PostTelemetryLogError([FromBody]Telemetry.Database.TelemetryLogError.TelemetryLogErrorDTO telemetryLogErrorDTO, CancellationToken cancellationToken = default)
		{
			if (telemetryLogErrorDTO == null)
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
			// Create a new TelemetryLogError object using the data from the DTO
			//
			Telemetry.Database.TelemetryLogError telemetryLogError = Telemetry.Database.TelemetryLogError.FromDTO(telemetryLogErrorDTO);

			try
			{
				if (telemetryLogError.capturedAt.Kind != DateTimeKind.Utc)
				{
					telemetryLogError.capturedAt = telemetryLogError.capturedAt.ToUniversalTime();
				}

				if (telemetryLogError.logFileName != null && telemetryLogError.logFileName.Length > 250)
				{
					telemetryLogError.logFileName = telemetryLogError.logFileName.Substring(0, 250);
				}

				if (telemetryLogError.logTimestamp.HasValue == true && telemetryLogError.logTimestamp.Value.Kind != DateTimeKind.Utc)
				{
					telemetryLogError.logTimestamp = telemetryLogError.logTimestamp.Value.ToUniversalTime();
				}

				if (telemetryLogError.level != null && telemetryLogError.level.Length > 50)
				{
					telemetryLogError.level = telemetryLogError.level.Substring(0, 50);
				}

				_context.TelemetryLogErrors.Add(telemetryLogError);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Telemetry.TelemetryLogError entity successfully created.",
					true,
					telemetryLogError.id.ToString(),
					"",
					JsonSerializer.Serialize(Telemetry.Database.TelemetryLogError.CreateAnonymousWithFirstLevelSubObjects(telemetryLogError)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Telemetry.TelemetryLogError entity creation failed.", false, telemetryLogError.id.ToString(), "", JsonSerializer.Serialize(telemetryLogError), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TelemetryLogError", telemetryLogError.id, telemetryLogError.logFileName));

			return CreatedAtRoute("TelemetryLogError", new { id = telemetryLogError.id }, Telemetry.Database.TelemetryLogError.CreateAnonymousWithFirstLevelSubObjects(telemetryLogError));
		}



        /// <summary>
        /// 
        /// This deletes a TelemetryLogError record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryLogError/{id}")]
		[Route("api/TelemetryLogError")]
		public async Task<IActionResult> DeleteTelemetryLogError(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Telemetry.Database.TelemetryLogError> query = (from x in _context.TelemetryLogErrors
				where
				(x.id == id)
				select x);


			Telemetry.Database.TelemetryLogError telemetryLogError = await query.FirstOrDefaultAsync(cancellationToken);

			if (telemetryLogError == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Telemetry.TelemetryLogError DELETE", id.ToString(), new Exception("No Telemetry.TelemetryLogError entity could be find with the primary key provided."));
				return NotFound();
			}
			Telemetry.Database.TelemetryLogError cloneOfExisting = (Telemetry.Database.TelemetryLogError)_context.Entry(telemetryLogError).GetDatabaseValues().ToObject();


			try
			{
				_context.TelemetryLogErrors.Remove(telemetryLogError);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Telemetry.TelemetryLogError entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Telemetry.Database.TelemetryLogError.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Telemetry.Database.TelemetryLogError.CreateAnonymousWithFirstLevelSubObjects(telemetryLogError)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Telemetry.TelemetryLogError entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Telemetry.Database.TelemetryLogError.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Telemetry.Database.TelemetryLogError.CreateAnonymousWithFirstLevelSubObjects(telemetryLogError)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of TelemetryLogError records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/TelemetryLogErrors/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? telemetryApplicationId = null,
			int? telemetrySnapshotId = null,
			DateTime? capturedAt = null,
			string logFileName = null,
			DateTime? logTimestamp = null,
			string level = null,
			string message = null,
			string exception = null,
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
			if (capturedAt.HasValue == true && capturedAt.Value.Kind != DateTimeKind.Utc)
			{
				capturedAt = capturedAt.Value.ToUniversalTime();
			}

			if (logTimestamp.HasValue == true && logTimestamp.Value.Kind != DateTimeKind.Utc)
			{
				logTimestamp = logTimestamp.Value.ToUniversalTime();
			}

			IQueryable<Telemetry.Database.TelemetryLogError> query = (from tle in _context.TelemetryLogErrors select tle);
			if (telemetryApplicationId.HasValue == true)
			{
				query = query.Where(tle => tle.telemetryApplicationId == telemetryApplicationId.Value);
			}
			if (telemetrySnapshotId.HasValue == true)
			{
				query = query.Where(tle => tle.telemetrySnapshotId == telemetrySnapshotId.Value);
			}
			if (capturedAt.HasValue == true)
			{
				query = query.Where(tle => tle.capturedAt == capturedAt.Value);
			}
			if (string.IsNullOrEmpty(logFileName) == false)
			{
				query = query.Where(tle => tle.logFileName == logFileName);
			}
			if (logTimestamp.HasValue == true)
			{
				query = query.Where(tle => tle.logTimestamp == logTimestamp.Value);
			}
			if (string.IsNullOrEmpty(level) == false)
			{
				query = query.Where(tle => tle.level == level);
			}
			if (string.IsNullOrEmpty(message) == false)
			{
				query = query.Where(tle => tle.message == message);
			}
			if (string.IsNullOrEmpty(exception) == false)
			{
				query = query.Where(tle => tle.exception == exception);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Telemetry Log Error, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.logFileName.Contains(anyStringContains)
			       || x.level.Contains(anyStringContains)
			       || x.message.Contains(anyStringContains)
			       || x.exception.Contains(anyStringContains)
			       || x.telemetryApplication.name.Contains(anyStringContains)
			       || x.telemetryApplication.url.Contains(anyStringContains)
			       || x.telemetrySnapshot.machineName.Contains(anyStringContains)
			       || x.telemetrySnapshot.dotNetVersion.Contains(anyStringContains)
			       || x.telemetrySnapshot.statusJson.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.logFileName).ThenBy(x => x.level);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Telemetry.Database.TelemetryLogError.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/TelemetryLogError/CreateAuditEvent")]
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
