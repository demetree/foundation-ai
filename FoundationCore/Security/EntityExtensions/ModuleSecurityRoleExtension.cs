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
	public partial class ModuleSecurityRole : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ModuleSecurityRoleDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 moduleId { get; set; }
			[Required]
			public Int32 securityRoleId { get; set; }
			public String comments { get; set; }
			public Boolean? active { get; set; }
			public Boolean? deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class ModuleSecurityRoleOutputDTO : ModuleSecurityRoleDTO
		{
			public Module.ModuleDTO module { get; set; }
			public SecurityRole.SecurityRoleDTO securityRole { get; set; }
		}


		/// <summary>
		///
		/// Converts a ModuleSecurityRole to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ModuleSecurityRoleDTO ToDTO()
		{
			return new ModuleSecurityRoleDTO
			{
				id = this.id,
				moduleId = this.moduleId,
				securityRoleId = this.securityRoleId,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ModuleSecurityRole list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ModuleSecurityRoleDTO> ToDTOList(List<ModuleSecurityRole> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ModuleSecurityRoleDTO> output = new List<ModuleSecurityRoleDTO>();

			output.Capacity = data.Count;

			foreach (ModuleSecurityRole moduleSecurityRole in data)
			{
				output.Add(moduleSecurityRole.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ModuleSecurityRole to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ModuleSecurityRoleEntity type directly.
		///
		/// </summary>
		public ModuleSecurityRoleOutputDTO ToOutputDTO()
		{
			return new ModuleSecurityRoleOutputDTO
			{
				id = this.id,
				moduleId = this.moduleId,
				securityRoleId = this.securityRoleId,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted,
				module = this.module?.ToDTO(),
				securityRole = this.securityRole?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ModuleSecurityRole list to list of Output Data Transfer Object intended to be used for serializing a list of ModuleSecurityRole objects to avoid using the ModuleSecurityRole entity type directly.
		///
		/// </summary>
		public static List<ModuleSecurityRoleOutputDTO> ToOutputDTOList(List<ModuleSecurityRole> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ModuleSecurityRoleOutputDTO> output = new List<ModuleSecurityRoleOutputDTO>();

			output.Capacity = data.Count;

			foreach (ModuleSecurityRole moduleSecurityRole in data)
			{
				output.Add(moduleSecurityRole.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ModuleSecurityRole Object.
		///
		/// </summary>
		public static Database.ModuleSecurityRole FromDTO(ModuleSecurityRoleDTO dto)
		{
			return new Database.ModuleSecurityRole
			{
				id = dto.id,
				moduleId = dto.moduleId,
				securityRoleId = dto.securityRoleId,
				comments = dto.comments,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ModuleSecurityRole Object.
		///
		/// </summary>
		public void ApplyDTO(ModuleSecurityRoleDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.moduleId = dto.moduleId;
			this.securityRoleId = dto.securityRoleId;
			this.comments = dto.comments;
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
		/// Creates a deep copy clone of a ModuleSecurityRole Object.
		///
		/// </summary>
		public ModuleSecurityRole Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ModuleSecurityRole{
				id = this.id,
				moduleId = this.moduleId,
				securityRoleId = this.securityRoleId,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ModuleSecurityRole Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ModuleSecurityRole Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ModuleSecurityRole Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ModuleSecurityRole Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ModuleSecurityRole moduleSecurityRole)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (moduleSecurityRole == null)
			{
				return null;
			}

			return new {
				id = moduleSecurityRole.id,
				moduleId = moduleSecurityRole.moduleId,
				securityRoleId = moduleSecurityRole.securityRoleId,
				comments = moduleSecurityRole.comments,
				active = moduleSecurityRole.active,
				deleted = moduleSecurityRole.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ModuleSecurityRole Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ModuleSecurityRole moduleSecurityRole)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (moduleSecurityRole == null)
			{
				return null;
			}

			return new {
				id = moduleSecurityRole.id,
				moduleId = moduleSecurityRole.moduleId,
				securityRoleId = moduleSecurityRole.securityRoleId,
				comments = moduleSecurityRole.comments,
				active = moduleSecurityRole.active,
				deleted = moduleSecurityRole.deleted,
				module = Module.CreateMinimalAnonymous(moduleSecurityRole.module),
				securityRole = SecurityRole.CreateMinimalAnonymous(moduleSecurityRole.securityRole)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ModuleSecurityRole Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ModuleSecurityRole moduleSecurityRole)
		{
			//
			// Return a very minimal object.
			//
			if (moduleSecurityRole == null)
			{
				return null;
			}

			return new {
				id = moduleSecurityRole.id,
				name = moduleSecurityRole.comments,
				description = string.Join(", ", new[] { moduleSecurityRole.comments}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
