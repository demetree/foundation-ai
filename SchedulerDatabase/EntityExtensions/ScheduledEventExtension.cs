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
	public partial class ScheduledEvent : IVersionTrackedEntity<ScheduledEvent>, IAnonymousConvertible
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
        public static ChangeHistoryToolset<ScheduledEvent, ScheduledEventChangeHistory> GetChangeHistoryToolsetForWriting(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return new ChangeHistoryToolset<ScheduledEvent, ScheduledEventChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
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
        public static Task<ChangeHistoryToolset<ScheduledEvent, ScheduledEventChangeHistory>> GetChangeHistoryToolsetForWritingAsync(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return Task.FromResult(new ChangeHistoryToolset<ScheduledEvent, ScheduledEventChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken));
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
        public static ChangeHistoryToolset<ScheduledEvent, ScheduledEventChangeHistory> GetChangeHistoryToolsetForReading(SchedulerContext context, CancellationToken cancellationToken = default)
        {
            return new ChangeHistoryToolset<ScheduledEvent, ScheduledEventChangeHistory>(context, cancellationToken);
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
        public async Task<VersionInformation<ScheduledEvent>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<ScheduledEvent>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<ScheduledEvent>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for point in time {pointInTime.ToString("s")} of this ScheduledEvent entity.");
            }

            VersionInformation<ScheduledEvent> version = new VersionInformation<ScheduledEvent>();

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
        public async Task<VersionInformation<ScheduledEvent>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for version {versionNumber} of this ScheduledEvent entity.");
            }

            VersionInformation<ScheduledEvent> version = new VersionInformation<ScheduledEvent>();

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
        public async Task<List<VersionInformation<ScheduledEvent>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)
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

            List <VersionInformation<ScheduledEvent>> versions = new List<VersionInformation<ScheduledEvent>>();

            foreach (AuditEntry versionAudit in versionAudits)
            {
                VersionInformation<ScheduledEvent> version = new VersionInformation<ScheduledEvent>();

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
		public class ScheduledEventDTO
		{
			public Int32 id { get; set; }
			public Int32? officeId { get; set; }
			public Int32? clientId { get; set; }
			public Int32? scheduledEventTemplateId { get; set; }
			public Int32? recurrenceRuleId { get; set; }
			public Int32? schedulingTargetId { get; set; }
			public Int32? timeZoneId { get; set; }
			public Int32? parentScheduledEventId { get; set; }
			public DateTime? recurrenceInstanceDate { get; set; }
			[Required]
			public String name { get; set; }
			public String description { get; set; }
			public Boolean? isAllDay { get; set; }
			[Required]
			public DateTime startDateTime { get; set; }
			[Required]
			public DateTime endDateTime { get; set; }
			public String location { get; set; }
			[Required]
			public Int32 eventStatusId { get; set; }
			public Int32? resourceId { get; set; }
			public Int32? crewId { get; set; }
			public Int32? priorityId { get; set; }
			public Int32? bookingSourceTypeId { get; set; }
			public Int32? eventTypeId { get; set; }
			public Int32? partySize { get; set; }
			public String bookingContactName { get; set; }
			public String bookingContactEmail { get; set; }
			public String bookingContactPhone { get; set; }
			public String notes { get; set; }
			public String color { get; set; }
			public String externalId { get; set; }
			public String attributes { get; set; }
			[Required]
			public Boolean isOpenForVolunteers { get; set; }
			public Int32? maxVolunteerSlots { get; set; }
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
		public class ScheduledEventOutputDTO : ScheduledEventDTO
		{
			public BookingSourceType.BookingSourceTypeDTO bookingSourceType { get; set; }
			public Client.ClientDTO client { get; set; }
			public Crew.CrewDTO crew { get; set; }
			public EventStatus.EventStatusDTO eventStatus { get; set; }
			public EventType.EventTypeDTO eventType { get; set; }
			public Office.OfficeDTO office { get; set; }
			public ScheduledEvent.ScheduledEventDTO parentScheduledEvent { get; set; }
			public Priority.PriorityDTO priority { get; set; }
			public RecurrenceRule.RecurrenceRuleDTO recurrenceRule { get; set; }
			public Resource.ResourceDTO resource { get; set; }
			public ScheduledEventTemplate.ScheduledEventTemplateDTO scheduledEventTemplate { get; set; }
			public SchedulingTarget.SchedulingTargetDTO schedulingTarget { get; set; }
			public TimeZone.TimeZoneDTO timeZone { get; set; }
		}


		/// <summary>
		///
		/// Converts a ScheduledEvent to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ScheduledEventDTO ToDTO()
		{
			return new ScheduledEventDTO
			{
				id = this.id,
				officeId = this.officeId,
				clientId = this.clientId,
				scheduledEventTemplateId = this.scheduledEventTemplateId,
				recurrenceRuleId = this.recurrenceRuleId,
				schedulingTargetId = this.schedulingTargetId,
				timeZoneId = this.timeZoneId,
				parentScheduledEventId = this.parentScheduledEventId,
				recurrenceInstanceDate = this.recurrenceInstanceDate,
				name = this.name,
				description = this.description,
				isAllDay = this.isAllDay,
				startDateTime = this.startDateTime,
				endDateTime = this.endDateTime,
				location = this.location,
				eventStatusId = this.eventStatusId,
				resourceId = this.resourceId,
				crewId = this.crewId,
				priorityId = this.priorityId,
				bookingSourceTypeId = this.bookingSourceTypeId,
				eventTypeId = this.eventTypeId,
				partySize = this.partySize,
				bookingContactName = this.bookingContactName,
				bookingContactEmail = this.bookingContactEmail,
				bookingContactPhone = this.bookingContactPhone,
				notes = this.notes,
				color = this.color,
				externalId = this.externalId,
				attributes = this.attributes,
				isOpenForVolunteers = this.isOpenForVolunteers,
				maxVolunteerSlots = this.maxVolunteerSlots,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ScheduledEvent list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ScheduledEventDTO> ToDTOList(List<ScheduledEvent> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ScheduledEventDTO> output = new List<ScheduledEventDTO>();

			output.Capacity = data.Count;

			foreach (ScheduledEvent scheduledEvent in data)
			{
				output.Add(scheduledEvent.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ScheduledEvent to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ScheduledEvent Entity type directly.
		///
		/// </summary>
		public ScheduledEventOutputDTO ToOutputDTO()
		{
			return new ScheduledEventOutputDTO
			{
				id = this.id,
				officeId = this.officeId,
				clientId = this.clientId,
				scheduledEventTemplateId = this.scheduledEventTemplateId,
				recurrenceRuleId = this.recurrenceRuleId,
				schedulingTargetId = this.schedulingTargetId,
				timeZoneId = this.timeZoneId,
				parentScheduledEventId = this.parentScheduledEventId,
				recurrenceInstanceDate = this.recurrenceInstanceDate,
				name = this.name,
				description = this.description,
				isAllDay = this.isAllDay,
				startDateTime = this.startDateTime,
				endDateTime = this.endDateTime,
				location = this.location,
				eventStatusId = this.eventStatusId,
				resourceId = this.resourceId,
				crewId = this.crewId,
				priorityId = this.priorityId,
				bookingSourceTypeId = this.bookingSourceTypeId,
				eventTypeId = this.eventTypeId,
				partySize = this.partySize,
				bookingContactName = this.bookingContactName,
				bookingContactEmail = this.bookingContactEmail,
				bookingContactPhone = this.bookingContactPhone,
				notes = this.notes,
				color = this.color,
				externalId = this.externalId,
				attributes = this.attributes,
				isOpenForVolunteers = this.isOpenForVolunteers,
				maxVolunteerSlots = this.maxVolunteerSlots,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				bookingSourceType = this.bookingSourceType?.ToDTO(),
				client = this.client?.ToDTO(),
				crew = this.crew?.ToDTO(),
				eventStatus = this.eventStatus?.ToDTO(),
				eventType = this.eventType?.ToDTO(),
				office = this.office?.ToDTO(),
				parentScheduledEvent = this.parentScheduledEvent?.ToDTO(),
				priority = this.priority?.ToDTO(),
				recurrenceRule = this.recurrenceRule?.ToDTO(),
				resource = this.resource?.ToDTO(),
				scheduledEventTemplate = this.scheduledEventTemplate?.ToDTO(),
				schedulingTarget = this.schedulingTarget?.ToDTO(),
				timeZone = this.timeZone?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ScheduledEvent list to list of Output Data Transfer Object intended to be used for serializing a list of ScheduledEvent objects to avoid using the ScheduledEvent entity type directly.
		///
		/// </summary>
		public static List<ScheduledEventOutputDTO> ToOutputDTOList(List<ScheduledEvent> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ScheduledEventOutputDTO> output = new List<ScheduledEventOutputDTO>();

			output.Capacity = data.Count;

			foreach (ScheduledEvent scheduledEvent in data)
			{
				output.Add(scheduledEvent.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ScheduledEvent Object.
		///
		/// </summary>
		public static Database.ScheduledEvent FromDTO(ScheduledEventDTO dto)
		{
			return new Database.ScheduledEvent
			{
				id = dto.id,
				officeId = dto.officeId,
				clientId = dto.clientId,
				scheduledEventTemplateId = dto.scheduledEventTemplateId,
				recurrenceRuleId = dto.recurrenceRuleId,
				schedulingTargetId = dto.schedulingTargetId,
				timeZoneId = dto.timeZoneId,
				parentScheduledEventId = dto.parentScheduledEventId,
				recurrenceInstanceDate = dto.recurrenceInstanceDate,
				name = dto.name,
				description = dto.description,
				isAllDay = dto.isAllDay,
				startDateTime = dto.startDateTime,
				endDateTime = dto.endDateTime,
				location = dto.location,
				eventStatusId = dto.eventStatusId,
				resourceId = dto.resourceId,
				crewId = dto.crewId,
				priorityId = dto.priorityId,
				bookingSourceTypeId = dto.bookingSourceTypeId,
				eventTypeId = dto.eventTypeId,
				partySize = dto.partySize,
				bookingContactName = dto.bookingContactName,
				bookingContactEmail = dto.bookingContactEmail,
				bookingContactPhone = dto.bookingContactPhone,
				notes = dto.notes,
				color = dto.color,
				externalId = dto.externalId,
				attributes = dto.attributes,
				isOpenForVolunteers = dto.isOpenForVolunteers,
				maxVolunteerSlots = dto.maxVolunteerSlots,
				versionNumber = dto.versionNumber,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ScheduledEvent Object.
		///
		/// </summary>
		public void ApplyDTO(ScheduledEventDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.officeId = dto.officeId;
			this.clientId = dto.clientId;
			this.scheduledEventTemplateId = dto.scheduledEventTemplateId;
			this.recurrenceRuleId = dto.recurrenceRuleId;
			this.schedulingTargetId = dto.schedulingTargetId;
			this.timeZoneId = dto.timeZoneId;
			this.parentScheduledEventId = dto.parentScheduledEventId;
			this.recurrenceInstanceDate = dto.recurrenceInstanceDate;
			this.name = dto.name;
			this.description = dto.description;
			this.isAllDay = dto.isAllDay;
			this.startDateTime = dto.startDateTime;
			this.endDateTime = dto.endDateTime;
			this.location = dto.location;
			this.eventStatusId = dto.eventStatusId;
			this.resourceId = dto.resourceId;
			this.crewId = dto.crewId;
			this.priorityId = dto.priorityId;
			this.bookingSourceTypeId = dto.bookingSourceTypeId;
			this.eventTypeId = dto.eventTypeId;
			this.partySize = dto.partySize;
			this.bookingContactName = dto.bookingContactName;
			this.bookingContactEmail = dto.bookingContactEmail;
			this.bookingContactPhone = dto.bookingContactPhone;
			this.notes = dto.notes;
			this.color = dto.color;
			this.externalId = dto.externalId;
			this.attributes = dto.attributes;
			this.isOpenForVolunteers = dto.isOpenForVolunteers;
			this.maxVolunteerSlots = dto.maxVolunteerSlots;
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
		/// Creates a deep copy clone of a ScheduledEvent Object.
		///
		/// </summary>
		public ScheduledEvent Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ScheduledEvent{
				id = this.id,
				tenantGuid = this.tenantGuid,
				officeId = this.officeId,
				clientId = this.clientId,
				scheduledEventTemplateId = this.scheduledEventTemplateId,
				recurrenceRuleId = this.recurrenceRuleId,
				schedulingTargetId = this.schedulingTargetId,
				timeZoneId = this.timeZoneId,
				parentScheduledEventId = this.parentScheduledEventId,
				recurrenceInstanceDate = this.recurrenceInstanceDate,
				name = this.name,
				description = this.description,
				isAllDay = this.isAllDay,
				startDateTime = this.startDateTime,
				endDateTime = this.endDateTime,
				location = this.location,
				eventStatusId = this.eventStatusId,
				resourceId = this.resourceId,
				crewId = this.crewId,
				priorityId = this.priorityId,
				bookingSourceTypeId = this.bookingSourceTypeId,
				eventTypeId = this.eventTypeId,
				partySize = this.partySize,
				bookingContactName = this.bookingContactName,
				bookingContactEmail = this.bookingContactEmail,
				bookingContactPhone = this.bookingContactPhone,
				notes = this.notes,
				color = this.color,
				externalId = this.externalId,
				attributes = this.attributes,
				isOpenForVolunteers = this.isOpenForVolunteers,
				maxVolunteerSlots = this.maxVolunteerSlots,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ScheduledEvent Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ScheduledEvent Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ScheduledEvent Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ScheduledEvent Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ScheduledEvent scheduledEvent)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (scheduledEvent == null)
			{
				return null;
			}

			return new {
				id = scheduledEvent.id,
				officeId = scheduledEvent.officeId,
				clientId = scheduledEvent.clientId,
				scheduledEventTemplateId = scheduledEvent.scheduledEventTemplateId,
				recurrenceRuleId = scheduledEvent.recurrenceRuleId,
				schedulingTargetId = scheduledEvent.schedulingTargetId,
				timeZoneId = scheduledEvent.timeZoneId,
				parentScheduledEventId = scheduledEvent.parentScheduledEventId,
				recurrenceInstanceDate = scheduledEvent.recurrenceInstanceDate,
				name = scheduledEvent.name,
				description = scheduledEvent.description,
				isAllDay = scheduledEvent.isAllDay,
				startDateTime = scheduledEvent.startDateTime,
				endDateTime = scheduledEvent.endDateTime,
				location = scheduledEvent.location,
				eventStatusId = scheduledEvent.eventStatusId,
				resourceId = scheduledEvent.resourceId,
				crewId = scheduledEvent.crewId,
				priorityId = scheduledEvent.priorityId,
				bookingSourceTypeId = scheduledEvent.bookingSourceTypeId,
				eventTypeId = scheduledEvent.eventTypeId,
				partySize = scheduledEvent.partySize,
				bookingContactName = scheduledEvent.bookingContactName,
				bookingContactEmail = scheduledEvent.bookingContactEmail,
				bookingContactPhone = scheduledEvent.bookingContactPhone,
				notes = scheduledEvent.notes,
				color = scheduledEvent.color,
				externalId = scheduledEvent.externalId,
				attributes = scheduledEvent.attributes,
				isOpenForVolunteers = scheduledEvent.isOpenForVolunteers,
				maxVolunteerSlots = scheduledEvent.maxVolunteerSlots,
				versionNumber = scheduledEvent.versionNumber,
				objectGuid = scheduledEvent.objectGuid,
				active = scheduledEvent.active,
				deleted = scheduledEvent.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ScheduledEvent Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ScheduledEvent scheduledEvent)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (scheduledEvent == null)
			{
				return null;
			}

			return new {
				id = scheduledEvent.id,
				officeId = scheduledEvent.officeId,
				clientId = scheduledEvent.clientId,
				scheduledEventTemplateId = scheduledEvent.scheduledEventTemplateId,
				recurrenceRuleId = scheduledEvent.recurrenceRuleId,
				schedulingTargetId = scheduledEvent.schedulingTargetId,
				timeZoneId = scheduledEvent.timeZoneId,
				parentScheduledEventId = scheduledEvent.parentScheduledEventId,
				recurrenceInstanceDate = scheduledEvent.recurrenceInstanceDate,
				name = scheduledEvent.name,
				description = scheduledEvent.description,
				isAllDay = scheduledEvent.isAllDay,
				startDateTime = scheduledEvent.startDateTime,
				endDateTime = scheduledEvent.endDateTime,
				location = scheduledEvent.location,
				eventStatusId = scheduledEvent.eventStatusId,
				resourceId = scheduledEvent.resourceId,
				crewId = scheduledEvent.crewId,
				priorityId = scheduledEvent.priorityId,
				bookingSourceTypeId = scheduledEvent.bookingSourceTypeId,
				eventTypeId = scheduledEvent.eventTypeId,
				partySize = scheduledEvent.partySize,
				bookingContactName = scheduledEvent.bookingContactName,
				bookingContactEmail = scheduledEvent.bookingContactEmail,
				bookingContactPhone = scheduledEvent.bookingContactPhone,
				notes = scheduledEvent.notes,
				color = scheduledEvent.color,
				externalId = scheduledEvent.externalId,
				attributes = scheduledEvent.attributes,
				isOpenForVolunteers = scheduledEvent.isOpenForVolunteers,
				maxVolunteerSlots = scheduledEvent.maxVolunteerSlots,
				versionNumber = scheduledEvent.versionNumber,
				objectGuid = scheduledEvent.objectGuid,
				active = scheduledEvent.active,
				deleted = scheduledEvent.deleted,
				bookingSourceType = BookingSourceType.CreateMinimalAnonymous(scheduledEvent.bookingSourceType),
				client = Client.CreateMinimalAnonymous(scheduledEvent.client),
				crew = Crew.CreateMinimalAnonymous(scheduledEvent.crew),
				eventStatus = EventStatus.CreateMinimalAnonymous(scheduledEvent.eventStatus),
				eventType = EventType.CreateMinimalAnonymous(scheduledEvent.eventType),
				office = Office.CreateMinimalAnonymous(scheduledEvent.office),
				parentScheduledEvent = ScheduledEvent.CreateMinimalAnonymous(scheduledEvent.parentScheduledEvent),
				priority = Priority.CreateMinimalAnonymous(scheduledEvent.priority),
				recurrenceRule = RecurrenceRule.CreateMinimalAnonymous(scheduledEvent.recurrenceRule),
				resource = Resource.CreateMinimalAnonymous(scheduledEvent.resource),
				scheduledEventTemplate = ScheduledEventTemplate.CreateMinimalAnonymous(scheduledEvent.scheduledEventTemplate),
				schedulingTarget = SchedulingTarget.CreateMinimalAnonymous(scheduledEvent.schedulingTarget),
				timeZone = TimeZone.CreateMinimalAnonymous(scheduledEvent.timeZone)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ScheduledEvent Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ScheduledEvent scheduledEvent)
		{
			//
			// Return a very minimal object.
			//
			if (scheduledEvent == null)
			{
				return null;
			}

			return new {
				id = scheduledEvent.id,
				name = scheduledEvent.name,
				description = scheduledEvent.description,
			 };
		}
	}
}
