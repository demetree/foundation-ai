using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.Alerting.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class ScheduleOverrideChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)scheduleOverrideId; }
			set { scheduleOverrideId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ScheduleOverrideChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 scheduleOverrideId { get; set; }
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
		public class ScheduleOverrideChangeHistoryOutputDTO : ScheduleOverrideChangeHistoryDTO
		{
			public ScheduleOverride.ScheduleOverrideDTO scheduleOverride { get; set; }
		}


		/// <summary>
		///
		/// Converts a ScheduleOverrideChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ScheduleOverrideChangeHistoryDTO ToDTO()
		{
			return new ScheduleOverrideChangeHistoryDTO
			{
				id = this.id,
				scheduleOverrideId = this.scheduleOverrideId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ScheduleOverrideChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ScheduleOverrideChangeHistoryDTO> ToDTOList(List<ScheduleOverrideChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ScheduleOverrideChangeHistoryDTO> output = new List<ScheduleOverrideChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ScheduleOverrideChangeHistory scheduleOverrideChangeHistory in data)
			{
				output.Add(scheduleOverrideChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ScheduleOverrideChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ScheduleOverrideChangeHistoryEntity type directly.
		///
		/// </summary>
		public ScheduleOverrideChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ScheduleOverrideChangeHistoryOutputDTO
			{
				id = this.id,
				scheduleOverrideId = this.scheduleOverrideId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				scheduleOverride = this.scheduleOverride?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ScheduleOverrideChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ScheduleOverrideChangeHistory objects to avoid using the ScheduleOverrideChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ScheduleOverrideChangeHistoryOutputDTO> ToOutputDTOList(List<ScheduleOverrideChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ScheduleOverrideChangeHistoryOutputDTO> output = new List<ScheduleOverrideChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ScheduleOverrideChangeHistory scheduleOverrideChangeHistory in data)
			{
				output.Add(scheduleOverrideChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ScheduleOverrideChangeHistory Object.
		///
		/// </summary>
		public static Database.ScheduleOverrideChangeHistory FromDTO(ScheduleOverrideChangeHistoryDTO dto)
		{
			return new Database.ScheduleOverrideChangeHistory
			{
				id = dto.id,
				scheduleOverrideId = dto.scheduleOverrideId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ScheduleOverrideChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ScheduleOverrideChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.scheduleOverrideId = dto.scheduleOverrideId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ScheduleOverrideChangeHistory Object.
		///
		/// </summary>
		public ScheduleOverrideChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ScheduleOverrideChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				scheduleOverrideId = this.scheduleOverrideId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ScheduleOverrideChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ScheduleOverrideChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ScheduleOverrideChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ScheduleOverrideChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ScheduleOverrideChangeHistory scheduleOverrideChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (scheduleOverrideChangeHistory == null)
			{
				return null;
			}

			return new {
				id = scheduleOverrideChangeHistory.id,
				scheduleOverrideId = scheduleOverrideChangeHistory.scheduleOverrideId,
				versionNumber = scheduleOverrideChangeHistory.versionNumber,
				timeStamp = scheduleOverrideChangeHistory.timeStamp,
				userId = scheduleOverrideChangeHistory.userId,
				data = scheduleOverrideChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ScheduleOverrideChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ScheduleOverrideChangeHistory scheduleOverrideChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (scheduleOverrideChangeHistory == null)
			{
				return null;
			}

			return new {
				id = scheduleOverrideChangeHistory.id,
				scheduleOverrideId = scheduleOverrideChangeHistory.scheduleOverrideId,
				versionNumber = scheduleOverrideChangeHistory.versionNumber,
				timeStamp = scheduleOverrideChangeHistory.timeStamp,
				userId = scheduleOverrideChangeHistory.userId,
				data = scheduleOverrideChangeHistory.data,
				scheduleOverride = ScheduleOverride.CreateMinimalAnonymous(scheduleOverrideChangeHistory.scheduleOverride),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ScheduleOverrideChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ScheduleOverrideChangeHistory scheduleOverrideChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (scheduleOverrideChangeHistory == null)
			{
				return null;
			}

			return new {
				id = scheduleOverrideChangeHistory.id,
				name = scheduleOverrideChangeHistory.id,
				description = scheduleOverrideChangeHistory.id
			 };
		}
	}
}
