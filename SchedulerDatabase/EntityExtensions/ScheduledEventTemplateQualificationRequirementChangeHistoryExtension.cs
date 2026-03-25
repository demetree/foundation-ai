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
	public partial class ScheduledEventTemplateQualificationRequirementChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)scheduledEventTemplateQualificationRequirementId; }
			set { scheduledEventTemplateQualificationRequirementId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ScheduledEventTemplateQualificationRequirementChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 scheduledEventTemplateQualificationRequirementId { get; set; }
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
		public class ScheduledEventTemplateQualificationRequirementChangeHistoryOutputDTO : ScheduledEventTemplateQualificationRequirementChangeHistoryDTO
		{
			public ScheduledEventTemplateQualificationRequirement.ScheduledEventTemplateQualificationRequirementDTO scheduledEventTemplateQualificationRequirement { get; set; }
		}


		/// <summary>
		///
		/// Converts a ScheduledEventTemplateQualificationRequirementChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ScheduledEventTemplateQualificationRequirementChangeHistoryDTO ToDTO()
		{
			return new ScheduledEventTemplateQualificationRequirementChangeHistoryDTO
			{
				id = this.id,
				scheduledEventTemplateQualificationRequirementId = this.scheduledEventTemplateQualificationRequirementId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ScheduledEventTemplateQualificationRequirementChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ScheduledEventTemplateQualificationRequirementChangeHistoryDTO> ToDTOList(List<ScheduledEventTemplateQualificationRequirementChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ScheduledEventTemplateQualificationRequirementChangeHistoryDTO> output = new List<ScheduledEventTemplateQualificationRequirementChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ScheduledEventTemplateQualificationRequirementChangeHistory scheduledEventTemplateQualificationRequirementChangeHistory in data)
			{
				output.Add(scheduledEventTemplateQualificationRequirementChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ScheduledEventTemplateQualificationRequirementChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ScheduledEventTemplateQualificationRequirementChangeHistory Entity type directly.
		///
		/// </summary>
		public ScheduledEventTemplateQualificationRequirementChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ScheduledEventTemplateQualificationRequirementChangeHistoryOutputDTO
			{
				id = this.id,
				scheduledEventTemplateQualificationRequirementId = this.scheduledEventTemplateQualificationRequirementId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				scheduledEventTemplateQualificationRequirement = this.scheduledEventTemplateQualificationRequirement?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ScheduledEventTemplateQualificationRequirementChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ScheduledEventTemplateQualificationRequirementChangeHistory objects to avoid using the ScheduledEventTemplateQualificationRequirementChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ScheduledEventTemplateQualificationRequirementChangeHistoryOutputDTO> ToOutputDTOList(List<ScheduledEventTemplateQualificationRequirementChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ScheduledEventTemplateQualificationRequirementChangeHistoryOutputDTO> output = new List<ScheduledEventTemplateQualificationRequirementChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ScheduledEventTemplateQualificationRequirementChangeHistory scheduledEventTemplateQualificationRequirementChangeHistory in data)
			{
				output.Add(scheduledEventTemplateQualificationRequirementChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ScheduledEventTemplateQualificationRequirementChangeHistory Object.
		///
		/// </summary>
		public static Database.ScheduledEventTemplateQualificationRequirementChangeHistory FromDTO(ScheduledEventTemplateQualificationRequirementChangeHistoryDTO dto)
		{
			return new Database.ScheduledEventTemplateQualificationRequirementChangeHistory
			{
				id = dto.id,
				scheduledEventTemplateQualificationRequirementId = dto.scheduledEventTemplateQualificationRequirementId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ScheduledEventTemplateQualificationRequirementChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ScheduledEventTemplateQualificationRequirementChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.scheduledEventTemplateQualificationRequirementId = dto.scheduledEventTemplateQualificationRequirementId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ScheduledEventTemplateQualificationRequirementChangeHistory Object.
		///
		/// </summary>
		public ScheduledEventTemplateQualificationRequirementChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ScheduledEventTemplateQualificationRequirementChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				scheduledEventTemplateQualificationRequirementId = this.scheduledEventTemplateQualificationRequirementId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ScheduledEventTemplateQualificationRequirementChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ScheduledEventTemplateQualificationRequirementChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ScheduledEventTemplateQualificationRequirementChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ScheduledEventTemplateQualificationRequirementChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ScheduledEventTemplateQualificationRequirementChangeHistory scheduledEventTemplateQualificationRequirementChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (scheduledEventTemplateQualificationRequirementChangeHistory == null)
			{
				return null;
			}

			return new {
				id = scheduledEventTemplateQualificationRequirementChangeHistory.id,
				scheduledEventTemplateQualificationRequirementId = scheduledEventTemplateQualificationRequirementChangeHistory.scheduledEventTemplateQualificationRequirementId,
				versionNumber = scheduledEventTemplateQualificationRequirementChangeHistory.versionNumber,
				timeStamp = scheduledEventTemplateQualificationRequirementChangeHistory.timeStamp,
				userId = scheduledEventTemplateQualificationRequirementChangeHistory.userId,
				data = scheduledEventTemplateQualificationRequirementChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ScheduledEventTemplateQualificationRequirementChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ScheduledEventTemplateQualificationRequirementChangeHistory scheduledEventTemplateQualificationRequirementChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (scheduledEventTemplateQualificationRequirementChangeHistory == null)
			{
				return null;
			}

			return new {
				id = scheduledEventTemplateQualificationRequirementChangeHistory.id,
				scheduledEventTemplateQualificationRequirementId = scheduledEventTemplateQualificationRequirementChangeHistory.scheduledEventTemplateQualificationRequirementId,
				versionNumber = scheduledEventTemplateQualificationRequirementChangeHistory.versionNumber,
				timeStamp = scheduledEventTemplateQualificationRequirementChangeHistory.timeStamp,
				userId = scheduledEventTemplateQualificationRequirementChangeHistory.userId,
				data = scheduledEventTemplateQualificationRequirementChangeHistory.data,
				scheduledEventTemplateQualificationRequirement = ScheduledEventTemplateQualificationRequirement.CreateMinimalAnonymous(scheduledEventTemplateQualificationRequirementChangeHistory.scheduledEventTemplateQualificationRequirement),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ScheduledEventTemplateQualificationRequirementChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ScheduledEventTemplateQualificationRequirementChangeHistory scheduledEventTemplateQualificationRequirementChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (scheduledEventTemplateQualificationRequirementChangeHistory == null)
			{
				return null;
			}

			return new {
				id = scheduledEventTemplateQualificationRequirementChangeHistory.id,
				name = scheduledEventTemplateQualificationRequirementChangeHistory.id,
				description = scheduledEventTemplateQualificationRequirementChangeHistory.id
			 };
		}
	}
}
