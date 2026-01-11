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
	public partial class Qualification : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class QualificationDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
			public Boolean? isLicense { get; set; }
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
		public class QualificationOutputDTO : QualificationDTO
		{
		}


		/// <summary>
		///
		/// Converts a Qualification to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public QualificationDTO ToDTO()
		{
			return new QualificationDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				isLicense = this.isLicense,
				color = this.color,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a Qualification list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<QualificationDTO> ToDTOList(List<Qualification> data)
		{
			if (data == null)
			{
				return null;
			}

			List<QualificationDTO> output = new List<QualificationDTO>();

			output.Capacity = data.Count;

			foreach (Qualification qualification in data)
			{
				output.Add(qualification.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a Qualification to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the QualificationEntity type directly.
		///
		/// </summary>
		public QualificationOutputDTO ToOutputDTO()
		{
			return new QualificationOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				isLicense = this.isLicense,
				color = this.color,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a Qualification list to list of Output Data Transfer Object intended to be used for serializing a list of Qualification objects to avoid using the Qualification entity type directly.
		///
		/// </summary>
		public static List<QualificationOutputDTO> ToOutputDTOList(List<Qualification> data)
		{
			if (data == null)
			{
				return null;
			}

			List<QualificationOutputDTO> output = new List<QualificationOutputDTO>();

			output.Capacity = data.Count;

			foreach (Qualification qualification in data)
			{
				output.Add(qualification.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a Qualification Object.
		///
		/// </summary>
		public static Database.Qualification FromDTO(QualificationDTO dto)
		{
			return new Database.Qualification
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				isLicense = dto.isLicense,
				color = dto.color,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a Qualification Object.
		///
		/// </summary>
		public void ApplyDTO(QualificationDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.isLicense = dto.isLicense;
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
		/// Creates a deep copy clone of a Qualification Object.
		///
		/// </summary>
		public Qualification Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new Qualification{
				id = this.id,
				tenantGuid = this.tenantGuid,
				name = this.name,
				description = this.description,
				isLicense = this.isLicense,
				color = this.color,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Qualification Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Qualification Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a Qualification Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a Qualification Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.Qualification qualification)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (qualification == null)
			{
				return null;
			}

			return new {
				id = qualification.id,
				name = qualification.name,
				description = qualification.description,
				isLicense = qualification.isLicense,
				color = qualification.color,
				sequence = qualification.sequence,
				objectGuid = qualification.objectGuid,
				active = qualification.active,
				deleted = qualification.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a Qualification Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(Qualification qualification)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (qualification == null)
			{
				return null;
			}

			return new {
				id = qualification.id,
				name = qualification.name,
				description = qualification.description,
				isLicense = qualification.isLicense,
				color = qualification.color,
				sequence = qualification.sequence,
				objectGuid = qualification.objectGuid,
				active = qualification.active,
				deleted = qualification.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a Qualification Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(Qualification qualification)
		{
			//
			// Return a very minimal object.
			//
			if (qualification == null)
			{
				return null;
			}

			return new {
				id = qualification.id,
				name = qualification.name,
				description = qualification.description,
			 };
		}
	}
}
