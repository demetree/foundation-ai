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
	public partial class DocumentTagChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)documentTagId; }
			set { documentTagId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class DocumentTagChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 documentTagId { get; set; }
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
		public class DocumentTagChangeHistoryOutputDTO : DocumentTagChangeHistoryDTO
		{
			public DocumentTag.DocumentTagDTO documentTag { get; set; }
		}


		/// <summary>
		///
		/// Converts a DocumentTagChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public DocumentTagChangeHistoryDTO ToDTO()
		{
			return new DocumentTagChangeHistoryDTO
			{
				id = this.id,
				documentTagId = this.documentTagId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a DocumentTagChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<DocumentTagChangeHistoryDTO> ToDTOList(List<DocumentTagChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<DocumentTagChangeHistoryDTO> output = new List<DocumentTagChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (DocumentTagChangeHistory documentTagChangeHistory in data)
			{
				output.Add(documentTagChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a DocumentTagChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the DocumentTagChangeHistory Entity type directly.
		///
		/// </summary>
		public DocumentTagChangeHistoryOutputDTO ToOutputDTO()
		{
			return new DocumentTagChangeHistoryOutputDTO
			{
				id = this.id,
				documentTagId = this.documentTagId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				documentTag = this.documentTag?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a DocumentTagChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of DocumentTagChangeHistory objects to avoid using the DocumentTagChangeHistory entity type directly.
		///
		/// </summary>
		public static List<DocumentTagChangeHistoryOutputDTO> ToOutputDTOList(List<DocumentTagChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<DocumentTagChangeHistoryOutputDTO> output = new List<DocumentTagChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (DocumentTagChangeHistory documentTagChangeHistory in data)
			{
				output.Add(documentTagChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a DocumentTagChangeHistory Object.
		///
		/// </summary>
		public static Database.DocumentTagChangeHistory FromDTO(DocumentTagChangeHistoryDTO dto)
		{
			return new Database.DocumentTagChangeHistory
			{
				id = dto.id,
				documentTagId = dto.documentTagId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a DocumentTagChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(DocumentTagChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.documentTagId = dto.documentTagId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a DocumentTagChangeHistory Object.
		///
		/// </summary>
		public DocumentTagChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new DocumentTagChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				documentTagId = this.documentTagId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a DocumentTagChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a DocumentTagChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a DocumentTagChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a DocumentTagChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.DocumentTagChangeHistory documentTagChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (documentTagChangeHistory == null)
			{
				return null;
			}

			return new {
				id = documentTagChangeHistory.id,
				documentTagId = documentTagChangeHistory.documentTagId,
				versionNumber = documentTagChangeHistory.versionNumber,
				timeStamp = documentTagChangeHistory.timeStamp,
				userId = documentTagChangeHistory.userId,
				data = documentTagChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a DocumentTagChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(DocumentTagChangeHistory documentTagChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (documentTagChangeHistory == null)
			{
				return null;
			}

			return new {
				id = documentTagChangeHistory.id,
				documentTagId = documentTagChangeHistory.documentTagId,
				versionNumber = documentTagChangeHistory.versionNumber,
				timeStamp = documentTagChangeHistory.timeStamp,
				userId = documentTagChangeHistory.userId,
				data = documentTagChangeHistory.data,
				documentTag = DocumentTag.CreateMinimalAnonymous(documentTagChangeHistory.documentTag)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a DocumentTagChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(DocumentTagChangeHistory documentTagChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (documentTagChangeHistory == null)
			{
				return null;
			}

			return new {
				id = documentTagChangeHistory.id,
				name = documentTagChangeHistory.id,
				description = documentTagChangeHistory.id
			 };
		}
	}
}
