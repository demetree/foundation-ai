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
	public partial class UserLostPart : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class UserLostPartDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 brickPartId { get; set; }
			[Required]
			public Int32 brickColourId { get; set; }
			public Int32? legoSetId { get; set; }
			[Required]
			public Int32 lostQuantity { get; set; }
			public Int32? rebrickableInvPartId { get; set; }
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
		public class UserLostPartOutputDTO : UserLostPartDTO
		{
			public BrickColour.BrickColourDTO brickColour { get; set; }
			public BrickPart.BrickPartDTO brickPart { get; set; }
			public LegoSet.LegoSetDTO legoSet { get; set; }
		}


		/// <summary>
		///
		/// Converts a UserLostPart to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public UserLostPartDTO ToDTO()
		{
			return new UserLostPartDTO
			{
				id = this.id,
				brickPartId = this.brickPartId,
				brickColourId = this.brickColourId,
				legoSetId = this.legoSetId,
				lostQuantity = this.lostQuantity,
				rebrickableInvPartId = this.rebrickableInvPartId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a UserLostPart list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<UserLostPartDTO> ToDTOList(List<UserLostPart> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserLostPartDTO> output = new List<UserLostPartDTO>();

			output.Capacity = data.Count;

			foreach (UserLostPart userLostPart in data)
			{
				output.Add(userLostPart.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a UserLostPart to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the UserLostPartEntity type directly.
		///
		/// </summary>
		public UserLostPartOutputDTO ToOutputDTO()
		{
			return new UserLostPartOutputDTO
			{
				id = this.id,
				brickPartId = this.brickPartId,
				brickColourId = this.brickColourId,
				legoSetId = this.legoSetId,
				lostQuantity = this.lostQuantity,
				rebrickableInvPartId = this.rebrickableInvPartId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				brickColour = this.brickColour?.ToDTO(),
				brickPart = this.brickPart?.ToDTO(),
				legoSet = this.legoSet?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a UserLostPart list to list of Output Data Transfer Object intended to be used for serializing a list of UserLostPart objects to avoid using the UserLostPart entity type directly.
		///
		/// </summary>
		public static List<UserLostPartOutputDTO> ToOutputDTOList(List<UserLostPart> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserLostPartOutputDTO> output = new List<UserLostPartOutputDTO>();

			output.Capacity = data.Count;

			foreach (UserLostPart userLostPart in data)
			{
				output.Add(userLostPart.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a UserLostPart Object.
		///
		/// </summary>
		public static Database.UserLostPart FromDTO(UserLostPartDTO dto)
		{
			return new Database.UserLostPart
			{
				id = dto.id,
				brickPartId = dto.brickPartId,
				brickColourId = dto.brickColourId,
				legoSetId = dto.legoSetId,
				lostQuantity = dto.lostQuantity,
				rebrickableInvPartId = dto.rebrickableInvPartId,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a UserLostPart Object.
		///
		/// </summary>
		public void ApplyDTO(UserLostPartDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.brickPartId = dto.brickPartId;
			this.brickColourId = dto.brickColourId;
			this.legoSetId = dto.legoSetId;
			this.lostQuantity = dto.lostQuantity;
			this.rebrickableInvPartId = dto.rebrickableInvPartId;
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
		/// Creates a deep copy clone of a UserLostPart Object.
		///
		/// </summary>
		public UserLostPart Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new UserLostPart{
				id = this.id,
				tenantGuid = this.tenantGuid,
				brickPartId = this.brickPartId,
				brickColourId = this.brickColourId,
				legoSetId = this.legoSetId,
				lostQuantity = this.lostQuantity,
				rebrickableInvPartId = this.rebrickableInvPartId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserLostPart Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserLostPart Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a UserLostPart Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a UserLostPart Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.UserLostPart userLostPart)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (userLostPart == null)
			{
				return null;
			}

			return new {
				id = userLostPart.id,
				brickPartId = userLostPart.brickPartId,
				brickColourId = userLostPart.brickColourId,
				legoSetId = userLostPart.legoSetId,
				lostQuantity = userLostPart.lostQuantity,
				rebrickableInvPartId = userLostPart.rebrickableInvPartId,
				objectGuid = userLostPart.objectGuid,
				active = userLostPart.active,
				deleted = userLostPart.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a UserLostPart Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(UserLostPart userLostPart)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (userLostPart == null)
			{
				return null;
			}

			return new {
				id = userLostPart.id,
				brickPartId = userLostPart.brickPartId,
				brickColourId = userLostPart.brickColourId,
				legoSetId = userLostPart.legoSetId,
				lostQuantity = userLostPart.lostQuantity,
				rebrickableInvPartId = userLostPart.rebrickableInvPartId,
				objectGuid = userLostPart.objectGuid,
				active = userLostPart.active,
				deleted = userLostPart.deleted,
				brickColour = BrickColour.CreateMinimalAnonymous(userLostPart.brickColour),
				brickPart = BrickPart.CreateMinimalAnonymous(userLostPart.brickPart),
				legoSet = LegoSet.CreateMinimalAnonymous(userLostPart.legoSet)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a UserLostPart Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(UserLostPart userLostPart)
		{
			//
			// Return a very minimal object.
			//
			if (userLostPart == null)
			{
				return null;
			}

			return new {
				id = userLostPart.id,
				name = userLostPart.id,
				description = userLostPart.id
			 };
		}
	}
}
