using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Security.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class SecurityDepartment : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SecurityDepartmentDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 securityOrganizationId { get; set; }
			[Required]
			public String name { get; set; }
			public String description { get; set; }
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
		public class SecurityDepartmentOutputDTO : SecurityDepartmentDTO
		{
			public SecurityOrganization.SecurityOrganizationDTO securityOrganization { get; set; }
		}


		/// <summary>
		///
		/// Converts a SecurityDepartment to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SecurityDepartmentDTO ToDTO()
		{
			return new SecurityDepartmentDTO
			{
				id = this.id,
				securityOrganizationId = this.securityOrganizationId,
				name = this.name,
				description = this.description,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SecurityDepartment list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SecurityDepartmentDTO> ToDTOList(List<SecurityDepartment> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityDepartmentDTO> output = new List<SecurityDepartmentDTO>();

			output.Capacity = data.Count;

			foreach (SecurityDepartment securityDepartment in data)
			{
				output.Add(securityDepartment.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SecurityDepartment to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SecurityDepartmentEntity type directly.
		///
		/// </summary>
		public SecurityDepartmentOutputDTO ToOutputDTO()
		{
			return new SecurityDepartmentOutputDTO
			{
				id = this.id,
				securityOrganizationId = this.securityOrganizationId,
				name = this.name,
				description = this.description,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				securityOrganization = this.securityOrganization?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a SecurityDepartment list to list of Output Data Transfer Object intended to be used for serializing a list of SecurityDepartment objects to avoid using the SecurityDepartment entity type directly.
		///
		/// </summary>
		public static List<SecurityDepartmentOutputDTO> ToOutputDTOList(List<SecurityDepartment> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityDepartmentOutputDTO> output = new List<SecurityDepartmentOutputDTO>();

			output.Capacity = data.Count;

			foreach (SecurityDepartment securityDepartment in data)
			{
				output.Add(securityDepartment.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SecurityDepartment Object.
		///
		/// </summary>
		public static Database.SecurityDepartment FromDTO(SecurityDepartmentDTO dto)
		{
			return new Database.SecurityDepartment
			{
				id = dto.id,
				securityOrganizationId = dto.securityOrganizationId,
				name = dto.name,
				description = dto.description,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SecurityDepartment Object.
		///
		/// </summary>
		public void ApplyDTO(SecurityDepartmentDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.securityOrganizationId = dto.securityOrganizationId;
			this.name = dto.name;
			this.description = dto.description;
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
		/// Creates a deep copy clone of a SecurityDepartment Object.
		///
		/// </summary>
		public SecurityDepartment Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SecurityDepartment{
				id = this.id,
				securityOrganizationId = this.securityOrganizationId,
				name = this.name,
				description = this.description,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityDepartment Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityDepartment Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SecurityDepartment Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityDepartment Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SecurityDepartment securityDepartment)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (securityDepartment == null)
			{
				return null;
			}

			return new {
				id = securityDepartment.id,
				securityOrganizationId = securityDepartment.securityOrganizationId,
				name = securityDepartment.name,
				description = securityDepartment.description,
				objectGuid = securityDepartment.objectGuid,
				active = securityDepartment.active,
				deleted = securityDepartment.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityDepartment Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SecurityDepartment securityDepartment)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (securityDepartment == null)
			{
				return null;
			}

			return new {
				id = securityDepartment.id,
				securityOrganizationId = securityDepartment.securityOrganizationId,
				name = securityDepartment.name,
				description = securityDepartment.description,
				objectGuid = securityDepartment.objectGuid,
				active = securityDepartment.active,
				deleted = securityDepartment.deleted,
				securityOrganization = SecurityOrganization.CreateMinimalAnonymous(securityDepartment.securityOrganization)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SecurityDepartment Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SecurityDepartment securityDepartment)
		{
			//
			// Return a very minimal object.
			//
			if (securityDepartment == null)
			{
				return null;
			}

			return new {
				id = securityDepartment.id,
				name = securityDepartment.name,
				description = securityDepartment.description,
			 };
		}
	}
}
