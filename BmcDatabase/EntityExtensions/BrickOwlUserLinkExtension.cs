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
	public partial class BrickOwlUserLink : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BrickOwlUserLinkDTO
		{
			public Int32 id { get; set; }
			public String encryptedApiKey { get; set; }
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
		public class BrickOwlUserLinkOutputDTO : BrickOwlUserLinkDTO
		{
		}


		/// <summary>
		///
		/// Converts a BrickOwlUserLink to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BrickOwlUserLinkDTO ToDTO()
		{
			return new BrickOwlUserLinkDTO
			{
				id = this.id,
				encryptedApiKey = this.encryptedApiKey,
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
		/// Converts a BrickOwlUserLink list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BrickOwlUserLinkDTO> ToDTOList(List<BrickOwlUserLink> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickOwlUserLinkDTO> output = new List<BrickOwlUserLinkDTO>();

			output.Capacity = data.Count;

			foreach (BrickOwlUserLink brickOwlUserLink in data)
			{
				output.Add(brickOwlUserLink.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BrickOwlUserLink to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BrickOwlUserLinkEntity type directly.
		///
		/// </summary>
		public BrickOwlUserLinkOutputDTO ToOutputDTO()
		{
			return new BrickOwlUserLinkOutputDTO
			{
				id = this.id,
				encryptedApiKey = this.encryptedApiKey,
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
		/// Converts a BrickOwlUserLink list to list of Output Data Transfer Object intended to be used for serializing a list of BrickOwlUserLink objects to avoid using the BrickOwlUserLink entity type directly.
		///
		/// </summary>
		public static List<BrickOwlUserLinkOutputDTO> ToOutputDTOList(List<BrickOwlUserLink> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickOwlUserLinkOutputDTO> output = new List<BrickOwlUserLinkOutputDTO>();

			output.Capacity = data.Count;

			foreach (BrickOwlUserLink brickOwlUserLink in data)
			{
				output.Add(brickOwlUserLink.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BrickOwlUserLink Object.
		///
		/// </summary>
		public static Database.BrickOwlUserLink FromDTO(BrickOwlUserLinkDTO dto)
		{
			return new Database.BrickOwlUserLink
			{
				id = dto.id,
				encryptedApiKey = dto.encryptedApiKey,
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
		/// Applies the values from an INPUT DTO to a BrickOwlUserLink Object.
		///
		/// </summary>
		public void ApplyDTO(BrickOwlUserLinkDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.encryptedApiKey = dto.encryptedApiKey;
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
		/// Creates a deep copy clone of a BrickOwlUserLink Object.
		///
		/// </summary>
		public BrickOwlUserLink Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BrickOwlUserLink{
				id = this.id,
				tenantGuid = this.tenantGuid,
				encryptedApiKey = this.encryptedApiKey,
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
        /// Creates an anonymous object containing properties from a BrickOwlUserLink Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickOwlUserLink Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BrickOwlUserLink Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BrickOwlUserLink Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BrickOwlUserLink brickOwlUserLink)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (brickOwlUserLink == null)
			{
				return null;
			}

			return new {
				id = brickOwlUserLink.id,
				encryptedApiKey = brickOwlUserLink.encryptedApiKey,
				syncEnabled = brickOwlUserLink.syncEnabled,
				syncDirection = brickOwlUserLink.syncDirection,
				lastSyncDate = brickOwlUserLink.lastSyncDate,
				lastPullDate = brickOwlUserLink.lastPullDate,
				lastPushDate = brickOwlUserLink.lastPushDate,
				lastSyncError = brickOwlUserLink.lastSyncError,
				objectGuid = brickOwlUserLink.objectGuid,
				active = brickOwlUserLink.active,
				deleted = brickOwlUserLink.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BrickOwlUserLink Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BrickOwlUserLink brickOwlUserLink)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (brickOwlUserLink == null)
			{
				return null;
			}

			return new {
				id = brickOwlUserLink.id,
				encryptedApiKey = brickOwlUserLink.encryptedApiKey,
				syncEnabled = brickOwlUserLink.syncEnabled,
				syncDirection = brickOwlUserLink.syncDirection,
				lastSyncDate = brickOwlUserLink.lastSyncDate,
				lastPullDate = brickOwlUserLink.lastPullDate,
				lastPushDate = brickOwlUserLink.lastPushDate,
				lastSyncError = brickOwlUserLink.lastSyncError,
				objectGuid = brickOwlUserLink.objectGuid,
				active = brickOwlUserLink.active,
				deleted = brickOwlUserLink.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BrickOwlUserLink Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BrickOwlUserLink brickOwlUserLink)
		{
			//
			// Return a very minimal object.
			//
			if (brickOwlUserLink == null)
			{
				return null;
			}

			return new {
				id = brickOwlUserLink.id,
				name = brickOwlUserLink.encryptedApiKey,
				description = string.Join(", ", new[] { brickOwlUserLink.encryptedApiKey, brickOwlUserLink.syncDirection, brickOwlUserLink.lastSyncError}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
