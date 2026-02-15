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
	public partial class MocFavourite : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class MocFavouriteDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 publishedMocId { get; set; }
			[Required]
			public Guid userTenantGuid { get; set; }
			[Required]
			public DateTime favouritedDate { get; set; }
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
		public class MocFavouriteOutputDTO : MocFavouriteDTO
		{
			public PublishedMoc.PublishedMocDTO publishedMoc { get; set; }
		}


		/// <summary>
		///
		/// Converts a MocFavourite to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public MocFavouriteDTO ToDTO()
		{
			return new MocFavouriteDTO
			{
				id = this.id,
				publishedMocId = this.publishedMocId,
				userTenantGuid = this.userTenantGuid,
				favouritedDate = this.favouritedDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a MocFavourite list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<MocFavouriteDTO> ToDTOList(List<MocFavourite> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MocFavouriteDTO> output = new List<MocFavouriteDTO>();

			output.Capacity = data.Count;

			foreach (MocFavourite mocFavourite in data)
			{
				output.Add(mocFavourite.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a MocFavourite to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the MocFavouriteEntity type directly.
		///
		/// </summary>
		public MocFavouriteOutputDTO ToOutputDTO()
		{
			return new MocFavouriteOutputDTO
			{
				id = this.id,
				publishedMocId = this.publishedMocId,
				userTenantGuid = this.userTenantGuid,
				favouritedDate = this.favouritedDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				publishedMoc = this.publishedMoc?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a MocFavourite list to list of Output Data Transfer Object intended to be used for serializing a list of MocFavourite objects to avoid using the MocFavourite entity type directly.
		///
		/// </summary>
		public static List<MocFavouriteOutputDTO> ToOutputDTOList(List<MocFavourite> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MocFavouriteOutputDTO> output = new List<MocFavouriteOutputDTO>();

			output.Capacity = data.Count;

			foreach (MocFavourite mocFavourite in data)
			{
				output.Add(mocFavourite.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a MocFavourite Object.
		///
		/// </summary>
		public static Database.MocFavourite FromDTO(MocFavouriteDTO dto)
		{
			return new Database.MocFavourite
			{
				id = dto.id,
				publishedMocId = dto.publishedMocId,
				userTenantGuid = dto.userTenantGuid,
				favouritedDate = dto.favouritedDate,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a MocFavourite Object.
		///
		/// </summary>
		public void ApplyDTO(MocFavouriteDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.publishedMocId = dto.publishedMocId;
			this.userTenantGuid = dto.userTenantGuid;
			this.favouritedDate = dto.favouritedDate;
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
		/// Creates a deep copy clone of a MocFavourite Object.
		///
		/// </summary>
		public MocFavourite Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new MocFavourite{
				id = this.id,
				publishedMocId = this.publishedMocId,
				userTenantGuid = this.userTenantGuid,
				favouritedDate = this.favouritedDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MocFavourite Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MocFavourite Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a MocFavourite Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a MocFavourite Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.MocFavourite mocFavourite)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (mocFavourite == null)
			{
				return null;
			}

			return new {
				id = mocFavourite.id,
				publishedMocId = mocFavourite.publishedMocId,
				userTenantGuid = mocFavourite.userTenantGuid,
				favouritedDate = mocFavourite.favouritedDate,
				objectGuid = mocFavourite.objectGuid,
				active = mocFavourite.active,
				deleted = mocFavourite.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a MocFavourite Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(MocFavourite mocFavourite)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (mocFavourite == null)
			{
				return null;
			}

			return new {
				id = mocFavourite.id,
				publishedMocId = mocFavourite.publishedMocId,
				userTenantGuid = mocFavourite.userTenantGuid,
				favouritedDate = mocFavourite.favouritedDate,
				objectGuid = mocFavourite.objectGuid,
				active = mocFavourite.active,
				deleted = mocFavourite.deleted,
				publishedMoc = PublishedMoc.CreateMinimalAnonymous(mocFavourite.publishedMoc)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a MocFavourite Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(MocFavourite mocFavourite)
		{
			//
			// Return a very minimal object.
			//
			if (mocFavourite == null)
			{
				return null;
			}

			return new {
				id = mocFavourite.id,
				name = mocFavourite.id,
				description = mocFavourite.id
			 };
		}
	}
}
