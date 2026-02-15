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
	public partial class UserFollow : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class UserFollowDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Guid followerTenantGuid { get; set; }
			[Required]
			public Guid followedTenantGuid { get; set; }
			[Required]
			public DateTime followedDate { get; set; }
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
		public class UserFollowOutputDTO : UserFollowDTO
		{
		}


		/// <summary>
		///
		/// Converts a UserFollow to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public UserFollowDTO ToDTO()
		{
			return new UserFollowDTO
			{
				id = this.id,
				followerTenantGuid = this.followerTenantGuid,
				followedTenantGuid = this.followedTenantGuid,
				followedDate = this.followedDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a UserFollow list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<UserFollowDTO> ToDTOList(List<UserFollow> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserFollowDTO> output = new List<UserFollowDTO>();

			output.Capacity = data.Count;

			foreach (UserFollow userFollow in data)
			{
				output.Add(userFollow.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a UserFollow to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the UserFollowEntity type directly.
		///
		/// </summary>
		public UserFollowOutputDTO ToOutputDTO()
		{
			return new UserFollowOutputDTO
			{
				id = this.id,
				followerTenantGuid = this.followerTenantGuid,
				followedTenantGuid = this.followedTenantGuid,
				followedDate = this.followedDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a UserFollow list to list of Output Data Transfer Object intended to be used for serializing a list of UserFollow objects to avoid using the UserFollow entity type directly.
		///
		/// </summary>
		public static List<UserFollowOutputDTO> ToOutputDTOList(List<UserFollow> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserFollowOutputDTO> output = new List<UserFollowOutputDTO>();

			output.Capacity = data.Count;

			foreach (UserFollow userFollow in data)
			{
				output.Add(userFollow.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a UserFollow Object.
		///
		/// </summary>
		public static Database.UserFollow FromDTO(UserFollowDTO dto)
		{
			return new Database.UserFollow
			{
				id = dto.id,
				followerTenantGuid = dto.followerTenantGuid,
				followedTenantGuid = dto.followedTenantGuid,
				followedDate = dto.followedDate,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a UserFollow Object.
		///
		/// </summary>
		public void ApplyDTO(UserFollowDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.followerTenantGuid = dto.followerTenantGuid;
			this.followedTenantGuid = dto.followedTenantGuid;
			this.followedDate = dto.followedDate;
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
		/// Creates a deep copy clone of a UserFollow Object.
		///
		/// </summary>
		public UserFollow Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new UserFollow{
				id = this.id,
				followerTenantGuid = this.followerTenantGuid,
				followedTenantGuid = this.followedTenantGuid,
				followedDate = this.followedDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserFollow Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserFollow Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a UserFollow Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a UserFollow Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.UserFollow userFollow)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (userFollow == null)
			{
				return null;
			}

			return new {
				id = userFollow.id,
				followerTenantGuid = userFollow.followerTenantGuid,
				followedTenantGuid = userFollow.followedTenantGuid,
				followedDate = userFollow.followedDate,
				objectGuid = userFollow.objectGuid,
				active = userFollow.active,
				deleted = userFollow.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a UserFollow Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(UserFollow userFollow)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (userFollow == null)
			{
				return null;
			}

			return new {
				id = userFollow.id,
				followerTenantGuid = userFollow.followerTenantGuid,
				followedTenantGuid = userFollow.followedTenantGuid,
				followedDate = userFollow.followedDate,
				objectGuid = userFollow.objectGuid,
				active = userFollow.active,
				deleted = userFollow.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a UserFollow Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(UserFollow userFollow)
		{
			//
			// Return a very minimal object.
			//
			if (userFollow == null)
			{
				return null;
			}

			return new {
				id = userFollow.id,
				name = userFollow.id,
				description = userFollow.id
			 };
		}
	}
}
