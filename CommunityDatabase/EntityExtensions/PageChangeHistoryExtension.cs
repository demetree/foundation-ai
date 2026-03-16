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
			get { return (long)pageId; }
			set { pageId = (int)value; } 
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
			public Int32 id { get; set; }
			[Required]
			public Int32 pageId { get; set; }
			public Int32 versionNumber { get; set; }
			[Required]
			public DateTime timeStamp { get; set; }
			[Required]
			public Int32 userId { get; set; }
			[Required]
			public String data { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class PageChangeHistoryOutputDTO : PageChangeHistoryDTO
		{
			public Page.PageDTO page { get; set; }
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
				id = this.id,
				pageId = this.pageId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
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
				id = this.id,
				pageId = this.pageId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				page = this.page?.ToDTO()
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
				id = dto.id,
				pageId = dto.pageId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
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

			this.pageId = dto.pageId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
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
				id = this.id,
				tenantGuid = this.tenantGuid,
				pageId = this.pageId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
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
				id = pageChangeHistory.id,
				pageId = pageChangeHistory.pageId,
				versionNumber = pageChangeHistory.versionNumber,
				timeStamp = pageChangeHistory.timeStamp,
				userId = pageChangeHistory.userId,
				data = pageChangeHistory.data,
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
				id = pageChangeHistory.id,
				pageId = pageChangeHistory.pageId,
				versionNumber = pageChangeHistory.versionNumber,
				timeStamp = pageChangeHistory.timeStamp,
				userId = pageChangeHistory.userId,
				data = pageChangeHistory.data,
				page = Page.CreateMinimalAnonymous(pageChangeHistory.page)
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
				id = pageChangeHistory.id,
				name = pageChangeHistory.id,
				description = pageChangeHistory.id
			 };
		}
	}
}
