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
	public partial class ChargeStatusChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)chargeStatusId; }
			set { chargeStatusId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ChargeStatusChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 chargeStatusId { get; set; }
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
		public class ChargeStatusChangeHistoryOutputDTO : ChargeStatusChangeHistoryDTO
		{
			public ChargeStatus.ChargeStatusDTO chargeStatus { get; set; }
		}


		/// <summary>
		///
		/// Converts a ChargeStatusChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ChargeStatusChangeHistoryDTO ToDTO()
		{
			return new ChargeStatusChangeHistoryDTO
			{
				id = this.id,
				chargeStatusId = this.chargeStatusId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ChargeStatusChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ChargeStatusChangeHistoryDTO> ToDTOList(List<ChargeStatusChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ChargeStatusChangeHistoryDTO> output = new List<ChargeStatusChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ChargeStatusChangeHistory chargeStatusChangeHistory in data)
			{
				output.Add(chargeStatusChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ChargeStatusChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ChargeStatusChangeHistoryEntity type directly.
		///
		/// </summary>
		public ChargeStatusChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ChargeStatusChangeHistoryOutputDTO
			{
				id = this.id,
				chargeStatusId = this.chargeStatusId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				chargeStatus = this.chargeStatus?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ChargeStatusChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ChargeStatusChangeHistory objects to avoid using the ChargeStatusChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ChargeStatusChangeHistoryOutputDTO> ToOutputDTOList(List<ChargeStatusChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ChargeStatusChangeHistoryOutputDTO> output = new List<ChargeStatusChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ChargeStatusChangeHistory chargeStatusChangeHistory in data)
			{
				output.Add(chargeStatusChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ChargeStatusChangeHistory Object.
		///
		/// </summary>
		public static Database.ChargeStatusChangeHistory FromDTO(ChargeStatusChangeHistoryDTO dto)
		{
			return new Database.ChargeStatusChangeHistory
			{
				id = dto.id,
				chargeStatusId = dto.chargeStatusId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ChargeStatusChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ChargeStatusChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.chargeStatusId = dto.chargeStatusId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ChargeStatusChangeHistory Object.
		///
		/// </summary>
		public ChargeStatusChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ChargeStatusChangeHistory{
				id = this.id,
				chargeStatusId = this.chargeStatusId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ChargeStatusChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ChargeStatusChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ChargeStatusChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ChargeStatusChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ChargeStatusChangeHistory chargeStatusChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (chargeStatusChangeHistory == null)
			{
				return null;
			}

			return new {
				id = chargeStatusChangeHistory.id,
				chargeStatusId = chargeStatusChangeHistory.chargeStatusId,
				versionNumber = chargeStatusChangeHistory.versionNumber,
				timeStamp = chargeStatusChangeHistory.timeStamp,
				userId = chargeStatusChangeHistory.userId,
				data = chargeStatusChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ChargeStatusChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ChargeStatusChangeHistory chargeStatusChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (chargeStatusChangeHistory == null)
			{
				return null;
			}

			return new {
				id = chargeStatusChangeHistory.id,
				chargeStatusId = chargeStatusChangeHistory.chargeStatusId,
				versionNumber = chargeStatusChangeHistory.versionNumber,
				timeStamp = chargeStatusChangeHistory.timeStamp,
				userId = chargeStatusChangeHistory.userId,
				data = chargeStatusChangeHistory.data,
				chargeStatus = ChargeStatus.CreateMinimalAnonymous(chargeStatusChangeHistory.chargeStatus),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ChargeStatusChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ChargeStatusChangeHistory chargeStatusChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (chargeStatusChangeHistory == null)
			{
				return null;
			}

			return new {
				id = chargeStatusChangeHistory.id,
				name = chargeStatusChangeHistory.id,
				description = chargeStatusChangeHistory.id
			 };
		}
	}
}
