using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.BMC.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class MocComment : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class MocCommentDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 publishedMocId { get; set; }
			[Required]
			public Guid commenterTenantGuid { get; set; }
			[Required]
			public String commentText { get; set; }
			[Required]
			public DateTime postedDate { get; set; }
			public Int32? mocCommentId { get; set; }
			[Required]
			public Boolean isEdited { get; set; }
			[Required]
			public Boolean isHidden { get; set; }
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
		public class MocCommentOutputDTO : MocCommentDTO
		{
			public MocComment.MocCommentDTO mocComment { get; set; }
			public PublishedMoc.PublishedMocDTO publishedMoc { get; set; }
		}


		/// <summary>
		///
		/// Converts a MocComment to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public MocCommentDTO ToDTO()
		{
			return new MocCommentDTO
			{
				id = this.id,
				publishedMocId = this.publishedMocId,
				commenterTenantGuid = this.commenterTenantGuid,
				commentText = this.commentText,
				postedDate = this.postedDate,
				mocCommentId = this.mocCommentId,
				isEdited = this.isEdited,
				isHidden = this.isHidden,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a MocComment list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<MocCommentDTO> ToDTOList(List<MocComment> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MocCommentDTO> output = new List<MocCommentDTO>();

			output.Capacity = data.Count;

			foreach (MocComment mocComment in data)
			{
				output.Add(mocComment.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a MocComment to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the MocCommentEntity type directly.
		///
		/// </summary>
		public MocCommentOutputDTO ToOutputDTO()
		{
			return new MocCommentOutputDTO
			{
				id = this.id,
				publishedMocId = this.publishedMocId,
				commenterTenantGuid = this.commenterTenantGuid,
				commentText = this.commentText,
				postedDate = this.postedDate,
				mocCommentId = this.mocCommentId,
				isEdited = this.isEdited,
				isHidden = this.isHidden,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				mocComment = this.mocComment?.ToDTO(),
				publishedMoc = this.publishedMoc?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a MocComment list to list of Output Data Transfer Object intended to be used for serializing a list of MocComment objects to avoid using the MocComment entity type directly.
		///
		/// </summary>
		public static List<MocCommentOutputDTO> ToOutputDTOList(List<MocComment> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MocCommentOutputDTO> output = new List<MocCommentOutputDTO>();

			output.Capacity = data.Count;

			foreach (MocComment mocComment in data)
			{
				output.Add(mocComment.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a MocComment Object.
		///
		/// </summary>
		public static Database.MocComment FromDTO(MocCommentDTO dto)
		{
			return new Database.MocComment
			{
				id = dto.id,
				publishedMocId = dto.publishedMocId,
				commenterTenantGuid = dto.commenterTenantGuid,
				commentText = dto.commentText,
				postedDate = dto.postedDate,
				mocCommentId = dto.mocCommentId,
				isEdited = dto.isEdited,
				isHidden = dto.isHidden,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a MocComment Object.
		///
		/// </summary>
		public void ApplyDTO(MocCommentDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.publishedMocId = dto.publishedMocId;
			this.commenterTenantGuid = dto.commenterTenantGuid;
			this.commentText = dto.commentText;
			this.postedDate = dto.postedDate;
			this.mocCommentId = dto.mocCommentId;
			this.isEdited = dto.isEdited;
			this.isHidden = dto.isHidden;
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
		/// Creates a deep copy clone of a MocComment Object.
		///
		/// </summary>
		public MocComment Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new MocComment{
				id = this.id,
				publishedMocId = this.publishedMocId,
				commenterTenantGuid = this.commenterTenantGuid,
				commentText = this.commentText,
				postedDate = this.postedDate,
				mocCommentId = this.mocCommentId,
				isEdited = this.isEdited,
				isHidden = this.isHidden,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MocComment Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MocComment Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a MocComment Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a MocComment Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.MocComment mocComment)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (mocComment == null)
			{
				return null;
			}

			return new {
				id = mocComment.id,
				publishedMocId = mocComment.publishedMocId,
				commenterTenantGuid = mocComment.commenterTenantGuid,
				commentText = mocComment.commentText,
				postedDate = mocComment.postedDate,
				mocCommentId = mocComment.mocCommentId,
				isEdited = mocComment.isEdited,
				isHidden = mocComment.isHidden,
				objectGuid = mocComment.objectGuid,
				active = mocComment.active,
				deleted = mocComment.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a MocComment Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(MocComment mocComment)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (mocComment == null)
			{
				return null;
			}

			return new {
				id = mocComment.id,
				publishedMocId = mocComment.publishedMocId,
				commenterTenantGuid = mocComment.commenterTenantGuid,
				commentText = mocComment.commentText,
				postedDate = mocComment.postedDate,
				mocCommentId = mocComment.mocCommentId,
				isEdited = mocComment.isEdited,
				isHidden = mocComment.isHidden,
				objectGuid = mocComment.objectGuid,
				active = mocComment.active,
				deleted = mocComment.deleted,
				mocComment = MocComment.CreateMinimalAnonymous(mocComment.mocComment),
				publishedMoc = PublishedMoc.CreateMinimalAnonymous(mocComment.publishedMoc)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a MocComment Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(MocComment mocComment)
		{
			//
			// Return a very minimal object.
			//
			if (mocComment == null)
			{
				return null;
			}

			return new {
				id = mocComment.id,
				name = mocComment.id,
				description = mocComment.id
			 };
		}
	}
}
