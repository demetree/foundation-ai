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
	public partial class ConversationMessageReaction : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ConversationMessageReactionDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 conversationMessageId { get; set; }
			[Required]
			public Int32 userId { get; set; }
			[Required]
			public String reaction { get; set; }
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
		public class ConversationMessageReactionOutputDTO : ConversationMessageReactionDTO
		{
			public ConversationMessage.ConversationMessageDTO conversationMessage { get; set; }
		}


		/// <summary>
		///
		/// Converts a ConversationMessageReaction to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ConversationMessageReactionDTO ToDTO()
		{
			return new ConversationMessageReactionDTO
			{
				id = this.id,
				conversationMessageId = this.conversationMessageId,
				userId = this.userId,
				reaction = this.reaction,
				dateTimeCreated = this.dateTimeCreated,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ConversationMessageReaction list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ConversationMessageReactionDTO> ToDTOList(List<ConversationMessageReaction> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConversationMessageReactionDTO> output = new List<ConversationMessageReactionDTO>();

			output.Capacity = data.Count;

			foreach (ConversationMessageReaction conversationMessageReaction in data)
			{
				output.Add(conversationMessageReaction.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ConversationMessageReaction to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ConversationMessageReaction Entity type directly.
		///
		/// </summary>
		public ConversationMessageReactionOutputDTO ToOutputDTO()
		{
			return new ConversationMessageReactionOutputDTO
			{
				id = this.id,
				conversationMessageId = this.conversationMessageId,
				userId = this.userId,
				reaction = this.reaction,
				dateTimeCreated = this.dateTimeCreated,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				conversationMessage = this.conversationMessage?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ConversationMessageReaction list to list of Output Data Transfer Object intended to be used for serializing a list of ConversationMessageReaction objects to avoid using the ConversationMessageReaction entity type directly.
		///
		/// </summary>
		public static List<ConversationMessageReactionOutputDTO> ToOutputDTOList(List<ConversationMessageReaction> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConversationMessageReactionOutputDTO> output = new List<ConversationMessageReactionOutputDTO>();

			output.Capacity = data.Count;

			foreach (ConversationMessageReaction conversationMessageReaction in data)
			{
				output.Add(conversationMessageReaction.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ConversationMessageReaction Object.
		///
		/// </summary>
		public static Database.ConversationMessageReaction FromDTO(ConversationMessageReactionDTO dto)
		{
			return new Database.ConversationMessageReaction
			{
				id = dto.id,
				conversationMessageId = dto.conversationMessageId,
				userId = dto.userId,
				reaction = dto.reaction,
				dateTimeCreated = dto.dateTimeCreated,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ConversationMessageReaction Object.
		///
		/// </summary>
		public void ApplyDTO(ConversationMessageReactionDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.conversationMessageId = dto.conversationMessageId;
			this.userId = dto.userId;
			this.reaction = dto.reaction;
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
		/// Creates a deep copy clone of a ConversationMessageReaction Object.
		///
		/// </summary>
		public ConversationMessageReaction Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ConversationMessageReaction{
				id = this.id,
				tenantGuid = this.tenantGuid,
				conversationMessageId = this.conversationMessageId,
				userId = this.userId,
				reaction = this.reaction,
				dateTimeCreated = this.dateTimeCreated,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConversationMessageReaction Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConversationMessageReaction Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ConversationMessageReaction Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ConversationMessageReaction Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ConversationMessageReaction conversationMessageReaction)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (conversationMessageReaction == null)
			{
				return null;
			}

			return new {
				id = conversationMessageReaction.id,
				conversationMessageId = conversationMessageReaction.conversationMessageId,
				userId = conversationMessageReaction.userId,
				reaction = conversationMessageReaction.reaction,
				dateTimeCreated = conversationMessageReaction.dateTimeCreated,
				objectGuid = conversationMessageReaction.objectGuid,
				active = conversationMessageReaction.active,
				deleted = conversationMessageReaction.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ConversationMessageReaction Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ConversationMessageReaction conversationMessageReaction)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (conversationMessageReaction == null)
			{
				return null;
			}

			return new {
				id = conversationMessageReaction.id,
				conversationMessageId = conversationMessageReaction.conversationMessageId,
				userId = conversationMessageReaction.userId,
				reaction = conversationMessageReaction.reaction,
				dateTimeCreated = conversationMessageReaction.dateTimeCreated,
				objectGuid = conversationMessageReaction.objectGuid,
				active = conversationMessageReaction.active,
				deleted = conversationMessageReaction.deleted,
				conversationMessage = ConversationMessage.CreateMinimalAnonymous(conversationMessageReaction.conversationMessage)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ConversationMessageReaction Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ConversationMessageReaction conversationMessageReaction)
		{
			//
			// Return a very minimal object.
			//
			if (conversationMessageReaction == null)
			{
				return null;
			}

			return new {
				id = conversationMessageReaction.id,
				name = conversationMessageReaction.reaction,
				description = string.Join(", ", new[] { conversationMessageReaction.reaction}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
