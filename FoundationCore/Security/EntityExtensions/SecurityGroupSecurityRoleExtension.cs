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
	public partial class SecurityGroupSecurityRole : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SecurityGroupSecurityRoleDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 securityGroupId { get; set; }
			[Required]
			public Int32 securityRoleId { get; set; }
			public String comments { get; set; }
			public Boolean? active { get; set; }
			public Boolean? deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class SecurityGroupSecurityRoleOutputDTO : SecurityGroupSecurityRoleDTO
		{
			public SecurityGroup.SecurityGroupDTO securityGroup { get; set; }
			public SecurityRole.SecurityRoleDTO securityRole { get; set; }
		}


		/// <summary>
		///
		/// Converts a SecurityGroupSecurityRole to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SecurityGroupSecurityRoleDTO ToDTO()
		{
			return new SecurityGroupSecurityRoleDTO
			{
				id = this.id,
				securityGroupId = this.securityGroupId,
				securityRoleId = this.securityRoleId,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SecurityGroupSecurityRole list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SecurityGroupSecurityRoleDTO> ToDTOList(List<SecurityGroupSecurityRole> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityGroupSecurityRoleDTO> output = new List<SecurityGroupSecurityRoleDTO>();

			output.Capacity = data.Count;

			foreach (SecurityGroupSecurityRole securityGroupSecurityRole in data)
			{
				output.Add(securityGroupSecurityRole.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SecurityGroupSecurityRole to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SecurityGroupSecurityRoleEntity type directly.
		///
		/// </summary>
		public SecurityGroupSecurityRoleOutputDTO ToOutputDTO()
		{
			return new SecurityGroupSecurityRoleOutputDTO
			{
				id = this.id,
				securityGroupId = this.securityGroupId,
				securityRoleId = this.securityRoleId,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted,
				securityGroup = this.securityGroup?.ToDTO(),
				securityRole = this.securityRole?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a SecurityGroupSecurityRole list to list of Output Data Transfer Object intended to be used for serializing a list of SecurityGroupSecurityRole objects to avoid using the SecurityGroupSecurityRole entity type directly.
		///
		/// </summary>
		public static List<SecurityGroupSecurityRoleOutputDTO> ToOutputDTOList(List<SecurityGroupSecurityRole> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityGroupSecurityRoleOutputDTO> output = new List<SecurityGroupSecurityRoleOutputDTO>();

			output.Capacity = data.Count;

			foreach (SecurityGroupSecurityRole securityGroupSecurityRole in data)
			{
				output.Add(securityGroupSecurityRole.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SecurityGroupSecurityRole Object.
		///
		/// </summary>
		public static Database.SecurityGroupSecurityRole FromDTO(SecurityGroupSecurityRoleDTO dto)
		{
			return new Database.SecurityGroupSecurityRole
			{
				id = dto.id,
				securityGroupId = dto.securityGroupId,
				securityRoleId = dto.securityRoleId,
				comments = dto.comments,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SecurityGroupSecurityRole Object.
		///
		/// </summary>
		public void ApplyDTO(SecurityGroupSecurityRoleDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.securityGroupId = dto.securityGroupId;
			this.securityRoleId = dto.securityRoleId;
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
		/// Creates a deep copy clone of a SecurityGroupSecurityRole Object.
		///
		/// </summary>
		public SecurityGroupSecurityRole Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SecurityGroupSecurityRole{
				id = this.id,
				securityGroupId = this.securityGroupId,
				securityRoleId = this.securityRoleId,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityGroupSecurityRole Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityGroupSecurityRole Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SecurityGroupSecurityRole Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityGroupSecurityRole Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SecurityGroupSecurityRole securityGroupSecurityRole)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (securityGroupSecurityRole == null)
			{
				return null;
			}

			return new {
				id = securityGroupSecurityRole.id,
				securityGroupId = securityGroupSecurityRole.securityGroupId,
				securityRoleId = securityGroupSecurityRole.securityRoleId,
				comments = securityGroupSecurityRole.comments,
				active = securityGroupSecurityRole.active,
				deleted = securityGroupSecurityRole.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityGroupSecurityRole Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SecurityGroupSecurityRole securityGroupSecurityRole)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (securityGroupSecurityRole == null)
			{
				return null;
			}

			return new {
				id = securityGroupSecurityRole.id,
				securityGroupId = securityGroupSecurityRole.securityGroupId,
				securityRoleId = securityGroupSecurityRole.securityRoleId,
				comments = securityGroupSecurityRole.comments,
				active = securityGroupSecurityRole.active,
				deleted = securityGroupSecurityRole.deleted,
				securityGroup = SecurityGroup.CreateMinimalAnonymous(securityGroupSecurityRole.securityGroup),
				securityRole = SecurityRole.CreateMinimalAnonymous(securityGroupSecurityRole.securityRole)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SecurityGroupSecurityRole Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SecurityGroupSecurityRole securityGroupSecurityRole)
		{
			//
			// Return a very minimal object.
			//
			if (securityGroupSecurityRole == null)
			{
				return null;
			}

			return new {
				id = securityGroupSecurityRole.id,
				name = securityGroupSecurityRole.comments,
				description = string.Join(", ", new[] { securityGroupSecurityRole.comments}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
