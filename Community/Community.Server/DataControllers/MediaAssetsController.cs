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
    /// This auto generated class provides the basic CRUD operations for the MediaAsset entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the MediaAsset entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class MediaAssetsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 10;

		private CommunityContext _context;

		private ILogger<MediaAssetsController> _logger;

		public MediaAssetsController(CommunityContext context, ILogger<MediaAssetsController> logger) : base("Community", "MediaAsset")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of MediaAssets filtered by the parameters provided.
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
		[Route("api/MediaAssets")]
		public async Task<IActionResult> GetMediaAssets(
			int? Id = null,
			string FileName = null,
			string FilePath = null,
			string MimeType = null,
			string AltText = null,
			string Caption = null,
			long? FileSizeBytes = null,
			int? ImageWidth = null,
			int? ImageHeight = null,
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

			IQueryable<Database.MediaAsset> query = (from ma in _context.MediaAssets select ma);
			if (Id.HasValue == true)
			{
				query = query.Where(ma => ma.Id == Id.Value);
			}
			if (string.IsNullOrEmpty(FileName) == false)
			{
				query = query.Where(ma => ma.FileName == FileName);
			}
			if (string.IsNullOrEmpty(FilePath) == false)
			{
				query = query.Where(ma => ma.FilePath == FilePath);
			}
			if (string.IsNullOrEmpty(MimeType) == false)
			{
				query = query.Where(ma => ma.MimeType == MimeType);
			}
			if (string.IsNullOrEmpty(AltText) == false)
			{
				query = query.Where(ma => ma.AltText == AltText);
			}
			if (string.IsNullOrEmpty(Caption) == false)
			{
				query = query.Where(ma => ma.Caption == Caption);
			}
			if (FileSizeBytes.HasValue == true)
			{
				query = query.Where(ma => ma.FileSizeBytes == FileSizeBytes.Value);
			}
			if (ImageWidth.HasValue == true)
			{
				query = query.Where(ma => ma.ImageWidth == ImageWidth.Value);
			}
			if (ImageHeight.HasValue == true)
			{
				query = query.Where(ma => ma.ImageHeight == ImageHeight.Value);
			}
			if (ObjectGuid.HasValue == true)
			{
				query = query.Where(ma => ma.ObjectGuid == ObjectGuid);
			}
			if (Active.HasValue == true)
			{
				query = query.Where(ma => ma.Active == Active.Value);
			}
			if (Deleted.HasValue == true)
			{
				query = query.Where(ma => ma.Deleted == Deleted.Value);
			}

			query = query.OrderBy(ma => ma.fileName).ThenBy(ma => ma.filePath).ThenBy(ma => ma.mimeType);


			//
			// Add the any string contains parameter to span all the string fields on the Media Asset, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.FileName.Contains(anyStringContains)
			       || x.FilePath.Contains(anyStringContains)
			       || x.MimeType.Contains(anyStringContains)
			       || x.AltText.Contains(anyStringContains)
			       || x.Caption.Contains(anyStringContains)
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
			
			List<Database.MediaAsset> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.MediaAsset mediaAsset in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(mediaAsset, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Community.MediaAsset Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Community.MediaAsset Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of MediaAssets filtered by the parameters provided.  Its query is similar to the GetMediaAssets method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MediaAssets/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? Id = null,
			string FileName = null,
			string FilePath = null,
			string MimeType = null,
			string AltText = null,
			string Caption = null,
			long? FileSizeBytes = null,
			int? ImageWidth = null,
			int? ImageHeight = null,
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

			IQueryable<Database.MediaAsset> query = (from ma in _context.MediaAssets select ma);
			if (Id.HasValue == true)
			{
				query = query.Where(ma => ma.Id == Id.Value);
			}
			if (FileName != null)
			{
				query = query.Where(ma => ma.FileName == FileName);
			}
			if (FilePath != null)
			{
				query = query.Where(ma => ma.FilePath == FilePath);
			}
			if (MimeType != null)
			{
				query = query.Where(ma => ma.MimeType == MimeType);
			}
			if (AltText != null)
			{
				query = query.Where(ma => ma.AltText == AltText);
			}
			if (Caption != null)
			{
				query = query.Where(ma => ma.Caption == Caption);
			}
			if (FileSizeBytes.HasValue == true)
			{
				query = query.Where(ma => ma.FileSizeBytes == FileSizeBytes.Value);
			}
			if (ImageWidth.HasValue == true)
			{
				query = query.Where(ma => ma.ImageWidth == ImageWidth.Value);
			}
			if (ImageHeight.HasValue == true)
			{
				query = query.Where(ma => ma.ImageHeight == ImageHeight.Value);
			}
			if (ObjectGuid.HasValue == true)
			{
				query = query.Where(ma => ma.ObjectGuid == ObjectGuid);
			}
			if (Active.HasValue == true)
			{
				query = query.Where(ma => ma.Active == Active.Value);
			}
			if (Deleted.HasValue == true)
			{
				query = query.Where(ma => ma.Deleted == Deleted.Value);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Media Asset, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.FileName.Contains(anyStringContains)
			       || x.FilePath.Contains(anyStringContains)
			       || x.MimeType.Contains(anyStringContains)
			       || x.AltText.Contains(anyStringContains)
			       || x.Caption.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single MediaAsset by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MediaAsset/{id}")]
		public async Task<IActionResult> GetMediaAsset(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.MediaAsset> query = (from ma in _context.MediaAssets where
				(ma.id == id)
					select ma);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.MediaAsset materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Community.MediaAsset Entity was read with Admin privilege." : "Community.MediaAsset Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "MediaAsset", materialized.id, materialized.fileName));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Community.MediaAsset entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Community.MediaAsset.   Entity was read with Admin privilege." : "Exception caught during entity read of Community.MediaAsset.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing MediaAsset record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/MediaAsset/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutMediaAsset(int id, [FromBody]Database.MediaAsset.MediaAssetDTO mediaAssetDTO, CancellationToken cancellationToken = default)
		{
			if (mediaAssetDTO == null)
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



			if (id != mediaAssetDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.MediaAsset> query = (from x in _context.MediaAssets
				where
				(x.id == id)
				select x);


			Database.MediaAsset existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.MediaAsset PUT", id.ToString(), new Exception("No Community.MediaAsset entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (mediaAssetDTO.objectGuid == Guid.Empty)
            {
                mediaAssetDTO.objectGuid = existing.objectGuid;
            }
            else if (mediaAssetDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a MediaAsset record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.MediaAsset cloneOfExisting = (Database.MediaAsset)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new MediaAsset object using the data from the existing record, updated with what is in the DTO.
			//
			Database.MediaAsset mediaAsset = (Database.MediaAsset)_context.Entry(existing).GetDatabaseValues().ToObject();
			mediaAsset.ApplyDTO(mediaAssetDTO);


			EntityEntry<Database.MediaAsset> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(mediaAsset);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Community.MediaAsset entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.MediaAsset.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MediaAsset.CreateAnonymousWithFirstLevelSubObjects(mediaAsset)),
					null);


				return Ok(Database.MediaAsset.CreateAnonymous(mediaAsset));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Community.MediaAsset entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.MediaAsset.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MediaAsset.CreateAnonymousWithFirstLevelSubObjects(mediaAsset)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new MediaAsset record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MediaAsset", Name = "MediaAsset")]
		public async Task<IActionResult> PostMediaAsset([FromBody]Database.MediaAsset.MediaAssetDTO mediaAssetDTO, CancellationToken cancellationToken = default)
		{
			if (mediaAssetDTO == null)
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
			// Create a new MediaAsset object using the data from the DTO
			//
			Database.MediaAsset mediaAsset = Database.MediaAsset.FromDTO(mediaAssetDTO);

			try
			{
				_context.MediaAssets.Add(mediaAsset);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Community.MediaAsset entity successfully created.",
					true,
					mediaAsset.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.MediaAsset.CreateAnonymousWithFirstLevelSubObjects(mediaAsset)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Community.MediaAsset entity creation failed.", false, mediaAsset.id.ToString(), "", JsonSerializer.Serialize(mediaAsset), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "MediaAsset", mediaAsset.id, mediaAsset.fileName));

			return CreatedAtRoute("MediaAsset", new { id = mediaAsset.id }, Database.MediaAsset.CreateAnonymousWithFirstLevelSubObjects(mediaAsset));
		}



        /// <summary>
        /// 
        /// This deletes a MediaAsset record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MediaAsset/{id}")]
		[Route("api/MediaAsset")]
		public async Task<IActionResult> DeleteMediaAsset(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.MediaAsset> query = (from x in _context.MediaAssets
				where
				(x.id == id)
				select x);


			Database.MediaAsset mediaAsset = await query.FirstOrDefaultAsync(cancellationToken);

			if (mediaAsset == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.MediaAsset DELETE", id.ToString(), new Exception("No Community.MediaAsset entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.MediaAsset cloneOfExisting = (Database.MediaAsset)_context.Entry(mediaAsset).GetDatabaseValues().ToObject();


			try
			{
				_context.MediaAssets.Remove(mediaAsset);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Community.MediaAsset entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.MediaAsset.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MediaAsset.CreateAnonymousWithFirstLevelSubObjects(mediaAsset)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Community.MediaAsset entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.MediaAsset.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MediaAsset.CreateAnonymousWithFirstLevelSubObjects(mediaAsset)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of MediaAsset records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/MediaAssets/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? Id = null,
			string FileName = null,
			string FilePath = null,
			string MimeType = null,
			string AltText = null,
			string Caption = null,
			long? FileSizeBytes = null,
			int? ImageWidth = null,
			int? ImageHeight = null,
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

			IQueryable<Database.MediaAsset> query = (from ma in _context.MediaAssets select ma);
			if (Id.HasValue == true)
			{
				query = query.Where(ma => ma.Id == Id.Value);
			}
			if (string.IsNullOrEmpty(FileName) == false)
			{
				query = query.Where(ma => ma.FileName == FileName);
			}
			if (string.IsNullOrEmpty(FilePath) == false)
			{
				query = query.Where(ma => ma.FilePath == FilePath);
			}
			if (string.IsNullOrEmpty(MimeType) == false)
			{
				query = query.Where(ma => ma.MimeType == MimeType);
			}
			if (string.IsNullOrEmpty(AltText) == false)
			{
				query = query.Where(ma => ma.AltText == AltText);
			}
			if (string.IsNullOrEmpty(Caption) == false)
			{
				query = query.Where(ma => ma.Caption == Caption);
			}
			if (FileSizeBytes.HasValue == true)
			{
				query = query.Where(ma => ma.FileSizeBytes == FileSizeBytes.Value);
			}
			if (ImageWidth.HasValue == true)
			{
				query = query.Where(ma => ma.ImageWidth == ImageWidth.Value);
			}
			if (ImageHeight.HasValue == true)
			{
				query = query.Where(ma => ma.ImageHeight == ImageHeight.Value);
			}
			if (ObjectGuid.HasValue == true)
			{
				query = query.Where(ma => ma.ObjectGuid == ObjectGuid);
			}
			if (Active.HasValue == true)
			{
				query = query.Where(ma => ma.Active == Active.Value);
			}
			if (Deleted.HasValue == true)
			{
				query = query.Where(ma => ma.Deleted == Deleted.Value);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Media Asset, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.FileName.Contains(anyStringContains)
			       || x.FilePath.Contains(anyStringContains)
			       || x.MimeType.Contains(anyStringContains)
			       || x.AltText.Contains(anyStringContains)
			       || x.Caption.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.fileName).ThenBy(x => x.filePath).ThenBy(x => x.mimeType);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.MediaAsset.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/MediaAsset/CreateAuditEvent")]
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
