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
    /// This auto generated class provides the basic CRUD operations for the BrickPartRelationship entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the BrickPartRelationship entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class BrickPartRelationshipsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<BrickPartRelationshipsController> _logger;

		public BrickPartRelationshipsController(BMCContext context, ILogger<BrickPartRelationshipsController> logger) : base("BMC", "BrickPartRelationship")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of BrickPartRelationships filtered by the parameters provided.
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
		[Route("api/BrickPartRelationships")]
		public async Task<IActionResult> GetBrickPartRelationships(
			int? childBrickPartId = null,
			int? parentBrickPartId = null,
			string relationshipType = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
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

			IQueryable<Database.BrickPartRelationship> query = (from bpr in _context.BrickPartRelationships select bpr);
			if (childBrickPartId.HasValue == true)
			{
				query = query.Where(bpr => bpr.childBrickPartId == childBrickPartId.Value);
			}
			if (parentBrickPartId.HasValue == true)
			{
				query = query.Where(bpr => bpr.parentBrickPartId == parentBrickPartId.Value);
			}
			if (string.IsNullOrEmpty(relationshipType) == false)
			{
				query = query.Where(bpr => bpr.relationshipType == relationshipType);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bpr => bpr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bpr => bpr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bpr => bpr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bpr => bpr.deleted == false);
				}
			}
			else
			{
				query = query.Where(bpr => bpr.active == true);
				query = query.Where(bpr => bpr.deleted == false);
			}

			query = query.OrderBy(bpr => bpr.relationshipType);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.childBrickPart);
				query = query.Include(x => x.parentBrickPart);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Brick Part Relationship, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.relationshipType.Contains(anyStringContains)
			       || (includeRelations == true && x.childBrickPart.name.Contains(anyStringContains))
			       || (includeRelations == true && x.childBrickPart.ldrawPartId.Contains(anyStringContains))
			       || (includeRelations == true && x.childBrickPart.ldrawTitle.Contains(anyStringContains))
			       || (includeRelations == true && x.childBrickPart.ldrawCategory.Contains(anyStringContains))
			       || (includeRelations == true && x.childBrickPart.keywords.Contains(anyStringContains))
			       || (includeRelations == true && x.childBrickPart.author.Contains(anyStringContains))
			       || (includeRelations == true && x.childBrickPart.rebrickablePartNum.Contains(anyStringContains))
			       || (includeRelations == true && x.childBrickPart.geometryFilePath.Contains(anyStringContains))
			       || (includeRelations == true && x.parentBrickPart.name.Contains(anyStringContains))
			       || (includeRelations == true && x.parentBrickPart.ldrawPartId.Contains(anyStringContains))
			       || (includeRelations == true && x.parentBrickPart.ldrawTitle.Contains(anyStringContains))
			       || (includeRelations == true && x.parentBrickPart.ldrawCategory.Contains(anyStringContains))
			       || (includeRelations == true && x.parentBrickPart.keywords.Contains(anyStringContains))
			       || (includeRelations == true && x.parentBrickPart.author.Contains(anyStringContains))
			       || (includeRelations == true && x.parentBrickPart.rebrickablePartNum.Contains(anyStringContains))
			       || (includeRelations == true && x.parentBrickPart.geometryFilePath.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.BrickPartRelationship> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.BrickPartRelationship brickPartRelationship in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(brickPartRelationship, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.BrickPartRelationship Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.BrickPartRelationship Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of BrickPartRelationships filtered by the parameters provided.  Its query is similar to the GetBrickPartRelationships method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickPartRelationships/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? childBrickPartId = null,
			int? parentBrickPartId = null,
			string relationshipType = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.BrickPartRelationship> query = (from bpr in _context.BrickPartRelationships select bpr);
			if (childBrickPartId.HasValue == true)
			{
				query = query.Where(bpr => bpr.childBrickPartId == childBrickPartId.Value);
			}
			if (parentBrickPartId.HasValue == true)
			{
				query = query.Where(bpr => bpr.parentBrickPartId == parentBrickPartId.Value);
			}
			if (relationshipType != null)
			{
				query = query.Where(bpr => bpr.relationshipType == relationshipType);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bpr => bpr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bpr => bpr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bpr => bpr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bpr => bpr.deleted == false);
				}
			}
			else
			{
				query = query.Where(bpr => bpr.active == true);
				query = query.Where(bpr => bpr.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Brick Part Relationship, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.relationshipType.Contains(anyStringContains)
			       || x.childBrickPart.name.Contains(anyStringContains)
			       || x.childBrickPart.ldrawPartId.Contains(anyStringContains)
			       || x.childBrickPart.ldrawTitle.Contains(anyStringContains)
			       || x.childBrickPart.ldrawCategory.Contains(anyStringContains)
			       || x.childBrickPart.keywords.Contains(anyStringContains)
			       || x.childBrickPart.author.Contains(anyStringContains)
			       || x.childBrickPart.rebrickablePartNum.Contains(anyStringContains)
			       || x.childBrickPart.geometryFilePath.Contains(anyStringContains)
			       || x.parentBrickPart.name.Contains(anyStringContains)
			       || x.parentBrickPart.ldrawPartId.Contains(anyStringContains)
			       || x.parentBrickPart.ldrawTitle.Contains(anyStringContains)
			       || x.parentBrickPart.ldrawCategory.Contains(anyStringContains)
			       || x.parentBrickPart.keywords.Contains(anyStringContains)
			       || x.parentBrickPart.author.Contains(anyStringContains)
			       || x.parentBrickPart.rebrickablePartNum.Contains(anyStringContains)
			       || x.parentBrickPart.geometryFilePath.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single BrickPartRelationship by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickPartRelationship/{id}")]
		public async Task<IActionResult> GetBrickPartRelationship(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.BrickPartRelationship> query = (from bpr in _context.BrickPartRelationships where
							(bpr.id == id) &&
							(userIsAdmin == true || bpr.deleted == false) &&
							(userIsWriter == true || bpr.active == true)
					select bpr);

				if (includeRelations == true)
				{
					query = query.Include(x => x.childBrickPart);
					query = query.Include(x => x.parentBrickPart);
					query = query.AsSplitQuery();
				}

				Database.BrickPartRelationship materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.BrickPartRelationship Entity was read with Admin privilege." : "BMC.BrickPartRelationship Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickPartRelationship", materialized.id, materialized.relationshipType));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.BrickPartRelationship entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.BrickPartRelationship.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.BrickPartRelationship.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing BrickPartRelationship record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/BrickPartRelationship/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutBrickPartRelationship(int id, [FromBody]Database.BrickPartRelationship.BrickPartRelationshipDTO brickPartRelationshipDTO, CancellationToken cancellationToken = default)
		{
			if (brickPartRelationshipDTO == null)
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



			if (id != brickPartRelationshipDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.BrickPartRelationship> query = (from x in _context.BrickPartRelationships
				where
				(x.id == id)
				select x);


			Database.BrickPartRelationship existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickPartRelationship PUT", id.ToString(), new Exception("No BMC.BrickPartRelationship entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (brickPartRelationshipDTO.objectGuid == Guid.Empty)
            {
                brickPartRelationshipDTO.objectGuid = existing.objectGuid;
            }
            else if (brickPartRelationshipDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a BrickPartRelationship record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.BrickPartRelationship cloneOfExisting = (Database.BrickPartRelationship)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new BrickPartRelationship object using the data from the existing record, updated with what is in the DTO.
			//
			Database.BrickPartRelationship brickPartRelationship = (Database.BrickPartRelationship)_context.Entry(existing).GetDatabaseValues().ToObject();
			brickPartRelationship.ApplyDTO(brickPartRelationshipDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (brickPartRelationship.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.BrickPartRelationship record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (brickPartRelationship.relationshipType != null && brickPartRelationship.relationshipType.Length > 50)
			{
				brickPartRelationship.relationshipType = brickPartRelationship.relationshipType.Substring(0, 50);
			}

			EntityEntry<Database.BrickPartRelationship> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(brickPartRelationship);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.BrickPartRelationship entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickPartRelationship.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickPartRelationship.CreateAnonymousWithFirstLevelSubObjects(brickPartRelationship)),
					null);


				return Ok(Database.BrickPartRelationship.CreateAnonymous(brickPartRelationship));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.BrickPartRelationship entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickPartRelationship.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickPartRelationship.CreateAnonymousWithFirstLevelSubObjects(brickPartRelationship)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new BrickPartRelationship record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickPartRelationship", Name = "BrickPartRelationship")]
		public async Task<IActionResult> PostBrickPartRelationship([FromBody]Database.BrickPartRelationship.BrickPartRelationshipDTO brickPartRelationshipDTO, CancellationToken cancellationToken = default)
		{
			if (brickPartRelationshipDTO == null)
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

			//
			// Create a new BrickPartRelationship object using the data from the DTO
			//
			Database.BrickPartRelationship brickPartRelationship = Database.BrickPartRelationship.FromDTO(brickPartRelationshipDTO);

			try
			{
				if (brickPartRelationship.relationshipType != null && brickPartRelationship.relationshipType.Length > 50)
				{
					brickPartRelationship.relationshipType = brickPartRelationship.relationshipType.Substring(0, 50);
				}

				brickPartRelationship.objectGuid = Guid.NewGuid();
				_context.BrickPartRelationships.Add(brickPartRelationship);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.BrickPartRelationship entity successfully created.",
					true,
					brickPartRelationship.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.BrickPartRelationship.CreateAnonymousWithFirstLevelSubObjects(brickPartRelationship)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.BrickPartRelationship entity creation failed.", false, brickPartRelationship.id.ToString(), "", JsonSerializer.Serialize(brickPartRelationship), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickPartRelationship", brickPartRelationship.id, brickPartRelationship.relationshipType));

			return CreatedAtRoute("BrickPartRelationship", new { id = brickPartRelationship.id }, Database.BrickPartRelationship.CreateAnonymousWithFirstLevelSubObjects(brickPartRelationship));
		}



        /// <summary>
        /// 
        /// This deletes a BrickPartRelationship record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickPartRelationship/{id}")]
		[Route("api/BrickPartRelationship")]
		public async Task<IActionResult> DeleteBrickPartRelationship(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.BrickPartRelationship> query = (from x in _context.BrickPartRelationships
				where
				(x.id == id)
				select x);


			Database.BrickPartRelationship brickPartRelationship = await query.FirstOrDefaultAsync(cancellationToken);

			if (brickPartRelationship == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickPartRelationship DELETE", id.ToString(), new Exception("No BMC.BrickPartRelationship entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.BrickPartRelationship cloneOfExisting = (Database.BrickPartRelationship)_context.Entry(brickPartRelationship).GetDatabaseValues().ToObject();


			try
			{
				brickPartRelationship.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BrickPartRelationship entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickPartRelationship.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickPartRelationship.CreateAnonymousWithFirstLevelSubObjects(brickPartRelationship)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BrickPartRelationship entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickPartRelationship.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickPartRelationship.CreateAnonymousWithFirstLevelSubObjects(brickPartRelationship)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of BrickPartRelationship records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/BrickPartRelationships/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? childBrickPartId = null,
			int? parentBrickPartId = null,
			string relationshipType = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);


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

			IQueryable<Database.BrickPartRelationship> query = (from bpr in _context.BrickPartRelationships select bpr);
			if (childBrickPartId.HasValue == true)
			{
				query = query.Where(bpr => bpr.childBrickPartId == childBrickPartId.Value);
			}
			if (parentBrickPartId.HasValue == true)
			{
				query = query.Where(bpr => bpr.parentBrickPartId == parentBrickPartId.Value);
			}
			if (string.IsNullOrEmpty(relationshipType) == false)
			{
				query = query.Where(bpr => bpr.relationshipType == relationshipType);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bpr => bpr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bpr => bpr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bpr => bpr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bpr => bpr.deleted == false);
				}
			}
			else
			{
				query = query.Where(bpr => bpr.active == true);
				query = query.Where(bpr => bpr.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Brick Part Relationship, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.relationshipType.Contains(anyStringContains)
			       || x.childBrickPart.name.Contains(anyStringContains)
			       || x.childBrickPart.ldrawPartId.Contains(anyStringContains)
			       || x.childBrickPart.ldrawTitle.Contains(anyStringContains)
			       || x.childBrickPart.ldrawCategory.Contains(anyStringContains)
			       || x.childBrickPart.keywords.Contains(anyStringContains)
			       || x.childBrickPart.author.Contains(anyStringContains)
			       || x.childBrickPart.rebrickablePartNum.Contains(anyStringContains)
			       || x.childBrickPart.geometryFilePath.Contains(anyStringContains)
			       || x.parentBrickPart.name.Contains(anyStringContains)
			       || x.parentBrickPart.ldrawPartId.Contains(anyStringContains)
			       || x.parentBrickPart.ldrawTitle.Contains(anyStringContains)
			       || x.parentBrickPart.ldrawCategory.Contains(anyStringContains)
			       || x.parentBrickPart.keywords.Contains(anyStringContains)
			       || x.parentBrickPart.author.Contains(anyStringContains)
			       || x.parentBrickPart.rebrickablePartNum.Contains(anyStringContains)
			       || x.parentBrickPart.geometryFilePath.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.relationshipType);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.BrickPartRelationship.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/BrickPartRelationship/CreateAuditEvent")]
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
