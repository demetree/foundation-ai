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
    /// This auto generated class provides the basic CRUD operations for the RenderPreset entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the RenderPreset entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class RenderPresetsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		private BMCContext _context;

		private ILogger<RenderPresetsController> _logger;

		public RenderPresetsController(BMCContext context, ILogger<RenderPresetsController> logger) : base("BMC", "RenderPreset")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of RenderPresets filtered by the parameters provided.
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
		[Route("api/RenderPresets")]
		public async Task<IActionResult> GetRenderPresets(
			string name = null,
			string description = null,
			int? resolutionWidth = null,
			int? resolutionHeight = null,
			string backgroundColorHex = null,
			bool? enableShadows = null,
			bool? enableReflections = null,
			string lightingMode = null,
			int? antiAliasLevel = null,
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

			IQueryable<Database.RenderPreset> query = (from rp in _context.RenderPresets select rp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(rp => rp.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(rp => rp.description == description);
			}
			if (resolutionWidth.HasValue == true)
			{
				query = query.Where(rp => rp.resolutionWidth == resolutionWidth.Value);
			}
			if (resolutionHeight.HasValue == true)
			{
				query = query.Where(rp => rp.resolutionHeight == resolutionHeight.Value);
			}
			if (string.IsNullOrEmpty(backgroundColorHex) == false)
			{
				query = query.Where(rp => rp.backgroundColorHex == backgroundColorHex);
			}
			if (enableShadows.HasValue == true)
			{
				query = query.Where(rp => rp.enableShadows == enableShadows.Value);
			}
			if (enableReflections.HasValue == true)
			{
				query = query.Where(rp => rp.enableReflections == enableReflections.Value);
			}
			if (string.IsNullOrEmpty(lightingMode) == false)
			{
				query = query.Where(rp => rp.lightingMode == lightingMode);
			}
			if (antiAliasLevel.HasValue == true)
			{
				query = query.Where(rp => rp.antiAliasLevel == antiAliasLevel.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rp => rp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rp => rp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rp => rp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rp => rp.deleted == false);
				}
			}
			else
			{
				query = query.Where(rp => rp.active == true);
				query = query.Where(rp => rp.deleted == false);
			}

			query = query.OrderBy(rp => rp.name).ThenBy(rp => rp.description).ThenBy(rp => rp.backgroundColorHex);


			//
			// Add the any string contains parameter to span all the string fields on the Render Preset, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.backgroundColorHex.Contains(anyStringContains)
			       || x.lightingMode.Contains(anyStringContains)
			   );
			}

			if (includeRelations == true)
			{
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.RenderPreset> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.RenderPreset renderPreset in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(renderPreset, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.RenderPreset Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.RenderPreset Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of RenderPresets filtered by the parameters provided.  Its query is similar to the GetRenderPresets method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RenderPresets/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			int? resolutionWidth = null,
			int? resolutionHeight = null,
			string backgroundColorHex = null,
			bool? enableShadows = null,
			bool? enableReflections = null,
			string lightingMode = null,
			int? antiAliasLevel = null,
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


			IQueryable<Database.RenderPreset> query = (from rp in _context.RenderPresets select rp);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(rp => rp.name == name);
			}
			if (description != null)
			{
				query = query.Where(rp => rp.description == description);
			}
			if (resolutionWidth.HasValue == true)
			{
				query = query.Where(rp => rp.resolutionWidth == resolutionWidth.Value);
			}
			if (resolutionHeight.HasValue == true)
			{
				query = query.Where(rp => rp.resolutionHeight == resolutionHeight.Value);
			}
			if (backgroundColorHex != null)
			{
				query = query.Where(rp => rp.backgroundColorHex == backgroundColorHex);
			}
			if (enableShadows.HasValue == true)
			{
				query = query.Where(rp => rp.enableShadows == enableShadows.Value);
			}
			if (enableReflections.HasValue == true)
			{
				query = query.Where(rp => rp.enableReflections == enableReflections.Value);
			}
			if (lightingMode != null)
			{
				query = query.Where(rp => rp.lightingMode == lightingMode);
			}
			if (antiAliasLevel.HasValue == true)
			{
				query = query.Where(rp => rp.antiAliasLevel == antiAliasLevel.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rp => rp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rp => rp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rp => rp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rp => rp.deleted == false);
				}
			}
			else
			{
				query = query.Where(rp => rp.active == true);
				query = query.Where(rp => rp.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Render Preset, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.backgroundColorHex.Contains(anyStringContains)
			       || x.lightingMode.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single RenderPreset by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RenderPreset/{id}")]
		public async Task<IActionResult> GetRenderPreset(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.RenderPreset> query = (from rp in _context.RenderPresets where
							(rp.id == id) &&
							(userIsAdmin == true || rp.deleted == false) &&
							(userIsWriter == true || rp.active == true)
					select rp);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.RenderPreset materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.RenderPreset Entity was read with Admin privilege." : "BMC.RenderPreset Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "RenderPreset", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.RenderPreset entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.RenderPreset.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.RenderPreset.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing RenderPreset record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/RenderPreset/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutRenderPreset(int id, [FromBody]Database.RenderPreset.RenderPresetDTO renderPresetDTO, CancellationToken cancellationToken = default)
		{
			if (renderPresetDTO == null)
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



			if (id != renderPresetDTO.id)
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


			IQueryable<Database.RenderPreset> query = (from x in _context.RenderPresets
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.RenderPreset existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.RenderPreset PUT", id.ToString(), new Exception("No BMC.RenderPreset entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (renderPresetDTO.objectGuid == Guid.Empty)
            {
                renderPresetDTO.objectGuid = existing.objectGuid;
            }
            else if (renderPresetDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a RenderPreset record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.RenderPreset cloneOfExisting = (Database.RenderPreset)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new RenderPreset object using the data from the existing record, updated with what is in the DTO.
			//
			Database.RenderPreset renderPreset = (Database.RenderPreset)_context.Entry(existing).GetDatabaseValues().ToObject();
			renderPreset.ApplyDTO(renderPresetDTO);
			//
			// The tenant guid for any RenderPreset being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the RenderPreset because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				renderPreset.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (renderPreset.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.RenderPreset record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (renderPreset.name != null && renderPreset.name.Length > 100)
			{
				renderPreset.name = renderPreset.name.Substring(0, 100);
			}

			if (renderPreset.description != null && renderPreset.description.Length > 500)
			{
				renderPreset.description = renderPreset.description.Substring(0, 500);
			}

			if (renderPreset.backgroundColorHex != null && renderPreset.backgroundColorHex.Length > 10)
			{
				renderPreset.backgroundColorHex = renderPreset.backgroundColorHex.Substring(0, 10);
			}

			if (renderPreset.lightingMode != null && renderPreset.lightingMode.Length > 100)
			{
				renderPreset.lightingMode = renderPreset.lightingMode.Substring(0, 100);
			}

			EntityEntry<Database.RenderPreset> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(renderPreset);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.RenderPreset entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.RenderPreset.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.RenderPreset.CreateAnonymousWithFirstLevelSubObjects(renderPreset)),
					null);


				return Ok(Database.RenderPreset.CreateAnonymous(renderPreset));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.RenderPreset entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.RenderPreset.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.RenderPreset.CreateAnonymousWithFirstLevelSubObjects(renderPreset)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new RenderPreset record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RenderPreset", Name = "RenderPreset")]
		public async Task<IActionResult> PostRenderPreset([FromBody]Database.RenderPreset.RenderPresetDTO renderPresetDTO, CancellationToken cancellationToken = default)
		{
			if (renderPresetDTO == null)
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
			// Create a new RenderPreset object using the data from the DTO
			//
			Database.RenderPreset renderPreset = Database.RenderPreset.FromDTO(renderPresetDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				renderPreset.tenantGuid = userTenantGuid;

				if (renderPreset.name != null && renderPreset.name.Length > 100)
				{
					renderPreset.name = renderPreset.name.Substring(0, 100);
				}

				if (renderPreset.description != null && renderPreset.description.Length > 500)
				{
					renderPreset.description = renderPreset.description.Substring(0, 500);
				}

				if (renderPreset.backgroundColorHex != null && renderPreset.backgroundColorHex.Length > 10)
				{
					renderPreset.backgroundColorHex = renderPreset.backgroundColorHex.Substring(0, 10);
				}

				if (renderPreset.lightingMode != null && renderPreset.lightingMode.Length > 100)
				{
					renderPreset.lightingMode = renderPreset.lightingMode.Substring(0, 100);
				}

				renderPreset.objectGuid = Guid.NewGuid();
				_context.RenderPresets.Add(renderPreset);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.RenderPreset entity successfully created.",
					true,
					renderPreset.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.RenderPreset.CreateAnonymousWithFirstLevelSubObjects(renderPreset)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.RenderPreset entity creation failed.", false, renderPreset.id.ToString(), "", JsonSerializer.Serialize(renderPreset), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "RenderPreset", renderPreset.id, renderPreset.name));

			return CreatedAtRoute("RenderPreset", new { id = renderPreset.id }, Database.RenderPreset.CreateAnonymousWithFirstLevelSubObjects(renderPreset));
		}



        /// <summary>
        /// 
        /// This deletes a RenderPreset record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RenderPreset/{id}")]
		[Route("api/RenderPreset")]
		public async Task<IActionResult> DeleteRenderPreset(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.RenderPreset> query = (from x in _context.RenderPresets
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.RenderPreset renderPreset = await query.FirstOrDefaultAsync(cancellationToken);

			if (renderPreset == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.RenderPreset DELETE", id.ToString(), new Exception("No BMC.RenderPreset entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.RenderPreset cloneOfExisting = (Database.RenderPreset)_context.Entry(renderPreset).GetDatabaseValues().ToObject();


			try
			{
				renderPreset.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.RenderPreset entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.RenderPreset.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.RenderPreset.CreateAnonymousWithFirstLevelSubObjects(renderPreset)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.RenderPreset entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.RenderPreset.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.RenderPreset.CreateAnonymousWithFirstLevelSubObjects(renderPreset)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of RenderPreset records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/RenderPresets/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			int? resolutionWidth = null,
			int? resolutionHeight = null,
			string backgroundColorHex = null,
			bool? enableShadows = null,
			bool? enableReflections = null,
			string lightingMode = null,
			int? antiAliasLevel = null,
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

			IQueryable<Database.RenderPreset> query = (from rp in _context.RenderPresets select rp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(rp => rp.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(rp => rp.description == description);
			}
			if (resolutionWidth.HasValue == true)
			{
				query = query.Where(rp => rp.resolutionWidth == resolutionWidth.Value);
			}
			if (resolutionHeight.HasValue == true)
			{
				query = query.Where(rp => rp.resolutionHeight == resolutionHeight.Value);
			}
			if (string.IsNullOrEmpty(backgroundColorHex) == false)
			{
				query = query.Where(rp => rp.backgroundColorHex == backgroundColorHex);
			}
			if (enableShadows.HasValue == true)
			{
				query = query.Where(rp => rp.enableShadows == enableShadows.Value);
			}
			if (enableReflections.HasValue == true)
			{
				query = query.Where(rp => rp.enableReflections == enableReflections.Value);
			}
			if (string.IsNullOrEmpty(lightingMode) == false)
			{
				query = query.Where(rp => rp.lightingMode == lightingMode);
			}
			if (antiAliasLevel.HasValue == true)
			{
				query = query.Where(rp => rp.antiAliasLevel == antiAliasLevel.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rp => rp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rp => rp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rp => rp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rp => rp.deleted == false);
				}
			}
			else
			{
				query = query.Where(rp => rp.active == true);
				query = query.Where(rp => rp.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Render Preset, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.backgroundColorHex.Contains(anyStringContains)
			       || x.lightingMode.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.backgroundColorHex);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.RenderPreset.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/RenderPreset/CreateAuditEvent")]
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
