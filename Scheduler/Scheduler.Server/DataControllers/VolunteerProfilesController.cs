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
    /// This auto generated class provides the basic CRUD operations for the VolunteerProfile entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the VolunteerProfile entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class VolunteerProfilesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 40;

		static object volunteerProfilePutSyncRoot = new object();
		static object volunteerProfileDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<VolunteerProfilesController> _logger;

		public VolunteerProfilesController(SchedulerContext context, ILogger<VolunteerProfilesController> logger) : base("Scheduler", "VolunteerProfile")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of VolunteerProfiles filtered by the parameters provided.
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
		[Route("api/VolunteerProfiles")]
		public async Task<IActionResult> GetVolunteerProfiles(
			int? resourceId = null,
			int? volunteerStatusId = null,
			float? totalHoursServed = null,
			bool? backgroundCheckCompleted = null,
			bool? confidentialityAgreementSigned = null,
			string availabilityPreferences = null,
			string interestsAndSkillsNotes = null,
			string emergencyContactNotes = null,
			int? constituentId = null,
			int? iconId = null,
			string color = null,
			string attributes = null,
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

			IQueryable<Database.VolunteerProfile> query = (from vp in _context.VolunteerProfiles select vp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (resourceId.HasValue == true)
			{
				query = query.Where(vp => vp.resourceId == resourceId.Value);
			}
			if (volunteerStatusId.HasValue == true)
			{
				query = query.Where(vp => vp.volunteerStatusId == volunteerStatusId.Value);
			}
			if (totalHoursServed.HasValue == true)
			{
				query = query.Where(vp => vp.totalHoursServed == totalHoursServed.Value);
			}
			if (backgroundCheckCompleted.HasValue == true)
			{
				query = query.Where(vp => vp.backgroundCheckCompleted == backgroundCheckCompleted.Value);
			}
			if (confidentialityAgreementSigned.HasValue == true)
			{
				query = query.Where(vp => vp.confidentialityAgreementSigned == confidentialityAgreementSigned.Value);
			}
			if (string.IsNullOrEmpty(availabilityPreferences) == false)
			{
				query = query.Where(vp => vp.availabilityPreferences == availabilityPreferences);
			}
			if (string.IsNullOrEmpty(interestsAndSkillsNotes) == false)
			{
				query = query.Where(vp => vp.interestsAndSkillsNotes == interestsAndSkillsNotes);
			}
			if (string.IsNullOrEmpty(emergencyContactNotes) == false)
			{
				query = query.Where(vp => vp.emergencyContactNotes == emergencyContactNotes);
			}
			if (constituentId.HasValue == true)
			{
				query = query.Where(vp => vp.constituentId == constituentId.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(vp => vp.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(vp => vp.color == color);
			}
			if (string.IsNullOrEmpty(attributes) == false)
			{
				query = query.Where(vp => vp.attributes == attributes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(vp => vp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(vp => vp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(vp => vp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(vp => vp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(vp => vp.deleted == false);
				}
			}
			else
			{
				query = query.Where(vp => vp.active == true);
				query = query.Where(vp => vp.deleted == false);
			}

			query = query.OrderBy(vp => vp.color);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.constituent);
				query = query.Include(x => x.icon);
				query = query.Include(x => x.resource);
				query = query.Include(x => x.volunteerStatus);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Volunteer Profile, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.availabilityPreferences.Contains(anyStringContains)
			       || x.interestsAndSkillsNotes.Contains(anyStringContains)
			       || x.emergencyContactNotes.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.attributes.Contains(anyStringContains)
			       || (includeRelations == true && x.constituent.constituentNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.constituent.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.constituent.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.constituent.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.constituent.color.Contains(anyStringContains))
			       || (includeRelations == true && x.constituent.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.constituent.avatarMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.icon.name.Contains(anyStringContains))
			       || (includeRelations == true && x.icon.fontAwesomeCode.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.name.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.description.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.color.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.avatarMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.volunteerStatus.name.Contains(anyStringContains))
			       || (includeRelations == true && x.volunteerStatus.description.Contains(anyStringContains))
			       || (includeRelations == true && x.volunteerStatus.color.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.VolunteerProfile> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.VolunteerProfile volunteerProfile in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(volunteerProfile, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.VolunteerProfile Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.VolunteerProfile Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of VolunteerProfiles filtered by the parameters provided.  Its query is similar to the GetVolunteerProfiles method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerProfiles/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? resourceId = null,
			int? volunteerStatusId = null,
			float? totalHoursServed = null,
			bool? backgroundCheckCompleted = null,
			bool? confidentialityAgreementSigned = null,
			string availabilityPreferences = null,
			string interestsAndSkillsNotes = null,
			string emergencyContactNotes = null,
			int? constituentId = null,
			int? iconId = null,
			string color = null,
			string attributes = null,
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


			IQueryable<Database.VolunteerProfile> query = (from vp in _context.VolunteerProfiles select vp);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (resourceId.HasValue == true)
			{
				query = query.Where(vp => vp.resourceId == resourceId.Value);
			}
			if (volunteerStatusId.HasValue == true)
			{
				query = query.Where(vp => vp.volunteerStatusId == volunteerStatusId.Value);
			}
			if (totalHoursServed.HasValue == true)
			{
				query = query.Where(vp => vp.totalHoursServed == totalHoursServed.Value);
			}
			if (backgroundCheckCompleted.HasValue == true)
			{
				query = query.Where(vp => vp.backgroundCheckCompleted == backgroundCheckCompleted.Value);
			}
			if (confidentialityAgreementSigned.HasValue == true)
			{
				query = query.Where(vp => vp.confidentialityAgreementSigned == confidentialityAgreementSigned.Value);
			}
			if (availabilityPreferences != null)
			{
				query = query.Where(vp => vp.availabilityPreferences == availabilityPreferences);
			}
			if (interestsAndSkillsNotes != null)
			{
				query = query.Where(vp => vp.interestsAndSkillsNotes == interestsAndSkillsNotes);
			}
			if (emergencyContactNotes != null)
			{
				query = query.Where(vp => vp.emergencyContactNotes == emergencyContactNotes);
			}
			if (constituentId.HasValue == true)
			{
				query = query.Where(vp => vp.constituentId == constituentId.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(vp => vp.iconId == iconId.Value);
			}
			if (color != null)
			{
				query = query.Where(vp => vp.color == color);
			}
			if (attributes != null)
			{
				query = query.Where(vp => vp.attributes == attributes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(vp => vp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(vp => vp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(vp => vp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(vp => vp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(vp => vp.deleted == false);
				}
			}
			else
			{
				query = query.Where(vp => vp.active == true);
				query = query.Where(vp => vp.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Volunteer Profile, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.availabilityPreferences.Contains(anyStringContains)
			       || x.interestsAndSkillsNotes.Contains(anyStringContains)
			       || x.emergencyContactNotes.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.attributes.Contains(anyStringContains)
			       || x.constituent.constituentNumber.Contains(anyStringContains)
			       || x.constituent.externalId.Contains(anyStringContains)
			       || x.constituent.notes.Contains(anyStringContains)
			       || x.constituent.attributes.Contains(anyStringContains)
			       || x.constituent.color.Contains(anyStringContains)
			       || x.constituent.avatarFileName.Contains(anyStringContains)
			       || x.constituent.avatarMimeType.Contains(anyStringContains)
			       || x.icon.name.Contains(anyStringContains)
			       || x.icon.fontAwesomeCode.Contains(anyStringContains)
			       || x.resource.name.Contains(anyStringContains)
			       || x.resource.description.Contains(anyStringContains)
			       || x.resource.notes.Contains(anyStringContains)
			       || x.resource.externalId.Contains(anyStringContains)
			       || x.resource.color.Contains(anyStringContains)
			       || x.resource.attributes.Contains(anyStringContains)
			       || x.resource.avatarFileName.Contains(anyStringContains)
			       || x.resource.avatarMimeType.Contains(anyStringContains)
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
        /// This gets a single VolunteerProfile by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerProfile/{id}")]
		public async Task<IActionResult> GetVolunteerProfile(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.VolunteerProfile> query = (from vp in _context.VolunteerProfiles where
							(vp.id == id) &&
							(userIsAdmin == true || vp.deleted == false) &&
							(userIsWriter == true || vp.active == true)
					select vp);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.constituent);
					query = query.Include(x => x.icon);
					query = query.Include(x => x.resource);
					query = query.Include(x => x.volunteerStatus);
					query = query.AsSplitQuery();
				}

				Database.VolunteerProfile materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.VolunteerProfile Entity was read with Admin privilege." : "Scheduler.VolunteerProfile Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "VolunteerProfile", materialized.id, materialized.color));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.VolunteerProfile entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.VolunteerProfile.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.VolunteerProfile.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing VolunteerProfile record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/VolunteerProfile/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutVolunteerProfile(int id, [FromBody]Database.VolunteerProfile.VolunteerProfileDTO volunteerProfileDTO, CancellationToken cancellationToken = default)
		{
			if (volunteerProfileDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Volunteer Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Volunteer Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != volunteerProfileDTO.id)
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


			IQueryable<Database.VolunteerProfile> query = (from x in _context.VolunteerProfiles
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.VolunteerProfile existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.VolunteerProfile PUT", id.ToString(), new Exception("No Scheduler.VolunteerProfile entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (volunteerProfileDTO.objectGuid == Guid.Empty)
            {
                volunteerProfileDTO.objectGuid = existing.objectGuid;
            }
            else if (volunteerProfileDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a VolunteerProfile record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.VolunteerProfile cloneOfExisting = (Database.VolunteerProfile)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new VolunteerProfile object using the data from the existing record, updated with what is in the DTO.
			//
			Database.VolunteerProfile volunteerProfile = (Database.VolunteerProfile)_context.Entry(existing).GetDatabaseValues().ToObject();
			volunteerProfile.ApplyDTO(volunteerProfileDTO);
			//
			// The tenant guid for any VolunteerProfile being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the VolunteerProfile because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				volunteerProfile.tenantGuid = existing.tenantGuid;
			}

			lock (volunteerProfilePutSyncRoot)
			{
				//
				// Validate the version number for the volunteerProfile being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != volunteerProfile.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "VolunteerProfile save attempt was made but save request was with version " + volunteerProfile.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The VolunteerProfile you are trying to update has already changed.  Please try your save again after reloading the VolunteerProfile.");
				}
				else
				{
					// Same record.  Increase version.
					volunteerProfile.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (volunteerProfile.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.VolunteerProfile record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (volunteerProfile.color != null && volunteerProfile.color.Length > 10)
				{
					volunteerProfile.color = volunteerProfile.color.Substring(0, 10);
				}

				try
				{
				    EntityEntry<Database.VolunteerProfile> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(volunteerProfile);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        VolunteerProfileChangeHistory volunteerProfileChangeHistory = new VolunteerProfileChangeHistory();
				        volunteerProfileChangeHistory.volunteerProfileId = volunteerProfile.id;
				        volunteerProfileChangeHistory.versionNumber = volunteerProfile.versionNumber;
				        volunteerProfileChangeHistory.timeStamp = DateTime.UtcNow;
				        volunteerProfileChangeHistory.userId = securityUser.id;
				        volunteerProfileChangeHistory.tenantGuid = userTenantGuid;
				        volunteerProfileChangeHistory.data = JsonSerializer.Serialize(Database.VolunteerProfile.CreateAnonymousWithFirstLevelSubObjects(volunteerProfile));
				        _context.VolunteerProfileChangeHistories.Add(volunteerProfileChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.VolunteerProfile entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.VolunteerProfile.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.VolunteerProfile.CreateAnonymousWithFirstLevelSubObjects(volunteerProfile)),
						null);

				return Ok(Database.VolunteerProfile.CreateAnonymous(volunteerProfile));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.VolunteerProfile entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.VolunteerProfile.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.VolunteerProfile.CreateAnonymousWithFirstLevelSubObjects(volunteerProfile)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new VolunteerProfile record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerProfile", Name = "VolunteerProfile")]
		public async Task<IActionResult> PostVolunteerProfile([FromBody]Database.VolunteerProfile.VolunteerProfileDTO volunteerProfileDTO, CancellationToken cancellationToken = default)
		{
			if (volunteerProfileDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Volunteer Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Volunteer Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new VolunteerProfile object using the data from the DTO
			//
			Database.VolunteerProfile volunteerProfile = Database.VolunteerProfile.FromDTO(volunteerProfileDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				volunteerProfile.tenantGuid = userTenantGuid;

				if (volunteerProfile.color != null && volunteerProfile.color.Length > 10)
				{
					volunteerProfile.color = volunteerProfile.color.Substring(0, 10);
				}

				volunteerProfile.objectGuid = Guid.NewGuid();
				volunteerProfile.versionNumber = 1;

				_context.VolunteerProfiles.Add(volunteerProfile);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the volunteerProfile object so that no further changes will be written to the database
				    //
				    _context.Entry(volunteerProfile).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					volunteerProfile.VolunteerProfileChangeHistories = null;
					volunteerProfile.constituent = null;
					volunteerProfile.icon = null;
					volunteerProfile.resource = null;
					volunteerProfile.volunteerStatus = null;


				    VolunteerProfileChangeHistory volunteerProfileChangeHistory = new VolunteerProfileChangeHistory();
				    volunteerProfileChangeHistory.volunteerProfileId = volunteerProfile.id;
				    volunteerProfileChangeHistory.versionNumber = volunteerProfile.versionNumber;
				    volunteerProfileChangeHistory.timeStamp = DateTime.UtcNow;
				    volunteerProfileChangeHistory.userId = securityUser.id;
				    volunteerProfileChangeHistory.tenantGuid = userTenantGuid;
				    volunteerProfileChangeHistory.data = JsonSerializer.Serialize(Database.VolunteerProfile.CreateAnonymousWithFirstLevelSubObjects(volunteerProfile));
				    _context.VolunteerProfileChangeHistories.Add(volunteerProfileChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.VolunteerProfile entity successfully created.",
						true,
						volunteerProfile. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.VolunteerProfile.CreateAnonymousWithFirstLevelSubObjects(volunteerProfile)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.VolunteerProfile entity creation failed.", false, volunteerProfile.id.ToString(), "", JsonSerializer.Serialize(Database.VolunteerProfile.CreateAnonymousWithFirstLevelSubObjects(volunteerProfile)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "VolunteerProfile", volunteerProfile.id, volunteerProfile.color));

			return CreatedAtRoute("VolunteerProfile", new { id = volunteerProfile.id }, Database.VolunteerProfile.CreateAnonymousWithFirstLevelSubObjects(volunteerProfile));
		}



        /// <summary>
        /// 
        /// This rolls a VolunteerProfile entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerProfile/Rollback/{id}")]
		[Route("api/VolunteerProfile/Rollback")]
		public async Task<IActionResult> RollbackToVolunteerProfileVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.VolunteerProfile> query = (from x in _context.VolunteerProfiles
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this VolunteerProfile concurrently
			//
			lock (volunteerProfilePutSyncRoot)
			{
				
				Database.VolunteerProfile volunteerProfile = query.FirstOrDefault();
				
				if (volunteerProfile == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.VolunteerProfile rollback", id.ToString(), new Exception("No Scheduler.VolunteerProfile entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the VolunteerProfile current state so we can log it.
				//
				Database.VolunteerProfile cloneOfExisting = (Database.VolunteerProfile)_context.Entry(volunteerProfile).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.VolunteerProfileChangeHistories = null;
				cloneOfExisting.constituent = null;
				cloneOfExisting.icon = null;
				cloneOfExisting.resource = null;
				cloneOfExisting.volunteerStatus = null;

				if (versionNumber >= volunteerProfile.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.VolunteerProfile rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.VolunteerProfile rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				VolunteerProfileChangeHistory volunteerProfileChangeHistory = (from x in _context.VolunteerProfileChangeHistories
				                                               where
				                                               x.volunteerProfileId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (volunteerProfileChangeHistory != null)
				{
				    Database.VolunteerProfile oldVolunteerProfile = JsonSerializer.Deserialize<Database.VolunteerProfile>(volunteerProfileChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    volunteerProfile.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    volunteerProfile.resourceId = oldVolunteerProfile.resourceId;
				    volunteerProfile.volunteerStatusId = oldVolunteerProfile.volunteerStatusId;
				    volunteerProfile.onboardedDate = oldVolunteerProfile.onboardedDate;
				    volunteerProfile.inactiveSince = oldVolunteerProfile.inactiveSince;
				    volunteerProfile.totalHoursServed = oldVolunteerProfile.totalHoursServed;
				    volunteerProfile.lastActivityDate = oldVolunteerProfile.lastActivityDate;
				    volunteerProfile.backgroundCheckCompleted = oldVolunteerProfile.backgroundCheckCompleted;
				    volunteerProfile.backgroundCheckDate = oldVolunteerProfile.backgroundCheckDate;
				    volunteerProfile.backgroundCheckExpiry = oldVolunteerProfile.backgroundCheckExpiry;
				    volunteerProfile.confidentialityAgreementSigned = oldVolunteerProfile.confidentialityAgreementSigned;
				    volunteerProfile.confidentialityAgreementDate = oldVolunteerProfile.confidentialityAgreementDate;
				    volunteerProfile.availabilityPreferences = oldVolunteerProfile.availabilityPreferences;
				    volunteerProfile.interestsAndSkillsNotes = oldVolunteerProfile.interestsAndSkillsNotes;
				    volunteerProfile.emergencyContactNotes = oldVolunteerProfile.emergencyContactNotes;
				    volunteerProfile.constituentId = oldVolunteerProfile.constituentId;
				    volunteerProfile.iconId = oldVolunteerProfile.iconId;
				    volunteerProfile.color = oldVolunteerProfile.color;
				    volunteerProfile.attributes = oldVolunteerProfile.attributes;
				    volunteerProfile.objectGuid = oldVolunteerProfile.objectGuid;
				    volunteerProfile.active = oldVolunteerProfile.active;
				    volunteerProfile.deleted = oldVolunteerProfile.deleted;

				    string serializedVolunteerProfile = JsonSerializer.Serialize(volunteerProfile);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        VolunteerProfileChangeHistory newVolunteerProfileChangeHistory = new VolunteerProfileChangeHistory();
				        newVolunteerProfileChangeHistory.volunteerProfileId = volunteerProfile.id;
				        newVolunteerProfileChangeHistory.versionNumber = volunteerProfile.versionNumber;
				        newVolunteerProfileChangeHistory.timeStamp = DateTime.UtcNow;
				        newVolunteerProfileChangeHistory.userId = securityUser.id;
				        newVolunteerProfileChangeHistory.tenantGuid = userTenantGuid;
				        newVolunteerProfileChangeHistory.data = JsonSerializer.Serialize(Database.VolunteerProfile.CreateAnonymousWithFirstLevelSubObjects(volunteerProfile));
				        _context.VolunteerProfileChangeHistories.Add(newVolunteerProfileChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.VolunteerProfile rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.VolunteerProfile.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.VolunteerProfile.CreateAnonymousWithFirstLevelSubObjects(volunteerProfile)),
						null);


				    return Ok(Database.VolunteerProfile.CreateAnonymous(volunteerProfile));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.VolunteerProfile rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.VolunteerProfile rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a VolunteerProfile.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the VolunteerProfile</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerProfile/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetVolunteerProfileChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.VolunteerProfile volunteerProfile = await _context.VolunteerProfiles.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (volunteerProfile == null)
			{
				return NotFound();
			}

			try
			{
				volunteerProfile.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.VolunteerProfile> versionInfo = await volunteerProfile.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a VolunteerProfile.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the VolunteerProfile</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerProfile/{id}/AuditHistory")]
		public async Task<IActionResult> GetVolunteerProfileAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.VolunteerProfile volunteerProfile = await _context.VolunteerProfiles.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (volunteerProfile == null)
			{
				return NotFound();
			}

			try
			{
				volunteerProfile.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.VolunteerProfile>> versions = await volunteerProfile.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a VolunteerProfile.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the VolunteerProfile</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The VolunteerProfile object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerProfile/{id}/Version/{version}")]
		public async Task<IActionResult> GetVolunteerProfileVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.VolunteerProfile volunteerProfile = await _context.VolunteerProfiles.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (volunteerProfile == null)
			{
				return NotFound();
			}

			try
			{
				volunteerProfile.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.VolunteerProfile> versionInfo = await volunteerProfile.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a VolunteerProfile at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the VolunteerProfile</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The VolunteerProfile object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerProfile/{id}/StateAtTime")]
		public async Task<IActionResult> GetVolunteerProfileStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.VolunteerProfile volunteerProfile = await _context.VolunteerProfiles.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (volunteerProfile == null)
			{
				return NotFound();
			}

			try
			{
				volunteerProfile.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.VolunteerProfile> versionInfo = await volunteerProfile.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a VolunteerProfile record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerProfile/{id}")]
		[Route("api/VolunteerProfile")]
		public async Task<IActionResult> DeleteVolunteerProfile(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Volunteer Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Volunteer Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.VolunteerProfile> query = (from x in _context.VolunteerProfiles
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.VolunteerProfile volunteerProfile = await query.FirstOrDefaultAsync(cancellationToken);

			if (volunteerProfile == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.VolunteerProfile DELETE", id.ToString(), new Exception("No Scheduler.VolunteerProfile entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.VolunteerProfile cloneOfExisting = (Database.VolunteerProfile)_context.Entry(volunteerProfile).GetDatabaseValues().ToObject();


			lock (volunteerProfileDeleteSyncRoot)
			{
			    try
			    {
			        volunteerProfile.deleted = true;
			        volunteerProfile.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        VolunteerProfileChangeHistory volunteerProfileChangeHistory = new VolunteerProfileChangeHistory();
			        volunteerProfileChangeHistory.volunteerProfileId = volunteerProfile.id;
			        volunteerProfileChangeHistory.versionNumber = volunteerProfile.versionNumber;
			        volunteerProfileChangeHistory.timeStamp = DateTime.UtcNow;
			        volunteerProfileChangeHistory.userId = securityUser.id;
			        volunteerProfileChangeHistory.tenantGuid = userTenantGuid;
			        volunteerProfileChangeHistory.data = JsonSerializer.Serialize(Database.VolunteerProfile.CreateAnonymousWithFirstLevelSubObjects(volunteerProfile));
			        _context.VolunteerProfileChangeHistories.Add(volunteerProfileChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.VolunteerProfile entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.VolunteerProfile.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.VolunteerProfile.CreateAnonymousWithFirstLevelSubObjects(volunteerProfile)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.VolunteerProfile entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.VolunteerProfile.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.VolunteerProfile.CreateAnonymousWithFirstLevelSubObjects(volunteerProfile)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of VolunteerProfile records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/VolunteerProfiles/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? resourceId = null,
			int? volunteerStatusId = null,
			float? totalHoursServed = null,
			bool? backgroundCheckCompleted = null,
			bool? confidentialityAgreementSigned = null,
			string availabilityPreferences = null,
			string interestsAndSkillsNotes = null,
			string emergencyContactNotes = null,
			int? constituentId = null,
			int? iconId = null,
			string color = null,
			string attributes = null,
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

			IQueryable<Database.VolunteerProfile> query = (from vp in _context.VolunteerProfiles select vp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (resourceId.HasValue == true)
			{
				query = query.Where(vp => vp.resourceId == resourceId.Value);
			}
			if (volunteerStatusId.HasValue == true)
			{
				query = query.Where(vp => vp.volunteerStatusId == volunteerStatusId.Value);
			}
			if (totalHoursServed.HasValue == true)
			{
				query = query.Where(vp => vp.totalHoursServed == totalHoursServed.Value);
			}
			if (backgroundCheckCompleted.HasValue == true)
			{
				query = query.Where(vp => vp.backgroundCheckCompleted == backgroundCheckCompleted.Value);
			}
			if (confidentialityAgreementSigned.HasValue == true)
			{
				query = query.Where(vp => vp.confidentialityAgreementSigned == confidentialityAgreementSigned.Value);
			}
			if (string.IsNullOrEmpty(availabilityPreferences) == false)
			{
				query = query.Where(vp => vp.availabilityPreferences == availabilityPreferences);
			}
			if (string.IsNullOrEmpty(interestsAndSkillsNotes) == false)
			{
				query = query.Where(vp => vp.interestsAndSkillsNotes == interestsAndSkillsNotes);
			}
			if (string.IsNullOrEmpty(emergencyContactNotes) == false)
			{
				query = query.Where(vp => vp.emergencyContactNotes == emergencyContactNotes);
			}
			if (constituentId.HasValue == true)
			{
				query = query.Where(vp => vp.constituentId == constituentId.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(vp => vp.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(vp => vp.color == color);
			}
			if (string.IsNullOrEmpty(attributes) == false)
			{
				query = query.Where(vp => vp.attributes == attributes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(vp => vp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(vp => vp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(vp => vp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(vp => vp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(vp => vp.deleted == false);
				}
			}
			else
			{
				query = query.Where(vp => vp.active == true);
				query = query.Where(vp => vp.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Volunteer Profile, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.availabilityPreferences.Contains(anyStringContains)
			       || x.interestsAndSkillsNotes.Contains(anyStringContains)
			       || x.emergencyContactNotes.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.attributes.Contains(anyStringContains)
			       || x.constituent.constituentNumber.Contains(anyStringContains)
			       || x.constituent.externalId.Contains(anyStringContains)
			       || x.constituent.notes.Contains(anyStringContains)
			       || x.constituent.attributes.Contains(anyStringContains)
			       || x.constituent.color.Contains(anyStringContains)
			       || x.constituent.avatarFileName.Contains(anyStringContains)
			       || x.constituent.avatarMimeType.Contains(anyStringContains)
			       || x.icon.name.Contains(anyStringContains)
			       || x.icon.fontAwesomeCode.Contains(anyStringContains)
			       || x.resource.name.Contains(anyStringContains)
			       || x.resource.description.Contains(anyStringContains)
			       || x.resource.notes.Contains(anyStringContains)
			       || x.resource.externalId.Contains(anyStringContains)
			       || x.resource.color.Contains(anyStringContains)
			       || x.resource.attributes.Contains(anyStringContains)
			       || x.resource.avatarFileName.Contains(anyStringContains)
			       || x.resource.avatarMimeType.Contains(anyStringContains)
			       || x.volunteerStatus.name.Contains(anyStringContains)
			       || x.volunteerStatus.description.Contains(anyStringContains)
			       || x.volunteerStatus.color.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.color);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.VolunteerProfile.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/VolunteerProfile/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Volunteer Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Volunteer Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
