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
	public partial class EventStatus : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class EventStatusDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
			public String color { get; set; }
			public Int32? sequence { get; set; }
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
		public class EventStatusOutputDTO : EventStatusDTO
		{
		}


		/// <summary>
		///
		/// Converts a EventStatus to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public EventStatusDTO ToDTO()
		{
			return new EventStatusDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				color = this.color,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a EventStatus list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<EventStatusDTO> ToDTOList(List<EventStatus> data)
		{
			if (data == null)
			{
				return null;
			}

			List<EventStatusDTO> output = new List<EventStatusDTO>();

			output.Capacity = data.Count;

			foreach (EventStatus eventStatus in data)
			{
				output.Add(eventStatus.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a EventStatus to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the EventStatus Entity type directly.
		///
		/// </summary>
		public EventStatusOutputDTO ToOutputDTO()
		{
			return new EventStatusOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				color = this.color,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a EventStatus list to list of Output Data Transfer Object intended to be used for serializing a list of EventStatus objects to avoid using the EventStatus entity type directly.
		///
		/// </summary>
		public static List<EventStatusOutputDTO> ToOutputDTOList(List<EventStatus> data)
		{
			if (data == null)
			{
				return null;
			}

			List<EventStatusOutputDTO> output = new List<EventStatusOutputDTO>();

			output.Capacity = data.Count;

			foreach (EventStatus eventStatus in data)
			{
				output.Add(eventStatus.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a EventStatus Object.
		///
		/// </summary>
		public static Database.EventStatus FromDTO(EventStatusDTO dto)
		{
			return new Database.EventStatus
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				color = dto.color,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a EventStatus Object.
		///
		/// </summary>
		public void ApplyDTO(EventStatusDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.color = dto.color;
			this.sequence = dto.sequence;
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
		/// Creates a deep copy clone of a EventStatus Object.
		///
		/// </summary>
		public EventStatus Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new EventStatus{
				id = this.id,
				name = this.name,
				description = this.description,
				color = this.color,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a EventStatus Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a EventStatus Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a EventStatus Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a EventStatus Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.EventStatus eventStatus)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (eventStatus == null)
			{
				return null;
			}

			return new {
				id = eventStatus.id,
				name = eventStatus.name,
				description = eventStatus.description,
				color = eventStatus.color,
				sequence = eventStatus.sequence,
				objectGuid = eventStatus.objectGuid,
				active = eventStatus.active,
				deleted = eventStatus.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a EventStatus Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(EventStatus eventStatus)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (eventStatus == null)
			{
				return null;
			}

			return new {
				id = eventStatus.id,
				name = eventStatus.name,
				description = eventStatus.description,
				color = eventStatus.color,
				sequence = eventStatus.sequence,
				objectGuid = eventStatus.objectGuid,
				active = eventStatus.active,
				deleted = eventStatus.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a EventStatus Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(EventStatus eventStatus)
		{
			//
			// Return a very minimal object.
			//
			if (eventStatus == null)
			{
				return null;
			}

			return new {
				id = eventStatus.id,
				name = eventStatus.name,
				description = eventStatus.description,
			 };
		}
	}
}
