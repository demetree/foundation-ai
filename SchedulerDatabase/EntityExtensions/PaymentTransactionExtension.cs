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
	public partial class PaymentTransaction : IVersionTrackedEntity<PaymentTransaction>, IAnonymousConvertible
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
        public static ChangeHistoryToolset<PaymentTransaction, PaymentTransactionChangeHistory> GetChangeHistoryToolsetForWriting(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return new ChangeHistoryToolset<PaymentTransaction, PaymentTransactionChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
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
        public static ChangeHistoryToolset<PaymentTransaction, PaymentTransactionChangeHistory> GetChangeHistoryToolsetForReading(SchedulerContext context, CancellationToken cancellationToken = default)
        {
            return new ChangeHistoryToolset<PaymentTransaction, PaymentTransactionChangeHistory>(context, cancellationToken);
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
        public async Task<VersionInformation<PaymentTransaction>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<PaymentTransaction>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<PaymentTransaction>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for point in time {pointInTime.ToString("s")} of this PaymentTransaction entity.");
            }

            VersionInformation<PaymentTransaction> version = new VersionInformation<PaymentTransaction>();

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
        public async Task<VersionInformation<PaymentTransaction>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for version {versionNumber} of this PaymentTransaction entity.");
            }

            VersionInformation<PaymentTransaction> version = new VersionInformation<PaymentTransaction>();

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
        public async Task<List<VersionInformation<PaymentTransaction>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)
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

            List <VersionInformation<PaymentTransaction>> versions = new List<VersionInformation<PaymentTransaction>>();

            foreach (AuditEntry versionAudit in versionAudits)
            {
                VersionInformation<PaymentTransaction> version = new VersionInformation<PaymentTransaction>();

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
		public class PaymentTransactionDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 paymentMethodId { get; set; }
			public Int32? paymentProviderId { get; set; }
			public Int32? scheduledEventId { get; set; }
			public Int32? financialTransactionId { get; set; }
			public Int32? eventChargeId { get; set; }
			[Required]
			public DateTime transactionDate { get; set; }
			[Required]
			public Decimal amount { get; set; }
			[Required]
			public Decimal processingFee { get; set; }
			[Required]
			public Decimal netAmount { get; set; }
			[Required]
			public Int32 currencyId { get; set; }
			[Required]
			public String status { get; set; }
			public String providerTransactionId { get; set; }
			public String providerResponse { get; set; }
			public String payerName { get; set; }
			public String payerEmail { get; set; }
			public String payerPhone { get; set; }
			public String receiptNumber { get; set; }
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
		public class PaymentTransactionOutputDTO : PaymentTransactionDTO
		{
			public Currency.CurrencyDTO currency { get; set; }
			public EventCharge.EventChargeDTO eventCharge { get; set; }
			public FinancialTransaction.FinancialTransactionDTO financialTransaction { get; set; }
			public PaymentMethod.PaymentMethodDTO paymentMethod { get; set; }
			public PaymentProvider.PaymentProviderDTO paymentProvider { get; set; }
			public ScheduledEvent.ScheduledEventDTO scheduledEvent { get; set; }
		}


		/// <summary>
		///
		/// Converts a PaymentTransaction to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public PaymentTransactionDTO ToDTO()
		{
			return new PaymentTransactionDTO
			{
				id = this.id,
				paymentMethodId = this.paymentMethodId,
				paymentProviderId = this.paymentProviderId,
				scheduledEventId = this.scheduledEventId,
				financialTransactionId = this.financialTransactionId,
				eventChargeId = this.eventChargeId,
				transactionDate = this.transactionDate,
				amount = this.amount,
				processingFee = this.processingFee,
				netAmount = this.netAmount,
				currencyId = this.currencyId,
				status = this.status,
				providerTransactionId = this.providerTransactionId,
				providerResponse = this.providerResponse,
				payerName = this.payerName,
				payerEmail = this.payerEmail,
				payerPhone = this.payerPhone,
				receiptNumber = this.receiptNumber,
				notes = this.notes,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a PaymentTransaction list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<PaymentTransactionDTO> ToDTOList(List<PaymentTransaction> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PaymentTransactionDTO> output = new List<PaymentTransactionDTO>();

			output.Capacity = data.Count;

			foreach (PaymentTransaction paymentTransaction in data)
			{
				output.Add(paymentTransaction.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a PaymentTransaction to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the PaymentTransactionEntity type directly.
		///
		/// </summary>
		public PaymentTransactionOutputDTO ToOutputDTO()
		{
			return new PaymentTransactionOutputDTO
			{
				id = this.id,
				paymentMethodId = this.paymentMethodId,
				paymentProviderId = this.paymentProviderId,
				scheduledEventId = this.scheduledEventId,
				financialTransactionId = this.financialTransactionId,
				eventChargeId = this.eventChargeId,
				transactionDate = this.transactionDate,
				amount = this.amount,
				processingFee = this.processingFee,
				netAmount = this.netAmount,
				currencyId = this.currencyId,
				status = this.status,
				providerTransactionId = this.providerTransactionId,
				providerResponse = this.providerResponse,
				payerName = this.payerName,
				payerEmail = this.payerEmail,
				payerPhone = this.payerPhone,
				receiptNumber = this.receiptNumber,
				notes = this.notes,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				currency = this.currency?.ToDTO(),
				eventCharge = this.eventCharge?.ToDTO(),
				financialTransaction = this.financialTransaction?.ToDTO(),
				paymentMethod = this.paymentMethod?.ToDTO(),
				paymentProvider = this.paymentProvider?.ToDTO(),
				scheduledEvent = this.scheduledEvent?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a PaymentTransaction list to list of Output Data Transfer Object intended to be used for serializing a list of PaymentTransaction objects to avoid using the PaymentTransaction entity type directly.
		///
		/// </summary>
		public static List<PaymentTransactionOutputDTO> ToOutputDTOList(List<PaymentTransaction> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PaymentTransactionOutputDTO> output = new List<PaymentTransactionOutputDTO>();

			output.Capacity = data.Count;

			foreach (PaymentTransaction paymentTransaction in data)
			{
				output.Add(paymentTransaction.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a PaymentTransaction Object.
		///
		/// </summary>
		public static Database.PaymentTransaction FromDTO(PaymentTransactionDTO dto)
		{
			return new Database.PaymentTransaction
			{
				id = dto.id,
				paymentMethodId = dto.paymentMethodId,
				paymentProviderId = dto.paymentProviderId,
				scheduledEventId = dto.scheduledEventId,
				financialTransactionId = dto.financialTransactionId,
				eventChargeId = dto.eventChargeId,
				transactionDate = dto.transactionDate,
				amount = dto.amount,
				processingFee = dto.processingFee,
				netAmount = dto.netAmount,
				currencyId = dto.currencyId,
				status = dto.status,
				providerTransactionId = dto.providerTransactionId,
				providerResponse = dto.providerResponse,
				payerName = dto.payerName,
				payerEmail = dto.payerEmail,
				payerPhone = dto.payerPhone,
				receiptNumber = dto.receiptNumber,
				notes = dto.notes,
				versionNumber = dto.versionNumber,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a PaymentTransaction Object.
		///
		/// </summary>
		public void ApplyDTO(PaymentTransactionDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.paymentMethodId = dto.paymentMethodId;
			this.paymentProviderId = dto.paymentProviderId;
			this.scheduledEventId = dto.scheduledEventId;
			this.financialTransactionId = dto.financialTransactionId;
			this.eventChargeId = dto.eventChargeId;
			this.transactionDate = dto.transactionDate;
			this.amount = dto.amount;
			this.processingFee = dto.processingFee;
			this.netAmount = dto.netAmount;
			this.currencyId = dto.currencyId;
			this.status = dto.status;
			this.providerTransactionId = dto.providerTransactionId;
			this.providerResponse = dto.providerResponse;
			this.payerName = dto.payerName;
			this.payerEmail = dto.payerEmail;
			this.payerPhone = dto.payerPhone;
			this.receiptNumber = dto.receiptNumber;
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
		/// Creates a deep copy clone of a PaymentTransaction Object.
		///
		/// </summary>
		public PaymentTransaction Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new PaymentTransaction{
				id = this.id,
				tenantGuid = this.tenantGuid,
				paymentMethodId = this.paymentMethodId,
				paymentProviderId = this.paymentProviderId,
				scheduledEventId = this.scheduledEventId,
				financialTransactionId = this.financialTransactionId,
				eventChargeId = this.eventChargeId,
				transactionDate = this.transactionDate,
				amount = this.amount,
				processingFee = this.processingFee,
				netAmount = this.netAmount,
				currencyId = this.currencyId,
				status = this.status,
				providerTransactionId = this.providerTransactionId,
				providerResponse = this.providerResponse,
				payerName = this.payerName,
				payerEmail = this.payerEmail,
				payerPhone = this.payerPhone,
				receiptNumber = this.receiptNumber,
				notes = this.notes,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PaymentTransaction Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PaymentTransaction Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a PaymentTransaction Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a PaymentTransaction Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.PaymentTransaction paymentTransaction)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (paymentTransaction == null)
			{
				return null;
			}

			return new {
				id = paymentTransaction.id,
				paymentMethodId = paymentTransaction.paymentMethodId,
				paymentProviderId = paymentTransaction.paymentProviderId,
				scheduledEventId = paymentTransaction.scheduledEventId,
				financialTransactionId = paymentTransaction.financialTransactionId,
				eventChargeId = paymentTransaction.eventChargeId,
				transactionDate = paymentTransaction.transactionDate,
				amount = paymentTransaction.amount,
				processingFee = paymentTransaction.processingFee,
				netAmount = paymentTransaction.netAmount,
				currencyId = paymentTransaction.currencyId,
				status = paymentTransaction.status,
				providerTransactionId = paymentTransaction.providerTransactionId,
				providerResponse = paymentTransaction.providerResponse,
				payerName = paymentTransaction.payerName,
				payerEmail = paymentTransaction.payerEmail,
				payerPhone = paymentTransaction.payerPhone,
				receiptNumber = paymentTransaction.receiptNumber,
				notes = paymentTransaction.notes,
				versionNumber = paymentTransaction.versionNumber,
				objectGuid = paymentTransaction.objectGuid,
				active = paymentTransaction.active,
				deleted = paymentTransaction.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a PaymentTransaction Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(PaymentTransaction paymentTransaction)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (paymentTransaction == null)
			{
				return null;
			}

			return new {
				id = paymentTransaction.id,
				paymentMethodId = paymentTransaction.paymentMethodId,
				paymentProviderId = paymentTransaction.paymentProviderId,
				scheduledEventId = paymentTransaction.scheduledEventId,
				financialTransactionId = paymentTransaction.financialTransactionId,
				eventChargeId = paymentTransaction.eventChargeId,
				transactionDate = paymentTransaction.transactionDate,
				amount = paymentTransaction.amount,
				processingFee = paymentTransaction.processingFee,
				netAmount = paymentTransaction.netAmount,
				currencyId = paymentTransaction.currencyId,
				status = paymentTransaction.status,
				providerTransactionId = paymentTransaction.providerTransactionId,
				providerResponse = paymentTransaction.providerResponse,
				payerName = paymentTransaction.payerName,
				payerEmail = paymentTransaction.payerEmail,
				payerPhone = paymentTransaction.payerPhone,
				receiptNumber = paymentTransaction.receiptNumber,
				notes = paymentTransaction.notes,
				versionNumber = paymentTransaction.versionNumber,
				objectGuid = paymentTransaction.objectGuid,
				active = paymentTransaction.active,
				deleted = paymentTransaction.deleted,
				currency = Currency.CreateMinimalAnonymous(paymentTransaction.currency),
				eventCharge = EventCharge.CreateMinimalAnonymous(paymentTransaction.eventCharge),
				financialTransaction = FinancialTransaction.CreateMinimalAnonymous(paymentTransaction.financialTransaction),
				paymentMethod = PaymentMethod.CreateMinimalAnonymous(paymentTransaction.paymentMethod),
				paymentProvider = PaymentProvider.CreateMinimalAnonymous(paymentTransaction.paymentProvider),
				scheduledEvent = ScheduledEvent.CreateMinimalAnonymous(paymentTransaction.scheduledEvent)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a PaymentTransaction Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(PaymentTransaction paymentTransaction)
		{
			//
			// Return a very minimal object.
			//
			if (paymentTransaction == null)
			{
				return null;
			}

			return new {
				id = paymentTransaction.id,
				name = paymentTransaction.status,
				description = string.Join(", ", new[] { paymentTransaction.status, paymentTransaction.providerTransactionId, paymentTransaction.payerName}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
