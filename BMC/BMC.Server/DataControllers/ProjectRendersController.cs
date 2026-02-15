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
    /// This auto generated class provides the basic CRUD operations for the ProjectRender entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ProjectRender entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ProjectRendersController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		private BMCContext _context;

		private ILogger<ProjectRendersController> _logger;

		public ProjectRendersController(BMCContext context, ILogger<ProjectRendersController> logger) : base("BMC", "ProjectRender")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ProjectRenders filtered by the parameters provided.
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
		[Route("api/ProjectRenders")]
		public async Task<IActionResult> GetProjectRenders(
			int? projectId = null,
			int? renderPresetId = null,
			string name = null,
			string outputFilePath = null,
			int? resolutionWidth = null,
			int? resolutionHeight = null,
			DateTime? renderedDate = null,
			float? renderDurationSeconds = null,
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
			if (renderedDate.HasValue == true && renderedDate.Value.Kind != DateTimeKind.Utc)
			{
				renderedDate = renderedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ProjectRender> query = (from pr in _context.ProjectRenders select pr);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (projectId.HasValue == true)
			{
				query = query.Where(pr => pr.projectId == projectId.Value);
			}
			if (renderPresetId.HasValue == true)
			{
				query = query.Where(pr => pr.renderPresetId == renderPresetId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(pr => pr.name == name);
			}
			if (string.IsNullOrEmpty(outputFilePath) == false)
			{
				query = query.Where(pr => pr.outputFilePath == outputFilePath);
			}
			if (resolutionWidth.HasValue == true)
			{
				query = query.Where(pr => pr.resolutionWidth == resolutionWidth.Value);
			}
			if (resolutionHeight.HasValue == true)
			{
				query = query.Where(pr => pr.resolutionHeight == resolutionHeight.Value);
			}
			if (renderedDate.HasValue == true)
			{
				query = query.Where(pr => pr.renderedDate == renderedDate.Value);
			}
			if (renderDurationSeconds.HasValue == true)
			{
				query = query.Where(pr => pr.renderDurationSeconds == renderDurationSeconds.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pr => pr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pr => pr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pr => pr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pr => pr.deleted == false);
				}
			}
			else
			{
				query = query.Where(pr => pr.active == true);
				query = query.Where(pr => pr.deleted == false);
			}

			query = query.OrderBy(pr => pr.name).ThenBy(pr => pr.outputFilePath);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.project);
				query = query.Include(x => x.renderPreset);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Project Render, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.outputFilePath.Contains(anyStringContains)
			       || (includeRelations == true && x.project.name.Contains(anyStringContains))
			       || (includeRelations == true && x.project.description.Contains(anyStringContains))
			       || (includeRelations == true && x.project.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.project.thumbnailImagePath.Contains(anyStringContains))
			       || (includeRelations == true && x.renderPreset.name.Contains(anyStringContains))
			       || (includeRelations == true && x.renderPreset.description.Contains(anyStringContains))
			       || (includeRelations == true && x.renderPreset.backgroundColorHex.Contains(anyStringContains))
			       || (includeRelations == true && x.renderPreset.lightingMode.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.ProjectRender> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ProjectRender projectRender in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(projectRender, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.ProjectRender Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.ProjectRender Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ProjectRenders filtered by the parameters provided.  Its query is similar to the GetProjectRenders method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ProjectRenders/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? projectId = null,
			int? renderPresetId = null,
			string name = null,
			string outputFilePath = null,
			int? resolutionWidth = null,
			int? resolutionHeight = null,
			DateTime? renderedDate = null,
			float? renderDurationSeconds = null,
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
			if (renderedDate.HasValue == true && renderedDate.Value.Kind != DateTimeKind.Utc)
			{
				renderedDate = renderedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ProjectRender> query = (from pr in _context.ProjectRenders select pr);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (projectId.HasValue == true)
			{
				query = query.Where(pr => pr.projectId == projectId.Value);
			}
			if (renderPresetId.HasValue == true)
			{
				query = query.Where(pr => pr.renderPresetId == renderPresetId.Value);
			}
			if (name != null)
			{
				query = query.Where(pr => pr.name == name);
			}
			if (outputFilePath != null)
			{
				query = query.Where(pr => pr.outputFilePath == outputFilePath);
			}
			if (resolutionWidth.HasValue == true)
			{
				query = query.Where(pr => pr.resolutionWidth == resolutionWidth.Value);
			}
			if (resolutionHeight.HasValue == true)
			{
				query = query.Where(pr => pr.resolutionHeight == resolutionHeight.Value);
			}
			if (renderedDate.HasValue == true)
			{
				query = query.Where(pr => pr.renderedDate == renderedDate.Value);
			}
			if (renderDurationSeconds.HasValue == true)
			{
				query = query.Where(pr => pr.renderDurationSeconds == renderDurationSeconds.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pr => pr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pr => pr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pr => pr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pr => pr.deleted == false);
				}
			}
			else
			{
				query = query.Where(pr => pr.active == true);
				query = query.Where(pr => pr.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Project Render, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.outputFilePath.Contains(anyStringContains)
			       || x.project.name.Contains(anyStringContains)
			       || x.project.description.Contains(anyStringContains)
			       || x.project.notes.Contains(anyStringContains)
			       || x.project.thumbnailImagePath.Contains(anyStringContains)
			       || x.renderPreset.name.Contains(anyStringContains)
			       || x.renderPreset.description.Contains(anyStringContains)
			       || x.renderPreset.backgroundColorHex.Contains(anyStringContains)
			       || x.renderPreset.lightingMode.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ProjectRender by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ProjectRender/{id}")]
		public async Task<IActionResult> GetProjectRender(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ProjectRender> query = (from pr in _context.ProjectRenders where
							(pr.id == id) &&
							(userIsAdmin == true || pr.deleted == false) &&
							(userIsWriter == true || pr.active == true)
					select pr);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.project);
					query = query.Include(x => x.renderPreset);
					query = query.AsSplitQuery();
				}

				Database.ProjectRender materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.ProjectRender Entity was read with Admin privilege." : "BMC.ProjectRender Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ProjectRender", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.ProjectRender entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.ProjectRender.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.ProjectRender.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ProjectRender record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ProjectRender/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutProjectRender(int id, [FromBody]Database.ProjectRender.ProjectRenderDTO projectRenderDTO, CancellationToken cancellationToken = default)
		{
			if (projectRenderDTO == null)
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



			if (id != projectRenderDTO.id)
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


			IQueryable<Database.ProjectRender> query = (from x in _context.ProjectRenders
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ProjectRender existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ProjectRender PUT", id.ToString(), new Exception("No BMC.ProjectRender entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (projectRenderDTO.objectGuid == Guid.Empty)
            {
                projectRenderDTO.objectGuid = existing.objectGuid;
            }
            else if (projectRenderDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ProjectRender record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ProjectRender cloneOfExisting = (Database.ProjectRender)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ProjectRender object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ProjectRender projectRender = (Database.ProjectRender)_context.Entry(existing).GetDatabaseValues().ToObject();
			projectRender.ApplyDTO(projectRenderDTO);
			//
			// The tenant guid for any ProjectRender being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ProjectRender because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				projectRender.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (projectRender.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.ProjectRender record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (projectRender.name != null && projectRender.name.Length > 100)
			{
				projectRender.name = projectRender.name.Substring(0, 100);
			}

			if (projectRender.outputFilePath != null && projectRender.outputFilePath.Length > 250)
			{
				projectRender.outputFilePath = projectRender.outputFilePath.Substring(0, 250);
			}

			if (projectRender.renderedDate.HasValue == true && projectRender.renderedDate.Value.Kind != DateTimeKind.Utc)
			{
				projectRender.renderedDate = projectRender.renderedDate.Value.ToUniversalTime();
			}

			EntityEntry<Database.ProjectRender> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(projectRender);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.ProjectRender entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ProjectRender.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ProjectRender.CreateAnonymousWithFirstLevelSubObjects(projectRender)),
					null);


				return Ok(Database.ProjectRender.CreateAnonymous(projectRender));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.ProjectRender entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ProjectRender.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ProjectRender.CreateAnonymousWithFirstLevelSubObjects(projectRender)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new ProjectRender record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ProjectRender", Name = "ProjectRender")]
		public async Task<IActionResult> PostProjectRender([FromBody]Database.ProjectRender.ProjectRenderDTO projectRenderDTO, CancellationToken cancellationToken = default)
		{
			if (projectRenderDTO == null)
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
			// Create a new ProjectRender object using the data from the DTO
			//
			Database.ProjectRender projectRender = Database.ProjectRender.FromDTO(projectRenderDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				projectRender.tenantGuid = userTenantGuid;

				if (projectRender.name != null && projectRender.name.Length > 100)
				{
					projectRender.name = projectRender.name.Substring(0, 100);
				}

				if (projectRender.outputFilePath != null && projectRender.outputFilePath.Length > 250)
				{
					projectRender.outputFilePath = projectRender.outputFilePath.Substring(0, 250);
				}

				if (projectRender.renderedDate.HasValue == true && projectRender.renderedDate.Value.Kind != DateTimeKind.Utc)
				{
					projectRender.renderedDate = projectRender.renderedDate.Value.ToUniversalTime();
				}

				projectRender.objectGuid = Guid.NewGuid();
				_context.ProjectRenders.Add(projectRender);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.ProjectRender entity successfully created.",
					true,
					projectRender.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ProjectRender.CreateAnonymousWithFirstLevelSubObjects(projectRender)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.ProjectRender entity creation failed.", false, projectRender.id.ToString(), "", JsonSerializer.Serialize(projectRender), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ProjectRender", projectRender.id, projectRender.name));

			return CreatedAtRoute("ProjectRender", new { id = projectRender.id }, Database.ProjectRender.CreateAnonymousWithFirstLevelSubObjects(projectRender));
		}



        /// <summary>
        /// 
        /// This deletes a ProjectRender record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ProjectRender/{id}")]
		[Route("api/ProjectRender")]
		public async Task<IActionResult> DeleteProjectRender(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ProjectRender> query = (from x in _context.ProjectRenders
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ProjectRender projectRender = await query.FirstOrDefaultAsync(cancellationToken);

			if (projectRender == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ProjectRender DELETE", id.ToString(), new Exception("No BMC.ProjectRender entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ProjectRender cloneOfExisting = (Database.ProjectRender)_context.Entry(projectRender).GetDatabaseValues().ToObject();


			try
			{
				projectRender.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ProjectRender entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ProjectRender.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ProjectRender.CreateAnonymousWithFirstLevelSubObjects(projectRender)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ProjectRender entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ProjectRender.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ProjectRender.CreateAnonymousWithFirstLevelSubObjects(projectRender)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of ProjectRender records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ProjectRenders/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? projectId = null,
			int? renderPresetId = null,
			string name = null,
			string outputFilePath = null,
			int? resolutionWidth = null,
			int? resolutionHeight = null,
			DateTime? renderedDate = null,
			float? renderDurationSeconds = null,
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
			if (renderedDate.HasValue == true && renderedDate.Value.Kind != DateTimeKind.Utc)
			{
				renderedDate = renderedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ProjectRender> query = (from pr in _context.ProjectRenders select pr);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (projectId.HasValue == true)
			{
				query = query.Where(pr => pr.projectId == projectId.Value);
			}
			if (renderPresetId.HasValue == true)
			{
				query = query.Where(pr => pr.renderPresetId == renderPresetId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(pr => pr.name == name);
			}
			if (string.IsNullOrEmpty(outputFilePath) == false)
			{
				query = query.Where(pr => pr.outputFilePath == outputFilePath);
			}
			if (resolutionWidth.HasValue == true)
			{
				query = query.Where(pr => pr.resolutionWidth == resolutionWidth.Value);
			}
			if (resolutionHeight.HasValue == true)
			{
				query = query.Where(pr => pr.resolutionHeight == resolutionHeight.Value);
			}
			if (renderedDate.HasValue == true)
			{
				query = query.Where(pr => pr.renderedDate == renderedDate.Value);
			}
			if (renderDurationSeconds.HasValue == true)
			{
				query = query.Where(pr => pr.renderDurationSeconds == renderDurationSeconds.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pr => pr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pr => pr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pr => pr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pr => pr.deleted == false);
				}
			}
			else
			{
				query = query.Where(pr => pr.active == true);
				query = query.Where(pr => pr.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Project Render, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.outputFilePath.Contains(anyStringContains)
			       || x.project.name.Contains(anyStringContains)
			       || x.project.description.Contains(anyStringContains)
			       || x.project.notes.Contains(anyStringContains)
			       || x.project.thumbnailImagePath.Contains(anyStringContains)
			       || x.renderPreset.name.Contains(anyStringContains)
			       || x.renderPreset.description.Contains(anyStringContains)
			       || x.renderPreset.backgroundColorHex.Contains(anyStringContains)
			       || x.renderPreset.lightingMode.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name).ThenBy(x => x.outputFilePath);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ProjectRender.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ProjectRender/CreateAuditEvent")]
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
