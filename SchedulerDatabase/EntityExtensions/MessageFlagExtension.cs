using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Scheduler.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class MessageFlag : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class MessageFlagDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 conversationMessageId { get; set; }
			[Required]
			public Int32 flaggedByUserId { get; set; }
			[Required]
			public String reason { get; set; }
			public String details { get; set; }
			[Required]
			public String status { get; set; }
			public Int32? reviewedByUserId { get; set; }
			public DateTime? dateTimeReviewed { get; set; }
			public String resolutionNotes { get; set; }
			[Required]
			public DateTime dateTimeCreated { get; set; }
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
		public class MessageFlagOutputDTO : MessageFlagDTO
		{
			public ConversationMessage.ConversationMessageDTO conversationMessage { get; set; }
		}


		/// <summary>
		///
		/// Converts a MessageFlag to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public MessageFlagDTO ToDTO()
		{
			return new MessageFlagDTO
			{
				id = this.id,
				conversationMessageId = this.conversationMessageId,
				flaggedByUserId = this.flaggedByUserId,
				reason = this.reason,
				details = this.details,
				status = this.status,
				reviewedByUserId = this.reviewedByUserId,
				dateTimeReviewed = this.dateTimeReviewed,
				resolutionNotes = this.resolutionNotes,
				dateTimeCreated = this.dateTimeCreated,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a MessageFlag list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<MessageFlagDTO> ToDTOList(List<MessageFlag> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MessageFlagDTO> output = new List<MessageFlagDTO>();

			output.Capacity = data.Count;

			foreach (MessageFlag messageFlag in data)
			{
				output.Add(messageFlag.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a MessageFlag to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the MessageFlag Entity type directly.
		///
		/// </summary>
		public MessageFlagOutputDTO ToOutputDTO()
		{
			return new MessageFlagOutputDTO
			{
				id = this.id,
				conversationMessageId = this.conversationMessageId,
				flaggedByUserId = this.flaggedByUserId,
				reason = this.reason,
				details = this.details,
				status = this.status,
				reviewedByUserId = this.reviewedByUserId,
				dateTimeReviewed = this.dateTimeReviewed,
				resolutionNotes = this.resolutionNotes,
				dateTimeCreated = this.dateTimeCreated,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				conversationMessage = this.conversationMessage?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a MessageFlag list to list of Output Data Transfer Object intended to be used for serializing a list of MessageFlag objects to avoid using the MessageFlag entity type directly.
		///
		/// </summary>
		public static List<MessageFlagOutputDTO> ToOutputDTOList(List<MessageFlag> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MessageFlagOutputDTO> output = new List<MessageFlagOutputDTO>();

			output.Capacity = data.Count;

			foreach (MessageFlag messageFlag in data)
			{
				output.Add(messageFlag.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a MessageFlag Object.
		///
		/// </summary>
		public static Database.MessageFlag FromDTO(MessageFlagDTO dto)
		{
			return new Database.MessageFlag
			{
				id = dto.id,
				conversationMessageId = dto.conversationMessageId,
				flaggedByUserId = dto.flaggedByUserId,
				reason = dto.reason,
				details = dto.details,
				status = dto.status,
				reviewedByUserId = dto.reviewedByUserId,
				dateTimeReviewed = dto.dateTimeReviewed,
				resolutionNotes = dto.resolutionNotes,
				dateTimeCreated = dto.dateTimeCreated,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a MessageFlag Object.
		///
		/// </summary>
		public void ApplyDTO(MessageFlagDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.conversationMessageId = dto.conversationMessageId;
			this.flaggedByUserId = dto.flaggedByUserId;
			this.reason = dto.reason;
			this.details = dto.details;
			this.status = dto.status;
			this.reviewedByUserId = dto.reviewedByUserId;
			this.dateTimeReviewed = dto.dateTimeReviewed;
			this.resolutionNotes = dto.resolutionNotes;
			this.dateTimeCreated = dto.dateTimeCreated;
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
		/// Creates a deep copy clone of a MessageFlag Object.
		///
		/// </summary>
		public MessageFlag Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new MessageFlag{
				id = this.id,
				tenantGuid = this.tenantGuid,
				conversationMessageId = this.conversationMessageId,
				flaggedByUserId = this.flaggedByUserId,
				reason = this.reason,
				details = this.details,
				status = this.status,
				reviewedByUserId = this.reviewedByUserId,
				dateTimeReviewed = this.dateTimeReviewed,
				resolutionNotes = this.resolutionNotes,
				dateTimeCreated = this.dateTimeCreated,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MessageFlag Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MessageFlag Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a MessageFlag Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a MessageFlag Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.MessageFlag messageFlag)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (messageFlag == null)
			{
				return null;
			}

			return new {
				id = messageFlag.id,
				conversationMessageId = messageFlag.conversationMessageId,
				flaggedByUserId = messageFlag.flaggedByUserId,
				reason = messageFlag.reason,
				details = messageFlag.details,
				status = messageFlag.status,
				reviewedByUserId = messageFlag.reviewedByUserId,
				dateTimeReviewed = messageFlag.dateTimeReviewed,
				resolutionNotes = messageFlag.resolutionNotes,
				dateTimeCreated = messageFlag.dateTimeCreated,
				objectGuid = messageFlag.objectGuid,
				active = messageFlag.active,
				deleted = messageFlag.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a MessageFlag Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(MessageFlag messageFlag)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (messageFlag == null)
			{
				return null;
			}

			return new {
				id = messageFlag.id,
				conversationMessageId = messageFlag.conversationMessageId,
				flaggedByUserId = messageFlag.flaggedByUserId,
				reason = messageFlag.reason,
				details = messageFlag.details,
				status = messageFlag.status,
				reviewedByUserId = messageFlag.reviewedByUserId,
				dateTimeReviewed = messageFlag.dateTimeReviewed,
				resolutionNotes = messageFlag.resolutionNotes,
				dateTimeCreated = messageFlag.dateTimeCreated,
				objectGuid = messageFlag.objectGuid,
				active = messageFlag.active,
				deleted = messageFlag.deleted,
				conversationMessage = ConversationMessage.CreateMinimalAnonymous(messageFlag.conversationMessage)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a MessageFlag Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(MessageFlag messageFlag)
		{
			//
			// Return a very minimal object.
			//
			if (messageFlag == null)
			{
				return null;
			}

			return new {
				id = messageFlag.id,
				name = messageFlag.reason,
				description = string.Join(", ", new[] { messageFlag.reason, messageFlag.details, messageFlag.status}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
