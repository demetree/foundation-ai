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
    /// This auto generated class provides the basic CRUD operations for the SecurityDepartmentUser entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SecurityDepartmentUser entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SecurityDepartmentUsersController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SecurityContext _context;

		private ILogger<SecurityDepartmentUsersController> _logger;

		public SecurityDepartmentUsersController(SecurityContext context, ILogger<SecurityDepartmentUsersController> logger) : base("Security", "SecurityDepartmentUser")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of SecurityDepartmentUsers filtered by the parameters provided.
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
		[Route("api/SecurityDepartmentUsers")]
		public async Task<IActionResult> GetSecurityDepartmentUsers(
			int? securityDepartmentId = null,
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

			IQueryable<Database.SecurityDepartmentUser> query = (from sdu in _context.SecurityDepartmentUsers select sdu);
			if (securityDepartmentId.HasValue == true)
			{
				query = query.Where(sdu => sdu.securityDepartmentId == securityDepartmentId.Value);
			}
			if (securityUserId.HasValue == true)
			{
				query = query.Where(sdu => sdu.securityUserId == securityUserId.Value);
			}
			if (canRead.HasValue == true)
			{
				query = query.Where(sdu => sdu.canRead == canRead.Value);
			}
			if (canWrite.HasValue == true)
			{
				query = query.Where(sdu => sdu.canWrite == canWrite.Value);
			}
			if (canChangeHierarchy.HasValue == true)
			{
				query = query.Where(sdu => sdu.canChangeHierarchy == canChangeHierarchy.Value);
			}
			if (canChangeOwner.HasValue == true)
			{
				query = query.Where(sdu => sdu.canChangeOwner == canChangeOwner.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sdu => sdu.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sdu => sdu.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sdu => sdu.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sdu => sdu.deleted == false);
				}
			}
			else
			{
				query = query.Where(sdu => sdu.active == true);
				query = query.Where(sdu => sdu.deleted == false);
			}

			query = query.OrderBy(sdu => sdu.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.securityDepartment);
				query = query.Include(x => x.securityUser);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Security Department User, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       (includeRelations == true && x.securityDepartment.name.Contains(anyStringContains))
			       || (includeRelations == true && x.securityDepartment.description.Contains(anyStringContains))
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
			
			List<Database.SecurityDepartmentUser> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SecurityDepartmentUser securityDepartmentUser in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(securityDepartmentUser, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Security.SecurityDepartmentUser Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Security.SecurityDepartmentUser Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of SecurityDepartmentUsers filtered by the parameters provided.  Its query is similar to the GetSecurityDepartmentUsers method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityDepartmentUsers/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? securityDepartmentId = null,
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

			IQueryable<Database.SecurityDepartmentUser> query = (from sdu in _context.SecurityDepartmentUsers select sdu);
			if (securityDepartmentId.HasValue == true)
			{
				query = query.Where(sdu => sdu.securityDepartmentId == securityDepartmentId.Value);
			}
			if (securityUserId.HasValue == true)
			{
				query = query.Where(sdu => sdu.securityUserId == securityUserId.Value);
			}
			if (canRead.HasValue == true)
			{
				query = query.Where(sdu => sdu.canRead == canRead.Value);
			}
			if (canWrite.HasValue == true)
			{
				query = query.Where(sdu => sdu.canWrite == canWrite.Value);
			}
			if (canChangeHierarchy.HasValue == true)
			{
				query = query.Where(sdu => sdu.canChangeHierarchy == canChangeHierarchy.Value);
			}
			if (canChangeOwner.HasValue == true)
			{
				query = query.Where(sdu => sdu.canChangeOwner == canChangeOwner.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sdu => sdu.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sdu => sdu.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sdu => sdu.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sdu => sdu.deleted == false);
				}
			}
			else
			{
				query = query.Where(sdu => sdu.active == true);
				query = query.Where(sdu => sdu.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Security Department User, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.securityDepartment.name.Contains(anyStringContains)
			       || x.securityDepartment.description.Contains(anyStringContains)
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
        /// This gets a single SecurityDepartmentUser by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityDepartmentUser/{id}")]
		public async Task<IActionResult> GetSecurityDepartmentUser(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.SecurityDepartmentUser> query = (from sdu in _context.SecurityDepartmentUsers where
							(sdu.id == id) &&
							(userIsAdmin == true || sdu.deleted == false) &&
							(userIsWriter == true || sdu.active == true)
					select sdu);

				if (includeRelations == true)
				{
					query = query.Include(x => x.securityDepartment);
					query = query.Include(x => x.securityUser);
					query = query.AsSplitQuery();
				}

				Database.SecurityDepartmentUser materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Security.SecurityDepartmentUser Entity was read with Admin privilege." : "Security.SecurityDepartmentUser Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityDepartmentUser", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Security.SecurityDepartmentUser entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Security.SecurityDepartmentUser.   Entity was read with Admin privilege." : "Exception caught during entity read of Security.SecurityDepartmentUser.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing SecurityDepartmentUser record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SecurityDepartmentUser/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSecurityDepartmentUser(int id, [FromBody]Database.SecurityDepartmentUser.SecurityDepartmentUserDTO securityDepartmentUserDTO, CancellationToken cancellationToken = default)
		{
			if (securityDepartmentUserDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != securityDepartmentUserDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.SecurityDepartmentUser> query = (from x in _context.SecurityDepartmentUsers
				where
				(x.id == id)
				select x);


			Database.SecurityDepartmentUser existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityDepartmentUser PUT", id.ToString(), new Exception("No Security.SecurityDepartmentUser entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (securityDepartmentUserDTO.objectGuid == Guid.Empty)
            {
                securityDepartmentUserDTO.objectGuid = existing.objectGuid;
            }
            else if (securityDepartmentUserDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a SecurityDepartmentUser record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SecurityDepartmentUser cloneOfExisting = (Database.SecurityDepartmentUser)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SecurityDepartmentUser object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SecurityDepartmentUser securityDepartmentUser = (Database.SecurityDepartmentUser)_context.Entry(existing).GetDatabaseValues().ToObject();
			securityDepartmentUser.ApplyDTO(securityDepartmentUserDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (securityDepartmentUser.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Security.SecurityDepartmentUser record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			EntityEntry<Database.SecurityDepartmentUser> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(securityDepartmentUser);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityDepartmentUser entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityDepartmentUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityDepartmentUser.CreateAnonymousWithFirstLevelSubObjects(securityDepartmentUser)),
					null);


				return Ok(Database.SecurityDepartmentUser.CreateAnonymous(securityDepartmentUser));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityDepartmentUser entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityDepartmentUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityDepartmentUser.CreateAnonymousWithFirstLevelSubObjects(securityDepartmentUser)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new SecurityDepartmentUser record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityDepartmentUser", Name = "SecurityDepartmentUser")]
		public async Task<IActionResult> PostSecurityDepartmentUser([FromBody]Database.SecurityDepartmentUser.SecurityDepartmentUserDTO securityDepartmentUserDTO, CancellationToken cancellationToken = default)
		{
			if (securityDepartmentUserDTO == null)
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
			// Create a new SecurityDepartmentUser object using the data from the DTO
			//
			Database.SecurityDepartmentUser securityDepartmentUser = Database.SecurityDepartmentUser.FromDTO(securityDepartmentUserDTO);

			try
			{
				securityDepartmentUser.objectGuid = Guid.NewGuid();
				_context.SecurityDepartmentUsers.Add(securityDepartmentUser);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Security.SecurityDepartmentUser entity successfully created.",
					true,
					securityDepartmentUser.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.SecurityDepartmentUser.CreateAnonymousWithFirstLevelSubObjects(securityDepartmentUser)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Security.SecurityDepartmentUser entity creation failed.", false, securityDepartmentUser.id.ToString(), "", JsonSerializer.Serialize(securityDepartmentUser), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityDepartmentUser", securityDepartmentUser.id, securityDepartmentUser.id.ToString()));

			return CreatedAtRoute("SecurityDepartmentUser", new { id = securityDepartmentUser.id }, Database.SecurityDepartmentUser.CreateAnonymousWithFirstLevelSubObjects(securityDepartmentUser));
		}



        /// <summary>
        /// 
        /// This deletes a SecurityDepartmentUser record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityDepartmentUser/{id}")]
		[Route("api/SecurityDepartmentUser")]
		public async Task<IActionResult> DeleteSecurityDepartmentUser(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.SecurityDepartmentUser> query = (from x in _context.SecurityDepartmentUsers
				where
				(x.id == id)
				select x);


			Database.SecurityDepartmentUser securityDepartmentUser = await query.FirstOrDefaultAsync(cancellationToken);

			if (securityDepartmentUser == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityDepartmentUser DELETE", id.ToString(), new Exception("No Security.SecurityDepartmentUser entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SecurityDepartmentUser cloneOfExisting = (Database.SecurityDepartmentUser)_context.Entry(securityDepartmentUser).GetDatabaseValues().ToObject();


			try
			{
				securityDepartmentUser.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityDepartmentUser entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityDepartmentUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityDepartmentUser.CreateAnonymousWithFirstLevelSubObjects(securityDepartmentUser)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityDepartmentUser entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityDepartmentUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityDepartmentUser.CreateAnonymousWithFirstLevelSubObjects(securityDepartmentUser)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of SecurityDepartmentUser records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SecurityDepartmentUsers/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? securityDepartmentId = null,
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

			IQueryable<Database.SecurityDepartmentUser> query = (from sdu in _context.SecurityDepartmentUsers select sdu);
			if (securityDepartmentId.HasValue == true)
			{
				query = query.Where(sdu => sdu.securityDepartmentId == securityDepartmentId.Value);
			}
			if (securityUserId.HasValue == true)
			{
				query = query.Where(sdu => sdu.securityUserId == securityUserId.Value);
			}
			if (canRead.HasValue == true)
			{
				query = query.Where(sdu => sdu.canRead == canRead.Value);
			}
			if (canWrite.HasValue == true)
			{
				query = query.Where(sdu => sdu.canWrite == canWrite.Value);
			}
			if (canChangeHierarchy.HasValue == true)
			{
				query = query.Where(sdu => sdu.canChangeHierarchy == canChangeHierarchy.Value);
			}
			if (canChangeOwner.HasValue == true)
			{
				query = query.Where(sdu => sdu.canChangeOwner == canChangeOwner.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sdu => sdu.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sdu => sdu.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sdu => sdu.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sdu => sdu.deleted == false);
				}
			}
			else
			{
				query = query.Where(sdu => sdu.active == true);
				query = query.Where(sdu => sdu.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Security Department User, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.securityDepartment.name.Contains(anyStringContains)
			       || x.securityDepartment.description.Contains(anyStringContains)
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
			return Ok(await (from queryData in query select Database.SecurityDepartmentUser.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/SecurityDepartmentUser/CreateAuditEvent")]
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
