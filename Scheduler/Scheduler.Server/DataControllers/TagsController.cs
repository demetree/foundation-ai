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
    /// This auto generated class provides the basic CRUD operations for the Tag entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Tag entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class TagsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SchedulerContext _context;

		private ILogger<TagsController> _logger;

		public TagsController(SchedulerContext context, ILogger<TagsController> logger) : base("Scheduler", "Tag")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Tags filtered by the parameters provided.
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
		[Route("api/Tags")]
		public async Task<IActionResult> GetTags(
			string name = null,
			string description = null,
			int? sequence = null,
			bool? isSystem = null,
			int? priorityId = null,
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

			IQueryable<Database.Tag> query = (from t in _context.Tags select t);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(t => t.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(t => t.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(t => t.sequence == sequence.Value);
			}
			if (isSystem.HasValue == true)
			{
				query = query.Where(t => t.isSystem == isSystem.Value);
			}
			if (priorityId.HasValue == true)
			{
				query = query.Where(t => t.priorityId == priorityId.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(t => t.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(t => t.color == color);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(t => t.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(t => t.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(t => t.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(t => t.deleted == false);
				}
			}
			else
			{
				query = query.Where(t => t.active == true);
				query = query.Where(t => t.deleted == false);
			}

			query = query.OrderBy(t => t.sequence).ThenBy(t => t.name).ThenBy(t => t.description).ThenBy(t => t.color);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.icon);
				query = query.Include(x => x.priority);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Tag, or on an any of the string fields on its immediate relations
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
			       || (includeRelations == true && x.priority.name.Contains(anyStringContains))
			       || (includeRelations == true && x.priority.description.Contains(anyStringContains))
			       || (includeRelations == true && x.priority.color.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.Tag> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Tag tag in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(tag, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.Tag Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.Tag Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Tags filtered by the parameters provided.  Its query is similar to the GetTags method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Tags/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			int? sequence = null,
			bool? isSystem = null,
			int? priorityId = null,
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


			IQueryable<Database.Tag> query = (from t in _context.Tags select t);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(t => t.name == name);
			}
			if (description != null)
			{
				query = query.Where(t => t.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(t => t.sequence == sequence.Value);
			}
			if (isSystem.HasValue == true)
			{
				query = query.Where(t => t.isSystem == isSystem.Value);
			}
			if (priorityId.HasValue == true)
			{
				query = query.Where(t => t.priorityId == priorityId.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(t => t.iconId == iconId.Value);
			}
			if (color != null)
			{
				query = query.Where(t => t.color == color);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(t => t.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(t => t.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(t => t.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(t => t.deleted == false);
				}
			}
			else
			{
				query = query.Where(t => t.active == true);
				query = query.Where(t => t.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Tag, or on an any of the string fields on its immediate relations
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
			       || x.priority.name.Contains(anyStringContains)
			       || x.priority.description.Contains(anyStringContains)
			       || x.priority.color.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single Tag by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Tag/{id}")]
		public async Task<IActionResult> GetTag(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.Tag> query = (from t in _context.Tags where
							(t.id == id) &&
							(userIsAdmin == true || t.deleted == false) &&
							(userIsWriter == true || t.active == true)
					select t);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.icon);
					query = query.Include(x => x.priority);
					query = query.AsSplitQuery();
				}

				Database.Tag materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.Tag Entity was read with Admin privilege." : "Scheduler.Tag Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Tag", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.Tag entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.Tag.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.Tag.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing Tag record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Tag/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutTag(int id, [FromBody]Database.Tag.TagDTO tagDTO, CancellationToken cancellationToken = default)
		{
			if (tagDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != tagDTO.id)
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


			IQueryable<Database.Tag> query = (from x in _context.Tags
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Tag existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Tag PUT", id.ToString(), new Exception("No Scheduler.Tag entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (tagDTO.objectGuid == Guid.Empty)
            {
                tagDTO.objectGuid = existing.objectGuid;
            }
            else if (tagDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Tag record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Tag cloneOfExisting = (Database.Tag)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Tag object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Tag tag = (Database.Tag)_context.Entry(existing).GetDatabaseValues().ToObject();
			tag.ApplyDTO(tagDTO);
			//
			// The tenant guid for any Tag being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the Tag because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				tag.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (tag.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.Tag record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (tag.name != null && tag.name.Length > 100)
			{
				tag.name = tag.name.Substring(0, 100);
			}

			if (tag.description != null && tag.description.Length > 500)
			{
				tag.description = tag.description.Substring(0, 500);
			}

			if (tag.color != null && tag.color.Length > 10)
			{
				tag.color = tag.color.Substring(0, 10);
			}

			EntityEntry<Database.Tag> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(tag);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.Tag entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.Tag.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.Tag.CreateAnonymousWithFirstLevelSubObjects(tag)),
					null);


				return Ok(Database.Tag.CreateAnonymous(tag));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.Tag entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.Tag.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.Tag.CreateAnonymousWithFirstLevelSubObjects(tag)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new Tag record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Tag", Name = "Tag")]
		public async Task<IActionResult> PostTag([FromBody]Database.Tag.TagDTO tagDTO, CancellationToken cancellationToken = default)
		{
			if (tagDTO == null)
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
			// Create a new Tag object using the data from the DTO
			//
			Database.Tag tag = Database.Tag.FromDTO(tagDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				tag.tenantGuid = userTenantGuid;

				if (tag.name != null && tag.name.Length > 100)
				{
					tag.name = tag.name.Substring(0, 100);
				}

				if (tag.description != null && tag.description.Length > 500)
				{
					tag.description = tag.description.Substring(0, 500);
				}

				if (tag.color != null && tag.color.Length > 10)
				{
					tag.color = tag.color.Substring(0, 10);
				}

				tag.objectGuid = Guid.NewGuid();
				_context.Tags.Add(tag);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.Tag entity successfully created.",
					true,
					tag.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.Tag.CreateAnonymousWithFirstLevelSubObjects(tag)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.Tag entity creation failed.", false, tag.id.ToString(), "", JsonSerializer.Serialize(tag), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Tag", tag.id, tag.name));

			return CreatedAtRoute("Tag", new { id = tag.id }, Database.Tag.CreateAnonymousWithFirstLevelSubObjects(tag));
		}



        /// <summary>
        /// 
        /// This deletes a Tag record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Tag/{id}")]
		[Route("api/Tag")]
		public async Task<IActionResult> DeleteTag(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.Tag> query = (from x in _context.Tags
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Tag tag = await query.FirstOrDefaultAsync(cancellationToken);

			if (tag == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Tag DELETE", id.ToString(), new Exception("No Scheduler.Tag entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Tag cloneOfExisting = (Database.Tag)_context.Entry(tag).GetDatabaseValues().ToObject();


			try
			{
				tag.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.Tag entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.Tag.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.Tag.CreateAnonymousWithFirstLevelSubObjects(tag)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.Tag entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.Tag.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.Tag.CreateAnonymousWithFirstLevelSubObjects(tag)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of Tag records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Tags/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			int? sequence = null,
			bool? isSystem = null,
			int? priorityId = null,
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

			IQueryable<Database.Tag> query = (from t in _context.Tags select t);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(t => t.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(t => t.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(t => t.sequence == sequence.Value);
			}
			if (isSystem.HasValue == true)
			{
				query = query.Where(t => t.isSystem == isSystem.Value);
			}
			if (priorityId.HasValue == true)
			{
				query = query.Where(t => t.priorityId == priorityId.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(t => t.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(t => t.color == color);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(t => t.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(t => t.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(t => t.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(t => t.deleted == false);
				}
			}
			else
			{
				query = query.Where(t => t.active == true);
				query = query.Where(t => t.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Tag, or on an any of the string fields on its immediate relations
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
			       || x.priority.name.Contains(anyStringContains)
			       || x.priority.description.Contains(anyStringContains)
			       || x.priority.color.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.color);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Tag.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Tag/CreateAuditEvent")]
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
