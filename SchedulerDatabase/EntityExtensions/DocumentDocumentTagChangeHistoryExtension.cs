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
	public partial class DocumentDocumentTagChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)documentDocumentTagId; }
			set { documentDocumentTagId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class DocumentDocumentTagChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 documentDocumentTagId { get; set; }
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
		public class DocumentDocumentTagChangeHistoryOutputDTO : DocumentDocumentTagChangeHistoryDTO
		{
			public DocumentDocumentTag.DocumentDocumentTagDTO documentDocumentTag { get; set; }
		}


		/// <summary>
		///
		/// Converts a DocumentDocumentTagChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public DocumentDocumentTagChangeHistoryDTO ToDTO()
		{
			return new DocumentDocumentTagChangeHistoryDTO
			{
				id = this.id,
				documentDocumentTagId = this.documentDocumentTagId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a DocumentDocumentTagChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<DocumentDocumentTagChangeHistoryDTO> ToDTOList(List<DocumentDocumentTagChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<DocumentDocumentTagChangeHistoryDTO> output = new List<DocumentDocumentTagChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (DocumentDocumentTagChangeHistory documentDocumentTagChangeHistory in data)
			{
				output.Add(documentDocumentTagChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a DocumentDocumentTagChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the DocumentDocumentTagChangeHistory Entity type directly.
		///
		/// </summary>
		public DocumentDocumentTagChangeHistoryOutputDTO ToOutputDTO()
		{
			return new DocumentDocumentTagChangeHistoryOutputDTO
			{
				id = this.id,
				documentDocumentTagId = this.documentDocumentTagId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				documentDocumentTag = this.documentDocumentTag?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a DocumentDocumentTagChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of DocumentDocumentTagChangeHistory objects to avoid using the DocumentDocumentTagChangeHistory entity type directly.
		///
		/// </summary>
		public static List<DocumentDocumentTagChangeHistoryOutputDTO> ToOutputDTOList(List<DocumentDocumentTagChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<DocumentDocumentTagChangeHistoryOutputDTO> output = new List<DocumentDocumentTagChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (DocumentDocumentTagChangeHistory documentDocumentTagChangeHistory in data)
			{
				output.Add(documentDocumentTagChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a DocumentDocumentTagChangeHistory Object.
		///
		/// </summary>
		public static Database.DocumentDocumentTagChangeHistory FromDTO(DocumentDocumentTagChangeHistoryDTO dto)
		{
			return new Database.DocumentDocumentTagChangeHistory
			{
				id = dto.id,
				documentDocumentTagId = dto.documentDocumentTagId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a DocumentDocumentTagChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(DocumentDocumentTagChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.documentDocumentTagId = dto.documentDocumentTagId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a DocumentDocumentTagChangeHistory Object.
		///
		/// </summary>
		public DocumentDocumentTagChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new DocumentDocumentTagChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				documentDocumentTagId = this.documentDocumentTagId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a DocumentDocumentTagChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a DocumentDocumentTagChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a DocumentDocumentTagChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a DocumentDocumentTagChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.DocumentDocumentTagChangeHistory documentDocumentTagChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (documentDocumentTagChangeHistory == null)
			{
				return null;
			}

			return new {
				id = documentDocumentTagChangeHistory.id,
				documentDocumentTagId = documentDocumentTagChangeHistory.documentDocumentTagId,
				versionNumber = documentDocumentTagChangeHistory.versionNumber,
				timeStamp = documentDocumentTagChangeHistory.timeStamp,
				userId = documentDocumentTagChangeHistory.userId,
				data = documentDocumentTagChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a DocumentDocumentTagChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(DocumentDocumentTagChangeHistory documentDocumentTagChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (documentDocumentTagChangeHistory == null)
			{
				return null;
			}

			return new {
				id = documentDocumentTagChangeHistory.id,
				documentDocumentTagId = documentDocumentTagChangeHistory.documentDocumentTagId,
				versionNumber = documentDocumentTagChangeHistory.versionNumber,
				timeStamp = documentDocumentTagChangeHistory.timeStamp,
				userId = documentDocumentTagChangeHistory.userId,
				data = documentDocumentTagChangeHistory.data,
				documentDocumentTag = DocumentDocumentTag.CreateMinimalAnonymous(documentDocumentTagChangeHistory.documentDocumentTag)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a DocumentDocumentTagChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(DocumentDocumentTagChangeHistory documentDocumentTagChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (documentDocumentTagChangeHistory == null)
			{
				return null;
			}

			return new {
				id = documentDocumentTagChangeHistory.id,
				name = documentDocumentTagChangeHistory.id,
				description = documentDocumentTagChangeHistory.id
			 };
		}
	}
}
