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
	public partial class RecurrenceExceptionChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)recurrenceExceptionId; }
			set { recurrenceExceptionId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class RecurrenceExceptionChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 recurrenceExceptionId { get; set; }
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
		public class RecurrenceExceptionChangeHistoryOutputDTO : RecurrenceExceptionChangeHistoryDTO
		{
			public RecurrenceException.RecurrenceExceptionDTO recurrenceException { get; set; }
		}


		/// <summary>
		///
		/// Converts a RecurrenceExceptionChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public RecurrenceExceptionChangeHistoryDTO ToDTO()
		{
			return new RecurrenceExceptionChangeHistoryDTO
			{
				id = this.id,
				recurrenceExceptionId = this.recurrenceExceptionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a RecurrenceExceptionChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<RecurrenceExceptionChangeHistoryDTO> ToDTOList(List<RecurrenceExceptionChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<RecurrenceExceptionChangeHistoryDTO> output = new List<RecurrenceExceptionChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (RecurrenceExceptionChangeHistory recurrenceExceptionChangeHistory in data)
			{
				output.Add(recurrenceExceptionChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a RecurrenceExceptionChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the RecurrenceExceptionChangeHistoryEntity type directly.
		///
		/// </summary>
		public RecurrenceExceptionChangeHistoryOutputDTO ToOutputDTO()
		{
			return new RecurrenceExceptionChangeHistoryOutputDTO
			{
				id = this.id,
				recurrenceExceptionId = this.recurrenceExceptionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				recurrenceException = this.recurrenceException?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a RecurrenceExceptionChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of RecurrenceExceptionChangeHistory objects to avoid using the RecurrenceExceptionChangeHistory entity type directly.
		///
		/// </summary>
		public static List<RecurrenceExceptionChangeHistoryOutputDTO> ToOutputDTOList(List<RecurrenceExceptionChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<RecurrenceExceptionChangeHistoryOutputDTO> output = new List<RecurrenceExceptionChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (RecurrenceExceptionChangeHistory recurrenceExceptionChangeHistory in data)
			{
				output.Add(recurrenceExceptionChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a RecurrenceExceptionChangeHistory Object.
		///
		/// </summary>
		public static Database.RecurrenceExceptionChangeHistory FromDTO(RecurrenceExceptionChangeHistoryDTO dto)
		{
			return new Database.RecurrenceExceptionChangeHistory
			{
				id = dto.id,
				recurrenceExceptionId = dto.recurrenceExceptionId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a RecurrenceExceptionChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(RecurrenceExceptionChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.recurrenceExceptionId = dto.recurrenceExceptionId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a RecurrenceExceptionChangeHistory Object.
		///
		/// </summary>
		public RecurrenceExceptionChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new RecurrenceExceptionChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				recurrenceExceptionId = this.recurrenceExceptionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a RecurrenceExceptionChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a RecurrenceExceptionChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a RecurrenceExceptionChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a RecurrenceExceptionChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.RecurrenceExceptionChangeHistory recurrenceExceptionChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (recurrenceExceptionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = recurrenceExceptionChangeHistory.id,
				recurrenceExceptionId = recurrenceExceptionChangeHistory.recurrenceExceptionId,
				versionNumber = recurrenceExceptionChangeHistory.versionNumber,
				timeStamp = recurrenceExceptionChangeHistory.timeStamp,
				userId = recurrenceExceptionChangeHistory.userId,
				data = recurrenceExceptionChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a RecurrenceExceptionChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(RecurrenceExceptionChangeHistory recurrenceExceptionChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (recurrenceExceptionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = recurrenceExceptionChangeHistory.id,
				recurrenceExceptionId = recurrenceExceptionChangeHistory.recurrenceExceptionId,
				versionNumber = recurrenceExceptionChangeHistory.versionNumber,
				timeStamp = recurrenceExceptionChangeHistory.timeStamp,
				userId = recurrenceExceptionChangeHistory.userId,
				data = recurrenceExceptionChangeHistory.data,
				recurrenceException = RecurrenceException.CreateMinimalAnonymous(recurrenceExceptionChangeHistory.recurrenceException),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a RecurrenceExceptionChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(RecurrenceExceptionChangeHistory recurrenceExceptionChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (recurrenceExceptionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = recurrenceExceptionChangeHistory.id,
				name = recurrenceExceptionChangeHistory.id,
				description = recurrenceExceptionChangeHistory.id
			 };
		}
	}
}
