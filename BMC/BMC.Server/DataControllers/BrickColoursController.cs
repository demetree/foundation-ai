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
    /// This auto generated class provides the basic CRUD operations for the BrickColour entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the BrickColour entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class BrickColoursController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<BrickColoursController> _logger;

		public BrickColoursController(BMCContext context, ILogger<BrickColoursController> logger) : base("BMC", "BrickColour")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of BrickColours filtered by the parameters provided.
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
		[Route("api/BrickColours")]
		public async Task<IActionResult> GetBrickColours(
			string name = null,
			int? ldrawColourCode = null,
			string hexRgb = null,
			string hexEdgeColour = null,
			int? alpha = null,
			bool? isTransparent = null,
			bool? isMetallic = null,
			int? colourFinishId = null,
			int? luminance = null,
			int? legoColourId = null,
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

			IQueryable<Database.BrickColour> query = (from bc in _context.BrickColours select bc);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(bc => bc.name == name);
			}
			if (ldrawColourCode.HasValue == true)
			{
				query = query.Where(bc => bc.ldrawColourCode == ldrawColourCode.Value);
			}
			if (string.IsNullOrEmpty(hexRgb) == false)
			{
				query = query.Where(bc => bc.hexRgb == hexRgb);
			}
			if (string.IsNullOrEmpty(hexEdgeColour) == false)
			{
				query = query.Where(bc => bc.hexEdgeColour == hexEdgeColour);
			}
			if (alpha.HasValue == true)
			{
				query = query.Where(bc => bc.alpha == alpha.Value);
			}
			if (isTransparent.HasValue == true)
			{
				query = query.Where(bc => bc.isTransparent == isTransparent.Value);
			}
			if (isMetallic.HasValue == true)
			{
				query = query.Where(bc => bc.isMetallic == isMetallic.Value);
			}
			if (colourFinishId.HasValue == true)
			{
				query = query.Where(bc => bc.colourFinishId == colourFinishId.Value);
			}
			if (luminance.HasValue == true)
			{
				query = query.Where(bc => bc.luminance == luminance.Value);
			}
			if (legoColourId.HasValue == true)
			{
				query = query.Where(bc => bc.legoColourId == legoColourId.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(bc => bc.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bc => bc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bc => bc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bc => bc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bc => bc.deleted == false);
				}
			}
			else
			{
				query = query.Where(bc => bc.active == true);
				query = query.Where(bc => bc.deleted == false);
			}

			query = query.OrderBy(bc => bc.sequence).ThenBy(bc => bc.name).ThenBy(bc => bc.hexRgb).ThenBy(bc => bc.hexEdgeColour);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.colourFinish);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Brick Colour, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.hexRgb.Contains(anyStringContains)
			       || x.hexEdgeColour.Contains(anyStringContains)
			       || (includeRelations == true && x.colourFinish.name.Contains(anyStringContains))
			       || (includeRelations == true && x.colourFinish.description.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.BrickColour> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.BrickColour brickColour in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(brickColour, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.BrickColour Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.BrickColour Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of BrickColours filtered by the parameters provided.  Its query is similar to the GetBrickColours method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickColours/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			int? ldrawColourCode = null,
			string hexRgb = null,
			string hexEdgeColour = null,
			int? alpha = null,
			bool? isTransparent = null,
			bool? isMetallic = null,
			int? colourFinishId = null,
			int? luminance = null,
			int? legoColourId = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.BrickColour> query = (from bc in _context.BrickColours select bc);
			if (name != null)
			{
				query = query.Where(bc => bc.name == name);
			}
			if (ldrawColourCode.HasValue == true)
			{
				query = query.Where(bc => bc.ldrawColourCode == ldrawColourCode.Value);
			}
			if (hexRgb != null)
			{
				query = query.Where(bc => bc.hexRgb == hexRgb);
			}
			if (hexEdgeColour != null)
			{
				query = query.Where(bc => bc.hexEdgeColour == hexEdgeColour);
			}
			if (alpha.HasValue == true)
			{
				query = query.Where(bc => bc.alpha == alpha.Value);
			}
			if (isTransparent.HasValue == true)
			{
				query = query.Where(bc => bc.isTransparent == isTransparent.Value);
			}
			if (isMetallic.HasValue == true)
			{
				query = query.Where(bc => bc.isMetallic == isMetallic.Value);
			}
			if (colourFinishId.HasValue == true)
			{
				query = query.Where(bc => bc.colourFinishId == colourFinishId.Value);
			}
			if (luminance.HasValue == true)
			{
				query = query.Where(bc => bc.luminance == luminance.Value);
			}
			if (legoColourId.HasValue == true)
			{
				query = query.Where(bc => bc.legoColourId == legoColourId.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(bc => bc.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bc => bc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bc => bc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bc => bc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bc => bc.deleted == false);
				}
			}
			else
			{
				query = query.Where(bc => bc.active == true);
				query = query.Where(bc => bc.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Brick Colour, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.hexRgb.Contains(anyStringContains)
			       || x.hexEdgeColour.Contains(anyStringContains)
			       || x.colourFinish.name.Contains(anyStringContains)
			       || x.colourFinish.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single BrickColour by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickColour/{id}")]
		public async Task<IActionResult> GetBrickColour(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.BrickColour> query = (from bc in _context.BrickColours where
							(bc.id == id) &&
							(userIsAdmin == true || bc.deleted == false) &&
							(userIsWriter == true || bc.active == true)
					select bc);

				if (includeRelations == true)
				{
					query = query.Include(x => x.colourFinish);
					query = query.AsSplitQuery();
				}

				Database.BrickColour materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.BrickColour Entity was read with Admin privilege." : "BMC.BrickColour Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickColour", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.BrickColour entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.BrickColour.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.BrickColour.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing BrickColour record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/BrickColour/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutBrickColour(int id, [FromBody]Database.BrickColour.BrickColourDTO brickColourDTO, CancellationToken cancellationToken = default)
		{
			if (brickColourDTO == null)
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



			if (id != brickColourDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.BrickColour> query = (from x in _context.BrickColours
				where
				(x.id == id)
				select x);


			Database.BrickColour existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickColour PUT", id.ToString(), new Exception("No BMC.BrickColour entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (brickColourDTO.objectGuid == Guid.Empty)
            {
                brickColourDTO.objectGuid = existing.objectGuid;
            }
            else if (brickColourDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a BrickColour record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.BrickColour cloneOfExisting = (Database.BrickColour)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new BrickColour object using the data from the existing record, updated with what is in the DTO.
			//
			Database.BrickColour brickColour = (Database.BrickColour)_context.Entry(existing).GetDatabaseValues().ToObject();
			brickColour.ApplyDTO(brickColourDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (brickColour.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.BrickColour record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (brickColour.name != null && brickColour.name.Length > 100)
			{
				brickColour.name = brickColour.name.Substring(0, 100);
			}

			if (brickColour.hexRgb != null && brickColour.hexRgb.Length > 10)
			{
				brickColour.hexRgb = brickColour.hexRgb.Substring(0, 10);
			}

			if (brickColour.hexEdgeColour != null && brickColour.hexEdgeColour.Length > 10)
			{
				brickColour.hexEdgeColour = brickColour.hexEdgeColour.Substring(0, 10);
			}

			EntityEntry<Database.BrickColour> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(brickColour);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.BrickColour entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickColour.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickColour.CreateAnonymousWithFirstLevelSubObjects(brickColour)),
					null);


				return Ok(Database.BrickColour.CreateAnonymous(brickColour));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.BrickColour entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickColour.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickColour.CreateAnonymousWithFirstLevelSubObjects(brickColour)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new BrickColour record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickColour", Name = "BrickColour")]
		public async Task<IActionResult> PostBrickColour([FromBody]Database.BrickColour.BrickColourDTO brickColourDTO, CancellationToken cancellationToken = default)
		{
			if (brickColourDTO == null)
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
			// Create a new BrickColour object using the data from the DTO
			//
			Database.BrickColour brickColour = Database.BrickColour.FromDTO(brickColourDTO);

			try
			{
				if (brickColour.name != null && brickColour.name.Length > 100)
				{
					brickColour.name = brickColour.name.Substring(0, 100);
				}

				if (brickColour.hexRgb != null && brickColour.hexRgb.Length > 10)
				{
					brickColour.hexRgb = brickColour.hexRgb.Substring(0, 10);
				}

				if (brickColour.hexEdgeColour != null && brickColour.hexEdgeColour.Length > 10)
				{
					brickColour.hexEdgeColour = brickColour.hexEdgeColour.Substring(0, 10);
				}

				brickColour.objectGuid = Guid.NewGuid();
				_context.BrickColours.Add(brickColour);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.BrickColour entity successfully created.",
					true,
					brickColour.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.BrickColour.CreateAnonymousWithFirstLevelSubObjects(brickColour)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.BrickColour entity creation failed.", false, brickColour.id.ToString(), "", JsonSerializer.Serialize(brickColour), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickColour", brickColour.id, brickColour.name));

			return CreatedAtRoute("BrickColour", new { id = brickColour.id }, Database.BrickColour.CreateAnonymousWithFirstLevelSubObjects(brickColour));
		}



        /// <summary>
        /// 
        /// This deletes a BrickColour record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickColour/{id}")]
		[Route("api/BrickColour")]
		public async Task<IActionResult> DeleteBrickColour(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.BrickColour> query = (from x in _context.BrickColours
				where
				(x.id == id)
				select x);


			Database.BrickColour brickColour = await query.FirstOrDefaultAsync(cancellationToken);

			if (brickColour == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickColour DELETE", id.ToString(), new Exception("No BMC.BrickColour entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.BrickColour cloneOfExisting = (Database.BrickColour)_context.Entry(brickColour).GetDatabaseValues().ToObject();


			try
			{
				brickColour.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BrickColour entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickColour.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickColour.CreateAnonymousWithFirstLevelSubObjects(brickColour)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BrickColour entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickColour.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickColour.CreateAnonymousWithFirstLevelSubObjects(brickColour)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of BrickColour records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/BrickColours/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			int? ldrawColourCode = null,
			string hexRgb = null,
			string hexEdgeColour = null,
			int? alpha = null,
			bool? isTransparent = null,
			bool? isMetallic = null,
			int? colourFinishId = null,
			int? luminance = null,
			int? legoColourId = null,
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

			IQueryable<Database.BrickColour> query = (from bc in _context.BrickColours select bc);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(bc => bc.name == name);
			}
			if (ldrawColourCode.HasValue == true)
			{
				query = query.Where(bc => bc.ldrawColourCode == ldrawColourCode.Value);
			}
			if (string.IsNullOrEmpty(hexRgb) == false)
			{
				query = query.Where(bc => bc.hexRgb == hexRgb);
			}
			if (string.IsNullOrEmpty(hexEdgeColour) == false)
			{
				query = query.Where(bc => bc.hexEdgeColour == hexEdgeColour);
			}
			if (alpha.HasValue == true)
			{
				query = query.Where(bc => bc.alpha == alpha.Value);
			}
			if (isTransparent.HasValue == true)
			{
				query = query.Where(bc => bc.isTransparent == isTransparent.Value);
			}
			if (isMetallic.HasValue == true)
			{
				query = query.Where(bc => bc.isMetallic == isMetallic.Value);
			}
			if (colourFinishId.HasValue == true)
			{
				query = query.Where(bc => bc.colourFinishId == colourFinishId.Value);
			}
			if (luminance.HasValue == true)
			{
				query = query.Where(bc => bc.luminance == luminance.Value);
			}
			if (legoColourId.HasValue == true)
			{
				query = query.Where(bc => bc.legoColourId == legoColourId.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(bc => bc.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bc => bc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bc => bc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bc => bc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bc => bc.deleted == false);
				}
			}
			else
			{
				query = query.Where(bc => bc.active == true);
				query = query.Where(bc => bc.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Brick Colour, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.hexRgb.Contains(anyStringContains)
			       || x.hexEdgeColour.Contains(anyStringContains)
			       || x.colourFinish.name.Contains(anyStringContains)
			       || x.colourFinish.description.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.hexRgb).ThenBy(x => x.hexEdgeColour);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.BrickColour.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/BrickColour/CreateAuditEvent")]
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
