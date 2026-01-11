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
	public partial class SecurityTeam : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SecurityTeamDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 securityDepartmentId { get; set; }
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
		public class SecurityTeamOutputDTO : SecurityTeamDTO
		{
			public SecurityDepartment.SecurityDepartmentDTO securityDepartment { get; set; }
		}


		/// <summary>
		///
		/// Converts a SecurityTeam to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SecurityTeamDTO ToDTO()
		{
			return new SecurityTeamDTO
			{
				id = this.id,
				securityDepartmentId = this.securityDepartmentId,
				name = this.name,
				description = this.description,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SecurityTeam list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SecurityTeamDTO> ToDTOList(List<SecurityTeam> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityTeamDTO> output = new List<SecurityTeamDTO>();

			output.Capacity = data.Count;

			foreach (SecurityTeam securityTeam in data)
			{
				output.Add(securityTeam.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SecurityTeam to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SecurityTeamEntity type directly.
		///
		/// </summary>
		public SecurityTeamOutputDTO ToOutputDTO()
		{
			return new SecurityTeamOutputDTO
			{
				id = this.id,
				securityDepartmentId = this.securityDepartmentId,
				name = this.name,
				description = this.description,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				securityDepartment = this.securityDepartment?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a SecurityTeam list to list of Output Data Transfer Object intended to be used for serializing a list of SecurityTeam objects to avoid using the SecurityTeam entity type directly.
		///
		/// </summary>
		public static List<SecurityTeamOutputDTO> ToOutputDTOList(List<SecurityTeam> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityTeamOutputDTO> output = new List<SecurityTeamOutputDTO>();

			output.Capacity = data.Count;

			foreach (SecurityTeam securityTeam in data)
			{
				output.Add(securityTeam.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SecurityTeam Object.
		///
		/// </summary>
		public static Database.SecurityTeam FromDTO(SecurityTeamDTO dto)
		{
			return new Database.SecurityTeam
			{
				id = dto.id,
				securityDepartmentId = dto.securityDepartmentId,
				name = dto.name,
				description = dto.description,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SecurityTeam Object.
		///
		/// </summary>
		public void ApplyDTO(SecurityTeamDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.securityDepartmentId = dto.securityDepartmentId;
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
		/// Creates a deep copy clone of a SecurityTeam Object.
		///
		/// </summary>
		public SecurityTeam Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SecurityTeam{
				id = this.id,
				securityDepartmentId = this.securityDepartmentId,
				name = this.name,
				description = this.description,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityTeam Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityTeam Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SecurityTeam Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityTeam Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SecurityTeam securityTeam)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (securityTeam == null)
			{
				return null;
			}

			return new {
				id = securityTeam.id,
				securityDepartmentId = securityTeam.securityDepartmentId,
				name = securityTeam.name,
				description = securityTeam.description,
				objectGuid = securityTeam.objectGuid,
				active = securityTeam.active,
				deleted = securityTeam.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityTeam Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SecurityTeam securityTeam)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (securityTeam == null)
			{
				return null;
			}

			return new {
				id = securityTeam.id,
				securityDepartmentId = securityTeam.securityDepartmentId,
				name = securityTeam.name,
				description = securityTeam.description,
				objectGuid = securityTeam.objectGuid,
				active = securityTeam.active,
				deleted = securityTeam.deleted,
				securityDepartment = SecurityDepartment.CreateMinimalAnonymous(securityTeam.securityDepartment)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SecurityTeam Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SecurityTeam securityTeam)
		{
			//
			// Return a very minimal object.
			//
			if (securityTeam == null)
			{
				return null;
			}

			return new {
				id = securityTeam.id,
				name = securityTeam.name,
				description = securityTeam.description,
			 };
		}
	}
}
