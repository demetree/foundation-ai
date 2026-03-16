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
	public partial class MediaContent : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class MediaContentDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 mediaAssetId { get; set; }
			[Required]
			public Byte[] fileData { get; set; }
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
		public class MediaContentOutputDTO : MediaContentDTO
		{
			public MediaAsset.MediaAssetDTO mediaAsset { get; set; }
		}


		/// <summary>
		///
		/// Converts a MediaContent to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public MediaContentDTO ToDTO()
		{
			return new MediaContentDTO
			{
				id = this.id,
				mediaAssetId = this.mediaAssetId,
				fileData = this.fileData,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a MediaContent list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<MediaContentDTO> ToDTOList(List<MediaContent> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MediaContentDTO> output = new List<MediaContentDTO>();

			output.Capacity = data.Count;

			foreach (MediaContent mediaContent in data)
			{
				output.Add(mediaContent.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a MediaContent to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the MediaContentEntity type directly.
		///
		/// </summary>
		public MediaContentOutputDTO ToOutputDTO()
		{
			return new MediaContentOutputDTO
			{
				id = this.id,
				mediaAssetId = this.mediaAssetId,
				fileData = this.fileData,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				mediaAsset = this.mediaAsset?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a MediaContent list to list of Output Data Transfer Object intended to be used for serializing a list of MediaContent objects to avoid using the MediaContent entity type directly.
		///
		/// </summary>
		public static List<MediaContentOutputDTO> ToOutputDTOList(List<MediaContent> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MediaContentOutputDTO> output = new List<MediaContentOutputDTO>();

			output.Capacity = data.Count;

			foreach (MediaContent mediaContent in data)
			{
				output.Add(mediaContent.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a MediaContent Object.
		///
		/// </summary>
		public static Database.MediaContent FromDTO(MediaContentDTO dto)
		{
			return new Database.MediaContent
			{
				id = dto.id,
				mediaAssetId = dto.mediaAssetId,
				fileData = dto.fileData,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a MediaContent Object.
		///
		/// </summary>
		public void ApplyDTO(MediaContentDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.mediaAssetId = dto.mediaAssetId;
			this.fileData = dto.fileData;
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
		/// Creates a deep copy clone of a MediaContent Object.
		///
		/// </summary>
		public MediaContent Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new MediaContent{
				id = this.id,
				tenantGuid = this.tenantGuid,
				mediaAssetId = this.mediaAssetId,
				fileData = this.fileData,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MediaContent Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MediaContent Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a MediaContent Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a MediaContent Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.MediaContent mediaContent)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (mediaContent == null)
			{
				return null;
			}

			return new {
				id = mediaContent.id,
				mediaAssetId = mediaContent.mediaAssetId,
				fileData = mediaContent.fileData,
				objectGuid = mediaContent.objectGuid,
				active = mediaContent.active,
				deleted = mediaContent.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a MediaContent Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(MediaContent mediaContent)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (mediaContent == null)
			{
				return null;
			}

			return new {
				id = mediaContent.id,
				mediaAssetId = mediaContent.mediaAssetId,
				fileData = mediaContent.fileData,
				objectGuid = mediaContent.objectGuid,
				active = mediaContent.active,
				deleted = mediaContent.deleted,
				mediaAsset = MediaAsset.CreateMinimalAnonymous(mediaContent.mediaAsset)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a MediaContent Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(MediaContent mediaContent)
		{
			//
			// Return a very minimal object.
			//
			if (mediaContent == null)
			{
				return null;
			}

			return new {
				id = mediaContent.id,
				name = mediaContent.id,
				description = mediaContent.id
			 };
		}
	}
}
