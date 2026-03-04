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
    /// This auto generated class provides the basic CRUD operations for the MarketDataCache entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the MarketDataCache entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class MarketDataCachesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<MarketDataCachesController> _logger;

		public MarketDataCachesController(BMCContext context, ILogger<MarketDataCachesController> logger) : base("BMC", "MarketDataCache")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of MarketDataCaches filtered by the parameters provided.
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
		[Route("api/MarketDataCaches")]
		public async Task<IActionResult> GetMarketDataCaches(
			string source = null,
			string itemType = null,
			string itemNumber = null,
			string condition = null,
			string responseJson = null,
			DateTime? fetchedDate = null,
			DateTime? expiresDate = null,
			int? ttlMinutes = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (fetchedDate.HasValue == true && fetchedDate.Value.Kind != DateTimeKind.Utc)
			{
				fetchedDate = fetchedDate.Value.ToUniversalTime();
			}

			if (expiresDate.HasValue == true && expiresDate.Value.Kind != DateTimeKind.Utc)
			{
				expiresDate = expiresDate.Value.ToUniversalTime();
			}

			IQueryable<Database.MarketDataCache> query = (from mdc in _context.MarketDataCaches select mdc);
			if (string.IsNullOrEmpty(source) == false)
			{
				query = query.Where(mdc => mdc.source == source);
			}
			if (string.IsNullOrEmpty(itemType) == false)
			{
				query = query.Where(mdc => mdc.itemType == itemType);
			}
			if (string.IsNullOrEmpty(itemNumber) == false)
			{
				query = query.Where(mdc => mdc.itemNumber == itemNumber);
			}
			if (string.IsNullOrEmpty(condition) == false)
			{
				query = query.Where(mdc => mdc.condition == condition);
			}
			if (string.IsNullOrEmpty(responseJson) == false)
			{
				query = query.Where(mdc => mdc.responseJson == responseJson);
			}
			if (fetchedDate.HasValue == true)
			{
				query = query.Where(mdc => mdc.fetchedDate == fetchedDate.Value);
			}
			if (expiresDate.HasValue == true)
			{
				query = query.Where(mdc => mdc.expiresDate == expiresDate.Value);
			}
			if (ttlMinutes.HasValue == true)
			{
				query = query.Where(mdc => mdc.ttlMinutes == ttlMinutes.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mdc => mdc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mdc => mdc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mdc => mdc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mdc => mdc.deleted == false);
				}
			}
			else
			{
				query = query.Where(mdc => mdc.active == true);
				query = query.Where(mdc => mdc.deleted == false);
			}

			query = query.OrderBy(mdc => mdc.source).ThenBy(mdc => mdc.itemType).ThenBy(mdc => mdc.itemNumber);


			//
			// Add the any string contains parameter to span all the string fields on the Market Data Cache, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.source.Contains(anyStringContains)
			       || x.itemType.Contains(anyStringContains)
			       || x.itemNumber.Contains(anyStringContains)
			       || x.condition.Contains(anyStringContains)
			       || x.responseJson.Contains(anyStringContains)
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
			
			List<Database.MarketDataCache> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.MarketDataCache marketDataCache in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(marketDataCache, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.MarketDataCache Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.MarketDataCache Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of MarketDataCaches filtered by the parameters provided.  Its query is similar to the GetMarketDataCaches method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MarketDataCaches/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string source = null,
			string itemType = null,
			string itemNumber = null,
			string condition = null,
			string responseJson = null,
			DateTime? fetchedDate = null,
			DateTime? expiresDate = null,
			int? ttlMinutes = null,
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

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (fetchedDate.HasValue == true && fetchedDate.Value.Kind != DateTimeKind.Utc)
			{
				fetchedDate = fetchedDate.Value.ToUniversalTime();
			}

			if (expiresDate.HasValue == true && expiresDate.Value.Kind != DateTimeKind.Utc)
			{
				expiresDate = expiresDate.Value.ToUniversalTime();
			}

			IQueryable<Database.MarketDataCache> query = (from mdc in _context.MarketDataCaches select mdc);
			if (source != null)
			{
				query = query.Where(mdc => mdc.source == source);
			}
			if (itemType != null)
			{
				query = query.Where(mdc => mdc.itemType == itemType);
			}
			if (itemNumber != null)
			{
				query = query.Where(mdc => mdc.itemNumber == itemNumber);
			}
			if (condition != null)
			{
				query = query.Where(mdc => mdc.condition == condition);
			}
			if (responseJson != null)
			{
				query = query.Where(mdc => mdc.responseJson == responseJson);
			}
			if (fetchedDate.HasValue == true)
			{
				query = query.Where(mdc => mdc.fetchedDate == fetchedDate.Value);
			}
			if (expiresDate.HasValue == true)
			{
				query = query.Where(mdc => mdc.expiresDate == expiresDate.Value);
			}
			if (ttlMinutes.HasValue == true)
			{
				query = query.Where(mdc => mdc.ttlMinutes == ttlMinutes.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mdc => mdc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mdc => mdc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mdc => mdc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mdc => mdc.deleted == false);
				}
			}
			else
			{
				query = query.Where(mdc => mdc.active == true);
				query = query.Where(mdc => mdc.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Market Data Cache, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.source.Contains(anyStringContains)
			       || x.itemType.Contains(anyStringContains)
			       || x.itemNumber.Contains(anyStringContains)
			       || x.condition.Contains(anyStringContains)
			       || x.responseJson.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single MarketDataCache by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MarketDataCache/{id}")]
		public async Task<IActionResult> GetMarketDataCache(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.MarketDataCache> query = (from mdc in _context.MarketDataCaches where
							(mdc.id == id) &&
							(userIsAdmin == true || mdc.deleted == false) &&
							(userIsWriter == true || mdc.active == true)
					select mdc);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.MarketDataCache materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.MarketDataCache Entity was read with Admin privilege." : "BMC.MarketDataCache Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "MarketDataCache", materialized.id, materialized.source));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.MarketDataCache entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.MarketDataCache.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.MarketDataCache.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing MarketDataCache record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/MarketDataCache/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutMarketDataCache(int id, [FromBody]Database.MarketDataCache.MarketDataCacheDTO marketDataCacheDTO, CancellationToken cancellationToken = default)
		{
			if (marketDataCacheDTO == null)
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



			if (id != marketDataCacheDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.MarketDataCache> query = (from x in _context.MarketDataCaches
				where
				(x.id == id)
				select x);


			Database.MarketDataCache existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.MarketDataCache PUT", id.ToString(), new Exception("No BMC.MarketDataCache entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (marketDataCacheDTO.objectGuid == Guid.Empty)
            {
                marketDataCacheDTO.objectGuid = existing.objectGuid;
            }
            else if (marketDataCacheDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a MarketDataCache record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.MarketDataCache cloneOfExisting = (Database.MarketDataCache)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new MarketDataCache object using the data from the existing record, updated with what is in the DTO.
			//
			Database.MarketDataCache marketDataCache = (Database.MarketDataCache)_context.Entry(existing).GetDatabaseValues().ToObject();
			marketDataCache.ApplyDTO(marketDataCacheDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (marketDataCache.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.MarketDataCache record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (marketDataCache.source != null && marketDataCache.source.Length > 50)
			{
				marketDataCache.source = marketDataCache.source.Substring(0, 50);
			}

			if (marketDataCache.itemType != null && marketDataCache.itemType.Length > 50)
			{
				marketDataCache.itemType = marketDataCache.itemType.Substring(0, 50);
			}

			if (marketDataCache.itemNumber != null && marketDataCache.itemNumber.Length > 100)
			{
				marketDataCache.itemNumber = marketDataCache.itemNumber.Substring(0, 100);
			}

			if (marketDataCache.condition != null && marketDataCache.condition.Length > 50)
			{
				marketDataCache.condition = marketDataCache.condition.Substring(0, 50);
			}

			if (marketDataCache.fetchedDate.Kind != DateTimeKind.Utc)
			{
				marketDataCache.fetchedDate = marketDataCache.fetchedDate.ToUniversalTime();
			}

			if (marketDataCache.expiresDate.Kind != DateTimeKind.Utc)
			{
				marketDataCache.expiresDate = marketDataCache.expiresDate.ToUniversalTime();
			}

			EntityEntry<Database.MarketDataCache> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(marketDataCache);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.MarketDataCache entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.MarketDataCache.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MarketDataCache.CreateAnonymousWithFirstLevelSubObjects(marketDataCache)),
					null);


				return Ok(Database.MarketDataCache.CreateAnonymous(marketDataCache));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.MarketDataCache entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.MarketDataCache.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MarketDataCache.CreateAnonymousWithFirstLevelSubObjects(marketDataCache)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new MarketDataCache record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MarketDataCache", Name = "MarketDataCache")]
		public async Task<IActionResult> PostMarketDataCache([FromBody]Database.MarketDataCache.MarketDataCacheDTO marketDataCacheDTO, CancellationToken cancellationToken = default)
		{
			if (marketDataCacheDTO == null)
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
			// Create a new MarketDataCache object using the data from the DTO
			//
			Database.MarketDataCache marketDataCache = Database.MarketDataCache.FromDTO(marketDataCacheDTO);

			try
			{
				if (marketDataCache.source != null && marketDataCache.source.Length > 50)
				{
					marketDataCache.source = marketDataCache.source.Substring(0, 50);
				}

				if (marketDataCache.itemType != null && marketDataCache.itemType.Length > 50)
				{
					marketDataCache.itemType = marketDataCache.itemType.Substring(0, 50);
				}

				if (marketDataCache.itemNumber != null && marketDataCache.itemNumber.Length > 100)
				{
					marketDataCache.itemNumber = marketDataCache.itemNumber.Substring(0, 100);
				}

				if (marketDataCache.condition != null && marketDataCache.condition.Length > 50)
				{
					marketDataCache.condition = marketDataCache.condition.Substring(0, 50);
				}

				if (marketDataCache.fetchedDate.Kind != DateTimeKind.Utc)
				{
					marketDataCache.fetchedDate = marketDataCache.fetchedDate.ToUniversalTime();
				}

				if (marketDataCache.expiresDate.Kind != DateTimeKind.Utc)
				{
					marketDataCache.expiresDate = marketDataCache.expiresDate.ToUniversalTime();
				}

				marketDataCache.objectGuid = Guid.NewGuid();
				_context.MarketDataCaches.Add(marketDataCache);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.MarketDataCache entity successfully created.",
					true,
					marketDataCache.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.MarketDataCache.CreateAnonymousWithFirstLevelSubObjects(marketDataCache)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.MarketDataCache entity creation failed.", false, marketDataCache.id.ToString(), "", JsonSerializer.Serialize(marketDataCache), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "MarketDataCache", marketDataCache.id, marketDataCache.source));

			return CreatedAtRoute("MarketDataCache", new { id = marketDataCache.id }, Database.MarketDataCache.CreateAnonymousWithFirstLevelSubObjects(marketDataCache));
		}



        /// <summary>
        /// 
        /// This deletes a MarketDataCache record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MarketDataCache/{id}")]
		[Route("api/MarketDataCache")]
		public async Task<IActionResult> DeleteMarketDataCache(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.MarketDataCache> query = (from x in _context.MarketDataCaches
				where
				(x.id == id)
				select x);


			Database.MarketDataCache marketDataCache = await query.FirstOrDefaultAsync(cancellationToken);

			if (marketDataCache == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.MarketDataCache DELETE", id.ToString(), new Exception("No BMC.MarketDataCache entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.MarketDataCache cloneOfExisting = (Database.MarketDataCache)_context.Entry(marketDataCache).GetDatabaseValues().ToObject();


			try
			{
				marketDataCache.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.MarketDataCache entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.MarketDataCache.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MarketDataCache.CreateAnonymousWithFirstLevelSubObjects(marketDataCache)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.MarketDataCache entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.MarketDataCache.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MarketDataCache.CreateAnonymousWithFirstLevelSubObjects(marketDataCache)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of MarketDataCache records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/MarketDataCaches/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string source = null,
			string itemType = null,
			string itemNumber = null,
			string condition = null,
			string responseJson = null,
			DateTime? fetchedDate = null,
			DateTime? expiresDate = null,
			int? ttlMinutes = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (fetchedDate.HasValue == true && fetchedDate.Value.Kind != DateTimeKind.Utc)
			{
				fetchedDate = fetchedDate.Value.ToUniversalTime();
			}

			if (expiresDate.HasValue == true && expiresDate.Value.Kind != DateTimeKind.Utc)
			{
				expiresDate = expiresDate.Value.ToUniversalTime();
			}

			IQueryable<Database.MarketDataCache> query = (from mdc in _context.MarketDataCaches select mdc);
			if (string.IsNullOrEmpty(source) == false)
			{
				query = query.Where(mdc => mdc.source == source);
			}
			if (string.IsNullOrEmpty(itemType) == false)
			{
				query = query.Where(mdc => mdc.itemType == itemType);
			}
			if (string.IsNullOrEmpty(itemNumber) == false)
			{
				query = query.Where(mdc => mdc.itemNumber == itemNumber);
			}
			if (string.IsNullOrEmpty(condition) == false)
			{
				query = query.Where(mdc => mdc.condition == condition);
			}
			if (string.IsNullOrEmpty(responseJson) == false)
			{
				query = query.Where(mdc => mdc.responseJson == responseJson);
			}
			if (fetchedDate.HasValue == true)
			{
				query = query.Where(mdc => mdc.fetchedDate == fetchedDate.Value);
			}
			if (expiresDate.HasValue == true)
			{
				query = query.Where(mdc => mdc.expiresDate == expiresDate.Value);
			}
			if (ttlMinutes.HasValue == true)
			{
				query = query.Where(mdc => mdc.ttlMinutes == ttlMinutes.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mdc => mdc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mdc => mdc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mdc => mdc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mdc => mdc.deleted == false);
				}
			}
			else
			{
				query = query.Where(mdc => mdc.active == true);
				query = query.Where(mdc => mdc.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Market Data Cache, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.source.Contains(anyStringContains)
			       || x.itemType.Contains(anyStringContains)
			       || x.itemNumber.Contains(anyStringContains)
			       || x.condition.Contains(anyStringContains)
			       || x.responseJson.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.source).ThenBy(x => x.itemType).ThenBy(x => x.itemNumber);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.MarketDataCache.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/MarketDataCache/CreateAuditEvent")]
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
