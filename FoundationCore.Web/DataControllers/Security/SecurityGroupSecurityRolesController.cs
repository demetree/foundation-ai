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
    /// This auto generated class provides the basic CRUD operations for the SecurityGroupSecurityRole entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SecurityGroupSecurityRole entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SecurityGroupSecurityRolesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private SecurityContext _context;

		private ILogger<SecurityGroupSecurityRolesController> _logger;

		public SecurityGroupSecurityRolesController(SecurityContext context, ILogger<SecurityGroupSecurityRolesController> logger) : base("Security", "SecurityGroupSecurityRole")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of SecurityGroupSecurityRoles filtered by the parameters provided.
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
		[Route("api/SecurityGroupSecurityRoles")]
		public async Task<IActionResult> GetSecurityGroupSecurityRoles(
			int? securityGroupId = null,
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

			IQueryable<Database.SecurityGroupSecurityRole> query = (from sgsr in _context.SecurityGroupSecurityRoles select sgsr);
			if (securityGroupId.HasValue == true)
			{
				query = query.Where(sgsr => sgsr.securityGroupId == securityGroupId.Value);
			}
			if (securityRoleId.HasValue == true)
			{
				query = query.Where(sgsr => sgsr.securityRoleId == securityRoleId.Value);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(sgsr => sgsr.comments == comments);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sgsr => sgsr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sgsr => sgsr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sgsr => sgsr.deleted == false);
				}
			}
			else
			{
				query = query.Where(sgsr => sgsr.active == true);
				query = query.Where(sgsr => sgsr.deleted == false);
			}

			query = query.OrderBy(sgsr => sgsr.comments);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.securityGroup);
				query = query.Include(x => x.securityRole);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Security Group Security Role, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.comments.Contains(anyStringContains)
			       || (includeRelations == true && x.securityGroup.name.Contains(anyStringContains))
			       || (includeRelations == true && x.securityGroup.description.Contains(anyStringContains))
			       || (includeRelations == true && x.securityRole.name.Contains(anyStringContains))
			       || (includeRelations == true && x.securityRole.description.Contains(anyStringContains))
			       || (includeRelations == true && x.securityRole.comments.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.SecurityGroupSecurityRole> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SecurityGroupSecurityRole securityGroupSecurityRole in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(securityGroupSecurityRole, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Security.SecurityGroupSecurityRole Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Security.SecurityGroupSecurityRole Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of SecurityGroupSecurityRoles filtered by the parameters provided.  Its query is similar to the GetSecurityGroupSecurityRoles method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityGroupSecurityRoles/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? securityGroupId = null,
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

			IQueryable<Database.SecurityGroupSecurityRole> query = (from sgsr in _context.SecurityGroupSecurityRoles select sgsr);
			if (securityGroupId.HasValue == true)
			{
				query = query.Where(sgsr => sgsr.securityGroupId == securityGroupId.Value);
			}
			if (securityRoleId.HasValue == true)
			{
				query = query.Where(sgsr => sgsr.securityRoleId == securityRoleId.Value);
			}
			if (comments != null)
			{
				query = query.Where(sgsr => sgsr.comments == comments);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sgsr => sgsr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sgsr => sgsr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sgsr => sgsr.deleted == false);
				}
			}
			else
			{
				query = query.Where(sgsr => sgsr.active == true);
				query = query.Where(sgsr => sgsr.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Security Group Security Role, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.comments.Contains(anyStringContains)
			       || x.securityGroup.name.Contains(anyStringContains)
			       || x.securityGroup.description.Contains(anyStringContains)
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
        /// This gets a single SecurityGroupSecurityRole by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityGroupSecurityRole/{id}")]
		public async Task<IActionResult> GetSecurityGroupSecurityRole(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.SecurityGroupSecurityRole> query = (from sgsr in _context.SecurityGroupSecurityRoles where
							(sgsr.id == id) &&
							(userIsAdmin == true || sgsr.deleted == false) &&
							(userIsWriter == true || sgsr.active == true)
					select sgsr);

				if (includeRelations == true)
				{
					query = query.Include(x => x.securityGroup);
					query = query.Include(x => x.securityRole);
					query = query.AsSplitQuery();
				}

				Database.SecurityGroupSecurityRole materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Security.SecurityGroupSecurityRole Entity was read with Admin privilege." : "Security.SecurityGroupSecurityRole Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityGroupSecurityRole", materialized.id, materialized.comments));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Security.SecurityGroupSecurityRole entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Security.SecurityGroupSecurityRole.   Entity was read with Admin privilege." : "Exception caught during entity read of Security.SecurityGroupSecurityRole.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing SecurityGroupSecurityRole record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SecurityGroupSecurityRole/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSecurityGroupSecurityRole(int id, [FromBody]Database.SecurityGroupSecurityRole.SecurityGroupSecurityRoleDTO securityGroupSecurityRoleDTO, CancellationToken cancellationToken = default)
		{
			if (securityGroupSecurityRoleDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != securityGroupSecurityRoleDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.SecurityGroupSecurityRole> query = (from x in _context.SecurityGroupSecurityRoles
				where
				(x.id == id)
				select x);


			Database.SecurityGroupSecurityRole existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityGroupSecurityRole PUT", id.ToString(), new Exception("No Security.SecurityGroupSecurityRole entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SecurityGroupSecurityRole cloneOfExisting = (Database.SecurityGroupSecurityRole)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SecurityGroupSecurityRole object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SecurityGroupSecurityRole securityGroupSecurityRole = (Database.SecurityGroupSecurityRole)_context.Entry(existing).GetDatabaseValues().ToObject();
			securityGroupSecurityRole.ApplyDTO(securityGroupSecurityRoleDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (securityGroupSecurityRole.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Security.SecurityGroupSecurityRole record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (securityGroupSecurityRole.comments != null && securityGroupSecurityRole.comments.Length > 1000)
			{
				securityGroupSecurityRole.comments = securityGroupSecurityRole.comments.Substring(0, 1000);
			}

			EntityEntry<Database.SecurityGroupSecurityRole> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(securityGroupSecurityRole);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityGroupSecurityRole entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityGroupSecurityRole.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityGroupSecurityRole.CreateAnonymousWithFirstLevelSubObjects(securityGroupSecurityRole)),
					null);


				return Ok(Database.SecurityGroupSecurityRole.CreateAnonymous(securityGroupSecurityRole));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityGroupSecurityRole entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityGroupSecurityRole.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityGroupSecurityRole.CreateAnonymousWithFirstLevelSubObjects(securityGroupSecurityRole)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new SecurityGroupSecurityRole record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityGroupSecurityRole", Name = "SecurityGroupSecurityRole")]
		public async Task<IActionResult> PostSecurityGroupSecurityRole([FromBody]Database.SecurityGroupSecurityRole.SecurityGroupSecurityRoleDTO securityGroupSecurityRoleDTO, CancellationToken cancellationToken = default)
		{
			if (securityGroupSecurityRoleDTO == null)
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
			// Create a new SecurityGroupSecurityRole object using the data from the DTO
			//
			Database.SecurityGroupSecurityRole securityGroupSecurityRole = Database.SecurityGroupSecurityRole.FromDTO(securityGroupSecurityRoleDTO);

			try
			{
				if (securityGroupSecurityRole.comments != null && securityGroupSecurityRole.comments.Length > 1000)
				{
					securityGroupSecurityRole.comments = securityGroupSecurityRole.comments.Substring(0, 1000);
				}

				_context.SecurityGroupSecurityRoles.Add(securityGroupSecurityRole);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Security.SecurityGroupSecurityRole entity successfully created.",
					true,
					securityGroupSecurityRole.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.SecurityGroupSecurityRole.CreateAnonymousWithFirstLevelSubObjects(securityGroupSecurityRole)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Security.SecurityGroupSecurityRole entity creation failed.", false, securityGroupSecurityRole.id.ToString(), "", JsonSerializer.Serialize(securityGroupSecurityRole), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityGroupSecurityRole", securityGroupSecurityRole.id, securityGroupSecurityRole.comments));

			return CreatedAtRoute("SecurityGroupSecurityRole", new { id = securityGroupSecurityRole.id }, Database.SecurityGroupSecurityRole.CreateAnonymousWithFirstLevelSubObjects(securityGroupSecurityRole));
		}



        /// <summary>
        /// 
        /// This deletes a SecurityGroupSecurityRole record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityGroupSecurityRole/{id}")]
		[Route("api/SecurityGroupSecurityRole")]
		public async Task<IActionResult> DeleteSecurityGroupSecurityRole(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.SecurityGroupSecurityRole> query = (from x in _context.SecurityGroupSecurityRoles
				where
				(x.id == id)
				select x);


			Database.SecurityGroupSecurityRole securityGroupSecurityRole = await query.FirstOrDefaultAsync(cancellationToken);

			if (securityGroupSecurityRole == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityGroupSecurityRole DELETE", id.ToString(), new Exception("No Security.SecurityGroupSecurityRole entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SecurityGroupSecurityRole cloneOfExisting = (Database.SecurityGroupSecurityRole)_context.Entry(securityGroupSecurityRole).GetDatabaseValues().ToObject();


			try
			{
				securityGroupSecurityRole.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityGroupSecurityRole entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityGroupSecurityRole.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityGroupSecurityRole.CreateAnonymousWithFirstLevelSubObjects(securityGroupSecurityRole)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityGroupSecurityRole entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityGroupSecurityRole.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityGroupSecurityRole.CreateAnonymousWithFirstLevelSubObjects(securityGroupSecurityRole)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of SecurityGroupSecurityRole records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SecurityGroupSecurityRoles/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? securityGroupId = null,
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

			IQueryable<Database.SecurityGroupSecurityRole> query = (from sgsr in _context.SecurityGroupSecurityRoles select sgsr);
			if (securityGroupId.HasValue == true)
			{
				query = query.Where(sgsr => sgsr.securityGroupId == securityGroupId.Value);
			}
			if (securityRoleId.HasValue == true)
			{
				query = query.Where(sgsr => sgsr.securityRoleId == securityRoleId.Value);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(sgsr => sgsr.comments == comments);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sgsr => sgsr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sgsr => sgsr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sgsr => sgsr.deleted == false);
				}
			}
			else
			{
				query = query.Where(sgsr => sgsr.active == true);
				query = query.Where(sgsr => sgsr.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Security Group Security Role, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.comments.Contains(anyStringContains)
			       || x.securityGroup.name.Contains(anyStringContains)
			       || x.securityGroup.description.Contains(anyStringContains)
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
			return Ok(await (from queryData in query select Database.SecurityGroupSecurityRole.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/SecurityGroupSecurityRole/CreateAuditEvent")]
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
