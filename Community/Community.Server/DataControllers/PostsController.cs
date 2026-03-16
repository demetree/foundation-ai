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
using Foundation.ChangeHistory;

namespace Foundation.Community.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the Post entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Post entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class PostsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 10;

		static object postPutSyncRoot = new object();
		static object postDeleteSyncRoot = new object();

		private CommunityContext _context;

		private ILogger<PostsController> _logger;

		public PostsController(CommunityContext context, ILogger<PostsController> logger) : base("Community", "Post")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Posts filtered by the parameters provided.
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
		[Route("api/Posts")]
		public async Task<IActionResult> GetPosts(
			string title = null,
			string slug = null,
			string body = null,
			string excerpt = null,
			string authorName = null,
			int? postCategoryId = null,
			string featuredImageUrl = null,
			string metaDescription = null,
			bool? isPublished = null,
			DateTime? publishedDate = null,
			bool? isFeatured = null,
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
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
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
			if (publishedDate.HasValue == true && publishedDate.Value.Kind != DateTimeKind.Utc)
			{
				publishedDate = publishedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.Post> query = (from p in _context.Posts select p);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(title) == false)
			{
				query = query.Where(p => p.title == title);
			}
			if (string.IsNullOrEmpty(slug) == false)
			{
				query = query.Where(p => p.slug == slug);
			}
			if (string.IsNullOrEmpty(body) == false)
			{
				query = query.Where(p => p.body == body);
			}
			if (string.IsNullOrEmpty(excerpt) == false)
			{
				query = query.Where(p => p.excerpt == excerpt);
			}
			if (string.IsNullOrEmpty(authorName) == false)
			{
				query = query.Where(p => p.authorName == authorName);
			}
			if (postCategoryId.HasValue == true)
			{
				query = query.Where(p => p.postCategoryId == postCategoryId.Value);
			}
			if (string.IsNullOrEmpty(featuredImageUrl) == false)
			{
				query = query.Where(p => p.featuredImageUrl == featuredImageUrl);
			}
			if (string.IsNullOrEmpty(metaDescription) == false)
			{
				query = query.Where(p => p.metaDescription == metaDescription);
			}
			if (isPublished.HasValue == true)
			{
				query = query.Where(p => p.isPublished == isPublished.Value);
			}
			if (publishedDate.HasValue == true)
			{
				query = query.Where(p => p.publishedDate == publishedDate.Value);
			}
			if (isFeatured.HasValue == true)
			{
				query = query.Where(p => p.isFeatured == isFeatured.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(p => p.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(p => p.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(p => p.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(p => p.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(p => p.deleted == false);
				}
			}
			else
			{
				query = query.Where(p => p.active == true);
				query = query.Where(p => p.deleted == false);
			}

			query = query.OrderBy(p => p.title).ThenBy(p => p.slug).ThenBy(p => p.excerpt);


			//
			// Add the any string contains parameter to span all the string fields on the Post, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.title.Contains(anyStringContains)
			       || x.slug.Contains(anyStringContains)
			       || x.body.Contains(anyStringContains)
			       || x.excerpt.Contains(anyStringContains)
			       || x.authorName.Contains(anyStringContains)
			       || x.featuredImageUrl.Contains(anyStringContains)
			       || x.metaDescription.Contains(anyStringContains)
			       || (includeRelations == true && x.postCategory.name.Contains(anyStringContains))
			       || (includeRelations == true && x.postCategory.description.Contains(anyStringContains))
			       || (includeRelations == true && x.postCategory.slug.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.postCategory);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.Post> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Post post in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(post, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Community.Post Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Community.Post Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Posts filtered by the parameters provided.  Its query is similar to the GetPosts method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Posts/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string title = null,
			string slug = null,
			string body = null,
			string excerpt = null,
			string authorName = null,
			int? postCategoryId = null,
			string featuredImageUrl = null,
			string metaDescription = null,
			bool? isPublished = null,
			DateTime? publishedDate = null,
			bool? isFeatured = null,
			int? versionNumber = null,
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
			if (publishedDate.HasValue == true && publishedDate.Value.Kind != DateTimeKind.Utc)
			{
				publishedDate = publishedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.Post> query = (from p in _context.Posts select p);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (title != null)
			{
				query = query.Where(p => p.title == title);
			}
			if (slug != null)
			{
				query = query.Where(p => p.slug == slug);
			}
			if (body != null)
			{
				query = query.Where(p => p.body == body);
			}
			if (excerpt != null)
			{
				query = query.Where(p => p.excerpt == excerpt);
			}
			if (authorName != null)
			{
				query = query.Where(p => p.authorName == authorName);
			}
			if (postCategoryId.HasValue == true)
			{
				query = query.Where(p => p.postCategoryId == postCategoryId.Value);
			}
			if (featuredImageUrl != null)
			{
				query = query.Where(p => p.featuredImageUrl == featuredImageUrl);
			}
			if (metaDescription != null)
			{
				query = query.Where(p => p.metaDescription == metaDescription);
			}
			if (isPublished.HasValue == true)
			{
				query = query.Where(p => p.isPublished == isPublished.Value);
			}
			if (publishedDate.HasValue == true)
			{
				query = query.Where(p => p.publishedDate == publishedDate.Value);
			}
			if (isFeatured.HasValue == true)
			{
				query = query.Where(p => p.isFeatured == isFeatured.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(p => p.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(p => p.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(p => p.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(p => p.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(p => p.deleted == false);
				}
			}
			else
			{
				query = query.Where(p => p.active == true);
				query = query.Where(p => p.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Post, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.title.Contains(anyStringContains)
			       || x.slug.Contains(anyStringContains)
			       || x.body.Contains(anyStringContains)
			       || x.excerpt.Contains(anyStringContains)
			       || x.authorName.Contains(anyStringContains)
			       || x.featuredImageUrl.Contains(anyStringContains)
			       || x.metaDescription.Contains(anyStringContains)
			       || x.postCategory.name.Contains(anyStringContains)
			       || x.postCategory.description.Contains(anyStringContains)
			       || x.postCategory.slug.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single Post by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Post/{id}")]
		public async Task<IActionResult> GetPost(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.Post> query = (from p in _context.Posts where
							(p.id == id) &&
							(userIsAdmin == true || p.deleted == false) &&
							(userIsWriter == true || p.active == true)
					select p);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.postCategory);
					query = query.AsSplitQuery();
				}

				Database.Post materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Community.Post Entity was read with Admin privilege." : "Community.Post Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Post", materialized.id, materialized.title));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Community.Post entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Community.Post.   Entity was read with Admin privilege." : "Exception caught during entity read of Community.Post.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing Post record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Post/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutPost(int id, [FromBody]Database.Post.PostDTO postDTO, CancellationToken cancellationToken = default)
		{
			if (postDTO == null)
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



			if (id != postDTO.id)
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


			IQueryable<Database.Post> query = (from x in _context.Posts
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Post existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.Post PUT", id.ToString(), new Exception("No Community.Post entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (postDTO.objectGuid == Guid.Empty)
            {
                postDTO.objectGuid = existing.objectGuid;
            }
            else if (postDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Post record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Post cloneOfExisting = (Database.Post)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Post object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Post post = (Database.Post)_context.Entry(existing).GetDatabaseValues().ToObject();
			post.ApplyDTO(postDTO);
			//
			// The tenant guid for any Post being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the Post because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				post.tenantGuid = existing.tenantGuid;
			}

			lock (postPutSyncRoot)
			{
				//
				// Validate the version number for the post being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != post.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Post save attempt was made but save request was with version " + post.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The Post you are trying to update has already changed.  Please try your save again after reloading the Post.");
				}
				else
				{
					// Same record.  Increase version.
					post.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (post.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Community.Post record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (post.title != null && post.title.Length > 250)
				{
					post.title = post.title.Substring(0, 250);
				}

				if (post.slug != null && post.slug.Length > 250)
				{
					post.slug = post.slug.Substring(0, 250);
				}

				if (post.excerpt != null && post.excerpt.Length > 500)
				{
					post.excerpt = post.excerpt.Substring(0, 500);
				}

				if (post.authorName != null && post.authorName.Length > 100)
				{
					post.authorName = post.authorName.Substring(0, 100);
				}

				if (post.featuredImageUrl != null && post.featuredImageUrl.Length > 500)
				{
					post.featuredImageUrl = post.featuredImageUrl.Substring(0, 500);
				}

				if (post.metaDescription != null && post.metaDescription.Length > 500)
				{
					post.metaDescription = post.metaDescription.Substring(0, 500);
				}

				if (post.publishedDate.HasValue == true && post.publishedDate.Value.Kind != DateTimeKind.Utc)
				{
					post.publishedDate = post.publishedDate.Value.ToUniversalTime();
				}

				try
				{
				    EntityEntry<Database.Post> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(post);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        PostChangeHistory postChangeHistory = new PostChangeHistory();
				        postChangeHistory.postId = post.id;
				        postChangeHistory.versionNumber = post.versionNumber;
				        postChangeHistory.timeStamp = DateTime.UtcNow;
				        postChangeHistory.userId = securityUser.id;
				        postChangeHistory.tenantGuid = userTenantGuid;
				        postChangeHistory.data = JsonSerializer.Serialize(Database.Post.CreateAnonymousWithFirstLevelSubObjects(post));
				        _context.PostChangeHistories.Add(postChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Community.Post entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Post.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Post.CreateAnonymousWithFirstLevelSubObjects(post)),
						null);

				return Ok(Database.Post.CreateAnonymous(post));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Community.Post entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Post.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Post.CreateAnonymousWithFirstLevelSubObjects(post)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new Post record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Post", Name = "Post")]
		public async Task<IActionResult> PostPost([FromBody]Database.Post.PostDTO postDTO, CancellationToken cancellationToken = default)
		{
			if (postDTO == null)
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
			// Create a new Post object using the data from the DTO
			//
			Database.Post post = Database.Post.FromDTO(postDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				post.tenantGuid = userTenantGuid;

				if (post.title != null && post.title.Length > 250)
				{
					post.title = post.title.Substring(0, 250);
				}

				if (post.slug != null && post.slug.Length > 250)
				{
					post.slug = post.slug.Substring(0, 250);
				}

				if (post.excerpt != null && post.excerpt.Length > 500)
				{
					post.excerpt = post.excerpt.Substring(0, 500);
				}

				if (post.authorName != null && post.authorName.Length > 100)
				{
					post.authorName = post.authorName.Substring(0, 100);
				}

				if (post.featuredImageUrl != null && post.featuredImageUrl.Length > 500)
				{
					post.featuredImageUrl = post.featuredImageUrl.Substring(0, 500);
				}

				if (post.metaDescription != null && post.metaDescription.Length > 500)
				{
					post.metaDescription = post.metaDescription.Substring(0, 500);
				}

				if (post.publishedDate.HasValue == true && post.publishedDate.Value.Kind != DateTimeKind.Utc)
				{
					post.publishedDate = post.publishedDate.Value.ToUniversalTime();
				}

				post.objectGuid = Guid.NewGuid();
				post.versionNumber = 1;

				_context.Posts.Add(post);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the post object so that no further changes will be written to the database
				    //
				    _context.Entry(post).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					post.PostChangeHistories = null;
					post.PostTagAssignments = null;
					post.postCategory = null;


				    PostChangeHistory postChangeHistory = new PostChangeHistory();
				    postChangeHistory.postId = post.id;
				    postChangeHistory.versionNumber = post.versionNumber;
				    postChangeHistory.timeStamp = DateTime.UtcNow;
				    postChangeHistory.userId = securityUser.id;
				    postChangeHistory.tenantGuid = userTenantGuid;
				    postChangeHistory.data = JsonSerializer.Serialize(Database.Post.CreateAnonymousWithFirstLevelSubObjects(post));
				    _context.PostChangeHistories.Add(postChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Community.Post entity successfully created.",
						true,
						post. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.Post.CreateAnonymousWithFirstLevelSubObjects(post)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Community.Post entity creation failed.", false, post.id.ToString(), "", JsonSerializer.Serialize(Database.Post.CreateAnonymousWithFirstLevelSubObjects(post)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Post", post.id, post.title));

			return CreatedAtRoute("Post", new { id = post.id }, Database.Post.CreateAnonymousWithFirstLevelSubObjects(post));
		}



        /// <summary>
        /// 
        /// This rolls a Post entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Post/Rollback/{id}")]
		[Route("api/Post/Rollback")]
		public async Task<IActionResult> RollbackToPostVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.Post> query = (from x in _context.Posts
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this Post concurrently
			//
			lock (postPutSyncRoot)
			{
				
				Database.Post post = query.FirstOrDefault();
				
				if (post == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.Post rollback", id.ToString(), new Exception("No Community.Post entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the Post current state so we can log it.
				//
				Database.Post cloneOfExisting = (Database.Post)_context.Entry(post).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.PostChangeHistories = null;
				cloneOfExisting.PostTagAssignments = null;
				cloneOfExisting.postCategory = null;

				if (versionNumber >= post.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Community.Post rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Community.Post rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				PostChangeHistory postChangeHistory = (from x in _context.PostChangeHistories
				                                               where
				                                               x.postId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (postChangeHistory != null)
				{
				    Database.Post oldPost = JsonSerializer.Deserialize<Database.Post>(postChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    post.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    post.title = oldPost.title;
				    post.slug = oldPost.slug;
				    post.body = oldPost.body;
				    post.excerpt = oldPost.excerpt;
				    post.authorName = oldPost.authorName;
				    post.postCategoryId = oldPost.postCategoryId;
				    post.featuredImageUrl = oldPost.featuredImageUrl;
				    post.metaDescription = oldPost.metaDescription;
				    post.isPublished = oldPost.isPublished;
				    post.publishedDate = oldPost.publishedDate;
				    post.isFeatured = oldPost.isFeatured;
				    post.objectGuid = oldPost.objectGuid;
				    post.active = oldPost.active;
				    post.deleted = oldPost.deleted;

				    string serializedPost = JsonSerializer.Serialize(post);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        PostChangeHistory newPostChangeHistory = new PostChangeHistory();
				        newPostChangeHistory.postId = post.id;
				        newPostChangeHistory.versionNumber = post.versionNumber;
				        newPostChangeHistory.timeStamp = DateTime.UtcNow;
				        newPostChangeHistory.userId = securityUser.id;
				        newPostChangeHistory.tenantGuid = userTenantGuid;
				        newPostChangeHistory.data = JsonSerializer.Serialize(Database.Post.CreateAnonymousWithFirstLevelSubObjects(post));
				        _context.PostChangeHistories.Add(newPostChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Community.Post rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Post.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Post.CreateAnonymousWithFirstLevelSubObjects(post)),
						null);


				    return Ok(Database.Post.CreateAnonymous(post));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Community.Post rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Community.Post rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a Post.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Post</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Post/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetPostChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.Post post = await _context.Posts.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (post == null)
			{
				return NotFound();
			}

			try
			{
				post.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Post> versionInfo = await post.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a Post.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Post</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Post/{id}/AuditHistory")]
		public async Task<IActionResult> GetPostAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.Post post = await _context.Posts.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (post == null)
			{
				return NotFound();
			}

			try
			{
				post.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.Post>> versions = await post.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a Post.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Post</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The Post object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Post/{id}/Version/{version}")]
		public async Task<IActionResult> GetPostVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.Post post = await _context.Posts.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (post == null)
			{
				return NotFound();
			}

			try
			{
				post.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Post> versionInfo = await post.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a Post at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Post</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The Post object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Post/{id}/StateAtTime")]
		public async Task<IActionResult> GetPostStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.Post post = await _context.Posts.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (post == null)
			{
				return NotFound();
			}

			try
			{
				post.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Post> versionInfo = await post.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a Post record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Post/{id}")]
		[Route("api/Post")]
		public async Task<IActionResult> DeletePost(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.Post> query = (from x in _context.Posts
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Post post = await query.FirstOrDefaultAsync(cancellationToken);

			if (post == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.Post DELETE", id.ToString(), new Exception("No Community.Post entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Post cloneOfExisting = (Database.Post)_context.Entry(post).GetDatabaseValues().ToObject();


			lock (postDeleteSyncRoot)
			{
			    try
			    {
			        post.deleted = true;
			        post.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        PostChangeHistory postChangeHistory = new PostChangeHistory();
			        postChangeHistory.postId = post.id;
			        postChangeHistory.versionNumber = post.versionNumber;
			        postChangeHistory.timeStamp = DateTime.UtcNow;
			        postChangeHistory.userId = securityUser.id;
			        postChangeHistory.tenantGuid = userTenantGuid;
			        postChangeHistory.data = JsonSerializer.Serialize(Database.Post.CreateAnonymousWithFirstLevelSubObjects(post));
			        _context.PostChangeHistories.Add(postChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Community.Post entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Post.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Post.CreateAnonymousWithFirstLevelSubObjects(post)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Community.Post entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Post.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Post.CreateAnonymousWithFirstLevelSubObjects(post)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of Post records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Posts/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string title = null,
			string slug = null,
			string body = null,
			string excerpt = null,
			string authorName = null,
			int? postCategoryId = null,
			string featuredImageUrl = null,
			string metaDescription = null,
			bool? isPublished = null,
			DateTime? publishedDate = null,
			bool? isFeatured = null,
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
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
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
			if (publishedDate.HasValue == true && publishedDate.Value.Kind != DateTimeKind.Utc)
			{
				publishedDate = publishedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.Post> query = (from p in _context.Posts select p);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(title) == false)
			{
				query = query.Where(p => p.title == title);
			}
			if (string.IsNullOrEmpty(slug) == false)
			{
				query = query.Where(p => p.slug == slug);
			}
			if (string.IsNullOrEmpty(body) == false)
			{
				query = query.Where(p => p.body == body);
			}
			if (string.IsNullOrEmpty(excerpt) == false)
			{
				query = query.Where(p => p.excerpt == excerpt);
			}
			if (string.IsNullOrEmpty(authorName) == false)
			{
				query = query.Where(p => p.authorName == authorName);
			}
			if (postCategoryId.HasValue == true)
			{
				query = query.Where(p => p.postCategoryId == postCategoryId.Value);
			}
			if (string.IsNullOrEmpty(featuredImageUrl) == false)
			{
				query = query.Where(p => p.featuredImageUrl == featuredImageUrl);
			}
			if (string.IsNullOrEmpty(metaDescription) == false)
			{
				query = query.Where(p => p.metaDescription == metaDescription);
			}
			if (isPublished.HasValue == true)
			{
				query = query.Where(p => p.isPublished == isPublished.Value);
			}
			if (publishedDate.HasValue == true)
			{
				query = query.Where(p => p.publishedDate == publishedDate.Value);
			}
			if (isFeatured.HasValue == true)
			{
				query = query.Where(p => p.isFeatured == isFeatured.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(p => p.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(p => p.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(p => p.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(p => p.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(p => p.deleted == false);
				}
			}
			else
			{
				query = query.Where(p => p.active == true);
				query = query.Where(p => p.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Post, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.title.Contains(anyStringContains)
			       || x.slug.Contains(anyStringContains)
			       || x.body.Contains(anyStringContains)
			       || x.excerpt.Contains(anyStringContains)
			       || x.authorName.Contains(anyStringContains)
			       || x.featuredImageUrl.Contains(anyStringContains)
			       || x.metaDescription.Contains(anyStringContains)
			       || x.postCategory.name.Contains(anyStringContains)
			       || x.postCategory.description.Contains(anyStringContains)
			       || x.postCategory.slug.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.title).ThenBy(x => x.slug).ThenBy(x => x.excerpt);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Post.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Post/CreateAuditEvent")]
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
