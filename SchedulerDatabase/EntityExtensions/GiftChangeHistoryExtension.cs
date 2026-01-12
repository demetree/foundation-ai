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
	public partial class GiftChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)giftId; }
			set { giftId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class GiftChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 giftId { get; set; }
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
		public class GiftChangeHistoryOutputDTO : GiftChangeHistoryDTO
		{
			public Gift.GiftDTO gift { get; set; }
		}


		/// <summary>
		///
		/// Converts a GiftChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public GiftChangeHistoryDTO ToDTO()
		{
			return new GiftChangeHistoryDTO
			{
				id = this.id,
				giftId = this.giftId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a GiftChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<GiftChangeHistoryDTO> ToDTOList(List<GiftChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<GiftChangeHistoryDTO> output = new List<GiftChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (GiftChangeHistory giftChangeHistory in data)
			{
				output.Add(giftChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a GiftChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the GiftChangeHistoryEntity type directly.
		///
		/// </summary>
		public GiftChangeHistoryOutputDTO ToOutputDTO()
		{
			return new GiftChangeHistoryOutputDTO
			{
				id = this.id,
				giftId = this.giftId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				gift = this.gift?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a GiftChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of GiftChangeHistory objects to avoid using the GiftChangeHistory entity type directly.
		///
		/// </summary>
		public static List<GiftChangeHistoryOutputDTO> ToOutputDTOList(List<GiftChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<GiftChangeHistoryOutputDTO> output = new List<GiftChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (GiftChangeHistory giftChangeHistory in data)
			{
				output.Add(giftChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a GiftChangeHistory Object.
		///
		/// </summary>
		public static Database.GiftChangeHistory FromDTO(GiftChangeHistoryDTO dto)
		{
			return new Database.GiftChangeHistory
			{
				id = dto.id,
				giftId = dto.giftId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a GiftChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(GiftChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.giftId = dto.giftId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a GiftChangeHistory Object.
		///
		/// </summary>
		public GiftChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new GiftChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				giftId = this.giftId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a GiftChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a GiftChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a GiftChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a GiftChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.GiftChangeHistory giftChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (giftChangeHistory == null)
			{
				return null;
			}

			return new {
				id = giftChangeHistory.id,
				giftId = giftChangeHistory.giftId,
				versionNumber = giftChangeHistory.versionNumber,
				timeStamp = giftChangeHistory.timeStamp,
				userId = giftChangeHistory.userId,
				data = giftChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a GiftChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(GiftChangeHistory giftChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (giftChangeHistory == null)
			{
				return null;
			}

			return new {
				id = giftChangeHistory.id,
				giftId = giftChangeHistory.giftId,
				versionNumber = giftChangeHistory.versionNumber,
				timeStamp = giftChangeHistory.timeStamp,
				userId = giftChangeHistory.userId,
				data = giftChangeHistory.data,
				gift = Gift.CreateMinimalAnonymous(giftChangeHistory.gift)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a GiftChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(GiftChangeHistory giftChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (giftChangeHistory == null)
			{
				return null;
			}

			return new {
				id = giftChangeHistory.id,
				name = giftChangeHistory.id,
				description = giftChangeHistory.id
			 };
		}
	}
}
