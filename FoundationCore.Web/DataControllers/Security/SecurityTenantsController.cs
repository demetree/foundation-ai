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
    /// This auto generated class provides the basic CRUD operations for the SecurityTenant entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SecurityTenant entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SecurityTenantsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 100;

		private SecurityContext _context;

		private ILogger<SecurityTenantsController> _logger;

		public SecurityTenantsController(SecurityContext context, ILogger<SecurityTenantsController> logger) : base("Security", "SecurityTenant")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of SecurityTenants filtered by the parameters provided.
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
		[Route("api/SecurityTenants")]
		public async Task<IActionResult> GetSecurityTenants(
			string name = null,
			string description = null,
			string settings = null,
			string hostName = null,
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

			IQueryable<Database.SecurityTenant> query = (from st in _context.SecurityTenants select st);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(st => st.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(st => st.description == description);
			}
			if (string.IsNullOrEmpty(settings) == false)
			{
				query = query.Where(st => st.settings == settings);
			}
			if (string.IsNullOrEmpty(hostName) == false)
			{
				query = query.Where(st => st.hostName == hostName);
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

			query = query.OrderBy(st => st.name).ThenBy(st => st.description).ThenBy(st => st.hostName);


			//
			// Add the any string contains parameter to span all the string fields on the Security Tenant, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.settings.Contains(anyStringContains)
			       || x.hostName.Contains(anyStringContains)
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
			
			List<Database.SecurityTenant> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SecurityTenant securityTenant in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(securityTenant, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Security.SecurityTenant Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Security.SecurityTenant Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of SecurityTenants filtered by the parameters provided.  Its query is similar to the GetSecurityTenants method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityTenants/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			string settings = null,
			string hostName = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.SecurityTenant> query = (from st in _context.SecurityTenants select st);
			if (name != null)
			{
				query = query.Where(st => st.name == name);
			}
			if (description != null)
			{
				query = query.Where(st => st.description == description);
			}
			if (settings != null)
			{
				query = query.Where(st => st.settings == settings);
			}
			if (hostName != null)
			{
				query = query.Where(st => st.hostName == hostName);
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
			// Add the any string contains parameter to span all the string fields on the Security Tenant, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.settings.Contains(anyStringContains)
			       || x.hostName.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single SecurityTenant by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityTenant/{id}")]
		public async Task<IActionResult> GetSecurityTenant(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.SecurityTenant> query = (from st in _context.SecurityTenants where
							(st.id == id) &&
							(userIsAdmin == true || st.deleted == false) &&
							(userIsWriter == true || st.active == true)
					select st);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.SecurityTenant materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Security.SecurityTenant Entity was read with Admin privilege." : "Security.SecurityTenant Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityTenant", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Security.SecurityTenant entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Security.SecurityTenant.   Entity was read with Admin privilege." : "Exception caught during entity read of Security.SecurityTenant.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing SecurityTenant record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SecurityTenant/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSecurityTenant(int id, [FromBody]Database.SecurityTenant.SecurityTenantDTO securityTenantDTO, CancellationToken cancellationToken = default)
		{
			if (securityTenantDTO == null)
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



			if (id != securityTenantDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.SecurityTenant> query = (from x in _context.SecurityTenants
				where
				(x.id == id)
				select x);


			Database.SecurityTenant existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityTenant PUT", id.ToString(), new Exception("No Security.SecurityTenant entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (securityTenantDTO.objectGuid == Guid.Empty)
            {
                securityTenantDTO.objectGuid = existing.objectGuid;
            }
            else if (securityTenantDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a SecurityTenant record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SecurityTenant cloneOfExisting = (Database.SecurityTenant)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SecurityTenant object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SecurityTenant securityTenant = (Database.SecurityTenant)_context.Entry(existing).GetDatabaseValues().ToObject();
			securityTenant.ApplyDTO(securityTenantDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (securityTenant.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Security.SecurityTenant record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (securityTenant.name != null && securityTenant.name.Length > 100)
			{
				securityTenant.name = securityTenant.name.Substring(0, 100);
			}

			if (securityTenant.description != null && securityTenant.description.Length > 500)
			{
				securityTenant.description = securityTenant.description.Substring(0, 500);
			}

			if (securityTenant.hostName != null && securityTenant.hostName.Length > 250)
			{
				securityTenant.hostName = securityTenant.hostName.Substring(0, 250);
			}

			EntityEntry<Database.SecurityTenant> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(securityTenant);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityTenant entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityTenant.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityTenant.CreateAnonymousWithFirstLevelSubObjects(securityTenant)),
					null);


				return Ok(Database.SecurityTenant.CreateAnonymous(securityTenant));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityTenant entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityTenant.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityTenant.CreateAnonymousWithFirstLevelSubObjects(securityTenant)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new SecurityTenant record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityTenant", Name = "SecurityTenant")]
		public async Task<IActionResult> PostSecurityTenant([FromBody]Database.SecurityTenant.SecurityTenantDTO securityTenantDTO, CancellationToken cancellationToken = default)
		{
			if (securityTenantDTO == null)
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
			// Create a new SecurityTenant object using the data from the DTO
			//
			Database.SecurityTenant securityTenant = Database.SecurityTenant.FromDTO(securityTenantDTO);

			try
			{
				if (securityTenant.name != null && securityTenant.name.Length > 100)
				{
					securityTenant.name = securityTenant.name.Substring(0, 100);
				}

				if (securityTenant.description != null && securityTenant.description.Length > 500)
				{
					securityTenant.description = securityTenant.description.Substring(0, 500);
				}

				if (securityTenant.hostName != null && securityTenant.hostName.Length > 250)
				{
					securityTenant.hostName = securityTenant.hostName.Substring(0, 250);
				}

				securityTenant.objectGuid = Guid.NewGuid();
				_context.SecurityTenants.Add(securityTenant);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Security.SecurityTenant entity successfully created.",
					true,
					securityTenant.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.SecurityTenant.CreateAnonymousWithFirstLevelSubObjects(securityTenant)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Security.SecurityTenant entity creation failed.", false, securityTenant.id.ToString(), "", JsonSerializer.Serialize(securityTenant), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityTenant", securityTenant.id, securityTenant.name));

			return CreatedAtRoute("SecurityTenant", new { id = securityTenant.id }, Database.SecurityTenant.CreateAnonymousWithFirstLevelSubObjects(securityTenant));
		}



        /// <summary>
        /// 
        /// This deletes a SecurityTenant record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityTenant/{id}")]
		[Route("api/SecurityTenant")]
		public async Task<IActionResult> DeleteSecurityTenant(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.SecurityTenant> query = (from x in _context.SecurityTenants
				where
				(x.id == id)
				select x);


			Database.SecurityTenant securityTenant = await query.FirstOrDefaultAsync(cancellationToken);

			if (securityTenant == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityTenant DELETE", id.ToString(), new Exception("No Security.SecurityTenant entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SecurityTenant cloneOfExisting = (Database.SecurityTenant)_context.Entry(securityTenant).GetDatabaseValues().ToObject();


			try
			{
				securityTenant.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityTenant entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityTenant.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityTenant.CreateAnonymousWithFirstLevelSubObjects(securityTenant)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityTenant entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityTenant.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityTenant.CreateAnonymousWithFirstLevelSubObjects(securityTenant)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of SecurityTenant records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SecurityTenants/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			string settings = null,
			string hostName = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);


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

			IQueryable<Database.SecurityTenant> query = (from st in _context.SecurityTenants select st);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(st => st.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(st => st.description == description);
			}
			if (string.IsNullOrEmpty(settings) == false)
			{
				query = query.Where(st => st.settings == settings);
			}
			if (string.IsNullOrEmpty(hostName) == false)
			{
				query = query.Where(st => st.hostName == hostName);
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
			// Add the any string contains parameter to span all the string fields on the Security Tenant, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.settings.Contains(anyStringContains)
			       || x.hostName.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.hostName);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.SecurityTenant.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/SecurityTenant/CreateAuditEvent")]
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
