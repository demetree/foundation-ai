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
	public partial class ConversationThreadUser : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ConversationThreadUserDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 conversationId { get; set; }
			[Required]
			public Int32 parentConversationMessageId { get; set; }
			[Required]
			public Int32 userId { get; set; }
			public Int32? lastReadMessageId { get; set; }
			public DateTime? lastReadDateTime { get; set; }
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
		public class ConversationThreadUserOutputDTO : ConversationThreadUserDTO
		{
			public Conversation.ConversationDTO conversation { get; set; }
			public ConversationMessage.ConversationMessageDTO parentConversationMessage { get; set; }
		}


		/// <summary>
		///
		/// Converts a ConversationThreadUser to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ConversationThreadUserDTO ToDTO()
		{
			return new ConversationThreadUserDTO
			{
				id = this.id,
				conversationId = this.conversationId,
				parentConversationMessageId = this.parentConversationMessageId,
				userId = this.userId,
				lastReadMessageId = this.lastReadMessageId,
				lastReadDateTime = this.lastReadDateTime,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ConversationThreadUser list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ConversationThreadUserDTO> ToDTOList(List<ConversationThreadUser> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConversationThreadUserDTO> output = new List<ConversationThreadUserDTO>();

			output.Capacity = data.Count;

			foreach (ConversationThreadUser conversationThreadUser in data)
			{
				output.Add(conversationThreadUser.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ConversationThreadUser to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ConversationThreadUser Entity type directly.
		///
		/// </summary>
		public ConversationThreadUserOutputDTO ToOutputDTO()
		{
			return new ConversationThreadUserOutputDTO
			{
				id = this.id,
				conversationId = this.conversationId,
				parentConversationMessageId = this.parentConversationMessageId,
				userId = this.userId,
				lastReadMessageId = this.lastReadMessageId,
				lastReadDateTime = this.lastReadDateTime,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				conversation = this.conversation?.ToDTO(),
				parentConversationMessage = this.parentConversationMessage?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ConversationThreadUser list to list of Output Data Transfer Object intended to be used for serializing a list of ConversationThreadUser objects to avoid using the ConversationThreadUser entity type directly.
		///
		/// </summary>
		public static List<ConversationThreadUserOutputDTO> ToOutputDTOList(List<ConversationThreadUser> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConversationThreadUserOutputDTO> output = new List<ConversationThreadUserOutputDTO>();

			output.Capacity = data.Count;

			foreach (ConversationThreadUser conversationThreadUser in data)
			{
				output.Add(conversationThreadUser.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ConversationThreadUser Object.
		///
		/// </summary>
		public static Database.ConversationThreadUser FromDTO(ConversationThreadUserDTO dto)
		{
			return new Database.ConversationThreadUser
			{
				id = dto.id,
				conversationId = dto.conversationId,
				parentConversationMessageId = dto.parentConversationMessageId,
				userId = dto.userId,
				lastReadMessageId = dto.lastReadMessageId,
				lastReadDateTime = dto.lastReadDateTime,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ConversationThreadUser Object.
		///
		/// </summary>
		public void ApplyDTO(ConversationThreadUserDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.conversationId = dto.conversationId;
			this.parentConversationMessageId = dto.parentConversationMessageId;
			this.userId = dto.userId;
			this.lastReadMessageId = dto.lastReadMessageId;
			this.lastReadDateTime = dto.lastReadDateTime;
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
		/// Creates a deep copy clone of a ConversationThreadUser Object.
		///
		/// </summary>
		public ConversationThreadUser Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ConversationThreadUser{
				id = this.id,
				tenantGuid = this.tenantGuid,
				conversationId = this.conversationId,
				parentConversationMessageId = this.parentConversationMessageId,
				userId = this.userId,
				lastReadMessageId = this.lastReadMessageId,
				lastReadDateTime = this.lastReadDateTime,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConversationThreadUser Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConversationThreadUser Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ConversationThreadUser Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ConversationThreadUser Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ConversationThreadUser conversationThreadUser)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (conversationThreadUser == null)
			{
				return null;
			}

			return new {
				id = conversationThreadUser.id,
				conversationId = conversationThreadUser.conversationId,
				parentConversationMessageId = conversationThreadUser.parentConversationMessageId,
				userId = conversationThreadUser.userId,
				lastReadMessageId = conversationThreadUser.lastReadMessageId,
				lastReadDateTime = conversationThreadUser.lastReadDateTime,
				objectGuid = conversationThreadUser.objectGuid,
				active = conversationThreadUser.active,
				deleted = conversationThreadUser.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ConversationThreadUser Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ConversationThreadUser conversationThreadUser)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (conversationThreadUser == null)
			{
				return null;
			}

			return new {
				id = conversationThreadUser.id,
				conversationId = conversationThreadUser.conversationId,
				parentConversationMessageId = conversationThreadUser.parentConversationMessageId,
				userId = conversationThreadUser.userId,
				lastReadMessageId = conversationThreadUser.lastReadMessageId,
				lastReadDateTime = conversationThreadUser.lastReadDateTime,
				objectGuid = conversationThreadUser.objectGuid,
				active = conversationThreadUser.active,
				deleted = conversationThreadUser.deleted,
				conversation = Conversation.CreateMinimalAnonymous(conversationThreadUser.conversation),
				parentConversationMessage = ConversationMessage.CreateMinimalAnonymous(conversationThreadUser.parentConversationMessage)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ConversationThreadUser Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ConversationThreadUser conversationThreadUser)
		{
			//
			// Return a very minimal object.
			//
			if (conversationThreadUser == null)
			{
				return null;
			}

			return new {
				id = conversationThreadUser.id,
				name = conversationThreadUser.id,
				description = conversationThreadUser.id
			 };
		}
	}
}
