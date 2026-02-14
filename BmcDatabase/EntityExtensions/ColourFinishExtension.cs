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
	public partial class ColourFinish : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ColourFinishDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
			[Required]
			public Boolean requiresEnvironmentMap { get; set; }
			[Required]
			public Boolean isMatte { get; set; }
			public Int32? defaultAlpha { get; set; }
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
		public class ColourFinishOutputDTO : ColourFinishDTO
		{
		}


		/// <summary>
		///
		/// Converts a ColourFinish to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ColourFinishDTO ToDTO()
		{
			return new ColourFinishDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				requiresEnvironmentMap = this.requiresEnvironmentMap,
				isMatte = this.isMatte,
				defaultAlpha = this.defaultAlpha,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ColourFinish list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ColourFinishDTO> ToDTOList(List<ColourFinish> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ColourFinishDTO> output = new List<ColourFinishDTO>();

			output.Capacity = data.Count;

			foreach (ColourFinish colourFinish in data)
			{
				output.Add(colourFinish.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ColourFinish to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ColourFinishEntity type directly.
		///
		/// </summary>
		public ColourFinishOutputDTO ToOutputDTO()
		{
			return new ColourFinishOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				requiresEnvironmentMap = this.requiresEnvironmentMap,
				isMatte = this.isMatte,
				defaultAlpha = this.defaultAlpha,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ColourFinish list to list of Output Data Transfer Object intended to be used for serializing a list of ColourFinish objects to avoid using the ColourFinish entity type directly.
		///
		/// </summary>
		public static List<ColourFinishOutputDTO> ToOutputDTOList(List<ColourFinish> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ColourFinishOutputDTO> output = new List<ColourFinishOutputDTO>();

			output.Capacity = data.Count;

			foreach (ColourFinish colourFinish in data)
			{
				output.Add(colourFinish.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ColourFinish Object.
		///
		/// </summary>
		public static Database.ColourFinish FromDTO(ColourFinishDTO dto)
		{
			return new Database.ColourFinish
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				requiresEnvironmentMap = dto.requiresEnvironmentMap,
				isMatte = dto.isMatte,
				defaultAlpha = dto.defaultAlpha,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ColourFinish Object.
		///
		/// </summary>
		public void ApplyDTO(ColourFinishDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.requiresEnvironmentMap = dto.requiresEnvironmentMap;
			this.isMatte = dto.isMatte;
			this.defaultAlpha = dto.defaultAlpha;
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
		/// Creates a deep copy clone of a ColourFinish Object.
		///
		/// </summary>
		public ColourFinish Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ColourFinish{
				id = this.id,
				name = this.name,
				description = this.description,
				requiresEnvironmentMap = this.requiresEnvironmentMap,
				isMatte = this.isMatte,
				defaultAlpha = this.defaultAlpha,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ColourFinish Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ColourFinish Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ColourFinish Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ColourFinish Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ColourFinish colourFinish)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (colourFinish == null)
			{
				return null;
			}

			return new {
				id = colourFinish.id,
				name = colourFinish.name,
				description = colourFinish.description,
				requiresEnvironmentMap = colourFinish.requiresEnvironmentMap,
				isMatte = colourFinish.isMatte,
				defaultAlpha = colourFinish.defaultAlpha,
				sequence = colourFinish.sequence,
				objectGuid = colourFinish.objectGuid,
				active = colourFinish.active,
				deleted = colourFinish.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ColourFinish Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ColourFinish colourFinish)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (colourFinish == null)
			{
				return null;
			}

			return new {
				id = colourFinish.id,
				name = colourFinish.name,
				description = colourFinish.description,
				requiresEnvironmentMap = colourFinish.requiresEnvironmentMap,
				isMatte = colourFinish.isMatte,
				defaultAlpha = colourFinish.defaultAlpha,
				sequence = colourFinish.sequence,
				objectGuid = colourFinish.objectGuid,
				active = colourFinish.active,
				deleted = colourFinish.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ColourFinish Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ColourFinish colourFinish)
		{
			//
			// Return a very minimal object.
			//
			if (colourFinish == null)
			{
				return null;
			}

			return new {
				id = colourFinish.id,
				name = colourFinish.name,
				description = colourFinish.description,
			 };
		}
	}
}
