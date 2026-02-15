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
    /// This auto generated class provides the basic CRUD operations for the UserWishlistItem entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the UserWishlistItem entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class UserWishlistItemsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 10;

		private BMCContext _context;

		private ILogger<UserWishlistItemsController> _logger;

		public UserWishlistItemsController(BMCContext context, ILogger<UserWishlistItemsController> logger) : base("BMC", "UserWishlistItem")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of UserWishlistItems filtered by the parameters provided.
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
		[Route("api/UserWishlistItems")]
		public async Task<IActionResult> GetUserWishlistItems(
			int? userCollectionId = null,
			int? brickPartId = null,
			int? brickColourId = null,
			int? quantityDesired = null,
			string notes = null,
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

			IQueryable<Database.UserWishlistItem> query = (from uwi in _context.UserWishlistItems select uwi);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userCollectionId.HasValue == true)
			{
				query = query.Where(uwi => uwi.userCollectionId == userCollectionId.Value);
			}
			if (brickPartId.HasValue == true)
			{
				query = query.Where(uwi => uwi.brickPartId == brickPartId.Value);
			}
			if (brickColourId.HasValue == true)
			{
				query = query.Where(uwi => uwi.brickColourId == brickColourId.Value);
			}
			if (quantityDesired.HasValue == true)
			{
				query = query.Where(uwi => uwi.quantityDesired == quantityDesired.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(uwi => uwi.notes == notes);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(uwi => uwi.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(uwi => uwi.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(uwi => uwi.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(uwi => uwi.deleted == false);
				}
			}
			else
			{
				query = query.Where(uwi => uwi.active == true);
				query = query.Where(uwi => uwi.deleted == false);
			}

			query = query.OrderBy(uwi => uwi.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.brickColour);
				query = query.Include(x => x.brickPart);
				query = query.Include(x => x.userCollection);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Wishlist Item, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.notes.Contains(anyStringContains)
			       || (includeRelations == true && x.brickColour.name.Contains(anyStringContains))
			       || (includeRelations == true && x.brickColour.hexRgb.Contains(anyStringContains))
			       || (includeRelations == true && x.brickColour.hexEdgeColour.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.name.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.ldrawPartId.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.ldrawTitle.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.ldrawCategory.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.keywords.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.author.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.geometryFilePath.Contains(anyStringContains))
			       || (includeRelations == true && x.userCollection.name.Contains(anyStringContains))
			       || (includeRelations == true && x.userCollection.description.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.UserWishlistItem> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.UserWishlistItem userWishlistItem in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(userWishlistItem, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.UserWishlistItem Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.UserWishlistItem Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of UserWishlistItems filtered by the parameters provided.  Its query is similar to the GetUserWishlistItems method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserWishlistItems/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? userCollectionId = null,
			int? brickPartId = null,
			int? brickColourId = null,
			int? quantityDesired = null,
			string notes = null,
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


			IQueryable<Database.UserWishlistItem> query = (from uwi in _context.UserWishlistItems select uwi);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (userCollectionId.HasValue == true)
			{
				query = query.Where(uwi => uwi.userCollectionId == userCollectionId.Value);
			}
			if (brickPartId.HasValue == true)
			{
				query = query.Where(uwi => uwi.brickPartId == brickPartId.Value);
			}
			if (brickColourId.HasValue == true)
			{
				query = query.Where(uwi => uwi.brickColourId == brickColourId.Value);
			}
			if (quantityDesired.HasValue == true)
			{
				query = query.Where(uwi => uwi.quantityDesired == quantityDesired.Value);
			}
			if (notes != null)
			{
				query = query.Where(uwi => uwi.notes == notes);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(uwi => uwi.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(uwi => uwi.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(uwi => uwi.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(uwi => uwi.deleted == false);
				}
			}
			else
			{
				query = query.Where(uwi => uwi.active == true);
				query = query.Where(uwi => uwi.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the User Wishlist Item, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.notes.Contains(anyStringContains)
			       || x.brickColour.name.Contains(anyStringContains)
			       || x.brickColour.hexRgb.Contains(anyStringContains)
			       || x.brickColour.hexEdgeColour.Contains(anyStringContains)
			       || x.brickPart.name.Contains(anyStringContains)
			       || x.brickPart.ldrawPartId.Contains(anyStringContains)
			       || x.brickPart.ldrawTitle.Contains(anyStringContains)
			       || x.brickPart.ldrawCategory.Contains(anyStringContains)
			       || x.brickPart.keywords.Contains(anyStringContains)
			       || x.brickPart.author.Contains(anyStringContains)
			       || x.brickPart.geometryFilePath.Contains(anyStringContains)
			       || x.userCollection.name.Contains(anyStringContains)
			       || x.userCollection.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single UserWishlistItem by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserWishlistItem/{id}")]
		public async Task<IActionResult> GetUserWishlistItem(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.UserWishlistItem> query = (from uwi in _context.UserWishlistItems where
							(uwi.id == id) &&
							(userIsAdmin == true || uwi.deleted == false) &&
							(userIsWriter == true || uwi.active == true)
					select uwi);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.brickColour);
					query = query.Include(x => x.brickPart);
					query = query.Include(x => x.userCollection);
					query = query.AsSplitQuery();
				}

				Database.UserWishlistItem materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.UserWishlistItem Entity was read with Admin privilege." : "BMC.UserWishlistItem Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserWishlistItem", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.UserWishlistItem entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.UserWishlistItem.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.UserWishlistItem.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing UserWishlistItem record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/UserWishlistItem/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutUserWishlistItem(int id, [FromBody]Database.UserWishlistItem.UserWishlistItemDTO userWishlistItemDTO, CancellationToken cancellationToken = default)
		{
			if (userWishlistItemDTO == null)
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



			if (id != userWishlistItemDTO.id)
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


			IQueryable<Database.UserWishlistItem> query = (from x in _context.UserWishlistItems
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserWishlistItem existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserWishlistItem PUT", id.ToString(), new Exception("No BMC.UserWishlistItem entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (userWishlistItemDTO.objectGuid == Guid.Empty)
            {
                userWishlistItemDTO.objectGuid = existing.objectGuid;
            }
            else if (userWishlistItemDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a UserWishlistItem record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.UserWishlistItem cloneOfExisting = (Database.UserWishlistItem)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new UserWishlistItem object using the data from the existing record, updated with what is in the DTO.
			//
			Database.UserWishlistItem userWishlistItem = (Database.UserWishlistItem)_context.Entry(existing).GetDatabaseValues().ToObject();
			userWishlistItem.ApplyDTO(userWishlistItemDTO);
			//
			// The tenant guid for any UserWishlistItem being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the UserWishlistItem because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				userWishlistItem.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (userWishlistItem.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.UserWishlistItem record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			EntityEntry<Database.UserWishlistItem> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(userWishlistItem);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.UserWishlistItem entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserWishlistItem.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserWishlistItem.CreateAnonymousWithFirstLevelSubObjects(userWishlistItem)),
					null);


				return Ok(Database.UserWishlistItem.CreateAnonymous(userWishlistItem));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.UserWishlistItem entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserWishlistItem.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserWishlistItem.CreateAnonymousWithFirstLevelSubObjects(userWishlistItem)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new UserWishlistItem record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserWishlistItem", Name = "UserWishlistItem")]
		public async Task<IActionResult> PostUserWishlistItem([FromBody]Database.UserWishlistItem.UserWishlistItemDTO userWishlistItemDTO, CancellationToken cancellationToken = default)
		{
			if (userWishlistItemDTO == null)
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
			// Create a new UserWishlistItem object using the data from the DTO
			//
			Database.UserWishlistItem userWishlistItem = Database.UserWishlistItem.FromDTO(userWishlistItemDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				userWishlistItem.tenantGuid = userTenantGuid;

				userWishlistItem.objectGuid = Guid.NewGuid();
				_context.UserWishlistItems.Add(userWishlistItem);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.UserWishlistItem entity successfully created.",
					true,
					userWishlistItem.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.UserWishlistItem.CreateAnonymousWithFirstLevelSubObjects(userWishlistItem)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.UserWishlistItem entity creation failed.", false, userWishlistItem.id.ToString(), "", JsonSerializer.Serialize(userWishlistItem), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserWishlistItem", userWishlistItem.id, userWishlistItem.id.ToString()));

			return CreatedAtRoute("UserWishlistItem", new { id = userWishlistItem.id }, Database.UserWishlistItem.CreateAnonymousWithFirstLevelSubObjects(userWishlistItem));
		}



        /// <summary>
        /// 
        /// This deletes a UserWishlistItem record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserWishlistItem/{id}")]
		[Route("api/UserWishlistItem")]
		public async Task<IActionResult> DeleteUserWishlistItem(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.UserWishlistItem> query = (from x in _context.UserWishlistItems
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserWishlistItem userWishlistItem = await query.FirstOrDefaultAsync(cancellationToken);

			if (userWishlistItem == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserWishlistItem DELETE", id.ToString(), new Exception("No BMC.UserWishlistItem entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.UserWishlistItem cloneOfExisting = (Database.UserWishlistItem)_context.Entry(userWishlistItem).GetDatabaseValues().ToObject();


			try
			{
				userWishlistItem.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.UserWishlistItem entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserWishlistItem.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserWishlistItem.CreateAnonymousWithFirstLevelSubObjects(userWishlistItem)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.UserWishlistItem entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserWishlistItem.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserWishlistItem.CreateAnonymousWithFirstLevelSubObjects(userWishlistItem)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of UserWishlistItem records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/UserWishlistItems/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? userCollectionId = null,
			int? brickPartId = null,
			int? brickColourId = null,
			int? quantityDesired = null,
			string notes = null,
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

			IQueryable<Database.UserWishlistItem> query = (from uwi in _context.UserWishlistItems select uwi);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userCollectionId.HasValue == true)
			{
				query = query.Where(uwi => uwi.userCollectionId == userCollectionId.Value);
			}
			if (brickPartId.HasValue == true)
			{
				query = query.Where(uwi => uwi.brickPartId == brickPartId.Value);
			}
			if (brickColourId.HasValue == true)
			{
				query = query.Where(uwi => uwi.brickColourId == brickColourId.Value);
			}
			if (quantityDesired.HasValue == true)
			{
				query = query.Where(uwi => uwi.quantityDesired == quantityDesired.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(uwi => uwi.notes == notes);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(uwi => uwi.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(uwi => uwi.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(uwi => uwi.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(uwi => uwi.deleted == false);
				}
			}
			else
			{
				query = query.Where(uwi => uwi.active == true);
				query = query.Where(uwi => uwi.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Wishlist Item, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.notes.Contains(anyStringContains)
			       || x.brickColour.name.Contains(anyStringContains)
			       || x.brickColour.hexRgb.Contains(anyStringContains)
			       || x.brickColour.hexEdgeColour.Contains(anyStringContains)
			       || x.brickPart.name.Contains(anyStringContains)
			       || x.brickPart.ldrawPartId.Contains(anyStringContains)
			       || x.brickPart.ldrawTitle.Contains(anyStringContains)
			       || x.brickPart.ldrawCategory.Contains(anyStringContains)
			       || x.brickPart.keywords.Contains(anyStringContains)
			       || x.brickPart.author.Contains(anyStringContains)
			       || x.brickPart.geometryFilePath.Contains(anyStringContains)
			       || x.userCollection.name.Contains(anyStringContains)
			       || x.userCollection.description.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.UserWishlistItem.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/UserWishlistItem/CreateAuditEvent")]
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
