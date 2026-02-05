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

namespace Foundation.Alerting.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the WebhookDeliveryAttempt entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the WebhookDeliveryAttempt entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class WebhookDeliveryAttemptsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private AlertingContext _context;

		private ILogger<WebhookDeliveryAttemptsController> _logger;

		public WebhookDeliveryAttemptsController(AlertingContext context, ILogger<WebhookDeliveryAttemptsController> logger) : base("Alerting", "WebhookDeliveryAttempt")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of WebhookDeliveryAttempts filtered by the parameters provided.
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
		[Route("api/WebhookDeliveryAttempts")]
		public async Task<IActionResult> GetWebhookDeliveryAttempts(
			int? incidentId = null,
			int? integrationId = null,
			int? incidentTimelineEventId = null,
			int? attemptNumber = null,
			DateTime? attemptedAt = null,
			int? httpStatusCode = null,
			bool? success = null,
			string payloadJson = null,
			string responseBody = null,
			string errorMessage = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
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
			if (attemptedAt.HasValue == true && attemptedAt.Value.Kind != DateTimeKind.Utc)
			{
				attemptedAt = attemptedAt.Value.ToUniversalTime();
			}

			IQueryable<Database.WebhookDeliveryAttempt> query = (from wda in _context.WebhookDeliveryAttempts select wda);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (incidentId.HasValue == true)
			{
				query = query.Where(wda => wda.incidentId == incidentId.Value);
			}
			if (integrationId.HasValue == true)
			{
				query = query.Where(wda => wda.integrationId == integrationId.Value);
			}
			if (incidentTimelineEventId.HasValue == true)
			{
				query = query.Where(wda => wda.incidentTimelineEventId == incidentTimelineEventId.Value);
			}
			if (attemptNumber.HasValue == true)
			{
				query = query.Where(wda => wda.attemptNumber == attemptNumber.Value);
			}
			if (attemptedAt.HasValue == true)
			{
				query = query.Where(wda => wda.attemptedAt == attemptedAt.Value);
			}
			if (httpStatusCode.HasValue == true)
			{
				query = query.Where(wda => wda.httpStatusCode == httpStatusCode.Value);
			}
			if (success.HasValue == true)
			{
				query = query.Where(wda => wda.success == success.Value);
			}
			if (string.IsNullOrEmpty(payloadJson) == false)
			{
				query = query.Where(wda => wda.payloadJson == payloadJson);
			}
			if (string.IsNullOrEmpty(responseBody) == false)
			{
				query = query.Where(wda => wda.responseBody == responseBody);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(wda => wda.errorMessage == errorMessage);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(wda => wda.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(wda => wda.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(wda => wda.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(wda => wda.deleted == false);
				}
			}
			else
			{
				query = query.Where(wda => wda.active == true);
				query = query.Where(wda => wda.deleted == false);
			}

			query = query.OrderBy(wda => wda.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.incident);
				query = query.Include(x => x.incidentTimelineEvent);
				query = query.Include(x => x.integration);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Webhook Delivery Attempt, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.payloadJson.Contains(anyStringContains)
			       || x.responseBody.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			       || (includeRelations == true && x.incident.incidentKey.Contains(anyStringContains))
			       || (includeRelations == true && x.incident.title.Contains(anyStringContains))
			       || (includeRelations == true && x.incident.description.Contains(anyStringContains))
			       || (includeRelations == true && x.incident.sourcePayloadJson.Contains(anyStringContains))
			       || (includeRelations == true && x.incidentTimelineEvent.detailsJson.Contains(anyStringContains))
			       || (includeRelations == true && x.incidentTimelineEvent.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.incidentTimelineEvent.source.Contains(anyStringContains))
			       || (includeRelations == true && x.integration.name.Contains(anyStringContains))
			       || (includeRelations == true && x.integration.description.Contains(anyStringContains))
			       || (includeRelations == true && x.integration.apiKeyHash.Contains(anyStringContains))
			       || (includeRelations == true && x.integration.callbackWebhookUrl.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.WebhookDeliveryAttempt> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.WebhookDeliveryAttempt webhookDeliveryAttempt in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(webhookDeliveryAttempt, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Alerting.WebhookDeliveryAttempt Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Alerting.WebhookDeliveryAttempt Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of WebhookDeliveryAttempts filtered by the parameters provided.  Its query is similar to the GetWebhookDeliveryAttempts method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/WebhookDeliveryAttempts/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? incidentId = null,
			int? integrationId = null,
			int? incidentTimelineEventId = null,
			int? attemptNumber = null,
			DateTime? attemptedAt = null,
			int? httpStatusCode = null,
			bool? success = null,
			string payloadJson = null,
			string responseBody = null,
			string errorMessage = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
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
			if (attemptedAt.HasValue == true && attemptedAt.Value.Kind != DateTimeKind.Utc)
			{
				attemptedAt = attemptedAt.Value.ToUniversalTime();
			}

			IQueryable<Database.WebhookDeliveryAttempt> query = (from wda in _context.WebhookDeliveryAttempts select wda);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (incidentId.HasValue == true)
			{
				query = query.Where(wda => wda.incidentId == incidentId.Value);
			}
			if (integrationId.HasValue == true)
			{
				query = query.Where(wda => wda.integrationId == integrationId.Value);
			}
			if (incidentTimelineEventId.HasValue == true)
			{
				query = query.Where(wda => wda.incidentTimelineEventId == incidentTimelineEventId.Value);
			}
			if (attemptNumber.HasValue == true)
			{
				query = query.Where(wda => wda.attemptNumber == attemptNumber.Value);
			}
			if (attemptedAt.HasValue == true)
			{
				query = query.Where(wda => wda.attemptedAt == attemptedAt.Value);
			}
			if (httpStatusCode.HasValue == true)
			{
				query = query.Where(wda => wda.httpStatusCode == httpStatusCode.Value);
			}
			if (success.HasValue == true)
			{
				query = query.Where(wda => wda.success == success.Value);
			}
			if (payloadJson != null)
			{
				query = query.Where(wda => wda.payloadJson == payloadJson);
			}
			if (responseBody != null)
			{
				query = query.Where(wda => wda.responseBody == responseBody);
			}
			if (errorMessage != null)
			{
				query = query.Where(wda => wda.errorMessage == errorMessage);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(wda => wda.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(wda => wda.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(wda => wda.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(wda => wda.deleted == false);
				}
			}
			else
			{
				query = query.Where(wda => wda.active == true);
				query = query.Where(wda => wda.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Webhook Delivery Attempt, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.payloadJson.Contains(anyStringContains)
			       || x.responseBody.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			       || x.incident.incidentKey.Contains(anyStringContains)
			       || x.incident.title.Contains(anyStringContains)
			       || x.incident.description.Contains(anyStringContains)
			       || x.incident.sourcePayloadJson.Contains(anyStringContains)
			       || x.incidentTimelineEvent.detailsJson.Contains(anyStringContains)
			       || x.incidentTimelineEvent.notes.Contains(anyStringContains)
			       || x.incidentTimelineEvent.source.Contains(anyStringContains)
			       || x.integration.name.Contains(anyStringContains)
			       || x.integration.description.Contains(anyStringContains)
			       || x.integration.apiKeyHash.Contains(anyStringContains)
			       || x.integration.callbackWebhookUrl.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single WebhookDeliveryAttempt by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/WebhookDeliveryAttempt/{id}")]
		public async Task<IActionResult> GetWebhookDeliveryAttempt(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
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
				IQueryable<Database.WebhookDeliveryAttempt> query = (from wda in _context.WebhookDeliveryAttempts where
							(wda.id == id) &&
							(userIsAdmin == true || wda.deleted == false) &&
							(userIsWriter == true || wda.active == true)
					select wda);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.incident);
					query = query.Include(x => x.incidentTimelineEvent);
					query = query.Include(x => x.integration);
					query = query.AsSplitQuery();
				}

				Database.WebhookDeliveryAttempt materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Alerting.WebhookDeliveryAttempt Entity was read with Admin privilege." : "Alerting.WebhookDeliveryAttempt Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "WebhookDeliveryAttempt", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Alerting.WebhookDeliveryAttempt entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Alerting.WebhookDeliveryAttempt.   Entity was read with Admin privilege." : "Exception caught during entity read of Alerting.WebhookDeliveryAttempt.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing WebhookDeliveryAttempt record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/WebhookDeliveryAttempt/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutWebhookDeliveryAttempt(int id, [FromBody]Database.WebhookDeliveryAttempt.WebhookDeliveryAttemptDTO webhookDeliveryAttemptDTO, CancellationToken cancellationToken = default)
		{
			if (webhookDeliveryAttemptDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Alerting Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != webhookDeliveryAttemptDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
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


			IQueryable<Database.WebhookDeliveryAttempt> query = (from x in _context.WebhookDeliveryAttempts
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.WebhookDeliveryAttempt existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.WebhookDeliveryAttempt PUT", id.ToString(), new Exception("No Alerting.WebhookDeliveryAttempt entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (webhookDeliveryAttemptDTO.objectGuid == Guid.Empty)
            {
                webhookDeliveryAttemptDTO.objectGuid = existing.objectGuid;
            }
            else if (webhookDeliveryAttemptDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a WebhookDeliveryAttempt record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.WebhookDeliveryAttempt cloneOfExisting = (Database.WebhookDeliveryAttempt)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new WebhookDeliveryAttempt object using the data from the existing record, updated with what is in the DTO.
			//
			Database.WebhookDeliveryAttempt webhookDeliveryAttempt = (Database.WebhookDeliveryAttempt)_context.Entry(existing).GetDatabaseValues().ToObject();
			webhookDeliveryAttempt.ApplyDTO(webhookDeliveryAttemptDTO);
			//
			// The tenant guid for any WebhookDeliveryAttempt being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the WebhookDeliveryAttempt because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				webhookDeliveryAttempt.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (webhookDeliveryAttempt.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Alerting.WebhookDeliveryAttempt record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (webhookDeliveryAttempt.attemptedAt.Kind != DateTimeKind.Utc)
			{
				webhookDeliveryAttempt.attemptedAt = webhookDeliveryAttempt.attemptedAt.ToUniversalTime();
			}

			EntityEntry<Database.WebhookDeliveryAttempt> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(webhookDeliveryAttempt);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Alerting.WebhookDeliveryAttempt entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.WebhookDeliveryAttempt.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.WebhookDeliveryAttempt.CreateAnonymousWithFirstLevelSubObjects(webhookDeliveryAttempt)),
					null);


				return Ok(Database.WebhookDeliveryAttempt.CreateAnonymous(webhookDeliveryAttempt));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Alerting.WebhookDeliveryAttempt entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.WebhookDeliveryAttempt.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.WebhookDeliveryAttempt.CreateAnonymousWithFirstLevelSubObjects(webhookDeliveryAttempt)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new WebhookDeliveryAttempt record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/WebhookDeliveryAttempt", Name = "WebhookDeliveryAttempt")]
		public async Task<IActionResult> PostWebhookDeliveryAttempt([FromBody]Database.WebhookDeliveryAttempt.WebhookDeliveryAttemptDTO webhookDeliveryAttemptDTO, CancellationToken cancellationToken = default)
		{
			if (webhookDeliveryAttemptDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Alerting Administrator role needed to write to this table.
			//
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

			//
			// Create a new WebhookDeliveryAttempt object using the data from the DTO
			//
			Database.WebhookDeliveryAttempt webhookDeliveryAttempt = Database.WebhookDeliveryAttempt.FromDTO(webhookDeliveryAttemptDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				webhookDeliveryAttempt.tenantGuid = userTenantGuid;

				if (webhookDeliveryAttempt.attemptedAt.Kind != DateTimeKind.Utc)
				{
					webhookDeliveryAttempt.attemptedAt = webhookDeliveryAttempt.attemptedAt.ToUniversalTime();
				}

				webhookDeliveryAttempt.objectGuid = Guid.NewGuid();
				_context.WebhookDeliveryAttempts.Add(webhookDeliveryAttempt);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Alerting.WebhookDeliveryAttempt entity successfully created.",
					true,
					webhookDeliveryAttempt.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.WebhookDeliveryAttempt.CreateAnonymousWithFirstLevelSubObjects(webhookDeliveryAttempt)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Alerting.WebhookDeliveryAttempt entity creation failed.", false, webhookDeliveryAttempt.id.ToString(), "", JsonSerializer.Serialize(webhookDeliveryAttempt), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "WebhookDeliveryAttempt", webhookDeliveryAttempt.id, webhookDeliveryAttempt.id.ToString()));

			return CreatedAtRoute("WebhookDeliveryAttempt", new { id = webhookDeliveryAttempt.id }, Database.WebhookDeliveryAttempt.CreateAnonymousWithFirstLevelSubObjects(webhookDeliveryAttempt));
		}



        /// <summary>
        /// 
        /// This deletes a WebhookDeliveryAttempt record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/WebhookDeliveryAttempt/{id}")]
		[Route("api/WebhookDeliveryAttempt")]
		public async Task<IActionResult> DeleteWebhookDeliveryAttempt(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Alerting Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.WebhookDeliveryAttempt> query = (from x in _context.WebhookDeliveryAttempts
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.WebhookDeliveryAttempt webhookDeliveryAttempt = await query.FirstOrDefaultAsync(cancellationToken);

			if (webhookDeliveryAttempt == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.WebhookDeliveryAttempt DELETE", id.ToString(), new Exception("No Alerting.WebhookDeliveryAttempt entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.WebhookDeliveryAttempt cloneOfExisting = (Database.WebhookDeliveryAttempt)_context.Entry(webhookDeliveryAttempt).GetDatabaseValues().ToObject();


			try
			{
				webhookDeliveryAttempt.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Alerting.WebhookDeliveryAttempt entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.WebhookDeliveryAttempt.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.WebhookDeliveryAttempt.CreateAnonymousWithFirstLevelSubObjects(webhookDeliveryAttempt)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Alerting.WebhookDeliveryAttempt entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.WebhookDeliveryAttempt.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.WebhookDeliveryAttempt.CreateAnonymousWithFirstLevelSubObjects(webhookDeliveryAttempt)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of WebhookDeliveryAttempt records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/WebhookDeliveryAttempts/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? incidentId = null,
			int? integrationId = null,
			int? incidentTimelineEventId = null,
			int? attemptNumber = null,
			DateTime? attemptedAt = null,
			int? httpStatusCode = null,
			bool? success = null,
			string payloadJson = null,
			string responseBody = null,
			string errorMessage = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);


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
			if (attemptedAt.HasValue == true && attemptedAt.Value.Kind != DateTimeKind.Utc)
			{
				attemptedAt = attemptedAt.Value.ToUniversalTime();
			}

			IQueryable<Database.WebhookDeliveryAttempt> query = (from wda in _context.WebhookDeliveryAttempts select wda);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (incidentId.HasValue == true)
			{
				query = query.Where(wda => wda.incidentId == incidentId.Value);
			}
			if (integrationId.HasValue == true)
			{
				query = query.Where(wda => wda.integrationId == integrationId.Value);
			}
			if (incidentTimelineEventId.HasValue == true)
			{
				query = query.Where(wda => wda.incidentTimelineEventId == incidentTimelineEventId.Value);
			}
			if (attemptNumber.HasValue == true)
			{
				query = query.Where(wda => wda.attemptNumber == attemptNumber.Value);
			}
			if (attemptedAt.HasValue == true)
			{
				query = query.Where(wda => wda.attemptedAt == attemptedAt.Value);
			}
			if (httpStatusCode.HasValue == true)
			{
				query = query.Where(wda => wda.httpStatusCode == httpStatusCode.Value);
			}
			if (success.HasValue == true)
			{
				query = query.Where(wda => wda.success == success.Value);
			}
			if (string.IsNullOrEmpty(payloadJson) == false)
			{
				query = query.Where(wda => wda.payloadJson == payloadJson);
			}
			if (string.IsNullOrEmpty(responseBody) == false)
			{
				query = query.Where(wda => wda.responseBody == responseBody);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(wda => wda.errorMessage == errorMessage);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(wda => wda.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(wda => wda.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(wda => wda.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(wda => wda.deleted == false);
				}
			}
			else
			{
				query = query.Where(wda => wda.active == true);
				query = query.Where(wda => wda.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Webhook Delivery Attempt, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.payloadJson.Contains(anyStringContains)
			       || x.responseBody.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			       || x.incident.incidentKey.Contains(anyStringContains)
			       || x.incident.title.Contains(anyStringContains)
			       || x.incident.description.Contains(anyStringContains)
			       || x.incident.sourcePayloadJson.Contains(anyStringContains)
			       || x.incidentTimelineEvent.detailsJson.Contains(anyStringContains)
			       || x.incidentTimelineEvent.notes.Contains(anyStringContains)
			       || x.incidentTimelineEvent.source.Contains(anyStringContains)
			       || x.integration.name.Contains(anyStringContains)
			       || x.integration.description.Contains(anyStringContains)
			       || x.integration.apiKeyHash.Contains(anyStringContains)
			       || x.integration.callbackWebhookUrl.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.WebhookDeliveryAttempt.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/WebhookDeliveryAttempt/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
