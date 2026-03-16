using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Community.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class SiteSetting : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SiteSettingDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String settingKey { get; set; }
			public String settingValue { get; set; }
			public String description { get; set; }
			public String settingGroup { get; set; }
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
		public class SiteSettingOutputDTO : SiteSettingDTO
		{
		}


		/// <summary>
		///
		/// Converts a SiteSetting to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SiteSettingDTO ToDTO()
		{
			return new SiteSettingDTO
			{
				id = this.id,
				settingKey = this.settingKey,
				settingValue = this.settingValue,
				description = this.description,
				settingGroup = this.settingGroup,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SiteSetting list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SiteSettingDTO> ToDTOList(List<SiteSetting> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SiteSettingDTO> output = new List<SiteSettingDTO>();

			output.Capacity = data.Count;

			foreach (SiteSetting siteSetting in data)
			{
				output.Add(siteSetting.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SiteSetting to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SiteSettingEntity type directly.
		///
		/// </summary>
		public SiteSettingOutputDTO ToOutputDTO()
		{
			return new SiteSettingOutputDTO
			{
				id = this.id,
				settingKey = this.settingKey,
				settingValue = this.settingValue,
				description = this.description,
				settingGroup = this.settingGroup,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SiteSetting list to list of Output Data Transfer Object intended to be used for serializing a list of SiteSetting objects to avoid using the SiteSetting entity type directly.
		///
		/// </summary>
		public static List<SiteSettingOutputDTO> ToOutputDTOList(List<SiteSetting> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SiteSettingOutputDTO> output = new List<SiteSettingOutputDTO>();

			output.Capacity = data.Count;

			foreach (SiteSetting siteSetting in data)
			{
				output.Add(siteSetting.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SiteSetting Object.
		///
		/// </summary>
		public static Database.SiteSetting FromDTO(SiteSettingDTO dto)
		{
			return new Database.SiteSetting
			{
				id = dto.id,
				settingKey = dto.settingKey,
				settingValue = dto.settingValue,
				description = dto.description,
				settingGroup = dto.settingGroup,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SiteSetting Object.
		///
		/// </summary>
		public void ApplyDTO(SiteSettingDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.settingKey = dto.settingKey;
			this.settingValue = dto.settingValue;
			this.description = dto.description;
			this.settingGroup = dto.settingGroup;
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
		/// Creates a deep copy clone of a SiteSetting Object.
		///
		/// </summary>
		public SiteSetting Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SiteSetting{
				id = this.id,
				tenantGuid = this.tenantGuid,
				settingKey = this.settingKey,
				settingValue = this.settingValue,
				description = this.description,
				settingGroup = this.settingGroup,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SiteSetting Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SiteSetting Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SiteSetting Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SiteSetting Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SiteSetting siteSetting)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (siteSetting == null)
			{
				return null;
			}

			return new {
				id = siteSetting.id,
				settingKey = siteSetting.settingKey,
				settingValue = siteSetting.settingValue,
				description = siteSetting.description,
				settingGroup = siteSetting.settingGroup,
				objectGuid = siteSetting.objectGuid,
				active = siteSetting.active,
				deleted = siteSetting.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SiteSetting Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SiteSetting siteSetting)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (siteSetting == null)
			{
				return null;
			}

			return new {
				id = siteSetting.id,
				settingKey = siteSetting.settingKey,
				settingValue = siteSetting.settingValue,
				description = siteSetting.description,
				settingGroup = siteSetting.settingGroup,
				objectGuid = siteSetting.objectGuid,
				active = siteSetting.active,
				deleted = siteSetting.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SiteSetting Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SiteSetting siteSetting)
		{
			//
			// Return a very minimal object.
			//
			if (siteSetting == null)
			{
				return null;
			}

			return new {
				id = siteSetting.id,
				description = siteSetting.description,
				name = siteSetting.settingKey
			 };
		}
	}
}
