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
	public partial class SecurityUserEventType : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SecurityUserEventTypeDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			public String description { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class SecurityUserEventTypeOutputDTO : SecurityUserEventTypeDTO
		{
		}


		/// <summary>
		///
		/// Converts a SecurityUserEventType to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SecurityUserEventTypeDTO ToDTO()
		{
			return new SecurityUserEventTypeDTO
			{
				id = this.id,
				name = this.name,
				description = this.description
			};
		}


		/// <summary>
		///
		/// Converts a SecurityUserEventType list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SecurityUserEventTypeDTO> ToDTOList(List<SecurityUserEventType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityUserEventTypeDTO> output = new List<SecurityUserEventTypeDTO>();

			output.Capacity = data.Count;

			foreach (SecurityUserEventType securityUserEventType in data)
			{
				output.Add(securityUserEventType.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SecurityUserEventType to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SecurityUserEventTypeEntity type directly.
		///
		/// </summary>
		public SecurityUserEventTypeOutputDTO ToOutputDTO()
		{
			return new SecurityUserEventTypeOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description
			};
		}


		/// <summary>
		///
		/// Converts a SecurityUserEventType list to list of Output Data Transfer Object intended to be used for serializing a list of SecurityUserEventType objects to avoid using the SecurityUserEventType entity type directly.
		///
		/// </summary>
		public static List<SecurityUserEventTypeOutputDTO> ToOutputDTOList(List<SecurityUserEventType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityUserEventTypeOutputDTO> output = new List<SecurityUserEventTypeOutputDTO>();

			output.Capacity = data.Count;

			foreach (SecurityUserEventType securityUserEventType in data)
			{
				output.Add(securityUserEventType.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SecurityUserEventType Object.
		///
		/// </summary>
		public static Database.SecurityUserEventType FromDTO(SecurityUserEventTypeDTO dto)
		{
			return new Database.SecurityUserEventType
			{
				id = dto.id,
				name = dto.name,
				description = dto.description
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SecurityUserEventType Object.
		///
		/// </summary>
		public void ApplyDTO(SecurityUserEventTypeDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a SecurityUserEventType Object.
		///
		/// </summary>
		public SecurityUserEventType Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SecurityUserEventType{
				id = this.id,
				name = this.name,
				description = this.description,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityUserEventType Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityUserEventType Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SecurityUserEventType Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityUserEventType Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SecurityUserEventType securityUserEventType)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (securityUserEventType == null)
			{
				return null;
			}

			return new {
				id = securityUserEventType.id,
				name = securityUserEventType.name,
				description = securityUserEventType.description,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityUserEventType Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SecurityUserEventType securityUserEventType)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (securityUserEventType == null)
			{
				return null;
			}

			return new {
				id = securityUserEventType.id,
				name = securityUserEventType.name,
				description = securityUserEventType.description,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SecurityUserEventType Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SecurityUserEventType securityUserEventType)
		{
			//
			// Return a very minimal object.
			//
			if (securityUserEventType == null)
			{
				return null;
			}

			return new {
				id = securityUserEventType.id,
				name = securityUserEventType.name,
				description = securityUserEventType.description,
			 };
		}
	}
}
