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
	public partial class ScheduleLayerChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)scheduleLayerId; }
			set { scheduleLayerId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ScheduleLayerChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 scheduleLayerId { get; set; }
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
		public class ScheduleLayerChangeHistoryOutputDTO : ScheduleLayerChangeHistoryDTO
		{
			public ScheduleLayer.ScheduleLayerDTO scheduleLayer { get; set; }
		}


		/// <summary>
		///
		/// Converts a ScheduleLayerChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ScheduleLayerChangeHistoryDTO ToDTO()
		{
			return new ScheduleLayerChangeHistoryDTO
			{
				id = this.id,
				scheduleLayerId = this.scheduleLayerId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ScheduleLayerChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ScheduleLayerChangeHistoryDTO> ToDTOList(List<ScheduleLayerChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ScheduleLayerChangeHistoryDTO> output = new List<ScheduleLayerChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ScheduleLayerChangeHistory scheduleLayerChangeHistory in data)
			{
				output.Add(scheduleLayerChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ScheduleLayerChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ScheduleLayerChangeHistoryEntity type directly.
		///
		/// </summary>
		public ScheduleLayerChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ScheduleLayerChangeHistoryOutputDTO
			{
				id = this.id,
				scheduleLayerId = this.scheduleLayerId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				scheduleLayer = this.scheduleLayer?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ScheduleLayerChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ScheduleLayerChangeHistory objects to avoid using the ScheduleLayerChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ScheduleLayerChangeHistoryOutputDTO> ToOutputDTOList(List<ScheduleLayerChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ScheduleLayerChangeHistoryOutputDTO> output = new List<ScheduleLayerChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ScheduleLayerChangeHistory scheduleLayerChangeHistory in data)
			{
				output.Add(scheduleLayerChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ScheduleLayerChangeHistory Object.
		///
		/// </summary>
		public static Database.ScheduleLayerChangeHistory FromDTO(ScheduleLayerChangeHistoryDTO dto)
		{
			return new Database.ScheduleLayerChangeHistory
			{
				id = dto.id,
				scheduleLayerId = dto.scheduleLayerId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ScheduleLayerChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ScheduleLayerChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.scheduleLayerId = dto.scheduleLayerId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ScheduleLayerChangeHistory Object.
		///
		/// </summary>
		public ScheduleLayerChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ScheduleLayerChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				scheduleLayerId = this.scheduleLayerId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ScheduleLayerChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ScheduleLayerChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ScheduleLayerChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ScheduleLayerChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ScheduleLayerChangeHistory scheduleLayerChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (scheduleLayerChangeHistory == null)
			{
				return null;
			}

			return new {
				id = scheduleLayerChangeHistory.id,
				scheduleLayerId = scheduleLayerChangeHistory.scheduleLayerId,
				versionNumber = scheduleLayerChangeHistory.versionNumber,
				timeStamp = scheduleLayerChangeHistory.timeStamp,
				userId = scheduleLayerChangeHistory.userId,
				data = scheduleLayerChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ScheduleLayerChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ScheduleLayerChangeHistory scheduleLayerChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (scheduleLayerChangeHistory == null)
			{
				return null;
			}

			return new {
				id = scheduleLayerChangeHistory.id,
				scheduleLayerId = scheduleLayerChangeHistory.scheduleLayerId,
				versionNumber = scheduleLayerChangeHistory.versionNumber,
				timeStamp = scheduleLayerChangeHistory.timeStamp,
				userId = scheduleLayerChangeHistory.userId,
				data = scheduleLayerChangeHistory.data,
				scheduleLayer = ScheduleLayer.CreateMinimalAnonymous(scheduleLayerChangeHistory.scheduleLayer)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ScheduleLayerChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ScheduleLayerChangeHistory scheduleLayerChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (scheduleLayerChangeHistory == null)
			{
				return null;
			}

			return new {
				id = scheduleLayerChangeHistory.id,
				name = scheduleLayerChangeHistory.id,
				description = scheduleLayerChangeHistory.id
			 };
		}
	}
}
