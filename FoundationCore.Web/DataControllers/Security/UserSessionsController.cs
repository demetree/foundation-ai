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
    /// This auto generated class provides the basic CRUD operations for the UserSession entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the UserSession entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class UserSessionsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 100;

		private SecurityContext _context;

		private ILogger<UserSessionsController> _logger;

		public UserSessionsController(SecurityContext context, ILogger<UserSessionsController> logger) : base("Security", "UserSession")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of UserSessions filtered by the parameters provided.
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
		[Route("api/UserSessions")]
		public async Task<IActionResult> GetUserSessions(
			int? securityUserId = null,
			Guid? objectGuid = null,
			string tokenId = null,
			DateTime? sessionStart = null,
			DateTime? expiresAt = null,
			string ipAddress = null,
			string userAgent = null,
			string loginMethod = null,
			string clientApplication = null,
			bool? isRevoked = null,
			DateTime? revokedAt = null,
			string revokedBy = null,
			string revokedReason = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (sessionStart.HasValue == true && sessionStart.Value.Kind != DateTimeKind.Utc)
			{
				sessionStart = sessionStart.Value.ToUniversalTime();
			}

			if (expiresAt.HasValue == true && expiresAt.Value.Kind != DateTimeKind.Utc)
			{
				expiresAt = expiresAt.Value.ToUniversalTime();
			}

			if (revokedAt.HasValue == true && revokedAt.Value.Kind != DateTimeKind.Utc)
			{
				revokedAt = revokedAt.Value.ToUniversalTime();
			}

			IQueryable<Database.UserSession> query = (from us in _context.UserSessions select us);
			if (securityUserId.HasValue == true)
			{
				query = query.Where(us => us.securityUserId == securityUserId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(us => us.objectGuid == objectGuid);
			}
			if (string.IsNullOrEmpty(tokenId) == false)
			{
				query = query.Where(us => us.tokenId == tokenId);
			}
			if (sessionStart.HasValue == true)
			{
				query = query.Where(us => us.sessionStart == sessionStart.Value);
			}
			if (expiresAt.HasValue == true)
			{
				query = query.Where(us => us.expiresAt == expiresAt.Value);
			}
			if (string.IsNullOrEmpty(ipAddress) == false)
			{
				query = query.Where(us => us.ipAddress == ipAddress);
			}
			if (string.IsNullOrEmpty(userAgent) == false)
			{
				query = query.Where(us => us.userAgent == userAgent);
			}
			if (string.IsNullOrEmpty(loginMethod) == false)
			{
				query = query.Where(us => us.loginMethod == loginMethod);
			}
			if (string.IsNullOrEmpty(clientApplication) == false)
			{
				query = query.Where(us => us.clientApplication == clientApplication);
			}
			if (isRevoked.HasValue == true)
			{
				query = query.Where(us => us.isRevoked == isRevoked.Value);
			}
			if (revokedAt.HasValue == true)
			{
				query = query.Where(us => us.revokedAt == revokedAt.Value);
			}
			if (string.IsNullOrEmpty(revokedBy) == false)
			{
				query = query.Where(us => us.revokedBy == revokedBy);
			}
			if (string.IsNullOrEmpty(revokedReason) == false)
			{
				query = query.Where(us => us.revokedReason == revokedReason);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(us => us.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(us => us.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(us => us.deleted == false);
				}
			}
			else
			{
				query = query.Where(us => us.active == true);
				query = query.Where(us => us.deleted == false);
			}

			query = query.OrderByDescending(us => us.sessionStart);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.securityUser);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Session, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.tokenId.Contains(anyStringContains)
			       || x.ipAddress.Contains(anyStringContains)
			       || x.userAgent.Contains(anyStringContains)
			       || x.loginMethod.Contains(anyStringContains)
			       || x.clientApplication.Contains(anyStringContains)
			       || x.revokedBy.Contains(anyStringContains)
			       || x.revokedReason.Contains(anyStringContains)
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
			
			List<Database.UserSession> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.UserSession userSession in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(userSession, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Security.UserSession Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Security.UserSession Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of UserSessions filtered by the parameters provided.  Its query is similar to the GetUserSessions method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserSessions/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? securityUserId = null,
			Guid? objectGuid = null,
			string tokenId = null,
			DateTime? sessionStart = null,
			DateTime? expiresAt = null,
			string ipAddress = null,
			string userAgent = null,
			string loginMethod = null,
			string clientApplication = null,
			bool? isRevoked = null,
			DateTime? revokedAt = null,
			string revokedBy = null,
			string revokedReason = null,
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

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (sessionStart.HasValue == true && sessionStart.Value.Kind != DateTimeKind.Utc)
			{
				sessionStart = sessionStart.Value.ToUniversalTime();
			}

			if (expiresAt.HasValue == true && expiresAt.Value.Kind != DateTimeKind.Utc)
			{
				expiresAt = expiresAt.Value.ToUniversalTime();
			}

			if (revokedAt.HasValue == true && revokedAt.Value.Kind != DateTimeKind.Utc)
			{
				revokedAt = revokedAt.Value.ToUniversalTime();
			}

			IQueryable<Database.UserSession> query = (from us in _context.UserSessions select us);
			if (securityUserId.HasValue == true)
			{
				query = query.Where(us => us.securityUserId == securityUserId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(us => us.objectGuid == objectGuid);
			}
			if (tokenId != null)
			{
				query = query.Where(us => us.tokenId == tokenId);
			}
			if (sessionStart.HasValue == true)
			{
				query = query.Where(us => us.sessionStart == sessionStart.Value);
			}
			if (expiresAt.HasValue == true)
			{
				query = query.Where(us => us.expiresAt == expiresAt.Value);
			}
			if (ipAddress != null)
			{
				query = query.Where(us => us.ipAddress == ipAddress);
			}
			if (userAgent != null)
			{
				query = query.Where(us => us.userAgent == userAgent);
			}
			if (loginMethod != null)
			{
				query = query.Where(us => us.loginMethod == loginMethod);
			}
			if (clientApplication != null)
			{
				query = query.Where(us => us.clientApplication == clientApplication);
			}
			if (isRevoked.HasValue == true)
			{
				query = query.Where(us => us.isRevoked == isRevoked.Value);
			}
			if (revokedAt.HasValue == true)
			{
				query = query.Where(us => us.revokedAt == revokedAt.Value);
			}
			if (revokedBy != null)
			{
				query = query.Where(us => us.revokedBy == revokedBy);
			}
			if (revokedReason != null)
			{
				query = query.Where(us => us.revokedReason == revokedReason);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(us => us.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(us => us.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(us => us.deleted == false);
				}
			}
			else
			{
				query = query.Where(us => us.active == true);
				query = query.Where(us => us.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the User Session, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.tokenId.Contains(anyStringContains)
			       || x.ipAddress.Contains(anyStringContains)
			       || x.userAgent.Contains(anyStringContains)
			       || x.loginMethod.Contains(anyStringContains)
			       || x.clientApplication.Contains(anyStringContains)
			       || x.revokedBy.Contains(anyStringContains)
			       || x.revokedReason.Contains(anyStringContains)
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
        /// This gets a single UserSession by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserSession/{id}")]
		public async Task<IActionResult> GetUserSession(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.UserSession> query = (from us in _context.UserSessions where
							(us.id == id) &&
							(userIsAdmin == true || us.deleted == false) &&
							(userIsWriter == true || us.active == true)
					select us);

				if (includeRelations == true)
				{
					query = query.Include(x => x.securityUser);
					query = query.AsSplitQuery();
				}

				Database.UserSession materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Security.UserSession Entity was read with Admin privilege." : "Security.UserSession Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserSession", materialized.id, materialized.tokenId));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Security.UserSession entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Security.UserSession.   Entity was read with Admin privilege." : "Exception caught during entity read of Security.UserSession.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing UserSession record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/UserSession/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutUserSession(int id, [FromBody]Database.UserSession.UserSessionDTO userSessionDTO, CancellationToken cancellationToken = default)
		{
			if (userSessionDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != userSessionDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.UserSession> query = (from x in _context.UserSessions
				where
				(x.id == id)
				select x);


			Database.UserSession existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.UserSession PUT", id.ToString(), new Exception("No Security.UserSession entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (userSessionDTO.objectGuid == Guid.Empty)
            {
                userSessionDTO.objectGuid = existing.objectGuid;
            }
            else if (userSessionDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a UserSession record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.UserSession cloneOfExisting = (Database.UserSession)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new UserSession object using the data from the existing record, updated with what is in the DTO.
			//
			Database.UserSession userSession = (Database.UserSession)_context.Entry(existing).GetDatabaseValues().ToObject();
			userSession.ApplyDTO(userSessionDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (userSession.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Security.UserSession record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (userSession.tokenId != null && userSession.tokenId.Length > 250)
			{
				userSession.tokenId = userSession.tokenId.Substring(0, 250);
			}

			if (userSession.sessionStart.Kind != DateTimeKind.Utc)
			{
				userSession.sessionStart = userSession.sessionStart.ToUniversalTime();
			}

			if (userSession.expiresAt.Kind != DateTimeKind.Utc)
			{
				userSession.expiresAt = userSession.expiresAt.ToUniversalTime();
			}

			if (userSession.ipAddress != null && userSession.ipAddress.Length > 50)
			{
				userSession.ipAddress = userSession.ipAddress.Substring(0, 50);
			}

			if (userSession.userAgent != null && userSession.userAgent.Length > 500)
			{
				userSession.userAgent = userSession.userAgent.Substring(0, 500);
			}

			if (userSession.loginMethod != null && userSession.loginMethod.Length > 50)
			{
				userSession.loginMethod = userSession.loginMethod.Substring(0, 50);
			}

			if (userSession.clientApplication != null && userSession.clientApplication.Length > 100)
			{
				userSession.clientApplication = userSession.clientApplication.Substring(0, 100);
			}

			if (userSession.revokedAt.HasValue == true && userSession.revokedAt.Value.Kind != DateTimeKind.Utc)
			{
				userSession.revokedAt = userSession.revokedAt.Value.ToUniversalTime();
			}

			if (userSession.revokedBy != null && userSession.revokedBy.Length > 100)
			{
				userSession.revokedBy = userSession.revokedBy.Substring(0, 100);
			}

			if (userSession.revokedReason != null && userSession.revokedReason.Length > 500)
			{
				userSession.revokedReason = userSession.revokedReason.Substring(0, 500);
			}

			EntityEntry<Database.UserSession> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(userSession);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Security.UserSession entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserSession.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserSession.CreateAnonymousWithFirstLevelSubObjects(userSession)),
					null);


				return Ok(Database.UserSession.CreateAnonymous(userSession));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Security.UserSession entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserSession.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserSession.CreateAnonymousWithFirstLevelSubObjects(userSession)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new UserSession record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserSession", Name = "UserSession")]
		public async Task<IActionResult> PostUserSession([FromBody]Database.UserSession.UserSessionDTO userSessionDTO, CancellationToken cancellationToken = default)
		{
			if (userSessionDTO == null)
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
			// Create a new UserSession object using the data from the DTO
			//
			Database.UserSession userSession = Database.UserSession.FromDTO(userSessionDTO);

			try
			{
				if (userSession.tokenId != null && userSession.tokenId.Length > 250)
				{
					userSession.tokenId = userSession.tokenId.Substring(0, 250);
				}

				if (userSession.sessionStart.Kind != DateTimeKind.Utc)
				{
					userSession.sessionStart = userSession.sessionStart.ToUniversalTime();
				}

				if (userSession.expiresAt.Kind != DateTimeKind.Utc)
				{
					userSession.expiresAt = userSession.expiresAt.ToUniversalTime();
				}

				if (userSession.ipAddress != null && userSession.ipAddress.Length > 50)
				{
					userSession.ipAddress = userSession.ipAddress.Substring(0, 50);
				}

				if (userSession.userAgent != null && userSession.userAgent.Length > 500)
				{
					userSession.userAgent = userSession.userAgent.Substring(0, 500);
				}

				if (userSession.loginMethod != null && userSession.loginMethod.Length > 50)
				{
					userSession.loginMethod = userSession.loginMethod.Substring(0, 50);
				}

				if (userSession.clientApplication != null && userSession.clientApplication.Length > 100)
				{
					userSession.clientApplication = userSession.clientApplication.Substring(0, 100);
				}

				if (userSession.revokedAt.HasValue == true && userSession.revokedAt.Value.Kind != DateTimeKind.Utc)
				{
					userSession.revokedAt = userSession.revokedAt.Value.ToUniversalTime();
				}

				if (userSession.revokedBy != null && userSession.revokedBy.Length > 100)
				{
					userSession.revokedBy = userSession.revokedBy.Substring(0, 100);
				}

				if (userSession.revokedReason != null && userSession.revokedReason.Length > 500)
				{
					userSession.revokedReason = userSession.revokedReason.Substring(0, 500);
				}

				userSession.objectGuid = Guid.NewGuid();
				_context.UserSessions.Add(userSession);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Security.UserSession entity successfully created.",
					true,
					userSession.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.UserSession.CreateAnonymousWithFirstLevelSubObjects(userSession)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Security.UserSession entity creation failed.", false, userSession.id.ToString(), "", JsonSerializer.Serialize(userSession), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserSession", userSession.id, userSession.tokenId));

			return CreatedAtRoute("UserSession", new { id = userSession.id }, Database.UserSession.CreateAnonymousWithFirstLevelSubObjects(userSession));
		}



        /// <summary>
        /// 
        /// This deletes a UserSession record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserSession/{id}")]
		[Route("api/UserSession")]
		public async Task<IActionResult> DeleteUserSession(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.UserSession> query = (from x in _context.UserSessions
				where
				(x.id == id)
				select x);


			Database.UserSession userSession = await query.FirstOrDefaultAsync(cancellationToken);

			if (userSession == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.UserSession DELETE", id.ToString(), new Exception("No Security.UserSession entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.UserSession cloneOfExisting = (Database.UserSession)_context.Entry(userSession).GetDatabaseValues().ToObject();


			try
			{
				userSession.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.UserSession entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserSession.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserSession.CreateAnonymousWithFirstLevelSubObjects(userSession)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.UserSession entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserSession.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserSession.CreateAnonymousWithFirstLevelSubObjects(userSession)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of UserSession records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/UserSessions/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? securityUserId = null,
			Guid? objectGuid = null,
			string tokenId = null,
			DateTime? sessionStart = null,
			DateTime? expiresAt = null,
			string ipAddress = null,
			string userAgent = null,
			string loginMethod = null,
			string clientApplication = null,
			bool? isRevoked = null,
			DateTime? revokedAt = null,
			string revokedBy = null,
			string revokedReason = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (sessionStart.HasValue == true && sessionStart.Value.Kind != DateTimeKind.Utc)
			{
				sessionStart = sessionStart.Value.ToUniversalTime();
			}

			if (expiresAt.HasValue == true && expiresAt.Value.Kind != DateTimeKind.Utc)
			{
				expiresAt = expiresAt.Value.ToUniversalTime();
			}

			if (revokedAt.HasValue == true && revokedAt.Value.Kind != DateTimeKind.Utc)
			{
				revokedAt = revokedAt.Value.ToUniversalTime();
			}

			IQueryable<Database.UserSession> query = (from us in _context.UserSessions select us);
			if (securityUserId.HasValue == true)
			{
				query = query.Where(us => us.securityUserId == securityUserId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(us => us.objectGuid == objectGuid);
			}
			if (string.IsNullOrEmpty(tokenId) == false)
			{
				query = query.Where(us => us.tokenId == tokenId);
			}
			if (sessionStart.HasValue == true)
			{
				query = query.Where(us => us.sessionStart == sessionStart.Value);
			}
			if (expiresAt.HasValue == true)
			{
				query = query.Where(us => us.expiresAt == expiresAt.Value);
			}
			if (string.IsNullOrEmpty(ipAddress) == false)
			{
				query = query.Where(us => us.ipAddress == ipAddress);
			}
			if (string.IsNullOrEmpty(userAgent) == false)
			{
				query = query.Where(us => us.userAgent == userAgent);
			}
			if (string.IsNullOrEmpty(loginMethod) == false)
			{
				query = query.Where(us => us.loginMethod == loginMethod);
			}
			if (string.IsNullOrEmpty(clientApplication) == false)
			{
				query = query.Where(us => us.clientApplication == clientApplication);
			}
			if (isRevoked.HasValue == true)
			{
				query = query.Where(us => us.isRevoked == isRevoked.Value);
			}
			if (revokedAt.HasValue == true)
			{
				query = query.Where(us => us.revokedAt == revokedAt.Value);
			}
			if (string.IsNullOrEmpty(revokedBy) == false)
			{
				query = query.Where(us => us.revokedBy == revokedBy);
			}
			if (string.IsNullOrEmpty(revokedReason) == false)
			{
				query = query.Where(us => us.revokedReason == revokedReason);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(us => us.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(us => us.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(us => us.deleted == false);
				}
			}
			else
			{
				query = query.Where(us => us.active == true);
				query = query.Where(us => us.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Session, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.tokenId.Contains(anyStringContains)
			       || x.ipAddress.Contains(anyStringContains)
			       || x.userAgent.Contains(anyStringContains)
			       || x.loginMethod.Contains(anyStringContains)
			       || x.clientApplication.Contains(anyStringContains)
			       || x.revokedBy.Contains(anyStringContains)
			       || x.revokedReason.Contains(anyStringContains)
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


			query = query.OrderByDescending (x => x.sessionStart);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.UserSession.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/UserSession/CreateAuditEvent")]
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
