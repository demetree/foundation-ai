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
	public partial class UserSetOwnership : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class UserSetOwnershipDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 legoSetId { get; set; }
			[Required]
			public String status { get; set; }
			public DateTime? acquiredDate { get; set; }
			public Int32? personalRating { get; set; }
			public String notes { get; set; }
			[Required]
			public Int32 quantity { get; set; }
			[Required]
			public Boolean isPublic { get; set; }
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
		public class UserSetOwnershipOutputDTO : UserSetOwnershipDTO
		{
			public LegoSet.LegoSetDTO legoSet { get; set; }
		}


		/// <summary>
		///
		/// Converts a UserSetOwnership to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public UserSetOwnershipDTO ToDTO()
		{
			return new UserSetOwnershipDTO
			{
				id = this.id,
				legoSetId = this.legoSetId,
				status = this.status,
				acquiredDate = this.acquiredDate,
				personalRating = this.personalRating,
				notes = this.notes,
				quantity = this.quantity,
				isPublic = this.isPublic,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a UserSetOwnership list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<UserSetOwnershipDTO> ToDTOList(List<UserSetOwnership> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserSetOwnershipDTO> output = new List<UserSetOwnershipDTO>();

			output.Capacity = data.Count;

			foreach (UserSetOwnership userSetOwnership in data)
			{
				output.Add(userSetOwnership.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a UserSetOwnership to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the UserSetOwnershipEntity type directly.
		///
		/// </summary>
		public UserSetOwnershipOutputDTO ToOutputDTO()
		{
			return new UserSetOwnershipOutputDTO
			{
				id = this.id,
				legoSetId = this.legoSetId,
				status = this.status,
				acquiredDate = this.acquiredDate,
				personalRating = this.personalRating,
				notes = this.notes,
				quantity = this.quantity,
				isPublic = this.isPublic,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				legoSet = this.legoSet?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a UserSetOwnership list to list of Output Data Transfer Object intended to be used for serializing a list of UserSetOwnership objects to avoid using the UserSetOwnership entity type directly.
		///
		/// </summary>
		public static List<UserSetOwnershipOutputDTO> ToOutputDTOList(List<UserSetOwnership> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserSetOwnershipOutputDTO> output = new List<UserSetOwnershipOutputDTO>();

			output.Capacity = data.Count;

			foreach (UserSetOwnership userSetOwnership in data)
			{
				output.Add(userSetOwnership.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a UserSetOwnership Object.
		///
		/// </summary>
		public static Database.UserSetOwnership FromDTO(UserSetOwnershipDTO dto)
		{
			return new Database.UserSetOwnership
			{
				id = dto.id,
				legoSetId = dto.legoSetId,
				status = dto.status,
				acquiredDate = dto.acquiredDate,
				personalRating = dto.personalRating,
				notes = dto.notes,
				quantity = dto.quantity,
				isPublic = dto.isPublic,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a UserSetOwnership Object.
		///
		/// </summary>
		public void ApplyDTO(UserSetOwnershipDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.legoSetId = dto.legoSetId;
			this.status = dto.status;
			this.acquiredDate = dto.acquiredDate;
			this.personalRating = dto.personalRating;
			this.notes = dto.notes;
			this.quantity = dto.quantity;
			this.isPublic = dto.isPublic;
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
		/// Creates a deep copy clone of a UserSetOwnership Object.
		///
		/// </summary>
		public UserSetOwnership Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new UserSetOwnership{
				id = this.id,
				tenantGuid = this.tenantGuid,
				legoSetId = this.legoSetId,
				status = this.status,
				acquiredDate = this.acquiredDate,
				personalRating = this.personalRating,
				notes = this.notes,
				quantity = this.quantity,
				isPublic = this.isPublic,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserSetOwnership Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserSetOwnership Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a UserSetOwnership Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a UserSetOwnership Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.UserSetOwnership userSetOwnership)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (userSetOwnership == null)
			{
				return null;
			}

			return new {
				id = userSetOwnership.id,
				legoSetId = userSetOwnership.legoSetId,
				status = userSetOwnership.status,
				acquiredDate = userSetOwnership.acquiredDate,
				personalRating = userSetOwnership.personalRating,
				notes = userSetOwnership.notes,
				quantity = userSetOwnership.quantity,
				isPublic = userSetOwnership.isPublic,
				objectGuid = userSetOwnership.objectGuid,
				active = userSetOwnership.active,
				deleted = userSetOwnership.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a UserSetOwnership Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(UserSetOwnership userSetOwnership)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (userSetOwnership == null)
			{
				return null;
			}

			return new {
				id = userSetOwnership.id,
				legoSetId = userSetOwnership.legoSetId,
				status = userSetOwnership.status,
				acquiredDate = userSetOwnership.acquiredDate,
				personalRating = userSetOwnership.personalRating,
				notes = userSetOwnership.notes,
				quantity = userSetOwnership.quantity,
				isPublic = userSetOwnership.isPublic,
				objectGuid = userSetOwnership.objectGuid,
				active = userSetOwnership.active,
				deleted = userSetOwnership.deleted,
				legoSet = LegoSet.CreateMinimalAnonymous(userSetOwnership.legoSet)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a UserSetOwnership Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(UserSetOwnership userSetOwnership)
		{
			//
			// Return a very minimal object.
			//
			if (userSetOwnership == null)
			{
				return null;
			}

			return new {
				id = userSetOwnership.id,
				name = userSetOwnership.status,
				description = string.Join(", ", new[] { userSetOwnership.status}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
