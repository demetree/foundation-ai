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
	public partial class UserWishlistItem : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class UserWishlistItemDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 userCollectionId { get; set; }
			[Required]
			public Int32 brickPartId { get; set; }
			public Int32? brickColourId { get; set; }
			public Int32? quantityDesired { get; set; }
			public String notes { get; set; }
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
		public class UserWishlistItemOutputDTO : UserWishlistItemDTO
		{
			public BrickColour.BrickColourDTO brickColour { get; set; }
			public BrickPart.BrickPartDTO brickPart { get; set; }
			public UserCollection.UserCollectionDTO userCollection { get; set; }
		}


		/// <summary>
		///
		/// Converts a UserWishlistItem to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public UserWishlistItemDTO ToDTO()
		{
			return new UserWishlistItemDTO
			{
				id = this.id,
				userCollectionId = this.userCollectionId,
				brickPartId = this.brickPartId,
				brickColourId = this.brickColourId,
				quantityDesired = this.quantityDesired,
				notes = this.notes,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a UserWishlistItem list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<UserWishlistItemDTO> ToDTOList(List<UserWishlistItem> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserWishlistItemDTO> output = new List<UserWishlistItemDTO>();

			output.Capacity = data.Count;

			foreach (UserWishlistItem userWishlistItem in data)
			{
				output.Add(userWishlistItem.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a UserWishlistItem to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the UserWishlistItemEntity type directly.
		///
		/// </summary>
		public UserWishlistItemOutputDTO ToOutputDTO()
		{
			return new UserWishlistItemOutputDTO
			{
				id = this.id,
				userCollectionId = this.userCollectionId,
				brickPartId = this.brickPartId,
				brickColourId = this.brickColourId,
				quantityDesired = this.quantityDesired,
				notes = this.notes,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				brickColour = this.brickColour?.ToDTO(),
				brickPart = this.brickPart?.ToDTO(),
				userCollection = this.userCollection?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a UserWishlistItem list to list of Output Data Transfer Object intended to be used for serializing a list of UserWishlistItem objects to avoid using the UserWishlistItem entity type directly.
		///
		/// </summary>
		public static List<UserWishlistItemOutputDTO> ToOutputDTOList(List<UserWishlistItem> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserWishlistItemOutputDTO> output = new List<UserWishlistItemOutputDTO>();

			output.Capacity = data.Count;

			foreach (UserWishlistItem userWishlistItem in data)
			{
				output.Add(userWishlistItem.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a UserWishlistItem Object.
		///
		/// </summary>
		public static Database.UserWishlistItem FromDTO(UserWishlistItemDTO dto)
		{
			return new Database.UserWishlistItem
			{
				id = dto.id,
				userCollectionId = dto.userCollectionId,
				brickPartId = dto.brickPartId,
				brickColourId = dto.brickColourId,
				quantityDesired = dto.quantityDesired,
				notes = dto.notes,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a UserWishlistItem Object.
		///
		/// </summary>
		public void ApplyDTO(UserWishlistItemDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.userCollectionId = dto.userCollectionId;
			this.brickPartId = dto.brickPartId;
			this.brickColourId = dto.brickColourId;
			this.quantityDesired = dto.quantityDesired;
			this.notes = dto.notes;
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
		/// Creates a deep copy clone of a UserWishlistItem Object.
		///
		/// </summary>
		public UserWishlistItem Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new UserWishlistItem{
				id = this.id,
				tenantGuid = this.tenantGuid,
				userCollectionId = this.userCollectionId,
				brickPartId = this.brickPartId,
				brickColourId = this.brickColourId,
				quantityDesired = this.quantityDesired,
				notes = this.notes,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserWishlistItem Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserWishlistItem Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a UserWishlistItem Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a UserWishlistItem Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.UserWishlistItem userWishlistItem)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (userWishlistItem == null)
			{
				return null;
			}

			return new {
				id = userWishlistItem.id,
				userCollectionId = userWishlistItem.userCollectionId,
				brickPartId = userWishlistItem.brickPartId,
				brickColourId = userWishlistItem.brickColourId,
				quantityDesired = userWishlistItem.quantityDesired,
				notes = userWishlistItem.notes,
				objectGuid = userWishlistItem.objectGuid,
				active = userWishlistItem.active,
				deleted = userWishlistItem.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a UserWishlistItem Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(UserWishlistItem userWishlistItem)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (userWishlistItem == null)
			{
				return null;
			}

			return new {
				id = userWishlistItem.id,
				userCollectionId = userWishlistItem.userCollectionId,
				brickPartId = userWishlistItem.brickPartId,
				brickColourId = userWishlistItem.brickColourId,
				quantityDesired = userWishlistItem.quantityDesired,
				notes = userWishlistItem.notes,
				objectGuid = userWishlistItem.objectGuid,
				active = userWishlistItem.active,
				deleted = userWishlistItem.deleted,
				brickColour = BrickColour.CreateMinimalAnonymous(userWishlistItem.brickColour),
				brickPart = BrickPart.CreateMinimalAnonymous(userWishlistItem.brickPart),
				userCollection = UserCollection.CreateMinimalAnonymous(userWishlistItem.userCollection)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a UserWishlistItem Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(UserWishlistItem userWishlistItem)
		{
			//
			// Return a very minimal object.
			//
			if (userWishlistItem == null)
			{
				return null;
			}

			return new {
				id = userWishlistItem.id,
				name = userWishlistItem.id,
				description = userWishlistItem.id
			 };
		}
	}
}
