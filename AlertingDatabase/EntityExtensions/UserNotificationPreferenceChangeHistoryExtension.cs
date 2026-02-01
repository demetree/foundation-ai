using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.Alerting.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class UserNotificationPreferenceChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)userNotificationPreferenceId; }
			set { userNotificationPreferenceId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class UserNotificationPreferenceChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 userNotificationPreferenceId { get; set; }
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
		public class UserNotificationPreferenceChangeHistoryOutputDTO : UserNotificationPreferenceChangeHistoryDTO
		{
			public UserNotificationPreference.UserNotificationPreferenceDTO userNotificationPreference { get; set; }
		}


		/// <summary>
		///
		/// Converts a UserNotificationPreferenceChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public UserNotificationPreferenceChangeHistoryDTO ToDTO()
		{
			return new UserNotificationPreferenceChangeHistoryDTO
			{
				id = this.id,
				userNotificationPreferenceId = this.userNotificationPreferenceId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a UserNotificationPreferenceChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<UserNotificationPreferenceChangeHistoryDTO> ToDTOList(List<UserNotificationPreferenceChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserNotificationPreferenceChangeHistoryDTO> output = new List<UserNotificationPreferenceChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (UserNotificationPreferenceChangeHistory userNotificationPreferenceChangeHistory in data)
			{
				output.Add(userNotificationPreferenceChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a UserNotificationPreferenceChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the UserNotificationPreferenceChangeHistoryEntity type directly.
		///
		/// </summary>
		public UserNotificationPreferenceChangeHistoryOutputDTO ToOutputDTO()
		{
			return new UserNotificationPreferenceChangeHistoryOutputDTO
			{
				id = this.id,
				userNotificationPreferenceId = this.userNotificationPreferenceId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				userNotificationPreference = this.userNotificationPreference?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a UserNotificationPreferenceChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of UserNotificationPreferenceChangeHistory objects to avoid using the UserNotificationPreferenceChangeHistory entity type directly.
		///
		/// </summary>
		public static List<UserNotificationPreferenceChangeHistoryOutputDTO> ToOutputDTOList(List<UserNotificationPreferenceChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserNotificationPreferenceChangeHistoryOutputDTO> output = new List<UserNotificationPreferenceChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (UserNotificationPreferenceChangeHistory userNotificationPreferenceChangeHistory in data)
			{
				output.Add(userNotificationPreferenceChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a UserNotificationPreferenceChangeHistory Object.
		///
		/// </summary>
		public static Database.UserNotificationPreferenceChangeHistory FromDTO(UserNotificationPreferenceChangeHistoryDTO dto)
		{
			return new Database.UserNotificationPreferenceChangeHistory
			{
				id = dto.id,
				userNotificationPreferenceId = dto.userNotificationPreferenceId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a UserNotificationPreferenceChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(UserNotificationPreferenceChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.userNotificationPreferenceId = dto.userNotificationPreferenceId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a UserNotificationPreferenceChangeHistory Object.
		///
		/// </summary>
		public UserNotificationPreferenceChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new UserNotificationPreferenceChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				userNotificationPreferenceId = this.userNotificationPreferenceId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserNotificationPreferenceChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserNotificationPreferenceChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a UserNotificationPreferenceChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a UserNotificationPreferenceChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.UserNotificationPreferenceChangeHistory userNotificationPreferenceChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (userNotificationPreferenceChangeHistory == null)
			{
				return null;
			}

			return new {
				id = userNotificationPreferenceChangeHistory.id,
				userNotificationPreferenceId = userNotificationPreferenceChangeHistory.userNotificationPreferenceId,
				versionNumber = userNotificationPreferenceChangeHistory.versionNumber,
				timeStamp = userNotificationPreferenceChangeHistory.timeStamp,
				userId = userNotificationPreferenceChangeHistory.userId,
				data = userNotificationPreferenceChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a UserNotificationPreferenceChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(UserNotificationPreferenceChangeHistory userNotificationPreferenceChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (userNotificationPreferenceChangeHistory == null)
			{
				return null;
			}

			return new {
				id = userNotificationPreferenceChangeHistory.id,
				userNotificationPreferenceId = userNotificationPreferenceChangeHistory.userNotificationPreferenceId,
				versionNumber = userNotificationPreferenceChangeHistory.versionNumber,
				timeStamp = userNotificationPreferenceChangeHistory.timeStamp,
				userId = userNotificationPreferenceChangeHistory.userId,
				data = userNotificationPreferenceChangeHistory.data,
				userNotificationPreference = UserNotificationPreference.CreateMinimalAnonymous(userNotificationPreferenceChangeHistory.userNotificationPreference),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a UserNotificationPreferenceChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(UserNotificationPreferenceChangeHistory userNotificationPreferenceChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (userNotificationPreferenceChangeHistory == null)
			{
				return null;
			}

			return new {
				id = userNotificationPreferenceChangeHistory.id,
				name = userNotificationPreferenceChangeHistory.id,
				description = userNotificationPreferenceChangeHistory.id
			 };
		}
	}
}
