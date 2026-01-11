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
	public partial class SecurityOrganizationUser : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SecurityOrganizationUserDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 securityOrganizationId { get; set; }
			[Required]
			public Int32 securityUserId { get; set; }
			[Required]
			public Boolean canRead { get; set; }
			[Required]
			public Boolean canWrite { get; set; }
			[Required]
			public Boolean canChangeHierarchy { get; set; }
			[Required]
			public Boolean canChangeOwner { get; set; }
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
		public class SecurityOrganizationUserOutputDTO : SecurityOrganizationUserDTO
		{
			public SecurityOrganization.SecurityOrganizationDTO securityOrganization { get; set; }
			public SecurityUser.SecurityUserDTO securityUser { get; set; }
		}


		/// <summary>
		///
		/// Converts a SecurityOrganizationUser to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SecurityOrganizationUserDTO ToDTO()
		{
			return new SecurityOrganizationUserDTO
			{
				id = this.id,
				securityOrganizationId = this.securityOrganizationId,
				securityUserId = this.securityUserId,
				canRead = this.canRead,
				canWrite = this.canWrite,
				canChangeHierarchy = this.canChangeHierarchy,
				canChangeOwner = this.canChangeOwner,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SecurityOrganizationUser list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SecurityOrganizationUserDTO> ToDTOList(List<SecurityOrganizationUser> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityOrganizationUserDTO> output = new List<SecurityOrganizationUserDTO>();

			output.Capacity = data.Count;

			foreach (SecurityOrganizationUser securityOrganizationUser in data)
			{
				output.Add(securityOrganizationUser.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SecurityOrganizationUser to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SecurityOrganizationUserEntity type directly.
		///
		/// </summary>
		public SecurityOrganizationUserOutputDTO ToOutputDTO()
		{
			return new SecurityOrganizationUserOutputDTO
			{
				id = this.id,
				securityOrganizationId = this.securityOrganizationId,
				securityUserId = this.securityUserId,
				canRead = this.canRead,
				canWrite = this.canWrite,
				canChangeHierarchy = this.canChangeHierarchy,
				canChangeOwner = this.canChangeOwner,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				securityOrganization = this.securityOrganization?.ToDTO(),
				securityUser = this.securityUser?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a SecurityOrganizationUser list to list of Output Data Transfer Object intended to be used for serializing a list of SecurityOrganizationUser objects to avoid using the SecurityOrganizationUser entity type directly.
		///
		/// </summary>
		public static List<SecurityOrganizationUserOutputDTO> ToOutputDTOList(List<SecurityOrganizationUser> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityOrganizationUserOutputDTO> output = new List<SecurityOrganizationUserOutputDTO>();

			output.Capacity = data.Count;

			foreach (SecurityOrganizationUser securityOrganizationUser in data)
			{
				output.Add(securityOrganizationUser.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SecurityOrganizationUser Object.
		///
		/// </summary>
		public static Database.SecurityOrganizationUser FromDTO(SecurityOrganizationUserDTO dto)
		{
			return new Database.SecurityOrganizationUser
			{
				id = dto.id,
				securityOrganizationId = dto.securityOrganizationId,
				securityUserId = dto.securityUserId,
				canRead = dto.canRead,
				canWrite = dto.canWrite,
				canChangeHierarchy = dto.canChangeHierarchy,
				canChangeOwner = dto.canChangeOwner,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SecurityOrganizationUser Object.
		///
		/// </summary>
		public void ApplyDTO(SecurityOrganizationUserDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.securityOrganizationId = dto.securityOrganizationId;
			this.securityUserId = dto.securityUserId;
			this.canRead = dto.canRead;
			this.canWrite = dto.canWrite;
			this.canChangeHierarchy = dto.canChangeHierarchy;
			this.canChangeOwner = dto.canChangeOwner;
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
		/// Creates a deep copy clone of a SecurityOrganizationUser Object.
		///
		/// </summary>
		public SecurityOrganizationUser Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SecurityOrganizationUser{
				id = this.id,
				securityOrganizationId = this.securityOrganizationId,
				securityUserId = this.securityUserId,
				canRead = this.canRead,
				canWrite = this.canWrite,
				canChangeHierarchy = this.canChangeHierarchy,
				canChangeOwner = this.canChangeOwner,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityOrganizationUser Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityOrganizationUser Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SecurityOrganizationUser Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityOrganizationUser Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SecurityOrganizationUser securityOrganizationUser)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (securityOrganizationUser == null)
			{
				return null;
			}

			return new {
				id = securityOrganizationUser.id,
				securityOrganizationId = securityOrganizationUser.securityOrganizationId,
				securityUserId = securityOrganizationUser.securityUserId,
				canRead = securityOrganizationUser.canRead,
				canWrite = securityOrganizationUser.canWrite,
				canChangeHierarchy = securityOrganizationUser.canChangeHierarchy,
				canChangeOwner = securityOrganizationUser.canChangeOwner,
				objectGuid = securityOrganizationUser.objectGuid,
				active = securityOrganizationUser.active,
				deleted = securityOrganizationUser.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityOrganizationUser Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SecurityOrganizationUser securityOrganizationUser)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (securityOrganizationUser == null)
			{
				return null;
			}

			return new {
				id = securityOrganizationUser.id,
				securityOrganizationId = securityOrganizationUser.securityOrganizationId,
				securityUserId = securityOrganizationUser.securityUserId,
				canRead = securityOrganizationUser.canRead,
				canWrite = securityOrganizationUser.canWrite,
				canChangeHierarchy = securityOrganizationUser.canChangeHierarchy,
				canChangeOwner = securityOrganizationUser.canChangeOwner,
				objectGuid = securityOrganizationUser.objectGuid,
				active = securityOrganizationUser.active,
				deleted = securityOrganizationUser.deleted,
				securityOrganization = SecurityOrganization.CreateMinimalAnonymous(securityOrganizationUser.securityOrganization),
				securityUser = SecurityUser.CreateMinimalAnonymous(securityOrganizationUser.securityUser)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SecurityOrganizationUser Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SecurityOrganizationUser securityOrganizationUser)
		{
			//
			// Return a very minimal object.
			//
			if (securityOrganizationUser == null)
			{
				return null;
			}

			return new {
				id = securityOrganizationUser.id,
				name = securityOrganizationUser.id,
				description = securityOrganizationUser.id
			 };
		}
	}
}
