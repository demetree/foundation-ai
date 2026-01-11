using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Auditor.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class AuditModuleEntity : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class AuditModuleEntityDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 auditModuleId { get; set; }
			[Required]
			public String name { get; set; }
			public String comments { get; set; }
			public DateTime? firstAccess { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class AuditModuleEntityOutputDTO : AuditModuleEntityDTO
		{
			public AuditModule.AuditModuleDTO auditModule { get; set; }
		}


		/// <summary>
		///
		/// Converts a AuditModuleEntity to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public AuditModuleEntityDTO ToDTO()
		{
			return new AuditModuleEntityDTO
			{
				id = this.id,
				auditModuleId = this.auditModuleId,
				name = this.name,
				comments = this.comments,
				firstAccess = this.firstAccess
			};
		}


		/// <summary>
		///
		/// Converts a AuditModuleEntity list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<AuditModuleEntityDTO> ToDTOList(List<AuditModuleEntity> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditModuleEntityDTO> output = new List<AuditModuleEntityDTO>();

			output.Capacity = data.Count;

			foreach (AuditModuleEntity auditModuleEntity in data)
			{
				output.Add(auditModuleEntity.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a AuditModuleEntity to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the AuditModuleEntityEntity type directly.
		///
		/// </summary>
		public AuditModuleEntityOutputDTO ToOutputDTO()
		{
			return new AuditModuleEntityOutputDTO
			{
				id = this.id,
				auditModuleId = this.auditModuleId,
				name = this.name,
				comments = this.comments,
				firstAccess = this.firstAccess,
				auditModule = this.auditModule?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a AuditModuleEntity list to list of Output Data Transfer Object intended to be used for serializing a list of AuditModuleEntity objects to avoid using the AuditModuleEntity entity type directly.
		///
		/// </summary>
		public static List<AuditModuleEntityOutputDTO> ToOutputDTOList(List<AuditModuleEntity> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditModuleEntityOutputDTO> output = new List<AuditModuleEntityOutputDTO>();

			output.Capacity = data.Count;

			foreach (AuditModuleEntity auditModuleEntity in data)
			{
				output.Add(auditModuleEntity.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a AuditModuleEntity Object.
		///
		/// </summary>
		public static Database.AuditModuleEntity FromDTO(AuditModuleEntityDTO dto)
		{
			return new Database.AuditModuleEntity
			{
				id = dto.id,
				auditModuleId = dto.auditModuleId,
				name = dto.name,
				comments = dto.comments,
				firstAccess = dto.firstAccess
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a AuditModuleEntity Object.
		///
		/// </summary>
		public void ApplyDTO(AuditModuleEntityDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.auditModuleId = dto.auditModuleId;
			this.name = dto.name;
			this.comments = dto.comments;
			this.firstAccess = dto.firstAccess;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a AuditModuleEntity Object.
		///
		/// </summary>
		public AuditModuleEntity Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new AuditModuleEntity{
				id = this.id,
				auditModuleId = this.auditModuleId,
				name = this.name,
				comments = this.comments,
				firstAccess = this.firstAccess,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditModuleEntity Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditModuleEntity Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a AuditModuleEntity Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a AuditModuleEntity Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.AuditModuleEntity auditModuleEntity)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (auditModuleEntity == null)
			{
				return null;
			}

			return new {
				id = auditModuleEntity.id,
				auditModuleId = auditModuleEntity.auditModuleId,
				name = auditModuleEntity.name,
				comments = auditModuleEntity.comments,
				firstAccess = auditModuleEntity.firstAccess,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a AuditModuleEntity Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(AuditModuleEntity auditModuleEntity)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (auditModuleEntity == null)
			{
				return null;
			}

			return new {
				id = auditModuleEntity.id,
				auditModuleId = auditModuleEntity.auditModuleId,
				name = auditModuleEntity.name,
				comments = auditModuleEntity.comments,
				firstAccess = auditModuleEntity.firstAccess,
				auditModule = AuditModule.CreateMinimalAnonymous(auditModuleEntity.auditModule)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a AuditModuleEntity Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(AuditModuleEntity auditModuleEntity)
		{
			//
			// Return a very minimal object.
			//
			if (auditModuleEntity == null)
			{
				return null;
			}

			return new {
				id = auditModuleEntity.id,
				name = auditModuleEntity.name,
				description = string.Join(", ", new[] { auditModuleEntity.name, auditModuleEntity.comments}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
