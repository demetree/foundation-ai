using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Scheduler.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class NotificationDistribution : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class NotificationDistributionDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 notificationId { get; set; }
			[Required]
			public Int32 userId { get; set; }
			[Required]
			public Boolean acknowledged { get; set; }
			public DateTime? dateTimeAcknowledged { get; set; }
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
		public class NotificationDistributionOutputDTO : NotificationDistributionDTO
		{
			public Notification.NotificationDTO notification { get; set; }
		}


		/// <summary>
		///
		/// Converts a NotificationDistribution to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public NotificationDistributionDTO ToDTO()
		{
			return new NotificationDistributionDTO
			{
				id = this.id,
				notificationId = this.notificationId,
				userId = this.userId,
				acknowledged = this.acknowledged,
				dateTimeAcknowledged = this.dateTimeAcknowledged,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a NotificationDistribution list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<NotificationDistributionDTO> ToDTOList(List<NotificationDistribution> data)
		{
			if (data == null)
			{
				return null;
			}

			List<NotificationDistributionDTO> output = new List<NotificationDistributionDTO>();

			output.Capacity = data.Count;

			foreach (NotificationDistribution notificationDistribution in data)
			{
				output.Add(notificationDistribution.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a NotificationDistribution to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the NotificationDistribution Entity type directly.
		///
		/// </summary>
		public NotificationDistributionOutputDTO ToOutputDTO()
		{
			return new NotificationDistributionOutputDTO
			{
				id = this.id,
				notificationId = this.notificationId,
				userId = this.userId,
				acknowledged = this.acknowledged,
				dateTimeAcknowledged = this.dateTimeAcknowledged,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				notification = this.notification?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a NotificationDistribution list to list of Output Data Transfer Object intended to be used for serializing a list of NotificationDistribution objects to avoid using the NotificationDistribution entity type directly.
		///
		/// </summary>
		public static List<NotificationDistributionOutputDTO> ToOutputDTOList(List<NotificationDistribution> data)
		{
			if (data == null)
			{
				return null;
			}

			List<NotificationDistributionOutputDTO> output = new List<NotificationDistributionOutputDTO>();

			output.Capacity = data.Count;

			foreach (NotificationDistribution notificationDistribution in data)
			{
				output.Add(notificationDistribution.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a NotificationDistribution Object.
		///
		/// </summary>
		public static Database.NotificationDistribution FromDTO(NotificationDistributionDTO dto)
		{
			return new Database.NotificationDistribution
			{
				id = dto.id,
				notificationId = dto.notificationId,
				userId = dto.userId,
				acknowledged = dto.acknowledged,
				dateTimeAcknowledged = dto.dateTimeAcknowledged,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a NotificationDistribution Object.
		///
		/// </summary>
		public void ApplyDTO(NotificationDistributionDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.notificationId = dto.notificationId;
			this.userId = dto.userId;
			this.acknowledged = dto.acknowledged;
			this.dateTimeAcknowledged = dto.dateTimeAcknowledged;
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
		/// Creates a deep copy clone of a NotificationDistribution Object.
		///
		/// </summary>
		public NotificationDistribution Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new NotificationDistribution{
				id = this.id,
				tenantGuid = this.tenantGuid,
				notificationId = this.notificationId,
				userId = this.userId,
				acknowledged = this.acknowledged,
				dateTimeAcknowledged = this.dateTimeAcknowledged,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a NotificationDistribution Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a NotificationDistribution Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a NotificationDistribution Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a NotificationDistribution Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.NotificationDistribution notificationDistribution)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (notificationDistribution == null)
			{
				return null;
			}

			return new {
				id = notificationDistribution.id,
				notificationId = notificationDistribution.notificationId,
				userId = notificationDistribution.userId,
				acknowledged = notificationDistribution.acknowledged,
				dateTimeAcknowledged = notificationDistribution.dateTimeAcknowledged,
				objectGuid = notificationDistribution.objectGuid,
				active = notificationDistribution.active,
				deleted = notificationDistribution.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a NotificationDistribution Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(NotificationDistribution notificationDistribution)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (notificationDistribution == null)
			{
				return null;
			}

			return new {
				id = notificationDistribution.id,
				notificationId = notificationDistribution.notificationId,
				userId = notificationDistribution.userId,
				acknowledged = notificationDistribution.acknowledged,
				dateTimeAcknowledged = notificationDistribution.dateTimeAcknowledged,
				objectGuid = notificationDistribution.objectGuid,
				active = notificationDistribution.active,
				deleted = notificationDistribution.deleted,
				notification = Notification.CreateMinimalAnonymous(notificationDistribution.notification)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a NotificationDistribution Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(NotificationDistribution notificationDistribution)
		{
			//
			// Return a very minimal object.
			//
			if (notificationDistribution == null)
			{
				return null;
			}

			return new {
				id = notificationDistribution.id,
				name = notificationDistribution.id,
				description = notificationDistribution.id
			 };
		}
	}
}
