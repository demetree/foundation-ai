using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.Community.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class PostChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)postId; }
			set { postId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class PostChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 postId { get; set; }
			public Int32 versionNumber { get; set; }
			[Required]
			public DateTime timeStamp { get; set; }
			[Required]
			public Int32 userId { get; set; }
			[Required]
			public String data { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class PostChangeHistoryOutputDTO : PostChangeHistoryDTO
		{
			public Post.PostDTO post { get; set; }
		}


		/// <summary>
		///
		/// Converts a PostChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public PostChangeHistoryDTO ToDTO()
		{
			return new PostChangeHistoryDTO
			{
				id = this.id,
				postId = this.postId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a PostChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<PostChangeHistoryDTO> ToDTOList(List<PostChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PostChangeHistoryDTO> output = new List<PostChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (PostChangeHistory postChangeHistory in data)
			{
				output.Add(postChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a PostChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the PostChangeHistoryEntity type directly.
		///
		/// </summary>
		public PostChangeHistoryOutputDTO ToOutputDTO()
		{
			return new PostChangeHistoryOutputDTO
			{
				id = this.id,
				postId = this.postId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				post = this.post?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a PostChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of PostChangeHistory objects to avoid using the PostChangeHistory entity type directly.
		///
		/// </summary>
		public static List<PostChangeHistoryOutputDTO> ToOutputDTOList(List<PostChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PostChangeHistoryOutputDTO> output = new List<PostChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (PostChangeHistory postChangeHistory in data)
			{
				output.Add(postChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a PostChangeHistory Object.
		///
		/// </summary>
		public static Database.PostChangeHistory FromDTO(PostChangeHistoryDTO dto)
		{
			return new Database.PostChangeHistory
			{
				id = dto.id,
				postId = dto.postId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a PostChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(PostChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.postId = dto.postId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a PostChangeHistory Object.
		///
		/// </summary>
		public PostChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new PostChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				postId = this.postId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PostChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PostChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a PostChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a PostChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.PostChangeHistory postChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (postChangeHistory == null)
			{
				return null;
			}

			return new {
				id = postChangeHistory.id,
				postId = postChangeHistory.postId,
				versionNumber = postChangeHistory.versionNumber,
				timeStamp = postChangeHistory.timeStamp,
				userId = postChangeHistory.userId,
				data = postChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a PostChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(PostChangeHistory postChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (postChangeHistory == null)
			{
				return null;
			}

			return new {
				id = postChangeHistory.id,
				postId = postChangeHistory.postId,
				versionNumber = postChangeHistory.versionNumber,
				timeStamp = postChangeHistory.timeStamp,
				userId = postChangeHistory.userId,
				data = postChangeHistory.data,
				post = Post.CreateMinimalAnonymous(postChangeHistory.post)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a PostChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(PostChangeHistory postChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (postChangeHistory == null)
			{
				return null;
			}

			return new {
				id = postChangeHistory.id,
				name = postChangeHistory.id,
				description = postChangeHistory.id
			 };
		}
	}
}
