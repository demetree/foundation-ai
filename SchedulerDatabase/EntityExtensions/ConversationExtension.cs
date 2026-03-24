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
	public partial class Conversation : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ConversationDTO
		{
			public Int32 id { get; set; }
			public Int32? createdByUserId { get; set; }
			public Int32? conversationTypeId { get; set; }
			[Required]
			public Int32 priority { get; set; }
			[Required]
			public DateTime dateTimeCreated { get; set; }
			public String entity { get; set; }
			public Int32? entityId { get; set; }
			public String externalURL { get; set; }
			public String name { get; set; }
			public String description { get; set; }
			public Boolean? isPublic { get; set; }
			public Int32? userId { get; set; }
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
		public class ConversationOutputDTO : ConversationDTO
		{
			public ConversationType.ConversationTypeDTO conversationType { get; set; }
		}


		/// <summary>
		///
		/// Converts a Conversation to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ConversationDTO ToDTO()
		{
			return new ConversationDTO
			{
				id = this.id,
				createdByUserId = this.createdByUserId,
				conversationTypeId = this.conversationTypeId,
				priority = this.priority,
				dateTimeCreated = this.dateTimeCreated,
				entity = this.entity,
				entityId = this.entityId,
				externalURL = this.externalURL,
				name = this.name,
				description = this.description,
				isPublic = this.isPublic,
				userId = this.userId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a Conversation list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ConversationDTO> ToDTOList(List<Conversation> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConversationDTO> output = new List<ConversationDTO>();

			output.Capacity = data.Count;

			foreach (Conversation conversation in data)
			{
				output.Add(conversation.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a Conversation to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the Conversation Entity type directly.
		///
		/// </summary>
		public ConversationOutputDTO ToOutputDTO()
		{
			return new ConversationOutputDTO
			{
				id = this.id,
				createdByUserId = this.createdByUserId,
				conversationTypeId = this.conversationTypeId,
				priority = this.priority,
				dateTimeCreated = this.dateTimeCreated,
				entity = this.entity,
				entityId = this.entityId,
				externalURL = this.externalURL,
				name = this.name,
				description = this.description,
				isPublic = this.isPublic,
				userId = this.userId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				conversationType = this.conversationType?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a Conversation list to list of Output Data Transfer Object intended to be used for serializing a list of Conversation objects to avoid using the Conversation entity type directly.
		///
		/// </summary>
		public static List<ConversationOutputDTO> ToOutputDTOList(List<Conversation> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConversationOutputDTO> output = new List<ConversationOutputDTO>();

			output.Capacity = data.Count;

			foreach (Conversation conversation in data)
			{
				output.Add(conversation.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a Conversation Object.
		///
		/// </summary>
		public static Database.Conversation FromDTO(ConversationDTO dto)
		{
			return new Database.Conversation
			{
				id = dto.id,
				createdByUserId = dto.createdByUserId,
				conversationTypeId = dto.conversationTypeId,
				priority = dto.priority,
				dateTimeCreated = dto.dateTimeCreated,
				entity = dto.entity,
				entityId = dto.entityId,
				externalURL = dto.externalURL,
				name = dto.name,
				description = dto.description,
				isPublic = dto.isPublic,
				userId = dto.userId,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a Conversation Object.
		///
		/// </summary>
		public void ApplyDTO(ConversationDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.createdByUserId = dto.createdByUserId;
			this.conversationTypeId = dto.conversationTypeId;
			this.priority = dto.priority;
			this.dateTimeCreated = dto.dateTimeCreated;
			this.entity = dto.entity;
			this.entityId = dto.entityId;
			this.externalURL = dto.externalURL;
			this.name = dto.name;
			this.description = dto.description;
			this.isPublic = dto.isPublic;
			this.userId = dto.userId;
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
		/// Creates a deep copy clone of a Conversation Object.
		///
		/// </summary>
		public Conversation Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new Conversation{
				id = this.id,
				tenantGuid = this.tenantGuid,
				createdByUserId = this.createdByUserId,
				conversationTypeId = this.conversationTypeId,
				priority = this.priority,
				dateTimeCreated = this.dateTimeCreated,
				entity = this.entity,
				entityId = this.entityId,
				externalURL = this.externalURL,
				name = this.name,
				description = this.description,
				isPublic = this.isPublic,
				userId = this.userId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Conversation Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Conversation Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a Conversation Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a Conversation Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.Conversation conversation)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (conversation == null)
			{
				return null;
			}

			return new {
				id = conversation.id,
				createdByUserId = conversation.createdByUserId,
				conversationTypeId = conversation.conversationTypeId,
				priority = conversation.priority,
				dateTimeCreated = conversation.dateTimeCreated,
				entity = conversation.entity,
				entityId = conversation.entityId,
				externalURL = conversation.externalURL,
				name = conversation.name,
				description = conversation.description,
				isPublic = conversation.isPublic,
				userId = conversation.userId,
				objectGuid = conversation.objectGuid,
				active = conversation.active,
				deleted = conversation.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a Conversation Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(Conversation conversation)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (conversation == null)
			{
				return null;
			}

			return new {
				id = conversation.id,
				createdByUserId = conversation.createdByUserId,
				conversationTypeId = conversation.conversationTypeId,
				priority = conversation.priority,
				dateTimeCreated = conversation.dateTimeCreated,
				entity = conversation.entity,
				entityId = conversation.entityId,
				externalURL = conversation.externalURL,
				name = conversation.name,
				description = conversation.description,
				isPublic = conversation.isPublic,
				userId = conversation.userId,
				objectGuid = conversation.objectGuid,
				active = conversation.active,
				deleted = conversation.deleted,
				conversationType = ConversationType.CreateMinimalAnonymous(conversation.conversationType)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a Conversation Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(Conversation conversation)
		{
			//
			// Return a very minimal object.
			//
			if (conversation == null)
			{
				return null;
			}

			return new {
				id = conversation.id,
				name = conversation.name,
				description = conversation.description,
			 };
		}
	}
}
