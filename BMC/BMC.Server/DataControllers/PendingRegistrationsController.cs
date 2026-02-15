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
using Foundation.Security.Database;
using static Foundation.Auditor.AuditEngine;
using Foundation.BMC.Database;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the PendingRegistration entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the PendingRegistration entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class PendingRegistrationsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 100;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<PendingRegistrationsController> _logger;

		public PendingRegistrationsController(BMCContext context, ILogger<PendingRegistrationsController> logger) : base("BMC", "PendingRegistration")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of PendingRegistrations filtered by the parameters provided.
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
		[Route("api/PendingRegistrations")]
		public async Task<IActionResult> GetPendingRegistrations(
			string accountName = null,
			string emailAddress = null,
			string displayName = null,
			string passwordHash = null,
			string verificationCode = null,
			DateTime? codeExpiresAt = null,
			int? verificationAttempts = null,
			string status = null,
			DateTime? createdAt = null,
			DateTime? verifiedAt = null,
			DateTime? provisionedAt = null,
			string ipAddress = null,
			string userAgent = null,
			string verificationChannel = null,
			string failureReason = null,
			int? provisionedSecurityUserId = null,
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
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
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
			if (codeExpiresAt.HasValue == true && codeExpiresAt.Value.Kind != DateTimeKind.Utc)
			{
				codeExpiresAt = codeExpiresAt.Value.ToUniversalTime();
			}

			if (createdAt.HasValue == true && createdAt.Value.Kind != DateTimeKind.Utc)
			{
				createdAt = createdAt.Value.ToUniversalTime();
			}

			if (verifiedAt.HasValue == true && verifiedAt.Value.Kind != DateTimeKind.Utc)
			{
				verifiedAt = verifiedAt.Value.ToUniversalTime();
			}

			if (provisionedAt.HasValue == true && provisionedAt.Value.Kind != DateTimeKind.Utc)
			{
				provisionedAt = provisionedAt.Value.ToUniversalTime();
			}

			IQueryable<Database.PendingRegistration> query = (from pr in _context.PendingRegistrations select pr);
			if (string.IsNullOrEmpty(accountName) == false)
			{
				query = query.Where(pr => pr.accountName == accountName);
			}
			if (string.IsNullOrEmpty(emailAddress) == false)
			{
				query = query.Where(pr => pr.emailAddress == emailAddress);
			}
			if (string.IsNullOrEmpty(displayName) == false)
			{
				query = query.Where(pr => pr.displayName == displayName);
			}
			if (string.IsNullOrEmpty(passwordHash) == false)
			{
				query = query.Where(pr => pr.passwordHash == passwordHash);
			}
			if (string.IsNullOrEmpty(verificationCode) == false)
			{
				query = query.Where(pr => pr.verificationCode == verificationCode);
			}
			if (codeExpiresAt.HasValue == true)
			{
				query = query.Where(pr => pr.codeExpiresAt == codeExpiresAt.Value);
			}
			if (verificationAttempts.HasValue == true)
			{
				query = query.Where(pr => pr.verificationAttempts == verificationAttempts.Value);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(pr => pr.status == status);
			}
			if (createdAt.HasValue == true)
			{
				query = query.Where(pr => pr.createdAt == createdAt.Value);
			}
			if (verifiedAt.HasValue == true)
			{
				query = query.Where(pr => pr.verifiedAt == verifiedAt.Value);
			}
			if (provisionedAt.HasValue == true)
			{
				query = query.Where(pr => pr.provisionedAt == provisionedAt.Value);
			}
			if (string.IsNullOrEmpty(ipAddress) == false)
			{
				query = query.Where(pr => pr.ipAddress == ipAddress);
			}
			if (string.IsNullOrEmpty(userAgent) == false)
			{
				query = query.Where(pr => pr.userAgent == userAgent);
			}
			if (string.IsNullOrEmpty(verificationChannel) == false)
			{
				query = query.Where(pr => pr.verificationChannel == verificationChannel);
			}
			if (string.IsNullOrEmpty(failureReason) == false)
			{
				query = query.Where(pr => pr.failureReason == failureReason);
			}
			if (provisionedSecurityUserId.HasValue == true)
			{
				query = query.Where(pr => pr.provisionedSecurityUserId == provisionedSecurityUserId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pr => pr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pr => pr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pr => pr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pr => pr.deleted == false);
				}
			}
			else
			{
				query = query.Where(pr => pr.active == true);
				query = query.Where(pr => pr.deleted == false);
			}

			query = query.OrderBy(pr => pr.accountName).ThenBy(pr => pr.emailAddress).ThenBy(pr => pr.displayName);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Pending Registration, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.accountName.Contains(anyStringContains)
			       || x.emailAddress.Contains(anyStringContains)
			       || x.displayName.Contains(anyStringContains)
			       || x.passwordHash.Contains(anyStringContains)
			       || x.verificationCode.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || x.ipAddress.Contains(anyStringContains)
			       || x.userAgent.Contains(anyStringContains)
			       || x.verificationChannel.Contains(anyStringContains)
			       || x.failureReason.Contains(anyStringContains)
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.PendingRegistration> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.PendingRegistration pendingRegistration in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(pendingRegistration, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.PendingRegistration Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.PendingRegistration Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of PendingRegistrations filtered by the parameters provided.  Its query is similar to the GetPendingRegistrations method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PendingRegistrations/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string accountName = null,
			string emailAddress = null,
			string displayName = null,
			string passwordHash = null,
			string verificationCode = null,
			DateTime? codeExpiresAt = null,
			int? verificationAttempts = null,
			string status = null,
			DateTime? createdAt = null,
			DateTime? verifiedAt = null,
			DateTime? provisionedAt = null,
			string ipAddress = null,
			string userAgent = null,
			string verificationChannel = null,
			string failureReason = null,
			int? provisionedSecurityUserId = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
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
			if (codeExpiresAt.HasValue == true && codeExpiresAt.Value.Kind != DateTimeKind.Utc)
			{
				codeExpiresAt = codeExpiresAt.Value.ToUniversalTime();
			}

			if (createdAt.HasValue == true && createdAt.Value.Kind != DateTimeKind.Utc)
			{
				createdAt = createdAt.Value.ToUniversalTime();
			}

			if (verifiedAt.HasValue == true && verifiedAt.Value.Kind != DateTimeKind.Utc)
			{
				verifiedAt = verifiedAt.Value.ToUniversalTime();
			}

			if (provisionedAt.HasValue == true && provisionedAt.Value.Kind != DateTimeKind.Utc)
			{
				provisionedAt = provisionedAt.Value.ToUniversalTime();
			}

			IQueryable<Database.PendingRegistration> query = (from pr in _context.PendingRegistrations select pr);
			if (accountName != null)
			{
				query = query.Where(pr => pr.accountName == accountName);
			}
			if (emailAddress != null)
			{
				query = query.Where(pr => pr.emailAddress == emailAddress);
			}
			if (displayName != null)
			{
				query = query.Where(pr => pr.displayName == displayName);
			}
			if (passwordHash != null)
			{
				query = query.Where(pr => pr.passwordHash == passwordHash);
			}
			if (verificationCode != null)
			{
				query = query.Where(pr => pr.verificationCode == verificationCode);
			}
			if (codeExpiresAt.HasValue == true)
			{
				query = query.Where(pr => pr.codeExpiresAt == codeExpiresAt.Value);
			}
			if (verificationAttempts.HasValue == true)
			{
				query = query.Where(pr => pr.verificationAttempts == verificationAttempts.Value);
			}
			if (status != null)
			{
				query = query.Where(pr => pr.status == status);
			}
			if (createdAt.HasValue == true)
			{
				query = query.Where(pr => pr.createdAt == createdAt.Value);
			}
			if (verifiedAt.HasValue == true)
			{
				query = query.Where(pr => pr.verifiedAt == verifiedAt.Value);
			}
			if (provisionedAt.HasValue == true)
			{
				query = query.Where(pr => pr.provisionedAt == provisionedAt.Value);
			}
			if (ipAddress != null)
			{
				query = query.Where(pr => pr.ipAddress == ipAddress);
			}
			if (userAgent != null)
			{
				query = query.Where(pr => pr.userAgent == userAgent);
			}
			if (verificationChannel != null)
			{
				query = query.Where(pr => pr.verificationChannel == verificationChannel);
			}
			if (failureReason != null)
			{
				query = query.Where(pr => pr.failureReason == failureReason);
			}
			if (provisionedSecurityUserId.HasValue == true)
			{
				query = query.Where(pr => pr.provisionedSecurityUserId == provisionedSecurityUserId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pr => pr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pr => pr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pr => pr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pr => pr.deleted == false);
				}
			}
			else
			{
				query = query.Where(pr => pr.active == true);
				query = query.Where(pr => pr.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Pending Registration, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.accountName.Contains(anyStringContains)
			       || x.emailAddress.Contains(anyStringContains)
			       || x.displayName.Contains(anyStringContains)
			       || x.passwordHash.Contains(anyStringContains)
			       || x.verificationCode.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || x.ipAddress.Contains(anyStringContains)
			       || x.userAgent.Contains(anyStringContains)
			       || x.verificationChannel.Contains(anyStringContains)
			       || x.failureReason.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single PendingRegistration by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PendingRegistration/{id}")]
		public async Task<IActionResult> GetPendingRegistration(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.PendingRegistration> query = (from pr in _context.PendingRegistrations where
							(pr.id == id) &&
							(userIsAdmin == true || pr.deleted == false) &&
							(userIsWriter == true || pr.active == true)
					select pr);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.PendingRegistration materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.PendingRegistration Entity was read with Admin privilege." : "BMC.PendingRegistration Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PendingRegistration", materialized.id, materialized.accountName));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.PendingRegistration entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.PendingRegistration.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.PendingRegistration.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing PendingRegistration record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/PendingRegistration/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutPendingRegistration(int id, [FromBody]Database.PendingRegistration.PendingRegistrationDTO pendingRegistrationDTO, CancellationToken cancellationToken = default)
		{
			if (pendingRegistrationDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != pendingRegistrationDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.PendingRegistration> query = (from x in _context.PendingRegistrations
				where
				(x.id == id)
				select x);


			Database.PendingRegistration existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.PendingRegistration PUT", id.ToString(), new Exception("No BMC.PendingRegistration entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (pendingRegistrationDTO.objectGuid == Guid.Empty)
            {
                pendingRegistrationDTO.objectGuid = existing.objectGuid;
            }
            else if (pendingRegistrationDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a PendingRegistration record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.PendingRegistration cloneOfExisting = (Database.PendingRegistration)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new PendingRegistration object using the data from the existing record, updated with what is in the DTO.
			//
			Database.PendingRegistration pendingRegistration = (Database.PendingRegistration)_context.Entry(existing).GetDatabaseValues().ToObject();
			pendingRegistration.ApplyDTO(pendingRegistrationDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (pendingRegistration.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.PendingRegistration record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (pendingRegistration.accountName != null && pendingRegistration.accountName.Length > 250)
			{
				pendingRegistration.accountName = pendingRegistration.accountName.Substring(0, 250);
			}

			if (pendingRegistration.emailAddress != null && pendingRegistration.emailAddress.Length > 100)
			{
				pendingRegistration.emailAddress = pendingRegistration.emailAddress.Substring(0, 100);
			}

			if (pendingRegistration.displayName != null && pendingRegistration.displayName.Length > 250)
			{
				pendingRegistration.displayName = pendingRegistration.displayName.Substring(0, 250);
			}

			if (pendingRegistration.passwordHash != null && pendingRegistration.passwordHash.Length > 250)
			{
				pendingRegistration.passwordHash = pendingRegistration.passwordHash.Substring(0, 250);
			}

			if (pendingRegistration.verificationCode != null && pendingRegistration.verificationCode.Length > 50)
			{
				pendingRegistration.verificationCode = pendingRegistration.verificationCode.Substring(0, 50);
			}

			if (pendingRegistration.codeExpiresAt.Kind != DateTimeKind.Utc)
			{
				pendingRegistration.codeExpiresAt = pendingRegistration.codeExpiresAt.ToUniversalTime();
			}

			if (pendingRegistration.status != null && pendingRegistration.status.Length > 50)
			{
				pendingRegistration.status = pendingRegistration.status.Substring(0, 50);
			}

			if (pendingRegistration.createdAt.Kind != DateTimeKind.Utc)
			{
				pendingRegistration.createdAt = pendingRegistration.createdAt.ToUniversalTime();
			}

			if (pendingRegistration.verifiedAt.HasValue == true && pendingRegistration.verifiedAt.Value.Kind != DateTimeKind.Utc)
			{
				pendingRegistration.verifiedAt = pendingRegistration.verifiedAt.Value.ToUniversalTime();
			}

			if (pendingRegistration.provisionedAt.HasValue == true && pendingRegistration.provisionedAt.Value.Kind != DateTimeKind.Utc)
			{
				pendingRegistration.provisionedAt = pendingRegistration.provisionedAt.Value.ToUniversalTime();
			}

			if (pendingRegistration.ipAddress != null && pendingRegistration.ipAddress.Length > 100)
			{
				pendingRegistration.ipAddress = pendingRegistration.ipAddress.Substring(0, 100);
			}

			if (pendingRegistration.userAgent != null && pendingRegistration.userAgent.Length > 500)
			{
				pendingRegistration.userAgent = pendingRegistration.userAgent.Substring(0, 500);
			}

			if (pendingRegistration.verificationChannel != null && pendingRegistration.verificationChannel.Length > 50)
			{
				pendingRegistration.verificationChannel = pendingRegistration.verificationChannel.Substring(0, 50);
			}

			if (pendingRegistration.failureReason != null && pendingRegistration.failureReason.Length > 1000)
			{
				pendingRegistration.failureReason = pendingRegistration.failureReason.Substring(0, 1000);
			}

			EntityEntry<Database.PendingRegistration> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(pendingRegistration);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.PendingRegistration entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.PendingRegistration.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PendingRegistration.CreateAnonymousWithFirstLevelSubObjects(pendingRegistration)),
					null);


				return Ok(Database.PendingRegistration.CreateAnonymous(pendingRegistration));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.PendingRegistration entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.PendingRegistration.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PendingRegistration.CreateAnonymousWithFirstLevelSubObjects(pendingRegistration)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new PendingRegistration record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PendingRegistration", Name = "PendingRegistration")]
		public async Task<IActionResult> PostPendingRegistration([FromBody]Database.PendingRegistration.PendingRegistrationDTO pendingRegistrationDTO, CancellationToken cancellationToken = default)
		{
			if (pendingRegistrationDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new PendingRegistration object using the data from the DTO
			//
			Database.PendingRegistration pendingRegistration = Database.PendingRegistration.FromDTO(pendingRegistrationDTO);

			try
			{
				if (pendingRegistration.accountName != null && pendingRegistration.accountName.Length > 250)
				{
					pendingRegistration.accountName = pendingRegistration.accountName.Substring(0, 250);
				}

				if (pendingRegistration.emailAddress != null && pendingRegistration.emailAddress.Length > 100)
				{
					pendingRegistration.emailAddress = pendingRegistration.emailAddress.Substring(0, 100);
				}

				if (pendingRegistration.displayName != null && pendingRegistration.displayName.Length > 250)
				{
					pendingRegistration.displayName = pendingRegistration.displayName.Substring(0, 250);
				}

				if (pendingRegistration.passwordHash != null && pendingRegistration.passwordHash.Length > 250)
				{
					pendingRegistration.passwordHash = pendingRegistration.passwordHash.Substring(0, 250);
				}

				if (pendingRegistration.verificationCode != null && pendingRegistration.verificationCode.Length > 50)
				{
					pendingRegistration.verificationCode = pendingRegistration.verificationCode.Substring(0, 50);
				}

				if (pendingRegistration.codeExpiresAt.Kind != DateTimeKind.Utc)
				{
					pendingRegistration.codeExpiresAt = pendingRegistration.codeExpiresAt.ToUniversalTime();
				}

				if (pendingRegistration.status != null && pendingRegistration.status.Length > 50)
				{
					pendingRegistration.status = pendingRegistration.status.Substring(0, 50);
				}

				if (pendingRegistration.createdAt.Kind != DateTimeKind.Utc)
				{
					pendingRegistration.createdAt = pendingRegistration.createdAt.ToUniversalTime();
				}

				if (pendingRegistration.verifiedAt.HasValue == true && pendingRegistration.verifiedAt.Value.Kind != DateTimeKind.Utc)
				{
					pendingRegistration.verifiedAt = pendingRegistration.verifiedAt.Value.ToUniversalTime();
				}

				if (pendingRegistration.provisionedAt.HasValue == true && pendingRegistration.provisionedAt.Value.Kind != DateTimeKind.Utc)
				{
					pendingRegistration.provisionedAt = pendingRegistration.provisionedAt.Value.ToUniversalTime();
				}

				if (pendingRegistration.ipAddress != null && pendingRegistration.ipAddress.Length > 100)
				{
					pendingRegistration.ipAddress = pendingRegistration.ipAddress.Substring(0, 100);
				}

				if (pendingRegistration.userAgent != null && pendingRegistration.userAgent.Length > 500)
				{
					pendingRegistration.userAgent = pendingRegistration.userAgent.Substring(0, 500);
				}

				if (pendingRegistration.verificationChannel != null && pendingRegistration.verificationChannel.Length > 50)
				{
					pendingRegistration.verificationChannel = pendingRegistration.verificationChannel.Substring(0, 50);
				}

				if (pendingRegistration.failureReason != null && pendingRegistration.failureReason.Length > 1000)
				{
					pendingRegistration.failureReason = pendingRegistration.failureReason.Substring(0, 1000);
				}

				pendingRegistration.objectGuid = Guid.NewGuid();
				_context.PendingRegistrations.Add(pendingRegistration);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.PendingRegistration entity successfully created.",
					true,
					pendingRegistration.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.PendingRegistration.CreateAnonymousWithFirstLevelSubObjects(pendingRegistration)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.PendingRegistration entity creation failed.", false, pendingRegistration.id.ToString(), "", JsonSerializer.Serialize(pendingRegistration), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PendingRegistration", pendingRegistration.id, pendingRegistration.accountName));

			return CreatedAtRoute("PendingRegistration", new { id = pendingRegistration.id }, Database.PendingRegistration.CreateAnonymousWithFirstLevelSubObjects(pendingRegistration));
		}



        /// <summary>
        /// 
        /// This deletes a PendingRegistration record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PendingRegistration/{id}")]
		[Route("api/PendingRegistration")]
		public async Task<IActionResult> DeletePendingRegistration(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.PendingRegistration> query = (from x in _context.PendingRegistrations
				where
				(x.id == id)
				select x);


			Database.PendingRegistration pendingRegistration = await query.FirstOrDefaultAsync(cancellationToken);

			if (pendingRegistration == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.PendingRegistration DELETE", id.ToString(), new Exception("No BMC.PendingRegistration entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.PendingRegistration cloneOfExisting = (Database.PendingRegistration)_context.Entry(pendingRegistration).GetDatabaseValues().ToObject();


			try
			{
				pendingRegistration.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.PendingRegistration entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.PendingRegistration.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PendingRegistration.CreateAnonymousWithFirstLevelSubObjects(pendingRegistration)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.PendingRegistration entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.PendingRegistration.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PendingRegistration.CreateAnonymousWithFirstLevelSubObjects(pendingRegistration)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of PendingRegistration records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/PendingRegistrations/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string accountName = null,
			string emailAddress = null,
			string displayName = null,
			string passwordHash = null,
			string verificationCode = null,
			DateTime? codeExpiresAt = null,
			int? verificationAttempts = null,
			string status = null,
			DateTime? createdAt = null,
			DateTime? verifiedAt = null,
			DateTime? provisionedAt = null,
			string ipAddress = null,
			string userAgent = null,
			string verificationChannel = null,
			string failureReason = null,
			int? provisionedSecurityUserId = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);


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
			if (codeExpiresAt.HasValue == true && codeExpiresAt.Value.Kind != DateTimeKind.Utc)
			{
				codeExpiresAt = codeExpiresAt.Value.ToUniversalTime();
			}

			if (createdAt.HasValue == true && createdAt.Value.Kind != DateTimeKind.Utc)
			{
				createdAt = createdAt.Value.ToUniversalTime();
			}

			if (verifiedAt.HasValue == true && verifiedAt.Value.Kind != DateTimeKind.Utc)
			{
				verifiedAt = verifiedAt.Value.ToUniversalTime();
			}

			if (provisionedAt.HasValue == true && provisionedAt.Value.Kind != DateTimeKind.Utc)
			{
				provisionedAt = provisionedAt.Value.ToUniversalTime();
			}

			IQueryable<Database.PendingRegistration> query = (from pr in _context.PendingRegistrations select pr);
			if (string.IsNullOrEmpty(accountName) == false)
			{
				query = query.Where(pr => pr.accountName == accountName);
			}
			if (string.IsNullOrEmpty(emailAddress) == false)
			{
				query = query.Where(pr => pr.emailAddress == emailAddress);
			}
			if (string.IsNullOrEmpty(displayName) == false)
			{
				query = query.Where(pr => pr.displayName == displayName);
			}
			if (string.IsNullOrEmpty(passwordHash) == false)
			{
				query = query.Where(pr => pr.passwordHash == passwordHash);
			}
			if (string.IsNullOrEmpty(verificationCode) == false)
			{
				query = query.Where(pr => pr.verificationCode == verificationCode);
			}
			if (codeExpiresAt.HasValue == true)
			{
				query = query.Where(pr => pr.codeExpiresAt == codeExpiresAt.Value);
			}
			if (verificationAttempts.HasValue == true)
			{
				query = query.Where(pr => pr.verificationAttempts == verificationAttempts.Value);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(pr => pr.status == status);
			}
			if (createdAt.HasValue == true)
			{
				query = query.Where(pr => pr.createdAt == createdAt.Value);
			}
			if (verifiedAt.HasValue == true)
			{
				query = query.Where(pr => pr.verifiedAt == verifiedAt.Value);
			}
			if (provisionedAt.HasValue == true)
			{
				query = query.Where(pr => pr.provisionedAt == provisionedAt.Value);
			}
			if (string.IsNullOrEmpty(ipAddress) == false)
			{
				query = query.Where(pr => pr.ipAddress == ipAddress);
			}
			if (string.IsNullOrEmpty(userAgent) == false)
			{
				query = query.Where(pr => pr.userAgent == userAgent);
			}
			if (string.IsNullOrEmpty(verificationChannel) == false)
			{
				query = query.Where(pr => pr.verificationChannel == verificationChannel);
			}
			if (string.IsNullOrEmpty(failureReason) == false)
			{
				query = query.Where(pr => pr.failureReason == failureReason);
			}
			if (provisionedSecurityUserId.HasValue == true)
			{
				query = query.Where(pr => pr.provisionedSecurityUserId == provisionedSecurityUserId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pr => pr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pr => pr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pr => pr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pr => pr.deleted == false);
				}
			}
			else
			{
				query = query.Where(pr => pr.active == true);
				query = query.Where(pr => pr.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Pending Registration, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.accountName.Contains(anyStringContains)
			       || x.emailAddress.Contains(anyStringContains)
			       || x.displayName.Contains(anyStringContains)
			       || x.passwordHash.Contains(anyStringContains)
			       || x.verificationCode.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || x.ipAddress.Contains(anyStringContains)
			       || x.userAgent.Contains(anyStringContains)
			       || x.verificationChannel.Contains(anyStringContains)
			       || x.failureReason.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.accountName).ThenBy(x => x.emailAddress).ThenBy(x => x.displayName);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.PendingRegistration.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/PendingRegistration/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
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
