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
	public partial class SecurityUserSecurityRole : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SecurityUserSecurityRoleDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 securityUserId { get; set; }
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
		public class SecurityUserSecurityRoleOutputDTO : SecurityUserSecurityRoleDTO
		{
			public SecurityRole.SecurityRoleDTO securityRole { get; set; }
			public SecurityUser.SecurityUserDTO securityUser { get; set; }
		}


		/// <summary>
		///
		/// Converts a SecurityUserSecurityRole to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SecurityUserSecurityRoleDTO ToDTO()
		{
			return new SecurityUserSecurityRoleDTO
			{
				id = this.id,
				securityUserId = this.securityUserId,
				securityRoleId = this.securityRoleId,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SecurityUserSecurityRole list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SecurityUserSecurityRoleDTO> ToDTOList(List<SecurityUserSecurityRole> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityUserSecurityRoleDTO> output = new List<SecurityUserSecurityRoleDTO>();

			output.Capacity = data.Count;

			foreach (SecurityUserSecurityRole securityUserSecurityRole in data)
			{
				output.Add(securityUserSecurityRole.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SecurityUserSecurityRole to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SecurityUserSecurityRoleEntity type directly.
		///
		/// </summary>
		public SecurityUserSecurityRoleOutputDTO ToOutputDTO()
		{
			return new SecurityUserSecurityRoleOutputDTO
			{
				id = this.id,
				securityUserId = this.securityUserId,
				securityRoleId = this.securityRoleId,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted,
				securityRole = this.securityRole?.ToDTO(),
				securityUser = this.securityUser?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a SecurityUserSecurityRole list to list of Output Data Transfer Object intended to be used for serializing a list of SecurityUserSecurityRole objects to avoid using the SecurityUserSecurityRole entity type directly.
		///
		/// </summary>
		public static List<SecurityUserSecurityRoleOutputDTO> ToOutputDTOList(List<SecurityUserSecurityRole> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityUserSecurityRoleOutputDTO> output = new List<SecurityUserSecurityRoleOutputDTO>();

			output.Capacity = data.Count;

			foreach (SecurityUserSecurityRole securityUserSecurityRole in data)
			{
				output.Add(securityUserSecurityRole.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SecurityUserSecurityRole Object.
		///
		/// </summary>
		public static Database.SecurityUserSecurityRole FromDTO(SecurityUserSecurityRoleDTO dto)
		{
			return new Database.SecurityUserSecurityRole
			{
				id = dto.id,
				securityUserId = dto.securityUserId,
				securityRoleId = dto.securityRoleId,
				comments = dto.comments,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SecurityUserSecurityRole Object.
		///
		/// </summary>
		public void ApplyDTO(SecurityUserSecurityRoleDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.securityUserId = dto.securityUserId;
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
		/// Creates a deep copy clone of a SecurityUserSecurityRole Object.
		///
		/// </summary>
		public SecurityUserSecurityRole Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SecurityUserSecurityRole{
				id = this.id,
				securityUserId = this.securityUserId,
				securityRoleId = this.securityRoleId,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityUserSecurityRole Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityUserSecurityRole Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SecurityUserSecurityRole Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityUserSecurityRole Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SecurityUserSecurityRole securityUserSecurityRole)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (securityUserSecurityRole == null)
			{
				return null;
			}

			return new {
				id = securityUserSecurityRole.id,
				securityUserId = securityUserSecurityRole.securityUserId,
				securityRoleId = securityUserSecurityRole.securityRoleId,
				comments = securityUserSecurityRole.comments,
				active = securityUserSecurityRole.active,
				deleted = securityUserSecurityRole.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityUserSecurityRole Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SecurityUserSecurityRole securityUserSecurityRole)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (securityUserSecurityRole == null)
			{
				return null;
			}

			return new {
				id = securityUserSecurityRole.id,
				securityUserId = securityUserSecurityRole.securityUserId,
				securityRoleId = securityUserSecurityRole.securityRoleId,
				comments = securityUserSecurityRole.comments,
				active = securityUserSecurityRole.active,
				deleted = securityUserSecurityRole.deleted,
				securityRole = SecurityRole.CreateMinimalAnonymous(securityUserSecurityRole.securityRole),
				securityUser = SecurityUser.CreateMinimalAnonymous(securityUserSecurityRole.securityUser)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SecurityUserSecurityRole Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SecurityUserSecurityRole securityUserSecurityRole)
		{
			//
			// Return a very minimal object.
			//
			if (securityUserSecurityRole == null)
			{
				return null;
			}

			return new {
				id = securityUserSecurityRole.id,
				name = securityUserSecurityRole.comments,
				description = string.Join(", ", new[] { securityUserSecurityRole.comments}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
