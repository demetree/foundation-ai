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
	public partial class RebrickableTransaction : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class RebrickableTransactionDTO
		{
			public Int32 id { get; set; }
			public DateTime? transactionDate { get; set; }
			[Required]
			public String direction { get; set; }
			[Required]
			public String httpMethod { get; set; }
			[Required]
			public String endpoint { get; set; }
			public String requestSummary { get; set; }
			public Int32? responseStatusCode { get; set; }
			public String responseBody { get; set; }
			[Required]
			public Boolean success { get; set; }
			public String errorMessage { get; set; }
			[Required]
			public String triggeredBy { get; set; }
			public Int32? recordCount { get; set; }
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
		public class RebrickableTransactionOutputDTO : RebrickableTransactionDTO
		{
		}


		/// <summary>
		///
		/// Converts a RebrickableTransaction to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public RebrickableTransactionDTO ToDTO()
		{
			return new RebrickableTransactionDTO
			{
				id = this.id,
				transactionDate = this.transactionDate,
				direction = this.direction,
				httpMethod = this.httpMethod,
				endpoint = this.endpoint,
				requestSummary = this.requestSummary,
				responseStatusCode = this.responseStatusCode,
				responseBody = this.responseBody,
				success = this.success,
				errorMessage = this.errorMessage,
				triggeredBy = this.triggeredBy,
				recordCount = this.recordCount,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a RebrickableTransaction list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<RebrickableTransactionDTO> ToDTOList(List<RebrickableTransaction> data)
		{
			if (data == null)
			{
				return null;
			}

			List<RebrickableTransactionDTO> output = new List<RebrickableTransactionDTO>();

			output.Capacity = data.Count;

			foreach (RebrickableTransaction rebrickableTransaction in data)
			{
				output.Add(rebrickableTransaction.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a RebrickableTransaction to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the RebrickableTransactionEntity type directly.
		///
		/// </summary>
		public RebrickableTransactionOutputDTO ToOutputDTO()
		{
			return new RebrickableTransactionOutputDTO
			{
				id = this.id,
				transactionDate = this.transactionDate,
				direction = this.direction,
				httpMethod = this.httpMethod,
				endpoint = this.endpoint,
				requestSummary = this.requestSummary,
				responseStatusCode = this.responseStatusCode,
				responseBody = this.responseBody,
				success = this.success,
				errorMessage = this.errorMessage,
				triggeredBy = this.triggeredBy,
				recordCount = this.recordCount,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a RebrickableTransaction list to list of Output Data Transfer Object intended to be used for serializing a list of RebrickableTransaction objects to avoid using the RebrickableTransaction entity type directly.
		///
		/// </summary>
		public static List<RebrickableTransactionOutputDTO> ToOutputDTOList(List<RebrickableTransaction> data)
		{
			if (data == null)
			{
				return null;
			}

			List<RebrickableTransactionOutputDTO> output = new List<RebrickableTransactionOutputDTO>();

			output.Capacity = data.Count;

			foreach (RebrickableTransaction rebrickableTransaction in data)
			{
				output.Add(rebrickableTransaction.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a RebrickableTransaction Object.
		///
		/// </summary>
		public static Database.RebrickableTransaction FromDTO(RebrickableTransactionDTO dto)
		{
			return new Database.RebrickableTransaction
			{
				id = dto.id,
				transactionDate = dto.transactionDate,
				direction = dto.direction,
				httpMethod = dto.httpMethod,
				endpoint = dto.endpoint,
				requestSummary = dto.requestSummary,
				responseStatusCode = dto.responseStatusCode,
				responseBody = dto.responseBody,
				success = dto.success,
				errorMessage = dto.errorMessage,
				triggeredBy = dto.triggeredBy,
				recordCount = dto.recordCount,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a RebrickableTransaction Object.
		///
		/// </summary>
		public void ApplyDTO(RebrickableTransactionDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.transactionDate = dto.transactionDate;
			this.direction = dto.direction;
			this.httpMethod = dto.httpMethod;
			this.endpoint = dto.endpoint;
			this.requestSummary = dto.requestSummary;
			this.responseStatusCode = dto.responseStatusCode;
			this.responseBody = dto.responseBody;
			this.success = dto.success;
			this.errorMessage = dto.errorMessage;
			this.triggeredBy = dto.triggeredBy;
			this.recordCount = dto.recordCount;
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
		/// Creates a deep copy clone of a RebrickableTransaction Object.
		///
		/// </summary>
		public RebrickableTransaction Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new RebrickableTransaction{
				id = this.id,
				tenantGuid = this.tenantGuid,
				transactionDate = this.transactionDate,
				direction = this.direction,
				httpMethod = this.httpMethod,
				endpoint = this.endpoint,
				requestSummary = this.requestSummary,
				responseStatusCode = this.responseStatusCode,
				responseBody = this.responseBody,
				success = this.success,
				errorMessage = this.errorMessage,
				triggeredBy = this.triggeredBy,
				recordCount = this.recordCount,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a RebrickableTransaction Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a RebrickableTransaction Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a RebrickableTransaction Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a RebrickableTransaction Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.RebrickableTransaction rebrickableTransaction)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (rebrickableTransaction == null)
			{
				return null;
			}

			return new {
				id = rebrickableTransaction.id,
				transactionDate = rebrickableTransaction.transactionDate,
				direction = rebrickableTransaction.direction,
				httpMethod = rebrickableTransaction.httpMethod,
				endpoint = rebrickableTransaction.endpoint,
				requestSummary = rebrickableTransaction.requestSummary,
				responseStatusCode = rebrickableTransaction.responseStatusCode,
				responseBody = rebrickableTransaction.responseBody,
				success = rebrickableTransaction.success,
				errorMessage = rebrickableTransaction.errorMessage,
				triggeredBy = rebrickableTransaction.triggeredBy,
				recordCount = rebrickableTransaction.recordCount,
				objectGuid = rebrickableTransaction.objectGuid,
				active = rebrickableTransaction.active,
				deleted = rebrickableTransaction.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a RebrickableTransaction Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(RebrickableTransaction rebrickableTransaction)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (rebrickableTransaction == null)
			{
				return null;
			}

			return new {
				id = rebrickableTransaction.id,
				transactionDate = rebrickableTransaction.transactionDate,
				direction = rebrickableTransaction.direction,
				httpMethod = rebrickableTransaction.httpMethod,
				endpoint = rebrickableTransaction.endpoint,
				requestSummary = rebrickableTransaction.requestSummary,
				responseStatusCode = rebrickableTransaction.responseStatusCode,
				responseBody = rebrickableTransaction.responseBody,
				success = rebrickableTransaction.success,
				errorMessage = rebrickableTransaction.errorMessage,
				triggeredBy = rebrickableTransaction.triggeredBy,
				recordCount = rebrickableTransaction.recordCount,
				objectGuid = rebrickableTransaction.objectGuid,
				active = rebrickableTransaction.active,
				deleted = rebrickableTransaction.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a RebrickableTransaction Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(RebrickableTransaction rebrickableTransaction)
		{
			//
			// Return a very minimal object.
			//
			if (rebrickableTransaction == null)
			{
				return null;
			}

			return new {
				id = rebrickableTransaction.id,
				name = rebrickableTransaction.direction,
				description = string.Join(", ", new[] { rebrickableTransaction.direction, rebrickableTransaction.httpMethod, rebrickableTransaction.endpoint}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
