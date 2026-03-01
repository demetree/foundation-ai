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
	public partial class UserSetListChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)userSetListId; }
			set { userSetListId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class UserSetListChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 userSetListId { get; set; }
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
		public class UserSetListChangeHistoryOutputDTO : UserSetListChangeHistoryDTO
		{
			public UserSetList.UserSetListDTO userSetList { get; set; }
		}


		/// <summary>
		///
		/// Converts a UserSetListChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public UserSetListChangeHistoryDTO ToDTO()
		{
			return new UserSetListChangeHistoryDTO
			{
				id = this.id,
				userSetListId = this.userSetListId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a UserSetListChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<UserSetListChangeHistoryDTO> ToDTOList(List<UserSetListChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserSetListChangeHistoryDTO> output = new List<UserSetListChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (UserSetListChangeHistory userSetListChangeHistory in data)
			{
				output.Add(userSetListChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a UserSetListChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the UserSetListChangeHistoryEntity type directly.
		///
		/// </summary>
		public UserSetListChangeHistoryOutputDTO ToOutputDTO()
		{
			return new UserSetListChangeHistoryOutputDTO
			{
				id = this.id,
				userSetListId = this.userSetListId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				userSetList = this.userSetList?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a UserSetListChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of UserSetListChangeHistory objects to avoid using the UserSetListChangeHistory entity type directly.
		///
		/// </summary>
		public static List<UserSetListChangeHistoryOutputDTO> ToOutputDTOList(List<UserSetListChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserSetListChangeHistoryOutputDTO> output = new List<UserSetListChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (UserSetListChangeHistory userSetListChangeHistory in data)
			{
				output.Add(userSetListChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a UserSetListChangeHistory Object.
		///
		/// </summary>
		public static Database.UserSetListChangeHistory FromDTO(UserSetListChangeHistoryDTO dto)
		{
			return new Database.UserSetListChangeHistory
			{
				id = dto.id,
				userSetListId = dto.userSetListId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a UserSetListChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(UserSetListChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.userSetListId = dto.userSetListId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a UserSetListChangeHistory Object.
		///
		/// </summary>
		public UserSetListChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new UserSetListChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				userSetListId = this.userSetListId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserSetListChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserSetListChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a UserSetListChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a UserSetListChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.UserSetListChangeHistory userSetListChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (userSetListChangeHistory == null)
			{
				return null;
			}

			return new {
				id = userSetListChangeHistory.id,
				userSetListId = userSetListChangeHistory.userSetListId,
				versionNumber = userSetListChangeHistory.versionNumber,
				timeStamp = userSetListChangeHistory.timeStamp,
				userId = userSetListChangeHistory.userId,
				data = userSetListChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a UserSetListChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(UserSetListChangeHistory userSetListChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (userSetListChangeHistory == null)
			{
				return null;
			}

			return new {
				id = userSetListChangeHistory.id,
				userSetListId = userSetListChangeHistory.userSetListId,
				versionNumber = userSetListChangeHistory.versionNumber,
				timeStamp = userSetListChangeHistory.timeStamp,
				userId = userSetListChangeHistory.userId,
				data = userSetListChangeHistory.data,
				userSetList = UserSetList.CreateMinimalAnonymous(userSetListChangeHistory.userSetList),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a UserSetListChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(UserSetListChangeHistory userSetListChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (userSetListChangeHistory == null)
			{
				return null;
			}

			return new {
				id = userSetListChangeHistory.id,
				name = userSetListChangeHistory.id,
				description = userSetListChangeHistory.id
			 };
		}
	}
}
