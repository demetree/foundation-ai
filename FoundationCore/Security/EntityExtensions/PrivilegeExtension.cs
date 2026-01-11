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
	public partial class Privilege : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class PrivilegeDTO
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
		public class PrivilegeOutputDTO : PrivilegeDTO
		{
		}


		/// <summary>
		///
		/// Converts a Privilege to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public PrivilegeDTO ToDTO()
		{
			return new PrivilegeDTO
			{
				id = this.id,
				name = this.name,
				description = this.description
			};
		}


		/// <summary>
		///
		/// Converts a Privilege list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<PrivilegeDTO> ToDTOList(List<Privilege> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PrivilegeDTO> output = new List<PrivilegeDTO>();

			output.Capacity = data.Count;

			foreach (Privilege privilege in data)
			{
				output.Add(privilege.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a Privilege to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the PrivilegeEntity type directly.
		///
		/// </summary>
		public PrivilegeOutputDTO ToOutputDTO()
		{
			return new PrivilegeOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description
			};
		}


		/// <summary>
		///
		/// Converts a Privilege list to list of Output Data Transfer Object intended to be used for serializing a list of Privilege objects to avoid using the Privilege entity type directly.
		///
		/// </summary>
		public static List<PrivilegeOutputDTO> ToOutputDTOList(List<Privilege> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PrivilegeOutputDTO> output = new List<PrivilegeOutputDTO>();

			output.Capacity = data.Count;

			foreach (Privilege privilege in data)
			{
				output.Add(privilege.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a Privilege Object.
		///
		/// </summary>
		public static Database.Privilege FromDTO(PrivilegeDTO dto)
		{
			return new Database.Privilege
			{
				id = dto.id,
				name = dto.name,
				description = dto.description
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a Privilege Object.
		///
		/// </summary>
		public void ApplyDTO(PrivilegeDTO dto)
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
		/// Creates a deep copy clone of a Privilege Object.
		///
		/// </summary>
		public Privilege Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new Privilege{
				id = this.id,
				name = this.name,
				description = this.description,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Privilege Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Privilege Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a Privilege Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a Privilege Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.Privilege privilege)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (privilege == null)
			{
				return null;
			}

			return new {
				id = privilege.id,
				name = privilege.name,
				description = privilege.description,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a Privilege Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(Privilege privilege)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (privilege == null)
			{
				return null;
			}

			return new {
				id = privilege.id,
				name = privilege.name,
				description = privilege.description,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a Privilege Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(Privilege privilege)
		{
			//
			// Return a very minimal object.
			//
			if (privilege == null)
			{
				return null;
			}

			return new {
				id = privilege.id,
				name = privilege.name,
				description = privilege.description,
			 };
		}
	}
}
