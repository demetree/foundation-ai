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
	public partial class UserCollectionSetImport : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class UserCollectionSetImportDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 userCollectionId { get; set; }
			[Required]
			public Int32 legoSetId { get; set; }
			public Int32? quantity { get; set; }
			public DateTime? importedDate { get; set; }
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
		public class UserCollectionSetImportOutputDTO : UserCollectionSetImportDTO
		{
			public LegoSet.LegoSetDTO legoSet { get; set; }
			public UserCollection.UserCollectionDTO userCollection { get; set; }
		}


		/// <summary>
		///
		/// Converts a UserCollectionSetImport to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public UserCollectionSetImportDTO ToDTO()
		{
			return new UserCollectionSetImportDTO
			{
				id = this.id,
				userCollectionId = this.userCollectionId,
				legoSetId = this.legoSetId,
				quantity = this.quantity,
				importedDate = this.importedDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a UserCollectionSetImport list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<UserCollectionSetImportDTO> ToDTOList(List<UserCollectionSetImport> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserCollectionSetImportDTO> output = new List<UserCollectionSetImportDTO>();

			output.Capacity = data.Count;

			foreach (UserCollectionSetImport userCollectionSetImport in data)
			{
				output.Add(userCollectionSetImport.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a UserCollectionSetImport to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the UserCollectionSetImportEntity type directly.
		///
		/// </summary>
		public UserCollectionSetImportOutputDTO ToOutputDTO()
		{
			return new UserCollectionSetImportOutputDTO
			{
				id = this.id,
				userCollectionId = this.userCollectionId,
				legoSetId = this.legoSetId,
				quantity = this.quantity,
				importedDate = this.importedDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				legoSet = this.legoSet?.ToDTO(),
				userCollection = this.userCollection?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a UserCollectionSetImport list to list of Output Data Transfer Object intended to be used for serializing a list of UserCollectionSetImport objects to avoid using the UserCollectionSetImport entity type directly.
		///
		/// </summary>
		public static List<UserCollectionSetImportOutputDTO> ToOutputDTOList(List<UserCollectionSetImport> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserCollectionSetImportOutputDTO> output = new List<UserCollectionSetImportOutputDTO>();

			output.Capacity = data.Count;

			foreach (UserCollectionSetImport userCollectionSetImport in data)
			{
				output.Add(userCollectionSetImport.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a UserCollectionSetImport Object.
		///
		/// </summary>
		public static Database.UserCollectionSetImport FromDTO(UserCollectionSetImportDTO dto)
		{
			return new Database.UserCollectionSetImport
			{
				id = dto.id,
				userCollectionId = dto.userCollectionId,
				legoSetId = dto.legoSetId,
				quantity = dto.quantity,
				importedDate = dto.importedDate,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a UserCollectionSetImport Object.
		///
		/// </summary>
		public void ApplyDTO(UserCollectionSetImportDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.userCollectionId = dto.userCollectionId;
			this.legoSetId = dto.legoSetId;
			this.quantity = dto.quantity;
			this.importedDate = dto.importedDate;
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
		/// Creates a deep copy clone of a UserCollectionSetImport Object.
		///
		/// </summary>
		public UserCollectionSetImport Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new UserCollectionSetImport{
				id = this.id,
				tenantGuid = this.tenantGuid,
				userCollectionId = this.userCollectionId,
				legoSetId = this.legoSetId,
				quantity = this.quantity,
				importedDate = this.importedDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserCollectionSetImport Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserCollectionSetImport Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a UserCollectionSetImport Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a UserCollectionSetImport Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.UserCollectionSetImport userCollectionSetImport)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (userCollectionSetImport == null)
			{
				return null;
			}

			return new {
				id = userCollectionSetImport.id,
				userCollectionId = userCollectionSetImport.userCollectionId,
				legoSetId = userCollectionSetImport.legoSetId,
				quantity = userCollectionSetImport.quantity,
				importedDate = userCollectionSetImport.importedDate,
				objectGuid = userCollectionSetImport.objectGuid,
				active = userCollectionSetImport.active,
				deleted = userCollectionSetImport.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a UserCollectionSetImport Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(UserCollectionSetImport userCollectionSetImport)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (userCollectionSetImport == null)
			{
				return null;
			}

			return new {
				id = userCollectionSetImport.id,
				userCollectionId = userCollectionSetImport.userCollectionId,
				legoSetId = userCollectionSetImport.legoSetId,
				quantity = userCollectionSetImport.quantity,
				importedDate = userCollectionSetImport.importedDate,
				objectGuid = userCollectionSetImport.objectGuid,
				active = userCollectionSetImport.active,
				deleted = userCollectionSetImport.deleted,
				legoSet = LegoSet.CreateMinimalAnonymous(userCollectionSetImport.legoSet),
				userCollection = UserCollection.CreateMinimalAnonymous(userCollectionSetImport.userCollection)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a UserCollectionSetImport Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(UserCollectionSetImport userCollectionSetImport)
		{
			//
			// Return a very minimal object.
			//
			if (userCollectionSetImport == null)
			{
				return null;
			}

			return new {
				id = userCollectionSetImport.id,
				name = userCollectionSetImport.id,
				description = userCollectionSetImport.id
			 };
		}
	}
}
