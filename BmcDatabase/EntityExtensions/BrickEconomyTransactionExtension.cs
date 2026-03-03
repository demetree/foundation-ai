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
	public partial class BrickEconomyTransaction : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BrickEconomyTransactionDTO
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
			public Int32? dailyQuotaRemaining { get; set; }
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
		public class BrickEconomyTransactionOutputDTO : BrickEconomyTransactionDTO
		{
		}


		/// <summary>
		///
		/// Converts a BrickEconomyTransaction to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BrickEconomyTransactionDTO ToDTO()
		{
			return new BrickEconomyTransactionDTO
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
				dailyQuotaRemaining = this.dailyQuotaRemaining,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BrickEconomyTransaction list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BrickEconomyTransactionDTO> ToDTOList(List<BrickEconomyTransaction> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickEconomyTransactionDTO> output = new List<BrickEconomyTransactionDTO>();

			output.Capacity = data.Count;

			foreach (BrickEconomyTransaction brickEconomyTransaction in data)
			{
				output.Add(brickEconomyTransaction.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BrickEconomyTransaction to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BrickEconomyTransactionEntity type directly.
		///
		/// </summary>
		public BrickEconomyTransactionOutputDTO ToOutputDTO()
		{
			return new BrickEconomyTransactionOutputDTO
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
				dailyQuotaRemaining = this.dailyQuotaRemaining,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BrickEconomyTransaction list to list of Output Data Transfer Object intended to be used for serializing a list of BrickEconomyTransaction objects to avoid using the BrickEconomyTransaction entity type directly.
		///
		/// </summary>
		public static List<BrickEconomyTransactionOutputDTO> ToOutputDTOList(List<BrickEconomyTransaction> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickEconomyTransactionOutputDTO> output = new List<BrickEconomyTransactionOutputDTO>();

			output.Capacity = data.Count;

			foreach (BrickEconomyTransaction brickEconomyTransaction in data)
			{
				output.Add(brickEconomyTransaction.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BrickEconomyTransaction Object.
		///
		/// </summary>
		public static Database.BrickEconomyTransaction FromDTO(BrickEconomyTransactionDTO dto)
		{
			return new Database.BrickEconomyTransaction
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
				dailyQuotaRemaining = dto.dailyQuotaRemaining,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BrickEconomyTransaction Object.
		///
		/// </summary>
		public void ApplyDTO(BrickEconomyTransactionDTO dto)
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
			this.dailyQuotaRemaining = dto.dailyQuotaRemaining;
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
		/// Creates a deep copy clone of a BrickEconomyTransaction Object.
		///
		/// </summary>
		public BrickEconomyTransaction Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BrickEconomyTransaction{
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
				dailyQuotaRemaining = this.dailyQuotaRemaining,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickEconomyTransaction Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickEconomyTransaction Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BrickEconomyTransaction Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BrickEconomyTransaction Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BrickEconomyTransaction brickEconomyTransaction)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (brickEconomyTransaction == null)
			{
				return null;
			}

			return new {
				id = brickEconomyTransaction.id,
				transactionDate = brickEconomyTransaction.transactionDate,
				direction = brickEconomyTransaction.direction,
				methodName = brickEconomyTransaction.methodName,
				requestSummary = brickEconomyTransaction.requestSummary,
				success = brickEconomyTransaction.success,
				errorMessage = brickEconomyTransaction.errorMessage,
				triggeredBy = brickEconomyTransaction.triggeredBy,
				recordCount = brickEconomyTransaction.recordCount,
				dailyQuotaRemaining = brickEconomyTransaction.dailyQuotaRemaining,
				objectGuid = brickEconomyTransaction.objectGuid,
				active = brickEconomyTransaction.active,
				deleted = brickEconomyTransaction.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BrickEconomyTransaction Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BrickEconomyTransaction brickEconomyTransaction)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (brickEconomyTransaction == null)
			{
				return null;
			}

			return new {
				id = brickEconomyTransaction.id,
				transactionDate = brickEconomyTransaction.transactionDate,
				direction = brickEconomyTransaction.direction,
				methodName = brickEconomyTransaction.methodName,
				requestSummary = brickEconomyTransaction.requestSummary,
				success = brickEconomyTransaction.success,
				errorMessage = brickEconomyTransaction.errorMessage,
				triggeredBy = brickEconomyTransaction.triggeredBy,
				recordCount = brickEconomyTransaction.recordCount,
				dailyQuotaRemaining = brickEconomyTransaction.dailyQuotaRemaining,
				objectGuid = brickEconomyTransaction.objectGuid,
				active = brickEconomyTransaction.active,
				deleted = brickEconomyTransaction.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BrickEconomyTransaction Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BrickEconomyTransaction brickEconomyTransaction)
		{
			//
			// Return a very minimal object.
			//
			if (brickEconomyTransaction == null)
			{
				return null;
			}

			return new {
				id = brickEconomyTransaction.id,
				name = brickEconomyTransaction.direction,
				description = string.Join(", ", new[] { brickEconomyTransaction.direction, brickEconomyTransaction.methodName, brickEconomyTransaction.triggeredBy}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
