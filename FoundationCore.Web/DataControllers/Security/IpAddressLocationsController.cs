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
using static Foundation.Auditor.AuditEngine;
using Foundation.Security.Database;

namespace Foundation.Security.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the IpAddressLocation entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the IpAddressLocation entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class IpAddressLocationsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private SecurityContext _context;

		private ILogger<IpAddressLocationsController> _logger;

		public IpAddressLocationsController(SecurityContext context, ILogger<IpAddressLocationsController> logger) : base("Security", "IpAddressLocation")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of IpAddressLocations filtered by the parameters provided.
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
		[Route("api/IpAddressLocations")]
		public async Task<IActionResult> GetIpAddressLocations(
			string ipAddress = null,
			string countryCode = null,
			string countryName = null,
			string city = null,
			double? latitude = null,
			double? longitude = null,
			DateTime? lastLookupDate = null,
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
			// Security Reader role or better needed to read from this table, as well as the minimum read permission level.
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
			if (lastLookupDate.HasValue == true && lastLookupDate.Value.Kind != DateTimeKind.Utc)
			{
				lastLookupDate = lastLookupDate.Value.ToUniversalTime();
			}

			IQueryable<Database.IpAddressLocation> query = (from ial in _context.IpAddressLocations select ial);
			if (string.IsNullOrEmpty(ipAddress) == false)
			{
				query = query.Where(ial => ial.ipAddress == ipAddress);
			}
			if (string.IsNullOrEmpty(countryCode) == false)
			{
				query = query.Where(ial => ial.countryCode == countryCode);
			}
			if (string.IsNullOrEmpty(countryName) == false)
			{
				query = query.Where(ial => ial.countryName == countryName);
			}
			if (string.IsNullOrEmpty(city) == false)
			{
				query = query.Where(ial => ial.city == city);
			}
			if (latitude.HasValue == true)
			{
				query = query.Where(ial => ial.latitude == latitude.Value);
			}
			if (longitude.HasValue == true)
			{
				query = query.Where(ial => ial.longitude == longitude.Value);
			}
			if (lastLookupDate.HasValue == true)
			{
				query = query.Where(ial => ial.lastLookupDate == lastLookupDate.Value);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ial => ial.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ial => ial.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ial => ial.deleted == false);
				}
			}
			else
			{
				query = query.Where(ial => ial.active == true);
				query = query.Where(ial => ial.deleted == false);
			}

			query = query.OrderByDescending(ial => ial.lastLookupDate);


			//
			// Add the any string contains parameter to span all the string fields on the Ip Address Location, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.ipAddress.Contains(anyStringContains)
			       || x.countryCode.Contains(anyStringContains)
			       || x.countryName.Contains(anyStringContains)
			       || x.city.Contains(anyStringContains)
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
			
			List<Database.IpAddressLocation> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.IpAddressLocation ipAddressLocation in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(ipAddressLocation, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Security.IpAddressLocation Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Security.IpAddressLocation Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of IpAddressLocations filtered by the parameters provided.  Its query is similar to the GetIpAddressLocations method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/IpAddressLocations/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string ipAddress = null,
			string countryCode = null,
			string countryName = null,
			string city = null,
			double? latitude = null,
			double? longitude = null,
			DateTime? lastLookupDate = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Security Reader role or better needed to read from this table, as well as the minimum read permission level.
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
			if (lastLookupDate.HasValue == true && lastLookupDate.Value.Kind != DateTimeKind.Utc)
			{
				lastLookupDate = lastLookupDate.Value.ToUniversalTime();
			}

			IQueryable<Database.IpAddressLocation> query = (from ial in _context.IpAddressLocations select ial);
			if (ipAddress != null)
			{
				query = query.Where(ial => ial.ipAddress == ipAddress);
			}
			if (countryCode != null)
			{
				query = query.Where(ial => ial.countryCode == countryCode);
			}
			if (countryName != null)
			{
				query = query.Where(ial => ial.countryName == countryName);
			}
			if (city != null)
			{
				query = query.Where(ial => ial.city == city);
			}
			if (latitude.HasValue == true)
			{
				query = query.Where(ial => ial.latitude == latitude.Value);
			}
			if (longitude.HasValue == true)
			{
				query = query.Where(ial => ial.longitude == longitude.Value);
			}
			if (lastLookupDate.HasValue == true)
			{
				query = query.Where(ial => ial.lastLookupDate == lastLookupDate.Value);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ial => ial.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ial => ial.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ial => ial.deleted == false);
				}
			}
			else
			{
				query = query.Where(ial => ial.active == true);
				query = query.Where(ial => ial.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Ip Address Location, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.ipAddress.Contains(anyStringContains)
			       || x.countryCode.Contains(anyStringContains)
			       || x.countryName.Contains(anyStringContains)
			       || x.city.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single IpAddressLocation by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/IpAddressLocation/{id}")]
		public async Task<IActionResult> GetIpAddressLocation(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Security Reader role or better needed to read from this table, as well as the minimum read permission level.
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
				IQueryable<Database.IpAddressLocation> query = (from ial in _context.IpAddressLocations where
							(ial.id == id) &&
							(userIsAdmin == true || ial.deleted == false) &&
							(userIsWriter == true || ial.active == true)
					select ial);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.IpAddressLocation materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Security.IpAddressLocation Entity was read with Admin privilege." : "Security.IpAddressLocation Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "IpAddressLocation", materialized.id, materialized.ipAddress));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Security.IpAddressLocation entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Security.IpAddressLocation.   Entity was read with Admin privilege." : "Exception caught during entity read of Security.IpAddressLocation.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing IpAddressLocation record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/IpAddressLocation/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutIpAddressLocation(int id, [FromBody]Database.IpAddressLocation.IpAddressLocationDTO ipAddressLocationDTO, CancellationToken cancellationToken = default)
		{
			if (ipAddressLocationDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Security Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != ipAddressLocationDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.IpAddressLocation> query = (from x in _context.IpAddressLocations
				where
				(x.id == id)
				select x);


			Database.IpAddressLocation existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.IpAddressLocation PUT", id.ToString(), new Exception("No Security.IpAddressLocation entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.IpAddressLocation cloneOfExisting = (Database.IpAddressLocation)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new IpAddressLocation object using the data from the existing record, updated with what is in the DTO.
			//
			Database.IpAddressLocation ipAddressLocation = (Database.IpAddressLocation)_context.Entry(existing).GetDatabaseValues().ToObject();
			ipAddressLocation.ApplyDTO(ipAddressLocationDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (ipAddressLocation.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Security.IpAddressLocation record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (ipAddressLocation.ipAddress != null && ipAddressLocation.ipAddress.Length > 50)
			{
				ipAddressLocation.ipAddress = ipAddressLocation.ipAddress.Substring(0, 50);
			}

			if (ipAddressLocation.countryCode != null && ipAddressLocation.countryCode.Length > 10)
			{
				ipAddressLocation.countryCode = ipAddressLocation.countryCode.Substring(0, 10);
			}

			if (ipAddressLocation.countryName != null && ipAddressLocation.countryName.Length > 100)
			{
				ipAddressLocation.countryName = ipAddressLocation.countryName.Substring(0, 100);
			}

			if (ipAddressLocation.city != null && ipAddressLocation.city.Length > 100)
			{
				ipAddressLocation.city = ipAddressLocation.city.Substring(0, 100);
			}

			if (ipAddressLocation.lastLookupDate.Kind != DateTimeKind.Utc)
			{
				ipAddressLocation.lastLookupDate = ipAddressLocation.lastLookupDate.ToUniversalTime();
			}

			EntityEntry<Database.IpAddressLocation> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(ipAddressLocation);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Security.IpAddressLocation entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.IpAddressLocation.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.IpAddressLocation.CreateAnonymousWithFirstLevelSubObjects(ipAddressLocation)),
					null);


				return Ok(Database.IpAddressLocation.CreateAnonymous(ipAddressLocation));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Security.IpAddressLocation entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.IpAddressLocation.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.IpAddressLocation.CreateAnonymousWithFirstLevelSubObjects(ipAddressLocation)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new IpAddressLocation record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/IpAddressLocation", Name = "IpAddressLocation")]
		public async Task<IActionResult> PostIpAddressLocation([FromBody]Database.IpAddressLocation.IpAddressLocationDTO ipAddressLocationDTO, CancellationToken cancellationToken = default)
		{
			if (ipAddressLocationDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Security Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new IpAddressLocation object using the data from the DTO
			//
			Database.IpAddressLocation ipAddressLocation = Database.IpAddressLocation.FromDTO(ipAddressLocationDTO);

			try
			{
				if (ipAddressLocation.ipAddress != null && ipAddressLocation.ipAddress.Length > 50)
				{
					ipAddressLocation.ipAddress = ipAddressLocation.ipAddress.Substring(0, 50);
				}

				if (ipAddressLocation.countryCode != null && ipAddressLocation.countryCode.Length > 10)
				{
					ipAddressLocation.countryCode = ipAddressLocation.countryCode.Substring(0, 10);
				}

				if (ipAddressLocation.countryName != null && ipAddressLocation.countryName.Length > 100)
				{
					ipAddressLocation.countryName = ipAddressLocation.countryName.Substring(0, 100);
				}

				if (ipAddressLocation.city != null && ipAddressLocation.city.Length > 100)
				{
					ipAddressLocation.city = ipAddressLocation.city.Substring(0, 100);
				}

				if (ipAddressLocation.lastLookupDate.Kind != DateTimeKind.Utc)
				{
					ipAddressLocation.lastLookupDate = ipAddressLocation.lastLookupDate.ToUniversalTime();
				}

				_context.IpAddressLocations.Add(ipAddressLocation);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Security.IpAddressLocation entity successfully created.",
					true,
					ipAddressLocation.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.IpAddressLocation.CreateAnonymousWithFirstLevelSubObjects(ipAddressLocation)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Security.IpAddressLocation entity creation failed.", false, ipAddressLocation.id.ToString(), "", JsonSerializer.Serialize(ipAddressLocation), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "IpAddressLocation", ipAddressLocation.id, ipAddressLocation.ipAddress));

			return CreatedAtRoute("IpAddressLocation", new { id = ipAddressLocation.id }, Database.IpAddressLocation.CreateAnonymousWithFirstLevelSubObjects(ipAddressLocation));
		}



        /// <summary>
        /// 
        /// This deletes a IpAddressLocation record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/IpAddressLocation/{id}")]
		[Route("api/IpAddressLocation")]
		public async Task<IActionResult> DeleteIpAddressLocation(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Security Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.IpAddressLocation> query = (from x in _context.IpAddressLocations
				where
				(x.id == id)
				select x);


			Database.IpAddressLocation ipAddressLocation = await query.FirstOrDefaultAsync(cancellationToken);

			if (ipAddressLocation == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.IpAddressLocation DELETE", id.ToString(), new Exception("No Security.IpAddressLocation entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.IpAddressLocation cloneOfExisting = (Database.IpAddressLocation)_context.Entry(ipAddressLocation).GetDatabaseValues().ToObject();


			try
			{
				ipAddressLocation.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.IpAddressLocation entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.IpAddressLocation.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.IpAddressLocation.CreateAnonymousWithFirstLevelSubObjects(ipAddressLocation)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.IpAddressLocation entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.IpAddressLocation.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.IpAddressLocation.CreateAnonymousWithFirstLevelSubObjects(ipAddressLocation)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of IpAddressLocation records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/IpAddressLocations/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string ipAddress = null,
			string countryCode = null,
			string countryName = null,
			string city = null,
			double? latitude = null,
			double? longitude = null,
			DateTime? lastLookupDate = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Security Reader role or better needed to read from this table, as well as the minimum read permission level.
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
			if (lastLookupDate.HasValue == true && lastLookupDate.Value.Kind != DateTimeKind.Utc)
			{
				lastLookupDate = lastLookupDate.Value.ToUniversalTime();
			}

			IQueryable<Database.IpAddressLocation> query = (from ial in _context.IpAddressLocations select ial);
			if (string.IsNullOrEmpty(ipAddress) == false)
			{
				query = query.Where(ial => ial.ipAddress == ipAddress);
			}
			if (string.IsNullOrEmpty(countryCode) == false)
			{
				query = query.Where(ial => ial.countryCode == countryCode);
			}
			if (string.IsNullOrEmpty(countryName) == false)
			{
				query = query.Where(ial => ial.countryName == countryName);
			}
			if (string.IsNullOrEmpty(city) == false)
			{
				query = query.Where(ial => ial.city == city);
			}
			if (lastLookupDate.HasValue == true)
			{
				query = query.Where(ial => ial.lastLookupDate == lastLookupDate.Value);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ial => ial.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ial => ial.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ial => ial.deleted == false);
				}
			}
			else
			{
				query = query.Where(ial => ial.active == true);
				query = query.Where(ial => ial.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Ip Address Location, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.ipAddress.Contains(anyStringContains)
			       || x.countryCode.Contains(anyStringContains)
			       || x.countryName.Contains(anyStringContains)
			       || x.city.Contains(anyStringContains)
			   );
			}


			query = query.OrderByDescending (x => x.lastLookupDate);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.IpAddressLocation.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/IpAddressLocation/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Security Writer role needed to write to this table, as well as the minimum write permission level.
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
