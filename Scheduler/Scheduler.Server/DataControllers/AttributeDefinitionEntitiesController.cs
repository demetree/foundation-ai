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
    /// This auto generated class provides the basic CRUD operations for the AttributeDefinitionEntity entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the AttributeDefinitionEntity entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class AttributeDefinitionEntitiesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SchedulerContext _context;

		private ILogger<AttributeDefinitionEntitiesController> _logger;

		public AttributeDefinitionEntitiesController(SchedulerContext context, ILogger<AttributeDefinitionEntitiesController> logger) : base("Scheduler", "AttributeDefinitionEntity")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of AttributeDefinitionEntities filtered by the parameters provided.
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
		[Route("api/AttributeDefinitionEntities")]
		public async Task<IActionResult> GetAttributeDefinitionEntities(
			string name = null,
			string description = null,
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

			IQueryable<Database.AttributeDefinitionEntity> query = (from ade in _context.AttributeDefinitionEntities select ade);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ade => ade.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ade => ade.description == description);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ade => ade.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ade => ade.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ade => ade.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ade => ade.deleted == false);
				}
			}
			else
			{
				query = query.Where(ade => ade.active == true);
				query = query.Where(ade => ade.deleted == false);
			}

			query = query.OrderBy(ade => ade.name).ThenBy(ade => ade.description);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Attribute Definition Entity, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.AttributeDefinitionEntity> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.AttributeDefinitionEntity attributeDefinitionEntity in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(attributeDefinitionEntity, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.AttributeDefinitionEntity Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.AttributeDefinitionEntity Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of AttributeDefinitionEntities filtered by the parameters provided.  Its query is similar to the GetAttributeDefinitionEntities method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AttributeDefinitionEntities/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
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

			IQueryable<Database.AttributeDefinitionEntity> query = (from ade in _context.AttributeDefinitionEntities select ade);
			if (name != null)
			{
				query = query.Where(ade => ade.name == name);
			}
			if (description != null)
			{
				query = query.Where(ade => ade.description == description);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ade => ade.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ade => ade.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ade => ade.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ade => ade.deleted == false);
				}
			}
			else
			{
				query = query.Where(ade => ade.active == true);
				query = query.Where(ade => ade.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Attribute Definition Entity, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single AttributeDefinitionEntity by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AttributeDefinitionEntity/{id}")]
		public async Task<IActionResult> GetAttributeDefinitionEntity(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.AttributeDefinitionEntity> query = (from ade in _context.AttributeDefinitionEntities where
							(ade.id == id) &&
							(userIsAdmin == true || ade.deleted == false) &&
							(userIsWriter == true || ade.active == true)
					select ade);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.AttributeDefinitionEntity materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.AttributeDefinitionEntity Entity was read with Admin privilege." : "Scheduler.AttributeDefinitionEntity Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AttributeDefinitionEntity", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.AttributeDefinitionEntity entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.AttributeDefinitionEntity.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.AttributeDefinitionEntity.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing AttributeDefinitionEntity record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/AttributeDefinitionEntity/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutAttributeDefinitionEntity(int id, [FromBody]Database.AttributeDefinitionEntity.AttributeDefinitionEntityDTO attributeDefinitionEntityDTO, CancellationToken cancellationToken = default)
		{
			if (attributeDefinitionEntityDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != attributeDefinitionEntityDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.AttributeDefinitionEntity> query = (from x in _context.AttributeDefinitionEntities
				where
				(x.id == id)
				select x);


			Database.AttributeDefinitionEntity existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.AttributeDefinitionEntity PUT", id.ToString(), new Exception("No Scheduler.AttributeDefinitionEntity entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (attributeDefinitionEntityDTO.objectGuid == Guid.Empty)
            {
                attributeDefinitionEntityDTO.objectGuid = existing.objectGuid;
            }
            else if (attributeDefinitionEntityDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a AttributeDefinitionEntity record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.AttributeDefinitionEntity cloneOfExisting = (Database.AttributeDefinitionEntity)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new AttributeDefinitionEntity object using the data from the existing record, updated with what is in the DTO.
			//
			Database.AttributeDefinitionEntity attributeDefinitionEntity = (Database.AttributeDefinitionEntity)_context.Entry(existing).GetDatabaseValues().ToObject();
			attributeDefinitionEntity.ApplyDTO(attributeDefinitionEntityDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (attributeDefinitionEntity.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.AttributeDefinitionEntity record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (attributeDefinitionEntity.name != null && attributeDefinitionEntity.name.Length > 100)
			{
				attributeDefinitionEntity.name = attributeDefinitionEntity.name.Substring(0, 100);
			}

			if (attributeDefinitionEntity.description != null && attributeDefinitionEntity.description.Length > 500)
			{
				attributeDefinitionEntity.description = attributeDefinitionEntity.description.Substring(0, 500);
			}

			EntityEntry<Database.AttributeDefinitionEntity> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(attributeDefinitionEntity);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.AttributeDefinitionEntity entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AttributeDefinitionEntity.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AttributeDefinitionEntity.CreateAnonymousWithFirstLevelSubObjects(attributeDefinitionEntity)),
					null);


				return Ok(Database.AttributeDefinitionEntity.CreateAnonymous(attributeDefinitionEntity));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.AttributeDefinitionEntity entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AttributeDefinitionEntity.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AttributeDefinitionEntity.CreateAnonymousWithFirstLevelSubObjects(attributeDefinitionEntity)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new AttributeDefinitionEntity record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AttributeDefinitionEntity", Name = "AttributeDefinitionEntity")]
		public async Task<IActionResult> PostAttributeDefinitionEntity([FromBody]Database.AttributeDefinitionEntity.AttributeDefinitionEntityDTO attributeDefinitionEntityDTO, CancellationToken cancellationToken = default)
		{
			if (attributeDefinitionEntityDTO == null)
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
			// Create a new AttributeDefinitionEntity object using the data from the DTO
			//
			Database.AttributeDefinitionEntity attributeDefinitionEntity = Database.AttributeDefinitionEntity.FromDTO(attributeDefinitionEntityDTO);

			try
			{
				if (attributeDefinitionEntity.name != null && attributeDefinitionEntity.name.Length > 100)
				{
					attributeDefinitionEntity.name = attributeDefinitionEntity.name.Substring(0, 100);
				}

				if (attributeDefinitionEntity.description != null && attributeDefinitionEntity.description.Length > 500)
				{
					attributeDefinitionEntity.description = attributeDefinitionEntity.description.Substring(0, 500);
				}

				attributeDefinitionEntity.objectGuid = Guid.NewGuid();
				_context.AttributeDefinitionEntities.Add(attributeDefinitionEntity);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.AttributeDefinitionEntity entity successfully created.",
					true,
					attributeDefinitionEntity.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.AttributeDefinitionEntity.CreateAnonymousWithFirstLevelSubObjects(attributeDefinitionEntity)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.AttributeDefinitionEntity entity creation failed.", false, attributeDefinitionEntity.id.ToString(), "", JsonSerializer.Serialize(attributeDefinitionEntity), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AttributeDefinitionEntity", attributeDefinitionEntity.id, attributeDefinitionEntity.name));

			return CreatedAtRoute("AttributeDefinitionEntity", new { id = attributeDefinitionEntity.id }, Database.AttributeDefinitionEntity.CreateAnonymousWithFirstLevelSubObjects(attributeDefinitionEntity));
		}



        /// <summary>
        /// 
        /// This deletes a AttributeDefinitionEntity record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AttributeDefinitionEntity/{id}")]
		[Route("api/AttributeDefinitionEntity")]
		public async Task<IActionResult> DeleteAttributeDefinitionEntity(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.AttributeDefinitionEntity> query = (from x in _context.AttributeDefinitionEntities
				where
				(x.id == id)
				select x);


			Database.AttributeDefinitionEntity attributeDefinitionEntity = await query.FirstOrDefaultAsync(cancellationToken);

			if (attributeDefinitionEntity == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.AttributeDefinitionEntity DELETE", id.ToString(), new Exception("No Scheduler.AttributeDefinitionEntity entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.AttributeDefinitionEntity cloneOfExisting = (Database.AttributeDefinitionEntity)_context.Entry(attributeDefinitionEntity).GetDatabaseValues().ToObject();


			try
			{
				attributeDefinitionEntity.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.AttributeDefinitionEntity entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AttributeDefinitionEntity.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AttributeDefinitionEntity.CreateAnonymousWithFirstLevelSubObjects(attributeDefinitionEntity)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.AttributeDefinitionEntity entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AttributeDefinitionEntity.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AttributeDefinitionEntity.CreateAnonymousWithFirstLevelSubObjects(attributeDefinitionEntity)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of AttributeDefinitionEntity records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/AttributeDefinitionEntities/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
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

			IQueryable<Database.AttributeDefinitionEntity> query = (from ade in _context.AttributeDefinitionEntities select ade);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ade => ade.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ade => ade.description == description);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ade => ade.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ade => ade.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ade => ade.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ade => ade.deleted == false);
				}
			}
			else
			{
				query = query.Where(ade => ade.active == true);
				query = query.Where(ade => ade.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Attribute Definition Entity, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.name).ThenBy(x => x.description);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.AttributeDefinitionEntity.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/AttributeDefinitionEntity/CreateAuditEvent")]
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
