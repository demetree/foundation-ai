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
	public partial class Achievement : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class AchievementDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 achievementCategoryId { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
			public String iconCssClass { get; set; }
			public String iconImagePath { get; set; }
			public String criteria { get; set; }
			public String criteriaCode { get; set; }
			[Required]
			public Int32 pointValue { get; set; }
			[Required]
			public String rarity { get; set; }
			[Required]
			public Boolean isActive { get; set; }
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
		public class AchievementOutputDTO : AchievementDTO
		{
			public AchievementCategory.AchievementCategoryDTO achievementCategory { get; set; }
		}


		/// <summary>
		///
		/// Converts a Achievement to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public AchievementDTO ToDTO()
		{
			return new AchievementDTO
			{
				id = this.id,
				achievementCategoryId = this.achievementCategoryId,
				name = this.name,
				description = this.description,
				iconCssClass = this.iconCssClass,
				iconImagePath = this.iconImagePath,
				criteria = this.criteria,
				criteriaCode = this.criteriaCode,
				pointValue = this.pointValue,
				rarity = this.rarity,
				isActive = this.isActive,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a Achievement list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<AchievementDTO> ToDTOList(List<Achievement> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AchievementDTO> output = new List<AchievementDTO>();

			output.Capacity = data.Count;

			foreach (Achievement achievement in data)
			{
				output.Add(achievement.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a Achievement to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the AchievementEntity type directly.
		///
		/// </summary>
		public AchievementOutputDTO ToOutputDTO()
		{
			return new AchievementOutputDTO
			{
				id = this.id,
				achievementCategoryId = this.achievementCategoryId,
				name = this.name,
				description = this.description,
				iconCssClass = this.iconCssClass,
				iconImagePath = this.iconImagePath,
				criteria = this.criteria,
				criteriaCode = this.criteriaCode,
				pointValue = this.pointValue,
				rarity = this.rarity,
				isActive = this.isActive,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				achievementCategory = this.achievementCategory?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a Achievement list to list of Output Data Transfer Object intended to be used for serializing a list of Achievement objects to avoid using the Achievement entity type directly.
		///
		/// </summary>
		public static List<AchievementOutputDTO> ToOutputDTOList(List<Achievement> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AchievementOutputDTO> output = new List<AchievementOutputDTO>();

			output.Capacity = data.Count;

			foreach (Achievement achievement in data)
			{
				output.Add(achievement.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a Achievement Object.
		///
		/// </summary>
		public static Database.Achievement FromDTO(AchievementDTO dto)
		{
			return new Database.Achievement
			{
				id = dto.id,
				achievementCategoryId = dto.achievementCategoryId,
				name = dto.name,
				description = dto.description,
				iconCssClass = dto.iconCssClass,
				iconImagePath = dto.iconImagePath,
				criteria = dto.criteria,
				criteriaCode = dto.criteriaCode,
				pointValue = dto.pointValue,
				rarity = dto.rarity,
				isActive = dto.isActive,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a Achievement Object.
		///
		/// </summary>
		public void ApplyDTO(AchievementDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.achievementCategoryId = dto.achievementCategoryId;
			this.name = dto.name;
			this.description = dto.description;
			this.iconCssClass = dto.iconCssClass;
			this.iconImagePath = dto.iconImagePath;
			this.criteria = dto.criteria;
			this.criteriaCode = dto.criteriaCode;
			this.pointValue = dto.pointValue;
			this.rarity = dto.rarity;
			this.isActive = dto.isActive;
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
		/// Creates a deep copy clone of a Achievement Object.
		///
		/// </summary>
		public Achievement Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new Achievement{
				id = this.id,
				achievementCategoryId = this.achievementCategoryId,
				name = this.name,
				description = this.description,
				iconCssClass = this.iconCssClass,
				iconImagePath = this.iconImagePath,
				criteria = this.criteria,
				criteriaCode = this.criteriaCode,
				pointValue = this.pointValue,
				rarity = this.rarity,
				isActive = this.isActive,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Achievement Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Achievement Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a Achievement Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a Achievement Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.Achievement achievement)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (achievement == null)
			{
				return null;
			}

			return new {
				id = achievement.id,
				achievementCategoryId = achievement.achievementCategoryId,
				name = achievement.name,
				description = achievement.description,
				iconCssClass = achievement.iconCssClass,
				iconImagePath = achievement.iconImagePath,
				criteria = achievement.criteria,
				criteriaCode = achievement.criteriaCode,
				pointValue = achievement.pointValue,
				rarity = achievement.rarity,
				isActive = achievement.isActive,
				sequence = achievement.sequence,
				objectGuid = achievement.objectGuid,
				active = achievement.active,
				deleted = achievement.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a Achievement Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(Achievement achievement)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (achievement == null)
			{
				return null;
			}

			return new {
				id = achievement.id,
				achievementCategoryId = achievement.achievementCategoryId,
				name = achievement.name,
				description = achievement.description,
				iconCssClass = achievement.iconCssClass,
				iconImagePath = achievement.iconImagePath,
				criteria = achievement.criteria,
				criteriaCode = achievement.criteriaCode,
				pointValue = achievement.pointValue,
				rarity = achievement.rarity,
				isActive = achievement.isActive,
				sequence = achievement.sequence,
				objectGuid = achievement.objectGuid,
				active = achievement.active,
				deleted = achievement.deleted,
				achievementCategory = AchievementCategory.CreateMinimalAnonymous(achievement.achievementCategory)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a Achievement Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(Achievement achievement)
		{
			//
			// Return a very minimal object.
			//
			if (achievement == null)
			{
				return null;
			}

			return new {
				id = achievement.id,
				name = achievement.name,
				description = achievement.description,
			 };
		}
	}
}
