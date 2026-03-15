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
			public Int32 Id { get; set; }
			public String Title { get; set; }
			public String Slug { get; set; }
			public String Description { get; set; }
			public String CoverImageUrl { get; set; }
			public Boolean IsPublished { get; set; }
			public Int32? Sequence { get; set; }
			public Guid ObjectGuid { get; set; }
			public Boolean? Active { get; set; }
			public Boolean? Deleted { get; set; }
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
				Id = this.Id,
				Title = this.Title,
				Slug = this.Slug,
				Description = this.Description,
				CoverImageUrl = this.CoverImageUrl,
				IsPublished = this.IsPublished,
				Sequence = this.Sequence,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted
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
				Id = this.Id,
				Title = this.Title,
				Slug = this.Slug,
				Description = this.Description,
				CoverImageUrl = this.CoverImageUrl,
				IsPublished = this.IsPublished,
				Sequence = this.Sequence,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted
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
				Id = dto.Id,
				Title = dto.Title,
				Slug = dto.Slug,
				Description = dto.Description,
				CoverImageUrl = dto.CoverImageUrl,
				IsPublished = dto.IsPublished,
				Sequence = dto.Sequence,
				ObjectGuid = dto.ObjectGuid,
				Active = dto.Active ?? true,
				Deleted = dto.Deleted ?? false
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

			this.Title = dto.Title;
			this.Slug = dto.Slug;
			this.Description = dto.Description;
			this.CoverImageUrl = dto.CoverImageUrl;
			this.IsPublished = dto.IsPublished;
			this.Sequence = dto.Sequence;
			this.ObjectGuid = dto.ObjectGuid;
			if (dto.Active.HasValue == true)
			{
				this.Active = dto.Active.Value;
			}
			if (dto.Deleted.HasValue == true)
			{
				this.Deleted = dto.Deleted.Value;
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
				Id = this.Id,
				Title = this.Title,
				Slug = this.Slug,
				Description = this.Description,
				CoverImageUrl = this.CoverImageUrl,
				IsPublished = this.IsPublished,
				Sequence = this.Sequence,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted,
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
				Id = galleryAlbum.Id,
				Title = galleryAlbum.Title,
				Slug = galleryAlbum.Slug,
				Description = galleryAlbum.Description,
				CoverImageUrl = galleryAlbum.CoverImageUrl,
				IsPublished = galleryAlbum.IsPublished,
				Sequence = galleryAlbum.Sequence,
				ObjectGuid = galleryAlbum.ObjectGuid,
				Active = galleryAlbum.Active,
				Deleted = galleryAlbum.Deleted,
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
				Id = galleryAlbum.Id,
				Title = galleryAlbum.Title,
				Slug = galleryAlbum.Slug,
				Description = galleryAlbum.Description,
				CoverImageUrl = galleryAlbum.CoverImageUrl,
				IsPublished = galleryAlbum.IsPublished,
				Sequence = galleryAlbum.Sequence,
				ObjectGuid = galleryAlbum.ObjectGuid,
				Active = galleryAlbum.Active,
				Deleted = galleryAlbum.Deleted,
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
				name = galleryAlbum.title,
				description = string.Join(", ", new[] { galleryAlbum.title, galleryAlbum.slug, galleryAlbum.coverImageUrl}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
