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
	public partial class DocumentShareLinkChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)documentShareLinkId; }
			set { documentShareLinkId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class DocumentShareLinkChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 documentShareLinkId { get; set; }
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
		public class DocumentShareLinkChangeHistoryOutputDTO : DocumentShareLinkChangeHistoryDTO
		{
			public DocumentShareLink.DocumentShareLinkDTO documentShareLink { get; set; }
		}


		/// <summary>
		///
		/// Converts a DocumentShareLinkChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public DocumentShareLinkChangeHistoryDTO ToDTO()
		{
			return new DocumentShareLinkChangeHistoryDTO
			{
				id = this.id,
				documentShareLinkId = this.documentShareLinkId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a DocumentShareLinkChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<DocumentShareLinkChangeHistoryDTO> ToDTOList(List<DocumentShareLinkChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<DocumentShareLinkChangeHistoryDTO> output = new List<DocumentShareLinkChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (DocumentShareLinkChangeHistory documentShareLinkChangeHistory in data)
			{
				output.Add(documentShareLinkChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a DocumentShareLinkChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the DocumentShareLinkChangeHistory Entity type directly.
		///
		/// </summary>
		public DocumentShareLinkChangeHistoryOutputDTO ToOutputDTO()
		{
			return new DocumentShareLinkChangeHistoryOutputDTO
			{
				id = this.id,
				documentShareLinkId = this.documentShareLinkId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				documentShareLink = this.documentShareLink?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a DocumentShareLinkChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of DocumentShareLinkChangeHistory objects to avoid using the DocumentShareLinkChangeHistory entity type directly.
		///
		/// </summary>
		public static List<DocumentShareLinkChangeHistoryOutputDTO> ToOutputDTOList(List<DocumentShareLinkChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<DocumentShareLinkChangeHistoryOutputDTO> output = new List<DocumentShareLinkChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (DocumentShareLinkChangeHistory documentShareLinkChangeHistory in data)
			{
				output.Add(documentShareLinkChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a DocumentShareLinkChangeHistory Object.
		///
		/// </summary>
		public static Database.DocumentShareLinkChangeHistory FromDTO(DocumentShareLinkChangeHistoryDTO dto)
		{
			return new Database.DocumentShareLinkChangeHistory
			{
				id = dto.id,
				documentShareLinkId = dto.documentShareLinkId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a DocumentShareLinkChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(DocumentShareLinkChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.documentShareLinkId = dto.documentShareLinkId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a DocumentShareLinkChangeHistory Object.
		///
		/// </summary>
		public DocumentShareLinkChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new DocumentShareLinkChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				documentShareLinkId = this.documentShareLinkId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a DocumentShareLinkChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a DocumentShareLinkChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a DocumentShareLinkChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a DocumentShareLinkChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.DocumentShareLinkChangeHistory documentShareLinkChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (documentShareLinkChangeHistory == null)
			{
				return null;
			}

			return new {
				id = documentShareLinkChangeHistory.id,
				documentShareLinkId = documentShareLinkChangeHistory.documentShareLinkId,
				versionNumber = documentShareLinkChangeHistory.versionNumber,
				timeStamp = documentShareLinkChangeHistory.timeStamp,
				userId = documentShareLinkChangeHistory.userId,
				data = documentShareLinkChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a DocumentShareLinkChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(DocumentShareLinkChangeHistory documentShareLinkChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (documentShareLinkChangeHistory == null)
			{
				return null;
			}

			return new {
				id = documentShareLinkChangeHistory.id,
				documentShareLinkId = documentShareLinkChangeHistory.documentShareLinkId,
				versionNumber = documentShareLinkChangeHistory.versionNumber,
				timeStamp = documentShareLinkChangeHistory.timeStamp,
				userId = documentShareLinkChangeHistory.userId,
				data = documentShareLinkChangeHistory.data,
				documentShareLink = DocumentShareLink.CreateMinimalAnonymous(documentShareLinkChangeHistory.documentShareLink),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a DocumentShareLinkChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(DocumentShareLinkChangeHistory documentShareLinkChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (documentShareLinkChangeHistory == null)
			{
				return null;
			}

			return new {
				id = documentShareLinkChangeHistory.id,
				name = documentShareLinkChangeHistory.id,
				description = documentShareLinkChangeHistory.id
			 };
		}
	}
}
