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
	public partial class RebrickableSyncQueue : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class RebrickableSyncQueueDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String operationType { get; set; }
			[Required]
			public String entityType { get; set; }
			[Required]
			public Int64 entityId { get; set; }
			public String payload { get; set; }
			[Required]
			public String status { get; set; }
			public DateTime? createdDate { get; set; }
			public DateTime? lastAttemptDate { get; set; }
			public DateTime? completedDate { get; set; }
			[Required]
			public Int32 attemptCount { get; set; }
			[Required]
			public Int32 maxAttempts { get; set; }
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
		public class RebrickableSyncQueueOutputDTO : RebrickableSyncQueueDTO
		{
		}


		/// <summary>
		///
		/// Converts a RebrickableSyncQueue to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public RebrickableSyncQueueDTO ToDTO()
		{
			return new RebrickableSyncQueueDTO
			{
				id = this.id,
				operationType = this.operationType,
				entityType = this.entityType,
				entityId = this.entityId,
				payload = this.payload,
				status = this.status,
				createdDate = this.createdDate,
				lastAttemptDate = this.lastAttemptDate,
				completedDate = this.completedDate,
				attemptCount = this.attemptCount,
				maxAttempts = this.maxAttempts,
				errorMessage = this.errorMessage,
				responseBody = this.responseBody,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a RebrickableSyncQueue list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<RebrickableSyncQueueDTO> ToDTOList(List<RebrickableSyncQueue> data)
		{
			if (data == null)
			{
				return null;
			}

			List<RebrickableSyncQueueDTO> output = new List<RebrickableSyncQueueDTO>();

			output.Capacity = data.Count;

			foreach (RebrickableSyncQueue rebrickableSyncQueue in data)
			{
				output.Add(rebrickableSyncQueue.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a RebrickableSyncQueue to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the RebrickableSyncQueueEntity type directly.
		///
		/// </summary>
		public RebrickableSyncQueueOutputDTO ToOutputDTO()
		{
			return new RebrickableSyncQueueOutputDTO
			{
				id = this.id,
				operationType = this.operationType,
				entityType = this.entityType,
				entityId = this.entityId,
				payload = this.payload,
				status = this.status,
				createdDate = this.createdDate,
				lastAttemptDate = this.lastAttemptDate,
				completedDate = this.completedDate,
				attemptCount = this.attemptCount,
				maxAttempts = this.maxAttempts,
				errorMessage = this.errorMessage,
				responseBody = this.responseBody,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a RebrickableSyncQueue list to list of Output Data Transfer Object intended to be used for serializing a list of RebrickableSyncQueue objects to avoid using the RebrickableSyncQueue entity type directly.
		///
		/// </summary>
		public static List<RebrickableSyncQueueOutputDTO> ToOutputDTOList(List<RebrickableSyncQueue> data)
		{
			if (data == null)
			{
				return null;
			}

			List<RebrickableSyncQueueOutputDTO> output = new List<RebrickableSyncQueueOutputDTO>();

			output.Capacity = data.Count;

			foreach (RebrickableSyncQueue rebrickableSyncQueue in data)
			{
				output.Add(rebrickableSyncQueue.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a RebrickableSyncQueue Object.
		///
		/// </summary>
		public static Database.RebrickableSyncQueue FromDTO(RebrickableSyncQueueDTO dto)
		{
			return new Database.RebrickableSyncQueue
			{
				id = dto.id,
				operationType = dto.operationType,
				entityType = dto.entityType,
				entityId = dto.entityId,
				payload = dto.payload,
				status = dto.status,
				createdDate = dto.createdDate,
				lastAttemptDate = dto.lastAttemptDate,
				completedDate = dto.completedDate,
				attemptCount = dto.attemptCount,
				maxAttempts = dto.maxAttempts,
				errorMessage = dto.errorMessage,
				responseBody = dto.responseBody,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a RebrickableSyncQueue Object.
		///
		/// </summary>
		public void ApplyDTO(RebrickableSyncQueueDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.operationType = dto.operationType;
			this.entityType = dto.entityType;
			this.entityId = dto.entityId;
			this.payload = dto.payload;
			this.status = dto.status;
			this.createdDate = dto.createdDate;
			this.lastAttemptDate = dto.lastAttemptDate;
			this.completedDate = dto.completedDate;
			this.attemptCount = dto.attemptCount;
			this.maxAttempts = dto.maxAttempts;
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
		/// Creates a deep copy clone of a RebrickableSyncQueue Object.
		///
		/// </summary>
		public RebrickableSyncQueue Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new RebrickableSyncQueue{
				id = this.id,
				tenantGuid = this.tenantGuid,
				operationType = this.operationType,
				entityType = this.entityType,
				entityId = this.entityId,
				payload = this.payload,
				status = this.status,
				createdDate = this.createdDate,
				lastAttemptDate = this.lastAttemptDate,
				completedDate = this.completedDate,
				attemptCount = this.attemptCount,
				maxAttempts = this.maxAttempts,
				errorMessage = this.errorMessage,
				responseBody = this.responseBody,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a RebrickableSyncQueue Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a RebrickableSyncQueue Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a RebrickableSyncQueue Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a RebrickableSyncQueue Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.RebrickableSyncQueue rebrickableSyncQueue)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (rebrickableSyncQueue == null)
			{
				return null;
			}

			return new {
				id = rebrickableSyncQueue.id,
				operationType = rebrickableSyncQueue.operationType,
				entityType = rebrickableSyncQueue.entityType,
				entityId = rebrickableSyncQueue.entityId,
				payload = rebrickableSyncQueue.payload,
				status = rebrickableSyncQueue.status,
				createdDate = rebrickableSyncQueue.createdDate,
				lastAttemptDate = rebrickableSyncQueue.lastAttemptDate,
				completedDate = rebrickableSyncQueue.completedDate,
				attemptCount = rebrickableSyncQueue.attemptCount,
				maxAttempts = rebrickableSyncQueue.maxAttempts,
				errorMessage = rebrickableSyncQueue.errorMessage,
				responseBody = rebrickableSyncQueue.responseBody,
				objectGuid = rebrickableSyncQueue.objectGuid,
				active = rebrickableSyncQueue.active,
				deleted = rebrickableSyncQueue.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a RebrickableSyncQueue Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(RebrickableSyncQueue rebrickableSyncQueue)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (rebrickableSyncQueue == null)
			{
				return null;
			}

			return new {
				id = rebrickableSyncQueue.id,
				operationType = rebrickableSyncQueue.operationType,
				entityType = rebrickableSyncQueue.entityType,
				entityId = rebrickableSyncQueue.entityId,
				payload = rebrickableSyncQueue.payload,
				status = rebrickableSyncQueue.status,
				createdDate = rebrickableSyncQueue.createdDate,
				lastAttemptDate = rebrickableSyncQueue.lastAttemptDate,
				completedDate = rebrickableSyncQueue.completedDate,
				attemptCount = rebrickableSyncQueue.attemptCount,
				maxAttempts = rebrickableSyncQueue.maxAttempts,
				errorMessage = rebrickableSyncQueue.errorMessage,
				responseBody = rebrickableSyncQueue.responseBody,
				objectGuid = rebrickableSyncQueue.objectGuid,
				active = rebrickableSyncQueue.active,
				deleted = rebrickableSyncQueue.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a RebrickableSyncQueue Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(RebrickableSyncQueue rebrickableSyncQueue)
		{
			//
			// Return a very minimal object.
			//
			if (rebrickableSyncQueue == null)
			{
				return null;
			}

			return new {
				id = rebrickableSyncQueue.id,
				name = rebrickableSyncQueue.operationType,
				description = string.Join(", ", new[] { rebrickableSyncQueue.operationType, rebrickableSyncQueue.entityType, rebrickableSyncQueue.status}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
