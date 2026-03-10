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
	public partial class InvoiceStatus : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class InvoiceStatusDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
			public String color { get; set; }
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
		public class InvoiceStatusOutputDTO : InvoiceStatusDTO
		{
		}


		/// <summary>
		///
		/// Converts a InvoiceStatus to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public InvoiceStatusDTO ToDTO()
		{
			return new InvoiceStatusDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				color = this.color,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a InvoiceStatus list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<InvoiceStatusDTO> ToDTOList(List<InvoiceStatus> data)
		{
			if (data == null)
			{
				return null;
			}

			List<InvoiceStatusDTO> output = new List<InvoiceStatusDTO>();

			output.Capacity = data.Count;

			foreach (InvoiceStatus invoiceStatus in data)
			{
				output.Add(invoiceStatus.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a InvoiceStatus to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the InvoiceStatusEntity type directly.
		///
		/// </summary>
		public InvoiceStatusOutputDTO ToOutputDTO()
		{
			return new InvoiceStatusOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				color = this.color,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a InvoiceStatus list to list of Output Data Transfer Object intended to be used for serializing a list of InvoiceStatus objects to avoid using the InvoiceStatus entity type directly.
		///
		/// </summary>
		public static List<InvoiceStatusOutputDTO> ToOutputDTOList(List<InvoiceStatus> data)
		{
			if (data == null)
			{
				return null;
			}

			List<InvoiceStatusOutputDTO> output = new List<InvoiceStatusOutputDTO>();

			output.Capacity = data.Count;

			foreach (InvoiceStatus invoiceStatus in data)
			{
				output.Add(invoiceStatus.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a InvoiceStatus Object.
		///
		/// </summary>
		public static Database.InvoiceStatus FromDTO(InvoiceStatusDTO dto)
		{
			return new Database.InvoiceStatus
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				color = dto.color,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a InvoiceStatus Object.
		///
		/// </summary>
		public void ApplyDTO(InvoiceStatusDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.color = dto.color;
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
		/// Creates a deep copy clone of a InvoiceStatus Object.
		///
		/// </summary>
		public InvoiceStatus Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new InvoiceStatus{
				id = this.id,
				name = this.name,
				description = this.description,
				color = this.color,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a InvoiceStatus Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a InvoiceStatus Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a InvoiceStatus Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a InvoiceStatus Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.InvoiceStatus invoiceStatus)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (invoiceStatus == null)
			{
				return null;
			}

			return new {
				id = invoiceStatus.id,
				name = invoiceStatus.name,
				description = invoiceStatus.description,
				color = invoiceStatus.color,
				sequence = invoiceStatus.sequence,
				objectGuid = invoiceStatus.objectGuid,
				active = invoiceStatus.active,
				deleted = invoiceStatus.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a InvoiceStatus Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(InvoiceStatus invoiceStatus)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (invoiceStatus == null)
			{
				return null;
			}

			return new {
				id = invoiceStatus.id,
				name = invoiceStatus.name,
				description = invoiceStatus.description,
				color = invoiceStatus.color,
				sequence = invoiceStatus.sequence,
				objectGuid = invoiceStatus.objectGuid,
				active = invoiceStatus.active,
				deleted = invoiceStatus.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a InvoiceStatus Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(InvoiceStatus invoiceStatus)
		{
			//
			// Return a very minimal object.
			//
			if (invoiceStatus == null)
			{
				return null;
			}

			return new {
				id = invoiceStatus.id,
				name = invoiceStatus.name,
				description = invoiceStatus.description,
			 };
		}
	}
}
