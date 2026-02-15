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
	public partial class ContentReportReason : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ContentReportReasonDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
			public Int32? sequence { get; set; }
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
		public class ContentReportReasonOutputDTO : ContentReportReasonDTO
		{
		}


		/// <summary>
		///
		/// Converts a ContentReportReason to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ContentReportReasonDTO ToDTO()
		{
			return new ContentReportReasonDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ContentReportReason list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ContentReportReasonDTO> ToDTOList(List<ContentReportReason> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ContentReportReasonDTO> output = new List<ContentReportReasonDTO>();

			output.Capacity = data.Count;

			foreach (ContentReportReason contentReportReason in data)
			{
				output.Add(contentReportReason.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ContentReportReason to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ContentReportReasonEntity type directly.
		///
		/// </summary>
		public ContentReportReasonOutputDTO ToOutputDTO()
		{
			return new ContentReportReasonOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ContentReportReason list to list of Output Data Transfer Object intended to be used for serializing a list of ContentReportReason objects to avoid using the ContentReportReason entity type directly.
		///
		/// </summary>
		public static List<ContentReportReasonOutputDTO> ToOutputDTOList(List<ContentReportReason> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ContentReportReasonOutputDTO> output = new List<ContentReportReasonOutputDTO>();

			output.Capacity = data.Count;

			foreach (ContentReportReason contentReportReason in data)
			{
				output.Add(contentReportReason.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ContentReportReason Object.
		///
		/// </summary>
		public static Database.ContentReportReason FromDTO(ContentReportReasonDTO dto)
		{
			return new Database.ContentReportReason
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ContentReportReason Object.
		///
		/// </summary>
		public void ApplyDTO(ContentReportReasonDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.sequence = dto.sequence;
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
		/// Creates a deep copy clone of a ContentReportReason Object.
		///
		/// </summary>
		public ContentReportReason Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ContentReportReason{
				id = this.id,
				name = this.name,
				description = this.description,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ContentReportReason Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ContentReportReason Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ContentReportReason Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ContentReportReason Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ContentReportReason contentReportReason)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (contentReportReason == null)
			{
				return null;
			}

			return new {
				id = contentReportReason.id,
				name = contentReportReason.name,
				description = contentReportReason.description,
				sequence = contentReportReason.sequence,
				objectGuid = contentReportReason.objectGuid,
				active = contentReportReason.active,
				deleted = contentReportReason.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ContentReportReason Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ContentReportReason contentReportReason)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (contentReportReason == null)
			{
				return null;
			}

			return new {
				id = contentReportReason.id,
				name = contentReportReason.name,
				description = contentReportReason.description,
				sequence = contentReportReason.sequence,
				objectGuid = contentReportReason.objectGuid,
				active = contentReportReason.active,
				deleted = contentReportReason.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ContentReportReason Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ContentReportReason contentReportReason)
		{
			//
			// Return a very minimal object.
			//
			if (contentReportReason == null)
			{
				return null;
			}

			return new {
				id = contentReportReason.id,
				name = contentReportReason.name,
				description = contentReportReason.description,
			 };
		}
	}
}
