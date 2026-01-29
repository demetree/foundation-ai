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
    /// This auto generated class provides the basic CRUD operations for the SecurityDepartment entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SecurityDepartment entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SecurityDepartmentsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 100;

		private SecurityContext _context;

		private ILogger<SecurityDepartmentsController> _logger;

		public SecurityDepartmentsController(SecurityContext context, ILogger<SecurityDepartmentsController> logger) : base("Security", "SecurityDepartment")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of SecurityDepartments filtered by the parameters provided.
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
		[Route("api/SecurityDepartments")]
		public async Task<IActionResult> GetSecurityDepartments(
			int? securityOrganizationId = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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

			IQueryable<Database.SecurityDepartment> query = (from sd in _context.SecurityDepartments select sd);
			if (securityOrganizationId.HasValue == true)
			{
				query = query.Where(sd => sd.securityOrganizationId == securityOrganizationId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(sd => sd.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(sd => sd.description == description);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sd => sd.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sd => sd.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sd => sd.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sd => sd.deleted == false);
				}
			}
			else
			{
				query = query.Where(sd => sd.active == true);
				query = query.Where(sd => sd.deleted == false);
			}

			query = query.OrderBy(sd => sd.name).ThenBy(sd => sd.description);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.securityOrganization);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Security Department, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || (includeRelations == true && x.securityOrganization.name.Contains(anyStringContains))
			       || (includeRelations == true && x.securityOrganization.description.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.SecurityDepartment> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SecurityDepartment securityDepartment in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(securityDepartment, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Security.SecurityDepartment Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Security.SecurityDepartment Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of SecurityDepartments filtered by the parameters provided.  Its query is similar to the GetSecurityDepartments method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityDepartments/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? securityOrganizationId = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.SecurityDepartment> query = (from sd in _context.SecurityDepartments select sd);
			if (securityOrganizationId.HasValue == true)
			{
				query = query.Where(sd => sd.securityOrganizationId == securityOrganizationId.Value);
			}
			if (name != null)
			{
				query = query.Where(sd => sd.name == name);
			}
			if (description != null)
			{
				query = query.Where(sd => sd.description == description);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sd => sd.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sd => sd.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sd => sd.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sd => sd.deleted == false);
				}
			}
			else
			{
				query = query.Where(sd => sd.active == true);
				query = query.Where(sd => sd.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Security Department, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.securityOrganization.name.Contains(anyStringContains)
			       || x.securityOrganization.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single SecurityDepartment by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityDepartment/{id}")]
		public async Task<IActionResult> GetSecurityDepartment(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.SecurityDepartment> query = (from sd in _context.SecurityDepartments where
							(sd.id == id) &&
							(userIsAdmin == true || sd.deleted == false) &&
							(userIsWriter == true || sd.active == true)
					select sd);

				if (includeRelations == true)
				{
					query = query.Include(x => x.securityOrganization);
					query = query.AsSplitQuery();
				}

				Database.SecurityDepartment materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Security.SecurityDepartment Entity was read with Admin privilege." : "Security.SecurityDepartment Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityDepartment", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Security.SecurityDepartment entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Security.SecurityDepartment.   Entity was read with Admin privilege." : "Exception caught during entity read of Security.SecurityDepartment.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing SecurityDepartment record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SecurityDepartment/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSecurityDepartment(int id, [FromBody]Database.SecurityDepartment.SecurityDepartmentDTO securityDepartmentDTO, CancellationToken cancellationToken = default)
		{
			if (securityDepartmentDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			// Admin privilege needed to write to this table.
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != securityDepartmentDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.SecurityDepartment> query = (from x in _context.SecurityDepartments
				where
				(x.id == id)
				select x);


			Database.SecurityDepartment existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityDepartment PUT", id.ToString(), new Exception("No Security.SecurityDepartment entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (securityDepartmentDTO.objectGuid == Guid.Empty)
            {
                securityDepartmentDTO.objectGuid = existing.objectGuid;
            }
            else if (securityDepartmentDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a SecurityDepartment record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SecurityDepartment cloneOfExisting = (Database.SecurityDepartment)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SecurityDepartment object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SecurityDepartment securityDepartment = (Database.SecurityDepartment)_context.Entry(existing).GetDatabaseValues().ToObject();
			securityDepartment.ApplyDTO(securityDepartmentDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (securityDepartment.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Security.SecurityDepartment record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (securityDepartment.name != null && securityDepartment.name.Length > 100)
			{
				securityDepartment.name = securityDepartment.name.Substring(0, 100);
			}

			if (securityDepartment.description != null && securityDepartment.description.Length > 500)
			{
				securityDepartment.description = securityDepartment.description.Substring(0, 500);
			}

			EntityEntry<Database.SecurityDepartment> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(securityDepartment);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityDepartment entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityDepartment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityDepartment.CreateAnonymousWithFirstLevelSubObjects(securityDepartment)),
					null);


				return Ok(Database.SecurityDepartment.CreateAnonymous(securityDepartment));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityDepartment entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityDepartment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityDepartment.CreateAnonymousWithFirstLevelSubObjects(securityDepartment)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new SecurityDepartment record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityDepartment", Name = "SecurityDepartment")]
		public async Task<IActionResult> PostSecurityDepartment([FromBody]Database.SecurityDepartment.SecurityDepartmentDTO securityDepartmentDTO, CancellationToken cancellationToken = default)
		{
			if (securityDepartmentDTO == null)
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
			// Create a new SecurityDepartment object using the data from the DTO
			//
			Database.SecurityDepartment securityDepartment = Database.SecurityDepartment.FromDTO(securityDepartmentDTO);

			try
			{
				if (securityDepartment.name != null && securityDepartment.name.Length > 100)
				{
					securityDepartment.name = securityDepartment.name.Substring(0, 100);
				}

				if (securityDepartment.description != null && securityDepartment.description.Length > 500)
				{
					securityDepartment.description = securityDepartment.description.Substring(0, 500);
				}

				securityDepartment.objectGuid = Guid.NewGuid();
				_context.SecurityDepartments.Add(securityDepartment);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Security.SecurityDepartment entity successfully created.",
					true,
					securityDepartment.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.SecurityDepartment.CreateAnonymousWithFirstLevelSubObjects(securityDepartment)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Security.SecurityDepartment entity creation failed.", false, securityDepartment.id.ToString(), "", JsonSerializer.Serialize(securityDepartment), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityDepartment", securityDepartment.id, securityDepartment.name));

			return CreatedAtRoute("SecurityDepartment", new { id = securityDepartment.id }, Database.SecurityDepartment.CreateAnonymousWithFirstLevelSubObjects(securityDepartment));
		}



        /// <summary>
        /// 
        /// This deletes a SecurityDepartment record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityDepartment/{id}")]
		[Route("api/SecurityDepartment")]
		public async Task<IActionResult> DeleteSecurityDepartment(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.SecurityDepartment> query = (from x in _context.SecurityDepartments
				where
				(x.id == id)
				select x);


			Database.SecurityDepartment securityDepartment = await query.FirstOrDefaultAsync(cancellationToken);

			if (securityDepartment == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityDepartment DELETE", id.ToString(), new Exception("No Security.SecurityDepartment entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SecurityDepartment cloneOfExisting = (Database.SecurityDepartment)_context.Entry(securityDepartment).GetDatabaseValues().ToObject();


			try
			{
				securityDepartment.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityDepartment entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityDepartment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityDepartment.CreateAnonymousWithFirstLevelSubObjects(securityDepartment)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityDepartment entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityDepartment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityDepartment.CreateAnonymousWithFirstLevelSubObjects(securityDepartment)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This gets a list of SecurityDepartment records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SecurityDepartments/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? securityOrganizationId = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);

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

			IQueryable<Database.SecurityDepartment> query = (from sd in _context.SecurityDepartments select sd);
			if (securityOrganizationId.HasValue == true)
			{
				query = query.Where(sd => sd.securityOrganizationId == securityOrganizationId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(sd => sd.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(sd => sd.description == description);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sd => sd.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sd => sd.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sd => sd.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sd => sd.deleted == false);
				}
			}
			else
			{
				query = query.Where(sd => sd.active == true);
				query = query.Where(sd => sd.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Security Department, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.securityOrganization.name.Contains(anyStringContains)
			       || x.securityOrganization.description.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.name).ThenBy(x => x.description);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.SecurityDepartment.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/SecurityDepartment/CreateAuditEvent")]
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
