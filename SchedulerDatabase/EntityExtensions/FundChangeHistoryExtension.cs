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
	public partial class FundChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)fundId; }
			set { fundId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class FundChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 fundId { get; set; }
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
		public class FundChangeHistoryOutputDTO : FundChangeHistoryDTO
		{
			public Fund.FundDTO fund { get; set; }
		}


		/// <summary>
		///
		/// Converts a FundChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public FundChangeHistoryDTO ToDTO()
		{
			return new FundChangeHistoryDTO
			{
				id = this.id,
				fundId = this.fundId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a FundChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<FundChangeHistoryDTO> ToDTOList(List<FundChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<FundChangeHistoryDTO> output = new List<FundChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (FundChangeHistory fundChangeHistory in data)
			{
				output.Add(fundChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a FundChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the FundChangeHistoryEntity type directly.
		///
		/// </summary>
		public FundChangeHistoryOutputDTO ToOutputDTO()
		{
			return new FundChangeHistoryOutputDTO
			{
				id = this.id,
				fundId = this.fundId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				fund = this.fund?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a FundChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of FundChangeHistory objects to avoid using the FundChangeHistory entity type directly.
		///
		/// </summary>
		public static List<FundChangeHistoryOutputDTO> ToOutputDTOList(List<FundChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<FundChangeHistoryOutputDTO> output = new List<FundChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (FundChangeHistory fundChangeHistory in data)
			{
				output.Add(fundChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a FundChangeHistory Object.
		///
		/// </summary>
		public static Database.FundChangeHistory FromDTO(FundChangeHistoryDTO dto)
		{
			return new Database.FundChangeHistory
			{
				id = dto.id,
				fundId = dto.fundId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a FundChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(FundChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.fundId = dto.fundId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a FundChangeHistory Object.
		///
		/// </summary>
		public FundChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new FundChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				fundId = this.fundId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a FundChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a FundChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a FundChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a FundChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.FundChangeHistory fundChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (fundChangeHistory == null)
			{
				return null;
			}

			return new {
				id = fundChangeHistory.id,
				fundId = fundChangeHistory.fundId,
				versionNumber = fundChangeHistory.versionNumber,
				timeStamp = fundChangeHistory.timeStamp,
				userId = fundChangeHistory.userId,
				data = fundChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a FundChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(FundChangeHistory fundChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (fundChangeHistory == null)
			{
				return null;
			}

			return new {
				id = fundChangeHistory.id,
				fundId = fundChangeHistory.fundId,
				versionNumber = fundChangeHistory.versionNumber,
				timeStamp = fundChangeHistory.timeStamp,
				userId = fundChangeHistory.userId,
				data = fundChangeHistory.data,
				fund = Fund.CreateMinimalAnonymous(fundChangeHistory.fund),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a FundChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(FundChangeHistory fundChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (fundChangeHistory == null)
			{
				return null;
			}

			return new {
				id = fundChangeHistory.id,
				name = fundChangeHistory.id,
				description = fundChangeHistory.id
			 };
		}
	}
}
