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
	public partial class ShiftPatternChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)shiftPatternId; }
			set { shiftPatternId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ShiftPatternChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 shiftPatternId { get; set; }
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
		public class ShiftPatternChangeHistoryOutputDTO : ShiftPatternChangeHistoryDTO
		{
			public ShiftPattern.ShiftPatternDTO shiftPattern { get; set; }
		}


		/// <summary>
		///
		/// Converts a ShiftPatternChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ShiftPatternChangeHistoryDTO ToDTO()
		{
			return new ShiftPatternChangeHistoryDTO
			{
				id = this.id,
				shiftPatternId = this.shiftPatternId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ShiftPatternChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ShiftPatternChangeHistoryDTO> ToDTOList(List<ShiftPatternChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ShiftPatternChangeHistoryDTO> output = new List<ShiftPatternChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ShiftPatternChangeHistory shiftPatternChangeHistory in data)
			{
				output.Add(shiftPatternChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ShiftPatternChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ShiftPatternChangeHistory Entity type directly.
		///
		/// </summary>
		public ShiftPatternChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ShiftPatternChangeHistoryOutputDTO
			{
				id = this.id,
				shiftPatternId = this.shiftPatternId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				shiftPattern = this.shiftPattern?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ShiftPatternChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ShiftPatternChangeHistory objects to avoid using the ShiftPatternChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ShiftPatternChangeHistoryOutputDTO> ToOutputDTOList(List<ShiftPatternChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ShiftPatternChangeHistoryOutputDTO> output = new List<ShiftPatternChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ShiftPatternChangeHistory shiftPatternChangeHistory in data)
			{
				output.Add(shiftPatternChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ShiftPatternChangeHistory Object.
		///
		/// </summary>
		public static Database.ShiftPatternChangeHistory FromDTO(ShiftPatternChangeHistoryDTO dto)
		{
			return new Database.ShiftPatternChangeHistory
			{
				id = dto.id,
				shiftPatternId = dto.shiftPatternId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ShiftPatternChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ShiftPatternChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.shiftPatternId = dto.shiftPatternId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ShiftPatternChangeHistory Object.
		///
		/// </summary>
		public ShiftPatternChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ShiftPatternChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				shiftPatternId = this.shiftPatternId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ShiftPatternChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ShiftPatternChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ShiftPatternChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ShiftPatternChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ShiftPatternChangeHistory shiftPatternChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (shiftPatternChangeHistory == null)
			{
				return null;
			}

			return new {
				id = shiftPatternChangeHistory.id,
				shiftPatternId = shiftPatternChangeHistory.shiftPatternId,
				versionNumber = shiftPatternChangeHistory.versionNumber,
				timeStamp = shiftPatternChangeHistory.timeStamp,
				userId = shiftPatternChangeHistory.userId,
				data = shiftPatternChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ShiftPatternChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ShiftPatternChangeHistory shiftPatternChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (shiftPatternChangeHistory == null)
			{
				return null;
			}

			return new {
				id = shiftPatternChangeHistory.id,
				shiftPatternId = shiftPatternChangeHistory.shiftPatternId,
				versionNumber = shiftPatternChangeHistory.versionNumber,
				timeStamp = shiftPatternChangeHistory.timeStamp,
				userId = shiftPatternChangeHistory.userId,
				data = shiftPatternChangeHistory.data,
				shiftPattern = ShiftPattern.CreateMinimalAnonymous(shiftPatternChangeHistory.shiftPattern),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ShiftPatternChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ShiftPatternChangeHistory shiftPatternChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (shiftPatternChangeHistory == null)
			{
				return null;
			}

			return new {
				id = shiftPatternChangeHistory.id,
				name = shiftPatternChangeHistory.id,
				description = shiftPatternChangeHistory.id
			 };
		}
	}
}
