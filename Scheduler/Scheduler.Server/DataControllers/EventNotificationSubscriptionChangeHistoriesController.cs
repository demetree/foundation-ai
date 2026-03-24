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
    /// This auto generated class provides the basic CRUD operations for the EventNotificationSubscriptionChangeHistory entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the EventNotificationSubscriptionChangeHistory entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class EventNotificationSubscriptionChangeHistoriesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 10;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private SchedulerContext _context;

		private ILogger<EventNotificationSubscriptionChangeHistoriesController> _logger;

		public EventNotificationSubscriptionChangeHistoriesController(SchedulerContext context, ILogger<EventNotificationSubscriptionChangeHistoriesController> logger) : base("Scheduler", "EventNotificationSubscriptionChangeHistory")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of EventNotificationSubscriptionChangeHistories filtered by the parameters provided.
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
		[Route("api/EventNotificationSubscriptionChangeHistories")]
		public async Task<IActionResult> GetEventNotificationSubscriptionChangeHistories(
			int? eventNotificationSubscriptionId = null,
			int? versionNumber = null,
			DateTime? timeStamp = null,
			int? userId = null,
			string data = null,
			int? pageSize = null,
			int? pageNumber = null,
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
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.EventNotificationSubscriptionChangeHistory> query = (from ensch in _context.EventNotificationSubscriptionChangeHistories select ensch);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (eventNotificationSubscriptionId.HasValue == true)
			{
				query = query.Where(ensch => ensch.eventNotificationSubscriptionId == eventNotificationSubscriptionId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ensch => ensch.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(ensch => ensch.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(ensch => ensch.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(data) == false)
			{
				query = query.Where(ensch => ensch.data == data);
			}

			query = query.OrderByDescending(ensch => ensch.id);

			if (includeRelations == true)
			{
				query = query.Include(x => x.eventNotificationSubscription);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.EventNotificationSubscriptionChangeHistory> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.EventNotificationSubscriptionChangeHistory eventNotificationSubscriptionChangeHistory in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(eventNotificationSubscriptionChangeHistory, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.EventNotificationSubscriptionChangeHistory Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.EventNotificationSubscriptionChangeHistory Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of EventNotificationSubscriptionChangeHistories filtered by the parameters provided.  Its query is similar to the GetEventNotificationSubscriptionChangeHistories method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventNotificationSubscriptionChangeHistories/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? eventNotificationSubscriptionId = null,
			int? versionNumber = null,
			DateTime? timeStamp = null,
			int? userId = null,
			string data = null,
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
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.EventNotificationSubscriptionChangeHistory> query = (from ensch in _context.EventNotificationSubscriptionChangeHistories select ensch);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (eventNotificationSubscriptionId.HasValue == true)
			{
				query = query.Where(ensch => ensch.eventNotificationSubscriptionId == eventNotificationSubscriptionId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ensch => ensch.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(ensch => ensch.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(ensch => ensch.userId == userId.Value);
			}
			if (data != null)
			{
				query = query.Where(ensch => ensch.data == data);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single EventNotificationSubscriptionChangeHistory by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventNotificationSubscriptionChangeHistory/{id}")]
		public async Task<IActionResult> GetEventNotificationSubscriptionChangeHistory(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.EventNotificationSubscriptionChangeHistory> query = (from ensch in _context.EventNotificationSubscriptionChangeHistories where
				(ensch.id == id)
					select ensch);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.eventNotificationSubscription);
					query = query.AsSplitQuery();
				}

				Database.EventNotificationSubscriptionChangeHistory materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.EventNotificationSubscriptionChangeHistory Entity was read with Admin privilege." : "Scheduler.EventNotificationSubscriptionChangeHistory Entity was read.");

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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.EventNotificationSubscriptionChangeHistory entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.EventNotificationSubscriptionChangeHistory.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.EventNotificationSubscriptionChangeHistory.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing EventNotificationSubscriptionChangeHistory record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/EventNotificationSubscriptionChangeHistory/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutEventNotificationSubscriptionChangeHistory(int id, [FromBody]Database.EventNotificationSubscriptionChangeHistory.EventNotificationSubscriptionChangeHistoryDTO eventNotificationSubscriptionChangeHistoryDTO, CancellationToken cancellationToken = default)
		{
			if (eventNotificationSubscriptionChangeHistoryDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != eventNotificationSubscriptionChangeHistoryDTO.id)
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


			IQueryable<Database.EventNotificationSubscriptionChangeHistory> query = (from x in _context.EventNotificationSubscriptionChangeHistories
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.EventNotificationSubscriptionChangeHistory existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.EventNotificationSubscriptionChangeHistory PUT", id.ToString(), new Exception("No Scheduler.EventNotificationSubscriptionChangeHistory entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.EventNotificationSubscriptionChangeHistory cloneOfExisting = (Database.EventNotificationSubscriptionChangeHistory)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new EventNotificationSubscriptionChangeHistory object using the data from the existing record, updated with what is in the DTO.
			//
			Database.EventNotificationSubscriptionChangeHistory eventNotificationSubscriptionChangeHistory = (Database.EventNotificationSubscriptionChangeHistory)_context.Entry(existing).GetDatabaseValues().ToObject();
			eventNotificationSubscriptionChangeHistory.ApplyDTO(eventNotificationSubscriptionChangeHistoryDTO);
			//
			// The tenant guid for any EventNotificationSubscriptionChangeHistory being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the EventNotificationSubscriptionChangeHistory because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				eventNotificationSubscriptionChangeHistory.tenantGuid = existing.tenantGuid;
			}



			if (eventNotificationSubscriptionChangeHistory.timeStamp.Kind != DateTimeKind.Utc)
			{
				eventNotificationSubscriptionChangeHistory.timeStamp = eventNotificationSubscriptionChangeHistory.timeStamp.ToUniversalTime();
			}

			EntityEntry<Database.EventNotificationSubscriptionChangeHistory> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(eventNotificationSubscriptionChangeHistory);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.EventNotificationSubscriptionChangeHistory entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.EventNotificationSubscriptionChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.EventNotificationSubscriptionChangeHistory.CreateAnonymousWithFirstLevelSubObjects(eventNotificationSubscriptionChangeHistory)),
					null);


				return Ok(Database.EventNotificationSubscriptionChangeHistory.CreateAnonymous(eventNotificationSubscriptionChangeHistory));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.EventNotificationSubscriptionChangeHistory entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.EventNotificationSubscriptionChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.EventNotificationSubscriptionChangeHistory.CreateAnonymousWithFirstLevelSubObjects(eventNotificationSubscriptionChangeHistory)),
					ex);

				return Problem(ex.Message);
			}

		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new EventNotificationSubscriptionChangeHistory record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventNotificationSubscriptionChangeHistory", Name = "EventNotificationSubscriptionChangeHistory")]
		public async Task<IActionResult> PostEventNotificationSubscriptionChangeHistory([FromBody]Database.EventNotificationSubscriptionChangeHistory.EventNotificationSubscriptionChangeHistoryDTO eventNotificationSubscriptionChangeHistoryDTO, CancellationToken cancellationToken = default)
		{
			if (eventNotificationSubscriptionChangeHistoryDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Administrator role needed to write to this table.
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
			// Create a new EventNotificationSubscriptionChangeHistory object using the data from the DTO
			//
			Database.EventNotificationSubscriptionChangeHistory eventNotificationSubscriptionChangeHistory = Database.EventNotificationSubscriptionChangeHistory.FromDTO(eventNotificationSubscriptionChangeHistoryDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				eventNotificationSubscriptionChangeHistory.tenantGuid = userTenantGuid;

				if (eventNotificationSubscriptionChangeHistory.timeStamp.Kind != DateTimeKind.Utc)
				{
					eventNotificationSubscriptionChangeHistory.timeStamp = eventNotificationSubscriptionChangeHistory.timeStamp.ToUniversalTime();
				}

				_context.EventNotificationSubscriptionChangeHistories.Add(eventNotificationSubscriptionChangeHistory);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.EventNotificationSubscriptionChangeHistory entity successfully created.",
					true,
					eventNotificationSubscriptionChangeHistory.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.EventNotificationSubscriptionChangeHistory.CreateAnonymousWithFirstLevelSubObjects(eventNotificationSubscriptionChangeHistory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.EventNotificationSubscriptionChangeHistory entity creation failed.", false, eventNotificationSubscriptionChangeHistory.id.ToString(), "", JsonSerializer.Serialize(eventNotificationSubscriptionChangeHistory), ex);

				return Problem(ex.Message);
			}


			return CreatedAtRoute("EventNotificationSubscriptionChangeHistory", new { id = eventNotificationSubscriptionChangeHistory.id }, Database.EventNotificationSubscriptionChangeHistory.CreateAnonymousWithFirstLevelSubObjects(eventNotificationSubscriptionChangeHistory));
		}

*/


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a EventNotificationSubscriptionChangeHistory record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventNotificationSubscriptionChangeHistory/{id}")]
		[Route("api/EventNotificationSubscriptionChangeHistory")]
		public async Task<IActionResult> DeleteEventNotificationSubscriptionChangeHistory(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Administrator role needed to write to this table.
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

			IQueryable<Database.EventNotificationSubscriptionChangeHistory> query = (from x in _context.EventNotificationSubscriptionChangeHistories
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.EventNotificationSubscriptionChangeHistory eventNotificationSubscriptionChangeHistory = await query.FirstOrDefaultAsync(cancellationToken);

			if (eventNotificationSubscriptionChangeHistory == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.EventNotificationSubscriptionChangeHistory DELETE", id.ToString(), new Exception("No Scheduler.EventNotificationSubscriptionChangeHistory entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.EventNotificationSubscriptionChangeHistory cloneOfExisting = (Database.EventNotificationSubscriptionChangeHistory)_context.Entry(eventNotificationSubscriptionChangeHistory).GetDatabaseValues().ToObject();


			try
			{
				_context.EventNotificationSubscriptionChangeHistories.Remove(eventNotificationSubscriptionChangeHistory);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.EventNotificationSubscriptionChangeHistory entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.EventNotificationSubscriptionChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.EventNotificationSubscriptionChangeHistory.CreateAnonymousWithFirstLevelSubObjects(eventNotificationSubscriptionChangeHistory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.EventNotificationSubscriptionChangeHistory entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.EventNotificationSubscriptionChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.EventNotificationSubscriptionChangeHistory.CreateAnonymousWithFirstLevelSubObjects(eventNotificationSubscriptionChangeHistory)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


*/
        /// <summary>
        /// 
        /// This gets a list of EventNotificationSubscriptionChangeHistory records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/EventNotificationSubscriptionChangeHistories/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? eventNotificationSubscriptionId = null,
			int? versionNumber = null,
			DateTime? timeStamp = null,
			int? userId = null,
			string data = null,
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
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.EventNotificationSubscriptionChangeHistory> query = (from ensch in _context.EventNotificationSubscriptionChangeHistories select ensch);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (eventNotificationSubscriptionId.HasValue == true)
			{
				query = query.Where(ensch => ensch.eventNotificationSubscriptionId == eventNotificationSubscriptionId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ensch => ensch.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(ensch => ensch.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(ensch => ensch.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(data) == false)
			{
				query = query.Where(ensch => ensch.data == data);
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderByDescending (x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.EventNotificationSubscriptionChangeHistory.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/EventNotificationSubscriptionChangeHistory/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Administrator role needed to write to this table.
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
