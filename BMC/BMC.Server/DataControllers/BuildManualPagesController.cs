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
    /// This auto generated class provides the basic CRUD operations for the BuildManualPage entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the BuildManualPage entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class BuildManualPagesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 20;

		private BMCContext _context;

		private ILogger<BuildManualPagesController> _logger;

		public BuildManualPagesController(BMCContext context, ILogger<BuildManualPagesController> logger) : base("BMC", "BuildManualPage")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of BuildManualPages filtered by the parameters provided.
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
		[Route("api/BuildManualPages")]
		public async Task<IActionResult> GetBuildManualPages(
			int? buildManualId = null,
			int? pageNum = null,
			string title = null,
			string notes = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 20, cancellationToken);
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

			IQueryable<Database.BuildManualPage> query = (from bmp in _context.BuildManualPages select bmp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (buildManualId.HasValue == true)
			{
				query = query.Where(bmp => bmp.buildManualId == buildManualId.Value);
			}
			if (pageNum.HasValue == true)
			{
				query = query.Where(bmp => bmp.pageNum == pageNum.Value);
			}
			if (string.IsNullOrEmpty(title) == false)
			{
				query = query.Where(bmp => bmp.title == title);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(bmp => bmp.notes == notes);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bmp => bmp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bmp => bmp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bmp => bmp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bmp => bmp.deleted == false);
				}
			}
			else
			{
				query = query.Where(bmp => bmp.active == true);
				query = query.Where(bmp => bmp.deleted == false);
			}

			query = query.OrderBy(bmp => bmp.title);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.buildManual);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Build Manual Page, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.title.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || (includeRelations == true && x.buildManual.name.Contains(anyStringContains))
			       || (includeRelations == true && x.buildManual.description.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.BuildManualPage> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.BuildManualPage buildManualPage in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(buildManualPage, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.BuildManualPage Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.BuildManualPage Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of BuildManualPages filtered by the parameters provided.  Its query is similar to the GetBuildManualPages method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildManualPages/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? buildManualId = null,
			int? pageNum = null,
			string title = null,
			string notes = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 20, cancellationToken);
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


			IQueryable<Database.BuildManualPage> query = (from bmp in _context.BuildManualPages select bmp);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (buildManualId.HasValue == true)
			{
				query = query.Where(bmp => bmp.buildManualId == buildManualId.Value);
			}
			if (pageNum.HasValue == true)
			{
				query = query.Where(bmp => bmp.pageNum == pageNum.Value);
			}
			if (title != null)
			{
				query = query.Where(bmp => bmp.title == title);
			}
			if (notes != null)
			{
				query = query.Where(bmp => bmp.notes == notes);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bmp => bmp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bmp => bmp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bmp => bmp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bmp => bmp.deleted == false);
				}
			}
			else
			{
				query = query.Where(bmp => bmp.active == true);
				query = query.Where(bmp => bmp.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Build Manual Page, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.title.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.buildManual.name.Contains(anyStringContains)
			       || x.buildManual.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single BuildManualPage by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildManualPage/{id}")]
		public async Task<IActionResult> GetBuildManualPage(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 20, cancellationToken);
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
				IQueryable<Database.BuildManualPage> query = (from bmp in _context.BuildManualPages where
							(bmp.id == id) &&
							(userIsAdmin == true || bmp.deleted == false) &&
							(userIsWriter == true || bmp.active == true)
					select bmp);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.buildManual);
					query = query.AsSplitQuery();
				}

				Database.BuildManualPage materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.BuildManualPage Entity was read with Admin privilege." : "BMC.BuildManualPage Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BuildManualPage", materialized.id, materialized.title));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.BuildManualPage entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.BuildManualPage.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.BuildManualPage.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing BuildManualPage record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/BuildManualPage/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutBuildManualPage(int id, [FromBody]Database.BuildManualPage.BuildManualPageDTO buildManualPageDTO, CancellationToken cancellationToken = default)
		{
			if (buildManualPageDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Instruction Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Instruction Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != buildManualPageDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 20, cancellationToken);
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


			IQueryable<Database.BuildManualPage> query = (from x in _context.BuildManualPages
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.BuildManualPage existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BuildManualPage PUT", id.ToString(), new Exception("No BMC.BuildManualPage entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (buildManualPageDTO.objectGuid == Guid.Empty)
            {
                buildManualPageDTO.objectGuid = existing.objectGuid;
            }
            else if (buildManualPageDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a BuildManualPage record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.BuildManualPage cloneOfExisting = (Database.BuildManualPage)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new BuildManualPage object using the data from the existing record, updated with what is in the DTO.
			//
			Database.BuildManualPage buildManualPage = (Database.BuildManualPage)_context.Entry(existing).GetDatabaseValues().ToObject();
			buildManualPage.ApplyDTO(buildManualPageDTO);
			//
			// The tenant guid for any BuildManualPage being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the BuildManualPage because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				buildManualPage.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (buildManualPage.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.BuildManualPage record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (buildManualPage.title != null && buildManualPage.title.Length > 250)
			{
				buildManualPage.title = buildManualPage.title.Substring(0, 250);
			}

			EntityEntry<Database.BuildManualPage> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(buildManualPage);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.BuildManualPage entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BuildManualPage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BuildManualPage.CreateAnonymousWithFirstLevelSubObjects(buildManualPage)),
					null);


				return Ok(Database.BuildManualPage.CreateAnonymous(buildManualPage));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.BuildManualPage entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BuildManualPage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BuildManualPage.CreateAnonymousWithFirstLevelSubObjects(buildManualPage)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new BuildManualPage record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildManualPage", Name = "BuildManualPage")]
		public async Task<IActionResult> PostBuildManualPage([FromBody]Database.BuildManualPage.BuildManualPageDTO buildManualPageDTO, CancellationToken cancellationToken = default)
		{
			if (buildManualPageDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Instruction Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Instruction Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new BuildManualPage object using the data from the DTO
			//
			Database.BuildManualPage buildManualPage = Database.BuildManualPage.FromDTO(buildManualPageDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				buildManualPage.tenantGuid = userTenantGuid;

				if (buildManualPage.title != null && buildManualPage.title.Length > 250)
				{
					buildManualPage.title = buildManualPage.title.Substring(0, 250);
				}

				buildManualPage.objectGuid = Guid.NewGuid();
				_context.BuildManualPages.Add(buildManualPage);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.BuildManualPage entity successfully created.",
					true,
					buildManualPage.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.BuildManualPage.CreateAnonymousWithFirstLevelSubObjects(buildManualPage)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.BuildManualPage entity creation failed.", false, buildManualPage.id.ToString(), "", JsonSerializer.Serialize(buildManualPage), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BuildManualPage", buildManualPage.id, buildManualPage.title));

			return CreatedAtRoute("BuildManualPage", new { id = buildManualPage.id }, Database.BuildManualPage.CreateAnonymousWithFirstLevelSubObjects(buildManualPage));
		}



        /// <summary>
        /// 
        /// This deletes a BuildManualPage record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildManualPage/{id}")]
		[Route("api/BuildManualPage")]
		public async Task<IActionResult> DeleteBuildManualPage(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Instruction Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Instruction Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.BuildManualPage> query = (from x in _context.BuildManualPages
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.BuildManualPage buildManualPage = await query.FirstOrDefaultAsync(cancellationToken);

			if (buildManualPage == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BuildManualPage DELETE", id.ToString(), new Exception("No BMC.BuildManualPage entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.BuildManualPage cloneOfExisting = (Database.BuildManualPage)_context.Entry(buildManualPage).GetDatabaseValues().ToObject();


			try
			{
				buildManualPage.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BuildManualPage entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BuildManualPage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BuildManualPage.CreateAnonymousWithFirstLevelSubObjects(buildManualPage)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BuildManualPage entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BuildManualPage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BuildManualPage.CreateAnonymousWithFirstLevelSubObjects(buildManualPage)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of BuildManualPage records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/BuildManualPages/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? buildManualId = null,
			int? pageNum = null,
			string title = null,
			string notes = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 20, cancellationToken);


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

			IQueryable<Database.BuildManualPage> query = (from bmp in _context.BuildManualPages select bmp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (buildManualId.HasValue == true)
			{
				query = query.Where(bmp => bmp.buildManualId == buildManualId.Value);
			}
			if (pageNum.HasValue == true)
			{
				query = query.Where(bmp => bmp.pageNum == pageNum.Value);
			}
			if (string.IsNullOrEmpty(title) == false)
			{
				query = query.Where(bmp => bmp.title == title);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(bmp => bmp.notes == notes);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bmp => bmp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bmp => bmp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bmp => bmp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bmp => bmp.deleted == false);
				}
			}
			else
			{
				query = query.Where(bmp => bmp.active == true);
				query = query.Where(bmp => bmp.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Build Manual Page, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.title.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.buildManual.name.Contains(anyStringContains)
			       || x.buildManual.description.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.title);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.BuildManualPage.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/BuildManualPage/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Instruction Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Instruction Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
