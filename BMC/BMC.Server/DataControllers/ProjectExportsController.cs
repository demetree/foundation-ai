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
    /// This auto generated class provides the basic CRUD operations for the ProjectExport entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ProjectExport entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ProjectExportsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		private BMCContext _context;

		private ILogger<ProjectExportsController> _logger;

		public ProjectExportsController(BMCContext context, ILogger<ProjectExportsController> logger) : base("BMC", "ProjectExport")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ProjectExports filtered by the parameters provided.
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
		[Route("api/ProjectExports")]
		public async Task<IActionResult> GetProjectExports(
			int? projectId = null,
			int? exportFormatId = null,
			string name = null,
			string outputFilePath = null,
			DateTime? exportedDate = null,
			bool? includeInstructions = null,
			bool? includePartsList = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (exportedDate.HasValue == true && exportedDate.Value.Kind != DateTimeKind.Utc)
			{
				exportedDate = exportedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ProjectExport> query = (from pe in _context.ProjectExports select pe);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (projectId.HasValue == true)
			{
				query = query.Where(pe => pe.projectId == projectId.Value);
			}
			if (exportFormatId.HasValue == true)
			{
				query = query.Where(pe => pe.exportFormatId == exportFormatId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(pe => pe.name == name);
			}
			if (string.IsNullOrEmpty(outputFilePath) == false)
			{
				query = query.Where(pe => pe.outputFilePath == outputFilePath);
			}
			if (exportedDate.HasValue == true)
			{
				query = query.Where(pe => pe.exportedDate == exportedDate.Value);
			}
			if (includeInstructions.HasValue == true)
			{
				query = query.Where(pe => pe.includeInstructions == includeInstructions.Value);
			}
			if (includePartsList.HasValue == true)
			{
				query = query.Where(pe => pe.includePartsList == includePartsList.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pe => pe.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pe => pe.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pe => pe.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pe => pe.deleted == false);
				}
			}
			else
			{
				query = query.Where(pe => pe.active == true);
				query = query.Where(pe => pe.deleted == false);
			}

			query = query.OrderBy(pe => pe.name).ThenBy(pe => pe.outputFilePath);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.exportFormat);
				query = query.Include(x => x.project);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Project Export, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.outputFilePath.Contains(anyStringContains)
			       || (includeRelations == true && x.exportFormat.name.Contains(anyStringContains))
			       || (includeRelations == true && x.exportFormat.description.Contains(anyStringContains))
			       || (includeRelations == true && x.exportFormat.fileExtension.Contains(anyStringContains))
			       || (includeRelations == true && x.project.name.Contains(anyStringContains))
			       || (includeRelations == true && x.project.description.Contains(anyStringContains))
			       || (includeRelations == true && x.project.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.project.thumbnailImagePath.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.ProjectExport> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ProjectExport projectExport in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(projectExport, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.ProjectExport Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.ProjectExport Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ProjectExports filtered by the parameters provided.  Its query is similar to the GetProjectExports method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ProjectExports/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? projectId = null,
			int? exportFormatId = null,
			string name = null,
			string outputFilePath = null,
			DateTime? exportedDate = null,
			bool? includeInstructions = null,
			bool? includePartsList = null,
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


			//
			// Fix any non-UTC date parameters that come in.
			//
			if (exportedDate.HasValue == true && exportedDate.Value.Kind != DateTimeKind.Utc)
			{
				exportedDate = exportedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ProjectExport> query = (from pe in _context.ProjectExports select pe);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (projectId.HasValue == true)
			{
				query = query.Where(pe => pe.projectId == projectId.Value);
			}
			if (exportFormatId.HasValue == true)
			{
				query = query.Where(pe => pe.exportFormatId == exportFormatId.Value);
			}
			if (name != null)
			{
				query = query.Where(pe => pe.name == name);
			}
			if (outputFilePath != null)
			{
				query = query.Where(pe => pe.outputFilePath == outputFilePath);
			}
			if (exportedDate.HasValue == true)
			{
				query = query.Where(pe => pe.exportedDate == exportedDate.Value);
			}
			if (includeInstructions.HasValue == true)
			{
				query = query.Where(pe => pe.includeInstructions == includeInstructions.Value);
			}
			if (includePartsList.HasValue == true)
			{
				query = query.Where(pe => pe.includePartsList == includePartsList.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pe => pe.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pe => pe.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pe => pe.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pe => pe.deleted == false);
				}
			}
			else
			{
				query = query.Where(pe => pe.active == true);
				query = query.Where(pe => pe.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Project Export, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.outputFilePath.Contains(anyStringContains)
			       || x.exportFormat.name.Contains(anyStringContains)
			       || x.exportFormat.description.Contains(anyStringContains)
			       || x.exportFormat.fileExtension.Contains(anyStringContains)
			       || x.project.name.Contains(anyStringContains)
			       || x.project.description.Contains(anyStringContains)
			       || x.project.notes.Contains(anyStringContains)
			       || x.project.thumbnailImagePath.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ProjectExport by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ProjectExport/{id}")]
		public async Task<IActionResult> GetProjectExport(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ProjectExport> query = (from pe in _context.ProjectExports where
							(pe.id == id) &&
							(userIsAdmin == true || pe.deleted == false) &&
							(userIsWriter == true || pe.active == true)
					select pe);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.exportFormat);
					query = query.Include(x => x.project);
					query = query.AsSplitQuery();
				}

				Database.ProjectExport materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.ProjectExport Entity was read with Admin privilege." : "BMC.ProjectExport Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ProjectExport", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.ProjectExport entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.ProjectExport.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.ProjectExport.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ProjectExport record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ProjectExport/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutProjectExport(int id, [FromBody]Database.ProjectExport.ProjectExportDTO projectExportDTO, CancellationToken cancellationToken = default)
		{
			if (projectExportDTO == null)
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



			if (id != projectExportDTO.id)
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


			IQueryable<Database.ProjectExport> query = (from x in _context.ProjectExports
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ProjectExport existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ProjectExport PUT", id.ToString(), new Exception("No BMC.ProjectExport entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (projectExportDTO.objectGuid == Guid.Empty)
            {
                projectExportDTO.objectGuid = existing.objectGuid;
            }
            else if (projectExportDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ProjectExport record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ProjectExport cloneOfExisting = (Database.ProjectExport)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ProjectExport object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ProjectExport projectExport = (Database.ProjectExport)_context.Entry(existing).GetDatabaseValues().ToObject();
			projectExport.ApplyDTO(projectExportDTO);
			//
			// The tenant guid for any ProjectExport being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ProjectExport because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				projectExport.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (projectExport.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.ProjectExport record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (projectExport.name != null && projectExport.name.Length > 100)
			{
				projectExport.name = projectExport.name.Substring(0, 100);
			}

			if (projectExport.outputFilePath != null && projectExport.outputFilePath.Length > 250)
			{
				projectExport.outputFilePath = projectExport.outputFilePath.Substring(0, 250);
			}

			if (projectExport.exportedDate.HasValue == true && projectExport.exportedDate.Value.Kind != DateTimeKind.Utc)
			{
				projectExport.exportedDate = projectExport.exportedDate.Value.ToUniversalTime();
			}

			EntityEntry<Database.ProjectExport> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(projectExport);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.ProjectExport entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ProjectExport.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ProjectExport.CreateAnonymousWithFirstLevelSubObjects(projectExport)),
					null);


				return Ok(Database.ProjectExport.CreateAnonymous(projectExport));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.ProjectExport entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ProjectExport.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ProjectExport.CreateAnonymousWithFirstLevelSubObjects(projectExport)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new ProjectExport record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ProjectExport", Name = "ProjectExport")]
		public async Task<IActionResult> PostProjectExport([FromBody]Database.ProjectExport.ProjectExportDTO projectExportDTO, CancellationToken cancellationToken = default)
		{
			if (projectExportDTO == null)
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
			// Create a new ProjectExport object using the data from the DTO
			//
			Database.ProjectExport projectExport = Database.ProjectExport.FromDTO(projectExportDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				projectExport.tenantGuid = userTenantGuid;

				if (projectExport.name != null && projectExport.name.Length > 100)
				{
					projectExport.name = projectExport.name.Substring(0, 100);
				}

				if (projectExport.outputFilePath != null && projectExport.outputFilePath.Length > 250)
				{
					projectExport.outputFilePath = projectExport.outputFilePath.Substring(0, 250);
				}

				if (projectExport.exportedDate.HasValue == true && projectExport.exportedDate.Value.Kind != DateTimeKind.Utc)
				{
					projectExport.exportedDate = projectExport.exportedDate.Value.ToUniversalTime();
				}

				projectExport.objectGuid = Guid.NewGuid();
				_context.ProjectExports.Add(projectExport);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.ProjectExport entity successfully created.",
					true,
					projectExport.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ProjectExport.CreateAnonymousWithFirstLevelSubObjects(projectExport)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.ProjectExport entity creation failed.", false, projectExport.id.ToString(), "", JsonSerializer.Serialize(projectExport), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ProjectExport", projectExport.id, projectExport.name));

			return CreatedAtRoute("ProjectExport", new { id = projectExport.id }, Database.ProjectExport.CreateAnonymousWithFirstLevelSubObjects(projectExport));
		}



        /// <summary>
        /// 
        /// This deletes a ProjectExport record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ProjectExport/{id}")]
		[Route("api/ProjectExport")]
		public async Task<IActionResult> DeleteProjectExport(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ProjectExport> query = (from x in _context.ProjectExports
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ProjectExport projectExport = await query.FirstOrDefaultAsync(cancellationToken);

			if (projectExport == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ProjectExport DELETE", id.ToString(), new Exception("No BMC.ProjectExport entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ProjectExport cloneOfExisting = (Database.ProjectExport)_context.Entry(projectExport).GetDatabaseValues().ToObject();


			try
			{
				projectExport.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ProjectExport entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ProjectExport.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ProjectExport.CreateAnonymousWithFirstLevelSubObjects(projectExport)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ProjectExport entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ProjectExport.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ProjectExport.CreateAnonymousWithFirstLevelSubObjects(projectExport)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of ProjectExport records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ProjectExports/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? projectId = null,
			int? exportFormatId = null,
			string name = null,
			string outputFilePath = null,
			DateTime? exportedDate = null,
			bool? includeInstructions = null,
			bool? includePartsList = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (exportedDate.HasValue == true && exportedDate.Value.Kind != DateTimeKind.Utc)
			{
				exportedDate = exportedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ProjectExport> query = (from pe in _context.ProjectExports select pe);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (projectId.HasValue == true)
			{
				query = query.Where(pe => pe.projectId == projectId.Value);
			}
			if (exportFormatId.HasValue == true)
			{
				query = query.Where(pe => pe.exportFormatId == exportFormatId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(pe => pe.name == name);
			}
			if (string.IsNullOrEmpty(outputFilePath) == false)
			{
				query = query.Where(pe => pe.outputFilePath == outputFilePath);
			}
			if (exportedDate.HasValue == true)
			{
				query = query.Where(pe => pe.exportedDate == exportedDate.Value);
			}
			if (includeInstructions.HasValue == true)
			{
				query = query.Where(pe => pe.includeInstructions == includeInstructions.Value);
			}
			if (includePartsList.HasValue == true)
			{
				query = query.Where(pe => pe.includePartsList == includePartsList.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pe => pe.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pe => pe.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pe => pe.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pe => pe.deleted == false);
				}
			}
			else
			{
				query = query.Where(pe => pe.active == true);
				query = query.Where(pe => pe.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Project Export, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.outputFilePath.Contains(anyStringContains)
			       || x.exportFormat.name.Contains(anyStringContains)
			       || x.exportFormat.description.Contains(anyStringContains)
			       || x.exportFormat.fileExtension.Contains(anyStringContains)
			       || x.project.name.Contains(anyStringContains)
			       || x.project.description.Contains(anyStringContains)
			       || x.project.notes.Contains(anyStringContains)
			       || x.project.thumbnailImagePath.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name).ThenBy(x => x.outputFilePath);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ProjectExport.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ProjectExport/CreateAuditEvent")]
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
