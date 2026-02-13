using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.BMC.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class ProjectChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)projectId; }
			set { projectId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ProjectChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 projectId { get; set; }
			public Int32 versionNumber { get; set; }
			[Required]
			public DateTime timeStamp { get; set; }
			[Required]
			public Int32 userId { get; set; }
			[Required]
			public String data { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class ProjectChangeHistoryOutputDTO : ProjectChangeHistoryDTO
		{
			public Project.ProjectDTO project { get; set; }
		}


		/// <summary>
		///
		/// Converts a ProjectChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ProjectChangeHistoryDTO ToDTO()
		{
			return new ProjectChangeHistoryDTO
			{
				id = this.id,
				projectId = this.projectId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ProjectChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ProjectChangeHistoryDTO> ToDTOList(List<ProjectChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ProjectChangeHistoryDTO> output = new List<ProjectChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ProjectChangeHistory projectChangeHistory in data)
			{
				output.Add(projectChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ProjectChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ProjectChangeHistoryEntity type directly.
		///
		/// </summary>
		public ProjectChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ProjectChangeHistoryOutputDTO
			{
				id = this.id,
				projectId = this.projectId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				project = this.project?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ProjectChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ProjectChangeHistory objects to avoid using the ProjectChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ProjectChangeHistoryOutputDTO> ToOutputDTOList(List<ProjectChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ProjectChangeHistoryOutputDTO> output = new List<ProjectChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ProjectChangeHistory projectChangeHistory in data)
			{
				output.Add(projectChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ProjectChangeHistory Object.
		///
		/// </summary>
		public static Database.ProjectChangeHistory FromDTO(ProjectChangeHistoryDTO dto)
		{
			return new Database.ProjectChangeHistory
			{
				id = dto.id,
				projectId = dto.projectId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ProjectChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ProjectChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.projectId = dto.projectId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ProjectChangeHistory Object.
		///
		/// </summary>
		public ProjectChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ProjectChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				projectId = this.projectId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ProjectChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ProjectChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ProjectChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ProjectChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ProjectChangeHistory projectChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (projectChangeHistory == null)
			{
				return null;
			}

			return new {
				id = projectChangeHistory.id,
				projectId = projectChangeHistory.projectId,
				versionNumber = projectChangeHistory.versionNumber,
				timeStamp = projectChangeHistory.timeStamp,
				userId = projectChangeHistory.userId,
				data = projectChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ProjectChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ProjectChangeHistory projectChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (projectChangeHistory == null)
			{
				return null;
			}

			return new {
				id = projectChangeHistory.id,
				projectId = projectChangeHistory.projectId,
				versionNumber = projectChangeHistory.versionNumber,
				timeStamp = projectChangeHistory.timeStamp,
				userId = projectChangeHistory.userId,
				data = projectChangeHistory.data,
				project = Project.CreateMinimalAnonymous(projectChangeHistory.project)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ProjectChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ProjectChangeHistory projectChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (projectChangeHistory == null)
			{
				return null;
			}

			return new {
				id = projectChangeHistory.id,
				name = projectChangeHistory.id,
				description = projectChangeHistory.id
			 };
		}
	}
}
