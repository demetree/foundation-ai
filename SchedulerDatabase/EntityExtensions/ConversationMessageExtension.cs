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
	public partial class ConversationMessage : IVersionTrackedEntity<ConversationMessage>, IAnonymousConvertible
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
        public static ChangeHistoryToolset<ConversationMessage, ConversationMessageChangeHistory> GetChangeHistoryToolsetForWriting(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return new ChangeHistoryToolset<ConversationMessage, ConversationMessageChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
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
        public static Task<ChangeHistoryToolset<ConversationMessage, ConversationMessageChangeHistory>> GetChangeHistoryToolsetForWritingAsync(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return Task.FromResult(new ChangeHistoryToolset<ConversationMessage, ConversationMessageChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken));
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
        public static ChangeHistoryToolset<ConversationMessage, ConversationMessageChangeHistory> GetChangeHistoryToolsetForReading(SchedulerContext context, CancellationToken cancellationToken = default)
        {
            return new ChangeHistoryToolset<ConversationMessage, ConversationMessageChangeHistory>(context, cancellationToken);
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
        public async Task<VersionInformation<ConversationMessage>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<ConversationMessage>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<ConversationMessage>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for point in time {pointInTime.ToString("s")} of this ConversationMessage entity.");
            }

            VersionInformation<ConversationMessage> version = new VersionInformation<ConversationMessage>();

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
        public async Task<VersionInformation<ConversationMessage>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for version {versionNumber} of this ConversationMessage entity.");
            }

            VersionInformation<ConversationMessage> version = new VersionInformation<ConversationMessage>();

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
        public async Task<List<VersionInformation<ConversationMessage>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)
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

            List <VersionInformation<ConversationMessage>> versions = new List<VersionInformation<ConversationMessage>>();

            foreach (AuditEntry versionAudit in versionAudits)
            {
                VersionInformation<ConversationMessage> version = new VersionInformation<ConversationMessage>();

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
		public class ConversationMessageDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 conversationId { get; set; }
			[Required]
			public Int32 userId { get; set; }
			public Int32? parentConversationMessageId { get; set; }
			public Int32? conversationChannelId { get; set; }
			[Required]
			public DateTime dateTimeCreated { get; set; }
			[Required]
			public String message { get; set; }
			public String messageType { get; set; }
			public String entity { get; set; }
			public Int32? entityId { get; set; }
			public String externalURL { get; set; }
			public Int32? forwardedFromMessageId { get; set; }
			public Int32? forwardedFromUserId { get; set; }
			[Required]
			public Boolean isScheduled { get; set; }
			public DateTime? scheduledDateTime { get; set; }
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
		public class ConversationMessageOutputDTO : ConversationMessageDTO
		{
			public Conversation.ConversationDTO conversation { get; set; }
			public ConversationChannel.ConversationChannelDTO conversationChannel { get; set; }
			public ConversationMessage.ConversationMessageDTO parentConversationMessage { get; set; }
		}


		/// <summary>
		///
		/// Converts a ConversationMessage to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ConversationMessageDTO ToDTO()
		{
			return new ConversationMessageDTO
			{
				id = this.id,
				conversationId = this.conversationId,
				userId = this.userId,
				parentConversationMessageId = this.parentConversationMessageId,
				conversationChannelId = this.conversationChannelId,
				dateTimeCreated = this.dateTimeCreated,
				message = this.message,
				messageType = this.messageType,
				entity = this.entity,
				entityId = this.entityId,
				externalURL = this.externalURL,
				forwardedFromMessageId = this.forwardedFromMessageId,
				forwardedFromUserId = this.forwardedFromUserId,
				isScheduled = this.isScheduled,
				scheduledDateTime = this.scheduledDateTime,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ConversationMessage list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ConversationMessageDTO> ToDTOList(List<ConversationMessage> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConversationMessageDTO> output = new List<ConversationMessageDTO>();

			output.Capacity = data.Count;

			foreach (ConversationMessage conversationMessage in data)
			{
				output.Add(conversationMessage.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ConversationMessage to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ConversationMessage Entity type directly.
		///
		/// </summary>
		public ConversationMessageOutputDTO ToOutputDTO()
		{
			return new ConversationMessageOutputDTO
			{
				id = this.id,
				conversationId = this.conversationId,
				userId = this.userId,
				parentConversationMessageId = this.parentConversationMessageId,
				conversationChannelId = this.conversationChannelId,
				dateTimeCreated = this.dateTimeCreated,
				message = this.message,
				messageType = this.messageType,
				entity = this.entity,
				entityId = this.entityId,
				externalURL = this.externalURL,
				forwardedFromMessageId = this.forwardedFromMessageId,
				forwardedFromUserId = this.forwardedFromUserId,
				isScheduled = this.isScheduled,
				scheduledDateTime = this.scheduledDateTime,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				conversation = this.conversation?.ToDTO(),
				conversationChannel = this.conversationChannel?.ToDTO(),
				parentConversationMessage = this.parentConversationMessage?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ConversationMessage list to list of Output Data Transfer Object intended to be used for serializing a list of ConversationMessage objects to avoid using the ConversationMessage entity type directly.
		///
		/// </summary>
		public static List<ConversationMessageOutputDTO> ToOutputDTOList(List<ConversationMessage> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConversationMessageOutputDTO> output = new List<ConversationMessageOutputDTO>();

			output.Capacity = data.Count;

			foreach (ConversationMessage conversationMessage in data)
			{
				output.Add(conversationMessage.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ConversationMessage Object.
		///
		/// </summary>
		public static Database.ConversationMessage FromDTO(ConversationMessageDTO dto)
		{
			return new Database.ConversationMessage
			{
				id = dto.id,
				conversationId = dto.conversationId,
				userId = dto.userId,
				parentConversationMessageId = dto.parentConversationMessageId,
				conversationChannelId = dto.conversationChannelId,
				dateTimeCreated = dto.dateTimeCreated,
				message = dto.message,
				messageType = dto.messageType,
				entity = dto.entity,
				entityId = dto.entityId,
				externalURL = dto.externalURL,
				forwardedFromMessageId = dto.forwardedFromMessageId,
				forwardedFromUserId = dto.forwardedFromUserId,
				isScheduled = dto.isScheduled,
				scheduledDateTime = dto.scheduledDateTime,
				versionNumber = dto.versionNumber,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ConversationMessage Object.
		///
		/// </summary>
		public void ApplyDTO(ConversationMessageDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.conversationId = dto.conversationId;
			this.userId = dto.userId;
			this.parentConversationMessageId = dto.parentConversationMessageId;
			this.conversationChannelId = dto.conversationChannelId;
			this.dateTimeCreated = dto.dateTimeCreated;
			this.message = dto.message;
			this.messageType = dto.messageType;
			this.entity = dto.entity;
			this.entityId = dto.entityId;
			this.externalURL = dto.externalURL;
			this.forwardedFromMessageId = dto.forwardedFromMessageId;
			this.forwardedFromUserId = dto.forwardedFromUserId;
			this.isScheduled = dto.isScheduled;
			this.scheduledDateTime = dto.scheduledDateTime;
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
		/// Creates a deep copy clone of a ConversationMessage Object.
		///
		/// </summary>
		public ConversationMessage Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ConversationMessage{
				id = this.id,
				tenantGuid = this.tenantGuid,
				conversationId = this.conversationId,
				userId = this.userId,
				parentConversationMessageId = this.parentConversationMessageId,
				conversationChannelId = this.conversationChannelId,
				dateTimeCreated = this.dateTimeCreated,
				message = this.message,
				messageType = this.messageType,
				entity = this.entity,
				entityId = this.entityId,
				externalURL = this.externalURL,
				forwardedFromMessageId = this.forwardedFromMessageId,
				forwardedFromUserId = this.forwardedFromUserId,
				isScheduled = this.isScheduled,
				scheduledDateTime = this.scheduledDateTime,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConversationMessage Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConversationMessage Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ConversationMessage Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ConversationMessage Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ConversationMessage conversationMessage)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (conversationMessage == null)
			{
				return null;
			}

			return new {
				id = conversationMessage.id,
				conversationId = conversationMessage.conversationId,
				userId = conversationMessage.userId,
				parentConversationMessageId = conversationMessage.parentConversationMessageId,
				conversationChannelId = conversationMessage.conversationChannelId,
				dateTimeCreated = conversationMessage.dateTimeCreated,
				message = conversationMessage.message,
				messageType = conversationMessage.messageType,
				entity = conversationMessage.entity,
				entityId = conversationMessage.entityId,
				externalURL = conversationMessage.externalURL,
				forwardedFromMessageId = conversationMessage.forwardedFromMessageId,
				forwardedFromUserId = conversationMessage.forwardedFromUserId,
				isScheduled = conversationMessage.isScheduled,
				scheduledDateTime = conversationMessage.scheduledDateTime,
				versionNumber = conversationMessage.versionNumber,
				objectGuid = conversationMessage.objectGuid,
				active = conversationMessage.active,
				deleted = conversationMessage.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ConversationMessage Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ConversationMessage conversationMessage)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (conversationMessage == null)
			{
				return null;
			}

			return new {
				id = conversationMessage.id,
				conversationId = conversationMessage.conversationId,
				userId = conversationMessage.userId,
				parentConversationMessageId = conversationMessage.parentConversationMessageId,
				conversationChannelId = conversationMessage.conversationChannelId,
				dateTimeCreated = conversationMessage.dateTimeCreated,
				message = conversationMessage.message,
				messageType = conversationMessage.messageType,
				entity = conversationMessage.entity,
				entityId = conversationMessage.entityId,
				externalURL = conversationMessage.externalURL,
				forwardedFromMessageId = conversationMessage.forwardedFromMessageId,
				forwardedFromUserId = conversationMessage.forwardedFromUserId,
				isScheduled = conversationMessage.isScheduled,
				scheduledDateTime = conversationMessage.scheduledDateTime,
				versionNumber = conversationMessage.versionNumber,
				objectGuid = conversationMessage.objectGuid,
				active = conversationMessage.active,
				deleted = conversationMessage.deleted,
				conversation = Conversation.CreateMinimalAnonymous(conversationMessage.conversation),
				conversationChannel = ConversationChannel.CreateMinimalAnonymous(conversationMessage.conversationChannel),
				parentConversationMessage = ConversationMessage.CreateMinimalAnonymous(conversationMessage.parentConversationMessage)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ConversationMessage Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ConversationMessage conversationMessage)
		{
			//
			// Return a very minimal object.
			//
			if (conversationMessage == null)
			{
				return null;
			}

			return new {
				id = conversationMessage.id,
				name = conversationMessage.messageType,
				description = string.Join(", ", new[] { conversationMessage.messageType, conversationMessage.entity, conversationMessage.externalURL}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
