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
    /// This auto generated class provides the basic CRUD operations for the AuditHostSystem entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the AuditHostSystem entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class AuditHostSystemsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private AuditorContext _context;

		private ILogger<AuditHostSystemsController> _logger;

		public AuditHostSystemsController(AuditorContext context, ILogger<AuditHostSystemsController> logger) : base("Auditor", "AuditHostSystem")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of AuditHostSystems filtered by the parameters provided.
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
		[Route("api/AuditHostSystems")]
		public async Task<IActionResult> GetAuditHostSystems(
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

			IQueryable<Database.AuditHostSystem> query = (from ahs in _context.AuditHostSystems select ahs);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ahs => ahs.name == name);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(ahs => ahs.comments == comments);
			}
			if (firstAccess.HasValue == true)
			{
				query = query.Where(ahs => ahs.firstAccess == firstAccess.Value);
			}

			query = query.OrderBy(ahs => ahs.name).ThenBy(ahs => ahs.comments);

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
			// Add the any string contains parameter to span all the string fields on the Audit Host System, or on an any of the string fields on its immediate relations
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
			
			List<Database.AuditHostSystem> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.AuditHostSystem auditHostSystem in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(auditHostSystem, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Auditor.AuditHostSystem Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Auditor.AuditHostSystem Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of AuditHostSystems filtered by the parameters provided.  Its query is similar to the GetAuditHostSystems method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditHostSystems/RowCount")]
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

			IQueryable<Database.AuditHostSystem> query = (from ahs in _context.AuditHostSystems select ahs);
			if (name != null)
			{
				query = query.Where(ahs => ahs.name == name);
			}
			if (comments != null)
			{
				query = query.Where(ahs => ahs.comments == comments);
			}
			if (firstAccess.HasValue == true)
			{
				query = query.Where(ahs => ahs.firstAccess == firstAccess.Value);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Audit Host System, or on an any of the string fields on its immediate relations
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
        /// This gets a single AuditHostSystem by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditHostSystem/{id}")]
		public async Task<IActionResult> GetAuditHostSystem(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.AuditHostSystem> query = (from ahs in _context.AuditHostSystems where
				(ahs.id == id)
					select ahs);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.AuditHostSystem materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Auditor.AuditHostSystem Entity was read with Admin privilege." : "Auditor.AuditHostSystem Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AuditHostSystem", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Auditor.AuditHostSystem entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Auditor.AuditHostSystem.   Entity was read with Admin privilege." : "Exception caught during entity read of Auditor.AuditHostSystem.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing AuditHostSystem record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/AuditHostSystem/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutAuditHostSystem(int id, [FromBody]Database.AuditHostSystem.AuditHostSystemDTO auditHostSystemDTO, CancellationToken cancellationToken = default)
		{
			if (auditHostSystemDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			// Admin privilege needed to write to this table.
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != auditHostSystemDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.AuditHostSystem> query = (from x in _context.AuditHostSystems
				where
				(x.id == id)
				select x);


			Database.AuditHostSystem existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Auditor.AuditHostSystem PUT", id.ToString(), new Exception("No Auditor.AuditHostSystem entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.AuditHostSystem cloneOfExisting = (Database.AuditHostSystem)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new AuditHostSystem object using the data from the existing record, updated with what is in the DTO.
			//
			Database.AuditHostSystem auditHostSystem = (Database.AuditHostSystem)_context.Entry(existing).GetDatabaseValues().ToObject();
			auditHostSystem.ApplyDTO(auditHostSystemDTO);


			if (auditHostSystem.name != null && auditHostSystem.name.Length > 500)
			{
				auditHostSystem.name = auditHostSystem.name.Substring(0, 500);
			}

			if (auditHostSystem.comments != null && auditHostSystem.comments.Length > 1000)
			{
				auditHostSystem.comments = auditHostSystem.comments.Substring(0, 1000);
			}

			if (auditHostSystem.firstAccess.HasValue == true && auditHostSystem.firstAccess.Value.Kind != DateTimeKind.Utc)
			{
				auditHostSystem.firstAccess = auditHostSystem.firstAccess.Value.ToUniversalTime();
			}

			EntityEntry<Database.AuditHostSystem> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(auditHostSystem);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Auditor.AuditHostSystem entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditHostSystem.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditHostSystem.CreateAnonymousWithFirstLevelSubObjects(auditHostSystem)),
					null);


				return Ok(Database.AuditHostSystem.CreateAnonymous(auditHostSystem));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Auditor.AuditHostSystem entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditHostSystem.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditHostSystem.CreateAnonymousWithFirstLevelSubObjects(auditHostSystem)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new AuditHostSystem record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditHostSystem", Name = "AuditHostSystem")]
		public async Task<IActionResult> PostAuditHostSystem([FromBody]Database.AuditHostSystem.AuditHostSystemDTO auditHostSystemDTO, CancellationToken cancellationToken = default)
		{
			if (auditHostSystemDTO == null)
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
			// Create a new AuditHostSystem object using the data from the DTO
			//
			Database.AuditHostSystem auditHostSystem = Database.AuditHostSystem.FromDTO(auditHostSystemDTO);

			try
			{
				if (auditHostSystem.name != null && auditHostSystem.name.Length > 500)
				{
					auditHostSystem.name = auditHostSystem.name.Substring(0, 500);
				}

				if (auditHostSystem.comments != null && auditHostSystem.comments.Length > 1000)
				{
					auditHostSystem.comments = auditHostSystem.comments.Substring(0, 1000);
				}

				if (auditHostSystem.firstAccess.HasValue == true && auditHostSystem.firstAccess.Value.Kind != DateTimeKind.Utc)
				{
					auditHostSystem.firstAccess = auditHostSystem.firstAccess.Value.ToUniversalTime();
				}

				_context.AuditHostSystems.Add(auditHostSystem);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Auditor.AuditHostSystem entity successfully created.",
					true,
					auditHostSystem.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.AuditHostSystem.CreateAnonymousWithFirstLevelSubObjects(auditHostSystem)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Auditor.AuditHostSystem entity creation failed.", false, auditHostSystem.id.ToString(), "", JsonSerializer.Serialize(auditHostSystem), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AuditHostSystem", auditHostSystem.id, auditHostSystem.name));

			return CreatedAtRoute("AuditHostSystem", new { id = auditHostSystem.id }, Database.AuditHostSystem.CreateAnonymousWithFirstLevelSubObjects(auditHostSystem));
		}



        /// <summary>
        /// 
        /// This deletes a AuditHostSystem record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditHostSystem/{id}")]
		[Route("api/AuditHostSystem")]
		public async Task<IActionResult> DeleteAuditHostSystem(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.AuditHostSystem> query = (from x in _context.AuditHostSystems
				where
				(x.id == id)
				select x);


			Database.AuditHostSystem auditHostSystem = await query.FirstOrDefaultAsync(cancellationToken);

			if (auditHostSystem == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Auditor.AuditHostSystem DELETE", id.ToString(), new Exception("No Auditor.AuditHostSystem entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.AuditHostSystem cloneOfExisting = (Database.AuditHostSystem)_context.Entry(auditHostSystem).GetDatabaseValues().ToObject();


			try
			{
				_context.AuditHostSystems.Remove(auditHostSystem);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Auditor.AuditHostSystem entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditHostSystem.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditHostSystem.CreateAnonymousWithFirstLevelSubObjects(auditHostSystem)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Auditor.AuditHostSystem entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditHostSystem.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditHostSystem.CreateAnonymousWithFirstLevelSubObjects(auditHostSystem)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of AuditHostSystem records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/AuditHostSystems/ListData")]
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

			IQueryable<Database.AuditHostSystem> query = (from ahs in _context.AuditHostSystems select ahs);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ahs => ahs.name == name);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(ahs => ahs.comments == comments);
			}
			if (firstAccess.HasValue == true)
			{
				query = query.Where(ahs => ahs.firstAccess == firstAccess.Value);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Audit Host System, or on an any of the string fields on its immediate relations
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
			return Ok(await (from queryData in query select Database.AuditHostSystem.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/AuditHostSystem/CreateAuditEvent")]
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
