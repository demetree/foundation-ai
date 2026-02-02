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
    /// This auto generated class provides the basic CRUD operations for the ScheduleOverride entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ScheduleOverride entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ScheduleOverridesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 100;

		static object scheduleOverridePutSyncRoot = new object();
		static object scheduleOverrideDeleteSyncRoot = new object();

		private AlertingContext _context;

		private ILogger<ScheduleOverridesController> _logger;

		public ScheduleOverridesController(AlertingContext context, ILogger<ScheduleOverridesController> logger) : base("Alerting", "ScheduleOverride")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ScheduleOverrides filtered by the parameters provided.
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
		[Route("api/ScheduleOverrides")]
		public async Task<IActionResult> GetScheduleOverrides(
			int? onCallScheduleId = null,
			int? scheduleLayerId = null,
			DateTime? startDateTime = null,
			DateTime? endDateTime = null,
			int? scheduleOverrideTypeId = null,
			Guid? originalUserObjectGuid = null,
			Guid? replacementUserObjectGuid = null,
			string reason = null,
			Guid? createdByUserObjectGuid = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (startDateTime.HasValue == true && startDateTime.Value.Kind != DateTimeKind.Utc)
			{
				startDateTime = startDateTime.Value.ToUniversalTime();
			}

			if (endDateTime.HasValue == true && endDateTime.Value.Kind != DateTimeKind.Utc)
			{
				endDateTime = endDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.ScheduleOverride> query = (from so in _context.ScheduleOverrides select so);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (onCallScheduleId.HasValue == true)
			{
				query = query.Where(so => so.onCallScheduleId == onCallScheduleId.Value);
			}
			if (scheduleLayerId.HasValue == true)
			{
				query = query.Where(so => so.scheduleLayerId == scheduleLayerId.Value);
			}
			if (startDateTime.HasValue == true)
			{
				query = query.Where(so => so.startDateTime == startDateTime.Value);
			}
			if (endDateTime.HasValue == true)
			{
				query = query.Where(so => so.endDateTime == endDateTime.Value);
			}
			if (scheduleOverrideTypeId.HasValue == true)
			{
				query = query.Where(so => so.scheduleOverrideTypeId == scheduleOverrideTypeId.Value);
			}
			if (originalUserObjectGuid.HasValue == true)
			{
				query = query.Where(so => so.originalUserObjectGuid == originalUserObjectGuid);
			}
			if (replacementUserObjectGuid.HasValue == true)
			{
				query = query.Where(so => so.replacementUserObjectGuid == replacementUserObjectGuid);
			}
			if (string.IsNullOrEmpty(reason) == false)
			{
				query = query.Where(so => so.reason == reason);
			}
			if (createdByUserObjectGuid.HasValue == true)
			{
				query = query.Where(so => so.createdByUserObjectGuid == createdByUserObjectGuid);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(so => so.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(so => so.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(so => so.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(so => so.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(so => so.deleted == false);
				}
			}
			else
			{
				query = query.Where(so => so.active == true);
				query = query.Where(so => so.deleted == false);
			}

			query = query.OrderBy(so => so.reason);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.onCallSchedule);
				query = query.Include(x => x.scheduleLayer);
				query = query.Include(x => x.scheduleOverrideType);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Schedule Override, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.reason.Contains(anyStringContains)
			       || (includeRelations == true && x.onCallSchedule.name.Contains(anyStringContains))
			       || (includeRelations == true && x.onCallSchedule.description.Contains(anyStringContains))
			       || (includeRelations == true && x.onCallSchedule.timeZoneId.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduleLayer.name.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduleLayer.description.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduleLayer.handoffTime.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduleOverrideType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduleOverrideType.description.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.ScheduleOverride> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ScheduleOverride scheduleOverride in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(scheduleOverride, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Alerting.ScheduleOverride Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Alerting.ScheduleOverride Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ScheduleOverrides filtered by the parameters provided.  Its query is similar to the GetScheduleOverrides method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleOverrides/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? onCallScheduleId = null,
			int? scheduleLayerId = null,
			DateTime? startDateTime = null,
			DateTime? endDateTime = null,
			int? scheduleOverrideTypeId = null,
			Guid? originalUserObjectGuid = null,
			Guid? replacementUserObjectGuid = null,
			string reason = null,
			Guid? createdByUserObjectGuid = null,
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


			//
			// Fix any non-UTC date parameters that come in.
			//
			if (startDateTime.HasValue == true && startDateTime.Value.Kind != DateTimeKind.Utc)
			{
				startDateTime = startDateTime.Value.ToUniversalTime();
			}

			if (endDateTime.HasValue == true && endDateTime.Value.Kind != DateTimeKind.Utc)
			{
				endDateTime = endDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.ScheduleOverride> query = (from so in _context.ScheduleOverrides select so);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (onCallScheduleId.HasValue == true)
			{
				query = query.Where(so => so.onCallScheduleId == onCallScheduleId.Value);
			}
			if (scheduleLayerId.HasValue == true)
			{
				query = query.Where(so => so.scheduleLayerId == scheduleLayerId.Value);
			}
			if (startDateTime.HasValue == true)
			{
				query = query.Where(so => so.startDateTime == startDateTime.Value);
			}
			if (endDateTime.HasValue == true)
			{
				query = query.Where(so => so.endDateTime == endDateTime.Value);
			}
			if (scheduleOverrideTypeId.HasValue == true)
			{
				query = query.Where(so => so.scheduleOverrideTypeId == scheduleOverrideTypeId.Value);
			}
			if (originalUserObjectGuid.HasValue == true)
			{
				query = query.Where(so => so.originalUserObjectGuid == originalUserObjectGuid);
			}
			if (replacementUserObjectGuid.HasValue == true)
			{
				query = query.Where(so => so.replacementUserObjectGuid == replacementUserObjectGuid);
			}
			if (reason != null)
			{
				query = query.Where(so => so.reason == reason);
			}
			if (createdByUserObjectGuid.HasValue == true)
			{
				query = query.Where(so => so.createdByUserObjectGuid == createdByUserObjectGuid);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(so => so.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(so => so.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(so => so.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(so => so.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(so => so.deleted == false);
				}
			}
			else
			{
				query = query.Where(so => so.active == true);
				query = query.Where(so => so.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Schedule Override, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.reason.Contains(anyStringContains)
			       || x.onCallSchedule.name.Contains(anyStringContains)
			       || x.onCallSchedule.description.Contains(anyStringContains)
			       || x.onCallSchedule.timeZoneId.Contains(anyStringContains)
			       || x.scheduleLayer.name.Contains(anyStringContains)
			       || x.scheduleLayer.description.Contains(anyStringContains)
			       || x.scheduleLayer.handoffTime.Contains(anyStringContains)
			       || x.scheduleOverrideType.name.Contains(anyStringContains)
			       || x.scheduleOverrideType.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ScheduleOverride by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleOverride/{id}")]
		public async Task<IActionResult> GetScheduleOverride(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ScheduleOverride> query = (from so in _context.ScheduleOverrides where
							(so.id == id) &&
							(userIsAdmin == true || so.deleted == false) &&
							(userIsWriter == true || so.active == true)
					select so);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.onCallSchedule);
					query = query.Include(x => x.scheduleLayer);
					query = query.Include(x => x.scheduleOverrideType);
					query = query.AsSplitQuery();
				}

				Database.ScheduleOverride materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Alerting.ScheduleOverride Entity was read with Admin privilege." : "Alerting.ScheduleOverride Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ScheduleOverride", materialized.id, materialized.reason));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Alerting.ScheduleOverride entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Alerting.ScheduleOverride.   Entity was read with Admin privilege." : "Exception caught during entity read of Alerting.ScheduleOverride.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ScheduleOverride record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ScheduleOverride/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutScheduleOverride(int id, [FromBody]Database.ScheduleOverride.ScheduleOverrideDTO scheduleOverrideDTO, CancellationToken cancellationToken = default)
		{
			if (scheduleOverrideDTO == null)
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



			if (id != scheduleOverrideDTO.id)
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


			IQueryable<Database.ScheduleOverride> query = (from x in _context.ScheduleOverrides
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ScheduleOverride existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.ScheduleOverride PUT", id.ToString(), new Exception("No Alerting.ScheduleOverride entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (scheduleOverrideDTO.objectGuid == Guid.Empty)
            {
                scheduleOverrideDTO.objectGuid = existing.objectGuid;
            }
            else if (scheduleOverrideDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ScheduleOverride record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ScheduleOverride cloneOfExisting = (Database.ScheduleOverride)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ScheduleOverride object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ScheduleOverride scheduleOverride = (Database.ScheduleOverride)_context.Entry(existing).GetDatabaseValues().ToObject();
			scheduleOverride.ApplyDTO(scheduleOverrideDTO);
			//
			// The tenant guid for any ScheduleOverride being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ScheduleOverride because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				scheduleOverride.tenantGuid = existing.tenantGuid;
			}

			lock (scheduleOverridePutSyncRoot)
			{
				//
				// Validate the version number for the scheduleOverride being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != scheduleOverride.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ScheduleOverride save attempt was made but save request was with version " + scheduleOverride.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ScheduleOverride you are trying to update has already changed.  Please try your save again after reloading the ScheduleOverride.");
				}
				else
				{
					// Same record.  Increase version.
					scheduleOverride.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (scheduleOverride.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Alerting.ScheduleOverride record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (scheduleOverride.startDateTime.Kind != DateTimeKind.Utc)
				{
					scheduleOverride.startDateTime = scheduleOverride.startDateTime.ToUniversalTime();
				}

				if (scheduleOverride.endDateTime.Kind != DateTimeKind.Utc)
				{
					scheduleOverride.endDateTime = scheduleOverride.endDateTime.ToUniversalTime();
				}

				if (scheduleOverride.reason != null && scheduleOverride.reason.Length > 500)
				{
					scheduleOverride.reason = scheduleOverride.reason.Substring(0, 500);
				}

				try
				{
				    EntityEntry<Database.ScheduleOverride> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(scheduleOverride);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ScheduleOverrideChangeHistory scheduleOverrideChangeHistory = new ScheduleOverrideChangeHistory();
				        scheduleOverrideChangeHistory.scheduleOverrideId = scheduleOverride.id;
				        scheduleOverrideChangeHistory.versionNumber = scheduleOverride.versionNumber;
				        scheduleOverrideChangeHistory.timeStamp = DateTime.UtcNow;
				        scheduleOverrideChangeHistory.userId = securityUser.id;
				        scheduleOverrideChangeHistory.tenantGuid = userTenantGuid;
				        scheduleOverrideChangeHistory.data = JsonSerializer.Serialize(Database.ScheduleOverride.CreateAnonymousWithFirstLevelSubObjects(scheduleOverride));
				        _context.ScheduleOverrideChangeHistories.Add(scheduleOverrideChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.ScheduleOverride entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduleOverride.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduleOverride.CreateAnonymousWithFirstLevelSubObjects(scheduleOverride)),
						null);

				return Ok(Database.ScheduleOverride.CreateAnonymous(scheduleOverride));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.ScheduleOverride entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduleOverride.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduleOverride.CreateAnonymousWithFirstLevelSubObjects(scheduleOverride)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ScheduleOverride record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleOverride", Name = "ScheduleOverride")]
		public async Task<IActionResult> PostScheduleOverride([FromBody]Database.ScheduleOverride.ScheduleOverrideDTO scheduleOverrideDTO, CancellationToken cancellationToken = default)
		{
			if (scheduleOverrideDTO == null)
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
			// Create a new ScheduleOverride object using the data from the DTO
			//
			Database.ScheduleOverride scheduleOverride = Database.ScheduleOverride.FromDTO(scheduleOverrideDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				scheduleOverride.tenantGuid = userTenantGuid;

				if (scheduleOverride.startDateTime.Kind != DateTimeKind.Utc)
				{
					scheduleOverride.startDateTime = scheduleOverride.startDateTime.ToUniversalTime();
				}

				if (scheduleOverride.endDateTime.Kind != DateTimeKind.Utc)
				{
					scheduleOverride.endDateTime = scheduleOverride.endDateTime.ToUniversalTime();
				}

				if (scheduleOverride.reason != null && scheduleOverride.reason.Length > 500)
				{
					scheduleOverride.reason = scheduleOverride.reason.Substring(0, 500);
				}

				scheduleOverride.objectGuid = Guid.NewGuid();
				scheduleOverride.versionNumber = 1;

				_context.ScheduleOverrides.Add(scheduleOverride);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the scheduleOverride object so that no further changes will be written to the database
				    //
				    _context.Entry(scheduleOverride).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					scheduleOverride.ScheduleOverrideChangeHistories = null;
					scheduleOverride.onCallSchedule = null;
					scheduleOverride.scheduleLayer = null;
					scheduleOverride.scheduleOverrideType = null;


				    ScheduleOverrideChangeHistory scheduleOverrideChangeHistory = new ScheduleOverrideChangeHistory();
				    scheduleOverrideChangeHistory.scheduleOverrideId = scheduleOverride.id;
				    scheduleOverrideChangeHistory.versionNumber = scheduleOverride.versionNumber;
				    scheduleOverrideChangeHistory.timeStamp = DateTime.UtcNow;
				    scheduleOverrideChangeHistory.userId = securityUser.id;
				    scheduleOverrideChangeHistory.tenantGuid = userTenantGuid;
				    scheduleOverrideChangeHistory.data = JsonSerializer.Serialize(Database.ScheduleOverride.CreateAnonymousWithFirstLevelSubObjects(scheduleOverride));
				    _context.ScheduleOverrideChangeHistories.Add(scheduleOverrideChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Alerting.ScheduleOverride entity successfully created.",
						true,
						scheduleOverride. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ScheduleOverride.CreateAnonymousWithFirstLevelSubObjects(scheduleOverride)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Alerting.ScheduleOverride entity creation failed.", false, scheduleOverride.id.ToString(), "", JsonSerializer.Serialize(Database.ScheduleOverride.CreateAnonymousWithFirstLevelSubObjects(scheduleOverride)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ScheduleOverride", scheduleOverride.id, scheduleOverride.reason));

			return CreatedAtRoute("ScheduleOverride", new { id = scheduleOverride.id }, Database.ScheduleOverride.CreateAnonymousWithFirstLevelSubObjects(scheduleOverride));
		}



        /// <summary>
        /// 
        /// This rolls a ScheduleOverride entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleOverride/Rollback/{id}")]
		[Route("api/ScheduleOverride/Rollback")]
		public async Task<IActionResult> RollbackToScheduleOverrideVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.ScheduleOverride> query = (from x in _context.ScheduleOverrides
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this ScheduleOverride concurrently
			//
			lock (scheduleOverridePutSyncRoot)
			{
				
				Database.ScheduleOverride scheduleOverride = query.FirstOrDefault();
				
				if (scheduleOverride == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.ScheduleOverride rollback", id.ToString(), new Exception("No Alerting.ScheduleOverride entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ScheduleOverride current state so we can log it.
				//
				Database.ScheduleOverride cloneOfExisting = (Database.ScheduleOverride)_context.Entry(scheduleOverride).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.ScheduleOverrideChangeHistories = null;
				cloneOfExisting.onCallSchedule = null;
				cloneOfExisting.scheduleLayer = null;
				cloneOfExisting.scheduleOverrideType = null;

				if (versionNumber >= scheduleOverride.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Alerting.ScheduleOverride rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Alerting.ScheduleOverride rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ScheduleOverrideChangeHistory scheduleOverrideChangeHistory = (from x in _context.ScheduleOverrideChangeHistories
				                                               where
				                                               x.scheduleOverrideId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (scheduleOverrideChangeHistory != null)
				{
				    Database.ScheduleOverride oldScheduleOverride = JsonSerializer.Deserialize<Database.ScheduleOverride>(scheduleOverrideChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    scheduleOverride.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    scheduleOverride.onCallScheduleId = oldScheduleOverride.onCallScheduleId;
				    scheduleOverride.scheduleLayerId = oldScheduleOverride.scheduleLayerId;
				    scheduleOverride.startDateTime = oldScheduleOverride.startDateTime;
				    scheduleOverride.endDateTime = oldScheduleOverride.endDateTime;
				    scheduleOverride.scheduleOverrideTypeId = oldScheduleOverride.scheduleOverrideTypeId;
				    scheduleOverride.originalUserObjectGuid = oldScheduleOverride.originalUserObjectGuid;
				    scheduleOverride.replacementUserObjectGuid = oldScheduleOverride.replacementUserObjectGuid;
				    scheduleOverride.reason = oldScheduleOverride.reason;
				    scheduleOverride.createdByUserObjectGuid = oldScheduleOverride.createdByUserObjectGuid;
				    scheduleOverride.objectGuid = oldScheduleOverride.objectGuid;
				    scheduleOverride.active = oldScheduleOverride.active;
				    scheduleOverride.deleted = oldScheduleOverride.deleted;

				    string serializedScheduleOverride = JsonSerializer.Serialize(scheduleOverride);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ScheduleOverrideChangeHistory newScheduleOverrideChangeHistory = new ScheduleOverrideChangeHistory();
				        newScheduleOverrideChangeHistory.scheduleOverrideId = scheduleOverride.id;
				        newScheduleOverrideChangeHistory.versionNumber = scheduleOverride.versionNumber;
				        newScheduleOverrideChangeHistory.timeStamp = DateTime.UtcNow;
				        newScheduleOverrideChangeHistory.userId = securityUser.id;
				        newScheduleOverrideChangeHistory.tenantGuid = userTenantGuid;
				        newScheduleOverrideChangeHistory.data = JsonSerializer.Serialize(Database.ScheduleOverride.CreateAnonymousWithFirstLevelSubObjects(scheduleOverride));
				        _context.ScheduleOverrideChangeHistories.Add(newScheduleOverrideChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.ScheduleOverride rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduleOverride.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduleOverride.CreateAnonymousWithFirstLevelSubObjects(scheduleOverride)),
						null);


				    return Ok(Database.ScheduleOverride.CreateAnonymous(scheduleOverride));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Alerting.ScheduleOverride rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Alerting.ScheduleOverride rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a ScheduleOverride.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduleOverride</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleOverride/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetScheduleOverrideChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.ScheduleOverride scheduleOverride = await _context.ScheduleOverrides.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduleOverride == null)
			{
				return NotFound();
			}

			try
			{
				scheduleOverride.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ScheduleOverride> versionInfo = await scheduleOverride.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a ScheduleOverride.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduleOverride</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleOverride/{id}/AuditHistory")]
		public async Task<IActionResult> GetScheduleOverrideAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.ScheduleOverride scheduleOverride = await _context.ScheduleOverrides.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduleOverride == null)
			{
				return NotFound();
			}

			try
			{
				scheduleOverride.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.ScheduleOverride>> versions = await scheduleOverride.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a ScheduleOverride.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduleOverride</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The ScheduleOverride object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleOverride/{id}/Version/{version}")]
		public async Task<IActionResult> GetScheduleOverrideVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.ScheduleOverride scheduleOverride = await _context.ScheduleOverrides.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduleOverride == null)
			{
				return NotFound();
			}

			try
			{
				scheduleOverride.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ScheduleOverride> versionInfo = await scheduleOverride.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a ScheduleOverride at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduleOverride</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The ScheduleOverride object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleOverride/{id}/StateAtTime")]
		public async Task<IActionResult> GetScheduleOverrideStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.ScheduleOverride scheduleOverride = await _context.ScheduleOverrides.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduleOverride == null)
			{
				return NotFound();
			}

			try
			{
				scheduleOverride.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ScheduleOverride> versionInfo = await scheduleOverride.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a ScheduleOverride record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleOverride/{id}")]
		[Route("api/ScheduleOverride")]
		public async Task<IActionResult> DeleteScheduleOverride(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ScheduleOverride> query = (from x in _context.ScheduleOverrides
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ScheduleOverride scheduleOverride = await query.FirstOrDefaultAsync(cancellationToken);

			if (scheduleOverride == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.ScheduleOverride DELETE", id.ToString(), new Exception("No Alerting.ScheduleOverride entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ScheduleOverride cloneOfExisting = (Database.ScheduleOverride)_context.Entry(scheduleOverride).GetDatabaseValues().ToObject();


			lock (scheduleOverrideDeleteSyncRoot)
			{
			    try
			    {
			        scheduleOverride.deleted = true;
			        scheduleOverride.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ScheduleOverrideChangeHistory scheduleOverrideChangeHistory = new ScheduleOverrideChangeHistory();
			        scheduleOverrideChangeHistory.scheduleOverrideId = scheduleOverride.id;
			        scheduleOverrideChangeHistory.versionNumber = scheduleOverride.versionNumber;
			        scheduleOverrideChangeHistory.timeStamp = DateTime.UtcNow;
			        scheduleOverrideChangeHistory.userId = securityUser.id;
			        scheduleOverrideChangeHistory.tenantGuid = userTenantGuid;
			        scheduleOverrideChangeHistory.data = JsonSerializer.Serialize(Database.ScheduleOverride.CreateAnonymousWithFirstLevelSubObjects(scheduleOverride));
			        _context.ScheduleOverrideChangeHistories.Add(scheduleOverrideChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Alerting.ScheduleOverride entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduleOverride.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduleOverride.CreateAnonymousWithFirstLevelSubObjects(scheduleOverride)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Alerting.ScheduleOverride entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduleOverride.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduleOverride.CreateAnonymousWithFirstLevelSubObjects(scheduleOverride)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of ScheduleOverride records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ScheduleOverrides/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? onCallScheduleId = null,
			int? scheduleLayerId = null,
			DateTime? startDateTime = null,
			DateTime? endDateTime = null,
			int? scheduleOverrideTypeId = null,
			Guid? originalUserObjectGuid = null,
			Guid? replacementUserObjectGuid = null,
			string reason = null,
			Guid? createdByUserObjectGuid = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (startDateTime.HasValue == true && startDateTime.Value.Kind != DateTimeKind.Utc)
			{
				startDateTime = startDateTime.Value.ToUniversalTime();
			}

			if (endDateTime.HasValue == true && endDateTime.Value.Kind != DateTimeKind.Utc)
			{
				endDateTime = endDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.ScheduleOverride> query = (from so in _context.ScheduleOverrides select so);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (onCallScheduleId.HasValue == true)
			{
				query = query.Where(so => so.onCallScheduleId == onCallScheduleId.Value);
			}
			if (scheduleLayerId.HasValue == true)
			{
				query = query.Where(so => so.scheduleLayerId == scheduleLayerId.Value);
			}
			if (startDateTime.HasValue == true)
			{
				query = query.Where(so => so.startDateTime == startDateTime.Value);
			}
			if (endDateTime.HasValue == true)
			{
				query = query.Where(so => so.endDateTime == endDateTime.Value);
			}
			if (scheduleOverrideTypeId.HasValue == true)
			{
				query = query.Where(so => so.scheduleOverrideTypeId == scheduleOverrideTypeId.Value);
			}
			if (originalUserObjectGuid.HasValue == true)
			{
				query = query.Where(so => so.originalUserObjectGuid == originalUserObjectGuid);
			}
			if (replacementUserObjectGuid.HasValue == true)
			{
				query = query.Where(so => so.replacementUserObjectGuid == replacementUserObjectGuid);
			}
			if (string.IsNullOrEmpty(reason) == false)
			{
				query = query.Where(so => so.reason == reason);
			}
			if (createdByUserObjectGuid.HasValue == true)
			{
				query = query.Where(so => so.createdByUserObjectGuid == createdByUserObjectGuid);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(so => so.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(so => so.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(so => so.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(so => so.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(so => so.deleted == false);
				}
			}
			else
			{
				query = query.Where(so => so.active == true);
				query = query.Where(so => so.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Schedule Override, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.reason.Contains(anyStringContains)
			       || x.onCallSchedule.name.Contains(anyStringContains)
			       || x.onCallSchedule.description.Contains(anyStringContains)
			       || x.onCallSchedule.timeZoneId.Contains(anyStringContains)
			       || x.scheduleLayer.name.Contains(anyStringContains)
			       || x.scheduleLayer.description.Contains(anyStringContains)
			       || x.scheduleLayer.handoffTime.Contains(anyStringContains)
			       || x.scheduleOverrideType.name.Contains(anyStringContains)
			       || x.scheduleOverrideType.description.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.reason);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ScheduleOverride.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ScheduleOverride/CreateAuditEvent")]
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
