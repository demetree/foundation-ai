using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Scheduler.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class SalesforceSyncQueue : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SalesforceSyncQueueDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String entityType { get; set; }
			[Required]
			public String operationType { get; set; }
			[Required]
			public Int32 entityId { get; set; }
			public String payload { get; set; }
			[Required]
			public String status { get; set; }
			[Required]
			public Int32 attemptCount { get; set; }
			[Required]
			public Int32 maxAttempts { get; set; }
			public DateTime? lastAttemptDate { get; set; }
			public DateTime? completedDate { get; set; }
			public DateTime? createdDate { get; set; }
			public String errorMessage { get; set; }
			public String responseBody { get; set; }
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
		public class SalesforceSyncQueueOutputDTO : SalesforceSyncQueueDTO
		{
		}


		/// <summary>
		///
		/// Converts a SalesforceSyncQueue to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SalesforceSyncQueueDTO ToDTO()
		{
			return new SalesforceSyncQueueDTO
			{
				id = this.id,
				entityType = this.entityType,
				operationType = this.operationType,
				entityId = this.entityId,
				payload = this.payload,
				status = this.status,
				attemptCount = this.attemptCount,
				maxAttempts = this.maxAttempts,
				lastAttemptDate = this.lastAttemptDate,
				completedDate = this.completedDate,
				createdDate = this.createdDate,
				errorMessage = this.errorMessage,
				responseBody = this.responseBody,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SalesforceSyncQueue list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SalesforceSyncQueueDTO> ToDTOList(List<SalesforceSyncQueue> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SalesforceSyncQueueDTO> output = new List<SalesforceSyncQueueDTO>();

			output.Capacity = data.Count;

			foreach (SalesforceSyncQueue salesforceSyncQueue in data)
			{
				output.Add(salesforceSyncQueue.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SalesforceSyncQueue to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SalesforceSyncQueue Entity type directly.
		///
		/// </summary>
		public SalesforceSyncQueueOutputDTO ToOutputDTO()
		{
			return new SalesforceSyncQueueOutputDTO
			{
				id = this.id,
				entityType = this.entityType,
				operationType = this.operationType,
				entityId = this.entityId,
				payload = this.payload,
				status = this.status,
				attemptCount = this.attemptCount,
				maxAttempts = this.maxAttempts,
				lastAttemptDate = this.lastAttemptDate,
				completedDate = this.completedDate,
				createdDate = this.createdDate,
				errorMessage = this.errorMessage,
				responseBody = this.responseBody,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SalesforceSyncQueue list to list of Output Data Transfer Object intended to be used for serializing a list of SalesforceSyncQueue objects to avoid using the SalesforceSyncQueue entity type directly.
		///
		/// </summary>
		public static List<SalesforceSyncQueueOutputDTO> ToOutputDTOList(List<SalesforceSyncQueue> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SalesforceSyncQueueOutputDTO> output = new List<SalesforceSyncQueueOutputDTO>();

			output.Capacity = data.Count;

			foreach (SalesforceSyncQueue salesforceSyncQueue in data)
			{
				output.Add(salesforceSyncQueue.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SalesforceSyncQueue Object.
		///
		/// </summary>
		public static Database.SalesforceSyncQueue FromDTO(SalesforceSyncQueueDTO dto)
		{
			return new Database.SalesforceSyncQueue
			{
				id = dto.id,
				entityType = dto.entityType,
				operationType = dto.operationType,
				entityId = dto.entityId,
				payload = dto.payload,
				status = dto.status,
				attemptCount = dto.attemptCount,
				maxAttempts = dto.maxAttempts,
				lastAttemptDate = dto.lastAttemptDate,
				completedDate = dto.completedDate,
				createdDate = dto.createdDate,
				errorMessage = dto.errorMessage,
				responseBody = dto.responseBody,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SalesforceSyncQueue Object.
		///
		/// </summary>
		public void ApplyDTO(SalesforceSyncQueueDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.entityType = dto.entityType;
			this.operationType = dto.operationType;
			this.entityId = dto.entityId;
			this.payload = dto.payload;
			this.status = dto.status;
			this.attemptCount = dto.attemptCount;
			this.maxAttempts = dto.maxAttempts;
			this.lastAttemptDate = dto.lastAttemptDate;
			this.completedDate = dto.completedDate;
			this.createdDate = dto.createdDate;
			this.errorMessage = dto.errorMessage;
			this.responseBody = dto.responseBody;
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
		/// Creates a deep copy clone of a SalesforceSyncQueue Object.
		///
		/// </summary>
		public SalesforceSyncQueue Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SalesforceSyncQueue{
				id = this.id,
				tenantGuid = this.tenantGuid,
				entityType = this.entityType,
				operationType = this.operationType,
				entityId = this.entityId,
				payload = this.payload,
				status = this.status,
				attemptCount = this.attemptCount,
				maxAttempts = this.maxAttempts,
				lastAttemptDate = this.lastAttemptDate,
				completedDate = this.completedDate,
				createdDate = this.createdDate,
				errorMessage = this.errorMessage,
				responseBody = this.responseBody,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SalesforceSyncQueue Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SalesforceSyncQueue Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SalesforceSyncQueue Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SalesforceSyncQueue Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SalesforceSyncQueue salesforceSyncQueue)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (salesforceSyncQueue == null)
			{
				return null;
			}

			return new {
				id = salesforceSyncQueue.id,
				entityType = salesforceSyncQueue.entityType,
				operationType = salesforceSyncQueue.operationType,
				entityId = salesforceSyncQueue.entityId,
				payload = salesforceSyncQueue.payload,
				status = salesforceSyncQueue.status,
				attemptCount = salesforceSyncQueue.attemptCount,
				maxAttempts = salesforceSyncQueue.maxAttempts,
				lastAttemptDate = salesforceSyncQueue.lastAttemptDate,
				completedDate = salesforceSyncQueue.completedDate,
				createdDate = salesforceSyncQueue.createdDate,
				errorMessage = salesforceSyncQueue.errorMessage,
				responseBody = salesforceSyncQueue.responseBody,
				objectGuid = salesforceSyncQueue.objectGuid,
				active = salesforceSyncQueue.active,
				deleted = salesforceSyncQueue.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SalesforceSyncQueue Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SalesforceSyncQueue salesforceSyncQueue)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (salesforceSyncQueue == null)
			{
				return null;
			}

			return new {
				id = salesforceSyncQueue.id,
				entityType = salesforceSyncQueue.entityType,
				operationType = salesforceSyncQueue.operationType,
				entityId = salesforceSyncQueue.entityId,
				payload = salesforceSyncQueue.payload,
				status = salesforceSyncQueue.status,
				attemptCount = salesforceSyncQueue.attemptCount,
				maxAttempts = salesforceSyncQueue.maxAttempts,
				lastAttemptDate = salesforceSyncQueue.lastAttemptDate,
				completedDate = salesforceSyncQueue.completedDate,
				createdDate = salesforceSyncQueue.createdDate,
				errorMessage = salesforceSyncQueue.errorMessage,
				responseBody = salesforceSyncQueue.responseBody,
				objectGuid = salesforceSyncQueue.objectGuid,
				active = salesforceSyncQueue.active,
				deleted = salesforceSyncQueue.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SalesforceSyncQueue Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SalesforceSyncQueue salesforceSyncQueue)
		{
			//
			// Return a very minimal object.
			//
			if (salesforceSyncQueue == null)
			{
				return null;
			}

			return new {
				id = salesforceSyncQueue.id,
				name = salesforceSyncQueue.entityType,
				description = string.Join(", ", new[] { salesforceSyncQueue.entityType, salesforceSyncQueue.operationType, salesforceSyncQueue.status}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
