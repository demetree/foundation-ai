using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Alerting.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class IncidentStatusType : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class IncidentStatusTypeDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			public String description { get; set; }
			public Boolean? active { get; set; }
			public Boolean? deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class IncidentStatusTypeOutputDTO : IncidentStatusTypeDTO
		{
		}


		/// <summary>
		///
		/// Converts a IncidentStatusType to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public IncidentStatusTypeDTO ToDTO()
		{
			return new IncidentStatusTypeDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a IncidentStatusType list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<IncidentStatusTypeDTO> ToDTOList(List<IncidentStatusType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<IncidentStatusTypeDTO> output = new List<IncidentStatusTypeDTO>();

			output.Capacity = data.Count;

			foreach (IncidentStatusType incidentStatusType in data)
			{
				output.Add(incidentStatusType.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a IncidentStatusType to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the IncidentStatusTypeEntity type directly.
		///
		/// </summary>
		public IncidentStatusTypeOutputDTO ToOutputDTO()
		{
			return new IncidentStatusTypeOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a IncidentStatusType list to list of Output Data Transfer Object intended to be used for serializing a list of IncidentStatusType objects to avoid using the IncidentStatusType entity type directly.
		///
		/// </summary>
		public static List<IncidentStatusTypeOutputDTO> ToOutputDTOList(List<IncidentStatusType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<IncidentStatusTypeOutputDTO> output = new List<IncidentStatusTypeOutputDTO>();

			output.Capacity = data.Count;

			foreach (IncidentStatusType incidentStatusType in data)
			{
				output.Add(incidentStatusType.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a IncidentStatusType Object.
		///
		/// </summary>
		public static Database.IncidentStatusType FromDTO(IncidentStatusTypeDTO dto)
		{
			return new Database.IncidentStatusType
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a IncidentStatusType Object.
		///
		/// </summary>
		public void ApplyDTO(IncidentStatusTypeDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
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
		/// Creates a deep copy clone of a IncidentStatusType Object.
		///
		/// </summary>
		public IncidentStatusType Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new IncidentStatusType{
				id = this.id,
				name = this.name,
				description = this.description,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a IncidentStatusType Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a IncidentStatusType Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a IncidentStatusType Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a IncidentStatusType Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.IncidentStatusType incidentStatusType)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (incidentStatusType == null)
			{
				return null;
			}

			return new {
				id = incidentStatusType.id,
				name = incidentStatusType.name,
				description = incidentStatusType.description,
				active = incidentStatusType.active,
				deleted = incidentStatusType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a IncidentStatusType Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(IncidentStatusType incidentStatusType)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (incidentStatusType == null)
			{
				return null;
			}

			return new {
				id = incidentStatusType.id,
				name = incidentStatusType.name,
				description = incidentStatusType.description,
				active = incidentStatusType.active,
				deleted = incidentStatusType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a IncidentStatusType Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(IncidentStatusType incidentStatusType)
		{
			//
			// Return a very minimal object.
			//
			if (incidentStatusType == null)
			{
				return null;
			}

			return new {
				id = incidentStatusType.id,
				name = incidentStatusType.name,
				description = incidentStatusType.description,
			 };
		}
	}
}
