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
	public partial class SecurityGroup : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SecurityGroupDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			public String description { get; set; }
			public Boolean? active { get; set; }
			public Boolean? deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class SecurityGroupOutputDTO : SecurityGroupDTO
		{
		}


		/// <summary>
		///
		/// Converts a SecurityGroup to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SecurityGroupDTO ToDTO()
		{
			return new SecurityGroupDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SecurityGroup list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SecurityGroupDTO> ToDTOList(List<SecurityGroup> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityGroupDTO> output = new List<SecurityGroupDTO>();

			output.Capacity = data.Count;

			foreach (SecurityGroup securityGroup in data)
			{
				output.Add(securityGroup.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SecurityGroup to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SecurityGroupEntity type directly.
		///
		/// </summary>
		public SecurityGroupOutputDTO ToOutputDTO()
		{
			return new SecurityGroupOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SecurityGroup list to list of Output Data Transfer Object intended to be used for serializing a list of SecurityGroup objects to avoid using the SecurityGroup entity type directly.
		///
		/// </summary>
		public static List<SecurityGroupOutputDTO> ToOutputDTOList(List<SecurityGroup> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityGroupOutputDTO> output = new List<SecurityGroupOutputDTO>();

			output.Capacity = data.Count;

			foreach (SecurityGroup securityGroup in data)
			{
				output.Add(securityGroup.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SecurityGroup Object.
		///
		/// </summary>
		public static Database.SecurityGroup FromDTO(SecurityGroupDTO dto)
		{
			return new Database.SecurityGroup
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SecurityGroup Object.
		///
		/// </summary>
		public void ApplyDTO(SecurityGroupDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
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
		/// Creates a deep copy clone of a SecurityGroup Object.
		///
		/// </summary>
		public SecurityGroup Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SecurityGroup{
				id = this.id,
				name = this.name,
				description = this.description,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityGroup Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityGroup Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SecurityGroup Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityGroup Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SecurityGroup securityGroup)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (securityGroup == null)
			{
				return null;
			}

			return new {
				id = securityGroup.id,
				name = securityGroup.name,
				description = securityGroup.description,
				active = securityGroup.active,
				deleted = securityGroup.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityGroup Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SecurityGroup securityGroup)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (securityGroup == null)
			{
				return null;
			}

			return new {
				id = securityGroup.id,
				name = securityGroup.name,
				description = securityGroup.description,
				active = securityGroup.active,
				deleted = securityGroup.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SecurityGroup Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SecurityGroup securityGroup)
		{
			//
			// Return a very minimal object.
			//
			if (securityGroup == null)
			{
				return null;
			}

			return new {
				id = securityGroup.id,
				name = securityGroup.name,
				description = securityGroup.description,
			 };
		}
	}
}
