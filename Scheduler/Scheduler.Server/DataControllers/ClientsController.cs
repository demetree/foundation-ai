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
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the Client entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Client entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ClientsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object clientPutSyncRoot = new object();
		static object clientDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<ClientsController> _logger;

		public ClientsController(SchedulerContext context, ILogger<ClientsController> logger) : base("Scheduler", "Client")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Clients filtered by the parameters provided.
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
		[Route("api/Clients")]
		public async Task<IActionResult> GetClients(
			string name = null,
			string description = null,
			int? clientTypeId = null,
			int? currencyId = null,
			int? timeZoneId = null,
			int? calendarId = null,
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

			IQueryable<Database.Client> query = (from c in _context.Clients select c);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(c => c.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(c => c.description == description);
			}
			if (clientTypeId.HasValue == true)
			{
				query = query.Where(c => c.clientTypeId == clientTypeId.Value);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(c => c.currencyId == currencyId.Value);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(c => c.timeZoneId == timeZoneId.Value);
			}
			if (calendarId.HasValue == true)
			{
				query = query.Where(c => c.calendarId == calendarId.Value);
			}
			if (string.IsNullOrEmpty(addressLine1) == false)
			{
				query = query.Where(c => c.addressLine1 == addressLine1);
			}
			if (string.IsNullOrEmpty(addressLine2) == false)
			{
				query = query.Where(c => c.addressLine2 == addressLine2);
			}
			if (string.IsNullOrEmpty(city) == false)
			{
				query = query.Where(c => c.city == city);
			}
			if (string.IsNullOrEmpty(postalCode) == false)
			{
				query = query.Where(c => c.postalCode == postalCode);
			}
			if (stateProvinceId.HasValue == true)
			{
				query = query.Where(c => c.stateProvinceId == stateProvinceId.Value);
			}
			if (countryId.HasValue == true)
			{
				query = query.Where(c => c.countryId == countryId.Value);
			}
			if (string.IsNullOrEmpty(phone) == false)
			{
				query = query.Where(c => c.phone == phone);
			}
			if (string.IsNullOrEmpty(email) == false)
			{
				query = query.Where(c => c.email == email);
			}
			if (latitude.HasValue == true)
			{
				query = query.Where(c => c.latitude == latitude.Value);
			}
			if (longitude.HasValue == true)
			{
				query = query.Where(c => c.longitude == longitude.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(c => c.notes == notes);
			}
			if (string.IsNullOrEmpty(externalId) == false)
			{
				query = query.Where(c => c.externalId == externalId);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(c => c.color == color);
			}
			if (string.IsNullOrEmpty(attributes) == false)
			{
				query = query.Where(c => c.attributes == attributes);
			}
			if (string.IsNullOrEmpty(avatarFileName) == false)
			{
				query = query.Where(c => c.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(c => c.avatarSize == avatarSize.Value);
			}
			if (string.IsNullOrEmpty(avatarMimeType) == false)
			{
				query = query.Where(c => c.avatarMimeType == avatarMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(c => c.versionNumber == versionNumber.Value);
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

			query = query.OrderBy(c => c.name).ThenBy(c => c.description).ThenBy(c => c.addressLine1);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.calendar);
				query = query.Include(x => x.clientType);
				query = query.Include(x => x.country);
				query = query.Include(x => x.currency);
				query = query.Include(x => x.stateProvince);
				query = query.Include(x => x.timeZone);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Client, or on an any of the string fields on its immediate relations
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
			       || (includeRelations == true && x.calendar.name.Contains(anyStringContains))
			       || (includeRelations == true && x.calendar.description.Contains(anyStringContains))
			       || (includeRelations == true && x.calendar.color.Contains(anyStringContains))
			       || (includeRelations == true && x.clientType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.clientType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.clientType.color.Contains(anyStringContains))
			       || (includeRelations == true && x.country.name.Contains(anyStringContains))
			       || (includeRelations == true && x.country.description.Contains(anyStringContains))
			       || (includeRelations == true && x.country.abbreviation.Contains(anyStringContains))
			       || (includeRelations == true && x.country.postalCodeFormat.Contains(anyStringContains))
			       || (includeRelations == true && x.country.postalCodeRegEx.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.name.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.description.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.code.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.color.Contains(anyStringContains))
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
			
			List<Database.Client> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Client client in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(client, databaseStoresDateWithTimeZone);
			}


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			if (diskBasedBinaryStorageMode == true)
			{
				var tasks = materialized.Select(async client =>
				{

					if (client.avatarData == null &&
					    client.avatarSize.HasValue == true &&
					    client.avatarSize.Value > 0)
					{
					    client.avatarData = await LoadDataFromDiskAsync(client.objectGuid, client.versionNumber, "data");
					}

				}).ToList();

				// Run tasks concurrently and await their completion
				await Task.WhenAll(tasks);
			}

			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.Client Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.Client Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Clients filtered by the parameters provided.  Its query is similar to the GetClients method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Clients/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			int? clientTypeId = null,
			int? currencyId = null,
			int? timeZoneId = null,
			int? calendarId = null,
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


			IQueryable<Database.Client> query = (from c in _context.Clients select c);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(c => c.name == name);
			}
			if (description != null)
			{
				query = query.Where(c => c.description == description);
			}
			if (clientTypeId.HasValue == true)
			{
				query = query.Where(c => c.clientTypeId == clientTypeId.Value);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(c => c.currencyId == currencyId.Value);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(c => c.timeZoneId == timeZoneId.Value);
			}
			if (calendarId.HasValue == true)
			{
				query = query.Where(c => c.calendarId == calendarId.Value);
			}
			if (addressLine1 != null)
			{
				query = query.Where(c => c.addressLine1 == addressLine1);
			}
			if (addressLine2 != null)
			{
				query = query.Where(c => c.addressLine2 == addressLine2);
			}
			if (city != null)
			{
				query = query.Where(c => c.city == city);
			}
			if (postalCode != null)
			{
				query = query.Where(c => c.postalCode == postalCode);
			}
			if (stateProvinceId.HasValue == true)
			{
				query = query.Where(c => c.stateProvinceId == stateProvinceId.Value);
			}
			if (countryId.HasValue == true)
			{
				query = query.Where(c => c.countryId == countryId.Value);
			}
			if (phone != null)
			{
				query = query.Where(c => c.phone == phone);
			}
			if (email != null)
			{
				query = query.Where(c => c.email == email);
			}
			if (latitude.HasValue == true)
			{
				query = query.Where(c => c.latitude == latitude.Value);
			}
			if (longitude.HasValue == true)
			{
				query = query.Where(c => c.longitude == longitude.Value);
			}
			if (notes != null)
			{
				query = query.Where(c => c.notes == notes);
			}
			if (externalId != null)
			{
				query = query.Where(c => c.externalId == externalId);
			}
			if (color != null)
			{
				query = query.Where(c => c.color == color);
			}
			if (attributes != null)
			{
				query = query.Where(c => c.attributes == attributes);
			}
			if (avatarFileName != null)
			{
				query = query.Where(c => c.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(c => c.avatarSize == avatarSize.Value);
			}
			if (avatarMimeType != null)
			{
				query = query.Where(c => c.avatarMimeType == avatarMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(c => c.versionNumber == versionNumber.Value);
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
			// Add the any string contains parameter to span all the string fields on the Client, or on an any of the string fields on its immediate relations
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
			       || x.calendar.name.Contains(anyStringContains)
			       || x.calendar.description.Contains(anyStringContains)
			       || x.calendar.color.Contains(anyStringContains)
			       || x.clientType.name.Contains(anyStringContains)
			       || x.clientType.description.Contains(anyStringContains)
			       || x.clientType.color.Contains(anyStringContains)
			       || x.country.name.Contains(anyStringContains)
			       || x.country.description.Contains(anyStringContains)
			       || x.country.abbreviation.Contains(anyStringContains)
			       || x.country.postalCodeFormat.Contains(anyStringContains)
			       || x.country.postalCodeRegEx.Contains(anyStringContains)
			       || x.currency.name.Contains(anyStringContains)
			       || x.currency.description.Contains(anyStringContains)
			       || x.currency.code.Contains(anyStringContains)
			       || x.currency.color.Contains(anyStringContains)
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
        /// This gets a single Client by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Client/{id}")]
		public async Task<IActionResult> GetClient(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.Client> query = (from c in _context.Clients where
							(c.id == id) &&
							(userIsAdmin == true || c.deleted == false) &&
							(userIsWriter == true || c.active == true)
					select c);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.calendar);
					query = query.Include(x => x.clientType);
					query = query.Include(x => x.country);
					query = query.Include(x => x.currency);
					query = query.Include(x => x.stateProvince);
					query = query.Include(x => x.timeZone);
					query = query.AsSplitQuery();
				}

				Database.Client materialized = await query.FirstOrDefaultAsync(cancellationToken);

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

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.Client Entity was read with Admin privilege." : "Scheduler.Client Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Client", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.Client entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.Client.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.Client.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing Client record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Client/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		[RequestSizeLimit(5000000)]
		public async Task<IActionResult> PutClient(int id, [FromBody]Database.Client.ClientDTO clientDTO, CancellationToken cancellationToken = default)
		{
			if (clientDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != clientDTO.id)
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


			IQueryable<Database.Client> query = (from x in _context.Clients
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Client existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Client PUT", id.ToString(), new Exception("No Scheduler.Client entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (clientDTO.objectGuid == Guid.Empty)
            {
                clientDTO.objectGuid = existing.objectGuid;
            }
            else if (clientDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Client record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Client cloneOfExisting = (Database.Client)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Client object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Client client = (Database.Client)_context.Entry(existing).GetDatabaseValues().ToObject();
			client.ApplyDTO(clientDTO);
			//
			// The tenant guid for any Client being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the Client because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				client.tenantGuid = existing.tenantGuid;
			}

			lock (clientPutSyncRoot)
			{
				//
				// Validate the version number for the client being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != client.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Client save attempt was made but save request was with version " + client.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The Client you are trying to update has already changed.  Please try your save again after reloading the Client.");
				}
				else
				{
					// Same record.  Increase version.
					client.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (client.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.Client record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (client.name != null && client.name.Length > 100)
				{
					client.name = client.name.Substring(0, 100);
				}

				if (client.description != null && client.description.Length > 500)
				{
					client.description = client.description.Substring(0, 500);
				}

				if (client.addressLine1 != null && client.addressLine1.Length > 250)
				{
					client.addressLine1 = client.addressLine1.Substring(0, 250);
				}

				if (client.addressLine2 != null && client.addressLine2.Length > 250)
				{
					client.addressLine2 = client.addressLine2.Substring(0, 250);
				}

				if (client.city != null && client.city.Length > 100)
				{
					client.city = client.city.Substring(0, 100);
				}

				if (client.postalCode != null && client.postalCode.Length > 100)
				{
					client.postalCode = client.postalCode.Substring(0, 100);
				}

				if (client.phone != null && client.phone.Length > 100)
				{
					client.phone = client.phone.Substring(0, 100);
				}

				if (client.email != null && client.email.Length > 250)
				{
					client.email = client.email.Substring(0, 250);
				}

				if (client.externalId != null && client.externalId.Length > 100)
				{
					client.externalId = client.externalId.Substring(0, 100);
				}

				if (client.color != null && client.color.Length > 10)
				{
					client.color = client.color.Substring(0, 10);
				}

				if (client.avatarFileName != null && client.avatarFileName.Length > 250)
				{
					client.avatarFileName = client.avatarFileName.Substring(0, 250);
				}

				if (client.avatarMimeType != null && client.avatarMimeType.Length > 100)
				{
					client.avatarMimeType = client.avatarMimeType.Substring(0, 100);
				}


				//
				// Add default values for any missing data attribute fields.
				//
				if (client.avatarData != null && string.IsNullOrEmpty(client.avatarFileName))
				{
				    client.avatarFileName = client.objectGuid.ToString() + ".data";
				}

				if (client.avatarData != null && (client.avatarSize.HasValue == false || client.avatarSize != client.avatarData.Length))
				{
				    client.avatarSize = client.avatarData.Length;
				}

				if (client.avatarData != null && string.IsNullOrEmpty(client.avatarMimeType))
				{
				    client.avatarMimeType = "application/octet-stream";
				}

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				try
				{
					byte[] dataReferenceBeforeClearing = client.avatarData;

					if (diskBasedBinaryStorageMode == true &&
					    client.avatarFileName != null &&
					    client.avatarData != null &&
					    client.avatarSize.HasValue == true &&
					    client.avatarSize.Value > 0)
					{
					    //
					    // write the bytes to disk
					    //
					    WriteDataToDisk(client.objectGuid, client.versionNumber, client.avatarData, "data");

					    //
					    // Clear the data from the object before we put it into the db
					    //
					    client.avatarData = null;

					}

				    EntityEntry<Database.Client> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(client);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ClientChangeHistory clientChangeHistory = new ClientChangeHistory();
				        clientChangeHistory.clientId = client.id;
				        clientChangeHistory.versionNumber = client.versionNumber;
				        clientChangeHistory.timeStamp = DateTime.UtcNow;
				        clientChangeHistory.userId = securityUser.id;
				        clientChangeHistory.tenantGuid = userTenantGuid;
				        clientChangeHistory.data = JsonSerializer.Serialize(Database.Client.CreateAnonymousWithFirstLevelSubObjects(client));
				        _context.ClientChangeHistories.Add(clientChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    client.avatarData = dataReferenceBeforeClearing;
					}

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Client entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Client.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Client.CreateAnonymousWithFirstLevelSubObjects(client)),
						null);

				return Ok(Database.Client.CreateAnonymous(client));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Client entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Client.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Client.CreateAnonymousWithFirstLevelSubObjects(client)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new Client record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Client", Name = "Client")]
		[RequestSizeLimit(5000000)]
		public async Task<IActionResult> PostClient([FromBody]Database.Client.ClientDTO clientDTO, CancellationToken cancellationToken = default)
		{
			if (clientDTO == null)
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
			// Create a new Client object using the data from the DTO
			//
			Database.Client client = Database.Client.FromDTO(clientDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				client.tenantGuid = userTenantGuid;

				if (client.name != null && client.name.Length > 100)
				{
					client.name = client.name.Substring(0, 100);
				}

				if (client.description != null && client.description.Length > 500)
				{
					client.description = client.description.Substring(0, 500);
				}

				if (client.addressLine1 != null && client.addressLine1.Length > 250)
				{
					client.addressLine1 = client.addressLine1.Substring(0, 250);
				}

				if (client.addressLine2 != null && client.addressLine2.Length > 250)
				{
					client.addressLine2 = client.addressLine2.Substring(0, 250);
				}

				if (client.city != null && client.city.Length > 100)
				{
					client.city = client.city.Substring(0, 100);
				}

				if (client.postalCode != null && client.postalCode.Length > 100)
				{
					client.postalCode = client.postalCode.Substring(0, 100);
				}

				if (client.phone != null && client.phone.Length > 100)
				{
					client.phone = client.phone.Substring(0, 100);
				}

				if (client.email != null && client.email.Length > 250)
				{
					client.email = client.email.Substring(0, 250);
				}

				if (client.externalId != null && client.externalId.Length > 100)
				{
					client.externalId = client.externalId.Substring(0, 100);
				}

				if (client.color != null && client.color.Length > 10)
				{
					client.color = client.color.Substring(0, 10);
				}

				if (client.avatarFileName != null && client.avatarFileName.Length > 250)
				{
					client.avatarFileName = client.avatarFileName.Substring(0, 250);
				}

				if (client.avatarMimeType != null && client.avatarMimeType.Length > 100)
				{
					client.avatarMimeType = client.avatarMimeType.Substring(0, 100);
				}

				client.objectGuid = Guid.NewGuid();

				//
				// Add default values for any missing data attribute fields.
				//
				if (client.avatarData != null && string.IsNullOrEmpty(client.avatarFileName))
				{
				    client.avatarFileName = client.objectGuid.ToString() + ".data";
				}

				if (client.avatarData != null && (client.avatarSize.HasValue == false || client.avatarSize != client.avatarData.Length))
				{
				    client.avatarSize = client.avatarData.Length;
				}

				if (client.avatarData != null && string.IsNullOrEmpty(client.avatarMimeType))
				{
				    client.avatarMimeType = "application/octet-stream";
				}

				client.versionNumber = 1;

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				byte[] dataReferenceBeforeClearing = client.avatarData;

				if (diskBasedBinaryStorageMode == true &&
				    client.avatarData != null &&
				    client.avatarFileName != null &&
				    client.avatarSize.HasValue == true &&
				    client.avatarSize.Value > 0)
				{
				    //
				    // write the bytes to disk
				    //
				    await WriteDataToDiskAsync(client.objectGuid, client.versionNumber, client.avatarData, "data", cancellationToken);

				    //
				    // Clear the data from the object before we put it into the db
				    //
				    client.avatarData = null;

				}

				_context.Clients.Add(client);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the client object so that no further changes will be written to the database
				    //
				    _context.Entry(client).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					client.avatarData = null;
					client.ClientChangeHistories = null;
					client.ClientContacts = null;
					client.Constituents = null;
					client.ScheduledEvents = null;
					client.SchedulingTargetAddresses = null;
					client.SchedulingTargets = null;
					client.calendar = null;
					client.clientType = null;
					client.country = null;
					client.currency = null;
					client.stateProvince = null;
					client.timeZone = null;


				    ClientChangeHistory clientChangeHistory = new ClientChangeHistory();
				    clientChangeHistory.clientId = client.id;
				    clientChangeHistory.versionNumber = client.versionNumber;
				    clientChangeHistory.timeStamp = DateTime.UtcNow;
				    clientChangeHistory.userId = securityUser.id;
				    clientChangeHistory.tenantGuid = userTenantGuid;
				    clientChangeHistory.data = JsonSerializer.Serialize(Database.Client.CreateAnonymousWithFirstLevelSubObjects(client));
				    _context.ClientChangeHistories.Add(clientChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.Client entity successfully created.",
						true,
						client. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.Client.CreateAnonymousWithFirstLevelSubObjects(client)),
						null);



					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    client.avatarData = dataReferenceBeforeClearing;
					}

				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.Client entity creation failed.", false, client.id.ToString(), "", JsonSerializer.Serialize(Database.Client.CreateAnonymousWithFirstLevelSubObjects(client)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Client", client.id, client.name));

			return CreatedAtRoute("Client", new { id = client.id }, Database.Client.CreateAnonymousWithFirstLevelSubObjects(client));
		}



        /// <summary>
        /// 
        /// This rolls a Client entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Client/Rollback/{id}")]
		[Route("api/Client/Rollback")]
		public async Task<IActionResult> RollbackToClientVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.Client> query = (from x in _context.Clients
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();


			//
			// Make sure nobody else is editing this Client concurrently
			//
			lock (clientPutSyncRoot)
			{
				
				Database.Client client = query.FirstOrDefault();
				
				if (client == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Client rollback", id.ToString(), new Exception("No Scheduler.Client entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the Client current state so we can log it.
				//
				Database.Client cloneOfExisting = (Database.Client)_context.Entry(client).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.avatarData = null;
				cloneOfExisting.ClientChangeHistories = null;
				cloneOfExisting.ClientContacts = null;
				cloneOfExisting.Constituents = null;
				cloneOfExisting.ScheduledEvents = null;
				cloneOfExisting.SchedulingTargetAddresses = null;
				cloneOfExisting.SchedulingTargets = null;
				cloneOfExisting.calendar = null;
				cloneOfExisting.clientType = null;
				cloneOfExisting.country = null;
				cloneOfExisting.currency = null;
				cloneOfExisting.stateProvince = null;
				cloneOfExisting.timeZone = null;

				if (versionNumber >= client.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.Client rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.Client rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ClientChangeHistory clientChangeHistory = (from x in _context.ClientChangeHistories
				                                               where
				                                               x.clientId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (clientChangeHistory != null)
				{
				    Database.Client oldClient = JsonSerializer.Deserialize<Database.Client>(clientChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    client.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    client.name = oldClient.name;
				    client.description = oldClient.description;
				    client.clientTypeId = oldClient.clientTypeId;
				    client.currencyId = oldClient.currencyId;
				    client.timeZoneId = oldClient.timeZoneId;
				    client.calendarId = oldClient.calendarId;
				    client.addressLine1 = oldClient.addressLine1;
				    client.addressLine2 = oldClient.addressLine2;
				    client.city = oldClient.city;
				    client.postalCode = oldClient.postalCode;
				    client.stateProvinceId = oldClient.stateProvinceId;
				    client.countryId = oldClient.countryId;
				    client.phone = oldClient.phone;
				    client.email = oldClient.email;
				    client.latitude = oldClient.latitude;
				    client.longitude = oldClient.longitude;
				    client.notes = oldClient.notes;
				    client.externalId = oldClient.externalId;
				    client.color = oldClient.color;
				    client.attributes = oldClient.attributes;
				    client.avatarFileName = oldClient.avatarFileName;
				    client.avatarSize = oldClient.avatarSize;
				    client.avatarData = oldClient.avatarData;
				    client.avatarMimeType = oldClient.avatarMimeType;
				    client.objectGuid = oldClient.objectGuid;
				    client.active = oldClient.active;
				    client.deleted = oldClient.deleted;
				    //
				    // If disk based binary mode is on, then we need to copy the old data file over as well.
				    //
				    if (diskBasedBinaryStorageMode == true)
				    {
				    	Byte[] binaryData = LoadDataFromDisk(oldClient.objectGuid, oldClient.versionNumber, "data");

				    	//
				    	// Write out the data as the new version
				    	//
				    	WriteDataToDisk(client.objectGuid, client.versionNumber, binaryData, "data");
				    }

				    string serializedClient = JsonSerializer.Serialize(client);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ClientChangeHistory newClientChangeHistory = new ClientChangeHistory();
				        newClientChangeHistory.clientId = client.id;
				        newClientChangeHistory.versionNumber = client.versionNumber;
				        newClientChangeHistory.timeStamp = DateTime.UtcNow;
				        newClientChangeHistory.userId = securityUser.id;
				        newClientChangeHistory.tenantGuid = userTenantGuid;
				        newClientChangeHistory.data = JsonSerializer.Serialize(Database.Client.CreateAnonymousWithFirstLevelSubObjects(client));
				        _context.ClientChangeHistories.Add(newClientChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Client rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Client.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Client.CreateAnonymousWithFirstLevelSubObjects(client)),
						null);


				    return Ok(Database.Client.CreateAnonymous(client));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.Client rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.Client rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}


        /// <summary>
        /// 
        /// This deletes a Client record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Client/{id}")]
		[Route("api/Client")]
		public async Task<IActionResult> DeleteClient(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.Client> query = (from x in _context.Clients
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Client client = await query.FirstOrDefaultAsync(cancellationToken);

			if (client == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Client DELETE", id.ToString(), new Exception("No Scheduler.Client entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Client cloneOfExisting = (Database.Client)_context.Entry(client).GetDatabaseValues().ToObject();


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			lock (clientDeleteSyncRoot)
			{
			    try
			    {
			        client.deleted = true;
			        client.versionNumber++;

			        _context.SaveChanges();

			        //
			        // If in disk based storage mode, create a copy of the disk data file for the new version.
			        //
			        if (diskBasedBinaryStorageMode == true)
			        {
			        	Byte[] binaryData = LoadDataFromDisk(client.objectGuid, client.versionNumber -1, "data");

			        	//
			        	// Write out the same data
			        	//
			        	WriteDataToDisk(client.objectGuid, client.versionNumber, binaryData, "data");
			        }

			        //
			        // Now add the change history
			        //
			        ClientChangeHistory clientChangeHistory = new ClientChangeHistory();
			        clientChangeHistory.clientId = client.id;
			        clientChangeHistory.versionNumber = client.versionNumber;
			        clientChangeHistory.timeStamp = DateTime.UtcNow;
			        clientChangeHistory.userId = securityUser.id;
			        clientChangeHistory.tenantGuid = userTenantGuid;
			        clientChangeHistory.data = JsonSerializer.Serialize(Database.Client.CreateAnonymousWithFirstLevelSubObjects(client));
			        _context.ClientChangeHistories.Add(clientChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Client entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Client.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Client.CreateAnonymousWithFirstLevelSubObjects(client)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Client entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Client.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Client.CreateAnonymousWithFirstLevelSubObjects(client)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of Client records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Clients/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			int? clientTypeId = null,
			int? currencyId = null,
			int? timeZoneId = null,
			int? calendarId = null,
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

			IQueryable<Database.Client> query = (from c in _context.Clients select c);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(c => c.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(c => c.description == description);
			}
			if (clientTypeId.HasValue == true)
			{
				query = query.Where(c => c.clientTypeId == clientTypeId.Value);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(c => c.currencyId == currencyId.Value);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(c => c.timeZoneId == timeZoneId.Value);
			}
			if (calendarId.HasValue == true)
			{
				query = query.Where(c => c.calendarId == calendarId.Value);
			}
			if (string.IsNullOrEmpty(addressLine1) == false)
			{
				query = query.Where(c => c.addressLine1 == addressLine1);
			}
			if (string.IsNullOrEmpty(addressLine2) == false)
			{
				query = query.Where(c => c.addressLine2 == addressLine2);
			}
			if (string.IsNullOrEmpty(city) == false)
			{
				query = query.Where(c => c.city == city);
			}
			if (string.IsNullOrEmpty(postalCode) == false)
			{
				query = query.Where(c => c.postalCode == postalCode);
			}
			if (stateProvinceId.HasValue == true)
			{
				query = query.Where(c => c.stateProvinceId == stateProvinceId.Value);
			}
			if (countryId.HasValue == true)
			{
				query = query.Where(c => c.countryId == countryId.Value);
			}
			if (string.IsNullOrEmpty(phone) == false)
			{
				query = query.Where(c => c.phone == phone);
			}
			if (string.IsNullOrEmpty(email) == false)
			{
				query = query.Where(c => c.email == email);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(c => c.notes == notes);
			}
			if (string.IsNullOrEmpty(externalId) == false)
			{
				query = query.Where(c => c.externalId == externalId);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(c => c.color == color);
			}
			if (string.IsNullOrEmpty(attributes) == false)
			{
				query = query.Where(c => c.attributes == attributes);
			}
			if (string.IsNullOrEmpty(avatarFileName) == false)
			{
				query = query.Where(c => c.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(c => c.avatarSize == avatarSize.Value);
			}
			if (string.IsNullOrEmpty(avatarMimeType) == false)
			{
				query = query.Where(c => c.avatarMimeType == avatarMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(c => c.versionNumber == versionNumber.Value);
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
			// Add the any string contains parameter to span all the string fields on the Client, or on an any of the string fields on its immediate relations
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
			       || x.calendar.name.Contains(anyStringContains)
			       || x.calendar.description.Contains(anyStringContains)
			       || x.calendar.color.Contains(anyStringContains)
			       || x.clientType.name.Contains(anyStringContains)
			       || x.clientType.description.Contains(anyStringContains)
			       || x.clientType.color.Contains(anyStringContains)
			       || x.country.name.Contains(anyStringContains)
			       || x.country.description.Contains(anyStringContains)
			       || x.country.abbreviation.Contains(anyStringContains)
			       || x.country.postalCodeFormat.Contains(anyStringContains)
			       || x.country.postalCodeRegEx.Contains(anyStringContains)
			       || x.currency.name.Contains(anyStringContains)
			       || x.currency.description.Contains(anyStringContains)
			       || x.currency.code.Contains(anyStringContains)
			       || x.currency.color.Contains(anyStringContains)
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
			return Ok(await (from queryData in query select Database.Client.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Client/CreateAuditEvent")]
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
        /// This makes a Client record a favourite for the current user.
		/// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/Client/Favourite/{id}")]
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


			IQueryable<Database.Client> query = (from x in _context.Clients
			                               where x.id == id
			                               select x);


			Database.Client client = await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

			if (client != null)
			{
				if (string.IsNullOrEmpty(description) == true)
				{
					description = client.name;
				}

				//
				// Add the user favourite Client
				//
				await SecurityLogic.AddToUserFavouritesAsync(securityUser, "Client", id, description, cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'Client' was added for record with id of " + id + " for user " + securityUser.accountName, true);

				//
				// Return the complete list of user favourites after the addition
				//
				return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
			}
			else
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'Client' add request was made with an invalid id value of " + id, false);
				return BadRequest();
			}
		}


        /// <summary>
        /// 
        /// This removes a Client record from the current user's favourites.
        /// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/Client/Favourite/{id}")]
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
			// Delete the user favourite Client
			//
			await SecurityLogic.RemoveFromUserFavouritesAsync(securityUser, "Client", id, cancellationToken);

			await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'Client' was removed for record with id of " + id + " for user " + securityUser.accountName, true);

			//
			// Return the complete list of user favourites after the deletion
			//
			return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
		}




        [Route("api/Client/Data/{id:int}")]
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


            Database.Client client = await (from x in _context.Clients where x.id == id && x.active == true && x.deleted == false select x).FirstOrDefaultAsync();
            if (client == null)
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

						lock (clientPutSyncRoot)
						{
							try
							{
								using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
								{
									client.avatarFileName = fileName.Trim();
									client.avatarMimeType = mimeType;
									client.avatarSize = section.Body.Length;

									client.versionNumber++;

									if (diskBasedBinaryStorageMode == true &&
										 client.avatarFileName != null &&
										 client.avatarSize > 0)
									{
										//
										// write the bytes to disk
										//
										WriteDataToDisk(client.objectGuid, client.versionNumber, section.Body, "data");
										//
										// Clear the data from the object before we put it into the db
										//
										client.avatarData = null;
									}
									else
									{
										using (MemoryStream memoryStream = new MemoryStream((int)section.Body.Length))
										{
											section.Body.CopyTo(memoryStream);
											client.avatarData = memoryStream.ToArray();
										}
									}
									//
									// Now add the change history
									//
									ClientChangeHistory clientChangeHistory = new ClientChangeHistory();
									clientChangeHistory.clientId = client.id;
									clientChangeHistory.versionNumber = client.versionNumber;
									clientChangeHistory.timeStamp = DateTime.UtcNow;
									clientChangeHistory.userId = securityUser.id;
									clientChangeHistory.tenantGuid = client.tenantGuid;
									clientChangeHistory.data = JsonSerializer.Serialize(Database.Client.CreateAnonymousWithFirstLevelSubObjects(client));
									_context.ClientChangeHistories.Add(clientChangeHistory);

									_context.SaveChanges();

									transaction.Commit();

									CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Client Data Uploaded with filename of " + fileName + " and with size of " + section.Body.Length, id.ToString());
								}
							}
							catch (Exception ex)
							{
								CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "Client Data Upload Failed.", false, id.ToString(), "", "", ex);

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
                lock (clientPutSyncRoot)
                {
                    try
                    {
                        using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                        {
                            if (diskBasedBinaryStorageMode == true)
                            {
								DeleteDataFromDisk(client.objectGuid, client.versionNumber, "data");
                            }

                            client.avatarFileName = null;
                            client.avatarMimeType = null;
                            client.avatarSize = 0;
                            client.avatarData = null;
                            client.versionNumber++;


                            //
                            // Now add the change history
                            //
                            ClientChangeHistory clientChangeHistory = new ClientChangeHistory();
                            clientChangeHistory.clientId = client.id;
                            clientChangeHistory.versionNumber = client.versionNumber;
                            clientChangeHistory.timeStamp = DateTime.UtcNow;
                            clientChangeHistory.userId = securityUser.id;
                                    clientChangeHistory.tenantGuid = client.tenantGuid;
                                    clientChangeHistory.data = JsonSerializer.Serialize(Database.Client.CreateAnonymousWithFirstLevelSubObjects(client));
                            _context.ClientChangeHistories.Add(clientChangeHistory);

                            _context.SaveChanges();

                            transaction.Commit();

                            CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Client data cleared.", id.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "Client data clear failed.", false, id.ToString(), "", "", ex);

                        return Problem(ex.Message);
                    }
                }
            }

            return Ok();
        }

        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/Client/Data/{id:int}")]
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
                Database.Client client = await (from d in context.Clients
                                                where d.id == id &&
                                                d.active == true &&
                                                d.deleted == false
                                                select d).FirstOrDefaultAsync();

                if (client != null && client.avatarData != null)
                {
                   return File(client.avatarData.ToArray<byte>(), client.avatarMimeType, client.avatarFileName != null ? client.avatarFileName.Trim() : "Client_" + client.id.ToString(), true);
                }
                else
                {
                    return BadRequest();
                }
            }
        }
	}
}
