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
	public partial class UserNotificationChannelPreferenceChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)userNotificationChannelPreferenceId; }
			set { userNotificationChannelPreferenceId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class UserNotificationChannelPreferenceChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 userNotificationChannelPreferenceId { get; set; }
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
		public class UserNotificationChannelPreferenceChangeHistoryOutputDTO : UserNotificationChannelPreferenceChangeHistoryDTO
		{
			public UserNotificationChannelPreference.UserNotificationChannelPreferenceDTO userNotificationChannelPreference { get; set; }
		}


		/// <summary>
		///
		/// Converts a UserNotificationChannelPreferenceChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public UserNotificationChannelPreferenceChangeHistoryDTO ToDTO()
		{
			return new UserNotificationChannelPreferenceChangeHistoryDTO
			{
				id = this.id,
				userNotificationChannelPreferenceId = this.userNotificationChannelPreferenceId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a UserNotificationChannelPreferenceChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<UserNotificationChannelPreferenceChangeHistoryDTO> ToDTOList(List<UserNotificationChannelPreferenceChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserNotificationChannelPreferenceChangeHistoryDTO> output = new List<UserNotificationChannelPreferenceChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (UserNotificationChannelPreferenceChangeHistory userNotificationChannelPreferenceChangeHistory in data)
			{
				output.Add(userNotificationChannelPreferenceChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a UserNotificationChannelPreferenceChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the UserNotificationChannelPreferenceChangeHistoryEntity type directly.
		///
		/// </summary>
		public UserNotificationChannelPreferenceChangeHistoryOutputDTO ToOutputDTO()
		{
			return new UserNotificationChannelPreferenceChangeHistoryOutputDTO
			{
				id = this.id,
				userNotificationChannelPreferenceId = this.userNotificationChannelPreferenceId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				userNotificationChannelPreference = this.userNotificationChannelPreference?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a UserNotificationChannelPreferenceChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of UserNotificationChannelPreferenceChangeHistory objects to avoid using the UserNotificationChannelPreferenceChangeHistory entity type directly.
		///
		/// </summary>
		public static List<UserNotificationChannelPreferenceChangeHistoryOutputDTO> ToOutputDTOList(List<UserNotificationChannelPreferenceChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserNotificationChannelPreferenceChangeHistoryOutputDTO> output = new List<UserNotificationChannelPreferenceChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (UserNotificationChannelPreferenceChangeHistory userNotificationChannelPreferenceChangeHistory in data)
			{
				output.Add(userNotificationChannelPreferenceChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a UserNotificationChannelPreferenceChangeHistory Object.
		///
		/// </summary>
		public static Database.UserNotificationChannelPreferenceChangeHistory FromDTO(UserNotificationChannelPreferenceChangeHistoryDTO dto)
		{
			return new Database.UserNotificationChannelPreferenceChangeHistory
			{
				id = dto.id,
				userNotificationChannelPreferenceId = dto.userNotificationChannelPreferenceId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a UserNotificationChannelPreferenceChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(UserNotificationChannelPreferenceChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.userNotificationChannelPreferenceId = dto.userNotificationChannelPreferenceId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a UserNotificationChannelPreferenceChangeHistory Object.
		///
		/// </summary>
		public UserNotificationChannelPreferenceChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new UserNotificationChannelPreferenceChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				userNotificationChannelPreferenceId = this.userNotificationChannelPreferenceId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserNotificationChannelPreferenceChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserNotificationChannelPreferenceChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a UserNotificationChannelPreferenceChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a UserNotificationChannelPreferenceChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.UserNotificationChannelPreferenceChangeHistory userNotificationChannelPreferenceChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (userNotificationChannelPreferenceChangeHistory == null)
			{
				return null;
			}

			return new {
				id = userNotificationChannelPreferenceChangeHistory.id,
				userNotificationChannelPreferenceId = userNotificationChannelPreferenceChangeHistory.userNotificationChannelPreferenceId,
				versionNumber = userNotificationChannelPreferenceChangeHistory.versionNumber,
				timeStamp = userNotificationChannelPreferenceChangeHistory.timeStamp,
				userId = userNotificationChannelPreferenceChangeHistory.userId,
				data = userNotificationChannelPreferenceChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a UserNotificationChannelPreferenceChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(UserNotificationChannelPreferenceChangeHistory userNotificationChannelPreferenceChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (userNotificationChannelPreferenceChangeHistory == null)
			{
				return null;
			}

			return new {
				id = userNotificationChannelPreferenceChangeHistory.id,
				userNotificationChannelPreferenceId = userNotificationChannelPreferenceChangeHistory.userNotificationChannelPreferenceId,
				versionNumber = userNotificationChannelPreferenceChangeHistory.versionNumber,
				timeStamp = userNotificationChannelPreferenceChangeHistory.timeStamp,
				userId = userNotificationChannelPreferenceChangeHistory.userId,
				data = userNotificationChannelPreferenceChangeHistory.data,
				userNotificationChannelPreference = UserNotificationChannelPreference.CreateMinimalAnonymous(userNotificationChannelPreferenceChangeHistory.userNotificationChannelPreference)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a UserNotificationChannelPreferenceChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(UserNotificationChannelPreferenceChangeHistory userNotificationChannelPreferenceChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (userNotificationChannelPreferenceChangeHistory == null)
			{
				return null;
			}

			return new {
				id = userNotificationChannelPreferenceChangeHistory.id,
				name = userNotificationChannelPreferenceChangeHistory.id,
				description = userNotificationChannelPreferenceChangeHistory.id
			 };
		}
	}
}
