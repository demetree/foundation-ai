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
	public partial class CalendarChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)calendarId; }
			set { calendarId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class CalendarChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 calendarId { get; set; }
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
		public class CalendarChangeHistoryOutputDTO : CalendarChangeHistoryDTO
		{
			public Calendar.CalendarDTO calendar { get; set; }
		}


		/// <summary>
		///
		/// Converts a CalendarChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public CalendarChangeHistoryDTO ToDTO()
		{
			return new CalendarChangeHistoryDTO
			{
				id = this.id,
				calendarId = this.calendarId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a CalendarChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<CalendarChangeHistoryDTO> ToDTOList(List<CalendarChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<CalendarChangeHistoryDTO> output = new List<CalendarChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (CalendarChangeHistory calendarChangeHistory in data)
			{
				output.Add(calendarChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a CalendarChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the CalendarChangeHistory Entity type directly.
		///
		/// </summary>
		public CalendarChangeHistoryOutputDTO ToOutputDTO()
		{
			return new CalendarChangeHistoryOutputDTO
			{
				id = this.id,
				calendarId = this.calendarId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				calendar = this.calendar?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a CalendarChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of CalendarChangeHistory objects to avoid using the CalendarChangeHistory entity type directly.
		///
		/// </summary>
		public static List<CalendarChangeHistoryOutputDTO> ToOutputDTOList(List<CalendarChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<CalendarChangeHistoryOutputDTO> output = new List<CalendarChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (CalendarChangeHistory calendarChangeHistory in data)
			{
				output.Add(calendarChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a CalendarChangeHistory Object.
		///
		/// </summary>
		public static Database.CalendarChangeHistory FromDTO(CalendarChangeHistoryDTO dto)
		{
			return new Database.CalendarChangeHistory
			{
				id = dto.id,
				calendarId = dto.calendarId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a CalendarChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(CalendarChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.calendarId = dto.calendarId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a CalendarChangeHistory Object.
		///
		/// </summary>
		public CalendarChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new CalendarChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				calendarId = this.calendarId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a CalendarChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a CalendarChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a CalendarChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a CalendarChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.CalendarChangeHistory calendarChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (calendarChangeHistory == null)
			{
				return null;
			}

			return new {
				id = calendarChangeHistory.id,
				calendarId = calendarChangeHistory.calendarId,
				versionNumber = calendarChangeHistory.versionNumber,
				timeStamp = calendarChangeHistory.timeStamp,
				userId = calendarChangeHistory.userId,
				data = calendarChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a CalendarChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(CalendarChangeHistory calendarChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (calendarChangeHistory == null)
			{
				return null;
			}

			return new {
				id = calendarChangeHistory.id,
				calendarId = calendarChangeHistory.calendarId,
				versionNumber = calendarChangeHistory.versionNumber,
				timeStamp = calendarChangeHistory.timeStamp,
				userId = calendarChangeHistory.userId,
				data = calendarChangeHistory.data,
				calendar = Calendar.CreateMinimalAnonymous(calendarChangeHistory.calendar),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a CalendarChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(CalendarChangeHistory calendarChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (calendarChangeHistory == null)
			{
				return null;
			}

			return new {
				id = calendarChangeHistory.id,
				name = calendarChangeHistory.id,
				description = calendarChangeHistory.id
			 };
		}
	}
}
