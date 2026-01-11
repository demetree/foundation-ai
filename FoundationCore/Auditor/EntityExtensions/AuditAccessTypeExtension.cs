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
	public partial class AuditAccessType : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class AuditAccessTypeDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			public String description { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class AuditAccessTypeOutputDTO : AuditAccessTypeDTO
		{
		}


		/// <summary>
		///
		/// Converts a AuditAccessType to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public AuditAccessTypeDTO ToDTO()
		{
			return new AuditAccessTypeDTO
			{
				id = this.id,
				name = this.name,
				description = this.description
			};
		}


		/// <summary>
		///
		/// Converts a AuditAccessType list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<AuditAccessTypeDTO> ToDTOList(List<AuditAccessType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditAccessTypeDTO> output = new List<AuditAccessTypeDTO>();

			output.Capacity = data.Count;

			foreach (AuditAccessType auditAccessType in data)
			{
				output.Add(auditAccessType.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a AuditAccessType to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the AuditAccessTypeEntity type directly.
		///
		/// </summary>
		public AuditAccessTypeOutputDTO ToOutputDTO()
		{
			return new AuditAccessTypeOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description
			};
		}


		/// <summary>
		///
		/// Converts a AuditAccessType list to list of Output Data Transfer Object intended to be used for serializing a list of AuditAccessType objects to avoid using the AuditAccessType entity type directly.
		///
		/// </summary>
		public static List<AuditAccessTypeOutputDTO> ToOutputDTOList(List<AuditAccessType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditAccessTypeOutputDTO> output = new List<AuditAccessTypeOutputDTO>();

			output.Capacity = data.Count;

			foreach (AuditAccessType auditAccessType in data)
			{
				output.Add(auditAccessType.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a AuditAccessType Object.
		///
		/// </summary>
		public static Database.AuditAccessType FromDTO(AuditAccessTypeDTO dto)
		{
			return new Database.AuditAccessType
			{
				id = dto.id,
				name = dto.name,
				description = dto.description
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a AuditAccessType Object.
		///
		/// </summary>
		public void ApplyDTO(AuditAccessTypeDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a AuditAccessType Object.
		///
		/// </summary>
		public AuditAccessType Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new AuditAccessType{
				id = this.id,
				name = this.name,
				description = this.description,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditAccessType Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditAccessType Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a AuditAccessType Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a AuditAccessType Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.AuditAccessType auditAccessType)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (auditAccessType == null)
			{
				return null;
			}

			return new {
				id = auditAccessType.id,
				name = auditAccessType.name,
				description = auditAccessType.description,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a AuditAccessType Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(AuditAccessType auditAccessType)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (auditAccessType == null)
			{
				return null;
			}

			return new {
				id = auditAccessType.id,
				name = auditAccessType.name,
				description = auditAccessType.description,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a AuditAccessType Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(AuditAccessType auditAccessType)
		{
			//
			// Return a very minimal object.
			//
			if (auditAccessType == null)
			{
				return null;
			}

			return new {
				id = auditAccessType.id,
				name = auditAccessType.name,
				description = auditAccessType.description,
			 };
		}
	}
}
