using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Alerting.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class NotificationDeliveryAttempt : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class NotificationDeliveryAttemptDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 incidentNotificationId { get; set; }
			[Required]
			public Int32 notificationChannelTypeId { get; set; }
			[Required]
			public Int32 attemptNumber { get; set; }
			[Required]
			public DateTime attemptedAt { get; set; }
			[Required]
			public String status { get; set; }
			public String errorMessage { get; set; }
			public String response { get; set; }
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
		public class NotificationDeliveryAttemptOutputDTO : NotificationDeliveryAttemptDTO
		{
			public IncidentNotification.IncidentNotificationDTO incidentNotification { get; set; }
			public NotificationChannelType.NotificationChannelTypeDTO notificationChannelType { get; set; }
		}


		/// <summary>
		///
		/// Converts a NotificationDeliveryAttempt to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public NotificationDeliveryAttemptDTO ToDTO()
		{
			return new NotificationDeliveryAttemptDTO
			{
				id = this.id,
				incidentNotificationId = this.incidentNotificationId,
				notificationChannelTypeId = this.notificationChannelTypeId,
				attemptNumber = this.attemptNumber,
				attemptedAt = this.attemptedAt,
				status = this.status,
				errorMessage = this.errorMessage,
				response = this.response,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a NotificationDeliveryAttempt list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<NotificationDeliveryAttemptDTO> ToDTOList(List<NotificationDeliveryAttempt> data)
		{
			if (data == null)
			{
				return null;
			}

			List<NotificationDeliveryAttemptDTO> output = new List<NotificationDeliveryAttemptDTO>();

			output.Capacity = data.Count;

			foreach (NotificationDeliveryAttempt notificationDeliveryAttempt in data)
			{
				output.Add(notificationDeliveryAttempt.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a NotificationDeliveryAttempt to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the NotificationDeliveryAttemptEntity type directly.
		///
		/// </summary>
		public NotificationDeliveryAttemptOutputDTO ToOutputDTO()
		{
			return new NotificationDeliveryAttemptOutputDTO
			{
				id = this.id,
				incidentNotificationId = this.incidentNotificationId,
				notificationChannelTypeId = this.notificationChannelTypeId,
				attemptNumber = this.attemptNumber,
				attemptedAt = this.attemptedAt,
				status = this.status,
				errorMessage = this.errorMessage,
				response = this.response,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				incidentNotification = this.incidentNotification?.ToDTO(),
				notificationChannelType = this.notificationChannelType?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a NotificationDeliveryAttempt list to list of Output Data Transfer Object intended to be used for serializing a list of NotificationDeliveryAttempt objects to avoid using the NotificationDeliveryAttempt entity type directly.
		///
		/// </summary>
		public static List<NotificationDeliveryAttemptOutputDTO> ToOutputDTOList(List<NotificationDeliveryAttempt> data)
		{
			if (data == null)
			{
				return null;
			}

			List<NotificationDeliveryAttemptOutputDTO> output = new List<NotificationDeliveryAttemptOutputDTO>();

			output.Capacity = data.Count;

			foreach (NotificationDeliveryAttempt notificationDeliveryAttempt in data)
			{
				output.Add(notificationDeliveryAttempt.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a NotificationDeliveryAttempt Object.
		///
		/// </summary>
		public static Database.NotificationDeliveryAttempt FromDTO(NotificationDeliveryAttemptDTO dto)
		{
			return new Database.NotificationDeliveryAttempt
			{
				id = dto.id,
				incidentNotificationId = dto.incidentNotificationId,
				notificationChannelTypeId = dto.notificationChannelTypeId,
				attemptNumber = dto.attemptNumber,
				attemptedAt = dto.attemptedAt,
				status = dto.status,
				errorMessage = dto.errorMessage,
				response = dto.response,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a NotificationDeliveryAttempt Object.
		///
		/// </summary>
		public void ApplyDTO(NotificationDeliveryAttemptDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.incidentNotificationId = dto.incidentNotificationId;
			this.notificationChannelTypeId = dto.notificationChannelTypeId;
			this.attemptNumber = dto.attemptNumber;
			this.attemptedAt = dto.attemptedAt;
			this.status = dto.status;
			this.errorMessage = dto.errorMessage;
			this.response = dto.response;
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
		/// Creates a deep copy clone of a NotificationDeliveryAttempt Object.
		///
		/// </summary>
		public NotificationDeliveryAttempt Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new NotificationDeliveryAttempt{
				id = this.id,
				tenantGuid = this.tenantGuid,
				incidentNotificationId = this.incidentNotificationId,
				notificationChannelTypeId = this.notificationChannelTypeId,
				attemptNumber = this.attemptNumber,
				attemptedAt = this.attemptedAt,
				status = this.status,
				errorMessage = this.errorMessage,
				response = this.response,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a NotificationDeliveryAttempt Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a NotificationDeliveryAttempt Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a NotificationDeliveryAttempt Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a NotificationDeliveryAttempt Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.NotificationDeliveryAttempt notificationDeliveryAttempt)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (notificationDeliveryAttempt == null)
			{
				return null;
			}

			return new {
				id = notificationDeliveryAttempt.id,
				incidentNotificationId = notificationDeliveryAttempt.incidentNotificationId,
				notificationChannelTypeId = notificationDeliveryAttempt.notificationChannelTypeId,
				attemptNumber = notificationDeliveryAttempt.attemptNumber,
				attemptedAt = notificationDeliveryAttempt.attemptedAt,
				status = notificationDeliveryAttempt.status,
				errorMessage = notificationDeliveryAttempt.errorMessage,
				response = notificationDeliveryAttempt.response,
				objectGuid = notificationDeliveryAttempt.objectGuid,
				active = notificationDeliveryAttempt.active,
				deleted = notificationDeliveryAttempt.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a NotificationDeliveryAttempt Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(NotificationDeliveryAttempt notificationDeliveryAttempt)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (notificationDeliveryAttempt == null)
			{
				return null;
			}

			return new {
				id = notificationDeliveryAttempt.id,
				incidentNotificationId = notificationDeliveryAttempt.incidentNotificationId,
				notificationChannelTypeId = notificationDeliveryAttempt.notificationChannelTypeId,
				attemptNumber = notificationDeliveryAttempt.attemptNumber,
				attemptedAt = notificationDeliveryAttempt.attemptedAt,
				status = notificationDeliveryAttempt.status,
				errorMessage = notificationDeliveryAttempt.errorMessage,
				response = notificationDeliveryAttempt.response,
				objectGuid = notificationDeliveryAttempt.objectGuid,
				active = notificationDeliveryAttempt.active,
				deleted = notificationDeliveryAttempt.deleted,
				incidentNotification = IncidentNotification.CreateMinimalAnonymous(notificationDeliveryAttempt.incidentNotification),
				notificationChannelType = NotificationChannelType.CreateMinimalAnonymous(notificationDeliveryAttempt.notificationChannelType)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a NotificationDeliveryAttempt Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(NotificationDeliveryAttempt notificationDeliveryAttempt)
		{
			//
			// Return a very minimal object.
			//
			if (notificationDeliveryAttempt == null)
			{
				return null;
			}

			return new {
				id = notificationDeliveryAttempt.id,
				name = notificationDeliveryAttempt.status,
				description = string.Join(", ", new[] { notificationDeliveryAttempt.status}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
