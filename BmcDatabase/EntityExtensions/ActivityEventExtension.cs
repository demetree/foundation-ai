using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.BMC.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class ActivityEvent : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ActivityEventDTO
		{
			public Int64 id { get; set; }
			[Required]
			public Int32 activityEventTypeId { get; set; }
			[Required]
			public String title { get; set; }
			public String description { get; set; }
			public String relatedEntityType { get; set; }
			public Int64? relatedEntityId { get; set; }
			[Required]
			public DateTime eventDate { get; set; }
			[Required]
			public Boolean isPublic { get; set; }
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
		public class ActivityEventOutputDTO : ActivityEventDTO
		{
			public ActivityEventType.ActivityEventTypeDTO activityEventType { get; set; }
		}


		/// <summary>
		///
		/// Converts a ActivityEvent to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ActivityEventDTO ToDTO()
		{
			return new ActivityEventDTO
			{
				id = this.id,
				activityEventTypeId = this.activityEventTypeId,
				title = this.title,
				description = this.description,
				relatedEntityType = this.relatedEntityType,
				relatedEntityId = this.relatedEntityId,
				eventDate = this.eventDate,
				isPublic = this.isPublic,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ActivityEvent list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ActivityEventDTO> ToDTOList(List<ActivityEvent> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ActivityEventDTO> output = new List<ActivityEventDTO>();

			output.Capacity = data.Count;

			foreach (ActivityEvent activityEvent in data)
			{
				output.Add(activityEvent.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ActivityEvent to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ActivityEventEntity type directly.
		///
		/// </summary>
		public ActivityEventOutputDTO ToOutputDTO()
		{
			return new ActivityEventOutputDTO
			{
				id = this.id,
				activityEventTypeId = this.activityEventTypeId,
				title = this.title,
				description = this.description,
				relatedEntityType = this.relatedEntityType,
				relatedEntityId = this.relatedEntityId,
				eventDate = this.eventDate,
				isPublic = this.isPublic,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				activityEventType = this.activityEventType?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ActivityEvent list to list of Output Data Transfer Object intended to be used for serializing a list of ActivityEvent objects to avoid using the ActivityEvent entity type directly.
		///
		/// </summary>
		public static List<ActivityEventOutputDTO> ToOutputDTOList(List<ActivityEvent> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ActivityEventOutputDTO> output = new List<ActivityEventOutputDTO>();

			output.Capacity = data.Count;

			foreach (ActivityEvent activityEvent in data)
			{
				output.Add(activityEvent.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ActivityEvent Object.
		///
		/// </summary>
		public static Database.ActivityEvent FromDTO(ActivityEventDTO dto)
		{
			return new Database.ActivityEvent
			{
				id = dto.id,
				activityEventTypeId = dto.activityEventTypeId,
				title = dto.title,
				description = dto.description,
				relatedEntityType = dto.relatedEntityType,
				relatedEntityId = dto.relatedEntityId,
				eventDate = dto.eventDate,
				isPublic = dto.isPublic,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ActivityEvent Object.
		///
		/// </summary>
		public void ApplyDTO(ActivityEventDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.activityEventTypeId = dto.activityEventTypeId;
			this.title = dto.title;
			this.description = dto.description;
			this.relatedEntityType = dto.relatedEntityType;
			this.relatedEntityId = dto.relatedEntityId;
			this.eventDate = dto.eventDate;
			this.isPublic = dto.isPublic;
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
		/// Creates a deep copy clone of a ActivityEvent Object.
		///
		/// </summary>
		public ActivityEvent Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ActivityEvent{
				id = this.id,
				tenantGuid = this.tenantGuid,
				activityEventTypeId = this.activityEventTypeId,
				title = this.title,
				description = this.description,
				relatedEntityType = this.relatedEntityType,
				relatedEntityId = this.relatedEntityId,
				eventDate = this.eventDate,
				isPublic = this.isPublic,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ActivityEvent Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ActivityEvent Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ActivityEvent Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ActivityEvent Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ActivityEvent activityEvent)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (activityEvent == null)
			{
				return null;
			}

			return new {
				id = activityEvent.id,
				activityEventTypeId = activityEvent.activityEventTypeId,
				title = activityEvent.title,
				description = activityEvent.description,
				relatedEntityType = activityEvent.relatedEntityType,
				relatedEntityId = activityEvent.relatedEntityId,
				eventDate = activityEvent.eventDate,
				isPublic = activityEvent.isPublic,
				objectGuid = activityEvent.objectGuid,
				active = activityEvent.active,
				deleted = activityEvent.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ActivityEvent Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ActivityEvent activityEvent)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (activityEvent == null)
			{
				return null;
			}

			return new {
				id = activityEvent.id,
				activityEventTypeId = activityEvent.activityEventTypeId,
				title = activityEvent.title,
				description = activityEvent.description,
				relatedEntityType = activityEvent.relatedEntityType,
				relatedEntityId = activityEvent.relatedEntityId,
				eventDate = activityEvent.eventDate,
				isPublic = activityEvent.isPublic,
				objectGuid = activityEvent.objectGuid,
				active = activityEvent.active,
				deleted = activityEvent.deleted,
				activityEventType = ActivityEventType.CreateMinimalAnonymous(activityEvent.activityEventType)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ActivityEvent Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ActivityEvent activityEvent)
		{
			//
			// Return a very minimal object.
			//
			if (activityEvent == null)
			{
				return null;
			}

			return new {
				id = activityEvent.id,
				description = activityEvent.description,
				name = activityEvent.title
			 };
		}
	}
}
