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
using Foundation.Auditor.Database;

namespace Foundation.Auditor.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the AuditSession entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the AuditSession entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class AuditSessionsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private AuditorContext _context;

		private ILogger<AuditSessionsController> _logger;

		public AuditSessionsController(AuditorContext context, ILogger<AuditSessionsController> logger) : base("Auditor", "AuditSession")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of AuditSessions filtered by the parameters provided.
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
		[Route("api/AuditSessions")]
		public async Task<IActionResult> GetAuditSessions(
			string name = null,
			string comments = null,
			DateTime? firstAccess = null,
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
			if (firstAccess.HasValue == true && firstAccess.Value.Kind != DateTimeKind.Utc)
			{
				firstAccess = firstAccess.Value.ToUniversalTime();
			}

			IQueryable<Database.AuditSession> query = (from _as in _context.AuditSessions select _as);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(_as => _as.name == name);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(_as => _as.comments == comments);
			}
			if (firstAccess.HasValue == true)
			{
				query = query.Where(_as => _as.firstAccess == firstAccess.Value);
			}

			query = query.OrderBy(_as => _as.name).ThenBy(_as => _as.comments);

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
			// Add the any string contains parameter to span all the string fields on the Audit Session, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.comments.Contains(anyStringContains)
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.AuditSession> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.AuditSession auditSession in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(auditSession, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Auditor.AuditSession Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Auditor.AuditSession Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of AuditSessions filtered by the parameters provided.  Its query is similar to the GetAuditSessions method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditSessions/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string comments = null,
			DateTime? firstAccess = null,
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
			if (firstAccess.HasValue == true && firstAccess.Value.Kind != DateTimeKind.Utc)
			{
				firstAccess = firstAccess.Value.ToUniversalTime();
			}

			IQueryable<Database.AuditSession> query = (from _as in _context.AuditSessions select _as);
			if (name != null)
			{
				query = query.Where(_as => _as.name == name);
			}
			if (comments != null)
			{
				query = query.Where(_as => _as.comments == comments);
			}
			if (firstAccess.HasValue == true)
			{
				query = query.Where(_as => _as.firstAccess == firstAccess.Value);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Audit Session, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.comments.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single AuditSession by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditSession/{id}")]
		public async Task<IActionResult> GetAuditSession(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.AuditSession> query = (from _as in _context.AuditSessions where
				(_as.id == id)
					select _as);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.AuditSession materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Auditor.AuditSession Entity was read with Admin privilege." : "Auditor.AuditSession Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AuditSession", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Auditor.AuditSession entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Auditor.AuditSession.   Entity was read with Admin privilege." : "Exception caught during entity read of Auditor.AuditSession.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing AuditSession record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/AuditSession/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutAuditSession(int id, [FromBody]Database.AuditSession.AuditSessionDTO auditSessionDTO, CancellationToken cancellationToken = default)
		{
			if (auditSessionDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			// Admin privilege needed to write to this table.
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != auditSessionDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.AuditSession> query = (from x in _context.AuditSessions
				where
				(x.id == id)
				select x);


			Database.AuditSession existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Auditor.AuditSession PUT", id.ToString(), new Exception("No Auditor.AuditSession entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.AuditSession cloneOfExisting = (Database.AuditSession)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new AuditSession object using the data from the existing record, updated with what is in the DTO.
			//
			Database.AuditSession auditSession = (Database.AuditSession)_context.Entry(existing).GetDatabaseValues().ToObject();
			auditSession.ApplyDTO(auditSessionDTO);


			if (auditSession.name != null && auditSession.name.Length > 500)
			{
				auditSession.name = auditSession.name.Substring(0, 500);
			}

			if (auditSession.comments != null && auditSession.comments.Length > 1000)
			{
				auditSession.comments = auditSession.comments.Substring(0, 1000);
			}

			if (auditSession.firstAccess.HasValue == true && auditSession.firstAccess.Value.Kind != DateTimeKind.Utc)
			{
				auditSession.firstAccess = auditSession.firstAccess.Value.ToUniversalTime();
			}

			EntityEntry<Database.AuditSession> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(auditSession);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Auditor.AuditSession entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditSession.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditSession.CreateAnonymousWithFirstLevelSubObjects(auditSession)),
					null);


				return Ok(Database.AuditSession.CreateAnonymous(auditSession));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Auditor.AuditSession entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditSession.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditSession.CreateAnonymousWithFirstLevelSubObjects(auditSession)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new AuditSession record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditSession", Name = "AuditSession")]
		public async Task<IActionResult> PostAuditSession([FromBody]Database.AuditSession.AuditSessionDTO auditSessionDTO, CancellationToken cancellationToken = default)
		{
			if (auditSessionDTO == null)
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
			// Create a new AuditSession object using the data from the DTO
			//
			Database.AuditSession auditSession = Database.AuditSession.FromDTO(auditSessionDTO);

			try
			{
				if (auditSession.name != null && auditSession.name.Length > 500)
				{
					auditSession.name = auditSession.name.Substring(0, 500);
				}

				if (auditSession.comments != null && auditSession.comments.Length > 1000)
				{
					auditSession.comments = auditSession.comments.Substring(0, 1000);
				}

				if (auditSession.firstAccess.HasValue == true && auditSession.firstAccess.Value.Kind != DateTimeKind.Utc)
				{
					auditSession.firstAccess = auditSession.firstAccess.Value.ToUniversalTime();
				}

				_context.AuditSessions.Add(auditSession);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Auditor.AuditSession entity successfully created.",
					true,
					auditSession.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.AuditSession.CreateAnonymousWithFirstLevelSubObjects(auditSession)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Auditor.AuditSession entity creation failed.", false, auditSession.id.ToString(), "", JsonSerializer.Serialize(auditSession), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AuditSession", auditSession.id, auditSession.name));

			return CreatedAtRoute("AuditSession", new { id = auditSession.id }, Database.AuditSession.CreateAnonymousWithFirstLevelSubObjects(auditSession));
		}



        /// <summary>
        /// 
        /// This deletes a AuditSession record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditSession/{id}")]
		[Route("api/AuditSession")]
		public async Task<IActionResult> DeleteAuditSession(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.AuditSession> query = (from x in _context.AuditSessions
				where
				(x.id == id)
				select x);


			Database.AuditSession auditSession = await query.FirstOrDefaultAsync(cancellationToken);

			if (auditSession == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Auditor.AuditSession DELETE", id.ToString(), new Exception("No Auditor.AuditSession entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.AuditSession cloneOfExisting = (Database.AuditSession)_context.Entry(auditSession).GetDatabaseValues().ToObject();


			try
			{
				_context.AuditSessions.Remove(auditSession);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Auditor.AuditSession entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditSession.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditSession.CreateAnonymousWithFirstLevelSubObjects(auditSession)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Auditor.AuditSession entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditSession.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditSession.CreateAnonymousWithFirstLevelSubObjects(auditSession)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of AuditSession records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/AuditSessions/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string comments = null,
			DateTime? firstAccess = null,
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
			if (firstAccess.HasValue == true && firstAccess.Value.Kind != DateTimeKind.Utc)
			{
				firstAccess = firstAccess.Value.ToUniversalTime();
			}

			IQueryable<Database.AuditSession> query = (from _as in _context.AuditSessions select _as);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(_as => _as.name == name);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(_as => _as.comments == comments);
			}
			if (firstAccess.HasValue == true)
			{
				query = query.Where(_as => _as.firstAccess == firstAccess.Value);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Audit Session, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.comments.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.name).ThenBy(x => x.comments);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.AuditSession.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/AuditSession/CreateAuditEvent")]
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
