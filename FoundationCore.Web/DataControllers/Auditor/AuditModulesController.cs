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
    /// This auto generated class provides the basic CRUD operations for the AuditModule entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the AuditModule entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class AuditModulesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private AuditorContext _context;

		private ILogger<AuditModulesController> _logger;

		public AuditModulesController(AuditorContext context, ILogger<AuditModulesController> logger) : base("Auditor", "AuditModule")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of AuditModules filtered by the parameters provided.
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
		[Route("api/AuditModules")]
		public async Task<IActionResult> GetAuditModules(
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

			IQueryable<Database.AuditModule> query = (from am in _context.AuditModules select am);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(am => am.name == name);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(am => am.comments == comments);
			}
			if (firstAccess.HasValue == true)
			{
				query = query.Where(am => am.firstAccess == firstAccess.Value);
			}

			query = query.OrderBy(am => am.name).ThenBy(am => am.comments);


			//
			// Add the any string contains parameter to span all the string fields on the Audit Module, or on an any of the string fields on its immediate relations
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
			
			List<Database.AuditModule> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.AuditModule auditModule in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(auditModule, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Auditor.AuditModule Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Auditor.AuditModule Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of AuditModules filtered by the parameters provided.  Its query is similar to the GetAuditModules method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditModules/RowCount")]
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

			IQueryable<Database.AuditModule> query = (from am in _context.AuditModules select am);
			if (name != null)
			{
				query = query.Where(am => am.name == name);
			}
			if (comments != null)
			{
				query = query.Where(am => am.comments == comments);
			}
			if (firstAccess.HasValue == true)
			{
				query = query.Where(am => am.firstAccess == firstAccess.Value);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Audit Module, or on an any of the string fields on its immediate relations
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
        /// This gets a single AuditModule by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditModule/{id}")]
		public async Task<IActionResult> GetAuditModule(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.AuditModule> query = (from am in _context.AuditModules where
				(am.id == id)
					select am);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.AuditModule materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Auditor.AuditModule Entity was read with Admin privilege." : "Auditor.AuditModule Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AuditModule", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Auditor.AuditModule entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Auditor.AuditModule.   Entity was read with Admin privilege." : "Exception caught during entity read of Auditor.AuditModule.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing AuditModule record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/AuditModule/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutAuditModule(int id, [FromBody]Database.AuditModule.AuditModuleDTO auditModuleDTO, CancellationToken cancellationToken = default)
		{
			if (auditModuleDTO == null)
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



			if (id != auditModuleDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.AuditModule> query = (from x in _context.AuditModules
				where
				(x.id == id)
				select x);


			Database.AuditModule existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Auditor.AuditModule PUT", id.ToString(), new Exception("No Auditor.AuditModule entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.AuditModule cloneOfExisting = (Database.AuditModule)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new AuditModule object using the data from the existing record, updated with what is in the DTO.
			//
			Database.AuditModule auditModule = (Database.AuditModule)_context.Entry(existing).GetDatabaseValues().ToObject();
			auditModule.ApplyDTO(auditModuleDTO);


			if (auditModule.name != null && auditModule.name.Length > 500)
			{
				auditModule.name = auditModule.name.Substring(0, 500);
			}

			if (auditModule.comments != null && auditModule.comments.Length > 1000)
			{
				auditModule.comments = auditModule.comments.Substring(0, 1000);
			}

			if (auditModule.firstAccess.HasValue == true && auditModule.firstAccess.Value.Kind != DateTimeKind.Utc)
			{
				auditModule.firstAccess = auditModule.firstAccess.Value.ToUniversalTime();
			}

			EntityEntry<Database.AuditModule> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(auditModule);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Auditor.AuditModule entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditModule.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditModule.CreateAnonymousWithFirstLevelSubObjects(auditModule)),
					null);


				return Ok(Database.AuditModule.CreateAnonymous(auditModule));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Auditor.AuditModule entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditModule.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditModule.CreateAnonymousWithFirstLevelSubObjects(auditModule)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new AuditModule record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditModule", Name = "AuditModule")]
		public async Task<IActionResult> PostAuditModule([FromBody]Database.AuditModule.AuditModuleDTO auditModuleDTO, CancellationToken cancellationToken = default)
		{
			if (auditModuleDTO == null)
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
			// Create a new AuditModule object using the data from the DTO
			//
			Database.AuditModule auditModule = Database.AuditModule.FromDTO(auditModuleDTO);

			try
			{
				if (auditModule.name != null && auditModule.name.Length > 500)
				{
					auditModule.name = auditModule.name.Substring(0, 500);
				}

				if (auditModule.comments != null && auditModule.comments.Length > 1000)
				{
					auditModule.comments = auditModule.comments.Substring(0, 1000);
				}

				if (auditModule.firstAccess.HasValue == true && auditModule.firstAccess.Value.Kind != DateTimeKind.Utc)
				{
					auditModule.firstAccess = auditModule.firstAccess.Value.ToUniversalTime();
				}

				_context.AuditModules.Add(auditModule);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Auditor.AuditModule entity successfully created.",
					true,
					auditModule.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.AuditModule.CreateAnonymousWithFirstLevelSubObjects(auditModule)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Auditor.AuditModule entity creation failed.", false, auditModule.id.ToString(), "", JsonSerializer.Serialize(auditModule), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AuditModule", auditModule.id, auditModule.name));

			return CreatedAtRoute("AuditModule", new { id = auditModule.id }, Database.AuditModule.CreateAnonymousWithFirstLevelSubObjects(auditModule));
		}



        /// <summary>
        /// 
        /// This deletes a AuditModule record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditModule/{id}")]
		[Route("api/AuditModule")]
		public async Task<IActionResult> DeleteAuditModule(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.AuditModule> query = (from x in _context.AuditModules
				where
				(x.id == id)
				select x);


			Database.AuditModule auditModule = await query.FirstOrDefaultAsync(cancellationToken);

			if (auditModule == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Auditor.AuditModule DELETE", id.ToString(), new Exception("No Auditor.AuditModule entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.AuditModule cloneOfExisting = (Database.AuditModule)_context.Entry(auditModule).GetDatabaseValues().ToObject();


			try
			{
				_context.AuditModules.Remove(auditModule);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Auditor.AuditModule entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditModule.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditModule.CreateAnonymousWithFirstLevelSubObjects(auditModule)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Auditor.AuditModule entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditModule.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditModule.CreateAnonymousWithFirstLevelSubObjects(auditModule)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of AuditModule records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/AuditModules/ListData")]
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

			IQueryable<Database.AuditModule> query = (from am in _context.AuditModules select am);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(am => am.name == name);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(am => am.comments == comments);
			}
			if (firstAccess.HasValue == true)
			{
				query = query.Where(am => am.firstAccess == firstAccess.Value);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Audit Module, or on an any of the string fields on its immediate relations
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
			return Ok(await (from queryData in query select Database.AuditModule.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/AuditModule/CreateAuditEvent")]
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
