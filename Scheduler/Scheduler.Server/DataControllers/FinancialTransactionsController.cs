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

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the FinancialTransaction entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the FinancialTransaction entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class FinancialTransactionsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object financialTransactionPutSyncRoot = new object();
		static object financialTransactionDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<FinancialTransactionsController> _logger;

		public FinancialTransactionsController(SchedulerContext context, ILogger<FinancialTransactionsController> logger) : base("Scheduler", "FinancialTransaction")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of FinancialTransactions filtered by the parameters provided.
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
		[Route("api/FinancialTransactions")]
		public async Task<IActionResult> GetFinancialTransactions(
			int? financialCategoryId = null,
			int? financialOfficeId = null,
			int? scheduledEventId = null,
			int? contactId = null,
			int? clientId = null,
			string contactRole = null,
			int? taxCodeId = null,
			int? fiscalPeriodId = null,
			int? paymentTypeId = null,
			DateTime? transactionDate = null,
			string description = null,
			decimal? amount = null,
			decimal? taxAmount = null,
			decimal? totalAmount = null,
			bool? isRevenue = null,
			string journalEntryType = null,
			string referenceNumber = null,
			string notes = null,
			int? currencyId = null,
			DateTime? exportedDate = null,
			string externalId = null,
			string externalSystemName = null,
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

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (transactionDate.HasValue == true && transactionDate.Value.Kind != DateTimeKind.Utc)
			{
				transactionDate = transactionDate.Value.ToUniversalTime();
			}

			if (exportedDate.HasValue == true && exportedDate.Value.Kind != DateTimeKind.Utc)
			{
				exportedDate = exportedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.FinancialTransaction> query = (from ft in _context.FinancialTransactions select ft);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (financialCategoryId.HasValue == true)
			{
				query = query.Where(ft => ft.financialCategoryId == financialCategoryId.Value);
			}
			if (financialOfficeId.HasValue == true)
			{
				query = query.Where(ft => ft.financialOfficeId == financialOfficeId.Value);
			}
			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(ft => ft.scheduledEventId == scheduledEventId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(ft => ft.contactId == contactId.Value);
			}
			if (clientId.HasValue == true)
			{
				query = query.Where(ft => ft.clientId == clientId.Value);
			}
			if (string.IsNullOrEmpty(contactRole) == false)
			{
				query = query.Where(ft => ft.contactRole == contactRole);
			}
			if (taxCodeId.HasValue == true)
			{
				query = query.Where(ft => ft.taxCodeId == taxCodeId.Value);
			}
			if (fiscalPeriodId.HasValue == true)
			{
				query = query.Where(ft => ft.fiscalPeriodId == fiscalPeriodId.Value);
			}
			if (paymentTypeId.HasValue == true)
			{
				query = query.Where(ft => ft.paymentTypeId == paymentTypeId.Value);
			}
			if (transactionDate.HasValue == true)
			{
				query = query.Where(ft => ft.transactionDate == transactionDate.Value);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ft => ft.description == description);
			}
			if (amount.HasValue == true)
			{
				query = query.Where(ft => ft.amount == amount.Value);
			}
			if (taxAmount.HasValue == true)
			{
				query = query.Where(ft => ft.taxAmount == taxAmount.Value);
			}
			if (totalAmount.HasValue == true)
			{
				query = query.Where(ft => ft.totalAmount == totalAmount.Value);
			}
			if (isRevenue.HasValue == true)
			{
				query = query.Where(ft => ft.isRevenue == isRevenue.Value);
			}
			if (string.IsNullOrEmpty(journalEntryType) == false)
			{
				query = query.Where(ft => ft.journalEntryType == journalEntryType);
			}
			if (string.IsNullOrEmpty(referenceNumber) == false)
			{
				query = query.Where(ft => ft.referenceNumber == referenceNumber);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(ft => ft.notes == notes);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(ft => ft.currencyId == currencyId.Value);
			}
			if (exportedDate.HasValue == true)
			{
				query = query.Where(ft => ft.exportedDate == exportedDate.Value);
			}
			if (string.IsNullOrEmpty(externalId) == false)
			{
				query = query.Where(ft => ft.externalId == externalId);
			}
			if (string.IsNullOrEmpty(externalSystemName) == false)
			{
				query = query.Where(ft => ft.externalSystemName == externalSystemName);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ft => ft.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ft => ft.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ft => ft.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ft => ft.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ft => ft.deleted == false);
				}
			}
			else
			{
				query = query.Where(ft => ft.active == true);
				query = query.Where(ft => ft.deleted == false);
			}

			query = query.OrderBy(ft => ft.contactRole).ThenBy(ft => ft.description).ThenBy(ft => ft.journalEntryType);


			//
			// Add the any string contains parameter to span all the string fields on the Financial Transaction, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.contactRole.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.journalEntryType.Contains(anyStringContains)
			       || x.referenceNumber.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.externalSystemName.Contains(anyStringContains)
			       || (includeRelations == true && x.client.name.Contains(anyStringContains))
			       || (includeRelations == true && x.client.description.Contains(anyStringContains))
			       || (includeRelations == true && x.client.addressLine1.Contains(anyStringContains))
			       || (includeRelations == true && x.client.addressLine2.Contains(anyStringContains))
			       || (includeRelations == true && x.client.city.Contains(anyStringContains))
			       || (includeRelations == true && x.client.postalCode.Contains(anyStringContains))
			       || (includeRelations == true && x.client.phone.Contains(anyStringContains))
			       || (includeRelations == true && x.client.email.Contains(anyStringContains))
			       || (includeRelations == true && x.client.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.client.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.client.color.Contains(anyStringContains))
			       || (includeRelations == true && x.client.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.client.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.client.avatarMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.firstName.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.middleName.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.lastName.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.title.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.company.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.email.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.phone.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.mobile.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.position.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.webSite.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.color.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.avatarMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.name.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.description.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.code.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.color.Contains(anyStringContains))
			       || (includeRelations == true && x.financialCategory.name.Contains(anyStringContains))
			       || (includeRelations == true && x.financialCategory.description.Contains(anyStringContains))
			       || (includeRelations == true && x.financialCategory.code.Contains(anyStringContains))
			       || (includeRelations == true && x.financialCategory.externalAccountId.Contains(anyStringContains))
			       || (includeRelations == true && x.financialCategory.color.Contains(anyStringContains))
			       || (includeRelations == true && x.financialOffice.name.Contains(anyStringContains))
			       || (includeRelations == true && x.financialOffice.description.Contains(anyStringContains))
			       || (includeRelations == true && x.financialOffice.code.Contains(anyStringContains))
			       || (includeRelations == true && x.financialOffice.contactName.Contains(anyStringContains))
			       || (includeRelations == true && x.financialOffice.contactEmail.Contains(anyStringContains))
			       || (includeRelations == true && x.financialOffice.exportFormat.Contains(anyStringContains))
			       || (includeRelations == true && x.financialOffice.color.Contains(anyStringContains))
			       || (includeRelations == true && x.fiscalPeriod.name.Contains(anyStringContains))
			       || (includeRelations == true && x.fiscalPeriod.description.Contains(anyStringContains))
			       || (includeRelations == true && x.fiscalPeriod.periodType.Contains(anyStringContains))
			       || (includeRelations == true && x.fiscalPeriod.closedBy.Contains(anyStringContains))
			       || (includeRelations == true && x.paymentType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.paymentType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.paymentType.color.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.name.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.description.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.location.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.bookingContactName.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.bookingContactEmail.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.bookingContactPhone.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.color.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.taxCode.name.Contains(anyStringContains))
			       || (includeRelations == true && x.taxCode.description.Contains(anyStringContains))
			       || (includeRelations == true && x.taxCode.code.Contains(anyStringContains))
			       || (includeRelations == true && x.taxCode.externalTaxCodeId.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.client);
				query = query.Include(x => x.contact);
				query = query.Include(x => x.currency);
				query = query.Include(x => x.financialCategory);
				query = query.Include(x => x.financialOffice);
				query = query.Include(x => x.fiscalPeriod);
				query = query.Include(x => x.paymentType);
				query = query.Include(x => x.scheduledEvent);
				query = query.Include(x => x.taxCode);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.FinancialTransaction> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.FinancialTransaction financialTransaction in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(financialTransaction, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.FinancialTransaction Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.FinancialTransaction Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of FinancialTransactions filtered by the parameters provided.  Its query is similar to the GetFinancialTransactions method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FinancialTransactions/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? financialCategoryId = null,
			int? financialOfficeId = null,
			int? scheduledEventId = null,
			int? contactId = null,
			int? clientId = null,
			string contactRole = null,
			int? taxCodeId = null,
			int? fiscalPeriodId = null,
			int? paymentTypeId = null,
			DateTime? transactionDate = null,
			string description = null,
			decimal? amount = null,
			decimal? taxAmount = null,
			decimal? totalAmount = null,
			bool? isRevenue = null,
			string journalEntryType = null,
			string referenceNumber = null,
			string notes = null,
			int? currencyId = null,
			DateTime? exportedDate = null,
			string externalId = null,
			string externalSystemName = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
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
			// Fix any non-UTC date parameters that come in.
			//
			if (transactionDate.HasValue == true && transactionDate.Value.Kind != DateTimeKind.Utc)
			{
				transactionDate = transactionDate.Value.ToUniversalTime();
			}

			if (exportedDate.HasValue == true && exportedDate.Value.Kind != DateTimeKind.Utc)
			{
				exportedDate = exportedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.FinancialTransaction> query = (from ft in _context.FinancialTransactions select ft);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (financialCategoryId.HasValue == true)
			{
				query = query.Where(ft => ft.financialCategoryId == financialCategoryId.Value);
			}
			if (financialOfficeId.HasValue == true)
			{
				query = query.Where(ft => ft.financialOfficeId == financialOfficeId.Value);
			}
			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(ft => ft.scheduledEventId == scheduledEventId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(ft => ft.contactId == contactId.Value);
			}
			if (clientId.HasValue == true)
			{
				query = query.Where(ft => ft.clientId == clientId.Value);
			}
			if (contactRole != null)
			{
				query = query.Where(ft => ft.contactRole == contactRole);
			}
			if (taxCodeId.HasValue == true)
			{
				query = query.Where(ft => ft.taxCodeId == taxCodeId.Value);
			}
			if (fiscalPeriodId.HasValue == true)
			{
				query = query.Where(ft => ft.fiscalPeriodId == fiscalPeriodId.Value);
			}
			if (paymentTypeId.HasValue == true)
			{
				query = query.Where(ft => ft.paymentTypeId == paymentTypeId.Value);
			}
			if (transactionDate.HasValue == true)
			{
				query = query.Where(ft => ft.transactionDate == transactionDate.Value);
			}
			if (description != null)
			{
				query = query.Where(ft => ft.description == description);
			}
			if (amount.HasValue == true)
			{
				query = query.Where(ft => ft.amount == amount.Value);
			}
			if (taxAmount.HasValue == true)
			{
				query = query.Where(ft => ft.taxAmount == taxAmount.Value);
			}
			if (totalAmount.HasValue == true)
			{
				query = query.Where(ft => ft.totalAmount == totalAmount.Value);
			}
			if (isRevenue.HasValue == true)
			{
				query = query.Where(ft => ft.isRevenue == isRevenue.Value);
			}
			if (journalEntryType != null)
			{
				query = query.Where(ft => ft.journalEntryType == journalEntryType);
			}
			if (referenceNumber != null)
			{
				query = query.Where(ft => ft.referenceNumber == referenceNumber);
			}
			if (notes != null)
			{
				query = query.Where(ft => ft.notes == notes);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(ft => ft.currencyId == currencyId.Value);
			}
			if (exportedDate.HasValue == true)
			{
				query = query.Where(ft => ft.exportedDate == exportedDate.Value);
			}
			if (externalId != null)
			{
				query = query.Where(ft => ft.externalId == externalId);
			}
			if (externalSystemName != null)
			{
				query = query.Where(ft => ft.externalSystemName == externalSystemName);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ft => ft.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ft => ft.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ft => ft.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ft => ft.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ft => ft.deleted == false);
				}
			}
			else
			{
				query = query.Where(ft => ft.active == true);
				query = query.Where(ft => ft.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Financial Transaction, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.contactRole.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.journalEntryType.Contains(anyStringContains)
			       || x.referenceNumber.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.externalSystemName.Contains(anyStringContains)
			       || x.client.name.Contains(anyStringContains)
			       || x.client.description.Contains(anyStringContains)
			       || x.client.addressLine1.Contains(anyStringContains)
			       || x.client.addressLine2.Contains(anyStringContains)
			       || x.client.city.Contains(anyStringContains)
			       || x.client.postalCode.Contains(anyStringContains)
			       || x.client.phone.Contains(anyStringContains)
			       || x.client.email.Contains(anyStringContains)
			       || x.client.notes.Contains(anyStringContains)
			       || x.client.externalId.Contains(anyStringContains)
			       || x.client.color.Contains(anyStringContains)
			       || x.client.attributes.Contains(anyStringContains)
			       || x.client.avatarFileName.Contains(anyStringContains)
			       || x.client.avatarMimeType.Contains(anyStringContains)
			       || x.contact.firstName.Contains(anyStringContains)
			       || x.contact.middleName.Contains(anyStringContains)
			       || x.contact.lastName.Contains(anyStringContains)
			       || x.contact.title.Contains(anyStringContains)
			       || x.contact.company.Contains(anyStringContains)
			       || x.contact.email.Contains(anyStringContains)
			       || x.contact.phone.Contains(anyStringContains)
			       || x.contact.mobile.Contains(anyStringContains)
			       || x.contact.position.Contains(anyStringContains)
			       || x.contact.webSite.Contains(anyStringContains)
			       || x.contact.notes.Contains(anyStringContains)
			       || x.contact.attributes.Contains(anyStringContains)
			       || x.contact.color.Contains(anyStringContains)
			       || x.contact.avatarFileName.Contains(anyStringContains)
			       || x.contact.avatarMimeType.Contains(anyStringContains)
			       || x.contact.externalId.Contains(anyStringContains)
			       || x.currency.name.Contains(anyStringContains)
			       || x.currency.description.Contains(anyStringContains)
			       || x.currency.code.Contains(anyStringContains)
			       || x.currency.color.Contains(anyStringContains)
			       || x.financialCategory.name.Contains(anyStringContains)
			       || x.financialCategory.description.Contains(anyStringContains)
			       || x.financialCategory.code.Contains(anyStringContains)
			       || x.financialCategory.externalAccountId.Contains(anyStringContains)
			       || x.financialCategory.color.Contains(anyStringContains)
			       || x.financialOffice.name.Contains(anyStringContains)
			       || x.financialOffice.description.Contains(anyStringContains)
			       || x.financialOffice.code.Contains(anyStringContains)
			       || x.financialOffice.contactName.Contains(anyStringContains)
			       || x.financialOffice.contactEmail.Contains(anyStringContains)
			       || x.financialOffice.exportFormat.Contains(anyStringContains)
			       || x.financialOffice.color.Contains(anyStringContains)
			       || x.fiscalPeriod.name.Contains(anyStringContains)
			       || x.fiscalPeriod.description.Contains(anyStringContains)
			       || x.fiscalPeriod.periodType.Contains(anyStringContains)
			       || x.fiscalPeriod.closedBy.Contains(anyStringContains)
			       || x.paymentType.name.Contains(anyStringContains)
			       || x.paymentType.description.Contains(anyStringContains)
			       || x.paymentType.color.Contains(anyStringContains)
			       || x.scheduledEvent.name.Contains(anyStringContains)
			       || x.scheduledEvent.description.Contains(anyStringContains)
			       || x.scheduledEvent.location.Contains(anyStringContains)
			       || x.scheduledEvent.bookingContactName.Contains(anyStringContains)
			       || x.scheduledEvent.bookingContactEmail.Contains(anyStringContains)
			       || x.scheduledEvent.bookingContactPhone.Contains(anyStringContains)
			       || x.scheduledEvent.notes.Contains(anyStringContains)
			       || x.scheduledEvent.color.Contains(anyStringContains)
			       || x.scheduledEvent.externalId.Contains(anyStringContains)
			       || x.scheduledEvent.attributes.Contains(anyStringContains)
			       || x.taxCode.name.Contains(anyStringContains)
			       || x.taxCode.description.Contains(anyStringContains)
			       || x.taxCode.code.Contains(anyStringContains)
			       || x.taxCode.externalTaxCodeId.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single FinancialTransaction by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FinancialTransaction/{id}")]
		public async Task<IActionResult> GetFinancialTransaction(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			
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
				IQueryable<Database.FinancialTransaction> query = (from ft in _context.FinancialTransactions where
							(ft.id == id) &&
							(userIsAdmin == true || ft.deleted == false) &&
							(userIsWriter == true || ft.active == true)
					select ft);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.client);
					query = query.Include(x => x.contact);
					query = query.Include(x => x.currency);
					query = query.Include(x => x.financialCategory);
					query = query.Include(x => x.financialOffice);
					query = query.Include(x => x.fiscalPeriod);
					query = query.Include(x => x.paymentType);
					query = query.Include(x => x.scheduledEvent);
					query = query.Include(x => x.taxCode);
					query = query.AsSplitQuery();
				}

				Database.FinancialTransaction materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.FinancialTransaction Entity was read with Admin privilege." : "Scheduler.FinancialTransaction Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "FinancialTransaction", materialized.id, materialized.contactRole));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.FinancialTransaction entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.FinancialTransaction.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.FinancialTransaction.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing FinancialTransaction record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/FinancialTransaction/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutFinancialTransaction(int id, [FromBody]Database.FinancialTransaction.FinancialTransactionDTO financialTransactionDTO, CancellationToken cancellationToken = default)
		{
			if (financialTransactionDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != financialTransactionDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
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


			IQueryable<Database.FinancialTransaction> query = (from x in _context.FinancialTransactions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.FinancialTransaction existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.FinancialTransaction PUT", id.ToString(), new Exception("No Scheduler.FinancialTransaction entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (financialTransactionDTO.objectGuid == Guid.Empty)
            {
                financialTransactionDTO.objectGuid = existing.objectGuid;
            }
            else if (financialTransactionDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a FinancialTransaction record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.FinancialTransaction cloneOfExisting = (Database.FinancialTransaction)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new FinancialTransaction object using the data from the existing record, updated with what is in the DTO.
			//
			Database.FinancialTransaction financialTransaction = (Database.FinancialTransaction)_context.Entry(existing).GetDatabaseValues().ToObject();
			financialTransaction.ApplyDTO(financialTransactionDTO);
			//
			// The tenant guid for any FinancialTransaction being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the FinancialTransaction because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				financialTransaction.tenantGuid = existing.tenantGuid;
			}

			lock (financialTransactionPutSyncRoot)
			{
				//
				// Validate the version number for the financialTransaction being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != financialTransaction.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "FinancialTransaction save attempt was made but save request was with version " + financialTransaction.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The FinancialTransaction you are trying to update has already changed.  Please try your save again after reloading the FinancialTransaction.");
				}
				else
				{
					// Same record.  Increase version.
					financialTransaction.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (financialTransaction.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.FinancialTransaction record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (financialTransaction.contactRole != null && financialTransaction.contactRole.Length > 50)
				{
					financialTransaction.contactRole = financialTransaction.contactRole.Substring(0, 50);
				}

				if (financialTransaction.transactionDate.Kind != DateTimeKind.Utc)
				{
					financialTransaction.transactionDate = financialTransaction.transactionDate.ToUniversalTime();
				}

				if (financialTransaction.description != null && financialTransaction.description.Length > 500)
				{
					financialTransaction.description = financialTransaction.description.Substring(0, 500);
				}

				if (financialTransaction.journalEntryType != null && financialTransaction.journalEntryType.Length > 50)
				{
					financialTransaction.journalEntryType = financialTransaction.journalEntryType.Substring(0, 50);
				}

				if (financialTransaction.referenceNumber != null && financialTransaction.referenceNumber.Length > 100)
				{
					financialTransaction.referenceNumber = financialTransaction.referenceNumber.Substring(0, 100);
				}

				if (financialTransaction.exportedDate.HasValue == true && financialTransaction.exportedDate.Value.Kind != DateTimeKind.Utc)
				{
					financialTransaction.exportedDate = financialTransaction.exportedDate.Value.ToUniversalTime();
				}

				if (financialTransaction.externalId != null && financialTransaction.externalId.Length > 100)
				{
					financialTransaction.externalId = financialTransaction.externalId.Substring(0, 100);
				}

				if (financialTransaction.externalSystemName != null && financialTransaction.externalSystemName.Length > 50)
				{
					financialTransaction.externalSystemName = financialTransaction.externalSystemName.Substring(0, 50);
				}

				try
				{
				    EntityEntry<Database.FinancialTransaction> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(financialTransaction);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        FinancialTransactionChangeHistory financialTransactionChangeHistory = new FinancialTransactionChangeHistory();
				        financialTransactionChangeHistory.financialTransactionId = financialTransaction.id;
				        financialTransactionChangeHistory.versionNumber = financialTransaction.versionNumber;
				        financialTransactionChangeHistory.timeStamp = DateTime.UtcNow;
				        financialTransactionChangeHistory.userId = securityUser.id;
				        financialTransactionChangeHistory.tenantGuid = userTenantGuid;
				        financialTransactionChangeHistory.data = JsonSerializer.Serialize(Database.FinancialTransaction.CreateAnonymousWithFirstLevelSubObjects(financialTransaction));
				        _context.FinancialTransactionChangeHistories.Add(financialTransactionChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.FinancialTransaction entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.FinancialTransaction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.FinancialTransaction.CreateAnonymousWithFirstLevelSubObjects(financialTransaction)),
						null);

				return Ok(Database.FinancialTransaction.CreateAnonymous(financialTransaction));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.FinancialTransaction entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.FinancialTransaction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.FinancialTransaction.CreateAnonymousWithFirstLevelSubObjects(financialTransaction)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new FinancialTransaction record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FinancialTransaction", Name = "FinancialTransaction")]
		public async Task<IActionResult> PostFinancialTransaction([FromBody]Database.FinancialTransaction.FinancialTransactionDTO financialTransactionDTO, CancellationToken cancellationToken = default)
		{
			if (financialTransactionDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
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
			// Create a new FinancialTransaction object using the data from the DTO
			//
			Database.FinancialTransaction financialTransaction = Database.FinancialTransaction.FromDTO(financialTransactionDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				financialTransaction.tenantGuid = userTenantGuid;

				if (financialTransaction.contactRole != null && financialTransaction.contactRole.Length > 50)
				{
					financialTransaction.contactRole = financialTransaction.contactRole.Substring(0, 50);
				}

				if (financialTransaction.transactionDate.Kind != DateTimeKind.Utc)
				{
					financialTransaction.transactionDate = financialTransaction.transactionDate.ToUniversalTime();
				}

				if (financialTransaction.description != null && financialTransaction.description.Length > 500)
				{
					financialTransaction.description = financialTransaction.description.Substring(0, 500);
				}

				if (financialTransaction.journalEntryType != null && financialTransaction.journalEntryType.Length > 50)
				{
					financialTransaction.journalEntryType = financialTransaction.journalEntryType.Substring(0, 50);
				}

				if (financialTransaction.referenceNumber != null && financialTransaction.referenceNumber.Length > 100)
				{
					financialTransaction.referenceNumber = financialTransaction.referenceNumber.Substring(0, 100);
				}

				if (financialTransaction.exportedDate.HasValue == true && financialTransaction.exportedDate.Value.Kind != DateTimeKind.Utc)
				{
					financialTransaction.exportedDate = financialTransaction.exportedDate.Value.ToUniversalTime();
				}

				if (financialTransaction.externalId != null && financialTransaction.externalId.Length > 100)
				{
					financialTransaction.externalId = financialTransaction.externalId.Substring(0, 100);
				}

				if (financialTransaction.externalSystemName != null && financialTransaction.externalSystemName.Length > 50)
				{
					financialTransaction.externalSystemName = financialTransaction.externalSystemName.Substring(0, 50);
				}

				financialTransaction.objectGuid = Guid.NewGuid();
				financialTransaction.versionNumber = 1;

				_context.FinancialTransactions.Add(financialTransaction);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the financialTransaction object so that no further changes will be written to the database
				    //
				    _context.Entry(financialTransaction).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					financialTransaction.Documents = null;
					financialTransaction.FinancialTransactionChangeHistories = null;
					financialTransaction.GeneralLedgerEntries = null;
					financialTransaction.PaymentTransactions = null;
					financialTransaction.Receipts = null;
					financialTransaction.client = null;
					financialTransaction.contact = null;
					financialTransaction.currency = null;
					financialTransaction.financialCategory = null;
					financialTransaction.financialOffice = null;
					financialTransaction.fiscalPeriod = null;
					financialTransaction.paymentType = null;
					financialTransaction.scheduledEvent = null;
					financialTransaction.taxCode = null;


				    FinancialTransactionChangeHistory financialTransactionChangeHistory = new FinancialTransactionChangeHistory();
				    financialTransactionChangeHistory.financialTransactionId = financialTransaction.id;
				    financialTransactionChangeHistory.versionNumber = financialTransaction.versionNumber;
				    financialTransactionChangeHistory.timeStamp = DateTime.UtcNow;
				    financialTransactionChangeHistory.userId = securityUser.id;
				    financialTransactionChangeHistory.tenantGuid = userTenantGuid;
				    financialTransactionChangeHistory.data = JsonSerializer.Serialize(Database.FinancialTransaction.CreateAnonymousWithFirstLevelSubObjects(financialTransaction));
				    _context.FinancialTransactionChangeHistories.Add(financialTransactionChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.FinancialTransaction entity successfully created.",
						true,
						financialTransaction. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.FinancialTransaction.CreateAnonymousWithFirstLevelSubObjects(financialTransaction)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.FinancialTransaction entity creation failed.", false, financialTransaction.id.ToString(), "", JsonSerializer.Serialize(Database.FinancialTransaction.CreateAnonymousWithFirstLevelSubObjects(financialTransaction)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "FinancialTransaction", financialTransaction.id, financialTransaction.contactRole));

			return CreatedAtRoute("FinancialTransaction", new { id = financialTransaction.id }, Database.FinancialTransaction.CreateAnonymousWithFirstLevelSubObjects(financialTransaction));
		}



        /// <summary>
        /// 
        /// This rolls a FinancialTransaction entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FinancialTransaction/Rollback/{id}")]
		[Route("api/FinancialTransaction/Rollback")]
		public async Task<IActionResult> RollbackToFinancialTransactionVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.FinancialTransaction> query = (from x in _context.FinancialTransactions
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this FinancialTransaction concurrently
			//
			lock (financialTransactionPutSyncRoot)
			{
				
				Database.FinancialTransaction financialTransaction = query.FirstOrDefault();
				
				if (financialTransaction == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.FinancialTransaction rollback", id.ToString(), new Exception("No Scheduler.FinancialTransaction entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the FinancialTransaction current state so we can log it.
				//
				Database.FinancialTransaction cloneOfExisting = (Database.FinancialTransaction)_context.Entry(financialTransaction).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.Documents = null;
				cloneOfExisting.FinancialTransactionChangeHistories = null;
				cloneOfExisting.GeneralLedgerEntries = null;
				cloneOfExisting.PaymentTransactions = null;
				cloneOfExisting.Receipts = null;
				cloneOfExisting.client = null;
				cloneOfExisting.contact = null;
				cloneOfExisting.currency = null;
				cloneOfExisting.financialCategory = null;
				cloneOfExisting.financialOffice = null;
				cloneOfExisting.fiscalPeriod = null;
				cloneOfExisting.paymentType = null;
				cloneOfExisting.scheduledEvent = null;
				cloneOfExisting.taxCode = null;

				if (versionNumber >= financialTransaction.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.FinancialTransaction rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.FinancialTransaction rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				FinancialTransactionChangeHistory financialTransactionChangeHistory = (from x in _context.FinancialTransactionChangeHistories
				                                               where
				                                               x.financialTransactionId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (financialTransactionChangeHistory != null)
				{
				    Database.FinancialTransaction oldFinancialTransaction = JsonSerializer.Deserialize<Database.FinancialTransaction>(financialTransactionChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    financialTransaction.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    financialTransaction.financialCategoryId = oldFinancialTransaction.financialCategoryId;
				    financialTransaction.financialOfficeId = oldFinancialTransaction.financialOfficeId;
				    financialTransaction.scheduledEventId = oldFinancialTransaction.scheduledEventId;
				    financialTransaction.contactId = oldFinancialTransaction.contactId;
				    financialTransaction.clientId = oldFinancialTransaction.clientId;
				    financialTransaction.contactRole = oldFinancialTransaction.contactRole;
				    financialTransaction.taxCodeId = oldFinancialTransaction.taxCodeId;
				    financialTransaction.fiscalPeriodId = oldFinancialTransaction.fiscalPeriodId;
				    financialTransaction.paymentTypeId = oldFinancialTransaction.paymentTypeId;
				    financialTransaction.transactionDate = oldFinancialTransaction.transactionDate;
				    financialTransaction.description = oldFinancialTransaction.description;
				    financialTransaction.amount = oldFinancialTransaction.amount;
				    financialTransaction.taxAmount = oldFinancialTransaction.taxAmount;
				    financialTransaction.totalAmount = oldFinancialTransaction.totalAmount;
				    financialTransaction.isRevenue = oldFinancialTransaction.isRevenue;
				    financialTransaction.journalEntryType = oldFinancialTransaction.journalEntryType;
				    financialTransaction.referenceNumber = oldFinancialTransaction.referenceNumber;
				    financialTransaction.notes = oldFinancialTransaction.notes;
				    financialTransaction.currencyId = oldFinancialTransaction.currencyId;
				    financialTransaction.exportedDate = oldFinancialTransaction.exportedDate;
				    financialTransaction.externalId = oldFinancialTransaction.externalId;
				    financialTransaction.externalSystemName = oldFinancialTransaction.externalSystemName;
				    financialTransaction.objectGuid = oldFinancialTransaction.objectGuid;
				    financialTransaction.active = oldFinancialTransaction.active;
				    financialTransaction.deleted = oldFinancialTransaction.deleted;

				    string serializedFinancialTransaction = JsonSerializer.Serialize(financialTransaction);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        FinancialTransactionChangeHistory newFinancialTransactionChangeHistory = new FinancialTransactionChangeHistory();
				        newFinancialTransactionChangeHistory.financialTransactionId = financialTransaction.id;
				        newFinancialTransactionChangeHistory.versionNumber = financialTransaction.versionNumber;
				        newFinancialTransactionChangeHistory.timeStamp = DateTime.UtcNow;
				        newFinancialTransactionChangeHistory.userId = securityUser.id;
				        newFinancialTransactionChangeHistory.tenantGuid = userTenantGuid;
				        newFinancialTransactionChangeHistory.data = JsonSerializer.Serialize(Database.FinancialTransaction.CreateAnonymousWithFirstLevelSubObjects(financialTransaction));
				        _context.FinancialTransactionChangeHistories.Add(newFinancialTransactionChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.FinancialTransaction rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.FinancialTransaction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.FinancialTransaction.CreateAnonymousWithFirstLevelSubObjects(financialTransaction)),
						null);


				    return Ok(Database.FinancialTransaction.CreateAnonymous(financialTransaction));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.FinancialTransaction rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.FinancialTransaction rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a FinancialTransaction.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the FinancialTransaction</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FinancialTransaction/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetFinancialTransactionChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
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


			Database.FinancialTransaction financialTransaction = await _context.FinancialTransactions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (financialTransaction == null)
			{
				return NotFound();
			}

			try
			{
				financialTransaction.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.FinancialTransaction> versionInfo = await financialTransaction.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a FinancialTransaction.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the FinancialTransaction</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FinancialTransaction/{id}/AuditHistory")]
		public async Task<IActionResult> GetFinancialTransactionAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
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


			Database.FinancialTransaction financialTransaction = await _context.FinancialTransactions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (financialTransaction == null)
			{
				return NotFound();
			}

			try
			{
				financialTransaction.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.FinancialTransaction>> versions = await financialTransaction.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a FinancialTransaction.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the FinancialTransaction</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The FinancialTransaction object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FinancialTransaction/{id}/Version/{version}")]
		public async Task<IActionResult> GetFinancialTransactionVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
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


			Database.FinancialTransaction financialTransaction = await _context.FinancialTransactions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (financialTransaction == null)
			{
				return NotFound();
			}

			try
			{
				financialTransaction.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.FinancialTransaction> versionInfo = await financialTransaction.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a FinancialTransaction at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the FinancialTransaction</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The FinancialTransaction object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FinancialTransaction/{id}/StateAtTime")]
		public async Task<IActionResult> GetFinancialTransactionStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
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


			Database.FinancialTransaction financialTransaction = await _context.FinancialTransactions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (financialTransaction == null)
			{
				return NotFound();
			}

			try
			{
				financialTransaction.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.FinancialTransaction> versionInfo = await financialTransaction.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a FinancialTransaction record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FinancialTransaction/{id}")]
		[Route("api/FinancialTransaction")]
		public async Task<IActionResult> DeleteFinancialTransaction(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
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

			IQueryable<Database.FinancialTransaction> query = (from x in _context.FinancialTransactions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.FinancialTransaction financialTransaction = await query.FirstOrDefaultAsync(cancellationToken);

			if (financialTransaction == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.FinancialTransaction DELETE", id.ToString(), new Exception("No Scheduler.FinancialTransaction entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.FinancialTransaction cloneOfExisting = (Database.FinancialTransaction)_context.Entry(financialTransaction).GetDatabaseValues().ToObject();


			lock (financialTransactionDeleteSyncRoot)
			{
			    try
			    {
			        financialTransaction.deleted = true;
			        financialTransaction.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        FinancialTransactionChangeHistory financialTransactionChangeHistory = new FinancialTransactionChangeHistory();
			        financialTransactionChangeHistory.financialTransactionId = financialTransaction.id;
			        financialTransactionChangeHistory.versionNumber = financialTransaction.versionNumber;
			        financialTransactionChangeHistory.timeStamp = DateTime.UtcNow;
			        financialTransactionChangeHistory.userId = securityUser.id;
			        financialTransactionChangeHistory.tenantGuid = userTenantGuid;
			        financialTransactionChangeHistory.data = JsonSerializer.Serialize(Database.FinancialTransaction.CreateAnonymousWithFirstLevelSubObjects(financialTransaction));
			        _context.FinancialTransactionChangeHistories.Add(financialTransactionChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.FinancialTransaction entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.FinancialTransaction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.FinancialTransaction.CreateAnonymousWithFirstLevelSubObjects(financialTransaction)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.FinancialTransaction entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.FinancialTransaction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.FinancialTransaction.CreateAnonymousWithFirstLevelSubObjects(financialTransaction)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of FinancialTransaction records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/FinancialTransactions/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? financialCategoryId = null,
			int? financialOfficeId = null,
			int? scheduledEventId = null,
			int? contactId = null,
			int? clientId = null,
			string contactRole = null,
			int? taxCodeId = null,
			int? fiscalPeriodId = null,
			int? paymentTypeId = null,
			DateTime? transactionDate = null,
			string description = null,
			decimal? amount = null,
			decimal? taxAmount = null,
			decimal? totalAmount = null,
			bool? isRevenue = null,
			string journalEntryType = null,
			string referenceNumber = null,
			string notes = null,
			int? currencyId = null,
			DateTime? exportedDate = null,
			string externalId = null,
			string externalSystemName = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);


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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (transactionDate.HasValue == true && transactionDate.Value.Kind != DateTimeKind.Utc)
			{
				transactionDate = transactionDate.Value.ToUniversalTime();
			}

			if (exportedDate.HasValue == true && exportedDate.Value.Kind != DateTimeKind.Utc)
			{
				exportedDate = exportedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.FinancialTransaction> query = (from ft in _context.FinancialTransactions select ft);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (financialCategoryId.HasValue == true)
			{
				query = query.Where(ft => ft.financialCategoryId == financialCategoryId.Value);
			}
			if (financialOfficeId.HasValue == true)
			{
				query = query.Where(ft => ft.financialOfficeId == financialOfficeId.Value);
			}
			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(ft => ft.scheduledEventId == scheduledEventId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(ft => ft.contactId == contactId.Value);
			}
			if (clientId.HasValue == true)
			{
				query = query.Where(ft => ft.clientId == clientId.Value);
			}
			if (string.IsNullOrEmpty(contactRole) == false)
			{
				query = query.Where(ft => ft.contactRole == contactRole);
			}
			if (taxCodeId.HasValue == true)
			{
				query = query.Where(ft => ft.taxCodeId == taxCodeId.Value);
			}
			if (fiscalPeriodId.HasValue == true)
			{
				query = query.Where(ft => ft.fiscalPeriodId == fiscalPeriodId.Value);
			}
			if (paymentTypeId.HasValue == true)
			{
				query = query.Where(ft => ft.paymentTypeId == paymentTypeId.Value);
			}
			if (transactionDate.HasValue == true)
			{
				query = query.Where(ft => ft.transactionDate == transactionDate.Value);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ft => ft.description == description);
			}
			if (amount.HasValue == true)
			{
				query = query.Where(ft => ft.amount == amount.Value);
			}
			if (taxAmount.HasValue == true)
			{
				query = query.Where(ft => ft.taxAmount == taxAmount.Value);
			}
			if (totalAmount.HasValue == true)
			{
				query = query.Where(ft => ft.totalAmount == totalAmount.Value);
			}
			if (isRevenue.HasValue == true)
			{
				query = query.Where(ft => ft.isRevenue == isRevenue.Value);
			}
			if (string.IsNullOrEmpty(journalEntryType) == false)
			{
				query = query.Where(ft => ft.journalEntryType == journalEntryType);
			}
			if (string.IsNullOrEmpty(referenceNumber) == false)
			{
				query = query.Where(ft => ft.referenceNumber == referenceNumber);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(ft => ft.notes == notes);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(ft => ft.currencyId == currencyId.Value);
			}
			if (exportedDate.HasValue == true)
			{
				query = query.Where(ft => ft.exportedDate == exportedDate.Value);
			}
			if (string.IsNullOrEmpty(externalId) == false)
			{
				query = query.Where(ft => ft.externalId == externalId);
			}
			if (string.IsNullOrEmpty(externalSystemName) == false)
			{
				query = query.Where(ft => ft.externalSystemName == externalSystemName);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ft => ft.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ft => ft.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ft => ft.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ft => ft.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ft => ft.deleted == false);
				}
			}
			else
			{
				query = query.Where(ft => ft.active == true);
				query = query.Where(ft => ft.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Financial Transaction, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.contactRole.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.journalEntryType.Contains(anyStringContains)
			       || x.referenceNumber.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.externalSystemName.Contains(anyStringContains)
			       || x.client.name.Contains(anyStringContains)
			       || x.client.description.Contains(anyStringContains)
			       || x.client.addressLine1.Contains(anyStringContains)
			       || x.client.addressLine2.Contains(anyStringContains)
			       || x.client.city.Contains(anyStringContains)
			       || x.client.postalCode.Contains(anyStringContains)
			       || x.client.phone.Contains(anyStringContains)
			       || x.client.email.Contains(anyStringContains)
			       || x.client.notes.Contains(anyStringContains)
			       || x.client.externalId.Contains(anyStringContains)
			       || x.client.color.Contains(anyStringContains)
			       || x.client.attributes.Contains(anyStringContains)
			       || x.client.avatarFileName.Contains(anyStringContains)
			       || x.client.avatarMimeType.Contains(anyStringContains)
			       || x.contact.firstName.Contains(anyStringContains)
			       || x.contact.middleName.Contains(anyStringContains)
			       || x.contact.lastName.Contains(anyStringContains)
			       || x.contact.title.Contains(anyStringContains)
			       || x.contact.company.Contains(anyStringContains)
			       || x.contact.email.Contains(anyStringContains)
			       || x.contact.phone.Contains(anyStringContains)
			       || x.contact.mobile.Contains(anyStringContains)
			       || x.contact.position.Contains(anyStringContains)
			       || x.contact.webSite.Contains(anyStringContains)
			       || x.contact.notes.Contains(anyStringContains)
			       || x.contact.attributes.Contains(anyStringContains)
			       || x.contact.color.Contains(anyStringContains)
			       || x.contact.avatarFileName.Contains(anyStringContains)
			       || x.contact.avatarMimeType.Contains(anyStringContains)
			       || x.contact.externalId.Contains(anyStringContains)
			       || x.currency.name.Contains(anyStringContains)
			       || x.currency.description.Contains(anyStringContains)
			       || x.currency.code.Contains(anyStringContains)
			       || x.currency.color.Contains(anyStringContains)
			       || x.financialCategory.name.Contains(anyStringContains)
			       || x.financialCategory.description.Contains(anyStringContains)
			       || x.financialCategory.code.Contains(anyStringContains)
			       || x.financialCategory.externalAccountId.Contains(anyStringContains)
			       || x.financialCategory.color.Contains(anyStringContains)
			       || x.financialOffice.name.Contains(anyStringContains)
			       || x.financialOffice.description.Contains(anyStringContains)
			       || x.financialOffice.code.Contains(anyStringContains)
			       || x.financialOffice.contactName.Contains(anyStringContains)
			       || x.financialOffice.contactEmail.Contains(anyStringContains)
			       || x.financialOffice.exportFormat.Contains(anyStringContains)
			       || x.financialOffice.color.Contains(anyStringContains)
			       || x.fiscalPeriod.name.Contains(anyStringContains)
			       || x.fiscalPeriod.description.Contains(anyStringContains)
			       || x.fiscalPeriod.periodType.Contains(anyStringContains)
			       || x.fiscalPeriod.closedBy.Contains(anyStringContains)
			       || x.paymentType.name.Contains(anyStringContains)
			       || x.paymentType.description.Contains(anyStringContains)
			       || x.paymentType.color.Contains(anyStringContains)
			       || x.scheduledEvent.name.Contains(anyStringContains)
			       || x.scheduledEvent.description.Contains(anyStringContains)
			       || x.scheduledEvent.location.Contains(anyStringContains)
			       || x.scheduledEvent.bookingContactName.Contains(anyStringContains)
			       || x.scheduledEvent.bookingContactEmail.Contains(anyStringContains)
			       || x.scheduledEvent.bookingContactPhone.Contains(anyStringContains)
			       || x.scheduledEvent.notes.Contains(anyStringContains)
			       || x.scheduledEvent.color.Contains(anyStringContains)
			       || x.scheduledEvent.externalId.Contains(anyStringContains)
			       || x.scheduledEvent.attributes.Contains(anyStringContains)
			       || x.taxCode.name.Contains(anyStringContains)
			       || x.taxCode.description.Contains(anyStringContains)
			       || x.taxCode.code.Contains(anyStringContains)
			       || x.taxCode.externalTaxCodeId.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.contactRole).ThenBy(x => x.description).ThenBy(x => x.journalEntryType);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.FinancialTransaction.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/FinancialTransaction/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
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
