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
	public partial class ScheduledEventTemplateChargeChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)scheduledEventTemplateChargeId; }
			set { scheduledEventTemplateChargeId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ScheduledEventTemplateChargeChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 scheduledEventTemplateChargeId { get; set; }
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
		public class ScheduledEventTemplateChargeChangeHistoryOutputDTO : ScheduledEventTemplateChargeChangeHistoryDTO
		{
			public ScheduledEventTemplateCharge.ScheduledEventTemplateChargeDTO scheduledEventTemplateCharge { get; set; }
		}


		/// <summary>
		///
		/// Converts a ScheduledEventTemplateChargeChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ScheduledEventTemplateChargeChangeHistoryDTO ToDTO()
		{
			return new ScheduledEventTemplateChargeChangeHistoryDTO
			{
				id = this.id,
				scheduledEventTemplateChargeId = this.scheduledEventTemplateChargeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ScheduledEventTemplateChargeChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ScheduledEventTemplateChargeChangeHistoryDTO> ToDTOList(List<ScheduledEventTemplateChargeChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ScheduledEventTemplateChargeChangeHistoryDTO> output = new List<ScheduledEventTemplateChargeChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ScheduledEventTemplateChargeChangeHistory scheduledEventTemplateChargeChangeHistory in data)
			{
				output.Add(scheduledEventTemplateChargeChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ScheduledEventTemplateChargeChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ScheduledEventTemplateChargeChangeHistoryEntity type directly.
		///
		/// </summary>
		public ScheduledEventTemplateChargeChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ScheduledEventTemplateChargeChangeHistoryOutputDTO
			{
				id = this.id,
				scheduledEventTemplateChargeId = this.scheduledEventTemplateChargeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				scheduledEventTemplateCharge = this.scheduledEventTemplateCharge?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ScheduledEventTemplateChargeChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ScheduledEventTemplateChargeChangeHistory objects to avoid using the ScheduledEventTemplateChargeChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ScheduledEventTemplateChargeChangeHistoryOutputDTO> ToOutputDTOList(List<ScheduledEventTemplateChargeChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ScheduledEventTemplateChargeChangeHistoryOutputDTO> output = new List<ScheduledEventTemplateChargeChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ScheduledEventTemplateChargeChangeHistory scheduledEventTemplateChargeChangeHistory in data)
			{
				output.Add(scheduledEventTemplateChargeChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ScheduledEventTemplateChargeChangeHistory Object.
		///
		/// </summary>
		public static Database.ScheduledEventTemplateChargeChangeHistory FromDTO(ScheduledEventTemplateChargeChangeHistoryDTO dto)
		{
			return new Database.ScheduledEventTemplateChargeChangeHistory
			{
				id = dto.id,
				scheduledEventTemplateChargeId = dto.scheduledEventTemplateChargeId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ScheduledEventTemplateChargeChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ScheduledEventTemplateChargeChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.scheduledEventTemplateChargeId = dto.scheduledEventTemplateChargeId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ScheduledEventTemplateChargeChangeHistory Object.
		///
		/// </summary>
		public ScheduledEventTemplateChargeChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ScheduledEventTemplateChargeChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				scheduledEventTemplateChargeId = this.scheduledEventTemplateChargeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ScheduledEventTemplateChargeChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ScheduledEventTemplateChargeChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ScheduledEventTemplateChargeChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ScheduledEventTemplateChargeChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ScheduledEventTemplateChargeChangeHistory scheduledEventTemplateChargeChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (scheduledEventTemplateChargeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = scheduledEventTemplateChargeChangeHistory.id,
				scheduledEventTemplateChargeId = scheduledEventTemplateChargeChangeHistory.scheduledEventTemplateChargeId,
				versionNumber = scheduledEventTemplateChargeChangeHistory.versionNumber,
				timeStamp = scheduledEventTemplateChargeChangeHistory.timeStamp,
				userId = scheduledEventTemplateChargeChangeHistory.userId,
				data = scheduledEventTemplateChargeChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ScheduledEventTemplateChargeChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ScheduledEventTemplateChargeChangeHistory scheduledEventTemplateChargeChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (scheduledEventTemplateChargeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = scheduledEventTemplateChargeChangeHistory.id,
				scheduledEventTemplateChargeId = scheduledEventTemplateChargeChangeHistory.scheduledEventTemplateChargeId,
				versionNumber = scheduledEventTemplateChargeChangeHistory.versionNumber,
				timeStamp = scheduledEventTemplateChargeChangeHistory.timeStamp,
				userId = scheduledEventTemplateChargeChangeHistory.userId,
				data = scheduledEventTemplateChargeChangeHistory.data,
				scheduledEventTemplateCharge = ScheduledEventTemplateCharge.CreateMinimalAnonymous(scheduledEventTemplateChargeChangeHistory.scheduledEventTemplateCharge)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ScheduledEventTemplateChargeChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ScheduledEventTemplateChargeChangeHistory scheduledEventTemplateChargeChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (scheduledEventTemplateChargeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = scheduledEventTemplateChargeChangeHistory.id,
				name = scheduledEventTemplateChargeChangeHistory.id,
				description = scheduledEventTemplateChargeChangeHistory.id
			 };
		}
	}
}
