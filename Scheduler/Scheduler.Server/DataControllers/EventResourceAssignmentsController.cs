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

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the EventResourceAssignment entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the EventResourceAssignment entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class EventResourceAssignmentsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object eventResourceAssignmentPutSyncRoot = new object();
		static object eventResourceAssignmentDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<EventResourceAssignmentsController> _logger;

		public EventResourceAssignmentsController(SchedulerContext context, ILogger<EventResourceAssignmentsController> logger) : base("Scheduler", "EventResourceAssignment")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of EventResourceAssignments filtered by the parameters provided.
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
		[Route("api/EventResourceAssignments")]
		public async Task<IActionResult> GetEventResourceAssignments(
			int? scheduledEventId = null,
			int? officeId = null,
			int? resourceId = null,
			int? crewId = null,
			int? assignmentRoleId = null,
			int? assignmentStatusId = null,
			DateTime? assignmentStartDateTime = null,
			DateTime? assignmentEndDateTime = null,
			string notes = null,
			bool? isTravelRequired = null,
			int? travelDurationMinutes = null,
			float? distanceKilometers = null,
			string startLocation = null,
			DateTime? actualStartDateTime = null,
			DateTime? actualEndDateTime = null,
			string actualNotes = null,
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

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
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
			if (assignmentStartDateTime.HasValue == true && assignmentStartDateTime.Value.Kind != DateTimeKind.Utc)
			{
				assignmentStartDateTime = assignmentStartDateTime.Value.ToUniversalTime();
			}

			if (assignmentEndDateTime.HasValue == true && assignmentEndDateTime.Value.Kind != DateTimeKind.Utc)
			{
				assignmentEndDateTime = assignmentEndDateTime.Value.ToUniversalTime();
			}

			if (actualStartDateTime.HasValue == true && actualStartDateTime.Value.Kind != DateTimeKind.Utc)
			{
				actualStartDateTime = actualStartDateTime.Value.ToUniversalTime();
			}

			if (actualEndDateTime.HasValue == true && actualEndDateTime.Value.Kind != DateTimeKind.Utc)
			{
				actualEndDateTime = actualEndDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.EventResourceAssignment> query = (from era in _context.EventResourceAssignments select era);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(era => era.scheduledEventId == scheduledEventId.Value);
			}
			if (officeId.HasValue == true)
			{
				query = query.Where(era => era.officeId == officeId.Value);
			}
			if (resourceId.HasValue == true)
			{
				query = query.Where(era => era.resourceId == resourceId.Value);
			}
			if (crewId.HasValue == true)
			{
				query = query.Where(era => era.crewId == crewId.Value);
			}
			if (assignmentRoleId.HasValue == true)
			{
				query = query.Where(era => era.assignmentRoleId == assignmentRoleId.Value);
			}
			if (assignmentStatusId.HasValue == true)
			{
				query = query.Where(era => era.assignmentStatusId == assignmentStatusId.Value);
			}
			if (assignmentStartDateTime.HasValue == true)
			{
				query = query.Where(era => era.assignmentStartDateTime == assignmentStartDateTime.Value);
			}
			if (assignmentEndDateTime.HasValue == true)
			{
				query = query.Where(era => era.assignmentEndDateTime == assignmentEndDateTime.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(era => era.notes == notes);
			}
			if (isTravelRequired.HasValue == true)
			{
				query = query.Where(era => era.isTravelRequired == isTravelRequired.Value);
			}
			if (travelDurationMinutes.HasValue == true)
			{
				query = query.Where(era => era.travelDurationMinutes == travelDurationMinutes.Value);
			}
			if (distanceKilometers.HasValue == true)
			{
				query = query.Where(era => era.distanceKilometers == distanceKilometers.Value);
			}
			if (string.IsNullOrEmpty(startLocation) == false)
			{
				query = query.Where(era => era.startLocation == startLocation);
			}
			if (actualStartDateTime.HasValue == true)
			{
				query = query.Where(era => era.actualStartDateTime == actualStartDateTime.Value);
			}
			if (actualEndDateTime.HasValue == true)
			{
				query = query.Where(era => era.actualEndDateTime == actualEndDateTime.Value);
			}
			if (string.IsNullOrEmpty(actualNotes) == false)
			{
				query = query.Where(era => era.actualNotes == actualNotes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(era => era.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(era => era.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(era => era.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(era => era.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(era => era.deleted == false);
				}
			}
			else
			{
				query = query.Where(era => era.active == true);
				query = query.Where(era => era.deleted == false);
			}

			query = query.OrderBy(era => era.startLocation);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.assignmentRole);
				query = query.Include(x => x.assignmentStatus);
				query = query.Include(x => x.crew);
				query = query.Include(x => x.office);
				query = query.Include(x => x.resource);
				query = query.Include(x => x.scheduledEvent);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Event Resource Assignment, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.notes.Contains(anyStringContains)
			       || x.startLocation.Contains(anyStringContains)
			       || x.actualNotes.Contains(anyStringContains)
			       || (includeRelations == true && x.assignmentRole.name.Contains(anyStringContains))
			       || (includeRelations == true && x.assignmentRole.description.Contains(anyStringContains))
			       || (includeRelations == true && x.assignmentRole.color.Contains(anyStringContains))
			       || (includeRelations == true && x.assignmentStatus.name.Contains(anyStringContains))
			       || (includeRelations == true && x.assignmentStatus.description.Contains(anyStringContains))
			       || (includeRelations == true && x.assignmentStatus.color.Contains(anyStringContains))
			       || (includeRelations == true && x.crew.name.Contains(anyStringContains))
			       || (includeRelations == true && x.crew.description.Contains(anyStringContains))
			       || (includeRelations == true && x.crew.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.crew.color.Contains(anyStringContains))
			       || (includeRelations == true && x.crew.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.crew.avatarMimeType.Contains(anyStringContains))
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
			       || (includeRelations == true && x.resource.name.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.description.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.color.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.avatarMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.name.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.description.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.location.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.color.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.attributes.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.EventResourceAssignment> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.EventResourceAssignment eventResourceAssignment in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(eventResourceAssignment, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.EventResourceAssignment Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.EventResourceAssignment Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of EventResourceAssignments filtered by the parameters provided.  Its query is similar to the GetEventResourceAssignments method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventResourceAssignments/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? scheduledEventId = null,
			int? officeId = null,
			int? resourceId = null,
			int? crewId = null,
			int? assignmentRoleId = null,
			int? assignmentStatusId = null,
			DateTime? assignmentStartDateTime = null,
			DateTime? assignmentEndDateTime = null,
			string notes = null,
			bool? isTravelRequired = null,
			int? travelDurationMinutes = null,
			float? distanceKilometers = null,
			string startLocation = null,
			DateTime? actualStartDateTime = null,
			DateTime? actualEndDateTime = null,
			string actualNotes = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
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
			if (assignmentStartDateTime.HasValue == true && assignmentStartDateTime.Value.Kind != DateTimeKind.Utc)
			{
				assignmentStartDateTime = assignmentStartDateTime.Value.ToUniversalTime();
			}

			if (assignmentEndDateTime.HasValue == true && assignmentEndDateTime.Value.Kind != DateTimeKind.Utc)
			{
				assignmentEndDateTime = assignmentEndDateTime.Value.ToUniversalTime();
			}

			if (actualStartDateTime.HasValue == true && actualStartDateTime.Value.Kind != DateTimeKind.Utc)
			{
				actualStartDateTime = actualStartDateTime.Value.ToUniversalTime();
			}

			if (actualEndDateTime.HasValue == true && actualEndDateTime.Value.Kind != DateTimeKind.Utc)
			{
				actualEndDateTime = actualEndDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.EventResourceAssignment> query = (from era in _context.EventResourceAssignments select era);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(era => era.scheduledEventId == scheduledEventId.Value);
			}
			if (officeId.HasValue == true)
			{
				query = query.Where(era => era.officeId == officeId.Value);
			}
			if (resourceId.HasValue == true)
			{
				query = query.Where(era => era.resourceId == resourceId.Value);
			}
			if (crewId.HasValue == true)
			{
				query = query.Where(era => era.crewId == crewId.Value);
			}
			if (assignmentRoleId.HasValue == true)
			{
				query = query.Where(era => era.assignmentRoleId == assignmentRoleId.Value);
			}
			if (assignmentStatusId.HasValue == true)
			{
				query = query.Where(era => era.assignmentStatusId == assignmentStatusId.Value);
			}
			if (assignmentStartDateTime.HasValue == true)
			{
				query = query.Where(era => era.assignmentStartDateTime == assignmentStartDateTime.Value);
			}
			if (assignmentEndDateTime.HasValue == true)
			{
				query = query.Where(era => era.assignmentEndDateTime == assignmentEndDateTime.Value);
			}
			if (notes != null)
			{
				query = query.Where(era => era.notes == notes);
			}
			if (isTravelRequired.HasValue == true)
			{
				query = query.Where(era => era.isTravelRequired == isTravelRequired.Value);
			}
			if (travelDurationMinutes.HasValue == true)
			{
				query = query.Where(era => era.travelDurationMinutes == travelDurationMinutes.Value);
			}
			if (distanceKilometers.HasValue == true)
			{
				query = query.Where(era => era.distanceKilometers == distanceKilometers.Value);
			}
			if (startLocation != null)
			{
				query = query.Where(era => era.startLocation == startLocation);
			}
			if (actualStartDateTime.HasValue == true)
			{
				query = query.Where(era => era.actualStartDateTime == actualStartDateTime.Value);
			}
			if (actualEndDateTime.HasValue == true)
			{
				query = query.Where(era => era.actualEndDateTime == actualEndDateTime.Value);
			}
			if (actualNotes != null)
			{
				query = query.Where(era => era.actualNotes == actualNotes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(era => era.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(era => era.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(era => era.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(era => era.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(era => era.deleted == false);
				}
			}
			else
			{
				query = query.Where(era => era.active == true);
				query = query.Where(era => era.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Event Resource Assignment, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.notes.Contains(anyStringContains)
			       || x.startLocation.Contains(anyStringContains)
			       || x.actualNotes.Contains(anyStringContains)
			       || x.assignmentRole.name.Contains(anyStringContains)
			       || x.assignmentRole.description.Contains(anyStringContains)
			       || x.assignmentRole.color.Contains(anyStringContains)
			       || x.assignmentStatus.name.Contains(anyStringContains)
			       || x.assignmentStatus.description.Contains(anyStringContains)
			       || x.assignmentStatus.color.Contains(anyStringContains)
			       || x.crew.name.Contains(anyStringContains)
			       || x.crew.description.Contains(anyStringContains)
			       || x.crew.notes.Contains(anyStringContains)
			       || x.crew.color.Contains(anyStringContains)
			       || x.crew.avatarFileName.Contains(anyStringContains)
			       || x.crew.avatarMimeType.Contains(anyStringContains)
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
			       || x.resource.name.Contains(anyStringContains)
			       || x.resource.description.Contains(anyStringContains)
			       || x.resource.notes.Contains(anyStringContains)
			       || x.resource.externalId.Contains(anyStringContains)
			       || x.resource.color.Contains(anyStringContains)
			       || x.resource.attributes.Contains(anyStringContains)
			       || x.resource.avatarFileName.Contains(anyStringContains)
			       || x.resource.avatarMimeType.Contains(anyStringContains)
			       || x.scheduledEvent.name.Contains(anyStringContains)
			       || x.scheduledEvent.description.Contains(anyStringContains)
			       || x.scheduledEvent.location.Contains(anyStringContains)
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
        /// This gets a single EventResourceAssignment by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventResourceAssignment/{id}")]
		public async Task<IActionResult> GetEventResourceAssignment(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
			
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
				IQueryable<Database.EventResourceAssignment> query = (from era in _context.EventResourceAssignments where
							(era.id == id) &&
							(userIsAdmin == true || era.deleted == false) &&
							(userIsWriter == true || era.active == true)
					select era);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.assignmentRole);
					query = query.Include(x => x.assignmentStatus);
					query = query.Include(x => x.crew);
					query = query.Include(x => x.office);
					query = query.Include(x => x.resource);
					query = query.Include(x => x.scheduledEvent);
					query = query.AsSplitQuery();
				}

				Database.EventResourceAssignment materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.EventResourceAssignment Entity was read with Admin privilege." : "Scheduler.EventResourceAssignment Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "EventResourceAssignment", materialized.id, materialized.startLocation));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.EventResourceAssignment entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.EventResourceAssignment.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.EventResourceAssignment.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing EventResourceAssignment record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/EventResourceAssignment/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutEventResourceAssignment(int id, [FromBody]Database.EventResourceAssignment.EventResourceAssignmentDTO eventResourceAssignmentDTO, CancellationToken cancellationToken = default)
		{
			if (eventResourceAssignmentDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != eventResourceAssignmentDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
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


			IQueryable<Database.EventResourceAssignment> query = (from x in _context.EventResourceAssignments
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.EventResourceAssignment existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.EventResourceAssignment PUT", id.ToString(), new Exception("No Scheduler.EventResourceAssignment entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (eventResourceAssignmentDTO.objectGuid == Guid.Empty)
            {
                eventResourceAssignmentDTO.objectGuid = existing.objectGuid;
            }
            else if (eventResourceAssignmentDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a EventResourceAssignment record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.EventResourceAssignment cloneOfExisting = (Database.EventResourceAssignment)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new EventResourceAssignment object using the data from the existing record, updated with what is in the DTO.
			//
			Database.EventResourceAssignment eventResourceAssignment = (Database.EventResourceAssignment)_context.Entry(existing).GetDatabaseValues().ToObject();
			eventResourceAssignment.ApplyDTO(eventResourceAssignmentDTO);
			//
			// The tenant guid for any EventResourceAssignment being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the EventResourceAssignment because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				eventResourceAssignment.tenantGuid = existing.tenantGuid;
			}

			lock (eventResourceAssignmentPutSyncRoot)
			{
				//
				// Validate the version number for the eventResourceAssignment being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != eventResourceAssignment.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "EventResourceAssignment save attempt was made but save request was with version " + eventResourceAssignment.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The EventResourceAssignment you are trying to update has already changed.  Please try your save again after reloading the EventResourceAssignment.");
				}
				else
				{
					// Same record.  Increase version.
					eventResourceAssignment.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (eventResourceAssignment.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.EventResourceAssignment record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (eventResourceAssignment.assignmentStartDateTime.HasValue == true && eventResourceAssignment.assignmentStartDateTime.Value.Kind != DateTimeKind.Utc)
				{
					eventResourceAssignment.assignmentStartDateTime = eventResourceAssignment.assignmentStartDateTime.Value.ToUniversalTime();
				}

				if (eventResourceAssignment.assignmentEndDateTime.HasValue == true && eventResourceAssignment.assignmentEndDateTime.Value.Kind != DateTimeKind.Utc)
				{
					eventResourceAssignment.assignmentEndDateTime = eventResourceAssignment.assignmentEndDateTime.Value.ToUniversalTime();
				}

				if (eventResourceAssignment.startLocation != null && eventResourceAssignment.startLocation.Length > 100)
				{
					eventResourceAssignment.startLocation = eventResourceAssignment.startLocation.Substring(0, 100);
				}

				if (eventResourceAssignment.actualStartDateTime.HasValue == true && eventResourceAssignment.actualStartDateTime.Value.Kind != DateTimeKind.Utc)
				{
					eventResourceAssignment.actualStartDateTime = eventResourceAssignment.actualStartDateTime.Value.ToUniversalTime();
				}

				if (eventResourceAssignment.actualEndDateTime.HasValue == true && eventResourceAssignment.actualEndDateTime.Value.Kind != DateTimeKind.Utc)
				{
					eventResourceAssignment.actualEndDateTime = eventResourceAssignment.actualEndDateTime.Value.ToUniversalTime();
				}

				try
				{
				    EntityEntry<Database.EventResourceAssignment> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(eventResourceAssignment);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        EventResourceAssignmentChangeHistory eventResourceAssignmentChangeHistory = new EventResourceAssignmentChangeHistory();
				        eventResourceAssignmentChangeHistory.eventResourceAssignmentId = eventResourceAssignment.id;
				        eventResourceAssignmentChangeHistory.versionNumber = eventResourceAssignment.versionNumber;
				        eventResourceAssignmentChangeHistory.timeStamp = DateTime.UtcNow;
				        eventResourceAssignmentChangeHistory.userId = securityUser.id;
				        eventResourceAssignmentChangeHistory.tenantGuid = userTenantGuid;
				        eventResourceAssignmentChangeHistory.data = JsonSerializer.Serialize(Database.EventResourceAssignment.CreateAnonymousWithFirstLevelSubObjects(eventResourceAssignment));
				        _context.EventResourceAssignmentChangeHistories.Add(eventResourceAssignmentChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.EventResourceAssignment entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.EventResourceAssignment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.EventResourceAssignment.CreateAnonymousWithFirstLevelSubObjects(eventResourceAssignment)),
						null);

				return Ok(Database.EventResourceAssignment.CreateAnonymous(eventResourceAssignment));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.EventResourceAssignment entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.EventResourceAssignment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.EventResourceAssignment.CreateAnonymousWithFirstLevelSubObjects(eventResourceAssignment)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new EventResourceAssignment record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventResourceAssignment", Name = "EventResourceAssignment")]
		public async Task<IActionResult> PostEventResourceAssignment([FromBody]Database.EventResourceAssignment.EventResourceAssignmentDTO eventResourceAssignmentDTO, CancellationToken cancellationToken = default)
		{
			if (eventResourceAssignmentDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
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
			// Create a new EventResourceAssignment object using the data from the DTO
			//
			Database.EventResourceAssignment eventResourceAssignment = Database.EventResourceAssignment.FromDTO(eventResourceAssignmentDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				eventResourceAssignment.tenantGuid = userTenantGuid;

				if (eventResourceAssignment.assignmentStartDateTime.HasValue == true && eventResourceAssignment.assignmentStartDateTime.Value.Kind != DateTimeKind.Utc)
				{
					eventResourceAssignment.assignmentStartDateTime = eventResourceAssignment.assignmentStartDateTime.Value.ToUniversalTime();
				}

				if (eventResourceAssignment.assignmentEndDateTime.HasValue == true && eventResourceAssignment.assignmentEndDateTime.Value.Kind != DateTimeKind.Utc)
				{
					eventResourceAssignment.assignmentEndDateTime = eventResourceAssignment.assignmentEndDateTime.Value.ToUniversalTime();
				}

				if (eventResourceAssignment.startLocation != null && eventResourceAssignment.startLocation.Length > 100)
				{
					eventResourceAssignment.startLocation = eventResourceAssignment.startLocation.Substring(0, 100);
				}

				if (eventResourceAssignment.actualStartDateTime.HasValue == true && eventResourceAssignment.actualStartDateTime.Value.Kind != DateTimeKind.Utc)
				{
					eventResourceAssignment.actualStartDateTime = eventResourceAssignment.actualStartDateTime.Value.ToUniversalTime();
				}

				if (eventResourceAssignment.actualEndDateTime.HasValue == true && eventResourceAssignment.actualEndDateTime.Value.Kind != DateTimeKind.Utc)
				{
					eventResourceAssignment.actualEndDateTime = eventResourceAssignment.actualEndDateTime.Value.ToUniversalTime();
				}

				eventResourceAssignment.objectGuid = Guid.NewGuid();
				eventResourceAssignment.versionNumber = 1;

				_context.EventResourceAssignments.Add(eventResourceAssignment);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the eventResourceAssignment object so that no further changes will be written to the database
				    //
				    _context.Entry(eventResourceAssignment).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					eventResourceAssignment.EventResourceAssignmentChangeHistories = null;
					eventResourceAssignment.assignmentRole = null;
					eventResourceAssignment.assignmentStatus = null;
					eventResourceAssignment.crew = null;
					eventResourceAssignment.office = null;
					eventResourceAssignment.resource = null;
					eventResourceAssignment.scheduledEvent = null;


				    EventResourceAssignmentChangeHistory eventResourceAssignmentChangeHistory = new EventResourceAssignmentChangeHistory();
				    eventResourceAssignmentChangeHistory.eventResourceAssignmentId = eventResourceAssignment.id;
				    eventResourceAssignmentChangeHistory.versionNumber = eventResourceAssignment.versionNumber;
				    eventResourceAssignmentChangeHistory.timeStamp = DateTime.UtcNow;
				    eventResourceAssignmentChangeHistory.userId = securityUser.id;
				    eventResourceAssignmentChangeHistory.tenantGuid = userTenantGuid;
				    eventResourceAssignmentChangeHistory.data = JsonSerializer.Serialize(Database.EventResourceAssignment.CreateAnonymousWithFirstLevelSubObjects(eventResourceAssignment));
				    _context.EventResourceAssignmentChangeHistories.Add(eventResourceAssignmentChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.EventResourceAssignment entity successfully created.",
						true,
						eventResourceAssignment. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.EventResourceAssignment.CreateAnonymousWithFirstLevelSubObjects(eventResourceAssignment)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.EventResourceAssignment entity creation failed.", false, eventResourceAssignment.id.ToString(), "", JsonSerializer.Serialize(Database.EventResourceAssignment.CreateAnonymousWithFirstLevelSubObjects(eventResourceAssignment)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "EventResourceAssignment", eventResourceAssignment.id, eventResourceAssignment.startLocation));

			return CreatedAtRoute("EventResourceAssignment", new { id = eventResourceAssignment.id }, Database.EventResourceAssignment.CreateAnonymousWithFirstLevelSubObjects(eventResourceAssignment));
		}



        /// <summary>
        /// 
        /// This rolls a EventResourceAssignment entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventResourceAssignment/Rollback/{id}")]
		[Route("api/EventResourceAssignment/Rollback")]
		public async Task<IActionResult> RollbackToEventResourceAssignmentVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);

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

			

			
			IQueryable <Database.EventResourceAssignment> query = (from x in _context.EventResourceAssignments
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this EventResourceAssignment concurrently
			//
			lock (eventResourceAssignmentPutSyncRoot)
			{
				
				Database.EventResourceAssignment eventResourceAssignment = query.FirstOrDefault();
				
				if (eventResourceAssignment == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.EventResourceAssignment rollback", id.ToString(), new Exception("No Scheduler.EventResourceAssignment entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the EventResourceAssignment current state so we can log it.
				//
				Database.EventResourceAssignment cloneOfExisting = (Database.EventResourceAssignment)_context.Entry(eventResourceAssignment).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.EventResourceAssignmentChangeHistories = null;
				cloneOfExisting.assignmentRole = null;
				cloneOfExisting.assignmentStatus = null;
				cloneOfExisting.crew = null;
				cloneOfExisting.office = null;
				cloneOfExisting.resource = null;
				cloneOfExisting.scheduledEvent = null;

				if (versionNumber >= eventResourceAssignment.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.EventResourceAssignment rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.EventResourceAssignment rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				EventResourceAssignmentChangeHistory eventResourceAssignmentChangeHistory = (from x in _context.EventResourceAssignmentChangeHistories
				                                               where
				                                               x.eventResourceAssignmentId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (eventResourceAssignmentChangeHistory != null)
				{
				    Database.EventResourceAssignment oldEventResourceAssignment = JsonSerializer.Deserialize<Database.EventResourceAssignment>(eventResourceAssignmentChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    eventResourceAssignment.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    eventResourceAssignment.scheduledEventId = oldEventResourceAssignment.scheduledEventId;
				    eventResourceAssignment.officeId = oldEventResourceAssignment.officeId;
				    eventResourceAssignment.resourceId = oldEventResourceAssignment.resourceId;
				    eventResourceAssignment.crewId = oldEventResourceAssignment.crewId;
				    eventResourceAssignment.assignmentRoleId = oldEventResourceAssignment.assignmentRoleId;
				    eventResourceAssignment.assignmentStatusId = oldEventResourceAssignment.assignmentStatusId;
				    eventResourceAssignment.assignmentStartDateTime = oldEventResourceAssignment.assignmentStartDateTime;
				    eventResourceAssignment.assignmentEndDateTime = oldEventResourceAssignment.assignmentEndDateTime;
				    eventResourceAssignment.notes = oldEventResourceAssignment.notes;
				    eventResourceAssignment.isTravelRequired = oldEventResourceAssignment.isTravelRequired;
				    eventResourceAssignment.travelDurationMinutes = oldEventResourceAssignment.travelDurationMinutes;
				    eventResourceAssignment.distanceKilometers = oldEventResourceAssignment.distanceKilometers;
				    eventResourceAssignment.startLocation = oldEventResourceAssignment.startLocation;
				    eventResourceAssignment.actualStartDateTime = oldEventResourceAssignment.actualStartDateTime;
				    eventResourceAssignment.actualEndDateTime = oldEventResourceAssignment.actualEndDateTime;
				    eventResourceAssignment.actualNotes = oldEventResourceAssignment.actualNotes;
				    eventResourceAssignment.objectGuid = oldEventResourceAssignment.objectGuid;
				    eventResourceAssignment.active = oldEventResourceAssignment.active;
				    eventResourceAssignment.deleted = oldEventResourceAssignment.deleted;

				    string serializedEventResourceAssignment = JsonSerializer.Serialize(eventResourceAssignment);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        EventResourceAssignmentChangeHistory newEventResourceAssignmentChangeHistory = new EventResourceAssignmentChangeHistory();
				        newEventResourceAssignmentChangeHistory.eventResourceAssignmentId = eventResourceAssignment.id;
				        newEventResourceAssignmentChangeHistory.versionNumber = eventResourceAssignment.versionNumber;
				        newEventResourceAssignmentChangeHistory.timeStamp = DateTime.UtcNow;
				        newEventResourceAssignmentChangeHistory.userId = securityUser.id;
				        newEventResourceAssignmentChangeHistory.tenantGuid = userTenantGuid;
				        newEventResourceAssignmentChangeHistory.data = JsonSerializer.Serialize(Database.EventResourceAssignment.CreateAnonymousWithFirstLevelSubObjects(eventResourceAssignment));
				        _context.EventResourceAssignmentChangeHistories.Add(newEventResourceAssignmentChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.EventResourceAssignment rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.EventResourceAssignment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.EventResourceAssignment.CreateAnonymousWithFirstLevelSubObjects(eventResourceAssignment)),
						null);


				    return Ok(Database.EventResourceAssignment.CreateAnonymous(eventResourceAssignment));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.EventResourceAssignment rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.EventResourceAssignment rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}


        /// <summary>
        /// 
        /// This deletes a EventResourceAssignment record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventResourceAssignment/{id}")]
		[Route("api/EventResourceAssignment")]
		public async Task<IActionResult> DeleteEventResourceAssignment(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(cancellationToken);
			
			
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

			IQueryable<Database.EventResourceAssignment> query = (from x in _context.EventResourceAssignments
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.EventResourceAssignment eventResourceAssignment = await query.FirstOrDefaultAsync(cancellationToken);

			if (eventResourceAssignment == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.EventResourceAssignment DELETE", id.ToString(), new Exception("No Scheduler.EventResourceAssignment entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.EventResourceAssignment cloneOfExisting = (Database.EventResourceAssignment)_context.Entry(eventResourceAssignment).GetDatabaseValues().ToObject();


			lock (eventResourceAssignmentDeleteSyncRoot)
			{
			    try
			    {
			        eventResourceAssignment.deleted = true;
			        eventResourceAssignment.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        EventResourceAssignmentChangeHistory eventResourceAssignmentChangeHistory = new EventResourceAssignmentChangeHistory();
			        eventResourceAssignmentChangeHistory.eventResourceAssignmentId = eventResourceAssignment.id;
			        eventResourceAssignmentChangeHistory.versionNumber = eventResourceAssignment.versionNumber;
			        eventResourceAssignmentChangeHistory.timeStamp = DateTime.UtcNow;
			        eventResourceAssignmentChangeHistory.userId = securityUser.id;
			        eventResourceAssignmentChangeHistory.tenantGuid = userTenantGuid;
			        eventResourceAssignmentChangeHistory.data = JsonSerializer.Serialize(Database.EventResourceAssignment.CreateAnonymousWithFirstLevelSubObjects(eventResourceAssignment));
			        _context.EventResourceAssignmentChangeHistories.Add(eventResourceAssignmentChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.EventResourceAssignment entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.EventResourceAssignment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.EventResourceAssignment.CreateAnonymousWithFirstLevelSubObjects(eventResourceAssignment)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.EventResourceAssignment entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.EventResourceAssignment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.EventResourceAssignment.CreateAnonymousWithFirstLevelSubObjects(eventResourceAssignment)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of EventResourceAssignment records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/EventResourceAssignments/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? scheduledEventId = null,
			int? officeId = null,
			int? resourceId = null,
			int? crewId = null,
			int? assignmentRoleId = null,
			int? assignmentStatusId = null,
			DateTime? assignmentStartDateTime = null,
			DateTime? assignmentEndDateTime = null,
			string notes = null,
			bool? isTravelRequired = null,
			int? travelDurationMinutes = null,
			float? distanceKilometers = null,
			string startLocation = null,
			DateTime? actualStartDateTime = null,
			DateTime? actualEndDateTime = null,
			string actualNotes = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);

			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);

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
			if (assignmentStartDateTime.HasValue == true && assignmentStartDateTime.Value.Kind != DateTimeKind.Utc)
			{
				assignmentStartDateTime = assignmentStartDateTime.Value.ToUniversalTime();
			}

			if (assignmentEndDateTime.HasValue == true && assignmentEndDateTime.Value.Kind != DateTimeKind.Utc)
			{
				assignmentEndDateTime = assignmentEndDateTime.Value.ToUniversalTime();
			}

			if (actualStartDateTime.HasValue == true && actualStartDateTime.Value.Kind != DateTimeKind.Utc)
			{
				actualStartDateTime = actualStartDateTime.Value.ToUniversalTime();
			}

			if (actualEndDateTime.HasValue == true && actualEndDateTime.Value.Kind != DateTimeKind.Utc)
			{
				actualEndDateTime = actualEndDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.EventResourceAssignment> query = (from era in _context.EventResourceAssignments select era);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(era => era.scheduledEventId == scheduledEventId.Value);
			}
			if (officeId.HasValue == true)
			{
				query = query.Where(era => era.officeId == officeId.Value);
			}
			if (resourceId.HasValue == true)
			{
				query = query.Where(era => era.resourceId == resourceId.Value);
			}
			if (crewId.HasValue == true)
			{
				query = query.Where(era => era.crewId == crewId.Value);
			}
			if (assignmentRoleId.HasValue == true)
			{
				query = query.Where(era => era.assignmentRoleId == assignmentRoleId.Value);
			}
			if (assignmentStatusId.HasValue == true)
			{
				query = query.Where(era => era.assignmentStatusId == assignmentStatusId.Value);
			}
			if (assignmentStartDateTime.HasValue == true)
			{
				query = query.Where(era => era.assignmentStartDateTime == assignmentStartDateTime.Value);
			}
			if (assignmentEndDateTime.HasValue == true)
			{
				query = query.Where(era => era.assignmentEndDateTime == assignmentEndDateTime.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(era => era.notes == notes);
			}
			if (isTravelRequired.HasValue == true)
			{
				query = query.Where(era => era.isTravelRequired == isTravelRequired.Value);
			}
			if (travelDurationMinutes.HasValue == true)
			{
				query = query.Where(era => era.travelDurationMinutes == travelDurationMinutes.Value);
			}
			if (distanceKilometers.HasValue == true)
			{
				query = query.Where(era => era.distanceKilometers == distanceKilometers.Value);
			}
			if (string.IsNullOrEmpty(startLocation) == false)
			{
				query = query.Where(era => era.startLocation == startLocation);
			}
			if (actualStartDateTime.HasValue == true)
			{
				query = query.Where(era => era.actualStartDateTime == actualStartDateTime.Value);
			}
			if (actualEndDateTime.HasValue == true)
			{
				query = query.Where(era => era.actualEndDateTime == actualEndDateTime.Value);
			}
			if (string.IsNullOrEmpty(actualNotes) == false)
			{
				query = query.Where(era => era.actualNotes == actualNotes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(era => era.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(era => era.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(era => era.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(era => era.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(era => era.deleted == false);
				}
			}
			else
			{
				query = query.Where(era => era.active == true);
				query = query.Where(era => era.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Event Resource Assignment, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.notes.Contains(anyStringContains)
			       || x.startLocation.Contains(anyStringContains)
			       || x.actualNotes.Contains(anyStringContains)
			       || x.assignmentRole.name.Contains(anyStringContains)
			       || x.assignmentRole.description.Contains(anyStringContains)
			       || x.assignmentRole.color.Contains(anyStringContains)
			       || x.assignmentStatus.name.Contains(anyStringContains)
			       || x.assignmentStatus.description.Contains(anyStringContains)
			       || x.assignmentStatus.color.Contains(anyStringContains)
			       || x.crew.name.Contains(anyStringContains)
			       || x.crew.description.Contains(anyStringContains)
			       || x.crew.notes.Contains(anyStringContains)
			       || x.crew.color.Contains(anyStringContains)
			       || x.crew.avatarFileName.Contains(anyStringContains)
			       || x.crew.avatarMimeType.Contains(anyStringContains)
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
			       || x.resource.name.Contains(anyStringContains)
			       || x.resource.description.Contains(anyStringContains)
			       || x.resource.notes.Contains(anyStringContains)
			       || x.resource.externalId.Contains(anyStringContains)
			       || x.resource.color.Contains(anyStringContains)
			       || x.resource.attributes.Contains(anyStringContains)
			       || x.resource.avatarFileName.Contains(anyStringContains)
			       || x.resource.avatarMimeType.Contains(anyStringContains)
			       || x.scheduledEvent.name.Contains(anyStringContains)
			       || x.scheduledEvent.description.Contains(anyStringContains)
			       || x.scheduledEvent.location.Contains(anyStringContains)
			       || x.scheduledEvent.notes.Contains(anyStringContains)
			       || x.scheduledEvent.color.Contains(anyStringContains)
			       || x.scheduledEvent.externalId.Contains(anyStringContains)
			       || x.scheduledEvent.attributes.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.startLocation);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.EventResourceAssignment.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/EventResourceAssignment/CreateAuditEvent")]
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
