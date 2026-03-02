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
    /// This auto generated class provides the basic CRUD operations for the UserPartList entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the UserPartList entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class UserPartListsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 10;

		static object userPartListPutSyncRoot = new object();
		static object userPartListDeleteSyncRoot = new object();

		private BMCContext _context;

		private ILogger<UserPartListsController> _logger;

		public UserPartListsController(BMCContext context, ILogger<UserPartListsController> logger) : base("BMC", "UserPartList")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of UserPartLists filtered by the parameters provided.
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
		[Route("api/UserPartLists")]
		public async Task<IActionResult> GetUserPartLists(
			string name = null,
			bool? isBuildable = null,
			int? rebrickableListId = null,
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

			IQueryable<Database.UserPartList> query = (from upl in _context.UserPartLists select upl);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(upl => upl.name == name);
			}
			if (isBuildable.HasValue == true)
			{
				query = query.Where(upl => upl.isBuildable == isBuildable.Value);
			}
			if (rebrickableListId.HasValue == true)
			{
				query = query.Where(upl => upl.rebrickableListId == rebrickableListId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(upl => upl.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(upl => upl.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(upl => upl.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(upl => upl.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(upl => upl.deleted == false);
				}
			}
			else
			{
				query = query.Where(upl => upl.active == true);
				query = query.Where(upl => upl.deleted == false);
			}

			query = query.OrderBy(upl => upl.name);


			//
			// Add the any string contains parameter to span all the string fields on the User Part List, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
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
			
			List<Database.UserPartList> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.UserPartList userPartList in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(userPartList, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.UserPartList Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.UserPartList Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of UserPartLists filtered by the parameters provided.  Its query is similar to the GetUserPartLists method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserPartLists/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			bool? isBuildable = null,
			int? rebrickableListId = null,
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


			IQueryable<Database.UserPartList> query = (from upl in _context.UserPartLists select upl);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(upl => upl.name == name);
			}
			if (isBuildable.HasValue == true)
			{
				query = query.Where(upl => upl.isBuildable == isBuildable.Value);
			}
			if (rebrickableListId.HasValue == true)
			{
				query = query.Where(upl => upl.rebrickableListId == rebrickableListId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(upl => upl.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(upl => upl.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(upl => upl.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(upl => upl.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(upl => upl.deleted == false);
				}
			}
			else
			{
				query = query.Where(upl => upl.active == true);
				query = query.Where(upl => upl.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the User Part List, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single UserPartList by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserPartList/{id}")]
		public async Task<IActionResult> GetUserPartList(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.UserPartList> query = (from upl in _context.UserPartLists where
							(upl.id == id) &&
							(userIsAdmin == true || upl.deleted == false) &&
							(userIsWriter == true || upl.active == true)
					select upl);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.UserPartList materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.UserPartList Entity was read with Admin privilege." : "BMC.UserPartList Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserPartList", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.UserPartList entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.UserPartList.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.UserPartList.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing UserPartList record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/UserPartList/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutUserPartList(int id, [FromBody]Database.UserPartList.UserPartListDTO userPartListDTO, CancellationToken cancellationToken = default)
		{
			if (userPartListDTO == null)
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



			if (id != userPartListDTO.id)
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


			IQueryable<Database.UserPartList> query = (from x in _context.UserPartLists
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserPartList existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserPartList PUT", id.ToString(), new Exception("No BMC.UserPartList entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (userPartListDTO.objectGuid == Guid.Empty)
            {
                userPartListDTO.objectGuid = existing.objectGuid;
            }
            else if (userPartListDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a UserPartList record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.UserPartList cloneOfExisting = (Database.UserPartList)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new UserPartList object using the data from the existing record, updated with what is in the DTO.
			//
			Database.UserPartList userPartList = (Database.UserPartList)_context.Entry(existing).GetDatabaseValues().ToObject();
			userPartList.ApplyDTO(userPartListDTO);
			//
			// The tenant guid for any UserPartList being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the UserPartList because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				userPartList.tenantGuid = existing.tenantGuid;
			}

			lock (userPartListPutSyncRoot)
			{
				//
				// Validate the version number for the userPartList being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != userPartList.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "UserPartList save attempt was made but save request was with version " + userPartList.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The UserPartList you are trying to update has already changed.  Please try your save again after reloading the UserPartList.");
				}
				else
				{
					// Same record.  Increase version.
					userPartList.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (userPartList.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.UserPartList record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (userPartList.name != null && userPartList.name.Length > 100)
				{
					userPartList.name = userPartList.name.Substring(0, 100);
				}

				try
				{
				    EntityEntry<Database.UserPartList> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(userPartList);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        UserPartListChangeHistory userPartListChangeHistory = new UserPartListChangeHistory();
				        userPartListChangeHistory.userPartListId = userPartList.id;
				        userPartListChangeHistory.versionNumber = userPartList.versionNumber;
				        userPartListChangeHistory.timeStamp = DateTime.UtcNow;
				        userPartListChangeHistory.userId = securityUser.id;
				        userPartListChangeHistory.tenantGuid = userTenantGuid;
				        userPartListChangeHistory.data = JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(userPartList));
				        _context.UserPartListChangeHistories.Add(userPartListChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.UserPartList entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(userPartList)),
						null);

				return Ok(Database.UserPartList.CreateAnonymous(userPartList));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.UserPartList entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(userPartList)),
						ex);

					return Problem(ex.Message);
				}

			}
		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new UserPartList record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserPartList", Name = "UserPartList")]
		public async Task<IActionResult> PostUserPartList([FromBody]Database.UserPartList.UserPartListDTO userPartListDTO, CancellationToken cancellationToken = default)
		{
			if (userPartListDTO == null)
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
			// Create a new UserPartList object using the data from the DTO
			//
			Database.UserPartList userPartList = Database.UserPartList.FromDTO(userPartListDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				userPartList.tenantGuid = userTenantGuid;

				if (userPartList.name != null && userPartList.name.Length > 100)
				{
					userPartList.name = userPartList.name.Substring(0, 100);
				}

				userPartList.objectGuid = Guid.NewGuid();
				userPartList.versionNumber = 1;

				_context.UserPartLists.Add(userPartList);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the userPartList object so that no further changes will be written to the database
				    //
				    _context.Entry(userPartList).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					userPartList.UserPartListChangeHistories = null;
					userPartList.UserPartListItems = null;


				    UserPartListChangeHistory userPartListChangeHistory = new UserPartListChangeHistory();
				    userPartListChangeHistory.userPartListId = userPartList.id;
				    userPartListChangeHistory.versionNumber = userPartList.versionNumber;
				    userPartListChangeHistory.timeStamp = DateTime.UtcNow;
				    userPartListChangeHistory.userId = securityUser.id;
				    userPartListChangeHistory.tenantGuid = userTenantGuid;
				    userPartListChangeHistory.data = JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(userPartList));
				    _context.UserPartListChangeHistories.Add(userPartListChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"BMC.UserPartList entity successfully created.",
						true,
						userPartList. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(userPartList)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.UserPartList entity creation failed.", false, userPartList.id.ToString(), "", JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(userPartList)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserPartList", userPartList.id, userPartList.name));

			return CreatedAtRoute("UserPartList", new { id = userPartList.id }, Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(userPartList));
		}

*/

/* This function is expected to be overridden in a custom file

        /// <summary>
        /// 
        /// This rolls a UserPartList entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserPartList/Rollback/{id}")]
		[Route("api/UserPartList/Rollback")]
		public async Task<IActionResult> RollbackToUserPartListVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.UserPartList> query = (from x in _context.UserPartLists
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this UserPartList concurrently
			//
			lock (userPartListPutSyncRoot)
			{
				
				Database.UserPartList userPartList = query.FirstOrDefault();
				
				if (userPartList == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserPartList rollback", id.ToString(), new Exception("No BMC.UserPartList entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the UserPartList current state so we can log it.
				//
				Database.UserPartList cloneOfExisting = (Database.UserPartList)_context.Entry(userPartList).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.UserPartListChangeHistories = null;
				cloneOfExisting.UserPartListItems = null;

				if (versionNumber >= userPartList.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for BMC.UserPartList rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for BMC.UserPartList rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				UserPartListChangeHistory userPartListChangeHistory = (from x in _context.UserPartListChangeHistories
				                                               where
				                                               x.userPartListId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (userPartListChangeHistory != null)
				{
				    Database.UserPartList oldUserPartList = JsonSerializer.Deserialize<Database.UserPartList>(userPartListChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    userPartList.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    userPartList.name = oldUserPartList.name;
				    userPartList.isBuildable = oldUserPartList.isBuildable;
				    userPartList.rebrickableListId = oldUserPartList.rebrickableListId;
				    userPartList.objectGuid = oldUserPartList.objectGuid;
				    userPartList.active = oldUserPartList.active;
				    userPartList.deleted = oldUserPartList.deleted;

				    string serializedUserPartList = JsonSerializer.Serialize(userPartList);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        UserPartListChangeHistory newUserPartListChangeHistory = new UserPartListChangeHistory();
				        newUserPartListChangeHistory.userPartListId = userPartList.id;
				        newUserPartListChangeHistory.versionNumber = userPartList.versionNumber;
				        newUserPartListChangeHistory.timeStamp = DateTime.UtcNow;
				        newUserPartListChangeHistory.userId = securityUser.id;
				        newUserPartListChangeHistory.tenantGuid = userTenantGuid;
				        newUserPartListChangeHistory.data = JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(userPartList));
				        _context.UserPartListChangeHistories.Add(newUserPartListChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.UserPartList rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(userPartList)),
						null);


				    return Ok(Database.UserPartList.CreateAnonymous(userPartList));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for BMC.UserPartList rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for BMC.UserPartList rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}

*/



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a UserPartList.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the UserPartList</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserPartList/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetUserPartListChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.UserPartList userPartList = await _context.UserPartLists.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (userPartList == null)
			{
				return NotFound();
			}

			try
			{
				userPartList.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.UserPartList> versionInfo = await userPartList.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a UserPartList.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the UserPartList</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserPartList/{id}/AuditHistory")]
		public async Task<IActionResult> GetUserPartListAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.UserPartList userPartList = await _context.UserPartLists.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (userPartList == null)
			{
				return NotFound();
			}

			try
			{
				userPartList.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.UserPartList>> versions = await userPartList.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a UserPartList.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the UserPartList</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The UserPartList object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserPartList/{id}/Version/{version}")]
		public async Task<IActionResult> GetUserPartListVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.UserPartList userPartList = await _context.UserPartLists.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (userPartList == null)
			{
				return NotFound();
			}

			try
			{
				userPartList.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.UserPartList> versionInfo = await userPartList.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a UserPartList at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the UserPartList</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The UserPartList object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserPartList/{id}/StateAtTime")]
		public async Task<IActionResult> GetUserPartListStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.UserPartList userPartList = await _context.UserPartLists.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (userPartList == null)
			{
				return NotFound();
			}

			try
			{
				userPartList.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.UserPartList> versionInfo = await userPartList.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a UserPartList record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserPartList/{id}")]
		[Route("api/UserPartList")]
		public async Task<IActionResult> DeleteUserPartList(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.UserPartList> query = (from x in _context.UserPartLists
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserPartList userPartList = await query.FirstOrDefaultAsync(cancellationToken);

			if (userPartList == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserPartList DELETE", id.ToString(), new Exception("No BMC.UserPartList entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.UserPartList cloneOfExisting = (Database.UserPartList)_context.Entry(userPartList).GetDatabaseValues().ToObject();


			lock (userPartListDeleteSyncRoot)
			{
			    try
			    {
			        userPartList.deleted = true;
			        userPartList.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        UserPartListChangeHistory userPartListChangeHistory = new UserPartListChangeHistory();
			        userPartListChangeHistory.userPartListId = userPartList.id;
			        userPartListChangeHistory.versionNumber = userPartList.versionNumber;
			        userPartListChangeHistory.timeStamp = DateTime.UtcNow;
			        userPartListChangeHistory.userId = securityUser.id;
			        userPartListChangeHistory.tenantGuid = userTenantGuid;
			        userPartListChangeHistory.data = JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(userPartList));
			        _context.UserPartListChangeHistories.Add(userPartListChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.UserPartList entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(userPartList)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.UserPartList entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(userPartList)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


*/
        /// <summary>
        /// 
        /// This gets a list of UserPartList records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/UserPartLists/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			bool? isBuildable = null,
			int? rebrickableListId = null,
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

			IQueryable<Database.UserPartList> query = (from upl in _context.UserPartLists select upl);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(upl => upl.name == name);
			}
			if (isBuildable.HasValue == true)
			{
				query = query.Where(upl => upl.isBuildable == isBuildable.Value);
			}
			if (rebrickableListId.HasValue == true)
			{
				query = query.Where(upl => upl.rebrickableListId == rebrickableListId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(upl => upl.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(upl => upl.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(upl => upl.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(upl => upl.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(upl => upl.deleted == false);
				}
			}
			else
			{
				query = query.Where(upl => upl.active == true);
				query = query.Where(upl => upl.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Part List, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.UserPartList.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/UserPartList/CreateAuditEvent")]
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
