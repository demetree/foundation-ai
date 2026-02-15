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
    /// This auto generated class provides the basic CRUD operations for the Submodel entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Submodel entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SubmodelsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object submodelPutSyncRoot = new object();
		static object submodelDeleteSyncRoot = new object();

		private BMCContext _context;

		private ILogger<SubmodelsController> _logger;

		public SubmodelsController(BMCContext context, ILogger<SubmodelsController> logger) : base("BMC", "Submodel")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Submodels filtered by the parameters provided.
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
		[Route("api/Submodels")]
		public async Task<IActionResult> GetSubmodels(
			int? projectId = null,
			string name = null,
			string description = null,
			int? submodelId = null,
			int? sequence = null,
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

			IQueryable<Database.Submodel> query = (from s in _context.Submodels select s);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (projectId.HasValue == true)
			{
				query = query.Where(s => s.projectId == projectId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(s => s.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(s => s.description == description);
			}
			if (submodelId.HasValue == true)
			{
				query = query.Where(s => s.submodelId == submodelId.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(s => s.sequence == sequence.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(s => s.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(s => s.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(s => s.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(s => s.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(s => s.deleted == false);
				}
			}
			else
			{
				query = query.Where(s => s.active == true);
				query = query.Where(s => s.deleted == false);
			}

			query = query.OrderBy(s => s.sequence).ThenBy(s => s.name).ThenBy(s => s.description);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.project);
				query = query.Include(x => x.submodel);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Submodel, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || (includeRelations == true && x.project.name.Contains(anyStringContains))
			       || (includeRelations == true && x.project.description.Contains(anyStringContains))
			       || (includeRelations == true && x.project.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.project.thumbnailImagePath.Contains(anyStringContains))
			       || (includeRelations == true && x.submodel.name.Contains(anyStringContains))
			       || (includeRelations == true && x.submodel.description.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.Submodel> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Submodel submodel in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(submodel, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.Submodel Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.Submodel Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Submodels filtered by the parameters provided.  Its query is similar to the GetSubmodels method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Submodels/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? projectId = null,
			string name = null,
			string description = null,
			int? submodelId = null,
			int? sequence = null,
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


			IQueryable<Database.Submodel> query = (from s in _context.Submodels select s);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (projectId.HasValue == true)
			{
				query = query.Where(s => s.projectId == projectId.Value);
			}
			if (name != null)
			{
				query = query.Where(s => s.name == name);
			}
			if (description != null)
			{
				query = query.Where(s => s.description == description);
			}
			if (submodelId.HasValue == true)
			{
				query = query.Where(s => s.submodelId == submodelId.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(s => s.sequence == sequence.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(s => s.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(s => s.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(s => s.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(s => s.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(s => s.deleted == false);
				}
			}
			else
			{
				query = query.Where(s => s.active == true);
				query = query.Where(s => s.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Submodel, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.project.name.Contains(anyStringContains)
			       || x.project.description.Contains(anyStringContains)
			       || x.project.notes.Contains(anyStringContains)
			       || x.project.thumbnailImagePath.Contains(anyStringContains)
			       || x.submodel.name.Contains(anyStringContains)
			       || x.submodel.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single Submodel by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Submodel/{id}")]
		public async Task<IActionResult> GetSubmodel(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.Submodel> query = (from s in _context.Submodels where
							(s.id == id) &&
							(userIsAdmin == true || s.deleted == false) &&
							(userIsWriter == true || s.active == true)
					select s);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.project);
					query = query.Include(x => x.submodel);
					query = query.AsSplitQuery();
				}

				Database.Submodel materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.Submodel Entity was read with Admin privilege." : "BMC.Submodel Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Submodel", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.Submodel entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.Submodel.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.Submodel.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing Submodel record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Submodel/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSubmodel(int id, [FromBody]Database.Submodel.SubmodelDTO submodelDTO, CancellationToken cancellationToken = default)
		{
			if (submodelDTO == null)
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



			if (id != submodelDTO.id)
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


			IQueryable<Database.Submodel> query = (from x in _context.Submodels
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Submodel existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.Submodel PUT", id.ToString(), new Exception("No BMC.Submodel entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (submodelDTO.objectGuid == Guid.Empty)
            {
                submodelDTO.objectGuid = existing.objectGuid;
            }
            else if (submodelDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Submodel record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Submodel cloneOfExisting = (Database.Submodel)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Submodel object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Submodel submodel = (Database.Submodel)_context.Entry(existing).GetDatabaseValues().ToObject();
			submodel.ApplyDTO(submodelDTO);
			//
			// The tenant guid for any Submodel being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the Submodel because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				submodel.tenantGuid = existing.tenantGuid;
			}

			lock (submodelPutSyncRoot)
			{
				//
				// Validate the version number for the submodel being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != submodel.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Submodel save attempt was made but save request was with version " + submodel.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The Submodel you are trying to update has already changed.  Please try your save again after reloading the Submodel.");
				}
				else
				{
					// Same record.  Increase version.
					submodel.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (submodel.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.Submodel record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (submodel.name != null && submodel.name.Length > 100)
				{
					submodel.name = submodel.name.Substring(0, 100);
				}

				if (submodel.description != null && submodel.description.Length > 500)
				{
					submodel.description = submodel.description.Substring(0, 500);
				}

				try
				{
				    EntityEntry<Database.Submodel> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(submodel);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        SubmodelChangeHistory submodelChangeHistory = new SubmodelChangeHistory();
				        submodelChangeHistory.submodelId = submodel.id;
				        submodelChangeHistory.versionNumber = submodel.versionNumber;
				        submodelChangeHistory.timeStamp = DateTime.UtcNow;
				        submodelChangeHistory.userId = securityUser.id;
				        submodelChangeHistory.tenantGuid = userTenantGuid;
				        submodelChangeHistory.data = JsonSerializer.Serialize(Database.Submodel.CreateAnonymousWithFirstLevelSubObjects(submodel));
				        _context.SubmodelChangeHistories.Add(submodelChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.Submodel entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Submodel.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Submodel.CreateAnonymousWithFirstLevelSubObjects(submodel)),
						null);

				return Ok(Database.Submodel.CreateAnonymous(submodel));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.Submodel entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Submodel.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Submodel.CreateAnonymousWithFirstLevelSubObjects(submodel)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new Submodel record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Submodel", Name = "Submodel")]
		public async Task<IActionResult> PostSubmodel([FromBody]Database.Submodel.SubmodelDTO submodelDTO, CancellationToken cancellationToken = default)
		{
			if (submodelDTO == null)
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
			// Create a new Submodel object using the data from the DTO
			//
			Database.Submodel submodel = Database.Submodel.FromDTO(submodelDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				submodel.tenantGuid = userTenantGuid;

				if (submodel.name != null && submodel.name.Length > 100)
				{
					submodel.name = submodel.name.Substring(0, 100);
				}

				if (submodel.description != null && submodel.description.Length > 500)
				{
					submodel.description = submodel.description.Substring(0, 500);
				}

				submodel.objectGuid = Guid.NewGuid();
				submodel.versionNumber = 1;

				_context.Submodels.Add(submodel);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the submodel object so that no further changes will be written to the database
				    //
				    _context.Entry(submodel).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					submodel.Inversesubmodel = null;
					submodel.SubmodelChangeHistories = null;
					submodel.SubmodelPlacedBricks = null;
					submodel.project = null;
					submodel.submodel = null;


				    SubmodelChangeHistory submodelChangeHistory = new SubmodelChangeHistory();
				    submodelChangeHistory.submodelId = submodel.id;
				    submodelChangeHistory.versionNumber = submodel.versionNumber;
				    submodelChangeHistory.timeStamp = DateTime.UtcNow;
				    submodelChangeHistory.userId = securityUser.id;
				    submodelChangeHistory.tenantGuid = userTenantGuid;
				    submodelChangeHistory.data = JsonSerializer.Serialize(Database.Submodel.CreateAnonymousWithFirstLevelSubObjects(submodel));
				    _context.SubmodelChangeHistories.Add(submodelChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"BMC.Submodel entity successfully created.",
						true,
						submodel. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.Submodel.CreateAnonymousWithFirstLevelSubObjects(submodel)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.Submodel entity creation failed.", false, submodel.id.ToString(), "", JsonSerializer.Serialize(Database.Submodel.CreateAnonymousWithFirstLevelSubObjects(submodel)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Submodel", submodel.id, submodel.name));

			return CreatedAtRoute("Submodel", new { id = submodel.id }, Database.Submodel.CreateAnonymousWithFirstLevelSubObjects(submodel));
		}



        /// <summary>
        /// 
        /// This rolls a Submodel entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Submodel/Rollback/{id}")]
		[Route("api/Submodel/Rollback")]
		public async Task<IActionResult> RollbackToSubmodelVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.Submodel> query = (from x in _context.Submodels
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this Submodel concurrently
			//
			lock (submodelPutSyncRoot)
			{
				
				Database.Submodel submodel = query.FirstOrDefault();
				
				if (submodel == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.Submodel rollback", id.ToString(), new Exception("No BMC.Submodel entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the Submodel current state so we can log it.
				//
				Database.Submodel cloneOfExisting = (Database.Submodel)_context.Entry(submodel).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.Inversesubmodel = null;
				cloneOfExisting.SubmodelChangeHistories = null;
				cloneOfExisting.SubmodelPlacedBricks = null;
				cloneOfExisting.project = null;
				cloneOfExisting.submodel = null;

				if (versionNumber >= submodel.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for BMC.Submodel rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for BMC.Submodel rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				SubmodelChangeHistory submodelChangeHistory = (from x in _context.SubmodelChangeHistories
				                                               where
				                                               x.submodelId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (submodelChangeHistory != null)
				{
				    Database.Submodel oldSubmodel = JsonSerializer.Deserialize<Database.Submodel>(submodelChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    submodel.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    submodel.projectId = oldSubmodel.projectId;
				    submodel.name = oldSubmodel.name;
				    submodel.description = oldSubmodel.description;
				    submodel.submodelId = oldSubmodel.submodelId;
				    submodel.sequence = oldSubmodel.sequence;
				    submodel.objectGuid = oldSubmodel.objectGuid;
				    submodel.active = oldSubmodel.active;
				    submodel.deleted = oldSubmodel.deleted;

				    string serializedSubmodel = JsonSerializer.Serialize(submodel);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        SubmodelChangeHistory newSubmodelChangeHistory = new SubmodelChangeHistory();
				        newSubmodelChangeHistory.submodelId = submodel.id;
				        newSubmodelChangeHistory.versionNumber = submodel.versionNumber;
				        newSubmodelChangeHistory.timeStamp = DateTime.UtcNow;
				        newSubmodelChangeHistory.userId = securityUser.id;
				        newSubmodelChangeHistory.tenantGuid = userTenantGuid;
				        newSubmodelChangeHistory.data = JsonSerializer.Serialize(Database.Submodel.CreateAnonymousWithFirstLevelSubObjects(submodel));
				        _context.SubmodelChangeHistories.Add(newSubmodelChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.Submodel rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Submodel.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Submodel.CreateAnonymousWithFirstLevelSubObjects(submodel)),
						null);


				    return Ok(Database.Submodel.CreateAnonymous(submodel));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for BMC.Submodel rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for BMC.Submodel rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a Submodel.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Submodel</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Submodel/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetSubmodelChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.Submodel submodel = await _context.Submodels.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (submodel == null)
			{
				return NotFound();
			}

			try
			{
				submodel.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Submodel> versionInfo = await submodel.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a Submodel.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Submodel</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Submodel/{id}/AuditHistory")]
		public async Task<IActionResult> GetSubmodelAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.Submodel submodel = await _context.Submodels.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (submodel == null)
			{
				return NotFound();
			}

			try
			{
				submodel.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.Submodel>> versions = await submodel.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a Submodel.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Submodel</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The Submodel object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Submodel/{id}/Version/{version}")]
		public async Task<IActionResult> GetSubmodelVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.Submodel submodel = await _context.Submodels.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (submodel == null)
			{
				return NotFound();
			}

			try
			{
				submodel.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Submodel> versionInfo = await submodel.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a Submodel at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Submodel</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The Submodel object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Submodel/{id}/StateAtTime")]
		public async Task<IActionResult> GetSubmodelStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.Submodel submodel = await _context.Submodels.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (submodel == null)
			{
				return NotFound();
			}

			try
			{
				submodel.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Submodel> versionInfo = await submodel.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a Submodel record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Submodel/{id}")]
		[Route("api/Submodel")]
		public async Task<IActionResult> DeleteSubmodel(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.Submodel> query = (from x in _context.Submodels
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Submodel submodel = await query.FirstOrDefaultAsync(cancellationToken);

			if (submodel == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.Submodel DELETE", id.ToString(), new Exception("No BMC.Submodel entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Submodel cloneOfExisting = (Database.Submodel)_context.Entry(submodel).GetDatabaseValues().ToObject();


			lock (submodelDeleteSyncRoot)
			{
			    try
			    {
			        submodel.deleted = true;
			        submodel.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        SubmodelChangeHistory submodelChangeHistory = new SubmodelChangeHistory();
			        submodelChangeHistory.submodelId = submodel.id;
			        submodelChangeHistory.versionNumber = submodel.versionNumber;
			        submodelChangeHistory.timeStamp = DateTime.UtcNow;
			        submodelChangeHistory.userId = securityUser.id;
			        submodelChangeHistory.tenantGuid = userTenantGuid;
			        submodelChangeHistory.data = JsonSerializer.Serialize(Database.Submodel.CreateAnonymousWithFirstLevelSubObjects(submodel));
			        _context.SubmodelChangeHistories.Add(submodelChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.Submodel entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Submodel.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Submodel.CreateAnonymousWithFirstLevelSubObjects(submodel)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.Submodel entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Submodel.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Submodel.CreateAnonymousWithFirstLevelSubObjects(submodel)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of Submodel records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Submodels/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? projectId = null,
			string name = null,
			string description = null,
			int? submodelId = null,
			int? sequence = null,
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

			IQueryable<Database.Submodel> query = (from s in _context.Submodels select s);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (projectId.HasValue == true)
			{
				query = query.Where(s => s.projectId == projectId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(s => s.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(s => s.description == description);
			}
			if (submodelId.HasValue == true)
			{
				query = query.Where(s => s.submodelId == submodelId.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(s => s.sequence == sequence.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(s => s.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(s => s.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(s => s.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(s => s.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(s => s.deleted == false);
				}
			}
			else
			{
				query = query.Where(s => s.active == true);
				query = query.Where(s => s.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Submodel, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.project.name.Contains(anyStringContains)
			       || x.project.description.Contains(anyStringContains)
			       || x.project.notes.Contains(anyStringContains)
			       || x.project.thumbnailImagePath.Contains(anyStringContains)
			       || x.submodel.name.Contains(anyStringContains)
			       || x.submodel.description.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Submodel.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Submodel/CreateAuditEvent")]
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
