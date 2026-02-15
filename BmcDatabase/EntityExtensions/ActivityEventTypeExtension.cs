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
	public partial class ActivityEventType : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ActivityEventTypeDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
			public String iconCssClass { get; set; }
			public String accentColor { get; set; }
			public Int32? sequence { get; set; }
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
		public class ActivityEventTypeOutputDTO : ActivityEventTypeDTO
		{
		}


		/// <summary>
		///
		/// Converts a ActivityEventType to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ActivityEventTypeDTO ToDTO()
		{
			return new ActivityEventTypeDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				iconCssClass = this.iconCssClass,
				accentColor = this.accentColor,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ActivityEventType list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ActivityEventTypeDTO> ToDTOList(List<ActivityEventType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ActivityEventTypeDTO> output = new List<ActivityEventTypeDTO>();

			output.Capacity = data.Count;

			foreach (ActivityEventType activityEventType in data)
			{
				output.Add(activityEventType.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ActivityEventType to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ActivityEventTypeEntity type directly.
		///
		/// </summary>
		public ActivityEventTypeOutputDTO ToOutputDTO()
		{
			return new ActivityEventTypeOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				iconCssClass = this.iconCssClass,
				accentColor = this.accentColor,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ActivityEventType list to list of Output Data Transfer Object intended to be used for serializing a list of ActivityEventType objects to avoid using the ActivityEventType entity type directly.
		///
		/// </summary>
		public static List<ActivityEventTypeOutputDTO> ToOutputDTOList(List<ActivityEventType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ActivityEventTypeOutputDTO> output = new List<ActivityEventTypeOutputDTO>();

			output.Capacity = data.Count;

			foreach (ActivityEventType activityEventType in data)
			{
				output.Add(activityEventType.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ActivityEventType Object.
		///
		/// </summary>
		public static Database.ActivityEventType FromDTO(ActivityEventTypeDTO dto)
		{
			return new Database.ActivityEventType
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				iconCssClass = dto.iconCssClass,
				accentColor = dto.accentColor,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ActivityEventType Object.
		///
		/// </summary>
		public void ApplyDTO(ActivityEventTypeDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.iconCssClass = dto.iconCssClass;
			this.accentColor = dto.accentColor;
			this.sequence = dto.sequence;
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
		/// Creates a deep copy clone of a ActivityEventType Object.
		///
		/// </summary>
		public ActivityEventType Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ActivityEventType{
				id = this.id,
				name = this.name,
				description = this.description,
				iconCssClass = this.iconCssClass,
				accentColor = this.accentColor,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ActivityEventType Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ActivityEventType Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ActivityEventType Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ActivityEventType Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ActivityEventType activityEventType)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (activityEventType == null)
			{
				return null;
			}

			return new {
				id = activityEventType.id,
				name = activityEventType.name,
				description = activityEventType.description,
				iconCssClass = activityEventType.iconCssClass,
				accentColor = activityEventType.accentColor,
				sequence = activityEventType.sequence,
				objectGuid = activityEventType.objectGuid,
				active = activityEventType.active,
				deleted = activityEventType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ActivityEventType Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ActivityEventType activityEventType)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (activityEventType == null)
			{
				return null;
			}

			return new {
				id = activityEventType.id,
				name = activityEventType.name,
				description = activityEventType.description,
				iconCssClass = activityEventType.iconCssClass,
				accentColor = activityEventType.accentColor,
				sequence = activityEventType.sequence,
				objectGuid = activityEventType.objectGuid,
				active = activityEventType.active,
				deleted = activityEventType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ActivityEventType Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ActivityEventType activityEventType)
		{
			//
			// Return a very minimal object.
			//
			if (activityEventType == null)
			{
				return null;
			}

			return new {
				id = activityEventType.id,
				name = activityEventType.name,
				description = activityEventType.description,
			 };
		}
	}
}
