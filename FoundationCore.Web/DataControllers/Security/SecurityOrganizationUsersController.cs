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
    /// This auto generated class provides the basic CRUD operations for the SecurityOrganizationUser entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SecurityOrganizationUser entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SecurityOrganizationUsersController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SecurityContext _context;

		private ILogger<SecurityOrganizationUsersController> _logger;

		public SecurityOrganizationUsersController(SecurityContext context, ILogger<SecurityOrganizationUsersController> logger) : base("Security", "SecurityOrganizationUser")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of SecurityOrganizationUsers filtered by the parameters provided.
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
		[Route("api/SecurityOrganizationUsers")]
		public async Task<IActionResult> GetSecurityOrganizationUsers(
			int? securityOrganizationId = null,
			int? securityUserId = null,
			bool? canRead = null,
			bool? canWrite = null,
			bool? canChangeHierarchy = null,
			bool? canChangeOwner = null,
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

			IQueryable<Database.SecurityOrganizationUser> query = (from sou in _context.SecurityOrganizationUsers select sou);
			if (securityOrganizationId.HasValue == true)
			{
				query = query.Where(sou => sou.securityOrganizationId == securityOrganizationId.Value);
			}
			if (securityUserId.HasValue == true)
			{
				query = query.Where(sou => sou.securityUserId == securityUserId.Value);
			}
			if (canRead.HasValue == true)
			{
				query = query.Where(sou => sou.canRead == canRead.Value);
			}
			if (canWrite.HasValue == true)
			{
				query = query.Where(sou => sou.canWrite == canWrite.Value);
			}
			if (canChangeHierarchy.HasValue == true)
			{
				query = query.Where(sou => sou.canChangeHierarchy == canChangeHierarchy.Value);
			}
			if (canChangeOwner.HasValue == true)
			{
				query = query.Where(sou => sou.canChangeOwner == canChangeOwner.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sou => sou.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sou => sou.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sou => sou.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sou => sou.deleted == false);
				}
			}
			else
			{
				query = query.Where(sou => sou.active == true);
				query = query.Where(sou => sou.deleted == false);
			}

			query = query.OrderBy(sou => sou.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.securityOrganization);
				query = query.Include(x => x.securityUser);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Security Organization User, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       (includeRelations == true && x.securityOrganization.name.Contains(anyStringContains))
			       || (includeRelations == true && x.securityOrganization.description.Contains(anyStringContains))
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
			
			List<Database.SecurityOrganizationUser> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SecurityOrganizationUser securityOrganizationUser in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(securityOrganizationUser, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Security.SecurityOrganizationUser Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Security.SecurityOrganizationUser Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of SecurityOrganizationUsers filtered by the parameters provided.  Its query is similar to the GetSecurityOrganizationUsers method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityOrganizationUsers/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? securityOrganizationId = null,
			int? securityUserId = null,
			bool? canRead = null,
			bool? canWrite = null,
			bool? canChangeHierarchy = null,
			bool? canChangeOwner = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.SecurityOrganizationUser> query = (from sou in _context.SecurityOrganizationUsers select sou);
			if (securityOrganizationId.HasValue == true)
			{
				query = query.Where(sou => sou.securityOrganizationId == securityOrganizationId.Value);
			}
			if (securityUserId.HasValue == true)
			{
				query = query.Where(sou => sou.securityUserId == securityUserId.Value);
			}
			if (canRead.HasValue == true)
			{
				query = query.Where(sou => sou.canRead == canRead.Value);
			}
			if (canWrite.HasValue == true)
			{
				query = query.Where(sou => sou.canWrite == canWrite.Value);
			}
			if (canChangeHierarchy.HasValue == true)
			{
				query = query.Where(sou => sou.canChangeHierarchy == canChangeHierarchy.Value);
			}
			if (canChangeOwner.HasValue == true)
			{
				query = query.Where(sou => sou.canChangeOwner == canChangeOwner.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sou => sou.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sou => sou.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sou => sou.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sou => sou.deleted == false);
				}
			}
			else
			{
				query = query.Where(sou => sou.active == true);
				query = query.Where(sou => sou.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Security Organization User, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.securityOrganization.name.Contains(anyStringContains)
			       || x.securityOrganization.description.Contains(anyStringContains)
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
        /// This gets a single SecurityOrganizationUser by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityOrganizationUser/{id}")]
		public async Task<IActionResult> GetSecurityOrganizationUser(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.SecurityOrganizationUser> query = (from sou in _context.SecurityOrganizationUsers where
							(sou.id == id) &&
							(userIsAdmin == true || sou.deleted == false) &&
							(userIsWriter == true || sou.active == true)
					select sou);

				if (includeRelations == true)
				{
					query = query.Include(x => x.securityOrganization);
					query = query.Include(x => x.securityUser);
					query = query.AsSplitQuery();
				}

				Database.SecurityOrganizationUser materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Security.SecurityOrganizationUser Entity was read with Admin privilege." : "Security.SecurityOrganizationUser Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityOrganizationUser", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Security.SecurityOrganizationUser entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Security.SecurityOrganizationUser.   Entity was read with Admin privilege." : "Exception caught during entity read of Security.SecurityOrganizationUser.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing SecurityOrganizationUser record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SecurityOrganizationUser/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSecurityOrganizationUser(int id, [FromBody]Database.SecurityOrganizationUser.SecurityOrganizationUserDTO securityOrganizationUserDTO, CancellationToken cancellationToken = default)
		{
			if (securityOrganizationUserDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Security Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != securityOrganizationUserDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.SecurityOrganizationUser> query = (from x in _context.SecurityOrganizationUsers
				where
				(x.id == id)
				select x);


			Database.SecurityOrganizationUser existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityOrganizationUser PUT", id.ToString(), new Exception("No Security.SecurityOrganizationUser entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (securityOrganizationUserDTO.objectGuid == Guid.Empty)
            {
                securityOrganizationUserDTO.objectGuid = existing.objectGuid;
            }
            else if (securityOrganizationUserDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a SecurityOrganizationUser record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SecurityOrganizationUser cloneOfExisting = (Database.SecurityOrganizationUser)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SecurityOrganizationUser object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SecurityOrganizationUser securityOrganizationUser = (Database.SecurityOrganizationUser)_context.Entry(existing).GetDatabaseValues().ToObject();
			securityOrganizationUser.ApplyDTO(securityOrganizationUserDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (securityOrganizationUser.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Security.SecurityOrganizationUser record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			EntityEntry<Database.SecurityOrganizationUser> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(securityOrganizationUser);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityOrganizationUser entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityOrganizationUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityOrganizationUser.CreateAnonymousWithFirstLevelSubObjects(securityOrganizationUser)),
					null);


				return Ok(Database.SecurityOrganizationUser.CreateAnonymous(securityOrganizationUser));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityOrganizationUser entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityOrganizationUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityOrganizationUser.CreateAnonymousWithFirstLevelSubObjects(securityOrganizationUser)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new SecurityOrganizationUser record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityOrganizationUser", Name = "SecurityOrganizationUser")]
		public async Task<IActionResult> PostSecurityOrganizationUser([FromBody]Database.SecurityOrganizationUser.SecurityOrganizationUserDTO securityOrganizationUserDTO, CancellationToken cancellationToken = default)
		{
			if (securityOrganizationUserDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Security Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new SecurityOrganizationUser object using the data from the DTO
			//
			Database.SecurityOrganizationUser securityOrganizationUser = Database.SecurityOrganizationUser.FromDTO(securityOrganizationUserDTO);

			try
			{
				securityOrganizationUser.objectGuid = Guid.NewGuid();
				_context.SecurityOrganizationUsers.Add(securityOrganizationUser);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Security.SecurityOrganizationUser entity successfully created.",
					true,
					securityOrganizationUser.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.SecurityOrganizationUser.CreateAnonymousWithFirstLevelSubObjects(securityOrganizationUser)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Security.SecurityOrganizationUser entity creation failed.", false, securityOrganizationUser.id.ToString(), "", JsonSerializer.Serialize(securityOrganizationUser), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityOrganizationUser", securityOrganizationUser.id, securityOrganizationUser.id.ToString()));

			return CreatedAtRoute("SecurityOrganizationUser", new { id = securityOrganizationUser.id }, Database.SecurityOrganizationUser.CreateAnonymousWithFirstLevelSubObjects(securityOrganizationUser));
		}



        /// <summary>
        /// 
        /// This deletes a SecurityOrganizationUser record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityOrganizationUser/{id}")]
		[Route("api/SecurityOrganizationUser")]
		public async Task<IActionResult> DeleteSecurityOrganizationUser(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Security Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.SecurityOrganizationUser> query = (from x in _context.SecurityOrganizationUsers
				where
				(x.id == id)
				select x);


			Database.SecurityOrganizationUser securityOrganizationUser = await query.FirstOrDefaultAsync(cancellationToken);

			if (securityOrganizationUser == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityOrganizationUser DELETE", id.ToString(), new Exception("No Security.SecurityOrganizationUser entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SecurityOrganizationUser cloneOfExisting = (Database.SecurityOrganizationUser)_context.Entry(securityOrganizationUser).GetDatabaseValues().ToObject();


			try
			{
				securityOrganizationUser.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityOrganizationUser entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityOrganizationUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityOrganizationUser.CreateAnonymousWithFirstLevelSubObjects(securityOrganizationUser)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityOrganizationUser entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityOrganizationUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityOrganizationUser.CreateAnonymousWithFirstLevelSubObjects(securityOrganizationUser)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of SecurityOrganizationUser records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SecurityOrganizationUsers/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? securityOrganizationId = null,
			int? securityUserId = null,
			bool? canRead = null,
			bool? canWrite = null,
			bool? canChangeHierarchy = null,
			bool? canChangeOwner = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);


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

			IQueryable<Database.SecurityOrganizationUser> query = (from sou in _context.SecurityOrganizationUsers select sou);
			if (securityOrganizationId.HasValue == true)
			{
				query = query.Where(sou => sou.securityOrganizationId == securityOrganizationId.Value);
			}
			if (securityUserId.HasValue == true)
			{
				query = query.Where(sou => sou.securityUserId == securityUserId.Value);
			}
			if (canRead.HasValue == true)
			{
				query = query.Where(sou => sou.canRead == canRead.Value);
			}
			if (canWrite.HasValue == true)
			{
				query = query.Where(sou => sou.canWrite == canWrite.Value);
			}
			if (canChangeHierarchy.HasValue == true)
			{
				query = query.Where(sou => sou.canChangeHierarchy == canChangeHierarchy.Value);
			}
			if (canChangeOwner.HasValue == true)
			{
				query = query.Where(sou => sou.canChangeOwner == canChangeOwner.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sou => sou.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sou => sou.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sou => sou.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sou => sou.deleted == false);
				}
			}
			else
			{
				query = query.Where(sou => sou.active == true);
				query = query.Where(sou => sou.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Security Organization User, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.securityOrganization.name.Contains(anyStringContains)
			       || x.securityOrganization.description.Contains(anyStringContains)
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
			return Ok(await (from queryData in query select Database.SecurityOrganizationUser.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/SecurityOrganizationUser/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Security Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
