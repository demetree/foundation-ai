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
	public partial class BudgetChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)budgetId; }
			set { budgetId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BudgetChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 budgetId { get; set; }
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
		public class BudgetChangeHistoryOutputDTO : BudgetChangeHistoryDTO
		{
			public Budget.BudgetDTO budget { get; set; }
		}


		/// <summary>
		///
		/// Converts a BudgetChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BudgetChangeHistoryDTO ToDTO()
		{
			return new BudgetChangeHistoryDTO
			{
				id = this.id,
				budgetId = this.budgetId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a BudgetChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BudgetChangeHistoryDTO> ToDTOList(List<BudgetChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BudgetChangeHistoryDTO> output = new List<BudgetChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (BudgetChangeHistory budgetChangeHistory in data)
			{
				output.Add(budgetChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BudgetChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BudgetChangeHistory Entity type directly.
		///
		/// </summary>
		public BudgetChangeHistoryOutputDTO ToOutputDTO()
		{
			return new BudgetChangeHistoryOutputDTO
			{
				id = this.id,
				budgetId = this.budgetId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				budget = this.budget?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a BudgetChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of BudgetChangeHistory objects to avoid using the BudgetChangeHistory entity type directly.
		///
		/// </summary>
		public static List<BudgetChangeHistoryOutputDTO> ToOutputDTOList(List<BudgetChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BudgetChangeHistoryOutputDTO> output = new List<BudgetChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (BudgetChangeHistory budgetChangeHistory in data)
			{
				output.Add(budgetChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BudgetChangeHistory Object.
		///
		/// </summary>
		public static Database.BudgetChangeHistory FromDTO(BudgetChangeHistoryDTO dto)
		{
			return new Database.BudgetChangeHistory
			{
				id = dto.id,
				budgetId = dto.budgetId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BudgetChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(BudgetChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.budgetId = dto.budgetId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a BudgetChangeHistory Object.
		///
		/// </summary>
		public BudgetChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BudgetChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				budgetId = this.budgetId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BudgetChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BudgetChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BudgetChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BudgetChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BudgetChangeHistory budgetChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (budgetChangeHistory == null)
			{
				return null;
			}

			return new {
				id = budgetChangeHistory.id,
				budgetId = budgetChangeHistory.budgetId,
				versionNumber = budgetChangeHistory.versionNumber,
				timeStamp = budgetChangeHistory.timeStamp,
				userId = budgetChangeHistory.userId,
				data = budgetChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BudgetChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BudgetChangeHistory budgetChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (budgetChangeHistory == null)
			{
				return null;
			}

			return new {
				id = budgetChangeHistory.id,
				budgetId = budgetChangeHistory.budgetId,
				versionNumber = budgetChangeHistory.versionNumber,
				timeStamp = budgetChangeHistory.timeStamp,
				userId = budgetChangeHistory.userId,
				data = budgetChangeHistory.data,
				budget = Budget.CreateMinimalAnonymous(budgetChangeHistory.budget)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BudgetChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BudgetChangeHistory budgetChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (budgetChangeHistory == null)
			{
				return null;
			}

			return new {
				id = budgetChangeHistory.id,
				name = budgetChangeHistory.id,
				description = budgetChangeHistory.id
			 };
		}
	}
}
