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
	public partial class ScheduledEventQualificationRequirementChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)scheduledEventQualificationRequirementId; }
			set { scheduledEventQualificationRequirementId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ScheduledEventQualificationRequirementChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 scheduledEventQualificationRequirementId { get; set; }
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
		public class ScheduledEventQualificationRequirementChangeHistoryOutputDTO : ScheduledEventQualificationRequirementChangeHistoryDTO
		{
			public ScheduledEventQualificationRequirement.ScheduledEventQualificationRequirementDTO scheduledEventQualificationRequirement { get; set; }
		}


		/// <summary>
		///
		/// Converts a ScheduledEventQualificationRequirementChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ScheduledEventQualificationRequirementChangeHistoryDTO ToDTO()
		{
			return new ScheduledEventQualificationRequirementChangeHistoryDTO
			{
				id = this.id,
				scheduledEventQualificationRequirementId = this.scheduledEventQualificationRequirementId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ScheduledEventQualificationRequirementChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ScheduledEventQualificationRequirementChangeHistoryDTO> ToDTOList(List<ScheduledEventQualificationRequirementChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ScheduledEventQualificationRequirementChangeHistoryDTO> output = new List<ScheduledEventQualificationRequirementChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ScheduledEventQualificationRequirementChangeHistory scheduledEventQualificationRequirementChangeHistory in data)
			{
				output.Add(scheduledEventQualificationRequirementChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ScheduledEventQualificationRequirementChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ScheduledEventQualificationRequirementChangeHistoryEntity type directly.
		///
		/// </summary>
		public ScheduledEventQualificationRequirementChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ScheduledEventQualificationRequirementChangeHistoryOutputDTO
			{
				id = this.id,
				scheduledEventQualificationRequirementId = this.scheduledEventQualificationRequirementId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				scheduledEventQualificationRequirement = this.scheduledEventQualificationRequirement?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ScheduledEventQualificationRequirementChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ScheduledEventQualificationRequirementChangeHistory objects to avoid using the ScheduledEventQualificationRequirementChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ScheduledEventQualificationRequirementChangeHistoryOutputDTO> ToOutputDTOList(List<ScheduledEventQualificationRequirementChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ScheduledEventQualificationRequirementChangeHistoryOutputDTO> output = new List<ScheduledEventQualificationRequirementChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ScheduledEventQualificationRequirementChangeHistory scheduledEventQualificationRequirementChangeHistory in data)
			{
				output.Add(scheduledEventQualificationRequirementChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ScheduledEventQualificationRequirementChangeHistory Object.
		///
		/// </summary>
		public static Database.ScheduledEventQualificationRequirementChangeHistory FromDTO(ScheduledEventQualificationRequirementChangeHistoryDTO dto)
		{
			return new Database.ScheduledEventQualificationRequirementChangeHistory
			{
				id = dto.id,
				scheduledEventQualificationRequirementId = dto.scheduledEventQualificationRequirementId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ScheduledEventQualificationRequirementChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ScheduledEventQualificationRequirementChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.scheduledEventQualificationRequirementId = dto.scheduledEventQualificationRequirementId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ScheduledEventQualificationRequirementChangeHistory Object.
		///
		/// </summary>
		public ScheduledEventQualificationRequirementChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ScheduledEventQualificationRequirementChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				scheduledEventQualificationRequirementId = this.scheduledEventQualificationRequirementId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ScheduledEventQualificationRequirementChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ScheduledEventQualificationRequirementChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ScheduledEventQualificationRequirementChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ScheduledEventQualificationRequirementChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ScheduledEventQualificationRequirementChangeHistory scheduledEventQualificationRequirementChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (scheduledEventQualificationRequirementChangeHistory == null)
			{
				return null;
			}

			return new {
				id = scheduledEventQualificationRequirementChangeHistory.id,
				scheduledEventQualificationRequirementId = scheduledEventQualificationRequirementChangeHistory.scheduledEventQualificationRequirementId,
				versionNumber = scheduledEventQualificationRequirementChangeHistory.versionNumber,
				timeStamp = scheduledEventQualificationRequirementChangeHistory.timeStamp,
				userId = scheduledEventQualificationRequirementChangeHistory.userId,
				data = scheduledEventQualificationRequirementChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ScheduledEventQualificationRequirementChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ScheduledEventQualificationRequirementChangeHistory scheduledEventQualificationRequirementChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (scheduledEventQualificationRequirementChangeHistory == null)
			{
				return null;
			}

			return new {
				id = scheduledEventQualificationRequirementChangeHistory.id,
				scheduledEventQualificationRequirementId = scheduledEventQualificationRequirementChangeHistory.scheduledEventQualificationRequirementId,
				versionNumber = scheduledEventQualificationRequirementChangeHistory.versionNumber,
				timeStamp = scheduledEventQualificationRequirementChangeHistory.timeStamp,
				userId = scheduledEventQualificationRequirementChangeHistory.userId,
				data = scheduledEventQualificationRequirementChangeHistory.data,
				scheduledEventQualificationRequirement = ScheduledEventQualificationRequirement.CreateMinimalAnonymous(scheduledEventQualificationRequirementChangeHistory.scheduledEventQualificationRequirement),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ScheduledEventQualificationRequirementChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ScheduledEventQualificationRequirementChangeHistory scheduledEventQualificationRequirementChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (scheduledEventQualificationRequirementChangeHistory == null)
			{
				return null;
			}

			return new {
				id = scheduledEventQualificationRequirementChangeHistory.id,
				name = scheduledEventQualificationRequirementChangeHistory.id,
				description = scheduledEventQualificationRequirementChangeHistory.id
			 };
		}
	}
}
