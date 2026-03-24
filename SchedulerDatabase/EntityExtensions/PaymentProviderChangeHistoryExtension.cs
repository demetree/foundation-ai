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
	public partial class PaymentProviderChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)paymentProviderId; }
			set { paymentProviderId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class PaymentProviderChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 paymentProviderId { get; set; }
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
		public class PaymentProviderChangeHistoryOutputDTO : PaymentProviderChangeHistoryDTO
		{
			public PaymentProvider.PaymentProviderDTO paymentProvider { get; set; }
		}


		/// <summary>
		///
		/// Converts a PaymentProviderChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public PaymentProviderChangeHistoryDTO ToDTO()
		{
			return new PaymentProviderChangeHistoryDTO
			{
				id = this.id,
				paymentProviderId = this.paymentProviderId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a PaymentProviderChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<PaymentProviderChangeHistoryDTO> ToDTOList(List<PaymentProviderChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PaymentProviderChangeHistoryDTO> output = new List<PaymentProviderChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (PaymentProviderChangeHistory paymentProviderChangeHistory in data)
			{
				output.Add(paymentProviderChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a PaymentProviderChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the PaymentProviderChangeHistory Entity type directly.
		///
		/// </summary>
		public PaymentProviderChangeHistoryOutputDTO ToOutputDTO()
		{
			return new PaymentProviderChangeHistoryOutputDTO
			{
				id = this.id,
				paymentProviderId = this.paymentProviderId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				paymentProvider = this.paymentProvider?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a PaymentProviderChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of PaymentProviderChangeHistory objects to avoid using the PaymentProviderChangeHistory entity type directly.
		///
		/// </summary>
		public static List<PaymentProviderChangeHistoryOutputDTO> ToOutputDTOList(List<PaymentProviderChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PaymentProviderChangeHistoryOutputDTO> output = new List<PaymentProviderChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (PaymentProviderChangeHistory paymentProviderChangeHistory in data)
			{
				output.Add(paymentProviderChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a PaymentProviderChangeHistory Object.
		///
		/// </summary>
		public static Database.PaymentProviderChangeHistory FromDTO(PaymentProviderChangeHistoryDTO dto)
		{
			return new Database.PaymentProviderChangeHistory
			{
				id = dto.id,
				paymentProviderId = dto.paymentProviderId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a PaymentProviderChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(PaymentProviderChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.paymentProviderId = dto.paymentProviderId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a PaymentProviderChangeHistory Object.
		///
		/// </summary>
		public PaymentProviderChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new PaymentProviderChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				paymentProviderId = this.paymentProviderId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PaymentProviderChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PaymentProviderChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a PaymentProviderChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a PaymentProviderChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.PaymentProviderChangeHistory paymentProviderChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (paymentProviderChangeHistory == null)
			{
				return null;
			}

			return new {
				id = paymentProviderChangeHistory.id,
				paymentProviderId = paymentProviderChangeHistory.paymentProviderId,
				versionNumber = paymentProviderChangeHistory.versionNumber,
				timeStamp = paymentProviderChangeHistory.timeStamp,
				userId = paymentProviderChangeHistory.userId,
				data = paymentProviderChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a PaymentProviderChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(PaymentProviderChangeHistory paymentProviderChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (paymentProviderChangeHistory == null)
			{
				return null;
			}

			return new {
				id = paymentProviderChangeHistory.id,
				paymentProviderId = paymentProviderChangeHistory.paymentProviderId,
				versionNumber = paymentProviderChangeHistory.versionNumber,
				timeStamp = paymentProviderChangeHistory.timeStamp,
				userId = paymentProviderChangeHistory.userId,
				data = paymentProviderChangeHistory.data,
				paymentProvider = PaymentProvider.CreateMinimalAnonymous(paymentProviderChangeHistory.paymentProvider)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a PaymentProviderChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(PaymentProviderChangeHistory paymentProviderChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (paymentProviderChangeHistory == null)
			{
				return null;
			}

			return new {
				id = paymentProviderChangeHistory.id,
				name = paymentProviderChangeHistory.id,
				description = paymentProviderChangeHistory.id
			 };
		}
	}
}
