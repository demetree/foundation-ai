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
    /// This auto generated class provides the basic CRUD operations for the MocCollaborator entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the MocCollaborator entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class MocCollaboratorsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		private BMCContext _context;

		private ILogger<MocCollaboratorsController> _logger;

		public MocCollaboratorsController(BMCContext context, ILogger<MocCollaboratorsController> logger) : base("BMC", "MocCollaborator")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of MocCollaborators filtered by the parameters provided.
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
		[Route("api/MocCollaborators")]
		public async Task<IActionResult> GetMocCollaborators(
			int? publishedMocId = null,
			Guid? collaboratorTenantGuid = null,
			string accessLevel = null,
			DateTime? invitedDate = null,
			DateTime? acceptedDate = null,
			bool? isAccepted = null,
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
			if (invitedDate.HasValue == true && invitedDate.Value.Kind != DateTimeKind.Utc)
			{
				invitedDate = invitedDate.Value.ToUniversalTime();
			}

			if (acceptedDate.HasValue == true && acceptedDate.Value.Kind != DateTimeKind.Utc)
			{
				acceptedDate = acceptedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.MocCollaborator> query = (from mc in _context.MocCollaborators select mc);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (publishedMocId.HasValue == true)
			{
				query = query.Where(mc => mc.publishedMocId == publishedMocId.Value);
			}
			if (collaboratorTenantGuid.HasValue == true)
			{
				query = query.Where(mc => mc.collaboratorTenantGuid == collaboratorTenantGuid);
			}
			if (string.IsNullOrEmpty(accessLevel) == false)
			{
				query = query.Where(mc => mc.accessLevel == accessLevel);
			}
			if (invitedDate.HasValue == true)
			{
				query = query.Where(mc => mc.invitedDate == invitedDate.Value);
			}
			if (acceptedDate.HasValue == true)
			{
				query = query.Where(mc => mc.acceptedDate == acceptedDate.Value);
			}
			if (isAccepted.HasValue == true)
			{
				query = query.Where(mc => mc.isAccepted == isAccepted.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mc => mc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mc => mc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mc => mc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mc => mc.deleted == false);
				}
			}
			else
			{
				query = query.Where(mc => mc.active == true);
				query = query.Where(mc => mc.deleted == false);
			}

			query = query.OrderBy(mc => mc.accessLevel);


			//
			// Add the any string contains parameter to span all the string fields on the Moc Collaborator, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.accessLevel.Contains(anyStringContains)
			       || (includeRelations == true && x.publishedMoc.name.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.description.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.thumbnailImagePath.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.tags.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.visibility.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.licenseName.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.readmeMarkdown.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.slug.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.defaultBranchName.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.publishedMoc);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.MocCollaborator> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.MocCollaborator mocCollaborator in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(mocCollaborator, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.MocCollaborator Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.MocCollaborator Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of MocCollaborators filtered by the parameters provided.  Its query is similar to the GetMocCollaborators method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MocCollaborators/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? publishedMocId = null,
			Guid? collaboratorTenantGuid = null,
			string accessLevel = null,
			DateTime? invitedDate = null,
			DateTime? acceptedDate = null,
			bool? isAccepted = null,
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
			if (invitedDate.HasValue == true && invitedDate.Value.Kind != DateTimeKind.Utc)
			{
				invitedDate = invitedDate.Value.ToUniversalTime();
			}

			if (acceptedDate.HasValue == true && acceptedDate.Value.Kind != DateTimeKind.Utc)
			{
				acceptedDate = acceptedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.MocCollaborator> query = (from mc in _context.MocCollaborators select mc);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (publishedMocId.HasValue == true)
			{
				query = query.Where(mc => mc.publishedMocId == publishedMocId.Value);
			}
			if (collaboratorTenantGuid.HasValue == true)
			{
				query = query.Where(mc => mc.collaboratorTenantGuid == collaboratorTenantGuid);
			}
			if (accessLevel != null)
			{
				query = query.Where(mc => mc.accessLevel == accessLevel);
			}
			if (invitedDate.HasValue == true)
			{
				query = query.Where(mc => mc.invitedDate == invitedDate.Value);
			}
			if (acceptedDate.HasValue == true)
			{
				query = query.Where(mc => mc.acceptedDate == acceptedDate.Value);
			}
			if (isAccepted.HasValue == true)
			{
				query = query.Where(mc => mc.isAccepted == isAccepted.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mc => mc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mc => mc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mc => mc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mc => mc.deleted == false);
				}
			}
			else
			{
				query = query.Where(mc => mc.active == true);
				query = query.Where(mc => mc.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Moc Collaborator, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.accessLevel.Contains(anyStringContains)
			       || x.publishedMoc.name.Contains(anyStringContains)
			       || x.publishedMoc.description.Contains(anyStringContains)
			       || x.publishedMoc.thumbnailImagePath.Contains(anyStringContains)
			       || x.publishedMoc.tags.Contains(anyStringContains)
			       || x.publishedMoc.visibility.Contains(anyStringContains)
			       || x.publishedMoc.licenseName.Contains(anyStringContains)
			       || x.publishedMoc.readmeMarkdown.Contains(anyStringContains)
			       || x.publishedMoc.slug.Contains(anyStringContains)
			       || x.publishedMoc.defaultBranchName.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single MocCollaborator by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MocCollaborator/{id}")]
		public async Task<IActionResult> GetMocCollaborator(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.MocCollaborator> query = (from mc in _context.MocCollaborators where
							(mc.id == id) &&
							(userIsAdmin == true || mc.deleted == false) &&
							(userIsWriter == true || mc.active == true)
					select mc);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.publishedMoc);
					query = query.AsSplitQuery();
				}

				Database.MocCollaborator materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.MocCollaborator Entity was read with Admin privilege." : "BMC.MocCollaborator Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "MocCollaborator", materialized.id, materialized.accessLevel));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.MocCollaborator entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.MocCollaborator.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.MocCollaborator.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing MocCollaborator record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/MocCollaborator/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutMocCollaborator(int id, [FromBody]Database.MocCollaborator.MocCollaboratorDTO mocCollaboratorDTO, CancellationToken cancellationToken = default)
		{
			if (mocCollaboratorDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != mocCollaboratorDTO.id)
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


			IQueryable<Database.MocCollaborator> query = (from x in _context.MocCollaborators
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.MocCollaborator existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.MocCollaborator PUT", id.ToString(), new Exception("No BMC.MocCollaborator entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (mocCollaboratorDTO.objectGuid == Guid.Empty)
            {
                mocCollaboratorDTO.objectGuid = existing.objectGuid;
            }
            else if (mocCollaboratorDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a MocCollaborator record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.MocCollaborator cloneOfExisting = (Database.MocCollaborator)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new MocCollaborator object using the data from the existing record, updated with what is in the DTO.
			//
			Database.MocCollaborator mocCollaborator = (Database.MocCollaborator)_context.Entry(existing).GetDatabaseValues().ToObject();
			mocCollaborator.ApplyDTO(mocCollaboratorDTO);
			//
			// The tenant guid for any MocCollaborator being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the MocCollaborator because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				mocCollaborator.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (mocCollaborator.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.MocCollaborator record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (mocCollaborator.accessLevel != null && mocCollaborator.accessLevel.Length > 50)
			{
				mocCollaborator.accessLevel = mocCollaborator.accessLevel.Substring(0, 50);
			}

			if (mocCollaborator.invitedDate.Kind != DateTimeKind.Utc)
			{
				mocCollaborator.invitedDate = mocCollaborator.invitedDate.ToUniversalTime();
			}

			if (mocCollaborator.acceptedDate.HasValue == true && mocCollaborator.acceptedDate.Value.Kind != DateTimeKind.Utc)
			{
				mocCollaborator.acceptedDate = mocCollaborator.acceptedDate.Value.ToUniversalTime();
			}

			EntityEntry<Database.MocCollaborator> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(mocCollaborator);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.MocCollaborator entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.MocCollaborator.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MocCollaborator.CreateAnonymousWithFirstLevelSubObjects(mocCollaborator)),
					null);


				return Ok(Database.MocCollaborator.CreateAnonymous(mocCollaborator));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.MocCollaborator entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.MocCollaborator.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MocCollaborator.CreateAnonymousWithFirstLevelSubObjects(mocCollaborator)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new MocCollaborator record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MocCollaborator", Name = "MocCollaborator")]
		public async Task<IActionResult> PostMocCollaborator([FromBody]Database.MocCollaborator.MocCollaboratorDTO mocCollaboratorDTO, CancellationToken cancellationToken = default)
		{
			if (mocCollaboratorDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new MocCollaborator object using the data from the DTO
			//
			Database.MocCollaborator mocCollaborator = Database.MocCollaborator.FromDTO(mocCollaboratorDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				mocCollaborator.tenantGuid = userTenantGuid;

				if (mocCollaborator.accessLevel != null && mocCollaborator.accessLevel.Length > 50)
				{
					mocCollaborator.accessLevel = mocCollaborator.accessLevel.Substring(0, 50);
				}

				if (mocCollaborator.invitedDate.Kind != DateTimeKind.Utc)
				{
					mocCollaborator.invitedDate = mocCollaborator.invitedDate.ToUniversalTime();
				}

				if (mocCollaborator.acceptedDate.HasValue == true && mocCollaborator.acceptedDate.Value.Kind != DateTimeKind.Utc)
				{
					mocCollaborator.acceptedDate = mocCollaborator.acceptedDate.Value.ToUniversalTime();
				}

				mocCollaborator.objectGuid = Guid.NewGuid();
				_context.MocCollaborators.Add(mocCollaborator);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.MocCollaborator entity successfully created.",
					true,
					mocCollaborator.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.MocCollaborator.CreateAnonymousWithFirstLevelSubObjects(mocCollaborator)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.MocCollaborator entity creation failed.", false, mocCollaborator.id.ToString(), "", JsonSerializer.Serialize(mocCollaborator), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "MocCollaborator", mocCollaborator.id, mocCollaborator.accessLevel));

			return CreatedAtRoute("MocCollaborator", new { id = mocCollaborator.id }, Database.MocCollaborator.CreateAnonymousWithFirstLevelSubObjects(mocCollaborator));
		}



        /// <summary>
        /// 
        /// This deletes a MocCollaborator record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MocCollaborator/{id}")]
		[Route("api/MocCollaborator")]
		public async Task<IActionResult> DeleteMocCollaborator(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.MocCollaborator> query = (from x in _context.MocCollaborators
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.MocCollaborator mocCollaborator = await query.FirstOrDefaultAsync(cancellationToken);

			if (mocCollaborator == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.MocCollaborator DELETE", id.ToString(), new Exception("No BMC.MocCollaborator entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.MocCollaborator cloneOfExisting = (Database.MocCollaborator)_context.Entry(mocCollaborator).GetDatabaseValues().ToObject();


			try
			{
				mocCollaborator.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.MocCollaborator entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.MocCollaborator.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MocCollaborator.CreateAnonymousWithFirstLevelSubObjects(mocCollaborator)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.MocCollaborator entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.MocCollaborator.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MocCollaborator.CreateAnonymousWithFirstLevelSubObjects(mocCollaborator)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of MocCollaborator records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/MocCollaborators/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? publishedMocId = null,
			Guid? collaboratorTenantGuid = null,
			string accessLevel = null,
			DateTime? invitedDate = null,
			DateTime? acceptedDate = null,
			bool? isAccepted = null,
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
			if (invitedDate.HasValue == true && invitedDate.Value.Kind != DateTimeKind.Utc)
			{
				invitedDate = invitedDate.Value.ToUniversalTime();
			}

			if (acceptedDate.HasValue == true && acceptedDate.Value.Kind != DateTimeKind.Utc)
			{
				acceptedDate = acceptedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.MocCollaborator> query = (from mc in _context.MocCollaborators select mc);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (publishedMocId.HasValue == true)
			{
				query = query.Where(mc => mc.publishedMocId == publishedMocId.Value);
			}
			if (collaboratorTenantGuid.HasValue == true)
			{
				query = query.Where(mc => mc.collaboratorTenantGuid == collaboratorTenantGuid);
			}
			if (string.IsNullOrEmpty(accessLevel) == false)
			{
				query = query.Where(mc => mc.accessLevel == accessLevel);
			}
			if (invitedDate.HasValue == true)
			{
				query = query.Where(mc => mc.invitedDate == invitedDate.Value);
			}
			if (acceptedDate.HasValue == true)
			{
				query = query.Where(mc => mc.acceptedDate == acceptedDate.Value);
			}
			if (isAccepted.HasValue == true)
			{
				query = query.Where(mc => mc.isAccepted == isAccepted.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mc => mc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mc => mc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mc => mc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mc => mc.deleted == false);
				}
			}
			else
			{
				query = query.Where(mc => mc.active == true);
				query = query.Where(mc => mc.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Moc Collaborator, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.accessLevel.Contains(anyStringContains)
			       || x.publishedMoc.name.Contains(anyStringContains)
			       || x.publishedMoc.description.Contains(anyStringContains)
			       || x.publishedMoc.thumbnailImagePath.Contains(anyStringContains)
			       || x.publishedMoc.tags.Contains(anyStringContains)
			       || x.publishedMoc.visibility.Contains(anyStringContains)
			       || x.publishedMoc.licenseName.Contains(anyStringContains)
			       || x.publishedMoc.readmeMarkdown.Contains(anyStringContains)
			       || x.publishedMoc.slug.Contains(anyStringContains)
			       || x.publishedMoc.defaultBranchName.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.accessLevel);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.MocCollaborator.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/MocCollaborator/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
