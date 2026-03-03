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
	public partial class BrickLinkTransaction : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BrickLinkTransactionDTO
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
		public class BrickLinkTransactionOutputDTO : BrickLinkTransactionDTO
		{
		}


		/// <summary>
		///
		/// Converts a BrickLinkTransaction to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BrickLinkTransactionDTO ToDTO()
		{
			return new BrickLinkTransactionDTO
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
		/// Converts a BrickLinkTransaction list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BrickLinkTransactionDTO> ToDTOList(List<BrickLinkTransaction> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickLinkTransactionDTO> output = new List<BrickLinkTransactionDTO>();

			output.Capacity = data.Count;

			foreach (BrickLinkTransaction brickLinkTransaction in data)
			{
				output.Add(brickLinkTransaction.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BrickLinkTransaction to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BrickLinkTransactionEntity type directly.
		///
		/// </summary>
		public BrickLinkTransactionOutputDTO ToOutputDTO()
		{
			return new BrickLinkTransactionOutputDTO
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
		/// Converts a BrickLinkTransaction list to list of Output Data Transfer Object intended to be used for serializing a list of BrickLinkTransaction objects to avoid using the BrickLinkTransaction entity type directly.
		///
		/// </summary>
		public static List<BrickLinkTransactionOutputDTO> ToOutputDTOList(List<BrickLinkTransaction> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickLinkTransactionOutputDTO> output = new List<BrickLinkTransactionOutputDTO>();

			output.Capacity = data.Count;

			foreach (BrickLinkTransaction brickLinkTransaction in data)
			{
				output.Add(brickLinkTransaction.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BrickLinkTransaction Object.
		///
		/// </summary>
		public static Database.BrickLinkTransaction FromDTO(BrickLinkTransactionDTO dto)
		{
			return new Database.BrickLinkTransaction
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
		/// Applies the values from an INPUT DTO to a BrickLinkTransaction Object.
		///
		/// </summary>
		public void ApplyDTO(BrickLinkTransactionDTO dto)
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
		/// Creates a deep copy clone of a BrickLinkTransaction Object.
		///
		/// </summary>
		public BrickLinkTransaction Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BrickLinkTransaction{
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
        /// Creates an anonymous object containing properties from a BrickLinkTransaction Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickLinkTransaction Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BrickLinkTransaction Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BrickLinkTransaction Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BrickLinkTransaction brickLinkTransaction)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (brickLinkTransaction == null)
			{
				return null;
			}

			return new {
				id = brickLinkTransaction.id,
				transactionDate = brickLinkTransaction.transactionDate,
				direction = brickLinkTransaction.direction,
				methodName = brickLinkTransaction.methodName,
				requestSummary = brickLinkTransaction.requestSummary,
				success = brickLinkTransaction.success,
				errorMessage = brickLinkTransaction.errorMessage,
				triggeredBy = brickLinkTransaction.triggeredBy,
				recordCount = brickLinkTransaction.recordCount,
				objectGuid = brickLinkTransaction.objectGuid,
				active = brickLinkTransaction.active,
				deleted = brickLinkTransaction.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BrickLinkTransaction Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BrickLinkTransaction brickLinkTransaction)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (brickLinkTransaction == null)
			{
				return null;
			}

			return new {
				id = brickLinkTransaction.id,
				transactionDate = brickLinkTransaction.transactionDate,
				direction = brickLinkTransaction.direction,
				methodName = brickLinkTransaction.methodName,
				requestSummary = brickLinkTransaction.requestSummary,
				success = brickLinkTransaction.success,
				errorMessage = brickLinkTransaction.errorMessage,
				triggeredBy = brickLinkTransaction.triggeredBy,
				recordCount = brickLinkTransaction.recordCount,
				objectGuid = brickLinkTransaction.objectGuid,
				active = brickLinkTransaction.active,
				deleted = brickLinkTransaction.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BrickLinkTransaction Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BrickLinkTransaction brickLinkTransaction)
		{
			//
			// Return a very minimal object.
			//
			if (brickLinkTransaction == null)
			{
				return null;
			}

			return new {
				id = brickLinkTransaction.id,
				name = brickLinkTransaction.direction,
				description = string.Join(", ", new[] { brickLinkTransaction.direction, brickLinkTransaction.methodName, brickLinkTransaction.triggeredBy}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
