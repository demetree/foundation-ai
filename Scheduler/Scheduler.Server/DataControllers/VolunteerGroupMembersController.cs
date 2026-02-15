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
    /// This auto generated class provides the basic CRUD operations for the VolunteerGroupMember entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the VolunteerGroupMember entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class VolunteerGroupMembersController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 40;

		static object volunteerGroupMemberPutSyncRoot = new object();
		static object volunteerGroupMemberDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<VolunteerGroupMembersController> _logger;

		public VolunteerGroupMembersController(SchedulerContext context, ILogger<VolunteerGroupMembersController> logger) : base("Scheduler", "VolunteerGroupMember")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of VolunteerGroupMembers filtered by the parameters provided.
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
		[Route("api/VolunteerGroupMembers")]
		public async Task<IActionResult> GetVolunteerGroupMembers(
			int? volunteerGroupId = null,
			int? resourceId = null,
			int? assignmentRoleId = null,
			int? sequence = null,
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

			IQueryable<Database.VolunteerGroupMember> query = (from vgm in _context.VolunteerGroupMembers select vgm);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (volunteerGroupId.HasValue == true)
			{
				query = query.Where(vgm => vgm.volunteerGroupId == volunteerGroupId.Value);
			}
			if (resourceId.HasValue == true)
			{
				query = query.Where(vgm => vgm.resourceId == resourceId.Value);
			}
			if (assignmentRoleId.HasValue == true)
			{
				query = query.Where(vgm => vgm.assignmentRoleId == assignmentRoleId.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(vgm => vgm.sequence == sequence.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(vgm => vgm.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(vgm => vgm.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(vgm => vgm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(vgm => vgm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(vgm => vgm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(vgm => vgm.deleted == false);
				}
			}
			else
			{
				query = query.Where(vgm => vgm.active == true);
				query = query.Where(vgm => vgm.deleted == false);
			}

			query = query.OrderBy(vgm => vgm.sequence).ThenBy(vgm => vgm.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.assignmentRole);
				query = query.Include(x => x.resource);
				query = query.Include(x => x.volunteerGroup);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Volunteer Group Member, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.notes.Contains(anyStringContains)
			       || (includeRelations == true && x.assignmentRole.name.Contains(anyStringContains))
			       || (includeRelations == true && x.assignmentRole.description.Contains(anyStringContains))
			       || (includeRelations == true && x.assignmentRole.color.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.name.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.description.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.color.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.avatarMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.volunteerGroup.name.Contains(anyStringContains))
			       || (includeRelations == true && x.volunteerGroup.description.Contains(anyStringContains))
			       || (includeRelations == true && x.volunteerGroup.purpose.Contains(anyStringContains))
			       || (includeRelations == true && x.volunteerGroup.color.Contains(anyStringContains))
			       || (includeRelations == true && x.volunteerGroup.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.volunteerGroup.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.volunteerGroup.avatarMimeType.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.VolunteerGroupMember> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.VolunteerGroupMember volunteerGroupMember in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(volunteerGroupMember, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.VolunteerGroupMember Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.VolunteerGroupMember Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of VolunteerGroupMembers filtered by the parameters provided.  Its query is similar to the GetVolunteerGroupMembers method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerGroupMembers/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? volunteerGroupId = null,
			int? resourceId = null,
			int? assignmentRoleId = null,
			int? sequence = null,
			string notes = null,
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


			IQueryable<Database.VolunteerGroupMember> query = (from vgm in _context.VolunteerGroupMembers select vgm);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (volunteerGroupId.HasValue == true)
			{
				query = query.Where(vgm => vgm.volunteerGroupId == volunteerGroupId.Value);
			}
			if (resourceId.HasValue == true)
			{
				query = query.Where(vgm => vgm.resourceId == resourceId.Value);
			}
			if (assignmentRoleId.HasValue == true)
			{
				query = query.Where(vgm => vgm.assignmentRoleId == assignmentRoleId.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(vgm => vgm.sequence == sequence.Value);
			}
			if (notes != null)
			{
				query = query.Where(vgm => vgm.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(vgm => vgm.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(vgm => vgm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(vgm => vgm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(vgm => vgm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(vgm => vgm.deleted == false);
				}
			}
			else
			{
				query = query.Where(vgm => vgm.active == true);
				query = query.Where(vgm => vgm.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Volunteer Group Member, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.notes.Contains(anyStringContains)
			       || x.assignmentRole.name.Contains(anyStringContains)
			       || x.assignmentRole.description.Contains(anyStringContains)
			       || x.assignmentRole.color.Contains(anyStringContains)
			       || x.resource.name.Contains(anyStringContains)
			       || x.resource.description.Contains(anyStringContains)
			       || x.resource.notes.Contains(anyStringContains)
			       || x.resource.externalId.Contains(anyStringContains)
			       || x.resource.color.Contains(anyStringContains)
			       || x.resource.attributes.Contains(anyStringContains)
			       || x.resource.avatarFileName.Contains(anyStringContains)
			       || x.resource.avatarMimeType.Contains(anyStringContains)
			       || x.volunteerGroup.name.Contains(anyStringContains)
			       || x.volunteerGroup.description.Contains(anyStringContains)
			       || x.volunteerGroup.purpose.Contains(anyStringContains)
			       || x.volunteerGroup.color.Contains(anyStringContains)
			       || x.volunteerGroup.notes.Contains(anyStringContains)
			       || x.volunteerGroup.avatarFileName.Contains(anyStringContains)
			       || x.volunteerGroup.avatarMimeType.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single VolunteerGroupMember by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerGroupMember/{id}")]
		public async Task<IActionResult> GetVolunteerGroupMember(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.VolunteerGroupMember> query = (from vgm in _context.VolunteerGroupMembers where
							(vgm.id == id) &&
							(userIsAdmin == true || vgm.deleted == false) &&
							(userIsWriter == true || vgm.active == true)
					select vgm);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.assignmentRole);
					query = query.Include(x => x.resource);
					query = query.Include(x => x.volunteerGroup);
					query = query.AsSplitQuery();
				}

				Database.VolunteerGroupMember materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.VolunteerGroupMember Entity was read with Admin privilege." : "Scheduler.VolunteerGroupMember Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "VolunteerGroupMember", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.VolunteerGroupMember entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.VolunteerGroupMember.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.VolunteerGroupMember.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing VolunteerGroupMember record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/VolunteerGroupMember/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutVolunteerGroupMember(int id, [FromBody]Database.VolunteerGroupMember.VolunteerGroupMemberDTO volunteerGroupMemberDTO, CancellationToken cancellationToken = default)
		{
			if (volunteerGroupMemberDTO == null)
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



			if (id != volunteerGroupMemberDTO.id)
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


			IQueryable<Database.VolunteerGroupMember> query = (from x in _context.VolunteerGroupMembers
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.VolunteerGroupMember existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.VolunteerGroupMember PUT", id.ToString(), new Exception("No Scheduler.VolunteerGroupMember entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (volunteerGroupMemberDTO.objectGuid == Guid.Empty)
            {
                volunteerGroupMemberDTO.objectGuid = existing.objectGuid;
            }
            else if (volunteerGroupMemberDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a VolunteerGroupMember record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.VolunteerGroupMember cloneOfExisting = (Database.VolunteerGroupMember)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new VolunteerGroupMember object using the data from the existing record, updated with what is in the DTO.
			//
			Database.VolunteerGroupMember volunteerGroupMember = (Database.VolunteerGroupMember)_context.Entry(existing).GetDatabaseValues().ToObject();
			volunteerGroupMember.ApplyDTO(volunteerGroupMemberDTO);
			//
			// The tenant guid for any VolunteerGroupMember being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the VolunteerGroupMember because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				volunteerGroupMember.tenantGuid = existing.tenantGuid;
			}

			lock (volunteerGroupMemberPutSyncRoot)
			{
				//
				// Validate the version number for the volunteerGroupMember being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != volunteerGroupMember.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "VolunteerGroupMember save attempt was made but save request was with version " + volunteerGroupMember.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The VolunteerGroupMember you are trying to update has already changed.  Please try your save again after reloading the VolunteerGroupMember.");
				}
				else
				{
					// Same record.  Increase version.
					volunteerGroupMember.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (volunteerGroupMember.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.VolunteerGroupMember record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				try
				{
				    EntityEntry<Database.VolunteerGroupMember> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(volunteerGroupMember);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        VolunteerGroupMemberChangeHistory volunteerGroupMemberChangeHistory = new VolunteerGroupMemberChangeHistory();
				        volunteerGroupMemberChangeHistory.volunteerGroupMemberId = volunteerGroupMember.id;
				        volunteerGroupMemberChangeHistory.versionNumber = volunteerGroupMember.versionNumber;
				        volunteerGroupMemberChangeHistory.timeStamp = DateTime.UtcNow;
				        volunteerGroupMemberChangeHistory.userId = securityUser.id;
				        volunteerGroupMemberChangeHistory.tenantGuid = userTenantGuid;
				        volunteerGroupMemberChangeHistory.data = JsonSerializer.Serialize(Database.VolunteerGroupMember.CreateAnonymousWithFirstLevelSubObjects(volunteerGroupMember));
				        _context.VolunteerGroupMemberChangeHistories.Add(volunteerGroupMemberChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.VolunteerGroupMember entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.VolunteerGroupMember.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.VolunteerGroupMember.CreateAnonymousWithFirstLevelSubObjects(volunteerGroupMember)),
						null);

				return Ok(Database.VolunteerGroupMember.CreateAnonymous(volunteerGroupMember));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.VolunteerGroupMember entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.VolunteerGroupMember.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.VolunteerGroupMember.CreateAnonymousWithFirstLevelSubObjects(volunteerGroupMember)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new VolunteerGroupMember record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerGroupMember", Name = "VolunteerGroupMember")]
		public async Task<IActionResult> PostVolunteerGroupMember([FromBody]Database.VolunteerGroupMember.VolunteerGroupMemberDTO volunteerGroupMemberDTO, CancellationToken cancellationToken = default)
		{
			if (volunteerGroupMemberDTO == null)
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
			// Create a new VolunteerGroupMember object using the data from the DTO
			//
			Database.VolunteerGroupMember volunteerGroupMember = Database.VolunteerGroupMember.FromDTO(volunteerGroupMemberDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				volunteerGroupMember.tenantGuid = userTenantGuid;

				volunteerGroupMember.objectGuid = Guid.NewGuid();
				volunteerGroupMember.versionNumber = 1;

				_context.VolunteerGroupMembers.Add(volunteerGroupMember);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the volunteerGroupMember object so that no further changes will be written to the database
				    //
				    _context.Entry(volunteerGroupMember).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					volunteerGroupMember.VolunteerGroupMemberChangeHistories = null;
					volunteerGroupMember.assignmentRole = null;
					volunteerGroupMember.resource = null;
					volunteerGroupMember.volunteerGroup = null;


				    VolunteerGroupMemberChangeHistory volunteerGroupMemberChangeHistory = new VolunteerGroupMemberChangeHistory();
				    volunteerGroupMemberChangeHistory.volunteerGroupMemberId = volunteerGroupMember.id;
				    volunteerGroupMemberChangeHistory.versionNumber = volunteerGroupMember.versionNumber;
				    volunteerGroupMemberChangeHistory.timeStamp = DateTime.UtcNow;
				    volunteerGroupMemberChangeHistory.userId = securityUser.id;
				    volunteerGroupMemberChangeHistory.tenantGuid = userTenantGuid;
				    volunteerGroupMemberChangeHistory.data = JsonSerializer.Serialize(Database.VolunteerGroupMember.CreateAnonymousWithFirstLevelSubObjects(volunteerGroupMember));
				    _context.VolunteerGroupMemberChangeHistories.Add(volunteerGroupMemberChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.VolunteerGroupMember entity successfully created.",
						true,
						volunteerGroupMember. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.VolunteerGroupMember.CreateAnonymousWithFirstLevelSubObjects(volunteerGroupMember)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.VolunteerGroupMember entity creation failed.", false, volunteerGroupMember.id.ToString(), "", JsonSerializer.Serialize(Database.VolunteerGroupMember.CreateAnonymousWithFirstLevelSubObjects(volunteerGroupMember)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "VolunteerGroupMember", volunteerGroupMember.id, volunteerGroupMember.id.ToString()));

			return CreatedAtRoute("VolunteerGroupMember", new { id = volunteerGroupMember.id }, Database.VolunteerGroupMember.CreateAnonymousWithFirstLevelSubObjects(volunteerGroupMember));
		}



        /// <summary>
        /// 
        /// This rolls a VolunteerGroupMember entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerGroupMember/Rollback/{id}")]
		[Route("api/VolunteerGroupMember/Rollback")]
		public async Task<IActionResult> RollbackToVolunteerGroupMemberVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.VolunteerGroupMember> query = (from x in _context.VolunteerGroupMembers
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this VolunteerGroupMember concurrently
			//
			lock (volunteerGroupMemberPutSyncRoot)
			{
				
				Database.VolunteerGroupMember volunteerGroupMember = query.FirstOrDefault();
				
				if (volunteerGroupMember == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.VolunteerGroupMember rollback", id.ToString(), new Exception("No Scheduler.VolunteerGroupMember entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the VolunteerGroupMember current state so we can log it.
				//
				Database.VolunteerGroupMember cloneOfExisting = (Database.VolunteerGroupMember)_context.Entry(volunteerGroupMember).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.VolunteerGroupMemberChangeHistories = null;
				cloneOfExisting.assignmentRole = null;
				cloneOfExisting.resource = null;
				cloneOfExisting.volunteerGroup = null;

				if (versionNumber >= volunteerGroupMember.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.VolunteerGroupMember rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.VolunteerGroupMember rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				VolunteerGroupMemberChangeHistory volunteerGroupMemberChangeHistory = (from x in _context.VolunteerGroupMemberChangeHistories
				                                               where
				                                               x.volunteerGroupMemberId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (volunteerGroupMemberChangeHistory != null)
				{
				    Database.VolunteerGroupMember oldVolunteerGroupMember = JsonSerializer.Deserialize<Database.VolunteerGroupMember>(volunteerGroupMemberChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    volunteerGroupMember.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    volunteerGroupMember.volunteerGroupId = oldVolunteerGroupMember.volunteerGroupId;
				    volunteerGroupMember.resourceId = oldVolunteerGroupMember.resourceId;
				    volunteerGroupMember.assignmentRoleId = oldVolunteerGroupMember.assignmentRoleId;
				    volunteerGroupMember.sequence = oldVolunteerGroupMember.sequence;
				    volunteerGroupMember.joinedDate = oldVolunteerGroupMember.joinedDate;
				    volunteerGroupMember.leftDate = oldVolunteerGroupMember.leftDate;
				    volunteerGroupMember.notes = oldVolunteerGroupMember.notes;
				    volunteerGroupMember.objectGuid = oldVolunteerGroupMember.objectGuid;
				    volunteerGroupMember.active = oldVolunteerGroupMember.active;
				    volunteerGroupMember.deleted = oldVolunteerGroupMember.deleted;

				    string serializedVolunteerGroupMember = JsonSerializer.Serialize(volunteerGroupMember);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        VolunteerGroupMemberChangeHistory newVolunteerGroupMemberChangeHistory = new VolunteerGroupMemberChangeHistory();
				        newVolunteerGroupMemberChangeHistory.volunteerGroupMemberId = volunteerGroupMember.id;
				        newVolunteerGroupMemberChangeHistory.versionNumber = volunteerGroupMember.versionNumber;
				        newVolunteerGroupMemberChangeHistory.timeStamp = DateTime.UtcNow;
				        newVolunteerGroupMemberChangeHistory.userId = securityUser.id;
				        newVolunteerGroupMemberChangeHistory.tenantGuid = userTenantGuid;
				        newVolunteerGroupMemberChangeHistory.data = JsonSerializer.Serialize(Database.VolunteerGroupMember.CreateAnonymousWithFirstLevelSubObjects(volunteerGroupMember));
				        _context.VolunteerGroupMemberChangeHistories.Add(newVolunteerGroupMemberChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.VolunteerGroupMember rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.VolunteerGroupMember.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.VolunteerGroupMember.CreateAnonymousWithFirstLevelSubObjects(volunteerGroupMember)),
						null);


				    return Ok(Database.VolunteerGroupMember.CreateAnonymous(volunteerGroupMember));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.VolunteerGroupMember rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.VolunteerGroupMember rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a VolunteerGroupMember.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the VolunteerGroupMember</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerGroupMember/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetVolunteerGroupMemberChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.VolunteerGroupMember volunteerGroupMember = await _context.VolunteerGroupMembers.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (volunteerGroupMember == null)
			{
				return NotFound();
			}

			try
			{
				volunteerGroupMember.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.VolunteerGroupMember> versionInfo = await volunteerGroupMember.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a VolunteerGroupMember.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the VolunteerGroupMember</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerGroupMember/{id}/AuditHistory")]
		public async Task<IActionResult> GetVolunteerGroupMemberAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.VolunteerGroupMember volunteerGroupMember = await _context.VolunteerGroupMembers.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (volunteerGroupMember == null)
			{
				return NotFound();
			}

			try
			{
				volunteerGroupMember.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.VolunteerGroupMember>> versions = await volunteerGroupMember.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a VolunteerGroupMember.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the VolunteerGroupMember</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The VolunteerGroupMember object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerGroupMember/{id}/Version/{version}")]
		public async Task<IActionResult> GetVolunteerGroupMemberVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.VolunteerGroupMember volunteerGroupMember = await _context.VolunteerGroupMembers.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (volunteerGroupMember == null)
			{
				return NotFound();
			}

			try
			{
				volunteerGroupMember.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.VolunteerGroupMember> versionInfo = await volunteerGroupMember.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a VolunteerGroupMember at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the VolunteerGroupMember</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The VolunteerGroupMember object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerGroupMember/{id}/StateAtTime")]
		public async Task<IActionResult> GetVolunteerGroupMemberStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.VolunteerGroupMember volunteerGroupMember = await _context.VolunteerGroupMembers.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (volunteerGroupMember == null)
			{
				return NotFound();
			}

			try
			{
				volunteerGroupMember.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.VolunteerGroupMember> versionInfo = await volunteerGroupMember.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a VolunteerGroupMember record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerGroupMember/{id}")]
		[Route("api/VolunteerGroupMember")]
		public async Task<IActionResult> DeleteVolunteerGroupMember(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.VolunteerGroupMember> query = (from x in _context.VolunteerGroupMembers
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.VolunteerGroupMember volunteerGroupMember = await query.FirstOrDefaultAsync(cancellationToken);

			if (volunteerGroupMember == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.VolunteerGroupMember DELETE", id.ToString(), new Exception("No Scheduler.VolunteerGroupMember entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.VolunteerGroupMember cloneOfExisting = (Database.VolunteerGroupMember)_context.Entry(volunteerGroupMember).GetDatabaseValues().ToObject();


			lock (volunteerGroupMemberDeleteSyncRoot)
			{
			    try
			    {
			        volunteerGroupMember.deleted = true;
			        volunteerGroupMember.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        VolunteerGroupMemberChangeHistory volunteerGroupMemberChangeHistory = new VolunteerGroupMemberChangeHistory();
			        volunteerGroupMemberChangeHistory.volunteerGroupMemberId = volunteerGroupMember.id;
			        volunteerGroupMemberChangeHistory.versionNumber = volunteerGroupMember.versionNumber;
			        volunteerGroupMemberChangeHistory.timeStamp = DateTime.UtcNow;
			        volunteerGroupMemberChangeHistory.userId = securityUser.id;
			        volunteerGroupMemberChangeHistory.tenantGuid = userTenantGuid;
			        volunteerGroupMemberChangeHistory.data = JsonSerializer.Serialize(Database.VolunteerGroupMember.CreateAnonymousWithFirstLevelSubObjects(volunteerGroupMember));
			        _context.VolunteerGroupMemberChangeHistories.Add(volunteerGroupMemberChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.VolunteerGroupMember entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.VolunteerGroupMember.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.VolunteerGroupMember.CreateAnonymousWithFirstLevelSubObjects(volunteerGroupMember)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.VolunteerGroupMember entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.VolunteerGroupMember.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.VolunteerGroupMember.CreateAnonymousWithFirstLevelSubObjects(volunteerGroupMember)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of VolunteerGroupMember records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/VolunteerGroupMembers/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? volunteerGroupId = null,
			int? resourceId = null,
			int? assignmentRoleId = null,
			int? sequence = null,
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

			IQueryable<Database.VolunteerGroupMember> query = (from vgm in _context.VolunteerGroupMembers select vgm);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (volunteerGroupId.HasValue == true)
			{
				query = query.Where(vgm => vgm.volunteerGroupId == volunteerGroupId.Value);
			}
			if (resourceId.HasValue == true)
			{
				query = query.Where(vgm => vgm.resourceId == resourceId.Value);
			}
			if (assignmentRoleId.HasValue == true)
			{
				query = query.Where(vgm => vgm.assignmentRoleId == assignmentRoleId.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(vgm => vgm.sequence == sequence.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(vgm => vgm.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(vgm => vgm.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(vgm => vgm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(vgm => vgm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(vgm => vgm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(vgm => vgm.deleted == false);
				}
			}
			else
			{
				query = query.Where(vgm => vgm.active == true);
				query = query.Where(vgm => vgm.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Volunteer Group Member, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.notes.Contains(anyStringContains)
			       || x.assignmentRole.name.Contains(anyStringContains)
			       || x.assignmentRole.description.Contains(anyStringContains)
			       || x.assignmentRole.color.Contains(anyStringContains)
			       || x.resource.name.Contains(anyStringContains)
			       || x.resource.description.Contains(anyStringContains)
			       || x.resource.notes.Contains(anyStringContains)
			       || x.resource.externalId.Contains(anyStringContains)
			       || x.resource.color.Contains(anyStringContains)
			       || x.resource.attributes.Contains(anyStringContains)
			       || x.resource.avatarFileName.Contains(anyStringContains)
			       || x.resource.avatarMimeType.Contains(anyStringContains)
			       || x.volunteerGroup.name.Contains(anyStringContains)
			       || x.volunteerGroup.description.Contains(anyStringContains)
			       || x.volunteerGroup.purpose.Contains(anyStringContains)
			       || x.volunteerGroup.color.Contains(anyStringContains)
			       || x.volunteerGroup.notes.Contains(anyStringContains)
			       || x.volunteerGroup.avatarFileName.Contains(anyStringContains)
			       || x.volunteerGroup.avatarMimeType.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.VolunteerGroupMember.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/VolunteerGroupMember/CreateAuditEvent")]
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
