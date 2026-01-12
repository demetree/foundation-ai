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
    /// This auto generated class provides the basic CRUD operations for the ContactTag entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ContactTag entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ContactTagsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object contactTagPutSyncRoot = new object();
		static object contactTagDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<ContactTagsController> _logger;

		public ContactTagsController(SchedulerContext context, ILogger<ContactTagsController> logger) : base("Scheduler", "ContactTag")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ContactTags filtered by the parameters provided.
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
		[Route("api/ContactTags")]
		public async Task<IActionResult> GetContactTags(
			int? contactId = null,
			int? tagId = null,
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

			IQueryable<Database.ContactTag> query = (from ct in _context.ContactTags select ct);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (contactId.HasValue == true)
			{
				query = query.Where(ct => ct.contactId == contactId.Value);
			}
			if (tagId.HasValue == true)
			{
				query = query.Where(ct => ct.tagId == tagId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ct => ct.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ct => ct.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ct => ct.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ct => ct.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ct => ct.deleted == false);
				}
			}
			else
			{
				query = query.Where(ct => ct.active == true);
				query = query.Where(ct => ct.deleted == false);
			}

			query = query.OrderBy(ct => ct.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.contact);
				query = query.Include(x => x.tag);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Contact Tag, or on an any of the string fields on its immediate relations
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
			       || (includeRelations == true && x.tag.name.Contains(anyStringContains))
			       || (includeRelations == true && x.tag.description.Contains(anyStringContains))
			       || (includeRelations == true && x.tag.color.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.ContactTag> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ContactTag contactTag in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(contactTag, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ContactTag Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ContactTag Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ContactTags filtered by the parameters provided.  Its query is similar to the GetContactTags method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContactTags/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? contactId = null,
			int? tagId = null,
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


			IQueryable<Database.ContactTag> query = (from ct in _context.ContactTags select ct);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (contactId.HasValue == true)
			{
				query = query.Where(ct => ct.contactId == contactId.Value);
			}
			if (tagId.HasValue == true)
			{
				query = query.Where(ct => ct.tagId == tagId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ct => ct.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ct => ct.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ct => ct.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ct => ct.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ct => ct.deleted == false);
				}
			}
			else
			{
				query = query.Where(ct => ct.active == true);
				query = query.Where(ct => ct.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Contact Tag, or on an any of the string fields on its immediate relations
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
			       || x.tag.name.Contains(anyStringContains)
			       || x.tag.description.Contains(anyStringContains)
			       || x.tag.color.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ContactTag by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContactTag/{id}")]
		public async Task<IActionResult> GetContactTag(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ContactTag> query = (from ct in _context.ContactTags where
							(ct.id == id) &&
							(userIsAdmin == true || ct.deleted == false) &&
							(userIsWriter == true || ct.active == true)
					select ct);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.contact);
					query = query.Include(x => x.tag);
					query = query.AsSplitQuery();
				}

				Database.ContactTag materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.ContactTag Entity was read with Admin privilege." : "Scheduler.ContactTag Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ContactTag", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.ContactTag entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.ContactTag.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.ContactTag.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ContactTag record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ContactTag/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutContactTag(int id, [FromBody]Database.ContactTag.ContactTagDTO contactTagDTO, CancellationToken cancellationToken = default)
		{
			if (contactTagDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != contactTagDTO.id)
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


			IQueryable<Database.ContactTag> query = (from x in _context.ContactTags
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ContactTag existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ContactTag PUT", id.ToString(), new Exception("No Scheduler.ContactTag entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (contactTagDTO.objectGuid == Guid.Empty)
            {
                contactTagDTO.objectGuid = existing.objectGuid;
            }
            else if (contactTagDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ContactTag record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ContactTag cloneOfExisting = (Database.ContactTag)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ContactTag object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ContactTag contactTag = (Database.ContactTag)_context.Entry(existing).GetDatabaseValues().ToObject();
			contactTag.ApplyDTO(contactTagDTO);
			//
			// The tenant guid for any ContactTag being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ContactTag because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				contactTag.tenantGuid = existing.tenantGuid;
			}

			lock (contactTagPutSyncRoot)
			{
				//
				// Validate the version number for the contactTag being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != contactTag.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ContactTag save attempt was made but save request was with version " + contactTag.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ContactTag you are trying to update has already changed.  Please try your save again after reloading the ContactTag.");
				}
				else
				{
					// Same record.  Increase version.
					contactTag.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (contactTag.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.ContactTag record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				try
				{
				    EntityEntry<Database.ContactTag> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(contactTag);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ContactTagChangeHistory contactTagChangeHistory = new ContactTagChangeHistory();
				        contactTagChangeHistory.contactTagId = contactTag.id;
				        contactTagChangeHistory.versionNumber = contactTag.versionNumber;
				        contactTagChangeHistory.timeStamp = DateTime.UtcNow;
				        contactTagChangeHistory.userId = securityUser.id;
				        contactTagChangeHistory.tenantGuid = userTenantGuid;
				        contactTagChangeHistory.data = JsonSerializer.Serialize(Database.ContactTag.CreateAnonymousWithFirstLevelSubObjects(contactTag));
				        _context.ContactTagChangeHistories.Add(contactTagChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ContactTag entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ContactTag.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ContactTag.CreateAnonymousWithFirstLevelSubObjects(contactTag)),
						null);

				return Ok(Database.ContactTag.CreateAnonymous(contactTag));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ContactTag entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ContactTag.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ContactTag.CreateAnonymousWithFirstLevelSubObjects(contactTag)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ContactTag record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContactTag", Name = "ContactTag")]
		public async Task<IActionResult> PostContactTag([FromBody]Database.ContactTag.ContactTagDTO contactTagDTO, CancellationToken cancellationToken = default)
		{
			if (contactTagDTO == null)
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
			// Create a new ContactTag object using the data from the DTO
			//
			Database.ContactTag contactTag = Database.ContactTag.FromDTO(contactTagDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				contactTag.tenantGuid = userTenantGuid;

				contactTag.objectGuid = Guid.NewGuid();
				contactTag.versionNumber = 1;

				_context.ContactTags.Add(contactTag);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the contactTag object so that no further changes will be written to the database
				    //
				    _context.Entry(contactTag).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					contactTag.ContactTagChangeHistories = null;
					contactTag.contact = null;
					contactTag.tag = null;


				    ContactTagChangeHistory contactTagChangeHistory = new ContactTagChangeHistory();
				    contactTagChangeHistory.contactTagId = contactTag.id;
				    contactTagChangeHistory.versionNumber = contactTag.versionNumber;
				    contactTagChangeHistory.timeStamp = DateTime.UtcNow;
				    contactTagChangeHistory.userId = securityUser.id;
				    contactTagChangeHistory.tenantGuid = userTenantGuid;
				    contactTagChangeHistory.data = JsonSerializer.Serialize(Database.ContactTag.CreateAnonymousWithFirstLevelSubObjects(contactTag));
				    _context.ContactTagChangeHistories.Add(contactTagChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.ContactTag entity successfully created.",
						true,
						contactTag. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ContactTag.CreateAnonymousWithFirstLevelSubObjects(contactTag)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.ContactTag entity creation failed.", false, contactTag.id.ToString(), "", JsonSerializer.Serialize(Database.ContactTag.CreateAnonymousWithFirstLevelSubObjects(contactTag)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ContactTag", contactTag.id, contactTag.id.ToString()));

			return CreatedAtRoute("ContactTag", new { id = contactTag.id }, Database.ContactTag.CreateAnonymousWithFirstLevelSubObjects(contactTag));
		}



        /// <summary>
        /// 
        /// This rolls a ContactTag entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContactTag/Rollback/{id}")]
		[Route("api/ContactTag/Rollback")]
		public async Task<IActionResult> RollbackToContactTagVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.ContactTag> query = (from x in _context.ContactTags
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this ContactTag concurrently
			//
			lock (contactTagPutSyncRoot)
			{
				
				Database.ContactTag contactTag = query.FirstOrDefault();
				
				if (contactTag == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ContactTag rollback", id.ToString(), new Exception("No Scheduler.ContactTag entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ContactTag current state so we can log it.
				//
				Database.ContactTag cloneOfExisting = (Database.ContactTag)_context.Entry(contactTag).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.ContactTagChangeHistories = null;
				cloneOfExisting.contact = null;
				cloneOfExisting.tag = null;

				if (versionNumber >= contactTag.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.ContactTag rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.ContactTag rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ContactTagChangeHistory contactTagChangeHistory = (from x in _context.ContactTagChangeHistories
				                                               where
				                                               x.contactTagId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (contactTagChangeHistory != null)
				{
				    Database.ContactTag oldContactTag = JsonSerializer.Deserialize<Database.ContactTag>(contactTagChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    contactTag.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    contactTag.contactId = oldContactTag.contactId;
				    contactTag.tagId = oldContactTag.tagId;
				    contactTag.objectGuid = oldContactTag.objectGuid;
				    contactTag.active = oldContactTag.active;
				    contactTag.deleted = oldContactTag.deleted;

				    string serializedContactTag = JsonSerializer.Serialize(contactTag);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ContactTagChangeHistory newContactTagChangeHistory = new ContactTagChangeHistory();
				        newContactTagChangeHistory.contactTagId = contactTag.id;
				        newContactTagChangeHistory.versionNumber = contactTag.versionNumber;
				        newContactTagChangeHistory.timeStamp = DateTime.UtcNow;
				        newContactTagChangeHistory.userId = securityUser.id;
				        newContactTagChangeHistory.tenantGuid = userTenantGuid;
				        newContactTagChangeHistory.data = JsonSerializer.Serialize(Database.ContactTag.CreateAnonymousWithFirstLevelSubObjects(contactTag));
				        _context.ContactTagChangeHistories.Add(newContactTagChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ContactTag rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ContactTag.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ContactTag.CreateAnonymousWithFirstLevelSubObjects(contactTag)),
						null);


				    return Ok(Database.ContactTag.CreateAnonymous(contactTag));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.ContactTag rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.ContactTag rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}


        /// <summary>
        /// 
        /// This deletes a ContactTag record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContactTag/{id}")]
		[Route("api/ContactTag")]
		public async Task<IActionResult> DeleteContactTag(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ContactTag> query = (from x in _context.ContactTags
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ContactTag contactTag = await query.FirstOrDefaultAsync(cancellationToken);

			if (contactTag == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ContactTag DELETE", id.ToString(), new Exception("No Scheduler.ContactTag entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ContactTag cloneOfExisting = (Database.ContactTag)_context.Entry(contactTag).GetDatabaseValues().ToObject();


			lock (contactTagDeleteSyncRoot)
			{
			    try
			    {
			        contactTag.deleted = true;
			        contactTag.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ContactTagChangeHistory contactTagChangeHistory = new ContactTagChangeHistory();
			        contactTagChangeHistory.contactTagId = contactTag.id;
			        contactTagChangeHistory.versionNumber = contactTag.versionNumber;
			        contactTagChangeHistory.timeStamp = DateTime.UtcNow;
			        contactTagChangeHistory.userId = securityUser.id;
			        contactTagChangeHistory.tenantGuid = userTenantGuid;
			        contactTagChangeHistory.data = JsonSerializer.Serialize(Database.ContactTag.CreateAnonymousWithFirstLevelSubObjects(contactTag));
			        _context.ContactTagChangeHistories.Add(contactTagChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ContactTag entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ContactTag.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ContactTag.CreateAnonymousWithFirstLevelSubObjects(contactTag)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ContactTag entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ContactTag.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ContactTag.CreateAnonymousWithFirstLevelSubObjects(contactTag)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of ContactTag records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ContactTags/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? contactId = null,
			int? tagId = null,
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

			IQueryable<Database.ContactTag> query = (from ct in _context.ContactTags select ct);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (contactId.HasValue == true)
			{
				query = query.Where(ct => ct.contactId == contactId.Value);
			}
			if (tagId.HasValue == true)
			{
				query = query.Where(ct => ct.tagId == tagId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ct => ct.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ct => ct.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ct => ct.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ct => ct.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ct => ct.deleted == false);
				}
			}
			else
			{
				query = query.Where(ct => ct.active == true);
				query = query.Where(ct => ct.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Contact Tag, or on an any of the string fields on its immediate relations
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
			       || x.tag.name.Contains(anyStringContains)
			       || x.tag.description.Contains(anyStringContains)
			       || x.tag.color.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ContactTag.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ContactTag/CreateAuditEvent")]
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
