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
	public partial class AuditType : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class AuditTypeDTO
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
		public class AuditTypeOutputDTO : AuditTypeDTO
		{
		}


		/// <summary>
		///
		/// Converts a AuditType to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public AuditTypeDTO ToDTO()
		{
			return new AuditTypeDTO
			{
				id = this.id,
				name = this.name,
				description = this.description
			};
		}


		/// <summary>
		///
		/// Converts a AuditType list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<AuditTypeDTO> ToDTOList(List<AuditType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditTypeDTO> output = new List<AuditTypeDTO>();

			output.Capacity = data.Count;

			foreach (AuditType auditType in data)
			{
				output.Add(auditType.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a AuditType to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the AuditTypeEntity type directly.
		///
		/// </summary>
		public AuditTypeOutputDTO ToOutputDTO()
		{
			return new AuditTypeOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description
			};
		}


		/// <summary>
		///
		/// Converts a AuditType list to list of Output Data Transfer Object intended to be used for serializing a list of AuditType objects to avoid using the AuditType entity type directly.
		///
		/// </summary>
		public static List<AuditTypeOutputDTO> ToOutputDTOList(List<AuditType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditTypeOutputDTO> output = new List<AuditTypeOutputDTO>();

			output.Capacity = data.Count;

			foreach (AuditType auditType in data)
			{
				output.Add(auditType.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a AuditType Object.
		///
		/// </summary>
		public static Database.AuditType FromDTO(AuditTypeDTO dto)
		{
			return new Database.AuditType
			{
				id = dto.id,
				name = dto.name,
				description = dto.description
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a AuditType Object.
		///
		/// </summary>
		public void ApplyDTO(AuditTypeDTO dto)
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
		/// Creates a deep copy clone of a AuditType Object.
		///
		/// </summary>
		public AuditType Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new AuditType{
				id = this.id,
				name = this.name,
				description = this.description,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditType Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditType Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a AuditType Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a AuditType Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.AuditType auditType)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (auditType == null)
			{
				return null;
			}

			return new {
				id = auditType.id,
				name = auditType.name,
				description = auditType.description,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a AuditType Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(AuditType auditType)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (auditType == null)
			{
				return null;
			}

			return new {
				id = auditType.id,
				name = auditType.name,
				description = auditType.description,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a AuditType Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(AuditType auditType)
		{
			//
			// Return a very minimal object.
			//
			if (auditType == null)
			{
				return null;
			}

			return new {
				id = auditType.id,
				name = auditType.name,
				description = auditType.description,
			 };
		}
	}
}
