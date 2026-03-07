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
	public partial class VolunteerGroupMemberChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)volunteerGroupMemberId; }
			set { volunteerGroupMemberId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class VolunteerGroupMemberChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 volunteerGroupMemberId { get; set; }
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
		public class VolunteerGroupMemberChangeHistoryOutputDTO : VolunteerGroupMemberChangeHistoryDTO
		{
			public VolunteerGroupMember.VolunteerGroupMemberDTO volunteerGroupMember { get; set; }
		}


		/// <summary>
		///
		/// Converts a VolunteerGroupMemberChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public VolunteerGroupMemberChangeHistoryDTO ToDTO()
		{
			return new VolunteerGroupMemberChangeHistoryDTO
			{
				id = this.id,
				volunteerGroupMemberId = this.volunteerGroupMemberId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a VolunteerGroupMemberChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<VolunteerGroupMemberChangeHistoryDTO> ToDTOList(List<VolunteerGroupMemberChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<VolunteerGroupMemberChangeHistoryDTO> output = new List<VolunteerGroupMemberChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (VolunteerGroupMemberChangeHistory volunteerGroupMemberChangeHistory in data)
			{
				output.Add(volunteerGroupMemberChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a VolunteerGroupMemberChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the VolunteerGroupMemberChangeHistoryEntity type directly.
		///
		/// </summary>
		public VolunteerGroupMemberChangeHistoryOutputDTO ToOutputDTO()
		{
			return new VolunteerGroupMemberChangeHistoryOutputDTO
			{
				id = this.id,
				volunteerGroupMemberId = this.volunteerGroupMemberId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				volunteerGroupMember = this.volunteerGroupMember?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a VolunteerGroupMemberChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of VolunteerGroupMemberChangeHistory objects to avoid using the VolunteerGroupMemberChangeHistory entity type directly.
		///
		/// </summary>
		public static List<VolunteerGroupMemberChangeHistoryOutputDTO> ToOutputDTOList(List<VolunteerGroupMemberChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<VolunteerGroupMemberChangeHistoryOutputDTO> output = new List<VolunteerGroupMemberChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (VolunteerGroupMemberChangeHistory volunteerGroupMemberChangeHistory in data)
			{
				output.Add(volunteerGroupMemberChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a VolunteerGroupMemberChangeHistory Object.
		///
		/// </summary>
		public static Database.VolunteerGroupMemberChangeHistory FromDTO(VolunteerGroupMemberChangeHistoryDTO dto)
		{
			return new Database.VolunteerGroupMemberChangeHistory
			{
				id = dto.id,
				volunteerGroupMemberId = dto.volunteerGroupMemberId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a VolunteerGroupMemberChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(VolunteerGroupMemberChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.volunteerGroupMemberId = dto.volunteerGroupMemberId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a VolunteerGroupMemberChangeHistory Object.
		///
		/// </summary>
		public VolunteerGroupMemberChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new VolunteerGroupMemberChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				volunteerGroupMemberId = this.volunteerGroupMemberId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a VolunteerGroupMemberChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a VolunteerGroupMemberChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a VolunteerGroupMemberChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a VolunteerGroupMemberChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.VolunteerGroupMemberChangeHistory volunteerGroupMemberChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (volunteerGroupMemberChangeHistory == null)
			{
				return null;
			}

			return new {
				id = volunteerGroupMemberChangeHistory.id,
				volunteerGroupMemberId = volunteerGroupMemberChangeHistory.volunteerGroupMemberId,
				versionNumber = volunteerGroupMemberChangeHistory.versionNumber,
				timeStamp = volunteerGroupMemberChangeHistory.timeStamp,
				userId = volunteerGroupMemberChangeHistory.userId,
				data = volunteerGroupMemberChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a VolunteerGroupMemberChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(VolunteerGroupMemberChangeHistory volunteerGroupMemberChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (volunteerGroupMemberChangeHistory == null)
			{
				return null;
			}

			return new {
				id = volunteerGroupMemberChangeHistory.id,
				volunteerGroupMemberId = volunteerGroupMemberChangeHistory.volunteerGroupMemberId,
				versionNumber = volunteerGroupMemberChangeHistory.versionNumber,
				timeStamp = volunteerGroupMemberChangeHistory.timeStamp,
				userId = volunteerGroupMemberChangeHistory.userId,
				data = volunteerGroupMemberChangeHistory.data,
				volunteerGroupMember = VolunteerGroupMember.CreateMinimalAnonymous(volunteerGroupMemberChangeHistory.volunteerGroupMember),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a VolunteerGroupMemberChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(VolunteerGroupMemberChangeHistory volunteerGroupMemberChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (volunteerGroupMemberChangeHistory == null)
			{
				return null;
			}

			return new {
				id = volunteerGroupMemberChangeHistory.id,
				name = volunteerGroupMemberChangeHistory.id,
				description = volunteerGroupMemberChangeHistory.id
			 };
		}
	}
}
