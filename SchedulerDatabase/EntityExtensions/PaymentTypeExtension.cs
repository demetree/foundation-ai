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
	public partial class PaymentType : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class PaymentTypeDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
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
		public class PaymentTypeOutputDTO : PaymentTypeDTO
		{
		}


		/// <summary>
		///
		/// Converts a PaymentType to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public PaymentTypeDTO ToDTO()
		{
			return new PaymentTypeDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a PaymentType list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<PaymentTypeDTO> ToDTOList(List<PaymentType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PaymentTypeDTO> output = new List<PaymentTypeDTO>();

			output.Capacity = data.Count;

			foreach (PaymentType paymentType in data)
			{
				output.Add(paymentType.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a PaymentType to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the PaymentTypeEntity type directly.
		///
		/// </summary>
		public PaymentTypeOutputDTO ToOutputDTO()
		{
			return new PaymentTypeOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a PaymentType list to list of Output Data Transfer Object intended to be used for serializing a list of PaymentType objects to avoid using the PaymentType entity type directly.
		///
		/// </summary>
		public static List<PaymentTypeOutputDTO> ToOutputDTOList(List<PaymentType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PaymentTypeOutputDTO> output = new List<PaymentTypeOutputDTO>();

			output.Capacity = data.Count;

			foreach (PaymentType paymentType in data)
			{
				output.Add(paymentType.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a PaymentType Object.
		///
		/// </summary>
		public static Database.PaymentType FromDTO(PaymentTypeDTO dto)
		{
			return new Database.PaymentType
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a PaymentType Object.
		///
		/// </summary>
		public void ApplyDTO(PaymentTypeDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
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
		/// Creates a deep copy clone of a PaymentType Object.
		///
		/// </summary>
		public PaymentType Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new PaymentType{
				id = this.id,
				name = this.name,
				description = this.description,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PaymentType Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PaymentType Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a PaymentType Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a PaymentType Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.PaymentType paymentType)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (paymentType == null)
			{
				return null;
			}

			return new {
				id = paymentType.id,
				name = paymentType.name,
				description = paymentType.description,
				sequence = paymentType.sequence,
				objectGuid = paymentType.objectGuid,
				active = paymentType.active,
				deleted = paymentType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a PaymentType Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(PaymentType paymentType)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (paymentType == null)
			{
				return null;
			}

			return new {
				id = paymentType.id,
				name = paymentType.name,
				description = paymentType.description,
				sequence = paymentType.sequence,
				objectGuid = paymentType.objectGuid,
				active = paymentType.active,
				deleted = paymentType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a PaymentType Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(PaymentType paymentType)
		{
			//
			// Return a very minimal object.
			//
			if (paymentType == null)
			{
				return null;
			}

			return new {
				id = paymentType.id,
				name = paymentType.name,
				description = paymentType.description,
			 };
		}
	}
}
