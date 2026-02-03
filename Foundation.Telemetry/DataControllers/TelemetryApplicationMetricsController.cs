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
    /// This auto generated class provides the basic CRUD operations for the TelemetryApplicationMetric entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the TelemetryApplicationMetric entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class TelemetryApplicationMetricsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private TelemetryContext _context;

		private ILogger<TelemetryApplicationMetricsController> _logger;

		public TelemetryApplicationMetricsController(TelemetryContext context, ILogger<TelemetryApplicationMetricsController> logger) : base("Telemetry", "TelemetryApplicationMetric")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of TelemetryApplicationMetrics filtered by the parameters provided.
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
		[Route("api/TelemetryApplicationMetrics")]
		public async Task<IActionResult> GetTelemetryApplicationMetrics(
			int? telemetrySnapshotId = null,
			string metricName = null,
			string metricValue = null,
			int? state = null,
			int? dataType = null,
			double? numericValue = null,
			string category = null,
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

			IQueryable<Database.TelemetryApplicationMetric> query = (from tam in _context.TelemetryApplicationMetrics select tam);
			if (telemetrySnapshotId.HasValue == true)
			{
				query = query.Where(tam => tam.telemetrySnapshotId == telemetrySnapshotId.Value);
			}
			if (string.IsNullOrEmpty(metricName) == false)
			{
				query = query.Where(tam => tam.metricName == metricName);
			}
			if (string.IsNullOrEmpty(metricValue) == false)
			{
				query = query.Where(tam => tam.metricValue == metricValue);
			}
			if (state.HasValue == true)
			{
				query = query.Where(tam => tam.state == state.Value);
			}
			if (dataType.HasValue == true)
			{
				query = query.Where(tam => tam.dataType == dataType.Value);
			}
			if (numericValue.HasValue == true)
			{
				query = query.Where(tam => tam.numericValue == numericValue.Value);
			}
			if (string.IsNullOrEmpty(category) == false)
			{
				query = query.Where(tam => tam.category == category);
			}

			query = query.OrderBy(tam => tam.metricName).ThenBy(tam => tam.metricValue).ThenBy(tam => tam.category);

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
			// Add the any string contains parameter to span all the string fields on the Telemetry Application Metric, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.metricName.Contains(anyStringContains)
			       || x.metricValue.Contains(anyStringContains)
			       || x.category.Contains(anyStringContains)
			       || (includeRelations == true && x.telemetrySnapshot.machineName.Contains(anyStringContains))
			       || (includeRelations == true && x.telemetrySnapshot.dotNetVersion.Contains(anyStringContains))
			       || (includeRelations == true && x.telemetrySnapshot.statusJson.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.TelemetryApplicationMetric> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.TelemetryApplicationMetric telemetryApplicationMetric in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(telemetryApplicationMetric, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Telemetry.TelemetryApplicationMetric Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Telemetry.TelemetryApplicationMetric Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of TelemetryApplicationMetrics filtered by the parameters provided.  Its query is similar to the GetTelemetryApplicationMetrics method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryApplicationMetrics/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? telemetrySnapshotId = null,
			string metricName = null,
			string metricValue = null,
			int? state = null,
			int? dataType = null,
			double? numericValue = null,
			string category = null,
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

			IQueryable<Database.TelemetryApplicationMetric> query = (from tam in _context.TelemetryApplicationMetrics select tam);
			if (telemetrySnapshotId.HasValue == true)
			{
				query = query.Where(tam => tam.telemetrySnapshotId == telemetrySnapshotId.Value);
			}
			if (metricName != null)
			{
				query = query.Where(tam => tam.metricName == metricName);
			}
			if (metricValue != null)
			{
				query = query.Where(tam => tam.metricValue == metricValue);
			}
			if (state.HasValue == true)
			{
				query = query.Where(tam => tam.state == state.Value);
			}
			if (dataType.HasValue == true)
			{
				query = query.Where(tam => tam.dataType == dataType.Value);
			}
			if (numericValue.HasValue == true)
			{
				query = query.Where(tam => tam.numericValue == numericValue.Value);
			}
			if (category != null)
			{
				query = query.Where(tam => tam.category == category);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Telemetry Application Metric, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.metricName.Contains(anyStringContains)
			       || x.metricValue.Contains(anyStringContains)
			       || x.category.Contains(anyStringContains)
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
        /// This gets a single TelemetryApplicationMetric by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryApplicationMetric/{id}")]
		public async Task<IActionResult> GetTelemetryApplicationMetric(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.TelemetryApplicationMetric> query = (from tam in _context.TelemetryApplicationMetrics where
				(tam.id == id)
					select tam);

				if (includeRelations == true)
				{
					query = query.Include(x => x.telemetrySnapshot);
					query = query.AsSplitQuery();
				}

				Database.TelemetryApplicationMetric materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Telemetry.TelemetryApplicationMetric Entity was read with Admin privilege." : "Telemetry.TelemetryApplicationMetric Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TelemetryApplicationMetric", materialized.id, materialized.metricName));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Telemetry.TelemetryApplicationMetric entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Telemetry.TelemetryApplicationMetric.   Entity was read with Admin privilege." : "Exception caught during entity read of Telemetry.TelemetryApplicationMetric.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing TelemetryApplicationMetric record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/TelemetryApplicationMetric/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutTelemetryApplicationMetric(int id, [FromBody]Database.TelemetryApplicationMetric.TelemetryApplicationMetricDTO telemetryApplicationMetricDTO, CancellationToken cancellationToken = default)
		{
			if (telemetryApplicationMetricDTO == null)
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



			if (id != telemetryApplicationMetricDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.TelemetryApplicationMetric> query = (from x in _context.TelemetryApplicationMetrics
				where
				(x.id == id)
				select x);


			Database.TelemetryApplicationMetric existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Telemetry.TelemetryApplicationMetric PUT", id.ToString(), new Exception("No Telemetry.TelemetryApplicationMetric entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.TelemetryApplicationMetric cloneOfExisting = (Database.TelemetryApplicationMetric)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new TelemetryApplicationMetric object using the data from the existing record, updated with what is in the DTO.
			//
			Database.TelemetryApplicationMetric telemetryApplicationMetric = (Database.TelemetryApplicationMetric)_context.Entry(existing).GetDatabaseValues().ToObject();
			telemetryApplicationMetric.ApplyDTO(telemetryApplicationMetricDTO);


			if (telemetryApplicationMetric.metricName != null && telemetryApplicationMetric.metricName.Length > 100)
			{
				telemetryApplicationMetric.metricName = telemetryApplicationMetric.metricName.Substring(0, 100);
			}

			if (telemetryApplicationMetric.metricValue != null && telemetryApplicationMetric.metricValue.Length > 500)
			{
				telemetryApplicationMetric.metricValue = telemetryApplicationMetric.metricValue.Substring(0, 500);
			}

			if (telemetryApplicationMetric.category != null && telemetryApplicationMetric.category.Length > 100)
			{
				telemetryApplicationMetric.category = telemetryApplicationMetric.category.Substring(0, 100);
			}

			EntityEntry<Database.TelemetryApplicationMetric> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(telemetryApplicationMetric);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Telemetry.TelemetryApplicationMetric entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryApplicationMetric.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryApplicationMetric.CreateAnonymousWithFirstLevelSubObjects(telemetryApplicationMetric)),
					null);


				return Ok(Database.TelemetryApplicationMetric.CreateAnonymous(telemetryApplicationMetric));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Telemetry.TelemetryApplicationMetric entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryApplicationMetric.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryApplicationMetric.CreateAnonymousWithFirstLevelSubObjects(telemetryApplicationMetric)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new TelemetryApplicationMetric record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryApplicationMetric", Name = "TelemetryApplicationMetric")]
		public async Task<IActionResult> PostTelemetryApplicationMetric([FromBody]Database.TelemetryApplicationMetric.TelemetryApplicationMetricDTO telemetryApplicationMetricDTO, CancellationToken cancellationToken = default)
		{
			if (telemetryApplicationMetricDTO == null)
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
			// Create a new TelemetryApplicationMetric object using the data from the DTO
			//
			Database.TelemetryApplicationMetric telemetryApplicationMetric = Database.TelemetryApplicationMetric.FromDTO(telemetryApplicationMetricDTO);

			try
			{
				if (telemetryApplicationMetric.metricName != null && telemetryApplicationMetric.metricName.Length > 100)
				{
					telemetryApplicationMetric.metricName = telemetryApplicationMetric.metricName.Substring(0, 100);
				}

				if (telemetryApplicationMetric.metricValue != null && telemetryApplicationMetric.metricValue.Length > 500)
				{
					telemetryApplicationMetric.metricValue = telemetryApplicationMetric.metricValue.Substring(0, 500);
				}

				if (telemetryApplicationMetric.category != null && telemetryApplicationMetric.category.Length > 100)
				{
					telemetryApplicationMetric.category = telemetryApplicationMetric.category.Substring(0, 100);
				}

				_context.TelemetryApplicationMetrics.Add(telemetryApplicationMetric);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Telemetry.TelemetryApplicationMetric entity successfully created.",
					true,
					telemetryApplicationMetric.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.TelemetryApplicationMetric.CreateAnonymousWithFirstLevelSubObjects(telemetryApplicationMetric)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Telemetry.TelemetryApplicationMetric entity creation failed.", false, telemetryApplicationMetric.id.ToString(), "", JsonSerializer.Serialize(telemetryApplicationMetric), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TelemetryApplicationMetric", telemetryApplicationMetric.id, telemetryApplicationMetric.metricName));

			return CreatedAtRoute("TelemetryApplicationMetric", new { id = telemetryApplicationMetric.id }, Database.TelemetryApplicationMetric.CreateAnonymousWithFirstLevelSubObjects(telemetryApplicationMetric));
		}



        /// <summary>
        /// 
        /// This deletes a TelemetryApplicationMetric record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryApplicationMetric/{id}")]
		[Route("api/TelemetryApplicationMetric")]
		public async Task<IActionResult> DeleteTelemetryApplicationMetric(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.TelemetryApplicationMetric> query = (from x in _context.TelemetryApplicationMetrics
				where
				(x.id == id)
				select x);


			Database.TelemetryApplicationMetric telemetryApplicationMetric = await query.FirstOrDefaultAsync(cancellationToken);

			if (telemetryApplicationMetric == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Telemetry.TelemetryApplicationMetric DELETE", id.ToString(), new Exception("No Telemetry.TelemetryApplicationMetric entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.TelemetryApplicationMetric cloneOfExisting = (Database.TelemetryApplicationMetric)_context.Entry(telemetryApplicationMetric).GetDatabaseValues().ToObject();


			try
			{
				_context.TelemetryApplicationMetrics.Remove(telemetryApplicationMetric);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Telemetry.TelemetryApplicationMetric entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryApplicationMetric.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryApplicationMetric.CreateAnonymousWithFirstLevelSubObjects(telemetryApplicationMetric)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Telemetry.TelemetryApplicationMetric entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryApplicationMetric.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryApplicationMetric.CreateAnonymousWithFirstLevelSubObjects(telemetryApplicationMetric)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of TelemetryApplicationMetric records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/TelemetryApplicationMetrics/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? telemetrySnapshotId = null,
			string metricName = null,
			string metricValue = null,
			int? state = null,
			int? dataType = null,
			double? numericValue = null,
			string category = null,
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

			IQueryable<Database.TelemetryApplicationMetric> query = (from tam in _context.TelemetryApplicationMetrics select tam);
			if (telemetrySnapshotId.HasValue == true)
			{
				query = query.Where(tam => tam.telemetrySnapshotId == telemetrySnapshotId.Value);
			}
			if (string.IsNullOrEmpty(metricName) == false)
			{
				query = query.Where(tam => tam.metricName == metricName);
			}
			if (string.IsNullOrEmpty(metricValue) == false)
			{
				query = query.Where(tam => tam.metricValue == metricValue);
			}
			if (state.HasValue == true)
			{
				query = query.Where(tam => tam.state == state.Value);
			}
			if (dataType.HasValue == true)
			{
				query = query.Where(tam => tam.dataType == dataType.Value);
			}
			if (string.IsNullOrEmpty(category) == false)
			{
				query = query.Where(tam => tam.category == category);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Telemetry Application Metric, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.metricName.Contains(anyStringContains)
			       || x.metricValue.Contains(anyStringContains)
			       || x.category.Contains(anyStringContains)
			       || x.telemetrySnapshot.machineName.Contains(anyStringContains)
			       || x.telemetrySnapshot.dotNetVersion.Contains(anyStringContains)
			       || x.telemetrySnapshot.statusJson.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.metricName).ThenBy(x => x.metricValue).ThenBy(x => x.category);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.TelemetryApplicationMetric.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/TelemetryApplicationMetric/CreateAuditEvent")]
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
