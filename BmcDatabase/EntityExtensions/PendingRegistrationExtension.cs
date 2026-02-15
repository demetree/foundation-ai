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
	public partial class PendingRegistration : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class PendingRegistrationDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String accountName { get; set; }
			[Required]
			public String emailAddress { get; set; }
			public String displayName { get; set; }
			[Required]
			public String passwordHash { get; set; }
			[Required]
			public String verificationCode { get; set; }
			[Required]
			public DateTime codeExpiresAt { get; set; }
			[Required]
			public Int32 verificationAttempts { get; set; }
			[Required]
			public String status { get; set; }
			[Required]
			public DateTime createdAt { get; set; }
			public DateTime? verifiedAt { get; set; }
			public DateTime? provisionedAt { get; set; }
			public String ipAddress { get; set; }
			public String userAgent { get; set; }
			public String verificationChannel { get; set; }
			public String failureReason { get; set; }
			public Int32? provisionedSecurityUserId { get; set; }
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
		public class PendingRegistrationOutputDTO : PendingRegistrationDTO
		{
		}


		/// <summary>
		///
		/// Converts a PendingRegistration to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public PendingRegistrationDTO ToDTO()
		{
			return new PendingRegistrationDTO
			{
				id = this.id,
				accountName = this.accountName,
				emailAddress = this.emailAddress,
				displayName = this.displayName,
				passwordHash = this.passwordHash,
				verificationCode = this.verificationCode,
				codeExpiresAt = this.codeExpiresAt,
				verificationAttempts = this.verificationAttempts,
				status = this.status,
				createdAt = this.createdAt,
				verifiedAt = this.verifiedAt,
				provisionedAt = this.provisionedAt,
				ipAddress = this.ipAddress,
				userAgent = this.userAgent,
				verificationChannel = this.verificationChannel,
				failureReason = this.failureReason,
				provisionedSecurityUserId = this.provisionedSecurityUserId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a PendingRegistration list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<PendingRegistrationDTO> ToDTOList(List<PendingRegistration> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PendingRegistrationDTO> output = new List<PendingRegistrationDTO>();

			output.Capacity = data.Count;

			foreach (PendingRegistration pendingRegistration in data)
			{
				output.Add(pendingRegistration.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a PendingRegistration to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the PendingRegistrationEntity type directly.
		///
		/// </summary>
		public PendingRegistrationOutputDTO ToOutputDTO()
		{
			return new PendingRegistrationOutputDTO
			{
				id = this.id,
				accountName = this.accountName,
				emailAddress = this.emailAddress,
				displayName = this.displayName,
				passwordHash = this.passwordHash,
				verificationCode = this.verificationCode,
				codeExpiresAt = this.codeExpiresAt,
				verificationAttempts = this.verificationAttempts,
				status = this.status,
				createdAt = this.createdAt,
				verifiedAt = this.verifiedAt,
				provisionedAt = this.provisionedAt,
				ipAddress = this.ipAddress,
				userAgent = this.userAgent,
				verificationChannel = this.verificationChannel,
				failureReason = this.failureReason,
				provisionedSecurityUserId = this.provisionedSecurityUserId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a PendingRegistration list to list of Output Data Transfer Object intended to be used for serializing a list of PendingRegistration objects to avoid using the PendingRegistration entity type directly.
		///
		/// </summary>
		public static List<PendingRegistrationOutputDTO> ToOutputDTOList(List<PendingRegistration> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PendingRegistrationOutputDTO> output = new List<PendingRegistrationOutputDTO>();

			output.Capacity = data.Count;

			foreach (PendingRegistration pendingRegistration in data)
			{
				output.Add(pendingRegistration.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a PendingRegistration Object.
		///
		/// </summary>
		public static Database.PendingRegistration FromDTO(PendingRegistrationDTO dto)
		{
			return new Database.PendingRegistration
			{
				id = dto.id,
				accountName = dto.accountName,
				emailAddress = dto.emailAddress,
				displayName = dto.displayName,
				passwordHash = dto.passwordHash,
				verificationCode = dto.verificationCode,
				codeExpiresAt = dto.codeExpiresAt,
				verificationAttempts = dto.verificationAttempts,
				status = dto.status,
				createdAt = dto.createdAt,
				verifiedAt = dto.verifiedAt,
				provisionedAt = dto.provisionedAt,
				ipAddress = dto.ipAddress,
				userAgent = dto.userAgent,
				verificationChannel = dto.verificationChannel,
				failureReason = dto.failureReason,
				provisionedSecurityUserId = dto.provisionedSecurityUserId,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a PendingRegistration Object.
		///
		/// </summary>
		public void ApplyDTO(PendingRegistrationDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.accountName = dto.accountName;
			this.emailAddress = dto.emailAddress;
			this.displayName = dto.displayName;
			this.passwordHash = dto.passwordHash;
			this.verificationCode = dto.verificationCode;
			this.codeExpiresAt = dto.codeExpiresAt;
			this.verificationAttempts = dto.verificationAttempts;
			this.status = dto.status;
			this.createdAt = dto.createdAt;
			this.verifiedAt = dto.verifiedAt;
			this.provisionedAt = dto.provisionedAt;
			this.ipAddress = dto.ipAddress;
			this.userAgent = dto.userAgent;
			this.verificationChannel = dto.verificationChannel;
			this.failureReason = dto.failureReason;
			this.provisionedSecurityUserId = dto.provisionedSecurityUserId;
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
		/// Creates a deep copy clone of a PendingRegistration Object.
		///
		/// </summary>
		public PendingRegistration Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new PendingRegistration{
				id = this.id,
				accountName = this.accountName,
				emailAddress = this.emailAddress,
				displayName = this.displayName,
				passwordHash = this.passwordHash,
				verificationCode = this.verificationCode,
				codeExpiresAt = this.codeExpiresAt,
				verificationAttempts = this.verificationAttempts,
				status = this.status,
				createdAt = this.createdAt,
				verifiedAt = this.verifiedAt,
				provisionedAt = this.provisionedAt,
				ipAddress = this.ipAddress,
				userAgent = this.userAgent,
				verificationChannel = this.verificationChannel,
				failureReason = this.failureReason,
				provisionedSecurityUserId = this.provisionedSecurityUserId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PendingRegistration Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PendingRegistration Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a PendingRegistration Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a PendingRegistration Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.PendingRegistration pendingRegistration)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (pendingRegistration == null)
			{
				return null;
			}

			return new {
				id = pendingRegistration.id,
				accountName = pendingRegistration.accountName,
				emailAddress = pendingRegistration.emailAddress,
				displayName = pendingRegistration.displayName,
				passwordHash = pendingRegistration.passwordHash,
				verificationCode = pendingRegistration.verificationCode,
				codeExpiresAt = pendingRegistration.codeExpiresAt,
				verificationAttempts = pendingRegistration.verificationAttempts,
				status = pendingRegistration.status,
				createdAt = pendingRegistration.createdAt,
				verifiedAt = pendingRegistration.verifiedAt,
				provisionedAt = pendingRegistration.provisionedAt,
				ipAddress = pendingRegistration.ipAddress,
				userAgent = pendingRegistration.userAgent,
				verificationChannel = pendingRegistration.verificationChannel,
				failureReason = pendingRegistration.failureReason,
				provisionedSecurityUserId = pendingRegistration.provisionedSecurityUserId,
				objectGuid = pendingRegistration.objectGuid,
				active = pendingRegistration.active,
				deleted = pendingRegistration.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a PendingRegistration Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(PendingRegistration pendingRegistration)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (pendingRegistration == null)
			{
				return null;
			}

			return new {
				id = pendingRegistration.id,
				accountName = pendingRegistration.accountName,
				emailAddress = pendingRegistration.emailAddress,
				displayName = pendingRegistration.displayName,
				passwordHash = pendingRegistration.passwordHash,
				verificationCode = pendingRegistration.verificationCode,
				codeExpiresAt = pendingRegistration.codeExpiresAt,
				verificationAttempts = pendingRegistration.verificationAttempts,
				status = pendingRegistration.status,
				createdAt = pendingRegistration.createdAt,
				verifiedAt = pendingRegistration.verifiedAt,
				provisionedAt = pendingRegistration.provisionedAt,
				ipAddress = pendingRegistration.ipAddress,
				userAgent = pendingRegistration.userAgent,
				verificationChannel = pendingRegistration.verificationChannel,
				failureReason = pendingRegistration.failureReason,
				provisionedSecurityUserId = pendingRegistration.provisionedSecurityUserId,
				objectGuid = pendingRegistration.objectGuid,
				active = pendingRegistration.active,
				deleted = pendingRegistration.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a PendingRegistration Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(PendingRegistration pendingRegistration)
		{
			//
			// Return a very minimal object.
			//
			if (pendingRegistration == null)
			{
				return null;
			}

			return new {
				id = pendingRegistration.id,
				name = pendingRegistration.accountName,
				description = string.Join(", ", new[] { pendingRegistration.accountName, pendingRegistration.emailAddress, pendingRegistration.displayName}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
