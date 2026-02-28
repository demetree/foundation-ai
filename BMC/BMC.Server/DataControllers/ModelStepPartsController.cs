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
    /// This auto generated class provides the basic CRUD operations for the ModelStepPart entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ModelStepPart entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ModelStepPartsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		private BMCContext _context;

		private ILogger<ModelStepPartsController> _logger;

		public ModelStepPartsController(BMCContext context, ILogger<ModelStepPartsController> logger) : base("BMC", "ModelStepPart")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ModelStepParts filtered by the parameters provided.
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
		[Route("api/ModelStepParts")]
		public async Task<IActionResult> GetModelStepParts(
			int? modelBuildStepId = null,
			int? brickPartId = null,
			int? brickColourId = null,
			string partFileName = null,
			int? colorCode = null,
			float? positionX = null,
			float? positionY = null,
			float? positionZ = null,
			string transformMatrix = null,
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

			IQueryable<Database.ModelStepPart> query = (from msp in _context.ModelStepParts select msp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (modelBuildStepId.HasValue == true)
			{
				query = query.Where(msp => msp.modelBuildStepId == modelBuildStepId.Value);
			}
			if (brickPartId.HasValue == true)
			{
				query = query.Where(msp => msp.brickPartId == brickPartId.Value);
			}
			if (brickColourId.HasValue == true)
			{
				query = query.Where(msp => msp.brickColourId == brickColourId.Value);
			}
			if (string.IsNullOrEmpty(partFileName) == false)
			{
				query = query.Where(msp => msp.partFileName == partFileName);
			}
			if (colorCode.HasValue == true)
			{
				query = query.Where(msp => msp.colorCode == colorCode.Value);
			}
			if (positionX.HasValue == true)
			{
				query = query.Where(msp => msp.positionX == positionX.Value);
			}
			if (positionY.HasValue == true)
			{
				query = query.Where(msp => msp.positionY == positionY.Value);
			}
			if (positionZ.HasValue == true)
			{
				query = query.Where(msp => msp.positionZ == positionZ.Value);
			}
			if (string.IsNullOrEmpty(transformMatrix) == false)
			{
				query = query.Where(msp => msp.transformMatrix == transformMatrix);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(msp => msp.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(msp => msp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(msp => msp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(msp => msp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(msp => msp.deleted == false);
				}
			}
			else
			{
				query = query.Where(msp => msp.active == true);
				query = query.Where(msp => msp.deleted == false);
			}

			query = query.OrderBy(msp => msp.sequence).ThenBy(msp => msp.partFileName).ThenBy(msp => msp.transformMatrix);


			//
			// Add the any string contains parameter to span all the string fields on the Model Step Part, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.partFileName.Contains(anyStringContains)
			       || x.transformMatrix.Contains(anyStringContains)
			       || (includeRelations == true && x.brickColour.name.Contains(anyStringContains))
			       || (includeRelations == true && x.brickColour.hexRgb.Contains(anyStringContains))
			       || (includeRelations == true && x.brickColour.hexEdgeColour.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.name.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.rebrickablePartNum.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.rebrickablePartUrl.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.rebrickableImgUrl.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.ldrawPartId.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.bricklinkId.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.brickowlId.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.legoDesignId.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.ldrawTitle.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.ldrawCategory.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.keywords.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.author.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.materialType.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.geometryFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.geometryMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.geometryFileFormat.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.geometryOriginalFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.modelBuildStep.rotationType.Contains(anyStringContains))
			       || (includeRelations == true && x.modelBuildStep.description.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.brickColour);
				query = query.Include(x => x.brickPart);
				query = query.Include(x => x.modelBuildStep);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.ModelStepPart> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ModelStepPart modelStepPart in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(modelStepPart, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.ModelStepPart Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.ModelStepPart Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ModelStepParts filtered by the parameters provided.  Its query is similar to the GetModelStepParts method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModelStepParts/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? modelBuildStepId = null,
			int? brickPartId = null,
			int? brickColourId = null,
			string partFileName = null,
			int? colorCode = null,
			float? positionX = null,
			float? positionY = null,
			float? positionZ = null,
			string transformMatrix = null,
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


			IQueryable<Database.ModelStepPart> query = (from msp in _context.ModelStepParts select msp);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (modelBuildStepId.HasValue == true)
			{
				query = query.Where(msp => msp.modelBuildStepId == modelBuildStepId.Value);
			}
			if (brickPartId.HasValue == true)
			{
				query = query.Where(msp => msp.brickPartId == brickPartId.Value);
			}
			if (brickColourId.HasValue == true)
			{
				query = query.Where(msp => msp.brickColourId == brickColourId.Value);
			}
			if (partFileName != null)
			{
				query = query.Where(msp => msp.partFileName == partFileName);
			}
			if (colorCode.HasValue == true)
			{
				query = query.Where(msp => msp.colorCode == colorCode.Value);
			}
			if (positionX.HasValue == true)
			{
				query = query.Where(msp => msp.positionX == positionX.Value);
			}
			if (positionY.HasValue == true)
			{
				query = query.Where(msp => msp.positionY == positionY.Value);
			}
			if (positionZ.HasValue == true)
			{
				query = query.Where(msp => msp.positionZ == positionZ.Value);
			}
			if (transformMatrix != null)
			{
				query = query.Where(msp => msp.transformMatrix == transformMatrix);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(msp => msp.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(msp => msp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(msp => msp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(msp => msp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(msp => msp.deleted == false);
				}
			}
			else
			{
				query = query.Where(msp => msp.active == true);
				query = query.Where(msp => msp.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Model Step Part, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.partFileName.Contains(anyStringContains)
			       || x.transformMatrix.Contains(anyStringContains)
			       || x.brickColour.name.Contains(anyStringContains)
			       || x.brickColour.hexRgb.Contains(anyStringContains)
			       || x.brickColour.hexEdgeColour.Contains(anyStringContains)
			       || x.brickPart.name.Contains(anyStringContains)
			       || x.brickPart.rebrickablePartNum.Contains(anyStringContains)
			       || x.brickPart.rebrickablePartUrl.Contains(anyStringContains)
			       || x.brickPart.rebrickableImgUrl.Contains(anyStringContains)
			       || x.brickPart.ldrawPartId.Contains(anyStringContains)
			       || x.brickPart.bricklinkId.Contains(anyStringContains)
			       || x.brickPart.brickowlId.Contains(anyStringContains)
			       || x.brickPart.legoDesignId.Contains(anyStringContains)
			       || x.brickPart.ldrawTitle.Contains(anyStringContains)
			       || x.brickPart.ldrawCategory.Contains(anyStringContains)
			       || x.brickPart.keywords.Contains(anyStringContains)
			       || x.brickPart.author.Contains(anyStringContains)
			       || x.brickPart.materialType.Contains(anyStringContains)
			       || x.brickPart.geometryFileName.Contains(anyStringContains)
			       || x.brickPart.geometryMimeType.Contains(anyStringContains)
			       || x.brickPart.geometryFileFormat.Contains(anyStringContains)
			       || x.brickPart.geometryOriginalFileName.Contains(anyStringContains)
			       || x.modelBuildStep.rotationType.Contains(anyStringContains)
			       || x.modelBuildStep.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ModelStepPart by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModelStepPart/{id}")]
		public async Task<IActionResult> GetModelStepPart(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ModelStepPart> query = (from msp in _context.ModelStepParts where
							(msp.id == id) &&
							(userIsAdmin == true || msp.deleted == false) &&
							(userIsWriter == true || msp.active == true)
					select msp);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.brickColour);
					query = query.Include(x => x.brickPart);
					query = query.Include(x => x.modelBuildStep);
					query = query.AsSplitQuery();
				}

				Database.ModelStepPart materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.ModelStepPart Entity was read with Admin privilege." : "BMC.ModelStepPart Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ModelStepPart", materialized.id, materialized.partFileName));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.ModelStepPart entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.ModelStepPart.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.ModelStepPart.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ModelStepPart record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ModelStepPart/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutModelStepPart(int id, [FromBody]Database.ModelStepPart.ModelStepPartDTO modelStepPartDTO, CancellationToken cancellationToken = default)
		{
			if (modelStepPartDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != modelStepPartDTO.id)
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


			IQueryable<Database.ModelStepPart> query = (from x in _context.ModelStepParts
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ModelStepPart existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ModelStepPart PUT", id.ToString(), new Exception("No BMC.ModelStepPart entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (modelStepPartDTO.objectGuid == Guid.Empty)
            {
                modelStepPartDTO.objectGuid = existing.objectGuid;
            }
            else if (modelStepPartDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ModelStepPart record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ModelStepPart cloneOfExisting = (Database.ModelStepPart)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ModelStepPart object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ModelStepPart modelStepPart = (Database.ModelStepPart)_context.Entry(existing).GetDatabaseValues().ToObject();
			modelStepPart.ApplyDTO(modelStepPartDTO);
			//
			// The tenant guid for any ModelStepPart being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ModelStepPart because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				modelStepPart.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (modelStepPart.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.ModelStepPart record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (modelStepPart.partFileName != null && modelStepPart.partFileName.Length > 250)
			{
				modelStepPart.partFileName = modelStepPart.partFileName.Substring(0, 250);
			}

			if (modelStepPart.transformMatrix != null && modelStepPart.transformMatrix.Length > 500)
			{
				modelStepPart.transformMatrix = modelStepPart.transformMatrix.Substring(0, 500);
			}

			EntityEntry<Database.ModelStepPart> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(modelStepPart);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.ModelStepPart entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ModelStepPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ModelStepPart.CreateAnonymousWithFirstLevelSubObjects(modelStepPart)),
					null);


				return Ok(Database.ModelStepPart.CreateAnonymous(modelStepPart));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.ModelStepPart entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ModelStepPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ModelStepPart.CreateAnonymousWithFirstLevelSubObjects(modelStepPart)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new ModelStepPart record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModelStepPart", Name = "ModelStepPart")]
		public async Task<IActionResult> PostModelStepPart([FromBody]Database.ModelStepPart.ModelStepPartDTO modelStepPartDTO, CancellationToken cancellationToken = default)
		{
			if (modelStepPartDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
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
			// Create a new ModelStepPart object using the data from the DTO
			//
			Database.ModelStepPart modelStepPart = Database.ModelStepPart.FromDTO(modelStepPartDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				modelStepPart.tenantGuid = userTenantGuid;

				if (modelStepPart.partFileName != null && modelStepPart.partFileName.Length > 250)
				{
					modelStepPart.partFileName = modelStepPart.partFileName.Substring(0, 250);
				}

				if (modelStepPart.transformMatrix != null && modelStepPart.transformMatrix.Length > 500)
				{
					modelStepPart.transformMatrix = modelStepPart.transformMatrix.Substring(0, 500);
				}

				modelStepPart.objectGuid = Guid.NewGuid();
				_context.ModelStepParts.Add(modelStepPart);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.ModelStepPart entity successfully created.",
					true,
					modelStepPart.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ModelStepPart.CreateAnonymousWithFirstLevelSubObjects(modelStepPart)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.ModelStepPart entity creation failed.", false, modelStepPart.id.ToString(), "", JsonSerializer.Serialize(modelStepPart), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ModelStepPart", modelStepPart.id, modelStepPart.partFileName));

			return CreatedAtRoute("ModelStepPart", new { id = modelStepPart.id }, Database.ModelStepPart.CreateAnonymousWithFirstLevelSubObjects(modelStepPart));
		}



        /// <summary>
        /// 
        /// This deletes a ModelStepPart record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModelStepPart/{id}")]
		[Route("api/ModelStepPart")]
		public async Task<IActionResult> DeleteModelStepPart(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
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

			IQueryable<Database.ModelStepPart> query = (from x in _context.ModelStepParts
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ModelStepPart modelStepPart = await query.FirstOrDefaultAsync(cancellationToken);

			if (modelStepPart == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ModelStepPart DELETE", id.ToString(), new Exception("No BMC.ModelStepPart entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ModelStepPart cloneOfExisting = (Database.ModelStepPart)_context.Entry(modelStepPart).GetDatabaseValues().ToObject();


			try
			{
				modelStepPart.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ModelStepPart entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ModelStepPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ModelStepPart.CreateAnonymousWithFirstLevelSubObjects(modelStepPart)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ModelStepPart entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ModelStepPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ModelStepPart.CreateAnonymousWithFirstLevelSubObjects(modelStepPart)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of ModelStepPart records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ModelStepParts/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? modelBuildStepId = null,
			int? brickPartId = null,
			int? brickColourId = null,
			string partFileName = null,
			int? colorCode = null,
			float? positionX = null,
			float? positionY = null,
			float? positionZ = null,
			string transformMatrix = null,
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

			IQueryable<Database.ModelStepPart> query = (from msp in _context.ModelStepParts select msp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (modelBuildStepId.HasValue == true)
			{
				query = query.Where(msp => msp.modelBuildStepId == modelBuildStepId.Value);
			}
			if (brickPartId.HasValue == true)
			{
				query = query.Where(msp => msp.brickPartId == brickPartId.Value);
			}
			if (brickColourId.HasValue == true)
			{
				query = query.Where(msp => msp.brickColourId == brickColourId.Value);
			}
			if (string.IsNullOrEmpty(partFileName) == false)
			{
				query = query.Where(msp => msp.partFileName == partFileName);
			}
			if (colorCode.HasValue == true)
			{
				query = query.Where(msp => msp.colorCode == colorCode.Value);
			}
			if (positionX.HasValue == true)
			{
				query = query.Where(msp => msp.positionX == positionX.Value);
			}
			if (positionY.HasValue == true)
			{
				query = query.Where(msp => msp.positionY == positionY.Value);
			}
			if (positionZ.HasValue == true)
			{
				query = query.Where(msp => msp.positionZ == positionZ.Value);
			}
			if (string.IsNullOrEmpty(transformMatrix) == false)
			{
				query = query.Where(msp => msp.transformMatrix == transformMatrix);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(msp => msp.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(msp => msp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(msp => msp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(msp => msp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(msp => msp.deleted == false);
				}
			}
			else
			{
				query = query.Where(msp => msp.active == true);
				query = query.Where(msp => msp.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Model Step Part, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.partFileName.Contains(anyStringContains)
			       || x.transformMatrix.Contains(anyStringContains)
			       || x.brickColour.name.Contains(anyStringContains)
			       || x.brickColour.hexRgb.Contains(anyStringContains)
			       || x.brickColour.hexEdgeColour.Contains(anyStringContains)
			       || x.brickPart.name.Contains(anyStringContains)
			       || x.brickPart.rebrickablePartNum.Contains(anyStringContains)
			       || x.brickPart.rebrickablePartUrl.Contains(anyStringContains)
			       || x.brickPart.rebrickableImgUrl.Contains(anyStringContains)
			       || x.brickPart.ldrawPartId.Contains(anyStringContains)
			       || x.brickPart.bricklinkId.Contains(anyStringContains)
			       || x.brickPart.brickowlId.Contains(anyStringContains)
			       || x.brickPart.legoDesignId.Contains(anyStringContains)
			       || x.brickPart.ldrawTitle.Contains(anyStringContains)
			       || x.brickPart.ldrawCategory.Contains(anyStringContains)
			       || x.brickPart.keywords.Contains(anyStringContains)
			       || x.brickPart.author.Contains(anyStringContains)
			       || x.brickPart.materialType.Contains(anyStringContains)
			       || x.brickPart.geometryFileName.Contains(anyStringContains)
			       || x.brickPart.geometryMimeType.Contains(anyStringContains)
			       || x.brickPart.geometryFileFormat.Contains(anyStringContains)
			       || x.brickPart.geometryOriginalFileName.Contains(anyStringContains)
			       || x.modelBuildStep.rotationType.Contains(anyStringContains)
			       || x.modelBuildStep.description.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.partFileName).ThenBy(x => x.transformMatrix);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ModelStepPart.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ModelStepPart/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
