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
	public partial class UserPartListChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)userPartListId; }
			set { userPartListId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class UserPartListChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 userPartListId { get; set; }
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
		public class UserPartListChangeHistoryOutputDTO : UserPartListChangeHistoryDTO
		{
			public UserPartList.UserPartListDTO userPartList { get; set; }
		}


		/// <summary>
		///
		/// Converts a UserPartListChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public UserPartListChangeHistoryDTO ToDTO()
		{
			return new UserPartListChangeHistoryDTO
			{
				id = this.id,
				userPartListId = this.userPartListId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a UserPartListChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<UserPartListChangeHistoryDTO> ToDTOList(List<UserPartListChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserPartListChangeHistoryDTO> output = new List<UserPartListChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (UserPartListChangeHistory userPartListChangeHistory in data)
			{
				output.Add(userPartListChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a UserPartListChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the UserPartListChangeHistoryEntity type directly.
		///
		/// </summary>
		public UserPartListChangeHistoryOutputDTO ToOutputDTO()
		{
			return new UserPartListChangeHistoryOutputDTO
			{
				id = this.id,
				userPartListId = this.userPartListId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				userPartList = this.userPartList?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a UserPartListChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of UserPartListChangeHistory objects to avoid using the UserPartListChangeHistory entity type directly.
		///
		/// </summary>
		public static List<UserPartListChangeHistoryOutputDTO> ToOutputDTOList(List<UserPartListChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserPartListChangeHistoryOutputDTO> output = new List<UserPartListChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (UserPartListChangeHistory userPartListChangeHistory in data)
			{
				output.Add(userPartListChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a UserPartListChangeHistory Object.
		///
		/// </summary>
		public static Database.UserPartListChangeHistory FromDTO(UserPartListChangeHistoryDTO dto)
		{
			return new Database.UserPartListChangeHistory
			{
				id = dto.id,
				userPartListId = dto.userPartListId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a UserPartListChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(UserPartListChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.userPartListId = dto.userPartListId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a UserPartListChangeHistory Object.
		///
		/// </summary>
		public UserPartListChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new UserPartListChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				userPartListId = this.userPartListId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserPartListChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserPartListChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a UserPartListChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a UserPartListChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.UserPartListChangeHistory userPartListChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (userPartListChangeHistory == null)
			{
				return null;
			}

			return new {
				id = userPartListChangeHistory.id,
				userPartListId = userPartListChangeHistory.userPartListId,
				versionNumber = userPartListChangeHistory.versionNumber,
				timeStamp = userPartListChangeHistory.timeStamp,
				userId = userPartListChangeHistory.userId,
				data = userPartListChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a UserPartListChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(UserPartListChangeHistory userPartListChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (userPartListChangeHistory == null)
			{
				return null;
			}

			return new {
				id = userPartListChangeHistory.id,
				userPartListId = userPartListChangeHistory.userPartListId,
				versionNumber = userPartListChangeHistory.versionNumber,
				timeStamp = userPartListChangeHistory.timeStamp,
				userId = userPartListChangeHistory.userId,
				data = userPartListChangeHistory.data,
				userPartList = UserPartList.CreateMinimalAnonymous(userPartListChangeHistory.userPartList)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a UserPartListChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(UserPartListChangeHistory userPartListChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (userPartListChangeHistory == null)
			{
				return null;
			}

			return new {
				id = userPartListChangeHistory.id,
				name = userPartListChangeHistory.id,
				description = userPartListChangeHistory.id
			 };
		}
	}
}
