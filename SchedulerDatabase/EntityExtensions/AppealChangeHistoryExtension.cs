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
	public partial class AppealChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)appealId; }
			set { appealId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class AppealChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 appealId { get; set; }
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
		public class AppealChangeHistoryOutputDTO : AppealChangeHistoryDTO
		{
			public Appeal.AppealDTO appeal { get; set; }
		}


		/// <summary>
		///
		/// Converts a AppealChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public AppealChangeHistoryDTO ToDTO()
		{
			return new AppealChangeHistoryDTO
			{
				id = this.id,
				appealId = this.appealId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a AppealChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<AppealChangeHistoryDTO> ToDTOList(List<AppealChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AppealChangeHistoryDTO> output = new List<AppealChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (AppealChangeHistory appealChangeHistory in data)
			{
				output.Add(appealChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a AppealChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the AppealChangeHistory Entity type directly.
		///
		/// </summary>
		public AppealChangeHistoryOutputDTO ToOutputDTO()
		{
			return new AppealChangeHistoryOutputDTO
			{
				id = this.id,
				appealId = this.appealId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				appeal = this.appeal?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a AppealChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of AppealChangeHistory objects to avoid using the AppealChangeHistory entity type directly.
		///
		/// </summary>
		public static List<AppealChangeHistoryOutputDTO> ToOutputDTOList(List<AppealChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AppealChangeHistoryOutputDTO> output = new List<AppealChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (AppealChangeHistory appealChangeHistory in data)
			{
				output.Add(appealChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a AppealChangeHistory Object.
		///
		/// </summary>
		public static Database.AppealChangeHistory FromDTO(AppealChangeHistoryDTO dto)
		{
			return new Database.AppealChangeHistory
			{
				id = dto.id,
				appealId = dto.appealId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a AppealChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(AppealChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.appealId = dto.appealId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a AppealChangeHistory Object.
		///
		/// </summary>
		public AppealChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new AppealChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				appealId = this.appealId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AppealChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AppealChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a AppealChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a AppealChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.AppealChangeHistory appealChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (appealChangeHistory == null)
			{
				return null;
			}

			return new {
				id = appealChangeHistory.id,
				appealId = appealChangeHistory.appealId,
				versionNumber = appealChangeHistory.versionNumber,
				timeStamp = appealChangeHistory.timeStamp,
				userId = appealChangeHistory.userId,
				data = appealChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a AppealChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(AppealChangeHistory appealChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (appealChangeHistory == null)
			{
				return null;
			}

			return new {
				id = appealChangeHistory.id,
				appealId = appealChangeHistory.appealId,
				versionNumber = appealChangeHistory.versionNumber,
				timeStamp = appealChangeHistory.timeStamp,
				userId = appealChangeHistory.userId,
				data = appealChangeHistory.data,
				appeal = Appeal.CreateMinimalAnonymous(appealChangeHistory.appeal)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a AppealChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(AppealChangeHistory appealChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (appealChangeHistory == null)
			{
				return null;
			}

			return new {
				id = appealChangeHistory.id,
				name = appealChangeHistory.id,
				description = appealChangeHistory.id
			 };
		}
	}
}
