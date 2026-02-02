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
using Foundation.Alerting.Database;
using Foundation.ChangeHistory;

namespace Foundation.Alerting.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the Service entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Service entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ServicesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 150;

		static object servicePutSyncRoot = new object();
		static object serviceDeleteSyncRoot = new object();

		private AlertingContext _context;

		private ILogger<ServicesController> _logger;

		public ServicesController(AlertingContext context, ILogger<ServicesController> logger) : base("Alerting", "Service")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Services filtered by the parameters provided.
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
		[Route("api/Services")]
		public async Task<IActionResult> GetServices(
			int? escalationPolicyId = null,
			string name = null,
			string description = null,
			Guid? ownerTeamObjectGuid = null,
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
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 150, cancellationToken);
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

			IQueryable<Database.Service> query = (from s in _context.Services select s);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (escalationPolicyId.HasValue == true)
			{
				query = query.Where(s => s.escalationPolicyId == escalationPolicyId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(s => s.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(s => s.description == description);
			}
			if (ownerTeamObjectGuid.HasValue == true)
			{
				query = query.Where(s => s.ownerTeamObjectGuid == ownerTeamObjectGuid);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(s => s.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(s => s.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(s => s.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(s => s.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(s => s.deleted == false);
				}
			}
			else
			{
				query = query.Where(s => s.active == true);
				query = query.Where(s => s.deleted == false);
			}

			query = query.OrderBy(s => s.name).ThenBy(s => s.description);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.escalationPolicy);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Service, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || (includeRelations == true && x.escalationPolicy.name.Contains(anyStringContains))
			       || (includeRelations == true && x.escalationPolicy.description.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.Service> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Service service in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(service, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Alerting.Service Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Alerting.Service Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Services filtered by the parameters provided.  Its query is similar to the GetServices method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Services/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? escalationPolicyId = null,
			string name = null,
			string description = null,
			Guid? ownerTeamObjectGuid = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 150, cancellationToken);
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


			IQueryable<Database.Service> query = (from s in _context.Services select s);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (escalationPolicyId.HasValue == true)
			{
				query = query.Where(s => s.escalationPolicyId == escalationPolicyId.Value);
			}
			if (name != null)
			{
				query = query.Where(s => s.name == name);
			}
			if (description != null)
			{
				query = query.Where(s => s.description == description);
			}
			if (ownerTeamObjectGuid.HasValue == true)
			{
				query = query.Where(s => s.ownerTeamObjectGuid == ownerTeamObjectGuid);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(s => s.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(s => s.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(s => s.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(s => s.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(s => s.deleted == false);
				}
			}
			else
			{
				query = query.Where(s => s.active == true);
				query = query.Where(s => s.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Service, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.escalationPolicy.name.Contains(anyStringContains)
			       || x.escalationPolicy.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single Service by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Service/{id}")]
		public async Task<IActionResult> GetService(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 150, cancellationToken);
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
				IQueryable<Database.Service> query = (from s in _context.Services where
							(s.id == id) &&
							(userIsAdmin == true || s.deleted == false) &&
							(userIsWriter == true || s.active == true)
					select s);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.escalationPolicy);
					query = query.AsSplitQuery();
				}

				Database.Service materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Alerting.Service Entity was read with Admin privilege." : "Alerting.Service Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Service", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Alerting.Service entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Alerting.Service.   Entity was read with Admin privilege." : "Exception caught during entity read of Alerting.Service.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing Service record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Service/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutService(int id, [FromBody]Database.Service.ServiceDTO serviceDTO, CancellationToken cancellationToken = default)
		{
			if (serviceDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Alerting Master Config Writer role needed to write to this table, or Alerting Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Alerting Master Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != serviceDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 150, cancellationToken);
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


			IQueryable<Database.Service> query = (from x in _context.Services
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Service existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.Service PUT", id.ToString(), new Exception("No Alerting.Service entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (serviceDTO.objectGuid == Guid.Empty)
            {
                serviceDTO.objectGuid = existing.objectGuid;
            }
            else if (serviceDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Service record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Service cloneOfExisting = (Database.Service)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Service object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Service service = (Database.Service)_context.Entry(existing).GetDatabaseValues().ToObject();
			service.ApplyDTO(serviceDTO);
			//
			// The tenant guid for any Service being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the Service because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				service.tenantGuid = existing.tenantGuid;
			}

			lock (servicePutSyncRoot)
			{
				//
				// Validate the version number for the service being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != service.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Service save attempt was made but save request was with version " + service.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The Service you are trying to update has already changed.  Please try your save again after reloading the Service.");
				}
				else
				{
					// Same record.  Increase version.
					service.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (service.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Alerting.Service record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (service.name != null && service.name.Length > 100)
				{
					service.name = service.name.Substring(0, 100);
				}

				if (service.description != null && service.description.Length > 500)
				{
					service.description = service.description.Substring(0, 500);
				}

				try
				{
				    EntityEntry<Database.Service> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(service);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ServiceChangeHistory serviceChangeHistory = new ServiceChangeHistory();
				        serviceChangeHistory.serviceId = service.id;
				        serviceChangeHistory.versionNumber = service.versionNumber;
				        serviceChangeHistory.timeStamp = DateTime.UtcNow;
				        serviceChangeHistory.userId = securityUser.id;
				        serviceChangeHistory.tenantGuid = userTenantGuid;
				        serviceChangeHistory.data = JsonSerializer.Serialize(Database.Service.CreateAnonymousWithFirstLevelSubObjects(service));
				        _context.ServiceChangeHistories.Add(serviceChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.Service entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Service.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Service.CreateAnonymousWithFirstLevelSubObjects(service)),
						null);

				return Ok(Database.Service.CreateAnonymous(service));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.Service entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Service.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Service.CreateAnonymousWithFirstLevelSubObjects(service)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new Service record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Service", Name = "Service")]
		public async Task<IActionResult> PostService([FromBody]Database.Service.ServiceDTO serviceDTO, CancellationToken cancellationToken = default)
		{
			if (serviceDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Alerting Master Config Writer role needed to write to this table, or Alerting Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Alerting Master Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new Service object using the data from the DTO
			//
			Database.Service service = Database.Service.FromDTO(serviceDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				service.tenantGuid = userTenantGuid;

				if (service.name != null && service.name.Length > 100)
				{
					service.name = service.name.Substring(0, 100);
				}

				if (service.description != null && service.description.Length > 500)
				{
					service.description = service.description.Substring(0, 500);
				}

				service.objectGuid = Guid.NewGuid();
				service.versionNumber = 1;

				_context.Services.Add(service);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the service object so that no further changes will be written to the database
				    //
				    _context.Entry(service).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					service.Incidents = null;
					service.Integrations = null;
					service.ServiceChangeHistories = null;
					service.escalationPolicy = null;


				    ServiceChangeHistory serviceChangeHistory = new ServiceChangeHistory();
				    serviceChangeHistory.serviceId = service.id;
				    serviceChangeHistory.versionNumber = service.versionNumber;
				    serviceChangeHistory.timeStamp = DateTime.UtcNow;
				    serviceChangeHistory.userId = securityUser.id;
				    serviceChangeHistory.tenantGuid = userTenantGuid;
				    serviceChangeHistory.data = JsonSerializer.Serialize(Database.Service.CreateAnonymousWithFirstLevelSubObjects(service));
				    _context.ServiceChangeHistories.Add(serviceChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Alerting.Service entity successfully created.",
						true,
						service. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.Service.CreateAnonymousWithFirstLevelSubObjects(service)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Alerting.Service entity creation failed.", false, service.id.ToString(), "", JsonSerializer.Serialize(Database.Service.CreateAnonymousWithFirstLevelSubObjects(service)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Service", service.id, service.name));

			return CreatedAtRoute("Service", new { id = service.id }, Database.Service.CreateAnonymousWithFirstLevelSubObjects(service));
		}



        /// <summary>
        /// 
        /// This rolls a Service entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Service/Rollback/{id}")]
		[Route("api/Service/Rollback")]
		public async Task<IActionResult> RollbackToServiceVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.Service> query = (from x in _context.Services
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this Service concurrently
			//
			lock (servicePutSyncRoot)
			{
				
				Database.Service service = query.FirstOrDefault();
				
				if (service == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.Service rollback", id.ToString(), new Exception("No Alerting.Service entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the Service current state so we can log it.
				//
				Database.Service cloneOfExisting = (Database.Service)_context.Entry(service).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.Incidents = null;
				cloneOfExisting.Integrations = null;
				cloneOfExisting.ServiceChangeHistories = null;
				cloneOfExisting.escalationPolicy = null;

				if (versionNumber >= service.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Alerting.Service rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Alerting.Service rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ServiceChangeHistory serviceChangeHistory = (from x in _context.ServiceChangeHistories
				                                               where
				                                               x.serviceId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (serviceChangeHistory != null)
				{
				    Database.Service oldService = JsonSerializer.Deserialize<Database.Service>(serviceChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    service.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    service.escalationPolicyId = oldService.escalationPolicyId;
				    service.name = oldService.name;
				    service.description = oldService.description;
				    service.ownerTeamObjectGuid = oldService.ownerTeamObjectGuid;
				    service.objectGuid = oldService.objectGuid;
				    service.active = oldService.active;
				    service.deleted = oldService.deleted;

				    string serializedService = JsonSerializer.Serialize(service);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ServiceChangeHistory newServiceChangeHistory = new ServiceChangeHistory();
				        newServiceChangeHistory.serviceId = service.id;
				        newServiceChangeHistory.versionNumber = service.versionNumber;
				        newServiceChangeHistory.timeStamp = DateTime.UtcNow;
				        newServiceChangeHistory.userId = securityUser.id;
				        newServiceChangeHistory.tenantGuid = userTenantGuid;
				        newServiceChangeHistory.data = JsonSerializer.Serialize(Database.Service.CreateAnonymousWithFirstLevelSubObjects(service));
				        _context.ServiceChangeHistories.Add(newServiceChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.Service rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Service.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Service.CreateAnonymousWithFirstLevelSubObjects(service)),
						null);


				    return Ok(Database.Service.CreateAnonymous(service));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Alerting.Service rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Alerting.Service rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a Service.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Service</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Service/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetServiceChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.Service service = await _context.Services.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (service == null)
			{
				return NotFound();
			}

			try
			{
				service.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Service> versionInfo = await service.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a Service.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Service</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Service/{id}/AuditHistory")]
		public async Task<IActionResult> GetServiceAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.Service service = await _context.Services.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (service == null)
			{
				return NotFound();
			}

			try
			{
				service.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.Service>> versions = await service.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a Service.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Service</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The Service object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Service/{id}/Version/{version}")]
		public async Task<IActionResult> GetServiceVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.Service service = await _context.Services.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (service == null)
			{
				return NotFound();
			}

			try
			{
				service.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Service> versionInfo = await service.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a Service at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Service</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The Service object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Service/{id}/StateAtTime")]
		public async Task<IActionResult> GetServiceStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.Service service = await _context.Services.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (service == null)
			{
				return NotFound();
			}

			try
			{
				service.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Service> versionInfo = await service.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a Service record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Service/{id}")]
		[Route("api/Service")]
		public async Task<IActionResult> DeleteService(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Alerting Master Config Writer role needed to write to this table, or Alerting Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Alerting Master Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.Service> query = (from x in _context.Services
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Service service = await query.FirstOrDefaultAsync(cancellationToken);

			if (service == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.Service DELETE", id.ToString(), new Exception("No Alerting.Service entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Service cloneOfExisting = (Database.Service)_context.Entry(service).GetDatabaseValues().ToObject();


			lock (serviceDeleteSyncRoot)
			{
			    try
			    {
			        service.deleted = true;
			        service.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ServiceChangeHistory serviceChangeHistory = new ServiceChangeHistory();
			        serviceChangeHistory.serviceId = service.id;
			        serviceChangeHistory.versionNumber = service.versionNumber;
			        serviceChangeHistory.timeStamp = DateTime.UtcNow;
			        serviceChangeHistory.userId = securityUser.id;
			        serviceChangeHistory.tenantGuid = userTenantGuid;
			        serviceChangeHistory.data = JsonSerializer.Serialize(Database.Service.CreateAnonymousWithFirstLevelSubObjects(service));
			        _context.ServiceChangeHistories.Add(serviceChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Alerting.Service entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Service.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Service.CreateAnonymousWithFirstLevelSubObjects(service)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Alerting.Service entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Service.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Service.CreateAnonymousWithFirstLevelSubObjects(service)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of Service records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Services/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? escalationPolicyId = null,
			string name = null,
			string description = null,
			Guid? ownerTeamObjectGuid = null,
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
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 150, cancellationToken);


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

			IQueryable<Database.Service> query = (from s in _context.Services select s);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (escalationPolicyId.HasValue == true)
			{
				query = query.Where(s => s.escalationPolicyId == escalationPolicyId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(s => s.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(s => s.description == description);
			}
			if (ownerTeamObjectGuid.HasValue == true)
			{
				query = query.Where(s => s.ownerTeamObjectGuid == ownerTeamObjectGuid);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(s => s.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(s => s.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(s => s.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(s => s.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(s => s.deleted == false);
				}
			}
			else
			{
				query = query.Where(s => s.active == true);
				query = query.Where(s => s.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Service, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.escalationPolicy.name.Contains(anyStringContains)
			       || x.escalationPolicy.description.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name).ThenBy(x => x.description);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Service.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Service/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Master Config Writer role needed to write to this table, or Alerting Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Alerting Master Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
