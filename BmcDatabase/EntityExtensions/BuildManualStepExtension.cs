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
	public partial class BuildManualStep : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BuildManualStepDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 buildManualPageId { get; set; }
			public Int32? stepNumber { get; set; }
			public Single? cameraPositionX { get; set; }
			public Single? cameraPositionY { get; set; }
			public Single? cameraPositionZ { get; set; }
			public Single? cameraTargetX { get; set; }
			public Single? cameraTargetY { get; set; }
			public Single? cameraTargetZ { get; set; }
			public Single? cameraZoom { get; set; }
			[Required]
			public Boolean showExplodedView { get; set; }
			public Single? explodedDistance { get; set; }
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
		public class BuildManualStepOutputDTO : BuildManualStepDTO
		{
			public BuildManualPage.BuildManualPageDTO buildManualPage { get; set; }
		}


		/// <summary>
		///
		/// Converts a BuildManualStep to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BuildManualStepDTO ToDTO()
		{
			return new BuildManualStepDTO
			{
				id = this.id,
				buildManualPageId = this.buildManualPageId,
				stepNumber = this.stepNumber,
				cameraPositionX = this.cameraPositionX,
				cameraPositionY = this.cameraPositionY,
				cameraPositionZ = this.cameraPositionZ,
				cameraTargetX = this.cameraTargetX,
				cameraTargetY = this.cameraTargetY,
				cameraTargetZ = this.cameraTargetZ,
				cameraZoom = this.cameraZoom,
				showExplodedView = this.showExplodedView,
				explodedDistance = this.explodedDistance,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BuildManualStep list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BuildManualStepDTO> ToDTOList(List<BuildManualStep> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BuildManualStepDTO> output = new List<BuildManualStepDTO>();

			output.Capacity = data.Count;

			foreach (BuildManualStep buildManualStep in data)
			{
				output.Add(buildManualStep.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BuildManualStep to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BuildManualStepEntity type directly.
		///
		/// </summary>
		public BuildManualStepOutputDTO ToOutputDTO()
		{
			return new BuildManualStepOutputDTO
			{
				id = this.id,
				buildManualPageId = this.buildManualPageId,
				stepNumber = this.stepNumber,
				cameraPositionX = this.cameraPositionX,
				cameraPositionY = this.cameraPositionY,
				cameraPositionZ = this.cameraPositionZ,
				cameraTargetX = this.cameraTargetX,
				cameraTargetY = this.cameraTargetY,
				cameraTargetZ = this.cameraTargetZ,
				cameraZoom = this.cameraZoom,
				showExplodedView = this.showExplodedView,
				explodedDistance = this.explodedDistance,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				buildManualPage = this.buildManualPage?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a BuildManualStep list to list of Output Data Transfer Object intended to be used for serializing a list of BuildManualStep objects to avoid using the BuildManualStep entity type directly.
		///
		/// </summary>
		public static List<BuildManualStepOutputDTO> ToOutputDTOList(List<BuildManualStep> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BuildManualStepOutputDTO> output = new List<BuildManualStepOutputDTO>();

			output.Capacity = data.Count;

			foreach (BuildManualStep buildManualStep in data)
			{
				output.Add(buildManualStep.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BuildManualStep Object.
		///
		/// </summary>
		public static Database.BuildManualStep FromDTO(BuildManualStepDTO dto)
		{
			return new Database.BuildManualStep
			{
				id = dto.id,
				buildManualPageId = dto.buildManualPageId,
				stepNumber = dto.stepNumber,
				cameraPositionX = dto.cameraPositionX,
				cameraPositionY = dto.cameraPositionY,
				cameraPositionZ = dto.cameraPositionZ,
				cameraTargetX = dto.cameraTargetX,
				cameraTargetY = dto.cameraTargetY,
				cameraTargetZ = dto.cameraTargetZ,
				cameraZoom = dto.cameraZoom,
				showExplodedView = dto.showExplodedView,
				explodedDistance = dto.explodedDistance,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BuildManualStep Object.
		///
		/// </summary>
		public void ApplyDTO(BuildManualStepDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.buildManualPageId = dto.buildManualPageId;
			this.stepNumber = dto.stepNumber;
			this.cameraPositionX = dto.cameraPositionX;
			this.cameraPositionY = dto.cameraPositionY;
			this.cameraPositionZ = dto.cameraPositionZ;
			this.cameraTargetX = dto.cameraTargetX;
			this.cameraTargetY = dto.cameraTargetY;
			this.cameraTargetZ = dto.cameraTargetZ;
			this.cameraZoom = dto.cameraZoom;
			this.showExplodedView = dto.showExplodedView;
			this.explodedDistance = dto.explodedDistance;
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
		/// Creates a deep copy clone of a BuildManualStep Object.
		///
		/// </summary>
		public BuildManualStep Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BuildManualStep{
				id = this.id,
				tenantGuid = this.tenantGuid,
				buildManualPageId = this.buildManualPageId,
				stepNumber = this.stepNumber,
				cameraPositionX = this.cameraPositionX,
				cameraPositionY = this.cameraPositionY,
				cameraPositionZ = this.cameraPositionZ,
				cameraTargetX = this.cameraTargetX,
				cameraTargetY = this.cameraTargetY,
				cameraTargetZ = this.cameraTargetZ,
				cameraZoom = this.cameraZoom,
				showExplodedView = this.showExplodedView,
				explodedDistance = this.explodedDistance,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BuildManualStep Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BuildManualStep Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BuildManualStep Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BuildManualStep Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BuildManualStep buildManualStep)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (buildManualStep == null)
			{
				return null;
			}

			return new {
				id = buildManualStep.id,
				buildManualPageId = buildManualStep.buildManualPageId,
				stepNumber = buildManualStep.stepNumber,
				cameraPositionX = buildManualStep.cameraPositionX,
				cameraPositionY = buildManualStep.cameraPositionY,
				cameraPositionZ = buildManualStep.cameraPositionZ,
				cameraTargetX = buildManualStep.cameraTargetX,
				cameraTargetY = buildManualStep.cameraTargetY,
				cameraTargetZ = buildManualStep.cameraTargetZ,
				cameraZoom = buildManualStep.cameraZoom,
				showExplodedView = buildManualStep.showExplodedView,
				explodedDistance = buildManualStep.explodedDistance,
				objectGuid = buildManualStep.objectGuid,
				active = buildManualStep.active,
				deleted = buildManualStep.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BuildManualStep Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BuildManualStep buildManualStep)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (buildManualStep == null)
			{
				return null;
			}

			return new {
				id = buildManualStep.id,
				buildManualPageId = buildManualStep.buildManualPageId,
				stepNumber = buildManualStep.stepNumber,
				cameraPositionX = buildManualStep.cameraPositionX,
				cameraPositionY = buildManualStep.cameraPositionY,
				cameraPositionZ = buildManualStep.cameraPositionZ,
				cameraTargetX = buildManualStep.cameraTargetX,
				cameraTargetY = buildManualStep.cameraTargetY,
				cameraTargetZ = buildManualStep.cameraTargetZ,
				cameraZoom = buildManualStep.cameraZoom,
				showExplodedView = buildManualStep.showExplodedView,
				explodedDistance = buildManualStep.explodedDistance,
				objectGuid = buildManualStep.objectGuid,
				active = buildManualStep.active,
				deleted = buildManualStep.deleted,
				buildManualPage = BuildManualPage.CreateMinimalAnonymous(buildManualStep.buildManualPage)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BuildManualStep Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BuildManualStep buildManualStep)
		{
			//
			// Return a very minimal object.
			//
			if (buildManualStep == null)
			{
				return null;
			}

			return new {
				id = buildManualStep.id,
				name = buildManualStep.id,
				description = buildManualStep.id
			 };
		}
	}
}
