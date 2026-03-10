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
	public partial class ReceiptChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)receiptId; }
			set { receiptId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ReceiptChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 receiptId { get; set; }
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
		public class ReceiptChangeHistoryOutputDTO : ReceiptChangeHistoryDTO
		{
			public Receipt.ReceiptDTO receipt { get; set; }
		}


		/// <summary>
		///
		/// Converts a ReceiptChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ReceiptChangeHistoryDTO ToDTO()
		{
			return new ReceiptChangeHistoryDTO
			{
				id = this.id,
				receiptId = this.receiptId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ReceiptChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ReceiptChangeHistoryDTO> ToDTOList(List<ReceiptChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ReceiptChangeHistoryDTO> output = new List<ReceiptChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ReceiptChangeHistory receiptChangeHistory in data)
			{
				output.Add(receiptChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ReceiptChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ReceiptChangeHistoryEntity type directly.
		///
		/// </summary>
		public ReceiptChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ReceiptChangeHistoryOutputDTO
			{
				id = this.id,
				receiptId = this.receiptId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				receipt = this.receipt?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ReceiptChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ReceiptChangeHistory objects to avoid using the ReceiptChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ReceiptChangeHistoryOutputDTO> ToOutputDTOList(List<ReceiptChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ReceiptChangeHistoryOutputDTO> output = new List<ReceiptChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ReceiptChangeHistory receiptChangeHistory in data)
			{
				output.Add(receiptChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ReceiptChangeHistory Object.
		///
		/// </summary>
		public static Database.ReceiptChangeHistory FromDTO(ReceiptChangeHistoryDTO dto)
		{
			return new Database.ReceiptChangeHistory
			{
				id = dto.id,
				receiptId = dto.receiptId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ReceiptChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ReceiptChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.receiptId = dto.receiptId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ReceiptChangeHistory Object.
		///
		/// </summary>
		public ReceiptChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ReceiptChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				receiptId = this.receiptId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ReceiptChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ReceiptChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ReceiptChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ReceiptChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ReceiptChangeHistory receiptChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (receiptChangeHistory == null)
			{
				return null;
			}

			return new {
				id = receiptChangeHistory.id,
				receiptId = receiptChangeHistory.receiptId,
				versionNumber = receiptChangeHistory.versionNumber,
				timeStamp = receiptChangeHistory.timeStamp,
				userId = receiptChangeHistory.userId,
				data = receiptChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ReceiptChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ReceiptChangeHistory receiptChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (receiptChangeHistory == null)
			{
				return null;
			}

			return new {
				id = receiptChangeHistory.id,
				receiptId = receiptChangeHistory.receiptId,
				versionNumber = receiptChangeHistory.versionNumber,
				timeStamp = receiptChangeHistory.timeStamp,
				userId = receiptChangeHistory.userId,
				data = receiptChangeHistory.data,
				receipt = Receipt.CreateMinimalAnonymous(receiptChangeHistory.receipt)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ReceiptChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ReceiptChangeHistory receiptChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (receiptChangeHistory == null)
			{
				return null;
			}

			return new {
				id = receiptChangeHistory.id,
				name = receiptChangeHistory.id,
				description = receiptChangeHistory.id
			 };
		}
	}
}
