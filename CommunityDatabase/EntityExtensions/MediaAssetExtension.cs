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
	public partial class MediaAsset : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class MediaAssetDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String fileName { get; set; }
			[Required]
			public String filePath { get; set; }
			[Required]
			public String mimeType { get; set; }
			public String altText { get; set; }
			public String caption { get; set; }
			public Int64? fileSizeBytes { get; set; }
			public Int32? imageWidth { get; set; }
			public Int32? imageHeight { get; set; }
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
		public class MediaAssetOutputDTO : MediaAssetDTO
		{
		}


		/// <summary>
		///
		/// Converts a MediaAsset to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public MediaAssetDTO ToDTO()
		{
			return new MediaAssetDTO
			{
				id = this.id,
				fileName = this.fileName,
				filePath = this.filePath,
				mimeType = this.mimeType,
				altText = this.altText,
				caption = this.caption,
				fileSizeBytes = this.fileSizeBytes,
				imageWidth = this.imageWidth,
				imageHeight = this.imageHeight,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a MediaAsset list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<MediaAssetDTO> ToDTOList(List<MediaAsset> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MediaAssetDTO> output = new List<MediaAssetDTO>();

			output.Capacity = data.Count;

			foreach (MediaAsset mediaAsset in data)
			{
				output.Add(mediaAsset.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a MediaAsset to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the MediaAssetEntity type directly.
		///
		/// </summary>
		public MediaAssetOutputDTO ToOutputDTO()
		{
			return new MediaAssetOutputDTO
			{
				id = this.id,
				fileName = this.fileName,
				filePath = this.filePath,
				mimeType = this.mimeType,
				altText = this.altText,
				caption = this.caption,
				fileSizeBytes = this.fileSizeBytes,
				imageWidth = this.imageWidth,
				imageHeight = this.imageHeight,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a MediaAsset list to list of Output Data Transfer Object intended to be used for serializing a list of MediaAsset objects to avoid using the MediaAsset entity type directly.
		///
		/// </summary>
		public static List<MediaAssetOutputDTO> ToOutputDTOList(List<MediaAsset> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MediaAssetOutputDTO> output = new List<MediaAssetOutputDTO>();

			output.Capacity = data.Count;

			foreach (MediaAsset mediaAsset in data)
			{
				output.Add(mediaAsset.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a MediaAsset Object.
		///
		/// </summary>
		public static Database.MediaAsset FromDTO(MediaAssetDTO dto)
		{
			return new Database.MediaAsset
			{
				id = dto.id,
				fileName = dto.fileName,
				filePath = dto.filePath,
				mimeType = dto.mimeType,
				altText = dto.altText,
				caption = dto.caption,
				fileSizeBytes = dto.fileSizeBytes,
				imageWidth = dto.imageWidth,
				imageHeight = dto.imageHeight,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a MediaAsset Object.
		///
		/// </summary>
		public void ApplyDTO(MediaAssetDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.fileName = dto.fileName;
			this.filePath = dto.filePath;
			this.mimeType = dto.mimeType;
			this.altText = dto.altText;
			this.caption = dto.caption;
			this.fileSizeBytes = dto.fileSizeBytes;
			this.imageWidth = dto.imageWidth;
			this.imageHeight = dto.imageHeight;
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
		/// Creates a deep copy clone of a MediaAsset Object.
		///
		/// </summary>
		public MediaAsset Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new MediaAsset{
				id = this.id,
				tenantGuid = this.tenantGuid,
				fileName = this.fileName,
				filePath = this.filePath,
				mimeType = this.mimeType,
				altText = this.altText,
				caption = this.caption,
				fileSizeBytes = this.fileSizeBytes,
				imageWidth = this.imageWidth,
				imageHeight = this.imageHeight,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MediaAsset Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MediaAsset Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a MediaAsset Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a MediaAsset Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.MediaAsset mediaAsset)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (mediaAsset == null)
			{
				return null;
			}

			return new {
				id = mediaAsset.id,
				fileName = mediaAsset.fileName,
				filePath = mediaAsset.filePath,
				mimeType = mediaAsset.mimeType,
				altText = mediaAsset.altText,
				caption = mediaAsset.caption,
				fileSizeBytes = mediaAsset.fileSizeBytes,
				imageWidth = mediaAsset.imageWidth,
				imageHeight = mediaAsset.imageHeight,
				objectGuid = mediaAsset.objectGuid,
				active = mediaAsset.active,
				deleted = mediaAsset.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a MediaAsset Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(MediaAsset mediaAsset)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (mediaAsset == null)
			{
				return null;
			}

			return new {
				id = mediaAsset.id,
				fileName = mediaAsset.fileName,
				filePath = mediaAsset.filePath,
				mimeType = mediaAsset.mimeType,
				altText = mediaAsset.altText,
				caption = mediaAsset.caption,
				fileSizeBytes = mediaAsset.fileSizeBytes,
				imageWidth = mediaAsset.imageWidth,
				imageHeight = mediaAsset.imageHeight,
				objectGuid = mediaAsset.objectGuid,
				active = mediaAsset.active,
				deleted = mediaAsset.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a MediaAsset Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(MediaAsset mediaAsset)
		{
			//
			// Return a very minimal object.
			//
			if (mediaAsset == null)
			{
				return null;
			}

			return new {
				id = mediaAsset.id,
				name = mediaAsset.fileName,
				description = string.Join(", ", new[] { mediaAsset.fileName, mediaAsset.filePath, mediaAsset.mimeType}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
