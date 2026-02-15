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
	public partial class ApiRequestLog : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ApiRequestLogDTO
		{
			public Int64 id { get; set; }
			[Required]
			public Int32 apiKeyId { get; set; }
			[Required]
			public String endpoint { get; set; }
			[Required]
			public String httpMethod { get; set; }
			[Required]
			public Int32 responseStatus { get; set; }
			[Required]
			public DateTime requestDate { get; set; }
			public Int32? durationMs { get; set; }
			public String clientIpAddress { get; set; }
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
		public class ApiRequestLogOutputDTO : ApiRequestLogDTO
		{
			public ApiKey.ApiKeyDTO apiKey { get; set; }
		}


		/// <summary>
		///
		/// Converts a ApiRequestLog to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ApiRequestLogDTO ToDTO()
		{
			return new ApiRequestLogDTO
			{
				id = this.id,
				apiKeyId = this.apiKeyId,
				endpoint = this.endpoint,
				httpMethod = this.httpMethod,
				responseStatus = this.responseStatus,
				requestDate = this.requestDate,
				durationMs = this.durationMs,
				clientIpAddress = this.clientIpAddress,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ApiRequestLog list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ApiRequestLogDTO> ToDTOList(List<ApiRequestLog> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ApiRequestLogDTO> output = new List<ApiRequestLogDTO>();

			output.Capacity = data.Count;

			foreach (ApiRequestLog apiRequestLog in data)
			{
				output.Add(apiRequestLog.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ApiRequestLog to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ApiRequestLogEntity type directly.
		///
		/// </summary>
		public ApiRequestLogOutputDTO ToOutputDTO()
		{
			return new ApiRequestLogOutputDTO
			{
				id = this.id,
				apiKeyId = this.apiKeyId,
				endpoint = this.endpoint,
				httpMethod = this.httpMethod,
				responseStatus = this.responseStatus,
				requestDate = this.requestDate,
				durationMs = this.durationMs,
				clientIpAddress = this.clientIpAddress,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				apiKey = this.apiKey?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ApiRequestLog list to list of Output Data Transfer Object intended to be used for serializing a list of ApiRequestLog objects to avoid using the ApiRequestLog entity type directly.
		///
		/// </summary>
		public static List<ApiRequestLogOutputDTO> ToOutputDTOList(List<ApiRequestLog> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ApiRequestLogOutputDTO> output = new List<ApiRequestLogOutputDTO>();

			output.Capacity = data.Count;

			foreach (ApiRequestLog apiRequestLog in data)
			{
				output.Add(apiRequestLog.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ApiRequestLog Object.
		///
		/// </summary>
		public static Database.ApiRequestLog FromDTO(ApiRequestLogDTO dto)
		{
			return new Database.ApiRequestLog
			{
				id = dto.id,
				apiKeyId = dto.apiKeyId,
				endpoint = dto.endpoint,
				httpMethod = dto.httpMethod,
				responseStatus = dto.responseStatus,
				requestDate = dto.requestDate,
				durationMs = dto.durationMs,
				clientIpAddress = dto.clientIpAddress,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ApiRequestLog Object.
		///
		/// </summary>
		public void ApplyDTO(ApiRequestLogDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.apiKeyId = dto.apiKeyId;
			this.endpoint = dto.endpoint;
			this.httpMethod = dto.httpMethod;
			this.responseStatus = dto.responseStatus;
			this.requestDate = dto.requestDate;
			this.durationMs = dto.durationMs;
			this.clientIpAddress = dto.clientIpAddress;
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
		/// Creates a deep copy clone of a ApiRequestLog Object.
		///
		/// </summary>
		public ApiRequestLog Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ApiRequestLog{
				id = this.id,
				apiKeyId = this.apiKeyId,
				endpoint = this.endpoint,
				httpMethod = this.httpMethod,
				responseStatus = this.responseStatus,
				requestDate = this.requestDate,
				durationMs = this.durationMs,
				clientIpAddress = this.clientIpAddress,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ApiRequestLog Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ApiRequestLog Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ApiRequestLog Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ApiRequestLog Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ApiRequestLog apiRequestLog)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (apiRequestLog == null)
			{
				return null;
			}

			return new {
				id = apiRequestLog.id,
				apiKeyId = apiRequestLog.apiKeyId,
				endpoint = apiRequestLog.endpoint,
				httpMethod = apiRequestLog.httpMethod,
				responseStatus = apiRequestLog.responseStatus,
				requestDate = apiRequestLog.requestDate,
				durationMs = apiRequestLog.durationMs,
				clientIpAddress = apiRequestLog.clientIpAddress,
				objectGuid = apiRequestLog.objectGuid,
				active = apiRequestLog.active,
				deleted = apiRequestLog.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ApiRequestLog Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ApiRequestLog apiRequestLog)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (apiRequestLog == null)
			{
				return null;
			}

			return new {
				id = apiRequestLog.id,
				apiKeyId = apiRequestLog.apiKeyId,
				endpoint = apiRequestLog.endpoint,
				httpMethod = apiRequestLog.httpMethod,
				responseStatus = apiRequestLog.responseStatus,
				requestDate = apiRequestLog.requestDate,
				durationMs = apiRequestLog.durationMs,
				clientIpAddress = apiRequestLog.clientIpAddress,
				objectGuid = apiRequestLog.objectGuid,
				active = apiRequestLog.active,
				deleted = apiRequestLog.deleted,
				apiKey = ApiKey.CreateMinimalAnonymous(apiRequestLog.apiKey)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ApiRequestLog Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ApiRequestLog apiRequestLog)
		{
			//
			// Return a very minimal object.
			//
			if (apiRequestLog == null)
			{
				return null;
			}

			return new {
				id = apiRequestLog.id,
				name = apiRequestLog.endpoint,
				description = string.Join(", ", new[] { apiRequestLog.endpoint, apiRequestLog.httpMethod, apiRequestLog.clientIpAddress}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
