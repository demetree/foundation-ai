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
	public partial class BrickSetTransaction : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BrickSetTransactionDTO
		{
			public Int32 id { get; set; }
			public DateTime? transactionDate { get; set; }
			[Required]
			public String direction { get; set; }
			[Required]
			public String methodName { get; set; }
			public String requestSummary { get; set; }
			[Required]
			public Boolean success { get; set; }
			public String errorMessage { get; set; }
			[Required]
			public String triggeredBy { get; set; }
			public Int32? recordCount { get; set; }
			public Int32? apiCallsRemaining { get; set; }
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
		public class BrickSetTransactionOutputDTO : BrickSetTransactionDTO
		{
		}


		/// <summary>
		///
		/// Converts a BrickSetTransaction to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BrickSetTransactionDTO ToDTO()
		{
			return new BrickSetTransactionDTO
			{
				id = this.id,
				transactionDate = this.transactionDate,
				direction = this.direction,
				methodName = this.methodName,
				requestSummary = this.requestSummary,
				success = this.success,
				errorMessage = this.errorMessage,
				triggeredBy = this.triggeredBy,
				recordCount = this.recordCount,
				apiCallsRemaining = this.apiCallsRemaining,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BrickSetTransaction list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BrickSetTransactionDTO> ToDTOList(List<BrickSetTransaction> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickSetTransactionDTO> output = new List<BrickSetTransactionDTO>();

			output.Capacity = data.Count;

			foreach (BrickSetTransaction brickSetTransaction in data)
			{
				output.Add(brickSetTransaction.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BrickSetTransaction to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BrickSetTransactionEntity type directly.
		///
		/// </summary>
		public BrickSetTransactionOutputDTO ToOutputDTO()
		{
			return new BrickSetTransactionOutputDTO
			{
				id = this.id,
				transactionDate = this.transactionDate,
				direction = this.direction,
				methodName = this.methodName,
				requestSummary = this.requestSummary,
				success = this.success,
				errorMessage = this.errorMessage,
				triggeredBy = this.triggeredBy,
				recordCount = this.recordCount,
				apiCallsRemaining = this.apiCallsRemaining,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BrickSetTransaction list to list of Output Data Transfer Object intended to be used for serializing a list of BrickSetTransaction objects to avoid using the BrickSetTransaction entity type directly.
		///
		/// </summary>
		public static List<BrickSetTransactionOutputDTO> ToOutputDTOList(List<BrickSetTransaction> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickSetTransactionOutputDTO> output = new List<BrickSetTransactionOutputDTO>();

			output.Capacity = data.Count;

			foreach (BrickSetTransaction brickSetTransaction in data)
			{
				output.Add(brickSetTransaction.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BrickSetTransaction Object.
		///
		/// </summary>
		public static Database.BrickSetTransaction FromDTO(BrickSetTransactionDTO dto)
		{
			return new Database.BrickSetTransaction
			{
				id = dto.id,
				transactionDate = dto.transactionDate,
				direction = dto.direction,
				methodName = dto.methodName,
				requestSummary = dto.requestSummary,
				success = dto.success,
				errorMessage = dto.errorMessage,
				triggeredBy = dto.triggeredBy,
				recordCount = dto.recordCount,
				apiCallsRemaining = dto.apiCallsRemaining,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BrickSetTransaction Object.
		///
		/// </summary>
		public void ApplyDTO(BrickSetTransactionDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.transactionDate = dto.transactionDate;
			this.direction = dto.direction;
			this.methodName = dto.methodName;
			this.requestSummary = dto.requestSummary;
			this.success = dto.success;
			this.errorMessage = dto.errorMessage;
			this.triggeredBy = dto.triggeredBy;
			this.recordCount = dto.recordCount;
			this.apiCallsRemaining = dto.apiCallsRemaining;
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
		/// Creates a deep copy clone of a BrickSetTransaction Object.
		///
		/// </summary>
		public BrickSetTransaction Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BrickSetTransaction{
				id = this.id,
				tenantGuid = this.tenantGuid,
				transactionDate = this.transactionDate,
				direction = this.direction,
				methodName = this.methodName,
				requestSummary = this.requestSummary,
				success = this.success,
				errorMessage = this.errorMessage,
				triggeredBy = this.triggeredBy,
				recordCount = this.recordCount,
				apiCallsRemaining = this.apiCallsRemaining,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickSetTransaction Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickSetTransaction Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BrickSetTransaction Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BrickSetTransaction Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BrickSetTransaction brickSetTransaction)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (brickSetTransaction == null)
			{
				return null;
			}

			return new {
				id = brickSetTransaction.id,
				transactionDate = brickSetTransaction.transactionDate,
				direction = brickSetTransaction.direction,
				methodName = brickSetTransaction.methodName,
				requestSummary = brickSetTransaction.requestSummary,
				success = brickSetTransaction.success,
				errorMessage = brickSetTransaction.errorMessage,
				triggeredBy = brickSetTransaction.triggeredBy,
				recordCount = brickSetTransaction.recordCount,
				apiCallsRemaining = brickSetTransaction.apiCallsRemaining,
				objectGuid = brickSetTransaction.objectGuid,
				active = brickSetTransaction.active,
				deleted = brickSetTransaction.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BrickSetTransaction Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BrickSetTransaction brickSetTransaction)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (brickSetTransaction == null)
			{
				return null;
			}

			return new {
				id = brickSetTransaction.id,
				transactionDate = brickSetTransaction.transactionDate,
				direction = brickSetTransaction.direction,
				methodName = brickSetTransaction.methodName,
				requestSummary = brickSetTransaction.requestSummary,
				success = brickSetTransaction.success,
				errorMessage = brickSetTransaction.errorMessage,
				triggeredBy = brickSetTransaction.triggeredBy,
				recordCount = brickSetTransaction.recordCount,
				apiCallsRemaining = brickSetTransaction.apiCallsRemaining,
				objectGuid = brickSetTransaction.objectGuid,
				active = brickSetTransaction.active,
				deleted = brickSetTransaction.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BrickSetTransaction Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BrickSetTransaction brickSetTransaction)
		{
			//
			// Return a very minimal object.
			//
			if (brickSetTransaction == null)
			{
				return null;
			}

			return new {
				id = brickSetTransaction.id,
				name = brickSetTransaction.direction,
				description = string.Join(", ", new[] { brickSetTransaction.direction, brickSetTransaction.methodName, brickSetTransaction.triggeredBy}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
