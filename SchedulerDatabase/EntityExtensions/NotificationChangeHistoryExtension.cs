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
	public partial class NotificationChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)notificationId; }
			set { notificationId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class NotificationChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 notificationId { get; set; }
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
		public class NotificationChangeHistoryOutputDTO : NotificationChangeHistoryDTO
		{
			public Notification.NotificationDTO notification { get; set; }
		}


		/// <summary>
		///
		/// Converts a NotificationChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public NotificationChangeHistoryDTO ToDTO()
		{
			return new NotificationChangeHistoryDTO
			{
				id = this.id,
				notificationId = this.notificationId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a NotificationChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<NotificationChangeHistoryDTO> ToDTOList(List<NotificationChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<NotificationChangeHistoryDTO> output = new List<NotificationChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (NotificationChangeHistory notificationChangeHistory in data)
			{
				output.Add(notificationChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a NotificationChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the NotificationChangeHistory Entity type directly.
		///
		/// </summary>
		public NotificationChangeHistoryOutputDTO ToOutputDTO()
		{
			return new NotificationChangeHistoryOutputDTO
			{
				id = this.id,
				notificationId = this.notificationId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				notification = this.notification?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a NotificationChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of NotificationChangeHistory objects to avoid using the NotificationChangeHistory entity type directly.
		///
		/// </summary>
		public static List<NotificationChangeHistoryOutputDTO> ToOutputDTOList(List<NotificationChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<NotificationChangeHistoryOutputDTO> output = new List<NotificationChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (NotificationChangeHistory notificationChangeHistory in data)
			{
				output.Add(notificationChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a NotificationChangeHistory Object.
		///
		/// </summary>
		public static Database.NotificationChangeHistory FromDTO(NotificationChangeHistoryDTO dto)
		{
			return new Database.NotificationChangeHistory
			{
				id = dto.id,
				notificationId = dto.notificationId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a NotificationChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(NotificationChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.notificationId = dto.notificationId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a NotificationChangeHistory Object.
		///
		/// </summary>
		public NotificationChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new NotificationChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				notificationId = this.notificationId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a NotificationChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a NotificationChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a NotificationChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a NotificationChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.NotificationChangeHistory notificationChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (notificationChangeHistory == null)
			{
				return null;
			}

			return new {
				id = notificationChangeHistory.id,
				notificationId = notificationChangeHistory.notificationId,
				versionNumber = notificationChangeHistory.versionNumber,
				timeStamp = notificationChangeHistory.timeStamp,
				userId = notificationChangeHistory.userId,
				data = notificationChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a NotificationChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(NotificationChangeHistory notificationChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (notificationChangeHistory == null)
			{
				return null;
			}

			return new {
				id = notificationChangeHistory.id,
				notificationId = notificationChangeHistory.notificationId,
				versionNumber = notificationChangeHistory.versionNumber,
				timeStamp = notificationChangeHistory.timeStamp,
				userId = notificationChangeHistory.userId,
				data = notificationChangeHistory.data,
				notification = Notification.CreateMinimalAnonymous(notificationChangeHistory.notification)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a NotificationChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(NotificationChangeHistory notificationChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (notificationChangeHistory == null)
			{
				return null;
			}

			return new {
				id = notificationChangeHistory.id,
				name = notificationChangeHistory.id,
				description = notificationChangeHistory.id
			 };
		}
	}
}
