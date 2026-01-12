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
    /// This auto generated class provides the basic CRUD operations for the ResourceContact entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ResourceContact entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ResourceContactsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object resourceContactPutSyncRoot = new object();
		static object resourceContactDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<ResourceContactsController> _logger;

		public ResourceContactsController(SchedulerContext context, ILogger<ResourceContactsController> logger) : base("Scheduler", "ResourceContact")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ResourceContacts filtered by the parameters provided.
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
		[Route("api/ResourceContacts")]
		public async Task<IActionResult> GetResourceContacts(
			int? resourceId = null,
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

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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

			IQueryable<Database.ResourceContact> query = (from rc in _context.ResourceContacts select rc);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (resourceId.HasValue == true)
			{
				query = query.Where(rc => rc.resourceId == resourceId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(rc => rc.contactId == contactId.Value);
			}
			if (isPrimary.HasValue == true)
			{
				query = query.Where(rc => rc.isPrimary == isPrimary.Value);
			}
			if (relationshipTypeId.HasValue == true)
			{
				query = query.Where(rc => rc.relationshipTypeId == relationshipTypeId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(rc => rc.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rc => rc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rc => rc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rc => rc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rc => rc.deleted == false);
				}
			}
			else
			{
				query = query.Where(rc => rc.active == true);
				query = query.Where(rc => rc.deleted == false);
			}

			query = query.OrderBy(rc => rc.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.contact);
				query = query.Include(x => x.relationshipType);
				query = query.Include(x => x.resource);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Resource Contact, or on an any of the string fields on its immediate relations
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
			
			List<Database.ResourceContact> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ResourceContact resourceContact in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(resourceContact, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ResourceContact Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ResourceContact Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ResourceContacts filtered by the parameters provided.  Its query is similar to the GetResourceContacts method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceContacts/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? resourceId = null,
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
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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


			IQueryable<Database.ResourceContact> query = (from rc in _context.ResourceContacts select rc);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (resourceId.HasValue == true)
			{
				query = query.Where(rc => rc.resourceId == resourceId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(rc => rc.contactId == contactId.Value);
			}
			if (isPrimary.HasValue == true)
			{
				query = query.Where(rc => rc.isPrimary == isPrimary.Value);
			}
			if (relationshipTypeId.HasValue == true)
			{
				query = query.Where(rc => rc.relationshipTypeId == relationshipTypeId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(rc => rc.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rc => rc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rc => rc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rc => rc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rc => rc.deleted == false);
				}
			}
			else
			{
				query = query.Where(rc => rc.active == true);
				query = query.Where(rc => rc.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Resource Contact, or on an any of the string fields on its immediate relations
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
        /// This gets a single ResourceContact by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceContact/{id}")]
		public async Task<IActionResult> GetResourceContact(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
				IQueryable<Database.ResourceContact> query = (from rc in _context.ResourceContacts where
							(rc.id == id) &&
							(userIsAdmin == true || rc.deleted == false) &&
							(userIsWriter == true || rc.active == true)
					select rc);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.contact);
					query = query.Include(x => x.relationshipType);
					query = query.Include(x => x.resource);
					query = query.AsSplitQuery();
				}

				Database.ResourceContact materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.ResourceContact Entity was read with Admin privilege." : "Scheduler.ResourceContact Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ResourceContact", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.ResourceContact entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.ResourceContact.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.ResourceContact.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ResourceContact record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ResourceContact/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutResourceContact(int id, [FromBody]Database.ResourceContact.ResourceContactDTO resourceContactDTO, CancellationToken cancellationToken = default)
		{
			if (resourceContactDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != resourceContactDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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


			IQueryable<Database.ResourceContact> query = (from x in _context.ResourceContacts
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ResourceContact existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ResourceContact PUT", id.ToString(), new Exception("No Scheduler.ResourceContact entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (resourceContactDTO.objectGuid == Guid.Empty)
            {
                resourceContactDTO.objectGuid = existing.objectGuid;
            }
            else if (resourceContactDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ResourceContact record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ResourceContact cloneOfExisting = (Database.ResourceContact)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ResourceContact object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ResourceContact resourceContact = (Database.ResourceContact)_context.Entry(existing).GetDatabaseValues().ToObject();
			resourceContact.ApplyDTO(resourceContactDTO);
			//
			// The tenant guid for any ResourceContact being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ResourceContact because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				resourceContact.tenantGuid = existing.tenantGuid;
			}

			lock (resourceContactPutSyncRoot)
			{
				//
				// Validate the version number for the resourceContact being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != resourceContact.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ResourceContact save attempt was made but save request was with version " + resourceContact.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ResourceContact you are trying to update has already changed.  Please try your save again after reloading the ResourceContact.");
				}
				else
				{
					// Same record.  Increase version.
					resourceContact.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (resourceContact.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.ResourceContact record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				try
				{
				    EntityEntry<Database.ResourceContact> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(resourceContact);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ResourceContactChangeHistory resourceContactChangeHistory = new ResourceContactChangeHistory();
				        resourceContactChangeHistory.resourceContactId = resourceContact.id;
				        resourceContactChangeHistory.versionNumber = resourceContact.versionNumber;
				        resourceContactChangeHistory.timeStamp = DateTime.UtcNow;
				        resourceContactChangeHistory.userId = securityUser.id;
				        resourceContactChangeHistory.tenantGuid = userTenantGuid;
				        resourceContactChangeHistory.data = JsonSerializer.Serialize(Database.ResourceContact.CreateAnonymousWithFirstLevelSubObjects(resourceContact));
				        _context.ResourceContactChangeHistories.Add(resourceContactChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ResourceContact entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ResourceContact.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ResourceContact.CreateAnonymousWithFirstLevelSubObjects(resourceContact)),
						null);

				return Ok(Database.ResourceContact.CreateAnonymous(resourceContact));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ResourceContact entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ResourceContact.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ResourceContact.CreateAnonymousWithFirstLevelSubObjects(resourceContact)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ResourceContact record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceContact", Name = "ResourceContact")]
		public async Task<IActionResult> PostResourceContact([FromBody]Database.ResourceContact.ResourceContactDTO resourceContactDTO, CancellationToken cancellationToken = default)
		{
			if (resourceContactDTO == null)
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
			// Create a new ResourceContact object using the data from the DTO
			//
			Database.ResourceContact resourceContact = Database.ResourceContact.FromDTO(resourceContactDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				resourceContact.tenantGuid = userTenantGuid;

				resourceContact.objectGuid = Guid.NewGuid();
				resourceContact.versionNumber = 1;

				_context.ResourceContacts.Add(resourceContact);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the resourceContact object so that no further changes will be written to the database
				    //
				    _context.Entry(resourceContact).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					resourceContact.ResourceContactChangeHistories = null;
					resourceContact.contact = null;
					resourceContact.relationshipType = null;
					resourceContact.resource = null;


				    ResourceContactChangeHistory resourceContactChangeHistory = new ResourceContactChangeHistory();
				    resourceContactChangeHistory.resourceContactId = resourceContact.id;
				    resourceContactChangeHistory.versionNumber = resourceContact.versionNumber;
				    resourceContactChangeHistory.timeStamp = DateTime.UtcNow;
				    resourceContactChangeHistory.userId = securityUser.id;
				    resourceContactChangeHistory.tenantGuid = userTenantGuid;
				    resourceContactChangeHistory.data = JsonSerializer.Serialize(Database.ResourceContact.CreateAnonymousWithFirstLevelSubObjects(resourceContact));
				    _context.ResourceContactChangeHistories.Add(resourceContactChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.ResourceContact entity successfully created.",
						true,
						resourceContact. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ResourceContact.CreateAnonymousWithFirstLevelSubObjects(resourceContact)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.ResourceContact entity creation failed.", false, resourceContact.id.ToString(), "", JsonSerializer.Serialize(Database.ResourceContact.CreateAnonymousWithFirstLevelSubObjects(resourceContact)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ResourceContact", resourceContact.id, resourceContact.id.ToString()));

			return CreatedAtRoute("ResourceContact", new { id = resourceContact.id }, Database.ResourceContact.CreateAnonymousWithFirstLevelSubObjects(resourceContact));
		}



        /// <summary>
        /// 
        /// This rolls a ResourceContact entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceContact/Rollback/{id}")]
		[Route("api/ResourceContact/Rollback")]
		public async Task<IActionResult> RollbackToResourceContactVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.ResourceContact> query = (from x in _context.ResourceContacts
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this ResourceContact concurrently
			//
			lock (resourceContactPutSyncRoot)
			{
				
				Database.ResourceContact resourceContact = query.FirstOrDefault();
				
				if (resourceContact == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ResourceContact rollback", id.ToString(), new Exception("No Scheduler.ResourceContact entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ResourceContact current state so we can log it.
				//
				Database.ResourceContact cloneOfExisting = (Database.ResourceContact)_context.Entry(resourceContact).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.ResourceContactChangeHistories = null;
				cloneOfExisting.contact = null;
				cloneOfExisting.relationshipType = null;
				cloneOfExisting.resource = null;

				if (versionNumber >= resourceContact.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.ResourceContact rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.ResourceContact rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ResourceContactChangeHistory resourceContactChangeHistory = (from x in _context.ResourceContactChangeHistories
				                                               where
				                                               x.resourceContactId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (resourceContactChangeHistory != null)
				{
				    Database.ResourceContact oldResourceContact = JsonSerializer.Deserialize<Database.ResourceContact>(resourceContactChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    resourceContact.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    resourceContact.resourceId = oldResourceContact.resourceId;
				    resourceContact.contactId = oldResourceContact.contactId;
				    resourceContact.isPrimary = oldResourceContact.isPrimary;
				    resourceContact.relationshipTypeId = oldResourceContact.relationshipTypeId;
				    resourceContact.objectGuid = oldResourceContact.objectGuid;
				    resourceContact.active = oldResourceContact.active;
				    resourceContact.deleted = oldResourceContact.deleted;

				    string serializedResourceContact = JsonSerializer.Serialize(resourceContact);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ResourceContactChangeHistory newResourceContactChangeHistory = new ResourceContactChangeHistory();
				        newResourceContactChangeHistory.resourceContactId = resourceContact.id;
				        newResourceContactChangeHistory.versionNumber = resourceContact.versionNumber;
				        newResourceContactChangeHistory.timeStamp = DateTime.UtcNow;
				        newResourceContactChangeHistory.userId = securityUser.id;
				        newResourceContactChangeHistory.tenantGuid = userTenantGuid;
				        newResourceContactChangeHistory.data = JsonSerializer.Serialize(Database.ResourceContact.CreateAnonymousWithFirstLevelSubObjects(resourceContact));
				        _context.ResourceContactChangeHistories.Add(newResourceContactChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ResourceContact rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ResourceContact.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ResourceContact.CreateAnonymousWithFirstLevelSubObjects(resourceContact)),
						null);


				    return Ok(Database.ResourceContact.CreateAnonymous(resourceContact));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.ResourceContact rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.ResourceContact rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}


        /// <summary>
        /// 
        /// This deletes a ResourceContact record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ResourceContact/{id}")]
		[Route("api/ResourceContact")]
		public async Task<IActionResult> DeleteResourceContact(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ResourceContact> query = (from x in _context.ResourceContacts
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ResourceContact resourceContact = await query.FirstOrDefaultAsync(cancellationToken);

			if (resourceContact == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ResourceContact DELETE", id.ToString(), new Exception("No Scheduler.ResourceContact entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ResourceContact cloneOfExisting = (Database.ResourceContact)_context.Entry(resourceContact).GetDatabaseValues().ToObject();


			lock (resourceContactDeleteSyncRoot)
			{
			    try
			    {
			        resourceContact.deleted = true;
			        resourceContact.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ResourceContactChangeHistory resourceContactChangeHistory = new ResourceContactChangeHistory();
			        resourceContactChangeHistory.resourceContactId = resourceContact.id;
			        resourceContactChangeHistory.versionNumber = resourceContact.versionNumber;
			        resourceContactChangeHistory.timeStamp = DateTime.UtcNow;
			        resourceContactChangeHistory.userId = securityUser.id;
			        resourceContactChangeHistory.tenantGuid = userTenantGuid;
			        resourceContactChangeHistory.data = JsonSerializer.Serialize(Database.ResourceContact.CreateAnonymousWithFirstLevelSubObjects(resourceContact));
			        _context.ResourceContactChangeHistories.Add(resourceContactChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ResourceContact entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ResourceContact.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ResourceContact.CreateAnonymousWithFirstLevelSubObjects(resourceContact)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ResourceContact entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ResourceContact.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ResourceContact.CreateAnonymousWithFirstLevelSubObjects(resourceContact)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of ResourceContact records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ResourceContacts/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? resourceId = null,
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
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);

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

			IQueryable<Database.ResourceContact> query = (from rc in _context.ResourceContacts select rc);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (resourceId.HasValue == true)
			{
				query = query.Where(rc => rc.resourceId == resourceId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(rc => rc.contactId == contactId.Value);
			}
			if (isPrimary.HasValue == true)
			{
				query = query.Where(rc => rc.isPrimary == isPrimary.Value);
			}
			if (relationshipTypeId.HasValue == true)
			{
				query = query.Where(rc => rc.relationshipTypeId == relationshipTypeId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(rc => rc.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rc => rc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rc => rc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rc => rc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rc => rc.deleted == false);
				}
			}
			else
			{
				query = query.Where(rc => rc.active == true);
				query = query.Where(rc => rc.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Resource Contact, or on an any of the string fields on its immediate relations
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


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ResourceContact.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ResourceContact/CreateAuditEvent")]
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
