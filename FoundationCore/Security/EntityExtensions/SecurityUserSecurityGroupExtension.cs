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
	public partial class SecurityUserSecurityGroup : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SecurityUserSecurityGroupDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 securityUserId { get; set; }
			[Required]
			public Int32 securityGroupId { get; set; }
			public String comments { get; set; }
			public Boolean? active { get; set; }
			public Boolean? deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class SecurityUserSecurityGroupOutputDTO : SecurityUserSecurityGroupDTO
		{
			public SecurityGroup.SecurityGroupDTO securityGroup { get; set; }
			public SecurityUser.SecurityUserDTO securityUser { get; set; }
		}


		/// <summary>
		///
		/// Converts a SecurityUserSecurityGroup to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SecurityUserSecurityGroupDTO ToDTO()
		{
			return new SecurityUserSecurityGroupDTO
			{
				id = this.id,
				securityUserId = this.securityUserId,
				securityGroupId = this.securityGroupId,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SecurityUserSecurityGroup list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SecurityUserSecurityGroupDTO> ToDTOList(List<SecurityUserSecurityGroup> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityUserSecurityGroupDTO> output = new List<SecurityUserSecurityGroupDTO>();

			output.Capacity = data.Count;

			foreach (SecurityUserSecurityGroup securityUserSecurityGroup in data)
			{
				output.Add(securityUserSecurityGroup.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SecurityUserSecurityGroup to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SecurityUserSecurityGroupEntity type directly.
		///
		/// </summary>
		public SecurityUserSecurityGroupOutputDTO ToOutputDTO()
		{
			return new SecurityUserSecurityGroupOutputDTO
			{
				id = this.id,
				securityUserId = this.securityUserId,
				securityGroupId = this.securityGroupId,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted,
				securityGroup = this.securityGroup?.ToDTO(),
				securityUser = this.securityUser?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a SecurityUserSecurityGroup list to list of Output Data Transfer Object intended to be used for serializing a list of SecurityUserSecurityGroup objects to avoid using the SecurityUserSecurityGroup entity type directly.
		///
		/// </summary>
		public static List<SecurityUserSecurityGroupOutputDTO> ToOutputDTOList(List<SecurityUserSecurityGroup> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityUserSecurityGroupOutputDTO> output = new List<SecurityUserSecurityGroupOutputDTO>();

			output.Capacity = data.Count;

			foreach (SecurityUserSecurityGroup securityUserSecurityGroup in data)
			{
				output.Add(securityUserSecurityGroup.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SecurityUserSecurityGroup Object.
		///
		/// </summary>
		public static Database.SecurityUserSecurityGroup FromDTO(SecurityUserSecurityGroupDTO dto)
		{
			return new Database.SecurityUserSecurityGroup
			{
				id = dto.id,
				securityUserId = dto.securityUserId,
				securityGroupId = dto.securityGroupId,
				comments = dto.comments,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SecurityUserSecurityGroup Object.
		///
		/// </summary>
		public void ApplyDTO(SecurityUserSecurityGroupDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.securityUserId = dto.securityUserId;
			this.securityGroupId = dto.securityGroupId;
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
		/// Creates a deep copy clone of a SecurityUserSecurityGroup Object.
		///
		/// </summary>
		public SecurityUserSecurityGroup Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SecurityUserSecurityGroup{
				id = this.id,
				securityUserId = this.securityUserId,
				securityGroupId = this.securityGroupId,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityUserSecurityGroup Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityUserSecurityGroup Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SecurityUserSecurityGroup Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityUserSecurityGroup Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SecurityUserSecurityGroup securityUserSecurityGroup)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (securityUserSecurityGroup == null)
			{
				return null;
			}

			return new {
				id = securityUserSecurityGroup.id,
				securityUserId = securityUserSecurityGroup.securityUserId,
				securityGroupId = securityUserSecurityGroup.securityGroupId,
				comments = securityUserSecurityGroup.comments,
				active = securityUserSecurityGroup.active,
				deleted = securityUserSecurityGroup.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityUserSecurityGroup Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SecurityUserSecurityGroup securityUserSecurityGroup)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (securityUserSecurityGroup == null)
			{
				return null;
			}

			return new {
				id = securityUserSecurityGroup.id,
				securityUserId = securityUserSecurityGroup.securityUserId,
				securityGroupId = securityUserSecurityGroup.securityGroupId,
				comments = securityUserSecurityGroup.comments,
				active = securityUserSecurityGroup.active,
				deleted = securityUserSecurityGroup.deleted,
				securityGroup = SecurityGroup.CreateMinimalAnonymous(securityUserSecurityGroup.securityGroup),
				securityUser = SecurityUser.CreateMinimalAnonymous(securityUserSecurityGroup.securityUser)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SecurityUserSecurityGroup Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SecurityUserSecurityGroup securityUserSecurityGroup)
		{
			//
			// Return a very minimal object.
			//
			if (securityUserSecurityGroup == null)
			{
				return null;
			}

			return new {
				id = securityUserSecurityGroup.id,
				name = securityUserSecurityGroup.comments,
				description = string.Join(", ", new[] { securityUserSecurityGroup.comments}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
