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
	public partial class NotificationType : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class NotificationTypeDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
			public Int32? sequence { get; set; }
			public String color { get; set; }
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
		public class NotificationTypeOutputDTO : NotificationTypeDTO
		{
		}


		/// <summary>
		///
		/// Converts a NotificationType to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public NotificationTypeDTO ToDTO()
		{
			return new NotificationTypeDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				sequence = this.sequence,
				color = this.color,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a NotificationType list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<NotificationTypeDTO> ToDTOList(List<NotificationType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<NotificationTypeDTO> output = new List<NotificationTypeDTO>();

			output.Capacity = data.Count;

			foreach (NotificationType notificationType in data)
			{
				output.Add(notificationType.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a NotificationType to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the NotificationTypeEntity type directly.
		///
		/// </summary>
		public NotificationTypeOutputDTO ToOutputDTO()
		{
			return new NotificationTypeOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				sequence = this.sequence,
				color = this.color,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a NotificationType list to list of Output Data Transfer Object intended to be used for serializing a list of NotificationType objects to avoid using the NotificationType entity type directly.
		///
		/// </summary>
		public static List<NotificationTypeOutputDTO> ToOutputDTOList(List<NotificationType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<NotificationTypeOutputDTO> output = new List<NotificationTypeOutputDTO>();

			output.Capacity = data.Count;

			foreach (NotificationType notificationType in data)
			{
				output.Add(notificationType.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a NotificationType Object.
		///
		/// </summary>
		public static Database.NotificationType FromDTO(NotificationTypeDTO dto)
		{
			return new Database.NotificationType
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				sequence = dto.sequence,
				color = dto.color,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a NotificationType Object.
		///
		/// </summary>
		public void ApplyDTO(NotificationTypeDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.sequence = dto.sequence;
			this.color = dto.color;
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
		/// Creates a deep copy clone of a NotificationType Object.
		///
		/// </summary>
		public NotificationType Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new NotificationType{
				id = this.id,
				name = this.name,
				description = this.description,
				sequence = this.sequence,
				color = this.color,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a NotificationType Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a NotificationType Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a NotificationType Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a NotificationType Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.NotificationType notificationType)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (notificationType == null)
			{
				return null;
			}

			return new {
				id = notificationType.id,
				name = notificationType.name,
				description = notificationType.description,
				sequence = notificationType.sequence,
				color = notificationType.color,
				objectGuid = notificationType.objectGuid,
				active = notificationType.active,
				deleted = notificationType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a NotificationType Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(NotificationType notificationType)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (notificationType == null)
			{
				return null;
			}

			return new {
				id = notificationType.id,
				name = notificationType.name,
				description = notificationType.description,
				sequence = notificationType.sequence,
				color = notificationType.color,
				objectGuid = notificationType.objectGuid,
				active = notificationType.active,
				deleted = notificationType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a NotificationType Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(NotificationType notificationType)
		{
			//
			// Return a very minimal object.
			//
			if (notificationType == null)
			{
				return null;
			}

			return new {
				id = notificationType.id,
				name = notificationType.name,
				description = notificationType.description,
			 };
		}
	}
}
