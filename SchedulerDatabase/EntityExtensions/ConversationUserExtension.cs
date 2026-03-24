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
	public partial class ConversationUser : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ConversationUserDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 conversationId { get; set; }
			[Required]
			public Int32 userId { get; set; }
			[Required]
			public String role { get; set; }
			[Required]
			public DateTime dateTimeAdded { get; set; }
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
		public class ConversationUserOutputDTO : ConversationUserDTO
		{
			public Conversation.ConversationDTO conversation { get; set; }
		}


		/// <summary>
		///
		/// Converts a ConversationUser to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ConversationUserDTO ToDTO()
		{
			return new ConversationUserDTO
			{
				id = this.id,
				conversationId = this.conversationId,
				userId = this.userId,
				role = this.role,
				dateTimeAdded = this.dateTimeAdded,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ConversationUser list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ConversationUserDTO> ToDTOList(List<ConversationUser> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConversationUserDTO> output = new List<ConversationUserDTO>();

			output.Capacity = data.Count;

			foreach (ConversationUser conversationUser in data)
			{
				output.Add(conversationUser.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ConversationUser to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ConversationUser Entity type directly.
		///
		/// </summary>
		public ConversationUserOutputDTO ToOutputDTO()
		{
			return new ConversationUserOutputDTO
			{
				id = this.id,
				conversationId = this.conversationId,
				userId = this.userId,
				role = this.role,
				dateTimeAdded = this.dateTimeAdded,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				conversation = this.conversation?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ConversationUser list to list of Output Data Transfer Object intended to be used for serializing a list of ConversationUser objects to avoid using the ConversationUser entity type directly.
		///
		/// </summary>
		public static List<ConversationUserOutputDTO> ToOutputDTOList(List<ConversationUser> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConversationUserOutputDTO> output = new List<ConversationUserOutputDTO>();

			output.Capacity = data.Count;

			foreach (ConversationUser conversationUser in data)
			{
				output.Add(conversationUser.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ConversationUser Object.
		///
		/// </summary>
		public static Database.ConversationUser FromDTO(ConversationUserDTO dto)
		{
			return new Database.ConversationUser
			{
				id = dto.id,
				conversationId = dto.conversationId,
				userId = dto.userId,
				role = dto.role,
				dateTimeAdded = dto.dateTimeAdded,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ConversationUser Object.
		///
		/// </summary>
		public void ApplyDTO(ConversationUserDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.conversationId = dto.conversationId;
			this.userId = dto.userId;
			this.role = dto.role;
			this.dateTimeAdded = dto.dateTimeAdded;
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
		/// Creates a deep copy clone of a ConversationUser Object.
		///
		/// </summary>
		public ConversationUser Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ConversationUser{
				id = this.id,
				tenantGuid = this.tenantGuid,
				conversationId = this.conversationId,
				userId = this.userId,
				role = this.role,
				dateTimeAdded = this.dateTimeAdded,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConversationUser Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConversationUser Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ConversationUser Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ConversationUser Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ConversationUser conversationUser)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (conversationUser == null)
			{
				return null;
			}

			return new {
				id = conversationUser.id,
				conversationId = conversationUser.conversationId,
				userId = conversationUser.userId,
				role = conversationUser.role,
				dateTimeAdded = conversationUser.dateTimeAdded,
				objectGuid = conversationUser.objectGuid,
				active = conversationUser.active,
				deleted = conversationUser.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ConversationUser Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ConversationUser conversationUser)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (conversationUser == null)
			{
				return null;
			}

			return new {
				id = conversationUser.id,
				conversationId = conversationUser.conversationId,
				userId = conversationUser.userId,
				role = conversationUser.role,
				dateTimeAdded = conversationUser.dateTimeAdded,
				objectGuid = conversationUser.objectGuid,
				active = conversationUser.active,
				deleted = conversationUser.deleted,
				conversation = Conversation.CreateMinimalAnonymous(conversationUser.conversation)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ConversationUser Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ConversationUser conversationUser)
		{
			//
			// Return a very minimal object.
			//
			if (conversationUser == null)
			{
				return null;
			}

			return new {
				id = conversationUser.id,
				name = conversationUser.role,
				description = string.Join(", ", new[] { conversationUser.role}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
