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
using Foundation.Community.Database;

namespace Foundation.Community.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the ContactSubmission entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ContactSubmission entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ContactSubmissionsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 100;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		private CommunityContext _context;

		private ILogger<ContactSubmissionsController> _logger;

		public ContactSubmissionsController(CommunityContext context, ILogger<ContactSubmissionsController> logger) : base("Community", "ContactSubmission")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ContactSubmissions filtered by the parameters provided.
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
		[Route("api/ContactSubmissions")]
		public async Task<IActionResult> GetContactSubmissions(
			string name = null,
			string email = null,
			string subject = null,
			string message = null,
			DateTime? submittedDate = null,
			bool? isRead = null,
			bool? isArchived = null,
			string adminNotes = null,
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
			// Community Admin role needed to read from this table, or Community Administrator role.  Note we do not check the user's read permission level here.  Role membership is the key to read access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (submittedDate.HasValue == true && submittedDate.Value.Kind != DateTimeKind.Utc)
			{
				submittedDate = submittedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ContactSubmission> query = (from cs in _context.ContactSubmissions select cs);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(cs => cs.name == name);
			}
			if (string.IsNullOrEmpty(email) == false)
			{
				query = query.Where(cs => cs.email == email);
			}
			if (string.IsNullOrEmpty(subject) == false)
			{
				query = query.Where(cs => cs.subject == subject);
			}
			if (string.IsNullOrEmpty(message) == false)
			{
				query = query.Where(cs => cs.message == message);
			}
			if (submittedDate.HasValue == true)
			{
				query = query.Where(cs => cs.submittedDate == submittedDate.Value);
			}
			if (isRead.HasValue == true)
			{
				query = query.Where(cs => cs.isRead == isRead.Value);
			}
			if (isArchived.HasValue == true)
			{
				query = query.Where(cs => cs.isArchived == isArchived.Value);
			}
			if (string.IsNullOrEmpty(adminNotes) == false)
			{
				query = query.Where(cs => cs.adminNotes == adminNotes);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cs => cs.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cs => cs.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cs => cs.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cs => cs.deleted == false);
				}
			}
			else
			{
				query = query.Where(cs => cs.active == true);
				query = query.Where(cs => cs.deleted == false);
			}

			query = query.OrderBy(cs => cs.name).ThenBy(cs => cs.email).ThenBy(cs => cs.subject);


			//
			// Add the any string contains parameter to span all the string fields on the Contact Submission, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.email.Contains(anyStringContains)
			       || x.subject.Contains(anyStringContains)
			       || x.message.Contains(anyStringContains)
			       || x.adminNotes.Contains(anyStringContains)
			   );
			}

			if (includeRelations == true)
			{
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.ContactSubmission> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ContactSubmission contactSubmission in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(contactSubmission, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Community.ContactSubmission Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Community.ContactSubmission Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ContactSubmissions filtered by the parameters provided.  Its query is similar to the GetContactSubmissions method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContactSubmissions/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string email = null,
			string subject = null,
			string message = null,
			DateTime? submittedDate = null,
			bool? isRead = null,
			bool? isArchived = null,
			string adminNotes = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Community Admin role needed to read from this table, or Community Administrator role.  Note we do not check the user's read permission level here.  Role membership is the key to read access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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


			//
			// Fix any non-UTC date parameters that come in.
			//
			if (submittedDate.HasValue == true && submittedDate.Value.Kind != DateTimeKind.Utc)
			{
				submittedDate = submittedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ContactSubmission> query = (from cs in _context.ContactSubmissions select cs);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(cs => cs.name == name);
			}
			if (email != null)
			{
				query = query.Where(cs => cs.email == email);
			}
			if (subject != null)
			{
				query = query.Where(cs => cs.subject == subject);
			}
			if (message != null)
			{
				query = query.Where(cs => cs.message == message);
			}
			if (submittedDate.HasValue == true)
			{
				query = query.Where(cs => cs.submittedDate == submittedDate.Value);
			}
			if (isRead.HasValue == true)
			{
				query = query.Where(cs => cs.isRead == isRead.Value);
			}
			if (isArchived.HasValue == true)
			{
				query = query.Where(cs => cs.isArchived == isArchived.Value);
			}
			if (adminNotes != null)
			{
				query = query.Where(cs => cs.adminNotes == adminNotes);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cs => cs.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cs => cs.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cs => cs.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cs => cs.deleted == false);
				}
			}
			else
			{
				query = query.Where(cs => cs.active == true);
				query = query.Where(cs => cs.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Contact Submission, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.email.Contains(anyStringContains)
			       || x.subject.Contains(anyStringContains)
			       || x.message.Contains(anyStringContains)
			       || x.adminNotes.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ContactSubmission by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContactSubmission/{id}")]
		public async Task<IActionResult> GetContactSubmission(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Community Admin role needed to read from this table, or Community Administrator role.  Note we do not check the user's read permission level here.  Role membership is the key to read access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
				IQueryable<Database.ContactSubmission> query = (from cs in _context.ContactSubmissions where
							(cs.id == id) &&
							(userIsAdmin == true || cs.deleted == false) &&
							(userIsWriter == true || cs.active == true)
					select cs);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.ContactSubmission materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Community.ContactSubmission Entity was read with Admin privilege." : "Community.ContactSubmission Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ContactSubmission", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Community.ContactSubmission entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Community.ContactSubmission.   Entity was read with Admin privilege." : "Exception caught during entity read of Community.ContactSubmission.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ContactSubmission record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ContactSubmission/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutContactSubmission(int id, [FromBody]Database.ContactSubmission.ContactSubmissionDTO contactSubmissionDTO, CancellationToken cancellationToken = default)
		{
			if (contactSubmissionDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Community Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != contactSubmissionDTO.id)
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


			IQueryable<Database.ContactSubmission> query = (from x in _context.ContactSubmissions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ContactSubmission existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.ContactSubmission PUT", id.ToString(), new Exception("No Community.ContactSubmission entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (contactSubmissionDTO.objectGuid == Guid.Empty)
            {
                contactSubmissionDTO.objectGuid = existing.objectGuid;
            }
            else if (contactSubmissionDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ContactSubmission record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ContactSubmission cloneOfExisting = (Database.ContactSubmission)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ContactSubmission object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ContactSubmission contactSubmission = (Database.ContactSubmission)_context.Entry(existing).GetDatabaseValues().ToObject();
			contactSubmission.ApplyDTO(contactSubmissionDTO);
			//
			// The tenant guid for any ContactSubmission being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ContactSubmission because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				contactSubmission.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (contactSubmission.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Community.ContactSubmission record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (contactSubmission.name != null && contactSubmission.name.Length > 100)
			{
				contactSubmission.name = contactSubmission.name.Substring(0, 100);
			}

			if (contactSubmission.email != null && contactSubmission.email.Length > 250)
			{
				contactSubmission.email = contactSubmission.email.Substring(0, 250);
			}

			if (contactSubmission.subject != null && contactSubmission.subject.Length > 250)
			{
				contactSubmission.subject = contactSubmission.subject.Substring(0, 250);
			}

			if (contactSubmission.submittedDate.Kind != DateTimeKind.Utc)
			{
				contactSubmission.submittedDate = contactSubmission.submittedDate.ToUniversalTime();
			}

			if (contactSubmission.adminNotes != null && contactSubmission.adminNotes.Length > 500)
			{
				contactSubmission.adminNotes = contactSubmission.adminNotes.Substring(0, 500);
			}

			EntityEntry<Database.ContactSubmission> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(contactSubmission);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Community.ContactSubmission entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ContactSubmission.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ContactSubmission.CreateAnonymousWithFirstLevelSubObjects(contactSubmission)),
					null);


				return Ok(Database.ContactSubmission.CreateAnonymous(contactSubmission));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Community.ContactSubmission entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ContactSubmission.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ContactSubmission.CreateAnonymousWithFirstLevelSubObjects(contactSubmission)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new ContactSubmission record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContactSubmission", Name = "ContactSubmission")]
		public async Task<IActionResult> PostContactSubmission([FromBody]Database.ContactSubmission.ContactSubmissionDTO contactSubmissionDTO, CancellationToken cancellationToken = default)
		{
			if (contactSubmissionDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Community Writer role needed to write to this table, as well as the minimum write permission level.
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
			// Create a new ContactSubmission object using the data from the DTO
			//
			Database.ContactSubmission contactSubmission = Database.ContactSubmission.FromDTO(contactSubmissionDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				contactSubmission.tenantGuid = userTenantGuid;

				if (contactSubmission.name != null && contactSubmission.name.Length > 100)
				{
					contactSubmission.name = contactSubmission.name.Substring(0, 100);
				}

				if (contactSubmission.email != null && contactSubmission.email.Length > 250)
				{
					contactSubmission.email = contactSubmission.email.Substring(0, 250);
				}

				if (contactSubmission.subject != null && contactSubmission.subject.Length > 250)
				{
					contactSubmission.subject = contactSubmission.subject.Substring(0, 250);
				}

				if (contactSubmission.submittedDate.Kind != DateTimeKind.Utc)
				{
					contactSubmission.submittedDate = contactSubmission.submittedDate.ToUniversalTime();
				}

				if (contactSubmission.adminNotes != null && contactSubmission.adminNotes.Length > 500)
				{
					contactSubmission.adminNotes = contactSubmission.adminNotes.Substring(0, 500);
				}

				contactSubmission.objectGuid = Guid.NewGuid();
				_context.ContactSubmissions.Add(contactSubmission);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Community.ContactSubmission entity successfully created.",
					true,
					contactSubmission.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ContactSubmission.CreateAnonymousWithFirstLevelSubObjects(contactSubmission)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Community.ContactSubmission entity creation failed.", false, contactSubmission.id.ToString(), "", JsonSerializer.Serialize(contactSubmission), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ContactSubmission", contactSubmission.id, contactSubmission.name));

			return CreatedAtRoute("ContactSubmission", new { id = contactSubmission.id }, Database.ContactSubmission.CreateAnonymousWithFirstLevelSubObjects(contactSubmission));
		}



        /// <summary>
        /// 
        /// This deletes a ContactSubmission record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContactSubmission/{id}")]
		[Route("api/ContactSubmission")]
		public async Task<IActionResult> DeleteContactSubmission(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Community Writer role needed to write to this table, as well as the minimum write permission level.
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

			IQueryable<Database.ContactSubmission> query = (from x in _context.ContactSubmissions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ContactSubmission contactSubmission = await query.FirstOrDefaultAsync(cancellationToken);

			if (contactSubmission == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.ContactSubmission DELETE", id.ToString(), new Exception("No Community.ContactSubmission entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ContactSubmission cloneOfExisting = (Database.ContactSubmission)_context.Entry(contactSubmission).GetDatabaseValues().ToObject();


			try
			{
				contactSubmission.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Community.ContactSubmission entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ContactSubmission.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ContactSubmission.CreateAnonymousWithFirstLevelSubObjects(contactSubmission)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Community.ContactSubmission entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ContactSubmission.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ContactSubmission.CreateAnonymousWithFirstLevelSubObjects(contactSubmission)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of ContactSubmission records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ContactSubmissions/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string email = null,
			string subject = null,
			string message = null,
			DateTime? submittedDate = null,
			bool? isRead = null,
			bool? isArchived = null,
			string adminNotes = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Community Admin role needed to read from this table, or Community Administrator role.  Note we do not check the user's read permission level here.  Role membership is the key to read access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (submittedDate.HasValue == true && submittedDate.Value.Kind != DateTimeKind.Utc)
			{
				submittedDate = submittedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ContactSubmission> query = (from cs in _context.ContactSubmissions select cs);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(cs => cs.name == name);
			}
			if (string.IsNullOrEmpty(email) == false)
			{
				query = query.Where(cs => cs.email == email);
			}
			if (string.IsNullOrEmpty(subject) == false)
			{
				query = query.Where(cs => cs.subject == subject);
			}
			if (string.IsNullOrEmpty(message) == false)
			{
				query = query.Where(cs => cs.message == message);
			}
			if (submittedDate.HasValue == true)
			{
				query = query.Where(cs => cs.submittedDate == submittedDate.Value);
			}
			if (isRead.HasValue == true)
			{
				query = query.Where(cs => cs.isRead == isRead.Value);
			}
			if (isArchived.HasValue == true)
			{
				query = query.Where(cs => cs.isArchived == isArchived.Value);
			}
			if (string.IsNullOrEmpty(adminNotes) == false)
			{
				query = query.Where(cs => cs.adminNotes == adminNotes);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cs => cs.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cs => cs.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cs => cs.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cs => cs.deleted == false);
				}
			}
			else
			{
				query = query.Where(cs => cs.active == true);
				query = query.Where(cs => cs.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Contact Submission, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.email.Contains(anyStringContains)
			       || x.subject.Contains(anyStringContains)
			       || x.message.Contains(anyStringContains)
			       || x.adminNotes.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name).ThenBy(x => x.email).ThenBy(x => x.subject);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ContactSubmission.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ContactSubmission/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Community Writer role needed to write to this table, as well as the minimum write permission level.
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
