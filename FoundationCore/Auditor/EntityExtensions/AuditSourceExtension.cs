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
	public partial class AuditSource : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class AuditSourceDTO
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
		public class AuditSourceOutputDTO : AuditSourceDTO
		{
		}


		/// <summary>
		///
		/// Converts a AuditSource to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public AuditSourceDTO ToDTO()
		{
			return new AuditSourceDTO
			{
				id = this.id,
				name = this.name,
				comments = this.comments,
				firstAccess = this.firstAccess
			};
		}


		/// <summary>
		///
		/// Converts a AuditSource list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<AuditSourceDTO> ToDTOList(List<AuditSource> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditSourceDTO> output = new List<AuditSourceDTO>();

			output.Capacity = data.Count;

			foreach (AuditSource auditSource in data)
			{
				output.Add(auditSource.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a AuditSource to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the AuditSourceEntity type directly.
		///
		/// </summary>
		public AuditSourceOutputDTO ToOutputDTO()
		{
			return new AuditSourceOutputDTO
			{
				id = this.id,
				name = this.name,
				comments = this.comments,
				firstAccess = this.firstAccess
			};
		}


		/// <summary>
		///
		/// Converts a AuditSource list to list of Output Data Transfer Object intended to be used for serializing a list of AuditSource objects to avoid using the AuditSource entity type directly.
		///
		/// </summary>
		public static List<AuditSourceOutputDTO> ToOutputDTOList(List<AuditSource> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditSourceOutputDTO> output = new List<AuditSourceOutputDTO>();

			output.Capacity = data.Count;

			foreach (AuditSource auditSource in data)
			{
				output.Add(auditSource.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a AuditSource Object.
		///
		/// </summary>
		public static Database.AuditSource FromDTO(AuditSourceDTO dto)
		{
			return new Database.AuditSource
			{
				id = dto.id,
				name = dto.name,
				comments = dto.comments,
				firstAccess = dto.firstAccess
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a AuditSource Object.
		///
		/// </summary>
		public void ApplyDTO(AuditSourceDTO dto)
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
		/// Creates a deep copy clone of a AuditSource Object.
		///
		/// </summary>
		public AuditSource Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new AuditSource{
				id = this.id,
				name = this.name,
				comments = this.comments,
				firstAccess = this.firstAccess,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditSource Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditSource Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a AuditSource Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a AuditSource Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.AuditSource auditSource)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (auditSource == null)
			{
				return null;
			}

			return new {
				id = auditSource.id,
				name = auditSource.name,
				comments = auditSource.comments,
				firstAccess = auditSource.firstAccess,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a AuditSource Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(AuditSource auditSource)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (auditSource == null)
			{
				return null;
			}

			return new {
				id = auditSource.id,
				name = auditSource.name,
				comments = auditSource.comments,
				firstAccess = auditSource.firstAccess,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a AuditSource Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(AuditSource auditSource)
		{
			//
			// Return a very minimal object.
			//
			if (auditSource == null)
			{
				return null;
			}

			return new {
				id = auditSource.id,
				name = auditSource.name,
				description = string.Join(", ", new[] { auditSource.name, auditSource.comments}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
