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
    /// This auto generated class provides the basic CRUD operations for the BrickPartColour entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the BrickPartColour entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class BrickPartColoursController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private BMCContext _context;

		private ILogger<BrickPartColoursController> _logger;

		public BrickPartColoursController(BMCContext context, ILogger<BrickPartColoursController> logger) : base("BMC", "BrickPartColour")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of BrickPartColours filtered by the parameters provided.
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
		[Route("api/BrickPartColours")]
		public async Task<IActionResult> GetBrickPartColours(
			int? brickPartId = null,
			int? brickColourId = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			int? pageSize = null,
			int? pageNumber = null,
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

			IQueryable<Database.BrickPartColour> query = (from bpc in _context.BrickPartColours select bpc);
			if (brickPartId.HasValue == true)
			{
				query = query.Where(bpc => bpc.brickPartId == brickPartId.Value);
			}
			if (brickColourId.HasValue == true)
			{
				query = query.Where(bpc => bpc.brickColourId == brickColourId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bpc => bpc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bpc => bpc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bpc => bpc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bpc => bpc.deleted == false);
				}
			}
			else
			{
				query = query.Where(bpc => bpc.active == true);
				query = query.Where(bpc => bpc.deleted == false);
			}

			query = query.OrderBy(bpc => bpc.id);

			if (includeRelations == true)
			{
				query = query.Include(x => x.brickColour);
				query = query.Include(x => x.brickPart);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.BrickPartColour> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.BrickPartColour brickPartColour in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(brickPartColour, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.BrickPartColour Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.BrickPartColour Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of BrickPartColours filtered by the parameters provided.  Its query is similar to the GetBrickPartColours method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickPartColours/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? brickPartId = null,
			int? brickColourId = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
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

			IQueryable<Database.BrickPartColour> query = (from bpc in _context.BrickPartColours select bpc);
			if (brickPartId.HasValue == true)
			{
				query = query.Where(bpc => bpc.brickPartId == brickPartId.Value);
			}
			if (brickColourId.HasValue == true)
			{
				query = query.Where(bpc => bpc.brickColourId == brickColourId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bpc => bpc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bpc => bpc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bpc => bpc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bpc => bpc.deleted == false);
				}
			}
			else
			{
				query = query.Where(bpc => bpc.active == true);
				query = query.Where(bpc => bpc.deleted == false);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single BrickPartColour by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickPartColour/{id}")]
		public async Task<IActionResult> GetBrickPartColour(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.BrickPartColour> query = (from bpc in _context.BrickPartColours where
							(bpc.id == id) &&
							(userIsAdmin == true || bpc.deleted == false) &&
							(userIsWriter == true || bpc.active == true)
					select bpc);

				if (includeRelations == true)
				{
					query = query.Include(x => x.brickColour);
					query = query.Include(x => x.brickPart);
					query = query.AsSplitQuery();
				}

				Database.BrickPartColour materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.BrickPartColour Entity was read with Admin privilege." : "BMC.BrickPartColour Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickPartColour", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.BrickPartColour entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.BrickPartColour.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.BrickPartColour.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing BrickPartColour record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/BrickPartColour/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutBrickPartColour(int id, [FromBody]Database.BrickPartColour.BrickPartColourDTO brickPartColourDTO, CancellationToken cancellationToken = default)
		{
			if (brickPartColourDTO == null)
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



			if (id != brickPartColourDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.BrickPartColour> query = (from x in _context.BrickPartColours
				where
				(x.id == id)
				select x);


			Database.BrickPartColour existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickPartColour PUT", id.ToString(), new Exception("No BMC.BrickPartColour entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (brickPartColourDTO.objectGuid == Guid.Empty)
            {
                brickPartColourDTO.objectGuid = existing.objectGuid;
            }
            else if (brickPartColourDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a BrickPartColour record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.BrickPartColour cloneOfExisting = (Database.BrickPartColour)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new BrickPartColour object using the data from the existing record, updated with what is in the DTO.
			//
			Database.BrickPartColour brickPartColour = (Database.BrickPartColour)_context.Entry(existing).GetDatabaseValues().ToObject();
			brickPartColour.ApplyDTO(brickPartColourDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (brickPartColour.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.BrickPartColour record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			EntityEntry<Database.BrickPartColour> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(brickPartColour);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.BrickPartColour entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickPartColour.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickPartColour.CreateAnonymousWithFirstLevelSubObjects(brickPartColour)),
					null);


				return Ok(Database.BrickPartColour.CreateAnonymous(brickPartColour));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.BrickPartColour entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickPartColour.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickPartColour.CreateAnonymousWithFirstLevelSubObjects(brickPartColour)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new BrickPartColour record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickPartColour", Name = "BrickPartColour")]
		public async Task<IActionResult> PostBrickPartColour([FromBody]Database.BrickPartColour.BrickPartColourDTO brickPartColourDTO, CancellationToken cancellationToken = default)
		{
			if (brickPartColourDTO == null)
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
			// Create a new BrickPartColour object using the data from the DTO
			//
			Database.BrickPartColour brickPartColour = Database.BrickPartColour.FromDTO(brickPartColourDTO);

			try
			{
				brickPartColour.objectGuid = Guid.NewGuid();
				_context.BrickPartColours.Add(brickPartColour);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.BrickPartColour entity successfully created.",
					true,
					brickPartColour.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.BrickPartColour.CreateAnonymousWithFirstLevelSubObjects(brickPartColour)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.BrickPartColour entity creation failed.", false, brickPartColour.id.ToString(), "", JsonSerializer.Serialize(brickPartColour), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickPartColour", brickPartColour.id, brickPartColour.id.ToString()));

			return CreatedAtRoute("BrickPartColour", new { id = brickPartColour.id }, Database.BrickPartColour.CreateAnonymousWithFirstLevelSubObjects(brickPartColour));
		}



        /// <summary>
        /// 
        /// This deletes a BrickPartColour record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickPartColour/{id}")]
		[Route("api/BrickPartColour")]
		public async Task<IActionResult> DeleteBrickPartColour(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.BrickPartColour> query = (from x in _context.BrickPartColours
				where
				(x.id == id)
				select x);


			Database.BrickPartColour brickPartColour = await query.FirstOrDefaultAsync(cancellationToken);

			if (brickPartColour == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickPartColour DELETE", id.ToString(), new Exception("No BMC.BrickPartColour entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.BrickPartColour cloneOfExisting = (Database.BrickPartColour)_context.Entry(brickPartColour).GetDatabaseValues().ToObject();


			try
			{
				brickPartColour.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BrickPartColour entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickPartColour.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickPartColour.CreateAnonymousWithFirstLevelSubObjects(brickPartColour)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BrickPartColour entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickPartColour.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickPartColour.CreateAnonymousWithFirstLevelSubObjects(brickPartColour)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of BrickPartColour records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/BrickPartColours/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? brickPartId = null,
			int? brickColourId = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
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

			IQueryable<Database.BrickPartColour> query = (from bpc in _context.BrickPartColours select bpc);
			if (brickPartId.HasValue == true)
			{
				query = query.Where(bpc => bpc.brickPartId == brickPartId.Value);
			}
			if (brickColourId.HasValue == true)
			{
				query = query.Where(bpc => bpc.brickColourId == brickColourId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bpc => bpc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bpc => bpc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bpc => bpc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bpc => bpc.deleted == false);
				}
			}
			else
			{
				query = query.Where(bpc => bpc.active == true);
				query = query.Where(bpc => bpc.deleted == false);
			}


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.BrickPartColour.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/BrickPartColour/CreateAuditEvent")]
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
