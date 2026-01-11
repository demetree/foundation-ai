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
    /// This auto generated class provides the basic CRUD operations for the ScheduledEventDependency entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ScheduledEventDependency entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ScheduledEventDependenciesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object scheduledEventDependencyPutSyncRoot = new object();
		static object scheduledEventDependencyDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<ScheduledEventDependenciesController> _logger;

		public ScheduledEventDependenciesController(SchedulerContext context, ILogger<ScheduledEventDependenciesController> logger) : base("Scheduler", "ScheduledEventDependency")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ScheduledEventDependencies filtered by the parameters provided.
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
		[Route("api/ScheduledEventDependencies")]
		public async Task<IActionResult> GetScheduledEventDependencies(
			int? predecessorEventId = null,
			int? successorEventId = null,
			int? dependencyTypeId = null,
			int? lagMinutes = null,
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

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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

			IQueryable<Database.ScheduledEventDependency> query = (from sed in _context.ScheduledEventDependencies select sed);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (predecessorEventId.HasValue == true)
			{
				query = query.Where(sed => sed.predecessorEventId == predecessorEventId.Value);
			}
			if (successorEventId.HasValue == true)
			{
				query = query.Where(sed => sed.successorEventId == successorEventId.Value);
			}
			if (dependencyTypeId.HasValue == true)
			{
				query = query.Where(sed => sed.dependencyTypeId == dependencyTypeId.Value);
			}
			if (lagMinutes.HasValue == true)
			{
				query = query.Where(sed => sed.lagMinutes == lagMinutes.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(sed => sed.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sed => sed.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sed => sed.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sed => sed.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sed => sed.deleted == false);
				}
			}
			else
			{
				query = query.Where(sed => sed.active == true);
				query = query.Where(sed => sed.deleted == false);
			}

			query = query.OrderBy(sed => sed.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.dependencyType);
				query = query.Include(x => x.predecessorEvent);
				query = query.Include(x => x.successorEvent);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Scheduled Event Dependency, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       (includeRelations == true && x.dependencyType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.dependencyType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.dependencyType.color.Contains(anyStringContains))
			       || (includeRelations == true && x.predecessorEvent.name.Contains(anyStringContains))
			       || (includeRelations == true && x.predecessorEvent.description.Contains(anyStringContains))
			       || (includeRelations == true && x.predecessorEvent.location.Contains(anyStringContains))
			       || (includeRelations == true && x.predecessorEvent.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.predecessorEvent.color.Contains(anyStringContains))
			       || (includeRelations == true && x.predecessorEvent.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.predecessorEvent.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.successorEvent.name.Contains(anyStringContains))
			       || (includeRelations == true && x.successorEvent.description.Contains(anyStringContains))
			       || (includeRelations == true && x.successorEvent.location.Contains(anyStringContains))
			       || (includeRelations == true && x.successorEvent.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.successorEvent.color.Contains(anyStringContains))
			       || (includeRelations == true && x.successorEvent.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.successorEvent.attributes.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.ScheduledEventDependency> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ScheduledEventDependency scheduledEventDependency in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(scheduledEventDependency, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ScheduledEventDependency Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ScheduledEventDependency Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ScheduledEventDependencies filtered by the parameters provided.  Its query is similar to the GetScheduledEventDependencies method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventDependencies/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? predecessorEventId = null,
			int? successorEventId = null,
			int? dependencyTypeId = null,
			int? lagMinutes = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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


			IQueryable<Database.ScheduledEventDependency> query = (from sed in _context.ScheduledEventDependencies select sed);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (predecessorEventId.HasValue == true)
			{
				query = query.Where(sed => sed.predecessorEventId == predecessorEventId.Value);
			}
			if (successorEventId.HasValue == true)
			{
				query = query.Where(sed => sed.successorEventId == successorEventId.Value);
			}
			if (dependencyTypeId.HasValue == true)
			{
				query = query.Where(sed => sed.dependencyTypeId == dependencyTypeId.Value);
			}
			if (lagMinutes.HasValue == true)
			{
				query = query.Where(sed => sed.lagMinutes == lagMinutes.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(sed => sed.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sed => sed.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sed => sed.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sed => sed.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sed => sed.deleted == false);
				}
			}
			else
			{
				query = query.Where(sed => sed.active == true);
				query = query.Where(sed => sed.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Scheduled Event Dependency, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.dependencyType.name.Contains(anyStringContains)
			       || x.dependencyType.description.Contains(anyStringContains)
			       || x.dependencyType.color.Contains(anyStringContains)
			       || x.predecessorEvent.name.Contains(anyStringContains)
			       || x.predecessorEvent.description.Contains(anyStringContains)
			       || x.predecessorEvent.location.Contains(anyStringContains)
			       || x.predecessorEvent.notes.Contains(anyStringContains)
			       || x.predecessorEvent.color.Contains(anyStringContains)
			       || x.predecessorEvent.externalId.Contains(anyStringContains)
			       || x.predecessorEvent.attributes.Contains(anyStringContains)
			       || x.successorEvent.name.Contains(anyStringContains)
			       || x.successorEvent.description.Contains(anyStringContains)
			       || x.successorEvent.location.Contains(anyStringContains)
			       || x.successorEvent.notes.Contains(anyStringContains)
			       || x.successorEvent.color.Contains(anyStringContains)
			       || x.successorEvent.externalId.Contains(anyStringContains)
			       || x.successorEvent.attributes.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ScheduledEventDependency by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventDependency/{id}")]
		public async Task<IActionResult> GetScheduledEventDependency(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
				IQueryable<Database.ScheduledEventDependency> query = (from sed in _context.ScheduledEventDependencies where
							(sed.id == id) &&
							(userIsAdmin == true || sed.deleted == false) &&
							(userIsWriter == true || sed.active == true)
					select sed);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.dependencyType);
					query = query.Include(x => x.predecessorEvent);
					query = query.Include(x => x.successorEvent);
					query = query.AsSplitQuery();
				}

				Database.ScheduledEventDependency materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.ScheduledEventDependency Entity was read with Admin privilege." : "Scheduler.ScheduledEventDependency Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ScheduledEventDependency", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.ScheduledEventDependency entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.ScheduledEventDependency.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.ScheduledEventDependency.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ScheduledEventDependency record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ScheduledEventDependency/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutScheduledEventDependency(int id, [FromBody]Database.ScheduledEventDependency.ScheduledEventDependencyDTO scheduledEventDependencyDTO, CancellationToken cancellationToken = default)
		{
			if (scheduledEventDependencyDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != scheduledEventDependencyDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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


			IQueryable<Database.ScheduledEventDependency> query = (from x in _context.ScheduledEventDependencies
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ScheduledEventDependency existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ScheduledEventDependency PUT", id.ToString(), new Exception("No Scheduler.ScheduledEventDependency entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (scheduledEventDependencyDTO.objectGuid == Guid.Empty)
            {
                scheduledEventDependencyDTO.objectGuid = existing.objectGuid;
            }
            else if (scheduledEventDependencyDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ScheduledEventDependency record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ScheduledEventDependency cloneOfExisting = (Database.ScheduledEventDependency)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ScheduledEventDependency object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ScheduledEventDependency scheduledEventDependency = (Database.ScheduledEventDependency)_context.Entry(existing).GetDatabaseValues().ToObject();
			scheduledEventDependency.ApplyDTO(scheduledEventDependencyDTO);
			//
			// The tenant guid for any ScheduledEventDependency being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ScheduledEventDependency because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				scheduledEventDependency.tenantGuid = existing.tenantGuid;
			}

			lock (scheduledEventDependencyPutSyncRoot)
			{
				//
				// Validate the version number for the scheduledEventDependency being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != scheduledEventDependency.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ScheduledEventDependency save attempt was made but save request was with version " + scheduledEventDependency.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ScheduledEventDependency you are trying to update has already changed.  Please try your save again after reloading the ScheduledEventDependency.");
				}
				else
				{
					// Same record.  Increase version.
					scheduledEventDependency.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (scheduledEventDependency.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.ScheduledEventDependency record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				try
				{
				    EntityEntry<Database.ScheduledEventDependency> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(scheduledEventDependency);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ScheduledEventDependencyChangeHistory scheduledEventDependencyChangeHistory = new ScheduledEventDependencyChangeHistory();
				        scheduledEventDependencyChangeHistory.scheduledEventDependencyId = scheduledEventDependency.id;
				        scheduledEventDependencyChangeHistory.versionNumber = scheduledEventDependency.versionNumber;
				        scheduledEventDependencyChangeHistory.timeStamp = DateTime.UtcNow;
				        scheduledEventDependencyChangeHistory.userId = securityUser.id;
				        scheduledEventDependencyChangeHistory.tenantGuid = userTenantGuid;
				        scheduledEventDependencyChangeHistory.data = JsonSerializer.Serialize(Database.ScheduledEventDependency.CreateAnonymousWithFirstLevelSubObjects(scheduledEventDependency));
				        _context.ScheduledEventDependencyChangeHistories.Add(scheduledEventDependencyChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ScheduledEventDependency entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduledEventDependency.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduledEventDependency.CreateAnonymousWithFirstLevelSubObjects(scheduledEventDependency)),
						null);

				return Ok(Database.ScheduledEventDependency.CreateAnonymous(scheduledEventDependency));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ScheduledEventDependency entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduledEventDependency.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduledEventDependency.CreateAnonymousWithFirstLevelSubObjects(scheduledEventDependency)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ScheduledEventDependency record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventDependency", Name = "ScheduledEventDependency")]
		public async Task<IActionResult> PostScheduledEventDependency([FromBody]Database.ScheduledEventDependency.ScheduledEventDependencyDTO scheduledEventDependencyDTO, CancellationToken cancellationToken = default)
		{
			if (scheduledEventDependencyDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
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
			// Create a new ScheduledEventDependency object using the data from the DTO
			//
			Database.ScheduledEventDependency scheduledEventDependency = Database.ScheduledEventDependency.FromDTO(scheduledEventDependencyDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				scheduledEventDependency.tenantGuid = userTenantGuid;

				scheduledEventDependency.objectGuid = Guid.NewGuid();
				scheduledEventDependency.versionNumber = 1;

				_context.ScheduledEventDependencies.Add(scheduledEventDependency);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the scheduledEventDependency object so that no further changes will be written to the database
				    //
				    _context.Entry(scheduledEventDependency).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					scheduledEventDependency.ScheduledEventDependencyChangeHistories = null;
					scheduledEventDependency.dependencyType = null;
					scheduledEventDependency.predecessorEvent = null;
					scheduledEventDependency.successorEvent = null;


				    ScheduledEventDependencyChangeHistory scheduledEventDependencyChangeHistory = new ScheduledEventDependencyChangeHistory();
				    scheduledEventDependencyChangeHistory.scheduledEventDependencyId = scheduledEventDependency.id;
				    scheduledEventDependencyChangeHistory.versionNumber = scheduledEventDependency.versionNumber;
				    scheduledEventDependencyChangeHistory.timeStamp = DateTime.UtcNow;
				    scheduledEventDependencyChangeHistory.userId = securityUser.id;
				    scheduledEventDependencyChangeHistory.tenantGuid = userTenantGuid;
				    scheduledEventDependencyChangeHistory.data = JsonSerializer.Serialize(Database.ScheduledEventDependency.CreateAnonymousWithFirstLevelSubObjects(scheduledEventDependency));
				    _context.ScheduledEventDependencyChangeHistories.Add(scheduledEventDependencyChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.ScheduledEventDependency entity successfully created.",
						true,
						scheduledEventDependency. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ScheduledEventDependency.CreateAnonymousWithFirstLevelSubObjects(scheduledEventDependency)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.ScheduledEventDependency entity creation failed.", false, scheduledEventDependency.id.ToString(), "", JsonSerializer.Serialize(Database.ScheduledEventDependency.CreateAnonymousWithFirstLevelSubObjects(scheduledEventDependency)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ScheduledEventDependency", scheduledEventDependency.id, scheduledEventDependency.id.ToString()));

			return CreatedAtRoute("ScheduledEventDependency", new { id = scheduledEventDependency.id }, Database.ScheduledEventDependency.CreateAnonymousWithFirstLevelSubObjects(scheduledEventDependency));
		}



        /// <summary>
        /// 
        /// This rolls a ScheduledEventDependency entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventDependency/Rollback/{id}")]
		[Route("api/ScheduledEventDependency/Rollback")]
		public async Task<IActionResult> RollbackToScheduledEventDependencyVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.ScheduledEventDependency> query = (from x in _context.ScheduledEventDependencies
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this ScheduledEventDependency concurrently
			//
			lock (scheduledEventDependencyPutSyncRoot)
			{
				
				Database.ScheduledEventDependency scheduledEventDependency = query.FirstOrDefault();
				
				if (scheduledEventDependency == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ScheduledEventDependency rollback", id.ToString(), new Exception("No Scheduler.ScheduledEventDependency entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ScheduledEventDependency current state so we can log it.
				//
				Database.ScheduledEventDependency cloneOfExisting = (Database.ScheduledEventDependency)_context.Entry(scheduledEventDependency).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.ScheduledEventDependencyChangeHistories = null;
				cloneOfExisting.dependencyType = null;
				cloneOfExisting.predecessorEvent = null;
				cloneOfExisting.successorEvent = null;

				if (versionNumber >= scheduledEventDependency.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.ScheduledEventDependency rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.ScheduledEventDependency rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ScheduledEventDependencyChangeHistory scheduledEventDependencyChangeHistory = (from x in _context.ScheduledEventDependencyChangeHistories
				                                               where
				                                               x.scheduledEventDependencyId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (scheduledEventDependencyChangeHistory != null)
				{
				    Database.ScheduledEventDependency oldScheduledEventDependency = JsonSerializer.Deserialize<Database.ScheduledEventDependency>(scheduledEventDependencyChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    scheduledEventDependency.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    scheduledEventDependency.predecessorEventId = oldScheduledEventDependency.predecessorEventId;
				    scheduledEventDependency.successorEventId = oldScheduledEventDependency.successorEventId;
				    scheduledEventDependency.dependencyTypeId = oldScheduledEventDependency.dependencyTypeId;
				    scheduledEventDependency.lagMinutes = oldScheduledEventDependency.lagMinutes;
				    scheduledEventDependency.objectGuid = oldScheduledEventDependency.objectGuid;
				    scheduledEventDependency.active = oldScheduledEventDependency.active;
				    scheduledEventDependency.deleted = oldScheduledEventDependency.deleted;

				    string serializedScheduledEventDependency = JsonSerializer.Serialize(scheduledEventDependency);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ScheduledEventDependencyChangeHistory newScheduledEventDependencyChangeHistory = new ScheduledEventDependencyChangeHistory();
				        newScheduledEventDependencyChangeHistory.scheduledEventDependencyId = scheduledEventDependency.id;
				        newScheduledEventDependencyChangeHistory.versionNumber = scheduledEventDependency.versionNumber;
				        newScheduledEventDependencyChangeHistory.timeStamp = DateTime.UtcNow;
				        newScheduledEventDependencyChangeHistory.userId = securityUser.id;
				        newScheduledEventDependencyChangeHistory.tenantGuid = userTenantGuid;
				        newScheduledEventDependencyChangeHistory.data = JsonSerializer.Serialize(Database.ScheduledEventDependency.CreateAnonymousWithFirstLevelSubObjects(scheduledEventDependency));
				        _context.ScheduledEventDependencyChangeHistories.Add(newScheduledEventDependencyChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ScheduledEventDependency rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduledEventDependency.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduledEventDependency.CreateAnonymousWithFirstLevelSubObjects(scheduledEventDependency)),
						null);


				    return Ok(Database.ScheduledEventDependency.CreateAnonymous(scheduledEventDependency));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.ScheduledEventDependency rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.ScheduledEventDependency rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}


        /// <summary>
        /// 
        /// This deletes a ScheduledEventDependency record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventDependency/{id}")]
		[Route("api/ScheduledEventDependency")]
		public async Task<IActionResult> DeleteScheduledEventDependency(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
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

			IQueryable<Database.ScheduledEventDependency> query = (from x in _context.ScheduledEventDependencies
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ScheduledEventDependency scheduledEventDependency = await query.FirstOrDefaultAsync(cancellationToken);

			if (scheduledEventDependency == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ScheduledEventDependency DELETE", id.ToString(), new Exception("No Scheduler.ScheduledEventDependency entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ScheduledEventDependency cloneOfExisting = (Database.ScheduledEventDependency)_context.Entry(scheduledEventDependency).GetDatabaseValues().ToObject();


			lock (scheduledEventDependencyDeleteSyncRoot)
			{
			    try
			    {
			        scheduledEventDependency.deleted = true;
			        scheduledEventDependency.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ScheduledEventDependencyChangeHistory scheduledEventDependencyChangeHistory = new ScheduledEventDependencyChangeHistory();
			        scheduledEventDependencyChangeHistory.scheduledEventDependencyId = scheduledEventDependency.id;
			        scheduledEventDependencyChangeHistory.versionNumber = scheduledEventDependency.versionNumber;
			        scheduledEventDependencyChangeHistory.timeStamp = DateTime.UtcNow;
			        scheduledEventDependencyChangeHistory.userId = securityUser.id;
			        scheduledEventDependencyChangeHistory.tenantGuid = userTenantGuid;
			        scheduledEventDependencyChangeHistory.data = JsonSerializer.Serialize(Database.ScheduledEventDependency.CreateAnonymousWithFirstLevelSubObjects(scheduledEventDependency));
			        _context.ScheduledEventDependencyChangeHistories.Add(scheduledEventDependencyChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ScheduledEventDependency entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduledEventDependency.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduledEventDependency.CreateAnonymousWithFirstLevelSubObjects(scheduledEventDependency)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ScheduledEventDependency entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduledEventDependency.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduledEventDependency.CreateAnonymousWithFirstLevelSubObjects(scheduledEventDependency)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of ScheduledEventDependency records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ScheduledEventDependencies/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? predecessorEventId = null,
			int? successorEventId = null,
			int? dependencyTypeId = null,
			int? lagMinutes = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);

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

			IQueryable<Database.ScheduledEventDependency> query = (from sed in _context.ScheduledEventDependencies select sed);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (predecessorEventId.HasValue == true)
			{
				query = query.Where(sed => sed.predecessorEventId == predecessorEventId.Value);
			}
			if (successorEventId.HasValue == true)
			{
				query = query.Where(sed => sed.successorEventId == successorEventId.Value);
			}
			if (dependencyTypeId.HasValue == true)
			{
				query = query.Where(sed => sed.dependencyTypeId == dependencyTypeId.Value);
			}
			if (lagMinutes.HasValue == true)
			{
				query = query.Where(sed => sed.lagMinutes == lagMinutes.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(sed => sed.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sed => sed.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sed => sed.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sed => sed.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sed => sed.deleted == false);
				}
			}
			else
			{
				query = query.Where(sed => sed.active == true);
				query = query.Where(sed => sed.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Scheduled Event Dependency, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.dependencyType.name.Contains(anyStringContains)
			       || x.dependencyType.description.Contains(anyStringContains)
			       || x.dependencyType.color.Contains(anyStringContains)
			       || x.predecessorEvent.name.Contains(anyStringContains)
			       || x.predecessorEvent.description.Contains(anyStringContains)
			       || x.predecessorEvent.location.Contains(anyStringContains)
			       || x.predecessorEvent.notes.Contains(anyStringContains)
			       || x.predecessorEvent.color.Contains(anyStringContains)
			       || x.predecessorEvent.externalId.Contains(anyStringContains)
			       || x.predecessorEvent.attributes.Contains(anyStringContains)
			       || x.successorEvent.name.Contains(anyStringContains)
			       || x.successorEvent.description.Contains(anyStringContains)
			       || x.successorEvent.location.Contains(anyStringContains)
			       || x.successorEvent.notes.Contains(anyStringContains)
			       || x.successorEvent.color.Contains(anyStringContains)
			       || x.successorEvent.externalId.Contains(anyStringContains)
			       || x.successorEvent.attributes.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ScheduledEventDependency.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ScheduledEventDependency/CreateAuditEvent")]
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
