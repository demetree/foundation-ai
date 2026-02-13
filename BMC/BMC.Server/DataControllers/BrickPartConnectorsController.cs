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
    /// This auto generated class provides the basic CRUD operations for the BrickPartConnector entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the BrickPartConnector entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class BrickPartConnectorsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private BMCContext _context;

		private ILogger<BrickPartConnectorsController> _logger;

		public BrickPartConnectorsController(BMCContext context, ILogger<BrickPartConnectorsController> logger) : base("BMC", "BrickPartConnector")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of BrickPartConnectors filtered by the parameters provided.
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
		[Route("api/BrickPartConnectors")]
		public async Task<IActionResult> GetBrickPartConnectors(
			int? brickPartId = null,
			int? connectorTypeId = null,
			float? positionX = null,
			float? positionY = null,
			float? positionZ = null,
			float? orientationX = null,
			float? orientationY = null,
			float? orientationZ = null,
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

			IQueryable<Database.BrickPartConnector> query = (from bpc in _context.BrickPartConnectors select bpc);
			if (brickPartId.HasValue == true)
			{
				query = query.Where(bpc => bpc.brickPartId == brickPartId.Value);
			}
			if (connectorTypeId.HasValue == true)
			{
				query = query.Where(bpc => bpc.connectorTypeId == connectorTypeId.Value);
			}
			if (positionX.HasValue == true)
			{
				query = query.Where(bpc => bpc.positionX == positionX.Value);
			}
			if (positionY.HasValue == true)
			{
				query = query.Where(bpc => bpc.positionY == positionY.Value);
			}
			if (positionZ.HasValue == true)
			{
				query = query.Where(bpc => bpc.positionZ == positionZ.Value);
			}
			if (orientationX.HasValue == true)
			{
				query = query.Where(bpc => bpc.orientationX == orientationX.Value);
			}
			if (orientationY.HasValue == true)
			{
				query = query.Where(bpc => bpc.orientationY == orientationY.Value);
			}
			if (orientationZ.HasValue == true)
			{
				query = query.Where(bpc => bpc.orientationZ == orientationZ.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(bpc => bpc.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bpc => bpc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bpc => bpc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bpc => bpc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bpc => bpc.deleted == false);
				}
			}
			else
			{
				query = query.Where(bpc => bpc.active == true);
				query = query.Where(bpc => bpc.deleted == false);
			}

			query = query.OrderBy(bpc => bpc.sequence).ThenBy(bpc => bpc.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.brickPart);
				query = query.Include(x => x.connectorType);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Brick Part Connector, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       (includeRelations == true && x.brickPart.name.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.ldrawPartId.Contains(anyStringContains))
			       || (includeRelations == true && x.brickPart.geometryFilePath.Contains(anyStringContains))
			       || (includeRelations == true && x.connectorType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.connectorType.description.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.BrickPartConnector> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.BrickPartConnector brickPartConnector in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(brickPartConnector, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.BrickPartConnector Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.BrickPartConnector Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of BrickPartConnectors filtered by the parameters provided.  Its query is similar to the GetBrickPartConnectors method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickPartConnectors/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? brickPartId = null,
			int? connectorTypeId = null,
			float? positionX = null,
			float? positionY = null,
			float? positionZ = null,
			float? orientationX = null,
			float? orientationY = null,
			float? orientationZ = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.BrickPartConnector> query = (from bpc in _context.BrickPartConnectors select bpc);
			if (brickPartId.HasValue == true)
			{
				query = query.Where(bpc => bpc.brickPartId == brickPartId.Value);
			}
			if (connectorTypeId.HasValue == true)
			{
				query = query.Where(bpc => bpc.connectorTypeId == connectorTypeId.Value);
			}
			if (positionX.HasValue == true)
			{
				query = query.Where(bpc => bpc.positionX == positionX.Value);
			}
			if (positionY.HasValue == true)
			{
				query = query.Where(bpc => bpc.positionY == positionY.Value);
			}
			if (positionZ.HasValue == true)
			{
				query = query.Where(bpc => bpc.positionZ == positionZ.Value);
			}
			if (orientationX.HasValue == true)
			{
				query = query.Where(bpc => bpc.orientationX == orientationX.Value);
			}
			if (orientationY.HasValue == true)
			{
				query = query.Where(bpc => bpc.orientationY == orientationY.Value);
			}
			if (orientationZ.HasValue == true)
			{
				query = query.Where(bpc => bpc.orientationZ == orientationZ.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(bpc => bpc.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bpc => bpc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bpc => bpc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bpc => bpc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bpc => bpc.deleted == false);
				}
			}
			else
			{
				query = query.Where(bpc => bpc.active == true);
				query = query.Where(bpc => bpc.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Brick Part Connector, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.brickPart.name.Contains(anyStringContains)
			       || x.brickPart.ldrawPartId.Contains(anyStringContains)
			       || x.brickPart.geometryFilePath.Contains(anyStringContains)
			       || x.connectorType.name.Contains(anyStringContains)
			       || x.connectorType.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single BrickPartConnector by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickPartConnector/{id}")]
		public async Task<IActionResult> GetBrickPartConnector(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.BrickPartConnector> query = (from bpc in _context.BrickPartConnectors where
							(bpc.id == id) &&
							(userIsAdmin == true || bpc.deleted == false) &&
							(userIsWriter == true || bpc.active == true)
					select bpc);

				if (includeRelations == true)
				{
					query = query.Include(x => x.brickPart);
					query = query.Include(x => x.connectorType);
					query = query.AsSplitQuery();
				}

				Database.BrickPartConnector materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.BrickPartConnector Entity was read with Admin privilege." : "BMC.BrickPartConnector Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickPartConnector", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.BrickPartConnector entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.BrickPartConnector.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.BrickPartConnector.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing BrickPartConnector record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/BrickPartConnector/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutBrickPartConnector(int id, [FromBody]Database.BrickPartConnector.BrickPartConnectorDTO brickPartConnectorDTO, CancellationToken cancellationToken = default)
		{
			if (brickPartConnectorDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Catalog Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Catalog Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != brickPartConnectorDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.BrickPartConnector> query = (from x in _context.BrickPartConnectors
				where
				(x.id == id)
				select x);


			Database.BrickPartConnector existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickPartConnector PUT", id.ToString(), new Exception("No BMC.BrickPartConnector entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (brickPartConnectorDTO.objectGuid == Guid.Empty)
            {
                brickPartConnectorDTO.objectGuid = existing.objectGuid;
            }
            else if (brickPartConnectorDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a BrickPartConnector record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.BrickPartConnector cloneOfExisting = (Database.BrickPartConnector)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new BrickPartConnector object using the data from the existing record, updated with what is in the DTO.
			//
			Database.BrickPartConnector brickPartConnector = (Database.BrickPartConnector)_context.Entry(existing).GetDatabaseValues().ToObject();
			brickPartConnector.ApplyDTO(brickPartConnectorDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (brickPartConnector.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.BrickPartConnector record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			EntityEntry<Database.BrickPartConnector> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(brickPartConnector);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.BrickPartConnector entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickPartConnector.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickPartConnector.CreateAnonymousWithFirstLevelSubObjects(brickPartConnector)),
					null);


				return Ok(Database.BrickPartConnector.CreateAnonymous(brickPartConnector));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.BrickPartConnector entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickPartConnector.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickPartConnector.CreateAnonymousWithFirstLevelSubObjects(brickPartConnector)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new BrickPartConnector record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickPartConnector", Name = "BrickPartConnector")]
		public async Task<IActionResult> PostBrickPartConnector([FromBody]Database.BrickPartConnector.BrickPartConnectorDTO brickPartConnectorDTO, CancellationToken cancellationToken = default)
		{
			if (brickPartConnectorDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Catalog Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Catalog Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new BrickPartConnector object using the data from the DTO
			//
			Database.BrickPartConnector brickPartConnector = Database.BrickPartConnector.FromDTO(brickPartConnectorDTO);

			try
			{
				brickPartConnector.objectGuid = Guid.NewGuid();
				_context.BrickPartConnectors.Add(brickPartConnector);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.BrickPartConnector entity successfully created.",
					true,
					brickPartConnector.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.BrickPartConnector.CreateAnonymousWithFirstLevelSubObjects(brickPartConnector)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.BrickPartConnector entity creation failed.", false, brickPartConnector.id.ToString(), "", JsonSerializer.Serialize(brickPartConnector), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickPartConnector", brickPartConnector.id, brickPartConnector.id.ToString()));

			return CreatedAtRoute("BrickPartConnector", new { id = brickPartConnector.id }, Database.BrickPartConnector.CreateAnonymousWithFirstLevelSubObjects(brickPartConnector));
		}



        /// <summary>
        /// 
        /// This deletes a BrickPartConnector record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickPartConnector/{id}")]
		[Route("api/BrickPartConnector")]
		public async Task<IActionResult> DeleteBrickPartConnector(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Catalog Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Catalog Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.BrickPartConnector> query = (from x in _context.BrickPartConnectors
				where
				(x.id == id)
				select x);


			Database.BrickPartConnector brickPartConnector = await query.FirstOrDefaultAsync(cancellationToken);

			if (brickPartConnector == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickPartConnector DELETE", id.ToString(), new Exception("No BMC.BrickPartConnector entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.BrickPartConnector cloneOfExisting = (Database.BrickPartConnector)_context.Entry(brickPartConnector).GetDatabaseValues().ToObject();


			try
			{
				brickPartConnector.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BrickPartConnector entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickPartConnector.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickPartConnector.CreateAnonymousWithFirstLevelSubObjects(brickPartConnector)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BrickPartConnector entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickPartConnector.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickPartConnector.CreateAnonymousWithFirstLevelSubObjects(brickPartConnector)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of BrickPartConnector records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/BrickPartConnectors/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? brickPartId = null,
			int? connectorTypeId = null,
			float? positionX = null,
			float? positionY = null,
			float? positionZ = null,
			float? orientationX = null,
			float? orientationY = null,
			float? orientationZ = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);


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

			IQueryable<Database.BrickPartConnector> query = (from bpc in _context.BrickPartConnectors select bpc);
			if (brickPartId.HasValue == true)
			{
				query = query.Where(bpc => bpc.brickPartId == brickPartId.Value);
			}
			if (connectorTypeId.HasValue == true)
			{
				query = query.Where(bpc => bpc.connectorTypeId == connectorTypeId.Value);
			}
			if (positionX.HasValue == true)
			{
				query = query.Where(bpc => bpc.positionX == positionX.Value);
			}
			if (positionY.HasValue == true)
			{
				query = query.Where(bpc => bpc.positionY == positionY.Value);
			}
			if (positionZ.HasValue == true)
			{
				query = query.Where(bpc => bpc.positionZ == positionZ.Value);
			}
			if (orientationX.HasValue == true)
			{
				query = query.Where(bpc => bpc.orientationX == orientationX.Value);
			}
			if (orientationY.HasValue == true)
			{
				query = query.Where(bpc => bpc.orientationY == orientationY.Value);
			}
			if (orientationZ.HasValue == true)
			{
				query = query.Where(bpc => bpc.orientationZ == orientationZ.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(bpc => bpc.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bpc => bpc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bpc => bpc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bpc => bpc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bpc => bpc.deleted == false);
				}
			}
			else
			{
				query = query.Where(bpc => bpc.active == true);
				query = query.Where(bpc => bpc.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Brick Part Connector, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.brickPart.name.Contains(anyStringContains)
			       || x.brickPart.ldrawPartId.Contains(anyStringContains)
			       || x.brickPart.geometryFilePath.Contains(anyStringContains)
			       || x.connectorType.name.Contains(anyStringContains)
			       || x.connectorType.description.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.BrickPartConnector.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/BrickPartConnector/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Catalog Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Catalog Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
