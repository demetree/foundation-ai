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
	public partial class UserSetListItem : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class UserSetListItemDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 userSetListId { get; set; }
			[Required]
			public Int32 legoSetId { get; set; }
			[Required]
			public Int32 quantity { get; set; }
			[Required]
			public Boolean includeSpares { get; set; }
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
		public class UserSetListItemOutputDTO : UserSetListItemDTO
		{
			public LegoSet.LegoSetDTO legoSet { get; set; }
			public UserSetList.UserSetListDTO userSetList { get; set; }
		}


		/// <summary>
		///
		/// Converts a UserSetListItem to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public UserSetListItemDTO ToDTO()
		{
			return new UserSetListItemDTO
			{
				id = this.id,
				userSetListId = this.userSetListId,
				legoSetId = this.legoSetId,
				quantity = this.quantity,
				includeSpares = this.includeSpares,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a UserSetListItem list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<UserSetListItemDTO> ToDTOList(List<UserSetListItem> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserSetListItemDTO> output = new List<UserSetListItemDTO>();

			output.Capacity = data.Count;

			foreach (UserSetListItem userSetListItem in data)
			{
				output.Add(userSetListItem.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a UserSetListItem to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the UserSetListItemEntity type directly.
		///
		/// </summary>
		public UserSetListItemOutputDTO ToOutputDTO()
		{
			return new UserSetListItemOutputDTO
			{
				id = this.id,
				userSetListId = this.userSetListId,
				legoSetId = this.legoSetId,
				quantity = this.quantity,
				includeSpares = this.includeSpares,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				legoSet = this.legoSet?.ToDTO(),
				userSetList = this.userSetList?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a UserSetListItem list to list of Output Data Transfer Object intended to be used for serializing a list of UserSetListItem objects to avoid using the UserSetListItem entity type directly.
		///
		/// </summary>
		public static List<UserSetListItemOutputDTO> ToOutputDTOList(List<UserSetListItem> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserSetListItemOutputDTO> output = new List<UserSetListItemOutputDTO>();

			output.Capacity = data.Count;

			foreach (UserSetListItem userSetListItem in data)
			{
				output.Add(userSetListItem.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a UserSetListItem Object.
		///
		/// </summary>
		public static Database.UserSetListItem FromDTO(UserSetListItemDTO dto)
		{
			return new Database.UserSetListItem
			{
				id = dto.id,
				userSetListId = dto.userSetListId,
				legoSetId = dto.legoSetId,
				quantity = dto.quantity,
				includeSpares = dto.includeSpares,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a UserSetListItem Object.
		///
		/// </summary>
		public void ApplyDTO(UserSetListItemDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.userSetListId = dto.userSetListId;
			this.legoSetId = dto.legoSetId;
			this.quantity = dto.quantity;
			this.includeSpares = dto.includeSpares;
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
		/// Creates a deep copy clone of a UserSetListItem Object.
		///
		/// </summary>
		public UserSetListItem Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new UserSetListItem{
				id = this.id,
				tenantGuid = this.tenantGuid,
				userSetListId = this.userSetListId,
				legoSetId = this.legoSetId,
				quantity = this.quantity,
				includeSpares = this.includeSpares,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserSetListItem Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserSetListItem Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a UserSetListItem Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a UserSetListItem Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.UserSetListItem userSetListItem)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (userSetListItem == null)
			{
				return null;
			}

			return new {
				id = userSetListItem.id,
				userSetListId = userSetListItem.userSetListId,
				legoSetId = userSetListItem.legoSetId,
				quantity = userSetListItem.quantity,
				includeSpares = userSetListItem.includeSpares,
				objectGuid = userSetListItem.objectGuid,
				active = userSetListItem.active,
				deleted = userSetListItem.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a UserSetListItem Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(UserSetListItem userSetListItem)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (userSetListItem == null)
			{
				return null;
			}

			return new {
				id = userSetListItem.id,
				userSetListId = userSetListItem.userSetListId,
				legoSetId = userSetListItem.legoSetId,
				quantity = userSetListItem.quantity,
				includeSpares = userSetListItem.includeSpares,
				objectGuid = userSetListItem.objectGuid,
				active = userSetListItem.active,
				deleted = userSetListItem.deleted,
				legoSet = LegoSet.CreateMinimalAnonymous(userSetListItem.legoSet),
				userSetList = UserSetList.CreateMinimalAnonymous(userSetListItem.userSetList)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a UserSetListItem Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(UserSetListItem userSetListItem)
		{
			//
			// Return a very minimal object.
			//
			if (userSetListItem == null)
			{
				return null;
			}

			return new {
				id = userSetListItem.id,
				name = userSetListItem.id,
				description = userSetListItem.id
			 };
		}
	}
}
