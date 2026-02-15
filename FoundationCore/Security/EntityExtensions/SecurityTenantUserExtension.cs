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
	public partial class SecurityTenantUser : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SecurityTenantUserDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 securityTenantId { get; set; }
			[Required]
			public Int32 securityUserId { get; set; }
			[Required]
			public Boolean isOwner { get; set; }
			[Required]
			public Boolean canRead { get; set; }
			[Required]
			public Boolean canWrite { get; set; }
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
		public class SecurityTenantUserOutputDTO : SecurityTenantUserDTO
		{
			public SecurityTenant.SecurityTenantDTO securityTenant { get; set; }
			public SecurityUser.SecurityUserDTO securityUser { get; set; }
		}


		/// <summary>
		///
		/// Converts a SecurityTenantUser to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SecurityTenantUserDTO ToDTO()
		{
			return new SecurityTenantUserDTO
			{
				id = this.id,
				securityTenantId = this.securityTenantId,
				securityUserId = this.securityUserId,
				isOwner = this.isOwner,
				canRead = this.canRead,
				canWrite = this.canWrite,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SecurityTenantUser list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SecurityTenantUserDTO> ToDTOList(List<SecurityTenantUser> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityTenantUserDTO> output = new List<SecurityTenantUserDTO>();

			output.Capacity = data.Count;

			foreach (SecurityTenantUser securityTenantUser in data)
			{
				output.Add(securityTenantUser.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SecurityTenantUser to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SecurityTenantUserEntity type directly.
		///
		/// </summary>
		public SecurityTenantUserOutputDTO ToOutputDTO()
		{
			return new SecurityTenantUserOutputDTO
			{
				id = this.id,
				securityTenantId = this.securityTenantId,
				securityUserId = this.securityUserId,
				isOwner = this.isOwner,
				canRead = this.canRead,
				canWrite = this.canWrite,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				securityTenant = this.securityTenant?.ToDTO(),
				securityUser = this.securityUser?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a SecurityTenantUser list to list of Output Data Transfer Object intended to be used for serializing a list of SecurityTenantUser objects to avoid using the SecurityTenantUser entity type directly.
		///
		/// </summary>
		public static List<SecurityTenantUserOutputDTO> ToOutputDTOList(List<SecurityTenantUser> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityTenantUserOutputDTO> output = new List<SecurityTenantUserOutputDTO>();

			output.Capacity = data.Count;

			foreach (SecurityTenantUser securityTenantUser in data)
			{
				output.Add(securityTenantUser.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SecurityTenantUser Object.
		///
		/// </summary>
		public static Database.SecurityTenantUser FromDTO(SecurityTenantUserDTO dto)
		{
			return new Database.SecurityTenantUser
			{
				id = dto.id,
				securityTenantId = dto.securityTenantId,
				securityUserId = dto.securityUserId,
				isOwner = dto.isOwner,
				canRead = dto.canRead,
				canWrite = dto.canWrite,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SecurityTenantUser Object.
		///
		/// </summary>
		public void ApplyDTO(SecurityTenantUserDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.securityTenantId = dto.securityTenantId;
			this.securityUserId = dto.securityUserId;
			this.isOwner = dto.isOwner;
			this.canRead = dto.canRead;
			this.canWrite = dto.canWrite;
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
		/// Creates a deep copy clone of a SecurityTenantUser Object.
		///
		/// </summary>
		public SecurityTenantUser Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SecurityTenantUser{
				id = this.id,
				securityTenantId = this.securityTenantId,
				securityUserId = this.securityUserId,
				isOwner = this.isOwner,
				canRead = this.canRead,
				canWrite = this.canWrite,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityTenantUser Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityTenantUser Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SecurityTenantUser Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityTenantUser Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SecurityTenantUser securityTenantUser)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (securityTenantUser == null)
			{
				return null;
			}

			return new {
				id = securityTenantUser.id,
				securityTenantId = securityTenantUser.securityTenantId,
				securityUserId = securityTenantUser.securityUserId,
				isOwner = securityTenantUser.isOwner,
				canRead = securityTenantUser.canRead,
				canWrite = securityTenantUser.canWrite,
				objectGuid = securityTenantUser.objectGuid,
				active = securityTenantUser.active,
				deleted = securityTenantUser.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityTenantUser Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SecurityTenantUser securityTenantUser)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (securityTenantUser == null)
			{
				return null;
			}

			return new {
				id = securityTenantUser.id,
				securityTenantId = securityTenantUser.securityTenantId,
				securityUserId = securityTenantUser.securityUserId,
				isOwner = securityTenantUser.isOwner,
				canRead = securityTenantUser.canRead,
				canWrite = securityTenantUser.canWrite,
				objectGuid = securityTenantUser.objectGuid,
				active = securityTenantUser.active,
				deleted = securityTenantUser.deleted,
				securityTenant = SecurityTenant.CreateMinimalAnonymous(securityTenantUser.securityTenant),
				securityUser = SecurityUser.CreateMinimalAnonymous(securityTenantUser.securityUser)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SecurityTenantUser Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SecurityTenantUser securityTenantUser)
		{
			//
			// Return a very minimal object.
			//
			if (securityTenantUser == null)
			{
				return null;
			}

			return new {
				id = securityTenantUser.id,
				name = securityTenantUser.id,
				description = securityTenantUser.id
			 };
		}
	}
}
