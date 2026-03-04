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
	public partial class EventCharge : IVersionTrackedEntity<EventCharge>, IAnonymousConvertible
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
        public static ChangeHistoryToolset<EventCharge, EventChargeChangeHistory> GetChangeHistoryToolsetForWriting(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return new ChangeHistoryToolset<EventCharge, EventChargeChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
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
        public static ChangeHistoryToolset<EventCharge, EventChargeChangeHistory> GetChangeHistoryToolsetForReading(SchedulerContext context, CancellationToken cancellationToken = default)
        {
            return new ChangeHistoryToolset<EventCharge, EventChargeChangeHistory>(context, cancellationToken);
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
        public async Task<VersionInformation<EventCharge>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<EventCharge>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<EventCharge>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for point in time {pointInTime.ToString("s")} of this EventCharge entity.");
            }

            VersionInformation<EventCharge> version = new VersionInformation<EventCharge>();

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
        public async Task<VersionInformation<EventCharge>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for version {versionNumber} of this EventCharge entity.");
            }

            VersionInformation<EventCharge> version = new VersionInformation<EventCharge>();

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
        public async Task<List<VersionInformation<EventCharge>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)
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

            List <VersionInformation<EventCharge>> versions = new List<VersionInformation<EventCharge>>();

            foreach (AuditEntry versionAudit in versionAudits)
            {
                VersionInformation<EventCharge> version = new VersionInformation<EventCharge>();

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
		public class EventChargeDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 scheduledEventId { get; set; }
			public Int32? resourceId { get; set; }
			[Required]
			public Int32 chargeTypeId { get; set; }
			[Required]
			public Int32 chargeStatusId { get; set; }
			public Decimal? quantity { get; set; }
			public Decimal? unitPrice { get; set; }
			[Required]
			public Decimal extendedAmount { get; set; }
			[Required]
			public Decimal taxAmount { get; set; }
			[Required]
			public Int32 currencyId { get; set; }
			public Int32? rateTypeId { get; set; }
			public String notes { get; set; }
			[Required]
			public Boolean isAutomatic { get; set; }
			[Required]
			public Boolean isDeposit { get; set; }
			public DateTime? depositRefundedDate { get; set; }
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
		public class EventChargeOutputDTO : EventChargeDTO
		{
			public ChargeStatus.ChargeStatusDTO chargeStatus { get; set; }
			public ChargeType.ChargeTypeDTO chargeType { get; set; }
			public Currency.CurrencyDTO currency { get; set; }
			public RateType.RateTypeDTO rateType { get; set; }
			public Resource.ResourceDTO resource { get; set; }
			public ScheduledEvent.ScheduledEventDTO scheduledEvent { get; set; }
		}


		/// <summary>
		///
		/// Converts a EventCharge to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public EventChargeDTO ToDTO()
		{
			return new EventChargeDTO
			{
				id = this.id,
				scheduledEventId = this.scheduledEventId,
				resourceId = this.resourceId,
				chargeTypeId = this.chargeTypeId,
				chargeStatusId = this.chargeStatusId,
				quantity = this.quantity,
				unitPrice = this.unitPrice,
				extendedAmount = this.extendedAmount,
				taxAmount = this.taxAmount,
				currencyId = this.currencyId,
				rateTypeId = this.rateTypeId,
				notes = this.notes,
				isAutomatic = this.isAutomatic,
				isDeposit = this.isDeposit,
				depositRefundedDate = this.depositRefundedDate,
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
		/// Converts a EventCharge list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<EventChargeDTO> ToDTOList(List<EventCharge> data)
		{
			if (data == null)
			{
				return null;
			}

			List<EventChargeDTO> output = new List<EventChargeDTO>();

			output.Capacity = data.Count;

			foreach (EventCharge eventCharge in data)
			{
				output.Add(eventCharge.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a EventCharge to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the EventChargeEntity type directly.
		///
		/// </summary>
		public EventChargeOutputDTO ToOutputDTO()
		{
			return new EventChargeOutputDTO
			{
				id = this.id,
				scheduledEventId = this.scheduledEventId,
				resourceId = this.resourceId,
				chargeTypeId = this.chargeTypeId,
				chargeStatusId = this.chargeStatusId,
				quantity = this.quantity,
				unitPrice = this.unitPrice,
				extendedAmount = this.extendedAmount,
				taxAmount = this.taxAmount,
				currencyId = this.currencyId,
				rateTypeId = this.rateTypeId,
				notes = this.notes,
				isAutomatic = this.isAutomatic,
				isDeposit = this.isDeposit,
				depositRefundedDate = this.depositRefundedDate,
				exportedDate = this.exportedDate,
				externalId = this.externalId,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				chargeStatus = this.chargeStatus?.ToDTO(),
				chargeType = this.chargeType?.ToDTO(),
				currency = this.currency?.ToDTO(),
				rateType = this.rateType?.ToDTO(),
				resource = this.resource?.ToDTO(),
				scheduledEvent = this.scheduledEvent?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a EventCharge list to list of Output Data Transfer Object intended to be used for serializing a list of EventCharge objects to avoid using the EventCharge entity type directly.
		///
		/// </summary>
		public static List<EventChargeOutputDTO> ToOutputDTOList(List<EventCharge> data)
		{
			if (data == null)
			{
				return null;
			}

			List<EventChargeOutputDTO> output = new List<EventChargeOutputDTO>();

			output.Capacity = data.Count;

			foreach (EventCharge eventCharge in data)
			{
				output.Add(eventCharge.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a EventCharge Object.
		///
		/// </summary>
		public static Database.EventCharge FromDTO(EventChargeDTO dto)
		{
			return new Database.EventCharge
			{
				id = dto.id,
				scheduledEventId = dto.scheduledEventId,
				resourceId = dto.resourceId,
				chargeTypeId = dto.chargeTypeId,
				chargeStatusId = dto.chargeStatusId,
				quantity = dto.quantity,
				unitPrice = dto.unitPrice,
				extendedAmount = dto.extendedAmount,
				taxAmount = dto.taxAmount,
				currencyId = dto.currencyId,
				rateTypeId = dto.rateTypeId,
				notes = dto.notes,
				isAutomatic = dto.isAutomatic,
				isDeposit = dto.isDeposit,
				depositRefundedDate = dto.depositRefundedDate,
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
		/// Applies the values from an INPUT DTO to a EventCharge Object.
		///
		/// </summary>
		public void ApplyDTO(EventChargeDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.scheduledEventId = dto.scheduledEventId;
			this.resourceId = dto.resourceId;
			this.chargeTypeId = dto.chargeTypeId;
			this.chargeStatusId = dto.chargeStatusId;
			this.quantity = dto.quantity;
			this.unitPrice = dto.unitPrice;
			this.extendedAmount = dto.extendedAmount;
			this.taxAmount = dto.taxAmount;
			this.currencyId = dto.currencyId;
			this.rateTypeId = dto.rateTypeId;
			this.notes = dto.notes;
			this.isAutomatic = dto.isAutomatic;
			this.isDeposit = dto.isDeposit;
			this.depositRefundedDate = dto.depositRefundedDate;
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
		/// Creates a deep copy clone of a EventCharge Object.
		///
		/// </summary>
		public EventCharge Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new EventCharge{
				id = this.id,
				tenantGuid = this.tenantGuid,
				scheduledEventId = this.scheduledEventId,
				resourceId = this.resourceId,
				chargeTypeId = this.chargeTypeId,
				chargeStatusId = this.chargeStatusId,
				quantity = this.quantity,
				unitPrice = this.unitPrice,
				extendedAmount = this.extendedAmount,
				taxAmount = this.taxAmount,
				currencyId = this.currencyId,
				rateTypeId = this.rateTypeId,
				notes = this.notes,
				isAutomatic = this.isAutomatic,
				isDeposit = this.isDeposit,
				depositRefundedDate = this.depositRefundedDate,
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
        /// Creates an anonymous object containing properties from a EventCharge Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a EventCharge Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a EventCharge Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a EventCharge Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.EventCharge eventCharge)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (eventCharge == null)
			{
				return null;
			}

			return new {
				id = eventCharge.id,
				scheduledEventId = eventCharge.scheduledEventId,
				resourceId = eventCharge.resourceId,
				chargeTypeId = eventCharge.chargeTypeId,
				chargeStatusId = eventCharge.chargeStatusId,
				quantity = eventCharge.quantity,
				unitPrice = eventCharge.unitPrice,
				extendedAmount = eventCharge.extendedAmount,
				taxAmount = eventCharge.taxAmount,
				currencyId = eventCharge.currencyId,
				rateTypeId = eventCharge.rateTypeId,
				notes = eventCharge.notes,
				isAutomatic = eventCharge.isAutomatic,
				isDeposit = eventCharge.isDeposit,
				depositRefundedDate = eventCharge.depositRefundedDate,
				exportedDate = eventCharge.exportedDate,
				externalId = eventCharge.externalId,
				versionNumber = eventCharge.versionNumber,
				objectGuid = eventCharge.objectGuid,
				active = eventCharge.active,
				deleted = eventCharge.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a EventCharge Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(EventCharge eventCharge)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (eventCharge == null)
			{
				return null;
			}

			return new {
				id = eventCharge.id,
				scheduledEventId = eventCharge.scheduledEventId,
				resourceId = eventCharge.resourceId,
				chargeTypeId = eventCharge.chargeTypeId,
				chargeStatusId = eventCharge.chargeStatusId,
				quantity = eventCharge.quantity,
				unitPrice = eventCharge.unitPrice,
				extendedAmount = eventCharge.extendedAmount,
				taxAmount = eventCharge.taxAmount,
				currencyId = eventCharge.currencyId,
				rateTypeId = eventCharge.rateTypeId,
				notes = eventCharge.notes,
				isAutomatic = eventCharge.isAutomatic,
				isDeposit = eventCharge.isDeposit,
				depositRefundedDate = eventCharge.depositRefundedDate,
				exportedDate = eventCharge.exportedDate,
				externalId = eventCharge.externalId,
				versionNumber = eventCharge.versionNumber,
				objectGuid = eventCharge.objectGuid,
				active = eventCharge.active,
				deleted = eventCharge.deleted,
				chargeStatus = ChargeStatus.CreateMinimalAnonymous(eventCharge.chargeStatus),
				chargeType = ChargeType.CreateMinimalAnonymous(eventCharge.chargeType),
				currency = Currency.CreateMinimalAnonymous(eventCharge.currency),
				rateType = RateType.CreateMinimalAnonymous(eventCharge.rateType),
				resource = Resource.CreateMinimalAnonymous(eventCharge.resource),
				scheduledEvent = ScheduledEvent.CreateMinimalAnonymous(eventCharge.scheduledEvent)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a EventCharge Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(EventCharge eventCharge)
		{
			//
			// Return a very minimal object.
			//
			if (eventCharge == null)
			{
				return null;
			}

			return new {
				id = eventCharge.id,
				name = eventCharge.externalId,
				description = string.Join(", ", new[] { eventCharge.externalId}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
