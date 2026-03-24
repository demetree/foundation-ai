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
	public partial class PaymentTransactionChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)paymentTransactionId; }
			set { paymentTransactionId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class PaymentTransactionChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 paymentTransactionId { get; set; }
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
		public class PaymentTransactionChangeHistoryOutputDTO : PaymentTransactionChangeHistoryDTO
		{
			public PaymentTransaction.PaymentTransactionDTO paymentTransaction { get; set; }
		}


		/// <summary>
		///
		/// Converts a PaymentTransactionChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public PaymentTransactionChangeHistoryDTO ToDTO()
		{
			return new PaymentTransactionChangeHistoryDTO
			{
				id = this.id,
				paymentTransactionId = this.paymentTransactionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a PaymentTransactionChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<PaymentTransactionChangeHistoryDTO> ToDTOList(List<PaymentTransactionChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PaymentTransactionChangeHistoryDTO> output = new List<PaymentTransactionChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (PaymentTransactionChangeHistory paymentTransactionChangeHistory in data)
			{
				output.Add(paymentTransactionChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a PaymentTransactionChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the PaymentTransactionChangeHistory Entity type directly.
		///
		/// </summary>
		public PaymentTransactionChangeHistoryOutputDTO ToOutputDTO()
		{
			return new PaymentTransactionChangeHistoryOutputDTO
			{
				id = this.id,
				paymentTransactionId = this.paymentTransactionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				paymentTransaction = this.paymentTransaction?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a PaymentTransactionChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of PaymentTransactionChangeHistory objects to avoid using the PaymentTransactionChangeHistory entity type directly.
		///
		/// </summary>
		public static List<PaymentTransactionChangeHistoryOutputDTO> ToOutputDTOList(List<PaymentTransactionChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PaymentTransactionChangeHistoryOutputDTO> output = new List<PaymentTransactionChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (PaymentTransactionChangeHistory paymentTransactionChangeHistory in data)
			{
				output.Add(paymentTransactionChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a PaymentTransactionChangeHistory Object.
		///
		/// </summary>
		public static Database.PaymentTransactionChangeHistory FromDTO(PaymentTransactionChangeHistoryDTO dto)
		{
			return new Database.PaymentTransactionChangeHistory
			{
				id = dto.id,
				paymentTransactionId = dto.paymentTransactionId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a PaymentTransactionChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(PaymentTransactionChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.paymentTransactionId = dto.paymentTransactionId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a PaymentTransactionChangeHistory Object.
		///
		/// </summary>
		public PaymentTransactionChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new PaymentTransactionChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				paymentTransactionId = this.paymentTransactionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PaymentTransactionChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PaymentTransactionChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a PaymentTransactionChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a PaymentTransactionChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.PaymentTransactionChangeHistory paymentTransactionChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (paymentTransactionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = paymentTransactionChangeHistory.id,
				paymentTransactionId = paymentTransactionChangeHistory.paymentTransactionId,
				versionNumber = paymentTransactionChangeHistory.versionNumber,
				timeStamp = paymentTransactionChangeHistory.timeStamp,
				userId = paymentTransactionChangeHistory.userId,
				data = paymentTransactionChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a PaymentTransactionChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(PaymentTransactionChangeHistory paymentTransactionChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (paymentTransactionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = paymentTransactionChangeHistory.id,
				paymentTransactionId = paymentTransactionChangeHistory.paymentTransactionId,
				versionNumber = paymentTransactionChangeHistory.versionNumber,
				timeStamp = paymentTransactionChangeHistory.timeStamp,
				userId = paymentTransactionChangeHistory.userId,
				data = paymentTransactionChangeHistory.data,
				paymentTransaction = PaymentTransaction.CreateMinimalAnonymous(paymentTransactionChangeHistory.paymentTransaction)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a PaymentTransactionChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(PaymentTransactionChangeHistory paymentTransactionChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (paymentTransactionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = paymentTransactionChangeHistory.id,
				name = paymentTransactionChangeHistory.id,
				description = paymentTransactionChangeHistory.id
			 };
		}
	}
}
