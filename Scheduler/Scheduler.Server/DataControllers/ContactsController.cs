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
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the Contact entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Contact entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ContactsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object contactPutSyncRoot = new object();
		static object contactDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<ContactsController> _logger;

		public ContactsController(SchedulerContext context, ILogger<ContactsController> logger) : base("Scheduler", "Contact")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Contacts filtered by the parameters provided.
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
		[Route("api/Contacts")]
		public async Task<IActionResult> GetContacts(
			int? contactTypeId = null,
			string firstName = null,
			string middleName = null,
			string lastName = null,
			int? salutationId = null,
			string title = null,
			string company = null,
			string email = null,
			string phone = null,
			string mobile = null,
			string position = null,
			string webSite = null,
			int? contactMethodId = null,
			string notes = null,
			int? timeZoneId = null,
			string attributes = null,
			int? iconId = null,
			string color = null,
			string avatarFileName = null,
			long? avatarSize = null,
			string avatarMimeType = null,
			string externalId = null,
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

			IQueryable<Database.Contact> query = (from c in _context.Contacts select c);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (contactTypeId.HasValue == true)
			{
				query = query.Where(c => c.contactTypeId == contactTypeId.Value);
			}
			if (string.IsNullOrEmpty(firstName) == false)
			{
				query = query.Where(c => c.firstName == firstName);
			}
			if (string.IsNullOrEmpty(middleName) == false)
			{
				query = query.Where(c => c.middleName == middleName);
			}
			if (string.IsNullOrEmpty(lastName) == false)
			{
				query = query.Where(c => c.lastName == lastName);
			}
			if (salutationId.HasValue == true)
			{
				query = query.Where(c => c.salutationId == salutationId.Value);
			}
			if (string.IsNullOrEmpty(title) == false)
			{
				query = query.Where(c => c.title == title);
			}
			if (string.IsNullOrEmpty(company) == false)
			{
				query = query.Where(c => c.company == company);
			}
			if (string.IsNullOrEmpty(email) == false)
			{
				query = query.Where(c => c.email == email);
			}
			if (string.IsNullOrEmpty(phone) == false)
			{
				query = query.Where(c => c.phone == phone);
			}
			if (string.IsNullOrEmpty(mobile) == false)
			{
				query = query.Where(c => c.mobile == mobile);
			}
			if (string.IsNullOrEmpty(position) == false)
			{
				query = query.Where(c => c.position == position);
			}
			if (string.IsNullOrEmpty(webSite) == false)
			{
				query = query.Where(c => c.webSite == webSite);
			}
			if (contactMethodId.HasValue == true)
			{
				query = query.Where(c => c.contactMethodId == contactMethodId.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(c => c.notes == notes);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(c => c.timeZoneId == timeZoneId.Value);
			}
			if (string.IsNullOrEmpty(attributes) == false)
			{
				query = query.Where(c => c.attributes == attributes);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(c => c.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(c => c.color == color);
			}
			if (string.IsNullOrEmpty(avatarFileName) == false)
			{
				query = query.Where(c => c.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(c => c.avatarSize == avatarSize.Value);
			}
			if (string.IsNullOrEmpty(avatarMimeType) == false)
			{
				query = query.Where(c => c.avatarMimeType == avatarMimeType);
			}
			if (string.IsNullOrEmpty(externalId) == false)
			{
				query = query.Where(c => c.externalId == externalId);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(c => c.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(c => c.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(c => c.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(c => c.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(c => c.deleted == false);
				}
			}
			else
			{
				query = query.Where(c => c.active == true);
				query = query.Where(c => c.deleted == false);
			}

			query = query.OrderBy(c => c.firstName).ThenBy(c => c.middleName).ThenBy(c => c.lastName);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.contactMethod);
				query = query.Include(x => x.contactType);
				query = query.Include(x => x.icon);
				query = query.Include(x => x.salutation);
				query = query.Include(x => x.timeZone);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Contact, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.firstName.Contains(anyStringContains)
			       || x.middleName.Contains(anyStringContains)
			       || x.lastName.Contains(anyStringContains)
			       || x.title.Contains(anyStringContains)
			       || x.company.Contains(anyStringContains)
			       || x.email.Contains(anyStringContains)
			       || x.phone.Contains(anyStringContains)
			       || x.mobile.Contains(anyStringContains)
			       || x.position.Contains(anyStringContains)
			       || x.webSite.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.attributes.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.avatarFileName.Contains(anyStringContains)
			       || x.avatarMimeType.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || (includeRelations == true && x.contactMethod.name.Contains(anyStringContains))
			       || (includeRelations == true && x.contactMethod.description.Contains(anyStringContains))
			       || (includeRelations == true && x.contactMethod.color.Contains(anyStringContains))
			       || (includeRelations == true && x.contactType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.contactType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.contactType.color.Contains(anyStringContains))
			       || (includeRelations == true && x.icon.name.Contains(anyStringContains))
			       || (includeRelations == true && x.icon.fontAwesomeCode.Contains(anyStringContains))
			       || (includeRelations == true && x.salutation.name.Contains(anyStringContains))
			       || (includeRelations == true && x.salutation.description.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.name.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.description.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.ianaTimeZone.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.abbreviation.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.abbreviationDaylightSavings.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.Contact> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Contact contact in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(contact, databaseStoresDateWithTimeZone);
			}


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			if (diskBasedBinaryStorageMode == true)
			{
				var tasks = materialized.Select(async contact =>
				{

					if (contact.avatarData == null &&
					    contact.avatarSize.HasValue == true &&
					    contact.avatarSize.Value > 0)
					{
					    contact.avatarData = await LoadDataFromDiskAsync(contact.objectGuid, contact.versionNumber, "data");
					}

				}).ToList();

				// Run tasks concurrently and await their completion
				await Task.WhenAll(tasks);
			}

			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.Contact Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.Contact Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Contacts filtered by the parameters provided.  Its query is similar to the GetContacts method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Contacts/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? contactTypeId = null,
			string firstName = null,
			string middleName = null,
			string lastName = null,
			int? salutationId = null,
			string title = null,
			string company = null,
			string email = null,
			string phone = null,
			string mobile = null,
			string position = null,
			string webSite = null,
			int? contactMethodId = null,
			string notes = null,
			int? timeZoneId = null,
			string attributes = null,
			int? iconId = null,
			string color = null,
			string avatarFileName = null,
			long? avatarSize = null,
			string avatarMimeType = null,
			string externalId = null,
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


			IQueryable<Database.Contact> query = (from c in _context.Contacts select c);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (contactTypeId.HasValue == true)
			{
				query = query.Where(c => c.contactTypeId == contactTypeId.Value);
			}
			if (firstName != null)
			{
				query = query.Where(c => c.firstName == firstName);
			}
			if (middleName != null)
			{
				query = query.Where(c => c.middleName == middleName);
			}
			if (lastName != null)
			{
				query = query.Where(c => c.lastName == lastName);
			}
			if (salutationId.HasValue == true)
			{
				query = query.Where(c => c.salutationId == salutationId.Value);
			}
			if (title != null)
			{
				query = query.Where(c => c.title == title);
			}
			if (company != null)
			{
				query = query.Where(c => c.company == company);
			}
			if (email != null)
			{
				query = query.Where(c => c.email == email);
			}
			if (phone != null)
			{
				query = query.Where(c => c.phone == phone);
			}
			if (mobile != null)
			{
				query = query.Where(c => c.mobile == mobile);
			}
			if (position != null)
			{
				query = query.Where(c => c.position == position);
			}
			if (webSite != null)
			{
				query = query.Where(c => c.webSite == webSite);
			}
			if (contactMethodId.HasValue == true)
			{
				query = query.Where(c => c.contactMethodId == contactMethodId.Value);
			}
			if (notes != null)
			{
				query = query.Where(c => c.notes == notes);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(c => c.timeZoneId == timeZoneId.Value);
			}
			if (attributes != null)
			{
				query = query.Where(c => c.attributes == attributes);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(c => c.iconId == iconId.Value);
			}
			if (color != null)
			{
				query = query.Where(c => c.color == color);
			}
			if (avatarFileName != null)
			{
				query = query.Where(c => c.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(c => c.avatarSize == avatarSize.Value);
			}
			if (avatarMimeType != null)
			{
				query = query.Where(c => c.avatarMimeType == avatarMimeType);
			}
			if (externalId != null)
			{
				query = query.Where(c => c.externalId == externalId);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(c => c.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(c => c.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(c => c.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(c => c.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(c => c.deleted == false);
				}
			}
			else
			{
				query = query.Where(c => c.active == true);
				query = query.Where(c => c.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Contact, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.firstName.Contains(anyStringContains)
			       || x.middleName.Contains(anyStringContains)
			       || x.lastName.Contains(anyStringContains)
			       || x.title.Contains(anyStringContains)
			       || x.company.Contains(anyStringContains)
			       || x.email.Contains(anyStringContains)
			       || x.phone.Contains(anyStringContains)
			       || x.mobile.Contains(anyStringContains)
			       || x.position.Contains(anyStringContains)
			       || x.webSite.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.attributes.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.avatarFileName.Contains(anyStringContains)
			       || x.avatarMimeType.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.contactMethod.name.Contains(anyStringContains)
			       || x.contactMethod.description.Contains(anyStringContains)
			       || x.contactMethod.color.Contains(anyStringContains)
			       || x.contactType.name.Contains(anyStringContains)
			       || x.contactType.description.Contains(anyStringContains)
			       || x.contactType.color.Contains(anyStringContains)
			       || x.icon.name.Contains(anyStringContains)
			       || x.icon.fontAwesomeCode.Contains(anyStringContains)
			       || x.salutation.name.Contains(anyStringContains)
			       || x.salutation.description.Contains(anyStringContains)
			       || x.timeZone.name.Contains(anyStringContains)
			       || x.timeZone.description.Contains(anyStringContains)
			       || x.timeZone.ianaTimeZone.Contains(anyStringContains)
			       || x.timeZone.abbreviation.Contains(anyStringContains)
			       || x.timeZone.abbreviationDaylightSavings.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single Contact by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Contact/{id}")]
		public async Task<IActionResult> GetContact(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.Contact> query = (from c in _context.Contacts where
							(c.id == id) &&
							(userIsAdmin == true || c.deleted == false) &&
							(userIsWriter == true || c.active == true)
					select c);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.contactMethod);
					query = query.Include(x => x.contactType);
					query = query.Include(x => x.icon);
					query = query.Include(x => x.salutation);
					query = query.Include(x => x.timeZone);
					query = query.AsSplitQuery();
				}

				Database.Contact materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

					if (diskBasedBinaryStorageMode == true &&
					    materialized.avatarData == null &&
					    materialized.avatarSize.HasValue == true &&
					    materialized.avatarSize.Value > 0)
					{
					    materialized.avatarData = await LoadDataFromDiskAsync(materialized.objectGuid, materialized.versionNumber, "data", cancellationToken);
					}

					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.Contact Entity was read with Admin privilege." : "Scheduler.Contact Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Contact", materialized.id, materialized.firstName));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.Contact entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.Contact.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.Contact.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing Contact record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Contact/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		[RequestSizeLimit(5000000)]
		public async Task<IActionResult> PutContact(int id, [FromBody]Database.Contact.ContactDTO contactDTO, CancellationToken cancellationToken = default)
		{
			if (contactDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != contactDTO.id)
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


			IQueryable<Database.Contact> query = (from x in _context.Contacts
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Contact existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Contact PUT", id.ToString(), new Exception("No Scheduler.Contact entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (contactDTO.objectGuid == Guid.Empty)
            {
                contactDTO.objectGuid = existing.objectGuid;
            }
            else if (contactDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Contact record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Contact cloneOfExisting = (Database.Contact)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Contact object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Contact contact = (Database.Contact)_context.Entry(existing).GetDatabaseValues().ToObject();
			contact.ApplyDTO(contactDTO);
			//
			// The tenant guid for any Contact being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the Contact because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				contact.tenantGuid = existing.tenantGuid;
			}

			lock (contactPutSyncRoot)
			{
				//
				// Validate the version number for the contact being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != contact.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Contact save attempt was made but save request was with version " + contact.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The Contact you are trying to update has already changed.  Please try your save again after reloading the Contact.");
				}
				else
				{
					// Same record.  Increase version.
					contact.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (contact.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.Contact record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (contact.firstName != null && contact.firstName.Length > 250)
				{
					contact.firstName = contact.firstName.Substring(0, 250);
				}

				if (contact.middleName != null && contact.middleName.Length > 250)
				{
					contact.middleName = contact.middleName.Substring(0, 250);
				}

				if (contact.lastName != null && contact.lastName.Length > 250)
				{
					contact.lastName = contact.lastName.Substring(0, 250);
				}

				if (contact.title != null && contact.title.Length > 250)
				{
					contact.title = contact.title.Substring(0, 250);
				}

				if (contact.company != null && contact.company.Length > 250)
				{
					contact.company = contact.company.Substring(0, 250);
				}

				if (contact.email != null && contact.email.Length > 250)
				{
					contact.email = contact.email.Substring(0, 250);
				}

				if (contact.phone != null && contact.phone.Length > 50)
				{
					contact.phone = contact.phone.Substring(0, 50);
				}

				if (contact.mobile != null && contact.mobile.Length > 50)
				{
					contact.mobile = contact.mobile.Substring(0, 50);
				}

				if (contact.position != null && contact.position.Length > 250)
				{
					contact.position = contact.position.Substring(0, 250);
				}

				if (contact.color != null && contact.color.Length > 10)
				{
					contact.color = contact.color.Substring(0, 10);
				}

				if (contact.avatarFileName != null && contact.avatarFileName.Length > 250)
				{
					contact.avatarFileName = contact.avatarFileName.Substring(0, 250);
				}

				if (contact.avatarMimeType != null && contact.avatarMimeType.Length > 100)
				{
					contact.avatarMimeType = contact.avatarMimeType.Substring(0, 100);
				}

				if (contact.externalId != null && contact.externalId.Length > 100)
				{
					contact.externalId = contact.externalId.Substring(0, 100);
				}


				//
				// Add default values for any missing data attribute fields.
				//
				if (contact.avatarData != null && string.IsNullOrEmpty(contact.avatarFileName))
				{
				    contact.avatarFileName = contact.objectGuid.ToString() + ".data";
				}

				if (contact.avatarData != null && (contact.avatarSize.HasValue == false || contact.avatarSize != contact.avatarData.Length))
				{
				    contact.avatarSize = contact.avatarData.Length;
				}

				if (contact.avatarData != null && string.IsNullOrEmpty(contact.avatarMimeType))
				{
				    contact.avatarMimeType = "application/octet-stream";
				}

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				try
				{
					byte[] dataReferenceBeforeClearing = contact.avatarData;

					if (diskBasedBinaryStorageMode == true &&
					    contact.avatarFileName != null &&
					    contact.avatarData != null &&
					    contact.avatarSize.HasValue == true &&
					    contact.avatarSize.Value > 0)
					{
					    //
					    // write the bytes to disk
					    //
					    WriteDataToDisk(contact.objectGuid, contact.versionNumber, contact.avatarData, "data");

					    //
					    // Clear the data from the object before we put it into the db
					    //
					    contact.avatarData = null;

					}

				    EntityEntry<Database.Contact> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(contact);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ContactChangeHistory contactChangeHistory = new ContactChangeHistory();
				        contactChangeHistory.contactId = contact.id;
				        contactChangeHistory.versionNumber = contact.versionNumber;
				        contactChangeHistory.timeStamp = DateTime.UtcNow;
				        contactChangeHistory.userId = securityUser.id;
				        contactChangeHistory.tenantGuid = userTenantGuid;
				        contactChangeHistory.data = JsonSerializer.Serialize(Database.Contact.CreateAnonymousWithFirstLevelSubObjects(contact));
				        _context.ContactChangeHistories.Add(contactChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    contact.avatarData = dataReferenceBeforeClearing;
					}

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Contact entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Contact.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Contact.CreateAnonymousWithFirstLevelSubObjects(contact)),
						null);

				return Ok(Database.Contact.CreateAnonymous(contact));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Contact entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Contact.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Contact.CreateAnonymousWithFirstLevelSubObjects(contact)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new Contact record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Contact", Name = "Contact")]
		[RequestSizeLimit(5000000)]
		public async Task<IActionResult> PostContact([FromBody]Database.Contact.ContactDTO contactDTO, CancellationToken cancellationToken = default)
		{
			if (contactDTO == null)
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
			// Create a new Contact object using the data from the DTO
			//
			Database.Contact contact = Database.Contact.FromDTO(contactDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				contact.tenantGuid = userTenantGuid;

				if (contact.firstName != null && contact.firstName.Length > 250)
				{
					contact.firstName = contact.firstName.Substring(0, 250);
				}

				if (contact.middleName != null && contact.middleName.Length > 250)
				{
					contact.middleName = contact.middleName.Substring(0, 250);
				}

				if (contact.lastName != null && contact.lastName.Length > 250)
				{
					contact.lastName = contact.lastName.Substring(0, 250);
				}

				if (contact.title != null && contact.title.Length > 250)
				{
					contact.title = contact.title.Substring(0, 250);
				}

				if (contact.company != null && contact.company.Length > 250)
				{
					contact.company = contact.company.Substring(0, 250);
				}

				if (contact.email != null && contact.email.Length > 250)
				{
					contact.email = contact.email.Substring(0, 250);
				}

				if (contact.phone != null && contact.phone.Length > 50)
				{
					contact.phone = contact.phone.Substring(0, 50);
				}

				if (contact.mobile != null && contact.mobile.Length > 50)
				{
					contact.mobile = contact.mobile.Substring(0, 50);
				}

				if (contact.position != null && contact.position.Length > 250)
				{
					contact.position = contact.position.Substring(0, 250);
				}

				if (contact.color != null && contact.color.Length > 10)
				{
					contact.color = contact.color.Substring(0, 10);
				}

				if (contact.avatarFileName != null && contact.avatarFileName.Length > 250)
				{
					contact.avatarFileName = contact.avatarFileName.Substring(0, 250);
				}

				if (contact.avatarMimeType != null && contact.avatarMimeType.Length > 100)
				{
					contact.avatarMimeType = contact.avatarMimeType.Substring(0, 100);
				}

				if (contact.externalId != null && contact.externalId.Length > 100)
				{
					contact.externalId = contact.externalId.Substring(0, 100);
				}

				contact.objectGuid = Guid.NewGuid();

				//
				// Add default values for any missing data attribute fields.
				//
				if (contact.avatarData != null && string.IsNullOrEmpty(contact.avatarFileName))
				{
				    contact.avatarFileName = contact.objectGuid.ToString() + ".data";
				}

				if (contact.avatarData != null && (contact.avatarSize.HasValue == false || contact.avatarSize != contact.avatarData.Length))
				{
				    contact.avatarSize = contact.avatarData.Length;
				}

				if (contact.avatarData != null && string.IsNullOrEmpty(contact.avatarMimeType))
				{
				    contact.avatarMimeType = "application/octet-stream";
				}

				contact.versionNumber = 1;

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				byte[] dataReferenceBeforeClearing = contact.avatarData;

				if (diskBasedBinaryStorageMode == true &&
				    contact.avatarData != null &&
				    contact.avatarFileName != null &&
				    contact.avatarSize.HasValue == true &&
				    contact.avatarSize.Value > 0)
				{
				    //
				    // write the bytes to disk
				    //
				    await WriteDataToDiskAsync(contact.objectGuid, contact.versionNumber, contact.avatarData, "data", cancellationToken);

				    //
				    // Clear the data from the object before we put it into the db
				    //
				    contact.avatarData = null;

				}

				_context.Contacts.Add(contact);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the contact object so that no further changes will be written to the database
				    //
				    _context.Entry(contact).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					contact.avatarData = null;
					contact.ClientContacts = null;
					contact.Constituents = null;
					contact.ContactChangeHistories = null;
					contact.ContactContactcontacts = null;
					contact.ContactContactrelatedContacts = null;
					contact.ContactInteractioncontacts = null;
					contact.ContactInteractioninitiatingContacts = null;
					contact.ContactTags = null;
					contact.EventResourceAssignments = null;
					contact.NotificationSubscriptions = null;
					contact.OfficeContacts = null;
					contact.ResourceContacts = null;
					contact.SchedulingTargetContacts = null;
					contact.contactMethod = null;
					contact.contactType = null;
					contact.icon = null;
					contact.salutation = null;
					contact.timeZone = null;


				    ContactChangeHistory contactChangeHistory = new ContactChangeHistory();
				    contactChangeHistory.contactId = contact.id;
				    contactChangeHistory.versionNumber = contact.versionNumber;
				    contactChangeHistory.timeStamp = DateTime.UtcNow;
				    contactChangeHistory.userId = securityUser.id;
				    contactChangeHistory.tenantGuid = userTenantGuid;
				    contactChangeHistory.data = JsonSerializer.Serialize(Database.Contact.CreateAnonymousWithFirstLevelSubObjects(contact));
				    _context.ContactChangeHistories.Add(contactChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.Contact entity successfully created.",
						true,
						contact. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.Contact.CreateAnonymousWithFirstLevelSubObjects(contact)),
						null);



					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    contact.avatarData = dataReferenceBeforeClearing;
					}

				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.Contact entity creation failed.", false, contact.id.ToString(), "", JsonSerializer.Serialize(Database.Contact.CreateAnonymousWithFirstLevelSubObjects(contact)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Contact", contact.id, contact.firstName));

			return CreatedAtRoute("Contact", new { id = contact.id }, Database.Contact.CreateAnonymousWithFirstLevelSubObjects(contact));
		}



        /// <summary>
        /// 
        /// This rolls a Contact entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Contact/Rollback/{id}")]
		[Route("api/Contact/Rollback")]
		public async Task<IActionResult> RollbackToContactVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.Contact> query = (from x in _context.Contacts
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();


			//
			// Make sure nobody else is editing this Contact concurrently
			//
			lock (contactPutSyncRoot)
			{
				
				Database.Contact contact = query.FirstOrDefault();
				
				if (contact == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Contact rollback", id.ToString(), new Exception("No Scheduler.Contact entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the Contact current state so we can log it.
				//
				Database.Contact cloneOfExisting = (Database.Contact)_context.Entry(contact).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.avatarData = null;
				cloneOfExisting.ClientContacts = null;
				cloneOfExisting.Constituents = null;
				cloneOfExisting.ContactChangeHistories = null;
				cloneOfExisting.ContactContactcontacts = null;
				cloneOfExisting.ContactContactrelatedContacts = null;
				cloneOfExisting.ContactInteractioncontacts = null;
				cloneOfExisting.ContactInteractioninitiatingContacts = null;
				cloneOfExisting.ContactTags = null;
				cloneOfExisting.EventResourceAssignments = null;
				cloneOfExisting.NotificationSubscriptions = null;
				cloneOfExisting.OfficeContacts = null;
				cloneOfExisting.ResourceContacts = null;
				cloneOfExisting.SchedulingTargetContacts = null;
				cloneOfExisting.contactMethod = null;
				cloneOfExisting.contactType = null;
				cloneOfExisting.icon = null;
				cloneOfExisting.salutation = null;
				cloneOfExisting.timeZone = null;

				if (versionNumber >= contact.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.Contact rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.Contact rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ContactChangeHistory contactChangeHistory = (from x in _context.ContactChangeHistories
				                                               where
				                                               x.contactId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (contactChangeHistory != null)
				{
				    Database.Contact oldContact = JsonSerializer.Deserialize<Database.Contact>(contactChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    contact.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    contact.contactTypeId = oldContact.contactTypeId;
				    contact.firstName = oldContact.firstName;
				    contact.middleName = oldContact.middleName;
				    contact.lastName = oldContact.lastName;
				    contact.salutationId = oldContact.salutationId;
				    contact.title = oldContact.title;
				    contact.birthDate = oldContact.birthDate;
				    contact.company = oldContact.company;
				    contact.email = oldContact.email;
				    contact.phone = oldContact.phone;
				    contact.mobile = oldContact.mobile;
				    contact.position = oldContact.position;
				    contact.webSite = oldContact.webSite;
				    contact.contactMethodId = oldContact.contactMethodId;
				    contact.notes = oldContact.notes;
				    contact.timeZoneId = oldContact.timeZoneId;
				    contact.attributes = oldContact.attributes;
				    contact.iconId = oldContact.iconId;
				    contact.color = oldContact.color;
				    contact.avatarFileName = oldContact.avatarFileName;
				    contact.avatarSize = oldContact.avatarSize;
				    contact.avatarData = oldContact.avatarData;
				    contact.avatarMimeType = oldContact.avatarMimeType;
				    contact.externalId = oldContact.externalId;
				    contact.objectGuid = oldContact.objectGuid;
				    contact.active = oldContact.active;
				    contact.deleted = oldContact.deleted;
				    //
				    // If disk based binary mode is on, then we need to copy the old data file over as well.
				    //
				    if (diskBasedBinaryStorageMode == true)
				    {
				    	Byte[] binaryData = LoadDataFromDisk(oldContact.objectGuid, oldContact.versionNumber, "data");

				    	//
				    	// Write out the data as the new version
				    	//
				    	WriteDataToDisk(contact.objectGuid, contact.versionNumber, binaryData, "data");
				    }

				    string serializedContact = JsonSerializer.Serialize(contact);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ContactChangeHistory newContactChangeHistory = new ContactChangeHistory();
				        newContactChangeHistory.contactId = contact.id;
				        newContactChangeHistory.versionNumber = contact.versionNumber;
				        newContactChangeHistory.timeStamp = DateTime.UtcNow;
				        newContactChangeHistory.userId = securityUser.id;
				        newContactChangeHistory.tenantGuid = userTenantGuid;
				        newContactChangeHistory.data = JsonSerializer.Serialize(Database.Contact.CreateAnonymousWithFirstLevelSubObjects(contact));
				        _context.ContactChangeHistories.Add(newContactChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Contact rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Contact.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Contact.CreateAnonymousWithFirstLevelSubObjects(contact)),
						null);


				    return Ok(Database.Contact.CreateAnonymous(contact));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.Contact rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.Contact rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a Contact.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Contact</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Contact/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetContactChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.Contact contact = await _context.Contacts.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (contact == null)
			{
				return NotFound();
			}

			try
			{
				contact.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Contact> versionInfo = await contact.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a Contact.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Contact</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Contact/{id}/AuditHistory")]
		public async Task<IActionResult> GetContactAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.Contact contact = await _context.Contacts.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (contact == null)
			{
				return NotFound();
			}

			try
			{
				contact.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.Contact>> versions = await contact.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a Contact.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Contact</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The Contact object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Contact/{id}/Version/{version}")]
		public async Task<IActionResult> GetContactVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.Contact contact = await _context.Contacts.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (contact == null)
			{
				return NotFound();
			}

			try
			{
				contact.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Contact> versionInfo = await contact.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a Contact at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Contact</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The Contact object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Contact/{id}/StateAtTime")]
		public async Task<IActionResult> GetContactStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.Contact contact = await _context.Contacts.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (contact == null)
			{
				return NotFound();
			}

			try
			{
				contact.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Contact> versionInfo = await contact.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a Contact record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Contact/{id}")]
		[Route("api/Contact")]
		public async Task<IActionResult> DeleteContact(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.Contact> query = (from x in _context.Contacts
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Contact contact = await query.FirstOrDefaultAsync(cancellationToken);

			if (contact == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Contact DELETE", id.ToString(), new Exception("No Scheduler.Contact entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Contact cloneOfExisting = (Database.Contact)_context.Entry(contact).GetDatabaseValues().ToObject();


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			lock (contactDeleteSyncRoot)
			{
			    try
			    {
			        contact.deleted = true;
			        contact.versionNumber++;

			        _context.SaveChanges();

			        //
			        // If in disk based storage mode, create a copy of the disk data file for the new version.
			        //
			        if (diskBasedBinaryStorageMode == true)
			        {
			        	Byte[] binaryData = LoadDataFromDisk(contact.objectGuid, contact.versionNumber -1, "data");

			        	//
			        	// Write out the same data
			        	//
			        	WriteDataToDisk(contact.objectGuid, contact.versionNumber, binaryData, "data");
			        }

			        //
			        // Now add the change history
			        //
			        ContactChangeHistory contactChangeHistory = new ContactChangeHistory();
			        contactChangeHistory.contactId = contact.id;
			        contactChangeHistory.versionNumber = contact.versionNumber;
			        contactChangeHistory.timeStamp = DateTime.UtcNow;
			        contactChangeHistory.userId = securityUser.id;
			        contactChangeHistory.tenantGuid = userTenantGuid;
			        contactChangeHistory.data = JsonSerializer.Serialize(Database.Contact.CreateAnonymousWithFirstLevelSubObjects(contact));
			        _context.ContactChangeHistories.Add(contactChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Contact entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Contact.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Contact.CreateAnonymousWithFirstLevelSubObjects(contact)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Contact entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Contact.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Contact.CreateAnonymousWithFirstLevelSubObjects(contact)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of Contact records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Contacts/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? contactTypeId = null,
			string firstName = null,
			string middleName = null,
			string lastName = null,
			int? salutationId = null,
			string title = null,
			string company = null,
			string email = null,
			string phone = null,
			string mobile = null,
			string position = null,
			string webSite = null,
			int? contactMethodId = null,
			string notes = null,
			int? timeZoneId = null,
			string attributes = null,
			int? iconId = null,
			string color = null,
			string avatarFileName = null,
			long? avatarSize = null,
			string avatarMimeType = null,
			string externalId = null,
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

			IQueryable<Database.Contact> query = (from c in _context.Contacts select c);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (contactTypeId.HasValue == true)
			{
				query = query.Where(c => c.contactTypeId == contactTypeId.Value);
			}
			if (string.IsNullOrEmpty(firstName) == false)
			{
				query = query.Where(c => c.firstName == firstName);
			}
			if (string.IsNullOrEmpty(middleName) == false)
			{
				query = query.Where(c => c.middleName == middleName);
			}
			if (string.IsNullOrEmpty(lastName) == false)
			{
				query = query.Where(c => c.lastName == lastName);
			}
			if (salutationId.HasValue == true)
			{
				query = query.Where(c => c.salutationId == salutationId.Value);
			}
			if (string.IsNullOrEmpty(title) == false)
			{
				query = query.Where(c => c.title == title);
			}
			if (string.IsNullOrEmpty(company) == false)
			{
				query = query.Where(c => c.company == company);
			}
			if (string.IsNullOrEmpty(email) == false)
			{
				query = query.Where(c => c.email == email);
			}
			if (string.IsNullOrEmpty(phone) == false)
			{
				query = query.Where(c => c.phone == phone);
			}
			if (string.IsNullOrEmpty(mobile) == false)
			{
				query = query.Where(c => c.mobile == mobile);
			}
			if (string.IsNullOrEmpty(position) == false)
			{
				query = query.Where(c => c.position == position);
			}
			if (string.IsNullOrEmpty(webSite) == false)
			{
				query = query.Where(c => c.webSite == webSite);
			}
			if (contactMethodId.HasValue == true)
			{
				query = query.Where(c => c.contactMethodId == contactMethodId.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(c => c.notes == notes);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(c => c.timeZoneId == timeZoneId.Value);
			}
			if (string.IsNullOrEmpty(attributes) == false)
			{
				query = query.Where(c => c.attributes == attributes);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(c => c.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(c => c.color == color);
			}
			if (string.IsNullOrEmpty(avatarFileName) == false)
			{
				query = query.Where(c => c.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(c => c.avatarSize == avatarSize.Value);
			}
			if (string.IsNullOrEmpty(avatarMimeType) == false)
			{
				query = query.Where(c => c.avatarMimeType == avatarMimeType);
			}
			if (string.IsNullOrEmpty(externalId) == false)
			{
				query = query.Where(c => c.externalId == externalId);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(c => c.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(c => c.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(c => c.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(c => c.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(c => c.deleted == false);
				}
			}
			else
			{
				query = query.Where(c => c.active == true);
				query = query.Where(c => c.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Contact, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.firstName.Contains(anyStringContains)
			       || x.middleName.Contains(anyStringContains)
			       || x.lastName.Contains(anyStringContains)
			       || x.title.Contains(anyStringContains)
			       || x.company.Contains(anyStringContains)
			       || x.email.Contains(anyStringContains)
			       || x.phone.Contains(anyStringContains)
			       || x.mobile.Contains(anyStringContains)
			       || x.position.Contains(anyStringContains)
			       || x.webSite.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.attributes.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.avatarFileName.Contains(anyStringContains)
			       || x.avatarMimeType.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.contactMethod.name.Contains(anyStringContains)
			       || x.contactMethod.description.Contains(anyStringContains)
			       || x.contactMethod.color.Contains(anyStringContains)
			       || x.contactType.name.Contains(anyStringContains)
			       || x.contactType.description.Contains(anyStringContains)
			       || x.contactType.color.Contains(anyStringContains)
			       || x.icon.name.Contains(anyStringContains)
			       || x.icon.fontAwesomeCode.Contains(anyStringContains)
			       || x.salutation.name.Contains(anyStringContains)
			       || x.salutation.description.Contains(anyStringContains)
			       || x.timeZone.name.Contains(anyStringContains)
			       || x.timeZone.description.Contains(anyStringContains)
			       || x.timeZone.ianaTimeZone.Contains(anyStringContains)
			       || x.timeZone.abbreviation.Contains(anyStringContains)
			       || x.timeZone.abbreviationDaylightSavings.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.firstName).ThenBy(x => x.middleName).ThenBy(x => x.lastName);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Contact.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Contact/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
			{
			   return Forbid();
			}

		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


        /// <summary>
        /// 
        /// This makes a Contact record a favourite for the current user.
		/// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/Contact/Favourite/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPut]
		public async Task<IActionResult> SetFavourite(int id, string description = null, CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(cancellationToken);
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


			IQueryable<Database.Contact> query = (from x in _context.Contacts
			                               where x.id == id
			                               select x);


			Database.Contact contact = await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

			if (contact != null)
			{
				if (string.IsNullOrEmpty(description) == true)
				{
					description = contact.firstName;
				}

				//
				// Add the user favourite Contact
				//
				await SecurityLogic.AddToUserFavouritesAsync(securityUser, "Contact", id, description, cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'Contact' was added for record with id of " + id + " for user " + securityUser.accountName, true);

				//
				// Return the complete list of user favourites after the addition
				//
				return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
			}
			else
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'Contact' add request was made with an invalid id value of " + id, false);
				return BadRequest();
			}
		}


        /// <summary>
        /// 
        /// This removes a Contact record from the current user's favourites.
        /// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/Contact/Favourite/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpDelete]
		public async Task<IActionResult> DeleteFavourite(int id, CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
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


			//
			// Delete the user favourite Contact
			//
			await SecurityLogic.RemoveFromUserFavouritesAsync(securityUser, "Contact", id, cancellationToken);

			await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'Contact' was removed for record with id of " + id + " for user " + securityUser.accountName, true);

			//
			// Return the complete list of user favourites after the deletion
			//
			return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
		}




        [Route("api/Contact/Data/{id:int}")]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [HttpPost]
        [HttpPut]
        public async Task<IActionResult> UploadData(int id, CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			MediaTypeHeaderValue mediaTypeHeader; 

            if (!HttpContext.Request.HasFormContentType ||
				!MediaTypeHeaderValue.TryParse(HttpContext.Request.ContentType, out mediaTypeHeader) ||
                string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
            {
                return new UnsupportedMediaTypeResult();
            }


            Database.Contact contact = await (from x in _context.Contacts where x.id == id && x.active == true && x.deleted == false select x).FirstOrDefaultAsync();
            if (contact == null)
            {
                return NotFound();
            }

            bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();


            // This will be used to signal whether we are saving data or clearing it.
            bool foundFileData = false;


            //
            // This will get the first file from the request and save it
            //
			try
			{
                MultipartReader reader = new MultipartReader(mediaTypeHeader.Boundary.Value, HttpContext.Request.Body);
                MultipartSection section = await reader.ReadNextSectionAsync();

                while (section != null)
				{
					ContentDispositionHeaderValue contentDisposition;

					bool hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);


					if (hasContentDispositionHeader && contentDisposition.DispositionType.Equals("form-data") &&
						!string.IsNullOrEmpty(contentDisposition.FileName.Value))
					{

						foundFileData = true;
						string fileName = contentDisposition.FileName.ToString().Trim('"');

						// default the mime type to be the one for arbitrary binary data unless we have a mime type on the content headers that tells us otherwise.
						MediaTypeHeaderValue mediaType;
						bool hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);

						string mimeType = "application/octet-stream";
						if (hasMediaTypeHeader && mediaTypeHeader.MediaType != null )
						{
							mimeType = mediaTypeHeader.MediaType.ToString();
						}

						lock (contactPutSyncRoot)
						{
							try
							{
								using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
								{
									contact.avatarFileName = fileName.Trim();
									contact.avatarMimeType = mimeType;
									contact.avatarSize = section.Body.Length;

									contact.versionNumber++;

									if (diskBasedBinaryStorageMode == true &&
										 contact.avatarFileName != null &&
										 contact.avatarSize > 0)
									{
										//
										// write the bytes to disk
										//
										WriteDataToDisk(contact.objectGuid, contact.versionNumber, section.Body, "data");
										//
										// Clear the data from the object before we put it into the db
										//
										contact.avatarData = null;
									}
									else
									{
										using (MemoryStream memoryStream = new MemoryStream((int)section.Body.Length))
										{
											section.Body.CopyTo(memoryStream);
											contact.avatarData = memoryStream.ToArray();
										}
									}
									//
									// Now add the change history
									//
									ContactChangeHistory contactChangeHistory = new ContactChangeHistory();
									contactChangeHistory.contactId = contact.id;
									contactChangeHistory.versionNumber = contact.versionNumber;
									contactChangeHistory.timeStamp = DateTime.UtcNow;
									contactChangeHistory.userId = securityUser.id;
									contactChangeHistory.tenantGuid = contact.tenantGuid;
									contactChangeHistory.data = JsonSerializer.Serialize(Database.Contact.CreateAnonymousWithFirstLevelSubObjects(contact));
									_context.ContactChangeHistories.Add(contactChangeHistory);

									_context.SaveChanges();

									transaction.Commit();

									CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Contact Data Uploaded with filename of " + fileName + " and with size of " + section.Body.Length, id.ToString());
								}
							}
							catch (Exception ex)
							{
								CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "Contact Data Upload Failed.", false, id.ToString(), "", "", ex);

								return Problem(ex.Message);
							}
						}


						//
						// Stop looking for more files.
						//
						break;
					}

					section = await reader.ReadNextSectionAsync();
				}
            }
            catch (Exception ex)
            {
                CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Caught error in UploadData handler", id.ToString(), ex);

                return Problem(ex.Message);
            }

            //
            // Treat the situation where we have a valid ID but no file content as a request to clear the data
            //
            if (foundFileData == false)
            {
                lock (contactPutSyncRoot)
                {
                    try
                    {
                        using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                        {
                            if (diskBasedBinaryStorageMode == true)
                            {
								DeleteDataFromDisk(contact.objectGuid, contact.versionNumber, "data");
                            }

                            contact.avatarFileName = null;
                            contact.avatarMimeType = null;
                            contact.avatarSize = 0;
                            contact.avatarData = null;
                            contact.versionNumber++;


                            //
                            // Now add the change history
                            //
                            ContactChangeHistory contactChangeHistory = new ContactChangeHistory();
                            contactChangeHistory.contactId = contact.id;
                            contactChangeHistory.versionNumber = contact.versionNumber;
                            contactChangeHistory.timeStamp = DateTime.UtcNow;
                            contactChangeHistory.userId = securityUser.id;
                                    contactChangeHistory.tenantGuid = contact.tenantGuid;
                                    contactChangeHistory.data = JsonSerializer.Serialize(Database.Contact.CreateAnonymousWithFirstLevelSubObjects(contact));
                            _context.ContactChangeHistories.Add(contactChangeHistory);

                            _context.SaveChanges();

                            transaction.Commit();

                            CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Contact data cleared.", id.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "Contact data clear failed.", false, id.ToString(), "", "", ex);

                        return Problem(ex.Message);
                    }
                }
            }

            return Ok();
        }

        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/Contact/Data/{id:int}")]
        public async Task<IActionResult> DownloadDataAsync(int id)
        {
             if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
             {
                 return Forbid();
             }


			using (SchedulerContext context = new SchedulerContext())
            {
                //
                // Return the data to the user as though it was a file.
                //
                Database.Contact contact = await (from d in context.Contacts
                                                where d.id == id &&
                                                d.active == true &&
                                                d.deleted == false
                                                select d).FirstOrDefaultAsync();

                if (contact != null && contact.avatarData != null)
                {
                   return File(contact.avatarData.ToArray<byte>(), contact.avatarMimeType, contact.avatarFileName != null ? contact.avatarFileName.Trim() : "Contact_" + contact.id.ToString(), true);
                }
                else
                {
                    return BadRequest();
                }
            }
        }
	}
}
