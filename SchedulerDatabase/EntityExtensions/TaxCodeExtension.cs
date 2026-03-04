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
	public partial class TaxCode : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class TaxCodeDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
			[Required]
			public String code { get; set; }
			[Required]
			public Decimal rate { get; set; }
			[Required]
			public Boolean isDefault { get; set; }
			[Required]
			public Boolean isExempt { get; set; }
			public String externalTaxCodeId { get; set; }
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
		public class TaxCodeOutputDTO : TaxCodeDTO
		{
		}


		/// <summary>
		///
		/// Converts a TaxCode to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public TaxCodeDTO ToDTO()
		{
			return new TaxCodeDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				code = this.code,
				rate = this.rate,
				isDefault = this.isDefault,
				isExempt = this.isExempt,
				externalTaxCodeId = this.externalTaxCodeId,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a TaxCode list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<TaxCodeDTO> ToDTOList(List<TaxCode> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TaxCodeDTO> output = new List<TaxCodeDTO>();

			output.Capacity = data.Count;

			foreach (TaxCode taxCode in data)
			{
				output.Add(taxCode.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a TaxCode to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the TaxCodeEntity type directly.
		///
		/// </summary>
		public TaxCodeOutputDTO ToOutputDTO()
		{
			return new TaxCodeOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				code = this.code,
				rate = this.rate,
				isDefault = this.isDefault,
				isExempt = this.isExempt,
				externalTaxCodeId = this.externalTaxCodeId,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a TaxCode list to list of Output Data Transfer Object intended to be used for serializing a list of TaxCode objects to avoid using the TaxCode entity type directly.
		///
		/// </summary>
		public static List<TaxCodeOutputDTO> ToOutputDTOList(List<TaxCode> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TaxCodeOutputDTO> output = new List<TaxCodeOutputDTO>();

			output.Capacity = data.Count;

			foreach (TaxCode taxCode in data)
			{
				output.Add(taxCode.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a TaxCode Object.
		///
		/// </summary>
		public static Database.TaxCode FromDTO(TaxCodeDTO dto)
		{
			return new Database.TaxCode
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				code = dto.code,
				rate = dto.rate,
				isDefault = dto.isDefault,
				isExempt = dto.isExempt,
				externalTaxCodeId = dto.externalTaxCodeId,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a TaxCode Object.
		///
		/// </summary>
		public void ApplyDTO(TaxCodeDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.code = dto.code;
			this.rate = dto.rate;
			this.isDefault = dto.isDefault;
			this.isExempt = dto.isExempt;
			this.externalTaxCodeId = dto.externalTaxCodeId;
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
		/// Creates a deep copy clone of a TaxCode Object.
		///
		/// </summary>
		public TaxCode Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new TaxCode{
				id = this.id,
				tenantGuid = this.tenantGuid,
				name = this.name,
				description = this.description,
				code = this.code,
				rate = this.rate,
				isDefault = this.isDefault,
				isExempt = this.isExempt,
				externalTaxCodeId = this.externalTaxCodeId,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TaxCode Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TaxCode Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a TaxCode Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a TaxCode Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.TaxCode taxCode)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (taxCode == null)
			{
				return null;
			}

			return new {
				id = taxCode.id,
				name = taxCode.name,
				description = taxCode.description,
				code = taxCode.code,
				rate = taxCode.rate,
				isDefault = taxCode.isDefault,
				isExempt = taxCode.isExempt,
				externalTaxCodeId = taxCode.externalTaxCodeId,
				sequence = taxCode.sequence,
				objectGuid = taxCode.objectGuid,
				active = taxCode.active,
				deleted = taxCode.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a TaxCode Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(TaxCode taxCode)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (taxCode == null)
			{
				return null;
			}

			return new {
				id = taxCode.id,
				name = taxCode.name,
				description = taxCode.description,
				code = taxCode.code,
				rate = taxCode.rate,
				isDefault = taxCode.isDefault,
				isExempt = taxCode.isExempt,
				externalTaxCodeId = taxCode.externalTaxCodeId,
				sequence = taxCode.sequence,
				objectGuid = taxCode.objectGuid,
				active = taxCode.active,
				deleted = taxCode.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a TaxCode Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(TaxCode taxCode)
		{
			//
			// Return a very minimal object.
			//
			if (taxCode == null)
			{
				return null;
			}

			return new {
				id = taxCode.id,
				name = taxCode.name,
				description = taxCode.description,
			 };
		}
	}
}
