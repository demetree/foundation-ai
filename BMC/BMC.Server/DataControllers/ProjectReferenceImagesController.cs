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
    /// This auto generated class provides the basic CRUD operations for the ProjectReferenceImage entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ProjectReferenceImage entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ProjectReferenceImagesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		private BMCContext _context;

		private ILogger<ProjectReferenceImagesController> _logger;

		public ProjectReferenceImagesController(BMCContext context, ILogger<ProjectReferenceImagesController> logger) : base("BMC", "ProjectReferenceImage")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ProjectReferenceImages filtered by the parameters provided.
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
		[Route("api/ProjectReferenceImages")]
		public async Task<IActionResult> GetProjectReferenceImages(
			int? projectId = null,
			string name = null,
			string imageFilePath = null,
			float? opacity = null,
			float? positionX = null,
			float? positionY = null,
			float? positionZ = null,
			float? scaleX = null,
			float? scaleY = null,
			bool? isVisible = null,
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

			IQueryable<Database.ProjectReferenceImage> query = (from pri in _context.ProjectReferenceImages select pri);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (projectId.HasValue == true)
			{
				query = query.Where(pri => pri.projectId == projectId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(pri => pri.name == name);
			}
			if (string.IsNullOrEmpty(imageFilePath) == false)
			{
				query = query.Where(pri => pri.imageFilePath == imageFilePath);
			}
			if (opacity.HasValue == true)
			{
				query = query.Where(pri => pri.opacity == opacity.Value);
			}
			if (positionX.HasValue == true)
			{
				query = query.Where(pri => pri.positionX == positionX.Value);
			}
			if (positionY.HasValue == true)
			{
				query = query.Where(pri => pri.positionY == positionY.Value);
			}
			if (positionZ.HasValue == true)
			{
				query = query.Where(pri => pri.positionZ == positionZ.Value);
			}
			if (scaleX.HasValue == true)
			{
				query = query.Where(pri => pri.scaleX == scaleX.Value);
			}
			if (scaleY.HasValue == true)
			{
				query = query.Where(pri => pri.scaleY == scaleY.Value);
			}
			if (isVisible.HasValue == true)
			{
				query = query.Where(pri => pri.isVisible == isVisible.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pri => pri.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pri => pri.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pri => pri.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pri => pri.deleted == false);
				}
			}
			else
			{
				query = query.Where(pri => pri.active == true);
				query = query.Where(pri => pri.deleted == false);
			}

			query = query.OrderBy(pri => pri.name).ThenBy(pri => pri.imageFilePath);


			//
			// Add the any string contains parameter to span all the string fields on the Project Reference Image, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.imageFilePath.Contains(anyStringContains)
			       || (includeRelations == true && x.project.name.Contains(anyStringContains))
			       || (includeRelations == true && x.project.description.Contains(anyStringContains))
			       || (includeRelations == true && x.project.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.project.thumbnailImagePath.Contains(anyStringContains))
			   );
			}

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
			
			List<Database.ProjectReferenceImage> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ProjectReferenceImage projectReferenceImage in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(projectReferenceImage, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.ProjectReferenceImage Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.ProjectReferenceImage Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ProjectReferenceImages filtered by the parameters provided.  Its query is similar to the GetProjectReferenceImages method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ProjectReferenceImages/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? projectId = null,
			string name = null,
			string imageFilePath = null,
			float? opacity = null,
			float? positionX = null,
			float? positionY = null,
			float? positionZ = null,
			float? scaleX = null,
			float? scaleY = null,
			bool? isVisible = null,
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


			IQueryable<Database.ProjectReferenceImage> query = (from pri in _context.ProjectReferenceImages select pri);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (projectId.HasValue == true)
			{
				query = query.Where(pri => pri.projectId == projectId.Value);
			}
			if (name != null)
			{
				query = query.Where(pri => pri.name == name);
			}
			if (imageFilePath != null)
			{
				query = query.Where(pri => pri.imageFilePath == imageFilePath);
			}
			if (opacity.HasValue == true)
			{
				query = query.Where(pri => pri.opacity == opacity.Value);
			}
			if (positionX.HasValue == true)
			{
				query = query.Where(pri => pri.positionX == positionX.Value);
			}
			if (positionY.HasValue == true)
			{
				query = query.Where(pri => pri.positionY == positionY.Value);
			}
			if (positionZ.HasValue == true)
			{
				query = query.Where(pri => pri.positionZ == positionZ.Value);
			}
			if (scaleX.HasValue == true)
			{
				query = query.Where(pri => pri.scaleX == scaleX.Value);
			}
			if (scaleY.HasValue == true)
			{
				query = query.Where(pri => pri.scaleY == scaleY.Value);
			}
			if (isVisible.HasValue == true)
			{
				query = query.Where(pri => pri.isVisible == isVisible.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pri => pri.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pri => pri.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pri => pri.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pri => pri.deleted == false);
				}
			}
			else
			{
				query = query.Where(pri => pri.active == true);
				query = query.Where(pri => pri.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Project Reference Image, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.imageFilePath.Contains(anyStringContains)
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
        /// This gets a single ProjectReferenceImage by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ProjectReferenceImage/{id}")]
		public async Task<IActionResult> GetProjectReferenceImage(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ProjectReferenceImage> query = (from pri in _context.ProjectReferenceImages where
							(pri.id == id) &&
							(userIsAdmin == true || pri.deleted == false) &&
							(userIsWriter == true || pri.active == true)
					select pri);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.project);
					query = query.AsSplitQuery();
				}

				Database.ProjectReferenceImage materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.ProjectReferenceImage Entity was read with Admin privilege." : "BMC.ProjectReferenceImage Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ProjectReferenceImage", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.ProjectReferenceImage entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.ProjectReferenceImage.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.ProjectReferenceImage.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ProjectReferenceImage record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ProjectReferenceImage/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutProjectReferenceImage(int id, [FromBody]Database.ProjectReferenceImage.ProjectReferenceImageDTO projectReferenceImageDTO, CancellationToken cancellationToken = default)
		{
			if (projectReferenceImageDTO == null)
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



			if (id != projectReferenceImageDTO.id)
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


			IQueryable<Database.ProjectReferenceImage> query = (from x in _context.ProjectReferenceImages
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ProjectReferenceImage existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ProjectReferenceImage PUT", id.ToString(), new Exception("No BMC.ProjectReferenceImage entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (projectReferenceImageDTO.objectGuid == Guid.Empty)
            {
                projectReferenceImageDTO.objectGuid = existing.objectGuid;
            }
            else if (projectReferenceImageDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ProjectReferenceImage record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ProjectReferenceImage cloneOfExisting = (Database.ProjectReferenceImage)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ProjectReferenceImage object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ProjectReferenceImage projectReferenceImage = (Database.ProjectReferenceImage)_context.Entry(existing).GetDatabaseValues().ToObject();
			projectReferenceImage.ApplyDTO(projectReferenceImageDTO);
			//
			// The tenant guid for any ProjectReferenceImage being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ProjectReferenceImage because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				projectReferenceImage.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (projectReferenceImage.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.ProjectReferenceImage record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (projectReferenceImage.name != null && projectReferenceImage.name.Length > 100)
			{
				projectReferenceImage.name = projectReferenceImage.name.Substring(0, 100);
			}

			if (projectReferenceImage.imageFilePath != null && projectReferenceImage.imageFilePath.Length > 250)
			{
				projectReferenceImage.imageFilePath = projectReferenceImage.imageFilePath.Substring(0, 250);
			}

			EntityEntry<Database.ProjectReferenceImage> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(projectReferenceImage);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.ProjectReferenceImage entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ProjectReferenceImage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ProjectReferenceImage.CreateAnonymousWithFirstLevelSubObjects(projectReferenceImage)),
					null);


				return Ok(Database.ProjectReferenceImage.CreateAnonymous(projectReferenceImage));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.ProjectReferenceImage entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ProjectReferenceImage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ProjectReferenceImage.CreateAnonymousWithFirstLevelSubObjects(projectReferenceImage)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new ProjectReferenceImage record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ProjectReferenceImage", Name = "ProjectReferenceImage")]
		public async Task<IActionResult> PostProjectReferenceImage([FromBody]Database.ProjectReferenceImage.ProjectReferenceImageDTO projectReferenceImageDTO, CancellationToken cancellationToken = default)
		{
			if (projectReferenceImageDTO == null)
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
			// Create a new ProjectReferenceImage object using the data from the DTO
			//
			Database.ProjectReferenceImage projectReferenceImage = Database.ProjectReferenceImage.FromDTO(projectReferenceImageDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				projectReferenceImage.tenantGuid = userTenantGuid;

				if (projectReferenceImage.name != null && projectReferenceImage.name.Length > 100)
				{
					projectReferenceImage.name = projectReferenceImage.name.Substring(0, 100);
				}

				if (projectReferenceImage.imageFilePath != null && projectReferenceImage.imageFilePath.Length > 250)
				{
					projectReferenceImage.imageFilePath = projectReferenceImage.imageFilePath.Substring(0, 250);
				}

				projectReferenceImage.objectGuid = Guid.NewGuid();
				_context.ProjectReferenceImages.Add(projectReferenceImage);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.ProjectReferenceImage entity successfully created.",
					true,
					projectReferenceImage.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ProjectReferenceImage.CreateAnonymousWithFirstLevelSubObjects(projectReferenceImage)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.ProjectReferenceImage entity creation failed.", false, projectReferenceImage.id.ToString(), "", JsonSerializer.Serialize(projectReferenceImage), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ProjectReferenceImage", projectReferenceImage.id, projectReferenceImage.name));

			return CreatedAtRoute("ProjectReferenceImage", new { id = projectReferenceImage.id }, Database.ProjectReferenceImage.CreateAnonymousWithFirstLevelSubObjects(projectReferenceImage));
		}



        /// <summary>
        /// 
        /// This deletes a ProjectReferenceImage record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ProjectReferenceImage/{id}")]
		[Route("api/ProjectReferenceImage")]
		public async Task<IActionResult> DeleteProjectReferenceImage(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ProjectReferenceImage> query = (from x in _context.ProjectReferenceImages
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ProjectReferenceImage projectReferenceImage = await query.FirstOrDefaultAsync(cancellationToken);

			if (projectReferenceImage == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ProjectReferenceImage DELETE", id.ToString(), new Exception("No BMC.ProjectReferenceImage entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ProjectReferenceImage cloneOfExisting = (Database.ProjectReferenceImage)_context.Entry(projectReferenceImage).GetDatabaseValues().ToObject();


			try
			{
				projectReferenceImage.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ProjectReferenceImage entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ProjectReferenceImage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ProjectReferenceImage.CreateAnonymousWithFirstLevelSubObjects(projectReferenceImage)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ProjectReferenceImage entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ProjectReferenceImage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ProjectReferenceImage.CreateAnonymousWithFirstLevelSubObjects(projectReferenceImage)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of ProjectReferenceImage records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ProjectReferenceImages/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? projectId = null,
			string name = null,
			string imageFilePath = null,
			float? opacity = null,
			float? positionX = null,
			float? positionY = null,
			float? positionZ = null,
			float? scaleX = null,
			float? scaleY = null,
			bool? isVisible = null,
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

			IQueryable<Database.ProjectReferenceImage> query = (from pri in _context.ProjectReferenceImages select pri);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (projectId.HasValue == true)
			{
				query = query.Where(pri => pri.projectId == projectId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(pri => pri.name == name);
			}
			if (string.IsNullOrEmpty(imageFilePath) == false)
			{
				query = query.Where(pri => pri.imageFilePath == imageFilePath);
			}
			if (opacity.HasValue == true)
			{
				query = query.Where(pri => pri.opacity == opacity.Value);
			}
			if (positionX.HasValue == true)
			{
				query = query.Where(pri => pri.positionX == positionX.Value);
			}
			if (positionY.HasValue == true)
			{
				query = query.Where(pri => pri.positionY == positionY.Value);
			}
			if (positionZ.HasValue == true)
			{
				query = query.Where(pri => pri.positionZ == positionZ.Value);
			}
			if (scaleX.HasValue == true)
			{
				query = query.Where(pri => pri.scaleX == scaleX.Value);
			}
			if (scaleY.HasValue == true)
			{
				query = query.Where(pri => pri.scaleY == scaleY.Value);
			}
			if (isVisible.HasValue == true)
			{
				query = query.Where(pri => pri.isVisible == isVisible.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pri => pri.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pri => pri.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pri => pri.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pri => pri.deleted == false);
				}
			}
			else
			{
				query = query.Where(pri => pri.active == true);
				query = query.Where(pri => pri.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Project Reference Image, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.imageFilePath.Contains(anyStringContains)
			       || x.project.name.Contains(anyStringContains)
			       || x.project.description.Contains(anyStringContains)
			       || x.project.notes.Contains(anyStringContains)
			       || x.project.thumbnailImagePath.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name).ThenBy(x => x.imageFilePath);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ProjectReferenceImage.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ProjectReferenceImage/CreateAuditEvent")]
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
