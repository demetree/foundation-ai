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
	public partial class RebrickableUserLink : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class RebrickableUserLinkDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String rebrickableUsername { get; set; }
			[Required]
			public String encryptedApiToken { get; set; }
			[Required]
			public String authMode { get; set; }
			public String encryptedPassword { get; set; }
			[Required]
			public Boolean syncEnabled { get; set; }
			[Required]
			public String syncDirectionFlags { get; set; }
			public Int32? pullIntervalMinutes { get; set; }
			public DateTime? lastSyncDate { get; set; }
			public DateTime? lastPullDate { get; set; }
			public DateTime? lastPushDate { get; set; }
			public String lastSyncError { get; set; }
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
		public class RebrickableUserLinkOutputDTO : RebrickableUserLinkDTO
		{
		}


		/// <summary>
		///
		/// Converts a RebrickableUserLink to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public RebrickableUserLinkDTO ToDTO()
		{
			return new RebrickableUserLinkDTO
			{
				id = this.id,
				rebrickableUsername = this.rebrickableUsername,
				encryptedApiToken = this.encryptedApiToken,
				authMode = this.authMode,
				encryptedPassword = this.encryptedPassword,
				syncEnabled = this.syncEnabled,
				syncDirectionFlags = this.syncDirectionFlags,
				pullIntervalMinutes = this.pullIntervalMinutes,
				lastSyncDate = this.lastSyncDate,
				lastPullDate = this.lastPullDate,
				lastPushDate = this.lastPushDate,
				lastSyncError = this.lastSyncError,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a RebrickableUserLink list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<RebrickableUserLinkDTO> ToDTOList(List<RebrickableUserLink> data)
		{
			if (data == null)
			{
				return null;
			}

			List<RebrickableUserLinkDTO> output = new List<RebrickableUserLinkDTO>();

			output.Capacity = data.Count;

			foreach (RebrickableUserLink rebrickableUserLink in data)
			{
				output.Add(rebrickableUserLink.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a RebrickableUserLink to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the RebrickableUserLinkEntity type directly.
		///
		/// </summary>
		public RebrickableUserLinkOutputDTO ToOutputDTO()
		{
			return new RebrickableUserLinkOutputDTO
			{
				id = this.id,
				rebrickableUsername = this.rebrickableUsername,
				encryptedApiToken = this.encryptedApiToken,
				authMode = this.authMode,
				encryptedPassword = this.encryptedPassword,
				syncEnabled = this.syncEnabled,
				syncDirectionFlags = this.syncDirectionFlags,
				pullIntervalMinutes = this.pullIntervalMinutes,
				lastSyncDate = this.lastSyncDate,
				lastPullDate = this.lastPullDate,
				lastPushDate = this.lastPushDate,
				lastSyncError = this.lastSyncError,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a RebrickableUserLink list to list of Output Data Transfer Object intended to be used for serializing a list of RebrickableUserLink objects to avoid using the RebrickableUserLink entity type directly.
		///
		/// </summary>
		public static List<RebrickableUserLinkOutputDTO> ToOutputDTOList(List<RebrickableUserLink> data)
		{
			if (data == null)
			{
				return null;
			}

			List<RebrickableUserLinkOutputDTO> output = new List<RebrickableUserLinkOutputDTO>();

			output.Capacity = data.Count;

			foreach (RebrickableUserLink rebrickableUserLink in data)
			{
				output.Add(rebrickableUserLink.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a RebrickableUserLink Object.
		///
		/// </summary>
		public static Database.RebrickableUserLink FromDTO(RebrickableUserLinkDTO dto)
		{
			return new Database.RebrickableUserLink
			{
				id = dto.id,
				rebrickableUsername = dto.rebrickableUsername,
				encryptedApiToken = dto.encryptedApiToken,
				authMode = dto.authMode,
				encryptedPassword = dto.encryptedPassword,
				syncEnabled = dto.syncEnabled,
				syncDirectionFlags = dto.syncDirectionFlags,
				pullIntervalMinutes = dto.pullIntervalMinutes,
				lastSyncDate = dto.lastSyncDate,
				lastPullDate = dto.lastPullDate,
				lastPushDate = dto.lastPushDate,
				lastSyncError = dto.lastSyncError,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a RebrickableUserLink Object.
		///
		/// </summary>
		public void ApplyDTO(RebrickableUserLinkDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.rebrickableUsername = dto.rebrickableUsername;
			this.encryptedApiToken = dto.encryptedApiToken;
			this.authMode = dto.authMode;
			this.encryptedPassword = dto.encryptedPassword;
			this.syncEnabled = dto.syncEnabled;
			this.syncDirectionFlags = dto.syncDirectionFlags;
			this.pullIntervalMinutes = dto.pullIntervalMinutes;
			this.lastSyncDate = dto.lastSyncDate;
			this.lastPullDate = dto.lastPullDate;
			this.lastPushDate = dto.lastPushDate;
			this.lastSyncError = dto.lastSyncError;
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
		/// Creates a deep copy clone of a RebrickableUserLink Object.
		///
		/// </summary>
		public RebrickableUserLink Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new RebrickableUserLink{
				id = this.id,
				tenantGuid = this.tenantGuid,
				rebrickableUsername = this.rebrickableUsername,
				encryptedApiToken = this.encryptedApiToken,
				authMode = this.authMode,
				encryptedPassword = this.encryptedPassword,
				syncEnabled = this.syncEnabled,
				syncDirectionFlags = this.syncDirectionFlags,
				pullIntervalMinutes = this.pullIntervalMinutes,
				lastSyncDate = this.lastSyncDate,
				lastPullDate = this.lastPullDate,
				lastPushDate = this.lastPushDate,
				lastSyncError = this.lastSyncError,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a RebrickableUserLink Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a RebrickableUserLink Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a RebrickableUserLink Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a RebrickableUserLink Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.RebrickableUserLink rebrickableUserLink)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (rebrickableUserLink == null)
			{
				return null;
			}

			return new {
				id = rebrickableUserLink.id,
				rebrickableUsername = rebrickableUserLink.rebrickableUsername,
				encryptedApiToken = rebrickableUserLink.encryptedApiToken,
				authMode = rebrickableUserLink.authMode,
				encryptedPassword = rebrickableUserLink.encryptedPassword,
				syncEnabled = rebrickableUserLink.syncEnabled,
				syncDirectionFlags = rebrickableUserLink.syncDirectionFlags,
				pullIntervalMinutes = rebrickableUserLink.pullIntervalMinutes,
				lastSyncDate = rebrickableUserLink.lastSyncDate,
				lastPullDate = rebrickableUserLink.lastPullDate,
				lastPushDate = rebrickableUserLink.lastPushDate,
				lastSyncError = rebrickableUserLink.lastSyncError,
				objectGuid = rebrickableUserLink.objectGuid,
				active = rebrickableUserLink.active,
				deleted = rebrickableUserLink.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a RebrickableUserLink Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(RebrickableUserLink rebrickableUserLink)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (rebrickableUserLink == null)
			{
				return null;
			}

			return new {
				id = rebrickableUserLink.id,
				rebrickableUsername = rebrickableUserLink.rebrickableUsername,
				encryptedApiToken = rebrickableUserLink.encryptedApiToken,
				authMode = rebrickableUserLink.authMode,
				encryptedPassword = rebrickableUserLink.encryptedPassword,
				syncEnabled = rebrickableUserLink.syncEnabled,
				syncDirectionFlags = rebrickableUserLink.syncDirectionFlags,
				pullIntervalMinutes = rebrickableUserLink.pullIntervalMinutes,
				lastSyncDate = rebrickableUserLink.lastSyncDate,
				lastPullDate = rebrickableUserLink.lastPullDate,
				lastPushDate = rebrickableUserLink.lastPushDate,
				lastSyncError = rebrickableUserLink.lastSyncError,
				objectGuid = rebrickableUserLink.objectGuid,
				active = rebrickableUserLink.active,
				deleted = rebrickableUserLink.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a RebrickableUserLink Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(RebrickableUserLink rebrickableUserLink)
		{
			//
			// Return a very minimal object.
			//
			if (rebrickableUserLink == null)
			{
				return null;
			}

			return new {
				id = rebrickableUserLink.id,
				name = rebrickableUserLink.rebrickableUsername,
				description = string.Join(", ", new[] { rebrickableUserLink.rebrickableUsername, rebrickableUserLink.encryptedApiToken, rebrickableUserLink.authMode}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
