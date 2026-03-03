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
    /// This auto generated class provides the basic CRUD operations for the UserSetOwnership entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the UserSetOwnership entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class UserSetOwnershipsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 10;

		private BMCContext _context;

		private ILogger<UserSetOwnershipsController> _logger;

		public UserSetOwnershipsController(BMCContext context, ILogger<UserSetOwnershipsController> logger) : base("BMC", "UserSetOwnership")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of UserSetOwnerships filtered by the parameters provided.
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
		[Route("api/UserSetOwnerships")]
		public async Task<IActionResult> GetUserSetOwnerships(
			int? legoSetId = null,
			string status = null,
			DateTime? acquiredDate = null,
			int? personalRating = null,
			string notes = null,
			int? quantity = null,
			bool? isPublic = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
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
			if (acquiredDate.HasValue == true && acquiredDate.Value.Kind != DateTimeKind.Utc)
			{
				acquiredDate = acquiredDate.Value.ToUniversalTime();
			}

			IQueryable<Database.UserSetOwnership> query = (from uso in _context.UserSetOwnerships select uso);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (legoSetId.HasValue == true)
			{
				query = query.Where(uso => uso.legoSetId == legoSetId.Value);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(uso => uso.status == status);
			}
			if (acquiredDate.HasValue == true)
			{
				query = query.Where(uso => uso.acquiredDate == acquiredDate.Value);
			}
			if (personalRating.HasValue == true)
			{
				query = query.Where(uso => uso.personalRating == personalRating.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(uso => uso.notes == notes);
			}
			if (quantity.HasValue == true)
			{
				query = query.Where(uso => uso.quantity == quantity.Value);
			}
			if (isPublic.HasValue == true)
			{
				query = query.Where(uso => uso.isPublic == isPublic.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(uso => uso.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(uso => uso.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(uso => uso.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(uso => uso.deleted == false);
				}
			}
			else
			{
				query = query.Where(uso => uso.active == true);
				query = query.Where(uso => uso.deleted == false);
			}

			query = query.OrderBy(uso => uso.status);


			//
			// Add the any string contains parameter to span all the string fields on the User Set Ownership, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.status.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || (includeRelations == true && x.legoSet.name.Contains(anyStringContains))
			       || (includeRelations == true && x.legoSet.setNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.legoSet.imageUrl.Contains(anyStringContains))
			       || (includeRelations == true && x.legoSet.brickLinkUrl.Contains(anyStringContains))
			       || (includeRelations == true && x.legoSet.rebrickableUrl.Contains(anyStringContains))
			       || (includeRelations == true && x.legoSet.rebrickableSetNum.Contains(anyStringContains))
			       || (includeRelations == true && x.legoSet.brickSetUrl.Contains(anyStringContains))
			       || (includeRelations == true && x.legoSet.instructionsUrl.Contains(anyStringContains))
			       || (includeRelations == true && x.legoSet.subtheme.Contains(anyStringContains))
			       || (includeRelations == true && x.legoSet.availability.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.legoSet);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.UserSetOwnership> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.UserSetOwnership userSetOwnership in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(userSetOwnership, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.UserSetOwnership Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.UserSetOwnership Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of UserSetOwnerships filtered by the parameters provided.  Its query is similar to the GetUserSetOwnerships method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserSetOwnerships/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? legoSetId = null,
			string status = null,
			DateTime? acquiredDate = null,
			int? personalRating = null,
			string notes = null,
			int? quantity = null,
			bool? isPublic = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
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
			if (acquiredDate.HasValue == true && acquiredDate.Value.Kind != DateTimeKind.Utc)
			{
				acquiredDate = acquiredDate.Value.ToUniversalTime();
			}

			IQueryable<Database.UserSetOwnership> query = (from uso in _context.UserSetOwnerships select uso);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (legoSetId.HasValue == true)
			{
				query = query.Where(uso => uso.legoSetId == legoSetId.Value);
			}
			if (status != null)
			{
				query = query.Where(uso => uso.status == status);
			}
			if (acquiredDate.HasValue == true)
			{
				query = query.Where(uso => uso.acquiredDate == acquiredDate.Value);
			}
			if (personalRating.HasValue == true)
			{
				query = query.Where(uso => uso.personalRating == personalRating.Value);
			}
			if (notes != null)
			{
				query = query.Where(uso => uso.notes == notes);
			}
			if (quantity.HasValue == true)
			{
				query = query.Where(uso => uso.quantity == quantity.Value);
			}
			if (isPublic.HasValue == true)
			{
				query = query.Where(uso => uso.isPublic == isPublic.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(uso => uso.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(uso => uso.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(uso => uso.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(uso => uso.deleted == false);
				}
			}
			else
			{
				query = query.Where(uso => uso.active == true);
				query = query.Where(uso => uso.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the User Set Ownership, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.status.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.legoSet.name.Contains(anyStringContains)
			       || x.legoSet.setNumber.Contains(anyStringContains)
			       || x.legoSet.imageUrl.Contains(anyStringContains)
			       || x.legoSet.brickLinkUrl.Contains(anyStringContains)
			       || x.legoSet.rebrickableUrl.Contains(anyStringContains)
			       || x.legoSet.rebrickableSetNum.Contains(anyStringContains)
			       || x.legoSet.brickSetUrl.Contains(anyStringContains)
			       || x.legoSet.instructionsUrl.Contains(anyStringContains)
			       || x.legoSet.subtheme.Contains(anyStringContains)
			       || x.legoSet.availability.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single UserSetOwnership by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserSetOwnership/{id}")]
		public async Task<IActionResult> GetUserSetOwnership(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
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
				IQueryable<Database.UserSetOwnership> query = (from uso in _context.UserSetOwnerships where
							(uso.id == id) &&
							(userIsAdmin == true || uso.deleted == false) &&
							(userIsWriter == true || uso.active == true)
					select uso);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.legoSet);
					query = query.AsSplitQuery();
				}

				Database.UserSetOwnership materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.UserSetOwnership Entity was read with Admin privilege." : "BMC.UserSetOwnership Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserSetOwnership", materialized.id, materialized.status));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.UserSetOwnership entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.UserSetOwnership.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.UserSetOwnership.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing UserSetOwnership record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/UserSetOwnership/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutUserSetOwnership(int id, [FromBody]Database.UserSetOwnership.UserSetOwnershipDTO userSetOwnershipDTO, CancellationToken cancellationToken = default)
		{
			if (userSetOwnershipDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Collection Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != userSetOwnershipDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
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


			IQueryable<Database.UserSetOwnership> query = (from x in _context.UserSetOwnerships
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserSetOwnership existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserSetOwnership PUT", id.ToString(), new Exception("No BMC.UserSetOwnership entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (userSetOwnershipDTO.objectGuid == Guid.Empty)
            {
                userSetOwnershipDTO.objectGuid = existing.objectGuid;
            }
            else if (userSetOwnershipDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a UserSetOwnership record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.UserSetOwnership cloneOfExisting = (Database.UserSetOwnership)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new UserSetOwnership object using the data from the existing record, updated with what is in the DTO.
			//
			Database.UserSetOwnership userSetOwnership = (Database.UserSetOwnership)_context.Entry(existing).GetDatabaseValues().ToObject();
			userSetOwnership.ApplyDTO(userSetOwnershipDTO);
			//
			// The tenant guid for any UserSetOwnership being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the UserSetOwnership because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				userSetOwnership.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (userSetOwnership.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.UserSetOwnership record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (userSetOwnership.status != null && userSetOwnership.status.Length > 50)
			{
				userSetOwnership.status = userSetOwnership.status.Substring(0, 50);
			}

			if (userSetOwnership.acquiredDate.HasValue == true && userSetOwnership.acquiredDate.Value.Kind != DateTimeKind.Utc)
			{
				userSetOwnership.acquiredDate = userSetOwnership.acquiredDate.Value.ToUniversalTime();
			}

			EntityEntry<Database.UserSetOwnership> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(userSetOwnership);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.UserSetOwnership entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserSetOwnership.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserSetOwnership.CreateAnonymousWithFirstLevelSubObjects(userSetOwnership)),
					null);


				return Ok(Database.UserSetOwnership.CreateAnonymous(userSetOwnership));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.UserSetOwnership entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserSetOwnership.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserSetOwnership.CreateAnonymousWithFirstLevelSubObjects(userSetOwnership)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new UserSetOwnership record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserSetOwnership", Name = "UserSetOwnership")]
		public async Task<IActionResult> PostUserSetOwnership([FromBody]Database.UserSetOwnership.UserSetOwnershipDTO userSetOwnershipDTO, CancellationToken cancellationToken = default)
		{
			if (userSetOwnershipDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Collection Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new UserSetOwnership object using the data from the DTO
			//
			Database.UserSetOwnership userSetOwnership = Database.UserSetOwnership.FromDTO(userSetOwnershipDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				userSetOwnership.tenantGuid = userTenantGuid;

				if (userSetOwnership.status != null && userSetOwnership.status.Length > 50)
				{
					userSetOwnership.status = userSetOwnership.status.Substring(0, 50);
				}

				if (userSetOwnership.acquiredDate.HasValue == true && userSetOwnership.acquiredDate.Value.Kind != DateTimeKind.Utc)
				{
					userSetOwnership.acquiredDate = userSetOwnership.acquiredDate.Value.ToUniversalTime();
				}

				userSetOwnership.objectGuid = Guid.NewGuid();
				_context.UserSetOwnerships.Add(userSetOwnership);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.UserSetOwnership entity successfully created.",
					true,
					userSetOwnership.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.UserSetOwnership.CreateAnonymousWithFirstLevelSubObjects(userSetOwnership)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.UserSetOwnership entity creation failed.", false, userSetOwnership.id.ToString(), "", JsonSerializer.Serialize(userSetOwnership), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserSetOwnership", userSetOwnership.id, userSetOwnership.status));

			return CreatedAtRoute("UserSetOwnership", new { id = userSetOwnership.id }, Database.UserSetOwnership.CreateAnonymousWithFirstLevelSubObjects(userSetOwnership));
		}



        /// <summary>
        /// 
        /// This deletes a UserSetOwnership record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserSetOwnership/{id}")]
		[Route("api/UserSetOwnership")]
		public async Task<IActionResult> DeleteUserSetOwnership(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Collection Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.UserSetOwnership> query = (from x in _context.UserSetOwnerships
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserSetOwnership userSetOwnership = await query.FirstOrDefaultAsync(cancellationToken);

			if (userSetOwnership == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserSetOwnership DELETE", id.ToString(), new Exception("No BMC.UserSetOwnership entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.UserSetOwnership cloneOfExisting = (Database.UserSetOwnership)_context.Entry(userSetOwnership).GetDatabaseValues().ToObject();


			try
			{
				userSetOwnership.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.UserSetOwnership entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserSetOwnership.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserSetOwnership.CreateAnonymousWithFirstLevelSubObjects(userSetOwnership)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.UserSetOwnership entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserSetOwnership.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserSetOwnership.CreateAnonymousWithFirstLevelSubObjects(userSetOwnership)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of UserSetOwnership records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/UserSetOwnerships/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? legoSetId = null,
			string status = null,
			DateTime? acquiredDate = null,
			int? personalRating = null,
			string notes = null,
			int? quantity = null,
			bool? isPublic = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);


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
			if (acquiredDate.HasValue == true && acquiredDate.Value.Kind != DateTimeKind.Utc)
			{
				acquiredDate = acquiredDate.Value.ToUniversalTime();
			}

			IQueryable<Database.UserSetOwnership> query = (from uso in _context.UserSetOwnerships select uso);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (legoSetId.HasValue == true)
			{
				query = query.Where(uso => uso.legoSetId == legoSetId.Value);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(uso => uso.status == status);
			}
			if (acquiredDate.HasValue == true)
			{
				query = query.Where(uso => uso.acquiredDate == acquiredDate.Value);
			}
			if (personalRating.HasValue == true)
			{
				query = query.Where(uso => uso.personalRating == personalRating.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(uso => uso.notes == notes);
			}
			if (quantity.HasValue == true)
			{
				query = query.Where(uso => uso.quantity == quantity.Value);
			}
			if (isPublic.HasValue == true)
			{
				query = query.Where(uso => uso.isPublic == isPublic.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(uso => uso.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(uso => uso.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(uso => uso.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(uso => uso.deleted == false);
				}
			}
			else
			{
				query = query.Where(uso => uso.active == true);
				query = query.Where(uso => uso.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Set Ownership, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.status.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.legoSet.name.Contains(anyStringContains)
			       || x.legoSet.setNumber.Contains(anyStringContains)
			       || x.legoSet.imageUrl.Contains(anyStringContains)
			       || x.legoSet.brickLinkUrl.Contains(anyStringContains)
			       || x.legoSet.rebrickableUrl.Contains(anyStringContains)
			       || x.legoSet.rebrickableSetNum.Contains(anyStringContains)
			       || x.legoSet.brickSetUrl.Contains(anyStringContains)
			       || x.legoSet.instructionsUrl.Contains(anyStringContains)
			       || x.legoSet.subtheme.Contains(anyStringContains)
			       || x.legoSet.availability.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.status);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.UserSetOwnership.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/UserSetOwnership/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Collection Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
