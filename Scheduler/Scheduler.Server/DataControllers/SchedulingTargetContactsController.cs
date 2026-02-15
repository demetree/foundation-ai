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
    /// This auto generated class provides the basic CRUD operations for the SchedulingTargetContact entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SchedulingTargetContact entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SchedulingTargetContactsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object schedulingTargetContactPutSyncRoot = new object();
		static object schedulingTargetContactDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<SchedulingTargetContactsController> _logger;

		public SchedulingTargetContactsController(SchedulerContext context, ILogger<SchedulingTargetContactsController> logger) : base("Scheduler", "SchedulingTargetContact")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of SchedulingTargetContacts filtered by the parameters provided.
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
		[Route("api/SchedulingTargetContacts")]
		public async Task<IActionResult> GetSchedulingTargetContacts(
			int? schedulingTargetId = null,
			int? contactId = null,
			bool? isPrimary = null,
			int? relationshipTypeId = null,
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
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			IQueryable<Database.SchedulingTargetContact> query = (from stc in _context.SchedulingTargetContacts select stc);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (schedulingTargetId.HasValue == true)
			{
				query = query.Where(stc => stc.schedulingTargetId == schedulingTargetId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(stc => stc.contactId == contactId.Value);
			}
			if (isPrimary.HasValue == true)
			{
				query = query.Where(stc => stc.isPrimary == isPrimary.Value);
			}
			if (relationshipTypeId.HasValue == true)
			{
				query = query.Where(stc => stc.relationshipTypeId == relationshipTypeId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(stc => stc.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(stc => stc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(stc => stc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(stc => stc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(stc => stc.deleted == false);
				}
			}
			else
			{
				query = query.Where(stc => stc.active == true);
				query = query.Where(stc => stc.deleted == false);
			}

			query = query.OrderBy(stc => stc.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.contact);
				query = query.Include(x => x.relationshipType);
				query = query.Include(x => x.schedulingTarget);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Scheduling Target Contact, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       (includeRelations == true && x.contact.firstName.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.middleName.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.lastName.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.title.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.company.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.email.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.phone.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.mobile.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.position.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.webSite.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.color.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.avatarMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.relationshipType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.relationshipType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.relationshipType.color.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.name.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.description.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.color.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.avatarMimeType.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.SchedulingTargetContact> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SchedulingTargetContact schedulingTargetContact in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(schedulingTargetContact, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.SchedulingTargetContact Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.SchedulingTargetContact Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of SchedulingTargetContacts filtered by the parameters provided.  Its query is similar to the GetSchedulingTargetContacts method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTargetContacts/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? schedulingTargetId = null,
			int? contactId = null,
			bool? isPrimary = null,
			int? relationshipTypeId = null,
			int? versionNumber = null,
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


			IQueryable<Database.SchedulingTargetContact> query = (from stc in _context.SchedulingTargetContacts select stc);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (schedulingTargetId.HasValue == true)
			{
				query = query.Where(stc => stc.schedulingTargetId == schedulingTargetId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(stc => stc.contactId == contactId.Value);
			}
			if (isPrimary.HasValue == true)
			{
				query = query.Where(stc => stc.isPrimary == isPrimary.Value);
			}
			if (relationshipTypeId.HasValue == true)
			{
				query = query.Where(stc => stc.relationshipTypeId == relationshipTypeId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(stc => stc.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(stc => stc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(stc => stc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(stc => stc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(stc => stc.deleted == false);
				}
			}
			else
			{
				query = query.Where(stc => stc.active == true);
				query = query.Where(stc => stc.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Scheduling Target Contact, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.contact.firstName.Contains(anyStringContains)
			       || x.contact.middleName.Contains(anyStringContains)
			       || x.contact.lastName.Contains(anyStringContains)
			       || x.contact.title.Contains(anyStringContains)
			       || x.contact.company.Contains(anyStringContains)
			       || x.contact.email.Contains(anyStringContains)
			       || x.contact.phone.Contains(anyStringContains)
			       || x.contact.mobile.Contains(anyStringContains)
			       || x.contact.position.Contains(anyStringContains)
			       || x.contact.webSite.Contains(anyStringContains)
			       || x.contact.notes.Contains(anyStringContains)
			       || x.contact.attributes.Contains(anyStringContains)
			       || x.contact.color.Contains(anyStringContains)
			       || x.contact.avatarFileName.Contains(anyStringContains)
			       || x.contact.avatarMimeType.Contains(anyStringContains)
			       || x.contact.externalId.Contains(anyStringContains)
			       || x.relationshipType.name.Contains(anyStringContains)
			       || x.relationshipType.description.Contains(anyStringContains)
			       || x.relationshipType.color.Contains(anyStringContains)
			       || x.schedulingTarget.name.Contains(anyStringContains)
			       || x.schedulingTarget.description.Contains(anyStringContains)
			       || x.schedulingTarget.notes.Contains(anyStringContains)
			       || x.schedulingTarget.externalId.Contains(anyStringContains)
			       || x.schedulingTarget.color.Contains(anyStringContains)
			       || x.schedulingTarget.attributes.Contains(anyStringContains)
			       || x.schedulingTarget.avatarFileName.Contains(anyStringContains)
			       || x.schedulingTarget.avatarMimeType.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single SchedulingTargetContact by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTargetContact/{id}")]
		public async Task<IActionResult> GetSchedulingTargetContact(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.SchedulingTargetContact> query = (from stc in _context.SchedulingTargetContacts where
							(stc.id == id) &&
							(userIsAdmin == true || stc.deleted == false) &&
							(userIsWriter == true || stc.active == true)
					select stc);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.contact);
					query = query.Include(x => x.relationshipType);
					query = query.Include(x => x.schedulingTarget);
					query = query.AsSplitQuery();
				}

				Database.SchedulingTargetContact materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.SchedulingTargetContact Entity was read with Admin privilege." : "Scheduler.SchedulingTargetContact Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SchedulingTargetContact", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.SchedulingTargetContact entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.SchedulingTargetContact.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.SchedulingTargetContact.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing SchedulingTargetContact record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SchedulingTargetContact/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSchedulingTargetContact(int id, [FromBody]Database.SchedulingTargetContact.SchedulingTargetContactDTO schedulingTargetContactDTO, CancellationToken cancellationToken = default)
		{
			if (schedulingTargetContactDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != schedulingTargetContactDTO.id)
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


			IQueryable<Database.SchedulingTargetContact> query = (from x in _context.SchedulingTargetContacts
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.SchedulingTargetContact existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.SchedulingTargetContact PUT", id.ToString(), new Exception("No Scheduler.SchedulingTargetContact entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (schedulingTargetContactDTO.objectGuid == Guid.Empty)
            {
                schedulingTargetContactDTO.objectGuid = existing.objectGuid;
            }
            else if (schedulingTargetContactDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a SchedulingTargetContact record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SchedulingTargetContact cloneOfExisting = (Database.SchedulingTargetContact)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SchedulingTargetContact object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SchedulingTargetContact schedulingTargetContact = (Database.SchedulingTargetContact)_context.Entry(existing).GetDatabaseValues().ToObject();
			schedulingTargetContact.ApplyDTO(schedulingTargetContactDTO);
			//
			// The tenant guid for any SchedulingTargetContact being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the SchedulingTargetContact because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				schedulingTargetContact.tenantGuid = existing.tenantGuid;
			}

			lock (schedulingTargetContactPutSyncRoot)
			{
				//
				// Validate the version number for the schedulingTargetContact being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != schedulingTargetContact.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "SchedulingTargetContact save attempt was made but save request was with version " + schedulingTargetContact.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The SchedulingTargetContact you are trying to update has already changed.  Please try your save again after reloading the SchedulingTargetContact.");
				}
				else
				{
					// Same record.  Increase version.
					schedulingTargetContact.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (schedulingTargetContact.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.SchedulingTargetContact record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				try
				{
				    EntityEntry<Database.SchedulingTargetContact> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(schedulingTargetContact);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        SchedulingTargetContactChangeHistory schedulingTargetContactChangeHistory = new SchedulingTargetContactChangeHistory();
				        schedulingTargetContactChangeHistory.schedulingTargetContactId = schedulingTargetContact.id;
				        schedulingTargetContactChangeHistory.versionNumber = schedulingTargetContact.versionNumber;
				        schedulingTargetContactChangeHistory.timeStamp = DateTime.UtcNow;
				        schedulingTargetContactChangeHistory.userId = securityUser.id;
				        schedulingTargetContactChangeHistory.tenantGuid = userTenantGuid;
				        schedulingTargetContactChangeHistory.data = JsonSerializer.Serialize(Database.SchedulingTargetContact.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetContact));
				        _context.SchedulingTargetContactChangeHistories.Add(schedulingTargetContactChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.SchedulingTargetContact entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.SchedulingTargetContact.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.SchedulingTargetContact.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetContact)),
						null);

				return Ok(Database.SchedulingTargetContact.CreateAnonymous(schedulingTargetContact));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.SchedulingTargetContact entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.SchedulingTargetContact.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.SchedulingTargetContact.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetContact)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new SchedulingTargetContact record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTargetContact", Name = "SchedulingTargetContact")]
		public async Task<IActionResult> PostSchedulingTargetContact([FromBody]Database.SchedulingTargetContact.SchedulingTargetContactDTO schedulingTargetContactDTO, CancellationToken cancellationToken = default)
		{
			if (schedulingTargetContactDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
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
			// Create a new SchedulingTargetContact object using the data from the DTO
			//
			Database.SchedulingTargetContact schedulingTargetContact = Database.SchedulingTargetContact.FromDTO(schedulingTargetContactDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				schedulingTargetContact.tenantGuid = userTenantGuid;

				schedulingTargetContact.objectGuid = Guid.NewGuid();
				schedulingTargetContact.versionNumber = 1;

				_context.SchedulingTargetContacts.Add(schedulingTargetContact);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the schedulingTargetContact object so that no further changes will be written to the database
				    //
				    _context.Entry(schedulingTargetContact).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					schedulingTargetContact.SchedulingTargetContactChangeHistories = null;
					schedulingTargetContact.contact = null;
					schedulingTargetContact.relationshipType = null;
					schedulingTargetContact.schedulingTarget = null;


				    SchedulingTargetContactChangeHistory schedulingTargetContactChangeHistory = new SchedulingTargetContactChangeHistory();
				    schedulingTargetContactChangeHistory.schedulingTargetContactId = schedulingTargetContact.id;
				    schedulingTargetContactChangeHistory.versionNumber = schedulingTargetContact.versionNumber;
				    schedulingTargetContactChangeHistory.timeStamp = DateTime.UtcNow;
				    schedulingTargetContactChangeHistory.userId = securityUser.id;
				    schedulingTargetContactChangeHistory.tenantGuid = userTenantGuid;
				    schedulingTargetContactChangeHistory.data = JsonSerializer.Serialize(Database.SchedulingTargetContact.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetContact));
				    _context.SchedulingTargetContactChangeHistories.Add(schedulingTargetContactChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.SchedulingTargetContact entity successfully created.",
						true,
						schedulingTargetContact. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.SchedulingTargetContact.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetContact)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.SchedulingTargetContact entity creation failed.", false, schedulingTargetContact.id.ToString(), "", JsonSerializer.Serialize(Database.SchedulingTargetContact.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetContact)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SchedulingTargetContact", schedulingTargetContact.id, schedulingTargetContact.id.ToString()));

			return CreatedAtRoute("SchedulingTargetContact", new { id = schedulingTargetContact.id }, Database.SchedulingTargetContact.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetContact));
		}



        /// <summary>
        /// 
        /// This rolls a SchedulingTargetContact entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTargetContact/Rollback/{id}")]
		[Route("api/SchedulingTargetContact/Rollback")]
		public async Task<IActionResult> RollbackToSchedulingTargetContactVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.SchedulingTargetContact> query = (from x in _context.SchedulingTargetContacts
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this SchedulingTargetContact concurrently
			//
			lock (schedulingTargetContactPutSyncRoot)
			{
				
				Database.SchedulingTargetContact schedulingTargetContact = query.FirstOrDefault();
				
				if (schedulingTargetContact == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.SchedulingTargetContact rollback", id.ToString(), new Exception("No Scheduler.SchedulingTargetContact entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the SchedulingTargetContact current state so we can log it.
				//
				Database.SchedulingTargetContact cloneOfExisting = (Database.SchedulingTargetContact)_context.Entry(schedulingTargetContact).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.SchedulingTargetContactChangeHistories = null;
				cloneOfExisting.contact = null;
				cloneOfExisting.relationshipType = null;
				cloneOfExisting.schedulingTarget = null;

				if (versionNumber >= schedulingTargetContact.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.SchedulingTargetContact rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.SchedulingTargetContact rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				SchedulingTargetContactChangeHistory schedulingTargetContactChangeHistory = (from x in _context.SchedulingTargetContactChangeHistories
				                                               where
				                                               x.schedulingTargetContactId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (schedulingTargetContactChangeHistory != null)
				{
				    Database.SchedulingTargetContact oldSchedulingTargetContact = JsonSerializer.Deserialize<Database.SchedulingTargetContact>(schedulingTargetContactChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    schedulingTargetContact.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    schedulingTargetContact.schedulingTargetId = oldSchedulingTargetContact.schedulingTargetId;
				    schedulingTargetContact.contactId = oldSchedulingTargetContact.contactId;
				    schedulingTargetContact.isPrimary = oldSchedulingTargetContact.isPrimary;
				    schedulingTargetContact.relationshipTypeId = oldSchedulingTargetContact.relationshipTypeId;
				    schedulingTargetContact.objectGuid = oldSchedulingTargetContact.objectGuid;
				    schedulingTargetContact.active = oldSchedulingTargetContact.active;
				    schedulingTargetContact.deleted = oldSchedulingTargetContact.deleted;

				    string serializedSchedulingTargetContact = JsonSerializer.Serialize(schedulingTargetContact);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        SchedulingTargetContactChangeHistory newSchedulingTargetContactChangeHistory = new SchedulingTargetContactChangeHistory();
				        newSchedulingTargetContactChangeHistory.schedulingTargetContactId = schedulingTargetContact.id;
				        newSchedulingTargetContactChangeHistory.versionNumber = schedulingTargetContact.versionNumber;
				        newSchedulingTargetContactChangeHistory.timeStamp = DateTime.UtcNow;
				        newSchedulingTargetContactChangeHistory.userId = securityUser.id;
				        newSchedulingTargetContactChangeHistory.tenantGuid = userTenantGuid;
				        newSchedulingTargetContactChangeHistory.data = JsonSerializer.Serialize(Database.SchedulingTargetContact.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetContact));
				        _context.SchedulingTargetContactChangeHistories.Add(newSchedulingTargetContactChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.SchedulingTargetContact rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.SchedulingTargetContact.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.SchedulingTargetContact.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetContact)),
						null);


				    return Ok(Database.SchedulingTargetContact.CreateAnonymous(schedulingTargetContact));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.SchedulingTargetContact rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.SchedulingTargetContact rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a SchedulingTargetContact.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the SchedulingTargetContact</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTargetContact/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetSchedulingTargetContactChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.SchedulingTargetContact schedulingTargetContact = await _context.SchedulingTargetContacts.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (schedulingTargetContact == null)
			{
				return NotFound();
			}

			try
			{
				schedulingTargetContact.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.SchedulingTargetContact> versionInfo = await schedulingTargetContact.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a SchedulingTargetContact.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the SchedulingTargetContact</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTargetContact/{id}/AuditHistory")]
		public async Task<IActionResult> GetSchedulingTargetContactAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.SchedulingTargetContact schedulingTargetContact = await _context.SchedulingTargetContacts.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (schedulingTargetContact == null)
			{
				return NotFound();
			}

			try
			{
				schedulingTargetContact.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.SchedulingTargetContact>> versions = await schedulingTargetContact.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a SchedulingTargetContact.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the SchedulingTargetContact</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The SchedulingTargetContact object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTargetContact/{id}/Version/{version}")]
		public async Task<IActionResult> GetSchedulingTargetContactVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.SchedulingTargetContact schedulingTargetContact = await _context.SchedulingTargetContacts.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (schedulingTargetContact == null)
			{
				return NotFound();
			}

			try
			{
				schedulingTargetContact.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.SchedulingTargetContact> versionInfo = await schedulingTargetContact.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a SchedulingTargetContact at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the SchedulingTargetContact</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The SchedulingTargetContact object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTargetContact/{id}/StateAtTime")]
		public async Task<IActionResult> GetSchedulingTargetContactStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.SchedulingTargetContact schedulingTargetContact = await _context.SchedulingTargetContacts.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (schedulingTargetContact == null)
			{
				return NotFound();
			}

			try
			{
				schedulingTargetContact.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.SchedulingTargetContact> versionInfo = await schedulingTargetContact.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a SchedulingTargetContact record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTargetContact/{id}")]
		[Route("api/SchedulingTargetContact")]
		public async Task<IActionResult> DeleteSchedulingTargetContact(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
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

			IQueryable<Database.SchedulingTargetContact> query = (from x in _context.SchedulingTargetContacts
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.SchedulingTargetContact schedulingTargetContact = await query.FirstOrDefaultAsync(cancellationToken);

			if (schedulingTargetContact == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.SchedulingTargetContact DELETE", id.ToString(), new Exception("No Scheduler.SchedulingTargetContact entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SchedulingTargetContact cloneOfExisting = (Database.SchedulingTargetContact)_context.Entry(schedulingTargetContact).GetDatabaseValues().ToObject();


			lock (schedulingTargetContactDeleteSyncRoot)
			{
			    try
			    {
			        schedulingTargetContact.deleted = true;
			        schedulingTargetContact.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        SchedulingTargetContactChangeHistory schedulingTargetContactChangeHistory = new SchedulingTargetContactChangeHistory();
			        schedulingTargetContactChangeHistory.schedulingTargetContactId = schedulingTargetContact.id;
			        schedulingTargetContactChangeHistory.versionNumber = schedulingTargetContact.versionNumber;
			        schedulingTargetContactChangeHistory.timeStamp = DateTime.UtcNow;
			        schedulingTargetContactChangeHistory.userId = securityUser.id;
			        schedulingTargetContactChangeHistory.tenantGuid = userTenantGuid;
			        schedulingTargetContactChangeHistory.data = JsonSerializer.Serialize(Database.SchedulingTargetContact.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetContact));
			        _context.SchedulingTargetContactChangeHistories.Add(schedulingTargetContactChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.SchedulingTargetContact entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.SchedulingTargetContact.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.SchedulingTargetContact.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetContact)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.SchedulingTargetContact entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.SchedulingTargetContact.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.SchedulingTargetContact.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetContact)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of SchedulingTargetContact records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SchedulingTargetContacts/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? schedulingTargetId = null,
			int? contactId = null,
			bool? isPrimary = null,
			int? relationshipTypeId = null,
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
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			IQueryable<Database.SchedulingTargetContact> query = (from stc in _context.SchedulingTargetContacts select stc);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (schedulingTargetId.HasValue == true)
			{
				query = query.Where(stc => stc.schedulingTargetId == schedulingTargetId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(stc => stc.contactId == contactId.Value);
			}
			if (isPrimary.HasValue == true)
			{
				query = query.Where(stc => stc.isPrimary == isPrimary.Value);
			}
			if (relationshipTypeId.HasValue == true)
			{
				query = query.Where(stc => stc.relationshipTypeId == relationshipTypeId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(stc => stc.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(stc => stc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(stc => stc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(stc => stc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(stc => stc.deleted == false);
				}
			}
			else
			{
				query = query.Where(stc => stc.active == true);
				query = query.Where(stc => stc.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Scheduling Target Contact, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.contact.firstName.Contains(anyStringContains)
			       || x.contact.middleName.Contains(anyStringContains)
			       || x.contact.lastName.Contains(anyStringContains)
			       || x.contact.title.Contains(anyStringContains)
			       || x.contact.company.Contains(anyStringContains)
			       || x.contact.email.Contains(anyStringContains)
			       || x.contact.phone.Contains(anyStringContains)
			       || x.contact.mobile.Contains(anyStringContains)
			       || x.contact.position.Contains(anyStringContains)
			       || x.contact.webSite.Contains(anyStringContains)
			       || x.contact.notes.Contains(anyStringContains)
			       || x.contact.attributes.Contains(anyStringContains)
			       || x.contact.color.Contains(anyStringContains)
			       || x.contact.avatarFileName.Contains(anyStringContains)
			       || x.contact.avatarMimeType.Contains(anyStringContains)
			       || x.contact.externalId.Contains(anyStringContains)
			       || x.relationshipType.name.Contains(anyStringContains)
			       || x.relationshipType.description.Contains(anyStringContains)
			       || x.relationshipType.color.Contains(anyStringContains)
			       || x.schedulingTarget.name.Contains(anyStringContains)
			       || x.schedulingTarget.description.Contains(anyStringContains)
			       || x.schedulingTarget.notes.Contains(anyStringContains)
			       || x.schedulingTarget.externalId.Contains(anyStringContains)
			       || x.schedulingTarget.color.Contains(anyStringContains)
			       || x.schedulingTarget.attributes.Contains(anyStringContains)
			       || x.schedulingTarget.avatarFileName.Contains(anyStringContains)
			       || x.schedulingTarget.avatarMimeType.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.SchedulingTargetContact.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/SchedulingTargetContact/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
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
