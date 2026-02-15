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
	public partial class ProjectCameraPreset : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ProjectCameraPresetDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 projectId { get; set; }
			[Required]
			public String name { get; set; }
			public Single? positionX { get; set; }
			public Single? positionY { get; set; }
			public Single? positionZ { get; set; }
			public Single? targetX { get; set; }
			public Single? targetY { get; set; }
			public Single? targetZ { get; set; }
			public Single? zoom { get; set; }
			[Required]
			public Boolean isPerspective { get; set; }
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
		public class ProjectCameraPresetOutputDTO : ProjectCameraPresetDTO
		{
			public Project.ProjectDTO project { get; set; }
		}


		/// <summary>
		///
		/// Converts a ProjectCameraPreset to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ProjectCameraPresetDTO ToDTO()
		{
			return new ProjectCameraPresetDTO
			{
				id = this.id,
				projectId = this.projectId,
				name = this.name,
				positionX = this.positionX,
				positionY = this.positionY,
				positionZ = this.positionZ,
				targetX = this.targetX,
				targetY = this.targetY,
				targetZ = this.targetZ,
				zoom = this.zoom,
				isPerspective = this.isPerspective,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ProjectCameraPreset list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ProjectCameraPresetDTO> ToDTOList(List<ProjectCameraPreset> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ProjectCameraPresetDTO> output = new List<ProjectCameraPresetDTO>();

			output.Capacity = data.Count;

			foreach (ProjectCameraPreset projectCameraPreset in data)
			{
				output.Add(projectCameraPreset.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ProjectCameraPreset to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ProjectCameraPresetEntity type directly.
		///
		/// </summary>
		public ProjectCameraPresetOutputDTO ToOutputDTO()
		{
			return new ProjectCameraPresetOutputDTO
			{
				id = this.id,
				projectId = this.projectId,
				name = this.name,
				positionX = this.positionX,
				positionY = this.positionY,
				positionZ = this.positionZ,
				targetX = this.targetX,
				targetY = this.targetY,
				targetZ = this.targetZ,
				zoom = this.zoom,
				isPerspective = this.isPerspective,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				project = this.project?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ProjectCameraPreset list to list of Output Data Transfer Object intended to be used for serializing a list of ProjectCameraPreset objects to avoid using the ProjectCameraPreset entity type directly.
		///
		/// </summary>
		public static List<ProjectCameraPresetOutputDTO> ToOutputDTOList(List<ProjectCameraPreset> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ProjectCameraPresetOutputDTO> output = new List<ProjectCameraPresetOutputDTO>();

			output.Capacity = data.Count;

			foreach (ProjectCameraPreset projectCameraPreset in data)
			{
				output.Add(projectCameraPreset.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ProjectCameraPreset Object.
		///
		/// </summary>
		public static Database.ProjectCameraPreset FromDTO(ProjectCameraPresetDTO dto)
		{
			return new Database.ProjectCameraPreset
			{
				id = dto.id,
				projectId = dto.projectId,
				name = dto.name,
				positionX = dto.positionX,
				positionY = dto.positionY,
				positionZ = dto.positionZ,
				targetX = dto.targetX,
				targetY = dto.targetY,
				targetZ = dto.targetZ,
				zoom = dto.zoom,
				isPerspective = dto.isPerspective,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ProjectCameraPreset Object.
		///
		/// </summary>
		public void ApplyDTO(ProjectCameraPresetDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.projectId = dto.projectId;
			this.name = dto.name;
			this.positionX = dto.positionX;
			this.positionY = dto.positionY;
			this.positionZ = dto.positionZ;
			this.targetX = dto.targetX;
			this.targetY = dto.targetY;
			this.targetZ = dto.targetZ;
			this.zoom = dto.zoom;
			this.isPerspective = dto.isPerspective;
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
		/// Creates a deep copy clone of a ProjectCameraPreset Object.
		///
		/// </summary>
		public ProjectCameraPreset Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ProjectCameraPreset{
				id = this.id,
				tenantGuid = this.tenantGuid,
				projectId = this.projectId,
				name = this.name,
				positionX = this.positionX,
				positionY = this.positionY,
				positionZ = this.positionZ,
				targetX = this.targetX,
				targetY = this.targetY,
				targetZ = this.targetZ,
				zoom = this.zoom,
				isPerspective = this.isPerspective,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ProjectCameraPreset Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ProjectCameraPreset Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ProjectCameraPreset Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ProjectCameraPreset Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ProjectCameraPreset projectCameraPreset)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (projectCameraPreset == null)
			{
				return null;
			}

			return new {
				id = projectCameraPreset.id,
				projectId = projectCameraPreset.projectId,
				name = projectCameraPreset.name,
				positionX = projectCameraPreset.positionX,
				positionY = projectCameraPreset.positionY,
				positionZ = projectCameraPreset.positionZ,
				targetX = projectCameraPreset.targetX,
				targetY = projectCameraPreset.targetY,
				targetZ = projectCameraPreset.targetZ,
				zoom = projectCameraPreset.zoom,
				isPerspective = projectCameraPreset.isPerspective,
				sequence = projectCameraPreset.sequence,
				objectGuid = projectCameraPreset.objectGuid,
				active = projectCameraPreset.active,
				deleted = projectCameraPreset.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ProjectCameraPreset Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ProjectCameraPreset projectCameraPreset)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (projectCameraPreset == null)
			{
				return null;
			}

			return new {
				id = projectCameraPreset.id,
				projectId = projectCameraPreset.projectId,
				name = projectCameraPreset.name,
				positionX = projectCameraPreset.positionX,
				positionY = projectCameraPreset.positionY,
				positionZ = projectCameraPreset.positionZ,
				targetX = projectCameraPreset.targetX,
				targetY = projectCameraPreset.targetY,
				targetZ = projectCameraPreset.targetZ,
				zoom = projectCameraPreset.zoom,
				isPerspective = projectCameraPreset.isPerspective,
				sequence = projectCameraPreset.sequence,
				objectGuid = projectCameraPreset.objectGuid,
				active = projectCameraPreset.active,
				deleted = projectCameraPreset.deleted,
				project = Project.CreateMinimalAnonymous(projectCameraPreset.project)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ProjectCameraPreset Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ProjectCameraPreset projectCameraPreset)
		{
			//
			// Return a very minimal object.
			//
			if (projectCameraPreset == null)
			{
				return null;
			}

			return new {
				id = projectCameraPreset.id,
				name = projectCameraPreset.name,
				description = string.Join(", ", new[] { projectCameraPreset.name}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
