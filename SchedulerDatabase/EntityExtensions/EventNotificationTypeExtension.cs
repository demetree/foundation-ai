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
	public partial class EventNotificationType : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class EventNotificationTypeDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
			public Int32? sequence { get; set; }
			public String color { get; set; }
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
		public class EventNotificationTypeOutputDTO : EventNotificationTypeDTO
		{
		}


		/// <summary>
		///
		/// Converts a EventNotificationType to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public EventNotificationTypeDTO ToDTO()
		{
			return new EventNotificationTypeDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				sequence = this.sequence,
				color = this.color,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a EventNotificationType list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<EventNotificationTypeDTO> ToDTOList(List<EventNotificationType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<EventNotificationTypeDTO> output = new List<EventNotificationTypeDTO>();

			output.Capacity = data.Count;

			foreach (EventNotificationType eventNotificationType in data)
			{
				output.Add(eventNotificationType.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a EventNotificationType to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the EventNotificationType Entity type directly.
		///
		/// </summary>
		public EventNotificationTypeOutputDTO ToOutputDTO()
		{
			return new EventNotificationTypeOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				sequence = this.sequence,
				color = this.color,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a EventNotificationType list to list of Output Data Transfer Object intended to be used for serializing a list of EventNotificationType objects to avoid using the EventNotificationType entity type directly.
		///
		/// </summary>
		public static List<EventNotificationTypeOutputDTO> ToOutputDTOList(List<EventNotificationType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<EventNotificationTypeOutputDTO> output = new List<EventNotificationTypeOutputDTO>();

			output.Capacity = data.Count;

			foreach (EventNotificationType eventNotificationType in data)
			{
				output.Add(eventNotificationType.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a EventNotificationType Object.
		///
		/// </summary>
		public static Database.EventNotificationType FromDTO(EventNotificationTypeDTO dto)
		{
			return new Database.EventNotificationType
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				sequence = dto.sequence,
				color = dto.color,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a EventNotificationType Object.
		///
		/// </summary>
		public void ApplyDTO(EventNotificationTypeDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.sequence = dto.sequence;
			this.color = dto.color;
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
		/// Creates a deep copy clone of a EventNotificationType Object.
		///
		/// </summary>
		public EventNotificationType Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new EventNotificationType{
				id = this.id,
				name = this.name,
				description = this.description,
				sequence = this.sequence,
				color = this.color,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a EventNotificationType Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a EventNotificationType Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a EventNotificationType Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a EventNotificationType Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.EventNotificationType eventNotificationType)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (eventNotificationType == null)
			{
				return null;
			}

			return new {
				id = eventNotificationType.id,
				name = eventNotificationType.name,
				description = eventNotificationType.description,
				sequence = eventNotificationType.sequence,
				color = eventNotificationType.color,
				objectGuid = eventNotificationType.objectGuid,
				active = eventNotificationType.active,
				deleted = eventNotificationType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a EventNotificationType Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(EventNotificationType eventNotificationType)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (eventNotificationType == null)
			{
				return null;
			}

			return new {
				id = eventNotificationType.id,
				name = eventNotificationType.name,
				description = eventNotificationType.description,
				sequence = eventNotificationType.sequence,
				color = eventNotificationType.color,
				objectGuid = eventNotificationType.objectGuid,
				active = eventNotificationType.active,
				deleted = eventNotificationType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a EventNotificationType Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(EventNotificationType eventNotificationType)
		{
			//
			// Return a very minimal object.
			//
			if (eventNotificationType == null)
			{
				return null;
			}

			return new {
				id = eventNotificationType.id,
				name = eventNotificationType.name,
				description = eventNotificationType.description,
			 };
		}
	}
}
