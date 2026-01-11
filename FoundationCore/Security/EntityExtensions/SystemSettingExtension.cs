using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Security.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class SystemSetting : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SystemSettingDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			public String description { get; set; }
			public String value { get; set; }
			public Boolean? active { get; set; }
			public Boolean? deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class SystemSettingOutputDTO : SystemSettingDTO
		{
		}


		/// <summary>
		///
		/// Converts a SystemSetting to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SystemSettingDTO ToDTO()
		{
			return new SystemSettingDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				value = this.value,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SystemSetting list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SystemSettingDTO> ToDTOList(List<SystemSetting> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SystemSettingDTO> output = new List<SystemSettingDTO>();

			output.Capacity = data.Count;

			foreach (SystemSetting systemSetting in data)
			{
				output.Add(systemSetting.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SystemSetting to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SystemSettingEntity type directly.
		///
		/// </summary>
		public SystemSettingOutputDTO ToOutputDTO()
		{
			return new SystemSettingOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				value = this.value,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SystemSetting list to list of Output Data Transfer Object intended to be used for serializing a list of SystemSetting objects to avoid using the SystemSetting entity type directly.
		///
		/// </summary>
		public static List<SystemSettingOutputDTO> ToOutputDTOList(List<SystemSetting> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SystemSettingOutputDTO> output = new List<SystemSettingOutputDTO>();

			output.Capacity = data.Count;

			foreach (SystemSetting systemSetting in data)
			{
				output.Add(systemSetting.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SystemSetting Object.
		///
		/// </summary>
		public static Database.SystemSetting FromDTO(SystemSettingDTO dto)
		{
			return new Database.SystemSetting
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				value = dto.value,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SystemSetting Object.
		///
		/// </summary>
		public void ApplyDTO(SystemSettingDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.value = dto.value;
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
		/// Creates a deep copy clone of a SystemSetting Object.
		///
		/// </summary>
		public SystemSetting Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SystemSetting{
				id = this.id,
				name = this.name,
				description = this.description,
				value = this.value,
				active = this.active,
				deleted = this.deleted
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SystemSetting Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SystemSetting Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SystemSetting Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SystemSetting Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SystemSetting systemSetting)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (systemSetting == null)
			{
				return null;
			}

			return new {
				id = systemSetting.id,
				name = systemSetting.name,
				description = systemSetting.description,
				value = systemSetting.value,
				active = systemSetting.active,
				deleted = systemSetting.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SystemSetting Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SystemSetting systemSetting)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (systemSetting == null)
			{
				return null;
			}

			return new {
				id = systemSetting.id,
				name = systemSetting.name,
				description = systemSetting.description,
				value = systemSetting.value,
				active = systemSetting.active,
				deleted = systemSetting.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SystemSetting Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SystemSetting systemSetting)
		{
			//
			// Return a very minimal object.
			//
			if (systemSetting == null)
			{
				return null;
			}

			return new {
				id = systemSetting.id,
				name = systemSetting.name,
				description = systemSetting.description,
			 };
		}
	}
}
