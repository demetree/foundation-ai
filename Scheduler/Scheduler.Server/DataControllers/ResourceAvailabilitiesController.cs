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
    /// This auto generated class provides the basic CRUD operations for the ResourceAvailability entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ResourceAvailability entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ResourceAvailabilitiesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object resourceAvailabilityPutSyncRoot = new object();
		static object resourceAvailabilityDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<ResourceAvailabilitiesController> _logger;

		public ResourceAvailabilitiesController(SchedulerContext context, ILogger<ResourceAvailabilitiesController> logger) : base("Scheduler", "ResourceAvailability")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ResourceAvailabilities filtered by the parameters provided.
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
		[Route("api/ResourceAvailabilities")]
		public async Task<IActionResult> GetResourceAvailabilities(
			int? resourceId = null,
			int? timeZoneId = null,
			DateTime? startDateTime = null,
			DateTime? endDateTime = null,
			string reason = null,
			string notes = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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

			IQueryable<Database.ResourceAvailability> query = (from ra in _context.ResourceAvailabilities select ra);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (resourceId.HasValue == true)
			{
				query = query.Where(ra => ra.resourceId == resourceId.Value);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(ra => ra.timeZoneId == timeZoneId.Value);
			}
			if (startDateTime.HasValue == true)
			{
				query = query.Where(ra => ra.startDateTime == startDateTime.Value);
			}
			if (endDateTime.HasValue == true)
			{
				query = query.Where(ra => ra.endDateTime == endDateTime.Value);
			}
			if (string.IsNullOrEmpty(reason) == false)
			{
				query = query.Where(ra => ra.reason == reason);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(ra => ra.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ra => ra.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ra => ra.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ra => ra.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ra => ra.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ra => ra.deleted == false);
				}
			}
			else
			{
				query = query.Where(ra => ra.active == true);
				query = query.Where(ra => ra.deleted == false);
			}

			query = query.OrderBy(ra => ra.reason);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.resource);
				query = query.Include(x => x.timeZone);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Resource Availability, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.reason.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || (includeRelations == true && x.resource.name.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.description.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.color.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.avatarMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.name.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.description.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.ianaTimeZone.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.abbreviation.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.abbreviationDaylightSavings.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.ResourceAvailability> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ResourceAvailability resourceAvailability in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(resourceAvailability, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ResourceAvailability Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ResourceAvailability Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ResourceAvailabilities filtered by the parameters provided.  Its query is similar to the GetResourceAvailabilities method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceAvailabilities/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? resourceId = null,
			int? timeZoneId = null,
			DateTime? startDateTime = null,
			DateTime? endDateTime = null,
			string reason = null,
			string notes = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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

			IQueryable<Database.ResourceAvailability> query = (from ra in _context.ResourceAvailabilities select ra);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (resourceId.HasValue == true)
			{
				query = query.Where(ra => ra.resourceId == resourceId.Value);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(ra => ra.timeZoneId == timeZoneId.Value);
			}
			if (startDateTime.HasValue == true)
			{
				query = query.Where(ra => ra.startDateTime == startDateTime.Value);
			}
			if (endDateTime.HasValue == true)
			{
				query = query.Where(ra => ra.endDateTime == endDateTime.Value);
			}
			if (reason != null)
			{
				query = query.Where(ra => ra.reason == reason);
			}
			if (notes != null)
			{
				query = query.Where(ra => ra.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ra => ra.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ra => ra.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ra => ra.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ra => ra.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ra => ra.deleted == false);
				}
			}
			else
			{
				query = query.Where(ra => ra.active == true);
				query = query.Where(ra => ra.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Resource Availability, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.reason.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.resource.name.Contains(anyStringContains)
			       || x.resource.description.Contains(anyStringContains)
			       || x.resource.notes.Contains(anyStringContains)
			       || x.resource.externalId.Contains(anyStringContains)
			       || x.resource.color.Contains(anyStringContains)
			       || x.resource.attributes.Contains(anyStringContains)
			       || x.resource.avatarFileName.Contains(anyStringContains)
			       || x.resource.avatarMimeType.Contains(anyStringContains)
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
        /// This gets a single ResourceAvailability by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceAvailability/{id}")]
		public async Task<IActionResult> GetResourceAvailability(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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
				IQueryable<Database.ResourceAvailability> query = (from ra in _context.ResourceAvailabilities where
							(ra.id == id) &&
							(userIsAdmin == true || ra.deleted == false) &&
							(userIsWriter == true || ra.active == true)
					select ra);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.resource);
					query = query.Include(x => x.timeZone);
					query = query.AsSplitQuery();
				}

				Database.ResourceAvailability materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.ResourceAvailability Entity was read with Admin privilege." : "Scheduler.ResourceAvailability Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ResourceAvailability", materialized.id, materialized.reason));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.ResourceAvailability entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.ResourceAvailability.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.ResourceAvailability.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ResourceAvailability record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ResourceAvailability/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutResourceAvailability(int id, [FromBody]Database.ResourceAvailability.ResourceAvailabilityDTO resourceAvailabilityDTO, CancellationToken cancellationToken = default)
		{
			if (resourceAvailabilityDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != resourceAvailabilityDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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


			IQueryable<Database.ResourceAvailability> query = (from x in _context.ResourceAvailabilities
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ResourceAvailability existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ResourceAvailability PUT", id.ToString(), new Exception("No Scheduler.ResourceAvailability entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (resourceAvailabilityDTO.objectGuid == Guid.Empty)
            {
                resourceAvailabilityDTO.objectGuid = existing.objectGuid;
            }
            else if (resourceAvailabilityDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ResourceAvailability record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ResourceAvailability cloneOfExisting = (Database.ResourceAvailability)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ResourceAvailability object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ResourceAvailability resourceAvailability = (Database.ResourceAvailability)_context.Entry(existing).GetDatabaseValues().ToObject();
			resourceAvailability.ApplyDTO(resourceAvailabilityDTO);
			//
			// The tenant guid for any ResourceAvailability being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ResourceAvailability because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				resourceAvailability.tenantGuid = existing.tenantGuid;
			}

			lock (resourceAvailabilityPutSyncRoot)
			{
				//
				// Validate the version number for the resourceAvailability being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != resourceAvailability.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ResourceAvailability save attempt was made but save request was with version " + resourceAvailability.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ResourceAvailability you are trying to update has already changed.  Please try your save again after reloading the ResourceAvailability.");
				}
				else
				{
					// Same record.  Increase version.
					resourceAvailability.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (resourceAvailability.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.ResourceAvailability record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (resourceAvailability.startDateTime.Kind != DateTimeKind.Utc)
				{
					resourceAvailability.startDateTime = resourceAvailability.startDateTime.ToUniversalTime();
				}

				if (resourceAvailability.endDateTime.HasValue == true && resourceAvailability.endDateTime.Value.Kind != DateTimeKind.Utc)
				{
					resourceAvailability.endDateTime = resourceAvailability.endDateTime.Value.ToUniversalTime();
				}

				if (resourceAvailability.reason != null && resourceAvailability.reason.Length > 250)
				{
					resourceAvailability.reason = resourceAvailability.reason.Substring(0, 250);
				}

				try
				{
				    EntityEntry<Database.ResourceAvailability> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(resourceAvailability);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ResourceAvailabilityChangeHistory resourceAvailabilityChangeHistory = new ResourceAvailabilityChangeHistory();
				        resourceAvailabilityChangeHistory.resourceAvailabilityId = resourceAvailability.id;
				        resourceAvailabilityChangeHistory.versionNumber = resourceAvailability.versionNumber;
				        resourceAvailabilityChangeHistory.timeStamp = DateTime.UtcNow;
				        resourceAvailabilityChangeHistory.userId = securityUser.id;
				        resourceAvailabilityChangeHistory.tenantGuid = userTenantGuid;
				        resourceAvailabilityChangeHistory.data = JsonSerializer.Serialize(Database.ResourceAvailability.CreateAnonymousWithFirstLevelSubObjects(resourceAvailability));
				        _context.ResourceAvailabilityChangeHistories.Add(resourceAvailabilityChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ResourceAvailability entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ResourceAvailability.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ResourceAvailability.CreateAnonymousWithFirstLevelSubObjects(resourceAvailability)),
						null);

				return Ok(Database.ResourceAvailability.CreateAnonymous(resourceAvailability));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ResourceAvailability entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ResourceAvailability.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ResourceAvailability.CreateAnonymousWithFirstLevelSubObjects(resourceAvailability)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ResourceAvailability record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceAvailability", Name = "ResourceAvailability")]
		public async Task<IActionResult> PostResourceAvailability([FromBody]Database.ResourceAvailability.ResourceAvailabilityDTO resourceAvailabilityDTO, CancellationToken cancellationToken = default)
		{
			if (resourceAvailabilityDTO == null)
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
			// Create a new ResourceAvailability object using the data from the DTO
			//
			Database.ResourceAvailability resourceAvailability = Database.ResourceAvailability.FromDTO(resourceAvailabilityDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				resourceAvailability.tenantGuid = userTenantGuid;

				if (resourceAvailability.startDateTime.Kind != DateTimeKind.Utc)
				{
					resourceAvailability.startDateTime = resourceAvailability.startDateTime.ToUniversalTime();
				}

				if (resourceAvailability.endDateTime.HasValue == true && resourceAvailability.endDateTime.Value.Kind != DateTimeKind.Utc)
				{
					resourceAvailability.endDateTime = resourceAvailability.endDateTime.Value.ToUniversalTime();
				}

				if (resourceAvailability.reason != null && resourceAvailability.reason.Length > 250)
				{
					resourceAvailability.reason = resourceAvailability.reason.Substring(0, 250);
				}

				resourceAvailability.objectGuid = Guid.NewGuid();
				resourceAvailability.versionNumber = 1;

				_context.ResourceAvailabilities.Add(resourceAvailability);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the resourceAvailability object so that no further changes will be written to the database
				    //
				    _context.Entry(resourceAvailability).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					resourceAvailability.ResourceAvailabilityChangeHistories = null;
					resourceAvailability.resource = null;
					resourceAvailability.timeZone = null;


				    ResourceAvailabilityChangeHistory resourceAvailabilityChangeHistory = new ResourceAvailabilityChangeHistory();
				    resourceAvailabilityChangeHistory.resourceAvailabilityId = resourceAvailability.id;
				    resourceAvailabilityChangeHistory.versionNumber = resourceAvailability.versionNumber;
				    resourceAvailabilityChangeHistory.timeStamp = DateTime.UtcNow;
				    resourceAvailabilityChangeHistory.userId = securityUser.id;
				    resourceAvailabilityChangeHistory.tenantGuid = userTenantGuid;
				    resourceAvailabilityChangeHistory.data = JsonSerializer.Serialize(Database.ResourceAvailability.CreateAnonymousWithFirstLevelSubObjects(resourceAvailability));
				    _context.ResourceAvailabilityChangeHistories.Add(resourceAvailabilityChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.ResourceAvailability entity successfully created.",
						true,
						resourceAvailability. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ResourceAvailability.CreateAnonymousWithFirstLevelSubObjects(resourceAvailability)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.ResourceAvailability entity creation failed.", false, resourceAvailability.id.ToString(), "", JsonSerializer.Serialize(Database.ResourceAvailability.CreateAnonymousWithFirstLevelSubObjects(resourceAvailability)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ResourceAvailability", resourceAvailability.id, resourceAvailability.reason));

			return CreatedAtRoute("ResourceAvailability", new { id = resourceAvailability.id }, Database.ResourceAvailability.CreateAnonymousWithFirstLevelSubObjects(resourceAvailability));
		}



        /// <summary>
        /// 
        /// This rolls a ResourceAvailability entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceAvailability/Rollback/{id}")]
		[Route("api/ResourceAvailability/Rollback")]
		public async Task<IActionResult> RollbackToResourceAvailabilityVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.ResourceAvailability> query = (from x in _context.ResourceAvailabilities
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this ResourceAvailability concurrently
			//
			lock (resourceAvailabilityPutSyncRoot)
			{
				
				Database.ResourceAvailability resourceAvailability = query.FirstOrDefault();
				
				if (resourceAvailability == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ResourceAvailability rollback", id.ToString(), new Exception("No Scheduler.ResourceAvailability entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ResourceAvailability current state so we can log it.
				//
				Database.ResourceAvailability cloneOfExisting = (Database.ResourceAvailability)_context.Entry(resourceAvailability).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.ResourceAvailabilityChangeHistories = null;
				cloneOfExisting.resource = null;
				cloneOfExisting.timeZone = null;

				if (versionNumber >= resourceAvailability.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.ResourceAvailability rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.ResourceAvailability rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ResourceAvailabilityChangeHistory resourceAvailabilityChangeHistory = (from x in _context.ResourceAvailabilityChangeHistories
				                                               where
				                                               x.resourceAvailabilityId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (resourceAvailabilityChangeHistory != null)
				{
				    Database.ResourceAvailability oldResourceAvailability = JsonSerializer.Deserialize<Database.ResourceAvailability>(resourceAvailabilityChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    resourceAvailability.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    resourceAvailability.resourceId = oldResourceAvailability.resourceId;
				    resourceAvailability.timeZoneId = oldResourceAvailability.timeZoneId;
				    resourceAvailability.startDateTime = oldResourceAvailability.startDateTime;
				    resourceAvailability.endDateTime = oldResourceAvailability.endDateTime;
				    resourceAvailability.reason = oldResourceAvailability.reason;
				    resourceAvailability.notes = oldResourceAvailability.notes;
				    resourceAvailability.objectGuid = oldResourceAvailability.objectGuid;
				    resourceAvailability.active = oldResourceAvailability.active;
				    resourceAvailability.deleted = oldResourceAvailability.deleted;

				    string serializedResourceAvailability = JsonSerializer.Serialize(resourceAvailability);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ResourceAvailabilityChangeHistory newResourceAvailabilityChangeHistory = new ResourceAvailabilityChangeHistory();
				        newResourceAvailabilityChangeHistory.resourceAvailabilityId = resourceAvailability.id;
				        newResourceAvailabilityChangeHistory.versionNumber = resourceAvailability.versionNumber;
				        newResourceAvailabilityChangeHistory.timeStamp = DateTime.UtcNow;
				        newResourceAvailabilityChangeHistory.userId = securityUser.id;
				        newResourceAvailabilityChangeHistory.tenantGuid = userTenantGuid;
				        newResourceAvailabilityChangeHistory.data = JsonSerializer.Serialize(Database.ResourceAvailability.CreateAnonymousWithFirstLevelSubObjects(resourceAvailability));
				        _context.ResourceAvailabilityChangeHistories.Add(newResourceAvailabilityChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ResourceAvailability rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ResourceAvailability.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ResourceAvailability.CreateAnonymousWithFirstLevelSubObjects(resourceAvailability)),
						null);


				    return Ok(Database.ResourceAvailability.CreateAnonymous(resourceAvailability));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.ResourceAvailability rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.ResourceAvailability rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a ResourceAvailability.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ResourceAvailability</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceAvailability/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetResourceAvailabilityChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{
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


			Database.ResourceAvailability resourceAvailability = await _context.ResourceAvailabilities.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (resourceAvailability == null)
			{
				return NotFound();
			}

			try
			{
				resourceAvailability.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ResourceAvailability> versionInfo = await resourceAvailability.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a ResourceAvailability.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ResourceAvailability</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceAvailability/{id}/AuditHistory")]
		public async Task<IActionResult> GetResourceAvailabilityAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{
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


			Database.ResourceAvailability resourceAvailability = await _context.ResourceAvailabilities.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (resourceAvailability == null)
			{
				return NotFound();
			}

			try
			{
				resourceAvailability.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.ResourceAvailability>> versions = await resourceAvailability.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a ResourceAvailability.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ResourceAvailability</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The ResourceAvailability object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceAvailability/{id}/Version/{version}")]
		public async Task<IActionResult> GetResourceAvailabilityVersion(int id, int version, CancellationToken cancellationToken = default)
		{
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


			Database.ResourceAvailability resourceAvailability = await _context.ResourceAvailabilities.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (resourceAvailability == null)
			{
				return NotFound();
			}

			try
			{
				resourceAvailability.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ResourceAvailability> versionInfo = await resourceAvailability.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a ResourceAvailability at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ResourceAvailability</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The ResourceAvailability object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceAvailability/{id}/StateAtTime")]
		public async Task<IActionResult> GetResourceAvailabilityStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{
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


			Database.ResourceAvailability resourceAvailability = await _context.ResourceAvailabilities.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (resourceAvailability == null)
			{
				return NotFound();
			}

			try
			{
				resourceAvailability.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ResourceAvailability> versionInfo = await resourceAvailability.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a ResourceAvailability record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceAvailability/{id}")]
		[Route("api/ResourceAvailability")]
		public async Task<IActionResult> DeleteResourceAvailability(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ResourceAvailability> query = (from x in _context.ResourceAvailabilities
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ResourceAvailability resourceAvailability = await query.FirstOrDefaultAsync(cancellationToken);

			if (resourceAvailability == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ResourceAvailability DELETE", id.ToString(), new Exception("No Scheduler.ResourceAvailability entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ResourceAvailability cloneOfExisting = (Database.ResourceAvailability)_context.Entry(resourceAvailability).GetDatabaseValues().ToObject();


			lock (resourceAvailabilityDeleteSyncRoot)
			{
			    try
			    {
			        resourceAvailability.deleted = true;
			        resourceAvailability.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ResourceAvailabilityChangeHistory resourceAvailabilityChangeHistory = new ResourceAvailabilityChangeHistory();
			        resourceAvailabilityChangeHistory.resourceAvailabilityId = resourceAvailability.id;
			        resourceAvailabilityChangeHistory.versionNumber = resourceAvailability.versionNumber;
			        resourceAvailabilityChangeHistory.timeStamp = DateTime.UtcNow;
			        resourceAvailabilityChangeHistory.userId = securityUser.id;
			        resourceAvailabilityChangeHistory.tenantGuid = userTenantGuid;
			        resourceAvailabilityChangeHistory.data = JsonSerializer.Serialize(Database.ResourceAvailability.CreateAnonymousWithFirstLevelSubObjects(resourceAvailability));
			        _context.ResourceAvailabilityChangeHistories.Add(resourceAvailabilityChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ResourceAvailability entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ResourceAvailability.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ResourceAvailability.CreateAnonymousWithFirstLevelSubObjects(resourceAvailability)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ResourceAvailability entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ResourceAvailability.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ResourceAvailability.CreateAnonymousWithFirstLevelSubObjects(resourceAvailability)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of ResourceAvailability records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ResourceAvailabilities/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? resourceId = null,
			int? timeZoneId = null,
			DateTime? startDateTime = null,
			DateTime? endDateTime = null,
			string reason = null,
			string notes = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);

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

			IQueryable<Database.ResourceAvailability> query = (from ra in _context.ResourceAvailabilities select ra);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (resourceId.HasValue == true)
			{
				query = query.Where(ra => ra.resourceId == resourceId.Value);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(ra => ra.timeZoneId == timeZoneId.Value);
			}
			if (startDateTime.HasValue == true)
			{
				query = query.Where(ra => ra.startDateTime == startDateTime.Value);
			}
			if (endDateTime.HasValue == true)
			{
				query = query.Where(ra => ra.endDateTime == endDateTime.Value);
			}
			if (string.IsNullOrEmpty(reason) == false)
			{
				query = query.Where(ra => ra.reason == reason);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(ra => ra.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ra => ra.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ra => ra.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ra => ra.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ra => ra.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ra => ra.deleted == false);
				}
			}
			else
			{
				query = query.Where(ra => ra.active == true);
				query = query.Where(ra => ra.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Resource Availability, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.reason.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.resource.name.Contains(anyStringContains)
			       || x.resource.description.Contains(anyStringContains)
			       || x.resource.notes.Contains(anyStringContains)
			       || x.resource.externalId.Contains(anyStringContains)
			       || x.resource.color.Contains(anyStringContains)
			       || x.resource.attributes.Contains(anyStringContains)
			       || x.resource.avatarFileName.Contains(anyStringContains)
			       || x.resource.avatarMimeType.Contains(anyStringContains)
			       || x.timeZone.name.Contains(anyStringContains)
			       || x.timeZone.description.Contains(anyStringContains)
			       || x.timeZone.ianaTimeZone.Contains(anyStringContains)
			       || x.timeZone.abbreviation.Contains(anyStringContains)
			       || x.timeZone.abbreviationDaylightSavings.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.reason);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ResourceAvailability.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ResourceAvailability/CreateAuditEvent")]
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
