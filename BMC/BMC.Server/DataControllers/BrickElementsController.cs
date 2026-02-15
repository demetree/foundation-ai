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
    /// This auto generated class provides the basic CRUD operations for the BrickElement entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the BrickElement entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class BrickElementsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<BrickElementsController> _logger;

		public BrickElementsController(BMCContext context, ILogger<BrickElementsController> logger) : base("BMC", "BrickElement")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of BrickElements filtered by the parameters provided.
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
		[Route("api/BrickElements")]
		public async Task<IActionResult> GetBrickElements(
			string elementId = null,
			int? brickPartId = null,
			int? brickColourId = null,
			string designId = null,
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

			IQueryable<Database.BrickElement> query = (from be in _context.BrickElements select be);
			if (string.IsNullOrEmpty(elementId) == false)
			{
				query = query.Where(be => be.elementId == elementId);
			}
			if (brickPartId.HasValue == true)
			{
				query = query.Where(be => be.brickPartId == brickPartId.Value);
			}
			if (brickColourId.HasValue == true)
			{
				query = query.Where(be => be.brickColourId == brickColourId.Value);
			}
			if (string.IsNullOrEmpty(designId) == false)
			{
				query = query.Where(be => be.designId == designId);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(be => be.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(be => be.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(be => be.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(be => be.deleted == false);
				}
			}
			else
			{
				query = query.Where(be => be.active == true);
				query = query.Where(be => be.deleted == false);
			}

			query = query.OrderBy(be => be.elementId).ThenBy(be => be.designId);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.brickColour);
				query = query.Include(x => x.brickPart);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Brick Element, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.elementId.Contains(anyStringContains)
			       || x.designId.Contains(anyStringContains)
			       || (includeRelations == true && x.brickColour.name.Contains(anyStringContains))
			       || (includeRelations == true && x.brickColour.hexRgb.Contains(anyStringContains))
			       || (includeRelations == true && x.brickColour.hexEdgeColour.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.name.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.ldrawPartId.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.ldrawTitle.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.ldrawCategory.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.keywords.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.author.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.rebrickablePartNum.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.geometryFilePath.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.BrickElement> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.BrickElement brickElement in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(brickElement, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.BrickElement Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.BrickElement Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of BrickElements filtered by the parameters provided.  Its query is similar to the GetBrickElements method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickElements/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string elementId = null,
			int? brickPartId = null,
			int? brickColourId = null,
			string designId = null,
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

			IQueryable<Database.BrickElement> query = (from be in _context.BrickElements select be);
			if (elementId != null)
			{
				query = query.Where(be => be.elementId == elementId);
			}
			if (brickPartId.HasValue == true)
			{
				query = query.Where(be => be.brickPartId == brickPartId.Value);
			}
			if (brickColourId.HasValue == true)
			{
				query = query.Where(be => be.brickColourId == brickColourId.Value);
			}
			if (designId != null)
			{
				query = query.Where(be => be.designId == designId);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(be => be.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(be => be.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(be => be.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(be => be.deleted == false);
				}
			}
			else
			{
				query = query.Where(be => be.active == true);
				query = query.Where(be => be.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Brick Element, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.elementId.Contains(anyStringContains)
			       || x.designId.Contains(anyStringContains)
			       || x.brickColour.name.Contains(anyStringContains)
			       || x.brickColour.hexRgb.Contains(anyStringContains)
			       || x.brickColour.hexEdgeColour.Contains(anyStringContains)
			       || x.brickPart.name.Contains(anyStringContains)
			       || x.brickPart.ldrawPartId.Contains(anyStringContains)
			       || x.brickPart.ldrawTitle.Contains(anyStringContains)
			       || x.brickPart.ldrawCategory.Contains(anyStringContains)
			       || x.brickPart.keywords.Contains(anyStringContains)
			       || x.brickPart.author.Contains(anyStringContains)
			       || x.brickPart.rebrickablePartNum.Contains(anyStringContains)
			       || x.brickPart.geometryFilePath.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single BrickElement by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickElement/{id}")]
		public async Task<IActionResult> GetBrickElement(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.BrickElement> query = (from be in _context.BrickElements where
							(be.id == id) &&
							(userIsAdmin == true || be.deleted == false) &&
							(userIsWriter == true || be.active == true)
					select be);

				if (includeRelations == true)
				{
					query = query.Include(x => x.brickColour);
					query = query.Include(x => x.brickPart);
					query = query.AsSplitQuery();
				}

				Database.BrickElement materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.BrickElement Entity was read with Admin privilege." : "BMC.BrickElement Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickElement", materialized.id, materialized.elementId));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.BrickElement entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.BrickElement.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.BrickElement.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing BrickElement record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/BrickElement/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutBrickElement(int id, [FromBody]Database.BrickElement.BrickElementDTO brickElementDTO, CancellationToken cancellationToken = default)
		{
			if (brickElementDTO == null)
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



			if (id != brickElementDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.BrickElement> query = (from x in _context.BrickElements
				where
				(x.id == id)
				select x);


			Database.BrickElement existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickElement PUT", id.ToString(), new Exception("No BMC.BrickElement entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (brickElementDTO.objectGuid == Guid.Empty)
            {
                brickElementDTO.objectGuid = existing.objectGuid;
            }
            else if (brickElementDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a BrickElement record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.BrickElement cloneOfExisting = (Database.BrickElement)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new BrickElement object using the data from the existing record, updated with what is in the DTO.
			//
			Database.BrickElement brickElement = (Database.BrickElement)_context.Entry(existing).GetDatabaseValues().ToObject();
			brickElement.ApplyDTO(brickElementDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (brickElement.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.BrickElement record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (brickElement.elementId != null && brickElement.elementId.Length > 50)
			{
				brickElement.elementId = brickElement.elementId.Substring(0, 50);
			}

			if (brickElement.designId != null && brickElement.designId.Length > 50)
			{
				brickElement.designId = brickElement.designId.Substring(0, 50);
			}

			EntityEntry<Database.BrickElement> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(brickElement);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.BrickElement entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickElement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickElement.CreateAnonymousWithFirstLevelSubObjects(brickElement)),
					null);


				return Ok(Database.BrickElement.CreateAnonymous(brickElement));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.BrickElement entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickElement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickElement.CreateAnonymousWithFirstLevelSubObjects(brickElement)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new BrickElement record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickElement", Name = "BrickElement")]
		public async Task<IActionResult> PostBrickElement([FromBody]Database.BrickElement.BrickElementDTO brickElementDTO, CancellationToken cancellationToken = default)
		{
			if (brickElementDTO == null)
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
			// Create a new BrickElement object using the data from the DTO
			//
			Database.BrickElement brickElement = Database.BrickElement.FromDTO(brickElementDTO);

			try
			{
				if (brickElement.elementId != null && brickElement.elementId.Length > 50)
				{
					brickElement.elementId = brickElement.elementId.Substring(0, 50);
				}

				if (brickElement.designId != null && brickElement.designId.Length > 50)
				{
					brickElement.designId = brickElement.designId.Substring(0, 50);
				}

				brickElement.objectGuid = Guid.NewGuid();
				_context.BrickElements.Add(brickElement);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.BrickElement entity successfully created.",
					true,
					brickElement.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.BrickElement.CreateAnonymousWithFirstLevelSubObjects(brickElement)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.BrickElement entity creation failed.", false, brickElement.id.ToString(), "", JsonSerializer.Serialize(brickElement), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickElement", brickElement.id, brickElement.elementId));

			return CreatedAtRoute("BrickElement", new { id = brickElement.id }, Database.BrickElement.CreateAnonymousWithFirstLevelSubObjects(brickElement));
		}



        /// <summary>
        /// 
        /// This deletes a BrickElement record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickElement/{id}")]
		[Route("api/BrickElement")]
		public async Task<IActionResult> DeleteBrickElement(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.BrickElement> query = (from x in _context.BrickElements
				where
				(x.id == id)
				select x);


			Database.BrickElement brickElement = await query.FirstOrDefaultAsync(cancellationToken);

			if (brickElement == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickElement DELETE", id.ToString(), new Exception("No BMC.BrickElement entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.BrickElement cloneOfExisting = (Database.BrickElement)_context.Entry(brickElement).GetDatabaseValues().ToObject();


			try
			{
				brickElement.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BrickElement entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickElement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickElement.CreateAnonymousWithFirstLevelSubObjects(brickElement)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BrickElement entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickElement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickElement.CreateAnonymousWithFirstLevelSubObjects(brickElement)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of BrickElement records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/BrickElements/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string elementId = null,
			int? brickPartId = null,
			int? brickColourId = null,
			string designId = null,
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

			IQueryable<Database.BrickElement> query = (from be in _context.BrickElements select be);
			if (string.IsNullOrEmpty(elementId) == false)
			{
				query = query.Where(be => be.elementId == elementId);
			}
			if (brickPartId.HasValue == true)
			{
				query = query.Where(be => be.brickPartId == brickPartId.Value);
			}
			if (brickColourId.HasValue == true)
			{
				query = query.Where(be => be.brickColourId == brickColourId.Value);
			}
			if (string.IsNullOrEmpty(designId) == false)
			{
				query = query.Where(be => be.designId == designId);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(be => be.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(be => be.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(be => be.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(be => be.deleted == false);
				}
			}
			else
			{
				query = query.Where(be => be.active == true);
				query = query.Where(be => be.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Brick Element, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.elementId.Contains(anyStringContains)
			       || x.designId.Contains(anyStringContains)
			       || x.brickColour.name.Contains(anyStringContains)
			       || x.brickColour.hexRgb.Contains(anyStringContains)
			       || x.brickColour.hexEdgeColour.Contains(anyStringContains)
			       || x.brickPart.name.Contains(anyStringContains)
			       || x.brickPart.ldrawPartId.Contains(anyStringContains)
			       || x.brickPart.ldrawTitle.Contains(anyStringContains)
			       || x.brickPart.ldrawCategory.Contains(anyStringContains)
			       || x.brickPart.keywords.Contains(anyStringContains)
			       || x.brickPart.author.Contains(anyStringContains)
			       || x.brickPart.rebrickablePartNum.Contains(anyStringContains)
			       || x.brickPart.geometryFilePath.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.elementId).ThenBy(x => x.designId);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.BrickElement.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/BrickElement/CreateAuditEvent")]
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
