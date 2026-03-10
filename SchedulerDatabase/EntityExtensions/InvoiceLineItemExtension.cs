using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Scheduler.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class InvoiceLineItem : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class InvoiceLineItemDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 invoiceId { get; set; }
			public Int32? eventChargeId { get; set; }
			public Int32? financialCategoryId { get; set; }
			[Required]
			public String description { get; set; }
			[Required]
			public Decimal quantity { get; set; }
			[Required]
			public Decimal unitPrice { get; set; }
			[Required]
			public Decimal amount { get; set; }
			[Required]
			public Decimal taxAmount { get; set; }
			[Required]
			public Decimal totalAmount { get; set; }
			public Int32? sequence { get; set; }
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
		public class InvoiceLineItemOutputDTO : InvoiceLineItemDTO
		{
			public EventCharge.EventChargeDTO eventCharge { get; set; }
			public FinancialCategory.FinancialCategoryDTO financialCategory { get; set; }
			public Invoice.InvoiceDTO invoice { get; set; }
		}


		/// <summary>
		///
		/// Converts a InvoiceLineItem to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public InvoiceLineItemDTO ToDTO()
		{
			return new InvoiceLineItemDTO
			{
				id = this.id,
				invoiceId = this.invoiceId,
				eventChargeId = this.eventChargeId,
				financialCategoryId = this.financialCategoryId,
				description = this.description,
				quantity = this.quantity,
				unitPrice = this.unitPrice,
				amount = this.amount,
				taxAmount = this.taxAmount,
				totalAmount = this.totalAmount,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a InvoiceLineItem list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<InvoiceLineItemDTO> ToDTOList(List<InvoiceLineItem> data)
		{
			if (data == null)
			{
				return null;
			}

			List<InvoiceLineItemDTO> output = new List<InvoiceLineItemDTO>();

			output.Capacity = data.Count;

			foreach (InvoiceLineItem invoiceLineItem in data)
			{
				output.Add(invoiceLineItem.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a InvoiceLineItem to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the InvoiceLineItemEntity type directly.
		///
		/// </summary>
		public InvoiceLineItemOutputDTO ToOutputDTO()
		{
			return new InvoiceLineItemOutputDTO
			{
				id = this.id,
				invoiceId = this.invoiceId,
				eventChargeId = this.eventChargeId,
				financialCategoryId = this.financialCategoryId,
				description = this.description,
				quantity = this.quantity,
				unitPrice = this.unitPrice,
				amount = this.amount,
				taxAmount = this.taxAmount,
				totalAmount = this.totalAmount,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				eventCharge = this.eventCharge?.ToDTO(),
				financialCategory = this.financialCategory?.ToDTO(),
				invoice = this.invoice?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a InvoiceLineItem list to list of Output Data Transfer Object intended to be used for serializing a list of InvoiceLineItem objects to avoid using the InvoiceLineItem entity type directly.
		///
		/// </summary>
		public static List<InvoiceLineItemOutputDTO> ToOutputDTOList(List<InvoiceLineItem> data)
		{
			if (data == null)
			{
				return null;
			}

			List<InvoiceLineItemOutputDTO> output = new List<InvoiceLineItemOutputDTO>();

			output.Capacity = data.Count;

			foreach (InvoiceLineItem invoiceLineItem in data)
			{
				output.Add(invoiceLineItem.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a InvoiceLineItem Object.
		///
		/// </summary>
		public static Database.InvoiceLineItem FromDTO(InvoiceLineItemDTO dto)
		{
			return new Database.InvoiceLineItem
			{
				id = dto.id,
				invoiceId = dto.invoiceId,
				eventChargeId = dto.eventChargeId,
				financialCategoryId = dto.financialCategoryId,
				description = dto.description,
				quantity = dto.quantity,
				unitPrice = dto.unitPrice,
				amount = dto.amount,
				taxAmount = dto.taxAmount,
				totalAmount = dto.totalAmount,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a InvoiceLineItem Object.
		///
		/// </summary>
		public void ApplyDTO(InvoiceLineItemDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.invoiceId = dto.invoiceId;
			this.eventChargeId = dto.eventChargeId;
			this.financialCategoryId = dto.financialCategoryId;
			this.description = dto.description;
			this.quantity = dto.quantity;
			this.unitPrice = dto.unitPrice;
			this.amount = dto.amount;
			this.taxAmount = dto.taxAmount;
			this.totalAmount = dto.totalAmount;
			this.sequence = dto.sequence;
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
		/// Creates a deep copy clone of a InvoiceLineItem Object.
		///
		/// </summary>
		public InvoiceLineItem Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new InvoiceLineItem{
				id = this.id,
				tenantGuid = this.tenantGuid,
				invoiceId = this.invoiceId,
				eventChargeId = this.eventChargeId,
				financialCategoryId = this.financialCategoryId,
				description = this.description,
				quantity = this.quantity,
				unitPrice = this.unitPrice,
				amount = this.amount,
				taxAmount = this.taxAmount,
				totalAmount = this.totalAmount,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a InvoiceLineItem Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a InvoiceLineItem Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a InvoiceLineItem Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a InvoiceLineItem Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.InvoiceLineItem invoiceLineItem)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (invoiceLineItem == null)
			{
				return null;
			}

			return new {
				id = invoiceLineItem.id,
				invoiceId = invoiceLineItem.invoiceId,
				eventChargeId = invoiceLineItem.eventChargeId,
				financialCategoryId = invoiceLineItem.financialCategoryId,
				description = invoiceLineItem.description,
				quantity = invoiceLineItem.quantity,
				unitPrice = invoiceLineItem.unitPrice,
				amount = invoiceLineItem.amount,
				taxAmount = invoiceLineItem.taxAmount,
				totalAmount = invoiceLineItem.totalAmount,
				sequence = invoiceLineItem.sequence,
				objectGuid = invoiceLineItem.objectGuid,
				active = invoiceLineItem.active,
				deleted = invoiceLineItem.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a InvoiceLineItem Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(InvoiceLineItem invoiceLineItem)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (invoiceLineItem == null)
			{
				return null;
			}

			return new {
				id = invoiceLineItem.id,
				invoiceId = invoiceLineItem.invoiceId,
				eventChargeId = invoiceLineItem.eventChargeId,
				financialCategoryId = invoiceLineItem.financialCategoryId,
				description = invoiceLineItem.description,
				quantity = invoiceLineItem.quantity,
				unitPrice = invoiceLineItem.unitPrice,
				amount = invoiceLineItem.amount,
				taxAmount = invoiceLineItem.taxAmount,
				totalAmount = invoiceLineItem.totalAmount,
				sequence = invoiceLineItem.sequence,
				objectGuid = invoiceLineItem.objectGuid,
				active = invoiceLineItem.active,
				deleted = invoiceLineItem.deleted,
				eventCharge = EventCharge.CreateMinimalAnonymous(invoiceLineItem.eventCharge),
				financialCategory = FinancialCategory.CreateMinimalAnonymous(invoiceLineItem.financialCategory),
				invoice = Invoice.CreateMinimalAnonymous(invoiceLineItem.invoice)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a InvoiceLineItem Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(InvoiceLineItem invoiceLineItem)
		{
			//
			// Return a very minimal object.
			//
			if (invoiceLineItem == null)
			{
				return null;
			}

			return new {
				id = invoiceLineItem.id,
				description = invoiceLineItem.description,
				name = invoiceLineItem.description
			 };
		}
	}
}
