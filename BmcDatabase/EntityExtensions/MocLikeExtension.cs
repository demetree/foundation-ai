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
	public partial class MocLike : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class MocLikeDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 publishedMocId { get; set; }
			[Required]
			public Guid likerTenantGuid { get; set; }
			[Required]
			public DateTime likedDate { get; set; }
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
		public class MocLikeOutputDTO : MocLikeDTO
		{
			public PublishedMoc.PublishedMocDTO publishedMoc { get; set; }
		}


		/// <summary>
		///
		/// Converts a MocLike to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public MocLikeDTO ToDTO()
		{
			return new MocLikeDTO
			{
				id = this.id,
				publishedMocId = this.publishedMocId,
				likerTenantGuid = this.likerTenantGuid,
				likedDate = this.likedDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a MocLike list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<MocLikeDTO> ToDTOList(List<MocLike> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MocLikeDTO> output = new List<MocLikeDTO>();

			output.Capacity = data.Count;

			foreach (MocLike mocLike in data)
			{
				output.Add(mocLike.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a MocLike to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the MocLikeEntity type directly.
		///
		/// </summary>
		public MocLikeOutputDTO ToOutputDTO()
		{
			return new MocLikeOutputDTO
			{
				id = this.id,
				publishedMocId = this.publishedMocId,
				likerTenantGuid = this.likerTenantGuid,
				likedDate = this.likedDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				publishedMoc = this.publishedMoc?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a MocLike list to list of Output Data Transfer Object intended to be used for serializing a list of MocLike objects to avoid using the MocLike entity type directly.
		///
		/// </summary>
		public static List<MocLikeOutputDTO> ToOutputDTOList(List<MocLike> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MocLikeOutputDTO> output = new List<MocLikeOutputDTO>();

			output.Capacity = data.Count;

			foreach (MocLike mocLike in data)
			{
				output.Add(mocLike.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a MocLike Object.
		///
		/// </summary>
		public static Database.MocLike FromDTO(MocLikeDTO dto)
		{
			return new Database.MocLike
			{
				id = dto.id,
				publishedMocId = dto.publishedMocId,
				likerTenantGuid = dto.likerTenantGuid,
				likedDate = dto.likedDate,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a MocLike Object.
		///
		/// </summary>
		public void ApplyDTO(MocLikeDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.publishedMocId = dto.publishedMocId;
			this.likerTenantGuid = dto.likerTenantGuid;
			this.likedDate = dto.likedDate;
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
		/// Creates a deep copy clone of a MocLike Object.
		///
		/// </summary>
		public MocLike Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new MocLike{
				id = this.id,
				publishedMocId = this.publishedMocId,
				likerTenantGuid = this.likerTenantGuid,
				likedDate = this.likedDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MocLike Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MocLike Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a MocLike Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a MocLike Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.MocLike mocLike)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (mocLike == null)
			{
				return null;
			}

			return new {
				id = mocLike.id,
				publishedMocId = mocLike.publishedMocId,
				likerTenantGuid = mocLike.likerTenantGuid,
				likedDate = mocLike.likedDate,
				objectGuid = mocLike.objectGuid,
				active = mocLike.active,
				deleted = mocLike.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a MocLike Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(MocLike mocLike)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (mocLike == null)
			{
				return null;
			}

			return new {
				id = mocLike.id,
				publishedMocId = mocLike.publishedMocId,
				likerTenantGuid = mocLike.likerTenantGuid,
				likedDate = mocLike.likedDate,
				objectGuid = mocLike.objectGuid,
				active = mocLike.active,
				deleted = mocLike.deleted,
				publishedMoc = PublishedMoc.CreateMinimalAnonymous(mocLike.publishedMoc)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a MocLike Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(MocLike mocLike)
		{
			//
			// Return a very minimal object.
			//
			if (mocLike == null)
			{
				return null;
			}

			return new {
				id = mocLike.id,
				name = mocLike.id,
				description = mocLike.id
			 };
		}
	}
}
