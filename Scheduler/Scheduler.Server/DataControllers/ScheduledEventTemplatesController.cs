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
    /// This auto generated class provides the basic CRUD operations for the ScheduledEventTemplate entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ScheduledEventTemplate entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ScheduledEventTemplatesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object scheduledEventTemplatePutSyncRoot = new object();
		static object scheduledEventTemplateDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<ScheduledEventTemplatesController> _logger;

		public ScheduledEventTemplatesController(SchedulerContext context, ILogger<ScheduledEventTemplatesController> logger) : base("Scheduler", "ScheduledEventTemplate")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ScheduledEventTemplates filtered by the parameters provided.
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
		[Route("api/ScheduledEventTemplates")]
		public async Task<IActionResult> GetScheduledEventTemplates(
			string name = null,
			string description = null,
			bool? defaultAllDay = null,
			int? defaultDurationMinutes = null,
			int? schedulingTargetTypeId = null,
			int? priorityId = null,
			string defaultLocationPattern = null,
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

			IQueryable<Database.ScheduledEventTemplate> query = (from set in _context.ScheduledEventTemplates select set);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(set => set.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(set => set.description == description);
			}
			if (defaultAllDay.HasValue == true)
			{
				query = query.Where(set => set.defaultAllDay == defaultAllDay.Value);
			}
			if (defaultDurationMinutes.HasValue == true)
			{
				query = query.Where(set => set.defaultDurationMinutes == defaultDurationMinutes.Value);
			}
			if (schedulingTargetTypeId.HasValue == true)
			{
				query = query.Where(set => set.schedulingTargetTypeId == schedulingTargetTypeId.Value);
			}
			if (priorityId.HasValue == true)
			{
				query = query.Where(set => set.priorityId == priorityId.Value);
			}
			if (string.IsNullOrEmpty(defaultLocationPattern) == false)
			{
				query = query.Where(set => set.defaultLocationPattern == defaultLocationPattern);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(set => set.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(set => set.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(set => set.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(set => set.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(set => set.deleted == false);
				}
			}
			else
			{
				query = query.Where(set => set.active == true);
				query = query.Where(set => set.deleted == false);
			}

			query = query.OrderBy(set => set.name).ThenBy(set => set.description).ThenBy(set => set.defaultLocationPattern);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.priority);
				query = query.Include(x => x.schedulingTargetType);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Scheduled Event Template, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.defaultLocationPattern.Contains(anyStringContains)
			       || (includeRelations == true && x.priority.name.Contains(anyStringContains))
			       || (includeRelations == true && x.priority.description.Contains(anyStringContains))
			       || (includeRelations == true && x.priority.color.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTargetType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTargetType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTargetType.color.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.ScheduledEventTemplate> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ScheduledEventTemplate scheduledEventTemplate in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(scheduledEventTemplate, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ScheduledEventTemplate Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ScheduledEventTemplate Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ScheduledEventTemplates filtered by the parameters provided.  Its query is similar to the GetScheduledEventTemplates method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplates/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			bool? defaultAllDay = null,
			int? defaultDurationMinutes = null,
			int? schedulingTargetTypeId = null,
			int? priorityId = null,
			string defaultLocationPattern = null,
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


			IQueryable<Database.ScheduledEventTemplate> query = (from set in _context.ScheduledEventTemplates select set);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(set => set.name == name);
			}
			if (description != null)
			{
				query = query.Where(set => set.description == description);
			}
			if (defaultAllDay.HasValue == true)
			{
				query = query.Where(set => set.defaultAllDay == defaultAllDay.Value);
			}
			if (defaultDurationMinutes.HasValue == true)
			{
				query = query.Where(set => set.defaultDurationMinutes == defaultDurationMinutes.Value);
			}
			if (schedulingTargetTypeId.HasValue == true)
			{
				query = query.Where(set => set.schedulingTargetTypeId == schedulingTargetTypeId.Value);
			}
			if (priorityId.HasValue == true)
			{
				query = query.Where(set => set.priorityId == priorityId.Value);
			}
			if (defaultLocationPattern != null)
			{
				query = query.Where(set => set.defaultLocationPattern == defaultLocationPattern);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(set => set.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(set => set.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(set => set.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(set => set.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(set => set.deleted == false);
				}
			}
			else
			{
				query = query.Where(set => set.active == true);
				query = query.Where(set => set.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Scheduled Event Template, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.defaultLocationPattern.Contains(anyStringContains)
			       || x.priority.name.Contains(anyStringContains)
			       || x.priority.description.Contains(anyStringContains)
			       || x.priority.color.Contains(anyStringContains)
			       || x.schedulingTargetType.name.Contains(anyStringContains)
			       || x.schedulingTargetType.description.Contains(anyStringContains)
			       || x.schedulingTargetType.color.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ScheduledEventTemplate by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplate/{id}")]
		public async Task<IActionResult> GetScheduledEventTemplate(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ScheduledEventTemplate> query = (from set in _context.ScheduledEventTemplates where
							(set.id == id) &&
							(userIsAdmin == true || set.deleted == false) &&
							(userIsWriter == true || set.active == true)
					select set);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.priority);
					query = query.Include(x => x.schedulingTargetType);
					query = query.AsSplitQuery();
				}

				Database.ScheduledEventTemplate materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.ScheduledEventTemplate Entity was read with Admin privilege." : "Scheduler.ScheduledEventTemplate Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ScheduledEventTemplate", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.ScheduledEventTemplate entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.ScheduledEventTemplate.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.ScheduledEventTemplate.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ScheduledEventTemplate record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ScheduledEventTemplate/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutScheduledEventTemplate(int id, [FromBody]Database.ScheduledEventTemplate.ScheduledEventTemplateDTO scheduledEventTemplateDTO, CancellationToken cancellationToken = default)
		{
			if (scheduledEventTemplateDTO == null)
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



			if (id != scheduledEventTemplateDTO.id)
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


			IQueryable<Database.ScheduledEventTemplate> query = (from x in _context.ScheduledEventTemplates
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ScheduledEventTemplate existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ScheduledEventTemplate PUT", id.ToString(), new Exception("No Scheduler.ScheduledEventTemplate entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (scheduledEventTemplateDTO.objectGuid == Guid.Empty)
            {
                scheduledEventTemplateDTO.objectGuid = existing.objectGuid;
            }
            else if (scheduledEventTemplateDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ScheduledEventTemplate record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ScheduledEventTemplate cloneOfExisting = (Database.ScheduledEventTemplate)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ScheduledEventTemplate object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ScheduledEventTemplate scheduledEventTemplate = (Database.ScheduledEventTemplate)_context.Entry(existing).GetDatabaseValues().ToObject();
			scheduledEventTemplate.ApplyDTO(scheduledEventTemplateDTO);
			//
			// The tenant guid for any ScheduledEventTemplate being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ScheduledEventTemplate because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				scheduledEventTemplate.tenantGuid = existing.tenantGuid;
			}

			lock (scheduledEventTemplatePutSyncRoot)
			{
				//
				// Validate the version number for the scheduledEventTemplate being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != scheduledEventTemplate.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ScheduledEventTemplate save attempt was made but save request was with version " + scheduledEventTemplate.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ScheduledEventTemplate you are trying to update has already changed.  Please try your save again after reloading the ScheduledEventTemplate.");
				}
				else
				{
					// Same record.  Increase version.
					scheduledEventTemplate.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (scheduledEventTemplate.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.ScheduledEventTemplate record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (scheduledEventTemplate.name != null && scheduledEventTemplate.name.Length > 100)
				{
					scheduledEventTemplate.name = scheduledEventTemplate.name.Substring(0, 100);
				}

				if (scheduledEventTemplate.description != null && scheduledEventTemplate.description.Length > 500)
				{
					scheduledEventTemplate.description = scheduledEventTemplate.description.Substring(0, 500);
				}

				if (scheduledEventTemplate.defaultLocationPattern != null && scheduledEventTemplate.defaultLocationPattern.Length > 250)
				{
					scheduledEventTemplate.defaultLocationPattern = scheduledEventTemplate.defaultLocationPattern.Substring(0, 250);
				}

				try
				{
				    EntityEntry<Database.ScheduledEventTemplate> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(scheduledEventTemplate);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ScheduledEventTemplateChangeHistory scheduledEventTemplateChangeHistory = new ScheduledEventTemplateChangeHistory();
				        scheduledEventTemplateChangeHistory.scheduledEventTemplateId = scheduledEventTemplate.id;
				        scheduledEventTemplateChangeHistory.versionNumber = scheduledEventTemplate.versionNumber;
				        scheduledEventTemplateChangeHistory.timeStamp = DateTime.UtcNow;
				        scheduledEventTemplateChangeHistory.userId = securityUser.id;
				        scheduledEventTemplateChangeHistory.tenantGuid = userTenantGuid;
				        scheduledEventTemplateChangeHistory.data = JsonSerializer.Serialize(Database.ScheduledEventTemplate.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplate));
				        _context.ScheduledEventTemplateChangeHistories.Add(scheduledEventTemplateChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ScheduledEventTemplate entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduledEventTemplate.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduledEventTemplate.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplate)),
						null);

				return Ok(Database.ScheduledEventTemplate.CreateAnonymous(scheduledEventTemplate));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ScheduledEventTemplate entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduledEventTemplate.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduledEventTemplate.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplate)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ScheduledEventTemplate record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplate", Name = "ScheduledEventTemplate")]
		public async Task<IActionResult> PostScheduledEventTemplate([FromBody]Database.ScheduledEventTemplate.ScheduledEventTemplateDTO scheduledEventTemplateDTO, CancellationToken cancellationToken = default)
		{
			if (scheduledEventTemplateDTO == null)
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
			// Create a new ScheduledEventTemplate object using the data from the DTO
			//
			Database.ScheduledEventTemplate scheduledEventTemplate = Database.ScheduledEventTemplate.FromDTO(scheduledEventTemplateDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				scheduledEventTemplate.tenantGuid = userTenantGuid;

				if (scheduledEventTemplate.name != null && scheduledEventTemplate.name.Length > 100)
				{
					scheduledEventTemplate.name = scheduledEventTemplate.name.Substring(0, 100);
				}

				if (scheduledEventTemplate.description != null && scheduledEventTemplate.description.Length > 500)
				{
					scheduledEventTemplate.description = scheduledEventTemplate.description.Substring(0, 500);
				}

				if (scheduledEventTemplate.defaultLocationPattern != null && scheduledEventTemplate.defaultLocationPattern.Length > 250)
				{
					scheduledEventTemplate.defaultLocationPattern = scheduledEventTemplate.defaultLocationPattern.Substring(0, 250);
				}

				scheduledEventTemplate.objectGuid = Guid.NewGuid();
				scheduledEventTemplate.versionNumber = 1;

				_context.ScheduledEventTemplates.Add(scheduledEventTemplate);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the scheduledEventTemplate object so that no further changes will be written to the database
				    //
				    _context.Entry(scheduledEventTemplate).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					scheduledEventTemplate.ScheduledEventTemplateChangeHistories = null;
					scheduledEventTemplate.ScheduledEventTemplateCharges = null;
					scheduledEventTemplate.ScheduledEventTemplateQualificationRequirements = null;
					scheduledEventTemplate.ScheduledEvents = null;
					scheduledEventTemplate.priority = null;
					scheduledEventTemplate.schedulingTargetType = null;


				    ScheduledEventTemplateChangeHistory scheduledEventTemplateChangeHistory = new ScheduledEventTemplateChangeHistory();
				    scheduledEventTemplateChangeHistory.scheduledEventTemplateId = scheduledEventTemplate.id;
				    scheduledEventTemplateChangeHistory.versionNumber = scheduledEventTemplate.versionNumber;
				    scheduledEventTemplateChangeHistory.timeStamp = DateTime.UtcNow;
				    scheduledEventTemplateChangeHistory.userId = securityUser.id;
				    scheduledEventTemplateChangeHistory.tenantGuid = userTenantGuid;
				    scheduledEventTemplateChangeHistory.data = JsonSerializer.Serialize(Database.ScheduledEventTemplate.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplate));
				    _context.ScheduledEventTemplateChangeHistories.Add(scheduledEventTemplateChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.ScheduledEventTemplate entity successfully created.",
						true,
						scheduledEventTemplate. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ScheduledEventTemplate.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplate)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.ScheduledEventTemplate entity creation failed.", false, scheduledEventTemplate.id.ToString(), "", JsonSerializer.Serialize(Database.ScheduledEventTemplate.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplate)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ScheduledEventTemplate", scheduledEventTemplate.id, scheduledEventTemplate.name));

			return CreatedAtRoute("ScheduledEventTemplate", new { id = scheduledEventTemplate.id }, Database.ScheduledEventTemplate.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplate));
		}



        /// <summary>
        /// 
        /// This rolls a ScheduledEventTemplate entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplate/Rollback/{id}")]
		[Route("api/ScheduledEventTemplate/Rollback")]
		public async Task<IActionResult> RollbackToScheduledEventTemplateVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.ScheduledEventTemplate> query = (from x in _context.ScheduledEventTemplates
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this ScheduledEventTemplate concurrently
			//
			lock (scheduledEventTemplatePutSyncRoot)
			{
				
				Database.ScheduledEventTemplate scheduledEventTemplate = query.FirstOrDefault();
				
				if (scheduledEventTemplate == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ScheduledEventTemplate rollback", id.ToString(), new Exception("No Scheduler.ScheduledEventTemplate entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ScheduledEventTemplate current state so we can log it.
				//
				Database.ScheduledEventTemplate cloneOfExisting = (Database.ScheduledEventTemplate)_context.Entry(scheduledEventTemplate).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.ScheduledEventTemplateChangeHistories = null;
				cloneOfExisting.ScheduledEventTemplateCharges = null;
				cloneOfExisting.ScheduledEventTemplateQualificationRequirements = null;
				cloneOfExisting.ScheduledEvents = null;
				cloneOfExisting.priority = null;
				cloneOfExisting.schedulingTargetType = null;

				if (versionNumber >= scheduledEventTemplate.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.ScheduledEventTemplate rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.ScheduledEventTemplate rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ScheduledEventTemplateChangeHistory scheduledEventTemplateChangeHistory = (from x in _context.ScheduledEventTemplateChangeHistories
				                                               where
				                                               x.scheduledEventTemplateId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (scheduledEventTemplateChangeHistory != null)
				{
				    Database.ScheduledEventTemplate oldScheduledEventTemplate = JsonSerializer.Deserialize<Database.ScheduledEventTemplate>(scheduledEventTemplateChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    scheduledEventTemplate.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    scheduledEventTemplate.name = oldScheduledEventTemplate.name;
				    scheduledEventTemplate.description = oldScheduledEventTemplate.description;
				    scheduledEventTemplate.defaultAllDay = oldScheduledEventTemplate.defaultAllDay;
				    scheduledEventTemplate.defaultDurationMinutes = oldScheduledEventTemplate.defaultDurationMinutes;
				    scheduledEventTemplate.schedulingTargetTypeId = oldScheduledEventTemplate.schedulingTargetTypeId;
				    scheduledEventTemplate.priorityId = oldScheduledEventTemplate.priorityId;
				    scheduledEventTemplate.defaultLocationPattern = oldScheduledEventTemplate.defaultLocationPattern;
				    scheduledEventTemplate.objectGuid = oldScheduledEventTemplate.objectGuid;
				    scheduledEventTemplate.active = oldScheduledEventTemplate.active;
				    scheduledEventTemplate.deleted = oldScheduledEventTemplate.deleted;

				    string serializedScheduledEventTemplate = JsonSerializer.Serialize(scheduledEventTemplate);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ScheduledEventTemplateChangeHistory newScheduledEventTemplateChangeHistory = new ScheduledEventTemplateChangeHistory();
				        newScheduledEventTemplateChangeHistory.scheduledEventTemplateId = scheduledEventTemplate.id;
				        newScheduledEventTemplateChangeHistory.versionNumber = scheduledEventTemplate.versionNumber;
				        newScheduledEventTemplateChangeHistory.timeStamp = DateTime.UtcNow;
				        newScheduledEventTemplateChangeHistory.userId = securityUser.id;
				        newScheduledEventTemplateChangeHistory.tenantGuid = userTenantGuid;
				        newScheduledEventTemplateChangeHistory.data = JsonSerializer.Serialize(Database.ScheduledEventTemplate.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplate));
				        _context.ScheduledEventTemplateChangeHistories.Add(newScheduledEventTemplateChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ScheduledEventTemplate rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduledEventTemplate.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduledEventTemplate.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplate)),
						null);


				    return Ok(Database.ScheduledEventTemplate.CreateAnonymous(scheduledEventTemplate));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.ScheduledEventTemplate rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.ScheduledEventTemplate rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a ScheduledEventTemplate.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduledEventTemplate</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplate/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetScheduledEventTemplateChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.ScheduledEventTemplate scheduledEventTemplate = await _context.ScheduledEventTemplates.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduledEventTemplate == null)
			{
				return NotFound();
			}

			try
			{
				scheduledEventTemplate.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ScheduledEventTemplate> versionInfo = await scheduledEventTemplate.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a ScheduledEventTemplate.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduledEventTemplate</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplate/{id}/AuditHistory")]
		public async Task<IActionResult> GetScheduledEventTemplateAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.ScheduledEventTemplate scheduledEventTemplate = await _context.ScheduledEventTemplates.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduledEventTemplate == null)
			{
				return NotFound();
			}

			try
			{
				scheduledEventTemplate.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.ScheduledEventTemplate>> versions = await scheduledEventTemplate.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a ScheduledEventTemplate.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduledEventTemplate</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The ScheduledEventTemplate object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplate/{id}/Version/{version}")]
		public async Task<IActionResult> GetScheduledEventTemplateVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.ScheduledEventTemplate scheduledEventTemplate = await _context.ScheduledEventTemplates.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduledEventTemplate == null)
			{
				return NotFound();
			}

			try
			{
				scheduledEventTemplate.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ScheduledEventTemplate> versionInfo = await scheduledEventTemplate.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a ScheduledEventTemplate at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduledEventTemplate</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The ScheduledEventTemplate object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplate/{id}/StateAtTime")]
		public async Task<IActionResult> GetScheduledEventTemplateStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.ScheduledEventTemplate scheduledEventTemplate = await _context.ScheduledEventTemplates.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduledEventTemplate == null)
			{
				return NotFound();
			}

			try
			{
				scheduledEventTemplate.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ScheduledEventTemplate> versionInfo = await scheduledEventTemplate.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a ScheduledEventTemplate record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplate/{id}")]
		[Route("api/ScheduledEventTemplate")]
		public async Task<IActionResult> DeleteScheduledEventTemplate(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ScheduledEventTemplate> query = (from x in _context.ScheduledEventTemplates
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ScheduledEventTemplate scheduledEventTemplate = await query.FirstOrDefaultAsync(cancellationToken);

			if (scheduledEventTemplate == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ScheduledEventTemplate DELETE", id.ToString(), new Exception("No Scheduler.ScheduledEventTemplate entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ScheduledEventTemplate cloneOfExisting = (Database.ScheduledEventTemplate)_context.Entry(scheduledEventTemplate).GetDatabaseValues().ToObject();


			lock (scheduledEventTemplateDeleteSyncRoot)
			{
			    try
			    {
			        scheduledEventTemplate.deleted = true;
			        scheduledEventTemplate.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ScheduledEventTemplateChangeHistory scheduledEventTemplateChangeHistory = new ScheduledEventTemplateChangeHistory();
			        scheduledEventTemplateChangeHistory.scheduledEventTemplateId = scheduledEventTemplate.id;
			        scheduledEventTemplateChangeHistory.versionNumber = scheduledEventTemplate.versionNumber;
			        scheduledEventTemplateChangeHistory.timeStamp = DateTime.UtcNow;
			        scheduledEventTemplateChangeHistory.userId = securityUser.id;
			        scheduledEventTemplateChangeHistory.tenantGuid = userTenantGuid;
			        scheduledEventTemplateChangeHistory.data = JsonSerializer.Serialize(Database.ScheduledEventTemplate.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplate));
			        _context.ScheduledEventTemplateChangeHistories.Add(scheduledEventTemplateChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ScheduledEventTemplate entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduledEventTemplate.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduledEventTemplate.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplate)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ScheduledEventTemplate entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduledEventTemplate.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduledEventTemplate.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplate)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of ScheduledEventTemplate records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ScheduledEventTemplates/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			bool? defaultAllDay = null,
			int? defaultDurationMinutes = null,
			int? schedulingTargetTypeId = null,
			int? priorityId = null,
			string defaultLocationPattern = null,
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

			IQueryable<Database.ScheduledEventTemplate> query = (from set in _context.ScheduledEventTemplates select set);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(set => set.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(set => set.description == description);
			}
			if (defaultAllDay.HasValue == true)
			{
				query = query.Where(set => set.defaultAllDay == defaultAllDay.Value);
			}
			if (defaultDurationMinutes.HasValue == true)
			{
				query = query.Where(set => set.defaultDurationMinutes == defaultDurationMinutes.Value);
			}
			if (schedulingTargetTypeId.HasValue == true)
			{
				query = query.Where(set => set.schedulingTargetTypeId == schedulingTargetTypeId.Value);
			}
			if (priorityId.HasValue == true)
			{
				query = query.Where(set => set.priorityId == priorityId.Value);
			}
			if (string.IsNullOrEmpty(defaultLocationPattern) == false)
			{
				query = query.Where(set => set.defaultLocationPattern == defaultLocationPattern);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(set => set.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(set => set.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(set => set.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(set => set.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(set => set.deleted == false);
				}
			}
			else
			{
				query = query.Where(set => set.active == true);
				query = query.Where(set => set.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Scheduled Event Template, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.defaultLocationPattern.Contains(anyStringContains)
			       || x.priority.name.Contains(anyStringContains)
			       || x.priority.description.Contains(anyStringContains)
			       || x.priority.color.Contains(anyStringContains)
			       || x.schedulingTargetType.name.Contains(anyStringContains)
			       || x.schedulingTargetType.description.Contains(anyStringContains)
			       || x.schedulingTargetType.color.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.defaultLocationPattern);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ScheduledEventTemplate.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ScheduledEventTemplate/CreateAuditEvent")]
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
        /// This makes a ScheduledEventTemplate record a favourite for the current user.
		/// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/ScheduledEventTemplate/Favourite/{id}")]
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


			IQueryable<Database.ScheduledEventTemplate> query = (from x in _context.ScheduledEventTemplates
			                               where x.id == id
			                               select x);


			Database.ScheduledEventTemplate scheduledEventTemplate = await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

			if (scheduledEventTemplate != null)
			{
				if (string.IsNullOrEmpty(description) == true)
				{
					description = scheduledEventTemplate.name;
				}

				//
				// Add the user favourite ScheduledEventTemplate
				//
				await SecurityLogic.AddToUserFavouritesAsync(securityUser, "ScheduledEventTemplate", id, description, cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'ScheduledEventTemplate' was added for record with id of " + id + " for user " + securityUser.accountName, true);

				//
				// Return the complete list of user favourites after the addition
				//
				return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
			}
			else
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'ScheduledEventTemplate' add request was made with an invalid id value of " + id, false);
				return BadRequest();
			}
		}


        /// <summary>
        /// 
        /// This removes a ScheduledEventTemplate record from the current user's favourites.
        /// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/ScheduledEventTemplate/Favourite/{id}")]
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
			// Delete the user favourite ScheduledEventTemplate
			//
			await SecurityLogic.RemoveFromUserFavouritesAsync(securityUser, "ScheduledEventTemplate", id, cancellationToken);

			await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'ScheduledEventTemplate' was removed for record with id of " + id + " for user " + securityUser.accountName, true);

			//
			// Return the complete list of user favourites after the deletion
			//
			return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
		}


	}
}
