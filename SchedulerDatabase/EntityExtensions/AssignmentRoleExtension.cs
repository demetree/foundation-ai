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
	public partial class AssignmentRole : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class AssignmentRoleDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			public String description { get; set; }
			public Int32? iconId { get; set; }
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
		public class AssignmentRoleOutputDTO : AssignmentRoleDTO
		{
			public Icon.IconDTO icon { get; set; }
		}


		/// <summary>
		///
		/// Converts a AssignmentRole to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public AssignmentRoleDTO ToDTO()
		{
			return new AssignmentRoleDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				iconId = this.iconId,
				color = this.color,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a AssignmentRole list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<AssignmentRoleDTO> ToDTOList(List<AssignmentRole> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AssignmentRoleDTO> output = new List<AssignmentRoleDTO>();

			output.Capacity = data.Count;

			foreach (AssignmentRole assignmentRole in data)
			{
				output.Add(assignmentRole.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a AssignmentRole to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the AssignmentRole Entity type directly.
		///
		/// </summary>
		public AssignmentRoleOutputDTO ToOutputDTO()
		{
			return new AssignmentRoleOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				iconId = this.iconId,
				color = this.color,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				icon = this.icon?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a AssignmentRole list to list of Output Data Transfer Object intended to be used for serializing a list of AssignmentRole objects to avoid using the AssignmentRole entity type directly.
		///
		/// </summary>
		public static List<AssignmentRoleOutputDTO> ToOutputDTOList(List<AssignmentRole> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AssignmentRoleOutputDTO> output = new List<AssignmentRoleOutputDTO>();

			output.Capacity = data.Count;

			foreach (AssignmentRole assignmentRole in data)
			{
				output.Add(assignmentRole.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a AssignmentRole Object.
		///
		/// </summary>
		public static Database.AssignmentRole FromDTO(AssignmentRoleDTO dto)
		{
			return new Database.AssignmentRole
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				iconId = dto.iconId,
				color = dto.color,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a AssignmentRole Object.
		///
		/// </summary>
		public void ApplyDTO(AssignmentRoleDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.iconId = dto.iconId;
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
		/// Creates a deep copy clone of a AssignmentRole Object.
		///
		/// </summary>
		public AssignmentRole Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new AssignmentRole{
				id = this.id,
				tenantGuid = this.tenantGuid,
				name = this.name,
				description = this.description,
				iconId = this.iconId,
				color = this.color,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AssignmentRole Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AssignmentRole Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a AssignmentRole Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a AssignmentRole Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.AssignmentRole assignmentRole)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (assignmentRole == null)
			{
				return null;
			}

			return new {
				id = assignmentRole.id,
				name = assignmentRole.name,
				description = assignmentRole.description,
				iconId = assignmentRole.iconId,
				color = assignmentRole.color,
				sequence = assignmentRole.sequence,
				objectGuid = assignmentRole.objectGuid,
				active = assignmentRole.active,
				deleted = assignmentRole.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a AssignmentRole Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(AssignmentRole assignmentRole)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (assignmentRole == null)
			{
				return null;
			}

			return new {
				id = assignmentRole.id,
				name = assignmentRole.name,
				description = assignmentRole.description,
				iconId = assignmentRole.iconId,
				color = assignmentRole.color,
				sequence = assignmentRole.sequence,
				objectGuid = assignmentRole.objectGuid,
				active = assignmentRole.active,
				deleted = assignmentRole.deleted,
				icon = Icon.CreateMinimalAnonymous(assignmentRole.icon)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a AssignmentRole Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(AssignmentRole assignmentRole)
		{
			//
			// Return a very minimal object.
			//
			if (assignmentRole == null)
			{
				return null;
			}

			return new {
				id = assignmentRole.id,
				name = assignmentRole.name,
				description = assignmentRole.description,
			 };
		}
	}
}
