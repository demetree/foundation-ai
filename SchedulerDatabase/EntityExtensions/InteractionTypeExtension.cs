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
	public partial class InteractionType : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class InteractionTypeDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
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
		public class InteractionTypeOutputDTO : InteractionTypeDTO
		{
			public Icon.IconDTO icon { get; set; }
		}


		/// <summary>
		///
		/// Converts a InteractionType to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public InteractionTypeDTO ToDTO()
		{
			return new InteractionTypeDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
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
		/// Converts a InteractionType list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<InteractionTypeDTO> ToDTOList(List<InteractionType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<InteractionTypeDTO> output = new List<InteractionTypeDTO>();

			output.Capacity = data.Count;

			foreach (InteractionType interactionType in data)
			{
				output.Add(interactionType.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a InteractionType to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the InteractionType Entity type directly.
		///
		/// </summary>
		public InteractionTypeOutputDTO ToOutputDTO()
		{
			return new InteractionTypeOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
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
		/// Converts a InteractionType list to list of Output Data Transfer Object intended to be used for serializing a list of InteractionType objects to avoid using the InteractionType entity type directly.
		///
		/// </summary>
		public static List<InteractionTypeOutputDTO> ToOutputDTOList(List<InteractionType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<InteractionTypeOutputDTO> output = new List<InteractionTypeOutputDTO>();

			output.Capacity = data.Count;

			foreach (InteractionType interactionType in data)
			{
				output.Add(interactionType.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a InteractionType Object.
		///
		/// </summary>
		public static Database.InteractionType FromDTO(InteractionTypeDTO dto)
		{
			return new Database.InteractionType
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
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
		/// Applies the values from an INPUT DTO to a InteractionType Object.
		///
		/// </summary>
		public void ApplyDTO(InteractionTypeDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
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
		/// Creates a deep copy clone of a InteractionType Object.
		///
		/// </summary>
		public InteractionType Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new InteractionType{
				id = this.id,
				name = this.name,
				description = this.description,
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
        /// Creates an anonymous object containing properties from a InteractionType Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a InteractionType Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a InteractionType Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a InteractionType Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.InteractionType interactionType)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (interactionType == null)
			{
				return null;
			}

			return new {
				id = interactionType.id,
				name = interactionType.name,
				description = interactionType.description,
				sequence = interactionType.sequence,
				iconId = interactionType.iconId,
				color = interactionType.color,
				objectGuid = interactionType.objectGuid,
				active = interactionType.active,
				deleted = interactionType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a InteractionType Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(InteractionType interactionType)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (interactionType == null)
			{
				return null;
			}

			return new {
				id = interactionType.id,
				name = interactionType.name,
				description = interactionType.description,
				sequence = interactionType.sequence,
				iconId = interactionType.iconId,
				color = interactionType.color,
				objectGuid = interactionType.objectGuid,
				active = interactionType.active,
				deleted = interactionType.deleted,
				icon = Icon.CreateMinimalAnonymous(interactionType.icon)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a InteractionType Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(InteractionType interactionType)
		{
			//
			// Return a very minimal object.
			//
			if (interactionType == null)
			{
				return null;
			}

			return new {
				id = interactionType.id,
				name = interactionType.name,
				description = interactionType.description,
			 };
		}
	}
}
