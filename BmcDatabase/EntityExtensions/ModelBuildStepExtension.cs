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
	public partial class ModelBuildStep : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ModelBuildStepDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 modelSubFileId { get; set; }
			[Required]
			public Int32 stepNumber { get; set; }
			public String rotationType { get; set; }
			public Single? rotationX { get; set; }
			public Single? rotationY { get; set; }
			public Single? rotationZ { get; set; }
			public String description { get; set; }
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
		public class ModelBuildStepOutputDTO : ModelBuildStepDTO
		{
			public ModelSubFile.ModelSubFileDTO modelSubFile { get; set; }
		}


		/// <summary>
		///
		/// Converts a ModelBuildStep to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ModelBuildStepDTO ToDTO()
		{
			return new ModelBuildStepDTO
			{
				id = this.id,
				modelSubFileId = this.modelSubFileId,
				stepNumber = this.stepNumber,
				rotationType = this.rotationType,
				rotationX = this.rotationX,
				rotationY = this.rotationY,
				rotationZ = this.rotationZ,
				description = this.description,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ModelBuildStep list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ModelBuildStepDTO> ToDTOList(List<ModelBuildStep> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ModelBuildStepDTO> output = new List<ModelBuildStepDTO>();

			output.Capacity = data.Count;

			foreach (ModelBuildStep modelBuildStep in data)
			{
				output.Add(modelBuildStep.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ModelBuildStep to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ModelBuildStepEntity type directly.
		///
		/// </summary>
		public ModelBuildStepOutputDTO ToOutputDTO()
		{
			return new ModelBuildStepOutputDTO
			{
				id = this.id,
				modelSubFileId = this.modelSubFileId,
				stepNumber = this.stepNumber,
				rotationType = this.rotationType,
				rotationX = this.rotationX,
				rotationY = this.rotationY,
				rotationZ = this.rotationZ,
				description = this.description,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				modelSubFile = this.modelSubFile?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ModelBuildStep list to list of Output Data Transfer Object intended to be used for serializing a list of ModelBuildStep objects to avoid using the ModelBuildStep entity type directly.
		///
		/// </summary>
		public static List<ModelBuildStepOutputDTO> ToOutputDTOList(List<ModelBuildStep> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ModelBuildStepOutputDTO> output = new List<ModelBuildStepOutputDTO>();

			output.Capacity = data.Count;

			foreach (ModelBuildStep modelBuildStep in data)
			{
				output.Add(modelBuildStep.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ModelBuildStep Object.
		///
		/// </summary>
		public static Database.ModelBuildStep FromDTO(ModelBuildStepDTO dto)
		{
			return new Database.ModelBuildStep
			{
				id = dto.id,
				modelSubFileId = dto.modelSubFileId,
				stepNumber = dto.stepNumber,
				rotationType = dto.rotationType,
				rotationX = dto.rotationX,
				rotationY = dto.rotationY,
				rotationZ = dto.rotationZ,
				description = dto.description,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ModelBuildStep Object.
		///
		/// </summary>
		public void ApplyDTO(ModelBuildStepDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.modelSubFileId = dto.modelSubFileId;
			this.stepNumber = dto.stepNumber;
			this.rotationType = dto.rotationType;
			this.rotationX = dto.rotationX;
			this.rotationY = dto.rotationY;
			this.rotationZ = dto.rotationZ;
			this.description = dto.description;
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
		/// Creates a deep copy clone of a ModelBuildStep Object.
		///
		/// </summary>
		public ModelBuildStep Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ModelBuildStep{
				id = this.id,
				tenantGuid = this.tenantGuid,
				modelSubFileId = this.modelSubFileId,
				stepNumber = this.stepNumber,
				rotationType = this.rotationType,
				rotationX = this.rotationX,
				rotationY = this.rotationY,
				rotationZ = this.rotationZ,
				description = this.description,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ModelBuildStep Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ModelBuildStep Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ModelBuildStep Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ModelBuildStep Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ModelBuildStep modelBuildStep)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (modelBuildStep == null)
			{
				return null;
			}

			return new {
				id = modelBuildStep.id,
				modelSubFileId = modelBuildStep.modelSubFileId,
				stepNumber = modelBuildStep.stepNumber,
				rotationType = modelBuildStep.rotationType,
				rotationX = modelBuildStep.rotationX,
				rotationY = modelBuildStep.rotationY,
				rotationZ = modelBuildStep.rotationZ,
				description = modelBuildStep.description,
				objectGuid = modelBuildStep.objectGuid,
				active = modelBuildStep.active,
				deleted = modelBuildStep.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ModelBuildStep Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ModelBuildStep modelBuildStep)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (modelBuildStep == null)
			{
				return null;
			}

			return new {
				id = modelBuildStep.id,
				modelSubFileId = modelBuildStep.modelSubFileId,
				stepNumber = modelBuildStep.stepNumber,
				rotationType = modelBuildStep.rotationType,
				rotationX = modelBuildStep.rotationX,
				rotationY = modelBuildStep.rotationY,
				rotationZ = modelBuildStep.rotationZ,
				description = modelBuildStep.description,
				objectGuid = modelBuildStep.objectGuid,
				active = modelBuildStep.active,
				deleted = modelBuildStep.deleted,
				modelSubFile = ModelSubFile.CreateMinimalAnonymous(modelBuildStep.modelSubFile)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ModelBuildStep Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ModelBuildStep modelBuildStep)
		{
			//
			// Return a very minimal object.
			//
			if (modelBuildStep == null)
			{
				return null;
			}

			return new {
				id = modelBuildStep.id,
				description = modelBuildStep.description,
				name = modelBuildStep.rotationType
			 };
		}
	}
}
