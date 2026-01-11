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
    /// This auto generated class provides the basic CRUD operations for the OfficeContact entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the OfficeContact entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class OfficeContactsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object officeContactPutSyncRoot = new object();
		static object officeContactDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<OfficeContactsController> _logger;

		public OfficeContactsController(SchedulerContext context, ILogger<OfficeContactsController> logger) : base("Scheduler", "OfficeContact")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of OfficeContacts filtered by the parameters provided.
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
		[Route("api/OfficeContacts")]
		public async Task<IActionResult> GetOfficeContacts(
			int? officeId = null,
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

			IQueryable<Database.OfficeContact> query = (from oc in _context.OfficeContacts select oc);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (officeId.HasValue == true)
			{
				query = query.Where(oc => oc.officeId == officeId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(oc => oc.contactId == contactId.Value);
			}
			if (isPrimary.HasValue == true)
			{
				query = query.Where(oc => oc.isPrimary == isPrimary.Value);
			}
			if (relationshipTypeId.HasValue == true)
			{
				query = query.Where(oc => oc.relationshipTypeId == relationshipTypeId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(oc => oc.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(oc => oc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(oc => oc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(oc => oc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(oc => oc.deleted == false);
				}
			}
			else
			{
				query = query.Where(oc => oc.active == true);
				query = query.Where(oc => oc.deleted == false);
			}

			query = query.OrderBy(oc => oc.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.contact);
				query = query.Include(x => x.office);
				query = query.Include(x => x.relationshipType);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Office Contact, or on an any of the string fields on its immediate relations
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
			       || (includeRelations == true && x.contact.color.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.avatarMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.office.name.Contains(anyStringContains))
			       || (includeRelations == true && x.office.description.Contains(anyStringContains))
			       || (includeRelations == true && x.office.addressLine1.Contains(anyStringContains))
			       || (includeRelations == true && x.office.addressLine2.Contains(anyStringContains))
			       || (includeRelations == true && x.office.city.Contains(anyStringContains))
			       || (includeRelations == true && x.office.postalCode.Contains(anyStringContains))
			       || (includeRelations == true && x.office.phone.Contains(anyStringContains))
			       || (includeRelations == true && x.office.email.Contains(anyStringContains))
			       || (includeRelations == true && x.office.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.office.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.office.color.Contains(anyStringContains))
			       || (includeRelations == true && x.office.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.office.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.office.avatarMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.relationshipType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.relationshipType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.relationshipType.color.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.OfficeContact> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.OfficeContact officeContact in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(officeContact, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.OfficeContact Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.OfficeContact Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of OfficeContacts filtered by the parameters provided.  Its query is similar to the GetOfficeContacts method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/OfficeContacts/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? officeId = null,
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


			IQueryable<Database.OfficeContact> query = (from oc in _context.OfficeContacts select oc);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (officeId.HasValue == true)
			{
				query = query.Where(oc => oc.officeId == officeId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(oc => oc.contactId == contactId.Value);
			}
			if (isPrimary.HasValue == true)
			{
				query = query.Where(oc => oc.isPrimary == isPrimary.Value);
			}
			if (relationshipTypeId.HasValue == true)
			{
				query = query.Where(oc => oc.relationshipTypeId == relationshipTypeId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(oc => oc.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(oc => oc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(oc => oc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(oc => oc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(oc => oc.deleted == false);
				}
			}
			else
			{
				query = query.Where(oc => oc.active == true);
				query = query.Where(oc => oc.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Office Contact, or on an any of the string fields on its immediate relations
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
			       || x.contact.color.Contains(anyStringContains)
			       || x.contact.avatarFileName.Contains(anyStringContains)
			       || x.contact.avatarMimeType.Contains(anyStringContains)
			       || x.contact.externalId.Contains(anyStringContains)
			       || x.office.name.Contains(anyStringContains)
			       || x.office.description.Contains(anyStringContains)
			       || x.office.addressLine1.Contains(anyStringContains)
			       || x.office.addressLine2.Contains(anyStringContains)
			       || x.office.city.Contains(anyStringContains)
			       || x.office.postalCode.Contains(anyStringContains)
			       || x.office.phone.Contains(anyStringContains)
			       || x.office.email.Contains(anyStringContains)
			       || x.office.notes.Contains(anyStringContains)
			       || x.office.externalId.Contains(anyStringContains)
			       || x.office.color.Contains(anyStringContains)
			       || x.office.attributes.Contains(anyStringContains)
			       || x.office.avatarFileName.Contains(anyStringContains)
			       || x.office.avatarMimeType.Contains(anyStringContains)
			       || x.relationshipType.name.Contains(anyStringContains)
			       || x.relationshipType.description.Contains(anyStringContains)
			       || x.relationshipType.color.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single OfficeContact by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/OfficeContact/{id}")]
		public async Task<IActionResult> GetOfficeContact(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.OfficeContact> query = (from oc in _context.OfficeContacts where
							(oc.id == id) &&
							(userIsAdmin == true || oc.deleted == false) &&
							(userIsWriter == true || oc.active == true)
					select oc);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.contact);
					query = query.Include(x => x.office);
					query = query.Include(x => x.relationshipType);
					query = query.AsSplitQuery();
				}

				Database.OfficeContact materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.OfficeContact Entity was read with Admin privilege." : "Scheduler.OfficeContact Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "OfficeContact", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.OfficeContact entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.OfficeContact.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.OfficeContact.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing OfficeContact record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/OfficeContact/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutOfficeContact(int id, [FromBody]Database.OfficeContact.OfficeContactDTO officeContactDTO, CancellationToken cancellationToken = default)
		{
			if (officeContactDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != officeContactDTO.id)
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


			IQueryable<Database.OfficeContact> query = (from x in _context.OfficeContacts
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.OfficeContact existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.OfficeContact PUT", id.ToString(), new Exception("No Scheduler.OfficeContact entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (officeContactDTO.objectGuid == Guid.Empty)
            {
                officeContactDTO.objectGuid = existing.objectGuid;
            }
            else if (officeContactDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a OfficeContact record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.OfficeContact cloneOfExisting = (Database.OfficeContact)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new OfficeContact object using the data from the existing record, updated with what is in the DTO.
			//
			Database.OfficeContact officeContact = (Database.OfficeContact)_context.Entry(existing).GetDatabaseValues().ToObject();
			officeContact.ApplyDTO(officeContactDTO);
			//
			// The tenant guid for any OfficeContact being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the OfficeContact because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				officeContact.tenantGuid = existing.tenantGuid;
			}

			lock (officeContactPutSyncRoot)
			{
				//
				// Validate the version number for the officeContact being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != officeContact.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "OfficeContact save attempt was made but save request was with version " + officeContact.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The OfficeContact you are trying to update has already changed.  Please try your save again after reloading the OfficeContact.");
				}
				else
				{
					// Same record.  Increase version.
					officeContact.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (officeContact.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.OfficeContact record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				try
				{
				    EntityEntry<Database.OfficeContact> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(officeContact);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        OfficeContactChangeHistory officeContactChangeHistory = new OfficeContactChangeHistory();
				        officeContactChangeHistory.officeContactId = officeContact.id;
				        officeContactChangeHistory.versionNumber = officeContact.versionNumber;
				        officeContactChangeHistory.timeStamp = DateTime.UtcNow;
				        officeContactChangeHistory.userId = securityUser.id;
				        officeContactChangeHistory.tenantGuid = userTenantGuid;
				        officeContactChangeHistory.data = JsonSerializer.Serialize(Database.OfficeContact.CreateAnonymousWithFirstLevelSubObjects(officeContact));
				        _context.OfficeContactChangeHistories.Add(officeContactChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.OfficeContact entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.OfficeContact.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.OfficeContact.CreateAnonymousWithFirstLevelSubObjects(officeContact)),
						null);

				return Ok(Database.OfficeContact.CreateAnonymous(officeContact));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.OfficeContact entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.OfficeContact.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.OfficeContact.CreateAnonymousWithFirstLevelSubObjects(officeContact)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new OfficeContact record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/OfficeContact", Name = "OfficeContact")]
		public async Task<IActionResult> PostOfficeContact([FromBody]Database.OfficeContact.OfficeContactDTO officeContactDTO, CancellationToken cancellationToken = default)
		{
			if (officeContactDTO == null)
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
			// Create a new OfficeContact object using the data from the DTO
			//
			Database.OfficeContact officeContact = Database.OfficeContact.FromDTO(officeContactDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				officeContact.tenantGuid = userTenantGuid;

				officeContact.objectGuid = Guid.NewGuid();
				officeContact.versionNumber = 1;

				_context.OfficeContacts.Add(officeContact);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the officeContact object so that no further changes will be written to the database
				    //
				    _context.Entry(officeContact).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					officeContact.OfficeContactChangeHistories = null;
					officeContact.contact = null;
					officeContact.office = null;
					officeContact.relationshipType = null;


				    OfficeContactChangeHistory officeContactChangeHistory = new OfficeContactChangeHistory();
				    officeContactChangeHistory.officeContactId = officeContact.id;
				    officeContactChangeHistory.versionNumber = officeContact.versionNumber;
				    officeContactChangeHistory.timeStamp = DateTime.UtcNow;
				    officeContactChangeHistory.userId = securityUser.id;
				    officeContactChangeHistory.tenantGuid = userTenantGuid;
				    officeContactChangeHistory.data = JsonSerializer.Serialize(Database.OfficeContact.CreateAnonymousWithFirstLevelSubObjects(officeContact));
				    _context.OfficeContactChangeHistories.Add(officeContactChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.OfficeContact entity successfully created.",
						true,
						officeContact. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.OfficeContact.CreateAnonymousWithFirstLevelSubObjects(officeContact)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.OfficeContact entity creation failed.", false, officeContact.id.ToString(), "", JsonSerializer.Serialize(Database.OfficeContact.CreateAnonymousWithFirstLevelSubObjects(officeContact)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "OfficeContact", officeContact.id, officeContact.id.ToString()));

			return CreatedAtRoute("OfficeContact", new { id = officeContact.id }, Database.OfficeContact.CreateAnonymousWithFirstLevelSubObjects(officeContact));
		}



        /// <summary>
        /// 
        /// This rolls a OfficeContact entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/OfficeContact/Rollback/{id}")]
		[Route("api/OfficeContact/Rollback")]
		public async Task<IActionResult> RollbackToOfficeContactVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.OfficeContact> query = (from x in _context.OfficeContacts
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this OfficeContact concurrently
			//
			lock (officeContactPutSyncRoot)
			{
				
				Database.OfficeContact officeContact = query.FirstOrDefault();
				
				if (officeContact == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.OfficeContact rollback", id.ToString(), new Exception("No Scheduler.OfficeContact entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the OfficeContact current state so we can log it.
				//
				Database.OfficeContact cloneOfExisting = (Database.OfficeContact)_context.Entry(officeContact).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.OfficeContactChangeHistories = null;
				cloneOfExisting.contact = null;
				cloneOfExisting.office = null;
				cloneOfExisting.relationshipType = null;

				if (versionNumber >= officeContact.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.OfficeContact rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.OfficeContact rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				OfficeContactChangeHistory officeContactChangeHistory = (from x in _context.OfficeContactChangeHistories
				                                               where
				                                               x.officeContactId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (officeContactChangeHistory != null)
				{
				    Database.OfficeContact oldOfficeContact = JsonSerializer.Deserialize<Database.OfficeContact>(officeContactChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    officeContact.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    officeContact.officeId = oldOfficeContact.officeId;
				    officeContact.contactId = oldOfficeContact.contactId;
				    officeContact.isPrimary = oldOfficeContact.isPrimary;
				    officeContact.relationshipTypeId = oldOfficeContact.relationshipTypeId;
				    officeContact.objectGuid = oldOfficeContact.objectGuid;
				    officeContact.active = oldOfficeContact.active;
				    officeContact.deleted = oldOfficeContact.deleted;

				    string serializedOfficeContact = JsonSerializer.Serialize(officeContact);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        OfficeContactChangeHistory newOfficeContactChangeHistory = new OfficeContactChangeHistory();
				        newOfficeContactChangeHistory.officeContactId = officeContact.id;
				        newOfficeContactChangeHistory.versionNumber = officeContact.versionNumber;
				        newOfficeContactChangeHistory.timeStamp = DateTime.UtcNow;
				        newOfficeContactChangeHistory.userId = securityUser.id;
				        newOfficeContactChangeHistory.tenantGuid = userTenantGuid;
				        newOfficeContactChangeHistory.data = JsonSerializer.Serialize(Database.OfficeContact.CreateAnonymousWithFirstLevelSubObjects(officeContact));
				        _context.OfficeContactChangeHistories.Add(newOfficeContactChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.OfficeContact rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.OfficeContact.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.OfficeContact.CreateAnonymousWithFirstLevelSubObjects(officeContact)),
						null);


				    return Ok(Database.OfficeContact.CreateAnonymous(officeContact));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.OfficeContact rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.OfficeContact rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}


        /// <summary>
        /// 
        /// This deletes a OfficeContact record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/OfficeContact/{id}")]
		[Route("api/OfficeContact")]
		public async Task<IActionResult> DeleteOfficeContact(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.OfficeContact> query = (from x in _context.OfficeContacts
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.OfficeContact officeContact = await query.FirstOrDefaultAsync(cancellationToken);

			if (officeContact == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.OfficeContact DELETE", id.ToString(), new Exception("No Scheduler.OfficeContact entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.OfficeContact cloneOfExisting = (Database.OfficeContact)_context.Entry(officeContact).GetDatabaseValues().ToObject();


			lock (officeContactDeleteSyncRoot)
			{
			    try
			    {
			        officeContact.deleted = true;
			        officeContact.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        OfficeContactChangeHistory officeContactChangeHistory = new OfficeContactChangeHistory();
			        officeContactChangeHistory.officeContactId = officeContact.id;
			        officeContactChangeHistory.versionNumber = officeContact.versionNumber;
			        officeContactChangeHistory.timeStamp = DateTime.UtcNow;
			        officeContactChangeHistory.userId = securityUser.id;
			        officeContactChangeHistory.tenantGuid = userTenantGuid;
			        officeContactChangeHistory.data = JsonSerializer.Serialize(Database.OfficeContact.CreateAnonymousWithFirstLevelSubObjects(officeContact));
			        _context.OfficeContactChangeHistories.Add(officeContactChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.OfficeContact entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.OfficeContact.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.OfficeContact.CreateAnonymousWithFirstLevelSubObjects(officeContact)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.OfficeContact entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.OfficeContact.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.OfficeContact.CreateAnonymousWithFirstLevelSubObjects(officeContact)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of OfficeContact records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/OfficeContacts/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? officeId = null,
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

			IQueryable<Database.OfficeContact> query = (from oc in _context.OfficeContacts select oc);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (officeId.HasValue == true)
			{
				query = query.Where(oc => oc.officeId == officeId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(oc => oc.contactId == contactId.Value);
			}
			if (isPrimary.HasValue == true)
			{
				query = query.Where(oc => oc.isPrimary == isPrimary.Value);
			}
			if (relationshipTypeId.HasValue == true)
			{
				query = query.Where(oc => oc.relationshipTypeId == relationshipTypeId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(oc => oc.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(oc => oc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(oc => oc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(oc => oc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(oc => oc.deleted == false);
				}
			}
			else
			{
				query = query.Where(oc => oc.active == true);
				query = query.Where(oc => oc.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Office Contact, or on an any of the string fields on its immediate relations
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
			       || x.contact.color.Contains(anyStringContains)
			       || x.contact.avatarFileName.Contains(anyStringContains)
			       || x.contact.avatarMimeType.Contains(anyStringContains)
			       || x.contact.externalId.Contains(anyStringContains)
			       || x.office.name.Contains(anyStringContains)
			       || x.office.description.Contains(anyStringContains)
			       || x.office.addressLine1.Contains(anyStringContains)
			       || x.office.addressLine2.Contains(anyStringContains)
			       || x.office.city.Contains(anyStringContains)
			       || x.office.postalCode.Contains(anyStringContains)
			       || x.office.phone.Contains(anyStringContains)
			       || x.office.email.Contains(anyStringContains)
			       || x.office.notes.Contains(anyStringContains)
			       || x.office.externalId.Contains(anyStringContains)
			       || x.office.color.Contains(anyStringContains)
			       || x.office.attributes.Contains(anyStringContains)
			       || x.office.avatarFileName.Contains(anyStringContains)
			       || x.office.avatarMimeType.Contains(anyStringContains)
			       || x.relationshipType.name.Contains(anyStringContains)
			       || x.relationshipType.description.Contains(anyStringContains)
			       || x.relationshipType.color.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.OfficeContact.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/OfficeContact/CreateAuditEvent")]
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
