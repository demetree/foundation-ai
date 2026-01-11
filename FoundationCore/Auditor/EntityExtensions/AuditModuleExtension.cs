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
	public partial class AuditModule : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class AuditModuleDTO
		{
			public Int32 id { get; set; }
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
		public class AuditModuleOutputDTO : AuditModuleDTO
		{
		}


		/// <summary>
		///
		/// Converts a AuditModule to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public AuditModuleDTO ToDTO()
		{
			return new AuditModuleDTO
			{
				id = this.id,
				name = this.name,
				comments = this.comments,
				firstAccess = this.firstAccess
			};
		}


		/// <summary>
		///
		/// Converts a AuditModule list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<AuditModuleDTO> ToDTOList(List<AuditModule> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditModuleDTO> output = new List<AuditModuleDTO>();

			output.Capacity = data.Count;

			foreach (AuditModule auditModule in data)
			{
				output.Add(auditModule.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a AuditModule to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the AuditModuleEntity type directly.
		///
		/// </summary>
		public AuditModuleOutputDTO ToOutputDTO()
		{
			return new AuditModuleOutputDTO
			{
				id = this.id,
				name = this.name,
				comments = this.comments,
				firstAccess = this.firstAccess
			};
		}


		/// <summary>
		///
		/// Converts a AuditModule list to list of Output Data Transfer Object intended to be used for serializing a list of AuditModule objects to avoid using the AuditModule entity type directly.
		///
		/// </summary>
		public static List<AuditModuleOutputDTO> ToOutputDTOList(List<AuditModule> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditModuleOutputDTO> output = new List<AuditModuleOutputDTO>();

			output.Capacity = data.Count;

			foreach (AuditModule auditModule in data)
			{
				output.Add(auditModule.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a AuditModule Object.
		///
		/// </summary>
		public static Database.AuditModule FromDTO(AuditModuleDTO dto)
		{
			return new Database.AuditModule
			{
				id = dto.id,
				name = dto.name,
				comments = dto.comments,
				firstAccess = dto.firstAccess
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a AuditModule Object.
		///
		/// </summary>
		public void ApplyDTO(AuditModuleDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.comments = dto.comments;
			this.firstAccess = dto.firstAccess;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a AuditModule Object.
		///
		/// </summary>
		public AuditModule Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new AuditModule{
				id = this.id,
				name = this.name,
				comments = this.comments,
				firstAccess = this.firstAccess,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditModule Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditModule Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a AuditModule Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a AuditModule Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.AuditModule auditModule)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (auditModule == null)
			{
				return null;
			}

			return new {
				id = auditModule.id,
				name = auditModule.name,
				comments = auditModule.comments,
				firstAccess = auditModule.firstAccess,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a AuditModule Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(AuditModule auditModule)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (auditModule == null)
			{
				return null;
			}

			return new {
				id = auditModule.id,
				name = auditModule.name,
				comments = auditModule.comments,
				firstAccess = auditModule.firstAccess,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a AuditModule Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(AuditModule auditModule)
		{
			//
			// Return a very minimal object.
			//
			if (auditModule == null)
			{
				return null;
			}

			return new {
				id = auditModule.id,
				name = auditModule.name,
				description = string.Join(", ", new[] { auditModule.name, auditModule.comments}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
