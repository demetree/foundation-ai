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
	public partial class UserBadge : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class UserBadgeDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
			public String iconCssClass { get; set; }
			public String iconImagePath { get; set; }
			public String badgeColor { get; set; }
			[Required]
			public Boolean isAutomatic { get; set; }
			public String automaticCriteriaCode { get; set; }
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
		public class UserBadgeOutputDTO : UserBadgeDTO
		{
		}


		/// <summary>
		///
		/// Converts a UserBadge to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public UserBadgeDTO ToDTO()
		{
			return new UserBadgeDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				iconCssClass = this.iconCssClass,
				iconImagePath = this.iconImagePath,
				badgeColor = this.badgeColor,
				isAutomatic = this.isAutomatic,
				automaticCriteriaCode = this.automaticCriteriaCode,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a UserBadge list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<UserBadgeDTO> ToDTOList(List<UserBadge> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserBadgeDTO> output = new List<UserBadgeDTO>();

			output.Capacity = data.Count;

			foreach (UserBadge userBadge in data)
			{
				output.Add(userBadge.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a UserBadge to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the UserBadgeEntity type directly.
		///
		/// </summary>
		public UserBadgeOutputDTO ToOutputDTO()
		{
			return new UserBadgeOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				iconCssClass = this.iconCssClass,
				iconImagePath = this.iconImagePath,
				badgeColor = this.badgeColor,
				isAutomatic = this.isAutomatic,
				automaticCriteriaCode = this.automaticCriteriaCode,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a UserBadge list to list of Output Data Transfer Object intended to be used for serializing a list of UserBadge objects to avoid using the UserBadge entity type directly.
		///
		/// </summary>
		public static List<UserBadgeOutputDTO> ToOutputDTOList(List<UserBadge> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserBadgeOutputDTO> output = new List<UserBadgeOutputDTO>();

			output.Capacity = data.Count;

			foreach (UserBadge userBadge in data)
			{
				output.Add(userBadge.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a UserBadge Object.
		///
		/// </summary>
		public static Database.UserBadge FromDTO(UserBadgeDTO dto)
		{
			return new Database.UserBadge
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				iconCssClass = dto.iconCssClass,
				iconImagePath = dto.iconImagePath,
				badgeColor = dto.badgeColor,
				isAutomatic = dto.isAutomatic,
				automaticCriteriaCode = dto.automaticCriteriaCode,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a UserBadge Object.
		///
		/// </summary>
		public void ApplyDTO(UserBadgeDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.iconCssClass = dto.iconCssClass;
			this.iconImagePath = dto.iconImagePath;
			this.badgeColor = dto.badgeColor;
			this.isAutomatic = dto.isAutomatic;
			this.automaticCriteriaCode = dto.automaticCriteriaCode;
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
		/// Creates a deep copy clone of a UserBadge Object.
		///
		/// </summary>
		public UserBadge Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new UserBadge{
				id = this.id,
				name = this.name,
				description = this.description,
				iconCssClass = this.iconCssClass,
				iconImagePath = this.iconImagePath,
				badgeColor = this.badgeColor,
				isAutomatic = this.isAutomatic,
				automaticCriteriaCode = this.automaticCriteriaCode,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserBadge Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserBadge Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a UserBadge Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a UserBadge Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.UserBadge userBadge)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (userBadge == null)
			{
				return null;
			}

			return new {
				id = userBadge.id,
				name = userBadge.name,
				description = userBadge.description,
				iconCssClass = userBadge.iconCssClass,
				iconImagePath = userBadge.iconImagePath,
				badgeColor = userBadge.badgeColor,
				isAutomatic = userBadge.isAutomatic,
				automaticCriteriaCode = userBadge.automaticCriteriaCode,
				sequence = userBadge.sequence,
				objectGuid = userBadge.objectGuid,
				active = userBadge.active,
				deleted = userBadge.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a UserBadge Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(UserBadge userBadge)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (userBadge == null)
			{
				return null;
			}

			return new {
				id = userBadge.id,
				name = userBadge.name,
				description = userBadge.description,
				iconCssClass = userBadge.iconCssClass,
				iconImagePath = userBadge.iconImagePath,
				badgeColor = userBadge.badgeColor,
				isAutomatic = userBadge.isAutomatic,
				automaticCriteriaCode = userBadge.automaticCriteriaCode,
				sequence = userBadge.sequence,
				objectGuid = userBadge.objectGuid,
				active = userBadge.active,
				deleted = userBadge.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a UserBadge Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(UserBadge userBadge)
		{
			//
			// Return a very minimal object.
			//
			if (userBadge == null)
			{
				return null;
			}

			return new {
				id = userBadge.id,
				name = userBadge.name,
				description = userBadge.description,
			 };
		}
	}
}
