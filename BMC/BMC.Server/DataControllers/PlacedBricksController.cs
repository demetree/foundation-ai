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
    /// This auto generated class provides the basic CRUD operations for the PlacedBrick entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the PlacedBrick entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class PlacedBricksController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object placedBrickPutSyncRoot = new object();
		static object placedBrickDeleteSyncRoot = new object();

		private BMCContext _context;

		private ILogger<PlacedBricksController> _logger;

		public PlacedBricksController(BMCContext context, ILogger<PlacedBricksController> logger) : base("BMC", "PlacedBrick")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of PlacedBricks filtered by the parameters provided.
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
		[Route("api/PlacedBricks")]
		public async Task<IActionResult> GetPlacedBricks(
			int? projectId = null,
			int? brickPartId = null,
			int? brickColourId = null,
			float? positionX = null,
			float? positionY = null,
			float? positionZ = null,
			float? rotationX = null,
			float? rotationY = null,
			float? rotationZ = null,
			float? rotationW = null,
			int? buildStepNumber = null,
			bool? isHidden = null,
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

			IQueryable<Database.PlacedBrick> query = (from pb in _context.PlacedBricks select pb);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (projectId.HasValue == true)
			{
				query = query.Where(pb => pb.projectId == projectId.Value);
			}
			if (brickPartId.HasValue == true)
			{
				query = query.Where(pb => pb.brickPartId == brickPartId.Value);
			}
			if (brickColourId.HasValue == true)
			{
				query = query.Where(pb => pb.brickColourId == brickColourId.Value);
			}
			if (positionX.HasValue == true)
			{
				query = query.Where(pb => pb.positionX == positionX.Value);
			}
			if (positionY.HasValue == true)
			{
				query = query.Where(pb => pb.positionY == positionY.Value);
			}
			if (positionZ.HasValue == true)
			{
				query = query.Where(pb => pb.positionZ == positionZ.Value);
			}
			if (rotationX.HasValue == true)
			{
				query = query.Where(pb => pb.rotationX == rotationX.Value);
			}
			if (rotationY.HasValue == true)
			{
				query = query.Where(pb => pb.rotationY == rotationY.Value);
			}
			if (rotationZ.HasValue == true)
			{
				query = query.Where(pb => pb.rotationZ == rotationZ.Value);
			}
			if (rotationW.HasValue == true)
			{
				query = query.Where(pb => pb.rotationW == rotationW.Value);
			}
			if (buildStepNumber.HasValue == true)
			{
				query = query.Where(pb => pb.buildStepNumber == buildStepNumber.Value);
			}
			if (isHidden.HasValue == true)
			{
				query = query.Where(pb => pb.isHidden == isHidden.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(pb => pb.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pb => pb.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pb => pb.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pb => pb.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pb => pb.deleted == false);
				}
			}
			else
			{
				query = query.Where(pb => pb.active == true);
				query = query.Where(pb => pb.deleted == false);
			}

			query = query.OrderBy(pb => pb.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.brickColour);
				query = query.Include(x => x.brickPart);
				query = query.Include(x => x.project);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Placed Brick, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       (includeRelations == true && x.brickColour.name.Contains(anyStringContains))
			       || (includeRelations == true && x.brickColour.hexRgb.Contains(anyStringContains))
			       || (includeRelations == true && x.brickColour.hexEdgeColour.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.name.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.ldrawPartId.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.ldrawTitle.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.ldrawCategory.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.keywords.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.author.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.geometryFilePath.Contains(anyStringContains))
			       || (includeRelations == true && x.project.name.Contains(anyStringContains))
			       || (includeRelations == true && x.project.description.Contains(anyStringContains))
			       || (includeRelations == true && x.project.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.project.thumbnailImagePath.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.PlacedBrick> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.PlacedBrick placedBrick in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(placedBrick, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.PlacedBrick Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.PlacedBrick Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of PlacedBricks filtered by the parameters provided.  Its query is similar to the GetPlacedBricks method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PlacedBricks/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? projectId = null,
			int? brickPartId = null,
			int? brickColourId = null,
			float? positionX = null,
			float? positionY = null,
			float? positionZ = null,
			float? rotationX = null,
			float? rotationY = null,
			float? rotationZ = null,
			float? rotationW = null,
			int? buildStepNumber = null,
			bool? isHidden = null,
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


			IQueryable<Database.PlacedBrick> query = (from pb in _context.PlacedBricks select pb);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (projectId.HasValue == true)
			{
				query = query.Where(pb => pb.projectId == projectId.Value);
			}
			if (brickPartId.HasValue == true)
			{
				query = query.Where(pb => pb.brickPartId == brickPartId.Value);
			}
			if (brickColourId.HasValue == true)
			{
				query = query.Where(pb => pb.brickColourId == brickColourId.Value);
			}
			if (positionX.HasValue == true)
			{
				query = query.Where(pb => pb.positionX == positionX.Value);
			}
			if (positionY.HasValue == true)
			{
				query = query.Where(pb => pb.positionY == positionY.Value);
			}
			if (positionZ.HasValue == true)
			{
				query = query.Where(pb => pb.positionZ == positionZ.Value);
			}
			if (rotationX.HasValue == true)
			{
				query = query.Where(pb => pb.rotationX == rotationX.Value);
			}
			if (rotationY.HasValue == true)
			{
				query = query.Where(pb => pb.rotationY == rotationY.Value);
			}
			if (rotationZ.HasValue == true)
			{
				query = query.Where(pb => pb.rotationZ == rotationZ.Value);
			}
			if (rotationW.HasValue == true)
			{
				query = query.Where(pb => pb.rotationW == rotationW.Value);
			}
			if (buildStepNumber.HasValue == true)
			{
				query = query.Where(pb => pb.buildStepNumber == buildStepNumber.Value);
			}
			if (isHidden.HasValue == true)
			{
				query = query.Where(pb => pb.isHidden == isHidden.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(pb => pb.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pb => pb.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pb => pb.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pb => pb.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pb => pb.deleted == false);
				}
			}
			else
			{
				query = query.Where(pb => pb.active == true);
				query = query.Where(pb => pb.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Placed Brick, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.brickColour.name.Contains(anyStringContains)
			       || x.brickColour.hexRgb.Contains(anyStringContains)
			       || x.brickColour.hexEdgeColour.Contains(anyStringContains)
			       || x.brickPart.name.Contains(anyStringContains)
			       || x.brickPart.ldrawPartId.Contains(anyStringContains)
			       || x.brickPart.ldrawTitle.Contains(anyStringContains)
			       || x.brickPart.ldrawCategory.Contains(anyStringContains)
			       || x.brickPart.keywords.Contains(anyStringContains)
			       || x.brickPart.author.Contains(anyStringContains)
			       || x.brickPart.geometryFilePath.Contains(anyStringContains)
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
        /// This gets a single PlacedBrick by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PlacedBrick/{id}")]
		public async Task<IActionResult> GetPlacedBrick(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.PlacedBrick> query = (from pb in _context.PlacedBricks where
							(pb.id == id) &&
							(userIsAdmin == true || pb.deleted == false) &&
							(userIsWriter == true || pb.active == true)
					select pb);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.brickColour);
					query = query.Include(x => x.brickPart);
					query = query.Include(x => x.project);
					query = query.AsSplitQuery();
				}

				Database.PlacedBrick materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.PlacedBrick Entity was read with Admin privilege." : "BMC.PlacedBrick Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PlacedBrick", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.PlacedBrick entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.PlacedBrick.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.PlacedBrick.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing PlacedBrick record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/PlacedBrick/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutPlacedBrick(int id, [FromBody]Database.PlacedBrick.PlacedBrickDTO placedBrickDTO, CancellationToken cancellationToken = default)
		{
			if (placedBrickDTO == null)
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



			if (id != placedBrickDTO.id)
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


			IQueryable<Database.PlacedBrick> query = (from x in _context.PlacedBricks
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.PlacedBrick existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.PlacedBrick PUT", id.ToString(), new Exception("No BMC.PlacedBrick entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (placedBrickDTO.objectGuid == Guid.Empty)
            {
                placedBrickDTO.objectGuid = existing.objectGuid;
            }
            else if (placedBrickDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a PlacedBrick record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.PlacedBrick cloneOfExisting = (Database.PlacedBrick)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new PlacedBrick object using the data from the existing record, updated with what is in the DTO.
			//
			Database.PlacedBrick placedBrick = (Database.PlacedBrick)_context.Entry(existing).GetDatabaseValues().ToObject();
			placedBrick.ApplyDTO(placedBrickDTO);
			//
			// The tenant guid for any PlacedBrick being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the PlacedBrick because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				placedBrick.tenantGuid = existing.tenantGuid;
			}

			lock (placedBrickPutSyncRoot)
			{
				//
				// Validate the version number for the placedBrick being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != placedBrick.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "PlacedBrick save attempt was made but save request was with version " + placedBrick.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The PlacedBrick you are trying to update has already changed.  Please try your save again after reloading the PlacedBrick.");
				}
				else
				{
					// Same record.  Increase version.
					placedBrick.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (placedBrick.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.PlacedBrick record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				try
				{
				    EntityEntry<Database.PlacedBrick> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(placedBrick);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        PlacedBrickChangeHistory placedBrickChangeHistory = new PlacedBrickChangeHistory();
				        placedBrickChangeHistory.placedBrickId = placedBrick.id;
				        placedBrickChangeHistory.versionNumber = placedBrick.versionNumber;
				        placedBrickChangeHistory.timeStamp = DateTime.UtcNow;
				        placedBrickChangeHistory.userId = securityUser.id;
				        placedBrickChangeHistory.tenantGuid = userTenantGuid;
				        placedBrickChangeHistory.data = JsonSerializer.Serialize(Database.PlacedBrick.CreateAnonymousWithFirstLevelSubObjects(placedBrick));
				        _context.PlacedBrickChangeHistories.Add(placedBrickChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.PlacedBrick entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.PlacedBrick.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.PlacedBrick.CreateAnonymousWithFirstLevelSubObjects(placedBrick)),
						null);

				return Ok(Database.PlacedBrick.CreateAnonymous(placedBrick));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.PlacedBrick entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.PlacedBrick.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.PlacedBrick.CreateAnonymousWithFirstLevelSubObjects(placedBrick)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new PlacedBrick record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PlacedBrick", Name = "PlacedBrick")]
		public async Task<IActionResult> PostPlacedBrick([FromBody]Database.PlacedBrick.PlacedBrickDTO placedBrickDTO, CancellationToken cancellationToken = default)
		{
			if (placedBrickDTO == null)
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
			// Create a new PlacedBrick object using the data from the DTO
			//
			Database.PlacedBrick placedBrick = Database.PlacedBrick.FromDTO(placedBrickDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				placedBrick.tenantGuid = userTenantGuid;

				placedBrick.objectGuid = Guid.NewGuid();
				placedBrick.versionNumber = 1;

				_context.PlacedBricks.Add(placedBrick);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the placedBrick object so that no further changes will be written to the database
				    //
				    _context.Entry(placedBrick).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					placedBrick.BuildStepAnnotations = null;
					placedBrick.BuildStepParts = null;
					placedBrick.PlacedBrickChangeHistories = null;
					placedBrick.SubmodelPlacedBricks = null;
					placedBrick.brickColour = null;
					placedBrick.brickPart = null;
					placedBrick.project = null;


				    PlacedBrickChangeHistory placedBrickChangeHistory = new PlacedBrickChangeHistory();
				    placedBrickChangeHistory.placedBrickId = placedBrick.id;
				    placedBrickChangeHistory.versionNumber = placedBrick.versionNumber;
				    placedBrickChangeHistory.timeStamp = DateTime.UtcNow;
				    placedBrickChangeHistory.userId = securityUser.id;
				    placedBrickChangeHistory.tenantGuid = userTenantGuid;
				    placedBrickChangeHistory.data = JsonSerializer.Serialize(Database.PlacedBrick.CreateAnonymousWithFirstLevelSubObjects(placedBrick));
				    _context.PlacedBrickChangeHistories.Add(placedBrickChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"BMC.PlacedBrick entity successfully created.",
						true,
						placedBrick. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.PlacedBrick.CreateAnonymousWithFirstLevelSubObjects(placedBrick)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.PlacedBrick entity creation failed.", false, placedBrick.id.ToString(), "", JsonSerializer.Serialize(Database.PlacedBrick.CreateAnonymousWithFirstLevelSubObjects(placedBrick)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PlacedBrick", placedBrick.id, placedBrick.id.ToString()));

			return CreatedAtRoute("PlacedBrick", new { id = placedBrick.id }, Database.PlacedBrick.CreateAnonymousWithFirstLevelSubObjects(placedBrick));
		}



        /// <summary>
        /// 
        /// This rolls a PlacedBrick entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PlacedBrick/Rollback/{id}")]
		[Route("api/PlacedBrick/Rollback")]
		public async Task<IActionResult> RollbackToPlacedBrickVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.PlacedBrick> query = (from x in _context.PlacedBricks
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this PlacedBrick concurrently
			//
			lock (placedBrickPutSyncRoot)
			{
				
				Database.PlacedBrick placedBrick = query.FirstOrDefault();
				
				if (placedBrick == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.PlacedBrick rollback", id.ToString(), new Exception("No BMC.PlacedBrick entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the PlacedBrick current state so we can log it.
				//
				Database.PlacedBrick cloneOfExisting = (Database.PlacedBrick)_context.Entry(placedBrick).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.BuildStepAnnotations = null;
				cloneOfExisting.BuildStepParts = null;
				cloneOfExisting.PlacedBrickChangeHistories = null;
				cloneOfExisting.SubmodelPlacedBricks = null;
				cloneOfExisting.brickColour = null;
				cloneOfExisting.brickPart = null;
				cloneOfExisting.project = null;

				if (versionNumber >= placedBrick.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for BMC.PlacedBrick rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for BMC.PlacedBrick rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				PlacedBrickChangeHistory placedBrickChangeHistory = (from x in _context.PlacedBrickChangeHistories
				                                               where
				                                               x.placedBrickId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (placedBrickChangeHistory != null)
				{
				    Database.PlacedBrick oldPlacedBrick = JsonSerializer.Deserialize<Database.PlacedBrick>(placedBrickChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    placedBrick.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    placedBrick.projectId = oldPlacedBrick.projectId;
				    placedBrick.brickPartId = oldPlacedBrick.brickPartId;
				    placedBrick.brickColourId = oldPlacedBrick.brickColourId;
				    placedBrick.positionX = oldPlacedBrick.positionX;
				    placedBrick.positionY = oldPlacedBrick.positionY;
				    placedBrick.positionZ = oldPlacedBrick.positionZ;
				    placedBrick.rotationX = oldPlacedBrick.rotationX;
				    placedBrick.rotationY = oldPlacedBrick.rotationY;
				    placedBrick.rotationZ = oldPlacedBrick.rotationZ;
				    placedBrick.rotationW = oldPlacedBrick.rotationW;
				    placedBrick.buildStepNumber = oldPlacedBrick.buildStepNumber;
				    placedBrick.isHidden = oldPlacedBrick.isHidden;
				    placedBrick.objectGuid = oldPlacedBrick.objectGuid;
				    placedBrick.active = oldPlacedBrick.active;
				    placedBrick.deleted = oldPlacedBrick.deleted;

				    string serializedPlacedBrick = JsonSerializer.Serialize(placedBrick);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        PlacedBrickChangeHistory newPlacedBrickChangeHistory = new PlacedBrickChangeHistory();
				        newPlacedBrickChangeHistory.placedBrickId = placedBrick.id;
				        newPlacedBrickChangeHistory.versionNumber = placedBrick.versionNumber;
				        newPlacedBrickChangeHistory.timeStamp = DateTime.UtcNow;
				        newPlacedBrickChangeHistory.userId = securityUser.id;
				        newPlacedBrickChangeHistory.tenantGuid = userTenantGuid;
				        newPlacedBrickChangeHistory.data = JsonSerializer.Serialize(Database.PlacedBrick.CreateAnonymousWithFirstLevelSubObjects(placedBrick));
				        _context.PlacedBrickChangeHistories.Add(newPlacedBrickChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.PlacedBrick rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.PlacedBrick.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.PlacedBrick.CreateAnonymousWithFirstLevelSubObjects(placedBrick)),
						null);


				    return Ok(Database.PlacedBrick.CreateAnonymous(placedBrick));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for BMC.PlacedBrick rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for BMC.PlacedBrick rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a PlacedBrick.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the PlacedBrick</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PlacedBrick/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetPlacedBrickChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.PlacedBrick placedBrick = await _context.PlacedBricks.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (placedBrick == null)
			{
				return NotFound();
			}

			try
			{
				placedBrick.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.PlacedBrick> versionInfo = await placedBrick.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a PlacedBrick.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the PlacedBrick</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PlacedBrick/{id}/AuditHistory")]
		public async Task<IActionResult> GetPlacedBrickAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.PlacedBrick placedBrick = await _context.PlacedBricks.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (placedBrick == null)
			{
				return NotFound();
			}

			try
			{
				placedBrick.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.PlacedBrick>> versions = await placedBrick.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a PlacedBrick.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the PlacedBrick</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The PlacedBrick object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PlacedBrick/{id}/Version/{version}")]
		public async Task<IActionResult> GetPlacedBrickVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.PlacedBrick placedBrick = await _context.PlacedBricks.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (placedBrick == null)
			{
				return NotFound();
			}

			try
			{
				placedBrick.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.PlacedBrick> versionInfo = await placedBrick.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a PlacedBrick at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the PlacedBrick</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The PlacedBrick object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PlacedBrick/{id}/StateAtTime")]
		public async Task<IActionResult> GetPlacedBrickStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.PlacedBrick placedBrick = await _context.PlacedBricks.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (placedBrick == null)
			{
				return NotFound();
			}

			try
			{
				placedBrick.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.PlacedBrick> versionInfo = await placedBrick.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a PlacedBrick record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PlacedBrick/{id}")]
		[Route("api/PlacedBrick")]
		public async Task<IActionResult> DeletePlacedBrick(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.PlacedBrick> query = (from x in _context.PlacedBricks
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.PlacedBrick placedBrick = await query.FirstOrDefaultAsync(cancellationToken);

			if (placedBrick == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.PlacedBrick DELETE", id.ToString(), new Exception("No BMC.PlacedBrick entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.PlacedBrick cloneOfExisting = (Database.PlacedBrick)_context.Entry(placedBrick).GetDatabaseValues().ToObject();


			lock (placedBrickDeleteSyncRoot)
			{
			    try
			    {
			        placedBrick.deleted = true;
			        placedBrick.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        PlacedBrickChangeHistory placedBrickChangeHistory = new PlacedBrickChangeHistory();
			        placedBrickChangeHistory.placedBrickId = placedBrick.id;
			        placedBrickChangeHistory.versionNumber = placedBrick.versionNumber;
			        placedBrickChangeHistory.timeStamp = DateTime.UtcNow;
			        placedBrickChangeHistory.userId = securityUser.id;
			        placedBrickChangeHistory.tenantGuid = userTenantGuid;
			        placedBrickChangeHistory.data = JsonSerializer.Serialize(Database.PlacedBrick.CreateAnonymousWithFirstLevelSubObjects(placedBrick));
			        _context.PlacedBrickChangeHistories.Add(placedBrickChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.PlacedBrick entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.PlacedBrick.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.PlacedBrick.CreateAnonymousWithFirstLevelSubObjects(placedBrick)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.PlacedBrick entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.PlacedBrick.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.PlacedBrick.CreateAnonymousWithFirstLevelSubObjects(placedBrick)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of PlacedBrick records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/PlacedBricks/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? projectId = null,
			int? brickPartId = null,
			int? brickColourId = null,
			float? positionX = null,
			float? positionY = null,
			float? positionZ = null,
			float? rotationX = null,
			float? rotationY = null,
			float? rotationZ = null,
			float? rotationW = null,
			int? buildStepNumber = null,
			bool? isHidden = null,
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

			IQueryable<Database.PlacedBrick> query = (from pb in _context.PlacedBricks select pb);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (projectId.HasValue == true)
			{
				query = query.Where(pb => pb.projectId == projectId.Value);
			}
			if (brickPartId.HasValue == true)
			{
				query = query.Where(pb => pb.brickPartId == brickPartId.Value);
			}
			if (brickColourId.HasValue == true)
			{
				query = query.Where(pb => pb.brickColourId == brickColourId.Value);
			}
			if (positionX.HasValue == true)
			{
				query = query.Where(pb => pb.positionX == positionX.Value);
			}
			if (positionY.HasValue == true)
			{
				query = query.Where(pb => pb.positionY == positionY.Value);
			}
			if (positionZ.HasValue == true)
			{
				query = query.Where(pb => pb.positionZ == positionZ.Value);
			}
			if (rotationX.HasValue == true)
			{
				query = query.Where(pb => pb.rotationX == rotationX.Value);
			}
			if (rotationY.HasValue == true)
			{
				query = query.Where(pb => pb.rotationY == rotationY.Value);
			}
			if (rotationZ.HasValue == true)
			{
				query = query.Where(pb => pb.rotationZ == rotationZ.Value);
			}
			if (rotationW.HasValue == true)
			{
				query = query.Where(pb => pb.rotationW == rotationW.Value);
			}
			if (buildStepNumber.HasValue == true)
			{
				query = query.Where(pb => pb.buildStepNumber == buildStepNumber.Value);
			}
			if (isHidden.HasValue == true)
			{
				query = query.Where(pb => pb.isHidden == isHidden.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(pb => pb.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pb => pb.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pb => pb.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pb => pb.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pb => pb.deleted == false);
				}
			}
			else
			{
				query = query.Where(pb => pb.active == true);
				query = query.Where(pb => pb.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Placed Brick, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.brickColour.name.Contains(anyStringContains)
			       || x.brickColour.hexRgb.Contains(anyStringContains)
			       || x.brickColour.hexEdgeColour.Contains(anyStringContains)
			       || x.brickPart.name.Contains(anyStringContains)
			       || x.brickPart.ldrawPartId.Contains(anyStringContains)
			       || x.brickPart.ldrawTitle.Contains(anyStringContains)
			       || x.brickPart.ldrawCategory.Contains(anyStringContains)
			       || x.brickPart.keywords.Contains(anyStringContains)
			       || x.brickPart.author.Contains(anyStringContains)
			       || x.brickPart.geometryFilePath.Contains(anyStringContains)
			       || x.project.name.Contains(anyStringContains)
			       || x.project.description.Contains(anyStringContains)
			       || x.project.notes.Contains(anyStringContains)
			       || x.project.thumbnailImagePath.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.PlacedBrick.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/PlacedBrick/CreateAuditEvent")]
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
