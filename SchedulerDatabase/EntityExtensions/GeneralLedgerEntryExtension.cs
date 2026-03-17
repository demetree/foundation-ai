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
	public partial class GeneralLedgerEntry : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class GeneralLedgerEntryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 journalEntryNumber { get; set; }
			[Required]
			public DateTime transactionDate { get; set; }
			public String description { get; set; }
			public String referenceNumber { get; set; }
			public Int32? financialTransactionId { get; set; }
			public Int32? fiscalPeriodId { get; set; }
			public Int32? financialOfficeId { get; set; }
			[Required]
			public Int32 postedBy { get; set; }
			[Required]
			public DateTime postedDate { get; set; }
			public Int32? reversalOfId { get; set; }
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
		public class GeneralLedgerEntryOutputDTO : GeneralLedgerEntryDTO
		{
			public FinancialOffice.FinancialOfficeDTO financialOffice { get; set; }
			public FinancialTransaction.FinancialTransactionDTO financialTransaction { get; set; }
			public FiscalPeriod.FiscalPeriodDTO fiscalPeriod { get; set; }
		}


		/// <summary>
		///
		/// Converts a GeneralLedgerEntry to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public GeneralLedgerEntryDTO ToDTO()
		{
			return new GeneralLedgerEntryDTO
			{
				id = this.id,
				journalEntryNumber = this.journalEntryNumber,
				transactionDate = this.transactionDate,
				description = this.description,
				referenceNumber = this.referenceNumber,
				financialTransactionId = this.financialTransactionId,
				fiscalPeriodId = this.fiscalPeriodId,
				financialOfficeId = this.financialOfficeId,
				postedBy = this.postedBy,
				postedDate = this.postedDate,
				reversalOfId = this.reversalOfId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a GeneralLedgerEntry list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<GeneralLedgerEntryDTO> ToDTOList(List<GeneralLedgerEntry> data)
		{
			if (data == null)
			{
				return null;
			}

			List<GeneralLedgerEntryDTO> output = new List<GeneralLedgerEntryDTO>();

			output.Capacity = data.Count;

			foreach (GeneralLedgerEntry generalLedgerEntry in data)
			{
				output.Add(generalLedgerEntry.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a GeneralLedgerEntry to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the GeneralLedgerEntry Entity type directly.
		///
		/// </summary>
		public GeneralLedgerEntryOutputDTO ToOutputDTO()
		{
			return new GeneralLedgerEntryOutputDTO
			{
				id = this.id,
				journalEntryNumber = this.journalEntryNumber,
				transactionDate = this.transactionDate,
				description = this.description,
				referenceNumber = this.referenceNumber,
				financialTransactionId = this.financialTransactionId,
				fiscalPeriodId = this.fiscalPeriodId,
				financialOfficeId = this.financialOfficeId,
				postedBy = this.postedBy,
				postedDate = this.postedDate,
				reversalOfId = this.reversalOfId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				financialOffice = this.financialOffice?.ToDTO(),
				financialTransaction = this.financialTransaction?.ToDTO(),
				fiscalPeriod = this.fiscalPeriod?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a GeneralLedgerEntry list to list of Output Data Transfer Object intended to be used for serializing a list of GeneralLedgerEntry objects to avoid using the GeneralLedgerEntry entity type directly.
		///
		/// </summary>
		public static List<GeneralLedgerEntryOutputDTO> ToOutputDTOList(List<GeneralLedgerEntry> data)
		{
			if (data == null)
			{
				return null;
			}

			List<GeneralLedgerEntryOutputDTO> output = new List<GeneralLedgerEntryOutputDTO>();

			output.Capacity = data.Count;

			foreach (GeneralLedgerEntry generalLedgerEntry in data)
			{
				output.Add(generalLedgerEntry.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a GeneralLedgerEntry Object.
		///
		/// </summary>
		public static Database.GeneralLedgerEntry FromDTO(GeneralLedgerEntryDTO dto)
		{
			return new Database.GeneralLedgerEntry
			{
				id = dto.id,
				journalEntryNumber = dto.journalEntryNumber,
				transactionDate = dto.transactionDate,
				description = dto.description,
				referenceNumber = dto.referenceNumber,
				financialTransactionId = dto.financialTransactionId,
				fiscalPeriodId = dto.fiscalPeriodId,
				financialOfficeId = dto.financialOfficeId,
				postedBy = dto.postedBy,
				postedDate = dto.postedDate,
				reversalOfId = dto.reversalOfId,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a GeneralLedgerEntry Object.
		///
		/// </summary>
		public void ApplyDTO(GeneralLedgerEntryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.journalEntryNumber = dto.journalEntryNumber;
			this.transactionDate = dto.transactionDate;
			this.description = dto.description;
			this.referenceNumber = dto.referenceNumber;
			this.financialTransactionId = dto.financialTransactionId;
			this.fiscalPeriodId = dto.fiscalPeriodId;
			this.financialOfficeId = dto.financialOfficeId;
			this.postedBy = dto.postedBy;
			this.postedDate = dto.postedDate;
			this.reversalOfId = dto.reversalOfId;
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
		/// Creates a deep copy clone of a GeneralLedgerEntry Object.
		///
		/// </summary>
		public GeneralLedgerEntry Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new GeneralLedgerEntry{
				id = this.id,
				tenantGuid = this.tenantGuid,
				journalEntryNumber = this.journalEntryNumber,
				transactionDate = this.transactionDate,
				description = this.description,
				referenceNumber = this.referenceNumber,
				financialTransactionId = this.financialTransactionId,
				fiscalPeriodId = this.fiscalPeriodId,
				financialOfficeId = this.financialOfficeId,
				postedBy = this.postedBy,
				postedDate = this.postedDate,
				reversalOfId = this.reversalOfId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a GeneralLedgerEntry Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a GeneralLedgerEntry Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a GeneralLedgerEntry Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a GeneralLedgerEntry Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.GeneralLedgerEntry generalLedgerEntry)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (generalLedgerEntry == null)
			{
				return null;
			}

			return new {
				id = generalLedgerEntry.id,
				journalEntryNumber = generalLedgerEntry.journalEntryNumber,
				transactionDate = generalLedgerEntry.transactionDate,
				description = generalLedgerEntry.description,
				referenceNumber = generalLedgerEntry.referenceNumber,
				financialTransactionId = generalLedgerEntry.financialTransactionId,
				fiscalPeriodId = generalLedgerEntry.fiscalPeriodId,
				financialOfficeId = generalLedgerEntry.financialOfficeId,
				postedBy = generalLedgerEntry.postedBy,
				postedDate = generalLedgerEntry.postedDate,
				reversalOfId = generalLedgerEntry.reversalOfId,
				objectGuid = generalLedgerEntry.objectGuid,
				active = generalLedgerEntry.active,
				deleted = generalLedgerEntry.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a GeneralLedgerEntry Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(GeneralLedgerEntry generalLedgerEntry)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (generalLedgerEntry == null)
			{
				return null;
			}

			return new {
				id = generalLedgerEntry.id,
				journalEntryNumber = generalLedgerEntry.journalEntryNumber,
				transactionDate = generalLedgerEntry.transactionDate,
				description = generalLedgerEntry.description,
				referenceNumber = generalLedgerEntry.referenceNumber,
				financialTransactionId = generalLedgerEntry.financialTransactionId,
				fiscalPeriodId = generalLedgerEntry.fiscalPeriodId,
				financialOfficeId = generalLedgerEntry.financialOfficeId,
				postedBy = generalLedgerEntry.postedBy,
				postedDate = generalLedgerEntry.postedDate,
				reversalOfId = generalLedgerEntry.reversalOfId,
				objectGuid = generalLedgerEntry.objectGuid,
				active = generalLedgerEntry.active,
				deleted = generalLedgerEntry.deleted,
				financialOffice = FinancialOffice.CreateMinimalAnonymous(generalLedgerEntry.financialOffice),
				financialTransaction = FinancialTransaction.CreateMinimalAnonymous(generalLedgerEntry.financialTransaction),
				fiscalPeriod = FiscalPeriod.CreateMinimalAnonymous(generalLedgerEntry.fiscalPeriod)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a GeneralLedgerEntry Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(GeneralLedgerEntry generalLedgerEntry)
		{
			//
			// Return a very minimal object.
			//
			if (generalLedgerEntry == null)
			{
				return null;
			}

			return new {
				id = generalLedgerEntry.id,
				description = generalLedgerEntry.description,
				name = generalLedgerEntry.description
			 };
		}
	}
}
