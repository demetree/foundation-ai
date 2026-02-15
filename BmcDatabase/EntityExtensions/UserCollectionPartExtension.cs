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
	public partial class UserCollectionPart : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class UserCollectionPartDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 userCollectionId { get; set; }
			[Required]
			public Int32 brickPartId { get; set; }
			[Required]
			public Int32 brickColourId { get; set; }
			public Int32? quantityOwned { get; set; }
			public Int32? quantityUsed { get; set; }
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
		public class UserCollectionPartOutputDTO : UserCollectionPartDTO
		{
			public BrickColour.BrickColourDTO brickColour { get; set; }
			public BrickPart.BrickPartDTO brickPart { get; set; }
			public UserCollection.UserCollectionDTO userCollection { get; set; }
		}


		/// <summary>
		///
		/// Converts a UserCollectionPart to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public UserCollectionPartDTO ToDTO()
		{
			return new UserCollectionPartDTO
			{
				id = this.id,
				userCollectionId = this.userCollectionId,
				brickPartId = this.brickPartId,
				brickColourId = this.brickColourId,
				quantityOwned = this.quantityOwned,
				quantityUsed = this.quantityUsed,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a UserCollectionPart list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<UserCollectionPartDTO> ToDTOList(List<UserCollectionPart> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserCollectionPartDTO> output = new List<UserCollectionPartDTO>();

			output.Capacity = data.Count;

			foreach (UserCollectionPart userCollectionPart in data)
			{
				output.Add(userCollectionPart.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a UserCollectionPart to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the UserCollectionPartEntity type directly.
		///
		/// </summary>
		public UserCollectionPartOutputDTO ToOutputDTO()
		{
			return new UserCollectionPartOutputDTO
			{
				id = this.id,
				userCollectionId = this.userCollectionId,
				brickPartId = this.brickPartId,
				brickColourId = this.brickColourId,
				quantityOwned = this.quantityOwned,
				quantityUsed = this.quantityUsed,
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
		/// Converts a UserCollectionPart list to list of Output Data Transfer Object intended to be used for serializing a list of UserCollectionPart objects to avoid using the UserCollectionPart entity type directly.
		///
		/// </summary>
		public static List<UserCollectionPartOutputDTO> ToOutputDTOList(List<UserCollectionPart> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserCollectionPartOutputDTO> output = new List<UserCollectionPartOutputDTO>();

			output.Capacity = data.Count;

			foreach (UserCollectionPart userCollectionPart in data)
			{
				output.Add(userCollectionPart.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a UserCollectionPart Object.
		///
		/// </summary>
		public static Database.UserCollectionPart FromDTO(UserCollectionPartDTO dto)
		{
			return new Database.UserCollectionPart
			{
				id = dto.id,
				userCollectionId = dto.userCollectionId,
				brickPartId = dto.brickPartId,
				brickColourId = dto.brickColourId,
				quantityOwned = dto.quantityOwned,
				quantityUsed = dto.quantityUsed,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a UserCollectionPart Object.
		///
		/// </summary>
		public void ApplyDTO(UserCollectionPartDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.userCollectionId = dto.userCollectionId;
			this.brickPartId = dto.brickPartId;
			this.brickColourId = dto.brickColourId;
			this.quantityOwned = dto.quantityOwned;
			this.quantityUsed = dto.quantityUsed;
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
		/// Creates a deep copy clone of a UserCollectionPart Object.
		///
		/// </summary>
		public UserCollectionPart Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new UserCollectionPart{
				id = this.id,
				tenantGuid = this.tenantGuid,
				userCollectionId = this.userCollectionId,
				brickPartId = this.brickPartId,
				brickColourId = this.brickColourId,
				quantityOwned = this.quantityOwned,
				quantityUsed = this.quantityUsed,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserCollectionPart Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserCollectionPart Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a UserCollectionPart Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a UserCollectionPart Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.UserCollectionPart userCollectionPart)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (userCollectionPart == null)
			{
				return null;
			}

			return new {
				id = userCollectionPart.id,
				userCollectionId = userCollectionPart.userCollectionId,
				brickPartId = userCollectionPart.brickPartId,
				brickColourId = userCollectionPart.brickColourId,
				quantityOwned = userCollectionPart.quantityOwned,
				quantityUsed = userCollectionPart.quantityUsed,
				objectGuid = userCollectionPart.objectGuid,
				active = userCollectionPart.active,
				deleted = userCollectionPart.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a UserCollectionPart Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(UserCollectionPart userCollectionPart)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (userCollectionPart == null)
			{
				return null;
			}

			return new {
				id = userCollectionPart.id,
				userCollectionId = userCollectionPart.userCollectionId,
				brickPartId = userCollectionPart.brickPartId,
				brickColourId = userCollectionPart.brickColourId,
				quantityOwned = userCollectionPart.quantityOwned,
				quantityUsed = userCollectionPart.quantityUsed,
				objectGuid = userCollectionPart.objectGuid,
				active = userCollectionPart.active,
				deleted = userCollectionPart.deleted,
				brickColour = BrickColour.CreateMinimalAnonymous(userCollectionPart.brickColour),
				brickPart = BrickPart.CreateMinimalAnonymous(userCollectionPart.brickPart),
				userCollection = UserCollection.CreateMinimalAnonymous(userCollectionPart.userCollection)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a UserCollectionPart Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(UserCollectionPart userCollectionPart)
		{
			//
			// Return a very minimal object.
			//
			if (userCollectionPart == null)
			{
				return null;
			}

			return new {
				id = userCollectionPart.id,
				name = userCollectionPart.id,
				description = userCollectionPart.id
			 };
		}
	}
}
