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
using Foundation.ChangeHistory;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the UserCollection entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the UserCollection entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class UserCollectionsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 10;

		static object userCollectionPutSyncRoot = new object();
		static object userCollectionDeleteSyncRoot = new object();

		private BMCContext _context;

		private ILogger<UserCollectionsController> _logger;

		public UserCollectionsController(BMCContext context, ILogger<UserCollectionsController> logger) : base("BMC", "UserCollection")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of UserCollections filtered by the parameters provided.
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
		[Route("api/UserCollections")]
		public async Task<IActionResult> GetUserCollections(
			string name = null,
			string description = null,
			bool? isDefault = null,
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

			IQueryable<Database.UserCollection> query = (from uc in _context.UserCollections select uc);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(uc => uc.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(uc => uc.description == description);
			}
			if (isDefault.HasValue == true)
			{
				query = query.Where(uc => uc.isDefault == isDefault.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(uc => uc.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(uc => uc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(uc => uc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(uc => uc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(uc => uc.deleted == false);
				}
			}
			else
			{
				query = query.Where(uc => uc.active == true);
				query = query.Where(uc => uc.deleted == false);
			}

			query = query.OrderBy(uc => uc.name).ThenBy(uc => uc.description);


			//
			// Add the any string contains parameter to span all the string fields on the User Collection, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			   );
			}

			if (includeRelations == true)
			{
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.UserCollection> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.UserCollection userCollection in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(userCollection, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.UserCollection Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.UserCollection Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of UserCollections filtered by the parameters provided.  Its query is similar to the GetUserCollections method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserCollections/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			bool? isDefault = null,
			int? versionNumber = null,
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


			IQueryable<Database.UserCollection> query = (from uc in _context.UserCollections select uc);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(uc => uc.name == name);
			}
			if (description != null)
			{
				query = query.Where(uc => uc.description == description);
			}
			if (isDefault.HasValue == true)
			{
				query = query.Where(uc => uc.isDefault == isDefault.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(uc => uc.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(uc => uc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(uc => uc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(uc => uc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(uc => uc.deleted == false);
				}
			}
			else
			{
				query = query.Where(uc => uc.active == true);
				query = query.Where(uc => uc.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the User Collection, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single UserCollection by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserCollection/{id}")]
		public async Task<IActionResult> GetUserCollection(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.UserCollection> query = (from uc in _context.UserCollections where
							(uc.id == id) &&
							(userIsAdmin == true || uc.deleted == false) &&
							(userIsWriter == true || uc.active == true)
					select uc);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.UserCollection materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.UserCollection Entity was read with Admin privilege." : "BMC.UserCollection Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserCollection", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.UserCollection entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.UserCollection.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.UserCollection.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing UserCollection record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/UserCollection/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutUserCollection(int id, [FromBody]Database.UserCollection.UserCollectionDTO userCollectionDTO, CancellationToken cancellationToken = default)
		{
			if (userCollectionDTO == null)
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



			if (id != userCollectionDTO.id)
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


			IQueryable<Database.UserCollection> query = (from x in _context.UserCollections
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserCollection existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserCollection PUT", id.ToString(), new Exception("No BMC.UserCollection entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (userCollectionDTO.objectGuid == Guid.Empty)
            {
                userCollectionDTO.objectGuid = existing.objectGuid;
            }
            else if (userCollectionDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a UserCollection record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.UserCollection cloneOfExisting = (Database.UserCollection)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new UserCollection object using the data from the existing record, updated with what is in the DTO.
			//
			Database.UserCollection userCollection = (Database.UserCollection)_context.Entry(existing).GetDatabaseValues().ToObject();
			userCollection.ApplyDTO(userCollectionDTO);
			//
			// The tenant guid for any UserCollection being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the UserCollection because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				userCollection.tenantGuid = existing.tenantGuid;
			}

			lock (userCollectionPutSyncRoot)
			{
				//
				// Validate the version number for the userCollection being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != userCollection.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "UserCollection save attempt was made but save request was with version " + userCollection.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The UserCollection you are trying to update has already changed.  Please try your save again after reloading the UserCollection.");
				}
				else
				{
					// Same record.  Increase version.
					userCollection.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (userCollection.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.UserCollection record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (userCollection.name != null && userCollection.name.Length > 100)
				{
					userCollection.name = userCollection.name.Substring(0, 100);
				}

				if (userCollection.description != null && userCollection.description.Length > 500)
				{
					userCollection.description = userCollection.description.Substring(0, 500);
				}

				try
				{
				    EntityEntry<Database.UserCollection> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(userCollection);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        UserCollectionChangeHistory userCollectionChangeHistory = new UserCollectionChangeHistory();
				        userCollectionChangeHistory.userCollectionId = userCollection.id;
				        userCollectionChangeHistory.versionNumber = userCollection.versionNumber;
				        userCollectionChangeHistory.timeStamp = DateTime.UtcNow;
				        userCollectionChangeHistory.userId = securityUser.id;
				        userCollectionChangeHistory.tenantGuid = userTenantGuid;
				        userCollectionChangeHistory.data = JsonSerializer.Serialize(Database.UserCollection.CreateAnonymousWithFirstLevelSubObjects(userCollection));
				        _context.UserCollectionChangeHistories.Add(userCollectionChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.UserCollection entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserCollection.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserCollection.CreateAnonymousWithFirstLevelSubObjects(userCollection)),
						null);

				return Ok(Database.UserCollection.CreateAnonymous(userCollection));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.UserCollection entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserCollection.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserCollection.CreateAnonymousWithFirstLevelSubObjects(userCollection)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new UserCollection record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserCollection", Name = "UserCollection")]
		public async Task<IActionResult> PostUserCollection([FromBody]Database.UserCollection.UserCollectionDTO userCollectionDTO, CancellationToken cancellationToken = default)
		{
			if (userCollectionDTO == null)
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
			// Create a new UserCollection object using the data from the DTO
			//
			Database.UserCollection userCollection = Database.UserCollection.FromDTO(userCollectionDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				userCollection.tenantGuid = userTenantGuid;

				if (userCollection.name != null && userCollection.name.Length > 100)
				{
					userCollection.name = userCollection.name.Substring(0, 100);
				}

				if (userCollection.description != null && userCollection.description.Length > 500)
				{
					userCollection.description = userCollection.description.Substring(0, 500);
				}

				userCollection.objectGuid = Guid.NewGuid();
				userCollection.versionNumber = 1;

				_context.UserCollections.Add(userCollection);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the userCollection object so that no further changes will be written to the database
				    //
				    _context.Entry(userCollection).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					userCollection.UserCollectionChangeHistories = null;
					userCollection.UserCollectionParts = null;
					userCollection.UserCollectionSetImports = null;
					userCollection.UserWishlistItems = null;


				    UserCollectionChangeHistory userCollectionChangeHistory = new UserCollectionChangeHistory();
				    userCollectionChangeHistory.userCollectionId = userCollection.id;
				    userCollectionChangeHistory.versionNumber = userCollection.versionNumber;
				    userCollectionChangeHistory.timeStamp = DateTime.UtcNow;
				    userCollectionChangeHistory.userId = securityUser.id;
				    userCollectionChangeHistory.tenantGuid = userTenantGuid;
				    userCollectionChangeHistory.data = JsonSerializer.Serialize(Database.UserCollection.CreateAnonymousWithFirstLevelSubObjects(userCollection));
				    _context.UserCollectionChangeHistories.Add(userCollectionChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"BMC.UserCollection entity successfully created.",
						true,
						userCollection. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.UserCollection.CreateAnonymousWithFirstLevelSubObjects(userCollection)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.UserCollection entity creation failed.", false, userCollection.id.ToString(), "", JsonSerializer.Serialize(Database.UserCollection.CreateAnonymousWithFirstLevelSubObjects(userCollection)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserCollection", userCollection.id, userCollection.name));

			return CreatedAtRoute("UserCollection", new { id = userCollection.id }, Database.UserCollection.CreateAnonymousWithFirstLevelSubObjects(userCollection));
		}



        /// <summary>
        /// 
        /// This rolls a UserCollection entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserCollection/Rollback/{id}")]
		[Route("api/UserCollection/Rollback")]
		public async Task<IActionResult> RollbackToUserCollectionVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.UserCollection> query = (from x in _context.UserCollections
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this UserCollection concurrently
			//
			lock (userCollectionPutSyncRoot)
			{
				
				Database.UserCollection userCollection = query.FirstOrDefault();
				
				if (userCollection == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserCollection rollback", id.ToString(), new Exception("No BMC.UserCollection entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the UserCollection current state so we can log it.
				//
				Database.UserCollection cloneOfExisting = (Database.UserCollection)_context.Entry(userCollection).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.UserCollectionChangeHistories = null;
				cloneOfExisting.UserCollectionParts = null;
				cloneOfExisting.UserCollectionSetImports = null;
				cloneOfExisting.UserWishlistItems = null;

				if (versionNumber >= userCollection.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for BMC.UserCollection rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for BMC.UserCollection rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				UserCollectionChangeHistory userCollectionChangeHistory = (from x in _context.UserCollectionChangeHistories
				                                               where
				                                               x.userCollectionId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (userCollectionChangeHistory != null)
				{
				    Database.UserCollection oldUserCollection = JsonSerializer.Deserialize<Database.UserCollection>(userCollectionChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    userCollection.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    userCollection.name = oldUserCollection.name;
				    userCollection.description = oldUserCollection.description;
				    userCollection.isDefault = oldUserCollection.isDefault;
				    userCollection.objectGuid = oldUserCollection.objectGuid;
				    userCollection.active = oldUserCollection.active;
				    userCollection.deleted = oldUserCollection.deleted;

				    string serializedUserCollection = JsonSerializer.Serialize(userCollection);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        UserCollectionChangeHistory newUserCollectionChangeHistory = new UserCollectionChangeHistory();
				        newUserCollectionChangeHistory.userCollectionId = userCollection.id;
				        newUserCollectionChangeHistory.versionNumber = userCollection.versionNumber;
				        newUserCollectionChangeHistory.timeStamp = DateTime.UtcNow;
				        newUserCollectionChangeHistory.userId = securityUser.id;
				        newUserCollectionChangeHistory.tenantGuid = userTenantGuid;
				        newUserCollectionChangeHistory.data = JsonSerializer.Serialize(Database.UserCollection.CreateAnonymousWithFirstLevelSubObjects(userCollection));
				        _context.UserCollectionChangeHistories.Add(newUserCollectionChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.UserCollection rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserCollection.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserCollection.CreateAnonymousWithFirstLevelSubObjects(userCollection)),
						null);


				    return Ok(Database.UserCollection.CreateAnonymous(userCollection));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for BMC.UserCollection rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for BMC.UserCollection rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a UserCollection.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the UserCollection</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserCollection/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetUserCollectionChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.UserCollection userCollection = await _context.UserCollections.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (userCollection == null)
			{
				return NotFound();
			}

			try
			{
				userCollection.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.UserCollection> versionInfo = await userCollection.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a UserCollection.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the UserCollection</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserCollection/{id}/AuditHistory")]
		public async Task<IActionResult> GetUserCollectionAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.UserCollection userCollection = await _context.UserCollections.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (userCollection == null)
			{
				return NotFound();
			}

			try
			{
				userCollection.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.UserCollection>> versions = await userCollection.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a UserCollection.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the UserCollection</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The UserCollection object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserCollection/{id}/Version/{version}")]
		public async Task<IActionResult> GetUserCollectionVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.UserCollection userCollection = await _context.UserCollections.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (userCollection == null)
			{
				return NotFound();
			}

			try
			{
				userCollection.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.UserCollection> versionInfo = await userCollection.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a UserCollection at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the UserCollection</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The UserCollection object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserCollection/{id}/StateAtTime")]
		public async Task<IActionResult> GetUserCollectionStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.UserCollection userCollection = await _context.UserCollections.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (userCollection == null)
			{
				return NotFound();
			}

			try
			{
				userCollection.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.UserCollection> versionInfo = await userCollection.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a UserCollection record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserCollection/{id}")]
		[Route("api/UserCollection")]
		public async Task<IActionResult> DeleteUserCollection(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.UserCollection> query = (from x in _context.UserCollections
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserCollection userCollection = await query.FirstOrDefaultAsync(cancellationToken);

			if (userCollection == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserCollection DELETE", id.ToString(), new Exception("No BMC.UserCollection entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.UserCollection cloneOfExisting = (Database.UserCollection)_context.Entry(userCollection).GetDatabaseValues().ToObject();


			lock (userCollectionDeleteSyncRoot)
			{
			    try
			    {
			        userCollection.deleted = true;
			        userCollection.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        UserCollectionChangeHistory userCollectionChangeHistory = new UserCollectionChangeHistory();
			        userCollectionChangeHistory.userCollectionId = userCollection.id;
			        userCollectionChangeHistory.versionNumber = userCollection.versionNumber;
			        userCollectionChangeHistory.timeStamp = DateTime.UtcNow;
			        userCollectionChangeHistory.userId = securityUser.id;
			        userCollectionChangeHistory.tenantGuid = userTenantGuid;
			        userCollectionChangeHistory.data = JsonSerializer.Serialize(Database.UserCollection.CreateAnonymousWithFirstLevelSubObjects(userCollection));
			        _context.UserCollectionChangeHistories.Add(userCollectionChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.UserCollection entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserCollection.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserCollection.CreateAnonymousWithFirstLevelSubObjects(userCollection)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.UserCollection entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserCollection.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserCollection.CreateAnonymousWithFirstLevelSubObjects(userCollection)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of UserCollection records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/UserCollections/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			bool? isDefault = null,
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

			IQueryable<Database.UserCollection> query = (from uc in _context.UserCollections select uc);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(uc => uc.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(uc => uc.description == description);
			}
			if (isDefault.HasValue == true)
			{
				query = query.Where(uc => uc.isDefault == isDefault.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(uc => uc.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(uc => uc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(uc => uc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(uc => uc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(uc => uc.deleted == false);
				}
			}
			else
			{
				query = query.Where(uc => uc.active == true);
				query = query.Where(uc => uc.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Collection, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name).ThenBy(x => x.description);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.UserCollection.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/UserCollection/CreateAuditEvent")]
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
