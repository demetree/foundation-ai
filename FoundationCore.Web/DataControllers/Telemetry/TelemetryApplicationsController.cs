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
    /// This auto generated class provides the basic CRUD operations for the TelemetryApplication entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the TelemetryApplication entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class TelemetryApplicationsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private TelemetryContext _context;

		private ILogger<TelemetryApplicationsController> _logger;

		public TelemetryApplicationsController(TelemetryContext context, ILogger<TelemetryApplicationsController> logger) : base("Telemetry", "TelemetryApplication")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of TelemetryApplications filtered by the parameters provided.
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
		[Route("api/TelemetryApplications")]
		public async Task<IActionResult> GetTelemetryApplications(
			string name = null,
			string url = null,
			bool? isSelf = null,
			DateTime? firstSeen = null,
			DateTime? lastSeen = null,
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
			if (firstSeen.HasValue == true && firstSeen.Value.Kind != DateTimeKind.Utc)
			{
				firstSeen = firstSeen.Value.ToUniversalTime();
			}

			if (lastSeen.HasValue == true && lastSeen.Value.Kind != DateTimeKind.Utc)
			{
				lastSeen = lastSeen.Value.ToUniversalTime();
			}

			IQueryable<Database.TelemetryApplication> query = (from ta in _context.TelemetryApplications select ta);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ta => ta.name == name);
			}
			if (string.IsNullOrEmpty(url) == false)
			{
				query = query.Where(ta => ta.url == url);
			}
			if (isSelf.HasValue == true)
			{
				query = query.Where(ta => ta.isSelf == isSelf.Value);
			}
			if (firstSeen.HasValue == true)
			{
				query = query.Where(ta => ta.firstSeen == firstSeen.Value);
			}
			if (lastSeen.HasValue == true)
			{
				query = query.Where(ta => ta.lastSeen == lastSeen.Value);
			}

			query = query.OrderBy(ta => ta.name).ThenBy(ta => ta.url);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Telemetry Application, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.url.Contains(anyStringContains)
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.TelemetryApplication> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.TelemetryApplication telemetryApplication in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(telemetryApplication, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Telemetry.TelemetryApplication Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Telemetry.TelemetryApplication Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of TelemetryApplications filtered by the parameters provided.  Its query is similar to the GetTelemetryApplications method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryApplications/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string url = null,
			bool? isSelf = null,
			DateTime? firstSeen = null,
			DateTime? lastSeen = null,
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
			if (firstSeen.HasValue == true && firstSeen.Value.Kind != DateTimeKind.Utc)
			{
				firstSeen = firstSeen.Value.ToUniversalTime();
			}

			if (lastSeen.HasValue == true && lastSeen.Value.Kind != DateTimeKind.Utc)
			{
				lastSeen = lastSeen.Value.ToUniversalTime();
			}

			IQueryable<Database.TelemetryApplication> query = (from ta in _context.TelemetryApplications select ta);
			if (name != null)
			{
				query = query.Where(ta => ta.name == name);
			}
			if (url != null)
			{
				query = query.Where(ta => ta.url == url);
			}
			if (isSelf.HasValue == true)
			{
				query = query.Where(ta => ta.isSelf == isSelf.Value);
			}
			if (firstSeen.HasValue == true)
			{
				query = query.Where(ta => ta.firstSeen == firstSeen.Value);
			}
			if (lastSeen.HasValue == true)
			{
				query = query.Where(ta => ta.lastSeen == lastSeen.Value);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Telemetry Application, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.url.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single TelemetryApplication by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryApplication/{id}")]
		public async Task<IActionResult> GetTelemetryApplication(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.TelemetryApplication> query = (from ta in _context.TelemetryApplications where
				(ta.id == id)
					select ta);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.TelemetryApplication materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Telemetry.TelemetryApplication Entity was read with Admin privilege." : "Telemetry.TelemetryApplication Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TelemetryApplication", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Telemetry.TelemetryApplication entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Telemetry.TelemetryApplication.   Entity was read with Admin privilege." : "Exception caught during entity read of Telemetry.TelemetryApplication.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing TelemetryApplication record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/TelemetryApplication/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutTelemetryApplication(int id, [FromBody]Database.TelemetryApplication.TelemetryApplicationDTO telemetryApplicationDTO, CancellationToken cancellationToken = default)
		{
			if (telemetryApplicationDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			// Admin privilege needed to write to this table.
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != telemetryApplicationDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.TelemetryApplication> query = (from x in _context.TelemetryApplications
				where
				(x.id == id)
				select x);


			Database.TelemetryApplication existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Telemetry.TelemetryApplication PUT", id.ToString(), new Exception("No Telemetry.TelemetryApplication entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.TelemetryApplication cloneOfExisting = (Database.TelemetryApplication)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new TelemetryApplication object using the data from the existing record, updated with what is in the DTO.
			//
			Database.TelemetryApplication telemetryApplication = (Database.TelemetryApplication)_context.Entry(existing).GetDatabaseValues().ToObject();
			telemetryApplication.ApplyDTO(telemetryApplicationDTO);


			if (telemetryApplication.name != null && telemetryApplication.name.Length > 100)
			{
				telemetryApplication.name = telemetryApplication.name.Substring(0, 100);
			}

			if (telemetryApplication.url != null && telemetryApplication.url.Length > 500)
			{
				telemetryApplication.url = telemetryApplication.url.Substring(0, 500);
			}

			if (telemetryApplication.firstSeen.Kind != DateTimeKind.Utc)
			{
				telemetryApplication.firstSeen = telemetryApplication.firstSeen.ToUniversalTime();
			}

			if (telemetryApplication.lastSeen.HasValue == true && telemetryApplication.lastSeen.Value.Kind != DateTimeKind.Utc)
			{
				telemetryApplication.lastSeen = telemetryApplication.lastSeen.Value.ToUniversalTime();
			}

			EntityEntry<Database.TelemetryApplication> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(telemetryApplication);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Telemetry.TelemetryApplication entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryApplication.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryApplication.CreateAnonymousWithFirstLevelSubObjects(telemetryApplication)),
					null);


				return Ok(Database.TelemetryApplication.CreateAnonymous(telemetryApplication));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Telemetry.TelemetryApplication entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryApplication.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryApplication.CreateAnonymousWithFirstLevelSubObjects(telemetryApplication)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new TelemetryApplication record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryApplication", Name = "TelemetryApplication")]
		public async Task<IActionResult> PostTelemetryApplication([FromBody]Database.TelemetryApplication.TelemetryApplicationDTO telemetryApplicationDTO, CancellationToken cancellationToken = default)
		{
			if (telemetryApplicationDTO == null)
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
			// Create a new TelemetryApplication object using the data from the DTO
			//
			Database.TelemetryApplication telemetryApplication = Database.TelemetryApplication.FromDTO(telemetryApplicationDTO);

			try
			{
				if (telemetryApplication.name != null && telemetryApplication.name.Length > 100)
				{
					telemetryApplication.name = telemetryApplication.name.Substring(0, 100);
				}

				if (telemetryApplication.url != null && telemetryApplication.url.Length > 500)
				{
					telemetryApplication.url = telemetryApplication.url.Substring(0, 500);
				}

				if (telemetryApplication.firstSeen.Kind != DateTimeKind.Utc)
				{
					telemetryApplication.firstSeen = telemetryApplication.firstSeen.ToUniversalTime();
				}

				if (telemetryApplication.lastSeen.HasValue == true && telemetryApplication.lastSeen.Value.Kind != DateTimeKind.Utc)
				{
					telemetryApplication.lastSeen = telemetryApplication.lastSeen.Value.ToUniversalTime();
				}

				_context.TelemetryApplications.Add(telemetryApplication);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Telemetry.TelemetryApplication entity successfully created.",
					true,
					telemetryApplication.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.TelemetryApplication.CreateAnonymousWithFirstLevelSubObjects(telemetryApplication)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Telemetry.TelemetryApplication entity creation failed.", false, telemetryApplication.id.ToString(), "", JsonSerializer.Serialize(telemetryApplication), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TelemetryApplication", telemetryApplication.id, telemetryApplication.name));

			return CreatedAtRoute("TelemetryApplication", new { id = telemetryApplication.id }, Database.TelemetryApplication.CreateAnonymousWithFirstLevelSubObjects(telemetryApplication));
		}



        /// <summary>
        /// 
        /// This deletes a TelemetryApplication record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TelemetryApplication/{id}")]
		[Route("api/TelemetryApplication")]
		public async Task<IActionResult> DeleteTelemetryApplication(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.TelemetryApplication> query = (from x in _context.TelemetryApplications
				where
				(x.id == id)
				select x);


			Database.TelemetryApplication telemetryApplication = await query.FirstOrDefaultAsync(cancellationToken);

			if (telemetryApplication == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Telemetry.TelemetryApplication DELETE", id.ToString(), new Exception("No Telemetry.TelemetryApplication entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.TelemetryApplication cloneOfExisting = (Database.TelemetryApplication)_context.Entry(telemetryApplication).GetDatabaseValues().ToObject();


			try
			{
				_context.TelemetryApplications.Remove(telemetryApplication);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Telemetry.TelemetryApplication entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryApplication.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryApplication.CreateAnonymousWithFirstLevelSubObjects(telemetryApplication)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Telemetry.TelemetryApplication entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.TelemetryApplication.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TelemetryApplication.CreateAnonymousWithFirstLevelSubObjects(telemetryApplication)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of TelemetryApplication records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/TelemetryApplications/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string url = null,
			bool? isSelf = null,
			DateTime? firstSeen = null,
			DateTime? lastSeen = null,
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
			if (firstSeen.HasValue == true && firstSeen.Value.Kind != DateTimeKind.Utc)
			{
				firstSeen = firstSeen.Value.ToUniversalTime();
			}

			if (lastSeen.HasValue == true && lastSeen.Value.Kind != DateTimeKind.Utc)
			{
				lastSeen = lastSeen.Value.ToUniversalTime();
			}

			IQueryable<Database.TelemetryApplication> query = (from ta in _context.TelemetryApplications select ta);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ta => ta.name == name);
			}
			if (string.IsNullOrEmpty(url) == false)
			{
				query = query.Where(ta => ta.url == url);
			}
			if (isSelf.HasValue == true)
			{
				query = query.Where(ta => ta.isSelf == isSelf.Value);
			}
			if (firstSeen.HasValue == true)
			{
				query = query.Where(ta => ta.firstSeen == firstSeen.Value);
			}
			if (lastSeen.HasValue == true)
			{
				query = query.Where(ta => ta.lastSeen == lastSeen.Value);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Telemetry Application, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.url.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.name).ThenBy(x => x.url);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.TelemetryApplication.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/TelemetryApplication/CreateAuditEvent")]
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
