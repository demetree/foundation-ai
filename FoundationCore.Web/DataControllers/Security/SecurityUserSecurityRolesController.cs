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
    /// This auto generated class provides the basic CRUD operations for the SecurityUserSecurityRole entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SecurityUserSecurityRole entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SecurityUserSecurityRolesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private SecurityContext _context;

		private ILogger<SecurityUserSecurityRolesController> _logger;

		public SecurityUserSecurityRolesController(SecurityContext context, ILogger<SecurityUserSecurityRolesController> logger) : base("Security", "SecurityUserSecurityRole")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of SecurityUserSecurityRoles filtered by the parameters provided.
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
		[Route("api/SecurityUserSecurityRoles")]
		public async Task<IActionResult> GetSecurityUserSecurityRoles(
			int? securityUserId = null,
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

			IQueryable<Database.SecurityUserSecurityRole> query = (from susr in _context.SecurityUserSecurityRoles select susr);
			if (securityUserId.HasValue == true)
			{
				query = query.Where(susr => susr.securityUserId == securityUserId.Value);
			}
			if (securityRoleId.HasValue == true)
			{
				query = query.Where(susr => susr.securityRoleId == securityRoleId.Value);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(susr => susr.comments == comments);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(susr => susr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(susr => susr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(susr => susr.deleted == false);
				}
			}
			else
			{
				query = query.Where(susr => susr.active == true);
				query = query.Where(susr => susr.deleted == false);
			}

			query = query.OrderBy(susr => susr.comments);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.securityRole);
				query = query.Include(x => x.securityUser);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Security User Security Role, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.comments.Contains(anyStringContains)
			       || (includeRelations == true && x.securityRole.name.Contains(anyStringContains))
			       || (includeRelations == true && x.securityRole.description.Contains(anyStringContains))
			       || (includeRelations == true && x.securityRole.comments.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.accountName.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.password.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.firstName.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.middleName.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.lastName.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.emailAddress.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.cellPhoneNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.phoneNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.phoneExtension.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.description.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.authenticationDomain.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.alternateIdentifier.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.settings.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.authenticationToken.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.twoFactorToken.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.SecurityUserSecurityRole> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SecurityUserSecurityRole securityUserSecurityRole in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(securityUserSecurityRole, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Security.SecurityUserSecurityRole Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Security.SecurityUserSecurityRole Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of SecurityUserSecurityRoles filtered by the parameters provided.  Its query is similar to the GetSecurityUserSecurityRoles method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityUserSecurityRoles/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? securityUserId = null,
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

			IQueryable<Database.SecurityUserSecurityRole> query = (from susr in _context.SecurityUserSecurityRoles select susr);
			if (securityUserId.HasValue == true)
			{
				query = query.Where(susr => susr.securityUserId == securityUserId.Value);
			}
			if (securityRoleId.HasValue == true)
			{
				query = query.Where(susr => susr.securityRoleId == securityRoleId.Value);
			}
			if (comments != null)
			{
				query = query.Where(susr => susr.comments == comments);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(susr => susr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(susr => susr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(susr => susr.deleted == false);
				}
			}
			else
			{
				query = query.Where(susr => susr.active == true);
				query = query.Where(susr => susr.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Security User Security Role, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.comments.Contains(anyStringContains)
			       || x.securityRole.name.Contains(anyStringContains)
			       || x.securityRole.description.Contains(anyStringContains)
			       || x.securityRole.comments.Contains(anyStringContains)
			       || x.securityUser.accountName.Contains(anyStringContains)
			       || x.securityUser.password.Contains(anyStringContains)
			       || x.securityUser.firstName.Contains(anyStringContains)
			       || x.securityUser.middleName.Contains(anyStringContains)
			       || x.securityUser.lastName.Contains(anyStringContains)
			       || x.securityUser.emailAddress.Contains(anyStringContains)
			       || x.securityUser.cellPhoneNumber.Contains(anyStringContains)
			       || x.securityUser.phoneNumber.Contains(anyStringContains)
			       || x.securityUser.phoneExtension.Contains(anyStringContains)
			       || x.securityUser.description.Contains(anyStringContains)
			       || x.securityUser.authenticationDomain.Contains(anyStringContains)
			       || x.securityUser.alternateIdentifier.Contains(anyStringContains)
			       || x.securityUser.settings.Contains(anyStringContains)
			       || x.securityUser.authenticationToken.Contains(anyStringContains)
			       || x.securityUser.twoFactorToken.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single SecurityUserSecurityRole by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityUserSecurityRole/{id}")]
		public async Task<IActionResult> GetSecurityUserSecurityRole(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.SecurityUserSecurityRole> query = (from susr in _context.SecurityUserSecurityRoles where
							(susr.id == id) &&
							(userIsAdmin == true || susr.deleted == false) &&
							(userIsWriter == true || susr.active == true)
					select susr);

				if (includeRelations == true)
				{
					query = query.Include(x => x.securityRole);
					query = query.Include(x => x.securityUser);
					query = query.AsSplitQuery();
				}

				Database.SecurityUserSecurityRole materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Security.SecurityUserSecurityRole Entity was read with Admin privilege." : "Security.SecurityUserSecurityRole Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityUserSecurityRole", materialized.id, materialized.comments));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Security.SecurityUserSecurityRole entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Security.SecurityUserSecurityRole.   Entity was read with Admin privilege." : "Exception caught during entity read of Security.SecurityUserSecurityRole.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing SecurityUserSecurityRole record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SecurityUserSecurityRole/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSecurityUserSecurityRole(int id, [FromBody]Database.SecurityUserSecurityRole.SecurityUserSecurityRoleDTO securityUserSecurityRoleDTO, CancellationToken cancellationToken = default)
		{
			if (securityUserSecurityRoleDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != securityUserSecurityRoleDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.SecurityUserSecurityRole> query = (from x in _context.SecurityUserSecurityRoles
				where
				(x.id == id)
				select x);


			Database.SecurityUserSecurityRole existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityUserSecurityRole PUT", id.ToString(), new Exception("No Security.SecurityUserSecurityRole entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SecurityUserSecurityRole cloneOfExisting = (Database.SecurityUserSecurityRole)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SecurityUserSecurityRole object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SecurityUserSecurityRole securityUserSecurityRole = (Database.SecurityUserSecurityRole)_context.Entry(existing).GetDatabaseValues().ToObject();
			securityUserSecurityRole.ApplyDTO(securityUserSecurityRoleDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (securityUserSecurityRole.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Security.SecurityUserSecurityRole record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (securityUserSecurityRole.comments != null && securityUserSecurityRole.comments.Length > 1000)
			{
				securityUserSecurityRole.comments = securityUserSecurityRole.comments.Substring(0, 1000);
			}

			EntityEntry<Database.SecurityUserSecurityRole> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(securityUserSecurityRole);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityUserSecurityRole entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityUserSecurityRole.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityUserSecurityRole.CreateAnonymousWithFirstLevelSubObjects(securityUserSecurityRole)),
					null);


				return Ok(Database.SecurityUserSecurityRole.CreateAnonymous(securityUserSecurityRole));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityUserSecurityRole entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityUserSecurityRole.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityUserSecurityRole.CreateAnonymousWithFirstLevelSubObjects(securityUserSecurityRole)),
					ex);

				return Problem(ex.Message);
			}

		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new SecurityUserSecurityRole record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityUserSecurityRole", Name = "SecurityUserSecurityRole")]
		public async Task<IActionResult> PostSecurityUserSecurityRole([FromBody]Database.SecurityUserSecurityRole.SecurityUserSecurityRoleDTO securityUserSecurityRoleDTO, CancellationToken cancellationToken = default)
		{
			if (securityUserSecurityRoleDTO == null)
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
			// Create a new SecurityUserSecurityRole object using the data from the DTO
			//
			Database.SecurityUserSecurityRole securityUserSecurityRole = Database.SecurityUserSecurityRole.FromDTO(securityUserSecurityRoleDTO);

			try
			{
				if (securityUserSecurityRole.comments != null && securityUserSecurityRole.comments.Length > 1000)
				{
					securityUserSecurityRole.comments = securityUserSecurityRole.comments.Substring(0, 1000);
				}

				_context.SecurityUserSecurityRoles.Add(securityUserSecurityRole);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Security.SecurityUserSecurityRole entity successfully created.",
					true,
					securityUserSecurityRole.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.SecurityUserSecurityRole.CreateAnonymousWithFirstLevelSubObjects(securityUserSecurityRole)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Security.SecurityUserSecurityRole entity creation failed.", false, securityUserSecurityRole.id.ToString(), "", JsonSerializer.Serialize(securityUserSecurityRole), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityUserSecurityRole", securityUserSecurityRole.id, securityUserSecurityRole.comments));

			return CreatedAtRoute("SecurityUserSecurityRole", new { id = securityUserSecurityRole.id }, Database.SecurityUserSecurityRole.CreateAnonymousWithFirstLevelSubObjects(securityUserSecurityRole));
		}

*/


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a SecurityUserSecurityRole record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityUserSecurityRole/{id}")]
		[Route("api/SecurityUserSecurityRole")]
		public async Task<IActionResult> DeleteSecurityUserSecurityRole(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.SecurityUserSecurityRole> query = (from x in _context.SecurityUserSecurityRoles
				where
				(x.id == id)
				select x);


			Database.SecurityUserSecurityRole securityUserSecurityRole = await query.FirstOrDefaultAsync(cancellationToken);

			if (securityUserSecurityRole == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityUserSecurityRole DELETE", id.ToString(), new Exception("No Security.SecurityUserSecurityRole entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SecurityUserSecurityRole cloneOfExisting = (Database.SecurityUserSecurityRole)_context.Entry(securityUserSecurityRole).GetDatabaseValues().ToObject();


			try
			{
				securityUserSecurityRole.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityUserSecurityRole entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityUserSecurityRole.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityUserSecurityRole.CreateAnonymousWithFirstLevelSubObjects(securityUserSecurityRole)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityUserSecurityRole entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityUserSecurityRole.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityUserSecurityRole.CreateAnonymousWithFirstLevelSubObjects(securityUserSecurityRole)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


*/
        /// <summary>
        /// 
        /// This gets a list of SecurityUserSecurityRole records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SecurityUserSecurityRoles/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? securityUserId = null,
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

			IQueryable<Database.SecurityUserSecurityRole> query = (from susr in _context.SecurityUserSecurityRoles select susr);
			if (securityUserId.HasValue == true)
			{
				query = query.Where(susr => susr.securityUserId == securityUserId.Value);
			}
			if (securityRoleId.HasValue == true)
			{
				query = query.Where(susr => susr.securityRoleId == securityRoleId.Value);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(susr => susr.comments == comments);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(susr => susr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(susr => susr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(susr => susr.deleted == false);
				}
			}
			else
			{
				query = query.Where(susr => susr.active == true);
				query = query.Where(susr => susr.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Security User Security Role, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.comments.Contains(anyStringContains)
			       || x.securityRole.name.Contains(anyStringContains)
			       || x.securityRole.description.Contains(anyStringContains)
			       || x.securityRole.comments.Contains(anyStringContains)
			       || x.securityUser.accountName.Contains(anyStringContains)
			       || x.securityUser.password.Contains(anyStringContains)
			       || x.securityUser.firstName.Contains(anyStringContains)
			       || x.securityUser.middleName.Contains(anyStringContains)
			       || x.securityUser.lastName.Contains(anyStringContains)
			       || x.securityUser.emailAddress.Contains(anyStringContains)
			       || x.securityUser.cellPhoneNumber.Contains(anyStringContains)
			       || x.securityUser.phoneNumber.Contains(anyStringContains)
			       || x.securityUser.phoneExtension.Contains(anyStringContains)
			       || x.securityUser.description.Contains(anyStringContains)
			       || x.securityUser.authenticationDomain.Contains(anyStringContains)
			       || x.securityUser.alternateIdentifier.Contains(anyStringContains)
			       || x.securityUser.settings.Contains(anyStringContains)
			       || x.securityUser.authenticationToken.Contains(anyStringContains)
			       || x.securityUser.twoFactorToken.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.comments);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.SecurityUserSecurityRole.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/SecurityUserSecurityRole/CreateAuditEvent")]
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
