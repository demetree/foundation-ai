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
    /// This auto generated class provides the basic CRUD operations for the TenantProfileChangeHistory entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the TenantProfileChangeHistory entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class TenantProfileChangeHistoriesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 10;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private SchedulerContext _context;

		private ILogger<TenantProfileChangeHistoriesController> _logger;

		public TenantProfileChangeHistoriesController(SchedulerContext context, ILogger<TenantProfileChangeHistoriesController> logger) : base("Scheduler", "TenantProfileChangeHistory")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of TenantProfileChangeHistories filtered by the parameters provided.
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
		[Route("api/TenantProfileChangeHistories")]
		public async Task<IActionResult> GetTenantProfileChangeHistories(
			int? tenantProfileId = null,
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

			IQueryable<Database.TenantProfileChangeHistory> query = (from tpch in _context.TenantProfileChangeHistories select tpch);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (tenantProfileId.HasValue == true)
			{
				query = query.Where(tpch => tpch.tenantProfileId == tenantProfileId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(tpch => tpch.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(tpch => tpch.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(tpch => tpch.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(data) == false)
			{
				query = query.Where(tpch => tpch.data == data);
			}

			query = query.OrderByDescending(tpch => tpch.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.tenantProfile);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Tenant Profile Change History, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.data.Contains(anyStringContains)
			       || (includeRelations == true && x.tenantProfile.name.Contains(anyStringContains))
			       || (includeRelations == true && x.tenantProfile.description.Contains(anyStringContains))
			       || (includeRelations == true && x.tenantProfile.companyLogoFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.tenantProfile.companyLogoMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.tenantProfile.addressLine1.Contains(anyStringContains))
			       || (includeRelations == true && x.tenantProfile.addressLine2.Contains(anyStringContains))
			       || (includeRelations == true && x.tenantProfile.addressLine3.Contains(anyStringContains))
			       || (includeRelations == true && x.tenantProfile.city.Contains(anyStringContains))
			       || (includeRelations == true && x.tenantProfile.postalCode.Contains(anyStringContains))
			       || (includeRelations == true && x.tenantProfile.phoneNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.tenantProfile.email.Contains(anyStringContains))
			       || (includeRelations == true && x.tenantProfile.website.Contains(anyStringContains))
			       || (includeRelations == true && x.tenantProfile.primaryColor.Contains(anyStringContains))
			       || (includeRelations == true && x.tenantProfile.secondaryColor.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.TenantProfileChangeHistory> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.TenantProfileChangeHistory tenantProfileChangeHistory in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(tenantProfileChangeHistory, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.TenantProfileChangeHistory Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.TenantProfileChangeHistory Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of TenantProfileChangeHistories filtered by the parameters provided.  Its query is similar to the GetTenantProfileChangeHistories method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TenantProfileChangeHistories/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? tenantProfileId = null,
			int? versionNumber = null,
			DateTime? timeStamp = null,
			int? userId = null,
			string data = null,
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

			IQueryable<Database.TenantProfileChangeHistory> query = (from tpch in _context.TenantProfileChangeHistories select tpch);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (tenantProfileId.HasValue == true)
			{
				query = query.Where(tpch => tpch.tenantProfileId == tenantProfileId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(tpch => tpch.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(tpch => tpch.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(tpch => tpch.userId == userId.Value);
			}
			if (data != null)
			{
				query = query.Where(tpch => tpch.data == data);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Tenant Profile Change History, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.data.Contains(anyStringContains)
			       || x.tenantProfile.name.Contains(anyStringContains)
			       || x.tenantProfile.description.Contains(anyStringContains)
			       || x.tenantProfile.companyLogoFileName.Contains(anyStringContains)
			       || x.tenantProfile.companyLogoMimeType.Contains(anyStringContains)
			       || x.tenantProfile.addressLine1.Contains(anyStringContains)
			       || x.tenantProfile.addressLine2.Contains(anyStringContains)
			       || x.tenantProfile.addressLine3.Contains(anyStringContains)
			       || x.tenantProfile.city.Contains(anyStringContains)
			       || x.tenantProfile.postalCode.Contains(anyStringContains)
			       || x.tenantProfile.phoneNumber.Contains(anyStringContains)
			       || x.tenantProfile.email.Contains(anyStringContains)
			       || x.tenantProfile.website.Contains(anyStringContains)
			       || x.tenantProfile.primaryColor.Contains(anyStringContains)
			       || x.tenantProfile.secondaryColor.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single TenantProfileChangeHistory by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TenantProfileChangeHistory/{id}")]
		public async Task<IActionResult> GetTenantProfileChangeHistory(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.TenantProfileChangeHistory> query = (from tpch in _context.TenantProfileChangeHistories where
				(tpch.id == id)
					select tpch);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.tenantProfile);
					query = query.AsSplitQuery();
				}

				Database.TenantProfileChangeHistory materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.TenantProfileChangeHistory Entity was read with Admin privilege." : "Scheduler.TenantProfileChangeHistory Entity was read.");

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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.TenantProfileChangeHistory entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.TenantProfileChangeHistory.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.TenantProfileChangeHistory.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing TenantProfileChangeHistory record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/TenantProfileChangeHistory/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutTenantProfileChangeHistory(int id, [FromBody]Database.TenantProfileChangeHistory.TenantProfileChangeHistoryDTO tenantProfileChangeHistoryDTO, CancellationToken cancellationToken = default)
		{
			if (tenantProfileChangeHistoryDTO == null)
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



			if (id != tenantProfileChangeHistoryDTO.id)
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


			IQueryable<Database.TenantProfileChangeHistory> query = (from x in _context.TenantProfileChangeHistories
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.TenantProfileChangeHistory existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.TenantProfileChangeHistory PUT", id.ToString(), new Exception("No Scheduler.TenantProfileChangeHistory entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.TenantProfileChangeHistory cloneOfExisting = (Database.TenantProfileChangeHistory)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new TenantProfileChangeHistory object using the data from the existing record, updated with what is in the DTO.
			//
			Database.TenantProfileChangeHistory tenantProfileChangeHistory = (Database.TenantProfileChangeHistory)_context.Entry(existing).GetDatabaseValues().ToObject();
			tenantProfileChangeHistory.ApplyDTO(tenantProfileChangeHistoryDTO);
			//
			// The tenant guid for any TenantProfileChangeHistory being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the TenantProfileChangeHistory because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				tenantProfileChangeHistory.tenantGuid = existing.tenantGuid;
			}



			if (tenantProfileChangeHistory.timeStamp.Kind != DateTimeKind.Utc)
			{
				tenantProfileChangeHistory.timeStamp = tenantProfileChangeHistory.timeStamp.ToUniversalTime();
			}

			EntityEntry<Database.TenantProfileChangeHistory> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(tenantProfileChangeHistory);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.TenantProfileChangeHistory entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.TenantProfileChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TenantProfileChangeHistory.CreateAnonymousWithFirstLevelSubObjects(tenantProfileChangeHistory)),
					null);


				return Ok(Database.TenantProfileChangeHistory.CreateAnonymous(tenantProfileChangeHistory));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.TenantProfileChangeHistory entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.TenantProfileChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TenantProfileChangeHistory.CreateAnonymousWithFirstLevelSubObjects(tenantProfileChangeHistory)),
					ex);

				return Problem(ex.Message);
			}

		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new TenantProfileChangeHistory record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TenantProfileChangeHistory", Name = "TenantProfileChangeHistory")]
		public async Task<IActionResult> PostTenantProfileChangeHistory([FromBody]Database.TenantProfileChangeHistory.TenantProfileChangeHistoryDTO tenantProfileChangeHistoryDTO, CancellationToken cancellationToken = default)
		{
			if (tenantProfileChangeHistoryDTO == null)
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
			// Create a new TenantProfileChangeHistory object using the data from the DTO
			//
			Database.TenantProfileChangeHistory tenantProfileChangeHistory = Database.TenantProfileChangeHistory.FromDTO(tenantProfileChangeHistoryDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				tenantProfileChangeHistory.tenantGuid = userTenantGuid;

				if (tenantProfileChangeHistory.timeStamp.Kind != DateTimeKind.Utc)
				{
					tenantProfileChangeHistory.timeStamp = tenantProfileChangeHistory.timeStamp.ToUniversalTime();
				}

				_context.TenantProfileChangeHistories.Add(tenantProfileChangeHistory);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.TenantProfileChangeHistory entity successfully created.",
					true,
					tenantProfileChangeHistory.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.TenantProfileChangeHistory.CreateAnonymousWithFirstLevelSubObjects(tenantProfileChangeHistory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.TenantProfileChangeHistory entity creation failed.", false, tenantProfileChangeHistory.id.ToString(), "", JsonSerializer.Serialize(tenantProfileChangeHistory), ex);

				return Problem(ex.Message);
			}


			return CreatedAtRoute("TenantProfileChangeHistory", new { id = tenantProfileChangeHistory.id }, Database.TenantProfileChangeHistory.CreateAnonymousWithFirstLevelSubObjects(tenantProfileChangeHistory));
		}

*/


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a TenantProfileChangeHistory record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TenantProfileChangeHistory/{id}")]
		[Route("api/TenantProfileChangeHistory")]
		public async Task<IActionResult> DeleteTenantProfileChangeHistory(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.TenantProfileChangeHistory> query = (from x in _context.TenantProfileChangeHistories
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.TenantProfileChangeHistory tenantProfileChangeHistory = await query.FirstOrDefaultAsync(cancellationToken);

			if (tenantProfileChangeHistory == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.TenantProfileChangeHistory DELETE", id.ToString(), new Exception("No Scheduler.TenantProfileChangeHistory entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.TenantProfileChangeHistory cloneOfExisting = (Database.TenantProfileChangeHistory)_context.Entry(tenantProfileChangeHistory).GetDatabaseValues().ToObject();


			try
			{
				_context.TenantProfileChangeHistories.Remove(tenantProfileChangeHistory);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.TenantProfileChangeHistory entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.TenantProfileChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TenantProfileChangeHistory.CreateAnonymousWithFirstLevelSubObjects(tenantProfileChangeHistory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.TenantProfileChangeHistory entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.TenantProfileChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TenantProfileChangeHistory.CreateAnonymousWithFirstLevelSubObjects(tenantProfileChangeHistory)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


*/
        /// <summary>
        /// 
        /// This gets a list of TenantProfileChangeHistory records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/TenantProfileChangeHistories/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? tenantProfileId = null,
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

			IQueryable<Database.TenantProfileChangeHistory> query = (from tpch in _context.TenantProfileChangeHistories select tpch);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (tenantProfileId.HasValue == true)
			{
				query = query.Where(tpch => tpch.tenantProfileId == tenantProfileId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(tpch => tpch.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(tpch => tpch.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(tpch => tpch.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(data) == false)
			{
				query = query.Where(tpch => tpch.data == data);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Tenant Profile Change History, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.data.Contains(anyStringContains)
			       || x.tenantProfile.name.Contains(anyStringContains)
			       || x.tenantProfile.description.Contains(anyStringContains)
			       || x.tenantProfile.companyLogoFileName.Contains(anyStringContains)
			       || x.tenantProfile.companyLogoMimeType.Contains(anyStringContains)
			       || x.tenantProfile.addressLine1.Contains(anyStringContains)
			       || x.tenantProfile.addressLine2.Contains(anyStringContains)
			       || x.tenantProfile.addressLine3.Contains(anyStringContains)
			       || x.tenantProfile.city.Contains(anyStringContains)
			       || x.tenantProfile.postalCode.Contains(anyStringContains)
			       || x.tenantProfile.phoneNumber.Contains(anyStringContains)
			       || x.tenantProfile.email.Contains(anyStringContains)
			       || x.tenantProfile.website.Contains(anyStringContains)
			       || x.tenantProfile.primaryColor.Contains(anyStringContains)
			       || x.tenantProfile.secondaryColor.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderByDescending (x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.TenantProfileChangeHistory.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/TenantProfileChangeHistory/CreateAuditEvent")]
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
