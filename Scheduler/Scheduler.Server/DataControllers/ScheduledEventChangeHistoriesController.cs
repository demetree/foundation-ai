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
    /// This auto generated class provides the basic CRUD operations for the ScheduledEventChangeHistory entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ScheduledEventChangeHistory entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ScheduledEventChangeHistoriesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 10;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private SchedulerContext _context;

		private ILogger<ScheduledEventChangeHistoriesController> _logger;

		public ScheduledEventChangeHistoriesController(SchedulerContext context, ILogger<ScheduledEventChangeHistoriesController> logger) : base("Scheduler", "ScheduledEventChangeHistory")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ScheduledEventChangeHistories filtered by the parameters provided.
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
		[Route("api/ScheduledEventChangeHistories")]
		public async Task<IActionResult> GetScheduledEventChangeHistories(
			int? scheduledEventId = null,
			int? versionNumber = null,
			DateTime? timeStamp = null,
			int? userId = null,
			string data = null,
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
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.ScheduledEventChangeHistory> query = (from sech in _context.ScheduledEventChangeHistories select sech);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(sech => sech.scheduledEventId == scheduledEventId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(sech => sech.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(sech => sech.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(sech => sech.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(data) == false)
			{
				query = query.Where(sech => sech.data == data);
			}

			query = query.OrderByDescending(sech => sech.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.scheduledEvent);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Scheduled Event Change History, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.data.Contains(anyStringContains)
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
			
			List<Database.ScheduledEventChangeHistory> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ScheduledEventChangeHistory scheduledEventChangeHistory in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(scheduledEventChangeHistory, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ScheduledEventChangeHistory Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ScheduledEventChangeHistory Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ScheduledEventChangeHistories filtered by the parameters provided.  Its query is similar to the GetScheduledEventChangeHistories method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventChangeHistories/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? scheduledEventId = null,
			int? versionNumber = null,
			DateTime? timeStamp = null,
			int? userId = null,
			string data = null,
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
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.ScheduledEventChangeHistory> query = (from sech in _context.ScheduledEventChangeHistories select sech);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(sech => sech.scheduledEventId == scheduledEventId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(sech => sech.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(sech => sech.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(sech => sech.userId == userId.Value);
			}
			if (data != null)
			{
				query = query.Where(sech => sech.data == data);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Scheduled Event Change History, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.data.Contains(anyStringContains)
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
        /// This gets a single ScheduledEventChangeHistory by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventChangeHistory/{id}")]
		public async Task<IActionResult> GetScheduledEventChangeHistory(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
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
				IQueryable<Database.ScheduledEventChangeHistory> query = (from sech in _context.ScheduledEventChangeHistories where
				(sech.id == id)
					select sech);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.scheduledEvent);
					query = query.AsSplitQuery();
				}

				Database.ScheduledEventChangeHistory materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.ScheduledEventChangeHistory Entity was read with Admin privilege." : "Scheduler.ScheduledEventChangeHistory Entity was read.");

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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.ScheduledEventChangeHistory entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.ScheduledEventChangeHistory.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.ScheduledEventChangeHistory.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing ScheduledEventChangeHistory record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ScheduledEventChangeHistory/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutScheduledEventChangeHistory(int id, [FromBody]Database.ScheduledEventChangeHistory.ScheduledEventChangeHistoryDTO scheduledEventChangeHistoryDTO, CancellationToken cancellationToken = default)
		{
			if (scheduledEventChangeHistoryDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			// Admin privilege needed to write to this table.
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != scheduledEventChangeHistoryDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
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


			IQueryable<Database.ScheduledEventChangeHistory> query = (from x in _context.ScheduledEventChangeHistories
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ScheduledEventChangeHistory existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ScheduledEventChangeHistory PUT", id.ToString(), new Exception("No Scheduler.ScheduledEventChangeHistory entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ScheduledEventChangeHistory cloneOfExisting = (Database.ScheduledEventChangeHistory)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ScheduledEventChangeHistory object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ScheduledEventChangeHistory scheduledEventChangeHistory = (Database.ScheduledEventChangeHistory)_context.Entry(existing).GetDatabaseValues().ToObject();
			scheduledEventChangeHistory.ApplyDTO(scheduledEventChangeHistoryDTO);
			//
			// The tenant guid for any ScheduledEventChangeHistory being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ScheduledEventChangeHistory because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				scheduledEventChangeHistory.tenantGuid = existing.tenantGuid;
			}



			if (scheduledEventChangeHistory.timeStamp.Kind != DateTimeKind.Utc)
			{
				scheduledEventChangeHistory.timeStamp = scheduledEventChangeHistory.timeStamp.ToUniversalTime();
			}

			EntityEntry<Database.ScheduledEventChangeHistory> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(scheduledEventChangeHistory);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.ScheduledEventChangeHistory entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ScheduledEventChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ScheduledEventChangeHistory.CreateAnonymousWithFirstLevelSubObjects(scheduledEventChangeHistory)),
					null);


				return Ok(Database.ScheduledEventChangeHistory.CreateAnonymous(scheduledEventChangeHistory));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.ScheduledEventChangeHistory entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ScheduledEventChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ScheduledEventChangeHistory.CreateAnonymousWithFirstLevelSubObjects(scheduledEventChangeHistory)),
					ex);

				return Problem(ex.Message);
			}

		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new ScheduledEventChangeHistory record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventChangeHistory", Name = "ScheduledEventChangeHistory")]
		public async Task<IActionResult> PostScheduledEventChangeHistory([FromBody]Database.ScheduledEventChangeHistory.ScheduledEventChangeHistoryDTO scheduledEventChangeHistoryDTO, CancellationToken cancellationToken = default)
		{
			if (scheduledEventChangeHistoryDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			// Admin privilege needed to write to this table.
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

			//
			// Create a new ScheduledEventChangeHistory object using the data from the DTO
			//
			Database.ScheduledEventChangeHistory scheduledEventChangeHistory = Database.ScheduledEventChangeHistory.FromDTO(scheduledEventChangeHistoryDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				scheduledEventChangeHistory.tenantGuid = userTenantGuid;

				if (scheduledEventChangeHistory.timeStamp.Kind != DateTimeKind.Utc)
				{
					scheduledEventChangeHistory.timeStamp = scheduledEventChangeHistory.timeStamp.ToUniversalTime();
				}

				_context.ScheduledEventChangeHistories.Add(scheduledEventChangeHistory);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.ScheduledEventChangeHistory entity successfully created.",
					true,
					scheduledEventChangeHistory.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ScheduledEventChangeHistory.CreateAnonymousWithFirstLevelSubObjects(scheduledEventChangeHistory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.ScheduledEventChangeHistory entity creation failed.", false, scheduledEventChangeHistory.id.ToString(), "", JsonSerializer.Serialize(scheduledEventChangeHistory), ex);

				return Problem(ex.Message);
			}


			return CreatedAtRoute("ScheduledEventChangeHistory", new { id = scheduledEventChangeHistory.id }, Database.ScheduledEventChangeHistory.CreateAnonymousWithFirstLevelSubObjects(scheduledEventChangeHistory));
		}

*/


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a ScheduledEventChangeHistory record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventChangeHistory/{id}")]
		[Route("api/ScheduledEventChangeHistory")]
		public async Task<IActionResult> DeleteScheduledEventChangeHistory(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.ScheduledEventChangeHistory> query = (from x in _context.ScheduledEventChangeHistories
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ScheduledEventChangeHistory scheduledEventChangeHistory = await query.FirstOrDefaultAsync(cancellationToken);

			if (scheduledEventChangeHistory == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ScheduledEventChangeHistory DELETE", id.ToString(), new Exception("No Scheduler.ScheduledEventChangeHistory entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ScheduledEventChangeHistory cloneOfExisting = (Database.ScheduledEventChangeHistory)_context.Entry(scheduledEventChangeHistory).GetDatabaseValues().ToObject();


			try
			{
				_context.ScheduledEventChangeHistories.Remove(scheduledEventChangeHistory);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.ScheduledEventChangeHistory entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ScheduledEventChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ScheduledEventChangeHistory.CreateAnonymousWithFirstLevelSubObjects(scheduledEventChangeHistory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.ScheduledEventChangeHistory entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ScheduledEventChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ScheduledEventChangeHistory.CreateAnonymousWithFirstLevelSubObjects(scheduledEventChangeHistory)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


*/
        /// <summary>
        /// 
        /// This gets a list of ScheduledEventChangeHistory records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ScheduledEventChangeHistories/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? scheduledEventId = null,
			int? versionNumber = null,
			DateTime? timeStamp = null,
			int? userId = null,
			string data = null,
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

			IQueryable<Database.ScheduledEventChangeHistory> query = (from sech in _context.ScheduledEventChangeHistories select sech);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(sech => sech.scheduledEventId == scheduledEventId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(sech => sech.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(sech => sech.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(sech => sech.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(data) == false)
			{
				query = query.Where(sech => sech.data == data);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Scheduled Event Change History, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.data.Contains(anyStringContains)
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


			query = query.OrderByDescending (x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ScheduledEventChangeHistory.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ScheduledEventChangeHistory/CreateAuditEvent")]
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
