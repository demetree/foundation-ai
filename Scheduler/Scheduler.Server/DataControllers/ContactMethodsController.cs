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
    /// This auto generated class provides the basic CRUD operations for the ContactMethod entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ContactMethod entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ContactMethodsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private SchedulerContext _context;

		private ILogger<ContactMethodsController> _logger;

		public ContactMethodsController(SchedulerContext context, ILogger<ContactMethodsController> logger) : base("Scheduler", "ContactMethod")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ContactMethods filtered by the parameters provided.
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
		[Route("api/ContactMethods")]
		public async Task<IActionResult> GetContactMethods(
			string name = null,
			string description = null,
			int? sequence = null,
			int? iconId = null,
			string color = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

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

			IQueryable<Database.ContactMethod> query = (from cm in _context.ContactMethods select cm);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(cm => cm.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(cm => cm.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(cm => cm.sequence == sequence.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(cm => cm.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(cm => cm.color == color);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cm => cm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cm => cm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cm => cm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cm => cm.deleted == false);
				}
			}
			else
			{
				query = query.Where(cm => cm.active == true);
				query = query.Where(cm => cm.deleted == false);
			}

			query = query.OrderBy(cm => cm.sequence).ThenBy(cm => cm.name).ThenBy(cm => cm.description).ThenBy(cm => cm.color);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.icon);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Contact Method, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || (includeRelations == true && x.icon.name.Contains(anyStringContains))
			       || (includeRelations == true && x.icon.fontAwesomeCode.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.ContactMethod> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ContactMethod contactMethod in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(contactMethod, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ContactMethod Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ContactMethod Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ContactMethods filtered by the parameters provided.  Its query is similar to the GetContactMethods method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContactMethods/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			int? sequence = null,
			int? iconId = null,
			string color = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.ContactMethod> query = (from cm in _context.ContactMethods select cm);
			if (name != null)
			{
				query = query.Where(cm => cm.name == name);
			}
			if (description != null)
			{
				query = query.Where(cm => cm.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(cm => cm.sequence == sequence.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(cm => cm.iconId == iconId.Value);
			}
			if (color != null)
			{
				query = query.Where(cm => cm.color == color);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cm => cm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cm => cm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cm => cm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cm => cm.deleted == false);
				}
			}
			else
			{
				query = query.Where(cm => cm.active == true);
				query = query.Where(cm => cm.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Contact Method, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.icon.name.Contains(anyStringContains)
			       || x.icon.fontAwesomeCode.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ContactMethod by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContactMethod/{id}")]
		public async Task<IActionResult> GetContactMethod(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.ContactMethod> query = (from cm in _context.ContactMethods where
							(cm.id == id) &&
							(userIsAdmin == true || cm.deleted == false) &&
							(userIsWriter == true || cm.active == true)
					select cm);

				if (includeRelations == true)
				{
					query = query.Include(x => x.icon);
					query = query.AsSplitQuery();
				}

				Database.ContactMethod materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.ContactMethod Entity was read with Admin privilege." : "Scheduler.ContactMethod Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ContactMethod", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.ContactMethod entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.ContactMethod.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.ContactMethod.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ContactMethod record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ContactMethod/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutContactMethod(int id, [FromBody]Database.ContactMethod.ContactMethodDTO contactMethodDTO, CancellationToken cancellationToken = default)
		{
			if (contactMethodDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != contactMethodDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.ContactMethod> query = (from x in _context.ContactMethods
				where
				(x.id == id)
				select x);


			Database.ContactMethod existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ContactMethod PUT", id.ToString(), new Exception("No Scheduler.ContactMethod entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (contactMethodDTO.objectGuid == Guid.Empty)
            {
                contactMethodDTO.objectGuid = existing.objectGuid;
            }
            else if (contactMethodDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ContactMethod record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ContactMethod cloneOfExisting = (Database.ContactMethod)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ContactMethod object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ContactMethod contactMethod = (Database.ContactMethod)_context.Entry(existing).GetDatabaseValues().ToObject();
			contactMethod.ApplyDTO(contactMethodDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (contactMethod.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.ContactMethod record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (contactMethod.name != null && contactMethod.name.Length > 100)
			{
				contactMethod.name = contactMethod.name.Substring(0, 100);
			}

			if (contactMethod.description != null && contactMethod.description.Length > 500)
			{
				contactMethod.description = contactMethod.description.Substring(0, 500);
			}

			if (contactMethod.color != null && contactMethod.color.Length > 10)
			{
				contactMethod.color = contactMethod.color.Substring(0, 10);
			}

			EntityEntry<Database.ContactMethod> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(contactMethod);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.ContactMethod entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ContactMethod.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ContactMethod.CreateAnonymousWithFirstLevelSubObjects(contactMethod)),
					null);


				return Ok(Database.ContactMethod.CreateAnonymous(contactMethod));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.ContactMethod entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ContactMethod.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ContactMethod.CreateAnonymousWithFirstLevelSubObjects(contactMethod)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new ContactMethod record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContactMethod", Name = "ContactMethod")]
		public async Task<IActionResult> PostContactMethod([FromBody]Database.ContactMethod.ContactMethodDTO contactMethodDTO, CancellationToken cancellationToken = default)
		{
			if (contactMethodDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new ContactMethod object using the data from the DTO
			//
			Database.ContactMethod contactMethod = Database.ContactMethod.FromDTO(contactMethodDTO);

			try
			{
				if (contactMethod.name != null && contactMethod.name.Length > 100)
				{
					contactMethod.name = contactMethod.name.Substring(0, 100);
				}

				if (contactMethod.description != null && contactMethod.description.Length > 500)
				{
					contactMethod.description = contactMethod.description.Substring(0, 500);
				}

				if (contactMethod.color != null && contactMethod.color.Length > 10)
				{
					contactMethod.color = contactMethod.color.Substring(0, 10);
				}

				contactMethod.objectGuid = Guid.NewGuid();
				_context.ContactMethods.Add(contactMethod);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.ContactMethod entity successfully created.",
					true,
					contactMethod.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ContactMethod.CreateAnonymousWithFirstLevelSubObjects(contactMethod)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.ContactMethod entity creation failed.", false, contactMethod.id.ToString(), "", JsonSerializer.Serialize(contactMethod), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ContactMethod", contactMethod.id, contactMethod.name));

			return CreatedAtRoute("ContactMethod", new { id = contactMethod.id }, Database.ContactMethod.CreateAnonymousWithFirstLevelSubObjects(contactMethod));
		}



        /// <summary>
        /// 
        /// This deletes a ContactMethod record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContactMethod/{id}")]
		[Route("api/ContactMethod")]
		public async Task<IActionResult> DeleteContactMethod(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.ContactMethod> query = (from x in _context.ContactMethods
				where
				(x.id == id)
				select x);


			Database.ContactMethod contactMethod = await query.FirstOrDefaultAsync(cancellationToken);

			if (contactMethod == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ContactMethod DELETE", id.ToString(), new Exception("No Scheduler.ContactMethod entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ContactMethod cloneOfExisting = (Database.ContactMethod)_context.Entry(contactMethod).GetDatabaseValues().ToObject();


			try
			{
				contactMethod.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.ContactMethod entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ContactMethod.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ContactMethod.CreateAnonymousWithFirstLevelSubObjects(contactMethod)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.ContactMethod entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ContactMethod.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ContactMethod.CreateAnonymousWithFirstLevelSubObjects(contactMethod)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of ContactMethod records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ContactMethods/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			int? sequence = null,
			int? iconId = null,
			string color = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);

			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);

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

			IQueryable<Database.ContactMethod> query = (from cm in _context.ContactMethods select cm);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(cm => cm.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(cm => cm.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(cm => cm.sequence == sequence.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(cm => cm.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(cm => cm.color == color);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cm => cm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cm => cm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cm => cm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cm => cm.deleted == false);
				}
			}
			else
			{
				query = query.Where(cm => cm.active == true);
				query = query.Where(cm => cm.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Contact Method, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.icon.name.Contains(anyStringContains)
			       || x.icon.fontAwesomeCode.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.color);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ContactMethod.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ContactMethod/CreateAuditEvent")]
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
