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
    /// This auto generated class provides the basic CRUD operations for the ResourceQualification entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ResourceQualification entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ResourceQualificationsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object resourceQualificationPutSyncRoot = new object();
		static object resourceQualificationDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<ResourceQualificationsController> _logger;

		public ResourceQualificationsController(SchedulerContext context, ILogger<ResourceQualificationsController> logger) : base("Scheduler", "ResourceQualification")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ResourceQualifications filtered by the parameters provided.
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
		[Route("api/ResourceQualifications")]
		public async Task<IActionResult> GetResourceQualifications(
			int? resourceId = null,
			int? qualificationId = null,
			DateTime? issueDate = null,
			DateTime? expiryDate = null,
			string issuer = null,
			string notes = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (issueDate.HasValue == true && issueDate.Value.Kind != DateTimeKind.Utc)
			{
				issueDate = issueDate.Value.ToUniversalTime();
			}

			if (expiryDate.HasValue == true && expiryDate.Value.Kind != DateTimeKind.Utc)
			{
				expiryDate = expiryDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ResourceQualification> query = (from rq in _context.ResourceQualifications select rq);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (resourceId.HasValue == true)
			{
				query = query.Where(rq => rq.resourceId == resourceId.Value);
			}
			if (qualificationId.HasValue == true)
			{
				query = query.Where(rq => rq.qualificationId == qualificationId.Value);
			}
			if (issueDate.HasValue == true)
			{
				query = query.Where(rq => rq.issueDate == issueDate.Value);
			}
			if (expiryDate.HasValue == true)
			{
				query = query.Where(rq => rq.expiryDate == expiryDate.Value);
			}
			if (string.IsNullOrEmpty(issuer) == false)
			{
				query = query.Where(rq => rq.issuer == issuer);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(rq => rq.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(rq => rq.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rq => rq.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rq => rq.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rq => rq.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rq => rq.deleted == false);
				}
			}
			else
			{
				query = query.Where(rq => rq.active == true);
				query = query.Where(rq => rq.deleted == false);
			}

			query = query.OrderBy(rq => rq.issuer);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.qualification);
				query = query.Include(x => x.resource);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Resource Qualification, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.issuer.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || (includeRelations == true && x.qualification.name.Contains(anyStringContains))
			       || (includeRelations == true && x.qualification.description.Contains(anyStringContains))
			       || (includeRelations == true && x.qualification.color.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.name.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.description.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.color.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.avatarMimeType.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.ResourceQualification> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ResourceQualification resourceQualification in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(resourceQualification, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ResourceQualification Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ResourceQualification Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ResourceQualifications filtered by the parameters provided.  Its query is similar to the GetResourceQualifications method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceQualifications/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? resourceId = null,
			int? qualificationId = null,
			DateTime? issueDate = null,
			DateTime? expiryDate = null,
			string issuer = null,
			string notes = null,
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


			//
			// Fix any non-UTC date parameters that come in.
			//
			if (issueDate.HasValue == true && issueDate.Value.Kind != DateTimeKind.Utc)
			{
				issueDate = issueDate.Value.ToUniversalTime();
			}

			if (expiryDate.HasValue == true && expiryDate.Value.Kind != DateTimeKind.Utc)
			{
				expiryDate = expiryDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ResourceQualification> query = (from rq in _context.ResourceQualifications select rq);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (resourceId.HasValue == true)
			{
				query = query.Where(rq => rq.resourceId == resourceId.Value);
			}
			if (qualificationId.HasValue == true)
			{
				query = query.Where(rq => rq.qualificationId == qualificationId.Value);
			}
			if (issueDate.HasValue == true)
			{
				query = query.Where(rq => rq.issueDate == issueDate.Value);
			}
			if (expiryDate.HasValue == true)
			{
				query = query.Where(rq => rq.expiryDate == expiryDate.Value);
			}
			if (issuer != null)
			{
				query = query.Where(rq => rq.issuer == issuer);
			}
			if (notes != null)
			{
				query = query.Where(rq => rq.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(rq => rq.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rq => rq.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rq => rq.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rq => rq.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rq => rq.deleted == false);
				}
			}
			else
			{
				query = query.Where(rq => rq.active == true);
				query = query.Where(rq => rq.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Resource Qualification, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.issuer.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.qualification.name.Contains(anyStringContains)
			       || x.qualification.description.Contains(anyStringContains)
			       || x.qualification.color.Contains(anyStringContains)
			       || x.resource.name.Contains(anyStringContains)
			       || x.resource.description.Contains(anyStringContains)
			       || x.resource.notes.Contains(anyStringContains)
			       || x.resource.externalId.Contains(anyStringContains)
			       || x.resource.color.Contains(anyStringContains)
			       || x.resource.attributes.Contains(anyStringContains)
			       || x.resource.avatarFileName.Contains(anyStringContains)
			       || x.resource.avatarMimeType.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ResourceQualification by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceQualification/{id}")]
		public async Task<IActionResult> GetResourceQualification(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ResourceQualification> query = (from rq in _context.ResourceQualifications where
							(rq.id == id) &&
							(userIsAdmin == true || rq.deleted == false) &&
							(userIsWriter == true || rq.active == true)
					select rq);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.qualification);
					query = query.Include(x => x.resource);
					query = query.AsSplitQuery();
				}

				Database.ResourceQualification materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.ResourceQualification Entity was read with Admin privilege." : "Scheduler.ResourceQualification Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ResourceQualification", materialized.id, materialized.issuer));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.ResourceQualification entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.ResourceQualification.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.ResourceQualification.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ResourceQualification record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ResourceQualification/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutResourceQualification(int id, [FromBody]Database.ResourceQualification.ResourceQualificationDTO resourceQualificationDTO, CancellationToken cancellationToken = default)
		{
			if (resourceQualificationDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != resourceQualificationDTO.id)
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


			IQueryable<Database.ResourceQualification> query = (from x in _context.ResourceQualifications
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ResourceQualification existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ResourceQualification PUT", id.ToString(), new Exception("No Scheduler.ResourceQualification entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (resourceQualificationDTO.objectGuid == Guid.Empty)
            {
                resourceQualificationDTO.objectGuid = existing.objectGuid;
            }
            else if (resourceQualificationDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ResourceQualification record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ResourceQualification cloneOfExisting = (Database.ResourceQualification)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ResourceQualification object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ResourceQualification resourceQualification = (Database.ResourceQualification)_context.Entry(existing).GetDatabaseValues().ToObject();
			resourceQualification.ApplyDTO(resourceQualificationDTO);
			//
			// The tenant guid for any ResourceQualification being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ResourceQualification because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				resourceQualification.tenantGuid = existing.tenantGuid;
			}

			lock (resourceQualificationPutSyncRoot)
			{
				//
				// Validate the version number for the resourceQualification being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != resourceQualification.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ResourceQualification save attempt was made but save request was with version " + resourceQualification.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ResourceQualification you are trying to update has already changed.  Please try your save again after reloading the ResourceQualification.");
				}
				else
				{
					// Same record.  Increase version.
					resourceQualification.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (resourceQualification.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.ResourceQualification record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (resourceQualification.issueDate.HasValue == true && resourceQualification.issueDate.Value.Kind != DateTimeKind.Utc)
				{
					resourceQualification.issueDate = resourceQualification.issueDate.Value.ToUniversalTime();
				}

				if (resourceQualification.expiryDate.HasValue == true && resourceQualification.expiryDate.Value.Kind != DateTimeKind.Utc)
				{
					resourceQualification.expiryDate = resourceQualification.expiryDate.Value.ToUniversalTime();
				}

				if (resourceQualification.issuer != null && resourceQualification.issuer.Length > 250)
				{
					resourceQualification.issuer = resourceQualification.issuer.Substring(0, 250);
				}

				try
				{
				    EntityEntry<Database.ResourceQualification> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(resourceQualification);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ResourceQualificationChangeHistory resourceQualificationChangeHistory = new ResourceQualificationChangeHistory();
				        resourceQualificationChangeHistory.resourceQualificationId = resourceQualification.id;
				        resourceQualificationChangeHistory.versionNumber = resourceQualification.versionNumber;
				        resourceQualificationChangeHistory.timeStamp = DateTime.UtcNow;
				        resourceQualificationChangeHistory.userId = securityUser.id;
				        resourceQualificationChangeHistory.tenantGuid = userTenantGuid;
				        resourceQualificationChangeHistory.data = JsonSerializer.Serialize(Database.ResourceQualification.CreateAnonymousWithFirstLevelSubObjects(resourceQualification));
				        _context.ResourceQualificationChangeHistories.Add(resourceQualificationChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ResourceQualification entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ResourceQualification.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ResourceQualification.CreateAnonymousWithFirstLevelSubObjects(resourceQualification)),
						null);

				return Ok(Database.ResourceQualification.CreateAnonymous(resourceQualification));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ResourceQualification entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ResourceQualification.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ResourceQualification.CreateAnonymousWithFirstLevelSubObjects(resourceQualification)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ResourceQualification record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceQualification", Name = "ResourceQualification")]
		public async Task<IActionResult> PostResourceQualification([FromBody]Database.ResourceQualification.ResourceQualificationDTO resourceQualificationDTO, CancellationToken cancellationToken = default)
		{
			if (resourceQualificationDTO == null)
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
			// Create a new ResourceQualification object using the data from the DTO
			//
			Database.ResourceQualification resourceQualification = Database.ResourceQualification.FromDTO(resourceQualificationDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				resourceQualification.tenantGuid = userTenantGuid;

				if (resourceQualification.issueDate.HasValue == true && resourceQualification.issueDate.Value.Kind != DateTimeKind.Utc)
				{
					resourceQualification.issueDate = resourceQualification.issueDate.Value.ToUniversalTime();
				}

				if (resourceQualification.expiryDate.HasValue == true && resourceQualification.expiryDate.Value.Kind != DateTimeKind.Utc)
				{
					resourceQualification.expiryDate = resourceQualification.expiryDate.Value.ToUniversalTime();
				}

				if (resourceQualification.issuer != null && resourceQualification.issuer.Length > 250)
				{
					resourceQualification.issuer = resourceQualification.issuer.Substring(0, 250);
				}

				resourceQualification.objectGuid = Guid.NewGuid();
				resourceQualification.versionNumber = 1;

				_context.ResourceQualifications.Add(resourceQualification);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the resourceQualification object so that no further changes will be written to the database
				    //
				    _context.Entry(resourceQualification).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					resourceQualification.ResourceQualificationChangeHistories = null;
					resourceQualification.qualification = null;
					resourceQualification.resource = null;


				    ResourceQualificationChangeHistory resourceQualificationChangeHistory = new ResourceQualificationChangeHistory();
				    resourceQualificationChangeHistory.resourceQualificationId = resourceQualification.id;
				    resourceQualificationChangeHistory.versionNumber = resourceQualification.versionNumber;
				    resourceQualificationChangeHistory.timeStamp = DateTime.UtcNow;
				    resourceQualificationChangeHistory.userId = securityUser.id;
				    resourceQualificationChangeHistory.tenantGuid = userTenantGuid;
				    resourceQualificationChangeHistory.data = JsonSerializer.Serialize(Database.ResourceQualification.CreateAnonymousWithFirstLevelSubObjects(resourceQualification));
				    _context.ResourceQualificationChangeHistories.Add(resourceQualificationChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.ResourceQualification entity successfully created.",
						true,
						resourceQualification. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ResourceQualification.CreateAnonymousWithFirstLevelSubObjects(resourceQualification)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.ResourceQualification entity creation failed.", false, resourceQualification.id.ToString(), "", JsonSerializer.Serialize(Database.ResourceQualification.CreateAnonymousWithFirstLevelSubObjects(resourceQualification)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ResourceQualification", resourceQualification.id, resourceQualification.issuer));

			return CreatedAtRoute("ResourceQualification", new { id = resourceQualification.id }, Database.ResourceQualification.CreateAnonymousWithFirstLevelSubObjects(resourceQualification));
		}



        /// <summary>
        /// 
        /// This rolls a ResourceQualification entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceQualification/Rollback/{id}")]
		[Route("api/ResourceQualification/Rollback")]
		public async Task<IActionResult> RollbackToResourceQualificationVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.ResourceQualification> query = (from x in _context.ResourceQualifications
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this ResourceQualification concurrently
			//
			lock (resourceQualificationPutSyncRoot)
			{
				
				Database.ResourceQualification resourceQualification = query.FirstOrDefault();
				
				if (resourceQualification == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ResourceQualification rollback", id.ToString(), new Exception("No Scheduler.ResourceQualification entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ResourceQualification current state so we can log it.
				//
				Database.ResourceQualification cloneOfExisting = (Database.ResourceQualification)_context.Entry(resourceQualification).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.ResourceQualificationChangeHistories = null;
				cloneOfExisting.qualification = null;
				cloneOfExisting.resource = null;

				if (versionNumber >= resourceQualification.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.ResourceQualification rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.ResourceQualification rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ResourceQualificationChangeHistory resourceQualificationChangeHistory = (from x in _context.ResourceQualificationChangeHistories
				                                               where
				                                               x.resourceQualificationId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (resourceQualificationChangeHistory != null)
				{
				    Database.ResourceQualification oldResourceQualification = JsonSerializer.Deserialize<Database.ResourceQualification>(resourceQualificationChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    resourceQualification.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    resourceQualification.resourceId = oldResourceQualification.resourceId;
				    resourceQualification.qualificationId = oldResourceQualification.qualificationId;
				    resourceQualification.issueDate = oldResourceQualification.issueDate;
				    resourceQualification.expiryDate = oldResourceQualification.expiryDate;
				    resourceQualification.issuer = oldResourceQualification.issuer;
				    resourceQualification.notes = oldResourceQualification.notes;
				    resourceQualification.objectGuid = oldResourceQualification.objectGuid;
				    resourceQualification.active = oldResourceQualification.active;
				    resourceQualification.deleted = oldResourceQualification.deleted;

				    string serializedResourceQualification = JsonSerializer.Serialize(resourceQualification);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ResourceQualificationChangeHistory newResourceQualificationChangeHistory = new ResourceQualificationChangeHistory();
				        newResourceQualificationChangeHistory.resourceQualificationId = resourceQualification.id;
				        newResourceQualificationChangeHistory.versionNumber = resourceQualification.versionNumber;
				        newResourceQualificationChangeHistory.timeStamp = DateTime.UtcNow;
				        newResourceQualificationChangeHistory.userId = securityUser.id;
				        newResourceQualificationChangeHistory.tenantGuid = userTenantGuid;
				        newResourceQualificationChangeHistory.data = JsonSerializer.Serialize(Database.ResourceQualification.CreateAnonymousWithFirstLevelSubObjects(resourceQualification));
				        _context.ResourceQualificationChangeHistories.Add(newResourceQualificationChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ResourceQualification rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ResourceQualification.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ResourceQualification.CreateAnonymousWithFirstLevelSubObjects(resourceQualification)),
						null);


				    return Ok(Database.ResourceQualification.CreateAnonymous(resourceQualification));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.ResourceQualification rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.ResourceQualification rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}


        /// <summary>
        /// 
        /// This deletes a ResourceQualification record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceQualification/{id}")]
		[Route("api/ResourceQualification")]
		public async Task<IActionResult> DeleteResourceQualification(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ResourceQualification> query = (from x in _context.ResourceQualifications
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ResourceQualification resourceQualification = await query.FirstOrDefaultAsync(cancellationToken);

			if (resourceQualification == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ResourceQualification DELETE", id.ToString(), new Exception("No Scheduler.ResourceQualification entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ResourceQualification cloneOfExisting = (Database.ResourceQualification)_context.Entry(resourceQualification).GetDatabaseValues().ToObject();


			lock (resourceQualificationDeleteSyncRoot)
			{
			    try
			    {
			        resourceQualification.deleted = true;
			        resourceQualification.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ResourceQualificationChangeHistory resourceQualificationChangeHistory = new ResourceQualificationChangeHistory();
			        resourceQualificationChangeHistory.resourceQualificationId = resourceQualification.id;
			        resourceQualificationChangeHistory.versionNumber = resourceQualification.versionNumber;
			        resourceQualificationChangeHistory.timeStamp = DateTime.UtcNow;
			        resourceQualificationChangeHistory.userId = securityUser.id;
			        resourceQualificationChangeHistory.tenantGuid = userTenantGuid;
			        resourceQualificationChangeHistory.data = JsonSerializer.Serialize(Database.ResourceQualification.CreateAnonymousWithFirstLevelSubObjects(resourceQualification));
			        _context.ResourceQualificationChangeHistories.Add(resourceQualificationChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ResourceQualification entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ResourceQualification.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ResourceQualification.CreateAnonymousWithFirstLevelSubObjects(resourceQualification)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ResourceQualification entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ResourceQualification.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ResourceQualification.CreateAnonymousWithFirstLevelSubObjects(resourceQualification)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of ResourceQualification records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ResourceQualifications/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? resourceId = null,
			int? qualificationId = null,
			DateTime? issueDate = null,
			DateTime? expiryDate = null,
			string issuer = null,
			string notes = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (issueDate.HasValue == true && issueDate.Value.Kind != DateTimeKind.Utc)
			{
				issueDate = issueDate.Value.ToUniversalTime();
			}

			if (expiryDate.HasValue == true && expiryDate.Value.Kind != DateTimeKind.Utc)
			{
				expiryDate = expiryDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ResourceQualification> query = (from rq in _context.ResourceQualifications select rq);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (resourceId.HasValue == true)
			{
				query = query.Where(rq => rq.resourceId == resourceId.Value);
			}
			if (qualificationId.HasValue == true)
			{
				query = query.Where(rq => rq.qualificationId == qualificationId.Value);
			}
			if (issueDate.HasValue == true)
			{
				query = query.Where(rq => rq.issueDate == issueDate.Value);
			}
			if (expiryDate.HasValue == true)
			{
				query = query.Where(rq => rq.expiryDate == expiryDate.Value);
			}
			if (string.IsNullOrEmpty(issuer) == false)
			{
				query = query.Where(rq => rq.issuer == issuer);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(rq => rq.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(rq => rq.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rq => rq.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rq => rq.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rq => rq.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rq => rq.deleted == false);
				}
			}
			else
			{
				query = query.Where(rq => rq.active == true);
				query = query.Where(rq => rq.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Resource Qualification, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.issuer.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.qualification.name.Contains(anyStringContains)
			       || x.qualification.description.Contains(anyStringContains)
			       || x.qualification.color.Contains(anyStringContains)
			       || x.resource.name.Contains(anyStringContains)
			       || x.resource.description.Contains(anyStringContains)
			       || x.resource.notes.Contains(anyStringContains)
			       || x.resource.externalId.Contains(anyStringContains)
			       || x.resource.color.Contains(anyStringContains)
			       || x.resource.attributes.Contains(anyStringContains)
			       || x.resource.avatarFileName.Contains(anyStringContains)
			       || x.resource.avatarMimeType.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.issuer);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ResourceQualification.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ResourceQualification/CreateAuditEvent")]
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
