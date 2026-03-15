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
    /// This auto generated class provides the basic CRUD operations for the ContactInteraction entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ContactInteraction entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ContactInteractionsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object contactInteractionPutSyncRoot = new object();
		static object contactInteractionDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<ContactInteractionsController> _logger;

		public ContactInteractionsController(SchedulerContext context, ILogger<ContactInteractionsController> logger) : base("Scheduler", "ContactInteraction")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ContactInteractions filtered by the parameters provided.
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
		[Route("api/ContactInteractions")]
		public async Task<IActionResult> GetContactInteractions(
			int? contactId = null,
			int? initiatingContactId = null,
			int? interactionTypeId = null,
			int? scheduledEventId = null,
			DateTime? startTime = null,
			DateTime? endTime = null,
			string notes = null,
			string location = null,
			int? priorityId = null,
			string externalId = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (startTime.HasValue == true && startTime.Value.Kind != DateTimeKind.Utc)
			{
				startTime = startTime.Value.ToUniversalTime();
			}

			if (endTime.HasValue == true && endTime.Value.Kind != DateTimeKind.Utc)
			{
				endTime = endTime.Value.ToUniversalTime();
			}

			IQueryable<Database.ContactInteraction> query = (from ci in _context.ContactInteractions select ci);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (contactId.HasValue == true)
			{
				query = query.Where(ci => ci.contactId == contactId.Value);
			}
			if (initiatingContactId.HasValue == true)
			{
				query = query.Where(ci => ci.initiatingContactId == initiatingContactId.Value);
			}
			if (interactionTypeId.HasValue == true)
			{
				query = query.Where(ci => ci.interactionTypeId == interactionTypeId.Value);
			}
			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(ci => ci.scheduledEventId == scheduledEventId.Value);
			}
			if (startTime.HasValue == true)
			{
				query = query.Where(ci => ci.startTime == startTime.Value);
			}
			if (endTime.HasValue == true)
			{
				query = query.Where(ci => ci.endTime == endTime.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(ci => ci.notes == notes);
			}
			if (string.IsNullOrEmpty(location) == false)
			{
				query = query.Where(ci => ci.location == location);
			}
			if (priorityId.HasValue == true)
			{
				query = query.Where(ci => ci.priorityId == priorityId.Value);
			}
			if (string.IsNullOrEmpty(externalId) == false)
			{
				query = query.Where(ci => ci.externalId == externalId);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ci => ci.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ci => ci.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ci => ci.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ci => ci.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ci => ci.deleted == false);
				}
			}
			else
			{
				query = query.Where(ci => ci.active == true);
				query = query.Where(ci => ci.deleted == false);
			}

			query = query.OrderBy(ci => ci.externalId);


			//
			// Add the any string contains parameter to span all the string fields on the Contact Interaction, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.notes.Contains(anyStringContains)
			       || x.location.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
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
			       || (includeRelations == true && x.initiatingContact.firstName.Contains(anyStringContains))
			       || (includeRelations == true && x.initiatingContact.middleName.Contains(anyStringContains))
			       || (includeRelations == true && x.initiatingContact.lastName.Contains(anyStringContains))
			       || (includeRelations == true && x.initiatingContact.title.Contains(anyStringContains))
			       || (includeRelations == true && x.initiatingContact.company.Contains(anyStringContains))
			       || (includeRelations == true && x.initiatingContact.email.Contains(anyStringContains))
			       || (includeRelations == true && x.initiatingContact.phone.Contains(anyStringContains))
			       || (includeRelations == true && x.initiatingContact.mobile.Contains(anyStringContains))
			       || (includeRelations == true && x.initiatingContact.position.Contains(anyStringContains))
			       || (includeRelations == true && x.initiatingContact.webSite.Contains(anyStringContains))
			       || (includeRelations == true && x.initiatingContact.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.initiatingContact.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.initiatingContact.color.Contains(anyStringContains))
			       || (includeRelations == true && x.initiatingContact.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.initiatingContact.avatarMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.initiatingContact.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.interactionType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.interactionType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.interactionType.color.Contains(anyStringContains))
			       || (includeRelations == true && x.priority.name.Contains(anyStringContains))
			       || (includeRelations == true && x.priority.description.Contains(anyStringContains))
			       || (includeRelations == true && x.priority.color.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.name.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.description.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.location.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.bookingContactName.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.bookingContactEmail.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.bookingContactPhone.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.color.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.attributes.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.contact);
				query = query.Include(x => x.initiatingContact);
				query = query.Include(x => x.interactionType);
				query = query.Include(x => x.priority);
				query = query.Include(x => x.scheduledEvent);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.ContactInteraction> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ContactInteraction contactInteraction in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(contactInteraction, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ContactInteraction Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ContactInteraction Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ContactInteractions filtered by the parameters provided.  Its query is similar to the GetContactInteractions method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContactInteractions/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? contactId = null,
			int? initiatingContactId = null,
			int? interactionTypeId = null,
			int? scheduledEventId = null,
			DateTime? startTime = null,
			DateTime? endTime = null,
			string notes = null,
			string location = null,
			int? priorityId = null,
			string externalId = null,
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


			//
			// Fix any non-UTC date parameters that come in.
			//
			if (startTime.HasValue == true && startTime.Value.Kind != DateTimeKind.Utc)
			{
				startTime = startTime.Value.ToUniversalTime();
			}

			if (endTime.HasValue == true && endTime.Value.Kind != DateTimeKind.Utc)
			{
				endTime = endTime.Value.ToUniversalTime();
			}

			IQueryable<Database.ContactInteraction> query = (from ci in _context.ContactInteractions select ci);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (contactId.HasValue == true)
			{
				query = query.Where(ci => ci.contactId == contactId.Value);
			}
			if (initiatingContactId.HasValue == true)
			{
				query = query.Where(ci => ci.initiatingContactId == initiatingContactId.Value);
			}
			if (interactionTypeId.HasValue == true)
			{
				query = query.Where(ci => ci.interactionTypeId == interactionTypeId.Value);
			}
			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(ci => ci.scheduledEventId == scheduledEventId.Value);
			}
			if (startTime.HasValue == true)
			{
				query = query.Where(ci => ci.startTime == startTime.Value);
			}
			if (endTime.HasValue == true)
			{
				query = query.Where(ci => ci.endTime == endTime.Value);
			}
			if (notes != null)
			{
				query = query.Where(ci => ci.notes == notes);
			}
			if (location != null)
			{
				query = query.Where(ci => ci.location == location);
			}
			if (priorityId.HasValue == true)
			{
				query = query.Where(ci => ci.priorityId == priorityId.Value);
			}
			if (externalId != null)
			{
				query = query.Where(ci => ci.externalId == externalId);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ci => ci.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ci => ci.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ci => ci.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ci => ci.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ci => ci.deleted == false);
				}
			}
			else
			{
				query = query.Where(ci => ci.active == true);
				query = query.Where(ci => ci.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Contact Interaction, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.notes.Contains(anyStringContains)
			       || x.location.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
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
			       || x.initiatingContact.firstName.Contains(anyStringContains)
			       || x.initiatingContact.middleName.Contains(anyStringContains)
			       || x.initiatingContact.lastName.Contains(anyStringContains)
			       || x.initiatingContact.title.Contains(anyStringContains)
			       || x.initiatingContact.company.Contains(anyStringContains)
			       || x.initiatingContact.email.Contains(anyStringContains)
			       || x.initiatingContact.phone.Contains(anyStringContains)
			       || x.initiatingContact.mobile.Contains(anyStringContains)
			       || x.initiatingContact.position.Contains(anyStringContains)
			       || x.initiatingContact.webSite.Contains(anyStringContains)
			       || x.initiatingContact.notes.Contains(anyStringContains)
			       || x.initiatingContact.attributes.Contains(anyStringContains)
			       || x.initiatingContact.color.Contains(anyStringContains)
			       || x.initiatingContact.avatarFileName.Contains(anyStringContains)
			       || x.initiatingContact.avatarMimeType.Contains(anyStringContains)
			       || x.initiatingContact.externalId.Contains(anyStringContains)
			       || x.interactionType.name.Contains(anyStringContains)
			       || x.interactionType.description.Contains(anyStringContains)
			       || x.interactionType.color.Contains(anyStringContains)
			       || x.priority.name.Contains(anyStringContains)
			       || x.priority.description.Contains(anyStringContains)
			       || x.priority.color.Contains(anyStringContains)
			       || x.scheduledEvent.name.Contains(anyStringContains)
			       || x.scheduledEvent.description.Contains(anyStringContains)
			       || x.scheduledEvent.location.Contains(anyStringContains)
			       || x.scheduledEvent.bookingContactName.Contains(anyStringContains)
			       || x.scheduledEvent.bookingContactEmail.Contains(anyStringContains)
			       || x.scheduledEvent.bookingContactPhone.Contains(anyStringContains)
			       || x.scheduledEvent.notes.Contains(anyStringContains)
			       || x.scheduledEvent.color.Contains(anyStringContains)
			       || x.scheduledEvent.externalId.Contains(anyStringContains)
			       || x.scheduledEvent.attributes.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ContactInteraction by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContactInteraction/{id}")]
		public async Task<IActionResult> GetContactInteraction(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ContactInteraction> query = (from ci in _context.ContactInteractions where
							(ci.id == id) &&
							(userIsAdmin == true || ci.deleted == false) &&
							(userIsWriter == true || ci.active == true)
					select ci);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.contact);
					query = query.Include(x => x.initiatingContact);
					query = query.Include(x => x.interactionType);
					query = query.Include(x => x.priority);
					query = query.Include(x => x.scheduledEvent);
					query = query.AsSplitQuery();
				}

				Database.ContactInteraction materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.ContactInteraction Entity was read with Admin privilege." : "Scheduler.ContactInteraction Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ContactInteraction", materialized.id, materialized.externalId));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.ContactInteraction entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.ContactInteraction.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.ContactInteraction.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ContactInteraction record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ContactInteraction/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutContactInteraction(int id, [FromBody]Database.ContactInteraction.ContactInteractionDTO contactInteractionDTO, CancellationToken cancellationToken = default)
		{
			if (contactInteractionDTO == null)
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



			if (id != contactInteractionDTO.id)
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


			IQueryable<Database.ContactInteraction> query = (from x in _context.ContactInteractions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ContactInteraction existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ContactInteraction PUT", id.ToString(), new Exception("No Scheduler.ContactInteraction entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (contactInteractionDTO.objectGuid == Guid.Empty)
            {
                contactInteractionDTO.objectGuid = existing.objectGuid;
            }
            else if (contactInteractionDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ContactInteraction record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ContactInteraction cloneOfExisting = (Database.ContactInteraction)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ContactInteraction object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ContactInteraction contactInteraction = (Database.ContactInteraction)_context.Entry(existing).GetDatabaseValues().ToObject();
			contactInteraction.ApplyDTO(contactInteractionDTO);
			//
			// The tenant guid for any ContactInteraction being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ContactInteraction because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				contactInteraction.tenantGuid = existing.tenantGuid;
			}

			lock (contactInteractionPutSyncRoot)
			{
				//
				// Validate the version number for the contactInteraction being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != contactInteraction.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ContactInteraction save attempt was made but save request was with version " + contactInteraction.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ContactInteraction you are trying to update has already changed.  Please try your save again after reloading the ContactInteraction.");
				}
				else
				{
					// Same record.  Increase version.
					contactInteraction.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (contactInteraction.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.ContactInteraction record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (contactInteraction.startTime.Kind != DateTimeKind.Utc)
				{
					contactInteraction.startTime = contactInteraction.startTime.ToUniversalTime();
				}

				if (contactInteraction.endTime.HasValue == true && contactInteraction.endTime.Value.Kind != DateTimeKind.Utc)
				{
					contactInteraction.endTime = contactInteraction.endTime.Value.ToUniversalTime();
				}

				if (contactInteraction.externalId != null && contactInteraction.externalId.Length > 100)
				{
					contactInteraction.externalId = contactInteraction.externalId.Substring(0, 100);
				}

				try
				{
				    EntityEntry<Database.ContactInteraction> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(contactInteraction);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ContactInteractionChangeHistory contactInteractionChangeHistory = new ContactInteractionChangeHistory();
				        contactInteractionChangeHistory.contactInteractionId = contactInteraction.id;
				        contactInteractionChangeHistory.versionNumber = contactInteraction.versionNumber;
				        contactInteractionChangeHistory.timeStamp = DateTime.UtcNow;
				        contactInteractionChangeHistory.userId = securityUser.id;
				        contactInteractionChangeHistory.tenantGuid = userTenantGuid;
				        contactInteractionChangeHistory.data = JsonSerializer.Serialize(Database.ContactInteraction.CreateAnonymousWithFirstLevelSubObjects(contactInteraction));
				        _context.ContactInteractionChangeHistories.Add(contactInteractionChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ContactInteraction entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ContactInteraction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ContactInteraction.CreateAnonymousWithFirstLevelSubObjects(contactInteraction)),
						null);

				return Ok(Database.ContactInteraction.CreateAnonymous(contactInteraction));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ContactInteraction entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ContactInteraction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ContactInteraction.CreateAnonymousWithFirstLevelSubObjects(contactInteraction)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ContactInteraction record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContactInteraction", Name = "ContactInteraction")]
		public async Task<IActionResult> PostContactInteraction([FromBody]Database.ContactInteraction.ContactInteractionDTO contactInteractionDTO, CancellationToken cancellationToken = default)
		{
			if (contactInteractionDTO == null)
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
			// Create a new ContactInteraction object using the data from the DTO
			//
			Database.ContactInteraction contactInteraction = Database.ContactInteraction.FromDTO(contactInteractionDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				contactInteraction.tenantGuid = userTenantGuid;

				if (contactInteraction.startTime.Kind != DateTimeKind.Utc)
				{
					contactInteraction.startTime = contactInteraction.startTime.ToUniversalTime();
				}

				if (contactInteraction.endTime.HasValue == true && contactInteraction.endTime.Value.Kind != DateTimeKind.Utc)
				{
					contactInteraction.endTime = contactInteraction.endTime.Value.ToUniversalTime();
				}

				if (contactInteraction.externalId != null && contactInteraction.externalId.Length > 100)
				{
					contactInteraction.externalId = contactInteraction.externalId.Substring(0, 100);
				}

				contactInteraction.objectGuid = Guid.NewGuid();
				contactInteraction.versionNumber = 1;

				_context.ContactInteractions.Add(contactInteraction);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the contactInteraction object so that no further changes will be written to the database
				    //
				    _context.Entry(contactInteraction).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					contactInteraction.ContactInteractionChangeHistories = null;
					contactInteraction.contact = null;
					contactInteraction.initiatingContact = null;
					contactInteraction.interactionType = null;
					contactInteraction.priority = null;
					contactInteraction.scheduledEvent = null;


				    ContactInteractionChangeHistory contactInteractionChangeHistory = new ContactInteractionChangeHistory();
				    contactInteractionChangeHistory.contactInteractionId = contactInteraction.id;
				    contactInteractionChangeHistory.versionNumber = contactInteraction.versionNumber;
				    contactInteractionChangeHistory.timeStamp = DateTime.UtcNow;
				    contactInteractionChangeHistory.userId = securityUser.id;
				    contactInteractionChangeHistory.tenantGuid = userTenantGuid;
				    contactInteractionChangeHistory.data = JsonSerializer.Serialize(Database.ContactInteraction.CreateAnonymousWithFirstLevelSubObjects(contactInteraction));
				    _context.ContactInteractionChangeHistories.Add(contactInteractionChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.ContactInteraction entity successfully created.",
						true,
						contactInteraction. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ContactInteraction.CreateAnonymousWithFirstLevelSubObjects(contactInteraction)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.ContactInteraction entity creation failed.", false, contactInteraction.id.ToString(), "", JsonSerializer.Serialize(Database.ContactInteraction.CreateAnonymousWithFirstLevelSubObjects(contactInteraction)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ContactInteraction", contactInteraction.id, contactInteraction.externalId));

			return CreatedAtRoute("ContactInteraction", new { id = contactInteraction.id }, Database.ContactInteraction.CreateAnonymousWithFirstLevelSubObjects(contactInteraction));
		}



        /// <summary>
        /// 
        /// This rolls a ContactInteraction entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContactInteraction/Rollback/{id}")]
		[Route("api/ContactInteraction/Rollback")]
		public async Task<IActionResult> RollbackToContactInteractionVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.ContactInteraction> query = (from x in _context.ContactInteractions
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this ContactInteraction concurrently
			//
			lock (contactInteractionPutSyncRoot)
			{
				
				Database.ContactInteraction contactInteraction = query.FirstOrDefault();
				
				if (contactInteraction == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ContactInteraction rollback", id.ToString(), new Exception("No Scheduler.ContactInteraction entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ContactInteraction current state so we can log it.
				//
				Database.ContactInteraction cloneOfExisting = (Database.ContactInteraction)_context.Entry(contactInteraction).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.ContactInteractionChangeHistories = null;
				cloneOfExisting.contact = null;
				cloneOfExisting.initiatingContact = null;
				cloneOfExisting.interactionType = null;
				cloneOfExisting.priority = null;
				cloneOfExisting.scheduledEvent = null;

				if (versionNumber >= contactInteraction.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.ContactInteraction rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.ContactInteraction rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ContactInteractionChangeHistory contactInteractionChangeHistory = (from x in _context.ContactInteractionChangeHistories
				                                               where
				                                               x.contactInteractionId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (contactInteractionChangeHistory != null)
				{
				    Database.ContactInteraction oldContactInteraction = JsonSerializer.Deserialize<Database.ContactInteraction>(contactInteractionChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    contactInteraction.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    contactInteraction.contactId = oldContactInteraction.contactId;
				    contactInteraction.initiatingContactId = oldContactInteraction.initiatingContactId;
				    contactInteraction.interactionTypeId = oldContactInteraction.interactionTypeId;
				    contactInteraction.scheduledEventId = oldContactInteraction.scheduledEventId;
				    contactInteraction.startTime = oldContactInteraction.startTime;
				    contactInteraction.endTime = oldContactInteraction.endTime;
				    contactInteraction.notes = oldContactInteraction.notes;
				    contactInteraction.location = oldContactInteraction.location;
				    contactInteraction.priorityId = oldContactInteraction.priorityId;
				    contactInteraction.externalId = oldContactInteraction.externalId;
				    contactInteraction.objectGuid = oldContactInteraction.objectGuid;
				    contactInteraction.active = oldContactInteraction.active;
				    contactInteraction.deleted = oldContactInteraction.deleted;

				    string serializedContactInteraction = JsonSerializer.Serialize(contactInteraction);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ContactInteractionChangeHistory newContactInteractionChangeHistory = new ContactInteractionChangeHistory();
				        newContactInteractionChangeHistory.contactInteractionId = contactInteraction.id;
				        newContactInteractionChangeHistory.versionNumber = contactInteraction.versionNumber;
				        newContactInteractionChangeHistory.timeStamp = DateTime.UtcNow;
				        newContactInteractionChangeHistory.userId = securityUser.id;
				        newContactInteractionChangeHistory.tenantGuid = userTenantGuid;
				        newContactInteractionChangeHistory.data = JsonSerializer.Serialize(Database.ContactInteraction.CreateAnonymousWithFirstLevelSubObjects(contactInteraction));
				        _context.ContactInteractionChangeHistories.Add(newContactInteractionChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ContactInteraction rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ContactInteraction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ContactInteraction.CreateAnonymousWithFirstLevelSubObjects(contactInteraction)),
						null);


				    return Ok(Database.ContactInteraction.CreateAnonymous(contactInteraction));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.ContactInteraction rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.ContactInteraction rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a ContactInteraction.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ContactInteraction</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContactInteraction/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetContactInteractionChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.ContactInteraction contactInteraction = await _context.ContactInteractions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (contactInteraction == null)
			{
				return NotFound();
			}

			try
			{
				contactInteraction.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ContactInteraction> versionInfo = await contactInteraction.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a ContactInteraction.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ContactInteraction</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContactInteraction/{id}/AuditHistory")]
		public async Task<IActionResult> GetContactInteractionAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.ContactInteraction contactInteraction = await _context.ContactInteractions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (contactInteraction == null)
			{
				return NotFound();
			}

			try
			{
				contactInteraction.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.ContactInteraction>> versions = await contactInteraction.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a ContactInteraction.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ContactInteraction</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The ContactInteraction object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContactInteraction/{id}/Version/{version}")]
		public async Task<IActionResult> GetContactInteractionVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.ContactInteraction contactInteraction = await _context.ContactInteractions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (contactInteraction == null)
			{
				return NotFound();
			}

			try
			{
				contactInteraction.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ContactInteraction> versionInfo = await contactInteraction.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a ContactInteraction at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ContactInteraction</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The ContactInteraction object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContactInteraction/{id}/StateAtTime")]
		public async Task<IActionResult> GetContactInteractionStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.ContactInteraction contactInteraction = await _context.ContactInteractions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (contactInteraction == null)
			{
				return NotFound();
			}

			try
			{
				contactInteraction.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ContactInteraction> versionInfo = await contactInteraction.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a ContactInteraction record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContactInteraction/{id}")]
		[Route("api/ContactInteraction")]
		public async Task<IActionResult> DeleteContactInteraction(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ContactInteraction> query = (from x in _context.ContactInteractions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ContactInteraction contactInteraction = await query.FirstOrDefaultAsync(cancellationToken);

			if (contactInteraction == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ContactInteraction DELETE", id.ToString(), new Exception("No Scheduler.ContactInteraction entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ContactInteraction cloneOfExisting = (Database.ContactInteraction)_context.Entry(contactInteraction).GetDatabaseValues().ToObject();


			lock (contactInteractionDeleteSyncRoot)
			{
			    try
			    {
			        contactInteraction.deleted = true;
			        contactInteraction.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ContactInteractionChangeHistory contactInteractionChangeHistory = new ContactInteractionChangeHistory();
			        contactInteractionChangeHistory.contactInteractionId = contactInteraction.id;
			        contactInteractionChangeHistory.versionNumber = contactInteraction.versionNumber;
			        contactInteractionChangeHistory.timeStamp = DateTime.UtcNow;
			        contactInteractionChangeHistory.userId = securityUser.id;
			        contactInteractionChangeHistory.tenantGuid = userTenantGuid;
			        contactInteractionChangeHistory.data = JsonSerializer.Serialize(Database.ContactInteraction.CreateAnonymousWithFirstLevelSubObjects(contactInteraction));
			        _context.ContactInteractionChangeHistories.Add(contactInteractionChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ContactInteraction entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ContactInteraction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ContactInteraction.CreateAnonymousWithFirstLevelSubObjects(contactInteraction)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ContactInteraction entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ContactInteraction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ContactInteraction.CreateAnonymousWithFirstLevelSubObjects(contactInteraction)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of ContactInteraction records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ContactInteractions/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? contactId = null,
			int? initiatingContactId = null,
			int? interactionTypeId = null,
			int? scheduledEventId = null,
			DateTime? startTime = null,
			DateTime? endTime = null,
			string notes = null,
			string location = null,
			int? priorityId = null,
			string externalId = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (startTime.HasValue == true && startTime.Value.Kind != DateTimeKind.Utc)
			{
				startTime = startTime.Value.ToUniversalTime();
			}

			if (endTime.HasValue == true && endTime.Value.Kind != DateTimeKind.Utc)
			{
				endTime = endTime.Value.ToUniversalTime();
			}

			IQueryable<Database.ContactInteraction> query = (from ci in _context.ContactInteractions select ci);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (contactId.HasValue == true)
			{
				query = query.Where(ci => ci.contactId == contactId.Value);
			}
			if (initiatingContactId.HasValue == true)
			{
				query = query.Where(ci => ci.initiatingContactId == initiatingContactId.Value);
			}
			if (interactionTypeId.HasValue == true)
			{
				query = query.Where(ci => ci.interactionTypeId == interactionTypeId.Value);
			}
			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(ci => ci.scheduledEventId == scheduledEventId.Value);
			}
			if (startTime.HasValue == true)
			{
				query = query.Where(ci => ci.startTime == startTime.Value);
			}
			if (endTime.HasValue == true)
			{
				query = query.Where(ci => ci.endTime == endTime.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(ci => ci.notes == notes);
			}
			if (string.IsNullOrEmpty(location) == false)
			{
				query = query.Where(ci => ci.location == location);
			}
			if (priorityId.HasValue == true)
			{
				query = query.Where(ci => ci.priorityId == priorityId.Value);
			}
			if (string.IsNullOrEmpty(externalId) == false)
			{
				query = query.Where(ci => ci.externalId == externalId);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ci => ci.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ci => ci.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ci => ci.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ci => ci.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ci => ci.deleted == false);
				}
			}
			else
			{
				query = query.Where(ci => ci.active == true);
				query = query.Where(ci => ci.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Contact Interaction, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.notes.Contains(anyStringContains)
			       || x.location.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
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
			       || x.initiatingContact.firstName.Contains(anyStringContains)
			       || x.initiatingContact.middleName.Contains(anyStringContains)
			       || x.initiatingContact.lastName.Contains(anyStringContains)
			       || x.initiatingContact.title.Contains(anyStringContains)
			       || x.initiatingContact.company.Contains(anyStringContains)
			       || x.initiatingContact.email.Contains(anyStringContains)
			       || x.initiatingContact.phone.Contains(anyStringContains)
			       || x.initiatingContact.mobile.Contains(anyStringContains)
			       || x.initiatingContact.position.Contains(anyStringContains)
			       || x.initiatingContact.webSite.Contains(anyStringContains)
			       || x.initiatingContact.notes.Contains(anyStringContains)
			       || x.initiatingContact.attributes.Contains(anyStringContains)
			       || x.initiatingContact.color.Contains(anyStringContains)
			       || x.initiatingContact.avatarFileName.Contains(anyStringContains)
			       || x.initiatingContact.avatarMimeType.Contains(anyStringContains)
			       || x.initiatingContact.externalId.Contains(anyStringContains)
			       || x.interactionType.name.Contains(anyStringContains)
			       || x.interactionType.description.Contains(anyStringContains)
			       || x.interactionType.color.Contains(anyStringContains)
			       || x.priority.name.Contains(anyStringContains)
			       || x.priority.description.Contains(anyStringContains)
			       || x.priority.color.Contains(anyStringContains)
			       || x.scheduledEvent.name.Contains(anyStringContains)
			       || x.scheduledEvent.description.Contains(anyStringContains)
			       || x.scheduledEvent.location.Contains(anyStringContains)
			       || x.scheduledEvent.bookingContactName.Contains(anyStringContains)
			       || x.scheduledEvent.bookingContactEmail.Contains(anyStringContains)
			       || x.scheduledEvent.bookingContactPhone.Contains(anyStringContains)
			       || x.scheduledEvent.notes.Contains(anyStringContains)
			       || x.scheduledEvent.color.Contains(anyStringContains)
			       || x.scheduledEvent.externalId.Contains(anyStringContains)
			       || x.scheduledEvent.attributes.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.externalId);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ContactInteraction.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ContactInteraction/CreateAuditEvent")]
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
