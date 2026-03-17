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
	public partial class AttributeDefinitionEntity : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class AttributeDefinitionEntityDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
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
		public class AttributeDefinitionEntityOutputDTO : AttributeDefinitionEntityDTO
		{
		}


		/// <summary>
		///
		/// Converts a AttributeDefinitionEntity to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public AttributeDefinitionEntityDTO ToDTO()
		{
			return new AttributeDefinitionEntityDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a AttributeDefinitionEntity list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<AttributeDefinitionEntityDTO> ToDTOList(List<AttributeDefinitionEntity> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AttributeDefinitionEntityDTO> output = new List<AttributeDefinitionEntityDTO>();

			output.Capacity = data.Count;

			foreach (AttributeDefinitionEntity attributeDefinitionEntity in data)
			{
				output.Add(attributeDefinitionEntity.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a AttributeDefinitionEntity to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the AttributeDefinitionEntity Entity type directly.
		///
		/// </summary>
		public AttributeDefinitionEntityOutputDTO ToOutputDTO()
		{
			return new AttributeDefinitionEntityOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a AttributeDefinitionEntity list to list of Output Data Transfer Object intended to be used for serializing a list of AttributeDefinitionEntity objects to avoid using the AttributeDefinitionEntity entity type directly.
		///
		/// </summary>
		public static List<AttributeDefinitionEntityOutputDTO> ToOutputDTOList(List<AttributeDefinitionEntity> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AttributeDefinitionEntityOutputDTO> output = new List<AttributeDefinitionEntityOutputDTO>();

			output.Capacity = data.Count;

			foreach (AttributeDefinitionEntity attributeDefinitionEntity in data)
			{
				output.Add(attributeDefinitionEntity.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a AttributeDefinitionEntity Object.
		///
		/// </summary>
		public static Database.AttributeDefinitionEntity FromDTO(AttributeDefinitionEntityDTO dto)
		{
			return new Database.AttributeDefinitionEntity
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a AttributeDefinitionEntity Object.
		///
		/// </summary>
		public void ApplyDTO(AttributeDefinitionEntityDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
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
		/// Creates a deep copy clone of a AttributeDefinitionEntity Object.
		///
		/// </summary>
		public AttributeDefinitionEntity Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new AttributeDefinitionEntity{
				id = this.id,
				name = this.name,
				description = this.description,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AttributeDefinitionEntity Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AttributeDefinitionEntity Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a AttributeDefinitionEntity Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a AttributeDefinitionEntity Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.AttributeDefinitionEntity attributeDefinitionEntity)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (attributeDefinitionEntity == null)
			{
				return null;
			}

			return new {
				id = attributeDefinitionEntity.id,
				name = attributeDefinitionEntity.name,
				description = attributeDefinitionEntity.description,
				objectGuid = attributeDefinitionEntity.objectGuid,
				active = attributeDefinitionEntity.active,
				deleted = attributeDefinitionEntity.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a AttributeDefinitionEntity Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(AttributeDefinitionEntity attributeDefinitionEntity)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (attributeDefinitionEntity == null)
			{
				return null;
			}

			return new {
				id = attributeDefinitionEntity.id,
				name = attributeDefinitionEntity.name,
				description = attributeDefinitionEntity.description,
				objectGuid = attributeDefinitionEntity.objectGuid,
				active = attributeDefinitionEntity.active,
				deleted = attributeDefinitionEntity.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a AttributeDefinitionEntity Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(AttributeDefinitionEntity attributeDefinitionEntity)
		{
			//
			// Return a very minimal object.
			//
			if (attributeDefinitionEntity == null)
			{
				return null;
			}

			return new {
				id = attributeDefinitionEntity.id,
				name = attributeDefinitionEntity.name,
				description = attributeDefinitionEntity.description,
			 };
		}
	}
}
