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
			public Int32 Id { get; set; }
			public String Name { get; set; }
			public String Slug { get; set; }
			public Guid ObjectGuid { get; set; }
			public Boolean? Active { get; set; }
			public Boolean? Deleted { get; set; }
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
				Id = this.Id,
				Name = this.Name,
				Slug = this.Slug,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted
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
				Id = this.Id,
				Name = this.Name,
				Slug = this.Slug,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted
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
				Id = dto.Id,
				Name = dto.Name,
				Slug = dto.Slug,
				ObjectGuid = dto.ObjectGuid,
				Active = dto.Active ?? true,
				Deleted = dto.Deleted ?? false
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

			this.Name = dto.Name;
			this.Slug = dto.Slug;
			this.ObjectGuid = dto.ObjectGuid;
			if (dto.Active.HasValue == true)
			{
				this.Active = dto.Active.Value;
			}
			if (dto.Deleted.HasValue == true)
			{
				this.Deleted = dto.Deleted.Value;
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
				Id = this.Id,
				Name = this.Name,
				Slug = this.Slug,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted,
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
				Id = postTag.Id,
				Name = postTag.Name,
				Slug = postTag.Slug,
				ObjectGuid = postTag.ObjectGuid,
				Active = postTag.Active,
				Deleted = postTag.Deleted,
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
				Id = postTag.Id,
				Name = postTag.Name,
				Slug = postTag.Slug,
				ObjectGuid = postTag.ObjectGuid,
				Active = postTag.Active,
				Deleted = postTag.Deleted,
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
				name = postTag.name,
				description = string.Join(", ", new[] { postTag.name, postTag.slug}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
