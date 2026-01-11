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
	public partial class SecurityTenant : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SecurityTenantDTO
		{
			public Int32 id { get; set; }
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
		public class SecurityTenantOutputDTO : SecurityTenantDTO
		{
		}


		/// <summary>
		///
		/// Converts a SecurityTenant to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SecurityTenantDTO ToDTO()
		{
			return new SecurityTenantDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SecurityTenant list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SecurityTenantDTO> ToDTOList(List<SecurityTenant> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityTenantDTO> output = new List<SecurityTenantDTO>();

			output.Capacity = data.Count;

			foreach (SecurityTenant securityTenant in data)
			{
				output.Add(securityTenant.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SecurityTenant to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SecurityTenantEntity type directly.
		///
		/// </summary>
		public SecurityTenantOutputDTO ToOutputDTO()
		{
			return new SecurityTenantOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SecurityTenant list to list of Output Data Transfer Object intended to be used for serializing a list of SecurityTenant objects to avoid using the SecurityTenant entity type directly.
		///
		/// </summary>
		public static List<SecurityTenantOutputDTO> ToOutputDTOList(List<SecurityTenant> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityTenantOutputDTO> output = new List<SecurityTenantOutputDTO>();

			output.Capacity = data.Count;

			foreach (SecurityTenant securityTenant in data)
			{
				output.Add(securityTenant.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SecurityTenant Object.
		///
		/// </summary>
		public static Database.SecurityTenant FromDTO(SecurityTenantDTO dto)
		{
			return new Database.SecurityTenant
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SecurityTenant Object.
		///
		/// </summary>
		public void ApplyDTO(SecurityTenantDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

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
		/// Creates a deep copy clone of a SecurityTenant Object.
		///
		/// </summary>
		public SecurityTenant Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SecurityTenant{
				id = this.id,
				name = this.name,
				description = this.description,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityTenant Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityTenant Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SecurityTenant Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityTenant Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SecurityTenant securityTenant)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (securityTenant == null)
			{
				return null;
			}

			return new {
				id = securityTenant.id,
				name = securityTenant.name,
				description = securityTenant.description,
				objectGuid = securityTenant.objectGuid,
				active = securityTenant.active,
				deleted = securityTenant.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityTenant Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SecurityTenant securityTenant)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (securityTenant == null)
			{
				return null;
			}

			return new {
				id = securityTenant.id,
				name = securityTenant.name,
				description = securityTenant.description,
				objectGuid = securityTenant.objectGuid,
				active = securityTenant.active,
				deleted = securityTenant.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SecurityTenant Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SecurityTenant securityTenant)
		{
			//
			// Return a very minimal object.
			//
			if (securityTenant == null)
			{
				return null;
			}

			return new {
				id = securityTenant.id,
				name = securityTenant.name,
				description = securityTenant.description,
			 };
		}
	}
}
