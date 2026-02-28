using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.BMC.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class ModelSubFile : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ModelSubFileDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 modelDocumentId { get; set; }
			[Required]
			public String fileName { get; set; }
			[Required]
			public Boolean isMainModel { get; set; }
			public Int32? parentModelSubFileId { get; set; }
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
		public class ModelSubFileOutputDTO : ModelSubFileDTO
		{
			public ModelDocument.ModelDocumentDTO modelDocument { get; set; }
			public ModelSubFile.ModelSubFileDTO parentModelSubFile { get; set; }
		}


		/// <summary>
		///
		/// Converts a ModelSubFile to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ModelSubFileDTO ToDTO()
		{
			return new ModelSubFileDTO
			{
				id = this.id,
				modelDocumentId = this.modelDocumentId,
				fileName = this.fileName,
				isMainModel = this.isMainModel,
				parentModelSubFileId = this.parentModelSubFileId,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ModelSubFile list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ModelSubFileDTO> ToDTOList(List<ModelSubFile> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ModelSubFileDTO> output = new List<ModelSubFileDTO>();

			output.Capacity = data.Count;

			foreach (ModelSubFile modelSubFile in data)
			{
				output.Add(modelSubFile.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ModelSubFile to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ModelSubFileEntity type directly.
		///
		/// </summary>
		public ModelSubFileOutputDTO ToOutputDTO()
		{
			return new ModelSubFileOutputDTO
			{
				id = this.id,
				modelDocumentId = this.modelDocumentId,
				fileName = this.fileName,
				isMainModel = this.isMainModel,
				parentModelSubFileId = this.parentModelSubFileId,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				modelDocument = this.modelDocument?.ToDTO(),
				parentModelSubFile = this.parentModelSubFile?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ModelSubFile list to list of Output Data Transfer Object intended to be used for serializing a list of ModelSubFile objects to avoid using the ModelSubFile entity type directly.
		///
		/// </summary>
		public static List<ModelSubFileOutputDTO> ToOutputDTOList(List<ModelSubFile> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ModelSubFileOutputDTO> output = new List<ModelSubFileOutputDTO>();

			output.Capacity = data.Count;

			foreach (ModelSubFile modelSubFile in data)
			{
				output.Add(modelSubFile.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ModelSubFile Object.
		///
		/// </summary>
		public static Database.ModelSubFile FromDTO(ModelSubFileDTO dto)
		{
			return new Database.ModelSubFile
			{
				id = dto.id,
				modelDocumentId = dto.modelDocumentId,
				fileName = dto.fileName,
				isMainModel = dto.isMainModel,
				parentModelSubFileId = dto.parentModelSubFileId,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ModelSubFile Object.
		///
		/// </summary>
		public void ApplyDTO(ModelSubFileDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.modelDocumentId = dto.modelDocumentId;
			this.fileName = dto.fileName;
			this.isMainModel = dto.isMainModel;
			this.parentModelSubFileId = dto.parentModelSubFileId;
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
		/// Creates a deep copy clone of a ModelSubFile Object.
		///
		/// </summary>
		public ModelSubFile Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ModelSubFile{
				id = this.id,
				tenantGuid = this.tenantGuid,
				modelDocumentId = this.modelDocumentId,
				fileName = this.fileName,
				isMainModel = this.isMainModel,
				parentModelSubFileId = this.parentModelSubFileId,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ModelSubFile Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ModelSubFile Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ModelSubFile Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ModelSubFile Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ModelSubFile modelSubFile)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (modelSubFile == null)
			{
				return null;
			}

			return new {
				id = modelSubFile.id,
				modelDocumentId = modelSubFile.modelDocumentId,
				fileName = modelSubFile.fileName,
				isMainModel = modelSubFile.isMainModel,
				parentModelSubFileId = modelSubFile.parentModelSubFileId,
				sequence = modelSubFile.sequence,
				objectGuid = modelSubFile.objectGuid,
				active = modelSubFile.active,
				deleted = modelSubFile.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ModelSubFile Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ModelSubFile modelSubFile)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (modelSubFile == null)
			{
				return null;
			}

			return new {
				id = modelSubFile.id,
				modelDocumentId = modelSubFile.modelDocumentId,
				fileName = modelSubFile.fileName,
				isMainModel = modelSubFile.isMainModel,
				parentModelSubFileId = modelSubFile.parentModelSubFileId,
				sequence = modelSubFile.sequence,
				objectGuid = modelSubFile.objectGuid,
				active = modelSubFile.active,
				deleted = modelSubFile.deleted,
				modelDocument = ModelDocument.CreateMinimalAnonymous(modelSubFile.modelDocument),
				parentModelSubFile = ModelSubFile.CreateMinimalAnonymous(modelSubFile.parentModelSubFile)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ModelSubFile Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ModelSubFile modelSubFile)
		{
			//
			// Return a very minimal object.
			//
			if (modelSubFile == null)
			{
				return null;
			}

			return new {
				id = modelSubFile.id,
				name = modelSubFile.fileName,
				description = string.Join(", ", new[] { modelSubFile.fileName}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
