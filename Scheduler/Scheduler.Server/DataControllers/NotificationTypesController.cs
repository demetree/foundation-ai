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
    /// This auto generated class provides the basic CRUD operations for the NotificationType entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the NotificationType entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class NotificationTypesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private SchedulerContext _context;

		private ILogger<NotificationTypesController> _logger;

		public NotificationTypesController(SchedulerContext context, ILogger<NotificationTypesController> logger) : base("Scheduler", "NotificationType")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of NotificationTypes filtered by the parameters provided.
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
		[Route("api/NotificationTypes")]
		public async Task<IActionResult> GetNotificationTypes(
			string name = null,
			string description = null,
			int? sequence = null,
			string color = null,
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

			IQueryable<Database.NotificationType> query = (from nt in _context.NotificationTypes select nt);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(nt => nt.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(nt => nt.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(nt => nt.sequence == sequence.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(nt => nt.color == color);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(nt => nt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(nt => nt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(nt => nt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(nt => nt.deleted == false);
				}
			}
			else
			{
				query = query.Where(nt => nt.active == true);
				query = query.Where(nt => nt.deleted == false);
			}

			query = query.OrderBy(nt => nt.sequence).ThenBy(nt => nt.name).ThenBy(nt => nt.description).ThenBy(nt => nt.color);


			//
			// Add the any string contains parameter to span all the string fields on the Notification Type, or on an any of the string fields on its immediate relations
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

			if (includeRelations == true)
			{
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.NotificationType> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.NotificationType notificationType in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(notificationType, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.NotificationType Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.NotificationType Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of NotificationTypes filtered by the parameters provided.  Its query is similar to the GetNotificationTypes method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationTypes/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			int? sequence = null,
			string color = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.NotificationType> query = (from nt in _context.NotificationTypes select nt);
			if (name != null)
			{
				query = query.Where(nt => nt.name == name);
			}
			if (description != null)
			{
				query = query.Where(nt => nt.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(nt => nt.sequence == sequence.Value);
			}
			if (color != null)
			{
				query = query.Where(nt => nt.color == color);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(nt => nt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(nt => nt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(nt => nt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(nt => nt.deleted == false);
				}
			}
			else
			{
				query = query.Where(nt => nt.active == true);
				query = query.Where(nt => nt.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Notification Type, or on an any of the string fields on its immediate relations
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
        /// This gets a single NotificationType by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationType/{id}")]
		public async Task<IActionResult> GetNotificationType(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.NotificationType> query = (from nt in _context.NotificationTypes where
							(nt.id == id) &&
							(userIsAdmin == true || nt.deleted == false) &&
							(userIsWriter == true || nt.active == true)
					select nt);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.NotificationType materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.NotificationType Entity was read with Admin privilege." : "Scheduler.NotificationType Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "NotificationType", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.NotificationType entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.NotificationType.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.NotificationType.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing NotificationType record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/NotificationType/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutNotificationType(int id, [FromBody]Database.NotificationType.NotificationTypeDTO notificationTypeDTO, CancellationToken cancellationToken = default)
		{
			if (notificationTypeDTO == null)
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



			if (id != notificationTypeDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.NotificationType> query = (from x in _context.NotificationTypes
				where
				(x.id == id)
				select x);


			Database.NotificationType existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.NotificationType PUT", id.ToString(), new Exception("No Scheduler.NotificationType entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (notificationTypeDTO.objectGuid == Guid.Empty)
            {
                notificationTypeDTO.objectGuid = existing.objectGuid;
            }
            else if (notificationTypeDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a NotificationType record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.NotificationType cloneOfExisting = (Database.NotificationType)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new NotificationType object using the data from the existing record, updated with what is in the DTO.
			//
			Database.NotificationType notificationType = (Database.NotificationType)_context.Entry(existing).GetDatabaseValues().ToObject();
			notificationType.ApplyDTO(notificationTypeDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (notificationType.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.NotificationType record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (notificationType.name != null && notificationType.name.Length > 100)
			{
				notificationType.name = notificationType.name.Substring(0, 100);
			}

			if (notificationType.description != null && notificationType.description.Length > 500)
			{
				notificationType.description = notificationType.description.Substring(0, 500);
			}

			if (notificationType.color != null && notificationType.color.Length > 10)
			{
				notificationType.color = notificationType.color.Substring(0, 10);
			}

			EntityEntry<Database.NotificationType> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(notificationType);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.NotificationType entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.NotificationType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.NotificationType.CreateAnonymousWithFirstLevelSubObjects(notificationType)),
					null);


				return Ok(Database.NotificationType.CreateAnonymous(notificationType));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.NotificationType entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.NotificationType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.NotificationType.CreateAnonymousWithFirstLevelSubObjects(notificationType)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new NotificationType record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationType", Name = "NotificationType")]
		public async Task<IActionResult> PostNotificationType([FromBody]Database.NotificationType.NotificationTypeDTO notificationTypeDTO, CancellationToken cancellationToken = default)
		{
			if (notificationTypeDTO == null)
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

			//
			// Create a new NotificationType object using the data from the DTO
			//
			Database.NotificationType notificationType = Database.NotificationType.FromDTO(notificationTypeDTO);

			try
			{
				if (notificationType.name != null && notificationType.name.Length > 100)
				{
					notificationType.name = notificationType.name.Substring(0, 100);
				}

				if (notificationType.description != null && notificationType.description.Length > 500)
				{
					notificationType.description = notificationType.description.Substring(0, 500);
				}

				if (notificationType.color != null && notificationType.color.Length > 10)
				{
					notificationType.color = notificationType.color.Substring(0, 10);
				}

				notificationType.objectGuid = Guid.NewGuid();
				_context.NotificationTypes.Add(notificationType);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.NotificationType entity successfully created.",
					true,
					notificationType.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.NotificationType.CreateAnonymousWithFirstLevelSubObjects(notificationType)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.NotificationType entity creation failed.", false, notificationType.id.ToString(), "", JsonSerializer.Serialize(notificationType), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "NotificationType", notificationType.id, notificationType.name));

			return CreatedAtRoute("NotificationType", new { id = notificationType.id }, Database.NotificationType.CreateAnonymousWithFirstLevelSubObjects(notificationType));
		}



        /// <summary>
        /// 
        /// This deletes a NotificationType record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationType/{id}")]
		[Route("api/NotificationType")]
		public async Task<IActionResult> DeleteNotificationType(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.NotificationType> query = (from x in _context.NotificationTypes
				where
				(x.id == id)
				select x);


			Database.NotificationType notificationType = await query.FirstOrDefaultAsync(cancellationToken);

			if (notificationType == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.NotificationType DELETE", id.ToString(), new Exception("No Scheduler.NotificationType entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.NotificationType cloneOfExisting = (Database.NotificationType)_context.Entry(notificationType).GetDatabaseValues().ToObject();


			try
			{
				notificationType.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.NotificationType entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.NotificationType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.NotificationType.CreateAnonymousWithFirstLevelSubObjects(notificationType)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.NotificationType entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.NotificationType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.NotificationType.CreateAnonymousWithFirstLevelSubObjects(notificationType)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of NotificationType records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/NotificationTypes/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			int? sequence = null,
			string color = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);


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

			IQueryable<Database.NotificationType> query = (from nt in _context.NotificationTypes select nt);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(nt => nt.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(nt => nt.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(nt => nt.sequence == sequence.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(nt => nt.color == color);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(nt => nt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(nt => nt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(nt => nt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(nt => nt.deleted == false);
				}
			}
			else
			{
				query = query.Where(nt => nt.active == true);
				query = query.Where(nt => nt.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Notification Type, or on an any of the string fields on its immediate relations
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
			return Ok(await (from queryData in query select Database.NotificationType.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/NotificationType/CreateAuditEvent")]
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
