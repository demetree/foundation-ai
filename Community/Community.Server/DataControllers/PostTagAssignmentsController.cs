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
using Foundation.Community.Database;

namespace Foundation.Community.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the PostTagAssignment entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the PostTagAssignment entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class PostTagAssignmentsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 10;

		private CommunityContext _context;

		private ILogger<PostTagAssignmentsController> _logger;

		public PostTagAssignmentsController(CommunityContext context, ILogger<PostTagAssignmentsController> logger) : base("Community", "PostTagAssignment")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of PostTagAssignments filtered by the parameters provided.
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
		[Route("api/PostTagAssignments")]
		public async Task<IActionResult> GetPostTagAssignments(
			int? postId = null,
			int? postTagId = null,
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
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
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

			IQueryable<Database.PostTagAssignment> query = (from pta in _context.PostTagAssignments select pta);
			if (postId.HasValue == true)
			{
				query = query.Where(pta => pta.postId == postId.Value);
			}
			if (postTagId.HasValue == true)
			{
				query = query.Where(pta => pta.postTagId == postTagId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pta => pta.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pta => pta.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pta => pta.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pta => pta.deleted == false);
				}
			}
			else
			{
				query = query.Where(pta => pta.active == true);
				query = query.Where(pta => pta.deleted == false);
			}

			query = query.OrderBy(pta => pta.id);

			if (includeRelations == true)
			{
				query = query.Include(x => x.post);
				query = query.Include(x => x.postTag);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.PostTagAssignment> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.PostTagAssignment postTagAssignment in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(postTagAssignment, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Community.PostTagAssignment Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Community.PostTagAssignment Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of PostTagAssignments filtered by the parameters provided.  Its query is similar to the GetPostTagAssignments method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PostTagAssignments/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? postId = null,
			int? postTagId = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.PostTagAssignment> query = (from pta in _context.PostTagAssignments select pta);
			if (postId.HasValue == true)
			{
				query = query.Where(pta => pta.postId == postId.Value);
			}
			if (postTagId.HasValue == true)
			{
				query = query.Where(pta => pta.postTagId == postTagId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pta => pta.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pta => pta.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pta => pta.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pta => pta.deleted == false);
				}
			}
			else
			{
				query = query.Where(pta => pta.active == true);
				query = query.Where(pta => pta.deleted == false);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single PostTagAssignment by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PostTagAssignment/{id}")]
		public async Task<IActionResult> GetPostTagAssignment(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.PostTagAssignment> query = (from pta in _context.PostTagAssignments where
							(pta.id == id) &&
							(userIsAdmin == true || pta.deleted == false) &&
							(userIsWriter == true || pta.active == true)
					select pta);

				if (includeRelations == true)
				{
					query = query.Include(x => x.post);
					query = query.Include(x => x.postTag);
					query = query.AsSplitQuery();
				}

				Database.PostTagAssignment materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Community.PostTagAssignment Entity was read with Admin privilege." : "Community.PostTagAssignment Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PostTagAssignment", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Community.PostTagAssignment entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Community.PostTagAssignment.   Entity was read with Admin privilege." : "Exception caught during entity read of Community.PostTagAssignment.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing PostTagAssignment record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/PostTagAssignment/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutPostTagAssignment(int id, [FromBody]Database.PostTagAssignment.PostTagAssignmentDTO postTagAssignmentDTO, CancellationToken cancellationToken = default)
		{
			if (postTagAssignmentDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Community Content Writer role needed to write to this table, or Community Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Community Content Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != postTagAssignmentDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.PostTagAssignment> query = (from x in _context.PostTagAssignments
				where
				(x.id == id)
				select x);


			Database.PostTagAssignment existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.PostTagAssignment PUT", id.ToString(), new Exception("No Community.PostTagAssignment entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (postTagAssignmentDTO.objectGuid == Guid.Empty)
            {
                postTagAssignmentDTO.objectGuid = existing.objectGuid;
            }
            else if (postTagAssignmentDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a PostTagAssignment record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.PostTagAssignment cloneOfExisting = (Database.PostTagAssignment)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new PostTagAssignment object using the data from the existing record, updated with what is in the DTO.
			//
			Database.PostTagAssignment postTagAssignment = (Database.PostTagAssignment)_context.Entry(existing).GetDatabaseValues().ToObject();
			postTagAssignment.ApplyDTO(postTagAssignmentDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (postTagAssignment.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Community.PostTagAssignment record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			EntityEntry<Database.PostTagAssignment> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(postTagAssignment);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Community.PostTagAssignment entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.PostTagAssignment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PostTagAssignment.CreateAnonymousWithFirstLevelSubObjects(postTagAssignment)),
					null);


				return Ok(Database.PostTagAssignment.CreateAnonymous(postTagAssignment));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Community.PostTagAssignment entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.PostTagAssignment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PostTagAssignment.CreateAnonymousWithFirstLevelSubObjects(postTagAssignment)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new PostTagAssignment record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PostTagAssignment", Name = "PostTagAssignment")]
		public async Task<IActionResult> PostPostTagAssignment([FromBody]Database.PostTagAssignment.PostTagAssignmentDTO postTagAssignmentDTO, CancellationToken cancellationToken = default)
		{
			if (postTagAssignmentDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Community Content Writer role needed to write to this table, or Community Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Community Content Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new PostTagAssignment object using the data from the DTO
			//
			Database.PostTagAssignment postTagAssignment = Database.PostTagAssignment.FromDTO(postTagAssignmentDTO);

			try
			{
				postTagAssignment.objectGuid = Guid.NewGuid();
				_context.PostTagAssignments.Add(postTagAssignment);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Community.PostTagAssignment entity successfully created.",
					true,
					postTagAssignment.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.PostTagAssignment.CreateAnonymousWithFirstLevelSubObjects(postTagAssignment)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Community.PostTagAssignment entity creation failed.", false, postTagAssignment.id.ToString(), "", JsonSerializer.Serialize(postTagAssignment), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PostTagAssignment", postTagAssignment.id, postTagAssignment.id.ToString()));

			return CreatedAtRoute("PostTagAssignment", new { id = postTagAssignment.id }, Database.PostTagAssignment.CreateAnonymousWithFirstLevelSubObjects(postTagAssignment));
		}



        /// <summary>
        /// 
        /// This deletes a PostTagAssignment record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PostTagAssignment/{id}")]
		[Route("api/PostTagAssignment")]
		public async Task<IActionResult> DeletePostTagAssignment(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Community Content Writer role needed to write to this table, or Community Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Community Content Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.PostTagAssignment> query = (from x in _context.PostTagAssignments
				where
				(x.id == id)
				select x);


			Database.PostTagAssignment postTagAssignment = await query.FirstOrDefaultAsync(cancellationToken);

			if (postTagAssignment == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.PostTagAssignment DELETE", id.ToString(), new Exception("No Community.PostTagAssignment entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.PostTagAssignment cloneOfExisting = (Database.PostTagAssignment)_context.Entry(postTagAssignment).GetDatabaseValues().ToObject();


			try
			{
				postTagAssignment.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Community.PostTagAssignment entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.PostTagAssignment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PostTagAssignment.CreateAnonymousWithFirstLevelSubObjects(postTagAssignment)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Community.PostTagAssignment entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.PostTagAssignment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PostTagAssignment.CreateAnonymousWithFirstLevelSubObjects(postTagAssignment)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of PostTagAssignment records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/PostTagAssignments/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? postId = null,
			int? postTagId = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);


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

			IQueryable<Database.PostTagAssignment> query = (from pta in _context.PostTagAssignments select pta);
			if (postId.HasValue == true)
			{
				query = query.Where(pta => pta.postId == postId.Value);
			}
			if (postTagId.HasValue == true)
			{
				query = query.Where(pta => pta.postTagId == postTagId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pta => pta.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pta => pta.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pta => pta.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pta => pta.deleted == false);
				}
			}
			else
			{
				query = query.Where(pta => pta.active == true);
				query = query.Where(pta => pta.deleted == false);
			}


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.PostTagAssignment.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/PostTagAssignment/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Community Content Writer role needed to write to this table, or Community Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Community Content Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
