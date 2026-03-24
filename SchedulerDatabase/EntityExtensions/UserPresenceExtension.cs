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
	public partial class UserPresence : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class UserPresenceDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 userId { get; set; }
			[Required]
			public String status { get; set; }
			public String customStatusMessage { get; set; }
			[Required]
			public DateTime lastSeenDateTime { get; set; }
			[Required]
			public DateTime lastActivityDateTime { get; set; }
			[Required]
			public Int32 connectionCount { get; set; }
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
		public class UserPresenceOutputDTO : UserPresenceDTO
		{
		}


		/// <summary>
		///
		/// Converts a UserPresence to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public UserPresenceDTO ToDTO()
		{
			return new UserPresenceDTO
			{
				id = this.id,
				userId = this.userId,
				status = this.status,
				customStatusMessage = this.customStatusMessage,
				lastSeenDateTime = this.lastSeenDateTime,
				lastActivityDateTime = this.lastActivityDateTime,
				connectionCount = this.connectionCount,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a UserPresence list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<UserPresenceDTO> ToDTOList(List<UserPresence> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserPresenceDTO> output = new List<UserPresenceDTO>();

			output.Capacity = data.Count;

			foreach (UserPresence userPresence in data)
			{
				output.Add(userPresence.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a UserPresence to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the UserPresence Entity type directly.
		///
		/// </summary>
		public UserPresenceOutputDTO ToOutputDTO()
		{
			return new UserPresenceOutputDTO
			{
				id = this.id,
				userId = this.userId,
				status = this.status,
				customStatusMessage = this.customStatusMessage,
				lastSeenDateTime = this.lastSeenDateTime,
				lastActivityDateTime = this.lastActivityDateTime,
				connectionCount = this.connectionCount,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a UserPresence list to list of Output Data Transfer Object intended to be used for serializing a list of UserPresence objects to avoid using the UserPresence entity type directly.
		///
		/// </summary>
		public static List<UserPresenceOutputDTO> ToOutputDTOList(List<UserPresence> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserPresenceOutputDTO> output = new List<UserPresenceOutputDTO>();

			output.Capacity = data.Count;

			foreach (UserPresence userPresence in data)
			{
				output.Add(userPresence.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a UserPresence Object.
		///
		/// </summary>
		public static Database.UserPresence FromDTO(UserPresenceDTO dto)
		{
			return new Database.UserPresence
			{
				id = dto.id,
				userId = dto.userId,
				status = dto.status,
				customStatusMessage = dto.customStatusMessage,
				lastSeenDateTime = dto.lastSeenDateTime,
				lastActivityDateTime = dto.lastActivityDateTime,
				connectionCount = dto.connectionCount,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a UserPresence Object.
		///
		/// </summary>
		public void ApplyDTO(UserPresenceDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.userId = dto.userId;
			this.status = dto.status;
			this.customStatusMessage = dto.customStatusMessage;
			this.lastSeenDateTime = dto.lastSeenDateTime;
			this.lastActivityDateTime = dto.lastActivityDateTime;
			this.connectionCount = dto.connectionCount;
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
		/// Creates a deep copy clone of a UserPresence Object.
		///
		/// </summary>
		public UserPresence Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new UserPresence{
				id = this.id,
				tenantGuid = this.tenantGuid,
				userId = this.userId,
				status = this.status,
				customStatusMessage = this.customStatusMessage,
				lastSeenDateTime = this.lastSeenDateTime,
				lastActivityDateTime = this.lastActivityDateTime,
				connectionCount = this.connectionCount,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserPresence Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserPresence Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a UserPresence Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a UserPresence Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.UserPresence userPresence)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (userPresence == null)
			{
				return null;
			}

			return new {
				id = userPresence.id,
				userId = userPresence.userId,
				status = userPresence.status,
				customStatusMessage = userPresence.customStatusMessage,
				lastSeenDateTime = userPresence.lastSeenDateTime,
				lastActivityDateTime = userPresence.lastActivityDateTime,
				connectionCount = userPresence.connectionCount,
				objectGuid = userPresence.objectGuid,
				active = userPresence.active,
				deleted = userPresence.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a UserPresence Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(UserPresence userPresence)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (userPresence == null)
			{
				return null;
			}

			return new {
				id = userPresence.id,
				userId = userPresence.userId,
				status = userPresence.status,
				customStatusMessage = userPresence.customStatusMessage,
				lastSeenDateTime = userPresence.lastSeenDateTime,
				lastActivityDateTime = userPresence.lastActivityDateTime,
				connectionCount = userPresence.connectionCount,
				objectGuid = userPresence.objectGuid,
				active = userPresence.active,
				deleted = userPresence.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a UserPresence Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(UserPresence userPresence)
		{
			//
			// Return a very minimal object.
			//
			if (userPresence == null)
			{
				return null;
			}

			return new {
				id = userPresence.id,
				name = userPresence.status,
				description = string.Join(", ", new[] { userPresence.status, userPresence.customStatusMessage}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
