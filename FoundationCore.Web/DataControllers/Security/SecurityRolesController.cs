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
    /// This auto generated class provides the basic CRUD operations for the SecurityRole entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SecurityRole entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SecurityRolesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private SecurityContext _context;

		private ILogger<SecurityRolesController> _logger;

		public SecurityRolesController(SecurityContext context, ILogger<SecurityRolesController> logger) : base("Security", "SecurityRole")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of SecurityRoles filtered by the parameters provided.
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
		[Route("api/SecurityRoles")]
		public async Task<IActionResult> GetSecurityRoles(
			int? privilegeId = null,
			string name = null,
			string description = null,
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

			IQueryable<Database.SecurityRole> query = (from sr in _context.SecurityRoles select sr);
			if (privilegeId.HasValue == true)
			{
				query = query.Where(sr => sr.privilegeId == privilegeId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(sr => sr.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(sr => sr.description == description);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(sr => sr.comments == comments);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sr => sr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sr => sr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sr => sr.deleted == false);
				}
			}
			else
			{
				query = query.Where(sr => sr.active == true);
				query = query.Where(sr => sr.deleted == false);
			}

			query = query.OrderBy(sr => sr.name).ThenBy(sr => sr.description).ThenBy(sr => sr.comments);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.privilege);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Security Role, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.comments.Contains(anyStringContains)
			       || (includeRelations == true && x.privilege.name.Contains(anyStringContains))
			       || (includeRelations == true && x.privilege.description.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.SecurityRole> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SecurityRole securityRole in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(securityRole, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Security.SecurityRole Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Security.SecurityRole Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of SecurityRoles filtered by the parameters provided.  Its query is similar to the GetSecurityRoles method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityRoles/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? privilegeId = null,
			string name = null,
			string description = null,
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

			IQueryable<Database.SecurityRole> query = (from sr in _context.SecurityRoles select sr);
			if (privilegeId.HasValue == true)
			{
				query = query.Where(sr => sr.privilegeId == privilegeId.Value);
			}
			if (name != null)
			{
				query = query.Where(sr => sr.name == name);
			}
			if (description != null)
			{
				query = query.Where(sr => sr.description == description);
			}
			if (comments != null)
			{
				query = query.Where(sr => sr.comments == comments);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sr => sr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sr => sr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sr => sr.deleted == false);
				}
			}
			else
			{
				query = query.Where(sr => sr.active == true);
				query = query.Where(sr => sr.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Security Role, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.comments.Contains(anyStringContains)
			       || x.privilege.name.Contains(anyStringContains)
			       || x.privilege.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single SecurityRole by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityRole/{id}")]
		public async Task<IActionResult> GetSecurityRole(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.SecurityRole> query = (from sr in _context.SecurityRoles where
							(sr.id == id) &&
							(userIsAdmin == true || sr.deleted == false) &&
							(userIsWriter == true || sr.active == true)
					select sr);

				if (includeRelations == true)
				{
					query = query.Include(x => x.privilege);
					query = query.AsSplitQuery();
				}

				Database.SecurityRole materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Security.SecurityRole Entity was read with Admin privilege." : "Security.SecurityRole Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityRole", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Security.SecurityRole entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Security.SecurityRole.   Entity was read with Admin privilege." : "Exception caught during entity read of Security.SecurityRole.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing SecurityRole record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SecurityRole/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSecurityRole(int id, [FromBody]Database.SecurityRole.SecurityRoleDTO securityRoleDTO, CancellationToken cancellationToken = default)
		{
			if (securityRoleDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != securityRoleDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.SecurityRole> query = (from x in _context.SecurityRoles
				where
				(x.id == id)
				select x);


			Database.SecurityRole existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityRole PUT", id.ToString(), new Exception("No Security.SecurityRole entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SecurityRole cloneOfExisting = (Database.SecurityRole)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SecurityRole object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SecurityRole securityRole = (Database.SecurityRole)_context.Entry(existing).GetDatabaseValues().ToObject();
			securityRole.ApplyDTO(securityRoleDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (securityRole.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Security.SecurityRole record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (securityRole.name != null && securityRole.name.Length > 100)
			{
				securityRole.name = securityRole.name.Substring(0, 100);
			}

			if (securityRole.description != null && securityRole.description.Length > 500)
			{
				securityRole.description = securityRole.description.Substring(0, 500);
			}

			if (securityRole.comments != null && securityRole.comments.Length > 1000)
			{
				securityRole.comments = securityRole.comments.Substring(0, 1000);
			}

			EntityEntry<Database.SecurityRole> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(securityRole);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityRole entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityRole.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityRole.CreateAnonymousWithFirstLevelSubObjects(securityRole)),
					null);


				return Ok(Database.SecurityRole.CreateAnonymous(securityRole));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityRole entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityRole.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityRole.CreateAnonymousWithFirstLevelSubObjects(securityRole)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new SecurityRole record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityRole", Name = "SecurityRole")]
		public async Task<IActionResult> PostSecurityRole([FromBody]Database.SecurityRole.SecurityRoleDTO securityRoleDTO, CancellationToken cancellationToken = default)
		{
			if (securityRoleDTO == null)
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
			// Create a new SecurityRole object using the data from the DTO
			//
			Database.SecurityRole securityRole = Database.SecurityRole.FromDTO(securityRoleDTO);

			try
			{
				if (securityRole.name != null && securityRole.name.Length > 100)
				{
					securityRole.name = securityRole.name.Substring(0, 100);
				}

				if (securityRole.description != null && securityRole.description.Length > 500)
				{
					securityRole.description = securityRole.description.Substring(0, 500);
				}

				if (securityRole.comments != null && securityRole.comments.Length > 1000)
				{
					securityRole.comments = securityRole.comments.Substring(0, 1000);
				}

				_context.SecurityRoles.Add(securityRole);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Security.SecurityRole entity successfully created.",
					true,
					securityRole.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.SecurityRole.CreateAnonymousWithFirstLevelSubObjects(securityRole)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Security.SecurityRole entity creation failed.", false, securityRole.id.ToString(), "", JsonSerializer.Serialize(securityRole), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityRole", securityRole.id, securityRole.name));

			return CreatedAtRoute("SecurityRole", new { id = securityRole.id }, Database.SecurityRole.CreateAnonymousWithFirstLevelSubObjects(securityRole));
		}



        /// <summary>
        /// 
        /// This deletes a SecurityRole record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityRole/{id}")]
		[Route("api/SecurityRole")]
		public async Task<IActionResult> DeleteSecurityRole(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.SecurityRole> query = (from x in _context.SecurityRoles
				where
				(x.id == id)
				select x);


			Database.SecurityRole securityRole = await query.FirstOrDefaultAsync(cancellationToken);

			if (securityRole == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityRole DELETE", id.ToString(), new Exception("No Security.SecurityRole entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SecurityRole cloneOfExisting = (Database.SecurityRole)_context.Entry(securityRole).GetDatabaseValues().ToObject();


			try
			{
				securityRole.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityRole entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityRole.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityRole.CreateAnonymousWithFirstLevelSubObjects(securityRole)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityRole entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityRole.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityRole.CreateAnonymousWithFirstLevelSubObjects(securityRole)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of SecurityRole records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SecurityRoles/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? privilegeId = null,
			string name = null,
			string description = null,
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

			IQueryable<Database.SecurityRole> query = (from sr in _context.SecurityRoles select sr);
			if (privilegeId.HasValue == true)
			{
				query = query.Where(sr => sr.privilegeId == privilegeId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(sr => sr.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(sr => sr.description == description);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(sr => sr.comments == comments);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sr => sr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sr => sr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sr => sr.deleted == false);
				}
			}
			else
			{
				query = query.Where(sr => sr.active == true);
				query = query.Where(sr => sr.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Security Role, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.comments.Contains(anyStringContains)
			       || x.privilege.name.Contains(anyStringContains)
			       || x.privilege.description.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.comments);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.SecurityRole.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/SecurityRole/CreateAuditEvent")]
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
