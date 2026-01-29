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
    /// This auto generated class provides the basic CRUD operations for the EventCalendar entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the EventCalendar entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class EventCalendarsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private SchedulerContext _context;

		private ILogger<EventCalendarsController> _logger;

		public EventCalendarsController(SchedulerContext context, ILogger<EventCalendarsController> logger) : base("Scheduler", "EventCalendar")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of EventCalendars filtered by the parameters provided.
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
		[Route("api/EventCalendars")]
		public async Task<IActionResult> GetEventCalendars(
			int? scheduledEventId = null,
			int? calendarId = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
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

			IQueryable<Database.EventCalendar> query = (from ec in _context.EventCalendars select ec);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(ec => ec.scheduledEventId == scheduledEventId.Value);
			}
			if (calendarId.HasValue == true)
			{
				query = query.Where(ec => ec.calendarId == calendarId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ec => ec.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ec => ec.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ec => ec.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ec => ec.deleted == false);
				}
			}
			else
			{
				query = query.Where(ec => ec.active == true);
				query = query.Where(ec => ec.deleted == false);
			}

			query = query.OrderBy(ec => ec.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.calendar);
				query = query.Include(x => x.scheduledEvent);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Event Calendar, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       (includeRelations == true && x.calendar.name.Contains(anyStringContains))
			       || (includeRelations == true && x.calendar.description.Contains(anyStringContains))
			       || (includeRelations == true && x.calendar.color.Contains(anyStringContains))
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
			
			List<Database.EventCalendar> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.EventCalendar eventCalendar in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(eventCalendar, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.EventCalendar Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.EventCalendar Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of EventCalendars filtered by the parameters provided.  Its query is similar to the GetEventCalendars method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventCalendars/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? scheduledEventId = null,
			int? calendarId = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
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


			IQueryable<Database.EventCalendar> query = (from ec in _context.EventCalendars select ec);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(ec => ec.scheduledEventId == scheduledEventId.Value);
			}
			if (calendarId.HasValue == true)
			{
				query = query.Where(ec => ec.calendarId == calendarId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ec => ec.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ec => ec.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ec => ec.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ec => ec.deleted == false);
				}
			}
			else
			{
				query = query.Where(ec => ec.active == true);
				query = query.Where(ec => ec.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Event Calendar, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.calendar.name.Contains(anyStringContains)
			       || x.calendar.description.Contains(anyStringContains)
			       || x.calendar.color.Contains(anyStringContains)
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
        /// This gets a single EventCalendar by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventCalendar/{id}")]
		public async Task<IActionResult> GetEventCalendar(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
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
				IQueryable<Database.EventCalendar> query = (from ec in _context.EventCalendars where
							(ec.id == id) &&
							(userIsAdmin == true || ec.deleted == false) &&
							(userIsWriter == true || ec.active == true)
					select ec);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.calendar);
					query = query.Include(x => x.scheduledEvent);
					query = query.AsSplitQuery();
				}

				Database.EventCalendar materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.EventCalendar Entity was read with Admin privilege." : "Scheduler.EventCalendar Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "EventCalendar", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.EventCalendar entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.EventCalendar.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.EventCalendar.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing EventCalendar record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/EventCalendar/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutEventCalendar(int id, [FromBody]Database.EventCalendar.EventCalendarDTO eventCalendarDTO, CancellationToken cancellationToken = default)
		{
			if (eventCalendarDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != eventCalendarDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
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


			IQueryable<Database.EventCalendar> query = (from x in _context.EventCalendars
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.EventCalendar existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.EventCalendar PUT", id.ToString(), new Exception("No Scheduler.EventCalendar entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (eventCalendarDTO.objectGuid == Guid.Empty)
            {
                eventCalendarDTO.objectGuid = existing.objectGuid;
            }
            else if (eventCalendarDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a EventCalendar record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.EventCalendar cloneOfExisting = (Database.EventCalendar)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new EventCalendar object using the data from the existing record, updated with what is in the DTO.
			//
			Database.EventCalendar eventCalendar = (Database.EventCalendar)_context.Entry(existing).GetDatabaseValues().ToObject();
			eventCalendar.ApplyDTO(eventCalendarDTO);
			//
			// The tenant guid for any EventCalendar being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the EventCalendar because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				eventCalendar.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (eventCalendar.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.EventCalendar record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			EntityEntry<Database.EventCalendar> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(eventCalendar);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.EventCalendar entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.EventCalendar.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.EventCalendar.CreateAnonymousWithFirstLevelSubObjects(eventCalendar)),
					null);


				return Ok(Database.EventCalendar.CreateAnonymous(eventCalendar));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.EventCalendar entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.EventCalendar.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.EventCalendar.CreateAnonymousWithFirstLevelSubObjects(eventCalendar)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new EventCalendar record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventCalendar", Name = "EventCalendar")]
		public async Task<IActionResult> PostEventCalendar([FromBody]Database.EventCalendar.EventCalendarDTO eventCalendarDTO, CancellationToken cancellationToken = default)
		{
			if (eventCalendarDTO == null)
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
			// Create a new EventCalendar object using the data from the DTO
			//
			Database.EventCalendar eventCalendar = Database.EventCalendar.FromDTO(eventCalendarDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				eventCalendar.tenantGuid = userTenantGuid;

				eventCalendar.objectGuid = Guid.NewGuid();
				_context.EventCalendars.Add(eventCalendar);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.EventCalendar entity successfully created.",
					true,
					eventCalendar.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.EventCalendar.CreateAnonymousWithFirstLevelSubObjects(eventCalendar)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.EventCalendar entity creation failed.", false, eventCalendar.id.ToString(), "", JsonSerializer.Serialize(eventCalendar), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "EventCalendar", eventCalendar.id, eventCalendar.id.ToString()));

			return CreatedAtRoute("EventCalendar", new { id = eventCalendar.id }, Database.EventCalendar.CreateAnonymousWithFirstLevelSubObjects(eventCalendar));
		}



        /// <summary>
        /// 
        /// This deletes a EventCalendar record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventCalendar/{id}")]
		[Route("api/EventCalendar")]
		public async Task<IActionResult> DeleteEventCalendar(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.EventCalendar> query = (from x in _context.EventCalendars
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.EventCalendar eventCalendar = await query.FirstOrDefaultAsync(cancellationToken);

			if (eventCalendar == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.EventCalendar DELETE", id.ToString(), new Exception("No Scheduler.EventCalendar entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.EventCalendar cloneOfExisting = (Database.EventCalendar)_context.Entry(eventCalendar).GetDatabaseValues().ToObject();


			try
			{
				eventCalendar.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.EventCalendar entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.EventCalendar.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.EventCalendar.CreateAnonymousWithFirstLevelSubObjects(eventCalendar)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.EventCalendar entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.EventCalendar.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.EventCalendar.CreateAnonymousWithFirstLevelSubObjects(eventCalendar)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of EventCalendar records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/EventCalendars/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? scheduledEventId = null,
			int? calendarId = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);

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

			IQueryable<Database.EventCalendar> query = (from ec in _context.EventCalendars select ec);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(ec => ec.scheduledEventId == scheduledEventId.Value);
			}
			if (calendarId.HasValue == true)
			{
				query = query.Where(ec => ec.calendarId == calendarId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ec => ec.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ec => ec.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ec => ec.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ec => ec.deleted == false);
				}
			}
			else
			{
				query = query.Where(ec => ec.active == true);
				query = query.Where(ec => ec.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Event Calendar, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.calendar.name.Contains(anyStringContains)
			       || x.calendar.description.Contains(anyStringContains)
			       || x.calendar.color.Contains(anyStringContains)
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


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.EventCalendar.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/EventCalendar/CreateAuditEvent")]
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
