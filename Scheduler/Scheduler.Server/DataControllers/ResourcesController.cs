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
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the Resource entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Resource entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ResourcesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object resourcePutSyncRoot = new object();
		static object resourceDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<ResourcesController> _logger;

		public ResourcesController(SchedulerContext context, ILogger<ResourcesController> logger) : base("Scheduler", "Resource")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Resources filtered by the parameters provided.
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
		[Route("api/Resources")]
		public async Task<IActionResult> GetResources(
			string name = null,
			string description = null,
			int? officeId = null,
			int? resourceTypeId = null,
			int? shiftPatternId = null,
			int? timeZoneId = null,
			float? targetWeeklyWorkHours = null,
			string notes = null,
			string externalId = null,
			string color = null,
			string attributes = null,
			string avatarFileName = null,
			long? avatarSize = null,
			string avatarMimeType = null,
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

			IQueryable<Database.Resource> query = (from r in _context.Resources select r);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(r => r.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(r => r.description == description);
			}
			if (officeId.HasValue == true)
			{
				query = query.Where(r => r.officeId == officeId.Value);
			}
			if (resourceTypeId.HasValue == true)
			{
				query = query.Where(r => r.resourceTypeId == resourceTypeId.Value);
			}
			if (shiftPatternId.HasValue == true)
			{
				query = query.Where(r => r.shiftPatternId == shiftPatternId.Value);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(r => r.timeZoneId == timeZoneId.Value);
			}
			if (targetWeeklyWorkHours.HasValue == true)
			{
				query = query.Where(r => r.targetWeeklyWorkHours == targetWeeklyWorkHours.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(r => r.notes == notes);
			}
			if (string.IsNullOrEmpty(externalId) == false)
			{
				query = query.Where(r => r.externalId == externalId);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(r => r.color == color);
			}
			if (string.IsNullOrEmpty(attributes) == false)
			{
				query = query.Where(r => r.attributes == attributes);
			}
			if (string.IsNullOrEmpty(avatarFileName) == false)
			{
				query = query.Where(r => r.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(r => r.avatarSize == avatarSize.Value);
			}
			if (string.IsNullOrEmpty(avatarMimeType) == false)
			{
				query = query.Where(r => r.avatarMimeType == avatarMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(r => r.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(r => r.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(r => r.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(r => r.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(r => r.deleted == false);
				}
			}
			else
			{
				query = query.Where(r => r.active == true);
				query = query.Where(r => r.deleted == false);
			}

			query = query.OrderBy(r => r.name);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.office);
				query = query.Include(x => x.resourceType);
				query = query.Include(x => x.shiftPattern);
				query = query.Include(x => x.timeZone);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Resource, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.attributes.Contains(anyStringContains)
			       || x.avatarFileName.Contains(anyStringContains)
			       || x.avatarMimeType.Contains(anyStringContains)
			       || (includeRelations == true && x.office.name.Contains(anyStringContains))
			       || (includeRelations == true && x.office.description.Contains(anyStringContains))
			       || (includeRelations == true && x.office.addressLine1.Contains(anyStringContains))
			       || (includeRelations == true && x.office.addressLine2.Contains(anyStringContains))
			       || (includeRelations == true && x.office.city.Contains(anyStringContains))
			       || (includeRelations == true && x.office.postalCode.Contains(anyStringContains))
			       || (includeRelations == true && x.office.phone.Contains(anyStringContains))
			       || (includeRelations == true && x.office.email.Contains(anyStringContains))
			       || (includeRelations == true && x.office.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.office.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.office.color.Contains(anyStringContains))
			       || (includeRelations == true && x.office.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.office.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.office.avatarMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.resourceType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.resourceType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.resourceType.color.Contains(anyStringContains))
			       || (includeRelations == true && x.shiftPattern.name.Contains(anyStringContains))
			       || (includeRelations == true && x.shiftPattern.description.Contains(anyStringContains))
			       || (includeRelations == true && x.shiftPattern.color.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.name.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.description.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.ianaTimeZone.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.abbreviation.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.abbreviationDaylightSavings.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.Resource> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Resource resource in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(resource, databaseStoresDateWithTimeZone);
			}


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			if (diskBasedBinaryStorageMode == true)
			{
				var tasks = materialized.Select(async resource =>
				{

					if (resource.avatarData == null &&
					    resource.avatarSize.HasValue == true &&
					    resource.avatarSize.Value > 0)
					{
					    resource.avatarData = await LoadDataFromDiskAsync(resource.objectGuid, resource.versionNumber, "data");
					}

				}).ToList();

				// Run tasks concurrently and await their completion
				await Task.WhenAll(tasks);
			}

			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.Resource Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.Resource Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Resources filtered by the parameters provided.  Its query is similar to the GetResources method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Resources/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			int? officeId = null,
			int? resourceTypeId = null,
			int? shiftPatternId = null,
			int? timeZoneId = null,
			float? targetWeeklyWorkHours = null,
			string notes = null,
			string externalId = null,
			string color = null,
			string attributes = null,
			string avatarFileName = null,
			long? avatarSize = null,
			string avatarMimeType = null,
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


			IQueryable<Database.Resource> query = (from r in _context.Resources select r);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(r => r.name == name);
			}
			if (description != null)
			{
				query = query.Where(r => r.description == description);
			}
			if (officeId.HasValue == true)
			{
				query = query.Where(r => r.officeId == officeId.Value);
			}
			if (resourceTypeId.HasValue == true)
			{
				query = query.Where(r => r.resourceTypeId == resourceTypeId.Value);
			}
			if (shiftPatternId.HasValue == true)
			{
				query = query.Where(r => r.shiftPatternId == shiftPatternId.Value);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(r => r.timeZoneId == timeZoneId.Value);
			}
			if (targetWeeklyWorkHours.HasValue == true)
			{
				query = query.Where(r => r.targetWeeklyWorkHours == targetWeeklyWorkHours.Value);
			}
			if (notes != null)
			{
				query = query.Where(r => r.notes == notes);
			}
			if (externalId != null)
			{
				query = query.Where(r => r.externalId == externalId);
			}
			if (color != null)
			{
				query = query.Where(r => r.color == color);
			}
			if (attributes != null)
			{
				query = query.Where(r => r.attributes == attributes);
			}
			if (avatarFileName != null)
			{
				query = query.Where(r => r.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(r => r.avatarSize == avatarSize.Value);
			}
			if (avatarMimeType != null)
			{
				query = query.Where(r => r.avatarMimeType == avatarMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(r => r.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(r => r.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(r => r.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(r => r.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(r => r.deleted == false);
				}
			}
			else
			{
				query = query.Where(r => r.active == true);
				query = query.Where(r => r.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Resource, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.attributes.Contains(anyStringContains)
			       || x.avatarFileName.Contains(anyStringContains)
			       || x.avatarMimeType.Contains(anyStringContains)
			       || x.office.name.Contains(anyStringContains)
			       || x.office.description.Contains(anyStringContains)
			       || x.office.addressLine1.Contains(anyStringContains)
			       || x.office.addressLine2.Contains(anyStringContains)
			       || x.office.city.Contains(anyStringContains)
			       || x.office.postalCode.Contains(anyStringContains)
			       || x.office.phone.Contains(anyStringContains)
			       || x.office.email.Contains(anyStringContains)
			       || x.office.notes.Contains(anyStringContains)
			       || x.office.externalId.Contains(anyStringContains)
			       || x.office.color.Contains(anyStringContains)
			       || x.office.attributes.Contains(anyStringContains)
			       || x.office.avatarFileName.Contains(anyStringContains)
			       || x.office.avatarMimeType.Contains(anyStringContains)
			       || x.resourceType.name.Contains(anyStringContains)
			       || x.resourceType.description.Contains(anyStringContains)
			       || x.resourceType.color.Contains(anyStringContains)
			       || x.shiftPattern.name.Contains(anyStringContains)
			       || x.shiftPattern.description.Contains(anyStringContains)
			       || x.shiftPattern.color.Contains(anyStringContains)
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
        /// This gets a single Resource by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Resource/{id}")]
		public async Task<IActionResult> GetResource(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.Resource> query = (from r in _context.Resources where
							(r.id == id) &&
							(userIsAdmin == true || r.deleted == false) &&
							(userIsWriter == true || r.active == true)
					select r);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.office);
					query = query.Include(x => x.resourceType);
					query = query.Include(x => x.shiftPattern);
					query = query.Include(x => x.timeZone);
					query = query.AsSplitQuery();
				}

				Database.Resource materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

					if (diskBasedBinaryStorageMode == true &&
					    materialized.avatarData == null &&
					    materialized.avatarSize.HasValue == true &&
					    materialized.avatarSize.Value > 0)
					{
					    materialized.avatarData = await LoadDataFromDiskAsync(materialized.objectGuid, materialized.versionNumber, "data", cancellationToken);
					}

					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.Resource Entity was read with Admin privilege." : "Scheduler.Resource Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Resource", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.Resource entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.Resource.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.Resource.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing Resource record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Resource/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		[RequestSizeLimit(5000000)]
		public async Task<IActionResult> PutResource(int id, [FromBody]Database.Resource.ResourceDTO resourceDTO, CancellationToken cancellationToken = default)
		{
			if (resourceDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != resourceDTO.id)
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


			IQueryable<Database.Resource> query = (from x in _context.Resources
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Resource existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Resource PUT", id.ToString(), new Exception("No Scheduler.Resource entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (resourceDTO.objectGuid == Guid.Empty)
            {
                resourceDTO.objectGuid = existing.objectGuid;
            }
            else if (resourceDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Resource record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Resource cloneOfExisting = (Database.Resource)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Resource object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Resource resource = (Database.Resource)_context.Entry(existing).GetDatabaseValues().ToObject();
			resource.ApplyDTO(resourceDTO);
			//
			// The tenant guid for any Resource being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the Resource because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				resource.tenantGuid = existing.tenantGuid;
			}

			lock (resourcePutSyncRoot)
			{
				//
				// Validate the version number for the resource being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != resource.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Resource save attempt was made but save request was with version " + resource.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The Resource you are trying to update has already changed.  Please try your save again after reloading the Resource.");
				}
				else
				{
					// Same record.  Increase version.
					resource.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (resource.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.Resource record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (resource.name != null && resource.name.Length > 100)
				{
					resource.name = resource.name.Substring(0, 100);
				}

				if (resource.description != null && resource.description.Length > 500)
				{
					resource.description = resource.description.Substring(0, 500);
				}

				if (resource.externalId != null && resource.externalId.Length > 100)
				{
					resource.externalId = resource.externalId.Substring(0, 100);
				}

				if (resource.color != null && resource.color.Length > 10)
				{
					resource.color = resource.color.Substring(0, 10);
				}

				if (resource.avatarFileName != null && resource.avatarFileName.Length > 250)
				{
					resource.avatarFileName = resource.avatarFileName.Substring(0, 250);
				}

				if (resource.avatarMimeType != null && resource.avatarMimeType.Length > 100)
				{
					resource.avatarMimeType = resource.avatarMimeType.Substring(0, 100);
				}


				//
				// Add default values for any missing data attribute fields.
				//
				if (resource.avatarData != null && string.IsNullOrEmpty(resource.avatarFileName))
				{
				    resource.avatarFileName = resource.objectGuid.ToString() + ".data";
				}

				if (resource.avatarData != null && (resource.avatarSize.HasValue == false || resource.avatarSize != resource.avatarData.Length))
				{
				    resource.avatarSize = resource.avatarData.Length;
				}

				if (resource.avatarData != null && string.IsNullOrEmpty(resource.avatarMimeType))
				{
				    resource.avatarMimeType = "application/octet-stream";
				}

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				try
				{
					byte[] dataReferenceBeforeClearing = resource.avatarData;

					if (diskBasedBinaryStorageMode == true &&
					    resource.avatarFileName != null &&
					    resource.avatarData != null &&
					    resource.avatarSize.HasValue == true &&
					    resource.avatarSize.Value > 0)
					{
					    //
					    // write the bytes to disk
					    //
					    WriteDataToDisk(resource.objectGuid, resource.versionNumber, resource.avatarData, "data");

					    //
					    // Clear the data from the object before we put it into the db
					    //
					    resource.avatarData = null;

					}

				    EntityEntry<Database.Resource> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(resource);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ResourceChangeHistory resourceChangeHistory = new ResourceChangeHistory();
				        resourceChangeHistory.resourceId = resource.id;
				        resourceChangeHistory.versionNumber = resource.versionNumber;
				        resourceChangeHistory.timeStamp = DateTime.UtcNow;
				        resourceChangeHistory.userId = securityUser.id;
				        resourceChangeHistory.tenantGuid = userTenantGuid;
				        resourceChangeHistory.data = JsonSerializer.Serialize(Database.Resource.CreateAnonymousWithFirstLevelSubObjects(resource));
				        _context.ResourceChangeHistories.Add(resourceChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    resource.avatarData = dataReferenceBeforeClearing;
					}

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Resource entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Resource.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Resource.CreateAnonymousWithFirstLevelSubObjects(resource)),
						null);

				return Ok(Database.Resource.CreateAnonymous(resource));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Resource entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Resource.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Resource.CreateAnonymousWithFirstLevelSubObjects(resource)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new Resource record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Resource", Name = "Resource")]
		[RequestSizeLimit(5000000)]
		public async Task<IActionResult> PostResource([FromBody]Database.Resource.ResourceDTO resourceDTO, CancellationToken cancellationToken = default)
		{
			if (resourceDTO == null)
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
			// Create a new Resource object using the data from the DTO
			//
			Database.Resource resource = Database.Resource.FromDTO(resourceDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				resource.tenantGuid = userTenantGuid;

				if (resource.name != null && resource.name.Length > 100)
				{
					resource.name = resource.name.Substring(0, 100);
				}

				if (resource.description != null && resource.description.Length > 500)
				{
					resource.description = resource.description.Substring(0, 500);
				}

				if (resource.externalId != null && resource.externalId.Length > 100)
				{
					resource.externalId = resource.externalId.Substring(0, 100);
				}

				if (resource.color != null && resource.color.Length > 10)
				{
					resource.color = resource.color.Substring(0, 10);
				}

				if (resource.avatarFileName != null && resource.avatarFileName.Length > 250)
				{
					resource.avatarFileName = resource.avatarFileName.Substring(0, 250);
				}

				if (resource.avatarMimeType != null && resource.avatarMimeType.Length > 100)
				{
					resource.avatarMimeType = resource.avatarMimeType.Substring(0, 100);
				}

				resource.objectGuid = Guid.NewGuid();

				//
				// Add default values for any missing data attribute fields.
				//
				if (resource.avatarData != null && string.IsNullOrEmpty(resource.avatarFileName))
				{
				    resource.avatarFileName = resource.objectGuid.ToString() + ".data";
				}

				if (resource.avatarData != null && (resource.avatarSize.HasValue == false || resource.avatarSize != resource.avatarData.Length))
				{
				    resource.avatarSize = resource.avatarData.Length;
				}

				if (resource.avatarData != null && string.IsNullOrEmpty(resource.avatarMimeType))
				{
				    resource.avatarMimeType = "application/octet-stream";
				}

				resource.versionNumber = 1;

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				byte[] dataReferenceBeforeClearing = resource.avatarData;

				if (diskBasedBinaryStorageMode == true &&
				    resource.avatarData != null &&
				    resource.avatarFileName != null &&
				    resource.avatarSize.HasValue == true &&
				    resource.avatarSize.Value > 0)
				{
				    //
				    // write the bytes to disk
				    //
				    await WriteDataToDiskAsync(resource.objectGuid, resource.versionNumber, resource.avatarData, "data", cancellationToken);

				    //
				    // Clear the data from the object before we put it into the db
				    //
				    resource.avatarData = null;

				}

				_context.Resources.Add(resource);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the resource object so that no further changes will be written to the database
				    //
				    _context.Entry(resource).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					resource.avatarData = null;
					resource.CrewMembers = null;
					resource.EventCharges = null;
					resource.EventResourceAssignments = null;
					resource.NotificationSubscriptions = null;
					resource.RateSheets = null;
					resource.ResourceAvailabilities = null;
					resource.ResourceChangeHistories = null;
					resource.ResourceContacts = null;
					resource.ResourceQualifications = null;
					resource.ResourceShifts = null;
					resource.ScheduledEvents = null;
					resource.office = null;
					resource.resourceType = null;
					resource.shiftPattern = null;
					resource.timeZone = null;


				    ResourceChangeHistory resourceChangeHistory = new ResourceChangeHistory();
				    resourceChangeHistory.resourceId = resource.id;
				    resourceChangeHistory.versionNumber = resource.versionNumber;
				    resourceChangeHistory.timeStamp = DateTime.UtcNow;
				    resourceChangeHistory.userId = securityUser.id;
				    resourceChangeHistory.tenantGuid = userTenantGuid;
				    resourceChangeHistory.data = JsonSerializer.Serialize(Database.Resource.CreateAnonymousWithFirstLevelSubObjects(resource));
				    _context.ResourceChangeHistories.Add(resourceChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.Resource entity successfully created.",
						true,
						resource. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.Resource.CreateAnonymousWithFirstLevelSubObjects(resource)),
						null);



					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    resource.avatarData = dataReferenceBeforeClearing;
					}

				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.Resource entity creation failed.", false, resource.id.ToString(), "", JsonSerializer.Serialize(Database.Resource.CreateAnonymousWithFirstLevelSubObjects(resource)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Resource", resource.id, resource.name));

			return CreatedAtRoute("Resource", new { id = resource.id }, Database.Resource.CreateAnonymousWithFirstLevelSubObjects(resource));
		}



        /// <summary>
        /// 
        /// This rolls a Resource entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Resource/Rollback/{id}")]
		[Route("api/Resource/Rollback")]
		public async Task<IActionResult> RollbackToResourceVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.Resource> query = (from x in _context.Resources
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();


			//
			// Make sure nobody else is editing this Resource concurrently
			//
			lock (resourcePutSyncRoot)
			{
				
				Database.Resource resource = query.FirstOrDefault();
				
				if (resource == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Resource rollback", id.ToString(), new Exception("No Scheduler.Resource entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the Resource current state so we can log it.
				//
				Database.Resource cloneOfExisting = (Database.Resource)_context.Entry(resource).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.avatarData = null;
				cloneOfExisting.CrewMembers = null;
				cloneOfExisting.EventCharges = null;
				cloneOfExisting.EventResourceAssignments = null;
				cloneOfExisting.NotificationSubscriptions = null;
				cloneOfExisting.RateSheets = null;
				cloneOfExisting.ResourceAvailabilities = null;
				cloneOfExisting.ResourceChangeHistories = null;
				cloneOfExisting.ResourceContacts = null;
				cloneOfExisting.ResourceQualifications = null;
				cloneOfExisting.ResourceShifts = null;
				cloneOfExisting.ScheduledEvents = null;
				cloneOfExisting.office = null;
				cloneOfExisting.resourceType = null;
				cloneOfExisting.shiftPattern = null;
				cloneOfExisting.timeZone = null;

				if (versionNumber >= resource.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.Resource rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.Resource rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ResourceChangeHistory resourceChangeHistory = (from x in _context.ResourceChangeHistories
				                                               where
				                                               x.resourceId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (resourceChangeHistory != null)
				{
				    Database.Resource oldResource = JsonSerializer.Deserialize<Database.Resource>(resourceChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    resource.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    resource.name = oldResource.name;
				    resource.description = oldResource.description;
				    resource.officeId = oldResource.officeId;
				    resource.resourceTypeId = oldResource.resourceTypeId;
				    resource.shiftPatternId = oldResource.shiftPatternId;
				    resource.timeZoneId = oldResource.timeZoneId;
				    resource.targetWeeklyWorkHours = oldResource.targetWeeklyWorkHours;
				    resource.notes = oldResource.notes;
				    resource.externalId = oldResource.externalId;
				    resource.color = oldResource.color;
				    resource.attributes = oldResource.attributes;
				    resource.avatarFileName = oldResource.avatarFileName;
				    resource.avatarSize = oldResource.avatarSize;
				    resource.avatarData = oldResource.avatarData;
				    resource.avatarMimeType = oldResource.avatarMimeType;
				    resource.objectGuid = oldResource.objectGuid;
				    resource.active = oldResource.active;
				    resource.deleted = oldResource.deleted;
				    //
				    // If disk based binary mode is on, then we need to copy the old data file over as well.
				    //
				    if (diskBasedBinaryStorageMode == true)
				    {
				    	Byte[] binaryData = LoadDataFromDisk(oldResource.objectGuid, oldResource.versionNumber, "data");

				    	//
				    	// Write out the data as the new version
				    	//
				    	WriteDataToDisk(resource.objectGuid, resource.versionNumber, binaryData, "data");
				    }

				    string serializedResource = JsonSerializer.Serialize(resource);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ResourceChangeHistory newResourceChangeHistory = new ResourceChangeHistory();
				        newResourceChangeHistory.resourceId = resource.id;
				        newResourceChangeHistory.versionNumber = resource.versionNumber;
				        newResourceChangeHistory.timeStamp = DateTime.UtcNow;
				        newResourceChangeHistory.userId = securityUser.id;
				        newResourceChangeHistory.tenantGuid = userTenantGuid;
				        newResourceChangeHistory.data = JsonSerializer.Serialize(Database.Resource.CreateAnonymousWithFirstLevelSubObjects(resource));
				        _context.ResourceChangeHistories.Add(newResourceChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Resource rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Resource.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Resource.CreateAnonymousWithFirstLevelSubObjects(resource)),
						null);


				    return Ok(Database.Resource.CreateAnonymous(resource));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.Resource rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.Resource rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a Resource.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Resource</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Resource/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetResourceChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.Resource resource = await _context.Resources.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (resource == null)
			{
				return NotFound();
			}

			try
			{
				resource.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Resource> versionInfo = await resource.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a Resource.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Resource</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Resource/{id}/AuditHistory")]
		public async Task<IActionResult> GetResourceAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.Resource resource = await _context.Resources.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (resource == null)
			{
				return NotFound();
			}

			try
			{
				resource.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.Resource>> versions = await resource.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a Resource.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Resource</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The Resource object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Resource/{id}/Version/{version}")]
		public async Task<IActionResult> GetResourceVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.Resource resource = await _context.Resources.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (resource == null)
			{
				return NotFound();
			}

			try
			{
				resource.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Resource> versionInfo = await resource.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a Resource at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Resource</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The Resource object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Resource/{id}/StateAtTime")]
		public async Task<IActionResult> GetResourceStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.Resource resource = await _context.Resources.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (resource == null)
			{
				return NotFound();
			}

			try
			{
				resource.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Resource> versionInfo = await resource.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a Resource record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Resource/{id}")]
		[Route("api/Resource")]
		public async Task<IActionResult> DeleteResource(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.Resource> query = (from x in _context.Resources
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Resource resource = await query.FirstOrDefaultAsync(cancellationToken);

			if (resource == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Resource DELETE", id.ToString(), new Exception("No Scheduler.Resource entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Resource cloneOfExisting = (Database.Resource)_context.Entry(resource).GetDatabaseValues().ToObject();


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			lock (resourceDeleteSyncRoot)
			{
			    try
			    {
			        resource.deleted = true;
			        resource.versionNumber++;

			        _context.SaveChanges();

			        //
			        // If in disk based storage mode, create a copy of the disk data file for the new version.
			        //
			        if (diskBasedBinaryStorageMode == true)
			        {
			        	Byte[] binaryData = LoadDataFromDisk(resource.objectGuid, resource.versionNumber -1, "data");

			        	//
			        	// Write out the same data
			        	//
			        	WriteDataToDisk(resource.objectGuid, resource.versionNumber, binaryData, "data");
			        }

			        //
			        // Now add the change history
			        //
			        ResourceChangeHistory resourceChangeHistory = new ResourceChangeHistory();
			        resourceChangeHistory.resourceId = resource.id;
			        resourceChangeHistory.versionNumber = resource.versionNumber;
			        resourceChangeHistory.timeStamp = DateTime.UtcNow;
			        resourceChangeHistory.userId = securityUser.id;
			        resourceChangeHistory.tenantGuid = userTenantGuid;
			        resourceChangeHistory.data = JsonSerializer.Serialize(Database.Resource.CreateAnonymousWithFirstLevelSubObjects(resource));
			        _context.ResourceChangeHistories.Add(resourceChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Resource entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Resource.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Resource.CreateAnonymousWithFirstLevelSubObjects(resource)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Resource entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Resource.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Resource.CreateAnonymousWithFirstLevelSubObjects(resource)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of Resource records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Resources/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			int? officeId = null,
			int? resourceTypeId = null,
			int? shiftPatternId = null,
			int? timeZoneId = null,
			float? targetWeeklyWorkHours = null,
			string notes = null,
			string externalId = null,
			string color = null,
			string attributes = null,
			string avatarFileName = null,
			long? avatarSize = null,
			string avatarMimeType = null,
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

			IQueryable<Database.Resource> query = (from r in _context.Resources select r);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(r => r.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(r => r.description == description);
			}
			if (officeId.HasValue == true)
			{
				query = query.Where(r => r.officeId == officeId.Value);
			}
			if (resourceTypeId.HasValue == true)
			{
				query = query.Where(r => r.resourceTypeId == resourceTypeId.Value);
			}
			if (shiftPatternId.HasValue == true)
			{
				query = query.Where(r => r.shiftPatternId == shiftPatternId.Value);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(r => r.timeZoneId == timeZoneId.Value);
			}
			if (targetWeeklyWorkHours.HasValue == true)
			{
				query = query.Where(r => r.targetWeeklyWorkHours == targetWeeklyWorkHours.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(r => r.notes == notes);
			}
			if (string.IsNullOrEmpty(externalId) == false)
			{
				query = query.Where(r => r.externalId == externalId);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(r => r.color == color);
			}
			if (string.IsNullOrEmpty(attributes) == false)
			{
				query = query.Where(r => r.attributes == attributes);
			}
			if (string.IsNullOrEmpty(avatarFileName) == false)
			{
				query = query.Where(r => r.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(r => r.avatarSize == avatarSize.Value);
			}
			if (string.IsNullOrEmpty(avatarMimeType) == false)
			{
				query = query.Where(r => r.avatarMimeType == avatarMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(r => r.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(r => r.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(r => r.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(r => r.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(r => r.deleted == false);
				}
			}
			else
			{
				query = query.Where(r => r.active == true);
				query = query.Where(r => r.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Resource, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.attributes.Contains(anyStringContains)
			       || x.avatarFileName.Contains(anyStringContains)
			       || x.avatarMimeType.Contains(anyStringContains)
			       || x.office.name.Contains(anyStringContains)
			       || x.office.description.Contains(anyStringContains)
			       || x.office.addressLine1.Contains(anyStringContains)
			       || x.office.addressLine2.Contains(anyStringContains)
			       || x.office.city.Contains(anyStringContains)
			       || x.office.postalCode.Contains(anyStringContains)
			       || x.office.phone.Contains(anyStringContains)
			       || x.office.email.Contains(anyStringContains)
			       || x.office.notes.Contains(anyStringContains)
			       || x.office.externalId.Contains(anyStringContains)
			       || x.office.color.Contains(anyStringContains)
			       || x.office.attributes.Contains(anyStringContains)
			       || x.office.avatarFileName.Contains(anyStringContains)
			       || x.office.avatarMimeType.Contains(anyStringContains)
			       || x.resourceType.name.Contains(anyStringContains)
			       || x.resourceType.description.Contains(anyStringContains)
			       || x.resourceType.color.Contains(anyStringContains)
			       || x.shiftPattern.name.Contains(anyStringContains)
			       || x.shiftPattern.description.Contains(anyStringContains)
			       || x.shiftPattern.color.Contains(anyStringContains)
			       || x.timeZone.name.Contains(anyStringContains)
			       || x.timeZone.description.Contains(anyStringContains)
			       || x.timeZone.ianaTimeZone.Contains(anyStringContains)
			       || x.timeZone.abbreviation.Contains(anyStringContains)
			       || x.timeZone.abbreviationDaylightSavings.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Resource.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Resource/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
			{
			   return Forbid();
			}

		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


        /// <summary>
        /// 
        /// This makes a Resource record a favourite for the current user.
		/// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/Resource/Favourite/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPut]
		public async Task<IActionResult> SetFavourite(int id, string description = null, CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(cancellationToken);
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


			IQueryable<Database.Resource> query = (from x in _context.Resources
			                               where x.id == id
			                               select x);


			Database.Resource resource = await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

			if (resource != null)
			{
				if (string.IsNullOrEmpty(description) == true)
				{
					description = resource.name;
				}

				//
				// Add the user favourite Resource
				//
				await SecurityLogic.AddToUserFavouritesAsync(securityUser, "Resource", id, description, cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'Resource' was added for record with id of " + id + " for user " + securityUser.accountName, true);

				//
				// Return the complete list of user favourites after the addition
				//
				return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
			}
			else
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'Resource' add request was made with an invalid id value of " + id, false);
				return BadRequest();
			}
		}


        /// <summary>
        /// 
        /// This removes a Resource record from the current user's favourites.
        /// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/Resource/Favourite/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpDelete]
		public async Task<IActionResult> DeleteFavourite(int id, CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
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
			// Delete the user favourite Resource
			//
			await SecurityLogic.RemoveFromUserFavouritesAsync(securityUser, "Resource", id, cancellationToken);

			await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'Resource' was removed for record with id of " + id + " for user " + securityUser.accountName, true);

			//
			// Return the complete list of user favourites after the deletion
			//
			return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
		}




        [Route("api/Resource/Data/{id:int}")]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [HttpPost]
        [HttpPut]
        public async Task<IActionResult> UploadData(int id, CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			MediaTypeHeaderValue mediaTypeHeader; 

            if (!HttpContext.Request.HasFormContentType ||
				!MediaTypeHeaderValue.TryParse(HttpContext.Request.ContentType, out mediaTypeHeader) ||
                string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
            {
                return new UnsupportedMediaTypeResult();
            }


            Database.Resource resource = await (from x in _context.Resources where x.id == id && x.active == true && x.deleted == false select x).FirstOrDefaultAsync();
            if (resource == null)
            {
                return NotFound();
            }

            bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();


            // This will be used to signal whether we are saving data or clearing it.
            bool foundFileData = false;


            //
            // This will get the first file from the request and save it
            //
			try
			{
                MultipartReader reader = new MultipartReader(mediaTypeHeader.Boundary.Value, HttpContext.Request.Body);
                MultipartSection section = await reader.ReadNextSectionAsync();

                while (section != null)
				{
					ContentDispositionHeaderValue contentDisposition;

					bool hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);


					if (hasContentDispositionHeader && contentDisposition.DispositionType.Equals("form-data") &&
						!string.IsNullOrEmpty(contentDisposition.FileName.Value))
					{

						foundFileData = true;
						string fileName = contentDisposition.FileName.ToString().Trim('"');

						// default the mime type to be the one for arbitrary binary data unless we have a mime type on the content headers that tells us otherwise.
						MediaTypeHeaderValue mediaType;
						bool hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);

						string mimeType = "application/octet-stream";
						if (hasMediaTypeHeader && mediaTypeHeader.MediaType != null )
						{
							mimeType = mediaTypeHeader.MediaType.ToString();
						}

						lock (resourcePutSyncRoot)
						{
							try
							{
								using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
								{
									resource.avatarFileName = fileName.Trim();
									resource.avatarMimeType = mimeType;
									resource.avatarSize = section.Body.Length;

									resource.versionNumber++;

									if (diskBasedBinaryStorageMode == true &&
										 resource.avatarFileName != null &&
										 resource.avatarSize > 0)
									{
										//
										// write the bytes to disk
										//
										WriteDataToDisk(resource.objectGuid, resource.versionNumber, section.Body, "data");
										//
										// Clear the data from the object before we put it into the db
										//
										resource.avatarData = null;
									}
									else
									{
										using (MemoryStream memoryStream = new MemoryStream((int)section.Body.Length))
										{
											section.Body.CopyTo(memoryStream);
											resource.avatarData = memoryStream.ToArray();
										}
									}
									//
									// Now add the change history
									//
									ResourceChangeHistory resourceChangeHistory = new ResourceChangeHistory();
									resourceChangeHistory.resourceId = resource.id;
									resourceChangeHistory.versionNumber = resource.versionNumber;
									resourceChangeHistory.timeStamp = DateTime.UtcNow;
									resourceChangeHistory.userId = securityUser.id;
									resourceChangeHistory.tenantGuid = resource.tenantGuid;
									resourceChangeHistory.data = JsonSerializer.Serialize(Database.Resource.CreateAnonymousWithFirstLevelSubObjects(resource));
									_context.ResourceChangeHistories.Add(resourceChangeHistory);

									_context.SaveChanges();

									transaction.Commit();

									CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Resource Data Uploaded with filename of " + fileName + " and with size of " + section.Body.Length, id.ToString());
								}
							}
							catch (Exception ex)
							{
								CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "Resource Data Upload Failed.", false, id.ToString(), "", "", ex);

								return Problem(ex.Message);
							}
						}


						//
						// Stop looking for more files.
						//
						break;
					}

					section = await reader.ReadNextSectionAsync();
				}
            }
            catch (Exception ex)
            {
                CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Caught error in UploadData handler", id.ToString(), ex);

                return Problem(ex.Message);
            }

            //
            // Treat the situation where we have a valid ID but no file content as a request to clear the data
            //
            if (foundFileData == false)
            {
                lock (resourcePutSyncRoot)
                {
                    try
                    {
                        using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                        {
                            if (diskBasedBinaryStorageMode == true)
                            {
								DeleteDataFromDisk(resource.objectGuid, resource.versionNumber, "data");
                            }

                            resource.avatarFileName = null;
                            resource.avatarMimeType = null;
                            resource.avatarSize = 0;
                            resource.avatarData = null;
                            resource.versionNumber++;


                            //
                            // Now add the change history
                            //
                            ResourceChangeHistory resourceChangeHistory = new ResourceChangeHistory();
                            resourceChangeHistory.resourceId = resource.id;
                            resourceChangeHistory.versionNumber = resource.versionNumber;
                            resourceChangeHistory.timeStamp = DateTime.UtcNow;
                            resourceChangeHistory.userId = securityUser.id;
                                    resourceChangeHistory.tenantGuid = resource.tenantGuid;
                                    resourceChangeHistory.data = JsonSerializer.Serialize(Database.Resource.CreateAnonymousWithFirstLevelSubObjects(resource));
                            _context.ResourceChangeHistories.Add(resourceChangeHistory);

                            _context.SaveChanges();

                            transaction.Commit();

                            CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Resource data cleared.", id.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "Resource data clear failed.", false, id.ToString(), "", "", ex);

                        return Problem(ex.Message);
                    }
                }
            }

            return Ok();
        }

        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/Resource/Data/{id:int}")]
        public async Task<IActionResult> DownloadDataAsync(int id)
        {
             if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
             {
                 return Forbid();
             }


			using (SchedulerContext context = new SchedulerContext())
            {
                //
                // Return the data to the user as though it was a file.
                //
                Database.Resource resource = await (from d in context.Resources
                                                where d.id == id &&
                                                d.active == true &&
                                                d.deleted == false
                                                select d).FirstOrDefaultAsync();

                if (resource != null && resource.avatarData != null)
                {
                   return File(resource.avatarData.ToArray<byte>(), resource.avatarMimeType, resource.avatarFileName != null ? resource.avatarFileName.Trim() : "Resource_" + resource.id.ToString(), true);
                }
                else
                {
                    return BadRequest();
                }
            }
        }
	}
}
