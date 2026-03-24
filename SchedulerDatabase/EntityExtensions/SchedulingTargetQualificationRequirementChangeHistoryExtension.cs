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
	public partial class SchedulingTargetQualificationRequirementChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)schedulingTargetQualificationRequirementId; }
			set { schedulingTargetQualificationRequirementId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SchedulingTargetQualificationRequirementChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 schedulingTargetQualificationRequirementId { get; set; }
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
		public class SchedulingTargetQualificationRequirementChangeHistoryOutputDTO : SchedulingTargetQualificationRequirementChangeHistoryDTO
		{
			public SchedulingTargetQualificationRequirement.SchedulingTargetQualificationRequirementDTO schedulingTargetQualificationRequirement { get; set; }
		}


		/// <summary>
		///
		/// Converts a SchedulingTargetQualificationRequirementChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SchedulingTargetQualificationRequirementChangeHistoryDTO ToDTO()
		{
			return new SchedulingTargetQualificationRequirementChangeHistoryDTO
			{
				id = this.id,
				schedulingTargetQualificationRequirementId = this.schedulingTargetQualificationRequirementId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a SchedulingTargetQualificationRequirementChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SchedulingTargetQualificationRequirementChangeHistoryDTO> ToDTOList(List<SchedulingTargetQualificationRequirementChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SchedulingTargetQualificationRequirementChangeHistoryDTO> output = new List<SchedulingTargetQualificationRequirementChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (SchedulingTargetQualificationRequirementChangeHistory schedulingTargetQualificationRequirementChangeHistory in data)
			{
				output.Add(schedulingTargetQualificationRequirementChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SchedulingTargetQualificationRequirementChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SchedulingTargetQualificationRequirementChangeHistory Entity type directly.
		///
		/// </summary>
		public SchedulingTargetQualificationRequirementChangeHistoryOutputDTO ToOutputDTO()
		{
			return new SchedulingTargetQualificationRequirementChangeHistoryOutputDTO
			{
				id = this.id,
				schedulingTargetQualificationRequirementId = this.schedulingTargetQualificationRequirementId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				schedulingTargetQualificationRequirement = this.schedulingTargetQualificationRequirement?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a SchedulingTargetQualificationRequirementChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of SchedulingTargetQualificationRequirementChangeHistory objects to avoid using the SchedulingTargetQualificationRequirementChangeHistory entity type directly.
		///
		/// </summary>
		public static List<SchedulingTargetQualificationRequirementChangeHistoryOutputDTO> ToOutputDTOList(List<SchedulingTargetQualificationRequirementChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SchedulingTargetQualificationRequirementChangeHistoryOutputDTO> output = new List<SchedulingTargetQualificationRequirementChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (SchedulingTargetQualificationRequirementChangeHistory schedulingTargetQualificationRequirementChangeHistory in data)
			{
				output.Add(schedulingTargetQualificationRequirementChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SchedulingTargetQualificationRequirementChangeHistory Object.
		///
		/// </summary>
		public static Database.SchedulingTargetQualificationRequirementChangeHistory FromDTO(SchedulingTargetQualificationRequirementChangeHistoryDTO dto)
		{
			return new Database.SchedulingTargetQualificationRequirementChangeHistory
			{
				id = dto.id,
				schedulingTargetQualificationRequirementId = dto.schedulingTargetQualificationRequirementId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SchedulingTargetQualificationRequirementChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(SchedulingTargetQualificationRequirementChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.schedulingTargetQualificationRequirementId = dto.schedulingTargetQualificationRequirementId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a SchedulingTargetQualificationRequirementChangeHistory Object.
		///
		/// </summary>
		public SchedulingTargetQualificationRequirementChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SchedulingTargetQualificationRequirementChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				schedulingTargetQualificationRequirementId = this.schedulingTargetQualificationRequirementId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SchedulingTargetQualificationRequirementChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SchedulingTargetQualificationRequirementChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SchedulingTargetQualificationRequirementChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SchedulingTargetQualificationRequirementChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SchedulingTargetQualificationRequirementChangeHistory schedulingTargetQualificationRequirementChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (schedulingTargetQualificationRequirementChangeHistory == null)
			{
				return null;
			}

			return new {
				id = schedulingTargetQualificationRequirementChangeHistory.id,
				schedulingTargetQualificationRequirementId = schedulingTargetQualificationRequirementChangeHistory.schedulingTargetQualificationRequirementId,
				versionNumber = schedulingTargetQualificationRequirementChangeHistory.versionNumber,
				timeStamp = schedulingTargetQualificationRequirementChangeHistory.timeStamp,
				userId = schedulingTargetQualificationRequirementChangeHistory.userId,
				data = schedulingTargetQualificationRequirementChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SchedulingTargetQualificationRequirementChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SchedulingTargetQualificationRequirementChangeHistory schedulingTargetQualificationRequirementChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (schedulingTargetQualificationRequirementChangeHistory == null)
			{
				return null;
			}

			return new {
				id = schedulingTargetQualificationRequirementChangeHistory.id,
				schedulingTargetQualificationRequirementId = schedulingTargetQualificationRequirementChangeHistory.schedulingTargetQualificationRequirementId,
				versionNumber = schedulingTargetQualificationRequirementChangeHistory.versionNumber,
				timeStamp = schedulingTargetQualificationRequirementChangeHistory.timeStamp,
				userId = schedulingTargetQualificationRequirementChangeHistory.userId,
				data = schedulingTargetQualificationRequirementChangeHistory.data,
				schedulingTargetQualificationRequirement = SchedulingTargetQualificationRequirement.CreateMinimalAnonymous(schedulingTargetQualificationRequirementChangeHistory.schedulingTargetQualificationRequirement)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SchedulingTargetQualificationRequirementChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SchedulingTargetQualificationRequirementChangeHistory schedulingTargetQualificationRequirementChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (schedulingTargetQualificationRequirementChangeHistory == null)
			{
				return null;
			}

			return new {
				id = schedulingTargetQualificationRequirementChangeHistory.id,
				name = schedulingTargetQualificationRequirementChangeHistory.id,
				description = schedulingTargetQualificationRequirementChangeHistory.id
			 };
		}
	}
}
