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
	public partial class HouseholdChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)householdId; }
			set { householdId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class HouseholdChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 householdId { get; set; }
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
		public class HouseholdChangeHistoryOutputDTO : HouseholdChangeHistoryDTO
		{
			public Household.HouseholdDTO household { get; set; }
		}


		/// <summary>
		///
		/// Converts a HouseholdChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public HouseholdChangeHistoryDTO ToDTO()
		{
			return new HouseholdChangeHistoryDTO
			{
				id = this.id,
				householdId = this.householdId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a HouseholdChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<HouseholdChangeHistoryDTO> ToDTOList(List<HouseholdChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<HouseholdChangeHistoryDTO> output = new List<HouseholdChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (HouseholdChangeHistory householdChangeHistory in data)
			{
				output.Add(householdChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a HouseholdChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the HouseholdChangeHistoryEntity type directly.
		///
		/// </summary>
		public HouseholdChangeHistoryOutputDTO ToOutputDTO()
		{
			return new HouseholdChangeHistoryOutputDTO
			{
				id = this.id,
				householdId = this.householdId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				household = this.household?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a HouseholdChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of HouseholdChangeHistory objects to avoid using the HouseholdChangeHistory entity type directly.
		///
		/// </summary>
		public static List<HouseholdChangeHistoryOutputDTO> ToOutputDTOList(List<HouseholdChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<HouseholdChangeHistoryOutputDTO> output = new List<HouseholdChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (HouseholdChangeHistory householdChangeHistory in data)
			{
				output.Add(householdChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a HouseholdChangeHistory Object.
		///
		/// </summary>
		public static Database.HouseholdChangeHistory FromDTO(HouseholdChangeHistoryDTO dto)
		{
			return new Database.HouseholdChangeHistory
			{
				id = dto.id,
				householdId = dto.householdId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a HouseholdChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(HouseholdChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.householdId = dto.householdId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a HouseholdChangeHistory Object.
		///
		/// </summary>
		public HouseholdChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new HouseholdChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				householdId = this.householdId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a HouseholdChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a HouseholdChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a HouseholdChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a HouseholdChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.HouseholdChangeHistory householdChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (householdChangeHistory == null)
			{
				return null;
			}

			return new {
				id = householdChangeHistory.id,
				householdId = householdChangeHistory.householdId,
				versionNumber = householdChangeHistory.versionNumber,
				timeStamp = householdChangeHistory.timeStamp,
				userId = householdChangeHistory.userId,
				data = householdChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a HouseholdChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(HouseholdChangeHistory householdChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (householdChangeHistory == null)
			{
				return null;
			}

			return new {
				id = householdChangeHistory.id,
				householdId = householdChangeHistory.householdId,
				versionNumber = householdChangeHistory.versionNumber,
				timeStamp = householdChangeHistory.timeStamp,
				userId = householdChangeHistory.userId,
				data = householdChangeHistory.data,
				household = Household.CreateMinimalAnonymous(householdChangeHistory.household)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a HouseholdChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(HouseholdChangeHistory householdChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (householdChangeHistory == null)
			{
				return null;
			}

			return new {
				id = householdChangeHistory.id,
				name = householdChangeHistory.id,
				description = householdChangeHistory.id
			 };
		}
	}
}
