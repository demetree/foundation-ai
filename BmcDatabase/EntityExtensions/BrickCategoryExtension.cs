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
	public partial class BrickCategory : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BrickCategoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
			public Int32? rebrickablePartCategoryId { get; set; }
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
		public class BrickCategoryOutputDTO : BrickCategoryDTO
		{
		}


		/// <summary>
		///
		/// Converts a BrickCategory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BrickCategoryDTO ToDTO()
		{
			return new BrickCategoryDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				rebrickablePartCategoryId = this.rebrickablePartCategoryId,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BrickCategory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BrickCategoryDTO> ToDTOList(List<BrickCategory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickCategoryDTO> output = new List<BrickCategoryDTO>();

			output.Capacity = data.Count;

			foreach (BrickCategory brickCategory in data)
			{
				output.Add(brickCategory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BrickCategory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BrickCategoryEntity type directly.
		///
		/// </summary>
		public BrickCategoryOutputDTO ToOutputDTO()
		{
			return new BrickCategoryOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				rebrickablePartCategoryId = this.rebrickablePartCategoryId,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BrickCategory list to list of Output Data Transfer Object intended to be used for serializing a list of BrickCategory objects to avoid using the BrickCategory entity type directly.
		///
		/// </summary>
		public static List<BrickCategoryOutputDTO> ToOutputDTOList(List<BrickCategory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickCategoryOutputDTO> output = new List<BrickCategoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (BrickCategory brickCategory in data)
			{
				output.Add(brickCategory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BrickCategory Object.
		///
		/// </summary>
		public static Database.BrickCategory FromDTO(BrickCategoryDTO dto)
		{
			return new Database.BrickCategory
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				rebrickablePartCategoryId = dto.rebrickablePartCategoryId,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BrickCategory Object.
		///
		/// </summary>
		public void ApplyDTO(BrickCategoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.rebrickablePartCategoryId = dto.rebrickablePartCategoryId;
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
		/// Creates a deep copy clone of a BrickCategory Object.
		///
		/// </summary>
		public BrickCategory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BrickCategory{
				id = this.id,
				name = this.name,
				description = this.description,
				rebrickablePartCategoryId = this.rebrickablePartCategoryId,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickCategory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickCategory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BrickCategory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BrickCategory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BrickCategory brickCategory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (brickCategory == null)
			{
				return null;
			}

			return new {
				id = brickCategory.id,
				name = brickCategory.name,
				description = brickCategory.description,
				rebrickablePartCategoryId = brickCategory.rebrickablePartCategoryId,
				sequence = brickCategory.sequence,
				objectGuid = brickCategory.objectGuid,
				active = brickCategory.active,
				deleted = brickCategory.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BrickCategory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BrickCategory brickCategory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (brickCategory == null)
			{
				return null;
			}

			return new {
				id = brickCategory.id,
				name = brickCategory.name,
				description = brickCategory.description,
				rebrickablePartCategoryId = brickCategory.rebrickablePartCategoryId,
				sequence = brickCategory.sequence,
				objectGuid = brickCategory.objectGuid,
				active = brickCategory.active,
				deleted = brickCategory.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BrickCategory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BrickCategory brickCategory)
		{
			//
			// Return a very minimal object.
			//
			if (brickCategory == null)
			{
				return null;
			}

			return new {
				id = brickCategory.id,
				name = brickCategory.name,
				description = brickCategory.description,
			 };
		}
	}
}
