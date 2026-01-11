using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Security.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class Module : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ModuleDTO
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
		public class ModuleOutputDTO : ModuleDTO
		{
		}


		/// <summary>
		///
		/// Converts a Module to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ModuleDTO ToDTO()
		{
			return new ModuleDTO
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
		/// Converts a Module list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ModuleDTO> ToDTOList(List<Module> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ModuleDTO> output = new List<ModuleDTO>();

			output.Capacity = data.Count;

			foreach (Module module in data)
			{
				output.Add(module.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a Module to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ModuleEntity type directly.
		///
		/// </summary>
		public ModuleOutputDTO ToOutputDTO()
		{
			return new ModuleOutputDTO
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
		/// Converts a Module list to list of Output Data Transfer Object intended to be used for serializing a list of Module objects to avoid using the Module entity type directly.
		///
		/// </summary>
		public static List<ModuleOutputDTO> ToOutputDTOList(List<Module> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ModuleOutputDTO> output = new List<ModuleOutputDTO>();

			output.Capacity = data.Count;

			foreach (Module module in data)
			{
				output.Add(module.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a Module Object.
		///
		/// </summary>
		public static Database.Module FromDTO(ModuleDTO dto)
		{
			return new Database.Module
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
		/// Applies the values from an INPUT DTO to a Module Object.
		///
		/// </summary>
		public void ApplyDTO(ModuleDTO dto)
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
		/// Creates a deep copy clone of a Module Object.
		///
		/// </summary>
		public Module Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new Module{
				id = this.id,
				name = this.name,
				description = this.description,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Module Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Module Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a Module Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a Module Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.Module module)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (module == null)
			{
				return null;
			}

			return new {
				id = module.id,
				name = module.name,
				description = module.description,
				active = module.active,
				deleted = module.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a Module Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(Module module)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (module == null)
			{
				return null;
			}

			return new {
				id = module.id,
				name = module.name,
				description = module.description,
				active = module.active,
				deleted = module.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a Module Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(Module module)
		{
			//
			// Return a very minimal object.
			//
			if (module == null)
			{
				return null;
			}

			return new {
				id = module.id,
				name = module.name,
				description = module.description,
			 };
		}
	}
}
