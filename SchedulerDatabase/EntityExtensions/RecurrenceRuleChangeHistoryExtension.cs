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
	public partial class RecurrenceRuleChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)recurrenceRuleId; }
			set { recurrenceRuleId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class RecurrenceRuleChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 recurrenceRuleId { get; set; }
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
		public class RecurrenceRuleChangeHistoryOutputDTO : RecurrenceRuleChangeHistoryDTO
		{
			public RecurrenceRule.RecurrenceRuleDTO recurrenceRule { get; set; }
		}


		/// <summary>
		///
		/// Converts a RecurrenceRuleChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public RecurrenceRuleChangeHistoryDTO ToDTO()
		{
			return new RecurrenceRuleChangeHistoryDTO
			{
				id = this.id,
				recurrenceRuleId = this.recurrenceRuleId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a RecurrenceRuleChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<RecurrenceRuleChangeHistoryDTO> ToDTOList(List<RecurrenceRuleChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<RecurrenceRuleChangeHistoryDTO> output = new List<RecurrenceRuleChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (RecurrenceRuleChangeHistory recurrenceRuleChangeHistory in data)
			{
				output.Add(recurrenceRuleChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a RecurrenceRuleChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the RecurrenceRuleChangeHistory Entity type directly.
		///
		/// </summary>
		public RecurrenceRuleChangeHistoryOutputDTO ToOutputDTO()
		{
			return new RecurrenceRuleChangeHistoryOutputDTO
			{
				id = this.id,
				recurrenceRuleId = this.recurrenceRuleId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				recurrenceRule = this.recurrenceRule?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a RecurrenceRuleChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of RecurrenceRuleChangeHistory objects to avoid using the RecurrenceRuleChangeHistory entity type directly.
		///
		/// </summary>
		public static List<RecurrenceRuleChangeHistoryOutputDTO> ToOutputDTOList(List<RecurrenceRuleChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<RecurrenceRuleChangeHistoryOutputDTO> output = new List<RecurrenceRuleChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (RecurrenceRuleChangeHistory recurrenceRuleChangeHistory in data)
			{
				output.Add(recurrenceRuleChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a RecurrenceRuleChangeHistory Object.
		///
		/// </summary>
		public static Database.RecurrenceRuleChangeHistory FromDTO(RecurrenceRuleChangeHistoryDTO dto)
		{
			return new Database.RecurrenceRuleChangeHistory
			{
				id = dto.id,
				recurrenceRuleId = dto.recurrenceRuleId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a RecurrenceRuleChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(RecurrenceRuleChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.recurrenceRuleId = dto.recurrenceRuleId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a RecurrenceRuleChangeHistory Object.
		///
		/// </summary>
		public RecurrenceRuleChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new RecurrenceRuleChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				recurrenceRuleId = this.recurrenceRuleId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a RecurrenceRuleChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a RecurrenceRuleChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a RecurrenceRuleChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a RecurrenceRuleChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.RecurrenceRuleChangeHistory recurrenceRuleChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (recurrenceRuleChangeHistory == null)
			{
				return null;
			}

			return new {
				id = recurrenceRuleChangeHistory.id,
				recurrenceRuleId = recurrenceRuleChangeHistory.recurrenceRuleId,
				versionNumber = recurrenceRuleChangeHistory.versionNumber,
				timeStamp = recurrenceRuleChangeHistory.timeStamp,
				userId = recurrenceRuleChangeHistory.userId,
				data = recurrenceRuleChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a RecurrenceRuleChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(RecurrenceRuleChangeHistory recurrenceRuleChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (recurrenceRuleChangeHistory == null)
			{
				return null;
			}

			return new {
				id = recurrenceRuleChangeHistory.id,
				recurrenceRuleId = recurrenceRuleChangeHistory.recurrenceRuleId,
				versionNumber = recurrenceRuleChangeHistory.versionNumber,
				timeStamp = recurrenceRuleChangeHistory.timeStamp,
				userId = recurrenceRuleChangeHistory.userId,
				data = recurrenceRuleChangeHistory.data,
				recurrenceRule = RecurrenceRule.CreateMinimalAnonymous(recurrenceRuleChangeHistory.recurrenceRule)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a RecurrenceRuleChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(RecurrenceRuleChangeHistory recurrenceRuleChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (recurrenceRuleChangeHistory == null)
			{
				return null;
			}

			return new {
				id = recurrenceRuleChangeHistory.id,
				name = recurrenceRuleChangeHistory.id,
				description = recurrenceRuleChangeHistory.id
			 };
		}
	}
}
