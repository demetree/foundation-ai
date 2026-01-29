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
    /// This auto generated class provides the basic CRUD operations for the SecurityTeamUser entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SecurityTeamUser entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SecurityTeamUsersController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SecurityContext _context;

		private ILogger<SecurityTeamUsersController> _logger;

		public SecurityTeamUsersController(SecurityContext context, ILogger<SecurityTeamUsersController> logger) : base("Security", "SecurityTeamUser")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of SecurityTeamUsers filtered by the parameters provided.
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
		[Route("api/SecurityTeamUsers")]
		public async Task<IActionResult> GetSecurityTeamUsers(
			int? securityTeamId = null,
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

			IQueryable<Database.SecurityTeamUser> query = (from stu in _context.SecurityTeamUsers select stu);
			if (securityTeamId.HasValue == true)
			{
				query = query.Where(stu => stu.securityTeamId == securityTeamId.Value);
			}
			if (securityUserId.HasValue == true)
			{
				query = query.Where(stu => stu.securityUserId == securityUserId.Value);
			}
			if (canRead.HasValue == true)
			{
				query = query.Where(stu => stu.canRead == canRead.Value);
			}
			if (canWrite.HasValue == true)
			{
				query = query.Where(stu => stu.canWrite == canWrite.Value);
			}
			if (canChangeHierarchy.HasValue == true)
			{
				query = query.Where(stu => stu.canChangeHierarchy == canChangeHierarchy.Value);
			}
			if (canChangeOwner.HasValue == true)
			{
				query = query.Where(stu => stu.canChangeOwner == canChangeOwner.Value);
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
				query = query.Include(x => x.securityTeam);
				query = query.Include(x => x.securityUser);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Security Team User, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       (includeRelations == true && x.securityTeam.name.Contains(anyStringContains))
			       || (includeRelations == true && x.securityTeam.description.Contains(anyStringContains))
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
			
			List<Database.SecurityTeamUser> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SecurityTeamUser securityTeamUser in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(securityTeamUser, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Security.SecurityTeamUser Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Security.SecurityTeamUser Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of SecurityTeamUsers filtered by the parameters provided.  Its query is similar to the GetSecurityTeamUsers method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityTeamUsers/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? securityTeamId = null,
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
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.SecurityTeamUser> query = (from stu in _context.SecurityTeamUsers select stu);
			if (securityTeamId.HasValue == true)
			{
				query = query.Where(stu => stu.securityTeamId == securityTeamId.Value);
			}
			if (securityUserId.HasValue == true)
			{
				query = query.Where(stu => stu.securityUserId == securityUserId.Value);
			}
			if (canRead.HasValue == true)
			{
				query = query.Where(stu => stu.canRead == canRead.Value);
			}
			if (canWrite.HasValue == true)
			{
				query = query.Where(stu => stu.canWrite == canWrite.Value);
			}
			if (canChangeHierarchy.HasValue == true)
			{
				query = query.Where(stu => stu.canChangeHierarchy == canChangeHierarchy.Value);
			}
			if (canChangeOwner.HasValue == true)
			{
				query = query.Where(stu => stu.canChangeOwner == canChangeOwner.Value);
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
			// Add the any string contains parameter to span all the string fields on the Security Team User, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.securityTeam.name.Contains(anyStringContains)
			       || x.securityTeam.description.Contains(anyStringContains)
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
        /// This gets a single SecurityTeamUser by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityTeamUser/{id}")]
		public async Task<IActionResult> GetSecurityTeamUser(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.SecurityTeamUser> query = (from stu in _context.SecurityTeamUsers where
							(stu.id == id) &&
							(userIsAdmin == true || stu.deleted == false) &&
							(userIsWriter == true || stu.active == true)
					select stu);

				if (includeRelations == true)
				{
					query = query.Include(x => x.securityTeam);
					query = query.Include(x => x.securityUser);
					query = query.AsSplitQuery();
				}

				Database.SecurityTeamUser materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Security.SecurityTeamUser Entity was read with Admin privilege." : "Security.SecurityTeamUser Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityTeamUser", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Security.SecurityTeamUser entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Security.SecurityTeamUser.   Entity was read with Admin privilege." : "Exception caught during entity read of Security.SecurityTeamUser.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing SecurityTeamUser record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SecurityTeamUser/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSecurityTeamUser(int id, [FromBody]Database.SecurityTeamUser.SecurityTeamUserDTO securityTeamUserDTO, CancellationToken cancellationToken = default)
		{
			if (securityTeamUserDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != securityTeamUserDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.SecurityTeamUser> query = (from x in _context.SecurityTeamUsers
				where
				(x.id == id)
				select x);


			Database.SecurityTeamUser existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityTeamUser PUT", id.ToString(), new Exception("No Security.SecurityTeamUser entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (securityTeamUserDTO.objectGuid == Guid.Empty)
            {
                securityTeamUserDTO.objectGuid = existing.objectGuid;
            }
            else if (securityTeamUserDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a SecurityTeamUser record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SecurityTeamUser cloneOfExisting = (Database.SecurityTeamUser)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SecurityTeamUser object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SecurityTeamUser securityTeamUser = (Database.SecurityTeamUser)_context.Entry(existing).GetDatabaseValues().ToObject();
			securityTeamUser.ApplyDTO(securityTeamUserDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (securityTeamUser.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Security.SecurityTeamUser record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			EntityEntry<Database.SecurityTeamUser> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(securityTeamUser);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityTeamUser entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityTeamUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityTeamUser.CreateAnonymousWithFirstLevelSubObjects(securityTeamUser)),
					null);


				return Ok(Database.SecurityTeamUser.CreateAnonymous(securityTeamUser));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityTeamUser entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityTeamUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityTeamUser.CreateAnonymousWithFirstLevelSubObjects(securityTeamUser)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new SecurityTeamUser record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityTeamUser", Name = "SecurityTeamUser")]
		public async Task<IActionResult> PostSecurityTeamUser([FromBody]Database.SecurityTeamUser.SecurityTeamUserDTO securityTeamUserDTO, CancellationToken cancellationToken = default)
		{
			if (securityTeamUserDTO == null)
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
			// Create a new SecurityTeamUser object using the data from the DTO
			//
			Database.SecurityTeamUser securityTeamUser = Database.SecurityTeamUser.FromDTO(securityTeamUserDTO);

			try
			{
				securityTeamUser.objectGuid = Guid.NewGuid();
				_context.SecurityTeamUsers.Add(securityTeamUser);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Security.SecurityTeamUser entity successfully created.",
					true,
					securityTeamUser.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.SecurityTeamUser.CreateAnonymousWithFirstLevelSubObjects(securityTeamUser)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Security.SecurityTeamUser entity creation failed.", false, securityTeamUser.id.ToString(), "", JsonSerializer.Serialize(securityTeamUser), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityTeamUser", securityTeamUser.id, securityTeamUser.id.ToString()));

			return CreatedAtRoute("SecurityTeamUser", new { id = securityTeamUser.id }, Database.SecurityTeamUser.CreateAnonymousWithFirstLevelSubObjects(securityTeamUser));
		}



        /// <summary>
        /// 
        /// This deletes a SecurityTeamUser record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityTeamUser/{id}")]
		[Route("api/SecurityTeamUser")]
		public async Task<IActionResult> DeleteSecurityTeamUser(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.SecurityTeamUser> query = (from x in _context.SecurityTeamUsers
				where
				(x.id == id)
				select x);


			Database.SecurityTeamUser securityTeamUser = await query.FirstOrDefaultAsync(cancellationToken);

			if (securityTeamUser == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityTeamUser DELETE", id.ToString(), new Exception("No Security.SecurityTeamUser entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SecurityTeamUser cloneOfExisting = (Database.SecurityTeamUser)_context.Entry(securityTeamUser).GetDatabaseValues().ToObject();


			try
			{
				securityTeamUser.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityTeamUser entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityTeamUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityTeamUser.CreateAnonymousWithFirstLevelSubObjects(securityTeamUser)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityTeamUser entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityTeamUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityTeamUser.CreateAnonymousWithFirstLevelSubObjects(securityTeamUser)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of SecurityTeamUser records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SecurityTeamUsers/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? securityTeamId = null,
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

			IQueryable<Database.SecurityTeamUser> query = (from stu in _context.SecurityTeamUsers select stu);
			if (securityTeamId.HasValue == true)
			{
				query = query.Where(stu => stu.securityTeamId == securityTeamId.Value);
			}
			if (securityUserId.HasValue == true)
			{
				query = query.Where(stu => stu.securityUserId == securityUserId.Value);
			}
			if (canRead.HasValue == true)
			{
				query = query.Where(stu => stu.canRead == canRead.Value);
			}
			if (canWrite.HasValue == true)
			{
				query = query.Where(stu => stu.canWrite == canWrite.Value);
			}
			if (canChangeHierarchy.HasValue == true)
			{
				query = query.Where(stu => stu.canChangeHierarchy == canChangeHierarchy.Value);
			}
			if (canChangeOwner.HasValue == true)
			{
				query = query.Where(stu => stu.canChangeOwner == canChangeOwner.Value);
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
			// Add the any string contains parameter to span all the string fields on the Security Team User, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.securityTeam.name.Contains(anyStringContains)
			       || x.securityTeam.description.Contains(anyStringContains)
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
			return Ok(await (from queryData in query select Database.SecurityTeamUser.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/SecurityTeamUser/CreateAuditEvent")]
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
