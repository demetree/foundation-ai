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
	public partial class RecurrenceFrequency : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class RecurrenceFrequencyDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
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
		public class RecurrenceFrequencyOutputDTO : RecurrenceFrequencyDTO
		{
		}


		/// <summary>
		///
		/// Converts a RecurrenceFrequency to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public RecurrenceFrequencyDTO ToDTO()
		{
			return new RecurrenceFrequencyDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a RecurrenceFrequency list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<RecurrenceFrequencyDTO> ToDTOList(List<RecurrenceFrequency> data)
		{
			if (data == null)
			{
				return null;
			}

			List<RecurrenceFrequencyDTO> output = new List<RecurrenceFrequencyDTO>();

			output.Capacity = data.Count;

			foreach (RecurrenceFrequency recurrenceFrequency in data)
			{
				output.Add(recurrenceFrequency.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a RecurrenceFrequency to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the RecurrenceFrequencyEntity type directly.
		///
		/// </summary>
		public RecurrenceFrequencyOutputDTO ToOutputDTO()
		{
			return new RecurrenceFrequencyOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a RecurrenceFrequency list to list of Output Data Transfer Object intended to be used for serializing a list of RecurrenceFrequency objects to avoid using the RecurrenceFrequency entity type directly.
		///
		/// </summary>
		public static List<RecurrenceFrequencyOutputDTO> ToOutputDTOList(List<RecurrenceFrequency> data)
		{
			if (data == null)
			{
				return null;
			}

			List<RecurrenceFrequencyOutputDTO> output = new List<RecurrenceFrequencyOutputDTO>();

			output.Capacity = data.Count;

			foreach (RecurrenceFrequency recurrenceFrequency in data)
			{
				output.Add(recurrenceFrequency.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a RecurrenceFrequency Object.
		///
		/// </summary>
		public static Database.RecurrenceFrequency FromDTO(RecurrenceFrequencyDTO dto)
		{
			return new Database.RecurrenceFrequency
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a RecurrenceFrequency Object.
		///
		/// </summary>
		public void ApplyDTO(RecurrenceFrequencyDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
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
		/// Creates a deep copy clone of a RecurrenceFrequency Object.
		///
		/// </summary>
		public RecurrenceFrequency Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new RecurrenceFrequency{
				id = this.id,
				name = this.name,
				description = this.description,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a RecurrenceFrequency Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a RecurrenceFrequency Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a RecurrenceFrequency Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a RecurrenceFrequency Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.RecurrenceFrequency recurrenceFrequency)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (recurrenceFrequency == null)
			{
				return null;
			}

			return new {
				id = recurrenceFrequency.id,
				name = recurrenceFrequency.name,
				description = recurrenceFrequency.description,
				sequence = recurrenceFrequency.sequence,
				objectGuid = recurrenceFrequency.objectGuid,
				active = recurrenceFrequency.active,
				deleted = recurrenceFrequency.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a RecurrenceFrequency Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(RecurrenceFrequency recurrenceFrequency)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (recurrenceFrequency == null)
			{
				return null;
			}

			return new {
				id = recurrenceFrequency.id,
				name = recurrenceFrequency.name,
				description = recurrenceFrequency.description,
				sequence = recurrenceFrequency.sequence,
				objectGuid = recurrenceFrequency.objectGuid,
				active = recurrenceFrequency.active,
				deleted = recurrenceFrequency.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a RecurrenceFrequency Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(RecurrenceFrequency recurrenceFrequency)
		{
			//
			// Return a very minimal object.
			//
			if (recurrenceFrequency == null)
			{
				return null;
			}

			return new {
				id = recurrenceFrequency.id,
				name = recurrenceFrequency.name,
				description = recurrenceFrequency.description,
			 };
		}
	}
}
