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
	public partial class AuditResource : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class AuditResourceDTO
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
		public class AuditResourceOutputDTO : AuditResourceDTO
		{
		}


		/// <summary>
		///
		/// Converts a AuditResource to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public AuditResourceDTO ToDTO()
		{
			return new AuditResourceDTO
			{
				id = this.id,
				name = this.name,
				comments = this.comments,
				firstAccess = this.firstAccess
			};
		}


		/// <summary>
		///
		/// Converts a AuditResource list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<AuditResourceDTO> ToDTOList(List<AuditResource> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditResourceDTO> output = new List<AuditResourceDTO>();

			output.Capacity = data.Count;

			foreach (AuditResource auditResource in data)
			{
				output.Add(auditResource.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a AuditResource to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the AuditResourceEntity type directly.
		///
		/// </summary>
		public AuditResourceOutputDTO ToOutputDTO()
		{
			return new AuditResourceOutputDTO
			{
				id = this.id,
				name = this.name,
				comments = this.comments,
				firstAccess = this.firstAccess
			};
		}


		/// <summary>
		///
		/// Converts a AuditResource list to list of Output Data Transfer Object intended to be used for serializing a list of AuditResource objects to avoid using the AuditResource entity type directly.
		///
		/// </summary>
		public static List<AuditResourceOutputDTO> ToOutputDTOList(List<AuditResource> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditResourceOutputDTO> output = new List<AuditResourceOutputDTO>();

			output.Capacity = data.Count;

			foreach (AuditResource auditResource in data)
			{
				output.Add(auditResource.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a AuditResource Object.
		///
		/// </summary>
		public static Database.AuditResource FromDTO(AuditResourceDTO dto)
		{
			return new Database.AuditResource
			{
				id = dto.id,
				name = dto.name,
				comments = dto.comments,
				firstAccess = dto.firstAccess
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a AuditResource Object.
		///
		/// </summary>
		public void ApplyDTO(AuditResourceDTO dto)
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
		/// Creates a deep copy clone of a AuditResource Object.
		///
		/// </summary>
		public AuditResource Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new AuditResource{
				id = this.id,
				name = this.name,
				comments = this.comments,
				firstAccess = this.firstAccess,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditResource Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditResource Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a AuditResource Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a AuditResource Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.AuditResource auditResource)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (auditResource == null)
			{
				return null;
			}

			return new {
				id = auditResource.id,
				name = auditResource.name,
				comments = auditResource.comments,
				firstAccess = auditResource.firstAccess,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a AuditResource Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(AuditResource auditResource)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (auditResource == null)
			{
				return null;
			}

			return new {
				id = auditResource.id,
				name = auditResource.name,
				comments = auditResource.comments,
				firstAccess = auditResource.firstAccess,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a AuditResource Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(AuditResource auditResource)
		{
			//
			// Return a very minimal object.
			//
			if (auditResource == null)
			{
				return null;
			}

			return new {
				id = auditResource.id,
				name = auditResource.name,
				description = string.Join(", ", new[] { auditResource.name, auditResource.comments}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
