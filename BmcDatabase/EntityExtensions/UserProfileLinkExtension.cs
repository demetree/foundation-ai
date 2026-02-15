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
	public partial class UserProfileLink : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class UserProfileLinkDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 userProfileId { get; set; }
			[Required]
			public Int32 userProfileLinkTypeId { get; set; }
			[Required]
			public String url { get; set; }
			public String displayLabel { get; set; }
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
		public class UserProfileLinkOutputDTO : UserProfileLinkDTO
		{
			public UserProfile.UserProfileDTO userProfile { get; set; }
			public UserProfileLinkType.UserProfileLinkTypeDTO userProfileLinkType { get; set; }
		}


		/// <summary>
		///
		/// Converts a UserProfileLink to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public UserProfileLinkDTO ToDTO()
		{
			return new UserProfileLinkDTO
			{
				id = this.id,
				userProfileId = this.userProfileId,
				userProfileLinkTypeId = this.userProfileLinkTypeId,
				url = this.url,
				displayLabel = this.displayLabel,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a UserProfileLink list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<UserProfileLinkDTO> ToDTOList(List<UserProfileLink> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserProfileLinkDTO> output = new List<UserProfileLinkDTO>();

			output.Capacity = data.Count;

			foreach (UserProfileLink userProfileLink in data)
			{
				output.Add(userProfileLink.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a UserProfileLink to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the UserProfileLinkEntity type directly.
		///
		/// </summary>
		public UserProfileLinkOutputDTO ToOutputDTO()
		{
			return new UserProfileLinkOutputDTO
			{
				id = this.id,
				userProfileId = this.userProfileId,
				userProfileLinkTypeId = this.userProfileLinkTypeId,
				url = this.url,
				displayLabel = this.displayLabel,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				userProfile = this.userProfile?.ToDTO(),
				userProfileLinkType = this.userProfileLinkType?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a UserProfileLink list to list of Output Data Transfer Object intended to be used for serializing a list of UserProfileLink objects to avoid using the UserProfileLink entity type directly.
		///
		/// </summary>
		public static List<UserProfileLinkOutputDTO> ToOutputDTOList(List<UserProfileLink> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserProfileLinkOutputDTO> output = new List<UserProfileLinkOutputDTO>();

			output.Capacity = data.Count;

			foreach (UserProfileLink userProfileLink in data)
			{
				output.Add(userProfileLink.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a UserProfileLink Object.
		///
		/// </summary>
		public static Database.UserProfileLink FromDTO(UserProfileLinkDTO dto)
		{
			return new Database.UserProfileLink
			{
				id = dto.id,
				userProfileId = dto.userProfileId,
				userProfileLinkTypeId = dto.userProfileLinkTypeId,
				url = dto.url,
				displayLabel = dto.displayLabel,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a UserProfileLink Object.
		///
		/// </summary>
		public void ApplyDTO(UserProfileLinkDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.userProfileId = dto.userProfileId;
			this.userProfileLinkTypeId = dto.userProfileLinkTypeId;
			this.url = dto.url;
			this.displayLabel = dto.displayLabel;
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
		/// Creates a deep copy clone of a UserProfileLink Object.
		///
		/// </summary>
		public UserProfileLink Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new UserProfileLink{
				id = this.id,
				tenantGuid = this.tenantGuid,
				userProfileId = this.userProfileId,
				userProfileLinkTypeId = this.userProfileLinkTypeId,
				url = this.url,
				displayLabel = this.displayLabel,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserProfileLink Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserProfileLink Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a UserProfileLink Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a UserProfileLink Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.UserProfileLink userProfileLink)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (userProfileLink == null)
			{
				return null;
			}

			return new {
				id = userProfileLink.id,
				userProfileId = userProfileLink.userProfileId,
				userProfileLinkTypeId = userProfileLink.userProfileLinkTypeId,
				url = userProfileLink.url,
				displayLabel = userProfileLink.displayLabel,
				sequence = userProfileLink.sequence,
				objectGuid = userProfileLink.objectGuid,
				active = userProfileLink.active,
				deleted = userProfileLink.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a UserProfileLink Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(UserProfileLink userProfileLink)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (userProfileLink == null)
			{
				return null;
			}

			return new {
				id = userProfileLink.id,
				userProfileId = userProfileLink.userProfileId,
				userProfileLinkTypeId = userProfileLink.userProfileLinkTypeId,
				url = userProfileLink.url,
				displayLabel = userProfileLink.displayLabel,
				sequence = userProfileLink.sequence,
				objectGuid = userProfileLink.objectGuid,
				active = userProfileLink.active,
				deleted = userProfileLink.deleted,
				userProfile = UserProfile.CreateMinimalAnonymous(userProfileLink.userProfile),
				userProfileLinkType = UserProfileLinkType.CreateMinimalAnonymous(userProfileLink.userProfileLinkType)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a UserProfileLink Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(UserProfileLink userProfileLink)
		{
			//
			// Return a very minimal object.
			//
			if (userProfileLink == null)
			{
				return null;
			}

			return new {
				id = userProfileLink.id,
				name = userProfileLink.url,
				description = string.Join(", ", new[] { userProfileLink.url, userProfileLink.displayLabel}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
