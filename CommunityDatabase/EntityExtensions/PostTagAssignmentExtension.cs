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
	public partial class PostTagAssignment : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class PostTagAssignmentDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 postId { get; set; }
			[Required]
			public Int32 postTagId { get; set; }
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
		public class PostTagAssignmentOutputDTO : PostTagAssignmentDTO
		{
			public Post.PostDTO post { get; set; }
			public PostTag.PostTagDTO postTag { get; set; }
		}


		/// <summary>
		///
		/// Converts a PostTagAssignment to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public PostTagAssignmentDTO ToDTO()
		{
			return new PostTagAssignmentDTO
			{
				id = this.id,
				postId = this.postId,
				postTagId = this.postTagId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a PostTagAssignment list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<PostTagAssignmentDTO> ToDTOList(List<PostTagAssignment> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PostTagAssignmentDTO> output = new List<PostTagAssignmentDTO>();

			output.Capacity = data.Count;

			foreach (PostTagAssignment postTagAssignment in data)
			{
				output.Add(postTagAssignment.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a PostTagAssignment to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the PostTagAssignmentEntity type directly.
		///
		/// </summary>
		public PostTagAssignmentOutputDTO ToOutputDTO()
		{
			return new PostTagAssignmentOutputDTO
			{
				id = this.id,
				postId = this.postId,
				postTagId = this.postTagId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				post = this.post?.ToDTO(),
				postTag = this.postTag?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a PostTagAssignment list to list of Output Data Transfer Object intended to be used for serializing a list of PostTagAssignment objects to avoid using the PostTagAssignment entity type directly.
		///
		/// </summary>
		public static List<PostTagAssignmentOutputDTO> ToOutputDTOList(List<PostTagAssignment> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PostTagAssignmentOutputDTO> output = new List<PostTagAssignmentOutputDTO>();

			output.Capacity = data.Count;

			foreach (PostTagAssignment postTagAssignment in data)
			{
				output.Add(postTagAssignment.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a PostTagAssignment Object.
		///
		/// </summary>
		public static Database.PostTagAssignment FromDTO(PostTagAssignmentDTO dto)
		{
			return new Database.PostTagAssignment
			{
				id = dto.id,
				postId = dto.postId,
				postTagId = dto.postTagId,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a PostTagAssignment Object.
		///
		/// </summary>
		public void ApplyDTO(PostTagAssignmentDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.postId = dto.postId;
			this.postTagId = dto.postTagId;
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
		/// Creates a deep copy clone of a PostTagAssignment Object.
		///
		/// </summary>
		public PostTagAssignment Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new PostTagAssignment{
				id = this.id,
				postId = this.postId,
				postTagId = this.postTagId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PostTagAssignment Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PostTagAssignment Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a PostTagAssignment Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a PostTagAssignment Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.PostTagAssignment postTagAssignment)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (postTagAssignment == null)
			{
				return null;
			}

			return new {
				id = postTagAssignment.id,
				postId = postTagAssignment.postId,
				postTagId = postTagAssignment.postTagId,
				objectGuid = postTagAssignment.objectGuid,
				active = postTagAssignment.active,
				deleted = postTagAssignment.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a PostTagAssignment Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(PostTagAssignment postTagAssignment)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (postTagAssignment == null)
			{
				return null;
			}

			return new {
				id = postTagAssignment.id,
				postId = postTagAssignment.postId,
				postTagId = postTagAssignment.postTagId,
				objectGuid = postTagAssignment.objectGuid,
				active = postTagAssignment.active,
				deleted = postTagAssignment.deleted,
				post = Post.CreateMinimalAnonymous(postTagAssignment.post),
				postTag = PostTag.CreateMinimalAnonymous(postTagAssignment.postTag)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a PostTagAssignment Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(PostTagAssignment postTagAssignment)
		{
			//
			// Return a very minimal object.
			//
			if (postTagAssignment == null)
			{
				return null;
			}

			return new {
				id = postTagAssignment.id,
				name = postTagAssignment.id,
				description = postTagAssignment.id
			 };
		}
	}
}
