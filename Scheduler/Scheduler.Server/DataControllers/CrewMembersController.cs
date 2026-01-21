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
    /// This auto generated class provides the basic CRUD operations for the CrewMember entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the CrewMember entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class CrewMembersController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object crewMemberPutSyncRoot = new object();
		static object crewMemberDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<CrewMembersController> _logger;

		public CrewMembersController(SchedulerContext context, ILogger<CrewMembersController> logger) : base("Scheduler", "CrewMember")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of CrewMembers filtered by the parameters provided.
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
		[Route("api/CrewMembers")]
		public async Task<IActionResult> GetCrewMembers(
			int? crewId = null,
			int? resourceId = null,
			int? assignmentRoleId = null,
			int? sequence = null,
			int? iconId = null,
			string color = null,
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

			IQueryable<Database.CrewMember> query = (from cm in _context.CrewMembers select cm);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (crewId.HasValue == true)
			{
				query = query.Where(cm => cm.crewId == crewId.Value);
			}
			if (resourceId.HasValue == true)
			{
				query = query.Where(cm => cm.resourceId == resourceId.Value);
			}
			if (assignmentRoleId.HasValue == true)
			{
				query = query.Where(cm => cm.assignmentRoleId == assignmentRoleId.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(cm => cm.sequence == sequence.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(cm => cm.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(cm => cm.color == color);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(cm => cm.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cm => cm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cm => cm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cm => cm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cm => cm.deleted == false);
				}
			}
			else
			{
				query = query.Where(cm => cm.active == true);
				query = query.Where(cm => cm.deleted == false);
			}

			query = query.OrderBy(cm => cm.sequence).ThenBy(cm => cm.color);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.assignmentRole);
				query = query.Include(x => x.crew);
				query = query.Include(x => x.icon);
				query = query.Include(x => x.resource);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Crew Member, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.color.Contains(anyStringContains)
			       || (includeRelations == true && x.assignmentRole.name.Contains(anyStringContains))
			       || (includeRelations == true && x.assignmentRole.description.Contains(anyStringContains))
			       || (includeRelations == true && x.assignmentRole.color.Contains(anyStringContains))
			       || (includeRelations == true && x.crew.name.Contains(anyStringContains))
			       || (includeRelations == true && x.crew.description.Contains(anyStringContains))
			       || (includeRelations == true && x.crew.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.crew.color.Contains(anyStringContains))
			       || (includeRelations == true && x.crew.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.crew.avatarMimeType.Contains(anyStringContains))
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
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.CrewMember> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.CrewMember crewMember in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(crewMember, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.CrewMember Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.CrewMember Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of CrewMembers filtered by the parameters provided.  Its query is similar to the GetCrewMembers method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/CrewMembers/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? crewId = null,
			int? resourceId = null,
			int? assignmentRoleId = null,
			int? sequence = null,
			int? iconId = null,
			string color = null,
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


			IQueryable<Database.CrewMember> query = (from cm in _context.CrewMembers select cm);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (crewId.HasValue == true)
			{
				query = query.Where(cm => cm.crewId == crewId.Value);
			}
			if (resourceId.HasValue == true)
			{
				query = query.Where(cm => cm.resourceId == resourceId.Value);
			}
			if (assignmentRoleId.HasValue == true)
			{
				query = query.Where(cm => cm.assignmentRoleId == assignmentRoleId.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(cm => cm.sequence == sequence.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(cm => cm.iconId == iconId.Value);
			}
			if (color != null)
			{
				query = query.Where(cm => cm.color == color);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(cm => cm.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cm => cm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cm => cm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cm => cm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cm => cm.deleted == false);
				}
			}
			else
			{
				query = query.Where(cm => cm.active == true);
				query = query.Where(cm => cm.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Crew Member, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.color.Contains(anyStringContains)
			       || x.assignmentRole.name.Contains(anyStringContains)
			       || x.assignmentRole.description.Contains(anyStringContains)
			       || x.assignmentRole.color.Contains(anyStringContains)
			       || x.crew.name.Contains(anyStringContains)
			       || x.crew.description.Contains(anyStringContains)
			       || x.crew.notes.Contains(anyStringContains)
			       || x.crew.color.Contains(anyStringContains)
			       || x.crew.avatarFileName.Contains(anyStringContains)
			       || x.crew.avatarMimeType.Contains(anyStringContains)
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
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single CrewMember by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/CrewMember/{id}")]
		public async Task<IActionResult> GetCrewMember(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.CrewMember> query = (from cm in _context.CrewMembers where
							(cm.id == id) &&
							(userIsAdmin == true || cm.deleted == false) &&
							(userIsWriter == true || cm.active == true)
					select cm);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.assignmentRole);
					query = query.Include(x => x.crew);
					query = query.Include(x => x.icon);
					query = query.Include(x => x.resource);
					query = query.AsSplitQuery();
				}

				Database.CrewMember materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.CrewMember Entity was read with Admin privilege." : "Scheduler.CrewMember Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "CrewMember", materialized.id, materialized.color));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.CrewMember entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.CrewMember.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.CrewMember.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing CrewMember record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/CrewMember/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutCrewMember(int id, [FromBody]Database.CrewMember.CrewMemberDTO crewMemberDTO, CancellationToken cancellationToken = default)
		{
			if (crewMemberDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != crewMemberDTO.id)
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


			IQueryable<Database.CrewMember> query = (from x in _context.CrewMembers
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.CrewMember existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.CrewMember PUT", id.ToString(), new Exception("No Scheduler.CrewMember entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (crewMemberDTO.objectGuid == Guid.Empty)
            {
                crewMemberDTO.objectGuid = existing.objectGuid;
            }
            else if (crewMemberDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a CrewMember record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.CrewMember cloneOfExisting = (Database.CrewMember)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new CrewMember object using the data from the existing record, updated with what is in the DTO.
			//
			Database.CrewMember crewMember = (Database.CrewMember)_context.Entry(existing).GetDatabaseValues().ToObject();
			crewMember.ApplyDTO(crewMemberDTO);
			//
			// The tenant guid for any CrewMember being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the CrewMember because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				crewMember.tenantGuid = existing.tenantGuid;
			}

			lock (crewMemberPutSyncRoot)
			{
				//
				// Validate the version number for the crewMember being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != crewMember.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "CrewMember save attempt was made but save request was with version " + crewMember.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The CrewMember you are trying to update has already changed.  Please try your save again after reloading the CrewMember.");
				}
				else
				{
					// Same record.  Increase version.
					crewMember.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (crewMember.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.CrewMember record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (crewMember.color != null && crewMember.color.Length > 10)
				{
					crewMember.color = crewMember.color.Substring(0, 10);
				}

				try
				{
				    EntityEntry<Database.CrewMember> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(crewMember);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        CrewMemberChangeHistory crewMemberChangeHistory = new CrewMemberChangeHistory();
				        crewMemberChangeHistory.crewMemberId = crewMember.id;
				        crewMemberChangeHistory.versionNumber = crewMember.versionNumber;
				        crewMemberChangeHistory.timeStamp = DateTime.UtcNow;
				        crewMemberChangeHistory.userId = securityUser.id;
				        crewMemberChangeHistory.tenantGuid = userTenantGuid;
				        crewMemberChangeHistory.data = JsonSerializer.Serialize(Database.CrewMember.CreateAnonymousWithFirstLevelSubObjects(crewMember));
				        _context.CrewMemberChangeHistories.Add(crewMemberChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.CrewMember entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.CrewMember.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.CrewMember.CreateAnonymousWithFirstLevelSubObjects(crewMember)),
						null);

				return Ok(Database.CrewMember.CreateAnonymous(crewMember));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.CrewMember entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.CrewMember.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.CrewMember.CreateAnonymousWithFirstLevelSubObjects(crewMember)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new CrewMember record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/CrewMember", Name = "CrewMember")]
		public async Task<IActionResult> PostCrewMember([FromBody]Database.CrewMember.CrewMemberDTO crewMemberDTO, CancellationToken cancellationToken = default)
		{
			if (crewMemberDTO == null)
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
			// Create a new CrewMember object using the data from the DTO
			//
			Database.CrewMember crewMember = Database.CrewMember.FromDTO(crewMemberDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				crewMember.tenantGuid = userTenantGuid;

				if (crewMember.color != null && crewMember.color.Length > 10)
				{
					crewMember.color = crewMember.color.Substring(0, 10);
				}

				crewMember.objectGuid = Guid.NewGuid();
				crewMember.versionNumber = 1;

				_context.CrewMembers.Add(crewMember);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the crewMember object so that no further changes will be written to the database
				    //
				    _context.Entry(crewMember).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					crewMember.CrewMemberChangeHistories = null;
					crewMember.assignmentRole = null;
					crewMember.crew = null;
					crewMember.icon = null;
					crewMember.resource = null;


				    CrewMemberChangeHistory crewMemberChangeHistory = new CrewMemberChangeHistory();
				    crewMemberChangeHistory.crewMemberId = crewMember.id;
				    crewMemberChangeHistory.versionNumber = crewMember.versionNumber;
				    crewMemberChangeHistory.timeStamp = DateTime.UtcNow;
				    crewMemberChangeHistory.userId = securityUser.id;
				    crewMemberChangeHistory.tenantGuid = userTenantGuid;
				    crewMemberChangeHistory.data = JsonSerializer.Serialize(Database.CrewMember.CreateAnonymousWithFirstLevelSubObjects(crewMember));
				    _context.CrewMemberChangeHistories.Add(crewMemberChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.CrewMember entity successfully created.",
						true,
						crewMember. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.CrewMember.CreateAnonymousWithFirstLevelSubObjects(crewMember)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.CrewMember entity creation failed.", false, crewMember.id.ToString(), "", JsonSerializer.Serialize(Database.CrewMember.CreateAnonymousWithFirstLevelSubObjects(crewMember)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "CrewMember", crewMember.id, crewMember.color));

			return CreatedAtRoute("CrewMember", new { id = crewMember.id }, Database.CrewMember.CreateAnonymousWithFirstLevelSubObjects(crewMember));
		}



        /// <summary>
        /// 
        /// This rolls a CrewMember entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/CrewMember/Rollback/{id}")]
		[Route("api/CrewMember/Rollback")]
		public async Task<IActionResult> RollbackToCrewMemberVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.CrewMember> query = (from x in _context.CrewMembers
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this CrewMember concurrently
			//
			lock (crewMemberPutSyncRoot)
			{
				
				Database.CrewMember crewMember = query.FirstOrDefault();
				
				if (crewMember == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.CrewMember rollback", id.ToString(), new Exception("No Scheduler.CrewMember entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the CrewMember current state so we can log it.
				//
				Database.CrewMember cloneOfExisting = (Database.CrewMember)_context.Entry(crewMember).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.CrewMemberChangeHistories = null;
				cloneOfExisting.assignmentRole = null;
				cloneOfExisting.crew = null;
				cloneOfExisting.icon = null;
				cloneOfExisting.resource = null;

				if (versionNumber >= crewMember.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.CrewMember rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.CrewMember rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				CrewMemberChangeHistory crewMemberChangeHistory = (from x in _context.CrewMemberChangeHistories
				                                               where
				                                               x.crewMemberId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (crewMemberChangeHistory != null)
				{
				    Database.CrewMember oldCrewMember = JsonSerializer.Deserialize<Database.CrewMember>(crewMemberChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    crewMember.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    crewMember.crewId = oldCrewMember.crewId;
				    crewMember.resourceId = oldCrewMember.resourceId;
				    crewMember.assignmentRoleId = oldCrewMember.assignmentRoleId;
				    crewMember.sequence = oldCrewMember.sequence;
				    crewMember.iconId = oldCrewMember.iconId;
				    crewMember.color = oldCrewMember.color;
				    crewMember.objectGuid = oldCrewMember.objectGuid;
				    crewMember.active = oldCrewMember.active;
				    crewMember.deleted = oldCrewMember.deleted;

				    string serializedCrewMember = JsonSerializer.Serialize(crewMember);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        CrewMemberChangeHistory newCrewMemberChangeHistory = new CrewMemberChangeHistory();
				        newCrewMemberChangeHistory.crewMemberId = crewMember.id;
				        newCrewMemberChangeHistory.versionNumber = crewMember.versionNumber;
				        newCrewMemberChangeHistory.timeStamp = DateTime.UtcNow;
				        newCrewMemberChangeHistory.userId = securityUser.id;
				        newCrewMemberChangeHistory.tenantGuid = userTenantGuid;
				        newCrewMemberChangeHistory.data = JsonSerializer.Serialize(Database.CrewMember.CreateAnonymousWithFirstLevelSubObjects(crewMember));
				        _context.CrewMemberChangeHistories.Add(newCrewMemberChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.CrewMember rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.CrewMember.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.CrewMember.CreateAnonymousWithFirstLevelSubObjects(crewMember)),
						null);


				    return Ok(Database.CrewMember.CreateAnonymous(crewMember));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.CrewMember rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.CrewMember rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a CrewMember.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the CrewMember</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/CrewMember/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetCrewMemberChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.CrewMember crewMember = await _context.CrewMembers.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (crewMember == null)
			{
				return NotFound();
			}

			try
			{
				crewMember.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.CrewMember> versionInfo = await crewMember.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a CrewMember.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the CrewMember</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/CrewMember/{id}/AuditHistory")]
		public async Task<IActionResult> GetCrewMemberAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.CrewMember crewMember = await _context.CrewMembers.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (crewMember == null)
			{
				return NotFound();
			}

			try
			{
				crewMember.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.CrewMember>> versions = await crewMember.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a CrewMember.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the CrewMember</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The CrewMember object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/CrewMember/{id}/Version/{version}")]
		public async Task<IActionResult> GetCrewMemberVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.CrewMember crewMember = await _context.CrewMembers.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (crewMember == null)
			{
				return NotFound();
			}

			try
			{
				crewMember.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.CrewMember> versionInfo = await crewMember.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a CrewMember at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the CrewMember</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The CrewMember object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/CrewMember/{id}/StateAtTime")]
		public async Task<IActionResult> GetCrewMemberStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.CrewMember crewMember = await _context.CrewMembers.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (crewMember == null)
			{
				return NotFound();
			}

			try
			{
				crewMember.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.CrewMember> versionInfo = await crewMember.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a CrewMember record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/CrewMember/{id}")]
		[Route("api/CrewMember")]
		public async Task<IActionResult> DeleteCrewMember(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.CrewMember> query = (from x in _context.CrewMembers
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.CrewMember crewMember = await query.FirstOrDefaultAsync(cancellationToken);

			if (crewMember == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.CrewMember DELETE", id.ToString(), new Exception("No Scheduler.CrewMember entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.CrewMember cloneOfExisting = (Database.CrewMember)_context.Entry(crewMember).GetDatabaseValues().ToObject();


			lock (crewMemberDeleteSyncRoot)
			{
			    try
			    {
			        crewMember.deleted = true;
			        crewMember.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        CrewMemberChangeHistory crewMemberChangeHistory = new CrewMemberChangeHistory();
			        crewMemberChangeHistory.crewMemberId = crewMember.id;
			        crewMemberChangeHistory.versionNumber = crewMember.versionNumber;
			        crewMemberChangeHistory.timeStamp = DateTime.UtcNow;
			        crewMemberChangeHistory.userId = securityUser.id;
			        crewMemberChangeHistory.tenantGuid = userTenantGuid;
			        crewMemberChangeHistory.data = JsonSerializer.Serialize(Database.CrewMember.CreateAnonymousWithFirstLevelSubObjects(crewMember));
			        _context.CrewMemberChangeHistories.Add(crewMemberChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.CrewMember entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.CrewMember.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.CrewMember.CreateAnonymousWithFirstLevelSubObjects(crewMember)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.CrewMember entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.CrewMember.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.CrewMember.CreateAnonymousWithFirstLevelSubObjects(crewMember)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of CrewMember records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/CrewMembers/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? crewId = null,
			int? resourceId = null,
			int? assignmentRoleId = null,
			int? sequence = null,
			int? iconId = null,
			string color = null,
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

			IQueryable<Database.CrewMember> query = (from cm in _context.CrewMembers select cm);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (crewId.HasValue == true)
			{
				query = query.Where(cm => cm.crewId == crewId.Value);
			}
			if (resourceId.HasValue == true)
			{
				query = query.Where(cm => cm.resourceId == resourceId.Value);
			}
			if (assignmentRoleId.HasValue == true)
			{
				query = query.Where(cm => cm.assignmentRoleId == assignmentRoleId.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(cm => cm.sequence == sequence.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(cm => cm.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(cm => cm.color == color);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(cm => cm.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cm => cm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cm => cm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cm => cm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cm => cm.deleted == false);
				}
			}
			else
			{
				query = query.Where(cm => cm.active == true);
				query = query.Where(cm => cm.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Crew Member, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.color.Contains(anyStringContains)
			       || x.assignmentRole.name.Contains(anyStringContains)
			       || x.assignmentRole.description.Contains(anyStringContains)
			       || x.assignmentRole.color.Contains(anyStringContains)
			       || x.crew.name.Contains(anyStringContains)
			       || x.crew.description.Contains(anyStringContains)
			       || x.crew.notes.Contains(anyStringContains)
			       || x.crew.color.Contains(anyStringContains)
			       || x.crew.avatarFileName.Contains(anyStringContains)
			       || x.crew.avatarMimeType.Contains(anyStringContains)
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
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.color);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.CrewMember.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/CrewMember/CreateAuditEvent")]
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
