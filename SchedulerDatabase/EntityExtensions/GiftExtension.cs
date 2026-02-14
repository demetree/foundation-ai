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
	public partial class Gift : IVersionTrackedEntity<Gift>, IAnonymousConvertible
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
        public static ChangeHistoryToolset<Gift, GiftChangeHistory> GetChangeHistoryToolsetForWriting(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return new ChangeHistoryToolset<Gift, GiftChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
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
        public static ChangeHistoryToolset<Gift, GiftChangeHistory> GetChangeHistoryToolsetForReading(SchedulerContext context, CancellationToken cancellationToken = default)
        {
            return new ChangeHistoryToolset<Gift, GiftChangeHistory>(context, cancellationToken);
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
        public async Task<VersionInformation<Gift>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<Gift>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<Gift>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for point in time {pointInTime.ToString("s")} of this Gift entity.");
            }

            VersionInformation<Gift> version = new VersionInformation<Gift>();

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
        public async Task<VersionInformation<Gift>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for version {versionNumber} of this Gift entity.");
            }

            VersionInformation<Gift> version = new VersionInformation<Gift>();

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
        public async Task<List<VersionInformation<Gift>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)
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

            List <VersionInformation<Gift>> versions = new List<VersionInformation<Gift>>();

            foreach (AuditEntry versionAudit in versionAudits)
            {
                VersionInformation<Gift> version = new VersionInformation<Gift>();

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
		public class GiftDTO
		{
			public Int32 id { get; set; }
			public Int32? officeId { get; set; }
			[Required]
			public Int32 constituentId { get; set; }
			public Int32? pledgeId { get; set; }
			[Required]
			public Decimal amount { get; set; }
			[Required]
			public DateTime receivedDate { get; set; }
			public DateTime? postedDate { get; set; }
			[Required]
			public Int32 fundId { get; set; }
			public Int32? campaignId { get; set; }
			public Int32? appealId { get; set; }
			[Required]
			public Int32 paymentTypeId { get; set; }
			public String referenceNumber { get; set; }
			public Int32? batchId { get; set; }
			public Int32? receiptTypeId { get; set; }
			public DateTime? receiptDate { get; set; }
			public Int32? tributeId { get; set; }
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
		public class GiftOutputDTO : GiftDTO
		{
			public Appeal.AppealDTO appeal { get; set; }
			public Batch.BatchDTO batch { get; set; }
			public Campaign.CampaignDTO campaign { get; set; }
			public Constituent.ConstituentDTO constituent { get; set; }
			public Fund.FundDTO fund { get; set; }
			public Office.OfficeDTO office { get; set; }
			public PaymentType.PaymentTypeDTO paymentType { get; set; }
			public Pledge.PledgeDTO pledge { get; set; }
			public ReceiptType.ReceiptTypeDTO receiptType { get; set; }
			public Tribute.TributeDTO tribute { get; set; }
		}


		/// <summary>
		///
		/// Converts a Gift to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public GiftDTO ToDTO()
		{
			return new GiftDTO
			{
				id = this.id,
				officeId = this.officeId,
				constituentId = this.constituentId,
				pledgeId = this.pledgeId,
				amount = this.amount,
				receivedDate = this.receivedDate,
				postedDate = this.postedDate,
				fundId = this.fundId,
				campaignId = this.campaignId,
				appealId = this.appealId,
				paymentTypeId = this.paymentTypeId,
				referenceNumber = this.referenceNumber,
				batchId = this.batchId,
				receiptTypeId = this.receiptTypeId,
				receiptDate = this.receiptDate,
				tributeId = this.tributeId,
				notes = this.notes,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a Gift list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<GiftDTO> ToDTOList(List<Gift> data)
		{
			if (data == null)
			{
				return null;
			}

			List<GiftDTO> output = new List<GiftDTO>();

			output.Capacity = data.Count;

			foreach (Gift gift in data)
			{
				output.Add(gift.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a Gift to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the GiftEntity type directly.
		///
		/// </summary>
		public GiftOutputDTO ToOutputDTO()
		{
			return new GiftOutputDTO
			{
				id = this.id,
				officeId = this.officeId,
				constituentId = this.constituentId,
				pledgeId = this.pledgeId,
				amount = this.amount,
				receivedDate = this.receivedDate,
				postedDate = this.postedDate,
				fundId = this.fundId,
				campaignId = this.campaignId,
				appealId = this.appealId,
				paymentTypeId = this.paymentTypeId,
				referenceNumber = this.referenceNumber,
				batchId = this.batchId,
				receiptTypeId = this.receiptTypeId,
				receiptDate = this.receiptDate,
				tributeId = this.tributeId,
				notes = this.notes,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				appeal = this.appeal?.ToDTO(),
				batch = this.batch?.ToDTO(),
				campaign = this.campaign?.ToDTO(),
				constituent = this.constituent?.ToDTO(),
				fund = this.fund?.ToDTO(),
				office = this.office?.ToDTO(),
				paymentType = this.paymentType?.ToDTO(),
				pledge = this.pledge?.ToDTO(),
				receiptType = this.receiptType?.ToDTO(),
				tribute = this.tribute?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a Gift list to list of Output Data Transfer Object intended to be used for serializing a list of Gift objects to avoid using the Gift entity type directly.
		///
		/// </summary>
		public static List<GiftOutputDTO> ToOutputDTOList(List<Gift> data)
		{
			if (data == null)
			{
				return null;
			}

			List<GiftOutputDTO> output = new List<GiftOutputDTO>();

			output.Capacity = data.Count;

			foreach (Gift gift in data)
			{
				output.Add(gift.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a Gift Object.
		///
		/// </summary>
		public static Database.Gift FromDTO(GiftDTO dto)
		{
			return new Database.Gift
			{
				id = dto.id,
				officeId = dto.officeId,
				constituentId = dto.constituentId,
				pledgeId = dto.pledgeId,
				amount = dto.amount,
				receivedDate = dto.receivedDate,
				postedDate = dto.postedDate,
				fundId = dto.fundId,
				campaignId = dto.campaignId,
				appealId = dto.appealId,
				paymentTypeId = dto.paymentTypeId,
				referenceNumber = dto.referenceNumber,
				batchId = dto.batchId,
				receiptTypeId = dto.receiptTypeId,
				receiptDate = dto.receiptDate,
				tributeId = dto.tributeId,
				notes = dto.notes,
				versionNumber = dto.versionNumber,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a Gift Object.
		///
		/// </summary>
		public void ApplyDTO(GiftDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.officeId = dto.officeId;
			this.constituentId = dto.constituentId;
			this.pledgeId = dto.pledgeId;
			this.amount = dto.amount;
			this.receivedDate = dto.receivedDate;
			this.postedDate = dto.postedDate;
			this.fundId = dto.fundId;
			this.campaignId = dto.campaignId;
			this.appealId = dto.appealId;
			this.paymentTypeId = dto.paymentTypeId;
			this.referenceNumber = dto.referenceNumber;
			this.batchId = dto.batchId;
			this.receiptTypeId = dto.receiptTypeId;
			this.receiptDate = dto.receiptDate;
			this.tributeId = dto.tributeId;
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
		/// Creates a deep copy clone of a Gift Object.
		///
		/// </summary>
		public Gift Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new Gift{
				id = this.id,
				tenantGuid = this.tenantGuid,
				officeId = this.officeId,
				constituentId = this.constituentId,
				pledgeId = this.pledgeId,
				amount = this.amount,
				receivedDate = this.receivedDate,
				postedDate = this.postedDate,
				fundId = this.fundId,
				campaignId = this.campaignId,
				appealId = this.appealId,
				paymentTypeId = this.paymentTypeId,
				referenceNumber = this.referenceNumber,
				batchId = this.batchId,
				receiptTypeId = this.receiptTypeId,
				receiptDate = this.receiptDate,
				tributeId = this.tributeId,
				notes = this.notes,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Gift Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Gift Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a Gift Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a Gift Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.Gift gift)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (gift == null)
			{
				return null;
			}

			return new {
				id = gift.id,
				officeId = gift.officeId,
				constituentId = gift.constituentId,
				pledgeId = gift.pledgeId,
				amount = gift.amount,
				receivedDate = gift.receivedDate,
				postedDate = gift.postedDate,
				fundId = gift.fundId,
				campaignId = gift.campaignId,
				appealId = gift.appealId,
				paymentTypeId = gift.paymentTypeId,
				referenceNumber = gift.referenceNumber,
				batchId = gift.batchId,
				receiptTypeId = gift.receiptTypeId,
				receiptDate = gift.receiptDate,
				tributeId = gift.tributeId,
				notes = gift.notes,
				versionNumber = gift.versionNumber,
				objectGuid = gift.objectGuid,
				active = gift.active,
				deleted = gift.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a Gift Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(Gift gift)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (gift == null)
			{
				return null;
			}

			return new {
				id = gift.id,
				officeId = gift.officeId,
				constituentId = gift.constituentId,
				pledgeId = gift.pledgeId,
				amount = gift.amount,
				receivedDate = gift.receivedDate,
				postedDate = gift.postedDate,
				fundId = gift.fundId,
				campaignId = gift.campaignId,
				appealId = gift.appealId,
				paymentTypeId = gift.paymentTypeId,
				referenceNumber = gift.referenceNumber,
				batchId = gift.batchId,
				receiptTypeId = gift.receiptTypeId,
				receiptDate = gift.receiptDate,
				tributeId = gift.tributeId,
				notes = gift.notes,
				versionNumber = gift.versionNumber,
				objectGuid = gift.objectGuid,
				active = gift.active,
				deleted = gift.deleted,
				appeal = Appeal.CreateMinimalAnonymous(gift.appeal),
				batch = Batch.CreateMinimalAnonymous(gift.batch),
				campaign = Campaign.CreateMinimalAnonymous(gift.campaign),
				constituent = Constituent.CreateMinimalAnonymous(gift.constituent),
				fund = Fund.CreateMinimalAnonymous(gift.fund),
				office = Office.CreateMinimalAnonymous(gift.office),
				paymentType = PaymentType.CreateMinimalAnonymous(gift.paymentType),
				pledge = Pledge.CreateMinimalAnonymous(gift.pledge),
				receiptType = ReceiptType.CreateMinimalAnonymous(gift.receiptType),
				tribute = Tribute.CreateMinimalAnonymous(gift.tribute)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a Gift Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(Gift gift)
		{
			//
			// Return a very minimal object.
			//
			if (gift == null)
			{
				return null;
			}

			return new {
				id = gift.id,
				name = gift.referenceNumber,
				description = string.Join(", ", new[] { gift.referenceNumber}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
