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
	public partial class BrickSetUserLink : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BrickSetUserLinkDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String brickSetUsername { get; set; }
			[Required]
			public String encryptedUserHash { get; set; }
			public String encryptedPassword { get; set; }
			[Required]
			public Boolean syncEnabled { get; set; }
			[Required]
			public String syncDirection { get; set; }
			public DateTime? lastSyncDate { get; set; }
			public DateTime? lastPullDate { get; set; }
			public DateTime? lastPushDate { get; set; }
			public String lastSyncError { get; set; }
			public DateTime? userHashStoredDate { get; set; }
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
		public class BrickSetUserLinkOutputDTO : BrickSetUserLinkDTO
		{
		}


		/// <summary>
		///
		/// Converts a BrickSetUserLink to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BrickSetUserLinkDTO ToDTO()
		{
			return new BrickSetUserLinkDTO
			{
				id = this.id,
				brickSetUsername = this.brickSetUsername,
				encryptedUserHash = this.encryptedUserHash,
				encryptedPassword = this.encryptedPassword,
				syncEnabled = this.syncEnabled,
				syncDirection = this.syncDirection,
				lastSyncDate = this.lastSyncDate,
				lastPullDate = this.lastPullDate,
				lastPushDate = this.lastPushDate,
				lastSyncError = this.lastSyncError,
				userHashStoredDate = this.userHashStoredDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BrickSetUserLink list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BrickSetUserLinkDTO> ToDTOList(List<BrickSetUserLink> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickSetUserLinkDTO> output = new List<BrickSetUserLinkDTO>();

			output.Capacity = data.Count;

			foreach (BrickSetUserLink brickSetUserLink in data)
			{
				output.Add(brickSetUserLink.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BrickSetUserLink to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BrickSetUserLinkEntity type directly.
		///
		/// </summary>
		public BrickSetUserLinkOutputDTO ToOutputDTO()
		{
			return new BrickSetUserLinkOutputDTO
			{
				id = this.id,
				brickSetUsername = this.brickSetUsername,
				encryptedUserHash = this.encryptedUserHash,
				encryptedPassword = this.encryptedPassword,
				syncEnabled = this.syncEnabled,
				syncDirection = this.syncDirection,
				lastSyncDate = this.lastSyncDate,
				lastPullDate = this.lastPullDate,
				lastPushDate = this.lastPushDate,
				lastSyncError = this.lastSyncError,
				userHashStoredDate = this.userHashStoredDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BrickSetUserLink list to list of Output Data Transfer Object intended to be used for serializing a list of BrickSetUserLink objects to avoid using the BrickSetUserLink entity type directly.
		///
		/// </summary>
		public static List<BrickSetUserLinkOutputDTO> ToOutputDTOList(List<BrickSetUserLink> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickSetUserLinkOutputDTO> output = new List<BrickSetUserLinkOutputDTO>();

			output.Capacity = data.Count;

			foreach (BrickSetUserLink brickSetUserLink in data)
			{
				output.Add(brickSetUserLink.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BrickSetUserLink Object.
		///
		/// </summary>
		public static Database.BrickSetUserLink FromDTO(BrickSetUserLinkDTO dto)
		{
			return new Database.BrickSetUserLink
			{
				id = dto.id,
				brickSetUsername = dto.brickSetUsername,
				encryptedUserHash = dto.encryptedUserHash,
				encryptedPassword = dto.encryptedPassword,
				syncEnabled = dto.syncEnabled,
				syncDirection = dto.syncDirection,
				lastSyncDate = dto.lastSyncDate,
				lastPullDate = dto.lastPullDate,
				lastPushDate = dto.lastPushDate,
				lastSyncError = dto.lastSyncError,
				userHashStoredDate = dto.userHashStoredDate,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BrickSetUserLink Object.
		///
		/// </summary>
		public void ApplyDTO(BrickSetUserLinkDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.brickSetUsername = dto.brickSetUsername;
			this.encryptedUserHash = dto.encryptedUserHash;
			this.encryptedPassword = dto.encryptedPassword;
			this.syncEnabled = dto.syncEnabled;
			this.syncDirection = dto.syncDirection;
			this.lastSyncDate = dto.lastSyncDate;
			this.lastPullDate = dto.lastPullDate;
			this.lastPushDate = dto.lastPushDate;
			this.lastSyncError = dto.lastSyncError;
			this.userHashStoredDate = dto.userHashStoredDate;
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
		/// Creates a deep copy clone of a BrickSetUserLink Object.
		///
		/// </summary>
		public BrickSetUserLink Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BrickSetUserLink{
				id = this.id,
				tenantGuid = this.tenantGuid,
				brickSetUsername = this.brickSetUsername,
				encryptedUserHash = this.encryptedUserHash,
				encryptedPassword = this.encryptedPassword,
				syncEnabled = this.syncEnabled,
				syncDirection = this.syncDirection,
				lastSyncDate = this.lastSyncDate,
				lastPullDate = this.lastPullDate,
				lastPushDate = this.lastPushDate,
				lastSyncError = this.lastSyncError,
				userHashStoredDate = this.userHashStoredDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickSetUserLink Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickSetUserLink Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BrickSetUserLink Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BrickSetUserLink Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BrickSetUserLink brickSetUserLink)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (brickSetUserLink == null)
			{
				return null;
			}

			return new {
				id = brickSetUserLink.id,
				brickSetUsername = brickSetUserLink.brickSetUsername,
				encryptedUserHash = brickSetUserLink.encryptedUserHash,
				encryptedPassword = brickSetUserLink.encryptedPassword,
				syncEnabled = brickSetUserLink.syncEnabled,
				syncDirection = brickSetUserLink.syncDirection,
				lastSyncDate = brickSetUserLink.lastSyncDate,
				lastPullDate = brickSetUserLink.lastPullDate,
				lastPushDate = brickSetUserLink.lastPushDate,
				lastSyncError = brickSetUserLink.lastSyncError,
				userHashStoredDate = brickSetUserLink.userHashStoredDate,
				objectGuid = brickSetUserLink.objectGuid,
				active = brickSetUserLink.active,
				deleted = brickSetUserLink.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BrickSetUserLink Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BrickSetUserLink brickSetUserLink)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (brickSetUserLink == null)
			{
				return null;
			}

			return new {
				id = brickSetUserLink.id,
				brickSetUsername = brickSetUserLink.brickSetUsername,
				encryptedUserHash = brickSetUserLink.encryptedUserHash,
				encryptedPassword = brickSetUserLink.encryptedPassword,
				syncEnabled = brickSetUserLink.syncEnabled,
				syncDirection = brickSetUserLink.syncDirection,
				lastSyncDate = brickSetUserLink.lastSyncDate,
				lastPullDate = brickSetUserLink.lastPullDate,
				lastPushDate = brickSetUserLink.lastPushDate,
				lastSyncError = brickSetUserLink.lastSyncError,
				userHashStoredDate = brickSetUserLink.userHashStoredDate,
				objectGuid = brickSetUserLink.objectGuid,
				active = brickSetUserLink.active,
				deleted = brickSetUserLink.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BrickSetUserLink Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BrickSetUserLink brickSetUserLink)
		{
			//
			// Return a very minimal object.
			//
			if (brickSetUserLink == null)
			{
				return null;
			}

			return new {
				id = brickSetUserLink.id,
				name = brickSetUserLink.brickSetUsername,
				description = string.Join(", ", new[] { brickSetUserLink.brickSetUsername, brickSetUserLink.encryptedUserHash, brickSetUserLink.encryptedPassword}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
