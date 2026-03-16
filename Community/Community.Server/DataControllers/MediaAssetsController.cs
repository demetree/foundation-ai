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

			this._context.Database.SetCommandTimeout(60);

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
			string fileName = null,
			string filePath = null,
			string mimeType = null,
			string altText = null,
			string caption = null,
			long? fileSizeBytes = null,
			int? imageWidth = null,
			int? imageHeight = null,
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

			IQueryable<Database.MediaAsset> query = (from ma in _context.MediaAssets select ma);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(fileName) == false)
			{
				query = query.Where(ma => ma.fileName == fileName);
			}
			if (string.IsNullOrEmpty(filePath) == false)
			{
				query = query.Where(ma => ma.filePath == filePath);
			}
			if (string.IsNullOrEmpty(mimeType) == false)
			{
				query = query.Where(ma => ma.mimeType == mimeType);
			}
			if (string.IsNullOrEmpty(altText) == false)
			{
				query = query.Where(ma => ma.altText == altText);
			}
			if (string.IsNullOrEmpty(caption) == false)
			{
				query = query.Where(ma => ma.caption == caption);
			}
			if (fileSizeBytes.HasValue == true)
			{
				query = query.Where(ma => ma.fileSizeBytes == fileSizeBytes.Value);
			}
			if (imageWidth.HasValue == true)
			{
				query = query.Where(ma => ma.imageWidth == imageWidth.Value);
			}
			if (imageHeight.HasValue == true)
			{
				query = query.Where(ma => ma.imageHeight == imageHeight.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ma => ma.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ma => ma.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ma => ma.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ma => ma.deleted == false);
				}
			}
			else
			{
				query = query.Where(ma => ma.active == true);
				query = query.Where(ma => ma.deleted == false);
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
			       x.fileName.Contains(anyStringContains)
			       || x.filePath.Contains(anyStringContains)
			       || x.mimeType.Contains(anyStringContains)
			       || x.altText.Contains(anyStringContains)
			       || x.caption.Contains(anyStringContains)
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
			string fileName = null,
			string filePath = null,
			string mimeType = null,
			string altText = null,
			string caption = null,
			long? fileSizeBytes = null,
			int? imageWidth = null,
			int? imageHeight = null,
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


			IQueryable<Database.MediaAsset> query = (from ma in _context.MediaAssets select ma);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (fileName != null)
			{
				query = query.Where(ma => ma.fileName == fileName);
			}
			if (filePath != null)
			{
				query = query.Where(ma => ma.filePath == filePath);
			}
			if (mimeType != null)
			{
				query = query.Where(ma => ma.mimeType == mimeType);
			}
			if (altText != null)
			{
				query = query.Where(ma => ma.altText == altText);
			}
			if (caption != null)
			{
				query = query.Where(ma => ma.caption == caption);
			}
			if (fileSizeBytes.HasValue == true)
			{
				query = query.Where(ma => ma.fileSizeBytes == fileSizeBytes.Value);
			}
			if (imageWidth.HasValue == true)
			{
				query = query.Where(ma => ma.imageWidth == imageWidth.Value);
			}
			if (imageHeight.HasValue == true)
			{
				query = query.Where(ma => ma.imageHeight == imageHeight.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ma => ma.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ma => ma.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ma => ma.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ma => ma.deleted == false);
				}
			}
			else
			{
				query = query.Where(ma => ma.active == true);
				query = query.Where(ma => ma.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Media Asset, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.fileName.Contains(anyStringContains)
			       || x.filePath.Contains(anyStringContains)
			       || x.mimeType.Contains(anyStringContains)
			       || x.altText.Contains(anyStringContains)
			       || x.caption.Contains(anyStringContains)
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
				IQueryable<Database.MediaAsset> query = (from ma in _context.MediaAssets where
							(ma.id == id) &&
							(userIsAdmin == true || ma.deleted == false) &&
							(userIsWriter == true || ma.active == true)
					select ma);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
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


			IQueryable<Database.MediaAsset> query = (from x in _context.MediaAssets
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

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
			//
			// The tenant guid for any MediaAsset being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the MediaAsset because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				mediaAsset.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (mediaAsset.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Community.MediaAsset record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (mediaAsset.fileName != null && mediaAsset.fileName.Length > 250)
			{
				mediaAsset.fileName = mediaAsset.fileName.Substring(0, 250);
			}

			if (mediaAsset.filePath != null && mediaAsset.filePath.Length > 500)
			{
				mediaAsset.filePath = mediaAsset.filePath.Substring(0, 500);
			}

			if (mediaAsset.mimeType != null && mediaAsset.mimeType.Length > 100)
			{
				mediaAsset.mimeType = mediaAsset.mimeType.Substring(0, 100);
			}

			if (mediaAsset.altText != null && mediaAsset.altText.Length > 250)
			{
				mediaAsset.altText = mediaAsset.altText.Substring(0, 250);
			}

			if (mediaAsset.caption != null && mediaAsset.caption.Length > 500)
			{
				mediaAsset.caption = mediaAsset.caption.Substring(0, 500);
			}

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
			// Create a new MediaAsset object using the data from the DTO
			//
			Database.MediaAsset mediaAsset = Database.MediaAsset.FromDTO(mediaAssetDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				mediaAsset.tenantGuid = userTenantGuid;

				if (mediaAsset.fileName != null && mediaAsset.fileName.Length > 250)
				{
					mediaAsset.fileName = mediaAsset.fileName.Substring(0, 250);
				}

				if (mediaAsset.filePath != null && mediaAsset.filePath.Length > 500)
				{
					mediaAsset.filePath = mediaAsset.filePath.Substring(0, 500);
				}

				if (mediaAsset.mimeType != null && mediaAsset.mimeType.Length > 100)
				{
					mediaAsset.mimeType = mediaAsset.mimeType.Substring(0, 100);
				}

				if (mediaAsset.altText != null && mediaAsset.altText.Length > 250)
				{
					mediaAsset.altText = mediaAsset.altText.Substring(0, 250);
				}

				if (mediaAsset.caption != null && mediaAsset.caption.Length > 500)
				{
					mediaAsset.caption = mediaAsset.caption.Substring(0, 500);
				}

				mediaAsset.objectGuid = Guid.NewGuid();
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

			IQueryable<Database.MediaAsset> query = (from x in _context.MediaAssets
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.MediaAsset mediaAsset = await query.FirstOrDefaultAsync(cancellationToken);

			if (mediaAsset == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.MediaAsset DELETE", id.ToString(), new Exception("No Community.MediaAsset entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.MediaAsset cloneOfExisting = (Database.MediaAsset)_context.Entry(mediaAsset).GetDatabaseValues().ToObject();


			try
			{
				mediaAsset.deleted = true;
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
			string fileName = null,
			string filePath = null,
			string mimeType = null,
			string altText = null,
			string caption = null,
			long? fileSizeBytes = null,
			int? imageWidth = null,
			int? imageHeight = null,
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

			IQueryable<Database.MediaAsset> query = (from ma in _context.MediaAssets select ma);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(fileName) == false)
			{
				query = query.Where(ma => ma.fileName == fileName);
			}
			if (string.IsNullOrEmpty(filePath) == false)
			{
				query = query.Where(ma => ma.filePath == filePath);
			}
			if (string.IsNullOrEmpty(mimeType) == false)
			{
				query = query.Where(ma => ma.mimeType == mimeType);
			}
			if (string.IsNullOrEmpty(altText) == false)
			{
				query = query.Where(ma => ma.altText == altText);
			}
			if (string.IsNullOrEmpty(caption) == false)
			{
				query = query.Where(ma => ma.caption == caption);
			}
			if (fileSizeBytes.HasValue == true)
			{
				query = query.Where(ma => ma.fileSizeBytes == fileSizeBytes.Value);
			}
			if (imageWidth.HasValue == true)
			{
				query = query.Where(ma => ma.imageWidth == imageWidth.Value);
			}
			if (imageHeight.HasValue == true)
			{
				query = query.Where(ma => ma.imageHeight == imageHeight.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ma => ma.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ma => ma.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ma => ma.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ma => ma.deleted == false);
				}
			}
			else
			{
				query = query.Where(ma => ma.active == true);
				query = query.Where(ma => ma.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Media Asset, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.fileName.Contains(anyStringContains)
			       || x.filePath.Contains(anyStringContains)
			       || x.mimeType.Contains(anyStringContains)
			       || x.altText.Contains(anyStringContains)
			       || x.caption.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


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
