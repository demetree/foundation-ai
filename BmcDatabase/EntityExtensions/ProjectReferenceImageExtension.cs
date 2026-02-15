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
	public partial class ProjectReferenceImage : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ProjectReferenceImageDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 projectId { get; set; }
			[Required]
			public String name { get; set; }
			public String imageFilePath { get; set; }
			public Single? opacity { get; set; }
			public Single? positionX { get; set; }
			public Single? positionY { get; set; }
			public Single? positionZ { get; set; }
			public Single? scaleX { get; set; }
			public Single? scaleY { get; set; }
			[Required]
			public Boolean isVisible { get; set; }
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
		public class ProjectReferenceImageOutputDTO : ProjectReferenceImageDTO
		{
			public Project.ProjectDTO project { get; set; }
		}


		/// <summary>
		///
		/// Converts a ProjectReferenceImage to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ProjectReferenceImageDTO ToDTO()
		{
			return new ProjectReferenceImageDTO
			{
				id = this.id,
				projectId = this.projectId,
				name = this.name,
				imageFilePath = this.imageFilePath,
				opacity = this.opacity,
				positionX = this.positionX,
				positionY = this.positionY,
				positionZ = this.positionZ,
				scaleX = this.scaleX,
				scaleY = this.scaleY,
				isVisible = this.isVisible,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ProjectReferenceImage list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ProjectReferenceImageDTO> ToDTOList(List<ProjectReferenceImage> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ProjectReferenceImageDTO> output = new List<ProjectReferenceImageDTO>();

			output.Capacity = data.Count;

			foreach (ProjectReferenceImage projectReferenceImage in data)
			{
				output.Add(projectReferenceImage.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ProjectReferenceImage to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ProjectReferenceImageEntity type directly.
		///
		/// </summary>
		public ProjectReferenceImageOutputDTO ToOutputDTO()
		{
			return new ProjectReferenceImageOutputDTO
			{
				id = this.id,
				projectId = this.projectId,
				name = this.name,
				imageFilePath = this.imageFilePath,
				opacity = this.opacity,
				positionX = this.positionX,
				positionY = this.positionY,
				positionZ = this.positionZ,
				scaleX = this.scaleX,
				scaleY = this.scaleY,
				isVisible = this.isVisible,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				project = this.project?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ProjectReferenceImage list to list of Output Data Transfer Object intended to be used for serializing a list of ProjectReferenceImage objects to avoid using the ProjectReferenceImage entity type directly.
		///
		/// </summary>
		public static List<ProjectReferenceImageOutputDTO> ToOutputDTOList(List<ProjectReferenceImage> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ProjectReferenceImageOutputDTO> output = new List<ProjectReferenceImageOutputDTO>();

			output.Capacity = data.Count;

			foreach (ProjectReferenceImage projectReferenceImage in data)
			{
				output.Add(projectReferenceImage.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ProjectReferenceImage Object.
		///
		/// </summary>
		public static Database.ProjectReferenceImage FromDTO(ProjectReferenceImageDTO dto)
		{
			return new Database.ProjectReferenceImage
			{
				id = dto.id,
				projectId = dto.projectId,
				name = dto.name,
				imageFilePath = dto.imageFilePath,
				opacity = dto.opacity,
				positionX = dto.positionX,
				positionY = dto.positionY,
				positionZ = dto.positionZ,
				scaleX = dto.scaleX,
				scaleY = dto.scaleY,
				isVisible = dto.isVisible,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ProjectReferenceImage Object.
		///
		/// </summary>
		public void ApplyDTO(ProjectReferenceImageDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.projectId = dto.projectId;
			this.name = dto.name;
			this.imageFilePath = dto.imageFilePath;
			this.opacity = dto.opacity;
			this.positionX = dto.positionX;
			this.positionY = dto.positionY;
			this.positionZ = dto.positionZ;
			this.scaleX = dto.scaleX;
			this.scaleY = dto.scaleY;
			this.isVisible = dto.isVisible;
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
		/// Creates a deep copy clone of a ProjectReferenceImage Object.
		///
		/// </summary>
		public ProjectReferenceImage Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ProjectReferenceImage{
				id = this.id,
				tenantGuid = this.tenantGuid,
				projectId = this.projectId,
				name = this.name,
				imageFilePath = this.imageFilePath,
				opacity = this.opacity,
				positionX = this.positionX,
				positionY = this.positionY,
				positionZ = this.positionZ,
				scaleX = this.scaleX,
				scaleY = this.scaleY,
				isVisible = this.isVisible,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ProjectReferenceImage Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ProjectReferenceImage Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ProjectReferenceImage Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ProjectReferenceImage Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ProjectReferenceImage projectReferenceImage)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (projectReferenceImage == null)
			{
				return null;
			}

			return new {
				id = projectReferenceImage.id,
				projectId = projectReferenceImage.projectId,
				name = projectReferenceImage.name,
				imageFilePath = projectReferenceImage.imageFilePath,
				opacity = projectReferenceImage.opacity,
				positionX = projectReferenceImage.positionX,
				positionY = projectReferenceImage.positionY,
				positionZ = projectReferenceImage.positionZ,
				scaleX = projectReferenceImage.scaleX,
				scaleY = projectReferenceImage.scaleY,
				isVisible = projectReferenceImage.isVisible,
				objectGuid = projectReferenceImage.objectGuid,
				active = projectReferenceImage.active,
				deleted = projectReferenceImage.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ProjectReferenceImage Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ProjectReferenceImage projectReferenceImage)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (projectReferenceImage == null)
			{
				return null;
			}

			return new {
				id = projectReferenceImage.id,
				projectId = projectReferenceImage.projectId,
				name = projectReferenceImage.name,
				imageFilePath = projectReferenceImage.imageFilePath,
				opacity = projectReferenceImage.opacity,
				positionX = projectReferenceImage.positionX,
				positionY = projectReferenceImage.positionY,
				positionZ = projectReferenceImage.positionZ,
				scaleX = projectReferenceImage.scaleX,
				scaleY = projectReferenceImage.scaleY,
				isVisible = projectReferenceImage.isVisible,
				objectGuid = projectReferenceImage.objectGuid,
				active = projectReferenceImage.active,
				deleted = projectReferenceImage.deleted,
				project = Project.CreateMinimalAnonymous(projectReferenceImage.project)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ProjectReferenceImage Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ProjectReferenceImage projectReferenceImage)
		{
			//
			// Return a very minimal object.
			//
			if (projectReferenceImage == null)
			{
				return null;
			}

			return new {
				id = projectReferenceImage.id,
				name = projectReferenceImage.name,
				description = string.Join(", ", new[] { projectReferenceImage.name, projectReferenceImage.imageFilePath}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
