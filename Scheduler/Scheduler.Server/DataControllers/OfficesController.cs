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
    /// This auto generated class provides the basic CRUD operations for the Office entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Office entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class OfficesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object officePutSyncRoot = new object();
		static object officeDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<OfficesController> _logger;

		public OfficesController(SchedulerContext context, ILogger<OfficesController> logger) : base("Scheduler", "Office")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Offices filtered by the parameters provided.
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
		[Route("api/Offices")]
		public async Task<IActionResult> GetOffices(
			string name = null,
			string description = null,
			int? officeTypeId = null,
			int? timeZoneId = null,
			int? currencyId = null,
			string addressLine1 = null,
			string addressLine2 = null,
			string city = null,
			string postalCode = null,
			int? stateProvinceId = null,
			int? countryId = null,
			string phone = null,
			string email = null,
			double? latitude = null,
			double? longitude = null,
			string notes = null,
			string externalId = null,
			string color = null,
			string attributes = null,
			string avatarFileName = null,
			long? avatarSize = null,
			string avatarMimeType = null,
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

			IQueryable<Database.Office> query = (from o in _context.Offices select o);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(o => o.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(o => o.description == description);
			}
			if (officeTypeId.HasValue == true)
			{
				query = query.Where(o => o.officeTypeId == officeTypeId.Value);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(o => o.timeZoneId == timeZoneId.Value);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(o => o.currencyId == currencyId.Value);
			}
			if (string.IsNullOrEmpty(addressLine1) == false)
			{
				query = query.Where(o => o.addressLine1 == addressLine1);
			}
			if (string.IsNullOrEmpty(addressLine2) == false)
			{
				query = query.Where(o => o.addressLine2 == addressLine2);
			}
			if (string.IsNullOrEmpty(city) == false)
			{
				query = query.Where(o => o.city == city);
			}
			if (string.IsNullOrEmpty(postalCode) == false)
			{
				query = query.Where(o => o.postalCode == postalCode);
			}
			if (stateProvinceId.HasValue == true)
			{
				query = query.Where(o => o.stateProvinceId == stateProvinceId.Value);
			}
			if (countryId.HasValue == true)
			{
				query = query.Where(o => o.countryId == countryId.Value);
			}
			if (string.IsNullOrEmpty(phone) == false)
			{
				query = query.Where(o => o.phone == phone);
			}
			if (string.IsNullOrEmpty(email) == false)
			{
				query = query.Where(o => o.email == email);
			}
			if (latitude.HasValue == true)
			{
				query = query.Where(o => o.latitude == latitude.Value);
			}
			if (longitude.HasValue == true)
			{
				query = query.Where(o => o.longitude == longitude.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(o => o.notes == notes);
			}
			if (string.IsNullOrEmpty(externalId) == false)
			{
				query = query.Where(o => o.externalId == externalId);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(o => o.color == color);
			}
			if (string.IsNullOrEmpty(attributes) == false)
			{
				query = query.Where(o => o.attributes == attributes);
			}
			if (string.IsNullOrEmpty(avatarFileName) == false)
			{
				query = query.Where(o => o.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(o => o.avatarSize == avatarSize.Value);
			}
			if (string.IsNullOrEmpty(avatarMimeType) == false)
			{
				query = query.Where(o => o.avatarMimeType == avatarMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(o => o.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(o => o.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(o => o.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(o => o.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(o => o.deleted == false);
				}
			}
			else
			{
				query = query.Where(o => o.active == true);
				query = query.Where(o => o.deleted == false);
			}

			query = query.OrderBy(o => o.name).ThenBy(o => o.description).ThenBy(o => o.addressLine1);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.country);
				query = query.Include(x => x.currency);
				query = query.Include(x => x.officeType);
				query = query.Include(x => x.stateProvince);
				query = query.Include(x => x.timeZone);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Office, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.addressLine1.Contains(anyStringContains)
			       || x.addressLine2.Contains(anyStringContains)
			       || x.city.Contains(anyStringContains)
			       || x.postalCode.Contains(anyStringContains)
			       || x.phone.Contains(anyStringContains)
			       || x.email.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.attributes.Contains(anyStringContains)
			       || x.avatarFileName.Contains(anyStringContains)
			       || x.avatarMimeType.Contains(anyStringContains)
			       || (includeRelations == true && x.country.name.Contains(anyStringContains))
			       || (includeRelations == true && x.country.description.Contains(anyStringContains))
			       || (includeRelations == true && x.country.abbreviation.Contains(anyStringContains))
			       || (includeRelations == true && x.country.postalCodeFormat.Contains(anyStringContains))
			       || (includeRelations == true && x.country.postalCodeRegEx.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.name.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.description.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.code.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.color.Contains(anyStringContains))
			       || (includeRelations == true && x.officeType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.officeType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.officeType.color.Contains(anyStringContains))
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
			
			List<Database.Office> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Office office in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(office, databaseStoresDateWithTimeZone);
			}


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			if (diskBasedBinaryStorageMode == true)
			{
				var tasks = materialized.Select(async office =>
				{

					if (office.avatarData == null &&
					    office.avatarSize.HasValue == true &&
					    office.avatarSize.Value > 0)
					{
					    office.avatarData = await LoadDataFromDiskAsync(office.objectGuid, office.versionNumber, "data");
					}

				}).ToList();

				// Run tasks concurrently and await their completion
				await Task.WhenAll(tasks);
			}

			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.Office Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.Office Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Offices filtered by the parameters provided.  Its query is similar to the GetOffices method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Offices/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			int? officeTypeId = null,
			int? timeZoneId = null,
			int? currencyId = null,
			string addressLine1 = null,
			string addressLine2 = null,
			string city = null,
			string postalCode = null,
			int? stateProvinceId = null,
			int? countryId = null,
			string phone = null,
			string email = null,
			double? latitude = null,
			double? longitude = null,
			string notes = null,
			string externalId = null,
			string color = null,
			string attributes = null,
			string avatarFileName = null,
			long? avatarSize = null,
			string avatarMimeType = null,
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


			IQueryable<Database.Office> query = (from o in _context.Offices select o);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(o => o.name == name);
			}
			if (description != null)
			{
				query = query.Where(o => o.description == description);
			}
			if (officeTypeId.HasValue == true)
			{
				query = query.Where(o => o.officeTypeId == officeTypeId.Value);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(o => o.timeZoneId == timeZoneId.Value);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(o => o.currencyId == currencyId.Value);
			}
			if (addressLine1 != null)
			{
				query = query.Where(o => o.addressLine1 == addressLine1);
			}
			if (addressLine2 != null)
			{
				query = query.Where(o => o.addressLine2 == addressLine2);
			}
			if (city != null)
			{
				query = query.Where(o => o.city == city);
			}
			if (postalCode != null)
			{
				query = query.Where(o => o.postalCode == postalCode);
			}
			if (stateProvinceId.HasValue == true)
			{
				query = query.Where(o => o.stateProvinceId == stateProvinceId.Value);
			}
			if (countryId.HasValue == true)
			{
				query = query.Where(o => o.countryId == countryId.Value);
			}
			if (phone != null)
			{
				query = query.Where(o => o.phone == phone);
			}
			if (email != null)
			{
				query = query.Where(o => o.email == email);
			}
			if (latitude.HasValue == true)
			{
				query = query.Where(o => o.latitude == latitude.Value);
			}
			if (longitude.HasValue == true)
			{
				query = query.Where(o => o.longitude == longitude.Value);
			}
			if (notes != null)
			{
				query = query.Where(o => o.notes == notes);
			}
			if (externalId != null)
			{
				query = query.Where(o => o.externalId == externalId);
			}
			if (color != null)
			{
				query = query.Where(o => o.color == color);
			}
			if (attributes != null)
			{
				query = query.Where(o => o.attributes == attributes);
			}
			if (avatarFileName != null)
			{
				query = query.Where(o => o.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(o => o.avatarSize == avatarSize.Value);
			}
			if (avatarMimeType != null)
			{
				query = query.Where(o => o.avatarMimeType == avatarMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(o => o.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(o => o.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(o => o.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(o => o.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(o => o.deleted == false);
				}
			}
			else
			{
				query = query.Where(o => o.active == true);
				query = query.Where(o => o.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Office, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.addressLine1.Contains(anyStringContains)
			       || x.addressLine2.Contains(anyStringContains)
			       || x.city.Contains(anyStringContains)
			       || x.postalCode.Contains(anyStringContains)
			       || x.phone.Contains(anyStringContains)
			       || x.email.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.attributes.Contains(anyStringContains)
			       || x.avatarFileName.Contains(anyStringContains)
			       || x.avatarMimeType.Contains(anyStringContains)
			       || x.country.name.Contains(anyStringContains)
			       || x.country.description.Contains(anyStringContains)
			       || x.country.abbreviation.Contains(anyStringContains)
			       || x.country.postalCodeFormat.Contains(anyStringContains)
			       || x.country.postalCodeRegEx.Contains(anyStringContains)
			       || x.currency.name.Contains(anyStringContains)
			       || x.currency.description.Contains(anyStringContains)
			       || x.currency.code.Contains(anyStringContains)
			       || x.currency.color.Contains(anyStringContains)
			       || x.officeType.name.Contains(anyStringContains)
			       || x.officeType.description.Contains(anyStringContains)
			       || x.officeType.color.Contains(anyStringContains)
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
        /// This gets a single Office by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Office/{id}")]
		public async Task<IActionResult> GetOffice(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.Office> query = (from o in _context.Offices where
							(o.id == id) &&
							(userIsAdmin == true || o.deleted == false) &&
							(userIsWriter == true || o.active == true)
					select o);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.country);
					query = query.Include(x => x.currency);
					query = query.Include(x => x.officeType);
					query = query.Include(x => x.stateProvince);
					query = query.Include(x => x.timeZone);
					query = query.AsSplitQuery();
				}

				Database.Office materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

					if (diskBasedBinaryStorageMode == true &&
					    materialized.avatarData == null &&
					    materialized.avatarSize.HasValue == true &&
					    materialized.avatarSize.Value > 0)
					{
					    materialized.avatarData = await LoadDataFromDiskAsync(materialized.objectGuid, materialized.versionNumber, "data", cancellationToken);
					}

					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.Office Entity was read with Admin privilege." : "Scheduler.Office Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Office", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.Office entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.Office.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.Office.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing Office record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Office/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		[RequestSizeLimit(5000000)]
		public async Task<IActionResult> PutOffice(int id, [FromBody]Database.Office.OfficeDTO officeDTO, CancellationToken cancellationToken = default)
		{
			if (officeDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != officeDTO.id)
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


			IQueryable<Database.Office> query = (from x in _context.Offices
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Office existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Office PUT", id.ToString(), new Exception("No Scheduler.Office entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (officeDTO.objectGuid == Guid.Empty)
            {
                officeDTO.objectGuid = existing.objectGuid;
            }
            else if (officeDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Office record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Office cloneOfExisting = (Database.Office)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Office object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Office office = (Database.Office)_context.Entry(existing).GetDatabaseValues().ToObject();
			office.ApplyDTO(officeDTO);
			//
			// The tenant guid for any Office being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the Office because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				office.tenantGuid = existing.tenantGuid;
			}

			lock (officePutSyncRoot)
			{
				//
				// Validate the version number for the office being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != office.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Office save attempt was made but save request was with version " + office.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The Office you are trying to update has already changed.  Please try your save again after reloading the Office.");
				}
				else
				{
					// Same record.  Increase version.
					office.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (office.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.Office record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (office.name != null && office.name.Length > 100)
				{
					office.name = office.name.Substring(0, 100);
				}

				if (office.description != null && office.description.Length > 500)
				{
					office.description = office.description.Substring(0, 500);
				}

				if (office.addressLine1 != null && office.addressLine1.Length > 250)
				{
					office.addressLine1 = office.addressLine1.Substring(0, 250);
				}

				if (office.addressLine2 != null && office.addressLine2.Length > 250)
				{
					office.addressLine2 = office.addressLine2.Substring(0, 250);
				}

				if (office.city != null && office.city.Length > 100)
				{
					office.city = office.city.Substring(0, 100);
				}

				if (office.postalCode != null && office.postalCode.Length > 100)
				{
					office.postalCode = office.postalCode.Substring(0, 100);
				}

				if (office.phone != null && office.phone.Length > 100)
				{
					office.phone = office.phone.Substring(0, 100);
				}

				if (office.email != null && office.email.Length > 250)
				{
					office.email = office.email.Substring(0, 250);
				}

				if (office.externalId != null && office.externalId.Length > 100)
				{
					office.externalId = office.externalId.Substring(0, 100);
				}

				if (office.color != null && office.color.Length > 10)
				{
					office.color = office.color.Substring(0, 10);
				}

				if (office.avatarFileName != null && office.avatarFileName.Length > 250)
				{
					office.avatarFileName = office.avatarFileName.Substring(0, 250);
				}

				if (office.avatarMimeType != null && office.avatarMimeType.Length > 100)
				{
					office.avatarMimeType = office.avatarMimeType.Substring(0, 100);
				}


				//
				// Add default values for any missing data attribute fields.
				//
				if (office.avatarData != null && string.IsNullOrEmpty(office.avatarFileName))
				{
				    office.avatarFileName = office.objectGuid.ToString() + ".data";
				}

				if (office.avatarData != null && (office.avatarSize.HasValue == false || office.avatarSize != office.avatarData.Length))
				{
				    office.avatarSize = office.avatarData.Length;
				}

				if (office.avatarData != null && string.IsNullOrEmpty(office.avatarMimeType))
				{
				    office.avatarMimeType = "application/octet-stream";
				}

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				try
				{
					byte[] dataReferenceBeforeClearing = office.avatarData;

					if (diskBasedBinaryStorageMode == true &&
					    office.avatarFileName != null &&
					    office.avatarData != null &&
					    office.avatarSize.HasValue == true &&
					    office.avatarSize.Value > 0)
					{
					    //
					    // write the bytes to disk
					    //
					    WriteDataToDisk(office.objectGuid, office.versionNumber, office.avatarData, "data");

					    //
					    // Clear the data from the object before we put it into the db
					    //
					    office.avatarData = null;

					}

				    EntityEntry<Database.Office> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(office);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        OfficeChangeHistory officeChangeHistory = new OfficeChangeHistory();
				        officeChangeHistory.officeId = office.id;
				        officeChangeHistory.versionNumber = office.versionNumber;
				        officeChangeHistory.timeStamp = DateTime.UtcNow;
				        officeChangeHistory.userId = securityUser.id;
				        officeChangeHistory.tenantGuid = userTenantGuid;
				        officeChangeHistory.data = JsonSerializer.Serialize(Database.Office.CreateAnonymousWithFirstLevelSubObjects(office));
				        _context.OfficeChangeHistories.Add(officeChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    office.avatarData = dataReferenceBeforeClearing;
					}

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Office entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Office.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Office.CreateAnonymousWithFirstLevelSubObjects(office)),
						null);

				return Ok(Database.Office.CreateAnonymous(office));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Office entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Office.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Office.CreateAnonymousWithFirstLevelSubObjects(office)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new Office record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Office", Name = "Office")]
		[RequestSizeLimit(5000000)]
		public async Task<IActionResult> PostOffice([FromBody]Database.Office.OfficeDTO officeDTO, CancellationToken cancellationToken = default)
		{
			if (officeDTO == null)
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
			// Create a new Office object using the data from the DTO
			//
			Database.Office office = Database.Office.FromDTO(officeDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				office.tenantGuid = userTenantGuid;

				if (office.name != null && office.name.Length > 100)
				{
					office.name = office.name.Substring(0, 100);
				}

				if (office.description != null && office.description.Length > 500)
				{
					office.description = office.description.Substring(0, 500);
				}

				if (office.addressLine1 != null && office.addressLine1.Length > 250)
				{
					office.addressLine1 = office.addressLine1.Substring(0, 250);
				}

				if (office.addressLine2 != null && office.addressLine2.Length > 250)
				{
					office.addressLine2 = office.addressLine2.Substring(0, 250);
				}

				if (office.city != null && office.city.Length > 100)
				{
					office.city = office.city.Substring(0, 100);
				}

				if (office.postalCode != null && office.postalCode.Length > 100)
				{
					office.postalCode = office.postalCode.Substring(0, 100);
				}

				if (office.phone != null && office.phone.Length > 100)
				{
					office.phone = office.phone.Substring(0, 100);
				}

				if (office.email != null && office.email.Length > 250)
				{
					office.email = office.email.Substring(0, 250);
				}

				if (office.externalId != null && office.externalId.Length > 100)
				{
					office.externalId = office.externalId.Substring(0, 100);
				}

				if (office.color != null && office.color.Length > 10)
				{
					office.color = office.color.Substring(0, 10);
				}

				if (office.avatarFileName != null && office.avatarFileName.Length > 250)
				{
					office.avatarFileName = office.avatarFileName.Substring(0, 250);
				}

				if (office.avatarMimeType != null && office.avatarMimeType.Length > 100)
				{
					office.avatarMimeType = office.avatarMimeType.Substring(0, 100);
				}

				office.objectGuid = Guid.NewGuid();

				//
				// Add default values for any missing data attribute fields.
				//
				if (office.avatarData != null && string.IsNullOrEmpty(office.avatarFileName))
				{
				    office.avatarFileName = office.objectGuid.ToString() + ".data";
				}

				if (office.avatarData != null && (office.avatarSize.HasValue == false || office.avatarSize != office.avatarData.Length))
				{
				    office.avatarSize = office.avatarData.Length;
				}

				if (office.avatarData != null && string.IsNullOrEmpty(office.avatarMimeType))
				{
				    office.avatarMimeType = "application/octet-stream";
				}

				office.versionNumber = 1;

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				byte[] dataReferenceBeforeClearing = office.avatarData;

				if (diskBasedBinaryStorageMode == true &&
				    office.avatarData != null &&
				    office.avatarFileName != null &&
				    office.avatarSize.HasValue == true &&
				    office.avatarSize.Value > 0)
				{
				    //
				    // write the bytes to disk
				    //
				    await WriteDataToDiskAsync(office.objectGuid, office.versionNumber, office.avatarData, "data", cancellationToken);

				    //
				    // Clear the data from the object before we put it into the db
				    //
				    office.avatarData = null;

				}

				_context.Offices.Add(office);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the office object so that no further changes will be written to the database
				    //
				    _context.Entry(office).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					office.avatarData = null;
					office.Calendars = null;
					office.Crews = null;
					office.EventResourceAssignments = null;
					office.Gifts = null;
					office.OfficeChangeHistories = null;
					office.OfficeContacts = null;
					office.RateSheets = null;
					office.Resources = null;
					office.ScheduledEvents = null;
					office.SchedulingTargets = null;
					office.VolunteerGroups = null;
					office.country = null;
					office.currency = null;
					office.officeType = null;
					office.stateProvince = null;
					office.timeZone = null;


				    OfficeChangeHistory officeChangeHistory = new OfficeChangeHistory();
				    officeChangeHistory.officeId = office.id;
				    officeChangeHistory.versionNumber = office.versionNumber;
				    officeChangeHistory.timeStamp = DateTime.UtcNow;
				    officeChangeHistory.userId = securityUser.id;
				    officeChangeHistory.tenantGuid = userTenantGuid;
				    officeChangeHistory.data = JsonSerializer.Serialize(Database.Office.CreateAnonymousWithFirstLevelSubObjects(office));
				    _context.OfficeChangeHistories.Add(officeChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.Office entity successfully created.",
						true,
						office. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.Office.CreateAnonymousWithFirstLevelSubObjects(office)),
						null);



					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    office.avatarData = dataReferenceBeforeClearing;
					}

				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.Office entity creation failed.", false, office.id.ToString(), "", JsonSerializer.Serialize(Database.Office.CreateAnonymousWithFirstLevelSubObjects(office)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Office", office.id, office.name));

			return CreatedAtRoute("Office", new { id = office.id }, Database.Office.CreateAnonymousWithFirstLevelSubObjects(office));
		}



        /// <summary>
        /// 
        /// This rolls a Office entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Office/Rollback/{id}")]
		[Route("api/Office/Rollback")]
		public async Task<IActionResult> RollbackToOfficeVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.Office> query = (from x in _context.Offices
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();


			//
			// Make sure nobody else is editing this Office concurrently
			//
			lock (officePutSyncRoot)
			{
				
				Database.Office office = query.FirstOrDefault();
				
				if (office == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Office rollback", id.ToString(), new Exception("No Scheduler.Office entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the Office current state so we can log it.
				//
				Database.Office cloneOfExisting = (Database.Office)_context.Entry(office).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.avatarData = null;
				cloneOfExisting.Calendars = null;
				cloneOfExisting.Crews = null;
				cloneOfExisting.EventResourceAssignments = null;
				cloneOfExisting.Gifts = null;
				cloneOfExisting.OfficeChangeHistories = null;
				cloneOfExisting.OfficeContacts = null;
				cloneOfExisting.RateSheets = null;
				cloneOfExisting.Resources = null;
				cloneOfExisting.ScheduledEvents = null;
				cloneOfExisting.SchedulingTargets = null;
				cloneOfExisting.VolunteerGroups = null;
				cloneOfExisting.country = null;
				cloneOfExisting.currency = null;
				cloneOfExisting.officeType = null;
				cloneOfExisting.stateProvince = null;
				cloneOfExisting.timeZone = null;

				if (versionNumber >= office.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.Office rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.Office rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				OfficeChangeHistory officeChangeHistory = (from x in _context.OfficeChangeHistories
				                                               where
				                                               x.officeId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (officeChangeHistory != null)
				{
				    Database.Office oldOffice = JsonSerializer.Deserialize<Database.Office>(officeChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    office.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    office.name = oldOffice.name;
				    office.description = oldOffice.description;
				    office.officeTypeId = oldOffice.officeTypeId;
				    office.timeZoneId = oldOffice.timeZoneId;
				    office.currencyId = oldOffice.currencyId;
				    office.addressLine1 = oldOffice.addressLine1;
				    office.addressLine2 = oldOffice.addressLine2;
				    office.city = oldOffice.city;
				    office.postalCode = oldOffice.postalCode;
				    office.stateProvinceId = oldOffice.stateProvinceId;
				    office.countryId = oldOffice.countryId;
				    office.phone = oldOffice.phone;
				    office.email = oldOffice.email;
				    office.latitude = oldOffice.latitude;
				    office.longitude = oldOffice.longitude;
				    office.notes = oldOffice.notes;
				    office.externalId = oldOffice.externalId;
				    office.color = oldOffice.color;
				    office.attributes = oldOffice.attributes;
				    office.avatarFileName = oldOffice.avatarFileName;
				    office.avatarSize = oldOffice.avatarSize;
				    office.avatarData = oldOffice.avatarData;
				    office.avatarMimeType = oldOffice.avatarMimeType;
				    office.objectGuid = oldOffice.objectGuid;
				    office.active = oldOffice.active;
				    office.deleted = oldOffice.deleted;
				    //
				    // If disk based binary mode is on, then we need to copy the old data file over as well.
				    //
				    if (diskBasedBinaryStorageMode == true)
				    {
				    	Byte[] binaryData = LoadDataFromDisk(oldOffice.objectGuid, oldOffice.versionNumber, "data");

				    	//
				    	// Write out the data as the new version
				    	//
				    	WriteDataToDisk(office.objectGuid, office.versionNumber, binaryData, "data");
				    }

				    string serializedOffice = JsonSerializer.Serialize(office);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        OfficeChangeHistory newOfficeChangeHistory = new OfficeChangeHistory();
				        newOfficeChangeHistory.officeId = office.id;
				        newOfficeChangeHistory.versionNumber = office.versionNumber;
				        newOfficeChangeHistory.timeStamp = DateTime.UtcNow;
				        newOfficeChangeHistory.userId = securityUser.id;
				        newOfficeChangeHistory.tenantGuid = userTenantGuid;
				        newOfficeChangeHistory.data = JsonSerializer.Serialize(Database.Office.CreateAnonymousWithFirstLevelSubObjects(office));
				        _context.OfficeChangeHistories.Add(newOfficeChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Office rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Office.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Office.CreateAnonymousWithFirstLevelSubObjects(office)),
						null);


				    return Ok(Database.Office.CreateAnonymous(office));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.Office rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.Office rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a Office.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Office</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Office/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetOfficeChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.Office office = await _context.Offices.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (office == null)
			{
				return NotFound();
			}

			try
			{
				office.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Office> versionInfo = await office.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a Office.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Office</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Office/{id}/AuditHistory")]
		public async Task<IActionResult> GetOfficeAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.Office office = await _context.Offices.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (office == null)
			{
				return NotFound();
			}

			try
			{
				office.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.Office>> versions = await office.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a Office.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Office</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The Office object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Office/{id}/Version/{version}")]
		public async Task<IActionResult> GetOfficeVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.Office office = await _context.Offices.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (office == null)
			{
				return NotFound();
			}

			try
			{
				office.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Office> versionInfo = await office.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a Office at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Office</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The Office object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Office/{id}/StateAtTime")]
		public async Task<IActionResult> GetOfficeStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.Office office = await _context.Offices.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (office == null)
			{
				return NotFound();
			}

			try
			{
				office.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Office> versionInfo = await office.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a Office record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Office/{id}")]
		[Route("api/Office")]
		public async Task<IActionResult> DeleteOffice(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.Office> query = (from x in _context.Offices
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Office office = await query.FirstOrDefaultAsync(cancellationToken);

			if (office == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Office DELETE", id.ToString(), new Exception("No Scheduler.Office entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Office cloneOfExisting = (Database.Office)_context.Entry(office).GetDatabaseValues().ToObject();


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			lock (officeDeleteSyncRoot)
			{
			    try
			    {
			        office.deleted = true;
			        office.versionNumber++;

			        _context.SaveChanges();

			        //
			        // If in disk based storage mode, create a copy of the disk data file for the new version.
			        //
			        if (diskBasedBinaryStorageMode == true)
			        {
			        	Byte[] binaryData = LoadDataFromDisk(office.objectGuid, office.versionNumber -1, "data");

			        	//
			        	// Write out the same data
			        	//
			        	WriteDataToDisk(office.objectGuid, office.versionNumber, binaryData, "data");
			        }

			        //
			        // Now add the change history
			        //
			        OfficeChangeHistory officeChangeHistory = new OfficeChangeHistory();
			        officeChangeHistory.officeId = office.id;
			        officeChangeHistory.versionNumber = office.versionNumber;
			        officeChangeHistory.timeStamp = DateTime.UtcNow;
			        officeChangeHistory.userId = securityUser.id;
			        officeChangeHistory.tenantGuid = userTenantGuid;
			        officeChangeHistory.data = JsonSerializer.Serialize(Database.Office.CreateAnonymousWithFirstLevelSubObjects(office));
			        _context.OfficeChangeHistories.Add(officeChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Office entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Office.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Office.CreateAnonymousWithFirstLevelSubObjects(office)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Office entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Office.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Office.CreateAnonymousWithFirstLevelSubObjects(office)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of Office records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Offices/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			int? officeTypeId = null,
			int? timeZoneId = null,
			int? currencyId = null,
			string addressLine1 = null,
			string addressLine2 = null,
			string city = null,
			string postalCode = null,
			int? stateProvinceId = null,
			int? countryId = null,
			string phone = null,
			string email = null,
			double? latitude = null,
			double? longitude = null,
			string notes = null,
			string externalId = null,
			string color = null,
			string attributes = null,
			string avatarFileName = null,
			long? avatarSize = null,
			string avatarMimeType = null,
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

			IQueryable<Database.Office> query = (from o in _context.Offices select o);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(o => o.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(o => o.description == description);
			}
			if (officeTypeId.HasValue == true)
			{
				query = query.Where(o => o.officeTypeId == officeTypeId.Value);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(o => o.timeZoneId == timeZoneId.Value);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(o => o.currencyId == currencyId.Value);
			}
			if (string.IsNullOrEmpty(addressLine1) == false)
			{
				query = query.Where(o => o.addressLine1 == addressLine1);
			}
			if (string.IsNullOrEmpty(addressLine2) == false)
			{
				query = query.Where(o => o.addressLine2 == addressLine2);
			}
			if (string.IsNullOrEmpty(city) == false)
			{
				query = query.Where(o => o.city == city);
			}
			if (string.IsNullOrEmpty(postalCode) == false)
			{
				query = query.Where(o => o.postalCode == postalCode);
			}
			if (stateProvinceId.HasValue == true)
			{
				query = query.Where(o => o.stateProvinceId == stateProvinceId.Value);
			}
			if (countryId.HasValue == true)
			{
				query = query.Where(o => o.countryId == countryId.Value);
			}
			if (string.IsNullOrEmpty(phone) == false)
			{
				query = query.Where(o => o.phone == phone);
			}
			if (string.IsNullOrEmpty(email) == false)
			{
				query = query.Where(o => o.email == email);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(o => o.notes == notes);
			}
			if (string.IsNullOrEmpty(externalId) == false)
			{
				query = query.Where(o => o.externalId == externalId);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(o => o.color == color);
			}
			if (string.IsNullOrEmpty(attributes) == false)
			{
				query = query.Where(o => o.attributes == attributes);
			}
			if (string.IsNullOrEmpty(avatarFileName) == false)
			{
				query = query.Where(o => o.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(o => o.avatarSize == avatarSize.Value);
			}
			if (string.IsNullOrEmpty(avatarMimeType) == false)
			{
				query = query.Where(o => o.avatarMimeType == avatarMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(o => o.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(o => o.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(o => o.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(o => o.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(o => o.deleted == false);
				}
			}
			else
			{
				query = query.Where(o => o.active == true);
				query = query.Where(o => o.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Office, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.addressLine1.Contains(anyStringContains)
			       || x.addressLine2.Contains(anyStringContains)
			       || x.city.Contains(anyStringContains)
			       || x.postalCode.Contains(anyStringContains)
			       || x.phone.Contains(anyStringContains)
			       || x.email.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.attributes.Contains(anyStringContains)
			       || x.avatarFileName.Contains(anyStringContains)
			       || x.avatarMimeType.Contains(anyStringContains)
			       || x.country.name.Contains(anyStringContains)
			       || x.country.description.Contains(anyStringContains)
			       || x.country.abbreviation.Contains(anyStringContains)
			       || x.country.postalCodeFormat.Contains(anyStringContains)
			       || x.country.postalCodeRegEx.Contains(anyStringContains)
			       || x.currency.name.Contains(anyStringContains)
			       || x.currency.description.Contains(anyStringContains)
			       || x.currency.code.Contains(anyStringContains)
			       || x.currency.color.Contains(anyStringContains)
			       || x.officeType.name.Contains(anyStringContains)
			       || x.officeType.description.Contains(anyStringContains)
			       || x.officeType.color.Contains(anyStringContains)
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


			query = query.OrderBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.addressLine1);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Office.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Office/CreateAuditEvent")]
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
        /// This makes a Office record a favourite for the current user.
		/// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/Office/Favourite/{id}")]
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


			IQueryable<Database.Office> query = (from x in _context.Offices
			                               where x.id == id
			                               select x);


			Database.Office office = await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

			if (office != null)
			{
				if (string.IsNullOrEmpty(description) == true)
				{
					description = office.name;
				}

				//
				// Add the user favourite Office
				//
				await SecurityLogic.AddToUserFavouritesAsync(securityUser, "Office", id, description, cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'Office' was added for record with id of " + id + " for user " + securityUser.accountName, true);

				//
				// Return the complete list of user favourites after the addition
				//
				return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
			}
			else
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'Office' add request was made with an invalid id value of " + id, false);
				return BadRequest();
			}
		}


        /// <summary>
        /// 
        /// This removes a Office record from the current user's favourites.
        /// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/Office/Favourite/{id}")]
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
			// Delete the user favourite Office
			//
			await SecurityLogic.RemoveFromUserFavouritesAsync(securityUser, "Office", id, cancellationToken);

			await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'Office' was removed for record with id of " + id + " for user " + securityUser.accountName, true);

			//
			// Return the complete list of user favourites after the deletion
			//
			return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
		}




        [Route("api/Office/Data/{id:int}")]
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


            Database.Office office = await (from x in _context.Offices where x.id == id && x.active == true && x.deleted == false select x).FirstOrDefaultAsync();
            if (office == null)
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

						lock (officePutSyncRoot)
						{
							try
							{
								using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
								{
									office.avatarFileName = fileName.Trim();
									office.avatarMimeType = mimeType;
									office.avatarSize = section.Body.Length;

									office.versionNumber++;

									if (diskBasedBinaryStorageMode == true &&
										 office.avatarFileName != null &&
										 office.avatarSize > 0)
									{
										//
										// write the bytes to disk
										//
										WriteDataToDisk(office.objectGuid, office.versionNumber, section.Body, "data");
										//
										// Clear the data from the object before we put it into the db
										//
										office.avatarData = null;
									}
									else
									{
										using (MemoryStream memoryStream = new MemoryStream((int)section.Body.Length))
										{
											section.Body.CopyTo(memoryStream);
											office.avatarData = memoryStream.ToArray();
										}
									}
									//
									// Now add the change history
									//
									OfficeChangeHistory officeChangeHistory = new OfficeChangeHistory();
									officeChangeHistory.officeId = office.id;
									officeChangeHistory.versionNumber = office.versionNumber;
									officeChangeHistory.timeStamp = DateTime.UtcNow;
									officeChangeHistory.userId = securityUser.id;
									officeChangeHistory.tenantGuid = office.tenantGuid;
									officeChangeHistory.data = JsonSerializer.Serialize(Database.Office.CreateAnonymousWithFirstLevelSubObjects(office));
									_context.OfficeChangeHistories.Add(officeChangeHistory);

									_context.SaveChanges();

									transaction.Commit();

									CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Office Data Uploaded with filename of " + fileName + " and with size of " + section.Body.Length, id.ToString());
								}
							}
							catch (Exception ex)
							{
								CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "Office Data Upload Failed.", false, id.ToString(), "", "", ex);

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
                lock (officePutSyncRoot)
                {
                    try
                    {
                        using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                        {
                            if (diskBasedBinaryStorageMode == true)
                            {
								DeleteDataFromDisk(office.objectGuid, office.versionNumber, "data");
                            }

                            office.avatarFileName = null;
                            office.avatarMimeType = null;
                            office.avatarSize = 0;
                            office.avatarData = null;
                            office.versionNumber++;


                            //
                            // Now add the change history
                            //
                            OfficeChangeHistory officeChangeHistory = new OfficeChangeHistory();
                            officeChangeHistory.officeId = office.id;
                            officeChangeHistory.versionNumber = office.versionNumber;
                            officeChangeHistory.timeStamp = DateTime.UtcNow;
                            officeChangeHistory.userId = securityUser.id;
                                    officeChangeHistory.tenantGuid = office.tenantGuid;
                                    officeChangeHistory.data = JsonSerializer.Serialize(Database.Office.CreateAnonymousWithFirstLevelSubObjects(office));
                            _context.OfficeChangeHistories.Add(officeChangeHistory);

                            _context.SaveChanges();

                            transaction.Commit();

                            CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Office data cleared.", id.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "Office data clear failed.", false, id.ToString(), "", "", ex);

                        return Problem(ex.Message);
                    }
                }
            }

            return Ok();
        }

        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/Office/Data/{id:int}")]
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
                Database.Office office = await (from d in context.Offices
                                                where d.id == id &&
                                                d.active == true &&
                                                d.deleted == false
                                                select d).FirstOrDefaultAsync();

                if (office != null && office.avatarData != null)
                {
                   return File(office.avatarData.ToArray<byte>(), office.avatarMimeType, office.avatarFileName != null ? office.avatarFileName.Trim() : "Office_" + office.id.ToString(), true);
                }
                else
                {
                    return BadRequest();
                }
            }
        }
	}
}
