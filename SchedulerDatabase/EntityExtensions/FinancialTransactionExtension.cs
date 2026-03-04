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
	public partial class FinancialTransaction : IVersionTrackedEntity<FinancialTransaction>, IAnonymousConvertible
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
        public static ChangeHistoryToolset<FinancialTransaction, FinancialTransactionChangeHistory> GetChangeHistoryToolsetForWriting(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return new ChangeHistoryToolset<FinancialTransaction, FinancialTransactionChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
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
        public static ChangeHistoryToolset<FinancialTransaction, FinancialTransactionChangeHistory> GetChangeHistoryToolsetForReading(SchedulerContext context, CancellationToken cancellationToken = default)
        {
            return new ChangeHistoryToolset<FinancialTransaction, FinancialTransactionChangeHistory>(context, cancellationToken);
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
        public async Task<VersionInformation<FinancialTransaction>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<FinancialTransaction>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<FinancialTransaction>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for point in time {pointInTime.ToString("s")} of this FinancialTransaction entity.");
            }

            VersionInformation<FinancialTransaction> version = new VersionInformation<FinancialTransaction>();

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
        public async Task<VersionInformation<FinancialTransaction>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for version {versionNumber} of this FinancialTransaction entity.");
            }

            VersionInformation<FinancialTransaction> version = new VersionInformation<FinancialTransaction>();

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
        public async Task<List<VersionInformation<FinancialTransaction>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)
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

            List <VersionInformation<FinancialTransaction>> versions = new List<VersionInformation<FinancialTransaction>>();

            foreach (AuditEntry versionAudit in versionAudits)
            {
                VersionInformation<FinancialTransaction> version = new VersionInformation<FinancialTransaction>();

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
		public class FinancialTransactionDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 financialCategoryId { get; set; }
			public Int32? scheduledEventId { get; set; }
			public Int32? contactId { get; set; }
			[Required]
			public DateTime transactionDate { get; set; }
			[Required]
			public String description { get; set; }
			[Required]
			public Decimal amount { get; set; }
			[Required]
			public Decimal taxAmount { get; set; }
			[Required]
			public Decimal totalAmount { get; set; }
			[Required]
			public Boolean isRevenue { get; set; }
			public String paymentMethod { get; set; }
			public String referenceNumber { get; set; }
			public String notes { get; set; }
			[Required]
			public Int32 currencyId { get; set; }
			public DateTime? exportedDate { get; set; }
			public String externalId { get; set; }
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
		public class FinancialTransactionOutputDTO : FinancialTransactionDTO
		{
			public Contact.ContactDTO contact { get; set; }
			public Currency.CurrencyDTO currency { get; set; }
			public FinancialCategory.FinancialCategoryDTO financialCategory { get; set; }
			public ScheduledEvent.ScheduledEventDTO scheduledEvent { get; set; }
		}


		/// <summary>
		///
		/// Converts a FinancialTransaction to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public FinancialTransactionDTO ToDTO()
		{
			return new FinancialTransactionDTO
			{
				id = this.id,
				financialCategoryId = this.financialCategoryId,
				scheduledEventId = this.scheduledEventId,
				contactId = this.contactId,
				transactionDate = this.transactionDate,
				description = this.description,
				amount = this.amount,
				taxAmount = this.taxAmount,
				totalAmount = this.totalAmount,
				isRevenue = this.isRevenue,
				paymentMethod = this.paymentMethod,
				referenceNumber = this.referenceNumber,
				notes = this.notes,
				currencyId = this.currencyId,
				exportedDate = this.exportedDate,
				externalId = this.externalId,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a FinancialTransaction list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<FinancialTransactionDTO> ToDTOList(List<FinancialTransaction> data)
		{
			if (data == null)
			{
				return null;
			}

			List<FinancialTransactionDTO> output = new List<FinancialTransactionDTO>();

			output.Capacity = data.Count;

			foreach (FinancialTransaction financialTransaction in data)
			{
				output.Add(financialTransaction.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a FinancialTransaction to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the FinancialTransactionEntity type directly.
		///
		/// </summary>
		public FinancialTransactionOutputDTO ToOutputDTO()
		{
			return new FinancialTransactionOutputDTO
			{
				id = this.id,
				financialCategoryId = this.financialCategoryId,
				scheduledEventId = this.scheduledEventId,
				contactId = this.contactId,
				transactionDate = this.transactionDate,
				description = this.description,
				amount = this.amount,
				taxAmount = this.taxAmount,
				totalAmount = this.totalAmount,
				isRevenue = this.isRevenue,
				paymentMethod = this.paymentMethod,
				referenceNumber = this.referenceNumber,
				notes = this.notes,
				currencyId = this.currencyId,
				exportedDate = this.exportedDate,
				externalId = this.externalId,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				contact = this.contact?.ToDTO(),
				currency = this.currency?.ToDTO(),
				financialCategory = this.financialCategory?.ToDTO(),
				scheduledEvent = this.scheduledEvent?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a FinancialTransaction list to list of Output Data Transfer Object intended to be used for serializing a list of FinancialTransaction objects to avoid using the FinancialTransaction entity type directly.
		///
		/// </summary>
		public static List<FinancialTransactionOutputDTO> ToOutputDTOList(List<FinancialTransaction> data)
		{
			if (data == null)
			{
				return null;
			}

			List<FinancialTransactionOutputDTO> output = new List<FinancialTransactionOutputDTO>();

			output.Capacity = data.Count;

			foreach (FinancialTransaction financialTransaction in data)
			{
				output.Add(financialTransaction.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a FinancialTransaction Object.
		///
		/// </summary>
		public static Database.FinancialTransaction FromDTO(FinancialTransactionDTO dto)
		{
			return new Database.FinancialTransaction
			{
				id = dto.id,
				financialCategoryId = dto.financialCategoryId,
				scheduledEventId = dto.scheduledEventId,
				contactId = dto.contactId,
				transactionDate = dto.transactionDate,
				description = dto.description,
				amount = dto.amount,
				taxAmount = dto.taxAmount,
				totalAmount = dto.totalAmount,
				isRevenue = dto.isRevenue,
				paymentMethod = dto.paymentMethod,
				referenceNumber = dto.referenceNumber,
				notes = dto.notes,
				currencyId = dto.currencyId,
				exportedDate = dto.exportedDate,
				externalId = dto.externalId,
				versionNumber = dto.versionNumber,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a FinancialTransaction Object.
		///
		/// </summary>
		public void ApplyDTO(FinancialTransactionDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.financialCategoryId = dto.financialCategoryId;
			this.scheduledEventId = dto.scheduledEventId;
			this.contactId = dto.contactId;
			this.transactionDate = dto.transactionDate;
			this.description = dto.description;
			this.amount = dto.amount;
			this.taxAmount = dto.taxAmount;
			this.totalAmount = dto.totalAmount;
			this.isRevenue = dto.isRevenue;
			this.paymentMethod = dto.paymentMethod;
			this.referenceNumber = dto.referenceNumber;
			this.notes = dto.notes;
			this.currencyId = dto.currencyId;
			this.exportedDate = dto.exportedDate;
			this.externalId = dto.externalId;
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
		/// Creates a deep copy clone of a FinancialTransaction Object.
		///
		/// </summary>
		public FinancialTransaction Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new FinancialTransaction{
				id = this.id,
				tenantGuid = this.tenantGuid,
				financialCategoryId = this.financialCategoryId,
				scheduledEventId = this.scheduledEventId,
				contactId = this.contactId,
				transactionDate = this.transactionDate,
				description = this.description,
				amount = this.amount,
				taxAmount = this.taxAmount,
				totalAmount = this.totalAmount,
				isRevenue = this.isRevenue,
				paymentMethod = this.paymentMethod,
				referenceNumber = this.referenceNumber,
				notes = this.notes,
				currencyId = this.currencyId,
				exportedDate = this.exportedDate,
				externalId = this.externalId,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a FinancialTransaction Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a FinancialTransaction Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a FinancialTransaction Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a FinancialTransaction Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.FinancialTransaction financialTransaction)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (financialTransaction == null)
			{
				return null;
			}

			return new {
				id = financialTransaction.id,
				financialCategoryId = financialTransaction.financialCategoryId,
				scheduledEventId = financialTransaction.scheduledEventId,
				contactId = financialTransaction.contactId,
				transactionDate = financialTransaction.transactionDate,
				description = financialTransaction.description,
				amount = financialTransaction.amount,
				taxAmount = financialTransaction.taxAmount,
				totalAmount = financialTransaction.totalAmount,
				isRevenue = financialTransaction.isRevenue,
				paymentMethod = financialTransaction.paymentMethod,
				referenceNumber = financialTransaction.referenceNumber,
				notes = financialTransaction.notes,
				currencyId = financialTransaction.currencyId,
				exportedDate = financialTransaction.exportedDate,
				externalId = financialTransaction.externalId,
				versionNumber = financialTransaction.versionNumber,
				objectGuid = financialTransaction.objectGuid,
				active = financialTransaction.active,
				deleted = financialTransaction.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a FinancialTransaction Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(FinancialTransaction financialTransaction)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (financialTransaction == null)
			{
				return null;
			}

			return new {
				id = financialTransaction.id,
				financialCategoryId = financialTransaction.financialCategoryId,
				scheduledEventId = financialTransaction.scheduledEventId,
				contactId = financialTransaction.contactId,
				transactionDate = financialTransaction.transactionDate,
				description = financialTransaction.description,
				amount = financialTransaction.amount,
				taxAmount = financialTransaction.taxAmount,
				totalAmount = financialTransaction.totalAmount,
				isRevenue = financialTransaction.isRevenue,
				paymentMethod = financialTransaction.paymentMethod,
				referenceNumber = financialTransaction.referenceNumber,
				notes = financialTransaction.notes,
				currencyId = financialTransaction.currencyId,
				exportedDate = financialTransaction.exportedDate,
				externalId = financialTransaction.externalId,
				versionNumber = financialTransaction.versionNumber,
				objectGuid = financialTransaction.objectGuid,
				active = financialTransaction.active,
				deleted = financialTransaction.deleted,
				contact = Contact.CreateMinimalAnonymous(financialTransaction.contact),
				currency = Currency.CreateMinimalAnonymous(financialTransaction.currency),
				financialCategory = FinancialCategory.CreateMinimalAnonymous(financialTransaction.financialCategory),
				scheduledEvent = ScheduledEvent.CreateMinimalAnonymous(financialTransaction.scheduledEvent)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a FinancialTransaction Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(FinancialTransaction financialTransaction)
		{
			//
			// Return a very minimal object.
			//
			if (financialTransaction == null)
			{
				return null;
			}

			return new {
				id = financialTransaction.id,
				description = financialTransaction.description,
				name = financialTransaction.description
			 };
		}
	}
}
