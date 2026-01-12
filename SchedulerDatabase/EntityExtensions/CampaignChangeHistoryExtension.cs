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
	public partial class CampaignChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)campaignId; }
			set { campaignId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class CampaignChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 campaignId { get; set; }
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
		public class CampaignChangeHistoryOutputDTO : CampaignChangeHistoryDTO
		{
			public Campaign.CampaignDTO campaign { get; set; }
		}


		/// <summary>
		///
		/// Converts a CampaignChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public CampaignChangeHistoryDTO ToDTO()
		{
			return new CampaignChangeHistoryDTO
			{
				id = this.id,
				campaignId = this.campaignId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a CampaignChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<CampaignChangeHistoryDTO> ToDTOList(List<CampaignChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<CampaignChangeHistoryDTO> output = new List<CampaignChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (CampaignChangeHistory campaignChangeHistory in data)
			{
				output.Add(campaignChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a CampaignChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the CampaignChangeHistoryEntity type directly.
		///
		/// </summary>
		public CampaignChangeHistoryOutputDTO ToOutputDTO()
		{
			return new CampaignChangeHistoryOutputDTO
			{
				id = this.id,
				campaignId = this.campaignId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				campaign = this.campaign?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a CampaignChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of CampaignChangeHistory objects to avoid using the CampaignChangeHistory entity type directly.
		///
		/// </summary>
		public static List<CampaignChangeHistoryOutputDTO> ToOutputDTOList(List<CampaignChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<CampaignChangeHistoryOutputDTO> output = new List<CampaignChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (CampaignChangeHistory campaignChangeHistory in data)
			{
				output.Add(campaignChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a CampaignChangeHistory Object.
		///
		/// </summary>
		public static Database.CampaignChangeHistory FromDTO(CampaignChangeHistoryDTO dto)
		{
			return new Database.CampaignChangeHistory
			{
				id = dto.id,
				campaignId = dto.campaignId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a CampaignChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(CampaignChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.campaignId = dto.campaignId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a CampaignChangeHistory Object.
		///
		/// </summary>
		public CampaignChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new CampaignChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				campaignId = this.campaignId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a CampaignChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a CampaignChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a CampaignChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a CampaignChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.CampaignChangeHistory campaignChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (campaignChangeHistory == null)
			{
				return null;
			}

			return new {
				id = campaignChangeHistory.id,
				campaignId = campaignChangeHistory.campaignId,
				versionNumber = campaignChangeHistory.versionNumber,
				timeStamp = campaignChangeHistory.timeStamp,
				userId = campaignChangeHistory.userId,
				data = campaignChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a CampaignChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(CampaignChangeHistory campaignChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (campaignChangeHistory == null)
			{
				return null;
			}

			return new {
				id = campaignChangeHistory.id,
				campaignId = campaignChangeHistory.campaignId,
				versionNumber = campaignChangeHistory.versionNumber,
				timeStamp = campaignChangeHistory.timeStamp,
				userId = campaignChangeHistory.userId,
				data = campaignChangeHistory.data,
				campaign = Campaign.CreateMinimalAnonymous(campaignChangeHistory.campaign)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a CampaignChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(CampaignChangeHistory campaignChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (campaignChangeHistory == null)
			{
				return null;
			}

			return new {
				id = campaignChangeHistory.id,
				name = campaignChangeHistory.id,
				description = campaignChangeHistory.id
			 };
		}
	}
}
