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
			public Int32 id { get; set; }
			[Required]
			public String title { get; set; }
			public String description { get; set; }
			[Required]
			public String filePath { get; set; }
			[Required]
			public String fileName { get; set; }
			public String mimeType { get; set; }
			public Int64? fileSizeBytes { get; set; }
			public String categoryName { get; set; }
			public DateTime? documentDate { get; set; }
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
				id = this.id,
				title = this.title,
				description = this.description,
				filePath = this.filePath,
				fileName = this.fileName,
				mimeType = this.mimeType,
				fileSizeBytes = this.fileSizeBytes,
				categoryName = this.categoryName,
				documentDate = this.documentDate,
				isPublished = this.isPublished,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
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
				id = this.id,
				title = this.title,
				description = this.description,
				filePath = this.filePath,
				fileName = this.fileName,
				mimeType = this.mimeType,
				fileSizeBytes = this.fileSizeBytes,
				categoryName = this.categoryName,
				documentDate = this.documentDate,
				isPublished = this.isPublished,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
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
				id = dto.id,
				title = dto.title,
				description = dto.description,
				filePath = dto.filePath,
				fileName = dto.fileName,
				mimeType = dto.mimeType,
				fileSizeBytes = dto.fileSizeBytes,
				categoryName = dto.categoryName,
				documentDate = dto.documentDate,
				isPublished = dto.isPublished,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
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

			this.title = dto.title;
			this.description = dto.description;
			this.filePath = dto.filePath;
			this.fileName = dto.fileName;
			this.mimeType = dto.mimeType;
			this.fileSizeBytes = dto.fileSizeBytes;
			this.categoryName = dto.categoryName;
			this.documentDate = dto.documentDate;
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
		/// Creates a deep copy clone of a DocumentDownload Object.
		///
		/// </summary>
		public DocumentDownload Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new DocumentDownload{
				id = this.id,
				tenantGuid = this.tenantGuid,
				title = this.title,
				description = this.description,
				filePath = this.filePath,
				fileName = this.fileName,
				mimeType = this.mimeType,
				fileSizeBytes = this.fileSizeBytes,
				categoryName = this.categoryName,
				documentDate = this.documentDate,
				isPublished = this.isPublished,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
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
				id = documentDownload.id,
				title = documentDownload.title,
				description = documentDownload.description,
				filePath = documentDownload.filePath,
				fileName = documentDownload.fileName,
				mimeType = documentDownload.mimeType,
				fileSizeBytes = documentDownload.fileSizeBytes,
				categoryName = documentDownload.categoryName,
				documentDate = documentDownload.documentDate,
				isPublished = documentDownload.isPublished,
				sequence = documentDownload.sequence,
				objectGuid = documentDownload.objectGuid,
				active = documentDownload.active,
				deleted = documentDownload.deleted
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
				id = documentDownload.id,
				title = documentDownload.title,
				description = documentDownload.description,
				filePath = documentDownload.filePath,
				fileName = documentDownload.fileName,
				mimeType = documentDownload.mimeType,
				fileSizeBytes = documentDownload.fileSizeBytes,
				categoryName = documentDownload.categoryName,
				documentDate = documentDownload.documentDate,
				isPublished = documentDownload.isPublished,
				sequence = documentDownload.sequence,
				objectGuid = documentDownload.objectGuid,
				active = documentDownload.active,
				deleted = documentDownload.deleted
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
				id = documentDownload.id,
				description = documentDownload.description,
				name = documentDownload.title
			 };
		}
	}
}
