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
	public partial class ConnectorTypeCompatibility : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ConnectorTypeCompatibilityDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 maleConnectorTypeId { get; set; }
			[Required]
			public Int32 femaleConnectorTypeId { get; set; }
			[Required]
			public String connectionStrength { get; set; }
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
		public class ConnectorTypeCompatibilityOutputDTO : ConnectorTypeCompatibilityDTO
		{
			public ConnectorType.ConnectorTypeDTO femaleConnectorType { get; set; }
			public ConnectorType.ConnectorTypeDTO maleConnectorType { get; set; }
		}


		/// <summary>
		///
		/// Converts a ConnectorTypeCompatibility to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ConnectorTypeCompatibilityDTO ToDTO()
		{
			return new ConnectorTypeCompatibilityDTO
			{
				id = this.id,
				maleConnectorTypeId = this.maleConnectorTypeId,
				femaleConnectorTypeId = this.femaleConnectorTypeId,
				connectionStrength = this.connectionStrength,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ConnectorTypeCompatibility list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ConnectorTypeCompatibilityDTO> ToDTOList(List<ConnectorTypeCompatibility> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConnectorTypeCompatibilityDTO> output = new List<ConnectorTypeCompatibilityDTO>();

			output.Capacity = data.Count;

			foreach (ConnectorTypeCompatibility connectorTypeCompatibility in data)
			{
				output.Add(connectorTypeCompatibility.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ConnectorTypeCompatibility to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ConnectorTypeCompatibilityEntity type directly.
		///
		/// </summary>
		public ConnectorTypeCompatibilityOutputDTO ToOutputDTO()
		{
			return new ConnectorTypeCompatibilityOutputDTO
			{
				id = this.id,
				maleConnectorTypeId = this.maleConnectorTypeId,
				femaleConnectorTypeId = this.femaleConnectorTypeId,
				connectionStrength = this.connectionStrength,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				femaleConnectorType = this.femaleConnectorType?.ToDTO(),
				maleConnectorType = this.maleConnectorType?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ConnectorTypeCompatibility list to list of Output Data Transfer Object intended to be used for serializing a list of ConnectorTypeCompatibility objects to avoid using the ConnectorTypeCompatibility entity type directly.
		///
		/// </summary>
		public static List<ConnectorTypeCompatibilityOutputDTO> ToOutputDTOList(List<ConnectorTypeCompatibility> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConnectorTypeCompatibilityOutputDTO> output = new List<ConnectorTypeCompatibilityOutputDTO>();

			output.Capacity = data.Count;

			foreach (ConnectorTypeCompatibility connectorTypeCompatibility in data)
			{
				output.Add(connectorTypeCompatibility.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ConnectorTypeCompatibility Object.
		///
		/// </summary>
		public static Database.ConnectorTypeCompatibility FromDTO(ConnectorTypeCompatibilityDTO dto)
		{
			return new Database.ConnectorTypeCompatibility
			{
				id = dto.id,
				maleConnectorTypeId = dto.maleConnectorTypeId,
				femaleConnectorTypeId = dto.femaleConnectorTypeId,
				connectionStrength = dto.connectionStrength,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ConnectorTypeCompatibility Object.
		///
		/// </summary>
		public void ApplyDTO(ConnectorTypeCompatibilityDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.maleConnectorTypeId = dto.maleConnectorTypeId;
			this.femaleConnectorTypeId = dto.femaleConnectorTypeId;
			this.connectionStrength = dto.connectionStrength;
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
		/// Creates a deep copy clone of a ConnectorTypeCompatibility Object.
		///
		/// </summary>
		public ConnectorTypeCompatibility Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ConnectorTypeCompatibility{
				id = this.id,
				maleConnectorTypeId = this.maleConnectorTypeId,
				femaleConnectorTypeId = this.femaleConnectorTypeId,
				connectionStrength = this.connectionStrength,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConnectorTypeCompatibility Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConnectorTypeCompatibility Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ConnectorTypeCompatibility Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ConnectorTypeCompatibility Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ConnectorTypeCompatibility connectorTypeCompatibility)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (connectorTypeCompatibility == null)
			{
				return null;
			}

			return new {
				id = connectorTypeCompatibility.id,
				maleConnectorTypeId = connectorTypeCompatibility.maleConnectorTypeId,
				femaleConnectorTypeId = connectorTypeCompatibility.femaleConnectorTypeId,
				connectionStrength = connectorTypeCompatibility.connectionStrength,
				objectGuid = connectorTypeCompatibility.objectGuid,
				active = connectorTypeCompatibility.active,
				deleted = connectorTypeCompatibility.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ConnectorTypeCompatibility Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ConnectorTypeCompatibility connectorTypeCompatibility)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (connectorTypeCompatibility == null)
			{
				return null;
			}

			return new {
				id = connectorTypeCompatibility.id,
				maleConnectorTypeId = connectorTypeCompatibility.maleConnectorTypeId,
				femaleConnectorTypeId = connectorTypeCompatibility.femaleConnectorTypeId,
				connectionStrength = connectorTypeCompatibility.connectionStrength,
				objectGuid = connectorTypeCompatibility.objectGuid,
				active = connectorTypeCompatibility.active,
				deleted = connectorTypeCompatibility.deleted,
				femaleConnectorType = ConnectorType.CreateMinimalAnonymous(connectorTypeCompatibility.femaleConnectorType),
				maleConnectorType = ConnectorType.CreateMinimalAnonymous(connectorTypeCompatibility.maleConnectorType)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ConnectorTypeCompatibility Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ConnectorTypeCompatibility connectorTypeCompatibility)
		{
			//
			// Return a very minimal object.
			//
			if (connectorTypeCompatibility == null)
			{
				return null;
			}

			return new {
				id = connectorTypeCompatibility.id,
				name = connectorTypeCompatibility.connectionStrength,
				description = string.Join(", ", new[] { connectorTypeCompatibility.connectionStrength}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
