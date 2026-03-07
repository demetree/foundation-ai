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
	public partial class SchedulingTargetChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)schedulingTargetId; }
			set { schedulingTargetId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SchedulingTargetChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 schedulingTargetId { get; set; }
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
		public class SchedulingTargetChangeHistoryOutputDTO : SchedulingTargetChangeHistoryDTO
		{
			public SchedulingTarget.SchedulingTargetDTO schedulingTarget { get; set; }
		}


		/// <summary>
		///
		/// Converts a SchedulingTargetChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SchedulingTargetChangeHistoryDTO ToDTO()
		{
			return new SchedulingTargetChangeHistoryDTO
			{
				id = this.id,
				schedulingTargetId = this.schedulingTargetId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a SchedulingTargetChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SchedulingTargetChangeHistoryDTO> ToDTOList(List<SchedulingTargetChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SchedulingTargetChangeHistoryDTO> output = new List<SchedulingTargetChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (SchedulingTargetChangeHistory schedulingTargetChangeHistory in data)
			{
				output.Add(schedulingTargetChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SchedulingTargetChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SchedulingTargetChangeHistoryEntity type directly.
		///
		/// </summary>
		public SchedulingTargetChangeHistoryOutputDTO ToOutputDTO()
		{
			return new SchedulingTargetChangeHistoryOutputDTO
			{
				id = this.id,
				schedulingTargetId = this.schedulingTargetId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				schedulingTarget = this.schedulingTarget?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a SchedulingTargetChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of SchedulingTargetChangeHistory objects to avoid using the SchedulingTargetChangeHistory entity type directly.
		///
		/// </summary>
		public static List<SchedulingTargetChangeHistoryOutputDTO> ToOutputDTOList(List<SchedulingTargetChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SchedulingTargetChangeHistoryOutputDTO> output = new List<SchedulingTargetChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (SchedulingTargetChangeHistory schedulingTargetChangeHistory in data)
			{
				output.Add(schedulingTargetChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SchedulingTargetChangeHistory Object.
		///
		/// </summary>
		public static Database.SchedulingTargetChangeHistory FromDTO(SchedulingTargetChangeHistoryDTO dto)
		{
			return new Database.SchedulingTargetChangeHistory
			{
				id = dto.id,
				schedulingTargetId = dto.schedulingTargetId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SchedulingTargetChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(SchedulingTargetChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.schedulingTargetId = dto.schedulingTargetId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a SchedulingTargetChangeHistory Object.
		///
		/// </summary>
		public SchedulingTargetChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SchedulingTargetChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				schedulingTargetId = this.schedulingTargetId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SchedulingTargetChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SchedulingTargetChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SchedulingTargetChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SchedulingTargetChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SchedulingTargetChangeHistory schedulingTargetChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (schedulingTargetChangeHistory == null)
			{
				return null;
			}

			return new {
				id = schedulingTargetChangeHistory.id,
				schedulingTargetId = schedulingTargetChangeHistory.schedulingTargetId,
				versionNumber = schedulingTargetChangeHistory.versionNumber,
				timeStamp = schedulingTargetChangeHistory.timeStamp,
				userId = schedulingTargetChangeHistory.userId,
				data = schedulingTargetChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SchedulingTargetChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SchedulingTargetChangeHistory schedulingTargetChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (schedulingTargetChangeHistory == null)
			{
				return null;
			}

			return new {
				id = schedulingTargetChangeHistory.id,
				schedulingTargetId = schedulingTargetChangeHistory.schedulingTargetId,
				versionNumber = schedulingTargetChangeHistory.versionNumber,
				timeStamp = schedulingTargetChangeHistory.timeStamp,
				userId = schedulingTargetChangeHistory.userId,
				data = schedulingTargetChangeHistory.data,
				schedulingTarget = SchedulingTarget.CreateMinimalAnonymous(schedulingTargetChangeHistory.schedulingTarget),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SchedulingTargetChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SchedulingTargetChangeHistory schedulingTargetChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (schedulingTargetChangeHistory == null)
			{
				return null;
			}

			return new {
				id = schedulingTargetChangeHistory.id,
				name = schedulingTargetChangeHistory.id,
				description = schedulingTargetChangeHistory.id
			 };
		}
	}
}
