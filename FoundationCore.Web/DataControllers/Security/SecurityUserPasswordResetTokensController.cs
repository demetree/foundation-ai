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
    /// This auto generated class provides the basic CRUD operations for the SecurityUserPasswordResetToken entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SecurityUserPasswordResetToken entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SecurityUserPasswordResetTokensController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private SecurityContext _context;

		private ILogger<SecurityUserPasswordResetTokensController> _logger;

		public SecurityUserPasswordResetTokensController(SecurityContext context, ILogger<SecurityUserPasswordResetTokensController> logger) : base("Security", "SecurityUserPasswordResetToken")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of SecurityUserPasswordResetTokens filtered by the parameters provided.
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
		[Route("api/SecurityUserPasswordResetTokens")]
		public async Task<IActionResult> GetSecurityUserPasswordResetTokens(
			int? securityUserId = null,
			string token = null,
			DateTime? timeStamp = null,
			DateTime? expiry = null,
			bool? systemInitiated = null,
			bool? completed = null,
			string comments = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			if (expiry.HasValue == true && expiry.Value.Kind != DateTimeKind.Utc)
			{
				expiry = expiry.Value.ToUniversalTime();
			}

			IQueryable<Database.SecurityUserPasswordResetToken> query = (from suprt in _context.SecurityUserPasswordResetTokens select suprt);
			if (securityUserId.HasValue == true)
			{
				query = query.Where(suprt => suprt.securityUserId == securityUserId.Value);
			}
			if (string.IsNullOrEmpty(token) == false)
			{
				query = query.Where(suprt => suprt.token == token);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(suprt => suprt.timeStamp == timeStamp.Value);
			}
			if (expiry.HasValue == true)
			{
				query = query.Where(suprt => suprt.expiry == expiry.Value);
			}
			if (systemInitiated.HasValue == true)
			{
				query = query.Where(suprt => suprt.systemInitiated == systemInitiated.Value);
			}
			if (completed.HasValue == true)
			{
				query = query.Where(suprt => suprt.completed == completed.Value);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(suprt => suprt.comments == comments);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(suprt => suprt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(suprt => suprt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(suprt => suprt.deleted == false);
				}
			}
			else
			{
				query = query.Where(suprt => suprt.active == true);
				query = query.Where(suprt => suprt.deleted == false);
			}

			query = query.OrderByDescending(suprt => suprt.timeStamp);

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
			// Add the any string contains parameter to span all the string fields on the Security User Password Reset Token, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.token.Contains(anyStringContains)
			       || x.comments.Contains(anyStringContains)
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
			
			List<Database.SecurityUserPasswordResetToken> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SecurityUserPasswordResetToken securityUserPasswordResetToken in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(securityUserPasswordResetToken, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Security.SecurityUserPasswordResetToken Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Security.SecurityUserPasswordResetToken Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of SecurityUserPasswordResetTokens filtered by the parameters provided.  Its query is similar to the GetSecurityUserPasswordResetTokens method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityUserPasswordResetTokens/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? securityUserId = null,
			string token = null,
			DateTime? timeStamp = null,
			DateTime? expiry = null,
			bool? systemInitiated = null,
			bool? completed = null,
			string comments = null,
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

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			if (expiry.HasValue == true && expiry.Value.Kind != DateTimeKind.Utc)
			{
				expiry = expiry.Value.ToUniversalTime();
			}

			IQueryable<Database.SecurityUserPasswordResetToken> query = (from suprt in _context.SecurityUserPasswordResetTokens select suprt);
			if (securityUserId.HasValue == true)
			{
				query = query.Where(suprt => suprt.securityUserId == securityUserId.Value);
			}
			if (token != null)
			{
				query = query.Where(suprt => suprt.token == token);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(suprt => suprt.timeStamp == timeStamp.Value);
			}
			if (expiry.HasValue == true)
			{
				query = query.Where(suprt => suprt.expiry == expiry.Value);
			}
			if (systemInitiated.HasValue == true)
			{
				query = query.Where(suprt => suprt.systemInitiated == systemInitiated.Value);
			}
			if (completed.HasValue == true)
			{
				query = query.Where(suprt => suprt.completed == completed.Value);
			}
			if (comments != null)
			{
				query = query.Where(suprt => suprt.comments == comments);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(suprt => suprt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(suprt => suprt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(suprt => suprt.deleted == false);
				}
			}
			else
			{
				query = query.Where(suprt => suprt.active == true);
				query = query.Where(suprt => suprt.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Security User Password Reset Token, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.token.Contains(anyStringContains)
			       || x.comments.Contains(anyStringContains)
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
        /// This gets a single SecurityUserPasswordResetToken by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityUserPasswordResetToken/{id}")]
		public async Task<IActionResult> GetSecurityUserPasswordResetToken(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.SecurityUserPasswordResetToken> query = (from suprt in _context.SecurityUserPasswordResetTokens where
							(suprt.id == id) &&
							(userIsAdmin == true || suprt.deleted == false) &&
							(userIsWriter == true || suprt.active == true)
					select suprt);

				if (includeRelations == true)
				{
					query = query.Include(x => x.securityUser);
					query = query.AsSplitQuery();
				}

				Database.SecurityUserPasswordResetToken materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Security.SecurityUserPasswordResetToken Entity was read with Admin privilege." : "Security.SecurityUserPasswordResetToken Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityUserPasswordResetToken", materialized.id, materialized.token));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Security.SecurityUserPasswordResetToken entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Security.SecurityUserPasswordResetToken.   Entity was read with Admin privilege." : "Exception caught during entity read of Security.SecurityUserPasswordResetToken.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing SecurityUserPasswordResetToken record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SecurityUserPasswordResetToken/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSecurityUserPasswordResetToken(int id, [FromBody]Database.SecurityUserPasswordResetToken.SecurityUserPasswordResetTokenDTO securityUserPasswordResetTokenDTO, CancellationToken cancellationToken = default)
		{
			if (securityUserPasswordResetTokenDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != securityUserPasswordResetTokenDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.SecurityUserPasswordResetToken> query = (from x in _context.SecurityUserPasswordResetTokens
				where
				(x.id == id)
				select x);


			Database.SecurityUserPasswordResetToken existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityUserPasswordResetToken PUT", id.ToString(), new Exception("No Security.SecurityUserPasswordResetToken entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SecurityUserPasswordResetToken cloneOfExisting = (Database.SecurityUserPasswordResetToken)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SecurityUserPasswordResetToken object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SecurityUserPasswordResetToken securityUserPasswordResetToken = (Database.SecurityUserPasswordResetToken)_context.Entry(existing).GetDatabaseValues().ToObject();
			securityUserPasswordResetToken.ApplyDTO(securityUserPasswordResetTokenDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (securityUserPasswordResetToken.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Security.SecurityUserPasswordResetToken record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (securityUserPasswordResetToken.token != null && securityUserPasswordResetToken.token.Length > 250)
			{
				securityUserPasswordResetToken.token = securityUserPasswordResetToken.token.Substring(0, 250);
			}

			if (securityUserPasswordResetToken.timeStamp.Kind != DateTimeKind.Utc)
			{
				securityUserPasswordResetToken.timeStamp = securityUserPasswordResetToken.timeStamp.ToUniversalTime();
			}

			if (securityUserPasswordResetToken.expiry.Kind != DateTimeKind.Utc)
			{
				securityUserPasswordResetToken.expiry = securityUserPasswordResetToken.expiry.ToUniversalTime();
			}

			if (securityUserPasswordResetToken.comments != null && securityUserPasswordResetToken.comments.Length > 1000)
			{
				securityUserPasswordResetToken.comments = securityUserPasswordResetToken.comments.Substring(0, 1000);
			}

			EntityEntry<Database.SecurityUserPasswordResetToken> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(securityUserPasswordResetToken);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityUserPasswordResetToken entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityUserPasswordResetToken.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityUserPasswordResetToken.CreateAnonymousWithFirstLevelSubObjects(securityUserPasswordResetToken)),
					null);


				return Ok(Database.SecurityUserPasswordResetToken.CreateAnonymous(securityUserPasswordResetToken));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityUserPasswordResetToken entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityUserPasswordResetToken.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityUserPasswordResetToken.CreateAnonymousWithFirstLevelSubObjects(securityUserPasswordResetToken)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new SecurityUserPasswordResetToken record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityUserPasswordResetToken", Name = "SecurityUserPasswordResetToken")]
		public async Task<IActionResult> PostSecurityUserPasswordResetToken([FromBody]Database.SecurityUserPasswordResetToken.SecurityUserPasswordResetTokenDTO securityUserPasswordResetTokenDTO, CancellationToken cancellationToken = default)
		{
			if (securityUserPasswordResetTokenDTO == null)
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
			// Create a new SecurityUserPasswordResetToken object using the data from the DTO
			//
			Database.SecurityUserPasswordResetToken securityUserPasswordResetToken = Database.SecurityUserPasswordResetToken.FromDTO(securityUserPasswordResetTokenDTO);

			try
			{
				if (securityUserPasswordResetToken.token != null && securityUserPasswordResetToken.token.Length > 250)
				{
					securityUserPasswordResetToken.token = securityUserPasswordResetToken.token.Substring(0, 250);
				}

				if (securityUserPasswordResetToken.timeStamp.Kind != DateTimeKind.Utc)
				{
					securityUserPasswordResetToken.timeStamp = securityUserPasswordResetToken.timeStamp.ToUniversalTime();
				}

				if (securityUserPasswordResetToken.expiry.Kind != DateTimeKind.Utc)
				{
					securityUserPasswordResetToken.expiry = securityUserPasswordResetToken.expiry.ToUniversalTime();
				}

				if (securityUserPasswordResetToken.comments != null && securityUserPasswordResetToken.comments.Length > 1000)
				{
					securityUserPasswordResetToken.comments = securityUserPasswordResetToken.comments.Substring(0, 1000);
				}

				_context.SecurityUserPasswordResetTokens.Add(securityUserPasswordResetToken);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Security.SecurityUserPasswordResetToken entity successfully created.",
					true,
					securityUserPasswordResetToken.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.SecurityUserPasswordResetToken.CreateAnonymousWithFirstLevelSubObjects(securityUserPasswordResetToken)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Security.SecurityUserPasswordResetToken entity creation failed.", false, securityUserPasswordResetToken.id.ToString(), "", JsonSerializer.Serialize(securityUserPasswordResetToken), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityUserPasswordResetToken", securityUserPasswordResetToken.id, securityUserPasswordResetToken.token));

			return CreatedAtRoute("SecurityUserPasswordResetToken", new { id = securityUserPasswordResetToken.id }, Database.SecurityUserPasswordResetToken.CreateAnonymousWithFirstLevelSubObjects(securityUserPasswordResetToken));
		}



        /// <summary>
        /// 
        /// This deletes a SecurityUserPasswordResetToken record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityUserPasswordResetToken/{id}")]
		[Route("api/SecurityUserPasswordResetToken")]
		public async Task<IActionResult> DeleteSecurityUserPasswordResetToken(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.SecurityUserPasswordResetToken> query = (from x in _context.SecurityUserPasswordResetTokens
				where
				(x.id == id)
				select x);


			Database.SecurityUserPasswordResetToken securityUserPasswordResetToken = await query.FirstOrDefaultAsync(cancellationToken);

			if (securityUserPasswordResetToken == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityUserPasswordResetToken DELETE", id.ToString(), new Exception("No Security.SecurityUserPasswordResetToken entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SecurityUserPasswordResetToken cloneOfExisting = (Database.SecurityUserPasswordResetToken)_context.Entry(securityUserPasswordResetToken).GetDatabaseValues().ToObject();


			try
			{
				securityUserPasswordResetToken.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityUserPasswordResetToken entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityUserPasswordResetToken.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityUserPasswordResetToken.CreateAnonymousWithFirstLevelSubObjects(securityUserPasswordResetToken)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityUserPasswordResetToken entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityUserPasswordResetToken.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityUserPasswordResetToken.CreateAnonymousWithFirstLevelSubObjects(securityUserPasswordResetToken)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of SecurityUserPasswordResetToken records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SecurityUserPasswordResetTokens/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? securityUserId = null,
			string token = null,
			DateTime? timeStamp = null,
			DateTime? expiry = null,
			bool? systemInitiated = null,
			bool? completed = null,
			string comments = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			if (expiry.HasValue == true && expiry.Value.Kind != DateTimeKind.Utc)
			{
				expiry = expiry.Value.ToUniversalTime();
			}

			IQueryable<Database.SecurityUserPasswordResetToken> query = (from suprt in _context.SecurityUserPasswordResetTokens select suprt);
			if (securityUserId.HasValue == true)
			{
				query = query.Where(suprt => suprt.securityUserId == securityUserId.Value);
			}
			if (string.IsNullOrEmpty(token) == false)
			{
				query = query.Where(suprt => suprt.token == token);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(suprt => suprt.timeStamp == timeStamp.Value);
			}
			if (expiry.HasValue == true)
			{
				query = query.Where(suprt => suprt.expiry == expiry.Value);
			}
			if (systemInitiated.HasValue == true)
			{
				query = query.Where(suprt => suprt.systemInitiated == systemInitiated.Value);
			}
			if (completed.HasValue == true)
			{
				query = query.Where(suprt => suprt.completed == completed.Value);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(suprt => suprt.comments == comments);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(suprt => suprt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(suprt => suprt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(suprt => suprt.deleted == false);
				}
			}
			else
			{
				query = query.Where(suprt => suprt.active == true);
				query = query.Where(suprt => suprt.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Security User Password Reset Token, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.token.Contains(anyStringContains)
			       || x.comments.Contains(anyStringContains)
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
			return Ok(await (from queryData in query select Database.SecurityUserPasswordResetToken.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/SecurityUserPasswordResetToken/CreateAuditEvent")]
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
