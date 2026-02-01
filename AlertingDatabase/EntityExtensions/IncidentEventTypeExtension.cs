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
	public partial class IncidentEventType : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class IncidentEventTypeDTO
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
		public class IncidentEventTypeOutputDTO : IncidentEventTypeDTO
		{
		}


		/// <summary>
		///
		/// Converts a IncidentEventType to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public IncidentEventTypeDTO ToDTO()
		{
			return new IncidentEventTypeDTO
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
		/// Converts a IncidentEventType list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<IncidentEventTypeDTO> ToDTOList(List<IncidentEventType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<IncidentEventTypeDTO> output = new List<IncidentEventTypeDTO>();

			output.Capacity = data.Count;

			foreach (IncidentEventType incidentEventType in data)
			{
				output.Add(incidentEventType.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a IncidentEventType to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the IncidentEventTypeEntity type directly.
		///
		/// </summary>
		public IncidentEventTypeOutputDTO ToOutputDTO()
		{
			return new IncidentEventTypeOutputDTO
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
		/// Converts a IncidentEventType list to list of Output Data Transfer Object intended to be used for serializing a list of IncidentEventType objects to avoid using the IncidentEventType entity type directly.
		///
		/// </summary>
		public static List<IncidentEventTypeOutputDTO> ToOutputDTOList(List<IncidentEventType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<IncidentEventTypeOutputDTO> output = new List<IncidentEventTypeOutputDTO>();

			output.Capacity = data.Count;

			foreach (IncidentEventType incidentEventType in data)
			{
				output.Add(incidentEventType.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a IncidentEventType Object.
		///
		/// </summary>
		public static Database.IncidentEventType FromDTO(IncidentEventTypeDTO dto)
		{
			return new Database.IncidentEventType
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
		/// Applies the values from an INPUT DTO to a IncidentEventType Object.
		///
		/// </summary>
		public void ApplyDTO(IncidentEventTypeDTO dto)
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
		/// Creates a deep copy clone of a IncidentEventType Object.
		///
		/// </summary>
		public IncidentEventType Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new IncidentEventType{
				id = this.id,
				name = this.name,
				description = this.description,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a IncidentEventType Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a IncidentEventType Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a IncidentEventType Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a IncidentEventType Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.IncidentEventType incidentEventType)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (incidentEventType == null)
			{
				return null;
			}

			return new {
				id = incidentEventType.id,
				name = incidentEventType.name,
				description = incidentEventType.description,
				active = incidentEventType.active,
				deleted = incidentEventType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a IncidentEventType Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(IncidentEventType incidentEventType)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (incidentEventType == null)
			{
				return null;
			}

			return new {
				id = incidentEventType.id,
				name = incidentEventType.name,
				description = incidentEventType.description,
				active = incidentEventType.active,
				deleted = incidentEventType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a IncidentEventType Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(IncidentEventType incidentEventType)
		{
			//
			// Return a very minimal object.
			//
			if (incidentEventType == null)
			{
				return null;
			}

			return new {
				id = incidentEventType.id,
				name = incidentEventType.name,
				description = incidentEventType.description,
			 };
		}
	}
}
