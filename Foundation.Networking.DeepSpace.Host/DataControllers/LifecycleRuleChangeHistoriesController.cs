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
using Foundation.DeepSpace.Database;

namespace Foundation.DeepSpace.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the LifecycleRuleChangeHistory entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the LifecycleRuleChangeHistory entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class LifecycleRuleChangeHistoriesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 10;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private DeepSpaceContext _context;

		private ILogger<LifecycleRuleChangeHistoriesController> _logger;

		public LifecycleRuleChangeHistoriesController(DeepSpaceContext context, ILogger<LifecycleRuleChangeHistoriesController> logger) : base("DeepSpace", "LifecycleRuleChangeHistory")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of LifecycleRuleChangeHistories filtered by the parameters provided.
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
		[Route("api/LifecycleRuleChangeHistories")]
		public async Task<IActionResult> GetLifecycleRuleChangeHistories(
			int? lifecycleRuleId = null,
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
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			IQueryable<Database.LifecycleRuleChangeHistory> query = (from lrch in _context.LifecycleRuleChangeHistories select lrch);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (lifecycleRuleId.HasValue == true)
			{
				query = query.Where(lrch => lrch.lifecycleRuleId == lifecycleRuleId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(lrch => lrch.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(lrch => lrch.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(lrch => lrch.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(data) == false)
			{
				query = query.Where(lrch => lrch.data == data);
			}

			query = query.OrderByDescending(lrch => lrch.id);

			if (includeRelations == true)
			{
				query = query.Include(x => x.lifecycleRule);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.LifecycleRuleChangeHistory> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.LifecycleRuleChangeHistory lifecycleRuleChangeHistory in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(lifecycleRuleChangeHistory, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "DeepSpace.LifecycleRuleChangeHistory Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "DeepSpace.LifecycleRuleChangeHistory Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of LifecycleRuleChangeHistories filtered by the parameters provided.  Its query is similar to the GetLifecycleRuleChangeHistories method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LifecycleRuleChangeHistories/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? lifecycleRuleId = null,
			int? versionNumber = null,
			DateTime? timeStamp = null,
			int? userId = null,
			string data = null,
			CancellationToken cancellationToken = default)
		{
			//
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			IQueryable<Database.LifecycleRuleChangeHistory> query = (from lrch in _context.LifecycleRuleChangeHistories select lrch);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (lifecycleRuleId.HasValue == true)
			{
				query = query.Where(lrch => lrch.lifecycleRuleId == lifecycleRuleId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(lrch => lrch.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(lrch => lrch.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(lrch => lrch.userId == userId.Value);
			}
			if (data != null)
			{
				query = query.Where(lrch => lrch.data == data);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single LifecycleRuleChangeHistory by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LifecycleRuleChangeHistory/{id}")]
		public async Task<IActionResult> GetLifecycleRuleChangeHistory(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
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
				IQueryable<Database.LifecycleRuleChangeHistory> query = (from lrch in _context.LifecycleRuleChangeHistories where
				(lrch.id == id)
					select lrch);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.lifecycleRule);
					query = query.AsSplitQuery();
				}

				Database.LifecycleRuleChangeHistory materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "DeepSpace.LifecycleRuleChangeHistory Entity was read with Admin privilege." : "DeepSpace.LifecycleRuleChangeHistory Entity was read.");

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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a DeepSpace.LifecycleRuleChangeHistory entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of DeepSpace.LifecycleRuleChangeHistory.   Entity was read with Admin privilege." : "Exception caught during entity read of DeepSpace.LifecycleRuleChangeHistory.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing LifecycleRuleChangeHistory record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/LifecycleRuleChangeHistory/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutLifecycleRuleChangeHistory(int id, [FromBody]Database.LifecycleRuleChangeHistory.LifecycleRuleChangeHistoryDTO lifecycleRuleChangeHistoryDTO, CancellationToken cancellationToken = default)
		{
			if (lifecycleRuleChangeHistoryDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// DeepSpace Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != lifecycleRuleChangeHistoryDTO.id)
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


			IQueryable<Database.LifecycleRuleChangeHistory> query = (from x in _context.LifecycleRuleChangeHistories
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.LifecycleRuleChangeHistory existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for DeepSpace.LifecycleRuleChangeHistory PUT", id.ToString(), new Exception("No DeepSpace.LifecycleRuleChangeHistory entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.LifecycleRuleChangeHistory cloneOfExisting = (Database.LifecycleRuleChangeHistory)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new LifecycleRuleChangeHistory object using the data from the existing record, updated with what is in the DTO.
			//
			Database.LifecycleRuleChangeHistory lifecycleRuleChangeHistory = (Database.LifecycleRuleChangeHistory)_context.Entry(existing).GetDatabaseValues().ToObject();
			lifecycleRuleChangeHistory.ApplyDTO(lifecycleRuleChangeHistoryDTO);
			//
			// The tenant guid for any LifecycleRuleChangeHistory being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the LifecycleRuleChangeHistory because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				lifecycleRuleChangeHistory.tenantGuid = existing.tenantGuid;
			}



			if (lifecycleRuleChangeHistory.timeStamp.Kind != DateTimeKind.Utc)
			{
				lifecycleRuleChangeHistory.timeStamp = lifecycleRuleChangeHistory.timeStamp.ToUniversalTime();
			}

			EntityEntry<Database.LifecycleRuleChangeHistory> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(lifecycleRuleChangeHistory);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"DeepSpace.LifecycleRuleChangeHistory entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.LifecycleRuleChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LifecycleRuleChangeHistory.CreateAnonymousWithFirstLevelSubObjects(lifecycleRuleChangeHistory)),
					null);


				return Ok(Database.LifecycleRuleChangeHistory.CreateAnonymous(lifecycleRuleChangeHistory));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"DeepSpace.LifecycleRuleChangeHistory entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.LifecycleRuleChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LifecycleRuleChangeHistory.CreateAnonymousWithFirstLevelSubObjects(lifecycleRuleChangeHistory)),
					ex);

				return Problem(ex.Message);
			}

		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new LifecycleRuleChangeHistory record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LifecycleRuleChangeHistory", Name = "LifecycleRuleChangeHistory")]
		public async Task<IActionResult> PostLifecycleRuleChangeHistory([FromBody]Database.LifecycleRuleChangeHistory.LifecycleRuleChangeHistoryDTO lifecycleRuleChangeHistoryDTO, CancellationToken cancellationToken = default)
		{
			if (lifecycleRuleChangeHistoryDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// DeepSpace Administrator role needed to write to this table.
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
			// Create a new LifecycleRuleChangeHistory object using the data from the DTO
			//
			Database.LifecycleRuleChangeHistory lifecycleRuleChangeHistory = Database.LifecycleRuleChangeHistory.FromDTO(lifecycleRuleChangeHistoryDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				lifecycleRuleChangeHistory.tenantGuid = userTenantGuid;

				if (lifecycleRuleChangeHistory.timeStamp.Kind != DateTimeKind.Utc)
				{
					lifecycleRuleChangeHistory.timeStamp = lifecycleRuleChangeHistory.timeStamp.ToUniversalTime();
				}

				_context.LifecycleRuleChangeHistories.Add(lifecycleRuleChangeHistory);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"DeepSpace.LifecycleRuleChangeHistory entity successfully created.",
					true,
					lifecycleRuleChangeHistory.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.LifecycleRuleChangeHistory.CreateAnonymousWithFirstLevelSubObjects(lifecycleRuleChangeHistory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "DeepSpace.LifecycleRuleChangeHistory entity creation failed.", false, lifecycleRuleChangeHistory.id.ToString(), "", JsonSerializer.Serialize(lifecycleRuleChangeHistory), ex);

				return Problem(ex.Message);
			}


			return CreatedAtRoute("LifecycleRuleChangeHistory", new { id = lifecycleRuleChangeHistory.id }, Database.LifecycleRuleChangeHistory.CreateAnonymousWithFirstLevelSubObjects(lifecycleRuleChangeHistory));
		}

*/


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a LifecycleRuleChangeHistory record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LifecycleRuleChangeHistory/{id}")]
		[Route("api/LifecycleRuleChangeHistory")]
		public async Task<IActionResult> DeleteLifecycleRuleChangeHistory(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// DeepSpace Administrator role needed to write to this table.
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

			IQueryable<Database.LifecycleRuleChangeHistory> query = (from x in _context.LifecycleRuleChangeHistories
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.LifecycleRuleChangeHistory lifecycleRuleChangeHistory = await query.FirstOrDefaultAsync(cancellationToken);

			if (lifecycleRuleChangeHistory == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for DeepSpace.LifecycleRuleChangeHistory DELETE", id.ToString(), new Exception("No DeepSpace.LifecycleRuleChangeHistory entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.LifecycleRuleChangeHistory cloneOfExisting = (Database.LifecycleRuleChangeHistory)_context.Entry(lifecycleRuleChangeHistory).GetDatabaseValues().ToObject();


			try
			{
				_context.LifecycleRuleChangeHistories.Remove(lifecycleRuleChangeHistory);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"DeepSpace.LifecycleRuleChangeHistory entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.LifecycleRuleChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LifecycleRuleChangeHistory.CreateAnonymousWithFirstLevelSubObjects(lifecycleRuleChangeHistory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"DeepSpace.LifecycleRuleChangeHistory entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.LifecycleRuleChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LifecycleRuleChangeHistory.CreateAnonymousWithFirstLevelSubObjects(lifecycleRuleChangeHistory)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


*/
        /// <summary>
        /// 
        /// This gets a list of LifecycleRuleChangeHistory records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/LifecycleRuleChangeHistories/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? lifecycleRuleId = null,
			int? versionNumber = null,
			DateTime? timeStamp = null,
			int? userId = null,
			string data = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			IQueryable<Database.LifecycleRuleChangeHistory> query = (from lrch in _context.LifecycleRuleChangeHistories select lrch);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (lifecycleRuleId.HasValue == true)
			{
				query = query.Where(lrch => lrch.lifecycleRuleId == lifecycleRuleId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(lrch => lrch.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(lrch => lrch.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(lrch => lrch.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(data) == false)
			{
				query = query.Where(lrch => lrch.data == data);
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderByDescending (x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.LifecycleRuleChangeHistory.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/LifecycleRuleChangeHistory/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// DeepSpace Administrator role needed to write to this table.
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
