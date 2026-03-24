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
	public partial class RateSheetChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)rateSheetId; }
			set { rateSheetId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class RateSheetChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 rateSheetId { get; set; }
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
		public class RateSheetChangeHistoryOutputDTO : RateSheetChangeHistoryDTO
		{
			public RateSheet.RateSheetDTO rateSheet { get; set; }
		}


		/// <summary>
		///
		/// Converts a RateSheetChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public RateSheetChangeHistoryDTO ToDTO()
		{
			return new RateSheetChangeHistoryDTO
			{
				id = this.id,
				rateSheetId = this.rateSheetId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a RateSheetChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<RateSheetChangeHistoryDTO> ToDTOList(List<RateSheetChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<RateSheetChangeHistoryDTO> output = new List<RateSheetChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (RateSheetChangeHistory rateSheetChangeHistory in data)
			{
				output.Add(rateSheetChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a RateSheetChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the RateSheetChangeHistory Entity type directly.
		///
		/// </summary>
		public RateSheetChangeHistoryOutputDTO ToOutputDTO()
		{
			return new RateSheetChangeHistoryOutputDTO
			{
				id = this.id,
				rateSheetId = this.rateSheetId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				rateSheet = this.rateSheet?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a RateSheetChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of RateSheetChangeHistory objects to avoid using the RateSheetChangeHistory entity type directly.
		///
		/// </summary>
		public static List<RateSheetChangeHistoryOutputDTO> ToOutputDTOList(List<RateSheetChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<RateSheetChangeHistoryOutputDTO> output = new List<RateSheetChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (RateSheetChangeHistory rateSheetChangeHistory in data)
			{
				output.Add(rateSheetChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a RateSheetChangeHistory Object.
		///
		/// </summary>
		public static Database.RateSheetChangeHistory FromDTO(RateSheetChangeHistoryDTO dto)
		{
			return new Database.RateSheetChangeHistory
			{
				id = dto.id,
				rateSheetId = dto.rateSheetId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a RateSheetChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(RateSheetChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.rateSheetId = dto.rateSheetId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a RateSheetChangeHistory Object.
		///
		/// </summary>
		public RateSheetChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new RateSheetChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				rateSheetId = this.rateSheetId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a RateSheetChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a RateSheetChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a RateSheetChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a RateSheetChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.RateSheetChangeHistory rateSheetChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (rateSheetChangeHistory == null)
			{
				return null;
			}

			return new {
				id = rateSheetChangeHistory.id,
				rateSheetId = rateSheetChangeHistory.rateSheetId,
				versionNumber = rateSheetChangeHistory.versionNumber,
				timeStamp = rateSheetChangeHistory.timeStamp,
				userId = rateSheetChangeHistory.userId,
				data = rateSheetChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a RateSheetChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(RateSheetChangeHistory rateSheetChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (rateSheetChangeHistory == null)
			{
				return null;
			}

			return new {
				id = rateSheetChangeHistory.id,
				rateSheetId = rateSheetChangeHistory.rateSheetId,
				versionNumber = rateSheetChangeHistory.versionNumber,
				timeStamp = rateSheetChangeHistory.timeStamp,
				userId = rateSheetChangeHistory.userId,
				data = rateSheetChangeHistory.data,
				rateSheet = RateSheet.CreateMinimalAnonymous(rateSheetChangeHistory.rateSheet)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a RateSheetChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(RateSheetChangeHistory rateSheetChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (rateSheetChangeHistory == null)
			{
				return null;
			}

			return new {
				id = rateSheetChangeHistory.id,
				name = rateSheetChangeHistory.id,
				description = rateSheetChangeHistory.id
			 };
		}
	}
}
