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
    /// This auto generated class provides the basic CRUD operations for the SecurityUser entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SecurityUser entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SecurityUsersController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SecurityContext _context;

		private ILogger<SecurityUsersController> _logger;

		public SecurityUsersController(SecurityContext context, ILogger<SecurityUsersController> logger) : base("Security", "SecurityUser")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This gets a list of SecurityUsers filtered by the parameters provided.
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
		[Route("api/SecurityUsers")]
		public async Task<IActionResult> GetSecurityUsers(
			string accountName = null,
			bool? activeDirectoryAccount = null,
			bool? canLogin = null,
			bool? mustChangePassword = null,
			string firstName = null,
			string middleName = null,
			string lastName = null,
			DateTime? dateOfBirth = null,
			string emailAddress = null,
			string cellPhoneNumber = null,
			string phoneNumber = null,
			string phoneExtension = null,
			string description = null,
			int? securityUserTitleId = null,
			int? reportsToSecurityUserId = null,
			string authenticationDomain = null,
			int? failedLoginCount = null,
			DateTime? lastLoginAttempt = null,
			DateTime? mostRecentActivity = null,
			string alternateIdentifier = null,
			string settings = null,
			int? securityTenantId = null,
			int? readPermissionLevel = null,
			int? writePermissionLevel = null,
			int? securityOrganizationId = null,
			int? securityDepartmentId = null,
			int? securityTeamId = null,
			string authenticationToken = null,
			DateTime? authenticationTokenExpiry = null,
			string twoFactorToken = null,
			DateTime? twoFactorTokenExpiry = null,
			bool? twoFactorSendByEmail = null,
			bool? twoFactorSendBySMS = null,
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
			if (dateOfBirth.HasValue == true && dateOfBirth.Value.Kind != DateTimeKind.Utc)
			{
				dateOfBirth = dateOfBirth.Value.ToUniversalTime();
			}

			if (lastLoginAttempt.HasValue == true && lastLoginAttempt.Value.Kind != DateTimeKind.Utc)
			{
				lastLoginAttempt = lastLoginAttempt.Value.ToUniversalTime();
			}

			if (mostRecentActivity.HasValue == true && mostRecentActivity.Value.Kind != DateTimeKind.Utc)
			{
				mostRecentActivity = mostRecentActivity.Value.ToUniversalTime();
			}

			if (authenticationTokenExpiry.HasValue == true && authenticationTokenExpiry.Value.Kind != DateTimeKind.Utc)
			{
				authenticationTokenExpiry = authenticationTokenExpiry.Value.ToUniversalTime();
			}

			if (twoFactorTokenExpiry.HasValue == true && twoFactorTokenExpiry.Value.Kind != DateTimeKind.Utc)
			{
				twoFactorTokenExpiry = twoFactorTokenExpiry.Value.ToUniversalTime();
			}

			IQueryable<Database.SecurityUser> query = (from su in _context.SecurityUsers select su);
			if (string.IsNullOrEmpty(accountName) == false)
			{
				query = query.Where(su => su.accountName == accountName);
			}
			if (activeDirectoryAccount.HasValue == true)
			{
				query = query.Where(su => su.activeDirectoryAccount == activeDirectoryAccount.Value);
			}
			if (canLogin.HasValue == true)
			{
				query = query.Where(su => su.canLogin == canLogin.Value);
			}
			if (mustChangePassword.HasValue == true)
			{
				query = query.Where(su => su.mustChangePassword == mustChangePassword.Value);
			}
			if (string.IsNullOrEmpty(firstName) == false)
			{
				query = query.Where(su => su.firstName == firstName);
			}
			if (string.IsNullOrEmpty(middleName) == false)
			{
				query = query.Where(su => su.middleName == middleName);
			}
			if (string.IsNullOrEmpty(lastName) == false)
			{
				query = query.Where(su => su.lastName == lastName);
			}
			if (dateOfBirth.HasValue == true)
			{
				query = query.Where(su => su.dateOfBirth == dateOfBirth.Value);
			}
			if (string.IsNullOrEmpty(emailAddress) == false)
			{
				query = query.Where(su => su.emailAddress == emailAddress);
			}
			if (string.IsNullOrEmpty(cellPhoneNumber) == false)
			{
				query = query.Where(su => su.cellPhoneNumber == cellPhoneNumber);
			}
			if (string.IsNullOrEmpty(phoneNumber) == false)
			{
				query = query.Where(su => su.phoneNumber == phoneNumber);
			}
			if (string.IsNullOrEmpty(phoneExtension) == false)
			{
				query = query.Where(su => su.phoneExtension == phoneExtension);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(su => su.description == description);
			}
			if (securityUserTitleId.HasValue == true)
			{
				query = query.Where(su => su.securityUserTitleId == securityUserTitleId.Value);
			}
			if (reportsToSecurityUserId.HasValue == true)
			{
				query = query.Where(su => su.reportsToSecurityUserId == reportsToSecurityUserId.Value);
			}
			if (string.IsNullOrEmpty(authenticationDomain) == false)
			{
				query = query.Where(su => su.authenticationDomain == authenticationDomain);
			}
			if (failedLoginCount.HasValue == true)
			{
				query = query.Where(su => su.failedLoginCount == failedLoginCount.Value);
			}
			if (lastLoginAttempt.HasValue == true)
			{
				query = query.Where(su => su.lastLoginAttempt == lastLoginAttempt.Value);
			}
			if (mostRecentActivity.HasValue == true)
			{
				query = query.Where(su => su.mostRecentActivity == mostRecentActivity.Value);
			}
			if (string.IsNullOrEmpty(alternateIdentifier) == false)
			{
				query = query.Where(su => su.alternateIdentifier == alternateIdentifier);
			}
			if (string.IsNullOrEmpty(settings) == false)
			{
				query = query.Where(su => su.settings == settings);
			}
			if (securityTenantId.HasValue == true)
			{
				query = query.Where(su => su.securityTenantId == securityTenantId.Value);
			}
			if (readPermissionLevel.HasValue == true)
			{
				query = query.Where(su => su.readPermissionLevel == readPermissionLevel.Value);
			}
			if (writePermissionLevel.HasValue == true)
			{
				query = query.Where(su => su.writePermissionLevel == writePermissionLevel.Value);
			}
			if (securityOrganizationId.HasValue == true)
			{
				query = query.Where(su => su.securityOrganizationId == securityOrganizationId.Value);
			}
			if (securityDepartmentId.HasValue == true)
			{
				query = query.Where(su => su.securityDepartmentId == securityDepartmentId.Value);
			}
			if (securityTeamId.HasValue == true)
			{
				query = query.Where(su => su.securityTeamId == securityTeamId.Value);
			}
			if (string.IsNullOrEmpty(authenticationToken) == false)
			{
				query = query.Where(su => su.authenticationToken == authenticationToken);
			}
			if (authenticationTokenExpiry.HasValue == true)
			{
				query = query.Where(su => su.authenticationTokenExpiry == authenticationTokenExpiry.Value);
			}
			if (string.IsNullOrEmpty(twoFactorToken) == false)
			{
				query = query.Where(su => su.twoFactorToken == twoFactorToken);
			}
			if (twoFactorTokenExpiry.HasValue == true)
			{
				query = query.Where(su => su.twoFactorTokenExpiry == twoFactorTokenExpiry.Value);
			}
			if (twoFactorSendByEmail.HasValue == true)
			{
				query = query.Where(su => su.twoFactorSendByEmail == twoFactorSendByEmail.Value);
			}
			if (twoFactorSendBySMS.HasValue == true)
			{
				query = query.Where(su => su.twoFactorSendBySMS == twoFactorSendBySMS.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(su => su.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(su => su.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(su => su.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(su => su.deleted == false);
				}
			}
			else
			{
				query = query.Where(su => su.active == true);
				query = query.Where(su => su.deleted == false);
			}

			query = query.OrderBy(su => su.accountName);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.reportsToSecurityUser);
				query = query.Include(x => x.securityDepartment);
				query = query.Include(x => x.securityOrganization);
				query = query.Include(x => x.securityTeam);
				query = query.Include(x => x.securityTenant);
				query = query.Include(x => x.securityUserTitle);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Security User, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.accountName.Contains(anyStringContains)
			       || x.password.Contains(anyStringContains)
			       || x.firstName.Contains(anyStringContains)
			       || x.middleName.Contains(anyStringContains)
			       || x.lastName.Contains(anyStringContains)
			       || x.emailAddress.Contains(anyStringContains)
			       || x.cellPhoneNumber.Contains(anyStringContains)
			       || x.phoneNumber.Contains(anyStringContains)
			       || x.phoneExtension.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.authenticationDomain.Contains(anyStringContains)
			       || x.alternateIdentifier.Contains(anyStringContains)
			       || x.settings.Contains(anyStringContains)
			       || x.authenticationToken.Contains(anyStringContains)
			       || x.twoFactorToken.Contains(anyStringContains)
			       || (includeRelations == true && x.reportsToSecurityUser.accountName.Contains(anyStringContains))
			       || (includeRelations == true && x.reportsToSecurityUser.password.Contains(anyStringContains))
			       || (includeRelations == true && x.reportsToSecurityUser.firstName.Contains(anyStringContains))
			       || (includeRelations == true && x.reportsToSecurityUser.middleName.Contains(anyStringContains))
			       || (includeRelations == true && x.reportsToSecurityUser.lastName.Contains(anyStringContains))
			       || (includeRelations == true && x.reportsToSecurityUser.emailAddress.Contains(anyStringContains))
			       || (includeRelations == true && x.reportsToSecurityUser.cellPhoneNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.reportsToSecurityUser.phoneNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.reportsToSecurityUser.phoneExtension.Contains(anyStringContains))
			       || (includeRelations == true && x.reportsToSecurityUser.description.Contains(anyStringContains))
			       || (includeRelations == true && x.reportsToSecurityUser.authenticationDomain.Contains(anyStringContains))
			       || (includeRelations == true && x.reportsToSecurityUser.alternateIdentifier.Contains(anyStringContains))
			       || (includeRelations == true && x.reportsToSecurityUser.settings.Contains(anyStringContains))
			       || (includeRelations == true && x.reportsToSecurityUser.authenticationToken.Contains(anyStringContains))
			       || (includeRelations == true && x.reportsToSecurityUser.twoFactorToken.Contains(anyStringContains))
			       || (includeRelations == true && x.securityDepartment.name.Contains(anyStringContains))
			       || (includeRelations == true && x.securityDepartment.description.Contains(anyStringContains))
			       || (includeRelations == true && x.securityOrganization.name.Contains(anyStringContains))
			       || (includeRelations == true && x.securityOrganization.description.Contains(anyStringContains))
			       || (includeRelations == true && x.securityTeam.name.Contains(anyStringContains))
			       || (includeRelations == true && x.securityTeam.description.Contains(anyStringContains))
			       || (includeRelations == true && x.securityTenant.name.Contains(anyStringContains))
			       || (includeRelations == true && x.securityTenant.description.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUserTitle.name.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUserTitle.description.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.SecurityUser> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SecurityUser securityUser in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(securityUser, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Security.SecurityUser Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Security.SecurityUser Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
		
*/
		
        /// <summary>
        /// 
        /// This returns a row count of SecurityUsers filtered by the parameters provided.  Its query is similar to the GetSecurityUsers method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityUsers/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string accountName = null,
			bool? activeDirectoryAccount = null,
			bool? canLogin = null,
			bool? mustChangePassword = null,
			string firstName = null,
			string middleName = null,
			string lastName = null,
			DateTime? dateOfBirth = null,
			string emailAddress = null,
			string cellPhoneNumber = null,
			string phoneNumber = null,
			string phoneExtension = null,
			string description = null,
			int? securityUserTitleId = null,
			int? reportsToSecurityUserId = null,
			string authenticationDomain = null,
			int? failedLoginCount = null,
			DateTime? lastLoginAttempt = null,
			DateTime? mostRecentActivity = null,
			string alternateIdentifier = null,
			string settings = null,
			int? securityTenantId = null,
			int? readPermissionLevel = null,
			int? writePermissionLevel = null,
			int? securityOrganizationId = null,
			int? securityDepartmentId = null,
			int? securityTeamId = null,
			string authenticationToken = null,
			DateTime? authenticationTokenExpiry = null,
			string twoFactorToken = null,
			DateTime? twoFactorTokenExpiry = null,
			bool? twoFactorSendByEmail = null,
			bool? twoFactorSendBySMS = null,
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

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (dateOfBirth.HasValue == true && dateOfBirth.Value.Kind != DateTimeKind.Utc)
			{
				dateOfBirth = dateOfBirth.Value.ToUniversalTime();
			}

			if (lastLoginAttempt.HasValue == true && lastLoginAttempt.Value.Kind != DateTimeKind.Utc)
			{
				lastLoginAttempt = lastLoginAttempt.Value.ToUniversalTime();
			}

			if (mostRecentActivity.HasValue == true && mostRecentActivity.Value.Kind != DateTimeKind.Utc)
			{
				mostRecentActivity = mostRecentActivity.Value.ToUniversalTime();
			}

			if (authenticationTokenExpiry.HasValue == true && authenticationTokenExpiry.Value.Kind != DateTimeKind.Utc)
			{
				authenticationTokenExpiry = authenticationTokenExpiry.Value.ToUniversalTime();
			}

			if (twoFactorTokenExpiry.HasValue == true && twoFactorTokenExpiry.Value.Kind != DateTimeKind.Utc)
			{
				twoFactorTokenExpiry = twoFactorTokenExpiry.Value.ToUniversalTime();
			}

			IQueryable<Database.SecurityUser> query = (from su in _context.SecurityUsers select su);
			if (accountName != null)
			{
				query = query.Where(su => su.accountName == accountName);
			}
			if (activeDirectoryAccount.HasValue == true)
			{
				query = query.Where(su => su.activeDirectoryAccount == activeDirectoryAccount.Value);
			}
			if (canLogin.HasValue == true)
			{
				query = query.Where(su => su.canLogin == canLogin.Value);
			}
			if (mustChangePassword.HasValue == true)
			{
				query = query.Where(su => su.mustChangePassword == mustChangePassword.Value);
			}
			if (firstName != null)
			{
				query = query.Where(su => su.firstName == firstName);
			}
			if (middleName != null)
			{
				query = query.Where(su => su.middleName == middleName);
			}
			if (lastName != null)
			{
				query = query.Where(su => su.lastName == lastName);
			}
			if (dateOfBirth.HasValue == true)
			{
				query = query.Where(su => su.dateOfBirth == dateOfBirth.Value);
			}
			if (emailAddress != null)
			{
				query = query.Where(su => su.emailAddress == emailAddress);
			}
			if (cellPhoneNumber != null)
			{
				query = query.Where(su => su.cellPhoneNumber == cellPhoneNumber);
			}
			if (phoneNumber != null)
			{
				query = query.Where(su => su.phoneNumber == phoneNumber);
			}
			if (phoneExtension != null)
			{
				query = query.Where(su => su.phoneExtension == phoneExtension);
			}
			if (description != null)
			{
				query = query.Where(su => su.description == description);
			}
			if (securityUserTitleId.HasValue == true)
			{
				query = query.Where(su => su.securityUserTitleId == securityUserTitleId.Value);
			}
			if (reportsToSecurityUserId.HasValue == true)
			{
				query = query.Where(su => su.reportsToSecurityUserId == reportsToSecurityUserId.Value);
			}
			if (authenticationDomain != null)
			{
				query = query.Where(su => su.authenticationDomain == authenticationDomain);
			}
			if (failedLoginCount.HasValue == true)
			{
				query = query.Where(su => su.failedLoginCount == failedLoginCount.Value);
			}
			if (lastLoginAttempt.HasValue == true)
			{
				query = query.Where(su => su.lastLoginAttempt == lastLoginAttempt.Value);
			}
			if (mostRecentActivity.HasValue == true)
			{
				query = query.Where(su => su.mostRecentActivity == mostRecentActivity.Value);
			}
			if (alternateIdentifier != null)
			{
				query = query.Where(su => su.alternateIdentifier == alternateIdentifier);
			}
			if (settings != null)
			{
				query = query.Where(su => su.settings == settings);
			}
			if (securityTenantId.HasValue == true)
			{
				query = query.Where(su => su.securityTenantId == securityTenantId.Value);
			}
			if (readPermissionLevel.HasValue == true)
			{
				query = query.Where(su => su.readPermissionLevel == readPermissionLevel.Value);
			}
			if (writePermissionLevel.HasValue == true)
			{
				query = query.Where(su => su.writePermissionLevel == writePermissionLevel.Value);
			}
			if (securityOrganizationId.HasValue == true)
			{
				query = query.Where(su => su.securityOrganizationId == securityOrganizationId.Value);
			}
			if (securityDepartmentId.HasValue == true)
			{
				query = query.Where(su => su.securityDepartmentId == securityDepartmentId.Value);
			}
			if (securityTeamId.HasValue == true)
			{
				query = query.Where(su => su.securityTeamId == securityTeamId.Value);
			}
			if (authenticationToken != null)
			{
				query = query.Where(su => su.authenticationToken == authenticationToken);
			}
			if (authenticationTokenExpiry.HasValue == true)
			{
				query = query.Where(su => su.authenticationTokenExpiry == authenticationTokenExpiry.Value);
			}
			if (twoFactorToken != null)
			{
				query = query.Where(su => su.twoFactorToken == twoFactorToken);
			}
			if (twoFactorTokenExpiry.HasValue == true)
			{
				query = query.Where(su => su.twoFactorTokenExpiry == twoFactorTokenExpiry.Value);
			}
			if (twoFactorSendByEmail.HasValue == true)
			{
				query = query.Where(su => su.twoFactorSendByEmail == twoFactorSendByEmail.Value);
			}
			if (twoFactorSendBySMS.HasValue == true)
			{
				query = query.Where(su => su.twoFactorSendBySMS == twoFactorSendBySMS.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(su => su.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(su => su.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(su => su.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(su => su.deleted == false);
				}
			}
			else
			{
				query = query.Where(su => su.active == true);
				query = query.Where(su => su.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Security User, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.accountName.Contains(anyStringContains)
			       || x.password.Contains(anyStringContains)
			       || x.firstName.Contains(anyStringContains)
			       || x.middleName.Contains(anyStringContains)
			       || x.lastName.Contains(anyStringContains)
			       || x.emailAddress.Contains(anyStringContains)
			       || x.cellPhoneNumber.Contains(anyStringContains)
			       || x.phoneNumber.Contains(anyStringContains)
			       || x.phoneExtension.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.authenticationDomain.Contains(anyStringContains)
			       || x.alternateIdentifier.Contains(anyStringContains)
			       || x.settings.Contains(anyStringContains)
			       || x.authenticationToken.Contains(anyStringContains)
			       || x.twoFactorToken.Contains(anyStringContains)
			       || x.reportsToSecurityUser.accountName.Contains(anyStringContains)
			       || x.reportsToSecurityUser.password.Contains(anyStringContains)
			       || x.reportsToSecurityUser.firstName.Contains(anyStringContains)
			       || x.reportsToSecurityUser.middleName.Contains(anyStringContains)
			       || x.reportsToSecurityUser.lastName.Contains(anyStringContains)
			       || x.reportsToSecurityUser.emailAddress.Contains(anyStringContains)
			       || x.reportsToSecurityUser.cellPhoneNumber.Contains(anyStringContains)
			       || x.reportsToSecurityUser.phoneNumber.Contains(anyStringContains)
			       || x.reportsToSecurityUser.phoneExtension.Contains(anyStringContains)
			       || x.reportsToSecurityUser.description.Contains(anyStringContains)
			       || x.reportsToSecurityUser.authenticationDomain.Contains(anyStringContains)
			       || x.reportsToSecurityUser.alternateIdentifier.Contains(anyStringContains)
			       || x.reportsToSecurityUser.settings.Contains(anyStringContains)
			       || x.reportsToSecurityUser.authenticationToken.Contains(anyStringContains)
			       || x.reportsToSecurityUser.twoFactorToken.Contains(anyStringContains)
			       || x.securityDepartment.name.Contains(anyStringContains)
			       || x.securityDepartment.description.Contains(anyStringContains)
			       || x.securityOrganization.name.Contains(anyStringContains)
			       || x.securityOrganization.description.Contains(anyStringContains)
			       || x.securityTeam.name.Contains(anyStringContains)
			       || x.securityTeam.description.Contains(anyStringContains)
			       || x.securityTenant.name.Contains(anyStringContains)
			       || x.securityTenant.description.Contains(anyStringContains)
			       || x.securityUserTitle.name.Contains(anyStringContains)
			       || x.securityUserTitle.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This gets a single SecurityUser by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityUser/{id}")]
		public async Task<IActionResult> GetSecurityUser(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.SecurityUser> query = (from su in _context.SecurityUsers where
							(su.id == id) &&
							(userIsAdmin == true || su.deleted == false) &&
							(userIsWriter == true || su.active == true)
					select su);

				if (includeRelations == true)
				{
					query = query.Include(x => x.reportsToSecurityUser);
					query = query.Include(x => x.securityDepartment);
					query = query.Include(x => x.securityOrganization);
					query = query.Include(x => x.securityTeam);
					query = query.Include(x => x.securityTenant);
					query = query.Include(x => x.securityUserTitle);
					query = query.AsSplitQuery();
				}

				Database.SecurityUser materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Security.SecurityUser Entity was read with Admin privilege." : "Security.SecurityUser Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityUser", materialized.id, materialized.accountName));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Security.SecurityUser entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Security.SecurityUser.   Entity was read with Admin privilege." : "Exception caught during entity read of Security.SecurityUser.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}
*/


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing SecurityUser record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SecurityUser/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSecurityUser(int id, [FromBody]Database.SecurityUser.SecurityUserDTO securityUserDTO, CancellationToken cancellationToken = default)
		{
			if (securityUserDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != securityUserDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.SecurityUser> query = (from x in _context.SecurityUsers
				where
				(x.id == id)
				select x);


			Database.SecurityUser existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityUser PUT", id.ToString(), new Exception("No Security.SecurityUser entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (securityUserDTO.objectGuid == Guid.Empty)
            {
                securityUserDTO.objectGuid = existing.objectGuid;
            }
            else if (securityUserDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a SecurityUser record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SecurityUser cloneOfExisting = (Database.SecurityUser)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SecurityUser object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SecurityUser securityUser = (Database.SecurityUser)_context.Entry(existing).GetDatabaseValues().ToObject();
			securityUser.ApplyDTO(securityUserDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (securityUser.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Security.SecurityUser record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (securityUser.accountName != null && securityUser.accountName.Length > 250)
			{
				securityUser.accountName = securityUser.accountName.Substring(0, 250);
			}

			if (securityUser.password != null && securityUser.password.Length > 250)
			{
				securityUser.password = securityUser.password.Substring(0, 250);
			}

			if (securityUser.firstName != null && securityUser.firstName.Length > 100)
			{
				securityUser.firstName = securityUser.firstName.Substring(0, 100);
			}

			if (securityUser.middleName != null && securityUser.middleName.Length > 100)
			{
				securityUser.middleName = securityUser.middleName.Substring(0, 100);
			}

			if (securityUser.lastName != null && securityUser.lastName.Length > 100)
			{
				securityUser.lastName = securityUser.lastName.Substring(0, 100);
			}

			if (securityUser.dateOfBirth.HasValue == true && securityUser.dateOfBirth.Value.Kind != DateTimeKind.Utc)
			{
				securityUser.dateOfBirth = securityUser.dateOfBirth.Value.ToUniversalTime();
			}

			if (securityUser.emailAddress != null && securityUser.emailAddress.Length > 100)
			{
				securityUser.emailAddress = securityUser.emailAddress.Substring(0, 100);
			}

			if (securityUser.cellPhoneNumber != null && securityUser.cellPhoneNumber.Length > 100)
			{
				securityUser.cellPhoneNumber = securityUser.cellPhoneNumber.Substring(0, 100);
			}

			if (securityUser.phoneNumber != null && securityUser.phoneNumber.Length > 50)
			{
				securityUser.phoneNumber = securityUser.phoneNumber.Substring(0, 50);
			}

			if (securityUser.phoneExtension != null && securityUser.phoneExtension.Length > 50)
			{
				securityUser.phoneExtension = securityUser.phoneExtension.Substring(0, 50);
			}

			if (securityUser.description != null && securityUser.description.Length > 500)
			{
				securityUser.description = securityUser.description.Substring(0, 500);
			}

			if (securityUser.authenticationDomain != null && securityUser.authenticationDomain.Length > 100)
			{
				securityUser.authenticationDomain = securityUser.authenticationDomain.Substring(0, 100);
			}

			if (securityUser.lastLoginAttempt.HasValue == true && securityUser.lastLoginAttempt.Value.Kind != DateTimeKind.Utc)
			{
				securityUser.lastLoginAttempt = securityUser.lastLoginAttempt.Value.ToUniversalTime();
			}

			if (securityUser.mostRecentActivity.HasValue == true && securityUser.mostRecentActivity.Value.Kind != DateTimeKind.Utc)
			{
				securityUser.mostRecentActivity = securityUser.mostRecentActivity.Value.ToUniversalTime();
			}

			if (securityUser.alternateIdentifier != null && securityUser.alternateIdentifier.Length > 100)
			{
				securityUser.alternateIdentifier = securityUser.alternateIdentifier.Substring(0, 100);
			}

			if (securityUser.authenticationToken != null && securityUser.authenticationToken.Length > 100)
			{
				securityUser.authenticationToken = securityUser.authenticationToken.Substring(0, 100);
			}

			if (securityUser.authenticationTokenExpiry.HasValue == true && securityUser.authenticationTokenExpiry.Value.Kind != DateTimeKind.Utc)
			{
				securityUser.authenticationTokenExpiry = securityUser.authenticationTokenExpiry.Value.ToUniversalTime();
			}

			if (securityUser.twoFactorToken != null && securityUser.twoFactorToken.Length > 10)
			{
				securityUser.twoFactorToken = securityUser.twoFactorToken.Substring(0, 10);
			}

			if (securityUser.twoFactorTokenExpiry.HasValue == true && securityUser.twoFactorTokenExpiry.Value.Kind != DateTimeKind.Utc)
			{
				securityUser.twoFactorTokenExpiry = securityUser.twoFactorTokenExpiry.Value.ToUniversalTime();
			}

			EntityEntry<Database.SecurityUser> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(securityUser);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityUser entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityUser.CreateAnonymousWithFirstLevelSubObjects(securityUser)),
					null);


				return Ok(Database.SecurityUser.CreateAnonymous(securityUser));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityUser entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityUser.CreateAnonymousWithFirstLevelSubObjects(securityUser)),
					ex);

				return Problem(ex.Message);
			}

		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new SecurityUser record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityUser", Name = "SecurityUser")]
		public async Task<IActionResult> PostSecurityUser([FromBody]Database.SecurityUser.SecurityUserDTO securityUserDTO, CancellationToken cancellationToken = default)
		{
			if (securityUserDTO == null)
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
			// Create a new SecurityUser object using the data from the DTO
			//
			Database.SecurityUser securityUser = Database.SecurityUser.FromDTO(securityUserDTO);

			try
			{
				if (securityUser.accountName != null && securityUser.accountName.Length > 250)
				{
					securityUser.accountName = securityUser.accountName.Substring(0, 250);
				}

				if (securityUser.password != null && securityUser.password.Length > 250)
				{
					securityUser.password = securityUser.password.Substring(0, 250);
				}

				if (securityUser.firstName != null && securityUser.firstName.Length > 100)
				{
					securityUser.firstName = securityUser.firstName.Substring(0, 100);
				}

				if (securityUser.middleName != null && securityUser.middleName.Length > 100)
				{
					securityUser.middleName = securityUser.middleName.Substring(0, 100);
				}

				if (securityUser.lastName != null && securityUser.lastName.Length > 100)
				{
					securityUser.lastName = securityUser.lastName.Substring(0, 100);
				}

				if (securityUser.dateOfBirth.HasValue == true && securityUser.dateOfBirth.Value.Kind != DateTimeKind.Utc)
				{
					securityUser.dateOfBirth = securityUser.dateOfBirth.Value.ToUniversalTime();
				}

				if (securityUser.emailAddress != null && securityUser.emailAddress.Length > 100)
				{
					securityUser.emailAddress = securityUser.emailAddress.Substring(0, 100);
				}

				if (securityUser.cellPhoneNumber != null && securityUser.cellPhoneNumber.Length > 100)
				{
					securityUser.cellPhoneNumber = securityUser.cellPhoneNumber.Substring(0, 100);
				}

				if (securityUser.phoneNumber != null && securityUser.phoneNumber.Length > 50)
				{
					securityUser.phoneNumber = securityUser.phoneNumber.Substring(0, 50);
				}

				if (securityUser.phoneExtension != null && securityUser.phoneExtension.Length > 50)
				{
					securityUser.phoneExtension = securityUser.phoneExtension.Substring(0, 50);
				}

				if (securityUser.description != null && securityUser.description.Length > 500)
				{
					securityUser.description = securityUser.description.Substring(0, 500);
				}

				if (securityUser.authenticationDomain != null && securityUser.authenticationDomain.Length > 100)
				{
					securityUser.authenticationDomain = securityUser.authenticationDomain.Substring(0, 100);
				}

				if (securityUser.lastLoginAttempt.HasValue == true && securityUser.lastLoginAttempt.Value.Kind != DateTimeKind.Utc)
				{
					securityUser.lastLoginAttempt = securityUser.lastLoginAttempt.Value.ToUniversalTime();
				}

				if (securityUser.mostRecentActivity.HasValue == true && securityUser.mostRecentActivity.Value.Kind != DateTimeKind.Utc)
				{
					securityUser.mostRecentActivity = securityUser.mostRecentActivity.Value.ToUniversalTime();
				}

				if (securityUser.alternateIdentifier != null && securityUser.alternateIdentifier.Length > 100)
				{
					securityUser.alternateIdentifier = securityUser.alternateIdentifier.Substring(0, 100);
				}

				if (securityUser.authenticationToken != null && securityUser.authenticationToken.Length > 100)
				{
					securityUser.authenticationToken = securityUser.authenticationToken.Substring(0, 100);
				}

				if (securityUser.authenticationTokenExpiry.HasValue == true && securityUser.authenticationTokenExpiry.Value.Kind != DateTimeKind.Utc)
				{
					securityUser.authenticationTokenExpiry = securityUser.authenticationTokenExpiry.Value.ToUniversalTime();
				}

				if (securityUser.twoFactorToken != null && securityUser.twoFactorToken.Length > 10)
				{
					securityUser.twoFactorToken = securityUser.twoFactorToken.Substring(0, 10);
				}

				if (securityUser.twoFactorTokenExpiry.HasValue == true && securityUser.twoFactorTokenExpiry.Value.Kind != DateTimeKind.Utc)
				{
					securityUser.twoFactorTokenExpiry = securityUser.twoFactorTokenExpiry.Value.ToUniversalTime();
				}

				securityUser.objectGuid = Guid.NewGuid();
				_context.SecurityUsers.Add(securityUser);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Security.SecurityUser entity successfully created.",
					true,
					securityUser.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.SecurityUser.CreateAnonymousWithFirstLevelSubObjects(securityUser)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Security.SecurityUser entity creation failed.", false, securityUser.id.ToString(), "", JsonSerializer.Serialize(securityUser), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityUser", securityUser.id, securityUser.accountName));

			return CreatedAtRoute("SecurityUser", new { id = securityUser.id }, Database.SecurityUser.CreateAnonymousWithFirstLevelSubObjects(securityUser));
		}

*/


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a SecurityUser record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityUser/{id}")]
		[Route("api/SecurityUser")]
		public async Task<IActionResult> DeleteSecurityUser(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.SecurityUser> query = (from x in _context.SecurityUsers
				where
				(x.id == id)
				select x);


			Database.SecurityUser securityUser = await query.FirstOrDefaultAsync(cancellationToken);

			if (securityUser == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityUser DELETE", id.ToString(), new Exception("No Security.SecurityUser entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SecurityUser cloneOfExisting = (Database.SecurityUser)_context.Entry(securityUser).GetDatabaseValues().ToObject();


			try
			{
				securityUser.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityUser entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityUser.CreateAnonymousWithFirstLevelSubObjects(securityUser)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityUser entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityUser.CreateAnonymousWithFirstLevelSubObjects(securityUser)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


*/
/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This gets a list of SecurityUser records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SecurityUsers/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string accountName = null,
			bool? activeDirectoryAccount = null,
			bool? canLogin = null,
			bool? mustChangePassword = null,
			string firstName = null,
			string middleName = null,
			string lastName = null,
			DateTime? dateOfBirth = null,
			string emailAddress = null,
			string cellPhoneNumber = null,
			string phoneNumber = null,
			string phoneExtension = null,
			string description = null,
			int? securityUserTitleId = null,
			int? reportsToSecurityUserId = null,
			string authenticationDomain = null,
			int? failedLoginCount = null,
			DateTime? lastLoginAttempt = null,
			DateTime? mostRecentActivity = null,
			string alternateIdentifier = null,
			string settings = null,
			int? securityTenantId = null,
			int? readPermissionLevel = null,
			int? writePermissionLevel = null,
			int? securityOrganizationId = null,
			int? securityDepartmentId = null,
			int? securityTeamId = null,
			string authenticationToken = null,
			DateTime? authenticationTokenExpiry = null,
			string twoFactorToken = null,
			DateTime? twoFactorTokenExpiry = null,
			bool? twoFactorSendByEmail = null,
			bool? twoFactorSendBySMS = null,
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
			if (dateOfBirth.HasValue == true && dateOfBirth.Value.Kind != DateTimeKind.Utc)
			{
				dateOfBirth = dateOfBirth.Value.ToUniversalTime();
			}

			if (lastLoginAttempt.HasValue == true && lastLoginAttempt.Value.Kind != DateTimeKind.Utc)
			{
				lastLoginAttempt = lastLoginAttempt.Value.ToUniversalTime();
			}

			if (mostRecentActivity.HasValue == true && mostRecentActivity.Value.Kind != DateTimeKind.Utc)
			{
				mostRecentActivity = mostRecentActivity.Value.ToUniversalTime();
			}

			if (authenticationTokenExpiry.HasValue == true && authenticationTokenExpiry.Value.Kind != DateTimeKind.Utc)
			{
				authenticationTokenExpiry = authenticationTokenExpiry.Value.ToUniversalTime();
			}

			if (twoFactorTokenExpiry.HasValue == true && twoFactorTokenExpiry.Value.Kind != DateTimeKind.Utc)
			{
				twoFactorTokenExpiry = twoFactorTokenExpiry.Value.ToUniversalTime();
			}

			IQueryable<Database.SecurityUser> query = (from su in _context.SecurityUsers select su);
			if (string.IsNullOrEmpty(accountName) == false)
			{
				query = query.Where(su => su.accountName == accountName);
			}
			if (activeDirectoryAccount.HasValue == true)
			{
				query = query.Where(su => su.activeDirectoryAccount == activeDirectoryAccount.Value);
			}
			if (canLogin.HasValue == true)
			{
				query = query.Where(su => su.canLogin == canLogin.Value);
			}
			if (mustChangePassword.HasValue == true)
			{
				query = query.Where(su => su.mustChangePassword == mustChangePassword.Value);
			}
			if (string.IsNullOrEmpty(firstName) == false)
			{
				query = query.Where(su => su.firstName == firstName);
			}
			if (string.IsNullOrEmpty(middleName) == false)
			{
				query = query.Where(su => su.middleName == middleName);
			}
			if (string.IsNullOrEmpty(lastName) == false)
			{
				query = query.Where(su => su.lastName == lastName);
			}
			if (dateOfBirth.HasValue == true)
			{
				query = query.Where(su => su.dateOfBirth == dateOfBirth.Value);
			}
			if (string.IsNullOrEmpty(emailAddress) == false)
			{
				query = query.Where(su => su.emailAddress == emailAddress);
			}
			if (string.IsNullOrEmpty(cellPhoneNumber) == false)
			{
				query = query.Where(su => su.cellPhoneNumber == cellPhoneNumber);
			}
			if (string.IsNullOrEmpty(phoneNumber) == false)
			{
				query = query.Where(su => su.phoneNumber == phoneNumber);
			}
			if (string.IsNullOrEmpty(phoneExtension) == false)
			{
				query = query.Where(su => su.phoneExtension == phoneExtension);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(su => su.description == description);
			}
			if (securityUserTitleId.HasValue == true)
			{
				query = query.Where(su => su.securityUserTitleId == securityUserTitleId.Value);
			}
			if (reportsToSecurityUserId.HasValue == true)
			{
				query = query.Where(su => su.reportsToSecurityUserId == reportsToSecurityUserId.Value);
			}
			if (string.IsNullOrEmpty(authenticationDomain) == false)
			{
				query = query.Where(su => su.authenticationDomain == authenticationDomain);
			}
			if (failedLoginCount.HasValue == true)
			{
				query = query.Where(su => su.failedLoginCount == failedLoginCount.Value);
			}
			if (lastLoginAttempt.HasValue == true)
			{
				query = query.Where(su => su.lastLoginAttempt == lastLoginAttempt.Value);
			}
			if (mostRecentActivity.HasValue == true)
			{
				query = query.Where(su => su.mostRecentActivity == mostRecentActivity.Value);
			}
			if (string.IsNullOrEmpty(alternateIdentifier) == false)
			{
				query = query.Where(su => su.alternateIdentifier == alternateIdentifier);
			}
			if (string.IsNullOrEmpty(settings) == false)
			{
				query = query.Where(su => su.settings == settings);
			}
			if (securityTenantId.HasValue == true)
			{
				query = query.Where(su => su.securityTenantId == securityTenantId.Value);
			}
			if (readPermissionLevel.HasValue == true)
			{
				query = query.Where(su => su.readPermissionLevel == readPermissionLevel.Value);
			}
			if (writePermissionLevel.HasValue == true)
			{
				query = query.Where(su => su.writePermissionLevel == writePermissionLevel.Value);
			}
			if (securityOrganizationId.HasValue == true)
			{
				query = query.Where(su => su.securityOrganizationId == securityOrganizationId.Value);
			}
			if (securityDepartmentId.HasValue == true)
			{
				query = query.Where(su => su.securityDepartmentId == securityDepartmentId.Value);
			}
			if (securityTeamId.HasValue == true)
			{
				query = query.Where(su => su.securityTeamId == securityTeamId.Value);
			}
			if (string.IsNullOrEmpty(authenticationToken) == false)
			{
				query = query.Where(su => su.authenticationToken == authenticationToken);
			}
			if (authenticationTokenExpiry.HasValue == true)
			{
				query = query.Where(su => su.authenticationTokenExpiry == authenticationTokenExpiry.Value);
			}
			if (string.IsNullOrEmpty(twoFactorToken) == false)
			{
				query = query.Where(su => su.twoFactorToken == twoFactorToken);
			}
			if (twoFactorTokenExpiry.HasValue == true)
			{
				query = query.Where(su => su.twoFactorTokenExpiry == twoFactorTokenExpiry.Value);
			}
			if (twoFactorSendByEmail.HasValue == true)
			{
				query = query.Where(su => su.twoFactorSendByEmail == twoFactorSendByEmail.Value);
			}
			if (twoFactorSendBySMS.HasValue == true)
			{
				query = query.Where(su => su.twoFactorSendBySMS == twoFactorSendBySMS.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(su => su.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(su => su.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(su => su.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(su => su.deleted == false);
				}
			}
			else
			{
				query = query.Where(su => su.active == true);
				query = query.Where(su => su.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Security User, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.accountName.Contains(anyStringContains)
			       || x.password.Contains(anyStringContains)
			       || x.firstName.Contains(anyStringContains)
			       || x.middleName.Contains(anyStringContains)
			       || x.lastName.Contains(anyStringContains)
			       || x.emailAddress.Contains(anyStringContains)
			       || x.cellPhoneNumber.Contains(anyStringContains)
			       || x.phoneNumber.Contains(anyStringContains)
			       || x.phoneExtension.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.authenticationDomain.Contains(anyStringContains)
			       || x.alternateIdentifier.Contains(anyStringContains)
			       || x.settings.Contains(anyStringContains)
			       || x.authenticationToken.Contains(anyStringContains)
			       || x.twoFactorToken.Contains(anyStringContains)
			       || x.reportsToSecurityUser.accountName.Contains(anyStringContains)
			       || x.reportsToSecurityUser.password.Contains(anyStringContains)
			       || x.reportsToSecurityUser.firstName.Contains(anyStringContains)
			       || x.reportsToSecurityUser.middleName.Contains(anyStringContains)
			       || x.reportsToSecurityUser.lastName.Contains(anyStringContains)
			       || x.reportsToSecurityUser.emailAddress.Contains(anyStringContains)
			       || x.reportsToSecurityUser.cellPhoneNumber.Contains(anyStringContains)
			       || x.reportsToSecurityUser.phoneNumber.Contains(anyStringContains)
			       || x.reportsToSecurityUser.phoneExtension.Contains(anyStringContains)
			       || x.reportsToSecurityUser.description.Contains(anyStringContains)
			       || x.reportsToSecurityUser.authenticationDomain.Contains(anyStringContains)
			       || x.reportsToSecurityUser.alternateIdentifier.Contains(anyStringContains)
			       || x.reportsToSecurityUser.settings.Contains(anyStringContains)
			       || x.reportsToSecurityUser.authenticationToken.Contains(anyStringContains)
			       || x.reportsToSecurityUser.twoFactorToken.Contains(anyStringContains)
			       || x.securityDepartment.name.Contains(anyStringContains)
			       || x.securityDepartment.description.Contains(anyStringContains)
			       || x.securityOrganization.name.Contains(anyStringContains)
			       || x.securityOrganization.description.Contains(anyStringContains)
			       || x.securityTeam.name.Contains(anyStringContains)
			       || x.securityTeam.description.Contains(anyStringContains)
			       || x.securityTenant.name.Contains(anyStringContains)
			       || x.securityTenant.description.Contains(anyStringContains)
			       || x.securityUserTitle.name.Contains(anyStringContains)
			       || x.securityUserTitle.description.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.accountName);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.SecurityUser.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
		}
*/


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
		[Route("api/SecurityUser/CreateAuditEvent")]
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
