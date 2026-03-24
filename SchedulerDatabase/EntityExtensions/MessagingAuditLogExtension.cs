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
	public partial class MessagingAuditLog : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class MessagingAuditLogDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 performedByUserId { get; set; }
			[Required]
			public String action { get; set; }
			public String entityType { get; set; }
			public Int32? entityId { get; set; }
			public String details { get; set; }
			public String ipAddress { get; set; }
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
		public class MessagingAuditLogOutputDTO : MessagingAuditLogDTO
		{
		}


		/// <summary>
		///
		/// Converts a MessagingAuditLog to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public MessagingAuditLogDTO ToDTO()
		{
			return new MessagingAuditLogDTO
			{
				id = this.id,
				performedByUserId = this.performedByUserId,
				action = this.action,
				entityType = this.entityType,
				entityId = this.entityId,
				details = this.details,
				ipAddress = this.ipAddress,
				dateTimeCreated = this.dateTimeCreated,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a MessagingAuditLog list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<MessagingAuditLogDTO> ToDTOList(List<MessagingAuditLog> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MessagingAuditLogDTO> output = new List<MessagingAuditLogDTO>();

			output.Capacity = data.Count;

			foreach (MessagingAuditLog messagingAuditLog in data)
			{
				output.Add(messagingAuditLog.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a MessagingAuditLog to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the MessagingAuditLog Entity type directly.
		///
		/// </summary>
		public MessagingAuditLogOutputDTO ToOutputDTO()
		{
			return new MessagingAuditLogOutputDTO
			{
				id = this.id,
				performedByUserId = this.performedByUserId,
				action = this.action,
				entityType = this.entityType,
				entityId = this.entityId,
				details = this.details,
				ipAddress = this.ipAddress,
				dateTimeCreated = this.dateTimeCreated,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a MessagingAuditLog list to list of Output Data Transfer Object intended to be used for serializing a list of MessagingAuditLog objects to avoid using the MessagingAuditLog entity type directly.
		///
		/// </summary>
		public static List<MessagingAuditLogOutputDTO> ToOutputDTOList(List<MessagingAuditLog> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MessagingAuditLogOutputDTO> output = new List<MessagingAuditLogOutputDTO>();

			output.Capacity = data.Count;

			foreach (MessagingAuditLog messagingAuditLog in data)
			{
				output.Add(messagingAuditLog.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a MessagingAuditLog Object.
		///
		/// </summary>
		public static Database.MessagingAuditLog FromDTO(MessagingAuditLogDTO dto)
		{
			return new Database.MessagingAuditLog
			{
				id = dto.id,
				performedByUserId = dto.performedByUserId,
				action = dto.action,
				entityType = dto.entityType,
				entityId = dto.entityId,
				details = dto.details,
				ipAddress = dto.ipAddress,
				dateTimeCreated = dto.dateTimeCreated,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a MessagingAuditLog Object.
		///
		/// </summary>
		public void ApplyDTO(MessagingAuditLogDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.performedByUserId = dto.performedByUserId;
			this.action = dto.action;
			this.entityType = dto.entityType;
			this.entityId = dto.entityId;
			this.details = dto.details;
			this.ipAddress = dto.ipAddress;
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
		/// Creates a deep copy clone of a MessagingAuditLog Object.
		///
		/// </summary>
		public MessagingAuditLog Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new MessagingAuditLog{
				id = this.id,
				tenantGuid = this.tenantGuid,
				performedByUserId = this.performedByUserId,
				action = this.action,
				entityType = this.entityType,
				entityId = this.entityId,
				details = this.details,
				ipAddress = this.ipAddress,
				dateTimeCreated = this.dateTimeCreated,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MessagingAuditLog Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MessagingAuditLog Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a MessagingAuditLog Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a MessagingAuditLog Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.MessagingAuditLog messagingAuditLog)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (messagingAuditLog == null)
			{
				return null;
			}

			return new {
				id = messagingAuditLog.id,
				performedByUserId = messagingAuditLog.performedByUserId,
				action = messagingAuditLog.action,
				entityType = messagingAuditLog.entityType,
				entityId = messagingAuditLog.entityId,
				details = messagingAuditLog.details,
				ipAddress = messagingAuditLog.ipAddress,
				dateTimeCreated = messagingAuditLog.dateTimeCreated,
				objectGuid = messagingAuditLog.objectGuid,
				active = messagingAuditLog.active,
				deleted = messagingAuditLog.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a MessagingAuditLog Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(MessagingAuditLog messagingAuditLog)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (messagingAuditLog == null)
			{
				return null;
			}

			return new {
				id = messagingAuditLog.id,
				performedByUserId = messagingAuditLog.performedByUserId,
				action = messagingAuditLog.action,
				entityType = messagingAuditLog.entityType,
				entityId = messagingAuditLog.entityId,
				details = messagingAuditLog.details,
				ipAddress = messagingAuditLog.ipAddress,
				dateTimeCreated = messagingAuditLog.dateTimeCreated,
				objectGuid = messagingAuditLog.objectGuid,
				active = messagingAuditLog.active,
				deleted = messagingAuditLog.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a MessagingAuditLog Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(MessagingAuditLog messagingAuditLog)
		{
			//
			// Return a very minimal object.
			//
			if (messagingAuditLog == null)
			{
				return null;
			}

			return new {
				id = messagingAuditLog.id,
				name = messagingAuditLog.action,
				description = string.Join(", ", new[] { messagingAuditLog.action, messagingAuditLog.entityType, messagingAuditLog.ipAddress}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
