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
	public partial class ReceiptTypeChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)receiptTypeId; }
			set { receiptTypeId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ReceiptTypeChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 receiptTypeId { get; set; }
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
		public class ReceiptTypeChangeHistoryOutputDTO : ReceiptTypeChangeHistoryDTO
		{
			public ReceiptType.ReceiptTypeDTO receiptType { get; set; }
		}


		/// <summary>
		///
		/// Converts a ReceiptTypeChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ReceiptTypeChangeHistoryDTO ToDTO()
		{
			return new ReceiptTypeChangeHistoryDTO
			{
				id = this.id,
				receiptTypeId = this.receiptTypeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ReceiptTypeChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ReceiptTypeChangeHistoryDTO> ToDTOList(List<ReceiptTypeChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ReceiptTypeChangeHistoryDTO> output = new List<ReceiptTypeChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ReceiptTypeChangeHistory receiptTypeChangeHistory in data)
			{
				output.Add(receiptTypeChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ReceiptTypeChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ReceiptTypeChangeHistoryEntity type directly.
		///
		/// </summary>
		public ReceiptTypeChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ReceiptTypeChangeHistoryOutputDTO
			{
				id = this.id,
				receiptTypeId = this.receiptTypeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				receiptType = this.receiptType?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ReceiptTypeChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ReceiptTypeChangeHistory objects to avoid using the ReceiptTypeChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ReceiptTypeChangeHistoryOutputDTO> ToOutputDTOList(List<ReceiptTypeChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ReceiptTypeChangeHistoryOutputDTO> output = new List<ReceiptTypeChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ReceiptTypeChangeHistory receiptTypeChangeHistory in data)
			{
				output.Add(receiptTypeChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ReceiptTypeChangeHistory Object.
		///
		/// </summary>
		public static Database.ReceiptTypeChangeHistory FromDTO(ReceiptTypeChangeHistoryDTO dto)
		{
			return new Database.ReceiptTypeChangeHistory
			{
				id = dto.id,
				receiptTypeId = dto.receiptTypeId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ReceiptTypeChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ReceiptTypeChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.receiptTypeId = dto.receiptTypeId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ReceiptTypeChangeHistory Object.
		///
		/// </summary>
		public ReceiptTypeChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ReceiptTypeChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				receiptTypeId = this.receiptTypeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ReceiptTypeChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ReceiptTypeChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ReceiptTypeChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ReceiptTypeChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ReceiptTypeChangeHistory receiptTypeChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (receiptTypeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = receiptTypeChangeHistory.id,
				receiptTypeId = receiptTypeChangeHistory.receiptTypeId,
				versionNumber = receiptTypeChangeHistory.versionNumber,
				timeStamp = receiptTypeChangeHistory.timeStamp,
				userId = receiptTypeChangeHistory.userId,
				data = receiptTypeChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ReceiptTypeChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ReceiptTypeChangeHistory receiptTypeChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (receiptTypeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = receiptTypeChangeHistory.id,
				receiptTypeId = receiptTypeChangeHistory.receiptTypeId,
				versionNumber = receiptTypeChangeHistory.versionNumber,
				timeStamp = receiptTypeChangeHistory.timeStamp,
				userId = receiptTypeChangeHistory.userId,
				data = receiptTypeChangeHistory.data,
				receiptType = ReceiptType.CreateMinimalAnonymous(receiptTypeChangeHistory.receiptType),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ReceiptTypeChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ReceiptTypeChangeHistory receiptTypeChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (receiptTypeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = receiptTypeChangeHistory.id,
				name = receiptTypeChangeHistory.id,
				description = receiptTypeChangeHistory.id
			 };
		}
	}
}
