using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.BMC.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class UserProfileChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)userProfileId; }
			set { userProfileId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class UserProfileChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 userProfileId { get; set; }
			public Int32 versionNumber { get; set; }
			[Required]
			public DateTime timeStamp { get; set; }
			[Required]
			public Int32 userId { get; set; }
			[Required]
			public String data { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class UserProfileChangeHistoryOutputDTO : UserProfileChangeHistoryDTO
		{
			public UserProfile.UserProfileDTO userProfile { get; set; }
		}


		/// <summary>
		///
		/// Converts a UserProfileChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public UserProfileChangeHistoryDTO ToDTO()
		{
			return new UserProfileChangeHistoryDTO
			{
				id = this.id,
				userProfileId = this.userProfileId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a UserProfileChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<UserProfileChangeHistoryDTO> ToDTOList(List<UserProfileChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserProfileChangeHistoryDTO> output = new List<UserProfileChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (UserProfileChangeHistory userProfileChangeHistory in data)
			{
				output.Add(userProfileChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a UserProfileChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the UserProfileChangeHistoryEntity type directly.
		///
		/// </summary>
		public UserProfileChangeHistoryOutputDTO ToOutputDTO()
		{
			return new UserProfileChangeHistoryOutputDTO
			{
				id = this.id,
				userProfileId = this.userProfileId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				userProfile = this.userProfile?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a UserProfileChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of UserProfileChangeHistory objects to avoid using the UserProfileChangeHistory entity type directly.
		///
		/// </summary>
		public static List<UserProfileChangeHistoryOutputDTO> ToOutputDTOList(List<UserProfileChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserProfileChangeHistoryOutputDTO> output = new List<UserProfileChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (UserProfileChangeHistory userProfileChangeHistory in data)
			{
				output.Add(userProfileChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a UserProfileChangeHistory Object.
		///
		/// </summary>
		public static Database.UserProfileChangeHistory FromDTO(UserProfileChangeHistoryDTO dto)
		{
			return new Database.UserProfileChangeHistory
			{
				id = dto.id,
				userProfileId = dto.userProfileId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a UserProfileChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(UserProfileChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.userProfileId = dto.userProfileId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a UserProfileChangeHistory Object.
		///
		/// </summary>
		public UserProfileChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new UserProfileChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				userProfileId = this.userProfileId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserProfileChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserProfileChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a UserProfileChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a UserProfileChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.UserProfileChangeHistory userProfileChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (userProfileChangeHistory == null)
			{
				return null;
			}

			return new {
				id = userProfileChangeHistory.id,
				userProfileId = userProfileChangeHistory.userProfileId,
				versionNumber = userProfileChangeHistory.versionNumber,
				timeStamp = userProfileChangeHistory.timeStamp,
				userId = userProfileChangeHistory.userId,
				data = userProfileChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a UserProfileChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(UserProfileChangeHistory userProfileChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (userProfileChangeHistory == null)
			{
				return null;
			}

			return new {
				id = userProfileChangeHistory.id,
				userProfileId = userProfileChangeHistory.userProfileId,
				versionNumber = userProfileChangeHistory.versionNumber,
				timeStamp = userProfileChangeHistory.timeStamp,
				userId = userProfileChangeHistory.userId,
				data = userProfileChangeHistory.data,
				userProfile = UserProfile.CreateMinimalAnonymous(userProfileChangeHistory.userProfile)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a UserProfileChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(UserProfileChangeHistory userProfileChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (userProfileChangeHistory == null)
			{
				return null;
			}

			return new {
				id = userProfileChangeHistory.id,
				name = userProfileChangeHistory.id,
				description = userProfileChangeHistory.id
			 };
		}
	}
}
