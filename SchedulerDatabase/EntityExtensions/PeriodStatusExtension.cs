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
	public partial class PeriodStatus : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class PeriodStatusDTO
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
		public class PeriodStatusOutputDTO : PeriodStatusDTO
		{
		}


		/// <summary>
		///
		/// Converts a PeriodStatus to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public PeriodStatusDTO ToDTO()
		{
			return new PeriodStatusDTO
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
		/// Converts a PeriodStatus list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<PeriodStatusDTO> ToDTOList(List<PeriodStatus> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PeriodStatusDTO> output = new List<PeriodStatusDTO>();

			output.Capacity = data.Count;

			foreach (PeriodStatus periodStatus in data)
			{
				output.Add(periodStatus.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a PeriodStatus to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the PeriodStatusEntity type directly.
		///
		/// </summary>
		public PeriodStatusOutputDTO ToOutputDTO()
		{
			return new PeriodStatusOutputDTO
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
		/// Converts a PeriodStatus list to list of Output Data Transfer Object intended to be used for serializing a list of PeriodStatus objects to avoid using the PeriodStatus entity type directly.
		///
		/// </summary>
		public static List<PeriodStatusOutputDTO> ToOutputDTOList(List<PeriodStatus> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PeriodStatusOutputDTO> output = new List<PeriodStatusOutputDTO>();

			output.Capacity = data.Count;

			foreach (PeriodStatus periodStatus in data)
			{
				output.Add(periodStatus.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a PeriodStatus Object.
		///
		/// </summary>
		public static Database.PeriodStatus FromDTO(PeriodStatusDTO dto)
		{
			return new Database.PeriodStatus
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
		/// Applies the values from an INPUT DTO to a PeriodStatus Object.
		///
		/// </summary>
		public void ApplyDTO(PeriodStatusDTO dto)
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
		/// Creates a deep copy clone of a PeriodStatus Object.
		///
		/// </summary>
		public PeriodStatus Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new PeriodStatus{
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
        /// Creates an anonymous object containing properties from a PeriodStatus Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PeriodStatus Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a PeriodStatus Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a PeriodStatus Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.PeriodStatus periodStatus)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (periodStatus == null)
			{
				return null;
			}

			return new {
				id = periodStatus.id,
				name = periodStatus.name,
				description = periodStatus.description,
				color = periodStatus.color,
				sequence = periodStatus.sequence,
				objectGuid = periodStatus.objectGuid,
				active = periodStatus.active,
				deleted = periodStatus.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a PeriodStatus Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(PeriodStatus periodStatus)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (periodStatus == null)
			{
				return null;
			}

			return new {
				id = periodStatus.id,
				name = periodStatus.name,
				description = periodStatus.description,
				color = periodStatus.color,
				sequence = periodStatus.sequence,
				objectGuid = periodStatus.objectGuid,
				active = periodStatus.active,
				deleted = periodStatus.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a PeriodStatus Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(PeriodStatus periodStatus)
		{
			//
			// Return a very minimal object.
			//
			if (periodStatus == null)
			{
				return null;
			}

			return new {
				id = periodStatus.id,
				name = periodStatus.name,
				description = periodStatus.description,
			 };
		}
	}
}
