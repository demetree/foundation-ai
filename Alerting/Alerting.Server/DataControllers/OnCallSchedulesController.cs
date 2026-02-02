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
using Foundation.ChangeHistory;

namespace Foundation.Alerting.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the OnCallSchedule entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the OnCallSchedule entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class OnCallSchedulesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 100;

		static object onCallSchedulePutSyncRoot = new object();
		static object onCallScheduleDeleteSyncRoot = new object();

		private AlertingContext _context;

		private ILogger<OnCallSchedulesController> _logger;

		public OnCallSchedulesController(AlertingContext context, ILogger<OnCallSchedulesController> logger) : base("Alerting", "OnCallSchedule")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of OnCallSchedules filtered by the parameters provided.
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
		[Route("api/OnCallSchedules")]
		public async Task<IActionResult> GetOnCallSchedules(
			string name = null,
			string description = null,
			string timeZoneId = null,
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
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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

			IQueryable<Database.OnCallSchedule> query = (from ocs in _context.OnCallSchedules select ocs);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ocs => ocs.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ocs => ocs.description == description);
			}
			if (string.IsNullOrEmpty(timeZoneId) == false)
			{
				query = query.Where(ocs => ocs.timeZoneId == timeZoneId);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ocs => ocs.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ocs => ocs.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ocs => ocs.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ocs => ocs.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ocs => ocs.deleted == false);
				}
			}
			else
			{
				query = query.Where(ocs => ocs.active == true);
				query = query.Where(ocs => ocs.deleted == false);
			}

			query = query.OrderBy(ocs => ocs.name).ThenBy(ocs => ocs.description).ThenBy(ocs => ocs.timeZoneId);

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
			// Add the any string contains parameter to span all the string fields on the On Call Schedule, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.timeZoneId.Contains(anyStringContains)
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.OnCallSchedule> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.OnCallSchedule onCallSchedule in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(onCallSchedule, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Alerting.OnCallSchedule Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Alerting.OnCallSchedule Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of OnCallSchedules filtered by the parameters provided.  Its query is similar to the GetOnCallSchedules method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/OnCallSchedules/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			string timeZoneId = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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


			IQueryable<Database.OnCallSchedule> query = (from ocs in _context.OnCallSchedules select ocs);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(ocs => ocs.name == name);
			}
			if (description != null)
			{
				query = query.Where(ocs => ocs.description == description);
			}
			if (timeZoneId != null)
			{
				query = query.Where(ocs => ocs.timeZoneId == timeZoneId);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ocs => ocs.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ocs => ocs.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ocs => ocs.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ocs => ocs.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ocs => ocs.deleted == false);
				}
			}
			else
			{
				query = query.Where(ocs => ocs.active == true);
				query = query.Where(ocs => ocs.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the On Call Schedule, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.timeZoneId.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single OnCallSchedule by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/OnCallSchedule/{id}")]
		public async Task<IActionResult> GetOnCallSchedule(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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
				IQueryable<Database.OnCallSchedule> query = (from ocs in _context.OnCallSchedules where
							(ocs.id == id) &&
							(userIsAdmin == true || ocs.deleted == false) &&
							(userIsWriter == true || ocs.active == true)
					select ocs);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.OnCallSchedule materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Alerting.OnCallSchedule Entity was read with Admin privilege." : "Alerting.OnCallSchedule Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "OnCallSchedule", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Alerting.OnCallSchedule entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Alerting.OnCallSchedule.   Entity was read with Admin privilege." : "Exception caught during entity read of Alerting.OnCallSchedule.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing OnCallSchedule record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/OnCallSchedule/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutOnCallSchedule(int id, [FromBody]Database.OnCallSchedule.OnCallScheduleDTO onCallScheduleDTO, CancellationToken cancellationToken = default)
		{
			if (onCallScheduleDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Alerting Schedule Writer role needed to write to this table, or Alerting Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Alerting Schedule Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != onCallScheduleDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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


			IQueryable<Database.OnCallSchedule> query = (from x in _context.OnCallSchedules
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.OnCallSchedule existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.OnCallSchedule PUT", id.ToString(), new Exception("No Alerting.OnCallSchedule entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (onCallScheduleDTO.objectGuid == Guid.Empty)
            {
                onCallScheduleDTO.objectGuid = existing.objectGuid;
            }
            else if (onCallScheduleDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a OnCallSchedule record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.OnCallSchedule cloneOfExisting = (Database.OnCallSchedule)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new OnCallSchedule object using the data from the existing record, updated with what is in the DTO.
			//
			Database.OnCallSchedule onCallSchedule = (Database.OnCallSchedule)_context.Entry(existing).GetDatabaseValues().ToObject();
			onCallSchedule.ApplyDTO(onCallScheduleDTO);
			//
			// The tenant guid for any OnCallSchedule being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the OnCallSchedule because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				onCallSchedule.tenantGuid = existing.tenantGuid;
			}

			lock (onCallSchedulePutSyncRoot)
			{
				//
				// Validate the version number for the onCallSchedule being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != onCallSchedule.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "OnCallSchedule save attempt was made but save request was with version " + onCallSchedule.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The OnCallSchedule you are trying to update has already changed.  Please try your save again after reloading the OnCallSchedule.");
				}
				else
				{
					// Same record.  Increase version.
					onCallSchedule.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (onCallSchedule.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Alerting.OnCallSchedule record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (onCallSchedule.name != null && onCallSchedule.name.Length > 100)
				{
					onCallSchedule.name = onCallSchedule.name.Substring(0, 100);
				}

				if (onCallSchedule.description != null && onCallSchedule.description.Length > 500)
				{
					onCallSchedule.description = onCallSchedule.description.Substring(0, 500);
				}

				if (onCallSchedule.timeZoneId != null && onCallSchedule.timeZoneId.Length > 50)
				{
					onCallSchedule.timeZoneId = onCallSchedule.timeZoneId.Substring(0, 50);
				}

				try
				{
				    EntityEntry<Database.OnCallSchedule> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(onCallSchedule);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        OnCallScheduleChangeHistory onCallScheduleChangeHistory = new OnCallScheduleChangeHistory();
				        onCallScheduleChangeHistory.onCallScheduleId = onCallSchedule.id;
				        onCallScheduleChangeHistory.versionNumber = onCallSchedule.versionNumber;
				        onCallScheduleChangeHistory.timeStamp = DateTime.UtcNow;
				        onCallScheduleChangeHistory.userId = securityUser.id;
				        onCallScheduleChangeHistory.tenantGuid = userTenantGuid;
				        onCallScheduleChangeHistory.data = JsonSerializer.Serialize(Database.OnCallSchedule.CreateAnonymousWithFirstLevelSubObjects(onCallSchedule));
				        _context.OnCallScheduleChangeHistories.Add(onCallScheduleChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.OnCallSchedule entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.OnCallSchedule.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.OnCallSchedule.CreateAnonymousWithFirstLevelSubObjects(onCallSchedule)),
						null);

				return Ok(Database.OnCallSchedule.CreateAnonymous(onCallSchedule));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.OnCallSchedule entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.OnCallSchedule.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.OnCallSchedule.CreateAnonymousWithFirstLevelSubObjects(onCallSchedule)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new OnCallSchedule record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/OnCallSchedule", Name = "OnCallSchedule")]
		public async Task<IActionResult> PostOnCallSchedule([FromBody]Database.OnCallSchedule.OnCallScheduleDTO onCallScheduleDTO, CancellationToken cancellationToken = default)
		{
			if (onCallScheduleDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Alerting Schedule Writer role needed to write to this table, or Alerting Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Alerting Schedule Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new OnCallSchedule object using the data from the DTO
			//
			Database.OnCallSchedule onCallSchedule = Database.OnCallSchedule.FromDTO(onCallScheduleDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				onCallSchedule.tenantGuid = userTenantGuid;

				if (onCallSchedule.name != null && onCallSchedule.name.Length > 100)
				{
					onCallSchedule.name = onCallSchedule.name.Substring(0, 100);
				}

				if (onCallSchedule.description != null && onCallSchedule.description.Length > 500)
				{
					onCallSchedule.description = onCallSchedule.description.Substring(0, 500);
				}

				if (onCallSchedule.timeZoneId != null && onCallSchedule.timeZoneId.Length > 50)
				{
					onCallSchedule.timeZoneId = onCallSchedule.timeZoneId.Substring(0, 50);
				}

				onCallSchedule.objectGuid = Guid.NewGuid();
				onCallSchedule.versionNumber = 1;

				_context.OnCallSchedules.Add(onCallSchedule);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the onCallSchedule object so that no further changes will be written to the database
				    //
				    _context.Entry(onCallSchedule).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					onCallSchedule.OnCallScheduleChangeHistories = null;
					onCallSchedule.ScheduleLayers = null;


				    OnCallScheduleChangeHistory onCallScheduleChangeHistory = new OnCallScheduleChangeHistory();
				    onCallScheduleChangeHistory.onCallScheduleId = onCallSchedule.id;
				    onCallScheduleChangeHistory.versionNumber = onCallSchedule.versionNumber;
				    onCallScheduleChangeHistory.timeStamp = DateTime.UtcNow;
				    onCallScheduleChangeHistory.userId = securityUser.id;
				    onCallScheduleChangeHistory.tenantGuid = userTenantGuid;
				    onCallScheduleChangeHistory.data = JsonSerializer.Serialize(Database.OnCallSchedule.CreateAnonymousWithFirstLevelSubObjects(onCallSchedule));
				    _context.OnCallScheduleChangeHistories.Add(onCallScheduleChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Alerting.OnCallSchedule entity successfully created.",
						true,
						onCallSchedule. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.OnCallSchedule.CreateAnonymousWithFirstLevelSubObjects(onCallSchedule)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Alerting.OnCallSchedule entity creation failed.", false, onCallSchedule.id.ToString(), "", JsonSerializer.Serialize(Database.OnCallSchedule.CreateAnonymousWithFirstLevelSubObjects(onCallSchedule)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "OnCallSchedule", onCallSchedule.id, onCallSchedule.name));

			return CreatedAtRoute("OnCallSchedule", new { id = onCallSchedule.id }, Database.OnCallSchedule.CreateAnonymousWithFirstLevelSubObjects(onCallSchedule));
		}



        /// <summary>
        /// 
        /// This rolls a OnCallSchedule entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/OnCallSchedule/Rollback/{id}")]
		[Route("api/OnCallSchedule/Rollback")]
		public async Task<IActionResult> RollbackToOnCallScheduleVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.OnCallSchedule> query = (from x in _context.OnCallSchedules
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this OnCallSchedule concurrently
			//
			lock (onCallSchedulePutSyncRoot)
			{
				
				Database.OnCallSchedule onCallSchedule = query.FirstOrDefault();
				
				if (onCallSchedule == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.OnCallSchedule rollback", id.ToString(), new Exception("No Alerting.OnCallSchedule entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the OnCallSchedule current state so we can log it.
				//
				Database.OnCallSchedule cloneOfExisting = (Database.OnCallSchedule)_context.Entry(onCallSchedule).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.OnCallScheduleChangeHistories = null;
				cloneOfExisting.ScheduleLayers = null;

				if (versionNumber >= onCallSchedule.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Alerting.OnCallSchedule rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Alerting.OnCallSchedule rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				OnCallScheduleChangeHistory onCallScheduleChangeHistory = (from x in _context.OnCallScheduleChangeHistories
				                                               where
				                                               x.onCallScheduleId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (onCallScheduleChangeHistory != null)
				{
				    Database.OnCallSchedule oldOnCallSchedule = JsonSerializer.Deserialize<Database.OnCallSchedule>(onCallScheduleChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    onCallSchedule.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    onCallSchedule.name = oldOnCallSchedule.name;
				    onCallSchedule.description = oldOnCallSchedule.description;
				    onCallSchedule.timeZoneId = oldOnCallSchedule.timeZoneId;
				    onCallSchedule.objectGuid = oldOnCallSchedule.objectGuid;
				    onCallSchedule.active = oldOnCallSchedule.active;
				    onCallSchedule.deleted = oldOnCallSchedule.deleted;

				    string serializedOnCallSchedule = JsonSerializer.Serialize(onCallSchedule);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        OnCallScheduleChangeHistory newOnCallScheduleChangeHistory = new OnCallScheduleChangeHistory();
				        newOnCallScheduleChangeHistory.onCallScheduleId = onCallSchedule.id;
				        newOnCallScheduleChangeHistory.versionNumber = onCallSchedule.versionNumber;
				        newOnCallScheduleChangeHistory.timeStamp = DateTime.UtcNow;
				        newOnCallScheduleChangeHistory.userId = securityUser.id;
				        newOnCallScheduleChangeHistory.tenantGuid = userTenantGuid;
				        newOnCallScheduleChangeHistory.data = JsonSerializer.Serialize(Database.OnCallSchedule.CreateAnonymousWithFirstLevelSubObjects(onCallSchedule));
				        _context.OnCallScheduleChangeHistories.Add(newOnCallScheduleChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.OnCallSchedule rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.OnCallSchedule.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.OnCallSchedule.CreateAnonymousWithFirstLevelSubObjects(onCallSchedule)),
						null);


				    return Ok(Database.OnCallSchedule.CreateAnonymous(onCallSchedule));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Alerting.OnCallSchedule rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Alerting.OnCallSchedule rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a OnCallSchedule.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the OnCallSchedule</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/OnCallSchedule/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetOnCallScheduleChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.OnCallSchedule onCallSchedule = await _context.OnCallSchedules.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (onCallSchedule == null)
			{
				return NotFound();
			}

			try
			{
				onCallSchedule.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.OnCallSchedule> versionInfo = await onCallSchedule.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a OnCallSchedule.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the OnCallSchedule</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/OnCallSchedule/{id}/AuditHistory")]
		public async Task<IActionResult> GetOnCallScheduleAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.OnCallSchedule onCallSchedule = await _context.OnCallSchedules.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (onCallSchedule == null)
			{
				return NotFound();
			}

			try
			{
				onCallSchedule.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.OnCallSchedule>> versions = await onCallSchedule.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a OnCallSchedule.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the OnCallSchedule</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The OnCallSchedule object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/OnCallSchedule/{id}/Version/{version}")]
		public async Task<IActionResult> GetOnCallScheduleVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.OnCallSchedule onCallSchedule = await _context.OnCallSchedules.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (onCallSchedule == null)
			{
				return NotFound();
			}

			try
			{
				onCallSchedule.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.OnCallSchedule> versionInfo = await onCallSchedule.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a OnCallSchedule at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the OnCallSchedule</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The OnCallSchedule object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/OnCallSchedule/{id}/StateAtTime")]
		public async Task<IActionResult> GetOnCallScheduleStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.OnCallSchedule onCallSchedule = await _context.OnCallSchedules.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (onCallSchedule == null)
			{
				return NotFound();
			}

			try
			{
				onCallSchedule.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.OnCallSchedule> versionInfo = await onCallSchedule.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a OnCallSchedule record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/OnCallSchedule/{id}")]
		[Route("api/OnCallSchedule")]
		public async Task<IActionResult> DeleteOnCallSchedule(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Alerting Schedule Writer role needed to write to this table, or Alerting Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Alerting Schedule Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.OnCallSchedule> query = (from x in _context.OnCallSchedules
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.OnCallSchedule onCallSchedule = await query.FirstOrDefaultAsync(cancellationToken);

			if (onCallSchedule == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.OnCallSchedule DELETE", id.ToString(), new Exception("No Alerting.OnCallSchedule entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.OnCallSchedule cloneOfExisting = (Database.OnCallSchedule)_context.Entry(onCallSchedule).GetDatabaseValues().ToObject();


			lock (onCallScheduleDeleteSyncRoot)
			{
			    try
			    {
			        onCallSchedule.deleted = true;
			        onCallSchedule.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        OnCallScheduleChangeHistory onCallScheduleChangeHistory = new OnCallScheduleChangeHistory();
			        onCallScheduleChangeHistory.onCallScheduleId = onCallSchedule.id;
			        onCallScheduleChangeHistory.versionNumber = onCallSchedule.versionNumber;
			        onCallScheduleChangeHistory.timeStamp = DateTime.UtcNow;
			        onCallScheduleChangeHistory.userId = securityUser.id;
			        onCallScheduleChangeHistory.tenantGuid = userTenantGuid;
			        onCallScheduleChangeHistory.data = JsonSerializer.Serialize(Database.OnCallSchedule.CreateAnonymousWithFirstLevelSubObjects(onCallSchedule));
			        _context.OnCallScheduleChangeHistories.Add(onCallScheduleChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Alerting.OnCallSchedule entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.OnCallSchedule.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.OnCallSchedule.CreateAnonymousWithFirstLevelSubObjects(onCallSchedule)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Alerting.OnCallSchedule entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.OnCallSchedule.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.OnCallSchedule.CreateAnonymousWithFirstLevelSubObjects(onCallSchedule)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of OnCallSchedule records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/OnCallSchedules/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			string timeZoneId = null,
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
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);


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

			IQueryable<Database.OnCallSchedule> query = (from ocs in _context.OnCallSchedules select ocs);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ocs => ocs.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ocs => ocs.description == description);
			}
			if (string.IsNullOrEmpty(timeZoneId) == false)
			{
				query = query.Where(ocs => ocs.timeZoneId == timeZoneId);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ocs => ocs.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ocs => ocs.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ocs => ocs.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ocs => ocs.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ocs => ocs.deleted == false);
				}
			}
			else
			{
				query = query.Where(ocs => ocs.active == true);
				query = query.Where(ocs => ocs.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the On Call Schedule, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.timeZoneId.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.timeZoneId);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.OnCallSchedule.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/OnCallSchedule/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Schedule Writer role needed to write to this table, or Alerting Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Alerting Schedule Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
