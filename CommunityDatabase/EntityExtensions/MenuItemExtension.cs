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
			public Int32 Id { get; set; }
			public Int32 MenuId { get; set; }
			public String Label { get; set; }
			public String Url { get; set; }
			public Int32? PageId { get; set; }
			public Int32? ParentMenuItemId { get; set; }
			public String IconClass { get; set; }
			public Boolean OpenInNewTab { get; set; }
			public Int32? Sequence { get; set; }
			public Guid ObjectGuid { get; set; }
			public Boolean? Active { get; set; }
			public Boolean? Deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class MenuItemOutputDTO : MenuItemDTO
		{
			public Menu.MenuDTO Menu { get; set; }
			public Page.PageDTO Page { get; set; }
			public MenuItem.MenuItemDTO ParentMenuItem { get; set; }
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
				Id = this.Id,
				MenuId = this.MenuId,
				Label = this.Label,
				Url = this.Url,
				PageId = this.PageId,
				ParentMenuItemId = this.ParentMenuItemId,
				IconClass = this.IconClass,
				OpenInNewTab = this.OpenInNewTab,
				Sequence = this.Sequence,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted
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
				Id = this.Id,
				MenuId = this.MenuId,
				Label = this.Label,
				Url = this.Url,
				PageId = this.PageId,
				ParentMenuItemId = this.ParentMenuItemId,
				IconClass = this.IconClass,
				OpenInNewTab = this.OpenInNewTab,
				Sequence = this.Sequence,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted,
				Menu = this.Menu?.ToDTO(),
				Page = this.Page?.ToDTO(),
				ParentMenuItem = this.ParentMenuItem?.ToDTO()
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
				Id = dto.Id,
				MenuId = dto.MenuId,
				Label = dto.Label,
				Url = dto.Url,
				PageId = dto.PageId,
				ParentMenuItemId = dto.ParentMenuItemId,
				IconClass = dto.IconClass,
				OpenInNewTab = dto.OpenInNewTab,
				Sequence = dto.Sequence,
				ObjectGuid = dto.ObjectGuid,
				Active = dto.Active ?? true,
				Deleted = dto.Deleted ?? false
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

			this.MenuId = dto.MenuId;
			this.Label = dto.Label;
			this.Url = dto.Url;
			this.PageId = dto.PageId;
			this.ParentMenuItemId = dto.ParentMenuItemId;
			this.IconClass = dto.IconClass;
			this.OpenInNewTab = dto.OpenInNewTab;
			this.Sequence = dto.Sequence;
			this.ObjectGuid = dto.ObjectGuid;
			if (dto.Active.HasValue == true)
			{
				this.Active = dto.Active.Value;
			}
			if (dto.Deleted.HasValue == true)
			{
				this.Deleted = dto.Deleted.Value;
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
				Id = this.Id,
				MenuId = this.MenuId,
				Label = this.Label,
				Url = this.Url,
				PageId = this.PageId,
				ParentMenuItemId = this.ParentMenuItemId,
				IconClass = this.IconClass,
				OpenInNewTab = this.OpenInNewTab,
				Sequence = this.Sequence,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted,
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
				Id = menuItem.Id,
				MenuId = menuItem.MenuId,
				Label = menuItem.Label,
				Url = menuItem.Url,
				PageId = menuItem.PageId,
				ParentMenuItemId = menuItem.ParentMenuItemId,
				IconClass = menuItem.IconClass,
				OpenInNewTab = menuItem.OpenInNewTab,
				Sequence = menuItem.Sequence,
				ObjectGuid = menuItem.ObjectGuid,
				Active = menuItem.Active,
				Deleted = menuItem.Deleted,
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
				Id = menuItem.Id,
				MenuId = menuItem.MenuId,
				Label = menuItem.Label,
				Url = menuItem.Url,
				PageId = menuItem.PageId,
				ParentMenuItemId = menuItem.ParentMenuItemId,
				IconClass = menuItem.IconClass,
				OpenInNewTab = menuItem.OpenInNewTab,
				Sequence = menuItem.Sequence,
				ObjectGuid = menuItem.ObjectGuid,
				Active = menuItem.Active,
				Deleted = menuItem.Deleted,
				Menu = Menu.CreateMinimalAnonymous(menuItem.Menu),
				Page = Page.CreateMinimalAnonymous(menuItem.Page),
				ParentMenuItem = MenuItem.CreateMinimalAnonymous(menuItem.ParentMenuItem)
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
				name = menuItem.label,
				description = string.Join(", ", new[] { menuItem.label, menuItem.url, menuItem.iconClass}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
