using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Alerting.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class SeverityType : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SeverityTypeDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			public String description { get; set; }
			[Required]
			public Int32 sortOrder { get; set; }
			public Boolean? active { get; set; }
			public Boolean? deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class SeverityTypeOutputDTO : SeverityTypeDTO
		{
		}


		/// <summary>
		///
		/// Converts a SeverityType to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SeverityTypeDTO ToDTO()
		{
			return new SeverityTypeDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				sortOrder = this.sortOrder,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SeverityType list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SeverityTypeDTO> ToDTOList(List<SeverityType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SeverityTypeDTO> output = new List<SeverityTypeDTO>();

			output.Capacity = data.Count;

			foreach (SeverityType severityType in data)
			{
				output.Add(severityType.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SeverityType to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SeverityTypeEntity type directly.
		///
		/// </summary>
		public SeverityTypeOutputDTO ToOutputDTO()
		{
			return new SeverityTypeOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				sortOrder = this.sortOrder,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SeverityType list to list of Output Data Transfer Object intended to be used for serializing a list of SeverityType objects to avoid using the SeverityType entity type directly.
		///
		/// </summary>
		public static List<SeverityTypeOutputDTO> ToOutputDTOList(List<SeverityType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SeverityTypeOutputDTO> output = new List<SeverityTypeOutputDTO>();

			output.Capacity = data.Count;

			foreach (SeverityType severityType in data)
			{
				output.Add(severityType.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SeverityType Object.
		///
		/// </summary>
		public static Database.SeverityType FromDTO(SeverityTypeDTO dto)
		{
			return new Database.SeverityType
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				sortOrder = dto.sortOrder,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SeverityType Object.
		///
		/// </summary>
		public void ApplyDTO(SeverityTypeDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.sortOrder = dto.sortOrder;
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
		/// Creates a deep copy clone of a SeverityType Object.
		///
		/// </summary>
		public SeverityType Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SeverityType{
				id = this.id,
				name = this.name,
				description = this.description,
				sortOrder = this.sortOrder,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SeverityType Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SeverityType Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SeverityType Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SeverityType Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SeverityType severityType)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (severityType == null)
			{
				return null;
			}

			return new {
				id = severityType.id,
				name = severityType.name,
				description = severityType.description,
				sortOrder = severityType.sortOrder,
				active = severityType.active,
				deleted = severityType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SeverityType Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SeverityType severityType)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (severityType == null)
			{
				return null;
			}

			return new {
				id = severityType.id,
				name = severityType.name,
				description = severityType.description,
				sortOrder = severityType.sortOrder,
				active = severityType.active,
				deleted = severityType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SeverityType Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SeverityType severityType)
		{
			//
			// Return a very minimal object.
			//
			if (severityType == null)
			{
				return null;
			}

			return new {
				id = severityType.id,
				name = severityType.name,
				description = severityType.description,
			 };
		}
	}
}
