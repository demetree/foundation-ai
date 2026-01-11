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
	public partial class ResourceType : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ResourceTypeDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
			public Boolean? isBillable { get; set; }
			public Int32? sequence { get; set; }
			public Int32? iconId { get; set; }
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
		public class ResourceTypeOutputDTO : ResourceTypeDTO
		{
			public Icon.IconDTO icon { get; set; }
		}


		/// <summary>
		///
		/// Converts a ResourceType to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ResourceTypeDTO ToDTO()
		{
			return new ResourceTypeDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				isBillable = this.isBillable,
				sequence = this.sequence,
				iconId = this.iconId,
				color = this.color,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ResourceType list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ResourceTypeDTO> ToDTOList(List<ResourceType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ResourceTypeDTO> output = new List<ResourceTypeDTO>();

			output.Capacity = data.Count;

			foreach (ResourceType resourceType in data)
			{
				output.Add(resourceType.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ResourceType to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ResourceTypeEntity type directly.
		///
		/// </summary>
		public ResourceTypeOutputDTO ToOutputDTO()
		{
			return new ResourceTypeOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				isBillable = this.isBillable,
				sequence = this.sequence,
				iconId = this.iconId,
				color = this.color,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				icon = this.icon?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ResourceType list to list of Output Data Transfer Object intended to be used for serializing a list of ResourceType objects to avoid using the ResourceType entity type directly.
		///
		/// </summary>
		public static List<ResourceTypeOutputDTO> ToOutputDTOList(List<ResourceType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ResourceTypeOutputDTO> output = new List<ResourceTypeOutputDTO>();

			output.Capacity = data.Count;

			foreach (ResourceType resourceType in data)
			{
				output.Add(resourceType.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ResourceType Object.
		///
		/// </summary>
		public static Database.ResourceType FromDTO(ResourceTypeDTO dto)
		{
			return new Database.ResourceType
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				isBillable = dto.isBillable,
				sequence = dto.sequence,
				iconId = dto.iconId,
				color = dto.color,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ResourceType Object.
		///
		/// </summary>
		public void ApplyDTO(ResourceTypeDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.isBillable = dto.isBillable;
			this.sequence = dto.sequence;
			this.iconId = dto.iconId;
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
		/// Creates a deep copy clone of a ResourceType Object.
		///
		/// </summary>
		public ResourceType Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ResourceType{
				id = this.id,
				tenantGuid = this.tenantGuid,
				name = this.name,
				description = this.description,
				isBillable = this.isBillable,
				sequence = this.sequence,
				iconId = this.iconId,
				color = this.color,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ResourceType Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ResourceType Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ResourceType Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ResourceType Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ResourceType resourceType)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (resourceType == null)
			{
				return null;
			}

			return new {
				id = resourceType.id,
				name = resourceType.name,
				description = resourceType.description,
				isBillable = resourceType.isBillable,
				sequence = resourceType.sequence,
				iconId = resourceType.iconId,
				color = resourceType.color,
				objectGuid = resourceType.objectGuid,
				active = resourceType.active,
				deleted = resourceType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ResourceType Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ResourceType resourceType)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (resourceType == null)
			{
				return null;
			}

			return new {
				id = resourceType.id,
				name = resourceType.name,
				description = resourceType.description,
				isBillable = resourceType.isBillable,
				sequence = resourceType.sequence,
				iconId = resourceType.iconId,
				color = resourceType.color,
				objectGuid = resourceType.objectGuid,
				active = resourceType.active,
				deleted = resourceType.deleted,
				icon = Icon.CreateMinimalAnonymous(resourceType.icon)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ResourceType Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ResourceType resourceType)
		{
			//
			// Return a very minimal object.
			//
			if (resourceType == null)
			{
				return null;
			}

			return new {
				id = resourceType.id,
				name = resourceType.name,
				description = resourceType.description,
			 };
		}
	}
}
