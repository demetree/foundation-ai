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
using Foundation.ChangeHistory;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the SchedulingTargetQualificationRequirement entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SchedulingTargetQualificationRequirement entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SchedulingTargetQualificationRequirementsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object schedulingTargetQualificationRequirementPutSyncRoot = new object();
		static object schedulingTargetQualificationRequirementDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<SchedulingTargetQualificationRequirementsController> _logger;

		public SchedulingTargetQualificationRequirementsController(SchedulerContext context, ILogger<SchedulingTargetQualificationRequirementsController> logger) : base("Scheduler", "SchedulingTargetQualificationRequirement")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of SchedulingTargetQualificationRequirements filtered by the parameters provided.
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
		[Route("api/SchedulingTargetQualificationRequirements")]
		public async Task<IActionResult> GetSchedulingTargetQualificationRequirements(
			int? schedulingTargetId = null,
			int? qualificationId = null,
			bool? isRequired = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			int? pageSize = null,
			int? pageNumber = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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

			IQueryable<Database.SchedulingTargetQualificationRequirement> query = (from stqr in _context.SchedulingTargetQualificationRequirements select stqr);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (schedulingTargetId.HasValue == true)
			{
				query = query.Where(stqr => stqr.schedulingTargetId == schedulingTargetId.Value);
			}
			if (qualificationId.HasValue == true)
			{
				query = query.Where(stqr => stqr.qualificationId == qualificationId.Value);
			}
			if (isRequired.HasValue == true)
			{
				query = query.Where(stqr => stqr.isRequired == isRequired.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(stqr => stqr.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(stqr => stqr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(stqr => stqr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(stqr => stqr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(stqr => stqr.deleted == false);
				}
			}
			else
			{
				query = query.Where(stqr => stqr.active == true);
				query = query.Where(stqr => stqr.deleted == false);
			}

			query = query.OrderBy(stqr => stqr.id);

			if (includeRelations == true)
			{
				query = query.Include(x => x.qualification);
				query = query.Include(x => x.schedulingTarget);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.SchedulingTargetQualificationRequirement> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SchedulingTargetQualificationRequirement schedulingTargetQualificationRequirement in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(schedulingTargetQualificationRequirement, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.SchedulingTargetQualificationRequirement Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.SchedulingTargetQualificationRequirement Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of SchedulingTargetQualificationRequirements filtered by the parameters provided.  Its query is similar to the GetSchedulingTargetQualificationRequirements method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTargetQualificationRequirements/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? schedulingTargetId = null,
			int? qualificationId = null,
			bool? isRequired = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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


			IQueryable<Database.SchedulingTargetQualificationRequirement> query = (from stqr in _context.SchedulingTargetQualificationRequirements select stqr);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (schedulingTargetId.HasValue == true)
			{
				query = query.Where(stqr => stqr.schedulingTargetId == schedulingTargetId.Value);
			}
			if (qualificationId.HasValue == true)
			{
				query = query.Where(stqr => stqr.qualificationId == qualificationId.Value);
			}
			if (isRequired.HasValue == true)
			{
				query = query.Where(stqr => stqr.isRequired == isRequired.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(stqr => stqr.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(stqr => stqr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(stqr => stqr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(stqr => stqr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(stqr => stqr.deleted == false);
				}
			}
			else
			{
				query = query.Where(stqr => stqr.active == true);
				query = query.Where(stqr => stqr.deleted == false);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single SchedulingTargetQualificationRequirement by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTargetQualificationRequirement/{id}")]
		public async Task<IActionResult> GetSchedulingTargetQualificationRequirement(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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
				IQueryable<Database.SchedulingTargetQualificationRequirement> query = (from stqr in _context.SchedulingTargetQualificationRequirements where
							(stqr.id == id) &&
							(userIsAdmin == true || stqr.deleted == false) &&
							(userIsWriter == true || stqr.active == true)
					select stqr);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.qualification);
					query = query.Include(x => x.schedulingTarget);
					query = query.AsSplitQuery();
				}

				Database.SchedulingTargetQualificationRequirement materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.SchedulingTargetQualificationRequirement Entity was read with Admin privilege." : "Scheduler.SchedulingTargetQualificationRequirement Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SchedulingTargetQualificationRequirement", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.SchedulingTargetQualificationRequirement entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.SchedulingTargetQualificationRequirement.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.SchedulingTargetQualificationRequirement.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing SchedulingTargetQualificationRequirement record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SchedulingTargetQualificationRequirement/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSchedulingTargetQualificationRequirement(int id, [FromBody]Database.SchedulingTargetQualificationRequirement.SchedulingTargetQualificationRequirementDTO schedulingTargetQualificationRequirementDTO, CancellationToken cancellationToken = default)
		{
			if (schedulingTargetQualificationRequirementDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Config Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != schedulingTargetQualificationRequirementDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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


			IQueryable<Database.SchedulingTargetQualificationRequirement> query = (from x in _context.SchedulingTargetQualificationRequirements
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.SchedulingTargetQualificationRequirement existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.SchedulingTargetQualificationRequirement PUT", id.ToString(), new Exception("No Scheduler.SchedulingTargetQualificationRequirement entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (schedulingTargetQualificationRequirementDTO.objectGuid == Guid.Empty)
            {
                schedulingTargetQualificationRequirementDTO.objectGuid = existing.objectGuid;
            }
            else if (schedulingTargetQualificationRequirementDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a SchedulingTargetQualificationRequirement record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SchedulingTargetQualificationRequirement cloneOfExisting = (Database.SchedulingTargetQualificationRequirement)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SchedulingTargetQualificationRequirement object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SchedulingTargetQualificationRequirement schedulingTargetQualificationRequirement = (Database.SchedulingTargetQualificationRequirement)_context.Entry(existing).GetDatabaseValues().ToObject();
			schedulingTargetQualificationRequirement.ApplyDTO(schedulingTargetQualificationRequirementDTO);
			//
			// The tenant guid for any SchedulingTargetQualificationRequirement being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the SchedulingTargetQualificationRequirement because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				schedulingTargetQualificationRequirement.tenantGuid = existing.tenantGuid;
			}

			lock (schedulingTargetQualificationRequirementPutSyncRoot)
			{
				//
				// Validate the version number for the schedulingTargetQualificationRequirement being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != schedulingTargetQualificationRequirement.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "SchedulingTargetQualificationRequirement save attempt was made but save request was with version " + schedulingTargetQualificationRequirement.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The SchedulingTargetQualificationRequirement you are trying to update has already changed.  Please try your save again after reloading the SchedulingTargetQualificationRequirement.");
				}
				else
				{
					// Same record.  Increase version.
					schedulingTargetQualificationRequirement.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (schedulingTargetQualificationRequirement.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.SchedulingTargetQualificationRequirement record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				try
				{
				    EntityEntry<Database.SchedulingTargetQualificationRequirement> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(schedulingTargetQualificationRequirement);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        SchedulingTargetQualificationRequirementChangeHistory schedulingTargetQualificationRequirementChangeHistory = new SchedulingTargetQualificationRequirementChangeHistory();
				        schedulingTargetQualificationRequirementChangeHistory.schedulingTargetQualificationRequirementId = schedulingTargetQualificationRequirement.id;
				        schedulingTargetQualificationRequirementChangeHistory.versionNumber = schedulingTargetQualificationRequirement.versionNumber;
				        schedulingTargetQualificationRequirementChangeHistory.timeStamp = DateTime.UtcNow;
				        schedulingTargetQualificationRequirementChangeHistory.userId = securityUser.id;
				        schedulingTargetQualificationRequirementChangeHistory.tenantGuid = userTenantGuid;
				        schedulingTargetQualificationRequirementChangeHistory.data = JsonSerializer.Serialize(Database.SchedulingTargetQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetQualificationRequirement));
				        _context.SchedulingTargetQualificationRequirementChangeHistories.Add(schedulingTargetQualificationRequirementChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.SchedulingTargetQualificationRequirement entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.SchedulingTargetQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.SchedulingTargetQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetQualificationRequirement)),
						null);

				return Ok(Database.SchedulingTargetQualificationRequirement.CreateAnonymous(schedulingTargetQualificationRequirement));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.SchedulingTargetQualificationRequirement entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.SchedulingTargetQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.SchedulingTargetQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetQualificationRequirement)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new SchedulingTargetQualificationRequirement record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTargetQualificationRequirement", Name = "SchedulingTargetQualificationRequirement")]
		public async Task<IActionResult> PostSchedulingTargetQualificationRequirement([FromBody]Database.SchedulingTargetQualificationRequirement.SchedulingTargetQualificationRequirementDTO schedulingTargetQualificationRequirementDTO, CancellationToken cancellationToken = default)
		{
			if (schedulingTargetQualificationRequirementDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Config Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new SchedulingTargetQualificationRequirement object using the data from the DTO
			//
			Database.SchedulingTargetQualificationRequirement schedulingTargetQualificationRequirement = Database.SchedulingTargetQualificationRequirement.FromDTO(schedulingTargetQualificationRequirementDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				schedulingTargetQualificationRequirement.tenantGuid = userTenantGuid;

				schedulingTargetQualificationRequirement.objectGuid = Guid.NewGuid();
				schedulingTargetQualificationRequirement.versionNumber = 1;

				_context.SchedulingTargetQualificationRequirements.Add(schedulingTargetQualificationRequirement);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the schedulingTargetQualificationRequirement object so that no further changes will be written to the database
				    //
				    _context.Entry(schedulingTargetQualificationRequirement).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					schedulingTargetQualificationRequirement.SchedulingTargetQualificationRequirementChangeHistories = null;
					schedulingTargetQualificationRequirement.qualification = null;
					schedulingTargetQualificationRequirement.schedulingTarget = null;


				    SchedulingTargetQualificationRequirementChangeHistory schedulingTargetQualificationRequirementChangeHistory = new SchedulingTargetQualificationRequirementChangeHistory();
				    schedulingTargetQualificationRequirementChangeHistory.schedulingTargetQualificationRequirementId = schedulingTargetQualificationRequirement.id;
				    schedulingTargetQualificationRequirementChangeHistory.versionNumber = schedulingTargetQualificationRequirement.versionNumber;
				    schedulingTargetQualificationRequirementChangeHistory.timeStamp = DateTime.UtcNow;
				    schedulingTargetQualificationRequirementChangeHistory.userId = securityUser.id;
				    schedulingTargetQualificationRequirementChangeHistory.tenantGuid = userTenantGuid;
				    schedulingTargetQualificationRequirementChangeHistory.data = JsonSerializer.Serialize(Database.SchedulingTargetQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetQualificationRequirement));
				    _context.SchedulingTargetQualificationRequirementChangeHistories.Add(schedulingTargetQualificationRequirementChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.SchedulingTargetQualificationRequirement entity successfully created.",
						true,
						schedulingTargetQualificationRequirement. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.SchedulingTargetQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetQualificationRequirement)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.SchedulingTargetQualificationRequirement entity creation failed.", false, schedulingTargetQualificationRequirement.id.ToString(), "", JsonSerializer.Serialize(Database.SchedulingTargetQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetQualificationRequirement)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SchedulingTargetQualificationRequirement", schedulingTargetQualificationRequirement.id, schedulingTargetQualificationRequirement.id.ToString()));

			return CreatedAtRoute("SchedulingTargetQualificationRequirement", new { id = schedulingTargetQualificationRequirement.id }, Database.SchedulingTargetQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetQualificationRequirement));
		}



        /// <summary>
        /// 
        /// This rolls a SchedulingTargetQualificationRequirement entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTargetQualificationRequirement/Rollback/{id}")]
		[Route("api/SchedulingTargetQualificationRequirement/Rollback")]
		public async Task<IActionResult> RollbackToSchedulingTargetQualificationRequirementVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.SchedulingTargetQualificationRequirement> query = (from x in _context.SchedulingTargetQualificationRequirements
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this SchedulingTargetQualificationRequirement concurrently
			//
			lock (schedulingTargetQualificationRequirementPutSyncRoot)
			{
				
				Database.SchedulingTargetQualificationRequirement schedulingTargetQualificationRequirement = query.FirstOrDefault();
				
				if (schedulingTargetQualificationRequirement == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.SchedulingTargetQualificationRequirement rollback", id.ToString(), new Exception("No Scheduler.SchedulingTargetQualificationRequirement entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the SchedulingTargetQualificationRequirement current state so we can log it.
				//
				Database.SchedulingTargetQualificationRequirement cloneOfExisting = (Database.SchedulingTargetQualificationRequirement)_context.Entry(schedulingTargetQualificationRequirement).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.SchedulingTargetQualificationRequirementChangeHistories = null;
				cloneOfExisting.qualification = null;
				cloneOfExisting.schedulingTarget = null;

				if (versionNumber >= schedulingTargetQualificationRequirement.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.SchedulingTargetQualificationRequirement rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.SchedulingTargetQualificationRequirement rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				SchedulingTargetQualificationRequirementChangeHistory schedulingTargetQualificationRequirementChangeHistory = (from x in _context.SchedulingTargetQualificationRequirementChangeHistories
				                                               where
				                                               x.schedulingTargetQualificationRequirementId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (schedulingTargetQualificationRequirementChangeHistory != null)
				{
				    Database.SchedulingTargetQualificationRequirement oldSchedulingTargetQualificationRequirement = JsonSerializer.Deserialize<Database.SchedulingTargetQualificationRequirement>(schedulingTargetQualificationRequirementChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    schedulingTargetQualificationRequirement.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    schedulingTargetQualificationRequirement.schedulingTargetId = oldSchedulingTargetQualificationRequirement.schedulingTargetId;
				    schedulingTargetQualificationRequirement.qualificationId = oldSchedulingTargetQualificationRequirement.qualificationId;
				    schedulingTargetQualificationRequirement.isRequired = oldSchedulingTargetQualificationRequirement.isRequired;
				    schedulingTargetQualificationRequirement.objectGuid = oldSchedulingTargetQualificationRequirement.objectGuid;
				    schedulingTargetQualificationRequirement.active = oldSchedulingTargetQualificationRequirement.active;
				    schedulingTargetQualificationRequirement.deleted = oldSchedulingTargetQualificationRequirement.deleted;

				    string serializedSchedulingTargetQualificationRequirement = JsonSerializer.Serialize(schedulingTargetQualificationRequirement);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        SchedulingTargetQualificationRequirementChangeHistory newSchedulingTargetQualificationRequirementChangeHistory = new SchedulingTargetQualificationRequirementChangeHistory();
				        newSchedulingTargetQualificationRequirementChangeHistory.schedulingTargetQualificationRequirementId = schedulingTargetQualificationRequirement.id;
				        newSchedulingTargetQualificationRequirementChangeHistory.versionNumber = schedulingTargetQualificationRequirement.versionNumber;
				        newSchedulingTargetQualificationRequirementChangeHistory.timeStamp = DateTime.UtcNow;
				        newSchedulingTargetQualificationRequirementChangeHistory.userId = securityUser.id;
				        newSchedulingTargetQualificationRequirementChangeHistory.tenantGuid = userTenantGuid;
				        newSchedulingTargetQualificationRequirementChangeHistory.data = JsonSerializer.Serialize(Database.SchedulingTargetQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetQualificationRequirement));
				        _context.SchedulingTargetQualificationRequirementChangeHistories.Add(newSchedulingTargetQualificationRequirementChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.SchedulingTargetQualificationRequirement rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.SchedulingTargetQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.SchedulingTargetQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetQualificationRequirement)),
						null);


				    return Ok(Database.SchedulingTargetQualificationRequirement.CreateAnonymous(schedulingTargetQualificationRequirement));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.SchedulingTargetQualificationRequirement rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.SchedulingTargetQualificationRequirement rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a SchedulingTargetQualificationRequirement.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the SchedulingTargetQualificationRequirement</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTargetQualificationRequirement/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetSchedulingTargetQualificationRequirementChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.SchedulingTargetQualificationRequirement schedulingTargetQualificationRequirement = await _context.SchedulingTargetQualificationRequirements.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (schedulingTargetQualificationRequirement == null)
			{
				return NotFound();
			}

			try
			{
				schedulingTargetQualificationRequirement.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.SchedulingTargetQualificationRequirement> versionInfo = await schedulingTargetQualificationRequirement.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a SchedulingTargetQualificationRequirement.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the SchedulingTargetQualificationRequirement</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTargetQualificationRequirement/{id}/AuditHistory")]
		public async Task<IActionResult> GetSchedulingTargetQualificationRequirementAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.SchedulingTargetQualificationRequirement schedulingTargetQualificationRequirement = await _context.SchedulingTargetQualificationRequirements.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (schedulingTargetQualificationRequirement == null)
			{
				return NotFound();
			}

			try
			{
				schedulingTargetQualificationRequirement.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.SchedulingTargetQualificationRequirement>> versions = await schedulingTargetQualificationRequirement.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a SchedulingTargetQualificationRequirement.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the SchedulingTargetQualificationRequirement</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The SchedulingTargetQualificationRequirement object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTargetQualificationRequirement/{id}/Version/{version}")]
		public async Task<IActionResult> GetSchedulingTargetQualificationRequirementVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.SchedulingTargetQualificationRequirement schedulingTargetQualificationRequirement = await _context.SchedulingTargetQualificationRequirements.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (schedulingTargetQualificationRequirement == null)
			{
				return NotFound();
			}

			try
			{
				schedulingTargetQualificationRequirement.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.SchedulingTargetQualificationRequirement> versionInfo = await schedulingTargetQualificationRequirement.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a SchedulingTargetQualificationRequirement at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the SchedulingTargetQualificationRequirement</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The SchedulingTargetQualificationRequirement object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTargetQualificationRequirement/{id}/StateAtTime")]
		public async Task<IActionResult> GetSchedulingTargetQualificationRequirementStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.SchedulingTargetQualificationRequirement schedulingTargetQualificationRequirement = await _context.SchedulingTargetQualificationRequirements.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (schedulingTargetQualificationRequirement == null)
			{
				return NotFound();
			}

			try
			{
				schedulingTargetQualificationRequirement.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.SchedulingTargetQualificationRequirement> versionInfo = await schedulingTargetQualificationRequirement.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a SchedulingTargetQualificationRequirement record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTargetQualificationRequirement/{id}")]
		[Route("api/SchedulingTargetQualificationRequirement")]
		public async Task<IActionResult> DeleteSchedulingTargetQualificationRequirement(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Config Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.SchedulingTargetQualificationRequirement> query = (from x in _context.SchedulingTargetQualificationRequirements
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.SchedulingTargetQualificationRequirement schedulingTargetQualificationRequirement = await query.FirstOrDefaultAsync(cancellationToken);

			if (schedulingTargetQualificationRequirement == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.SchedulingTargetQualificationRequirement DELETE", id.ToString(), new Exception("No Scheduler.SchedulingTargetQualificationRequirement entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SchedulingTargetQualificationRequirement cloneOfExisting = (Database.SchedulingTargetQualificationRequirement)_context.Entry(schedulingTargetQualificationRequirement).GetDatabaseValues().ToObject();


			lock (schedulingTargetQualificationRequirementDeleteSyncRoot)
			{
			    try
			    {
			        schedulingTargetQualificationRequirement.deleted = true;
			        schedulingTargetQualificationRequirement.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        SchedulingTargetQualificationRequirementChangeHistory schedulingTargetQualificationRequirementChangeHistory = new SchedulingTargetQualificationRequirementChangeHistory();
			        schedulingTargetQualificationRequirementChangeHistory.schedulingTargetQualificationRequirementId = schedulingTargetQualificationRequirement.id;
			        schedulingTargetQualificationRequirementChangeHistory.versionNumber = schedulingTargetQualificationRequirement.versionNumber;
			        schedulingTargetQualificationRequirementChangeHistory.timeStamp = DateTime.UtcNow;
			        schedulingTargetQualificationRequirementChangeHistory.userId = securityUser.id;
			        schedulingTargetQualificationRequirementChangeHistory.tenantGuid = userTenantGuid;
			        schedulingTargetQualificationRequirementChangeHistory.data = JsonSerializer.Serialize(Database.SchedulingTargetQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetQualificationRequirement));
			        _context.SchedulingTargetQualificationRequirementChangeHistories.Add(schedulingTargetQualificationRequirementChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.SchedulingTargetQualificationRequirement entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.SchedulingTargetQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.SchedulingTargetQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetQualificationRequirement)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.SchedulingTargetQualificationRequirement entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.SchedulingTargetQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.SchedulingTargetQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetQualificationRequirement)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of SchedulingTargetQualificationRequirement records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SchedulingTargetQualificationRequirements/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? schedulingTargetId = null,
			int? qualificationId = null,
			bool? isRequired = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);


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

			IQueryable<Database.SchedulingTargetQualificationRequirement> query = (from stqr in _context.SchedulingTargetQualificationRequirements select stqr);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (schedulingTargetId.HasValue == true)
			{
				query = query.Where(stqr => stqr.schedulingTargetId == schedulingTargetId.Value);
			}
			if (qualificationId.HasValue == true)
			{
				query = query.Where(stqr => stqr.qualificationId == qualificationId.Value);
			}
			if (isRequired.HasValue == true)
			{
				query = query.Where(stqr => stqr.isRequired == isRequired.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(stqr => stqr.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(stqr => stqr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(stqr => stqr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(stqr => stqr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(stqr => stqr.deleted == false);
				}
			}
			else
			{
				query = query.Where(stqr => stqr.active == true);
				query = query.Where(stqr => stqr.deleted == false);
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.SchedulingTargetQualificationRequirement.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/SchedulingTargetQualificationRequirement/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Config Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
