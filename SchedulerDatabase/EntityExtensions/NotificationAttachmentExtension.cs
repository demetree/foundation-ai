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
	public partial class NotificationAttachment : IVersionTrackedEntity<NotificationAttachment>, IAnonymousConvertible
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
        public static ChangeHistoryToolset<NotificationAttachment, NotificationAttachmentChangeHistory> GetChangeHistoryToolsetForWriting(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return new ChangeHistoryToolset<NotificationAttachment, NotificationAttachmentChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
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
        public static Task<ChangeHistoryToolset<NotificationAttachment, NotificationAttachmentChangeHistory>> GetChangeHistoryToolsetForWritingAsync(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return Task.FromResult(new ChangeHistoryToolset<NotificationAttachment, NotificationAttachmentChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken));
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
        public static ChangeHistoryToolset<NotificationAttachment, NotificationAttachmentChangeHistory> GetChangeHistoryToolsetForReading(SchedulerContext context, CancellationToken cancellationToken = default)
        {
            return new ChangeHistoryToolset<NotificationAttachment, NotificationAttachmentChangeHistory>(context, cancellationToken);
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
        public async Task<VersionInformation<NotificationAttachment>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<NotificationAttachment>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<NotificationAttachment>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for point in time {pointInTime.ToString("s")} of this NotificationAttachment entity.");
            }

            VersionInformation<NotificationAttachment> version = new VersionInformation<NotificationAttachment>();

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
        public async Task<VersionInformation<NotificationAttachment>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for version {versionNumber} of this NotificationAttachment entity.");
            }

            VersionInformation<NotificationAttachment> version = new VersionInformation<NotificationAttachment>();

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
        public async Task<List<VersionInformation<NotificationAttachment>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)
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

            List <VersionInformation<NotificationAttachment>> versions = new List<VersionInformation<NotificationAttachment>>();

            foreach (AuditEntry versionAudit in versionAudits)
            {
                VersionInformation<NotificationAttachment> version = new VersionInformation<NotificationAttachment>();

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
		public class NotificationAttachmentDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 notificationId { get; set; }
			[Required]
			public Int32 userId { get; set; }
			[Required]
			public DateTime dateTimeCreated { get; set; }
			[Required]
			public String contentFileName { get; set; }
			[Required]
			public Int64 contentSize { get; set; }
			[Required]
			public Byte[] contentData { get; set; }
			[Required]
			public String contentMimeType { get; set; }
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
		public class NotificationAttachmentOutputDTO : NotificationAttachmentDTO
		{
			public Notification.NotificationDTO notification { get; set; }
		}


		/// <summary>
		///
		/// Converts a NotificationAttachment to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public NotificationAttachmentDTO ToDTO()
		{
			return new NotificationAttachmentDTO
			{
				id = this.id,
				notificationId = this.notificationId,
				userId = this.userId,
				dateTimeCreated = this.dateTimeCreated,
				contentFileName = this.contentFileName,
				contentSize = this.contentSize,
				contentData = this.contentData,
				contentMimeType = this.contentMimeType,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a NotificationAttachment list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<NotificationAttachmentDTO> ToDTOList(List<NotificationAttachment> data)
		{
			if (data == null)
			{
				return null;
			}

			List<NotificationAttachmentDTO> output = new List<NotificationAttachmentDTO>();

			output.Capacity = data.Count;

			foreach (NotificationAttachment notificationAttachment in data)
			{
				output.Add(notificationAttachment.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a NotificationAttachment to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the NotificationAttachment Entity type directly.
		///
		/// </summary>
		public NotificationAttachmentOutputDTO ToOutputDTO()
		{
			return new NotificationAttachmentOutputDTO
			{
				id = this.id,
				notificationId = this.notificationId,
				userId = this.userId,
				dateTimeCreated = this.dateTimeCreated,
				contentFileName = this.contentFileName,
				contentSize = this.contentSize,
				contentData = this.contentData,
				contentMimeType = this.contentMimeType,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				notification = this.notification?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a NotificationAttachment list to list of Output Data Transfer Object intended to be used for serializing a list of NotificationAttachment objects to avoid using the NotificationAttachment entity type directly.
		///
		/// </summary>
		public static List<NotificationAttachmentOutputDTO> ToOutputDTOList(List<NotificationAttachment> data)
		{
			if (data == null)
			{
				return null;
			}

			List<NotificationAttachmentOutputDTO> output = new List<NotificationAttachmentOutputDTO>();

			output.Capacity = data.Count;

			foreach (NotificationAttachment notificationAttachment in data)
			{
				output.Add(notificationAttachment.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a NotificationAttachment Object.
		///
		/// </summary>
		public static Database.NotificationAttachment FromDTO(NotificationAttachmentDTO dto)
		{
			return new Database.NotificationAttachment
			{
				id = dto.id,
				notificationId = dto.notificationId,
				userId = dto.userId,
				dateTimeCreated = dto.dateTimeCreated,
				contentFileName = dto.contentFileName,
				contentSize = dto.contentSize,
				contentData = dto.contentData,
				contentMimeType = dto.contentMimeType,
				versionNumber = dto.versionNumber,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a NotificationAttachment Object.
		///
		/// </summary>
		public void ApplyDTO(NotificationAttachmentDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.notificationId = dto.notificationId;
			this.userId = dto.userId;
			this.dateTimeCreated = dto.dateTimeCreated;
			this.contentFileName = dto.contentFileName;
			this.contentSize = dto.contentSize;
			this.contentData = dto.contentData;
			this.contentMimeType = dto.contentMimeType;
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
		/// Creates a deep copy clone of a NotificationAttachment Object.
		///
		/// </summary>
		public NotificationAttachment Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new NotificationAttachment{
				id = this.id,
				tenantGuid = this.tenantGuid,
				notificationId = this.notificationId,
				userId = this.userId,
				dateTimeCreated = this.dateTimeCreated,
				contentFileName = this.contentFileName,
				contentSize = this.contentSize,
				contentData = this.contentData,
				contentMimeType = this.contentMimeType,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a NotificationAttachment Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a NotificationAttachment Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a NotificationAttachment Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a NotificationAttachment Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.NotificationAttachment notificationAttachment)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (notificationAttachment == null)
			{
				return null;
			}

			return new {
				id = notificationAttachment.id,
				notificationId = notificationAttachment.notificationId,
				userId = notificationAttachment.userId,
				dateTimeCreated = notificationAttachment.dateTimeCreated,
				contentFileName = notificationAttachment.contentFileName,
				contentSize = notificationAttachment.contentSize,
				contentData = notificationAttachment.contentData,
				contentMimeType = notificationAttachment.contentMimeType,
				versionNumber = notificationAttachment.versionNumber,
				objectGuid = notificationAttachment.objectGuid,
				active = notificationAttachment.active,
				deleted = notificationAttachment.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a NotificationAttachment Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(NotificationAttachment notificationAttachment)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (notificationAttachment == null)
			{
				return null;
			}

			return new {
				id = notificationAttachment.id,
				notificationId = notificationAttachment.notificationId,
				userId = notificationAttachment.userId,
				dateTimeCreated = notificationAttachment.dateTimeCreated,
				contentFileName = notificationAttachment.contentFileName,
				contentSize = notificationAttachment.contentSize,
				contentData = notificationAttachment.contentData,
				contentMimeType = notificationAttachment.contentMimeType,
				versionNumber = notificationAttachment.versionNumber,
				objectGuid = notificationAttachment.objectGuid,
				active = notificationAttachment.active,
				deleted = notificationAttachment.deleted,
				notification = Notification.CreateMinimalAnonymous(notificationAttachment.notification)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a NotificationAttachment Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(NotificationAttachment notificationAttachment)
		{
			//
			// Return a very minimal object.
			//
			if (notificationAttachment == null)
			{
				return null;
			}

			return new {
				id = notificationAttachment.id,
				name = notificationAttachment.contentFileName,
				description = string.Join(", ", new[] { notificationAttachment.contentFileName, notificationAttachment.contentMimeType}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
