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
using static Foundation.Auditor.AuditEngine;
using Foundation.Security.Database;

namespace Foundation.Security.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the ModuleSecurityRole entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ModuleSecurityRole entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ModuleSecurityRolesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private SecurityContext _context;

		private ILogger<ModuleSecurityRolesController> _logger;

		public ModuleSecurityRolesController(SecurityContext context, ILogger<ModuleSecurityRolesController> logger) : base("Security", "ModuleSecurityRole")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ModuleSecurityRoles filtered by the parameters provided.
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
		[Route("api/ModuleSecurityRoles")]
		public async Task<IActionResult> GetModuleSecurityRoles(
			int? moduleId = null,
			int? securityRoleId = null,
			string comments = null,
			bool? active = null,
			bool? deleted = null,
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

			IQueryable<Database.ModuleSecurityRole> query = (from msr in _context.ModuleSecurityRoles select msr);
			if (moduleId.HasValue == true)
			{
				query = query.Where(msr => msr.moduleId == moduleId.Value);
			}
			if (securityRoleId.HasValue == true)
			{
				query = query.Where(msr => msr.securityRoleId == securityRoleId.Value);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(msr => msr.comments == comments);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(msr => msr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(msr => msr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(msr => msr.deleted == false);
				}
			}
			else
			{
				query = query.Where(msr => msr.active == true);
				query = query.Where(msr => msr.deleted == false);
			}

			query = query.OrderBy(msr => msr.comments);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.module);
				query = query.Include(x => x.securityRole);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Module Security Role, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.comments.Contains(anyStringContains)
			       || (includeRelations == true && x.module.name.Contains(anyStringContains))
			       || (includeRelations == true && x.module.description.Contains(anyStringContains))
			       || (includeRelations == true && x.securityRole.name.Contains(anyStringContains))
			       || (includeRelations == true && x.securityRole.description.Contains(anyStringContains))
			       || (includeRelations == true && x.securityRole.comments.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.ModuleSecurityRole> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ModuleSecurityRole moduleSecurityRole in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(moduleSecurityRole, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Security.ModuleSecurityRole Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Security.ModuleSecurityRole Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ModuleSecurityRoles filtered by the parameters provided.  Its query is similar to the GetModuleSecurityRoles method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModuleSecurityRoles/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? moduleId = null,
			int? securityRoleId = null,
			string comments = null,
			bool? active = null,
			bool? deleted = null,
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

			IQueryable<Database.ModuleSecurityRole> query = (from msr in _context.ModuleSecurityRoles select msr);
			if (moduleId.HasValue == true)
			{
				query = query.Where(msr => msr.moduleId == moduleId.Value);
			}
			if (securityRoleId.HasValue == true)
			{
				query = query.Where(msr => msr.securityRoleId == securityRoleId.Value);
			}
			if (comments != null)
			{
				query = query.Where(msr => msr.comments == comments);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(msr => msr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(msr => msr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(msr => msr.deleted == false);
				}
			}
			else
			{
				query = query.Where(msr => msr.active == true);
				query = query.Where(msr => msr.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Module Security Role, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.comments.Contains(anyStringContains)
			       || x.module.name.Contains(anyStringContains)
			       || x.module.description.Contains(anyStringContains)
			       || x.securityRole.name.Contains(anyStringContains)
			       || x.securityRole.description.Contains(anyStringContains)
			       || x.securityRole.comments.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ModuleSecurityRole by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModuleSecurityRole/{id}")]
		public async Task<IActionResult> GetModuleSecurityRole(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ModuleSecurityRole> query = (from msr in _context.ModuleSecurityRoles where
							(msr.id == id) &&
							(userIsAdmin == true || msr.deleted == false) &&
							(userIsWriter == true || msr.active == true)
					select msr);

				if (includeRelations == true)
				{
					query = query.Include(x => x.module);
					query = query.Include(x => x.securityRole);
					query = query.AsSplitQuery();
				}

				Database.ModuleSecurityRole materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Security.ModuleSecurityRole Entity was read with Admin privilege." : "Security.ModuleSecurityRole Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ModuleSecurityRole", materialized.id, materialized.comments));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Security.ModuleSecurityRole entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Security.ModuleSecurityRole.   Entity was read with Admin privilege." : "Exception caught during entity read of Security.ModuleSecurityRole.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ModuleSecurityRole record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ModuleSecurityRole/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutModuleSecurityRole(int id, [FromBody]Database.ModuleSecurityRole.ModuleSecurityRoleDTO moduleSecurityRoleDTO, CancellationToken cancellationToken = default)
		{
			if (moduleSecurityRoleDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != moduleSecurityRoleDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.ModuleSecurityRole> query = (from x in _context.ModuleSecurityRoles
				where
				(x.id == id)
				select x);


			Database.ModuleSecurityRole existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.ModuleSecurityRole PUT", id.ToString(), new Exception("No Security.ModuleSecurityRole entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ModuleSecurityRole cloneOfExisting = (Database.ModuleSecurityRole)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ModuleSecurityRole object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ModuleSecurityRole moduleSecurityRole = (Database.ModuleSecurityRole)_context.Entry(existing).GetDatabaseValues().ToObject();
			moduleSecurityRole.ApplyDTO(moduleSecurityRoleDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (moduleSecurityRole.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Security.ModuleSecurityRole record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (moduleSecurityRole.comments != null && moduleSecurityRole.comments.Length > 1000)
			{
				moduleSecurityRole.comments = moduleSecurityRole.comments.Substring(0, 1000);
			}

			EntityEntry<Database.ModuleSecurityRole> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(moduleSecurityRole);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Security.ModuleSecurityRole entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ModuleSecurityRole.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ModuleSecurityRole.CreateAnonymousWithFirstLevelSubObjects(moduleSecurityRole)),
					null);


				return Ok(Database.ModuleSecurityRole.CreateAnonymous(moduleSecurityRole));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Security.ModuleSecurityRole entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ModuleSecurityRole.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ModuleSecurityRole.CreateAnonymousWithFirstLevelSubObjects(moduleSecurityRole)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new ModuleSecurityRole record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModuleSecurityRole", Name = "ModuleSecurityRole")]
		public async Task<IActionResult> PostModuleSecurityRole([FromBody]Database.ModuleSecurityRole.ModuleSecurityRoleDTO moduleSecurityRoleDTO, CancellationToken cancellationToken = default)
		{
			if (moduleSecurityRoleDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new ModuleSecurityRole object using the data from the DTO
			//
			Database.ModuleSecurityRole moduleSecurityRole = Database.ModuleSecurityRole.FromDTO(moduleSecurityRoleDTO);

			try
			{
				if (moduleSecurityRole.comments != null && moduleSecurityRole.comments.Length > 1000)
				{
					moduleSecurityRole.comments = moduleSecurityRole.comments.Substring(0, 1000);
				}

				_context.ModuleSecurityRoles.Add(moduleSecurityRole);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Security.ModuleSecurityRole entity successfully created.",
					true,
					moduleSecurityRole.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ModuleSecurityRole.CreateAnonymousWithFirstLevelSubObjects(moduleSecurityRole)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Security.ModuleSecurityRole entity creation failed.", false, moduleSecurityRole.id.ToString(), "", JsonSerializer.Serialize(moduleSecurityRole), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ModuleSecurityRole", moduleSecurityRole.id, moduleSecurityRole.comments));

			return CreatedAtRoute("ModuleSecurityRole", new { id = moduleSecurityRole.id }, Database.ModuleSecurityRole.CreateAnonymousWithFirstLevelSubObjects(moduleSecurityRole));
		}



        /// <summary>
        /// 
        /// This deletes a ModuleSecurityRole record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModuleSecurityRole/{id}")]
		[Route("api/ModuleSecurityRole")]
		public async Task<IActionResult> DeleteModuleSecurityRole(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.ModuleSecurityRole> query = (from x in _context.ModuleSecurityRoles
				where
				(x.id == id)
				select x);


			Database.ModuleSecurityRole moduleSecurityRole = await query.FirstOrDefaultAsync(cancellationToken);

			if (moduleSecurityRole == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.ModuleSecurityRole DELETE", id.ToString(), new Exception("No Security.ModuleSecurityRole entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ModuleSecurityRole cloneOfExisting = (Database.ModuleSecurityRole)_context.Entry(moduleSecurityRole).GetDatabaseValues().ToObject();


			try
			{
				moduleSecurityRole.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.ModuleSecurityRole entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ModuleSecurityRole.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ModuleSecurityRole.CreateAnonymousWithFirstLevelSubObjects(moduleSecurityRole)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.ModuleSecurityRole entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ModuleSecurityRole.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ModuleSecurityRole.CreateAnonymousWithFirstLevelSubObjects(moduleSecurityRole)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of ModuleSecurityRole records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ModuleSecurityRoles/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? moduleId = null,
			int? securityRoleId = null,
			string comments = null,
			bool? active = null,
			bool? deleted = null,
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

			IQueryable<Database.ModuleSecurityRole> query = (from msr in _context.ModuleSecurityRoles select msr);
			if (moduleId.HasValue == true)
			{
				query = query.Where(msr => msr.moduleId == moduleId.Value);
			}
			if (securityRoleId.HasValue == true)
			{
				query = query.Where(msr => msr.securityRoleId == securityRoleId.Value);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(msr => msr.comments == comments);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(msr => msr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(msr => msr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(msr => msr.deleted == false);
				}
			}
			else
			{
				query = query.Where(msr => msr.active == true);
				query = query.Where(msr => msr.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Module Security Role, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.comments.Contains(anyStringContains)
			       || x.module.name.Contains(anyStringContains)
			       || x.module.description.Contains(anyStringContains)
			       || x.securityRole.name.Contains(anyStringContains)
			       || x.securityRole.description.Contains(anyStringContains)
			       || x.securityRole.comments.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.comments);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ModuleSecurityRole.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ModuleSecurityRole/CreateAuditEvent")]
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
