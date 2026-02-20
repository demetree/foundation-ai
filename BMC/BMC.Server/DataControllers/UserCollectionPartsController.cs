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
    /// This auto generated class provides the basic CRUD operations for the UserCollectionPart entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the UserCollectionPart entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class UserCollectionPartsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 10;

		private BMCContext _context;

		private ILogger<UserCollectionPartsController> _logger;

		public UserCollectionPartsController(BMCContext context, ILogger<UserCollectionPartsController> logger) : base("BMC", "UserCollectionPart")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of UserCollectionParts filtered by the parameters provided.
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
		[Route("api/UserCollectionParts")]
		public async Task<IActionResult> GetUserCollectionParts(
			int? userCollectionId = null,
			int? brickPartId = null,
			int? brickColourId = null,
			int? quantityOwned = null,
			int? quantityUsed = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			int? pageSize = null,
			int? pageNumber = null,
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

			IQueryable<Database.UserCollectionPart> query = (from ucp in _context.UserCollectionParts select ucp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userCollectionId.HasValue == true)
			{
				query = query.Where(ucp => ucp.userCollectionId == userCollectionId.Value);
			}
			if (brickPartId.HasValue == true)
			{
				query = query.Where(ucp => ucp.brickPartId == brickPartId.Value);
			}
			if (brickColourId.HasValue == true)
			{
				query = query.Where(ucp => ucp.brickColourId == brickColourId.Value);
			}
			if (quantityOwned.HasValue == true)
			{
				query = query.Where(ucp => ucp.quantityOwned == quantityOwned.Value);
			}
			if (quantityUsed.HasValue == true)
			{
				query = query.Where(ucp => ucp.quantityUsed == quantityUsed.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ucp => ucp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ucp => ucp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ucp => ucp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ucp => ucp.deleted == false);
				}
			}
			else
			{
				query = query.Where(ucp => ucp.active == true);
				query = query.Where(ucp => ucp.deleted == false);
			}

			query = query.OrderBy(ucp => ucp.id);

			if (includeRelations == true)
			{
				query = query.Include(x => x.brickColour);
				query = query.Include(x => x.brickPart);
				query = query.Include(x => x.userCollection);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.UserCollectionPart> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.UserCollectionPart userCollectionPart in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(userCollectionPart, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.UserCollectionPart Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.UserCollectionPart Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of UserCollectionParts filtered by the parameters provided.  Its query is similar to the GetUserCollectionParts method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserCollectionParts/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? userCollectionId = null,
			int? brickPartId = null,
			int? brickColourId = null,
			int? quantityOwned = null,
			int? quantityUsed = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
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


			IQueryable<Database.UserCollectionPart> query = (from ucp in _context.UserCollectionParts select ucp);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (userCollectionId.HasValue == true)
			{
				query = query.Where(ucp => ucp.userCollectionId == userCollectionId.Value);
			}
			if (brickPartId.HasValue == true)
			{
				query = query.Where(ucp => ucp.brickPartId == brickPartId.Value);
			}
			if (brickColourId.HasValue == true)
			{
				query = query.Where(ucp => ucp.brickColourId == brickColourId.Value);
			}
			if (quantityOwned.HasValue == true)
			{
				query = query.Where(ucp => ucp.quantityOwned == quantityOwned.Value);
			}
			if (quantityUsed.HasValue == true)
			{
				query = query.Where(ucp => ucp.quantityUsed == quantityUsed.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ucp => ucp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ucp => ucp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ucp => ucp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ucp => ucp.deleted == false);
				}
			}
			else
			{
				query = query.Where(ucp => ucp.active == true);
				query = query.Where(ucp => ucp.deleted == false);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single UserCollectionPart by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserCollectionPart/{id}")]
		public async Task<IActionResult> GetUserCollectionPart(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.UserCollectionPart> query = (from ucp in _context.UserCollectionParts where
							(ucp.id == id) &&
							(userIsAdmin == true || ucp.deleted == false) &&
							(userIsWriter == true || ucp.active == true)
					select ucp);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.brickColour);
					query = query.Include(x => x.brickPart);
					query = query.Include(x => x.userCollection);
					query = query.AsSplitQuery();
				}

				Database.UserCollectionPart materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.UserCollectionPart Entity was read with Admin privilege." : "BMC.UserCollectionPart Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserCollectionPart", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.UserCollectionPart entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.UserCollectionPart.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.UserCollectionPart.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing UserCollectionPart record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/UserCollectionPart/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutUserCollectionPart(int id, [FromBody]Database.UserCollectionPart.UserCollectionPartDTO userCollectionPartDTO, CancellationToken cancellationToken = default)
		{
			if (userCollectionPartDTO == null)
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



			if (id != userCollectionPartDTO.id)
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


			IQueryable<Database.UserCollectionPart> query = (from x in _context.UserCollectionParts
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserCollectionPart existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserCollectionPart PUT", id.ToString(), new Exception("No BMC.UserCollectionPart entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (userCollectionPartDTO.objectGuid == Guid.Empty)
            {
                userCollectionPartDTO.objectGuid = existing.objectGuid;
            }
            else if (userCollectionPartDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a UserCollectionPart record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.UserCollectionPart cloneOfExisting = (Database.UserCollectionPart)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new UserCollectionPart object using the data from the existing record, updated with what is in the DTO.
			//
			Database.UserCollectionPart userCollectionPart = (Database.UserCollectionPart)_context.Entry(existing).GetDatabaseValues().ToObject();
			userCollectionPart.ApplyDTO(userCollectionPartDTO);
			//
			// The tenant guid for any UserCollectionPart being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the UserCollectionPart because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				userCollectionPart.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (userCollectionPart.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.UserCollectionPart record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			EntityEntry<Database.UserCollectionPart> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(userCollectionPart);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.UserCollectionPart entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserCollectionPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserCollectionPart.CreateAnonymousWithFirstLevelSubObjects(userCollectionPart)),
					null);


				return Ok(Database.UserCollectionPart.CreateAnonymous(userCollectionPart));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.UserCollectionPart entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserCollectionPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserCollectionPart.CreateAnonymousWithFirstLevelSubObjects(userCollectionPart)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new UserCollectionPart record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserCollectionPart", Name = "UserCollectionPart")]
		public async Task<IActionResult> PostUserCollectionPart([FromBody]Database.UserCollectionPart.UserCollectionPartDTO userCollectionPartDTO, CancellationToken cancellationToken = default)
		{
			if (userCollectionPartDTO == null)
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
			// Create a new UserCollectionPart object using the data from the DTO
			//
			Database.UserCollectionPart userCollectionPart = Database.UserCollectionPart.FromDTO(userCollectionPartDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				userCollectionPart.tenantGuid = userTenantGuid;

				userCollectionPart.objectGuid = Guid.NewGuid();
				_context.UserCollectionParts.Add(userCollectionPart);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.UserCollectionPart entity successfully created.",
					true,
					userCollectionPart.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.UserCollectionPart.CreateAnonymousWithFirstLevelSubObjects(userCollectionPart)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.UserCollectionPart entity creation failed.", false, userCollectionPart.id.ToString(), "", JsonSerializer.Serialize(userCollectionPart), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserCollectionPart", userCollectionPart.id, userCollectionPart.id.ToString()));

			return CreatedAtRoute("UserCollectionPart", new { id = userCollectionPart.id }, Database.UserCollectionPart.CreateAnonymousWithFirstLevelSubObjects(userCollectionPart));
		}



        /// <summary>
        /// 
        /// This deletes a UserCollectionPart record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserCollectionPart/{id}")]
		[Route("api/UserCollectionPart")]
		public async Task<IActionResult> DeleteUserCollectionPart(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.UserCollectionPart> query = (from x in _context.UserCollectionParts
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserCollectionPart userCollectionPart = await query.FirstOrDefaultAsync(cancellationToken);

			if (userCollectionPart == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserCollectionPart DELETE", id.ToString(), new Exception("No BMC.UserCollectionPart entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.UserCollectionPart cloneOfExisting = (Database.UserCollectionPart)_context.Entry(userCollectionPart).GetDatabaseValues().ToObject();


			try
			{
				userCollectionPart.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.UserCollectionPart entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserCollectionPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserCollectionPart.CreateAnonymousWithFirstLevelSubObjects(userCollectionPart)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.UserCollectionPart entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserCollectionPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserCollectionPart.CreateAnonymousWithFirstLevelSubObjects(userCollectionPart)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of UserCollectionPart records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/UserCollectionParts/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? userCollectionId = null,
			int? brickPartId = null,
			int? brickColourId = null,
			int? quantityOwned = null,
			int? quantityUsed = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
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

			IQueryable<Database.UserCollectionPart> query = (from ucp in _context.UserCollectionParts select ucp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userCollectionId.HasValue == true)
			{
				query = query.Where(ucp => ucp.userCollectionId == userCollectionId.Value);
			}
			if (brickPartId.HasValue == true)
			{
				query = query.Where(ucp => ucp.brickPartId == brickPartId.Value);
			}
			if (brickColourId.HasValue == true)
			{
				query = query.Where(ucp => ucp.brickColourId == brickColourId.Value);
			}
			if (quantityOwned.HasValue == true)
			{
				query = query.Where(ucp => ucp.quantityOwned == quantityOwned.Value);
			}
			if (quantityUsed.HasValue == true)
			{
				query = query.Where(ucp => ucp.quantityUsed == quantityUsed.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ucp => ucp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ucp => ucp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ucp => ucp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ucp => ucp.deleted == false);
				}
			}
			else
			{
				query = query.Where(ucp => ucp.active == true);
				query = query.Where(ucp => ucp.deleted == false);
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.UserCollectionPart.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/UserCollectionPart/CreateAuditEvent")]
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
