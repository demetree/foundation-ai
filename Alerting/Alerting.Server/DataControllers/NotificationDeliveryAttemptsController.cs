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
    /// This auto generated class provides the basic CRUD operations for the NotificationDeliveryAttempt entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the NotificationDeliveryAttempt entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class NotificationDeliveryAttemptsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private AlertingContext _context;

		private ILogger<NotificationDeliveryAttemptsController> _logger;

		public NotificationDeliveryAttemptsController(AlertingContext context, ILogger<NotificationDeliveryAttemptsController> logger) : base("Alerting", "NotificationDeliveryAttempt")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of NotificationDeliveryAttempts filtered by the parameters provided.
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
		[Route("api/NotificationDeliveryAttempts")]
		public async Task<IActionResult> GetNotificationDeliveryAttempts(
			int? incidentNotificationId = null,
			int? notificationChannelTypeId = null,
			int? attemptNumber = null,
			DateTime? attemptedAt = null,
			string status = null,
			string errorMessage = null,
			string response = null,
			string recipientAddress = null,
			string subject = null,
			string bodyContent = null,
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

			IQueryable<Database.NotificationDeliveryAttempt> query = (from nda in _context.NotificationDeliveryAttempts select nda);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (incidentNotificationId.HasValue == true)
			{
				query = query.Where(nda => nda.incidentNotificationId == incidentNotificationId.Value);
			}
			if (notificationChannelTypeId.HasValue == true)
			{
				query = query.Where(nda => nda.notificationChannelTypeId == notificationChannelTypeId.Value);
			}
			if (attemptNumber.HasValue == true)
			{
				query = query.Where(nda => nda.attemptNumber == attemptNumber.Value);
			}
			if (attemptedAt.HasValue == true)
			{
				query = query.Where(nda => nda.attemptedAt == attemptedAt.Value);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(nda => nda.status == status);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(nda => nda.errorMessage == errorMessage);
			}
			if (string.IsNullOrEmpty(response) == false)
			{
				query = query.Where(nda => nda.response == response);
			}
			if (string.IsNullOrEmpty(recipientAddress) == false)
			{
				query = query.Where(nda => nda.recipientAddress == recipientAddress);
			}
			if (string.IsNullOrEmpty(subject) == false)
			{
				query = query.Where(nda => nda.subject == subject);
			}
			if (string.IsNullOrEmpty(bodyContent) == false)
			{
				query = query.Where(nda => nda.bodyContent == bodyContent);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(nda => nda.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(nda => nda.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(nda => nda.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(nda => nda.deleted == false);
				}
			}
			else
			{
				query = query.Where(nda => nda.active == true);
				query = query.Where(nda => nda.deleted == false);
			}

			query = query.OrderBy(nda => nda.status).ThenBy(nda => nda.recipientAddress).ThenBy(nda => nda.subject);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.incidentNotification);
				query = query.Include(x => x.notificationChannelType);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Notification Delivery Attempt, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.status.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			       || x.response.Contains(anyStringContains)
			       || x.recipientAddress.Contains(anyStringContains)
			       || x.subject.Contains(anyStringContains)
			       || x.bodyContent.Contains(anyStringContains)
			       || (includeRelations == true && x.notificationChannelType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.notificationChannelType.description.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.NotificationDeliveryAttempt> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.NotificationDeliveryAttempt notificationDeliveryAttempt in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(notificationDeliveryAttempt, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Alerting.NotificationDeliveryAttempt Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Alerting.NotificationDeliveryAttempt Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of NotificationDeliveryAttempts filtered by the parameters provided.  Its query is similar to the GetNotificationDeliveryAttempts method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationDeliveryAttempts/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? incidentNotificationId = null,
			int? notificationChannelTypeId = null,
			int? attemptNumber = null,
			DateTime? attemptedAt = null,
			string status = null,
			string errorMessage = null,
			string response = null,
			string recipientAddress = null,
			string subject = null,
			string bodyContent = null,
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

			IQueryable<Database.NotificationDeliveryAttempt> query = (from nda in _context.NotificationDeliveryAttempts select nda);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (incidentNotificationId.HasValue == true)
			{
				query = query.Where(nda => nda.incidentNotificationId == incidentNotificationId.Value);
			}
			if (notificationChannelTypeId.HasValue == true)
			{
				query = query.Where(nda => nda.notificationChannelTypeId == notificationChannelTypeId.Value);
			}
			if (attemptNumber.HasValue == true)
			{
				query = query.Where(nda => nda.attemptNumber == attemptNumber.Value);
			}
			if (attemptedAt.HasValue == true)
			{
				query = query.Where(nda => nda.attemptedAt == attemptedAt.Value);
			}
			if (status != null)
			{
				query = query.Where(nda => nda.status == status);
			}
			if (errorMessage != null)
			{
				query = query.Where(nda => nda.errorMessage == errorMessage);
			}
			if (response != null)
			{
				query = query.Where(nda => nda.response == response);
			}
			if (recipientAddress != null)
			{
				query = query.Where(nda => nda.recipientAddress == recipientAddress);
			}
			if (subject != null)
			{
				query = query.Where(nda => nda.subject == subject);
			}
			if (bodyContent != null)
			{
				query = query.Where(nda => nda.bodyContent == bodyContent);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(nda => nda.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(nda => nda.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(nda => nda.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(nda => nda.deleted == false);
				}
			}
			else
			{
				query = query.Where(nda => nda.active == true);
				query = query.Where(nda => nda.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Notification Delivery Attempt, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.status.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			       || x.response.Contains(anyStringContains)
			       || x.recipientAddress.Contains(anyStringContains)
			       || x.subject.Contains(anyStringContains)
			       || x.bodyContent.Contains(anyStringContains)
			       || x.notificationChannelType.name.Contains(anyStringContains)
			       || x.notificationChannelType.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single NotificationDeliveryAttempt by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationDeliveryAttempt/{id}")]
		public async Task<IActionResult> GetNotificationDeliveryAttempt(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.NotificationDeliveryAttempt> query = (from nda in _context.NotificationDeliveryAttempts where
							(nda.id == id) &&
							(userIsAdmin == true || nda.deleted == false) &&
							(userIsWriter == true || nda.active == true)
					select nda);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.incidentNotification);
					query = query.Include(x => x.notificationChannelType);
					query = query.AsSplitQuery();
				}

				Database.NotificationDeliveryAttempt materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Alerting.NotificationDeliveryAttempt Entity was read with Admin privilege." : "Alerting.NotificationDeliveryAttempt Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "NotificationDeliveryAttempt", materialized.id, materialized.status));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Alerting.NotificationDeliveryAttempt entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Alerting.NotificationDeliveryAttempt.   Entity was read with Admin privilege." : "Exception caught during entity read of Alerting.NotificationDeliveryAttempt.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing NotificationDeliveryAttempt record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/NotificationDeliveryAttempt/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutNotificationDeliveryAttempt(int id, [FromBody]Database.NotificationDeliveryAttempt.NotificationDeliveryAttemptDTO notificationDeliveryAttemptDTO, CancellationToken cancellationToken = default)
		{
			if (notificationDeliveryAttemptDTO == null)
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



			if (id != notificationDeliveryAttemptDTO.id)
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


			IQueryable<Database.NotificationDeliveryAttempt> query = (from x in _context.NotificationDeliveryAttempts
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.NotificationDeliveryAttempt existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.NotificationDeliveryAttempt PUT", id.ToString(), new Exception("No Alerting.NotificationDeliveryAttempt entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (notificationDeliveryAttemptDTO.objectGuid == Guid.Empty)
            {
                notificationDeliveryAttemptDTO.objectGuid = existing.objectGuid;
            }
            else if (notificationDeliveryAttemptDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a NotificationDeliveryAttempt record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.NotificationDeliveryAttempt cloneOfExisting = (Database.NotificationDeliveryAttempt)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new NotificationDeliveryAttempt object using the data from the existing record, updated with what is in the DTO.
			//
			Database.NotificationDeliveryAttempt notificationDeliveryAttempt = (Database.NotificationDeliveryAttempt)_context.Entry(existing).GetDatabaseValues().ToObject();
			notificationDeliveryAttempt.ApplyDTO(notificationDeliveryAttemptDTO);
			//
			// The tenant guid for any NotificationDeliveryAttempt being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the NotificationDeliveryAttempt because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				notificationDeliveryAttempt.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (notificationDeliveryAttempt.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Alerting.NotificationDeliveryAttempt record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (notificationDeliveryAttempt.attemptedAt.Kind != DateTimeKind.Utc)
			{
				notificationDeliveryAttempt.attemptedAt = notificationDeliveryAttempt.attemptedAt.ToUniversalTime();
			}

			if (notificationDeliveryAttempt.status != null && notificationDeliveryAttempt.status.Length > 50)
			{
				notificationDeliveryAttempt.status = notificationDeliveryAttempt.status.Substring(0, 50);
			}

			if (notificationDeliveryAttempt.recipientAddress != null && notificationDeliveryAttempt.recipientAddress.Length > 250)
			{
				notificationDeliveryAttempt.recipientAddress = notificationDeliveryAttempt.recipientAddress.Substring(0, 250);
			}

			if (notificationDeliveryAttempt.subject != null && notificationDeliveryAttempt.subject.Length > 500)
			{
				notificationDeliveryAttempt.subject = notificationDeliveryAttempt.subject.Substring(0, 500);
			}

			EntityEntry<Database.NotificationDeliveryAttempt> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(notificationDeliveryAttempt);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Alerting.NotificationDeliveryAttempt entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.NotificationDeliveryAttempt.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.NotificationDeliveryAttempt.CreateAnonymousWithFirstLevelSubObjects(notificationDeliveryAttempt)),
					null);


				return Ok(Database.NotificationDeliveryAttempt.CreateAnonymous(notificationDeliveryAttempt));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Alerting.NotificationDeliveryAttempt entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.NotificationDeliveryAttempt.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.NotificationDeliveryAttempt.CreateAnonymousWithFirstLevelSubObjects(notificationDeliveryAttempt)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new NotificationDeliveryAttempt record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationDeliveryAttempt", Name = "NotificationDeliveryAttempt")]
		public async Task<IActionResult> PostNotificationDeliveryAttempt([FromBody]Database.NotificationDeliveryAttempt.NotificationDeliveryAttemptDTO notificationDeliveryAttemptDTO, CancellationToken cancellationToken = default)
		{
			if (notificationDeliveryAttemptDTO == null)
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
			// Create a new NotificationDeliveryAttempt object using the data from the DTO
			//
			Database.NotificationDeliveryAttempt notificationDeliveryAttempt = Database.NotificationDeliveryAttempt.FromDTO(notificationDeliveryAttemptDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				notificationDeliveryAttempt.tenantGuid = userTenantGuid;

				if (notificationDeliveryAttempt.attemptedAt.Kind != DateTimeKind.Utc)
				{
					notificationDeliveryAttempt.attemptedAt = notificationDeliveryAttempt.attemptedAt.ToUniversalTime();
				}

				if (notificationDeliveryAttempt.status != null && notificationDeliveryAttempt.status.Length > 50)
				{
					notificationDeliveryAttempt.status = notificationDeliveryAttempt.status.Substring(0, 50);
				}

				if (notificationDeliveryAttempt.recipientAddress != null && notificationDeliveryAttempt.recipientAddress.Length > 250)
				{
					notificationDeliveryAttempt.recipientAddress = notificationDeliveryAttempt.recipientAddress.Substring(0, 250);
				}

				if (notificationDeliveryAttempt.subject != null && notificationDeliveryAttempt.subject.Length > 500)
				{
					notificationDeliveryAttempt.subject = notificationDeliveryAttempt.subject.Substring(0, 500);
				}

				notificationDeliveryAttempt.objectGuid = Guid.NewGuid();
				_context.NotificationDeliveryAttempts.Add(notificationDeliveryAttempt);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Alerting.NotificationDeliveryAttempt entity successfully created.",
					true,
					notificationDeliveryAttempt.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.NotificationDeliveryAttempt.CreateAnonymousWithFirstLevelSubObjects(notificationDeliveryAttempt)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Alerting.NotificationDeliveryAttempt entity creation failed.", false, notificationDeliveryAttempt.id.ToString(), "", JsonSerializer.Serialize(notificationDeliveryAttempt), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "NotificationDeliveryAttempt", notificationDeliveryAttempt.id, notificationDeliveryAttempt.status));

			return CreatedAtRoute("NotificationDeliveryAttempt", new { id = notificationDeliveryAttempt.id }, Database.NotificationDeliveryAttempt.CreateAnonymousWithFirstLevelSubObjects(notificationDeliveryAttempt));
		}



        /// <summary>
        /// 
        /// This deletes a NotificationDeliveryAttempt record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationDeliveryAttempt/{id}")]
		[Route("api/NotificationDeliveryAttempt")]
		public async Task<IActionResult> DeleteNotificationDeliveryAttempt(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.NotificationDeliveryAttempt> query = (from x in _context.NotificationDeliveryAttempts
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.NotificationDeliveryAttempt notificationDeliveryAttempt = await query.FirstOrDefaultAsync(cancellationToken);

			if (notificationDeliveryAttempt == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.NotificationDeliveryAttempt DELETE", id.ToString(), new Exception("No Alerting.NotificationDeliveryAttempt entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.NotificationDeliveryAttempt cloneOfExisting = (Database.NotificationDeliveryAttempt)_context.Entry(notificationDeliveryAttempt).GetDatabaseValues().ToObject();


			try
			{
				notificationDeliveryAttempt.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Alerting.NotificationDeliveryAttempt entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.NotificationDeliveryAttempt.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.NotificationDeliveryAttempt.CreateAnonymousWithFirstLevelSubObjects(notificationDeliveryAttempt)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Alerting.NotificationDeliveryAttempt entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.NotificationDeliveryAttempt.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.NotificationDeliveryAttempt.CreateAnonymousWithFirstLevelSubObjects(notificationDeliveryAttempt)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of NotificationDeliveryAttempt records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/NotificationDeliveryAttempts/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? incidentNotificationId = null,
			int? notificationChannelTypeId = null,
			int? attemptNumber = null,
			DateTime? attemptedAt = null,
			string status = null,
			string errorMessage = null,
			string response = null,
			string recipientAddress = null,
			string subject = null,
			string bodyContent = null,
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

			IQueryable<Database.NotificationDeliveryAttempt> query = (from nda in _context.NotificationDeliveryAttempts select nda);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (incidentNotificationId.HasValue == true)
			{
				query = query.Where(nda => nda.incidentNotificationId == incidentNotificationId.Value);
			}
			if (notificationChannelTypeId.HasValue == true)
			{
				query = query.Where(nda => nda.notificationChannelTypeId == notificationChannelTypeId.Value);
			}
			if (attemptNumber.HasValue == true)
			{
				query = query.Where(nda => nda.attemptNumber == attemptNumber.Value);
			}
			if (attemptedAt.HasValue == true)
			{
				query = query.Where(nda => nda.attemptedAt == attemptedAt.Value);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(nda => nda.status == status);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(nda => nda.errorMessage == errorMessage);
			}
			if (string.IsNullOrEmpty(response) == false)
			{
				query = query.Where(nda => nda.response == response);
			}
			if (string.IsNullOrEmpty(recipientAddress) == false)
			{
				query = query.Where(nda => nda.recipientAddress == recipientAddress);
			}
			if (string.IsNullOrEmpty(subject) == false)
			{
				query = query.Where(nda => nda.subject == subject);
			}
			if (string.IsNullOrEmpty(bodyContent) == false)
			{
				query = query.Where(nda => nda.bodyContent == bodyContent);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(nda => nda.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(nda => nda.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(nda => nda.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(nda => nda.deleted == false);
				}
			}
			else
			{
				query = query.Where(nda => nda.active == true);
				query = query.Where(nda => nda.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Notification Delivery Attempt, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.status.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			       || x.response.Contains(anyStringContains)
			       || x.recipientAddress.Contains(anyStringContains)
			       || x.subject.Contains(anyStringContains)
			       || x.bodyContent.Contains(anyStringContains)
			       || x.notificationChannelType.name.Contains(anyStringContains)
			       || x.notificationChannelType.description.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.status).ThenBy(x => x.recipientAddress).ThenBy(x => x.subject);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.NotificationDeliveryAttempt.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/NotificationDeliveryAttempt/CreateAuditEvent")]
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
