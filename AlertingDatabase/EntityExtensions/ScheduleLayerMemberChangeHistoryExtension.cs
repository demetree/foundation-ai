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
	public partial class ScheduleLayerMemberChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)scheduleLayerMemberId; }
			set { scheduleLayerMemberId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ScheduleLayerMemberChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 scheduleLayerMemberId { get; set; }
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
		public class ScheduleLayerMemberChangeHistoryOutputDTO : ScheduleLayerMemberChangeHistoryDTO
		{
			public ScheduleLayerMember.ScheduleLayerMemberDTO scheduleLayerMember { get; set; }
		}


		/// <summary>
		///
		/// Converts a ScheduleLayerMemberChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ScheduleLayerMemberChangeHistoryDTO ToDTO()
		{
			return new ScheduleLayerMemberChangeHistoryDTO
			{
				id = this.id,
				scheduleLayerMemberId = this.scheduleLayerMemberId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ScheduleLayerMemberChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ScheduleLayerMemberChangeHistoryDTO> ToDTOList(List<ScheduleLayerMemberChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ScheduleLayerMemberChangeHistoryDTO> output = new List<ScheduleLayerMemberChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ScheduleLayerMemberChangeHistory scheduleLayerMemberChangeHistory in data)
			{
				output.Add(scheduleLayerMemberChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ScheduleLayerMemberChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ScheduleLayerMemberChangeHistoryEntity type directly.
		///
		/// </summary>
		public ScheduleLayerMemberChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ScheduleLayerMemberChangeHistoryOutputDTO
			{
				id = this.id,
				scheduleLayerMemberId = this.scheduleLayerMemberId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				scheduleLayerMember = this.scheduleLayerMember?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ScheduleLayerMemberChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ScheduleLayerMemberChangeHistory objects to avoid using the ScheduleLayerMemberChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ScheduleLayerMemberChangeHistoryOutputDTO> ToOutputDTOList(List<ScheduleLayerMemberChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ScheduleLayerMemberChangeHistoryOutputDTO> output = new List<ScheduleLayerMemberChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ScheduleLayerMemberChangeHistory scheduleLayerMemberChangeHistory in data)
			{
				output.Add(scheduleLayerMemberChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ScheduleLayerMemberChangeHistory Object.
		///
		/// </summary>
		public static Database.ScheduleLayerMemberChangeHistory FromDTO(ScheduleLayerMemberChangeHistoryDTO dto)
		{
			return new Database.ScheduleLayerMemberChangeHistory
			{
				id = dto.id,
				scheduleLayerMemberId = dto.scheduleLayerMemberId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ScheduleLayerMemberChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ScheduleLayerMemberChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.scheduleLayerMemberId = dto.scheduleLayerMemberId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ScheduleLayerMemberChangeHistory Object.
		///
		/// </summary>
		public ScheduleLayerMemberChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ScheduleLayerMemberChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				scheduleLayerMemberId = this.scheduleLayerMemberId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ScheduleLayerMemberChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ScheduleLayerMemberChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ScheduleLayerMemberChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ScheduleLayerMemberChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ScheduleLayerMemberChangeHistory scheduleLayerMemberChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (scheduleLayerMemberChangeHistory == null)
			{
				return null;
			}

			return new {
				id = scheduleLayerMemberChangeHistory.id,
				scheduleLayerMemberId = scheduleLayerMemberChangeHistory.scheduleLayerMemberId,
				versionNumber = scheduleLayerMemberChangeHistory.versionNumber,
				timeStamp = scheduleLayerMemberChangeHistory.timeStamp,
				userId = scheduleLayerMemberChangeHistory.userId,
				data = scheduleLayerMemberChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ScheduleLayerMemberChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ScheduleLayerMemberChangeHistory scheduleLayerMemberChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (scheduleLayerMemberChangeHistory == null)
			{
				return null;
			}

			return new {
				id = scheduleLayerMemberChangeHistory.id,
				scheduleLayerMemberId = scheduleLayerMemberChangeHistory.scheduleLayerMemberId,
				versionNumber = scheduleLayerMemberChangeHistory.versionNumber,
				timeStamp = scheduleLayerMemberChangeHistory.timeStamp,
				userId = scheduleLayerMemberChangeHistory.userId,
				data = scheduleLayerMemberChangeHistory.data,
				scheduleLayerMember = ScheduleLayerMember.CreateMinimalAnonymous(scheduleLayerMemberChangeHistory.scheduleLayerMember)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ScheduleLayerMemberChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ScheduleLayerMemberChangeHistory scheduleLayerMemberChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (scheduleLayerMemberChangeHistory == null)
			{
				return null;
			}

			return new {
				id = scheduleLayerMemberChangeHistory.id,
				name = scheduleLayerMemberChangeHistory.id,
				description = scheduleLayerMemberChangeHistory.id
			 };
		}
	}
}
