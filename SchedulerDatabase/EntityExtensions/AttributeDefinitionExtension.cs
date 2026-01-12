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
	public partial class AttributeDefinition : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class AttributeDefinitionDTO
		{
			public Int32 id { get; set; }
			public String entityName { get; set; }
			public String key { get; set; }
			public String label { get; set; }
			public String type { get; set; }
			public String options { get; set; }
			[Required]
			public Boolean isRequired { get; set; }
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
		public class AttributeDefinitionOutputDTO : AttributeDefinitionDTO
		{
		}


		/// <summary>
		///
		/// Converts a AttributeDefinition to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public AttributeDefinitionDTO ToDTO()
		{
			return new AttributeDefinitionDTO
			{
				id = this.id,
				entityName = this.entityName,
				key = this.key,
				label = this.label,
				type = this.type,
				options = this.options,
				isRequired = this.isRequired,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a AttributeDefinition list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<AttributeDefinitionDTO> ToDTOList(List<AttributeDefinition> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AttributeDefinitionDTO> output = new List<AttributeDefinitionDTO>();

			output.Capacity = data.Count;

			foreach (AttributeDefinition attributeDefinition in data)
			{
				output.Add(attributeDefinition.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a AttributeDefinition to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the AttributeDefinitionEntity type directly.
		///
		/// </summary>
		public AttributeDefinitionOutputDTO ToOutputDTO()
		{
			return new AttributeDefinitionOutputDTO
			{
				id = this.id,
				entityName = this.entityName,
				key = this.key,
				label = this.label,
				type = this.type,
				options = this.options,
				isRequired = this.isRequired,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a AttributeDefinition list to list of Output Data Transfer Object intended to be used for serializing a list of AttributeDefinition objects to avoid using the AttributeDefinition entity type directly.
		///
		/// </summary>
		public static List<AttributeDefinitionOutputDTO> ToOutputDTOList(List<AttributeDefinition> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AttributeDefinitionOutputDTO> output = new List<AttributeDefinitionOutputDTO>();

			output.Capacity = data.Count;

			foreach (AttributeDefinition attributeDefinition in data)
			{
				output.Add(attributeDefinition.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a AttributeDefinition Object.
		///
		/// </summary>
		public static Database.AttributeDefinition FromDTO(AttributeDefinitionDTO dto)
		{
			return new Database.AttributeDefinition
			{
				id = dto.id,
				entityName = dto.entityName,
				key = dto.key,
				label = dto.label,
				type = dto.type,
				options = dto.options,
				isRequired = dto.isRequired,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a AttributeDefinition Object.
		///
		/// </summary>
		public void ApplyDTO(AttributeDefinitionDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.entityName = dto.entityName;
			this.key = dto.key;
			this.label = dto.label;
			this.type = dto.type;
			this.options = dto.options;
			this.isRequired = dto.isRequired;
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
		/// Creates a deep copy clone of a AttributeDefinition Object.
		///
		/// </summary>
		public AttributeDefinition Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new AttributeDefinition{
				id = this.id,
				tenantGuid = this.tenantGuid,
				entityName = this.entityName,
				key = this.key,
				label = this.label,
				type = this.type,
				options = this.options,
				isRequired = this.isRequired,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AttributeDefinition Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AttributeDefinition Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a AttributeDefinition Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a AttributeDefinition Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.AttributeDefinition attributeDefinition)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (attributeDefinition == null)
			{
				return null;
			}

			return new {
				id = attributeDefinition.id,
				entityName = attributeDefinition.entityName,
				key = attributeDefinition.key,
				label = attributeDefinition.label,
				type = attributeDefinition.type,
				options = attributeDefinition.options,
				isRequired = attributeDefinition.isRequired,
				sequence = attributeDefinition.sequence,
				objectGuid = attributeDefinition.objectGuid,
				active = attributeDefinition.active,
				deleted = attributeDefinition.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a AttributeDefinition Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(AttributeDefinition attributeDefinition)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (attributeDefinition == null)
			{
				return null;
			}

			return new {
				id = attributeDefinition.id,
				entityName = attributeDefinition.entityName,
				key = attributeDefinition.key,
				label = attributeDefinition.label,
				type = attributeDefinition.type,
				options = attributeDefinition.options,
				isRequired = attributeDefinition.isRequired,
				sequence = attributeDefinition.sequence,
				objectGuid = attributeDefinition.objectGuid,
				active = attributeDefinition.active,
				deleted = attributeDefinition.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a AttributeDefinition Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(AttributeDefinition attributeDefinition)
		{
			//
			// Return a very minimal object.
			//
			if (attributeDefinition == null)
			{
				return null;
			}

			return new {
				id = attributeDefinition.id,
				name = attributeDefinition.entityName,
				description = string.Join(", ", new[] { attributeDefinition.entityName, attributeDefinition.key, attributeDefinition.label}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
