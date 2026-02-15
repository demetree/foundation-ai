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
	public partial class SubmodelPlacedBrick : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SubmodelPlacedBrickDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 submodelId { get; set; }
			[Required]
			public Int32 placedBrickId { get; set; }
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
		public class SubmodelPlacedBrickOutputDTO : SubmodelPlacedBrickDTO
		{
			public PlacedBrick.PlacedBrickDTO placedBrick { get; set; }
			public Submodel.SubmodelDTO submodel { get; set; }
		}


		/// <summary>
		///
		/// Converts a SubmodelPlacedBrick to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SubmodelPlacedBrickDTO ToDTO()
		{
			return new SubmodelPlacedBrickDTO
			{
				id = this.id,
				submodelId = this.submodelId,
				placedBrickId = this.placedBrickId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SubmodelPlacedBrick list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SubmodelPlacedBrickDTO> ToDTOList(List<SubmodelPlacedBrick> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SubmodelPlacedBrickDTO> output = new List<SubmodelPlacedBrickDTO>();

			output.Capacity = data.Count;

			foreach (SubmodelPlacedBrick submodelPlacedBrick in data)
			{
				output.Add(submodelPlacedBrick.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SubmodelPlacedBrick to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SubmodelPlacedBrickEntity type directly.
		///
		/// </summary>
		public SubmodelPlacedBrickOutputDTO ToOutputDTO()
		{
			return new SubmodelPlacedBrickOutputDTO
			{
				id = this.id,
				submodelId = this.submodelId,
				placedBrickId = this.placedBrickId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				placedBrick = this.placedBrick?.ToDTO(),
				submodel = this.submodel?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a SubmodelPlacedBrick list to list of Output Data Transfer Object intended to be used for serializing a list of SubmodelPlacedBrick objects to avoid using the SubmodelPlacedBrick entity type directly.
		///
		/// </summary>
		public static List<SubmodelPlacedBrickOutputDTO> ToOutputDTOList(List<SubmodelPlacedBrick> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SubmodelPlacedBrickOutputDTO> output = new List<SubmodelPlacedBrickOutputDTO>();

			output.Capacity = data.Count;

			foreach (SubmodelPlacedBrick submodelPlacedBrick in data)
			{
				output.Add(submodelPlacedBrick.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SubmodelPlacedBrick Object.
		///
		/// </summary>
		public static Database.SubmodelPlacedBrick FromDTO(SubmodelPlacedBrickDTO dto)
		{
			return new Database.SubmodelPlacedBrick
			{
				id = dto.id,
				submodelId = dto.submodelId,
				placedBrickId = dto.placedBrickId,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SubmodelPlacedBrick Object.
		///
		/// </summary>
		public void ApplyDTO(SubmodelPlacedBrickDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.submodelId = dto.submodelId;
			this.placedBrickId = dto.placedBrickId;
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
		/// Creates a deep copy clone of a SubmodelPlacedBrick Object.
		///
		/// </summary>
		public SubmodelPlacedBrick Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SubmodelPlacedBrick{
				id = this.id,
				tenantGuid = this.tenantGuid,
				submodelId = this.submodelId,
				placedBrickId = this.placedBrickId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SubmodelPlacedBrick Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SubmodelPlacedBrick Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SubmodelPlacedBrick Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SubmodelPlacedBrick Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SubmodelPlacedBrick submodelPlacedBrick)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (submodelPlacedBrick == null)
			{
				return null;
			}

			return new {
				id = submodelPlacedBrick.id,
				submodelId = submodelPlacedBrick.submodelId,
				placedBrickId = submodelPlacedBrick.placedBrickId,
				objectGuid = submodelPlacedBrick.objectGuid,
				active = submodelPlacedBrick.active,
				deleted = submodelPlacedBrick.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SubmodelPlacedBrick Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SubmodelPlacedBrick submodelPlacedBrick)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (submodelPlacedBrick == null)
			{
				return null;
			}

			return new {
				id = submodelPlacedBrick.id,
				submodelId = submodelPlacedBrick.submodelId,
				placedBrickId = submodelPlacedBrick.placedBrickId,
				objectGuid = submodelPlacedBrick.objectGuid,
				active = submodelPlacedBrick.active,
				deleted = submodelPlacedBrick.deleted,
				placedBrick = PlacedBrick.CreateMinimalAnonymous(submodelPlacedBrick.placedBrick),
				submodel = Submodel.CreateMinimalAnonymous(submodelPlacedBrick.submodel)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SubmodelPlacedBrick Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SubmodelPlacedBrick submodelPlacedBrick)
		{
			//
			// Return a very minimal object.
			//
			if (submodelPlacedBrick == null)
			{
				return null;
			}

			return new {
				id = submodelPlacedBrick.id,
				name = submodelPlacedBrick.id,
				description = submodelPlacedBrick.id
			 };
		}
	}
}
