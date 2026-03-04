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
    /// This auto generated class provides the basic CRUD operations for the ShiftPattern entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ShiftPattern entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ShiftPatternsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object shiftPatternPutSyncRoot = new object();
		static object shiftPatternDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<ShiftPatternsController> _logger;

		public ShiftPatternsController(SchedulerContext context, ILogger<ShiftPatternsController> logger) : base("Scheduler", "ShiftPattern")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ShiftPatterns filtered by the parameters provided.
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
		[Route("api/ShiftPatterns")]
		public async Task<IActionResult> GetShiftPatterns(
			string name = null,
			string description = null,
			int? timeZoneId = null,
			string color = null,
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

			IQueryable<Database.ShiftPattern> query = (from sp in _context.ShiftPatterns select sp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(sp => sp.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(sp => sp.description == description);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(sp => sp.timeZoneId == timeZoneId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(sp => sp.color == color);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(sp => sp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sp => sp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sp => sp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sp => sp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sp => sp.deleted == false);
				}
			}
			else
			{
				query = query.Where(sp => sp.active == true);
				query = query.Where(sp => sp.deleted == false);
			}

			query = query.OrderBy(sp => sp.name).ThenBy(sp => sp.description).ThenBy(sp => sp.color);


			//
			// Add the any string contains parameter to span all the string fields on the Shift Pattern, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || (includeRelations == true && x.timeZone.name.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.description.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.ianaTimeZone.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.abbreviation.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.abbreviationDaylightSavings.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.timeZone);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.ShiftPattern> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ShiftPattern shiftPattern in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(shiftPattern, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ShiftPattern Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ShiftPattern Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ShiftPatterns filtered by the parameters provided.  Its query is similar to the GetShiftPatterns method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ShiftPatterns/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			int? timeZoneId = null,
			string color = null,
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


			IQueryable<Database.ShiftPattern> query = (from sp in _context.ShiftPatterns select sp);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(sp => sp.name == name);
			}
			if (description != null)
			{
				query = query.Where(sp => sp.description == description);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(sp => sp.timeZoneId == timeZoneId.Value);
			}
			if (color != null)
			{
				query = query.Where(sp => sp.color == color);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(sp => sp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sp => sp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sp => sp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sp => sp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sp => sp.deleted == false);
				}
			}
			else
			{
				query = query.Where(sp => sp.active == true);
				query = query.Where(sp => sp.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Shift Pattern, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.timeZone.name.Contains(anyStringContains)
			       || x.timeZone.description.Contains(anyStringContains)
			       || x.timeZone.ianaTimeZone.Contains(anyStringContains)
			       || x.timeZone.abbreviation.Contains(anyStringContains)
			       || x.timeZone.abbreviationDaylightSavings.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ShiftPattern by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ShiftPattern/{id}")]
		public async Task<IActionResult> GetShiftPattern(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ShiftPattern> query = (from sp in _context.ShiftPatterns where
							(sp.id == id) &&
							(userIsAdmin == true || sp.deleted == false) &&
							(userIsWriter == true || sp.active == true)
					select sp);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.timeZone);
					query = query.AsSplitQuery();
				}

				Database.ShiftPattern materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.ShiftPattern Entity was read with Admin privilege." : "Scheduler.ShiftPattern Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ShiftPattern", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.ShiftPattern entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.ShiftPattern.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.ShiftPattern.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ShiftPattern record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ShiftPattern/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutShiftPattern(int id, [FromBody]Database.ShiftPattern.ShiftPatternDTO shiftPatternDTO, CancellationToken cancellationToken = default)
		{
			if (shiftPatternDTO == null)
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



			if (id != shiftPatternDTO.id)
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


			IQueryable<Database.ShiftPattern> query = (from x in _context.ShiftPatterns
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ShiftPattern existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ShiftPattern PUT", id.ToString(), new Exception("No Scheduler.ShiftPattern entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (shiftPatternDTO.objectGuid == Guid.Empty)
            {
                shiftPatternDTO.objectGuid = existing.objectGuid;
            }
            else if (shiftPatternDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ShiftPattern record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ShiftPattern cloneOfExisting = (Database.ShiftPattern)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ShiftPattern object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ShiftPattern shiftPattern = (Database.ShiftPattern)_context.Entry(existing).GetDatabaseValues().ToObject();
			shiftPattern.ApplyDTO(shiftPatternDTO);
			//
			// The tenant guid for any ShiftPattern being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ShiftPattern because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				shiftPattern.tenantGuid = existing.tenantGuid;
			}

			lock (shiftPatternPutSyncRoot)
			{
				//
				// Validate the version number for the shiftPattern being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != shiftPattern.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ShiftPattern save attempt was made but save request was with version " + shiftPattern.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ShiftPattern you are trying to update has already changed.  Please try your save again after reloading the ShiftPattern.");
				}
				else
				{
					// Same record.  Increase version.
					shiftPattern.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (shiftPattern.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.ShiftPattern record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (shiftPattern.name != null && shiftPattern.name.Length > 100)
				{
					shiftPattern.name = shiftPattern.name.Substring(0, 100);
				}

				if (shiftPattern.description != null && shiftPattern.description.Length > 500)
				{
					shiftPattern.description = shiftPattern.description.Substring(0, 500);
				}

				if (shiftPattern.color != null && shiftPattern.color.Length > 10)
				{
					shiftPattern.color = shiftPattern.color.Substring(0, 10);
				}

				try
				{
				    EntityEntry<Database.ShiftPattern> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(shiftPattern);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ShiftPatternChangeHistory shiftPatternChangeHistory = new ShiftPatternChangeHistory();
				        shiftPatternChangeHistory.shiftPatternId = shiftPattern.id;
				        shiftPatternChangeHistory.versionNumber = shiftPattern.versionNumber;
				        shiftPatternChangeHistory.timeStamp = DateTime.UtcNow;
				        shiftPatternChangeHistory.userId = securityUser.id;
				        shiftPatternChangeHistory.tenantGuid = userTenantGuid;
				        shiftPatternChangeHistory.data = JsonSerializer.Serialize(Database.ShiftPattern.CreateAnonymousWithFirstLevelSubObjects(shiftPattern));
				        _context.ShiftPatternChangeHistories.Add(shiftPatternChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ShiftPattern entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ShiftPattern.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ShiftPattern.CreateAnonymousWithFirstLevelSubObjects(shiftPattern)),
						null);

				return Ok(Database.ShiftPattern.CreateAnonymous(shiftPattern));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ShiftPattern entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ShiftPattern.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ShiftPattern.CreateAnonymousWithFirstLevelSubObjects(shiftPattern)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ShiftPattern record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ShiftPattern", Name = "ShiftPattern")]
		public async Task<IActionResult> PostShiftPattern([FromBody]Database.ShiftPattern.ShiftPatternDTO shiftPatternDTO, CancellationToken cancellationToken = default)
		{
			if (shiftPatternDTO == null)
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
			// Create a new ShiftPattern object using the data from the DTO
			//
			Database.ShiftPattern shiftPattern = Database.ShiftPattern.FromDTO(shiftPatternDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				shiftPattern.tenantGuid = userTenantGuid;

				if (shiftPattern.name != null && shiftPattern.name.Length > 100)
				{
					shiftPattern.name = shiftPattern.name.Substring(0, 100);
				}

				if (shiftPattern.description != null && shiftPattern.description.Length > 500)
				{
					shiftPattern.description = shiftPattern.description.Substring(0, 500);
				}

				if (shiftPattern.color != null && shiftPattern.color.Length > 10)
				{
					shiftPattern.color = shiftPattern.color.Substring(0, 10);
				}

				shiftPattern.objectGuid = Guid.NewGuid();
				shiftPattern.versionNumber = 1;

				_context.ShiftPatterns.Add(shiftPattern);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the shiftPattern object so that no further changes will be written to the database
				    //
				    _context.Entry(shiftPattern).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					shiftPattern.Resources = null;
					shiftPattern.ShiftPatternChangeHistories = null;
					shiftPattern.ShiftPatternDays = null;
					shiftPattern.timeZone = null;


				    ShiftPatternChangeHistory shiftPatternChangeHistory = new ShiftPatternChangeHistory();
				    shiftPatternChangeHistory.shiftPatternId = shiftPattern.id;
				    shiftPatternChangeHistory.versionNumber = shiftPattern.versionNumber;
				    shiftPatternChangeHistory.timeStamp = DateTime.UtcNow;
				    shiftPatternChangeHistory.userId = securityUser.id;
				    shiftPatternChangeHistory.tenantGuid = userTenantGuid;
				    shiftPatternChangeHistory.data = JsonSerializer.Serialize(Database.ShiftPattern.CreateAnonymousWithFirstLevelSubObjects(shiftPattern));
				    _context.ShiftPatternChangeHistories.Add(shiftPatternChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.ShiftPattern entity successfully created.",
						true,
						shiftPattern. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ShiftPattern.CreateAnonymousWithFirstLevelSubObjects(shiftPattern)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.ShiftPattern entity creation failed.", false, shiftPattern.id.ToString(), "", JsonSerializer.Serialize(Database.ShiftPattern.CreateAnonymousWithFirstLevelSubObjects(shiftPattern)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ShiftPattern", shiftPattern.id, shiftPattern.name));

			return CreatedAtRoute("ShiftPattern", new { id = shiftPattern.id }, Database.ShiftPattern.CreateAnonymousWithFirstLevelSubObjects(shiftPattern));
		}



        /// <summary>
        /// 
        /// This rolls a ShiftPattern entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ShiftPattern/Rollback/{id}")]
		[Route("api/ShiftPattern/Rollback")]
		public async Task<IActionResult> RollbackToShiftPatternVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.ShiftPattern> query = (from x in _context.ShiftPatterns
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this ShiftPattern concurrently
			//
			lock (shiftPatternPutSyncRoot)
			{
				
				Database.ShiftPattern shiftPattern = query.FirstOrDefault();
				
				if (shiftPattern == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ShiftPattern rollback", id.ToString(), new Exception("No Scheduler.ShiftPattern entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ShiftPattern current state so we can log it.
				//
				Database.ShiftPattern cloneOfExisting = (Database.ShiftPattern)_context.Entry(shiftPattern).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.Resources = null;
				cloneOfExisting.ShiftPatternChangeHistories = null;
				cloneOfExisting.ShiftPatternDays = null;
				cloneOfExisting.timeZone = null;

				if (versionNumber >= shiftPattern.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.ShiftPattern rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.ShiftPattern rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ShiftPatternChangeHistory shiftPatternChangeHistory = (from x in _context.ShiftPatternChangeHistories
				                                               where
				                                               x.shiftPatternId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (shiftPatternChangeHistory != null)
				{
				    Database.ShiftPattern oldShiftPattern = JsonSerializer.Deserialize<Database.ShiftPattern>(shiftPatternChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    shiftPattern.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    shiftPattern.name = oldShiftPattern.name;
				    shiftPattern.description = oldShiftPattern.description;
				    shiftPattern.timeZoneId = oldShiftPattern.timeZoneId;
				    shiftPattern.color = oldShiftPattern.color;
				    shiftPattern.objectGuid = oldShiftPattern.objectGuid;
				    shiftPattern.active = oldShiftPattern.active;
				    shiftPattern.deleted = oldShiftPattern.deleted;

				    string serializedShiftPattern = JsonSerializer.Serialize(shiftPattern);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ShiftPatternChangeHistory newShiftPatternChangeHistory = new ShiftPatternChangeHistory();
				        newShiftPatternChangeHistory.shiftPatternId = shiftPattern.id;
				        newShiftPatternChangeHistory.versionNumber = shiftPattern.versionNumber;
				        newShiftPatternChangeHistory.timeStamp = DateTime.UtcNow;
				        newShiftPatternChangeHistory.userId = securityUser.id;
				        newShiftPatternChangeHistory.tenantGuid = userTenantGuid;
				        newShiftPatternChangeHistory.data = JsonSerializer.Serialize(Database.ShiftPattern.CreateAnonymousWithFirstLevelSubObjects(shiftPattern));
				        _context.ShiftPatternChangeHistories.Add(newShiftPatternChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ShiftPattern rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ShiftPattern.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ShiftPattern.CreateAnonymousWithFirstLevelSubObjects(shiftPattern)),
						null);


				    return Ok(Database.ShiftPattern.CreateAnonymous(shiftPattern));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.ShiftPattern rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.ShiftPattern rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a ShiftPattern.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ShiftPattern</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ShiftPattern/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetShiftPatternChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.ShiftPattern shiftPattern = await _context.ShiftPatterns.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (shiftPattern == null)
			{
				return NotFound();
			}

			try
			{
				shiftPattern.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ShiftPattern> versionInfo = await shiftPattern.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a ShiftPattern.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ShiftPattern</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ShiftPattern/{id}/AuditHistory")]
		public async Task<IActionResult> GetShiftPatternAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.ShiftPattern shiftPattern = await _context.ShiftPatterns.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (shiftPattern == null)
			{
				return NotFound();
			}

			try
			{
				shiftPattern.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.ShiftPattern>> versions = await shiftPattern.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a ShiftPattern.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ShiftPattern</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The ShiftPattern object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ShiftPattern/{id}/Version/{version}")]
		public async Task<IActionResult> GetShiftPatternVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.ShiftPattern shiftPattern = await _context.ShiftPatterns.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (shiftPattern == null)
			{
				return NotFound();
			}

			try
			{
				shiftPattern.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ShiftPattern> versionInfo = await shiftPattern.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a ShiftPattern at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ShiftPattern</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The ShiftPattern object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ShiftPattern/{id}/StateAtTime")]
		public async Task<IActionResult> GetShiftPatternStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.ShiftPattern shiftPattern = await _context.ShiftPatterns.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (shiftPattern == null)
			{
				return NotFound();
			}

			try
			{
				shiftPattern.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ShiftPattern> versionInfo = await shiftPattern.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a ShiftPattern record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ShiftPattern/{id}")]
		[Route("api/ShiftPattern")]
		public async Task<IActionResult> DeleteShiftPattern(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ShiftPattern> query = (from x in _context.ShiftPatterns
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ShiftPattern shiftPattern = await query.FirstOrDefaultAsync(cancellationToken);

			if (shiftPattern == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ShiftPattern DELETE", id.ToString(), new Exception("No Scheduler.ShiftPattern entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ShiftPattern cloneOfExisting = (Database.ShiftPattern)_context.Entry(shiftPattern).GetDatabaseValues().ToObject();


			lock (shiftPatternDeleteSyncRoot)
			{
			    try
			    {
			        shiftPattern.deleted = true;
			        shiftPattern.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ShiftPatternChangeHistory shiftPatternChangeHistory = new ShiftPatternChangeHistory();
			        shiftPatternChangeHistory.shiftPatternId = shiftPattern.id;
			        shiftPatternChangeHistory.versionNumber = shiftPattern.versionNumber;
			        shiftPatternChangeHistory.timeStamp = DateTime.UtcNow;
			        shiftPatternChangeHistory.userId = securityUser.id;
			        shiftPatternChangeHistory.tenantGuid = userTenantGuid;
			        shiftPatternChangeHistory.data = JsonSerializer.Serialize(Database.ShiftPattern.CreateAnonymousWithFirstLevelSubObjects(shiftPattern));
			        _context.ShiftPatternChangeHistories.Add(shiftPatternChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ShiftPattern entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ShiftPattern.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ShiftPattern.CreateAnonymousWithFirstLevelSubObjects(shiftPattern)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ShiftPattern entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ShiftPattern.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ShiftPattern.CreateAnonymousWithFirstLevelSubObjects(shiftPattern)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of ShiftPattern records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ShiftPatterns/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			int? timeZoneId = null,
			string color = null,
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

			IQueryable<Database.ShiftPattern> query = (from sp in _context.ShiftPatterns select sp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(sp => sp.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(sp => sp.description == description);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(sp => sp.timeZoneId == timeZoneId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(sp => sp.color == color);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(sp => sp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sp => sp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sp => sp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sp => sp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sp => sp.deleted == false);
				}
			}
			else
			{
				query = query.Where(sp => sp.active == true);
				query = query.Where(sp => sp.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Shift Pattern, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.timeZone.name.Contains(anyStringContains)
			       || x.timeZone.description.Contains(anyStringContains)
			       || x.timeZone.ianaTimeZone.Contains(anyStringContains)
			       || x.timeZone.abbreviation.Contains(anyStringContains)
			       || x.timeZone.abbreviationDaylightSavings.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.color);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ShiftPattern.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ShiftPattern/CreateAuditEvent")]
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


        /// <summary>
        /// 
        /// This makes a ShiftPattern record a favourite for the current user.
		/// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/ShiftPattern/Favourite/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPut]
		public async Task<IActionResult> SetFavourite(int id, string description = null, CancellationToken cancellationToken = default)
		{
			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(cancellationToken);

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


			IQueryable<Database.ShiftPattern> query = (from x in _context.ShiftPatterns
			                               where x.id == id
			                               select x);


			Database.ShiftPattern shiftPattern = await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

			if (shiftPattern != null)
			{
				if (string.IsNullOrEmpty(description) == true)
				{
					description = shiftPattern.name;
				}

				//
				// Add the user favourite ShiftPattern
				//
				await SecurityLogic.AddToUserFavouritesAsync(securityUser, "ShiftPattern", id, description, cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'ShiftPattern' was added for record with id of " + id + " for user " + securityUser.accountName, true);

				//
				// Return the complete list of user favourites after the addition
				//
				return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
			}
			else
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'ShiftPattern' add request was made with an invalid id value of " + id, false);
				return BadRequest();
			}
		}


        /// <summary>
        /// 
        /// This removes a ShiftPattern record from the current user's favourites.
        /// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/ShiftPattern/Favourite/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpDelete]
		public async Task<IActionResult> DeleteFavourite(int id, CancellationToken cancellationToken = default)
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


			//
			// Delete the user favourite ShiftPattern
			//
			await SecurityLogic.RemoveFromUserFavouritesAsync(securityUser, "ShiftPattern", id, cancellationToken);

			await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'ShiftPattern' was removed for record with id of " + id + " for user " + securityUser.accountName, true);

			//
			// Return the complete list of user favourites after the deletion
			//
			return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
		}


	}
}
