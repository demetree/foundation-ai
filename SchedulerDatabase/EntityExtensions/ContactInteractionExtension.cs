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
	public partial class ContactInteraction : IVersionTrackedEntity<ContactInteraction>, IAnonymousConvertible
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
        public static ChangeHistoryToolset<ContactInteraction, ContactInteractionChangeHistory> GetChangeHistoryToolsetForWriting(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return new ChangeHistoryToolset<ContactInteraction, ContactInteractionChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
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
        public static ChangeHistoryToolset<ContactInteraction, ContactInteractionChangeHistory> GetChangeHistoryToolsetForReading(SchedulerContext context, CancellationToken cancellationToken = default)
        {
            return new ChangeHistoryToolset<ContactInteraction, ContactInteractionChangeHistory>(context, cancellationToken);
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
        public async Task<VersionInformation<ContactInteraction>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<ContactInteraction>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<ContactInteraction>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for point in time {pointInTime.ToString("s")} of this ContactInteraction entity.");
            }

            VersionInformation<ContactInteraction> version = new VersionInformation<ContactInteraction>();

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
        public async Task<VersionInformation<ContactInteraction>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for version {versionNumber} of this ContactInteraction entity.");
            }

            VersionInformation<ContactInteraction> version = new VersionInformation<ContactInteraction>();

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
        public async Task<List<VersionInformation<ContactInteraction>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)
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

            List <VersionInformation<ContactInteraction>> versions = new List<VersionInformation<ContactInteraction>>();

            foreach (AuditEntry versionAudit in versionAudits)
            {
                VersionInformation<ContactInteraction> version = new VersionInformation<ContactInteraction>();

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
                    version.data = await chts.GetVersionAsync(this, 1).ConfigureAwait(false);
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
		public class ContactInteractionDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 contactId { get; set; }
			public Int32? initiatingContactId { get; set; }
			[Required]
			public Int32 interactionTypeId { get; set; }
			public Int32? scheduledEventId { get; set; }
			[Required]
			public DateTime startTime { get; set; }
			public DateTime? endTime { get; set; }
			public String notes { get; set; }
			public String location { get; set; }
			public Int32? priorityId { get; set; }
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
		public class ContactInteractionOutputDTO : ContactInteractionDTO
		{
			public Contact.ContactDTO contact { get; set; }
			public Contact.ContactDTO initiatingContact { get; set; }
			public InteractionType.InteractionTypeDTO interactionType { get; set; }
			public Priority.PriorityDTO priority { get; set; }
			public ScheduledEvent.ScheduledEventDTO scheduledEvent { get; set; }
		}


		/// <summary>
		///
		/// Converts a ContactInteraction to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ContactInteractionDTO ToDTO()
		{
			return new ContactInteractionDTO
			{
				id = this.id,
				contactId = this.contactId,
				initiatingContactId = this.initiatingContactId,
				interactionTypeId = this.interactionTypeId,
				scheduledEventId = this.scheduledEventId,
				startTime = this.startTime,
				endTime = this.endTime,
				notes = this.notes,
				location = this.location,
				priorityId = this.priorityId,
				externalId = this.externalId,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ContactInteraction list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ContactInteractionDTO> ToDTOList(List<ContactInteraction> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ContactInteractionDTO> output = new List<ContactInteractionDTO>();

			output.Capacity = data.Count;

			foreach (ContactInteraction contactInteraction in data)
			{
				output.Add(contactInteraction.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ContactInteraction to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ContactInteractionEntity type directly.
		///
		/// </summary>
		public ContactInteractionOutputDTO ToOutputDTO()
		{
			return new ContactInteractionOutputDTO
			{
				id = this.id,
				contactId = this.contactId,
				initiatingContactId = this.initiatingContactId,
				interactionTypeId = this.interactionTypeId,
				scheduledEventId = this.scheduledEventId,
				startTime = this.startTime,
				endTime = this.endTime,
				notes = this.notes,
				location = this.location,
				priorityId = this.priorityId,
				externalId = this.externalId,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				contact = this.contact?.ToDTO(),
				initiatingContact = this.initiatingContact?.ToDTO(),
				interactionType = this.interactionType?.ToDTO(),
				priority = this.priority?.ToDTO(),
				scheduledEvent = this.scheduledEvent?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ContactInteraction list to list of Output Data Transfer Object intended to be used for serializing a list of ContactInteraction objects to avoid using the ContactInteraction entity type directly.
		///
		/// </summary>
		public static List<ContactInteractionOutputDTO> ToOutputDTOList(List<ContactInteraction> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ContactInteractionOutputDTO> output = new List<ContactInteractionOutputDTO>();

			output.Capacity = data.Count;

			foreach (ContactInteraction contactInteraction in data)
			{
				output.Add(contactInteraction.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ContactInteraction Object.
		///
		/// </summary>
		public static Database.ContactInteraction FromDTO(ContactInteractionDTO dto)
		{
			return new Database.ContactInteraction
			{
				id = dto.id,
				contactId = dto.contactId,
				initiatingContactId = dto.initiatingContactId,
				interactionTypeId = dto.interactionTypeId,
				scheduledEventId = dto.scheduledEventId,
				startTime = dto.startTime,
				endTime = dto.endTime,
				notes = dto.notes,
				location = dto.location,
				priorityId = dto.priorityId,
				externalId = dto.externalId,
				versionNumber = dto.versionNumber,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ContactInteraction Object.
		///
		/// </summary>
		public void ApplyDTO(ContactInteractionDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.contactId = dto.contactId;
			this.initiatingContactId = dto.initiatingContactId;
			this.interactionTypeId = dto.interactionTypeId;
			this.scheduledEventId = dto.scheduledEventId;
			this.startTime = dto.startTime;
			this.endTime = dto.endTime;
			this.notes = dto.notes;
			this.location = dto.location;
			this.priorityId = dto.priorityId;
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
		/// Creates a deep copy clone of a ContactInteraction Object.
		///
		/// </summary>
		public ContactInteraction Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ContactInteraction{
				id = this.id,
				tenantGuid = this.tenantGuid,
				contactId = this.contactId,
				initiatingContactId = this.initiatingContactId,
				interactionTypeId = this.interactionTypeId,
				scheduledEventId = this.scheduledEventId,
				startTime = this.startTime,
				endTime = this.endTime,
				notes = this.notes,
				location = this.location,
				priorityId = this.priorityId,
				externalId = this.externalId,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ContactInteraction Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ContactInteraction Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ContactInteraction Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ContactInteraction Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ContactInteraction contactInteraction)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (contactInteraction == null)
			{
				return null;
			}

			return new {
				id = contactInteraction.id,
				contactId = contactInteraction.contactId,
				initiatingContactId = contactInteraction.initiatingContactId,
				interactionTypeId = contactInteraction.interactionTypeId,
				scheduledEventId = contactInteraction.scheduledEventId,
				startTime = contactInteraction.startTime,
				endTime = contactInteraction.endTime,
				notes = contactInteraction.notes,
				location = contactInteraction.location,
				priorityId = contactInteraction.priorityId,
				externalId = contactInteraction.externalId,
				versionNumber = contactInteraction.versionNumber,
				objectGuid = contactInteraction.objectGuid,
				active = contactInteraction.active,
				deleted = contactInteraction.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ContactInteraction Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ContactInteraction contactInteraction)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (contactInteraction == null)
			{
				return null;
			}

			return new {
				id = contactInteraction.id,
				contactId = contactInteraction.contactId,
				initiatingContactId = contactInteraction.initiatingContactId,
				interactionTypeId = contactInteraction.interactionTypeId,
				scheduledEventId = contactInteraction.scheduledEventId,
				startTime = contactInteraction.startTime,
				endTime = contactInteraction.endTime,
				notes = contactInteraction.notes,
				location = contactInteraction.location,
				priorityId = contactInteraction.priorityId,
				externalId = contactInteraction.externalId,
				versionNumber = contactInteraction.versionNumber,
				objectGuid = contactInteraction.objectGuid,
				active = contactInteraction.active,
				deleted = contactInteraction.deleted,
				contact = Contact.CreateMinimalAnonymous(contactInteraction.contact),
				initiatingContact = Contact.CreateMinimalAnonymous(contactInteraction.initiatingContact),
				interactionType = InteractionType.CreateMinimalAnonymous(contactInteraction.interactionType),
				priority = Priority.CreateMinimalAnonymous(contactInteraction.priority),
				scheduledEvent = ScheduledEvent.CreateMinimalAnonymous(contactInteraction.scheduledEvent)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ContactInteraction Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ContactInteraction contactInteraction)
		{
			//
			// Return a very minimal object.
			//
			if (contactInteraction == null)
			{
				return null;
			}

			return new {
				id = contactInteraction.id,
				name = contactInteraction.externalId,
				description = string.Join(", ", new[] { contactInteraction.externalId}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
