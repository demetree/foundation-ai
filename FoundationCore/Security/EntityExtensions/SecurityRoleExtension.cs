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
	public partial class SecurityRole : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SecurityRoleDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 privilegeId { get; set; }
			[Required]
			public String name { get; set; }
			public String description { get; set; }
			public String comments { get; set; }
			public Boolean? active { get; set; }
			public Boolean? deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class SecurityRoleOutputDTO : SecurityRoleDTO
		{
			public Privilege.PrivilegeDTO privilege { get; set; }
		}


		/// <summary>
		///
		/// Converts a SecurityRole to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SecurityRoleDTO ToDTO()
		{
			return new SecurityRoleDTO
			{
				id = this.id,
				privilegeId = this.privilegeId,
				name = this.name,
				description = this.description,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SecurityRole list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SecurityRoleDTO> ToDTOList(List<SecurityRole> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityRoleDTO> output = new List<SecurityRoleDTO>();

			output.Capacity = data.Count;

			foreach (SecurityRole securityRole in data)
			{
				output.Add(securityRole.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SecurityRole to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SecurityRoleEntity type directly.
		///
		/// </summary>
		public SecurityRoleOutputDTO ToOutputDTO()
		{
			return new SecurityRoleOutputDTO
			{
				id = this.id,
				privilegeId = this.privilegeId,
				name = this.name,
				description = this.description,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted,
				privilege = this.privilege?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a SecurityRole list to list of Output Data Transfer Object intended to be used for serializing a list of SecurityRole objects to avoid using the SecurityRole entity type directly.
		///
		/// </summary>
		public static List<SecurityRoleOutputDTO> ToOutputDTOList(List<SecurityRole> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityRoleOutputDTO> output = new List<SecurityRoleOutputDTO>();

			output.Capacity = data.Count;

			foreach (SecurityRole securityRole in data)
			{
				output.Add(securityRole.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SecurityRole Object.
		///
		/// </summary>
		public static Database.SecurityRole FromDTO(SecurityRoleDTO dto)
		{
			return new Database.SecurityRole
			{
				id = dto.id,
				privilegeId = dto.privilegeId,
				name = dto.name,
				description = dto.description,
				comments = dto.comments,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SecurityRole Object.
		///
		/// </summary>
		public void ApplyDTO(SecurityRoleDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.privilegeId = dto.privilegeId;
			this.name = dto.name;
			this.description = dto.description;
			this.comments = dto.comments;
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
		/// Creates a deep copy clone of a SecurityRole Object.
		///
		/// </summary>
		public SecurityRole Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SecurityRole{
				id = this.id,
				privilegeId = this.privilegeId,
				name = this.name,
				description = this.description,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityRole Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityRole Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SecurityRole Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityRole Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SecurityRole securityRole)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (securityRole == null)
			{
				return null;
			}

			return new {
				id = securityRole.id,
				privilegeId = securityRole.privilegeId,
				name = securityRole.name,
				description = securityRole.description,
				comments = securityRole.comments,
				active = securityRole.active,
				deleted = securityRole.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityRole Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SecurityRole securityRole)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (securityRole == null)
			{
				return null;
			}

			return new {
				id = securityRole.id,
				privilegeId = securityRole.privilegeId,
				name = securityRole.name,
				description = securityRole.description,
				comments = securityRole.comments,
				active = securityRole.active,
				deleted = securityRole.deleted,
				privilege = Privilege.CreateMinimalAnonymous(securityRole.privilege)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SecurityRole Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SecurityRole securityRole)
		{
			//
			// Return a very minimal object.
			//
			if (securityRole == null)
			{
				return null;
			}

			return new {
				id = securityRole.id,
				name = securityRole.name,
				description = securityRole.description,
			 };
		}
	}
}
