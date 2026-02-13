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
	public partial class BrickPartColour : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BrickPartColourDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 brickPartId { get; set; }
			[Required]
			public Int32 brickColourId { get; set; }
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
		public class BrickPartColourOutputDTO : BrickPartColourDTO
		{
			public BrickColour.BrickColourDTO brickColour { get; set; }
			public BrickPart.BrickPartDTO brickPart { get; set; }
		}


		/// <summary>
		///
		/// Converts a BrickPartColour to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BrickPartColourDTO ToDTO()
		{
			return new BrickPartColourDTO
			{
				id = this.id,
				brickPartId = this.brickPartId,
				brickColourId = this.brickColourId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BrickPartColour list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BrickPartColourDTO> ToDTOList(List<BrickPartColour> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickPartColourDTO> output = new List<BrickPartColourDTO>();

			output.Capacity = data.Count;

			foreach (BrickPartColour brickPartColour in data)
			{
				output.Add(brickPartColour.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BrickPartColour to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BrickPartColourEntity type directly.
		///
		/// </summary>
		public BrickPartColourOutputDTO ToOutputDTO()
		{
			return new BrickPartColourOutputDTO
			{
				id = this.id,
				brickPartId = this.brickPartId,
				brickColourId = this.brickColourId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				brickColour = this.brickColour?.ToDTO(),
				brickPart = this.brickPart?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a BrickPartColour list to list of Output Data Transfer Object intended to be used for serializing a list of BrickPartColour objects to avoid using the BrickPartColour entity type directly.
		///
		/// </summary>
		public static List<BrickPartColourOutputDTO> ToOutputDTOList(List<BrickPartColour> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickPartColourOutputDTO> output = new List<BrickPartColourOutputDTO>();

			output.Capacity = data.Count;

			foreach (BrickPartColour brickPartColour in data)
			{
				output.Add(brickPartColour.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BrickPartColour Object.
		///
		/// </summary>
		public static Database.BrickPartColour FromDTO(BrickPartColourDTO dto)
		{
			return new Database.BrickPartColour
			{
				id = dto.id,
				brickPartId = dto.brickPartId,
				brickColourId = dto.brickColourId,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BrickPartColour Object.
		///
		/// </summary>
		public void ApplyDTO(BrickPartColourDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.brickPartId = dto.brickPartId;
			this.brickColourId = dto.brickColourId;
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
		/// Creates a deep copy clone of a BrickPartColour Object.
		///
		/// </summary>
		public BrickPartColour Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BrickPartColour{
				id = this.id,
				brickPartId = this.brickPartId,
				brickColourId = this.brickColourId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickPartColour Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickPartColour Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BrickPartColour Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BrickPartColour Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BrickPartColour brickPartColour)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (brickPartColour == null)
			{
				return null;
			}

			return new {
				id = brickPartColour.id,
				brickPartId = brickPartColour.brickPartId,
				brickColourId = brickPartColour.brickColourId,
				objectGuid = brickPartColour.objectGuid,
				active = brickPartColour.active,
				deleted = brickPartColour.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BrickPartColour Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BrickPartColour brickPartColour)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (brickPartColour == null)
			{
				return null;
			}

			return new {
				id = brickPartColour.id,
				brickPartId = brickPartColour.brickPartId,
				brickColourId = brickPartColour.brickColourId,
				objectGuid = brickPartColour.objectGuid,
				active = brickPartColour.active,
				deleted = brickPartColour.deleted,
				brickColour = BrickColour.CreateMinimalAnonymous(brickPartColour.brickColour),
				brickPart = BrickPart.CreateMinimalAnonymous(brickPartColour.brickPart)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BrickPartColour Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BrickPartColour brickPartColour)
		{
			//
			// Return a very minimal object.
			//
			if (brickPartColour == null)
			{
				return null;
			}

			return new {
				id = brickPartColour.id,
				name = brickPartColour.id,
				description = brickPartColour.id
			 };
		}
	}
}
