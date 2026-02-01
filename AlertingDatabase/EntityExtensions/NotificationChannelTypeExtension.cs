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
	public partial class NotificationChannelType : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class NotificationChannelTypeDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			public String description { get; set; }
			[Required]
			public Int32 defaultPriority { get; set; }
			public Boolean? active { get; set; }
			public Boolean? deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class NotificationChannelTypeOutputDTO : NotificationChannelTypeDTO
		{
		}


		/// <summary>
		///
		/// Converts a NotificationChannelType to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public NotificationChannelTypeDTO ToDTO()
		{
			return new NotificationChannelTypeDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				defaultPriority = this.defaultPriority,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a NotificationChannelType list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<NotificationChannelTypeDTO> ToDTOList(List<NotificationChannelType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<NotificationChannelTypeDTO> output = new List<NotificationChannelTypeDTO>();

			output.Capacity = data.Count;

			foreach (NotificationChannelType notificationChannelType in data)
			{
				output.Add(notificationChannelType.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a NotificationChannelType to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the NotificationChannelTypeEntity type directly.
		///
		/// </summary>
		public NotificationChannelTypeOutputDTO ToOutputDTO()
		{
			return new NotificationChannelTypeOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				defaultPriority = this.defaultPriority,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a NotificationChannelType list to list of Output Data Transfer Object intended to be used for serializing a list of NotificationChannelType objects to avoid using the NotificationChannelType entity type directly.
		///
		/// </summary>
		public static List<NotificationChannelTypeOutputDTO> ToOutputDTOList(List<NotificationChannelType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<NotificationChannelTypeOutputDTO> output = new List<NotificationChannelTypeOutputDTO>();

			output.Capacity = data.Count;

			foreach (NotificationChannelType notificationChannelType in data)
			{
				output.Add(notificationChannelType.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a NotificationChannelType Object.
		///
		/// </summary>
		public static Database.NotificationChannelType FromDTO(NotificationChannelTypeDTO dto)
		{
			return new Database.NotificationChannelType
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				defaultPriority = dto.defaultPriority,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a NotificationChannelType Object.
		///
		/// </summary>
		public void ApplyDTO(NotificationChannelTypeDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.defaultPriority = dto.defaultPriority;
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
		/// Creates a deep copy clone of a NotificationChannelType Object.
		///
		/// </summary>
		public NotificationChannelType Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new NotificationChannelType{
				id = this.id,
				name = this.name,
				description = this.description,
				defaultPriority = this.defaultPriority,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a NotificationChannelType Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a NotificationChannelType Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a NotificationChannelType Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a NotificationChannelType Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.NotificationChannelType notificationChannelType)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (notificationChannelType == null)
			{
				return null;
			}

			return new {
				id = notificationChannelType.id,
				name = notificationChannelType.name,
				description = notificationChannelType.description,
				defaultPriority = notificationChannelType.defaultPriority,
				active = notificationChannelType.active,
				deleted = notificationChannelType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a NotificationChannelType Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(NotificationChannelType notificationChannelType)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (notificationChannelType == null)
			{
				return null;
			}

			return new {
				id = notificationChannelType.id,
				name = notificationChannelType.name,
				description = notificationChannelType.description,
				defaultPriority = notificationChannelType.defaultPriority,
				active = notificationChannelType.active,
				deleted = notificationChannelType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a NotificationChannelType Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(NotificationChannelType notificationChannelType)
		{
			//
			// Return a very minimal object.
			//
			if (notificationChannelType == null)
			{
				return null;
			}

			return new {
				id = notificationChannelType.id,
				name = notificationChannelType.name,
				description = notificationChannelType.description,
			 };
		}
	}
}
