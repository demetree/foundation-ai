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
    /// This auto generated class provides the basic CRUD operations for the GalleryImage entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the GalleryImage entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class GalleryImagesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 10;

		private CommunityContext _context;

		private ILogger<GalleryImagesController> _logger;

		public GalleryImagesController(CommunityContext context, ILogger<GalleryImagesController> logger) : base("Community", "GalleryImage")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of GalleryImages filtered by the parameters provided.
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
		[Route("api/GalleryImages")]
		public async Task<IActionResult> GetGalleryImages(
			int? Id = null,
			int? GalleryAlbumId = null,
			string ImageUrl = null,
			string Caption = null,
			string AltText = null,
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

			IQueryable<Database.GalleryImage> query = (from gi in _context.GalleryImages select gi);
			if (Id.HasValue == true)
			{
				query = query.Where(gi => gi.Id == Id.Value);
			}
			if (GalleryAlbumId.HasValue == true)
			{
				query = query.Where(gi => gi.GalleryAlbumId == GalleryAlbumId.Value);
			}
			if (string.IsNullOrEmpty(ImageUrl) == false)
			{
				query = query.Where(gi => gi.ImageUrl == ImageUrl);
			}
			if (string.IsNullOrEmpty(Caption) == false)
			{
				query = query.Where(gi => gi.Caption == Caption);
			}
			if (string.IsNullOrEmpty(AltText) == false)
			{
				query = query.Where(gi => gi.AltText == AltText);
			}
			if (Sequence.HasValue == true)
			{
				query = query.Where(gi => gi.Sequence == Sequence.Value);
			}
			if (ObjectGuid.HasValue == true)
			{
				query = query.Where(gi => gi.ObjectGuid == ObjectGuid);
			}
			if (Active.HasValue == true)
			{
				query = query.Where(gi => gi.Active == Active.Value);
			}
			if (Deleted.HasValue == true)
			{
				query = query.Where(gi => gi.Deleted == Deleted.Value);
			}

			query = query.OrderBy(gi => gi.sequence).ThenBy(gi => gi.imageUrl).ThenBy(gi => gi.caption).ThenBy(gi => gi.altText);


			//
			// Add the any string contains parameter to span all the string fields on the Gallery Image, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.ImageUrl.Contains(anyStringContains)
			       || x.Caption.Contains(anyStringContains)
			       || x.AltText.Contains(anyStringContains)
			       || (includeRelations == true && x.GalleryAlbum.Title.Contains(anyStringContains))
			       || (includeRelations == true && x.GalleryAlbum.Slug.Contains(anyStringContains))
			       || (includeRelations == true && x.GalleryAlbum.Description.Contains(anyStringContains))
			       || (includeRelations == true && x.GalleryAlbum.CoverImageUrl.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.GalleryAlbum);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.GalleryImage> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.GalleryImage galleryImage in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(galleryImage, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Community.GalleryImage Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Community.GalleryImage Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of GalleryImages filtered by the parameters provided.  Its query is similar to the GetGalleryImages method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/GalleryImages/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? Id = null,
			int? GalleryAlbumId = null,
			string ImageUrl = null,
			string Caption = null,
			string AltText = null,
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

			IQueryable<Database.GalleryImage> query = (from gi in _context.GalleryImages select gi);
			if (Id.HasValue == true)
			{
				query = query.Where(gi => gi.Id == Id.Value);
			}
			if (GalleryAlbumId.HasValue == true)
			{
				query = query.Where(gi => gi.GalleryAlbumId == GalleryAlbumId.Value);
			}
			if (ImageUrl != null)
			{
				query = query.Where(gi => gi.ImageUrl == ImageUrl);
			}
			if (Caption != null)
			{
				query = query.Where(gi => gi.Caption == Caption);
			}
			if (AltText != null)
			{
				query = query.Where(gi => gi.AltText == AltText);
			}
			if (Sequence.HasValue == true)
			{
				query = query.Where(gi => gi.Sequence == Sequence.Value);
			}
			if (ObjectGuid.HasValue == true)
			{
				query = query.Where(gi => gi.ObjectGuid == ObjectGuid);
			}
			if (Active.HasValue == true)
			{
				query = query.Where(gi => gi.Active == Active.Value);
			}
			if (Deleted.HasValue == true)
			{
				query = query.Where(gi => gi.Deleted == Deleted.Value);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Gallery Image, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.ImageUrl.Contains(anyStringContains)
			       || x.Caption.Contains(anyStringContains)
			       || x.AltText.Contains(anyStringContains)
			       || x.GalleryAlbum.Title.Contains(anyStringContains)
			       || x.GalleryAlbum.Slug.Contains(anyStringContains)
			       || x.GalleryAlbum.Description.Contains(anyStringContains)
			       || x.GalleryAlbum.CoverImageUrl.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single GalleryImage by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/GalleryImage/{id}")]
		public async Task<IActionResult> GetGalleryImage(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.GalleryImage> query = (from gi in _context.GalleryImages where
				(gi.id == id)
					select gi);

				if (includeRelations == true)
				{
					query = query.Include(x => x.GalleryAlbum);
					query = query.AsSplitQuery();
				}

				Database.GalleryImage materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Community.GalleryImage Entity was read with Admin privilege." : "Community.GalleryImage Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "GalleryImage", materialized.id, materialized.imageUrl));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Community.GalleryImage entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Community.GalleryImage.   Entity was read with Admin privilege." : "Exception caught during entity read of Community.GalleryImage.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing GalleryImage record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/GalleryImage/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutGalleryImage(int id, [FromBody]Database.GalleryImage.GalleryImageDTO galleryImageDTO, CancellationToken cancellationToken = default)
		{
			if (galleryImageDTO == null)
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



			if (id != galleryImageDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.GalleryImage> query = (from x in _context.GalleryImages
				where
				(x.id == id)
				select x);


			Database.GalleryImage existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.GalleryImage PUT", id.ToString(), new Exception("No Community.GalleryImage entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (galleryImageDTO.objectGuid == Guid.Empty)
            {
                galleryImageDTO.objectGuid = existing.objectGuid;
            }
            else if (galleryImageDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a GalleryImage record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.GalleryImage cloneOfExisting = (Database.GalleryImage)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new GalleryImage object using the data from the existing record, updated with what is in the DTO.
			//
			Database.GalleryImage galleryImage = (Database.GalleryImage)_context.Entry(existing).GetDatabaseValues().ToObject();
			galleryImage.ApplyDTO(galleryImageDTO);


			EntityEntry<Database.GalleryImage> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(galleryImage);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Community.GalleryImage entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.GalleryImage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.GalleryImage.CreateAnonymousWithFirstLevelSubObjects(galleryImage)),
					null);


				return Ok(Database.GalleryImage.CreateAnonymous(galleryImage));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Community.GalleryImage entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.GalleryImage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.GalleryImage.CreateAnonymousWithFirstLevelSubObjects(galleryImage)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new GalleryImage record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/GalleryImage", Name = "GalleryImage")]
		public async Task<IActionResult> PostGalleryImage([FromBody]Database.GalleryImage.GalleryImageDTO galleryImageDTO, CancellationToken cancellationToken = default)
		{
			if (galleryImageDTO == null)
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
			// Create a new GalleryImage object using the data from the DTO
			//
			Database.GalleryImage galleryImage = Database.GalleryImage.FromDTO(galleryImageDTO);

			try
			{
				_context.GalleryImages.Add(galleryImage);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Community.GalleryImage entity successfully created.",
					true,
					galleryImage.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.GalleryImage.CreateAnonymousWithFirstLevelSubObjects(galleryImage)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Community.GalleryImage entity creation failed.", false, galleryImage.id.ToString(), "", JsonSerializer.Serialize(galleryImage), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "GalleryImage", galleryImage.id, galleryImage.imageUrl));

			return CreatedAtRoute("GalleryImage", new { id = galleryImage.id }, Database.GalleryImage.CreateAnonymousWithFirstLevelSubObjects(galleryImage));
		}



        /// <summary>
        /// 
        /// This deletes a GalleryImage record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/GalleryImage/{id}")]
		[Route("api/GalleryImage")]
		public async Task<IActionResult> DeleteGalleryImage(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.GalleryImage> query = (from x in _context.GalleryImages
				where
				(x.id == id)
				select x);


			Database.GalleryImage galleryImage = await query.FirstOrDefaultAsync(cancellationToken);

			if (galleryImage == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.GalleryImage DELETE", id.ToString(), new Exception("No Community.GalleryImage entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.GalleryImage cloneOfExisting = (Database.GalleryImage)_context.Entry(galleryImage).GetDatabaseValues().ToObject();


			try
			{
				_context.GalleryImages.Remove(galleryImage);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Community.GalleryImage entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.GalleryImage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.GalleryImage.CreateAnonymousWithFirstLevelSubObjects(galleryImage)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Community.GalleryImage entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.GalleryImage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.GalleryImage.CreateAnonymousWithFirstLevelSubObjects(galleryImage)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of GalleryImage records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/GalleryImages/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? Id = null,
			int? GalleryAlbumId = null,
			string ImageUrl = null,
			string Caption = null,
			string AltText = null,
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

			IQueryable<Database.GalleryImage> query = (from gi in _context.GalleryImages select gi);
			if (Id.HasValue == true)
			{
				query = query.Where(gi => gi.Id == Id.Value);
			}
			if (GalleryAlbumId.HasValue == true)
			{
				query = query.Where(gi => gi.GalleryAlbumId == GalleryAlbumId.Value);
			}
			if (string.IsNullOrEmpty(ImageUrl) == false)
			{
				query = query.Where(gi => gi.ImageUrl == ImageUrl);
			}
			if (string.IsNullOrEmpty(Caption) == false)
			{
				query = query.Where(gi => gi.Caption == Caption);
			}
			if (string.IsNullOrEmpty(AltText) == false)
			{
				query = query.Where(gi => gi.AltText == AltText);
			}
			if (Sequence.HasValue == true)
			{
				query = query.Where(gi => gi.Sequence == Sequence.Value);
			}
			if (ObjectGuid.HasValue == true)
			{
				query = query.Where(gi => gi.ObjectGuid == ObjectGuid);
			}
			if (Active.HasValue == true)
			{
				query = query.Where(gi => gi.Active == Active.Value);
			}
			if (Deleted.HasValue == true)
			{
				query = query.Where(gi => gi.Deleted == Deleted.Value);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Gallery Image, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.ImageUrl.Contains(anyStringContains)
			       || x.Caption.Contains(anyStringContains)
			       || x.AltText.Contains(anyStringContains)
			       || x.GalleryAlbum.Title.Contains(anyStringContains)
			       || x.GalleryAlbum.Slug.Contains(anyStringContains)
			       || x.GalleryAlbum.Description.Contains(anyStringContains)
			       || x.GalleryAlbum.CoverImageUrl.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.imageUrl).ThenBy(x => x.caption).ThenBy(x => x.altText);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.GalleryImage.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/GalleryImage/CreateAuditEvent")]
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
