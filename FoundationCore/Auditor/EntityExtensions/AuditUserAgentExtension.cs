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
	public partial class AuditUserAgent : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class AuditUserAgentDTO
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
		public class AuditUserAgentOutputDTO : AuditUserAgentDTO
		{
		}


		/// <summary>
		///
		/// Converts a AuditUserAgent to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public AuditUserAgentDTO ToDTO()
		{
			return new AuditUserAgentDTO
			{
				id = this.id,
				name = this.name,
				comments = this.comments,
				firstAccess = this.firstAccess
			};
		}


		/// <summary>
		///
		/// Converts a AuditUserAgent list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<AuditUserAgentDTO> ToDTOList(List<AuditUserAgent> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditUserAgentDTO> output = new List<AuditUserAgentDTO>();

			output.Capacity = data.Count;

			foreach (AuditUserAgent auditUserAgent in data)
			{
				output.Add(auditUserAgent.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a AuditUserAgent to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the AuditUserAgentEntity type directly.
		///
		/// </summary>
		public AuditUserAgentOutputDTO ToOutputDTO()
		{
			return new AuditUserAgentOutputDTO
			{
				id = this.id,
				name = this.name,
				comments = this.comments,
				firstAccess = this.firstAccess
			};
		}


		/// <summary>
		///
		/// Converts a AuditUserAgent list to list of Output Data Transfer Object intended to be used for serializing a list of AuditUserAgent objects to avoid using the AuditUserAgent entity type directly.
		///
		/// </summary>
		public static List<AuditUserAgentOutputDTO> ToOutputDTOList(List<AuditUserAgent> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditUserAgentOutputDTO> output = new List<AuditUserAgentOutputDTO>();

			output.Capacity = data.Count;

			foreach (AuditUserAgent auditUserAgent in data)
			{
				output.Add(auditUserAgent.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a AuditUserAgent Object.
		///
		/// </summary>
		public static Database.AuditUserAgent FromDTO(AuditUserAgentDTO dto)
		{
			return new Database.AuditUserAgent
			{
				id = dto.id,
				name = dto.name,
				comments = dto.comments,
				firstAccess = dto.firstAccess
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a AuditUserAgent Object.
		///
		/// </summary>
		public void ApplyDTO(AuditUserAgentDTO dto)
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
		/// Creates a deep copy clone of a AuditUserAgent Object.
		///
		/// </summary>
		public AuditUserAgent Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new AuditUserAgent{
				id = this.id,
				name = this.name,
				comments = this.comments,
				firstAccess = this.firstAccess,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditUserAgent Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditUserAgent Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a AuditUserAgent Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a AuditUserAgent Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.AuditUserAgent auditUserAgent)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (auditUserAgent == null)
			{
				return null;
			}

			return new {
				id = auditUserAgent.id,
				name = auditUserAgent.name,
				comments = auditUserAgent.comments,
				firstAccess = auditUserAgent.firstAccess,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a AuditUserAgent Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(AuditUserAgent auditUserAgent)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (auditUserAgent == null)
			{
				return null;
			}

			return new {
				id = auditUserAgent.id,
				name = auditUserAgent.name,
				comments = auditUserAgent.comments,
				firstAccess = auditUserAgent.firstAccess,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a AuditUserAgent Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(AuditUserAgent auditUserAgent)
		{
			//
			// Return a very minimal object.
			//
			if (auditUserAgent == null)
			{
				return null;
			}

			return new {
				id = auditUserAgent.id,
				name = auditUserAgent.name,
				description = string.Join(", ", new[] { auditUserAgent.name, auditUserAgent.comments}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
