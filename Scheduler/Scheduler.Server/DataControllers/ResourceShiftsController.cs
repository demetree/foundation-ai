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
    /// This auto generated class provides the basic CRUD operations for the ResourceShift entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ResourceShift entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ResourceShiftsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object resourceShiftPutSyncRoot = new object();
		static object resourceShiftDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<ResourceShiftsController> _logger;

		public ResourceShiftsController(SchedulerContext context, ILogger<ResourceShiftsController> logger) : base("Scheduler", "ResourceShift")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ResourceShifts filtered by the parameters provided.
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
		[Route("api/ResourceShifts")]
		public async Task<IActionResult> GetResourceShifts(
			int? resourceId = null,
			int? dayOfWeek = null,
			int? timeZoneId = null,
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

			IQueryable<Database.ResourceShift> query = (from rs in _context.ResourceShifts select rs);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (resourceId.HasValue == true)
			{
				query = query.Where(rs => rs.resourceId == resourceId.Value);
			}
			if (dayOfWeek.HasValue == true)
			{
				query = query.Where(rs => rs.dayOfWeek == dayOfWeek.Value);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(rs => rs.timeZoneId == timeZoneId.Value);
			}
			if (hours.HasValue == true)
			{
				query = query.Where(rs => rs.hours == hours.Value);
			}
			if (string.IsNullOrEmpty(label) == false)
			{
				query = query.Where(rs => rs.label == label);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(rs => rs.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rs => rs.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rs => rs.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rs => rs.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rs => rs.deleted == false);
				}
			}
			else
			{
				query = query.Where(rs => rs.active == true);
				query = query.Where(rs => rs.deleted == false);
			}

			query = query.OrderBy(rs => rs.label);

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
			// Add the any string contains parameter to span all the string fields on the Resource Shift, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.label.Contains(anyStringContains)
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
			
			List<Database.ResourceShift> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ResourceShift resourceShift in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(resourceShift, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ResourceShift Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ResourceShift Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ResourceShifts filtered by the parameters provided.  Its query is similar to the GetResourceShifts method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceShifts/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? resourceId = null,
			int? dayOfWeek = null,
			int? timeZoneId = null,
			float? hours = null,
			string label = null,
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


			IQueryable<Database.ResourceShift> query = (from rs in _context.ResourceShifts select rs);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (resourceId.HasValue == true)
			{
				query = query.Where(rs => rs.resourceId == resourceId.Value);
			}
			if (dayOfWeek.HasValue == true)
			{
				query = query.Where(rs => rs.dayOfWeek == dayOfWeek.Value);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(rs => rs.timeZoneId == timeZoneId.Value);
			}
			if (hours.HasValue == true)
			{
				query = query.Where(rs => rs.hours == hours.Value);
			}
			if (label != null)
			{
				query = query.Where(rs => rs.label == label);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(rs => rs.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rs => rs.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rs => rs.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rs => rs.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rs => rs.deleted == false);
				}
			}
			else
			{
				query = query.Where(rs => rs.active == true);
				query = query.Where(rs => rs.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Resource Shift, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.label.Contains(anyStringContains)
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
        /// This gets a single ResourceShift by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceShift/{id}")]
		public async Task<IActionResult> GetResourceShift(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ResourceShift> query = (from rs in _context.ResourceShifts where
							(rs.id == id) &&
							(userIsAdmin == true || rs.deleted == false) &&
							(userIsWriter == true || rs.active == true)
					select rs);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.resource);
					query = query.Include(x => x.timeZone);
					query = query.AsSplitQuery();
				}

				Database.ResourceShift materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.ResourceShift Entity was read with Admin privilege." : "Scheduler.ResourceShift Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ResourceShift", materialized.id, materialized.label));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.ResourceShift entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.ResourceShift.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.ResourceShift.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ResourceShift record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ResourceShift/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutResourceShift(int id, [FromBody]Database.ResourceShift.ResourceShiftDTO resourceShiftDTO, CancellationToken cancellationToken = default)
		{
			if (resourceShiftDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != resourceShiftDTO.id)
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


			IQueryable<Database.ResourceShift> query = (from x in _context.ResourceShifts
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ResourceShift existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ResourceShift PUT", id.ToString(), new Exception("No Scheduler.ResourceShift entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (resourceShiftDTO.objectGuid == Guid.Empty)
            {
                resourceShiftDTO.objectGuid = existing.objectGuid;
            }
            else if (resourceShiftDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ResourceShift record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ResourceShift cloneOfExisting = (Database.ResourceShift)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ResourceShift object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ResourceShift resourceShift = (Database.ResourceShift)_context.Entry(existing).GetDatabaseValues().ToObject();
			resourceShift.ApplyDTO(resourceShiftDTO);
			//
			// The tenant guid for any ResourceShift being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ResourceShift because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				resourceShift.tenantGuid = existing.tenantGuid;
			}

			lock (resourceShiftPutSyncRoot)
			{
				//
				// Validate the version number for the resourceShift being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != resourceShift.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ResourceShift save attempt was made but save request was with version " + resourceShift.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ResourceShift you are trying to update has already changed.  Please try your save again after reloading the ResourceShift.");
				}
				else
				{
					// Same record.  Increase version.
					resourceShift.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (resourceShift.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.ResourceShift record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (resourceShift.label != null && resourceShift.label.Length > 250)
				{
					resourceShift.label = resourceShift.label.Substring(0, 250);
				}

				try
				{
				    EntityEntry<Database.ResourceShift> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(resourceShift);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ResourceShiftChangeHistory resourceShiftChangeHistory = new ResourceShiftChangeHistory();
				        resourceShiftChangeHistory.resourceShiftId = resourceShift.id;
				        resourceShiftChangeHistory.versionNumber = resourceShift.versionNumber;
				        resourceShiftChangeHistory.timeStamp = DateTime.UtcNow;
				        resourceShiftChangeHistory.userId = securityUser.id;
				        resourceShiftChangeHistory.tenantGuid = userTenantGuid;
				        resourceShiftChangeHistory.data = JsonSerializer.Serialize(Database.ResourceShift.CreateAnonymousWithFirstLevelSubObjects(resourceShift));
				        _context.ResourceShiftChangeHistories.Add(resourceShiftChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ResourceShift entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ResourceShift.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ResourceShift.CreateAnonymousWithFirstLevelSubObjects(resourceShift)),
						null);

				return Ok(Database.ResourceShift.CreateAnonymous(resourceShift));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ResourceShift entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ResourceShift.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ResourceShift.CreateAnonymousWithFirstLevelSubObjects(resourceShift)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ResourceShift record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceShift", Name = "ResourceShift")]
		public async Task<IActionResult> PostResourceShift([FromBody]Database.ResourceShift.ResourceShiftDTO resourceShiftDTO, CancellationToken cancellationToken = default)
		{
			if (resourceShiftDTO == null)
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
			// Create a new ResourceShift object using the data from the DTO
			//
			Database.ResourceShift resourceShift = Database.ResourceShift.FromDTO(resourceShiftDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				resourceShift.tenantGuid = userTenantGuid;

				if (resourceShift.label != null && resourceShift.label.Length > 250)
				{
					resourceShift.label = resourceShift.label.Substring(0, 250);
				}

				resourceShift.objectGuid = Guid.NewGuid();
				resourceShift.versionNumber = 1;

				_context.ResourceShifts.Add(resourceShift);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the resourceShift object so that no further changes will be written to the database
				    //
				    _context.Entry(resourceShift).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					resourceShift.ResourceShiftChangeHistories = null;
					resourceShift.resource = null;
					resourceShift.timeZone = null;


				    ResourceShiftChangeHistory resourceShiftChangeHistory = new ResourceShiftChangeHistory();
				    resourceShiftChangeHistory.resourceShiftId = resourceShift.id;
				    resourceShiftChangeHistory.versionNumber = resourceShift.versionNumber;
				    resourceShiftChangeHistory.timeStamp = DateTime.UtcNow;
				    resourceShiftChangeHistory.userId = securityUser.id;
				    resourceShiftChangeHistory.tenantGuid = userTenantGuid;
				    resourceShiftChangeHistory.data = JsonSerializer.Serialize(Database.ResourceShift.CreateAnonymousWithFirstLevelSubObjects(resourceShift));
				    _context.ResourceShiftChangeHistories.Add(resourceShiftChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.ResourceShift entity successfully created.",
						true,
						resourceShift. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ResourceShift.CreateAnonymousWithFirstLevelSubObjects(resourceShift)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.ResourceShift entity creation failed.", false, resourceShift.id.ToString(), "", JsonSerializer.Serialize(Database.ResourceShift.CreateAnonymousWithFirstLevelSubObjects(resourceShift)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ResourceShift", resourceShift.id, resourceShift.label));

			return CreatedAtRoute("ResourceShift", new { id = resourceShift.id }, Database.ResourceShift.CreateAnonymousWithFirstLevelSubObjects(resourceShift));
		}



        /// <summary>
        /// 
        /// This rolls a ResourceShift entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceShift/Rollback/{id}")]
		[Route("api/ResourceShift/Rollback")]
		public async Task<IActionResult> RollbackToResourceShiftVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.ResourceShift> query = (from x in _context.ResourceShifts
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this ResourceShift concurrently
			//
			lock (resourceShiftPutSyncRoot)
			{
				
				Database.ResourceShift resourceShift = query.FirstOrDefault();
				
				if (resourceShift == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ResourceShift rollback", id.ToString(), new Exception("No Scheduler.ResourceShift entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ResourceShift current state so we can log it.
				//
				Database.ResourceShift cloneOfExisting = (Database.ResourceShift)_context.Entry(resourceShift).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.ResourceShiftChangeHistories = null;
				cloneOfExisting.resource = null;
				cloneOfExisting.timeZone = null;

				if (versionNumber >= resourceShift.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.ResourceShift rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.ResourceShift rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ResourceShiftChangeHistory resourceShiftChangeHistory = (from x in _context.ResourceShiftChangeHistories
				                                               where
				                                               x.resourceShiftId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (resourceShiftChangeHistory != null)
				{
				    Database.ResourceShift oldResourceShift = JsonSerializer.Deserialize<Database.ResourceShift>(resourceShiftChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    resourceShift.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    resourceShift.resourceId = oldResourceShift.resourceId;
				    resourceShift.dayOfWeek = oldResourceShift.dayOfWeek;
				    resourceShift.timeZoneId = oldResourceShift.timeZoneId;
				    resourceShift.startTime = oldResourceShift.startTime;
				    resourceShift.hours = oldResourceShift.hours;
				    resourceShift.label = oldResourceShift.label;
				    resourceShift.objectGuid = oldResourceShift.objectGuid;
				    resourceShift.active = oldResourceShift.active;
				    resourceShift.deleted = oldResourceShift.deleted;

				    string serializedResourceShift = JsonSerializer.Serialize(resourceShift);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ResourceShiftChangeHistory newResourceShiftChangeHistory = new ResourceShiftChangeHistory();
				        newResourceShiftChangeHistory.resourceShiftId = resourceShift.id;
				        newResourceShiftChangeHistory.versionNumber = resourceShift.versionNumber;
				        newResourceShiftChangeHistory.timeStamp = DateTime.UtcNow;
				        newResourceShiftChangeHistory.userId = securityUser.id;
				        newResourceShiftChangeHistory.tenantGuid = userTenantGuid;
				        newResourceShiftChangeHistory.data = JsonSerializer.Serialize(Database.ResourceShift.CreateAnonymousWithFirstLevelSubObjects(resourceShift));
				        _context.ResourceShiftChangeHistories.Add(newResourceShiftChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ResourceShift rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ResourceShift.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ResourceShift.CreateAnonymousWithFirstLevelSubObjects(resourceShift)),
						null);


				    return Ok(Database.ResourceShift.CreateAnonymous(resourceShift));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.ResourceShift rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.ResourceShift rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a ResourceShift.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ResourceShift</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceShift/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetResourceShiftChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.ResourceShift resourceShift = await _context.ResourceShifts.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (resourceShift == null)
			{
				return NotFound();
			}

			try
			{
				resourceShift.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ResourceShift> versionInfo = await resourceShift.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a ResourceShift.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ResourceShift</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceShift/{id}/AuditHistory")]
		public async Task<IActionResult> GetResourceShiftAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.ResourceShift resourceShift = await _context.ResourceShifts.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (resourceShift == null)
			{
				return NotFound();
			}

			try
			{
				resourceShift.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.ResourceShift>> versions = await resourceShift.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a ResourceShift.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ResourceShift</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The ResourceShift object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceShift/{id}/Version/{version}")]
		public async Task<IActionResult> GetResourceShiftVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.ResourceShift resourceShift = await _context.ResourceShifts.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (resourceShift == null)
			{
				return NotFound();
			}

			try
			{
				resourceShift.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ResourceShift> versionInfo = await resourceShift.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a ResourceShift at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ResourceShift</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The ResourceShift object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceShift/{id}/StateAtTime")]
		public async Task<IActionResult> GetResourceShiftStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.ResourceShift resourceShift = await _context.ResourceShifts.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (resourceShift == null)
			{
				return NotFound();
			}

			try
			{
				resourceShift.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ResourceShift> versionInfo = await resourceShift.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a ResourceShift record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceShift/{id}")]
		[Route("api/ResourceShift")]
		public async Task<IActionResult> DeleteResourceShift(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ResourceShift> query = (from x in _context.ResourceShifts
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ResourceShift resourceShift = await query.FirstOrDefaultAsync(cancellationToken);

			if (resourceShift == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ResourceShift DELETE", id.ToString(), new Exception("No Scheduler.ResourceShift entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ResourceShift cloneOfExisting = (Database.ResourceShift)_context.Entry(resourceShift).GetDatabaseValues().ToObject();


			lock (resourceShiftDeleteSyncRoot)
			{
			    try
			    {
			        resourceShift.deleted = true;
			        resourceShift.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ResourceShiftChangeHistory resourceShiftChangeHistory = new ResourceShiftChangeHistory();
			        resourceShiftChangeHistory.resourceShiftId = resourceShift.id;
			        resourceShiftChangeHistory.versionNumber = resourceShift.versionNumber;
			        resourceShiftChangeHistory.timeStamp = DateTime.UtcNow;
			        resourceShiftChangeHistory.userId = securityUser.id;
			        resourceShiftChangeHistory.tenantGuid = userTenantGuid;
			        resourceShiftChangeHistory.data = JsonSerializer.Serialize(Database.ResourceShift.CreateAnonymousWithFirstLevelSubObjects(resourceShift));
			        _context.ResourceShiftChangeHistories.Add(resourceShiftChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ResourceShift entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ResourceShift.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ResourceShift.CreateAnonymousWithFirstLevelSubObjects(resourceShift)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ResourceShift entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ResourceShift.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ResourceShift.CreateAnonymousWithFirstLevelSubObjects(resourceShift)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of ResourceShift records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ResourceShifts/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? resourceId = null,
			int? dayOfWeek = null,
			int? timeZoneId = null,
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

			IQueryable<Database.ResourceShift> query = (from rs in _context.ResourceShifts select rs);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (resourceId.HasValue == true)
			{
				query = query.Where(rs => rs.resourceId == resourceId.Value);
			}
			if (dayOfWeek.HasValue == true)
			{
				query = query.Where(rs => rs.dayOfWeek == dayOfWeek.Value);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(rs => rs.timeZoneId == timeZoneId.Value);
			}
			if (hours.HasValue == true)
			{
				query = query.Where(rs => rs.hours == hours.Value);
			}
			if (string.IsNullOrEmpty(label) == false)
			{
				query = query.Where(rs => rs.label == label);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(rs => rs.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rs => rs.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rs => rs.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rs => rs.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rs => rs.deleted == false);
				}
			}
			else
			{
				query = query.Where(rs => rs.active == true);
				query = query.Where(rs => rs.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Resource Shift, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.label.Contains(anyStringContains)
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


			query = query.OrderBy(x => x.label);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ResourceShift.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ResourceShift/CreateAuditEvent")]
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
