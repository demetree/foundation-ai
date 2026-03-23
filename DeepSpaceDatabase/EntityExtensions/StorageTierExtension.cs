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
	public partial class StorageTier : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class StorageTierDTO
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
		public class StorageTierOutputDTO : StorageTierDTO
		{
		}


		/// <summary>
		///
		/// Converts a StorageTier to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public StorageTierDTO ToDTO()
		{
			return new StorageTierDTO
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
		/// Converts a StorageTier list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<StorageTierDTO> ToDTOList(List<StorageTier> data)
		{
			if (data == null)
			{
				return null;
			}

			List<StorageTierDTO> output = new List<StorageTierDTO>();

			output.Capacity = data.Count;

			foreach (StorageTier storageTier in data)
			{
				output.Add(storageTier.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a StorageTier to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the StorageTier Entity type directly.
		///
		/// </summary>
		public StorageTierOutputDTO ToOutputDTO()
		{
			return new StorageTierOutputDTO
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
		/// Converts a StorageTier list to list of Output Data Transfer Object intended to be used for serializing a list of StorageTier objects to avoid using the StorageTier entity type directly.
		///
		/// </summary>
		public static List<StorageTierOutputDTO> ToOutputDTOList(List<StorageTier> data)
		{
			if (data == null)
			{
				return null;
			}

			List<StorageTierOutputDTO> output = new List<StorageTierOutputDTO>();

			output.Capacity = data.Count;

			foreach (StorageTier storageTier in data)
			{
				output.Add(storageTier.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a StorageTier Object.
		///
		/// </summary>
		public static Database.StorageTier FromDTO(StorageTierDTO dto)
		{
			return new Database.StorageTier
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
		/// Applies the values from an INPUT DTO to a StorageTier Object.
		///
		/// </summary>
		public void ApplyDTO(StorageTierDTO dto)
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
		/// Creates a deep copy clone of a StorageTier Object.
		///
		/// </summary>
		public StorageTier Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new StorageTier{
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
        /// Creates an anonymous object containing properties from a StorageTier Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a StorageTier Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a StorageTier Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a StorageTier Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.StorageTier storageTier)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (storageTier == null)
			{
				return null;
			}

			return new {
				id = storageTier.id,
				name = storageTier.name,
				description = storageTier.description,
				sequence = storageTier.sequence,
				objectGuid = storageTier.objectGuid,
				active = storageTier.active,
				deleted = storageTier.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a StorageTier Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(StorageTier storageTier)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (storageTier == null)
			{
				return null;
			}

			return new {
				id = storageTier.id,
				name = storageTier.name,
				description = storageTier.description,
				sequence = storageTier.sequence,
				objectGuid = storageTier.objectGuid,
				active = storageTier.active,
				deleted = storageTier.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a StorageTier Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(StorageTier storageTier)
		{
			//
			// Return a very minimal object.
			//
			if (storageTier == null)
			{
				return null;
			}

			return new {
				id = storageTier.id,
				name = storageTier.name,
				description = storageTier.description,
			 };
		}
	}
}
