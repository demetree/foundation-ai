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
	public partial class UserProfilePreferredTheme : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class UserProfilePreferredThemeDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 userProfileId { get; set; }
			[Required]
			public Int32 legoThemeId { get; set; }
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
		public class UserProfilePreferredThemeOutputDTO : UserProfilePreferredThemeDTO
		{
			public LegoTheme.LegoThemeDTO legoTheme { get; set; }
			public UserProfile.UserProfileDTO userProfile { get; set; }
		}


		/// <summary>
		///
		/// Converts a UserProfilePreferredTheme to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public UserProfilePreferredThemeDTO ToDTO()
		{
			return new UserProfilePreferredThemeDTO
			{
				id = this.id,
				userProfileId = this.userProfileId,
				legoThemeId = this.legoThemeId,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a UserProfilePreferredTheme list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<UserProfilePreferredThemeDTO> ToDTOList(List<UserProfilePreferredTheme> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserProfilePreferredThemeDTO> output = new List<UserProfilePreferredThemeDTO>();

			output.Capacity = data.Count;

			foreach (UserProfilePreferredTheme userProfilePreferredTheme in data)
			{
				output.Add(userProfilePreferredTheme.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a UserProfilePreferredTheme to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the UserProfilePreferredThemeEntity type directly.
		///
		/// </summary>
		public UserProfilePreferredThemeOutputDTO ToOutputDTO()
		{
			return new UserProfilePreferredThemeOutputDTO
			{
				id = this.id,
				userProfileId = this.userProfileId,
				legoThemeId = this.legoThemeId,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				legoTheme = this.legoTheme?.ToDTO(),
				userProfile = this.userProfile?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a UserProfilePreferredTheme list to list of Output Data Transfer Object intended to be used for serializing a list of UserProfilePreferredTheme objects to avoid using the UserProfilePreferredTheme entity type directly.
		///
		/// </summary>
		public static List<UserProfilePreferredThemeOutputDTO> ToOutputDTOList(List<UserProfilePreferredTheme> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserProfilePreferredThemeOutputDTO> output = new List<UserProfilePreferredThemeOutputDTO>();

			output.Capacity = data.Count;

			foreach (UserProfilePreferredTheme userProfilePreferredTheme in data)
			{
				output.Add(userProfilePreferredTheme.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a UserProfilePreferredTheme Object.
		///
		/// </summary>
		public static Database.UserProfilePreferredTheme FromDTO(UserProfilePreferredThemeDTO dto)
		{
			return new Database.UserProfilePreferredTheme
			{
				id = dto.id,
				userProfileId = dto.userProfileId,
				legoThemeId = dto.legoThemeId,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a UserProfilePreferredTheme Object.
		///
		/// </summary>
		public void ApplyDTO(UserProfilePreferredThemeDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.userProfileId = dto.userProfileId;
			this.legoThemeId = dto.legoThemeId;
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
		/// Creates a deep copy clone of a UserProfilePreferredTheme Object.
		///
		/// </summary>
		public UserProfilePreferredTheme Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new UserProfilePreferredTheme{
				id = this.id,
				tenantGuid = this.tenantGuid,
				userProfileId = this.userProfileId,
				legoThemeId = this.legoThemeId,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserProfilePreferredTheme Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserProfilePreferredTheme Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a UserProfilePreferredTheme Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a UserProfilePreferredTheme Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.UserProfilePreferredTheme userProfilePreferredTheme)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (userProfilePreferredTheme == null)
			{
				return null;
			}

			return new {
				id = userProfilePreferredTheme.id,
				userProfileId = userProfilePreferredTheme.userProfileId,
				legoThemeId = userProfilePreferredTheme.legoThemeId,
				sequence = userProfilePreferredTheme.sequence,
				objectGuid = userProfilePreferredTheme.objectGuid,
				active = userProfilePreferredTheme.active,
				deleted = userProfilePreferredTheme.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a UserProfilePreferredTheme Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(UserProfilePreferredTheme userProfilePreferredTheme)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (userProfilePreferredTheme == null)
			{
				return null;
			}

			return new {
				id = userProfilePreferredTheme.id,
				userProfileId = userProfilePreferredTheme.userProfileId,
				legoThemeId = userProfilePreferredTheme.legoThemeId,
				sequence = userProfilePreferredTheme.sequence,
				objectGuid = userProfilePreferredTheme.objectGuid,
				active = userProfilePreferredTheme.active,
				deleted = userProfilePreferredTheme.deleted,
				legoTheme = LegoTheme.CreateMinimalAnonymous(userProfilePreferredTheme.legoTheme),
				userProfile = UserProfile.CreateMinimalAnonymous(userProfilePreferredTheme.userProfile)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a UserProfilePreferredTheme Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(UserProfilePreferredTheme userProfilePreferredTheme)
		{
			//
			// Return a very minimal object.
			//
			if (userProfilePreferredTheme == null)
			{
				return null;
			}

			return new {
				id = userProfilePreferredTheme.id,
				name = userProfilePreferredTheme.id,
				description = userProfilePreferredTheme.id
			 };
		}
	}
}
