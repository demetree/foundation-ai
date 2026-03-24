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
	public partial class DocumentChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)documentId; }
			set { documentId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class DocumentChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 documentId { get; set; }
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
		public class DocumentChangeHistoryOutputDTO : DocumentChangeHistoryDTO
		{
			public Document.DocumentDTO document { get; set; }
		}


		/// <summary>
		///
		/// Converts a DocumentChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public DocumentChangeHistoryDTO ToDTO()
		{
			return new DocumentChangeHistoryDTO
			{
				id = this.id,
				documentId = this.documentId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a DocumentChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<DocumentChangeHistoryDTO> ToDTOList(List<DocumentChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<DocumentChangeHistoryDTO> output = new List<DocumentChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (DocumentChangeHistory documentChangeHistory in data)
			{
				output.Add(documentChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a DocumentChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the DocumentChangeHistory Entity type directly.
		///
		/// </summary>
		public DocumentChangeHistoryOutputDTO ToOutputDTO()
		{
			return new DocumentChangeHistoryOutputDTO
			{
				id = this.id,
				documentId = this.documentId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				document = this.document?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a DocumentChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of DocumentChangeHistory objects to avoid using the DocumentChangeHistory entity type directly.
		///
		/// </summary>
		public static List<DocumentChangeHistoryOutputDTO> ToOutputDTOList(List<DocumentChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<DocumentChangeHistoryOutputDTO> output = new List<DocumentChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (DocumentChangeHistory documentChangeHistory in data)
			{
				output.Add(documentChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a DocumentChangeHistory Object.
		///
		/// </summary>
		public static Database.DocumentChangeHistory FromDTO(DocumentChangeHistoryDTO dto)
		{
			return new Database.DocumentChangeHistory
			{
				id = dto.id,
				documentId = dto.documentId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a DocumentChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(DocumentChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.documentId = dto.documentId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a DocumentChangeHistory Object.
		///
		/// </summary>
		public DocumentChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new DocumentChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				documentId = this.documentId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a DocumentChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a DocumentChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a DocumentChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a DocumentChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.DocumentChangeHistory documentChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (documentChangeHistory == null)
			{
				return null;
			}

			return new {
				id = documentChangeHistory.id,
				documentId = documentChangeHistory.documentId,
				versionNumber = documentChangeHistory.versionNumber,
				timeStamp = documentChangeHistory.timeStamp,
				userId = documentChangeHistory.userId,
				data = documentChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a DocumentChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(DocumentChangeHistory documentChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (documentChangeHistory == null)
			{
				return null;
			}

			return new {
				id = documentChangeHistory.id,
				documentId = documentChangeHistory.documentId,
				versionNumber = documentChangeHistory.versionNumber,
				timeStamp = documentChangeHistory.timeStamp,
				userId = documentChangeHistory.userId,
				data = documentChangeHistory.data,
				document = Document.CreateMinimalAnonymous(documentChangeHistory.document)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a DocumentChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(DocumentChangeHistory documentChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (documentChangeHistory == null)
			{
				return null;
			}

			return new {
				id = documentChangeHistory.id,
				name = documentChangeHistory.id,
				description = documentChangeHistory.id
			 };
		}
	}
}
