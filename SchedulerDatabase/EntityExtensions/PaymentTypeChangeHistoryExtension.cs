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
	public partial class PaymentTypeChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)paymentTypeId; }
			set { paymentTypeId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class PaymentTypeChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 paymentTypeId { get; set; }
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
		public class PaymentTypeChangeHistoryOutputDTO : PaymentTypeChangeHistoryDTO
		{
			public PaymentType.PaymentTypeDTO paymentType { get; set; }
		}


		/// <summary>
		///
		/// Converts a PaymentTypeChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public PaymentTypeChangeHistoryDTO ToDTO()
		{
			return new PaymentTypeChangeHistoryDTO
			{
				id = this.id,
				paymentTypeId = this.paymentTypeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a PaymentTypeChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<PaymentTypeChangeHistoryDTO> ToDTOList(List<PaymentTypeChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PaymentTypeChangeHistoryDTO> output = new List<PaymentTypeChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (PaymentTypeChangeHistory paymentTypeChangeHistory in data)
			{
				output.Add(paymentTypeChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a PaymentTypeChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the PaymentTypeChangeHistoryEntity type directly.
		///
		/// </summary>
		public PaymentTypeChangeHistoryOutputDTO ToOutputDTO()
		{
			return new PaymentTypeChangeHistoryOutputDTO
			{
				id = this.id,
				paymentTypeId = this.paymentTypeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				paymentType = this.paymentType?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a PaymentTypeChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of PaymentTypeChangeHistory objects to avoid using the PaymentTypeChangeHistory entity type directly.
		///
		/// </summary>
		public static List<PaymentTypeChangeHistoryOutputDTO> ToOutputDTOList(List<PaymentTypeChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PaymentTypeChangeHistoryOutputDTO> output = new List<PaymentTypeChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (PaymentTypeChangeHistory paymentTypeChangeHistory in data)
			{
				output.Add(paymentTypeChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a PaymentTypeChangeHistory Object.
		///
		/// </summary>
		public static Database.PaymentTypeChangeHistory FromDTO(PaymentTypeChangeHistoryDTO dto)
		{
			return new Database.PaymentTypeChangeHistory
			{
				id = dto.id,
				paymentTypeId = dto.paymentTypeId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a PaymentTypeChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(PaymentTypeChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.paymentTypeId = dto.paymentTypeId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a PaymentTypeChangeHistory Object.
		///
		/// </summary>
		public PaymentTypeChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new PaymentTypeChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				paymentTypeId = this.paymentTypeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PaymentTypeChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PaymentTypeChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a PaymentTypeChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a PaymentTypeChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.PaymentTypeChangeHistory paymentTypeChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (paymentTypeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = paymentTypeChangeHistory.id,
				paymentTypeId = paymentTypeChangeHistory.paymentTypeId,
				versionNumber = paymentTypeChangeHistory.versionNumber,
				timeStamp = paymentTypeChangeHistory.timeStamp,
				userId = paymentTypeChangeHistory.userId,
				data = paymentTypeChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a PaymentTypeChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(PaymentTypeChangeHistory paymentTypeChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (paymentTypeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = paymentTypeChangeHistory.id,
				paymentTypeId = paymentTypeChangeHistory.paymentTypeId,
				versionNumber = paymentTypeChangeHistory.versionNumber,
				timeStamp = paymentTypeChangeHistory.timeStamp,
				userId = paymentTypeChangeHistory.userId,
				data = paymentTypeChangeHistory.data,
				paymentType = PaymentType.CreateMinimalAnonymous(paymentTypeChangeHistory.paymentType),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a PaymentTypeChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(PaymentTypeChangeHistory paymentTypeChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (paymentTypeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = paymentTypeChangeHistory.id,
				name = paymentTypeChangeHistory.id,
				description = paymentTypeChangeHistory.id
			 };
		}
	}
}
