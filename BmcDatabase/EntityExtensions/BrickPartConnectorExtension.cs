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
	public partial class BrickPartConnector : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BrickPartConnectorDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 brickPartId { get; set; }
			[Required]
			public Int32 connectorTypeId { get; set; }
			public Single? positionX { get; set; }
			public Single? positionY { get; set; }
			public Single? positionZ { get; set; }
			public Single? orientationX { get; set; }
			public Single? orientationY { get; set; }
			public Single? orientationZ { get; set; }
			public Int32? connectorGroupId { get; set; }
			[Required]
			public Boolean isAutoExtracted { get; set; }
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
		public class BrickPartConnectorOutputDTO : BrickPartConnectorDTO
		{
			public BrickPart.BrickPartDTO brickPart { get; set; }
			public ConnectorType.ConnectorTypeDTO connectorType { get; set; }
		}


		/// <summary>
		///
		/// Converts a BrickPartConnector to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BrickPartConnectorDTO ToDTO()
		{
			return new BrickPartConnectorDTO
			{
				id = this.id,
				brickPartId = this.brickPartId,
				connectorTypeId = this.connectorTypeId,
				positionX = this.positionX,
				positionY = this.positionY,
				positionZ = this.positionZ,
				orientationX = this.orientationX,
				orientationY = this.orientationY,
				orientationZ = this.orientationZ,
				connectorGroupId = this.connectorGroupId,
				isAutoExtracted = this.isAutoExtracted,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BrickPartConnector list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BrickPartConnectorDTO> ToDTOList(List<BrickPartConnector> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickPartConnectorDTO> output = new List<BrickPartConnectorDTO>();

			output.Capacity = data.Count;

			foreach (BrickPartConnector brickPartConnector in data)
			{
				output.Add(brickPartConnector.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BrickPartConnector to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BrickPartConnectorEntity type directly.
		///
		/// </summary>
		public BrickPartConnectorOutputDTO ToOutputDTO()
		{
			return new BrickPartConnectorOutputDTO
			{
				id = this.id,
				brickPartId = this.brickPartId,
				connectorTypeId = this.connectorTypeId,
				positionX = this.positionX,
				positionY = this.positionY,
				positionZ = this.positionZ,
				orientationX = this.orientationX,
				orientationY = this.orientationY,
				orientationZ = this.orientationZ,
				connectorGroupId = this.connectorGroupId,
				isAutoExtracted = this.isAutoExtracted,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				brickPart = this.brickPart?.ToDTO(),
				connectorType = this.connectorType?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a BrickPartConnector list to list of Output Data Transfer Object intended to be used for serializing a list of BrickPartConnector objects to avoid using the BrickPartConnector entity type directly.
		///
		/// </summary>
		public static List<BrickPartConnectorOutputDTO> ToOutputDTOList(List<BrickPartConnector> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickPartConnectorOutputDTO> output = new List<BrickPartConnectorOutputDTO>();

			output.Capacity = data.Count;

			foreach (BrickPartConnector brickPartConnector in data)
			{
				output.Add(brickPartConnector.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BrickPartConnector Object.
		///
		/// </summary>
		public static Database.BrickPartConnector FromDTO(BrickPartConnectorDTO dto)
		{
			return new Database.BrickPartConnector
			{
				id = dto.id,
				brickPartId = dto.brickPartId,
				connectorTypeId = dto.connectorTypeId,
				positionX = dto.positionX,
				positionY = dto.positionY,
				positionZ = dto.positionZ,
				orientationX = dto.orientationX,
				orientationY = dto.orientationY,
				orientationZ = dto.orientationZ,
				connectorGroupId = dto.connectorGroupId,
				isAutoExtracted = dto.isAutoExtracted,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BrickPartConnector Object.
		///
		/// </summary>
		public void ApplyDTO(BrickPartConnectorDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.brickPartId = dto.brickPartId;
			this.connectorTypeId = dto.connectorTypeId;
			this.positionX = dto.positionX;
			this.positionY = dto.positionY;
			this.positionZ = dto.positionZ;
			this.orientationX = dto.orientationX;
			this.orientationY = dto.orientationY;
			this.orientationZ = dto.orientationZ;
			this.connectorGroupId = dto.connectorGroupId;
			this.isAutoExtracted = dto.isAutoExtracted;
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
		/// Creates a deep copy clone of a BrickPartConnector Object.
		///
		/// </summary>
		public BrickPartConnector Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BrickPartConnector{
				id = this.id,
				brickPartId = this.brickPartId,
				connectorTypeId = this.connectorTypeId,
				positionX = this.positionX,
				positionY = this.positionY,
				positionZ = this.positionZ,
				orientationX = this.orientationX,
				orientationY = this.orientationY,
				orientationZ = this.orientationZ,
				connectorGroupId = this.connectorGroupId,
				isAutoExtracted = this.isAutoExtracted,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickPartConnector Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickPartConnector Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BrickPartConnector Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BrickPartConnector Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BrickPartConnector brickPartConnector)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (brickPartConnector == null)
			{
				return null;
			}

			return new {
				id = brickPartConnector.id,
				brickPartId = brickPartConnector.brickPartId,
				connectorTypeId = brickPartConnector.connectorTypeId,
				positionX = brickPartConnector.positionX,
				positionY = brickPartConnector.positionY,
				positionZ = brickPartConnector.positionZ,
				orientationX = brickPartConnector.orientationX,
				orientationY = brickPartConnector.orientationY,
				orientationZ = brickPartConnector.orientationZ,
				connectorGroupId = brickPartConnector.connectorGroupId,
				isAutoExtracted = brickPartConnector.isAutoExtracted,
				sequence = brickPartConnector.sequence,
				objectGuid = brickPartConnector.objectGuid,
				active = brickPartConnector.active,
				deleted = brickPartConnector.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BrickPartConnector Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BrickPartConnector brickPartConnector)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (brickPartConnector == null)
			{
				return null;
			}

			return new {
				id = brickPartConnector.id,
				brickPartId = brickPartConnector.brickPartId,
				connectorTypeId = brickPartConnector.connectorTypeId,
				positionX = brickPartConnector.positionX,
				positionY = brickPartConnector.positionY,
				positionZ = brickPartConnector.positionZ,
				orientationX = brickPartConnector.orientationX,
				orientationY = brickPartConnector.orientationY,
				orientationZ = brickPartConnector.orientationZ,
				connectorGroupId = brickPartConnector.connectorGroupId,
				isAutoExtracted = brickPartConnector.isAutoExtracted,
				sequence = brickPartConnector.sequence,
				objectGuid = brickPartConnector.objectGuid,
				active = brickPartConnector.active,
				deleted = brickPartConnector.deleted,
				brickPart = BrickPart.CreateMinimalAnonymous(brickPartConnector.brickPart),
				connectorType = ConnectorType.CreateMinimalAnonymous(brickPartConnector.connectorType)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BrickPartConnector Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BrickPartConnector brickPartConnector)
		{
			//
			// Return a very minimal object.
			//
			if (brickPartConnector == null)
			{
				return null;
			}

			return new {
				id = brickPartConnector.id,
				name = brickPartConnector.id,
				description = brickPartConnector.id
			 };
		}
	}
}
