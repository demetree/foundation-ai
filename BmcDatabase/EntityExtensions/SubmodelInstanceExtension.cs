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
	public partial class SubmodelInstance : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SubmodelInstanceDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 submodelId { get; set; }
			public Int32? parentSubmodelId { get; set; }
			public Single? positionX { get; set; }
			public Single? positionY { get; set; }
			public Single? positionZ { get; set; }
			public Single? rotationX { get; set; }
			public Single? rotationY { get; set; }
			public Single? rotationZ { get; set; }
			public Single? rotationW { get; set; }
			[Required]
			public Int32 colourCode { get; set; }
			[Required]
			public Int32 buildStepNumber { get; set; }
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
		public class SubmodelInstanceOutputDTO : SubmodelInstanceDTO
		{
			public Submodel.SubmodelDTO parentSubmodel { get; set; }
			public Submodel.SubmodelDTO submodel { get; set; }
		}


		/// <summary>
		///
		/// Converts a SubmodelInstance to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SubmodelInstanceDTO ToDTO()
		{
			return new SubmodelInstanceDTO
			{
				id = this.id,
				submodelId = this.submodelId,
				parentSubmodelId = this.parentSubmodelId,
				positionX = this.positionX,
				positionY = this.positionY,
				positionZ = this.positionZ,
				rotationX = this.rotationX,
				rotationY = this.rotationY,
				rotationZ = this.rotationZ,
				rotationW = this.rotationW,
				colourCode = this.colourCode,
				buildStepNumber = this.buildStepNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SubmodelInstance list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SubmodelInstanceDTO> ToDTOList(List<SubmodelInstance> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SubmodelInstanceDTO> output = new List<SubmodelInstanceDTO>();

			output.Capacity = data.Count;

			foreach (SubmodelInstance submodelInstance in data)
			{
				output.Add(submodelInstance.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SubmodelInstance to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SubmodelInstanceEntity type directly.
		///
		/// </summary>
		public SubmodelInstanceOutputDTO ToOutputDTO()
		{
			return new SubmodelInstanceOutputDTO
			{
				id = this.id,
				submodelId = this.submodelId,
				parentSubmodelId = this.parentSubmodelId,
				positionX = this.positionX,
				positionY = this.positionY,
				positionZ = this.positionZ,
				rotationX = this.rotationX,
				rotationY = this.rotationY,
				rotationZ = this.rotationZ,
				rotationW = this.rotationW,
				colourCode = this.colourCode,
				buildStepNumber = this.buildStepNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				parentSubmodel = this.parentSubmodel?.ToDTO(),
				submodel = this.submodel?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a SubmodelInstance list to list of Output Data Transfer Object intended to be used for serializing a list of SubmodelInstance objects to avoid using the SubmodelInstance entity type directly.
		///
		/// </summary>
		public static List<SubmodelInstanceOutputDTO> ToOutputDTOList(List<SubmodelInstance> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SubmodelInstanceOutputDTO> output = new List<SubmodelInstanceOutputDTO>();

			output.Capacity = data.Count;

			foreach (SubmodelInstance submodelInstance in data)
			{
				output.Add(submodelInstance.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SubmodelInstance Object.
		///
		/// </summary>
		public static Database.SubmodelInstance FromDTO(SubmodelInstanceDTO dto)
		{
			return new Database.SubmodelInstance
			{
				id = dto.id,
				submodelId = dto.submodelId,
				parentSubmodelId = dto.parentSubmodelId,
				positionX = dto.positionX,
				positionY = dto.positionY,
				positionZ = dto.positionZ,
				rotationX = dto.rotationX,
				rotationY = dto.rotationY,
				rotationZ = dto.rotationZ,
				rotationW = dto.rotationW,
				colourCode = dto.colourCode,
				buildStepNumber = dto.buildStepNumber,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SubmodelInstance Object.
		///
		/// </summary>
		public void ApplyDTO(SubmodelInstanceDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.submodelId = dto.submodelId;
			this.parentSubmodelId = dto.parentSubmodelId;
			this.positionX = dto.positionX;
			this.positionY = dto.positionY;
			this.positionZ = dto.positionZ;
			this.rotationX = dto.rotationX;
			this.rotationY = dto.rotationY;
			this.rotationZ = dto.rotationZ;
			this.rotationW = dto.rotationW;
			this.colourCode = dto.colourCode;
			this.buildStepNumber = dto.buildStepNumber;
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
		/// Creates a deep copy clone of a SubmodelInstance Object.
		///
		/// </summary>
		public SubmodelInstance Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SubmodelInstance{
				id = this.id,
				tenantGuid = this.tenantGuid,
				submodelId = this.submodelId,
				parentSubmodelId = this.parentSubmodelId,
				positionX = this.positionX,
				positionY = this.positionY,
				positionZ = this.positionZ,
				rotationX = this.rotationX,
				rotationY = this.rotationY,
				rotationZ = this.rotationZ,
				rotationW = this.rotationW,
				colourCode = this.colourCode,
				buildStepNumber = this.buildStepNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SubmodelInstance Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SubmodelInstance Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SubmodelInstance Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SubmodelInstance Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SubmodelInstance submodelInstance)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (submodelInstance == null)
			{
				return null;
			}

			return new {
				id = submodelInstance.id,
				submodelId = submodelInstance.submodelId,
				parentSubmodelId = submodelInstance.parentSubmodelId,
				positionX = submodelInstance.positionX,
				positionY = submodelInstance.positionY,
				positionZ = submodelInstance.positionZ,
				rotationX = submodelInstance.rotationX,
				rotationY = submodelInstance.rotationY,
				rotationZ = submodelInstance.rotationZ,
				rotationW = submodelInstance.rotationW,
				colourCode = submodelInstance.colourCode,
				buildStepNumber = submodelInstance.buildStepNumber,
				objectGuid = submodelInstance.objectGuid,
				active = submodelInstance.active,
				deleted = submodelInstance.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SubmodelInstance Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SubmodelInstance submodelInstance)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (submodelInstance == null)
			{
				return null;
			}

			return new {
				id = submodelInstance.id,
				submodelId = submodelInstance.submodelId,
				parentSubmodelId = submodelInstance.parentSubmodelId,
				positionX = submodelInstance.positionX,
				positionY = submodelInstance.positionY,
				positionZ = submodelInstance.positionZ,
				rotationX = submodelInstance.rotationX,
				rotationY = submodelInstance.rotationY,
				rotationZ = submodelInstance.rotationZ,
				rotationW = submodelInstance.rotationW,
				colourCode = submodelInstance.colourCode,
				buildStepNumber = submodelInstance.buildStepNumber,
				objectGuid = submodelInstance.objectGuid,
				active = submodelInstance.active,
				deleted = submodelInstance.deleted,
				parentSubmodel = Submodel.CreateMinimalAnonymous(submodelInstance.parentSubmodel),
				submodel = Submodel.CreateMinimalAnonymous(submodelInstance.submodel)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SubmodelInstance Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SubmodelInstance submodelInstance)
		{
			//
			// Return a very minimal object.
			//
			if (submodelInstance == null)
			{
				return null;
			}

			return new {
				id = submodelInstance.id,
				name = submodelInstance.id,
				description = submodelInstance.id
			 };
		}
	}
}
