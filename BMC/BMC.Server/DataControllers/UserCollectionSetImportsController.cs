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
    /// This auto generated class provides the basic CRUD operations for the UserCollectionSetImport entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the UserCollectionSetImport entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class UserCollectionSetImportsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 10;

		private BMCContext _context;

		private ILogger<UserCollectionSetImportsController> _logger;

		public UserCollectionSetImportsController(BMCContext context, ILogger<UserCollectionSetImportsController> logger) : base("BMC", "UserCollectionSetImport")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of UserCollectionSetImports filtered by the parameters provided.
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
		[Route("api/UserCollectionSetImports")]
		public async Task<IActionResult> GetUserCollectionSetImports(
			int? userCollectionId = null,
			int? legoSetId = null,
			int? quantity = null,
			DateTime? importedDate = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
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
			if (importedDate.HasValue == true && importedDate.Value.Kind != DateTimeKind.Utc)
			{
				importedDate = importedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.UserCollectionSetImport> query = (from ucsi in _context.UserCollectionSetImports select ucsi);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userCollectionId.HasValue == true)
			{
				query = query.Where(ucsi => ucsi.userCollectionId == userCollectionId.Value);
			}
			if (legoSetId.HasValue == true)
			{
				query = query.Where(ucsi => ucsi.legoSetId == legoSetId.Value);
			}
			if (quantity.HasValue == true)
			{
				query = query.Where(ucsi => ucsi.quantity == quantity.Value);
			}
			if (importedDate.HasValue == true)
			{
				query = query.Where(ucsi => ucsi.importedDate == importedDate.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ucsi => ucsi.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ucsi => ucsi.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ucsi => ucsi.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ucsi => ucsi.deleted == false);
				}
			}
			else
			{
				query = query.Where(ucsi => ucsi.active == true);
				query = query.Where(ucsi => ucsi.deleted == false);
			}

			query = query.OrderBy(ucsi => ucsi.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.legoSet);
				query = query.Include(x => x.userCollection);
				query = query.AsSplitQuery();
			}

			query = query.AsNoTracking();
			
			List<Database.UserCollectionSetImport> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.UserCollectionSetImport userCollectionSetImport in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(userCollectionSetImport, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.UserCollectionSetImport Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.UserCollectionSetImport Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of UserCollectionSetImports filtered by the parameters provided.  Its query is similar to the GetUserCollectionSetImports method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserCollectionSetImports/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? userCollectionId = null,
			int? legoSetId = null,
			int? quantity = null,
			DateTime? importedDate = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
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
			if (importedDate.HasValue == true && importedDate.Value.Kind != DateTimeKind.Utc)
			{
				importedDate = importedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.UserCollectionSetImport> query = (from ucsi in _context.UserCollectionSetImports select ucsi);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (userCollectionId.HasValue == true)
			{
				query = query.Where(ucsi => ucsi.userCollectionId == userCollectionId.Value);
			}
			if (legoSetId.HasValue == true)
			{
				query = query.Where(ucsi => ucsi.legoSetId == legoSetId.Value);
			}
			if (quantity.HasValue == true)
			{
				query = query.Where(ucsi => ucsi.quantity == quantity.Value);
			}
			if (importedDate.HasValue == true)
			{
				query = query.Where(ucsi => ucsi.importedDate == importedDate.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ucsi => ucsi.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ucsi => ucsi.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ucsi => ucsi.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ucsi => ucsi.deleted == false);
				}
			}
			else
			{
				query = query.Where(ucsi => ucsi.active == true);
				query = query.Where(ucsi => ucsi.deleted == false);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single UserCollectionSetImport by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserCollectionSetImport/{id}")]
		public async Task<IActionResult> GetUserCollectionSetImport(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
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
				IQueryable<Database.UserCollectionSetImport> query = (from ucsi in _context.UserCollectionSetImports where
							(ucsi.id == id) &&
							(userIsAdmin == true || ucsi.deleted == false) &&
							(userIsWriter == true || ucsi.active == true)
					select ucsi);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.legoSet);
					query = query.Include(x => x.userCollection);
					query = query.AsSplitQuery();
				}

				Database.UserCollectionSetImport materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.UserCollectionSetImport Entity was read with Admin privilege." : "BMC.UserCollectionSetImport Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserCollectionSetImport", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.UserCollectionSetImport entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.UserCollectionSetImport.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.UserCollectionSetImport.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing UserCollectionSetImport record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/UserCollectionSetImport/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutUserCollectionSetImport(int id, [FromBody]Database.UserCollectionSetImport.UserCollectionSetImportDTO userCollectionSetImportDTO, CancellationToken cancellationToken = default)
		{
			if (userCollectionSetImportDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Collection Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != userCollectionSetImportDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
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


			IQueryable<Database.UserCollectionSetImport> query = (from x in _context.UserCollectionSetImports
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserCollectionSetImport existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserCollectionSetImport PUT", id.ToString(), new Exception("No BMC.UserCollectionSetImport entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (userCollectionSetImportDTO.objectGuid == Guid.Empty)
            {
                userCollectionSetImportDTO.objectGuid = existing.objectGuid;
            }
            else if (userCollectionSetImportDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a UserCollectionSetImport record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.UserCollectionSetImport cloneOfExisting = (Database.UserCollectionSetImport)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new UserCollectionSetImport object using the data from the existing record, updated with what is in the DTO.
			//
			Database.UserCollectionSetImport userCollectionSetImport = (Database.UserCollectionSetImport)_context.Entry(existing).GetDatabaseValues().ToObject();
			userCollectionSetImport.ApplyDTO(userCollectionSetImportDTO);
			//
			// The tenant guid for any UserCollectionSetImport being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the UserCollectionSetImport because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				userCollectionSetImport.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (userCollectionSetImport.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.UserCollectionSetImport record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (userCollectionSetImport.importedDate.HasValue == true && userCollectionSetImport.importedDate.Value.Kind != DateTimeKind.Utc)
			{
				userCollectionSetImport.importedDate = userCollectionSetImport.importedDate.Value.ToUniversalTime();
			}

			EntityEntry<Database.UserCollectionSetImport> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(userCollectionSetImport);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.UserCollectionSetImport entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserCollectionSetImport.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserCollectionSetImport.CreateAnonymousWithFirstLevelSubObjects(userCollectionSetImport)),
					null);


				return Ok(Database.UserCollectionSetImport.CreateAnonymous(userCollectionSetImport));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.UserCollectionSetImport entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserCollectionSetImport.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserCollectionSetImport.CreateAnonymousWithFirstLevelSubObjects(userCollectionSetImport)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new UserCollectionSetImport record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserCollectionSetImport", Name = "UserCollectionSetImport")]
		public async Task<IActionResult> PostUserCollectionSetImport([FromBody]Database.UserCollectionSetImport.UserCollectionSetImportDTO userCollectionSetImportDTO, CancellationToken cancellationToken = default)
		{
			if (userCollectionSetImportDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Collection Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new UserCollectionSetImport object using the data from the DTO
			//
			Database.UserCollectionSetImport userCollectionSetImport = Database.UserCollectionSetImport.FromDTO(userCollectionSetImportDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				userCollectionSetImport.tenantGuid = userTenantGuid;

				if (userCollectionSetImport.importedDate.HasValue == true && userCollectionSetImport.importedDate.Value.Kind != DateTimeKind.Utc)
				{
					userCollectionSetImport.importedDate = userCollectionSetImport.importedDate.Value.ToUniversalTime();
				}

				userCollectionSetImport.objectGuid = Guid.NewGuid();
				_context.UserCollectionSetImports.Add(userCollectionSetImport);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.UserCollectionSetImport entity successfully created.",
					true,
					userCollectionSetImport.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.UserCollectionSetImport.CreateAnonymousWithFirstLevelSubObjects(userCollectionSetImport)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.UserCollectionSetImport entity creation failed.", false, userCollectionSetImport.id.ToString(), "", JsonSerializer.Serialize(userCollectionSetImport), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserCollectionSetImport", userCollectionSetImport.id, userCollectionSetImport.id.ToString()));

			return CreatedAtRoute("UserCollectionSetImport", new { id = userCollectionSetImport.id }, Database.UserCollectionSetImport.CreateAnonymousWithFirstLevelSubObjects(userCollectionSetImport));
		}



        /// <summary>
        /// 
        /// This deletes a UserCollectionSetImport record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserCollectionSetImport/{id}")]
		[Route("api/UserCollectionSetImport")]
		public async Task<IActionResult> DeleteUserCollectionSetImport(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Collection Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.UserCollectionSetImport> query = (from x in _context.UserCollectionSetImports
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserCollectionSetImport userCollectionSetImport = await query.FirstOrDefaultAsync(cancellationToken);

			if (userCollectionSetImport == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserCollectionSetImport DELETE", id.ToString(), new Exception("No BMC.UserCollectionSetImport entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.UserCollectionSetImport cloneOfExisting = (Database.UserCollectionSetImport)_context.Entry(userCollectionSetImport).GetDatabaseValues().ToObject();


			try
			{
				userCollectionSetImport.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.UserCollectionSetImport entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserCollectionSetImport.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserCollectionSetImport.CreateAnonymousWithFirstLevelSubObjects(userCollectionSetImport)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.UserCollectionSetImport entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserCollectionSetImport.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserCollectionSetImport.CreateAnonymousWithFirstLevelSubObjects(userCollectionSetImport)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of UserCollectionSetImport records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/UserCollectionSetImports/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? userCollectionId = null,
			int? legoSetId = null,
			int? quantity = null,
			DateTime? importedDate = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);


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
			if (importedDate.HasValue == true && importedDate.Value.Kind != DateTimeKind.Utc)
			{
				importedDate = importedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.UserCollectionSetImport> query = (from ucsi in _context.UserCollectionSetImports select ucsi);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userCollectionId.HasValue == true)
			{
				query = query.Where(ucsi => ucsi.userCollectionId == userCollectionId.Value);
			}
			if (legoSetId.HasValue == true)
			{
				query = query.Where(ucsi => ucsi.legoSetId == legoSetId.Value);
			}
			if (quantity.HasValue == true)
			{
				query = query.Where(ucsi => ucsi.quantity == quantity.Value);
			}
			if (importedDate.HasValue == true)
			{
				query = query.Where(ucsi => ucsi.importedDate == importedDate.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ucsi => ucsi.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ucsi => ucsi.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ucsi => ucsi.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ucsi => ucsi.deleted == false);
				}
			}
			else
			{
				query = query.Where(ucsi => ucsi.active == true);
				query = query.Where(ucsi => ucsi.deleted == false);
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.UserCollectionSetImport.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/UserCollectionSetImport/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Collection Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
