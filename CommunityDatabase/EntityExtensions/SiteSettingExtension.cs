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
			public Int32 Id { get; set; }
			public String SettingKey { get; set; }
			public String SettingValue { get; set; }
			public String Description { get; set; }
			public String SettingGroup { get; set; }
			public Guid ObjectGuid { get; set; }
			public Boolean? Active { get; set; }
			public Boolean? Deleted { get; set; }
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
				Id = this.Id,
				SettingKey = this.SettingKey,
				SettingValue = this.SettingValue,
				Description = this.Description,
				SettingGroup = this.SettingGroup,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted
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
				Id = this.Id,
				SettingKey = this.SettingKey,
				SettingValue = this.SettingValue,
				Description = this.Description,
				SettingGroup = this.SettingGroup,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted
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
				Id = dto.Id,
				SettingKey = dto.SettingKey,
				SettingValue = dto.SettingValue,
				Description = dto.Description,
				SettingGroup = dto.SettingGroup,
				ObjectGuid = dto.ObjectGuid,
				Active = dto.Active ?? true,
				Deleted = dto.Deleted ?? false
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

			this.SettingKey = dto.SettingKey;
			this.SettingValue = dto.SettingValue;
			this.Description = dto.Description;
			this.SettingGroup = dto.SettingGroup;
			this.ObjectGuid = dto.ObjectGuid;
			if (dto.Active.HasValue == true)
			{
				this.Active = dto.Active.Value;
			}
			if (dto.Deleted.HasValue == true)
			{
				this.Deleted = dto.Deleted.Value;
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
				Id = this.Id,
				SettingKey = this.SettingKey,
				SettingValue = this.SettingValue,
				Description = this.Description,
				SettingGroup = this.SettingGroup,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted
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
				Id = siteSetting.Id,
				SettingKey = siteSetting.SettingKey,
				SettingValue = siteSetting.SettingValue,
				Description = siteSetting.Description,
				SettingGroup = siteSetting.SettingGroup,
				ObjectGuid = siteSetting.ObjectGuid,
				Active = siteSetting.Active,
				Deleted = siteSetting.Deleted
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
				Id = siteSetting.Id,
				SettingKey = siteSetting.SettingKey,
				SettingValue = siteSetting.SettingValue,
				Description = siteSetting.Description,
				SettingGroup = siteSetting.SettingGroup,
				ObjectGuid = siteSetting.ObjectGuid,
				Active = siteSetting.Active,
				Deleted = siteSetting.Deleted
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
				name = siteSetting.settingKey,
				description = string.Join(", ", new[] { siteSetting.settingKey, siteSetting.description, siteSetting.settingGroup}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
