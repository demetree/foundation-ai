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
    /// This auto generated class provides the basic CRUD operations for the UserFollow entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the UserFollow entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class UserFollowsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		private BMCContext _context;

		private ILogger<UserFollowsController> _logger;

		public UserFollowsController(BMCContext context, ILogger<UserFollowsController> logger) : base("BMC", "UserFollow")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of UserFollows filtered by the parameters provided.
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
		[Route("api/UserFollows")]
		public async Task<IActionResult> GetUserFollows(
			Guid? followerTenantGuid = null,
			Guid? followedTenantGuid = null,
			DateTime? followedDate = null,
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
			if (followedDate.HasValue == true && followedDate.Value.Kind != DateTimeKind.Utc)
			{
				followedDate = followedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.UserFollow> query = (from uf in _context.UserFollows select uf);
			if (followerTenantGuid.HasValue == true)
			{
				query = query.Where(uf => uf.followerTenantGuid == followerTenantGuid);
			}
			if (followedTenantGuid.HasValue == true)
			{
				query = query.Where(uf => uf.followedTenantGuid == followedTenantGuid);
			}
			if (followedDate.HasValue == true)
			{
				query = query.Where(uf => uf.followedDate == followedDate.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(uf => uf.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(uf => uf.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(uf => uf.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(uf => uf.deleted == false);
				}
			}
			else
			{
				query = query.Where(uf => uf.active == true);
				query = query.Where(uf => uf.deleted == false);
			}

			query = query.OrderBy(uf => uf.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Follow, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			//if (!string.IsNullOrEmpty(anyStringContains))
			//{
			//   query = query.Where(x =>
			//   );
			//}

			query = query.AsNoTracking();
			
			List<Database.UserFollow> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.UserFollow userFollow in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(userFollow, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.UserFollow Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.UserFollow Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of UserFollows filtered by the parameters provided.  Its query is similar to the GetUserFollows method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserFollows/RowCount")]
		public async Task<IActionResult> GetRowCount(
			Guid? followerTenantGuid = null,
			Guid? followedTenantGuid = null,
			DateTime? followedDate = null,
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

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (followedDate.HasValue == true && followedDate.Value.Kind != DateTimeKind.Utc)
			{
				followedDate = followedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.UserFollow> query = (from uf in _context.UserFollows select uf);
			if (followerTenantGuid.HasValue == true)
			{
				query = query.Where(uf => uf.followerTenantGuid == followerTenantGuid);
			}
			if (followedTenantGuid.HasValue == true)
			{
				query = query.Where(uf => uf.followedTenantGuid == followedTenantGuid);
			}
			if (followedDate.HasValue == true)
			{
				query = query.Where(uf => uf.followedDate == followedDate.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(uf => uf.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(uf => uf.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(uf => uf.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(uf => uf.deleted == false);
				}
			}
			else
			{
				query = query.Where(uf => uf.active == true);
				query = query.Where(uf => uf.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the User Follow, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			////
			//if (!string.IsNullOrEmpty(anyStringContains))
			//{
			//   query = query.Where(x =>
			//   );
			//}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single UserFollow by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserFollow/{id}")]
		public async Task<IActionResult> GetUserFollow(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			try
			{
				IQueryable<Database.UserFollow> query = (from uf in _context.UserFollows where
							(uf.id == id) &&
							(userIsAdmin == true || uf.deleted == false) &&
							(userIsWriter == true || uf.active == true)
					select uf);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.UserFollow materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.UserFollow Entity was read with Admin privilege." : "BMC.UserFollow Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserFollow", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.UserFollow entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.UserFollow.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.UserFollow.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing UserFollow record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/UserFollow/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutUserFollow(int id, [FromBody]Database.UserFollow.UserFollowDTO userFollowDTO, CancellationToken cancellationToken = default)
		{
			if (userFollowDTO == null)
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



			if (id != userFollowDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.UserFollow> query = (from x in _context.UserFollows
				where
				(x.id == id)
				select x);


			Database.UserFollow existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserFollow PUT", id.ToString(), new Exception("No BMC.UserFollow entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (userFollowDTO.objectGuid == Guid.Empty)
            {
                userFollowDTO.objectGuid = existing.objectGuid;
            }
            else if (userFollowDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a UserFollow record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.UserFollow cloneOfExisting = (Database.UserFollow)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new UserFollow object using the data from the existing record, updated with what is in the DTO.
			//
			Database.UserFollow userFollow = (Database.UserFollow)_context.Entry(existing).GetDatabaseValues().ToObject();
			userFollow.ApplyDTO(userFollowDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (userFollow.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.UserFollow record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (userFollow.followedDate.Kind != DateTimeKind.Utc)
			{
				userFollow.followedDate = userFollow.followedDate.ToUniversalTime();
			}

			EntityEntry<Database.UserFollow> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(userFollow);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.UserFollow entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserFollow.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserFollow.CreateAnonymousWithFirstLevelSubObjects(userFollow)),
					null);


				return Ok(Database.UserFollow.CreateAnonymous(userFollow));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.UserFollow entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserFollow.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserFollow.CreateAnonymousWithFirstLevelSubObjects(userFollow)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new UserFollow record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserFollow", Name = "UserFollow")]
		public async Task<IActionResult> PostUserFollow([FromBody]Database.UserFollow.UserFollowDTO userFollowDTO, CancellationToken cancellationToken = default)
		{
			if (userFollowDTO == null)
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

			//
			// Create a new UserFollow object using the data from the DTO
			//
			Database.UserFollow userFollow = Database.UserFollow.FromDTO(userFollowDTO);

			try
			{
				if (userFollow.followedDate.Kind != DateTimeKind.Utc)
				{
					userFollow.followedDate = userFollow.followedDate.ToUniversalTime();
				}

				userFollow.objectGuid = Guid.NewGuid();
				_context.UserFollows.Add(userFollow);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.UserFollow entity successfully created.",
					true,
					userFollow.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.UserFollow.CreateAnonymousWithFirstLevelSubObjects(userFollow)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.UserFollow entity creation failed.", false, userFollow.id.ToString(), "", JsonSerializer.Serialize(userFollow), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserFollow", userFollow.id, userFollow.id.ToString()));

			return CreatedAtRoute("UserFollow", new { id = userFollow.id }, Database.UserFollow.CreateAnonymousWithFirstLevelSubObjects(userFollow));
		}



        /// <summary>
        /// 
        /// This deletes a UserFollow record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserFollow/{id}")]
		[Route("api/UserFollow")]
		public async Task<IActionResult> DeleteUserFollow(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.UserFollow> query = (from x in _context.UserFollows
				where
				(x.id == id)
				select x);


			Database.UserFollow userFollow = await query.FirstOrDefaultAsync(cancellationToken);

			if (userFollow == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserFollow DELETE", id.ToString(), new Exception("No BMC.UserFollow entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.UserFollow cloneOfExisting = (Database.UserFollow)_context.Entry(userFollow).GetDatabaseValues().ToObject();


			try
			{
				userFollow.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.UserFollow entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserFollow.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserFollow.CreateAnonymousWithFirstLevelSubObjects(userFollow)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.UserFollow entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserFollow.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserFollow.CreateAnonymousWithFirstLevelSubObjects(userFollow)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of UserFollow records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/UserFollows/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			Guid? followerTenantGuid = null,
			Guid? followedTenantGuid = null,
			DateTime? followedDate = null,
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
			if (followedDate.HasValue == true && followedDate.Value.Kind != DateTimeKind.Utc)
			{
				followedDate = followedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.UserFollow> query = (from uf in _context.UserFollows select uf);
			if (followerTenantGuid.HasValue == true)
			{
				query = query.Where(uf => uf.followerTenantGuid == followerTenantGuid);
			}
			if (followedTenantGuid.HasValue == true)
			{
				query = query.Where(uf => uf.followedTenantGuid == followedTenantGuid);
			}
			if (followedDate.HasValue == true)
			{
				query = query.Where(uf => uf.followedDate == followedDate.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(uf => uf.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(uf => uf.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(uf => uf.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(uf => uf.deleted == false);
				}
			}
			else
			{
				query = query.Where(uf => uf.active == true);
				query = query.Where(uf => uf.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Follow, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			//if (!string.IsNullOrEmpty(anyStringContains))
			//{
			//   query = query.Where(x =>
			//   );
			//}


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.UserFollow.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/UserFollow/CreateAuditEvent")]
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
