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
    /// This auto generated class provides the basic CRUD operations for the PublishedMoc entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the PublishedMoc entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class PublishedMocsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object publishedMocPutSyncRoot = new object();
		static object publishedMocDeleteSyncRoot = new object();

		private BMCContext _context;

		private ILogger<PublishedMocsController> _logger;

		public PublishedMocsController(BMCContext context, ILogger<PublishedMocsController> logger) : base("BMC", "PublishedMoc")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of PublishedMocs filtered by the parameters provided.
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
		[Route("api/PublishedMocs")]
		public async Task<IActionResult> GetPublishedMocs(
			int? projectId = null,
			string name = null,
			string description = null,
			string thumbnailImagePath = null,
			string tags = null,
			bool? isPublished = null,
			bool? isFeatured = null,
			DateTime? publishedDate = null,
			int? viewCount = null,
			int? likeCount = null,
			int? commentCount = null,
			int? favouriteCount = null,
			int? partCount = null,
			bool? allowForking = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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

			IQueryable<Database.PublishedMoc> query = (from pm in _context.PublishedMocs select pm);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (projectId.HasValue == true)
			{
				query = query.Where(pm => pm.projectId == projectId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(pm => pm.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(pm => pm.description == description);
			}
			if (string.IsNullOrEmpty(thumbnailImagePath) == false)
			{
				query = query.Where(pm => pm.thumbnailImagePath == thumbnailImagePath);
			}
			if (string.IsNullOrEmpty(tags) == false)
			{
				query = query.Where(pm => pm.tags == tags);
			}
			if (isPublished.HasValue == true)
			{
				query = query.Where(pm => pm.isPublished == isPublished.Value);
			}
			if (isFeatured.HasValue == true)
			{
				query = query.Where(pm => pm.isFeatured == isFeatured.Value);
			}
			if (publishedDate.HasValue == true)
			{
				query = query.Where(pm => pm.publishedDate == publishedDate.Value);
			}
			if (viewCount.HasValue == true)
			{
				query = query.Where(pm => pm.viewCount == viewCount.Value);
			}
			if (likeCount.HasValue == true)
			{
				query = query.Where(pm => pm.likeCount == likeCount.Value);
			}
			if (commentCount.HasValue == true)
			{
				query = query.Where(pm => pm.commentCount == commentCount.Value);
			}
			if (favouriteCount.HasValue == true)
			{
				query = query.Where(pm => pm.favouriteCount == favouriteCount.Value);
			}
			if (partCount.HasValue == true)
			{
				query = query.Where(pm => pm.partCount == partCount.Value);
			}
			if (allowForking.HasValue == true)
			{
				query = query.Where(pm => pm.allowForking == allowForking.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(pm => pm.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pm => pm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pm => pm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pm => pm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pm => pm.deleted == false);
				}
			}
			else
			{
				query = query.Where(pm => pm.active == true);
				query = query.Where(pm => pm.deleted == false);
			}

			query = query.OrderBy(pm => pm.name).ThenBy(pm => pm.thumbnailImagePath);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.project);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Published Moc, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.thumbnailImagePath.Contains(anyStringContains)
			       || x.tags.Contains(anyStringContains)
			       || (includeRelations == true && x.project.name.Contains(anyStringContains))
			       || (includeRelations == true && x.project.description.Contains(anyStringContains))
			       || (includeRelations == true && x.project.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.project.thumbnailImagePath.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.PublishedMoc> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.PublishedMoc publishedMoc in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(publishedMoc, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.PublishedMoc Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.PublishedMoc Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of PublishedMocs filtered by the parameters provided.  Its query is similar to the GetPublishedMocs method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PublishedMocs/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? projectId = null,
			string name = null,
			string description = null,
			string thumbnailImagePath = null,
			string tags = null,
			bool? isPublished = null,
			bool? isFeatured = null,
			DateTime? publishedDate = null,
			int? viewCount = null,
			int? likeCount = null,
			int? commentCount = null,
			int? favouriteCount = null,
			int? partCount = null,
			bool? allowForking = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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

			IQueryable<Database.PublishedMoc> query = (from pm in _context.PublishedMocs select pm);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (projectId.HasValue == true)
			{
				query = query.Where(pm => pm.projectId == projectId.Value);
			}
			if (name != null)
			{
				query = query.Where(pm => pm.name == name);
			}
			if (description != null)
			{
				query = query.Where(pm => pm.description == description);
			}
			if (thumbnailImagePath != null)
			{
				query = query.Where(pm => pm.thumbnailImagePath == thumbnailImagePath);
			}
			if (tags != null)
			{
				query = query.Where(pm => pm.tags == tags);
			}
			if (isPublished.HasValue == true)
			{
				query = query.Where(pm => pm.isPublished == isPublished.Value);
			}
			if (isFeatured.HasValue == true)
			{
				query = query.Where(pm => pm.isFeatured == isFeatured.Value);
			}
			if (publishedDate.HasValue == true)
			{
				query = query.Where(pm => pm.publishedDate == publishedDate.Value);
			}
			if (viewCount.HasValue == true)
			{
				query = query.Where(pm => pm.viewCount == viewCount.Value);
			}
			if (likeCount.HasValue == true)
			{
				query = query.Where(pm => pm.likeCount == likeCount.Value);
			}
			if (commentCount.HasValue == true)
			{
				query = query.Where(pm => pm.commentCount == commentCount.Value);
			}
			if (favouriteCount.HasValue == true)
			{
				query = query.Where(pm => pm.favouriteCount == favouriteCount.Value);
			}
			if (partCount.HasValue == true)
			{
				query = query.Where(pm => pm.partCount == partCount.Value);
			}
			if (allowForking.HasValue == true)
			{
				query = query.Where(pm => pm.allowForking == allowForking.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(pm => pm.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pm => pm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pm => pm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pm => pm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pm => pm.deleted == false);
				}
			}
			else
			{
				query = query.Where(pm => pm.active == true);
				query = query.Where(pm => pm.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Published Moc, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.thumbnailImagePath.Contains(anyStringContains)
			       || x.tags.Contains(anyStringContains)
			       || x.project.name.Contains(anyStringContains)
			       || x.project.description.Contains(anyStringContains)
			       || x.project.notes.Contains(anyStringContains)
			       || x.project.thumbnailImagePath.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single PublishedMoc by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PublishedMoc/{id}")]
		public async Task<IActionResult> GetPublishedMoc(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.PublishedMoc> query = (from pm in _context.PublishedMocs where
							(pm.id == id) &&
							(userIsAdmin == true || pm.deleted == false) &&
							(userIsWriter == true || pm.active == true)
					select pm);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.project);
					query = query.AsSplitQuery();
				}

				Database.PublishedMoc materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.PublishedMoc Entity was read with Admin privilege." : "BMC.PublishedMoc Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PublishedMoc", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.PublishedMoc entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.PublishedMoc.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.PublishedMoc.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing PublishedMoc record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/PublishedMoc/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutPublishedMoc(int id, [FromBody]Database.PublishedMoc.PublishedMocDTO publishedMocDTO, CancellationToken cancellationToken = default)
		{
			if (publishedMocDTO == null)
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



			if (id != publishedMocDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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


			IQueryable<Database.PublishedMoc> query = (from x in _context.PublishedMocs
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.PublishedMoc existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.PublishedMoc PUT", id.ToString(), new Exception("No BMC.PublishedMoc entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (publishedMocDTO.objectGuid == Guid.Empty)
            {
                publishedMocDTO.objectGuid = existing.objectGuid;
            }
            else if (publishedMocDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a PublishedMoc record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.PublishedMoc cloneOfExisting = (Database.PublishedMoc)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new PublishedMoc object using the data from the existing record, updated with what is in the DTO.
			//
			Database.PublishedMoc publishedMoc = (Database.PublishedMoc)_context.Entry(existing).GetDatabaseValues().ToObject();
			publishedMoc.ApplyDTO(publishedMocDTO);
			//
			// The tenant guid for any PublishedMoc being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the PublishedMoc because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				publishedMoc.tenantGuid = existing.tenantGuid;
			}

			lock (publishedMocPutSyncRoot)
			{
				//
				// Validate the version number for the publishedMoc being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != publishedMoc.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "PublishedMoc save attempt was made but save request was with version " + publishedMoc.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The PublishedMoc you are trying to update has already changed.  Please try your save again after reloading the PublishedMoc.");
				}
				else
				{
					// Same record.  Increase version.
					publishedMoc.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (publishedMoc.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.PublishedMoc record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (publishedMoc.name != null && publishedMoc.name.Length > 100)
				{
					publishedMoc.name = publishedMoc.name.Substring(0, 100);
				}

				if (publishedMoc.thumbnailImagePath != null && publishedMoc.thumbnailImagePath.Length > 250)
				{
					publishedMoc.thumbnailImagePath = publishedMoc.thumbnailImagePath.Substring(0, 250);
				}

				if (publishedMoc.publishedDate.HasValue == true && publishedMoc.publishedDate.Value.Kind != DateTimeKind.Utc)
				{
					publishedMoc.publishedDate = publishedMoc.publishedDate.Value.ToUniversalTime();
				}

				try
				{
				    EntityEntry<Database.PublishedMoc> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(publishedMoc);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        PublishedMocChangeHistory publishedMocChangeHistory = new PublishedMocChangeHistory();
				        publishedMocChangeHistory.publishedMocId = publishedMoc.id;
				        publishedMocChangeHistory.versionNumber = publishedMoc.versionNumber;
				        publishedMocChangeHistory.timeStamp = DateTime.UtcNow;
				        publishedMocChangeHistory.userId = securityUser.id;
				        publishedMocChangeHistory.tenantGuid = userTenantGuid;
				        publishedMocChangeHistory.data = JsonSerializer.Serialize(Database.PublishedMoc.CreateAnonymousWithFirstLevelSubObjects(publishedMoc));
				        _context.PublishedMocChangeHistories.Add(publishedMocChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.PublishedMoc entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.PublishedMoc.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.PublishedMoc.CreateAnonymousWithFirstLevelSubObjects(publishedMoc)),
						null);

				return Ok(Database.PublishedMoc.CreateAnonymous(publishedMoc));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.PublishedMoc entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.PublishedMoc.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.PublishedMoc.CreateAnonymousWithFirstLevelSubObjects(publishedMoc)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new PublishedMoc record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PublishedMoc", Name = "PublishedMoc")]
		public async Task<IActionResult> PostPublishedMoc([FromBody]Database.PublishedMoc.PublishedMocDTO publishedMocDTO, CancellationToken cancellationToken = default)
		{
			if (publishedMocDTO == null)
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
			// Create a new PublishedMoc object using the data from the DTO
			//
			Database.PublishedMoc publishedMoc = Database.PublishedMoc.FromDTO(publishedMocDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				publishedMoc.tenantGuid = userTenantGuid;

				if (publishedMoc.name != null && publishedMoc.name.Length > 100)
				{
					publishedMoc.name = publishedMoc.name.Substring(0, 100);
				}

				if (publishedMoc.thumbnailImagePath != null && publishedMoc.thumbnailImagePath.Length > 250)
				{
					publishedMoc.thumbnailImagePath = publishedMoc.thumbnailImagePath.Substring(0, 250);
				}

				if (publishedMoc.publishedDate.HasValue == true && publishedMoc.publishedDate.Value.Kind != DateTimeKind.Utc)
				{
					publishedMoc.publishedDate = publishedMoc.publishedDate.Value.ToUniversalTime();
				}

				publishedMoc.objectGuid = Guid.NewGuid();
				publishedMoc.versionNumber = 1;

				_context.PublishedMocs.Add(publishedMoc);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the publishedMoc object so that no further changes will be written to the database
				    //
				    _context.Entry(publishedMoc).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					publishedMoc.BuildChallengeEntries = null;
					publishedMoc.MocComments = null;
					publishedMoc.MocFavourites = null;
					publishedMoc.MocLikes = null;
					publishedMoc.PublishedMocChangeHistories = null;
					publishedMoc.PublishedMocImages = null;
					publishedMoc.SharedInstructions = null;
					publishedMoc.project = null;


				    PublishedMocChangeHistory publishedMocChangeHistory = new PublishedMocChangeHistory();
				    publishedMocChangeHistory.publishedMocId = publishedMoc.id;
				    publishedMocChangeHistory.versionNumber = publishedMoc.versionNumber;
				    publishedMocChangeHistory.timeStamp = DateTime.UtcNow;
				    publishedMocChangeHistory.userId = securityUser.id;
				    publishedMocChangeHistory.tenantGuid = userTenantGuid;
				    publishedMocChangeHistory.data = JsonSerializer.Serialize(Database.PublishedMoc.CreateAnonymousWithFirstLevelSubObjects(publishedMoc));
				    _context.PublishedMocChangeHistories.Add(publishedMocChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"BMC.PublishedMoc entity successfully created.",
						true,
						publishedMoc. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.PublishedMoc.CreateAnonymousWithFirstLevelSubObjects(publishedMoc)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.PublishedMoc entity creation failed.", false, publishedMoc.id.ToString(), "", JsonSerializer.Serialize(Database.PublishedMoc.CreateAnonymousWithFirstLevelSubObjects(publishedMoc)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PublishedMoc", publishedMoc.id, publishedMoc.name));

			return CreatedAtRoute("PublishedMoc", new { id = publishedMoc.id }, Database.PublishedMoc.CreateAnonymousWithFirstLevelSubObjects(publishedMoc));
		}



        /// <summary>
        /// 
        /// This rolls a PublishedMoc entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PublishedMoc/Rollback/{id}")]
		[Route("api/PublishedMoc/Rollback")]
		public async Task<IActionResult> RollbackToPublishedMocVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.PublishedMoc> query = (from x in _context.PublishedMocs
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this PublishedMoc concurrently
			//
			lock (publishedMocPutSyncRoot)
			{
				
				Database.PublishedMoc publishedMoc = query.FirstOrDefault();
				
				if (publishedMoc == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.PublishedMoc rollback", id.ToString(), new Exception("No BMC.PublishedMoc entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the PublishedMoc current state so we can log it.
				//
				Database.PublishedMoc cloneOfExisting = (Database.PublishedMoc)_context.Entry(publishedMoc).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.BuildChallengeEntries = null;
				cloneOfExisting.MocComments = null;
				cloneOfExisting.MocFavourites = null;
				cloneOfExisting.MocLikes = null;
				cloneOfExisting.PublishedMocChangeHistories = null;
				cloneOfExisting.PublishedMocImages = null;
				cloneOfExisting.SharedInstructions = null;
				cloneOfExisting.project = null;

				if (versionNumber >= publishedMoc.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for BMC.PublishedMoc rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for BMC.PublishedMoc rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				PublishedMocChangeHistory publishedMocChangeHistory = (from x in _context.PublishedMocChangeHistories
				                                               where
				                                               x.publishedMocId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (publishedMocChangeHistory != null)
				{
				    Database.PublishedMoc oldPublishedMoc = JsonSerializer.Deserialize<Database.PublishedMoc>(publishedMocChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    publishedMoc.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    publishedMoc.projectId = oldPublishedMoc.projectId;
				    publishedMoc.name = oldPublishedMoc.name;
				    publishedMoc.description = oldPublishedMoc.description;
				    publishedMoc.thumbnailImagePath = oldPublishedMoc.thumbnailImagePath;
				    publishedMoc.tags = oldPublishedMoc.tags;
				    publishedMoc.isPublished = oldPublishedMoc.isPublished;
				    publishedMoc.isFeatured = oldPublishedMoc.isFeatured;
				    publishedMoc.publishedDate = oldPublishedMoc.publishedDate;
				    publishedMoc.viewCount = oldPublishedMoc.viewCount;
				    publishedMoc.likeCount = oldPublishedMoc.likeCount;
				    publishedMoc.commentCount = oldPublishedMoc.commentCount;
				    publishedMoc.favouriteCount = oldPublishedMoc.favouriteCount;
				    publishedMoc.partCount = oldPublishedMoc.partCount;
				    publishedMoc.allowForking = oldPublishedMoc.allowForking;
				    publishedMoc.objectGuid = oldPublishedMoc.objectGuid;
				    publishedMoc.active = oldPublishedMoc.active;
				    publishedMoc.deleted = oldPublishedMoc.deleted;

				    string serializedPublishedMoc = JsonSerializer.Serialize(publishedMoc);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        PublishedMocChangeHistory newPublishedMocChangeHistory = new PublishedMocChangeHistory();
				        newPublishedMocChangeHistory.publishedMocId = publishedMoc.id;
				        newPublishedMocChangeHistory.versionNumber = publishedMoc.versionNumber;
				        newPublishedMocChangeHistory.timeStamp = DateTime.UtcNow;
				        newPublishedMocChangeHistory.userId = securityUser.id;
				        newPublishedMocChangeHistory.tenantGuid = userTenantGuid;
				        newPublishedMocChangeHistory.data = JsonSerializer.Serialize(Database.PublishedMoc.CreateAnonymousWithFirstLevelSubObjects(publishedMoc));
				        _context.PublishedMocChangeHistories.Add(newPublishedMocChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.PublishedMoc rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.PublishedMoc.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.PublishedMoc.CreateAnonymousWithFirstLevelSubObjects(publishedMoc)),
						null);


				    return Ok(Database.PublishedMoc.CreateAnonymous(publishedMoc));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for BMC.PublishedMoc rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for BMC.PublishedMoc rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a PublishedMoc.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the PublishedMoc</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PublishedMoc/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetPublishedMocChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.PublishedMoc publishedMoc = await _context.PublishedMocs.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (publishedMoc == null)
			{
				return NotFound();
			}

			try
			{
				publishedMoc.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.PublishedMoc> versionInfo = await publishedMoc.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a PublishedMoc.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the PublishedMoc</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PublishedMoc/{id}/AuditHistory")]
		public async Task<IActionResult> GetPublishedMocAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.PublishedMoc publishedMoc = await _context.PublishedMocs.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (publishedMoc == null)
			{
				return NotFound();
			}

			try
			{
				publishedMoc.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.PublishedMoc>> versions = await publishedMoc.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a PublishedMoc.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the PublishedMoc</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The PublishedMoc object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PublishedMoc/{id}/Version/{version}")]
		public async Task<IActionResult> GetPublishedMocVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.PublishedMoc publishedMoc = await _context.PublishedMocs.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (publishedMoc == null)
			{
				return NotFound();
			}

			try
			{
				publishedMoc.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.PublishedMoc> versionInfo = await publishedMoc.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a PublishedMoc at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the PublishedMoc</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The PublishedMoc object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PublishedMoc/{id}/StateAtTime")]
		public async Task<IActionResult> GetPublishedMocStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.PublishedMoc publishedMoc = await _context.PublishedMocs.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (publishedMoc == null)
			{
				return NotFound();
			}

			try
			{
				publishedMoc.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.PublishedMoc> versionInfo = await publishedMoc.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a PublishedMoc record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PublishedMoc/{id}")]
		[Route("api/PublishedMoc")]
		public async Task<IActionResult> DeletePublishedMoc(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.PublishedMoc> query = (from x in _context.PublishedMocs
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.PublishedMoc publishedMoc = await query.FirstOrDefaultAsync(cancellationToken);

			if (publishedMoc == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.PublishedMoc DELETE", id.ToString(), new Exception("No BMC.PublishedMoc entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.PublishedMoc cloneOfExisting = (Database.PublishedMoc)_context.Entry(publishedMoc).GetDatabaseValues().ToObject();


			lock (publishedMocDeleteSyncRoot)
			{
			    try
			    {
			        publishedMoc.deleted = true;
			        publishedMoc.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        PublishedMocChangeHistory publishedMocChangeHistory = new PublishedMocChangeHistory();
			        publishedMocChangeHistory.publishedMocId = publishedMoc.id;
			        publishedMocChangeHistory.versionNumber = publishedMoc.versionNumber;
			        publishedMocChangeHistory.timeStamp = DateTime.UtcNow;
			        publishedMocChangeHistory.userId = securityUser.id;
			        publishedMocChangeHistory.tenantGuid = userTenantGuid;
			        publishedMocChangeHistory.data = JsonSerializer.Serialize(Database.PublishedMoc.CreateAnonymousWithFirstLevelSubObjects(publishedMoc));
			        _context.PublishedMocChangeHistories.Add(publishedMocChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.PublishedMoc entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.PublishedMoc.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.PublishedMoc.CreateAnonymousWithFirstLevelSubObjects(publishedMoc)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.PublishedMoc entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.PublishedMoc.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.PublishedMoc.CreateAnonymousWithFirstLevelSubObjects(publishedMoc)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of PublishedMoc records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/PublishedMocs/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? projectId = null,
			string name = null,
			string description = null,
			string thumbnailImagePath = null,
			string tags = null,
			bool? isPublished = null,
			bool? isFeatured = null,
			DateTime? publishedDate = null,
			int? viewCount = null,
			int? likeCount = null,
			int? commentCount = null,
			int? favouriteCount = null,
			int? partCount = null,
			bool? allowForking = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);


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

			IQueryable<Database.PublishedMoc> query = (from pm in _context.PublishedMocs select pm);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (projectId.HasValue == true)
			{
				query = query.Where(pm => pm.projectId == projectId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(pm => pm.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(pm => pm.description == description);
			}
			if (string.IsNullOrEmpty(thumbnailImagePath) == false)
			{
				query = query.Where(pm => pm.thumbnailImagePath == thumbnailImagePath);
			}
			if (string.IsNullOrEmpty(tags) == false)
			{
				query = query.Where(pm => pm.tags == tags);
			}
			if (isPublished.HasValue == true)
			{
				query = query.Where(pm => pm.isPublished == isPublished.Value);
			}
			if (isFeatured.HasValue == true)
			{
				query = query.Where(pm => pm.isFeatured == isFeatured.Value);
			}
			if (publishedDate.HasValue == true)
			{
				query = query.Where(pm => pm.publishedDate == publishedDate.Value);
			}
			if (viewCount.HasValue == true)
			{
				query = query.Where(pm => pm.viewCount == viewCount.Value);
			}
			if (likeCount.HasValue == true)
			{
				query = query.Where(pm => pm.likeCount == likeCount.Value);
			}
			if (commentCount.HasValue == true)
			{
				query = query.Where(pm => pm.commentCount == commentCount.Value);
			}
			if (favouriteCount.HasValue == true)
			{
				query = query.Where(pm => pm.favouriteCount == favouriteCount.Value);
			}
			if (partCount.HasValue == true)
			{
				query = query.Where(pm => pm.partCount == partCount.Value);
			}
			if (allowForking.HasValue == true)
			{
				query = query.Where(pm => pm.allowForking == allowForking.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(pm => pm.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pm => pm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pm => pm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pm => pm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pm => pm.deleted == false);
				}
			}
			else
			{
				query = query.Where(pm => pm.active == true);
				query = query.Where(pm => pm.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Published Moc, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.thumbnailImagePath.Contains(anyStringContains)
			       || x.tags.Contains(anyStringContains)
			       || x.project.name.Contains(anyStringContains)
			       || x.project.description.Contains(anyStringContains)
			       || x.project.notes.Contains(anyStringContains)
			       || x.project.thumbnailImagePath.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name).ThenBy(x => x.thumbnailImagePath);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.PublishedMoc.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/PublishedMoc/CreateAuditEvent")]
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
