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
	public partial class ScheduledEventTemplateChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)scheduledEventTemplateId; }
			set { scheduledEventTemplateId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ScheduledEventTemplateChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 scheduledEventTemplateId { get; set; }
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
		public class ScheduledEventTemplateChangeHistoryOutputDTO : ScheduledEventTemplateChangeHistoryDTO
		{
			public ScheduledEventTemplate.ScheduledEventTemplateDTO scheduledEventTemplate { get; set; }
		}


		/// <summary>
		///
		/// Converts a ScheduledEventTemplateChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ScheduledEventTemplateChangeHistoryDTO ToDTO()
		{
			return new ScheduledEventTemplateChangeHistoryDTO
			{
				id = this.id,
				scheduledEventTemplateId = this.scheduledEventTemplateId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ScheduledEventTemplateChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ScheduledEventTemplateChangeHistoryDTO> ToDTOList(List<ScheduledEventTemplateChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ScheduledEventTemplateChangeHistoryDTO> output = new List<ScheduledEventTemplateChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ScheduledEventTemplateChangeHistory scheduledEventTemplateChangeHistory in data)
			{
				output.Add(scheduledEventTemplateChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ScheduledEventTemplateChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ScheduledEventTemplateChangeHistoryEntity type directly.
		///
		/// </summary>
		public ScheduledEventTemplateChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ScheduledEventTemplateChangeHistoryOutputDTO
			{
				id = this.id,
				scheduledEventTemplateId = this.scheduledEventTemplateId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				scheduledEventTemplate = this.scheduledEventTemplate?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ScheduledEventTemplateChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ScheduledEventTemplateChangeHistory objects to avoid using the ScheduledEventTemplateChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ScheduledEventTemplateChangeHistoryOutputDTO> ToOutputDTOList(List<ScheduledEventTemplateChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ScheduledEventTemplateChangeHistoryOutputDTO> output = new List<ScheduledEventTemplateChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ScheduledEventTemplateChangeHistory scheduledEventTemplateChangeHistory in data)
			{
				output.Add(scheduledEventTemplateChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ScheduledEventTemplateChangeHistory Object.
		///
		/// </summary>
		public static Database.ScheduledEventTemplateChangeHistory FromDTO(ScheduledEventTemplateChangeHistoryDTO dto)
		{
			return new Database.ScheduledEventTemplateChangeHistory
			{
				id = dto.id,
				scheduledEventTemplateId = dto.scheduledEventTemplateId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ScheduledEventTemplateChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ScheduledEventTemplateChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.scheduledEventTemplateId = dto.scheduledEventTemplateId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ScheduledEventTemplateChangeHistory Object.
		///
		/// </summary>
		public ScheduledEventTemplateChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ScheduledEventTemplateChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				scheduledEventTemplateId = this.scheduledEventTemplateId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ScheduledEventTemplateChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ScheduledEventTemplateChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ScheduledEventTemplateChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ScheduledEventTemplateChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ScheduledEventTemplateChangeHistory scheduledEventTemplateChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (scheduledEventTemplateChangeHistory == null)
			{
				return null;
			}

			return new {
				id = scheduledEventTemplateChangeHistory.id,
				scheduledEventTemplateId = scheduledEventTemplateChangeHistory.scheduledEventTemplateId,
				versionNumber = scheduledEventTemplateChangeHistory.versionNumber,
				timeStamp = scheduledEventTemplateChangeHistory.timeStamp,
				userId = scheduledEventTemplateChangeHistory.userId,
				data = scheduledEventTemplateChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ScheduledEventTemplateChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ScheduledEventTemplateChangeHistory scheduledEventTemplateChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (scheduledEventTemplateChangeHistory == null)
			{
				return null;
			}

			return new {
				id = scheduledEventTemplateChangeHistory.id,
				scheduledEventTemplateId = scheduledEventTemplateChangeHistory.scheduledEventTemplateId,
				versionNumber = scheduledEventTemplateChangeHistory.versionNumber,
				timeStamp = scheduledEventTemplateChangeHistory.timeStamp,
				userId = scheduledEventTemplateChangeHistory.userId,
				data = scheduledEventTemplateChangeHistory.data,
				scheduledEventTemplate = ScheduledEventTemplate.CreateMinimalAnonymous(scheduledEventTemplateChangeHistory.scheduledEventTemplate),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ScheduledEventTemplateChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ScheduledEventTemplateChangeHistory scheduledEventTemplateChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (scheduledEventTemplateChangeHistory == null)
			{
				return null;
			}

			return new {
				id = scheduledEventTemplateChangeHistory.id,
				name = scheduledEventTemplateChangeHistory.id,
				description = scheduledEventTemplateChangeHistory.id
			 };
		}
	}
}
