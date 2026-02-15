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
using Foundation.Scheduler.Database;
using Foundation.ChangeHistory;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the NotificationSubscription entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the NotificationSubscription entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class NotificationSubscriptionsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object notificationSubscriptionPutSyncRoot = new object();
		static object notificationSubscriptionDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<NotificationSubscriptionsController> _logger;

		public NotificationSubscriptionsController(SchedulerContext context, ILogger<NotificationSubscriptionsController> logger) : base("Scheduler", "NotificationSubscription")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of NotificationSubscriptions filtered by the parameters provided.
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
		[Route("api/NotificationSubscriptions")]
		public async Task<IActionResult> GetNotificationSubscriptions(
			int? resourceId = null,
			int? contactId = null,
			int? notificationTypeId = null,
			int? triggerEvents = null,
			string recipientAddress = null,
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
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			IQueryable<Database.NotificationSubscription> query = (from ns in _context.NotificationSubscriptions select ns);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (resourceId.HasValue == true)
			{
				query = query.Where(ns => ns.resourceId == resourceId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(ns => ns.contactId == contactId.Value);
			}
			if (notificationTypeId.HasValue == true)
			{
				query = query.Where(ns => ns.notificationTypeId == notificationTypeId.Value);
			}
			if (triggerEvents.HasValue == true)
			{
				query = query.Where(ns => ns.triggerEvents == triggerEvents.Value);
			}
			if (string.IsNullOrEmpty(recipientAddress) == false)
			{
				query = query.Where(ns => ns.recipientAddress == recipientAddress);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ns => ns.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ns => ns.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ns => ns.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ns => ns.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ns => ns.deleted == false);
				}
			}
			else
			{
				query = query.Where(ns => ns.active == true);
				query = query.Where(ns => ns.deleted == false);
			}

			query = query.OrderBy(ns => ns.recipientAddress);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.contact);
				query = query.Include(x => x.notificationType);
				query = query.Include(x => x.resource);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Notification Subscription, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.recipientAddress.Contains(anyStringContains)
			       || (includeRelations == true && x.contact.firstName.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.middleName.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.lastName.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.title.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.company.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.email.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.phone.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.mobile.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.position.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.webSite.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.color.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.avatarMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.notificationType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.notificationType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.notificationType.color.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.name.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.description.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.color.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.avatarMimeType.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.NotificationSubscription> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.NotificationSubscription notificationSubscription in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(notificationSubscription, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.NotificationSubscription Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.NotificationSubscription Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of NotificationSubscriptions filtered by the parameters provided.  Its query is similar to the GetNotificationSubscriptions method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationSubscriptions/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? resourceId = null,
			int? contactId = null,
			int? notificationTypeId = null,
			int? triggerEvents = null,
			string recipientAddress = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			IQueryable<Database.NotificationSubscription> query = (from ns in _context.NotificationSubscriptions select ns);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (resourceId.HasValue == true)
			{
				query = query.Where(ns => ns.resourceId == resourceId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(ns => ns.contactId == contactId.Value);
			}
			if (notificationTypeId.HasValue == true)
			{
				query = query.Where(ns => ns.notificationTypeId == notificationTypeId.Value);
			}
			if (triggerEvents.HasValue == true)
			{
				query = query.Where(ns => ns.triggerEvents == triggerEvents.Value);
			}
			if (recipientAddress != null)
			{
				query = query.Where(ns => ns.recipientAddress == recipientAddress);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ns => ns.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ns => ns.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ns => ns.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ns => ns.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ns => ns.deleted == false);
				}
			}
			else
			{
				query = query.Where(ns => ns.active == true);
				query = query.Where(ns => ns.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Notification Subscription, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.recipientAddress.Contains(anyStringContains)
			       || x.contact.firstName.Contains(anyStringContains)
			       || x.contact.middleName.Contains(anyStringContains)
			       || x.contact.lastName.Contains(anyStringContains)
			       || x.contact.title.Contains(anyStringContains)
			       || x.contact.company.Contains(anyStringContains)
			       || x.contact.email.Contains(anyStringContains)
			       || x.contact.phone.Contains(anyStringContains)
			       || x.contact.mobile.Contains(anyStringContains)
			       || x.contact.position.Contains(anyStringContains)
			       || x.contact.webSite.Contains(anyStringContains)
			       || x.contact.notes.Contains(anyStringContains)
			       || x.contact.attributes.Contains(anyStringContains)
			       || x.contact.color.Contains(anyStringContains)
			       || x.contact.avatarFileName.Contains(anyStringContains)
			       || x.contact.avatarMimeType.Contains(anyStringContains)
			       || x.contact.externalId.Contains(anyStringContains)
			       || x.notificationType.name.Contains(anyStringContains)
			       || x.notificationType.description.Contains(anyStringContains)
			       || x.notificationType.color.Contains(anyStringContains)
			       || x.resource.name.Contains(anyStringContains)
			       || x.resource.description.Contains(anyStringContains)
			       || x.resource.notes.Contains(anyStringContains)
			       || x.resource.externalId.Contains(anyStringContains)
			       || x.resource.color.Contains(anyStringContains)
			       || x.resource.attributes.Contains(anyStringContains)
			       || x.resource.avatarFileName.Contains(anyStringContains)
			       || x.resource.avatarMimeType.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single NotificationSubscription by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationSubscription/{id}")]
		public async Task<IActionResult> GetNotificationSubscription(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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
				IQueryable<Database.NotificationSubscription> query = (from ns in _context.NotificationSubscriptions where
							(ns.id == id) &&
							(userIsAdmin == true || ns.deleted == false) &&
							(userIsWriter == true || ns.active == true)
					select ns);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.contact);
					query = query.Include(x => x.notificationType);
					query = query.Include(x => x.resource);
					query = query.AsSplitQuery();
				}

				Database.NotificationSubscription materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.NotificationSubscription Entity was read with Admin privilege." : "Scheduler.NotificationSubscription Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "NotificationSubscription", materialized.id, materialized.recipientAddress));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.NotificationSubscription entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.NotificationSubscription.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.NotificationSubscription.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing NotificationSubscription record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/NotificationSubscription/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutNotificationSubscription(int id, [FromBody]Database.NotificationSubscription.NotificationSubscriptionDTO notificationSubscriptionDTO, CancellationToken cancellationToken = default)
		{
			if (notificationSubscriptionDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != notificationSubscriptionDTO.id)
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


			IQueryable<Database.NotificationSubscription> query = (from x in _context.NotificationSubscriptions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.NotificationSubscription existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.NotificationSubscription PUT", id.ToString(), new Exception("No Scheduler.NotificationSubscription entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (notificationSubscriptionDTO.objectGuid == Guid.Empty)
            {
                notificationSubscriptionDTO.objectGuid = existing.objectGuid;
            }
            else if (notificationSubscriptionDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a NotificationSubscription record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.NotificationSubscription cloneOfExisting = (Database.NotificationSubscription)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new NotificationSubscription object using the data from the existing record, updated with what is in the DTO.
			//
			Database.NotificationSubscription notificationSubscription = (Database.NotificationSubscription)_context.Entry(existing).GetDatabaseValues().ToObject();
			notificationSubscription.ApplyDTO(notificationSubscriptionDTO);
			//
			// The tenant guid for any NotificationSubscription being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the NotificationSubscription because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				notificationSubscription.tenantGuid = existing.tenantGuid;
			}

			lock (notificationSubscriptionPutSyncRoot)
			{
				//
				// Validate the version number for the notificationSubscription being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != notificationSubscription.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "NotificationSubscription save attempt was made but save request was with version " + notificationSubscription.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The NotificationSubscription you are trying to update has already changed.  Please try your save again after reloading the NotificationSubscription.");
				}
				else
				{
					// Same record.  Increase version.
					notificationSubscription.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (notificationSubscription.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.NotificationSubscription record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (notificationSubscription.recipientAddress != null && notificationSubscription.recipientAddress.Length > 250)
				{
					notificationSubscription.recipientAddress = notificationSubscription.recipientAddress.Substring(0, 250);
				}

				try
				{
				    EntityEntry<Database.NotificationSubscription> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(notificationSubscription);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        NotificationSubscriptionChangeHistory notificationSubscriptionChangeHistory = new NotificationSubscriptionChangeHistory();
				        notificationSubscriptionChangeHistory.notificationSubscriptionId = notificationSubscription.id;
				        notificationSubscriptionChangeHistory.versionNumber = notificationSubscription.versionNumber;
				        notificationSubscriptionChangeHistory.timeStamp = DateTime.UtcNow;
				        notificationSubscriptionChangeHistory.userId = securityUser.id;
				        notificationSubscriptionChangeHistory.tenantGuid = userTenantGuid;
				        notificationSubscriptionChangeHistory.data = JsonSerializer.Serialize(Database.NotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(notificationSubscription));
				        _context.NotificationSubscriptionChangeHistories.Add(notificationSubscriptionChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.NotificationSubscription entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.NotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.NotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(notificationSubscription)),
						null);

				return Ok(Database.NotificationSubscription.CreateAnonymous(notificationSubscription));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.NotificationSubscription entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.NotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.NotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(notificationSubscription)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new NotificationSubscription record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationSubscription", Name = "NotificationSubscription")]
		public async Task<IActionResult> PostNotificationSubscription([FromBody]Database.NotificationSubscription.NotificationSubscriptionDTO notificationSubscriptionDTO, CancellationToken cancellationToken = default)
		{
			if (notificationSubscriptionDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
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
			// Create a new NotificationSubscription object using the data from the DTO
			//
			Database.NotificationSubscription notificationSubscription = Database.NotificationSubscription.FromDTO(notificationSubscriptionDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				notificationSubscription.tenantGuid = userTenantGuid;

				if (notificationSubscription.recipientAddress != null && notificationSubscription.recipientAddress.Length > 250)
				{
					notificationSubscription.recipientAddress = notificationSubscription.recipientAddress.Substring(0, 250);
				}

				notificationSubscription.objectGuid = Guid.NewGuid();
				notificationSubscription.versionNumber = 1;

				_context.NotificationSubscriptions.Add(notificationSubscription);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the notificationSubscription object so that no further changes will be written to the database
				    //
				    _context.Entry(notificationSubscription).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					notificationSubscription.NotificationSubscriptionChangeHistories = null;
					notificationSubscription.contact = null;
					notificationSubscription.notificationType = null;
					notificationSubscription.resource = null;


				    NotificationSubscriptionChangeHistory notificationSubscriptionChangeHistory = new NotificationSubscriptionChangeHistory();
				    notificationSubscriptionChangeHistory.notificationSubscriptionId = notificationSubscription.id;
				    notificationSubscriptionChangeHistory.versionNumber = notificationSubscription.versionNumber;
				    notificationSubscriptionChangeHistory.timeStamp = DateTime.UtcNow;
				    notificationSubscriptionChangeHistory.userId = securityUser.id;
				    notificationSubscriptionChangeHistory.tenantGuid = userTenantGuid;
				    notificationSubscriptionChangeHistory.data = JsonSerializer.Serialize(Database.NotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(notificationSubscription));
				    _context.NotificationSubscriptionChangeHistories.Add(notificationSubscriptionChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.NotificationSubscription entity successfully created.",
						true,
						notificationSubscription. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.NotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(notificationSubscription)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.NotificationSubscription entity creation failed.", false, notificationSubscription.id.ToString(), "", JsonSerializer.Serialize(Database.NotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(notificationSubscription)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "NotificationSubscription", notificationSubscription.id, notificationSubscription.recipientAddress));

			return CreatedAtRoute("NotificationSubscription", new { id = notificationSubscription.id }, Database.NotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(notificationSubscription));
		}



        /// <summary>
        /// 
        /// This rolls a NotificationSubscription entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationSubscription/Rollback/{id}")]
		[Route("api/NotificationSubscription/Rollback")]
		public async Task<IActionResult> RollbackToNotificationSubscriptionVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.NotificationSubscription> query = (from x in _context.NotificationSubscriptions
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this NotificationSubscription concurrently
			//
			lock (notificationSubscriptionPutSyncRoot)
			{
				
				Database.NotificationSubscription notificationSubscription = query.FirstOrDefault();
				
				if (notificationSubscription == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.NotificationSubscription rollback", id.ToString(), new Exception("No Scheduler.NotificationSubscription entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the NotificationSubscription current state so we can log it.
				//
				Database.NotificationSubscription cloneOfExisting = (Database.NotificationSubscription)_context.Entry(notificationSubscription).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.NotificationSubscriptionChangeHistories = null;
				cloneOfExisting.contact = null;
				cloneOfExisting.notificationType = null;
				cloneOfExisting.resource = null;

				if (versionNumber >= notificationSubscription.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.NotificationSubscription rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.NotificationSubscription rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				NotificationSubscriptionChangeHistory notificationSubscriptionChangeHistory = (from x in _context.NotificationSubscriptionChangeHistories
				                                               where
				                                               x.notificationSubscriptionId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (notificationSubscriptionChangeHistory != null)
				{
				    Database.NotificationSubscription oldNotificationSubscription = JsonSerializer.Deserialize<Database.NotificationSubscription>(notificationSubscriptionChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    notificationSubscription.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    notificationSubscription.resourceId = oldNotificationSubscription.resourceId;
				    notificationSubscription.contactId = oldNotificationSubscription.contactId;
				    notificationSubscription.notificationTypeId = oldNotificationSubscription.notificationTypeId;
				    notificationSubscription.triggerEvents = oldNotificationSubscription.triggerEvents;
				    notificationSubscription.recipientAddress = oldNotificationSubscription.recipientAddress;
				    notificationSubscription.objectGuid = oldNotificationSubscription.objectGuid;
				    notificationSubscription.active = oldNotificationSubscription.active;
				    notificationSubscription.deleted = oldNotificationSubscription.deleted;

				    string serializedNotificationSubscription = JsonSerializer.Serialize(notificationSubscription);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        NotificationSubscriptionChangeHistory newNotificationSubscriptionChangeHistory = new NotificationSubscriptionChangeHistory();
				        newNotificationSubscriptionChangeHistory.notificationSubscriptionId = notificationSubscription.id;
				        newNotificationSubscriptionChangeHistory.versionNumber = notificationSubscription.versionNumber;
				        newNotificationSubscriptionChangeHistory.timeStamp = DateTime.UtcNow;
				        newNotificationSubscriptionChangeHistory.userId = securityUser.id;
				        newNotificationSubscriptionChangeHistory.tenantGuid = userTenantGuid;
				        newNotificationSubscriptionChangeHistory.data = JsonSerializer.Serialize(Database.NotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(notificationSubscription));
				        _context.NotificationSubscriptionChangeHistories.Add(newNotificationSubscriptionChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.NotificationSubscription rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.NotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.NotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(notificationSubscription)),
						null);


				    return Ok(Database.NotificationSubscription.CreateAnonymous(notificationSubscription));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.NotificationSubscription rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.NotificationSubscription rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a NotificationSubscription.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the NotificationSubscription</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationSubscription/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetNotificationSubscriptionChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.NotificationSubscription notificationSubscription = await _context.NotificationSubscriptions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (notificationSubscription == null)
			{
				return NotFound();
			}

			try
			{
				notificationSubscription.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.NotificationSubscription> versionInfo = await notificationSubscription.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a NotificationSubscription.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the NotificationSubscription</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationSubscription/{id}/AuditHistory")]
		public async Task<IActionResult> GetNotificationSubscriptionAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.NotificationSubscription notificationSubscription = await _context.NotificationSubscriptions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (notificationSubscription == null)
			{
				return NotFound();
			}

			try
			{
				notificationSubscription.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.NotificationSubscription>> versions = await notificationSubscription.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a NotificationSubscription.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the NotificationSubscription</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The NotificationSubscription object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationSubscription/{id}/Version/{version}")]
		public async Task<IActionResult> GetNotificationSubscriptionVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.NotificationSubscription notificationSubscription = await _context.NotificationSubscriptions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (notificationSubscription == null)
			{
				return NotFound();
			}

			try
			{
				notificationSubscription.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.NotificationSubscription> versionInfo = await notificationSubscription.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a NotificationSubscription at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the NotificationSubscription</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The NotificationSubscription object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationSubscription/{id}/StateAtTime")]
		public async Task<IActionResult> GetNotificationSubscriptionStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.NotificationSubscription notificationSubscription = await _context.NotificationSubscriptions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (notificationSubscription == null)
			{
				return NotFound();
			}

			try
			{
				notificationSubscription.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.NotificationSubscription> versionInfo = await notificationSubscription.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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

        /// <summary>
        /// 
        /// This deletes a NotificationSubscription record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationSubscription/{id}")]
		[Route("api/NotificationSubscription")]
		public async Task<IActionResult> DeleteNotificationSubscription(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
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

			IQueryable<Database.NotificationSubscription> query = (from x in _context.NotificationSubscriptions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.NotificationSubscription notificationSubscription = await query.FirstOrDefaultAsync(cancellationToken);

			if (notificationSubscription == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.NotificationSubscription DELETE", id.ToString(), new Exception("No Scheduler.NotificationSubscription entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.NotificationSubscription cloneOfExisting = (Database.NotificationSubscription)_context.Entry(notificationSubscription).GetDatabaseValues().ToObject();


			lock (notificationSubscriptionDeleteSyncRoot)
			{
			    try
			    {
			        notificationSubscription.deleted = true;
			        notificationSubscription.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        NotificationSubscriptionChangeHistory notificationSubscriptionChangeHistory = new NotificationSubscriptionChangeHistory();
			        notificationSubscriptionChangeHistory.notificationSubscriptionId = notificationSubscription.id;
			        notificationSubscriptionChangeHistory.versionNumber = notificationSubscription.versionNumber;
			        notificationSubscriptionChangeHistory.timeStamp = DateTime.UtcNow;
			        notificationSubscriptionChangeHistory.userId = securityUser.id;
			        notificationSubscriptionChangeHistory.tenantGuid = userTenantGuid;
			        notificationSubscriptionChangeHistory.data = JsonSerializer.Serialize(Database.NotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(notificationSubscription));
			        _context.NotificationSubscriptionChangeHistories.Add(notificationSubscriptionChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.NotificationSubscription entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.NotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.NotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(notificationSubscription)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.NotificationSubscription entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.NotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.NotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(notificationSubscription)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of NotificationSubscription records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/NotificationSubscriptions/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? resourceId = null,
			int? contactId = null,
			int? notificationTypeId = null,
			int? triggerEvents = null,
			string recipientAddress = null,
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
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			IQueryable<Database.NotificationSubscription> query = (from ns in _context.NotificationSubscriptions select ns);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (resourceId.HasValue == true)
			{
				query = query.Where(ns => ns.resourceId == resourceId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(ns => ns.contactId == contactId.Value);
			}
			if (notificationTypeId.HasValue == true)
			{
				query = query.Where(ns => ns.notificationTypeId == notificationTypeId.Value);
			}
			if (triggerEvents.HasValue == true)
			{
				query = query.Where(ns => ns.triggerEvents == triggerEvents.Value);
			}
			if (string.IsNullOrEmpty(recipientAddress) == false)
			{
				query = query.Where(ns => ns.recipientAddress == recipientAddress);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ns => ns.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ns => ns.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ns => ns.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ns => ns.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ns => ns.deleted == false);
				}
			}
			else
			{
				query = query.Where(ns => ns.active == true);
				query = query.Where(ns => ns.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Notification Subscription, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.recipientAddress.Contains(anyStringContains)
			       || x.contact.firstName.Contains(anyStringContains)
			       || x.contact.middleName.Contains(anyStringContains)
			       || x.contact.lastName.Contains(anyStringContains)
			       || x.contact.title.Contains(anyStringContains)
			       || x.contact.company.Contains(anyStringContains)
			       || x.contact.email.Contains(anyStringContains)
			       || x.contact.phone.Contains(anyStringContains)
			       || x.contact.mobile.Contains(anyStringContains)
			       || x.contact.position.Contains(anyStringContains)
			       || x.contact.webSite.Contains(anyStringContains)
			       || x.contact.notes.Contains(anyStringContains)
			       || x.contact.attributes.Contains(anyStringContains)
			       || x.contact.color.Contains(anyStringContains)
			       || x.contact.avatarFileName.Contains(anyStringContains)
			       || x.contact.avatarMimeType.Contains(anyStringContains)
			       || x.contact.externalId.Contains(anyStringContains)
			       || x.notificationType.name.Contains(anyStringContains)
			       || x.notificationType.description.Contains(anyStringContains)
			       || x.notificationType.color.Contains(anyStringContains)
			       || x.resource.name.Contains(anyStringContains)
			       || x.resource.description.Contains(anyStringContains)
			       || x.resource.notes.Contains(anyStringContains)
			       || x.resource.externalId.Contains(anyStringContains)
			       || x.resource.color.Contains(anyStringContains)
			       || x.resource.attributes.Contains(anyStringContains)
			       || x.resource.avatarFileName.Contains(anyStringContains)
			       || x.resource.avatarMimeType.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.recipientAddress);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.NotificationSubscription.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/NotificationSubscription/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
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
