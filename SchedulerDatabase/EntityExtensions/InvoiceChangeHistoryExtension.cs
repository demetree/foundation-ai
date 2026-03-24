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
	public partial class InvoiceChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)invoiceId; }
			set { invoiceId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class InvoiceChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 invoiceId { get; set; }
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
		public class InvoiceChangeHistoryOutputDTO : InvoiceChangeHistoryDTO
		{
			public Invoice.InvoiceDTO invoice { get; set; }
		}


		/// <summary>
		///
		/// Converts a InvoiceChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public InvoiceChangeHistoryDTO ToDTO()
		{
			return new InvoiceChangeHistoryDTO
			{
				id = this.id,
				invoiceId = this.invoiceId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a InvoiceChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<InvoiceChangeHistoryDTO> ToDTOList(List<InvoiceChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<InvoiceChangeHistoryDTO> output = new List<InvoiceChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (InvoiceChangeHistory invoiceChangeHistory in data)
			{
				output.Add(invoiceChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a InvoiceChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the InvoiceChangeHistory Entity type directly.
		///
		/// </summary>
		public InvoiceChangeHistoryOutputDTO ToOutputDTO()
		{
			return new InvoiceChangeHistoryOutputDTO
			{
				id = this.id,
				invoiceId = this.invoiceId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				invoice = this.invoice?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a InvoiceChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of InvoiceChangeHistory objects to avoid using the InvoiceChangeHistory entity type directly.
		///
		/// </summary>
		public static List<InvoiceChangeHistoryOutputDTO> ToOutputDTOList(List<InvoiceChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<InvoiceChangeHistoryOutputDTO> output = new List<InvoiceChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (InvoiceChangeHistory invoiceChangeHistory in data)
			{
				output.Add(invoiceChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a InvoiceChangeHistory Object.
		///
		/// </summary>
		public static Database.InvoiceChangeHistory FromDTO(InvoiceChangeHistoryDTO dto)
		{
			return new Database.InvoiceChangeHistory
			{
				id = dto.id,
				invoiceId = dto.invoiceId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a InvoiceChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(InvoiceChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.invoiceId = dto.invoiceId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a InvoiceChangeHistory Object.
		///
		/// </summary>
		public InvoiceChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new InvoiceChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				invoiceId = this.invoiceId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a InvoiceChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a InvoiceChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a InvoiceChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a InvoiceChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.InvoiceChangeHistory invoiceChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (invoiceChangeHistory == null)
			{
				return null;
			}

			return new {
				id = invoiceChangeHistory.id,
				invoiceId = invoiceChangeHistory.invoiceId,
				versionNumber = invoiceChangeHistory.versionNumber,
				timeStamp = invoiceChangeHistory.timeStamp,
				userId = invoiceChangeHistory.userId,
				data = invoiceChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a InvoiceChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(InvoiceChangeHistory invoiceChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (invoiceChangeHistory == null)
			{
				return null;
			}

			return new {
				id = invoiceChangeHistory.id,
				invoiceId = invoiceChangeHistory.invoiceId,
				versionNumber = invoiceChangeHistory.versionNumber,
				timeStamp = invoiceChangeHistory.timeStamp,
				userId = invoiceChangeHistory.userId,
				data = invoiceChangeHistory.data,
				invoice = Invoice.CreateMinimalAnonymous(invoiceChangeHistory.invoice)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a InvoiceChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(InvoiceChangeHistory invoiceChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (invoiceChangeHistory == null)
			{
				return null;
			}

			return new {
				id = invoiceChangeHistory.id,
				name = invoiceChangeHistory.id,
				description = invoiceChangeHistory.id
			 };
		}
	}
}
