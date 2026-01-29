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
    /// This auto generated class provides the basic CRUD operations for the RelationshipType entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the RelationshipType entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class RelationshipTypesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private SchedulerContext _context;

		private ILogger<RelationshipTypesController> _logger;

		public RelationshipTypesController(SchedulerContext context, ILogger<RelationshipTypesController> logger) : base("Scheduler", "RelationshipType")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of RelationshipTypes filtered by the parameters provided.
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
		[Route("api/RelationshipTypes")]
		public async Task<IActionResult> GetRelationshipTypes(
			string name = null,
			string description = null,
			bool? isEmergencyEligible = null,
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

			IQueryable<Database.RelationshipType> query = (from rt in _context.RelationshipTypes select rt);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(rt => rt.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(rt => rt.description == description);
			}
			if (isEmergencyEligible.HasValue == true)
			{
				query = query.Where(rt => rt.isEmergencyEligible == isEmergencyEligible.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(rt => rt.sequence == sequence.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(rt => rt.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(rt => rt.color == color);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rt => rt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rt => rt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rt => rt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rt => rt.deleted == false);
				}
			}
			else
			{
				query = query.Where(rt => rt.active == true);
				query = query.Where(rt => rt.deleted == false);
			}

			query = query.OrderBy(rt => rt.sequence).ThenBy(rt => rt.name).ThenBy(rt => rt.description).ThenBy(rt => rt.color);

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
			// Add the any string contains parameter to span all the string fields on the Relationship Type, or on an any of the string fields on its immediate relations
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
			
			List<Database.RelationshipType> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.RelationshipType relationshipType in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(relationshipType, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.RelationshipType Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.RelationshipType Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of RelationshipTypes filtered by the parameters provided.  Its query is similar to the GetRelationshipTypes method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RelationshipTypes/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			bool? isEmergencyEligible = null,
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

			IQueryable<Database.RelationshipType> query = (from rt in _context.RelationshipTypes select rt);
			if (name != null)
			{
				query = query.Where(rt => rt.name == name);
			}
			if (description != null)
			{
				query = query.Where(rt => rt.description == description);
			}
			if (isEmergencyEligible.HasValue == true)
			{
				query = query.Where(rt => rt.isEmergencyEligible == isEmergencyEligible.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(rt => rt.sequence == sequence.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(rt => rt.iconId == iconId.Value);
			}
			if (color != null)
			{
				query = query.Where(rt => rt.color == color);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rt => rt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rt => rt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rt => rt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rt => rt.deleted == false);
				}
			}
			else
			{
				query = query.Where(rt => rt.active == true);
				query = query.Where(rt => rt.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Relationship Type, or on an any of the string fields on its immediate relations
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
        /// This gets a single RelationshipType by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RelationshipType/{id}")]
		public async Task<IActionResult> GetRelationshipType(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.RelationshipType> query = (from rt in _context.RelationshipTypes where
							(rt.id == id) &&
							(userIsAdmin == true || rt.deleted == false) &&
							(userIsWriter == true || rt.active == true)
					select rt);

				if (includeRelations == true)
				{
					query = query.Include(x => x.icon);
					query = query.AsSplitQuery();
				}

				Database.RelationshipType materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.RelationshipType Entity was read with Admin privilege." : "Scheduler.RelationshipType Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "RelationshipType", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.RelationshipType entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.RelationshipType.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.RelationshipType.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing RelationshipType record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/RelationshipType/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutRelationshipType(int id, [FromBody]Database.RelationshipType.RelationshipTypeDTO relationshipTypeDTO, CancellationToken cancellationToken = default)
		{
			if (relationshipTypeDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != relationshipTypeDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.RelationshipType> query = (from x in _context.RelationshipTypes
				where
				(x.id == id)
				select x);


			Database.RelationshipType existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.RelationshipType PUT", id.ToString(), new Exception("No Scheduler.RelationshipType entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (relationshipTypeDTO.objectGuid == Guid.Empty)
            {
                relationshipTypeDTO.objectGuid = existing.objectGuid;
            }
            else if (relationshipTypeDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a RelationshipType record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.RelationshipType cloneOfExisting = (Database.RelationshipType)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new RelationshipType object using the data from the existing record, updated with what is in the DTO.
			//
			Database.RelationshipType relationshipType = (Database.RelationshipType)_context.Entry(existing).GetDatabaseValues().ToObject();
			relationshipType.ApplyDTO(relationshipTypeDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (relationshipType.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.RelationshipType record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (relationshipType.name != null && relationshipType.name.Length > 100)
			{
				relationshipType.name = relationshipType.name.Substring(0, 100);
			}

			if (relationshipType.description != null && relationshipType.description.Length > 500)
			{
				relationshipType.description = relationshipType.description.Substring(0, 500);
			}

			if (relationshipType.color != null && relationshipType.color.Length > 10)
			{
				relationshipType.color = relationshipType.color.Substring(0, 10);
			}

			EntityEntry<Database.RelationshipType> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(relationshipType);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.RelationshipType entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.RelationshipType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.RelationshipType.CreateAnonymousWithFirstLevelSubObjects(relationshipType)),
					null);


				return Ok(Database.RelationshipType.CreateAnonymous(relationshipType));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.RelationshipType entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.RelationshipType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.RelationshipType.CreateAnonymousWithFirstLevelSubObjects(relationshipType)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new RelationshipType record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RelationshipType", Name = "RelationshipType")]
		public async Task<IActionResult> PostRelationshipType([FromBody]Database.RelationshipType.RelationshipTypeDTO relationshipTypeDTO, CancellationToken cancellationToken = default)
		{
			if (relationshipTypeDTO == null)
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
			// Create a new RelationshipType object using the data from the DTO
			//
			Database.RelationshipType relationshipType = Database.RelationshipType.FromDTO(relationshipTypeDTO);

			try
			{
				if (relationshipType.name != null && relationshipType.name.Length > 100)
				{
					relationshipType.name = relationshipType.name.Substring(0, 100);
				}

				if (relationshipType.description != null && relationshipType.description.Length > 500)
				{
					relationshipType.description = relationshipType.description.Substring(0, 500);
				}

				if (relationshipType.color != null && relationshipType.color.Length > 10)
				{
					relationshipType.color = relationshipType.color.Substring(0, 10);
				}

				relationshipType.objectGuid = Guid.NewGuid();
				_context.RelationshipTypes.Add(relationshipType);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.RelationshipType entity successfully created.",
					true,
					relationshipType.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.RelationshipType.CreateAnonymousWithFirstLevelSubObjects(relationshipType)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.RelationshipType entity creation failed.", false, relationshipType.id.ToString(), "", JsonSerializer.Serialize(relationshipType), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "RelationshipType", relationshipType.id, relationshipType.name));

			return CreatedAtRoute("RelationshipType", new { id = relationshipType.id }, Database.RelationshipType.CreateAnonymousWithFirstLevelSubObjects(relationshipType));
		}



        /// <summary>
        /// 
        /// This deletes a RelationshipType record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RelationshipType/{id}")]
		[Route("api/RelationshipType")]
		public async Task<IActionResult> DeleteRelationshipType(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.RelationshipType> query = (from x in _context.RelationshipTypes
				where
				(x.id == id)
				select x);


			Database.RelationshipType relationshipType = await query.FirstOrDefaultAsync(cancellationToken);

			if (relationshipType == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.RelationshipType DELETE", id.ToString(), new Exception("No Scheduler.RelationshipType entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.RelationshipType cloneOfExisting = (Database.RelationshipType)_context.Entry(relationshipType).GetDatabaseValues().ToObject();


			try
			{
				relationshipType.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.RelationshipType entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.RelationshipType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.RelationshipType.CreateAnonymousWithFirstLevelSubObjects(relationshipType)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.RelationshipType entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.RelationshipType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.RelationshipType.CreateAnonymousWithFirstLevelSubObjects(relationshipType)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of RelationshipType records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/RelationshipTypes/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			bool? isEmergencyEligible = null,
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

			IQueryable<Database.RelationshipType> query = (from rt in _context.RelationshipTypes select rt);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(rt => rt.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(rt => rt.description == description);
			}
			if (isEmergencyEligible.HasValue == true)
			{
				query = query.Where(rt => rt.isEmergencyEligible == isEmergencyEligible.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(rt => rt.sequence == sequence.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(rt => rt.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(rt => rt.color == color);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rt => rt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rt => rt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rt => rt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rt => rt.deleted == false);
				}
			}
			else
			{
				query = query.Where(rt => rt.active == true);
				query = query.Where(rt => rt.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Relationship Type, or on an any of the string fields on its immediate relations
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
			return Ok(await (from queryData in query select Database.RelationshipType.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/RelationshipType/CreateAuditEvent")]
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
