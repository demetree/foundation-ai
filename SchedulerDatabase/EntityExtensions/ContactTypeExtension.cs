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
	public partial class ContactType : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ContactTypeDTO
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
		public class ContactTypeOutputDTO : ContactTypeDTO
		{
			public Icon.IconDTO icon { get; set; }
		}


		/// <summary>
		///
		/// Converts a ContactType to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ContactTypeDTO ToDTO()
		{
			return new ContactTypeDTO
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
		/// Converts a ContactType list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ContactTypeDTO> ToDTOList(List<ContactType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ContactTypeDTO> output = new List<ContactTypeDTO>();

			output.Capacity = data.Count;

			foreach (ContactType contactType in data)
			{
				output.Add(contactType.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ContactType to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ContactType Entity type directly.
		///
		/// </summary>
		public ContactTypeOutputDTO ToOutputDTO()
		{
			return new ContactTypeOutputDTO
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
		/// Converts a ContactType list to list of Output Data Transfer Object intended to be used for serializing a list of ContactType objects to avoid using the ContactType entity type directly.
		///
		/// </summary>
		public static List<ContactTypeOutputDTO> ToOutputDTOList(List<ContactType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ContactTypeOutputDTO> output = new List<ContactTypeOutputDTO>();

			output.Capacity = data.Count;

			foreach (ContactType contactType in data)
			{
				output.Add(contactType.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ContactType Object.
		///
		/// </summary>
		public static Database.ContactType FromDTO(ContactTypeDTO dto)
		{
			return new Database.ContactType
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
		/// Applies the values from an INPUT DTO to a ContactType Object.
		///
		/// </summary>
		public void ApplyDTO(ContactTypeDTO dto)
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
		/// Creates a deep copy clone of a ContactType Object.
		///
		/// </summary>
		public ContactType Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ContactType{
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
        /// Creates an anonymous object containing properties from a ContactType Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ContactType Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ContactType Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ContactType Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ContactType contactType)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (contactType == null)
			{
				return null;
			}

			return new {
				id = contactType.id,
				name = contactType.name,
				description = contactType.description,
				sequence = contactType.sequence,
				iconId = contactType.iconId,
				color = contactType.color,
				objectGuid = contactType.objectGuid,
				active = contactType.active,
				deleted = contactType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ContactType Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ContactType contactType)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (contactType == null)
			{
				return null;
			}

			return new {
				id = contactType.id,
				name = contactType.name,
				description = contactType.description,
				sequence = contactType.sequence,
				iconId = contactType.iconId,
				color = contactType.color,
				objectGuid = contactType.objectGuid,
				active = contactType.active,
				deleted = contactType.deleted,
				icon = Icon.CreateMinimalAnonymous(contactType.icon)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ContactType Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ContactType contactType)
		{
			//
			// Return a very minimal object.
			//
			if (contactType == null)
			{
				return null;
			}

			return new {
				id = contactType.id,
				name = contactType.name,
				description = contactType.description,
			 };
		}
	}
}
