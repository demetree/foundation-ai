using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.BMC.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class PartType : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class PartTypeDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
			[Required]
			public Boolean isUserVisible { get; set; }
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
		public class PartTypeOutputDTO : PartTypeDTO
		{
		}


		/// <summary>
		///
		/// Converts a PartType to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public PartTypeDTO ToDTO()
		{
			return new PartTypeDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				isUserVisible = this.isUserVisible,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a PartType list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<PartTypeDTO> ToDTOList(List<PartType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PartTypeDTO> output = new List<PartTypeDTO>();

			output.Capacity = data.Count;

			foreach (PartType partType in data)
			{
				output.Add(partType.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a PartType to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the PartTypeEntity type directly.
		///
		/// </summary>
		public PartTypeOutputDTO ToOutputDTO()
		{
			return new PartTypeOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				isUserVisible = this.isUserVisible,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a PartType list to list of Output Data Transfer Object intended to be used for serializing a list of PartType objects to avoid using the PartType entity type directly.
		///
		/// </summary>
		public static List<PartTypeOutputDTO> ToOutputDTOList(List<PartType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PartTypeOutputDTO> output = new List<PartTypeOutputDTO>();

			output.Capacity = data.Count;

			foreach (PartType partType in data)
			{
				output.Add(partType.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a PartType Object.
		///
		/// </summary>
		public static Database.PartType FromDTO(PartTypeDTO dto)
		{
			return new Database.PartType
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				isUserVisible = dto.isUserVisible,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a PartType Object.
		///
		/// </summary>
		public void ApplyDTO(PartTypeDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.isUserVisible = dto.isUserVisible;
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
		/// Creates a deep copy clone of a PartType Object.
		///
		/// </summary>
		public PartType Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new PartType{
				id = this.id,
				name = this.name,
				description = this.description,
				isUserVisible = this.isUserVisible,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PartType Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PartType Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a PartType Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a PartType Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.PartType partType)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (partType == null)
			{
				return null;
			}

			return new {
				id = partType.id,
				name = partType.name,
				description = partType.description,
				isUserVisible = partType.isUserVisible,
				sequence = partType.sequence,
				objectGuid = partType.objectGuid,
				active = partType.active,
				deleted = partType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a PartType Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(PartType partType)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (partType == null)
			{
				return null;
			}

			return new {
				id = partType.id,
				name = partType.name,
				description = partType.description,
				isUserVisible = partType.isUserVisible,
				sequence = partType.sequence,
				objectGuid = partType.objectGuid,
				active = partType.active,
				deleted = partType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a PartType Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(PartType partType)
		{
			//
			// Return a very minimal object.
			//
			if (partType == null)
			{
				return null;
			}

			return new {
				id = partType.id,
				name = partType.name,
				description = partType.description,
			 };
		}
	}
}
