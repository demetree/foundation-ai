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
	public partial class MenuItem : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class MenuItemDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 menuId { get; set; }
			[Required]
			public String label { get; set; }
			public String url { get; set; }
			public Int32? pageId { get; set; }
			public Int32? parentMenuItemId { get; set; }
			public String iconClass { get; set; }
			[Required]
			public Boolean openInNewTab { get; set; }
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
		public class MenuItemOutputDTO : MenuItemDTO
		{
			public Menu.MenuDTO menu { get; set; }
			public Page.PageDTO page { get; set; }
			public MenuItem.MenuItemDTO parentMenuItem { get; set; }
		}


		/// <summary>
		///
		/// Converts a MenuItem to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public MenuItemDTO ToDTO()
		{
			return new MenuItemDTO
			{
				id = this.id,
				menuId = this.menuId,
				label = this.label,
				url = this.url,
				pageId = this.pageId,
				parentMenuItemId = this.parentMenuItemId,
				iconClass = this.iconClass,
				openInNewTab = this.openInNewTab,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a MenuItem list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<MenuItemDTO> ToDTOList(List<MenuItem> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MenuItemDTO> output = new List<MenuItemDTO>();

			output.Capacity = data.Count;

			foreach (MenuItem menuItem in data)
			{
				output.Add(menuItem.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a MenuItem to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the MenuItemEntity type directly.
		///
		/// </summary>
		public MenuItemOutputDTO ToOutputDTO()
		{
			return new MenuItemOutputDTO
			{
				id = this.id,
				menuId = this.menuId,
				label = this.label,
				url = this.url,
				pageId = this.pageId,
				parentMenuItemId = this.parentMenuItemId,
				iconClass = this.iconClass,
				openInNewTab = this.openInNewTab,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				menu = this.menu?.ToDTO(),
				page = this.page?.ToDTO(),
				parentMenuItem = this.parentMenuItem?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a MenuItem list to list of Output Data Transfer Object intended to be used for serializing a list of MenuItem objects to avoid using the MenuItem entity type directly.
		///
		/// </summary>
		public static List<MenuItemOutputDTO> ToOutputDTOList(List<MenuItem> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MenuItemOutputDTO> output = new List<MenuItemOutputDTO>();

			output.Capacity = data.Count;

			foreach (MenuItem menuItem in data)
			{
				output.Add(menuItem.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a MenuItem Object.
		///
		/// </summary>
		public static Database.MenuItem FromDTO(MenuItemDTO dto)
		{
			return new Database.MenuItem
			{
				id = dto.id,
				menuId = dto.menuId,
				label = dto.label,
				url = dto.url,
				pageId = dto.pageId,
				parentMenuItemId = dto.parentMenuItemId,
				iconClass = dto.iconClass,
				openInNewTab = dto.openInNewTab,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a MenuItem Object.
		///
		/// </summary>
		public void ApplyDTO(MenuItemDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.menuId = dto.menuId;
			this.label = dto.label;
			this.url = dto.url;
			this.pageId = dto.pageId;
			this.parentMenuItemId = dto.parentMenuItemId;
			this.iconClass = dto.iconClass;
			this.openInNewTab = dto.openInNewTab;
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
		/// Creates a deep copy clone of a MenuItem Object.
		///
		/// </summary>
		public MenuItem Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new MenuItem{
				id = this.id,
				tenantGuid = this.tenantGuid,
				menuId = this.menuId,
				label = this.label,
				url = this.url,
				pageId = this.pageId,
				parentMenuItemId = this.parentMenuItemId,
				iconClass = this.iconClass,
				openInNewTab = this.openInNewTab,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MenuItem Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MenuItem Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a MenuItem Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a MenuItem Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.MenuItem menuItem)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (menuItem == null)
			{
				return null;
			}

			return new {
				id = menuItem.id,
				menuId = menuItem.menuId,
				label = menuItem.label,
				url = menuItem.url,
				pageId = menuItem.pageId,
				parentMenuItemId = menuItem.parentMenuItemId,
				iconClass = menuItem.iconClass,
				openInNewTab = menuItem.openInNewTab,
				sequence = menuItem.sequence,
				objectGuid = menuItem.objectGuid,
				active = menuItem.active,
				deleted = menuItem.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a MenuItem Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(MenuItem menuItem)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (menuItem == null)
			{
				return null;
			}

			return new {
				id = menuItem.id,
				menuId = menuItem.menuId,
				label = menuItem.label,
				url = menuItem.url,
				pageId = menuItem.pageId,
				parentMenuItemId = menuItem.parentMenuItemId,
				iconClass = menuItem.iconClass,
				openInNewTab = menuItem.openInNewTab,
				sequence = menuItem.sequence,
				objectGuid = menuItem.objectGuid,
				active = menuItem.active,
				deleted = menuItem.deleted,
				menu = Menu.CreateMinimalAnonymous(menuItem.menu),
				page = Page.CreateMinimalAnonymous(menuItem.page),
				parentMenuItem = MenuItem.CreateMinimalAnonymous(menuItem.parentMenuItem)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a MenuItem Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(MenuItem menuItem)
		{
			//
			// Return a very minimal object.
			//
			if (menuItem == null)
			{
				return null;
			}

			return new {
				id = menuItem.id,
				name = menuItem.label,
				description = string.Join(", ", new[] { menuItem.label, menuItem.url, menuItem.iconClass}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
