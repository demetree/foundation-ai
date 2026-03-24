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
	public partial class ConversationMessageUser : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ConversationMessageUserDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 conversationMessageId { get; set; }
			[Required]
			public Int32 userId { get; set; }
			[Required]
			public DateTime dateTimeCreated { get; set; }
			[Required]
			public Boolean acknowledged { get; set; }
			[Required]
			public DateTime dateTimeAcknowledged { get; set; }
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
		public class ConversationMessageUserOutputDTO : ConversationMessageUserDTO
		{
			public ConversationMessage.ConversationMessageDTO conversationMessage { get; set; }
		}


		/// <summary>
		///
		/// Converts a ConversationMessageUser to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ConversationMessageUserDTO ToDTO()
		{
			return new ConversationMessageUserDTO
			{
				id = this.id,
				conversationMessageId = this.conversationMessageId,
				userId = this.userId,
				dateTimeCreated = this.dateTimeCreated,
				acknowledged = this.acknowledged,
				dateTimeAcknowledged = this.dateTimeAcknowledged,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ConversationMessageUser list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ConversationMessageUserDTO> ToDTOList(List<ConversationMessageUser> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConversationMessageUserDTO> output = new List<ConversationMessageUserDTO>();

			output.Capacity = data.Count;

			foreach (ConversationMessageUser conversationMessageUser in data)
			{
				output.Add(conversationMessageUser.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ConversationMessageUser to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ConversationMessageUser Entity type directly.
		///
		/// </summary>
		public ConversationMessageUserOutputDTO ToOutputDTO()
		{
			return new ConversationMessageUserOutputDTO
			{
				id = this.id,
				conversationMessageId = this.conversationMessageId,
				userId = this.userId,
				dateTimeCreated = this.dateTimeCreated,
				acknowledged = this.acknowledged,
				dateTimeAcknowledged = this.dateTimeAcknowledged,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				conversationMessage = this.conversationMessage?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ConversationMessageUser list to list of Output Data Transfer Object intended to be used for serializing a list of ConversationMessageUser objects to avoid using the ConversationMessageUser entity type directly.
		///
		/// </summary>
		public static List<ConversationMessageUserOutputDTO> ToOutputDTOList(List<ConversationMessageUser> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConversationMessageUserOutputDTO> output = new List<ConversationMessageUserOutputDTO>();

			output.Capacity = data.Count;

			foreach (ConversationMessageUser conversationMessageUser in data)
			{
				output.Add(conversationMessageUser.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ConversationMessageUser Object.
		///
		/// </summary>
		public static Database.ConversationMessageUser FromDTO(ConversationMessageUserDTO dto)
		{
			return new Database.ConversationMessageUser
			{
				id = dto.id,
				conversationMessageId = dto.conversationMessageId,
				userId = dto.userId,
				dateTimeCreated = dto.dateTimeCreated,
				acknowledged = dto.acknowledged,
				dateTimeAcknowledged = dto.dateTimeAcknowledged,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ConversationMessageUser Object.
		///
		/// </summary>
		public void ApplyDTO(ConversationMessageUserDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.conversationMessageId = dto.conversationMessageId;
			this.userId = dto.userId;
			this.dateTimeCreated = dto.dateTimeCreated;
			this.acknowledged = dto.acknowledged;
			this.dateTimeAcknowledged = dto.dateTimeAcknowledged;
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
		/// Creates a deep copy clone of a ConversationMessageUser Object.
		///
		/// </summary>
		public ConversationMessageUser Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ConversationMessageUser{
				id = this.id,
				tenantGuid = this.tenantGuid,
				conversationMessageId = this.conversationMessageId,
				userId = this.userId,
				dateTimeCreated = this.dateTimeCreated,
				acknowledged = this.acknowledged,
				dateTimeAcknowledged = this.dateTimeAcknowledged,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConversationMessageUser Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConversationMessageUser Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ConversationMessageUser Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ConversationMessageUser Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ConversationMessageUser conversationMessageUser)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (conversationMessageUser == null)
			{
				return null;
			}

			return new {
				id = conversationMessageUser.id,
				conversationMessageId = conversationMessageUser.conversationMessageId,
				userId = conversationMessageUser.userId,
				dateTimeCreated = conversationMessageUser.dateTimeCreated,
				acknowledged = conversationMessageUser.acknowledged,
				dateTimeAcknowledged = conversationMessageUser.dateTimeAcknowledged,
				objectGuid = conversationMessageUser.objectGuid,
				active = conversationMessageUser.active,
				deleted = conversationMessageUser.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ConversationMessageUser Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ConversationMessageUser conversationMessageUser)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (conversationMessageUser == null)
			{
				return null;
			}

			return new {
				id = conversationMessageUser.id,
				conversationMessageId = conversationMessageUser.conversationMessageId,
				userId = conversationMessageUser.userId,
				dateTimeCreated = conversationMessageUser.dateTimeCreated,
				acknowledged = conversationMessageUser.acknowledged,
				dateTimeAcknowledged = conversationMessageUser.dateTimeAcknowledged,
				objectGuid = conversationMessageUser.objectGuid,
				active = conversationMessageUser.active,
				deleted = conversationMessageUser.deleted,
				conversationMessage = ConversationMessage.CreateMinimalAnonymous(conversationMessageUser.conversationMessage)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ConversationMessageUser Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ConversationMessageUser conversationMessageUser)
		{
			//
			// Return a very minimal object.
			//
			if (conversationMessageUser == null)
			{
				return null;
			}

			return new {
				id = conversationMessageUser.id,
				name = conversationMessageUser.id,
				description = conversationMessageUser.id
			 };
		}
	}
}
