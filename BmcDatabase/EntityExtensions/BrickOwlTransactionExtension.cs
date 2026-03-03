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
	public partial class BrickOwlTransaction : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BrickOwlTransactionDTO
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
		public class BrickOwlTransactionOutputDTO : BrickOwlTransactionDTO
		{
		}


		/// <summary>
		///
		/// Converts a BrickOwlTransaction to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BrickOwlTransactionDTO ToDTO()
		{
			return new BrickOwlTransactionDTO
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
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BrickOwlTransaction list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BrickOwlTransactionDTO> ToDTOList(List<BrickOwlTransaction> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickOwlTransactionDTO> output = new List<BrickOwlTransactionDTO>();

			output.Capacity = data.Count;

			foreach (BrickOwlTransaction brickOwlTransaction in data)
			{
				output.Add(brickOwlTransaction.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BrickOwlTransaction to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BrickOwlTransactionEntity type directly.
		///
		/// </summary>
		public BrickOwlTransactionOutputDTO ToOutputDTO()
		{
			return new BrickOwlTransactionOutputDTO
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
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BrickOwlTransaction list to list of Output Data Transfer Object intended to be used for serializing a list of BrickOwlTransaction objects to avoid using the BrickOwlTransaction entity type directly.
		///
		/// </summary>
		public static List<BrickOwlTransactionOutputDTO> ToOutputDTOList(List<BrickOwlTransaction> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickOwlTransactionOutputDTO> output = new List<BrickOwlTransactionOutputDTO>();

			output.Capacity = data.Count;

			foreach (BrickOwlTransaction brickOwlTransaction in data)
			{
				output.Add(brickOwlTransaction.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BrickOwlTransaction Object.
		///
		/// </summary>
		public static Database.BrickOwlTransaction FromDTO(BrickOwlTransactionDTO dto)
		{
			return new Database.BrickOwlTransaction
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
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BrickOwlTransaction Object.
		///
		/// </summary>
		public void ApplyDTO(BrickOwlTransactionDTO dto)
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
		/// Creates a deep copy clone of a BrickOwlTransaction Object.
		///
		/// </summary>
		public BrickOwlTransaction Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BrickOwlTransaction{
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
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickOwlTransaction Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickOwlTransaction Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BrickOwlTransaction Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BrickOwlTransaction Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BrickOwlTransaction brickOwlTransaction)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (brickOwlTransaction == null)
			{
				return null;
			}

			return new {
				id = brickOwlTransaction.id,
				transactionDate = brickOwlTransaction.transactionDate,
				direction = brickOwlTransaction.direction,
				methodName = brickOwlTransaction.methodName,
				requestSummary = brickOwlTransaction.requestSummary,
				success = brickOwlTransaction.success,
				errorMessage = brickOwlTransaction.errorMessage,
				triggeredBy = brickOwlTransaction.triggeredBy,
				recordCount = brickOwlTransaction.recordCount,
				objectGuid = brickOwlTransaction.objectGuid,
				active = brickOwlTransaction.active,
				deleted = brickOwlTransaction.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BrickOwlTransaction Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BrickOwlTransaction brickOwlTransaction)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (brickOwlTransaction == null)
			{
				return null;
			}

			return new {
				id = brickOwlTransaction.id,
				transactionDate = brickOwlTransaction.transactionDate,
				direction = brickOwlTransaction.direction,
				methodName = brickOwlTransaction.methodName,
				requestSummary = brickOwlTransaction.requestSummary,
				success = brickOwlTransaction.success,
				errorMessage = brickOwlTransaction.errorMessage,
				triggeredBy = brickOwlTransaction.triggeredBy,
				recordCount = brickOwlTransaction.recordCount,
				objectGuid = brickOwlTransaction.objectGuid,
				active = brickOwlTransaction.active,
				deleted = brickOwlTransaction.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BrickOwlTransaction Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BrickOwlTransaction brickOwlTransaction)
		{
			//
			// Return a very minimal object.
			//
			if (brickOwlTransaction == null)
			{
				return null;
			}

			return new {
				id = brickOwlTransaction.id,
				name = brickOwlTransaction.direction,
				description = string.Join(", ", new[] { brickOwlTransaction.direction, brickOwlTransaction.methodName, brickOwlTransaction.triggeredBy}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
