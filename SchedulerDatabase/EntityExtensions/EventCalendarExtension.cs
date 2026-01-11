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
	public partial class EventCalendar : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class EventCalendarDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 scheduledEventId { get; set; }
			[Required]
			public Int32 calendarId { get; set; }
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
		public class EventCalendarOutputDTO : EventCalendarDTO
		{
			public Calendar.CalendarDTO calendar { get; set; }
			public ScheduledEvent.ScheduledEventDTO scheduledEvent { get; set; }
		}


		/// <summary>
		///
		/// Converts a EventCalendar to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public EventCalendarDTO ToDTO()
		{
			return new EventCalendarDTO
			{
				id = this.id,
				scheduledEventId = this.scheduledEventId,
				calendarId = this.calendarId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a EventCalendar list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<EventCalendarDTO> ToDTOList(List<EventCalendar> data)
		{
			if (data == null)
			{
				return null;
			}

			List<EventCalendarDTO> output = new List<EventCalendarDTO>();

			output.Capacity = data.Count;

			foreach (EventCalendar eventCalendar in data)
			{
				output.Add(eventCalendar.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a EventCalendar to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the EventCalendarEntity type directly.
		///
		/// </summary>
		public EventCalendarOutputDTO ToOutputDTO()
		{
			return new EventCalendarOutputDTO
			{
				id = this.id,
				scheduledEventId = this.scheduledEventId,
				calendarId = this.calendarId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				calendar = this.calendar?.ToDTO(),
				scheduledEvent = this.scheduledEvent?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a EventCalendar list to list of Output Data Transfer Object intended to be used for serializing a list of EventCalendar objects to avoid using the EventCalendar entity type directly.
		///
		/// </summary>
		public static List<EventCalendarOutputDTO> ToOutputDTOList(List<EventCalendar> data)
		{
			if (data == null)
			{
				return null;
			}

			List<EventCalendarOutputDTO> output = new List<EventCalendarOutputDTO>();

			output.Capacity = data.Count;

			foreach (EventCalendar eventCalendar in data)
			{
				output.Add(eventCalendar.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a EventCalendar Object.
		///
		/// </summary>
		public static Database.EventCalendar FromDTO(EventCalendarDTO dto)
		{
			return new Database.EventCalendar
			{
				id = dto.id,
				scheduledEventId = dto.scheduledEventId,
				calendarId = dto.calendarId,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a EventCalendar Object.
		///
		/// </summary>
		public void ApplyDTO(EventCalendarDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.scheduledEventId = dto.scheduledEventId;
			this.calendarId = dto.calendarId;
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
		/// Creates a deep copy clone of a EventCalendar Object.
		///
		/// </summary>
		public EventCalendar Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new EventCalendar{
				id = this.id,
				tenantGuid = this.tenantGuid,
				scheduledEventId = this.scheduledEventId,
				calendarId = this.calendarId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a EventCalendar Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a EventCalendar Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a EventCalendar Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a EventCalendar Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.EventCalendar eventCalendar)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (eventCalendar == null)
			{
				return null;
			}

			return new {
				id = eventCalendar.id,
				scheduledEventId = eventCalendar.scheduledEventId,
				calendarId = eventCalendar.calendarId,
				objectGuid = eventCalendar.objectGuid,
				active = eventCalendar.active,
				deleted = eventCalendar.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a EventCalendar Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(EventCalendar eventCalendar)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (eventCalendar == null)
			{
				return null;
			}

			return new {
				id = eventCalendar.id,
				scheduledEventId = eventCalendar.scheduledEventId,
				calendarId = eventCalendar.calendarId,
				objectGuid = eventCalendar.objectGuid,
				active = eventCalendar.active,
				deleted = eventCalendar.deleted,
				calendar = Calendar.CreateMinimalAnonymous(eventCalendar.calendar),
				scheduledEvent = ScheduledEvent.CreateMinimalAnonymous(eventCalendar.scheduledEvent)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a EventCalendar Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(EventCalendar eventCalendar)
		{
			//
			// Return a very minimal object.
			//
			if (eventCalendar == null)
			{
				return null;
			}

			return new {
				id = eventCalendar.id,
				name = eventCalendar.id,
				description = eventCalendar.id
			 };
		}
	}
}
