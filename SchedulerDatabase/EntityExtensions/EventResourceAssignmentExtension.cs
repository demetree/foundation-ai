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
	public partial class EventResourceAssignment : IVersionTrackedEntity<EventResourceAssignment>, IAnonymousConvertible
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
        public static ChangeHistoryToolset<EventResourceAssignment, EventResourceAssignmentChangeHistory> GetChangeHistoryToolsetForWriting(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return new ChangeHistoryToolset<EventResourceAssignment, EventResourceAssignmentChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
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
        public static Task<ChangeHistoryToolset<EventResourceAssignment, EventResourceAssignmentChangeHistory>> GetChangeHistoryToolsetForWritingAsync(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return Task.FromResult(new ChangeHistoryToolset<EventResourceAssignment, EventResourceAssignmentChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken));
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
        public static ChangeHistoryToolset<EventResourceAssignment, EventResourceAssignmentChangeHistory> GetChangeHistoryToolsetForReading(SchedulerContext context, CancellationToken cancellationToken = default)
        {
            return new ChangeHistoryToolset<EventResourceAssignment, EventResourceAssignmentChangeHistory>(context, cancellationToken);
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
        public async Task<VersionInformation<EventResourceAssignment>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<EventResourceAssignment>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<EventResourceAssignment>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for point in time {pointInTime.ToString("s")} of this EventResourceAssignment entity.");
            }

            VersionInformation<EventResourceAssignment> version = new VersionInformation<EventResourceAssignment>();

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
        public async Task<VersionInformation<EventResourceAssignment>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for version {versionNumber} of this EventResourceAssignment entity.");
            }

            VersionInformation<EventResourceAssignment> version = new VersionInformation<EventResourceAssignment>();

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
        public async Task<List<VersionInformation<EventResourceAssignment>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)
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

            List <VersionInformation<EventResourceAssignment>> versions = new List<VersionInformation<EventResourceAssignment>>();

            foreach (AuditEntry versionAudit in versionAudits)
            {
                VersionInformation<EventResourceAssignment> version = new VersionInformation<EventResourceAssignment>();

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
		public class EventResourceAssignmentDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 scheduledEventId { get; set; }
			public Int32? officeId { get; set; }
			public Int32? resourceId { get; set; }
			public Int32? crewId { get; set; }
			public Int32? volunteerGroupId { get; set; }
			public Int32? assignmentRoleId { get; set; }
			[Required]
			public Int32 assignmentStatusId { get; set; }
			public DateTime? assignmentStartDateTime { get; set; }
			public DateTime? assignmentEndDateTime { get; set; }
			public String notes { get; set; }
			public Boolean? isTravelRequired { get; set; }
			public Int32? travelDurationMinutes { get; set; }
			public Single? distanceKilometers { get; set; }
			public String startLocation { get; set; }
			public DateTime? actualStartDateTime { get; set; }
			public DateTime? actualEndDateTime { get; set; }
			public String actualNotes { get; set; }
			[Required]
			public Boolean isVolunteer { get; set; }
			public Single? reportedVolunteerHours { get; set; }
			public Single? approvedVolunteerHours { get; set; }
			public Int32? hoursApprovedByContactId { get; set; }
			public DateTime? approvedDateTime { get; set; }
			public Decimal? reimbursementAmount { get; set; }
			public Int32? chargeTypeId { get; set; }
			[Required]
			public Boolean reimbursementRequested { get; set; }
			public String volunteerNotes { get; set; }
			public DateTime? reminderSentDateTime { get; set; }
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
		public class EventResourceAssignmentOutputDTO : EventResourceAssignmentDTO
		{
			public AssignmentRole.AssignmentRoleDTO assignmentRole { get; set; }
			public AssignmentStatus.AssignmentStatusDTO assignmentStatus { get; set; }
			public ChargeType.ChargeTypeDTO chargeType { get; set; }
			public Crew.CrewDTO crew { get; set; }
			public Contact.ContactDTO hoursApprovedByContact { get; set; }
			public Office.OfficeDTO office { get; set; }
			public Resource.ResourceDTO resource { get; set; }
			public ScheduledEvent.ScheduledEventDTO scheduledEvent { get; set; }
			public VolunteerGroup.VolunteerGroupDTO volunteerGroup { get; set; }
		}


		/// <summary>
		///
		/// Converts a EventResourceAssignment to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public EventResourceAssignmentDTO ToDTO()
		{
			return new EventResourceAssignmentDTO
			{
				id = this.id,
				scheduledEventId = this.scheduledEventId,
				officeId = this.officeId,
				resourceId = this.resourceId,
				crewId = this.crewId,
				volunteerGroupId = this.volunteerGroupId,
				assignmentRoleId = this.assignmentRoleId,
				assignmentStatusId = this.assignmentStatusId,
				assignmentStartDateTime = this.assignmentStartDateTime,
				assignmentEndDateTime = this.assignmentEndDateTime,
				notes = this.notes,
				isTravelRequired = this.isTravelRequired,
				travelDurationMinutes = this.travelDurationMinutes,
				distanceKilometers = this.distanceKilometers,
				startLocation = this.startLocation,
				actualStartDateTime = this.actualStartDateTime,
				actualEndDateTime = this.actualEndDateTime,
				actualNotes = this.actualNotes,
				isVolunteer = this.isVolunteer,
				reportedVolunteerHours = this.reportedVolunteerHours,
				approvedVolunteerHours = this.approvedVolunteerHours,
				hoursApprovedByContactId = this.hoursApprovedByContactId,
				approvedDateTime = this.approvedDateTime,
				reimbursementAmount = this.reimbursementAmount,
				chargeTypeId = this.chargeTypeId,
				reimbursementRequested = this.reimbursementRequested,
				volunteerNotes = this.volunteerNotes,
				reminderSentDateTime = this.reminderSentDateTime,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a EventResourceAssignment list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<EventResourceAssignmentDTO> ToDTOList(List<EventResourceAssignment> data)
		{
			if (data == null)
			{
				return null;
			}

			List<EventResourceAssignmentDTO> output = new List<EventResourceAssignmentDTO>();

			output.Capacity = data.Count;

			foreach (EventResourceAssignment eventResourceAssignment in data)
			{
				output.Add(eventResourceAssignment.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a EventResourceAssignment to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the EventResourceAssignment Entity type directly.
		///
		/// </summary>
		public EventResourceAssignmentOutputDTO ToOutputDTO()
		{
			return new EventResourceAssignmentOutputDTO
			{
				id = this.id,
				scheduledEventId = this.scheduledEventId,
				officeId = this.officeId,
				resourceId = this.resourceId,
				crewId = this.crewId,
				volunteerGroupId = this.volunteerGroupId,
				assignmentRoleId = this.assignmentRoleId,
				assignmentStatusId = this.assignmentStatusId,
				assignmentStartDateTime = this.assignmentStartDateTime,
				assignmentEndDateTime = this.assignmentEndDateTime,
				notes = this.notes,
				isTravelRequired = this.isTravelRequired,
				travelDurationMinutes = this.travelDurationMinutes,
				distanceKilometers = this.distanceKilometers,
				startLocation = this.startLocation,
				actualStartDateTime = this.actualStartDateTime,
				actualEndDateTime = this.actualEndDateTime,
				actualNotes = this.actualNotes,
				isVolunteer = this.isVolunteer,
				reportedVolunteerHours = this.reportedVolunteerHours,
				approvedVolunteerHours = this.approvedVolunteerHours,
				hoursApprovedByContactId = this.hoursApprovedByContactId,
				approvedDateTime = this.approvedDateTime,
				reimbursementAmount = this.reimbursementAmount,
				chargeTypeId = this.chargeTypeId,
				reimbursementRequested = this.reimbursementRequested,
				volunteerNotes = this.volunteerNotes,
				reminderSentDateTime = this.reminderSentDateTime,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				assignmentRole = this.assignmentRole?.ToDTO(),
				assignmentStatus = this.assignmentStatus?.ToDTO(),
				chargeType = this.chargeType?.ToDTO(),
				crew = this.crew?.ToDTO(),
				hoursApprovedByContact = this.hoursApprovedByContact?.ToDTO(),
				office = this.office?.ToDTO(),
				resource = this.resource?.ToDTO(),
				scheduledEvent = this.scheduledEvent?.ToDTO(),
				volunteerGroup = this.volunteerGroup?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a EventResourceAssignment list to list of Output Data Transfer Object intended to be used for serializing a list of EventResourceAssignment objects to avoid using the EventResourceAssignment entity type directly.
		///
		/// </summary>
		public static List<EventResourceAssignmentOutputDTO> ToOutputDTOList(List<EventResourceAssignment> data)
		{
			if (data == null)
			{
				return null;
			}

			List<EventResourceAssignmentOutputDTO> output = new List<EventResourceAssignmentOutputDTO>();

			output.Capacity = data.Count;

			foreach (EventResourceAssignment eventResourceAssignment in data)
			{
				output.Add(eventResourceAssignment.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a EventResourceAssignment Object.
		///
		/// </summary>
		public static Database.EventResourceAssignment FromDTO(EventResourceAssignmentDTO dto)
		{
			return new Database.EventResourceAssignment
			{
				id = dto.id,
				scheduledEventId = dto.scheduledEventId,
				officeId = dto.officeId,
				resourceId = dto.resourceId,
				crewId = dto.crewId,
				volunteerGroupId = dto.volunteerGroupId,
				assignmentRoleId = dto.assignmentRoleId,
				assignmentStatusId = dto.assignmentStatusId,
				assignmentStartDateTime = dto.assignmentStartDateTime,
				assignmentEndDateTime = dto.assignmentEndDateTime,
				notes = dto.notes,
				isTravelRequired = dto.isTravelRequired,
				travelDurationMinutes = dto.travelDurationMinutes,
				distanceKilometers = dto.distanceKilometers,
				startLocation = dto.startLocation,
				actualStartDateTime = dto.actualStartDateTime,
				actualEndDateTime = dto.actualEndDateTime,
				actualNotes = dto.actualNotes,
				isVolunteer = dto.isVolunteer,
				reportedVolunteerHours = dto.reportedVolunteerHours,
				approvedVolunteerHours = dto.approvedVolunteerHours,
				hoursApprovedByContactId = dto.hoursApprovedByContactId,
				approvedDateTime = dto.approvedDateTime,
				reimbursementAmount = dto.reimbursementAmount,
				chargeTypeId = dto.chargeTypeId,
				reimbursementRequested = dto.reimbursementRequested,
				volunteerNotes = dto.volunteerNotes,
				reminderSentDateTime = dto.reminderSentDateTime,
				versionNumber = dto.versionNumber,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a EventResourceAssignment Object.
		///
		/// </summary>
		public void ApplyDTO(EventResourceAssignmentDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.scheduledEventId = dto.scheduledEventId;
			this.officeId = dto.officeId;
			this.resourceId = dto.resourceId;
			this.crewId = dto.crewId;
			this.volunteerGroupId = dto.volunteerGroupId;
			this.assignmentRoleId = dto.assignmentRoleId;
			this.assignmentStatusId = dto.assignmentStatusId;
			this.assignmentStartDateTime = dto.assignmentStartDateTime;
			this.assignmentEndDateTime = dto.assignmentEndDateTime;
			this.notes = dto.notes;
			this.isTravelRequired = dto.isTravelRequired;
			this.travelDurationMinutes = dto.travelDurationMinutes;
			this.distanceKilometers = dto.distanceKilometers;
			this.startLocation = dto.startLocation;
			this.actualStartDateTime = dto.actualStartDateTime;
			this.actualEndDateTime = dto.actualEndDateTime;
			this.actualNotes = dto.actualNotes;
			this.isVolunteer = dto.isVolunteer;
			this.reportedVolunteerHours = dto.reportedVolunteerHours;
			this.approvedVolunteerHours = dto.approvedVolunteerHours;
			this.hoursApprovedByContactId = dto.hoursApprovedByContactId;
			this.approvedDateTime = dto.approvedDateTime;
			this.reimbursementAmount = dto.reimbursementAmount;
			this.chargeTypeId = dto.chargeTypeId;
			this.reimbursementRequested = dto.reimbursementRequested;
			this.volunteerNotes = dto.volunteerNotes;
			this.reminderSentDateTime = dto.reminderSentDateTime;
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
		/// Creates a deep copy clone of a EventResourceAssignment Object.
		///
		/// </summary>
		public EventResourceAssignment Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new EventResourceAssignment{
				id = this.id,
				tenantGuid = this.tenantGuid,
				scheduledEventId = this.scheduledEventId,
				officeId = this.officeId,
				resourceId = this.resourceId,
				crewId = this.crewId,
				volunteerGroupId = this.volunteerGroupId,
				assignmentRoleId = this.assignmentRoleId,
				assignmentStatusId = this.assignmentStatusId,
				assignmentStartDateTime = this.assignmentStartDateTime,
				assignmentEndDateTime = this.assignmentEndDateTime,
				notes = this.notes,
				isTravelRequired = this.isTravelRequired,
				travelDurationMinutes = this.travelDurationMinutes,
				distanceKilometers = this.distanceKilometers,
				startLocation = this.startLocation,
				actualStartDateTime = this.actualStartDateTime,
				actualEndDateTime = this.actualEndDateTime,
				actualNotes = this.actualNotes,
				isVolunteer = this.isVolunteer,
				reportedVolunteerHours = this.reportedVolunteerHours,
				approvedVolunteerHours = this.approvedVolunteerHours,
				hoursApprovedByContactId = this.hoursApprovedByContactId,
				approvedDateTime = this.approvedDateTime,
				reimbursementAmount = this.reimbursementAmount,
				chargeTypeId = this.chargeTypeId,
				reimbursementRequested = this.reimbursementRequested,
				volunteerNotes = this.volunteerNotes,
				reminderSentDateTime = this.reminderSentDateTime,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a EventResourceAssignment Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a EventResourceAssignment Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a EventResourceAssignment Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a EventResourceAssignment Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.EventResourceAssignment eventResourceAssignment)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (eventResourceAssignment == null)
			{
				return null;
			}

			return new {
				id = eventResourceAssignment.id,
				scheduledEventId = eventResourceAssignment.scheduledEventId,
				officeId = eventResourceAssignment.officeId,
				resourceId = eventResourceAssignment.resourceId,
				crewId = eventResourceAssignment.crewId,
				volunteerGroupId = eventResourceAssignment.volunteerGroupId,
				assignmentRoleId = eventResourceAssignment.assignmentRoleId,
				assignmentStatusId = eventResourceAssignment.assignmentStatusId,
				assignmentStartDateTime = eventResourceAssignment.assignmentStartDateTime,
				assignmentEndDateTime = eventResourceAssignment.assignmentEndDateTime,
				notes = eventResourceAssignment.notes,
				isTravelRequired = eventResourceAssignment.isTravelRequired,
				travelDurationMinutes = eventResourceAssignment.travelDurationMinutes,
				distanceKilometers = eventResourceAssignment.distanceKilometers,
				startLocation = eventResourceAssignment.startLocation,
				actualStartDateTime = eventResourceAssignment.actualStartDateTime,
				actualEndDateTime = eventResourceAssignment.actualEndDateTime,
				actualNotes = eventResourceAssignment.actualNotes,
				isVolunteer = eventResourceAssignment.isVolunteer,
				reportedVolunteerHours = eventResourceAssignment.reportedVolunteerHours,
				approvedVolunteerHours = eventResourceAssignment.approvedVolunteerHours,
				hoursApprovedByContactId = eventResourceAssignment.hoursApprovedByContactId,
				approvedDateTime = eventResourceAssignment.approvedDateTime,
				reimbursementAmount = eventResourceAssignment.reimbursementAmount,
				chargeTypeId = eventResourceAssignment.chargeTypeId,
				reimbursementRequested = eventResourceAssignment.reimbursementRequested,
				volunteerNotes = eventResourceAssignment.volunteerNotes,
				reminderSentDateTime = eventResourceAssignment.reminderSentDateTime,
				versionNumber = eventResourceAssignment.versionNumber,
				objectGuid = eventResourceAssignment.objectGuid,
				active = eventResourceAssignment.active,
				deleted = eventResourceAssignment.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a EventResourceAssignment Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(EventResourceAssignment eventResourceAssignment)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (eventResourceAssignment == null)
			{
				return null;
			}

			return new {
				id = eventResourceAssignment.id,
				scheduledEventId = eventResourceAssignment.scheduledEventId,
				officeId = eventResourceAssignment.officeId,
				resourceId = eventResourceAssignment.resourceId,
				crewId = eventResourceAssignment.crewId,
				volunteerGroupId = eventResourceAssignment.volunteerGroupId,
				assignmentRoleId = eventResourceAssignment.assignmentRoleId,
				assignmentStatusId = eventResourceAssignment.assignmentStatusId,
				assignmentStartDateTime = eventResourceAssignment.assignmentStartDateTime,
				assignmentEndDateTime = eventResourceAssignment.assignmentEndDateTime,
				notes = eventResourceAssignment.notes,
				isTravelRequired = eventResourceAssignment.isTravelRequired,
				travelDurationMinutes = eventResourceAssignment.travelDurationMinutes,
				distanceKilometers = eventResourceAssignment.distanceKilometers,
				startLocation = eventResourceAssignment.startLocation,
				actualStartDateTime = eventResourceAssignment.actualStartDateTime,
				actualEndDateTime = eventResourceAssignment.actualEndDateTime,
				actualNotes = eventResourceAssignment.actualNotes,
				isVolunteer = eventResourceAssignment.isVolunteer,
				reportedVolunteerHours = eventResourceAssignment.reportedVolunteerHours,
				approvedVolunteerHours = eventResourceAssignment.approvedVolunteerHours,
				hoursApprovedByContactId = eventResourceAssignment.hoursApprovedByContactId,
				approvedDateTime = eventResourceAssignment.approvedDateTime,
				reimbursementAmount = eventResourceAssignment.reimbursementAmount,
				chargeTypeId = eventResourceAssignment.chargeTypeId,
				reimbursementRequested = eventResourceAssignment.reimbursementRequested,
				volunteerNotes = eventResourceAssignment.volunteerNotes,
				reminderSentDateTime = eventResourceAssignment.reminderSentDateTime,
				versionNumber = eventResourceAssignment.versionNumber,
				objectGuid = eventResourceAssignment.objectGuid,
				active = eventResourceAssignment.active,
				deleted = eventResourceAssignment.deleted,
				assignmentRole = AssignmentRole.CreateMinimalAnonymous(eventResourceAssignment.assignmentRole),
				assignmentStatus = AssignmentStatus.CreateMinimalAnonymous(eventResourceAssignment.assignmentStatus),
				chargeType = ChargeType.CreateMinimalAnonymous(eventResourceAssignment.chargeType),
				crew = Crew.CreateMinimalAnonymous(eventResourceAssignment.crew),
				hoursApprovedByContact = Contact.CreateMinimalAnonymous(eventResourceAssignment.hoursApprovedByContact),
				office = Office.CreateMinimalAnonymous(eventResourceAssignment.office),
				resource = Resource.CreateMinimalAnonymous(eventResourceAssignment.resource),
				scheduledEvent = ScheduledEvent.CreateMinimalAnonymous(eventResourceAssignment.scheduledEvent),
				volunteerGroup = VolunteerGroup.CreateMinimalAnonymous(eventResourceAssignment.volunteerGroup)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a EventResourceAssignment Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(EventResourceAssignment eventResourceAssignment)
		{
			//
			// Return a very minimal object.
			//
			if (eventResourceAssignment == null)
			{
				return null;
			}

			return new {
				id = eventResourceAssignment.id,
				name = eventResourceAssignment.startLocation,
				description = string.Join(", ", new[] { eventResourceAssignment.startLocation}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
