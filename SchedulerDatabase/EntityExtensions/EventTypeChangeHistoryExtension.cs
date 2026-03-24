using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.Scheduler.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class EventTypeChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)eventTypeId; }
			set { eventTypeId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class EventTypeChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 eventTypeId { get; set; }
			public Int32 versionNumber { get; set; }
			[Required]
			public DateTime timeStamp { get; set; }
			[Required]
			public Int32 userId { get; set; }
			[Required]
			public String data { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class EventTypeChangeHistoryOutputDTO : EventTypeChangeHistoryDTO
		{
			public EventType.EventTypeDTO eventType { get; set; }
		}


		/// <summary>
		///
		/// Converts a EventTypeChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public EventTypeChangeHistoryDTO ToDTO()
		{
			return new EventTypeChangeHistoryDTO
			{
				id = this.id,
				eventTypeId = this.eventTypeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a EventTypeChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<EventTypeChangeHistoryDTO> ToDTOList(List<EventTypeChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<EventTypeChangeHistoryDTO> output = new List<EventTypeChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (EventTypeChangeHistory eventTypeChangeHistory in data)
			{
				output.Add(eventTypeChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a EventTypeChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the EventTypeChangeHistory Entity type directly.
		///
		/// </summary>
		public EventTypeChangeHistoryOutputDTO ToOutputDTO()
		{
			return new EventTypeChangeHistoryOutputDTO
			{
				id = this.id,
				eventTypeId = this.eventTypeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				eventType = this.eventType?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a EventTypeChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of EventTypeChangeHistory objects to avoid using the EventTypeChangeHistory entity type directly.
		///
		/// </summary>
		public static List<EventTypeChangeHistoryOutputDTO> ToOutputDTOList(List<EventTypeChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<EventTypeChangeHistoryOutputDTO> output = new List<EventTypeChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (EventTypeChangeHistory eventTypeChangeHistory in data)
			{
				output.Add(eventTypeChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a EventTypeChangeHistory Object.
		///
		/// </summary>
		public static Database.EventTypeChangeHistory FromDTO(EventTypeChangeHistoryDTO dto)
		{
			return new Database.EventTypeChangeHistory
			{
				id = dto.id,
				eventTypeId = dto.eventTypeId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a EventTypeChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(EventTypeChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.eventTypeId = dto.eventTypeId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a EventTypeChangeHistory Object.
		///
		/// </summary>
		public EventTypeChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new EventTypeChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				eventTypeId = this.eventTypeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a EventTypeChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a EventTypeChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a EventTypeChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a EventTypeChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.EventTypeChangeHistory eventTypeChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (eventTypeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = eventTypeChangeHistory.id,
				eventTypeId = eventTypeChangeHistory.eventTypeId,
				versionNumber = eventTypeChangeHistory.versionNumber,
				timeStamp = eventTypeChangeHistory.timeStamp,
				userId = eventTypeChangeHistory.userId,
				data = eventTypeChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a EventTypeChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(EventTypeChangeHistory eventTypeChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (eventTypeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = eventTypeChangeHistory.id,
				eventTypeId = eventTypeChangeHistory.eventTypeId,
				versionNumber = eventTypeChangeHistory.versionNumber,
				timeStamp = eventTypeChangeHistory.timeStamp,
				userId = eventTypeChangeHistory.userId,
				data = eventTypeChangeHistory.data,
				eventType = EventType.CreateMinimalAnonymous(eventTypeChangeHistory.eventType)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a EventTypeChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(EventTypeChangeHistory eventTypeChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (eventTypeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = eventTypeChangeHistory.id,
				name = eventTypeChangeHistory.id,
				description = eventTypeChangeHistory.id
			 };
		}
	}
}
