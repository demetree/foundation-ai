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
	public partial class FinancialOfficeChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)financialOfficeId; }
			set { financialOfficeId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class FinancialOfficeChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 financialOfficeId { get; set; }
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
		public class FinancialOfficeChangeHistoryOutputDTO : FinancialOfficeChangeHistoryDTO
		{
			public FinancialOffice.FinancialOfficeDTO financialOffice { get; set; }
		}


		/// <summary>
		///
		/// Converts a FinancialOfficeChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public FinancialOfficeChangeHistoryDTO ToDTO()
		{
			return new FinancialOfficeChangeHistoryDTO
			{
				id = this.id,
				financialOfficeId = this.financialOfficeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a FinancialOfficeChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<FinancialOfficeChangeHistoryDTO> ToDTOList(List<FinancialOfficeChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<FinancialOfficeChangeHistoryDTO> output = new List<FinancialOfficeChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (FinancialOfficeChangeHistory financialOfficeChangeHistory in data)
			{
				output.Add(financialOfficeChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a FinancialOfficeChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the FinancialOfficeChangeHistoryEntity type directly.
		///
		/// </summary>
		public FinancialOfficeChangeHistoryOutputDTO ToOutputDTO()
		{
			return new FinancialOfficeChangeHistoryOutputDTO
			{
				id = this.id,
				financialOfficeId = this.financialOfficeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				financialOffice = this.financialOffice?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a FinancialOfficeChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of FinancialOfficeChangeHistory objects to avoid using the FinancialOfficeChangeHistory entity type directly.
		///
		/// </summary>
		public static List<FinancialOfficeChangeHistoryOutputDTO> ToOutputDTOList(List<FinancialOfficeChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<FinancialOfficeChangeHistoryOutputDTO> output = new List<FinancialOfficeChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (FinancialOfficeChangeHistory financialOfficeChangeHistory in data)
			{
				output.Add(financialOfficeChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a FinancialOfficeChangeHistory Object.
		///
		/// </summary>
		public static Database.FinancialOfficeChangeHistory FromDTO(FinancialOfficeChangeHistoryDTO dto)
		{
			return new Database.FinancialOfficeChangeHistory
			{
				id = dto.id,
				financialOfficeId = dto.financialOfficeId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a FinancialOfficeChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(FinancialOfficeChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.financialOfficeId = dto.financialOfficeId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a FinancialOfficeChangeHistory Object.
		///
		/// </summary>
		public FinancialOfficeChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new FinancialOfficeChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				financialOfficeId = this.financialOfficeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a FinancialOfficeChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a FinancialOfficeChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a FinancialOfficeChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a FinancialOfficeChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.FinancialOfficeChangeHistory financialOfficeChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (financialOfficeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = financialOfficeChangeHistory.id,
				financialOfficeId = financialOfficeChangeHistory.financialOfficeId,
				versionNumber = financialOfficeChangeHistory.versionNumber,
				timeStamp = financialOfficeChangeHistory.timeStamp,
				userId = financialOfficeChangeHistory.userId,
				data = financialOfficeChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a FinancialOfficeChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(FinancialOfficeChangeHistory financialOfficeChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (financialOfficeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = financialOfficeChangeHistory.id,
				financialOfficeId = financialOfficeChangeHistory.financialOfficeId,
				versionNumber = financialOfficeChangeHistory.versionNumber,
				timeStamp = financialOfficeChangeHistory.timeStamp,
				userId = financialOfficeChangeHistory.userId,
				data = financialOfficeChangeHistory.data,
				financialOffice = FinancialOffice.CreateMinimalAnonymous(financialOfficeChangeHistory.financialOffice),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a FinancialOfficeChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(FinancialOfficeChangeHistory financialOfficeChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (financialOfficeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = financialOfficeChangeHistory.id,
				name = financialOfficeChangeHistory.id,
				description = financialOfficeChangeHistory.id
			 };
		}
	}
}
