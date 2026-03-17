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
	public partial class GeneralLedgerLine : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class GeneralLedgerLineDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 generalLedgerEntryId { get; set; }
			[Required]
			public Int32 financialCategoryId { get; set; }
			[Required]
			public Decimal debitAmount { get; set; }
			[Required]
			public Decimal creditAmount { get; set; }
			public String description { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class GeneralLedgerLineOutputDTO : GeneralLedgerLineDTO
		{
			public FinancialCategory.FinancialCategoryDTO financialCategory { get; set; }
			public GeneralLedgerEntry.GeneralLedgerEntryDTO generalLedgerEntry { get; set; }
		}


		/// <summary>
		///
		/// Converts a GeneralLedgerLine to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public GeneralLedgerLineDTO ToDTO()
		{
			return new GeneralLedgerLineDTO
			{
				id = this.id,
				generalLedgerEntryId = this.generalLedgerEntryId,
				financialCategoryId = this.financialCategoryId,
				debitAmount = this.debitAmount,
				creditAmount = this.creditAmount,
				description = this.description
			};
		}


		/// <summary>
		///
		/// Converts a GeneralLedgerLine list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<GeneralLedgerLineDTO> ToDTOList(List<GeneralLedgerLine> data)
		{
			if (data == null)
			{
				return null;
			}

			List<GeneralLedgerLineDTO> output = new List<GeneralLedgerLineDTO>();

			output.Capacity = data.Count;

			foreach (GeneralLedgerLine generalLedgerLine in data)
			{
				output.Add(generalLedgerLine.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a GeneralLedgerLine to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the GeneralLedgerLine Entity type directly.
		///
		/// </summary>
		public GeneralLedgerLineOutputDTO ToOutputDTO()
		{
			return new GeneralLedgerLineOutputDTO
			{
				id = this.id,
				generalLedgerEntryId = this.generalLedgerEntryId,
				financialCategoryId = this.financialCategoryId,
				debitAmount = this.debitAmount,
				creditAmount = this.creditAmount,
				description = this.description,
				financialCategory = this.financialCategory?.ToDTO(),
				generalLedgerEntry = this.generalLedgerEntry?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a GeneralLedgerLine list to list of Output Data Transfer Object intended to be used for serializing a list of GeneralLedgerLine objects to avoid using the GeneralLedgerLine entity type directly.
		///
		/// </summary>
		public static List<GeneralLedgerLineOutputDTO> ToOutputDTOList(List<GeneralLedgerLine> data)
		{
			if (data == null)
			{
				return null;
			}

			List<GeneralLedgerLineOutputDTO> output = new List<GeneralLedgerLineOutputDTO>();

			output.Capacity = data.Count;

			foreach (GeneralLedgerLine generalLedgerLine in data)
			{
				output.Add(generalLedgerLine.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a GeneralLedgerLine Object.
		///
		/// </summary>
		public static Database.GeneralLedgerLine FromDTO(GeneralLedgerLineDTO dto)
		{
			return new Database.GeneralLedgerLine
			{
				id = dto.id,
				generalLedgerEntryId = dto.generalLedgerEntryId,
				financialCategoryId = dto.financialCategoryId,
				debitAmount = dto.debitAmount,
				creditAmount = dto.creditAmount,
				description = dto.description
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a GeneralLedgerLine Object.
		///
		/// </summary>
		public void ApplyDTO(GeneralLedgerLineDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.generalLedgerEntryId = dto.generalLedgerEntryId;
			this.financialCategoryId = dto.financialCategoryId;
			this.debitAmount = dto.debitAmount;
			this.creditAmount = dto.creditAmount;
			this.description = dto.description;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a GeneralLedgerLine Object.
		///
		/// </summary>
		public GeneralLedgerLine Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new GeneralLedgerLine{
				id = this.id,
				generalLedgerEntryId = this.generalLedgerEntryId,
				financialCategoryId = this.financialCategoryId,
				debitAmount = this.debitAmount,
				creditAmount = this.creditAmount,
				description = this.description,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a GeneralLedgerLine Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a GeneralLedgerLine Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a GeneralLedgerLine Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a GeneralLedgerLine Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.GeneralLedgerLine generalLedgerLine)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (generalLedgerLine == null)
			{
				return null;
			}

			return new {
				id = generalLedgerLine.id,
				generalLedgerEntryId = generalLedgerLine.generalLedgerEntryId,
				financialCategoryId = generalLedgerLine.financialCategoryId,
				debitAmount = generalLedgerLine.debitAmount,
				creditAmount = generalLedgerLine.creditAmount,
				description = generalLedgerLine.description,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a GeneralLedgerLine Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(GeneralLedgerLine generalLedgerLine)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (generalLedgerLine == null)
			{
				return null;
			}

			return new {
				id = generalLedgerLine.id,
				generalLedgerEntryId = generalLedgerLine.generalLedgerEntryId,
				financialCategoryId = generalLedgerLine.financialCategoryId,
				debitAmount = generalLedgerLine.debitAmount,
				creditAmount = generalLedgerLine.creditAmount,
				description = generalLedgerLine.description,
				financialCategory = FinancialCategory.CreateMinimalAnonymous(generalLedgerLine.financialCategory),
				generalLedgerEntry = GeneralLedgerEntry.CreateMinimalAnonymous(generalLedgerLine.generalLedgerEntry)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a GeneralLedgerLine Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(GeneralLedgerLine generalLedgerLine)
		{
			//
			// Return a very minimal object.
			//
			if (generalLedgerLine == null)
			{
				return null;
			}

			return new {
				id = generalLedgerLine.id,
				description = generalLedgerLine.description,
				name = generalLedgerLine.description
			 };
		}
	}
}
