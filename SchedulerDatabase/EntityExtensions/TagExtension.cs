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
	public partial class Tag : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class TagDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
			public Int32? sequence { get; set; }
			public Boolean? isSystem { get; set; }
			public Int32? priorityId { get; set; }
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
		public class TagOutputDTO : TagDTO
		{
			public Icon.IconDTO icon { get; set; }
			public Priority.PriorityDTO priority { get; set; }
		}


		/// <summary>
		///
		/// Converts a Tag to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public TagDTO ToDTO()
		{
			return new TagDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				sequence = this.sequence,
				isSystem = this.isSystem,
				priorityId = this.priorityId,
				iconId = this.iconId,
				color = this.color,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a Tag list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<TagDTO> ToDTOList(List<Tag> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TagDTO> output = new List<TagDTO>();

			output.Capacity = data.Count;

			foreach (Tag tag in data)
			{
				output.Add(tag.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a Tag to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the TagEntity type directly.
		///
		/// </summary>
		public TagOutputDTO ToOutputDTO()
		{
			return new TagOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				sequence = this.sequence,
				isSystem = this.isSystem,
				priorityId = this.priorityId,
				iconId = this.iconId,
				color = this.color,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				icon = this.icon?.ToDTO(),
				priority = this.priority?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a Tag list to list of Output Data Transfer Object intended to be used for serializing a list of Tag objects to avoid using the Tag entity type directly.
		///
		/// </summary>
		public static List<TagOutputDTO> ToOutputDTOList(List<Tag> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TagOutputDTO> output = new List<TagOutputDTO>();

			output.Capacity = data.Count;

			foreach (Tag tag in data)
			{
				output.Add(tag.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a Tag Object.
		///
		/// </summary>
		public static Database.Tag FromDTO(TagDTO dto)
		{
			return new Database.Tag
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				sequence = dto.sequence,
				isSystem = dto.isSystem,
				priorityId = dto.priorityId,
				iconId = dto.iconId,
				color = dto.color,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a Tag Object.
		///
		/// </summary>
		public void ApplyDTO(TagDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.sequence = dto.sequence;
			this.isSystem = dto.isSystem;
			this.priorityId = dto.priorityId;
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
		/// Creates a deep copy clone of a Tag Object.
		///
		/// </summary>
		public Tag Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new Tag{
				id = this.id,
				tenantGuid = this.tenantGuid,
				name = this.name,
				description = this.description,
				sequence = this.sequence,
				isSystem = this.isSystem,
				priorityId = this.priorityId,
				iconId = this.iconId,
				color = this.color,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Tag Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Tag Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a Tag Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a Tag Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.Tag tag)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (tag == null)
			{
				return null;
			}

			return new {
				id = tag.id,
				name = tag.name,
				description = tag.description,
				sequence = tag.sequence,
				isSystem = tag.isSystem,
				priorityId = tag.priorityId,
				iconId = tag.iconId,
				color = tag.color,
				objectGuid = tag.objectGuid,
				active = tag.active,
				deleted = tag.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a Tag Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(Tag tag)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (tag == null)
			{
				return null;
			}

			return new {
				id = tag.id,
				name = tag.name,
				description = tag.description,
				sequence = tag.sequence,
				isSystem = tag.isSystem,
				priorityId = tag.priorityId,
				iconId = tag.iconId,
				color = tag.color,
				objectGuid = tag.objectGuid,
				active = tag.active,
				deleted = tag.deleted,
				icon = Icon.CreateMinimalAnonymous(tag.icon),
				priority = Priority.CreateMinimalAnonymous(tag.priority)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a Tag Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(Tag tag)
		{
			//
			// Return a very minimal object.
			//
			if (tag == null)
			{
				return null;
			}

			return new {
				id = tag.id,
				name = tag.name,
				description = tag.description,
			 };
		}
	}
}
