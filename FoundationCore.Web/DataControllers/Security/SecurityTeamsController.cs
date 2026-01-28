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
    /// This auto generated class provides the basic CRUD operations for the SecurityTeam entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SecurityTeam entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SecurityTeamsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SecurityContext _context;

		private ILogger<SecurityTeamsController> _logger;

		public SecurityTeamsController(SecurityContext context, ILogger<SecurityTeamsController> logger) : base("Security", "SecurityTeam")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of SecurityTeams filtered by the parameters provided.
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
		[Route("api/SecurityTeams")]
		public async Task<IActionResult> GetSecurityTeams(
			int? securityDepartmentId = null,
			string name = null,
			string description = null,
			Guid? objectGuid = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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

			IQueryable<Database.SecurityTeam> query = (from st in _context.SecurityTeams select st);
			if (securityDepartmentId.HasValue == true)
			{
				query = query.Where(st => st.securityDepartmentId == securityDepartmentId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(st => st.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(st => st.description == description);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(st => st.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(st => st.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(st => st.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(st => st.deleted == false);
				}
			}
			else
			{
				query = query.Where(st => st.active == true);
				query = query.Where(st => st.deleted == false);
			}

			query = query.OrderBy(st => st.name).ThenBy(st => st.description);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.securityDepartment);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Security Team, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || (includeRelations == true && x.securityDepartment.name.Contains(anyStringContains))
			       || (includeRelations == true && x.securityDepartment.description.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.SecurityTeam> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SecurityTeam securityTeam in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(securityTeam, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Security.SecurityTeam Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Security.SecurityTeam Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of SecurityTeams filtered by the parameters provided.  Its query is similar to the GetSecurityTeams method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityTeams/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? securityDepartmentId = null,
			string name = null,
			string description = null,
			Guid? objectGuid = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.SecurityTeam> query = (from st in _context.SecurityTeams select st);
			if (securityDepartmentId.HasValue == true)
			{
				query = query.Where(st => st.securityDepartmentId == securityDepartmentId.Value);
			}
			if (name != null)
			{
				query = query.Where(st => st.name == name);
			}
			if (description != null)
			{
				query = query.Where(st => st.description == description);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(st => st.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(st => st.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(st => st.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(st => st.deleted == false);
				}
			}
			else
			{
				query = query.Where(st => st.active == true);
				query = query.Where(st => st.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Security Team, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.securityDepartment.name.Contains(anyStringContains)
			       || x.securityDepartment.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single SecurityTeam by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityTeam/{id}")]
		public async Task<IActionResult> GetSecurityTeam(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.SecurityTeam> query = (from st in _context.SecurityTeams where
							(st.id == id) &&
							(userIsAdmin == true || st.deleted == false) &&
							(userIsWriter == true || st.active == true)
					select st);

				if (includeRelations == true)
				{
					query = query.Include(x => x.securityDepartment);
					query = query.AsSplitQuery();
				}

				Database.SecurityTeam materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Security.SecurityTeam Entity was read with Admin privilege." : "Security.SecurityTeam Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityTeam", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Security.SecurityTeam entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Security.SecurityTeam.   Entity was read with Admin privilege." : "Exception caught during entity read of Security.SecurityTeam.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing SecurityTeam record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SecurityTeam/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSecurityTeam(int id, [FromBody]Database.SecurityTeam.SecurityTeamDTO securityTeamDTO, CancellationToken cancellationToken = default)
		{
			if (securityTeamDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			// Admin privilege needed to write to this table.
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != securityTeamDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.SecurityTeam> query = (from x in _context.SecurityTeams
				where
				(x.id == id)
				select x);


			Database.SecurityTeam existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityTeam PUT", id.ToString(), new Exception("No Security.SecurityTeam entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (securityTeamDTO.objectGuid == Guid.Empty)
            {
                securityTeamDTO.objectGuid = existing.objectGuid;
            }
            else if (securityTeamDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a SecurityTeam record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SecurityTeam cloneOfExisting = (Database.SecurityTeam)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SecurityTeam object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SecurityTeam securityTeam = (Database.SecurityTeam)_context.Entry(existing).GetDatabaseValues().ToObject();
			securityTeam.ApplyDTO(securityTeamDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (securityTeam.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Security.SecurityTeam record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (securityTeam.name != null && securityTeam.name.Length > 100)
			{
				securityTeam.name = securityTeam.name.Substring(0, 100);
			}

			if (securityTeam.description != null && securityTeam.description.Length > 500)
			{
				securityTeam.description = securityTeam.description.Substring(0, 500);
			}

			EntityEntry<Database.SecurityTeam> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(securityTeam);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityTeam entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityTeam.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityTeam.CreateAnonymousWithFirstLevelSubObjects(securityTeam)),
					null);


				return Ok(Database.SecurityTeam.CreateAnonymous(securityTeam));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityTeam entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityTeam.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityTeam.CreateAnonymousWithFirstLevelSubObjects(securityTeam)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new SecurityTeam record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityTeam", Name = "SecurityTeam")]
		public async Task<IActionResult> PostSecurityTeam([FromBody]Database.SecurityTeam.SecurityTeamDTO securityTeamDTO, CancellationToken cancellationToken = default)
		{
			if (securityTeamDTO == null)
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
			// Create a new SecurityTeam object using the data from the DTO
			//
			Database.SecurityTeam securityTeam = Database.SecurityTeam.FromDTO(securityTeamDTO);

			try
			{
				if (securityTeam.name != null && securityTeam.name.Length > 100)
				{
					securityTeam.name = securityTeam.name.Substring(0, 100);
				}

				if (securityTeam.description != null && securityTeam.description.Length > 500)
				{
					securityTeam.description = securityTeam.description.Substring(0, 500);
				}

				securityTeam.objectGuid = Guid.NewGuid();
				_context.SecurityTeams.Add(securityTeam);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Security.SecurityTeam entity successfully created.",
					true,
					securityTeam.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.SecurityTeam.CreateAnonymousWithFirstLevelSubObjects(securityTeam)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Security.SecurityTeam entity creation failed.", false, securityTeam.id.ToString(), "", JsonSerializer.Serialize(securityTeam), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityTeam", securityTeam.id, securityTeam.name));

			return CreatedAtRoute("SecurityTeam", new { id = securityTeam.id }, Database.SecurityTeam.CreateAnonymousWithFirstLevelSubObjects(securityTeam));
		}



        /// <summary>
        /// 
        /// This deletes a SecurityTeam record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityTeam/{id}")]
		[Route("api/SecurityTeam")]
		public async Task<IActionResult> DeleteSecurityTeam(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.SecurityTeam> query = (from x in _context.SecurityTeams
				where
				(x.id == id)
				select x);


			Database.SecurityTeam securityTeam = await query.FirstOrDefaultAsync(cancellationToken);

			if (securityTeam == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityTeam DELETE", id.ToString(), new Exception("No Security.SecurityTeam entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SecurityTeam cloneOfExisting = (Database.SecurityTeam)_context.Entry(securityTeam).GetDatabaseValues().ToObject();


			try
			{
				securityTeam.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityTeam entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityTeam.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityTeam.CreateAnonymousWithFirstLevelSubObjects(securityTeam)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityTeam entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityTeam.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityTeam.CreateAnonymousWithFirstLevelSubObjects(securityTeam)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This gets a list of SecurityTeam records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SecurityTeams/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? securityDepartmentId = null,
			string name = null,
			string description = null,
			Guid? objectGuid = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);

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

			IQueryable<Database.SecurityTeam> query = (from st in _context.SecurityTeams select st);
			if (securityDepartmentId.HasValue == true)
			{
				query = query.Where(st => st.securityDepartmentId == securityDepartmentId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(st => st.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(st => st.description == description);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(st => st.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(st => st.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(st => st.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(st => st.deleted == false);
				}
			}
			else
			{
				query = query.Where(st => st.active == true);
				query = query.Where(st => st.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Security Team, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.securityDepartment.name.Contains(anyStringContains)
			       || x.securityDepartment.description.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.name).ThenBy(x => x.description);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.SecurityTeam.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
		}
*/


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
		[Route("api/SecurityTeam/CreateAuditEvent")]
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
