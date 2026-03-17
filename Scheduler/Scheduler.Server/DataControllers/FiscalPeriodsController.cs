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
    /// This auto generated class provides the basic CRUD operations for the FiscalPeriod entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the FiscalPeriod entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class FiscalPeriodsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object fiscalPeriodPutSyncRoot = new object();
		static object fiscalPeriodDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<FiscalPeriodsController> _logger;

		public FiscalPeriodsController(SchedulerContext context, ILogger<FiscalPeriodsController> logger) : base("Scheduler", "FiscalPeriod")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of FiscalPeriods filtered by the parameters provided.
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
		[Route("api/FiscalPeriods")]
		public async Task<IActionResult> GetFiscalPeriods(
			string name = null,
			string description = null,
			DateTime? startDate = null,
			DateTime? endDate = null,
			string periodType = null,
			int? fiscalYear = null,
			int? periodNumber = null,
			int? periodStatusId = null,
			DateTime? closedDate = null,
			string closedBy = null,
			int? sequence = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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
			if (startDate.HasValue == true && startDate.Value.Kind != DateTimeKind.Utc)
			{
				startDate = startDate.Value.ToUniversalTime();
			}

			if (endDate.HasValue == true && endDate.Value.Kind != DateTimeKind.Utc)
			{
				endDate = endDate.Value.ToUniversalTime();
			}

			if (closedDate.HasValue == true && closedDate.Value.Kind != DateTimeKind.Utc)
			{
				closedDate = closedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.FiscalPeriod> query = (from fp in _context.FiscalPeriods select fp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(fp => fp.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(fp => fp.description == description);
			}
			if (startDate.HasValue == true)
			{
				query = query.Where(fp => fp.startDate == startDate.Value);
			}
			if (endDate.HasValue == true)
			{
				query = query.Where(fp => fp.endDate == endDate.Value);
			}
			if (string.IsNullOrEmpty(periodType) == false)
			{
				query = query.Where(fp => fp.periodType == periodType);
			}
			if (fiscalYear.HasValue == true)
			{
				query = query.Where(fp => fp.fiscalYear == fiscalYear.Value);
			}
			if (periodNumber.HasValue == true)
			{
				query = query.Where(fp => fp.periodNumber == periodNumber.Value);
			}
			if (periodStatusId.HasValue == true)
			{
				query = query.Where(fp => fp.periodStatusId == periodStatusId.Value);
			}
			if (closedDate.HasValue == true)
			{
				query = query.Where(fp => fp.closedDate == closedDate.Value);
			}
			if (string.IsNullOrEmpty(closedBy) == false)
			{
				query = query.Where(fp => fp.closedBy == closedBy);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(fp => fp.sequence == sequence.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(fp => fp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(fp => fp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(fp => fp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(fp => fp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(fp => fp.deleted == false);
				}
			}
			else
			{
				query = query.Where(fp => fp.active == true);
				query = query.Where(fp => fp.deleted == false);
			}

			query = query.OrderBy(fp => fp.sequence).ThenBy(fp => fp.name).ThenBy(fp => fp.description).ThenBy(fp => fp.periodType);


			//
			// Add the any string contains parameter to span all the string fields on the Fiscal Period, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.periodType.Contains(anyStringContains)
			       || x.closedBy.Contains(anyStringContains)
			       || (includeRelations == true && x.periodStatus.name.Contains(anyStringContains))
			       || (includeRelations == true && x.periodStatus.description.Contains(anyStringContains))
			       || (includeRelations == true && x.periodStatus.color.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.periodStatus);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.FiscalPeriod> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.FiscalPeriod fiscalPeriod in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(fiscalPeriod, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.FiscalPeriod Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.FiscalPeriod Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of FiscalPeriods filtered by the parameters provided.  Its query is similar to the GetFiscalPeriods method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FiscalPeriods/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			DateTime? startDate = null,
			DateTime? endDate = null,
			string periodType = null,
			int? fiscalYear = null,
			int? periodNumber = null,
			int? periodStatusId = null,
			DateTime? closedDate = null,
			string closedBy = null,
			int? sequence = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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
			if (startDate.HasValue == true && startDate.Value.Kind != DateTimeKind.Utc)
			{
				startDate = startDate.Value.ToUniversalTime();
			}

			if (endDate.HasValue == true && endDate.Value.Kind != DateTimeKind.Utc)
			{
				endDate = endDate.Value.ToUniversalTime();
			}

			if (closedDate.HasValue == true && closedDate.Value.Kind != DateTimeKind.Utc)
			{
				closedDate = closedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.FiscalPeriod> query = (from fp in _context.FiscalPeriods select fp);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(fp => fp.name == name);
			}
			if (description != null)
			{
				query = query.Where(fp => fp.description == description);
			}
			if (startDate.HasValue == true)
			{
				query = query.Where(fp => fp.startDate == startDate.Value);
			}
			if (endDate.HasValue == true)
			{
				query = query.Where(fp => fp.endDate == endDate.Value);
			}
			if (periodType != null)
			{
				query = query.Where(fp => fp.periodType == periodType);
			}
			if (fiscalYear.HasValue == true)
			{
				query = query.Where(fp => fp.fiscalYear == fiscalYear.Value);
			}
			if (periodNumber.HasValue == true)
			{
				query = query.Where(fp => fp.periodNumber == periodNumber.Value);
			}
			if (periodStatusId.HasValue == true)
			{
				query = query.Where(fp => fp.periodStatusId == periodStatusId.Value);
			}
			if (closedDate.HasValue == true)
			{
				query = query.Where(fp => fp.closedDate == closedDate.Value);
			}
			if (closedBy != null)
			{
				query = query.Where(fp => fp.closedBy == closedBy);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(fp => fp.sequence == sequence.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(fp => fp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(fp => fp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(fp => fp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(fp => fp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(fp => fp.deleted == false);
				}
			}
			else
			{
				query = query.Where(fp => fp.active == true);
				query = query.Where(fp => fp.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Fiscal Period, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.periodType.Contains(anyStringContains)
			       || x.closedBy.Contains(anyStringContains)
			       || x.periodStatus.name.Contains(anyStringContains)
			       || x.periodStatus.description.Contains(anyStringContains)
			       || x.periodStatus.color.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single FiscalPeriod by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FiscalPeriod/{id}")]
		public async Task<IActionResult> GetFiscalPeriod(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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
				IQueryable<Database.FiscalPeriod> query = (from fp in _context.FiscalPeriods where
							(fp.id == id) &&
							(userIsAdmin == true || fp.deleted == false) &&
							(userIsWriter == true || fp.active == true)
					select fp);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.periodStatus);
					query = query.AsSplitQuery();
				}

				Database.FiscalPeriod materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.FiscalPeriod Entity was read with Admin privilege." : "Scheduler.FiscalPeriod Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "FiscalPeriod", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.FiscalPeriod entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.FiscalPeriod.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.FiscalPeriod.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing FiscalPeriod record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/FiscalPeriod/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutFiscalPeriod(int id, [FromBody]Database.FiscalPeriod.FiscalPeriodDTO fiscalPeriodDTO, CancellationToken cancellationToken = default)
		{
			if (fiscalPeriodDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Config Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != fiscalPeriodDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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


			IQueryable<Database.FiscalPeriod> query = (from x in _context.FiscalPeriods
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.FiscalPeriod existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.FiscalPeriod PUT", id.ToString(), new Exception("No Scheduler.FiscalPeriod entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (fiscalPeriodDTO.objectGuid == Guid.Empty)
            {
                fiscalPeriodDTO.objectGuid = existing.objectGuid;
            }
            else if (fiscalPeriodDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a FiscalPeriod record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.FiscalPeriod cloneOfExisting = (Database.FiscalPeriod)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new FiscalPeriod object using the data from the existing record, updated with what is in the DTO.
			//
			Database.FiscalPeriod fiscalPeriod = (Database.FiscalPeriod)_context.Entry(existing).GetDatabaseValues().ToObject();
			fiscalPeriod.ApplyDTO(fiscalPeriodDTO);
			//
			// The tenant guid for any FiscalPeriod being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the FiscalPeriod because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				fiscalPeriod.tenantGuid = existing.tenantGuid;
			}

			lock (fiscalPeriodPutSyncRoot)
			{
				//
				// Validate the version number for the fiscalPeriod being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != fiscalPeriod.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "FiscalPeriod save attempt was made but save request was with version " + fiscalPeriod.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The FiscalPeriod you are trying to update has already changed.  Please try your save again after reloading the FiscalPeriod.");
				}
				else
				{
					// Same record.  Increase version.
					fiscalPeriod.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (fiscalPeriod.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.FiscalPeriod record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (fiscalPeriod.name != null && fiscalPeriod.name.Length > 100)
				{
					fiscalPeriod.name = fiscalPeriod.name.Substring(0, 100);
				}

				if (fiscalPeriod.description != null && fiscalPeriod.description.Length > 500)
				{
					fiscalPeriod.description = fiscalPeriod.description.Substring(0, 500);
				}

				if (fiscalPeriod.startDate.Kind != DateTimeKind.Utc)
				{
					fiscalPeriod.startDate = fiscalPeriod.startDate.ToUniversalTime();
				}

				if (fiscalPeriod.endDate.Kind != DateTimeKind.Utc)
				{
					fiscalPeriod.endDate = fiscalPeriod.endDate.ToUniversalTime();
				}

				if (fiscalPeriod.periodType != null && fiscalPeriod.periodType.Length > 50)
				{
					fiscalPeriod.periodType = fiscalPeriod.periodType.Substring(0, 50);
				}

				if (fiscalPeriod.closedDate.HasValue == true && fiscalPeriod.closedDate.Value.Kind != DateTimeKind.Utc)
				{
					fiscalPeriod.closedDate = fiscalPeriod.closedDate.Value.ToUniversalTime();
				}

				if (fiscalPeriod.closedBy != null && fiscalPeriod.closedBy.Length > 100)
				{
					fiscalPeriod.closedBy = fiscalPeriod.closedBy.Substring(0, 100);
				}

				try
				{
				    EntityEntry<Database.FiscalPeriod> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(fiscalPeriod);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        FiscalPeriodChangeHistory fiscalPeriodChangeHistory = new FiscalPeriodChangeHistory();
				        fiscalPeriodChangeHistory.fiscalPeriodId = fiscalPeriod.id;
				        fiscalPeriodChangeHistory.versionNumber = fiscalPeriod.versionNumber;
				        fiscalPeriodChangeHistory.timeStamp = DateTime.UtcNow;
				        fiscalPeriodChangeHistory.userId = securityUser.id;
				        fiscalPeriodChangeHistory.tenantGuid = userTenantGuid;
				        fiscalPeriodChangeHistory.data = JsonSerializer.Serialize(Database.FiscalPeriod.CreateAnonymousWithFirstLevelSubObjects(fiscalPeriod));
				        _context.FiscalPeriodChangeHistories.Add(fiscalPeriodChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.FiscalPeriod entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.FiscalPeriod.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.FiscalPeriod.CreateAnonymousWithFirstLevelSubObjects(fiscalPeriod)),
						null);

				return Ok(Database.FiscalPeriod.CreateAnonymous(fiscalPeriod));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.FiscalPeriod entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.FiscalPeriod.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.FiscalPeriod.CreateAnonymousWithFirstLevelSubObjects(fiscalPeriod)),
						ex);

					return Problem(ex.Message);
				}

			}
		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new FiscalPeriod record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FiscalPeriod", Name = "FiscalPeriod")]
		public async Task<IActionResult> PostFiscalPeriod([FromBody]Database.FiscalPeriod.FiscalPeriodDTO fiscalPeriodDTO, CancellationToken cancellationToken = default)
		{
			if (fiscalPeriodDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Config Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new FiscalPeriod object using the data from the DTO
			//
			Database.FiscalPeriod fiscalPeriod = Database.FiscalPeriod.FromDTO(fiscalPeriodDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				fiscalPeriod.tenantGuid = userTenantGuid;

				if (fiscalPeriod.name != null && fiscalPeriod.name.Length > 100)
				{
					fiscalPeriod.name = fiscalPeriod.name.Substring(0, 100);
				}

				if (fiscalPeriod.description != null && fiscalPeriod.description.Length > 500)
				{
					fiscalPeriod.description = fiscalPeriod.description.Substring(0, 500);
				}

				if (fiscalPeriod.startDate.Kind != DateTimeKind.Utc)
				{
					fiscalPeriod.startDate = fiscalPeriod.startDate.ToUniversalTime();
				}

				if (fiscalPeriod.endDate.Kind != DateTimeKind.Utc)
				{
					fiscalPeriod.endDate = fiscalPeriod.endDate.ToUniversalTime();
				}

				if (fiscalPeriod.periodType != null && fiscalPeriod.periodType.Length > 50)
				{
					fiscalPeriod.periodType = fiscalPeriod.periodType.Substring(0, 50);
				}

				if (fiscalPeriod.closedDate.HasValue == true && fiscalPeriod.closedDate.Value.Kind != DateTimeKind.Utc)
				{
					fiscalPeriod.closedDate = fiscalPeriod.closedDate.Value.ToUniversalTime();
				}

				if (fiscalPeriod.closedBy != null && fiscalPeriod.closedBy.Length > 100)
				{
					fiscalPeriod.closedBy = fiscalPeriod.closedBy.Substring(0, 100);
				}

				fiscalPeriod.objectGuid = Guid.NewGuid();
				fiscalPeriod.versionNumber = 1;

				_context.FiscalPeriods.Add(fiscalPeriod);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the fiscalPeriod object so that no further changes will be written to the database
				    //
				    _context.Entry(fiscalPeriod).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					fiscalPeriod.Budgets = null;
					fiscalPeriod.FinancialTransactions = null;
					fiscalPeriod.FiscalPeriodChangeHistories = null;
					fiscalPeriod.GeneralLedgerEntries = null;
					fiscalPeriod.periodStatus = null;


				    FiscalPeriodChangeHistory fiscalPeriodChangeHistory = new FiscalPeriodChangeHistory();
				    fiscalPeriodChangeHistory.fiscalPeriodId = fiscalPeriod.id;
				    fiscalPeriodChangeHistory.versionNumber = fiscalPeriod.versionNumber;
				    fiscalPeriodChangeHistory.timeStamp = DateTime.UtcNow;
				    fiscalPeriodChangeHistory.userId = securityUser.id;
				    fiscalPeriodChangeHistory.tenantGuid = userTenantGuid;
				    fiscalPeriodChangeHistory.data = JsonSerializer.Serialize(Database.FiscalPeriod.CreateAnonymousWithFirstLevelSubObjects(fiscalPeriod));
				    _context.FiscalPeriodChangeHistories.Add(fiscalPeriodChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.FiscalPeriod entity successfully created.",
						true,
						fiscalPeriod. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.FiscalPeriod.CreateAnonymousWithFirstLevelSubObjects(fiscalPeriod)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.FiscalPeriod entity creation failed.", false, fiscalPeriod.id.ToString(), "", JsonSerializer.Serialize(Database.FiscalPeriod.CreateAnonymousWithFirstLevelSubObjects(fiscalPeriod)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "FiscalPeriod", fiscalPeriod.id, fiscalPeriod.name));

			return CreatedAtRoute("FiscalPeriod", new { id = fiscalPeriod.id }, Database.FiscalPeriod.CreateAnonymousWithFirstLevelSubObjects(fiscalPeriod));
		}

*/

/* This function is expected to be overridden in a custom file

        /// <summary>
        /// 
        /// This rolls a FiscalPeriod entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FiscalPeriod/Rollback/{id}")]
		[Route("api/FiscalPeriod/Rollback")]
		public async Task<IActionResult> RollbackToFiscalPeriodVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.FiscalPeriod> query = (from x in _context.FiscalPeriods
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this FiscalPeriod concurrently
			//
			lock (fiscalPeriodPutSyncRoot)
			{
				
				Database.FiscalPeriod fiscalPeriod = query.FirstOrDefault();
				
				if (fiscalPeriod == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.FiscalPeriod rollback", id.ToString(), new Exception("No Scheduler.FiscalPeriod entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the FiscalPeriod current state so we can log it.
				//
				Database.FiscalPeriod cloneOfExisting = (Database.FiscalPeriod)_context.Entry(fiscalPeriod).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.Budgets = null;
				cloneOfExisting.FinancialTransactions = null;
				cloneOfExisting.FiscalPeriodChangeHistories = null;
				cloneOfExisting.GeneralLedgerEntries = null;
				cloneOfExisting.periodStatus = null;

				if (versionNumber >= fiscalPeriod.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.FiscalPeriod rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.FiscalPeriod rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				FiscalPeriodChangeHistory fiscalPeriodChangeHistory = (from x in _context.FiscalPeriodChangeHistories
				                                               where
				                                               x.fiscalPeriodId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (fiscalPeriodChangeHistory != null)
				{
				    Database.FiscalPeriod oldFiscalPeriod = JsonSerializer.Deserialize<Database.FiscalPeriod>(fiscalPeriodChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    fiscalPeriod.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    fiscalPeriod.name = oldFiscalPeriod.name;
				    fiscalPeriod.description = oldFiscalPeriod.description;
				    fiscalPeriod.startDate = oldFiscalPeriod.startDate;
				    fiscalPeriod.endDate = oldFiscalPeriod.endDate;
				    fiscalPeriod.periodType = oldFiscalPeriod.periodType;
				    fiscalPeriod.fiscalYear = oldFiscalPeriod.fiscalYear;
				    fiscalPeriod.periodNumber = oldFiscalPeriod.periodNumber;
				    fiscalPeriod.periodStatusId = oldFiscalPeriod.periodStatusId;
				    fiscalPeriod.closedDate = oldFiscalPeriod.closedDate;
				    fiscalPeriod.closedBy = oldFiscalPeriod.closedBy;
				    fiscalPeriod.sequence = oldFiscalPeriod.sequence;
				    fiscalPeriod.objectGuid = oldFiscalPeriod.objectGuid;
				    fiscalPeriod.active = oldFiscalPeriod.active;
				    fiscalPeriod.deleted = oldFiscalPeriod.deleted;

				    string serializedFiscalPeriod = JsonSerializer.Serialize(fiscalPeriod);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        FiscalPeriodChangeHistory newFiscalPeriodChangeHistory = new FiscalPeriodChangeHistory();
				        newFiscalPeriodChangeHistory.fiscalPeriodId = fiscalPeriod.id;
				        newFiscalPeriodChangeHistory.versionNumber = fiscalPeriod.versionNumber;
				        newFiscalPeriodChangeHistory.timeStamp = DateTime.UtcNow;
				        newFiscalPeriodChangeHistory.userId = securityUser.id;
				        newFiscalPeriodChangeHistory.tenantGuid = userTenantGuid;
				        newFiscalPeriodChangeHistory.data = JsonSerializer.Serialize(Database.FiscalPeriod.CreateAnonymousWithFirstLevelSubObjects(fiscalPeriod));
				        _context.FiscalPeriodChangeHistories.Add(newFiscalPeriodChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.FiscalPeriod rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.FiscalPeriod.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.FiscalPeriod.CreateAnonymousWithFirstLevelSubObjects(fiscalPeriod)),
						null);


				    return Ok(Database.FiscalPeriod.CreateAnonymous(fiscalPeriod));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.FiscalPeriod rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.FiscalPeriod rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}

*/



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a FiscalPeriod.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the FiscalPeriod</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FiscalPeriod/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetFiscalPeriodChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.FiscalPeriod fiscalPeriod = await _context.FiscalPeriods.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (fiscalPeriod == null)
			{
				return NotFound();
			}

			try
			{
				fiscalPeriod.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.FiscalPeriod> versionInfo = await fiscalPeriod.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a FiscalPeriod.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the FiscalPeriod</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FiscalPeriod/{id}/AuditHistory")]
		public async Task<IActionResult> GetFiscalPeriodAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.FiscalPeriod fiscalPeriod = await _context.FiscalPeriods.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (fiscalPeriod == null)
			{
				return NotFound();
			}

			try
			{
				fiscalPeriod.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.FiscalPeriod>> versions = await fiscalPeriod.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a FiscalPeriod.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the FiscalPeriod</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The FiscalPeriod object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FiscalPeriod/{id}/Version/{version}")]
		public async Task<IActionResult> GetFiscalPeriodVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.FiscalPeriod fiscalPeriod = await _context.FiscalPeriods.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (fiscalPeriod == null)
			{
				return NotFound();
			}

			try
			{
				fiscalPeriod.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.FiscalPeriod> versionInfo = await fiscalPeriod.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a FiscalPeriod at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the FiscalPeriod</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The FiscalPeriod object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FiscalPeriod/{id}/StateAtTime")]
		public async Task<IActionResult> GetFiscalPeriodStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.FiscalPeriod fiscalPeriod = await _context.FiscalPeriods.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (fiscalPeriod == null)
			{
				return NotFound();
			}

			try
			{
				fiscalPeriod.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.FiscalPeriod> versionInfo = await fiscalPeriod.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a FiscalPeriod record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FiscalPeriod/{id}")]
		[Route("api/FiscalPeriod")]
		public async Task<IActionResult> DeleteFiscalPeriod(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Config Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.FiscalPeriod> query = (from x in _context.FiscalPeriods
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.FiscalPeriod fiscalPeriod = await query.FirstOrDefaultAsync(cancellationToken);

			if (fiscalPeriod == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.FiscalPeriod DELETE", id.ToString(), new Exception("No Scheduler.FiscalPeriod entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.FiscalPeriod cloneOfExisting = (Database.FiscalPeriod)_context.Entry(fiscalPeriod).GetDatabaseValues().ToObject();


			lock (fiscalPeriodDeleteSyncRoot)
			{
			    try
			    {
			        fiscalPeriod.deleted = true;
			        fiscalPeriod.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        FiscalPeriodChangeHistory fiscalPeriodChangeHistory = new FiscalPeriodChangeHistory();
			        fiscalPeriodChangeHistory.fiscalPeriodId = fiscalPeriod.id;
			        fiscalPeriodChangeHistory.versionNumber = fiscalPeriod.versionNumber;
			        fiscalPeriodChangeHistory.timeStamp = DateTime.UtcNow;
			        fiscalPeriodChangeHistory.userId = securityUser.id;
			        fiscalPeriodChangeHistory.tenantGuid = userTenantGuid;
			        fiscalPeriodChangeHistory.data = JsonSerializer.Serialize(Database.FiscalPeriod.CreateAnonymousWithFirstLevelSubObjects(fiscalPeriod));
			        _context.FiscalPeriodChangeHistories.Add(fiscalPeriodChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.FiscalPeriod entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.FiscalPeriod.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.FiscalPeriod.CreateAnonymousWithFirstLevelSubObjects(fiscalPeriod)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.FiscalPeriod entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.FiscalPeriod.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.FiscalPeriod.CreateAnonymousWithFirstLevelSubObjects(fiscalPeriod)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


*/
        /// <summary>
        /// 
        /// This gets a list of FiscalPeriod records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/FiscalPeriods/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			DateTime? startDate = null,
			DateTime? endDate = null,
			string periodType = null,
			int? fiscalYear = null,
			int? periodNumber = null,
			int? periodStatusId = null,
			DateTime? closedDate = null,
			string closedBy = null,
			int? sequence = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);


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
			if (startDate.HasValue == true && startDate.Value.Kind != DateTimeKind.Utc)
			{
				startDate = startDate.Value.ToUniversalTime();
			}

			if (endDate.HasValue == true && endDate.Value.Kind != DateTimeKind.Utc)
			{
				endDate = endDate.Value.ToUniversalTime();
			}

			if (closedDate.HasValue == true && closedDate.Value.Kind != DateTimeKind.Utc)
			{
				closedDate = closedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.FiscalPeriod> query = (from fp in _context.FiscalPeriods select fp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(fp => fp.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(fp => fp.description == description);
			}
			if (startDate.HasValue == true)
			{
				query = query.Where(fp => fp.startDate == startDate.Value);
			}
			if (endDate.HasValue == true)
			{
				query = query.Where(fp => fp.endDate == endDate.Value);
			}
			if (string.IsNullOrEmpty(periodType) == false)
			{
				query = query.Where(fp => fp.periodType == periodType);
			}
			if (fiscalYear.HasValue == true)
			{
				query = query.Where(fp => fp.fiscalYear == fiscalYear.Value);
			}
			if (periodNumber.HasValue == true)
			{
				query = query.Where(fp => fp.periodNumber == periodNumber.Value);
			}
			if (periodStatusId.HasValue == true)
			{
				query = query.Where(fp => fp.periodStatusId == periodStatusId.Value);
			}
			if (closedDate.HasValue == true)
			{
				query = query.Where(fp => fp.closedDate == closedDate.Value);
			}
			if (string.IsNullOrEmpty(closedBy) == false)
			{
				query = query.Where(fp => fp.closedBy == closedBy);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(fp => fp.sequence == sequence.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(fp => fp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(fp => fp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(fp => fp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(fp => fp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(fp => fp.deleted == false);
				}
			}
			else
			{
				query = query.Where(fp => fp.active == true);
				query = query.Where(fp => fp.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Fiscal Period, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.periodType.Contains(anyStringContains)
			       || x.closedBy.Contains(anyStringContains)
			       || x.periodStatus.name.Contains(anyStringContains)
			       || x.periodStatus.description.Contains(anyStringContains)
			       || x.periodStatus.color.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.periodType);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.FiscalPeriod.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/FiscalPeriod/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Config Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
