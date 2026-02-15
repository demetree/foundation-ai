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
	public partial class BrickElement : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BrickElementDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String elementId { get; set; }
			[Required]
			public Int32 brickPartId { get; set; }
			[Required]
			public Int32 brickColourId { get; set; }
			public String designId { get; set; }
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
		public class BrickElementOutputDTO : BrickElementDTO
		{
			public BrickColour.BrickColourDTO brickColour { get; set; }
			public BrickPart.BrickPartDTO brickPart { get; set; }
		}


		/// <summary>
		///
		/// Converts a BrickElement to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BrickElementDTO ToDTO()
		{
			return new BrickElementDTO
			{
				id = this.id,
				elementId = this.elementId,
				brickPartId = this.brickPartId,
				brickColourId = this.brickColourId,
				designId = this.designId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BrickElement list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BrickElementDTO> ToDTOList(List<BrickElement> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickElementDTO> output = new List<BrickElementDTO>();

			output.Capacity = data.Count;

			foreach (BrickElement brickElement in data)
			{
				output.Add(brickElement.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BrickElement to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BrickElementEntity type directly.
		///
		/// </summary>
		public BrickElementOutputDTO ToOutputDTO()
		{
			return new BrickElementOutputDTO
			{
				id = this.id,
				elementId = this.elementId,
				brickPartId = this.brickPartId,
				brickColourId = this.brickColourId,
				designId = this.designId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				brickColour = this.brickColour?.ToDTO(),
				brickPart = this.brickPart?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a BrickElement list to list of Output Data Transfer Object intended to be used for serializing a list of BrickElement objects to avoid using the BrickElement entity type directly.
		///
		/// </summary>
		public static List<BrickElementOutputDTO> ToOutputDTOList(List<BrickElement> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickElementOutputDTO> output = new List<BrickElementOutputDTO>();

			output.Capacity = data.Count;

			foreach (BrickElement brickElement in data)
			{
				output.Add(brickElement.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BrickElement Object.
		///
		/// </summary>
		public static Database.BrickElement FromDTO(BrickElementDTO dto)
		{
			return new Database.BrickElement
			{
				id = dto.id,
				elementId = dto.elementId,
				brickPartId = dto.brickPartId,
				brickColourId = dto.brickColourId,
				designId = dto.designId,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BrickElement Object.
		///
		/// </summary>
		public void ApplyDTO(BrickElementDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.elementId = dto.elementId;
			this.brickPartId = dto.brickPartId;
			this.brickColourId = dto.brickColourId;
			this.designId = dto.designId;
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
		/// Creates a deep copy clone of a BrickElement Object.
		///
		/// </summary>
		public BrickElement Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BrickElement{
				id = this.id,
				elementId = this.elementId,
				brickPartId = this.brickPartId,
				brickColourId = this.brickColourId,
				designId = this.designId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickElement Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickElement Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BrickElement Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BrickElement Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BrickElement brickElement)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (brickElement == null)
			{
				return null;
			}

			return new {
				id = brickElement.id,
				elementId = brickElement.elementId,
				brickPartId = brickElement.brickPartId,
				brickColourId = brickElement.brickColourId,
				designId = brickElement.designId,
				objectGuid = brickElement.objectGuid,
				active = brickElement.active,
				deleted = brickElement.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BrickElement Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BrickElement brickElement)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (brickElement == null)
			{
				return null;
			}

			return new {
				id = brickElement.id,
				elementId = brickElement.elementId,
				brickPartId = brickElement.brickPartId,
				brickColourId = brickElement.brickColourId,
				designId = brickElement.designId,
				objectGuid = brickElement.objectGuid,
				active = brickElement.active,
				deleted = brickElement.deleted,
				brickColour = BrickColour.CreateMinimalAnonymous(brickElement.brickColour),
				brickPart = BrickPart.CreateMinimalAnonymous(brickElement.brickPart)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BrickElement Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BrickElement brickElement)
		{
			//
			// Return a very minimal object.
			//
			if (brickElement == null)
			{
				return null;
			}

			return new {
				id = brickElement.id,
				name = brickElement.elementId,
				description = string.Join(", ", new[] { brickElement.elementId, brickElement.designId}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
