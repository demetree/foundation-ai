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
	public partial class DocumentDownload : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class DocumentDownloadDTO
		{
			public Int32 Id { get; set; }
			public String Title { get; set; }
			public String Description { get; set; }
			public String FilePath { get; set; }
			public String FileName { get; set; }
			public String MimeType { get; set; }
			public Int64? FileSizeBytes { get; set; }
			public String CategoryName { get; set; }
			public DateTime? DocumentDate { get; set; }
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
		public class DocumentDownloadOutputDTO : DocumentDownloadDTO
		{
		}


		/// <summary>
		///
		/// Converts a DocumentDownload to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public DocumentDownloadDTO ToDTO()
		{
			return new DocumentDownloadDTO
			{
				Id = this.Id,
				Title = this.Title,
				Description = this.Description,
				FilePath = this.FilePath,
				FileName = this.FileName,
				MimeType = this.MimeType,
				FileSizeBytes = this.FileSizeBytes,
				CategoryName = this.CategoryName,
				DocumentDate = this.DocumentDate,
				IsPublished = this.IsPublished,
				Sequence = this.Sequence,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted
			};
		}


		/// <summary>
		///
		/// Converts a DocumentDownload list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<DocumentDownloadDTO> ToDTOList(List<DocumentDownload> data)
		{
			if (data == null)
			{
				return null;
			}

			List<DocumentDownloadDTO> output = new List<DocumentDownloadDTO>();

			output.Capacity = data.Count;

			foreach (DocumentDownload documentDownload in data)
			{
				output.Add(documentDownload.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a DocumentDownload to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the DocumentDownloadEntity type directly.
		///
		/// </summary>
		public DocumentDownloadOutputDTO ToOutputDTO()
		{
			return new DocumentDownloadOutputDTO
			{
				Id = this.Id,
				Title = this.Title,
				Description = this.Description,
				FilePath = this.FilePath,
				FileName = this.FileName,
				MimeType = this.MimeType,
				FileSizeBytes = this.FileSizeBytes,
				CategoryName = this.CategoryName,
				DocumentDate = this.DocumentDate,
				IsPublished = this.IsPublished,
				Sequence = this.Sequence,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted
			};
		}


		/// <summary>
		///
		/// Converts a DocumentDownload list to list of Output Data Transfer Object intended to be used for serializing a list of DocumentDownload objects to avoid using the DocumentDownload entity type directly.
		///
		/// </summary>
		public static List<DocumentDownloadOutputDTO> ToOutputDTOList(List<DocumentDownload> data)
		{
			if (data == null)
			{
				return null;
			}

			List<DocumentDownloadOutputDTO> output = new List<DocumentDownloadOutputDTO>();

			output.Capacity = data.Count;

			foreach (DocumentDownload documentDownload in data)
			{
				output.Add(documentDownload.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a DocumentDownload Object.
		///
		/// </summary>
		public static Database.DocumentDownload FromDTO(DocumentDownloadDTO dto)
		{
			return new Database.DocumentDownload
			{
				Id = dto.Id,
				Title = dto.Title,
				Description = dto.Description,
				FilePath = dto.FilePath,
				FileName = dto.FileName,
				MimeType = dto.MimeType,
				FileSizeBytes = dto.FileSizeBytes,
				CategoryName = dto.CategoryName,
				DocumentDate = dto.DocumentDate,
				IsPublished = dto.IsPublished,
				Sequence = dto.Sequence,
				ObjectGuid = dto.ObjectGuid,
				Active = dto.Active ?? true,
				Deleted = dto.Deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a DocumentDownload Object.
		///
		/// </summary>
		public void ApplyDTO(DocumentDownloadDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.Title = dto.Title;
			this.Description = dto.Description;
			this.FilePath = dto.FilePath;
			this.FileName = dto.FileName;
			this.MimeType = dto.MimeType;
			this.FileSizeBytes = dto.FileSizeBytes;
			this.CategoryName = dto.CategoryName;
			this.DocumentDate = dto.DocumentDate;
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
		/// Creates a deep copy clone of a DocumentDownload Object.
		///
		/// </summary>
		public DocumentDownload Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new DocumentDownload{
				Id = this.Id,
				Title = this.Title,
				Description = this.Description,
				FilePath = this.FilePath,
				FileName = this.FileName,
				MimeType = this.MimeType,
				FileSizeBytes = this.FileSizeBytes,
				CategoryName = this.CategoryName,
				DocumentDate = this.DocumentDate,
				IsPublished = this.IsPublished,
				Sequence = this.Sequence,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a DocumentDownload Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a DocumentDownload Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a DocumentDownload Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a DocumentDownload Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.DocumentDownload documentDownload)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (documentDownload == null)
			{
				return null;
			}

			return new {
				Id = documentDownload.Id,
				Title = documentDownload.Title,
				Description = documentDownload.Description,
				FilePath = documentDownload.FilePath,
				FileName = documentDownload.FileName,
				MimeType = documentDownload.MimeType,
				FileSizeBytes = documentDownload.FileSizeBytes,
				CategoryName = documentDownload.CategoryName,
				DocumentDate = documentDownload.DocumentDate,
				IsPublished = documentDownload.IsPublished,
				Sequence = documentDownload.Sequence,
				ObjectGuid = documentDownload.ObjectGuid,
				Active = documentDownload.Active,
				Deleted = documentDownload.Deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a DocumentDownload Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(DocumentDownload documentDownload)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (documentDownload == null)
			{
				return null;
			}

			return new {
				Id = documentDownload.Id,
				Title = documentDownload.Title,
				Description = documentDownload.Description,
				FilePath = documentDownload.FilePath,
				FileName = documentDownload.FileName,
				MimeType = documentDownload.MimeType,
				FileSizeBytes = documentDownload.FileSizeBytes,
				CategoryName = documentDownload.CategoryName,
				DocumentDate = documentDownload.DocumentDate,
				IsPublished = documentDownload.IsPublished,
				Sequence = documentDownload.Sequence,
				ObjectGuid = documentDownload.ObjectGuid,
				Active = documentDownload.Active,
				Deleted = documentDownload.Deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a DocumentDownload Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(DocumentDownload documentDownload)
		{
			//
			// Return a very minimal object.
			//
			if (documentDownload == null)
			{
				return null;
			}

			return new {
				name = documentDownload.title,
				description = string.Join(", ", new[] { documentDownload.title, documentDownload.filePath, documentDownload.fileName}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
