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
	public partial class SecurityUser : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SecurityUserDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String accountName { get; set; }
			[Required]
			public Boolean activeDirectoryAccount { get; set; }
			public String password { get; set; }
			[Required]
			public Boolean canLogin { get; set; }
			[Required]
			public Boolean mustChangePassword { get; set; }
			public String firstName { get; set; }
			public String middleName { get; set; }
			public String lastName { get; set; }
			public DateTime? dateOfBirth { get; set; }
			public String emailAddress { get; set; }
			public String cellPhoneNumber { get; set; }
			public String phoneNumber { get; set; }
			public String phoneExtension { get; set; }
			public String description { get; set; }
			public Int32? securityUserTitleId { get; set; }
			public Int32? reportsToSecurityUserId { get; set; }
			public String authenticationDomain { get; set; }
			public Int32? failedLoginCount { get; set; }
			public DateTime? lastLoginAttempt { get; set; }
			public DateTime? mostRecentActivity { get; set; }
			public String alternateIdentifier { get; set; }
			public Byte[] image { get; set; }
			public String settings { get; set; }
			public Int32? securityTenantId { get; set; }
			[Required]
			public Int32 readPermissionLevel { get; set; }
			[Required]
			public Int32 writePermissionLevel { get; set; }
			public Int32? securityOrganizationId { get; set; }
			public Int32? securityDepartmentId { get; set; }
			public Int32? securityTeamId { get; set; }
			public String authenticationToken { get; set; }
			public DateTime? authenticationTokenExpiry { get; set; }
			public String twoFactorToken { get; set; }
			public DateTime? twoFactorTokenExpiry { get; set; }
			public Boolean? twoFactorSendByEmail { get; set; }
			public Boolean? twoFactorSendBySMS { get; set; }
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
		public class SecurityUserOutputDTO : SecurityUserDTO
		{
			public SecurityUser.SecurityUserDTO reportsToSecurityUser { get; set; }
			public SecurityDepartment.SecurityDepartmentDTO securityDepartment { get; set; }
			public SecurityOrganization.SecurityOrganizationDTO securityOrganization { get; set; }
			public SecurityTeam.SecurityTeamDTO securityTeam { get; set; }
			public SecurityTenant.SecurityTenantDTO securityTenant { get; set; }
			public SecurityUserTitle.SecurityUserTitleDTO securityUserTitle { get; set; }
		}


		/// <summary>
		///
		/// Converts a SecurityUser to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SecurityUserDTO ToDTO()
		{
			return new SecurityUserDTO
			{
				id = this.id,
				accountName = this.accountName,
				activeDirectoryAccount = this.activeDirectoryAccount,
				password = this.password,
				canLogin = this.canLogin,
				mustChangePassword = this.mustChangePassword,
				firstName = this.firstName,
				middleName = this.middleName,
				lastName = this.lastName,
				dateOfBirth = this.dateOfBirth,
				emailAddress = this.emailAddress,
				cellPhoneNumber = this.cellPhoneNumber,
				phoneNumber = this.phoneNumber,
				phoneExtension = this.phoneExtension,
				description = this.description,
				securityUserTitleId = this.securityUserTitleId,
				reportsToSecurityUserId = this.reportsToSecurityUserId,
				authenticationDomain = this.authenticationDomain,
				failedLoginCount = this.failedLoginCount,
				lastLoginAttempt = this.lastLoginAttempt,
				mostRecentActivity = this.mostRecentActivity,
				alternateIdentifier = this.alternateIdentifier,
				image = this.image,
				settings = this.settings,
				securityTenantId = this.securityTenantId,
				readPermissionLevel = this.readPermissionLevel,
				writePermissionLevel = this.writePermissionLevel,
				securityOrganizationId = this.securityOrganizationId,
				securityDepartmentId = this.securityDepartmentId,
				securityTeamId = this.securityTeamId,
				authenticationToken = this.authenticationToken,
				authenticationTokenExpiry = this.authenticationTokenExpiry,
				twoFactorToken = this.twoFactorToken,
				twoFactorTokenExpiry = this.twoFactorTokenExpiry,
				twoFactorSendByEmail = this.twoFactorSendByEmail,
				twoFactorSendBySMS = this.twoFactorSendBySMS,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SecurityUser list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SecurityUserDTO> ToDTOList(List<SecurityUser> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityUserDTO> output = new List<SecurityUserDTO>();

			output.Capacity = data.Count;

			foreach (SecurityUser securityUser in data)
			{
				output.Add(securityUser.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SecurityUser to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SecurityUserEntity type directly.
		///
		/// </summary>
		public SecurityUserOutputDTO ToOutputDTO()
		{
			return new SecurityUserOutputDTO
			{
				id = this.id,
				accountName = this.accountName,
				activeDirectoryAccount = this.activeDirectoryAccount,
				password = this.password,
				canLogin = this.canLogin,
				mustChangePassword = this.mustChangePassword,
				firstName = this.firstName,
				middleName = this.middleName,
				lastName = this.lastName,
				dateOfBirth = this.dateOfBirth,
				emailAddress = this.emailAddress,
				cellPhoneNumber = this.cellPhoneNumber,
				phoneNumber = this.phoneNumber,
				phoneExtension = this.phoneExtension,
				description = this.description,
				securityUserTitleId = this.securityUserTitleId,
				reportsToSecurityUserId = this.reportsToSecurityUserId,
				authenticationDomain = this.authenticationDomain,
				failedLoginCount = this.failedLoginCount,
				lastLoginAttempt = this.lastLoginAttempt,
				mostRecentActivity = this.mostRecentActivity,
				alternateIdentifier = this.alternateIdentifier,
				image = this.image,
				settings = this.settings,
				securityTenantId = this.securityTenantId,
				readPermissionLevel = this.readPermissionLevel,
				writePermissionLevel = this.writePermissionLevel,
				securityOrganizationId = this.securityOrganizationId,
				securityDepartmentId = this.securityDepartmentId,
				securityTeamId = this.securityTeamId,
				authenticationToken = this.authenticationToken,
				authenticationTokenExpiry = this.authenticationTokenExpiry,
				twoFactorToken = this.twoFactorToken,
				twoFactorTokenExpiry = this.twoFactorTokenExpiry,
				twoFactorSendByEmail = this.twoFactorSendByEmail,
				twoFactorSendBySMS = this.twoFactorSendBySMS,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				reportsToSecurityUser = this.reportsToSecurityUser?.ToDTO(),
				securityDepartment = this.securityDepartment?.ToDTO(),
				securityOrganization = this.securityOrganization?.ToDTO(),
				securityTeam = this.securityTeam?.ToDTO(),
				securityTenant = this.securityTenant?.ToDTO(),
				securityUserTitle = this.securityUserTitle?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a SecurityUser list to list of Output Data Transfer Object intended to be used for serializing a list of SecurityUser objects to avoid using the SecurityUser entity type directly.
		///
		/// </summary>
		public static List<SecurityUserOutputDTO> ToOutputDTOList(List<SecurityUser> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityUserOutputDTO> output = new List<SecurityUserOutputDTO>();

			output.Capacity = data.Count;

			foreach (SecurityUser securityUser in data)
			{
				output.Add(securityUser.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SecurityUser Object.
		///
		/// </summary>
		public static Database.SecurityUser FromDTO(SecurityUserDTO dto)
		{
			return new Database.SecurityUser
			{
				id = dto.id,
				accountName = dto.accountName,
				activeDirectoryAccount = dto.activeDirectoryAccount,
				password = dto.password,
				canLogin = dto.canLogin,
				mustChangePassword = dto.mustChangePassword,
				firstName = dto.firstName,
				middleName = dto.middleName,
				lastName = dto.lastName,
				dateOfBirth = dto.dateOfBirth,
				emailAddress = dto.emailAddress,
				cellPhoneNumber = dto.cellPhoneNumber,
				phoneNumber = dto.phoneNumber,
				phoneExtension = dto.phoneExtension,
				description = dto.description,
				securityUserTitleId = dto.securityUserTitleId,
				reportsToSecurityUserId = dto.reportsToSecurityUserId,
				authenticationDomain = dto.authenticationDomain,
				failedLoginCount = dto.failedLoginCount,
				lastLoginAttempt = dto.lastLoginAttempt,
				mostRecentActivity = dto.mostRecentActivity,
				alternateIdentifier = dto.alternateIdentifier,
				image = dto.image,
				settings = dto.settings,
				securityTenantId = dto.securityTenantId,
				readPermissionLevel = dto.readPermissionLevel,
				writePermissionLevel = dto.writePermissionLevel,
				securityOrganizationId = dto.securityOrganizationId,
				securityDepartmentId = dto.securityDepartmentId,
				securityTeamId = dto.securityTeamId,
				authenticationToken = dto.authenticationToken,
				authenticationTokenExpiry = dto.authenticationTokenExpiry,
				twoFactorToken = dto.twoFactorToken,
				twoFactorTokenExpiry = dto.twoFactorTokenExpiry,
				twoFactorSendByEmail = dto.twoFactorSendByEmail,
				twoFactorSendBySMS = dto.twoFactorSendBySMS,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SecurityUser Object.
		///
		/// </summary>
		public void ApplyDTO(SecurityUserDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.accountName = dto.accountName;
			this.activeDirectoryAccount = dto.activeDirectoryAccount;
			this.password = dto.password;
			this.canLogin = dto.canLogin;
			this.mustChangePassword = dto.mustChangePassword;
			this.firstName = dto.firstName;
			this.middleName = dto.middleName;
			this.lastName = dto.lastName;
			this.dateOfBirth = dto.dateOfBirth;
			this.emailAddress = dto.emailAddress;
			this.cellPhoneNumber = dto.cellPhoneNumber;
			this.phoneNumber = dto.phoneNumber;
			this.phoneExtension = dto.phoneExtension;
			this.description = dto.description;
			this.securityUserTitleId = dto.securityUserTitleId;
			this.reportsToSecurityUserId = dto.reportsToSecurityUserId;
			this.authenticationDomain = dto.authenticationDomain;
			this.failedLoginCount = dto.failedLoginCount;
			this.lastLoginAttempt = dto.lastLoginAttempt;
			this.mostRecentActivity = dto.mostRecentActivity;
			this.alternateIdentifier = dto.alternateIdentifier;
			this.image = dto.image;
			this.settings = dto.settings;
			this.securityTenantId = dto.securityTenantId;
			this.readPermissionLevel = dto.readPermissionLevel;
			this.writePermissionLevel = dto.writePermissionLevel;
			this.securityOrganizationId = dto.securityOrganizationId;
			this.securityDepartmentId = dto.securityDepartmentId;
			this.securityTeamId = dto.securityTeamId;
			this.authenticationToken = dto.authenticationToken;
			this.authenticationTokenExpiry = dto.authenticationTokenExpiry;
			this.twoFactorToken = dto.twoFactorToken;
			this.twoFactorTokenExpiry = dto.twoFactorTokenExpiry;
			this.twoFactorSendByEmail = dto.twoFactorSendByEmail;
			this.twoFactorSendBySMS = dto.twoFactorSendBySMS;
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
		/// Creates a deep copy clone of a SecurityUser Object.
		///
		/// </summary>
		public SecurityUser Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SecurityUser{
				id = this.id,
				accountName = this.accountName,
				activeDirectoryAccount = this.activeDirectoryAccount,
				password = this.password,
				canLogin = this.canLogin,
				mustChangePassword = this.mustChangePassword,
				firstName = this.firstName,
				middleName = this.middleName,
				lastName = this.lastName,
				dateOfBirth = this.dateOfBirth,
				emailAddress = this.emailAddress,
				cellPhoneNumber = this.cellPhoneNumber,
				phoneNumber = this.phoneNumber,
				phoneExtension = this.phoneExtension,
				description = this.description,
				securityUserTitleId = this.securityUserTitleId,
				reportsToSecurityUserId = this.reportsToSecurityUserId,
				authenticationDomain = this.authenticationDomain,
				failedLoginCount = this.failedLoginCount,
				lastLoginAttempt = this.lastLoginAttempt,
				mostRecentActivity = this.mostRecentActivity,
				alternateIdentifier = this.alternateIdentifier,
				image = this.image,
				settings = this.settings,
				securityTenantId = this.securityTenantId,
				readPermissionLevel = this.readPermissionLevel,
				writePermissionLevel = this.writePermissionLevel,
				securityOrganizationId = this.securityOrganizationId,
				securityDepartmentId = this.securityDepartmentId,
				securityTeamId = this.securityTeamId,
				authenticationToken = this.authenticationToken,
				authenticationTokenExpiry = this.authenticationTokenExpiry,
				twoFactorToken = this.twoFactorToken,
				twoFactorTokenExpiry = this.twoFactorTokenExpiry,
				twoFactorSendByEmail = this.twoFactorSendByEmail,
				twoFactorSendBySMS = this.twoFactorSendBySMS,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityUser Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityUser Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SecurityUser Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityUser Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SecurityUser securityUser)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (securityUser == null)
			{
				return null;
			}

			return new {
				id = securityUser.id,
				accountName = securityUser.accountName,
				activeDirectoryAccount = securityUser.activeDirectoryAccount,
				password = securityUser.password,
				canLogin = securityUser.canLogin,
				mustChangePassword = securityUser.mustChangePassword,
				firstName = securityUser.firstName,
				middleName = securityUser.middleName,
				lastName = securityUser.lastName,
				dateOfBirth = securityUser.dateOfBirth,
				emailAddress = securityUser.emailAddress,
				cellPhoneNumber = securityUser.cellPhoneNumber,
				phoneNumber = securityUser.phoneNumber,
				phoneExtension = securityUser.phoneExtension,
				description = securityUser.description,
				securityUserTitleId = securityUser.securityUserTitleId,
				reportsToSecurityUserId = securityUser.reportsToSecurityUserId,
				authenticationDomain = securityUser.authenticationDomain,
				failedLoginCount = securityUser.failedLoginCount,
				lastLoginAttempt = securityUser.lastLoginAttempt,
				mostRecentActivity = securityUser.mostRecentActivity,
				alternateIdentifier = securityUser.alternateIdentifier,
				image = securityUser.image,
				settings = securityUser.settings,
				securityTenantId = securityUser.securityTenantId,
				readPermissionLevel = securityUser.readPermissionLevel,
				writePermissionLevel = securityUser.writePermissionLevel,
				securityOrganizationId = securityUser.securityOrganizationId,
				securityDepartmentId = securityUser.securityDepartmentId,
				securityTeamId = securityUser.securityTeamId,
				authenticationToken = securityUser.authenticationToken,
				authenticationTokenExpiry = securityUser.authenticationTokenExpiry,
				twoFactorToken = securityUser.twoFactorToken,
				twoFactorTokenExpiry = securityUser.twoFactorTokenExpiry,
				twoFactorSendByEmail = securityUser.twoFactorSendByEmail,
				twoFactorSendBySMS = securityUser.twoFactorSendBySMS,
				objectGuid = securityUser.objectGuid,
				active = securityUser.active,
				deleted = securityUser.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityUser Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SecurityUser securityUser)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (securityUser == null)
			{
				return null;
			}

			return new {
				id = securityUser.id,
				accountName = securityUser.accountName,
				activeDirectoryAccount = securityUser.activeDirectoryAccount,
				password = securityUser.password,
				canLogin = securityUser.canLogin,
				mustChangePassword = securityUser.mustChangePassword,
				firstName = securityUser.firstName,
				middleName = securityUser.middleName,
				lastName = securityUser.lastName,
				dateOfBirth = securityUser.dateOfBirth,
				emailAddress = securityUser.emailAddress,
				cellPhoneNumber = securityUser.cellPhoneNumber,
				phoneNumber = securityUser.phoneNumber,
				phoneExtension = securityUser.phoneExtension,
				description = securityUser.description,
				securityUserTitleId = securityUser.securityUserTitleId,
				reportsToSecurityUserId = securityUser.reportsToSecurityUserId,
				authenticationDomain = securityUser.authenticationDomain,
				failedLoginCount = securityUser.failedLoginCount,
				lastLoginAttempt = securityUser.lastLoginAttempt,
				mostRecentActivity = securityUser.mostRecentActivity,
				alternateIdentifier = securityUser.alternateIdentifier,
				image = securityUser.image,
				settings = securityUser.settings,
				securityTenantId = securityUser.securityTenantId,
				readPermissionLevel = securityUser.readPermissionLevel,
				writePermissionLevel = securityUser.writePermissionLevel,
				securityOrganizationId = securityUser.securityOrganizationId,
				securityDepartmentId = securityUser.securityDepartmentId,
				securityTeamId = securityUser.securityTeamId,
				authenticationToken = securityUser.authenticationToken,
				authenticationTokenExpiry = securityUser.authenticationTokenExpiry,
				twoFactorToken = securityUser.twoFactorToken,
				twoFactorTokenExpiry = securityUser.twoFactorTokenExpiry,
				twoFactorSendByEmail = securityUser.twoFactorSendByEmail,
				twoFactorSendBySMS = securityUser.twoFactorSendBySMS,
				objectGuid = securityUser.objectGuid,
				active = securityUser.active,
				deleted = securityUser.deleted,
				reportsToSecurityUser = SecurityUser.CreateMinimalAnonymous(securityUser.reportsToSecurityUser),
				securityDepartment = SecurityDepartment.CreateMinimalAnonymous(securityUser.securityDepartment),
				securityOrganization = SecurityOrganization.CreateMinimalAnonymous(securityUser.securityOrganization),
				securityTeam = SecurityTeam.CreateMinimalAnonymous(securityUser.securityTeam),
				securityTenant = SecurityTenant.CreateMinimalAnonymous(securityUser.securityTenant),
				securityUserTitle = SecurityUserTitle.CreateMinimalAnonymous(securityUser.securityUserTitle)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SecurityUser Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SecurityUser securityUser)
		{
			//
			// Return a very minimal object.
			//
			if (securityUser == null)
			{
				return null;
			}

			return new {
				id = securityUser.id,
				description = securityUser.description,
				name = string.Join(", ", new[] { securityUser.accountName}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
