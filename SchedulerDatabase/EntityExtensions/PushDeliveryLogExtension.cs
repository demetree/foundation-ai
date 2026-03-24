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
	public partial class PushDeliveryLog : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class PushDeliveryLogDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 userId { get; set; }
			[Required]
			public String providerId { get; set; }
			public String destination { get; set; }
			public String sourceType { get; set; }
			public Int32? sourceNotificationId { get; set; }
			public Int32? sourceConversationMessageId { get; set; }
			[Required]
			public Boolean success { get; set; }
			public String externalId { get; set; }
			public String errorMessage { get; set; }
			[Required]
			public Int32 attemptNumber { get; set; }
			[Required]
			public DateTime dateTimeCreated { get; set; }
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
		public class PushDeliveryLogOutputDTO : PushDeliveryLogDTO
		{
		}


		/// <summary>
		///
		/// Converts a PushDeliveryLog to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public PushDeliveryLogDTO ToDTO()
		{
			return new PushDeliveryLogDTO
			{
				id = this.id,
				userId = this.userId,
				providerId = this.providerId,
				destination = this.destination,
				sourceType = this.sourceType,
				sourceNotificationId = this.sourceNotificationId,
				sourceConversationMessageId = this.sourceConversationMessageId,
				success = this.success,
				externalId = this.externalId,
				errorMessage = this.errorMessage,
				attemptNumber = this.attemptNumber,
				dateTimeCreated = this.dateTimeCreated,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a PushDeliveryLog list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<PushDeliveryLogDTO> ToDTOList(List<PushDeliveryLog> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PushDeliveryLogDTO> output = new List<PushDeliveryLogDTO>();

			output.Capacity = data.Count;

			foreach (PushDeliveryLog pushDeliveryLog in data)
			{
				output.Add(pushDeliveryLog.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a PushDeliveryLog to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the PushDeliveryLog Entity type directly.
		///
		/// </summary>
		public PushDeliveryLogOutputDTO ToOutputDTO()
		{
			return new PushDeliveryLogOutputDTO
			{
				id = this.id,
				userId = this.userId,
				providerId = this.providerId,
				destination = this.destination,
				sourceType = this.sourceType,
				sourceNotificationId = this.sourceNotificationId,
				sourceConversationMessageId = this.sourceConversationMessageId,
				success = this.success,
				externalId = this.externalId,
				errorMessage = this.errorMessage,
				attemptNumber = this.attemptNumber,
				dateTimeCreated = this.dateTimeCreated,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a PushDeliveryLog list to list of Output Data Transfer Object intended to be used for serializing a list of PushDeliveryLog objects to avoid using the PushDeliveryLog entity type directly.
		///
		/// </summary>
		public static List<PushDeliveryLogOutputDTO> ToOutputDTOList(List<PushDeliveryLog> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PushDeliveryLogOutputDTO> output = new List<PushDeliveryLogOutputDTO>();

			output.Capacity = data.Count;

			foreach (PushDeliveryLog pushDeliveryLog in data)
			{
				output.Add(pushDeliveryLog.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a PushDeliveryLog Object.
		///
		/// </summary>
		public static Database.PushDeliveryLog FromDTO(PushDeliveryLogDTO dto)
		{
			return new Database.PushDeliveryLog
			{
				id = dto.id,
				userId = dto.userId,
				providerId = dto.providerId,
				destination = dto.destination,
				sourceType = dto.sourceType,
				sourceNotificationId = dto.sourceNotificationId,
				sourceConversationMessageId = dto.sourceConversationMessageId,
				success = dto.success,
				externalId = dto.externalId,
				errorMessage = dto.errorMessage,
				attemptNumber = dto.attemptNumber,
				dateTimeCreated = dto.dateTimeCreated,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a PushDeliveryLog Object.
		///
		/// </summary>
		public void ApplyDTO(PushDeliveryLogDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.userId = dto.userId;
			this.providerId = dto.providerId;
			this.destination = dto.destination;
			this.sourceType = dto.sourceType;
			this.sourceNotificationId = dto.sourceNotificationId;
			this.sourceConversationMessageId = dto.sourceConversationMessageId;
			this.success = dto.success;
			this.externalId = dto.externalId;
			this.errorMessage = dto.errorMessage;
			this.attemptNumber = dto.attemptNumber;
			this.dateTimeCreated = dto.dateTimeCreated;
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
		/// Creates a deep copy clone of a PushDeliveryLog Object.
		///
		/// </summary>
		public PushDeliveryLog Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new PushDeliveryLog{
				id = this.id,
				tenantGuid = this.tenantGuid,
				userId = this.userId,
				providerId = this.providerId,
				destination = this.destination,
				sourceType = this.sourceType,
				sourceNotificationId = this.sourceNotificationId,
				sourceConversationMessageId = this.sourceConversationMessageId,
				success = this.success,
				externalId = this.externalId,
				errorMessage = this.errorMessage,
				attemptNumber = this.attemptNumber,
				dateTimeCreated = this.dateTimeCreated,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PushDeliveryLog Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PushDeliveryLog Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a PushDeliveryLog Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a PushDeliveryLog Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.PushDeliveryLog pushDeliveryLog)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (pushDeliveryLog == null)
			{
				return null;
			}

			return new {
				id = pushDeliveryLog.id,
				userId = pushDeliveryLog.userId,
				providerId = pushDeliveryLog.providerId,
				destination = pushDeliveryLog.destination,
				sourceType = pushDeliveryLog.sourceType,
				sourceNotificationId = pushDeliveryLog.sourceNotificationId,
				sourceConversationMessageId = pushDeliveryLog.sourceConversationMessageId,
				success = pushDeliveryLog.success,
				externalId = pushDeliveryLog.externalId,
				errorMessage = pushDeliveryLog.errorMessage,
				attemptNumber = pushDeliveryLog.attemptNumber,
				dateTimeCreated = pushDeliveryLog.dateTimeCreated,
				objectGuid = pushDeliveryLog.objectGuid,
				active = pushDeliveryLog.active,
				deleted = pushDeliveryLog.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a PushDeliveryLog Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(PushDeliveryLog pushDeliveryLog)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (pushDeliveryLog == null)
			{
				return null;
			}

			return new {
				id = pushDeliveryLog.id,
				userId = pushDeliveryLog.userId,
				providerId = pushDeliveryLog.providerId,
				destination = pushDeliveryLog.destination,
				sourceType = pushDeliveryLog.sourceType,
				sourceNotificationId = pushDeliveryLog.sourceNotificationId,
				sourceConversationMessageId = pushDeliveryLog.sourceConversationMessageId,
				success = pushDeliveryLog.success,
				externalId = pushDeliveryLog.externalId,
				errorMessage = pushDeliveryLog.errorMessage,
				attemptNumber = pushDeliveryLog.attemptNumber,
				dateTimeCreated = pushDeliveryLog.dateTimeCreated,
				objectGuid = pushDeliveryLog.objectGuid,
				active = pushDeliveryLog.active,
				deleted = pushDeliveryLog.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a PushDeliveryLog Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(PushDeliveryLog pushDeliveryLog)
		{
			//
			// Return a very minimal object.
			//
			if (pushDeliveryLog == null)
			{
				return null;
			}

			return new {
				id = pushDeliveryLog.id,
				name = pushDeliveryLog.providerId,
				description = string.Join(", ", new[] { pushDeliveryLog.providerId, pushDeliveryLog.destination, pushDeliveryLog.sourceType}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
