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
	public partial class StateProvince : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class StateProvinceDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 countryId { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
			[Required]
			public String abbreviation { get; set; }
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
		public class StateProvinceOutputDTO : StateProvinceDTO
		{
			public Country.CountryDTO country { get; set; }
		}


		/// <summary>
		///
		/// Converts a StateProvince to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public StateProvinceDTO ToDTO()
		{
			return new StateProvinceDTO
			{
				id = this.id,
				countryId = this.countryId,
				name = this.name,
				description = this.description,
				abbreviation = this.abbreviation,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a StateProvince list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<StateProvinceDTO> ToDTOList(List<StateProvince> data)
		{
			if (data == null)
			{
				return null;
			}

			List<StateProvinceDTO> output = new List<StateProvinceDTO>();

			output.Capacity = data.Count;

			foreach (StateProvince stateProvince in data)
			{
				output.Add(stateProvince.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a StateProvince to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the StateProvinceEntity type directly.
		///
		/// </summary>
		public StateProvinceOutputDTO ToOutputDTO()
		{
			return new StateProvinceOutputDTO
			{
				id = this.id,
				countryId = this.countryId,
				name = this.name,
				description = this.description,
				abbreviation = this.abbreviation,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				country = this.country?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a StateProvince list to list of Output Data Transfer Object intended to be used for serializing a list of StateProvince objects to avoid using the StateProvince entity type directly.
		///
		/// </summary>
		public static List<StateProvinceOutputDTO> ToOutputDTOList(List<StateProvince> data)
		{
			if (data == null)
			{
				return null;
			}

			List<StateProvinceOutputDTO> output = new List<StateProvinceOutputDTO>();

			output.Capacity = data.Count;

			foreach (StateProvince stateProvince in data)
			{
				output.Add(stateProvince.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a StateProvince Object.
		///
		/// </summary>
		public static Database.StateProvince FromDTO(StateProvinceDTO dto)
		{
			return new Database.StateProvince
			{
				id = dto.id,
				countryId = dto.countryId,
				name = dto.name,
				description = dto.description,
				abbreviation = dto.abbreviation,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a StateProvince Object.
		///
		/// </summary>
		public void ApplyDTO(StateProvinceDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.countryId = dto.countryId;
			this.name = dto.name;
			this.description = dto.description;
			this.abbreviation = dto.abbreviation;
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
		/// Creates a deep copy clone of a StateProvince Object.
		///
		/// </summary>
		public StateProvince Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new StateProvince{
				id = this.id,
				countryId = this.countryId,
				name = this.name,
				description = this.description,
				abbreviation = this.abbreviation,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a StateProvince Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a StateProvince Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a StateProvince Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a StateProvince Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.StateProvince stateProvince)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (stateProvince == null)
			{
				return null;
			}

			return new {
				id = stateProvince.id,
				countryId = stateProvince.countryId,
				name = stateProvince.name,
				description = stateProvince.description,
				abbreviation = stateProvince.abbreviation,
				sequence = stateProvince.sequence,
				objectGuid = stateProvince.objectGuid,
				active = stateProvince.active,
				deleted = stateProvince.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a StateProvince Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(StateProvince stateProvince)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (stateProvince == null)
			{
				return null;
			}

			return new {
				id = stateProvince.id,
				countryId = stateProvince.countryId,
				name = stateProvince.name,
				description = stateProvince.description,
				abbreviation = stateProvince.abbreviation,
				sequence = stateProvince.sequence,
				objectGuid = stateProvince.objectGuid,
				active = stateProvince.active,
				deleted = stateProvince.deleted,
				country = Country.CreateMinimalAnonymous(stateProvince.country)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a StateProvince Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(StateProvince stateProvince)
		{
			//
			// Return a very minimal object.
			//
			if (stateProvince == null)
			{
				return null;
			}

			return new {
				id = stateProvince.id,
				name = stateProvince.name,
				description = stateProvince.description,
			 };
		}
	}
}
