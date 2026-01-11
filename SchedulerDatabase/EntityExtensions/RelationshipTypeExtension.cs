using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Scheduler.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class RelationshipType : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class RelationshipTypeDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
			[Required]
			public Boolean isEmergencyEligible { get; set; }
			public Int32? sequence { get; set; }
			public Int32? iconId { get; set; }
			public String color { get; set; }
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
		public class RelationshipTypeOutputDTO : RelationshipTypeDTO
		{
			public Icon.IconDTO icon { get; set; }
		}


		/// <summary>
		///
		/// Converts a RelationshipType to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public RelationshipTypeDTO ToDTO()
		{
			return new RelationshipTypeDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				isEmergencyEligible = this.isEmergencyEligible,
				sequence = this.sequence,
				iconId = this.iconId,
				color = this.color,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a RelationshipType list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<RelationshipTypeDTO> ToDTOList(List<RelationshipType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<RelationshipTypeDTO> output = new List<RelationshipTypeDTO>();

			output.Capacity = data.Count;

			foreach (RelationshipType relationshipType in data)
			{
				output.Add(relationshipType.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a RelationshipType to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the RelationshipTypeEntity type directly.
		///
		/// </summary>
		public RelationshipTypeOutputDTO ToOutputDTO()
		{
			return new RelationshipTypeOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				isEmergencyEligible = this.isEmergencyEligible,
				sequence = this.sequence,
				iconId = this.iconId,
				color = this.color,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				icon = this.icon?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a RelationshipType list to list of Output Data Transfer Object intended to be used for serializing a list of RelationshipType objects to avoid using the RelationshipType entity type directly.
		///
		/// </summary>
		public static List<RelationshipTypeOutputDTO> ToOutputDTOList(List<RelationshipType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<RelationshipTypeOutputDTO> output = new List<RelationshipTypeOutputDTO>();

			output.Capacity = data.Count;

			foreach (RelationshipType relationshipType in data)
			{
				output.Add(relationshipType.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a RelationshipType Object.
		///
		/// </summary>
		public static Database.RelationshipType FromDTO(RelationshipTypeDTO dto)
		{
			return new Database.RelationshipType
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				isEmergencyEligible = dto.isEmergencyEligible,
				sequence = dto.sequence,
				iconId = dto.iconId,
				color = dto.color,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a RelationshipType Object.
		///
		/// </summary>
		public void ApplyDTO(RelationshipTypeDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.isEmergencyEligible = dto.isEmergencyEligible;
			this.sequence = dto.sequence;
			this.iconId = dto.iconId;
			this.color = dto.color;
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
		/// Creates a deep copy clone of a RelationshipType Object.
		///
		/// </summary>
		public RelationshipType Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new RelationshipType{
				id = this.id,
				name = this.name,
				description = this.description,
				isEmergencyEligible = this.isEmergencyEligible,
				sequence = this.sequence,
				iconId = this.iconId,
				color = this.color,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a RelationshipType Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a RelationshipType Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a RelationshipType Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a RelationshipType Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.RelationshipType relationshipType)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (relationshipType == null)
			{
				return null;
			}

			return new {
				id = relationshipType.id,
				name = relationshipType.name,
				description = relationshipType.description,
				isEmergencyEligible = relationshipType.isEmergencyEligible,
				sequence = relationshipType.sequence,
				iconId = relationshipType.iconId,
				color = relationshipType.color,
				objectGuid = relationshipType.objectGuid,
				active = relationshipType.active,
				deleted = relationshipType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a RelationshipType Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(RelationshipType relationshipType)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (relationshipType == null)
			{
				return null;
			}

			return new {
				id = relationshipType.id,
				name = relationshipType.name,
				description = relationshipType.description,
				isEmergencyEligible = relationshipType.isEmergencyEligible,
				sequence = relationshipType.sequence,
				iconId = relationshipType.iconId,
				color = relationshipType.color,
				objectGuid = relationshipType.objectGuid,
				active = relationshipType.active,
				deleted = relationshipType.deleted,
				icon = Icon.CreateMinimalAnonymous(relationshipType.icon)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a RelationshipType Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(RelationshipType relationshipType)
		{
			//
			// Return a very minimal object.
			//
			if (relationshipType == null)
			{
				return null;
			}

			return new {
				id = relationshipType.id,
				name = relationshipType.name,
				description = relationshipType.description,
			 };
		}
	}
}
