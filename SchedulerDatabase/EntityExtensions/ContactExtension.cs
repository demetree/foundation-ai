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
	public partial class Contact : IVersionTrackedEntity<Contact>, IAnonymousConvertible
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
        public static ChangeHistoryToolset<Contact, ContactChangeHistory> GetChangeHistoryToolsetForWriting(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return new ChangeHistoryToolset<Contact, ContactChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
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
        public static Task<ChangeHistoryToolset<Contact, ContactChangeHistory>> GetChangeHistoryToolsetForWritingAsync(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return Task.FromResult(new ChangeHistoryToolset<Contact, ContactChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken));
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
        public static ChangeHistoryToolset<Contact, ContactChangeHistory> GetChangeHistoryToolsetForReading(SchedulerContext context, CancellationToken cancellationToken = default)
        {
            return new ChangeHistoryToolset<Contact, ContactChangeHistory>(context, cancellationToken);
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
        public async Task<VersionInformation<Contact>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<Contact>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<Contact>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for point in time {pointInTime.ToString("s")} of this Contact entity.");
            }

            VersionInformation<Contact> version = new VersionInformation<Contact>();

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
        public async Task<VersionInformation<Contact>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for version {versionNumber} of this Contact entity.");
            }

            VersionInformation<Contact> version = new VersionInformation<Contact>();

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
        public async Task<List<VersionInformation<Contact>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)
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

            List <VersionInformation<Contact>> versions = new List<VersionInformation<Contact>>();

            foreach (AuditEntry versionAudit in versionAudits)
            {
                VersionInformation<Contact> version = new VersionInformation<Contact>();

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
		public class ContactDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 contactTypeId { get; set; }
			[Required]
			public String firstName { get; set; }
			public String middleName { get; set; }
			[Required]
			public String lastName { get; set; }
			public Int32? salutationId { get; set; }
			public String title { get; set; }
			public DateOnly? birthDate { get; set; }
			public String company { get; set; }
			public String email { get; set; }
			public String phone { get; set; }
			public String mobile { get; set; }
			public String position { get; set; }
			public String webSite { get; set; }
			public Int32? contactMethodId { get; set; }
			public String notes { get; set; }
			public Int32? timeZoneId { get; set; }
			public String attributes { get; set; }
			public Int32? iconId { get; set; }
			public String color { get; set; }
			public String avatarFileName { get; set; }
			public Int64? avatarSize { get; set; }
			public Byte[] avatarData { get; set; }
			public String avatarMimeType { get; set; }
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
		public class ContactOutputDTO : ContactDTO
		{
			public ContactMethod.ContactMethodDTO contactMethod { get; set; }
			public ContactType.ContactTypeDTO contactType { get; set; }
			public Icon.IconDTO icon { get; set; }
			public Salutation.SalutationDTO salutation { get; set; }
			public TimeZone.TimeZoneDTO timeZone { get; set; }
		}


		/// <summary>
		///
		/// Converts a Contact to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ContactDTO ToDTO()
		{
			return new ContactDTO
			{
				id = this.id,
				contactTypeId = this.contactTypeId,
				firstName = this.firstName,
				middleName = this.middleName,
				lastName = this.lastName,
				salutationId = this.salutationId,
				title = this.title,
				birthDate = this.birthDate,
				company = this.company,
				email = this.email,
				phone = this.phone,
				mobile = this.mobile,
				position = this.position,
				webSite = this.webSite,
				contactMethodId = this.contactMethodId,
				notes = this.notes,
				timeZoneId = this.timeZoneId,
				attributes = this.attributes,
				iconId = this.iconId,
				color = this.color,
				avatarFileName = this.avatarFileName,
				avatarSize = this.avatarSize,
				avatarData = this.avatarData,
				avatarMimeType = this.avatarMimeType,
				externalId = this.externalId,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a Contact list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ContactDTO> ToDTOList(List<Contact> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ContactDTO> output = new List<ContactDTO>();

			output.Capacity = data.Count;

			foreach (Contact contact in data)
			{
				output.Add(contact.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a Contact to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the Contact Entity type directly.
		///
		/// </summary>
		public ContactOutputDTO ToOutputDTO()
		{
			return new ContactOutputDTO
			{
				id = this.id,
				contactTypeId = this.contactTypeId,
				firstName = this.firstName,
				middleName = this.middleName,
				lastName = this.lastName,
				salutationId = this.salutationId,
				title = this.title,
				birthDate = this.birthDate,
				company = this.company,
				email = this.email,
				phone = this.phone,
				mobile = this.mobile,
				position = this.position,
				webSite = this.webSite,
				contactMethodId = this.contactMethodId,
				notes = this.notes,
				timeZoneId = this.timeZoneId,
				attributes = this.attributes,
				iconId = this.iconId,
				color = this.color,
				avatarFileName = this.avatarFileName,
				avatarSize = this.avatarSize,
				avatarData = this.avatarData,
				avatarMimeType = this.avatarMimeType,
				externalId = this.externalId,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				contactMethod = this.contactMethod?.ToDTO(),
				contactType = this.contactType?.ToDTO(),
				icon = this.icon?.ToDTO(),
				salutation = this.salutation?.ToDTO(),
				timeZone = this.timeZone?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a Contact list to list of Output Data Transfer Object intended to be used for serializing a list of Contact objects to avoid using the Contact entity type directly.
		///
		/// </summary>
		public static List<ContactOutputDTO> ToOutputDTOList(List<Contact> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ContactOutputDTO> output = new List<ContactOutputDTO>();

			output.Capacity = data.Count;

			foreach (Contact contact in data)
			{
				output.Add(contact.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a Contact Object.
		///
		/// </summary>
		public static Database.Contact FromDTO(ContactDTO dto)
		{
			return new Database.Contact
			{
				id = dto.id,
				contactTypeId = dto.contactTypeId,
				firstName = dto.firstName,
				middleName = dto.middleName,
				lastName = dto.lastName,
				salutationId = dto.salutationId,
				title = dto.title,
				birthDate = dto.birthDate,
				company = dto.company,
				email = dto.email,
				phone = dto.phone,
				mobile = dto.mobile,
				position = dto.position,
				webSite = dto.webSite,
				contactMethodId = dto.contactMethodId,
				notes = dto.notes,
				timeZoneId = dto.timeZoneId,
				attributes = dto.attributes,
				iconId = dto.iconId,
				color = dto.color,
				avatarFileName = dto.avatarFileName,
				avatarSize = dto.avatarSize,
				avatarData = dto.avatarData,
				avatarMimeType = dto.avatarMimeType,
				externalId = dto.externalId,
				versionNumber = dto.versionNumber,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a Contact Object.
		///
		/// </summary>
		public void ApplyDTO(ContactDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.contactTypeId = dto.contactTypeId;
			this.firstName = dto.firstName;
			this.middleName = dto.middleName;
			this.lastName = dto.lastName;
			this.salutationId = dto.salutationId;
			this.title = dto.title;
			this.birthDate = dto.birthDate;
			this.company = dto.company;
			this.email = dto.email;
			this.phone = dto.phone;
			this.mobile = dto.mobile;
			this.position = dto.position;
			this.webSite = dto.webSite;
			this.contactMethodId = dto.contactMethodId;
			this.notes = dto.notes;
			this.timeZoneId = dto.timeZoneId;
			this.attributes = dto.attributes;
			this.iconId = dto.iconId;
			this.color = dto.color;
			this.avatarFileName = dto.avatarFileName;
			this.avatarSize = dto.avatarSize;
			this.avatarData = dto.avatarData;
			this.avatarMimeType = dto.avatarMimeType;
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
		/// Creates a deep copy clone of a Contact Object.
		///
		/// </summary>
		public Contact Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new Contact{
				id = this.id,
				tenantGuid = this.tenantGuid,
				contactTypeId = this.contactTypeId,
				firstName = this.firstName,
				middleName = this.middleName,
				lastName = this.lastName,
				salutationId = this.salutationId,
				title = this.title,
				company = this.company,
				email = this.email,
				phone = this.phone,
				mobile = this.mobile,
				position = this.position,
				webSite = this.webSite,
				contactMethodId = this.contactMethodId,
				notes = this.notes,
				timeZoneId = this.timeZoneId,
				attributes = this.attributes,
				iconId = this.iconId,
				color = this.color,
				avatarFileName = this.avatarFileName,
				avatarSize = this.avatarSize,
				avatarData = this.avatarData,
				avatarMimeType = this.avatarMimeType,
				externalId = this.externalId,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Contact Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Contact Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a Contact Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a Contact Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.Contact contact)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (contact == null)
			{
				return null;
			}

			return new {
				id = contact.id,
				contactTypeId = contact.contactTypeId,
				firstName = contact.firstName,
				middleName = contact.middleName,
				lastName = contact.lastName,
				salutationId = contact.salutationId,
				title = contact.title,
				company = contact.company,
				email = contact.email,
				phone = contact.phone,
				mobile = contact.mobile,
				position = contact.position,
				webSite = contact.webSite,
				contactMethodId = contact.contactMethodId,
				notes = contact.notes,
				timeZoneId = contact.timeZoneId,
				attributes = contact.attributes,
				iconId = contact.iconId,
				color = contact.color,
				avatarFileName = contact.avatarFileName,
				avatarSize = contact.avatarSize,
				avatarData = contact.avatarData,
				avatarMimeType = contact.avatarMimeType,
				externalId = contact.externalId,
				versionNumber = contact.versionNumber,
				objectGuid = contact.objectGuid,
				active = contact.active,
				deleted = contact.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a Contact Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(Contact contact)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (contact == null)
			{
				return null;
			}

			return new {
				id = contact.id,
				contactTypeId = contact.contactTypeId,
				firstName = contact.firstName,
				middleName = contact.middleName,
				lastName = contact.lastName,
				salutationId = contact.salutationId,
				title = contact.title,
				birthDate = contact.birthDate,
				company = contact.company,
				email = contact.email,
				phone = contact.phone,
				mobile = contact.mobile,
				position = contact.position,
				webSite = contact.webSite,
				contactMethodId = contact.contactMethodId,
				notes = contact.notes,
				timeZoneId = contact.timeZoneId,
				attributes = contact.attributes,
				iconId = contact.iconId,
				color = contact.color,
				avatarFileName = contact.avatarFileName,
				avatarSize = contact.avatarSize,
				avatarData = contact.avatarData,
				avatarMimeType = contact.avatarMimeType,
				externalId = contact.externalId,
				versionNumber = contact.versionNumber,
				objectGuid = contact.objectGuid,
				active = contact.active,
				deleted = contact.deleted,
				contactMethod = ContactMethod.CreateMinimalAnonymous(contact.contactMethod),
				contactType = ContactType.CreateMinimalAnonymous(contact.contactType),
				icon = Icon.CreateMinimalAnonymous(contact.icon),
				salutation = Salutation.CreateMinimalAnonymous(contact.salutation),
				timeZone = TimeZone.CreateMinimalAnonymous(contact.timeZone)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a Contact Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(Contact contact)
		{
			//
			// Return a very minimal object.
			//
			if (contact == null)
			{
				return null;
			}

			return new {
				id = contact.id,
				name = contact.firstName,
				description = string.Join(", ", new[] { contact.firstName, contact.middleName, contact.lastName}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
