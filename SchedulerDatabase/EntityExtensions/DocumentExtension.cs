using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.Scheduler.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class Document : IVersionTrackedEntity<Document>, IAnonymousConvertible
	{
        /// <summary>
        /// This is for setting the context for change history inquiries.
        /// </summary>
        private SchedulerContext _contextForVersionInquiry = null;
        private Guid _tenantGuidForVersionInquiry = Guid.Empty;



        /// <summary>
        /// 
        /// Gets the a Change History toolset for the user that support write and read operations.
        /// 
        /// </summary>
        /// <param name="context">A context object that contains the entities</param>
        /// <param name="securityUser">The security user that the changes will be made on behalf of.</param>
        /// <param name="insideTransaction">Whether or not there is a transaction in process by the using function</param>
        /// <returns>A change history toolset instance to interact with the change history of the entity</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public static ChangeHistoryToolset<Document, DocumentChangeHistory> GetChangeHistoryToolsetForWriting(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (securityUser == null)
            {
                throw new ArgumentNullException(nameof(securityUser));
            }

            //
            // This table does not have data visibility enabled, therefore the user ID is to be taken directly from the security user object.
            // 
            return new ChangeHistoryToolset<Document, DocumentChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
        }


        /// <summary>
        /// 
        /// Gets the a Change History toolset for the user that support write and read operations.  (Async variant)
        /// 
        /// </summary>
        /// <param name="context">A context object that contains the entities</param>
        /// <param name="securityUser">The security user that the changes will be made on behalf of.</param>
        /// <param name="insideTransaction">Whether or not there is a transaction in process by the using function</param>
        /// <returns>A change history toolset instance to interact with the change history of the entity</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public static Task<ChangeHistoryToolset<Document, DocumentChangeHistory>> GetChangeHistoryToolsetForWritingAsync(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (securityUser == null)
            {
                throw new ArgumentNullException(nameof(securityUser));
            }

            //
            // This table does not have data visibility enabled, therefore the user ID is to be taken directly from the security user object.
            // No async work is needed — return completed task.
            // 
            return Task.FromResult(new ChangeHistoryToolset<Document, DocumentChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken));
        }

        /// <summary>
        /// 
        /// Gets the a Change History toolset for read only purposes.
        /// 
        /// </summary>
        /// <param name="context">A context object that contains the entities</param>
        /// <returns>A change history toolset instance to interact with the change history of the entity</returns>       
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public static ChangeHistoryToolset<Document, DocumentChangeHistory> GetChangeHistoryToolsetForReading(SchedulerContext context, CancellationToken cancellationToken = default)
        {
            return new ChangeHistoryToolset<Document, DocumentChangeHistory>(context, cancellationToken);
        }


        /// <summary>
        /// 
        /// This needs to be called before running any version inquiry method from the IVersionTrackedEntity interface.
        ///
        /// It sets up the context and the tenant guid to use.  Provide the context used for the work, and the tenant guid of the user executing the logic.
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tenantGuid"></param>
        public void SetupVersionInquiry(SchedulerContext context, Guid tenantGuid)
        {
            _contextForVersionInquiry = context;
            _tenantGuidForVersionInquiry = tenantGuid;
        }


        /// <summary>
        /// 
        /// Gets meta data and optionally the entity data about the entity's version history using the version of the entity as the basis for the query.
        /// 
        /// Use this to get the update user/time metadata for this version.  IncludingData here is optional and default to false, as it is probably redundant in most cases 
        /// unless the entity you're working with might have unsaved changes.
        /// 
        /// </summary>
        /// <param name="includeData">Whether or not to return the entity data with the results.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<VersionInformation<Document>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)
        {
            return await GetVersionAsync(this.versionNumber, includeData, cancellationToken).ConfigureAwait(false);
        }


        /// <summary>
        /// 
        /// Gets meta data and optionally the entity data about the first version of the entity.  Equivalent to GetVersionAsync(1, includeData), but name is a bit more concise.
        /// 
        /// </summary>
        /// <param name="includeData">Whether or not to return the entity data with the results.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<VersionInformation<Document>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)
        {
            return await GetVersionAsync(1, includeData, cancellationToken).ConfigureAwait(false);
        }


        /// <summary>
        /// 
        /// Gets meta data and optionally the entity data about the version of the entity at the provided point in time.
        /// 
        /// </summary>
        /// <param name="includeData">Whether or not to return the entity data with the results.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<VersionInformation<Document>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)
        {
            if (_contextForVersionInquiry == null || _tenantGuidForVersionInquiry == Guid.Empty)
            {
                throw new Exception("Context for version inquiry is not set.  Please call SetupVersionInquiry() before calling this function.");
            }


            var chts = GetChangeHistoryToolsetForReading(_contextForVersionInquiry, cancellationToken);

            // Get the version for the point in time provided
            AuditEntry versionAudit = await chts.GetAuditForTime(this, pointInTime).ConfigureAwait(false);

            if (versionAudit == null)
            {
                throw new Exception($"No change history found for point in time {pointInTime.ToString("s")} of this Document entity.");
            }

            VersionInformation<Document> version = new VersionInformation<Document>();

            version.versionNumber = versionAudit.versionNumber;

            version.timeStamp = versionAudit.timeStamp;

            if (versionAudit.userId.HasValue == true)
            {
                // Note that this system has multi tenancy enabled but not data visibility, so it gets its change history users from the security module by linking to tenant users.
                version.user = await Foundation.Security.ChangeHistoryMultiTenant.GetChangeHistoryUserAsync(versionAudit.userId.Value, _tenantGuidForVersionInquiry, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // Continency to return a change history user configured to indicate that we don't know the user.
                version.user = new ChangeHistoryUser() { firstName = "Unknown", id = 0, middleName = null, lastName = "User" };
            }

            if (includeData == true)
            {
                version.data = await chts.GetVersionAsync(this, versionAudit.versionNumber).ConfigureAwait(false);
            }

            return version;
        }


        /// <summary>
        /// 
        /// Gets meta data and optionally the entity data about a specific version of the entity.
        /// 
        /// </summary>
        /// <param name="includeData">Whether or not to return the entity data with the results.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<VersionInformation<Document>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)
        {
            if (_contextForVersionInquiry == null || _tenantGuidForVersionInquiry == Guid.Empty)
            {
                throw new Exception("Context for version inquiry is not set.  Please call SetupVersionInquiry() before accessing the GetVersion function.");
            }

            var chts = GetChangeHistoryToolsetForReading(_contextForVersionInquiry, cancellationToken);

            // Get the requested version
            AuditEntry versionAudit = await chts.GetAuditForVersion(this, versionNumber).ConfigureAwait(false);

            if (versionAudit == null)
            {
                throw new Exception($"No change history found for version {versionNumber} of this Document entity.");
            }

            VersionInformation<Document> version = new VersionInformation<Document>();

            version.versionNumber = versionAudit.versionNumber;
            version.timeStamp = versionAudit.timeStamp;

            if (versionAudit.userId.HasValue == true)
            {
                // Note that this system has multi tenancy enabled but not data visibility, so it gets its change history users from the security module by linking to tenant users.
                version.user = await Foundation.Security.ChangeHistoryMultiTenant.GetChangeHistoryUserAsync(versionAudit.userId.Value, _tenantGuidForVersionInquiry, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // Continency to return a change history user configured to indicate that we don't know the user.
                version.user = new ChangeHistoryUser() { firstName = "Unknown", id = 0, middleName = null, lastName = "User" };
            }

            if (includeData == true)
            {
                version.data = await chts.GetVersionAsync(this, versionNumber).ConfigureAwait(false);
            }

            return version;
        }


        /// <summary>
        /// 
        /// This gets all the available meta data version information for this entity, and optionally the entity states too
        /// 
        /// </summary>
        /// <param name="includeData">Whether or not to return the entity data with the results.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<VersionInformation<Document>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)
        {
            if (_contextForVersionInquiry == null || _tenantGuidForVersionInquiry == Guid.Empty)
            {
                throw new Exception("Context for version inquiry is not set.Please call SetupVersionInquiry() before accessing the GetAllVersions function.");
            }

            var chts = GetChangeHistoryToolsetForReading(_contextForVersionInquiry, cancellationToken);

            List<AuditEntry> versionAudits = await chts.GetAuditTrailAsync(this).ConfigureAwait(false);

            if (versionAudits == null)
            {
                throw new Exception($"No change history audits found for this entity.");
            }

            List <VersionInformation<Document>> versions = new List<VersionInformation<Document>>();

            foreach (AuditEntry versionAudit in versionAudits)
            {
                VersionInformation<Document> version = new VersionInformation<Document>();

                version.versionNumber = versionAudit.versionNumber;
                version.timeStamp = versionAudit.timeStamp;

                if (versionAudit.userId.HasValue == true)
                {
                // Note that this system has multi tenancy enabled but not data visibility, so it gets its change history users from the security module by linking to tenant users.
                version.user = await Foundation.Security.ChangeHistoryMultiTenant.GetChangeHistoryUserAsync(versionAudit.userId.Value, _tenantGuidForVersionInquiry, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    // Continency to return a change history user configured to indicate that we don't know the user.
                    version.user = new ChangeHistoryUser() { firstName = "Unknown", id = 0, middleName = null, lastName = "User" };
                }

                if (includeData == true)
                {
                    version.data = await chts.GetVersionAsync(this, versionAudit.versionNumber).ConfigureAwait(false);
                }

                versions.Add(version);
            }

            return versions;
        }


		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class DocumentDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 documentTypeId { get; set; }
			public Int32? invoiceId { get; set; }
			public Int32? receiptId { get; set; }
			[Required]
			public String name { get; set; }
			public String description { get; set; }
			[Required]
			public String fileName { get; set; }
			[Required]
			public String mimeType { get; set; }
			[Required]
			public Int64 fileSizeBytes { get; set; }
			public String fileDataFileName { get; set; }
			public Int64? fileDataSize { get; set; }
			public Byte[] fileDataData { get; set; }
			public String fileDataMimeType { get; set; }
			public Int32? scheduledEventId { get; set; }
			public Int32? financialTransactionId { get; set; }
			public Int32? contactId { get; set; }
			public Int32? resourceId { get; set; }
			public String status { get; set; }
			public DateTime? statusDate { get; set; }
			public String statusChangedBy { get; set; }
			[Required]
			public DateTime uploadedDate { get; set; }
			public String uploadedBy { get; set; }
			public String notes { get; set; }
			public Int32 versionNumber { get; set; }
			[Required]
			public Guid objectGuid { get; set; }
			public Boolean? active { get; set; }
			public Boolean? deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class DocumentOutputDTO : DocumentDTO
		{
			public Contact.ContactDTO contact { get; set; }
			public DocumentType.DocumentTypeDTO documentType { get; set; }
			public FinancialTransaction.FinancialTransactionDTO financialTransaction { get; set; }
			public Invoice.InvoiceDTO invoice { get; set; }
			public Receipt.ReceiptDTO receipt { get; set; }
			public Resource.ResourceDTO resource { get; set; }
			public ScheduledEvent.ScheduledEventDTO scheduledEvent { get; set; }
		}


		/// <summary>
		///
		/// Converts a Document to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public DocumentDTO ToDTO()
		{
			return new DocumentDTO
			{
				id = this.id,
				documentTypeId = this.documentTypeId,
				invoiceId = this.invoiceId,
				receiptId = this.receiptId,
				name = this.name,
				description = this.description,
				fileName = this.fileName,
				mimeType = this.mimeType,
				fileSizeBytes = this.fileSizeBytes,
				fileDataFileName = this.fileDataFileName,
				fileDataSize = this.fileDataSize,
				fileDataData = this.fileDataData,
				fileDataMimeType = this.fileDataMimeType,
				scheduledEventId = this.scheduledEventId,
				financialTransactionId = this.financialTransactionId,
				contactId = this.contactId,
				resourceId = this.resourceId,
				status = this.status,
				statusDate = this.statusDate,
				statusChangedBy = this.statusChangedBy,
				uploadedDate = this.uploadedDate,
				uploadedBy = this.uploadedBy,
				notes = this.notes,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a Document list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<DocumentDTO> ToDTOList(List<Document> data)
		{
			if (data == null)
			{
				return null;
			}

			List<DocumentDTO> output = new List<DocumentDTO>();

			output.Capacity = data.Count;

			foreach (Document document in data)
			{
				output.Add(document.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a Document to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the Document Entity type directly.
		///
		/// </summary>
		public DocumentOutputDTO ToOutputDTO()
		{
			return new DocumentOutputDTO
			{
				id = this.id,
				documentTypeId = this.documentTypeId,
				invoiceId = this.invoiceId,
				receiptId = this.receiptId,
				name = this.name,
				description = this.description,
				fileName = this.fileName,
				mimeType = this.mimeType,
				fileSizeBytes = this.fileSizeBytes,
				fileDataFileName = this.fileDataFileName,
				fileDataSize = this.fileDataSize,
				fileDataData = this.fileDataData,
				fileDataMimeType = this.fileDataMimeType,
				scheduledEventId = this.scheduledEventId,
				financialTransactionId = this.financialTransactionId,
				contactId = this.contactId,
				resourceId = this.resourceId,
				status = this.status,
				statusDate = this.statusDate,
				statusChangedBy = this.statusChangedBy,
				uploadedDate = this.uploadedDate,
				uploadedBy = this.uploadedBy,
				notes = this.notes,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				contact = this.contact?.ToDTO(),
				documentType = this.documentType?.ToDTO(),
				financialTransaction = this.financialTransaction?.ToDTO(),
				invoice = this.invoice?.ToDTO(),
				receipt = this.receipt?.ToDTO(),
				resource = this.resource?.ToDTO(),
				scheduledEvent = this.scheduledEvent?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a Document list to list of Output Data Transfer Object intended to be used for serializing a list of Document objects to avoid using the Document entity type directly.
		///
		/// </summary>
		public static List<DocumentOutputDTO> ToOutputDTOList(List<Document> data)
		{
			if (data == null)
			{
				return null;
			}

			List<DocumentOutputDTO> output = new List<DocumentOutputDTO>();

			output.Capacity = data.Count;

			foreach (Document document in data)
			{
				output.Add(document.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a Document Object.
		///
		/// </summary>
		public static Database.Document FromDTO(DocumentDTO dto)
		{
			return new Database.Document
			{
				id = dto.id,
				documentTypeId = dto.documentTypeId,
				invoiceId = dto.invoiceId,
				receiptId = dto.receiptId,
				name = dto.name,
				description = dto.description,
				fileName = dto.fileName,
				mimeType = dto.mimeType,
				fileSizeBytes = dto.fileSizeBytes,
				fileDataFileName = dto.fileDataFileName,
				fileDataSize = dto.fileDataSize,
				fileDataData = dto.fileDataData,
				fileDataMimeType = dto.fileDataMimeType,
				scheduledEventId = dto.scheduledEventId,
				financialTransactionId = dto.financialTransactionId,
				contactId = dto.contactId,
				resourceId = dto.resourceId,
				status = dto.status,
				statusDate = dto.statusDate,
				statusChangedBy = dto.statusChangedBy,
				uploadedDate = dto.uploadedDate,
				uploadedBy = dto.uploadedBy,
				notes = dto.notes,
				versionNumber = dto.versionNumber,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a Document Object.
		///
		/// </summary>
		public void ApplyDTO(DocumentDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.documentTypeId = dto.documentTypeId;
			this.invoiceId = dto.invoiceId;
			this.receiptId = dto.receiptId;
			this.name = dto.name;
			this.description = dto.description;
			this.fileName = dto.fileName;
			this.mimeType = dto.mimeType;
			this.fileSizeBytes = dto.fileSizeBytes;
			this.fileDataFileName = dto.fileDataFileName;
			this.fileDataSize = dto.fileDataSize;
			this.fileDataData = dto.fileDataData;
			this.fileDataMimeType = dto.fileDataMimeType;
			this.scheduledEventId = dto.scheduledEventId;
			this.financialTransactionId = dto.financialTransactionId;
			this.contactId = dto.contactId;
			this.resourceId = dto.resourceId;
			this.status = dto.status;
			this.statusDate = dto.statusDate;
			this.statusChangedBy = dto.statusChangedBy;
			this.uploadedDate = dto.uploadedDate;
			this.uploadedBy = dto.uploadedBy;
			this.notes = dto.notes;
			this.versionNumber = dto.versionNumber;
			this.objectGuid = dto.objectGuid;
			if (dto.active.HasValue == true)
			{
				this.active = dto.active.Value;
			}
			if (dto.deleted.HasValue == true)
			{
				this.deleted = dto.deleted.Value;
			}
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a Document Object.
		///
		/// </summary>
		public Document Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new Document{
				id = this.id,
				tenantGuid = this.tenantGuid,
				documentTypeId = this.documentTypeId,
				invoiceId = this.invoiceId,
				receiptId = this.receiptId,
				name = this.name,
				description = this.description,
				fileName = this.fileName,
				mimeType = this.mimeType,
				fileSizeBytes = this.fileSizeBytes,
				fileDataFileName = this.fileDataFileName,
				fileDataSize = this.fileDataSize,
				fileDataData = this.fileDataData,
				fileDataMimeType = this.fileDataMimeType,
				scheduledEventId = this.scheduledEventId,
				financialTransactionId = this.financialTransactionId,
				contactId = this.contactId,
				resourceId = this.resourceId,
				status = this.status,
				statusDate = this.statusDate,
				statusChangedBy = this.statusChangedBy,
				uploadedDate = this.uploadedDate,
				uploadedBy = this.uploadedBy,
				notes = this.notes,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Document Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Document Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a Document Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a Document Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.Document document)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (document == null)
			{
				return null;
			}

			return new {
				id = document.id,
				documentTypeId = document.documentTypeId,
				invoiceId = document.invoiceId,
				receiptId = document.receiptId,
				name = document.name,
				description = document.description,
				fileName = document.fileName,
				mimeType = document.mimeType,
				fileSizeBytes = document.fileSizeBytes,
				fileDataFileName = document.fileDataFileName,
				fileDataSize = document.fileDataSize,
				fileDataData = document.fileDataData,
				fileDataMimeType = document.fileDataMimeType,
				scheduledEventId = document.scheduledEventId,
				financialTransactionId = document.financialTransactionId,
				contactId = document.contactId,
				resourceId = document.resourceId,
				status = document.status,
				statusDate = document.statusDate,
				statusChangedBy = document.statusChangedBy,
				uploadedDate = document.uploadedDate,
				uploadedBy = document.uploadedBy,
				notes = document.notes,
				versionNumber = document.versionNumber,
				objectGuid = document.objectGuid,
				active = document.active,
				deleted = document.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a Document Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(Document document)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (document == null)
			{
				return null;
			}

			return new {
				id = document.id,
				documentTypeId = document.documentTypeId,
				invoiceId = document.invoiceId,
				receiptId = document.receiptId,
				name = document.name,
				description = document.description,
				fileName = document.fileName,
				mimeType = document.mimeType,
				fileSizeBytes = document.fileSizeBytes,
				fileDataFileName = document.fileDataFileName,
				fileDataSize = document.fileDataSize,
				fileDataData = document.fileDataData,
				fileDataMimeType = document.fileDataMimeType,
				scheduledEventId = document.scheduledEventId,
				financialTransactionId = document.financialTransactionId,
				contactId = document.contactId,
				resourceId = document.resourceId,
				status = document.status,
				statusDate = document.statusDate,
				statusChangedBy = document.statusChangedBy,
				uploadedDate = document.uploadedDate,
				uploadedBy = document.uploadedBy,
				notes = document.notes,
				versionNumber = document.versionNumber,
				objectGuid = document.objectGuid,
				active = document.active,
				deleted = document.deleted,
				contact = Contact.CreateMinimalAnonymous(document.contact),
				documentType = DocumentType.CreateMinimalAnonymous(document.documentType),
				financialTransaction = FinancialTransaction.CreateMinimalAnonymous(document.financialTransaction),
				invoice = Invoice.CreateMinimalAnonymous(document.invoice),
				receipt = Receipt.CreateMinimalAnonymous(document.receipt),
				resource = Resource.CreateMinimalAnonymous(document.resource),
				scheduledEvent = ScheduledEvent.CreateMinimalAnonymous(document.scheduledEvent)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a Document Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(Document document)
		{
			//
			// Return a very minimal object.
			//
			if (document == null)
			{
				return null;
			}

			return new {
				id = document.id,
				name = document.name,
				description = document.description,
			 };
		}
	}
}
