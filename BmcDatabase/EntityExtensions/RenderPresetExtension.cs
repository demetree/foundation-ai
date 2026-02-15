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
	public partial class RenderPreset : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class RenderPresetDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
			public Int32? resolutionWidth { get; set; }
			public Int32? resolutionHeight { get; set; }
			public String backgroundColorHex { get; set; }
			[Required]
			public Boolean enableShadows { get; set; }
			[Required]
			public Boolean enableReflections { get; set; }
			public String lightingMode { get; set; }
			public Int32? antiAliasLevel { get; set; }
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
		public class RenderPresetOutputDTO : RenderPresetDTO
		{
		}


		/// <summary>
		///
		/// Converts a RenderPreset to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public RenderPresetDTO ToDTO()
		{
			return new RenderPresetDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				resolutionWidth = this.resolutionWidth,
				resolutionHeight = this.resolutionHeight,
				backgroundColorHex = this.backgroundColorHex,
				enableShadows = this.enableShadows,
				enableReflections = this.enableReflections,
				lightingMode = this.lightingMode,
				antiAliasLevel = this.antiAliasLevel,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a RenderPreset list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<RenderPresetDTO> ToDTOList(List<RenderPreset> data)
		{
			if (data == null)
			{
				return null;
			}

			List<RenderPresetDTO> output = new List<RenderPresetDTO>();

			output.Capacity = data.Count;

			foreach (RenderPreset renderPreset in data)
			{
				output.Add(renderPreset.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a RenderPreset to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the RenderPresetEntity type directly.
		///
		/// </summary>
		public RenderPresetOutputDTO ToOutputDTO()
		{
			return new RenderPresetOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				resolutionWidth = this.resolutionWidth,
				resolutionHeight = this.resolutionHeight,
				backgroundColorHex = this.backgroundColorHex,
				enableShadows = this.enableShadows,
				enableReflections = this.enableReflections,
				lightingMode = this.lightingMode,
				antiAliasLevel = this.antiAliasLevel,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a RenderPreset list to list of Output Data Transfer Object intended to be used for serializing a list of RenderPreset objects to avoid using the RenderPreset entity type directly.
		///
		/// </summary>
		public static List<RenderPresetOutputDTO> ToOutputDTOList(List<RenderPreset> data)
		{
			if (data == null)
			{
				return null;
			}

			List<RenderPresetOutputDTO> output = new List<RenderPresetOutputDTO>();

			output.Capacity = data.Count;

			foreach (RenderPreset renderPreset in data)
			{
				output.Add(renderPreset.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a RenderPreset Object.
		///
		/// </summary>
		public static Database.RenderPreset FromDTO(RenderPresetDTO dto)
		{
			return new Database.RenderPreset
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				resolutionWidth = dto.resolutionWidth,
				resolutionHeight = dto.resolutionHeight,
				backgroundColorHex = dto.backgroundColorHex,
				enableShadows = dto.enableShadows,
				enableReflections = dto.enableReflections,
				lightingMode = dto.lightingMode,
				antiAliasLevel = dto.antiAliasLevel,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a RenderPreset Object.
		///
		/// </summary>
		public void ApplyDTO(RenderPresetDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.resolutionWidth = dto.resolutionWidth;
			this.resolutionHeight = dto.resolutionHeight;
			this.backgroundColorHex = dto.backgroundColorHex;
			this.enableShadows = dto.enableShadows;
			this.enableReflections = dto.enableReflections;
			this.lightingMode = dto.lightingMode;
			this.antiAliasLevel = dto.antiAliasLevel;
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
		/// Creates a deep copy clone of a RenderPreset Object.
		///
		/// </summary>
		public RenderPreset Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new RenderPreset{
				id = this.id,
				tenantGuid = this.tenantGuid,
				name = this.name,
				description = this.description,
				resolutionWidth = this.resolutionWidth,
				resolutionHeight = this.resolutionHeight,
				backgroundColorHex = this.backgroundColorHex,
				enableShadows = this.enableShadows,
				enableReflections = this.enableReflections,
				lightingMode = this.lightingMode,
				antiAliasLevel = this.antiAliasLevel,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a RenderPreset Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a RenderPreset Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a RenderPreset Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a RenderPreset Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.RenderPreset renderPreset)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (renderPreset == null)
			{
				return null;
			}

			return new {
				id = renderPreset.id,
				name = renderPreset.name,
				description = renderPreset.description,
				resolutionWidth = renderPreset.resolutionWidth,
				resolutionHeight = renderPreset.resolutionHeight,
				backgroundColorHex = renderPreset.backgroundColorHex,
				enableShadows = renderPreset.enableShadows,
				enableReflections = renderPreset.enableReflections,
				lightingMode = renderPreset.lightingMode,
				antiAliasLevel = renderPreset.antiAliasLevel,
				objectGuid = renderPreset.objectGuid,
				active = renderPreset.active,
				deleted = renderPreset.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a RenderPreset Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(RenderPreset renderPreset)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (renderPreset == null)
			{
				return null;
			}

			return new {
				id = renderPreset.id,
				name = renderPreset.name,
				description = renderPreset.description,
				resolutionWidth = renderPreset.resolutionWidth,
				resolutionHeight = renderPreset.resolutionHeight,
				backgroundColorHex = renderPreset.backgroundColorHex,
				enableShadows = renderPreset.enableShadows,
				enableReflections = renderPreset.enableReflections,
				lightingMode = renderPreset.lightingMode,
				antiAliasLevel = renderPreset.antiAliasLevel,
				objectGuid = renderPreset.objectGuid,
				active = renderPreset.active,
				deleted = renderPreset.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a RenderPreset Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(RenderPreset renderPreset)
		{
			//
			// Return a very minimal object.
			//
			if (renderPreset == null)
			{
				return null;
			}

			return new {
				id = renderPreset.id,
				name = renderPreset.name,
				description = renderPreset.description,
			 };
		}
	}
}
