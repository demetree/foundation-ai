using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.DeepSpace.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class StorageObjectVersion : IAnonymousConvertible
	{


		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class StorageObjectVersionDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 storageObjectId { get; set; }
			public Int32 versionNumber { get; set; }
			[Required]
			public Int32 storageProviderId { get; set; }
			public String providerKey { get; set; }
			[Required]
			public Int32 sizeBytes { get; set; }
			public String md5Hash { get; set; }
			public Guid? createdByUserGuid { get; set; }
			[Required]
			public DateTime createdUtc { get; set; }
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
		public class StorageObjectVersionOutputDTO : StorageObjectVersionDTO
		{
			public StorageObject.StorageObjectDTO storageObject { get; set; }
			public StorageProvider.StorageProviderDTO storageProvider { get; set; }
		}


		/// <summary>
		///
		/// Converts a StorageObjectVersion to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public StorageObjectVersionDTO ToDTO()
		{
			return new StorageObjectVersionDTO
			{
				id = this.id,
				storageObjectId = this.storageObjectId,
				versionNumber = this.versionNumber,
				storageProviderId = this.storageProviderId,
				providerKey = this.providerKey,
				sizeBytes = this.sizeBytes,
				md5Hash = this.md5Hash,
				createdByUserGuid = this.createdByUserGuid,
				createdUtc = this.createdUtc,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a StorageObjectVersion list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<StorageObjectVersionDTO> ToDTOList(List<StorageObjectVersion> data)
		{
			if (data == null)
			{
				return null;
			}

			List<StorageObjectVersionDTO> output = new List<StorageObjectVersionDTO>();

			output.Capacity = data.Count;

			foreach (StorageObjectVersion storageObjectVersion in data)
			{
				output.Add(storageObjectVersion.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a StorageObjectVersion to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the StorageObjectVersion Entity type directly.
		///
		/// </summary>
		public StorageObjectVersionOutputDTO ToOutputDTO()
		{
			return new StorageObjectVersionOutputDTO
			{
				id = this.id,
				storageObjectId = this.storageObjectId,
				versionNumber = this.versionNumber,
				storageProviderId = this.storageProviderId,
				providerKey = this.providerKey,
				sizeBytes = this.sizeBytes,
				md5Hash = this.md5Hash,
				createdByUserGuid = this.createdByUserGuid,
				createdUtc = this.createdUtc,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				storageObject = this.storageObject?.ToDTO(),
				storageProvider = this.storageProvider?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a StorageObjectVersion list to list of Output Data Transfer Object intended to be used for serializing a list of StorageObjectVersion objects to avoid using the StorageObjectVersion entity type directly.
		///
		/// </summary>
		public static List<StorageObjectVersionOutputDTO> ToOutputDTOList(List<StorageObjectVersion> data)
		{
			if (data == null)
			{
				return null;
			}

			List<StorageObjectVersionOutputDTO> output = new List<StorageObjectVersionOutputDTO>();

			output.Capacity = data.Count;

			foreach (StorageObjectVersion storageObjectVersion in data)
			{
				output.Add(storageObjectVersion.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a StorageObjectVersion Object.
		///
		/// </summary>
		public static Database.StorageObjectVersion FromDTO(StorageObjectVersionDTO dto)
		{
			return new Database.StorageObjectVersion
			{
				id = dto.id,
				storageObjectId = dto.storageObjectId,
				versionNumber = dto.versionNumber,
				storageProviderId = dto.storageProviderId,
				providerKey = dto.providerKey,
				sizeBytes = dto.sizeBytes,
				md5Hash = dto.md5Hash,
				createdByUserGuid = dto.createdByUserGuid,
				createdUtc = dto.createdUtc,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a StorageObjectVersion Object.
		///
		/// </summary>
		public void ApplyDTO(StorageObjectVersionDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.storageObjectId = dto.storageObjectId;
			this.versionNumber = dto.versionNumber;
			this.storageProviderId = dto.storageProviderId;
			this.providerKey = dto.providerKey;
			this.sizeBytes = dto.sizeBytes;
			this.md5Hash = dto.md5Hash;
			this.createdByUserGuid = dto.createdByUserGuid;
			this.createdUtc = dto.createdUtc;
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
		/// Creates a deep copy clone of a StorageObjectVersion Object.
		///
		/// </summary>
		public StorageObjectVersion Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new StorageObjectVersion{
				id = this.id,
				storageObjectId = this.storageObjectId,
				versionNumber = this.versionNumber,
				storageProviderId = this.storageProviderId,
				providerKey = this.providerKey,
				sizeBytes = this.sizeBytes,
				md5Hash = this.md5Hash,
				createdByUserGuid = this.createdByUserGuid,
				createdUtc = this.createdUtc,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a StorageObjectVersion Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a StorageObjectVersion Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a StorageObjectVersion Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a StorageObjectVersion Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.StorageObjectVersion storageObjectVersion)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (storageObjectVersion == null)
			{
				return null;
			}

			return new {
				id = storageObjectVersion.id,
				storageObjectId = storageObjectVersion.storageObjectId,
				versionNumber = storageObjectVersion.versionNumber,
				storageProviderId = storageObjectVersion.storageProviderId,
				providerKey = storageObjectVersion.providerKey,
				sizeBytes = storageObjectVersion.sizeBytes,
				md5Hash = storageObjectVersion.md5Hash,
				createdByUserGuid = storageObjectVersion.createdByUserGuid,
				createdUtc = storageObjectVersion.createdUtc,
				objectGuid = storageObjectVersion.objectGuid,
				active = storageObjectVersion.active,
				deleted = storageObjectVersion.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a StorageObjectVersion Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(StorageObjectVersion storageObjectVersion)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (storageObjectVersion == null)
			{
				return null;
			}

			return new {
				id = storageObjectVersion.id,
				storageObjectId = storageObjectVersion.storageObjectId,
				versionNumber = storageObjectVersion.versionNumber,
				storageProviderId = storageObjectVersion.storageProviderId,
				providerKey = storageObjectVersion.providerKey,
				sizeBytes = storageObjectVersion.sizeBytes,
				md5Hash = storageObjectVersion.md5Hash,
				createdByUserGuid = storageObjectVersion.createdByUserGuid,
				createdUtc = storageObjectVersion.createdUtc,
				objectGuid = storageObjectVersion.objectGuid,
				active = storageObjectVersion.active,
				deleted = storageObjectVersion.deleted,
				storageObject = StorageObject.CreateMinimalAnonymous(storageObjectVersion.storageObject),
				storageProvider = StorageProvider.CreateMinimalAnonymous(storageObjectVersion.storageProvider)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a StorageObjectVersion Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(StorageObjectVersion storageObjectVersion)
		{
			//
			// Return a very minimal object.
			//
			if (storageObjectVersion == null)
			{
				return null;
			}

			return new {
				id = storageObjectVersion.id,
				name = storageObjectVersion.providerKey,
				description = string.Join(", ", new[] { storageObjectVersion.providerKey, storageObjectVersion.md5Hash}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
