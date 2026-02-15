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
	public partial class BrickPartRelationship : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BrickPartRelationshipDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 childBrickPartId { get; set; }
			[Required]
			public Int32 parentBrickPartId { get; set; }
			[Required]
			public String relationshipType { get; set; }
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
		public class BrickPartRelationshipOutputDTO : BrickPartRelationshipDTO
		{
			public BrickPart.BrickPartDTO childBrickPart { get; set; }
			public BrickPart.BrickPartDTO parentBrickPart { get; set; }
		}


		/// <summary>
		///
		/// Converts a BrickPartRelationship to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BrickPartRelationshipDTO ToDTO()
		{
			return new BrickPartRelationshipDTO
			{
				id = this.id,
				childBrickPartId = this.childBrickPartId,
				parentBrickPartId = this.parentBrickPartId,
				relationshipType = this.relationshipType,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BrickPartRelationship list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BrickPartRelationshipDTO> ToDTOList(List<BrickPartRelationship> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickPartRelationshipDTO> output = new List<BrickPartRelationshipDTO>();

			output.Capacity = data.Count;

			foreach (BrickPartRelationship brickPartRelationship in data)
			{
				output.Add(brickPartRelationship.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BrickPartRelationship to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BrickPartRelationshipEntity type directly.
		///
		/// </summary>
		public BrickPartRelationshipOutputDTO ToOutputDTO()
		{
			return new BrickPartRelationshipOutputDTO
			{
				id = this.id,
				childBrickPartId = this.childBrickPartId,
				parentBrickPartId = this.parentBrickPartId,
				relationshipType = this.relationshipType,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				childBrickPart = this.childBrickPart?.ToDTO(),
				parentBrickPart = this.parentBrickPart?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a BrickPartRelationship list to list of Output Data Transfer Object intended to be used for serializing a list of BrickPartRelationship objects to avoid using the BrickPartRelationship entity type directly.
		///
		/// </summary>
		public static List<BrickPartRelationshipOutputDTO> ToOutputDTOList(List<BrickPartRelationship> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickPartRelationshipOutputDTO> output = new List<BrickPartRelationshipOutputDTO>();

			output.Capacity = data.Count;

			foreach (BrickPartRelationship brickPartRelationship in data)
			{
				output.Add(brickPartRelationship.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BrickPartRelationship Object.
		///
		/// </summary>
		public static Database.BrickPartRelationship FromDTO(BrickPartRelationshipDTO dto)
		{
			return new Database.BrickPartRelationship
			{
				id = dto.id,
				childBrickPartId = dto.childBrickPartId,
				parentBrickPartId = dto.parentBrickPartId,
				relationshipType = dto.relationshipType,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BrickPartRelationship Object.
		///
		/// </summary>
		public void ApplyDTO(BrickPartRelationshipDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.childBrickPartId = dto.childBrickPartId;
			this.parentBrickPartId = dto.parentBrickPartId;
			this.relationshipType = dto.relationshipType;
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
		/// Creates a deep copy clone of a BrickPartRelationship Object.
		///
		/// </summary>
		public BrickPartRelationship Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BrickPartRelationship{
				id = this.id,
				childBrickPartId = this.childBrickPartId,
				parentBrickPartId = this.parentBrickPartId,
				relationshipType = this.relationshipType,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickPartRelationship Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickPartRelationship Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BrickPartRelationship Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BrickPartRelationship Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BrickPartRelationship brickPartRelationship)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (brickPartRelationship == null)
			{
				return null;
			}

			return new {
				id = brickPartRelationship.id,
				childBrickPartId = brickPartRelationship.childBrickPartId,
				parentBrickPartId = brickPartRelationship.parentBrickPartId,
				relationshipType = brickPartRelationship.relationshipType,
				objectGuid = brickPartRelationship.objectGuid,
				active = brickPartRelationship.active,
				deleted = brickPartRelationship.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BrickPartRelationship Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BrickPartRelationship brickPartRelationship)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (brickPartRelationship == null)
			{
				return null;
			}

			return new {
				id = brickPartRelationship.id,
				childBrickPartId = brickPartRelationship.childBrickPartId,
				parentBrickPartId = brickPartRelationship.parentBrickPartId,
				relationshipType = brickPartRelationship.relationshipType,
				objectGuid = brickPartRelationship.objectGuid,
				active = brickPartRelationship.active,
				deleted = brickPartRelationship.deleted,
				childBrickPart = BrickPart.CreateMinimalAnonymous(brickPartRelationship.childBrickPart),
				parentBrickPart = BrickPart.CreateMinimalAnonymous(brickPartRelationship.parentBrickPart)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BrickPartRelationship Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BrickPartRelationship brickPartRelationship)
		{
			//
			// Return a very minimal object.
			//
			if (brickPartRelationship == null)
			{
				return null;
			}

			return new {
				id = brickPartRelationship.id,
				name = brickPartRelationship.relationshipType,
				description = string.Join(", ", new[] { brickPartRelationship.relationshipType}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
