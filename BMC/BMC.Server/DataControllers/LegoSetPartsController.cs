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
    /// This auto generated class provides the basic CRUD operations for the LegoSetPart entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the LegoSetPart entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class LegoSetPartsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<LegoSetPartsController> _logger;

		public LegoSetPartsController(BMCContext context, ILogger<LegoSetPartsController> logger) : base("BMC", "LegoSetPart")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of LegoSetParts filtered by the parameters provided.
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
		[Route("api/LegoSetParts")]
		public async Task<IActionResult> GetLegoSetParts(
			int? legoSetId = null,
			int? brickPartId = null,
			int? brickColourId = null,
			int? quantity = null,
			bool? isSpare = null,
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

			IQueryable<Database.LegoSetPart> query = (from lsp in _context.LegoSetParts select lsp);
			if (legoSetId.HasValue == true)
			{
				query = query.Where(lsp => lsp.legoSetId == legoSetId.Value);
			}
			if (brickPartId.HasValue == true)
			{
				query = query.Where(lsp => lsp.brickPartId == brickPartId.Value);
			}
			if (brickColourId.HasValue == true)
			{
				query = query.Where(lsp => lsp.brickColourId == brickColourId.Value);
			}
			if (quantity.HasValue == true)
			{
				query = query.Where(lsp => lsp.quantity == quantity.Value);
			}
			if (isSpare.HasValue == true)
			{
				query = query.Where(lsp => lsp.isSpare == isSpare.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(lsp => lsp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(lsp => lsp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(lsp => lsp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(lsp => lsp.deleted == false);
				}
			}
			else
			{
				query = query.Where(lsp => lsp.active == true);
				query = query.Where(lsp => lsp.deleted == false);
			}

			query = query.OrderBy(lsp => lsp.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.brickColour);
				query = query.Include(x => x.brickPart);
				query = query.Include(x => x.legoSet);
				query = query.AsSplitQuery();
			}

			query = query.AsNoTracking();
			
			List<Database.LegoSetPart> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.LegoSetPart legoSetPart in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(legoSetPart, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.LegoSetPart Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.LegoSetPart Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of LegoSetParts filtered by the parameters provided.  Its query is similar to the GetLegoSetParts method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LegoSetParts/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? legoSetId = null,
			int? brickPartId = null,
			int? brickColourId = null,
			int? quantity = null,
			bool? isSpare = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.LegoSetPart> query = (from lsp in _context.LegoSetParts select lsp);
			if (legoSetId.HasValue == true)
			{
				query = query.Where(lsp => lsp.legoSetId == legoSetId.Value);
			}
			if (brickPartId.HasValue == true)
			{
				query = query.Where(lsp => lsp.brickPartId == brickPartId.Value);
			}
			if (brickColourId.HasValue == true)
			{
				query = query.Where(lsp => lsp.brickColourId == brickColourId.Value);
			}
			if (quantity.HasValue == true)
			{
				query = query.Where(lsp => lsp.quantity == quantity.Value);
			}
			if (isSpare.HasValue == true)
			{
				query = query.Where(lsp => lsp.isSpare == isSpare.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(lsp => lsp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(lsp => lsp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(lsp => lsp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(lsp => lsp.deleted == false);
				}
			}
			else
			{
				query = query.Where(lsp => lsp.active == true);
				query = query.Where(lsp => lsp.deleted == false);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single LegoSetPart by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LegoSetPart/{id}")]
		public async Task<IActionResult> GetLegoSetPart(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.LegoSetPart> query = (from lsp in _context.LegoSetParts where
							(lsp.id == id) &&
							(userIsAdmin == true || lsp.deleted == false) &&
							(userIsWriter == true || lsp.active == true)
					select lsp);

				if (includeRelations == true)
				{
					query = query.Include(x => x.brickColour);
					query = query.Include(x => x.brickPart);
					query = query.Include(x => x.legoSet);
					query = query.AsSplitQuery();
				}

				Database.LegoSetPart materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.LegoSetPart Entity was read with Admin privilege." : "BMC.LegoSetPart Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "LegoSetPart", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.LegoSetPart entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.LegoSetPart.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.LegoSetPart.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing LegoSetPart record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/LegoSetPart/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutLegoSetPart(int id, [FromBody]Database.LegoSetPart.LegoSetPartDTO legoSetPartDTO, CancellationToken cancellationToken = default)
		{
			if (legoSetPartDTO == null)
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



			if (id != legoSetPartDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.LegoSetPart> query = (from x in _context.LegoSetParts
				where
				(x.id == id)
				select x);


			Database.LegoSetPart existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.LegoSetPart PUT", id.ToString(), new Exception("No BMC.LegoSetPart entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (legoSetPartDTO.objectGuid == Guid.Empty)
            {
                legoSetPartDTO.objectGuid = existing.objectGuid;
            }
            else if (legoSetPartDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a LegoSetPart record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.LegoSetPart cloneOfExisting = (Database.LegoSetPart)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new LegoSetPart object using the data from the existing record, updated with what is in the DTO.
			//
			Database.LegoSetPart legoSetPart = (Database.LegoSetPart)_context.Entry(existing).GetDatabaseValues().ToObject();
			legoSetPart.ApplyDTO(legoSetPartDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (legoSetPart.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.LegoSetPart record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			EntityEntry<Database.LegoSetPart> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(legoSetPart);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.LegoSetPart entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.LegoSetPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LegoSetPart.CreateAnonymousWithFirstLevelSubObjects(legoSetPart)),
					null);


				return Ok(Database.LegoSetPart.CreateAnonymous(legoSetPart));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.LegoSetPart entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.LegoSetPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LegoSetPart.CreateAnonymousWithFirstLevelSubObjects(legoSetPart)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new LegoSetPart record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LegoSetPart", Name = "LegoSetPart")]
		public async Task<IActionResult> PostLegoSetPart([FromBody]Database.LegoSetPart.LegoSetPartDTO legoSetPartDTO, CancellationToken cancellationToken = default)
		{
			if (legoSetPartDTO == null)
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
			// Create a new LegoSetPart object using the data from the DTO
			//
			Database.LegoSetPart legoSetPart = Database.LegoSetPart.FromDTO(legoSetPartDTO);

			try
			{
				legoSetPart.objectGuid = Guid.NewGuid();
				_context.LegoSetParts.Add(legoSetPart);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.LegoSetPart entity successfully created.",
					true,
					legoSetPart.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.LegoSetPart.CreateAnonymousWithFirstLevelSubObjects(legoSetPart)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.LegoSetPart entity creation failed.", false, legoSetPart.id.ToString(), "", JsonSerializer.Serialize(legoSetPart), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "LegoSetPart", legoSetPart.id, legoSetPart.id.ToString()));

			return CreatedAtRoute("LegoSetPart", new { id = legoSetPart.id }, Database.LegoSetPart.CreateAnonymousWithFirstLevelSubObjects(legoSetPart));
		}



        /// <summary>
        /// 
        /// This deletes a LegoSetPart record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LegoSetPart/{id}")]
		[Route("api/LegoSetPart")]
		public async Task<IActionResult> DeleteLegoSetPart(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.LegoSetPart> query = (from x in _context.LegoSetParts
				where
				(x.id == id)
				select x);


			Database.LegoSetPart legoSetPart = await query.FirstOrDefaultAsync(cancellationToken);

			if (legoSetPart == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.LegoSetPart DELETE", id.ToString(), new Exception("No BMC.LegoSetPart entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.LegoSetPart cloneOfExisting = (Database.LegoSetPart)_context.Entry(legoSetPart).GetDatabaseValues().ToObject();


			try
			{
				legoSetPart.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.LegoSetPart entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.LegoSetPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LegoSetPart.CreateAnonymousWithFirstLevelSubObjects(legoSetPart)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.LegoSetPart entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.LegoSetPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LegoSetPart.CreateAnonymousWithFirstLevelSubObjects(legoSetPart)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of LegoSetPart records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/LegoSetParts/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? legoSetId = null,
			int? brickPartId = null,
			int? brickColourId = null,
			int? quantity = null,
			bool? isSpare = null,
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

			IQueryable<Database.LegoSetPart> query = (from lsp in _context.LegoSetParts select lsp);
			if (legoSetId.HasValue == true)
			{
				query = query.Where(lsp => lsp.legoSetId == legoSetId.Value);
			}
			if (brickPartId.HasValue == true)
			{
				query = query.Where(lsp => lsp.brickPartId == brickPartId.Value);
			}
			if (brickColourId.HasValue == true)
			{
				query = query.Where(lsp => lsp.brickColourId == brickColourId.Value);
			}
			if (quantity.HasValue == true)
			{
				query = query.Where(lsp => lsp.quantity == quantity.Value);
			}
			if (isSpare.HasValue == true)
			{
				query = query.Where(lsp => lsp.isSpare == isSpare.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(lsp => lsp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(lsp => lsp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(lsp => lsp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(lsp => lsp.deleted == false);
				}
			}
			else
			{
				query = query.Where(lsp => lsp.active == true);
				query = query.Where(lsp => lsp.deleted == false);
			}


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.LegoSetPart.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/LegoSetPart/CreateAuditEvent")]
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
