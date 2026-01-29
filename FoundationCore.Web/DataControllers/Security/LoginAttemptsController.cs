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
    /// This auto generated class provides the basic CRUD operations for the LoginAttempt entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the LoginAttempt entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class LoginAttemptsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private SecurityContext _context;

		private ILogger<LoginAttemptsController> _logger;

		public LoginAttemptsController(SecurityContext context, ILogger<LoginAttemptsController> logger) : base("Security", "LoginAttempt")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of LoginAttempts filtered by the parameters provided.
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
		[Route("api/LoginAttempts")]
		public async Task<IActionResult> GetLoginAttempts(
			DateTime? timeStamp = null,
			string userName = null,
			int? passwordHash = null,
			string resource = null,
			string sessionId = null,
			string ipAddress = null,
			string userAgent = null,
			string value = null,
			bool? success = null,
			int? securityUserId = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
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
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.LoginAttempt> query = (from la in _context.LoginAttempts select la);
			if (timeStamp.HasValue == true)
			{
				query = query.Where(la => la.timeStamp == timeStamp.Value);
			}
			if (string.IsNullOrEmpty(userName) == false)
			{
				query = query.Where(la => la.userName == userName);
			}
			if (passwordHash.HasValue == true)
			{
				query = query.Where(la => la.passwordHash == passwordHash.Value);
			}
			if (string.IsNullOrEmpty(resource) == false)
			{
				query = query.Where(la => la.resource == resource);
			}
			if (string.IsNullOrEmpty(sessionId) == false)
			{
				query = query.Where(la => la.sessionId == sessionId);
			}
			if (string.IsNullOrEmpty(ipAddress) == false)
			{
				query = query.Where(la => la.ipAddress == ipAddress);
			}
			if (string.IsNullOrEmpty(userAgent) == false)
			{
				query = query.Where(la => la.userAgent == userAgent);
			}
			if (string.IsNullOrEmpty(value) == false)
			{
				query = query.Where(la => la.value == value);
			}
			if (success.HasValue == true)
			{
				query = query.Where(la => la.success == success.Value);
			}
			if (securityUserId.HasValue == true)
			{
				query = query.Where(la => la.securityUserId == securityUserId.Value);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(la => la.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(la => la.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(la => la.deleted == false);
				}
			}
			else
			{
				query = query.Where(la => la.active == true);
				query = query.Where(la => la.deleted == false);
			}

			query = query.OrderByDescending(la => la.timeStamp);

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
			// Add the any string contains parameter to span all the string fields on the Login Attempt, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.userName.Contains(anyStringContains)
			       || x.resource.Contains(anyStringContains)
			       || x.sessionId.Contains(anyStringContains)
			       || x.ipAddress.Contains(anyStringContains)
			       || x.userAgent.Contains(anyStringContains)
			       || x.value.Contains(anyStringContains)
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
			
			List<Database.LoginAttempt> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.LoginAttempt loginAttempt in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(loginAttempt, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Security.LoginAttempt Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Security.LoginAttempt Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of LoginAttempts filtered by the parameters provided.  Its query is similar to the GetLoginAttempts method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LoginAttempts/RowCount")]
		public async Task<IActionResult> GetRowCount(
			DateTime? timeStamp = null,
			string userName = null,
			int? passwordHash = null,
			string resource = null,
			string sessionId = null,
			string ipAddress = null,
			string userAgent = null,
			string value = null,
			bool? success = null,
			int? securityUserId = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.LoginAttempt> query = (from la in _context.LoginAttempts select la);
			if (timeStamp.HasValue == true)
			{
				query = query.Where(la => la.timeStamp == timeStamp.Value);
			}
			if (userName != null)
			{
				query = query.Where(la => la.userName == userName);
			}
			if (passwordHash.HasValue == true)
			{
				query = query.Where(la => la.passwordHash == passwordHash.Value);
			}
			if (resource != null)
			{
				query = query.Where(la => la.resource == resource);
			}
			if (sessionId != null)
			{
				query = query.Where(la => la.sessionId == sessionId);
			}
			if (ipAddress != null)
			{
				query = query.Where(la => la.ipAddress == ipAddress);
			}
			if (userAgent != null)
			{
				query = query.Where(la => la.userAgent == userAgent);
			}
			if (value != null)
			{
				query = query.Where(la => la.value == value);
			}
			if (success.HasValue == true)
			{
				query = query.Where(la => la.success == success.Value);
			}
			if (securityUserId.HasValue == true)
			{
				query = query.Where(la => la.securityUserId == securityUserId.Value);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(la => la.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(la => la.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(la => la.deleted == false);
				}
			}
			else
			{
				query = query.Where(la => la.active == true);
				query = query.Where(la => la.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Login Attempt, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.userName.Contains(anyStringContains)
			       || x.resource.Contains(anyStringContains)
			       || x.sessionId.Contains(anyStringContains)
			       || x.ipAddress.Contains(anyStringContains)
			       || x.userAgent.Contains(anyStringContains)
			       || x.value.Contains(anyStringContains)
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
        /// This gets a single LoginAttempt by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LoginAttempt/{id}")]
		public async Task<IActionResult> GetLoginAttempt(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.LoginAttempt> query = (from la in _context.LoginAttempts where
							(la.id == id) &&
							(userIsAdmin == true || la.deleted == false) &&
							(userIsWriter == true || la.active == true)
					select la);

				if (includeRelations == true)
				{
					query = query.Include(x => x.securityUser);
					query = query.AsSplitQuery();
				}

				Database.LoginAttempt materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Security.LoginAttempt Entity was read with Admin privilege." : "Security.LoginAttempt Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "LoginAttempt", materialized.id, materialized.userName));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Security.LoginAttempt entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Security.LoginAttempt.   Entity was read with Admin privilege." : "Exception caught during entity read of Security.LoginAttempt.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing LoginAttempt record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/LoginAttempt/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutLoginAttempt(int id, [FromBody]Database.LoginAttempt.LoginAttemptDTO loginAttemptDTO, CancellationToken cancellationToken = default)
		{
			if (loginAttemptDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != loginAttemptDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.LoginAttempt> query = (from x in _context.LoginAttempts
				where
				(x.id == id)
				select x);


			Database.LoginAttempt existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.LoginAttempt PUT", id.ToString(), new Exception("No Security.LoginAttempt entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.LoginAttempt cloneOfExisting = (Database.LoginAttempt)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new LoginAttempt object using the data from the existing record, updated with what is in the DTO.
			//
			Database.LoginAttempt loginAttempt = (Database.LoginAttempt)_context.Entry(existing).GetDatabaseValues().ToObject();
			loginAttempt.ApplyDTO(loginAttemptDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (loginAttempt.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Security.LoginAttempt record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (loginAttempt.timeStamp.Kind != DateTimeKind.Utc)
			{
				loginAttempt.timeStamp = loginAttempt.timeStamp.ToUniversalTime();
			}

			if (loginAttempt.userName != null && loginAttempt.userName.Length > 250)
			{
				loginAttempt.userName = loginAttempt.userName.Substring(0, 250);
			}

			if (loginAttempt.resource != null && loginAttempt.resource.Length > 500)
			{
				loginAttempt.resource = loginAttempt.resource.Substring(0, 500);
			}

			if (loginAttempt.sessionId != null && loginAttempt.sessionId.Length > 50)
			{
				loginAttempt.sessionId = loginAttempt.sessionId.Substring(0, 50);
			}

			if (loginAttempt.ipAddress != null && loginAttempt.ipAddress.Length > 50)
			{
				loginAttempt.ipAddress = loginAttempt.ipAddress.Substring(0, 50);
			}

			if (loginAttempt.userAgent != null && loginAttempt.userAgent.Length > 250)
			{
				loginAttempt.userAgent = loginAttempt.userAgent.Substring(0, 250);
			}

			EntityEntry<Database.LoginAttempt> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(loginAttempt);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Security.LoginAttempt entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.LoginAttempt.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LoginAttempt.CreateAnonymousWithFirstLevelSubObjects(loginAttempt)),
					null);


				return Ok(Database.LoginAttempt.CreateAnonymous(loginAttempt));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Security.LoginAttempt entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.LoginAttempt.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LoginAttempt.CreateAnonymousWithFirstLevelSubObjects(loginAttempt)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new LoginAttempt record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LoginAttempt", Name = "LoginAttempt")]
		public async Task<IActionResult> PostLoginAttempt([FromBody]Database.LoginAttempt.LoginAttemptDTO loginAttemptDTO, CancellationToken cancellationToken = default)
		{
			if (loginAttemptDTO == null)
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
			// Create a new LoginAttempt object using the data from the DTO
			//
			Database.LoginAttempt loginAttempt = Database.LoginAttempt.FromDTO(loginAttemptDTO);

			try
			{
				if (loginAttempt.timeStamp.Kind != DateTimeKind.Utc)
				{
					loginAttempt.timeStamp = loginAttempt.timeStamp.ToUniversalTime();
				}

				if (loginAttempt.userName != null && loginAttempt.userName.Length > 250)
				{
					loginAttempt.userName = loginAttempt.userName.Substring(0, 250);
				}

				if (loginAttempt.resource != null && loginAttempt.resource.Length > 500)
				{
					loginAttempt.resource = loginAttempt.resource.Substring(0, 500);
				}

				if (loginAttempt.sessionId != null && loginAttempt.sessionId.Length > 50)
				{
					loginAttempt.sessionId = loginAttempt.sessionId.Substring(0, 50);
				}

				if (loginAttempt.ipAddress != null && loginAttempt.ipAddress.Length > 50)
				{
					loginAttempt.ipAddress = loginAttempt.ipAddress.Substring(0, 50);
				}

				if (loginAttempt.userAgent != null && loginAttempt.userAgent.Length > 250)
				{
					loginAttempt.userAgent = loginAttempt.userAgent.Substring(0, 250);
				}

				_context.LoginAttempts.Add(loginAttempt);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Security.LoginAttempt entity successfully created.",
					true,
					loginAttempt.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.LoginAttempt.CreateAnonymousWithFirstLevelSubObjects(loginAttempt)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Security.LoginAttempt entity creation failed.", false, loginAttempt.id.ToString(), "", JsonSerializer.Serialize(loginAttempt), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "LoginAttempt", loginAttempt.id, loginAttempt.userName));

			return CreatedAtRoute("LoginAttempt", new { id = loginAttempt.id }, Database.LoginAttempt.CreateAnonymousWithFirstLevelSubObjects(loginAttempt));
		}



        /// <summary>
        /// 
        /// This deletes a LoginAttempt record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LoginAttempt/{id}")]
		[Route("api/LoginAttempt")]
		public async Task<IActionResult> DeleteLoginAttempt(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.LoginAttempt> query = (from x in _context.LoginAttempts
				where
				(x.id == id)
				select x);


			Database.LoginAttempt loginAttempt = await query.FirstOrDefaultAsync(cancellationToken);

			if (loginAttempt == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.LoginAttempt DELETE", id.ToString(), new Exception("No Security.LoginAttempt entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.LoginAttempt cloneOfExisting = (Database.LoginAttempt)_context.Entry(loginAttempt).GetDatabaseValues().ToObject();


			try
			{
				loginAttempt.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.LoginAttempt entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.LoginAttempt.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LoginAttempt.CreateAnonymousWithFirstLevelSubObjects(loginAttempt)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.LoginAttempt entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.LoginAttempt.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LoginAttempt.CreateAnonymousWithFirstLevelSubObjects(loginAttempt)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of LoginAttempt records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/LoginAttempts/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			DateTime? timeStamp = null,
			string userName = null,
			int? passwordHash = null,
			string resource = null,
			string sessionId = null,
			string ipAddress = null,
			string userAgent = null,
			string value = null,
			bool? success = null,
			int? securityUserId = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);

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
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.LoginAttempt> query = (from la in _context.LoginAttempts select la);
			if (timeStamp.HasValue == true)
			{
				query = query.Where(la => la.timeStamp == timeStamp.Value);
			}
			if (string.IsNullOrEmpty(userName) == false)
			{
				query = query.Where(la => la.userName == userName);
			}
			if (passwordHash.HasValue == true)
			{
				query = query.Where(la => la.passwordHash == passwordHash.Value);
			}
			if (string.IsNullOrEmpty(resource) == false)
			{
				query = query.Where(la => la.resource == resource);
			}
			if (string.IsNullOrEmpty(sessionId) == false)
			{
				query = query.Where(la => la.sessionId == sessionId);
			}
			if (string.IsNullOrEmpty(ipAddress) == false)
			{
				query = query.Where(la => la.ipAddress == ipAddress);
			}
			if (string.IsNullOrEmpty(userAgent) == false)
			{
				query = query.Where(la => la.userAgent == userAgent);
			}
			if (string.IsNullOrEmpty(value) == false)
			{
				query = query.Where(la => la.value == value);
			}
			if (success.HasValue == true)
			{
				query = query.Where(la => la.success == success.Value);
			}
			if (securityUserId.HasValue == true)
			{
				query = query.Where(la => la.securityUserId == securityUserId.Value);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(la => la.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(la => la.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(la => la.deleted == false);
				}
			}
			else
			{
				query = query.Where(la => la.active == true);
				query = query.Where(la => la.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Login Attempt, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.userName.Contains(anyStringContains)
			       || x.resource.Contains(anyStringContains)
			       || x.sessionId.Contains(anyStringContains)
			       || x.ipAddress.Contains(anyStringContains)
			       || x.userAgent.Contains(anyStringContains)
			       || x.value.Contains(anyStringContains)
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


			query = query.OrderByDescending (x => x.timeStamp);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.LoginAttempt.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/LoginAttempt/CreateAuditEvent")]
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
