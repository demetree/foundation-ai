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
    /// This auto generated class provides the basic CRUD operations for the CompiledGlb entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the CompiledGlb entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class CompiledGlbsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		private BMCContext _context;

		private ILogger<CompiledGlbsController> _logger;

		public CompiledGlbsController(BMCContext context, ILogger<CompiledGlbsController> logger) : base("BMC", "CompiledGlb")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of CompiledGlbs filtered by the parameters provided.
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
		[Route("api/CompiledGlbs")]
		public async Task<IActionResult> GetCompiledGlbs(
			int? projectId = null,
			int? projectVersionNumber = null,
			bool? includesEdgeLines = null,
			long? glbSizeBytes = null,
			int? triangleCount = null,
			int? stepCount = null,
			DateTime? compiledAt = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (compiledAt.HasValue == true && compiledAt.Value.Kind != DateTimeKind.Utc)
			{
				compiledAt = compiledAt.Value.ToUniversalTime();
			}

			IQueryable<Database.CompiledGlb> query = (from cg in _context.CompiledGlbs select cg);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (projectId.HasValue == true)
			{
				query = query.Where(cg => cg.projectId == projectId.Value);
			}
			if (projectVersionNumber.HasValue == true)
			{
				query = query.Where(cg => cg.projectVersionNumber == projectVersionNumber.Value);
			}
			if (includesEdgeLines.HasValue == true)
			{
				query = query.Where(cg => cg.includesEdgeLines == includesEdgeLines.Value);
			}
			if (glbSizeBytes.HasValue == true)
			{
				query = query.Where(cg => cg.glbSizeBytes == glbSizeBytes.Value);
			}
			if (triangleCount.HasValue == true)
			{
				query = query.Where(cg => cg.triangleCount == triangleCount.Value);
			}
			if (stepCount.HasValue == true)
			{
				query = query.Where(cg => cg.stepCount == stepCount.Value);
			}
			if (compiledAt.HasValue == true)
			{
				query = query.Where(cg => cg.compiledAt == compiledAt.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cg => cg.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cg => cg.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cg => cg.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cg => cg.deleted == false);
				}
			}
			else
			{
				query = query.Where(cg => cg.active == true);
				query = query.Where(cg => cg.deleted == false);
			}

			query = query.OrderBy(cg => cg.id);

			if (includeRelations == true)
			{
				query = query.Include(x => x.project);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.CompiledGlb> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.CompiledGlb compiledGlb in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(compiledGlb, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.CompiledGlb Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.CompiledGlb Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of CompiledGlbs filtered by the parameters provided.  Its query is similar to the GetCompiledGlbs method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/CompiledGlbs/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? projectId = null,
			int? projectVersionNumber = null,
			bool? includesEdgeLines = null,
			long? glbSizeBytes = null,
			int? triangleCount = null,
			int? stepCount = null,
			DateTime? compiledAt = null,
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
			// Fix any non-UTC date parameters that come in.
			//
			if (compiledAt.HasValue == true && compiledAt.Value.Kind != DateTimeKind.Utc)
			{
				compiledAt = compiledAt.Value.ToUniversalTime();
			}

			IQueryable<Database.CompiledGlb> query = (from cg in _context.CompiledGlbs select cg);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (projectId.HasValue == true)
			{
				query = query.Where(cg => cg.projectId == projectId.Value);
			}
			if (projectVersionNumber.HasValue == true)
			{
				query = query.Where(cg => cg.projectVersionNumber == projectVersionNumber.Value);
			}
			if (includesEdgeLines.HasValue == true)
			{
				query = query.Where(cg => cg.includesEdgeLines == includesEdgeLines.Value);
			}
			if (glbSizeBytes.HasValue == true)
			{
				query = query.Where(cg => cg.glbSizeBytes == glbSizeBytes.Value);
			}
			if (triangleCount.HasValue == true)
			{
				query = query.Where(cg => cg.triangleCount == triangleCount.Value);
			}
			if (stepCount.HasValue == true)
			{
				query = query.Where(cg => cg.stepCount == stepCount.Value);
			}
			if (compiledAt.HasValue == true)
			{
				query = query.Where(cg => cg.compiledAt == compiledAt.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cg => cg.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cg => cg.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cg => cg.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cg => cg.deleted == false);
				}
			}
			else
			{
				query = query.Where(cg => cg.active == true);
				query = query.Where(cg => cg.deleted == false);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single CompiledGlb by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/CompiledGlb/{id}")]
		public async Task<IActionResult> GetCompiledGlb(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.CompiledGlb> query = (from cg in _context.CompiledGlbs where
							(cg.id == id) &&
							(userIsAdmin == true || cg.deleted == false) &&
							(userIsWriter == true || cg.active == true)
					select cg);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.project);
					query = query.AsSplitQuery();
				}

				Database.CompiledGlb materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.CompiledGlb Entity was read with Admin privilege." : "BMC.CompiledGlb Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "CompiledGlb", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.CompiledGlb entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.CompiledGlb.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.CompiledGlb.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing CompiledGlb record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/CompiledGlb/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutCompiledGlb(int id, [FromBody]Database.CompiledGlb.CompiledGlbDTO compiledGlbDTO, CancellationToken cancellationToken = default)
		{
			if (compiledGlbDTO == null)
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



			if (id != compiledGlbDTO.id)
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


			IQueryable<Database.CompiledGlb> query = (from x in _context.CompiledGlbs
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.CompiledGlb existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.CompiledGlb PUT", id.ToString(), new Exception("No BMC.CompiledGlb entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (compiledGlbDTO.objectGuid == Guid.Empty)
            {
                compiledGlbDTO.objectGuid = existing.objectGuid;
            }
            else if (compiledGlbDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a CompiledGlb record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.CompiledGlb cloneOfExisting = (Database.CompiledGlb)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new CompiledGlb object using the data from the existing record, updated with what is in the DTO.
			//
			Database.CompiledGlb compiledGlb = (Database.CompiledGlb)_context.Entry(existing).GetDatabaseValues().ToObject();
			compiledGlb.ApplyDTO(compiledGlbDTO);
			//
			// The tenant guid for any CompiledGlb being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the CompiledGlb because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				compiledGlb.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (compiledGlb.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.CompiledGlb record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (compiledGlb.compiledAt.Kind != DateTimeKind.Utc)
			{
				compiledGlb.compiledAt = compiledGlb.compiledAt.ToUniversalTime();
			}

			EntityEntry<Database.CompiledGlb> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(compiledGlb);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.CompiledGlb entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.CompiledGlb.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.CompiledGlb.CreateAnonymousWithFirstLevelSubObjects(compiledGlb)),
					null);


				return Ok(Database.CompiledGlb.CreateAnonymous(compiledGlb));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.CompiledGlb entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.CompiledGlb.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.CompiledGlb.CreateAnonymousWithFirstLevelSubObjects(compiledGlb)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new CompiledGlb record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/CompiledGlb", Name = "CompiledGlb")]
		public async Task<IActionResult> PostCompiledGlb([FromBody]Database.CompiledGlb.CompiledGlbDTO compiledGlbDTO, CancellationToken cancellationToken = default)
		{
			if (compiledGlbDTO == null)
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
			// Create a new CompiledGlb object using the data from the DTO
			//
			Database.CompiledGlb compiledGlb = Database.CompiledGlb.FromDTO(compiledGlbDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				compiledGlb.tenantGuid = userTenantGuid;

				if (compiledGlb.compiledAt.Kind != DateTimeKind.Utc)
				{
					compiledGlb.compiledAt = compiledGlb.compiledAt.ToUniversalTime();
				}

				compiledGlb.objectGuid = Guid.NewGuid();
				_context.CompiledGlbs.Add(compiledGlb);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.CompiledGlb entity successfully created.",
					true,
					compiledGlb.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.CompiledGlb.CreateAnonymousWithFirstLevelSubObjects(compiledGlb)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.CompiledGlb entity creation failed.", false, compiledGlb.id.ToString(), "", JsonSerializer.Serialize(compiledGlb), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "CompiledGlb", compiledGlb.id, compiledGlb.id.ToString()));

			return CreatedAtRoute("CompiledGlb", new { id = compiledGlb.id }, Database.CompiledGlb.CreateAnonymousWithFirstLevelSubObjects(compiledGlb));
		}



        /// <summary>
        /// 
        /// This deletes a CompiledGlb record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/CompiledGlb/{id}")]
		[Route("api/CompiledGlb")]
		public async Task<IActionResult> DeleteCompiledGlb(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.CompiledGlb> query = (from x in _context.CompiledGlbs
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.CompiledGlb compiledGlb = await query.FirstOrDefaultAsync(cancellationToken);

			if (compiledGlb == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.CompiledGlb DELETE", id.ToString(), new Exception("No BMC.CompiledGlb entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.CompiledGlb cloneOfExisting = (Database.CompiledGlb)_context.Entry(compiledGlb).GetDatabaseValues().ToObject();


			try
			{
				compiledGlb.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.CompiledGlb entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.CompiledGlb.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.CompiledGlb.CreateAnonymousWithFirstLevelSubObjects(compiledGlb)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.CompiledGlb entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.CompiledGlb.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.CompiledGlb.CreateAnonymousWithFirstLevelSubObjects(compiledGlb)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of CompiledGlb records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/CompiledGlbs/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? projectId = null,
			int? projectVersionNumber = null,
			bool? includesEdgeLines = null,
			long? glbSizeBytes = null,
			int? triangleCount = null,
			int? stepCount = null,
			DateTime? compiledAt = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (compiledAt.HasValue == true && compiledAt.Value.Kind != DateTimeKind.Utc)
			{
				compiledAt = compiledAt.Value.ToUniversalTime();
			}

			IQueryable<Database.CompiledGlb> query = (from cg in _context.CompiledGlbs select cg);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (projectId.HasValue == true)
			{
				query = query.Where(cg => cg.projectId == projectId.Value);
			}
			if (projectVersionNumber.HasValue == true)
			{
				query = query.Where(cg => cg.projectVersionNumber == projectVersionNumber.Value);
			}
			if (includesEdgeLines.HasValue == true)
			{
				query = query.Where(cg => cg.includesEdgeLines == includesEdgeLines.Value);
			}
			if (glbSizeBytes.HasValue == true)
			{
				query = query.Where(cg => cg.glbSizeBytes == glbSizeBytes.Value);
			}
			if (triangleCount.HasValue == true)
			{
				query = query.Where(cg => cg.triangleCount == triangleCount.Value);
			}
			if (stepCount.HasValue == true)
			{
				query = query.Where(cg => cg.stepCount == stepCount.Value);
			}
			if (compiledAt.HasValue == true)
			{
				query = query.Where(cg => cg.compiledAt == compiledAt.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cg => cg.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cg => cg.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cg => cg.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cg => cg.deleted == false);
				}
			}
			else
			{
				query = query.Where(cg => cg.active == true);
				query = query.Where(cg => cg.deleted == false);
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.CompiledGlb.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/CompiledGlb/CreateAuditEvent")]
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
