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
    /// This auto generated class provides the basic CRUD operations for the LegoSet entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the LegoSet entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class LegoSetsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<LegoSetsController> _logger;

		public LegoSetsController(BMCContext context, ILogger<LegoSetsController> logger) : base("BMC", "LegoSet")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of LegoSets filtered by the parameters provided.
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
		[Route("api/LegoSets")]
		public async Task<IActionResult> GetLegoSets(
			string name = null,
			string setNumber = null,
			int? year = null,
			int? partCount = null,
			int? legoThemeId = null,
			string imageUrl = null,
			string brickLinkUrl = null,
			string rebrickableUrl = null,
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

			IQueryable<Database.LegoSet> query = (from ls in _context.LegoSets select ls);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ls => ls.name == name);
			}
			if (string.IsNullOrEmpty(setNumber) == false)
			{
				query = query.Where(ls => ls.setNumber == setNumber);
			}
			if (year.HasValue == true)
			{
				query = query.Where(ls => ls.year == year.Value);
			}
			if (partCount.HasValue == true)
			{
				query = query.Where(ls => ls.partCount == partCount.Value);
			}
			if (legoThemeId.HasValue == true)
			{
				query = query.Where(ls => ls.legoThemeId == legoThemeId.Value);
			}
			if (string.IsNullOrEmpty(imageUrl) == false)
			{
				query = query.Where(ls => ls.imageUrl == imageUrl);
			}
			if (string.IsNullOrEmpty(brickLinkUrl) == false)
			{
				query = query.Where(ls => ls.brickLinkUrl == brickLinkUrl);
			}
			if (string.IsNullOrEmpty(rebrickableUrl) == false)
			{
				query = query.Where(ls => ls.rebrickableUrl == rebrickableUrl);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ls => ls.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ls => ls.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ls => ls.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ls => ls.deleted == false);
				}
			}
			else
			{
				query = query.Where(ls => ls.active == true);
				query = query.Where(ls => ls.deleted == false);
			}

			query = query.OrderBy(ls => ls.name).ThenBy(ls => ls.setNumber).ThenBy(ls => ls.imageUrl);


			//
			// Add the any string contains parameter to span all the string fields on the Lego Set, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.setNumber.Contains(anyStringContains)
			       || x.imageUrl.Contains(anyStringContains)
			       || x.brickLinkUrl.Contains(anyStringContains)
			       || x.rebrickableUrl.Contains(anyStringContains)
			       || (includeRelations == true && x.legoTheme.name.Contains(anyStringContains))
			       || (includeRelations == true && x.legoTheme.description.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.legoTheme);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.LegoSet> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.LegoSet legoSet in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(legoSet, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.LegoSet Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.LegoSet Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of LegoSets filtered by the parameters provided.  Its query is similar to the GetLegoSets method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LegoSets/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string setNumber = null,
			int? year = null,
			int? partCount = null,
			int? legoThemeId = null,
			string imageUrl = null,
			string brickLinkUrl = null,
			string rebrickableUrl = null,
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

			IQueryable<Database.LegoSet> query = (from ls in _context.LegoSets select ls);
			if (name != null)
			{
				query = query.Where(ls => ls.name == name);
			}
			if (setNumber != null)
			{
				query = query.Where(ls => ls.setNumber == setNumber);
			}
			if (year.HasValue == true)
			{
				query = query.Where(ls => ls.year == year.Value);
			}
			if (partCount.HasValue == true)
			{
				query = query.Where(ls => ls.partCount == partCount.Value);
			}
			if (legoThemeId.HasValue == true)
			{
				query = query.Where(ls => ls.legoThemeId == legoThemeId.Value);
			}
			if (imageUrl != null)
			{
				query = query.Where(ls => ls.imageUrl == imageUrl);
			}
			if (brickLinkUrl != null)
			{
				query = query.Where(ls => ls.brickLinkUrl == brickLinkUrl);
			}
			if (rebrickableUrl != null)
			{
				query = query.Where(ls => ls.rebrickableUrl == rebrickableUrl);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ls => ls.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ls => ls.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ls => ls.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ls => ls.deleted == false);
				}
			}
			else
			{
				query = query.Where(ls => ls.active == true);
				query = query.Where(ls => ls.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Lego Set, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.setNumber.Contains(anyStringContains)
			       || x.imageUrl.Contains(anyStringContains)
			       || x.brickLinkUrl.Contains(anyStringContains)
			       || x.rebrickableUrl.Contains(anyStringContains)
			       || x.legoTheme.name.Contains(anyStringContains)
			       || x.legoTheme.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single LegoSet by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LegoSet/{id}")]
		public async Task<IActionResult> GetLegoSet(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.LegoSet> query = (from ls in _context.LegoSets where
							(ls.id == id) &&
							(userIsAdmin == true || ls.deleted == false) &&
							(userIsWriter == true || ls.active == true)
					select ls);

				if (includeRelations == true)
				{
					query = query.Include(x => x.legoTheme);
					query = query.AsSplitQuery();
				}

				Database.LegoSet materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.LegoSet Entity was read with Admin privilege." : "BMC.LegoSet Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "LegoSet", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.LegoSet entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.LegoSet.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.LegoSet.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing LegoSet record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/LegoSet/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutLegoSet(int id, [FromBody]Database.LegoSet.LegoSetDTO legoSetDTO, CancellationToken cancellationToken = default)
		{
			if (legoSetDTO == null)
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



			if (id != legoSetDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.LegoSet> query = (from x in _context.LegoSets
				where
				(x.id == id)
				select x);


			Database.LegoSet existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.LegoSet PUT", id.ToString(), new Exception("No BMC.LegoSet entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (legoSetDTO.objectGuid == Guid.Empty)
            {
                legoSetDTO.objectGuid = existing.objectGuid;
            }
            else if (legoSetDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a LegoSet record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.LegoSet cloneOfExisting = (Database.LegoSet)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new LegoSet object using the data from the existing record, updated with what is in the DTO.
			//
			Database.LegoSet legoSet = (Database.LegoSet)_context.Entry(existing).GetDatabaseValues().ToObject();
			legoSet.ApplyDTO(legoSetDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (legoSet.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.LegoSet record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (legoSet.name != null && legoSet.name.Length > 500)
			{
				legoSet.name = legoSet.name.Substring(0, 500);
			}

			if (legoSet.setNumber != null && legoSet.setNumber.Length > 100)
			{
				legoSet.setNumber = legoSet.setNumber.Substring(0, 100);
			}

			if (legoSet.imageUrl != null && legoSet.imageUrl.Length > 250)
			{
				legoSet.imageUrl = legoSet.imageUrl.Substring(0, 250);
			}

			if (legoSet.brickLinkUrl != null && legoSet.brickLinkUrl.Length > 250)
			{
				legoSet.brickLinkUrl = legoSet.brickLinkUrl.Substring(0, 250);
			}

			if (legoSet.rebrickableUrl != null && legoSet.rebrickableUrl.Length > 250)
			{
				legoSet.rebrickableUrl = legoSet.rebrickableUrl.Substring(0, 250);
			}

			EntityEntry<Database.LegoSet> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(legoSet);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.LegoSet entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.LegoSet.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LegoSet.CreateAnonymousWithFirstLevelSubObjects(legoSet)),
					null);


				return Ok(Database.LegoSet.CreateAnonymous(legoSet));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.LegoSet entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.LegoSet.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LegoSet.CreateAnonymousWithFirstLevelSubObjects(legoSet)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new LegoSet record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LegoSet", Name = "LegoSet")]
		public async Task<IActionResult> PostLegoSet([FromBody]Database.LegoSet.LegoSetDTO legoSetDTO, CancellationToken cancellationToken = default)
		{
			if (legoSetDTO == null)
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
			// Create a new LegoSet object using the data from the DTO
			//
			Database.LegoSet legoSet = Database.LegoSet.FromDTO(legoSetDTO);

			try
			{
				if (legoSet.name != null && legoSet.name.Length > 500)
				{
					legoSet.name = legoSet.name.Substring(0, 500);
				}

				if (legoSet.setNumber != null && legoSet.setNumber.Length > 100)
				{
					legoSet.setNumber = legoSet.setNumber.Substring(0, 100);
				}

				if (legoSet.imageUrl != null && legoSet.imageUrl.Length > 250)
				{
					legoSet.imageUrl = legoSet.imageUrl.Substring(0, 250);
				}

				if (legoSet.brickLinkUrl != null && legoSet.brickLinkUrl.Length > 250)
				{
					legoSet.brickLinkUrl = legoSet.brickLinkUrl.Substring(0, 250);
				}

				if (legoSet.rebrickableUrl != null && legoSet.rebrickableUrl.Length > 250)
				{
					legoSet.rebrickableUrl = legoSet.rebrickableUrl.Substring(0, 250);
				}

				legoSet.objectGuid = Guid.NewGuid();
				_context.LegoSets.Add(legoSet);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.LegoSet entity successfully created.",
					true,
					legoSet.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.LegoSet.CreateAnonymousWithFirstLevelSubObjects(legoSet)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.LegoSet entity creation failed.", false, legoSet.id.ToString(), "", JsonSerializer.Serialize(legoSet), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "LegoSet", legoSet.id, legoSet.name));

			return CreatedAtRoute("LegoSet", new { id = legoSet.id }, Database.LegoSet.CreateAnonymousWithFirstLevelSubObjects(legoSet));
		}



        /// <summary>
        /// 
        /// This deletes a LegoSet record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LegoSet/{id}")]
		[Route("api/LegoSet")]
		public async Task<IActionResult> DeleteLegoSet(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.LegoSet> query = (from x in _context.LegoSets
				where
				(x.id == id)
				select x);


			Database.LegoSet legoSet = await query.FirstOrDefaultAsync(cancellationToken);

			if (legoSet == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.LegoSet DELETE", id.ToString(), new Exception("No BMC.LegoSet entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.LegoSet cloneOfExisting = (Database.LegoSet)_context.Entry(legoSet).GetDatabaseValues().ToObject();


			try
			{
				legoSet.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.LegoSet entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.LegoSet.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LegoSet.CreateAnonymousWithFirstLevelSubObjects(legoSet)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.LegoSet entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.LegoSet.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LegoSet.CreateAnonymousWithFirstLevelSubObjects(legoSet)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of LegoSet records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/LegoSets/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string setNumber = null,
			int? year = null,
			int? partCount = null,
			int? legoThemeId = null,
			string imageUrl = null,
			string brickLinkUrl = null,
			string rebrickableUrl = null,
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

			IQueryable<Database.LegoSet> query = (from ls in _context.LegoSets select ls);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ls => ls.name == name);
			}
			if (string.IsNullOrEmpty(setNumber) == false)
			{
				query = query.Where(ls => ls.setNumber == setNumber);
			}
			if (year.HasValue == true)
			{
				query = query.Where(ls => ls.year == year.Value);
			}
			if (partCount.HasValue == true)
			{
				query = query.Where(ls => ls.partCount == partCount.Value);
			}
			if (legoThemeId.HasValue == true)
			{
				query = query.Where(ls => ls.legoThemeId == legoThemeId.Value);
			}
			if (string.IsNullOrEmpty(imageUrl) == false)
			{
				query = query.Where(ls => ls.imageUrl == imageUrl);
			}
			if (string.IsNullOrEmpty(brickLinkUrl) == false)
			{
				query = query.Where(ls => ls.brickLinkUrl == brickLinkUrl);
			}
			if (string.IsNullOrEmpty(rebrickableUrl) == false)
			{
				query = query.Where(ls => ls.rebrickableUrl == rebrickableUrl);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ls => ls.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ls => ls.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ls => ls.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ls => ls.deleted == false);
				}
			}
			else
			{
				query = query.Where(ls => ls.active == true);
				query = query.Where(ls => ls.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Lego Set, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.setNumber.Contains(anyStringContains)
			       || x.imageUrl.Contains(anyStringContains)
			       || x.brickLinkUrl.Contains(anyStringContains)
			       || x.rebrickableUrl.Contains(anyStringContains)
			       || x.legoTheme.name.Contains(anyStringContains)
			       || x.legoTheme.description.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.name).ThenBy(x => x.setNumber).ThenBy(x => x.imageUrl);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.LegoSet.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/LegoSet/CreateAuditEvent")]
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
