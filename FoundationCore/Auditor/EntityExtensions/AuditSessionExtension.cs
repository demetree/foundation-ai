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
	public partial class AuditSession : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class AuditSessionDTO
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
		public class AuditSessionOutputDTO : AuditSessionDTO
		{
		}


		/// <summary>
		///
		/// Converts a AuditSession to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public AuditSessionDTO ToDTO()
		{
			return new AuditSessionDTO
			{
				id = this.id,
				name = this.name,
				comments = this.comments,
				firstAccess = this.firstAccess
			};
		}


		/// <summary>
		///
		/// Converts a AuditSession list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<AuditSessionDTO> ToDTOList(List<AuditSession> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditSessionDTO> output = new List<AuditSessionDTO>();

			output.Capacity = data.Count;

			foreach (AuditSession auditSession in data)
			{
				output.Add(auditSession.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a AuditSession to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the AuditSessionEntity type directly.
		///
		/// </summary>
		public AuditSessionOutputDTO ToOutputDTO()
		{
			return new AuditSessionOutputDTO
			{
				id = this.id,
				name = this.name,
				comments = this.comments,
				firstAccess = this.firstAccess
			};
		}


		/// <summary>
		///
		/// Converts a AuditSession list to list of Output Data Transfer Object intended to be used for serializing a list of AuditSession objects to avoid using the AuditSession entity type directly.
		///
		/// </summary>
		public static List<AuditSessionOutputDTO> ToOutputDTOList(List<AuditSession> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditSessionOutputDTO> output = new List<AuditSessionOutputDTO>();

			output.Capacity = data.Count;

			foreach (AuditSession auditSession in data)
			{
				output.Add(auditSession.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a AuditSession Object.
		///
		/// </summary>
		public static Database.AuditSession FromDTO(AuditSessionDTO dto)
		{
			return new Database.AuditSession
			{
				id = dto.id,
				name = dto.name,
				comments = dto.comments,
				firstAccess = dto.firstAccess
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a AuditSession Object.
		///
		/// </summary>
		public void ApplyDTO(AuditSessionDTO dto)
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
		/// Creates a deep copy clone of a AuditSession Object.
		///
		/// </summary>
		public AuditSession Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new AuditSession{
				id = this.id,
				name = this.name,
				comments = this.comments,
				firstAccess = this.firstAccess,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditSession Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditSession Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a AuditSession Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a AuditSession Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.AuditSession auditSession)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (auditSession == null)
			{
				return null;
			}

			return new {
				id = auditSession.id,
				name = auditSession.name,
				comments = auditSession.comments,
				firstAccess = auditSession.firstAccess,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a AuditSession Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(AuditSession auditSession)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (auditSession == null)
			{
				return null;
			}

			return new {
				id = auditSession.id,
				name = auditSession.name,
				comments = auditSession.comments,
				firstAccess = auditSession.firstAccess,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a AuditSession Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(AuditSession auditSession)
		{
			//
			// Return a very minimal object.
			//
			if (auditSession == null)
			{
				return null;
			}

			return new {
				id = auditSession.id,
				name = auditSession.name,
				description = string.Join(", ", new[] { auditSession.name, auditSession.comments}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
