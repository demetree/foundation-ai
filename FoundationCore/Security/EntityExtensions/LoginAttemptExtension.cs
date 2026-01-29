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
	public partial class LoginAttempt : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class LoginAttemptDTO
		{
			public Int32 id { get; set; }
			[Required]
			public DateTime timeStamp { get; set; }
			public String userName { get; set; }
			public Int32? passwordHash { get; set; }
			public String resource { get; set; }
			public String sessionId { get; set; }
			public String ipAddress { get; set; }
			public String userAgent { get; set; }
			public String value { get; set; }
			public Boolean? success { get; set; }
			public Int32? securityUserId { get; set; }
			public Boolean? active { get; set; }
			public Boolean? deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class LoginAttemptOutputDTO : LoginAttemptDTO
		{
			public SecurityUser.SecurityUserDTO securityUser { get; set; }
		}


		/// <summary>
		///
		/// Converts a LoginAttempt to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public LoginAttemptDTO ToDTO()
		{
			return new LoginAttemptDTO
			{
				id = this.id,
				timeStamp = this.timeStamp,
				userName = this.userName,
				passwordHash = this.passwordHash,
				resource = this.resource,
				sessionId = this.sessionId,
				ipAddress = this.ipAddress,
				userAgent = this.userAgent,
				value = this.value,
				success = this.success,
				securityUserId = this.securityUserId,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a LoginAttempt list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<LoginAttemptDTO> ToDTOList(List<LoginAttempt> data)
		{
			if (data == null)
			{
				return null;
			}

			List<LoginAttemptDTO> output = new List<LoginAttemptDTO>();

			output.Capacity = data.Count;

			foreach (LoginAttempt loginAttempt in data)
			{
				output.Add(loginAttempt.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a LoginAttempt to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the LoginAttemptEntity type directly.
		///
		/// </summary>
		public LoginAttemptOutputDTO ToOutputDTO()
		{
			return new LoginAttemptOutputDTO
			{
				id = this.id,
				timeStamp = this.timeStamp,
				userName = this.userName,
				passwordHash = this.passwordHash,
				resource = this.resource,
				sessionId = this.sessionId,
				ipAddress = this.ipAddress,
				userAgent = this.userAgent,
				value = this.value,
				success = this.success,
				securityUserId = this.securityUserId,
				active = this.active,
				deleted = this.deleted,
				securityUser = this.securityUser?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a LoginAttempt list to list of Output Data Transfer Object intended to be used for serializing a list of LoginAttempt objects to avoid using the LoginAttempt entity type directly.
		///
		/// </summary>
		public static List<LoginAttemptOutputDTO> ToOutputDTOList(List<LoginAttempt> data)
		{
			if (data == null)
			{
				return null;
			}

			List<LoginAttemptOutputDTO> output = new List<LoginAttemptOutputDTO>();

			output.Capacity = data.Count;

			foreach (LoginAttempt loginAttempt in data)
			{
				output.Add(loginAttempt.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a LoginAttempt Object.
		///
		/// </summary>
		public static Database.LoginAttempt FromDTO(LoginAttemptDTO dto)
		{
			return new Database.LoginAttempt
			{
				id = dto.id,
				timeStamp = dto.timeStamp,
				userName = dto.userName,
				passwordHash = dto.passwordHash,
				resource = dto.resource,
				sessionId = dto.sessionId,
				ipAddress = dto.ipAddress,
				userAgent = dto.userAgent,
				value = dto.value,
				success = dto.success,
				securityUserId = dto.securityUserId,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a LoginAttempt Object.
		///
		/// </summary>
		public void ApplyDTO(LoginAttemptDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.timeStamp = dto.timeStamp;
			this.userName = dto.userName;
			this.passwordHash = dto.passwordHash;
			this.resource = dto.resource;
			this.sessionId = dto.sessionId;
			this.ipAddress = dto.ipAddress;
			this.userAgent = dto.userAgent;
			this.value = dto.value;
			this.success = dto.success;
			this.securityUserId = dto.securityUserId;
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
		/// Creates a deep copy clone of a LoginAttempt Object.
		///
		/// </summary>
		public LoginAttempt Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new LoginAttempt{
				id = this.id,
				timeStamp = this.timeStamp,
				userName = this.userName,
				passwordHash = this.passwordHash,
				resource = this.resource,
				sessionId = this.sessionId,
				ipAddress = this.ipAddress,
				userAgent = this.userAgent,
				value = this.value,
				success = this.success,
				securityUserId = this.securityUserId,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a LoginAttempt Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a LoginAttempt Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a LoginAttempt Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a LoginAttempt Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.LoginAttempt loginAttempt)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (loginAttempt == null)
			{
				return null;
			}

			return new {
				id = loginAttempt.id,
				timeStamp = loginAttempt.timeStamp,
				userName = loginAttempt.userName,
				passwordHash = loginAttempt.passwordHash,
				resource = loginAttempt.resource,
				sessionId = loginAttempt.sessionId,
				ipAddress = loginAttempt.ipAddress,
				userAgent = loginAttempt.userAgent,
				value = loginAttempt.value,
				success = loginAttempt.success,
				securityUserId = loginAttempt.securityUserId,
				active = loginAttempt.active,
				deleted = loginAttempt.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a LoginAttempt Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(LoginAttempt loginAttempt)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (loginAttempt == null)
			{
				return null;
			}

			return new {
				id = loginAttempt.id,
				timeStamp = loginAttempt.timeStamp,
				userName = loginAttempt.userName,
				passwordHash = loginAttempt.passwordHash,
				resource = loginAttempt.resource,
				sessionId = loginAttempt.sessionId,
				ipAddress = loginAttempt.ipAddress,
				userAgent = loginAttempt.userAgent,
				value = loginAttempt.value,
				success = loginAttempt.success,
				securityUserId = loginAttempt.securityUserId,
				active = loginAttempt.active,
				deleted = loginAttempt.deleted,
				securityUser = SecurityUser.CreateMinimalAnonymous(loginAttempt.securityUser)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a LoginAttempt Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(LoginAttempt loginAttempt)
		{
			//
			// Return a very minimal object.
			//
			if (loginAttempt == null)
			{
				return null;
			}

			return new {
				id = loginAttempt.id,
				name = loginAttempt.userName,
				description = string.Join(", ", new[] { loginAttempt.userName, loginAttempt.resource, loginAttempt.sessionId}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
