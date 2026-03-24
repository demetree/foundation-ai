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
	public partial class ConversationType : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ConversationTypeDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			public String description { get; set; }
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
		public class ConversationTypeOutputDTO : ConversationTypeDTO
		{
		}


		/// <summary>
		///
		/// Converts a ConversationType to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ConversationTypeDTO ToDTO()
		{
			return new ConversationTypeDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ConversationType list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ConversationTypeDTO> ToDTOList(List<ConversationType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConversationTypeDTO> output = new List<ConversationTypeDTO>();

			output.Capacity = data.Count;

			foreach (ConversationType conversationType in data)
			{
				output.Add(conversationType.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ConversationType to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ConversationType Entity type directly.
		///
		/// </summary>
		public ConversationTypeOutputDTO ToOutputDTO()
		{
			return new ConversationTypeOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ConversationType list to list of Output Data Transfer Object intended to be used for serializing a list of ConversationType objects to avoid using the ConversationType entity type directly.
		///
		/// </summary>
		public static List<ConversationTypeOutputDTO> ToOutputDTOList(List<ConversationType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConversationTypeOutputDTO> output = new List<ConversationTypeOutputDTO>();

			output.Capacity = data.Count;

			foreach (ConversationType conversationType in data)
			{
				output.Add(conversationType.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ConversationType Object.
		///
		/// </summary>
		public static Database.ConversationType FromDTO(ConversationTypeDTO dto)
		{
			return new Database.ConversationType
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ConversationType Object.
		///
		/// </summary>
		public void ApplyDTO(ConversationTypeDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
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
		/// Creates a deep copy clone of a ConversationType Object.
		///
		/// </summary>
		public ConversationType Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ConversationType{
				id = this.id,
				name = this.name,
				description = this.description,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConversationType Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConversationType Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ConversationType Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ConversationType Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ConversationType conversationType)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (conversationType == null)
			{
				return null;
			}

			return new {
				id = conversationType.id,
				name = conversationType.name,
				description = conversationType.description,
				objectGuid = conversationType.objectGuid,
				active = conversationType.active,
				deleted = conversationType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ConversationType Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ConversationType conversationType)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (conversationType == null)
			{
				return null;
			}

			return new {
				id = conversationType.id,
				name = conversationType.name,
				description = conversationType.description,
				objectGuid = conversationType.objectGuid,
				active = conversationType.active,
				deleted = conversationType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ConversationType Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ConversationType conversationType)
		{
			//
			// Return a very minimal object.
			//
			if (conversationType == null)
			{
				return null;
			}

			return new {
				id = conversationType.id,
				name = conversationType.name,
				description = conversationType.description,
			 };
		}
	}
}
