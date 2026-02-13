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
	public partial class BrickConnection : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BrickConnectionDTO
		{
			public Int32 id { get; set; }
			public Int32? projectId { get; set; }
			public Int64? sourcePlacedBrickId { get; set; }
			public Int64? sourceConnectorId { get; set; }
			public Int64? targetPlacedBrickId { get; set; }
			public Int64? targetConnectorId { get; set; }
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
		public class BrickConnectionOutputDTO : BrickConnectionDTO
		{
			public Project.ProjectDTO project { get; set; }
		}


		/// <summary>
		///
		/// Converts a BrickConnection to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BrickConnectionDTO ToDTO()
		{
			return new BrickConnectionDTO
			{
				id = this.id,
				projectId = this.projectId,
				sourcePlacedBrickId = this.sourcePlacedBrickId,
				sourceConnectorId = this.sourceConnectorId,
				targetPlacedBrickId = this.targetPlacedBrickId,
				targetConnectorId = this.targetConnectorId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BrickConnection list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BrickConnectionDTO> ToDTOList(List<BrickConnection> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickConnectionDTO> output = new List<BrickConnectionDTO>();

			output.Capacity = data.Count;

			foreach (BrickConnection brickConnection in data)
			{
				output.Add(brickConnection.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BrickConnection to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BrickConnectionEntity type directly.
		///
		/// </summary>
		public BrickConnectionOutputDTO ToOutputDTO()
		{
			return new BrickConnectionOutputDTO
			{
				id = this.id,
				projectId = this.projectId,
				sourcePlacedBrickId = this.sourcePlacedBrickId,
				sourceConnectorId = this.sourceConnectorId,
				targetPlacedBrickId = this.targetPlacedBrickId,
				targetConnectorId = this.targetConnectorId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				project = this.project?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a BrickConnection list to list of Output Data Transfer Object intended to be used for serializing a list of BrickConnection objects to avoid using the BrickConnection entity type directly.
		///
		/// </summary>
		public static List<BrickConnectionOutputDTO> ToOutputDTOList(List<BrickConnection> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickConnectionOutputDTO> output = new List<BrickConnectionOutputDTO>();

			output.Capacity = data.Count;

			foreach (BrickConnection brickConnection in data)
			{
				output.Add(brickConnection.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BrickConnection Object.
		///
		/// </summary>
		public static Database.BrickConnection FromDTO(BrickConnectionDTO dto)
		{
			return new Database.BrickConnection
			{
				id = dto.id,
				projectId = dto.projectId,
				sourcePlacedBrickId = dto.sourcePlacedBrickId,
				sourceConnectorId = dto.sourceConnectorId,
				targetPlacedBrickId = dto.targetPlacedBrickId,
				targetConnectorId = dto.targetConnectorId,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BrickConnection Object.
		///
		/// </summary>
		public void ApplyDTO(BrickConnectionDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.projectId = dto.projectId;
			this.sourcePlacedBrickId = dto.sourcePlacedBrickId;
			this.sourceConnectorId = dto.sourceConnectorId;
			this.targetPlacedBrickId = dto.targetPlacedBrickId;
			this.targetConnectorId = dto.targetConnectorId;
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
		/// Creates a deep copy clone of a BrickConnection Object.
		///
		/// </summary>
		public BrickConnection Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BrickConnection{
				id = this.id,
				tenantGuid = this.tenantGuid,
				projectId = this.projectId,
				sourcePlacedBrickId = this.sourcePlacedBrickId,
				sourceConnectorId = this.sourceConnectorId,
				targetPlacedBrickId = this.targetPlacedBrickId,
				targetConnectorId = this.targetConnectorId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickConnection Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickConnection Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BrickConnection Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BrickConnection Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BrickConnection brickConnection)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (brickConnection == null)
			{
				return null;
			}

			return new {
				id = brickConnection.id,
				projectId = brickConnection.projectId,
				sourcePlacedBrickId = brickConnection.sourcePlacedBrickId,
				sourceConnectorId = brickConnection.sourceConnectorId,
				targetPlacedBrickId = brickConnection.targetPlacedBrickId,
				targetConnectorId = brickConnection.targetConnectorId,
				objectGuid = brickConnection.objectGuid,
				active = brickConnection.active,
				deleted = brickConnection.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BrickConnection Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BrickConnection brickConnection)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (brickConnection == null)
			{
				return null;
			}

			return new {
				id = brickConnection.id,
				projectId = brickConnection.projectId,
				sourcePlacedBrickId = brickConnection.sourcePlacedBrickId,
				sourceConnectorId = brickConnection.sourceConnectorId,
				targetPlacedBrickId = brickConnection.targetPlacedBrickId,
				targetConnectorId = brickConnection.targetConnectorId,
				objectGuid = brickConnection.objectGuid,
				active = brickConnection.active,
				deleted = brickConnection.deleted,
				project = Project.CreateMinimalAnonymous(brickConnection.project)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BrickConnection Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BrickConnection brickConnection)
		{
			//
			// Return a very minimal object.
			//
			if (brickConnection == null)
			{
				return null;
			}

			return new {
				id = brickConnection.id,
				name = brickConnection.id,
				description = brickConnection.id
			 };
		}
	}
}
