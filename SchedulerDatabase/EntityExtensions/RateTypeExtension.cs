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
	public partial class RateType : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class RateTypeDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
			public Int32? sequence { get; set; }
			public String color { get; set; }
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
		public class RateTypeOutputDTO : RateTypeDTO
		{
		}


		/// <summary>
		///
		/// Converts a RateType to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public RateTypeDTO ToDTO()
		{
			return new RateTypeDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				sequence = this.sequence,
				color = this.color,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a RateType list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<RateTypeDTO> ToDTOList(List<RateType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<RateTypeDTO> output = new List<RateTypeDTO>();

			output.Capacity = data.Count;

			foreach (RateType rateType in data)
			{
				output.Add(rateType.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a RateType to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the RateType Entity type directly.
		///
		/// </summary>
		public RateTypeOutputDTO ToOutputDTO()
		{
			return new RateTypeOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				sequence = this.sequence,
				color = this.color,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a RateType list to list of Output Data Transfer Object intended to be used for serializing a list of RateType objects to avoid using the RateType entity type directly.
		///
		/// </summary>
		public static List<RateTypeOutputDTO> ToOutputDTOList(List<RateType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<RateTypeOutputDTO> output = new List<RateTypeOutputDTO>();

			output.Capacity = data.Count;

			foreach (RateType rateType in data)
			{
				output.Add(rateType.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a RateType Object.
		///
		/// </summary>
		public static Database.RateType FromDTO(RateTypeDTO dto)
		{
			return new Database.RateType
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				sequence = dto.sequence,
				color = dto.color,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a RateType Object.
		///
		/// </summary>
		public void ApplyDTO(RateTypeDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.sequence = dto.sequence;
			this.color = dto.color;
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
		/// Creates a deep copy clone of a RateType Object.
		///
		/// </summary>
		public RateType Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new RateType{
				id = this.id,
				tenantGuid = this.tenantGuid,
				name = this.name,
				description = this.description,
				sequence = this.sequence,
				color = this.color,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a RateType Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a RateType Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a RateType Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a RateType Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.RateType rateType)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (rateType == null)
			{
				return null;
			}

			return new {
				id = rateType.id,
				name = rateType.name,
				description = rateType.description,
				sequence = rateType.sequence,
				color = rateType.color,
				objectGuid = rateType.objectGuid,
				active = rateType.active,
				deleted = rateType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a RateType Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(RateType rateType)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (rateType == null)
			{
				return null;
			}

			return new {
				id = rateType.id,
				name = rateType.name,
				description = rateType.description,
				sequence = rateType.sequence,
				color = rateType.color,
				objectGuid = rateType.objectGuid,
				active = rateType.active,
				deleted = rateType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a RateType Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(RateType rateType)
		{
			//
			// Return a very minimal object.
			//
			if (rateType == null)
			{
				return null;
			}

			return new {
				id = rateType.id,
				name = rateType.name,
				description = rateType.description,
			 };
		}
	}
}
