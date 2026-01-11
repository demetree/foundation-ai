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
	public partial class SecurityOrganization : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SecurityOrganizationDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 securityTenantId { get; set; }
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
		public class SecurityOrganizationOutputDTO : SecurityOrganizationDTO
		{
			public SecurityTenant.SecurityTenantDTO securityTenant { get; set; }
		}


		/// <summary>
		///
		/// Converts a SecurityOrganization to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SecurityOrganizationDTO ToDTO()
		{
			return new SecurityOrganizationDTO
			{
				id = this.id,
				securityTenantId = this.securityTenantId,
				name = this.name,
				description = this.description,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SecurityOrganization list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SecurityOrganizationDTO> ToDTOList(List<SecurityOrganization> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityOrganizationDTO> output = new List<SecurityOrganizationDTO>();

			output.Capacity = data.Count;

			foreach (SecurityOrganization securityOrganization in data)
			{
				output.Add(securityOrganization.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SecurityOrganization to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SecurityOrganizationEntity type directly.
		///
		/// </summary>
		public SecurityOrganizationOutputDTO ToOutputDTO()
		{
			return new SecurityOrganizationOutputDTO
			{
				id = this.id,
				securityTenantId = this.securityTenantId,
				name = this.name,
				description = this.description,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				securityTenant = this.securityTenant?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a SecurityOrganization list to list of Output Data Transfer Object intended to be used for serializing a list of SecurityOrganization objects to avoid using the SecurityOrganization entity type directly.
		///
		/// </summary>
		public static List<SecurityOrganizationOutputDTO> ToOutputDTOList(List<SecurityOrganization> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityOrganizationOutputDTO> output = new List<SecurityOrganizationOutputDTO>();

			output.Capacity = data.Count;

			foreach (SecurityOrganization securityOrganization in data)
			{
				output.Add(securityOrganization.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SecurityOrganization Object.
		///
		/// </summary>
		public static Database.SecurityOrganization FromDTO(SecurityOrganizationDTO dto)
		{
			return new Database.SecurityOrganization
			{
				id = dto.id,
				securityTenantId = dto.securityTenantId,
				name = dto.name,
				description = dto.description,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SecurityOrganization Object.
		///
		/// </summary>
		public void ApplyDTO(SecurityOrganizationDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.securityTenantId = dto.securityTenantId;
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
		/// Creates a deep copy clone of a SecurityOrganization Object.
		///
		/// </summary>
		public SecurityOrganization Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SecurityOrganization{
				id = this.id,
				securityTenantId = this.securityTenantId,
				name = this.name,
				description = this.description,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityOrganization Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityOrganization Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SecurityOrganization Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityOrganization Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SecurityOrganization securityOrganization)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (securityOrganization == null)
			{
				return null;
			}

			return new {
				id = securityOrganization.id,
				securityTenantId = securityOrganization.securityTenantId,
				name = securityOrganization.name,
				description = securityOrganization.description,
				objectGuid = securityOrganization.objectGuid,
				active = securityOrganization.active,
				deleted = securityOrganization.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityOrganization Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SecurityOrganization securityOrganization)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (securityOrganization == null)
			{
				return null;
			}

			return new {
				id = securityOrganization.id,
				securityTenantId = securityOrganization.securityTenantId,
				name = securityOrganization.name,
				description = securityOrganization.description,
				objectGuid = securityOrganization.objectGuid,
				active = securityOrganization.active,
				deleted = securityOrganization.deleted,
				securityTenant = SecurityTenant.CreateMinimalAnonymous(securityOrganization.securityTenant)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SecurityOrganization Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SecurityOrganization securityOrganization)
		{
			//
			// Return a very minimal object.
			//
			if (securityOrganization == null)
			{
				return null;
			}

			return new {
				id = securityOrganization.id,
				name = securityOrganization.name,
				description = securityOrganization.description,
			 };
		}
	}
}
