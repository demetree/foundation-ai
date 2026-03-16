using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Community.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class Menu : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class MenuDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String location { get; set; }
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
		public class MenuOutputDTO : MenuDTO
		{
		}


		/// <summary>
		///
		/// Converts a Menu to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public MenuDTO ToDTO()
		{
			return new MenuDTO
			{
				id = this.id,
				name = this.name,
				location = this.location,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a Menu list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<MenuDTO> ToDTOList(List<Menu> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MenuDTO> output = new List<MenuDTO>();

			output.Capacity = data.Count;

			foreach (Menu menu in data)
			{
				output.Add(menu.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a Menu to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the MenuEntity type directly.
		///
		/// </summary>
		public MenuOutputDTO ToOutputDTO()
		{
			return new MenuOutputDTO
			{
				id = this.id,
				name = this.name,
				location = this.location,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a Menu list to list of Output Data Transfer Object intended to be used for serializing a list of Menu objects to avoid using the Menu entity type directly.
		///
		/// </summary>
		public static List<MenuOutputDTO> ToOutputDTOList(List<Menu> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MenuOutputDTO> output = new List<MenuOutputDTO>();

			output.Capacity = data.Count;

			foreach (Menu menu in data)
			{
				output.Add(menu.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a Menu Object.
		///
		/// </summary>
		public static Database.Menu FromDTO(MenuDTO dto)
		{
			return new Database.Menu
			{
				id = dto.id,
				name = dto.name,
				location = dto.location,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a Menu Object.
		///
		/// </summary>
		public void ApplyDTO(MenuDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.location = dto.location;
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
		/// Creates a deep copy clone of a Menu Object.
		///
		/// </summary>
		public Menu Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new Menu{
				id = this.id,
				tenantGuid = this.tenantGuid,
				name = this.name,
				location = this.location,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Menu Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Menu Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a Menu Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a Menu Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.Menu menu)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (menu == null)
			{
				return null;
			}

			return new {
				id = menu.id,
				name = menu.name,
				location = menu.location,
				objectGuid = menu.objectGuid,
				active = menu.active,
				deleted = menu.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a Menu Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(Menu menu)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (menu == null)
			{
				return null;
			}

			return new {
				id = menu.id,
				name = menu.name,
				location = menu.location,
				objectGuid = menu.objectGuid,
				active = menu.active,
				deleted = menu.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a Menu Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(Menu menu)
		{
			//
			// Return a very minimal object.
			//
			if (menu == null)
			{
				return null;
			}

			return new {
				id = menu.id,
				name = menu.name,
				description = string.Join(", ", new[] { menu.name, menu.location}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
