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
    /// This auto generated class provides the basic CRUD operations for the ScheduledEvent entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ScheduledEvent entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ScheduledEventsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object scheduledEventPutSyncRoot = new object();
		static object scheduledEventDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<ScheduledEventsController> _logger;

		public ScheduledEventsController(SchedulerContext context, ILogger<ScheduledEventsController> logger) : base("Scheduler", "ScheduledEvent")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This gets a list of ScheduledEvents filtered by the parameters provided.
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
		[Route("api/ScheduledEvents")]
		public async Task<IActionResult> GetScheduledEvents(
			int? officeId = null,
			int? clientId = null,
			int? scheduledEventTemplateId = null,
			int? recurrenceRuleId = null,
			int? schedulingTargetId = null,
			int? timeZoneId = null,
			int? parentScheduledEventId = null,
			DateTime? recurrenceInstanceDate = null,
			string name = null,
			string description = null,
			bool? isAllDay = null,
			DateTime? startDateTime = null,
			DateTime? endDateTime = null,
			string location = null,
			int? eventStatusId = null,
			int? resourceId = null,
			int? crewId = null,
			int? priorityId = null,
			int? bookingSourceTypeId = null,
			int? eventTypeId = null,
			int? partySize = null,
			string bookingContactName = null,
			string bookingContactEmail = null,
			string bookingContactPhone = null,
			string notes = null,
			string color = null,
			string externalId = null,
			string attributes = null,
			bool? isOpenForVolunteers = null,
			int? maxVolunteerSlots = null,
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
			if (recurrenceInstanceDate.HasValue == true && recurrenceInstanceDate.Value.Kind != DateTimeKind.Utc)
			{
				recurrenceInstanceDate = recurrenceInstanceDate.Value.ToUniversalTime();
			}

			if (startDateTime.HasValue == true && startDateTime.Value.Kind != DateTimeKind.Utc)
			{
				startDateTime = startDateTime.Value.ToUniversalTime();
			}

			if (endDateTime.HasValue == true && endDateTime.Value.Kind != DateTimeKind.Utc)
			{
				endDateTime = endDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.ScheduledEvent> query = (from se in _context.ScheduledEvents select se);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (officeId.HasValue == true)
			{
				query = query.Where(se => se.officeId == officeId.Value);
			}
			if (clientId.HasValue == true)
			{
				query = query.Where(se => se.clientId == clientId.Value);
			}
			if (scheduledEventTemplateId.HasValue == true)
			{
				query = query.Where(se => se.scheduledEventTemplateId == scheduledEventTemplateId.Value);
			}
			if (recurrenceRuleId.HasValue == true)
			{
				query = query.Where(se => se.recurrenceRuleId == recurrenceRuleId.Value);
			}
			if (schedulingTargetId.HasValue == true)
			{
				query = query.Where(se => se.schedulingTargetId == schedulingTargetId.Value);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(se => se.timeZoneId == timeZoneId.Value);
			}
			if (parentScheduledEventId.HasValue == true)
			{
				query = query.Where(se => se.parentScheduledEventId == parentScheduledEventId.Value);
			}
			if (recurrenceInstanceDate.HasValue == true)
			{
				query = query.Where(se => se.recurrenceInstanceDate == recurrenceInstanceDate.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(se => se.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(se => se.description == description);
			}
			if (isAllDay.HasValue == true)
			{
				query = query.Where(se => se.isAllDay == isAllDay.Value);
			}
			if (startDateTime.HasValue == true)
			{
				query = query.Where(se => se.startDateTime == startDateTime.Value);
			}
			if (endDateTime.HasValue == true)
			{
				query = query.Where(se => se.endDateTime == endDateTime.Value);
			}
			if (string.IsNullOrEmpty(location) == false)
			{
				query = query.Where(se => se.location == location);
			}
			if (eventStatusId.HasValue == true)
			{
				query = query.Where(se => se.eventStatusId == eventStatusId.Value);
			}
			if (resourceId.HasValue == true)
			{
				query = query.Where(se => se.resourceId == resourceId.Value);
			}
			if (crewId.HasValue == true)
			{
				query = query.Where(se => se.crewId == crewId.Value);
			}
			if (priorityId.HasValue == true)
			{
				query = query.Where(se => se.priorityId == priorityId.Value);
			}
			if (bookingSourceTypeId.HasValue == true)
			{
				query = query.Where(se => se.bookingSourceTypeId == bookingSourceTypeId.Value);
			}
			if (eventTypeId.HasValue == true)
			{
				query = query.Where(se => se.eventTypeId == eventTypeId.Value);
			}
			if (partySize.HasValue == true)
			{
				query = query.Where(se => se.partySize == partySize.Value);
			}
			if (string.IsNullOrEmpty(bookingContactName) == false)
			{
				query = query.Where(se => se.bookingContactName == bookingContactName);
			}
			if (string.IsNullOrEmpty(bookingContactEmail) == false)
			{
				query = query.Where(se => se.bookingContactEmail == bookingContactEmail);
			}
			if (string.IsNullOrEmpty(bookingContactPhone) == false)
			{
				query = query.Where(se => se.bookingContactPhone == bookingContactPhone);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(se => se.notes == notes);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(se => se.color == color);
			}
			if (string.IsNullOrEmpty(externalId) == false)
			{
				query = query.Where(se => se.externalId == externalId);
			}
			if (string.IsNullOrEmpty(attributes) == false)
			{
				query = query.Where(se => se.attributes == attributes);
			}
			if (isOpenForVolunteers.HasValue == true)
			{
				query = query.Where(se => se.isOpenForVolunteers == isOpenForVolunteers.Value);
			}
			if (maxVolunteerSlots.HasValue == true)
			{
				query = query.Where(se => se.maxVolunteerSlots == maxVolunteerSlots.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(se => se.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(se => se.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(se => se.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(se => se.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(se => se.deleted == false);
				}
			}
			else
			{
				query = query.Where(se => se.active == true);
				query = query.Where(se => se.deleted == false);
			}

			query = query.OrderBy(se => se.name);


			//
			// Add the any string contains parameter to span all the string fields on the Scheduled Event, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.location.Contains(anyStringContains)
			       || x.bookingContactName.Contains(anyStringContains)
			       || x.bookingContactEmail.Contains(anyStringContains)
			       || x.bookingContactPhone.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.attributes.Contains(anyStringContains)
			       || (includeRelations == true && x.bookingSourceType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.bookingSourceType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.bookingSourceType.color.Contains(anyStringContains))
			       || (includeRelations == true && x.client.name.Contains(anyStringContains))
			       || (includeRelations == true && x.client.description.Contains(anyStringContains))
			       || (includeRelations == true && x.client.addressLine1.Contains(anyStringContains))
			       || (includeRelations == true && x.client.addressLine2.Contains(anyStringContains))
			       || (includeRelations == true && x.client.city.Contains(anyStringContains))
			       || (includeRelations == true && x.client.postalCode.Contains(anyStringContains))
			       || (includeRelations == true && x.client.phone.Contains(anyStringContains))
			       || (includeRelations == true && x.client.email.Contains(anyStringContains))
			       || (includeRelations == true && x.client.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.client.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.client.color.Contains(anyStringContains))
			       || (includeRelations == true && x.client.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.client.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.client.avatarMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.crew.name.Contains(anyStringContains))
			       || (includeRelations == true && x.crew.description.Contains(anyStringContains))
			       || (includeRelations == true && x.crew.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.crew.color.Contains(anyStringContains))
			       || (includeRelations == true && x.crew.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.crew.avatarMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.eventStatus.name.Contains(anyStringContains))
			       || (includeRelations == true && x.eventStatus.description.Contains(anyStringContains))
			       || (includeRelations == true && x.eventStatus.color.Contains(anyStringContains))
			       || (includeRelations == true && x.eventType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.eventType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.eventType.color.Contains(anyStringContains))
			       || (includeRelations == true && x.office.name.Contains(anyStringContains))
			       || (includeRelations == true && x.office.description.Contains(anyStringContains))
			       || (includeRelations == true && x.office.addressLine1.Contains(anyStringContains))
			       || (includeRelations == true && x.office.addressLine2.Contains(anyStringContains))
			       || (includeRelations == true && x.office.city.Contains(anyStringContains))
			       || (includeRelations == true && x.office.postalCode.Contains(anyStringContains))
			       || (includeRelations == true && x.office.phone.Contains(anyStringContains))
			       || (includeRelations == true && x.office.email.Contains(anyStringContains))
			       || (includeRelations == true && x.office.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.office.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.office.color.Contains(anyStringContains))
			       || (includeRelations == true && x.office.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.office.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.office.avatarMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.parentScheduledEvent.name.Contains(anyStringContains))
			       || (includeRelations == true && x.parentScheduledEvent.description.Contains(anyStringContains))
			       || (includeRelations == true && x.parentScheduledEvent.location.Contains(anyStringContains))
			       || (includeRelations == true && x.parentScheduledEvent.bookingContactName.Contains(anyStringContains))
			       || (includeRelations == true && x.parentScheduledEvent.bookingContactEmail.Contains(anyStringContains))
			       || (includeRelations == true && x.parentScheduledEvent.bookingContactPhone.Contains(anyStringContains))
			       || (includeRelations == true && x.parentScheduledEvent.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.parentScheduledEvent.color.Contains(anyStringContains))
			       || (includeRelations == true && x.parentScheduledEvent.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.parentScheduledEvent.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.priority.name.Contains(anyStringContains))
			       || (includeRelations == true && x.priority.description.Contains(anyStringContains))
			       || (includeRelations == true && x.priority.color.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.name.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.description.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.color.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.avatarMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEventTemplate.name.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEventTemplate.description.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEventTemplate.defaultLocationPattern.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.name.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.description.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.color.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.avatarMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.name.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.description.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.ianaTimeZone.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.abbreviation.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.abbreviationDaylightSavings.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.bookingSourceType);
				query = query.Include(x => x.client);
				query = query.Include(x => x.crew);
				query = query.Include(x => x.eventStatus);
				query = query.Include(x => x.eventType);
				query = query.Include(x => x.office);
				query = query.Include(x => x.parentScheduledEvent);
				query = query.Include(x => x.priority);
				query = query.Include(x => x.recurrenceRule);
				query = query.Include(x => x.resource);
				query = query.Include(x => x.scheduledEventTemplate);
				query = query.Include(x => x.schedulingTarget);
				query = query.Include(x => x.timeZone);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.ScheduledEvent> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ScheduledEvent scheduledEvent in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(scheduledEvent, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ScheduledEvent Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ScheduledEvent Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
		
*/
		
/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This returns a row count of ScheduledEvents filtered by the parameters provided.  Its query is similar to the GetScheduledEvents method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEvents/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? officeId = null,
			int? clientId = null,
			int? scheduledEventTemplateId = null,
			int? recurrenceRuleId = null,
			int? schedulingTargetId = null,
			int? timeZoneId = null,
			int? parentScheduledEventId = null,
			DateTime? recurrenceInstanceDate = null,
			string name = null,
			string description = null,
			bool? isAllDay = null,
			DateTime? startDateTime = null,
			DateTime? endDateTime = null,
			string location = null,
			int? eventStatusId = null,
			int? resourceId = null,
			int? crewId = null,
			int? priorityId = null,
			int? bookingSourceTypeId = null,
			int? eventTypeId = null,
			int? partySize = null,
			string bookingContactName = null,
			string bookingContactEmail = null,
			string bookingContactPhone = null,
			string notes = null,
			string color = null,
			string externalId = null,
			string attributes = null,
			bool? isOpenForVolunteers = null,
			int? maxVolunteerSlots = null,
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
			if (recurrenceInstanceDate.HasValue == true && recurrenceInstanceDate.Value.Kind != DateTimeKind.Utc)
			{
				recurrenceInstanceDate = recurrenceInstanceDate.Value.ToUniversalTime();
			}

			if (startDateTime.HasValue == true && startDateTime.Value.Kind != DateTimeKind.Utc)
			{
				startDateTime = startDateTime.Value.ToUniversalTime();
			}

			if (endDateTime.HasValue == true && endDateTime.Value.Kind != DateTimeKind.Utc)
			{
				endDateTime = endDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.ScheduledEvent> query = (from se in _context.ScheduledEvents select se);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (officeId.HasValue == true)
			{
				query = query.Where(se => se.officeId == officeId.Value);
			}
			if (clientId.HasValue == true)
			{
				query = query.Where(se => se.clientId == clientId.Value);
			}
			if (scheduledEventTemplateId.HasValue == true)
			{
				query = query.Where(se => se.scheduledEventTemplateId == scheduledEventTemplateId.Value);
			}
			if (recurrenceRuleId.HasValue == true)
			{
				query = query.Where(se => se.recurrenceRuleId == recurrenceRuleId.Value);
			}
			if (schedulingTargetId.HasValue == true)
			{
				query = query.Where(se => se.schedulingTargetId == schedulingTargetId.Value);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(se => se.timeZoneId == timeZoneId.Value);
			}
			if (parentScheduledEventId.HasValue == true)
			{
				query = query.Where(se => se.parentScheduledEventId == parentScheduledEventId.Value);
			}
			if (recurrenceInstanceDate.HasValue == true)
			{
				query = query.Where(se => se.recurrenceInstanceDate == recurrenceInstanceDate.Value);
			}
			if (name != null)
			{
				query = query.Where(se => se.name == name);
			}
			if (description != null)
			{
				query = query.Where(se => se.description == description);
			}
			if (isAllDay.HasValue == true)
			{
				query = query.Where(se => se.isAllDay == isAllDay.Value);
			}
			if (startDateTime.HasValue == true)
			{
				query = query.Where(se => se.startDateTime == startDateTime.Value);
			}
			if (endDateTime.HasValue == true)
			{
				query = query.Where(se => se.endDateTime == endDateTime.Value);
			}
			if (location != null)
			{
				query = query.Where(se => se.location == location);
			}
			if (eventStatusId.HasValue == true)
			{
				query = query.Where(se => se.eventStatusId == eventStatusId.Value);
			}
			if (resourceId.HasValue == true)
			{
				query = query.Where(se => se.resourceId == resourceId.Value);
			}
			if (crewId.HasValue == true)
			{
				query = query.Where(se => se.crewId == crewId.Value);
			}
			if (priorityId.HasValue == true)
			{
				query = query.Where(se => se.priorityId == priorityId.Value);
			}
			if (bookingSourceTypeId.HasValue == true)
			{
				query = query.Where(se => se.bookingSourceTypeId == bookingSourceTypeId.Value);
			}
			if (eventTypeId.HasValue == true)
			{
				query = query.Where(se => se.eventTypeId == eventTypeId.Value);
			}
			if (partySize.HasValue == true)
			{
				query = query.Where(se => se.partySize == partySize.Value);
			}
			if (bookingContactName != null)
			{
				query = query.Where(se => se.bookingContactName == bookingContactName);
			}
			if (bookingContactEmail != null)
			{
				query = query.Where(se => se.bookingContactEmail == bookingContactEmail);
			}
			if (bookingContactPhone != null)
			{
				query = query.Where(se => se.bookingContactPhone == bookingContactPhone);
			}
			if (notes != null)
			{
				query = query.Where(se => se.notes == notes);
			}
			if (color != null)
			{
				query = query.Where(se => se.color == color);
			}
			if (externalId != null)
			{
				query = query.Where(se => se.externalId == externalId);
			}
			if (attributes != null)
			{
				query = query.Where(se => se.attributes == attributes);
			}
			if (isOpenForVolunteers.HasValue == true)
			{
				query = query.Where(se => se.isOpenForVolunteers == isOpenForVolunteers.Value);
			}
			if (maxVolunteerSlots.HasValue == true)
			{
				query = query.Where(se => se.maxVolunteerSlots == maxVolunteerSlots.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(se => se.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(se => se.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(se => se.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(se => se.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(se => se.deleted == false);
				}
			}
			else
			{
				query = query.Where(se => se.active == true);
				query = query.Where(se => se.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Scheduled Event, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.location.Contains(anyStringContains)
			       || x.bookingContactName.Contains(anyStringContains)
			       || x.bookingContactEmail.Contains(anyStringContains)
			       || x.bookingContactPhone.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.attributes.Contains(anyStringContains)
			       || x.bookingSourceType.name.Contains(anyStringContains)
			       || x.bookingSourceType.description.Contains(anyStringContains)
			       || x.bookingSourceType.color.Contains(anyStringContains)
			       || x.client.name.Contains(anyStringContains)
			       || x.client.description.Contains(anyStringContains)
			       || x.client.addressLine1.Contains(anyStringContains)
			       || x.client.addressLine2.Contains(anyStringContains)
			       || x.client.city.Contains(anyStringContains)
			       || x.client.postalCode.Contains(anyStringContains)
			       || x.client.phone.Contains(anyStringContains)
			       || x.client.email.Contains(anyStringContains)
			       || x.client.notes.Contains(anyStringContains)
			       || x.client.externalId.Contains(anyStringContains)
			       || x.client.color.Contains(anyStringContains)
			       || x.client.attributes.Contains(anyStringContains)
			       || x.client.avatarFileName.Contains(anyStringContains)
			       || x.client.avatarMimeType.Contains(anyStringContains)
			       || x.crew.name.Contains(anyStringContains)
			       || x.crew.description.Contains(anyStringContains)
			       || x.crew.notes.Contains(anyStringContains)
			       || x.crew.color.Contains(anyStringContains)
			       || x.crew.avatarFileName.Contains(anyStringContains)
			       || x.crew.avatarMimeType.Contains(anyStringContains)
			       || x.eventStatus.name.Contains(anyStringContains)
			       || x.eventStatus.description.Contains(anyStringContains)
			       || x.eventStatus.color.Contains(anyStringContains)
			       || x.eventType.name.Contains(anyStringContains)
			       || x.eventType.description.Contains(anyStringContains)
			       || x.eventType.color.Contains(anyStringContains)
			       || x.office.name.Contains(anyStringContains)
			       || x.office.description.Contains(anyStringContains)
			       || x.office.addressLine1.Contains(anyStringContains)
			       || x.office.addressLine2.Contains(anyStringContains)
			       || x.office.city.Contains(anyStringContains)
			       || x.office.postalCode.Contains(anyStringContains)
			       || x.office.phone.Contains(anyStringContains)
			       || x.office.email.Contains(anyStringContains)
			       || x.office.notes.Contains(anyStringContains)
			       || x.office.externalId.Contains(anyStringContains)
			       || x.office.color.Contains(anyStringContains)
			       || x.office.attributes.Contains(anyStringContains)
			       || x.office.avatarFileName.Contains(anyStringContains)
			       || x.office.avatarMimeType.Contains(anyStringContains)
			       || x.parentScheduledEvent.name.Contains(anyStringContains)
			       || x.parentScheduledEvent.description.Contains(anyStringContains)
			       || x.parentScheduledEvent.location.Contains(anyStringContains)
			       || x.parentScheduledEvent.bookingContactName.Contains(anyStringContains)
			       || x.parentScheduledEvent.bookingContactEmail.Contains(anyStringContains)
			       || x.parentScheduledEvent.bookingContactPhone.Contains(anyStringContains)
			       || x.parentScheduledEvent.notes.Contains(anyStringContains)
			       || x.parentScheduledEvent.color.Contains(anyStringContains)
			       || x.parentScheduledEvent.externalId.Contains(anyStringContains)
			       || x.parentScheduledEvent.attributes.Contains(anyStringContains)
			       || x.priority.name.Contains(anyStringContains)
			       || x.priority.description.Contains(anyStringContains)
			       || x.priority.color.Contains(anyStringContains)
			       || x.resource.name.Contains(anyStringContains)
			       || x.resource.description.Contains(anyStringContains)
			       || x.resource.notes.Contains(anyStringContains)
			       || x.resource.externalId.Contains(anyStringContains)
			       || x.resource.color.Contains(anyStringContains)
			       || x.resource.attributes.Contains(anyStringContains)
			       || x.resource.avatarFileName.Contains(anyStringContains)
			       || x.resource.avatarMimeType.Contains(anyStringContains)
			       || x.scheduledEventTemplate.name.Contains(anyStringContains)
			       || x.scheduledEventTemplate.description.Contains(anyStringContains)
			       || x.scheduledEventTemplate.defaultLocationPattern.Contains(anyStringContains)
			       || x.schedulingTarget.name.Contains(anyStringContains)
			       || x.schedulingTarget.description.Contains(anyStringContains)
			       || x.schedulingTarget.notes.Contains(anyStringContains)
			       || x.schedulingTarget.externalId.Contains(anyStringContains)
			       || x.schedulingTarget.color.Contains(anyStringContains)
			       || x.schedulingTarget.attributes.Contains(anyStringContains)
			       || x.schedulingTarget.avatarFileName.Contains(anyStringContains)
			       || x.schedulingTarget.avatarMimeType.Contains(anyStringContains)
			       || x.timeZone.name.Contains(anyStringContains)
			       || x.timeZone.description.Contains(anyStringContains)
			       || x.timeZone.ianaTimeZone.Contains(anyStringContains)
			       || x.timeZone.abbreviation.Contains(anyStringContains)
			       || x.timeZone.abbreviationDaylightSavings.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}

*/

        /// <summary>
        /// 
        /// This gets a single ScheduledEvent by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEvent/{id}")]
		public async Task<IActionResult> GetScheduledEvent(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ScheduledEvent> query = (from se in _context.ScheduledEvents where
							(se.id == id) &&
							(userIsAdmin == true || se.deleted == false) &&
							(userIsWriter == true || se.active == true)
					select se);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.bookingSourceType);
					query = query.Include(x => x.client);
					query = query.Include(x => x.crew);
					query = query.Include(x => x.eventStatus);
					query = query.Include(x => x.eventType);
					query = query.Include(x => x.office);
					query = query.Include(x => x.parentScheduledEvent);
					query = query.Include(x => x.priority);
					query = query.Include(x => x.recurrenceRule);
					query = query.Include(x => x.resource);
					query = query.Include(x => x.scheduledEventTemplate);
					query = query.Include(x => x.schedulingTarget);
					query = query.Include(x => x.timeZone);
					query = query.AsSplitQuery();
				}

				Database.ScheduledEvent materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.ScheduledEvent Entity was read with Admin privilege." : "Scheduler.ScheduledEvent Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ScheduledEvent", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.ScheduledEvent entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.ScheduledEvent.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.ScheduledEvent.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ScheduledEvent record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ScheduledEvent/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutScheduledEvent(int id, [FromBody]Database.ScheduledEvent.ScheduledEventDTO scheduledEventDTO, CancellationToken cancellationToken = default)
		{
			if (scheduledEventDTO == null)
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



			if (id != scheduledEventDTO.id)
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


			IQueryable<Database.ScheduledEvent> query = (from x in _context.ScheduledEvents
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ScheduledEvent existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ScheduledEvent PUT", id.ToString(), new Exception("No Scheduler.ScheduledEvent entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (scheduledEventDTO.objectGuid == Guid.Empty)
            {
                scheduledEventDTO.objectGuid = existing.objectGuid;
            }
            else if (scheduledEventDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ScheduledEvent record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ScheduledEvent cloneOfExisting = (Database.ScheduledEvent)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ScheduledEvent object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ScheduledEvent scheduledEvent = (Database.ScheduledEvent)_context.Entry(existing).GetDatabaseValues().ToObject();
			scheduledEvent.ApplyDTO(scheduledEventDTO);
			//
			// The tenant guid for any ScheduledEvent being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ScheduledEvent because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				scheduledEvent.tenantGuid = existing.tenantGuid;
			}

			lock (scheduledEventPutSyncRoot)
			{
				//
				// Validate the version number for the scheduledEvent being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != scheduledEvent.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ScheduledEvent save attempt was made but save request was with version " + scheduledEvent.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ScheduledEvent you are trying to update has already changed.  Please try your save again after reloading the ScheduledEvent.");
				}
				else
				{
					// Same record.  Increase version.
					scheduledEvent.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (scheduledEvent.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.ScheduledEvent record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (scheduledEvent.recurrenceInstanceDate.HasValue == true && scheduledEvent.recurrenceInstanceDate.Value.Kind != DateTimeKind.Utc)
				{
					scheduledEvent.recurrenceInstanceDate = scheduledEvent.recurrenceInstanceDate.Value.ToUniversalTime();
				}

				if (scheduledEvent.name != null && scheduledEvent.name.Length > 100)
				{
					scheduledEvent.name = scheduledEvent.name.Substring(0, 100);
				}

				if (scheduledEvent.description != null && scheduledEvent.description.Length > 500)
				{
					scheduledEvent.description = scheduledEvent.description.Substring(0, 500);
				}

				if (scheduledEvent.startDateTime.Kind != DateTimeKind.Utc)
				{
					scheduledEvent.startDateTime = scheduledEvent.startDateTime.ToUniversalTime();
				}

				if (scheduledEvent.endDateTime.Kind != DateTimeKind.Utc)
				{
					scheduledEvent.endDateTime = scheduledEvent.endDateTime.ToUniversalTime();
				}

				if (scheduledEvent.location != null && scheduledEvent.location.Length > 250)
				{
					scheduledEvent.location = scheduledEvent.location.Substring(0, 250);
				}

				if (scheduledEvent.bookingContactName != null && scheduledEvent.bookingContactName.Length > 250)
				{
					scheduledEvent.bookingContactName = scheduledEvent.bookingContactName.Substring(0, 250);
				}

				if (scheduledEvent.bookingContactEmail != null && scheduledEvent.bookingContactEmail.Length > 250)
				{
					scheduledEvent.bookingContactEmail = scheduledEvent.bookingContactEmail.Substring(0, 250);
				}

				if (scheduledEvent.bookingContactPhone != null && scheduledEvent.bookingContactPhone.Length > 50)
				{
					scheduledEvent.bookingContactPhone = scheduledEvent.bookingContactPhone.Substring(0, 50);
				}

				if (scheduledEvent.color != null && scheduledEvent.color.Length > 10)
				{
					scheduledEvent.color = scheduledEvent.color.Substring(0, 10);
				}

				if (scheduledEvent.externalId != null && scheduledEvent.externalId.Length > 100)
				{
					scheduledEvent.externalId = scheduledEvent.externalId.Substring(0, 100);
				}

				try
				{
				    EntityEntry<Database.ScheduledEvent> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(scheduledEvent);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ScheduledEventChangeHistory scheduledEventChangeHistory = new ScheduledEventChangeHistory();
				        scheduledEventChangeHistory.scheduledEventId = scheduledEvent.id;
				        scheduledEventChangeHistory.versionNumber = scheduledEvent.versionNumber;
				        scheduledEventChangeHistory.timeStamp = DateTime.UtcNow;
				        scheduledEventChangeHistory.userId = securityUser.id;
				        scheduledEventChangeHistory.tenantGuid = userTenantGuid;
				        scheduledEventChangeHistory.data = JsonSerializer.Serialize(Database.ScheduledEvent.CreateAnonymousWithFirstLevelSubObjects(scheduledEvent));
				        _context.ScheduledEventChangeHistories.Add(scheduledEventChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ScheduledEvent entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduledEvent.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduledEvent.CreateAnonymousWithFirstLevelSubObjects(scheduledEvent)),
						null);

				return Ok(Database.ScheduledEvent.CreateAnonymous(scheduledEvent));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ScheduledEvent entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduledEvent.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduledEvent.CreateAnonymousWithFirstLevelSubObjects(scheduledEvent)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ScheduledEvent record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEvent", Name = "ScheduledEvent")]
		public async Task<IActionResult> PostScheduledEvent([FromBody]Database.ScheduledEvent.ScheduledEventDTO scheduledEventDTO, CancellationToken cancellationToken = default)
		{
			if (scheduledEventDTO == null)
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
			// Create a new ScheduledEvent object using the data from the DTO
			//
			Database.ScheduledEvent scheduledEvent = Database.ScheduledEvent.FromDTO(scheduledEventDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				scheduledEvent.tenantGuid = userTenantGuid;

				if (scheduledEvent.recurrenceInstanceDate.HasValue == true && scheduledEvent.recurrenceInstanceDate.Value.Kind != DateTimeKind.Utc)
				{
					scheduledEvent.recurrenceInstanceDate = scheduledEvent.recurrenceInstanceDate.Value.ToUniversalTime();
				}

				if (scheduledEvent.name != null && scheduledEvent.name.Length > 100)
				{
					scheduledEvent.name = scheduledEvent.name.Substring(0, 100);
				}

				if (scheduledEvent.description != null && scheduledEvent.description.Length > 500)
				{
					scheduledEvent.description = scheduledEvent.description.Substring(0, 500);
				}

				if (scheduledEvent.startDateTime.Kind != DateTimeKind.Utc)
				{
					scheduledEvent.startDateTime = scheduledEvent.startDateTime.ToUniversalTime();
				}

				if (scheduledEvent.endDateTime.Kind != DateTimeKind.Utc)
				{
					scheduledEvent.endDateTime = scheduledEvent.endDateTime.ToUniversalTime();
				}

				if (scheduledEvent.location != null && scheduledEvent.location.Length > 250)
				{
					scheduledEvent.location = scheduledEvent.location.Substring(0, 250);
				}

				if (scheduledEvent.bookingContactName != null && scheduledEvent.bookingContactName.Length > 250)
				{
					scheduledEvent.bookingContactName = scheduledEvent.bookingContactName.Substring(0, 250);
				}

				if (scheduledEvent.bookingContactEmail != null && scheduledEvent.bookingContactEmail.Length > 250)
				{
					scheduledEvent.bookingContactEmail = scheduledEvent.bookingContactEmail.Substring(0, 250);
				}

				if (scheduledEvent.bookingContactPhone != null && scheduledEvent.bookingContactPhone.Length > 50)
				{
					scheduledEvent.bookingContactPhone = scheduledEvent.bookingContactPhone.Substring(0, 50);
				}

				if (scheduledEvent.color != null && scheduledEvent.color.Length > 10)
				{
					scheduledEvent.color = scheduledEvent.color.Substring(0, 10);
				}

				if (scheduledEvent.externalId != null && scheduledEvent.externalId.Length > 100)
				{
					scheduledEvent.externalId = scheduledEvent.externalId.Substring(0, 100);
				}

				scheduledEvent.objectGuid = Guid.NewGuid();
				scheduledEvent.versionNumber = 1;

				_context.ScheduledEvents.Add(scheduledEvent);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the scheduledEvent object so that no further changes will be written to the database
				    //
				    _context.Entry(scheduledEvent).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					scheduledEvent.ContactInteractions = null;
					scheduledEvent.Documents = null;
					scheduledEvent.EventCalendars = null;
					scheduledEvent.EventCharges = null;
					scheduledEvent.EventResourceAssignments = null;
					scheduledEvent.FinancialTransactions = null;
					scheduledEvent.InverseparentScheduledEvent = null;
					scheduledEvent.Invoices = null;
					scheduledEvent.PaymentTransactions = null;
					scheduledEvent.RecurrenceExceptions = null;
					scheduledEvent.ScheduledEventChangeHistories = null;
					scheduledEvent.ScheduledEventDependencypredecessorEvents = null;
					scheduledEvent.ScheduledEventDependencysuccessorEvents = null;
					scheduledEvent.ScheduledEventQualificationRequirements = null;
					scheduledEvent.bookingSourceType = null;
					scheduledEvent.client = null;
					scheduledEvent.crew = null;
					scheduledEvent.eventStatus = null;
					scheduledEvent.eventType = null;
					scheduledEvent.office = null;
					scheduledEvent.parentScheduledEvent = null;
					scheduledEvent.priority = null;
					scheduledEvent.recurrenceRule = null;
					scheduledEvent.resource = null;
					scheduledEvent.scheduledEventTemplate = null;
					scheduledEvent.schedulingTarget = null;
					scheduledEvent.timeZone = null;


				    ScheduledEventChangeHistory scheduledEventChangeHistory = new ScheduledEventChangeHistory();
				    scheduledEventChangeHistory.scheduledEventId = scheduledEvent.id;
				    scheduledEventChangeHistory.versionNumber = scheduledEvent.versionNumber;
				    scheduledEventChangeHistory.timeStamp = DateTime.UtcNow;
				    scheduledEventChangeHistory.userId = securityUser.id;
				    scheduledEventChangeHistory.tenantGuid = userTenantGuid;
				    scheduledEventChangeHistory.data = JsonSerializer.Serialize(Database.ScheduledEvent.CreateAnonymousWithFirstLevelSubObjects(scheduledEvent));
				    _context.ScheduledEventChangeHistories.Add(scheduledEventChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.ScheduledEvent entity successfully created.",
						true,
						scheduledEvent. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ScheduledEvent.CreateAnonymousWithFirstLevelSubObjects(scheduledEvent)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.ScheduledEvent entity creation failed.", false, scheduledEvent.id.ToString(), "", JsonSerializer.Serialize(Database.ScheduledEvent.CreateAnonymousWithFirstLevelSubObjects(scheduledEvent)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ScheduledEvent", scheduledEvent.id, scheduledEvent.name));

			return CreatedAtRoute("ScheduledEvent", new { id = scheduledEvent.id }, Database.ScheduledEvent.CreateAnonymousWithFirstLevelSubObjects(scheduledEvent));
		}



        /// <summary>
        /// 
        /// This rolls a ScheduledEvent entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEvent/Rollback/{id}")]
		[Route("api/ScheduledEvent/Rollback")]
		public async Task<IActionResult> RollbackToScheduledEventVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.ScheduledEvent> query = (from x in _context.ScheduledEvents
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this ScheduledEvent concurrently
			//
			lock (scheduledEventPutSyncRoot)
			{
				
				Database.ScheduledEvent scheduledEvent = query.FirstOrDefault();
				
				if (scheduledEvent == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ScheduledEvent rollback", id.ToString(), new Exception("No Scheduler.ScheduledEvent entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ScheduledEvent current state so we can log it.
				//
				Database.ScheduledEvent cloneOfExisting = (Database.ScheduledEvent)_context.Entry(scheduledEvent).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.ContactInteractions = null;
				cloneOfExisting.Documents = null;
				cloneOfExisting.EventCalendars = null;
				cloneOfExisting.EventCharges = null;
				cloneOfExisting.EventResourceAssignments = null;
				cloneOfExisting.FinancialTransactions = null;
				cloneOfExisting.InverseparentScheduledEvent = null;
				cloneOfExisting.Invoices = null;
				cloneOfExisting.PaymentTransactions = null;
				cloneOfExisting.RecurrenceExceptions = null;
				cloneOfExisting.ScheduledEventChangeHistories = null;
				cloneOfExisting.ScheduledEventDependencypredecessorEvents = null;
				cloneOfExisting.ScheduledEventDependencysuccessorEvents = null;
				cloneOfExisting.ScheduledEventQualificationRequirements = null;
				cloneOfExisting.bookingSourceType = null;
				cloneOfExisting.client = null;
				cloneOfExisting.crew = null;
				cloneOfExisting.eventStatus = null;
				cloneOfExisting.eventType = null;
				cloneOfExisting.office = null;
				cloneOfExisting.parentScheduledEvent = null;
				cloneOfExisting.priority = null;
				cloneOfExisting.recurrenceRule = null;
				cloneOfExisting.resource = null;
				cloneOfExisting.scheduledEventTemplate = null;
				cloneOfExisting.schedulingTarget = null;
				cloneOfExisting.timeZone = null;

				if (versionNumber >= scheduledEvent.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.ScheduledEvent rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.ScheduledEvent rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ScheduledEventChangeHistory scheduledEventChangeHistory = (from x in _context.ScheduledEventChangeHistories
				                                               where
				                                               x.scheduledEventId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (scheduledEventChangeHistory != null)
				{
				    Database.ScheduledEvent oldScheduledEvent = JsonSerializer.Deserialize<Database.ScheduledEvent>(scheduledEventChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    scheduledEvent.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    scheduledEvent.officeId = oldScheduledEvent.officeId;
				    scheduledEvent.clientId = oldScheduledEvent.clientId;
				    scheduledEvent.scheduledEventTemplateId = oldScheduledEvent.scheduledEventTemplateId;
				    scheduledEvent.recurrenceRuleId = oldScheduledEvent.recurrenceRuleId;
				    scheduledEvent.schedulingTargetId = oldScheduledEvent.schedulingTargetId;
				    scheduledEvent.timeZoneId = oldScheduledEvent.timeZoneId;
				    scheduledEvent.parentScheduledEventId = oldScheduledEvent.parentScheduledEventId;
				    scheduledEvent.recurrenceInstanceDate = oldScheduledEvent.recurrenceInstanceDate;
				    scheduledEvent.name = oldScheduledEvent.name;
				    scheduledEvent.description = oldScheduledEvent.description;
				    scheduledEvent.isAllDay = oldScheduledEvent.isAllDay;
				    scheduledEvent.startDateTime = oldScheduledEvent.startDateTime;
				    scheduledEvent.endDateTime = oldScheduledEvent.endDateTime;
				    scheduledEvent.location = oldScheduledEvent.location;
				    scheduledEvent.eventStatusId = oldScheduledEvent.eventStatusId;
				    scheduledEvent.resourceId = oldScheduledEvent.resourceId;
				    scheduledEvent.crewId = oldScheduledEvent.crewId;
				    scheduledEvent.priorityId = oldScheduledEvent.priorityId;
				    scheduledEvent.bookingSourceTypeId = oldScheduledEvent.bookingSourceTypeId;
				    scheduledEvent.eventTypeId = oldScheduledEvent.eventTypeId;
				    scheduledEvent.partySize = oldScheduledEvent.partySize;
				    scheduledEvent.bookingContactName = oldScheduledEvent.bookingContactName;
				    scheduledEvent.bookingContactEmail = oldScheduledEvent.bookingContactEmail;
				    scheduledEvent.bookingContactPhone = oldScheduledEvent.bookingContactPhone;
				    scheduledEvent.notes = oldScheduledEvent.notes;
				    scheduledEvent.color = oldScheduledEvent.color;
				    scheduledEvent.externalId = oldScheduledEvent.externalId;
				    scheduledEvent.attributes = oldScheduledEvent.attributes;
				    scheduledEvent.isOpenForVolunteers = oldScheduledEvent.isOpenForVolunteers;
				    scheduledEvent.maxVolunteerSlots = oldScheduledEvent.maxVolunteerSlots;
				    scheduledEvent.objectGuid = oldScheduledEvent.objectGuid;
				    scheduledEvent.active = oldScheduledEvent.active;
				    scheduledEvent.deleted = oldScheduledEvent.deleted;

				    string serializedScheduledEvent = JsonSerializer.Serialize(scheduledEvent);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ScheduledEventChangeHistory newScheduledEventChangeHistory = new ScheduledEventChangeHistory();
				        newScheduledEventChangeHistory.scheduledEventId = scheduledEvent.id;
				        newScheduledEventChangeHistory.versionNumber = scheduledEvent.versionNumber;
				        newScheduledEventChangeHistory.timeStamp = DateTime.UtcNow;
				        newScheduledEventChangeHistory.userId = securityUser.id;
				        newScheduledEventChangeHistory.tenantGuid = userTenantGuid;
				        newScheduledEventChangeHistory.data = JsonSerializer.Serialize(Database.ScheduledEvent.CreateAnonymousWithFirstLevelSubObjects(scheduledEvent));
				        _context.ScheduledEventChangeHistories.Add(newScheduledEventChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ScheduledEvent rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduledEvent.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduledEvent.CreateAnonymousWithFirstLevelSubObjects(scheduledEvent)),
						null);


				    return Ok(Database.ScheduledEvent.CreateAnonymous(scheduledEvent));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.ScheduledEvent rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.ScheduledEvent rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a ScheduledEvent.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduledEvent</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEvent/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetScheduledEventChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.ScheduledEvent scheduledEvent = await _context.ScheduledEvents.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduledEvent == null)
			{
				return NotFound();
			}

			try
			{
				scheduledEvent.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ScheduledEvent> versionInfo = await scheduledEvent.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a ScheduledEvent.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduledEvent</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEvent/{id}/AuditHistory")]
		public async Task<IActionResult> GetScheduledEventAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.ScheduledEvent scheduledEvent = await _context.ScheduledEvents.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduledEvent == null)
			{
				return NotFound();
			}

			try
			{
				scheduledEvent.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.ScheduledEvent>> versions = await scheduledEvent.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a ScheduledEvent.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduledEvent</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The ScheduledEvent object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEvent/{id}/Version/{version}")]
		public async Task<IActionResult> GetScheduledEventVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.ScheduledEvent scheduledEvent = await _context.ScheduledEvents.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduledEvent == null)
			{
				return NotFound();
			}

			try
			{
				scheduledEvent.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ScheduledEvent> versionInfo = await scheduledEvent.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a ScheduledEvent at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduledEvent</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The ScheduledEvent object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEvent/{id}/StateAtTime")]
		public async Task<IActionResult> GetScheduledEventStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.ScheduledEvent scheduledEvent = await _context.ScheduledEvents.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduledEvent == null)
			{
				return NotFound();
			}

			try
			{
				scheduledEvent.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ScheduledEvent> versionInfo = await scheduledEvent.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a ScheduledEvent record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEvent/{id}")]
		[Route("api/ScheduledEvent")]
		public async Task<IActionResult> DeleteScheduledEvent(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ScheduledEvent> query = (from x in _context.ScheduledEvents
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ScheduledEvent scheduledEvent = await query.FirstOrDefaultAsync(cancellationToken);

			if (scheduledEvent == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ScheduledEvent DELETE", id.ToString(), new Exception("No Scheduler.ScheduledEvent entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ScheduledEvent cloneOfExisting = (Database.ScheduledEvent)_context.Entry(scheduledEvent).GetDatabaseValues().ToObject();


			lock (scheduledEventDeleteSyncRoot)
			{
			    try
			    {
			        scheduledEvent.deleted = true;
			        scheduledEvent.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ScheduledEventChangeHistory scheduledEventChangeHistory = new ScheduledEventChangeHistory();
			        scheduledEventChangeHistory.scheduledEventId = scheduledEvent.id;
			        scheduledEventChangeHistory.versionNumber = scheduledEvent.versionNumber;
			        scheduledEventChangeHistory.timeStamp = DateTime.UtcNow;
			        scheduledEventChangeHistory.userId = securityUser.id;
			        scheduledEventChangeHistory.tenantGuid = userTenantGuid;
			        scheduledEventChangeHistory.data = JsonSerializer.Serialize(Database.ScheduledEvent.CreateAnonymousWithFirstLevelSubObjects(scheduledEvent));
			        _context.ScheduledEventChangeHistories.Add(scheduledEventChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ScheduledEvent entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduledEvent.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduledEvent.CreateAnonymousWithFirstLevelSubObjects(scheduledEvent)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ScheduledEvent entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduledEvent.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduledEvent.CreateAnonymousWithFirstLevelSubObjects(scheduledEvent)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This gets a list of ScheduledEvent records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ScheduledEvents/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? officeId = null,
			int? clientId = null,
			int? scheduledEventTemplateId = null,
			int? recurrenceRuleId = null,
			int? schedulingTargetId = null,
			int? timeZoneId = null,
			int? parentScheduledEventId = null,
			DateTime? recurrenceInstanceDate = null,
			string name = null,
			string description = null,
			bool? isAllDay = null,
			DateTime? startDateTime = null,
			DateTime? endDateTime = null,
			string location = null,
			int? eventStatusId = null,
			int? resourceId = null,
			int? crewId = null,
			int? priorityId = null,
			int? bookingSourceTypeId = null,
			int? eventTypeId = null,
			int? partySize = null,
			string bookingContactName = null,
			string bookingContactEmail = null,
			string bookingContactPhone = null,
			string notes = null,
			string color = null,
			string externalId = null,
			string attributes = null,
			bool? isOpenForVolunteers = null,
			int? maxVolunteerSlots = null,
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
			if (recurrenceInstanceDate.HasValue == true && recurrenceInstanceDate.Value.Kind != DateTimeKind.Utc)
			{
				recurrenceInstanceDate = recurrenceInstanceDate.Value.ToUniversalTime();
			}

			if (startDateTime.HasValue == true && startDateTime.Value.Kind != DateTimeKind.Utc)
			{
				startDateTime = startDateTime.Value.ToUniversalTime();
			}

			if (endDateTime.HasValue == true && endDateTime.Value.Kind != DateTimeKind.Utc)
			{
				endDateTime = endDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.ScheduledEvent> query = (from se in _context.ScheduledEvents select se);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (officeId.HasValue == true)
			{
				query = query.Where(se => se.officeId == officeId.Value);
			}
			if (clientId.HasValue == true)
			{
				query = query.Where(se => se.clientId == clientId.Value);
			}
			if (scheduledEventTemplateId.HasValue == true)
			{
				query = query.Where(se => se.scheduledEventTemplateId == scheduledEventTemplateId.Value);
			}
			if (recurrenceRuleId.HasValue == true)
			{
				query = query.Where(se => se.recurrenceRuleId == recurrenceRuleId.Value);
			}
			if (schedulingTargetId.HasValue == true)
			{
				query = query.Where(se => se.schedulingTargetId == schedulingTargetId.Value);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(se => se.timeZoneId == timeZoneId.Value);
			}
			if (parentScheduledEventId.HasValue == true)
			{
				query = query.Where(se => se.parentScheduledEventId == parentScheduledEventId.Value);
			}
			if (recurrenceInstanceDate.HasValue == true)
			{
				query = query.Where(se => se.recurrenceInstanceDate == recurrenceInstanceDate.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(se => se.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(se => se.description == description);
			}
			if (isAllDay.HasValue == true)
			{
				query = query.Where(se => se.isAllDay == isAllDay.Value);
			}
			if (startDateTime.HasValue == true)
			{
				query = query.Where(se => se.startDateTime == startDateTime.Value);
			}
			if (endDateTime.HasValue == true)
			{
				query = query.Where(se => se.endDateTime == endDateTime.Value);
			}
			if (string.IsNullOrEmpty(location) == false)
			{
				query = query.Where(se => se.location == location);
			}
			if (eventStatusId.HasValue == true)
			{
				query = query.Where(se => se.eventStatusId == eventStatusId.Value);
			}
			if (resourceId.HasValue == true)
			{
				query = query.Where(se => se.resourceId == resourceId.Value);
			}
			if (crewId.HasValue == true)
			{
				query = query.Where(se => se.crewId == crewId.Value);
			}
			if (priorityId.HasValue == true)
			{
				query = query.Where(se => se.priorityId == priorityId.Value);
			}
			if (bookingSourceTypeId.HasValue == true)
			{
				query = query.Where(se => se.bookingSourceTypeId == bookingSourceTypeId.Value);
			}
			if (eventTypeId.HasValue == true)
			{
				query = query.Where(se => se.eventTypeId == eventTypeId.Value);
			}
			if (partySize.HasValue == true)
			{
				query = query.Where(se => se.partySize == partySize.Value);
			}
			if (string.IsNullOrEmpty(bookingContactName) == false)
			{
				query = query.Where(se => se.bookingContactName == bookingContactName);
			}
			if (string.IsNullOrEmpty(bookingContactEmail) == false)
			{
				query = query.Where(se => se.bookingContactEmail == bookingContactEmail);
			}
			if (string.IsNullOrEmpty(bookingContactPhone) == false)
			{
				query = query.Where(se => se.bookingContactPhone == bookingContactPhone);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(se => se.notes == notes);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(se => se.color == color);
			}
			if (string.IsNullOrEmpty(externalId) == false)
			{
				query = query.Where(se => se.externalId == externalId);
			}
			if (string.IsNullOrEmpty(attributes) == false)
			{
				query = query.Where(se => se.attributes == attributes);
			}
			if (isOpenForVolunteers.HasValue == true)
			{
				query = query.Where(se => se.isOpenForVolunteers == isOpenForVolunteers.Value);
			}
			if (maxVolunteerSlots.HasValue == true)
			{
				query = query.Where(se => se.maxVolunteerSlots == maxVolunteerSlots.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(se => se.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(se => se.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(se => se.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(se => se.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(se => se.deleted == false);
				}
			}
			else
			{
				query = query.Where(se => se.active == true);
				query = query.Where(se => se.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Scheduled Event, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.location.Contains(anyStringContains)
			       || x.bookingContactName.Contains(anyStringContains)
			       || x.bookingContactEmail.Contains(anyStringContains)
			       || x.bookingContactPhone.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.attributes.Contains(anyStringContains)
			       || x.bookingSourceType.name.Contains(anyStringContains)
			       || x.bookingSourceType.description.Contains(anyStringContains)
			       || x.bookingSourceType.color.Contains(anyStringContains)
			       || x.client.name.Contains(anyStringContains)
			       || x.client.description.Contains(anyStringContains)
			       || x.client.addressLine1.Contains(anyStringContains)
			       || x.client.addressLine2.Contains(anyStringContains)
			       || x.client.city.Contains(anyStringContains)
			       || x.client.postalCode.Contains(anyStringContains)
			       || x.client.phone.Contains(anyStringContains)
			       || x.client.email.Contains(anyStringContains)
			       || x.client.notes.Contains(anyStringContains)
			       || x.client.externalId.Contains(anyStringContains)
			       || x.client.color.Contains(anyStringContains)
			       || x.client.attributes.Contains(anyStringContains)
			       || x.client.avatarFileName.Contains(anyStringContains)
			       || x.client.avatarMimeType.Contains(anyStringContains)
			       || x.crew.name.Contains(anyStringContains)
			       || x.crew.description.Contains(anyStringContains)
			       || x.crew.notes.Contains(anyStringContains)
			       || x.crew.color.Contains(anyStringContains)
			       || x.crew.avatarFileName.Contains(anyStringContains)
			       || x.crew.avatarMimeType.Contains(anyStringContains)
			       || x.eventStatus.name.Contains(anyStringContains)
			       || x.eventStatus.description.Contains(anyStringContains)
			       || x.eventStatus.color.Contains(anyStringContains)
			       || x.eventType.name.Contains(anyStringContains)
			       || x.eventType.description.Contains(anyStringContains)
			       || x.eventType.color.Contains(anyStringContains)
			       || x.office.name.Contains(anyStringContains)
			       || x.office.description.Contains(anyStringContains)
			       || x.office.addressLine1.Contains(anyStringContains)
			       || x.office.addressLine2.Contains(anyStringContains)
			       || x.office.city.Contains(anyStringContains)
			       || x.office.postalCode.Contains(anyStringContains)
			       || x.office.phone.Contains(anyStringContains)
			       || x.office.email.Contains(anyStringContains)
			       || x.office.notes.Contains(anyStringContains)
			       || x.office.externalId.Contains(anyStringContains)
			       || x.office.color.Contains(anyStringContains)
			       || x.office.attributes.Contains(anyStringContains)
			       || x.office.avatarFileName.Contains(anyStringContains)
			       || x.office.avatarMimeType.Contains(anyStringContains)
			       || x.parentScheduledEvent.name.Contains(anyStringContains)
			       || x.parentScheduledEvent.description.Contains(anyStringContains)
			       || x.parentScheduledEvent.location.Contains(anyStringContains)
			       || x.parentScheduledEvent.bookingContactName.Contains(anyStringContains)
			       || x.parentScheduledEvent.bookingContactEmail.Contains(anyStringContains)
			       || x.parentScheduledEvent.bookingContactPhone.Contains(anyStringContains)
			       || x.parentScheduledEvent.notes.Contains(anyStringContains)
			       || x.parentScheduledEvent.color.Contains(anyStringContains)
			       || x.parentScheduledEvent.externalId.Contains(anyStringContains)
			       || x.parentScheduledEvent.attributes.Contains(anyStringContains)
			       || x.priority.name.Contains(anyStringContains)
			       || x.priority.description.Contains(anyStringContains)
			       || x.priority.color.Contains(anyStringContains)
			       || x.resource.name.Contains(anyStringContains)
			       || x.resource.description.Contains(anyStringContains)
			       || x.resource.notes.Contains(anyStringContains)
			       || x.resource.externalId.Contains(anyStringContains)
			       || x.resource.color.Contains(anyStringContains)
			       || x.resource.attributes.Contains(anyStringContains)
			       || x.resource.avatarFileName.Contains(anyStringContains)
			       || x.resource.avatarMimeType.Contains(anyStringContains)
			       || x.scheduledEventTemplate.name.Contains(anyStringContains)
			       || x.scheduledEventTemplate.description.Contains(anyStringContains)
			       || x.scheduledEventTemplate.defaultLocationPattern.Contains(anyStringContains)
			       || x.schedulingTarget.name.Contains(anyStringContains)
			       || x.schedulingTarget.description.Contains(anyStringContains)
			       || x.schedulingTarget.notes.Contains(anyStringContains)
			       || x.schedulingTarget.externalId.Contains(anyStringContains)
			       || x.schedulingTarget.color.Contains(anyStringContains)
			       || x.schedulingTarget.attributes.Contains(anyStringContains)
			       || x.schedulingTarget.avatarFileName.Contains(anyStringContains)
			       || x.schedulingTarget.avatarMimeType.Contains(anyStringContains)
			       || x.timeZone.name.Contains(anyStringContains)
			       || x.timeZone.description.Contains(anyStringContains)
			       || x.timeZone.ianaTimeZone.Contains(anyStringContains)
			       || x.timeZone.abbreviation.Contains(anyStringContains)
			       || x.timeZone.abbreviationDaylightSavings.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ScheduledEvent.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
		}
*/


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
		[Route("api/ScheduledEvent/CreateAuditEvent")]
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


        /// <summary>
        /// 
        /// This makes a ScheduledEvent record a favourite for the current user.
		/// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/ScheduledEvent/Favourite/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPut]
		public async Task<IActionResult> SetFavourite(int id, string description = null, CancellationToken cancellationToken = default)
		{
			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(cancellationToken);

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


			IQueryable<Database.ScheduledEvent> query = (from x in _context.ScheduledEvents
			                               where x.id == id
			                               select x);


			Database.ScheduledEvent scheduledEvent = await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

			if (scheduledEvent != null)
			{
				if (string.IsNullOrEmpty(description) == true)
				{
					description = scheduledEvent.name;
				}

				//
				// Add the user favourite ScheduledEvent
				//
				await SecurityLogic.AddToUserFavouritesAsync(securityUser, "ScheduledEvent", id, description, cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'ScheduledEvent' was added for record with id of " + id + " for user " + securityUser.accountName, true);

				//
				// Return the complete list of user favourites after the addition
				//
				return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
			}
			else
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'ScheduledEvent' add request was made with an invalid id value of " + id, false);
				return BadRequest();
			}
		}


        /// <summary>
        /// 
        /// This removes a ScheduledEvent record from the current user's favourites.
        /// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/ScheduledEvent/Favourite/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpDelete]
		public async Task<IActionResult> DeleteFavourite(int id, CancellationToken cancellationToken = default)
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


			//
			// Delete the user favourite ScheduledEvent
			//
			await SecurityLogic.RemoveFromUserFavouritesAsync(securityUser, "ScheduledEvent", id, cancellationToken);

			await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'ScheduledEvent' was removed for record with id of " + id + " for user " + securityUser.accountName, true);

			//
			// Return the complete list of user favourites after the deletion
			//
			return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
		}


	}
}
