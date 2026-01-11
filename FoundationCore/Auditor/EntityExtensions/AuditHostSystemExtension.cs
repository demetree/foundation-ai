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
	public partial class AuditHostSystem : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class AuditHostSystemDTO
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
		public class AuditHostSystemOutputDTO : AuditHostSystemDTO
		{
		}


		/// <summary>
		///
		/// Converts a AuditHostSystem to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public AuditHostSystemDTO ToDTO()
		{
			return new AuditHostSystemDTO
			{
				id = this.id,
				name = this.name,
				comments = this.comments,
				firstAccess = this.firstAccess
			};
		}


		/// <summary>
		///
		/// Converts a AuditHostSystem list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<AuditHostSystemDTO> ToDTOList(List<AuditHostSystem> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditHostSystemDTO> output = new List<AuditHostSystemDTO>();

			output.Capacity = data.Count;

			foreach (AuditHostSystem auditHostSystem in data)
			{
				output.Add(auditHostSystem.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a AuditHostSystem to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the AuditHostSystemEntity type directly.
		///
		/// </summary>
		public AuditHostSystemOutputDTO ToOutputDTO()
		{
			return new AuditHostSystemOutputDTO
			{
				id = this.id,
				name = this.name,
				comments = this.comments,
				firstAccess = this.firstAccess
			};
		}


		/// <summary>
		///
		/// Converts a AuditHostSystem list to list of Output Data Transfer Object intended to be used for serializing a list of AuditHostSystem objects to avoid using the AuditHostSystem entity type directly.
		///
		/// </summary>
		public static List<AuditHostSystemOutputDTO> ToOutputDTOList(List<AuditHostSystem> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditHostSystemOutputDTO> output = new List<AuditHostSystemOutputDTO>();

			output.Capacity = data.Count;

			foreach (AuditHostSystem auditHostSystem in data)
			{
				output.Add(auditHostSystem.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a AuditHostSystem Object.
		///
		/// </summary>
		public static Database.AuditHostSystem FromDTO(AuditHostSystemDTO dto)
		{
			return new Database.AuditHostSystem
			{
				id = dto.id,
				name = dto.name,
				comments = dto.comments,
				firstAccess = dto.firstAccess
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a AuditHostSystem Object.
		///
		/// </summary>
		public void ApplyDTO(AuditHostSystemDTO dto)
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
		/// Creates a deep copy clone of a AuditHostSystem Object.
		///
		/// </summary>
		public AuditHostSystem Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new AuditHostSystem{
				id = this.id,
				name = this.name,
				comments = this.comments,
				firstAccess = this.firstAccess,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditHostSystem Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditHostSystem Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a AuditHostSystem Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a AuditHostSystem Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.AuditHostSystem auditHostSystem)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (auditHostSystem == null)
			{
				return null;
			}

			return new {
				id = auditHostSystem.id,
				name = auditHostSystem.name,
				comments = auditHostSystem.comments,
				firstAccess = auditHostSystem.firstAccess,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a AuditHostSystem Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(AuditHostSystem auditHostSystem)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (auditHostSystem == null)
			{
				return null;
			}

			return new {
				id = auditHostSystem.id,
				name = auditHostSystem.name,
				comments = auditHostSystem.comments,
				firstAccess = auditHostSystem.firstAccess,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a AuditHostSystem Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(AuditHostSystem auditHostSystem)
		{
			//
			// Return a very minimal object.
			//
			if (auditHostSystem == null)
			{
				return null;
			}

			return new {
				id = auditHostSystem.id,
				name = auditHostSystem.name,
				description = string.Join(", ", new[] { auditHostSystem.name, auditHostSystem.comments}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
