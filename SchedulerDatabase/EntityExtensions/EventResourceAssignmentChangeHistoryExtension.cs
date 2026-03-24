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
	public partial class EventResourceAssignmentChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)eventResourceAssignmentId; }
			set { eventResourceAssignmentId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class EventResourceAssignmentChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 eventResourceAssignmentId { get; set; }
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
		public class EventResourceAssignmentChangeHistoryOutputDTO : EventResourceAssignmentChangeHistoryDTO
		{
			public EventResourceAssignment.EventResourceAssignmentDTO eventResourceAssignment { get; set; }
		}


		/// <summary>
		///
		/// Converts a EventResourceAssignmentChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public EventResourceAssignmentChangeHistoryDTO ToDTO()
		{
			return new EventResourceAssignmentChangeHistoryDTO
			{
				id = this.id,
				eventResourceAssignmentId = this.eventResourceAssignmentId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a EventResourceAssignmentChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<EventResourceAssignmentChangeHistoryDTO> ToDTOList(List<EventResourceAssignmentChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<EventResourceAssignmentChangeHistoryDTO> output = new List<EventResourceAssignmentChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (EventResourceAssignmentChangeHistory eventResourceAssignmentChangeHistory in data)
			{
				output.Add(eventResourceAssignmentChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a EventResourceAssignmentChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the EventResourceAssignmentChangeHistory Entity type directly.
		///
		/// </summary>
		public EventResourceAssignmentChangeHistoryOutputDTO ToOutputDTO()
		{
			return new EventResourceAssignmentChangeHistoryOutputDTO
			{
				id = this.id,
				eventResourceAssignmentId = this.eventResourceAssignmentId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				eventResourceAssignment = this.eventResourceAssignment?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a EventResourceAssignmentChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of EventResourceAssignmentChangeHistory objects to avoid using the EventResourceAssignmentChangeHistory entity type directly.
		///
		/// </summary>
		public static List<EventResourceAssignmentChangeHistoryOutputDTO> ToOutputDTOList(List<EventResourceAssignmentChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<EventResourceAssignmentChangeHistoryOutputDTO> output = new List<EventResourceAssignmentChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (EventResourceAssignmentChangeHistory eventResourceAssignmentChangeHistory in data)
			{
				output.Add(eventResourceAssignmentChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a EventResourceAssignmentChangeHistory Object.
		///
		/// </summary>
		public static Database.EventResourceAssignmentChangeHistory FromDTO(EventResourceAssignmentChangeHistoryDTO dto)
		{
			return new Database.EventResourceAssignmentChangeHistory
			{
				id = dto.id,
				eventResourceAssignmentId = dto.eventResourceAssignmentId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a EventResourceAssignmentChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(EventResourceAssignmentChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.eventResourceAssignmentId = dto.eventResourceAssignmentId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a EventResourceAssignmentChangeHistory Object.
		///
		/// </summary>
		public EventResourceAssignmentChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new EventResourceAssignmentChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				eventResourceAssignmentId = this.eventResourceAssignmentId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a EventResourceAssignmentChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a EventResourceAssignmentChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a EventResourceAssignmentChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a EventResourceAssignmentChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.EventResourceAssignmentChangeHistory eventResourceAssignmentChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (eventResourceAssignmentChangeHistory == null)
			{
				return null;
			}

			return new {
				id = eventResourceAssignmentChangeHistory.id,
				eventResourceAssignmentId = eventResourceAssignmentChangeHistory.eventResourceAssignmentId,
				versionNumber = eventResourceAssignmentChangeHistory.versionNumber,
				timeStamp = eventResourceAssignmentChangeHistory.timeStamp,
				userId = eventResourceAssignmentChangeHistory.userId,
				data = eventResourceAssignmentChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a EventResourceAssignmentChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(EventResourceAssignmentChangeHistory eventResourceAssignmentChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (eventResourceAssignmentChangeHistory == null)
			{
				return null;
			}

			return new {
				id = eventResourceAssignmentChangeHistory.id,
				eventResourceAssignmentId = eventResourceAssignmentChangeHistory.eventResourceAssignmentId,
				versionNumber = eventResourceAssignmentChangeHistory.versionNumber,
				timeStamp = eventResourceAssignmentChangeHistory.timeStamp,
				userId = eventResourceAssignmentChangeHistory.userId,
				data = eventResourceAssignmentChangeHistory.data,
				eventResourceAssignment = EventResourceAssignment.CreateMinimalAnonymous(eventResourceAssignmentChangeHistory.eventResourceAssignment)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a EventResourceAssignmentChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(EventResourceAssignmentChangeHistory eventResourceAssignmentChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (eventResourceAssignmentChangeHistory == null)
			{
				return null;
			}

			return new {
				id = eventResourceAssignmentChangeHistory.id,
				name = eventResourceAssignmentChangeHistory.id,
				description = eventResourceAssignmentChangeHistory.id
			 };
		}
	}
}
