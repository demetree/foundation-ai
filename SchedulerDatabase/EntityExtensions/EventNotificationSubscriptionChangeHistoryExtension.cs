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
	public partial class EventNotificationSubscriptionChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)eventNotificationSubscriptionId; }
			set { eventNotificationSubscriptionId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class EventNotificationSubscriptionChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 eventNotificationSubscriptionId { get; set; }
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
		public class EventNotificationSubscriptionChangeHistoryOutputDTO : EventNotificationSubscriptionChangeHistoryDTO
		{
			public EventNotificationSubscription.EventNotificationSubscriptionDTO eventNotificationSubscription { get; set; }
		}


		/// <summary>
		///
		/// Converts a EventNotificationSubscriptionChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public EventNotificationSubscriptionChangeHistoryDTO ToDTO()
		{
			return new EventNotificationSubscriptionChangeHistoryDTO
			{
				id = this.id,
				eventNotificationSubscriptionId = this.eventNotificationSubscriptionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a EventNotificationSubscriptionChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<EventNotificationSubscriptionChangeHistoryDTO> ToDTOList(List<EventNotificationSubscriptionChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<EventNotificationSubscriptionChangeHistoryDTO> output = new List<EventNotificationSubscriptionChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (EventNotificationSubscriptionChangeHistory eventNotificationSubscriptionChangeHistory in data)
			{
				output.Add(eventNotificationSubscriptionChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a EventNotificationSubscriptionChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the EventNotificationSubscriptionChangeHistory Entity type directly.
		///
		/// </summary>
		public EventNotificationSubscriptionChangeHistoryOutputDTO ToOutputDTO()
		{
			return new EventNotificationSubscriptionChangeHistoryOutputDTO
			{
				id = this.id,
				eventNotificationSubscriptionId = this.eventNotificationSubscriptionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				eventNotificationSubscription = this.eventNotificationSubscription?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a EventNotificationSubscriptionChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of EventNotificationSubscriptionChangeHistory objects to avoid using the EventNotificationSubscriptionChangeHistory entity type directly.
		///
		/// </summary>
		public static List<EventNotificationSubscriptionChangeHistoryOutputDTO> ToOutputDTOList(List<EventNotificationSubscriptionChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<EventNotificationSubscriptionChangeHistoryOutputDTO> output = new List<EventNotificationSubscriptionChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (EventNotificationSubscriptionChangeHistory eventNotificationSubscriptionChangeHistory in data)
			{
				output.Add(eventNotificationSubscriptionChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a EventNotificationSubscriptionChangeHistory Object.
		///
		/// </summary>
		public static Database.EventNotificationSubscriptionChangeHistory FromDTO(EventNotificationSubscriptionChangeHistoryDTO dto)
		{
			return new Database.EventNotificationSubscriptionChangeHistory
			{
				id = dto.id,
				eventNotificationSubscriptionId = dto.eventNotificationSubscriptionId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a EventNotificationSubscriptionChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(EventNotificationSubscriptionChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.eventNotificationSubscriptionId = dto.eventNotificationSubscriptionId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a EventNotificationSubscriptionChangeHistory Object.
		///
		/// </summary>
		public EventNotificationSubscriptionChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new EventNotificationSubscriptionChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				eventNotificationSubscriptionId = this.eventNotificationSubscriptionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a EventNotificationSubscriptionChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a EventNotificationSubscriptionChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a EventNotificationSubscriptionChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a EventNotificationSubscriptionChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.EventNotificationSubscriptionChangeHistory eventNotificationSubscriptionChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (eventNotificationSubscriptionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = eventNotificationSubscriptionChangeHistory.id,
				eventNotificationSubscriptionId = eventNotificationSubscriptionChangeHistory.eventNotificationSubscriptionId,
				versionNumber = eventNotificationSubscriptionChangeHistory.versionNumber,
				timeStamp = eventNotificationSubscriptionChangeHistory.timeStamp,
				userId = eventNotificationSubscriptionChangeHistory.userId,
				data = eventNotificationSubscriptionChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a EventNotificationSubscriptionChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(EventNotificationSubscriptionChangeHistory eventNotificationSubscriptionChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (eventNotificationSubscriptionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = eventNotificationSubscriptionChangeHistory.id,
				eventNotificationSubscriptionId = eventNotificationSubscriptionChangeHistory.eventNotificationSubscriptionId,
				versionNumber = eventNotificationSubscriptionChangeHistory.versionNumber,
				timeStamp = eventNotificationSubscriptionChangeHistory.timeStamp,
				userId = eventNotificationSubscriptionChangeHistory.userId,
				data = eventNotificationSubscriptionChangeHistory.data,
				eventNotificationSubscription = EventNotificationSubscription.CreateMinimalAnonymous(eventNotificationSubscriptionChangeHistory.eventNotificationSubscription),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a EventNotificationSubscriptionChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(EventNotificationSubscriptionChangeHistory eventNotificationSubscriptionChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (eventNotificationSubscriptionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = eventNotificationSubscriptionChangeHistory.id,
				name = eventNotificationSubscriptionChangeHistory.id,
				description = eventNotificationSubscriptionChangeHistory.id
			 };
		}
	}
}
