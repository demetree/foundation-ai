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
	public partial class BuildStepAnnotation : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BuildStepAnnotationDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 buildManualStepId { get; set; }
			[Required]
			public Int32 buildStepAnnotationTypeId { get; set; }
			public Single? positionX { get; set; }
			public Single? positionY { get; set; }
			public Single? width { get; set; }
			public Single? height { get; set; }
			public String text { get; set; }
			public Int32? placedBrickId { get; set; }
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
		public class BuildStepAnnotationOutputDTO : BuildStepAnnotationDTO
		{
			public BuildManualStep.BuildManualStepDTO buildManualStep { get; set; }
			public BuildStepAnnotationType.BuildStepAnnotationTypeDTO buildStepAnnotationType { get; set; }
			public PlacedBrick.PlacedBrickDTO placedBrick { get; set; }
		}


		/// <summary>
		///
		/// Converts a BuildStepAnnotation to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BuildStepAnnotationDTO ToDTO()
		{
			return new BuildStepAnnotationDTO
			{
				id = this.id,
				buildManualStepId = this.buildManualStepId,
				buildStepAnnotationTypeId = this.buildStepAnnotationTypeId,
				positionX = this.positionX,
				positionY = this.positionY,
				width = this.width,
				height = this.height,
				text = this.text,
				placedBrickId = this.placedBrickId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BuildStepAnnotation list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BuildStepAnnotationDTO> ToDTOList(List<BuildStepAnnotation> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BuildStepAnnotationDTO> output = new List<BuildStepAnnotationDTO>();

			output.Capacity = data.Count;

			foreach (BuildStepAnnotation buildStepAnnotation in data)
			{
				output.Add(buildStepAnnotation.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BuildStepAnnotation to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BuildStepAnnotationEntity type directly.
		///
		/// </summary>
		public BuildStepAnnotationOutputDTO ToOutputDTO()
		{
			return new BuildStepAnnotationOutputDTO
			{
				id = this.id,
				buildManualStepId = this.buildManualStepId,
				buildStepAnnotationTypeId = this.buildStepAnnotationTypeId,
				positionX = this.positionX,
				positionY = this.positionY,
				width = this.width,
				height = this.height,
				text = this.text,
				placedBrickId = this.placedBrickId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				buildManualStep = this.buildManualStep?.ToDTO(),
				buildStepAnnotationType = this.buildStepAnnotationType?.ToDTO(),
				placedBrick = this.placedBrick?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a BuildStepAnnotation list to list of Output Data Transfer Object intended to be used for serializing a list of BuildStepAnnotation objects to avoid using the BuildStepAnnotation entity type directly.
		///
		/// </summary>
		public static List<BuildStepAnnotationOutputDTO> ToOutputDTOList(List<BuildStepAnnotation> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BuildStepAnnotationOutputDTO> output = new List<BuildStepAnnotationOutputDTO>();

			output.Capacity = data.Count;

			foreach (BuildStepAnnotation buildStepAnnotation in data)
			{
				output.Add(buildStepAnnotation.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BuildStepAnnotation Object.
		///
		/// </summary>
		public static Database.BuildStepAnnotation FromDTO(BuildStepAnnotationDTO dto)
		{
			return new Database.BuildStepAnnotation
			{
				id = dto.id,
				buildManualStepId = dto.buildManualStepId,
				buildStepAnnotationTypeId = dto.buildStepAnnotationTypeId,
				positionX = dto.positionX,
				positionY = dto.positionY,
				width = dto.width,
				height = dto.height,
				text = dto.text,
				placedBrickId = dto.placedBrickId,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BuildStepAnnotation Object.
		///
		/// </summary>
		public void ApplyDTO(BuildStepAnnotationDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.buildManualStepId = dto.buildManualStepId;
			this.buildStepAnnotationTypeId = dto.buildStepAnnotationTypeId;
			this.positionX = dto.positionX;
			this.positionY = dto.positionY;
			this.width = dto.width;
			this.height = dto.height;
			this.text = dto.text;
			this.placedBrickId = dto.placedBrickId;
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
		/// Creates a deep copy clone of a BuildStepAnnotation Object.
		///
		/// </summary>
		public BuildStepAnnotation Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BuildStepAnnotation{
				id = this.id,
				tenantGuid = this.tenantGuid,
				buildManualStepId = this.buildManualStepId,
				buildStepAnnotationTypeId = this.buildStepAnnotationTypeId,
				positionX = this.positionX,
				positionY = this.positionY,
				width = this.width,
				height = this.height,
				text = this.text,
				placedBrickId = this.placedBrickId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BuildStepAnnotation Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BuildStepAnnotation Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BuildStepAnnotation Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BuildStepAnnotation Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BuildStepAnnotation buildStepAnnotation)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (buildStepAnnotation == null)
			{
				return null;
			}

			return new {
				id = buildStepAnnotation.id,
				buildManualStepId = buildStepAnnotation.buildManualStepId,
				buildStepAnnotationTypeId = buildStepAnnotation.buildStepAnnotationTypeId,
				positionX = buildStepAnnotation.positionX,
				positionY = buildStepAnnotation.positionY,
				width = buildStepAnnotation.width,
				height = buildStepAnnotation.height,
				text = buildStepAnnotation.text,
				placedBrickId = buildStepAnnotation.placedBrickId,
				objectGuid = buildStepAnnotation.objectGuid,
				active = buildStepAnnotation.active,
				deleted = buildStepAnnotation.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BuildStepAnnotation Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BuildStepAnnotation buildStepAnnotation)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (buildStepAnnotation == null)
			{
				return null;
			}

			return new {
				id = buildStepAnnotation.id,
				buildManualStepId = buildStepAnnotation.buildManualStepId,
				buildStepAnnotationTypeId = buildStepAnnotation.buildStepAnnotationTypeId,
				positionX = buildStepAnnotation.positionX,
				positionY = buildStepAnnotation.positionY,
				width = buildStepAnnotation.width,
				height = buildStepAnnotation.height,
				text = buildStepAnnotation.text,
				placedBrickId = buildStepAnnotation.placedBrickId,
				objectGuid = buildStepAnnotation.objectGuid,
				active = buildStepAnnotation.active,
				deleted = buildStepAnnotation.deleted,
				buildManualStep = BuildManualStep.CreateMinimalAnonymous(buildStepAnnotation.buildManualStep),
				buildStepAnnotationType = BuildStepAnnotationType.CreateMinimalAnonymous(buildStepAnnotation.buildStepAnnotationType),
				placedBrick = PlacedBrick.CreateMinimalAnonymous(buildStepAnnotation.placedBrick)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BuildStepAnnotation Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BuildStepAnnotation buildStepAnnotation)
		{
			//
			// Return a very minimal object.
			//
			if (buildStepAnnotation == null)
			{
				return null;
			}

			return new {
				id = buildStepAnnotation.id,
				name = buildStepAnnotation.id,
				description = buildStepAnnotation.id
			 };
		}
	}
}
