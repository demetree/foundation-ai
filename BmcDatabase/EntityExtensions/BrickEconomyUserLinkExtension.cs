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
	public partial class BrickEconomyUserLink : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BrickEconomyUserLinkDTO
		{
			public Int32 id { get; set; }
			public String encryptedApiKey { get; set; }
			[Required]
			public Boolean syncEnabled { get; set; }
			public DateTime? lastSyncDate { get; set; }
			public String lastSyncError { get; set; }
			public Int32? dailyQuotaUsed { get; set; }
			public DateTime? quotaResetDate { get; set; }
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
		public class BrickEconomyUserLinkOutputDTO : BrickEconomyUserLinkDTO
		{
		}


		/// <summary>
		///
		/// Converts a BrickEconomyUserLink to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BrickEconomyUserLinkDTO ToDTO()
		{
			return new BrickEconomyUserLinkDTO
			{
				id = this.id,
				encryptedApiKey = this.encryptedApiKey,
				syncEnabled = this.syncEnabled,
				lastSyncDate = this.lastSyncDate,
				lastSyncError = this.lastSyncError,
				dailyQuotaUsed = this.dailyQuotaUsed,
				quotaResetDate = this.quotaResetDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BrickEconomyUserLink list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BrickEconomyUserLinkDTO> ToDTOList(List<BrickEconomyUserLink> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickEconomyUserLinkDTO> output = new List<BrickEconomyUserLinkDTO>();

			output.Capacity = data.Count;

			foreach (BrickEconomyUserLink brickEconomyUserLink in data)
			{
				output.Add(brickEconomyUserLink.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BrickEconomyUserLink to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BrickEconomyUserLinkEntity type directly.
		///
		/// </summary>
		public BrickEconomyUserLinkOutputDTO ToOutputDTO()
		{
			return new BrickEconomyUserLinkOutputDTO
			{
				id = this.id,
				encryptedApiKey = this.encryptedApiKey,
				syncEnabled = this.syncEnabled,
				lastSyncDate = this.lastSyncDate,
				lastSyncError = this.lastSyncError,
				dailyQuotaUsed = this.dailyQuotaUsed,
				quotaResetDate = this.quotaResetDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BrickEconomyUserLink list to list of Output Data Transfer Object intended to be used for serializing a list of BrickEconomyUserLink objects to avoid using the BrickEconomyUserLink entity type directly.
		///
		/// </summary>
		public static List<BrickEconomyUserLinkOutputDTO> ToOutputDTOList(List<BrickEconomyUserLink> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickEconomyUserLinkOutputDTO> output = new List<BrickEconomyUserLinkOutputDTO>();

			output.Capacity = data.Count;

			foreach (BrickEconomyUserLink brickEconomyUserLink in data)
			{
				output.Add(brickEconomyUserLink.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BrickEconomyUserLink Object.
		///
		/// </summary>
		public static Database.BrickEconomyUserLink FromDTO(BrickEconomyUserLinkDTO dto)
		{
			return new Database.BrickEconomyUserLink
			{
				id = dto.id,
				encryptedApiKey = dto.encryptedApiKey,
				syncEnabled = dto.syncEnabled,
				lastSyncDate = dto.lastSyncDate,
				lastSyncError = dto.lastSyncError,
				dailyQuotaUsed = dto.dailyQuotaUsed,
				quotaResetDate = dto.quotaResetDate,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BrickEconomyUserLink Object.
		///
		/// </summary>
		public void ApplyDTO(BrickEconomyUserLinkDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.encryptedApiKey = dto.encryptedApiKey;
			this.syncEnabled = dto.syncEnabled;
			this.lastSyncDate = dto.lastSyncDate;
			this.lastSyncError = dto.lastSyncError;
			this.dailyQuotaUsed = dto.dailyQuotaUsed;
			this.quotaResetDate = dto.quotaResetDate;
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
		/// Creates a deep copy clone of a BrickEconomyUserLink Object.
		///
		/// </summary>
		public BrickEconomyUserLink Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BrickEconomyUserLink{
				id = this.id,
				tenantGuid = this.tenantGuid,
				encryptedApiKey = this.encryptedApiKey,
				syncEnabled = this.syncEnabled,
				lastSyncDate = this.lastSyncDate,
				lastSyncError = this.lastSyncError,
				dailyQuotaUsed = this.dailyQuotaUsed,
				quotaResetDate = this.quotaResetDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickEconomyUserLink Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickEconomyUserLink Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BrickEconomyUserLink Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BrickEconomyUserLink Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BrickEconomyUserLink brickEconomyUserLink)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (brickEconomyUserLink == null)
			{
				return null;
			}

			return new {
				id = brickEconomyUserLink.id,
				encryptedApiKey = brickEconomyUserLink.encryptedApiKey,
				syncEnabled = brickEconomyUserLink.syncEnabled,
				lastSyncDate = brickEconomyUserLink.lastSyncDate,
				lastSyncError = brickEconomyUserLink.lastSyncError,
				dailyQuotaUsed = brickEconomyUserLink.dailyQuotaUsed,
				quotaResetDate = brickEconomyUserLink.quotaResetDate,
				objectGuid = brickEconomyUserLink.objectGuid,
				active = brickEconomyUserLink.active,
				deleted = brickEconomyUserLink.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BrickEconomyUserLink Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BrickEconomyUserLink brickEconomyUserLink)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (brickEconomyUserLink == null)
			{
				return null;
			}

			return new {
				id = brickEconomyUserLink.id,
				encryptedApiKey = brickEconomyUserLink.encryptedApiKey,
				syncEnabled = brickEconomyUserLink.syncEnabled,
				lastSyncDate = brickEconomyUserLink.lastSyncDate,
				lastSyncError = brickEconomyUserLink.lastSyncError,
				dailyQuotaUsed = brickEconomyUserLink.dailyQuotaUsed,
				quotaResetDate = brickEconomyUserLink.quotaResetDate,
				objectGuid = brickEconomyUserLink.objectGuid,
				active = brickEconomyUserLink.active,
				deleted = brickEconomyUserLink.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BrickEconomyUserLink Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BrickEconomyUserLink brickEconomyUserLink)
		{
			//
			// Return a very minimal object.
			//
			if (brickEconomyUserLink == null)
			{
				return null;
			}

			return new {
				id = brickEconomyUserLink.id,
				name = brickEconomyUserLink.encryptedApiKey,
				description = string.Join(", ", new[] { brickEconomyUserLink.encryptedApiKey, brickEconomyUserLink.lastSyncError}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
