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
	public partial class AttributeDefinitionType : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class AttributeDefinitionTypeDTO
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
		public class AttributeDefinitionTypeOutputDTO : AttributeDefinitionTypeDTO
		{
		}


		/// <summary>
		///
		/// Converts a AttributeDefinitionType to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public AttributeDefinitionTypeDTO ToDTO()
		{
			return new AttributeDefinitionTypeDTO
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
		/// Converts a AttributeDefinitionType list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<AttributeDefinitionTypeDTO> ToDTOList(List<AttributeDefinitionType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AttributeDefinitionTypeDTO> output = new List<AttributeDefinitionTypeDTO>();

			output.Capacity = data.Count;

			foreach (AttributeDefinitionType attributeDefinitionType in data)
			{
				output.Add(attributeDefinitionType.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a AttributeDefinitionType to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the AttributeDefinitionTypeEntity type directly.
		///
		/// </summary>
		public AttributeDefinitionTypeOutputDTO ToOutputDTO()
		{
			return new AttributeDefinitionTypeOutputDTO
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
		/// Converts a AttributeDefinitionType list to list of Output Data Transfer Object intended to be used for serializing a list of AttributeDefinitionType objects to avoid using the AttributeDefinitionType entity type directly.
		///
		/// </summary>
		public static List<AttributeDefinitionTypeOutputDTO> ToOutputDTOList(List<AttributeDefinitionType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AttributeDefinitionTypeOutputDTO> output = new List<AttributeDefinitionTypeOutputDTO>();

			output.Capacity = data.Count;

			foreach (AttributeDefinitionType attributeDefinitionType in data)
			{
				output.Add(attributeDefinitionType.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a AttributeDefinitionType Object.
		///
		/// </summary>
		public static Database.AttributeDefinitionType FromDTO(AttributeDefinitionTypeDTO dto)
		{
			return new Database.AttributeDefinitionType
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
		/// Applies the values from an INPUT DTO to a AttributeDefinitionType Object.
		///
		/// </summary>
		public void ApplyDTO(AttributeDefinitionTypeDTO dto)
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
		/// Creates a deep copy clone of a AttributeDefinitionType Object.
		///
		/// </summary>
		public AttributeDefinitionType Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new AttributeDefinitionType{
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
        /// Creates an anonymous object containing properties from a AttributeDefinitionType Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AttributeDefinitionType Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a AttributeDefinitionType Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a AttributeDefinitionType Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.AttributeDefinitionType attributeDefinitionType)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (attributeDefinitionType == null)
			{
				return null;
			}

			return new {
				id = attributeDefinitionType.id,
				name = attributeDefinitionType.name,
				description = attributeDefinitionType.description,
				sequence = attributeDefinitionType.sequence,
				objectGuid = attributeDefinitionType.objectGuid,
				active = attributeDefinitionType.active,
				deleted = attributeDefinitionType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a AttributeDefinitionType Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(AttributeDefinitionType attributeDefinitionType)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (attributeDefinitionType == null)
			{
				return null;
			}

			return new {
				id = attributeDefinitionType.id,
				name = attributeDefinitionType.name,
				description = attributeDefinitionType.description,
				sequence = attributeDefinitionType.sequence,
				objectGuid = attributeDefinitionType.objectGuid,
				active = attributeDefinitionType.active,
				deleted = attributeDefinitionType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a AttributeDefinitionType Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(AttributeDefinitionType attributeDefinitionType)
		{
			//
			// Return a very minimal object.
			//
			if (attributeDefinitionType == null)
			{
				return null;
			}

			return new {
				id = attributeDefinitionType.id,
				name = attributeDefinitionType.name,
				description = attributeDefinitionType.description,
			 };
		}
	}
}
