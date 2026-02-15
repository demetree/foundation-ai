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
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object projectPutSyncRoot = new object();
		static object projectDeleteSyncRoot = new object();

		private BMCContext _context;

		private ILogger<ProjectsController> _logger;

		public ProjectsController(BMCContext context, ILogger<ProjectsController> logger) : base("BMC", "Project")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Projects filtered by the parameters provided.
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
		[Route("api/Projects")]
		public async Task<IActionResult> GetProjects(
			string name = null,
			string description = null,
			string notes = null,
			string thumbnailImagePath = null,
			int? partCount = null,
			DateTime? lastBuildDate = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (lastBuildDate.HasValue == true && lastBuildDate.Value.Kind != DateTimeKind.Utc)
			{
				lastBuildDate = lastBuildDate.Value.ToUniversalTime();
			}

			IQueryable<Database.Project> query = (from p in _context.Projects select p);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(p => p.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(p => p.description == description);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(p => p.notes == notes);
			}
			if (string.IsNullOrEmpty(thumbnailImagePath) == false)
			{
				query = query.Where(p => p.thumbnailImagePath == thumbnailImagePath);
			}
			if (partCount.HasValue == true)
			{
				query = query.Where(p => p.partCount == partCount.Value);
			}
			if (lastBuildDate.HasValue == true)
			{
				query = query.Where(p => p.lastBuildDate == lastBuildDate.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(p => p.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(p => p.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(p => p.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(p => p.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(p => p.deleted == false);
				}
			}
			else
			{
				query = query.Where(p => p.active == true);
				query = query.Where(p => p.deleted == false);
			}

			query = query.OrderBy(p => p.name).ThenBy(p => p.description).ThenBy(p => p.thumbnailImagePath);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Project, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.thumbnailImagePath.Contains(anyStringContains)
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.Project> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Project project in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(project, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.Project Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.Project Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Projects filtered by the parameters provided.  Its query is similar to the GetProjects method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Projects/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			string notes = null,
			string thumbnailImagePath = null,
			int? partCount = null,
			DateTime? lastBuildDate = null,
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


			//
			// Fix any non-UTC date parameters that come in.
			//
			if (lastBuildDate.HasValue == true && lastBuildDate.Value.Kind != DateTimeKind.Utc)
			{
				lastBuildDate = lastBuildDate.Value.ToUniversalTime();
			}

			IQueryable<Database.Project> query = (from p in _context.Projects select p);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(p => p.name == name);
			}
			if (description != null)
			{
				query = query.Where(p => p.description == description);
			}
			if (notes != null)
			{
				query = query.Where(p => p.notes == notes);
			}
			if (thumbnailImagePath != null)
			{
				query = query.Where(p => p.thumbnailImagePath == thumbnailImagePath);
			}
			if (partCount.HasValue == true)
			{
				query = query.Where(p => p.partCount == partCount.Value);
			}
			if (lastBuildDate.HasValue == true)
			{
				query = query.Where(p => p.lastBuildDate == lastBuildDate.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(p => p.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(p => p.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(p => p.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(p => p.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(p => p.deleted == false);
				}
			}
			else
			{
				query = query.Where(p => p.active == true);
				query = query.Where(p => p.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Project, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.thumbnailImagePath.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single Project by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Project/{id}")]
		public async Task<IActionResult> GetProject(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.Project> query = (from p in _context.Projects where
							(p.id == id) &&
							(userIsAdmin == true || p.deleted == false) &&
							(userIsWriter == true || p.active == true)
					select p);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.Project materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.Project Entity was read with Admin privilege." : "BMC.Project Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Project", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.Project entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.Project.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.Project.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing Project record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Project/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutProject(int id, [FromBody]Database.Project.ProjectDTO projectDTO, CancellationToken cancellationToken = default)
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



			if (id != projectDTO.id)
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


			IQueryable<Database.Project> query = (from x in _context.Projects
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Project existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.Project PUT", id.ToString(), new Exception("No BMC.Project entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (projectDTO.objectGuid == Guid.Empty)
            {
                projectDTO.objectGuid = existing.objectGuid;
            }
            else if (projectDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Project record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Project cloneOfExisting = (Database.Project)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Project object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Project project = (Database.Project)_context.Entry(existing).GetDatabaseValues().ToObject();
			project.ApplyDTO(projectDTO);
			//
			// The tenant guid for any Project being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the Project because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				project.tenantGuid = existing.tenantGuid;
			}

			lock (projectPutSyncRoot)
			{
				//
				// Validate the version number for the project being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != project.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Project save attempt was made but save request was with version " + project.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The Project you are trying to update has already changed.  Please try your save again after reloading the Project.");
				}
				else
				{
					// Same record.  Increase version.
					project.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (project.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.Project record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

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

				try
				{
				    EntityEntry<Database.Project> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(project);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ProjectChangeHistory projectChangeHistory = new ProjectChangeHistory();
				        projectChangeHistory.projectId = project.id;
				        projectChangeHistory.versionNumber = project.versionNumber;
				        projectChangeHistory.timeStamp = DateTime.UtcNow;
				        projectChangeHistory.userId = securityUser.id;
				        projectChangeHistory.tenantGuid = userTenantGuid;
				        projectChangeHistory.data = JsonSerializer.Serialize(Database.Project.CreateAnonymousWithFirstLevelSubObjects(project));
				        _context.ProjectChangeHistories.Add(projectChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.Project entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Project.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Project.CreateAnonymousWithFirstLevelSubObjects(project)),
						null);

				return Ok(Database.Project.CreateAnonymous(project));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.Project entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Project.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Project.CreateAnonymousWithFirstLevelSubObjects(project)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

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
					project.BrickConnections = null;
					project.BuildManuals = null;
					project.PlacedBricks = null;
					project.ProjectCameraPresets = null;
					project.ProjectChangeHistories = null;
					project.ProjectExports = null;
					project.ProjectReferenceImages = null;
					project.ProjectRenders = null;
					project.ProjectTagAssignments = null;
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



        /// <summary>
        /// 
        /// This rolls a Project entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Project/Rollback/{id}")]
		[Route("api/Project/Rollback")]
		public async Task<IActionResult> RollbackToProjectVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.Project> query = (from x in _context.Projects
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this Project concurrently
			//
			lock (projectPutSyncRoot)
			{
				
				Database.Project project = query.FirstOrDefault();
				
				if (project == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.Project rollback", id.ToString(), new Exception("No BMC.Project entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the Project current state so we can log it.
				//
				Database.Project cloneOfExisting = (Database.Project)_context.Entry(project).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.BrickConnections = null;
				cloneOfExisting.BuildManuals = null;
				cloneOfExisting.PlacedBricks = null;
				cloneOfExisting.ProjectCameraPresets = null;
				cloneOfExisting.ProjectChangeHistories = null;
				cloneOfExisting.ProjectExports = null;
				cloneOfExisting.ProjectReferenceImages = null;
				cloneOfExisting.ProjectRenders = null;
				cloneOfExisting.ProjectTagAssignments = null;
				cloneOfExisting.Submodels = null;

				if (versionNumber >= project.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for BMC.Project rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for BMC.Project rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ProjectChangeHistory projectChangeHistory = (from x in _context.ProjectChangeHistories
				                                               where
				                                               x.projectId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (projectChangeHistory != null)
				{
				    Database.Project oldProject = JsonSerializer.Deserialize<Database.Project>(projectChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    project.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    project.name = oldProject.name;
				    project.description = oldProject.description;
				    project.notes = oldProject.notes;
				    project.thumbnailImagePath = oldProject.thumbnailImagePath;
				    project.partCount = oldProject.partCount;
				    project.lastBuildDate = oldProject.lastBuildDate;
				    project.objectGuid = oldProject.objectGuid;
				    project.active = oldProject.active;
				    project.deleted = oldProject.deleted;

				    string serializedProject = JsonSerializer.Serialize(project);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ProjectChangeHistory newProjectChangeHistory = new ProjectChangeHistory();
				        newProjectChangeHistory.projectId = project.id;
				        newProjectChangeHistory.versionNumber = project.versionNumber;
				        newProjectChangeHistory.timeStamp = DateTime.UtcNow;
				        newProjectChangeHistory.userId = securityUser.id;
				        newProjectChangeHistory.tenantGuid = userTenantGuid;
				        newProjectChangeHistory.data = JsonSerializer.Serialize(Database.Project.CreateAnonymousWithFirstLevelSubObjects(project));
				        _context.ProjectChangeHistories.Add(newProjectChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.Project rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Project.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Project.CreateAnonymousWithFirstLevelSubObjects(project)),
						null);


				    return Ok(Database.Project.CreateAnonymous(project));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for BMC.Project rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for BMC.Project rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a Project.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Project</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Project/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetProjectChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.Project project = await _context.Projects.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (project == null)
			{
				return NotFound();
			}

			try
			{
				project.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Project> versionInfo = await project.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a Project.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Project</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Project/{id}/AuditHistory")]
		public async Task<IActionResult> GetProjectAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.Project project = await _context.Projects.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (project == null)
			{
				return NotFound();
			}

			try
			{
				project.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.Project>> versions = await project.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a Project.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Project</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The Project object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Project/{id}/Version/{version}")]
		public async Task<IActionResult> GetProjectVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.Project project = await _context.Projects.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (project == null)
			{
				return NotFound();
			}

			try
			{
				project.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Project> versionInfo = await project.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a Project at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Project</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The Project object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Project/{id}/StateAtTime")]
		public async Task<IActionResult> GetProjectStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.Project project = await _context.Projects.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (project == null)
			{
				return NotFound();
			}

			try
			{
				project.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Project> versionInfo = await project.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a Project record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Project/{id}")]
		[Route("api/Project")]
		public async Task<IActionResult> DeleteProject(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.Project> query = (from x in _context.Projects
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Project project = await query.FirstOrDefaultAsync(cancellationToken);

			if (project == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.Project DELETE", id.ToString(), new Exception("No BMC.Project entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Project cloneOfExisting = (Database.Project)_context.Entry(project).GetDatabaseValues().ToObject();


			lock (projectDeleteSyncRoot)
			{
			    try
			    {
			        project.deleted = true;
			        project.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ProjectChangeHistory projectChangeHistory = new ProjectChangeHistory();
			        projectChangeHistory.projectId = project.id;
			        projectChangeHistory.versionNumber = project.versionNumber;
			        projectChangeHistory.timeStamp = DateTime.UtcNow;
			        projectChangeHistory.userId = securityUser.id;
			        projectChangeHistory.tenantGuid = userTenantGuid;
			        projectChangeHistory.data = JsonSerializer.Serialize(Database.Project.CreateAnonymousWithFirstLevelSubObjects(project));
			        _context.ProjectChangeHistories.Add(projectChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.Project entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Project.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Project.CreateAnonymousWithFirstLevelSubObjects(project)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.Project entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Project.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Project.CreateAnonymousWithFirstLevelSubObjects(project)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of Project records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Projects/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			string notes = null,
			string thumbnailImagePath = null,
			int? partCount = null,
			DateTime? lastBuildDate = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (lastBuildDate.HasValue == true && lastBuildDate.Value.Kind != DateTimeKind.Utc)
			{
				lastBuildDate = lastBuildDate.Value.ToUniversalTime();
			}

			IQueryable<Database.Project> query = (from p in _context.Projects select p);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(p => p.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(p => p.description == description);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(p => p.notes == notes);
			}
			if (string.IsNullOrEmpty(thumbnailImagePath) == false)
			{
				query = query.Where(p => p.thumbnailImagePath == thumbnailImagePath);
			}
			if (partCount.HasValue == true)
			{
				query = query.Where(p => p.partCount == partCount.Value);
			}
			if (lastBuildDate.HasValue == true)
			{
				query = query.Where(p => p.lastBuildDate == lastBuildDate.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(p => p.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(p => p.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(p => p.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(p => p.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(p => p.deleted == false);
				}
			}
			else
			{
				query = query.Where(p => p.active == true);
				query = query.Where(p => p.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Project, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.thumbnailImagePath.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.thumbnailImagePath);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Project.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Project/CreateAuditEvent")]
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
