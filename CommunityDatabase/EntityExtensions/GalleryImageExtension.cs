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
	public partial class GalleryImage : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class GalleryImageDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 galleryAlbumId { get; set; }
			[Required]
			public String imageUrl { get; set; }
			public String caption { get; set; }
			public String altText { get; set; }
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
		public class GalleryImageOutputDTO : GalleryImageDTO
		{
			public GalleryAlbum.GalleryAlbumDTO galleryAlbum { get; set; }
		}


		/// <summary>
		///
		/// Converts a GalleryImage to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public GalleryImageDTO ToDTO()
		{
			return new GalleryImageDTO
			{
				id = this.id,
				galleryAlbumId = this.galleryAlbumId,
				imageUrl = this.imageUrl,
				caption = this.caption,
				altText = this.altText,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a GalleryImage list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<GalleryImageDTO> ToDTOList(List<GalleryImage> data)
		{
			if (data == null)
			{
				return null;
			}

			List<GalleryImageDTO> output = new List<GalleryImageDTO>();

			output.Capacity = data.Count;

			foreach (GalleryImage galleryImage in data)
			{
				output.Add(galleryImage.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a GalleryImage to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the GalleryImageEntity type directly.
		///
		/// </summary>
		public GalleryImageOutputDTO ToOutputDTO()
		{
			return new GalleryImageOutputDTO
			{
				id = this.id,
				galleryAlbumId = this.galleryAlbumId,
				imageUrl = this.imageUrl,
				caption = this.caption,
				altText = this.altText,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				galleryAlbum = this.galleryAlbum?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a GalleryImage list to list of Output Data Transfer Object intended to be used for serializing a list of GalleryImage objects to avoid using the GalleryImage entity type directly.
		///
		/// </summary>
		public static List<GalleryImageOutputDTO> ToOutputDTOList(List<GalleryImage> data)
		{
			if (data == null)
			{
				return null;
			}

			List<GalleryImageOutputDTO> output = new List<GalleryImageOutputDTO>();

			output.Capacity = data.Count;

			foreach (GalleryImage galleryImage in data)
			{
				output.Add(galleryImage.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a GalleryImage Object.
		///
		/// </summary>
		public static Database.GalleryImage FromDTO(GalleryImageDTO dto)
		{
			return new Database.GalleryImage
			{
				id = dto.id,
				galleryAlbumId = dto.galleryAlbumId,
				imageUrl = dto.imageUrl,
				caption = dto.caption,
				altText = dto.altText,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a GalleryImage Object.
		///
		/// </summary>
		public void ApplyDTO(GalleryImageDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.galleryAlbumId = dto.galleryAlbumId;
			this.imageUrl = dto.imageUrl;
			this.caption = dto.caption;
			this.altText = dto.altText;
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
		/// Creates a deep copy clone of a GalleryImage Object.
		///
		/// </summary>
		public GalleryImage Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new GalleryImage{
				id = this.id,
				tenantGuid = this.tenantGuid,
				galleryAlbumId = this.galleryAlbumId,
				imageUrl = this.imageUrl,
				caption = this.caption,
				altText = this.altText,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a GalleryImage Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a GalleryImage Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a GalleryImage Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a GalleryImage Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.GalleryImage galleryImage)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (galleryImage == null)
			{
				return null;
			}

			return new {
				id = galleryImage.id,
				galleryAlbumId = galleryImage.galleryAlbumId,
				imageUrl = galleryImage.imageUrl,
				caption = galleryImage.caption,
				altText = galleryImage.altText,
				sequence = galleryImage.sequence,
				objectGuid = galleryImage.objectGuid,
				active = galleryImage.active,
				deleted = galleryImage.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a GalleryImage Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(GalleryImage galleryImage)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (galleryImage == null)
			{
				return null;
			}

			return new {
				id = galleryImage.id,
				galleryAlbumId = galleryImage.galleryAlbumId,
				imageUrl = galleryImage.imageUrl,
				caption = galleryImage.caption,
				altText = galleryImage.altText,
				sequence = galleryImage.sequence,
				objectGuid = galleryImage.objectGuid,
				active = galleryImage.active,
				deleted = galleryImage.deleted,
				galleryAlbum = GalleryAlbum.CreateMinimalAnonymous(galleryImage.galleryAlbum)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a GalleryImage Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(GalleryImage galleryImage)
		{
			//
			// Return a very minimal object.
			//
			if (galleryImage == null)
			{
				return null;
			}

			return new {
				id = galleryImage.id,
				name = galleryImage.imageUrl,
				description = string.Join(", ", new[] { galleryImage.imageUrl, galleryImage.caption, galleryImage.altText}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
