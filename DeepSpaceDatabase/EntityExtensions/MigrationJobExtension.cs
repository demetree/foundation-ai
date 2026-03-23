using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.DeepSpace.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class MigrationJob : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class MigrationJobDTO
		{
			public Int32 id { get; set; }
			public Int32? lifecycleRuleId { get; set; }
			[Required]
			public Int32 storageObjectId { get; set; }
			[Required]
			public Int32 sourceStorageProviderId { get; set; }
			[Required]
			public Int32 targetStorageProviderId { get; set; }
			[Required]
			public Int32 migrationJobStatusId { get; set; }
			public DateTime? startedUtc { get; set; }
			public DateTime? completedUtc { get; set; }
			public Int32? bytesTransferred { get; set; }
			public String errorMessage { get; set; }
			[Required]
			public Int32 retryCount { get; set; }
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
		public class MigrationJobOutputDTO : MigrationJobDTO
		{
			public LifecycleRule.LifecycleRuleDTO lifecycleRule { get; set; }
			public MigrationJobStatu.MigrationJobStatuDTO migrationJobStatus { get; set; }
			public StorageProvider.StorageProviderDTO sourceStorageProvider { get; set; }
			public StorageObject.StorageObjectDTO storageObject { get; set; }
			public StorageProvider.StorageProviderDTO targetStorageProvider { get; set; }
		}


		/// <summary>
		///
		/// Converts a MigrationJob to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public MigrationJobDTO ToDTO()
		{
			return new MigrationJobDTO
			{
				id = this.id,
				lifecycleRuleId = this.lifecycleRuleId,
				storageObjectId = this.storageObjectId,
				sourceStorageProviderId = this.sourceStorageProviderId,
				targetStorageProviderId = this.targetStorageProviderId,
				migrationJobStatusId = this.migrationJobStatusId,
				startedUtc = this.startedUtc,
				completedUtc = this.completedUtc,
				bytesTransferred = this.bytesTransferred,
				errorMessage = this.errorMessage,
				retryCount = this.retryCount,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a MigrationJob list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<MigrationJobDTO> ToDTOList(List<MigrationJob> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MigrationJobDTO> output = new List<MigrationJobDTO>();

			output.Capacity = data.Count;

			foreach (MigrationJob migrationJob in data)
			{
				output.Add(migrationJob.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a MigrationJob to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the MigrationJob Entity type directly.
		///
		/// </summary>
		public MigrationJobOutputDTO ToOutputDTO()
		{
			return new MigrationJobOutputDTO
			{
				id = this.id,
				lifecycleRuleId = this.lifecycleRuleId,
				storageObjectId = this.storageObjectId,
				sourceStorageProviderId = this.sourceStorageProviderId,
				targetStorageProviderId = this.targetStorageProviderId,
				migrationJobStatusId = this.migrationJobStatusId,
				startedUtc = this.startedUtc,
				completedUtc = this.completedUtc,
				bytesTransferred = this.bytesTransferred,
				errorMessage = this.errorMessage,
				retryCount = this.retryCount,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				lifecycleRule = this.lifecycleRule?.ToDTO(),
				migrationJobStatus = this.migrationJobStatus?.ToDTO(),
				sourceStorageProvider = this.sourceStorageProvider?.ToDTO(),
				storageObject = this.storageObject?.ToDTO(),
				targetStorageProvider = this.targetStorageProvider?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a MigrationJob list to list of Output Data Transfer Object intended to be used for serializing a list of MigrationJob objects to avoid using the MigrationJob entity type directly.
		///
		/// </summary>
		public static List<MigrationJobOutputDTO> ToOutputDTOList(List<MigrationJob> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MigrationJobOutputDTO> output = new List<MigrationJobOutputDTO>();

			output.Capacity = data.Count;

			foreach (MigrationJob migrationJob in data)
			{
				output.Add(migrationJob.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a MigrationJob Object.
		///
		/// </summary>
		public static Database.MigrationJob FromDTO(MigrationJobDTO dto)
		{
			return new Database.MigrationJob
			{
				id = dto.id,
				lifecycleRuleId = dto.lifecycleRuleId,
				storageObjectId = dto.storageObjectId,
				sourceStorageProviderId = dto.sourceStorageProviderId,
				targetStorageProviderId = dto.targetStorageProviderId,
				migrationJobStatusId = dto.migrationJobStatusId,
				startedUtc = dto.startedUtc,
				completedUtc = dto.completedUtc,
				bytesTransferred = dto.bytesTransferred,
				errorMessage = dto.errorMessage,
				retryCount = dto.retryCount,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a MigrationJob Object.
		///
		/// </summary>
		public void ApplyDTO(MigrationJobDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.lifecycleRuleId = dto.lifecycleRuleId;
			this.storageObjectId = dto.storageObjectId;
			this.sourceStorageProviderId = dto.sourceStorageProviderId;
			this.targetStorageProviderId = dto.targetStorageProviderId;
			this.migrationJobStatusId = dto.migrationJobStatusId;
			this.startedUtc = dto.startedUtc;
			this.completedUtc = dto.completedUtc;
			this.bytesTransferred = dto.bytesTransferred;
			this.errorMessage = dto.errorMessage;
			this.retryCount = dto.retryCount;
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
		/// Creates a deep copy clone of a MigrationJob Object.
		///
		/// </summary>
		public MigrationJob Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new MigrationJob{
				id = this.id,
				lifecycleRuleId = this.lifecycleRuleId,
				storageObjectId = this.storageObjectId,
				sourceStorageProviderId = this.sourceStorageProviderId,
				targetStorageProviderId = this.targetStorageProviderId,
				migrationJobStatusId = this.migrationJobStatusId,
				startedUtc = this.startedUtc,
				completedUtc = this.completedUtc,
				bytesTransferred = this.bytesTransferred,
				errorMessage = this.errorMessage,
				retryCount = this.retryCount,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MigrationJob Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MigrationJob Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a MigrationJob Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a MigrationJob Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.MigrationJob migrationJob)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (migrationJob == null)
			{
				return null;
			}

			return new {
				id = migrationJob.id,
				lifecycleRuleId = migrationJob.lifecycleRuleId,
				storageObjectId = migrationJob.storageObjectId,
				sourceStorageProviderId = migrationJob.sourceStorageProviderId,
				targetStorageProviderId = migrationJob.targetStorageProviderId,
				migrationJobStatusId = migrationJob.migrationJobStatusId,
				startedUtc = migrationJob.startedUtc,
				completedUtc = migrationJob.completedUtc,
				bytesTransferred = migrationJob.bytesTransferred,
				errorMessage = migrationJob.errorMessage,
				retryCount = migrationJob.retryCount,
				objectGuid = migrationJob.objectGuid,
				active = migrationJob.active,
				deleted = migrationJob.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a MigrationJob Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(MigrationJob migrationJob)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (migrationJob == null)
			{
				return null;
			}

			return new {
				id = migrationJob.id,
				lifecycleRuleId = migrationJob.lifecycleRuleId,
				storageObjectId = migrationJob.storageObjectId,
				sourceStorageProviderId = migrationJob.sourceStorageProviderId,
				targetStorageProviderId = migrationJob.targetStorageProviderId,
				migrationJobStatusId = migrationJob.migrationJobStatusId,
				startedUtc = migrationJob.startedUtc,
				completedUtc = migrationJob.completedUtc,
				bytesTransferred = migrationJob.bytesTransferred,
				errorMessage = migrationJob.errorMessage,
				retryCount = migrationJob.retryCount,
				objectGuid = migrationJob.objectGuid,
				active = migrationJob.active,
				deleted = migrationJob.deleted,
				lifecycleRule = LifecycleRule.CreateMinimalAnonymous(migrationJob.lifecycleRule),
				migrationJobStatus = MigrationJobStatu.CreateMinimalAnonymous(migrationJob.migrationJobStatus),
				sourceStorageProvider = StorageProvider.CreateMinimalAnonymous(migrationJob.sourceStorageProvider),
				storageObject = StorageObject.CreateMinimalAnonymous(migrationJob.storageObject),
				targetStorageProvider = StorageProvider.CreateMinimalAnonymous(migrationJob.targetStorageProvider)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a MigrationJob Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(MigrationJob migrationJob)
		{
			//
			// Return a very minimal object.
			//
			if (migrationJob == null)
			{
				return null;
			}

			return new {
				id = migrationJob.id,
				name = migrationJob.id,
				description = migrationJob.id
			 };
		}
	}
}
