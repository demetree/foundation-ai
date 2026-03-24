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
	public partial class CallEventLog : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class CallEventLogDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 callId { get; set; }
			[Required]
			public String eventType { get; set; }
			public Int32? userId { get; set; }
			public String providerId { get; set; }
			public String metadata { get; set; }
			[Required]
			public DateTime dateTimeCreated { get; set; }
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
		public class CallEventLogOutputDTO : CallEventLogDTO
		{
			public Call.CallDTO call { get; set; }
		}


		/// <summary>
		///
		/// Converts a CallEventLog to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public CallEventLogDTO ToDTO()
		{
			return new CallEventLogDTO
			{
				id = this.id,
				callId = this.callId,
				eventType = this.eventType,
				userId = this.userId,
				providerId = this.providerId,
				metadata = this.metadata,
				dateTimeCreated = this.dateTimeCreated,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a CallEventLog list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<CallEventLogDTO> ToDTOList(List<CallEventLog> data)
		{
			if (data == null)
			{
				return null;
			}

			List<CallEventLogDTO> output = new List<CallEventLogDTO>();

			output.Capacity = data.Count;

			foreach (CallEventLog callEventLog in data)
			{
				output.Add(callEventLog.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a CallEventLog to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the CallEventLog Entity type directly.
		///
		/// </summary>
		public CallEventLogOutputDTO ToOutputDTO()
		{
			return new CallEventLogOutputDTO
			{
				id = this.id,
				callId = this.callId,
				eventType = this.eventType,
				userId = this.userId,
				providerId = this.providerId,
				metadata = this.metadata,
				dateTimeCreated = this.dateTimeCreated,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				call = this.call?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a CallEventLog list to list of Output Data Transfer Object intended to be used for serializing a list of CallEventLog objects to avoid using the CallEventLog entity type directly.
		///
		/// </summary>
		public static List<CallEventLogOutputDTO> ToOutputDTOList(List<CallEventLog> data)
		{
			if (data == null)
			{
				return null;
			}

			List<CallEventLogOutputDTO> output = new List<CallEventLogOutputDTO>();

			output.Capacity = data.Count;

			foreach (CallEventLog callEventLog in data)
			{
				output.Add(callEventLog.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a CallEventLog Object.
		///
		/// </summary>
		public static Database.CallEventLog FromDTO(CallEventLogDTO dto)
		{
			return new Database.CallEventLog
			{
				id = dto.id,
				callId = dto.callId,
				eventType = dto.eventType,
				userId = dto.userId,
				providerId = dto.providerId,
				metadata = dto.metadata,
				dateTimeCreated = dto.dateTimeCreated,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a CallEventLog Object.
		///
		/// </summary>
		public void ApplyDTO(CallEventLogDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.callId = dto.callId;
			this.eventType = dto.eventType;
			this.userId = dto.userId;
			this.providerId = dto.providerId;
			this.metadata = dto.metadata;
			this.dateTimeCreated = dto.dateTimeCreated;
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
		/// Creates a deep copy clone of a CallEventLog Object.
		///
		/// </summary>
		public CallEventLog Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new CallEventLog{
				id = this.id,
				tenantGuid = this.tenantGuid,
				callId = this.callId,
				eventType = this.eventType,
				userId = this.userId,
				providerId = this.providerId,
				metadata = this.metadata,
				dateTimeCreated = this.dateTimeCreated,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a CallEventLog Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a CallEventLog Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a CallEventLog Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a CallEventLog Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.CallEventLog callEventLog)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (callEventLog == null)
			{
				return null;
			}

			return new {
				id = callEventLog.id,
				callId = callEventLog.callId,
				eventType = callEventLog.eventType,
				userId = callEventLog.userId,
				providerId = callEventLog.providerId,
				metadata = callEventLog.metadata,
				dateTimeCreated = callEventLog.dateTimeCreated,
				objectGuid = callEventLog.objectGuid,
				active = callEventLog.active,
				deleted = callEventLog.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a CallEventLog Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(CallEventLog callEventLog)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (callEventLog == null)
			{
				return null;
			}

			return new {
				id = callEventLog.id,
				callId = callEventLog.callId,
				eventType = callEventLog.eventType,
				userId = callEventLog.userId,
				providerId = callEventLog.providerId,
				metadata = callEventLog.metadata,
				dateTimeCreated = callEventLog.dateTimeCreated,
				objectGuid = callEventLog.objectGuid,
				active = callEventLog.active,
				deleted = callEventLog.deleted,
				call = Call.CreateMinimalAnonymous(callEventLog.call)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a CallEventLog Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(CallEventLog callEventLog)
		{
			//
			// Return a very minimal object.
			//
			if (callEventLog == null)
			{
				return null;
			}

			return new {
				id = callEventLog.id,
				name = callEventLog.eventType,
				description = string.Join(", ", new[] { callEventLog.eventType, callEventLog.providerId}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
