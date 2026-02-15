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
	public partial class ModerationAction : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ModerationActionDTO
		{
			public Int64 id { get; set; }
			[Required]
			public Guid moderatorTenantGuid { get; set; }
			[Required]
			public String actionType { get; set; }
			public Guid? targetTenantGuid { get; set; }
			public String targetEntityType { get; set; }
			public Int64? targetEntityId { get; set; }
			public String reason { get; set; }
			[Required]
			public DateTime actionDate { get; set; }
			public Int32? contentReportId { get; set; }
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
		public class ModerationActionOutputDTO : ModerationActionDTO
		{
			public ContentReport.ContentReportDTO contentReport { get; set; }
		}


		/// <summary>
		///
		/// Converts a ModerationAction to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ModerationActionDTO ToDTO()
		{
			return new ModerationActionDTO
			{
				id = this.id,
				moderatorTenantGuid = this.moderatorTenantGuid,
				actionType = this.actionType,
				targetTenantGuid = this.targetTenantGuid,
				targetEntityType = this.targetEntityType,
				targetEntityId = this.targetEntityId,
				reason = this.reason,
				actionDate = this.actionDate,
				contentReportId = this.contentReportId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ModerationAction list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ModerationActionDTO> ToDTOList(List<ModerationAction> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ModerationActionDTO> output = new List<ModerationActionDTO>();

			output.Capacity = data.Count;

			foreach (ModerationAction moderationAction in data)
			{
				output.Add(moderationAction.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ModerationAction to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ModerationActionEntity type directly.
		///
		/// </summary>
		public ModerationActionOutputDTO ToOutputDTO()
		{
			return new ModerationActionOutputDTO
			{
				id = this.id,
				moderatorTenantGuid = this.moderatorTenantGuid,
				actionType = this.actionType,
				targetTenantGuid = this.targetTenantGuid,
				targetEntityType = this.targetEntityType,
				targetEntityId = this.targetEntityId,
				reason = this.reason,
				actionDate = this.actionDate,
				contentReportId = this.contentReportId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				contentReport = this.contentReport?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ModerationAction list to list of Output Data Transfer Object intended to be used for serializing a list of ModerationAction objects to avoid using the ModerationAction entity type directly.
		///
		/// </summary>
		public static List<ModerationActionOutputDTO> ToOutputDTOList(List<ModerationAction> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ModerationActionOutputDTO> output = new List<ModerationActionOutputDTO>();

			output.Capacity = data.Count;

			foreach (ModerationAction moderationAction in data)
			{
				output.Add(moderationAction.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ModerationAction Object.
		///
		/// </summary>
		public static Database.ModerationAction FromDTO(ModerationActionDTO dto)
		{
			return new Database.ModerationAction
			{
				id = dto.id,
				moderatorTenantGuid = dto.moderatorTenantGuid,
				actionType = dto.actionType,
				targetTenantGuid = dto.targetTenantGuid,
				targetEntityType = dto.targetEntityType,
				targetEntityId = dto.targetEntityId,
				reason = dto.reason,
				actionDate = dto.actionDate,
				contentReportId = dto.contentReportId,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ModerationAction Object.
		///
		/// </summary>
		public void ApplyDTO(ModerationActionDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.moderatorTenantGuid = dto.moderatorTenantGuid;
			this.actionType = dto.actionType;
			this.targetTenantGuid = dto.targetTenantGuid;
			this.targetEntityType = dto.targetEntityType;
			this.targetEntityId = dto.targetEntityId;
			this.reason = dto.reason;
			this.actionDate = dto.actionDate;
			this.contentReportId = dto.contentReportId;
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
		/// Creates a deep copy clone of a ModerationAction Object.
		///
		/// </summary>
		public ModerationAction Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ModerationAction{
				id = this.id,
				moderatorTenantGuid = this.moderatorTenantGuid,
				actionType = this.actionType,
				targetTenantGuid = this.targetTenantGuid,
				targetEntityType = this.targetEntityType,
				targetEntityId = this.targetEntityId,
				reason = this.reason,
				actionDate = this.actionDate,
				contentReportId = this.contentReportId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ModerationAction Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ModerationAction Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ModerationAction Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ModerationAction Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ModerationAction moderationAction)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (moderationAction == null)
			{
				return null;
			}

			return new {
				id = moderationAction.id,
				moderatorTenantGuid = moderationAction.moderatorTenantGuid,
				actionType = moderationAction.actionType,
				targetTenantGuid = moderationAction.targetTenantGuid,
				targetEntityType = moderationAction.targetEntityType,
				targetEntityId = moderationAction.targetEntityId,
				reason = moderationAction.reason,
				actionDate = moderationAction.actionDate,
				contentReportId = moderationAction.contentReportId,
				objectGuid = moderationAction.objectGuid,
				active = moderationAction.active,
				deleted = moderationAction.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ModerationAction Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ModerationAction moderationAction)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (moderationAction == null)
			{
				return null;
			}

			return new {
				id = moderationAction.id,
				moderatorTenantGuid = moderationAction.moderatorTenantGuid,
				actionType = moderationAction.actionType,
				targetTenantGuid = moderationAction.targetTenantGuid,
				targetEntityType = moderationAction.targetEntityType,
				targetEntityId = moderationAction.targetEntityId,
				reason = moderationAction.reason,
				actionDate = moderationAction.actionDate,
				contentReportId = moderationAction.contentReportId,
				objectGuid = moderationAction.objectGuid,
				active = moderationAction.active,
				deleted = moderationAction.deleted,
				contentReport = ContentReport.CreateMinimalAnonymous(moderationAction.contentReport)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ModerationAction Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ModerationAction moderationAction)
		{
			//
			// Return a very minimal object.
			//
			if (moderationAction == null)
			{
				return null;
			}

			return new {
				id = moderationAction.id,
				name = moderationAction.actionType,
				description = string.Join(", ", new[] { moderationAction.actionType, moderationAction.targetEntityType}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
