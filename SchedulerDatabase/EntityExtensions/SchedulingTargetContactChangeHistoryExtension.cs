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
	public partial class SchedulingTargetContactChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)schedulingTargetContactId; }
			set { schedulingTargetContactId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SchedulingTargetContactChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 schedulingTargetContactId { get; set; }
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
		public class SchedulingTargetContactChangeHistoryOutputDTO : SchedulingTargetContactChangeHistoryDTO
		{
			public SchedulingTargetContact.SchedulingTargetContactDTO schedulingTargetContact { get; set; }
		}


		/// <summary>
		///
		/// Converts a SchedulingTargetContactChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SchedulingTargetContactChangeHistoryDTO ToDTO()
		{
			return new SchedulingTargetContactChangeHistoryDTO
			{
				id = this.id,
				schedulingTargetContactId = this.schedulingTargetContactId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a SchedulingTargetContactChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SchedulingTargetContactChangeHistoryDTO> ToDTOList(List<SchedulingTargetContactChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SchedulingTargetContactChangeHistoryDTO> output = new List<SchedulingTargetContactChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (SchedulingTargetContactChangeHistory schedulingTargetContactChangeHistory in data)
			{
				output.Add(schedulingTargetContactChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SchedulingTargetContactChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SchedulingTargetContactChangeHistoryEntity type directly.
		///
		/// </summary>
		public SchedulingTargetContactChangeHistoryOutputDTO ToOutputDTO()
		{
			return new SchedulingTargetContactChangeHistoryOutputDTO
			{
				id = this.id,
				schedulingTargetContactId = this.schedulingTargetContactId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				schedulingTargetContact = this.schedulingTargetContact?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a SchedulingTargetContactChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of SchedulingTargetContactChangeHistory objects to avoid using the SchedulingTargetContactChangeHistory entity type directly.
		///
		/// </summary>
		public static List<SchedulingTargetContactChangeHistoryOutputDTO> ToOutputDTOList(List<SchedulingTargetContactChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SchedulingTargetContactChangeHistoryOutputDTO> output = new List<SchedulingTargetContactChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (SchedulingTargetContactChangeHistory schedulingTargetContactChangeHistory in data)
			{
				output.Add(schedulingTargetContactChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SchedulingTargetContactChangeHistory Object.
		///
		/// </summary>
		public static Database.SchedulingTargetContactChangeHistory FromDTO(SchedulingTargetContactChangeHistoryDTO dto)
		{
			return new Database.SchedulingTargetContactChangeHistory
			{
				id = dto.id,
				schedulingTargetContactId = dto.schedulingTargetContactId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SchedulingTargetContactChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(SchedulingTargetContactChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.schedulingTargetContactId = dto.schedulingTargetContactId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a SchedulingTargetContactChangeHistory Object.
		///
		/// </summary>
		public SchedulingTargetContactChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SchedulingTargetContactChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				schedulingTargetContactId = this.schedulingTargetContactId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SchedulingTargetContactChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SchedulingTargetContactChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SchedulingTargetContactChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SchedulingTargetContactChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SchedulingTargetContactChangeHistory schedulingTargetContactChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (schedulingTargetContactChangeHistory == null)
			{
				return null;
			}

			return new {
				id = schedulingTargetContactChangeHistory.id,
				schedulingTargetContactId = schedulingTargetContactChangeHistory.schedulingTargetContactId,
				versionNumber = schedulingTargetContactChangeHistory.versionNumber,
				timeStamp = schedulingTargetContactChangeHistory.timeStamp,
				userId = schedulingTargetContactChangeHistory.userId,
				data = schedulingTargetContactChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SchedulingTargetContactChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SchedulingTargetContactChangeHistory schedulingTargetContactChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (schedulingTargetContactChangeHistory == null)
			{
				return null;
			}

			return new {
				id = schedulingTargetContactChangeHistory.id,
				schedulingTargetContactId = schedulingTargetContactChangeHistory.schedulingTargetContactId,
				versionNumber = schedulingTargetContactChangeHistory.versionNumber,
				timeStamp = schedulingTargetContactChangeHistory.timeStamp,
				userId = schedulingTargetContactChangeHistory.userId,
				data = schedulingTargetContactChangeHistory.data,
				schedulingTargetContact = SchedulingTargetContact.CreateMinimalAnonymous(schedulingTargetContactChangeHistory.schedulingTargetContact),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SchedulingTargetContactChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SchedulingTargetContactChangeHistory schedulingTargetContactChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (schedulingTargetContactChangeHistory == null)
			{
				return null;
			}

			return new {
				id = schedulingTargetContactChangeHistory.id,
				name = schedulingTargetContactChangeHistory.id,
				description = schedulingTargetContactChangeHistory.id
			 };
		}
	}
}
