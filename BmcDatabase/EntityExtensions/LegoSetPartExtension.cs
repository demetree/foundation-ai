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
	public partial class LegoSetPart : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class LegoSetPartDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 legoSetId { get; set; }
			[Required]
			public Int32 brickPartId { get; set; }
			[Required]
			public Int32 brickColourId { get; set; }
			public Int32? quantity { get; set; }
			[Required]
			public Boolean isSpare { get; set; }
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
		public class LegoSetPartOutputDTO : LegoSetPartDTO
		{
			public BrickColour.BrickColourDTO brickColour { get; set; }
			public BrickPart.BrickPartDTO brickPart { get; set; }
			public LegoSet.LegoSetDTO legoSet { get; set; }
		}


		/// <summary>
		///
		/// Converts a LegoSetPart to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public LegoSetPartDTO ToDTO()
		{
			return new LegoSetPartDTO
			{
				id = this.id,
				legoSetId = this.legoSetId,
				brickPartId = this.brickPartId,
				brickColourId = this.brickColourId,
				quantity = this.quantity,
				isSpare = this.isSpare,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a LegoSetPart list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<LegoSetPartDTO> ToDTOList(List<LegoSetPart> data)
		{
			if (data == null)
			{
				return null;
			}

			List<LegoSetPartDTO> output = new List<LegoSetPartDTO>();

			output.Capacity = data.Count;

			foreach (LegoSetPart legoSetPart in data)
			{
				output.Add(legoSetPart.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a LegoSetPart to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the LegoSetPartEntity type directly.
		///
		/// </summary>
		public LegoSetPartOutputDTO ToOutputDTO()
		{
			return new LegoSetPartOutputDTO
			{
				id = this.id,
				legoSetId = this.legoSetId,
				brickPartId = this.brickPartId,
				brickColourId = this.brickColourId,
				quantity = this.quantity,
				isSpare = this.isSpare,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				brickColour = this.brickColour?.ToDTO(),
				brickPart = this.brickPart?.ToDTO(),
				legoSet = this.legoSet?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a LegoSetPart list to list of Output Data Transfer Object intended to be used for serializing a list of LegoSetPart objects to avoid using the LegoSetPart entity type directly.
		///
		/// </summary>
		public static List<LegoSetPartOutputDTO> ToOutputDTOList(List<LegoSetPart> data)
		{
			if (data == null)
			{
				return null;
			}

			List<LegoSetPartOutputDTO> output = new List<LegoSetPartOutputDTO>();

			output.Capacity = data.Count;

			foreach (LegoSetPart legoSetPart in data)
			{
				output.Add(legoSetPart.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a LegoSetPart Object.
		///
		/// </summary>
		public static Database.LegoSetPart FromDTO(LegoSetPartDTO dto)
		{
			return new Database.LegoSetPart
			{
				id = dto.id,
				legoSetId = dto.legoSetId,
				brickPartId = dto.brickPartId,
				brickColourId = dto.brickColourId,
				quantity = dto.quantity,
				isSpare = dto.isSpare,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a LegoSetPart Object.
		///
		/// </summary>
		public void ApplyDTO(LegoSetPartDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.legoSetId = dto.legoSetId;
			this.brickPartId = dto.brickPartId;
			this.brickColourId = dto.brickColourId;
			this.quantity = dto.quantity;
			this.isSpare = dto.isSpare;
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
		/// Creates a deep copy clone of a LegoSetPart Object.
		///
		/// </summary>
		public LegoSetPart Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new LegoSetPart{
				id = this.id,
				legoSetId = this.legoSetId,
				brickPartId = this.brickPartId,
				brickColourId = this.brickColourId,
				quantity = this.quantity,
				isSpare = this.isSpare,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a LegoSetPart Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a LegoSetPart Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a LegoSetPart Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a LegoSetPart Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.LegoSetPart legoSetPart)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (legoSetPart == null)
			{
				return null;
			}

			return new {
				id = legoSetPart.id,
				legoSetId = legoSetPart.legoSetId,
				brickPartId = legoSetPart.brickPartId,
				brickColourId = legoSetPart.brickColourId,
				quantity = legoSetPart.quantity,
				isSpare = legoSetPart.isSpare,
				objectGuid = legoSetPart.objectGuid,
				active = legoSetPart.active,
				deleted = legoSetPart.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a LegoSetPart Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(LegoSetPart legoSetPart)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (legoSetPart == null)
			{
				return null;
			}

			return new {
				id = legoSetPart.id,
				legoSetId = legoSetPart.legoSetId,
				brickPartId = legoSetPart.brickPartId,
				brickColourId = legoSetPart.brickColourId,
				quantity = legoSetPart.quantity,
				isSpare = legoSetPart.isSpare,
				objectGuid = legoSetPart.objectGuid,
				active = legoSetPart.active,
				deleted = legoSetPart.deleted,
				brickColour = BrickColour.CreateMinimalAnonymous(legoSetPart.brickColour),
				brickPart = BrickPart.CreateMinimalAnonymous(legoSetPart.brickPart),
				legoSet = LegoSet.CreateMinimalAnonymous(legoSetPart.legoSet)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a LegoSetPart Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(LegoSetPart legoSetPart)
		{
			//
			// Return a very minimal object.
			//
			if (legoSetPart == null)
			{
				return null;
			}

			return new {
				id = legoSetPart.id,
				name = legoSetPart.id,
				description = legoSetPart.id
			 };
		}
	}
}
