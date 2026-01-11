using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Security.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class EntityDataTokenEvent : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class EntityDataTokenEventDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 entityDataTokenId { get; set; }
			[Required]
			public Int32 entityDataTokenEventTypeId { get; set; }
			[Required]
			public DateTime timeStamp { get; set; }
			public String comments { get; set; }
			public Boolean? active { get; set; }
			public Boolean? deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class EntityDataTokenEventOutputDTO : EntityDataTokenEventDTO
		{
			public EntityDataToken.EntityDataTokenDTO entityDataToken { get; set; }
			public EntityDataTokenEventType.EntityDataTokenEventTypeDTO entityDataTokenEventType { get; set; }
		}


		/// <summary>
		///
		/// Converts a EntityDataTokenEvent to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public EntityDataTokenEventDTO ToDTO()
		{
			return new EntityDataTokenEventDTO
			{
				id = this.id,
				entityDataTokenId = this.entityDataTokenId,
				entityDataTokenEventTypeId = this.entityDataTokenEventTypeId,
				timeStamp = this.timeStamp,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a EntityDataTokenEvent list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<EntityDataTokenEventDTO> ToDTOList(List<EntityDataTokenEvent> data)
		{
			if (data == null)
			{
				return null;
			}

			List<EntityDataTokenEventDTO> output = new List<EntityDataTokenEventDTO>();

			output.Capacity = data.Count;

			foreach (EntityDataTokenEvent entityDataTokenEvent in data)
			{
				output.Add(entityDataTokenEvent.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a EntityDataTokenEvent to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the EntityDataTokenEventEntity type directly.
		///
		/// </summary>
		public EntityDataTokenEventOutputDTO ToOutputDTO()
		{
			return new EntityDataTokenEventOutputDTO
			{
				id = this.id,
				entityDataTokenId = this.entityDataTokenId,
				entityDataTokenEventTypeId = this.entityDataTokenEventTypeId,
				timeStamp = this.timeStamp,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted,
				entityDataToken = this.entityDataToken?.ToDTO(),
				entityDataTokenEventType = this.entityDataTokenEventType?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a EntityDataTokenEvent list to list of Output Data Transfer Object intended to be used for serializing a list of EntityDataTokenEvent objects to avoid using the EntityDataTokenEvent entity type directly.
		///
		/// </summary>
		public static List<EntityDataTokenEventOutputDTO> ToOutputDTOList(List<EntityDataTokenEvent> data)
		{
			if (data == null)
			{
				return null;
			}

			List<EntityDataTokenEventOutputDTO> output = new List<EntityDataTokenEventOutputDTO>();

			output.Capacity = data.Count;

			foreach (EntityDataTokenEvent entityDataTokenEvent in data)
			{
				output.Add(entityDataTokenEvent.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a EntityDataTokenEvent Object.
		///
		/// </summary>
		public static Database.EntityDataTokenEvent FromDTO(EntityDataTokenEventDTO dto)
		{
			return new Database.EntityDataTokenEvent
			{
				id = dto.id,
				entityDataTokenId = dto.entityDataTokenId,
				entityDataTokenEventTypeId = dto.entityDataTokenEventTypeId,
				timeStamp = dto.timeStamp,
				comments = dto.comments,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a EntityDataTokenEvent Object.
		///
		/// </summary>
		public void ApplyDTO(EntityDataTokenEventDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.entityDataTokenId = dto.entityDataTokenId;
			this.entityDataTokenEventTypeId = dto.entityDataTokenEventTypeId;
			this.timeStamp = dto.timeStamp;
			this.comments = dto.comments;
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
		/// Creates a deep copy clone of a EntityDataTokenEvent Object.
		///
		/// </summary>
		public EntityDataTokenEvent Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new EntityDataTokenEvent{
				id = this.id,
				entityDataTokenId = this.entityDataTokenId,
				entityDataTokenEventTypeId = this.entityDataTokenEventTypeId,
				timeStamp = this.timeStamp,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a EntityDataTokenEvent Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a EntityDataTokenEvent Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a EntityDataTokenEvent Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a EntityDataTokenEvent Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.EntityDataTokenEvent entityDataTokenEvent)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (entityDataTokenEvent == null)
			{
				return null;
			}

			return new {
				id = entityDataTokenEvent.id,
				entityDataTokenId = entityDataTokenEvent.entityDataTokenId,
				entityDataTokenEventTypeId = entityDataTokenEvent.entityDataTokenEventTypeId,
				timeStamp = entityDataTokenEvent.timeStamp,
				comments = entityDataTokenEvent.comments,
				active = entityDataTokenEvent.active,
				deleted = entityDataTokenEvent.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a EntityDataTokenEvent Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(EntityDataTokenEvent entityDataTokenEvent)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (entityDataTokenEvent == null)
			{
				return null;
			}

			return new {
				id = entityDataTokenEvent.id,
				entityDataTokenId = entityDataTokenEvent.entityDataTokenId,
				entityDataTokenEventTypeId = entityDataTokenEvent.entityDataTokenEventTypeId,
				timeStamp = entityDataTokenEvent.timeStamp,
				comments = entityDataTokenEvent.comments,
				active = entityDataTokenEvent.active,
				deleted = entityDataTokenEvent.deleted,
				entityDataToken = EntityDataToken.CreateMinimalAnonymous(entityDataTokenEvent.entityDataToken),
				entityDataTokenEventType = EntityDataTokenEventType.CreateMinimalAnonymous(entityDataTokenEvent.entityDataTokenEventType)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a EntityDataTokenEvent Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(EntityDataTokenEvent entityDataTokenEvent)
		{
			//
			// Return a very minimal object.
			//
			if (entityDataTokenEvent == null)
			{
				return null;
			}

			return new {
				id = entityDataTokenEvent.id,
				name = entityDataTokenEvent.comments,
				description = string.Join(", ", new[] { entityDataTokenEvent.comments}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
