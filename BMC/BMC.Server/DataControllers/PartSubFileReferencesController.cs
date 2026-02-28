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

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the PartSubFileReference entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the PartSubFileReference entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class PartSubFileReferencesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private BMCContext _context;

		private ILogger<PartSubFileReferencesController> _logger;

		public PartSubFileReferencesController(BMCContext context, ILogger<PartSubFileReferencesController> logger) : base("BMC", "PartSubFileReference")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of PartSubFileReferences filtered by the parameters provided.
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
		[Route("api/PartSubFileReferences")]
		public async Task<IActionResult> GetPartSubFileReferences(
			int? parentBrickPartId = null,
			int? referencedBrickPartId = null,
			string referencedFileName = null,
			string transformMatrix = null,
			int? colorCode = null,
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

			IQueryable<Database.PartSubFileReference> query = (from psfr in _context.PartSubFileReferences select psfr);
			if (parentBrickPartId.HasValue == true)
			{
				query = query.Where(psfr => psfr.parentBrickPartId == parentBrickPartId.Value);
			}
			if (referencedBrickPartId.HasValue == true)
			{
				query = query.Where(psfr => psfr.referencedBrickPartId == referencedBrickPartId.Value);
			}
			if (string.IsNullOrEmpty(referencedFileName) == false)
			{
				query = query.Where(psfr => psfr.referencedFileName == referencedFileName);
			}
			if (string.IsNullOrEmpty(transformMatrix) == false)
			{
				query = query.Where(psfr => psfr.transformMatrix == transformMatrix);
			}
			if (colorCode.HasValue == true)
			{
				query = query.Where(psfr => psfr.colorCode == colorCode.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(psfr => psfr.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(psfr => psfr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(psfr => psfr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(psfr => psfr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(psfr => psfr.deleted == false);
				}
			}
			else
			{
				query = query.Where(psfr => psfr.active == true);
				query = query.Where(psfr => psfr.deleted == false);
			}

			query = query.OrderBy(psfr => psfr.sequence).ThenBy(psfr => psfr.referencedFileName).ThenBy(psfr => psfr.transformMatrix);


			//
			// Add the any string contains parameter to span all the string fields on the Part Sub File Reference, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.referencedFileName.Contains(anyStringContains)
			       || x.transformMatrix.Contains(anyStringContains)
			       || (includeRelations == true && x.parentBrickPart.name.Contains(anyStringContains))
			       || (includeRelations == true && x.parentBrickPart.rebrickablePartNum.Contains(anyStringContains))
			       || (includeRelations == true && x.parentBrickPart.rebrickablePartUrl.Contains(anyStringContains))
			       || (includeRelations == true && x.parentBrickPart.rebrickableImgUrl.Contains(anyStringContains))
			       || (includeRelations == true && x.parentBrickPart.ldrawPartId.Contains(anyStringContains))
			       || (includeRelations == true && x.parentBrickPart.bricklinkId.Contains(anyStringContains))
			       || (includeRelations == true && x.parentBrickPart.brickowlId.Contains(anyStringContains))
			       || (includeRelations == true && x.parentBrickPart.legoDesignId.Contains(anyStringContains))
			       || (includeRelations == true && x.parentBrickPart.ldrawTitle.Contains(anyStringContains))
			       || (includeRelations == true && x.parentBrickPart.ldrawCategory.Contains(anyStringContains))
			       || (includeRelations == true && x.parentBrickPart.keywords.Contains(anyStringContains))
			       || (includeRelations == true && x.parentBrickPart.author.Contains(anyStringContains))
			       || (includeRelations == true && x.parentBrickPart.materialType.Contains(anyStringContains))
			       || (includeRelations == true && x.parentBrickPart.geometryFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.parentBrickPart.geometryMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.parentBrickPart.geometryFileFormat.Contains(anyStringContains))
			       || (includeRelations == true && x.parentBrickPart.geometryOriginalFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.referencedBrickPart.name.Contains(anyStringContains))
			       || (includeRelations == true && x.referencedBrickPart.rebrickablePartNum.Contains(anyStringContains))
			       || (includeRelations == true && x.referencedBrickPart.rebrickablePartUrl.Contains(anyStringContains))
			       || (includeRelations == true && x.referencedBrickPart.rebrickableImgUrl.Contains(anyStringContains))
			       || (includeRelations == true && x.referencedBrickPart.ldrawPartId.Contains(anyStringContains))
			       || (includeRelations == true && x.referencedBrickPart.bricklinkId.Contains(anyStringContains))
			       || (includeRelations == true && x.referencedBrickPart.brickowlId.Contains(anyStringContains))
			       || (includeRelations == true && x.referencedBrickPart.legoDesignId.Contains(anyStringContains))
			       || (includeRelations == true && x.referencedBrickPart.ldrawTitle.Contains(anyStringContains))
			       || (includeRelations == true && x.referencedBrickPart.ldrawCategory.Contains(anyStringContains))
			       || (includeRelations == true && x.referencedBrickPart.keywords.Contains(anyStringContains))
			       || (includeRelations == true && x.referencedBrickPart.author.Contains(anyStringContains))
			       || (includeRelations == true && x.referencedBrickPart.materialType.Contains(anyStringContains))
			       || (includeRelations == true && x.referencedBrickPart.geometryFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.referencedBrickPart.geometryMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.referencedBrickPart.geometryFileFormat.Contains(anyStringContains))
			       || (includeRelations == true && x.referencedBrickPart.geometryOriginalFileName.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.parentBrickPart);
				query = query.Include(x => x.referencedBrickPart);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.PartSubFileReference> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.PartSubFileReference partSubFileReference in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(partSubFileReference, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.PartSubFileReference Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.PartSubFileReference Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of PartSubFileReferences filtered by the parameters provided.  Its query is similar to the GetPartSubFileReferences method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PartSubFileReferences/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? parentBrickPartId = null,
			int? referencedBrickPartId = null,
			string referencedFileName = null,
			string transformMatrix = null,
			int? colorCode = null,
			int? sequence = null,
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

			IQueryable<Database.PartSubFileReference> query = (from psfr in _context.PartSubFileReferences select psfr);
			if (parentBrickPartId.HasValue == true)
			{
				query = query.Where(psfr => psfr.parentBrickPartId == parentBrickPartId.Value);
			}
			if (referencedBrickPartId.HasValue == true)
			{
				query = query.Where(psfr => psfr.referencedBrickPartId == referencedBrickPartId.Value);
			}
			if (referencedFileName != null)
			{
				query = query.Where(psfr => psfr.referencedFileName == referencedFileName);
			}
			if (transformMatrix != null)
			{
				query = query.Where(psfr => psfr.transformMatrix == transformMatrix);
			}
			if (colorCode.HasValue == true)
			{
				query = query.Where(psfr => psfr.colorCode == colorCode.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(psfr => psfr.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(psfr => psfr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(psfr => psfr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(psfr => psfr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(psfr => psfr.deleted == false);
				}
			}
			else
			{
				query = query.Where(psfr => psfr.active == true);
				query = query.Where(psfr => psfr.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Part Sub File Reference, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.referencedFileName.Contains(anyStringContains)
			       || x.transformMatrix.Contains(anyStringContains)
			       || x.parentBrickPart.name.Contains(anyStringContains)
			       || x.parentBrickPart.rebrickablePartNum.Contains(anyStringContains)
			       || x.parentBrickPart.rebrickablePartUrl.Contains(anyStringContains)
			       || x.parentBrickPart.rebrickableImgUrl.Contains(anyStringContains)
			       || x.parentBrickPart.ldrawPartId.Contains(anyStringContains)
			       || x.parentBrickPart.bricklinkId.Contains(anyStringContains)
			       || x.parentBrickPart.brickowlId.Contains(anyStringContains)
			       || x.parentBrickPart.legoDesignId.Contains(anyStringContains)
			       || x.parentBrickPart.ldrawTitle.Contains(anyStringContains)
			       || x.parentBrickPart.ldrawCategory.Contains(anyStringContains)
			       || x.parentBrickPart.keywords.Contains(anyStringContains)
			       || x.parentBrickPart.author.Contains(anyStringContains)
			       || x.parentBrickPart.materialType.Contains(anyStringContains)
			       || x.parentBrickPart.geometryFileName.Contains(anyStringContains)
			       || x.parentBrickPart.geometryMimeType.Contains(anyStringContains)
			       || x.parentBrickPart.geometryFileFormat.Contains(anyStringContains)
			       || x.parentBrickPart.geometryOriginalFileName.Contains(anyStringContains)
			       || x.referencedBrickPart.name.Contains(anyStringContains)
			       || x.referencedBrickPart.rebrickablePartNum.Contains(anyStringContains)
			       || x.referencedBrickPart.rebrickablePartUrl.Contains(anyStringContains)
			       || x.referencedBrickPart.rebrickableImgUrl.Contains(anyStringContains)
			       || x.referencedBrickPart.ldrawPartId.Contains(anyStringContains)
			       || x.referencedBrickPart.bricklinkId.Contains(anyStringContains)
			       || x.referencedBrickPart.brickowlId.Contains(anyStringContains)
			       || x.referencedBrickPart.legoDesignId.Contains(anyStringContains)
			       || x.referencedBrickPart.ldrawTitle.Contains(anyStringContains)
			       || x.referencedBrickPart.ldrawCategory.Contains(anyStringContains)
			       || x.referencedBrickPart.keywords.Contains(anyStringContains)
			       || x.referencedBrickPart.author.Contains(anyStringContains)
			       || x.referencedBrickPart.materialType.Contains(anyStringContains)
			       || x.referencedBrickPart.geometryFileName.Contains(anyStringContains)
			       || x.referencedBrickPart.geometryMimeType.Contains(anyStringContains)
			       || x.referencedBrickPart.geometryFileFormat.Contains(anyStringContains)
			       || x.referencedBrickPart.geometryOriginalFileName.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single PartSubFileReference by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PartSubFileReference/{id}")]
		public async Task<IActionResult> GetPartSubFileReference(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.PartSubFileReference> query = (from psfr in _context.PartSubFileReferences where
							(psfr.id == id) &&
							(userIsAdmin == true || psfr.deleted == false) &&
							(userIsWriter == true || psfr.active == true)
					select psfr);

				if (includeRelations == true)
				{
					query = query.Include(x => x.parentBrickPart);
					query = query.Include(x => x.referencedBrickPart);
					query = query.AsSplitQuery();
				}

				Database.PartSubFileReference materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.PartSubFileReference Entity was read with Admin privilege." : "BMC.PartSubFileReference Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PartSubFileReference", materialized.id, materialized.referencedFileName));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.PartSubFileReference entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.PartSubFileReference.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.PartSubFileReference.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing PartSubFileReference record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/PartSubFileReference/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutPartSubFileReference(int id, [FromBody]Database.PartSubFileReference.PartSubFileReferenceDTO partSubFileReferenceDTO, CancellationToken cancellationToken = default)
		{
			if (partSubFileReferenceDTO == null)
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



			if (id != partSubFileReferenceDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.PartSubFileReference> query = (from x in _context.PartSubFileReferences
				where
				(x.id == id)
				select x);


			Database.PartSubFileReference existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.PartSubFileReference PUT", id.ToString(), new Exception("No BMC.PartSubFileReference entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (partSubFileReferenceDTO.objectGuid == Guid.Empty)
            {
                partSubFileReferenceDTO.objectGuid = existing.objectGuid;
            }
            else if (partSubFileReferenceDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a PartSubFileReference record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.PartSubFileReference cloneOfExisting = (Database.PartSubFileReference)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new PartSubFileReference object using the data from the existing record, updated with what is in the DTO.
			//
			Database.PartSubFileReference partSubFileReference = (Database.PartSubFileReference)_context.Entry(existing).GetDatabaseValues().ToObject();
			partSubFileReference.ApplyDTO(partSubFileReferenceDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (partSubFileReference.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.PartSubFileReference record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (partSubFileReference.referencedFileName != null && partSubFileReference.referencedFileName.Length > 250)
			{
				partSubFileReference.referencedFileName = partSubFileReference.referencedFileName.Substring(0, 250);
			}

			if (partSubFileReference.transformMatrix != null && partSubFileReference.transformMatrix.Length > 500)
			{
				partSubFileReference.transformMatrix = partSubFileReference.transformMatrix.Substring(0, 500);
			}

			EntityEntry<Database.PartSubFileReference> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(partSubFileReference);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.PartSubFileReference entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.PartSubFileReference.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PartSubFileReference.CreateAnonymousWithFirstLevelSubObjects(partSubFileReference)),
					null);


				return Ok(Database.PartSubFileReference.CreateAnonymous(partSubFileReference));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.PartSubFileReference entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.PartSubFileReference.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PartSubFileReference.CreateAnonymousWithFirstLevelSubObjects(partSubFileReference)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new PartSubFileReference record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PartSubFileReference", Name = "PartSubFileReference")]
		public async Task<IActionResult> PostPartSubFileReference([FromBody]Database.PartSubFileReference.PartSubFileReferenceDTO partSubFileReferenceDTO, CancellationToken cancellationToken = default)
		{
			if (partSubFileReferenceDTO == null)
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
			// Create a new PartSubFileReference object using the data from the DTO
			//
			Database.PartSubFileReference partSubFileReference = Database.PartSubFileReference.FromDTO(partSubFileReferenceDTO);

			try
			{
				if (partSubFileReference.referencedFileName != null && partSubFileReference.referencedFileName.Length > 250)
				{
					partSubFileReference.referencedFileName = partSubFileReference.referencedFileName.Substring(0, 250);
				}

				if (partSubFileReference.transformMatrix != null && partSubFileReference.transformMatrix.Length > 500)
				{
					partSubFileReference.transformMatrix = partSubFileReference.transformMatrix.Substring(0, 500);
				}

				partSubFileReference.objectGuid = Guid.NewGuid();
				_context.PartSubFileReferences.Add(partSubFileReference);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.PartSubFileReference entity successfully created.",
					true,
					partSubFileReference.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.PartSubFileReference.CreateAnonymousWithFirstLevelSubObjects(partSubFileReference)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.PartSubFileReference entity creation failed.", false, partSubFileReference.id.ToString(), "", JsonSerializer.Serialize(partSubFileReference), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PartSubFileReference", partSubFileReference.id, partSubFileReference.referencedFileName));

			return CreatedAtRoute("PartSubFileReference", new { id = partSubFileReference.id }, Database.PartSubFileReference.CreateAnonymousWithFirstLevelSubObjects(partSubFileReference));
		}



        /// <summary>
        /// 
        /// This deletes a PartSubFileReference record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PartSubFileReference/{id}")]
		[Route("api/PartSubFileReference")]
		public async Task<IActionResult> DeletePartSubFileReference(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.PartSubFileReference> query = (from x in _context.PartSubFileReferences
				where
				(x.id == id)
				select x);


			Database.PartSubFileReference partSubFileReference = await query.FirstOrDefaultAsync(cancellationToken);

			if (partSubFileReference == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.PartSubFileReference DELETE", id.ToString(), new Exception("No BMC.PartSubFileReference entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.PartSubFileReference cloneOfExisting = (Database.PartSubFileReference)_context.Entry(partSubFileReference).GetDatabaseValues().ToObject();


			try
			{
				partSubFileReference.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.PartSubFileReference entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.PartSubFileReference.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PartSubFileReference.CreateAnonymousWithFirstLevelSubObjects(partSubFileReference)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.PartSubFileReference entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.PartSubFileReference.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PartSubFileReference.CreateAnonymousWithFirstLevelSubObjects(partSubFileReference)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of PartSubFileReference records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/PartSubFileReferences/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? parentBrickPartId = null,
			int? referencedBrickPartId = null,
			string referencedFileName = null,
			string transformMatrix = null,
			int? colorCode = null,
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

			IQueryable<Database.PartSubFileReference> query = (from psfr in _context.PartSubFileReferences select psfr);
			if (parentBrickPartId.HasValue == true)
			{
				query = query.Where(psfr => psfr.parentBrickPartId == parentBrickPartId.Value);
			}
			if (referencedBrickPartId.HasValue == true)
			{
				query = query.Where(psfr => psfr.referencedBrickPartId == referencedBrickPartId.Value);
			}
			if (string.IsNullOrEmpty(referencedFileName) == false)
			{
				query = query.Where(psfr => psfr.referencedFileName == referencedFileName);
			}
			if (string.IsNullOrEmpty(transformMatrix) == false)
			{
				query = query.Where(psfr => psfr.transformMatrix == transformMatrix);
			}
			if (colorCode.HasValue == true)
			{
				query = query.Where(psfr => psfr.colorCode == colorCode.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(psfr => psfr.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(psfr => psfr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(psfr => psfr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(psfr => psfr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(psfr => psfr.deleted == false);
				}
			}
			else
			{
				query = query.Where(psfr => psfr.active == true);
				query = query.Where(psfr => psfr.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Part Sub File Reference, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.referencedFileName.Contains(anyStringContains)
			       || x.transformMatrix.Contains(anyStringContains)
			       || x.parentBrickPart.name.Contains(anyStringContains)
			       || x.parentBrickPart.rebrickablePartNum.Contains(anyStringContains)
			       || x.parentBrickPart.rebrickablePartUrl.Contains(anyStringContains)
			       || x.parentBrickPart.rebrickableImgUrl.Contains(anyStringContains)
			       || x.parentBrickPart.ldrawPartId.Contains(anyStringContains)
			       || x.parentBrickPart.bricklinkId.Contains(anyStringContains)
			       || x.parentBrickPart.brickowlId.Contains(anyStringContains)
			       || x.parentBrickPart.legoDesignId.Contains(anyStringContains)
			       || x.parentBrickPart.ldrawTitle.Contains(anyStringContains)
			       || x.parentBrickPart.ldrawCategory.Contains(anyStringContains)
			       || x.parentBrickPart.keywords.Contains(anyStringContains)
			       || x.parentBrickPart.author.Contains(anyStringContains)
			       || x.parentBrickPart.materialType.Contains(anyStringContains)
			       || x.parentBrickPart.geometryFileName.Contains(anyStringContains)
			       || x.parentBrickPart.geometryMimeType.Contains(anyStringContains)
			       || x.parentBrickPart.geometryFileFormat.Contains(anyStringContains)
			       || x.parentBrickPart.geometryOriginalFileName.Contains(anyStringContains)
			       || x.referencedBrickPart.name.Contains(anyStringContains)
			       || x.referencedBrickPart.rebrickablePartNum.Contains(anyStringContains)
			       || x.referencedBrickPart.rebrickablePartUrl.Contains(anyStringContains)
			       || x.referencedBrickPart.rebrickableImgUrl.Contains(anyStringContains)
			       || x.referencedBrickPart.ldrawPartId.Contains(anyStringContains)
			       || x.referencedBrickPart.bricklinkId.Contains(anyStringContains)
			       || x.referencedBrickPart.brickowlId.Contains(anyStringContains)
			       || x.referencedBrickPart.legoDesignId.Contains(anyStringContains)
			       || x.referencedBrickPart.ldrawTitle.Contains(anyStringContains)
			       || x.referencedBrickPart.ldrawCategory.Contains(anyStringContains)
			       || x.referencedBrickPart.keywords.Contains(anyStringContains)
			       || x.referencedBrickPart.author.Contains(anyStringContains)
			       || x.referencedBrickPart.materialType.Contains(anyStringContains)
			       || x.referencedBrickPart.geometryFileName.Contains(anyStringContains)
			       || x.referencedBrickPart.geometryMimeType.Contains(anyStringContains)
			       || x.referencedBrickPart.geometryFileFormat.Contains(anyStringContains)
			       || x.referencedBrickPart.geometryOriginalFileName.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.referencedFileName).ThenBy(x => x.transformMatrix);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.PartSubFileReference.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/PartSubFileReference/CreateAuditEvent")]
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


	}
}
