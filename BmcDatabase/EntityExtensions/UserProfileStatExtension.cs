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
	public partial class UserProfileStat : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class UserProfileStatDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 userProfileId { get; set; }
			[Required]
			public Int32 totalPartsOwned { get; set; }
			[Required]
			public Int32 totalUniquePartsOwned { get; set; }
			[Required]
			public Int32 totalSetsOwned { get; set; }
			[Required]
			public Int32 totalMocsPublished { get; set; }
			[Required]
			public Int32 totalFollowers { get; set; }
			[Required]
			public Int32 totalFollowing { get; set; }
			[Required]
			public Int32 totalLikesReceived { get; set; }
			[Required]
			public Int32 totalAchievementPoints { get; set; }
			public DateTime? lastCalculatedDate { get; set; }
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
		public class UserProfileStatOutputDTO : UserProfileStatDTO
		{
			public UserProfile.UserProfileDTO userProfile { get; set; }
		}


		/// <summary>
		///
		/// Converts a UserProfileStat to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public UserProfileStatDTO ToDTO()
		{
			return new UserProfileStatDTO
			{
				id = this.id,
				userProfileId = this.userProfileId,
				totalPartsOwned = this.totalPartsOwned,
				totalUniquePartsOwned = this.totalUniquePartsOwned,
				totalSetsOwned = this.totalSetsOwned,
				totalMocsPublished = this.totalMocsPublished,
				totalFollowers = this.totalFollowers,
				totalFollowing = this.totalFollowing,
				totalLikesReceived = this.totalLikesReceived,
				totalAchievementPoints = this.totalAchievementPoints,
				lastCalculatedDate = this.lastCalculatedDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a UserProfileStat list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<UserProfileStatDTO> ToDTOList(List<UserProfileStat> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserProfileStatDTO> output = new List<UserProfileStatDTO>();

			output.Capacity = data.Count;

			foreach (UserProfileStat userProfileStat in data)
			{
				output.Add(userProfileStat.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a UserProfileStat to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the UserProfileStatEntity type directly.
		///
		/// </summary>
		public UserProfileStatOutputDTO ToOutputDTO()
		{
			return new UserProfileStatOutputDTO
			{
				id = this.id,
				userProfileId = this.userProfileId,
				totalPartsOwned = this.totalPartsOwned,
				totalUniquePartsOwned = this.totalUniquePartsOwned,
				totalSetsOwned = this.totalSetsOwned,
				totalMocsPublished = this.totalMocsPublished,
				totalFollowers = this.totalFollowers,
				totalFollowing = this.totalFollowing,
				totalLikesReceived = this.totalLikesReceived,
				totalAchievementPoints = this.totalAchievementPoints,
				lastCalculatedDate = this.lastCalculatedDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				userProfile = this.userProfile?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a UserProfileStat list to list of Output Data Transfer Object intended to be used for serializing a list of UserProfileStat objects to avoid using the UserProfileStat entity type directly.
		///
		/// </summary>
		public static List<UserProfileStatOutputDTO> ToOutputDTOList(List<UserProfileStat> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserProfileStatOutputDTO> output = new List<UserProfileStatOutputDTO>();

			output.Capacity = data.Count;

			foreach (UserProfileStat userProfileStat in data)
			{
				output.Add(userProfileStat.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a UserProfileStat Object.
		///
		/// </summary>
		public static Database.UserProfileStat FromDTO(UserProfileStatDTO dto)
		{
			return new Database.UserProfileStat
			{
				id = dto.id,
				userProfileId = dto.userProfileId,
				totalPartsOwned = dto.totalPartsOwned,
				totalUniquePartsOwned = dto.totalUniquePartsOwned,
				totalSetsOwned = dto.totalSetsOwned,
				totalMocsPublished = dto.totalMocsPublished,
				totalFollowers = dto.totalFollowers,
				totalFollowing = dto.totalFollowing,
				totalLikesReceived = dto.totalLikesReceived,
				totalAchievementPoints = dto.totalAchievementPoints,
				lastCalculatedDate = dto.lastCalculatedDate,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a UserProfileStat Object.
		///
		/// </summary>
		public void ApplyDTO(UserProfileStatDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.userProfileId = dto.userProfileId;
			this.totalPartsOwned = dto.totalPartsOwned;
			this.totalUniquePartsOwned = dto.totalUniquePartsOwned;
			this.totalSetsOwned = dto.totalSetsOwned;
			this.totalMocsPublished = dto.totalMocsPublished;
			this.totalFollowers = dto.totalFollowers;
			this.totalFollowing = dto.totalFollowing;
			this.totalLikesReceived = dto.totalLikesReceived;
			this.totalAchievementPoints = dto.totalAchievementPoints;
			this.lastCalculatedDate = dto.lastCalculatedDate;
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
		/// Creates a deep copy clone of a UserProfileStat Object.
		///
		/// </summary>
		public UserProfileStat Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new UserProfileStat{
				id = this.id,
				tenantGuid = this.tenantGuid,
				userProfileId = this.userProfileId,
				totalPartsOwned = this.totalPartsOwned,
				totalUniquePartsOwned = this.totalUniquePartsOwned,
				totalSetsOwned = this.totalSetsOwned,
				totalMocsPublished = this.totalMocsPublished,
				totalFollowers = this.totalFollowers,
				totalFollowing = this.totalFollowing,
				totalLikesReceived = this.totalLikesReceived,
				totalAchievementPoints = this.totalAchievementPoints,
				lastCalculatedDate = this.lastCalculatedDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserProfileStat Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserProfileStat Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a UserProfileStat Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a UserProfileStat Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.UserProfileStat userProfileStat)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (userProfileStat == null)
			{
				return null;
			}

			return new {
				id = userProfileStat.id,
				userProfileId = userProfileStat.userProfileId,
				totalPartsOwned = userProfileStat.totalPartsOwned,
				totalUniquePartsOwned = userProfileStat.totalUniquePartsOwned,
				totalSetsOwned = userProfileStat.totalSetsOwned,
				totalMocsPublished = userProfileStat.totalMocsPublished,
				totalFollowers = userProfileStat.totalFollowers,
				totalFollowing = userProfileStat.totalFollowing,
				totalLikesReceived = userProfileStat.totalLikesReceived,
				totalAchievementPoints = userProfileStat.totalAchievementPoints,
				lastCalculatedDate = userProfileStat.lastCalculatedDate,
				objectGuid = userProfileStat.objectGuid,
				active = userProfileStat.active,
				deleted = userProfileStat.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a UserProfileStat Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(UserProfileStat userProfileStat)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (userProfileStat == null)
			{
				return null;
			}

			return new {
				id = userProfileStat.id,
				userProfileId = userProfileStat.userProfileId,
				totalPartsOwned = userProfileStat.totalPartsOwned,
				totalUniquePartsOwned = userProfileStat.totalUniquePartsOwned,
				totalSetsOwned = userProfileStat.totalSetsOwned,
				totalMocsPublished = userProfileStat.totalMocsPublished,
				totalFollowers = userProfileStat.totalFollowers,
				totalFollowing = userProfileStat.totalFollowing,
				totalLikesReceived = userProfileStat.totalLikesReceived,
				totalAchievementPoints = userProfileStat.totalAchievementPoints,
				lastCalculatedDate = userProfileStat.lastCalculatedDate,
				objectGuid = userProfileStat.objectGuid,
				active = userProfileStat.active,
				deleted = userProfileStat.deleted,
				userProfile = UserProfile.CreateMinimalAnonymous(userProfileStat.userProfile)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a UserProfileStat Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(UserProfileStat userProfileStat)
		{
			//
			// Return a very minimal object.
			//
			if (userProfileStat == null)
			{
				return null;
			}

			return new {
				id = userProfileStat.id,
				name = userProfileStat.id,
				description = userProfileStat.id
			 };
		}
	}
}
