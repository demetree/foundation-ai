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
	public partial class FiscalPeriodChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)fiscalPeriodId; }
			set { fiscalPeriodId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class FiscalPeriodChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 fiscalPeriodId { get; set; }
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
		public class FiscalPeriodChangeHistoryOutputDTO : FiscalPeriodChangeHistoryDTO
		{
			public FiscalPeriod.FiscalPeriodDTO fiscalPeriod { get; set; }
		}


		/// <summary>
		///
		/// Converts a FiscalPeriodChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public FiscalPeriodChangeHistoryDTO ToDTO()
		{
			return new FiscalPeriodChangeHistoryDTO
			{
				id = this.id,
				fiscalPeriodId = this.fiscalPeriodId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a FiscalPeriodChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<FiscalPeriodChangeHistoryDTO> ToDTOList(List<FiscalPeriodChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<FiscalPeriodChangeHistoryDTO> output = new List<FiscalPeriodChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (FiscalPeriodChangeHistory fiscalPeriodChangeHistory in data)
			{
				output.Add(fiscalPeriodChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a FiscalPeriodChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the FiscalPeriodChangeHistoryEntity type directly.
		///
		/// </summary>
		public FiscalPeriodChangeHistoryOutputDTO ToOutputDTO()
		{
			return new FiscalPeriodChangeHistoryOutputDTO
			{
				id = this.id,
				fiscalPeriodId = this.fiscalPeriodId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				fiscalPeriod = this.fiscalPeriod?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a FiscalPeriodChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of FiscalPeriodChangeHistory objects to avoid using the FiscalPeriodChangeHistory entity type directly.
		///
		/// </summary>
		public static List<FiscalPeriodChangeHistoryOutputDTO> ToOutputDTOList(List<FiscalPeriodChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<FiscalPeriodChangeHistoryOutputDTO> output = new List<FiscalPeriodChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (FiscalPeriodChangeHistory fiscalPeriodChangeHistory in data)
			{
				output.Add(fiscalPeriodChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a FiscalPeriodChangeHistory Object.
		///
		/// </summary>
		public static Database.FiscalPeriodChangeHistory FromDTO(FiscalPeriodChangeHistoryDTO dto)
		{
			return new Database.FiscalPeriodChangeHistory
			{
				id = dto.id,
				fiscalPeriodId = dto.fiscalPeriodId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a FiscalPeriodChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(FiscalPeriodChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.fiscalPeriodId = dto.fiscalPeriodId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a FiscalPeriodChangeHistory Object.
		///
		/// </summary>
		public FiscalPeriodChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new FiscalPeriodChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				fiscalPeriodId = this.fiscalPeriodId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a FiscalPeriodChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a FiscalPeriodChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a FiscalPeriodChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a FiscalPeriodChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.FiscalPeriodChangeHistory fiscalPeriodChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (fiscalPeriodChangeHistory == null)
			{
				return null;
			}

			return new {
				id = fiscalPeriodChangeHistory.id,
				fiscalPeriodId = fiscalPeriodChangeHistory.fiscalPeriodId,
				versionNumber = fiscalPeriodChangeHistory.versionNumber,
				timeStamp = fiscalPeriodChangeHistory.timeStamp,
				userId = fiscalPeriodChangeHistory.userId,
				data = fiscalPeriodChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a FiscalPeriodChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(FiscalPeriodChangeHistory fiscalPeriodChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (fiscalPeriodChangeHistory == null)
			{
				return null;
			}

			return new {
				id = fiscalPeriodChangeHistory.id,
				fiscalPeriodId = fiscalPeriodChangeHistory.fiscalPeriodId,
				versionNumber = fiscalPeriodChangeHistory.versionNumber,
				timeStamp = fiscalPeriodChangeHistory.timeStamp,
				userId = fiscalPeriodChangeHistory.userId,
				data = fiscalPeriodChangeHistory.data,
				fiscalPeriod = FiscalPeriod.CreateMinimalAnonymous(fiscalPeriodChangeHistory.fiscalPeriod)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a FiscalPeriodChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(FiscalPeriodChangeHistory fiscalPeriodChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (fiscalPeriodChangeHistory == null)
			{
				return null;
			}

			return new {
				id = fiscalPeriodChangeHistory.id,
				name = fiscalPeriodChangeHistory.id,
				description = fiscalPeriodChangeHistory.id
			 };
		}
	}
}
