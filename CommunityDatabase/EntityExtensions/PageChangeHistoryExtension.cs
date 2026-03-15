using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.Community.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class PageChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)PageId; }
			set { PageId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class PageChangeHistoryDTO
		{
			public Int32 Id { get; set; }
			public Int32 PageId { get; set; }
			public Int32 VersionNumber { get; set; }
			public DateTime TimeStamp { get; set; }
			public Int32 UserId { get; set; }
			public String Data { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class PageChangeHistoryOutputDTO : PageChangeHistoryDTO
		{
			public Page.PageDTO Page { get; set; }
		}


		/// <summary>
		///
		/// Converts a PageChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public PageChangeHistoryDTO ToDTO()
		{
			return new PageChangeHistoryDTO
			{
				Id = this.Id,
				PageId = this.PageId,
				VersionNumber = this.VersionNumber,
				TimeStamp = this.TimeStamp,
				UserId = this.UserId,
				Data = this.Data
			};
		}


		/// <summary>
		///
		/// Converts a PageChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<PageChangeHistoryDTO> ToDTOList(List<PageChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PageChangeHistoryDTO> output = new List<PageChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (PageChangeHistory pageChangeHistory in data)
			{
				output.Add(pageChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a PageChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the PageChangeHistoryEntity type directly.
		///
		/// </summary>
		public PageChangeHistoryOutputDTO ToOutputDTO()
		{
			return new PageChangeHistoryOutputDTO
			{
				Id = this.Id,
				PageId = this.PageId,
				VersionNumber = this.VersionNumber,
				TimeStamp = this.TimeStamp,
				UserId = this.UserId,
				Data = this.Data,
				Page = this.Page?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a PageChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of PageChangeHistory objects to avoid using the PageChangeHistory entity type directly.
		///
		/// </summary>
		public static List<PageChangeHistoryOutputDTO> ToOutputDTOList(List<PageChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PageChangeHistoryOutputDTO> output = new List<PageChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (PageChangeHistory pageChangeHistory in data)
			{
				output.Add(pageChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a PageChangeHistory Object.
		///
		/// </summary>
		public static Database.PageChangeHistory FromDTO(PageChangeHistoryDTO dto)
		{
			return new Database.PageChangeHistory
			{
				Id = dto.Id,
				PageId = dto.PageId,
				VersionNumber = dto.VersionNumber,
				TimeStamp = dto.TimeStamp,
				UserId = dto.UserId,
				Data = dto.Data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a PageChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(PageChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.PageId = dto.PageId;
			this.VersionNumber = dto.VersionNumber;
			this.TimeStamp = dto.TimeStamp;
			this.UserId = dto.UserId;
			this.Data = dto.Data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a PageChangeHistory Object.
		///
		/// </summary>
		public PageChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new PageChangeHistory{
				Id = this.Id,
				PageId = this.PageId,
				VersionNumber = this.VersionNumber,
				TimeStamp = this.TimeStamp,
				UserId = this.UserId,
				Data = this.Data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PageChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PageChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a PageChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a PageChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.PageChangeHistory pageChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (pageChangeHistory == null)
			{
				return null;
			}

			return new {
				Id = pageChangeHistory.Id,
				PageId = pageChangeHistory.PageId,
				VersionNumber = pageChangeHistory.VersionNumber,
				TimeStamp = pageChangeHistory.TimeStamp,
				UserId = pageChangeHistory.UserId,
				Data = pageChangeHistory.Data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a PageChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(PageChangeHistory pageChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (pageChangeHistory == null)
			{
				return null;
			}

			return new {
				Id = pageChangeHistory.Id,
				PageId = pageChangeHistory.PageId,
				VersionNumber = pageChangeHistory.VersionNumber,
				TimeStamp = pageChangeHistory.TimeStamp,
				UserId = pageChangeHistory.UserId,
				Data = pageChangeHistory.Data,
				Page = Page.CreateMinimalAnonymous(pageChangeHistory.Page)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a PageChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(PageChangeHistory pageChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (pageChangeHistory == null)
			{
				return null;
			}

			return new {
				name = pageChangeHistory.id,
				description = pageChangeHistory.id
			 };
		}
	}
}
