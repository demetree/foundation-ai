using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Scheduler.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class TimeZone : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class TimeZoneDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
			[Required]
			public String ianaTimeZone { get; set; }
			[Required]
			public String abbreviation { get; set; }
			[Required]
			public String abbreviationDaylightSavings { get; set; }
			[Required]
			public Boolean supportsDaylightSavings { get; set; }
			[Required]
			public Single standardUTCOffsetHours { get; set; }
			[Required]
			public Single dstUTCOffsetHours { get; set; }
			public Int32? sequence { get; set; }
			[Required]
			public Guid objectGuid { get; set; }
			public Boolean? active { get; set; }
			public Boolean? deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class TimeZoneOutputDTO : TimeZoneDTO
		{
		}


		/// <summary>
		///
		/// Converts a TimeZone to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public TimeZoneDTO ToDTO()
		{
			return new TimeZoneDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				ianaTimeZone = this.ianaTimeZone,
				abbreviation = this.abbreviation,
				abbreviationDaylightSavings = this.abbreviationDaylightSavings,
				supportsDaylightSavings = this.supportsDaylightSavings,
				standardUTCOffsetHours = this.standardUTCOffsetHours,
				dstUTCOffsetHours = this.dstUTCOffsetHours,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a TimeZone list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<TimeZoneDTO> ToDTOList(List<TimeZone> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TimeZoneDTO> output = new List<TimeZoneDTO>();

			output.Capacity = data.Count;

			foreach (TimeZone timeZone in data)
			{
				output.Add(timeZone.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a TimeZone to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the TimeZone Entity type directly.
		///
		/// </summary>
		public TimeZoneOutputDTO ToOutputDTO()
		{
			return new TimeZoneOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				ianaTimeZone = this.ianaTimeZone,
				abbreviation = this.abbreviation,
				abbreviationDaylightSavings = this.abbreviationDaylightSavings,
				supportsDaylightSavings = this.supportsDaylightSavings,
				standardUTCOffsetHours = this.standardUTCOffsetHours,
				dstUTCOffsetHours = this.dstUTCOffsetHours,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a TimeZone list to list of Output Data Transfer Object intended to be used for serializing a list of TimeZone objects to avoid using the TimeZone entity type directly.
		///
		/// </summary>
		public static List<TimeZoneOutputDTO> ToOutputDTOList(List<TimeZone> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TimeZoneOutputDTO> output = new List<TimeZoneOutputDTO>();

			output.Capacity = data.Count;

			foreach (TimeZone timeZone in data)
			{
				output.Add(timeZone.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a TimeZone Object.
		///
		/// </summary>
		public static Database.TimeZone FromDTO(TimeZoneDTO dto)
		{
			return new Database.TimeZone
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				ianaTimeZone = dto.ianaTimeZone,
				abbreviation = dto.abbreviation,
				abbreviationDaylightSavings = dto.abbreviationDaylightSavings,
				supportsDaylightSavings = dto.supportsDaylightSavings,
				standardUTCOffsetHours = dto.standardUTCOffsetHours,
				dstUTCOffsetHours = dto.dstUTCOffsetHours,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a TimeZone Object.
		///
		/// </summary>
		public void ApplyDTO(TimeZoneDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.ianaTimeZone = dto.ianaTimeZone;
			this.abbreviation = dto.abbreviation;
			this.abbreviationDaylightSavings = dto.abbreviationDaylightSavings;
			this.supportsDaylightSavings = dto.supportsDaylightSavings;
			this.standardUTCOffsetHours = dto.standardUTCOffsetHours;
			this.dstUTCOffsetHours = dto.dstUTCOffsetHours;
			this.sequence = dto.sequence;
			this.objectGuid = dto.objectGuid;
			if (dto.active.HasValue == true)
			{
				this.active = dto.active.Value;
			}
			if (dto.deleted.HasValue == true)
			{
				this.deleted = dto.deleted.Value;
			}
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a TimeZone Object.
		///
		/// </summary>
		public TimeZone Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new TimeZone{
				id = this.id,
				name = this.name,
				description = this.description,
				ianaTimeZone = this.ianaTimeZone,
				abbreviation = this.abbreviation,
				abbreviationDaylightSavings = this.abbreviationDaylightSavings,
				supportsDaylightSavings = this.supportsDaylightSavings,
				standardUTCOffsetHours = this.standardUTCOffsetHours,
				dstUTCOffsetHours = this.dstUTCOffsetHours,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TimeZone Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TimeZone Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a TimeZone Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a TimeZone Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.TimeZone timeZone)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (timeZone == null)
			{
				return null;
			}

			return new {
				id = timeZone.id,
				name = timeZone.name,
				description = timeZone.description,
				ianaTimeZone = timeZone.ianaTimeZone,
				abbreviation = timeZone.abbreviation,
				abbreviationDaylightSavings = timeZone.abbreviationDaylightSavings,
				supportsDaylightSavings = timeZone.supportsDaylightSavings,
				standardUTCOffsetHours = timeZone.standardUTCOffsetHours,
				dstUTCOffsetHours = timeZone.dstUTCOffsetHours,
				sequence = timeZone.sequence,
				objectGuid = timeZone.objectGuid,
				active = timeZone.active,
				deleted = timeZone.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a TimeZone Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(TimeZone timeZone)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (timeZone == null)
			{
				return null;
			}

			return new {
				id = timeZone.id,
				name = timeZone.name,
				description = timeZone.description,
				ianaTimeZone = timeZone.ianaTimeZone,
				abbreviation = timeZone.abbreviation,
				abbreviationDaylightSavings = timeZone.abbreviationDaylightSavings,
				supportsDaylightSavings = timeZone.supportsDaylightSavings,
				standardUTCOffsetHours = timeZone.standardUTCOffsetHours,
				dstUTCOffsetHours = timeZone.dstUTCOffsetHours,
				sequence = timeZone.sequence,
				objectGuid = timeZone.objectGuid,
				active = timeZone.active,
				deleted = timeZone.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a TimeZone Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(TimeZone timeZone)
		{
			//
			// Return a very minimal object.
			//
			if (timeZone == null)
			{
				return null;
			}

			return new {
				id = timeZone.id,
				name = timeZone.name,
				description = timeZone.description,
			 };
		}
	}
}
