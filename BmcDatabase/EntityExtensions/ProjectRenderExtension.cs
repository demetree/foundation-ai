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
	public partial class ProjectRender : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ProjectRenderDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 projectId { get; set; }
			public Int32? renderPresetId { get; set; }
			[Required]
			public String name { get; set; }
			public String outputFilePath { get; set; }
			public Int32? resolutionWidth { get; set; }
			public Int32? resolutionHeight { get; set; }
			public DateTime? renderedDate { get; set; }
			public Single? renderDurationSeconds { get; set; }
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
		public class ProjectRenderOutputDTO : ProjectRenderDTO
		{
			public Project.ProjectDTO project { get; set; }
			public RenderPreset.RenderPresetDTO renderPreset { get; set; }
		}


		/// <summary>
		///
		/// Converts a ProjectRender to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ProjectRenderDTO ToDTO()
		{
			return new ProjectRenderDTO
			{
				id = this.id,
				projectId = this.projectId,
				renderPresetId = this.renderPresetId,
				name = this.name,
				outputFilePath = this.outputFilePath,
				resolutionWidth = this.resolutionWidth,
				resolutionHeight = this.resolutionHeight,
				renderedDate = this.renderedDate,
				renderDurationSeconds = this.renderDurationSeconds,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ProjectRender list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ProjectRenderDTO> ToDTOList(List<ProjectRender> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ProjectRenderDTO> output = new List<ProjectRenderDTO>();

			output.Capacity = data.Count;

			foreach (ProjectRender projectRender in data)
			{
				output.Add(projectRender.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ProjectRender to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ProjectRenderEntity type directly.
		///
		/// </summary>
		public ProjectRenderOutputDTO ToOutputDTO()
		{
			return new ProjectRenderOutputDTO
			{
				id = this.id,
				projectId = this.projectId,
				renderPresetId = this.renderPresetId,
				name = this.name,
				outputFilePath = this.outputFilePath,
				resolutionWidth = this.resolutionWidth,
				resolutionHeight = this.resolutionHeight,
				renderedDate = this.renderedDate,
				renderDurationSeconds = this.renderDurationSeconds,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				project = this.project?.ToDTO(),
				renderPreset = this.renderPreset?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ProjectRender list to list of Output Data Transfer Object intended to be used for serializing a list of ProjectRender objects to avoid using the ProjectRender entity type directly.
		///
		/// </summary>
		public static List<ProjectRenderOutputDTO> ToOutputDTOList(List<ProjectRender> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ProjectRenderOutputDTO> output = new List<ProjectRenderOutputDTO>();

			output.Capacity = data.Count;

			foreach (ProjectRender projectRender in data)
			{
				output.Add(projectRender.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ProjectRender Object.
		///
		/// </summary>
		public static Database.ProjectRender FromDTO(ProjectRenderDTO dto)
		{
			return new Database.ProjectRender
			{
				id = dto.id,
				projectId = dto.projectId,
				renderPresetId = dto.renderPresetId,
				name = dto.name,
				outputFilePath = dto.outputFilePath,
				resolutionWidth = dto.resolutionWidth,
				resolutionHeight = dto.resolutionHeight,
				renderedDate = dto.renderedDate,
				renderDurationSeconds = dto.renderDurationSeconds,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ProjectRender Object.
		///
		/// </summary>
		public void ApplyDTO(ProjectRenderDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.projectId = dto.projectId;
			this.renderPresetId = dto.renderPresetId;
			this.name = dto.name;
			this.outputFilePath = dto.outputFilePath;
			this.resolutionWidth = dto.resolutionWidth;
			this.resolutionHeight = dto.resolutionHeight;
			this.renderedDate = dto.renderedDate;
			this.renderDurationSeconds = dto.renderDurationSeconds;
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
		/// Creates a deep copy clone of a ProjectRender Object.
		///
		/// </summary>
		public ProjectRender Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ProjectRender{
				id = this.id,
				tenantGuid = this.tenantGuid,
				projectId = this.projectId,
				renderPresetId = this.renderPresetId,
				name = this.name,
				outputFilePath = this.outputFilePath,
				resolutionWidth = this.resolutionWidth,
				resolutionHeight = this.resolutionHeight,
				renderedDate = this.renderedDate,
				renderDurationSeconds = this.renderDurationSeconds,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ProjectRender Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ProjectRender Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ProjectRender Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ProjectRender Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ProjectRender projectRender)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (projectRender == null)
			{
				return null;
			}

			return new {
				id = projectRender.id,
				projectId = projectRender.projectId,
				renderPresetId = projectRender.renderPresetId,
				name = projectRender.name,
				outputFilePath = projectRender.outputFilePath,
				resolutionWidth = projectRender.resolutionWidth,
				resolutionHeight = projectRender.resolutionHeight,
				renderedDate = projectRender.renderedDate,
				renderDurationSeconds = projectRender.renderDurationSeconds,
				objectGuid = projectRender.objectGuid,
				active = projectRender.active,
				deleted = projectRender.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ProjectRender Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ProjectRender projectRender)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (projectRender == null)
			{
				return null;
			}

			return new {
				id = projectRender.id,
				projectId = projectRender.projectId,
				renderPresetId = projectRender.renderPresetId,
				name = projectRender.name,
				outputFilePath = projectRender.outputFilePath,
				resolutionWidth = projectRender.resolutionWidth,
				resolutionHeight = projectRender.resolutionHeight,
				renderedDate = projectRender.renderedDate,
				renderDurationSeconds = projectRender.renderDurationSeconds,
				objectGuid = projectRender.objectGuid,
				active = projectRender.active,
				deleted = projectRender.deleted,
				project = Project.CreateMinimalAnonymous(projectRender.project),
				renderPreset = RenderPreset.CreateMinimalAnonymous(projectRender.renderPreset)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ProjectRender Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ProjectRender projectRender)
		{
			//
			// Return a very minimal object.
			//
			if (projectRender == null)
			{
				return null;
			}

			return new {
				id = projectRender.id,
				name = projectRender.name,
				description = string.Join(", ", new[] { projectRender.name, projectRender.outputFilePath}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
