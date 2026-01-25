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
	public partial class UserSession : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class UserSessionDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 securityUserId { get; set; }
			[Required]
			public Guid objectGuid { get; set; }
			public String tokenId { get; set; }
			[Required]
			public DateTime sessionStart { get; set; }
			[Required]
			public DateTime expiresAt { get; set; }
			public String ipAddress { get; set; }
			public String userAgent { get; set; }
			public String loginMethod { get; set; }
			public String clientApplication { get; set; }
			[Required]
			public Boolean isRevoked { get; set; }
			public DateTime? revokedAt { get; set; }
			public String revokedBy { get; set; }
			public String revokedReason { get; set; }
			public Boolean? active { get; set; }
			public Boolean? deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class UserSessionOutputDTO : UserSessionDTO
		{
			public SecurityUser.SecurityUserDTO securityUser { get; set; }
		}


		/// <summary>
		///
		/// Converts a UserSession to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public UserSessionDTO ToDTO()
		{
			return new UserSessionDTO
			{
				id = this.id,
				securityUserId = this.securityUserId,
				objectGuid = this.objectGuid,
				tokenId = this.tokenId,
				sessionStart = this.sessionStart,
				expiresAt = this.expiresAt,
				ipAddress = this.ipAddress,
				userAgent = this.userAgent,
				loginMethod = this.loginMethod,
				clientApplication = this.clientApplication,
				isRevoked = this.isRevoked,
				revokedAt = this.revokedAt,
				revokedBy = this.revokedBy,
				revokedReason = this.revokedReason,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a UserSession list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<UserSessionDTO> ToDTOList(List<UserSession> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserSessionDTO> output = new List<UserSessionDTO>();

			output.Capacity = data.Count;

			foreach (UserSession userSession in data)
			{
				output.Add(userSession.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a UserSession to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the UserSessionEntity type directly.
		///
		/// </summary>
		public UserSessionOutputDTO ToOutputDTO()
		{
			return new UserSessionOutputDTO
			{
				id = this.id,
				securityUserId = this.securityUserId,
				objectGuid = this.objectGuid,
				tokenId = this.tokenId,
				sessionStart = this.sessionStart,
				expiresAt = this.expiresAt,
				ipAddress = this.ipAddress,
				userAgent = this.userAgent,
				loginMethod = this.loginMethod,
				clientApplication = this.clientApplication,
				isRevoked = this.isRevoked,
				revokedAt = this.revokedAt,
				revokedBy = this.revokedBy,
				revokedReason = this.revokedReason,
				active = this.active,
				deleted = this.deleted,
				securityUser = this.securityUser?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a UserSession list to list of Output Data Transfer Object intended to be used for serializing a list of UserSession objects to avoid using the UserSession entity type directly.
		///
		/// </summary>
		public static List<UserSessionOutputDTO> ToOutputDTOList(List<UserSession> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserSessionOutputDTO> output = new List<UserSessionOutputDTO>();

			output.Capacity = data.Count;

			foreach (UserSession userSession in data)
			{
				output.Add(userSession.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a UserSession Object.
		///
		/// </summary>
		public static Database.UserSession FromDTO(UserSessionDTO dto)
		{
			return new Database.UserSession
			{
				id = dto.id,
				securityUserId = dto.securityUserId,
				objectGuid = dto.objectGuid,
				tokenId = dto.tokenId,
				sessionStart = dto.sessionStart,
				expiresAt = dto.expiresAt,
				ipAddress = dto.ipAddress,
				userAgent = dto.userAgent,
				loginMethod = dto.loginMethod,
				clientApplication = dto.clientApplication,
				isRevoked = dto.isRevoked,
				revokedAt = dto.revokedAt,
				revokedBy = dto.revokedBy,
				revokedReason = dto.revokedReason,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a UserSession Object.
		///
		/// </summary>
		public void ApplyDTO(UserSessionDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.securityUserId = dto.securityUserId;
			this.objectGuid = dto.objectGuid;
			this.tokenId = dto.tokenId;
			this.sessionStart = dto.sessionStart;
			this.expiresAt = dto.expiresAt;
			this.ipAddress = dto.ipAddress;
			this.userAgent = dto.userAgent;
			this.loginMethod = dto.loginMethod;
			this.clientApplication = dto.clientApplication;
			this.isRevoked = dto.isRevoked;
			this.revokedAt = dto.revokedAt;
			this.revokedBy = dto.revokedBy;
			this.revokedReason = dto.revokedReason;
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
		/// Creates a deep copy clone of a UserSession Object.
		///
		/// </summary>
		public UserSession Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new UserSession{
				id = this.id,
				securityUserId = this.securityUserId,
				objectGuid = this.objectGuid,
				tokenId = this.tokenId,
				sessionStart = this.sessionStart,
				expiresAt = this.expiresAt,
				ipAddress = this.ipAddress,
				userAgent = this.userAgent,
				loginMethod = this.loginMethod,
				clientApplication = this.clientApplication,
				isRevoked = this.isRevoked,
				revokedAt = this.revokedAt,
				revokedBy = this.revokedBy,
				revokedReason = this.revokedReason,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserSession Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserSession Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a UserSession Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a UserSession Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.UserSession userSession)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (userSession == null)
			{
				return null;
			}

			return new {
				id = userSession.id,
				securityUserId = userSession.securityUserId,
				objectGuid = userSession.objectGuid,
				tokenId = userSession.tokenId,
				sessionStart = userSession.sessionStart,
				expiresAt = userSession.expiresAt,
				ipAddress = userSession.ipAddress,
				userAgent = userSession.userAgent,
				loginMethod = userSession.loginMethod,
				clientApplication = userSession.clientApplication,
				isRevoked = userSession.isRevoked,
				revokedAt = userSession.revokedAt,
				revokedBy = userSession.revokedBy,
				revokedReason = userSession.revokedReason,
				active = userSession.active,
				deleted = userSession.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a UserSession Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(UserSession userSession)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (userSession == null)
			{
				return null;
			}

			return new {
				id = userSession.id,
				securityUserId = userSession.securityUserId,
				objectGuid = userSession.objectGuid,
				tokenId = userSession.tokenId,
				sessionStart = userSession.sessionStart,
				expiresAt = userSession.expiresAt,
				ipAddress = userSession.ipAddress,
				userAgent = userSession.userAgent,
				loginMethod = userSession.loginMethod,
				clientApplication = userSession.clientApplication,
				isRevoked = userSession.isRevoked,
				revokedAt = userSession.revokedAt,
				revokedBy = userSession.revokedBy,
				revokedReason = userSession.revokedReason,
				active = userSession.active,
				deleted = userSession.deleted,
				securityUser = SecurityUser.CreateMinimalAnonymous(userSession.securityUser)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a UserSession Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(UserSession userSession)
		{
			//
			// Return a very minimal object.
			//
			if (userSession == null)
			{
				return null;
			}

			return new {
				id = userSession.id,
				name = userSession.tokenId,
				description = string.Join(", ", new[] { userSession.tokenId, userSession.ipAddress, userSession.userAgent}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
