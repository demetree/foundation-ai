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
    /// This auto generated class provides the basic CRUD operations for the Project entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Project entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ProjectsController : SecureWebAPIController
	{

        /// <summary>
        /// 
        /// This creates a new Project record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Project", Name = "Project")]
		public async Task<IActionResult> PostProject([FromBody]Database.Project.ProjectDTO projectDTO, CancellationToken cancellationToken = default)
		{
			if (projectDTO == null)
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
			// Create a new Project object using the data from the DTO
			//
			Database.Project project = Database.Project.FromDTO(projectDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				project.tenantGuid = userTenantGuid;

				if (project.name != null && project.name.Length > 100)
				{
					project.name = project.name.Substring(0, 100);
				}

				if (project.description != null && project.description.Length > 500)
				{
					project.description = project.description.Substring(0, 500);
				}

				if (project.thumbnailImagePath != null && project.thumbnailImagePath.Length > 250)
				{
					project.thumbnailImagePath = project.thumbnailImagePath.Substring(0, 250);
				}

				if (project.lastBuildDate.HasValue == true && project.lastBuildDate.Value.Kind != DateTimeKind.Utc)
				{
					project.lastBuildDate = project.lastBuildDate.Value.ToUniversalTime();
				}

				project.objectGuid = Guid.NewGuid();
				project.versionNumber = 1;

				_context.Projects.Add(project);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the project object so that no further changes will be written to the database
				    //
				    _context.Entry(project).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					project.thumbnailData = null;
					project.BrickConnections = null;
					project.BuildManuals = null;
					project.CompiledGlbs = null;
					project.ModelDocuments = null;
					project.PlacedBricks = null;
					project.ProjectCameraPresets = null;
					project.ProjectChangeHistories = null;
					project.ProjectExports = null;
					project.ProjectReferenceImages = null;
					project.ProjectRenders = null;
					project.ProjectTagAssignments = null;
					project.PublishedMocs = null;
					project.Submodels = null;


				    ProjectChangeHistory projectChangeHistory = new ProjectChangeHistory();
				    projectChangeHistory.projectId = project.id;
				    projectChangeHistory.versionNumber = project.versionNumber;
				    projectChangeHistory.timeStamp = DateTime.UtcNow;
				    projectChangeHistory.userId = securityUser.id;
				    projectChangeHistory.tenantGuid = userTenantGuid;
				    projectChangeHistory.data = JsonSerializer.Serialize(Database.Project.CreateAnonymousWithFirstLevelSubObjects(project));
				    _context.ProjectChangeHistories.Add(projectChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"BMC.Project entity successfully created.",
						true,
						project. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.Project.CreateAnonymousWithFirstLevelSubObjects(project)),
						null);

                    //
                    // Automatically create an initial Build Manual and Page for this new project
                    //
                    Database.BuildManual initialManual = new Database.BuildManual
                    {
                        projectId = project.id,
                        tenantGuid = userTenantGuid,
                        name = ((project.name?.Length ?? 0) > 88 ? project.name.Substring(0, 88) : project.name ?? "New") + " — Manual",
                        description = "Auto-generated manual for your project.",
                        pageWidthMm = 210, // A4 width
                        pageHeightMm = 297, // A4 height
                        isPublished = false,
                        objectGuid = Guid.NewGuid(),
                        versionNumber = 1,
                        active = true,
                        deleted = false
                    };

                    _context.BuildManuals.Add(initialManual);
                    await _context.SaveChangesAsync(cancellationToken);

                    Database.BuildManualPage initialPage = new Database.BuildManualPage
                    {
                        buildManualId = initialManual.id,
                        tenantGuid = userTenantGuid,
                        pageNum = 1,
                        title = "Getting Started",
                        backgroundTheme = "Blueprint",
                        layoutPreset = "SingleStep",
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };

                    _context.BuildManualPages.Add(initialPage);
                    await _context.SaveChangesAsync(cancellationToken);



				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.Project entity creation failed.", false, project.id.ToString(), "", JsonSerializer.Serialize(Database.Project.CreateAnonymousWithFirstLevelSubObjects(project)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Project", project.id, project.name));

			return CreatedAtRoute("Project", new { id = project.id }, Database.Project.CreateAnonymousWithFirstLevelSubObjects(project));
		}


	}
}
