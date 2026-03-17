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
	public partial class Invoice : IVersionTrackedEntity<Invoice>, IAnonymousConvertible
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
        public static ChangeHistoryToolset<Invoice, InvoiceChangeHistory> GetChangeHistoryToolsetForWriting(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return new ChangeHistoryToolset<Invoice, InvoiceChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
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
        public static Task<ChangeHistoryToolset<Invoice, InvoiceChangeHistory>> GetChangeHistoryToolsetForWritingAsync(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return Task.FromResult(new ChangeHistoryToolset<Invoice, InvoiceChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken));
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
        public static ChangeHistoryToolset<Invoice, InvoiceChangeHistory> GetChangeHistoryToolsetForReading(SchedulerContext context, CancellationToken cancellationToken = default)
        {
            return new ChangeHistoryToolset<Invoice, InvoiceChangeHistory>(context, cancellationToken);
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
        public async Task<VersionInformation<Invoice>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<Invoice>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<Invoice>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for point in time {pointInTime.ToString("s")} of this Invoice entity.");
            }

            VersionInformation<Invoice> version = new VersionInformation<Invoice>();

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
        public async Task<VersionInformation<Invoice>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for version {versionNumber} of this Invoice entity.");
            }

            VersionInformation<Invoice> version = new VersionInformation<Invoice>();

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
        public async Task<List<VersionInformation<Invoice>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)
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

            List <VersionInformation<Invoice>> versions = new List<VersionInformation<Invoice>>();

            foreach (AuditEntry versionAudit in versionAudits)
            {
                VersionInformation<Invoice> version = new VersionInformation<Invoice>();

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
		public class InvoiceDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String invoiceNumber { get; set; }
			[Required]
			public Int32 clientId { get; set; }
			public Int32? contactId { get; set; }
			public Int32? scheduledEventId { get; set; }
			public Int32? financialOfficeId { get; set; }
			[Required]
			public Int32 invoiceStatusId { get; set; }
			[Required]
			public Int32 currencyId { get; set; }
			public Int32? taxCodeId { get; set; }
			[Required]
			public DateTime invoiceDate { get; set; }
			[Required]
			public DateTime dueDate { get; set; }
			[Required]
			public Decimal subtotal { get; set; }
			[Required]
			public Decimal taxAmount { get; set; }
			[Required]
			public Decimal totalAmount { get; set; }
			[Required]
			public Decimal amountPaid { get; set; }
			public DateTime? sentDate { get; set; }
			public DateTime? paidDate { get; set; }
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
		public class InvoiceOutputDTO : InvoiceDTO
		{
			public Client.ClientDTO client { get; set; }
			public Contact.ContactDTO contact { get; set; }
			public Currency.CurrencyDTO currency { get; set; }
			public FinancialOffice.FinancialOfficeDTO financialOffice { get; set; }
			public InvoiceStatus.InvoiceStatusDTO invoiceStatus { get; set; }
			public ScheduledEvent.ScheduledEventDTO scheduledEvent { get; set; }
			public TaxCode.TaxCodeDTO taxCode { get; set; }
		}


		/// <summary>
		///
		/// Converts a Invoice to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public InvoiceDTO ToDTO()
		{
			return new InvoiceDTO
			{
				id = this.id,
				invoiceNumber = this.invoiceNumber,
				clientId = this.clientId,
				contactId = this.contactId,
				scheduledEventId = this.scheduledEventId,
				financialOfficeId = this.financialOfficeId,
				invoiceStatusId = this.invoiceStatusId,
				currencyId = this.currencyId,
				taxCodeId = this.taxCodeId,
				invoiceDate = this.invoiceDate,
				dueDate = this.dueDate,
				subtotal = this.subtotal,
				taxAmount = this.taxAmount,
				totalAmount = this.totalAmount,
				amountPaid = this.amountPaid,
				sentDate = this.sentDate,
				paidDate = this.paidDate,
				notes = this.notes,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a Invoice list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<InvoiceDTO> ToDTOList(List<Invoice> data)
		{
			if (data == null)
			{
				return null;
			}

			List<InvoiceDTO> output = new List<InvoiceDTO>();

			output.Capacity = data.Count;

			foreach (Invoice invoice in data)
			{
				output.Add(invoice.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a Invoice to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the Invoice Entity type directly.
		///
		/// </summary>
		public InvoiceOutputDTO ToOutputDTO()
		{
			return new InvoiceOutputDTO
			{
				id = this.id,
				invoiceNumber = this.invoiceNumber,
				clientId = this.clientId,
				contactId = this.contactId,
				scheduledEventId = this.scheduledEventId,
				financialOfficeId = this.financialOfficeId,
				invoiceStatusId = this.invoiceStatusId,
				currencyId = this.currencyId,
				taxCodeId = this.taxCodeId,
				invoiceDate = this.invoiceDate,
				dueDate = this.dueDate,
				subtotal = this.subtotal,
				taxAmount = this.taxAmount,
				totalAmount = this.totalAmount,
				amountPaid = this.amountPaid,
				sentDate = this.sentDate,
				paidDate = this.paidDate,
				notes = this.notes,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				client = this.client?.ToDTO(),
				contact = this.contact?.ToDTO(),
				currency = this.currency?.ToDTO(),
				financialOffice = this.financialOffice?.ToDTO(),
				invoiceStatus = this.invoiceStatus?.ToDTO(),
				scheduledEvent = this.scheduledEvent?.ToDTO(),
				taxCode = this.taxCode?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a Invoice list to list of Output Data Transfer Object intended to be used for serializing a list of Invoice objects to avoid using the Invoice entity type directly.
		///
		/// </summary>
		public static List<InvoiceOutputDTO> ToOutputDTOList(List<Invoice> data)
		{
			if (data == null)
			{
				return null;
			}

			List<InvoiceOutputDTO> output = new List<InvoiceOutputDTO>();

			output.Capacity = data.Count;

			foreach (Invoice invoice in data)
			{
				output.Add(invoice.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a Invoice Object.
		///
		/// </summary>
		public static Database.Invoice FromDTO(InvoiceDTO dto)
		{
			return new Database.Invoice
			{
				id = dto.id,
				invoiceNumber = dto.invoiceNumber,
				clientId = dto.clientId,
				contactId = dto.contactId,
				scheduledEventId = dto.scheduledEventId,
				financialOfficeId = dto.financialOfficeId,
				invoiceStatusId = dto.invoiceStatusId,
				currencyId = dto.currencyId,
				taxCodeId = dto.taxCodeId,
				invoiceDate = dto.invoiceDate,
				dueDate = dto.dueDate,
				subtotal = dto.subtotal,
				taxAmount = dto.taxAmount,
				totalAmount = dto.totalAmount,
				amountPaid = dto.amountPaid,
				sentDate = dto.sentDate,
				paidDate = dto.paidDate,
				notes = dto.notes,
				versionNumber = dto.versionNumber,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a Invoice Object.
		///
		/// </summary>
		public void ApplyDTO(InvoiceDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.invoiceNumber = dto.invoiceNumber;
			this.clientId = dto.clientId;
			this.contactId = dto.contactId;
			this.scheduledEventId = dto.scheduledEventId;
			this.financialOfficeId = dto.financialOfficeId;
			this.invoiceStatusId = dto.invoiceStatusId;
			this.currencyId = dto.currencyId;
			this.taxCodeId = dto.taxCodeId;
			this.invoiceDate = dto.invoiceDate;
			this.dueDate = dto.dueDate;
			this.subtotal = dto.subtotal;
			this.taxAmount = dto.taxAmount;
			this.totalAmount = dto.totalAmount;
			this.amountPaid = dto.amountPaid;
			this.sentDate = dto.sentDate;
			this.paidDate = dto.paidDate;
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
		/// Creates a deep copy clone of a Invoice Object.
		///
		/// </summary>
		public Invoice Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new Invoice{
				id = this.id,
				tenantGuid = this.tenantGuid,
				invoiceNumber = this.invoiceNumber,
				clientId = this.clientId,
				contactId = this.contactId,
				scheduledEventId = this.scheduledEventId,
				financialOfficeId = this.financialOfficeId,
				invoiceStatusId = this.invoiceStatusId,
				currencyId = this.currencyId,
				taxCodeId = this.taxCodeId,
				invoiceDate = this.invoiceDate,
				dueDate = this.dueDate,
				subtotal = this.subtotal,
				taxAmount = this.taxAmount,
				totalAmount = this.totalAmount,
				amountPaid = this.amountPaid,
				sentDate = this.sentDate,
				paidDate = this.paidDate,
				notes = this.notes,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Invoice Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Invoice Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a Invoice Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a Invoice Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.Invoice invoice)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (invoice == null)
			{
				return null;
			}

			return new {
				id = invoice.id,
				invoiceNumber = invoice.invoiceNumber,
				clientId = invoice.clientId,
				contactId = invoice.contactId,
				scheduledEventId = invoice.scheduledEventId,
				financialOfficeId = invoice.financialOfficeId,
				invoiceStatusId = invoice.invoiceStatusId,
				currencyId = invoice.currencyId,
				taxCodeId = invoice.taxCodeId,
				invoiceDate = invoice.invoiceDate,
				dueDate = invoice.dueDate,
				subtotal = invoice.subtotal,
				taxAmount = invoice.taxAmount,
				totalAmount = invoice.totalAmount,
				amountPaid = invoice.amountPaid,
				sentDate = invoice.sentDate,
				paidDate = invoice.paidDate,
				notes = invoice.notes,
				versionNumber = invoice.versionNumber,
				objectGuid = invoice.objectGuid,
				active = invoice.active,
				deleted = invoice.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a Invoice Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(Invoice invoice)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (invoice == null)
			{
				return null;
			}

			return new {
				id = invoice.id,
				invoiceNumber = invoice.invoiceNumber,
				clientId = invoice.clientId,
				contactId = invoice.contactId,
				scheduledEventId = invoice.scheduledEventId,
				financialOfficeId = invoice.financialOfficeId,
				invoiceStatusId = invoice.invoiceStatusId,
				currencyId = invoice.currencyId,
				taxCodeId = invoice.taxCodeId,
				invoiceDate = invoice.invoiceDate,
				dueDate = invoice.dueDate,
				subtotal = invoice.subtotal,
				taxAmount = invoice.taxAmount,
				totalAmount = invoice.totalAmount,
				amountPaid = invoice.amountPaid,
				sentDate = invoice.sentDate,
				paidDate = invoice.paidDate,
				notes = invoice.notes,
				versionNumber = invoice.versionNumber,
				objectGuid = invoice.objectGuid,
				active = invoice.active,
				deleted = invoice.deleted,
				client = Client.CreateMinimalAnonymous(invoice.client),
				contact = Contact.CreateMinimalAnonymous(invoice.contact),
				currency = Currency.CreateMinimalAnonymous(invoice.currency),
				financialOffice = FinancialOffice.CreateMinimalAnonymous(invoice.financialOffice),
				invoiceStatus = InvoiceStatus.CreateMinimalAnonymous(invoice.invoiceStatus),
				scheduledEvent = ScheduledEvent.CreateMinimalAnonymous(invoice.scheduledEvent),
				taxCode = TaxCode.CreateMinimalAnonymous(invoice.taxCode)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a Invoice Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(Invoice invoice)
		{
			//
			// Return a very minimal object.
			//
			if (invoice == null)
			{
				return null;
			}

			return new {
				id = invoice.id,
				name = string.Join(", ", new[] { invoice.invoiceNumber}.Where(s => !string.IsNullOrWhiteSpace(s))),
				description = string.Join(", ", new[] { invoice.invoiceNumber}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
