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
	public partial class PaymentMethod : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class PaymentMethodDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
			[Required]
			public Boolean isElectronic { get; set; }
			public Int32? sequence { get; set; }
			public String color { get; set; }
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
		public class PaymentMethodOutputDTO : PaymentMethodDTO
		{
		}


		/// <summary>
		///
		/// Converts a PaymentMethod to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public PaymentMethodDTO ToDTO()
		{
			return new PaymentMethodDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				isElectronic = this.isElectronic,
				sequence = this.sequence,
				color = this.color,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a PaymentMethod list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<PaymentMethodDTO> ToDTOList(List<PaymentMethod> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PaymentMethodDTO> output = new List<PaymentMethodDTO>();

			output.Capacity = data.Count;

			foreach (PaymentMethod paymentMethod in data)
			{
				output.Add(paymentMethod.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a PaymentMethod to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the PaymentMethodEntity type directly.
		///
		/// </summary>
		public PaymentMethodOutputDTO ToOutputDTO()
		{
			return new PaymentMethodOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				isElectronic = this.isElectronic,
				sequence = this.sequence,
				color = this.color,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a PaymentMethod list to list of Output Data Transfer Object intended to be used for serializing a list of PaymentMethod objects to avoid using the PaymentMethod entity type directly.
		///
		/// </summary>
		public static List<PaymentMethodOutputDTO> ToOutputDTOList(List<PaymentMethod> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PaymentMethodOutputDTO> output = new List<PaymentMethodOutputDTO>();

			output.Capacity = data.Count;

			foreach (PaymentMethod paymentMethod in data)
			{
				output.Add(paymentMethod.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a PaymentMethod Object.
		///
		/// </summary>
		public static Database.PaymentMethod FromDTO(PaymentMethodDTO dto)
		{
			return new Database.PaymentMethod
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				isElectronic = dto.isElectronic,
				sequence = dto.sequence,
				color = dto.color,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a PaymentMethod Object.
		///
		/// </summary>
		public void ApplyDTO(PaymentMethodDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.isElectronic = dto.isElectronic;
			this.sequence = dto.sequence;
			this.color = dto.color;
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
		/// Creates a deep copy clone of a PaymentMethod Object.
		///
		/// </summary>
		public PaymentMethod Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new PaymentMethod{
				id = this.id,
				name = this.name,
				description = this.description,
				isElectronic = this.isElectronic,
				sequence = this.sequence,
				color = this.color,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PaymentMethod Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PaymentMethod Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a PaymentMethod Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a PaymentMethod Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.PaymentMethod paymentMethod)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (paymentMethod == null)
			{
				return null;
			}

			return new {
				id = paymentMethod.id,
				name = paymentMethod.name,
				description = paymentMethod.description,
				isElectronic = paymentMethod.isElectronic,
				sequence = paymentMethod.sequence,
				color = paymentMethod.color,
				objectGuid = paymentMethod.objectGuid,
				active = paymentMethod.active,
				deleted = paymentMethod.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a PaymentMethod Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(PaymentMethod paymentMethod)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (paymentMethod == null)
			{
				return null;
			}

			return new {
				id = paymentMethod.id,
				name = paymentMethod.name,
				description = paymentMethod.description,
				isElectronic = paymentMethod.isElectronic,
				sequence = paymentMethod.sequence,
				color = paymentMethod.color,
				objectGuid = paymentMethod.objectGuid,
				active = paymentMethod.active,
				deleted = paymentMethod.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a PaymentMethod Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(PaymentMethod paymentMethod)
		{
			//
			// Return a very minimal object.
			//
			if (paymentMethod == null)
			{
				return null;
			}

			return new {
				id = paymentMethod.id,
				name = paymentMethod.name,
				description = paymentMethod.description,
			 };
		}
	}
}
