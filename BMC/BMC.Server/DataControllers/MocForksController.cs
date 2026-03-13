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
    /// This auto generated class provides the basic CRUD operations for the MocFork entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the MocFork entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class MocForksController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		private BMCContext _context;

		private ILogger<MocForksController> _logger;

		public MocForksController(BMCContext context, ILogger<MocForksController> logger) : base("BMC", "MocFork")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of MocForks filtered by the parameters provided.
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
		[Route("api/MocForks")]
		public async Task<IActionResult> GetMocForks(
			int? forkedMocId = null,
			int? sourceMocId = null,
			int? mocVersionId = null,
			Guid? forkerTenantGuid = null,
			DateTime? forkedDate = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
			if (forkedDate.HasValue == true && forkedDate.Value.Kind != DateTimeKind.Utc)
			{
				forkedDate = forkedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.MocFork> query = (from mf in _context.MocForks select mf);
			if (forkedMocId.HasValue == true)
			{
				query = query.Where(mf => mf.forkedMocId == forkedMocId.Value);
			}
			if (sourceMocId.HasValue == true)
			{
				query = query.Where(mf => mf.sourceMocId == sourceMocId.Value);
			}
			if (mocVersionId.HasValue == true)
			{
				query = query.Where(mf => mf.mocVersionId == mocVersionId.Value);
			}
			if (forkerTenantGuid.HasValue == true)
			{
				query = query.Where(mf => mf.forkerTenantGuid == forkerTenantGuid);
			}
			if (forkedDate.HasValue == true)
			{
				query = query.Where(mf => mf.forkedDate == forkedDate.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mf => mf.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mf => mf.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mf => mf.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mf => mf.deleted == false);
				}
			}
			else
			{
				query = query.Where(mf => mf.active == true);
				query = query.Where(mf => mf.deleted == false);
			}

			query = query.OrderBy(mf => mf.id);

			if (includeRelations == true)
			{
				query = query.Include(x => x.forkedMoc);
				query = query.Include(x => x.mocVersion);
				query = query.Include(x => x.sourceMoc);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.MocFork> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.MocFork mocFork in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(mocFork, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.MocFork Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.MocFork Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of MocForks filtered by the parameters provided.  Its query is similar to the GetMocForks method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MocForks/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? forkedMocId = null,
			int? sourceMocId = null,
			int? mocVersionId = null,
			Guid? forkerTenantGuid = null,
			DateTime? forkedDate = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (forkedDate.HasValue == true && forkedDate.Value.Kind != DateTimeKind.Utc)
			{
				forkedDate = forkedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.MocFork> query = (from mf in _context.MocForks select mf);
			if (forkedMocId.HasValue == true)
			{
				query = query.Where(mf => mf.forkedMocId == forkedMocId.Value);
			}
			if (sourceMocId.HasValue == true)
			{
				query = query.Where(mf => mf.sourceMocId == sourceMocId.Value);
			}
			if (mocVersionId.HasValue == true)
			{
				query = query.Where(mf => mf.mocVersionId == mocVersionId.Value);
			}
			if (forkerTenantGuid.HasValue == true)
			{
				query = query.Where(mf => mf.forkerTenantGuid == forkerTenantGuid);
			}
			if (forkedDate.HasValue == true)
			{
				query = query.Where(mf => mf.forkedDate == forkedDate.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mf => mf.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mf => mf.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mf => mf.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mf => mf.deleted == false);
				}
			}
			else
			{
				query = query.Where(mf => mf.active == true);
				query = query.Where(mf => mf.deleted == false);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single MocFork by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MocFork/{id}")]
		public async Task<IActionResult> GetMocFork(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			try
			{
				IQueryable<Database.MocFork> query = (from mf in _context.MocForks where
							(mf.id == id) &&
							(userIsAdmin == true || mf.deleted == false) &&
							(userIsWriter == true || mf.active == true)
					select mf);

				if (includeRelations == true)
				{
					query = query.Include(x => x.forkedMoc);
					query = query.Include(x => x.mocVersion);
					query = query.Include(x => x.sourceMoc);
					query = query.AsSplitQuery();
				}

				Database.MocFork materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.MocFork Entity was read with Admin privilege." : "BMC.MocFork Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "MocFork", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.MocFork entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.MocFork.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.MocFork.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing MocFork record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/MocFork/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutMocFork(int id, [FromBody]Database.MocFork.MocForkDTO mocForkDTO, CancellationToken cancellationToken = default)
		{
			if (mocForkDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != mocForkDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.MocFork> query = (from x in _context.MocForks
				where
				(x.id == id)
				select x);


			Database.MocFork existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.MocFork PUT", id.ToString(), new Exception("No BMC.MocFork entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (mocForkDTO.objectGuid == Guid.Empty)
            {
                mocForkDTO.objectGuid = existing.objectGuid;
            }
            else if (mocForkDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a MocFork record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.MocFork cloneOfExisting = (Database.MocFork)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new MocFork object using the data from the existing record, updated with what is in the DTO.
			//
			Database.MocFork mocFork = (Database.MocFork)_context.Entry(existing).GetDatabaseValues().ToObject();
			mocFork.ApplyDTO(mocForkDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (mocFork.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.MocFork record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (mocFork.forkedDate.Kind != DateTimeKind.Utc)
			{
				mocFork.forkedDate = mocFork.forkedDate.ToUniversalTime();
			}

			EntityEntry<Database.MocFork> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(mocFork);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.MocFork entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.MocFork.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MocFork.CreateAnonymousWithFirstLevelSubObjects(mocFork)),
					null);


				return Ok(Database.MocFork.CreateAnonymous(mocFork));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.MocFork entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.MocFork.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MocFork.CreateAnonymousWithFirstLevelSubObjects(mocFork)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new MocFork record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MocFork", Name = "MocFork")]
		public async Task<IActionResult> PostMocFork([FromBody]Database.MocFork.MocForkDTO mocForkDTO, CancellationToken cancellationToken = default)
		{
			if (mocForkDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new MocFork object using the data from the DTO
			//
			Database.MocFork mocFork = Database.MocFork.FromDTO(mocForkDTO);

			try
			{
				if (mocFork.forkedDate.Kind != DateTimeKind.Utc)
				{
					mocFork.forkedDate = mocFork.forkedDate.ToUniversalTime();
				}

				mocFork.objectGuid = Guid.NewGuid();
				_context.MocForks.Add(mocFork);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.MocFork entity successfully created.",
					true,
					mocFork.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.MocFork.CreateAnonymousWithFirstLevelSubObjects(mocFork)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.MocFork entity creation failed.", false, mocFork.id.ToString(), "", JsonSerializer.Serialize(mocFork), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "MocFork", mocFork.id, mocFork.id.ToString()));

			return CreatedAtRoute("MocFork", new { id = mocFork.id }, Database.MocFork.CreateAnonymousWithFirstLevelSubObjects(mocFork));
		}



        /// <summary>
        /// 
        /// This deletes a MocFork record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MocFork/{id}")]
		[Route("api/MocFork")]
		public async Task<IActionResult> DeleteMocFork(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.MocFork> query = (from x in _context.MocForks
				where
				(x.id == id)
				select x);


			Database.MocFork mocFork = await query.FirstOrDefaultAsync(cancellationToken);

			if (mocFork == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.MocFork DELETE", id.ToString(), new Exception("No BMC.MocFork entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.MocFork cloneOfExisting = (Database.MocFork)_context.Entry(mocFork).GetDatabaseValues().ToObject();


			try
			{
				mocFork.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.MocFork entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.MocFork.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MocFork.CreateAnonymousWithFirstLevelSubObjects(mocFork)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.MocFork entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.MocFork.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MocFork.CreateAnonymousWithFirstLevelSubObjects(mocFork)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of MocFork records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/MocForks/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? forkedMocId = null,
			int? sourceMocId = null,
			int? mocVersionId = null,
			Guid? forkerTenantGuid = null,
			DateTime? forkedDate = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);


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
			if (forkedDate.HasValue == true && forkedDate.Value.Kind != DateTimeKind.Utc)
			{
				forkedDate = forkedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.MocFork> query = (from mf in _context.MocForks select mf);
			if (forkedMocId.HasValue == true)
			{
				query = query.Where(mf => mf.forkedMocId == forkedMocId.Value);
			}
			if (sourceMocId.HasValue == true)
			{
				query = query.Where(mf => mf.sourceMocId == sourceMocId.Value);
			}
			if (mocVersionId.HasValue == true)
			{
				query = query.Where(mf => mf.mocVersionId == mocVersionId.Value);
			}
			if (forkerTenantGuid.HasValue == true)
			{
				query = query.Where(mf => mf.forkerTenantGuid == forkerTenantGuid);
			}
			if (forkedDate.HasValue == true)
			{
				query = query.Where(mf => mf.forkedDate == forkedDate.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mf => mf.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mf => mf.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mf => mf.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mf => mf.deleted == false);
				}
			}
			else
			{
				query = query.Where(mf => mf.active == true);
				query = query.Where(mf => mf.deleted == false);
			}


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.MocFork.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/MocFork/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
