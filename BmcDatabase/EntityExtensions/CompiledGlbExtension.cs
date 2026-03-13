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
	public partial class CompiledGlb : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class CompiledGlbDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 projectId { get; set; }
			[Required]
			public Int32 projectVersionNumber { get; set; }
			[Required]
			public Boolean includesEdgeLines { get; set; }
			public Byte[] glbData { get; set; }
			[Required]
			public Int64 glbSizeBytes { get; set; }
			public Int32? triangleCount { get; set; }
			public Int32? stepCount { get; set; }
			[Required]
			public DateTime compiledAt { get; set; }
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
		public class CompiledGlbOutputDTO : CompiledGlbDTO
		{
			public Project.ProjectDTO project { get; set; }
		}


		/// <summary>
		///
		/// Converts a CompiledGlb to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public CompiledGlbDTO ToDTO()
		{
			return new CompiledGlbDTO
			{
				id = this.id,
				projectId = this.projectId,
				projectVersionNumber = this.projectVersionNumber,
				includesEdgeLines = this.includesEdgeLines,
				glbData = this.glbData,
				glbSizeBytes = this.glbSizeBytes,
				triangleCount = this.triangleCount,
				stepCount = this.stepCount,
				compiledAt = this.compiledAt,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a CompiledGlb list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<CompiledGlbDTO> ToDTOList(List<CompiledGlb> data)
		{
			if (data == null)
			{
				return null;
			}

			List<CompiledGlbDTO> output = new List<CompiledGlbDTO>();

			output.Capacity = data.Count;

			foreach (CompiledGlb compiledGlb in data)
			{
				output.Add(compiledGlb.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a CompiledGlb to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the CompiledGlbEntity type directly.
		///
		/// </summary>
		public CompiledGlbOutputDTO ToOutputDTO()
		{
			return new CompiledGlbOutputDTO
			{
				id = this.id,
				projectId = this.projectId,
				projectVersionNumber = this.projectVersionNumber,
				includesEdgeLines = this.includesEdgeLines,
				glbData = this.glbData,
				glbSizeBytes = this.glbSizeBytes,
				triangleCount = this.triangleCount,
				stepCount = this.stepCount,
				compiledAt = this.compiledAt,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				project = this.project?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a CompiledGlb list to list of Output Data Transfer Object intended to be used for serializing a list of CompiledGlb objects to avoid using the CompiledGlb entity type directly.
		///
		/// </summary>
		public static List<CompiledGlbOutputDTO> ToOutputDTOList(List<CompiledGlb> data)
		{
			if (data == null)
			{
				return null;
			}

			List<CompiledGlbOutputDTO> output = new List<CompiledGlbOutputDTO>();

			output.Capacity = data.Count;

			foreach (CompiledGlb compiledGlb in data)
			{
				output.Add(compiledGlb.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a CompiledGlb Object.
		///
		/// </summary>
		public static Database.CompiledGlb FromDTO(CompiledGlbDTO dto)
		{
			return new Database.CompiledGlb
			{
				id = dto.id,
				projectId = dto.projectId,
				projectVersionNumber = dto.projectVersionNumber,
				includesEdgeLines = dto.includesEdgeLines,
				glbData = dto.glbData,
				glbSizeBytes = dto.glbSizeBytes,
				triangleCount = dto.triangleCount,
				stepCount = dto.stepCount,
				compiledAt = dto.compiledAt,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a CompiledGlb Object.
		///
		/// </summary>
		public void ApplyDTO(CompiledGlbDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.projectId = dto.projectId;
			this.projectVersionNumber = dto.projectVersionNumber;
			this.includesEdgeLines = dto.includesEdgeLines;
			this.glbData = dto.glbData;
			this.glbSizeBytes = dto.glbSizeBytes;
			this.triangleCount = dto.triangleCount;
			this.stepCount = dto.stepCount;
			this.compiledAt = dto.compiledAt;
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
		/// Creates a deep copy clone of a CompiledGlb Object.
		///
		/// </summary>
		public CompiledGlb Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new CompiledGlb{
				id = this.id,
				tenantGuid = this.tenantGuid,
				projectId = this.projectId,
				projectVersionNumber = this.projectVersionNumber,
				includesEdgeLines = this.includesEdgeLines,
				glbData = this.glbData,
				glbSizeBytes = this.glbSizeBytes,
				triangleCount = this.triangleCount,
				stepCount = this.stepCount,
				compiledAt = this.compiledAt,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a CompiledGlb Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a CompiledGlb Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a CompiledGlb Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a CompiledGlb Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.CompiledGlb compiledGlb)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (compiledGlb == null)
			{
				return null;
			}

			return new {
				id = compiledGlb.id,
				projectId = compiledGlb.projectId,
				projectVersionNumber = compiledGlb.projectVersionNumber,
				includesEdgeLines = compiledGlb.includesEdgeLines,
				glbData = compiledGlb.glbData,
				glbSizeBytes = compiledGlb.glbSizeBytes,
				triangleCount = compiledGlb.triangleCount,
				stepCount = compiledGlb.stepCount,
				compiledAt = compiledGlb.compiledAt,
				objectGuid = compiledGlb.objectGuid,
				active = compiledGlb.active,
				deleted = compiledGlb.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a CompiledGlb Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(CompiledGlb compiledGlb)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (compiledGlb == null)
			{
				return null;
			}

			return new {
				id = compiledGlb.id,
				projectId = compiledGlb.projectId,
				projectVersionNumber = compiledGlb.projectVersionNumber,
				includesEdgeLines = compiledGlb.includesEdgeLines,
				glbData = compiledGlb.glbData,
				glbSizeBytes = compiledGlb.glbSizeBytes,
				triangleCount = compiledGlb.triangleCount,
				stepCount = compiledGlb.stepCount,
				compiledAt = compiledGlb.compiledAt,
				objectGuid = compiledGlb.objectGuid,
				active = compiledGlb.active,
				deleted = compiledGlb.deleted,
				project = Project.CreateMinimalAnonymous(compiledGlb.project)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a CompiledGlb Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(CompiledGlb compiledGlb)
		{
			//
			// Return a very minimal object.
			//
			if (compiledGlb == null)
			{
				return null;
			}

			return new {
				id = compiledGlb.id,
				name = compiledGlb.id,
				description = compiledGlb.id
			 };
		}
	}
}
