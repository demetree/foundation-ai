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
	public partial class UserCollectionChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)userCollectionId; }
			set { userCollectionId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class UserCollectionChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 userCollectionId { get; set; }
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
		public class UserCollectionChangeHistoryOutputDTO : UserCollectionChangeHistoryDTO
		{
			public UserCollection.UserCollectionDTO userCollection { get; set; }
		}


		/// <summary>
		///
		/// Converts a UserCollectionChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public UserCollectionChangeHistoryDTO ToDTO()
		{
			return new UserCollectionChangeHistoryDTO
			{
				id = this.id,
				userCollectionId = this.userCollectionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a UserCollectionChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<UserCollectionChangeHistoryDTO> ToDTOList(List<UserCollectionChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserCollectionChangeHistoryDTO> output = new List<UserCollectionChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (UserCollectionChangeHistory userCollectionChangeHistory in data)
			{
				output.Add(userCollectionChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a UserCollectionChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the UserCollectionChangeHistoryEntity type directly.
		///
		/// </summary>
		public UserCollectionChangeHistoryOutputDTO ToOutputDTO()
		{
			return new UserCollectionChangeHistoryOutputDTO
			{
				id = this.id,
				userCollectionId = this.userCollectionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				userCollection = this.userCollection?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a UserCollectionChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of UserCollectionChangeHistory objects to avoid using the UserCollectionChangeHistory entity type directly.
		///
		/// </summary>
		public static List<UserCollectionChangeHistoryOutputDTO> ToOutputDTOList(List<UserCollectionChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserCollectionChangeHistoryOutputDTO> output = new List<UserCollectionChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (UserCollectionChangeHistory userCollectionChangeHistory in data)
			{
				output.Add(userCollectionChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a UserCollectionChangeHistory Object.
		///
		/// </summary>
		public static Database.UserCollectionChangeHistory FromDTO(UserCollectionChangeHistoryDTO dto)
		{
			return new Database.UserCollectionChangeHistory
			{
				id = dto.id,
				userCollectionId = dto.userCollectionId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a UserCollectionChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(UserCollectionChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.userCollectionId = dto.userCollectionId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a UserCollectionChangeHistory Object.
		///
		/// </summary>
		public UserCollectionChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new UserCollectionChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				userCollectionId = this.userCollectionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserCollectionChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserCollectionChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a UserCollectionChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a UserCollectionChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.UserCollectionChangeHistory userCollectionChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (userCollectionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = userCollectionChangeHistory.id,
				userCollectionId = userCollectionChangeHistory.userCollectionId,
				versionNumber = userCollectionChangeHistory.versionNumber,
				timeStamp = userCollectionChangeHistory.timeStamp,
				userId = userCollectionChangeHistory.userId,
				data = userCollectionChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a UserCollectionChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(UserCollectionChangeHistory userCollectionChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (userCollectionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = userCollectionChangeHistory.id,
				userCollectionId = userCollectionChangeHistory.userCollectionId,
				versionNumber = userCollectionChangeHistory.versionNumber,
				timeStamp = userCollectionChangeHistory.timeStamp,
				userId = userCollectionChangeHistory.userId,
				data = userCollectionChangeHistory.data,
				userCollection = UserCollection.CreateMinimalAnonymous(userCollectionChangeHistory.userCollection),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a UserCollectionChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(UserCollectionChangeHistory userCollectionChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (userCollectionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = userCollectionChangeHistory.id,
				name = userCollectionChangeHistory.id,
				description = userCollectionChangeHistory.id
			 };
		}
	}
}
