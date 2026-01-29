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
    /// This auto generated class provides the basic CRUD operations for the EventStatus entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the EventStatus entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class EventStatusesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private SchedulerContext _context;

		private ILogger<EventStatusesController> _logger;

		public EventStatusesController(SchedulerContext context, ILogger<EventStatusesController> logger) : base("Scheduler", "EventStatus")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of EventStatuses filtered by the parameters provided.
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
		[Route("api/EventStatuses")]
		public async Task<IActionResult> GetEventStatuses(
			string name = null,
			string description = null,
			string color = null,
			int? sequence = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

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

			IQueryable<Database.EventStatus> query = (from es in _context.EventStatuses select es);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(es => es.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(es => es.description == description);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(es => es.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(es => es.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(es => es.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(es => es.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(es => es.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(es => es.deleted == false);
				}
			}
			else
			{
				query = query.Where(es => es.active == true);
				query = query.Where(es => es.deleted == false);
			}

			query = query.OrderBy(es => es.sequence).ThenBy(es => es.name).ThenBy(es => es.description).ThenBy(es => es.color);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Event Status, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.EventStatus> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.EventStatus eventStatus in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(eventStatus, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.EventStatus Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.EventStatus Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of EventStatuses filtered by the parameters provided.  Its query is similar to the GetEventStatuses method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventStatuses/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			string color = null,
			int? sequence = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.EventStatus> query = (from es in _context.EventStatuses select es);
			if (name != null)
			{
				query = query.Where(es => es.name == name);
			}
			if (description != null)
			{
				query = query.Where(es => es.description == description);
			}
			if (color != null)
			{
				query = query.Where(es => es.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(es => es.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(es => es.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(es => es.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(es => es.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(es => es.deleted == false);
				}
			}
			else
			{
				query = query.Where(es => es.active == true);
				query = query.Where(es => es.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Event Status, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single EventStatus by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventStatus/{id}")]
		public async Task<IActionResult> GetEventStatus(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.EventStatus> query = (from es in _context.EventStatuses where
							(es.id == id) &&
							(userIsAdmin == true || es.deleted == false) &&
							(userIsWriter == true || es.active == true)
					select es);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.EventStatus materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.EventStatus Entity was read with Admin privilege." : "Scheduler.EventStatus Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "EventStatus", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.EventStatus entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.EventStatus.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.EventStatus.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing EventStatus record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/EventStatus/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutEventStatus(int id, [FromBody]Database.EventStatus.EventStatusDTO eventStatusDTO, CancellationToken cancellationToken = default)
		{
			if (eventStatusDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != eventStatusDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.EventStatus> query = (from x in _context.EventStatuses
				where
				(x.id == id)
				select x);


			Database.EventStatus existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.EventStatus PUT", id.ToString(), new Exception("No Scheduler.EventStatus entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (eventStatusDTO.objectGuid == Guid.Empty)
            {
                eventStatusDTO.objectGuid = existing.objectGuid;
            }
            else if (eventStatusDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a EventStatus record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.EventStatus cloneOfExisting = (Database.EventStatus)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new EventStatus object using the data from the existing record, updated with what is in the DTO.
			//
			Database.EventStatus eventStatus = (Database.EventStatus)_context.Entry(existing).GetDatabaseValues().ToObject();
			eventStatus.ApplyDTO(eventStatusDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (eventStatus.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.EventStatus record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (eventStatus.name != null && eventStatus.name.Length > 100)
			{
				eventStatus.name = eventStatus.name.Substring(0, 100);
			}

			if (eventStatus.description != null && eventStatus.description.Length > 500)
			{
				eventStatus.description = eventStatus.description.Substring(0, 500);
			}

			if (eventStatus.color != null && eventStatus.color.Length > 10)
			{
				eventStatus.color = eventStatus.color.Substring(0, 10);
			}

			EntityEntry<Database.EventStatus> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(eventStatus);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.EventStatus entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.EventStatus.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.EventStatus.CreateAnonymousWithFirstLevelSubObjects(eventStatus)),
					null);


				return Ok(Database.EventStatus.CreateAnonymous(eventStatus));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.EventStatus entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.EventStatus.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.EventStatus.CreateAnonymousWithFirstLevelSubObjects(eventStatus)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new EventStatus record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventStatus", Name = "EventStatus")]
		public async Task<IActionResult> PostEventStatus([FromBody]Database.EventStatus.EventStatusDTO eventStatusDTO, CancellationToken cancellationToken = default)
		{
			if (eventStatusDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new EventStatus object using the data from the DTO
			//
			Database.EventStatus eventStatus = Database.EventStatus.FromDTO(eventStatusDTO);

			try
			{
				if (eventStatus.name != null && eventStatus.name.Length > 100)
				{
					eventStatus.name = eventStatus.name.Substring(0, 100);
				}

				if (eventStatus.description != null && eventStatus.description.Length > 500)
				{
					eventStatus.description = eventStatus.description.Substring(0, 500);
				}

				if (eventStatus.color != null && eventStatus.color.Length > 10)
				{
					eventStatus.color = eventStatus.color.Substring(0, 10);
				}

				eventStatus.objectGuid = Guid.NewGuid();
				_context.EventStatuses.Add(eventStatus);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.EventStatus entity successfully created.",
					true,
					eventStatus.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.EventStatus.CreateAnonymousWithFirstLevelSubObjects(eventStatus)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.EventStatus entity creation failed.", false, eventStatus.id.ToString(), "", JsonSerializer.Serialize(eventStatus), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "EventStatus", eventStatus.id, eventStatus.name));

			return CreatedAtRoute("EventStatus", new { id = eventStatus.id }, Database.EventStatus.CreateAnonymousWithFirstLevelSubObjects(eventStatus));
		}



        /// <summary>
        /// 
        /// This deletes a EventStatus record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventStatus/{id}")]
		[Route("api/EventStatus")]
		public async Task<IActionResult> DeleteEventStatus(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.EventStatus> query = (from x in _context.EventStatuses
				where
				(x.id == id)
				select x);


			Database.EventStatus eventStatus = await query.FirstOrDefaultAsync(cancellationToken);

			if (eventStatus == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.EventStatus DELETE", id.ToString(), new Exception("No Scheduler.EventStatus entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.EventStatus cloneOfExisting = (Database.EventStatus)_context.Entry(eventStatus).GetDatabaseValues().ToObject();


			try
			{
				eventStatus.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.EventStatus entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.EventStatus.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.EventStatus.CreateAnonymousWithFirstLevelSubObjects(eventStatus)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.EventStatus entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.EventStatus.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.EventStatus.CreateAnonymousWithFirstLevelSubObjects(eventStatus)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of EventStatus records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/EventStatuses/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			string color = null,
			int? sequence = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);

			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);

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

			IQueryable<Database.EventStatus> query = (from es in _context.EventStatuses select es);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(es => es.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(es => es.description == description);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(es => es.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(es => es.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(es => es.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(es => es.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(es => es.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(es => es.deleted == false);
				}
			}
			else
			{
				query = query.Where(es => es.active == true);
				query = query.Where(es => es.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Event Status, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.color);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.EventStatus.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/EventStatus/CreateAuditEvent")]
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
