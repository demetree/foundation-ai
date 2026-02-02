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
    /// This auto generated class provides the basic CRUD operations for the EscalationPolicy entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the EscalationPolicy entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class EscalationPoliciesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 150;

		static object escalationPolicyPutSyncRoot = new object();
		static object escalationPolicyDeleteSyncRoot = new object();

		private AlertingContext _context;

		private ILogger<EscalationPoliciesController> _logger;

		public EscalationPoliciesController(AlertingContext context, ILogger<EscalationPoliciesController> logger) : base("Alerting", "EscalationPolicy")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of EscalationPolicies filtered by the parameters provided.
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
		[Route("api/EscalationPolicies")]
		public async Task<IActionResult> GetEscalationPolicies(
			string name = null,
			string description = null,
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

			IQueryable<Database.EscalationPolicy> query = (from ep in _context.EscalationPolicies select ep);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ep => ep.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ep => ep.description == description);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ep => ep.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ep => ep.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ep => ep.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ep => ep.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ep => ep.deleted == false);
				}
			}
			else
			{
				query = query.Where(ep => ep.active == true);
				query = query.Where(ep => ep.deleted == false);
			}

			query = query.OrderBy(ep => ep.name).ThenBy(ep => ep.description);

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
			// Add the any string contains parameter to span all the string fields on the Escalation Policy, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.EscalationPolicy> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.EscalationPolicy escalationPolicy in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(escalationPolicy, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Alerting.EscalationPolicy Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Alerting.EscalationPolicy Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of EscalationPolicies filtered by the parameters provided.  Its query is similar to the GetEscalationPolicies method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EscalationPolicies/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
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


			IQueryable<Database.EscalationPolicy> query = (from ep in _context.EscalationPolicies select ep);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(ep => ep.name == name);
			}
			if (description != null)
			{
				query = query.Where(ep => ep.description == description);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ep => ep.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ep => ep.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ep => ep.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ep => ep.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ep => ep.deleted == false);
				}
			}
			else
			{
				query = query.Where(ep => ep.active == true);
				query = query.Where(ep => ep.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Escalation Policy, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single EscalationPolicy by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EscalationPolicy/{id}")]
		public async Task<IActionResult> GetEscalationPolicy(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.EscalationPolicy> query = (from ep in _context.EscalationPolicies where
							(ep.id == id) &&
							(userIsAdmin == true || ep.deleted == false) &&
							(userIsWriter == true || ep.active == true)
					select ep);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.EscalationPolicy materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Alerting.EscalationPolicy Entity was read with Admin privilege." : "Alerting.EscalationPolicy Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "EscalationPolicy", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Alerting.EscalationPolicy entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Alerting.EscalationPolicy.   Entity was read with Admin privilege." : "Exception caught during entity read of Alerting.EscalationPolicy.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing EscalationPolicy record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/EscalationPolicy/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutEscalationPolicy(int id, [FromBody]Database.EscalationPolicy.EscalationPolicyDTO escalationPolicyDTO, CancellationToken cancellationToken = default)
		{
			if (escalationPolicyDTO == null)
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



			if (id != escalationPolicyDTO.id)
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


			IQueryable<Database.EscalationPolicy> query = (from x in _context.EscalationPolicies
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.EscalationPolicy existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.EscalationPolicy PUT", id.ToString(), new Exception("No Alerting.EscalationPolicy entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (escalationPolicyDTO.objectGuid == Guid.Empty)
            {
                escalationPolicyDTO.objectGuid = existing.objectGuid;
            }
            else if (escalationPolicyDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a EscalationPolicy record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.EscalationPolicy cloneOfExisting = (Database.EscalationPolicy)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new EscalationPolicy object using the data from the existing record, updated with what is in the DTO.
			//
			Database.EscalationPolicy escalationPolicy = (Database.EscalationPolicy)_context.Entry(existing).GetDatabaseValues().ToObject();
			escalationPolicy.ApplyDTO(escalationPolicyDTO);
			//
			// The tenant guid for any EscalationPolicy being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the EscalationPolicy because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				escalationPolicy.tenantGuid = existing.tenantGuid;
			}

			lock (escalationPolicyPutSyncRoot)
			{
				//
				// Validate the version number for the escalationPolicy being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != escalationPolicy.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "EscalationPolicy save attempt was made but save request was with version " + escalationPolicy.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The EscalationPolicy you are trying to update has already changed.  Please try your save again after reloading the EscalationPolicy.");
				}
				else
				{
					// Same record.  Increase version.
					escalationPolicy.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (escalationPolicy.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Alerting.EscalationPolicy record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (escalationPolicy.name != null && escalationPolicy.name.Length > 100)
				{
					escalationPolicy.name = escalationPolicy.name.Substring(0, 100);
				}

				if (escalationPolicy.description != null && escalationPolicy.description.Length > 500)
				{
					escalationPolicy.description = escalationPolicy.description.Substring(0, 500);
				}

				try
				{
				    EntityEntry<Database.EscalationPolicy> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(escalationPolicy);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        EscalationPolicyChangeHistory escalationPolicyChangeHistory = new EscalationPolicyChangeHistory();
				        escalationPolicyChangeHistory.escalationPolicyId = escalationPolicy.id;
				        escalationPolicyChangeHistory.versionNumber = escalationPolicy.versionNumber;
				        escalationPolicyChangeHistory.timeStamp = DateTime.UtcNow;
				        escalationPolicyChangeHistory.userId = securityUser.id;
				        escalationPolicyChangeHistory.tenantGuid = userTenantGuid;
				        escalationPolicyChangeHistory.data = JsonSerializer.Serialize(Database.EscalationPolicy.CreateAnonymousWithFirstLevelSubObjects(escalationPolicy));
				        _context.EscalationPolicyChangeHistories.Add(escalationPolicyChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.EscalationPolicy entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.EscalationPolicy.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.EscalationPolicy.CreateAnonymousWithFirstLevelSubObjects(escalationPolicy)),
						null);

				return Ok(Database.EscalationPolicy.CreateAnonymous(escalationPolicy));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.EscalationPolicy entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.EscalationPolicy.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.EscalationPolicy.CreateAnonymousWithFirstLevelSubObjects(escalationPolicy)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new EscalationPolicy record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EscalationPolicy", Name = "EscalationPolicy")]
		public async Task<IActionResult> PostEscalationPolicy([FromBody]Database.EscalationPolicy.EscalationPolicyDTO escalationPolicyDTO, CancellationToken cancellationToken = default)
		{
			if (escalationPolicyDTO == null)
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
			// Create a new EscalationPolicy object using the data from the DTO
			//
			Database.EscalationPolicy escalationPolicy = Database.EscalationPolicy.FromDTO(escalationPolicyDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				escalationPolicy.tenantGuid = userTenantGuid;

				if (escalationPolicy.name != null && escalationPolicy.name.Length > 100)
				{
					escalationPolicy.name = escalationPolicy.name.Substring(0, 100);
				}

				if (escalationPolicy.description != null && escalationPolicy.description.Length > 500)
				{
					escalationPolicy.description = escalationPolicy.description.Substring(0, 500);
				}

				escalationPolicy.objectGuid = Guid.NewGuid();
				escalationPolicy.versionNumber = 1;

				_context.EscalationPolicies.Add(escalationPolicy);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the escalationPolicy object so that no further changes will be written to the database
				    //
				    _context.Entry(escalationPolicy).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					escalationPolicy.EscalationPolicyChangeHistories = null;
					escalationPolicy.EscalationRules = null;
					escalationPolicy.Services = null;


				    EscalationPolicyChangeHistory escalationPolicyChangeHistory = new EscalationPolicyChangeHistory();
				    escalationPolicyChangeHistory.escalationPolicyId = escalationPolicy.id;
				    escalationPolicyChangeHistory.versionNumber = escalationPolicy.versionNumber;
				    escalationPolicyChangeHistory.timeStamp = DateTime.UtcNow;
				    escalationPolicyChangeHistory.userId = securityUser.id;
				    escalationPolicyChangeHistory.tenantGuid = userTenantGuid;
				    escalationPolicyChangeHistory.data = JsonSerializer.Serialize(Database.EscalationPolicy.CreateAnonymousWithFirstLevelSubObjects(escalationPolicy));
				    _context.EscalationPolicyChangeHistories.Add(escalationPolicyChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Alerting.EscalationPolicy entity successfully created.",
						true,
						escalationPolicy. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.EscalationPolicy.CreateAnonymousWithFirstLevelSubObjects(escalationPolicy)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Alerting.EscalationPolicy entity creation failed.", false, escalationPolicy.id.ToString(), "", JsonSerializer.Serialize(Database.EscalationPolicy.CreateAnonymousWithFirstLevelSubObjects(escalationPolicy)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "EscalationPolicy", escalationPolicy.id, escalationPolicy.name));

			return CreatedAtRoute("EscalationPolicy", new { id = escalationPolicy.id }, Database.EscalationPolicy.CreateAnonymousWithFirstLevelSubObjects(escalationPolicy));
		}



        /// <summary>
        /// 
        /// This rolls a EscalationPolicy entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EscalationPolicy/Rollback/{id}")]
		[Route("api/EscalationPolicy/Rollback")]
		public async Task<IActionResult> RollbackToEscalationPolicyVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.EscalationPolicy> query = (from x in _context.EscalationPolicies
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this EscalationPolicy concurrently
			//
			lock (escalationPolicyPutSyncRoot)
			{
				
				Database.EscalationPolicy escalationPolicy = query.FirstOrDefault();
				
				if (escalationPolicy == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.EscalationPolicy rollback", id.ToString(), new Exception("No Alerting.EscalationPolicy entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the EscalationPolicy current state so we can log it.
				//
				Database.EscalationPolicy cloneOfExisting = (Database.EscalationPolicy)_context.Entry(escalationPolicy).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.EscalationPolicyChangeHistories = null;
				cloneOfExisting.EscalationRules = null;
				cloneOfExisting.Services = null;

				if (versionNumber >= escalationPolicy.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Alerting.EscalationPolicy rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Alerting.EscalationPolicy rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				EscalationPolicyChangeHistory escalationPolicyChangeHistory = (from x in _context.EscalationPolicyChangeHistories
				                                               where
				                                               x.escalationPolicyId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (escalationPolicyChangeHistory != null)
				{
				    Database.EscalationPolicy oldEscalationPolicy = JsonSerializer.Deserialize<Database.EscalationPolicy>(escalationPolicyChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    escalationPolicy.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    escalationPolicy.name = oldEscalationPolicy.name;
				    escalationPolicy.description = oldEscalationPolicy.description;
				    escalationPolicy.objectGuid = oldEscalationPolicy.objectGuid;
				    escalationPolicy.active = oldEscalationPolicy.active;
				    escalationPolicy.deleted = oldEscalationPolicy.deleted;

				    string serializedEscalationPolicy = JsonSerializer.Serialize(escalationPolicy);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        EscalationPolicyChangeHistory newEscalationPolicyChangeHistory = new EscalationPolicyChangeHistory();
				        newEscalationPolicyChangeHistory.escalationPolicyId = escalationPolicy.id;
				        newEscalationPolicyChangeHistory.versionNumber = escalationPolicy.versionNumber;
				        newEscalationPolicyChangeHistory.timeStamp = DateTime.UtcNow;
				        newEscalationPolicyChangeHistory.userId = securityUser.id;
				        newEscalationPolicyChangeHistory.tenantGuid = userTenantGuid;
				        newEscalationPolicyChangeHistory.data = JsonSerializer.Serialize(Database.EscalationPolicy.CreateAnonymousWithFirstLevelSubObjects(escalationPolicy));
				        _context.EscalationPolicyChangeHistories.Add(newEscalationPolicyChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.EscalationPolicy rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.EscalationPolicy.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.EscalationPolicy.CreateAnonymousWithFirstLevelSubObjects(escalationPolicy)),
						null);


				    return Ok(Database.EscalationPolicy.CreateAnonymous(escalationPolicy));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Alerting.EscalationPolicy rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Alerting.EscalationPolicy rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a EscalationPolicy.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the EscalationPolicy</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EscalationPolicy/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetEscalationPolicyChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.EscalationPolicy escalationPolicy = await _context.EscalationPolicies.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (escalationPolicy == null)
			{
				return NotFound();
			}

			try
			{
				escalationPolicy.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.EscalationPolicy> versionInfo = await escalationPolicy.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a EscalationPolicy.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the EscalationPolicy</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EscalationPolicy/{id}/AuditHistory")]
		public async Task<IActionResult> GetEscalationPolicyAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.EscalationPolicy escalationPolicy = await _context.EscalationPolicies.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (escalationPolicy == null)
			{
				return NotFound();
			}

			try
			{
				escalationPolicy.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.EscalationPolicy>> versions = await escalationPolicy.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a EscalationPolicy.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the EscalationPolicy</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The EscalationPolicy object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EscalationPolicy/{id}/Version/{version}")]
		public async Task<IActionResult> GetEscalationPolicyVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.EscalationPolicy escalationPolicy = await _context.EscalationPolicies.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (escalationPolicy == null)
			{
				return NotFound();
			}

			try
			{
				escalationPolicy.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.EscalationPolicy> versionInfo = await escalationPolicy.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a EscalationPolicy at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the EscalationPolicy</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The EscalationPolicy object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EscalationPolicy/{id}/StateAtTime")]
		public async Task<IActionResult> GetEscalationPolicyStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.EscalationPolicy escalationPolicy = await _context.EscalationPolicies.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (escalationPolicy == null)
			{
				return NotFound();
			}

			try
			{
				escalationPolicy.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.EscalationPolicy> versionInfo = await escalationPolicy.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a EscalationPolicy record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EscalationPolicy/{id}")]
		[Route("api/EscalationPolicy")]
		public async Task<IActionResult> DeleteEscalationPolicy(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.EscalationPolicy> query = (from x in _context.EscalationPolicies
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.EscalationPolicy escalationPolicy = await query.FirstOrDefaultAsync(cancellationToken);

			if (escalationPolicy == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.EscalationPolicy DELETE", id.ToString(), new Exception("No Alerting.EscalationPolicy entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.EscalationPolicy cloneOfExisting = (Database.EscalationPolicy)_context.Entry(escalationPolicy).GetDatabaseValues().ToObject();


			lock (escalationPolicyDeleteSyncRoot)
			{
			    try
			    {
			        escalationPolicy.deleted = true;
			        escalationPolicy.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        EscalationPolicyChangeHistory escalationPolicyChangeHistory = new EscalationPolicyChangeHistory();
			        escalationPolicyChangeHistory.escalationPolicyId = escalationPolicy.id;
			        escalationPolicyChangeHistory.versionNumber = escalationPolicy.versionNumber;
			        escalationPolicyChangeHistory.timeStamp = DateTime.UtcNow;
			        escalationPolicyChangeHistory.userId = securityUser.id;
			        escalationPolicyChangeHistory.tenantGuid = userTenantGuid;
			        escalationPolicyChangeHistory.data = JsonSerializer.Serialize(Database.EscalationPolicy.CreateAnonymousWithFirstLevelSubObjects(escalationPolicy));
			        _context.EscalationPolicyChangeHistories.Add(escalationPolicyChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Alerting.EscalationPolicy entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.EscalationPolicy.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.EscalationPolicy.CreateAnonymousWithFirstLevelSubObjects(escalationPolicy)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Alerting.EscalationPolicy entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.EscalationPolicy.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.EscalationPolicy.CreateAnonymousWithFirstLevelSubObjects(escalationPolicy)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of EscalationPolicy records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/EscalationPolicies/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
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

			IQueryable<Database.EscalationPolicy> query = (from ep in _context.EscalationPolicies select ep);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ep => ep.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ep => ep.description == description);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ep => ep.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ep => ep.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ep => ep.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ep => ep.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ep => ep.deleted == false);
				}
			}
			else
			{
				query = query.Where(ep => ep.active == true);
				query = query.Where(ep => ep.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Escalation Policy, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name).ThenBy(x => x.description);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.EscalationPolicy.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/EscalationPolicy/CreateAuditEvent")]
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
