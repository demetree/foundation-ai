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
    /// This auto generated class provides the basic CRUD operations for the PaymentTransaction entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the PaymentTransaction entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class PaymentTransactionsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object paymentTransactionPutSyncRoot = new object();
		static object paymentTransactionDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<PaymentTransactionsController> _logger;

		public PaymentTransactionsController(SchedulerContext context, ILogger<PaymentTransactionsController> logger) : base("Scheduler", "PaymentTransaction")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of PaymentTransactions filtered by the parameters provided.
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
		[Route("api/PaymentTransactions")]
		public async Task<IActionResult> GetPaymentTransactions(
			int? paymentMethodId = null,
			int? paymentProviderId = null,
			int? scheduledEventId = null,
			int? financialTransactionId = null,
			int? eventChargeId = null,
			DateTime? transactionDate = null,
			decimal? amount = null,
			decimal? processingFee = null,
			decimal? netAmount = null,
			int? currencyId = null,
			string status = null,
			string providerTransactionId = null,
			string providerResponse = null,
			string payerName = null,
			string payerEmail = null,
			string payerPhone = null,
			string receiptNumber = null,
			string notes = null,
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

			IQueryable<Database.PaymentTransaction> query = (from pt in _context.PaymentTransactions select pt);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (paymentMethodId.HasValue == true)
			{
				query = query.Where(pt => pt.paymentMethodId == paymentMethodId.Value);
			}
			if (paymentProviderId.HasValue == true)
			{
				query = query.Where(pt => pt.paymentProviderId == paymentProviderId.Value);
			}
			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(pt => pt.scheduledEventId == scheduledEventId.Value);
			}
			if (financialTransactionId.HasValue == true)
			{
				query = query.Where(pt => pt.financialTransactionId == financialTransactionId.Value);
			}
			if (eventChargeId.HasValue == true)
			{
				query = query.Where(pt => pt.eventChargeId == eventChargeId.Value);
			}
			if (transactionDate.HasValue == true)
			{
				query = query.Where(pt => pt.transactionDate == transactionDate.Value);
			}
			if (amount.HasValue == true)
			{
				query = query.Where(pt => pt.amount == amount.Value);
			}
			if (processingFee.HasValue == true)
			{
				query = query.Where(pt => pt.processingFee == processingFee.Value);
			}
			if (netAmount.HasValue == true)
			{
				query = query.Where(pt => pt.netAmount == netAmount.Value);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(pt => pt.currencyId == currencyId.Value);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(pt => pt.status == status);
			}
			if (string.IsNullOrEmpty(providerTransactionId) == false)
			{
				query = query.Where(pt => pt.providerTransactionId == providerTransactionId);
			}
			if (string.IsNullOrEmpty(providerResponse) == false)
			{
				query = query.Where(pt => pt.providerResponse == providerResponse);
			}
			if (string.IsNullOrEmpty(payerName) == false)
			{
				query = query.Where(pt => pt.payerName == payerName);
			}
			if (string.IsNullOrEmpty(payerEmail) == false)
			{
				query = query.Where(pt => pt.payerEmail == payerEmail);
			}
			if (string.IsNullOrEmpty(payerPhone) == false)
			{
				query = query.Where(pt => pt.payerPhone == payerPhone);
			}
			if (string.IsNullOrEmpty(receiptNumber) == false)
			{
				query = query.Where(pt => pt.receiptNumber == receiptNumber);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(pt => pt.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(pt => pt.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pt => pt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pt => pt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pt => pt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pt => pt.deleted == false);
				}
			}
			else
			{
				query = query.Where(pt => pt.active == true);
				query = query.Where(pt => pt.deleted == false);
			}

			query = query.OrderBy(pt => pt.status).ThenBy(pt => pt.providerTransactionId).ThenBy(pt => pt.payerName);


			//
			// Add the any string contains parameter to span all the string fields on the Payment Transaction, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.status.Contains(anyStringContains)
			       || x.providerTransactionId.Contains(anyStringContains)
			       || x.providerResponse.Contains(anyStringContains)
			       || x.payerName.Contains(anyStringContains)
			       || x.payerEmail.Contains(anyStringContains)
			       || x.payerPhone.Contains(anyStringContains)
			       || x.receiptNumber.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || (includeRelations == true && x.currency.name.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.description.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.code.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.color.Contains(anyStringContains))
			       || (includeRelations == true && x.eventCharge.description.Contains(anyStringContains))
			       || (includeRelations == true && x.eventCharge.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.eventCharge.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.contactRole.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.description.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.journalEntryType.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.referenceNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.externalSystemName.Contains(anyStringContains))
			       || (includeRelations == true && x.paymentMethod.name.Contains(anyStringContains))
			       || (includeRelations == true && x.paymentMethod.description.Contains(anyStringContains))
			       || (includeRelations == true && x.paymentMethod.color.Contains(anyStringContains))
			       || (includeRelations == true && x.paymentProvider.name.Contains(anyStringContains))
			       || (includeRelations == true && x.paymentProvider.description.Contains(anyStringContains))
			       || (includeRelations == true && x.paymentProvider.providerType.Contains(anyStringContains))
			       || (includeRelations == true && x.paymentProvider.apiKeyEncrypted.Contains(anyStringContains))
			       || (includeRelations == true && x.paymentProvider.merchantId.Contains(anyStringContains))
			       || (includeRelations == true && x.paymentProvider.webhookSecret.Contains(anyStringContains))
			       || (includeRelations == true && x.paymentProvider.notes.Contains(anyStringContains))
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
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.currency);
				query = query.Include(x => x.eventCharge);
				query = query.Include(x => x.financialTransaction);
				query = query.Include(x => x.paymentMethod);
				query = query.Include(x => x.paymentProvider);
				query = query.Include(x => x.scheduledEvent);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.PaymentTransaction> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.PaymentTransaction paymentTransaction in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(paymentTransaction, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.PaymentTransaction Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.PaymentTransaction Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of PaymentTransactions filtered by the parameters provided.  Its query is similar to the GetPaymentTransactions method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentTransactions/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? paymentMethodId = null,
			int? paymentProviderId = null,
			int? scheduledEventId = null,
			int? financialTransactionId = null,
			int? eventChargeId = null,
			DateTime? transactionDate = null,
			decimal? amount = null,
			decimal? processingFee = null,
			decimal? netAmount = null,
			int? currencyId = null,
			string status = null,
			string providerTransactionId = null,
			string providerResponse = null,
			string payerName = null,
			string payerEmail = null,
			string payerPhone = null,
			string receiptNumber = null,
			string notes = null,
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

			IQueryable<Database.PaymentTransaction> query = (from pt in _context.PaymentTransactions select pt);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (paymentMethodId.HasValue == true)
			{
				query = query.Where(pt => pt.paymentMethodId == paymentMethodId.Value);
			}
			if (paymentProviderId.HasValue == true)
			{
				query = query.Where(pt => pt.paymentProviderId == paymentProviderId.Value);
			}
			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(pt => pt.scheduledEventId == scheduledEventId.Value);
			}
			if (financialTransactionId.HasValue == true)
			{
				query = query.Where(pt => pt.financialTransactionId == financialTransactionId.Value);
			}
			if (eventChargeId.HasValue == true)
			{
				query = query.Where(pt => pt.eventChargeId == eventChargeId.Value);
			}
			if (transactionDate.HasValue == true)
			{
				query = query.Where(pt => pt.transactionDate == transactionDate.Value);
			}
			if (amount.HasValue == true)
			{
				query = query.Where(pt => pt.amount == amount.Value);
			}
			if (processingFee.HasValue == true)
			{
				query = query.Where(pt => pt.processingFee == processingFee.Value);
			}
			if (netAmount.HasValue == true)
			{
				query = query.Where(pt => pt.netAmount == netAmount.Value);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(pt => pt.currencyId == currencyId.Value);
			}
			if (status != null)
			{
				query = query.Where(pt => pt.status == status);
			}
			if (providerTransactionId != null)
			{
				query = query.Where(pt => pt.providerTransactionId == providerTransactionId);
			}
			if (providerResponse != null)
			{
				query = query.Where(pt => pt.providerResponse == providerResponse);
			}
			if (payerName != null)
			{
				query = query.Where(pt => pt.payerName == payerName);
			}
			if (payerEmail != null)
			{
				query = query.Where(pt => pt.payerEmail == payerEmail);
			}
			if (payerPhone != null)
			{
				query = query.Where(pt => pt.payerPhone == payerPhone);
			}
			if (receiptNumber != null)
			{
				query = query.Where(pt => pt.receiptNumber == receiptNumber);
			}
			if (notes != null)
			{
				query = query.Where(pt => pt.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(pt => pt.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pt => pt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pt => pt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pt => pt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pt => pt.deleted == false);
				}
			}
			else
			{
				query = query.Where(pt => pt.active == true);
				query = query.Where(pt => pt.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Payment Transaction, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.status.Contains(anyStringContains)
			       || x.providerTransactionId.Contains(anyStringContains)
			       || x.providerResponse.Contains(anyStringContains)
			       || x.payerName.Contains(anyStringContains)
			       || x.payerEmail.Contains(anyStringContains)
			       || x.payerPhone.Contains(anyStringContains)
			       || x.receiptNumber.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.currency.name.Contains(anyStringContains)
			       || x.currency.description.Contains(anyStringContains)
			       || x.currency.code.Contains(anyStringContains)
			       || x.currency.color.Contains(anyStringContains)
			       || x.eventCharge.description.Contains(anyStringContains)
			       || x.eventCharge.notes.Contains(anyStringContains)
			       || x.eventCharge.externalId.Contains(anyStringContains)
			       || x.financialTransaction.contactRole.Contains(anyStringContains)
			       || x.financialTransaction.description.Contains(anyStringContains)
			       || x.financialTransaction.journalEntryType.Contains(anyStringContains)
			       || x.financialTransaction.referenceNumber.Contains(anyStringContains)
			       || x.financialTransaction.notes.Contains(anyStringContains)
			       || x.financialTransaction.externalId.Contains(anyStringContains)
			       || x.financialTransaction.externalSystemName.Contains(anyStringContains)
			       || x.paymentMethod.name.Contains(anyStringContains)
			       || x.paymentMethod.description.Contains(anyStringContains)
			       || x.paymentMethod.color.Contains(anyStringContains)
			       || x.paymentProvider.name.Contains(anyStringContains)
			       || x.paymentProvider.description.Contains(anyStringContains)
			       || x.paymentProvider.providerType.Contains(anyStringContains)
			       || x.paymentProvider.apiKeyEncrypted.Contains(anyStringContains)
			       || x.paymentProvider.merchantId.Contains(anyStringContains)
			       || x.paymentProvider.webhookSecret.Contains(anyStringContains)
			       || x.paymentProvider.notes.Contains(anyStringContains)
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
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single PaymentTransaction by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentTransaction/{id}")]
		public async Task<IActionResult> GetPaymentTransaction(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.PaymentTransaction> query = (from pt in _context.PaymentTransactions where
							(pt.id == id) &&
							(userIsAdmin == true || pt.deleted == false) &&
							(userIsWriter == true || pt.active == true)
					select pt);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.currency);
					query = query.Include(x => x.eventCharge);
					query = query.Include(x => x.financialTransaction);
					query = query.Include(x => x.paymentMethod);
					query = query.Include(x => x.paymentProvider);
					query = query.Include(x => x.scheduledEvent);
					query = query.AsSplitQuery();
				}

				Database.PaymentTransaction materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.PaymentTransaction Entity was read with Admin privilege." : "Scheduler.PaymentTransaction Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PaymentTransaction", materialized.id, materialized.status));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.PaymentTransaction entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.PaymentTransaction.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.PaymentTransaction.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing PaymentTransaction record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/PaymentTransaction/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutPaymentTransaction(int id, [FromBody]Database.PaymentTransaction.PaymentTransactionDTO paymentTransactionDTO, CancellationToken cancellationToken = default)
		{
			if (paymentTransactionDTO == null)
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



			if (id != paymentTransactionDTO.id)
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


			IQueryable<Database.PaymentTransaction> query = (from x in _context.PaymentTransactions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.PaymentTransaction existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.PaymentTransaction PUT", id.ToString(), new Exception("No Scheduler.PaymentTransaction entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (paymentTransactionDTO.objectGuid == Guid.Empty)
            {
                paymentTransactionDTO.objectGuid = existing.objectGuid;
            }
            else if (paymentTransactionDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a PaymentTransaction record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.PaymentTransaction cloneOfExisting = (Database.PaymentTransaction)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new PaymentTransaction object using the data from the existing record, updated with what is in the DTO.
			//
			Database.PaymentTransaction paymentTransaction = (Database.PaymentTransaction)_context.Entry(existing).GetDatabaseValues().ToObject();
			paymentTransaction.ApplyDTO(paymentTransactionDTO);
			//
			// The tenant guid for any PaymentTransaction being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the PaymentTransaction because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				paymentTransaction.tenantGuid = existing.tenantGuid;
			}

			lock (paymentTransactionPutSyncRoot)
			{
				//
				// Validate the version number for the paymentTransaction being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != paymentTransaction.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "PaymentTransaction save attempt was made but save request was with version " + paymentTransaction.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The PaymentTransaction you are trying to update has already changed.  Please try your save again after reloading the PaymentTransaction.");
				}
				else
				{
					// Same record.  Increase version.
					paymentTransaction.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (paymentTransaction.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.PaymentTransaction record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (paymentTransaction.transactionDate.Kind != DateTimeKind.Utc)
				{
					paymentTransaction.transactionDate = paymentTransaction.transactionDate.ToUniversalTime();
				}

				if (paymentTransaction.status != null && paymentTransaction.status.Length > 50)
				{
					paymentTransaction.status = paymentTransaction.status.Substring(0, 50);
				}

				if (paymentTransaction.providerTransactionId != null && paymentTransaction.providerTransactionId.Length > 250)
				{
					paymentTransaction.providerTransactionId = paymentTransaction.providerTransactionId.Substring(0, 250);
				}

				if (paymentTransaction.payerName != null && paymentTransaction.payerName.Length > 250)
				{
					paymentTransaction.payerName = paymentTransaction.payerName.Substring(0, 250);
				}

				if (paymentTransaction.payerEmail != null && paymentTransaction.payerEmail.Length > 250)
				{
					paymentTransaction.payerEmail = paymentTransaction.payerEmail.Substring(0, 250);
				}

				if (paymentTransaction.payerPhone != null && paymentTransaction.payerPhone.Length > 50)
				{
					paymentTransaction.payerPhone = paymentTransaction.payerPhone.Substring(0, 50);
				}

				if (paymentTransaction.receiptNumber != null && paymentTransaction.receiptNumber.Length > 100)
				{
					paymentTransaction.receiptNumber = paymentTransaction.receiptNumber.Substring(0, 100);
				}

				try
				{
				    EntityEntry<Database.PaymentTransaction> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(paymentTransaction);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        PaymentTransactionChangeHistory paymentTransactionChangeHistory = new PaymentTransactionChangeHistory();
				        paymentTransactionChangeHistory.paymentTransactionId = paymentTransaction.id;
				        paymentTransactionChangeHistory.versionNumber = paymentTransaction.versionNumber;
				        paymentTransactionChangeHistory.timeStamp = DateTime.UtcNow;
				        paymentTransactionChangeHistory.userId = securityUser.id;
				        paymentTransactionChangeHistory.tenantGuid = userTenantGuid;
				        paymentTransactionChangeHistory.data = JsonSerializer.Serialize(Database.PaymentTransaction.CreateAnonymousWithFirstLevelSubObjects(paymentTransaction));
				        _context.PaymentTransactionChangeHistories.Add(paymentTransactionChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.PaymentTransaction entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.PaymentTransaction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.PaymentTransaction.CreateAnonymousWithFirstLevelSubObjects(paymentTransaction)),
						null);

				return Ok(Database.PaymentTransaction.CreateAnonymous(paymentTransaction));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.PaymentTransaction entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.PaymentTransaction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.PaymentTransaction.CreateAnonymousWithFirstLevelSubObjects(paymentTransaction)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new PaymentTransaction record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentTransaction", Name = "PaymentTransaction")]
		public async Task<IActionResult> PostPaymentTransaction([FromBody]Database.PaymentTransaction.PaymentTransactionDTO paymentTransactionDTO, CancellationToken cancellationToken = default)
		{
			if (paymentTransactionDTO == null)
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
			// Create a new PaymentTransaction object using the data from the DTO
			//
			Database.PaymentTransaction paymentTransaction = Database.PaymentTransaction.FromDTO(paymentTransactionDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				paymentTransaction.tenantGuid = userTenantGuid;

				if (paymentTransaction.transactionDate.Kind != DateTimeKind.Utc)
				{
					paymentTransaction.transactionDate = paymentTransaction.transactionDate.ToUniversalTime();
				}

				if (paymentTransaction.status != null && paymentTransaction.status.Length > 50)
				{
					paymentTransaction.status = paymentTransaction.status.Substring(0, 50);
				}

				if (paymentTransaction.providerTransactionId != null && paymentTransaction.providerTransactionId.Length > 250)
				{
					paymentTransaction.providerTransactionId = paymentTransaction.providerTransactionId.Substring(0, 250);
				}

				if (paymentTransaction.payerName != null && paymentTransaction.payerName.Length > 250)
				{
					paymentTransaction.payerName = paymentTransaction.payerName.Substring(0, 250);
				}

				if (paymentTransaction.payerEmail != null && paymentTransaction.payerEmail.Length > 250)
				{
					paymentTransaction.payerEmail = paymentTransaction.payerEmail.Substring(0, 250);
				}

				if (paymentTransaction.payerPhone != null && paymentTransaction.payerPhone.Length > 50)
				{
					paymentTransaction.payerPhone = paymentTransaction.payerPhone.Substring(0, 50);
				}

				if (paymentTransaction.receiptNumber != null && paymentTransaction.receiptNumber.Length > 100)
				{
					paymentTransaction.receiptNumber = paymentTransaction.receiptNumber.Substring(0, 100);
				}

				paymentTransaction.objectGuid = Guid.NewGuid();
				paymentTransaction.versionNumber = 1;

				_context.PaymentTransactions.Add(paymentTransaction);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the paymentTransaction object so that no further changes will be written to the database
				    //
				    _context.Entry(paymentTransaction).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					paymentTransaction.Documents = null;
					paymentTransaction.PaymentTransactionChangeHistories = null;
					paymentTransaction.Receipts = null;
					paymentTransaction.currency = null;
					paymentTransaction.eventCharge = null;
					paymentTransaction.financialTransaction = null;
					paymentTransaction.paymentMethod = null;
					paymentTransaction.paymentProvider = null;
					paymentTransaction.scheduledEvent = null;


				    PaymentTransactionChangeHistory paymentTransactionChangeHistory = new PaymentTransactionChangeHistory();
				    paymentTransactionChangeHistory.paymentTransactionId = paymentTransaction.id;
				    paymentTransactionChangeHistory.versionNumber = paymentTransaction.versionNumber;
				    paymentTransactionChangeHistory.timeStamp = DateTime.UtcNow;
				    paymentTransactionChangeHistory.userId = securityUser.id;
				    paymentTransactionChangeHistory.tenantGuid = userTenantGuid;
				    paymentTransactionChangeHistory.data = JsonSerializer.Serialize(Database.PaymentTransaction.CreateAnonymousWithFirstLevelSubObjects(paymentTransaction));
				    _context.PaymentTransactionChangeHistories.Add(paymentTransactionChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.PaymentTransaction entity successfully created.",
						true,
						paymentTransaction. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.PaymentTransaction.CreateAnonymousWithFirstLevelSubObjects(paymentTransaction)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.PaymentTransaction entity creation failed.", false, paymentTransaction.id.ToString(), "", JsonSerializer.Serialize(Database.PaymentTransaction.CreateAnonymousWithFirstLevelSubObjects(paymentTransaction)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PaymentTransaction", paymentTransaction.id, paymentTransaction.status));

			return CreatedAtRoute("PaymentTransaction", new { id = paymentTransaction.id }, Database.PaymentTransaction.CreateAnonymousWithFirstLevelSubObjects(paymentTransaction));
		}



        /// <summary>
        /// 
        /// This rolls a PaymentTransaction entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentTransaction/Rollback/{id}")]
		[Route("api/PaymentTransaction/Rollback")]
		public async Task<IActionResult> RollbackToPaymentTransactionVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.PaymentTransaction> query = (from x in _context.PaymentTransactions
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this PaymentTransaction concurrently
			//
			lock (paymentTransactionPutSyncRoot)
			{
				
				Database.PaymentTransaction paymentTransaction = query.FirstOrDefault();
				
				if (paymentTransaction == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.PaymentTransaction rollback", id.ToString(), new Exception("No Scheduler.PaymentTransaction entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the PaymentTransaction current state so we can log it.
				//
				Database.PaymentTransaction cloneOfExisting = (Database.PaymentTransaction)_context.Entry(paymentTransaction).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.Documents = null;
				cloneOfExisting.PaymentTransactionChangeHistories = null;
				cloneOfExisting.Receipts = null;
				cloneOfExisting.currency = null;
				cloneOfExisting.eventCharge = null;
				cloneOfExisting.financialTransaction = null;
				cloneOfExisting.paymentMethod = null;
				cloneOfExisting.paymentProvider = null;
				cloneOfExisting.scheduledEvent = null;

				if (versionNumber >= paymentTransaction.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.PaymentTransaction rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.PaymentTransaction rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				PaymentTransactionChangeHistory paymentTransactionChangeHistory = (from x in _context.PaymentTransactionChangeHistories
				                                               where
				                                               x.paymentTransactionId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (paymentTransactionChangeHistory != null)
				{
				    Database.PaymentTransaction oldPaymentTransaction = JsonSerializer.Deserialize<Database.PaymentTransaction>(paymentTransactionChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    paymentTransaction.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    paymentTransaction.paymentMethodId = oldPaymentTransaction.paymentMethodId;
				    paymentTransaction.paymentProviderId = oldPaymentTransaction.paymentProviderId;
				    paymentTransaction.scheduledEventId = oldPaymentTransaction.scheduledEventId;
				    paymentTransaction.financialTransactionId = oldPaymentTransaction.financialTransactionId;
				    paymentTransaction.eventChargeId = oldPaymentTransaction.eventChargeId;
				    paymentTransaction.transactionDate = oldPaymentTransaction.transactionDate;
				    paymentTransaction.amount = oldPaymentTransaction.amount;
				    paymentTransaction.processingFee = oldPaymentTransaction.processingFee;
				    paymentTransaction.netAmount = oldPaymentTransaction.netAmount;
				    paymentTransaction.currencyId = oldPaymentTransaction.currencyId;
				    paymentTransaction.status = oldPaymentTransaction.status;
				    paymentTransaction.providerTransactionId = oldPaymentTransaction.providerTransactionId;
				    paymentTransaction.providerResponse = oldPaymentTransaction.providerResponse;
				    paymentTransaction.payerName = oldPaymentTransaction.payerName;
				    paymentTransaction.payerEmail = oldPaymentTransaction.payerEmail;
				    paymentTransaction.payerPhone = oldPaymentTransaction.payerPhone;
				    paymentTransaction.receiptNumber = oldPaymentTransaction.receiptNumber;
				    paymentTransaction.notes = oldPaymentTransaction.notes;
				    paymentTransaction.objectGuid = oldPaymentTransaction.objectGuid;
				    paymentTransaction.active = oldPaymentTransaction.active;
				    paymentTransaction.deleted = oldPaymentTransaction.deleted;

				    string serializedPaymentTransaction = JsonSerializer.Serialize(paymentTransaction);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        PaymentTransactionChangeHistory newPaymentTransactionChangeHistory = new PaymentTransactionChangeHistory();
				        newPaymentTransactionChangeHistory.paymentTransactionId = paymentTransaction.id;
				        newPaymentTransactionChangeHistory.versionNumber = paymentTransaction.versionNumber;
				        newPaymentTransactionChangeHistory.timeStamp = DateTime.UtcNow;
				        newPaymentTransactionChangeHistory.userId = securityUser.id;
				        newPaymentTransactionChangeHistory.tenantGuid = userTenantGuid;
				        newPaymentTransactionChangeHistory.data = JsonSerializer.Serialize(Database.PaymentTransaction.CreateAnonymousWithFirstLevelSubObjects(paymentTransaction));
				        _context.PaymentTransactionChangeHistories.Add(newPaymentTransactionChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.PaymentTransaction rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.PaymentTransaction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.PaymentTransaction.CreateAnonymousWithFirstLevelSubObjects(paymentTransaction)),
						null);


				    return Ok(Database.PaymentTransaction.CreateAnonymous(paymentTransaction));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.PaymentTransaction rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.PaymentTransaction rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a PaymentTransaction.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the PaymentTransaction</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentTransaction/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetPaymentTransactionChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.PaymentTransaction paymentTransaction = await _context.PaymentTransactions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (paymentTransaction == null)
			{
				return NotFound();
			}

			try
			{
				paymentTransaction.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.PaymentTransaction> versionInfo = await paymentTransaction.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a PaymentTransaction.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the PaymentTransaction</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentTransaction/{id}/AuditHistory")]
		public async Task<IActionResult> GetPaymentTransactionAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.PaymentTransaction paymentTransaction = await _context.PaymentTransactions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (paymentTransaction == null)
			{
				return NotFound();
			}

			try
			{
				paymentTransaction.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.PaymentTransaction>> versions = await paymentTransaction.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a PaymentTransaction.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the PaymentTransaction</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The PaymentTransaction object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentTransaction/{id}/Version/{version}")]
		public async Task<IActionResult> GetPaymentTransactionVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.PaymentTransaction paymentTransaction = await _context.PaymentTransactions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (paymentTransaction == null)
			{
				return NotFound();
			}

			try
			{
				paymentTransaction.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.PaymentTransaction> versionInfo = await paymentTransaction.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a PaymentTransaction at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the PaymentTransaction</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The PaymentTransaction object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentTransaction/{id}/StateAtTime")]
		public async Task<IActionResult> GetPaymentTransactionStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.PaymentTransaction paymentTransaction = await _context.PaymentTransactions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (paymentTransaction == null)
			{
				return NotFound();
			}

			try
			{
				paymentTransaction.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.PaymentTransaction> versionInfo = await paymentTransaction.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a PaymentTransaction record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentTransaction/{id}")]
		[Route("api/PaymentTransaction")]
		public async Task<IActionResult> DeletePaymentTransaction(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.PaymentTransaction> query = (from x in _context.PaymentTransactions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.PaymentTransaction paymentTransaction = await query.FirstOrDefaultAsync(cancellationToken);

			if (paymentTransaction == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.PaymentTransaction DELETE", id.ToString(), new Exception("No Scheduler.PaymentTransaction entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.PaymentTransaction cloneOfExisting = (Database.PaymentTransaction)_context.Entry(paymentTransaction).GetDatabaseValues().ToObject();


			lock (paymentTransactionDeleteSyncRoot)
			{
			    try
			    {
			        paymentTransaction.deleted = true;
			        paymentTransaction.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        PaymentTransactionChangeHistory paymentTransactionChangeHistory = new PaymentTransactionChangeHistory();
			        paymentTransactionChangeHistory.paymentTransactionId = paymentTransaction.id;
			        paymentTransactionChangeHistory.versionNumber = paymentTransaction.versionNumber;
			        paymentTransactionChangeHistory.timeStamp = DateTime.UtcNow;
			        paymentTransactionChangeHistory.userId = securityUser.id;
			        paymentTransactionChangeHistory.tenantGuid = userTenantGuid;
			        paymentTransactionChangeHistory.data = JsonSerializer.Serialize(Database.PaymentTransaction.CreateAnonymousWithFirstLevelSubObjects(paymentTransaction));
			        _context.PaymentTransactionChangeHistories.Add(paymentTransactionChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.PaymentTransaction entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.PaymentTransaction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.PaymentTransaction.CreateAnonymousWithFirstLevelSubObjects(paymentTransaction)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.PaymentTransaction entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.PaymentTransaction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.PaymentTransaction.CreateAnonymousWithFirstLevelSubObjects(paymentTransaction)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of PaymentTransaction records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/PaymentTransactions/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? paymentMethodId = null,
			int? paymentProviderId = null,
			int? scheduledEventId = null,
			int? financialTransactionId = null,
			int? eventChargeId = null,
			DateTime? transactionDate = null,
			decimal? amount = null,
			decimal? processingFee = null,
			decimal? netAmount = null,
			int? currencyId = null,
			string status = null,
			string providerTransactionId = null,
			string providerResponse = null,
			string payerName = null,
			string payerEmail = null,
			string payerPhone = null,
			string receiptNumber = null,
			string notes = null,
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

			IQueryable<Database.PaymentTransaction> query = (from pt in _context.PaymentTransactions select pt);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (paymentMethodId.HasValue == true)
			{
				query = query.Where(pt => pt.paymentMethodId == paymentMethodId.Value);
			}
			if (paymentProviderId.HasValue == true)
			{
				query = query.Where(pt => pt.paymentProviderId == paymentProviderId.Value);
			}
			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(pt => pt.scheduledEventId == scheduledEventId.Value);
			}
			if (financialTransactionId.HasValue == true)
			{
				query = query.Where(pt => pt.financialTransactionId == financialTransactionId.Value);
			}
			if (eventChargeId.HasValue == true)
			{
				query = query.Where(pt => pt.eventChargeId == eventChargeId.Value);
			}
			if (transactionDate.HasValue == true)
			{
				query = query.Where(pt => pt.transactionDate == transactionDate.Value);
			}
			if (amount.HasValue == true)
			{
				query = query.Where(pt => pt.amount == amount.Value);
			}
			if (processingFee.HasValue == true)
			{
				query = query.Where(pt => pt.processingFee == processingFee.Value);
			}
			if (netAmount.HasValue == true)
			{
				query = query.Where(pt => pt.netAmount == netAmount.Value);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(pt => pt.currencyId == currencyId.Value);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(pt => pt.status == status);
			}
			if (string.IsNullOrEmpty(providerTransactionId) == false)
			{
				query = query.Where(pt => pt.providerTransactionId == providerTransactionId);
			}
			if (string.IsNullOrEmpty(providerResponse) == false)
			{
				query = query.Where(pt => pt.providerResponse == providerResponse);
			}
			if (string.IsNullOrEmpty(payerName) == false)
			{
				query = query.Where(pt => pt.payerName == payerName);
			}
			if (string.IsNullOrEmpty(payerEmail) == false)
			{
				query = query.Where(pt => pt.payerEmail == payerEmail);
			}
			if (string.IsNullOrEmpty(payerPhone) == false)
			{
				query = query.Where(pt => pt.payerPhone == payerPhone);
			}
			if (string.IsNullOrEmpty(receiptNumber) == false)
			{
				query = query.Where(pt => pt.receiptNumber == receiptNumber);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(pt => pt.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(pt => pt.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pt => pt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pt => pt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pt => pt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pt => pt.deleted == false);
				}
			}
			else
			{
				query = query.Where(pt => pt.active == true);
				query = query.Where(pt => pt.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Payment Transaction, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.status.Contains(anyStringContains)
			       || x.providerTransactionId.Contains(anyStringContains)
			       || x.providerResponse.Contains(anyStringContains)
			       || x.payerName.Contains(anyStringContains)
			       || x.payerEmail.Contains(anyStringContains)
			       || x.payerPhone.Contains(anyStringContains)
			       || x.receiptNumber.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.currency.name.Contains(anyStringContains)
			       || x.currency.description.Contains(anyStringContains)
			       || x.currency.code.Contains(anyStringContains)
			       || x.currency.color.Contains(anyStringContains)
			       || x.eventCharge.description.Contains(anyStringContains)
			       || x.eventCharge.notes.Contains(anyStringContains)
			       || x.eventCharge.externalId.Contains(anyStringContains)
			       || x.financialTransaction.contactRole.Contains(anyStringContains)
			       || x.financialTransaction.description.Contains(anyStringContains)
			       || x.financialTransaction.journalEntryType.Contains(anyStringContains)
			       || x.financialTransaction.referenceNumber.Contains(anyStringContains)
			       || x.financialTransaction.notes.Contains(anyStringContains)
			       || x.financialTransaction.externalId.Contains(anyStringContains)
			       || x.financialTransaction.externalSystemName.Contains(anyStringContains)
			       || x.paymentMethod.name.Contains(anyStringContains)
			       || x.paymentMethod.description.Contains(anyStringContains)
			       || x.paymentMethod.color.Contains(anyStringContains)
			       || x.paymentProvider.name.Contains(anyStringContains)
			       || x.paymentProvider.description.Contains(anyStringContains)
			       || x.paymentProvider.providerType.Contains(anyStringContains)
			       || x.paymentProvider.apiKeyEncrypted.Contains(anyStringContains)
			       || x.paymentProvider.merchantId.Contains(anyStringContains)
			       || x.paymentProvider.webhookSecret.Contains(anyStringContains)
			       || x.paymentProvider.notes.Contains(anyStringContains)
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
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.status).ThenBy(x => x.providerTransactionId).ThenBy(x => x.payerName);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.PaymentTransaction.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/PaymentTransaction/CreateAuditEvent")]
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
