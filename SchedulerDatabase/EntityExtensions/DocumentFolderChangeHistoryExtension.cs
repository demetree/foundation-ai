using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.Scheduler.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class DocumentFolderChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)documentFolderId; }
			set { documentFolderId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class DocumentFolderChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 documentFolderId { get; set; }
			public Int32 versionNumber { get; set; }
			[Required]
			public DateTime timeStamp { get; set; }
			[Required]
			public Int32 userId { get; set; }
			[Required]
			public String data { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class DocumentFolderChangeHistoryOutputDTO : DocumentFolderChangeHistoryDTO
		{
			public DocumentFolder.DocumentFolderDTO documentFolder { get; set; }
		}


		/// <summary>
		///
		/// Converts a DocumentFolderChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public DocumentFolderChangeHistoryDTO ToDTO()
		{
			return new DocumentFolderChangeHistoryDTO
			{
				id = this.id,
				documentFolderId = this.documentFolderId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a DocumentFolderChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<DocumentFolderChangeHistoryDTO> ToDTOList(List<DocumentFolderChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<DocumentFolderChangeHistoryDTO> output = new List<DocumentFolderChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (DocumentFolderChangeHistory documentFolderChangeHistory in data)
			{
				output.Add(documentFolderChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a DocumentFolderChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the DocumentFolderChangeHistory Entity type directly.
		///
		/// </summary>
		public DocumentFolderChangeHistoryOutputDTO ToOutputDTO()
		{
			return new DocumentFolderChangeHistoryOutputDTO
			{
				id = this.id,
				documentFolderId = this.documentFolderId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				documentFolder = this.documentFolder?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a DocumentFolderChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of DocumentFolderChangeHistory objects to avoid using the DocumentFolderChangeHistory entity type directly.
		///
		/// </summary>
		public static List<DocumentFolderChangeHistoryOutputDTO> ToOutputDTOList(List<DocumentFolderChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<DocumentFolderChangeHistoryOutputDTO> output = new List<DocumentFolderChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (DocumentFolderChangeHistory documentFolderChangeHistory in data)
			{
				output.Add(documentFolderChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a DocumentFolderChangeHistory Object.
		///
		/// </summary>
		public static Database.DocumentFolderChangeHistory FromDTO(DocumentFolderChangeHistoryDTO dto)
		{
			return new Database.DocumentFolderChangeHistory
			{
				id = dto.id,
				documentFolderId = dto.documentFolderId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a DocumentFolderChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(DocumentFolderChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.documentFolderId = dto.documentFolderId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a DocumentFolderChangeHistory Object.
		///
		/// </summary>
		public DocumentFolderChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new DocumentFolderChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				documentFolderId = this.documentFolderId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a DocumentFolderChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a DocumentFolderChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a DocumentFolderChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a DocumentFolderChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.DocumentFolderChangeHistory documentFolderChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (documentFolderChangeHistory == null)
			{
				return null;
			}

			return new {
				id = documentFolderChangeHistory.id,
				documentFolderId = documentFolderChangeHistory.documentFolderId,
				versionNumber = documentFolderChangeHistory.versionNumber,
				timeStamp = documentFolderChangeHistory.timeStamp,
				userId = documentFolderChangeHistory.userId,
				data = documentFolderChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a DocumentFolderChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(DocumentFolderChangeHistory documentFolderChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (documentFolderChangeHistory == null)
			{
				return null;
			}

			return new {
				id = documentFolderChangeHistory.id,
				documentFolderId = documentFolderChangeHistory.documentFolderId,
				versionNumber = documentFolderChangeHistory.versionNumber,
				timeStamp = documentFolderChangeHistory.timeStamp,
				userId = documentFolderChangeHistory.userId,
				data = documentFolderChangeHistory.data,
				documentFolder = DocumentFolder.CreateMinimalAnonymous(documentFolderChangeHistory.documentFolder),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a DocumentFolderChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(DocumentFolderChangeHistory documentFolderChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (documentFolderChangeHistory == null)
			{
				return null;
			}

			return new {
				id = documentFolderChangeHistory.id,
				name = documentFolderChangeHistory.id,
				description = documentFolderChangeHistory.id
			 };
		}
	}
}
