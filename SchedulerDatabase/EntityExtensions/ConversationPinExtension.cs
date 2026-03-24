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
	public partial class ConversationPin : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ConversationPinDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 conversationId { get; set; }
			[Required]
			public Int32 conversationMessageId { get; set; }
			[Required]
			public Int32 pinnedByUserId { get; set; }
			[Required]
			public DateTime dateTimePinned { get; set; }
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
		public class ConversationPinOutputDTO : ConversationPinDTO
		{
			public Conversation.ConversationDTO conversation { get; set; }
			public ConversationMessage.ConversationMessageDTO conversationMessage { get; set; }
		}


		/// <summary>
		///
		/// Converts a ConversationPin to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ConversationPinDTO ToDTO()
		{
			return new ConversationPinDTO
			{
				id = this.id,
				conversationId = this.conversationId,
				conversationMessageId = this.conversationMessageId,
				pinnedByUserId = this.pinnedByUserId,
				dateTimePinned = this.dateTimePinned,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ConversationPin list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ConversationPinDTO> ToDTOList(List<ConversationPin> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConversationPinDTO> output = new List<ConversationPinDTO>();

			output.Capacity = data.Count;

			foreach (ConversationPin conversationPin in data)
			{
				output.Add(conversationPin.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ConversationPin to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ConversationPin Entity type directly.
		///
		/// </summary>
		public ConversationPinOutputDTO ToOutputDTO()
		{
			return new ConversationPinOutputDTO
			{
				id = this.id,
				conversationId = this.conversationId,
				conversationMessageId = this.conversationMessageId,
				pinnedByUserId = this.pinnedByUserId,
				dateTimePinned = this.dateTimePinned,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				conversation = this.conversation?.ToDTO(),
				conversationMessage = this.conversationMessage?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ConversationPin list to list of Output Data Transfer Object intended to be used for serializing a list of ConversationPin objects to avoid using the ConversationPin entity type directly.
		///
		/// </summary>
		public static List<ConversationPinOutputDTO> ToOutputDTOList(List<ConversationPin> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConversationPinOutputDTO> output = new List<ConversationPinOutputDTO>();

			output.Capacity = data.Count;

			foreach (ConversationPin conversationPin in data)
			{
				output.Add(conversationPin.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ConversationPin Object.
		///
		/// </summary>
		public static Database.ConversationPin FromDTO(ConversationPinDTO dto)
		{
			return new Database.ConversationPin
			{
				id = dto.id,
				conversationId = dto.conversationId,
				conversationMessageId = dto.conversationMessageId,
				pinnedByUserId = dto.pinnedByUserId,
				dateTimePinned = dto.dateTimePinned,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ConversationPin Object.
		///
		/// </summary>
		public void ApplyDTO(ConversationPinDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.conversationId = dto.conversationId;
			this.conversationMessageId = dto.conversationMessageId;
			this.pinnedByUserId = dto.pinnedByUserId;
			this.dateTimePinned = dto.dateTimePinned;
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
		/// Creates a deep copy clone of a ConversationPin Object.
		///
		/// </summary>
		public ConversationPin Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ConversationPin{
				id = this.id,
				tenantGuid = this.tenantGuid,
				conversationId = this.conversationId,
				conversationMessageId = this.conversationMessageId,
				pinnedByUserId = this.pinnedByUserId,
				dateTimePinned = this.dateTimePinned,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConversationPin Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConversationPin Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ConversationPin Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ConversationPin Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ConversationPin conversationPin)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (conversationPin == null)
			{
				return null;
			}

			return new {
				id = conversationPin.id,
				conversationId = conversationPin.conversationId,
				conversationMessageId = conversationPin.conversationMessageId,
				pinnedByUserId = conversationPin.pinnedByUserId,
				dateTimePinned = conversationPin.dateTimePinned,
				objectGuid = conversationPin.objectGuid,
				active = conversationPin.active,
				deleted = conversationPin.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ConversationPin Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ConversationPin conversationPin)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (conversationPin == null)
			{
				return null;
			}

			return new {
				id = conversationPin.id,
				conversationId = conversationPin.conversationId,
				conversationMessageId = conversationPin.conversationMessageId,
				pinnedByUserId = conversationPin.pinnedByUserId,
				dateTimePinned = conversationPin.dateTimePinned,
				objectGuid = conversationPin.objectGuid,
				active = conversationPin.active,
				deleted = conversationPin.deleted,
				conversation = Conversation.CreateMinimalAnonymous(conversationPin.conversation),
				conversationMessage = ConversationMessage.CreateMinimalAnonymous(conversationPin.conversationMessage)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ConversationPin Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ConversationPin conversationPin)
		{
			//
			// Return a very minimal object.
			//
			if (conversationPin == null)
			{
				return null;
			}

			return new {
				id = conversationPin.id,
				name = conversationPin.id,
				description = conversationPin.id
			 };
		}
	}
}
