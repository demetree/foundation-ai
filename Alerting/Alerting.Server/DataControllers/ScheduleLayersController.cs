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
    /// This auto generated class provides the basic CRUD operations for the ScheduleLayer entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ScheduleLayer entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ScheduleLayersController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 100;

		static object scheduleLayerPutSyncRoot = new object();
		static object scheduleLayerDeleteSyncRoot = new object();

		private AlertingContext _context;

		private ILogger<ScheduleLayersController> _logger;

		public ScheduleLayersController(AlertingContext context, ILogger<ScheduleLayersController> logger) : base("Alerting", "ScheduleLayer")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ScheduleLayers filtered by the parameters provided.
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
		[Route("api/ScheduleLayers")]
		public async Task<IActionResult> GetScheduleLayers(
			int? onCallScheduleId = null,
			string name = null,
			string description = null,
			int? layerLevel = null,
			DateTime? rotationStart = null,
			int? rotationDays = null,
			string handoffTime = null,
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
			if (rotationStart.HasValue == true && rotationStart.Value.Kind != DateTimeKind.Utc)
			{
				rotationStart = rotationStart.Value.ToUniversalTime();
			}

			IQueryable<Database.ScheduleLayer> query = (from sl in _context.ScheduleLayers select sl);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (onCallScheduleId.HasValue == true)
			{
				query = query.Where(sl => sl.onCallScheduleId == onCallScheduleId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(sl => sl.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(sl => sl.description == description);
			}
			if (layerLevel.HasValue == true)
			{
				query = query.Where(sl => sl.layerLevel == layerLevel.Value);
			}
			if (rotationStart.HasValue == true)
			{
				query = query.Where(sl => sl.rotationStart == rotationStart.Value);
			}
			if (rotationDays.HasValue == true)
			{
				query = query.Where(sl => sl.rotationDays == rotationDays.Value);
			}
			if (string.IsNullOrEmpty(handoffTime) == false)
			{
				query = query.Where(sl => sl.handoffTime == handoffTime);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(sl => sl.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sl => sl.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sl => sl.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sl => sl.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sl => sl.deleted == false);
				}
			}
			else
			{
				query = query.Where(sl => sl.active == true);
				query = query.Where(sl => sl.deleted == false);
			}

			query = query.OrderBy(sl => sl.name).ThenBy(sl => sl.description).ThenBy(sl => sl.handoffTime);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.onCallSchedule);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Schedule Layer, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.handoffTime.Contains(anyStringContains)
			       || (includeRelations == true && x.onCallSchedule.name.Contains(anyStringContains))
			       || (includeRelations == true && x.onCallSchedule.description.Contains(anyStringContains))
			       || (includeRelations == true && x.onCallSchedule.timeZoneId.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.ScheduleLayer> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ScheduleLayer scheduleLayer in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(scheduleLayer, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Alerting.ScheduleLayer Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Alerting.ScheduleLayer Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ScheduleLayers filtered by the parameters provided.  Its query is similar to the GetScheduleLayers method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleLayers/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? onCallScheduleId = null,
			string name = null,
			string description = null,
			int? layerLevel = null,
			DateTime? rotationStart = null,
			int? rotationDays = null,
			string handoffTime = null,
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
			if (rotationStart.HasValue == true && rotationStart.Value.Kind != DateTimeKind.Utc)
			{
				rotationStart = rotationStart.Value.ToUniversalTime();
			}

			IQueryable<Database.ScheduleLayer> query = (from sl in _context.ScheduleLayers select sl);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (onCallScheduleId.HasValue == true)
			{
				query = query.Where(sl => sl.onCallScheduleId == onCallScheduleId.Value);
			}
			if (name != null)
			{
				query = query.Where(sl => sl.name == name);
			}
			if (description != null)
			{
				query = query.Where(sl => sl.description == description);
			}
			if (layerLevel.HasValue == true)
			{
				query = query.Where(sl => sl.layerLevel == layerLevel.Value);
			}
			if (rotationStart.HasValue == true)
			{
				query = query.Where(sl => sl.rotationStart == rotationStart.Value);
			}
			if (rotationDays.HasValue == true)
			{
				query = query.Where(sl => sl.rotationDays == rotationDays.Value);
			}
			if (handoffTime != null)
			{
				query = query.Where(sl => sl.handoffTime == handoffTime);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(sl => sl.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sl => sl.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sl => sl.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sl => sl.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sl => sl.deleted == false);
				}
			}
			else
			{
				query = query.Where(sl => sl.active == true);
				query = query.Where(sl => sl.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Schedule Layer, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.handoffTime.Contains(anyStringContains)
			       || x.onCallSchedule.name.Contains(anyStringContains)
			       || x.onCallSchedule.description.Contains(anyStringContains)
			       || x.onCallSchedule.timeZoneId.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ScheduleLayer by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleLayer/{id}")]
		public async Task<IActionResult> GetScheduleLayer(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ScheduleLayer> query = (from sl in _context.ScheduleLayers where
							(sl.id == id) &&
							(userIsAdmin == true || sl.deleted == false) &&
							(userIsWriter == true || sl.active == true)
					select sl);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.onCallSchedule);
					query = query.AsSplitQuery();
				}

				Database.ScheduleLayer materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Alerting.ScheduleLayer Entity was read with Admin privilege." : "Alerting.ScheduleLayer Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ScheduleLayer", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Alerting.ScheduleLayer entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Alerting.ScheduleLayer.   Entity was read with Admin privilege." : "Exception caught during entity read of Alerting.ScheduleLayer.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ScheduleLayer record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ScheduleLayer/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutScheduleLayer(int id, [FromBody]Database.ScheduleLayer.ScheduleLayerDTO scheduleLayerDTO, CancellationToken cancellationToken = default)
		{
			if (scheduleLayerDTO == null)
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



			if (id != scheduleLayerDTO.id)
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


			IQueryable<Database.ScheduleLayer> query = (from x in _context.ScheduleLayers
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ScheduleLayer existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.ScheduleLayer PUT", id.ToString(), new Exception("No Alerting.ScheduleLayer entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (scheduleLayerDTO.objectGuid == Guid.Empty)
            {
                scheduleLayerDTO.objectGuid = existing.objectGuid;
            }
            else if (scheduleLayerDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ScheduleLayer record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ScheduleLayer cloneOfExisting = (Database.ScheduleLayer)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ScheduleLayer object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ScheduleLayer scheduleLayer = (Database.ScheduleLayer)_context.Entry(existing).GetDatabaseValues().ToObject();
			scheduleLayer.ApplyDTO(scheduleLayerDTO);
			//
			// The tenant guid for any ScheduleLayer being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ScheduleLayer because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				scheduleLayer.tenantGuid = existing.tenantGuid;
			}

			lock (scheduleLayerPutSyncRoot)
			{
				//
				// Validate the version number for the scheduleLayer being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != scheduleLayer.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ScheduleLayer save attempt was made but save request was with version " + scheduleLayer.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ScheduleLayer you are trying to update has already changed.  Please try your save again after reloading the ScheduleLayer.");
				}
				else
				{
					// Same record.  Increase version.
					scheduleLayer.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (scheduleLayer.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Alerting.ScheduleLayer record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (scheduleLayer.name != null && scheduleLayer.name.Length > 100)
				{
					scheduleLayer.name = scheduleLayer.name.Substring(0, 100);
				}

				if (scheduleLayer.description != null && scheduleLayer.description.Length > 500)
				{
					scheduleLayer.description = scheduleLayer.description.Substring(0, 500);
				}

				if (scheduleLayer.rotationStart.Kind != DateTimeKind.Utc)
				{
					scheduleLayer.rotationStart = scheduleLayer.rotationStart.ToUniversalTime();
				}

				if (scheduleLayer.handoffTime != null && scheduleLayer.handoffTime.Length > 50)
				{
					scheduleLayer.handoffTime = scheduleLayer.handoffTime.Substring(0, 50);
				}

				try
				{
				    EntityEntry<Database.ScheduleLayer> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(scheduleLayer);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ScheduleLayerChangeHistory scheduleLayerChangeHistory = new ScheduleLayerChangeHistory();
				        scheduleLayerChangeHistory.scheduleLayerId = scheduleLayer.id;
				        scheduleLayerChangeHistory.versionNumber = scheduleLayer.versionNumber;
				        scheduleLayerChangeHistory.timeStamp = DateTime.UtcNow;
				        scheduleLayerChangeHistory.userId = securityUser.id;
				        scheduleLayerChangeHistory.tenantGuid = userTenantGuid;
				        scheduleLayerChangeHistory.data = JsonSerializer.Serialize(Database.ScheduleLayer.CreateAnonymousWithFirstLevelSubObjects(scheduleLayer));
				        _context.ScheduleLayerChangeHistories.Add(scheduleLayerChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.ScheduleLayer entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduleLayer.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduleLayer.CreateAnonymousWithFirstLevelSubObjects(scheduleLayer)),
						null);

				return Ok(Database.ScheduleLayer.CreateAnonymous(scheduleLayer));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.ScheduleLayer entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduleLayer.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduleLayer.CreateAnonymousWithFirstLevelSubObjects(scheduleLayer)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ScheduleLayer record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleLayer", Name = "ScheduleLayer")]
		public async Task<IActionResult> PostScheduleLayer([FromBody]Database.ScheduleLayer.ScheduleLayerDTO scheduleLayerDTO, CancellationToken cancellationToken = default)
		{
			if (scheduleLayerDTO == null)
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
			// Create a new ScheduleLayer object using the data from the DTO
			//
			Database.ScheduleLayer scheduleLayer = Database.ScheduleLayer.FromDTO(scheduleLayerDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				scheduleLayer.tenantGuid = userTenantGuid;

				if (scheduleLayer.name != null && scheduleLayer.name.Length > 100)
				{
					scheduleLayer.name = scheduleLayer.name.Substring(0, 100);
				}

				if (scheduleLayer.description != null && scheduleLayer.description.Length > 500)
				{
					scheduleLayer.description = scheduleLayer.description.Substring(0, 500);
				}

				if (scheduleLayer.rotationStart.Kind != DateTimeKind.Utc)
				{
					scheduleLayer.rotationStart = scheduleLayer.rotationStart.ToUniversalTime();
				}

				if (scheduleLayer.handoffTime != null && scheduleLayer.handoffTime.Length > 50)
				{
					scheduleLayer.handoffTime = scheduleLayer.handoffTime.Substring(0, 50);
				}

				scheduleLayer.objectGuid = Guid.NewGuid();
				scheduleLayer.versionNumber = 1;

				_context.ScheduleLayers.Add(scheduleLayer);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the scheduleLayer object so that no further changes will be written to the database
				    //
				    _context.Entry(scheduleLayer).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					scheduleLayer.ScheduleLayerChangeHistories = null;
					scheduleLayer.ScheduleLayerMembers = null;
					scheduleLayer.ScheduleOverrides = null;
					scheduleLayer.onCallSchedule = null;


				    ScheduleLayerChangeHistory scheduleLayerChangeHistory = new ScheduleLayerChangeHistory();
				    scheduleLayerChangeHistory.scheduleLayerId = scheduleLayer.id;
				    scheduleLayerChangeHistory.versionNumber = scheduleLayer.versionNumber;
				    scheduleLayerChangeHistory.timeStamp = DateTime.UtcNow;
				    scheduleLayerChangeHistory.userId = securityUser.id;
				    scheduleLayerChangeHistory.tenantGuid = userTenantGuid;
				    scheduleLayerChangeHistory.data = JsonSerializer.Serialize(Database.ScheduleLayer.CreateAnonymousWithFirstLevelSubObjects(scheduleLayer));
				    _context.ScheduleLayerChangeHistories.Add(scheduleLayerChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Alerting.ScheduleLayer entity successfully created.",
						true,
						scheduleLayer. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ScheduleLayer.CreateAnonymousWithFirstLevelSubObjects(scheduleLayer)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Alerting.ScheduleLayer entity creation failed.", false, scheduleLayer.id.ToString(), "", JsonSerializer.Serialize(Database.ScheduleLayer.CreateAnonymousWithFirstLevelSubObjects(scheduleLayer)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ScheduleLayer", scheduleLayer.id, scheduleLayer.name));

			return CreatedAtRoute("ScheduleLayer", new { id = scheduleLayer.id }, Database.ScheduleLayer.CreateAnonymousWithFirstLevelSubObjects(scheduleLayer));
		}



        /// <summary>
        /// 
        /// This rolls a ScheduleLayer entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleLayer/Rollback/{id}")]
		[Route("api/ScheduleLayer/Rollback")]
		public async Task<IActionResult> RollbackToScheduleLayerVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.ScheduleLayer> query = (from x in _context.ScheduleLayers
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this ScheduleLayer concurrently
			//
			lock (scheduleLayerPutSyncRoot)
			{
				
				Database.ScheduleLayer scheduleLayer = query.FirstOrDefault();
				
				if (scheduleLayer == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.ScheduleLayer rollback", id.ToString(), new Exception("No Alerting.ScheduleLayer entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ScheduleLayer current state so we can log it.
				//
				Database.ScheduleLayer cloneOfExisting = (Database.ScheduleLayer)_context.Entry(scheduleLayer).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.ScheduleLayerChangeHistories = null;
				cloneOfExisting.ScheduleLayerMembers = null;
				cloneOfExisting.ScheduleOverrides = null;
				cloneOfExisting.onCallSchedule = null;

				if (versionNumber >= scheduleLayer.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Alerting.ScheduleLayer rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Alerting.ScheduleLayer rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ScheduleLayerChangeHistory scheduleLayerChangeHistory = (from x in _context.ScheduleLayerChangeHistories
				                                               where
				                                               x.scheduleLayerId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (scheduleLayerChangeHistory != null)
				{
				    Database.ScheduleLayer oldScheduleLayer = JsonSerializer.Deserialize<Database.ScheduleLayer>(scheduleLayerChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    scheduleLayer.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    scheduleLayer.onCallScheduleId = oldScheduleLayer.onCallScheduleId;
				    scheduleLayer.name = oldScheduleLayer.name;
				    scheduleLayer.description = oldScheduleLayer.description;
				    scheduleLayer.layerLevel = oldScheduleLayer.layerLevel;
				    scheduleLayer.rotationStart = oldScheduleLayer.rotationStart;
				    scheduleLayer.rotationDays = oldScheduleLayer.rotationDays;
				    scheduleLayer.handoffTime = oldScheduleLayer.handoffTime;
				    scheduleLayer.objectGuid = oldScheduleLayer.objectGuid;
				    scheduleLayer.active = oldScheduleLayer.active;
				    scheduleLayer.deleted = oldScheduleLayer.deleted;

				    string serializedScheduleLayer = JsonSerializer.Serialize(scheduleLayer);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ScheduleLayerChangeHistory newScheduleLayerChangeHistory = new ScheduleLayerChangeHistory();
				        newScheduleLayerChangeHistory.scheduleLayerId = scheduleLayer.id;
				        newScheduleLayerChangeHistory.versionNumber = scheduleLayer.versionNumber;
				        newScheduleLayerChangeHistory.timeStamp = DateTime.UtcNow;
				        newScheduleLayerChangeHistory.userId = securityUser.id;
				        newScheduleLayerChangeHistory.tenantGuid = userTenantGuid;
				        newScheduleLayerChangeHistory.data = JsonSerializer.Serialize(Database.ScheduleLayer.CreateAnonymousWithFirstLevelSubObjects(scheduleLayer));
				        _context.ScheduleLayerChangeHistories.Add(newScheduleLayerChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.ScheduleLayer rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduleLayer.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduleLayer.CreateAnonymousWithFirstLevelSubObjects(scheduleLayer)),
						null);


				    return Ok(Database.ScheduleLayer.CreateAnonymous(scheduleLayer));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Alerting.ScheduleLayer rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Alerting.ScheduleLayer rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a ScheduleLayer.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduleLayer</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleLayer/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetScheduleLayerChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.ScheduleLayer scheduleLayer = await _context.ScheduleLayers.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduleLayer == null)
			{
				return NotFound();
			}

			try
			{
				scheduleLayer.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ScheduleLayer> versionInfo = await scheduleLayer.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a ScheduleLayer.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduleLayer</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleLayer/{id}/AuditHistory")]
		public async Task<IActionResult> GetScheduleLayerAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.ScheduleLayer scheduleLayer = await _context.ScheduleLayers.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduleLayer == null)
			{
				return NotFound();
			}

			try
			{
				scheduleLayer.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.ScheduleLayer>> versions = await scheduleLayer.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a ScheduleLayer.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduleLayer</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The ScheduleLayer object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleLayer/{id}/Version/{version}")]
		public async Task<IActionResult> GetScheduleLayerVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.ScheduleLayer scheduleLayer = await _context.ScheduleLayers.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduleLayer == null)
			{
				return NotFound();
			}

			try
			{
				scheduleLayer.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ScheduleLayer> versionInfo = await scheduleLayer.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a ScheduleLayer at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduleLayer</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The ScheduleLayer object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleLayer/{id}/StateAtTime")]
		public async Task<IActionResult> GetScheduleLayerStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.ScheduleLayer scheduleLayer = await _context.ScheduleLayers.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduleLayer == null)
			{
				return NotFound();
			}

			try
			{
				scheduleLayer.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ScheduleLayer> versionInfo = await scheduleLayer.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a ScheduleLayer record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleLayer/{id}")]
		[Route("api/ScheduleLayer")]
		public async Task<IActionResult> DeleteScheduleLayer(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ScheduleLayer> query = (from x in _context.ScheduleLayers
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ScheduleLayer scheduleLayer = await query.FirstOrDefaultAsync(cancellationToken);

			if (scheduleLayer == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.ScheduleLayer DELETE", id.ToString(), new Exception("No Alerting.ScheduleLayer entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ScheduleLayer cloneOfExisting = (Database.ScheduleLayer)_context.Entry(scheduleLayer).GetDatabaseValues().ToObject();


			lock (scheduleLayerDeleteSyncRoot)
			{
			    try
			    {
			        scheduleLayer.deleted = true;
			        scheduleLayer.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ScheduleLayerChangeHistory scheduleLayerChangeHistory = new ScheduleLayerChangeHistory();
			        scheduleLayerChangeHistory.scheduleLayerId = scheduleLayer.id;
			        scheduleLayerChangeHistory.versionNumber = scheduleLayer.versionNumber;
			        scheduleLayerChangeHistory.timeStamp = DateTime.UtcNow;
			        scheduleLayerChangeHistory.userId = securityUser.id;
			        scheduleLayerChangeHistory.tenantGuid = userTenantGuid;
			        scheduleLayerChangeHistory.data = JsonSerializer.Serialize(Database.ScheduleLayer.CreateAnonymousWithFirstLevelSubObjects(scheduleLayer));
			        _context.ScheduleLayerChangeHistories.Add(scheduleLayerChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Alerting.ScheduleLayer entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduleLayer.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduleLayer.CreateAnonymousWithFirstLevelSubObjects(scheduleLayer)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Alerting.ScheduleLayer entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduleLayer.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduleLayer.CreateAnonymousWithFirstLevelSubObjects(scheduleLayer)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of ScheduleLayer records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ScheduleLayers/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? onCallScheduleId = null,
			string name = null,
			string description = null,
			int? layerLevel = null,
			DateTime? rotationStart = null,
			int? rotationDays = null,
			string handoffTime = null,
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
			if (rotationStart.HasValue == true && rotationStart.Value.Kind != DateTimeKind.Utc)
			{
				rotationStart = rotationStart.Value.ToUniversalTime();
			}

			IQueryable<Database.ScheduleLayer> query = (from sl in _context.ScheduleLayers select sl);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (onCallScheduleId.HasValue == true)
			{
				query = query.Where(sl => sl.onCallScheduleId == onCallScheduleId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(sl => sl.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(sl => sl.description == description);
			}
			if (layerLevel.HasValue == true)
			{
				query = query.Where(sl => sl.layerLevel == layerLevel.Value);
			}
			if (rotationStart.HasValue == true)
			{
				query = query.Where(sl => sl.rotationStart == rotationStart.Value);
			}
			if (rotationDays.HasValue == true)
			{
				query = query.Where(sl => sl.rotationDays == rotationDays.Value);
			}
			if (string.IsNullOrEmpty(handoffTime) == false)
			{
				query = query.Where(sl => sl.handoffTime == handoffTime);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(sl => sl.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sl => sl.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sl => sl.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sl => sl.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sl => sl.deleted == false);
				}
			}
			else
			{
				query = query.Where(sl => sl.active == true);
				query = query.Where(sl => sl.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Schedule Layer, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.handoffTime.Contains(anyStringContains)
			       || x.onCallSchedule.name.Contains(anyStringContains)
			       || x.onCallSchedule.description.Contains(anyStringContains)
			       || x.onCallSchedule.timeZoneId.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.handoffTime);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ScheduleLayer.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ScheduleLayer/CreateAuditEvent")]
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
