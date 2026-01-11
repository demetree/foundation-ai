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
	public partial class NotificationSubscriptionChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)notificationSubscriptionId; }
			set { notificationSubscriptionId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class NotificationSubscriptionChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 notificationSubscriptionId { get; set; }
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
		public class NotificationSubscriptionChangeHistoryOutputDTO : NotificationSubscriptionChangeHistoryDTO
		{
			public NotificationSubscription.NotificationSubscriptionDTO notificationSubscription { get; set; }
		}


		/// <summary>
		///
		/// Converts a NotificationSubscriptionChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public NotificationSubscriptionChangeHistoryDTO ToDTO()
		{
			return new NotificationSubscriptionChangeHistoryDTO
			{
				id = this.id,
				notificationSubscriptionId = this.notificationSubscriptionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a NotificationSubscriptionChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<NotificationSubscriptionChangeHistoryDTO> ToDTOList(List<NotificationSubscriptionChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<NotificationSubscriptionChangeHistoryDTO> output = new List<NotificationSubscriptionChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (NotificationSubscriptionChangeHistory notificationSubscriptionChangeHistory in data)
			{
				output.Add(notificationSubscriptionChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a NotificationSubscriptionChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the NotificationSubscriptionChangeHistoryEntity type directly.
		///
		/// </summary>
		public NotificationSubscriptionChangeHistoryOutputDTO ToOutputDTO()
		{
			return new NotificationSubscriptionChangeHistoryOutputDTO
			{
				id = this.id,
				notificationSubscriptionId = this.notificationSubscriptionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				notificationSubscription = this.notificationSubscription?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a NotificationSubscriptionChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of NotificationSubscriptionChangeHistory objects to avoid using the NotificationSubscriptionChangeHistory entity type directly.
		///
		/// </summary>
		public static List<NotificationSubscriptionChangeHistoryOutputDTO> ToOutputDTOList(List<NotificationSubscriptionChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<NotificationSubscriptionChangeHistoryOutputDTO> output = new List<NotificationSubscriptionChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (NotificationSubscriptionChangeHistory notificationSubscriptionChangeHistory in data)
			{
				output.Add(notificationSubscriptionChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a NotificationSubscriptionChangeHistory Object.
		///
		/// </summary>
		public static Database.NotificationSubscriptionChangeHistory FromDTO(NotificationSubscriptionChangeHistoryDTO dto)
		{
			return new Database.NotificationSubscriptionChangeHistory
			{
				id = dto.id,
				notificationSubscriptionId = dto.notificationSubscriptionId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a NotificationSubscriptionChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(NotificationSubscriptionChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.notificationSubscriptionId = dto.notificationSubscriptionId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a NotificationSubscriptionChangeHistory Object.
		///
		/// </summary>
		public NotificationSubscriptionChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new NotificationSubscriptionChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				notificationSubscriptionId = this.notificationSubscriptionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a NotificationSubscriptionChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a NotificationSubscriptionChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a NotificationSubscriptionChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a NotificationSubscriptionChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.NotificationSubscriptionChangeHistory notificationSubscriptionChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (notificationSubscriptionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = notificationSubscriptionChangeHistory.id,
				notificationSubscriptionId = notificationSubscriptionChangeHistory.notificationSubscriptionId,
				versionNumber = notificationSubscriptionChangeHistory.versionNumber,
				timeStamp = notificationSubscriptionChangeHistory.timeStamp,
				userId = notificationSubscriptionChangeHistory.userId,
				data = notificationSubscriptionChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a NotificationSubscriptionChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(NotificationSubscriptionChangeHistory notificationSubscriptionChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (notificationSubscriptionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = notificationSubscriptionChangeHistory.id,
				notificationSubscriptionId = notificationSubscriptionChangeHistory.notificationSubscriptionId,
				versionNumber = notificationSubscriptionChangeHistory.versionNumber,
				timeStamp = notificationSubscriptionChangeHistory.timeStamp,
				userId = notificationSubscriptionChangeHistory.userId,
				data = notificationSubscriptionChangeHistory.data,
				notificationSubscription = NotificationSubscription.CreateMinimalAnonymous(notificationSubscriptionChangeHistory.notificationSubscription),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a NotificationSubscriptionChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(NotificationSubscriptionChangeHistory notificationSubscriptionChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (notificationSubscriptionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = notificationSubscriptionChangeHistory.id,
				name = notificationSubscriptionChangeHistory.id,
				description = notificationSubscriptionChangeHistory.id
			 };
		}
	}
}
