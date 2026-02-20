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
    /// This auto generated class provides the basic CRUD operations for the LegoMinifig entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the LegoMinifig entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class LegoMinifigsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<LegoMinifigsController> _logger;

		public LegoMinifigsController(BMCContext context, ILogger<LegoMinifigsController> logger) : base("BMC", "LegoMinifig")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of LegoMinifigs filtered by the parameters provided.
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
		[Route("api/LegoMinifigs")]
		public async Task<IActionResult> GetLegoMinifigs(
			string name = null,
			string figNumber = null,
			int? partCount = null,
			string imageUrl = null,
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

			IQueryable<Database.LegoMinifig> query = (from lm in _context.LegoMinifigs select lm);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(lm => lm.name == name);
			}
			if (string.IsNullOrEmpty(figNumber) == false)
			{
				query = query.Where(lm => lm.figNumber == figNumber);
			}
			if (partCount.HasValue == true)
			{
				query = query.Where(lm => lm.partCount == partCount.Value);
			}
			if (string.IsNullOrEmpty(imageUrl) == false)
			{
				query = query.Where(lm => lm.imageUrl == imageUrl);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(lm => lm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(lm => lm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(lm => lm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(lm => lm.deleted == false);
				}
			}
			else
			{
				query = query.Where(lm => lm.active == true);
				query = query.Where(lm => lm.deleted == false);
			}

			query = query.OrderBy(lm => lm.name).ThenBy(lm => lm.figNumber).ThenBy(lm => lm.imageUrl);


			//
			// Add the any string contains parameter to span all the string fields on the Lego Minifig, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.figNumber.Contains(anyStringContains)
			       || x.imageUrl.Contains(anyStringContains)
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
			
			List<Database.LegoMinifig> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.LegoMinifig legoMinifig in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(legoMinifig, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.LegoMinifig Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.LegoMinifig Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of LegoMinifigs filtered by the parameters provided.  Its query is similar to the GetLegoMinifigs method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LegoMinifigs/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string figNumber = null,
			int? partCount = null,
			string imageUrl = null,
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

			IQueryable<Database.LegoMinifig> query = (from lm in _context.LegoMinifigs select lm);
			if (name != null)
			{
				query = query.Where(lm => lm.name == name);
			}
			if (figNumber != null)
			{
				query = query.Where(lm => lm.figNumber == figNumber);
			}
			if (partCount.HasValue == true)
			{
				query = query.Where(lm => lm.partCount == partCount.Value);
			}
			if (imageUrl != null)
			{
				query = query.Where(lm => lm.imageUrl == imageUrl);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(lm => lm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(lm => lm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(lm => lm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(lm => lm.deleted == false);
				}
			}
			else
			{
				query = query.Where(lm => lm.active == true);
				query = query.Where(lm => lm.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Lego Minifig, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.figNumber.Contains(anyStringContains)
			       || x.imageUrl.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single LegoMinifig by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LegoMinifig/{id}")]
		public async Task<IActionResult> GetLegoMinifig(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.LegoMinifig> query = (from lm in _context.LegoMinifigs where
							(lm.id == id) &&
							(userIsAdmin == true || lm.deleted == false) &&
							(userIsWriter == true || lm.active == true)
					select lm);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.LegoMinifig materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.LegoMinifig Entity was read with Admin privilege." : "BMC.LegoMinifig Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "LegoMinifig", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.LegoMinifig entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.LegoMinifig.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.LegoMinifig.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing LegoMinifig record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/LegoMinifig/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutLegoMinifig(int id, [FromBody]Database.LegoMinifig.LegoMinifigDTO legoMinifigDTO, CancellationToken cancellationToken = default)
		{
			if (legoMinifigDTO == null)
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



			if (id != legoMinifigDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.LegoMinifig> query = (from x in _context.LegoMinifigs
				where
				(x.id == id)
				select x);


			Database.LegoMinifig existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.LegoMinifig PUT", id.ToString(), new Exception("No BMC.LegoMinifig entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (legoMinifigDTO.objectGuid == Guid.Empty)
            {
                legoMinifigDTO.objectGuid = existing.objectGuid;
            }
            else if (legoMinifigDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a LegoMinifig record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.LegoMinifig cloneOfExisting = (Database.LegoMinifig)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new LegoMinifig object using the data from the existing record, updated with what is in the DTO.
			//
			Database.LegoMinifig legoMinifig = (Database.LegoMinifig)_context.Entry(existing).GetDatabaseValues().ToObject();
			legoMinifig.ApplyDTO(legoMinifigDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (legoMinifig.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.LegoMinifig record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (legoMinifig.name != null && legoMinifig.name.Length > 500)
			{
				legoMinifig.name = legoMinifig.name.Substring(0, 500);
			}

			if (legoMinifig.figNumber != null && legoMinifig.figNumber.Length > 100)
			{
				legoMinifig.figNumber = legoMinifig.figNumber.Substring(0, 100);
			}

			if (legoMinifig.imageUrl != null && legoMinifig.imageUrl.Length > 250)
			{
				legoMinifig.imageUrl = legoMinifig.imageUrl.Substring(0, 250);
			}

			EntityEntry<Database.LegoMinifig> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(legoMinifig);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.LegoMinifig entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.LegoMinifig.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LegoMinifig.CreateAnonymousWithFirstLevelSubObjects(legoMinifig)),
					null);


				return Ok(Database.LegoMinifig.CreateAnonymous(legoMinifig));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.LegoMinifig entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.LegoMinifig.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LegoMinifig.CreateAnonymousWithFirstLevelSubObjects(legoMinifig)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new LegoMinifig record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LegoMinifig", Name = "LegoMinifig")]
		public async Task<IActionResult> PostLegoMinifig([FromBody]Database.LegoMinifig.LegoMinifigDTO legoMinifigDTO, CancellationToken cancellationToken = default)
		{
			if (legoMinifigDTO == null)
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
			// Create a new LegoMinifig object using the data from the DTO
			//
			Database.LegoMinifig legoMinifig = Database.LegoMinifig.FromDTO(legoMinifigDTO);

			try
			{
				if (legoMinifig.name != null && legoMinifig.name.Length > 500)
				{
					legoMinifig.name = legoMinifig.name.Substring(0, 500);
				}

				if (legoMinifig.figNumber != null && legoMinifig.figNumber.Length > 100)
				{
					legoMinifig.figNumber = legoMinifig.figNumber.Substring(0, 100);
				}

				if (legoMinifig.imageUrl != null && legoMinifig.imageUrl.Length > 250)
				{
					legoMinifig.imageUrl = legoMinifig.imageUrl.Substring(0, 250);
				}

				legoMinifig.objectGuid = Guid.NewGuid();
				_context.LegoMinifigs.Add(legoMinifig);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.LegoMinifig entity successfully created.",
					true,
					legoMinifig.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.LegoMinifig.CreateAnonymousWithFirstLevelSubObjects(legoMinifig)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.LegoMinifig entity creation failed.", false, legoMinifig.id.ToString(), "", JsonSerializer.Serialize(legoMinifig), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "LegoMinifig", legoMinifig.id, legoMinifig.name));

			return CreatedAtRoute("LegoMinifig", new { id = legoMinifig.id }, Database.LegoMinifig.CreateAnonymousWithFirstLevelSubObjects(legoMinifig));
		}



        /// <summary>
        /// 
        /// This deletes a LegoMinifig record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LegoMinifig/{id}")]
		[Route("api/LegoMinifig")]
		public async Task<IActionResult> DeleteLegoMinifig(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.LegoMinifig> query = (from x in _context.LegoMinifigs
				where
				(x.id == id)
				select x);


			Database.LegoMinifig legoMinifig = await query.FirstOrDefaultAsync(cancellationToken);

			if (legoMinifig == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.LegoMinifig DELETE", id.ToString(), new Exception("No BMC.LegoMinifig entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.LegoMinifig cloneOfExisting = (Database.LegoMinifig)_context.Entry(legoMinifig).GetDatabaseValues().ToObject();


			try
			{
				legoMinifig.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.LegoMinifig entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.LegoMinifig.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LegoMinifig.CreateAnonymousWithFirstLevelSubObjects(legoMinifig)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.LegoMinifig entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.LegoMinifig.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LegoMinifig.CreateAnonymousWithFirstLevelSubObjects(legoMinifig)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of LegoMinifig records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/LegoMinifigs/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string figNumber = null,
			int? partCount = null,
			string imageUrl = null,
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

			IQueryable<Database.LegoMinifig> query = (from lm in _context.LegoMinifigs select lm);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(lm => lm.name == name);
			}
			if (string.IsNullOrEmpty(figNumber) == false)
			{
				query = query.Where(lm => lm.figNumber == figNumber);
			}
			if (partCount.HasValue == true)
			{
				query = query.Where(lm => lm.partCount == partCount.Value);
			}
			if (string.IsNullOrEmpty(imageUrl) == false)
			{
				query = query.Where(lm => lm.imageUrl == imageUrl);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(lm => lm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(lm => lm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(lm => lm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(lm => lm.deleted == false);
				}
			}
			else
			{
				query = query.Where(lm => lm.active == true);
				query = query.Where(lm => lm.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Lego Minifig, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.figNumber.Contains(anyStringContains)
			       || x.imageUrl.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.name).ThenBy(x => x.figNumber).ThenBy(x => x.imageUrl);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.LegoMinifig.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/LegoMinifig/CreateAuditEvent")]
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
