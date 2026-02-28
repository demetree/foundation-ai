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
	public partial class UserPartListItem : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class UserPartListItemDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 userPartListId { get; set; }
			[Required]
			public Int32 brickPartId { get; set; }
			[Required]
			public Int32 brickColourId { get; set; }
			[Required]
			public Int32 quantity { get; set; }
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
		public class UserPartListItemOutputDTO : UserPartListItemDTO
		{
			public BrickColour.BrickColourDTO brickColour { get; set; }
			public BrickPart.BrickPartDTO brickPart { get; set; }
			public UserPartList.UserPartListDTO userPartList { get; set; }
		}


		/// <summary>
		///
		/// Converts a UserPartListItem to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public UserPartListItemDTO ToDTO()
		{
			return new UserPartListItemDTO
			{
				id = this.id,
				userPartListId = this.userPartListId,
				brickPartId = this.brickPartId,
				brickColourId = this.brickColourId,
				quantity = this.quantity,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a UserPartListItem list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<UserPartListItemDTO> ToDTOList(List<UserPartListItem> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserPartListItemDTO> output = new List<UserPartListItemDTO>();

			output.Capacity = data.Count;

			foreach (UserPartListItem userPartListItem in data)
			{
				output.Add(userPartListItem.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a UserPartListItem to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the UserPartListItemEntity type directly.
		///
		/// </summary>
		public UserPartListItemOutputDTO ToOutputDTO()
		{
			return new UserPartListItemOutputDTO
			{
				id = this.id,
				userPartListId = this.userPartListId,
				brickPartId = this.brickPartId,
				brickColourId = this.brickColourId,
				quantity = this.quantity,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				brickColour = this.brickColour?.ToDTO(),
				brickPart = this.brickPart?.ToDTO(),
				userPartList = this.userPartList?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a UserPartListItem list to list of Output Data Transfer Object intended to be used for serializing a list of UserPartListItem objects to avoid using the UserPartListItem entity type directly.
		///
		/// </summary>
		public static List<UserPartListItemOutputDTO> ToOutputDTOList(List<UserPartListItem> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserPartListItemOutputDTO> output = new List<UserPartListItemOutputDTO>();

			output.Capacity = data.Count;

			foreach (UserPartListItem userPartListItem in data)
			{
				output.Add(userPartListItem.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a UserPartListItem Object.
		///
		/// </summary>
		public static Database.UserPartListItem FromDTO(UserPartListItemDTO dto)
		{
			return new Database.UserPartListItem
			{
				id = dto.id,
				userPartListId = dto.userPartListId,
				brickPartId = dto.brickPartId,
				brickColourId = dto.brickColourId,
				quantity = dto.quantity,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a UserPartListItem Object.
		///
		/// </summary>
		public void ApplyDTO(UserPartListItemDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.userPartListId = dto.userPartListId;
			this.brickPartId = dto.brickPartId;
			this.brickColourId = dto.brickColourId;
			this.quantity = dto.quantity;
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
		/// Creates a deep copy clone of a UserPartListItem Object.
		///
		/// </summary>
		public UserPartListItem Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new UserPartListItem{
				id = this.id,
				tenantGuid = this.tenantGuid,
				userPartListId = this.userPartListId,
				brickPartId = this.brickPartId,
				brickColourId = this.brickColourId,
				quantity = this.quantity,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserPartListItem Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserPartListItem Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a UserPartListItem Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a UserPartListItem Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.UserPartListItem userPartListItem)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (userPartListItem == null)
			{
				return null;
			}

			return new {
				id = userPartListItem.id,
				userPartListId = userPartListItem.userPartListId,
				brickPartId = userPartListItem.brickPartId,
				brickColourId = userPartListItem.brickColourId,
				quantity = userPartListItem.quantity,
				objectGuid = userPartListItem.objectGuid,
				active = userPartListItem.active,
				deleted = userPartListItem.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a UserPartListItem Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(UserPartListItem userPartListItem)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (userPartListItem == null)
			{
				return null;
			}

			return new {
				id = userPartListItem.id,
				userPartListId = userPartListItem.userPartListId,
				brickPartId = userPartListItem.brickPartId,
				brickColourId = userPartListItem.brickColourId,
				quantity = userPartListItem.quantity,
				objectGuid = userPartListItem.objectGuid,
				active = userPartListItem.active,
				deleted = userPartListItem.deleted,
				brickColour = BrickColour.CreateMinimalAnonymous(userPartListItem.brickColour),
				brickPart = BrickPart.CreateMinimalAnonymous(userPartListItem.brickPart),
				userPartList = UserPartList.CreateMinimalAnonymous(userPartListItem.userPartList)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a UserPartListItem Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(UserPartListItem userPartListItem)
		{
			//
			// Return a very minimal object.
			//
			if (userPartListItem == null)
			{
				return null;
			}

			return new {
				id = userPartListItem.id,
				name = userPartListItem.id,
				description = userPartListItem.id
			 };
		}
	}
}
