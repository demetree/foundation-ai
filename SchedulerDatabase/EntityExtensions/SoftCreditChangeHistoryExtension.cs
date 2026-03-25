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
	public partial class SoftCreditChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)softCreditId; }
			set { softCreditId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SoftCreditChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 softCreditId { get; set; }
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
		public class SoftCreditChangeHistoryOutputDTO : SoftCreditChangeHistoryDTO
		{
			public SoftCredit.SoftCreditDTO softCredit { get; set; }
		}


		/// <summary>
		///
		/// Converts a SoftCreditChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SoftCreditChangeHistoryDTO ToDTO()
		{
			return new SoftCreditChangeHistoryDTO
			{
				id = this.id,
				softCreditId = this.softCreditId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a SoftCreditChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SoftCreditChangeHistoryDTO> ToDTOList(List<SoftCreditChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SoftCreditChangeHistoryDTO> output = new List<SoftCreditChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (SoftCreditChangeHistory softCreditChangeHistory in data)
			{
				output.Add(softCreditChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SoftCreditChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SoftCreditChangeHistory Entity type directly.
		///
		/// </summary>
		public SoftCreditChangeHistoryOutputDTO ToOutputDTO()
		{
			return new SoftCreditChangeHistoryOutputDTO
			{
				id = this.id,
				softCreditId = this.softCreditId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				softCredit = this.softCredit?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a SoftCreditChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of SoftCreditChangeHistory objects to avoid using the SoftCreditChangeHistory entity type directly.
		///
		/// </summary>
		public static List<SoftCreditChangeHistoryOutputDTO> ToOutputDTOList(List<SoftCreditChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SoftCreditChangeHistoryOutputDTO> output = new List<SoftCreditChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (SoftCreditChangeHistory softCreditChangeHistory in data)
			{
				output.Add(softCreditChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SoftCreditChangeHistory Object.
		///
		/// </summary>
		public static Database.SoftCreditChangeHistory FromDTO(SoftCreditChangeHistoryDTO dto)
		{
			return new Database.SoftCreditChangeHistory
			{
				id = dto.id,
				softCreditId = dto.softCreditId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SoftCreditChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(SoftCreditChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.softCreditId = dto.softCreditId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a SoftCreditChangeHistory Object.
		///
		/// </summary>
		public SoftCreditChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SoftCreditChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				softCreditId = this.softCreditId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SoftCreditChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SoftCreditChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SoftCreditChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SoftCreditChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SoftCreditChangeHistory softCreditChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (softCreditChangeHistory == null)
			{
				return null;
			}

			return new {
				id = softCreditChangeHistory.id,
				softCreditId = softCreditChangeHistory.softCreditId,
				versionNumber = softCreditChangeHistory.versionNumber,
				timeStamp = softCreditChangeHistory.timeStamp,
				userId = softCreditChangeHistory.userId,
				data = softCreditChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SoftCreditChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SoftCreditChangeHistory softCreditChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (softCreditChangeHistory == null)
			{
				return null;
			}

			return new {
				id = softCreditChangeHistory.id,
				softCreditId = softCreditChangeHistory.softCreditId,
				versionNumber = softCreditChangeHistory.versionNumber,
				timeStamp = softCreditChangeHistory.timeStamp,
				userId = softCreditChangeHistory.userId,
				data = softCreditChangeHistory.data,
				softCredit = SoftCredit.CreateMinimalAnonymous(softCreditChangeHistory.softCredit),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SoftCreditChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SoftCreditChangeHistory softCreditChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (softCreditChangeHistory == null)
			{
				return null;
			}

			return new {
				id = softCreditChangeHistory.id,
				name = softCreditChangeHistory.id,
				description = softCreditChangeHistory.id
			 };
		}
	}
}
