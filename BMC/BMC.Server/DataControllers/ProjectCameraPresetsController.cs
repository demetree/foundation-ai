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
    /// This auto generated class provides the basic CRUD operations for the ProjectCameraPreset entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ProjectCameraPreset entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ProjectCameraPresetsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		private BMCContext _context;

		private ILogger<ProjectCameraPresetsController> _logger;

		public ProjectCameraPresetsController(BMCContext context, ILogger<ProjectCameraPresetsController> logger) : base("BMC", "ProjectCameraPreset")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ProjectCameraPresets filtered by the parameters provided.
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
		[Route("api/ProjectCameraPresets")]
		public async Task<IActionResult> GetProjectCameraPresets(
			int? projectId = null,
			string name = null,
			float? positionX = null,
			float? positionY = null,
			float? positionZ = null,
			float? targetX = null,
			float? targetY = null,
			float? targetZ = null,
			float? zoom = null,
			bool? isPerspective = null,
			int? sequence = null,
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

			IQueryable<Database.ProjectCameraPreset> query = (from pcp in _context.ProjectCameraPresets select pcp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (projectId.HasValue == true)
			{
				query = query.Where(pcp => pcp.projectId == projectId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(pcp => pcp.name == name);
			}
			if (positionX.HasValue == true)
			{
				query = query.Where(pcp => pcp.positionX == positionX.Value);
			}
			if (positionY.HasValue == true)
			{
				query = query.Where(pcp => pcp.positionY == positionY.Value);
			}
			if (positionZ.HasValue == true)
			{
				query = query.Where(pcp => pcp.positionZ == positionZ.Value);
			}
			if (targetX.HasValue == true)
			{
				query = query.Where(pcp => pcp.targetX == targetX.Value);
			}
			if (targetY.HasValue == true)
			{
				query = query.Where(pcp => pcp.targetY == targetY.Value);
			}
			if (targetZ.HasValue == true)
			{
				query = query.Where(pcp => pcp.targetZ == targetZ.Value);
			}
			if (zoom.HasValue == true)
			{
				query = query.Where(pcp => pcp.zoom == zoom.Value);
			}
			if (isPerspective.HasValue == true)
			{
				query = query.Where(pcp => pcp.isPerspective == isPerspective.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(pcp => pcp.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pcp => pcp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pcp => pcp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pcp => pcp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pcp => pcp.deleted == false);
				}
			}
			else
			{
				query = query.Where(pcp => pcp.active == true);
				query = query.Where(pcp => pcp.deleted == false);
			}

			query = query.OrderBy(pcp => pcp.sequence).ThenBy(pcp => pcp.name);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.project);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Project Camera Preset, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || (includeRelations == true && x.project.name.Contains(anyStringContains))
			       || (includeRelations == true && x.project.description.Contains(anyStringContains))
			       || (includeRelations == true && x.project.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.project.thumbnailImagePath.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.ProjectCameraPreset> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ProjectCameraPreset projectCameraPreset in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(projectCameraPreset, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.ProjectCameraPreset Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.ProjectCameraPreset Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ProjectCameraPresets filtered by the parameters provided.  Its query is similar to the GetProjectCameraPresets method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ProjectCameraPresets/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? projectId = null,
			string name = null,
			float? positionX = null,
			float? positionY = null,
			float? positionZ = null,
			float? targetX = null,
			float? targetY = null,
			float? targetZ = null,
			float? zoom = null,
			bool? isPerspective = null,
			int? sequence = null,
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


			IQueryable<Database.ProjectCameraPreset> query = (from pcp in _context.ProjectCameraPresets select pcp);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (projectId.HasValue == true)
			{
				query = query.Where(pcp => pcp.projectId == projectId.Value);
			}
			if (name != null)
			{
				query = query.Where(pcp => pcp.name == name);
			}
			if (positionX.HasValue == true)
			{
				query = query.Where(pcp => pcp.positionX == positionX.Value);
			}
			if (positionY.HasValue == true)
			{
				query = query.Where(pcp => pcp.positionY == positionY.Value);
			}
			if (positionZ.HasValue == true)
			{
				query = query.Where(pcp => pcp.positionZ == positionZ.Value);
			}
			if (targetX.HasValue == true)
			{
				query = query.Where(pcp => pcp.targetX == targetX.Value);
			}
			if (targetY.HasValue == true)
			{
				query = query.Where(pcp => pcp.targetY == targetY.Value);
			}
			if (targetZ.HasValue == true)
			{
				query = query.Where(pcp => pcp.targetZ == targetZ.Value);
			}
			if (zoom.HasValue == true)
			{
				query = query.Where(pcp => pcp.zoom == zoom.Value);
			}
			if (isPerspective.HasValue == true)
			{
				query = query.Where(pcp => pcp.isPerspective == isPerspective.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(pcp => pcp.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pcp => pcp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pcp => pcp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pcp => pcp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pcp => pcp.deleted == false);
				}
			}
			else
			{
				query = query.Where(pcp => pcp.active == true);
				query = query.Where(pcp => pcp.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Project Camera Preset, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
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
        /// This gets a single ProjectCameraPreset by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ProjectCameraPreset/{id}")]
		public async Task<IActionResult> GetProjectCameraPreset(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ProjectCameraPreset> query = (from pcp in _context.ProjectCameraPresets where
							(pcp.id == id) &&
							(userIsAdmin == true || pcp.deleted == false) &&
							(userIsWriter == true || pcp.active == true)
					select pcp);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.project);
					query = query.AsSplitQuery();
				}

				Database.ProjectCameraPreset materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.ProjectCameraPreset Entity was read with Admin privilege." : "BMC.ProjectCameraPreset Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ProjectCameraPreset", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.ProjectCameraPreset entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.ProjectCameraPreset.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.ProjectCameraPreset.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ProjectCameraPreset record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ProjectCameraPreset/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutProjectCameraPreset(int id, [FromBody]Database.ProjectCameraPreset.ProjectCameraPresetDTO projectCameraPresetDTO, CancellationToken cancellationToken = default)
		{
			if (projectCameraPresetDTO == null)
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



			if (id != projectCameraPresetDTO.id)
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


			IQueryable<Database.ProjectCameraPreset> query = (from x in _context.ProjectCameraPresets
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ProjectCameraPreset existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ProjectCameraPreset PUT", id.ToString(), new Exception("No BMC.ProjectCameraPreset entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (projectCameraPresetDTO.objectGuid == Guid.Empty)
            {
                projectCameraPresetDTO.objectGuid = existing.objectGuid;
            }
            else if (projectCameraPresetDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ProjectCameraPreset record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ProjectCameraPreset cloneOfExisting = (Database.ProjectCameraPreset)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ProjectCameraPreset object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ProjectCameraPreset projectCameraPreset = (Database.ProjectCameraPreset)_context.Entry(existing).GetDatabaseValues().ToObject();
			projectCameraPreset.ApplyDTO(projectCameraPresetDTO);
			//
			// The tenant guid for any ProjectCameraPreset being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ProjectCameraPreset because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				projectCameraPreset.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (projectCameraPreset.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.ProjectCameraPreset record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (projectCameraPreset.name != null && projectCameraPreset.name.Length > 100)
			{
				projectCameraPreset.name = projectCameraPreset.name.Substring(0, 100);
			}

			EntityEntry<Database.ProjectCameraPreset> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(projectCameraPreset);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.ProjectCameraPreset entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ProjectCameraPreset.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ProjectCameraPreset.CreateAnonymousWithFirstLevelSubObjects(projectCameraPreset)),
					null);


				return Ok(Database.ProjectCameraPreset.CreateAnonymous(projectCameraPreset));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.ProjectCameraPreset entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ProjectCameraPreset.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ProjectCameraPreset.CreateAnonymousWithFirstLevelSubObjects(projectCameraPreset)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new ProjectCameraPreset record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ProjectCameraPreset", Name = "ProjectCameraPreset")]
		public async Task<IActionResult> PostProjectCameraPreset([FromBody]Database.ProjectCameraPreset.ProjectCameraPresetDTO projectCameraPresetDTO, CancellationToken cancellationToken = default)
		{
			if (projectCameraPresetDTO == null)
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
			// Create a new ProjectCameraPreset object using the data from the DTO
			//
			Database.ProjectCameraPreset projectCameraPreset = Database.ProjectCameraPreset.FromDTO(projectCameraPresetDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				projectCameraPreset.tenantGuid = userTenantGuid;

				if (projectCameraPreset.name != null && projectCameraPreset.name.Length > 100)
				{
					projectCameraPreset.name = projectCameraPreset.name.Substring(0, 100);
				}

				projectCameraPreset.objectGuid = Guid.NewGuid();
				_context.ProjectCameraPresets.Add(projectCameraPreset);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.ProjectCameraPreset entity successfully created.",
					true,
					projectCameraPreset.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ProjectCameraPreset.CreateAnonymousWithFirstLevelSubObjects(projectCameraPreset)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.ProjectCameraPreset entity creation failed.", false, projectCameraPreset.id.ToString(), "", JsonSerializer.Serialize(projectCameraPreset), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ProjectCameraPreset", projectCameraPreset.id, projectCameraPreset.name));

			return CreatedAtRoute("ProjectCameraPreset", new { id = projectCameraPreset.id }, Database.ProjectCameraPreset.CreateAnonymousWithFirstLevelSubObjects(projectCameraPreset));
		}



        /// <summary>
        /// 
        /// This deletes a ProjectCameraPreset record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ProjectCameraPreset/{id}")]
		[Route("api/ProjectCameraPreset")]
		public async Task<IActionResult> DeleteProjectCameraPreset(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ProjectCameraPreset> query = (from x in _context.ProjectCameraPresets
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ProjectCameraPreset projectCameraPreset = await query.FirstOrDefaultAsync(cancellationToken);

			if (projectCameraPreset == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ProjectCameraPreset DELETE", id.ToString(), new Exception("No BMC.ProjectCameraPreset entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ProjectCameraPreset cloneOfExisting = (Database.ProjectCameraPreset)_context.Entry(projectCameraPreset).GetDatabaseValues().ToObject();


			try
			{
				projectCameraPreset.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ProjectCameraPreset entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ProjectCameraPreset.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ProjectCameraPreset.CreateAnonymousWithFirstLevelSubObjects(projectCameraPreset)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ProjectCameraPreset entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ProjectCameraPreset.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ProjectCameraPreset.CreateAnonymousWithFirstLevelSubObjects(projectCameraPreset)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of ProjectCameraPreset records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ProjectCameraPresets/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? projectId = null,
			string name = null,
			float? positionX = null,
			float? positionY = null,
			float? positionZ = null,
			float? targetX = null,
			float? targetY = null,
			float? targetZ = null,
			float? zoom = null,
			bool? isPerspective = null,
			int? sequence = null,
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

			IQueryable<Database.ProjectCameraPreset> query = (from pcp in _context.ProjectCameraPresets select pcp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (projectId.HasValue == true)
			{
				query = query.Where(pcp => pcp.projectId == projectId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(pcp => pcp.name == name);
			}
			if (positionX.HasValue == true)
			{
				query = query.Where(pcp => pcp.positionX == positionX.Value);
			}
			if (positionY.HasValue == true)
			{
				query = query.Where(pcp => pcp.positionY == positionY.Value);
			}
			if (positionZ.HasValue == true)
			{
				query = query.Where(pcp => pcp.positionZ == positionZ.Value);
			}
			if (targetX.HasValue == true)
			{
				query = query.Where(pcp => pcp.targetX == targetX.Value);
			}
			if (targetY.HasValue == true)
			{
				query = query.Where(pcp => pcp.targetY == targetY.Value);
			}
			if (targetZ.HasValue == true)
			{
				query = query.Where(pcp => pcp.targetZ == targetZ.Value);
			}
			if (zoom.HasValue == true)
			{
				query = query.Where(pcp => pcp.zoom == zoom.Value);
			}
			if (isPerspective.HasValue == true)
			{
				query = query.Where(pcp => pcp.isPerspective == isPerspective.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(pcp => pcp.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pcp => pcp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pcp => pcp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pcp => pcp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pcp => pcp.deleted == false);
				}
			}
			else
			{
				query = query.Where(pcp => pcp.active == true);
				query = query.Where(pcp => pcp.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Project Camera Preset, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.project.name.Contains(anyStringContains)
			       || x.project.description.Contains(anyStringContains)
			       || x.project.notes.Contains(anyStringContains)
			       || x.project.thumbnailImagePath.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ProjectCameraPreset.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ProjectCameraPreset/CreateAuditEvent")]
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
