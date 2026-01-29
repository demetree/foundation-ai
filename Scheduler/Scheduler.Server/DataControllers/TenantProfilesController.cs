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
using Foundation.ChangeHistory;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the TenantProfile entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the TenantProfile entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class TenantProfilesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object tenantProfilePutSyncRoot = new object();
		static object tenantProfileDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<TenantProfilesController> _logger;

		public TenantProfilesController(SchedulerContext context, ILogger<TenantProfilesController> logger) : base("Scheduler", "TenantProfile")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of TenantProfiles filtered by the parameters provided.
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
		[Route("api/TenantProfiles")]
		public async Task<IActionResult> GetTenantProfiles(
			string name = null,
			string description = null,
			string companyLogoFileName = null,
			long? companyLogoSize = null,
			string companyLogoMimeType = null,
			string addressLine1 = null,
			string addressLine2 = null,
			string addressLine3 = null,
			string city = null,
			string postalCode = null,
			int? stateProvinceId = null,
			int? countryId = null,
			int? timeZoneId = null,
			string phoneNumber = null,
			string email = null,
			string website = null,
			double? latitude = null,
			double? longitude = null,
			string primaryColor = null,
			string secondaryColor = null,
			bool? displaysMetric = null,
			bool? displaysUSTerms = null,
			int? versionNumber = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}

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

			IQueryable<Database.TenantProfile> query = (from tp in _context.TenantProfiles select tp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(tp => tp.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(tp => tp.description == description);
			}
			if (string.IsNullOrEmpty(companyLogoFileName) == false)
			{
				query = query.Where(tp => tp.companyLogoFileName == companyLogoFileName);
			}
			if (companyLogoSize.HasValue == true)
			{
				query = query.Where(tp => tp.companyLogoSize == companyLogoSize.Value);
			}
			if (string.IsNullOrEmpty(companyLogoMimeType) == false)
			{
				query = query.Where(tp => tp.companyLogoMimeType == companyLogoMimeType);
			}
			if (string.IsNullOrEmpty(addressLine1) == false)
			{
				query = query.Where(tp => tp.addressLine1 == addressLine1);
			}
			if (string.IsNullOrEmpty(addressLine2) == false)
			{
				query = query.Where(tp => tp.addressLine2 == addressLine2);
			}
			if (string.IsNullOrEmpty(addressLine3) == false)
			{
				query = query.Where(tp => tp.addressLine3 == addressLine3);
			}
			if (string.IsNullOrEmpty(city) == false)
			{
				query = query.Where(tp => tp.city == city);
			}
			if (string.IsNullOrEmpty(postalCode) == false)
			{
				query = query.Where(tp => tp.postalCode == postalCode);
			}
			if (stateProvinceId.HasValue == true)
			{
				query = query.Where(tp => tp.stateProvinceId == stateProvinceId.Value);
			}
			if (countryId.HasValue == true)
			{
				query = query.Where(tp => tp.countryId == countryId.Value);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(tp => tp.timeZoneId == timeZoneId.Value);
			}
			if (string.IsNullOrEmpty(phoneNumber) == false)
			{
				query = query.Where(tp => tp.phoneNumber == phoneNumber);
			}
			if (string.IsNullOrEmpty(email) == false)
			{
				query = query.Where(tp => tp.email == email);
			}
			if (string.IsNullOrEmpty(website) == false)
			{
				query = query.Where(tp => tp.website == website);
			}
			if (latitude.HasValue == true)
			{
				query = query.Where(tp => tp.latitude == latitude.Value);
			}
			if (longitude.HasValue == true)
			{
				query = query.Where(tp => tp.longitude == longitude.Value);
			}
			if (string.IsNullOrEmpty(primaryColor) == false)
			{
				query = query.Where(tp => tp.primaryColor == primaryColor);
			}
			if (string.IsNullOrEmpty(secondaryColor) == false)
			{
				query = query.Where(tp => tp.secondaryColor == secondaryColor);
			}
			if (displaysMetric.HasValue == true)
			{
				query = query.Where(tp => tp.displaysMetric == displaysMetric.Value);
			}
			if (displaysUSTerms.HasValue == true)
			{
				query = query.Where(tp => tp.displaysUSTerms == displaysUSTerms.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(tp => tp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(tp => tp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(tp => tp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(tp => tp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(tp => tp.deleted == false);
				}
			}
			else
			{
				query = query.Where(tp => tp.active == true);
				query = query.Where(tp => tp.deleted == false);
			}

			query = query.OrderBy(tp => tp.name).ThenBy(tp => tp.description).ThenBy(tp => tp.companyLogoFileName);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.country);
				query = query.Include(x => x.stateProvince);
				query = query.Include(x => x.timeZone);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Tenant Profile, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.companyLogoFileName.Contains(anyStringContains)
			       || x.companyLogoMimeType.Contains(anyStringContains)
			       || x.addressLine1.Contains(anyStringContains)
			       || x.addressLine2.Contains(anyStringContains)
			       || x.addressLine3.Contains(anyStringContains)
			       || x.city.Contains(anyStringContains)
			       || x.postalCode.Contains(anyStringContains)
			       || x.phoneNumber.Contains(anyStringContains)
			       || x.email.Contains(anyStringContains)
			       || x.website.Contains(anyStringContains)
			       || x.primaryColor.Contains(anyStringContains)
			       || x.secondaryColor.Contains(anyStringContains)
			       || (includeRelations == true && x.country.name.Contains(anyStringContains))
			       || (includeRelations == true && x.country.description.Contains(anyStringContains))
			       || (includeRelations == true && x.country.abbreviation.Contains(anyStringContains))
			       || (includeRelations == true && x.country.postalCodeFormat.Contains(anyStringContains))
			       || (includeRelations == true && x.country.postalCodeRegEx.Contains(anyStringContains))
			       || (includeRelations == true && x.stateProvince.name.Contains(anyStringContains))
			       || (includeRelations == true && x.stateProvince.description.Contains(anyStringContains))
			       || (includeRelations == true && x.stateProvince.abbreviation.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.name.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.description.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.ianaTimeZone.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.abbreviation.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.abbreviationDaylightSavings.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.TenantProfile> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.TenantProfile tenantProfile in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(tenantProfile, databaseStoresDateWithTimeZone);
			}


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			if (diskBasedBinaryStorageMode == true)
			{
				var tasks = materialized.Select(async tenantProfile =>
				{

					if (tenantProfile.companyLogoData == null &&
					    tenantProfile.companyLogoSize.HasValue == true &&
					    tenantProfile.companyLogoSize.Value > 0)
					{
					    tenantProfile.companyLogoData = await LoadDataFromDiskAsync(tenantProfile.objectGuid, tenantProfile.versionNumber, "data");
					}

				}).ToList();

				// Run tasks concurrently and await their completion
				await Task.WhenAll(tasks);
			}

			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.TenantProfile Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.TenantProfile Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of TenantProfiles filtered by the parameters provided.  Its query is similar to the GetTenantProfiles method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TenantProfiles/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			string companyLogoFileName = null,
			long? companyLogoSize = null,
			string companyLogoMimeType = null,
			string addressLine1 = null,
			string addressLine2 = null,
			string addressLine3 = null,
			string city = null,
			string postalCode = null,
			int? stateProvinceId = null,
			int? countryId = null,
			int? timeZoneId = null,
			string phoneNumber = null,
			string email = null,
			string website = null,
			double? latitude = null,
			double? longitude = null,
			string primaryColor = null,
			string secondaryColor = null,
			bool? displaysMetric = null,
			bool? displaysUSTerms = null,
			int? versionNumber = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			IQueryable<Database.TenantProfile> query = (from tp in _context.TenantProfiles select tp);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(tp => tp.name == name);
			}
			if (description != null)
			{
				query = query.Where(tp => tp.description == description);
			}
			if (companyLogoFileName != null)
			{
				query = query.Where(tp => tp.companyLogoFileName == companyLogoFileName);
			}
			if (companyLogoSize.HasValue == true)
			{
				query = query.Where(tp => tp.companyLogoSize == companyLogoSize.Value);
			}
			if (companyLogoMimeType != null)
			{
				query = query.Where(tp => tp.companyLogoMimeType == companyLogoMimeType);
			}
			if (addressLine1 != null)
			{
				query = query.Where(tp => tp.addressLine1 == addressLine1);
			}
			if (addressLine2 != null)
			{
				query = query.Where(tp => tp.addressLine2 == addressLine2);
			}
			if (addressLine3 != null)
			{
				query = query.Where(tp => tp.addressLine3 == addressLine3);
			}
			if (city != null)
			{
				query = query.Where(tp => tp.city == city);
			}
			if (postalCode != null)
			{
				query = query.Where(tp => tp.postalCode == postalCode);
			}
			if (stateProvinceId.HasValue == true)
			{
				query = query.Where(tp => tp.stateProvinceId == stateProvinceId.Value);
			}
			if (countryId.HasValue == true)
			{
				query = query.Where(tp => tp.countryId == countryId.Value);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(tp => tp.timeZoneId == timeZoneId.Value);
			}
			if (phoneNumber != null)
			{
				query = query.Where(tp => tp.phoneNumber == phoneNumber);
			}
			if (email != null)
			{
				query = query.Where(tp => tp.email == email);
			}
			if (website != null)
			{
				query = query.Where(tp => tp.website == website);
			}
			if (latitude.HasValue == true)
			{
				query = query.Where(tp => tp.latitude == latitude.Value);
			}
			if (longitude.HasValue == true)
			{
				query = query.Where(tp => tp.longitude == longitude.Value);
			}
			if (primaryColor != null)
			{
				query = query.Where(tp => tp.primaryColor == primaryColor);
			}
			if (secondaryColor != null)
			{
				query = query.Where(tp => tp.secondaryColor == secondaryColor);
			}
			if (displaysMetric.HasValue == true)
			{
				query = query.Where(tp => tp.displaysMetric == displaysMetric.Value);
			}
			if (displaysUSTerms.HasValue == true)
			{
				query = query.Where(tp => tp.displaysUSTerms == displaysUSTerms.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(tp => tp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(tp => tp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(tp => tp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(tp => tp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(tp => tp.deleted == false);
				}
			}
			else
			{
				query = query.Where(tp => tp.active == true);
				query = query.Where(tp => tp.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Tenant Profile, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.companyLogoFileName.Contains(anyStringContains)
			       || x.companyLogoMimeType.Contains(anyStringContains)
			       || x.addressLine1.Contains(anyStringContains)
			       || x.addressLine2.Contains(anyStringContains)
			       || x.addressLine3.Contains(anyStringContains)
			       || x.city.Contains(anyStringContains)
			       || x.postalCode.Contains(anyStringContains)
			       || x.phoneNumber.Contains(anyStringContains)
			       || x.email.Contains(anyStringContains)
			       || x.website.Contains(anyStringContains)
			       || x.primaryColor.Contains(anyStringContains)
			       || x.secondaryColor.Contains(anyStringContains)
			       || x.country.name.Contains(anyStringContains)
			       || x.country.description.Contains(anyStringContains)
			       || x.country.abbreviation.Contains(anyStringContains)
			       || x.country.postalCodeFormat.Contains(anyStringContains)
			       || x.country.postalCodeRegEx.Contains(anyStringContains)
			       || x.stateProvince.name.Contains(anyStringContains)
			       || x.stateProvince.description.Contains(anyStringContains)
			       || x.stateProvince.abbreviation.Contains(anyStringContains)
			       || x.timeZone.name.Contains(anyStringContains)
			       || x.timeZone.description.Contains(anyStringContains)
			       || x.timeZone.ianaTimeZone.Contains(anyStringContains)
			       || x.timeZone.abbreviation.Contains(anyStringContains)
			       || x.timeZone.abbreviationDaylightSavings.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single TenantProfile by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TenantProfile/{id}")]
		public async Task<IActionResult> GetTenantProfile(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
			
			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			try
			{
				IQueryable<Database.TenantProfile> query = (from tp in _context.TenantProfiles where
							(tp.id == id) &&
							(userIsAdmin == true || tp.deleted == false) &&
							(userIsWriter == true || tp.active == true)
					select tp);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.country);
					query = query.Include(x => x.stateProvince);
					query = query.Include(x => x.timeZone);
					query = query.AsSplitQuery();
				}

				Database.TenantProfile materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

					if (diskBasedBinaryStorageMode == true &&
					    materialized.companyLogoData == null &&
					    materialized.companyLogoSize.HasValue == true &&
					    materialized.companyLogoSize.Value > 0)
					{
					    materialized.companyLogoData = await LoadDataFromDiskAsync(materialized.objectGuid, materialized.versionNumber, "data", cancellationToken);
					}

					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.TenantProfile Entity was read with Admin privilege." : "Scheduler.TenantProfile Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TenantProfile", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.TenantProfile entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.TenantProfile.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.TenantProfile.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing TenantProfile record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/TenantProfile/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutTenantProfile(int id, [FromBody]Database.TenantProfile.TenantProfileDTO tenantProfileDTO, CancellationToken cancellationToken = default)
		{
			if (tenantProfileDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != tenantProfileDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			IQueryable<Database.TenantProfile> query = (from x in _context.TenantProfiles
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.TenantProfile existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.TenantProfile PUT", id.ToString(), new Exception("No Scheduler.TenantProfile entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (tenantProfileDTO.objectGuid == Guid.Empty)
            {
                tenantProfileDTO.objectGuid = existing.objectGuid;
            }
            else if (tenantProfileDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a TenantProfile record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.TenantProfile cloneOfExisting = (Database.TenantProfile)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new TenantProfile object using the data from the existing record, updated with what is in the DTO.
			//
			Database.TenantProfile tenantProfile = (Database.TenantProfile)_context.Entry(existing).GetDatabaseValues().ToObject();
			tenantProfile.ApplyDTO(tenantProfileDTO);
			//
			// The tenant guid for any TenantProfile being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the TenantProfile because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				tenantProfile.tenantGuid = existing.tenantGuid;
			}

			lock (tenantProfilePutSyncRoot)
			{
				//
				// Validate the version number for the tenantProfile being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != tenantProfile.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "TenantProfile save attempt was made but save request was with version " + tenantProfile.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The TenantProfile you are trying to update has already changed.  Please try your save again after reloading the TenantProfile.");
				}
				else
				{
					// Same record.  Increase version.
					tenantProfile.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (tenantProfile.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.TenantProfile record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (tenantProfile.name != null && tenantProfile.name.Length > 100)
				{
					tenantProfile.name = tenantProfile.name.Substring(0, 100);
				}

				if (tenantProfile.description != null && tenantProfile.description.Length > 500)
				{
					tenantProfile.description = tenantProfile.description.Substring(0, 500);
				}

				if (tenantProfile.companyLogoFileName != null && tenantProfile.companyLogoFileName.Length > 250)
				{
					tenantProfile.companyLogoFileName = tenantProfile.companyLogoFileName.Substring(0, 250);
				}

				if (tenantProfile.companyLogoMimeType != null && tenantProfile.companyLogoMimeType.Length > 100)
				{
					tenantProfile.companyLogoMimeType = tenantProfile.companyLogoMimeType.Substring(0, 100);
				}

				if (tenantProfile.addressLine1 != null && tenantProfile.addressLine1.Length > 250)
				{
					tenantProfile.addressLine1 = tenantProfile.addressLine1.Substring(0, 250);
				}

				if (tenantProfile.addressLine2 != null && tenantProfile.addressLine2.Length > 250)
				{
					tenantProfile.addressLine2 = tenantProfile.addressLine2.Substring(0, 250);
				}

				if (tenantProfile.addressLine3 != null && tenantProfile.addressLine3.Length > 250)
				{
					tenantProfile.addressLine3 = tenantProfile.addressLine3.Substring(0, 250);
				}

				if (tenantProfile.city != null && tenantProfile.city.Length > 100)
				{
					tenantProfile.city = tenantProfile.city.Substring(0, 100);
				}

				if (tenantProfile.postalCode != null && tenantProfile.postalCode.Length > 100)
				{
					tenantProfile.postalCode = tenantProfile.postalCode.Substring(0, 100);
				}

				if (tenantProfile.phoneNumber != null && tenantProfile.phoneNumber.Length > 100)
				{
					tenantProfile.phoneNumber = tenantProfile.phoneNumber.Substring(0, 100);
				}

				if (tenantProfile.email != null && tenantProfile.email.Length > 250)
				{
					tenantProfile.email = tenantProfile.email.Substring(0, 250);
				}

				if (tenantProfile.primaryColor != null && tenantProfile.primaryColor.Length > 10)
				{
					tenantProfile.primaryColor = tenantProfile.primaryColor.Substring(0, 10);
				}

				if (tenantProfile.secondaryColor != null && tenantProfile.secondaryColor.Length > 10)
				{
					tenantProfile.secondaryColor = tenantProfile.secondaryColor.Substring(0, 10);
				}


				//
				// Add default values for any missing data attribute fields.
				//
				if (tenantProfile.companyLogoData != null && string.IsNullOrEmpty(tenantProfile.companyLogoFileName))
				{
				    tenantProfile.companyLogoFileName = tenantProfile.objectGuid.ToString() + ".data";
				}

				if (tenantProfile.companyLogoData != null && (tenantProfile.companyLogoSize.HasValue == false || tenantProfile.companyLogoSize != tenantProfile.companyLogoData.Length))
				{
				    tenantProfile.companyLogoSize = tenantProfile.companyLogoData.Length;
				}

				if (tenantProfile.companyLogoData != null && string.IsNullOrEmpty(tenantProfile.companyLogoMimeType))
				{
				    tenantProfile.companyLogoMimeType = "application/octet-stream";
				}

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				try
				{
					byte[] dataReferenceBeforeClearing = tenantProfile.companyLogoData;

					if (diskBasedBinaryStorageMode == true &&
					    tenantProfile.companyLogoFileName != null &&
					    tenantProfile.companyLogoData != null &&
					    tenantProfile.companyLogoSize.HasValue == true &&
					    tenantProfile.companyLogoSize.Value > 0)
					{
					    //
					    // write the bytes to disk
					    //
					    WriteDataToDisk(tenantProfile.objectGuid, tenantProfile.versionNumber, tenantProfile.companyLogoData, "data");

					    //
					    // Clear the data from the object before we put it into the db
					    //
					    tenantProfile.companyLogoData = null;

					}

				    EntityEntry<Database.TenantProfile> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(tenantProfile);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        TenantProfileChangeHistory tenantProfileChangeHistory = new TenantProfileChangeHistory();
				        tenantProfileChangeHistory.tenantProfileId = tenantProfile.id;
				        tenantProfileChangeHistory.versionNumber = tenantProfile.versionNumber;
				        tenantProfileChangeHistory.timeStamp = DateTime.UtcNow;
				        tenantProfileChangeHistory.userId = securityUser.id;
				        tenantProfileChangeHistory.tenantGuid = userTenantGuid;
				        tenantProfileChangeHistory.data = JsonSerializer.Serialize(Database.TenantProfile.CreateAnonymousWithFirstLevelSubObjects(tenantProfile));
				        _context.TenantProfileChangeHistories.Add(tenantProfileChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    tenantProfile.companyLogoData = dataReferenceBeforeClearing;
					}

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.TenantProfile entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.TenantProfile.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.TenantProfile.CreateAnonymousWithFirstLevelSubObjects(tenantProfile)),
						null);

				return Ok(Database.TenantProfile.CreateAnonymous(tenantProfile));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.TenantProfile entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.TenantProfile.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.TenantProfile.CreateAnonymousWithFirstLevelSubObjects(tenantProfile)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new TenantProfile record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TenantProfile", Name = "TenantProfile")]
		public async Task<IActionResult> PostTenantProfile([FromBody]Database.TenantProfile.TenantProfileDTO tenantProfileDTO, CancellationToken cancellationToken = default)
		{
			if (tenantProfileDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}

			//
			// Create a new TenantProfile object using the data from the DTO
			//
			Database.TenantProfile tenantProfile = Database.TenantProfile.FromDTO(tenantProfileDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				tenantProfile.tenantGuid = userTenantGuid;

				if (tenantProfile.name != null && tenantProfile.name.Length > 100)
				{
					tenantProfile.name = tenantProfile.name.Substring(0, 100);
				}

				if (tenantProfile.description != null && tenantProfile.description.Length > 500)
				{
					tenantProfile.description = tenantProfile.description.Substring(0, 500);
				}

				if (tenantProfile.companyLogoFileName != null && tenantProfile.companyLogoFileName.Length > 250)
				{
					tenantProfile.companyLogoFileName = tenantProfile.companyLogoFileName.Substring(0, 250);
				}

				if (tenantProfile.companyLogoMimeType != null && tenantProfile.companyLogoMimeType.Length > 100)
				{
					tenantProfile.companyLogoMimeType = tenantProfile.companyLogoMimeType.Substring(0, 100);
				}

				if (tenantProfile.addressLine1 != null && tenantProfile.addressLine1.Length > 250)
				{
					tenantProfile.addressLine1 = tenantProfile.addressLine1.Substring(0, 250);
				}

				if (tenantProfile.addressLine2 != null && tenantProfile.addressLine2.Length > 250)
				{
					tenantProfile.addressLine2 = tenantProfile.addressLine2.Substring(0, 250);
				}

				if (tenantProfile.addressLine3 != null && tenantProfile.addressLine3.Length > 250)
				{
					tenantProfile.addressLine3 = tenantProfile.addressLine3.Substring(0, 250);
				}

				if (tenantProfile.city != null && tenantProfile.city.Length > 100)
				{
					tenantProfile.city = tenantProfile.city.Substring(0, 100);
				}

				if (tenantProfile.postalCode != null && tenantProfile.postalCode.Length > 100)
				{
					tenantProfile.postalCode = tenantProfile.postalCode.Substring(0, 100);
				}

				if (tenantProfile.phoneNumber != null && tenantProfile.phoneNumber.Length > 100)
				{
					tenantProfile.phoneNumber = tenantProfile.phoneNumber.Substring(0, 100);
				}

				if (tenantProfile.email != null && tenantProfile.email.Length > 250)
				{
					tenantProfile.email = tenantProfile.email.Substring(0, 250);
				}

				if (tenantProfile.primaryColor != null && tenantProfile.primaryColor.Length > 10)
				{
					tenantProfile.primaryColor = tenantProfile.primaryColor.Substring(0, 10);
				}

				if (tenantProfile.secondaryColor != null && tenantProfile.secondaryColor.Length > 10)
				{
					tenantProfile.secondaryColor = tenantProfile.secondaryColor.Substring(0, 10);
				}

				tenantProfile.objectGuid = Guid.NewGuid();

				//
				// Add default values for any missing data attribute fields.
				//
				if (tenantProfile.companyLogoData != null && string.IsNullOrEmpty(tenantProfile.companyLogoFileName))
				{
				    tenantProfile.companyLogoFileName = tenantProfile.objectGuid.ToString() + ".data";
				}

				if (tenantProfile.companyLogoData != null && (tenantProfile.companyLogoSize.HasValue == false || tenantProfile.companyLogoSize != tenantProfile.companyLogoData.Length))
				{
				    tenantProfile.companyLogoSize = tenantProfile.companyLogoData.Length;
				}

				if (tenantProfile.companyLogoData != null && string.IsNullOrEmpty(tenantProfile.companyLogoMimeType))
				{
				    tenantProfile.companyLogoMimeType = "application/octet-stream";
				}

				tenantProfile.versionNumber = 1;

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				byte[] dataReferenceBeforeClearing = tenantProfile.companyLogoData;

				if (diskBasedBinaryStorageMode == true &&
				    tenantProfile.companyLogoData != null &&
				    tenantProfile.companyLogoFileName != null &&
				    tenantProfile.companyLogoSize.HasValue == true &&
				    tenantProfile.companyLogoSize.Value > 0)
				{
				    //
				    // write the bytes to disk
				    //
				    await WriteDataToDiskAsync(tenantProfile.objectGuid, tenantProfile.versionNumber, tenantProfile.companyLogoData, "data", cancellationToken);

				    //
				    // Clear the data from the object before we put it into the db
				    //
				    tenantProfile.companyLogoData = null;

				}

				_context.TenantProfiles.Add(tenantProfile);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the tenantProfile object so that no further changes will be written to the database
				    //
				    _context.Entry(tenantProfile).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					tenantProfile.companyLogoData = null;
					tenantProfile.TenantProfileChangeHistories = null;
					tenantProfile.country = null;
					tenantProfile.stateProvince = null;
					tenantProfile.timeZone = null;


				    TenantProfileChangeHistory tenantProfileChangeHistory = new TenantProfileChangeHistory();
				    tenantProfileChangeHistory.tenantProfileId = tenantProfile.id;
				    tenantProfileChangeHistory.versionNumber = tenantProfile.versionNumber;
				    tenantProfileChangeHistory.timeStamp = DateTime.UtcNow;
				    tenantProfileChangeHistory.userId = securityUser.id;
				    tenantProfileChangeHistory.tenantGuid = userTenantGuid;
				    tenantProfileChangeHistory.data = JsonSerializer.Serialize(Database.TenantProfile.CreateAnonymousWithFirstLevelSubObjects(tenantProfile));
				    _context.TenantProfileChangeHistories.Add(tenantProfileChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.TenantProfile entity successfully created.",
						true,
						tenantProfile. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.TenantProfile.CreateAnonymousWithFirstLevelSubObjects(tenantProfile)),
						null);



					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    tenantProfile.companyLogoData = dataReferenceBeforeClearing;
					}

				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.TenantProfile entity creation failed.", false, tenantProfile.id.ToString(), "", JsonSerializer.Serialize(Database.TenantProfile.CreateAnonymousWithFirstLevelSubObjects(tenantProfile)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TenantProfile", tenantProfile.id, tenantProfile.name));

			return CreatedAtRoute("TenantProfile", new { id = tenantProfile.id }, Database.TenantProfile.CreateAnonymousWithFirstLevelSubObjects(tenantProfile));
		}



        /// <summary>
        /// 
        /// This rolls a TenantProfile entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TenantProfile/Rollback/{id}")]
		[Route("api/TenantProfile/Rollback")]
		public async Task<IActionResult> RollbackToTenantProfileVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
		{
			//
			// Data rollback is an admin only function, like Deletes.
			//
			StartAuditEventClock();
			
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			
			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
			
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);

			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}

			

			
			IQueryable <Database.TenantProfile> query = (from x in _context.TenantProfiles
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();


			//
			// Make sure nobody else is editing this TenantProfile concurrently
			//
			lock (tenantProfilePutSyncRoot)
			{
				
				Database.TenantProfile tenantProfile = query.FirstOrDefault();
				
				if (tenantProfile == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.TenantProfile rollback", id.ToString(), new Exception("No Scheduler.TenantProfile entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the TenantProfile current state so we can log it.
				//
				Database.TenantProfile cloneOfExisting = (Database.TenantProfile)_context.Entry(tenantProfile).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.companyLogoData = null;
				cloneOfExisting.TenantProfileChangeHistories = null;
				cloneOfExisting.country = null;
				cloneOfExisting.stateProvince = null;
				cloneOfExisting.timeZone = null;

				if (versionNumber >= tenantProfile.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.TenantProfile rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.TenantProfile rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				TenantProfileChangeHistory tenantProfileChangeHistory = (from x in _context.TenantProfileChangeHistories
				                                               where
				                                               x.tenantProfileId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (tenantProfileChangeHistory != null)
				{
				    Database.TenantProfile oldTenantProfile = JsonSerializer.Deserialize<Database.TenantProfile>(tenantProfileChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    tenantProfile.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    tenantProfile.name = oldTenantProfile.name;
				    tenantProfile.description = oldTenantProfile.description;
				    tenantProfile.companyLogoFileName = oldTenantProfile.companyLogoFileName;
				    tenantProfile.companyLogoSize = oldTenantProfile.companyLogoSize;
				    tenantProfile.companyLogoData = oldTenantProfile.companyLogoData;
				    tenantProfile.companyLogoMimeType = oldTenantProfile.companyLogoMimeType;
				    tenantProfile.addressLine1 = oldTenantProfile.addressLine1;
				    tenantProfile.addressLine2 = oldTenantProfile.addressLine2;
				    tenantProfile.addressLine3 = oldTenantProfile.addressLine3;
				    tenantProfile.city = oldTenantProfile.city;
				    tenantProfile.postalCode = oldTenantProfile.postalCode;
				    tenantProfile.stateProvinceId = oldTenantProfile.stateProvinceId;
				    tenantProfile.countryId = oldTenantProfile.countryId;
				    tenantProfile.timeZoneId = oldTenantProfile.timeZoneId;
				    tenantProfile.phoneNumber = oldTenantProfile.phoneNumber;
				    tenantProfile.email = oldTenantProfile.email;
				    tenantProfile.website = oldTenantProfile.website;
				    tenantProfile.latitude = oldTenantProfile.latitude;
				    tenantProfile.longitude = oldTenantProfile.longitude;
				    tenantProfile.primaryColor = oldTenantProfile.primaryColor;
				    tenantProfile.secondaryColor = oldTenantProfile.secondaryColor;
				    tenantProfile.displaysMetric = oldTenantProfile.displaysMetric;
				    tenantProfile.displaysUSTerms = oldTenantProfile.displaysUSTerms;
				    tenantProfile.objectGuid = oldTenantProfile.objectGuid;
				    tenantProfile.active = oldTenantProfile.active;
				    tenantProfile.deleted = oldTenantProfile.deleted;
				    //
				    // If disk based binary mode is on, then we need to copy the old data file over as well.
				    //
				    if (diskBasedBinaryStorageMode == true)
				    {
				    	Byte[] binaryData = LoadDataFromDisk(oldTenantProfile.objectGuid, oldTenantProfile.versionNumber, "data");

				    	//
				    	// Write out the data as the new version
				    	//
				    	WriteDataToDisk(tenantProfile.objectGuid, tenantProfile.versionNumber, binaryData, "data");
				    }

				    string serializedTenantProfile = JsonSerializer.Serialize(tenantProfile);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        TenantProfileChangeHistory newTenantProfileChangeHistory = new TenantProfileChangeHistory();
				        newTenantProfileChangeHistory.tenantProfileId = tenantProfile.id;
				        newTenantProfileChangeHistory.versionNumber = tenantProfile.versionNumber;
				        newTenantProfileChangeHistory.timeStamp = DateTime.UtcNow;
				        newTenantProfileChangeHistory.userId = securityUser.id;
				        newTenantProfileChangeHistory.tenantGuid = userTenantGuid;
				        newTenantProfileChangeHistory.data = JsonSerializer.Serialize(Database.TenantProfile.CreateAnonymousWithFirstLevelSubObjects(tenantProfile));
				        _context.TenantProfileChangeHistories.Add(newTenantProfileChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.TenantProfile rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.TenantProfile.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.TenantProfile.CreateAnonymousWithFirstLevelSubObjects(tenantProfile)),
						null);


				    return Ok(Database.TenantProfile.CreateAnonymous(tenantProfile));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.TenantProfile rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.TenantProfile rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a TenantProfile.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the TenantProfile</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TenantProfile/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetTenantProfileChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
				return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			Database.TenantProfile tenantProfile = await _context.TenantProfiles.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (tenantProfile == null)
			{
				return NotFound();
			}

			try
			{
				tenantProfile.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.TenantProfile> versionInfo = await tenantProfile.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

				if (versionInfo == null)
				{
					return NotFound($"Version {versionNumber} not found.");
				}

				return Ok(versionInfo);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets the full audit history for a TenantProfile.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the TenantProfile</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TenantProfile/{id}/AuditHistory")]
		public async Task<IActionResult> GetTenantProfileAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
				return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			Database.TenantProfile tenantProfile = await _context.TenantProfiles.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (tenantProfile == null)
			{
				return NotFound();
			}

			try
			{
				tenantProfile.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.TenantProfile>> versions = await tenantProfile.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a TenantProfile.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the TenantProfile</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The TenantProfile object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TenantProfile/{id}/Version/{version}")]
		public async Task<IActionResult> GetTenantProfileVersion(int id, int version, CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
				return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			Database.TenantProfile tenantProfile = await _context.TenantProfiles.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (tenantProfile == null)
			{
				return NotFound();
			}

			try
			{
				tenantProfile.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.TenantProfile> versionInfo = await tenantProfile.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

				if (versionInfo == null || versionInfo.data == null)
				{
					return NotFound();
				}

				return Ok(versionInfo.data.ToOutputDTO());
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets the state of a TenantProfile at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the TenantProfile</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The TenantProfile object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TenantProfile/{id}/StateAtTime")]
		public async Task<IActionResult> GetTenantProfileStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
				return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			Database.TenantProfile tenantProfile = await _context.TenantProfiles.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (tenantProfile == null)
			{
				return NotFound();
			}

			try
			{
				tenantProfile.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.TenantProfile> versionInfo = await tenantProfile.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

				if (versionInfo == null || versionInfo.data == null)
				{
					return NotFound("No state found at specified time.");
				}

				return Ok(versionInfo.data.ToOutputDTO());
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}

        /// <summary>
        /// 
        /// This deletes a TenantProfile record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TenantProfile/{id}")]
		[Route("api/TenantProfile")]
		public async Task<IActionResult> DeleteTenantProfile(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(cancellationToken);
			
			
			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}

			IQueryable<Database.TenantProfile> query = (from x in _context.TenantProfiles
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.TenantProfile tenantProfile = await query.FirstOrDefaultAsync(cancellationToken);

			if (tenantProfile == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.TenantProfile DELETE", id.ToString(), new Exception("No Scheduler.TenantProfile entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.TenantProfile cloneOfExisting = (Database.TenantProfile)_context.Entry(tenantProfile).GetDatabaseValues().ToObject();


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			lock (tenantProfileDeleteSyncRoot)
			{
			    try
			    {
			        tenantProfile.deleted = true;
			        tenantProfile.versionNumber++;

			        _context.SaveChanges();

			        //
			        // If in disk based storage mode, create a copy of the disk data file for the new version.
			        //
			        if (diskBasedBinaryStorageMode == true)
			        {
			        	Byte[] binaryData = LoadDataFromDisk(tenantProfile.objectGuid, tenantProfile.versionNumber -1, "data");

			        	//
			        	// Write out the same data
			        	//
			        	WriteDataToDisk(tenantProfile.objectGuid, tenantProfile.versionNumber, binaryData, "data");
			        }

			        //
			        // Now add the change history
			        //
			        TenantProfileChangeHistory tenantProfileChangeHistory = new TenantProfileChangeHistory();
			        tenantProfileChangeHistory.tenantProfileId = tenantProfile.id;
			        tenantProfileChangeHistory.versionNumber = tenantProfile.versionNumber;
			        tenantProfileChangeHistory.timeStamp = DateTime.UtcNow;
			        tenantProfileChangeHistory.userId = securityUser.id;
			        tenantProfileChangeHistory.tenantGuid = userTenantGuid;
			        tenantProfileChangeHistory.data = JsonSerializer.Serialize(Database.TenantProfile.CreateAnonymousWithFirstLevelSubObjects(tenantProfile));
			        _context.TenantProfileChangeHistories.Add(tenantProfileChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.TenantProfile entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.TenantProfile.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.TenantProfile.CreateAnonymousWithFirstLevelSubObjects(tenantProfile)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.TenantProfile entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.TenantProfile.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.TenantProfile.CreateAnonymousWithFirstLevelSubObjects(tenantProfile)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of TenantProfile records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/TenantProfiles/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			string companyLogoFileName = null,
			long? companyLogoSize = null,
			string companyLogoMimeType = null,
			string addressLine1 = null,
			string addressLine2 = null,
			string addressLine3 = null,
			string city = null,
			string postalCode = null,
			int? stateProvinceId = null,
			int? countryId = null,
			int? timeZoneId = null,
			string phoneNumber = null,
			string email = null,
			string website = null,
			double? latitude = null,
			double? longitude = null,
			string primaryColor = null,
			string secondaryColor = null,
			bool? displaysMetric = null,
			bool? displaysUSTerms = null,
			int? versionNumber = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);

			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);

			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


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

			IQueryable<Database.TenantProfile> query = (from tp in _context.TenantProfiles select tp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(tp => tp.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(tp => tp.description == description);
			}
			if (string.IsNullOrEmpty(companyLogoFileName) == false)
			{
				query = query.Where(tp => tp.companyLogoFileName == companyLogoFileName);
			}
			if (companyLogoSize.HasValue == true)
			{
				query = query.Where(tp => tp.companyLogoSize == companyLogoSize.Value);
			}
			if (string.IsNullOrEmpty(companyLogoMimeType) == false)
			{
				query = query.Where(tp => tp.companyLogoMimeType == companyLogoMimeType);
			}
			if (string.IsNullOrEmpty(addressLine1) == false)
			{
				query = query.Where(tp => tp.addressLine1 == addressLine1);
			}
			if (string.IsNullOrEmpty(addressLine2) == false)
			{
				query = query.Where(tp => tp.addressLine2 == addressLine2);
			}
			if (string.IsNullOrEmpty(addressLine3) == false)
			{
				query = query.Where(tp => tp.addressLine3 == addressLine3);
			}
			if (string.IsNullOrEmpty(city) == false)
			{
				query = query.Where(tp => tp.city == city);
			}
			if (string.IsNullOrEmpty(postalCode) == false)
			{
				query = query.Where(tp => tp.postalCode == postalCode);
			}
			if (stateProvinceId.HasValue == true)
			{
				query = query.Where(tp => tp.stateProvinceId == stateProvinceId.Value);
			}
			if (countryId.HasValue == true)
			{
				query = query.Where(tp => tp.countryId == countryId.Value);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(tp => tp.timeZoneId == timeZoneId.Value);
			}
			if (string.IsNullOrEmpty(phoneNumber) == false)
			{
				query = query.Where(tp => tp.phoneNumber == phoneNumber);
			}
			if (string.IsNullOrEmpty(email) == false)
			{
				query = query.Where(tp => tp.email == email);
			}
			if (string.IsNullOrEmpty(website) == false)
			{
				query = query.Where(tp => tp.website == website);
			}
			if (string.IsNullOrEmpty(primaryColor) == false)
			{
				query = query.Where(tp => tp.primaryColor == primaryColor);
			}
			if (string.IsNullOrEmpty(secondaryColor) == false)
			{
				query = query.Where(tp => tp.secondaryColor == secondaryColor);
			}
			if (displaysMetric.HasValue == true)
			{
				query = query.Where(tp => tp.displaysMetric == displaysMetric.Value);
			}
			if (displaysUSTerms.HasValue == true)
			{
				query = query.Where(tp => tp.displaysUSTerms == displaysUSTerms.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(tp => tp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(tp => tp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(tp => tp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(tp => tp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(tp => tp.deleted == false);
				}
			}
			else
			{
				query = query.Where(tp => tp.active == true);
				query = query.Where(tp => tp.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Tenant Profile, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.companyLogoFileName.Contains(anyStringContains)
			       || x.companyLogoMimeType.Contains(anyStringContains)
			       || x.addressLine1.Contains(anyStringContains)
			       || x.addressLine2.Contains(anyStringContains)
			       || x.addressLine3.Contains(anyStringContains)
			       || x.city.Contains(anyStringContains)
			       || x.postalCode.Contains(anyStringContains)
			       || x.phoneNumber.Contains(anyStringContains)
			       || x.email.Contains(anyStringContains)
			       || x.website.Contains(anyStringContains)
			       || x.primaryColor.Contains(anyStringContains)
			       || x.secondaryColor.Contains(anyStringContains)
			       || x.country.name.Contains(anyStringContains)
			       || x.country.description.Contains(anyStringContains)
			       || x.country.abbreviation.Contains(anyStringContains)
			       || x.country.postalCodeFormat.Contains(anyStringContains)
			       || x.country.postalCodeRegEx.Contains(anyStringContains)
			       || x.stateProvince.name.Contains(anyStringContains)
			       || x.stateProvince.description.Contains(anyStringContains)
			       || x.stateProvince.abbreviation.Contains(anyStringContains)
			       || x.timeZone.name.Contains(anyStringContains)
			       || x.timeZone.description.Contains(anyStringContains)
			       || x.timeZone.ianaTimeZone.Contains(anyStringContains)
			       || x.timeZone.abbreviation.Contains(anyStringContains)
			       || x.timeZone.abbreviationDaylightSavings.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.companyLogoFileName);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.TenantProfile.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/TenantProfile/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
			{
			   return Forbid();
			}

		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


        /// <summary>
        /// 
        /// This makes a TenantProfile record a favourite for the current user.
		/// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/TenantProfile/Favourite/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPut]
		public async Task<IActionResult> SetFavourite(int id, string description = null, CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(cancellationToken);
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(cancellationToken);

			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			IQueryable<Database.TenantProfile> query = (from x in _context.TenantProfiles
			                               where x.id == id
			                               select x);


			Database.TenantProfile tenantProfile = await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

			if (tenantProfile != null)
			{
				if (string.IsNullOrEmpty(description) == true)
				{
					description = tenantProfile.name;
				}

				//
				// Add the user favourite TenantProfile
				//
				await SecurityLogic.AddToUserFavouritesAsync(securityUser, "TenantProfile", id, description, cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'TenantProfile' was added for record with id of " + id + " for user " + securityUser.accountName, true);

				//
				// Return the complete list of user favourites after the addition
				//
				return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
			}
			else
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'TenantProfile' add request was made with an invalid id value of " + id, false);
				return BadRequest();
			}
		}


        /// <summary>
        /// 
        /// This removes a TenantProfile record from the current user's favourites.
        /// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/TenantProfile/Favourite/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpDelete]
		public async Task<IActionResult> DeleteFavourite(int id, CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			//
			// Delete the user favourite TenantProfile
			//
			await SecurityLogic.RemoveFromUserFavouritesAsync(securityUser, "TenantProfile", id, cancellationToken);

			await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'TenantProfile' was removed for record with id of " + id + " for user " + securityUser.accountName, true);

			//
			// Return the complete list of user favourites after the deletion
			//
			return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
		}




        [Route("api/TenantProfile/Data/{id:int}")]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [HttpPost]
        [HttpPut]
        public async Task<IActionResult> UploadData(int id, CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			MediaTypeHeaderValue mediaTypeHeader; 

            if (!HttpContext.Request.HasFormContentType ||
				!MediaTypeHeaderValue.TryParse(HttpContext.Request.ContentType, out mediaTypeHeader) ||
                string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
            {
                return new UnsupportedMediaTypeResult();
            }


            Database.TenantProfile tenantProfile = await (from x in _context.TenantProfiles where x.id == id && x.active == true && x.deleted == false select x).FirstOrDefaultAsync();
            if (tenantProfile == null)
            {
                return NotFound();
            }

            bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();


            // This will be used to signal whether we are saving data or clearing it.
            bool foundFileData = false;


            //
            // This will get the first file from the request and save it
            //
			try
			{
                MultipartReader reader = new MultipartReader(mediaTypeHeader.Boundary.Value, HttpContext.Request.Body);
                MultipartSection section = await reader.ReadNextSectionAsync();

                while (section != null)
				{
					ContentDispositionHeaderValue contentDisposition;

					bool hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);


					if (hasContentDispositionHeader && contentDisposition.DispositionType.Equals("form-data") &&
						!string.IsNullOrEmpty(contentDisposition.FileName.Value))
					{

						foundFileData = true;
						string fileName = contentDisposition.FileName.ToString().Trim('"');

						// default the mime type to be the one for arbitrary binary data unless we have a mime type on the content headers that tells us otherwise.
						MediaTypeHeaderValue mediaType;
						bool hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);

						string mimeType = "application/octet-stream";
						if (hasMediaTypeHeader && mediaTypeHeader.MediaType != null )
						{
							mimeType = mediaTypeHeader.MediaType.ToString();
						}

						lock (tenantProfilePutSyncRoot)
						{
							try
							{
								using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
								{
									tenantProfile.companyLogoFileName = fileName.Trim();
									tenantProfile.companyLogoMimeType = mimeType;
									tenantProfile.companyLogoSize = section.Body.Length;

									tenantProfile.versionNumber++;

									if (diskBasedBinaryStorageMode == true &&
										 tenantProfile.companyLogoFileName != null &&
										 tenantProfile.companyLogoSize > 0)
									{
										//
										// write the bytes to disk
										//
										WriteDataToDisk(tenantProfile.objectGuid, tenantProfile.versionNumber, section.Body, "data");
										//
										// Clear the data from the object before we put it into the db
										//
										tenantProfile.companyLogoData = null;
									}
									else
									{
										using (MemoryStream memoryStream = new MemoryStream((int)section.Body.Length))
										{
											section.Body.CopyTo(memoryStream);
											tenantProfile.companyLogoData = memoryStream.ToArray();
										}
									}
									//
									// Now add the change history
									//
									TenantProfileChangeHistory tenantProfileChangeHistory = new TenantProfileChangeHistory();
									tenantProfileChangeHistory.tenantProfileId = tenantProfile.id;
									tenantProfileChangeHistory.versionNumber = tenantProfile.versionNumber;
									tenantProfileChangeHistory.timeStamp = DateTime.UtcNow;
									tenantProfileChangeHistory.userId = securityUser.id;
									tenantProfileChangeHistory.tenantGuid = tenantProfile.tenantGuid;
									tenantProfileChangeHistory.data = JsonSerializer.Serialize(Database.TenantProfile.CreateAnonymousWithFirstLevelSubObjects(tenantProfile));
									_context.TenantProfileChangeHistories.Add(tenantProfileChangeHistory);

									_context.SaveChanges();

									transaction.Commit();

									CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "TenantProfile Data Uploaded with filename of " + fileName + " and with size of " + section.Body.Length, id.ToString());
								}
							}
							catch (Exception ex)
							{
								CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "TenantProfile Data Upload Failed.", false, id.ToString(), "", "", ex);

								return Problem(ex.Message);
							}
						}


						//
						// Stop looking for more files.
						//
						break;
					}

					section = await reader.ReadNextSectionAsync();
				}
            }
            catch (Exception ex)
            {
                CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Caught error in UploadData handler", id.ToString(), ex);

                return Problem(ex.Message);
            }

            //
            // Treat the situation where we have a valid ID but no file content as a request to clear the data
            //
            if (foundFileData == false)
            {
                lock (tenantProfilePutSyncRoot)
                {
                    try
                    {
                        using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                        {
                            if (diskBasedBinaryStorageMode == true)
                            {
								DeleteDataFromDisk(tenantProfile.objectGuid, tenantProfile.versionNumber, "data");
                            }

                            tenantProfile.companyLogoFileName = null;
                            tenantProfile.companyLogoMimeType = null;
                            tenantProfile.companyLogoSize = 0;
                            tenantProfile.companyLogoData = null;
                            tenantProfile.versionNumber++;


                            //
                            // Now add the change history
                            //
                            TenantProfileChangeHistory tenantProfileChangeHistory = new TenantProfileChangeHistory();
                            tenantProfileChangeHistory.tenantProfileId = tenantProfile.id;
                            tenantProfileChangeHistory.versionNumber = tenantProfile.versionNumber;
                            tenantProfileChangeHistory.timeStamp = DateTime.UtcNow;
                            tenantProfileChangeHistory.userId = securityUser.id;
                                    tenantProfileChangeHistory.tenantGuid = tenantProfile.tenantGuid;
                                    tenantProfileChangeHistory.data = JsonSerializer.Serialize(Database.TenantProfile.CreateAnonymousWithFirstLevelSubObjects(tenantProfile));
                            _context.TenantProfileChangeHistories.Add(tenantProfileChangeHistory);

                            _context.SaveChanges();

                            transaction.Commit();

                            CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "TenantProfile data cleared.", id.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "TenantProfile data clear failed.", false, id.ToString(), "", "", ex);

                        return Problem(ex.Message);
                    }
                }
            }

            return Ok();
        }

        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/TenantProfile/Data/{id:int}")]
        public async Task<IActionResult> DownloadDataAsync(int id)
        {
             if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
             {
                 return Forbid();
             }


			using (SchedulerContext context = new SchedulerContext())
            {
                //
                // Return the data to the user as though it was a file.
                //
                Database.TenantProfile tenantProfile = await (from d in context.TenantProfiles
                                                where d.id == id &&
                                                d.active == true &&
                                                d.deleted == false
                                                select d).FirstOrDefaultAsync();

                if (tenantProfile != null && tenantProfile.companyLogoData != null)
                {
                   return File(tenantProfile.companyLogoData.ToArray<byte>(), tenantProfile.companyLogoMimeType, tenantProfile.companyLogoFileName != null ? tenantProfile.companyLogoFileName.Trim() : "TenantProfile_" + tenantProfile.id.ToString(), true);
                }
                else
                {
                    return BadRequest();
                }
            }
        }
	}
}
