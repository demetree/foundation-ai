using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.BMC.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class ModelDocumentChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)modelDocumentId; }
			set { modelDocumentId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ModelDocumentChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 modelDocumentId { get; set; }
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
		public class ModelDocumentChangeHistoryOutputDTO : ModelDocumentChangeHistoryDTO
		{
			public ModelDocument.ModelDocumentDTO modelDocument { get; set; }
		}


		/// <summary>
		///
		/// Converts a ModelDocumentChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ModelDocumentChangeHistoryDTO ToDTO()
		{
			return new ModelDocumentChangeHistoryDTO
			{
				id = this.id,
				modelDocumentId = this.modelDocumentId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ModelDocumentChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ModelDocumentChangeHistoryDTO> ToDTOList(List<ModelDocumentChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ModelDocumentChangeHistoryDTO> output = new List<ModelDocumentChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ModelDocumentChangeHistory modelDocumentChangeHistory in data)
			{
				output.Add(modelDocumentChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ModelDocumentChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ModelDocumentChangeHistoryEntity type directly.
		///
		/// </summary>
		public ModelDocumentChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ModelDocumentChangeHistoryOutputDTO
			{
				id = this.id,
				modelDocumentId = this.modelDocumentId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				modelDocument = this.modelDocument?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ModelDocumentChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ModelDocumentChangeHistory objects to avoid using the ModelDocumentChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ModelDocumentChangeHistoryOutputDTO> ToOutputDTOList(List<ModelDocumentChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ModelDocumentChangeHistoryOutputDTO> output = new List<ModelDocumentChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ModelDocumentChangeHistory modelDocumentChangeHistory in data)
			{
				output.Add(modelDocumentChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ModelDocumentChangeHistory Object.
		///
		/// </summary>
		public static Database.ModelDocumentChangeHistory FromDTO(ModelDocumentChangeHistoryDTO dto)
		{
			return new Database.ModelDocumentChangeHistory
			{
				id = dto.id,
				modelDocumentId = dto.modelDocumentId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ModelDocumentChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ModelDocumentChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.modelDocumentId = dto.modelDocumentId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ModelDocumentChangeHistory Object.
		///
		/// </summary>
		public ModelDocumentChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ModelDocumentChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				modelDocumentId = this.modelDocumentId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ModelDocumentChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ModelDocumentChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ModelDocumentChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ModelDocumentChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ModelDocumentChangeHistory modelDocumentChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (modelDocumentChangeHistory == null)
			{
				return null;
			}

			return new {
				id = modelDocumentChangeHistory.id,
				modelDocumentId = modelDocumentChangeHistory.modelDocumentId,
				versionNumber = modelDocumentChangeHistory.versionNumber,
				timeStamp = modelDocumentChangeHistory.timeStamp,
				userId = modelDocumentChangeHistory.userId,
				data = modelDocumentChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ModelDocumentChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ModelDocumentChangeHistory modelDocumentChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (modelDocumentChangeHistory == null)
			{
				return null;
			}

			return new {
				id = modelDocumentChangeHistory.id,
				modelDocumentId = modelDocumentChangeHistory.modelDocumentId,
				versionNumber = modelDocumentChangeHistory.versionNumber,
				timeStamp = modelDocumentChangeHistory.timeStamp,
				userId = modelDocumentChangeHistory.userId,
				data = modelDocumentChangeHistory.data,
				modelDocument = ModelDocument.CreateMinimalAnonymous(modelDocumentChangeHistory.modelDocument)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ModelDocumentChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ModelDocumentChangeHistory modelDocumentChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (modelDocumentChangeHistory == null)
			{
				return null;
			}

			return new {
				id = modelDocumentChangeHistory.id,
				name = modelDocumentChangeHistory.id,
				description = modelDocumentChangeHistory.id
			 };
		}
	}
}
