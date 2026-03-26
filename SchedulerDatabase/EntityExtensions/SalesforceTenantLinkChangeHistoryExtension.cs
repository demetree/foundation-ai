using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.Scheduler.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class SalesforceTenantLinkChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)salesforceTenantLinkId; }
			set { salesforceTenantLinkId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SalesforceTenantLinkChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 salesforceTenantLinkId { get; set; }
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
		public class SalesforceTenantLinkChangeHistoryOutputDTO : SalesforceTenantLinkChangeHistoryDTO
		{
			public SalesforceTenantLink.SalesforceTenantLinkDTO salesforceTenantLink { get; set; }
		}


		/// <summary>
		///
		/// Converts a SalesforceTenantLinkChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SalesforceTenantLinkChangeHistoryDTO ToDTO()
		{
			return new SalesforceTenantLinkChangeHistoryDTO
			{
				id = this.id,
				salesforceTenantLinkId = this.salesforceTenantLinkId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a SalesforceTenantLinkChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SalesforceTenantLinkChangeHistoryDTO> ToDTOList(List<SalesforceTenantLinkChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SalesforceTenantLinkChangeHistoryDTO> output = new List<SalesforceTenantLinkChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (SalesforceTenantLinkChangeHistory salesforceTenantLinkChangeHistory in data)
			{
				output.Add(salesforceTenantLinkChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SalesforceTenantLinkChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SalesforceTenantLinkChangeHistory Entity type directly.
		///
		/// </summary>
		public SalesforceTenantLinkChangeHistoryOutputDTO ToOutputDTO()
		{
			return new SalesforceTenantLinkChangeHistoryOutputDTO
			{
				id = this.id,
				salesforceTenantLinkId = this.salesforceTenantLinkId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				salesforceTenantLink = this.salesforceTenantLink?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a SalesforceTenantLinkChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of SalesforceTenantLinkChangeHistory objects to avoid using the SalesforceTenantLinkChangeHistory entity type directly.
		///
		/// </summary>
		public static List<SalesforceTenantLinkChangeHistoryOutputDTO> ToOutputDTOList(List<SalesforceTenantLinkChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SalesforceTenantLinkChangeHistoryOutputDTO> output = new List<SalesforceTenantLinkChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (SalesforceTenantLinkChangeHistory salesforceTenantLinkChangeHistory in data)
			{
				output.Add(salesforceTenantLinkChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SalesforceTenantLinkChangeHistory Object.
		///
		/// </summary>
		public static Database.SalesforceTenantLinkChangeHistory FromDTO(SalesforceTenantLinkChangeHistoryDTO dto)
		{
			return new Database.SalesforceTenantLinkChangeHistory
			{
				id = dto.id,
				salesforceTenantLinkId = dto.salesforceTenantLinkId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SalesforceTenantLinkChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(SalesforceTenantLinkChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.salesforceTenantLinkId = dto.salesforceTenantLinkId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a SalesforceTenantLinkChangeHistory Object.
		///
		/// </summary>
		public SalesforceTenantLinkChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SalesforceTenantLinkChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				salesforceTenantLinkId = this.salesforceTenantLinkId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SalesforceTenantLinkChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SalesforceTenantLinkChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SalesforceTenantLinkChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SalesforceTenantLinkChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SalesforceTenantLinkChangeHistory salesforceTenantLinkChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (salesforceTenantLinkChangeHistory == null)
			{
				return null;
			}

			return new {
				id = salesforceTenantLinkChangeHistory.id,
				salesforceTenantLinkId = salesforceTenantLinkChangeHistory.salesforceTenantLinkId,
				versionNumber = salesforceTenantLinkChangeHistory.versionNumber,
				timeStamp = salesforceTenantLinkChangeHistory.timeStamp,
				userId = salesforceTenantLinkChangeHistory.userId,
				data = salesforceTenantLinkChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SalesforceTenantLinkChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SalesforceTenantLinkChangeHistory salesforceTenantLinkChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (salesforceTenantLinkChangeHistory == null)
			{
				return null;
			}

			return new {
				id = salesforceTenantLinkChangeHistory.id,
				salesforceTenantLinkId = salesforceTenantLinkChangeHistory.salesforceTenantLinkId,
				versionNumber = salesforceTenantLinkChangeHistory.versionNumber,
				timeStamp = salesforceTenantLinkChangeHistory.timeStamp,
				userId = salesforceTenantLinkChangeHistory.userId,
				data = salesforceTenantLinkChangeHistory.data,
				salesforceTenantLink = SalesforceTenantLink.CreateMinimalAnonymous(salesforceTenantLinkChangeHistory.salesforceTenantLink)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SalesforceTenantLinkChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SalesforceTenantLinkChangeHistory salesforceTenantLinkChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (salesforceTenantLinkChangeHistory == null)
			{
				return null;
			}

			return new {
				id = salesforceTenantLinkChangeHistory.id,
				name = salesforceTenantLinkChangeHistory.id,
				description = salesforceTenantLinkChangeHistory.id
			 };
		}
	}
}
