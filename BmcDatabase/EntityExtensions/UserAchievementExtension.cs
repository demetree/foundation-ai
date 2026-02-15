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
	public partial class UserAchievement : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class UserAchievementDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 achievementId { get; set; }
			[Required]
			public DateTime earnedDate { get; set; }
			[Required]
			public Boolean isDisplayed { get; set; }
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
		public class UserAchievementOutputDTO : UserAchievementDTO
		{
			public Achievement.AchievementDTO achievement { get; set; }
		}


		/// <summary>
		///
		/// Converts a UserAchievement to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public UserAchievementDTO ToDTO()
		{
			return new UserAchievementDTO
			{
				id = this.id,
				achievementId = this.achievementId,
				earnedDate = this.earnedDate,
				isDisplayed = this.isDisplayed,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a UserAchievement list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<UserAchievementDTO> ToDTOList(List<UserAchievement> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserAchievementDTO> output = new List<UserAchievementDTO>();

			output.Capacity = data.Count;

			foreach (UserAchievement userAchievement in data)
			{
				output.Add(userAchievement.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a UserAchievement to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the UserAchievementEntity type directly.
		///
		/// </summary>
		public UserAchievementOutputDTO ToOutputDTO()
		{
			return new UserAchievementOutputDTO
			{
				id = this.id,
				achievementId = this.achievementId,
				earnedDate = this.earnedDate,
				isDisplayed = this.isDisplayed,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				achievement = this.achievement?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a UserAchievement list to list of Output Data Transfer Object intended to be used for serializing a list of UserAchievement objects to avoid using the UserAchievement entity type directly.
		///
		/// </summary>
		public static List<UserAchievementOutputDTO> ToOutputDTOList(List<UserAchievement> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserAchievementOutputDTO> output = new List<UserAchievementOutputDTO>();

			output.Capacity = data.Count;

			foreach (UserAchievement userAchievement in data)
			{
				output.Add(userAchievement.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a UserAchievement Object.
		///
		/// </summary>
		public static Database.UserAchievement FromDTO(UserAchievementDTO dto)
		{
			return new Database.UserAchievement
			{
				id = dto.id,
				achievementId = dto.achievementId,
				earnedDate = dto.earnedDate,
				isDisplayed = dto.isDisplayed,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a UserAchievement Object.
		///
		/// </summary>
		public void ApplyDTO(UserAchievementDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.achievementId = dto.achievementId;
			this.earnedDate = dto.earnedDate;
			this.isDisplayed = dto.isDisplayed;
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
		/// Creates a deep copy clone of a UserAchievement Object.
		///
		/// </summary>
		public UserAchievement Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new UserAchievement{
				id = this.id,
				tenantGuid = this.tenantGuid,
				achievementId = this.achievementId,
				earnedDate = this.earnedDate,
				isDisplayed = this.isDisplayed,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserAchievement Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserAchievement Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a UserAchievement Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a UserAchievement Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.UserAchievement userAchievement)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (userAchievement == null)
			{
				return null;
			}

			return new {
				id = userAchievement.id,
				achievementId = userAchievement.achievementId,
				earnedDate = userAchievement.earnedDate,
				isDisplayed = userAchievement.isDisplayed,
				objectGuid = userAchievement.objectGuid,
				active = userAchievement.active,
				deleted = userAchievement.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a UserAchievement Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(UserAchievement userAchievement)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (userAchievement == null)
			{
				return null;
			}

			return new {
				id = userAchievement.id,
				achievementId = userAchievement.achievementId,
				earnedDate = userAchievement.earnedDate,
				isDisplayed = userAchievement.isDisplayed,
				objectGuid = userAchievement.objectGuid,
				active = userAchievement.active,
				deleted = userAchievement.deleted,
				achievement = Achievement.CreateMinimalAnonymous(userAchievement.achievement)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a UserAchievement Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(UserAchievement userAchievement)
		{
			//
			// Return a very minimal object.
			//
			if (userAchievement == null)
			{
				return null;
			}

			return new {
				id = userAchievement.id,
				name = userAchievement.id,
				description = userAchievement.id
			 };
		}
	}
}
