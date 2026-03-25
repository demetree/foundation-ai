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
	public partial class ChargeTypeChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)chargeTypeId; }
			set { chargeTypeId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ChargeTypeChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 chargeTypeId { get; set; }
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
		public class ChargeTypeChangeHistoryOutputDTO : ChargeTypeChangeHistoryDTO
		{
			public ChargeType.ChargeTypeDTO chargeType { get; set; }
		}


		/// <summary>
		///
		/// Converts a ChargeTypeChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ChargeTypeChangeHistoryDTO ToDTO()
		{
			return new ChargeTypeChangeHistoryDTO
			{
				id = this.id,
				chargeTypeId = this.chargeTypeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ChargeTypeChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ChargeTypeChangeHistoryDTO> ToDTOList(List<ChargeTypeChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ChargeTypeChangeHistoryDTO> output = new List<ChargeTypeChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ChargeTypeChangeHistory chargeTypeChangeHistory in data)
			{
				output.Add(chargeTypeChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ChargeTypeChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ChargeTypeChangeHistory Entity type directly.
		///
		/// </summary>
		public ChargeTypeChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ChargeTypeChangeHistoryOutputDTO
			{
				id = this.id,
				chargeTypeId = this.chargeTypeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				chargeType = this.chargeType?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ChargeTypeChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ChargeTypeChangeHistory objects to avoid using the ChargeTypeChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ChargeTypeChangeHistoryOutputDTO> ToOutputDTOList(List<ChargeTypeChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ChargeTypeChangeHistoryOutputDTO> output = new List<ChargeTypeChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ChargeTypeChangeHistory chargeTypeChangeHistory in data)
			{
				output.Add(chargeTypeChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ChargeTypeChangeHistory Object.
		///
		/// </summary>
		public static Database.ChargeTypeChangeHistory FromDTO(ChargeTypeChangeHistoryDTO dto)
		{
			return new Database.ChargeTypeChangeHistory
			{
				id = dto.id,
				chargeTypeId = dto.chargeTypeId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ChargeTypeChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ChargeTypeChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.chargeTypeId = dto.chargeTypeId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ChargeTypeChangeHistory Object.
		///
		/// </summary>
		public ChargeTypeChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ChargeTypeChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				chargeTypeId = this.chargeTypeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ChargeTypeChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ChargeTypeChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ChargeTypeChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ChargeTypeChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ChargeTypeChangeHistory chargeTypeChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (chargeTypeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = chargeTypeChangeHistory.id,
				chargeTypeId = chargeTypeChangeHistory.chargeTypeId,
				versionNumber = chargeTypeChangeHistory.versionNumber,
				timeStamp = chargeTypeChangeHistory.timeStamp,
				userId = chargeTypeChangeHistory.userId,
				data = chargeTypeChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ChargeTypeChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ChargeTypeChangeHistory chargeTypeChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (chargeTypeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = chargeTypeChangeHistory.id,
				chargeTypeId = chargeTypeChangeHistory.chargeTypeId,
				versionNumber = chargeTypeChangeHistory.versionNumber,
				timeStamp = chargeTypeChangeHistory.timeStamp,
				userId = chargeTypeChangeHistory.userId,
				data = chargeTypeChangeHistory.data,
				chargeType = ChargeType.CreateMinimalAnonymous(chargeTypeChangeHistory.chargeType),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ChargeTypeChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ChargeTypeChangeHistory chargeTypeChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (chargeTypeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = chargeTypeChangeHistory.id,
				name = chargeTypeChangeHistory.id,
				description = chargeTypeChangeHistory.id
			 };
		}
	}
}
