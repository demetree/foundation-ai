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
    /// This auto generated class provides the basic CRUD operations for the Page entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Page entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class PagesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 10;

		static object pagePutSyncRoot = new object();
		static object pageDeleteSyncRoot = new object();

		private CommunityContext _context;

		private ILogger<PagesController> _logger;

		public PagesController(CommunityContext context, ILogger<PagesController> logger) : base("Community", "Page")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Pages filtered by the parameters provided.
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
		[Route("api/Pages")]
		public async Task<IActionResult> GetPages(
			string title = null,
			string slug = null,
			string body = null,
			string metaDescription = null,
			string featuredImageUrl = null,
			bool? isPublished = null,
			DateTime? publishedDate = null,
			int? sortOrder = null,
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

			IQueryable<Database.Page> query = (from p in _context.Pages select p);

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
			if (string.IsNullOrEmpty(metaDescription) == false)
			{
				query = query.Where(p => p.metaDescription == metaDescription);
			}
			if (string.IsNullOrEmpty(featuredImageUrl) == false)
			{
				query = query.Where(p => p.featuredImageUrl == featuredImageUrl);
			}
			if (isPublished.HasValue == true)
			{
				query = query.Where(p => p.isPublished == isPublished.Value);
			}
			if (publishedDate.HasValue == true)
			{
				query = query.Where(p => p.publishedDate == publishedDate.Value);
			}
			if (sortOrder.HasValue == true)
			{
				query = query.Where(p => p.sortOrder == sortOrder.Value);
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

			query = query.OrderBy(p => p.title).ThenBy(p => p.slug).ThenBy(p => p.metaDescription);


			//
			// Add the any string contains parameter to span all the string fields on the Page, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.title.Contains(anyStringContains)
			       || x.slug.Contains(anyStringContains)
			       || x.body.Contains(anyStringContains)
			       || x.metaDescription.Contains(anyStringContains)
			       || x.featuredImageUrl.Contains(anyStringContains)
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
			
			List<Database.Page> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Page page in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(page, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Community.Page Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Community.Page Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Pages filtered by the parameters provided.  Its query is similar to the GetPages method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Pages/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string title = null,
			string slug = null,
			string body = null,
			string metaDescription = null,
			string featuredImageUrl = null,
			bool? isPublished = null,
			DateTime? publishedDate = null,
			int? sortOrder = null,
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

			IQueryable<Database.Page> query = (from p in _context.Pages select p);
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
			if (metaDescription != null)
			{
				query = query.Where(p => p.metaDescription == metaDescription);
			}
			if (featuredImageUrl != null)
			{
				query = query.Where(p => p.featuredImageUrl == featuredImageUrl);
			}
			if (isPublished.HasValue == true)
			{
				query = query.Where(p => p.isPublished == isPublished.Value);
			}
			if (publishedDate.HasValue == true)
			{
				query = query.Where(p => p.publishedDate == publishedDate.Value);
			}
			if (sortOrder.HasValue == true)
			{
				query = query.Where(p => p.sortOrder == sortOrder.Value);
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
			// Add the any string contains parameter to span all the string fields on the Page, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.title.Contains(anyStringContains)
			       || x.slug.Contains(anyStringContains)
			       || x.body.Contains(anyStringContains)
			       || x.metaDescription.Contains(anyStringContains)
			       || x.featuredImageUrl.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single Page by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Page/{id}")]
		public async Task<IActionResult> GetPage(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.Page> query = (from p in _context.Pages where
							(p.id == id) &&
							(userIsAdmin == true || p.deleted == false) &&
							(userIsWriter == true || p.active == true)
					select p);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.Page materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Community.Page Entity was read with Admin privilege." : "Community.Page Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Page", materialized.id, materialized.title));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Community.Page entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Community.Page.   Entity was read with Admin privilege." : "Exception caught during entity read of Community.Page.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing Page record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Page/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutPage(int id, [FromBody]Database.Page.PageDTO pageDTO, CancellationToken cancellationToken = default)
		{
			if (pageDTO == null)
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



			if (id != pageDTO.id)
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


			IQueryable<Database.Page> query = (from x in _context.Pages
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Page existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.Page PUT", id.ToString(), new Exception("No Community.Page entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (pageDTO.objectGuid == Guid.Empty)
            {
                pageDTO.objectGuid = existing.objectGuid;
            }
            else if (pageDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Page record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Page cloneOfExisting = (Database.Page)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Page object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Page page = (Database.Page)_context.Entry(existing).GetDatabaseValues().ToObject();
			page.ApplyDTO(pageDTO);
			//
			// The tenant guid for any Page being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the Page because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				page.tenantGuid = existing.tenantGuid;
			}

			lock (pagePutSyncRoot)
			{
				//
				// Validate the version number for the page being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != page.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Page save attempt was made but save request was with version " + page.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The Page you are trying to update has already changed.  Please try your save again after reloading the Page.");
				}
				else
				{
					// Same record.  Increase version.
					page.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (page.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Community.Page record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (page.title != null && page.title.Length > 250)
				{
					page.title = page.title.Substring(0, 250);
				}

				if (page.slug != null && page.slug.Length > 250)
				{
					page.slug = page.slug.Substring(0, 250);
				}

				if (page.metaDescription != null && page.metaDescription.Length > 500)
				{
					page.metaDescription = page.metaDescription.Substring(0, 500);
				}

				if (page.featuredImageUrl != null && page.featuredImageUrl.Length > 500)
				{
					page.featuredImageUrl = page.featuredImageUrl.Substring(0, 500);
				}

				if (page.publishedDate.HasValue == true && page.publishedDate.Value.Kind != DateTimeKind.Utc)
				{
					page.publishedDate = page.publishedDate.Value.ToUniversalTime();
				}

				try
				{
				    EntityEntry<Database.Page> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(page);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        PageChangeHistory pageChangeHistory = new PageChangeHistory();
				        pageChangeHistory.pageId = page.id;
				        pageChangeHistory.versionNumber = page.versionNumber;
				        pageChangeHistory.timeStamp = DateTime.UtcNow;
				        pageChangeHistory.userId = securityUser.id;
				        pageChangeHistory.tenantGuid = userTenantGuid;
				        pageChangeHistory.data = JsonSerializer.Serialize(Database.Page.CreateAnonymousWithFirstLevelSubObjects(page));
				        _context.PageChangeHistories.Add(pageChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Community.Page entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Page.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Page.CreateAnonymousWithFirstLevelSubObjects(page)),
						null);

				return Ok(Database.Page.CreateAnonymous(page));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Community.Page entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Page.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Page.CreateAnonymousWithFirstLevelSubObjects(page)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new Page record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Page", Name = "Page")]
		public async Task<IActionResult> PostPage([FromBody]Database.Page.PageDTO pageDTO, CancellationToken cancellationToken = default)
		{
			if (pageDTO == null)
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
			// Create a new Page object using the data from the DTO
			//
			Database.Page page = Database.Page.FromDTO(pageDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				page.tenantGuid = userTenantGuid;

				if (page.title != null && page.title.Length > 250)
				{
					page.title = page.title.Substring(0, 250);
				}

				if (page.slug != null && page.slug.Length > 250)
				{
					page.slug = page.slug.Substring(0, 250);
				}

				if (page.metaDescription != null && page.metaDescription.Length > 500)
				{
					page.metaDescription = page.metaDescription.Substring(0, 500);
				}

				if (page.featuredImageUrl != null && page.featuredImageUrl.Length > 500)
				{
					page.featuredImageUrl = page.featuredImageUrl.Substring(0, 500);
				}

				if (page.publishedDate.HasValue == true && page.publishedDate.Value.Kind != DateTimeKind.Utc)
				{
					page.publishedDate = page.publishedDate.Value.ToUniversalTime();
				}

				page.objectGuid = Guid.NewGuid();
				page.versionNumber = 1;

				_context.Pages.Add(page);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the page object so that no further changes will be written to the database
				    //
				    _context.Entry(page).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					page.MenuItems = null;
					page.PageChangeHistories = null;


				    PageChangeHistory pageChangeHistory = new PageChangeHistory();
				    pageChangeHistory.pageId = page.id;
				    pageChangeHistory.versionNumber = page.versionNumber;
				    pageChangeHistory.timeStamp = DateTime.UtcNow;
				    pageChangeHistory.userId = securityUser.id;
				    pageChangeHistory.tenantGuid = userTenantGuid;
				    pageChangeHistory.data = JsonSerializer.Serialize(Database.Page.CreateAnonymousWithFirstLevelSubObjects(page));
				    _context.PageChangeHistories.Add(pageChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Community.Page entity successfully created.",
						true,
						page. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.Page.CreateAnonymousWithFirstLevelSubObjects(page)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Community.Page entity creation failed.", false, page.id.ToString(), "", JsonSerializer.Serialize(Database.Page.CreateAnonymousWithFirstLevelSubObjects(page)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Page", page.id, page.title));

			return CreatedAtRoute("Page", new { id = page.id }, Database.Page.CreateAnonymousWithFirstLevelSubObjects(page));
		}



        /// <summary>
        /// 
        /// This rolls a Page entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Page/Rollback/{id}")]
		[Route("api/Page/Rollback")]
		public async Task<IActionResult> RollbackToPageVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.Page> query = (from x in _context.Pages
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this Page concurrently
			//
			lock (pagePutSyncRoot)
			{
				
				Database.Page page = query.FirstOrDefault();
				
				if (page == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.Page rollback", id.ToString(), new Exception("No Community.Page entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the Page current state so we can log it.
				//
				Database.Page cloneOfExisting = (Database.Page)_context.Entry(page).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.MenuItems = null;
				cloneOfExisting.PageChangeHistories = null;

				if (versionNumber >= page.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Community.Page rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Community.Page rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				PageChangeHistory pageChangeHistory = (from x in _context.PageChangeHistories
				                                               where
				                                               x.pageId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (pageChangeHistory != null)
				{
				    Database.Page oldPage = JsonSerializer.Deserialize<Database.Page>(pageChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    page.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    page.title = oldPage.title;
				    page.slug = oldPage.slug;
				    page.body = oldPage.body;
				    page.metaDescription = oldPage.metaDescription;
				    page.featuredImageUrl = oldPage.featuredImageUrl;
				    page.isPublished = oldPage.isPublished;
				    page.publishedDate = oldPage.publishedDate;
				    page.sortOrder = oldPage.sortOrder;
				    page.objectGuid = oldPage.objectGuid;
				    page.active = oldPage.active;
				    page.deleted = oldPage.deleted;

				    string serializedPage = JsonSerializer.Serialize(page);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        PageChangeHistory newPageChangeHistory = new PageChangeHistory();
				        newPageChangeHistory.pageId = page.id;
				        newPageChangeHistory.versionNumber = page.versionNumber;
				        newPageChangeHistory.timeStamp = DateTime.UtcNow;
				        newPageChangeHistory.userId = securityUser.id;
				        newPageChangeHistory.tenantGuid = userTenantGuid;
				        newPageChangeHistory.data = JsonSerializer.Serialize(Database.Page.CreateAnonymousWithFirstLevelSubObjects(page));
				        _context.PageChangeHistories.Add(newPageChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Community.Page rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Page.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Page.CreateAnonymousWithFirstLevelSubObjects(page)),
						null);


				    return Ok(Database.Page.CreateAnonymous(page));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Community.Page rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Community.Page rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a Page.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Page</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Page/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetPageChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.Page page = await _context.Pages.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (page == null)
			{
				return NotFound();
			}

			try
			{
				page.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Page> versionInfo = await page.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a Page.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Page</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Page/{id}/AuditHistory")]
		public async Task<IActionResult> GetPageAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.Page page = await _context.Pages.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (page == null)
			{
				return NotFound();
			}

			try
			{
				page.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.Page>> versions = await page.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a Page.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Page</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The Page object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Page/{id}/Version/{version}")]
		public async Task<IActionResult> GetPageVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.Page page = await _context.Pages.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (page == null)
			{
				return NotFound();
			}

			try
			{
				page.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Page> versionInfo = await page.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a Page at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Page</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The Page object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Page/{id}/StateAtTime")]
		public async Task<IActionResult> GetPageStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.Page page = await _context.Pages.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (page == null)
			{
				return NotFound();
			}

			try
			{
				page.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Page> versionInfo = await page.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a Page record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Page/{id}")]
		[Route("api/Page")]
		public async Task<IActionResult> DeletePage(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.Page> query = (from x in _context.Pages
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Page page = await query.FirstOrDefaultAsync(cancellationToken);

			if (page == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.Page DELETE", id.ToString(), new Exception("No Community.Page entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Page cloneOfExisting = (Database.Page)_context.Entry(page).GetDatabaseValues().ToObject();


			lock (pageDeleteSyncRoot)
			{
			    try
			    {
			        page.deleted = true;
			        page.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        PageChangeHistory pageChangeHistory = new PageChangeHistory();
			        pageChangeHistory.pageId = page.id;
			        pageChangeHistory.versionNumber = page.versionNumber;
			        pageChangeHistory.timeStamp = DateTime.UtcNow;
			        pageChangeHistory.userId = securityUser.id;
			        pageChangeHistory.tenantGuid = userTenantGuid;
			        pageChangeHistory.data = JsonSerializer.Serialize(Database.Page.CreateAnonymousWithFirstLevelSubObjects(page));
			        _context.PageChangeHistories.Add(pageChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Community.Page entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Page.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Page.CreateAnonymousWithFirstLevelSubObjects(page)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Community.Page entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Page.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Page.CreateAnonymousWithFirstLevelSubObjects(page)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of Page records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Pages/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string title = null,
			string slug = null,
			string body = null,
			string metaDescription = null,
			string featuredImageUrl = null,
			bool? isPublished = null,
			DateTime? publishedDate = null,
			int? sortOrder = null,
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

			IQueryable<Database.Page> query = (from p in _context.Pages select p);

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
			if (string.IsNullOrEmpty(metaDescription) == false)
			{
				query = query.Where(p => p.metaDescription == metaDescription);
			}
			if (string.IsNullOrEmpty(featuredImageUrl) == false)
			{
				query = query.Where(p => p.featuredImageUrl == featuredImageUrl);
			}
			if (isPublished.HasValue == true)
			{
				query = query.Where(p => p.isPublished == isPublished.Value);
			}
			if (publishedDate.HasValue == true)
			{
				query = query.Where(p => p.publishedDate == publishedDate.Value);
			}
			if (sortOrder.HasValue == true)
			{
				query = query.Where(p => p.sortOrder == sortOrder.Value);
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
			// Add the any string contains parameter to span all the string fields on the Page, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.title.Contains(anyStringContains)
			       || x.slug.Contains(anyStringContains)
			       || x.body.Contains(anyStringContains)
			       || x.metaDescription.Contains(anyStringContains)
			       || x.featuredImageUrl.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.title).ThenBy(x => x.slug).ThenBy(x => x.metaDescription);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Page.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Page/CreateAuditEvent")]
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
