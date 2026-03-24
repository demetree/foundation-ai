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
using Foundation.Scheduler.Database;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the PushProviderConfiguration entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the PushProviderConfiguration entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class PushProviderConfigurationsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 50;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 100;

		private SchedulerContext _context;

		private ILogger<PushProviderConfigurationsController> _logger;

		public PushProviderConfigurationsController(SchedulerContext context, ILogger<PushProviderConfigurationsController> logger) : base("Scheduler", "PushProviderConfiguration")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of PushProviderConfigurations filtered by the parameters provided.
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
		[Route("api/PushProviderConfigurations")]
		public async Task<IActionResult> GetPushProviderConfigurations(
			string providerId = null,
			bool? enabled = null,
			string configurationJson = null,
			DateTime? dateTimeModified = null,
			int? modifiedByUserId = null,
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
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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
			if (dateTimeModified.HasValue == true && dateTimeModified.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeModified = dateTimeModified.Value.ToUniversalTime();
			}

			IQueryable<Database.PushProviderConfiguration> query = (from ppc in _context.PushProviderConfigurations select ppc);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(providerId) == false)
			{
				query = query.Where(ppc => ppc.providerId == providerId);
			}
			if (enabled.HasValue == true)
			{
				query = query.Where(ppc => ppc.enabled == enabled.Value);
			}
			if (string.IsNullOrEmpty(configurationJson) == false)
			{
				query = query.Where(ppc => ppc.configurationJson == configurationJson);
			}
			if (dateTimeModified.HasValue == true)
			{
				query = query.Where(ppc => ppc.dateTimeModified == dateTimeModified.Value);
			}
			if (modifiedByUserId.HasValue == true)
			{
				query = query.Where(ppc => ppc.modifiedByUserId == modifiedByUserId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ppc => ppc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ppc => ppc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ppc => ppc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ppc => ppc.deleted == false);
				}
			}
			else
			{
				query = query.Where(ppc => ppc.active == true);
				query = query.Where(ppc => ppc.deleted == false);
			}

			query = query.OrderBy(ppc => ppc.providerId);


			//
			// Add the any string contains parameter to span all the string fields on the Push Provider Configuration, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.providerId.Contains(anyStringContains)
			       || x.configurationJson.Contains(anyStringContains)
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
			
			List<Database.PushProviderConfiguration> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.PushProviderConfiguration pushProviderConfiguration in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(pushProviderConfiguration, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.PushProviderConfiguration Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.PushProviderConfiguration Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of PushProviderConfigurations filtered by the parameters provided.  Its query is similar to the GetPushProviderConfigurations method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PushProviderConfigurations/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string providerId = null,
			bool? enabled = null,
			string configurationJson = null,
			DateTime? dateTimeModified = null,
			int? modifiedByUserId = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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
			if (dateTimeModified.HasValue == true && dateTimeModified.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeModified = dateTimeModified.Value.ToUniversalTime();
			}

			IQueryable<Database.PushProviderConfiguration> query = (from ppc in _context.PushProviderConfigurations select ppc);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (providerId != null)
			{
				query = query.Where(ppc => ppc.providerId == providerId);
			}
			if (enabled.HasValue == true)
			{
				query = query.Where(ppc => ppc.enabled == enabled.Value);
			}
			if (configurationJson != null)
			{
				query = query.Where(ppc => ppc.configurationJson == configurationJson);
			}
			if (dateTimeModified.HasValue == true)
			{
				query = query.Where(ppc => ppc.dateTimeModified == dateTimeModified.Value);
			}
			if (modifiedByUserId.HasValue == true)
			{
				query = query.Where(ppc => ppc.modifiedByUserId == modifiedByUserId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ppc => ppc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ppc => ppc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ppc => ppc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ppc => ppc.deleted == false);
				}
			}
			else
			{
				query = query.Where(ppc => ppc.active == true);
				query = query.Where(ppc => ppc.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Push Provider Configuration, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.providerId.Contains(anyStringContains)
			       || x.configurationJson.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single PushProviderConfiguration by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PushProviderConfiguration/{id}")]
		public async Task<IActionResult> GetPushProviderConfiguration(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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
				IQueryable<Database.PushProviderConfiguration> query = (from ppc in _context.PushProviderConfigurations where
							(ppc.id == id) &&
							(userIsAdmin == true || ppc.deleted == false) &&
							(userIsWriter == true || ppc.active == true)
					select ppc);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.PushProviderConfiguration materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.PushProviderConfiguration Entity was read with Admin privilege." : "Scheduler.PushProviderConfiguration Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PushProviderConfiguration", materialized.id, materialized.providerId));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.PushProviderConfiguration entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.PushProviderConfiguration.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.PushProviderConfiguration.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing PushProviderConfiguration record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/PushProviderConfiguration/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutPushProviderConfiguration(int id, [FromBody]Database.PushProviderConfiguration.PushProviderConfigurationDTO pushProviderConfigurationDTO, CancellationToken cancellationToken = default)
		{
			if (pushProviderConfigurationDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != pushProviderConfigurationDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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


			IQueryable<Database.PushProviderConfiguration> query = (from x in _context.PushProviderConfigurations
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.PushProviderConfiguration existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.PushProviderConfiguration PUT", id.ToString(), new Exception("No Scheduler.PushProviderConfiguration entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (pushProviderConfigurationDTO.objectGuid == Guid.Empty)
            {
                pushProviderConfigurationDTO.objectGuid = existing.objectGuid;
            }
            else if (pushProviderConfigurationDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a PushProviderConfiguration record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.PushProviderConfiguration cloneOfExisting = (Database.PushProviderConfiguration)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new PushProviderConfiguration object using the data from the existing record, updated with what is in the DTO.
			//
			Database.PushProviderConfiguration pushProviderConfiguration = (Database.PushProviderConfiguration)_context.Entry(existing).GetDatabaseValues().ToObject();
			pushProviderConfiguration.ApplyDTO(pushProviderConfigurationDTO);
			//
			// The tenant guid for any PushProviderConfiguration being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the PushProviderConfiguration because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				pushProviderConfiguration.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (pushProviderConfiguration.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.PushProviderConfiguration record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (pushProviderConfiguration.providerId != null && pushProviderConfiguration.providerId.Length > 50)
			{
				pushProviderConfiguration.providerId = pushProviderConfiguration.providerId.Substring(0, 50);
			}

			if (pushProviderConfiguration.dateTimeModified.Kind != DateTimeKind.Utc)
			{
				pushProviderConfiguration.dateTimeModified = pushProviderConfiguration.dateTimeModified.ToUniversalTime();
			}

			EntityEntry<Database.PushProviderConfiguration> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(pushProviderConfiguration);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.PushProviderConfiguration entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.PushProviderConfiguration.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PushProviderConfiguration.CreateAnonymousWithFirstLevelSubObjects(pushProviderConfiguration)),
					null);


				return Ok(Database.PushProviderConfiguration.CreateAnonymous(pushProviderConfiguration));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.PushProviderConfiguration entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.PushProviderConfiguration.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PushProviderConfiguration.CreateAnonymousWithFirstLevelSubObjects(pushProviderConfiguration)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new PushProviderConfiguration record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PushProviderConfiguration", Name = "PushProviderConfiguration")]
		public async Task<IActionResult> PostPushProviderConfiguration([FromBody]Database.PushProviderConfiguration.PushProviderConfigurationDTO pushProviderConfigurationDTO, CancellationToken cancellationToken = default)
		{
			if (pushProviderConfigurationDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Administrator role needed to write to this table.
			//
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

			//
			// Create a new PushProviderConfiguration object using the data from the DTO
			//
			Database.PushProviderConfiguration pushProviderConfiguration = Database.PushProviderConfiguration.FromDTO(pushProviderConfigurationDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				pushProviderConfiguration.tenantGuid = userTenantGuid;

				if (pushProviderConfiguration.providerId != null && pushProviderConfiguration.providerId.Length > 50)
				{
					pushProviderConfiguration.providerId = pushProviderConfiguration.providerId.Substring(0, 50);
				}

				if (pushProviderConfiguration.dateTimeModified.Kind != DateTimeKind.Utc)
				{
					pushProviderConfiguration.dateTimeModified = pushProviderConfiguration.dateTimeModified.ToUniversalTime();
				}

				pushProviderConfiguration.objectGuid = Guid.NewGuid();
				_context.PushProviderConfigurations.Add(pushProviderConfiguration);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.PushProviderConfiguration entity successfully created.",
					true,
					pushProviderConfiguration.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.PushProviderConfiguration.CreateAnonymousWithFirstLevelSubObjects(pushProviderConfiguration)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.PushProviderConfiguration entity creation failed.", false, pushProviderConfiguration.id.ToString(), "", JsonSerializer.Serialize(pushProviderConfiguration), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PushProviderConfiguration", pushProviderConfiguration.id, pushProviderConfiguration.providerId));

			return CreatedAtRoute("PushProviderConfiguration", new { id = pushProviderConfiguration.id }, Database.PushProviderConfiguration.CreateAnonymousWithFirstLevelSubObjects(pushProviderConfiguration));
		}



        /// <summary>
        /// 
        /// This deletes a PushProviderConfiguration record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PushProviderConfiguration/{id}")]
		[Route("api/PushProviderConfiguration")]
		public async Task<IActionResult> DeletePushProviderConfiguration(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.PushProviderConfiguration> query = (from x in _context.PushProviderConfigurations
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.PushProviderConfiguration pushProviderConfiguration = await query.FirstOrDefaultAsync(cancellationToken);

			if (pushProviderConfiguration == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.PushProviderConfiguration DELETE", id.ToString(), new Exception("No Scheduler.PushProviderConfiguration entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.PushProviderConfiguration cloneOfExisting = (Database.PushProviderConfiguration)_context.Entry(pushProviderConfiguration).GetDatabaseValues().ToObject();


			try
			{
				pushProviderConfiguration.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.PushProviderConfiguration entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.PushProviderConfiguration.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PushProviderConfiguration.CreateAnonymousWithFirstLevelSubObjects(pushProviderConfiguration)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.PushProviderConfiguration entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.PushProviderConfiguration.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PushProviderConfiguration.CreateAnonymousWithFirstLevelSubObjects(pushProviderConfiguration)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of PushProviderConfiguration records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/PushProviderConfigurations/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string providerId = null,
			bool? enabled = null,
			string configurationJson = null,
			DateTime? dateTimeModified = null,
			int? modifiedByUserId = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);


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
			if (dateTimeModified.HasValue == true && dateTimeModified.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeModified = dateTimeModified.Value.ToUniversalTime();
			}

			IQueryable<Database.PushProviderConfiguration> query = (from ppc in _context.PushProviderConfigurations select ppc);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(providerId) == false)
			{
				query = query.Where(ppc => ppc.providerId == providerId);
			}
			if (enabled.HasValue == true)
			{
				query = query.Where(ppc => ppc.enabled == enabled.Value);
			}
			if (string.IsNullOrEmpty(configurationJson) == false)
			{
				query = query.Where(ppc => ppc.configurationJson == configurationJson);
			}
			if (dateTimeModified.HasValue == true)
			{
				query = query.Where(ppc => ppc.dateTimeModified == dateTimeModified.Value);
			}
			if (modifiedByUserId.HasValue == true)
			{
				query = query.Where(ppc => ppc.modifiedByUserId == modifiedByUserId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ppc => ppc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ppc => ppc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ppc => ppc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ppc => ppc.deleted == false);
				}
			}
			else
			{
				query = query.Where(ppc => ppc.active == true);
				query = query.Where(ppc => ppc.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Push Provider Configuration, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.providerId.Contains(anyStringContains)
			       || x.configurationJson.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.providerId);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.PushProviderConfiguration.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/PushProviderConfiguration/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
