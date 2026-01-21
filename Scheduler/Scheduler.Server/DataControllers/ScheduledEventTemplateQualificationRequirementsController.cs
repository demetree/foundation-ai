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
    /// This auto generated class provides the basic CRUD operations for the ScheduledEventTemplateQualificationRequirement entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ScheduledEventTemplateQualificationRequirement entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ScheduledEventTemplateQualificationRequirementsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object scheduledEventTemplateQualificationRequirementPutSyncRoot = new object();
		static object scheduledEventTemplateQualificationRequirementDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<ScheduledEventTemplateQualificationRequirementsController> _logger;

		public ScheduledEventTemplateQualificationRequirementsController(SchedulerContext context, ILogger<ScheduledEventTemplateQualificationRequirementsController> logger) : base("Scheduler", "ScheduledEventTemplateQualificationRequirement")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ScheduledEventTemplateQualificationRequirements filtered by the parameters provided.
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
		[Route("api/ScheduledEventTemplateQualificationRequirements")]
		public async Task<IActionResult> GetScheduledEventTemplateQualificationRequirements(
			int? scheduledEventTemplateId = null,
			int? qualificationId = null,
			bool? isRequired = null,
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

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
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

			IQueryable<Database.ScheduledEventTemplateQualificationRequirement> query = (from setqr in _context.ScheduledEventTemplateQualificationRequirements select setqr);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (scheduledEventTemplateId.HasValue == true)
			{
				query = query.Where(setqr => setqr.scheduledEventTemplateId == scheduledEventTemplateId.Value);
			}
			if (qualificationId.HasValue == true)
			{
				query = query.Where(setqr => setqr.qualificationId == qualificationId.Value);
			}
			if (isRequired.HasValue == true)
			{
				query = query.Where(setqr => setqr.isRequired == isRequired.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(setqr => setqr.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(setqr => setqr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(setqr => setqr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(setqr => setqr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(setqr => setqr.deleted == false);
				}
			}
			else
			{
				query = query.Where(setqr => setqr.active == true);
				query = query.Where(setqr => setqr.deleted == false);
			}

			query = query.OrderBy(setqr => setqr.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.qualification);
				query = query.Include(x => x.scheduledEventTemplate);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Scheduled Event Template Qualification Requirement, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       (includeRelations == true && x.qualification.name.Contains(anyStringContains))
			       || (includeRelations == true && x.qualification.description.Contains(anyStringContains))
			       || (includeRelations == true && x.qualification.color.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEventTemplate.name.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEventTemplate.description.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEventTemplate.defaultLocationPattern.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.ScheduledEventTemplateQualificationRequirement> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ScheduledEventTemplateQualificationRequirement scheduledEventTemplateQualificationRequirement in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(scheduledEventTemplateQualificationRequirement, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ScheduledEventTemplateQualificationRequirement Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ScheduledEventTemplateQualificationRequirement Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ScheduledEventTemplateQualificationRequirements filtered by the parameters provided.  Its query is similar to the GetScheduledEventTemplateQualificationRequirements method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplateQualificationRequirements/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? scheduledEventTemplateId = null,
			int? qualificationId = null,
			bool? isRequired = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
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


			IQueryable<Database.ScheduledEventTemplateQualificationRequirement> query = (from setqr in _context.ScheduledEventTemplateQualificationRequirements select setqr);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (scheduledEventTemplateId.HasValue == true)
			{
				query = query.Where(setqr => setqr.scheduledEventTemplateId == scheduledEventTemplateId.Value);
			}
			if (qualificationId.HasValue == true)
			{
				query = query.Where(setqr => setqr.qualificationId == qualificationId.Value);
			}
			if (isRequired.HasValue == true)
			{
				query = query.Where(setqr => setqr.isRequired == isRequired.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(setqr => setqr.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(setqr => setqr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(setqr => setqr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(setqr => setqr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(setqr => setqr.deleted == false);
				}
			}
			else
			{
				query = query.Where(setqr => setqr.active == true);
				query = query.Where(setqr => setqr.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Scheduled Event Template Qualification Requirement, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.qualification.name.Contains(anyStringContains)
			       || x.qualification.description.Contains(anyStringContains)
			       || x.qualification.color.Contains(anyStringContains)
			       || x.scheduledEventTemplate.name.Contains(anyStringContains)
			       || x.scheduledEventTemplate.description.Contains(anyStringContains)
			       || x.scheduledEventTemplate.defaultLocationPattern.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ScheduledEventTemplateQualificationRequirement by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplateQualificationRequirement/{id}")]
		public async Task<IActionResult> GetScheduledEventTemplateQualificationRequirement(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
			
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
				IQueryable<Database.ScheduledEventTemplateQualificationRequirement> query = (from setqr in _context.ScheduledEventTemplateQualificationRequirements where
							(setqr.id == id) &&
							(userIsAdmin == true || setqr.deleted == false) &&
							(userIsWriter == true || setqr.active == true)
					select setqr);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.qualification);
					query = query.Include(x => x.scheduledEventTemplate);
					query = query.AsSplitQuery();
				}

				Database.ScheduledEventTemplateQualificationRequirement materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.ScheduledEventTemplateQualificationRequirement Entity was read with Admin privilege." : "Scheduler.ScheduledEventTemplateQualificationRequirement Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ScheduledEventTemplateQualificationRequirement", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.ScheduledEventTemplateQualificationRequirement entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.ScheduledEventTemplateQualificationRequirement.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.ScheduledEventTemplateQualificationRequirement.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ScheduledEventTemplateQualificationRequirement record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ScheduledEventTemplateQualificationRequirement/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutScheduledEventTemplateQualificationRequirement(int id, [FromBody]Database.ScheduledEventTemplateQualificationRequirement.ScheduledEventTemplateQualificationRequirementDTO scheduledEventTemplateQualificationRequirementDTO, CancellationToken cancellationToken = default)
		{
			if (scheduledEventTemplateQualificationRequirementDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != scheduledEventTemplateQualificationRequirementDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
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


			IQueryable<Database.ScheduledEventTemplateQualificationRequirement> query = (from x in _context.ScheduledEventTemplateQualificationRequirements
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ScheduledEventTemplateQualificationRequirement existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ScheduledEventTemplateQualificationRequirement PUT", id.ToString(), new Exception("No Scheduler.ScheduledEventTemplateQualificationRequirement entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (scheduledEventTemplateQualificationRequirementDTO.objectGuid == Guid.Empty)
            {
                scheduledEventTemplateQualificationRequirementDTO.objectGuid = existing.objectGuid;
            }
            else if (scheduledEventTemplateQualificationRequirementDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ScheduledEventTemplateQualificationRequirement record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ScheduledEventTemplateQualificationRequirement cloneOfExisting = (Database.ScheduledEventTemplateQualificationRequirement)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ScheduledEventTemplateQualificationRequirement object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ScheduledEventTemplateQualificationRequirement scheduledEventTemplateQualificationRequirement = (Database.ScheduledEventTemplateQualificationRequirement)_context.Entry(existing).GetDatabaseValues().ToObject();
			scheduledEventTemplateQualificationRequirement.ApplyDTO(scheduledEventTemplateQualificationRequirementDTO);
			//
			// The tenant guid for any ScheduledEventTemplateQualificationRequirement being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ScheduledEventTemplateQualificationRequirement because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				scheduledEventTemplateQualificationRequirement.tenantGuid = existing.tenantGuid;
			}

			lock (scheduledEventTemplateQualificationRequirementPutSyncRoot)
			{
				//
				// Validate the version number for the scheduledEventTemplateQualificationRequirement being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != scheduledEventTemplateQualificationRequirement.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ScheduledEventTemplateQualificationRequirement save attempt was made but save request was with version " + scheduledEventTemplateQualificationRequirement.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ScheduledEventTemplateQualificationRequirement you are trying to update has already changed.  Please try your save again after reloading the ScheduledEventTemplateQualificationRequirement.");
				}
				else
				{
					// Same record.  Increase version.
					scheduledEventTemplateQualificationRequirement.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (scheduledEventTemplateQualificationRequirement.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.ScheduledEventTemplateQualificationRequirement record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				try
				{
				    EntityEntry<Database.ScheduledEventTemplateQualificationRequirement> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(scheduledEventTemplateQualificationRequirement);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ScheduledEventTemplateQualificationRequirementChangeHistory scheduledEventTemplateQualificationRequirementChangeHistory = new ScheduledEventTemplateQualificationRequirementChangeHistory();
				        scheduledEventTemplateQualificationRequirementChangeHistory.scheduledEventTemplateQualificationRequirementId = scheduledEventTemplateQualificationRequirement.id;
				        scheduledEventTemplateQualificationRequirementChangeHistory.versionNumber = scheduledEventTemplateQualificationRequirement.versionNumber;
				        scheduledEventTemplateQualificationRequirementChangeHistory.timeStamp = DateTime.UtcNow;
				        scheduledEventTemplateQualificationRequirementChangeHistory.userId = securityUser.id;
				        scheduledEventTemplateQualificationRequirementChangeHistory.tenantGuid = userTenantGuid;
				        scheduledEventTemplateQualificationRequirementChangeHistory.data = JsonSerializer.Serialize(Database.ScheduledEventTemplateQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplateQualificationRequirement));
				        _context.ScheduledEventTemplateQualificationRequirementChangeHistories.Add(scheduledEventTemplateQualificationRequirementChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ScheduledEventTemplateQualificationRequirement entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduledEventTemplateQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduledEventTemplateQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplateQualificationRequirement)),
						null);

				return Ok(Database.ScheduledEventTemplateQualificationRequirement.CreateAnonymous(scheduledEventTemplateQualificationRequirement));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ScheduledEventTemplateQualificationRequirement entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduledEventTemplateQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduledEventTemplateQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplateQualificationRequirement)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ScheduledEventTemplateQualificationRequirement record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplateQualificationRequirement", Name = "ScheduledEventTemplateQualificationRequirement")]
		public async Task<IActionResult> PostScheduledEventTemplateQualificationRequirement([FromBody]Database.ScheduledEventTemplateQualificationRequirement.ScheduledEventTemplateQualificationRequirementDTO scheduledEventTemplateQualificationRequirementDTO, CancellationToken cancellationToken = default)
		{
			if (scheduledEventTemplateQualificationRequirementDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
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
			// Create a new ScheduledEventTemplateQualificationRequirement object using the data from the DTO
			//
			Database.ScheduledEventTemplateQualificationRequirement scheduledEventTemplateQualificationRequirement = Database.ScheduledEventTemplateQualificationRequirement.FromDTO(scheduledEventTemplateQualificationRequirementDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				scheduledEventTemplateQualificationRequirement.tenantGuid = userTenantGuid;

				scheduledEventTemplateQualificationRequirement.objectGuid = Guid.NewGuid();
				scheduledEventTemplateQualificationRequirement.versionNumber = 1;

				_context.ScheduledEventTemplateQualificationRequirements.Add(scheduledEventTemplateQualificationRequirement);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the scheduledEventTemplateQualificationRequirement object so that no further changes will be written to the database
				    //
				    _context.Entry(scheduledEventTemplateQualificationRequirement).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					scheduledEventTemplateQualificationRequirement.ScheduledEventTemplateQualificationRequirementChangeHistories = null;
					scheduledEventTemplateQualificationRequirement.qualification = null;
					scheduledEventTemplateQualificationRequirement.scheduledEventTemplate = null;


				    ScheduledEventTemplateQualificationRequirementChangeHistory scheduledEventTemplateQualificationRequirementChangeHistory = new ScheduledEventTemplateQualificationRequirementChangeHistory();
				    scheduledEventTemplateQualificationRequirementChangeHistory.scheduledEventTemplateQualificationRequirementId = scheduledEventTemplateQualificationRequirement.id;
				    scheduledEventTemplateQualificationRequirementChangeHistory.versionNumber = scheduledEventTemplateQualificationRequirement.versionNumber;
				    scheduledEventTemplateQualificationRequirementChangeHistory.timeStamp = DateTime.UtcNow;
				    scheduledEventTemplateQualificationRequirementChangeHistory.userId = securityUser.id;
				    scheduledEventTemplateQualificationRequirementChangeHistory.tenantGuid = userTenantGuid;
				    scheduledEventTemplateQualificationRequirementChangeHistory.data = JsonSerializer.Serialize(Database.ScheduledEventTemplateQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplateQualificationRequirement));
				    _context.ScheduledEventTemplateQualificationRequirementChangeHistories.Add(scheduledEventTemplateQualificationRequirementChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.ScheduledEventTemplateQualificationRequirement entity successfully created.",
						true,
						scheduledEventTemplateQualificationRequirement. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ScheduledEventTemplateQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplateQualificationRequirement)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.ScheduledEventTemplateQualificationRequirement entity creation failed.", false, scheduledEventTemplateQualificationRequirement.id.ToString(), "", JsonSerializer.Serialize(Database.ScheduledEventTemplateQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplateQualificationRequirement)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ScheduledEventTemplateQualificationRequirement", scheduledEventTemplateQualificationRequirement.id, scheduledEventTemplateQualificationRequirement.id.ToString()));

			return CreatedAtRoute("ScheduledEventTemplateQualificationRequirement", new { id = scheduledEventTemplateQualificationRequirement.id }, Database.ScheduledEventTemplateQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplateQualificationRequirement));
		}



        /// <summary>
        /// 
        /// This rolls a ScheduledEventTemplateQualificationRequirement entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplateQualificationRequirement/Rollback/{id}")]
		[Route("api/ScheduledEventTemplateQualificationRequirement/Rollback")]
		public async Task<IActionResult> RollbackToScheduledEventTemplateQualificationRequirementVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);

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

			

			
			IQueryable <Database.ScheduledEventTemplateQualificationRequirement> query = (from x in _context.ScheduledEventTemplateQualificationRequirements
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this ScheduledEventTemplateQualificationRequirement concurrently
			//
			lock (scheduledEventTemplateQualificationRequirementPutSyncRoot)
			{
				
				Database.ScheduledEventTemplateQualificationRequirement scheduledEventTemplateQualificationRequirement = query.FirstOrDefault();
				
				if (scheduledEventTemplateQualificationRequirement == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ScheduledEventTemplateQualificationRequirement rollback", id.ToString(), new Exception("No Scheduler.ScheduledEventTemplateQualificationRequirement entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ScheduledEventTemplateQualificationRequirement current state so we can log it.
				//
				Database.ScheduledEventTemplateQualificationRequirement cloneOfExisting = (Database.ScheduledEventTemplateQualificationRequirement)_context.Entry(scheduledEventTemplateQualificationRequirement).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.ScheduledEventTemplateQualificationRequirementChangeHistories = null;
				cloneOfExisting.qualification = null;
				cloneOfExisting.scheduledEventTemplate = null;

				if (versionNumber >= scheduledEventTemplateQualificationRequirement.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.ScheduledEventTemplateQualificationRequirement rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.ScheduledEventTemplateQualificationRequirement rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ScheduledEventTemplateQualificationRequirementChangeHistory scheduledEventTemplateQualificationRequirementChangeHistory = (from x in _context.ScheduledEventTemplateQualificationRequirementChangeHistories
				                                               where
				                                               x.scheduledEventTemplateQualificationRequirementId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (scheduledEventTemplateQualificationRequirementChangeHistory != null)
				{
				    Database.ScheduledEventTemplateQualificationRequirement oldScheduledEventTemplateQualificationRequirement = JsonSerializer.Deserialize<Database.ScheduledEventTemplateQualificationRequirement>(scheduledEventTemplateQualificationRequirementChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    scheduledEventTemplateQualificationRequirement.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    scheduledEventTemplateQualificationRequirement.scheduledEventTemplateId = oldScheduledEventTemplateQualificationRequirement.scheduledEventTemplateId;
				    scheduledEventTemplateQualificationRequirement.qualificationId = oldScheduledEventTemplateQualificationRequirement.qualificationId;
				    scheduledEventTemplateQualificationRequirement.isRequired = oldScheduledEventTemplateQualificationRequirement.isRequired;
				    scheduledEventTemplateQualificationRequirement.objectGuid = oldScheduledEventTemplateQualificationRequirement.objectGuid;
				    scheduledEventTemplateQualificationRequirement.active = oldScheduledEventTemplateQualificationRequirement.active;
				    scheduledEventTemplateQualificationRequirement.deleted = oldScheduledEventTemplateQualificationRequirement.deleted;

				    string serializedScheduledEventTemplateQualificationRequirement = JsonSerializer.Serialize(scheduledEventTemplateQualificationRequirement);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ScheduledEventTemplateQualificationRequirementChangeHistory newScheduledEventTemplateQualificationRequirementChangeHistory = new ScheduledEventTemplateQualificationRequirementChangeHistory();
				        newScheduledEventTemplateQualificationRequirementChangeHistory.scheduledEventTemplateQualificationRequirementId = scheduledEventTemplateQualificationRequirement.id;
				        newScheduledEventTemplateQualificationRequirementChangeHistory.versionNumber = scheduledEventTemplateQualificationRequirement.versionNumber;
				        newScheduledEventTemplateQualificationRequirementChangeHistory.timeStamp = DateTime.UtcNow;
				        newScheduledEventTemplateQualificationRequirementChangeHistory.userId = securityUser.id;
				        newScheduledEventTemplateQualificationRequirementChangeHistory.tenantGuid = userTenantGuid;
				        newScheduledEventTemplateQualificationRequirementChangeHistory.data = JsonSerializer.Serialize(Database.ScheduledEventTemplateQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplateQualificationRequirement));
				        _context.ScheduledEventTemplateQualificationRequirementChangeHistories.Add(newScheduledEventTemplateQualificationRequirementChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ScheduledEventTemplateQualificationRequirement rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduledEventTemplateQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduledEventTemplateQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplateQualificationRequirement)),
						null);


				    return Ok(Database.ScheduledEventTemplateQualificationRequirement.CreateAnonymous(scheduledEventTemplateQualificationRequirement));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.ScheduledEventTemplateQualificationRequirement rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.ScheduledEventTemplateQualificationRequirement rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a ScheduledEventTemplateQualificationRequirement.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduledEventTemplateQualificationRequirement</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplateQualificationRequirement/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetScheduledEventTemplateQualificationRequirementChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{
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


			Database.ScheduledEventTemplateQualificationRequirement scheduledEventTemplateQualificationRequirement = await _context.ScheduledEventTemplateQualificationRequirements.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduledEventTemplateQualificationRequirement == null)
			{
				return NotFound();
			}

			try
			{
				scheduledEventTemplateQualificationRequirement.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ScheduledEventTemplateQualificationRequirement> versionInfo = await scheduledEventTemplateQualificationRequirement.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a ScheduledEventTemplateQualificationRequirement.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduledEventTemplateQualificationRequirement</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplateQualificationRequirement/{id}/AuditHistory")]
		public async Task<IActionResult> GetScheduledEventTemplateQualificationRequirementAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{
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


			Database.ScheduledEventTemplateQualificationRequirement scheduledEventTemplateQualificationRequirement = await _context.ScheduledEventTemplateQualificationRequirements.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduledEventTemplateQualificationRequirement == null)
			{
				return NotFound();
			}

			try
			{
				scheduledEventTemplateQualificationRequirement.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.ScheduledEventTemplateQualificationRequirement>> versions = await scheduledEventTemplateQualificationRequirement.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a ScheduledEventTemplateQualificationRequirement.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduledEventTemplateQualificationRequirement</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The ScheduledEventTemplateQualificationRequirement object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplateQualificationRequirement/{id}/Version/{version}")]
		public async Task<IActionResult> GetScheduledEventTemplateQualificationRequirementVersion(int id, int version, CancellationToken cancellationToken = default)
		{
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


			Database.ScheduledEventTemplateQualificationRequirement scheduledEventTemplateQualificationRequirement = await _context.ScheduledEventTemplateQualificationRequirements.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduledEventTemplateQualificationRequirement == null)
			{
				return NotFound();
			}

			try
			{
				scheduledEventTemplateQualificationRequirement.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ScheduledEventTemplateQualificationRequirement> versionInfo = await scheduledEventTemplateQualificationRequirement.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a ScheduledEventTemplateQualificationRequirement at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduledEventTemplateQualificationRequirement</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The ScheduledEventTemplateQualificationRequirement object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplateQualificationRequirement/{id}/StateAtTime")]
		public async Task<IActionResult> GetScheduledEventTemplateQualificationRequirementStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{
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


			Database.ScheduledEventTemplateQualificationRequirement scheduledEventTemplateQualificationRequirement = await _context.ScheduledEventTemplateQualificationRequirements.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduledEventTemplateQualificationRequirement == null)
			{
				return NotFound();
			}

			try
			{
				scheduledEventTemplateQualificationRequirement.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ScheduledEventTemplateQualificationRequirement> versionInfo = await scheduledEventTemplateQualificationRequirement.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a ScheduledEventTemplateQualificationRequirement record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplateQualificationRequirement/{id}")]
		[Route("api/ScheduledEventTemplateQualificationRequirement")]
		public async Task<IActionResult> DeleteScheduledEventTemplateQualificationRequirement(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(cancellationToken);
			
			
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

			IQueryable<Database.ScheduledEventTemplateQualificationRequirement> query = (from x in _context.ScheduledEventTemplateQualificationRequirements
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ScheduledEventTemplateQualificationRequirement scheduledEventTemplateQualificationRequirement = await query.FirstOrDefaultAsync(cancellationToken);

			if (scheduledEventTemplateQualificationRequirement == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ScheduledEventTemplateQualificationRequirement DELETE", id.ToString(), new Exception("No Scheduler.ScheduledEventTemplateQualificationRequirement entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ScheduledEventTemplateQualificationRequirement cloneOfExisting = (Database.ScheduledEventTemplateQualificationRequirement)_context.Entry(scheduledEventTemplateQualificationRequirement).GetDatabaseValues().ToObject();


			lock (scheduledEventTemplateQualificationRequirementDeleteSyncRoot)
			{
			    try
			    {
			        scheduledEventTemplateQualificationRequirement.deleted = true;
			        scheduledEventTemplateQualificationRequirement.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ScheduledEventTemplateQualificationRequirementChangeHistory scheduledEventTemplateQualificationRequirementChangeHistory = new ScheduledEventTemplateQualificationRequirementChangeHistory();
			        scheduledEventTemplateQualificationRequirementChangeHistory.scheduledEventTemplateQualificationRequirementId = scheduledEventTemplateQualificationRequirement.id;
			        scheduledEventTemplateQualificationRequirementChangeHistory.versionNumber = scheduledEventTemplateQualificationRequirement.versionNumber;
			        scheduledEventTemplateQualificationRequirementChangeHistory.timeStamp = DateTime.UtcNow;
			        scheduledEventTemplateQualificationRequirementChangeHistory.userId = securityUser.id;
			        scheduledEventTemplateQualificationRequirementChangeHistory.tenantGuid = userTenantGuid;
			        scheduledEventTemplateQualificationRequirementChangeHistory.data = JsonSerializer.Serialize(Database.ScheduledEventTemplateQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplateQualificationRequirement));
			        _context.ScheduledEventTemplateQualificationRequirementChangeHistories.Add(scheduledEventTemplateQualificationRequirementChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ScheduledEventTemplateQualificationRequirement entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduledEventTemplateQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduledEventTemplateQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplateQualificationRequirement)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ScheduledEventTemplateQualificationRequirement entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduledEventTemplateQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduledEventTemplateQualificationRequirement.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplateQualificationRequirement)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of ScheduledEventTemplateQualificationRequirement records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ScheduledEventTemplateQualificationRequirements/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? scheduledEventTemplateId = null,
			int? qualificationId = null,
			bool? isRequired = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);

			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);

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

			IQueryable<Database.ScheduledEventTemplateQualificationRequirement> query = (from setqr in _context.ScheduledEventTemplateQualificationRequirements select setqr);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (scheduledEventTemplateId.HasValue == true)
			{
				query = query.Where(setqr => setqr.scheduledEventTemplateId == scheduledEventTemplateId.Value);
			}
			if (qualificationId.HasValue == true)
			{
				query = query.Where(setqr => setqr.qualificationId == qualificationId.Value);
			}
			if (isRequired.HasValue == true)
			{
				query = query.Where(setqr => setqr.isRequired == isRequired.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(setqr => setqr.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(setqr => setqr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(setqr => setqr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(setqr => setqr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(setqr => setqr.deleted == false);
				}
			}
			else
			{
				query = query.Where(setqr => setqr.active == true);
				query = query.Where(setqr => setqr.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Scheduled Event Template Qualification Requirement, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.qualification.name.Contains(anyStringContains)
			       || x.qualification.description.Contains(anyStringContains)
			       || x.qualification.color.Contains(anyStringContains)
			       || x.scheduledEventTemplate.name.Contains(anyStringContains)
			       || x.scheduledEventTemplate.description.Contains(anyStringContains)
			       || x.scheduledEventTemplate.defaultLocationPattern.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ScheduledEventTemplateQualificationRequirement.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ScheduledEventTemplateQualificationRequirement/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
			{
			   return Forbid();
			}

		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
