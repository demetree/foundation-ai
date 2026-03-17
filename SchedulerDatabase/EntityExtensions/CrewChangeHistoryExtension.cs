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
	public partial class CrewChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)crewId; }
			set { crewId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class CrewChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 crewId { get; set; }
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
		public class CrewChangeHistoryOutputDTO : CrewChangeHistoryDTO
		{
			public Crew.CrewDTO crew { get; set; }
		}


		/// <summary>
		///
		/// Converts a CrewChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public CrewChangeHistoryDTO ToDTO()
		{
			return new CrewChangeHistoryDTO
			{
				id = this.id,
				crewId = this.crewId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a CrewChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<CrewChangeHistoryDTO> ToDTOList(List<CrewChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<CrewChangeHistoryDTO> output = new List<CrewChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (CrewChangeHistory crewChangeHistory in data)
			{
				output.Add(crewChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a CrewChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the CrewChangeHistory Entity type directly.
		///
		/// </summary>
		public CrewChangeHistoryOutputDTO ToOutputDTO()
		{
			return new CrewChangeHistoryOutputDTO
			{
				id = this.id,
				crewId = this.crewId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				crew = this.crew?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a CrewChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of CrewChangeHistory objects to avoid using the CrewChangeHistory entity type directly.
		///
		/// </summary>
		public static List<CrewChangeHistoryOutputDTO> ToOutputDTOList(List<CrewChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<CrewChangeHistoryOutputDTO> output = new List<CrewChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (CrewChangeHistory crewChangeHistory in data)
			{
				output.Add(crewChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a CrewChangeHistory Object.
		///
		/// </summary>
		public static Database.CrewChangeHistory FromDTO(CrewChangeHistoryDTO dto)
		{
			return new Database.CrewChangeHistory
			{
				id = dto.id,
				crewId = dto.crewId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a CrewChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(CrewChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.crewId = dto.crewId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a CrewChangeHistory Object.
		///
		/// </summary>
		public CrewChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new CrewChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				crewId = this.crewId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a CrewChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a CrewChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a CrewChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a CrewChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.CrewChangeHistory crewChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (crewChangeHistory == null)
			{
				return null;
			}

			return new {
				id = crewChangeHistory.id,
				crewId = crewChangeHistory.crewId,
				versionNumber = crewChangeHistory.versionNumber,
				timeStamp = crewChangeHistory.timeStamp,
				userId = crewChangeHistory.userId,
				data = crewChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a CrewChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(CrewChangeHistory crewChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (crewChangeHistory == null)
			{
				return null;
			}

			return new {
				id = crewChangeHistory.id,
				crewId = crewChangeHistory.crewId,
				versionNumber = crewChangeHistory.versionNumber,
				timeStamp = crewChangeHistory.timeStamp,
				userId = crewChangeHistory.userId,
				data = crewChangeHistory.data,
				crew = Crew.CreateMinimalAnonymous(crewChangeHistory.crew),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a CrewChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(CrewChangeHistory crewChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (crewChangeHistory == null)
			{
				return null;
			}

			return new {
				id = crewChangeHistory.id,
				name = crewChangeHistory.id,
				description = crewChangeHistory.id
			 };
		}
	}
}
