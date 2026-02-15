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
	public partial class ProjectTagAssignment : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ProjectTagAssignmentDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 projectId { get; set; }
			[Required]
			public Int32 projectTagId { get; set; }
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
		public class ProjectTagAssignmentOutputDTO : ProjectTagAssignmentDTO
		{
			public Project.ProjectDTO project { get; set; }
			public ProjectTag.ProjectTagDTO projectTag { get; set; }
		}


		/// <summary>
		///
		/// Converts a ProjectTagAssignment to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ProjectTagAssignmentDTO ToDTO()
		{
			return new ProjectTagAssignmentDTO
			{
				id = this.id,
				projectId = this.projectId,
				projectTagId = this.projectTagId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ProjectTagAssignment list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ProjectTagAssignmentDTO> ToDTOList(List<ProjectTagAssignment> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ProjectTagAssignmentDTO> output = new List<ProjectTagAssignmentDTO>();

			output.Capacity = data.Count;

			foreach (ProjectTagAssignment projectTagAssignment in data)
			{
				output.Add(projectTagAssignment.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ProjectTagAssignment to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ProjectTagAssignmentEntity type directly.
		///
		/// </summary>
		public ProjectTagAssignmentOutputDTO ToOutputDTO()
		{
			return new ProjectTagAssignmentOutputDTO
			{
				id = this.id,
				projectId = this.projectId,
				projectTagId = this.projectTagId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				project = this.project?.ToDTO(),
				projectTag = this.projectTag?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ProjectTagAssignment list to list of Output Data Transfer Object intended to be used for serializing a list of ProjectTagAssignment objects to avoid using the ProjectTagAssignment entity type directly.
		///
		/// </summary>
		public static List<ProjectTagAssignmentOutputDTO> ToOutputDTOList(List<ProjectTagAssignment> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ProjectTagAssignmentOutputDTO> output = new List<ProjectTagAssignmentOutputDTO>();

			output.Capacity = data.Count;

			foreach (ProjectTagAssignment projectTagAssignment in data)
			{
				output.Add(projectTagAssignment.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ProjectTagAssignment Object.
		///
		/// </summary>
		public static Database.ProjectTagAssignment FromDTO(ProjectTagAssignmentDTO dto)
		{
			return new Database.ProjectTagAssignment
			{
				id = dto.id,
				projectId = dto.projectId,
				projectTagId = dto.projectTagId,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ProjectTagAssignment Object.
		///
		/// </summary>
		public void ApplyDTO(ProjectTagAssignmentDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.projectId = dto.projectId;
			this.projectTagId = dto.projectTagId;
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
		/// Creates a deep copy clone of a ProjectTagAssignment Object.
		///
		/// </summary>
		public ProjectTagAssignment Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ProjectTagAssignment{
				id = this.id,
				tenantGuid = this.tenantGuid,
				projectId = this.projectId,
				projectTagId = this.projectTagId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ProjectTagAssignment Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ProjectTagAssignment Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ProjectTagAssignment Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ProjectTagAssignment Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ProjectTagAssignment projectTagAssignment)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (projectTagAssignment == null)
			{
				return null;
			}

			return new {
				id = projectTagAssignment.id,
				projectId = projectTagAssignment.projectId,
				projectTagId = projectTagAssignment.projectTagId,
				objectGuid = projectTagAssignment.objectGuid,
				active = projectTagAssignment.active,
				deleted = projectTagAssignment.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ProjectTagAssignment Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ProjectTagAssignment projectTagAssignment)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (projectTagAssignment == null)
			{
				return null;
			}

			return new {
				id = projectTagAssignment.id,
				projectId = projectTagAssignment.projectId,
				projectTagId = projectTagAssignment.projectTagId,
				objectGuid = projectTagAssignment.objectGuid,
				active = projectTagAssignment.active,
				deleted = projectTagAssignment.deleted,
				project = Project.CreateMinimalAnonymous(projectTagAssignment.project),
				projectTag = ProjectTag.CreateMinimalAnonymous(projectTagAssignment.projectTag)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ProjectTagAssignment Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ProjectTagAssignment projectTagAssignment)
		{
			//
			// Return a very minimal object.
			//
			if (projectTagAssignment == null)
			{
				return null;
			}

			return new {
				id = projectTagAssignment.id,
				name = projectTagAssignment.id,
				description = projectTagAssignment.id
			 };
		}
	}
}
