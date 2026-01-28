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
    /// This auto generated class provides the basic CRUD operations for the SecurityTenantUser entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SecurityTenantUser entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SecurityTenantUsersController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private SecurityContext _context;

		private ILogger<SecurityTenantUsersController> _logger;

		public SecurityTenantUsersController(SecurityContext context, ILogger<SecurityTenantUsersController> logger) : base("Security", "SecurityTenantUser")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of SecurityTenantUsers filtered by the parameters provided.
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
		[Route("api/SecurityTenantUsers")]
		public async Task<IActionResult> GetSecurityTenantUsers(
			int? securityTenantId = null,
			int? securityUserId = null,
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

			IQueryable<Database.SecurityTenantUser> query = (from stu in _context.SecurityTenantUsers select stu);
			if (securityTenantId.HasValue == true)
			{
				query = query.Where(stu => stu.securityTenantId == securityTenantId.Value);
			}
			if (securityUserId.HasValue == true)
			{
				query = query.Where(stu => stu.securityUserId == securityUserId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(stu => stu.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(stu => stu.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(stu => stu.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(stu => stu.deleted == false);
				}
			}
			else
			{
				query = query.Where(stu => stu.active == true);
				query = query.Where(stu => stu.deleted == false);
			}

			query = query.OrderBy(stu => stu.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.securityTenant);
				query = query.Include(x => x.securityUser);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Security Tenant User, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       (includeRelations == true && x.securityTenant.name.Contains(anyStringContains))
			       || (includeRelations == true && x.securityTenant.description.Contains(anyStringContains))
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
			
			List<Database.SecurityTenantUser> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SecurityTenantUser securityTenantUser in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(securityTenantUser, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Security.SecurityTenantUser Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Security.SecurityTenantUser Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of SecurityTenantUsers filtered by the parameters provided.  Its query is similar to the GetSecurityTenantUsers method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityTenantUsers/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? securityTenantId = null,
			int? securityUserId = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.SecurityTenantUser> query = (from stu in _context.SecurityTenantUsers select stu);
			if (securityTenantId.HasValue == true)
			{
				query = query.Where(stu => stu.securityTenantId == securityTenantId.Value);
			}
			if (securityUserId.HasValue == true)
			{
				query = query.Where(stu => stu.securityUserId == securityUserId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(stu => stu.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(stu => stu.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(stu => stu.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(stu => stu.deleted == false);
				}
			}
			else
			{
				query = query.Where(stu => stu.active == true);
				query = query.Where(stu => stu.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Security Tenant User, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.securityTenant.name.Contains(anyStringContains)
			       || x.securityTenant.description.Contains(anyStringContains)
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
        /// This gets a single SecurityTenantUser by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityTenantUser/{id}")]
		public async Task<IActionResult> GetSecurityTenantUser(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.SecurityTenantUser> query = (from stu in _context.SecurityTenantUsers where
							(stu.id == id) &&
							(userIsAdmin == true || stu.deleted == false) &&
							(userIsWriter == true || stu.active == true)
					select stu);

				if (includeRelations == true)
				{
					query = query.Include(x => x.securityTenant);
					query = query.Include(x => x.securityUser);
					query = query.AsSplitQuery();
				}

				Database.SecurityTenantUser materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Security.SecurityTenantUser Entity was read with Admin privilege." : "Security.SecurityTenantUser Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityTenantUser", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Security.SecurityTenantUser entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Security.SecurityTenantUser.   Entity was read with Admin privilege." : "Exception caught during entity read of Security.SecurityTenantUser.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing SecurityTenantUser record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SecurityTenantUser/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSecurityTenantUser(int id, [FromBody]Database.SecurityTenantUser.SecurityTenantUserDTO securityTenantUserDTO, CancellationToken cancellationToken = default)
		{
			if (securityTenantUserDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != securityTenantUserDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.SecurityTenantUser> query = (from x in _context.SecurityTenantUsers
				where
				(x.id == id)
				select x);


			Database.SecurityTenantUser existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityTenantUser PUT", id.ToString(), new Exception("No Security.SecurityTenantUser entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (securityTenantUserDTO.objectGuid == Guid.Empty)
            {
                securityTenantUserDTO.objectGuid = existing.objectGuid;
            }
            else if (securityTenantUserDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a SecurityTenantUser record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SecurityTenantUser cloneOfExisting = (Database.SecurityTenantUser)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SecurityTenantUser object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SecurityTenantUser securityTenantUser = (Database.SecurityTenantUser)_context.Entry(existing).GetDatabaseValues().ToObject();
			securityTenantUser.ApplyDTO(securityTenantUserDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (securityTenantUser.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Security.SecurityTenantUser record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			EntityEntry<Database.SecurityTenantUser> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(securityTenantUser);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityTenantUser entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityTenantUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityTenantUser.CreateAnonymousWithFirstLevelSubObjects(securityTenantUser)),
					null);


				return Ok(Database.SecurityTenantUser.CreateAnonymous(securityTenantUser));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityTenantUser entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityTenantUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityTenantUser.CreateAnonymousWithFirstLevelSubObjects(securityTenantUser)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new SecurityTenantUser record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityTenantUser", Name = "SecurityTenantUser")]
		public async Task<IActionResult> PostSecurityTenantUser([FromBody]Database.SecurityTenantUser.SecurityTenantUserDTO securityTenantUserDTO, CancellationToken cancellationToken = default)
		{
			if (securityTenantUserDTO == null)
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
			// Create a new SecurityTenantUser object using the data from the DTO
			//
			Database.SecurityTenantUser securityTenantUser = Database.SecurityTenantUser.FromDTO(securityTenantUserDTO);

			try
			{
				securityTenantUser.objectGuid = Guid.NewGuid();
				_context.SecurityTenantUsers.Add(securityTenantUser);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Security.SecurityTenantUser entity successfully created.",
					true,
					securityTenantUser.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.SecurityTenantUser.CreateAnonymousWithFirstLevelSubObjects(securityTenantUser)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Security.SecurityTenantUser entity creation failed.", false, securityTenantUser.id.ToString(), "", JsonSerializer.Serialize(securityTenantUser), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityTenantUser", securityTenantUser.id, securityTenantUser.id.ToString()));

			return CreatedAtRoute("SecurityTenantUser", new { id = securityTenantUser.id }, Database.SecurityTenantUser.CreateAnonymousWithFirstLevelSubObjects(securityTenantUser));
		}



        /// <summary>
        /// 
        /// This deletes a SecurityTenantUser record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityTenantUser/{id}")]
		[Route("api/SecurityTenantUser")]
		public async Task<IActionResult> DeleteSecurityTenantUser(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.SecurityTenantUser> query = (from x in _context.SecurityTenantUsers
				where
				(x.id == id)
				select x);


			Database.SecurityTenantUser securityTenantUser = await query.FirstOrDefaultAsync(cancellationToken);

			if (securityTenantUser == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityTenantUser DELETE", id.ToString(), new Exception("No Security.SecurityTenantUser entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SecurityTenantUser cloneOfExisting = (Database.SecurityTenantUser)_context.Entry(securityTenantUser).GetDatabaseValues().ToObject();


			try
			{
				securityTenantUser.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityTenantUser entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityTenantUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityTenantUser.CreateAnonymousWithFirstLevelSubObjects(securityTenantUser)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityTenantUser entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityTenantUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityTenantUser.CreateAnonymousWithFirstLevelSubObjects(securityTenantUser)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of SecurityTenantUser records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SecurityTenantUsers/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? securityTenantId = null,
			int? securityUserId = null,
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

			IQueryable<Database.SecurityTenantUser> query = (from stu in _context.SecurityTenantUsers select stu);
			if (securityTenantId.HasValue == true)
			{
				query = query.Where(stu => stu.securityTenantId == securityTenantId.Value);
			}
			if (securityUserId.HasValue == true)
			{
				query = query.Where(stu => stu.securityUserId == securityUserId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(stu => stu.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(stu => stu.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(stu => stu.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(stu => stu.deleted == false);
				}
			}
			else
			{
				query = query.Where(stu => stu.active == true);
				query = query.Where(stu => stu.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Security Tenant User, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.securityTenant.name.Contains(anyStringContains)
			       || x.securityTenant.description.Contains(anyStringContains)
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


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.SecurityTenantUser.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/SecurityTenantUser/CreateAuditEvent")]
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
