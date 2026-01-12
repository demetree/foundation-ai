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
	public partial class CrewMemberChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)crewMemberId; }
			set { crewMemberId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class CrewMemberChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 crewMemberId { get; set; }
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
		public class CrewMemberChangeHistoryOutputDTO : CrewMemberChangeHistoryDTO
		{
			public CrewMember.CrewMemberDTO crewMember { get; set; }
		}


		/// <summary>
		///
		/// Converts a CrewMemberChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public CrewMemberChangeHistoryDTO ToDTO()
		{
			return new CrewMemberChangeHistoryDTO
			{
				id = this.id,
				crewMemberId = this.crewMemberId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a CrewMemberChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<CrewMemberChangeHistoryDTO> ToDTOList(List<CrewMemberChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<CrewMemberChangeHistoryDTO> output = new List<CrewMemberChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (CrewMemberChangeHistory crewMemberChangeHistory in data)
			{
				output.Add(crewMemberChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a CrewMemberChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the CrewMemberChangeHistoryEntity type directly.
		///
		/// </summary>
		public CrewMemberChangeHistoryOutputDTO ToOutputDTO()
		{
			return new CrewMemberChangeHistoryOutputDTO
			{
				id = this.id,
				crewMemberId = this.crewMemberId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				crewMember = this.crewMember?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a CrewMemberChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of CrewMemberChangeHistory objects to avoid using the CrewMemberChangeHistory entity type directly.
		///
		/// </summary>
		public static List<CrewMemberChangeHistoryOutputDTO> ToOutputDTOList(List<CrewMemberChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<CrewMemberChangeHistoryOutputDTO> output = new List<CrewMemberChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (CrewMemberChangeHistory crewMemberChangeHistory in data)
			{
				output.Add(crewMemberChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a CrewMemberChangeHistory Object.
		///
		/// </summary>
		public static Database.CrewMemberChangeHistory FromDTO(CrewMemberChangeHistoryDTO dto)
		{
			return new Database.CrewMemberChangeHistory
			{
				id = dto.id,
				crewMemberId = dto.crewMemberId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a CrewMemberChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(CrewMemberChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.crewMemberId = dto.crewMemberId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a CrewMemberChangeHistory Object.
		///
		/// </summary>
		public CrewMemberChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new CrewMemberChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				crewMemberId = this.crewMemberId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a CrewMemberChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a CrewMemberChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a CrewMemberChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a CrewMemberChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.CrewMemberChangeHistory crewMemberChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (crewMemberChangeHistory == null)
			{
				return null;
			}

			return new {
				id = crewMemberChangeHistory.id,
				crewMemberId = crewMemberChangeHistory.crewMemberId,
				versionNumber = crewMemberChangeHistory.versionNumber,
				timeStamp = crewMemberChangeHistory.timeStamp,
				userId = crewMemberChangeHistory.userId,
				data = crewMemberChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a CrewMemberChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(CrewMemberChangeHistory crewMemberChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (crewMemberChangeHistory == null)
			{
				return null;
			}

			return new {
				id = crewMemberChangeHistory.id,
				crewMemberId = crewMemberChangeHistory.crewMemberId,
				versionNumber = crewMemberChangeHistory.versionNumber,
				timeStamp = crewMemberChangeHistory.timeStamp,
				userId = crewMemberChangeHistory.userId,
				data = crewMemberChangeHistory.data,
				crewMember = CrewMember.CreateMinimalAnonymous(crewMemberChangeHistory.crewMember)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a CrewMemberChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(CrewMemberChangeHistory crewMemberChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (crewMemberChangeHistory == null)
			{
				return null;
			}

			return new {
				id = crewMemberChangeHistory.id,
				name = crewMemberChangeHistory.id,
				description = crewMemberChangeHistory.id
			 };
		}
	}
}
