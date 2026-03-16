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
    /// This auto generated class provides the basic CRUD operations for the PostCategory entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the PostCategory entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class PostCategoriesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 10;

		private CommunityContext _context;

		private ILogger<PostCategoriesController> _logger;

		public PostCategoriesController(CommunityContext context, ILogger<PostCategoriesController> logger) : base("Community", "PostCategory")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of PostCategories filtered by the parameters provided.
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
		[Route("api/PostCategories")]
		public async Task<IActionResult> GetPostCategories(
			string name = null,
			string description = null,
			string slug = null,
			int? sequence = null,
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

			IQueryable<Database.PostCategory> query = (from pc in _context.PostCategories select pc);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(pc => pc.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(pc => pc.description == description);
			}
			if (string.IsNullOrEmpty(slug) == false)
			{
				query = query.Where(pc => pc.slug == slug);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(pc => pc.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pc => pc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pc => pc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pc => pc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pc => pc.deleted == false);
				}
			}
			else
			{
				query = query.Where(pc => pc.active == true);
				query = query.Where(pc => pc.deleted == false);
			}

			query = query.OrderBy(pc => pc.sequence).ThenBy(pc => pc.name).ThenBy(pc => pc.description).ThenBy(pc => pc.slug);


			//
			// Add the any string contains parameter to span all the string fields on the Post Category, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.slug.Contains(anyStringContains)
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
			
			List<Database.PostCategory> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.PostCategory postCategory in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(postCategory, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Community.PostCategory Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Community.PostCategory Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of PostCategories filtered by the parameters provided.  Its query is similar to the GetPostCategories method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PostCategories/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			string slug = null,
			int? sequence = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
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

			IQueryable<Database.PostCategory> query = (from pc in _context.PostCategories select pc);
			if (name != null)
			{
				query = query.Where(pc => pc.name == name);
			}
			if (description != null)
			{
				query = query.Where(pc => pc.description == description);
			}
			if (slug != null)
			{
				query = query.Where(pc => pc.slug == slug);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(pc => pc.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pc => pc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pc => pc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pc => pc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pc => pc.deleted == false);
				}
			}
			else
			{
				query = query.Where(pc => pc.active == true);
				query = query.Where(pc => pc.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Post Category, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.slug.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single PostCategory by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PostCategory/{id}")]
		public async Task<IActionResult> GetPostCategory(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.PostCategory> query = (from pc in _context.PostCategories where
							(pc.id == id) &&
							(userIsAdmin == true || pc.deleted == false) &&
							(userIsWriter == true || pc.active == true)
					select pc);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.PostCategory materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Community.PostCategory Entity was read with Admin privilege." : "Community.PostCategory Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PostCategory", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Community.PostCategory entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Community.PostCategory.   Entity was read with Admin privilege." : "Exception caught during entity read of Community.PostCategory.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing PostCategory record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/PostCategory/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutPostCategory(int id, [FromBody]Database.PostCategory.PostCategoryDTO postCategoryDTO, CancellationToken cancellationToken = default)
		{
			if (postCategoryDTO == null)
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



			if (id != postCategoryDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.PostCategory> query = (from x in _context.PostCategories
				where
				(x.id == id)
				select x);


			Database.PostCategory existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.PostCategory PUT", id.ToString(), new Exception("No Community.PostCategory entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (postCategoryDTO.objectGuid == Guid.Empty)
            {
                postCategoryDTO.objectGuid = existing.objectGuid;
            }
            else if (postCategoryDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a PostCategory record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.PostCategory cloneOfExisting = (Database.PostCategory)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new PostCategory object using the data from the existing record, updated with what is in the DTO.
			//
			Database.PostCategory postCategory = (Database.PostCategory)_context.Entry(existing).GetDatabaseValues().ToObject();
			postCategory.ApplyDTO(postCategoryDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (postCategory.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Community.PostCategory record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (postCategory.name != null && postCategory.name.Length > 100)
			{
				postCategory.name = postCategory.name.Substring(0, 100);
			}

			if (postCategory.description != null && postCategory.description.Length > 500)
			{
				postCategory.description = postCategory.description.Substring(0, 500);
			}

			if (postCategory.slug != null && postCategory.slug.Length > 250)
			{
				postCategory.slug = postCategory.slug.Substring(0, 250);
			}

			EntityEntry<Database.PostCategory> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(postCategory);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Community.PostCategory entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.PostCategory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PostCategory.CreateAnonymousWithFirstLevelSubObjects(postCategory)),
					null);


				return Ok(Database.PostCategory.CreateAnonymous(postCategory));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Community.PostCategory entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.PostCategory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PostCategory.CreateAnonymousWithFirstLevelSubObjects(postCategory)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new PostCategory record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PostCategory", Name = "PostCategory")]
		public async Task<IActionResult> PostPostCategory([FromBody]Database.PostCategory.PostCategoryDTO postCategoryDTO, CancellationToken cancellationToken = default)
		{
			if (postCategoryDTO == null)
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
			// Create a new PostCategory object using the data from the DTO
			//
			Database.PostCategory postCategory = Database.PostCategory.FromDTO(postCategoryDTO);

			try
			{
				if (postCategory.name != null && postCategory.name.Length > 100)
				{
					postCategory.name = postCategory.name.Substring(0, 100);
				}

				if (postCategory.description != null && postCategory.description.Length > 500)
				{
					postCategory.description = postCategory.description.Substring(0, 500);
				}

				if (postCategory.slug != null && postCategory.slug.Length > 250)
				{
					postCategory.slug = postCategory.slug.Substring(0, 250);
				}

				postCategory.objectGuid = Guid.NewGuid();
				_context.PostCategories.Add(postCategory);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Community.PostCategory entity successfully created.",
					true,
					postCategory.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.PostCategory.CreateAnonymousWithFirstLevelSubObjects(postCategory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Community.PostCategory entity creation failed.", false, postCategory.id.ToString(), "", JsonSerializer.Serialize(postCategory), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PostCategory", postCategory.id, postCategory.name));

			return CreatedAtRoute("PostCategory", new { id = postCategory.id }, Database.PostCategory.CreateAnonymousWithFirstLevelSubObjects(postCategory));
		}



        /// <summary>
        /// 
        /// This deletes a PostCategory record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PostCategory/{id}")]
		[Route("api/PostCategory")]
		public async Task<IActionResult> DeletePostCategory(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.PostCategory> query = (from x in _context.PostCategories
				where
				(x.id == id)
				select x);


			Database.PostCategory postCategory = await query.FirstOrDefaultAsync(cancellationToken);

			if (postCategory == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.PostCategory DELETE", id.ToString(), new Exception("No Community.PostCategory entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.PostCategory cloneOfExisting = (Database.PostCategory)_context.Entry(postCategory).GetDatabaseValues().ToObject();


			try
			{
				postCategory.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Community.PostCategory entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.PostCategory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PostCategory.CreateAnonymousWithFirstLevelSubObjects(postCategory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Community.PostCategory entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.PostCategory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PostCategory.CreateAnonymousWithFirstLevelSubObjects(postCategory)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of PostCategory records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/PostCategories/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			string slug = null,
			int? sequence = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
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

			IQueryable<Database.PostCategory> query = (from pc in _context.PostCategories select pc);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(pc => pc.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(pc => pc.description == description);
			}
			if (string.IsNullOrEmpty(slug) == false)
			{
				query = query.Where(pc => pc.slug == slug);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(pc => pc.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pc => pc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pc => pc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pc => pc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pc => pc.deleted == false);
				}
			}
			else
			{
				query = query.Where(pc => pc.active == true);
				query = query.Where(pc => pc.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Post Category, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.slug.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.slug);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.PostCategory.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/PostCategory/CreateAuditEvent")]
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
