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
	public partial class OnCallScheduleChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)onCallScheduleId; }
			set { onCallScheduleId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class OnCallScheduleChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 onCallScheduleId { get; set; }
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
		public class OnCallScheduleChangeHistoryOutputDTO : OnCallScheduleChangeHistoryDTO
		{
			public OnCallSchedule.OnCallScheduleDTO onCallSchedule { get; set; }
		}


		/// <summary>
		///
		/// Converts a OnCallScheduleChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public OnCallScheduleChangeHistoryDTO ToDTO()
		{
			return new OnCallScheduleChangeHistoryDTO
			{
				id = this.id,
				onCallScheduleId = this.onCallScheduleId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a OnCallScheduleChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<OnCallScheduleChangeHistoryDTO> ToDTOList(List<OnCallScheduleChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<OnCallScheduleChangeHistoryDTO> output = new List<OnCallScheduleChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (OnCallScheduleChangeHistory onCallScheduleChangeHistory in data)
			{
				output.Add(onCallScheduleChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a OnCallScheduleChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the OnCallScheduleChangeHistoryEntity type directly.
		///
		/// </summary>
		public OnCallScheduleChangeHistoryOutputDTO ToOutputDTO()
		{
			return new OnCallScheduleChangeHistoryOutputDTO
			{
				id = this.id,
				onCallScheduleId = this.onCallScheduleId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				onCallSchedule = this.onCallSchedule?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a OnCallScheduleChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of OnCallScheduleChangeHistory objects to avoid using the OnCallScheduleChangeHistory entity type directly.
		///
		/// </summary>
		public static List<OnCallScheduleChangeHistoryOutputDTO> ToOutputDTOList(List<OnCallScheduleChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<OnCallScheduleChangeHistoryOutputDTO> output = new List<OnCallScheduleChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (OnCallScheduleChangeHistory onCallScheduleChangeHistory in data)
			{
				output.Add(onCallScheduleChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a OnCallScheduleChangeHistory Object.
		///
		/// </summary>
		public static Database.OnCallScheduleChangeHistory FromDTO(OnCallScheduleChangeHistoryDTO dto)
		{
			return new Database.OnCallScheduleChangeHistory
			{
				id = dto.id,
				onCallScheduleId = dto.onCallScheduleId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a OnCallScheduleChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(OnCallScheduleChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.onCallScheduleId = dto.onCallScheduleId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a OnCallScheduleChangeHistory Object.
		///
		/// </summary>
		public OnCallScheduleChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new OnCallScheduleChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				onCallScheduleId = this.onCallScheduleId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a OnCallScheduleChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a OnCallScheduleChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a OnCallScheduleChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a OnCallScheduleChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.OnCallScheduleChangeHistory onCallScheduleChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (onCallScheduleChangeHistory == null)
			{
				return null;
			}

			return new {
				id = onCallScheduleChangeHistory.id,
				onCallScheduleId = onCallScheduleChangeHistory.onCallScheduleId,
				versionNumber = onCallScheduleChangeHistory.versionNumber,
				timeStamp = onCallScheduleChangeHistory.timeStamp,
				userId = onCallScheduleChangeHistory.userId,
				data = onCallScheduleChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a OnCallScheduleChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(OnCallScheduleChangeHistory onCallScheduleChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (onCallScheduleChangeHistory == null)
			{
				return null;
			}

			return new {
				id = onCallScheduleChangeHistory.id,
				onCallScheduleId = onCallScheduleChangeHistory.onCallScheduleId,
				versionNumber = onCallScheduleChangeHistory.versionNumber,
				timeStamp = onCallScheduleChangeHistory.timeStamp,
				userId = onCallScheduleChangeHistory.userId,
				data = onCallScheduleChangeHistory.data,
				onCallSchedule = OnCallSchedule.CreateMinimalAnonymous(onCallScheduleChangeHistory.onCallSchedule),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a OnCallScheduleChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(OnCallScheduleChangeHistory onCallScheduleChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (onCallScheduleChangeHistory == null)
			{
				return null;
			}

			return new {
				id = onCallScheduleChangeHistory.id,
				name = onCallScheduleChangeHistory.id,
				description = onCallScheduleChangeHistory.id
			 };
		}
	}
}
