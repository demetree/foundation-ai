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
	public partial class PostCategory : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class PostCategoryDTO
		{
			public Int32 Id { get; set; }
			public String Name { get; set; }
			public String Description { get; set; }
			public String Slug { get; set; }
			public Int32? Sequence { get; set; }
			public Guid ObjectGuid { get; set; }
			public Boolean? Active { get; set; }
			public Boolean? Deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class PostCategoryOutputDTO : PostCategoryDTO
		{
		}


		/// <summary>
		///
		/// Converts a PostCategory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public PostCategoryDTO ToDTO()
		{
			return new PostCategoryDTO
			{
				Id = this.Id,
				Name = this.Name,
				Description = this.Description,
				Slug = this.Slug,
				Sequence = this.Sequence,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted
			};
		}


		/// <summary>
		///
		/// Converts a PostCategory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<PostCategoryDTO> ToDTOList(List<PostCategory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PostCategoryDTO> output = new List<PostCategoryDTO>();

			output.Capacity = data.Count;

			foreach (PostCategory postCategory in data)
			{
				output.Add(postCategory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a PostCategory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the PostCategoryEntity type directly.
		///
		/// </summary>
		public PostCategoryOutputDTO ToOutputDTO()
		{
			return new PostCategoryOutputDTO
			{
				Id = this.Id,
				Name = this.Name,
				Description = this.Description,
				Slug = this.Slug,
				Sequence = this.Sequence,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted
			};
		}


		/// <summary>
		///
		/// Converts a PostCategory list to list of Output Data Transfer Object intended to be used for serializing a list of PostCategory objects to avoid using the PostCategory entity type directly.
		///
		/// </summary>
		public static List<PostCategoryOutputDTO> ToOutputDTOList(List<PostCategory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PostCategoryOutputDTO> output = new List<PostCategoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (PostCategory postCategory in data)
			{
				output.Add(postCategory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a PostCategory Object.
		///
		/// </summary>
		public static Database.PostCategory FromDTO(PostCategoryDTO dto)
		{
			return new Database.PostCategory
			{
				Id = dto.Id,
				Name = dto.Name,
				Description = dto.Description,
				Slug = dto.Slug,
				Sequence = dto.Sequence,
				ObjectGuid = dto.ObjectGuid,
				Active = dto.Active ?? true,
				Deleted = dto.Deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a PostCategory Object.
		///
		/// </summary>
		public void ApplyDTO(PostCategoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.Name = dto.Name;
			this.Description = dto.Description;
			this.Slug = dto.Slug;
			this.Sequence = dto.Sequence;
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
		/// Creates a deep copy clone of a PostCategory Object.
		///
		/// </summary>
		public PostCategory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new PostCategory{
				Id = this.Id,
				Name = this.Name,
				Description = this.Description,
				Slug = this.Slug,
				Sequence = this.Sequence,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PostCategory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PostCategory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a PostCategory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a PostCategory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.PostCategory postCategory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (postCategory == null)
			{
				return null;
			}

			return new {
				Id = postCategory.Id,
				Name = postCategory.Name,
				Description = postCategory.Description,
				Slug = postCategory.Slug,
				Sequence = postCategory.Sequence,
				ObjectGuid = postCategory.ObjectGuid,
				Active = postCategory.Active,
				Deleted = postCategory.Deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a PostCategory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(PostCategory postCategory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (postCategory == null)
			{
				return null;
			}

			return new {
				Id = postCategory.Id,
				Name = postCategory.Name,
				Description = postCategory.Description,
				Slug = postCategory.Slug,
				Sequence = postCategory.Sequence,
				ObjectGuid = postCategory.ObjectGuid,
				Active = postCategory.Active,
				Deleted = postCategory.Deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a PostCategory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(PostCategory postCategory)
		{
			//
			// Return a very minimal object.
			//
			if (postCategory == null)
			{
				return null;
			}

			return new {
				name = postCategory.name,
				description = string.Join(", ", new[] { postCategory.name, postCategory.description, postCategory.slug}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
