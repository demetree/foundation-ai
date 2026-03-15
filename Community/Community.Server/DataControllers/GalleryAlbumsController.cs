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
    /// This auto generated class provides the basic CRUD operations for the GalleryAlbum entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the GalleryAlbum entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class GalleryAlbumsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 10;

		private CommunityContext _context;

		private ILogger<GalleryAlbumsController> _logger;

		public GalleryAlbumsController(CommunityContext context, ILogger<GalleryAlbumsController> logger) : base("Community", "GalleryAlbum")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of GalleryAlbums filtered by the parameters provided.
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
		[Route("api/GalleryAlbums")]
		public async Task<IActionResult> GetGalleryAlbums(
			int? Id = null,
			string Title = null,
			string Slug = null,
			string Description = null,
			string CoverImageUrl = null,
			bool? IsPublished = null,
			int? Sequence = null,
			Guid? ObjectGuid = null,
			bool? Active = null,
			bool? Deleted = null,
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

			IQueryable<Database.GalleryAlbum> query = (from ga in _context.GalleryAlbums select ga);
			if (Id.HasValue == true)
			{
				query = query.Where(ga => ga.Id == Id.Value);
			}
			if (string.IsNullOrEmpty(Title) == false)
			{
				query = query.Where(ga => ga.Title == Title);
			}
			if (string.IsNullOrEmpty(Slug) == false)
			{
				query = query.Where(ga => ga.Slug == Slug);
			}
			if (string.IsNullOrEmpty(Description) == false)
			{
				query = query.Where(ga => ga.Description == Description);
			}
			if (string.IsNullOrEmpty(CoverImageUrl) == false)
			{
				query = query.Where(ga => ga.CoverImageUrl == CoverImageUrl);
			}
			if (IsPublished.HasValue == true)
			{
				query = query.Where(ga => ga.IsPublished == IsPublished.Value);
			}
			if (Sequence.HasValue == true)
			{
				query = query.Where(ga => ga.Sequence == Sequence.Value);
			}
			if (ObjectGuid.HasValue == true)
			{
				query = query.Where(ga => ga.ObjectGuid == ObjectGuid);
			}
			if (Active.HasValue == true)
			{
				query = query.Where(ga => ga.Active == Active.Value);
			}
			if (Deleted.HasValue == true)
			{
				query = query.Where(ga => ga.Deleted == Deleted.Value);
			}

			query = query.OrderBy(ga => ga.sequence).ThenBy(ga => ga.title).ThenBy(ga => ga.slug).ThenBy(ga => ga.coverImageUrl);


			//
			// Add the any string contains parameter to span all the string fields on the Gallery Album, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.Title.Contains(anyStringContains)
			       || x.Slug.Contains(anyStringContains)
			       || x.Description.Contains(anyStringContains)
			       || x.CoverImageUrl.Contains(anyStringContains)
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
			
			List<Database.GalleryAlbum> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.GalleryAlbum galleryAlbum in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(galleryAlbum, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Community.GalleryAlbum Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Community.GalleryAlbum Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of GalleryAlbums filtered by the parameters provided.  Its query is similar to the GetGalleryAlbums method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/GalleryAlbums/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? Id = null,
			string Title = null,
			string Slug = null,
			string Description = null,
			string CoverImageUrl = null,
			bool? IsPublished = null,
			int? Sequence = null,
			Guid? ObjectGuid = null,
			bool? Active = null,
			bool? Deleted = null,
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

			IQueryable<Database.GalleryAlbum> query = (from ga in _context.GalleryAlbums select ga);
			if (Id.HasValue == true)
			{
				query = query.Where(ga => ga.Id == Id.Value);
			}
			if (Title != null)
			{
				query = query.Where(ga => ga.Title == Title);
			}
			if (Slug != null)
			{
				query = query.Where(ga => ga.Slug == Slug);
			}
			if (Description != null)
			{
				query = query.Where(ga => ga.Description == Description);
			}
			if (CoverImageUrl != null)
			{
				query = query.Where(ga => ga.CoverImageUrl == CoverImageUrl);
			}
			if (IsPublished.HasValue == true)
			{
				query = query.Where(ga => ga.IsPublished == IsPublished.Value);
			}
			if (Sequence.HasValue == true)
			{
				query = query.Where(ga => ga.Sequence == Sequence.Value);
			}
			if (ObjectGuid.HasValue == true)
			{
				query = query.Where(ga => ga.ObjectGuid == ObjectGuid);
			}
			if (Active.HasValue == true)
			{
				query = query.Where(ga => ga.Active == Active.Value);
			}
			if (Deleted.HasValue == true)
			{
				query = query.Where(ga => ga.Deleted == Deleted.Value);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Gallery Album, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.Title.Contains(anyStringContains)
			       || x.Slug.Contains(anyStringContains)
			       || x.Description.Contains(anyStringContains)
			       || x.CoverImageUrl.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single GalleryAlbum by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/GalleryAlbum/{id}")]
		public async Task<IActionResult> GetGalleryAlbum(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.GalleryAlbum> query = (from ga in _context.GalleryAlbums where
				(ga.id == id)
					select ga);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.GalleryAlbum materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Community.GalleryAlbum Entity was read with Admin privilege." : "Community.GalleryAlbum Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "GalleryAlbum", materialized.id, materialized.title));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Community.GalleryAlbum entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Community.GalleryAlbum.   Entity was read with Admin privilege." : "Exception caught during entity read of Community.GalleryAlbum.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing GalleryAlbum record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/GalleryAlbum/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutGalleryAlbum(int id, [FromBody]Database.GalleryAlbum.GalleryAlbumDTO galleryAlbumDTO, CancellationToken cancellationToken = default)
		{
			if (galleryAlbumDTO == null)
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



			if (id != galleryAlbumDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.GalleryAlbum> query = (from x in _context.GalleryAlbums
				where
				(x.id == id)
				select x);


			Database.GalleryAlbum existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.GalleryAlbum PUT", id.ToString(), new Exception("No Community.GalleryAlbum entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (galleryAlbumDTO.objectGuid == Guid.Empty)
            {
                galleryAlbumDTO.objectGuid = existing.objectGuid;
            }
            else if (galleryAlbumDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a GalleryAlbum record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.GalleryAlbum cloneOfExisting = (Database.GalleryAlbum)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new GalleryAlbum object using the data from the existing record, updated with what is in the DTO.
			//
			Database.GalleryAlbum galleryAlbum = (Database.GalleryAlbum)_context.Entry(existing).GetDatabaseValues().ToObject();
			galleryAlbum.ApplyDTO(galleryAlbumDTO);


			EntityEntry<Database.GalleryAlbum> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(galleryAlbum);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Community.GalleryAlbum entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.GalleryAlbum.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.GalleryAlbum.CreateAnonymousWithFirstLevelSubObjects(galleryAlbum)),
					null);


				return Ok(Database.GalleryAlbum.CreateAnonymous(galleryAlbum));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Community.GalleryAlbum entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.GalleryAlbum.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.GalleryAlbum.CreateAnonymousWithFirstLevelSubObjects(galleryAlbum)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new GalleryAlbum record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/GalleryAlbum", Name = "GalleryAlbum")]
		public async Task<IActionResult> PostGalleryAlbum([FromBody]Database.GalleryAlbum.GalleryAlbumDTO galleryAlbumDTO, CancellationToken cancellationToken = default)
		{
			if (galleryAlbumDTO == null)
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
			// Create a new GalleryAlbum object using the data from the DTO
			//
			Database.GalleryAlbum galleryAlbum = Database.GalleryAlbum.FromDTO(galleryAlbumDTO);

			try
			{
				_context.GalleryAlbums.Add(galleryAlbum);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Community.GalleryAlbum entity successfully created.",
					true,
					galleryAlbum.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.GalleryAlbum.CreateAnonymousWithFirstLevelSubObjects(galleryAlbum)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Community.GalleryAlbum entity creation failed.", false, galleryAlbum.id.ToString(), "", JsonSerializer.Serialize(galleryAlbum), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "GalleryAlbum", galleryAlbum.id, galleryAlbum.title));

			return CreatedAtRoute("GalleryAlbum", new { id = galleryAlbum.id }, Database.GalleryAlbum.CreateAnonymousWithFirstLevelSubObjects(galleryAlbum));
		}



        /// <summary>
        /// 
        /// This deletes a GalleryAlbum record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/GalleryAlbum/{id}")]
		[Route("api/GalleryAlbum")]
		public async Task<IActionResult> DeleteGalleryAlbum(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.GalleryAlbum> query = (from x in _context.GalleryAlbums
				where
				(x.id == id)
				select x);


			Database.GalleryAlbum galleryAlbum = await query.FirstOrDefaultAsync(cancellationToken);

			if (galleryAlbum == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.GalleryAlbum DELETE", id.ToString(), new Exception("No Community.GalleryAlbum entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.GalleryAlbum cloneOfExisting = (Database.GalleryAlbum)_context.Entry(galleryAlbum).GetDatabaseValues().ToObject();


			try
			{
				_context.GalleryAlbums.Remove(galleryAlbum);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Community.GalleryAlbum entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.GalleryAlbum.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.GalleryAlbum.CreateAnonymousWithFirstLevelSubObjects(galleryAlbum)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Community.GalleryAlbum entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.GalleryAlbum.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.GalleryAlbum.CreateAnonymousWithFirstLevelSubObjects(galleryAlbum)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of GalleryAlbum records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/GalleryAlbums/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? Id = null,
			string Title = null,
			string Slug = null,
			string Description = null,
			string CoverImageUrl = null,
			bool? IsPublished = null,
			int? Sequence = null,
			Guid? ObjectGuid = null,
			bool? Active = null,
			bool? Deleted = null,
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

			IQueryable<Database.GalleryAlbum> query = (from ga in _context.GalleryAlbums select ga);
			if (Id.HasValue == true)
			{
				query = query.Where(ga => ga.Id == Id.Value);
			}
			if (string.IsNullOrEmpty(Title) == false)
			{
				query = query.Where(ga => ga.Title == Title);
			}
			if (string.IsNullOrEmpty(Slug) == false)
			{
				query = query.Where(ga => ga.Slug == Slug);
			}
			if (string.IsNullOrEmpty(Description) == false)
			{
				query = query.Where(ga => ga.Description == Description);
			}
			if (string.IsNullOrEmpty(CoverImageUrl) == false)
			{
				query = query.Where(ga => ga.CoverImageUrl == CoverImageUrl);
			}
			if (IsPublished.HasValue == true)
			{
				query = query.Where(ga => ga.IsPublished == IsPublished.Value);
			}
			if (Sequence.HasValue == true)
			{
				query = query.Where(ga => ga.Sequence == Sequence.Value);
			}
			if (ObjectGuid.HasValue == true)
			{
				query = query.Where(ga => ga.ObjectGuid == ObjectGuid);
			}
			if (Active.HasValue == true)
			{
				query = query.Where(ga => ga.Active == Active.Value);
			}
			if (Deleted.HasValue == true)
			{
				query = query.Where(ga => ga.Deleted == Deleted.Value);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Gallery Album, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.Title.Contains(anyStringContains)
			       || x.Slug.Contains(anyStringContains)
			       || x.Description.Contains(anyStringContains)
			       || x.CoverImageUrl.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.title).ThenBy(x => x.slug).ThenBy(x => x.coverImageUrl);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.GalleryAlbum.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/GalleryAlbum/CreateAuditEvent")]
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
