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
			public Int32 Id { get; set; }
			public String FileName { get; set; }
			public String FilePath { get; set; }
			public String MimeType { get; set; }
			public String AltText { get; set; }
			public String Caption { get; set; }
			public Int64? FileSizeBytes { get; set; }
			public Int32? ImageWidth { get; set; }
			public Int32? ImageHeight { get; set; }
			public Guid ObjectGuid { get; set; }
			public Boolean? Active { get; set; }
			public Boolean? Deleted { get; set; }
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
				Id = this.Id,
				FileName = this.FileName,
				FilePath = this.FilePath,
				MimeType = this.MimeType,
				AltText = this.AltText,
				Caption = this.Caption,
				FileSizeBytes = this.FileSizeBytes,
				ImageWidth = this.ImageWidth,
				ImageHeight = this.ImageHeight,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted
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
				Id = this.Id,
				FileName = this.FileName,
				FilePath = this.FilePath,
				MimeType = this.MimeType,
				AltText = this.AltText,
				Caption = this.Caption,
				FileSizeBytes = this.FileSizeBytes,
				ImageWidth = this.ImageWidth,
				ImageHeight = this.ImageHeight,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted
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
				Id = dto.Id,
				FileName = dto.FileName,
				FilePath = dto.FilePath,
				MimeType = dto.MimeType,
				AltText = dto.AltText,
				Caption = dto.Caption,
				FileSizeBytes = dto.FileSizeBytes,
				ImageWidth = dto.ImageWidth,
				ImageHeight = dto.ImageHeight,
				ObjectGuid = dto.ObjectGuid,
				Active = dto.Active ?? true,
				Deleted = dto.Deleted ?? false
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

			this.FileName = dto.FileName;
			this.FilePath = dto.FilePath;
			this.MimeType = dto.MimeType;
			this.AltText = dto.AltText;
			this.Caption = dto.Caption;
			this.FileSizeBytes = dto.FileSizeBytes;
			this.ImageWidth = dto.ImageWidth;
			this.ImageHeight = dto.ImageHeight;
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
		/// Creates a deep copy clone of a MediaAsset Object.
		///
		/// </summary>
		public MediaAsset Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new MediaAsset{
				Id = this.Id,
				FileName = this.FileName,
				FilePath = this.FilePath,
				MimeType = this.MimeType,
				AltText = this.AltText,
				Caption = this.Caption,
				FileSizeBytes = this.FileSizeBytes,
				ImageWidth = this.ImageWidth,
				ImageHeight = this.ImageHeight,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted
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
				Id = mediaAsset.Id,
				FileName = mediaAsset.FileName,
				FilePath = mediaAsset.FilePath,
				MimeType = mediaAsset.MimeType,
				AltText = mediaAsset.AltText,
				Caption = mediaAsset.Caption,
				FileSizeBytes = mediaAsset.FileSizeBytes,
				ImageWidth = mediaAsset.ImageWidth,
				ImageHeight = mediaAsset.ImageHeight,
				ObjectGuid = mediaAsset.ObjectGuid,
				Active = mediaAsset.Active,
				Deleted = mediaAsset.Deleted
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
				Id = mediaAsset.Id,
				FileName = mediaAsset.FileName,
				FilePath = mediaAsset.FilePath,
				MimeType = mediaAsset.MimeType,
				AltText = mediaAsset.AltText,
				Caption = mediaAsset.Caption,
				FileSizeBytes = mediaAsset.FileSizeBytes,
				ImageWidth = mediaAsset.ImageWidth,
				ImageHeight = mediaAsset.ImageHeight,
				ObjectGuid = mediaAsset.ObjectGuid,
				Active = mediaAsset.Active,
				Deleted = mediaAsset.Deleted
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
				name = mediaAsset.fileName,
				description = string.Join(", ", new[] { mediaAsset.fileName, mediaAsset.filePath, mediaAsset.mimeType}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
