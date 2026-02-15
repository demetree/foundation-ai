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
	public partial class ProjectExport : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ProjectExportDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 projectId { get; set; }
			[Required]
			public Int32 exportFormatId { get; set; }
			[Required]
			public String name { get; set; }
			public String outputFilePath { get; set; }
			public DateTime? exportedDate { get; set; }
			[Required]
			public Boolean includeInstructions { get; set; }
			[Required]
			public Boolean includePartsList { get; set; }
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
		public class ProjectExportOutputDTO : ProjectExportDTO
		{
			public ExportFormat.ExportFormatDTO exportFormat { get; set; }
			public Project.ProjectDTO project { get; set; }
		}


		/// <summary>
		///
		/// Converts a ProjectExport to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ProjectExportDTO ToDTO()
		{
			return new ProjectExportDTO
			{
				id = this.id,
				projectId = this.projectId,
				exportFormatId = this.exportFormatId,
				name = this.name,
				outputFilePath = this.outputFilePath,
				exportedDate = this.exportedDate,
				includeInstructions = this.includeInstructions,
				includePartsList = this.includePartsList,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ProjectExport list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ProjectExportDTO> ToDTOList(List<ProjectExport> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ProjectExportDTO> output = new List<ProjectExportDTO>();

			output.Capacity = data.Count;

			foreach (ProjectExport projectExport in data)
			{
				output.Add(projectExport.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ProjectExport to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ProjectExportEntity type directly.
		///
		/// </summary>
		public ProjectExportOutputDTO ToOutputDTO()
		{
			return new ProjectExportOutputDTO
			{
				id = this.id,
				projectId = this.projectId,
				exportFormatId = this.exportFormatId,
				name = this.name,
				outputFilePath = this.outputFilePath,
				exportedDate = this.exportedDate,
				includeInstructions = this.includeInstructions,
				includePartsList = this.includePartsList,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				exportFormat = this.exportFormat?.ToDTO(),
				project = this.project?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ProjectExport list to list of Output Data Transfer Object intended to be used for serializing a list of ProjectExport objects to avoid using the ProjectExport entity type directly.
		///
		/// </summary>
		public static List<ProjectExportOutputDTO> ToOutputDTOList(List<ProjectExport> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ProjectExportOutputDTO> output = new List<ProjectExportOutputDTO>();

			output.Capacity = data.Count;

			foreach (ProjectExport projectExport in data)
			{
				output.Add(projectExport.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ProjectExport Object.
		///
		/// </summary>
		public static Database.ProjectExport FromDTO(ProjectExportDTO dto)
		{
			return new Database.ProjectExport
			{
				id = dto.id,
				projectId = dto.projectId,
				exportFormatId = dto.exportFormatId,
				name = dto.name,
				outputFilePath = dto.outputFilePath,
				exportedDate = dto.exportedDate,
				includeInstructions = dto.includeInstructions,
				includePartsList = dto.includePartsList,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ProjectExport Object.
		///
		/// </summary>
		public void ApplyDTO(ProjectExportDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.projectId = dto.projectId;
			this.exportFormatId = dto.exportFormatId;
			this.name = dto.name;
			this.outputFilePath = dto.outputFilePath;
			this.exportedDate = dto.exportedDate;
			this.includeInstructions = dto.includeInstructions;
			this.includePartsList = dto.includePartsList;
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
		/// Creates a deep copy clone of a ProjectExport Object.
		///
		/// </summary>
		public ProjectExport Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ProjectExport{
				id = this.id,
				tenantGuid = this.tenantGuid,
				projectId = this.projectId,
				exportFormatId = this.exportFormatId,
				name = this.name,
				outputFilePath = this.outputFilePath,
				exportedDate = this.exportedDate,
				includeInstructions = this.includeInstructions,
				includePartsList = this.includePartsList,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ProjectExport Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ProjectExport Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ProjectExport Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ProjectExport Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ProjectExport projectExport)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (projectExport == null)
			{
				return null;
			}

			return new {
				id = projectExport.id,
				projectId = projectExport.projectId,
				exportFormatId = projectExport.exportFormatId,
				name = projectExport.name,
				outputFilePath = projectExport.outputFilePath,
				exportedDate = projectExport.exportedDate,
				includeInstructions = projectExport.includeInstructions,
				includePartsList = projectExport.includePartsList,
				objectGuid = projectExport.objectGuid,
				active = projectExport.active,
				deleted = projectExport.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ProjectExport Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ProjectExport projectExport)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (projectExport == null)
			{
				return null;
			}

			return new {
				id = projectExport.id,
				projectId = projectExport.projectId,
				exportFormatId = projectExport.exportFormatId,
				name = projectExport.name,
				outputFilePath = projectExport.outputFilePath,
				exportedDate = projectExport.exportedDate,
				includeInstructions = projectExport.includeInstructions,
				includePartsList = projectExport.includePartsList,
				objectGuid = projectExport.objectGuid,
				active = projectExport.active,
				deleted = projectExport.deleted,
				exportFormat = ExportFormat.CreateMinimalAnonymous(projectExport.exportFormat),
				project = Project.CreateMinimalAnonymous(projectExport.project)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ProjectExport Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ProjectExport projectExport)
		{
			//
			// Return a very minimal object.
			//
			if (projectExport == null)
			{
				return null;
			}

			return new {
				id = projectExport.id,
				name = projectExport.name,
				description = string.Join(", ", new[] { projectExport.name, projectExport.outputFilePath}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
