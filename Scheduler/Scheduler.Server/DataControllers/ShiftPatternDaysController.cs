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
    /// This auto generated class provides the basic CRUD operations for the ShiftPatternDay entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ShiftPatternDay entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ShiftPatternDaysController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		static object shiftPatternDayPutSyncRoot = new object();
		static object shiftPatternDayDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<ShiftPatternDaysController> _logger;

		public ShiftPatternDaysController(SchedulerContext context, ILogger<ShiftPatternDaysController> logger) : base("Scheduler", "ShiftPatternDay")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ShiftPatternDays filtered by the parameters provided.
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
		[Route("api/ShiftPatternDays")]
		public async Task<IActionResult> GetShiftPatternDays(
			int? shiftPatternId = null,
			int? dayOfWeek = null,
			TimeOnly? startTime = null,
			float? hours = null,
			string label = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
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

			IQueryable<Database.ShiftPatternDay> query = (from spd in _context.ShiftPatternDays select spd);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (shiftPatternId.HasValue == true)
			{
				query = query.Where(spd => spd.shiftPatternId == shiftPatternId.Value);
			}
			if (dayOfWeek.HasValue == true)
			{
				query = query.Where(spd => spd.dayOfWeek == dayOfWeek.Value);
			}
			if (hours.HasValue == true)
			{
				query = query.Where(spd => spd.hours == hours.Value);
			}
			if (string.IsNullOrEmpty(label) == false)
			{
				query = query.Where(spd => spd.label == label);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(spd => spd.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(spd => spd.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(spd => spd.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(spd => spd.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(spd => spd.deleted == false);
				}
			}
			else
			{
				query = query.Where(spd => spd.active == true);
				query = query.Where(spd => spd.deleted == false);
			}

			query = query.OrderBy(spd => spd.label);


			//
			// Add the any string contains parameter to span all the string fields on the Shift Pattern Day, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.label.Contains(anyStringContains)
			       || (includeRelations == true && x.shiftPattern.name.Contains(anyStringContains))
			       || (includeRelations == true && x.shiftPattern.description.Contains(anyStringContains))
			       || (includeRelations == true && x.shiftPattern.color.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.shiftPattern);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.ShiftPatternDay> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ShiftPatternDay shiftPatternDay in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(shiftPatternDay, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ShiftPatternDay Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ShiftPatternDay Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ShiftPatternDays filtered by the parameters provided.  Its query is similar to the GetShiftPatternDays method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ShiftPatternDays/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? shiftPatternId = null,
			int? dayOfWeek = null,
			TimeOnly? startTime = null,
			float? hours = null,
			string label = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
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


			IQueryable<Database.ShiftPatternDay> query = (from spd in _context.ShiftPatternDays select spd);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (shiftPatternId.HasValue == true)
			{
				query = query.Where(spd => spd.shiftPatternId == shiftPatternId.Value);
			}
			if (dayOfWeek.HasValue == true)
			{
				query = query.Where(spd => spd.dayOfWeek == dayOfWeek.Value);
			}
			if (hours.HasValue == true)
			{
				query = query.Where(spd => spd.hours == hours.Value);
			}
			if (label != null)
			{
				query = query.Where(spd => spd.label == label);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(spd => spd.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(spd => spd.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(spd => spd.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(spd => spd.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(spd => spd.deleted == false);
				}
			}
			else
			{
				query = query.Where(spd => spd.active == true);
				query = query.Where(spd => spd.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Shift Pattern Day, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.label.Contains(anyStringContains)
			       || x.shiftPattern.name.Contains(anyStringContains)
			       || x.shiftPattern.description.Contains(anyStringContains)
			       || x.shiftPattern.color.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ShiftPatternDay by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ShiftPatternDay/{id}")]
		public async Task<IActionResult> GetShiftPatternDay(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
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
				IQueryable<Database.ShiftPatternDay> query = (from spd in _context.ShiftPatternDays where
							(spd.id == id) &&
							(userIsAdmin == true || spd.deleted == false) &&
							(userIsWriter == true || spd.active == true)
					select spd);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.shiftPattern);
					query = query.AsSplitQuery();
				}

				Database.ShiftPatternDay materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.ShiftPatternDay Entity was read with Admin privilege." : "Scheduler.ShiftPatternDay Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ShiftPatternDay", materialized.id, materialized.label));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.ShiftPatternDay entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.ShiftPatternDay.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.ShiftPatternDay.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ShiftPatternDay record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ShiftPatternDay/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutShiftPatternDay(int id, [FromBody]Database.ShiftPatternDay.ShiftPatternDayDTO shiftPatternDayDTO, CancellationToken cancellationToken = default)
		{
			if (shiftPatternDayDTO == null)
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



			if (id != shiftPatternDayDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
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


			IQueryable<Database.ShiftPatternDay> query = (from x in _context.ShiftPatternDays
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ShiftPatternDay existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ShiftPatternDay PUT", id.ToString(), new Exception("No Scheduler.ShiftPatternDay entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (shiftPatternDayDTO.objectGuid == Guid.Empty)
            {
                shiftPatternDayDTO.objectGuid = existing.objectGuid;
            }
            else if (shiftPatternDayDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ShiftPatternDay record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ShiftPatternDay cloneOfExisting = (Database.ShiftPatternDay)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ShiftPatternDay object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ShiftPatternDay shiftPatternDay = (Database.ShiftPatternDay)_context.Entry(existing).GetDatabaseValues().ToObject();
			shiftPatternDay.ApplyDTO(shiftPatternDayDTO);
			//
			// The tenant guid for any ShiftPatternDay being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ShiftPatternDay because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				shiftPatternDay.tenantGuid = existing.tenantGuid;
			}

			lock (shiftPatternDayPutSyncRoot)
			{
				//
				// Validate the version number for the shiftPatternDay being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != shiftPatternDay.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ShiftPatternDay save attempt was made but save request was with version " + shiftPatternDay.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ShiftPatternDay you are trying to update has already changed.  Please try your save again after reloading the ShiftPatternDay.");
				}
				else
				{
					// Same record.  Increase version.
					shiftPatternDay.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (shiftPatternDay.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.ShiftPatternDay record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (shiftPatternDay.label != null && shiftPatternDay.label.Length > 250)
				{
					shiftPatternDay.label = shiftPatternDay.label.Substring(0, 250);
				}

				try
				{
				    EntityEntry<Database.ShiftPatternDay> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(shiftPatternDay);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ShiftPatternDayChangeHistory shiftPatternDayChangeHistory = new ShiftPatternDayChangeHistory();
				        shiftPatternDayChangeHistory.shiftPatternDayId = shiftPatternDay.id;
				        shiftPatternDayChangeHistory.versionNumber = shiftPatternDay.versionNumber;
				        shiftPatternDayChangeHistory.timeStamp = DateTime.UtcNow;
				        shiftPatternDayChangeHistory.userId = securityUser.id;
				        shiftPatternDayChangeHistory.tenantGuid = userTenantGuid;
				        shiftPatternDayChangeHistory.data = JsonSerializer.Serialize(Database.ShiftPatternDay.CreateAnonymousWithFirstLevelSubObjects(shiftPatternDay));
				        _context.ShiftPatternDayChangeHistories.Add(shiftPatternDayChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ShiftPatternDay entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ShiftPatternDay.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ShiftPatternDay.CreateAnonymousWithFirstLevelSubObjects(shiftPatternDay)),
						null);

				return Ok(Database.ShiftPatternDay.CreateAnonymous(shiftPatternDay));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ShiftPatternDay entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ShiftPatternDay.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ShiftPatternDay.CreateAnonymousWithFirstLevelSubObjects(shiftPatternDay)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ShiftPatternDay record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ShiftPatternDay", Name = "ShiftPatternDay")]
		public async Task<IActionResult> PostShiftPatternDay([FromBody]Database.ShiftPatternDay.ShiftPatternDayDTO shiftPatternDayDTO, CancellationToken cancellationToken = default)
		{
			if (shiftPatternDayDTO == null)
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
			// Create a new ShiftPatternDay object using the data from the DTO
			//
			Database.ShiftPatternDay shiftPatternDay = Database.ShiftPatternDay.FromDTO(shiftPatternDayDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				shiftPatternDay.tenantGuid = userTenantGuid;

				if (shiftPatternDay.label != null && shiftPatternDay.label.Length > 250)
				{
					shiftPatternDay.label = shiftPatternDay.label.Substring(0, 250);
				}

				shiftPatternDay.objectGuid = Guid.NewGuid();
				shiftPatternDay.versionNumber = 1;

				_context.ShiftPatternDays.Add(shiftPatternDay);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the shiftPatternDay object so that no further changes will be written to the database
				    //
				    _context.Entry(shiftPatternDay).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					shiftPatternDay.ShiftPatternDayChangeHistories = null;
					shiftPatternDay.shiftPattern = null;


				    ShiftPatternDayChangeHistory shiftPatternDayChangeHistory = new ShiftPatternDayChangeHistory();
				    shiftPatternDayChangeHistory.shiftPatternDayId = shiftPatternDay.id;
				    shiftPatternDayChangeHistory.versionNumber = shiftPatternDay.versionNumber;
				    shiftPatternDayChangeHistory.timeStamp = DateTime.UtcNow;
				    shiftPatternDayChangeHistory.userId = securityUser.id;
				    shiftPatternDayChangeHistory.tenantGuid = userTenantGuid;
				    shiftPatternDayChangeHistory.data = JsonSerializer.Serialize(Database.ShiftPatternDay.CreateAnonymousWithFirstLevelSubObjects(shiftPatternDay));
				    _context.ShiftPatternDayChangeHistories.Add(shiftPatternDayChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.ShiftPatternDay entity successfully created.",
						true,
						shiftPatternDay. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ShiftPatternDay.CreateAnonymousWithFirstLevelSubObjects(shiftPatternDay)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.ShiftPatternDay entity creation failed.", false, shiftPatternDay.id.ToString(), "", JsonSerializer.Serialize(Database.ShiftPatternDay.CreateAnonymousWithFirstLevelSubObjects(shiftPatternDay)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ShiftPatternDay", shiftPatternDay.id, shiftPatternDay.label));

			return CreatedAtRoute("ShiftPatternDay", new { id = shiftPatternDay.id }, Database.ShiftPatternDay.CreateAnonymousWithFirstLevelSubObjects(shiftPatternDay));
		}



        /// <summary>
        /// 
        /// This rolls a ShiftPatternDay entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ShiftPatternDay/Rollback/{id}")]
		[Route("api/ShiftPatternDay/Rollback")]
		public async Task<IActionResult> RollbackToShiftPatternDayVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.ShiftPatternDay> query = (from x in _context.ShiftPatternDays
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this ShiftPatternDay concurrently
			//
			lock (shiftPatternDayPutSyncRoot)
			{
				
				Database.ShiftPatternDay shiftPatternDay = query.FirstOrDefault();
				
				if (shiftPatternDay == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ShiftPatternDay rollback", id.ToString(), new Exception("No Scheduler.ShiftPatternDay entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ShiftPatternDay current state so we can log it.
				//
				Database.ShiftPatternDay cloneOfExisting = (Database.ShiftPatternDay)_context.Entry(shiftPatternDay).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.ShiftPatternDayChangeHistories = null;
				cloneOfExisting.shiftPattern = null;

				if (versionNumber >= shiftPatternDay.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.ShiftPatternDay rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.ShiftPatternDay rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ShiftPatternDayChangeHistory shiftPatternDayChangeHistory = (from x in _context.ShiftPatternDayChangeHistories
				                                               where
				                                               x.shiftPatternDayId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (shiftPatternDayChangeHistory != null)
				{
				    Database.ShiftPatternDay oldShiftPatternDay = JsonSerializer.Deserialize<Database.ShiftPatternDay>(shiftPatternDayChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    shiftPatternDay.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    shiftPatternDay.shiftPatternId = oldShiftPatternDay.shiftPatternId;
				    shiftPatternDay.dayOfWeek = oldShiftPatternDay.dayOfWeek;
				    shiftPatternDay.startTime = oldShiftPatternDay.startTime;
				    shiftPatternDay.hours = oldShiftPatternDay.hours;
				    shiftPatternDay.label = oldShiftPatternDay.label;
				    shiftPatternDay.objectGuid = oldShiftPatternDay.objectGuid;
				    shiftPatternDay.active = oldShiftPatternDay.active;
				    shiftPatternDay.deleted = oldShiftPatternDay.deleted;

				    string serializedShiftPatternDay = JsonSerializer.Serialize(shiftPatternDay);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ShiftPatternDayChangeHistory newShiftPatternDayChangeHistory = new ShiftPatternDayChangeHistory();
				        newShiftPatternDayChangeHistory.shiftPatternDayId = shiftPatternDay.id;
				        newShiftPatternDayChangeHistory.versionNumber = shiftPatternDay.versionNumber;
				        newShiftPatternDayChangeHistory.timeStamp = DateTime.UtcNow;
				        newShiftPatternDayChangeHistory.userId = securityUser.id;
				        newShiftPatternDayChangeHistory.tenantGuid = userTenantGuid;
				        newShiftPatternDayChangeHistory.data = JsonSerializer.Serialize(Database.ShiftPatternDay.CreateAnonymousWithFirstLevelSubObjects(shiftPatternDay));
				        _context.ShiftPatternDayChangeHistories.Add(newShiftPatternDayChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ShiftPatternDay rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ShiftPatternDay.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ShiftPatternDay.CreateAnonymousWithFirstLevelSubObjects(shiftPatternDay)),
						null);


				    return Ok(Database.ShiftPatternDay.CreateAnonymous(shiftPatternDay));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.ShiftPatternDay rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.ShiftPatternDay rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a ShiftPatternDay.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ShiftPatternDay</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ShiftPatternDay/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetShiftPatternDayChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.ShiftPatternDay shiftPatternDay = await _context.ShiftPatternDays.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (shiftPatternDay == null)
			{
				return NotFound();
			}

			try
			{
				shiftPatternDay.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ShiftPatternDay> versionInfo = await shiftPatternDay.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a ShiftPatternDay.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ShiftPatternDay</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ShiftPatternDay/{id}/AuditHistory")]
		public async Task<IActionResult> GetShiftPatternDayAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.ShiftPatternDay shiftPatternDay = await _context.ShiftPatternDays.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (shiftPatternDay == null)
			{
				return NotFound();
			}

			try
			{
				shiftPatternDay.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.ShiftPatternDay>> versions = await shiftPatternDay.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a ShiftPatternDay.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ShiftPatternDay</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The ShiftPatternDay object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ShiftPatternDay/{id}/Version/{version}")]
		public async Task<IActionResult> GetShiftPatternDayVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.ShiftPatternDay shiftPatternDay = await _context.ShiftPatternDays.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (shiftPatternDay == null)
			{
				return NotFound();
			}

			try
			{
				shiftPatternDay.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ShiftPatternDay> versionInfo = await shiftPatternDay.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a ShiftPatternDay at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ShiftPatternDay</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The ShiftPatternDay object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ShiftPatternDay/{id}/StateAtTime")]
		public async Task<IActionResult> GetShiftPatternDayStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.ShiftPatternDay shiftPatternDay = await _context.ShiftPatternDays.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (shiftPatternDay == null)
			{
				return NotFound();
			}

			try
			{
				shiftPatternDay.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ShiftPatternDay> versionInfo = await shiftPatternDay.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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

        /// <summary>
        /// 
        /// This deletes a ShiftPatternDay record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ShiftPatternDay/{id}")]
		[Route("api/ShiftPatternDay")]
		public async Task<IActionResult> DeleteShiftPatternDay(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ShiftPatternDay> query = (from x in _context.ShiftPatternDays
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ShiftPatternDay shiftPatternDay = await query.FirstOrDefaultAsync(cancellationToken);

			if (shiftPatternDay == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ShiftPatternDay DELETE", id.ToString(), new Exception("No Scheduler.ShiftPatternDay entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ShiftPatternDay cloneOfExisting = (Database.ShiftPatternDay)_context.Entry(shiftPatternDay).GetDatabaseValues().ToObject();


			lock (shiftPatternDayDeleteSyncRoot)
			{
			    try
			    {
			        shiftPatternDay.deleted = true;
			        shiftPatternDay.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ShiftPatternDayChangeHistory shiftPatternDayChangeHistory = new ShiftPatternDayChangeHistory();
			        shiftPatternDayChangeHistory.shiftPatternDayId = shiftPatternDay.id;
			        shiftPatternDayChangeHistory.versionNumber = shiftPatternDay.versionNumber;
			        shiftPatternDayChangeHistory.timeStamp = DateTime.UtcNow;
			        shiftPatternDayChangeHistory.userId = securityUser.id;
			        shiftPatternDayChangeHistory.tenantGuid = userTenantGuid;
			        shiftPatternDayChangeHistory.data = JsonSerializer.Serialize(Database.ShiftPatternDay.CreateAnonymousWithFirstLevelSubObjects(shiftPatternDay));
			        _context.ShiftPatternDayChangeHistories.Add(shiftPatternDayChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ShiftPatternDay entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ShiftPatternDay.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ShiftPatternDay.CreateAnonymousWithFirstLevelSubObjects(shiftPatternDay)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ShiftPatternDay entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ShiftPatternDay.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ShiftPatternDay.CreateAnonymousWithFirstLevelSubObjects(shiftPatternDay)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of ShiftPatternDay records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ShiftPatternDays/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? shiftPatternId = null,
			int? dayOfWeek = null,
			TimeOnly? startTime = null,
			float? hours = null,
			string label = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);


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

			IQueryable<Database.ShiftPatternDay> query = (from spd in _context.ShiftPatternDays select spd);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (shiftPatternId.HasValue == true)
			{
				query = query.Where(spd => spd.shiftPatternId == shiftPatternId.Value);
			}
			if (dayOfWeek.HasValue == true)
			{
				query = query.Where(spd => spd.dayOfWeek == dayOfWeek.Value);
			}
			if (hours.HasValue == true)
			{
				query = query.Where(spd => spd.hours == hours.Value);
			}
			if (string.IsNullOrEmpty(label) == false)
			{
				query = query.Where(spd => spd.label == label);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(spd => spd.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(spd => spd.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(spd => spd.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(spd => spd.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(spd => spd.deleted == false);
				}
			}
			else
			{
				query = query.Where(spd => spd.active == true);
				query = query.Where(spd => spd.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Shift Pattern Day, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.label.Contains(anyStringContains)
			       || x.shiftPattern.name.Contains(anyStringContains)
			       || x.shiftPattern.description.Contains(anyStringContains)
			       || x.shiftPattern.color.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.label);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ShiftPatternDay.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ShiftPatternDay/CreateAuditEvent")]
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
