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
    /// This auto generated class provides the basic CRUD operations for the Document entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Document entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class DocumentsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object documentPutSyncRoot = new object();
		static object documentDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<DocumentsController> _logger;

		public DocumentsController(SchedulerContext context, ILogger<DocumentsController> logger) : base("Scheduler", "Document")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Documents filtered by the parameters provided.
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
		[Route("api/Documents")]
		public async Task<IActionResult> GetDocuments(
			int? documentTypeId = null,
			int? invoiceId = null,
			int? receiptId = null,
			string name = null,
			string description = null,
			string fileName = null,
			string mimeType = null,
			long? fileSizeBytes = null,
			string fileDataFileName = null,
			long? fileDataSize = null,
			string fileDataMimeType = null,
			int? scheduledEventId = null,
			int? financialTransactionId = null,
			int? contactId = null,
			int? resourceId = null,
			string status = null,
			DateTime? statusDate = null,
			string statusChangedBy = null,
			DateTime? uploadedDate = null,
			string uploadedBy = null,
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
			if (statusDate.HasValue == true && statusDate.Value.Kind != DateTimeKind.Utc)
			{
				statusDate = statusDate.Value.ToUniversalTime();
			}

			if (uploadedDate.HasValue == true && uploadedDate.Value.Kind != DateTimeKind.Utc)
			{
				uploadedDate = uploadedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.Document> query = (from d in _context.Documents select d);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (documentTypeId.HasValue == true)
			{
				query = query.Where(d => d.documentTypeId == documentTypeId.Value);
			}
			if (invoiceId.HasValue == true)
			{
				query = query.Where(d => d.invoiceId == invoiceId.Value);
			}
			if (receiptId.HasValue == true)
			{
				query = query.Where(d => d.receiptId == receiptId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(d => d.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(d => d.description == description);
			}
			if (string.IsNullOrEmpty(fileName) == false)
			{
				query = query.Where(d => d.fileName == fileName);
			}
			if (string.IsNullOrEmpty(mimeType) == false)
			{
				query = query.Where(d => d.mimeType == mimeType);
			}
			if (fileSizeBytes.HasValue == true)
			{
				query = query.Where(d => d.fileSizeBytes == fileSizeBytes.Value);
			}
			if (string.IsNullOrEmpty(fileDataFileName) == false)
			{
				query = query.Where(d => d.fileDataFileName == fileDataFileName);
			}
			if (fileDataSize.HasValue == true)
			{
				query = query.Where(d => d.fileDataSize == fileDataSize.Value);
			}
			if (string.IsNullOrEmpty(fileDataMimeType) == false)
			{
				query = query.Where(d => d.fileDataMimeType == fileDataMimeType);
			}
			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(d => d.scheduledEventId == scheduledEventId.Value);
			}
			if (financialTransactionId.HasValue == true)
			{
				query = query.Where(d => d.financialTransactionId == financialTransactionId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(d => d.contactId == contactId.Value);
			}
			if (resourceId.HasValue == true)
			{
				query = query.Where(d => d.resourceId == resourceId.Value);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(d => d.status == status);
			}
			if (statusDate.HasValue == true)
			{
				query = query.Where(d => d.statusDate == statusDate.Value);
			}
			if (string.IsNullOrEmpty(statusChangedBy) == false)
			{
				query = query.Where(d => d.statusChangedBy == statusChangedBy);
			}
			if (uploadedDate.HasValue == true)
			{
				query = query.Where(d => d.uploadedDate == uploadedDate.Value);
			}
			if (string.IsNullOrEmpty(uploadedBy) == false)
			{
				query = query.Where(d => d.uploadedBy == uploadedBy);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(d => d.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(d => d.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(d => d.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(d => d.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(d => d.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(d => d.deleted == false);
				}
			}
			else
			{
				query = query.Where(d => d.active == true);
				query = query.Where(d => d.deleted == false);
			}

			query = query.OrderBy(d => d.name).ThenBy(d => d.description).ThenBy(d => d.fileName);


			//
			// Add the any string contains parameter to span all the string fields on the Document, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.fileName.Contains(anyStringContains)
			       || x.mimeType.Contains(anyStringContains)
			       || x.fileDataFileName.Contains(anyStringContains)
			       || x.fileDataMimeType.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || x.statusChangedBy.Contains(anyStringContains)
			       || x.uploadedBy.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
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
			       || (includeRelations == true && x.documentType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.documentType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.documentType.color.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.contactRole.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.description.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.journalEntryType.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.referenceNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.externalSystemName.Contains(anyStringContains))
			       || (includeRelations == true && x.invoice.invoiceNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.invoice.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.receipt.receiptNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.receipt.paymentMethod.Contains(anyStringContains))
			       || (includeRelations == true && x.receipt.description.Contains(anyStringContains))
			       || (includeRelations == true && x.receipt.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.name.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.description.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.color.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.avatarMimeType.Contains(anyStringContains))
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
				query = query.Include(x => x.contact);
				query = query.Include(x => x.documentType);
				query = query.Include(x => x.financialTransaction);
				query = query.Include(x => x.invoice);
				query = query.Include(x => x.receipt);
				query = query.Include(x => x.resource);
				query = query.Include(x => x.scheduledEvent);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.Document> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Document document in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(document, databaseStoresDateWithTimeZone);
			}


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			if (diskBasedBinaryStorageMode == true)
			{
				var tasks = materialized.Select(async document =>
				{

					if (document.fileDataData == null &&
					    document.fileDataSize.HasValue == true &&
					    document.fileDataSize.Value > 0)
					{
					    document.fileDataData = await LoadDataFromDiskAsync(document.objectGuid, document.versionNumber, "data");
					}

				}).ToList();

				// Run tasks concurrently and await their completion
				await Task.WhenAll(tasks);
			}

			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.Document Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.Document Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Documents filtered by the parameters provided.  Its query is similar to the GetDocuments method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Documents/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? documentTypeId = null,
			int? invoiceId = null,
			int? receiptId = null,
			string name = null,
			string description = null,
			string fileName = null,
			string mimeType = null,
			long? fileSizeBytes = null,
			string fileDataFileName = null,
			long? fileDataSize = null,
			string fileDataMimeType = null,
			int? scheduledEventId = null,
			int? financialTransactionId = null,
			int? contactId = null,
			int? resourceId = null,
			string status = null,
			DateTime? statusDate = null,
			string statusChangedBy = null,
			DateTime? uploadedDate = null,
			string uploadedBy = null,
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
			if (statusDate.HasValue == true && statusDate.Value.Kind != DateTimeKind.Utc)
			{
				statusDate = statusDate.Value.ToUniversalTime();
			}

			if (uploadedDate.HasValue == true && uploadedDate.Value.Kind != DateTimeKind.Utc)
			{
				uploadedDate = uploadedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.Document> query = (from d in _context.Documents select d);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (documentTypeId.HasValue == true)
			{
				query = query.Where(d => d.documentTypeId == documentTypeId.Value);
			}
			if (invoiceId.HasValue == true)
			{
				query = query.Where(d => d.invoiceId == invoiceId.Value);
			}
			if (receiptId.HasValue == true)
			{
				query = query.Where(d => d.receiptId == receiptId.Value);
			}
			if (name != null)
			{
				query = query.Where(d => d.name == name);
			}
			if (description != null)
			{
				query = query.Where(d => d.description == description);
			}
			if (fileName != null)
			{
				query = query.Where(d => d.fileName == fileName);
			}
			if (mimeType != null)
			{
				query = query.Where(d => d.mimeType == mimeType);
			}
			if (fileSizeBytes.HasValue == true)
			{
				query = query.Where(d => d.fileSizeBytes == fileSizeBytes.Value);
			}
			if (fileDataFileName != null)
			{
				query = query.Where(d => d.fileDataFileName == fileDataFileName);
			}
			if (fileDataSize.HasValue == true)
			{
				query = query.Where(d => d.fileDataSize == fileDataSize.Value);
			}
			if (fileDataMimeType != null)
			{
				query = query.Where(d => d.fileDataMimeType == fileDataMimeType);
			}
			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(d => d.scheduledEventId == scheduledEventId.Value);
			}
			if (financialTransactionId.HasValue == true)
			{
				query = query.Where(d => d.financialTransactionId == financialTransactionId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(d => d.contactId == contactId.Value);
			}
			if (resourceId.HasValue == true)
			{
				query = query.Where(d => d.resourceId == resourceId.Value);
			}
			if (status != null)
			{
				query = query.Where(d => d.status == status);
			}
			if (statusDate.HasValue == true)
			{
				query = query.Where(d => d.statusDate == statusDate.Value);
			}
			if (statusChangedBy != null)
			{
				query = query.Where(d => d.statusChangedBy == statusChangedBy);
			}
			if (uploadedDate.HasValue == true)
			{
				query = query.Where(d => d.uploadedDate == uploadedDate.Value);
			}
			if (uploadedBy != null)
			{
				query = query.Where(d => d.uploadedBy == uploadedBy);
			}
			if (notes != null)
			{
				query = query.Where(d => d.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(d => d.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(d => d.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(d => d.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(d => d.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(d => d.deleted == false);
				}
			}
			else
			{
				query = query.Where(d => d.active == true);
				query = query.Where(d => d.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Document, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.fileName.Contains(anyStringContains)
			       || x.mimeType.Contains(anyStringContains)
			       || x.fileDataFileName.Contains(anyStringContains)
			       || x.fileDataMimeType.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || x.statusChangedBy.Contains(anyStringContains)
			       || x.uploadedBy.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
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
			       || x.documentType.name.Contains(anyStringContains)
			       || x.documentType.description.Contains(anyStringContains)
			       || x.documentType.color.Contains(anyStringContains)
			       || x.financialTransaction.contactRole.Contains(anyStringContains)
			       || x.financialTransaction.description.Contains(anyStringContains)
			       || x.financialTransaction.journalEntryType.Contains(anyStringContains)
			       || x.financialTransaction.referenceNumber.Contains(anyStringContains)
			       || x.financialTransaction.notes.Contains(anyStringContains)
			       || x.financialTransaction.externalId.Contains(anyStringContains)
			       || x.financialTransaction.externalSystemName.Contains(anyStringContains)
			       || x.invoice.invoiceNumber.Contains(anyStringContains)
			       || x.invoice.notes.Contains(anyStringContains)
			       || x.receipt.receiptNumber.Contains(anyStringContains)
			       || x.receipt.paymentMethod.Contains(anyStringContains)
			       || x.receipt.description.Contains(anyStringContains)
			       || x.receipt.notes.Contains(anyStringContains)
			       || x.resource.name.Contains(anyStringContains)
			       || x.resource.description.Contains(anyStringContains)
			       || x.resource.notes.Contains(anyStringContains)
			       || x.resource.externalId.Contains(anyStringContains)
			       || x.resource.color.Contains(anyStringContains)
			       || x.resource.attributes.Contains(anyStringContains)
			       || x.resource.avatarFileName.Contains(anyStringContains)
			       || x.resource.avatarMimeType.Contains(anyStringContains)
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
        /// This gets a single Document by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Document/{id}")]
		public async Task<IActionResult> GetDocument(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.Document> query = (from d in _context.Documents where
							(d.id == id) &&
							(userIsAdmin == true || d.deleted == false) &&
							(userIsWriter == true || d.active == true)
					select d);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.contact);
					query = query.Include(x => x.documentType);
					query = query.Include(x => x.financialTransaction);
					query = query.Include(x => x.invoice);
					query = query.Include(x => x.receipt);
					query = query.Include(x => x.resource);
					query = query.Include(x => x.scheduledEvent);
					query = query.AsSplitQuery();
				}

				Database.Document materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

					if (diskBasedBinaryStorageMode == true &&
					    materialized.fileDataData == null &&
					    materialized.fileDataSize.HasValue == true &&
					    materialized.fileDataSize.Value > 0)
					{
					    materialized.fileDataData = await LoadDataFromDiskAsync(materialized.objectGuid, materialized.versionNumber, "data", cancellationToken);
					}

					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.Document Entity was read with Admin privilege." : "Scheduler.Document Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Document", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.Document entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.Document.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.Document.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing Document record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Document/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutDocument(int id, [FromBody]Database.Document.DocumentDTO documentDTO, CancellationToken cancellationToken = default)
		{
			if (documentDTO == null)
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



			if (id != documentDTO.id)
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


			IQueryable<Database.Document> query = (from x in _context.Documents
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Document existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Document PUT", id.ToString(), new Exception("No Scheduler.Document entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (documentDTO.objectGuid == Guid.Empty)
            {
                documentDTO.objectGuid = existing.objectGuid;
            }
            else if (documentDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Document record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Document cloneOfExisting = (Database.Document)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Document object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Document document = (Database.Document)_context.Entry(existing).GetDatabaseValues().ToObject();
			document.ApplyDTO(documentDTO);
			//
			// The tenant guid for any Document being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the Document because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				document.tenantGuid = existing.tenantGuid;
			}

			lock (documentPutSyncRoot)
			{
				//
				// Validate the version number for the document being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != document.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Document save attempt was made but save request was with version " + document.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The Document you are trying to update has already changed.  Please try your save again after reloading the Document.");
				}
				else
				{
					// Same record.  Increase version.
					document.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (document.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.Document record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (document.name != null && document.name.Length > 250)
				{
					document.name = document.name.Substring(0, 250);
				}

				if (document.description != null && document.description.Length > 500)
				{
					document.description = document.description.Substring(0, 500);
				}

				if (document.fileName != null && document.fileName.Length > 500)
				{
					document.fileName = document.fileName.Substring(0, 500);
				}

				if (document.mimeType != null && document.mimeType.Length > 100)
				{
					document.mimeType = document.mimeType.Substring(0, 100);
				}

				if (document.fileDataFileName != null && document.fileDataFileName.Length > 250)
				{
					document.fileDataFileName = document.fileDataFileName.Substring(0, 250);
				}

				if (document.fileDataMimeType != null && document.fileDataMimeType.Length > 100)
				{
					document.fileDataMimeType = document.fileDataMimeType.Substring(0, 100);
				}

				if (document.status != null && document.status.Length > 50)
				{
					document.status = document.status.Substring(0, 50);
				}

				if (document.statusDate.HasValue == true && document.statusDate.Value.Kind != DateTimeKind.Utc)
				{
					document.statusDate = document.statusDate.Value.ToUniversalTime();
				}

				if (document.statusChangedBy != null && document.statusChangedBy.Length > 100)
				{
					document.statusChangedBy = document.statusChangedBy.Substring(0, 100);
				}

				if (document.uploadedDate.Kind != DateTimeKind.Utc)
				{
					document.uploadedDate = document.uploadedDate.ToUniversalTime();
				}

				if (document.uploadedBy != null && document.uploadedBy.Length > 100)
				{
					document.uploadedBy = document.uploadedBy.Substring(0, 100);
				}


				//
				// Add default values for any missing data attribute fields.
				//
				if (document.fileDataData != null && string.IsNullOrEmpty(document.fileDataFileName))
				{
				    document.fileDataFileName = document.objectGuid.ToString() + ".data";
				}

				if (document.fileDataData != null && (document.fileDataSize.HasValue == false || document.fileDataSize != document.fileDataData.Length))
				{
				    document.fileDataSize = document.fileDataData.Length;
				}

				if (document.fileDataData != null && string.IsNullOrEmpty(document.fileDataMimeType))
				{
				    document.fileDataMimeType = "application/octet-stream";
				}

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				try
				{
					byte[] dataReferenceBeforeClearing = document.fileDataData;

					if (diskBasedBinaryStorageMode == true &&
					    document.fileDataFileName != null &&
					    document.fileDataData != null &&
					    document.fileDataSize.HasValue == true &&
					    document.fileDataSize.Value > 0)
					{
					    //
					    // write the bytes to disk
					    //
					    WriteDataToDisk(document.objectGuid, document.versionNumber, document.fileDataData, "data");

					    //
					    // Clear the data from the object before we put it into the db
					    //
					    document.fileDataData = null;

					}

				    EntityEntry<Database.Document> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(document);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        DocumentChangeHistory documentChangeHistory = new DocumentChangeHistory();
				        documentChangeHistory.documentId = document.id;
				        documentChangeHistory.versionNumber = document.versionNumber;
				        documentChangeHistory.timeStamp = DateTime.UtcNow;
				        documentChangeHistory.userId = securityUser.id;
				        documentChangeHistory.tenantGuid = userTenantGuid;
				        documentChangeHistory.data = JsonSerializer.Serialize(Database.Document.CreateAnonymousWithFirstLevelSubObjects(document));
				        _context.DocumentChangeHistories.Add(documentChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    document.fileDataData = dataReferenceBeforeClearing;
					}

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Document entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Document.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Document.CreateAnonymousWithFirstLevelSubObjects(document)),
						null);

				return Ok(Database.Document.CreateAnonymous(document));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Document entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Document.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Document.CreateAnonymousWithFirstLevelSubObjects(document)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new Document record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Document", Name = "Document")]
		public async Task<IActionResult> PostDocument([FromBody]Database.Document.DocumentDTO documentDTO, CancellationToken cancellationToken = default)
		{
			if (documentDTO == null)
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
			// Create a new Document object using the data from the DTO
			//
			Database.Document document = Database.Document.FromDTO(documentDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				document.tenantGuid = userTenantGuid;

				if (document.name != null && document.name.Length > 250)
				{
					document.name = document.name.Substring(0, 250);
				}

				if (document.description != null && document.description.Length > 500)
				{
					document.description = document.description.Substring(0, 500);
				}

				if (document.fileName != null && document.fileName.Length > 500)
				{
					document.fileName = document.fileName.Substring(0, 500);
				}

				if (document.mimeType != null && document.mimeType.Length > 100)
				{
					document.mimeType = document.mimeType.Substring(0, 100);
				}

				if (document.fileDataFileName != null && document.fileDataFileName.Length > 250)
				{
					document.fileDataFileName = document.fileDataFileName.Substring(0, 250);
				}

				if (document.fileDataMimeType != null && document.fileDataMimeType.Length > 100)
				{
					document.fileDataMimeType = document.fileDataMimeType.Substring(0, 100);
				}

				if (document.status != null && document.status.Length > 50)
				{
					document.status = document.status.Substring(0, 50);
				}

				if (document.statusDate.HasValue == true && document.statusDate.Value.Kind != DateTimeKind.Utc)
				{
					document.statusDate = document.statusDate.Value.ToUniversalTime();
				}

				if (document.statusChangedBy != null && document.statusChangedBy.Length > 100)
				{
					document.statusChangedBy = document.statusChangedBy.Substring(0, 100);
				}

				if (document.uploadedDate.Kind != DateTimeKind.Utc)
				{
					document.uploadedDate = document.uploadedDate.ToUniversalTime();
				}

				if (document.uploadedBy != null && document.uploadedBy.Length > 100)
				{
					document.uploadedBy = document.uploadedBy.Substring(0, 100);
				}

				document.objectGuid = Guid.NewGuid();

				//
				// Add default values for any missing data attribute fields.
				//
				if (document.fileDataData != null && string.IsNullOrEmpty(document.fileDataFileName))
				{
				    document.fileDataFileName = document.objectGuid.ToString() + ".data";
				}

				if (document.fileDataData != null && (document.fileDataSize.HasValue == false || document.fileDataSize != document.fileDataData.Length))
				{
				    document.fileDataSize = document.fileDataData.Length;
				}

				if (document.fileDataData != null && string.IsNullOrEmpty(document.fileDataMimeType))
				{
				    document.fileDataMimeType = "application/octet-stream";
				}

				document.versionNumber = 1;

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				byte[] dataReferenceBeforeClearing = document.fileDataData;

				if (diskBasedBinaryStorageMode == true &&
				    document.fileDataData != null &&
				    document.fileDataFileName != null &&
				    document.fileDataSize.HasValue == true &&
				    document.fileDataSize.Value > 0)
				{
				    //
				    // write the bytes to disk
				    //
				    await WriteDataToDiskAsync(document.objectGuid, document.versionNumber, document.fileDataData, "data", cancellationToken);

				    //
				    // Clear the data from the object before we put it into the db
				    //
				    document.fileDataData = null;

				}

				_context.Documents.Add(document);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the document object so that no further changes will be written to the database
				    //
				    _context.Entry(document).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					document.fileDataData = null;
					document.DocumentChangeHistories = null;
					document.contact = null;
					document.documentType = null;
					document.financialTransaction = null;
					document.invoice = null;
					document.receipt = null;
					document.resource = null;
					document.scheduledEvent = null;


				    DocumentChangeHistory documentChangeHistory = new DocumentChangeHistory();
				    documentChangeHistory.documentId = document.id;
				    documentChangeHistory.versionNumber = document.versionNumber;
				    documentChangeHistory.timeStamp = DateTime.UtcNow;
				    documentChangeHistory.userId = securityUser.id;
				    documentChangeHistory.tenantGuid = userTenantGuid;
				    documentChangeHistory.data = JsonSerializer.Serialize(Database.Document.CreateAnonymousWithFirstLevelSubObjects(document));
				    _context.DocumentChangeHistories.Add(documentChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.Document entity successfully created.",
						true,
						document. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.Document.CreateAnonymousWithFirstLevelSubObjects(document)),
						null);



					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    document.fileDataData = dataReferenceBeforeClearing;
					}

				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.Document entity creation failed.", false, document.id.ToString(), "", JsonSerializer.Serialize(Database.Document.CreateAnonymousWithFirstLevelSubObjects(document)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Document", document.id, document.name));

			return CreatedAtRoute("Document", new { id = document.id }, Database.Document.CreateAnonymousWithFirstLevelSubObjects(document));
		}



        /// <summary>
        /// 
        /// This rolls a Document entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Document/Rollback/{id}")]
		[Route("api/Document/Rollback")]
		public async Task<IActionResult> RollbackToDocumentVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.Document> query = (from x in _context.Documents
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();


			//
			// Make sure nobody else is editing this Document concurrently
			//
			lock (documentPutSyncRoot)
			{
				
				Database.Document document = query.FirstOrDefault();
				
				if (document == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Document rollback", id.ToString(), new Exception("No Scheduler.Document entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the Document current state so we can log it.
				//
				Database.Document cloneOfExisting = (Database.Document)_context.Entry(document).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.fileDataData = null;
				cloneOfExisting.DocumentChangeHistories = null;
				cloneOfExisting.contact = null;
				cloneOfExisting.documentType = null;
				cloneOfExisting.financialTransaction = null;
				cloneOfExisting.invoice = null;
				cloneOfExisting.receipt = null;
				cloneOfExisting.resource = null;
				cloneOfExisting.scheduledEvent = null;

				if (versionNumber >= document.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.Document rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.Document rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				DocumentChangeHistory documentChangeHistory = (from x in _context.DocumentChangeHistories
				                                               where
				                                               x.documentId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (documentChangeHistory != null)
				{
				    Database.Document oldDocument = JsonSerializer.Deserialize<Database.Document>(documentChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    document.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    document.documentTypeId = oldDocument.documentTypeId;
				    document.invoiceId = oldDocument.invoiceId;
				    document.receiptId = oldDocument.receiptId;
				    document.name = oldDocument.name;
				    document.description = oldDocument.description;
				    document.fileName = oldDocument.fileName;
				    document.mimeType = oldDocument.mimeType;
				    document.fileSizeBytes = oldDocument.fileSizeBytes;
				    document.fileDataFileName = oldDocument.fileDataFileName;
				    document.fileDataSize = oldDocument.fileDataSize;
				    document.fileDataData = oldDocument.fileDataData;
				    document.fileDataMimeType = oldDocument.fileDataMimeType;
				    document.scheduledEventId = oldDocument.scheduledEventId;
				    document.financialTransactionId = oldDocument.financialTransactionId;
				    document.contactId = oldDocument.contactId;
				    document.resourceId = oldDocument.resourceId;
				    document.status = oldDocument.status;
				    document.statusDate = oldDocument.statusDate;
				    document.statusChangedBy = oldDocument.statusChangedBy;
				    document.uploadedDate = oldDocument.uploadedDate;
				    document.uploadedBy = oldDocument.uploadedBy;
				    document.notes = oldDocument.notes;
				    document.objectGuid = oldDocument.objectGuid;
				    document.active = oldDocument.active;
				    document.deleted = oldDocument.deleted;
				    //
				    // If disk based binary mode is on, then we need to copy the old data file over as well.
				    //
				    if (diskBasedBinaryStorageMode == true)
				    {
				    	Byte[] binaryData = LoadDataFromDisk(oldDocument.objectGuid, oldDocument.versionNumber, "data");

				    	//
				    	// Write out the data as the new version
				    	//
				    	WriteDataToDisk(document.objectGuid, document.versionNumber, binaryData, "data");
				    }

				    string serializedDocument = JsonSerializer.Serialize(document);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        DocumentChangeHistory newDocumentChangeHistory = new DocumentChangeHistory();
				        newDocumentChangeHistory.documentId = document.id;
				        newDocumentChangeHistory.versionNumber = document.versionNumber;
				        newDocumentChangeHistory.timeStamp = DateTime.UtcNow;
				        newDocumentChangeHistory.userId = securityUser.id;
				        newDocumentChangeHistory.tenantGuid = userTenantGuid;
				        newDocumentChangeHistory.data = JsonSerializer.Serialize(Database.Document.CreateAnonymousWithFirstLevelSubObjects(document));
				        _context.DocumentChangeHistories.Add(newDocumentChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Document rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Document.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Document.CreateAnonymousWithFirstLevelSubObjects(document)),
						null);


				    return Ok(Database.Document.CreateAnonymous(document));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.Document rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.Document rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a Document.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Document</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Document/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetDocumentChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.Document document = await _context.Documents.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (document == null)
			{
				return NotFound();
			}

			try
			{
				document.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Document> versionInfo = await document.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a Document.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Document</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Document/{id}/AuditHistory")]
		public async Task<IActionResult> GetDocumentAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.Document document = await _context.Documents.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (document == null)
			{
				return NotFound();
			}

			try
			{
				document.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.Document>> versions = await document.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a Document.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Document</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The Document object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Document/{id}/Version/{version}")]
		public async Task<IActionResult> GetDocumentVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.Document document = await _context.Documents.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (document == null)
			{
				return NotFound();
			}

			try
			{
				document.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Document> versionInfo = await document.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a Document at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Document</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The Document object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Document/{id}/StateAtTime")]
		public async Task<IActionResult> GetDocumentStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.Document document = await _context.Documents.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (document == null)
			{
				return NotFound();
			}

			try
			{
				document.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Document> versionInfo = await document.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a Document record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Document/{id}")]
		[Route("api/Document")]
		public async Task<IActionResult> DeleteDocument(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.Document> query = (from x in _context.Documents
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Document document = await query.FirstOrDefaultAsync(cancellationToken);

			if (document == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Document DELETE", id.ToString(), new Exception("No Scheduler.Document entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Document cloneOfExisting = (Database.Document)_context.Entry(document).GetDatabaseValues().ToObject();


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			lock (documentDeleteSyncRoot)
			{
			    try
			    {
			        document.deleted = true;
			        document.versionNumber++;

			        _context.SaveChanges();

			        //
			        // If in disk based storage mode, create a copy of the disk data file for the new version.
			        //
			        if (diskBasedBinaryStorageMode == true)
			        {
			        	Byte[] binaryData = LoadDataFromDisk(document.objectGuid, document.versionNumber -1, "data");

			        	//
			        	// Write out the same data
			        	//
			        	WriteDataToDisk(document.objectGuid, document.versionNumber, binaryData, "data");
			        }

			        //
			        // Now add the change history
			        //
			        DocumentChangeHistory documentChangeHistory = new DocumentChangeHistory();
			        documentChangeHistory.documentId = document.id;
			        documentChangeHistory.versionNumber = document.versionNumber;
			        documentChangeHistory.timeStamp = DateTime.UtcNow;
			        documentChangeHistory.userId = securityUser.id;
			        documentChangeHistory.tenantGuid = userTenantGuid;
			        documentChangeHistory.data = JsonSerializer.Serialize(Database.Document.CreateAnonymousWithFirstLevelSubObjects(document));
			        _context.DocumentChangeHistories.Add(documentChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Document entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Document.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Document.CreateAnonymousWithFirstLevelSubObjects(document)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Document entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Document.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Document.CreateAnonymousWithFirstLevelSubObjects(document)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of Document records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Documents/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? documentTypeId = null,
			int? invoiceId = null,
			int? receiptId = null,
			string name = null,
			string description = null,
			string fileName = null,
			string mimeType = null,
			long? fileSizeBytes = null,
			string fileDataFileName = null,
			long? fileDataSize = null,
			string fileDataMimeType = null,
			int? scheduledEventId = null,
			int? financialTransactionId = null,
			int? contactId = null,
			int? resourceId = null,
			string status = null,
			DateTime? statusDate = null,
			string statusChangedBy = null,
			DateTime? uploadedDate = null,
			string uploadedBy = null,
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
			if (statusDate.HasValue == true && statusDate.Value.Kind != DateTimeKind.Utc)
			{
				statusDate = statusDate.Value.ToUniversalTime();
			}

			if (uploadedDate.HasValue == true && uploadedDate.Value.Kind != DateTimeKind.Utc)
			{
				uploadedDate = uploadedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.Document> query = (from d in _context.Documents select d);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (documentTypeId.HasValue == true)
			{
				query = query.Where(d => d.documentTypeId == documentTypeId.Value);
			}
			if (invoiceId.HasValue == true)
			{
				query = query.Where(d => d.invoiceId == invoiceId.Value);
			}
			if (receiptId.HasValue == true)
			{
				query = query.Where(d => d.receiptId == receiptId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(d => d.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(d => d.description == description);
			}
			if (string.IsNullOrEmpty(fileName) == false)
			{
				query = query.Where(d => d.fileName == fileName);
			}
			if (string.IsNullOrEmpty(mimeType) == false)
			{
				query = query.Where(d => d.mimeType == mimeType);
			}
			if (fileSizeBytes.HasValue == true)
			{
				query = query.Where(d => d.fileSizeBytes == fileSizeBytes.Value);
			}
			if (string.IsNullOrEmpty(fileDataFileName) == false)
			{
				query = query.Where(d => d.fileDataFileName == fileDataFileName);
			}
			if (fileDataSize.HasValue == true)
			{
				query = query.Where(d => d.fileDataSize == fileDataSize.Value);
			}
			if (string.IsNullOrEmpty(fileDataMimeType) == false)
			{
				query = query.Where(d => d.fileDataMimeType == fileDataMimeType);
			}
			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(d => d.scheduledEventId == scheduledEventId.Value);
			}
			if (financialTransactionId.HasValue == true)
			{
				query = query.Where(d => d.financialTransactionId == financialTransactionId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(d => d.contactId == contactId.Value);
			}
			if (resourceId.HasValue == true)
			{
				query = query.Where(d => d.resourceId == resourceId.Value);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(d => d.status == status);
			}
			if (statusDate.HasValue == true)
			{
				query = query.Where(d => d.statusDate == statusDate.Value);
			}
			if (string.IsNullOrEmpty(statusChangedBy) == false)
			{
				query = query.Where(d => d.statusChangedBy == statusChangedBy);
			}
			if (uploadedDate.HasValue == true)
			{
				query = query.Where(d => d.uploadedDate == uploadedDate.Value);
			}
			if (string.IsNullOrEmpty(uploadedBy) == false)
			{
				query = query.Where(d => d.uploadedBy == uploadedBy);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(d => d.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(d => d.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(d => d.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(d => d.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(d => d.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(d => d.deleted == false);
				}
			}
			else
			{
				query = query.Where(d => d.active == true);
				query = query.Where(d => d.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Document, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.fileName.Contains(anyStringContains)
			       || x.mimeType.Contains(anyStringContains)
			       || x.fileDataFileName.Contains(anyStringContains)
			       || x.fileDataMimeType.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || x.statusChangedBy.Contains(anyStringContains)
			       || x.uploadedBy.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
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
			       || x.documentType.name.Contains(anyStringContains)
			       || x.documentType.description.Contains(anyStringContains)
			       || x.documentType.color.Contains(anyStringContains)
			       || x.financialTransaction.contactRole.Contains(anyStringContains)
			       || x.financialTransaction.description.Contains(anyStringContains)
			       || x.financialTransaction.journalEntryType.Contains(anyStringContains)
			       || x.financialTransaction.referenceNumber.Contains(anyStringContains)
			       || x.financialTransaction.notes.Contains(anyStringContains)
			       || x.financialTransaction.externalId.Contains(anyStringContains)
			       || x.financialTransaction.externalSystemName.Contains(anyStringContains)
			       || x.invoice.invoiceNumber.Contains(anyStringContains)
			       || x.invoice.notes.Contains(anyStringContains)
			       || x.receipt.receiptNumber.Contains(anyStringContains)
			       || x.receipt.paymentMethod.Contains(anyStringContains)
			       || x.receipt.description.Contains(anyStringContains)
			       || x.receipt.notes.Contains(anyStringContains)
			       || x.resource.name.Contains(anyStringContains)
			       || x.resource.description.Contains(anyStringContains)
			       || x.resource.notes.Contains(anyStringContains)
			       || x.resource.externalId.Contains(anyStringContains)
			       || x.resource.color.Contains(anyStringContains)
			       || x.resource.attributes.Contains(anyStringContains)
			       || x.resource.avatarFileName.Contains(anyStringContains)
			       || x.resource.avatarMimeType.Contains(anyStringContains)
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


			query = query.OrderBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.fileName);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Document.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Document/CreateAuditEvent")]
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




        [Route("api/Document/Data/{id:int}")]
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


            Database.Document document = await (from x in _context.Documents where x.id == id && x.active == true && x.deleted == false select x).FirstOrDefaultAsync();
            if (document == null)
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

						lock (documentPutSyncRoot)
						{
							try
							{
								using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
								{
									document.fileDataFileName = fileName.Trim();
									document.fileDataMimeType = mimeType;
									document.fileDataSize = section.Body.Length;

									document.versionNumber++;

									if (diskBasedBinaryStorageMode == true &&
										 document.fileDataFileName != null &&
										 document.fileDataSize > 0)
									{
										//
										// write the bytes to disk
										//
										WriteDataToDisk(document.objectGuid, document.versionNumber, section.Body, "data");
										//
										// Clear the data from the object before we put it into the db
										//
										document.fileDataData = null;
									}
									else
									{
										using (MemoryStream memoryStream = new MemoryStream((int)section.Body.Length))
										{
											section.Body.CopyTo(memoryStream);
											document.fileDataData = memoryStream.ToArray();
										}
									}
									//
									// Now add the change history
									//
									DocumentChangeHistory documentChangeHistory = new DocumentChangeHistory();
									documentChangeHistory.documentId = document.id;
									documentChangeHistory.versionNumber = document.versionNumber;
									documentChangeHistory.timeStamp = DateTime.UtcNow;
									documentChangeHistory.userId = securityUser.id;
									documentChangeHistory.tenantGuid = document.tenantGuid;
									documentChangeHistory.data = JsonSerializer.Serialize(Database.Document.CreateAnonymousWithFirstLevelSubObjects(document));
									_context.DocumentChangeHistories.Add(documentChangeHistory);

									_context.SaveChanges();

									transaction.Commit();

									CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Document Data Uploaded with filename of " + fileName + " and with size of " + section.Body.Length, id.ToString());
								}
							}
							catch (Exception ex)
							{
								CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "Document Data Upload Failed.", false, id.ToString(), "", "", ex);

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
                lock (documentPutSyncRoot)
                {
                    try
                    {
                        using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                        {
                            if (diskBasedBinaryStorageMode == true)
                            {
								DeleteDataFromDisk(document.objectGuid, document.versionNumber, "data");
                            }

                            document.fileDataFileName = null;
                            document.fileDataMimeType = null;
                            document.fileDataSize = 0;
                            document.fileDataData = null;
                            document.versionNumber++;


                            //
                            // Now add the change history
                            //
                            DocumentChangeHistory documentChangeHistory = new DocumentChangeHistory();
                            documentChangeHistory.documentId = document.id;
                            documentChangeHistory.versionNumber = document.versionNumber;
                            documentChangeHistory.timeStamp = DateTime.UtcNow;
                            documentChangeHistory.userId = securityUser.id;
                                    documentChangeHistory.tenantGuid = document.tenantGuid;
                                    documentChangeHistory.data = JsonSerializer.Serialize(Database.Document.CreateAnonymousWithFirstLevelSubObjects(document));
                            _context.DocumentChangeHistories.Add(documentChangeHistory);

                            _context.SaveChanges();

                            transaction.Commit();

                            CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Document data cleared.", id.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "Document data clear failed.", false, id.ToString(), "", "", ex);

                        return Problem(ex.Message);
                    }
                }
            }

            return Ok();
        }

        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/Document/Data/{id:int}")]
        public async Task<IActionResult> DownloadDataAsync(int id, CancellationToken cancellationToken = default)
        {

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			using (SchedulerContext context = new SchedulerContext())
            {
                //
                // Return the data to the user as though it was a file.
                //
                Database.Document document = await (from d in context.Documents
                                                where d.id == id &&
                                                d.active == true &&
                                                d.deleted == false
                                                select d).FirstOrDefaultAsync();

                if (document != null && document.fileDataData != null)
                {
                   return File(document.fileDataData.ToArray<byte>(), document.fileDataMimeType, document.fileDataFileName != null ? document.fileDataFileName.Trim() : "Document_" + document.id.ToString(), true);
                }
                else
                {
                    return BadRequest();
                }
            }
        }
	}
}
