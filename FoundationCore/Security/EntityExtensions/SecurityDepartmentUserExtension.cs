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
	public partial class SecurityDepartmentUser : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SecurityDepartmentUserDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 securityDepartmentId { get; set; }
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
		public class SecurityDepartmentUserOutputDTO : SecurityDepartmentUserDTO
		{
			public SecurityDepartment.SecurityDepartmentDTO securityDepartment { get; set; }
			public SecurityUser.SecurityUserDTO securityUser { get; set; }
		}


		/// <summary>
		///
		/// Converts a SecurityDepartmentUser to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SecurityDepartmentUserDTO ToDTO()
		{
			return new SecurityDepartmentUserDTO
			{
				id = this.id,
				securityDepartmentId = this.securityDepartmentId,
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
		/// Converts a SecurityDepartmentUser list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SecurityDepartmentUserDTO> ToDTOList(List<SecurityDepartmentUser> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityDepartmentUserDTO> output = new List<SecurityDepartmentUserDTO>();

			output.Capacity = data.Count;

			foreach (SecurityDepartmentUser securityDepartmentUser in data)
			{
				output.Add(securityDepartmentUser.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SecurityDepartmentUser to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SecurityDepartmentUserEntity type directly.
		///
		/// </summary>
		public SecurityDepartmentUserOutputDTO ToOutputDTO()
		{
			return new SecurityDepartmentUserOutputDTO
			{
				id = this.id,
				securityDepartmentId = this.securityDepartmentId,
				securityUserId = this.securityUserId,
				canRead = this.canRead,
				canWrite = this.canWrite,
				canChangeHierarchy = this.canChangeHierarchy,
				canChangeOwner = this.canChangeOwner,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				securityDepartment = this.securityDepartment?.ToDTO(),
				securityUser = this.securityUser?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a SecurityDepartmentUser list to list of Output Data Transfer Object intended to be used for serializing a list of SecurityDepartmentUser objects to avoid using the SecurityDepartmentUser entity type directly.
		///
		/// </summary>
		public static List<SecurityDepartmentUserOutputDTO> ToOutputDTOList(List<SecurityDepartmentUser> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityDepartmentUserOutputDTO> output = new List<SecurityDepartmentUserOutputDTO>();

			output.Capacity = data.Count;

			foreach (SecurityDepartmentUser securityDepartmentUser in data)
			{
				output.Add(securityDepartmentUser.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SecurityDepartmentUser Object.
		///
		/// </summary>
		public static Database.SecurityDepartmentUser FromDTO(SecurityDepartmentUserDTO dto)
		{
			return new Database.SecurityDepartmentUser
			{
				id = dto.id,
				securityDepartmentId = dto.securityDepartmentId,
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
		/// Applies the values from an INPUT DTO to a SecurityDepartmentUser Object.
		///
		/// </summary>
		public void ApplyDTO(SecurityDepartmentUserDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.securityDepartmentId = dto.securityDepartmentId;
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
		/// Creates a deep copy clone of a SecurityDepartmentUser Object.
		///
		/// </summary>
		public SecurityDepartmentUser Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SecurityDepartmentUser{
				id = this.id,
				securityDepartmentId = this.securityDepartmentId,
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
        /// Creates an anonymous object containing properties from a SecurityDepartmentUser Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityDepartmentUser Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SecurityDepartmentUser Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityDepartmentUser Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SecurityDepartmentUser securityDepartmentUser)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (securityDepartmentUser == null)
			{
				return null;
			}

			return new {
				id = securityDepartmentUser.id,
				securityDepartmentId = securityDepartmentUser.securityDepartmentId,
				securityUserId = securityDepartmentUser.securityUserId,
				canRead = securityDepartmentUser.canRead,
				canWrite = securityDepartmentUser.canWrite,
				canChangeHierarchy = securityDepartmentUser.canChangeHierarchy,
				canChangeOwner = securityDepartmentUser.canChangeOwner,
				objectGuid = securityDepartmentUser.objectGuid,
				active = securityDepartmentUser.active,
				deleted = securityDepartmentUser.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityDepartmentUser Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SecurityDepartmentUser securityDepartmentUser)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (securityDepartmentUser == null)
			{
				return null;
			}

			return new {
				id = securityDepartmentUser.id,
				securityDepartmentId = securityDepartmentUser.securityDepartmentId,
				securityUserId = securityDepartmentUser.securityUserId,
				canRead = securityDepartmentUser.canRead,
				canWrite = securityDepartmentUser.canWrite,
				canChangeHierarchy = securityDepartmentUser.canChangeHierarchy,
				canChangeOwner = securityDepartmentUser.canChangeOwner,
				objectGuid = securityDepartmentUser.objectGuid,
				active = securityDepartmentUser.active,
				deleted = securityDepartmentUser.deleted,
				securityDepartment = SecurityDepartment.CreateMinimalAnonymous(securityDepartmentUser.securityDepartment),
				securityUser = SecurityUser.CreateMinimalAnonymous(securityDepartmentUser.securityUser)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SecurityDepartmentUser Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SecurityDepartmentUser securityDepartmentUser)
		{
			//
			// Return a very minimal object.
			//
			if (securityDepartmentUser == null)
			{
				return null;
			}

			return new {
				id = securityDepartmentUser.id,
				name = securityDepartmentUser.id,
				description = securityDepartmentUser.id
			 };
		}
	}
}
