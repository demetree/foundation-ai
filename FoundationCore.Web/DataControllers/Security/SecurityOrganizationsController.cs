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
    /// This auto generated class provides the basic CRUD operations for the SecurityOrganization entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SecurityOrganization entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SecurityOrganizationsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 150;

		private SecurityContext _context;

		private ILogger<SecurityOrganizationsController> _logger;

		public SecurityOrganizationsController(SecurityContext context, ILogger<SecurityOrganizationsController> logger) : base("Security", "SecurityOrganization")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of SecurityOrganizations filtered by the parameters provided.
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
		[Route("api/SecurityOrganizations")]
		public async Task<IActionResult> GetSecurityOrganizations(
			int? securityTenantId = null,
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

			//
			// Security Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 150, cancellationToken);
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

			IQueryable<Database.SecurityOrganization> query = (from so in _context.SecurityOrganizations select so);
			if (securityTenantId.HasValue == true)
			{
				query = query.Where(so => so.securityTenantId == securityTenantId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(so => so.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(so => so.description == description);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(so => so.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(so => so.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(so => so.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(so => so.deleted == false);
				}
			}
			else
			{
				query = query.Where(so => so.active == true);
				query = query.Where(so => so.deleted == false);
			}

			query = query.OrderBy(so => so.name).ThenBy(so => so.description);


			//
			// Add the any string contains parameter to span all the string fields on the Security Organization, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || (includeRelations == true && x.securityTenant.name.Contains(anyStringContains))
			       || (includeRelations == true && x.securityTenant.description.Contains(anyStringContains))
			       || (includeRelations == true && x.securityTenant.settings.Contains(anyStringContains))
			       || (includeRelations == true && x.securityTenant.hostName.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.securityTenant);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.SecurityOrganization> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SecurityOrganization securityOrganization in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(securityOrganization, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Security.SecurityOrganization Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Security.SecurityOrganization Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of SecurityOrganizations filtered by the parameters provided.  Its query is similar to the GetSecurityOrganizations method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityOrganizations/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? securityTenantId = null,
			string name = null,
			string description = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Security Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 150, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.SecurityOrganization> query = (from so in _context.SecurityOrganizations select so);
			if (securityTenantId.HasValue == true)
			{
				query = query.Where(so => so.securityTenantId == securityTenantId.Value);
			}
			if (name != null)
			{
				query = query.Where(so => so.name == name);
			}
			if (description != null)
			{
				query = query.Where(so => so.description == description);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(so => so.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(so => so.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(so => so.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(so => so.deleted == false);
				}
			}
			else
			{
				query = query.Where(so => so.active == true);
				query = query.Where(so => so.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Security Organization, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.securityTenant.name.Contains(anyStringContains)
			       || x.securityTenant.description.Contains(anyStringContains)
			       || x.securityTenant.settings.Contains(anyStringContains)
			       || x.securityTenant.hostName.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single SecurityOrganization by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityOrganization/{id}")]
		public async Task<IActionResult> GetSecurityOrganization(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Security Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 150, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.SecurityOrganization> query = (from so in _context.SecurityOrganizations where
							(so.id == id) &&
							(userIsAdmin == true || so.deleted == false) &&
							(userIsWriter == true || so.active == true)
					select so);

				if (includeRelations == true)
				{
					query = query.Include(x => x.securityTenant);
					query = query.AsSplitQuery();
				}

				Database.SecurityOrganization materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Security.SecurityOrganization Entity was read with Admin privilege." : "Security.SecurityOrganization Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityOrganization", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Security.SecurityOrganization entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Security.SecurityOrganization.   Entity was read with Admin privilege." : "Exception caught during entity read of Security.SecurityOrganization.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing SecurityOrganization record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SecurityOrganization/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSecurityOrganization(int id, [FromBody]Database.SecurityOrganization.SecurityOrganizationDTO securityOrganizationDTO, CancellationToken cancellationToken = default)
		{
			if (securityOrganizationDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Security Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != securityOrganizationDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 150, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.SecurityOrganization> query = (from x in _context.SecurityOrganizations
				where
				(x.id == id)
				select x);


			Database.SecurityOrganization existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityOrganization PUT", id.ToString(), new Exception("No Security.SecurityOrganization entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (securityOrganizationDTO.objectGuid == Guid.Empty)
            {
                securityOrganizationDTO.objectGuid = existing.objectGuid;
            }
            else if (securityOrganizationDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a SecurityOrganization record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SecurityOrganization cloneOfExisting = (Database.SecurityOrganization)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SecurityOrganization object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SecurityOrganization securityOrganization = (Database.SecurityOrganization)_context.Entry(existing).GetDatabaseValues().ToObject();
			securityOrganization.ApplyDTO(securityOrganizationDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (securityOrganization.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Security.SecurityOrganization record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (securityOrganization.name != null && securityOrganization.name.Length > 100)
			{
				securityOrganization.name = securityOrganization.name.Substring(0, 100);
			}

			if (securityOrganization.description != null && securityOrganization.description.Length > 500)
			{
				securityOrganization.description = securityOrganization.description.Substring(0, 500);
			}

			EntityEntry<Database.SecurityOrganization> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(securityOrganization);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityOrganization entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityOrganization.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityOrganization.CreateAnonymousWithFirstLevelSubObjects(securityOrganization)),
					null);


				return Ok(Database.SecurityOrganization.CreateAnonymous(securityOrganization));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityOrganization entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityOrganization.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityOrganization.CreateAnonymousWithFirstLevelSubObjects(securityOrganization)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new SecurityOrganization record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityOrganization", Name = "SecurityOrganization")]
		public async Task<IActionResult> PostSecurityOrganization([FromBody]Database.SecurityOrganization.SecurityOrganizationDTO securityOrganizationDTO, CancellationToken cancellationToken = default)
		{
			if (securityOrganizationDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Security Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new SecurityOrganization object using the data from the DTO
			//
			Database.SecurityOrganization securityOrganization = Database.SecurityOrganization.FromDTO(securityOrganizationDTO);

			try
			{
				if (securityOrganization.name != null && securityOrganization.name.Length > 100)
				{
					securityOrganization.name = securityOrganization.name.Substring(0, 100);
				}

				if (securityOrganization.description != null && securityOrganization.description.Length > 500)
				{
					securityOrganization.description = securityOrganization.description.Substring(0, 500);
				}

				securityOrganization.objectGuid = Guid.NewGuid();
				_context.SecurityOrganizations.Add(securityOrganization);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Security.SecurityOrganization entity successfully created.",
					true,
					securityOrganization.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.SecurityOrganization.CreateAnonymousWithFirstLevelSubObjects(securityOrganization)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Security.SecurityOrganization entity creation failed.", false, securityOrganization.id.ToString(), "", JsonSerializer.Serialize(securityOrganization), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityOrganization", securityOrganization.id, securityOrganization.name));

			return CreatedAtRoute("SecurityOrganization", new { id = securityOrganization.id }, Database.SecurityOrganization.CreateAnonymousWithFirstLevelSubObjects(securityOrganization));
		}



        /// <summary>
        /// 
        /// This deletes a SecurityOrganization record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityOrganization/{id}")]
		[Route("api/SecurityOrganization")]
		public async Task<IActionResult> DeleteSecurityOrganization(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Security Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.SecurityOrganization> query = (from x in _context.SecurityOrganizations
				where
				(x.id == id)
				select x);


			Database.SecurityOrganization securityOrganization = await query.FirstOrDefaultAsync(cancellationToken);

			if (securityOrganization == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityOrganization DELETE", id.ToString(), new Exception("No Security.SecurityOrganization entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SecurityOrganization cloneOfExisting = (Database.SecurityOrganization)_context.Entry(securityOrganization).GetDatabaseValues().ToObject();


			try
			{
				securityOrganization.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityOrganization entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityOrganization.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityOrganization.CreateAnonymousWithFirstLevelSubObjects(securityOrganization)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityOrganization entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityOrganization.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityOrganization.CreateAnonymousWithFirstLevelSubObjects(securityOrganization)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This gets a list of SecurityOrganization records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SecurityOrganizations/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? securityTenantId = null,
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
			//
			// Security Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 150, cancellationToken);


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

			IQueryable<Database.SecurityOrganization> query = (from so in _context.SecurityOrganizations select so);
			if (securityTenantId.HasValue == true)
			{
				query = query.Where(so => so.securityTenantId == securityTenantId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(so => so.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(so => so.description == description);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(so => so.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(so => so.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(so => so.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(so => so.deleted == false);
				}
			}
			else
			{
				query = query.Where(so => so.active == true);
				query = query.Where(so => so.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Security Organization, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.securityTenant.name.Contains(anyStringContains)
			       || x.securityTenant.description.Contains(anyStringContains)
			       || x.securityTenant.settings.Contains(anyStringContains)
			       || x.securityTenant.hostName.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.name).ThenBy(x => x.description);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.SecurityOrganization.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/SecurityOrganization/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Security Administrator role needed to write to this table.
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
