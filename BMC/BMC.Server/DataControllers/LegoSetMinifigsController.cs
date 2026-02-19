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
    /// This auto generated class provides the basic CRUD operations for the LegoSetMinifig entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the LegoSetMinifig entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class LegoSetMinifigsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<LegoSetMinifigsController> _logger;

		public LegoSetMinifigsController(BMCContext context, ILogger<LegoSetMinifigsController> logger) : base("BMC", "LegoSetMinifig")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of LegoSetMinifigs filtered by the parameters provided.
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
		[Route("api/LegoSetMinifigs")]
		public async Task<IActionResult> GetLegoSetMinifigs(
			int? legoSetId = null,
			int? legoMinifigId = null,
			int? quantity = null,
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

			IQueryable<Database.LegoSetMinifig> query = (from lsm in _context.LegoSetMinifigs select lsm);
			if (legoSetId.HasValue == true)
			{
				query = query.Where(lsm => lsm.legoSetId == legoSetId.Value);
			}
			if (legoMinifigId.HasValue == true)
			{
				query = query.Where(lsm => lsm.legoMinifigId == legoMinifigId.Value);
			}
			if (quantity.HasValue == true)
			{
				query = query.Where(lsm => lsm.quantity == quantity.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(lsm => lsm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(lsm => lsm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(lsm => lsm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(lsm => lsm.deleted == false);
				}
			}
			else
			{
				query = query.Where(lsm => lsm.active == true);
				query = query.Where(lsm => lsm.deleted == false);
			}

			query = query.OrderBy(lsm => lsm.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.legoMinifig);
				query = query.Include(x => x.legoSet);
				query = query.AsSplitQuery();
			}

			query = query.AsNoTracking();
			
			List<Database.LegoSetMinifig> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.LegoSetMinifig legoSetMinifig in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(legoSetMinifig, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.LegoSetMinifig Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.LegoSetMinifig Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of LegoSetMinifigs filtered by the parameters provided.  Its query is similar to the GetLegoSetMinifigs method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LegoSetMinifigs/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? legoSetId = null,
			int? legoMinifigId = null,
			int? quantity = null,
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

			IQueryable<Database.LegoSetMinifig> query = (from lsm in _context.LegoSetMinifigs select lsm);
			if (legoSetId.HasValue == true)
			{
				query = query.Where(lsm => lsm.legoSetId == legoSetId.Value);
			}
			if (legoMinifigId.HasValue == true)
			{
				query = query.Where(lsm => lsm.legoMinifigId == legoMinifigId.Value);
			}
			if (quantity.HasValue == true)
			{
				query = query.Where(lsm => lsm.quantity == quantity.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(lsm => lsm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(lsm => lsm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(lsm => lsm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(lsm => lsm.deleted == false);
				}
			}
			else
			{
				query = query.Where(lsm => lsm.active == true);
				query = query.Where(lsm => lsm.deleted == false);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single LegoSetMinifig by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LegoSetMinifig/{id}")]
		public async Task<IActionResult> GetLegoSetMinifig(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.LegoSetMinifig> query = (from lsm in _context.LegoSetMinifigs where
							(lsm.id == id) &&
							(userIsAdmin == true || lsm.deleted == false) &&
							(userIsWriter == true || lsm.active == true)
					select lsm);

				if (includeRelations == true)
				{
					query = query.Include(x => x.legoMinifig);
					query = query.Include(x => x.legoSet);
					query = query.AsSplitQuery();
				}

				Database.LegoSetMinifig materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.LegoSetMinifig Entity was read with Admin privilege." : "BMC.LegoSetMinifig Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "LegoSetMinifig", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.LegoSetMinifig entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.LegoSetMinifig.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.LegoSetMinifig.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing LegoSetMinifig record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/LegoSetMinifig/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutLegoSetMinifig(int id, [FromBody]Database.LegoSetMinifig.LegoSetMinifigDTO legoSetMinifigDTO, CancellationToken cancellationToken = default)
		{
			if (legoSetMinifigDTO == null)
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



			if (id != legoSetMinifigDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.LegoSetMinifig> query = (from x in _context.LegoSetMinifigs
				where
				(x.id == id)
				select x);


			Database.LegoSetMinifig existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.LegoSetMinifig PUT", id.ToString(), new Exception("No BMC.LegoSetMinifig entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (legoSetMinifigDTO.objectGuid == Guid.Empty)
            {
                legoSetMinifigDTO.objectGuid = existing.objectGuid;
            }
            else if (legoSetMinifigDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a LegoSetMinifig record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.LegoSetMinifig cloneOfExisting = (Database.LegoSetMinifig)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new LegoSetMinifig object using the data from the existing record, updated with what is in the DTO.
			//
			Database.LegoSetMinifig legoSetMinifig = (Database.LegoSetMinifig)_context.Entry(existing).GetDatabaseValues().ToObject();
			legoSetMinifig.ApplyDTO(legoSetMinifigDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (legoSetMinifig.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.LegoSetMinifig record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			EntityEntry<Database.LegoSetMinifig> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(legoSetMinifig);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.LegoSetMinifig entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.LegoSetMinifig.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LegoSetMinifig.CreateAnonymousWithFirstLevelSubObjects(legoSetMinifig)),
					null);


				return Ok(Database.LegoSetMinifig.CreateAnonymous(legoSetMinifig));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.LegoSetMinifig entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.LegoSetMinifig.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LegoSetMinifig.CreateAnonymousWithFirstLevelSubObjects(legoSetMinifig)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new LegoSetMinifig record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LegoSetMinifig", Name = "LegoSetMinifig")]
		public async Task<IActionResult> PostLegoSetMinifig([FromBody]Database.LegoSetMinifig.LegoSetMinifigDTO legoSetMinifigDTO, CancellationToken cancellationToken = default)
		{
			if (legoSetMinifigDTO == null)
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
			// Create a new LegoSetMinifig object using the data from the DTO
			//
			Database.LegoSetMinifig legoSetMinifig = Database.LegoSetMinifig.FromDTO(legoSetMinifigDTO);

			try
			{
				legoSetMinifig.objectGuid = Guid.NewGuid();
				_context.LegoSetMinifigs.Add(legoSetMinifig);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.LegoSetMinifig entity successfully created.",
					true,
					legoSetMinifig.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.LegoSetMinifig.CreateAnonymousWithFirstLevelSubObjects(legoSetMinifig)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.LegoSetMinifig entity creation failed.", false, legoSetMinifig.id.ToString(), "", JsonSerializer.Serialize(legoSetMinifig), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "LegoSetMinifig", legoSetMinifig.id, legoSetMinifig.id.ToString()));

			return CreatedAtRoute("LegoSetMinifig", new { id = legoSetMinifig.id }, Database.LegoSetMinifig.CreateAnonymousWithFirstLevelSubObjects(legoSetMinifig));
		}



        /// <summary>
        /// 
        /// This deletes a LegoSetMinifig record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LegoSetMinifig/{id}")]
		[Route("api/LegoSetMinifig")]
		public async Task<IActionResult> DeleteLegoSetMinifig(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.LegoSetMinifig> query = (from x in _context.LegoSetMinifigs
				where
				(x.id == id)
				select x);


			Database.LegoSetMinifig legoSetMinifig = await query.FirstOrDefaultAsync(cancellationToken);

			if (legoSetMinifig == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.LegoSetMinifig DELETE", id.ToString(), new Exception("No BMC.LegoSetMinifig entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.LegoSetMinifig cloneOfExisting = (Database.LegoSetMinifig)_context.Entry(legoSetMinifig).GetDatabaseValues().ToObject();


			try
			{
				legoSetMinifig.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.LegoSetMinifig entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.LegoSetMinifig.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LegoSetMinifig.CreateAnonymousWithFirstLevelSubObjects(legoSetMinifig)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.LegoSetMinifig entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.LegoSetMinifig.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LegoSetMinifig.CreateAnonymousWithFirstLevelSubObjects(legoSetMinifig)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of LegoSetMinifig records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/LegoSetMinifigs/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? legoSetId = null,
			int? legoMinifigId = null,
			int? quantity = null,
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

			IQueryable<Database.LegoSetMinifig> query = (from lsm in _context.LegoSetMinifigs select lsm);
			if (legoSetId.HasValue == true)
			{
				query = query.Where(lsm => lsm.legoSetId == legoSetId.Value);
			}
			if (legoMinifigId.HasValue == true)
			{
				query = query.Where(lsm => lsm.legoMinifigId == legoMinifigId.Value);
			}
			if (quantity.HasValue == true)
			{
				query = query.Where(lsm => lsm.quantity == quantity.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(lsm => lsm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(lsm => lsm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(lsm => lsm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(lsm => lsm.deleted == false);
				}
			}
			else
			{
				query = query.Where(lsm => lsm.active == true);
				query = query.Where(lsm => lsm.deleted == false);
			}


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.LegoSetMinifig.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/LegoSetMinifig/CreateAuditEvent")]
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
