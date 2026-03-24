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
	public partial class MessageBookmark : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class MessageBookmarkDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 userId { get; set; }
			[Required]
			public Int32 conversationMessageId { get; set; }
			public String note { get; set; }
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
		public class MessageBookmarkOutputDTO : MessageBookmarkDTO
		{
			public ConversationMessage.ConversationMessageDTO conversationMessage { get; set; }
		}


		/// <summary>
		///
		/// Converts a MessageBookmark to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public MessageBookmarkDTO ToDTO()
		{
			return new MessageBookmarkDTO
			{
				id = this.id,
				userId = this.userId,
				conversationMessageId = this.conversationMessageId,
				note = this.note,
				dateTimeCreated = this.dateTimeCreated,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a MessageBookmark list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<MessageBookmarkDTO> ToDTOList(List<MessageBookmark> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MessageBookmarkDTO> output = new List<MessageBookmarkDTO>();

			output.Capacity = data.Count;

			foreach (MessageBookmark messageBookmark in data)
			{
				output.Add(messageBookmark.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a MessageBookmark to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the MessageBookmark Entity type directly.
		///
		/// </summary>
		public MessageBookmarkOutputDTO ToOutputDTO()
		{
			return new MessageBookmarkOutputDTO
			{
				id = this.id,
				userId = this.userId,
				conversationMessageId = this.conversationMessageId,
				note = this.note,
				dateTimeCreated = this.dateTimeCreated,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				conversationMessage = this.conversationMessage?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a MessageBookmark list to list of Output Data Transfer Object intended to be used for serializing a list of MessageBookmark objects to avoid using the MessageBookmark entity type directly.
		///
		/// </summary>
		public static List<MessageBookmarkOutputDTO> ToOutputDTOList(List<MessageBookmark> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MessageBookmarkOutputDTO> output = new List<MessageBookmarkOutputDTO>();

			output.Capacity = data.Count;

			foreach (MessageBookmark messageBookmark in data)
			{
				output.Add(messageBookmark.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a MessageBookmark Object.
		///
		/// </summary>
		public static Database.MessageBookmark FromDTO(MessageBookmarkDTO dto)
		{
			return new Database.MessageBookmark
			{
				id = dto.id,
				userId = dto.userId,
				conversationMessageId = dto.conversationMessageId,
				note = dto.note,
				dateTimeCreated = dto.dateTimeCreated,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a MessageBookmark Object.
		///
		/// </summary>
		public void ApplyDTO(MessageBookmarkDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.userId = dto.userId;
			this.conversationMessageId = dto.conversationMessageId;
			this.note = dto.note;
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
		/// Creates a deep copy clone of a MessageBookmark Object.
		///
		/// </summary>
		public MessageBookmark Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new MessageBookmark{
				id = this.id,
				tenantGuid = this.tenantGuid,
				userId = this.userId,
				conversationMessageId = this.conversationMessageId,
				note = this.note,
				dateTimeCreated = this.dateTimeCreated,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MessageBookmark Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MessageBookmark Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a MessageBookmark Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a MessageBookmark Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.MessageBookmark messageBookmark)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (messageBookmark == null)
			{
				return null;
			}

			return new {
				id = messageBookmark.id,
				userId = messageBookmark.userId,
				conversationMessageId = messageBookmark.conversationMessageId,
				note = messageBookmark.note,
				dateTimeCreated = messageBookmark.dateTimeCreated,
				objectGuid = messageBookmark.objectGuid,
				active = messageBookmark.active,
				deleted = messageBookmark.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a MessageBookmark Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(MessageBookmark messageBookmark)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (messageBookmark == null)
			{
				return null;
			}

			return new {
				id = messageBookmark.id,
				userId = messageBookmark.userId,
				conversationMessageId = messageBookmark.conversationMessageId,
				note = messageBookmark.note,
				dateTimeCreated = messageBookmark.dateTimeCreated,
				objectGuid = messageBookmark.objectGuid,
				active = messageBookmark.active,
				deleted = messageBookmark.deleted,
				conversationMessage = ConversationMessage.CreateMinimalAnonymous(messageBookmark.conversationMessage)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a MessageBookmark Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(MessageBookmark messageBookmark)
		{
			//
			// Return a very minimal object.
			//
			if (messageBookmark == null)
			{
				return null;
			}

			return new {
				id = messageBookmark.id,
				name = messageBookmark.note,
				description = string.Join(", ", new[] { messageBookmark.note}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
