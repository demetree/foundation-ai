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
using Foundation.Alerting.Database;
using Foundation.ChangeHistory;

namespace Foundation.Alerting.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the Integration entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Integration entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class IntegrationsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 150;

		static object integrationPutSyncRoot = new object();
		static object integrationDeleteSyncRoot = new object();

		private AlertingContext _context;

		private ILogger<IntegrationsController> _logger;

		public IntegrationsController(AlertingContext context, ILogger<IntegrationsController> logger) : base("Alerting", "Integration")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Integrations filtered by the parameters provided.
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
		[Route("api/Integrations")]
		public async Task<IActionResult> GetIntegrations(
			int? serviceId = null,
			string name = null,
			string description = null,
			string apiKeyHash = null,
			string callbackWebhookUrl = null,
			int? maxRetryAttempts = null,
			int? retryBackoffSeconds = null,
			string callbackOnEventTypes = null,
			DateTime? lastCallbackSuccessAt = null,
			int? consecutiveCallbackFailures = null,
			int? versionNumber = null,
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
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 150, cancellationToken);
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
			if (lastCallbackSuccessAt.HasValue == true && lastCallbackSuccessAt.Value.Kind != DateTimeKind.Utc)
			{
				lastCallbackSuccessAt = lastCallbackSuccessAt.Value.ToUniversalTime();
			}

			IQueryable<Database.Integration> query = (from i in _context.Integrations select i);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (serviceId.HasValue == true)
			{
				query = query.Where(i => i.serviceId == serviceId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(i => i.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(i => i.description == description);
			}
			if (string.IsNullOrEmpty(apiKeyHash) == false)
			{
				query = query.Where(i => i.apiKeyHash == apiKeyHash);
			}
			if (string.IsNullOrEmpty(callbackWebhookUrl) == false)
			{
				query = query.Where(i => i.callbackWebhookUrl == callbackWebhookUrl);
			}
			if (maxRetryAttempts.HasValue == true)
			{
				query = query.Where(i => i.maxRetryAttempts == maxRetryAttempts.Value);
			}
			if (retryBackoffSeconds.HasValue == true)
			{
				query = query.Where(i => i.retryBackoffSeconds == retryBackoffSeconds.Value);
			}
			if (string.IsNullOrEmpty(callbackOnEventTypes) == false)
			{
				query = query.Where(i => i.callbackOnEventTypes == callbackOnEventTypes);
			}
			if (lastCallbackSuccessAt.HasValue == true)
			{
				query = query.Where(i => i.lastCallbackSuccessAt == lastCallbackSuccessAt.Value);
			}
			if (consecutiveCallbackFailures.HasValue == true)
			{
				query = query.Where(i => i.consecutiveCallbackFailures == consecutiveCallbackFailures.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(i => i.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(i => i.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(i => i.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(i => i.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(i => i.deleted == false);
				}
			}
			else
			{
				query = query.Where(i => i.active == true);
				query = query.Where(i => i.deleted == false);
			}

			query = query.OrderBy(i => i.name).ThenBy(i => i.description).ThenBy(i => i.apiKeyHash);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.service);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Integration, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.apiKeyHash.Contains(anyStringContains)
			       || x.callbackWebhookUrl.Contains(anyStringContains)
			       || x.callbackOnEventTypes.Contains(anyStringContains)
			       || (includeRelations == true && x.service.name.Contains(anyStringContains))
			       || (includeRelations == true && x.service.description.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.Integration> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Integration integration in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(integration, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Alerting.Integration Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Alerting.Integration Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Integrations filtered by the parameters provided.  Its query is similar to the GetIntegrations method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Integrations/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? serviceId = null,
			string name = null,
			string description = null,
			string apiKeyHash = null,
			string callbackWebhookUrl = null,
			int? maxRetryAttempts = null,
			int? retryBackoffSeconds = null,
			string callbackOnEventTypes = null,
			DateTime? lastCallbackSuccessAt = null,
			int? consecutiveCallbackFailures = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 150, cancellationToken);
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
			if (lastCallbackSuccessAt.HasValue == true && lastCallbackSuccessAt.Value.Kind != DateTimeKind.Utc)
			{
				lastCallbackSuccessAt = lastCallbackSuccessAt.Value.ToUniversalTime();
			}

			IQueryable<Database.Integration> query = (from i in _context.Integrations select i);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (serviceId.HasValue == true)
			{
				query = query.Where(i => i.serviceId == serviceId.Value);
			}
			if (name != null)
			{
				query = query.Where(i => i.name == name);
			}
			if (description != null)
			{
				query = query.Where(i => i.description == description);
			}
			if (apiKeyHash != null)
			{
				query = query.Where(i => i.apiKeyHash == apiKeyHash);
			}
			if (callbackWebhookUrl != null)
			{
				query = query.Where(i => i.callbackWebhookUrl == callbackWebhookUrl);
			}
			if (maxRetryAttempts.HasValue == true)
			{
				query = query.Where(i => i.maxRetryAttempts == maxRetryAttempts.Value);
			}
			if (retryBackoffSeconds.HasValue == true)
			{
				query = query.Where(i => i.retryBackoffSeconds == retryBackoffSeconds.Value);
			}
			if (callbackOnEventTypes != null)
			{
				query = query.Where(i => i.callbackOnEventTypes == callbackOnEventTypes);
			}
			if (lastCallbackSuccessAt.HasValue == true)
			{
				query = query.Where(i => i.lastCallbackSuccessAt == lastCallbackSuccessAt.Value);
			}
			if (consecutiveCallbackFailures.HasValue == true)
			{
				query = query.Where(i => i.consecutiveCallbackFailures == consecutiveCallbackFailures.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(i => i.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(i => i.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(i => i.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(i => i.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(i => i.deleted == false);
				}
			}
			else
			{
				query = query.Where(i => i.active == true);
				query = query.Where(i => i.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Integration, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.apiKeyHash.Contains(anyStringContains)
			       || x.callbackWebhookUrl.Contains(anyStringContains)
			       || x.callbackOnEventTypes.Contains(anyStringContains)
			       || x.service.name.Contains(anyStringContains)
			       || x.service.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single Integration by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Integration/{id}")]
		public async Task<IActionResult> GetIntegration(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 150, cancellationToken);
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
				IQueryable<Database.Integration> query = (from i in _context.Integrations where
							(i.id == id) &&
							(userIsAdmin == true || i.deleted == false) &&
							(userIsWriter == true || i.active == true)
					select i);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.service);
					query = query.AsSplitQuery();
				}

				Database.Integration materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Alerting.Integration Entity was read with Admin privilege." : "Alerting.Integration Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Integration", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Alerting.Integration entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Alerting.Integration.   Entity was read with Admin privilege." : "Exception caught during entity read of Alerting.Integration.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing Integration record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Integration/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutIntegration(int id, [FromBody]Database.Integration.IntegrationDTO integrationDTO, CancellationToken cancellationToken = default)
		{
			if (integrationDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Alerting Master Config Writer role needed to write to this table, or Alerting Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Alerting Master Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != integrationDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 150, cancellationToken);
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


			IQueryable<Database.Integration> query = (from x in _context.Integrations
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Integration existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.Integration PUT", id.ToString(), new Exception("No Alerting.Integration entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (integrationDTO.objectGuid == Guid.Empty)
            {
                integrationDTO.objectGuid = existing.objectGuid;
            }
            else if (integrationDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Integration record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Integration cloneOfExisting = (Database.Integration)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Integration object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Integration integration = (Database.Integration)_context.Entry(existing).GetDatabaseValues().ToObject();
			integration.ApplyDTO(integrationDTO);
			//
			// The tenant guid for any Integration being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the Integration because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				integration.tenantGuid = existing.tenantGuid;
			}

			lock (integrationPutSyncRoot)
			{
				//
				// Validate the version number for the integration being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != integration.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Integration save attempt was made but save request was with version " + integration.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The Integration you are trying to update has already changed.  Please try your save again after reloading the Integration.");
				}
				else
				{
					// Same record.  Increase version.
					integration.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (integration.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Alerting.Integration record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (integration.name != null && integration.name.Length > 100)
				{
					integration.name = integration.name.Substring(0, 100);
				}

				if (integration.description != null && integration.description.Length > 500)
				{
					integration.description = integration.description.Substring(0, 500);
				}

				if (integration.apiKeyHash != null && integration.apiKeyHash.Length > 250)
				{
					integration.apiKeyHash = integration.apiKeyHash.Substring(0, 250);
				}

				if (integration.callbackWebhookUrl != null && integration.callbackWebhookUrl.Length > 1000)
				{
					integration.callbackWebhookUrl = integration.callbackWebhookUrl.Substring(0, 1000);
				}

				if (integration.callbackOnEventTypes != null && integration.callbackOnEventTypes.Length > 500)
				{
					integration.callbackOnEventTypes = integration.callbackOnEventTypes.Substring(0, 500);
				}

				if (integration.lastCallbackSuccessAt.HasValue == true && integration.lastCallbackSuccessAt.Value.Kind != DateTimeKind.Utc)
				{
					integration.lastCallbackSuccessAt = integration.lastCallbackSuccessAt.Value.ToUniversalTime();
				}

				try
				{
				    EntityEntry<Database.Integration> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(integration);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        IntegrationChangeHistory integrationChangeHistory = new IntegrationChangeHistory();
				        integrationChangeHistory.integrationId = integration.id;
				        integrationChangeHistory.versionNumber = integration.versionNumber;
				        integrationChangeHistory.timeStamp = DateTime.UtcNow;
				        integrationChangeHistory.userId = securityUser.id;
				        integrationChangeHistory.tenantGuid = userTenantGuid;
				        integrationChangeHistory.data = JsonSerializer.Serialize(Database.Integration.CreateAnonymousWithFirstLevelSubObjects(integration));
				        _context.IntegrationChangeHistories.Add(integrationChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.Integration entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Integration.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Integration.CreateAnonymousWithFirstLevelSubObjects(integration)),
						null);

				return Ok(Database.Integration.CreateAnonymous(integration));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.Integration entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Integration.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Integration.CreateAnonymousWithFirstLevelSubObjects(integration)),
						ex);

					return Problem(ex.Message);
				}

			}
		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new Integration record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Integration", Name = "Integration")]
		public async Task<IActionResult> PostIntegration([FromBody]Database.Integration.IntegrationDTO integrationDTO, CancellationToken cancellationToken = default)
		{
			if (integrationDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Alerting Master Config Writer role needed to write to this table, or Alerting Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Alerting Master Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new Integration object using the data from the DTO
			//
			Database.Integration integration = Database.Integration.FromDTO(integrationDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				integration.tenantGuid = userTenantGuid;

				if (integration.name != null && integration.name.Length > 100)
				{
					integration.name = integration.name.Substring(0, 100);
				}

				if (integration.description != null && integration.description.Length > 500)
				{
					integration.description = integration.description.Substring(0, 500);
				}

				if (integration.apiKeyHash != null && integration.apiKeyHash.Length > 250)
				{
					integration.apiKeyHash = integration.apiKeyHash.Substring(0, 250);
				}

				if (integration.callbackWebhookUrl != null && integration.callbackWebhookUrl.Length > 1000)
				{
					integration.callbackWebhookUrl = integration.callbackWebhookUrl.Substring(0, 1000);
				}

				if (integration.callbackOnEventTypes != null && integration.callbackOnEventTypes.Length > 500)
				{
					integration.callbackOnEventTypes = integration.callbackOnEventTypes.Substring(0, 500);
				}

				if (integration.lastCallbackSuccessAt.HasValue == true && integration.lastCallbackSuccessAt.Value.Kind != DateTimeKind.Utc)
				{
					integration.lastCallbackSuccessAt = integration.lastCallbackSuccessAt.Value.ToUniversalTime();
				}

				integration.objectGuid = Guid.NewGuid();
				integration.versionNumber = 1;

				_context.Integrations.Add(integration);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the integration object so that no further changes will be written to the database
				    //
				    _context.Entry(integration).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					integration.IntegrationCallbackIncidentEventTypes = null;
					integration.IntegrationChangeHistories = null;
					integration.WebhookDeliveryAttempts = null;
					integration.service = null;


				    IntegrationChangeHistory integrationChangeHistory = new IntegrationChangeHistory();
				    integrationChangeHistory.integrationId = integration.id;
				    integrationChangeHistory.versionNumber = integration.versionNumber;
				    integrationChangeHistory.timeStamp = DateTime.UtcNow;
				    integrationChangeHistory.userId = securityUser.id;
				    integrationChangeHistory.tenantGuid = userTenantGuid;
				    integrationChangeHistory.data = JsonSerializer.Serialize(Database.Integration.CreateAnonymousWithFirstLevelSubObjects(integration));
				    _context.IntegrationChangeHistories.Add(integrationChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Alerting.Integration entity successfully created.",
						true,
						integration. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.Integration.CreateAnonymousWithFirstLevelSubObjects(integration)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Alerting.Integration entity creation failed.", false, integration.id.ToString(), "", JsonSerializer.Serialize(Database.Integration.CreateAnonymousWithFirstLevelSubObjects(integration)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Integration", integration.id, integration.name));

			return CreatedAtRoute("Integration", new { id = integration.id }, Database.Integration.CreateAnonymousWithFirstLevelSubObjects(integration));
		}

*/

/* This function is expected to be overridden in a custom file

        /// <summary>
        /// 
        /// This rolls a Integration entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Integration/Rollback/{id}")]
		[Route("api/Integration/Rollback")]
		public async Task<IActionResult> RollbackToIntegrationVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
		{
			//
			// Data rollback is an admin only function, like Deletes.
			//
			StartAuditEventClock();
			
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			

			
			IQueryable <Database.Integration> query = (from x in _context.Integrations
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this Integration concurrently
			//
			lock (integrationPutSyncRoot)
			{
				
				Database.Integration integration = query.FirstOrDefault();
				
				if (integration == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.Integration rollback", id.ToString(), new Exception("No Alerting.Integration entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the Integration current state so we can log it.
				//
				Database.Integration cloneOfExisting = (Database.Integration)_context.Entry(integration).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.IntegrationCallbackIncidentEventTypes = null;
				cloneOfExisting.IntegrationChangeHistories = null;
				cloneOfExisting.WebhookDeliveryAttempts = null;
				cloneOfExisting.service = null;

				if (versionNumber >= integration.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Alerting.Integration rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Alerting.Integration rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				IntegrationChangeHistory integrationChangeHistory = (from x in _context.IntegrationChangeHistories
				                                               where
				                                               x.integrationId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (integrationChangeHistory != null)
				{
				    Database.Integration oldIntegration = JsonSerializer.Deserialize<Database.Integration>(integrationChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    integration.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    integration.serviceId = oldIntegration.serviceId;
				    integration.name = oldIntegration.name;
				    integration.description = oldIntegration.description;
				    integration.apiKeyHash = oldIntegration.apiKeyHash;
				    integration.callbackWebhookUrl = oldIntegration.callbackWebhookUrl;
				    integration.maxRetryAttempts = oldIntegration.maxRetryAttempts;
				    integration.retryBackoffSeconds = oldIntegration.retryBackoffSeconds;
				    integration.callbackOnEventTypes = oldIntegration.callbackOnEventTypes;
				    integration.lastCallbackSuccessAt = oldIntegration.lastCallbackSuccessAt;
				    integration.consecutiveCallbackFailures = oldIntegration.consecutiveCallbackFailures;
				    integration.objectGuid = oldIntegration.objectGuid;
				    integration.active = oldIntegration.active;
				    integration.deleted = oldIntegration.deleted;

				    string serializedIntegration = JsonSerializer.Serialize(integration);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        IntegrationChangeHistory newIntegrationChangeHistory = new IntegrationChangeHistory();
				        newIntegrationChangeHistory.integrationId = integration.id;
				        newIntegrationChangeHistory.versionNumber = integration.versionNumber;
				        newIntegrationChangeHistory.timeStamp = DateTime.UtcNow;
				        newIntegrationChangeHistory.userId = securityUser.id;
				        newIntegrationChangeHistory.tenantGuid = userTenantGuid;
				        newIntegrationChangeHistory.data = JsonSerializer.Serialize(Database.Integration.CreateAnonymousWithFirstLevelSubObjects(integration));
				        _context.IntegrationChangeHistories.Add(newIntegrationChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.Integration rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Integration.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Integration.CreateAnonymousWithFirstLevelSubObjects(integration)),
						null);


				    return Ok(Database.Integration.CreateAnonymous(integration));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Alerting.Integration rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Alerting.Integration rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}

*/



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a Integration.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Integration</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Integration/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetIntegrationChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
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


			Database.Integration integration = await _context.Integrations.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (integration == null)
			{
				return NotFound();
			}

			try
			{
				integration.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Integration> versionInfo = await integration.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

				if (versionInfo == null)
				{
					return NotFound($"Version {versionNumber} not found.");
				}

				return Ok(versionInfo);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets the full audit history for a Integration.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Integration</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Integration/{id}/AuditHistory")]
		public async Task<IActionResult> GetIntegrationAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
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


			Database.Integration integration = await _context.Integrations.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (integration == null)
			{
				return NotFound();
			}

			try
			{
				integration.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.Integration>> versions = await integration.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a Integration.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Integration</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The Integration object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Integration/{id}/Version/{version}")]
		public async Task<IActionResult> GetIntegrationVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
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


			Database.Integration integration = await _context.Integrations.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (integration == null)
			{
				return NotFound();
			}

			try
			{
				integration.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Integration> versionInfo = await integration.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

				if (versionInfo == null || versionInfo.data == null)
				{
					return NotFound();
				}

				return Ok(versionInfo.data.ToOutputDTO());
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets the state of a Integration at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Integration</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The Integration object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Integration/{id}/StateAtTime")]
		public async Task<IActionResult> GetIntegrationStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
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


			Database.Integration integration = await _context.Integrations.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (integration == null)
			{
				return NotFound();
			}

			try
			{
				integration.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Integration> versionInfo = await integration.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

				if (versionInfo == null || versionInfo.data == null)
				{
					return NotFound("No state found at specified time.");
				}

				return Ok(versionInfo.data.ToOutputDTO());
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a Integration record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Integration/{id}")]
		[Route("api/Integration")]
		public async Task<IActionResult> DeleteIntegration(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Alerting Master Config Writer role needed to write to this table, or Alerting Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Alerting Master Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.Integration> query = (from x in _context.Integrations
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Integration integration = await query.FirstOrDefaultAsync(cancellationToken);

			if (integration == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.Integration DELETE", id.ToString(), new Exception("No Alerting.Integration entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Integration cloneOfExisting = (Database.Integration)_context.Entry(integration).GetDatabaseValues().ToObject();


			lock (integrationDeleteSyncRoot)
			{
			    try
			    {
			        integration.deleted = true;
			        integration.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        IntegrationChangeHistory integrationChangeHistory = new IntegrationChangeHistory();
			        integrationChangeHistory.integrationId = integration.id;
			        integrationChangeHistory.versionNumber = integration.versionNumber;
			        integrationChangeHistory.timeStamp = DateTime.UtcNow;
			        integrationChangeHistory.userId = securityUser.id;
			        integrationChangeHistory.tenantGuid = userTenantGuid;
			        integrationChangeHistory.data = JsonSerializer.Serialize(Database.Integration.CreateAnonymousWithFirstLevelSubObjects(integration));
			        _context.IntegrationChangeHistories.Add(integrationChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Alerting.Integration entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Integration.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Integration.CreateAnonymousWithFirstLevelSubObjects(integration)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Alerting.Integration entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Integration.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Integration.CreateAnonymousWithFirstLevelSubObjects(integration)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


*/
        /// <summary>
        /// 
        /// This gets a list of Integration records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Integrations/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? serviceId = null,
			string name = null,
			string description = null,
			string apiKeyHash = null,
			string callbackWebhookUrl = null,
			int? maxRetryAttempts = null,
			int? retryBackoffSeconds = null,
			string callbackOnEventTypes = null,
			DateTime? lastCallbackSuccessAt = null,
			int? consecutiveCallbackFailures = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 150, cancellationToken);


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
			if (lastCallbackSuccessAt.HasValue == true && lastCallbackSuccessAt.Value.Kind != DateTimeKind.Utc)
			{
				lastCallbackSuccessAt = lastCallbackSuccessAt.Value.ToUniversalTime();
			}

			IQueryable<Database.Integration> query = (from i in _context.Integrations select i);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (serviceId.HasValue == true)
			{
				query = query.Where(i => i.serviceId == serviceId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(i => i.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(i => i.description == description);
			}
			if (string.IsNullOrEmpty(apiKeyHash) == false)
			{
				query = query.Where(i => i.apiKeyHash == apiKeyHash);
			}
			if (string.IsNullOrEmpty(callbackWebhookUrl) == false)
			{
				query = query.Where(i => i.callbackWebhookUrl == callbackWebhookUrl);
			}
			if (maxRetryAttempts.HasValue == true)
			{
				query = query.Where(i => i.maxRetryAttempts == maxRetryAttempts.Value);
			}
			if (retryBackoffSeconds.HasValue == true)
			{
				query = query.Where(i => i.retryBackoffSeconds == retryBackoffSeconds.Value);
			}
			if (string.IsNullOrEmpty(callbackOnEventTypes) == false)
			{
				query = query.Where(i => i.callbackOnEventTypes == callbackOnEventTypes);
			}
			if (lastCallbackSuccessAt.HasValue == true)
			{
				query = query.Where(i => i.lastCallbackSuccessAt == lastCallbackSuccessAt.Value);
			}
			if (consecutiveCallbackFailures.HasValue == true)
			{
				query = query.Where(i => i.consecutiveCallbackFailures == consecutiveCallbackFailures.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(i => i.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(i => i.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(i => i.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(i => i.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(i => i.deleted == false);
				}
			}
			else
			{
				query = query.Where(i => i.active == true);
				query = query.Where(i => i.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Integration, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.apiKeyHash.Contains(anyStringContains)
			       || x.callbackWebhookUrl.Contains(anyStringContains)
			       || x.callbackOnEventTypes.Contains(anyStringContains)
			       || x.service.name.Contains(anyStringContains)
			       || x.service.description.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.apiKeyHash);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Integration.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Integration/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Master Config Writer role needed to write to this table, or Alerting Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Alerting Master Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
