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
    /// This auto generated class provides the basic CRUD operations for the ApiKey entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ApiKey entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ApiKeysController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		private BMCContext _context;

		private ILogger<ApiKeysController> _logger;

		public ApiKeysController(BMCContext context, ILogger<ApiKeysController> logger) : base("BMC", "ApiKey")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ApiKeys filtered by the parameters provided.
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
		[Route("api/ApiKeys")]
		public async Task<IActionResult> GetApiKeys(
			string keyHash = null,
			string keyPrefix = null,
			string name = null,
			string description = null,
			bool? isActive = null,
			DateTime? createdDate = null,
			DateTime? lastUsedDate = null,
			DateTime? expiresDate = null,
			int? rateLimitPerHour = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}

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
			if (createdDate.HasValue == true && createdDate.Value.Kind != DateTimeKind.Utc)
			{
				createdDate = createdDate.Value.ToUniversalTime();
			}

			if (lastUsedDate.HasValue == true && lastUsedDate.Value.Kind != DateTimeKind.Utc)
			{
				lastUsedDate = lastUsedDate.Value.ToUniversalTime();
			}

			if (expiresDate.HasValue == true && expiresDate.Value.Kind != DateTimeKind.Utc)
			{
				expiresDate = expiresDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ApiKey> query = (from ak in _context.ApiKeys select ak);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(keyHash) == false)
			{
				query = query.Where(ak => ak.keyHash == keyHash);
			}
			if (string.IsNullOrEmpty(keyPrefix) == false)
			{
				query = query.Where(ak => ak.keyPrefix == keyPrefix);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ak => ak.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ak => ak.description == description);
			}
			if (isActive.HasValue == true)
			{
				query = query.Where(ak => ak.isActive == isActive.Value);
			}
			if (createdDate.HasValue == true)
			{
				query = query.Where(ak => ak.createdDate == createdDate.Value);
			}
			if (lastUsedDate.HasValue == true)
			{
				query = query.Where(ak => ak.lastUsedDate == lastUsedDate.Value);
			}
			if (expiresDate.HasValue == true)
			{
				query = query.Where(ak => ak.expiresDate == expiresDate.Value);
			}
			if (rateLimitPerHour.HasValue == true)
			{
				query = query.Where(ak => ak.rateLimitPerHour == rateLimitPerHour.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ak => ak.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ak => ak.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ak => ak.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ak => ak.deleted == false);
				}
			}
			else
			{
				query = query.Where(ak => ak.active == true);
				query = query.Where(ak => ak.deleted == false);
			}

			query = query.OrderBy(ak => ak.keyHash).ThenBy(ak => ak.keyPrefix).ThenBy(ak => ak.name);


			//
			// Add the any string contains parameter to span all the string fields on the Api Key, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.keyHash.Contains(anyStringContains)
			       || x.keyPrefix.Contains(anyStringContains)
			       || x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
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
			
			List<Database.ApiKey> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ApiKey apiKey in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(apiKey, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.ApiKey Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.ApiKey Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ApiKeys filtered by the parameters provided.  Its query is similar to the GetApiKeys method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ApiKeys/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string keyHash = null,
			string keyPrefix = null,
			string name = null,
			string description = null,
			bool? isActive = null,
			DateTime? createdDate = null,
			DateTime? lastUsedDate = null,
			DateTime? expiresDate = null,
			int? rateLimitPerHour = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			//
			// Fix any non-UTC date parameters that come in.
			//
			if (createdDate.HasValue == true && createdDate.Value.Kind != DateTimeKind.Utc)
			{
				createdDate = createdDate.Value.ToUniversalTime();
			}

			if (lastUsedDate.HasValue == true && lastUsedDate.Value.Kind != DateTimeKind.Utc)
			{
				lastUsedDate = lastUsedDate.Value.ToUniversalTime();
			}

			if (expiresDate.HasValue == true && expiresDate.Value.Kind != DateTimeKind.Utc)
			{
				expiresDate = expiresDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ApiKey> query = (from ak in _context.ApiKeys select ak);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (keyHash != null)
			{
				query = query.Where(ak => ak.keyHash == keyHash);
			}
			if (keyPrefix != null)
			{
				query = query.Where(ak => ak.keyPrefix == keyPrefix);
			}
			if (name != null)
			{
				query = query.Where(ak => ak.name == name);
			}
			if (description != null)
			{
				query = query.Where(ak => ak.description == description);
			}
			if (isActive.HasValue == true)
			{
				query = query.Where(ak => ak.isActive == isActive.Value);
			}
			if (createdDate.HasValue == true)
			{
				query = query.Where(ak => ak.createdDate == createdDate.Value);
			}
			if (lastUsedDate.HasValue == true)
			{
				query = query.Where(ak => ak.lastUsedDate == lastUsedDate.Value);
			}
			if (expiresDate.HasValue == true)
			{
				query = query.Where(ak => ak.expiresDate == expiresDate.Value);
			}
			if (rateLimitPerHour.HasValue == true)
			{
				query = query.Where(ak => ak.rateLimitPerHour == rateLimitPerHour.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ak => ak.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ak => ak.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ak => ak.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ak => ak.deleted == false);
				}
			}
			else
			{
				query = query.Where(ak => ak.active == true);
				query = query.Where(ak => ak.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Api Key, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.keyHash.Contains(anyStringContains)
			       || x.keyPrefix.Contains(anyStringContains)
			       || x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ApiKey by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ApiKey/{id}")]
		public async Task<IActionResult> GetApiKey(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			
			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			try
			{
				IQueryable<Database.ApiKey> query = (from ak in _context.ApiKeys where
							(ak.id == id) &&
							(userIsAdmin == true || ak.deleted == false) &&
							(userIsWriter == true || ak.active == true)
					select ak);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.ApiKey materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.ApiKey Entity was read with Admin privilege." : "BMC.ApiKey Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ApiKey", materialized.id, materialized.keyHash));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.ApiKey entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.ApiKey.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.ApiKey.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ApiKey record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ApiKey/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutApiKey(int id, [FromBody]Database.ApiKey.ApiKeyDTO apiKeyDTO, CancellationToken cancellationToken = default)
		{
			if (apiKeyDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != apiKeyDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			IQueryable<Database.ApiKey> query = (from x in _context.ApiKeys
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ApiKey existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ApiKey PUT", id.ToString(), new Exception("No BMC.ApiKey entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (apiKeyDTO.objectGuid == Guid.Empty)
            {
                apiKeyDTO.objectGuid = existing.objectGuid;
            }
            else if (apiKeyDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ApiKey record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ApiKey cloneOfExisting = (Database.ApiKey)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ApiKey object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ApiKey apiKey = (Database.ApiKey)_context.Entry(existing).GetDatabaseValues().ToObject();
			apiKey.ApplyDTO(apiKeyDTO);
			//
			// The tenant guid for any ApiKey being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ApiKey because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				apiKey.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (apiKey.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.ApiKey record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (apiKey.keyHash != null && apiKey.keyHash.Length > 250)
			{
				apiKey.keyHash = apiKey.keyHash.Substring(0, 250);
			}

			if (apiKey.keyPrefix != null && apiKey.keyPrefix.Length > 100)
			{
				apiKey.keyPrefix = apiKey.keyPrefix.Substring(0, 100);
			}

			if (apiKey.name != null && apiKey.name.Length > 100)
			{
				apiKey.name = apiKey.name.Substring(0, 100);
			}

			if (apiKey.createdDate.Kind != DateTimeKind.Utc)
			{
				apiKey.createdDate = apiKey.createdDate.ToUniversalTime();
			}

			if (apiKey.lastUsedDate.HasValue == true && apiKey.lastUsedDate.Value.Kind != DateTimeKind.Utc)
			{
				apiKey.lastUsedDate = apiKey.lastUsedDate.Value.ToUniversalTime();
			}

			if (apiKey.expiresDate.HasValue == true && apiKey.expiresDate.Value.Kind != DateTimeKind.Utc)
			{
				apiKey.expiresDate = apiKey.expiresDate.Value.ToUniversalTime();
			}

			EntityEntry<Database.ApiKey> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(apiKey);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.ApiKey entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ApiKey.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ApiKey.CreateAnonymousWithFirstLevelSubObjects(apiKey)),
					null);


				return Ok(Database.ApiKey.CreateAnonymous(apiKey));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.ApiKey entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ApiKey.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ApiKey.CreateAnonymousWithFirstLevelSubObjects(apiKey)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new ApiKey record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ApiKey", Name = "ApiKey")]
		public async Task<IActionResult> PostApiKey([FromBody]Database.ApiKey.ApiKeyDTO apiKeyDTO, CancellationToken cancellationToken = default)
		{
			if (apiKeyDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}

			//
			// Create a new ApiKey object using the data from the DTO
			//
			Database.ApiKey apiKey = Database.ApiKey.FromDTO(apiKeyDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				apiKey.tenantGuid = userTenantGuid;

				if (apiKey.keyHash != null && apiKey.keyHash.Length > 250)
				{
					apiKey.keyHash = apiKey.keyHash.Substring(0, 250);
				}

				if (apiKey.keyPrefix != null && apiKey.keyPrefix.Length > 100)
				{
					apiKey.keyPrefix = apiKey.keyPrefix.Substring(0, 100);
				}

				if (apiKey.name != null && apiKey.name.Length > 100)
				{
					apiKey.name = apiKey.name.Substring(0, 100);
				}

				if (apiKey.createdDate.Kind != DateTimeKind.Utc)
				{
					apiKey.createdDate = apiKey.createdDate.ToUniversalTime();
				}

				if (apiKey.lastUsedDate.HasValue == true && apiKey.lastUsedDate.Value.Kind != DateTimeKind.Utc)
				{
					apiKey.lastUsedDate = apiKey.lastUsedDate.Value.ToUniversalTime();
				}

				if (apiKey.expiresDate.HasValue == true && apiKey.expiresDate.Value.Kind != DateTimeKind.Utc)
				{
					apiKey.expiresDate = apiKey.expiresDate.Value.ToUniversalTime();
				}

				apiKey.objectGuid = Guid.NewGuid();
				_context.ApiKeys.Add(apiKey);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.ApiKey entity successfully created.",
					true,
					apiKey.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ApiKey.CreateAnonymousWithFirstLevelSubObjects(apiKey)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.ApiKey entity creation failed.", false, apiKey.id.ToString(), "", JsonSerializer.Serialize(apiKey), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ApiKey", apiKey.id, apiKey.keyHash));

			return CreatedAtRoute("ApiKey", new { id = apiKey.id }, Database.ApiKey.CreateAnonymousWithFirstLevelSubObjects(apiKey));
		}



        /// <summary>
        /// 
        /// This deletes a ApiKey record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ApiKey/{id}")]
		[Route("api/ApiKey")]
		public async Task<IActionResult> DeleteApiKey(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			
			
			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}

			IQueryable<Database.ApiKey> query = (from x in _context.ApiKeys
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ApiKey apiKey = await query.FirstOrDefaultAsync(cancellationToken);

			if (apiKey == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ApiKey DELETE", id.ToString(), new Exception("No BMC.ApiKey entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ApiKey cloneOfExisting = (Database.ApiKey)_context.Entry(apiKey).GetDatabaseValues().ToObject();


			try
			{
				apiKey.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ApiKey entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ApiKey.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ApiKey.CreateAnonymousWithFirstLevelSubObjects(apiKey)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ApiKey entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ApiKey.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ApiKey.CreateAnonymousWithFirstLevelSubObjects(apiKey)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of ApiKey records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ApiKeys/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string keyHash = null,
			string keyPrefix = null,
			string name = null,
			string description = null,
			bool? isActive = null,
			DateTime? createdDate = null,
			DateTime? lastUsedDate = null,
			DateTime? expiresDate = null,
			int? rateLimitPerHour = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);


			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


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
			if (createdDate.HasValue == true && createdDate.Value.Kind != DateTimeKind.Utc)
			{
				createdDate = createdDate.Value.ToUniversalTime();
			}

			if (lastUsedDate.HasValue == true && lastUsedDate.Value.Kind != DateTimeKind.Utc)
			{
				lastUsedDate = lastUsedDate.Value.ToUniversalTime();
			}

			if (expiresDate.HasValue == true && expiresDate.Value.Kind != DateTimeKind.Utc)
			{
				expiresDate = expiresDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ApiKey> query = (from ak in _context.ApiKeys select ak);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(keyHash) == false)
			{
				query = query.Where(ak => ak.keyHash == keyHash);
			}
			if (string.IsNullOrEmpty(keyPrefix) == false)
			{
				query = query.Where(ak => ak.keyPrefix == keyPrefix);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ak => ak.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ak => ak.description == description);
			}
			if (isActive.HasValue == true)
			{
				query = query.Where(ak => ak.isActive == isActive.Value);
			}
			if (createdDate.HasValue == true)
			{
				query = query.Where(ak => ak.createdDate == createdDate.Value);
			}
			if (lastUsedDate.HasValue == true)
			{
				query = query.Where(ak => ak.lastUsedDate == lastUsedDate.Value);
			}
			if (expiresDate.HasValue == true)
			{
				query = query.Where(ak => ak.expiresDate == expiresDate.Value);
			}
			if (rateLimitPerHour.HasValue == true)
			{
				query = query.Where(ak => ak.rateLimitPerHour == rateLimitPerHour.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ak => ak.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ak => ak.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ak => ak.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ak => ak.deleted == false);
				}
			}
			else
			{
				query = query.Where(ak => ak.active == true);
				query = query.Where(ak => ak.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Api Key, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.keyHash.Contains(anyStringContains)
			       || x.keyPrefix.Contains(anyStringContains)
			       || x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.keyHash).ThenBy(x => x.keyPrefix).ThenBy(x => x.name);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ApiKey.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ApiKey/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
