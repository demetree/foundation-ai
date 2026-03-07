using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.Scheduler.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class FinancialTransactionChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)financialTransactionId; }
			set { financialTransactionId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class FinancialTransactionChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 financialTransactionId { get; set; }
			public Int32 versionNumber { get; set; }
			[Required]
			public DateTime timeStamp { get; set; }
			[Required]
			public Int32 userId { get; set; }
			[Required]
			public String data { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class FinancialTransactionChangeHistoryOutputDTO : FinancialTransactionChangeHistoryDTO
		{
			public FinancialTransaction.FinancialTransactionDTO financialTransaction { get; set; }
		}


		/// <summary>
		///
		/// Converts a FinancialTransactionChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public FinancialTransactionChangeHistoryDTO ToDTO()
		{
			return new FinancialTransactionChangeHistoryDTO
			{
				id = this.id,
				financialTransactionId = this.financialTransactionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a FinancialTransactionChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<FinancialTransactionChangeHistoryDTO> ToDTOList(List<FinancialTransactionChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<FinancialTransactionChangeHistoryDTO> output = new List<FinancialTransactionChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (FinancialTransactionChangeHistory financialTransactionChangeHistory in data)
			{
				output.Add(financialTransactionChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a FinancialTransactionChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the FinancialTransactionChangeHistoryEntity type directly.
		///
		/// </summary>
		public FinancialTransactionChangeHistoryOutputDTO ToOutputDTO()
		{
			return new FinancialTransactionChangeHistoryOutputDTO
			{
				id = this.id,
				financialTransactionId = this.financialTransactionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				financialTransaction = this.financialTransaction?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a FinancialTransactionChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of FinancialTransactionChangeHistory objects to avoid using the FinancialTransactionChangeHistory entity type directly.
		///
		/// </summary>
		public static List<FinancialTransactionChangeHistoryOutputDTO> ToOutputDTOList(List<FinancialTransactionChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<FinancialTransactionChangeHistoryOutputDTO> output = new List<FinancialTransactionChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (FinancialTransactionChangeHistory financialTransactionChangeHistory in data)
			{
				output.Add(financialTransactionChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a FinancialTransactionChangeHistory Object.
		///
		/// </summary>
		public static Database.FinancialTransactionChangeHistory FromDTO(FinancialTransactionChangeHistoryDTO dto)
		{
			return new Database.FinancialTransactionChangeHistory
			{
				id = dto.id,
				financialTransactionId = dto.financialTransactionId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a FinancialTransactionChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(FinancialTransactionChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.financialTransactionId = dto.financialTransactionId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a FinancialTransactionChangeHistory Object.
		///
		/// </summary>
		public FinancialTransactionChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new FinancialTransactionChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				financialTransactionId = this.financialTransactionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a FinancialTransactionChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a FinancialTransactionChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a FinancialTransactionChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a FinancialTransactionChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.FinancialTransactionChangeHistory financialTransactionChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (financialTransactionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = financialTransactionChangeHistory.id,
				financialTransactionId = financialTransactionChangeHistory.financialTransactionId,
				versionNumber = financialTransactionChangeHistory.versionNumber,
				timeStamp = financialTransactionChangeHistory.timeStamp,
				userId = financialTransactionChangeHistory.userId,
				data = financialTransactionChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a FinancialTransactionChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(FinancialTransactionChangeHistory financialTransactionChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (financialTransactionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = financialTransactionChangeHistory.id,
				financialTransactionId = financialTransactionChangeHistory.financialTransactionId,
				versionNumber = financialTransactionChangeHistory.versionNumber,
				timeStamp = financialTransactionChangeHistory.timeStamp,
				userId = financialTransactionChangeHistory.userId,
				data = financialTransactionChangeHistory.data,
				financialTransaction = FinancialTransaction.CreateMinimalAnonymous(financialTransactionChangeHistory.financialTransaction)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a FinancialTransactionChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(FinancialTransactionChangeHistory financialTransactionChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (financialTransactionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = financialTransactionChangeHistory.id,
				name = financialTransactionChangeHistory.id,
				description = financialTransactionChangeHistory.id
			 };
		}
	}
}
