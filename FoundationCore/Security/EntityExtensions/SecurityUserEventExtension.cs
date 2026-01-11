using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Security.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class SecurityUserEvent : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SecurityUserEventDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 securityUserId { get; set; }
			[Required]
			public Int32 securityUserEventTypeId { get; set; }
			[Required]
			public DateTime timeStamp { get; set; }
			public String comments { get; set; }
			public Boolean? active { get; set; }
			public Boolean? deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class SecurityUserEventOutputDTO : SecurityUserEventDTO
		{
			public SecurityUser.SecurityUserDTO securityUser { get; set; }
			public SecurityUserEventType.SecurityUserEventTypeDTO securityUserEventType { get; set; }
		}


		/// <summary>
		///
		/// Converts a SecurityUserEvent to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SecurityUserEventDTO ToDTO()
		{
			return new SecurityUserEventDTO
			{
				id = this.id,
				securityUserId = this.securityUserId,
				securityUserEventTypeId = this.securityUserEventTypeId,
				timeStamp = this.timeStamp,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SecurityUserEvent list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SecurityUserEventDTO> ToDTOList(List<SecurityUserEvent> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityUserEventDTO> output = new List<SecurityUserEventDTO>();

			output.Capacity = data.Count;

			foreach (SecurityUserEvent securityUserEvent in data)
			{
				output.Add(securityUserEvent.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SecurityUserEvent to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SecurityUserEventEntity type directly.
		///
		/// </summary>
		public SecurityUserEventOutputDTO ToOutputDTO()
		{
			return new SecurityUserEventOutputDTO
			{
				id = this.id,
				securityUserId = this.securityUserId,
				securityUserEventTypeId = this.securityUserEventTypeId,
				timeStamp = this.timeStamp,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted,
				securityUser = this.securityUser?.ToDTO(),
				securityUserEventType = this.securityUserEventType?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a SecurityUserEvent list to list of Output Data Transfer Object intended to be used for serializing a list of SecurityUserEvent objects to avoid using the SecurityUserEvent entity type directly.
		///
		/// </summary>
		public static List<SecurityUserEventOutputDTO> ToOutputDTOList(List<SecurityUserEvent> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityUserEventOutputDTO> output = new List<SecurityUserEventOutputDTO>();

			output.Capacity = data.Count;

			foreach (SecurityUserEvent securityUserEvent in data)
			{
				output.Add(securityUserEvent.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SecurityUserEvent Object.
		///
		/// </summary>
		public static Database.SecurityUserEvent FromDTO(SecurityUserEventDTO dto)
		{
			return new Database.SecurityUserEvent
			{
				id = dto.id,
				securityUserId = dto.securityUserId,
				securityUserEventTypeId = dto.securityUserEventTypeId,
				timeStamp = dto.timeStamp,
				comments = dto.comments,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SecurityUserEvent Object.
		///
		/// </summary>
		public void ApplyDTO(SecurityUserEventDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.securityUserId = dto.securityUserId;
			this.securityUserEventTypeId = dto.securityUserEventTypeId;
			this.timeStamp = dto.timeStamp;
			this.comments = dto.comments;
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
		/// Creates a deep copy clone of a SecurityUserEvent Object.
		///
		/// </summary>
		public SecurityUserEvent Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SecurityUserEvent{
				id = this.id,
				securityUserId = this.securityUserId,
				securityUserEventTypeId = this.securityUserEventTypeId,
				timeStamp = this.timeStamp,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityUserEvent Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityUserEvent Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SecurityUserEvent Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityUserEvent Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SecurityUserEvent securityUserEvent)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (securityUserEvent == null)
			{
				return null;
			}

			return new {
				id = securityUserEvent.id,
				securityUserId = securityUserEvent.securityUserId,
				securityUserEventTypeId = securityUserEvent.securityUserEventTypeId,
				timeStamp = securityUserEvent.timeStamp,
				comments = securityUserEvent.comments,
				active = securityUserEvent.active,
				deleted = securityUserEvent.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityUserEvent Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SecurityUserEvent securityUserEvent)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (securityUserEvent == null)
			{
				return null;
			}

			return new {
				id = securityUserEvent.id,
				securityUserId = securityUserEvent.securityUserId,
				securityUserEventTypeId = securityUserEvent.securityUserEventTypeId,
				timeStamp = securityUserEvent.timeStamp,
				comments = securityUserEvent.comments,
				active = securityUserEvent.active,
				deleted = securityUserEvent.deleted,
				securityUser = SecurityUser.CreateMinimalAnonymous(securityUserEvent.securityUser),
				securityUserEventType = SecurityUserEventType.CreateMinimalAnonymous(securityUserEvent.securityUserEventType)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SecurityUserEvent Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SecurityUserEvent securityUserEvent)
		{
			//
			// Return a very minimal object.
			//
			if (securityUserEvent == null)
			{
				return null;
			}

			return new {
				id = securityUserEvent.id,
				name = securityUserEvent.comments,
				description = string.Join(", ", new[] { securityUserEvent.comments}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
