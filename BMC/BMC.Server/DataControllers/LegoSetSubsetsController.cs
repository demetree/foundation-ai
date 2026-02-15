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
    /// This auto generated class provides the basic CRUD operations for the LegoSetSubset entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the LegoSetSubset entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class LegoSetSubsetsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<LegoSetSubsetsController> _logger;

		public LegoSetSubsetsController(BMCContext context, ILogger<LegoSetSubsetsController> logger) : base("BMC", "LegoSetSubset")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of LegoSetSubsets filtered by the parameters provided.
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
		[Route("api/LegoSetSubsets")]
		public async Task<IActionResult> GetLegoSetSubsets(
			int? parentLegoSetId = null,
			int? childLegoSetId = null,
			int? quantity = null,
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

			IQueryable<Database.LegoSetSubset> query = (from lss in _context.LegoSetSubsets select lss);
			if (parentLegoSetId.HasValue == true)
			{
				query = query.Where(lss => lss.parentLegoSetId == parentLegoSetId.Value);
			}
			if (childLegoSetId.HasValue == true)
			{
				query = query.Where(lss => lss.childLegoSetId == childLegoSetId.Value);
			}
			if (quantity.HasValue == true)
			{
				query = query.Where(lss => lss.quantity == quantity.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(lss => lss.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(lss => lss.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(lss => lss.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(lss => lss.deleted == false);
				}
			}
			else
			{
				query = query.Where(lss => lss.active == true);
				query = query.Where(lss => lss.deleted == false);
			}

			query = query.OrderBy(lss => lss.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.childLegoSet);
				query = query.Include(x => x.parentLegoSet);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Lego Set Subset, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       (includeRelations == true && x.childLegoSet.name.Contains(anyStringContains))
			       || (includeRelations == true && x.childLegoSet.setNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.childLegoSet.imageUrl.Contains(anyStringContains))
			       || (includeRelations == true && x.childLegoSet.brickLinkUrl.Contains(anyStringContains))
			       || (includeRelations == true && x.childLegoSet.rebrickableUrl.Contains(anyStringContains))
			       || (includeRelations == true && x.parentLegoSet.name.Contains(anyStringContains))
			       || (includeRelations == true && x.parentLegoSet.setNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.parentLegoSet.imageUrl.Contains(anyStringContains))
			       || (includeRelations == true && x.parentLegoSet.brickLinkUrl.Contains(anyStringContains))
			       || (includeRelations == true && x.parentLegoSet.rebrickableUrl.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.LegoSetSubset> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.LegoSetSubset legoSetSubset in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(legoSetSubset, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.LegoSetSubset Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.LegoSetSubset Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of LegoSetSubsets filtered by the parameters provided.  Its query is similar to the GetLegoSetSubsets method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LegoSetSubsets/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? parentLegoSetId = null,
			int? childLegoSetId = null,
			int? quantity = null,
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

			IQueryable<Database.LegoSetSubset> query = (from lss in _context.LegoSetSubsets select lss);
			if (parentLegoSetId.HasValue == true)
			{
				query = query.Where(lss => lss.parentLegoSetId == parentLegoSetId.Value);
			}
			if (childLegoSetId.HasValue == true)
			{
				query = query.Where(lss => lss.childLegoSetId == childLegoSetId.Value);
			}
			if (quantity.HasValue == true)
			{
				query = query.Where(lss => lss.quantity == quantity.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(lss => lss.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(lss => lss.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(lss => lss.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(lss => lss.deleted == false);
				}
			}
			else
			{
				query = query.Where(lss => lss.active == true);
				query = query.Where(lss => lss.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Lego Set Subset, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.childLegoSet.name.Contains(anyStringContains)
			       || x.childLegoSet.setNumber.Contains(anyStringContains)
			       || x.childLegoSet.imageUrl.Contains(anyStringContains)
			       || x.childLegoSet.brickLinkUrl.Contains(anyStringContains)
			       || x.childLegoSet.rebrickableUrl.Contains(anyStringContains)
			       || x.parentLegoSet.name.Contains(anyStringContains)
			       || x.parentLegoSet.setNumber.Contains(anyStringContains)
			       || x.parentLegoSet.imageUrl.Contains(anyStringContains)
			       || x.parentLegoSet.brickLinkUrl.Contains(anyStringContains)
			       || x.parentLegoSet.rebrickableUrl.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single LegoSetSubset by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LegoSetSubset/{id}")]
		public async Task<IActionResult> GetLegoSetSubset(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.LegoSetSubset> query = (from lss in _context.LegoSetSubsets where
							(lss.id == id) &&
							(userIsAdmin == true || lss.deleted == false) &&
							(userIsWriter == true || lss.active == true)
					select lss);

				if (includeRelations == true)
				{
					query = query.Include(x => x.childLegoSet);
					query = query.Include(x => x.parentLegoSet);
					query = query.AsSplitQuery();
				}

				Database.LegoSetSubset materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.LegoSetSubset Entity was read with Admin privilege." : "BMC.LegoSetSubset Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "LegoSetSubset", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.LegoSetSubset entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.LegoSetSubset.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.LegoSetSubset.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing LegoSetSubset record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/LegoSetSubset/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutLegoSetSubset(int id, [FromBody]Database.LegoSetSubset.LegoSetSubsetDTO legoSetSubsetDTO, CancellationToken cancellationToken = default)
		{
			if (legoSetSubsetDTO == null)
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



			if (id != legoSetSubsetDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.LegoSetSubset> query = (from x in _context.LegoSetSubsets
				where
				(x.id == id)
				select x);


			Database.LegoSetSubset existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.LegoSetSubset PUT", id.ToString(), new Exception("No BMC.LegoSetSubset entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (legoSetSubsetDTO.objectGuid == Guid.Empty)
            {
                legoSetSubsetDTO.objectGuid = existing.objectGuid;
            }
            else if (legoSetSubsetDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a LegoSetSubset record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.LegoSetSubset cloneOfExisting = (Database.LegoSetSubset)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new LegoSetSubset object using the data from the existing record, updated with what is in the DTO.
			//
			Database.LegoSetSubset legoSetSubset = (Database.LegoSetSubset)_context.Entry(existing).GetDatabaseValues().ToObject();
			legoSetSubset.ApplyDTO(legoSetSubsetDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (legoSetSubset.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.LegoSetSubset record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			EntityEntry<Database.LegoSetSubset> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(legoSetSubset);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.LegoSetSubset entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.LegoSetSubset.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LegoSetSubset.CreateAnonymousWithFirstLevelSubObjects(legoSetSubset)),
					null);


				return Ok(Database.LegoSetSubset.CreateAnonymous(legoSetSubset));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.LegoSetSubset entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.LegoSetSubset.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LegoSetSubset.CreateAnonymousWithFirstLevelSubObjects(legoSetSubset)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new LegoSetSubset record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LegoSetSubset", Name = "LegoSetSubset")]
		public async Task<IActionResult> PostLegoSetSubset([FromBody]Database.LegoSetSubset.LegoSetSubsetDTO legoSetSubsetDTO, CancellationToken cancellationToken = default)
		{
			if (legoSetSubsetDTO == null)
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
			// Create a new LegoSetSubset object using the data from the DTO
			//
			Database.LegoSetSubset legoSetSubset = Database.LegoSetSubset.FromDTO(legoSetSubsetDTO);

			try
			{
				legoSetSubset.objectGuid = Guid.NewGuid();
				_context.LegoSetSubsets.Add(legoSetSubset);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.LegoSetSubset entity successfully created.",
					true,
					legoSetSubset.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.LegoSetSubset.CreateAnonymousWithFirstLevelSubObjects(legoSetSubset)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.LegoSetSubset entity creation failed.", false, legoSetSubset.id.ToString(), "", JsonSerializer.Serialize(legoSetSubset), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "LegoSetSubset", legoSetSubset.id, legoSetSubset.id.ToString()));

			return CreatedAtRoute("LegoSetSubset", new { id = legoSetSubset.id }, Database.LegoSetSubset.CreateAnonymousWithFirstLevelSubObjects(legoSetSubset));
		}



        /// <summary>
        /// 
        /// This deletes a LegoSetSubset record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LegoSetSubset/{id}")]
		[Route("api/LegoSetSubset")]
		public async Task<IActionResult> DeleteLegoSetSubset(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.LegoSetSubset> query = (from x in _context.LegoSetSubsets
				where
				(x.id == id)
				select x);


			Database.LegoSetSubset legoSetSubset = await query.FirstOrDefaultAsync(cancellationToken);

			if (legoSetSubset == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.LegoSetSubset DELETE", id.ToString(), new Exception("No BMC.LegoSetSubset entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.LegoSetSubset cloneOfExisting = (Database.LegoSetSubset)_context.Entry(legoSetSubset).GetDatabaseValues().ToObject();


			try
			{
				legoSetSubset.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.LegoSetSubset entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.LegoSetSubset.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LegoSetSubset.CreateAnonymousWithFirstLevelSubObjects(legoSetSubset)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.LegoSetSubset entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.LegoSetSubset.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LegoSetSubset.CreateAnonymousWithFirstLevelSubObjects(legoSetSubset)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of LegoSetSubset records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/LegoSetSubsets/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? parentLegoSetId = null,
			int? childLegoSetId = null,
			int? quantity = null,
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

			IQueryable<Database.LegoSetSubset> query = (from lss in _context.LegoSetSubsets select lss);
			if (parentLegoSetId.HasValue == true)
			{
				query = query.Where(lss => lss.parentLegoSetId == parentLegoSetId.Value);
			}
			if (childLegoSetId.HasValue == true)
			{
				query = query.Where(lss => lss.childLegoSetId == childLegoSetId.Value);
			}
			if (quantity.HasValue == true)
			{
				query = query.Where(lss => lss.quantity == quantity.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(lss => lss.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(lss => lss.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(lss => lss.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(lss => lss.deleted == false);
				}
			}
			else
			{
				query = query.Where(lss => lss.active == true);
				query = query.Where(lss => lss.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Lego Set Subset, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.childLegoSet.name.Contains(anyStringContains)
			       || x.childLegoSet.setNumber.Contains(anyStringContains)
			       || x.childLegoSet.imageUrl.Contains(anyStringContains)
			       || x.childLegoSet.brickLinkUrl.Contains(anyStringContains)
			       || x.childLegoSet.rebrickableUrl.Contains(anyStringContains)
			       || x.parentLegoSet.name.Contains(anyStringContains)
			       || x.parentLegoSet.setNumber.Contains(anyStringContains)
			       || x.parentLegoSet.imageUrl.Contains(anyStringContains)
			       || x.parentLegoSet.brickLinkUrl.Contains(anyStringContains)
			       || x.parentLegoSet.rebrickableUrl.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.LegoSetSubset.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/LegoSetSubset/CreateAuditEvent")]
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
