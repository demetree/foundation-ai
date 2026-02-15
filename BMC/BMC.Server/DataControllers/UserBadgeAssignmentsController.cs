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
using Foundation.BMC.Database;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the UserBadgeAssignment entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the UserBadgeAssignment entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class UserBadgeAssignmentsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		private BMCContext _context;

		private ILogger<UserBadgeAssignmentsController> _logger;

		public UserBadgeAssignmentsController(BMCContext context, ILogger<UserBadgeAssignmentsController> logger) : base("BMC", "UserBadgeAssignment")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of UserBadgeAssignments filtered by the parameters provided.
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
		[Route("api/UserBadgeAssignments")]
		public async Task<IActionResult> GetUserBadgeAssignments(
			int? userBadgeId = null,
			DateTime? awardedDate = null,
			Guid? awardedByTenantGuid = null,
			string reason = null,
			bool? isDisplayed = null,
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
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
			if (awardedDate.HasValue == true && awardedDate.Value.Kind != DateTimeKind.Utc)
			{
				awardedDate = awardedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.UserBadgeAssignment> query = (from uba in _context.UserBadgeAssignments select uba);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userBadgeId.HasValue == true)
			{
				query = query.Where(uba => uba.userBadgeId == userBadgeId.Value);
			}
			if (awardedDate.HasValue == true)
			{
				query = query.Where(uba => uba.awardedDate == awardedDate.Value);
			}
			if (awardedByTenantGuid.HasValue == true)
			{
				query = query.Where(uba => uba.awardedByTenantGuid == awardedByTenantGuid);
			}
			if (string.IsNullOrEmpty(reason) == false)
			{
				query = query.Where(uba => uba.reason == reason);
			}
			if (isDisplayed.HasValue == true)
			{
				query = query.Where(uba => uba.isDisplayed == isDisplayed.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(uba => uba.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(uba => uba.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(uba => uba.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(uba => uba.deleted == false);
				}
			}
			else
			{
				query = query.Where(uba => uba.active == true);
				query = query.Where(uba => uba.deleted == false);
			}

			query = query.OrderBy(uba => uba.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.userBadge);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Badge Assignment, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.reason.Contains(anyStringContains)
			       || (includeRelations == true && x.userBadge.name.Contains(anyStringContains))
			       || (includeRelations == true && x.userBadge.description.Contains(anyStringContains))
			       || (includeRelations == true && x.userBadge.iconCssClass.Contains(anyStringContains))
			       || (includeRelations == true && x.userBadge.iconImagePath.Contains(anyStringContains))
			       || (includeRelations == true && x.userBadge.badgeColor.Contains(anyStringContains))
			       || (includeRelations == true && x.userBadge.automaticCriteriaCode.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.UserBadgeAssignment> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.UserBadgeAssignment userBadgeAssignment in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(userBadgeAssignment, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.UserBadgeAssignment Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.UserBadgeAssignment Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of UserBadgeAssignments filtered by the parameters provided.  Its query is similar to the GetUserBadgeAssignments method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserBadgeAssignments/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? userBadgeId = null,
			DateTime? awardedDate = null,
			Guid? awardedByTenantGuid = null,
			string reason = null,
			bool? isDisplayed = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
			if (awardedDate.HasValue == true && awardedDate.Value.Kind != DateTimeKind.Utc)
			{
				awardedDate = awardedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.UserBadgeAssignment> query = (from uba in _context.UserBadgeAssignments select uba);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (userBadgeId.HasValue == true)
			{
				query = query.Where(uba => uba.userBadgeId == userBadgeId.Value);
			}
			if (awardedDate.HasValue == true)
			{
				query = query.Where(uba => uba.awardedDate == awardedDate.Value);
			}
			if (awardedByTenantGuid.HasValue == true)
			{
				query = query.Where(uba => uba.awardedByTenantGuid == awardedByTenantGuid);
			}
			if (reason != null)
			{
				query = query.Where(uba => uba.reason == reason);
			}
			if (isDisplayed.HasValue == true)
			{
				query = query.Where(uba => uba.isDisplayed == isDisplayed.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(uba => uba.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(uba => uba.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(uba => uba.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(uba => uba.deleted == false);
				}
			}
			else
			{
				query = query.Where(uba => uba.active == true);
				query = query.Where(uba => uba.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the User Badge Assignment, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.reason.Contains(anyStringContains)
			       || x.userBadge.name.Contains(anyStringContains)
			       || x.userBadge.description.Contains(anyStringContains)
			       || x.userBadge.iconCssClass.Contains(anyStringContains)
			       || x.userBadge.iconImagePath.Contains(anyStringContains)
			       || x.userBadge.badgeColor.Contains(anyStringContains)
			       || x.userBadge.automaticCriteriaCode.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single UserBadgeAssignment by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserBadgeAssignment/{id}")]
		public async Task<IActionResult> GetUserBadgeAssignment(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
				IQueryable<Database.UserBadgeAssignment> query = (from uba in _context.UserBadgeAssignments where
							(uba.id == id) &&
							(userIsAdmin == true || uba.deleted == false) &&
							(userIsWriter == true || uba.active == true)
					select uba);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.userBadge);
					query = query.AsSplitQuery();
				}

				Database.UserBadgeAssignment materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.UserBadgeAssignment Entity was read with Admin privilege." : "BMC.UserBadgeAssignment Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserBadgeAssignment", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.UserBadgeAssignment entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.UserBadgeAssignment.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.UserBadgeAssignment.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing UserBadgeAssignment record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/UserBadgeAssignment/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutUserBadgeAssignment(int id, [FromBody]Database.UserBadgeAssignment.UserBadgeAssignmentDTO userBadgeAssignmentDTO, CancellationToken cancellationToken = default)
		{
			if (userBadgeAssignmentDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != userBadgeAssignmentDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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


			IQueryable<Database.UserBadgeAssignment> query = (from x in _context.UserBadgeAssignments
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserBadgeAssignment existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserBadgeAssignment PUT", id.ToString(), new Exception("No BMC.UserBadgeAssignment entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (userBadgeAssignmentDTO.objectGuid == Guid.Empty)
            {
                userBadgeAssignmentDTO.objectGuid = existing.objectGuid;
            }
            else if (userBadgeAssignmentDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a UserBadgeAssignment record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.UserBadgeAssignment cloneOfExisting = (Database.UserBadgeAssignment)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new UserBadgeAssignment object using the data from the existing record, updated with what is in the DTO.
			//
			Database.UserBadgeAssignment userBadgeAssignment = (Database.UserBadgeAssignment)_context.Entry(existing).GetDatabaseValues().ToObject();
			userBadgeAssignment.ApplyDTO(userBadgeAssignmentDTO);
			//
			// The tenant guid for any UserBadgeAssignment being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the UserBadgeAssignment because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				userBadgeAssignment.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (userBadgeAssignment.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.UserBadgeAssignment record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (userBadgeAssignment.awardedDate.Kind != DateTimeKind.Utc)
			{
				userBadgeAssignment.awardedDate = userBadgeAssignment.awardedDate.ToUniversalTime();
			}

			EntityEntry<Database.UserBadgeAssignment> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(userBadgeAssignment);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.UserBadgeAssignment entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserBadgeAssignment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserBadgeAssignment.CreateAnonymousWithFirstLevelSubObjects(userBadgeAssignment)),
					null);


				return Ok(Database.UserBadgeAssignment.CreateAnonymous(userBadgeAssignment));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.UserBadgeAssignment entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserBadgeAssignment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserBadgeAssignment.CreateAnonymousWithFirstLevelSubObjects(userBadgeAssignment)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new UserBadgeAssignment record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserBadgeAssignment", Name = "UserBadgeAssignment")]
		public async Task<IActionResult> PostUserBadgeAssignment([FromBody]Database.UserBadgeAssignment.UserBadgeAssignmentDTO userBadgeAssignmentDTO, CancellationToken cancellationToken = default)
		{
			if (userBadgeAssignmentDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new UserBadgeAssignment object using the data from the DTO
			//
			Database.UserBadgeAssignment userBadgeAssignment = Database.UserBadgeAssignment.FromDTO(userBadgeAssignmentDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				userBadgeAssignment.tenantGuid = userTenantGuid;

				if (userBadgeAssignment.awardedDate.Kind != DateTimeKind.Utc)
				{
					userBadgeAssignment.awardedDate = userBadgeAssignment.awardedDate.ToUniversalTime();
				}

				userBadgeAssignment.objectGuid = Guid.NewGuid();
				_context.UserBadgeAssignments.Add(userBadgeAssignment);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.UserBadgeAssignment entity successfully created.",
					true,
					userBadgeAssignment.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.UserBadgeAssignment.CreateAnonymousWithFirstLevelSubObjects(userBadgeAssignment)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.UserBadgeAssignment entity creation failed.", false, userBadgeAssignment.id.ToString(), "", JsonSerializer.Serialize(userBadgeAssignment), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserBadgeAssignment", userBadgeAssignment.id, userBadgeAssignment.id.ToString()));

			return CreatedAtRoute("UserBadgeAssignment", new { id = userBadgeAssignment.id }, Database.UserBadgeAssignment.CreateAnonymousWithFirstLevelSubObjects(userBadgeAssignment));
		}



        /// <summary>
        /// 
        /// This deletes a UserBadgeAssignment record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserBadgeAssignment/{id}")]
		[Route("api/UserBadgeAssignment")]
		public async Task<IActionResult> DeleteUserBadgeAssignment(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.UserBadgeAssignment> query = (from x in _context.UserBadgeAssignments
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserBadgeAssignment userBadgeAssignment = await query.FirstOrDefaultAsync(cancellationToken);

			if (userBadgeAssignment == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserBadgeAssignment DELETE", id.ToString(), new Exception("No BMC.UserBadgeAssignment entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.UserBadgeAssignment cloneOfExisting = (Database.UserBadgeAssignment)_context.Entry(userBadgeAssignment).GetDatabaseValues().ToObject();


			try
			{
				userBadgeAssignment.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.UserBadgeAssignment entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserBadgeAssignment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserBadgeAssignment.CreateAnonymousWithFirstLevelSubObjects(userBadgeAssignment)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.UserBadgeAssignment entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserBadgeAssignment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserBadgeAssignment.CreateAnonymousWithFirstLevelSubObjects(userBadgeAssignment)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of UserBadgeAssignment records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/UserBadgeAssignments/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? userBadgeId = null,
			DateTime? awardedDate = null,
			Guid? awardedByTenantGuid = null,
			string reason = null,
			bool? isDisplayed = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);


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
			if (awardedDate.HasValue == true && awardedDate.Value.Kind != DateTimeKind.Utc)
			{
				awardedDate = awardedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.UserBadgeAssignment> query = (from uba in _context.UserBadgeAssignments select uba);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userBadgeId.HasValue == true)
			{
				query = query.Where(uba => uba.userBadgeId == userBadgeId.Value);
			}
			if (awardedDate.HasValue == true)
			{
				query = query.Where(uba => uba.awardedDate == awardedDate.Value);
			}
			if (awardedByTenantGuid.HasValue == true)
			{
				query = query.Where(uba => uba.awardedByTenantGuid == awardedByTenantGuid);
			}
			if (string.IsNullOrEmpty(reason) == false)
			{
				query = query.Where(uba => uba.reason == reason);
			}
			if (isDisplayed.HasValue == true)
			{
				query = query.Where(uba => uba.isDisplayed == isDisplayed.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(uba => uba.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(uba => uba.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(uba => uba.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(uba => uba.deleted == false);
				}
			}
			else
			{
				query = query.Where(uba => uba.active == true);
				query = query.Where(uba => uba.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Badge Assignment, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.reason.Contains(anyStringContains)
			       || x.userBadge.name.Contains(anyStringContains)
			       || x.userBadge.description.Contains(anyStringContains)
			       || x.userBadge.iconCssClass.Contains(anyStringContains)
			       || x.userBadge.iconImagePath.Contains(anyStringContains)
			       || x.userBadge.badgeColor.Contains(anyStringContains)
			       || x.userBadge.automaticCriteriaCode.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.UserBadgeAssignment.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/UserBadgeAssignment/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
