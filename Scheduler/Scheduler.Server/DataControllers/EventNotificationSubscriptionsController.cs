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
    /// This auto generated class provides the basic CRUD operations for the EventNotificationSubscription entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the EventNotificationSubscription entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class EventNotificationSubscriptionsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object eventNotificationSubscriptionPutSyncRoot = new object();
		static object eventNotificationSubscriptionDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<EventNotificationSubscriptionsController> _logger;

		public EventNotificationSubscriptionsController(SchedulerContext context, ILogger<EventNotificationSubscriptionsController> logger) : base("Scheduler", "EventNotificationSubscription")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of EventNotificationSubscriptions filtered by the parameters provided.
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
		[Route("api/EventNotificationSubscriptions")]
		public async Task<IActionResult> GetEventNotificationSubscriptions(
			int? resourceId = null,
			int? contactId = null,
			int? eventNotificationTypeId = null,
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

			IQueryable<Database.EventNotificationSubscription> query = (from ens in _context.EventNotificationSubscriptions select ens);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (resourceId.HasValue == true)
			{
				query = query.Where(ens => ens.resourceId == resourceId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(ens => ens.contactId == contactId.Value);
			}
			if (eventNotificationTypeId.HasValue == true)
			{
				query = query.Where(ens => ens.eventNotificationTypeId == eventNotificationTypeId.Value);
			}
			if (triggerEvents.HasValue == true)
			{
				query = query.Where(ens => ens.triggerEvents == triggerEvents.Value);
			}
			if (string.IsNullOrEmpty(recipientAddress) == false)
			{
				query = query.Where(ens => ens.recipientAddress == recipientAddress);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ens => ens.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ens => ens.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ens => ens.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ens => ens.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ens => ens.deleted == false);
				}
			}
			else
			{
				query = query.Where(ens => ens.active == true);
				query = query.Where(ens => ens.deleted == false);
			}

			query = query.OrderBy(ens => ens.recipientAddress);


			//
			// Add the any string contains parameter to span all the string fields on the Event Notification Subscription, or on an any of the string fields on its immediate relations
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
			       || (includeRelations == true && x.eventNotificationType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.eventNotificationType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.eventNotificationType.color.Contains(anyStringContains))
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

			if (includeRelations == true)
			{
				query = query.Include(x => x.contact);
				query = query.Include(x => x.eventNotificationType);
				query = query.Include(x => x.resource);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.EventNotificationSubscription> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.EventNotificationSubscription eventNotificationSubscription in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(eventNotificationSubscription, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.EventNotificationSubscription Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.EventNotificationSubscription Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of EventNotificationSubscriptions filtered by the parameters provided.  Its query is similar to the GetEventNotificationSubscriptions method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventNotificationSubscriptions/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? resourceId = null,
			int? contactId = null,
			int? eventNotificationTypeId = null,
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


			IQueryable<Database.EventNotificationSubscription> query = (from ens in _context.EventNotificationSubscriptions select ens);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (resourceId.HasValue == true)
			{
				query = query.Where(ens => ens.resourceId == resourceId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(ens => ens.contactId == contactId.Value);
			}
			if (eventNotificationTypeId.HasValue == true)
			{
				query = query.Where(ens => ens.eventNotificationTypeId == eventNotificationTypeId.Value);
			}
			if (triggerEvents.HasValue == true)
			{
				query = query.Where(ens => ens.triggerEvents == triggerEvents.Value);
			}
			if (recipientAddress != null)
			{
				query = query.Where(ens => ens.recipientAddress == recipientAddress);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ens => ens.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ens => ens.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ens => ens.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ens => ens.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ens => ens.deleted == false);
				}
			}
			else
			{
				query = query.Where(ens => ens.active == true);
				query = query.Where(ens => ens.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Event Notification Subscription, or on an any of the string fields on its immediate relations
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
			       || x.eventNotificationType.name.Contains(anyStringContains)
			       || x.eventNotificationType.description.Contains(anyStringContains)
			       || x.eventNotificationType.color.Contains(anyStringContains)
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
        /// This gets a single EventNotificationSubscription by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventNotificationSubscription/{id}")]
		public async Task<IActionResult> GetEventNotificationSubscription(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.EventNotificationSubscription> query = (from ens in _context.EventNotificationSubscriptions where
							(ens.id == id) &&
							(userIsAdmin == true || ens.deleted == false) &&
							(userIsWriter == true || ens.active == true)
					select ens);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.contact);
					query = query.Include(x => x.eventNotificationType);
					query = query.Include(x => x.resource);
					query = query.AsSplitQuery();
				}

				Database.EventNotificationSubscription materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.EventNotificationSubscription Entity was read with Admin privilege." : "Scheduler.EventNotificationSubscription Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "EventNotificationSubscription", materialized.id, materialized.recipientAddress));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.EventNotificationSubscription entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.EventNotificationSubscription.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.EventNotificationSubscription.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing EventNotificationSubscription record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/EventNotificationSubscription/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutEventNotificationSubscription(int id, [FromBody]Database.EventNotificationSubscription.EventNotificationSubscriptionDTO eventNotificationSubscriptionDTO, CancellationToken cancellationToken = default)
		{
			if (eventNotificationSubscriptionDTO == null)
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



			if (id != eventNotificationSubscriptionDTO.id)
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


			IQueryable<Database.EventNotificationSubscription> query = (from x in _context.EventNotificationSubscriptions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.EventNotificationSubscription existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.EventNotificationSubscription PUT", id.ToString(), new Exception("No Scheduler.EventNotificationSubscription entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (eventNotificationSubscriptionDTO.objectGuid == Guid.Empty)
            {
                eventNotificationSubscriptionDTO.objectGuid = existing.objectGuid;
            }
            else if (eventNotificationSubscriptionDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a EventNotificationSubscription record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.EventNotificationSubscription cloneOfExisting = (Database.EventNotificationSubscription)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new EventNotificationSubscription object using the data from the existing record, updated with what is in the DTO.
			//
			Database.EventNotificationSubscription eventNotificationSubscription = (Database.EventNotificationSubscription)_context.Entry(existing).GetDatabaseValues().ToObject();
			eventNotificationSubscription.ApplyDTO(eventNotificationSubscriptionDTO);
			//
			// The tenant guid for any EventNotificationSubscription being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the EventNotificationSubscription because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				eventNotificationSubscription.tenantGuid = existing.tenantGuid;
			}

			lock (eventNotificationSubscriptionPutSyncRoot)
			{
				//
				// Validate the version number for the eventNotificationSubscription being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != eventNotificationSubscription.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "EventNotificationSubscription save attempt was made but save request was with version " + eventNotificationSubscription.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The EventNotificationSubscription you are trying to update has already changed.  Please try your save again after reloading the EventNotificationSubscription.");
				}
				else
				{
					// Same record.  Increase version.
					eventNotificationSubscription.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (eventNotificationSubscription.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.EventNotificationSubscription record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (eventNotificationSubscription.recipientAddress != null && eventNotificationSubscription.recipientAddress.Length > 250)
				{
					eventNotificationSubscription.recipientAddress = eventNotificationSubscription.recipientAddress.Substring(0, 250);
				}

				try
				{
				    EntityEntry<Database.EventNotificationSubscription> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(eventNotificationSubscription);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        EventNotificationSubscriptionChangeHistory eventNotificationSubscriptionChangeHistory = new EventNotificationSubscriptionChangeHistory();
				        eventNotificationSubscriptionChangeHistory.eventNotificationSubscriptionId = eventNotificationSubscription.id;
				        eventNotificationSubscriptionChangeHistory.versionNumber = eventNotificationSubscription.versionNumber;
				        eventNotificationSubscriptionChangeHistory.timeStamp = DateTime.UtcNow;
				        eventNotificationSubscriptionChangeHistory.userId = securityUser.id;
				        eventNotificationSubscriptionChangeHistory.tenantGuid = userTenantGuid;
				        eventNotificationSubscriptionChangeHistory.data = JsonSerializer.Serialize(Database.EventNotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(eventNotificationSubscription));
				        _context.EventNotificationSubscriptionChangeHistories.Add(eventNotificationSubscriptionChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.EventNotificationSubscription entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.EventNotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.EventNotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(eventNotificationSubscription)),
						null);

				return Ok(Database.EventNotificationSubscription.CreateAnonymous(eventNotificationSubscription));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.EventNotificationSubscription entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.EventNotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.EventNotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(eventNotificationSubscription)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new EventNotificationSubscription record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventNotificationSubscription", Name = "EventNotificationSubscription")]
		public async Task<IActionResult> PostEventNotificationSubscription([FromBody]Database.EventNotificationSubscription.EventNotificationSubscriptionDTO eventNotificationSubscriptionDTO, CancellationToken cancellationToken = default)
		{
			if (eventNotificationSubscriptionDTO == null)
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
			// Create a new EventNotificationSubscription object using the data from the DTO
			//
			Database.EventNotificationSubscription eventNotificationSubscription = Database.EventNotificationSubscription.FromDTO(eventNotificationSubscriptionDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				eventNotificationSubscription.tenantGuid = userTenantGuid;

				if (eventNotificationSubscription.recipientAddress != null && eventNotificationSubscription.recipientAddress.Length > 250)
				{
					eventNotificationSubscription.recipientAddress = eventNotificationSubscription.recipientAddress.Substring(0, 250);
				}

				eventNotificationSubscription.objectGuid = Guid.NewGuid();
				eventNotificationSubscription.versionNumber = 1;

				_context.EventNotificationSubscriptions.Add(eventNotificationSubscription);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the eventNotificationSubscription object so that no further changes will be written to the database
				    //
				    _context.Entry(eventNotificationSubscription).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					eventNotificationSubscription.EventNotificationSubscriptionChangeHistories = null;
					eventNotificationSubscription.contact = null;
					eventNotificationSubscription.eventNotificationType = null;
					eventNotificationSubscription.resource = null;


				    EventNotificationSubscriptionChangeHistory eventNotificationSubscriptionChangeHistory = new EventNotificationSubscriptionChangeHistory();
				    eventNotificationSubscriptionChangeHistory.eventNotificationSubscriptionId = eventNotificationSubscription.id;
				    eventNotificationSubscriptionChangeHistory.versionNumber = eventNotificationSubscription.versionNumber;
				    eventNotificationSubscriptionChangeHistory.timeStamp = DateTime.UtcNow;
				    eventNotificationSubscriptionChangeHistory.userId = securityUser.id;
				    eventNotificationSubscriptionChangeHistory.tenantGuid = userTenantGuid;
				    eventNotificationSubscriptionChangeHistory.data = JsonSerializer.Serialize(Database.EventNotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(eventNotificationSubscription));
				    _context.EventNotificationSubscriptionChangeHistories.Add(eventNotificationSubscriptionChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.EventNotificationSubscription entity successfully created.",
						true,
						eventNotificationSubscription. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.EventNotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(eventNotificationSubscription)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.EventNotificationSubscription entity creation failed.", false, eventNotificationSubscription.id.ToString(), "", JsonSerializer.Serialize(Database.EventNotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(eventNotificationSubscription)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "EventNotificationSubscription", eventNotificationSubscription.id, eventNotificationSubscription.recipientAddress));

			return CreatedAtRoute("EventNotificationSubscription", new { id = eventNotificationSubscription.id }, Database.EventNotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(eventNotificationSubscription));
		}



        /// <summary>
        /// 
        /// This rolls a EventNotificationSubscription entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventNotificationSubscription/Rollback/{id}")]
		[Route("api/EventNotificationSubscription/Rollback")]
		public async Task<IActionResult> RollbackToEventNotificationSubscriptionVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.EventNotificationSubscription> query = (from x in _context.EventNotificationSubscriptions
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this EventNotificationSubscription concurrently
			//
			lock (eventNotificationSubscriptionPutSyncRoot)
			{
				
				Database.EventNotificationSubscription eventNotificationSubscription = query.FirstOrDefault();
				
				if (eventNotificationSubscription == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.EventNotificationSubscription rollback", id.ToString(), new Exception("No Scheduler.EventNotificationSubscription entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the EventNotificationSubscription current state so we can log it.
				//
				Database.EventNotificationSubscription cloneOfExisting = (Database.EventNotificationSubscription)_context.Entry(eventNotificationSubscription).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.EventNotificationSubscriptionChangeHistories = null;
				cloneOfExisting.contact = null;
				cloneOfExisting.eventNotificationType = null;
				cloneOfExisting.resource = null;

				if (versionNumber >= eventNotificationSubscription.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.EventNotificationSubscription rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.EventNotificationSubscription rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				EventNotificationSubscriptionChangeHistory eventNotificationSubscriptionChangeHistory = (from x in _context.EventNotificationSubscriptionChangeHistories
				                                               where
				                                               x.eventNotificationSubscriptionId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (eventNotificationSubscriptionChangeHistory != null)
				{
				    Database.EventNotificationSubscription oldEventNotificationSubscription = JsonSerializer.Deserialize<Database.EventNotificationSubscription>(eventNotificationSubscriptionChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    eventNotificationSubscription.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    eventNotificationSubscription.resourceId = oldEventNotificationSubscription.resourceId;
				    eventNotificationSubscription.contactId = oldEventNotificationSubscription.contactId;
				    eventNotificationSubscription.eventNotificationTypeId = oldEventNotificationSubscription.eventNotificationTypeId;
				    eventNotificationSubscription.triggerEvents = oldEventNotificationSubscription.triggerEvents;
				    eventNotificationSubscription.recipientAddress = oldEventNotificationSubscription.recipientAddress;
				    eventNotificationSubscription.objectGuid = oldEventNotificationSubscription.objectGuid;
				    eventNotificationSubscription.active = oldEventNotificationSubscription.active;
				    eventNotificationSubscription.deleted = oldEventNotificationSubscription.deleted;

				    string serializedEventNotificationSubscription = JsonSerializer.Serialize(eventNotificationSubscription);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        EventNotificationSubscriptionChangeHistory newEventNotificationSubscriptionChangeHistory = new EventNotificationSubscriptionChangeHistory();
				        newEventNotificationSubscriptionChangeHistory.eventNotificationSubscriptionId = eventNotificationSubscription.id;
				        newEventNotificationSubscriptionChangeHistory.versionNumber = eventNotificationSubscription.versionNumber;
				        newEventNotificationSubscriptionChangeHistory.timeStamp = DateTime.UtcNow;
				        newEventNotificationSubscriptionChangeHistory.userId = securityUser.id;
				        newEventNotificationSubscriptionChangeHistory.tenantGuid = userTenantGuid;
				        newEventNotificationSubscriptionChangeHistory.data = JsonSerializer.Serialize(Database.EventNotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(eventNotificationSubscription));
				        _context.EventNotificationSubscriptionChangeHistories.Add(newEventNotificationSubscriptionChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.EventNotificationSubscription rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.EventNotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.EventNotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(eventNotificationSubscription)),
						null);


				    return Ok(Database.EventNotificationSubscription.CreateAnonymous(eventNotificationSubscription));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.EventNotificationSubscription rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.EventNotificationSubscription rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a EventNotificationSubscription.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the EventNotificationSubscription</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventNotificationSubscription/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetEventNotificationSubscriptionChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.EventNotificationSubscription eventNotificationSubscription = await _context.EventNotificationSubscriptions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (eventNotificationSubscription == null)
			{
				return NotFound();
			}

			try
			{
				eventNotificationSubscription.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.EventNotificationSubscription> versionInfo = await eventNotificationSubscription.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a EventNotificationSubscription.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the EventNotificationSubscription</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventNotificationSubscription/{id}/AuditHistory")]
		public async Task<IActionResult> GetEventNotificationSubscriptionAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.EventNotificationSubscription eventNotificationSubscription = await _context.EventNotificationSubscriptions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (eventNotificationSubscription == null)
			{
				return NotFound();
			}

			try
			{
				eventNotificationSubscription.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.EventNotificationSubscription>> versions = await eventNotificationSubscription.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a EventNotificationSubscription.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the EventNotificationSubscription</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The EventNotificationSubscription object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventNotificationSubscription/{id}/Version/{version}")]
		public async Task<IActionResult> GetEventNotificationSubscriptionVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.EventNotificationSubscription eventNotificationSubscription = await _context.EventNotificationSubscriptions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (eventNotificationSubscription == null)
			{
				return NotFound();
			}

			try
			{
				eventNotificationSubscription.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.EventNotificationSubscription> versionInfo = await eventNotificationSubscription.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a EventNotificationSubscription at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the EventNotificationSubscription</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The EventNotificationSubscription object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventNotificationSubscription/{id}/StateAtTime")]
		public async Task<IActionResult> GetEventNotificationSubscriptionStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.EventNotificationSubscription eventNotificationSubscription = await _context.EventNotificationSubscriptions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (eventNotificationSubscription == null)
			{
				return NotFound();
			}

			try
			{
				eventNotificationSubscription.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.EventNotificationSubscription> versionInfo = await eventNotificationSubscription.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a EventNotificationSubscription record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventNotificationSubscription/{id}")]
		[Route("api/EventNotificationSubscription")]
		public async Task<IActionResult> DeleteEventNotificationSubscription(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.EventNotificationSubscription> query = (from x in _context.EventNotificationSubscriptions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.EventNotificationSubscription eventNotificationSubscription = await query.FirstOrDefaultAsync(cancellationToken);

			if (eventNotificationSubscription == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.EventNotificationSubscription DELETE", id.ToString(), new Exception("No Scheduler.EventNotificationSubscription entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.EventNotificationSubscription cloneOfExisting = (Database.EventNotificationSubscription)_context.Entry(eventNotificationSubscription).GetDatabaseValues().ToObject();


			lock (eventNotificationSubscriptionDeleteSyncRoot)
			{
			    try
			    {
			        eventNotificationSubscription.deleted = true;
			        eventNotificationSubscription.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        EventNotificationSubscriptionChangeHistory eventNotificationSubscriptionChangeHistory = new EventNotificationSubscriptionChangeHistory();
			        eventNotificationSubscriptionChangeHistory.eventNotificationSubscriptionId = eventNotificationSubscription.id;
			        eventNotificationSubscriptionChangeHistory.versionNumber = eventNotificationSubscription.versionNumber;
			        eventNotificationSubscriptionChangeHistory.timeStamp = DateTime.UtcNow;
			        eventNotificationSubscriptionChangeHistory.userId = securityUser.id;
			        eventNotificationSubscriptionChangeHistory.tenantGuid = userTenantGuid;
			        eventNotificationSubscriptionChangeHistory.data = JsonSerializer.Serialize(Database.EventNotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(eventNotificationSubscription));
			        _context.EventNotificationSubscriptionChangeHistories.Add(eventNotificationSubscriptionChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.EventNotificationSubscription entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.EventNotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.EventNotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(eventNotificationSubscription)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.EventNotificationSubscription entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.EventNotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.EventNotificationSubscription.CreateAnonymousWithFirstLevelSubObjects(eventNotificationSubscription)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of EventNotificationSubscription records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/EventNotificationSubscriptions/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? resourceId = null,
			int? contactId = null,
			int? eventNotificationTypeId = null,
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

			IQueryable<Database.EventNotificationSubscription> query = (from ens in _context.EventNotificationSubscriptions select ens);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (resourceId.HasValue == true)
			{
				query = query.Where(ens => ens.resourceId == resourceId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(ens => ens.contactId == contactId.Value);
			}
			if (eventNotificationTypeId.HasValue == true)
			{
				query = query.Where(ens => ens.eventNotificationTypeId == eventNotificationTypeId.Value);
			}
			if (triggerEvents.HasValue == true)
			{
				query = query.Where(ens => ens.triggerEvents == triggerEvents.Value);
			}
			if (string.IsNullOrEmpty(recipientAddress) == false)
			{
				query = query.Where(ens => ens.recipientAddress == recipientAddress);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ens => ens.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ens => ens.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ens => ens.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ens => ens.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ens => ens.deleted == false);
				}
			}
			else
			{
				query = query.Where(ens => ens.active == true);
				query = query.Where(ens => ens.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Event Notification Subscription, or on an any of the string fields on its immediate relations
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
			       || x.eventNotificationType.name.Contains(anyStringContains)
			       || x.eventNotificationType.description.Contains(anyStringContains)
			       || x.eventNotificationType.color.Contains(anyStringContains)
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
			return Ok(await (from queryData in query select Database.EventNotificationSubscription.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/EventNotificationSubscription/CreateAuditEvent")]
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
