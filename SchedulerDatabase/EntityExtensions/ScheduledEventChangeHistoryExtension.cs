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
	public partial class ScheduledEventChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)scheduledEventId; }
			set { scheduledEventId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ScheduledEventChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 scheduledEventId { get; set; }
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
		public class ScheduledEventChangeHistoryOutputDTO : ScheduledEventChangeHistoryDTO
		{
			public ScheduledEvent.ScheduledEventDTO scheduledEvent { get; set; }
		}


		/// <summary>
		///
		/// Converts a ScheduledEventChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ScheduledEventChangeHistoryDTO ToDTO()
		{
			return new ScheduledEventChangeHistoryDTO
			{
				id = this.id,
				scheduledEventId = this.scheduledEventId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ScheduledEventChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ScheduledEventChangeHistoryDTO> ToDTOList(List<ScheduledEventChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ScheduledEventChangeHistoryDTO> output = new List<ScheduledEventChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ScheduledEventChangeHistory scheduledEventChangeHistory in data)
			{
				output.Add(scheduledEventChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ScheduledEventChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ScheduledEventChangeHistoryEntity type directly.
		///
		/// </summary>
		public ScheduledEventChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ScheduledEventChangeHistoryOutputDTO
			{
				id = this.id,
				scheduledEventId = this.scheduledEventId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				scheduledEvent = this.scheduledEvent?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ScheduledEventChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ScheduledEventChangeHistory objects to avoid using the ScheduledEventChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ScheduledEventChangeHistoryOutputDTO> ToOutputDTOList(List<ScheduledEventChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ScheduledEventChangeHistoryOutputDTO> output = new List<ScheduledEventChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ScheduledEventChangeHistory scheduledEventChangeHistory in data)
			{
				output.Add(scheduledEventChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ScheduledEventChangeHistory Object.
		///
		/// </summary>
		public static Database.ScheduledEventChangeHistory FromDTO(ScheduledEventChangeHistoryDTO dto)
		{
			return new Database.ScheduledEventChangeHistory
			{
				id = dto.id,
				scheduledEventId = dto.scheduledEventId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ScheduledEventChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ScheduledEventChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.scheduledEventId = dto.scheduledEventId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ScheduledEventChangeHistory Object.
		///
		/// </summary>
		public ScheduledEventChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ScheduledEventChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				scheduledEventId = this.scheduledEventId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ScheduledEventChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ScheduledEventChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ScheduledEventChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ScheduledEventChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ScheduledEventChangeHistory scheduledEventChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (scheduledEventChangeHistory == null)
			{
				return null;
			}

			return new {
				id = scheduledEventChangeHistory.id,
				scheduledEventId = scheduledEventChangeHistory.scheduledEventId,
				versionNumber = scheduledEventChangeHistory.versionNumber,
				timeStamp = scheduledEventChangeHistory.timeStamp,
				userId = scheduledEventChangeHistory.userId,
				data = scheduledEventChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ScheduledEventChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ScheduledEventChangeHistory scheduledEventChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (scheduledEventChangeHistory == null)
			{
				return null;
			}

			return new {
				id = scheduledEventChangeHistory.id,
				scheduledEventId = scheduledEventChangeHistory.scheduledEventId,
				versionNumber = scheduledEventChangeHistory.versionNumber,
				timeStamp = scheduledEventChangeHistory.timeStamp,
				userId = scheduledEventChangeHistory.userId,
				data = scheduledEventChangeHistory.data,
				scheduledEvent = ScheduledEvent.CreateMinimalAnonymous(scheduledEventChangeHistory.scheduledEvent),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ScheduledEventChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ScheduledEventChangeHistory scheduledEventChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (scheduledEventChangeHistory == null)
			{
				return null;
			}

			return new {
				id = scheduledEventChangeHistory.id,
				name = scheduledEventChangeHistory.id,
				description = scheduledEventChangeHistory.id
			 };
		}
	}
}
