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
	public partial class BrickColour : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BrickColourDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public Int32 ldrawColourCode { get; set; }
			public String hexRgb { get; set; }
			public String hexEdgeColour { get; set; }
			public Int32? alpha { get; set; }
			[Required]
			public Boolean isTransparent { get; set; }
			[Required]
			public Boolean isMetallic { get; set; }
			[Required]
			public Int32 colourFinishId { get; set; }
			public Int32? luminance { get; set; }
			public Int32? legoColourId { get; set; }
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
		public class BrickColourOutputDTO : BrickColourDTO
		{
			public ColourFinish.ColourFinishDTO colourFinish { get; set; }
		}


		/// <summary>
		///
		/// Converts a BrickColour to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BrickColourDTO ToDTO()
		{
			return new BrickColourDTO
			{
				id = this.id,
				name = this.name,
				ldrawColourCode = this.ldrawColourCode,
				hexRgb = this.hexRgb,
				hexEdgeColour = this.hexEdgeColour,
				alpha = this.alpha,
				isTransparent = this.isTransparent,
				isMetallic = this.isMetallic,
				colourFinishId = this.colourFinishId,
				luminance = this.luminance,
				legoColourId = this.legoColourId,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BrickColour list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BrickColourDTO> ToDTOList(List<BrickColour> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickColourDTO> output = new List<BrickColourDTO>();

			output.Capacity = data.Count;

			foreach (BrickColour brickColour in data)
			{
				output.Add(brickColour.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BrickColour to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BrickColourEntity type directly.
		///
		/// </summary>
		public BrickColourOutputDTO ToOutputDTO()
		{
			return new BrickColourOutputDTO
			{
				id = this.id,
				name = this.name,
				ldrawColourCode = this.ldrawColourCode,
				hexRgb = this.hexRgb,
				hexEdgeColour = this.hexEdgeColour,
				alpha = this.alpha,
				isTransparent = this.isTransparent,
				isMetallic = this.isMetallic,
				colourFinishId = this.colourFinishId,
				luminance = this.luminance,
				legoColourId = this.legoColourId,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				colourFinish = this.colourFinish?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a BrickColour list to list of Output Data Transfer Object intended to be used for serializing a list of BrickColour objects to avoid using the BrickColour entity type directly.
		///
		/// </summary>
		public static List<BrickColourOutputDTO> ToOutputDTOList(List<BrickColour> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickColourOutputDTO> output = new List<BrickColourOutputDTO>();

			output.Capacity = data.Count;

			foreach (BrickColour brickColour in data)
			{
				output.Add(brickColour.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BrickColour Object.
		///
		/// </summary>
		public static Database.BrickColour FromDTO(BrickColourDTO dto)
		{
			return new Database.BrickColour
			{
				id = dto.id,
				name = dto.name,
				ldrawColourCode = dto.ldrawColourCode,
				hexRgb = dto.hexRgb,
				hexEdgeColour = dto.hexEdgeColour,
				alpha = dto.alpha,
				isTransparent = dto.isTransparent,
				isMetallic = dto.isMetallic,
				colourFinishId = dto.colourFinishId,
				luminance = dto.luminance,
				legoColourId = dto.legoColourId,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BrickColour Object.
		///
		/// </summary>
		public void ApplyDTO(BrickColourDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.ldrawColourCode = dto.ldrawColourCode;
			this.hexRgb = dto.hexRgb;
			this.hexEdgeColour = dto.hexEdgeColour;
			this.alpha = dto.alpha;
			this.isTransparent = dto.isTransparent;
			this.isMetallic = dto.isMetallic;
			this.colourFinishId = dto.colourFinishId;
			this.luminance = dto.luminance;
			this.legoColourId = dto.legoColourId;
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
		/// Creates a deep copy clone of a BrickColour Object.
		///
		/// </summary>
		public BrickColour Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BrickColour{
				id = this.id,
				name = this.name,
				ldrawColourCode = this.ldrawColourCode,
				hexRgb = this.hexRgb,
				hexEdgeColour = this.hexEdgeColour,
				alpha = this.alpha,
				isTransparent = this.isTransparent,
				isMetallic = this.isMetallic,
				colourFinishId = this.colourFinishId,
				luminance = this.luminance,
				legoColourId = this.legoColourId,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickColour Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickColour Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BrickColour Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BrickColour Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BrickColour brickColour)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (brickColour == null)
			{
				return null;
			}

			return new {
				id = brickColour.id,
				name = brickColour.name,
				ldrawColourCode = brickColour.ldrawColourCode,
				hexRgb = brickColour.hexRgb,
				hexEdgeColour = brickColour.hexEdgeColour,
				alpha = brickColour.alpha,
				isTransparent = brickColour.isTransparent,
				isMetallic = brickColour.isMetallic,
				colourFinishId = brickColour.colourFinishId,
				luminance = brickColour.luminance,
				legoColourId = brickColour.legoColourId,
				sequence = brickColour.sequence,
				objectGuid = brickColour.objectGuid,
				active = brickColour.active,
				deleted = brickColour.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BrickColour Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BrickColour brickColour)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (brickColour == null)
			{
				return null;
			}

			return new {
				id = brickColour.id,
				name = brickColour.name,
				ldrawColourCode = brickColour.ldrawColourCode,
				hexRgb = brickColour.hexRgb,
				hexEdgeColour = brickColour.hexEdgeColour,
				alpha = brickColour.alpha,
				isTransparent = brickColour.isTransparent,
				isMetallic = brickColour.isMetallic,
				colourFinishId = brickColour.colourFinishId,
				luminance = brickColour.luminance,
				legoColourId = brickColour.legoColourId,
				sequence = brickColour.sequence,
				objectGuid = brickColour.objectGuid,
				active = brickColour.active,
				deleted = brickColour.deleted,
				colourFinish = ColourFinish.CreateMinimalAnonymous(brickColour.colourFinish)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BrickColour Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BrickColour brickColour)
		{
			//
			// Return a very minimal object.
			//
			if (brickColour == null)
			{
				return null;
			}

			return new {
				id = brickColour.id,
				name = brickColour.name,
				description = string.Join(", ", new[] { brickColour.name, brickColour.hexRgb, brickColour.hexEdgeColour}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
