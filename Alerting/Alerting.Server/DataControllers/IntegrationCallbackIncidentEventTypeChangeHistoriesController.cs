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
using Foundation.Alerting.Database;

namespace Foundation.Alerting.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the IntegrationCallbackIncidentEventTypeChangeHistory entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the IntegrationCallbackIncidentEventTypeChangeHistory entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class IntegrationCallbackIncidentEventTypeChangeHistoriesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 10;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private AlertingContext _context;

		private ILogger<IntegrationCallbackIncidentEventTypeChangeHistoriesController> _logger;

		public IntegrationCallbackIncidentEventTypeChangeHistoriesController(AlertingContext context, ILogger<IntegrationCallbackIncidentEventTypeChangeHistoriesController> logger) : base("Alerting", "IntegrationCallbackIncidentEventTypeChangeHistory")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of IntegrationCallbackIncidentEventTypeChangeHistories filtered by the parameters provided.
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
		[Route("api/IntegrationCallbackIncidentEventTypeChangeHistories")]
		public async Task<IActionResult> GetIntegrationCallbackIncidentEventTypeChangeHistories(
			int? integrationCallbackIncidentEventTypeId = null,
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

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			IQueryable<Database.IntegrationCallbackIncidentEventTypeChangeHistory> query = (from icietch in _context.IntegrationCallbackIncidentEventTypeChangeHistories select icietch);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (integrationCallbackIncidentEventTypeId.HasValue == true)
			{
				query = query.Where(icietch => icietch.integrationCallbackIncidentEventTypeId == integrationCallbackIncidentEventTypeId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(icietch => icietch.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(icietch => icietch.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(icietch => icietch.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(data) == false)
			{
				query = query.Where(icietch => icietch.data == data);
			}

			query = query.OrderByDescending(icietch => icietch.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.integrationCallbackIncidentEventType);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Integration Callback Incident Event Type Change History, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.data.Contains(anyStringContains)
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.IntegrationCallbackIncidentEventTypeChangeHistory> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.IntegrationCallbackIncidentEventTypeChangeHistory integrationCallbackIncidentEventTypeChangeHistory in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(integrationCallbackIncidentEventTypeChangeHistory, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Alerting.IntegrationCallbackIncidentEventTypeChangeHistory Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Alerting.IntegrationCallbackIncidentEventTypeChangeHistory Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of IntegrationCallbackIncidentEventTypeChangeHistories filtered by the parameters provided.  Its query is similar to the GetIntegrationCallbackIncidentEventTypeChangeHistories method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/IntegrationCallbackIncidentEventTypeChangeHistories/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? integrationCallbackIncidentEventTypeId = null,
			int? versionNumber = null,
			DateTime? timeStamp = null,
			int? userId = null,
			string data = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			IQueryable<Database.IntegrationCallbackIncidentEventTypeChangeHistory> query = (from icietch in _context.IntegrationCallbackIncidentEventTypeChangeHistories select icietch);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (integrationCallbackIncidentEventTypeId.HasValue == true)
			{
				query = query.Where(icietch => icietch.integrationCallbackIncidentEventTypeId == integrationCallbackIncidentEventTypeId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(icietch => icietch.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(icietch => icietch.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(icietch => icietch.userId == userId.Value);
			}
			if (data != null)
			{
				query = query.Where(icietch => icietch.data == data);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Integration Callback Incident Event Type Change History, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.data.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single IntegrationCallbackIncidentEventTypeChangeHistory by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/IntegrationCallbackIncidentEventTypeChangeHistory/{id}")]
		public async Task<IActionResult> GetIntegrationCallbackIncidentEventTypeChangeHistory(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
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
				IQueryable<Database.IntegrationCallbackIncidentEventTypeChangeHistory> query = (from icietch in _context.IntegrationCallbackIncidentEventTypeChangeHistories where
				(icietch.id == id)
					select icietch);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.integrationCallbackIncidentEventType);
					query = query.AsSplitQuery();
				}

				Database.IntegrationCallbackIncidentEventTypeChangeHistory materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Alerting.IntegrationCallbackIncidentEventTypeChangeHistory Entity was read with Admin privilege." : "Alerting.IntegrationCallbackIncidentEventTypeChangeHistory Entity was read.");

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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Alerting.IntegrationCallbackIncidentEventTypeChangeHistory entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Alerting.IntegrationCallbackIncidentEventTypeChangeHistory.   Entity was read with Admin privilege." : "Exception caught during entity read of Alerting.IntegrationCallbackIncidentEventTypeChangeHistory.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing IntegrationCallbackIncidentEventTypeChangeHistory record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/IntegrationCallbackIncidentEventTypeChangeHistory/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutIntegrationCallbackIncidentEventTypeChangeHistory(int id, [FromBody]Database.IntegrationCallbackIncidentEventTypeChangeHistory.IntegrationCallbackIncidentEventTypeChangeHistoryDTO integrationCallbackIncidentEventTypeChangeHistoryDTO, CancellationToken cancellationToken = default)
		{
			if (integrationCallbackIncidentEventTypeChangeHistoryDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Alerting Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != integrationCallbackIncidentEventTypeChangeHistoryDTO.id)
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


			IQueryable<Database.IntegrationCallbackIncidentEventTypeChangeHistory> query = (from x in _alertingContext.IntegrationCallbackIncidentEventTypeChangeHistories
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.IntegrationCallbackIncidentEventTypeChangeHistory existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.IntegrationCallbackIncidentEventTypeChangeHistory PUT", id.ToString(), new Exception("No Alerting.IntegrationCallbackIncidentEventTypeChangeHistory entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.IntegrationCallbackIncidentEventTypeChangeHistory cloneOfExisting = (Database.IntegrationCallbackIncidentEventTypeChangeHistory)_alertingContext.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new IntegrationCallbackIncidentEventTypeChangeHistory object using the data from the existing record, updated with what is in the DTO.
			//
			Database.IntegrationCallbackIncidentEventTypeChangeHistory integrationCallbackIncidentEventTypeChangeHistory = (Database.IntegrationCallbackIncidentEventTypeChangeHistory)_alertingContext.Entry(existing).GetDatabaseValues().ToObject();
			integrationCallbackIncidentEventTypeChangeHistory.ApplyDTO(integrationCallbackIncidentEventTypeChangeHistoryDTO);
			//
			// The tenant guid for any IntegrationCallbackIncidentEventTypeChangeHistory being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the IntegrationCallbackIncidentEventTypeChangeHistory because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				integrationCallbackIncidentEventTypeChangeHistory.tenantGuid = existing.tenantGuid;
			}



			if (integrationCallbackIncidentEventTypeChangeHistory.timeStamp.Kind != DateTimeKind.Utc)
			{
				integrationCallbackIncidentEventTypeChangeHistory.timeStamp = integrationCallbackIncidentEventTypeChangeHistory.timeStamp.ToUniversalTime();
			}

			EntityEntry<Database.IntegrationCallbackIncidentEventTypeChangeHistory> attached = _alertingContext.Entry(existing);
			attached.CurrentValues.SetValues(integrationCallbackIncidentEventTypeChangeHistory);

			try
			{
				await _alertingContext.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Alerting.IntegrationCallbackIncidentEventTypeChangeHistory entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.IntegrationCallbackIncidentEventTypeChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.IntegrationCallbackIncidentEventTypeChangeHistory.CreateAnonymousWithFirstLevelSubObjects(integrationCallbackIncidentEventTypeChangeHistory)),
					null);


				return Ok(Database.IntegrationCallbackIncidentEventTypeChangeHistory.CreateAnonymous(integrationCallbackIncidentEventTypeChangeHistory));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Alerting.IntegrationCallbackIncidentEventTypeChangeHistory entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.IntegrationCallbackIncidentEventTypeChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.IntegrationCallbackIncidentEventTypeChangeHistory.CreateAnonymousWithFirstLevelSubObjects(integrationCallbackIncidentEventTypeChangeHistory)),
					ex);

				return Problem(ex.Message);
			}

		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new IntegrationCallbackIncidentEventTypeChangeHistory record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/IntegrationCallbackIncidentEventTypeChangeHistory", Name = "IntegrationCallbackIncidentEventTypeChangeHistory")]
		public async Task<IActionResult> PostIntegrationCallbackIncidentEventTypeChangeHistory([FromBody]Database.IntegrationCallbackIncidentEventTypeChangeHistory.IntegrationCallbackIncidentEventTypeChangeHistoryDTO integrationCallbackIncidentEventTypeChangeHistoryDTO, CancellationToken cancellationToken = default)
		{
			if (integrationCallbackIncidentEventTypeChangeHistoryDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Alerting Administrator role needed to write to this table.
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
			// Create a new IntegrationCallbackIncidentEventTypeChangeHistory object using the data from the DTO
			//
			Database.IntegrationCallbackIncidentEventTypeChangeHistory integrationCallbackIncidentEventTypeChangeHistory = Database.IntegrationCallbackIncidentEventTypeChangeHistory.FromDTO(integrationCallbackIncidentEventTypeChangeHistoryDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				integrationCallbackIncidentEventTypeChangeHistory.tenantGuid = userTenantGuid;

				if (integrationCallbackIncidentEventTypeChangeHistory.timeStamp.Kind != DateTimeKind.Utc)
				{
					integrationCallbackIncidentEventTypeChangeHistory.timeStamp = integrationCallbackIncidentEventTypeChangeHistory.timeStamp.ToUniversalTime();
				}

				_alertingContext.IntegrationCallbackIncidentEventTypeChangeHistories.Add(integrationCallbackIncidentEventTypeChangeHistory);
				await _alertingContext.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Alerting.IntegrationCallbackIncidentEventTypeChangeHistory entity successfully created.",
					true,
					integrationCallbackIncidentEventTypeChangeHistory.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.IntegrationCallbackIncidentEventTypeChangeHistory.CreateAnonymousWithFirstLevelSubObjects(integrationCallbackIncidentEventTypeChangeHistory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Alerting.IntegrationCallbackIncidentEventTypeChangeHistory entity creation failed.", false, integrationCallbackIncidentEventTypeChangeHistory.id.ToString(), "", JsonSerializer.Serialize(integrationCallbackIncidentEventTypeChangeHistory), ex);

				return Problem(ex.Message);
			}


			return CreatedAtRoute("IntegrationCallbackIncidentEventTypeChangeHistory", new { id = integrationCallbackIncidentEventTypeChangeHistory.id }, Database.IntegrationCallbackIncidentEventTypeChangeHistory.CreateAnonymousWithFirstLevelSubObjects(integrationCallbackIncidentEventTypeChangeHistory));
		}

*/


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a IntegrationCallbackIncidentEventTypeChangeHistory record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/IntegrationCallbackIncidentEventTypeChangeHistory/{id}")]
		[Route("api/IntegrationCallbackIncidentEventTypeChangeHistory")]
		public async Task<IActionResult> DeleteIntegrationCallbackIncidentEventTypeChangeHistory(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Alerting Administrator role needed to write to this table.
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

			IQueryable<Database.IntegrationCallbackIncidentEventTypeChangeHistory> query = (from x in _alertingContext.IntegrationCallbackIncidentEventTypeChangeHistories
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.IntegrationCallbackIncidentEventTypeChangeHistory integrationCallbackIncidentEventTypeChangeHistory = await query.FirstOrDefaultAsync(cancellationToken);

			if (integrationCallbackIncidentEventTypeChangeHistory == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.IntegrationCallbackIncidentEventTypeChangeHistory DELETE", id.ToString(), new Exception("No Alerting.IntegrationCallbackIncidentEventTypeChangeHistory entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.IntegrationCallbackIncidentEventTypeChangeHistory cloneOfExisting = (Database.IntegrationCallbackIncidentEventTypeChangeHistory)_alertingContext.Entry(integrationCallbackIncidentEventTypeChangeHistory).GetDatabaseValues().ToObject();


			try
			{
				_alertingContext.IntegrationCallbackIncidentEventTypeChangeHistories.Remove(integrationCallbackIncidentEventTypeChangeHistory);
				await _alertingContext.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Alerting.IntegrationCallbackIncidentEventTypeChangeHistory entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.IntegrationCallbackIncidentEventTypeChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.IntegrationCallbackIncidentEventTypeChangeHistory.CreateAnonymousWithFirstLevelSubObjects(integrationCallbackIncidentEventTypeChangeHistory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Alerting.IntegrationCallbackIncidentEventTypeChangeHistory entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.IntegrationCallbackIncidentEventTypeChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.IntegrationCallbackIncidentEventTypeChangeHistory.CreateAnonymousWithFirstLevelSubObjects(integrationCallbackIncidentEventTypeChangeHistory)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


*/
        /// <summary>
        /// 
        /// This gets a list of IntegrationCallbackIncidentEventTypeChangeHistory records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/IntegrationCallbackIncidentEventTypeChangeHistories/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? integrationCallbackIncidentEventTypeId = null,
			int? versionNumber = null,
			DateTime? timeStamp = null,
			int? userId = null,
			string data = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			IQueryable<Database.IntegrationCallbackIncidentEventTypeChangeHistory> query = (from icietch in _context.IntegrationCallbackIncidentEventTypeChangeHistories select icietch);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (integrationCallbackIncidentEventTypeId.HasValue == true)
			{
				query = query.Where(icietch => icietch.integrationCallbackIncidentEventTypeId == integrationCallbackIncidentEventTypeId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(icietch => icietch.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(icietch => icietch.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(icietch => icietch.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(data) == false)
			{
				query = query.Where(icietch => icietch.data == data);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Integration Callback Incident Event Type Change History, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.data.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderByDescending (x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.IntegrationCallbackIncidentEventTypeChangeHistory.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/IntegrationCallbackIncidentEventTypeChangeHistory/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Administrator role needed to write to this table.
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
