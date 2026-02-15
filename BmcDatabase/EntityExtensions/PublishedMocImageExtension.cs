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
	public partial class PublishedMocImage : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class PublishedMocImageDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 publishedMocId { get; set; }
			[Required]
			public String imagePath { get; set; }
			public String caption { get; set; }
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
		public class PublishedMocImageOutputDTO : PublishedMocImageDTO
		{
			public PublishedMoc.PublishedMocDTO publishedMoc { get; set; }
		}


		/// <summary>
		///
		/// Converts a PublishedMocImage to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public PublishedMocImageDTO ToDTO()
		{
			return new PublishedMocImageDTO
			{
				id = this.id,
				publishedMocId = this.publishedMocId,
				imagePath = this.imagePath,
				caption = this.caption,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a PublishedMocImage list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<PublishedMocImageDTO> ToDTOList(List<PublishedMocImage> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PublishedMocImageDTO> output = new List<PublishedMocImageDTO>();

			output.Capacity = data.Count;

			foreach (PublishedMocImage publishedMocImage in data)
			{
				output.Add(publishedMocImage.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a PublishedMocImage to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the PublishedMocImageEntity type directly.
		///
		/// </summary>
		public PublishedMocImageOutputDTO ToOutputDTO()
		{
			return new PublishedMocImageOutputDTO
			{
				id = this.id,
				publishedMocId = this.publishedMocId,
				imagePath = this.imagePath,
				caption = this.caption,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				publishedMoc = this.publishedMoc?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a PublishedMocImage list to list of Output Data Transfer Object intended to be used for serializing a list of PublishedMocImage objects to avoid using the PublishedMocImage entity type directly.
		///
		/// </summary>
		public static List<PublishedMocImageOutputDTO> ToOutputDTOList(List<PublishedMocImage> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PublishedMocImageOutputDTO> output = new List<PublishedMocImageOutputDTO>();

			output.Capacity = data.Count;

			foreach (PublishedMocImage publishedMocImage in data)
			{
				output.Add(publishedMocImage.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a PublishedMocImage Object.
		///
		/// </summary>
		public static Database.PublishedMocImage FromDTO(PublishedMocImageDTO dto)
		{
			return new Database.PublishedMocImage
			{
				id = dto.id,
				publishedMocId = dto.publishedMocId,
				imagePath = dto.imagePath,
				caption = dto.caption,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a PublishedMocImage Object.
		///
		/// </summary>
		public void ApplyDTO(PublishedMocImageDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.publishedMocId = dto.publishedMocId;
			this.imagePath = dto.imagePath;
			this.caption = dto.caption;
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
		/// Creates a deep copy clone of a PublishedMocImage Object.
		///
		/// </summary>
		public PublishedMocImage Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new PublishedMocImage{
				id = this.id,
				tenantGuid = this.tenantGuid,
				publishedMocId = this.publishedMocId,
				imagePath = this.imagePath,
				caption = this.caption,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PublishedMocImage Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PublishedMocImage Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a PublishedMocImage Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a PublishedMocImage Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.PublishedMocImage publishedMocImage)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (publishedMocImage == null)
			{
				return null;
			}

			return new {
				id = publishedMocImage.id,
				publishedMocId = publishedMocImage.publishedMocId,
				imagePath = publishedMocImage.imagePath,
				caption = publishedMocImage.caption,
				sequence = publishedMocImage.sequence,
				objectGuid = publishedMocImage.objectGuid,
				active = publishedMocImage.active,
				deleted = publishedMocImage.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a PublishedMocImage Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(PublishedMocImage publishedMocImage)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (publishedMocImage == null)
			{
				return null;
			}

			return new {
				id = publishedMocImage.id,
				publishedMocId = publishedMocImage.publishedMocId,
				imagePath = publishedMocImage.imagePath,
				caption = publishedMocImage.caption,
				sequence = publishedMocImage.sequence,
				objectGuid = publishedMocImage.objectGuid,
				active = publishedMocImage.active,
				deleted = publishedMocImage.deleted,
				publishedMoc = PublishedMoc.CreateMinimalAnonymous(publishedMocImage.publishedMoc)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a PublishedMocImage Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(PublishedMocImage publishedMocImage)
		{
			//
			// Return a very minimal object.
			//
			if (publishedMocImage == null)
			{
				return null;
			}

			return new {
				id = publishedMocImage.id,
				name = publishedMocImage.imagePath,
				description = string.Join(", ", new[] { publishedMocImage.imagePath, publishedMocImage.caption}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
