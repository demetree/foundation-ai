using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.Scheduler.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class NotificationAttachmentChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)notificationAttachmentId; }
			set { notificationAttachmentId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class NotificationAttachmentChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 notificationAttachmentId { get; set; }
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
		public class NotificationAttachmentChangeHistoryOutputDTO : NotificationAttachmentChangeHistoryDTO
		{
			public NotificationAttachment.NotificationAttachmentDTO notificationAttachment { get; set; }
		}


		/// <summary>
		///
		/// Converts a NotificationAttachmentChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public NotificationAttachmentChangeHistoryDTO ToDTO()
		{
			return new NotificationAttachmentChangeHistoryDTO
			{
				id = this.id,
				notificationAttachmentId = this.notificationAttachmentId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a NotificationAttachmentChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<NotificationAttachmentChangeHistoryDTO> ToDTOList(List<NotificationAttachmentChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<NotificationAttachmentChangeHistoryDTO> output = new List<NotificationAttachmentChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (NotificationAttachmentChangeHistory notificationAttachmentChangeHistory in data)
			{
				output.Add(notificationAttachmentChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a NotificationAttachmentChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the NotificationAttachmentChangeHistory Entity type directly.
		///
		/// </summary>
		public NotificationAttachmentChangeHistoryOutputDTO ToOutputDTO()
		{
			return new NotificationAttachmentChangeHistoryOutputDTO
			{
				id = this.id,
				notificationAttachmentId = this.notificationAttachmentId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				notificationAttachment = this.notificationAttachment?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a NotificationAttachmentChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of NotificationAttachmentChangeHistory objects to avoid using the NotificationAttachmentChangeHistory entity type directly.
		///
		/// </summary>
		public static List<NotificationAttachmentChangeHistoryOutputDTO> ToOutputDTOList(List<NotificationAttachmentChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<NotificationAttachmentChangeHistoryOutputDTO> output = new List<NotificationAttachmentChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (NotificationAttachmentChangeHistory notificationAttachmentChangeHistory in data)
			{
				output.Add(notificationAttachmentChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a NotificationAttachmentChangeHistory Object.
		///
		/// </summary>
		public static Database.NotificationAttachmentChangeHistory FromDTO(NotificationAttachmentChangeHistoryDTO dto)
		{
			return new Database.NotificationAttachmentChangeHistory
			{
				id = dto.id,
				notificationAttachmentId = dto.notificationAttachmentId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a NotificationAttachmentChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(NotificationAttachmentChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.notificationAttachmentId = dto.notificationAttachmentId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a NotificationAttachmentChangeHistory Object.
		///
		/// </summary>
		public NotificationAttachmentChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new NotificationAttachmentChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				notificationAttachmentId = this.notificationAttachmentId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a NotificationAttachmentChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a NotificationAttachmentChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a NotificationAttachmentChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a NotificationAttachmentChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.NotificationAttachmentChangeHistory notificationAttachmentChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (notificationAttachmentChangeHistory == null)
			{
				return null;
			}

			return new {
				id = notificationAttachmentChangeHistory.id,
				notificationAttachmentId = notificationAttachmentChangeHistory.notificationAttachmentId,
				versionNumber = notificationAttachmentChangeHistory.versionNumber,
				timeStamp = notificationAttachmentChangeHistory.timeStamp,
				userId = notificationAttachmentChangeHistory.userId,
				data = notificationAttachmentChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a NotificationAttachmentChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(NotificationAttachmentChangeHistory notificationAttachmentChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (notificationAttachmentChangeHistory == null)
			{
				return null;
			}

			return new {
				id = notificationAttachmentChangeHistory.id,
				notificationAttachmentId = notificationAttachmentChangeHistory.notificationAttachmentId,
				versionNumber = notificationAttachmentChangeHistory.versionNumber,
				timeStamp = notificationAttachmentChangeHistory.timeStamp,
				userId = notificationAttachmentChangeHistory.userId,
				data = notificationAttachmentChangeHistory.data,
				notificationAttachment = NotificationAttachment.CreateMinimalAnonymous(notificationAttachmentChangeHistory.notificationAttachment),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a NotificationAttachmentChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(NotificationAttachmentChangeHistory notificationAttachmentChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (notificationAttachmentChangeHistory == null)
			{
				return null;
			}

			return new {
				id = notificationAttachmentChangeHistory.id,
				name = notificationAttachmentChangeHistory.id,
				description = notificationAttachmentChangeHistory.id
			 };
		}
	}
}
