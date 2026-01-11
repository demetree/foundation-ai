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
	public partial class SecurityTeamUser : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SecurityTeamUserDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 securityTeamId { get; set; }
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
		public class SecurityTeamUserOutputDTO : SecurityTeamUserDTO
		{
			public SecurityTeam.SecurityTeamDTO securityTeam { get; set; }
			public SecurityUser.SecurityUserDTO securityUser { get; set; }
		}


		/// <summary>
		///
		/// Converts a SecurityTeamUser to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SecurityTeamUserDTO ToDTO()
		{
			return new SecurityTeamUserDTO
			{
				id = this.id,
				securityTeamId = this.securityTeamId,
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
		/// Converts a SecurityTeamUser list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SecurityTeamUserDTO> ToDTOList(List<SecurityTeamUser> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityTeamUserDTO> output = new List<SecurityTeamUserDTO>();

			output.Capacity = data.Count;

			foreach (SecurityTeamUser securityTeamUser in data)
			{
				output.Add(securityTeamUser.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SecurityTeamUser to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SecurityTeamUserEntity type directly.
		///
		/// </summary>
		public SecurityTeamUserOutputDTO ToOutputDTO()
		{
			return new SecurityTeamUserOutputDTO
			{
				id = this.id,
				securityTeamId = this.securityTeamId,
				securityUserId = this.securityUserId,
				canRead = this.canRead,
				canWrite = this.canWrite,
				canChangeHierarchy = this.canChangeHierarchy,
				canChangeOwner = this.canChangeOwner,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				securityTeam = this.securityTeam?.ToDTO(),
				securityUser = this.securityUser?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a SecurityTeamUser list to list of Output Data Transfer Object intended to be used for serializing a list of SecurityTeamUser objects to avoid using the SecurityTeamUser entity type directly.
		///
		/// </summary>
		public static List<SecurityTeamUserOutputDTO> ToOutputDTOList(List<SecurityTeamUser> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityTeamUserOutputDTO> output = new List<SecurityTeamUserOutputDTO>();

			output.Capacity = data.Count;

			foreach (SecurityTeamUser securityTeamUser in data)
			{
				output.Add(securityTeamUser.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SecurityTeamUser Object.
		///
		/// </summary>
		public static Database.SecurityTeamUser FromDTO(SecurityTeamUserDTO dto)
		{
			return new Database.SecurityTeamUser
			{
				id = dto.id,
				securityTeamId = dto.securityTeamId,
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
		/// Applies the values from an INPUT DTO to a SecurityTeamUser Object.
		///
		/// </summary>
		public void ApplyDTO(SecurityTeamUserDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.securityTeamId = dto.securityTeamId;
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
		/// Creates a deep copy clone of a SecurityTeamUser Object.
		///
		/// </summary>
		public SecurityTeamUser Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SecurityTeamUser{
				id = this.id,
				securityTeamId = this.securityTeamId,
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
        /// Creates an anonymous object containing properties from a SecurityTeamUser Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityTeamUser Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SecurityTeamUser Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityTeamUser Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SecurityTeamUser securityTeamUser)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (securityTeamUser == null)
			{
				return null;
			}

			return new {
				id = securityTeamUser.id,
				securityTeamId = securityTeamUser.securityTeamId,
				securityUserId = securityTeamUser.securityUserId,
				canRead = securityTeamUser.canRead,
				canWrite = securityTeamUser.canWrite,
				canChangeHierarchy = securityTeamUser.canChangeHierarchy,
				canChangeOwner = securityTeamUser.canChangeOwner,
				objectGuid = securityTeamUser.objectGuid,
				active = securityTeamUser.active,
				deleted = securityTeamUser.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityTeamUser Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SecurityTeamUser securityTeamUser)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (securityTeamUser == null)
			{
				return null;
			}

			return new {
				id = securityTeamUser.id,
				securityTeamId = securityTeamUser.securityTeamId,
				securityUserId = securityTeamUser.securityUserId,
				canRead = securityTeamUser.canRead,
				canWrite = securityTeamUser.canWrite,
				canChangeHierarchy = securityTeamUser.canChangeHierarchy,
				canChangeOwner = securityTeamUser.canChangeOwner,
				objectGuid = securityTeamUser.objectGuid,
				active = securityTeamUser.active,
				deleted = securityTeamUser.deleted,
				securityTeam = SecurityTeam.CreateMinimalAnonymous(securityTeamUser.securityTeam),
				securityUser = SecurityUser.CreateMinimalAnonymous(securityTeamUser.securityUser)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SecurityTeamUser Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SecurityTeamUser securityTeamUser)
		{
			//
			// Return a very minimal object.
			//
			if (securityTeamUser == null)
			{
				return null;
			}

			return new {
				id = securityTeamUser.id,
				name = securityTeamUser.id,
				description = securityTeamUser.id
			 };
		}
	}
}
