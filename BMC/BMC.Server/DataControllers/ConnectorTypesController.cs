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
using Foundation.BMC.Database;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the ConnectorType entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ConnectorType entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ConnectorTypesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<ConnectorTypesController> _logger;

		public ConnectorTypesController(BMCContext context, ILogger<ConnectorTypesController> logger) : base("BMC", "ConnectorType")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ConnectorTypes filtered by the parameters provided.
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
		[Route("api/ConnectorTypes")]
		public async Task<IActionResult> GetConnectorTypes(
			string name = null,
			string description = null,
			int? degreesOfFreedom = null,
			bool? allowsRotation = null,
			bool? allowsSlide = null,
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
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			IQueryable<Database.ConnectorType> query = (from ct in _context.ConnectorTypes select ct);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ct => ct.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ct => ct.description == description);
			}
			if (degreesOfFreedom.HasValue == true)
			{
				query = query.Where(ct => ct.degreesOfFreedom == degreesOfFreedom.Value);
			}
			if (allowsRotation.HasValue == true)
			{
				query = query.Where(ct => ct.allowsRotation == allowsRotation.Value);
			}
			if (allowsSlide.HasValue == true)
			{
				query = query.Where(ct => ct.allowsSlide == allowsSlide.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(ct => ct.sequence == sequence.Value);
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

			query = query.OrderBy(ct => ct.sequence).ThenBy(ct => ct.name).ThenBy(ct => ct.description);


			//
			// Add the any string contains parameter to span all the string fields on the Connector Type, or on an any of the string fields on its immediate relations
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
			
			List<Database.ConnectorType> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ConnectorType connectorType in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(connectorType, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.ConnectorType Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.ConnectorType Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ConnectorTypes filtered by the parameters provided.  Its query is similar to the GetConnectorTypes method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConnectorTypes/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			int? degreesOfFreedom = null,
			bool? allowsRotation = null,
			bool? allowsSlide = null,
			int? sequence = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.ConnectorType> query = (from ct in _context.ConnectorTypes select ct);
			if (name != null)
			{
				query = query.Where(ct => ct.name == name);
			}
			if (description != null)
			{
				query = query.Where(ct => ct.description == description);
			}
			if (degreesOfFreedom.HasValue == true)
			{
				query = query.Where(ct => ct.degreesOfFreedom == degreesOfFreedom.Value);
			}
			if (allowsRotation.HasValue == true)
			{
				query = query.Where(ct => ct.allowsRotation == allowsRotation.Value);
			}
			if (allowsSlide.HasValue == true)
			{
				query = query.Where(ct => ct.allowsSlide == allowsSlide.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(ct => ct.sequence == sequence.Value);
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
			// Add the any string contains parameter to span all the string fields on the Connector Type, or on an any of the string fields on its immediate relations
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
        /// This gets a single ConnectorType by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConnectorType/{id}")]
		public async Task<IActionResult> GetConnectorType(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
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
				IQueryable<Database.ConnectorType> query = (from ct in _context.ConnectorTypes where
							(ct.id == id) &&
							(userIsAdmin == true || ct.deleted == false) &&
							(userIsWriter == true || ct.active == true)
					select ct);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.ConnectorType materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.ConnectorType Entity was read with Admin privilege." : "BMC.ConnectorType Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ConnectorType", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.ConnectorType entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.ConnectorType.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.ConnectorType.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ConnectorType record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ConnectorType/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutConnectorType(int id, [FromBody]Database.ConnectorType.ConnectorTypeDTO connectorTypeDTO, CancellationToken cancellationToken = default)
		{
			if (connectorTypeDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != connectorTypeDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.ConnectorType> query = (from x in _context.ConnectorTypes
				where
				(x.id == id)
				select x);


			Database.ConnectorType existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ConnectorType PUT", id.ToString(), new Exception("No BMC.ConnectorType entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (connectorTypeDTO.objectGuid == Guid.Empty)
            {
                connectorTypeDTO.objectGuid = existing.objectGuid;
            }
            else if (connectorTypeDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ConnectorType record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ConnectorType cloneOfExisting = (Database.ConnectorType)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ConnectorType object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ConnectorType connectorType = (Database.ConnectorType)_context.Entry(existing).GetDatabaseValues().ToObject();
			connectorType.ApplyDTO(connectorTypeDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (connectorType.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.ConnectorType record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (connectorType.name != null && connectorType.name.Length > 100)
			{
				connectorType.name = connectorType.name.Substring(0, 100);
			}

			if (connectorType.description != null && connectorType.description.Length > 500)
			{
				connectorType.description = connectorType.description.Substring(0, 500);
			}

			EntityEntry<Database.ConnectorType> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(connectorType);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.ConnectorType entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ConnectorType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ConnectorType.CreateAnonymousWithFirstLevelSubObjects(connectorType)),
					null);


				return Ok(Database.ConnectorType.CreateAnonymous(connectorType));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.ConnectorType entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ConnectorType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ConnectorType.CreateAnonymousWithFirstLevelSubObjects(connectorType)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new ConnectorType record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConnectorType", Name = "ConnectorType")]
		public async Task<IActionResult> PostConnectorType([FromBody]Database.ConnectorType.ConnectorTypeDTO connectorTypeDTO, CancellationToken cancellationToken = default)
		{
			if (connectorTypeDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new ConnectorType object using the data from the DTO
			//
			Database.ConnectorType connectorType = Database.ConnectorType.FromDTO(connectorTypeDTO);

			try
			{
				if (connectorType.name != null && connectorType.name.Length > 100)
				{
					connectorType.name = connectorType.name.Substring(0, 100);
				}

				if (connectorType.description != null && connectorType.description.Length > 500)
				{
					connectorType.description = connectorType.description.Substring(0, 500);
				}

				connectorType.objectGuid = Guid.NewGuid();
				_context.ConnectorTypes.Add(connectorType);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.ConnectorType entity successfully created.",
					true,
					connectorType.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ConnectorType.CreateAnonymousWithFirstLevelSubObjects(connectorType)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.ConnectorType entity creation failed.", false, connectorType.id.ToString(), "", JsonSerializer.Serialize(connectorType), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ConnectorType", connectorType.id, connectorType.name));

			return CreatedAtRoute("ConnectorType", new { id = connectorType.id }, Database.ConnectorType.CreateAnonymousWithFirstLevelSubObjects(connectorType));
		}



        /// <summary>
        /// 
        /// This deletes a ConnectorType record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConnectorType/{id}")]
		[Route("api/ConnectorType")]
		public async Task<IActionResult> DeleteConnectorType(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.ConnectorType> query = (from x in _context.ConnectorTypes
				where
				(x.id == id)
				select x);


			Database.ConnectorType connectorType = await query.FirstOrDefaultAsync(cancellationToken);

			if (connectorType == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ConnectorType DELETE", id.ToString(), new Exception("No BMC.ConnectorType entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ConnectorType cloneOfExisting = (Database.ConnectorType)_context.Entry(connectorType).GetDatabaseValues().ToObject();


			try
			{
				connectorType.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ConnectorType entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ConnectorType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ConnectorType.CreateAnonymousWithFirstLevelSubObjects(connectorType)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ConnectorType entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ConnectorType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ConnectorType.CreateAnonymousWithFirstLevelSubObjects(connectorType)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of ConnectorType records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ConnectorTypes/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			int? degreesOfFreedom = null,
			bool? allowsRotation = null,
			bool? allowsSlide = null,
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
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			IQueryable<Database.ConnectorType> query = (from ct in _context.ConnectorTypes select ct);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ct => ct.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ct => ct.description == description);
			}
			if (degreesOfFreedom.HasValue == true)
			{
				query = query.Where(ct => ct.degreesOfFreedom == degreesOfFreedom.Value);
			}
			if (allowsRotation.HasValue == true)
			{
				query = query.Where(ct => ct.allowsRotation == allowsRotation.Value);
			}
			if (allowsSlide.HasValue == true)
			{
				query = query.Where(ct => ct.allowsSlide == allowsSlide.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(ct => ct.sequence == sequence.Value);
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
			// Add the any string contains parameter to span all the string fields on the Connector Type, or on an any of the string fields on its immediate relations
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
			return Ok(await (from queryData in query select Database.ConnectorType.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ConnectorType/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
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
