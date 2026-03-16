using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Community.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class PostTag : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class PostTagDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String slug { get; set; }
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
		public class PostTagOutputDTO : PostTagDTO
		{
		}


		/// <summary>
		///
		/// Converts a PostTag to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public PostTagDTO ToDTO()
		{
			return new PostTagDTO
			{
				id = this.id,
				name = this.name,
				slug = this.slug,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a PostTag list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<PostTagDTO> ToDTOList(List<PostTag> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PostTagDTO> output = new List<PostTagDTO>();

			output.Capacity = data.Count;

			foreach (PostTag postTag in data)
			{
				output.Add(postTag.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a PostTag to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the PostTagEntity type directly.
		///
		/// </summary>
		public PostTagOutputDTO ToOutputDTO()
		{
			return new PostTagOutputDTO
			{
				id = this.id,
				name = this.name,
				slug = this.slug,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a PostTag list to list of Output Data Transfer Object intended to be used for serializing a list of PostTag objects to avoid using the PostTag entity type directly.
		///
		/// </summary>
		public static List<PostTagOutputDTO> ToOutputDTOList(List<PostTag> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PostTagOutputDTO> output = new List<PostTagOutputDTO>();

			output.Capacity = data.Count;

			foreach (PostTag postTag in data)
			{
				output.Add(postTag.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a PostTag Object.
		///
		/// </summary>
		public static Database.PostTag FromDTO(PostTagDTO dto)
		{
			return new Database.PostTag
			{
				id = dto.id,
				name = dto.name,
				slug = dto.slug,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a PostTag Object.
		///
		/// </summary>
		public void ApplyDTO(PostTagDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.slug = dto.slug;
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
		/// Creates a deep copy clone of a PostTag Object.
		///
		/// </summary>
		public PostTag Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new PostTag{
				id = this.id,
				tenantGuid = this.tenantGuid,
				name = this.name,
				slug = this.slug,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PostTag Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PostTag Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a PostTag Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a PostTag Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.PostTag postTag)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (postTag == null)
			{
				return null;
			}

			return new {
				id = postTag.id,
				name = postTag.name,
				slug = postTag.slug,
				objectGuid = postTag.objectGuid,
				active = postTag.active,
				deleted = postTag.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a PostTag Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(PostTag postTag)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (postTag == null)
			{
				return null;
			}

			return new {
				id = postTag.id,
				name = postTag.name,
				slug = postTag.slug,
				objectGuid = postTag.objectGuid,
				active = postTag.active,
				deleted = postTag.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a PostTag Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(PostTag postTag)
		{
			//
			// Return a very minimal object.
			//
			if (postTag == null)
			{
				return null;
			}

			return new {
				id = postTag.id,
				name = postTag.name,
				description = string.Join(", ", new[] { postTag.name, postTag.slug}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
