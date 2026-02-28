using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.BMC.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class LegoTheme : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class LegoThemeDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
			public Int32? legoThemeId { get; set; }
			[Required]
			public Int32 rebrickableThemeId { get; set; }
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
		public class LegoThemeOutputDTO : LegoThemeDTO
		{
			public LegoTheme.LegoThemeDTO legoTheme { get; set; }
		}


		/// <summary>
		///
		/// Converts a LegoTheme to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public LegoThemeDTO ToDTO()
		{
			return new LegoThemeDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				legoThemeId = this.legoThemeId,
				rebrickableThemeId = this.rebrickableThemeId,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a LegoTheme list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<LegoThemeDTO> ToDTOList(List<LegoTheme> data)
		{
			if (data == null)
			{
				return null;
			}

			List<LegoThemeDTO> output = new List<LegoThemeDTO>();

			output.Capacity = data.Count;

			foreach (LegoTheme legoTheme in data)
			{
				output.Add(legoTheme.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a LegoTheme to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the LegoThemeEntity type directly.
		///
		/// </summary>
		public LegoThemeOutputDTO ToOutputDTO()
		{
			return new LegoThemeOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				legoThemeId = this.legoThemeId,
				rebrickableThemeId = this.rebrickableThemeId,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				legoTheme = this.legoTheme?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a LegoTheme list to list of Output Data Transfer Object intended to be used for serializing a list of LegoTheme objects to avoid using the LegoTheme entity type directly.
		///
		/// </summary>
		public static List<LegoThemeOutputDTO> ToOutputDTOList(List<LegoTheme> data)
		{
			if (data == null)
			{
				return null;
			}

			List<LegoThemeOutputDTO> output = new List<LegoThemeOutputDTO>();

			output.Capacity = data.Count;

			foreach (LegoTheme legoTheme in data)
			{
				output.Add(legoTheme.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a LegoTheme Object.
		///
		/// </summary>
		public static Database.LegoTheme FromDTO(LegoThemeDTO dto)
		{
			return new Database.LegoTheme
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				legoThemeId = dto.legoThemeId,
				rebrickableThemeId = dto.rebrickableThemeId,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a LegoTheme Object.
		///
		/// </summary>
		public void ApplyDTO(LegoThemeDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.legoThemeId = dto.legoThemeId;
			this.rebrickableThemeId = dto.rebrickableThemeId;
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
		/// Creates a deep copy clone of a LegoTheme Object.
		///
		/// </summary>
		public LegoTheme Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new LegoTheme{
				id = this.id,
				name = this.name,
				description = this.description,
				legoThemeId = this.legoThemeId,
				rebrickableThemeId = this.rebrickableThemeId,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a LegoTheme Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a LegoTheme Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a LegoTheme Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a LegoTheme Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.LegoTheme legoTheme)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (legoTheme == null)
			{
				return null;
			}

			return new {
				id = legoTheme.id,
				name = legoTheme.name,
				description = legoTheme.description,
				legoThemeId = legoTheme.legoThemeId,
				rebrickableThemeId = legoTheme.rebrickableThemeId,
				sequence = legoTheme.sequence,
				objectGuid = legoTheme.objectGuid,
				active = legoTheme.active,
				deleted = legoTheme.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a LegoTheme Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(LegoTheme legoTheme)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (legoTheme == null)
			{
				return null;
			}

			return new {
				id = legoTheme.id,
				name = legoTheme.name,
				description = legoTheme.description,
				legoThemeId = legoTheme.legoThemeId,
				rebrickableThemeId = legoTheme.rebrickableThemeId,
				sequence = legoTheme.sequence,
				objectGuid = legoTheme.objectGuid,
				active = legoTheme.active,
				deleted = legoTheme.deleted,
				legoTheme = LegoTheme.CreateMinimalAnonymous(legoTheme.legoTheme)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a LegoTheme Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(LegoTheme legoTheme)
		{
			//
			// Return a very minimal object.
			//
			if (legoTheme == null)
			{
				return null;
			}

			return new {
				id = legoTheme.id,
				name = legoTheme.name,
				description = legoTheme.description,
			 };
		}
	}
}
