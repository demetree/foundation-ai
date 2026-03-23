using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.DeepSpace.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class StorageProviderType : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class StorageProviderTypeDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
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
		public class StorageProviderTypeOutputDTO : StorageProviderTypeDTO
		{
		}


		/// <summary>
		///
		/// Converts a StorageProviderType to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public StorageProviderTypeDTO ToDTO()
		{
			return new StorageProviderTypeDTO
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
		/// Converts a StorageProviderType list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<StorageProviderTypeDTO> ToDTOList(List<StorageProviderType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<StorageProviderTypeDTO> output = new List<StorageProviderTypeDTO>();

			output.Capacity = data.Count;

			foreach (StorageProviderType storageProviderType in data)
			{
				output.Add(storageProviderType.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a StorageProviderType to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the StorageProviderType Entity type directly.
		///
		/// </summary>
		public StorageProviderTypeOutputDTO ToOutputDTO()
		{
			return new StorageProviderTypeOutputDTO
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
		/// Converts a StorageProviderType list to list of Output Data Transfer Object intended to be used for serializing a list of StorageProviderType objects to avoid using the StorageProviderType entity type directly.
		///
		/// </summary>
		public static List<StorageProviderTypeOutputDTO> ToOutputDTOList(List<StorageProviderType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<StorageProviderTypeOutputDTO> output = new List<StorageProviderTypeOutputDTO>();

			output.Capacity = data.Count;

			foreach (StorageProviderType storageProviderType in data)
			{
				output.Add(storageProviderType.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a StorageProviderType Object.
		///
		/// </summary>
		public static Database.StorageProviderType FromDTO(StorageProviderTypeDTO dto)
		{
			return new Database.StorageProviderType
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
		/// Applies the values from an INPUT DTO to a StorageProviderType Object.
		///
		/// </summary>
		public void ApplyDTO(StorageProviderTypeDTO dto)
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
		/// Creates a deep copy clone of a StorageProviderType Object.
		///
		/// </summary>
		public StorageProviderType Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new StorageProviderType{
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
        /// Creates an anonymous object containing properties from a StorageProviderType Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a StorageProviderType Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a StorageProviderType Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a StorageProviderType Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.StorageProviderType storageProviderType)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (storageProviderType == null)
			{
				return null;
			}

			return new {
				id = storageProviderType.id,
				name = storageProviderType.name,
				description = storageProviderType.description,
				sequence = storageProviderType.sequence,
				objectGuid = storageProviderType.objectGuid,
				active = storageProviderType.active,
				deleted = storageProviderType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a StorageProviderType Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(StorageProviderType storageProviderType)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (storageProviderType == null)
			{
				return null;
			}

			return new {
				id = storageProviderType.id,
				name = storageProviderType.name,
				description = storageProviderType.description,
				sequence = storageProviderType.sequence,
				objectGuid = storageProviderType.objectGuid,
				active = storageProviderType.active,
				deleted = storageProviderType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a StorageProviderType Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(StorageProviderType storageProviderType)
		{
			//
			// Return a very minimal object.
			//
			if (storageProviderType == null)
			{
				return null;
			}

			return new {
				id = storageProviderType.id,
				name = storageProviderType.name,
				description = storageProviderType.description,
			 };
		}
	}
}
