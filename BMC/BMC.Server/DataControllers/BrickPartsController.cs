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
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the BrickPart entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the BrickPart entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class BrickPartsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object brickPartPutSyncRoot = new object();
		static object brickPartDeleteSyncRoot = new object();

		private BMCContext _context;

		private ILogger<BrickPartsController> _logger;

		public BrickPartsController(BMCContext context, ILogger<BrickPartsController> logger) : base("BMC", "BrickPart")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of BrickParts filtered by the parameters provided.
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
		[Route("api/BrickParts")]
		public async Task<IActionResult> GetBrickParts(
			string name = null,
			string rebrickablePartNum = null,
			string rebrickablePartUrl = null,
			string rebrickableImgUrl = null,
			string ldrawPartId = null,
			string bricklinkId = null,
			string brickowlId = null,
			string legoDesignId = null,
			string ldrawTitle = null,
			string ldrawCategory = null,
			int? partTypeId = null,
			string keywords = null,
			string author = null,
			int? brickCategoryId = null,
			float? widthLdu = null,
			float? heightLdu = null,
			float? depthLdu = null,
			float? massGrams = null,
			float? momentOfInertiaX = null,
			float? momentOfInertiaY = null,
			float? momentOfInertiaZ = null,
			float? frictionCoefficient = null,
			string materialType = null,
			float? centerOfMassX = null,
			float? centerOfMassY = null,
			float? centerOfMassZ = null,
			string geometryFileName = null,
			long? geometrySize = null,
			string geometryMimeType = null,
			string geometryFileFormat = null,
			string geometryOriginalFileName = null,
			float? boundingBoxMinX = null,
			float? boundingBoxMinY = null,
			float? boundingBoxMinZ = null,
			float? boundingBoxMaxX = null,
			float? boundingBoxMaxY = null,
			float? boundingBoxMaxZ = null,
			int? subFileCount = null,
			int? polygonCount = null,
			int? toothCount = null,
			float? gearRatio = null,
			DateTime? lastModifiedDate = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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
			if (lastModifiedDate.HasValue == true && lastModifiedDate.Value.Kind != DateTimeKind.Utc)
			{
				lastModifiedDate = lastModifiedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.BrickPart> query = (from bp in _context.BrickParts select bp);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(bp => bp.name == name);
			}
			if (string.IsNullOrEmpty(rebrickablePartNum) == false)
			{
				query = query.Where(bp => bp.rebrickablePartNum == rebrickablePartNum);
			}
			if (string.IsNullOrEmpty(rebrickablePartUrl) == false)
			{
				query = query.Where(bp => bp.rebrickablePartUrl == rebrickablePartUrl);
			}
			if (string.IsNullOrEmpty(rebrickableImgUrl) == false)
			{
				query = query.Where(bp => bp.rebrickableImgUrl == rebrickableImgUrl);
			}
			if (string.IsNullOrEmpty(ldrawPartId) == false)
			{
				query = query.Where(bp => bp.ldrawPartId == ldrawPartId);
			}
			if (string.IsNullOrEmpty(bricklinkId) == false)
			{
				query = query.Where(bp => bp.bricklinkId == bricklinkId);
			}
			if (string.IsNullOrEmpty(brickowlId) == false)
			{
				query = query.Where(bp => bp.brickowlId == brickowlId);
			}
			if (string.IsNullOrEmpty(legoDesignId) == false)
			{
				query = query.Where(bp => bp.legoDesignId == legoDesignId);
			}
			if (string.IsNullOrEmpty(ldrawTitle) == false)
			{
				query = query.Where(bp => bp.ldrawTitle == ldrawTitle);
			}
			if (string.IsNullOrEmpty(ldrawCategory) == false)
			{
				query = query.Where(bp => bp.ldrawCategory == ldrawCategory);
			}
			if (partTypeId.HasValue == true)
			{
				query = query.Where(bp => bp.partTypeId == partTypeId.Value);
			}
			if (string.IsNullOrEmpty(keywords) == false)
			{
				query = query.Where(bp => bp.keywords == keywords);
			}
			if (string.IsNullOrEmpty(author) == false)
			{
				query = query.Where(bp => bp.author == author);
			}
			if (brickCategoryId.HasValue == true)
			{
				query = query.Where(bp => bp.brickCategoryId == brickCategoryId.Value);
			}
			if (widthLdu.HasValue == true)
			{
				query = query.Where(bp => bp.widthLdu == widthLdu.Value);
			}
			if (heightLdu.HasValue == true)
			{
				query = query.Where(bp => bp.heightLdu == heightLdu.Value);
			}
			if (depthLdu.HasValue == true)
			{
				query = query.Where(bp => bp.depthLdu == depthLdu.Value);
			}
			if (massGrams.HasValue == true)
			{
				query = query.Where(bp => bp.massGrams == massGrams.Value);
			}
			if (momentOfInertiaX.HasValue == true)
			{
				query = query.Where(bp => bp.momentOfInertiaX == momentOfInertiaX.Value);
			}
			if (momentOfInertiaY.HasValue == true)
			{
				query = query.Where(bp => bp.momentOfInertiaY == momentOfInertiaY.Value);
			}
			if (momentOfInertiaZ.HasValue == true)
			{
				query = query.Where(bp => bp.momentOfInertiaZ == momentOfInertiaZ.Value);
			}
			if (frictionCoefficient.HasValue == true)
			{
				query = query.Where(bp => bp.frictionCoefficient == frictionCoefficient.Value);
			}
			if (string.IsNullOrEmpty(materialType) == false)
			{
				query = query.Where(bp => bp.materialType == materialType);
			}
			if (centerOfMassX.HasValue == true)
			{
				query = query.Where(bp => bp.centerOfMassX == centerOfMassX.Value);
			}
			if (centerOfMassY.HasValue == true)
			{
				query = query.Where(bp => bp.centerOfMassY == centerOfMassY.Value);
			}
			if (centerOfMassZ.HasValue == true)
			{
				query = query.Where(bp => bp.centerOfMassZ == centerOfMassZ.Value);
			}
			if (string.IsNullOrEmpty(geometryFileName) == false)
			{
				query = query.Where(bp => bp.geometryFileName == geometryFileName);
			}
			if (geometrySize.HasValue == true)
			{
				query = query.Where(bp => bp.geometrySize == geometrySize.Value);
			}
			if (string.IsNullOrEmpty(geometryMimeType) == false)
			{
				query = query.Where(bp => bp.geometryMimeType == geometryMimeType);
			}
			if (string.IsNullOrEmpty(geometryFileFormat) == false)
			{
				query = query.Where(bp => bp.geometryFileFormat == geometryFileFormat);
			}
			if (string.IsNullOrEmpty(geometryOriginalFileName) == false)
			{
				query = query.Where(bp => bp.geometryOriginalFileName == geometryOriginalFileName);
			}
			if (boundingBoxMinX.HasValue == true)
			{
				query = query.Where(bp => bp.boundingBoxMinX == boundingBoxMinX.Value);
			}
			if (boundingBoxMinY.HasValue == true)
			{
				query = query.Where(bp => bp.boundingBoxMinY == boundingBoxMinY.Value);
			}
			if (boundingBoxMinZ.HasValue == true)
			{
				query = query.Where(bp => bp.boundingBoxMinZ == boundingBoxMinZ.Value);
			}
			if (boundingBoxMaxX.HasValue == true)
			{
				query = query.Where(bp => bp.boundingBoxMaxX == boundingBoxMaxX.Value);
			}
			if (boundingBoxMaxY.HasValue == true)
			{
				query = query.Where(bp => bp.boundingBoxMaxY == boundingBoxMaxY.Value);
			}
			if (boundingBoxMaxZ.HasValue == true)
			{
				query = query.Where(bp => bp.boundingBoxMaxZ == boundingBoxMaxZ.Value);
			}
			if (subFileCount.HasValue == true)
			{
				query = query.Where(bp => bp.subFileCount == subFileCount.Value);
			}
			if (polygonCount.HasValue == true)
			{
				query = query.Where(bp => bp.polygonCount == polygonCount.Value);
			}
			if (toothCount.HasValue == true)
			{
				query = query.Where(bp => bp.toothCount == toothCount.Value);
			}
			if (gearRatio.HasValue == true)
			{
				query = query.Where(bp => bp.gearRatio == gearRatio.Value);
			}
			if (lastModifiedDate.HasValue == true)
			{
				query = query.Where(bp => bp.lastModifiedDate == lastModifiedDate.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(bp => bp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bp => bp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bp => bp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bp => bp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bp => bp.deleted == false);
				}
			}
			else
			{
				query = query.Where(bp => bp.active == true);
				query = query.Where(bp => bp.deleted == false);
			}

			query = query.OrderBy(bp => bp.name).ThenBy(bp => bp.rebrickablePartNum).ThenBy(bp => bp.rebrickablePartUrl);


			//
			// Add the any string contains parameter to span all the string fields on the Brick Part, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.rebrickablePartNum.Contains(anyStringContains)
			       || x.rebrickablePartUrl.Contains(anyStringContains)
			       || x.rebrickableImgUrl.Contains(anyStringContains)
			       || x.ldrawPartId.Contains(anyStringContains)
			       || x.bricklinkId.Contains(anyStringContains)
			       || x.brickowlId.Contains(anyStringContains)
			       || x.legoDesignId.Contains(anyStringContains)
			       || x.ldrawTitle.Contains(anyStringContains)
			       || x.ldrawCategory.Contains(anyStringContains)
			       || x.keywords.Contains(anyStringContains)
			       || x.author.Contains(anyStringContains)
			       || x.materialType.Contains(anyStringContains)
			       || x.geometryFileName.Contains(anyStringContains)
			       || x.geometryMimeType.Contains(anyStringContains)
			       || x.geometryFileFormat.Contains(anyStringContains)
			       || x.geometryOriginalFileName.Contains(anyStringContains)
			       || (includeRelations == true && x.brickCategory.name.Contains(anyStringContains))
			       || (includeRelations == true && x.brickCategory.description.Contains(anyStringContains))
			       || (includeRelations == true && x.partType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.partType.description.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.brickCategory);
				query = query.Include(x => x.partType);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.BrickPart> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.BrickPart brickPart in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(brickPart, databaseStoresDateWithTimeZone);
			}


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			if (diskBasedBinaryStorageMode == true)
			{
				var tasks = materialized.Select(async brickPart =>
				{

					if (brickPart.geometryData == null &&
					    brickPart.geometrySize.HasValue == true &&
					    brickPart.geometrySize.Value > 0)
					{
					    brickPart.geometryData = await LoadDataFromDiskAsync(brickPart.objectGuid, brickPart.versionNumber, "data");
					}

				}).ToList();

				// Run tasks concurrently and await their completion
				await Task.WhenAll(tasks);
			}

			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.BrickPart Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.BrickPart Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of BrickParts filtered by the parameters provided.  Its query is similar to the GetBrickParts method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickParts/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string rebrickablePartNum = null,
			string rebrickablePartUrl = null,
			string rebrickableImgUrl = null,
			string ldrawPartId = null,
			string bricklinkId = null,
			string brickowlId = null,
			string legoDesignId = null,
			string ldrawTitle = null,
			string ldrawCategory = null,
			int? partTypeId = null,
			string keywords = null,
			string author = null,
			int? brickCategoryId = null,
			float? widthLdu = null,
			float? heightLdu = null,
			float? depthLdu = null,
			float? massGrams = null,
			float? momentOfInertiaX = null,
			float? momentOfInertiaY = null,
			float? momentOfInertiaZ = null,
			float? frictionCoefficient = null,
			string materialType = null,
			float? centerOfMassX = null,
			float? centerOfMassY = null,
			float? centerOfMassZ = null,
			string geometryFileName = null,
			long? geometrySize = null,
			string geometryMimeType = null,
			string geometryFileFormat = null,
			string geometryOriginalFileName = null,
			float? boundingBoxMinX = null,
			float? boundingBoxMinY = null,
			float? boundingBoxMinZ = null,
			float? boundingBoxMaxX = null,
			float? boundingBoxMaxY = null,
			float? boundingBoxMaxZ = null,
			int? subFileCount = null,
			int? polygonCount = null,
			int? toothCount = null,
			float? gearRatio = null,
			DateTime? lastModifiedDate = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (lastModifiedDate.HasValue == true && lastModifiedDate.Value.Kind != DateTimeKind.Utc)
			{
				lastModifiedDate = lastModifiedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.BrickPart> query = (from bp in _context.BrickParts select bp);
			if (name != null)
			{
				query = query.Where(bp => bp.name == name);
			}
			if (rebrickablePartNum != null)
			{
				query = query.Where(bp => bp.rebrickablePartNum == rebrickablePartNum);
			}
			if (rebrickablePartUrl != null)
			{
				query = query.Where(bp => bp.rebrickablePartUrl == rebrickablePartUrl);
			}
			if (rebrickableImgUrl != null)
			{
				query = query.Where(bp => bp.rebrickableImgUrl == rebrickableImgUrl);
			}
			if (ldrawPartId != null)
			{
				query = query.Where(bp => bp.ldrawPartId == ldrawPartId);
			}
			if (bricklinkId != null)
			{
				query = query.Where(bp => bp.bricklinkId == bricklinkId);
			}
			if (brickowlId != null)
			{
				query = query.Where(bp => bp.brickowlId == brickowlId);
			}
			if (legoDesignId != null)
			{
				query = query.Where(bp => bp.legoDesignId == legoDesignId);
			}
			if (ldrawTitle != null)
			{
				query = query.Where(bp => bp.ldrawTitle == ldrawTitle);
			}
			if (ldrawCategory != null)
			{
				query = query.Where(bp => bp.ldrawCategory == ldrawCategory);
			}
			if (partTypeId.HasValue == true)
			{
				query = query.Where(bp => bp.partTypeId == partTypeId.Value);
			}
			if (keywords != null)
			{
				query = query.Where(bp => bp.keywords == keywords);
			}
			if (author != null)
			{
				query = query.Where(bp => bp.author == author);
			}
			if (brickCategoryId.HasValue == true)
			{
				query = query.Where(bp => bp.brickCategoryId == brickCategoryId.Value);
			}
			if (widthLdu.HasValue == true)
			{
				query = query.Where(bp => bp.widthLdu == widthLdu.Value);
			}
			if (heightLdu.HasValue == true)
			{
				query = query.Where(bp => bp.heightLdu == heightLdu.Value);
			}
			if (depthLdu.HasValue == true)
			{
				query = query.Where(bp => bp.depthLdu == depthLdu.Value);
			}
			if (massGrams.HasValue == true)
			{
				query = query.Where(bp => bp.massGrams == massGrams.Value);
			}
			if (momentOfInertiaX.HasValue == true)
			{
				query = query.Where(bp => bp.momentOfInertiaX == momentOfInertiaX.Value);
			}
			if (momentOfInertiaY.HasValue == true)
			{
				query = query.Where(bp => bp.momentOfInertiaY == momentOfInertiaY.Value);
			}
			if (momentOfInertiaZ.HasValue == true)
			{
				query = query.Where(bp => bp.momentOfInertiaZ == momentOfInertiaZ.Value);
			}
			if (frictionCoefficient.HasValue == true)
			{
				query = query.Where(bp => bp.frictionCoefficient == frictionCoefficient.Value);
			}
			if (materialType != null)
			{
				query = query.Where(bp => bp.materialType == materialType);
			}
			if (centerOfMassX.HasValue == true)
			{
				query = query.Where(bp => bp.centerOfMassX == centerOfMassX.Value);
			}
			if (centerOfMassY.HasValue == true)
			{
				query = query.Where(bp => bp.centerOfMassY == centerOfMassY.Value);
			}
			if (centerOfMassZ.HasValue == true)
			{
				query = query.Where(bp => bp.centerOfMassZ == centerOfMassZ.Value);
			}
			if (geometryFileName != null)
			{
				query = query.Where(bp => bp.geometryFileName == geometryFileName);
			}
			if (geometrySize.HasValue == true)
			{
				query = query.Where(bp => bp.geometrySize == geometrySize.Value);
			}
			if (geometryMimeType != null)
			{
				query = query.Where(bp => bp.geometryMimeType == geometryMimeType);
			}
			if (geometryFileFormat != null)
			{
				query = query.Where(bp => bp.geometryFileFormat == geometryFileFormat);
			}
			if (geometryOriginalFileName != null)
			{
				query = query.Where(bp => bp.geometryOriginalFileName == geometryOriginalFileName);
			}
			if (boundingBoxMinX.HasValue == true)
			{
				query = query.Where(bp => bp.boundingBoxMinX == boundingBoxMinX.Value);
			}
			if (boundingBoxMinY.HasValue == true)
			{
				query = query.Where(bp => bp.boundingBoxMinY == boundingBoxMinY.Value);
			}
			if (boundingBoxMinZ.HasValue == true)
			{
				query = query.Where(bp => bp.boundingBoxMinZ == boundingBoxMinZ.Value);
			}
			if (boundingBoxMaxX.HasValue == true)
			{
				query = query.Where(bp => bp.boundingBoxMaxX == boundingBoxMaxX.Value);
			}
			if (boundingBoxMaxY.HasValue == true)
			{
				query = query.Where(bp => bp.boundingBoxMaxY == boundingBoxMaxY.Value);
			}
			if (boundingBoxMaxZ.HasValue == true)
			{
				query = query.Where(bp => bp.boundingBoxMaxZ == boundingBoxMaxZ.Value);
			}
			if (subFileCount.HasValue == true)
			{
				query = query.Where(bp => bp.subFileCount == subFileCount.Value);
			}
			if (polygonCount.HasValue == true)
			{
				query = query.Where(bp => bp.polygonCount == polygonCount.Value);
			}
			if (toothCount.HasValue == true)
			{
				query = query.Where(bp => bp.toothCount == toothCount.Value);
			}
			if (gearRatio.HasValue == true)
			{
				query = query.Where(bp => bp.gearRatio == gearRatio.Value);
			}
			if (lastModifiedDate.HasValue == true)
			{
				query = query.Where(bp => bp.lastModifiedDate == lastModifiedDate.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(bp => bp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bp => bp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bp => bp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bp => bp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bp => bp.deleted == false);
				}
			}
			else
			{
				query = query.Where(bp => bp.active == true);
				query = query.Where(bp => bp.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Brick Part, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.rebrickablePartNum.Contains(anyStringContains)
			       || x.rebrickablePartUrl.Contains(anyStringContains)
			       || x.rebrickableImgUrl.Contains(anyStringContains)
			       || x.ldrawPartId.Contains(anyStringContains)
			       || x.bricklinkId.Contains(anyStringContains)
			       || x.brickowlId.Contains(anyStringContains)
			       || x.legoDesignId.Contains(anyStringContains)
			       || x.ldrawTitle.Contains(anyStringContains)
			       || x.ldrawCategory.Contains(anyStringContains)
			       || x.keywords.Contains(anyStringContains)
			       || x.author.Contains(anyStringContains)
			       || x.materialType.Contains(anyStringContains)
			       || x.geometryFileName.Contains(anyStringContains)
			       || x.geometryMimeType.Contains(anyStringContains)
			       || x.geometryFileFormat.Contains(anyStringContains)
			       || x.geometryOriginalFileName.Contains(anyStringContains)
			       || x.brickCategory.name.Contains(anyStringContains)
			       || x.brickCategory.description.Contains(anyStringContains)
			       || x.partType.name.Contains(anyStringContains)
			       || x.partType.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single BrickPart by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickPart/{id}")]
		public async Task<IActionResult> GetBrickPart(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.BrickPart> query = (from bp in _context.BrickParts where
							(bp.id == id) &&
							(userIsAdmin == true || bp.deleted == false) &&
							(userIsWriter == true || bp.active == true)
					select bp);

				if (includeRelations == true)
				{
					query = query.Include(x => x.brickCategory);
					query = query.Include(x => x.partType);
					query = query.AsSplitQuery();
				}

				Database.BrickPart materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

					if (diskBasedBinaryStorageMode == true &&
					    materialized.geometryData == null &&
					    materialized.geometrySize.HasValue == true &&
					    materialized.geometrySize.Value > 0)
					{
					    materialized.geometryData = await LoadDataFromDiskAsync(materialized.objectGuid, materialized.versionNumber, "data", cancellationToken);
					}

					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.BrickPart Entity was read with Admin privilege." : "BMC.BrickPart Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickPart", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.BrickPart entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.BrickPart.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.BrickPart.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing BrickPart record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/BrickPart/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutBrickPart(int id, [FromBody]Database.BrickPart.BrickPartDTO brickPartDTO, CancellationToken cancellationToken = default)
		{
			if (brickPartDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Catalog Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Catalog Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != brickPartDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.BrickPart> query = (from x in _context.BrickParts
				where
				(x.id == id)
				select x);


			Database.BrickPart existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickPart PUT", id.ToString(), new Exception("No BMC.BrickPart entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (brickPartDTO.objectGuid == Guid.Empty)
            {
                brickPartDTO.objectGuid = existing.objectGuid;
            }
            else if (brickPartDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a BrickPart record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.BrickPart cloneOfExisting = (Database.BrickPart)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new BrickPart object using the data from the existing record, updated with what is in the DTO.
			//
			Database.BrickPart brickPart = (Database.BrickPart)_context.Entry(existing).GetDatabaseValues().ToObject();
			brickPart.ApplyDTO(brickPartDTO);
			lock (brickPartPutSyncRoot)
			{
				//
				// Validate the version number for the brickPart being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != brickPart.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "BrickPart save attempt was made but save request was with version " + brickPart.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The BrickPart you are trying to update has already changed.  Please try your save again after reloading the BrickPart.");
				}
				else
				{
					// Same record.  Increase version.
					brickPart.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (brickPart.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.BrickPart record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (brickPart.name != null && brickPart.name.Length > 100)
				{
					brickPart.name = brickPart.name.Substring(0, 100);
				}

				if (brickPart.rebrickablePartNum != null && brickPart.rebrickablePartNum.Length > 100)
				{
					brickPart.rebrickablePartNum = brickPart.rebrickablePartNum.Substring(0, 100);
				}

				if (brickPart.rebrickablePartUrl != null && brickPart.rebrickablePartUrl.Length > 250)
				{
					brickPart.rebrickablePartUrl = brickPart.rebrickablePartUrl.Substring(0, 250);
				}

				if (brickPart.rebrickableImgUrl != null && brickPart.rebrickableImgUrl.Length > 250)
				{
					brickPart.rebrickableImgUrl = brickPart.rebrickableImgUrl.Substring(0, 250);
				}

				if (brickPart.ldrawPartId != null && brickPart.ldrawPartId.Length > 100)
				{
					brickPart.ldrawPartId = brickPart.ldrawPartId.Substring(0, 100);
				}

				if (brickPart.bricklinkId != null && brickPart.bricklinkId.Length > 100)
				{
					brickPart.bricklinkId = brickPart.bricklinkId.Substring(0, 100);
				}

				if (brickPart.brickowlId != null && brickPart.brickowlId.Length > 100)
				{
					brickPart.brickowlId = brickPart.brickowlId.Substring(0, 100);
				}

				if (brickPart.legoDesignId != null && brickPart.legoDesignId.Length > 100)
				{
					brickPart.legoDesignId = brickPart.legoDesignId.Substring(0, 100);
				}

				if (brickPart.ldrawTitle != null && brickPart.ldrawTitle.Length > 250)
				{
					brickPart.ldrawTitle = brickPart.ldrawTitle.Substring(0, 250);
				}

				if (brickPart.ldrawCategory != null && brickPart.ldrawCategory.Length > 100)
				{
					brickPart.ldrawCategory = brickPart.ldrawCategory.Substring(0, 100);
				}

				if (brickPart.author != null && brickPart.author.Length > 100)
				{
					brickPart.author = brickPart.author.Substring(0, 100);
				}

				if (brickPart.materialType != null && brickPart.materialType.Length > 50)
				{
					brickPart.materialType = brickPart.materialType.Substring(0, 50);
				}

				if (brickPart.geometryFileName != null && brickPart.geometryFileName.Length > 250)
				{
					brickPart.geometryFileName = brickPart.geometryFileName.Substring(0, 250);
				}

				if (brickPart.geometryMimeType != null && brickPart.geometryMimeType.Length > 100)
				{
					brickPart.geometryMimeType = brickPart.geometryMimeType.Substring(0, 100);
				}

				if (brickPart.geometryFileFormat != null && brickPart.geometryFileFormat.Length > 50)
				{
					brickPart.geometryFileFormat = brickPart.geometryFileFormat.Substring(0, 50);
				}

				if (brickPart.geometryOriginalFileName != null && brickPart.geometryOriginalFileName.Length > 250)
				{
					brickPart.geometryOriginalFileName = brickPart.geometryOriginalFileName.Substring(0, 250);
				}

				if (brickPart.lastModifiedDate.HasValue == true && brickPart.lastModifiedDate.Value.Kind != DateTimeKind.Utc)
				{
					brickPart.lastModifiedDate = brickPart.lastModifiedDate.Value.ToUniversalTime();
				}


				//
				// Add default values for any missing data attribute fields.
				//
				if (brickPart.geometryData != null && string.IsNullOrEmpty(brickPart.geometryFileName))
				{
				    brickPart.geometryFileName = brickPart.objectGuid.ToString() + ".data";
				}

				if (brickPart.geometryData != null && (brickPart.geometrySize.HasValue == false || brickPart.geometrySize != brickPart.geometryData.Length))
				{
				    brickPart.geometrySize = brickPart.geometryData.Length;
				}

				if (brickPart.geometryData != null && string.IsNullOrEmpty(brickPart.geometryMimeType))
				{
				    brickPart.geometryMimeType = "application/octet-stream";
				}

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				try
				{
					byte[] dataReferenceBeforeClearing = brickPart.geometryData;

					if (diskBasedBinaryStorageMode == true &&
					    brickPart.geometryFileName != null &&
					    brickPart.geometryData != null &&
					    brickPart.geometrySize.HasValue == true &&
					    brickPart.geometrySize.Value > 0)
					{
					    //
					    // write the bytes to disk
					    //
					    WriteDataToDisk(brickPart.objectGuid, brickPart.versionNumber, brickPart.geometryData, "data");

					    //
					    // Clear the data from the object before we put it into the db
					    //
					    brickPart.geometryData = null;

					}

				    EntityEntry<Database.BrickPart> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(brickPart);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        BrickPartChangeHistory brickPartChangeHistory = new BrickPartChangeHistory();
				        brickPartChangeHistory.brickPartId = brickPart.id;
				        brickPartChangeHistory.versionNumber = brickPart.versionNumber;
				        brickPartChangeHistory.timeStamp = DateTime.UtcNow;
				        brickPartChangeHistory.userId = securityUser.id;
				        brickPartChangeHistory.data = JsonSerializer.Serialize(Database.BrickPart.CreateAnonymousWithFirstLevelSubObjects(brickPart));
				        _context.BrickPartChangeHistories.Add(brickPartChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    brickPart.geometryData = dataReferenceBeforeClearing;
					}

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.BrickPart entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.BrickPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.BrickPart.CreateAnonymousWithFirstLevelSubObjects(brickPart)),
						null);

				return Ok(Database.BrickPart.CreateAnonymous(brickPart));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.BrickPart entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.BrickPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.BrickPart.CreateAnonymousWithFirstLevelSubObjects(brickPart)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new BrickPart record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickPart", Name = "BrickPart")]
		public async Task<IActionResult> PostBrickPart([FromBody]Database.BrickPart.BrickPartDTO brickPartDTO, CancellationToken cancellationToken = default)
		{
			if (brickPartDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Catalog Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Catalog Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new BrickPart object using the data from the DTO
			//
			Database.BrickPart brickPart = Database.BrickPart.FromDTO(brickPartDTO);

			try
			{
				if (brickPart.name != null && brickPart.name.Length > 100)
				{
					brickPart.name = brickPart.name.Substring(0, 100);
				}

				if (brickPart.rebrickablePartNum != null && brickPart.rebrickablePartNum.Length > 100)
				{
					brickPart.rebrickablePartNum = brickPart.rebrickablePartNum.Substring(0, 100);
				}

				if (brickPart.rebrickablePartUrl != null && brickPart.rebrickablePartUrl.Length > 250)
				{
					brickPart.rebrickablePartUrl = brickPart.rebrickablePartUrl.Substring(0, 250);
				}

				if (brickPart.rebrickableImgUrl != null && brickPart.rebrickableImgUrl.Length > 250)
				{
					brickPart.rebrickableImgUrl = brickPart.rebrickableImgUrl.Substring(0, 250);
				}

				if (brickPart.ldrawPartId != null && brickPart.ldrawPartId.Length > 100)
				{
					brickPart.ldrawPartId = brickPart.ldrawPartId.Substring(0, 100);
				}

				if (brickPart.bricklinkId != null && brickPart.bricklinkId.Length > 100)
				{
					brickPart.bricklinkId = brickPart.bricklinkId.Substring(0, 100);
				}

				if (brickPart.brickowlId != null && brickPart.brickowlId.Length > 100)
				{
					brickPart.brickowlId = brickPart.brickowlId.Substring(0, 100);
				}

				if (brickPart.legoDesignId != null && brickPart.legoDesignId.Length > 100)
				{
					brickPart.legoDesignId = brickPart.legoDesignId.Substring(0, 100);
				}

				if (brickPart.ldrawTitle != null && brickPart.ldrawTitle.Length > 250)
				{
					brickPart.ldrawTitle = brickPart.ldrawTitle.Substring(0, 250);
				}

				if (brickPart.ldrawCategory != null && brickPart.ldrawCategory.Length > 100)
				{
					brickPart.ldrawCategory = brickPart.ldrawCategory.Substring(0, 100);
				}

				if (brickPart.author != null && brickPart.author.Length > 100)
				{
					brickPart.author = brickPart.author.Substring(0, 100);
				}

				if (brickPart.materialType != null && brickPart.materialType.Length > 50)
				{
					brickPart.materialType = brickPart.materialType.Substring(0, 50);
				}

				if (brickPart.geometryFileName != null && brickPart.geometryFileName.Length > 250)
				{
					brickPart.geometryFileName = brickPart.geometryFileName.Substring(0, 250);
				}

				if (brickPart.geometryMimeType != null && brickPart.geometryMimeType.Length > 100)
				{
					brickPart.geometryMimeType = brickPart.geometryMimeType.Substring(0, 100);
				}

				if (brickPart.geometryFileFormat != null && brickPart.geometryFileFormat.Length > 50)
				{
					brickPart.geometryFileFormat = brickPart.geometryFileFormat.Substring(0, 50);
				}

				if (brickPart.geometryOriginalFileName != null && brickPart.geometryOriginalFileName.Length > 250)
				{
					brickPart.geometryOriginalFileName = brickPart.geometryOriginalFileName.Substring(0, 250);
				}

				if (brickPart.lastModifiedDate.HasValue == true && brickPart.lastModifiedDate.Value.Kind != DateTimeKind.Utc)
				{
					brickPart.lastModifiedDate = brickPart.lastModifiedDate.Value.ToUniversalTime();
				}

				brickPart.objectGuid = Guid.NewGuid();

				//
				// Add default values for any missing data attribute fields.
				//
				if (brickPart.geometryData != null && string.IsNullOrEmpty(brickPart.geometryFileName))
				{
				    brickPart.geometryFileName = brickPart.objectGuid.ToString() + ".data";
				}

				if (brickPart.geometryData != null && (brickPart.geometrySize.HasValue == false || brickPart.geometrySize != brickPart.geometryData.Length))
				{
				    brickPart.geometrySize = brickPart.geometryData.Length;
				}

				if (brickPart.geometryData != null && string.IsNullOrEmpty(brickPart.geometryMimeType))
				{
				    brickPart.geometryMimeType = "application/octet-stream";
				}

				brickPart.versionNumber = 1;

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				byte[] dataReferenceBeforeClearing = brickPart.geometryData;

				if (diskBasedBinaryStorageMode == true &&
				    brickPart.geometryData != null &&
				    brickPart.geometryFileName != null &&
				    brickPart.geometrySize.HasValue == true &&
				    brickPart.geometrySize.Value > 0)
				{
				    //
				    // write the bytes to disk
				    //
				    await WriteDataToDiskAsync(brickPart.objectGuid, brickPart.versionNumber, brickPart.geometryData, "data", cancellationToken);

				    //
				    // Clear the data from the object before we put it into the db
				    //
				    brickPart.geometryData = null;

				}

				_context.BrickParts.Add(brickPart);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the brickPart object so that no further changes will be written to the database
				    //
				    _context.Entry(brickPart).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					brickPart.geometryData = null;
					brickPart.BrickElements = null;
					brickPart.BrickPartChangeHistories = null;
					brickPart.BrickPartColours = null;
					brickPart.BrickPartConnectors = null;
					brickPart.BrickPartRelationshipchildBrickParts = null;
					brickPart.BrickPartRelationshipparentBrickParts = null;
					brickPart.LegoSetParts = null;
					brickPart.ModelStepParts = null;
					brickPart.PartSubFileReferenceparentBrickParts = null;
					brickPart.PartSubFileReferencereferencedBrickParts = null;
					brickPart.PlacedBricks = null;
					brickPart.UserCollectionParts = null;
					brickPart.UserLostParts = null;
					brickPart.UserPartListItems = null;
					brickPart.UserWishlistItems = null;
					brickPart.brickCategory = null;
					brickPart.partType = null;


				    BrickPartChangeHistory brickPartChangeHistory = new BrickPartChangeHistory();
				    brickPartChangeHistory.brickPartId = brickPart.id;
				    brickPartChangeHistory.versionNumber = brickPart.versionNumber;
				    brickPartChangeHistory.timeStamp = DateTime.UtcNow;
				    brickPartChangeHistory.userId = securityUser.id;
				    brickPartChangeHistory.data = JsonSerializer.Serialize(Database.BrickPart.CreateAnonymousWithFirstLevelSubObjects(brickPart));
				    _context.BrickPartChangeHistories.Add(brickPartChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"BMC.BrickPart entity successfully created.",
						true,
						brickPart. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.BrickPart.CreateAnonymousWithFirstLevelSubObjects(brickPart)),
						null);



					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    brickPart.geometryData = dataReferenceBeforeClearing;
					}

				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.BrickPart entity creation failed.", false, brickPart.id.ToString(), "", JsonSerializer.Serialize(Database.BrickPart.CreateAnonymousWithFirstLevelSubObjects(brickPart)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickPart", brickPart.id, brickPart.name));

			return CreatedAtRoute("BrickPart", new { id = brickPart.id }, Database.BrickPart.CreateAnonymousWithFirstLevelSubObjects(brickPart));
		}



        /// <summary>
        /// 
        /// This rolls a BrickPart entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickPart/Rollback/{id}")]
		[Route("api/BrickPart/Rollback")]
		public async Task<IActionResult> RollbackToBrickPartVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.BrickPart> query = (from x in _context.BrickParts
			        where
			        (x.id == id)
			        select x);

			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();


			//
			// Make sure nobody else is editing this BrickPart concurrently
			//
			lock (brickPartPutSyncRoot)
			{
				
				Database.BrickPart brickPart = query.FirstOrDefault();
				
				if (brickPart == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickPart rollback", id.ToString(), new Exception("No BMC.BrickPart entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the BrickPart current state so we can log it.
				//
				Database.BrickPart cloneOfExisting = (Database.BrickPart)_context.Entry(brickPart).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.geometryData = null;
				cloneOfExisting.BrickElements = null;
				cloneOfExisting.BrickPartChangeHistories = null;
				cloneOfExisting.BrickPartColours = null;
				cloneOfExisting.BrickPartConnectors = null;
				cloneOfExisting.BrickPartRelationshipchildBrickParts = null;
				cloneOfExisting.BrickPartRelationshipparentBrickParts = null;
				cloneOfExisting.LegoSetParts = null;
				cloneOfExisting.ModelStepParts = null;
				cloneOfExisting.PartSubFileReferenceparentBrickParts = null;
				cloneOfExisting.PartSubFileReferencereferencedBrickParts = null;
				cloneOfExisting.PlacedBricks = null;
				cloneOfExisting.UserCollectionParts = null;
				cloneOfExisting.UserLostParts = null;
				cloneOfExisting.UserPartListItems = null;
				cloneOfExisting.UserWishlistItems = null;
				cloneOfExisting.brickCategory = null;
				cloneOfExisting.partType = null;

				if (versionNumber >= brickPart.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for BMC.BrickPart rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for BMC.BrickPart rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				BrickPartChangeHistory brickPartChangeHistory = (from x in _context.BrickPartChangeHistories
				                                               where
				                                               x.brickPartId == id &&
				                                               x.versionNumber == versionNumber
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (brickPartChangeHistory != null)
				{
				    Database.BrickPart oldBrickPart = JsonSerializer.Deserialize<Database.BrickPart>(brickPartChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    brickPart.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    brickPart.name = oldBrickPart.name;
				    brickPart.rebrickablePartNum = oldBrickPart.rebrickablePartNum;
				    brickPart.rebrickablePartUrl = oldBrickPart.rebrickablePartUrl;
				    brickPart.rebrickableImgUrl = oldBrickPart.rebrickableImgUrl;
				    brickPart.ldrawPartId = oldBrickPart.ldrawPartId;
				    brickPart.bricklinkId = oldBrickPart.bricklinkId;
				    brickPart.brickowlId = oldBrickPart.brickowlId;
				    brickPart.legoDesignId = oldBrickPart.legoDesignId;
				    brickPart.ldrawTitle = oldBrickPart.ldrawTitle;
				    brickPart.ldrawCategory = oldBrickPart.ldrawCategory;
				    brickPart.partTypeId = oldBrickPart.partTypeId;
				    brickPart.keywords = oldBrickPart.keywords;
				    brickPart.author = oldBrickPart.author;
				    brickPart.brickCategoryId = oldBrickPart.brickCategoryId;
				    brickPart.widthLdu = oldBrickPart.widthLdu;
				    brickPart.heightLdu = oldBrickPart.heightLdu;
				    brickPart.depthLdu = oldBrickPart.depthLdu;
				    brickPart.massGrams = oldBrickPart.massGrams;
				    brickPart.momentOfInertiaX = oldBrickPart.momentOfInertiaX;
				    brickPart.momentOfInertiaY = oldBrickPart.momentOfInertiaY;
				    brickPart.momentOfInertiaZ = oldBrickPart.momentOfInertiaZ;
				    brickPart.frictionCoefficient = oldBrickPart.frictionCoefficient;
				    brickPart.materialType = oldBrickPart.materialType;
				    brickPart.centerOfMassX = oldBrickPart.centerOfMassX;
				    brickPart.centerOfMassY = oldBrickPart.centerOfMassY;
				    brickPart.centerOfMassZ = oldBrickPart.centerOfMassZ;
				    brickPart.geometryFileName = oldBrickPart.geometryFileName;
				    brickPart.geometrySize = oldBrickPart.geometrySize;
				    brickPart.geometryData = oldBrickPart.geometryData;
				    brickPart.geometryMimeType = oldBrickPart.geometryMimeType;
				    brickPart.geometryFileFormat = oldBrickPart.geometryFileFormat;
				    brickPart.geometryOriginalFileName = oldBrickPart.geometryOriginalFileName;
				    brickPart.boundingBoxMinX = oldBrickPart.boundingBoxMinX;
				    brickPart.boundingBoxMinY = oldBrickPart.boundingBoxMinY;
				    brickPart.boundingBoxMinZ = oldBrickPart.boundingBoxMinZ;
				    brickPart.boundingBoxMaxX = oldBrickPart.boundingBoxMaxX;
				    brickPart.boundingBoxMaxY = oldBrickPart.boundingBoxMaxY;
				    brickPart.boundingBoxMaxZ = oldBrickPart.boundingBoxMaxZ;
				    brickPart.subFileCount = oldBrickPart.subFileCount;
				    brickPart.polygonCount = oldBrickPart.polygonCount;
				    brickPart.toothCount = oldBrickPart.toothCount;
				    brickPart.gearRatio = oldBrickPart.gearRatio;
				    brickPart.lastModifiedDate = oldBrickPart.lastModifiedDate;
				    brickPart.objectGuid = oldBrickPart.objectGuid;
				    brickPart.active = oldBrickPart.active;
				    brickPart.deleted = oldBrickPart.deleted;
				    //
				    // If disk based binary mode is on, then we need to copy the old data file over as well.
				    //
				    if (diskBasedBinaryStorageMode == true)
				    {
				    	Byte[] binaryData = LoadDataFromDisk(oldBrickPart.objectGuid, oldBrickPart.versionNumber, "data");

				    	//
				    	// Write out the data as the new version
				    	//
				    	WriteDataToDisk(brickPart.objectGuid, brickPart.versionNumber, binaryData, "data");
				    }

				    string serializedBrickPart = JsonSerializer.Serialize(brickPart);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        BrickPartChangeHistory newBrickPartChangeHistory = new BrickPartChangeHistory();
				        newBrickPartChangeHistory.brickPartId = brickPart.id;
				        newBrickPartChangeHistory.versionNumber = brickPart.versionNumber;
				        newBrickPartChangeHistory.timeStamp = DateTime.UtcNow;
				        newBrickPartChangeHistory.userId = securityUser.id;
				        newBrickPartChangeHistory.data = JsonSerializer.Serialize(Database.BrickPart.CreateAnonymousWithFirstLevelSubObjects(brickPart));
				        _context.BrickPartChangeHistories.Add(newBrickPartChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.BrickPart rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.BrickPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.BrickPart.CreateAnonymousWithFirstLevelSubObjects(brickPart)),
						null);


				    return Ok(Database.BrickPart.CreateAnonymous(brickPart));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for BMC.BrickPart rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for BMC.BrickPart rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a BrickPart.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the BrickPart</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickPart/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetBrickPartChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Database.BrickPart brickPart = await _context.BrickParts.Where(x => x.id == id
			).FirstOrDefaultAsync(cancellationToken);

			if (brickPart == null)
			{
				return NotFound();
			}

			try
			{
				brickPart.SetupVersionInquiry(_context, Guid.Empty);

				VersionInformation<Database.BrickPart> versionInfo = await brickPart.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a BrickPart.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the BrickPart</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickPart/{id}/AuditHistory")]
		public async Task<IActionResult> GetBrickPartAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Database.BrickPart brickPart = await _context.BrickParts.Where(x => x.id == id
			).FirstOrDefaultAsync(cancellationToken);

			if (brickPart == null)
			{
				return NotFound();
			}

			try
			{
				brickPart.SetupVersionInquiry(_context, Guid.Empty);

				List<VersionInformation<Database.BrickPart>> versions = await brickPart.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a BrickPart.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the BrickPart</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The BrickPart object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickPart/{id}/Version/{version}")]
		public async Task<IActionResult> GetBrickPartVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Database.BrickPart brickPart = await _context.BrickParts.Where(x => x.id == id
			).FirstOrDefaultAsync(cancellationToken);

			if (brickPart == null)
			{
				return NotFound();
			}

			try
			{
				brickPart.SetupVersionInquiry(_context, Guid.Empty);

				VersionInformation<Database.BrickPart> versionInfo = await brickPart.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a BrickPart at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the BrickPart</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The BrickPart object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickPart/{id}/StateAtTime")]
		public async Task<IActionResult> GetBrickPartStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Database.BrickPart brickPart = await _context.BrickParts.Where(x => x.id == id
			).FirstOrDefaultAsync(cancellationToken);

			if (brickPart == null)
			{
				return NotFound();
			}

			try
			{
				brickPart.SetupVersionInquiry(_context, Guid.Empty);

				VersionInformation<Database.BrickPart> versionInfo = await brickPart.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a BrickPart record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickPart/{id}")]
		[Route("api/BrickPart")]
		public async Task<IActionResult> DeleteBrickPart(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Catalog Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Catalog Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.BrickPart> query = (from x in _context.BrickParts
				where
				(x.id == id)
				select x);


			Database.BrickPart brickPart = await query.FirstOrDefaultAsync(cancellationToken);

			if (brickPart == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickPart DELETE", id.ToString(), new Exception("No BMC.BrickPart entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.BrickPart cloneOfExisting = (Database.BrickPart)_context.Entry(brickPart).GetDatabaseValues().ToObject();


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			lock (brickPartDeleteSyncRoot)
			{
			    try
			    {
			        brickPart.deleted = true;
			        brickPart.versionNumber++;

			        _context.SaveChanges();

			        //
			        // If in disk based storage mode, create a copy of the disk data file for the new version.
			        //
			        if (diskBasedBinaryStorageMode == true)
			        {
			        	Byte[] binaryData = LoadDataFromDisk(brickPart.objectGuid, brickPart.versionNumber -1, "data");

			        	//
			        	// Write out the same data
			        	//
			        	WriteDataToDisk(brickPart.objectGuid, brickPart.versionNumber, binaryData, "data");
			        }

			        //
			        // Now add the change history
			        //
			        BrickPartChangeHistory brickPartChangeHistory = new BrickPartChangeHistory();
			        brickPartChangeHistory.brickPartId = brickPart.id;
			        brickPartChangeHistory.versionNumber = brickPart.versionNumber;
			        brickPartChangeHistory.timeStamp = DateTime.UtcNow;
			        brickPartChangeHistory.userId = securityUser.id;
			        brickPartChangeHistory.data = JsonSerializer.Serialize(Database.BrickPart.CreateAnonymousWithFirstLevelSubObjects(brickPart));
			        _context.BrickPartChangeHistories.Add(brickPartChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.BrickPart entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.BrickPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.BrickPart.CreateAnonymousWithFirstLevelSubObjects(brickPart)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.BrickPart entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.BrickPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.BrickPart.CreateAnonymousWithFirstLevelSubObjects(brickPart)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of BrickPart records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/BrickParts/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string rebrickablePartNum = null,
			string rebrickablePartUrl = null,
			string rebrickableImgUrl = null,
			string ldrawPartId = null,
			string bricklinkId = null,
			string brickowlId = null,
			string legoDesignId = null,
			string ldrawTitle = null,
			string ldrawCategory = null,
			int? partTypeId = null,
			string keywords = null,
			string author = null,
			int? brickCategoryId = null,
			float? widthLdu = null,
			float? heightLdu = null,
			float? depthLdu = null,
			float? massGrams = null,
			float? momentOfInertiaX = null,
			float? momentOfInertiaY = null,
			float? momentOfInertiaZ = null,
			float? frictionCoefficient = null,
			string materialType = null,
			float? centerOfMassX = null,
			float? centerOfMassY = null,
			float? centerOfMassZ = null,
			string geometryFileName = null,
			long? geometrySize = null,
			string geometryMimeType = null,
			string geometryFileFormat = null,
			string geometryOriginalFileName = null,
			float? boundingBoxMinX = null,
			float? boundingBoxMinY = null,
			float? boundingBoxMinZ = null,
			float? boundingBoxMaxX = null,
			float? boundingBoxMaxY = null,
			float? boundingBoxMaxZ = null,
			int? subFileCount = null,
			int? polygonCount = null,
			int? toothCount = null,
			float? gearRatio = null,
			DateTime? lastModifiedDate = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);


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
			if (lastModifiedDate.HasValue == true && lastModifiedDate.Value.Kind != DateTimeKind.Utc)
			{
				lastModifiedDate = lastModifiedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.BrickPart> query = (from bp in _context.BrickParts select bp);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(bp => bp.name == name);
			}
			if (string.IsNullOrEmpty(rebrickablePartNum) == false)
			{
				query = query.Where(bp => bp.rebrickablePartNum == rebrickablePartNum);
			}
			if (string.IsNullOrEmpty(rebrickablePartUrl) == false)
			{
				query = query.Where(bp => bp.rebrickablePartUrl == rebrickablePartUrl);
			}
			if (string.IsNullOrEmpty(rebrickableImgUrl) == false)
			{
				query = query.Where(bp => bp.rebrickableImgUrl == rebrickableImgUrl);
			}
			if (string.IsNullOrEmpty(ldrawPartId) == false)
			{
				query = query.Where(bp => bp.ldrawPartId == ldrawPartId);
			}
			if (string.IsNullOrEmpty(bricklinkId) == false)
			{
				query = query.Where(bp => bp.bricklinkId == bricklinkId);
			}
			if (string.IsNullOrEmpty(brickowlId) == false)
			{
				query = query.Where(bp => bp.brickowlId == brickowlId);
			}
			if (string.IsNullOrEmpty(legoDesignId) == false)
			{
				query = query.Where(bp => bp.legoDesignId == legoDesignId);
			}
			if (string.IsNullOrEmpty(ldrawTitle) == false)
			{
				query = query.Where(bp => bp.ldrawTitle == ldrawTitle);
			}
			if (string.IsNullOrEmpty(ldrawCategory) == false)
			{
				query = query.Where(bp => bp.ldrawCategory == ldrawCategory);
			}
			if (partTypeId.HasValue == true)
			{
				query = query.Where(bp => bp.partTypeId == partTypeId.Value);
			}
			if (string.IsNullOrEmpty(keywords) == false)
			{
				query = query.Where(bp => bp.keywords == keywords);
			}
			if (string.IsNullOrEmpty(author) == false)
			{
				query = query.Where(bp => bp.author == author);
			}
			if (brickCategoryId.HasValue == true)
			{
				query = query.Where(bp => bp.brickCategoryId == brickCategoryId.Value);
			}
			if (widthLdu.HasValue == true)
			{
				query = query.Where(bp => bp.widthLdu == widthLdu.Value);
			}
			if (heightLdu.HasValue == true)
			{
				query = query.Where(bp => bp.heightLdu == heightLdu.Value);
			}
			if (depthLdu.HasValue == true)
			{
				query = query.Where(bp => bp.depthLdu == depthLdu.Value);
			}
			if (massGrams.HasValue == true)
			{
				query = query.Where(bp => bp.massGrams == massGrams.Value);
			}
			if (momentOfInertiaX.HasValue == true)
			{
				query = query.Where(bp => bp.momentOfInertiaX == momentOfInertiaX.Value);
			}
			if (momentOfInertiaY.HasValue == true)
			{
				query = query.Where(bp => bp.momentOfInertiaY == momentOfInertiaY.Value);
			}
			if (momentOfInertiaZ.HasValue == true)
			{
				query = query.Where(bp => bp.momentOfInertiaZ == momentOfInertiaZ.Value);
			}
			if (frictionCoefficient.HasValue == true)
			{
				query = query.Where(bp => bp.frictionCoefficient == frictionCoefficient.Value);
			}
			if (string.IsNullOrEmpty(materialType) == false)
			{
				query = query.Where(bp => bp.materialType == materialType);
			}
			if (centerOfMassX.HasValue == true)
			{
				query = query.Where(bp => bp.centerOfMassX == centerOfMassX.Value);
			}
			if (centerOfMassY.HasValue == true)
			{
				query = query.Where(bp => bp.centerOfMassY == centerOfMassY.Value);
			}
			if (centerOfMassZ.HasValue == true)
			{
				query = query.Where(bp => bp.centerOfMassZ == centerOfMassZ.Value);
			}
			if (string.IsNullOrEmpty(geometryFileName) == false)
			{
				query = query.Where(bp => bp.geometryFileName == geometryFileName);
			}
			if (geometrySize.HasValue == true)
			{
				query = query.Where(bp => bp.geometrySize == geometrySize.Value);
			}
			if (string.IsNullOrEmpty(geometryMimeType) == false)
			{
				query = query.Where(bp => bp.geometryMimeType == geometryMimeType);
			}
			if (string.IsNullOrEmpty(geometryFileFormat) == false)
			{
				query = query.Where(bp => bp.geometryFileFormat == geometryFileFormat);
			}
			if (string.IsNullOrEmpty(geometryOriginalFileName) == false)
			{
				query = query.Where(bp => bp.geometryOriginalFileName == geometryOriginalFileName);
			}
			if (boundingBoxMinX.HasValue == true)
			{
				query = query.Where(bp => bp.boundingBoxMinX == boundingBoxMinX.Value);
			}
			if (boundingBoxMinY.HasValue == true)
			{
				query = query.Where(bp => bp.boundingBoxMinY == boundingBoxMinY.Value);
			}
			if (boundingBoxMinZ.HasValue == true)
			{
				query = query.Where(bp => bp.boundingBoxMinZ == boundingBoxMinZ.Value);
			}
			if (boundingBoxMaxX.HasValue == true)
			{
				query = query.Where(bp => bp.boundingBoxMaxX == boundingBoxMaxX.Value);
			}
			if (boundingBoxMaxY.HasValue == true)
			{
				query = query.Where(bp => bp.boundingBoxMaxY == boundingBoxMaxY.Value);
			}
			if (boundingBoxMaxZ.HasValue == true)
			{
				query = query.Where(bp => bp.boundingBoxMaxZ == boundingBoxMaxZ.Value);
			}
			if (subFileCount.HasValue == true)
			{
				query = query.Where(bp => bp.subFileCount == subFileCount.Value);
			}
			if (polygonCount.HasValue == true)
			{
				query = query.Where(bp => bp.polygonCount == polygonCount.Value);
			}
			if (toothCount.HasValue == true)
			{
				query = query.Where(bp => bp.toothCount == toothCount.Value);
			}
			if (gearRatio.HasValue == true)
			{
				query = query.Where(bp => bp.gearRatio == gearRatio.Value);
			}
			if (lastModifiedDate.HasValue == true)
			{
				query = query.Where(bp => bp.lastModifiedDate == lastModifiedDate.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(bp => bp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bp => bp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bp => bp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bp => bp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bp => bp.deleted == false);
				}
			}
			else
			{
				query = query.Where(bp => bp.active == true);
				query = query.Where(bp => bp.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Brick Part, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.rebrickablePartNum.Contains(anyStringContains)
			       || x.rebrickablePartUrl.Contains(anyStringContains)
			       || x.rebrickableImgUrl.Contains(anyStringContains)
			       || x.ldrawPartId.Contains(anyStringContains)
			       || x.bricklinkId.Contains(anyStringContains)
			       || x.brickowlId.Contains(anyStringContains)
			       || x.legoDesignId.Contains(anyStringContains)
			       || x.ldrawTitle.Contains(anyStringContains)
			       || x.ldrawCategory.Contains(anyStringContains)
			       || x.keywords.Contains(anyStringContains)
			       || x.author.Contains(anyStringContains)
			       || x.materialType.Contains(anyStringContains)
			       || x.geometryFileName.Contains(anyStringContains)
			       || x.geometryMimeType.Contains(anyStringContains)
			       || x.geometryFileFormat.Contains(anyStringContains)
			       || x.geometryOriginalFileName.Contains(anyStringContains)
			       || x.brickCategory.name.Contains(anyStringContains)
			       || x.brickCategory.description.Contains(anyStringContains)
			       || x.partType.name.Contains(anyStringContains)
			       || x.partType.description.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.name).ThenBy(x => x.rebrickablePartNum).ThenBy(x => x.rebrickablePartUrl);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.BrickPart.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/BrickPart/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Catalog Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Catalog Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}




        [Route("api/BrickPart/Data/{id:int}")]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [HttpPost]
        [HttpPut]
        public async Task<IActionResult> UploadData(int id, CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			MediaTypeHeaderValue mediaTypeHeader; 

            if (!HttpContext.Request.HasFormContentType ||
				!MediaTypeHeaderValue.TryParse(HttpContext.Request.ContentType, out mediaTypeHeader) ||
                string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
            {
                return new UnsupportedMediaTypeResult();
            }


            Database.BrickPart brickPart = await (from x in _context.BrickParts where x.id == id && x.active == true && x.deleted == false select x).FirstOrDefaultAsync();
            if (brickPart == null)
            {
                return NotFound();
            }

            bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();


            // This will be used to signal whether we are saving data or clearing it.
            bool foundFileData = false;


            //
            // This will get the first file from the request and save it
            //
			try
			{
                MultipartReader reader = new MultipartReader(mediaTypeHeader.Boundary.Value, HttpContext.Request.Body);
                MultipartSection section = await reader.ReadNextSectionAsync();

                while (section != null)
				{
					ContentDispositionHeaderValue contentDisposition;

					bool hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);


					if (hasContentDispositionHeader && contentDisposition.DispositionType.Equals("form-data") &&
						!string.IsNullOrEmpty(contentDisposition.FileName.Value))
					{

						foundFileData = true;
						string fileName = contentDisposition.FileName.ToString().Trim('"');

						// default the mime type to be the one for arbitrary binary data unless we have a mime type on the content headers that tells us otherwise.
						MediaTypeHeaderValue mediaType;
						bool hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);

						string mimeType = "application/octet-stream";
						if (hasMediaTypeHeader && mediaTypeHeader.MediaType != null )
						{
							mimeType = mediaTypeHeader.MediaType.ToString();
						}

						lock (brickPartPutSyncRoot)
						{
							try
							{
								using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
								{
									brickPart.geometryFileName = fileName.Trim();
									brickPart.geometryMimeType = mimeType;
									brickPart.geometrySize = section.Body.Length;

									brickPart.versionNumber++;

									if (diskBasedBinaryStorageMode == true &&
										 brickPart.geometryFileName != null &&
										 brickPart.geometrySize > 0)
									{
										//
										// write the bytes to disk
										//
										WriteDataToDisk(brickPart.objectGuid, brickPart.versionNumber, section.Body, "data");
										//
										// Clear the data from the object before we put it into the db
										//
										brickPart.geometryData = null;
									}
									else
									{
										using (MemoryStream memoryStream = new MemoryStream((int)section.Body.Length))
										{
											section.Body.CopyTo(memoryStream);
											brickPart.geometryData = memoryStream.ToArray();
										}
									}
									//
									// Now add the change history
									//
									BrickPartChangeHistory brickPartChangeHistory = new BrickPartChangeHistory();
									brickPartChangeHistory.brickPartId = brickPart.id;
									brickPartChangeHistory.versionNumber = brickPart.versionNumber;
									brickPartChangeHistory.timeStamp = DateTime.UtcNow;
									brickPartChangeHistory.userId = securityUser.id;
									brickPartChangeHistory.data = JsonSerializer.Serialize(Database.BrickPart.CreateAnonymousWithFirstLevelSubObjects(brickPart));
									_context.BrickPartChangeHistories.Add(brickPartChangeHistory);

									_context.SaveChanges();

									transaction.Commit();

									CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "BrickPart Data Uploaded with filename of " + fileName + " and with size of " + section.Body.Length, id.ToString());
								}
							}
							catch (Exception ex)
							{
								CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "BrickPart Data Upload Failed.", false, id.ToString(), "", "", ex);

								return Problem(ex.Message);
							}
						}


						//
						// Stop looking for more files.
						//
						break;
					}

					section = await reader.ReadNextSectionAsync();
				}
            }
            catch (Exception ex)
            {
                CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Caught error in UploadData handler", id.ToString(), ex);

                return Problem(ex.Message);
            }

            //
            // Treat the situation where we have a valid ID but no file content as a request to clear the data
            //
            if (foundFileData == false)
            {
                lock (brickPartPutSyncRoot)
                {
                    try
                    {
                        using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                        {
                            if (diskBasedBinaryStorageMode == true)
                            {
								DeleteDataFromDisk(brickPart.objectGuid, brickPart.versionNumber, "data");
                            }

                            brickPart.geometryFileName = null;
                            brickPart.geometryMimeType = null;
                            brickPart.geometrySize = 0;
                            brickPart.geometryData = null;
                            brickPart.versionNumber++;


                            //
                            // Now add the change history
                            //
                            BrickPartChangeHistory brickPartChangeHistory = new BrickPartChangeHistory();
                            brickPartChangeHistory.brickPartId = brickPart.id;
                            brickPartChangeHistory.versionNumber = brickPart.versionNumber;
                            brickPartChangeHistory.timeStamp = DateTime.UtcNow;
                            brickPartChangeHistory.userId = securityUser.id;
                                    brickPartChangeHistory.data = JsonSerializer.Serialize(Database.BrickPart.CreateAnonymousWithFirstLevelSubObjects(brickPart));
                            _context.BrickPartChangeHistories.Add(brickPartChangeHistory);

                            _context.SaveChanges();

                            transaction.Commit();

                            CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "BrickPart data cleared.", id.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "BrickPart data clear failed.", false, id.ToString(), "", "", ex);

                        return Problem(ex.Message);
                    }
                }
            }

            return Ok();
        }

        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/BrickPart/Data/{id:int}")]
        public async Task<IActionResult> DownloadDataAsync(int id, CancellationToken cancellationToken = default)
        {

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			using (BMCContext context = new BMCContext())
            {
                //
                // Return the data to the user as though it was a file.
                //
                Database.BrickPart brickPart = await (from d in context.BrickParts
                                                where d.id == id &&
                                                d.active == true &&
                                                d.deleted == false
                                                select d).FirstOrDefaultAsync();

                if (brickPart != null && brickPart.geometryData != null)
                {
                   return File(brickPart.geometryData.ToArray<byte>(), brickPart.geometryMimeType, brickPart.geometryFileName != null ? brickPart.geometryFileName.Trim() : "BrickPart_" + brickPart.id.ToString(), true);
                }
                else
                {
                    return BadRequest();
                }
            }
        }
	}
}
