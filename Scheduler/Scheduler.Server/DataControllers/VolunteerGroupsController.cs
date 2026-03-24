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
    /// This auto generated class provides the basic CRUD operations for the VolunteerGroup entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the VolunteerGroup entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class VolunteerGroupsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 40;

		static object volunteerGroupPutSyncRoot = new object();
		static object volunteerGroupDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<VolunteerGroupsController> _logger;

		public VolunteerGroupsController(SchedulerContext context, ILogger<VolunteerGroupsController> logger) : base("Scheduler", "VolunteerGroup")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of VolunteerGroups filtered by the parameters provided.
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
		[Route("api/VolunteerGroups")]
		public async Task<IActionResult> GetVolunteerGroups(
			string name = null,
			string description = null,
			string purpose = null,
			int? officeId = null,
			int? volunteerStatusId = null,
			int? maxMembers = null,
			int? iconId = null,
			string color = null,
			string notes = null,
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

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 40, cancellationToken);
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

			IQueryable<Database.VolunteerGroup> query = (from vg in _context.VolunteerGroups select vg);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(vg => vg.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(vg => vg.description == description);
			}
			if (string.IsNullOrEmpty(purpose) == false)
			{
				query = query.Where(vg => vg.purpose == purpose);
			}
			if (officeId.HasValue == true)
			{
				query = query.Where(vg => vg.officeId == officeId.Value);
			}
			if (volunteerStatusId.HasValue == true)
			{
				query = query.Where(vg => vg.volunteerStatusId == volunteerStatusId.Value);
			}
			if (maxMembers.HasValue == true)
			{
				query = query.Where(vg => vg.maxMembers == maxMembers.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(vg => vg.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(vg => vg.color == color);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(vg => vg.notes == notes);
			}
			if (string.IsNullOrEmpty(avatarFileName) == false)
			{
				query = query.Where(vg => vg.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(vg => vg.avatarSize == avatarSize.Value);
			}
			if (string.IsNullOrEmpty(avatarMimeType) == false)
			{
				query = query.Where(vg => vg.avatarMimeType == avatarMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(vg => vg.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(vg => vg.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(vg => vg.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(vg => vg.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(vg => vg.deleted == false);
				}
			}
			else
			{
				query = query.Where(vg => vg.active == true);
				query = query.Where(vg => vg.deleted == false);
			}

			query = query.OrderBy(vg => vg.name);


			//
			// Add the any string contains parameter to span all the string fields on the Volunteer Group, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.purpose.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.avatarFileName.Contains(anyStringContains)
			       || x.avatarMimeType.Contains(anyStringContains)
			       || (includeRelations == true && x.icon.name.Contains(anyStringContains))
			       || (includeRelations == true && x.icon.fontAwesomeCode.Contains(anyStringContains))
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
			       || (includeRelations == true && x.volunteerStatus.name.Contains(anyStringContains))
			       || (includeRelations == true && x.volunteerStatus.description.Contains(anyStringContains))
			       || (includeRelations == true && x.volunteerStatus.color.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.icon);
				query = query.Include(x => x.office);
				query = query.Include(x => x.volunteerStatus);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.VolunteerGroup> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.VolunteerGroup volunteerGroup in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(volunteerGroup, databaseStoresDateWithTimeZone);
			}


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			if (diskBasedBinaryStorageMode == true)
			{
				var tasks = materialized.Select(async volunteerGroup =>
				{

					if (volunteerGroup.avatarData == null &&
					    volunteerGroup.avatarSize.HasValue == true &&
					    volunteerGroup.avatarSize.Value > 0)
					{
					    volunteerGroup.avatarData = await LoadDataFromDiskAsync(volunteerGroup.objectGuid, volunteerGroup.versionNumber, "data");
					}

				}).ToList();

				// Run tasks concurrently and await their completion
				await Task.WhenAll(tasks);
			}

			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.VolunteerGroup Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.VolunteerGroup Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of VolunteerGroups filtered by the parameters provided.  Its query is similar to the GetVolunteerGroups method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerGroups/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			string purpose = null,
			int? officeId = null,
			int? volunteerStatusId = null,
			int? maxMembers = null,
			int? iconId = null,
			string color = null,
			string notes = null,
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
			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 40, cancellationToken);
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


			IQueryable<Database.VolunteerGroup> query = (from vg in _context.VolunteerGroups select vg);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(vg => vg.name == name);
			}
			if (description != null)
			{
				query = query.Where(vg => vg.description == description);
			}
			if (purpose != null)
			{
				query = query.Where(vg => vg.purpose == purpose);
			}
			if (officeId.HasValue == true)
			{
				query = query.Where(vg => vg.officeId == officeId.Value);
			}
			if (volunteerStatusId.HasValue == true)
			{
				query = query.Where(vg => vg.volunteerStatusId == volunteerStatusId.Value);
			}
			if (maxMembers.HasValue == true)
			{
				query = query.Where(vg => vg.maxMembers == maxMembers.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(vg => vg.iconId == iconId.Value);
			}
			if (color != null)
			{
				query = query.Where(vg => vg.color == color);
			}
			if (notes != null)
			{
				query = query.Where(vg => vg.notes == notes);
			}
			if (avatarFileName != null)
			{
				query = query.Where(vg => vg.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(vg => vg.avatarSize == avatarSize.Value);
			}
			if (avatarMimeType != null)
			{
				query = query.Where(vg => vg.avatarMimeType == avatarMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(vg => vg.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(vg => vg.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(vg => vg.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(vg => vg.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(vg => vg.deleted == false);
				}
			}
			else
			{
				query = query.Where(vg => vg.active == true);
				query = query.Where(vg => vg.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Volunteer Group, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.purpose.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.avatarFileName.Contains(anyStringContains)
			       || x.avatarMimeType.Contains(anyStringContains)
			       || x.icon.name.Contains(anyStringContains)
			       || x.icon.fontAwesomeCode.Contains(anyStringContains)
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
			       || x.volunteerStatus.name.Contains(anyStringContains)
			       || x.volunteerStatus.description.Contains(anyStringContains)
			       || x.volunteerStatus.color.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single VolunteerGroup by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerGroup/{id}")]
		public async Task<IActionResult> GetVolunteerGroup(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 40, cancellationToken);
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
				IQueryable<Database.VolunteerGroup> query = (from vg in _context.VolunteerGroups where
							(vg.id == id) &&
							(userIsAdmin == true || vg.deleted == false) &&
							(userIsWriter == true || vg.active == true)
					select vg);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.icon);
					query = query.Include(x => x.office);
					query = query.Include(x => x.volunteerStatus);
					query = query.AsSplitQuery();
				}

				Database.VolunteerGroup materialized = await query.FirstOrDefaultAsync(cancellationToken);

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

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.VolunteerGroup Entity was read with Admin privilege." : "Scheduler.VolunteerGroup Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "VolunteerGroup", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.VolunteerGroup entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.VolunteerGroup.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.VolunteerGroup.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing VolunteerGroup record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/VolunteerGroup/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutVolunteerGroup(int id, [FromBody]Database.VolunteerGroup.VolunteerGroupDTO volunteerGroupDTO, CancellationToken cancellationToken = default)
		{
			if (volunteerGroupDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Volunteer Manager role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Volunteer Manager", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != volunteerGroupDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 40, cancellationToken);
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


			IQueryable<Database.VolunteerGroup> query = (from x in _context.VolunteerGroups
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.VolunteerGroup existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.VolunteerGroup PUT", id.ToString(), new Exception("No Scheduler.VolunteerGroup entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (volunteerGroupDTO.objectGuid == Guid.Empty)
            {
                volunteerGroupDTO.objectGuid = existing.objectGuid;
            }
            else if (volunteerGroupDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a VolunteerGroup record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.VolunteerGroup cloneOfExisting = (Database.VolunteerGroup)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new VolunteerGroup object using the data from the existing record, updated with what is in the DTO.
			//
			Database.VolunteerGroup volunteerGroup = (Database.VolunteerGroup)_context.Entry(existing).GetDatabaseValues().ToObject();
			volunteerGroup.ApplyDTO(volunteerGroupDTO);
			//
			// The tenant guid for any VolunteerGroup being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the VolunteerGroup because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				volunteerGroup.tenantGuid = existing.tenantGuid;
			}

			lock (volunteerGroupPutSyncRoot)
			{
				//
				// Validate the version number for the volunteerGroup being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != volunteerGroup.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "VolunteerGroup save attempt was made but save request was with version " + volunteerGroup.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The VolunteerGroup you are trying to update has already changed.  Please try your save again after reloading the VolunteerGroup.");
				}
				else
				{
					// Same record.  Increase version.
					volunteerGroup.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (volunteerGroup.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.VolunteerGroup record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (volunteerGroup.name != null && volunteerGroup.name.Length > 100)
				{
					volunteerGroup.name = volunteerGroup.name.Substring(0, 100);
				}

				if (volunteerGroup.description != null && volunteerGroup.description.Length > 500)
				{
					volunteerGroup.description = volunteerGroup.description.Substring(0, 500);
				}

				if (volunteerGroup.color != null && volunteerGroup.color.Length > 10)
				{
					volunteerGroup.color = volunteerGroup.color.Substring(0, 10);
				}

				if (volunteerGroup.avatarFileName != null && volunteerGroup.avatarFileName.Length > 250)
				{
					volunteerGroup.avatarFileName = volunteerGroup.avatarFileName.Substring(0, 250);
				}

				if (volunteerGroup.avatarMimeType != null && volunteerGroup.avatarMimeType.Length > 100)
				{
					volunteerGroup.avatarMimeType = volunteerGroup.avatarMimeType.Substring(0, 100);
				}


				//
				// Add default values for any missing data attribute fields.
				//
				if (volunteerGroup.avatarData != null && string.IsNullOrEmpty(volunteerGroup.avatarFileName))
				{
				    volunteerGroup.avatarFileName = volunteerGroup.objectGuid.ToString() + ".data";
				}

				if (volunteerGroup.avatarData != null && (volunteerGroup.avatarSize.HasValue == false || volunteerGroup.avatarSize != volunteerGroup.avatarData.Length))
				{
				    volunteerGroup.avatarSize = volunteerGroup.avatarData.Length;
				}

				if (volunteerGroup.avatarData != null && string.IsNullOrEmpty(volunteerGroup.avatarMimeType))
				{
				    volunteerGroup.avatarMimeType = "application/octet-stream";
				}

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				try
				{
					byte[] dataReferenceBeforeClearing = volunteerGroup.avatarData;

					if (diskBasedBinaryStorageMode == true &&
					    volunteerGroup.avatarFileName != null &&
					    volunteerGroup.avatarData != null &&
					    volunteerGroup.avatarSize.HasValue == true &&
					    volunteerGroup.avatarSize.Value > 0)
					{
					    //
					    // write the bytes to disk
					    //
					    WriteDataToDisk(volunteerGroup.objectGuid, volunteerGroup.versionNumber, volunteerGroup.avatarData, "data");

					    //
					    // Clear the data from the object before we put it into the db
					    //
					    volunteerGroup.avatarData = null;

					}

				    EntityEntry<Database.VolunteerGroup> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(volunteerGroup);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        VolunteerGroupChangeHistory volunteerGroupChangeHistory = new VolunteerGroupChangeHistory();
				        volunteerGroupChangeHistory.volunteerGroupId = volunteerGroup.id;
				        volunteerGroupChangeHistory.versionNumber = volunteerGroup.versionNumber;
				        volunteerGroupChangeHistory.timeStamp = DateTime.UtcNow;
				        volunteerGroupChangeHistory.userId = securityUser.id;
				        volunteerGroupChangeHistory.tenantGuid = userTenantGuid;
				        volunteerGroupChangeHistory.data = JsonSerializer.Serialize(Database.VolunteerGroup.CreateAnonymousWithFirstLevelSubObjects(volunteerGroup));
				        _context.VolunteerGroupChangeHistories.Add(volunteerGroupChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    volunteerGroup.avatarData = dataReferenceBeforeClearing;
					}

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.VolunteerGroup entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.VolunteerGroup.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.VolunteerGroup.CreateAnonymousWithFirstLevelSubObjects(volunteerGroup)),
						null);

				return Ok(Database.VolunteerGroup.CreateAnonymous(volunteerGroup));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.VolunteerGroup entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.VolunteerGroup.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.VolunteerGroup.CreateAnonymousWithFirstLevelSubObjects(volunteerGroup)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new VolunteerGroup record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerGroup", Name = "VolunteerGroup")]
		public async Task<IActionResult> PostVolunteerGroup([FromBody]Database.VolunteerGroup.VolunteerGroupDTO volunteerGroupDTO, CancellationToken cancellationToken = default)
		{
			if (volunteerGroupDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Volunteer Manager role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Volunteer Manager", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new VolunteerGroup object using the data from the DTO
			//
			Database.VolunteerGroup volunteerGroup = Database.VolunteerGroup.FromDTO(volunteerGroupDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				volunteerGroup.tenantGuid = userTenantGuid;

				if (volunteerGroup.name != null && volunteerGroup.name.Length > 100)
				{
					volunteerGroup.name = volunteerGroup.name.Substring(0, 100);
				}

				if (volunteerGroup.description != null && volunteerGroup.description.Length > 500)
				{
					volunteerGroup.description = volunteerGroup.description.Substring(0, 500);
				}

				if (volunteerGroup.color != null && volunteerGroup.color.Length > 10)
				{
					volunteerGroup.color = volunteerGroup.color.Substring(0, 10);
				}

				if (volunteerGroup.avatarFileName != null && volunteerGroup.avatarFileName.Length > 250)
				{
					volunteerGroup.avatarFileName = volunteerGroup.avatarFileName.Substring(0, 250);
				}

				if (volunteerGroup.avatarMimeType != null && volunteerGroup.avatarMimeType.Length > 100)
				{
					volunteerGroup.avatarMimeType = volunteerGroup.avatarMimeType.Substring(0, 100);
				}

				volunteerGroup.objectGuid = Guid.NewGuid();

				//
				// Add default values for any missing data attribute fields.
				//
				if (volunteerGroup.avatarData != null && string.IsNullOrEmpty(volunteerGroup.avatarFileName))
				{
				    volunteerGroup.avatarFileName = volunteerGroup.objectGuid.ToString() + ".data";
				}

				if (volunteerGroup.avatarData != null && (volunteerGroup.avatarSize.HasValue == false || volunteerGroup.avatarSize != volunteerGroup.avatarData.Length))
				{
				    volunteerGroup.avatarSize = volunteerGroup.avatarData.Length;
				}

				if (volunteerGroup.avatarData != null && string.IsNullOrEmpty(volunteerGroup.avatarMimeType))
				{
				    volunteerGroup.avatarMimeType = "application/octet-stream";
				}

				volunteerGroup.versionNumber = 1;

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				byte[] dataReferenceBeforeClearing = volunteerGroup.avatarData;

				if (diskBasedBinaryStorageMode == true &&
				    volunteerGroup.avatarData != null &&
				    volunteerGroup.avatarFileName != null &&
				    volunteerGroup.avatarSize.HasValue == true &&
				    volunteerGroup.avatarSize.Value > 0)
				{
				    //
				    // write the bytes to disk
				    //
				    await WriteDataToDiskAsync(volunteerGroup.objectGuid, volunteerGroup.versionNumber, volunteerGroup.avatarData, "data", cancellationToken);

				    //
				    // Clear the data from the object before we put it into the db
				    //
				    volunteerGroup.avatarData = null;

				}

				_context.VolunteerGroups.Add(volunteerGroup);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the volunteerGroup object so that no further changes will be written to the database
				    //
				    _context.Entry(volunteerGroup).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					volunteerGroup.avatarData = null;
					volunteerGroup.EventResourceAssignments = null;
					volunteerGroup.VolunteerGroupChangeHistories = null;
					volunteerGroup.VolunteerGroupMembers = null;
					volunteerGroup.icon = null;
					volunteerGroup.office = null;
					volunteerGroup.volunteerStatus = null;


				    VolunteerGroupChangeHistory volunteerGroupChangeHistory = new VolunteerGroupChangeHistory();
				    volunteerGroupChangeHistory.volunteerGroupId = volunteerGroup.id;
				    volunteerGroupChangeHistory.versionNumber = volunteerGroup.versionNumber;
				    volunteerGroupChangeHistory.timeStamp = DateTime.UtcNow;
				    volunteerGroupChangeHistory.userId = securityUser.id;
				    volunteerGroupChangeHistory.tenantGuid = userTenantGuid;
				    volunteerGroupChangeHistory.data = JsonSerializer.Serialize(Database.VolunteerGroup.CreateAnonymousWithFirstLevelSubObjects(volunteerGroup));
				    _context.VolunteerGroupChangeHistories.Add(volunteerGroupChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.VolunteerGroup entity successfully created.",
						true,
						volunteerGroup. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.VolunteerGroup.CreateAnonymousWithFirstLevelSubObjects(volunteerGroup)),
						null);



					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    volunteerGroup.avatarData = dataReferenceBeforeClearing;
					}

				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.VolunteerGroup entity creation failed.", false, volunteerGroup.id.ToString(), "", JsonSerializer.Serialize(Database.VolunteerGroup.CreateAnonymousWithFirstLevelSubObjects(volunteerGroup)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "VolunteerGroup", volunteerGroup.id, volunteerGroup.name));

			return CreatedAtRoute("VolunteerGroup", new { id = volunteerGroup.id }, Database.VolunteerGroup.CreateAnonymousWithFirstLevelSubObjects(volunteerGroup));
		}



        /// <summary>
        /// 
        /// This rolls a VolunteerGroup entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerGroup/Rollback/{id}")]
		[Route("api/VolunteerGroup/Rollback")]
		public async Task<IActionResult> RollbackToVolunteerGroupVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.VolunteerGroup> query = (from x in _context.VolunteerGroups
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();


			//
			// Make sure nobody else is editing this VolunteerGroup concurrently
			//
			lock (volunteerGroupPutSyncRoot)
			{
				
				Database.VolunteerGroup volunteerGroup = query.FirstOrDefault();
				
				if (volunteerGroup == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.VolunteerGroup rollback", id.ToString(), new Exception("No Scheduler.VolunteerGroup entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the VolunteerGroup current state so we can log it.
				//
				Database.VolunteerGroup cloneOfExisting = (Database.VolunteerGroup)_context.Entry(volunteerGroup).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.avatarData = null;
				cloneOfExisting.EventResourceAssignments = null;
				cloneOfExisting.VolunteerGroupChangeHistories = null;
				cloneOfExisting.VolunteerGroupMembers = null;
				cloneOfExisting.icon = null;
				cloneOfExisting.office = null;
				cloneOfExisting.volunteerStatus = null;

				if (versionNumber >= volunteerGroup.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.VolunteerGroup rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.VolunteerGroup rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				VolunteerGroupChangeHistory volunteerGroupChangeHistory = (from x in _context.VolunteerGroupChangeHistories
				                                               where
				                                               x.volunteerGroupId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (volunteerGroupChangeHistory != null)
				{
				    Database.VolunteerGroup oldVolunteerGroup = JsonSerializer.Deserialize<Database.VolunteerGroup>(volunteerGroupChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    volunteerGroup.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    volunteerGroup.name = oldVolunteerGroup.name;
				    volunteerGroup.description = oldVolunteerGroup.description;
				    volunteerGroup.purpose = oldVolunteerGroup.purpose;
				    volunteerGroup.officeId = oldVolunteerGroup.officeId;
				    volunteerGroup.volunteerStatusId = oldVolunteerGroup.volunteerStatusId;
				    volunteerGroup.maxMembers = oldVolunteerGroup.maxMembers;
				    volunteerGroup.iconId = oldVolunteerGroup.iconId;
				    volunteerGroup.color = oldVolunteerGroup.color;
				    volunteerGroup.notes = oldVolunteerGroup.notes;
				    volunteerGroup.avatarFileName = oldVolunteerGroup.avatarFileName;
				    volunteerGroup.avatarSize = oldVolunteerGroup.avatarSize;
				    volunteerGroup.avatarData = oldVolunteerGroup.avatarData;
				    volunteerGroup.avatarMimeType = oldVolunteerGroup.avatarMimeType;
				    volunteerGroup.objectGuid = oldVolunteerGroup.objectGuid;
				    volunteerGroup.active = oldVolunteerGroup.active;
				    volunteerGroup.deleted = oldVolunteerGroup.deleted;
				    //
				    // If disk based binary mode is on, then we need to copy the old data file over as well.
				    //
				    if (diskBasedBinaryStorageMode == true)
				    {
				    	Byte[] binaryData = LoadDataFromDisk(oldVolunteerGroup.objectGuid, oldVolunteerGroup.versionNumber, "data");

				    	//
				    	// Write out the data as the new version
				    	//
				    	WriteDataToDisk(volunteerGroup.objectGuid, volunteerGroup.versionNumber, binaryData, "data");
				    }

				    string serializedVolunteerGroup = JsonSerializer.Serialize(volunteerGroup);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        VolunteerGroupChangeHistory newVolunteerGroupChangeHistory = new VolunteerGroupChangeHistory();
				        newVolunteerGroupChangeHistory.volunteerGroupId = volunteerGroup.id;
				        newVolunteerGroupChangeHistory.versionNumber = volunteerGroup.versionNumber;
				        newVolunteerGroupChangeHistory.timeStamp = DateTime.UtcNow;
				        newVolunteerGroupChangeHistory.userId = securityUser.id;
				        newVolunteerGroupChangeHistory.tenantGuid = userTenantGuid;
				        newVolunteerGroupChangeHistory.data = JsonSerializer.Serialize(Database.VolunteerGroup.CreateAnonymousWithFirstLevelSubObjects(volunteerGroup));
				        _context.VolunteerGroupChangeHistories.Add(newVolunteerGroupChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.VolunteerGroup rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.VolunteerGroup.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.VolunteerGroup.CreateAnonymousWithFirstLevelSubObjects(volunteerGroup)),
						null);


				    return Ok(Database.VolunteerGroup.CreateAnonymous(volunteerGroup));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.VolunteerGroup rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.VolunteerGroup rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a VolunteerGroup.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the VolunteerGroup</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerGroup/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetVolunteerGroupChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.VolunteerGroup volunteerGroup = await _context.VolunteerGroups.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (volunteerGroup == null)
			{
				return NotFound();
			}

			try
			{
				volunteerGroup.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.VolunteerGroup> versionInfo = await volunteerGroup.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a VolunteerGroup.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the VolunteerGroup</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerGroup/{id}/AuditHistory")]
		public async Task<IActionResult> GetVolunteerGroupAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.VolunteerGroup volunteerGroup = await _context.VolunteerGroups.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (volunteerGroup == null)
			{
				return NotFound();
			}

			try
			{
				volunteerGroup.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.VolunteerGroup>> versions = await volunteerGroup.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a VolunteerGroup.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the VolunteerGroup</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The VolunteerGroup object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerGroup/{id}/Version/{version}")]
		public async Task<IActionResult> GetVolunteerGroupVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.VolunteerGroup volunteerGroup = await _context.VolunteerGroups.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (volunteerGroup == null)
			{
				return NotFound();
			}

			try
			{
				volunteerGroup.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.VolunteerGroup> versionInfo = await volunteerGroup.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a VolunteerGroup at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the VolunteerGroup</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The VolunteerGroup object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerGroup/{id}/StateAtTime")]
		public async Task<IActionResult> GetVolunteerGroupStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.VolunteerGroup volunteerGroup = await _context.VolunteerGroups.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (volunteerGroup == null)
			{
				return NotFound();
			}

			try
			{
				volunteerGroup.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.VolunteerGroup> versionInfo = await volunteerGroup.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a VolunteerGroup record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerGroup/{id}")]
		[Route("api/VolunteerGroup")]
		public async Task<IActionResult> DeleteVolunteerGroup(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Volunteer Manager role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Volunteer Manager", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.VolunteerGroup> query = (from x in _context.VolunteerGroups
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.VolunteerGroup volunteerGroup = await query.FirstOrDefaultAsync(cancellationToken);

			if (volunteerGroup == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.VolunteerGroup DELETE", id.ToString(), new Exception("No Scheduler.VolunteerGroup entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.VolunteerGroup cloneOfExisting = (Database.VolunteerGroup)_context.Entry(volunteerGroup).GetDatabaseValues().ToObject();


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			lock (volunteerGroupDeleteSyncRoot)
			{
			    try
			    {
			        volunteerGroup.deleted = true;
			        volunteerGroup.versionNumber++;

			        _context.SaveChanges();

			        //
			        // If in disk based storage mode, create a copy of the disk data file for the new version.
			        //
			        if (diskBasedBinaryStorageMode == true)
			        {
			        	Byte[] binaryData = LoadDataFromDisk(volunteerGroup.objectGuid, volunteerGroup.versionNumber -1, "data");

			        	//
			        	// Write out the same data
			        	//
			        	WriteDataToDisk(volunteerGroup.objectGuid, volunteerGroup.versionNumber, binaryData, "data");
			        }

			        //
			        // Now add the change history
			        //
			        VolunteerGroupChangeHistory volunteerGroupChangeHistory = new VolunteerGroupChangeHistory();
			        volunteerGroupChangeHistory.volunteerGroupId = volunteerGroup.id;
			        volunteerGroupChangeHistory.versionNumber = volunteerGroup.versionNumber;
			        volunteerGroupChangeHistory.timeStamp = DateTime.UtcNow;
			        volunteerGroupChangeHistory.userId = securityUser.id;
			        volunteerGroupChangeHistory.tenantGuid = userTenantGuid;
			        volunteerGroupChangeHistory.data = JsonSerializer.Serialize(Database.VolunteerGroup.CreateAnonymousWithFirstLevelSubObjects(volunteerGroup));
			        _context.VolunteerGroupChangeHistories.Add(volunteerGroupChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.VolunteerGroup entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.VolunteerGroup.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.VolunteerGroup.CreateAnonymousWithFirstLevelSubObjects(volunteerGroup)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.VolunteerGroup entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.VolunteerGroup.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.VolunteerGroup.CreateAnonymousWithFirstLevelSubObjects(volunteerGroup)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of VolunteerGroup records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/VolunteerGroups/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			string purpose = null,
			int? officeId = null,
			int? volunteerStatusId = null,
			int? maxMembers = null,
			int? iconId = null,
			string color = null,
			string notes = null,
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
			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 40, cancellationToken);


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

			IQueryable<Database.VolunteerGroup> query = (from vg in _context.VolunteerGroups select vg);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(vg => vg.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(vg => vg.description == description);
			}
			if (string.IsNullOrEmpty(purpose) == false)
			{
				query = query.Where(vg => vg.purpose == purpose);
			}
			if (officeId.HasValue == true)
			{
				query = query.Where(vg => vg.officeId == officeId.Value);
			}
			if (volunteerStatusId.HasValue == true)
			{
				query = query.Where(vg => vg.volunteerStatusId == volunteerStatusId.Value);
			}
			if (maxMembers.HasValue == true)
			{
				query = query.Where(vg => vg.maxMembers == maxMembers.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(vg => vg.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(vg => vg.color == color);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(vg => vg.notes == notes);
			}
			if (string.IsNullOrEmpty(avatarFileName) == false)
			{
				query = query.Where(vg => vg.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(vg => vg.avatarSize == avatarSize.Value);
			}
			if (string.IsNullOrEmpty(avatarMimeType) == false)
			{
				query = query.Where(vg => vg.avatarMimeType == avatarMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(vg => vg.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(vg => vg.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(vg => vg.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(vg => vg.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(vg => vg.deleted == false);
				}
			}
			else
			{
				query = query.Where(vg => vg.active == true);
				query = query.Where(vg => vg.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Volunteer Group, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.purpose.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.avatarFileName.Contains(anyStringContains)
			       || x.avatarMimeType.Contains(anyStringContains)
			       || x.icon.name.Contains(anyStringContains)
			       || x.icon.fontAwesomeCode.Contains(anyStringContains)
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
			       || x.volunteerStatus.name.Contains(anyStringContains)
			       || x.volunteerStatus.description.Contains(anyStringContains)
			       || x.volunteerStatus.color.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.VolunteerGroup.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/VolunteerGroup/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Volunteer Manager role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Volunteer Manager", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


        /// <summary>
        /// 
        /// This makes a VolunteerGroup record a favourite for the current user.
		/// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/VolunteerGroup/Favourite/{id}")]
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


			IQueryable<Database.VolunteerGroup> query = (from x in _context.VolunteerGroups
			                               where x.id == id
			                               select x);


			Database.VolunteerGroup volunteerGroup = await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

			if (volunteerGroup != null)
			{
				if (string.IsNullOrEmpty(description) == true)
				{
					description = volunteerGroup.name;
				}

				//
				// Add the user favourite VolunteerGroup
				//
				await SecurityLogic.AddToUserFavouritesAsync(securityUser, "VolunteerGroup", id, description, cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'VolunteerGroup' was added for record with id of " + id + " for user " + securityUser.accountName, true);

				//
				// Return the complete list of user favourites after the addition
				//
				return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
			}
			else
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'VolunteerGroup' add request was made with an invalid id value of " + id, false);
				return BadRequest();
			}
		}


        /// <summary>
        /// 
        /// This removes a VolunteerGroup record from the current user's favourites.
        /// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/VolunteerGroup/Favourite/{id}")]
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
			// Delete the user favourite VolunteerGroup
			//
			await SecurityLogic.RemoveFromUserFavouritesAsync(securityUser, "VolunteerGroup", id, cancellationToken);

			await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'VolunteerGroup' was removed for record with id of " + id + " for user " + securityUser.accountName, true);

			//
			// Return the complete list of user favourites after the deletion
			//
			return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
		}




        [Route("api/VolunteerGroup/Data/{id:int}")]
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


            Database.VolunteerGroup volunteerGroup = await (from x in _context.VolunteerGroups where x.id == id && x.active == true && x.deleted == false select x).FirstOrDefaultAsync();
            if (volunteerGroup == null)
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

						lock (volunteerGroupPutSyncRoot)
						{
							try
							{
								using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
								{
									volunteerGroup.avatarFileName = fileName.Trim();
									volunteerGroup.avatarMimeType = mimeType;
									volunteerGroup.avatarSize = section.Body.Length;

									volunteerGroup.versionNumber++;

									if (diskBasedBinaryStorageMode == true &&
										 volunteerGroup.avatarFileName != null &&
										 volunteerGroup.avatarSize > 0)
									{
										//
										// write the bytes to disk
										//
										WriteDataToDisk(volunteerGroup.objectGuid, volunteerGroup.versionNumber, section.Body, "data");
										//
										// Clear the data from the object before we put it into the db
										//
										volunteerGroup.avatarData = null;
									}
									else
									{
										using (MemoryStream memoryStream = new MemoryStream((int)section.Body.Length))
										{
											section.Body.CopyTo(memoryStream);
											volunteerGroup.avatarData = memoryStream.ToArray();
										}
									}
									//
									// Now add the change history
									//
									VolunteerGroupChangeHistory volunteerGroupChangeHistory = new VolunteerGroupChangeHistory();
									volunteerGroupChangeHistory.volunteerGroupId = volunteerGroup.id;
									volunteerGroupChangeHistory.versionNumber = volunteerGroup.versionNumber;
									volunteerGroupChangeHistory.timeStamp = DateTime.UtcNow;
									volunteerGroupChangeHistory.userId = securityUser.id;
									volunteerGroupChangeHistory.tenantGuid = volunteerGroup.tenantGuid;
									volunteerGroupChangeHistory.data = JsonSerializer.Serialize(Database.VolunteerGroup.CreateAnonymousWithFirstLevelSubObjects(volunteerGroup));
									_context.VolunteerGroupChangeHistories.Add(volunteerGroupChangeHistory);

									_context.SaveChanges();

									transaction.Commit();

									CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "VolunteerGroup Data Uploaded with filename of " + fileName + " and with size of " + section.Body.Length, id.ToString());
								}
							}
							catch (Exception ex)
							{
								CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "VolunteerGroup Data Upload Failed.", false, id.ToString(), "", "", ex);

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
                lock (volunteerGroupPutSyncRoot)
                {
                    try
                    {
                        using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                        {
                            if (diskBasedBinaryStorageMode == true)
                            {
								DeleteDataFromDisk(volunteerGroup.objectGuid, volunteerGroup.versionNumber, "data");
                            }

                            volunteerGroup.avatarFileName = null;
                            volunteerGroup.avatarMimeType = null;
                            volunteerGroup.avatarSize = 0;
                            volunteerGroup.avatarData = null;
                            volunteerGroup.versionNumber++;


                            //
                            // Now add the change history
                            //
                            VolunteerGroupChangeHistory volunteerGroupChangeHistory = new VolunteerGroupChangeHistory();
                            volunteerGroupChangeHistory.volunteerGroupId = volunteerGroup.id;
                            volunteerGroupChangeHistory.versionNumber = volunteerGroup.versionNumber;
                            volunteerGroupChangeHistory.timeStamp = DateTime.UtcNow;
                            volunteerGroupChangeHistory.userId = securityUser.id;
                                    volunteerGroupChangeHistory.tenantGuid = volunteerGroup.tenantGuid;
                                    volunteerGroupChangeHistory.data = JsonSerializer.Serialize(Database.VolunteerGroup.CreateAnonymousWithFirstLevelSubObjects(volunteerGroup));
                            _context.VolunteerGroupChangeHistories.Add(volunteerGroupChangeHistory);

                            _context.SaveChanges();

                            transaction.Commit();

                            CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "VolunteerGroup data cleared.", id.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "VolunteerGroup data clear failed.", false, id.ToString(), "", "", ex);

                        return Problem(ex.Message);
                    }
                }
            }

            return Ok();
        }

        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/VolunteerGroup/Data/{id:int}")]
        public async Task<IActionResult> DownloadDataAsync(int id, CancellationToken cancellationToken = default)
        {

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			using (SchedulerContext context = new SchedulerContext())
            {
                //
                // Return the data to the user as though it was a file.
                //
                Database.VolunteerGroup volunteerGroup = await (from d in context.VolunteerGroups
                                                where d.id == id &&
                                                d.active == true &&
                                                d.deleted == false
                                                select d).FirstOrDefaultAsync();

                if (volunteerGroup != null && volunteerGroup.avatarData != null)
                {
                   return File(volunteerGroup.avatarData.ToArray<byte>(), volunteerGroup.avatarMimeType, volunteerGroup.avatarFileName != null ? volunteerGroup.avatarFileName.Trim() : "VolunteerGroup_" + volunteerGroup.id.ToString(), true);
                }
                else
                {
                    return BadRequest();
                }
            }
        }
	}
}
