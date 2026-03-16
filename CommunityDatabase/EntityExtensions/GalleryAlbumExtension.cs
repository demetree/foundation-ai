using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Community.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class GalleryAlbum : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class GalleryAlbumDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String title { get; set; }
			[Required]
			public String slug { get; set; }
			public String description { get; set; }
			public String coverImageUrl { get; set; }
			[Required]
			public Boolean isPublished { get; set; }
			public Int32? sequence { get; set; }
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
		public class GalleryAlbumOutputDTO : GalleryAlbumDTO
		{
		}


		/// <summary>
		///
		/// Converts a GalleryAlbum to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public GalleryAlbumDTO ToDTO()
		{
			return new GalleryAlbumDTO
			{
				id = this.id,
				title = this.title,
				slug = this.slug,
				description = this.description,
				coverImageUrl = this.coverImageUrl,
				isPublished = this.isPublished,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a GalleryAlbum list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<GalleryAlbumDTO> ToDTOList(List<GalleryAlbum> data)
		{
			if (data == null)
			{
				return null;
			}

			List<GalleryAlbumDTO> output = new List<GalleryAlbumDTO>();

			output.Capacity = data.Count;

			foreach (GalleryAlbum galleryAlbum in data)
			{
				output.Add(galleryAlbum.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a GalleryAlbum to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the GalleryAlbumEntity type directly.
		///
		/// </summary>
		public GalleryAlbumOutputDTO ToOutputDTO()
		{
			return new GalleryAlbumOutputDTO
			{
				id = this.id,
				title = this.title,
				slug = this.slug,
				description = this.description,
				coverImageUrl = this.coverImageUrl,
				isPublished = this.isPublished,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a GalleryAlbum list to list of Output Data Transfer Object intended to be used for serializing a list of GalleryAlbum objects to avoid using the GalleryAlbum entity type directly.
		///
		/// </summary>
		public static List<GalleryAlbumOutputDTO> ToOutputDTOList(List<GalleryAlbum> data)
		{
			if (data == null)
			{
				return null;
			}

			List<GalleryAlbumOutputDTO> output = new List<GalleryAlbumOutputDTO>();

			output.Capacity = data.Count;

			foreach (GalleryAlbum galleryAlbum in data)
			{
				output.Add(galleryAlbum.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a GalleryAlbum Object.
		///
		/// </summary>
		public static Database.GalleryAlbum FromDTO(GalleryAlbumDTO dto)
		{
			return new Database.GalleryAlbum
			{
				id = dto.id,
				title = dto.title,
				slug = dto.slug,
				description = dto.description,
				coverImageUrl = dto.coverImageUrl,
				isPublished = dto.isPublished,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a GalleryAlbum Object.
		///
		/// </summary>
		public void ApplyDTO(GalleryAlbumDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.title = dto.title;
			this.slug = dto.slug;
			this.description = dto.description;
			this.coverImageUrl = dto.coverImageUrl;
			this.isPublished = dto.isPublished;
			this.sequence = dto.sequence;
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
		/// Creates a deep copy clone of a GalleryAlbum Object.
		///
		/// </summary>
		public GalleryAlbum Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new GalleryAlbum{
				id = this.id,
				tenantGuid = this.tenantGuid,
				title = this.title,
				slug = this.slug,
				description = this.description,
				coverImageUrl = this.coverImageUrl,
				isPublished = this.isPublished,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a GalleryAlbum Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a GalleryAlbum Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a GalleryAlbum Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a GalleryAlbum Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.GalleryAlbum galleryAlbum)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (galleryAlbum == null)
			{
				return null;
			}

			return new {
				id = galleryAlbum.id,
				title = galleryAlbum.title,
				slug = galleryAlbum.slug,
				description = galleryAlbum.description,
				coverImageUrl = galleryAlbum.coverImageUrl,
				isPublished = galleryAlbum.isPublished,
				sequence = galleryAlbum.sequence,
				objectGuid = galleryAlbum.objectGuid,
				active = galleryAlbum.active,
				deleted = galleryAlbum.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a GalleryAlbum Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(GalleryAlbum galleryAlbum)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (galleryAlbum == null)
			{
				return null;
			}

			return new {
				id = galleryAlbum.id,
				title = galleryAlbum.title,
				slug = galleryAlbum.slug,
				description = galleryAlbum.description,
				coverImageUrl = galleryAlbum.coverImageUrl,
				isPublished = galleryAlbum.isPublished,
				sequence = galleryAlbum.sequence,
				objectGuid = galleryAlbum.objectGuid,
				active = galleryAlbum.active,
				deleted = galleryAlbum.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a GalleryAlbum Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(GalleryAlbum galleryAlbum)
		{
			//
			// Return a very minimal object.
			//
			if (galleryAlbum == null)
			{
				return null;
			}

			return new {
				id = galleryAlbum.id,
				description = galleryAlbum.description,
				name = galleryAlbum.title
			 };
		}
	}
}
