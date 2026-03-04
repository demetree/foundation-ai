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
    /// This auto generated class provides the basic CRUD operations for the AttributeDefinitionType entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the AttributeDefinitionType entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class AttributeDefinitionTypesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private SchedulerContext _context;

		private ILogger<AttributeDefinitionTypesController> _logger;

		public AttributeDefinitionTypesController(SchedulerContext context, ILogger<AttributeDefinitionTypesController> logger) : base("Scheduler", "AttributeDefinitionType")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of AttributeDefinitionTypes filtered by the parameters provided.
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
		[Route("api/AttributeDefinitionTypes")]
		public async Task<IActionResult> GetAttributeDefinitionTypes(
			string name = null,
			string description = null,
			int? sequence = null,
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

			IQueryable<Database.AttributeDefinitionType> query = (from adt in _context.AttributeDefinitionTypes select adt);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(adt => adt.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(adt => adt.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(adt => adt.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(adt => adt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(adt => adt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(adt => adt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(adt => adt.deleted == false);
				}
			}
			else
			{
				query = query.Where(adt => adt.active == true);
				query = query.Where(adt => adt.deleted == false);
			}

			query = query.OrderBy(adt => adt.sequence).ThenBy(adt => adt.name).ThenBy(adt => adt.description);


			//
			// Add the any string contains parameter to span all the string fields on the Attribute Definition Type, or on an any of the string fields on its immediate relations
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
			
			List<Database.AttributeDefinitionType> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.AttributeDefinitionType attributeDefinitionType in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(attributeDefinitionType, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.AttributeDefinitionType Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.AttributeDefinitionType Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of AttributeDefinitionTypes filtered by the parameters provided.  Its query is similar to the GetAttributeDefinitionTypes method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AttributeDefinitionTypes/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			int? sequence = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.AttributeDefinitionType> query = (from adt in _context.AttributeDefinitionTypes select adt);
			if (name != null)
			{
				query = query.Where(adt => adt.name == name);
			}
			if (description != null)
			{
				query = query.Where(adt => adt.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(adt => adt.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(adt => adt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(adt => adt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(adt => adt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(adt => adt.deleted == false);
				}
			}
			else
			{
				query = query.Where(adt => adt.active == true);
				query = query.Where(adt => adt.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Attribute Definition Type, or on an any of the string fields on its immediate relations
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
        /// This gets a single AttributeDefinitionType by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AttributeDefinitionType/{id}")]
		public async Task<IActionResult> GetAttributeDefinitionType(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.AttributeDefinitionType> query = (from adt in _context.AttributeDefinitionTypes where
							(adt.id == id) &&
							(userIsAdmin == true || adt.deleted == false) &&
							(userIsWriter == true || adt.active == true)
					select adt);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.AttributeDefinitionType materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.AttributeDefinitionType Entity was read with Admin privilege." : "Scheduler.AttributeDefinitionType Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AttributeDefinitionType", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.AttributeDefinitionType entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.AttributeDefinitionType.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.AttributeDefinitionType.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing AttributeDefinitionType record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/AttributeDefinitionType/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutAttributeDefinitionType(int id, [FromBody]Database.AttributeDefinitionType.AttributeDefinitionTypeDTO attributeDefinitionTypeDTO, CancellationToken cancellationToken = default)
		{
			if (attributeDefinitionTypeDTO == null)
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



			if (id != attributeDefinitionTypeDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.AttributeDefinitionType> query = (from x in _context.AttributeDefinitionTypes
				where
				(x.id == id)
				select x);


			Database.AttributeDefinitionType existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.AttributeDefinitionType PUT", id.ToString(), new Exception("No Scheduler.AttributeDefinitionType entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (attributeDefinitionTypeDTO.objectGuid == Guid.Empty)
            {
                attributeDefinitionTypeDTO.objectGuid = existing.objectGuid;
            }
            else if (attributeDefinitionTypeDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a AttributeDefinitionType record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.AttributeDefinitionType cloneOfExisting = (Database.AttributeDefinitionType)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new AttributeDefinitionType object using the data from the existing record, updated with what is in the DTO.
			//
			Database.AttributeDefinitionType attributeDefinitionType = (Database.AttributeDefinitionType)_context.Entry(existing).GetDatabaseValues().ToObject();
			attributeDefinitionType.ApplyDTO(attributeDefinitionTypeDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (attributeDefinitionType.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.AttributeDefinitionType record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (attributeDefinitionType.name != null && attributeDefinitionType.name.Length > 100)
			{
				attributeDefinitionType.name = attributeDefinitionType.name.Substring(0, 100);
			}

			if (attributeDefinitionType.description != null && attributeDefinitionType.description.Length > 500)
			{
				attributeDefinitionType.description = attributeDefinitionType.description.Substring(0, 500);
			}

			EntityEntry<Database.AttributeDefinitionType> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(attributeDefinitionType);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.AttributeDefinitionType entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AttributeDefinitionType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AttributeDefinitionType.CreateAnonymousWithFirstLevelSubObjects(attributeDefinitionType)),
					null);


				return Ok(Database.AttributeDefinitionType.CreateAnonymous(attributeDefinitionType));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.AttributeDefinitionType entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AttributeDefinitionType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AttributeDefinitionType.CreateAnonymousWithFirstLevelSubObjects(attributeDefinitionType)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new AttributeDefinitionType record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AttributeDefinitionType", Name = "AttributeDefinitionType")]
		public async Task<IActionResult> PostAttributeDefinitionType([FromBody]Database.AttributeDefinitionType.AttributeDefinitionTypeDTO attributeDefinitionTypeDTO, CancellationToken cancellationToken = default)
		{
			if (attributeDefinitionTypeDTO == null)
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

			//
			// Create a new AttributeDefinitionType object using the data from the DTO
			//
			Database.AttributeDefinitionType attributeDefinitionType = Database.AttributeDefinitionType.FromDTO(attributeDefinitionTypeDTO);

			try
			{
				if (attributeDefinitionType.name != null && attributeDefinitionType.name.Length > 100)
				{
					attributeDefinitionType.name = attributeDefinitionType.name.Substring(0, 100);
				}

				if (attributeDefinitionType.description != null && attributeDefinitionType.description.Length > 500)
				{
					attributeDefinitionType.description = attributeDefinitionType.description.Substring(0, 500);
				}

				attributeDefinitionType.objectGuid = Guid.NewGuid();
				_context.AttributeDefinitionTypes.Add(attributeDefinitionType);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.AttributeDefinitionType entity successfully created.",
					true,
					attributeDefinitionType.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.AttributeDefinitionType.CreateAnonymousWithFirstLevelSubObjects(attributeDefinitionType)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.AttributeDefinitionType entity creation failed.", false, attributeDefinitionType.id.ToString(), "", JsonSerializer.Serialize(attributeDefinitionType), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AttributeDefinitionType", attributeDefinitionType.id, attributeDefinitionType.name));

			return CreatedAtRoute("AttributeDefinitionType", new { id = attributeDefinitionType.id }, Database.AttributeDefinitionType.CreateAnonymousWithFirstLevelSubObjects(attributeDefinitionType));
		}



        /// <summary>
        /// 
        /// This deletes a AttributeDefinitionType record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AttributeDefinitionType/{id}")]
		[Route("api/AttributeDefinitionType")]
		public async Task<IActionResult> DeleteAttributeDefinitionType(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.AttributeDefinitionType> query = (from x in _context.AttributeDefinitionTypes
				where
				(x.id == id)
				select x);


			Database.AttributeDefinitionType attributeDefinitionType = await query.FirstOrDefaultAsync(cancellationToken);

			if (attributeDefinitionType == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.AttributeDefinitionType DELETE", id.ToString(), new Exception("No Scheduler.AttributeDefinitionType entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.AttributeDefinitionType cloneOfExisting = (Database.AttributeDefinitionType)_context.Entry(attributeDefinitionType).GetDatabaseValues().ToObject();


			try
			{
				attributeDefinitionType.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.AttributeDefinitionType entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AttributeDefinitionType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AttributeDefinitionType.CreateAnonymousWithFirstLevelSubObjects(attributeDefinitionType)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.AttributeDefinitionType entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AttributeDefinitionType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AttributeDefinitionType.CreateAnonymousWithFirstLevelSubObjects(attributeDefinitionType)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of AttributeDefinitionType records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/AttributeDefinitionTypes/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			int? sequence = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);


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

			IQueryable<Database.AttributeDefinitionType> query = (from adt in _context.AttributeDefinitionTypes select adt);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(adt => adt.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(adt => adt.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(adt => adt.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(adt => adt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(adt => adt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(adt => adt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(adt => adt.deleted == false);
				}
			}
			else
			{
				query = query.Where(adt => adt.active == true);
				query = query.Where(adt => adt.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Attribute Definition Type, or on an any of the string fields on its immediate relations
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


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.AttributeDefinitionType.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/AttributeDefinitionType/CreateAuditEvent")]
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
