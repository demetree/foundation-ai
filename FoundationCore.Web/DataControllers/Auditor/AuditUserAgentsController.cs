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
    /// This auto generated class provides the basic CRUD operations for the AuditUserAgent entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the AuditUserAgent entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class AuditUserAgentsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private AuditorContext _context;

		private ILogger<AuditUserAgentsController> _logger;

		public AuditUserAgentsController(AuditorContext context, ILogger<AuditUserAgentsController> logger) : base("Auditor", "AuditUserAgent")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of AuditUserAgents filtered by the parameters provided.
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
		[Route("api/AuditUserAgents")]
		public async Task<IActionResult> GetAuditUserAgents(
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

			//
			// Auditor Reader role or better needed to read from this table, as well as the minimum read permission level.
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
			if (firstAccess.HasValue == true && firstAccess.Value.Kind != DateTimeKind.Utc)
			{
				firstAccess = firstAccess.Value.ToUniversalTime();
			}

			IQueryable<Database.AuditUserAgent> query = (from aua in _context.AuditUserAgents select aua);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(aua => aua.name == name);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(aua => aua.comments == comments);
			}
			if (firstAccess.HasValue == true)
			{
				query = query.Where(aua => aua.firstAccess == firstAccess.Value);
			}

			query = query.OrderBy(aua => aua.name).ThenBy(aua => aua.comments);

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
			// Add the any string contains parameter to span all the string fields on the Audit User Agent, or on an any of the string fields on its immediate relations
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
			
			List<Database.AuditUserAgent> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.AuditUserAgent auditUserAgent in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(auditUserAgent, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Auditor.AuditUserAgent Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Auditor.AuditUserAgent Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of AuditUserAgents filtered by the parameters provided.  Its query is similar to the GetAuditUserAgents method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditUserAgents/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string comments = null,
			DateTime? firstAccess = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Auditor Reader role or better needed to read from this table, as well as the minimum read permission level.
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
			if (firstAccess.HasValue == true && firstAccess.Value.Kind != DateTimeKind.Utc)
			{
				firstAccess = firstAccess.Value.ToUniversalTime();
			}

			IQueryable<Database.AuditUserAgent> query = (from aua in _context.AuditUserAgents select aua);
			if (name != null)
			{
				query = query.Where(aua => aua.name == name);
			}
			if (comments != null)
			{
				query = query.Where(aua => aua.comments == comments);
			}
			if (firstAccess.HasValue == true)
			{
				query = query.Where(aua => aua.firstAccess == firstAccess.Value);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Audit User Agent, or on an any of the string fields on its immediate relations
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
        /// This gets a single AuditUserAgent by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditUserAgent/{id}")]
		public async Task<IActionResult> GetAuditUserAgent(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Auditor Reader role or better needed to read from this table, as well as the minimum read permission level.
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
				IQueryable<Database.AuditUserAgent> query = (from aua in _context.AuditUserAgents where
				(aua.id == id)
					select aua);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.AuditUserAgent materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Auditor.AuditUserAgent Entity was read with Admin privilege." : "Auditor.AuditUserAgent Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AuditUserAgent", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Auditor.AuditUserAgent entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Auditor.AuditUserAgent.   Entity was read with Admin privilege." : "Exception caught during entity read of Auditor.AuditUserAgent.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing AuditUserAgent record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/AuditUserAgent/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutAuditUserAgent(int id, [FromBody]Database.AuditUserAgent.AuditUserAgentDTO auditUserAgentDTO, CancellationToken cancellationToken = default)
		{
			if (auditUserAgentDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Auditor Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != auditUserAgentDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.AuditUserAgent> query = (from x in _context.AuditUserAgents
				where
				(x.id == id)
				select x);


			Database.AuditUserAgent existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Auditor.AuditUserAgent PUT", id.ToString(), new Exception("No Auditor.AuditUserAgent entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.AuditUserAgent cloneOfExisting = (Database.AuditUserAgent)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new AuditUserAgent object using the data from the existing record, updated with what is in the DTO.
			//
			Database.AuditUserAgent auditUserAgent = (Database.AuditUserAgent)_context.Entry(existing).GetDatabaseValues().ToObject();
			auditUserAgent.ApplyDTO(auditUserAgentDTO);


			if (auditUserAgent.name != null && auditUserAgent.name.Length > 500)
			{
				auditUserAgent.name = auditUserAgent.name.Substring(0, 500);
			}

			if (auditUserAgent.comments != null && auditUserAgent.comments.Length > 1000)
			{
				auditUserAgent.comments = auditUserAgent.comments.Substring(0, 1000);
			}

			if (auditUserAgent.firstAccess.HasValue == true && auditUserAgent.firstAccess.Value.Kind != DateTimeKind.Utc)
			{
				auditUserAgent.firstAccess = auditUserAgent.firstAccess.Value.ToUniversalTime();
			}

			EntityEntry<Database.AuditUserAgent> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(auditUserAgent);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Auditor.AuditUserAgent entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditUserAgent.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditUserAgent.CreateAnonymousWithFirstLevelSubObjects(auditUserAgent)),
					null);


				return Ok(Database.AuditUserAgent.CreateAnonymous(auditUserAgent));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Auditor.AuditUserAgent entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditUserAgent.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditUserAgent.CreateAnonymousWithFirstLevelSubObjects(auditUserAgent)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new AuditUserAgent record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditUserAgent", Name = "AuditUserAgent")]
		public async Task<IActionResult> PostAuditUserAgent([FromBody]Database.AuditUserAgent.AuditUserAgentDTO auditUserAgentDTO, CancellationToken cancellationToken = default)
		{
			if (auditUserAgentDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Auditor Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new AuditUserAgent object using the data from the DTO
			//
			Database.AuditUserAgent auditUserAgent = Database.AuditUserAgent.FromDTO(auditUserAgentDTO);

			try
			{
				if (auditUserAgent.name != null && auditUserAgent.name.Length > 500)
				{
					auditUserAgent.name = auditUserAgent.name.Substring(0, 500);
				}

				if (auditUserAgent.comments != null && auditUserAgent.comments.Length > 1000)
				{
					auditUserAgent.comments = auditUserAgent.comments.Substring(0, 1000);
				}

				if (auditUserAgent.firstAccess.HasValue == true && auditUserAgent.firstAccess.Value.Kind != DateTimeKind.Utc)
				{
					auditUserAgent.firstAccess = auditUserAgent.firstAccess.Value.ToUniversalTime();
				}

				_context.AuditUserAgents.Add(auditUserAgent);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Auditor.AuditUserAgent entity successfully created.",
					true,
					auditUserAgent.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.AuditUserAgent.CreateAnonymousWithFirstLevelSubObjects(auditUserAgent)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Auditor.AuditUserAgent entity creation failed.", false, auditUserAgent.id.ToString(), "", JsonSerializer.Serialize(auditUserAgent), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AuditUserAgent", auditUserAgent.id, auditUserAgent.name));

			return CreatedAtRoute("AuditUserAgent", new { id = auditUserAgent.id }, Database.AuditUserAgent.CreateAnonymousWithFirstLevelSubObjects(auditUserAgent));
		}



        /// <summary>
        /// 
        /// This deletes a AuditUserAgent record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditUserAgent/{id}")]
		[Route("api/AuditUserAgent")]
		public async Task<IActionResult> DeleteAuditUserAgent(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Auditor Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.AuditUserAgent> query = (from x in _context.AuditUserAgents
				where
				(x.id == id)
				select x);


			Database.AuditUserAgent auditUserAgent = await query.FirstOrDefaultAsync(cancellationToken);

			if (auditUserAgent == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Auditor.AuditUserAgent DELETE", id.ToString(), new Exception("No Auditor.AuditUserAgent entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.AuditUserAgent cloneOfExisting = (Database.AuditUserAgent)_context.Entry(auditUserAgent).GetDatabaseValues().ToObject();


			try
			{
				_context.AuditUserAgents.Remove(auditUserAgent);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Auditor.AuditUserAgent entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditUserAgent.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditUserAgent.CreateAnonymousWithFirstLevelSubObjects(auditUserAgent)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Auditor.AuditUserAgent entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditUserAgent.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditUserAgent.CreateAnonymousWithFirstLevelSubObjects(auditUserAgent)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of AuditUserAgent records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/AuditUserAgents/ListData")]
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
			//
			// Auditor Reader role or better needed to read from this table, as well as the minimum read permission level.
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
			if (firstAccess.HasValue == true && firstAccess.Value.Kind != DateTimeKind.Utc)
			{
				firstAccess = firstAccess.Value.ToUniversalTime();
			}

			IQueryable<Database.AuditUserAgent> query = (from aua in _context.AuditUserAgents select aua);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(aua => aua.name == name);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(aua => aua.comments == comments);
			}
			if (firstAccess.HasValue == true)
			{
				query = query.Where(aua => aua.firstAccess == firstAccess.Value);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Audit User Agent, or on an any of the string fields on its immediate relations
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
			return Ok(await (from queryData in query select Database.AuditUserAgent.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/AuditUserAgent/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Auditor Administrator role needed to write to this table.
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
