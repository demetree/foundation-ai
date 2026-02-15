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
	public partial class ContentReport : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ContentReportDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 contentReportReasonId { get; set; }
			[Required]
			public Guid reporterTenantGuid { get; set; }
			[Required]
			public String reportedEntityType { get; set; }
			[Required]
			public Int64 reportedEntityId { get; set; }
			public String description { get; set; }
			[Required]
			public String status { get; set; }
			[Required]
			public DateTime reportedDate { get; set; }
			public DateTime? reviewedDate { get; set; }
			public Guid? reviewerTenantGuid { get; set; }
			public String reviewNotes { get; set; }
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
		public class ContentReportOutputDTO : ContentReportDTO
		{
			public ContentReportReason.ContentReportReasonDTO contentReportReason { get; set; }
		}


		/// <summary>
		///
		/// Converts a ContentReport to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ContentReportDTO ToDTO()
		{
			return new ContentReportDTO
			{
				id = this.id,
				contentReportReasonId = this.contentReportReasonId,
				reporterTenantGuid = this.reporterTenantGuid,
				reportedEntityType = this.reportedEntityType,
				reportedEntityId = this.reportedEntityId,
				description = this.description,
				status = this.status,
				reportedDate = this.reportedDate,
				reviewedDate = this.reviewedDate,
				reviewerTenantGuid = this.reviewerTenantGuid,
				reviewNotes = this.reviewNotes,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ContentReport list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ContentReportDTO> ToDTOList(List<ContentReport> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ContentReportDTO> output = new List<ContentReportDTO>();

			output.Capacity = data.Count;

			foreach (ContentReport contentReport in data)
			{
				output.Add(contentReport.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ContentReport to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ContentReportEntity type directly.
		///
		/// </summary>
		public ContentReportOutputDTO ToOutputDTO()
		{
			return new ContentReportOutputDTO
			{
				id = this.id,
				contentReportReasonId = this.contentReportReasonId,
				reporterTenantGuid = this.reporterTenantGuid,
				reportedEntityType = this.reportedEntityType,
				reportedEntityId = this.reportedEntityId,
				description = this.description,
				status = this.status,
				reportedDate = this.reportedDate,
				reviewedDate = this.reviewedDate,
				reviewerTenantGuid = this.reviewerTenantGuid,
				reviewNotes = this.reviewNotes,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				contentReportReason = this.contentReportReason?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ContentReport list to list of Output Data Transfer Object intended to be used for serializing a list of ContentReport objects to avoid using the ContentReport entity type directly.
		///
		/// </summary>
		public static List<ContentReportOutputDTO> ToOutputDTOList(List<ContentReport> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ContentReportOutputDTO> output = new List<ContentReportOutputDTO>();

			output.Capacity = data.Count;

			foreach (ContentReport contentReport in data)
			{
				output.Add(contentReport.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ContentReport Object.
		///
		/// </summary>
		public static Database.ContentReport FromDTO(ContentReportDTO dto)
		{
			return new Database.ContentReport
			{
				id = dto.id,
				contentReportReasonId = dto.contentReportReasonId,
				reporterTenantGuid = dto.reporterTenantGuid,
				reportedEntityType = dto.reportedEntityType,
				reportedEntityId = dto.reportedEntityId,
				description = dto.description,
				status = dto.status,
				reportedDate = dto.reportedDate,
				reviewedDate = dto.reviewedDate,
				reviewerTenantGuid = dto.reviewerTenantGuid,
				reviewNotes = dto.reviewNotes,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ContentReport Object.
		///
		/// </summary>
		public void ApplyDTO(ContentReportDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.contentReportReasonId = dto.contentReportReasonId;
			this.reporterTenantGuid = dto.reporterTenantGuid;
			this.reportedEntityType = dto.reportedEntityType;
			this.reportedEntityId = dto.reportedEntityId;
			this.description = dto.description;
			this.status = dto.status;
			this.reportedDate = dto.reportedDate;
			this.reviewedDate = dto.reviewedDate;
			this.reviewerTenantGuid = dto.reviewerTenantGuid;
			this.reviewNotes = dto.reviewNotes;
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
		/// Creates a deep copy clone of a ContentReport Object.
		///
		/// </summary>
		public ContentReport Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ContentReport{
				id = this.id,
				contentReportReasonId = this.contentReportReasonId,
				reporterTenantGuid = this.reporterTenantGuid,
				reportedEntityType = this.reportedEntityType,
				reportedEntityId = this.reportedEntityId,
				description = this.description,
				status = this.status,
				reportedDate = this.reportedDate,
				reviewedDate = this.reviewedDate,
				reviewerTenantGuid = this.reviewerTenantGuid,
				reviewNotes = this.reviewNotes,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ContentReport Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ContentReport Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ContentReport Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ContentReport Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ContentReport contentReport)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (contentReport == null)
			{
				return null;
			}

			return new {
				id = contentReport.id,
				contentReportReasonId = contentReport.contentReportReasonId,
				reporterTenantGuid = contentReport.reporterTenantGuid,
				reportedEntityType = contentReport.reportedEntityType,
				reportedEntityId = contentReport.reportedEntityId,
				description = contentReport.description,
				status = contentReport.status,
				reportedDate = contentReport.reportedDate,
				reviewedDate = contentReport.reviewedDate,
				reviewerTenantGuid = contentReport.reviewerTenantGuid,
				reviewNotes = contentReport.reviewNotes,
				objectGuid = contentReport.objectGuid,
				active = contentReport.active,
				deleted = contentReport.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ContentReport Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ContentReport contentReport)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (contentReport == null)
			{
				return null;
			}

			return new {
				id = contentReport.id,
				contentReportReasonId = contentReport.contentReportReasonId,
				reporterTenantGuid = contentReport.reporterTenantGuid,
				reportedEntityType = contentReport.reportedEntityType,
				reportedEntityId = contentReport.reportedEntityId,
				description = contentReport.description,
				status = contentReport.status,
				reportedDate = contentReport.reportedDate,
				reviewedDate = contentReport.reviewedDate,
				reviewerTenantGuid = contentReport.reviewerTenantGuid,
				reviewNotes = contentReport.reviewNotes,
				objectGuid = contentReport.objectGuid,
				active = contentReport.active,
				deleted = contentReport.deleted,
				contentReportReason = ContentReportReason.CreateMinimalAnonymous(contentReport.contentReportReason)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ContentReport Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ContentReport contentReport)
		{
			//
			// Return a very minimal object.
			//
			if (contentReport == null)
			{
				return null;
			}

			return new {
				id = contentReport.id,
				description = contentReport.description,
				name = contentReport.reportedEntityType
			 };
		}
	}
}
