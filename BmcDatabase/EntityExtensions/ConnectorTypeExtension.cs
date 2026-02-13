using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.BMC.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class ConnectorType : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ConnectorTypeDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
			public Int32? degreesOfFreedom { get; set; }
			[Required]
			public Boolean allowsRotation { get; set; }
			[Required]
			public Boolean allowsSlide { get; set; }
			public Int32? sequence { get; set; }
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
		public class ConnectorTypeOutputDTO : ConnectorTypeDTO
		{
		}


		/// <summary>
		///
		/// Converts a ConnectorType to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ConnectorTypeDTO ToDTO()
		{
			return new ConnectorTypeDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				degreesOfFreedom = this.degreesOfFreedom,
				allowsRotation = this.allowsRotation,
				allowsSlide = this.allowsSlide,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ConnectorType list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ConnectorTypeDTO> ToDTOList(List<ConnectorType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConnectorTypeDTO> output = new List<ConnectorTypeDTO>();

			output.Capacity = data.Count;

			foreach (ConnectorType connectorType in data)
			{
				output.Add(connectorType.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ConnectorType to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ConnectorTypeEntity type directly.
		///
		/// </summary>
		public ConnectorTypeOutputDTO ToOutputDTO()
		{
			return new ConnectorTypeOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				degreesOfFreedom = this.degreesOfFreedom,
				allowsRotation = this.allowsRotation,
				allowsSlide = this.allowsSlide,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ConnectorType list to list of Output Data Transfer Object intended to be used for serializing a list of ConnectorType objects to avoid using the ConnectorType entity type directly.
		///
		/// </summary>
		public static List<ConnectorTypeOutputDTO> ToOutputDTOList(List<ConnectorType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConnectorTypeOutputDTO> output = new List<ConnectorTypeOutputDTO>();

			output.Capacity = data.Count;

			foreach (ConnectorType connectorType in data)
			{
				output.Add(connectorType.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ConnectorType Object.
		///
		/// </summary>
		public static Database.ConnectorType FromDTO(ConnectorTypeDTO dto)
		{
			return new Database.ConnectorType
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				degreesOfFreedom = dto.degreesOfFreedom,
				allowsRotation = dto.allowsRotation,
				allowsSlide = dto.allowsSlide,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ConnectorType Object.
		///
		/// </summary>
		public void ApplyDTO(ConnectorTypeDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.degreesOfFreedom = dto.degreesOfFreedom;
			this.allowsRotation = dto.allowsRotation;
			this.allowsSlide = dto.allowsSlide;
			this.sequence = dto.sequence;
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
		/// Creates a deep copy clone of a ConnectorType Object.
		///
		/// </summary>
		public ConnectorType Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ConnectorType{
				id = this.id,
				name = this.name,
				description = this.description,
				degreesOfFreedom = this.degreesOfFreedom,
				allowsRotation = this.allowsRotation,
				allowsSlide = this.allowsSlide,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConnectorType Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConnectorType Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ConnectorType Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ConnectorType Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ConnectorType connectorType)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (connectorType == null)
			{
				return null;
			}

			return new {
				id = connectorType.id,
				name = connectorType.name,
				description = connectorType.description,
				degreesOfFreedom = connectorType.degreesOfFreedom,
				allowsRotation = connectorType.allowsRotation,
				allowsSlide = connectorType.allowsSlide,
				sequence = connectorType.sequence,
				objectGuid = connectorType.objectGuid,
				active = connectorType.active,
				deleted = connectorType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ConnectorType Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ConnectorType connectorType)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (connectorType == null)
			{
				return null;
			}

			return new {
				id = connectorType.id,
				name = connectorType.name,
				description = connectorType.description,
				degreesOfFreedom = connectorType.degreesOfFreedom,
				allowsRotation = connectorType.allowsRotation,
				allowsSlide = connectorType.allowsSlide,
				sequence = connectorType.sequence,
				objectGuid = connectorType.objectGuid,
				active = connectorType.active,
				deleted = connectorType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ConnectorType Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ConnectorType connectorType)
		{
			//
			// Return a very minimal object.
			//
			if (connectorType == null)
			{
				return null;
			}

			return new {
				id = connectorType.id,
				name = connectorType.name,
				description = connectorType.description,
			 };
		}
	}
}
