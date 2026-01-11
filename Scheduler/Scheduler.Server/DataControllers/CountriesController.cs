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
    /// This auto generated class provides the basic CRUD operations for the Country entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Country entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class CountriesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private SchedulerContext _context;

		private ILogger<CountriesController> _logger;

		public CountriesController(SchedulerContext context, ILogger<CountriesController> logger) : base("Scheduler", "Country")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Countries filtered by the parameters provided.
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
		[Route("api/Countries")]
		public async Task<IActionResult> GetCountries(
			string name = null,
			string description = null,
			string abbreviation = null,
			string postalCodeFormat = null,
			string postalCodeRegEx = null,
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

			IQueryable<Database.Country> query = (from c in _context.Countries select c);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(c => c.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(c => c.description == description);
			}
			if (string.IsNullOrEmpty(abbreviation) == false)
			{
				query = query.Where(c => c.abbreviation == abbreviation);
			}
			if (string.IsNullOrEmpty(postalCodeFormat) == false)
			{
				query = query.Where(c => c.postalCodeFormat == postalCodeFormat);
			}
			if (string.IsNullOrEmpty(postalCodeRegEx) == false)
			{
				query = query.Where(c => c.postalCodeRegEx == postalCodeRegEx);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(c => c.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(c => c.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(c => c.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(c => c.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(c => c.deleted == false);
				}
			}
			else
			{
				query = query.Where(c => c.active == true);
				query = query.Where(c => c.deleted == false);
			}

			query = query.OrderBy(c => c.sequence).ThenBy(c => c.name).ThenBy(c => c.description).ThenBy(c => c.abbreviation);

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
			// Add the any string contains parameter to span all the string fields on the Country, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.abbreviation.Contains(anyStringContains)
			       || x.postalCodeFormat.Contains(anyStringContains)
			       || x.postalCodeRegEx.Contains(anyStringContains)
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.Country> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Country country in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(country, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.Country Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.Country Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Countries filtered by the parameters provided.  Its query is similar to the GetCountries method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Countries/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			string abbreviation = null,
			string postalCodeFormat = null,
			string postalCodeRegEx = null,
			int? sequence = null,
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

			IQueryable<Database.Country> query = (from c in _context.Countries select c);
			if (name != null)
			{
				query = query.Where(c => c.name == name);
			}
			if (description != null)
			{
				query = query.Where(c => c.description == description);
			}
			if (abbreviation != null)
			{
				query = query.Where(c => c.abbreviation == abbreviation);
			}
			if (postalCodeFormat != null)
			{
				query = query.Where(c => c.postalCodeFormat == postalCodeFormat);
			}
			if (postalCodeRegEx != null)
			{
				query = query.Where(c => c.postalCodeRegEx == postalCodeRegEx);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(c => c.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(c => c.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(c => c.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(c => c.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(c => c.deleted == false);
				}
			}
			else
			{
				query = query.Where(c => c.active == true);
				query = query.Where(c => c.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Country, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.abbreviation.Contains(anyStringContains)
			       || x.postalCodeFormat.Contains(anyStringContains)
			       || x.postalCodeRegEx.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single Country by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Country/{id}")]
		public async Task<IActionResult> GetCountry(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.Country> query = (from c in _context.Countries where
							(c.id == id) &&
							(userIsAdmin == true || c.deleted == false) &&
							(userIsWriter == true || c.active == true)
					select c);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.Country materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.Country Entity was read with Admin privilege." : "Scheduler.Country Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Country", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.Country entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.Country.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.Country.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing Country record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Country/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutCountry(int id, [FromBody]Database.Country.CountryDTO countryDTO, CancellationToken cancellationToken = default)
		{
			if (countryDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != countryDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.Country> query = (from x in _context.Countries
				where
				(x.id == id)
				select x);


			Database.Country existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Country PUT", id.ToString(), new Exception("No Scheduler.Country entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (countryDTO.objectGuid == Guid.Empty)
            {
                countryDTO.objectGuid = existing.objectGuid;
            }
            else if (countryDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Country record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Country cloneOfExisting = (Database.Country)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Country object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Country country = (Database.Country)_context.Entry(existing).GetDatabaseValues().ToObject();
			country.ApplyDTO(countryDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (country.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.Country record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (country.name != null && country.name.Length > 100)
			{
				country.name = country.name.Substring(0, 100);
			}

			if (country.description != null && country.description.Length > 500)
			{
				country.description = country.description.Substring(0, 500);
			}

			if (country.abbreviation != null && country.abbreviation.Length > 10)
			{
				country.abbreviation = country.abbreviation.Substring(0, 10);
			}

			if (country.postalCodeFormat != null && country.postalCodeFormat.Length > 50)
			{
				country.postalCodeFormat = country.postalCodeFormat.Substring(0, 50);
			}

			if (country.postalCodeRegEx != null && country.postalCodeRegEx.Length > 50)
			{
				country.postalCodeRegEx = country.postalCodeRegEx.Substring(0, 50);
			}

			EntityEntry<Database.Country> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(country);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.Country entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.Country.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.Country.CreateAnonymousWithFirstLevelSubObjects(country)),
					null);


				return Ok(Database.Country.CreateAnonymous(country));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.Country entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.Country.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.Country.CreateAnonymousWithFirstLevelSubObjects(country)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new Country record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Country", Name = "Country")]
		public async Task<IActionResult> PostCountry([FromBody]Database.Country.CountryDTO countryDTO, CancellationToken cancellationToken = default)
		{
			if (countryDTO == null)
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
			// Create a new Country object using the data from the DTO
			//
			Database.Country country = Database.Country.FromDTO(countryDTO);

			try
			{
				if (country.name != null && country.name.Length > 100)
				{
					country.name = country.name.Substring(0, 100);
				}

				if (country.description != null && country.description.Length > 500)
				{
					country.description = country.description.Substring(0, 500);
				}

				if (country.abbreviation != null && country.abbreviation.Length > 10)
				{
					country.abbreviation = country.abbreviation.Substring(0, 10);
				}

				if (country.postalCodeFormat != null && country.postalCodeFormat.Length > 50)
				{
					country.postalCodeFormat = country.postalCodeFormat.Substring(0, 50);
				}

				if (country.postalCodeRegEx != null && country.postalCodeRegEx.Length > 50)
				{
					country.postalCodeRegEx = country.postalCodeRegEx.Substring(0, 50);
				}

				country.objectGuid = Guid.NewGuid();
				_context.Countries.Add(country);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.Country entity successfully created.",
					true,
					country.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.Country.CreateAnonymousWithFirstLevelSubObjects(country)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.Country entity creation failed.", false, country.id.ToString(), "", JsonSerializer.Serialize(country), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Country", country.id, country.name));

			return CreatedAtRoute("Country", new { id = country.id }, Database.Country.CreateAnonymousWithFirstLevelSubObjects(country));
		}



        /// <summary>
        /// 
        /// This deletes a Country record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Country/{id}")]
		[Route("api/Country")]
		public async Task<IActionResult> DeleteCountry(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.Country> query = (from x in _context.Countries
				where
				(x.id == id)
				select x);


			Database.Country country = await query.FirstOrDefaultAsync(cancellationToken);

			if (country == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Country DELETE", id.ToString(), new Exception("No Scheduler.Country entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Country cloneOfExisting = (Database.Country)_context.Entry(country).GetDatabaseValues().ToObject();


			try
			{
				country.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.Country entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.Country.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.Country.CreateAnonymousWithFirstLevelSubObjects(country)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.Country entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.Country.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.Country.CreateAnonymousWithFirstLevelSubObjects(country)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of Country records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Countries/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			string abbreviation = null,
			string postalCodeFormat = null,
			string postalCodeRegEx = null,
			int? sequence = null,
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

			IQueryable<Database.Country> query = (from c in _context.Countries select c);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(c => c.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(c => c.description == description);
			}
			if (string.IsNullOrEmpty(abbreviation) == false)
			{
				query = query.Where(c => c.abbreviation == abbreviation);
			}
			if (string.IsNullOrEmpty(postalCodeFormat) == false)
			{
				query = query.Where(c => c.postalCodeFormat == postalCodeFormat);
			}
			if (string.IsNullOrEmpty(postalCodeRegEx) == false)
			{
				query = query.Where(c => c.postalCodeRegEx == postalCodeRegEx);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(c => c.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(c => c.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(c => c.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(c => c.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(c => c.deleted == false);
				}
			}
			else
			{
				query = query.Where(c => c.active == true);
				query = query.Where(c => c.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Country, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.abbreviation.Contains(anyStringContains)
			       || x.postalCodeFormat.Contains(anyStringContains)
			       || x.postalCodeRegEx.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.abbreviation);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Country.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Country/CreateAuditEvent")]
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
