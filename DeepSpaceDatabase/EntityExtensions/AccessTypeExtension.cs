using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.DeepSpace.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class AccessType : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class AccessTypeDTO
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
		public class AccessTypeOutputDTO : AccessTypeDTO
		{
		}


		/// <summary>
		///
		/// Converts a AccessType to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public AccessTypeDTO ToDTO()
		{
			return new AccessTypeDTO
			{
				id = this.id,
				name = this.name,
				description = this.description
			};
		}


		/// <summary>
		///
		/// Converts a AccessType list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<AccessTypeDTO> ToDTOList(List<AccessType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AccessTypeDTO> output = new List<AccessTypeDTO>();

			output.Capacity = data.Count;

			foreach (AccessType accessType in data)
			{
				output.Add(accessType.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a AccessType to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the AccessType Entity type directly.
		///
		/// </summary>
		public AccessTypeOutputDTO ToOutputDTO()
		{
			return new AccessTypeOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description
			};
		}


		/// <summary>
		///
		/// Converts a AccessType list to list of Output Data Transfer Object intended to be used for serializing a list of AccessType objects to avoid using the AccessType entity type directly.
		///
		/// </summary>
		public static List<AccessTypeOutputDTO> ToOutputDTOList(List<AccessType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AccessTypeOutputDTO> output = new List<AccessTypeOutputDTO>();

			output.Capacity = data.Count;

			foreach (AccessType accessType in data)
			{
				output.Add(accessType.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a AccessType Object.
		///
		/// </summary>
		public static Database.AccessType FromDTO(AccessTypeDTO dto)
		{
			return new Database.AccessType
			{
				id = dto.id,
				name = dto.name,
				description = dto.description
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a AccessType Object.
		///
		/// </summary>
		public void ApplyDTO(AccessTypeDTO dto)
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
		/// Creates a deep copy clone of a AccessType Object.
		///
		/// </summary>
		public AccessType Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new AccessType{
				id = this.id,
				name = this.name,
				description = this.description,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AccessType Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AccessType Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a AccessType Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a AccessType Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.AccessType accessType)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (accessType == null)
			{
				return null;
			}

			return new {
				id = accessType.id,
				name = accessType.name,
				description = accessType.description,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a AccessType Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(AccessType accessType)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (accessType == null)
			{
				return null;
			}

			return new {
				id = accessType.id,
				name = accessType.name,
				description = accessType.description,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a AccessType Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(AccessType accessType)
		{
			//
			// Return a very minimal object.
			//
			if (accessType == null)
			{
				return null;
			}

			return new {
				id = accessType.id,
				name = accessType.name,
				description = accessType.description,
			 };
		}
	}
}
