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
	public partial class BrickLinkUserLink : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BrickLinkUserLinkDTO
		{
			public Int32 id { get; set; }
			public String encryptedTokenValue { get; set; }
			public String encryptedTokenSecret { get; set; }
			[Required]
			public Boolean syncEnabled { get; set; }
			public String syncDirection { get; set; }
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
		public class BrickLinkUserLinkOutputDTO : BrickLinkUserLinkDTO
		{
		}


		/// <summary>
		///
		/// Converts a BrickLinkUserLink to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BrickLinkUserLinkDTO ToDTO()
		{
			return new BrickLinkUserLinkDTO
			{
				id = this.id,
				encryptedTokenValue = this.encryptedTokenValue,
				encryptedTokenSecret = this.encryptedTokenSecret,
				syncEnabled = this.syncEnabled,
				syncDirection = this.syncDirection,
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
		/// Converts a BrickLinkUserLink list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BrickLinkUserLinkDTO> ToDTOList(List<BrickLinkUserLink> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickLinkUserLinkDTO> output = new List<BrickLinkUserLinkDTO>();

			output.Capacity = data.Count;

			foreach (BrickLinkUserLink brickLinkUserLink in data)
			{
				output.Add(brickLinkUserLink.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BrickLinkUserLink to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BrickLinkUserLinkEntity type directly.
		///
		/// </summary>
		public BrickLinkUserLinkOutputDTO ToOutputDTO()
		{
			return new BrickLinkUserLinkOutputDTO
			{
				id = this.id,
				encryptedTokenValue = this.encryptedTokenValue,
				encryptedTokenSecret = this.encryptedTokenSecret,
				syncEnabled = this.syncEnabled,
				syncDirection = this.syncDirection,
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
		/// Converts a BrickLinkUserLink list to list of Output Data Transfer Object intended to be used for serializing a list of BrickLinkUserLink objects to avoid using the BrickLinkUserLink entity type directly.
		///
		/// </summary>
		public static List<BrickLinkUserLinkOutputDTO> ToOutputDTOList(List<BrickLinkUserLink> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickLinkUserLinkOutputDTO> output = new List<BrickLinkUserLinkOutputDTO>();

			output.Capacity = data.Count;

			foreach (BrickLinkUserLink brickLinkUserLink in data)
			{
				output.Add(brickLinkUserLink.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BrickLinkUserLink Object.
		///
		/// </summary>
		public static Database.BrickLinkUserLink FromDTO(BrickLinkUserLinkDTO dto)
		{
			return new Database.BrickLinkUserLink
			{
				id = dto.id,
				encryptedTokenValue = dto.encryptedTokenValue,
				encryptedTokenSecret = dto.encryptedTokenSecret,
				syncEnabled = dto.syncEnabled,
				syncDirection = dto.syncDirection,
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
		/// Applies the values from an INPUT DTO to a BrickLinkUserLink Object.
		///
		/// </summary>
		public void ApplyDTO(BrickLinkUserLinkDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.encryptedTokenValue = dto.encryptedTokenValue;
			this.encryptedTokenSecret = dto.encryptedTokenSecret;
			this.syncEnabled = dto.syncEnabled;
			this.syncDirection = dto.syncDirection;
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
		/// Creates a deep copy clone of a BrickLinkUserLink Object.
		///
		/// </summary>
		public BrickLinkUserLink Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BrickLinkUserLink{
				id = this.id,
				tenantGuid = this.tenantGuid,
				encryptedTokenValue = this.encryptedTokenValue,
				encryptedTokenSecret = this.encryptedTokenSecret,
				syncEnabled = this.syncEnabled,
				syncDirection = this.syncDirection,
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
        /// Creates an anonymous object containing properties from a BrickLinkUserLink Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickLinkUserLink Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BrickLinkUserLink Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BrickLinkUserLink Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BrickLinkUserLink brickLinkUserLink)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (brickLinkUserLink == null)
			{
				return null;
			}

			return new {
				id = brickLinkUserLink.id,
				encryptedTokenValue = brickLinkUserLink.encryptedTokenValue,
				encryptedTokenSecret = brickLinkUserLink.encryptedTokenSecret,
				syncEnabled = brickLinkUserLink.syncEnabled,
				syncDirection = brickLinkUserLink.syncDirection,
				lastSyncDate = brickLinkUserLink.lastSyncDate,
				lastPullDate = brickLinkUserLink.lastPullDate,
				lastPushDate = brickLinkUserLink.lastPushDate,
				lastSyncError = brickLinkUserLink.lastSyncError,
				objectGuid = brickLinkUserLink.objectGuid,
				active = brickLinkUserLink.active,
				deleted = brickLinkUserLink.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BrickLinkUserLink Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BrickLinkUserLink brickLinkUserLink)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (brickLinkUserLink == null)
			{
				return null;
			}

			return new {
				id = brickLinkUserLink.id,
				encryptedTokenValue = brickLinkUserLink.encryptedTokenValue,
				encryptedTokenSecret = brickLinkUserLink.encryptedTokenSecret,
				syncEnabled = brickLinkUserLink.syncEnabled,
				syncDirection = brickLinkUserLink.syncDirection,
				lastSyncDate = brickLinkUserLink.lastSyncDate,
				lastPullDate = brickLinkUserLink.lastPullDate,
				lastPushDate = brickLinkUserLink.lastPushDate,
				lastSyncError = brickLinkUserLink.lastSyncError,
				objectGuid = brickLinkUserLink.objectGuid,
				active = brickLinkUserLink.active,
				deleted = brickLinkUserLink.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BrickLinkUserLink Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BrickLinkUserLink brickLinkUserLink)
		{
			//
			// Return a very minimal object.
			//
			if (brickLinkUserLink == null)
			{
				return null;
			}

			return new {
				id = brickLinkUserLink.id,
				name = brickLinkUserLink.encryptedTokenValue,
				description = string.Join(", ", new[] { brickLinkUserLink.encryptedTokenValue, brickLinkUserLink.encryptedTokenSecret, brickLinkUserLink.syncDirection}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
