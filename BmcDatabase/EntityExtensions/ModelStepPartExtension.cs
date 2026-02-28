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
	public partial class ModelStepPart : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ModelStepPartDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 modelBuildStepId { get; set; }
			public Int32? brickPartId { get; set; }
			public Int32? brickColourId { get; set; }
			[Required]
			public String partFileName { get; set; }
			[Required]
			public Int32 colorCode { get; set; }
			public Single? positionX { get; set; }
			public Single? positionY { get; set; }
			public Single? positionZ { get; set; }
			[Required]
			public String transformMatrix { get; set; }
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
		public class ModelStepPartOutputDTO : ModelStepPartDTO
		{
			public BrickColour.BrickColourDTO brickColour { get; set; }
			public BrickPart.BrickPartDTO brickPart { get; set; }
			public ModelBuildStep.ModelBuildStepDTO modelBuildStep { get; set; }
		}


		/// <summary>
		///
		/// Converts a ModelStepPart to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ModelStepPartDTO ToDTO()
		{
			return new ModelStepPartDTO
			{
				id = this.id,
				modelBuildStepId = this.modelBuildStepId,
				brickPartId = this.brickPartId,
				brickColourId = this.brickColourId,
				partFileName = this.partFileName,
				colorCode = this.colorCode,
				positionX = this.positionX,
				positionY = this.positionY,
				positionZ = this.positionZ,
				transformMatrix = this.transformMatrix,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ModelStepPart list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ModelStepPartDTO> ToDTOList(List<ModelStepPart> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ModelStepPartDTO> output = new List<ModelStepPartDTO>();

			output.Capacity = data.Count;

			foreach (ModelStepPart modelStepPart in data)
			{
				output.Add(modelStepPart.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ModelStepPart to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ModelStepPartEntity type directly.
		///
		/// </summary>
		public ModelStepPartOutputDTO ToOutputDTO()
		{
			return new ModelStepPartOutputDTO
			{
				id = this.id,
				modelBuildStepId = this.modelBuildStepId,
				brickPartId = this.brickPartId,
				brickColourId = this.brickColourId,
				partFileName = this.partFileName,
				colorCode = this.colorCode,
				positionX = this.positionX,
				positionY = this.positionY,
				positionZ = this.positionZ,
				transformMatrix = this.transformMatrix,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				brickColour = this.brickColour?.ToDTO(),
				brickPart = this.brickPart?.ToDTO(),
				modelBuildStep = this.modelBuildStep?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ModelStepPart list to list of Output Data Transfer Object intended to be used for serializing a list of ModelStepPart objects to avoid using the ModelStepPart entity type directly.
		///
		/// </summary>
		public static List<ModelStepPartOutputDTO> ToOutputDTOList(List<ModelStepPart> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ModelStepPartOutputDTO> output = new List<ModelStepPartOutputDTO>();

			output.Capacity = data.Count;

			foreach (ModelStepPart modelStepPart in data)
			{
				output.Add(modelStepPart.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ModelStepPart Object.
		///
		/// </summary>
		public static Database.ModelStepPart FromDTO(ModelStepPartDTO dto)
		{
			return new Database.ModelStepPart
			{
				id = dto.id,
				modelBuildStepId = dto.modelBuildStepId,
				brickPartId = dto.brickPartId,
				brickColourId = dto.brickColourId,
				partFileName = dto.partFileName,
				colorCode = dto.colorCode,
				positionX = dto.positionX,
				positionY = dto.positionY,
				positionZ = dto.positionZ,
				transformMatrix = dto.transformMatrix,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ModelStepPart Object.
		///
		/// </summary>
		public void ApplyDTO(ModelStepPartDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.modelBuildStepId = dto.modelBuildStepId;
			this.brickPartId = dto.brickPartId;
			this.brickColourId = dto.brickColourId;
			this.partFileName = dto.partFileName;
			this.colorCode = dto.colorCode;
			this.positionX = dto.positionX;
			this.positionY = dto.positionY;
			this.positionZ = dto.positionZ;
			this.transformMatrix = dto.transformMatrix;
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
		/// Creates a deep copy clone of a ModelStepPart Object.
		///
		/// </summary>
		public ModelStepPart Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ModelStepPart{
				id = this.id,
				tenantGuid = this.tenantGuid,
				modelBuildStepId = this.modelBuildStepId,
				brickPartId = this.brickPartId,
				brickColourId = this.brickColourId,
				partFileName = this.partFileName,
				colorCode = this.colorCode,
				positionX = this.positionX,
				positionY = this.positionY,
				positionZ = this.positionZ,
				transformMatrix = this.transformMatrix,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ModelStepPart Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ModelStepPart Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ModelStepPart Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ModelStepPart Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ModelStepPart modelStepPart)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (modelStepPart == null)
			{
				return null;
			}

			return new {
				id = modelStepPart.id,
				modelBuildStepId = modelStepPart.modelBuildStepId,
				brickPartId = modelStepPart.brickPartId,
				brickColourId = modelStepPart.brickColourId,
				partFileName = modelStepPart.partFileName,
				colorCode = modelStepPart.colorCode,
				positionX = modelStepPart.positionX,
				positionY = modelStepPart.positionY,
				positionZ = modelStepPart.positionZ,
				transformMatrix = modelStepPart.transformMatrix,
				sequence = modelStepPart.sequence,
				objectGuid = modelStepPart.objectGuid,
				active = modelStepPart.active,
				deleted = modelStepPart.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ModelStepPart Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ModelStepPart modelStepPart)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (modelStepPart == null)
			{
				return null;
			}

			return new {
				id = modelStepPart.id,
				modelBuildStepId = modelStepPart.modelBuildStepId,
				brickPartId = modelStepPart.brickPartId,
				brickColourId = modelStepPart.brickColourId,
				partFileName = modelStepPart.partFileName,
				colorCode = modelStepPart.colorCode,
				positionX = modelStepPart.positionX,
				positionY = modelStepPart.positionY,
				positionZ = modelStepPart.positionZ,
				transformMatrix = modelStepPart.transformMatrix,
				sequence = modelStepPart.sequence,
				objectGuid = modelStepPart.objectGuid,
				active = modelStepPart.active,
				deleted = modelStepPart.deleted,
				brickColour = BrickColour.CreateMinimalAnonymous(modelStepPart.brickColour),
				brickPart = BrickPart.CreateMinimalAnonymous(modelStepPart.brickPart),
				modelBuildStep = ModelBuildStep.CreateMinimalAnonymous(modelStepPart.modelBuildStep)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ModelStepPart Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ModelStepPart modelStepPart)
		{
			//
			// Return a very minimal object.
			//
			if (modelStepPart == null)
			{
				return null;
			}

			return new {
				id = modelStepPart.id,
				name = modelStepPart.partFileName,
				description = string.Join(", ", new[] { modelStepPart.partFileName, modelStepPart.transformMatrix}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
